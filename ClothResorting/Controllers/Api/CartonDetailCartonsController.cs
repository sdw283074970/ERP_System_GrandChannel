using ClothResorting.Models;
using ClothResorting.Models.ApiTransformModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using AutoMapper;
using ClothResorting.Dtos;

namespace ClothResorting.Controllers.Api
{
    public class CartonDetailCartonsController : ApiController
    {
        private ApplicationDbContext _context;

        public CartonDetailCartonsController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/receivedcartons 每一个carton编号范围收取的carton数量
        [HttpPost]
        public IHttpActionResult UpdateReceivedCartons([FromBody]int[] arr)
        {
            var id = arr[0];
            var changeValue = arr[1];
            var cartonDetailInDb = _context.SilkIconCartonDetails
                .Include(c => c.SilkIconPackingList.SilkIconPreReceiveOrder)
                .SingleOrDefault(s => s.Id == id);

            cartonDetailInDb.ActualReceived += changeValue;
            cartonDetailInDb.Available += changeValue;
            _context.SaveChanges();

            //每更新一次carton编号范围内收取数量，同步一次该po的收货总量及库存数量
            var pl = cartonDetailInDb.SilkIconPackingList;
            var preReceivedId = pl.SilkIconPreReceiveOrder.Id;

            pl.ActualReceived = _context.SilkIconCartonDetails
                .Where(s => s.SilkIconPackingList.SilkIconPreReceiveOrder.Id == preReceivedId)
                .Sum(s => s.ActualReceived);

            pl.Available = _context.SilkIconCartonDetails
                .Where(s => s.SilkIconPackingList.SilkIconPreReceiveOrder.Id == preReceivedId)
                .Sum(s => s.Available);

            _context.SaveChanges();

            //每更新一次carton编号范围内收取数量，同步一次该Pre-receive Order的收货总量及库存数量
            var preReceive = pl.SilkIconPreReceiveOrder;

            preReceive.ActualReceived = _context.SilkIconPackingLists
                .Include(s => s.SilkIconPreReceiveOrder)
                .Where(s => s.SilkIconPreReceiveOrder.Id == preReceivedId)
                .Sum(s => s.ActualReceived);

            preReceive.Available = _context.SilkIconPackingLists
                .Include(s => s.SilkIconPreReceiveOrder)
                .Where(s => s.SilkIconPreReceiveOrder.Id == preReceivedId)
                .Sum(s => s.Available);

            _context.SaveChanges();

            var cartonDetail = Mapper.Map<SilkIconCartonDetail, SilkIconCartonDetailDto>(cartonDetailInDb);

            return Ok(cartonDetail);
        }
    }
}