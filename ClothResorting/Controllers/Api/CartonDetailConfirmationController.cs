using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using ClothResorting.Helpers;

namespace ClothResorting.Controllers.Api
{
    public class CartonDetailConfirmationController : ApiController
    {
        private ApplicationDbContext _context;

        public CartonDetailConfirmationController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/cartondetailconfirmation
        //将所有被选中的对象(id数组)视为正常正确收货，即实际收货数量及库存数量直接等于应收货数量
        [HttpPost]
        public IHttpActionResult SetSelectedObjNormalReceived([FromBody]int[] arr)
        {
            foreach(var id in arr)
            {
                var cartonDetailInDb = _context.CartonDetails
                    .Include(s => s.CartonBreakdowns)
                    .SingleOrDefault(s => s.Id == id);

                cartonDetailInDb.ActualReceived = cartonDetailInDb.SumOfCarton;
                cartonDetailInDb.Available = cartonDetailInDb.SumOfCarton;
                cartonDetailInDb.ActualReceivedPcs = cartonDetailInDb.TotalPcs;
                cartonDetailInDb.AvailablePcs = cartonDetailInDb.TotalPcs;
                cartonDetailInDb.ReceivedDate = DateTime.Today;

                foreach(var breakdown in cartonDetailInDb.CartonBreakdowns)
                {
                    breakdown.ActualPcs = breakdown.ForecastPcs;
                    breakdown.AvailablePcs = breakdown.ForecastPcs;
                    breakdown.ReceivedDate = DateTime.Today;
                }
            }

            _context.SaveChanges();

            //返回第一个样本
            var idFirst = arr[0];
            var cartonDetailSample = _context.CartonDetails
                .Include(s => s.PackingList.PreReceiveOrder)
                .SingleOrDefault(s => s.Id == idFirst);
            var cartonDetail = Mapper.Map<CartonDetail, SilkIconCartonDetailDto>(cartonDetailSample);

            //同步purchase order中ctn和pcs的数量
            var sync = new DbSynchronizer();
            sync.SyncPurchaseOrder(cartonDetailSample);
            _context.SaveChanges();

            //同步pre-receive order中ctn和pcs的数量
            sync.SyncPreReceivedOrder(cartonDetailSample);
            _context.SaveChanges();

            return Created(new Uri(Request.RequestUri + "/" + arr[0]), cartonDetail);
        }
    }
}
