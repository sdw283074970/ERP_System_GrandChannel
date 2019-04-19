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

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBAInvoiceDetailController : ApiController
    {
        private ApplicationDbContext _context;
        private string _userName;

        public FBAInvoiceDetailController()
        {
            _context = new ApplicationDbContext();
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0];
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
                return Ok(_context.InvoiceDetails
                    .Include(x => x.FBAMasterOrder)
                    .Where(x => x.FBAMasterOrder.Container == reference)
                    .Select(Mapper.Map<InvoiceDetail, InvoiceDetailDto>));
            }
            else if (invoiceType == FBAInvoiceType.ShipOrder)
            {
                return Ok(_context.InvoiceDetails
                    .Include(x => x.FBAShipOrder)
                    .Where(x => x.FBAShipOrder.ShipOrderNumber == reference)
                    .Select(Mapper.Map<InvoiceDetail, InvoiceDetailDto>));
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
                case 0:        //ajax 第0步 获取该单号下的托盘总数和箱子总数
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

                            return Ok(new { Pallets = plts, Cartons = ctns, OriginalPallets = masterOrderInDb.OriginalPlts});
                        }
                        else if (invoiceType == "ShipOrder")
                        {
                            var shipOrder = _context.FBAPickDetails
                                .Include(x => x.FBAShipOrder)
                                .Where(x => x.FBAShipOrder.ShipOrderNumber == reference)
                                .ToList();

                            var plts = shipOrder.Sum(x => x.ActualPlts);
                            var ctns = shipOrder.Sum(x => x.ActualQuantity);

                            return Ok(new { Pallets = plts, Cartons = ctns, OriginalPallets = "N/A" });
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

        // GET /api/fba/FBAInvoiceDetail/?customerId={customerId}&itemName={itemName}    ajax3，获取所选择项目的费率和计价单位
        [HttpGet]
        public IHttpActionResult GetRate([FromUri]string reference, [FromUri]string invoiceType, [FromUri]string itemName)
        {
            var customerId = GetCustomerId(reference, invoiceType);

            var chargingItemInDb = _context.ChargingItems
                .Include(x => x.UpperVendor)
                .SingleOrDefault(x => x.Name == itemName
                    && x.UpperVendor.Id == customerId);

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
                Quantity = invoiceDetailIdInDb.Quantity,
                Memo = invoiceDetailIdInDb.Memo,
                Amount = invoiceDetailIdInDb.Amount,
                Unit = invoiceDetailIdInDb.Unit
            };

            return Ok(invoiceDetailDto);
        }

        // POST /api/fba/FBAInvoiceDetail/?reference={reference}&invoiceType={invoiceType}&description={description}
        [HttpPost]
        public IHttpActionResult GreateChargingItemRef([FromUri]string reference, [FromUri]string invoiceType, [FromUri]string description)
        {
            var detail = new ChargingItemDetail();

            if (invoiceType == FBAInvoiceType.MasterOrder)
            {
                var masterOrderInDb = _context.FBAMasterOrders.SingleOrDefault(x => x.Container == reference);

                var newDetail = new ChargingItemDetail {
                    Status = "Waiting for charging",
                    CreateBy = _userName,
                    CreateDate = DateTime.Now,
                    Description = description,
                    FBAMasterOrder = masterOrderInDb
                };

                detail = newDetail;
                _context.ChargingItemDetails.Add(detail);
            }
            else if (invoiceType == FBAInvoiceType.ShipOrder)
            {
                var shipOrderInDb = _context.FBAShipOrders.SingleOrDefault(x => x.ShipOrderNumber == reference);

                var newDetail = new ChargingItemDetail
                {
                    Status = "Waiting for charging",
                    CreateBy = _userName,
                    CreateDate = DateTime.Now,
                    Description = description,
                    FBAShipOrder = shipOrderInDb
                };

                detail = newDetail;
                _context.ChargingItemDetails.Add(detail);
            }

            _context.SaveChanges();

            return Created(Request.RequestUri + "/", Mapper.Map<ChargingItemDetail, ChargingItemDetailDto>(detail));
        }

        // POST /api/fba/FBAInvoiceDetail/?reference={reference}&invoiceType={invoiceType}
        [HttpPost]
        public IHttpActionResult GreateChargingItem([FromUri]string reference, [FromUri]string invoiceType, [FromBody]InvoiceDetailJsonObj obj)
        {
            var invoice = new InvoiceDetail();

            if (invoiceType == FBAInvoiceType.MasterOrder)
            {
                var masterOrderInDb = _context.FBAMasterOrders.SingleOrDefault(x => x.Container == reference);

                var invoiceDetail = new InvoiceDetail {
                    Activity = obj.Activity,
                    ChargingType = obj.ChargingType,
                    DateOfCost = obj.DateOfCost,
                    Memo = obj.Memo,
                    Unit = obj.Unit,
                    Rate = obj.Rate,
                    Cost = obj.Cost,
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
                    Memo = obj.Memo,
                    Unit = obj.Unit,
                    Rate = obj.Rate,
                    Cost = obj.Cost,
                    Quantity = obj.Quantity,
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

        // PUT /api/fba/fbainvoicedetail/?chargingItemDetailId={chargingItemDetailId}
        [HttpPut]
        public void ChangeStatus([FromUri]int chargingItemDetailId)
        {
            var detailInDb = _context.ChargingItemDetails.Find(chargingItemDetailId);

            if (detailInDb.Status == "Waiting for charging")
            {
                detailInDb.Status = "Charged";
            }
            else
            {
                detailInDb.Status = "Waiting for charging";
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
            invoiceDetailInDb.Memo = obj.Memo;
            invoiceDetailInDb.Operator = _userName;
            invoiceDetailInDb.Quantity = obj.Quantity;
            invoiceDetailInDb.Rate = obj.Rate;
            invoiceDetailInDb.Unit = obj.Unit;

            _context.SaveChanges();
        }

        // PUT /api/fba/fbainvoicedetail/?reference{reference}&invoiceType={invoiceType}&closeDate={closeDate}
        [HttpPut]
        public void CloseChargeOrder([FromUri]string reference, [FromUri]string invoiceType, [FromUri]DateTime closeDate)
        {
            if (invoiceType == FBAInvoiceType.ShipOrder)
            {
                var shipOrderInDb = _context.FBAShipOrders
                    .SingleOrDefault(x => x.ShipOrderNumber == reference);

                shipOrderInDb.InvoiceStatus = "Closed";
                shipOrderInDb.CloseDate = closeDate;
                shipOrderInDb.ConfirmedBy = _userName;
            }
            else if (invoiceType == FBAInvoiceType.MasterOrder)
            {
                var masterOrder = _context.FBAMasterOrders
                    .SingleOrDefault(x => x.Container == reference);

                masterOrder.InvoiceStatus = "Closed";
                masterOrder.CloseDate = closeDate;
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
            var detailInDb = _context.ChargingItemDetails.Find(chargingItemDetailId);

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
}
