using AutoMapper;
using ClothResorting.Controllers.Api.Filters;
using ClothResorting.Dtos.Fba;
using ClothResorting.Helpers;
using ClothResorting.Helpers.FBAHelper;
using ClothResorting.Models;
using ClothResorting.Models.FBAModels;
using ClothResorting.Models.FBAModels.StaticModels;
using ClothResorting.Models.StaticClass;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBAexAPIController : ApiController
    {
        private ApplicationDbContext _context;
        private FBAexAPIValidator _validator;

        public FBAexAPIController()
        {
            _context = new ApplicationDbContext();
            _validator = new FBAexAPIValidator();
        }

        // POST /api/FBAexAPI/?customerCode={customerCode}&requestId={requestId}&version={version}
        [HttpPost]
        [ValidateModel]
        public async Task<IHttpActionResult> CreateInboundOrderFromExternalRequest([FromUri]string appKey, [FromUri]string customerCode, [FromUri]string requestId, [FromUri]string version, [FromUri]string sign, [FromBody]FBAInboundOrder order)
        {
            // 检查Container是否重复，否则返回错误
            var masterOrderInDb = _context.FBAMasterOrders.SingleOrDefault(x => x.Container == order.Container);

            if (masterOrderInDb != null)
            {
                return Json(new JsonResponse { Code = 506, ValidationStatus = "Failed", Message = "Container No. " + order.Container + " already existed in system. Please report this contianer No. to CSR of Grand Channel for more support." });
            }

            var customerInDb = _context.UpperVendors.SingleOrDefault(x => x.CustomerCode == customerCode);

            var jsonResult = _validator.ValidateSign(appKey, customerInDb, requestId, version, sign);

            if (jsonResult.Code != 200)
                return Json(jsonResult);

            // 创建订单逻辑
            if (version == "V1")
            {
                //建立主单并记录成功的操作，写入日志
                await CreateInboundOrderByAgentRequestV1(customerInDb, customerCode, order, requestId);
                //建立分单并记录成功的操作，写入日志
                //await CreateOutboundOrdersByAgentRequestV1(customerCode, order, requestId);
                return Created(Request.RequestUri, new JsonResponse { Code = 200, ValidationStatus = "Success", Message = "Success!" });
            }

            return Ok(new JsonResponse { Code = 200, ValidationStatus = "Success", Message = "No operation applied." });
        }

        public async Task CreateInboundOrderByAgentRequestV1(UpperVendor customer, string customerCode, FBAInboundOrder order, string requestId)
        {
            // 建立主单
            var newMasterOrder = new FBAMasterOrder();

            newMasterOrder.GrandNumber = "N/A";
            newMasterOrder.Agency = order.Agency;
            newMasterOrder.Container = order.Container;
            newMasterOrder.CreatedBy = order.Agency;
            newMasterOrder.SubCustomer = order.Subcustomer;
            newMasterOrder.StorageType = "SEE INSTRUCTION";
            newMasterOrder.Status = FBAStatus.Draft;
            newMasterOrder.UnloadingType = "DROP-OFF";
            newMasterOrder.InboundType = "FCL";
            newMasterOrder.Palletizing = "<=80";
            newMasterOrder.TotalCBM = order.FBAJobs.Sum(x => x.CBM);
            newMasterOrder.TotalCtns = order.FBAJobs.Sum(x => x.Quantity);
            newMasterOrder.OriginalPlts = order.FBAJobs.Sum(x => x.PalletQuantity);
            newMasterOrder.CustomerCode = customerCode;
            newMasterOrder.WarehouseLocation = order.WarehouseLocation == "" ? "W0" : order.WarehouseLocation;
            newMasterOrder.Customer = customer;
            newMasterOrder.PortOfLoading = order.PortOfLoading;
            newMasterOrder.ETAPort = order.ETADate;
            newMasterOrder.ETA = order.ETADate;
            newMasterOrder.PlaceOfDelivery = order.DeliveryPort;
            newMasterOrder.Vessel = order.Vessel;
            newMasterOrder.Carrier = order.Carrier;
            newMasterOrder.ContainerSize = order.ContainerSize;
            newMasterOrder.SealNumber = order.SealNumber;
            newMasterOrder.Comment = "ETL DATE: " + order.ETLDate;
            newMasterOrder.UpdateLog = "Created by agency via API";

            _context.FBAMasterOrders.Add(newMasterOrder);

            foreach (var j in order.FBAJobs)
            {
                var orderDetail = new FBAOrderDetail();
                orderDetail.Container = order.Container;
                orderDetail.GrandNumber = "N/A";
                orderDetail.GrossWeight = j.GrossWeight;
                orderDetail.CBM = j.CBM;
                orderDetail.Quantity = j.Quantity;
                orderDetail.ShipmentId = j.ShipmentId;
                orderDetail.AmzRefId = j.AmzRefId;
                orderDetail.WarehouseCode = j.WarehouseCode;
                orderDetail.Remark = "包装：" + j.PackageType + "；产品类型：" + j.ProductType + "；打托数量：" + j.PalletQuantity;
                orderDetail.FBAMasterOrder = newMasterOrder;
                _context.FBAOrderDetails.Add(orderDetail);
            }

            var instruction = new ChargingItemDetail();

            instruction.Description = "To CSR: This inbound order is created by an agency from api. If there is no further customer's instructions below, please contact customer to do a further confirmation.";
            instruction.HandlingStatus = "N/A";
            instruction.Status = FBAStatus.NoNeedForCharging;
            instruction.IsCharging = false;
            instruction.IsInstruction = true;
            instruction.IsOperation = false;
            instruction.OriginalDescription = instruction.Description;
            instruction.FBAMasterOrder = newMasterOrder;
            _context.ChargingItemDetails.Add(instruction);

            if (order.Instructions != null)
            {
                foreach (var i in order.Instructions)
                {
                    var customerInstruction = new ChargingItemDetail();

                    customerInstruction.Description = i;
                    customerInstruction.HandlingStatus = "N/A";
                    customerInstruction.Status = FBAStatus.TBD;
                    customerInstruction.IsCharging = false;
                    customerInstruction.IsInstruction = true;
                    customerInstruction.IsOperation = false;
                    customerInstruction.OriginalDescription = instruction.Description;
                    customerInstruction.FBAMasterOrder = newMasterOrder;
                    _context.ChargingItemDetails.Add(customerInstruction);
                }
            }

            // 添加Request日志
            var logger = new Logger(_context, order.Agency);

            await logger.AddCreatedLogAsync<FBAMasterOrder>(null, Mapper.Map<FBAMasterOrder, FBAMasterOrderDto>(newMasterOrder), "Created by agency from api.", null, OperationLevel.Mediunm);

            var logInDb = _context.OperationLogs.OrderByDescending(x => x.Id).First();

            logInDb.RequestId = requestId;

            _context.SaveChanges();
        }

        public async Task CreateOutboundOrdersByAgentRequestV1(string customerCode, FBAInboundOrder order, string requestId)
        {
            // 添加Request日志
            var logger = new Logger(_context, order.Agency);

            var index = 1;

            foreach(var j in order.FBAJobs)
            {
                var shipOrder = new FBAShipOrder();

                shipOrder.ShipOrderNumber = order.Container + "-" + index + "-" + j.ShipmentId;
                shipOrder.CreateBy = order.Agency;
                shipOrder.Carrier = "Unknow";
                shipOrder.CustomerCode = customerCode;
                shipOrder.Agency = order.Agency;
                shipOrder.OrderType = "Standard";
                shipOrder.ETSTimeRange = "Unknow";
                shipOrder.Destination = j.WarehouseCode;
                shipOrder.SubCustomer = j.Subcustomer;
                shipOrder.Instruction = "This order is generated by api automatically. Some fields are pending. Please fill out them once all information is clear.";
                shipOrder.OperationLog = "Created by agency via API";

                _context.FBAShipOrders.Add(shipOrder);

                var instruction = new ChargingItemDetail();

                instruction.Description = "To CSR: Please pick the following quantity of sku in system: " + j.Quantity + j.PackageType + " of shipmentID: " + j.ShipmentId + ", amz Ref Id: " + j.AmzRefId + ", warehouse code: " + j.WarehouseCode;
                instruction.HandlingStatus = "N/A";
                instruction.Status = FBAStatus.NoNeedForCharging;
                instruction.IsCharging = false;
                instruction.IsInstruction = true;
                instruction.IsOperation = false;
                instruction.OriginalDescription = instruction.Description;
                instruction.FBAShipOrder = shipOrder;

                _context.ChargingItemDetails.Add(instruction);

                await logger.AddCreatedLogAsync<FBAShipOrder>(null, Mapper.Map<FBAShipOrder, FBAShipOrderDto>(shipOrder), "Created by agency from api.", null, OperationLevel.Mediunm);

                var logInDb = _context.OperationLogs.OrderByDescending(x => x.Id).First();

                logInDb.RequestId = requestId;

                index += 1;
            }

            _context.SaveChanges();
        }
    }

    public class FBAInboundOrder
    {
        [Required(ErrorMessage = "Agency is required.")]
        public string Agency { get; set; }

        public string MBL { get; set; }

        [Required(ErrorMessage = "Container number is required.")]
        public string Container { get; set; }

        public string Subcustomer { get; set; }

        public string PortOfLoading { get; set; }

        public string WarehouseLocation { get; set; }

        public string DeliveryPort { get; set; }

        [Required(ErrorMessage = "Estimate launch date is required.")]
        [RegularExpression(@"^\d{4}-\d{1,2}-\d{1,2}", ErrorMessage = "Must match DateTime format: yyyy-MM-dd")]
        public string ETLDate { get; set; }

        [Required(ErrorMessage = "Estimate arrive date is required.")]
        [RegularExpression(@"^\d{4}-\d{1,2}-\d{1,2}", ErrorMessage = "Must match DateTime format: yyyy-MM-dd")]
        public string ETADate { get; set; }

        [Required(ErrorMessage = "Carrier name is required.")]
        public string Carrier { get; set; }

        public string Vessel { get; set; }

        public string SealNumber { get; set; }

        [Required(ErrorMessage = "Container size is required.")]
        public string ContainerSize { get; set; }

        public IList<FBAJob> FBAJobs { get; set; }

        public IList<string> Instructions { get; set; }
    }

    public class FBAJob
    {
        [Required(ErrorMessage = "Shipment Id is required.")]
        public string ShipmentId { get; set; }

        [Required(ErrorMessage = "Amz Ref Id is required.")]
        public string AmzRefId { get; set; }

        public string ProductType { get; set; }

        public string Subcustomer { get; set; }

        [Required(ErrorMessage = "Warehouse coed is required.")]
        public string WarehouseCode { get; set; }

        [Required(ErrorMessage = "Ctn quantity is required.")]
        [Range(typeof(int), "0", "9999999")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Gross Weight(KG) is required.")]
        [Range(typeof(float), "0.00", "9999999.99")]
        public float GrossWeight { get; set; }

        [Required(ErrorMessage = "CBM is required.")]
        [Range(typeof(float), "0.00", "9999999.99")]
        public float CBM { get; set; }

        [Range(typeof(int), "0", "99999")]
        public int PalletQuantity { get; set; }

        public string PackageType { get; set; }
    }
}
