using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Models;
using ClothResorting.Models.ApiTransformModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ClothResorting.Controllers.Api
{
    public class BulkPOSummaryController : ApiController
    {
        private ApplicationDbContext _context;

        public BulkPOSummaryController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/bulkposummary/
        [HttpPost]
        public IHttpActionResult CreatBulkPOSummary([FromBody]BulkPOSummaryJsonObj obj)
        {
            var preReceiveOrderInDb = _context.PreReceiveOrders.Find(obj.PreId);

            var newBulkPO = new POSummary {
                Container = "Unknown",
                PurchaseOrder = obj.PurchaseOrder,
                Style = obj.Style,
                OrderType = obj.OrderType,
                PoLine = obj.POLine,
                Customer = obj.Customer,
                Quantity = 0,
                ActualPcs = 0,
                Cartons = 0,
                ActualCtns = 0,
                PreReceiveOrder = preReceiveOrderInDb
            };

            _context.POSummaries.Add(newBulkPO);
            _context.SaveChanges();

            var sampleDto = Mapper.Map<POSummary, POSummaryDto>(_context.POSummaries.OrderByDescending(x => x.Id).First());

            return Created(Request.RequestUri + "/" + sampleDto.Id, sampleDto);
        }
    }
}
