using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using ClothResorting.Models.DataTransferModels;

namespace ClothResorting.Controllers.Api
{
    public class PickDetailSummaryController : ApiController
    {
        private ApplicationDbContext _context;

        public PickDetailSummaryController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/pickdetailsummary/{id}(shiporderId)
        public IHttpActionResult GetSummary([FromUri]int id)
        {
            var summaryList = new List<PickDetailSummary>();

            var pickDetailsInDb = _context.PickDetails
                .Include(x => x.ShipOrder)
                .Where(x => x.ShipOrder.Id == id);


            foreach(var pickDetail in pickDetailsInDb)
            {
                if (pickDetail.PickPcs == 0)
                {
                    continue;
                }

                var pickDetailInSummary = summaryList.SingleOrDefault(x => x.Style == pickDetail.Style 
                    && x.Color == pickDetail.Color 
                    && x.SizeBundle == pickDetail.SizeBundle
                    && x.PurchaseOrder == pickDetail.PurchaseOrder);

                if (pickDetailInSummary == null)
                {
                    summaryList.Add(new PickDetailSummary {
                        PurchaseOrder = pickDetail.PurchaseOrder,
                        Style = pickDetail.Style,
                        Color = pickDetail.Color,
                        SizeBundle = pickDetail.SizeBundle,
                        PickCtns = pickDetail.PickCtns,
                        PickPcs = pickDetail.PickPcs
                    });
                }
                else
                {
                    pickDetailInSummary.PickCtns += pickDetail.PickCtns;
                    pickDetailInSummary.PickPcs += pickDetail.PickPcs;
                }
            }

            return Ok(summaryList);
        }
    }
}
