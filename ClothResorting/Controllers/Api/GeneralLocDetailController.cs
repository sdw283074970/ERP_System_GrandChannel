using AutoMapper;
using ClothResorting.Dtos;
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
    public class GeneralLocDetailController : ApiController
    {
        private ApplicationDbContext _context;

        public GeneralLocDetailController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/generallocdetail/?generalLocationSummaryId={generalLocationSummaryId}
        [HttpGet]
        public IHttpActionResult GetRegularLocationDetail([FromUri]int generalLocationSummaryId)
        {
            var result = _context.ReplenishmentLocationDetails
                .Include(x => x.GeneralLocationSummary)
                .Where(c => c.GeneralLocationSummary.Id == generalLocationSummaryId)
                .ToList();

            var resultDto = Mapper.Map<List<ReplenishmentLocationDetail>, List<ReplenishmentLocationDetailDto>>(result);

            return Ok(resultDto);
        }
    }
}
