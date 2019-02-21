using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Dtos.Fba;
using ClothResorting.Models;
using ClothResorting.Models.FBAModels;
using ClothResorting.Models.StaticClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBAIndexController : ApiController
    {
        private ApplicationDbContext _context;

        public FBAIndexController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/fba/index
        [HttpGet]
        public IHttpActionResult GetActiveCustomers()
        {
            return Ok(_context.UpperVendors
                .Where(x => x.Status == Status.Active && x.DepartmentCode == DepartmentCode.FBA)
                .Select(Mapper.Map<UpperVendor, UpperVendorDto>));
        }

        // POST /api/fba/index/?requestId={requestId}
        [HttpPost]
        public IHttpActionResult PushDataFromFrontierSystem([FromUri]string requestId, [FromBody]FBAMasterOrderAPIDto order)
        {
            if (ModelState.IsValid)
                return Ok();
            else
                throw new Exception("Invalid!");
        }
    }
}
