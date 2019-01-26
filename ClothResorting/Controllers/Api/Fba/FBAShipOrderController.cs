using AutoMapper;
using ClothResorting.Dtos.Fba;
using ClothResorting.Models;
using ClothResorting.Models.FBAModels;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBAShipOrderController : ApiController
    {
        private ApplicationDbContext _context;
        private string _userName;

        public FBAShipOrderController()
        {
            _context = new ApplicationDbContext();
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0];
        }

        // GET /api/fba/fbashiporder/
        [HttpGet]
        public IHttpActionResult GetAllFBAShipOrder()
        {
            return Ok(_context.FBAShipOrders.Select(Mapper.Map<FBAShipOrder, FBAShipOrderDto>));
        }

        // POST /api/fba/fbashiporder/
        [HttpPost]
        public IHttpActionResult CreateNewShipOrder([FromBody]FBAShipOrder obj)
        {
            var shipOrder = new FBAShipOrder();

            shipOrder.AssembleBaseInfo(obj.ShipOrderNumber, obj.CustomerCode, obj.OrderType, obj.Destination, obj.PickReference);
            shipOrder.CreateBy = _userName;
            shipOrder.ShipDate = new DateTime(1970, 1, 1, 0, 0, 0, 0);

            _context.FBAShipOrders.Add(shipOrder);
            _context.SaveChanges();

            var sampleDto = Mapper.Map<FBAShipOrder, FBAShipOrderDto>(_context.FBAShipOrders.OrderByDescending(x => x.Id).First());

            return Created(Request.RequestUri + "/" + sampleDto.Id, sampleDto);
        }
    }
}
