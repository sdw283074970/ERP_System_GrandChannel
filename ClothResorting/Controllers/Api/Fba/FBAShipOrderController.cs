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

            return Ok();
        }

        // POST /api/fba/fbashiporder/
        [HttpPost]
        public IHttpActionResult CreateNewShipOrder([FromBody]ShipOrderDto obj)
        {
            if (_context.FBAShipOrders.SingleOrDefault(x => x.ShipOrderNumber == obj.ShipOrderNumber) != null)
            {
                throw new Exception("Ship Order Number " + obj.ShipOrderNumber + " has been taken. Please delete the existed order and try agian.");
            }

            var shipOrder = new FBAShipOrder();

            shipOrder.AssembleBaseInfo(obj.ShipOrderNumber, obj.CustomerCode, obj.OrderType, obj.Destination, obj.PickReference);
            shipOrder.CreateBy = _userName;
            shipOrder.BOLNumber = obj.BOLNumber;
            shipOrder.Carrier = obj.Carrier;
            shipOrder.ETS = obj.ETS;
            shipOrder.ETSTimeRange = obj.TimeRange;
            shipOrder.PickNumber = obj.PickNumber;
            shipOrder.PurchaseOrderNumber = obj.PurchaseOrderNumber;
            shipOrder.Instruction = obj.Instruction;

            _context.FBAShipOrders.Add(shipOrder);
            _context.SaveChanges();

            var sampleDto = Mapper.Map<FBAShipOrder, FBAShipOrderDto>(_context.FBAShipOrders.OrderByDescending(x => x.Id).First());

            return Created(Request.RequestUri + "/" + sampleDto.Id, sampleDto);
        }

        // PUT /api/fba/fbashiporder/?shipOrderId={shipOrderId}
        [HttpPut]
        public void UpdateShipOrder([FromUri]int shipOrderId, [FromBody]ShipOrderDto obj)
        {
            var shipOrderInDb = _context.FBAShipOrders.Find(shipOrderId);

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

            _context.SaveChanges();
        }

        // PUT /api/fba/fbashiporder/?shipOrderId={shipOrderId}&shipDate={shipDate}&operation={operation}
        [HttpPut]
        public void ChangeShipOrderStatus([FromUri]int shipOrderId, [FromUri]DateTime shipDate, [FromUri]string operation)
        {
            var shipOrderInDb = _context.FBAShipOrders
                .Include(x => x.FBAPickDetails)
                .SingleOrDefault(x => x.Id == shipOrderId);

            //当操作类型为调整订单状态时
            if (operation == FBAOperation.ChangeStatus)
            {
                //新建的空单和正在被仓库处理的单都能转换成Ready状态
                if (shipOrderInDb.Status == FBAStatus.NewCreated || shipOrderInDb.Status == FBAStatus.Processing)
                {
                    shipOrderInDb.Status = FBAStatus.Ready;
                    shipOrderInDb.ReadyBy = _userName;
                }
                //如果订单未在拣状态，则转换为给仓库的新订单状态
                else if (shipOrderInDb.Status == FBAStatus.Picking)
                {
                    shipOrderInDb.Status = FBAStatus.NewOrder;
                    shipOrderInDb.PlacedBy = _userName;
                }
                //如果订单未准备状态，则点准备状态变回Processing状态（如果是空单则不会返回给仓库）
                else if (shipOrderInDb.Status == FBAStatus.Ready)
                {
                    shipOrderInDb.Status = FBAStatus.Picking;
                    shipOrderInDb.ReadyBy = "Cancelled by " + _userName;
                }
            }
            //当操作类型为发货且状态为Release的情况下才从库存实际扣除
            else if (operation == FBAOperation.ShipOrder && shipOrderInDb.Status == FBAStatus.Released)
            {
                if (shipOrderInDb.FBAPickDetails.Count() == 0)
                {
                    shipOrderInDb.Status = FBAStatus.Shipped;
                    shipOrderInDb.ShipDate = shipDate;
                    shipOrderInDb.ShippedBy = _userName;
                }
                else
                {
                    foreach (var pickDetailInDb in shipOrderInDb.FBAPickDetails)
                    {
                        ShipPickDetail(_context, pickDetailInDb.Id);
                    }
                    shipOrderInDb.ShippedBy = _userName;
                    shipOrderInDb.ShipDate = shipDate;
                    shipOrderInDb.Status = FBAStatus.Shipped;
                }
            }

            _context.SaveChanges();
        }

        // PUT /api/fba/fbashiporder/?shipOrderId={shipOrderId}&shipDate={shipDate}
        [HttpPut]
        public void MarkShipTime([FromUri]int shipOrderId, [FromUri]DateTime shipDate)
        {
            var shipOrderInDb = _context.FBAShipOrders.Find(shipOrderId);

            //如果已经发货了，则报错
            if (shipOrderInDb.ShipDate.ToString("yyyy-MM-dd") != "1900-01-01")
            {
                throw new Exception("Cannot override existed date.");
            }
            //只有当订单状态为ready或New Created的时候才能release并填上日期
            else if (shipOrderInDb.Status == FBAStatus.Ready || shipOrderInDb.Status == FBAStatus.NewCreated)
            {
                shipOrderInDb.ShipDate = shipDate;
                shipOrderInDb.Status = FBAStatus.Released;
                shipOrderInDb.ReleasedBy = _userName;
            }

            _context.SaveChanges();
        }

        // DELETE /api/fba/fbashiporder/?shipOrderId={shipOrderId}
        [HttpDelete]
        public void DeleteShipOrder([FromUri]int shipOrderId)
        {
            var pickDetailsInDb = _context.FBAPickDetails
                .Include(x => x.FBAShipOrder)
                .Where(x => x.FBAShipOrder.Id == shipOrderId);

            var shipOrderInDb = _context.FBAShipOrders.Find(shipOrderId);

            var fbaPickDetailAPI = new FBAPickDetailController();

            foreach(var detail in pickDetailsInDb)
            {
                fbaPickDetailAPI.RemovePickDetail(_context, detail.Id);
            }

            var chargingItemDetailsInDb = _context.ChargingItemDetails
                .Include(x => x.FBAShipOrder)
                .Where(x => x.FBAShipOrder.Id == shipOrderId);

            var invoiceDetailsInDb = _context.InvoiceDetails
                .Include(x => x.FBAShipOrder)
                .Where(x => x.FBAShipOrder.Id == shipOrderId);

            _context.ChargingItemDetails.RemoveRange(chargingItemDetailsInDb);
            _context.InvoiceDetails.RemoveRange(invoiceDetailsInDb);
            _context.FBAShipOrders.Remove(shipOrderInDb);
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
                pickDetailInDb.FBAPalletLocation.ShippedPlts += pickDetailInDb.ActualPlts;
                pickDetailInDb.FBAPalletLocation.PickingPlts -= pickDetailInDb.ActualPlts;

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
                            plt = pickDetail.ActualPlts;
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
}
