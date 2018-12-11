using ClothResorting.Models;
using ClothResorting.Models.ApiTransformModels;
using ClothResorting.Models.DataTransferModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;

namespace ClothResorting.Controllers.Api
{
    public class InboundHistoryController : ApiController
    {
        private ApplicationDbContext _context;

        public InboundHistoryController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/InboundHistory/
        [HttpGet]
        public IHttpActionResult ReturnInboundHistory([FromBody]BasicFourAttrsJsonObj obj)
        {
            var inboundHistoryList = new List<InboundHistoryRecord>();

            var inboundRecords = _context.ReplenishmentLocationDetails
                .Include(x => x.GeneralLocationSummary)
                .Where(c => c.PurchaseOrder == obj.PurchaseOrder
                    && c.Style == obj.Style
                    && c.Color == obj.Color
                    && c.Size == obj.Size)
                .ToList();

            foreach(var record in inboundRecords)
            {
                if (record.GeneralLocationSummary != null)
                {
                    inboundHistoryList.Add(new InboundHistoryRecord
                    {
                        FileName = record.GeneralLocationSummary.UploadedFileName,
                        Location = record.Location,
                        InboundPcs = "+" + record.Quantity.ToString(),
                        ResidualPcs = record.AvailablePcs
                    });
                }
            }

            return Ok(inboundHistoryList);
        }
    }
}
