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
    public class CartonDetailsController : ApiController
    {
        private ApplicationDbContext _context;

        public CartonDetailsController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/CartonDetails/输入po号，返回这个po号的所有CartonDetails
        [HttpPost]
        public IHttpActionResult GetPurchaseOrderDetail([FromBody]string po)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var cartonDetails = _context.SilkIconCartonDetails
                .Where(s => s.PurchaseOrderNumber == po)
                .Select(Mapper.Map<SilkIconCartonDetail, SilkIconCartonDetailDto>);

            return Ok(cartonDetails);
        }
    }
}
