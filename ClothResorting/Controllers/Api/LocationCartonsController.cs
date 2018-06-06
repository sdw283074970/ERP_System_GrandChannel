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
    public class LocationCartonsController : ApiController
    {
        private ApplicationDbContext _context;

        public LocationCartonsController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/LocationCartons/ 从地址栏获取preid和po，返回所有有收货记录的CartonBreakDown
        [HttpPost]
        public IHttpActionResult GetCartonDetail([FromBody]PreIdPoJsonObj obj)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var preId = obj.PreId;
            var po = obj.Po;

            var cartons = _context.CartonBreakDowns
                .Include(c => c.SilkIconPackingList.SilkIconPreReceiveOrder)
                .Where(c => c.SilkIconPackingList.SilkIconPreReceiveOrder.Id == preId
                    && c.SilkIconPackingList.PurchaseOrderNumber == po
                    && c.ActualPcs != 0)
                .Select(Mapper.Map<CartonBreakDown, CartonBreakDownDto>);

            return Ok(cartons);
        }
    }
}
