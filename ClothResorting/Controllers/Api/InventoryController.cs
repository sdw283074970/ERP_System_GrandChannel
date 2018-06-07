using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Data.Entity;
using System.Web.Http;
using ClothResorting.Models.ApiTransformModels;

namespace ClothResorting.Controllers.Api
{
    public class InventoryController : ApiController
    {
        private ApplicationDbContext _context;

        public InventoryController()
        {
            _context = new ApplicationDbContext();
        }

        //GET /api/Inventory/获取所有的PreReceiveOrders
        public IHttpActionResult GetPrereceiveOrder()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var preReceiveOrderLists = _context.SilkIconPreReceiveOrders
                .Select(Mapper.Map<SilkIconPreReceiveOrder, SilkIconPreReceiveOrdersDto>);

            return Ok(preReceiveOrderLists);
        }

        //GET /api/Inventory/id/输入PreReceiveOrder的Id，返回所有的Po细节概览
        public IHttpActionResult GetPurchaseOrderDetail(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var purchaseOrderDetails = _context.SilkIconPackingLists
                .Where(s => s.SilkIconPreReceiveOrder.Id == id)
                .Select(Mapper.Map<SilkIconPackingList, SilkIconPackingListDto>);

            return Ok(purchaseOrderDetails);
        }


        // POST /api/Inventory/输入cartondetail的id，返回该id下的CartonBreakDown的所有细节
        [HttpPost]
        public IHttpActionResult GetCartonDetail([FromBody]int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var cartons = _context.CartonBreakDowns
                .Include(c => c.SilkIconPackingList)
                .Where(c => c.SilkIconCartonDetail.Id == id)
                .Select(Mapper.Map<CartonBreakDown, CartonBreakDownDto>);

            return Created(new Uri(Request.RequestUri + "/" + "cartondetailid=" + id), cartons);
        }
    }
}
