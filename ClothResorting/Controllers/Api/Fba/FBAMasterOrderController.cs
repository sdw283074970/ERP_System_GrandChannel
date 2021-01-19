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
using ClothResorting.Helpers.FBAHelper;
using ClothResorting.Manager.NetSuit;
using ClothResorting.Manager.ZT;
using ClothResorting.Manager;

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBAMasterOrderController : ApiController
    {
        private ApplicationDbContext _context;
        private string _userName;
        private CustomerCallbackManager _customerCallbackManager;
        private Logger _logger;

        public FBAMasterOrderController()
        {
            _context = new ApplicationDbContext();
            _customerCallbackManager = new CustomerCallbackManager(_context);
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0] == "" ? (HttpContext.Current.Request.Headers.Get("AppUser") == null ? "" : HttpContext.Current.Request.Headers.Get("AppUser")) : HttpContext.Current.User.Identity.Name.Split('@')[0];
            _logger = new Logger(_context);
        }

        // GET /api/fba/fbamasterorder/
        [HttpGet]
        public IHttpActionResult GetAllMasterOrders([FromBody]Filter filter)
        {
            var getter = new FBAGetter();

            if (filter == null)
                return Ok(getter.GetAllMasterOrders());
            else
                return Ok(getter.GetFilteredMasterOrder(filter));
        }

        // GET /api/fbamasterorder/?sku={sku}&orderType={orderType}
        [HttpGet]
        public IHttpActionResult GetOrdersBySKUQuery([FromUri]string sku, [FromUri]string orderType)
        {
            if (orderType == FBAOrderType.MasterOrder)
            {
                var masterOrders = _context.FBAMasterOrders
                    .Include(x => x.FBAOrderDetails)
                    .Include(x => x.InvoiceDetails)
                    .Include(x => x.ChargingItemDetails)
                    .Include(x => x.FBAPallets)
                    .Where(x => x.FBAOrderDetails.Where(c => c.ShipmentId.Contains(sku) || c.AmzRefId.Contains(sku)).Any() || x.ChargingItemDetails.Where(c => c.Description.Contains(sku)).Any())
                    .ToList();
                //.Select(Mapper.Map<FBAMasterOrder, FBAMasterOrderDto>);

                var skuList = new List<int>();

                foreach (var m in masterOrders)
                {
                    m.TotalAmount = (float)m.InvoiceDetails.Sum(x => x.Amount);
                    m.TotalCost = (float)m.InvoiceDetails.Sum(x => x.Cost);
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
                    resultDto[i].Net = resultDto[i].TotalAmount - resultDto[i].TotalCost;
                }

                return Ok(resultDto);
            }
            else if (orderType == FBAOrderType.ShipOrder)
            {
                var shipOrders = _context.FBAShipOrders
                    .Include(x => x.InvoiceDetails)
                    .Include(x => x.FBAPickDetails)
                    .Include(x => x.ChargingItemDetails)
                    .Where(x => x.FBAPickDetails.Where(c => c.ShipmentId.Contains(sku) || c.AmzRefId.Contains(sku)).Any() || x.ChargingItemDetails.Where(c => c.Description.Contains(sku)).Any())
                    .ToList();

                foreach (var s in shipOrders)
                {
                    s.TotalAmount = (float)s.InvoiceDetails.Sum(x => x.Amount);
                    s.TotalCost = (float)s.InvoiceDetails.Sum(x => x.Cost);
                    s.TotalCtns = s.FBAPickDetails.Sum(x => x.ActualQuantity);
                    s.TotalPlts = s.FBAPickDetails.Sum(x => x.ActualPlts);
                    s.ETSTimeRange = s.ETS.ToString("yyyy-MM-dd") + " " + s.ETSTimeRange;
                }

                var dtos = Mapper.Map<IEnumerable<FBAShipOrder>, IEnumerable<FBAShipOrderDto>>(shipOrders);

                foreach(var d in dtos)
                {
                    d.Net = d.TotalAmount - d.TotalCost;
                }

                return Ok(dtos);
            }

            return Ok();
        }

        // GET /api/fba/fbamasterorder/{id}
        [HttpGet]
        public IHttpActionResult GetMasterOrdersByCustomerCode([FromUri]int id)
        {
            var masterOrders = _context.FBAMasterOrders
                .Include(x => x.InvoiceDetails)
                .Include(x => x.FBAOrderDetails)
                .Include(x => x.Customer)
                .Include(x => x.FBAPallets)
                .Where(x => x.Customer.Id == id)
                .ToList();

            var customerCode = _context.UpperVendors.Find(id).CustomerCode;

            var skuList = new List<int>();

            foreach (var m in masterOrders)
            {
                m.TotalAmount = (float)m.InvoiceDetails.Sum(x => x.Amount);
                m.TotalCBM = m.FBAOrderDetails.Sum(x => x.CBM);
                m.TotalCtns = m.FBAOrderDetails.Sum(x => x.Quantity);
                m.ActualCBM = m.FBAOrderDetails.Sum(x => x.ActualCBM);
                m.ActualCtns = m.FBAOrderDetails.Sum(x => x.ActualQuantity);
                m.ActualPlts = m.FBAPallets.Sum(x => x.ActualPallets);
                m.TotalCost = (float)m.InvoiceDetails.Sum(x => x.Cost);
                skuList.Add(m.FBAOrderDetails.GroupBy(x => x.ShipmentId).Count());
            }

            var resultDto = Mapper.Map<IList<FBAMasterOrder>, IList< FBAMasterOrderDto >>(masterOrders);

            for (int i = 0; i < masterOrders.Count; i++)
            {
                resultDto[i].SKUNumber = skuList[i];
                resultDto[i].Net = resultDto[i].TotalAmount - resultDto[i].TotalCost;
            }

            //var masterOrderDto = new MasterOrderDto
            //{
            //    CustomerCode = customerCode,
            //    FBAMasterOrderDtos = resultDto
            //};

            return Ok(resultDto);
        }

        // GET /api/fba/fbamasterorder/{id}
        [HttpGet]
        public IHttpActionResult GetMasterOrders([FromUri]string customerCode)
        {
            var masterOrders = _context.FBAMasterOrders
                .Include(x => x.InvoiceDetails)
                .Include(x => x.FBAOrderDetails)
                .Include(x => x.Customer)
                .Include(x => x.FBAPallets)
                .Where(x => x.CustomerCode == customerCode && x.Status != "Old Order")
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
                m.TotalCost = (float)m.InvoiceDetails.Sum(x => x.Cost);
                skuList.Add(m.FBAOrderDetails.GroupBy(x => x.ShipmentId).Count());
            }

            var resultDto = Mapper.Map<IList<FBAMasterOrder>, IList<FBAMasterOrderDto>>(masterOrders);

            for (int i = 0; i < masterOrders.Count; i++)
            {
                resultDto[i].SKUNumber = skuList[i];
                resultDto[i].Net = resultDto[i].TotalAmount - resultDto[i].TotalCost;
            }

            return Ok(resultDto);
        }

        // GET /api/fba/fbamasterOrder/?customerId={customerId}
        public IHttpActionResult GetCustomerCodeByCustomerId([FromUri]int customerId)
        {
            return Ok(_context.UpperVendors.Find(customerId).CustomerCode);
        }

        // GET /api/fbamasterorder/?masterOrderId={masterOrderId}&operation={operation}
        [HttpGet]
        public IHttpActionResult GetInfo([FromUri]int masterOrderId, [FromUri]string operation)
        {
            var masterOrderInDb = _context.FBAMasterOrders
                .Include(x => x.FBAOrderDetails.Select(c => c.FBACartonLocations))
                .Include(x => x.InvoiceDetails)
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

                var s = packingList.ToList();

                var chargingList = _context.ChargingItemDetails
                    .Include(x => x.FBAMasterOrder)
                    .Where(x => x.FBAMasterOrder.Id == masterOrderId);

                foreach(var c in chargingList)
                {
                    unloadWO.OperationInstructions.Add(new OperationInstruction {
                        Id = c.Id,
                        Description = c.Description,
                        Comment = c.Comment,
                        Result = c.Result,
                        HandlingStatus = c.HandlingStatus,
                        Status = c.Status,
                        IsOperation = c.IsOperation,
                        IsCharging = c.IsCharging,
                        IsInstruction = c.IsInstruction
                    });
                }

                unloadWO.PackingList = packingList.ToList();

                return Ok(unloadWO);
            }
            else if (operation == "Download")
            {
                return Ok(GenerateUnloadingWOAndPackingList(masterOrderId));
            }
            else if (operation == "Get")
            {
                return Ok(Mapper.Map<FBAMasterOrder, FBAMasterOrderDto>(masterOrderInDb));
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

        // POST /api/fbamasterorder/?masterOrderId={masterOrderId}
        [HttpPost]
        public IHttpActionResult UpdateMasterOrderById([FromUri]int masterOrderId, [FromBody]FBAMasterOrder obj)
        {
            obj.Container = obj.Container.Trim();

            var masterOrderInDb = _context.FBAMasterOrders
                .Include(x => x.FBAOrderDetails)
                .Include(x => x.InvoiceDetails)
                .Include(x => x.FBAPallets)
                .SingleOrDefault(x => x.Id == masterOrderId);

            var currentContainer = masterOrderInDb.Container;

            if (currentContainer != obj.Container && _context.FBAMasterOrders.SingleOrDefault(x => x.Container == obj.Container) != null)
            {
                throw new Exception("Contianer Number " + obj.Container + " has been taken. Please delete the existed order and try agian.");
            }

            masterOrderInDb.Carrier = obj.Carrier;
            masterOrderInDb.Vessel = obj.Vessel;
            masterOrderInDb.WarehouseLocation = obj.WarehouseLocation;
            masterOrderInDb.Voy = obj.Voy;
            masterOrderInDb.ETA = obj.ETA;
            masterOrderInDb.SubCustomer = obj.SubCustomer;
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

            return Ok(Mapper.Map<FBAMasterOrder, FBAMasterOrderDto>(masterOrderInDb));
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
            obj.Container = obj.Container.Trim();

            if (_context.FBAMasterOrders.SingleOrDefault(x => x.Container == obj.Container) != null)
            {
                throw new Exception("Contianer Number " + obj.Container + " has been taken. Please delete the existed order and try agian.");
            }

            var customer = _context.UpperVendors.SingleOrDefault(x => x.CustomerCode == obj.CustomerCode);
            var customerCode = customer.CustomerCode;
            //Unix时间戳加客户代码组成独一无二的GrandNumber
            var grandNumber = customerCode + ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000).ToString();

            if(_context.FBAMasterOrders.Where(x => x.GrandNumber == grandNumber).Count() > 0)
            {
                throw new Exception("Grand Number " + grandNumber + " has been taken. Please try agian.");
            }

            var chargingItemDetailList = new List<ChargingItemDetail>();

            var customerWOInstructions = _context.InstructionTemplates
                .Include(x => x.Customer)
                .Where(x => x.Customer.CustomerCode == obj.CustomerCode
                    && x.IsApplyToMasterOrder == true)
                .ToList();

            foreach (var c in customerWOInstructions)
            {
                chargingItemDetailList.Add(new ChargingItemDetail
                {
                    Status = c.Status,
                    HandlingStatus = c.IsOperation || c.IsInstruction ? FBAStatus.New : FBAStatus.Na,
                    Description = c.Description,
                    CreateBy = _userName,
                    CreateDate = DateTime.Now,
                    IsCharging = c.IsCharging,
                    IsInstruction = c.IsInstruction,
                    IsOperation = c.IsOperation
                });
            }

            _context.ChargingItemDetails.AddRange(chargingItemDetailList);

            var masterOrder = new FBAMasterOrder();

            masterOrder.AssembleFirstPart(obj.ETA, obj.Carrier, obj.Vessel, obj.Voy);
            masterOrder.AssembeSecondPart(obj.ETAPort, obj.PlaceOfReceipt, obj.PortOfLoading, obj.PortOfDischarge, obj.PlaceOfDelivery);
            masterOrder.AssembeThirdPart(obj.SealNumber, obj.ContainerSize, obj.Container);
            masterOrder.GrandNumber = grandNumber;
            masterOrder.WarehouseLocation = obj.WarehouseLocation;
            masterOrder.Customer = customer;
            masterOrder.CustomerCode = customerCode;
            masterOrder.SubCustomer = obj.SubCustomer;
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
            masterOrder.ChargingItemDetails = chargingItemDetailList;

            if (obj.InvoiceStatus == "Closed")
            {
                masterOrder.CloseDate = DateTime.Now;
                masterOrder.ConfirmedBy = _userName;
            }

            _context.FBAMasterOrders.Add(masterOrder);
            _context.SaveChanges();

            var resultDto = Mapper.Map<FBAMasterOrder, FBAMasterOrderDto>(_context.FBAMasterOrders.SingleOrDefault(x => x.GrandNumber == grandNumber));
            return Created(Request.RequestUri + "/" + resultDto.Id, resultDto);
        }

        // PUT /api/fba/fbamasterOrder/?masterOrderId={masterOrderId}&container={container}&inboundDate={inboundDate}
        [HttpPut]
        public void UpdateMasterOrderInfo([FromUri]int masterOrderId, [FromUri]string container, [FromUri]string inboundDate)
        {
            container = container.Trim();

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

        // PUT /api/fbamasterorder/?masterOrderId={masterOrderId}&operationDate={operationDate}&operation={operation}
        [HttpPut]
        public void OperateWorkOrderWithDate([FromUri]int masterOrderId, [FromUri]DateTime operationDate, [FromUri]string operation)
        {
            var masterOrderInDb = _context.FBAMasterOrders
                .Include(x => x.FBAOrderDetails)
                .SingleOrDefault(x => x.Id == masterOrderId);

            if (operation == "Push")
            {
                if (masterOrderInDb.FBAOrderDetails == null)
                    throw new Exception("Cannot push orders with 0 SKUs");

                masterOrderInDb.PushTime = operationDate;
                masterOrderInDb.UpdateLog = "Placed by " + _userName + " at " + DateTime.Now.ToString();
                masterOrderInDb.Status = FBAStatus.Incoming;
            }
            else if (operation == "Callback")
            {
                masterOrderInDb.Status = FBAStatus.NewCreated;
                masterOrderInDb.UpdateLog = "Callback by " + _userName + " at " + DateTime.Now.ToString();
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
                }
                catch(Exception)
                {
                    ctnsInPlts = 0;
                }

                try 
                {
                    var t = _context.FBACartonLocations
                       .Include(x => x.FBAOrderDetail.FBAMasterOrder)
                       .Where(x => x.Status != "InPallet"
                        && x.FBAOrderDetail.FBAMasterOrder.Id == masterOrderId).ToList();

                    ctnsOutPlts = _context.FBACartonLocations
                       .Include(x => x.FBAOrderDetail.FBAMasterOrder)
                       .Where(x => x.Status != "InPallet"
                        && x.FBAOrderDetail.FBAMasterOrder.Id == masterOrderId)
                       .Sum(x => x.ActualQuantity);
                }
                catch(Exception e)
                {
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
            else if (operation == "Reverse")
            {
                switch(masterOrderInDb.Status)
                {
                    case FBAStatus.Registered:
                        masterOrderInDb.Status = FBAStatus.Received;
                        break;
                    case FBAStatus.Received:
                        masterOrderInDb.Status = FBAStatus.Processing;
                        break;
                    case FBAStatus.Arrived:
                        masterOrderInDb.InboundDate = new DateTime(1900, 1, 1);
                        masterOrderInDb.Status = FBAStatus.Incoming;
                        break;
                    default:
                        masterOrderInDb.Status = FBAStatus.Arrived;
                        break;
                }
            }
            else if (operation == "Submit")
            {
                if (masterOrderInDb.Status == FBAStatus.NewCreated)
                    masterOrderInDb.Status = FBAStatus.Draft;
                else
                    throw new Exception("Cannot submit a order that status is not new created.");
            }
            else if (operation == "Reset")
            {
                masterOrderInDb.Status = FBAStatus.Arrived;
            }
            else if (operation == "Start")
            {
                masterOrderInDb.Status = FBAStatus.Processing;
                masterOrderInDb.UnloadStartTime = DateTime.Now;
            }
            else if (operation == "Finish Allocating")
            {
                masterOrderInDb.Status = FBAStatus.Allocated;
                masterOrderInDb.UpdateLog = "Allocated by " + _userName;
            }
            else if (operation == "Finish Palletizing")
            {
                masterOrderInDb.Status = FBAStatus.Registered;
                masterOrderInDb.UpdateLog = "Palletized by " + _userName;
            }
            else if (operation == "Confirmed")
            {
                masterOrderInDb.Status = FBAStatus.Confirmed;
                masterOrderInDb.UpdateLog = "Order complete. Confirmed by " + _userName;
            }

            _context.SaveChanges();
        }

        // PUT /api/fbamasterorder/?masterOrderId={masterOrderId}&operationDate={operationDate}&operation={operation}
        [HttpPut]
        public void OperateWorkOrder([FromUri]int masterOrderId, [FromUri]string operation)
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
            else if (operation == "Callback")
            {
                masterOrderInDb.Status = FBAStatus.NewCreated;
                masterOrderInDb.UpdateLog = "Callback by " + _userName + " at " + DateTime.Now.ToString();
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
                }
                catch (Exception)
                {
                    ctnsInPlts = 0;
                }

                try
                {
                    var t = _context.FBACartonLocations
                       .Include(x => x.FBAOrderDetail.FBAMasterOrder)
                       .Where(x => x.Status != "InPallet"
                        && x.FBAOrderDetail.FBAMasterOrder.Id == masterOrderId).ToList();

                    ctnsOutPlts = _context.FBACartonLocations
                       .Include(x => x.FBAOrderDetail.FBAMasterOrder)
                       .Where(x => x.Status != "InPallet"
                        && x.FBAOrderDetail.FBAMasterOrder.Id == masterOrderId)
                       .Sum(x => x.ActualQuantity);
                }
                catch (Exception e)
                {
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
            else if (operation == "Reverse")
            {
                switch (masterOrderInDb.Status)
                {
                    case FBAStatus.Registered:
                        masterOrderInDb.Status = FBAStatus.Received;
                        break;
                    case FBAStatus.Received:
                        masterOrderInDb.Status = FBAStatus.Processing;
                        break;
                    case FBAStatus.Arrived:
                        masterOrderInDb.InboundDate = new DateTime(1900, 1, 1);
                        masterOrderInDb.Status = FBAStatus.Incoming;
                        break;
                    default:
                        masterOrderInDb.Status = FBAStatus.Arrived;
                        break;
                }
            }
            else if (operation == "Submit")
            {
                if (masterOrderInDb.Status == FBAStatus.NewCreated)
                    masterOrderInDb.Status = FBAStatus.Draft;
                else
                    throw new Exception("Cannot submit a order that status is not new created.");
            }
            else if (operation == "Reset")
            {
                masterOrderInDb.Status = FBAStatus.Arrived;
            }
            else if (operation == "Start")
            {
                masterOrderInDb.Status = FBAStatus.Processing;
                masterOrderInDb.UnloadStartTime = DateTime.Now;
            }
            else if (operation == "Finish Allocating")
            {
                masterOrderInDb.Status = FBAStatus.Allocated;
                masterOrderInDb.UpdateLog = "Allocated by " + _userName;
            }
            else if (operation == "Finish Palletizing")
            {
                masterOrderInDb.Status = FBAStatus.Registered;
                masterOrderInDb.UpdateLog = "Palletized by " + _userName;
            }
            else if (operation == "Confirmed")
            {
                // 客户定制反馈接口
                _customerCallbackManager.CallBackWhenInboundOrderCompleted(masterOrderInDb);

                masterOrderInDb.Status = FBAStatus.Confirmed;
                masterOrderInDb.UpdateLog = "Order complete. Confirmed by " + _userName;
            }

            _context.SaveChanges();
        }

        // PUT /api/fba/fbamasterorder/?grandNumber={grandNumber}&operation={edit}
        [HttpPut]
        public void UpdateMasterOrder([FromUri]string grandNumber, [FromBody]FBAMasterOrder obj)
        {
            obj.Container = obj.Container.Trim();

            var masterOrderInDb = _context.FBAMasterOrders
                .SingleOrDefault(x => x.GrandNumber == grandNumber);

            var currentContainer = masterOrderInDb.Container;

            if (currentContainer != obj.Container && _context.FBAMasterOrders.SingleOrDefault(x => x.Container == obj.Container) != null)
            {
                throw new Exception("Contianer Number " + obj.Container + " has been taken. Please delete the existed order and try agian.");
            }

            masterOrderInDb.Carrier = obj.Carrier;
            masterOrderInDb.WarehouseLocation = obj.WarehouseLocation;
            masterOrderInDb.Vessel = obj.Vessel;
            masterOrderInDb.Voy = obj.Voy;
            masterOrderInDb.ETA = obj.ETA;
            masterOrderInDb.SubCustomer = obj.SubCustomer;
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

            var eFilesInDb = _context.EFiles
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.FBAMasterOrder.Id == masterOrderId);

            _context.EFiles.RemoveRange(eFilesInDb);

            var masterOrderInDb = _context.FBAMasterOrders.Find(masterOrderId);

            _context.FBAMasterOrders.Remove(masterOrderInDb);

            try
            {
                _context.SaveChanges();

            }
            catch (Exception e)
            {
                throw new Exception("Cannot delete this master order. Please put back or delete related picked items first.");
            }
        }

        public void DeleteMasterOrderById(int masterOrderId)
        {
            var invoiceDetails = _context.InvoiceDetails
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.FBAMasterOrder.Id == masterOrderId);

            _context.InvoiceDetails.RemoveRange(invoiceDetails);

            var chargingItemDetails = _context.ChargingItemDetails
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.FBAMasterOrder.Id == masterOrderId);

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

            var eFilesInDb = _context.EFiles
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.FBAMasterOrder.Id == masterOrderId);

            _context.EFiles.RemoveRange(eFilesInDb);

            var masterOrderInDb = _context.FBAMasterOrders.Find(masterOrderId);

            _context.FBAMasterOrders.Remove(masterOrderInDb);

            try
            {
                _context.SaveChanges();

            }
            catch (Exception e)
            {
                throw new Exception("Cannot delete this master order. Please put back or delete related picked items first.");
            }
        }

        private DateTime ParseStringToDateTime(DateTime dateTime, string stringTime)
        {
            DateTime.TryParseExact(stringTime, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
            return dateTime;
        }

        private string GenerateUnloadingWOAndPackingList(int masterOrderId)
        {
            var generator = new FBAExcelGenerator(@"D:\Template\UnloadingReport-Template.xlsx");
            var path = generator.GenerateUnloadingWOAndPackingList(masterOrderId);

            return path;
        }
    }

    public class MasterOrderDto
    {
        public string CustomerCode { get; set; }

        public IList<FBAMasterOrderDto> FBAMasterOrderDtos { get; set; }
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
