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
using ClothResorting.Models.FBAModels.BaseClass;
using System.Globalization;

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBAMasterOrderController : ApiController
    {
        private ApplicationDbContext _context;

        public FBAMasterOrderController()
        {
            _context = new ApplicationDbContext();
        }

        //GET /api/fba/fbamasterorder/{id}
        [HttpGet]
        public IHttpActionResult GetMasterOrders([FromUri]int id)
        {
            return Ok(_context.FBAMasterOrders
                .Include(x => x.Customer)
                .Where(x => x.Customer.Id == id)
                .Select(Mapper.Map<FBAMasterOrder, FBAMasterOrderDto>));
        }

        //POST /api/fba/fbamasterorder/{id}
        [HttpPost]
        public IHttpActionResult CreateMasterOrder([FromBody]BaseFBAMasterOrder obj, [FromUri]int id)
        {
            var customer = _context.UpperVendors.Find(id);
            var customerCode = customer.CustomerCode;
            //Unix时间戳加客户代码组成独一无二的GrandNumber
            var grandNumber = customerCode + ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000).ToString();

            if(_context.FBAMasterOrders.Where(x => x.GrandNumber == grandNumber).Count() > 0)
            {
                throw new Exception("Grand Number " + grandNumber + " has been taken. Please try agian. The system will allocate another number for this order.");
            }

            var masterOrder = new FBAMasterOrder();

            masterOrder.AssembleFirstPart(obj.ETA, obj.Carrier, obj.Vessel, obj.Voy, obj.ETD);
            masterOrder.AssembeSecondPart(obj.ETAPort, obj.PlaceOfReceipt, obj.PortOfLoading, obj.PortOfDischarge, obj.PlaceOfDelivery);
            masterOrder.AssembeThirdPart(obj.SealNumber, obj.ContainerSize, obj.Container);
            masterOrder.GrandNumber = grandNumber;
            masterOrder.Customer = customer;

            _context.FBAMasterOrders.Add(masterOrder);
            _context.SaveChanges();

            var resultDto = Mapper.Map<FBAMasterOrder, FBAMasterOrderDto>(_context.FBAMasterOrders.SingleOrDefault(x => x.GrandNumber == grandNumber));
            return Created(Request.RequestUri + "/" + resultDto.Id, resultDto);
        }

        // PUT /api/fba/fbamasterOrder/?masterOrderId={masterOrderId}&container={container}&inboundDate={inboundDate}
        [HttpPut]
        public void UpdateMasterOrderInfo([FromUri]int masterOrderId, [FromUri]string container, [FromUri]string inboundDate)
        {
            var inboundDateTime = new DateTime();
            inboundDateTime = ParseStringToDateTime(inboundDateTime, inboundDate);

            var masterOrderInDb = _context.FBAMasterOrders.Include(x => x.FBAOrderDetails).SingleOrDefault(x => x.Id == masterOrderId);

            masterOrderInDb.Container = container;
            masterOrderInDb.InboundDate = inboundDateTime;

            foreach(var detail in masterOrderInDb.FBAOrderDetails)
            {
                detail.Container = container;
            }

            _context.SaveChanges();
        }

        public DateTime ParseStringToDateTime(DateTime dateTime, string stringTime)
        {
            DateTime.TryParseExact(stringTime, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
            return dateTime;
        }
    }
}
