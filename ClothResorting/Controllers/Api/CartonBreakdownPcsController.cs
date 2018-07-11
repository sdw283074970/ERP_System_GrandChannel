using ClothResorting.Models;
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
    //每一个cartonBreakDown收取的pcs数量
    public class CartonBreakdownPcsController : ApiController
    {
        private ApplicationDbContext _context;

        public CartonBreakdownPcsController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/CartonReceiving/输入cartonBreakDown的Id和输入框数字，将结果写入数据库
        [HttpPost]
        public IHttpActionResult UpdateReceiving([FromBody]int[] arr)
        {
            var id = arr[0];
            var changeValue = arr[1];

            if (!ModelState.IsValid)
                return BadRequest();
            
            var cartonInDb = _context.CartonBreakDowns
                .Include(s => s.PurchaseOrderSummary.PreReceiveOrder)
                .Include(s => s.PurchaseOrderSummary.CartonDetails)
                .SingleOrDefault(s => s.Id == id);

            cartonInDb.ReceivedDate = DateTime.Now;
            cartonInDb.ActualPcs += changeValue;
            cartonInDb.AvailablePcs += changeValue;

            _context.SaveChanges();

            //每更新一次CartonBreakdown的pcs收取数量，同步一次该po的pcs收货总量及库存数量
            var pl = cartonInDb.PurchaseOrderSummary;
            var po = pl.PurchaseOrder;
            var preId = pl.PreReceiveOrder.Id;

                //查询preId为当前packinglist且po为当前po的breakdown对象
            pl.ActualReceivedPcs = _context.CartonBreakDowns
                .Include(c => c.PurchaseOrderSummary.PreReceiveOrder)
                .Where(s => s.PurchaseOrderSummary.PreReceiveOrder.Id == preId
                    && s.PurchaseOrderSummary.PurchaseOrder == po)
                .Sum(s => s.ActualPcs);

            pl.AvailablePcs = _context.CartonBreakDowns
                .Include(c => c.PurchaseOrderSummary.PreReceiveOrder)
                .Where(s => s.PurchaseOrderSummary.PreReceiveOrder.Id == preId
                    && s.PurchaseOrderSummary.PurchaseOrder == po)
                .Sum(s => s.AvailablePcs);

            _context.SaveChanges();

            //每更新一次CartonBreakdown的pcs收取数量，同步一次该pre-receive Order的pcs收货总量及库存数量
            var preReceiveOrder = _context.PreReceiveOrders
                .Include(s => s.PurchaseOrderSummary)
                .SingleOrDefault(s => s.Id == preId);

            preReceiveOrder.ActualReceivedPcs = preReceiveOrder
                .PurchaseOrderSummary.Sum(s => s.ActualReceivedPcs);

            _context.SaveChanges();

            //每更新一次CartonBreakdown的pcs收取数量，同步一次与此Breakdown对应的Carton Details中的pcs收货总量及库存数量

            // To Do: 此处需要优化

            foreach (var cartonDeatil in pl.CartonDetails)
            {
                var cartonBreakDown = _context.CartonBreakDowns
                    .Where(c => c.CartonNumberRangeTo == cartonDeatil.CartonNumberRangeTo);

                cartonDeatil.ActualReceivedPcs = cartonBreakDown.Sum(c => c.ActualPcs);

                cartonDeatil.AvailablePcs = cartonBreakDown.Sum(c => c.AvailablePcs);
            }

            _context.SaveChanges();

            var carton = Mapper.Map<CartonBreakDown, CartonBreakDownDto>(cartonInDb);

            return Ok(carton);
        }
    }
}
