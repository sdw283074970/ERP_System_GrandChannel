using ClothResorting.Helpers.FBAHelper;
using ClothResorting.Models;
using ClothResorting.Models.FBAModels;
using ClothResorting.Models.StaticClass;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Web.Http;
using ClothResorting.Helpers;
using AutoMapper;
using ClothResorting.Dtos.Fba;
using ClothResorting.Controllers.Api.Filters;
using ClothResorting.Models.FBAModels.StaticModels;

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBAShipmentAPIController : ApiController
    {
        private ApplicationDbContext _context;
        private FBAexAPIValidator _validator;
        private FBAInventoryPicker _picker;

        public FBAShipmentAPIController()
        {
            _context = new ApplicationDbContext();
            _validator = new FBAexAPIValidator();
            _picker = new FBAInventoryPicker();
        }

        // POST /api/FBAShipmentAPI/?customerCode={customerCode}&requestId={requestId}&version={version}
        [HttpPost]
        [ValidateModel]
        public async Task<IHttpActionResult> CreateOutboundOrderFromExternalRequest([FromUri]string appKey, [FromUri]string customerCode, [FromUri]string requestId, [FromUri]string version, [FromUri]string sign, [FromBody]FBAOutboundOrder order)
        {
            // 验证签名
            var customerInDb = _context.UpperVendors.SingleOrDefault(x => x.CustomerCode == customerCode);
            var jsonResult = _validator.ValidateSign(appKey, customerInDb, requestId, version, sign);

            if (jsonResult.Code != 200)
                return Ok(jsonResult);

            // 检查推送过来的Order的字段完整性,缺省的字段套用默认值
            var pickingStatus = await CreateShipOrderAsync(order, customerInDb, requestId);

            jsonResult.PickingStatus = new { Status = CheckStatus(pickingStatus) ? "Success" : "Failed", Details =  pickingStatus };

            return Created(Request.RequestUri, jsonResult);
        }

        public string GenerateShipOrderNumber(string customerCode, string shipOrderNumber)
        {
            if (shipOrderNumber != null && shipOrderNumber != "")
            {
                var shipOrderInDb = _context.FBAShipOrders.SingleOrDefault(x => x.ShipOrderNumber == shipOrderNumber);

                if (shipOrderInDb == null)
                {
                    return shipOrderNumber;
                }
            }

            var sequence = 1;
            var result = customerCode + "-" +  "OUTBOUND" + "-" + DateTime.Now.ToString("yyyyMMdd") + "-" + sequence.ToString();
            var outboundOrderInDb = _context.FBAShipOrders.SingleOrDefault(x => x.ShipOrderNumber == result);

            if (outboundOrderInDb == null)
            {
                return result;
            }
            else
            {
                sequence += 1;
                return customerCode + "-" + "OUTBOUND" + "-" + DateTime.Now.ToString("yyyyMMdd") + "-" + sequence.ToString();
            }
        }

        public  async Task<IList<PickingStatus>> CreateShipOrderAsync(FBAOutboundOrder order, UpperVendor customerInDb, string requestId)
        {
            var shipOrder = new FBAShipOrder { 
                OrderType = order.OrderType,
                CustomerCode = customerInDb.CustomerCode,
                Status = FBAStatus.Draft,
                CreateBy = "External API",
                ETS = DateTime.ParseExact(order.ETS, "yyyy-MM-dd", System.Globalization.CultureInfo.CurrentCulture),
                ETSTimeRange = order.EtsTimeRange,
                Destination = order.Destionation,
                CreateDate = DateTime.Now,
                ShipOrderNumber = GenerateShipOrderNumber(customerInDb.CustomerCode, order.ShipOrderNumber)
            };

            _context.FBAShipOrders.Add(shipOrder);

            var pickingStatusList = new List<PickingStatus>();
            var pickDetailCartonList = new List<FBAPickDetailCarton>();

            foreach (var p in order.PickingList)
            {
                var pickingStatus = new PickingStatus(shipOrder.ShipOrderNumber, p.Contianer, p.ShipmentId, p.AmzRefId, p.WarehouseCode, p.Quantity, p.PalletQuantity);

                var inventoryList = _picker.SearchPalletInventory(customerInDb.CustomerCode, p.Contianer, p.ShipmentId, p.AmzRefId, p.WarehouseCode);

                if (inventoryList.Count() == 0 || inventoryList.First().FBACartonLocations.Count() == 0)
                {
                    pickingStatus.StatusCode = 3002;
                    pickingStatus.Status = "Failed";
                    pickingStatus.Message = "Picking failed. No target was found in inventory.";
                    continue;
                }
                else if (inventoryList.Count() > 1)
                {
                    pickingStatus.StatusCode = 3001;
                    pickingStatus.Status = "Failed";
                    pickingStatus.Message = "Picking failed. More than one target found in inventory.";
                    continue;
                }

                var pltTarget = inventoryList.First();
                var ctnTarget = pltTarget.FBACartonLocations.First();
                var quantity = p.Quantity;

                if (ctnTarget.AvailableCtns < quantity)
                {
                    pickingStatus.StatusCode = 3003;
                    pickingStatus.Status = "Uncompleted";
                    pickingStatus.Message = "Picking uncompeleted. Not enough quantity was found in inventory.";
                    pickingStatus.PickedCtns = ctnTarget.AvailableCtns;
                    quantity = ctnTarget.AvailableCtns;
                }

                var palletLocationInDb = _context.FBAPalletLocations
                    .Include(x => x.FBAPallet.FBACartonLocations)
                    .Include(x => x.FBAMasterOrder)
                    .Include(x => x.FBAPallet.FBAPalletLocations)
                    .SingleOrDefault(x => x.Id == pltTarget.Id);

                var pickCartonDtoList = new List<PickCartonDto>();
                pickCartonDtoList.Add(new PickCartonDto { Id = ctnTarget.Id, PickQuantity = quantity });
                var pickDetail = _picker.CreateFBAPickDetailFromPalletLocation(palletLocationInDb, shipOrder, p.PalletQuantity, p.NewPallet, pickDetailCartonList, pickCartonDtoList, pickingStatus);

                pickingStatusList.Add(pickingStatus);
                pickDetail.PickableCtns = p.Quantity;
                _context.FBAPickDetailCartons.AddRange(pickDetailCartonList);
                _context.FBAPickDetails.Add(pickDetail);
            }

            var logger = new Logger(_context, order.Agency);

            await logger.AddCreatedLogAsync<FBAShipOrder>(null, Mapper.Map<FBAShipOrder, FBAShipOrderDto>(shipOrder), "Created by agency from api.", null, OperationLevel.Mediunm);

            var logInDb = _context.OperationLogs.OrderByDescending(x => x.Id).First();

            logInDb.RequestId = requestId;

            if (CheckStatus(pickingStatusList))
            {
                _context.SaveChanges();
            }

            return pickingStatusList;
        }

        private bool CheckStatus(IList<PickingStatus> pickingStatus)
        {
            foreach(var p in pickingStatus)
            {
                if (p.Status != "Success")
                {
                    return false;
                }
            }

            return true;
        }
    }

    public  class FBAOutboundOrder
    {
        public FBAOutboundOrder()
        {
            OrderType = FBAOrderType.Standard;
            EtsTimeRange = "NA";
            Destionation = "NA";
            Address = "NA";
        }

        [Required(ErrorMessage = "Agency is required.")]
        public string Agency { get; set; }

        public string ShipOrderNumber { get; set; }

        public string OrderType { get; set; }

        [Required(ErrorMessage = "Estimate time shipping is required.")]
        [RegularExpression(@"^\d{4}-\d{1,2}-\d{1,2}", ErrorMessage = "Must match DateTime format: yyyy-MM-dd")]
        public string ETS { get; set; }

        public string EtsTimeRange { get; set; }

        public string Destionation { get; set; }

        public string Address { get; set; }

        public IList<FBAPickingList> PickingList { get; set; }
    }

    public class FBAPickingList
    {
        public FBAPickingList()
        {
            PalletQuantity = 0;
            NewPallet = 0;
        }

        public string Contianer { get; set; }

        public string ShipmentId { get; set; }

        public string AmzRefId { get; set; }

        public string WarehouseCode { get; set; }

        [Required(ErrorMessage = "Ctn quantity is required.")]
        [Range(typeof(int), "0", "9999999")]
        public int Quantity { get; set; }

        [Range(typeof(int), "0", "9999999")]
        public int PalletQuantity { get; set; }

        [Range(typeof(int), "0", "9999999")]
        public int NewPallet { get; set; }
    }

    public class PickingStatus
    {
        public PickingStatus(string shipOrderNumber, string container, string shipmentId, string amzRefId, string warehouseCode, int estCtns, int estPlts)
        {
            ShipOrderNumber = shipOrderNumber;
            Contianer = container;
            ShipmentId = shipmentId;
            AmzRefId = amzRefId;
            WarehouseCode = warehouseCode;
            ESTCtns = estCtns;
            StatusCode = 2000;
            Status = "Success";
            NewPlts = 0;
            PickedCtns = 0;
            PickedPlts = 0;
            ESTPlts = estPlts;
            InstockCtns = 0;
            Message = "Success";
        }

        public String ShipOrderNumber { get; set; }

        public string Status { get; set; }

        public int  StatusCode { get; set; }

        public string Contianer { get; set; }

        public string ShipmentId { get; set; }

        public string AmzRefId { get; set; }

        public string WarehouseCode { get; set; }

        public int ESTCtns { get; set; }

        public int PickedCtns { get; set; }

        public int PickedPlts { get; set; }

        public int ESTPlts { get; set; }

        public int NewPlts { get; set; }

        public int InstockCtns { get; set; }

        public string Message { get; set; }
    }
}
