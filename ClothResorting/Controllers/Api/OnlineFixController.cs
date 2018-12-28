using ClothResorting.Models;
using ClothResorting.Models.StaticClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using static org.in2bits.MyOle2.Metadata.Property;

namespace ClothResorting.Controllers.Api
{
    public class OnlineFixController : ApiController
    {
        private ApplicationDbContext _context;

        public OnlineFixController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/onlinefix/
        [HttpPost]
        public IHttpActionResult FixProblems()
        {
            AllocateBatch("HRZU4532001");

            AllocateBatch("MATU5163140");

            return Ok("TEST");
        }

        // DELETE /onlinefix/
        [HttpDelete]
        public void DeleteRepetitiveRecords()
        {
            //var preId = 310;

            //var poSummariesInDb = _context.POSummaries
            //    .Include(x => x.PreReceiveOrder)
            //    .Where(x => x.PreReceiveOrder.Id == preId)
            //    .ToList();

            //var repetitveIdList = new List<int>();

            //for (int i = 0; i < poSummariesInDb.Count() - 1; i++)
            //{
            //    for (int j = i + 1; j < poSummariesInDb.Count() - 1; j++)
            //    {
            //        if (poSummariesInDb[j].Quantity == poSummariesInDb[i].Quantity && poSummariesInDb[j].Style == poSummariesInDb[i].Style && poSummariesInDb[j].PurchaseOrder == poSummariesInDb[i].PurchaseOrder && poSummariesInDb[j].Cartons == poSummariesInDb[i].Cartons)
            //        {
            //            if (!repetitveIdList.Contains(poSummariesInDb[j].Id))
            //            {
            //                repetitveIdList.Add(poSummariesInDb[j].Id);
            //            }
            //        }
            //    }
            //}
        }

        public void AllocateBatch(string contianer)
        {
            var locationInDb = _context.FCRegularLocationDetails.Where(x => x.Container == contianer);

            var index = 1;
            var poList = new List<string>();

            foreach (var location in locationInDb)
            {
                if (!poList.Contains(location.PurchaseOrder))
                {
                    poList.Add(location.PurchaseOrder);
                }
            }

            foreach (var po in poList)
            {
                var locationPoInDb = locationInDb.Where(x => x.PurchaseOrder == po);

                foreach (var location in locationPoInDb)
                {
                    location.Batch = index.ToString();
                }

                index += 1;
            }

            _context.SaveChanges();
        }
    }
}
