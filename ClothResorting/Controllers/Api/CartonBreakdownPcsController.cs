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
    //每一个cartonBreakDown收取的数量
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
            if (!ModelState.IsValid)
                return BadRequest();

            var id = arr[0];
            var changeValue = arr[1];

            var cartonInDb = _context.CartonBreakDowns
                .Include(s => s.SilkIconPackingList.SilkIconPreReceiveOrder)
                .SingleOrDefault(s => s.Id == id);

            cartonInDb.ActualPcs += changeValue;
            cartonInDb.AvailablePcs += changeValue;

            _context.SaveChanges();

            //每更新一次CartonBreakdown的pcs收取数量，同步一次该po的pcs收货总量及库存数量
            var pl = cartonInDb.SilkIconPackingList;

            pl.ActualReceivedPcs = _context.CartonBreakDowns
                .Where(s => s.PurchaseNumber == pl.PurchaseOrderNumber)
                .Sum(s => s.ActualPcs);

            pl.AvailablePcs = _context.CartonBreakDowns
                .Where(s => s.PurchaseNumber == pl.PurchaseOrderNumber)
                .Sum(s => s.AvailablePcs);

            _context.SaveChanges();

            //每更新一次CartonBreakdown的pcs收取数量，同步一次该pre-receive Order的pcs收货总量及库存数量
            var preId = cartonInDb.SilkIconPackingList.SilkIconPreReceiveOrder.Id;

            var preReceiveOrder = _context.SilkIconPreReceiveOrders
                .Include(s => s.SilkIconPackingLists)
                .SingleOrDefault(s => s.Id == preId);

            preReceiveOrder.ActualReceivedPcs = preReceiveOrder
                .SilkIconPackingLists.Sum(s => s.ActualReceivedPcs);

            preReceiveOrder.AvailablePcs = preReceiveOrder
                .SilkIconPackingLists.Sum(s => s.AvailablePcs);

            _context.SaveChanges();

            var carton = Mapper.Map<CartonBreakDown, CartonBreakDownDto>(cartonInDb);

            return Ok(carton);
        }
    }
}
