using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Models;
using ClothResorting.Models.ApiTransformModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace ClothResorting.Controllers.Api
{
    public class BulkPOSummaryController : ApiController
    {
        private ApplicationDbContext _context;
        private string _userName;

        public BulkPOSummaryController()
        {
            _context = new ApplicationDbContext();
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0] == "" ? (HttpContext.Current.Request.Headers.Get("AppUser") == null ? "" : HttpContext.Current.Request.Headers.Get("AppUser")) : HttpContext.Current.User.Identity.Name.Split('@')[0];
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
                Operator = _userName,
                Batch = (preReceiveOrderInDb.LastBatch + 1).ToString(),
                PreReceiveOrder = preReceiveOrderInDb,
                Vendor = obj.Vendor
            };

            preReceiveOrderInDb.LastBatch += 1;

            _context.POSummaries.Add(newBulkPO);
            _context.SaveChanges();

            var sampleDto = Mapper.Map<POSummary, POSummaryDto>(_context.POSummaries.OrderByDescending(x => x.Id).First());

            return Created(Request.RequestUri + "/" + sampleDto.Id, sampleDto);
        }
    }
}
