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
using System.Web;
using ClothResorting.Models.StaticClass;

namespace ClothResorting.Controllers.Api
{
    public class PurchaseOrderStatisticsController : ApiController
    {
        private ApplicationDbContext _context;
        private DateTime _timeNow = DateTime.Now;
        private string _userName;

        public PurchaseOrderStatisticsController()
        {
            _context = new ApplicationDbContext();
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0];
        }

        // GET /api/purchaseorderstatistics/?po={po}
        [HttpGet]
        public IHttpActionResult GetPoStatistics([FromUri]string po)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = _context.SpeciesInventories
                .Where(c => c.PurchaseOrder == po);

            var resultDto = Mapper.Map<IEnumerable<SpeciesInventory>, IEnumerable<SpeciesInventoryDto>>(result);

            return Ok(resultDto);
        }

        // POST /api/purchaseOrderStatistics
        [HttpPost]
        public IHttpActionResult CreateAdjustmentRecord([FromBody]AdjustmentJsonObj obj)
        {
            var speciesInDb = _context.SpeciesInventories
                .Include(c => c.PurchaseOrderInventory)
                .SingleOrDefault(c => c.PurchaseOrder == obj.PurchaseOrder
                    && c.Style == obj.Style
                    && c.Color == obj.Color
                    && c.Size == obj.Size);

            if (speciesInDb == null)
            {
                throw new Exception("Invalid SKU input.");
            }

            if (obj.Adjust > 0)
            {
                _context.ReplenishmentLocationDetails.Add(new ReplenishmentLocationDetail
                {
                    PurchaseOrder = obj.PurchaseOrder,
                    Style = obj.Style,
                    Color = obj.Color,
                    Size = obj.Size,
                    Location = "Adjustment",
                    InboundDate = _timeNow,
                    Quantity = obj.Adjust,
                    AvailablePcs = obj.Adjust,
                    Operator = _userName,
                    Editor = _userName,
                    Vendor = speciesInDb.Vendor,
                    IsHanger = false,
                    Status = Status.InStock,
                    SpeciesInventory = speciesInDb,
                    PurchaseOrderInventory = speciesInDb.PurchaseOrderInventory
                });
            }

            _context.AdjustmentRecords.Add(new AdjustmentRecord {
                PurchaseOrder = obj.PurchaseOrder,
                Style = obj.Style,
                Color = obj.Color,
                Size = obj.Size,
                Adjustment = obj.Adjust < 0 ? "-" + -obj.Adjust : "+" + obj.Adjust,
                AdjustDate = _timeNow,
                SpeciesInventory = speciesInDb,
                Balance = (speciesInDb.AvailablePcs + obj.Adjust).ToString(),
                Memo = obj.Description
            });

            speciesInDb.AdjPcs += obj.Adjust;
            speciesInDb.AvailablePcs += obj.Adjust;
            speciesInDb.PurchaseOrderInventory.AvailablePcs += obj.Adjust;

            _context.SaveChanges();

            var sampleInDb = _context.AdjustmentRecords
                .OrderByDescending(c => c.Id)
                .First();

            var sampleDto = Mapper.Map<AdjustmentRecord, AdjustmentRecordDto>(sampleInDb);

            return Created(Request.RequestUri + "/" + sampleInDb.Id, sampleDto);
        }
    }
}
