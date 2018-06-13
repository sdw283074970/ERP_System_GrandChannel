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
    //即将删除
    public class CartonDetailCartonsController : ApiController
    {
        private ApplicationDbContext _context;

        public CartonDetailCartonsController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/cartondetailcartons 每一个carton编号范围收取的carton数量
        [HttpPost]
        public IHttpActionResult UpdateReceivedCartons([FromBody]int[] arr)
        {
            var id = arr[0];
            var changeValue = arr[1];

            var cartonDetailInDb = _context.CartonDetails
                .Include(c => c.PackingList.PreReceiveOrder)
                .SingleOrDefault(s => s.Id == id);

            cartonDetailInDb.ActualReceived += changeValue;
            cartonDetailInDb.Available += changeValue;
            _context.SaveChanges();

            //每更新一次carton编号范围内收取数量，同步一次该po的收货总量及库存数量
            var pl = cartonDetailInDb.PackingList;
            var po = pl.PurchaseOrder;
            var preReceivedId = pl.PreReceiveOrder.Id;

                //查询preId为当前packinglist且po为当前po的cartondetail对象
            pl.ActualReceived = _context.CartonDetails
                .Where(s => s.PackingList.PreReceiveOrder.Id == preReceivedId
                    && s.PackingList.PurchaseOrder == po)
                .Sum(s => s.ActualReceived);

            pl.Available = _context.CartonDetails
                .Where(s => s.PackingList.PreReceiveOrder.Id == preReceivedId
                    && s.PackingList.PurchaseOrder == po)
                .Sum(s => s.Available);

            _context.SaveChanges();

            //每更新一次carton编号范围内收取数量，同步一次该Pre-receive Order的收货总量及库存数量
            var preReceive = pl.PreReceiveOrder;

            preReceive.ActualReceived = _context.PackingLists
                .Include(s => s.PreReceiveOrder)
                .Where(s => s.PreReceiveOrder.Id == preReceivedId)
                .Sum(s => s.ActualReceived);

            preReceive.Available = _context.PackingLists
                .Include(s => s.PreReceiveOrder)
                .Where(s => s.PreReceiveOrder.Id == preReceivedId)
                .Sum(s => s.Available);

            _context.SaveChanges();

            var cartonDetail = Mapper.Map<CartonDetail, SilkIconCartonDetailDto>(cartonDetailInDb);

            return Ok(cartonDetail);
        }
    }
}