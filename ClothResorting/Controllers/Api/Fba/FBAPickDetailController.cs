using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using AutoMapper;
using ClothResorting.Models.FBAModels;
using ClothResorting.Dtos.Fba;

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBAPickDetailController : ApiController
    {
        private ApplicationDbContext _context;

        public FBAPickDetailController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/fba/fbapickdetail/?shipOrderId={shipOrderId}&orderType={orderType}
        [HttpGet]
        public IHttpActionResult GetPickDetail([FromUri]int shipOrderId, [FromUri]string orderType)
        {
            return Ok(_context.FBAPickDetails
                .Include(x => x.FBAShipOrder)
                .Where(x => x.FBAShipOrder.Id == shipOrderId && x.OrderType == orderType)
                .Select(Mapper.Map<FBAPickDetail, FBAPickDetailsDto>));
        }
    }
}
