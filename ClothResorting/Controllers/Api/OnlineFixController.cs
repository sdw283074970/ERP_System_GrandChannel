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

        // PUT /api/onlinefix/
        [HttpPut]
        public void FixProblems()
        {
            var workOrdersInDb = _context.PreReceiveOrders
                .Include(x => x.UpperVendor)
                .Where(x => x.Id > 0);

            var fc = _context.UpperVendors.Find(2);

            foreach(var w in workOrdersInDb)
            {
                w.UpperVendor = fc;
                w.CustomerName = Vendor.FreeCountry;
            }

            var poSummariesInDb = _context.POSummaries
                .Include(x => x.RegularCartonDetails)
                .Where(x => x.Id > 0);

            foreach (var p in poSummariesInDb)
            {
                p.Vendor = Vendor.FreeCountry;

                if (p.RegularCartonDetails.Count == 1)
                {
                    p.OrderType = OrderType.Prepack;
                }
                else
                {
                    p.OrderType = OrderType.SolidPack;
                }
            }

            _context.SaveChanges();

            var cartonDetailsInDb = _context.RegularCartonDetails
                .Include(x => x.POSummary)
                .Where(x => x.Id > 0);

            foreach (var c in cartonDetailsInDb)
            {
                c.OrderType = c.POSummary.OrderType;
            }

            _context.SaveChanges();
        }
    }
}
