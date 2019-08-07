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
using ClothResorting.Models.FBAModels.StaticModels;
using ClothResorting.Helpers.FBAHelper;
using ClothResorting.Models.StaticClass;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ClothResorting.Dtos;

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBAShipOrderController : ApiController
    {
        private ApplicationDbContext _context;
        private string _userName;
        private Logger _logger;

        public FBAShipOrderController()
        {
            _context = new ApplicationDbContext();
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0];
            _logger = new Logger(_context);
        }

        //GET /api/fba/fbashiporder/
        [HttpGet]
        public IHttpActionResult GetAllFBAShipOrder()
        {
            var shipOrders = _context.FBAShipOrders
                .Include(x => x.InvoiceDetails)
                .Include(x => x.FBAPickDetails)
                .ToList();

            foreach(var s in shipOrders)
            {
                s.TotalAmount = (float)s.InvoiceDetails.Sum(x => x.Amount);
                s.TotalCtns = s.FBAPickDetails.Sum(x => x.ActualQuantity);
                s.TotalPlts = s.FBAPickDetails.Sum(x => x.ActualPlts);
                s.ETSTimeRange = s.ETS.ToString("yyyy-MM-dd") + " " + s.ETSTimeRange;
            }

            return Ok(Mapper.Map<IEnumerable<FBAShipOrder>, IEnumerable<FBAShipOrderDto>>(shipOrders));
        }

        // GET /api/fba/fbashiporder/?customerId={customerId}
        [HttpGet]
        public IHttpActionResult GetFBAShipOrderByCustomer([FromUri]int customerId)
        {
            var customerCode = _context.UpperVendors.Find(customerId).CustomerCode;

            var shipOrders = _context.FBAShipOrders
                .Include(x => x.InvoiceDetails)
                .Where(x => x.CustomerCode == customerCode)
                .ToList();

            foreach (var s in shipOrders)
            {
                s.TotalAmount = (float)s.InvoiceDetails.Sum(x => x.Amount);
            }

            return Ok(Mapper.Map<IEnumerable<FBAShipOrder>, IEnumerable<FBAShipOrderDto>>(shipOrders));
        }

        // GET /api/fba/fbashiporder/?shipOrderId={shipOrderId}&operation={operation}
        [HttpGet]
        public IHttpActionResult GetShipOrderInfo([FromUri]int shipOrderId, [FromUri]string operation)
        {
            if (operation == "BOL")
            {
                var pickDetailsInDb = _context.FBAPickDetails
                    .Include(x => x.FBAShipOrder)
                    .Include(x => x.FBAPickDetailCartons)
                    .Include(x => x.FBAPalletLocation.FBAPallet.FBACartonLocations)
                    .Where(x => x.FBAShipOrder.Id == shipOrderId)
                    .ToList();

                var bolList = GenerateFBABOLList(pickDetailsInDb);

                var generator = new FBAExcelGenerator(@"D:\Template\BOL-Template.xlsx");

                var fileName = generator.GenerateExcelBol(shipOrderId, bolList);

                return Ok(fileName);
            }
            else if (operation == "Update")
            {
                return Ok(Mapper.Map<FBAShipOrder, FBAShipOrderDto>(_context.FBAShipOrders.Find(shipOrderId)));
            }
            else if (operation == "WO")
            {
                var shipOrder = _context.FBAShipOrders
                    .Include(x => x.FBAPickDetails)
                    .Include(x => x.ChargingItemDetails)
                    .SingleOrDefault(x => x.Id == shipOrderId);

                return Ok(GenerateWorkOrder(shipOrder));
            }

            return Ok();
        }

        // GET /api/fba/shiporder/?chargingDetailId={chargingDetailId}
        [HttpGet]
        public IHttpActionResult GetChargingDetail([FromUri]int chargingDetailId)
        {
            var chargingDetailInDb = _context.ChargingItemDetails.Find(chargingDetailId);

            return Ok(Mapper.Map<ChargingItemDetail, ChargingItemDetailDto>(chargingDetailInDb));
        }

        // POST /api/fba/fbashiporder/
        [HttpPost]
        public async Task<IHttpActionResult> CreateNewShipOrder([FromBody]ShipOrderDto obj)
        {
            if (Checker.CheckString(obj.ShipOrderNumber))
            {
                throw new Exception("Container number cannot contain space.");
            }

            if (_context.FBAShipOrders.SingleOrDefault(x => x.ShipOrderNumber == obj.ShipOrderNumber) != null)
            {
                throw new Exception("Ship Order Number " + obj.ShipOrderNumber + " has been taken. Please delete the existed order and try agian.");
            }

            var shipOrder = new FBAShipOrder();
            var chargingItemDetailList = new List<ChargingItemDetail>();

            var customerWOInstructions = _context.UpperVendors
                .Include(x => x.InstructionTemplates)
                .SingleOrDefault(x => x.CustomerCode == obj.CustomerCode)
                .InstructionTemplates
                .ToList();

            foreach(var c in customerWOInstructions)
            {
                chargingItemDetailList.Add(new ChargingItemDetail {
                    Status = c.Status,
                    HandlingStatus = FBAStatus.New,
                    Description = c.Description,
                    CreateBy = _userName,
                    CreateDate = DateTime.Now
                });
            }

            _context.ChargingItemDetails.AddRange(chargingItemDetailList);

            shipOrder.AssembleBaseInfo(obj.ShipOrderNumber, obj.CustomerCode, obj.OrderType, obj.Destination, obj.PickReference);
            shipOrder.CreateBy = _userName;
            shipOrder.CreateDate = DateTime.Now;
            shipOrder.BOLNumber = obj.BOLNumber;
            shipOrder.Carrier = obj.Carrier;
            shipOrder.ETS = obj.ETS;
            shipOrder.ETSTimeRange = obj.TimeRange;
            shipOrder.PickNumber = obj.PickNumber;
            shipOrder.PurchaseOrderNumber = obj.PurchaseOrderNumber;
            shipOrder.Instruction = obj.Instruction;
            shipOrder.ChargingItemDetails = chargingItemDetailList;

            _context.FBAShipOrders.Add(shipOrder);
            _context.SaveChanges();

            var sampleDto = Mapper.Map<FBAShipOrder, FBAShipOrderDto>(_context.FBAShipOrders.OrderByDescending(x => x.Id).First());

            await _logger.AddCreatedLogAsync<FBAShipOrder>(null, sampleDto, "Created a new FBA ship order", null, OperationLevel.Normal);

            return Created(Request.RequestUri + "/" + sampleDto.Id, sampleDto);
        }

        // POST /api/fba/fbashiporder/?shipOrderId={shipOrderId}&comment={comment}&operation={operation}
        [HttpPost]
        public async Task<IHttpActionResult> CreateNewCommentFromWarehouse([FromUri]int shipOrderId, [FromUri]string comment, [FromUri]string operation)
        {
            if (operation == "AddNewComment")
            {
                var shipOrderInDb = _context.FBAShipOrders.Find(shipOrderId);
                shipOrderInDb.Status = FBAStatus.Pending;
                var newComment = new ChargingItemDetail
                {
                    CreateBy = _userName,
                    Comment = comment,
                    CreateDate = DateTime.Now,
                    Description = "Extral comment from warehouse",
                    HandlingStatus = FBAStatus.Pending,
                    Status = FBAStatus.Unhandled,
                    FBAShipOrder = shipOrderInDb
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

        // PUT /api/fba/fbashiporder/?shipOrderId={shipOrderId}
        [HttpPut]
        public async Task UpdateShipOrder([FromUri]int shipOrderId, [FromBody]ShipOrderDto obj)
        {
            if (Checker.CheckString(obj.ShipOrderNumber))
            {
                throw new Exception("Container number cannot contain space.");
            }

            var shipOrderInDb = _context.FBAShipOrders.Find(shipOrderId);
            var oldValueDto = Mapper.Map<FBAShipOrder, FBAShipOrderDto>(shipOrderInDb);
            //如果更新的运单号不是之前的运单号且在数据库中有重复，则报错
            if (obj.ShipOrderNumber != shipOrderInDb.ShipOrderNumber && _context.FBAShipOrders.SingleOrDefault(x => x.ShipOrderNumber == obj.ShipOrderNumber) != null)
            {
                throw new Exception("Ship Order Number " + obj.ShipOrderNumber + " has been taken. Please delete the existed order and try agian.");
            }

            shipOrderInDb.ShipOrderNumber = obj.ShipOrderNumber;
            shipOrderInDb.CustomerCode = obj.CustomerCode;
            shipOrderInDb.OrderType = obj.OrderType;
            shipOrderInDb.Destination = obj.Destination;
            shipOrderInDb.PickReference = obj.PickReference;
            shipOrderInDb.Carrier = obj.Carrier;
            shipOrderInDb.BOLNumber = obj.BOLNumber;
            shipOrderInDb.ETS = obj.ETS;
            shipOrderInDb.ETSTimeRange = obj.TimeRange;
            shipOrderInDb.PickNumber = obj.PickNumber;
            shipOrderInDb.PurchaseOrderNumber = obj.PurchaseOrderNumber;
            shipOrderInDb.EditBy = _userName;
            shipOrderInDb.Instruction = obj.Instruction;

            var resultDto = Mapper.Map<FBAShipOrder, FBAShipOrderDto>(shipOrderInDb);

            await _logger.AddUpdatedLogAndSaveChangesAsync<FBAShipOrder>(oldValueDto, resultDto, "Updated some basic ship order info", null, OperationLevel.Mediunm);
        }
            
        // PUT /api/fba/fbashiporder/?shipOrderId={shipOrderId}&operationDate={operationDate}&operation={operation}
        [HttpPut]
        public async Task ChangeShipOrderStatus([FromUri]int shipOrderId, [FromUri]DateTime operationDate, [FromUri]string operation)
        {
            var shipOrderInDb = _context.FBAShipOrders
                .Include(x => x.FBAPickDetails)
                .Include(x => x.ChargingItemDetails)
                .SingleOrDefault(x => x.Id == shipOrderId);

            var oldStatus = shipOrderInDb.Status;
            var oldValueDto = Mapper.Map<FBAShipOrder, FBAShipOrderDto>(shipOrderInDb);

            ChangeStatus(shipOrderInDb, operation, operationDate);

            var description = "Change ship order status from " + oldStatus + " to " + shipOrderInDb.Status;

            var resultDto = Mapper.Map<FBAShipOrder, FBAShipOrderDto>(shipOrderInDb);

            await _logger.AddUpdatedLogAndSaveChangesAsync<FBAShipOrder>(oldValueDto, resultDto, description, null, OperationLevel.Mediunm);
        }

        // PUT /api/fba/fbashiporder/?shipOrderId={shipOrderId}&shipDate={shipDate}
        [HttpPut]
        public async Task MarkShipTime([FromUri]int shipOrderId, [FromUri]DateTime operationDate)
        {
            var shipOrderInDb = _context.FBAShipOrders.Find(shipOrderId);
            var oldStatus = shipOrderInDb.Status;
            var oldValueDto = Mapper.Map<FBAShipOrder, FBAShipOrderDto>(shipOrderInDb);

            ////如果已经发货了，则报错
            //if (shipOrderInDb.ShipDate.ToString("yyyy-MM-dd") != "1900-01-01")
            //{
            //    throw new Exception("Cannot override existed date.");
            //}

            //当订单状态为picking时强行release并印上日期
            if (shipOrderInDb.Status == FBAStatus.NewCreated)
            {
                shipOrderInDb.ShipDate = operationDate;
                shipOrderInDb.Status = FBAStatus.Released;
                shipOrderInDb.ReleasedBy = _userName;
            }

            var description = "Change ship order status from " + oldStatus + " to " + shipOrderInDb.Status;

            var resultDto = Mapper.Map<FBAShipOrder, FBAShipOrderDto>(shipOrderInDb);
            await _logger.AddUpdatedLogAndSaveChangesAsync<FBAShipOrder>(oldValueDto, resultDto, description, null, OperationLevel.Mediunm);
        }

        // PUT /api/fba/fbashiporder/?shipOrderId={shipOrderId}&isRelease={isRelease}
        [HttpPut]
        public async Task MarkPickingToRelease([FromUri]int shipOrderId, [FromUri]bool isRelease)
        {
            var shipOrderInDb = _context.FBAShipOrders.Find(shipOrderId);
            var oldStatus = shipOrderInDb.Status;
            var oldValueDto = Mapper.Map<FBAShipOrder, FBAShipOrderDto>(shipOrderInDb);

            //当订单状态为picking时强行release并印上日期
            if (shipOrderInDb.Status == FBAStatus.Picking)
            {
                shipOrderInDb.Status = FBAStatus.Released;
                shipOrderInDb.ReleasedDate = DateTime.Now;
                shipOrderInDb.ReleasedBy = _userName;
                shipOrderInDb.OperationLog = "Released by " + _userName;
            }

            var description = "Change ship order status from " + oldStatus + " to " + shipOrderInDb.Status;

            var resultDto = Mapper.Map<FBAShipOrder, FBAShipOrderDto>(shipOrderInDb);
            await _logger.AddUpdatedLogAndSaveChangesAsync<FBAShipOrder>(oldValueDto, resultDto, description, null, OperationLevel.Mediunm);
        }

        // PUT /api/fba/fbashiporder/?referenceId={referenceId}&orderType={orderType}}&operation={operation}
        [HttpPut]
        public async Task ResetWorkOrderInstruction([FromUri]int referenceId, [FromUri]string orderType, [FromUri]string operation)
        {
            if (operation == "Reset")
            {
                var instructionTemplates = new List<InstructionTemplate>();

                if (orderType == FBAOrderType.ShipOrder)
                {
                    var referenceInDb = _context.FBAShipOrders
                        .Include(x => x.ChargingItemDetails)
                        .SingleOrDefault(x => x.Id == referenceId);

                    var oldValueDto = Mapper.Map<IEnumerable<ChargingItemDetail>, IEnumerable<ChargingItemDetailDto>>(referenceInDb.ChargingItemDetails);

                    referenceInDb.ChargingItemDetails = null;

                    var chargingItemDetailList = new List<ChargingItemDetail>();

                    instructionTemplates = _context.InstructionTemplates
                        .Include(x => x.Customer)
                        .Where(x => x.Customer.CustomerCode == referenceInDb.CustomerCode
                            && x.IsApplyToShipOrder == true)
                        .ToList();

                    foreach (var i in instructionTemplates)
                    {
                        chargingItemDetailList.Add(new ChargingItemDetail
                        {
                            Status = i.Status,
                            CreateBy = _userName,
                            Description = i.Description,
                            CreateDate = DateTime.Now,
                            HandlingStatus = FBAStatus.New,
                            FBAShipOrder = referenceInDb
                        });
                    }

                    _context.ChargingItemDetails.AddRange(chargingItemDetailList);
                    _context.SaveChanges();

                    var resultDto = Mapper.Map<IEnumerable<ChargingItemDetail>, IEnumerable<ChargingItemDetailDto>>(_context.ChargingItemDetails.OrderByDescending(x => x.Id).Take(chargingItemDetailList.Count));
                    var shipOrderStr = JsonConvert.SerializeObject(Mapper.Map<FBAShipOrder, FBAShipOrderDto>(referenceInDb));
                    await _logger.AddUpdatedLogAndSaveChangesAsync<ChargingItemDetail>(oldValueDto, resultDto, "Reset all instructions in [dbo].[FBAShipOrder] " + shipOrderStr, null, OperationLevel.Mediunm);
                }
                else if (orderType == FBAOrderType.MasterOrder)
                {
                    var referenceInDb = _context.FBAMasterOrders
                        .Include(x => x.Customer)
                        .Include(x => x.ChargingItemDetails)
                        .SingleOrDefault(x => x.Id == referenceId);

                    var oldValueDto = Mapper.Map<IEnumerable<ChargingItemDetail>, IEnumerable<ChargingItemDetailDto>>(referenceInDb.ChargingItemDetails);

                    referenceInDb.ChargingItemDetails = null;

                    var chargingItemDetailList = new List<ChargingItemDetail>();

                    instructionTemplates = _context.InstructionTemplates
                        .Include(x => x.Customer)
                        .Where(x => x.Customer.CustomerCode == referenceInDb.Customer.CustomerCode
                            && x.IsApplyToMasterOrder == true)
                        .ToList();

                    foreach (var i in instructionTemplates)
                    {
                        chargingItemDetailList.Add(new ChargingItemDetail
                        {
                            Status = i.Status,
                            CreateBy = _userName,
                            Description = i.Description,
                            CreateDate = DateTime.Now,
                            HandlingStatus = FBAStatus.New,
                            FBAMasterOrder = referenceInDb
                        });
                    }

                    _context.ChargingItemDetails.AddRange(chargingItemDetailList);
                    _context.SaveChanges();

                    var resultDto = Mapper.Map<IEnumerable<ChargingItemDetail>, IEnumerable<ChargingItemDetailDto>>(_context.ChargingItemDetails.OrderByDescending(x => x.Id).Take(chargingItemDetailList.Count));
                    var masterOrderStr = JsonConvert.SerializeObject(Mapper.Map<FBAMasterOrder, FBAMasterOrderDto>(referenceInDb));
                    await _logger.AddUpdatedLogAndSaveChangesAsync<ChargingItemDetail>(oldValueDto, resultDto, "Reset all instructions in [dbo].[FBAMasterOrder] " + masterOrderStr, null, OperationLevel.Mediunm);
                }
            }
        }

        // PUT /api/fba/fbashiporder/?chargingDetailId={chargingDetailId}&comment={comment}&operation={operation}
        [HttpPut]
        public async Task UpdateInstruction([FromUri]int chargingDetailId, [FromUri]string comment, [FromUri]bool isChargingItem, [FromUri]string operation)
        {
            var instructionInDb = _context.ChargingItemDetails
                .Include(x => x.FBAMasterOrder)
                .Include(x => x.FBAShipOrder)
                .SingleOrDefault(x => x.Id == chargingDetailId);

            var oldValueDto = Mapper.Map<ChargingItemDetail, ChargingItemDetailDto>(instructionInDb);
            string description = "";

            if (operation == "UpdateInstruction")
            {
                instructionInDb.Description = comment;
                instructionInDb.HandlingStatus = FBAStatus.New;
                if (instructionInDb.FBAMasterOrder != null && instructionInDb.FBAMasterOrder.Status == FBAStatus.Pending)
                    instructionInDb.FBAMasterOrder.Status = FBAStatus.Updated;
                else if (instructionInDb.FBAShipOrder != null && instructionInDb.FBAShipOrder.Status == FBAStatus.Pending)
                    instructionInDb.FBAShipOrder.Status = FBAStatus.Updated;

                if (isChargingItem)
                {
                    instructionInDb.Status = FBAStatus.WaitingForCharging;
                }
                else
                {
                    instructionInDb.Status = FBAStatus.NoNeedForCharging;
                }

                description = "Updated instruction by office client";
            }
            else if (operation == "UpdateComment")
            {
                instructionInDb.Comment = comment;
                instructionInDb.HandlingStatus = FBAStatus.Pending;
                instructionInDb.ConfirmedBy = _userName;

                description = "Updated comment by warehouse client";

                if (instructionInDb.FBAShipOrder != null)
                    instructionInDb.FBAShipOrder.Status = FBAStatus.Pending;
                else
                    instructionInDb.FBAMasterOrder.Status = FBAStatus.Pending;
            }
            else if (operation == "UpdateResult")
            {
                instructionInDb.Result = comment;
                instructionInDb.HandlingStatus = FBAStatus.Updated;

                if (instructionInDb.FBAShipOrder != null)
                    instructionInDb.FBAShipOrder.Status = FBAStatus.Updated;
                else
                    instructionInDb.FBAMasterOrder.Status = FBAStatus.Updated;

                description = "Updated result by office client";
            }

            var instructionDto = Mapper.Map<ChargingItemDetail, ChargingItemDetailDto>(instructionInDb);

            await _logger.AddUpdatedLogAndSaveChangesAsync<ChargingItemDetail>(oldValueDto, instructionDto, description, null, OperationLevel.Mediunm);
        }

        // DELETE /api/fba/fbashiporder/?shipOrderId={shipOrderId}
        [HttpDelete]
        public async Task DeleteShipOrder([FromUri]int shipOrderId)
        {
            var pickDetailsInDb = _context.FBAPickDetails
                .Include(x => x.FBAShipOrder)
                .Where(x => x.FBAShipOrder.Id == shipOrderId);

            var pickDetailsDto = Mapper.Map<IEnumerable<FBAPickDetail>, IEnumerable<FBAPickDetailsDto>>(pickDetailsInDb);

            var shipOrderInDb = _context.FBAShipOrders.Find(shipOrderId);
            var shipOrderDto = Mapper.Map<FBAShipOrder, FBAShipOrderDto>(shipOrderInDb);

            var fbaPickDetailAPI = new FBAPickDetailController();

            foreach(var detail in pickDetailsInDb)
            {
                fbaPickDetailAPI.RemovePickDetail(_context, detail.Id);
            }

            var chargingItemDetailsInDb = _context.ChargingItemDetails
                .Include(x => x.FBAShipOrder)
                .Where(x => x.FBAShipOrder.Id == shipOrderId);

            var chargingItemDetailsDto = Mapper.Map<IEnumerable<ChargingItemDetail>, IEnumerable<ChargingItemDetailDto>>(chargingItemDetailsInDb);

            var invoiceDetailsInDb = _context.InvoiceDetails
                .Include(x => x.FBAShipOrder)
                .Where(x => x.FBAShipOrder.Id == shipOrderId);

            var invoiceDetailsDto = Mapper.Map<IEnumerable<InvoiceDetail>, IEnumerable<InvoiceDetailDto>>(invoiceDetailsInDb);

            _context.ChargingItemDetails.RemoveRange(chargingItemDetailsInDb);
            _context.InvoiceDetails.RemoveRange(invoiceDetailsInDb);
            _context.FBAShipOrders.Remove(shipOrderInDb);
            try
            {
                _context.SaveChanges();
            }
            catch(Exception e)
            {
                throw new Exception("Please remove all diagnostics before deleting this ship order.");
                //await _logger.AddDeletedLogAsync<FBAShipOrder>(shipOrderDto, "Attemp to delete ship order failed", e.Message, OperationLevel.Exception);
            }

            await _logger.AddDeletedLogAsync<FBAPickDetail>(pickDetailsDto, "Put back picked items", null, OperationLevel.Mediunm);
            await _logger.AddDeletedLogAsync<ChargingItemDetail>(chargingItemDetailsDto, "Deleted all WO instructions and charging instructions", null, OperationLevel.Mediunm);
            await _logger.AddDeletedLogAsync<InvoiceDetail>(invoiceDetailsDto, "Deleted all invoiceDetails", null, OperationLevel.High);
            await _logger.AddDeletedLogAsync<FBAShipOrder>(shipOrderDto, "Deleted ship order", null, OperationLevel.High);

            _context.SaveChanges();
        }

        private void ShipPickDetail(ApplicationDbContext context, int pickDetailId)
        {
            var pickDetailInDb = context.FBAPickDetails
                .Include(x => x.FBAPalletLocation.FBAPallet.FBACartonLocations)
                .Include(x => x.FBACartonLocation)
                .Include(x => x.FBAPickDetailCartons)
                .SingleOrDefault(x => x.Id == pickDetailId);

            if (pickDetailInDb.FBAPalletLocation != null)       //pickDetail是拣货pallet的情况
            {
                //将库存中的该pallet标记发货
                pickDetailInDb.FBAPalletLocation.ShippedPlts += pickDetailInDb.PltsFromInventory;
                pickDetailInDb.FBAPalletLocation.PickingPlts -= pickDetailInDb.PltsFromInventory;

                //将pallet中的carton也标记发货
                ShipCartonsInPallet(context, pickDetailInDb);

                //更新在库状态
                if (pickDetailInDb.FBAPalletLocation.PickingPlts == 0 && pickDetailInDb.FBAPalletLocation.AvailablePlts != 0)
                {
                    pickDetailInDb.FBAPalletLocation.Status = FBAStatus.InStock;
                }
                else if (pickDetailInDb.FBAPalletLocation.PickingPlts == 0 && pickDetailInDb.FBAPalletLocation.AvailablePlts == 0)
                {
                    pickDetailInDb.FBAPalletLocation.Status = FBAStatus.Shipped;
                }
            }
            else if (pickDetailInDb.FBACartonLocation != null)      //pickDetail是直接拣货carton的情况
            {
                //直接将carton发货
                ShipPickDetailCartons(pickDetailInDb.FBACartonLocation, pickDetailInDb);
            }
        }

        private void ShipPickDetailCartons(FBACartonLocation cartonLocationInDb, FBAPickDetail pickDetailInDb)
        {
            cartonLocationInDb.ShippedCtns += pickDetailInDb.ActualQuantity;
            cartonLocationInDb.PickingCtns -= pickDetailInDb.ActualQuantity;

            if (cartonLocationInDb.PickingCtns == 0 && cartonLocationInDb.AvailableCtns != 0 && cartonLocationInDb.Location != "Pallet")
            {
                cartonLocationInDb.Status = FBAStatus.InStock;
            }
            else if (cartonLocationInDb.PickingCtns == 0 && cartonLocationInDb.AvailableCtns != 0 && cartonLocationInDb.Location == "Pallet")
            {
                cartonLocationInDb.Status = FBAStatus.InPallet;
            }
            else if (cartonLocationInDb.PickingCtns == 0 && cartonLocationInDb.AvailableCtns == 0)
            {
                cartonLocationInDb.Status = FBAStatus.Shipped;
            }
        }

        private void ShipCartonsInPallet(ApplicationDbContext context, FBAPickDetail pickDetailInDb)
        {
            var cartonsInPalletInDb = context.FBAPickDetailCartons
                .Include(x => x.FBAPickDetail)
                .Include(x => x.FBACartonLocation)
                .Where(x => x.FBAPickDetail.Id == pickDetailInDb.Id);

            foreach(var carton in cartonsInPalletInDb)
            {
                carton.FBACartonLocation.ShippedCtns += carton.PickCtns;
                carton.FBACartonLocation.PickingCtns -= carton.PickCtns;

                if (carton.FBACartonLocation.Status != FBAStatus.InPallet)
                {
                    if (carton.FBACartonLocation.PickingCtns == 0 && carton.FBACartonLocation.AvailableCtns != 0 && carton.FBACartonLocation.Location != "Pallet")
                    {
                        carton.FBACartonLocation.Status = FBAStatus.InStock;
                    }
                    else if (carton.FBACartonLocation.PickingCtns == 0 && carton.FBACartonLocation.AvailableCtns != 0 && carton.FBACartonLocation.Location == "Pallet")
                    {
                        carton.FBACartonLocation.Status = FBAStatus.InPallet;
                    }
                    else if (carton.FBACartonLocation.PickingCtns == 0 && carton.FBACartonLocation.AvailableCtns == 0)
                    {
                        carton.FBACartonLocation.Status = FBAStatus.Shipped;
                    }
                }
            }
        }

        private void ChangeStatus(FBAShipOrder shipOrderInDb, string operation, DateTime operationDate)
        {
            //当操作类型为正向转换订单状态时
            if (operation == FBAOperation.ChangeStatus)
            {
                //新建的空单能被直接转换为Release状态
                if (shipOrderInDb.Status == FBAStatus.NewCreated)
                {
                    shipOrderInDb.Status = FBAStatus.Ready;
                    shipOrderInDb.ReleasedBy = _userName;
                    shipOrderInDb.ReleasedDate = operationDate;
                    shipOrderInDb.OperationLog = "Ready By " + _userName;
                }
                //如果订单为在拣状态，则转换为给仓库的新订单状态
                else if (shipOrderInDb.Status == FBAStatus.Picking)
                {
                    shipOrderInDb.Status = FBAStatus.NewOrder;
                    shipOrderInDb.PlacedBy = _userName;
                    shipOrderInDb.PlaceTime = operationDate;
                    shipOrderInDb.OperationLog = "Placed by " + _userName;
                }
                //如果订单为新的订单状态，则转换为processing状态
                else if (shipOrderInDb.Status == FBAStatus.NewOrder || shipOrderInDb.Status == FBAStatus.ReturnedOrder)
                {
                    shipOrderInDb.Status = FBAStatus.Processing;
                    shipOrderInDb.StartedBy = _userName;
                    shipOrderInDb.StartedTime = operationDate;
                    shipOrderInDb.OperationLog = "Started by " + _userName;
                }
                //如果订单为processing状态，如果仓库有返回不同的操作，转换为Pending状态,否则转换为ready状态
                else if (shipOrderInDb.Status == FBAStatus.Processing)
                {
                    if (IsPending(shipOrderInDb))
                    {
                        shipOrderInDb.Status = FBAStatus.Pending;
                        shipOrderInDb.OperationLog = "Submited by " + _userName;
                    }
                    else
                    {
                        shipOrderInDb.Status = FBAStatus.Ready;
                        shipOrderInDb.ReadyBy = _userName;
                        shipOrderInDb.ReadyTime = operationDate;
                        shipOrderInDb.OperationLog = "Ready by " + _userName;
                    }
                }
                //Pending状态，则转回Processing状态
                //else if (shipOrderInDb.Status == FBAStatus.Pending)
                //{
                //    shipOrderInDb.Status = FBAStatus.Updated;
                //    shipOrderInDb.OperationLog = "Returned by " + _userName;
                //}
                //如果订单为ready状态，则转换为Released状态（如果是空单则不会返回给仓库）
                else if (shipOrderInDb.Status == FBAStatus.Ready)
                {
                    shipOrderInDb.Status = FBAStatus.Released;
                    shipOrderInDb.ReleasedDate = operationDate;
                    shipOrderInDb.ReleasedBy = _userName;
                    shipOrderInDb.OperationLog = "Released by " + _userName;
                }
                //当状态为Release的情况下，从库存实际扣除
                else if (shipOrderInDb.Status == FBAStatus.Released)
                {
                    shipOrderInDb.OperationLog = "Shipping confirmed by " + _userName;
                    shipOrderInDb.Status = FBAStatus.Shipped;
                    shipOrderInDb.ShippedBy = _userName;
                    shipOrderInDb.ShipDate = operationDate;

                    if (shipOrderInDb.FBAPickDetails.Count() != 0)
                    {
                        foreach (var pickDetailInDb in shipOrderInDb.FBAPickDetails)
                        {
                            ShipPickDetail(_context, pickDetailInDb.Id);
                        }

                    }
                }
            }
            //当操作类型为逆向转换订单状态时
            else if (operation == FBAOperation.ReverseStatus)
            {
                if (shipOrderInDb.Status == FBAStatus.Picking)
                {
                    if (shipOrderInDb.FBAPickDetails.Count != 0)
                    {
                        throw new Exception("Cannot reverse status because the pick details is not empty.");
                    }
                    else
                    {
                        shipOrderInDb.Status = FBAStatus.NewCreated;
                        shipOrderInDb.OperationLog = "Revert by " + _userName;
                    }
                }
                else if (shipOrderInDb.Status == FBAStatus.NewOrder || shipOrderInDb.Status == FBAStatus.ReturnedOrder)
                {
                    shipOrderInDb.Status = FBAStatus.Picking;
                    shipOrderInDb.OperationLog = "Recalled by " + _userName;
                }
                else if (shipOrderInDb.Status == FBAStatus.Processing || shipOrderInDb.Status == FBAStatus.Pending || shipOrderInDb.Status == FBAStatus.Updated)
                {
                    shipOrderInDb.Status = FBAStatus.ReturnedOrder;
                    shipOrderInDb.OperationLog = "Reset by " + _userName;
                }
                else if (shipOrderInDb.Status == FBAStatus.Ready)
                {
                    shipOrderInDb.Status = FBAStatus.ReturnedOrder;
                    shipOrderInDb.OperationLog = "Unready by " + _userName;
                }
                else if (shipOrderInDb.Status == FBAStatus.Released)
                {
                    shipOrderInDb.Status = FBAStatus.Ready;
                    shipOrderInDb.ReleasedBy = "Cancelled by " + _userName;
                }
            }
            //当操作直接为ready时，直接将订单mark成ready
            else if (operation == FBAStatus.Ready)
            {
                if (IsPending(shipOrderInDb))
                {
                    throw new Exception("Cannot ready because some of the comments are pending.");
                }

                shipOrderInDb.Status = FBAStatus.Ready;
                shipOrderInDb.ReadyTime = operationDate;
                shipOrderInDb.OperationLog = "Approved by " + _userName;
            }
            else if (operation == FBAStatus.Released)
            {
                if (shipOrderInDb.FBAPickDetails.Count != 0)
                {
                    throw new Exception("操作失败！请不要利用状态刷新不及时的BUG将有拣货内容的运单跳过仓库端操作直接发货！");
                }

                shipOrderInDb.Status = FBAStatus.Released;
                shipOrderInDb.ReleasedDate = operationDate;
                shipOrderInDb.OperationLog = "Release directly by " + _userName;
            }
        }

        private IList<FBABOLDetail> GenerateFBABOLList(IEnumerable<FBAPickDetail> pickDetailsInDb)
        {
            var bolList = new List<FBABOLDetail>();

            foreach (var pickDetail in pickDetailsInDb)
            {
                if (pickDetail.FBAPalletLocation != null)
                {
                    var cartonInPickList = pickDetail.FBAPickDetailCartons.ToList();
                    for (int i = 0; i < cartonInPickList.Count; i++)
                    {
                        var plt = 0;
                        var isMainItem = true;
                        //只有托盘中的第一项物品显示托盘数，其他物品不显示并在生成PDF的时候取消表格顶线，99999用于区分是否是同一托盘的非首项
                        if (i == 0)
                        {
                            plt = pickDetail.PltsFromInventory;
                        }
                        else
                        {
                            isMainItem = false;
                        }

                        bolList.Add(new FBABOLDetail
                        {
                            CustomerOrderNumber = cartonInPickList[i].FBACartonLocation.ShipmentId,
                            Contianer = pickDetail.Container,
                            CartonQuantity = cartonInPickList[i].PickCtns,
                            PalletQuantity = plt,
                            Weight = cartonInPickList[i].FBACartonLocation.GrossWeightPerCtn * cartonInPickList[i].PickCtns,
                            Location = pickDetail.Location,
                            IsMainItem = isMainItem
                        });
                    }
                }
                else
                {
                    bolList.Add(new FBABOLDetail
                    {
                        CustomerOrderNumber = pickDetail.ShipmentId,
                        Contianer = pickDetail.Container,
                        CartonQuantity = pickDetail.ActualQuantity,
                        PalletQuantity = 0,
                        Weight = pickDetail.ActualGrossWeight,
                        Location = pickDetail.Location
                    });
                }
            }

            return bolList;
        }

        private  FBAWorkOrder GenerateWorkOrder(FBAShipOrder shipOrder)
        {
            var wo = new FBAWorkOrder();
            var pickCartonDetails = _context.FBAPickDetailCartons
                .Include(x => x.FBACartonLocation)
                .Include(x => x.FBAPickDetail.FBAShipOrder)
                .Where(x => x.FBAPickDetail.FBAShipOrder.Id == shipOrder.Id)
                .ToList();

            var pickDetails = _context.FBAPickDetails
                .Include(x => x.FBAShipOrder)
                .Include(x => x.FBAPickDetailCartons)
                .Where(x => x.FBAShipOrder.Id == shipOrder.Id)
                .ToList();

            wo.PlaceTime = shipOrder.PlaceTime.Year == 1900 ? " " : shipOrder.PlaceTime.ToString("yyyy-MM-dd hh:mm");
            wo.ReadyTime = shipOrder.ReadyTime.Year == 1900 ? " " : shipOrder.ReadyTime.ToString("yyyy-MM-dd hh:mm");
            wo.ETS = shipOrder.ETS.ToString("yyyy-MM-dd") + shipOrder.ETSTimeRange;
            wo.PickMan = shipOrder.PickMan;
            wo.Instructor = shipOrder.Instructor;

            wo.ShipOrderNumber = shipOrder.ShipOrderNumber;
            wo.Reference = shipOrder.PickReference;
            wo.Destination = shipOrder.Destination;
            wo.Carrier = shipOrder.Carrier;
            wo.Lot = shipOrder.Lot;

            wo.PickableCtns = shipOrder.FBAPickDetails.Sum(x => x.ActualQuantity);
            wo.PickablePlts = shipOrder.FBAPickDetails.Sum(x => x.PltsFromInventory);
            wo.PltsFromInventory = shipOrder.FBAPickDetails.Sum(x => x.PltsFromInventory);
            wo.NewPlts = shipOrder.FBAPickDetails.Sum(x => x.NewPlts);
            wo.OutboundPlts = shipOrder.FBAPickDetails.Sum(x => x.ActualPlts);

            var order = 1;

            foreach(var p in pickDetails)
            {
                if (p.FBAPickDetailCartons.Count == 0)
                {
                    wo.PickingLists.Add(new PickingList
                    {
                        Order = order++,
                        Container = p.Container,
                        SKU = p.ShipmentId,
                        PickableCtns = p.ActualQuantity,
                        PickablePlts = p.PltsFromInventory,
                        Location = p.Location
                    });
                }
            }

            if (pickCartonDetails.Count != 0)
            {
                var lastSku = " ";
                foreach(var p in pickCartonDetails)
                {
                    var pickList = new PickingList
                    {
                        Order = order++,
                        Container = p.FBAPickDetail.Container,
                        SKU = p.FBACartonLocation.ShipmentId,
                        PickableCtns = p.PickCtns,
                        PickablePlts = 0,
                        Location = p.FBAPickDetail.Location
                    };

                    if (p.FBAPickDetail.ShipmentId != lastSku)
                    {
                        pickList.PickablePlts = p.FBAPickDetail.PltsFromInventory;
                        lastSku = p.FBAPickDetail.ShipmentId;
                    }

                    wo.PickingLists.Add(pickList);
                }
            }

            var vaildDetails = shipOrder.ChargingItemDetails.Where(x => x.HandlingStatus != FBAStatus.Na);

            foreach(var c in vaildDetails)
            {
                wo.OperationInstructions.Add(new OperationInstruction {
                    Id = c.Id,
                    Description = c.Description,
                    Comment = c.Comment,
                    CreateBy = c.CreateBy,
                    CreateDate = c.CreateDate,
                    Result = c.Result,
                    HandlingStatus = c.HandlingStatus,
                    Status =c.Status
                });
            }

            return wo;
        }

        private bool IsPending(FBAShipOrder shipOrderInDb)
        {
            var result = false;
            foreach(var s in shipOrderInDb.ChargingItemDetails)
            {
                if (s.HandlingStatus == FBAStatus.Pending)
                {
                    result = true;
                    break;
                }
            }
            return result;
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

        public string Instruction { get; set; }

        public string ShippedBy { get; set; }

        public string BOLNumber { get; set; }

        public string Carrier { get; set; }

        public DateTime ETS { get; set; }

        public string TimeRange { get; set; }

        public string PickNumber { get; set; }

        public string PurchaseOrderNumber { get; set; }
    }

    public class FBAWorkOrder
    {
        public string PlaceTime { get; set; }

        public string ShipOrderNumber { get; set; }

        public string Reference { get; set; }

        public string ETS { get; set; }

        public int PickableCtns { get; set; }

        public int PickablePlts { get; set; }

        public string Carrier { get; set; }

        public string Destination { get; set; }

        public string ReadyTime { get; set; }

        public string Lot { get; set; }

        public string PickMan { get; set; }

        public string Instructor { get; set; }

        public int PltsFromInventory { get; set; }

        public int NewPlts { get; set; }

        public int OutboundPlts { get; set; }

        public ICollection<PickingList> PickingLists { get; set; }

        public ICollection<OperationInstruction> OperationInstructions { get; set; }

        public FBAWorkOrder()
        {
            PickingLists = new List<PickingList>();

            OperationInstructions = new List<OperationInstruction>();
        }
    }

    public class PickingList
    {
        public int Order { get; set; }

        public string Container { get; set; }

        public string SKU { get; set; }

        public int PickableCtns { get; set; }

        public int PickablePlts { get; set; }

        public string Location { get; set; }
    }

    public class OperationInstruction
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public string Status { get; set; }

        public DateTime CreateDate { get; set; }

        public string CreateBy { get; set; }

        public string Comment { get; set; }

        public string Result { get; set; }

        public string HandlingStatus { get; set; }
    }
}
