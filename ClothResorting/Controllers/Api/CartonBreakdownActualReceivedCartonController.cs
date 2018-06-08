using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using ClothResorting.Helpers;
using AutoMapper;
using ClothResorting.Dtos;

namespace ClothResorting.Controllers.Api
{
    public class CartonBreakdownActualReceivedCartonController : ApiController
    {
        private ApplicationDbContext _context;

        public CartonBreakdownActualReceivedCartonController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/CartonBreakdownActualReceivedCarton 读取手动输入，更新该cartonId的实际到货箱数
        [HttpPost]
        public IHttpActionResult UpdateCartonQuantity([FromBody]int[] arr)
        {
            var id = arr[0];
            var value = arr[1];

            var cartonInDb = _context.SilkIconCartonDetails
                .Include(s => s.SilkIconPackingList.SilkIconPreReceiveOrder)
                .SingleOrDefault(s => s.Id == id);

            cartonInDb.ActualReceived += value;
            cartonInDb.Available += value;

            _context.SaveChanges();

            //同步po和pre-received Order
            var sync = new DbSynchronizer();

            sync.SyncPurchaseOrder(cartonInDb);
            _context.SaveChanges();

            sync.SyncPreReceivedOrder(cartonInDb);
            _context.SaveChanges();

            var cartonInDbDto = Mapper.Map<SilkIconCartonDetail, SilkIconCartonDetailDto>(cartonInDb);

            return Created(new Uri(Request.RequestUri + "/" + arr[0]), cartonInDbDto);
        }
    }
}
