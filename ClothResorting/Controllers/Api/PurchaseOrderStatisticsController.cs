using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ClothResorting.Controllers.Api
{
    public class PurchaseOrderStatisticsController : ApiController
    {
        private ApplicationDbContext _context;

        public PurchaseOrderStatisticsController()
        {
            _context = new ApplicationDbContext();
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
    }
}
