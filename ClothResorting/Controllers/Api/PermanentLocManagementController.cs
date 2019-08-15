using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Models;
using ClothResorting.Models.ApiTransformModels;
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
    public class PermanentLocManagementController : ApiController
    {
        private ApplicationDbContext _context;

        public PermanentLocManagementController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/permanentlocmanagement
        [HttpGet]
        public IHttpActionResult GetAllPermanentLocation()
        {
            var resultInDb = _context.PermanentSKUs
                .Include(x => x.InboundLogs)
                .Include(x => x.OutboundLogs.Select(c => c.ShipOrder));

            var list = resultInDb.ToList();

            foreach(var r in resultInDb)
            {
                r.Quantity = r.InboundLogs.Count() == 0 ? 0 : r.InboundLogs.Sum(x => x.ToPermanentPcs);
                r.ShippedPcs = r.OutboundLogs.Where(x => x.ShipOrder.Status == Status.Shipped).Count() == 0 ? 0: r.OutboundLogs.Where(x => x.ShipOrder.Status == Status.Shipped).Sum(x => x.PickPcs);
                r.PickingPcs = r.OutboundLogs.Where(x => x.ShipOrder.Status != Status.Shipped).Count() == 0? 0 : r.OutboundLogs.Where(x => x.ShipOrder.Status != Status.Shipped).Sum(x => x.PickPcs);
                r.AvailablePcs = r.Quantity - r.ShippedPcs - r.PickingPcs;
            }

            var result = Mapper.Map<IEnumerable<PermanentSKU>, IEnumerable<PermanentSKUDto>>(resultInDb);

            return Ok(result);
        }

        // GET /api/permanentlocamanagement/?locId={locId}
        [HttpGet]
        public IHttpActionResult GetHistory([FromUri]int locId)
        {
            var historyList = new List<PermanentHistory>();

            var cartonDetailsInDb = _context.RegularCartonDetails
                .Include(x => x.PermanentSKU)
                .Where(x => x.PermanentSKU.Id == locId);

            var pickDetailsInDb = _context.PickDetails
                .Include(x => x.PermanentSKU)
                .Include(x => x.ShipOrder)
                .Where(x => x.PermanentSKU.Id == locId);

            var containerList = _context.Containers.ToList();

            foreach(var c in cartonDetailsInDb)
            {
                historyList.Add(new PermanentHistory {
                    Id = c.Id,
                    InOrPickDate = containerList.SingleOrDefault(x => x.ContainerNumber == c.Container).InboundDate.ToString("MM/dd/yyyy"),
                    RefType = "Inbound",
                    RefNumber = c.Container,
                    PurchaseOrder = c.PurchaseOrder,
                    Style = c.Style,
                    Size = c.SizeBundle,
                    Color = c.Color,
                    QuantityChange = c.ToPermanentPcs,
                    CartonChange = c.ToPermanentCtns,
                    DisplayQuantityChange = "+" + c.ToPermanentPcs.ToString(),
                    DisplayCartonChange = "+" + c.ToPermanentCtns.ToString()
                });
            }

            foreach(var p in pickDetailsInDb)
            {
                historyList.Add(new PermanentHistory
                {
                    Id = p.Id,
                    InOrPickDate = p.PickDate,
                    RefType = "Outbound",
                    RefNumber = p.ShipOrder.OrderPurchaseOrder,
                    PurchaseOrder = p.PurchaseOrder,
                    Style = p.Style,
                    Size = p.SizeBundle,
                    Color = p.Color,
                    QuantityChange = p.PickPcs,
                    CartonChange = 0,
                    DisplayQuantityChange = "-" + p.PickPcs.ToString(),
                    DisplayCartonChange = "NA"
                });
            }

            return Ok(historyList);
        }

        // POST /api/permanentlocmanagement
        [HttpPost]
        public IHttpActionResult CreateNewPermanentLocation([FromBody]PermanentLocJsonObj obj)
        {
            var location = new PermanentSKU
            {
                Location = obj.Location,
                Vendor = obj.Vender,
                UPCNumber = obj.UPCNumber,
                PurchaseOrder = obj.PurchaseOrder,
                Style = obj.Style,
                Color = obj.Color,
                Size = obj.Size,
                Quantity = 0,
                Status = Status.Active
            };

            //查重
            var skuInDb = _context.PermanentSKUs
                .Where(x => x.PurchaseOrder == obj.PurchaseOrder
                    && x.Vendor == obj.Vender
                    && x.Style == obj.Style
                    && x.Color == obj.Color
                    && x.UPCNumber == obj.UPCNumber
                    && x.Size == obj.Size)
                .ToList();

            if (skuInDb.Count > 0)
            {
                throw new Exception("This SKU already exist in the system.");
            }
            else
            {
                _context.PermanentSKUs.Add(location);
            }

            _context.SaveChanges();

            var id = _context.PermanentSKUs.OrderByDescending(c => c.Id).First().Id;

            var results = _context.PermanentSKUs
                .Where(c => c.Id > 0)
                .OrderByDescending(c => c.Id)
                .ToList()
                .Select(Mapper.Map<PermanentSKU, PermanentSKUDto>);

            return Created(Request.RequestUri + "/" + id, results);
        }

        // PUT /api/permanentlocmanagement/?cartonDetailId={cartonDetailId}
        [HttpPut]
        public void PutbackInboundRecord([FromUri]int cartonDetailId)
        {
            var cartonDetailInDb = _context.RegularCartonDetails.Find(cartonDetailId);
            cartonDetailInDb.ToBeAllocatedCtns = cartonDetailInDb.ToPermanentCtns;
            cartonDetailInDb.ToBeAllocatedPcs = cartonDetailInDb.ToPermanentPcs;

            cartonDetailInDb.ToPermanentCtns = 0;
            cartonDetailInDb.ToPermanentPcs = 0;

            _context.SaveChanges();
        }

        // PUT /api/permanentlocmanagement/?locId={locId}
        [HttpPut]
        public void ActiveOrDeactiveLoc([FromUri]int locId)
        {
            var locInDb = _context.PermanentSKUs.Find(locId);

            if (locInDb.Status == Status.Active)
                locInDb.Status = Status.Inactive;
            else
                locInDb.Status = Status.Active;

            _context.SaveChanges();
        }
    }

    public class PermanentHistory
    {
        public int Id { get; set; }
        public string RefType { get; set; }

        public string RefNumber { get; set; }

        public string PurchaseOrder { get; set; }

        public string Style { get; set; }

        public string Color { get; set; }

        public string Size { get; set; }

        public int QuantityChange { get; set; }

        public int CartonChange { get; set; }

        public string DisplayQuantityChange { get; set; }

        public string DisplayCartonChange { get; set; }

        public string InOrPickDate { get; set; }
    }
}
