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
    public class AdjustedHistoryController : ApiController
    {
        private ApplicationDbContext _context;

        public AdjustedHistoryController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/adjustedHistory
        [HttpPost]
        public IHttpActionResult ReturnAdjustedHistory([FromBody]BasicFourAttrsJsonObj obj)
        {
            var adjustedRecordList = new List<AdjustedRecord>();

            var adjustmentRecords = _context.AdjustmentRecords
                .Where(c => c.PurchaseOrder == obj.PurchaseOrder
                    && c.Style == obj.Style
                    && c.Color == obj.Color
                    && c.Size == obj.Size)
                .ToList();

            foreach(var record in adjustmentRecords)
            {
                adjustedRecordList.Add(new AdjustedRecord {
                    AdjustedDate = record.AdjustDate,
                    AdjustedPcs = record.Adjustment,
                    Balance = record.Balance,
                    Memo = record.Memo
                });
            }

            return Created(Request.RequestUri
                + "/" + obj.PurchaseOrder
                + "&" + obj.Style
                + "&" + obj.Color
                + "&" + obj.Size, adjustedRecordList);
        }
    }
}
