using ClothResorting.Models;
using ClothResorting.Models.ApiTransformModels;
using ClothResorting.Models.DataTransferModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

//namespace ClothResorting.Controllers.Api
//{
//    public class RelocationHistoryController : ApiController
//    {
//        private ApplicationDbContext _context;

//        public RelocationHistoryController()
//        {
//            _context = new ApplicationDbContext();
//        }

//        // POST /api/outboundhistory
//        [HttpPost]
//        public IHttpActionResult ReturnRelocationHistoryRecords([FromBody]BasicFourAttrsJsonObj obj)
//        {
//            var relocationHistoryList = new List<RelocationHistoryRecord>();

//            var records = _context.PermanentLocIORecord
//                .Where(c => c.PurchaseOrder == obj.PurchaseOrder
//                    && c.Style == obj.Style
//                    && c.Color == obj.Color
//                    && c.Size == obj.Size
//                    && c.FromLocation != ""
//                    && c.FromLocation != "Shortage")
//                .ToList();

//            foreach (var record in records)
//            {
//                relocationHistoryList.Add(new RelocationHistoryRecord
//                {
//                    RelocatedDate = record.OperationDate,
//                    FromLocation = record.FromLocation,
//                    ToLocation = record.PermanentLoc,
//                    RelocatedPcs = record.InvChange.ToString()
//                });
//            }

//            return Created(Request.RequestUri
//                + "/" + obj.PurchaseOrder
//                + "&" + obj.Style
//                + "&" + obj.Color
//                + "&" + obj.Size, relocationHistoryList);
//        }
//    }
//}
