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
            var result = _context.PermanentSKUs
                .Select(Mapper.Map<PermanentSKU, PermanentSKUDto>);

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
                PurchaseOrder = obj.PurchaseOrder,
                Style = obj.Style,
                Color = obj.Color,
                Size = obj.Size,
                Quantity = 0
            };

            //查重
            var skuInDb = _context.PermanentSKUs
                .Where(x => x.PurchaseOrder == obj.PurchaseOrder
                    && x.Vendor == obj.Vender
                    && x.Style == obj.Style
                    && x.Color == obj.Color
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
    }

    public class PermanentHistory
    {
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
