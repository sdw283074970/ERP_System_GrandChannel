using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using ClothResorting.Models.StaticClass;

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
                var history = ConvertPickDetailToPickDetailHistory(i);

                historyList.Add(history);
            }

            return Ok(historyList);
        }

        // GET /api/shipDetailHistory/?locationId={locationId}&orderType={orderType}
        [HttpGet]
        public IHttpActionResult GetHistoryByLocationId([FromUri]int locationId, [FromUri]string orderType)
        {
            var historyList = new List<PickDetailHistory>();

            if (orderType == OrderType.Replenishment)
            {
                var pickDetails = _context.PickDetails
                    .Include(x => x.ShipOrder)
                    .Include(x => x.ReplenishmentLocationDetail)
                    .Where(x => x.ReplenishmentLocationDetail.Id == locationId);

                foreach(var p in pickDetails)
                {
                    var history = ConvertPickDetailToPickDetailHistory(p);

                    historyList.Add(history);
                }
            }
            else if (orderType == OrderType.Regular)
            {
                var pickDetails = _context.PickDetails
                    .Include(x => x.ShipOrder)
                    .Include(x => x.FCRegularLocationDetail)
                    .Where(x => x.FCRegularLocationDetail.Id == locationId);

                foreach (var p in pickDetails)
                {
                    var history = ConvertPickDetailToPickDetailHistory(p);

                    historyList.Add(history);
                }
            }

            return Ok(historyList);
        }

        private PickDetailHistory ConvertPickDetailToPickDetailHistory(PickDetail pickDetail)
        {
            return new PickDetailHistory
            {
                Container = pickDetail.Container,
                vendor = pickDetail.ShipOrder.Vendor,
                CutPO = pickDetail.PurchaseOrder,
                ShipPO = pickDetail.ShipOrder.OrderPurchaseOrder,
                ShipDate = pickDetail.ShipOrder.ShipDate,
                CustomerCode = pickDetail.CustomerCode,
                Style = pickDetail.Style,
                Status = pickDetail.ShipOrder.Status,
                Color = pickDetail.Color,
                Size = pickDetail.SizeBundle,
                Pcs = pickDetail.PcsBundle,
                PickedCtns = pickDetail.PickCtns,
                PickedPcs = pickDetail.PickPcs,
                AllocatedBy = pickDetail.FCRegularLocationDetail == null ? pickDetail.ReplenishmentLocationDetail.Operator : pickDetail.FCRegularLocationDetail.Allocator,
                CartonRange = pickDetail.CartonRange,
                Batch = pickDetail.FCRegularLocationDetail == null ? "N/A" : pickDetail.FCRegularLocationDetail.Batch,
                Location = pickDetail.FCRegularLocationDetail == null ? pickDetail.ReplenishmentLocationDetail.Location : pickDetail.FCRegularLocationDetail.Location
            };
        }
    }


    public class PickDetailHistory
    {
        public string Container { get; set; }

        public string Status { get; set; }

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
