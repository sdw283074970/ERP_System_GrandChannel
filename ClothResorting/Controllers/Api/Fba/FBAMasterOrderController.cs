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
using ClothResorting.Models.FBAModels.StaticModels;
using System.Web;

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBAMasterOrderController : ApiController
    {
        private ApplicationDbContext _context;
        private string _userName;

        public FBAMasterOrderController()
        {
            _context = new ApplicationDbContext();
            _userName = HttpContext.Current.User.Identity.Name.Split('@').First();
        }

        // GET /api/fba/fbamasterorder/
        [HttpGet]
        public IHttpActionResult GetAllMasterOrders()
        {
            var masterOrders = _context.FBAMasterOrders
                .Include(x => x.InvoiceDetails)
                .Include(x => x.FBAOrderDetails)
                .Include(x => x.FBAPallets)
                .ToList();

            var skuList = new List<int>();

            foreach (var m in masterOrders)
            {
                m.TotalAmount = (float)m.InvoiceDetails.Sum(x => x.Amount);
                m.TotalCBM = m.FBAOrderDetails.Sum(x => x.CBM);
                m.TotalCtns = m.FBAOrderDetails.Sum(x => x.Quantity);
                m.ActualCBM = m.FBAOrderDetails.Sum(x => x.ActualCBM);
                m.ActualCtns = m.FBAOrderDetails.Sum(x => x.ActualQuantity);
                m.ActualPlts = m.FBAPallets.Sum(x => x.ActualPallets);
                skuList.Add(m.FBAOrderDetails.GroupBy(x => x.ShipmentId).Count());
            }

            var resultDto = Mapper.Map<IList<FBAMasterOrder>, IList<FBAMasterOrderDto>>(masterOrders);

            for (int i = 0; i < masterOrders.Count; i++)
            {
                resultDto[i].SKUNumber = skuList[i];
            }
            return Ok(resultDto);
        }

        // GET /api/fba/fbamasterorder/{id}
        [HttpGet]
        public IHttpActionResult GetMasterOrders([FromUri]int id)
        {
            var masterOrders = _context.FBAMasterOrders
                .Include(x => x.InvoiceDetails)
                .Include(x => x.FBAOrderDetails)
                .Include(x => x.Customer)
                .Include(x => x.FBAPallets)
                .Where(x => x.Customer.Id == id)
                .ToList();

            var skuList = new List<int>();

            foreach (var m in masterOrders)
            {
                m.TotalAmount = (float)m.InvoiceDetails.Sum(x => x.Amount);
                m.TotalCBM = m.FBAOrderDetails.Sum(x => x.CBM);
                m.TotalCtns = m.FBAOrderDetails.Sum(x => x.Quantity);
                m.ActualCBM = m.FBAOrderDetails.Sum(x => x.ActualCBM);
                m.ActualCtns = m.FBAOrderDetails.Sum(x => x.ActualQuantity);
                m.ActualPlts = m.FBAPallets.Sum(x => x.ActualPallets);
                skuList.Add(m.FBAOrderDetails.GroupBy(x => x.ShipmentId).Count());
            }

            var resultDto = Mapper.Map<IList<FBAMasterOrder>, IList< FBAMasterOrderDto >>(masterOrders);

            for (int i = 0; i < masterOrders.Count; i++)
            {
                resultDto[i].SKUNumber = skuList[i];
            }
            return Ok(resultDto);
        }

        // GET /api/fba/fbamasterorder/?grandNumber={grandNumber}&operation={edit}
        [HttpGet]
        public IHttpActionResult GetMasterOrderInfo([FromUri]string grandNumber, [FromUri]string operation)
        {
            if (operation == FBAOperation.Edit)
            {
                var masterOrderInDb = _context.FBAMasterOrders
                    .SingleOrDefault(x => x.GrandNumber == grandNumber);

                var resultDto = Mapper.Map<FBAMasterOrder, FBAMasterOrderDto>(masterOrderInDb);

                return Ok(resultDto);
            }

            return Ok();
        }

        //POST /api/fba/fbamasterorder/{id}
        [HttpPost]
        public IHttpActionResult CreateMasterOrder([FromBody]FBAMasterOrder obj, [FromUri]int id)
        {
            if (_context.FBAMasterOrders.SingleOrDefault(x => x.Container == obj.Container) != null)
            {
                throw new Exception("Contianer Number " + obj.Container + " has been taken. Please delete the existed order and try agian.");
            }

            var customer = _context.UpperVendors.Find(id);
            var customerCode = customer.CustomerCode;
            //Unix时间戳加客户代码组成独一无二的GrandNumber
            var grandNumber = customerCode + ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000).ToString();

            if(_context.FBAMasterOrders.Where(x => x.GrandNumber == grandNumber).Count() > 0)
            {
                throw new Exception("Grand Number " + grandNumber + " has been taken. Please try agian.");
            }

            var masterOrder = new FBAMasterOrder();

            masterOrder.AssembleFirstPart(obj.ETA, obj.Carrier, obj.Vessel, obj.Voy, obj.ETD);
            masterOrder.AssembeSecondPart(obj.ETAPort, obj.PlaceOfReceipt, obj.PortOfLoading, obj.PortOfDischarge, obj.PlaceOfDelivery);
            masterOrder.AssembeThirdPart(obj.SealNumber, obj.ContainerSize, obj.Container);
            masterOrder.GrandNumber = grandNumber;
            masterOrder.Customer = customer;
            masterOrder.OriginalPlts = obj.OriginalPlts;
            masterOrder.InboundType = obj.InboundType;
            masterOrder.InvoiceStatus = "Await";
            masterOrder.UpdateLog += "Update by " + _userName + " at " + DateTime.Now.ToString() + ". ";

            _context.FBAMasterOrders.Add(masterOrder);
            _context.SaveChanges();

            var resultDto = Mapper.Map<FBAMasterOrder, FBAMasterOrderDto>(_context.FBAMasterOrders.SingleOrDefault(x => x.GrandNumber == grandNumber));
            return Created(Request.RequestUri + "/" + resultDto.Id, resultDto);
        }

        // PUT /api/fba/fbamasterOrder/?masterOrderId={masterOrderId}&container={container}&inboundDate={inboundDate}
        [HttpPut]
        public void UpdateMasterOrderInfo([FromUri]string masterOrderId, [FromUri]string container, [FromUri]string inboundDate)
        {
            var inboundDateTime = new DateTime();
            inboundDateTime = ParseStringToDateTime(inboundDateTime, inboundDate);

            var masterOrderInDb = _context.FBAMasterOrders.Include(x => x.FBAOrderDetails).SingleOrDefault(x => x.GrandNumber == masterOrderId);

            if (container != "NULL")
            {
                masterOrderInDb.Container = container;
            }

            masterOrderInDb.InboundDate = inboundDateTime;
            masterOrderInDb.ReceivedBy = _userName;

            foreach(var detail in masterOrderInDb.FBAOrderDetails)
            {
                detail.Container = container;
            }

            _context.SaveChanges();
        }

        // PUT /api/fba/fbamasterorder/?grandNumber={grandNumber}&operation={edit}
        [HttpPut]
        public void UpdateMasterOrderInfo([FromUri]string grandNumber, [FromBody]FBAMasterOrder obj)
        {
            var masterOrderInDb = _context.FBAMasterOrders
                .SingleOrDefault(x => x.GrandNumber == grandNumber);

            var currentContainer = masterOrderInDb.Container;

            if (currentContainer != obj.Container && _context.FBAMasterOrders.SingleOrDefault(x => x.Container == obj.Container) != null)
            {
                throw new Exception("Contianer Number " + obj.Container + " has been taken. Please delete the existed order and try agian.");
            }

            masterOrderInDb.Carrier = obj.Carrier;
            masterOrderInDb.Vessel = obj.Vessel;
            masterOrderInDb.Voy = obj.Voy;
            masterOrderInDb.ETA = obj.ETA;
            masterOrderInDb.ETD = obj.ETD;
            masterOrderInDb.ETAPort = obj.ETAPort;
            masterOrderInDb.PlaceOfReceipt = obj.PlaceOfReceipt;
            masterOrderInDb.PortOfLoading = obj.PortOfLoading;
            masterOrderInDb.PortOfDischarge = obj.PortOfDischarge;
            masterOrderInDb.PlaceOfDelivery = obj.PlaceOfDelivery;
            masterOrderInDb.Container = obj.Container;
            masterOrderInDb.OriginalPlts = obj.OriginalPlts;
            masterOrderInDb.SealNumber = obj.SealNumber;
            masterOrderInDb.InboundType = obj.InboundType;

            _context.SaveChanges();
        }

        // DELETE /api/fba/fbamasterorder/?grandNumber={grandNumber}
        [HttpDelete]
        public void DeleteMasterOrder([FromUri]string grandNumber)
        {
            var masterOrderId = _context.FBAMasterOrders.SingleOrDefault(x => x.GrandNumber == grandNumber).Id;

            var invoiceDetails = _context.InvoiceDetails
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.FBAMasterOrder.Id == masterOrderId);

            _context.InvoiceDetails.RemoveRange(invoiceDetails);

            var chargingItemDetails = _context.ChargingItemDetails
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.FBAMasterOrder.GrandNumber == grandNumber);

            _context.ChargingItemDetails.RemoveRange(chargingItemDetails);

            var cartonLocationsInDb = _context.FBACartonLocations
                .Include(x => x.FBAOrderDetail.FBAMasterOrder)
                .Where(x => x.FBAOrderDetail.FBAMasterOrder.Id == masterOrderId);

            _context.FBACartonLocations.RemoveRange(cartonLocationsInDb);

            var palletLocationsInDb = _context.FBAPalletLocations
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.FBAMasterOrder.Id == masterOrderId);

            _context.FBAPalletLocations.RemoveRange(palletLocationsInDb);

            var orderDetailsInDb = _context.FBAOrderDetails
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.FBAMasterOrder.Id == masterOrderId);

            _context.FBAOrderDetails.RemoveRange(orderDetailsInDb);

            var palletsInDb = _context.FBAPallets
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.FBAMasterOrder.Id == masterOrderId);

            _context.FBAPallets.RemoveRange(palletsInDb);

            var masterOrderInDb = _context.FBAMasterOrders.Find(masterOrderId);

            _context.FBAMasterOrders.Remove(masterOrderInDb);

            try
            {
                _context.SaveChanges();

            }
            catch (Exception e)
            {
                throw new Exception("Cannot delete this master order. Please delete related ship order first then try again.");
            }
        }

        private DateTime ParseStringToDateTime(DateTime dateTime, string stringTime)
        {
            DateTime.TryParseExact(stringTime, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
            return dateTime;
        }
    }
}
