using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using AutoMapper;
using ClothResorting.Dtos;

namespace ClothResorting.Controllers.Api
{
    public class AdjustmentDetailController : ApiController
    {
        private ApplicationDbContext _context;

        public AdjustmentDetailController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/AdjustmentDetail/?poid={poid}
        [HttpGet]
        public IHttpActionResult GetAdjustmentDetail([FromUri]int poid)
        {
            var result = _context.AdjustmentRecords
                .Include(c => c.SpeciesInventory)
                .Where(c => c.SpeciesInventory.Id == poid);

            var resultDto = Mapper.Map<IEnumerable<AdjustmentRecord>, IEnumerable<AdjustmentRecordDto>>(result);

            return Ok(resultDto);
        }
    }
}
