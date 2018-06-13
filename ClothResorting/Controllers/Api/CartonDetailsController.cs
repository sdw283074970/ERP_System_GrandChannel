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
using ClothResorting.Models.ApiTransformModels;

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
        public IHttpActionResult GetPurchaseOrderDetail([FromBody]PreIdPoJsonObj obj)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var preId = obj.PreId;
            var po = obj.Po;

            var cartonDetails = _context.CartonDetails
                .Include(c => c.PackingList.PreReceiveOrder)
                .Where(s => s.PackingList.PreReceiveOrder.Id == preId
                    && s.PackingList.PurchaseOrder == po)
                .Select(Mapper.Map<CartonDetail, CartonDetailDto>);

            return Ok(cartonDetails);
        }
    }
}
