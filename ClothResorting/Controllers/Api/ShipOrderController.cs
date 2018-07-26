using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Models;
using ClothResorting.Models.ApiTransformModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ClothResorting.Controllers.Api
{
    public class ShipOrderController : ApiController
    {
        private ApplicationDbContext _context;

        public ShipOrderController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/shiporder/
        [HttpGet]
        public IHttpActionResult GetAllShipOrder()
        {
            var result = _context.ShipOrders.Where(x => x.Id > 0).Select(Mapper.Map<ShipOrder, ShipOrderDto>);
            return Ok(result);
        }

        // POST /api/shiporder/
        [HttpPost]
        public IHttpActionResult CreateNewShipOrder([FromBody]ShipOrderJsonObj Obj)
        {
            _context.ShipOrders.Add(new ShipOrder {
                OrderPurchaseOrder = Obj.OrderPurchaseOrder,
                Customer = Obj.Customer,
                Address_1 = Obj.Address_1,
                Address_2 = Obj.Address_2,
                PickTicketsRange = Obj.PickTicketsRange,
                ShipDate = DateTime.Now.ToString("MM/dd/yyyy"),
                Status = "New Create"
            });

            _context.SaveChanges();

            var sample = _context.ShipOrders.OrderByDescending(x => x.Id).First();
            var sampleDto = Mapper.Map<ShipOrder, ShipOrderDto>(sample);
            return Created(Request.RequestUri + "/" + sampleDto.Id, sampleDto);
        }
    }
}
