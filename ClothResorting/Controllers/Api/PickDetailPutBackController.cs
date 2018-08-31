using ClothResorting.Models;
using ClothResorting.Models.ApiTransformModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;

namespace ClothResorting.Controllers.Api
{
    public class PickDetailPutBackController : ApiController
    {
        private ApplicationDbContext _context;
        private DateTime _timeNow;

        public PickDetailPutBackController()
        {
            _context = new ApplicationDbContext();
            _timeNow = DateTime.Now;
        }

        // POST /pickdetailputback/
        [HttpPost]
        public IHttpActionResult CreatePutBackLocation([FromBody]PutBackJsonObj obj)
        {

            var locationInDb = _context.FCRegularLocationDetails
            .Include(x => x.PreReceiveOrder)
            .Where(x => x.Container == obj.Container)
            .FirstOrDefault();

            if (locationInDb == null)
                throw new Exception("Cannot find location info of container " + obj.Container);

            var prereceivedOrderInDb = _context.PreReceiveOrders.Find(locationInDb.PreReceiveOrder.Id);

            var putBackCartonLocation = new FCRegularLocationDetail {
                Container = obj.Container,
                Status = "Put Back",
                CartonRange = obj.CartonRange,
                PurchaseOrder = obj.PurchaseOrder,
                Style = obj.Style,
                Color = obj.Color,
                CustomerCode = obj.Customer,
                SizeBundle = obj.SizeBundle,
                PcsBundle = obj.PcsBundle,
                AvailableCtns = obj.Cartons,
                PickingCtns = 0,
                ShippedCtns = -obj.Cartons,
                AvailablePcs = obj.Quantity,
                PickingPcs = 0,
                ShippedPcs = -obj.Quantity,
                InboundDate = _timeNow,
                Location = obj.Location + "(Put Back)",
                PcsPerCaron = obj.PcsPerCarton,
                Cartons = 0,
                Quantity = 0,
                PreReceiveOrder = prereceivedOrderInDb
            };

            return Ok();
        }
    }
}
