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
    public class InboundHistoryController : ApiController
    {
        private ApplicationDbContext _context;

        public InboundHistoryController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/InboundHistory
        [HttpPost]
        public IHttpActionResult ReturnInboundHistory([FromBody]BasicFourAttrsJsonObj obj)
        {
            var inboundHistoryList = new List<InboundHistoryRecord>();

            var inboundRecords = _context.LocationDetails
                .Where(c => c.PurchaseOrder == obj.PurchaseOrder
                    && c.Style == obj.Style
                    && c.Color == obj.Color
                    && c.Size == obj.Size)
                .ToList();

            foreach(var record in inboundRecords)
            {
                inboundHistoryList.Add(new InboundHistoryRecord {
                    InboundDate = record.InboundDate,
                    Location = record.Location,
                    InboundPcs = "+" + record.OrgPcs.ToString(),
                    ResidualPcs = record.InvPcs
                });
            }

            return Created(Request.RequestUri
                + "/" + obj.PurchaseOrder
                + "&" + obj.Style
                + "&" + obj.Color
                + "&" + obj.Size, inboundHistoryList);
        }
    }
}
