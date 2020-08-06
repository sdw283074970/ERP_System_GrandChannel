using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using ClothResorting.Models.FBAModels.StaticModels;
using ClothResorting.Models.StaticClass;

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBADashboardController : ApiController
    {
        private ApplicationDbContext _context;

        public FBADashboardController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /fba/fbadashboard/?operation={operation}
        [HttpGet]
        public IHttpActionResult GetDashBordData([FromUri]string operation)
        {
            if (operation == "GetTransitReminder")
            {
                var result = GetTransitReminder();
                return Ok(result);
            }

            return Ok("No operation applied.");
        }

        public IList<UnhandledTransit> GetTransitReminder()
        {
            var result = new List<UnhandledTransit>();

            var transitInDb = _context.FBAMasterOrders
                .Include(x => x.FBACartonLocations)
                .Include(x => x.FBAPalletLocations)
                //.Where(x => x.StorageType == FBAStorageType.TransitShipment && x.FBACartonLocations.Any() && x.FBACartonLocations.Sum(c => c.AvailableCtns) != 0);
                .Where(x => x.StorageType == FBAStorageType.TransitShipment && ((x.FBAPalletLocations.Any() && x.FBAPalletLocations.Sum(c => c.AvailablePlts) != 0) || (x.FBACartonLocations.Any() && x.FBACartonLocations.Sum(c => c.AvailableCtns) != 0)));

            foreach (var t in transitInDb)
            {
                result.Add(new UnhandledTransit
                {
                    StorageType = t.StorageType,
                    Id = t.Id,
                    Container = t.Container,
                    Status = t.Status,
                    InboundDate = t.InboundDate,
                    CustomerCode = t.CustomerCode,
                    AvaCtns = t.FBACartonLocations.Sum(x => x.AvailableCtns),
                    AvaPlts = t.FBAPalletLocations.Sum(x => x.AvailablePlts)
                });
            }

            return result;
        }
    }

    public class UnhandledTransit
    {
        public int Id { get; set; }

        public string  Container { get; set; }

        public string Status { get; set; }

        public string CustomerCode { get; set; }

        public string StorageType { get; set; }

        public DateTime InboundDate { get; set; }

        public int AvaCtns { get; set; }

        public int AvaPlts { get; set; }
    }
}
