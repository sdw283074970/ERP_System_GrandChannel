using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;

namespace ClothResorting.Controllers.Api
{
    public class ShipDetailHistoryController : ApiController
    {
        private ApplicationDbContext _context;

        public ShipDetailHistoryController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/shipDetailHistory/?vendor={vendor}&container={container}&purchaseOrder={po}&style={style}&color={color}&customer={customer}&size={size}&location={location}
        [HttpGet]
        public IHttpActionResult GetResultBySearchingConditions([FromUri]string vendor, [FromUri]string container, [FromUri]string purchaseOrder, [FromUri]string style, [FromUri]string color, [FromUri]string customer, [FromUri]string size, [FromUri]string location)
        {
            var pickDetailList = _context.PickDetails
                .Include(x => x.ShipOrder)
                .Include(x => x.FCRegularLocationDetail)
                .Include(x => x.ReplenishmentLocationDetail)
                .Where(x => x.ShipOrder.Vendor == vendor)
                .ToList();

            var historyList = new List<PickDetailHistory>();

            if (container != null)
            {
                pickDetailList = pickDetailList.Where(x => x.Container == container).ToList();
            }

            if (purchaseOrder != null)
            {
                pickDetailList = pickDetailList.Where(x => x.PurchaseOrder == purchaseOrder).ToList();
            }

            if (style != null)
            {
                pickDetailList = pickDetailList.Where(x => x.Style == style).ToList();
            }

            if (color != null)
            {
                pickDetailList = pickDetailList.Where(x => x.Color == color).ToList();
            }

            if (size != null)
            {
                pickDetailList = pickDetailList.Where(x => x.SizeBundle == size).ToList();
            }

            if (customer != null)
            {
                pickDetailList = pickDetailList.Where(x => x.CustomerCode == customer).ToList();
            }

            if (location != null)
            {
                pickDetailList = pickDetailList.Where(x => x.Location == location).ToList();
            }

            foreach(var i in pickDetailList)
            {
                var history = new PickDetailHistory {
                    Container = i.Container,
                    vendor = i.ShipOrder.Vendor,
                    CutPO = i.PurchaseOrder,
                    ShipPO = i.ShipOrder.OrderPurchaseOrder,
                    ShipDate = i.ShipOrder.ShipDate,
                    CustomerCode = i.CustomerCode,
                    Style = i.Style,
                    Color = i.Color,
                    Size = i.SizeBundle,
                    Pcs = i.PcsBundle,
                    PickedCtns = i.PickCtns,
                    PickedPcs = i.PickPcs,
                    AllocatedBy = i.FCRegularLocationDetail == null ? i.ReplenishmentLocationDetail.Operator : i.FCRegularLocationDetail.Allocator,
                    CartonRange = i.CartonRange,
                    Batch = i.FCRegularLocationDetail == null ? "N/A" : i.FCRegularLocationDetail.Batch,
                    Location = i.FCRegularLocationDetail == null ? i.ReplenishmentLocationDetail.Location : i.FCRegularLocationDetail.Location
                };

                historyList.Add(history);
            }

            return Ok(historyList);
        }
    }


    public class PickDetailHistory
    {
        public string Container { get; set; }

        public string vendor { get; set; }

        public string CutPO { get; set; }

        public string ShipPO { get; set; }

        public DateTime ShipDate { get; set; }

        public string CustomerCode { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Size { get; set; }

        public string Pcs { get; set; }

        public int PickedCtns { get; set; }

        public int PickedPcs { get; set; }

        public string AllocatedBy { get; set; }

        public string CartonRange { get; set; }

        public string Batch { get; set; }

        public string Location { get; set; }
    }
}
