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
                .Include(x => x.FBAOrderDetails)
                .Include(x => x.FBAPallets)
                //.Where(x => x.Status == FBAStatus.Processing || x.Status == FBAStatus.Allocated)
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
                    UnloadTime = m.UnloadTime,
                    UpdateLog = m.UpdateLog
                };

                inboundLogList.Add(newLog);
            }

            //获取所有服装部待收货主单
            //

            return Ok(inboundLogList);
        }

        // PUT /api/warehouseinboundlog/?masterOrderId={masterOrderId}&operation={operation}
        public void UpdateMasterOrderFromWarehouse([FromUri]int masterOrderId, [FromUri]string operation, [FromBody]InboundLog log)
        {
            var orderInDb = _context.FBAMasterOrders
                .Include(x => x.FBAOrderDetails)
                .SingleOrDefault(x => x.Id == masterOrderId);

            if (operation == "Update")
            {
                orderInDb.InboundDate = log.InboundDate;
                orderInDb.UnloadTime = log.UnloadTime;
                orderInDb.AvailableTime = log.AvailableTime;
                orderInDb.OutTime = log.OutTime;
                orderInDb.DockNumber = log.DockNumber;

                _context.SaveChanges();
            }
            else if (operation == "Receive")
            {
                //检查以上5个信息是否都填了
                if (CheckIfAllFieldsAreFilled(orderInDb) && CheckIfCanBeReceived(orderInDb))
                {
                    orderInDb.Status = FBAStatus.Received;
                }
                else
                {
                    throw new Exception("All fields must be updated before confirming received.");
                }
            }
            else if (operation == "Finish")
            {
                if (CheckIfAllCtnsAreAllocated(orderInDb))
                {
                    orderInDb.Status = FBAStatus.Allocated;
                }
                else
                {
                    throw new Exception("All cartons must be allocated before marking this order finished.");
                }
            }
        }

        // PUT /api/warehouseinboundlog/?masterOrderId={masterOrder}

        bool CheckIfAllFieldsAreFilled(FBAMasterOrder orderInDb)
        {
            if (orderInDb.DockNumber != null && orderInDb.InboundDate.Year != 1900 && orderInDb.UnloadTime.Year != 1900 && orderInDb.AvailableTime.Year != 1900 && orderInDb.OutTime.Year != 1900)
                return true;

            return false;
        }

        bool CheckIfCanBeReceived(FBAMasterOrder orderInDb)
        {
            if (orderInDb.FBAOrderDetails.Count == 0)
                throw new Exception("Cannot mark this empty order received");

            if (orderInDb.FBAOrderDetails.Sum(x => x.ActualQuantity) != 0)
                return true;

            return false;
        }

        bool CheckIfAllCtnsAreAllocated(FBAMasterOrder orderInDb)
        {
            if (orderInDb.FBAOrderDetails.Count == 0)
                throw new Exception("Cannot mark this empty order allocated.");

            if (orderInDb.FBAOrderDetails.Sum(x => x.ComsumedQuantity) == orderInDb.FBAOrderDetails.Sum(x => x.ActualQuantity))
                return true;

            return false;
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

        public DateTime InboundDate { get; set; }

        public string DockNumber { get; set; }

        public string Container { get; set; }

        public int Ctns { get; set; }

        public int SKU { get; set; }

        public int OriginalPlts { get; set; }

        public string Carrier { get; set; }

        public string Lumper { get; set; }

        public string Instruction { get; set; }

        public string UpdateLog { get; set; }

        public DateTime PushTime { get; set; }

        public DateTime UnloadTime { get; set; }

        public DateTime AvailableTime { get; set; }

        public DateTime OutTime { get; set; }
    }

}
