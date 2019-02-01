using AutoMapper;
using ClothResorting.Dtos.Fba;
using ClothResorting.Helpers;
using ClothResorting.Models;
using ClothResorting.Models.FBAModels;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Data.Entity;

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

        // GET /api/fba/fbashiporder/?shipOrderId={shipOrderId}
        [HttpGet]
        public IHttpActionResult GetBolFileName([FromUri]int shipOrderId)
        {
            var bolList = new List<FBABOLDetail>();

            var pickDetailsInDb = _context.FBAPickDetails
                .Include(x => x.FBAShipOrder)
                .Where(x => x.FBAShipOrder.Id == shipOrderId);

            foreach(var pickDetail in pickDetailsInDb)
            {
                bolList.Add(new FBABOLDetail {
                    CustoerOrderNumber = pickDetail.ShipmentId,
                    Contianer = pickDetail.Container,
                    CartonQuantity = pickDetail.ActualQuantity,
                    PalletQuantity = pickDetail.ActualPlts,
                    Weight = pickDetail.ActualGrossWeight,
                    Location = pickDetail.Location
                });
            }

            var generator = new PDFGenerator();

            var fileName = generator.GenerateFBABOL(shipOrderId, bolList);

            return Ok(fileName);
        }

        // POST /api/fba/fbashiporder/
        [HttpPost]
        public IHttpActionResult CreateNewShipOrder([FromBody]ShipOrderDto obj)
        {
            var shipOrder = new FBAShipOrder();
            var ets = new DateTime();

            DateTime.TryParseExact(obj.ETS, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out ets);

            shipOrder.AssembleBaseInfo(obj.ShipOrderNumber, obj.CustomerCode, obj.OrderType, obj.Destination, obj.PickReference);
            shipOrder.CreateBy = _userName;
            shipOrder.ShipDate = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            shipOrder.BOLNumber = obj.BOLNumber;
            shipOrder.Carrier = obj.Carrier;
            shipOrder.ETS = ets;

            _context.FBAShipOrders.Add(shipOrder);
            _context.SaveChanges();

            var sampleDto = Mapper.Map<FBAShipOrder, FBAShipOrderDto>(_context.FBAShipOrders.OrderByDescending(x => x.Id).First());

            return Created(Request.RequestUri + "/" + sampleDto.Id, sampleDto);
        }
    }

    public class ShipOrderDto
    {
        public string ShipOrderNumber { get; set; }

        public string CustomerCode { get; set; }

        public string OrderType { get; set; }

        public string Destination { get; set; }

        public string PickReference { get; set; }

        public DateTime CreateDate { get; set; }

        public string CreateBy { get; set; }

        public DateTime PickDate { get; set; }

        public DateTime ShipDate { get; set; }

        public string PickMan { get; set; }

        public string Status { get; set; }

        public string ShippedBy { get; set; }

        public string BOLNumber { get; set; }

        public string Carrier { get; set; }

        public string ETS { get; set; }
    }
}
