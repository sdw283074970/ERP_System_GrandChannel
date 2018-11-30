using ClothResorting.Models;
using ClothResorting.Models.ApiTransformModels;
using ClothResorting.Models.DataTransferModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ClothResorting.Controllers.Api
{
    public class OutboundHistoryController : ApiController
    {
        private ApplicationDbContext _context;

        public OutboundHistoryController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/outboundhistory
        [HttpPost]
        public IHttpActionResult ReturnOutboundHistoryRecords([FromBody]BasicFourAttrsJsonObj obj)
        {
            //var outboundHistoryList = new List<OutboundHistoryRecord>();

            //var records = _context.PermanentLocIORecord
            //    .Where(c => c.PurchaseOrder == obj.PurchaseOrder
            //        && c.Style == obj.Style
            //        && c.Color == obj.Color
            //        && c.Size == obj.Size
            //        && c.FromLocation == "")
            //    .ToList();

            //foreach (var record in records)
            //{
            //    outboundHistoryList.Add(new OutboundHistoryRecord
            //    {
            //        OutboundDate = record.OperationDate,
            //        FromLocation = record.PermanentLoc,
            //        OutboundPcs = "-" + -record.InvChange,
            //        OrderPurchaseOrder = record.OrderPurchaseOrder
            //    });
            //}

            //return Created(Request.RequestUri
            //    + "/" + obj.PurchaseOrder
            //    + "&" + obj.Style
            //    + "&" + obj.Color
            //    + "&" + obj.Size, outboundHistoryList);

            return Ok();
        }
    }
}
