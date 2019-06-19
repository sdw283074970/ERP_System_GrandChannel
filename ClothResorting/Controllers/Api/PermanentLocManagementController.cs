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
}
