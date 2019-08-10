using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Data.Entity;
using ClothResorting.Models.FBAModels.StaticModels;
using AutoMapper;
using ClothResorting.Models.FBAModels;
using ClothResorting.Models.StaticClass;
using ClothResorting.Dtos.Fba;

namespace ClothResorting.Controllers.Api.Warehouse
{
    public class WarehouseInboundLogController : ApiController
    {
        private ApplicationDbContext _context;

        public WarehouseInboundLogController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/warehouseinboundlog/
        [HttpGet]
        public IHttpActionResult GetAllProcessingMasterOrders()
        {
            //获取FBA部门的所有待收货主单
            var masterOrders = _context.FBAMasterOrders
                .Include(x => x.Customer)
                .Include(x => x.FBAOrderDetails.Select(c => c.FBACartonLocations))
                .Include(x => x.FBAPallets)
                .Where(x => x.Status != FBAStatus.NewCreated && x.Status != "Old Order")
                .ToList();

            var inboundLogList = new List<InboundLog>();

            foreach (var m in masterOrders)
            {
                var newLog = new InboundLog {
                    Id = m.Id,
                    Status = m.Status,
                    Department = DepartmentCode.FBA,
                    Customer = m.Customer.CustomerCode,
                    InboundType = m.InboundType,
                    ETA = m.ETA,
                    InboundDate = m.InboundDate,
                    DockNumber = m.DockNumber,
                    Container = m.Container,
                    Ctns = m.FBAOrderDetails.Sum(x => x.Quantity),
                    SKU = m.FBAOrderDetails.GroupBy(x => x.ShipmentId).Count(),
                    OriginalPlts = m.OriginalPlts,
                    Carrier = m.Carrier,
                    Lumper = m.Lumper,
                    Instruction = m.Instruction,
                    PushTime = m.PushTime,
                    AvailableTime = m.AvailableTime,
                    OutTime = m.OutTime,
                    UnloadFinishTime = m.UnloadFinishTime,
                    UnloadStartTime = m.UnloadStartTime,
                    UpdateLog = m.UpdateLog,
                    VerifiedBy = m.VerifiedBy
                };

                inboundLogList.Add(newLog);
            }

            //获取所有服装部待收货主单
            //

            return Ok(inboundLogList);
        }

        // GET /api/warehouseinboundlog/?masterOrderId={masterOrderId}
        [HttpGet]
        public IHttpActionResult GetInboundLog([FromUri]int masterOrderId)
        {
            var masterOrderInDb = _context.FBAMasterOrders.Find(masterOrderId);

            var log = new InboundLog {
                InboundDate = masterOrderInDb.InboundDate,
                UnloadFinishTime = masterOrderInDb.UnloadFinishTime,
                AvailableTime = masterOrderInDb.AvailableTime,
                OutTime = masterOrderInDb.OutTime,
                DockNumber = masterOrderInDb.DockNumber,
                VerifiedBy = masterOrderInDb.VerifiedBy
            };

            return Ok(log);
        }

        // PUT /api/warehouseinboundlog/?masterOrderId={masterOrder}&operationDate={operationDate}&operation={operation}
        [HttpPut]
        public void ChangeInboundLogStatus([FromUri]int masterOrderId, [FromUri]DateTime operationDate, [FromUri]string operation)
        {
            var masterOrderInDb = _context.FBAMasterOrders
                .Include(x => x.FBAPallets)
                .Include(x => x.FBAOrderDetails)
                .Include(x => x.ChargingItemDetails)
                .SingleOrDefault(x => x.Id == masterOrderId);

            if (operation == "MarkInboundDate")
            {
                masterOrderInDb.InboundDate = operationDate;
                masterOrderInDb.Status = FBAStatus.Arrived;
            }
            else if (operation == "Start")
            {
                masterOrderInDb.UnloadStartTime = operationDate;
                masterOrderInDb.Status = FBAStatus.Processing;
            }
            else if (operation  == "Register")
            {
                masterOrderInDb.Status = FBAStatus.Registered;
            }
            else if (operation == "Allocate")
            {
                if (CheckIfAllCtnsAreAllocated(masterOrderInDb))
                    masterOrderInDb.Status = FBAStatus.Allocated;
                else
                    throw new Exception("Failed. Please ensure that all the plts and ctns are allocated.");
            }
            _context.SaveChanges();
        }

        // PUT /api/warehouseinboundlog/?masterOrderId={masterOrderId}
        [HttpPut]
        public void UpdateMasterOrderFromWarehouse([FromUri]int masterOrderId, [FromUri]string operation, [FromBody]InboundLog log)
        {
            var orderInDb = _context.FBAMasterOrders
                .Include(x => x.FBAOrderDetails)
                .Include(x => x.ChargingItemDetails)
                .SingleOrDefault(x => x.Id == masterOrderId);

            orderInDb.InboundDate = log.InboundDate;
            orderInDb.UnloadFinishTime = log.UnloadFinishTime;
            orderInDb.AvailableTime = log.AvailableTime;
            orderInDb.OutTime = log.OutTime;
            orderInDb.DockNumber = log.DockNumber;
            orderInDb.VerifiedBy = log.VerifiedBy;

            if (operation == "Report")
            {
                //以上5个信息是都填了且收货数量大于0且所有命令都被执行才能被完整确认收货
                if (CheckIfAllFieldsAreFilled(orderInDb) && CheckIfCanBeReceived(orderInDb) && CheckIfAllOrdersAreFinished(orderInDb))
                {
                    orderInDb.Status = FBAStatus.Received;
                }
                else
                {
                    throw new Exception("Inbound date, Finish time, Dock # and Verified by must be updated before submit unloading report.");
                }
            }

            _context.SaveChanges();
        }

        bool CheckIfAllFieldsAreFilled(FBAMasterOrder orderInDb)
        {
            if (orderInDb.VerifiedBy != null && orderInDb.DockNumber != null && orderInDb.InboundDate.Year != 1900 && orderInDb.UnloadFinishTime.Year != 1900)
                return true;

            return false;
        }

        bool CheckIfAllOrdersAreFinished(FBAMasterOrder orderInDb)
        {
            if (orderInDb.ChargingItemDetails.Where(x => x.HandlingStatus !=  FBAStatus.Finished).Any())
                throw new Exception("Failed. Please ensure that all instructions in the work order have been completed.");

            return true;
        }

        bool CheckIfCanBeReceived(FBAMasterOrder orderInDb)
        {
            if (orderInDb.FBAOrderDetails.Count == 0)
                throw new Exception("Cannot mark this empty order received");

            if (orderInDb.FBAOrderDetails.Sum(x => x.ActualQuantity) > 0)
                return true;
            else
                throw new Exception("Cannot mark this 0 received order received");
        }

        bool CheckIfAllCtnsAreAllocated(FBAMasterOrder orderInDb)
        {
            var result = false;

            if (orderInDb.FBAOrderDetails.Count == 0)
                throw new Exception("Cannot mark this empty order allocated.");

            if (orderInDb.FBAOrderDetails.Sum(x => x.ComsumedQuantity) == orderInDb.FBAOrderDetails.Sum(x => x.ActualQuantity))
                result = true;

            if (orderInDb.FBAPallets.Any() && orderInDb.FBAPallets.Sum(x => x.ComsumedPallets) != orderInDb.FBAPallets.Sum(x => x.ActualPallets))
                result = false;

            return result;
        }
    }

    public class InboundLog
    {
        public int Id { get; set; }

        public string Status { get; set; }

        public string Department { get; set; }

        public string Customer { get; set; }

        public string InboundType { get; set; }

        public string ETA { get; set; }

        public string Container { get; set; }

        public int Ctns { get; set; }

        public int SKU { get; set; }

        public int OriginalPlts { get; set; }

        public string Carrier { get; set; }

        public string Lumper { get; set; }

        public string Instruction { get; set; }

        public string UpdateLog { get; set; }

        public DateTime PushTime { get; set; }

        public DateTime UnloadStartTime { get; set; }

        public float LogonProgress { get; set; }

        public float RegisterProgress { get; set; }

        public float AllocationProgress { get;set;}

        public DateTime AvailableTime { get; set; }

        public DateTime InboundDate { get; set; }

        public string DockNumber { get; set; }

        public DateTime OutTime { get; set; }

        public DateTime UnloadFinishTime { get; set; }

        public string VerifiedBy { get; set; }
    }

}
