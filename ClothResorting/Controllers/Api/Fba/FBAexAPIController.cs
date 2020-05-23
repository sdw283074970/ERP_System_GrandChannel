using AutoMapper;
using ClothResorting.Controllers.Api.Filters;
using ClothResorting.Dtos.Fba;
using ClothResorting.Helpers;
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

        public FBAexAPIController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/FBAexAPI/?customerCode={customerCode}&requestId={requestId}&version={version}
        [HttpPost]
        [ValidateModel]
        public async Task<IHttpActionResult> CreateInboundOrderAndOutboundOrdersFromExternalRequest([FromUri]string appKey, [FromUri]string customerCode, [FromUri]string requestId, [FromUri]string version, [FromUri]string sign, [FromBody]FBAAgentOrder order)
        {
            // 参数验证加密验证
            var auth = _context.AuthAppInfos.SingleOrDefault(x => x.AppKey == appKey);
            if (auth == null)
            {
                return Json(new JsonResponse { Code = 500, ResultStatus = "Failed", Message = "Unregistered app request." });
            }
            var vs = auth.SecretKey.ToUpper() + "&appKey=" + appKey + "&customerCode=" + customerCode + "&requestId=" + requestId + "&version=" + version;
            var md5sign = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.Default.GetBytes(vs))).Replace("-", "S");

            if (md5sign != sign)
            {
                return Json(new JsonResponse { Code = 501, ResultStatus = "Failed", Message = "Invalid sign." });

            }

            // 检查customerCode是否存在，否则返回错误
            var customerInDb = _context.UpperVendors.SingleOrDefault(x => x.CustomerCode == customerCode);
            if (customerInDb == null)
            {
                return Json(new JsonResponse { Code = 501, ResultStatus = "Failed", Message = "Invalid sign." });
            }

            // 检查request body中的必填项

            // 防止重放攻击和网络延迟等非攻击意向的二次请求，如请求重复则返回错误
            if (StaticCollections.RequestIds.Contains(requestId))
            {
                return Json(new JsonResponse { Code = 504, ResultStatus = "Failed", Message = "Duplicated request detectived. Request Id: " + requestId + " has already been processed. Please report this request Id: " + requestId + " to CSR of Grand Channel for more support." });
            }
            else
            {
                StaticCollections.RequestIds.Add(requestId);
            }

            // 防止重放攻击的二道关卡，如重复则返回错误
            var logInDb = _context.OperationLogs.Where(x => x.RequestId == requestId);
            if (logInDb.Count() != 0)
            {
                return Json(new JsonResponse { Code = 504, ResultStatus = "Failed", Message = "Duplicated request detectived. Request Id: " + requestId + " has already been processed. Please report this request Id: " + requestId + " to CSR of Grand Channel for more support." });
            }

            // 检查version是否支持，否则返回错误
            if (version != "V1")
            {
                return Json(new JsonResponse { Code = 505, ResultStatus = "Failed", Message = "Invalid API version." });
            }

            // 检查Container是否重复，否则返回错误
            var masterOrderInDb = _context.FBAMasterOrders.SingleOrDefault(x => x.Container == order.Container);

            if (masterOrderInDb != null)
            {
                return Json(new JsonResponse { Code = 506, ResultStatus = "Failed", Message = "Container No. " + order.Container + " already existed in system. Please report this contianer No. to CSR of Grand Channel for more support." });
            }

            // 创建订单逻辑
            if (version == "V1")
            {
                //建立主单并记录成功的操作，写入日志
                await CreateInboundOrderByAgentRequestV1(customerInDb, customerCode, order, requestId);
                //建立分单并记录成功的操作，写入日志
                await CreateOutboundOrdersByAgentRequestV1(customerCode, order, requestId);
                return Created(Request.RequestUri, new JsonResponse { Code = 200, ResultStatus = "Success", Message = "Success!" });
            }

            return Ok(new JsonResponse { Code = 200, ResultStatus = "Success", Message = "No operation applied." });
        }

        public async Task CreateInboundOrderByAgentRequestV1(UpperVendor customer, string customerCode, FBAAgentOrder order, string requestId)
        {
            // 建立主单
            var newMasterOrder = new FBAMasterOrder();

            newMasterOrder.GrandNumber = "N/A";
            newMasterOrder.Container = order.Container;
            newMasterOrder.CreatedBy = order.Agency;
            newMasterOrder.SubCustomer = order.Subcustomer;
            newMasterOrder.StorageType = "SEE INSTRUCTION";
            newMasterOrder.UnloadingType = "DROP-OFF";
            newMasterOrder.InboundType = "FCL";
            newMasterOrder.Palletizing = "<=80";
            newMasterOrder.TotalCBM = order.FBAJobs.Sum(x => x.CBM);
            newMasterOrder.TotalCtns = order.FBAJobs.Sum(x => x.Quantity);
            newMasterOrder.OriginalPlts = order.FBAJobs.Sum(x => x.PalletQuantity);
            newMasterOrder.CustomerCode = customerCode;
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

            instruction.Description = "To CSR: This inbound order is created by an agency from api. Each SKU in this inbound order corresponds to a newly created ship order and these newly ship orders have been created automatically. Please complete the shipping order corresponding to each SKU after the inbound order is physically confirmed.";
            instruction.HandlingStatus = "N/A";
            instruction.Status = FBAStatus.NoNeedForCharging;
            instruction.IsCharging = false;
            instruction.IsInstruction = true;
            instruction.IsOperation = false;
            instruction.OriginalDescription = instruction.Description;
            instruction.FBAMasterOrder = newMasterOrder;

            _context.ChargingItemDetails.Add(instruction);

            // 添加Request日志
            var logger = new Logger(_context, order.Agency);
            
            await logger.AddCreatedLogAsync<FBAMasterOrder>(null, Mapper.Map<FBAMasterOrder, FBAMasterOrderDto>(newMasterOrder), "Created by agency from api.", null, OperationLevel.Mediunm);

            var logInDb = _context.OperationLogs.OrderByDescending(x => x.Id).First();

            logInDb.RequestId = requestId;

            _context.SaveChanges();
        }

        public async Task CreateOutboundOrdersByAgentRequestV1(string customerCode, FBAAgentOrder order, string requestId)
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

    public class FBAAgentOrder
    {
        [Required(ErrorMessage = "Agency is required.")]
        public string Agency { get; set; }

        public string MBL { get; set; }

        [Required(ErrorMessage = "Container number is required.")]
        public string Container { get; set; }

        public string Subcustomer { get; set; }

        public string PortOfLoading { get; set; }

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
        [Range(typeof(int), "0", "99999")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Gross Weight(KG) is required.")]
        [Range(typeof(float), "0.00", "99999.99")]
        public float GrossWeight { get; set; }

        [Required(ErrorMessage = "CBM is required.")]
        [Range(typeof(float), "0.00", "99999.99")]
        public float CBM { get; set; }

        [Range(typeof(int), "0", "99999")]
        public int PalletQuantity { get; set; }

        public string PackageType { get; set; }
    }
}
