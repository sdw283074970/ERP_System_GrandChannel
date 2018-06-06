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
    public class LocationCartonsController : ApiController
    {
        private ApplicationDbContext _context;

        public LocationCartonsController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/InStockCartons/输入Po号，返回所有有收货记录的CartonBreakDown
        [HttpPost]
        public IHttpActionResult GetCartonDetail([FromBody]string po)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var cartons = _context.CartonBreakDowns
                .Where(c => c.PurchaseNumber == po && c.ActualPcs != 0)
                .Select(Mapper.Map<CartonBreakDown, CartonBreakDownDto>);

            return Ok(cartons);
        }
    }
}
