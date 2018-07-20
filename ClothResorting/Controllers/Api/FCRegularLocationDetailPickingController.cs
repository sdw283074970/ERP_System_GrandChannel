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
    public class FCRegularLocationDetailPickingController : ApiController
    {
        private ApplicationDbContext _context;

        public FCRegularLocationDetailPickingController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/FCRegularLocationDetailPicking
        public IHttpActionResult GetAllPicking([FromUri]int id)
        {
            var resultDto = _context.FCRegularLocationDetails
                .Include(c => c.PreReceiveOrder)
                .Where(c => c.PreReceiveOrder.Id == id
                    && c.Status == "Picking")
                .ToList()
                .Select(Mapper.Map<FCRegularLocationDetail, FCRegularLocationDetailDto>);

            return Ok(resultDto);
        }
    }
}
