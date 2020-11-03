using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Models.StaticClass;
using ClothResorting.Models.ApiTransformModels;
using ClothResorting.Models.FBAModels;
using ClothResorting.Dtos.Fba;
using System.Web;
using ClothResorting.Models.FBAModels.StaticModels;
using ClothResorting.Models.FBAModels.BaseClass;

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBAInvoiceDetailController : ApiController
    {
        private ApplicationDbContext _context;
        private string _userName;

        public FBAInvoiceDetailController()
        {
            _context = new ApplicationDbContext();
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0] == "" ? (HttpContext.Current.Request.Headers.Get("AppUser") == null ? "" : HttpContext.Current.Request.Headers.Get("AppUser")) : HttpContext.Current.User.Identity.Name.Split('@')[0];
        }

        // GET /api/fba/FBAInvoiceDetail/?customerId={customerId}&reference={reference}&invoiceType={invoiceType}  获取收费项目草表
        [HttpGet]
        public IHttpActionResult GetCharginItemDetails([FromUri]string reference, [FromUri]string invoiceType, [FromUri]bool isChargingItemDetail)
        {
            if (isChargingItemDetail)
            {
                if (invoiceType == FBAInvoiceType.MasterOrder)
                {
                    return Ok(_context.ChargingItemDetails
                        .Include(x => x.FBAMasterOrder)
                        .Where(x => x.FBAMasterOrder.Container == reference)
                        .Select(Mapper.Map<ChargingItemDetail, ChargingItemDetailDto>));
                }
                else if (invoiceType == FBAInvoiceType.ShipOrder)
                {
                    return Ok(_context.ChargingItemDetails
                        .Include(x => x.FBAShipOrder)
                        .Where(x => x.FBAShipOrder.ShipOrderNumber == reference)
                        .Select(Mapper.Map<ChargingItemDetail, ChargingItemDetailDto>));
                }
            }
            else
            {
                if (invoiceType == FBAInvoiceType.MasterOrder)
                {
                    return Ok(_context.FBAMasterOrders
                        .Include(x => x.Customer)
                        .SingleOrDefault(x => x.Container == reference)
                        .Customer.CustomerCode);
                }
                else if (invoiceType == FBAInvoiceType.ShipOrder)
                {
                    return Ok(_context.FBAShipOrders
                        .SingleOrDefault(x => x.ShipOrderNumber == reference)
                        .CustomerCode);
                }
            }

            return Ok();
        }

        // GET /api/fba/FBAInvoiceDetail/?customerId={customerId}&reference={reference}&invoiceType={invoiceType}
        [HttpGet]
        public IHttpActionResult GetInvoiceDetails([FromUri]string reference, [FromUri]string invoiceType)
        {
            if (invoiceType == FBAInvoiceType.MasterOrder)
            {
                var dto = _context.InvoiceDetails
                    .Include(x => x.FBAMasterOrder)
                    .Where(x => x.FBAMasterOrder.Container == reference)
                    .Select(Mapper.Map<InvoiceDetail, InvoiceDetailDto>)
                    .ToList();

                foreach (var d in dto)
                {
                    d.Net = d.Amount - d.Cost;
                    d.OriginalAmount = d.OriginalAmount;
                }

                return Ok(dto);
            }
            else if (invoiceType == FBAInvoiceType.ShipOrder)
            {
                var dto = _context.InvoiceDetails
                    .Include(x => x.FBAShipOrder)
                    .Where(x => x.FBAShipOrder.ShipOrderNumber == reference)
                    .Select(Mapper.Map<InvoiceDetail, InvoiceDetailDto>)
                    .ToList();

                foreach (var d in dto)
                {
                    d.Net = d.Amount - d.Cost;
                }

                return Ok(dto);
            }
            else
            {
                return Ok();
            }
        }

        // GET /api/fba/FBAInvoiceDetail/?reference={reference}&invoiceType={invoiceType}&ajaxStep={ajaxStep}
        [HttpGet]
        public IHttpActionResult GetInformation([FromUri]string reference, [FromUri]string invoiceType, [FromUri]int ajaxStep)
        {
            var customerId = GetCustomerId(reference, invoiceType);

            switch(ajaxStep)
            {
                case 0:        //ajax 第0步 获取该单号下的托盘总数和箱子总数+SKU数量
                    {
                        if (invoiceType == "MasterOrder")
                        {
                            var masterOrderPallets = _context.FBAPallets
                                .Where(x => x.Container == reference)
                                .ToList();

                            var masterOrderInDb = _context.FBAMasterOrders
                                .SingleOrDefault(x => x.Container == reference);

                            var orderDetsils = _context.FBAOrderDetails
                                .Include(x => x.FBAMasterOrder)
                                .Where(x => x.FBAMasterOrder.Container == reference)
                                .ToList();

                            var plts = masterOrderPallets.Sum(x => x.ActualPallets);
                            var ctns = orderDetsils.Sum(x => x.ActualQuantity);
                            var skuNumber = orderDetsils.GroupBy(x => x.ShipmentId).Count();

                            return Ok(new { Pallets = plts, Cartons = ctns, OriginalPallets = masterOrderInDb.OriginalPlts, SkuNumber = skuNumber, ReleasedDate = "1900-1-1", InboundDate = masterOrderInDb.InboundDate.ToString("yyyy-MM-dd") });
                        }
                        else if (invoiceType == "ShipOrder")
                        {
                            var pickDetails = _context.FBAPickDetails
                                .Include(x => x.FBAShipOrder)
                                .Where(x => x.FBAShipOrder.ShipOrderNumber == reference)
                                .ToList();

                            var shipOrderInDb = _context.FBAShipOrders.SingleOrDefault(x => x.ShipOrderNumber == reference);

                            var plts = pickDetails.Sum(x => x.ActualPlts);
                            var ctns = pickDetails.Sum(x => x.ActualQuantity);

                            return Ok(new { Pallets = plts, Cartons = ctns, OriginalPallets = "N/A", SkuNumber = "N/A" , ReleasedDate = shipOrderInDb.ReleasedDate.ToString("yyyy-MM-dd"), InboundDate = "1900-1-1"});
                        }
                        else
                        {
                            return Ok();
                        }
                    }
                case 1:    //Ajax第1步，获取收费项目分类
                    {
                        var typesGroup = _context.ChargingItems
                            .Include(x => x.UpperVendor)
                            .Where(x => x.UpperVendor.Id == customerId)
                            .GroupBy(x => x.ChargingType)
                            .ToList();

                        var typesList = new List<string>();

                        foreach (var g in typesGroup)
                        {
                            typesList.Add(g.First().ChargingType);
                        }

                        return Ok(typesList);
                    }
            }

            return Ok();
        }

        //// GET /api/FBAInvoiceDetail/?customerId={customerId}  Ajax第1步，获取收费项目分类
        //[HttpGet]
        //public IHttpActionResult GetChargingTypes([FromUri]int customerId)
        //{
        //    var typesGroup = _context.ChargingItems
        //        .Include(x => x.UpperVendor)
        //        .Where(x => x.UpperVendor.Id == customerId)
        //        .GroupBy(x => x.ChargingType);

        //    var typesList = new List<string>();

        //    foreach (var g in typesGroup)
        //    {
        //        typesList.Add(g.First().ChargingType);
        //    }

        //    return Ok(typesList);
        //}

        // GET /api/FBAinvoiceDetail/?customerId={customerId}&chargingType={chargingType}             Ajax2, 改变下拉菜单的收费类型，获取该收费类型的所有选项
        [HttpGet]
        public IHttpActionResult GetChargingItems([FromUri]string reference, [FromUri]string invoiceType, [FromUri]string chargingType)
        {
            var nameList = new List<string>();
            var customerId = GetCustomerId(reference, invoiceType);

            var chargingItemInDb = _context.ChargingItems
                .Include(x => x.UpperVendor)
                .Where(x => x.ChargingType == chargingType
                    && x.UpperVendor.Id == customerId);

            foreach (var c in chargingItemInDb)
            {
                nameList.Add(c.Name);
            }

            return Ok(nameList);
        }

        // GET /api/fba/FBAInvoiceDetail/?reference={reference}&invoiceType={invoiceType}&operation={operation}
        //[HttpGet]
        //public IHttpActionResult GetCascaderChargingType([FromUri]string reference, [FromUri]string invoiceType, [FromUri]string operation)
        //{
        //    var customerId = GetCustomerId(reference, invoiceType);

        //    if (operation == "Cascader")
        //    {
                
        //    }
        //}

        // GET /api/fba/FBAInvoiceDetail/?customerId={customerId}&itemName={itemName}    ajax3，获取所选择项目的费率和计价单位
        [HttpGet]
        public IHttpActionResult GetRate([FromUri]string reference, [FromUri]string invoiceType, [FromUri]string itemName)
        {
            var customerId = GetCustomerId(reference, invoiceType);

            var chargingItemInDb = _context.ChargingItems
                .Include(x => x.UpperVendor)
                .Where(x => x.Name == itemName
                    && x.UpperVendor.Id == customerId)
                .First();

            var annoyObj = new
            {
                Rate = chargingItemInDb.Rate,
                Unit = chargingItemInDb.Unit,
                Description = chargingItemInDb.Description
            };

            return Ok(annoyObj);
        }

        // GET /api/fba/FBAInvoiceDetail/?invoiceDetailId={invoiceDetailId}
        [HttpGet]
        public IHttpActionResult GetInvoiceDetail([FromUri]int invoiceDetailId)
        {
            var invoiceDetailIdInDb = _context.InvoiceDetails.Find(invoiceDetailId);

            var invoiceDetailDto = new InvoiceDetailDto
            {
                DateOfCost = invoiceDetailIdInDb.DateOfCost,
                Activity = invoiceDetailIdInDb.Activity,
                ChargingType = invoiceDetailIdInDb.ChargingType,
                Cost = invoiceDetailIdInDb.Cost,
                Rate = invoiceDetailIdInDb.Rate,
                Discount = invoiceDetailIdInDb.Discount,
                Quantity = invoiceDetailIdInDb.Quantity,
                OriginalAmount = (float)invoiceDetailIdInDb.OriginalAmount,
                Memo = invoiceDetailIdInDb.Memo,
                Amount = invoiceDetailIdInDb.Amount,
                Unit = invoiceDetailIdInDb.Unit
            };

            return Ok(invoiceDetailDto);
        }

        // POST /api/fba/FBAInvoiceDetail/?reference={reference}&invoiceType={invoiceType}&description={description}&isChargingItem={isChargingItem}
        public IHttpActionResult CreateChargingItemByAccounting([FromUri]string reference, [FromUri]string invoiceType, [FromUri]string description, [FromUri]bool isChargingItem)
        {
            var chargingItem = new ChargingItemDetail
            {
                CreateBy = _userName,
                Description = description,
                CreateDate = DateTime.Now,
                OriginalDescription = description,
                HandlingStatus = "N/A",
                IsCharging = isChargingItem
            };

            chargingItem.Status = isChargingItem ? FBAStatus.WaitingForCharging : FBAStatus.NoNeedForCharging;

            if (invoiceType == FBAOrderType.MasterOrder)
            {
                var masterOrderInDb = _context.FBAMasterOrders.SingleOrDefault(x => x.Container == reference);
                chargingItem.FBAMasterOrder = masterOrderInDb;
            }
            else
            {
                var shipOrderInDb = _context.FBAShipOrders.SingleOrDefault(x => x.ShipOrderNumber == reference);
                chargingItem.FBAShipOrder = shipOrderInDb;
            }

            _context.ChargingItemDetails.Add(chargingItem);
            _context.SaveChanges();

            var id = _context.ChargingItemDetails.OrderByDescending(x => x.Id).First().Id;

            return Created(Request.RequestUri, new { Id = id, Description = description, HandlingStatus = "Draft" });
        }

        // POST /api/fba/FBAInvoiceDetail/?reference={reference}&invoiceType={invoiceType}&description={description}
        [HttpPost]
        public IHttpActionResult CreateInstructionByCustomer([FromUri]string reference, [FromUri]string invoiceType, [FromUri]string description)
        {
            var instruction = new ChargingItemDetail
            {
                CreateBy = _userName,
                Description = description,
                CreateDate = DateTime.Now,
                OriginalDescription = description,
                HandlingStatus = "Draft",
                IsInstruction = true,
                Status = "TBD"
            };

            if (invoiceType == FBAOrderType.MasterOrder)
            {
                var masterOrderInDb = _context.FBAMasterOrders.SingleOrDefault(x => x.Container == reference);
                instruction.FBAMasterOrder = masterOrderInDb;
            }
            else
            {
                var shipOrderInDb = _context.FBAShipOrders.SingleOrDefault(x => x.ShipOrderNumber == reference);
                instruction.FBAShipOrder = shipOrderInDb;
            }

            _context.ChargingItemDetails.Add(instruction);
            _context.SaveChanges();

            var id = _context.ChargingItemDetails.OrderByDescending(x => x.Id).First().Id;

            return Created(Request.RequestUri, new { Id = id, Description = description, HandlingStatus = "Draft" });
        }

        // POST /api/fba/FBAInvoiceDetail/?reference={reference}&invoiceType={invoiceType}&description={description}&isChargingIte{isChargingItem}&isInstruction={isInstruction}
        [HttpPost]
        public IHttpActionResult CreateChargingItemRef([FromUri]string reference, [FromUri]string invoiceType, [FromUri]string description, [FromUri]bool isChargingItem, [FromUri]bool isInstruction, [FromBody]ObjectBody obj)
        {
            var detail = new ChargingItemDetail();

            if (invoiceType == FBAInvoiceType.MasterOrder)
            {
                var masterOrderInDb = _context.FBAMasterOrders.SingleOrDefault(x => x.Container == reference);

                var newDetail = new ChargingItemDetail {
                    Status = FBAStatus.Unhandled,
                    HandlingStatus = isInstruction == true ? FBAStatus.New : FBAStatus.Na,
                    CreateBy = _userName,
                    OriginalDescription = description,
                    CreateDate = DateTime.Now,
                    Description = description,
                    IsCharging = isChargingItem,
                    FBAMasterOrder = masterOrderInDb
                };

                if (masterOrderInDb.Status == FBAStatus.Pending)
                    masterOrderInDb.Status = FBAStatus.Updated;

                newDetail.Status = isChargingItem ? FBAStatus.WaitingForCharging : FBAStatus.NoNeedForCharging;

                detail = newDetail;
            }
            else if (invoiceType == FBAInvoiceType.ShipOrder)
            {
                var shipOrderInDb = _context.FBAShipOrders.SingleOrDefault(x => x.ShipOrderNumber == reference);

                var newDetail = new ChargingItemDetail
                {
                    Status = FBAStatus.Unhandled,
                    HandlingStatus = isInstruction == true ? FBAStatus.New : FBAStatus.Na,
                    CreateBy = _userName,
                    CreateDate = DateTime.Now,
                    OriginalDescription = description,
                    Description = description,
                    IsCharging = isChargingItem,
                    FBAShipOrder = shipOrderInDb
                };

                if (shipOrderInDb.Status == FBAStatus.Pending)
                    shipOrderInDb.Status = FBAStatus.Updated;

                newDetail.Status = isChargingItem ? FBAStatus.WaitingForCharging : FBAStatus.NoNeedForCharging;
                detail = newDetail;
            }

            if (obj != null)
            {
                detail.HandlingStatus = obj.Content;
            }

            _context.ChargingItemDetails.Add(detail);
            _context.SaveChanges();

            return Created(Request.RequestUri + "/", Mapper.Map<ChargingItemDetail, ChargingItemDetailDto>(detail));
        }

        // POST /api/fba/FBAInvoiceDetail/?reference={reference}&invoiceType={invoiceType}
        [HttpPost]
        public IHttpActionResult CreateChargingItem([FromUri]string reference, [FromUri]string invoiceType, [FromBody]InvoiceDetailJsonObj obj)
        {
            var invoice = new InvoiceDetail();

            if (invoiceType == FBAInvoiceType.MasterOrder)
            {
                var masterOrderInDb = _context.FBAMasterOrders.SingleOrDefault(x => x.Container == reference);

                var invoiceDetail = new InvoiceDetail {
                    Activity = obj.Activity,
                    ChargingType = obj.ChargingType,
                    DateOfCost = obj.DateOfCost,
                    Memo = obj.Memo == null ? null : obj.Memo.ToUpper(),
                    Unit = obj.Unit,
                    Rate = obj.Rate,
                    Cost = obj.Cost,
                    OriginalAmount = obj.OriginalAmount,
                    Discount = obj.Discount,
                    Quantity = obj.Quantity,
                    InvoiceType = invoiceType,
                    Amount = obj.Amount,
                    FBAMasterOrder = masterOrderInDb,
                    Operator = _userName
                };

                invoice = invoiceDetail;
                _context.InvoiceDetails.Add(invoiceDetail);
            }
            else if (invoiceType == FBAInvoiceType.ShipOrder)
            {
                var shipOrderInDb = _context.FBAShipOrders.SingleOrDefault(x => x.ShipOrderNumber == reference);

                var invoiceDetail = new InvoiceDetail
                {
                    Activity = obj.Activity,
                    ChargingType = obj.ChargingType,
                    DateOfCost = obj.DateOfCost,
                    Memo = obj.Memo == null ? null : obj.Memo.ToUpper(),
                    Unit = obj.Unit,
                    Rate = obj.Rate,
                    Discount = obj.Discount,
                    Cost = obj.Cost,
                    Quantity = obj.Quantity,
                    OriginalAmount = obj.OriginalAmount,
                    InvoiceType = invoiceType,
                    Amount = obj.Amount,
                    FBAShipOrder = shipOrderInDb,
                    Operator = _userName
                };

                invoice = invoiceDetail;
                _context.InvoiceDetails.Add(invoiceDetail);
            }

            _context.SaveChanges();

            return Created(Request.RequestUri + "/", Mapper.Map<InvoiceDetail, InvoiceDetailDto>(invoice));
        }

        // POST /api/fba/FBAInvoiceDetail/?reference={reference}&invoiceType={invoiceType}
        [HttpPost]
        public IHttpActionResult CreateInstructionByModel([FromBody]Instruction obj)
        {
            var detail = new ChargingItemDetail();

            if (obj.OrderType == FBAInvoiceType.MasterOrder)
            {
                var masterOrderInDb = _context.FBAMasterOrders.SingleOrDefault(x => x.Container == obj.Reference);

                if (masterOrderInDb.InvoiceStatus == FBAStatus.Closed || masterOrderInDb.InvoiceStatus == FBAStatus.Generated)
                {
                    throw new Exception("Cannot add any items in a closed order");
                }

                var newDetail = new ChargingItemDetail
                {
                    Status = FBAStatus.Unhandled,
                    HandlingStatus = obj.IsInstruction || obj.IsOperation ? FBAStatus.New : FBAStatus.Na,
                    CreateBy = _userName,
                    OriginalDescription = obj.Description,
                    IsOperation = obj.IsOperation,
                    IsCharging = obj.IsChargingItem,
                    IsInstruction = obj.IsInstruction,
                    CreateDate = DateTime.Now,
                    Description = obj.Description,
                    FBAMasterOrder = masterOrderInDb
                };

                if (masterOrderInDb.Status == FBAStatus.Pending)
                    masterOrderInDb.Status = FBAStatus.Updated;

                if (obj.IsChargingItem)
                {
                    newDetail.Status = FBAStatus.WaitingForCharging;
                }
                else
                {
                    newDetail.Status = FBAStatus.NoNeedForCharging;
                }

                detail = newDetail;
            }
            else if (obj.OrderType == FBAInvoiceType.ShipOrder)
            {
                var shipOrderInDb = _context.FBAShipOrders.SingleOrDefault(x => x.ShipOrderNumber == obj.Reference);

                if (shipOrderInDb.InvoiceStatus == FBAStatus.Generated || shipOrderInDb.InvoiceStatus == FBAStatus.Closed)
                {
                    throw new Exception("Cannot add any items in a closed order");
                }

                var newDetail = new ChargingItemDetail
                {
                    Status = FBAStatus.Unhandled,
                    HandlingStatus = obj.IsInstruction || obj.IsOperation ? FBAStatus.New : FBAStatus.Na,
                    CreateBy = _userName,
                    CreateDate = DateTime.Now,
                    OriginalDescription = obj.Description,
                    IsOperation = obj.IsOperation,
                    IsCharging = obj.IsChargingItem,
                    IsInstruction = obj.IsInstruction,
                    Description = obj.Description,
                    FBAShipOrder = shipOrderInDb
                };

                if (shipOrderInDb.Status == FBAStatus.Pending)
                    shipOrderInDb.Status = FBAStatus.Updated;

                if (obj.IsChargingItem)
                {
                    newDetail.Status = FBAStatus.WaitingForCharging;
                }
                else
                {
                    newDetail.Status = FBAStatus.NoNeedForCharging;
                }

                detail = newDetail;
            }

            _context.ChargingItemDetails.Add(detail);
            _context.SaveChanges();

            return Created(Request.RequestUri + "/", Mapper.Map<ChargingItemDetail, ChargingItemDetailDto>(detail));
        }

        // PUT /api/fba/FBAInvoiceDetail/?chargingItemDetailId={chargingItemDetailId}&description={description}
        [HttpPut]
        public IHttpActionResult UpdateInstructionByCustomer([FromUri]int chargingItemDetailId, [FromUri]string description)
        {
            var item = _context.ChargingItemDetails.Find(chargingItemDetailId);

            if (item.HandlingStatus != "Draft")
            {
                throw new Exception("Cannot edit a instruction that status is not Draft.");
            }

            item.Description = description;
            item.OriginalDescription = description;

            _context.SaveChanges();

            return Ok(new { item.Id, item.Description, item.HandlingStatus });
        }

        // PUT /api/fba/fbainvoicedetail/?chargingItemDetailId={chargingItemDetailId}
        [HttpPut]
        public void ChangeStatus([FromUri]int chargingItemDetailId)
        {
            var detailInDb = _context.ChargingItemDetails.Find(chargingItemDetailId);

            if (detailInDb.Status == "Waiting for charging")
            {
                detailInDb.Status = "Charged";
            }
            else if (detailInDb.Status == "No need for charging")
            {
                detailInDb.Status = "Waiting for charging";
            }
            else
            {
                detailInDb.Status = "No need for charging";
            }

            _context.SaveChanges();
        }

        // PUT /api/fba/fbainvoicedetail/?invoiceDetailId={invoiceDetailId}
        [HttpPut]
        public void UpdateInvoiceDetail([FromUri]int invoiceDetailId, [FromBody]InvoiceDetailJsonObj obj)
        {
            var invoiceDetailInDb = _context.InvoiceDetails.Find(invoiceDetailId);

            invoiceDetailInDb.Activity = obj.Activity;
            invoiceDetailInDb.Amount = obj.Amount;
            invoiceDetailInDb.ChargingType = obj.ChargingType;
            invoiceDetailInDb.Cost = obj.Cost;
            invoiceDetailInDb.DateOfCost = obj.DateOfCost;
            invoiceDetailInDb.Memo = obj.Memo == null ? null : obj.Memo.ToUpper();
            invoiceDetailInDb.OriginalAmount = obj.OriginalAmount;
            invoiceDetailInDb.Discount = obj.Discount;
            invoiceDetailInDb.Operator = _userName;
            invoiceDetailInDb.Quantity = obj.Quantity;
            invoiceDetailInDb.Rate = obj.Rate;
            invoiceDetailInDb.Unit = obj.Unit;

            _context.SaveChanges();
        }

        // PUT /api/fba/fbainvoicedetail/?reference{reference}&invoiceType={invoiceType}&closeDate={closeDate}&isAppliedMinCharge={isAppliedMinCharge}
        [HttpPut]
        public void GenerateInvoice([FromUri]string reference, [FromUri]string invoiceType, [FromUri]DateTime closeDate, [FromUri]bool isAppliedMinCharge)
        {
            if (invoiceType == FBAInvoiceType.ShipOrder)
            {
                var shipOrderInDb = _context.FBAShipOrders
                    .SingleOrDefault(x => x.ShipOrderNumber == reference);

                shipOrderInDb.InvoiceStatus = FBAStatus.Generated;
                shipOrderInDb.CloseDate = closeDate;
                shipOrderInDb.ConfirmedBy = _userName;

                if (isAppliedMinCharge)
                {
                    var invoiceDetailsInDb = _context.InvoiceDetails
                        .Include(x => x.FBAShipOrder)
                        .Where(x => x.FBAShipOrder.ShipOrderNumber == reference);
                    float outboundMinCharge = 0;

                    try
                    {
                       outboundMinCharge = _context.UpperVendors
                            .SingleOrDefault(x => x.CustomerCode == shipOrderInDb.CustomerCode)
                            .OutboundMinCharge;
                    }
                    catch(Exception e)
                    {
                        throw new Exception("Customer code: " + shipOrderInDb.CustomerCode + " was not found in system.");
                    }

                    //检查是否已经存在差价收费项目
                    var priceDifference = invoiceDetailsInDb.SingleOrDefault(x => x.Activity == "Price Difference");
                    float pd = 0;

                    if (invoiceDetailsInDb.Count() == 0)
                    {
                        pd = outboundMinCharge;
                    }
                    else
                    {
                        pd = outboundMinCharge - (float)invoiceDetailsInDb.Sum(x => x.Amount);
                    }

                    if (priceDifference == null)
                    {
                        //检查是否满足shiporder每单最小值,不满足的话则建立差价收费项目
                        if (pd > 0)
                        {
                            _context.InvoiceDetails.Add(new InvoiceDetail
                            {
                                Activity = "Price Difference",
                                ChargingType = "Price Difference",
                                Unit = "N/A",
                                Quantity = 1,
                                Discount = 1,
                                Rate = pd,
                                Amount = pd,
                                InvoiceType = FBAInvoiceType.ShipOrder,
                                DateOfCost = DateTime.Now,
                                Operator = _userName,
                                FBAShipOrder = shipOrderInDb,
                                Memo = "MIN CHARGE"
                            });
                        }
                    }
                    //如果已经有收费项目，则刷新收费项目中的金额
                    else
                    {
                        var amount = invoiceDetailsInDb.Where(x => x.Activity != "Price Difference").ToList().Sum(x => x.Amount);
                        pd = outboundMinCharge - (float)amount;

                        //如果仍然有差额，则刷新差额收费
                        if (pd > 0)
                        {
                            priceDifference.Amount = pd;
                            priceDifference.Rate = pd;
                        }
                        //如果没有差额，则删除差额收费
                        else
                        {
                            _context.InvoiceDetails.Remove(priceDifference);
                        }
                    }
                }
            }
            else if (invoiceType == FBAInvoiceType.MasterOrder)
            {
                var masterOrder = _context.FBAMasterOrders
                    .SingleOrDefault(x => x.Container == reference);

                masterOrder.InvoiceStatus = FBAStatus.Generated;
                masterOrder.CloseDate = closeDate;
                masterOrder.ConfirmedBy = _userName;

                if (isAppliedMinCharge)
                {
                    var invoiceDetailsInDb = _context.InvoiceDetails
                        .Include(x => x.FBAShipOrder)
                        .Where(x => x.FBAMasterOrder.Container == reference);
                    float inboundMinCharge = 0;

                    try
                    {
                        inboundMinCharge = _context.UpperVendors
                             .SingleOrDefault(x => x.CustomerCode == masterOrder.CustomerCode)
                             .InboundMinCharge;
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Customer code: " + masterOrder.CustomerCode + " was not found in system.");
                    }

                    //检查是否已经存在差价收费项目
                    var priceDifference = invoiceDetailsInDb.SingleOrDefault(x => x.Activity == "Price Difference");
                    float pd = 0;

                    if (invoiceDetailsInDb.Count() == 0)
                    {
                        pd = inboundMinCharge;
                    }
                    else
                    {
                        pd = inboundMinCharge - (float)invoiceDetailsInDb.Sum(x => x.Amount);
                    }

                    if (priceDifference == null)
                    {
                        //检查是否满足shiporder每单最小值,不满足的话则建立差价收费项目
                        if (pd > 0)
                        {
                            _context.InvoiceDetails.Add(new InvoiceDetail
                            {
                                Activity = "Price Difference",
                                ChargingType = "Price Difference",
                                Unit = "N/A",
                                Quantity = 1,
                                Discount = 1,
                                Rate = pd,
                                Amount = pd,
                                InvoiceType = FBAInvoiceType.MasterOrder,
                                DateOfCost = DateTime.Now,
                                Operator = _userName,
                                FBAMasterOrder = masterOrder,
                                Memo = "MIN CHARGE"
                            });
                        }
                    }
                    //如果已经有收费项目，则刷新收费项目中的金额
                    else
                    {
                        var amount = invoiceDetailsInDb.Where(x => x.Activity != "Price Difference").ToList().Sum(x => x.Amount);
                        pd = inboundMinCharge - (float)amount;

                        //如果仍然有差额，则刷新差额收费
                        if (pd > 0)
                        {
                            priceDifference.Amount = pd;
                            priceDifference.Rate = pd;
                        }
                        //如果没有差额，则删除差额收费
                        else
                        {
                            _context.InvoiceDetails.Remove(priceDifference);
                        }
                    }
                }
            }

            _context.SaveChanges();
        }

        // PUT /api/fbainvoicedetail/?reference={reference}&invoiceType={invoiceType}&status={status}
        [HttpPut]
        public void UpdateOrderStatus([FromUri]string reference, [FromUri]string invoiceType, [FromUri]string status)
        {
            if (invoiceType == FBAInvoiceType.ShipOrder)
            {
                var shipOrderInDb = _context.FBAShipOrders
                    .SingleOrDefault(x => x.ShipOrderNumber == reference);

                shipOrderInDb.InvoiceStatus = status;
                shipOrderInDb.ConfirmedBy = _userName;
            }
            else if (invoiceType == FBAInvoiceType.MasterOrder)
            {
                var masterOrder = _context.FBAMasterOrders
                    .SingleOrDefault(x => x.Container == reference);

                masterOrder.InvoiceStatus = status;
                masterOrder.ConfirmedBy = _userName;
            }

            _context.SaveChanges();
        }

        // PUT /api/fbainvoicedetail/?buttonType={buttonType}
        [HttpPut]
        public void UpdateButtonStatus([FromUri]int invoiceDetailId, [FromUri]string buttonType)
        {
            var invoiceDetailInDb = _context.InvoiceDetails.Find(invoiceDetailId);

            if (buttonType == FBAButtonType.Confirm)
            {
                if (invoiceDetailInDb.CostConfirm)
                {
                    invoiceDetailInDb.CostConfirm = false;
                }
                else
                {
                    invoiceDetailInDb.CostConfirm = true;
                }
            }
            else if (buttonType == FBAButtonType.Payment)
            {
                if (invoiceDetailInDb.PaymentStatus)
                {
                    invoiceDetailInDb.PaymentStatus = false;
                }
                else
                {
                    invoiceDetailInDb.PaymentStatus = true;
                }
            }
            else if (buttonType == FBAButtonType.Collection)
            {
                if (invoiceDetailInDb.CollectionStatus)
                {
                    invoiceDetailInDb.CollectionStatus = false;
                }
                else
                {
                    invoiceDetailInDb.CollectionStatus = true;
                }
            }

            _context.SaveChanges();
        }

        // DELETE /api/fba/fbainvoicedetail/?chargingItemDetailId={chargingItemDetailId}
        [HttpDelete]
        public void DeleteChargingItemDetail([FromUri]int chargingItemDetailId)
        {
            var detailInDb = _context.ChargingItemDetails
                .Include(x => x.FBAMasterOrder.ChargingItemDetails)
                .Include(x => x.FBAShipOrder.ChargingItemDetails)
                .SingleOrDefault(x => x.Id == chargingItemDetailId);

            detailInDb.HandlingStatus = "Discarded";

            if (detailInDb.FBAShipOrder != null)
            {
                var originalStatus = (detailInDb.FBAShipOrder.Status == FBAStatus.Pending || detailInDb.FBAShipOrder.Status == FBAStatus.Updated) ? FBAStatus.Updated : detailInDb.FBAShipOrder.Status;
                if (detailInDb.FBAShipOrder.ChargingItemDetails.Where(x => x.HandlingStatus == FBAStatus.Updated).Any())
                    detailInDb.FBAShipOrder.Status = FBAStatus.Updated;
                else if (detailInDb.FBAShipOrder.ChargingItemDetails.Where(x => x.HandlingStatus == FBAStatus.Pending).Any())
                    detailInDb.FBAShipOrder.Status = FBAStatus.Pending;
                else
                    detailInDb.FBAShipOrder.Status = originalStatus;
            }
            else
            {
                var originalStatus = (detailInDb.FBAMasterOrder.Status == FBAStatus.Pending || detailInDb.FBAMasterOrder.Status == FBAStatus.Updated) ? FBAStatus.Updated : detailInDb.FBAMasterOrder.Status;
                if (detailInDb.FBAMasterOrder.ChargingItemDetails.Where(x => x.HandlingStatus == FBAStatus.Updated).Any())
                    detailInDb.FBAMasterOrder.Status = FBAStatus.Updated;
                else if (detailInDb.FBAMasterOrder.ChargingItemDetails.Where(x => x.HandlingStatus == FBAStatus.Pending).Any())
                    detailInDb.FBAMasterOrder.Status = FBAStatus.Pending;
                else
                    detailInDb.FBAMasterOrder.Status = originalStatus;
            }

            _context.ChargingItemDetails.Remove(detailInDb);
            _context.SaveChanges();
        }

        private int GetCustomerId(string reference, string invoiceType)
        {
            var customerId = 0;

            if (invoiceType == FBAInvoiceType.MasterOrder)
            {
                customerId = _context.FBAMasterOrders
                    .Include(x => x.Customer)
                    .FirstOrDefault(x => x.Container == reference)
                    .Customer
                    .Id;
            }
            else if (invoiceType == FBAInvoiceType.ShipOrder)
            {
                var customerCode = _context.FBAShipOrders.SingleOrDefault(x => x.ShipOrderNumber == reference).CustomerCode;

                customerId = _context.UpperVendors.SingleOrDefault(x => x.CustomerCode == customerCode).Id;
            }

            return customerId;
        }
    }

    public class ObjectBody
    {
        public string Content { get; set; }
    }

    public class InvoiceApi
    {
        public void CloseShipOrder(ApplicationDbContext context, FBAShipOrder shipOrderInDb, string _userName, string reference, string invoiceType, DateTime closeDate, bool isAppliedMinCharge)
        {
            shipOrderInDb.InvoiceStatus = FBAStatus.Closed;
            shipOrderInDb.CloseDate = closeDate;
            shipOrderInDb.ConfirmedBy = _userName;

            if (isAppliedMinCharge)
            {
                var invoiceDetailsInDb = shipOrderInDb.InvoiceDetails;

                var outboundMinCharge = context.UpperVendors
                    .SingleOrDefault(x => x.CustomerCode == shipOrderInDb.CustomerCode)
                    .OutboundMinCharge;

                //检查是否已经存在差价收费项目
                var priceDifference = invoiceDetailsInDb.SingleOrDefault(x => x.Activity == "Price Difference");
                float pd = 0;

                if (invoiceDetailsInDb.Count() == 0)
                {
                    pd = outboundMinCharge;
                }
                else
                {
                    pd = outboundMinCharge - (float)invoiceDetailsInDb.Sum(x => x.Amount);
                }

                if (priceDifference == null)
                {
                    //检查是否满足shiporder每单最小值,不满足的话则建立差价收费项目
                    if (pd > 0)
                    {
                        context.InvoiceDetails.Add(new InvoiceDetail
                        {
                            Activity = "Price Difference",
                            ChargingType = "Price Difference",
                            Unit = "N/A",
                            Quantity = 1,
                            Rate = Math.Round(pd, 2),
                            Amount = Math.Round(pd, 2),
                            InvoiceType = FBAInvoiceType.ShipOrder,
                            DateOfCost = DateTime.Now,
                            Operator = _userName,
                            FBAShipOrder = shipOrderInDb,
                            Memo = "MIN CHARGE"
                        });
                    }
                }
                //如果已经有收费项目，则刷新收费项目中的金额
                else
                {
                    var amount = invoiceDetailsInDb.Where(x => x.Activity != "Price Difference").Sum(x => x.Amount);
                    pd = outboundMinCharge - (float)amount;

                    //如果仍然有差额，则刷新差额收费
                    if (pd > 0)
                    {
                        priceDifference.Amount = Math.Round(pd, 2);
                        priceDifference.Rate = Math.Round(pd, 2);
                    }
                    //如果没有差额，则删除差额收费
                    else
                    {
                        context.InvoiceDetails.Remove(priceDifference);
                    }
                }
            }
        }

        public void CloseMasterOrder(FBAMasterOrder masterOrder,string _userName, DateTime closeDate)
        {
            masterOrder.InvoiceStatus = FBAStatus.Closed;
            masterOrder.CloseDate = closeDate;
            masterOrder.ConfirmedBy = _userName;
        }
    }
}
