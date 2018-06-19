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
using AutoMapper;

namespace ClothResorting.Controllers.Api
{
    public class LocationDetailController : ApiController
    {
        private ApplicationDbContext _context;

        public LocationDetailController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/locationdeatil
        [HttpGet]
        public IHttpActionResult GetLocationDetail([FromUri]PreIdPoJsonObj obj)
        {
            var result = new List<LocationDetail>();

            var query = _context.LocationDetails
                .Include(c => c.PackingList.PreReceiveOrder)
                .Where(c => c.PurchaseOrder == obj.Po && c.PackingList.PreReceiveOrder.Id == obj.PreId)
                .ToList();

            result.AddRange(query);

            var resultDto = Mapper.Map<List<LocationDetail>, List<LocationDetailDto>>(result);

            return Ok(resultDto);
        }
    }
}
