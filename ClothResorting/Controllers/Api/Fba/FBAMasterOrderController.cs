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
using ClothResorting.Helpers;
using System.Threading.Tasks;
using ClothResorting.Models.StaticClass;

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBAMasterOrderController : ApiController
    {
        private ApplicationDbContext _context;
        private string _userName;
        private Logger _logger;

        public FBAMasterOrderController()
        {
            _context = new ApplicationDbContext();
            _userName = HttpContext.Current.User.Identity.Name.Split('@').First();
            _logger = new Logger(_context);
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

        // GET /api/fbamasterorder/?masterOrderId={masterOrderId}&operation={operation}
        [HttpGet]
        public IHttpActionResult GetUnloadWorkOrder([FromUri]int masterOrderId, [FromUri]string operation)
        {
            var masterOrderInDb = _context.FBAMasterOrders
                .Include(x => x.FBAOrderDetails.Select(c => c.FBACartonLocations))
                .Include(x => x.FBAPallets)
                .SingleOrDefault(x => x.Id == masterOrderId);

            if (operation == "WO")
            {
                var unloadWO = new UnloadWorkOrder
                {
                    UnloadingType = masterOrderInDb.UnloadingType,
                    StorageType = masterOrderInDb.StorageType,
                    Palletizing = masterOrderInDb.Palletizing,
                    PlaceTime = masterOrderInDb.PushTime,
                    UnloadFinishTime = masterOrderInDb.UnloadFinishTime,
                    ETA = masterOrderInDb.ETA,
                    Container = masterOrderInDb.Container,
                    GrandNumber = masterOrderInDb.GrandNumber,
                    Carrier = masterOrderInDb.Carrier,
                    InboundDate = masterOrderInDb.InboundDate,
                    OutTime = masterOrderInDb.OutTime,
                    IsDamaged = masterOrderInDb.IsDamaged,
                    VerifiedBy = masterOrderInDb.VerifiedBy,
                    UnloadStartTime = masterOrderInDb.UnloadStartTime,
                    DockNumber = masterOrderInDb.DockNumber
                };

                double logonProgress = masterOrderInDb.FBAOrderDetails != null ? (double)masterOrderInDb.FBAOrderDetails.Sum(x => x.ActualQuantity) / (double)masterOrderInDb.FBAOrderDetails.Sum(x => x.Quantity) : 0;
                unloadWO.LogonProgress = (float)Math.Round(logonProgress, 2) * 100;

                double registerProgress = masterOrderInDb.FBAPallets != null ? (double)masterOrderInDb.FBAPallets.Sum(x => x.ActualQuantity) / (double)masterOrderInDb.FBAOrderDetails.Sum(x => x.Quantity) : 0;
                unloadWO.RegisterProgress = (float)Math.Round(registerProgress, 2) * 100;

                double allocationProgress = masterOrderInDb.FBAOrderDetails != null ? (double)masterOrderInDb.FBAOrderDetails.Select(x => x.FBACartonLocations.Sum(c => c.ActualQuantity)).Sum() / (double)masterOrderInDb.FBAOrderDetails.Sum(z => z.Quantity) : 0;
                unloadWO.AllocationProgress = (float)Math.Round(allocationProgress, 2) * 100;

                var packingList = _context.FBAOrderDetails
                    .Include(x => x.FBAMasterOrder)
                    .Where(x => x.FBAMasterOrder.Id == masterOrderId)
                    .Select(Mapper.Map<FBAOrderDetail, FBAOrderDetailDto>);

                var chargingList = _context.ChargingItemDetails
                    .Include(x => x.FBAMasterOrder)
                    .Where(x => x.FBAMasterOrder.Id == masterOrderId
                        && x.Status != "N/A");

                foreach(var c in chargingList)
                {
                    unloadWO.OperationInstructions.Add(new OperationInstruction {
                        Id = c.Id,
                        Description = c.Description,
                        Comment = c.Comment,
                        Result = c.Result,
                        HandlingStatus = c.HandlingStatus,
                        Status = c.Status
                    });
                }

                unloadWO.PackingList = packingList.ToList();

                return Ok(unloadWO);
            }

            return Ok();
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

        // POST /api/fba/fbamasterorder/?masterOrderId={masterOrderId}&comment={comment}&operation={operation}
        [HttpPost]
        public async Task<IHttpActionResult> CreateNewCommentFromWarehouse([FromUri]int masterOrderId, [FromUri]string comment, [FromUri]string operation)
        {
            if (operation == "AddNewComment")
            {
                var masterOrderInDb = _context.FBAMasterOrders.Find(masterOrderId);
                masterOrderInDb.Status = FBAStatus.Pending;
                var newComment = new ChargingItemDetail
                {
                    CreateBy = _userName,
                    Comment = comment,
                    CreateDate = DateTime.Now,
                    Description = "Extral comment from warehouse",
                    HandlingStatus = FBAStatus.Pending,
                    Status = FBAStatus.Unhandled,
                    FBAMasterOrder = masterOrderInDb
                };

                _context.ChargingItemDetails.Add(newComment);
                _context.SaveChanges();

                var result = Mapper.Map<ChargingItemDetail, ChargingItemDetailDto>(_context.ChargingItemDetails.Include(x => x.FBAShipOrder).OrderByDescending(x => x.Id).First());

                await _logger.AddCreatedLogAsync<ChargingItemDetail>(null, result, "Added a new WO comment from warehouse", null, OperationLevel.Normal);

                return Created(Request.RequestUri + "/" + result.Id, result);
            }
            else
            {
                return Ok("Invaild operation.");
            }
        }

        //POST /api/fba/fbamasterorder/{id}
        [HttpPost]
        public IHttpActionResult CreateMasterOrder([FromBody]FBAMasterOrder obj, [FromUri]int id)
        {
            if (Checker.CheckString(obj.Container))
            {
                throw new Exception("Container number cannot contain space.");
            }

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

            masterOrder.AssembleFirstPart(obj.ETA, obj.Carrier, obj.Vessel, obj.Voy);
            masterOrder.AssembeSecondPart(obj.ETAPort, obj.PlaceOfReceipt, obj.PortOfLoading, obj.PortOfDischarge, obj.PlaceOfDelivery);
            masterOrder.AssembeThirdPart(obj.SealNumber, obj.ContainerSize, obj.Container);
            masterOrder.GrandNumber = grandNumber;
            masterOrder.Customer = customer;
            masterOrder.OriginalPlts = obj.OriginalPlts;
            masterOrder.InboundType = obj.InboundType;
            masterOrder.UnloadingType = obj.UnloadingType;
            masterOrder.StorageType = obj.StorageType;
            masterOrder.Palletizing = obj.Palletizing;
            masterOrder.CreatedBy = _userName;
            masterOrder.InvoiceStatus = "Await";
            masterOrder.UpdateLog += "Update by " + _userName + " at " + DateTime.Now.ToString() + ". ";
            masterOrder.Status = FBAStatus.NewCreated;
            masterOrder.Instruction = obj.Instruction;

            _context.FBAMasterOrders.Add(masterOrder);
            _context.SaveChanges();

            var resultDto = Mapper.Map<FBAMasterOrder, FBAMasterOrderDto>(_context.FBAMasterOrders.SingleOrDefault(x => x.GrandNumber == grandNumber));
            return Created(Request.RequestUri + "/" + resultDto.Id, resultDto);
        }

        // PUT /api/fba/fbamasterOrder/?masterOrderId={masterOrderId}&container={container}&inboundDate={inboundDate}
        [HttpPut]
        public void UpdateMasterOrderInfo([FromUri]int masterOrderId, [FromUri]string container, [FromUri]string inboundDate)
        {
            if (Checker.CheckString(container))
            {
                throw new Exception("Container number cannot contain space.");
            }

            var inboundDateTime = new DateTime();
            inboundDateTime = ParseStringToDateTime(inboundDateTime, inboundDate);

            var masterOrderInDb = _context.FBAMasterOrders.Include(x => x.FBAOrderDetails).SingleOrDefault(x => x.Id == masterOrderId);

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

        // PUT /api/fbamasterorder/?masterOrderId={masterOrderId}&operation={operation}
        [HttpPut]
        public void PushUnloadWorkOrder([FromUri]int masterOrderId, [FromUri]string operation)
        {
            var masterOrderInDb = _context.FBAMasterOrders
                .Include(x => x.FBAOrderDetails)
                .SingleOrDefault(x => x.Id == masterOrderId);

            if (operation == "Push")
            {
                if (masterOrderInDb.FBAOrderDetails == null)
                    throw new Exception("Cannot push orders with 0 SKUs");

                masterOrderInDb.PushTime = DateTime.Now;
                masterOrderInDb.UpdateLog = "Placed by " + _userName + " at " + DateTime.Now.ToString();
                masterOrderInDb.Status = FBAStatus.Incoming;
            }
            else if (operation == "Register")
            {
                masterOrderInDb.Status = FBAStatus.Registered;
                masterOrderInDb.UpdateLog = "Registed by " + _userName + " at " + DateTime.Now.ToString();
            }
            else if (operation == "Allocate")
            {
                var ctnsInPlts = 0;
                var ctnsOutPlts = 0;
                try
                {
                    ctnsInPlts = _context.FBAPalletLocations
                        .Include(x => x.FBAMasterOrder)
                        .Where(x => x.FBAMasterOrder.Id == masterOrderId)
                        .Sum(x => x.ActualQuantity);

                    ctnsOutPlts = _context.FBACartonLocations
                       .Include(x => x.FBAOrderDetail.FBAMasterOrder)
                       .Where(x => x.Status != "InPallet")
                       .Sum(x => x.ActualQuantity);
                }
                catch(Exception e)
                {
                    ctnsInPlts = 0;
                    ctnsOutPlts = 0;
                }

                if ((ctnsInPlts + ctnsOutPlts) == masterOrderInDb.FBAOrderDetails.Sum(x => x.ActualQuantity))
                {
                    masterOrderInDb.UpdateLog = "Allocated by " + _userName + " at " + DateTime.Now.ToString();
                    masterOrderInDb.Status = FBAStatus.Allocated;
                }
                else
                    throw new Exception("cannot mark an order as allocated before the goods are fully allocated.");
            }
            _context.SaveChanges();
        }

        // PUT /api/fba/fbamasterorder/?grandNumber={grandNumber}&operation={edit}
        [HttpPut]
        public void UpdateMasterOrderInfo([FromUri]string grandNumber, [FromBody]FBAMasterOrder obj)
        {
            if (Checker.CheckString(obj.Container))
            {
                throw new Exception("Container number cannot contain space.");
            }

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
            masterOrderInDb.UnloadingType = obj.UnloadingType;
            masterOrderInDb.StorageType = obj.StorageType;
            masterOrderInDb.Palletizing = obj.Palletizing;
            masterOrderInDb.ETAPort = obj.ETAPort;
            masterOrderInDb.PlaceOfReceipt = obj.PlaceOfReceipt;
            masterOrderInDb.PortOfLoading = obj.PortOfLoading;
            masterOrderInDb.PortOfDischarge = obj.PortOfDischarge;
            masterOrderInDb.PlaceOfDelivery = obj.PlaceOfDelivery;
            masterOrderInDb.Container = obj.Container;
            masterOrderInDb.OriginalPlts = obj.OriginalPlts;
            masterOrderInDb.SealNumber = obj.SealNumber;
            masterOrderInDb.InboundType = obj.InboundType;
            masterOrderInDb.ContainerSize = obj.ContainerSize;
            masterOrderInDb.Instruction = obj.Instruction;

            _context.SaveChanges();
        }

        // PUT /api/fba/fbamasterorder/?masterOrderId={masterOrderId}&status={status}
        [HttpPut]
        public void RollbackStatus([FromUri]int masterOrderId, [FromUri]string status)
        {
            var masterOrderInDb = _context.FBAMasterOrders.Find(masterOrderId);
            masterOrderInDb.Status = status;
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

    public class UnloadWorkOrder
    {
        public DateTime PlaceTime { get; set; }

        public DateTime UnloadFinishTime { get; set; }

        public DateTime UnloadStartTime { get; set; }

        public string ETA { get; set; }

        public string Container { get; set; }

        public string GrandNumber { get; set; }

        public string Carrier { get; set; }

        public string VerifiedBy { get; set; }

        public float LogonProgress { get; set; }

        public float RegisterProgress { get; set; }

        public float AllocationProgress { get; set; }

        public string DockNumber { get; set; }

        public DateTime InboundDate { get; set; }

        public DateTime OutTime { get; set; }

        public string IsDamaged { get; set; }

        public string UnloadingType { get; set; }

        public string StorageType { get; set; }

        public string Palletizing { get; set; }

        public ICollection<FBAOrderDetailDto> PackingList { get; set; }

        public ICollection<OperationInstruction> OperationInstructions { get; set; }

        public UnloadWorkOrder()
        {
            PackingList = new List<FBAOrderDetailDto>();

            OperationInstructions = new List<OperationInstruction>();
        }
    }
}
