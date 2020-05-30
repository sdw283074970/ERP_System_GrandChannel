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
using ClothResorting.Dtos;

namespace ClothResorting.Controllers.Api.Warehouse
{
    public class WarehouseIndexController : ApiController
    {
        private ApplicationDbContext _context;
        private string _userName;

        public WarehouseIndexController()
        {
            _context = new ApplicationDbContext();
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0] == "" ? (HttpContext.Current.Request.Headers.Get("AppUser") == null ? "" : HttpContext.Current.Request.Headers.Get("AppUser")) : HttpContext.Current.User.Identity.Name.Split('@')[0];
        }

        // GET /api/warehouseIndex/?operation
        [HttpGet]
        public IHttpActionResult GetAllWarehouseOrder([FromUri]string operation)
        {
            //将FBA运单转成outbound work order
            var list = new List<WarehouseOutboundLog>();

            var ordersInDb = _context.FBAShipOrders
                .Include(x => x.FBAPickDetails)
                .Where(x => x.Status != FBAStatus.NewCreated && x.Status != FBAStatus.Picking && x.Status != FBAStatus.Shipped && x.Status != FBAStatus.Draft);

            if (operation == FBAStatus.NewOrder)
            {
                ordersInDb = ordersInDb.Where(x => x.Status == FBAStatus.NewOrder);
            }
            else if (operation == FBAStatus.Processing)
            {
                ordersInDb = ordersInDb.Where(x => x.Status == FBAStatus.Processing);
            }
            else if (operation == FBAStatus.Ready)
            {
                ordersInDb = ordersInDb.Where(x => x.Status == FBAStatus.Ready);
            }
            else if (operation == FBAStatus.Released)
            {
                ordersInDb = ordersInDb.Where(x => x.Status == FBAStatus.Released);
            }

            foreach (var o in ordersInDb)
            {
                var order = Mapper.Map<FBAShipOrder, WarehouseOutboundLog>(o);

                order.Department = "FBA";
                order.WarehouseOrderType = o.OrderType == FBAOrderType.Adjustment ? FBAOrderType.Adjustment : FBAOrderType.Outbound;
                order.ETS = o.ETS.ToString("yyyy-MM-dd") + " " + o.ETSTimeRange;
                order.TotalCtns = o.FBAPickDetails.Sum(x => x.ActualQuantity);
                order.TotalPlts = o.FBAPickDetails.Sum(x => x.ActualPlts);
                order.ShipDate = o.ShipDate;
                order.ReleaseTime = o.ReleasedDate;
                order.OrderNumber = o.ShipOrderNumber;
                order.BatchNumber = o.BatchNumber;

                list.Add(order);
            }

            return Ok(list);
        }

        // PUT /api/warehouseIndex/?shipOrderId={shipOrderId}&pickMan={pickMan}&instructor={instructor}&location={location}&operation={operation}
        [HttpPut]
        public void UpdateAndSave([FromUri]int shipOrderId, [FromUri]string pickMan, [FromUri]string instructor, [FromUri]string location, [FromUri]string operation)
        {
            var shipOrderInDb = _context.FBAShipOrders
                .Include(x => x.ChargingItemDetails)
                .Include(x => x.FBAPickDetails)
                .SingleOrDefault(x => x.Id == shipOrderId);

            UpdateWOInfo(shipOrderInDb, pickMan, instructor, location);

            if (IsPending(shipOrderInDb))
            {
                shipOrderInDb.Status = FBAStatus.Pending;
            }

            if (operation == "Save&Ready")
            {
                shipOrderInDb.ReadyTime = DateTime.Now;
                shipOrderInDb.ReadyBy = _userName;

                if ((shipOrderInDb.OrderType == FBAOrderType.Standard && shipOrderInDb.FBAPickDetails.Count > 0 && shipOrderInDb.FBAPickDetails.Sum(x => x.ActualPlts) == 0))
                {
                    throw new Exception("Cannot ready for now. The actual outbound plts of a standard ship order cannot be 0. Please go and adjust actual plts first.");
                }

                if (!IsAllowedToReady(shipOrderInDb))
                {
                    throw new Exception("Cannot ready for now. You must finish all the instruction in this work order.");
                }
                else if (IsPending(shipOrderInDb))
                {
                    shipOrderInDb.Status = FBAStatus.Pending;
                    shipOrderInDb.OperationLog = "Submited by " + _userName;
                }
                else
                {
                    shipOrderInDb.Status = FBAStatus.Ready;
                    shipOrderInDb.OperationLog = "Ready by " + _userName;
                }
            }

            _context.SaveChanges();
        }

        // PUT /api/warehouseIndex/?chargingItemDetailId={chargingItemDetailId}
        [HttpPut]
        public void ConfirmAndFinishInstruction([FromUri]int chargingItemDetailId)
        {
            var detailInDb = _context.ChargingItemDetails
                .Include(x => x.FBAMasterOrder.ChargingItemDetails)
                .Include(x => x.FBAShipOrder.ChargingItemDetails)
                .SingleOrDefault(x => x.Id == chargingItemDetailId);

            if (detailInDb.HandlingStatus == FBAStatus.Confirmed)
                detailInDb.HandlingStatus = FBAStatus.Finished;
            else
                detailInDb.HandlingStatus = FBAStatus.Confirmed;

            detailInDb.ConfirmedBy = _userName;

            if (detailInDb.FBAShipOrder != null)
            {
                var originalStatus = (detailInDb.FBAShipOrder.Status == FBAStatus.Pending || detailInDb.FBAShipOrder.Status == FBAStatus.Updated) ? FBAStatus.Processing : detailInDb.FBAShipOrder.Status;
                if (detailInDb.FBAShipOrder.ChargingItemDetails.Where(x => x.HandlingStatus == FBAStatus.Updated).Any())
                    detailInDb.FBAShipOrder.Status = FBAStatus.Updated;
                else if (detailInDb.FBAShipOrder.ChargingItemDetails.Where(x => x.HandlingStatus == FBAStatus.Pending).Any())
                    detailInDb.FBAShipOrder.Status = FBAStatus.Pending;
                else
                    detailInDb.FBAShipOrder.Status = originalStatus;
            }
            else
            {
                var originalStatus = (detailInDb.FBAMasterOrder.Status == FBAStatus.Pending || detailInDb.FBAMasterOrder.Status == FBAStatus.Updated) ? FBAStatus.Processing : detailInDb.FBAMasterOrder.Status;
                if (detailInDb.FBAMasterOrder.ChargingItemDetails.Where(x => x.HandlingStatus == FBAStatus.Updated).Any())
                    detailInDb.FBAMasterOrder.Status = FBAStatus.Updated;
                else if (detailInDb.FBAMasterOrder.ChargingItemDetails.Where(x => x.HandlingStatus == FBAStatus.Pending).Any())
                    detailInDb.FBAMasterOrder.Status = FBAStatus.Pending;
                else
                    detailInDb.FBAMasterOrder.Status = originalStatus;
            }

            _context.SaveChanges();
        }

        // PUT /api/warehouserIndex/?shipOrderId={shipOrderId}&operation={operation}
        [HttpPut]
        public void UpdateOrderProcess([FromUri]int shipOrderId, [FromUri]string operation, [FromBody]WarehouseOutboundLog report)
        {
            var shipOrderInDb = _context.FBAShipOrders
                .Include(x => x.ChargingItemDetails)
                .Include(x => x.FBAPickDetails)
                .SingleOrDefault(x => x.Id == shipOrderId);
            if (operation == "Reset")
            {
                shipOrderInDb.Status = FBAStatus.NewOrder;
            }
            else
            {
                shipOrderInDb.PlaceTime = report.PlaceTime;
                shipOrderInDb.StartedTime = report.StartedTime;
                shipOrderInDb.ReadyTime = report.ReadyTime;
                shipOrderInDb.PickMan = report.PickMan;
                shipOrderInDb.Lot = report.Lot;
                shipOrderInDb.ConfirmedBy = report.ConfirmedBy;

                if (operation == "Finish")
                {
                    if ((shipOrderInDb.OrderType == FBAOrderType.Standard && shipOrderInDb.FBAPickDetails.Count > 0 && shipOrderInDb.FBAPickDetails.Sum(x => x.ActualPlts) == 0))
                    {
                        throw new Exception("Cannot ready for now. The actual outbound plts of a standard ship order cannot be 0. Please go and adjust actual plts first.");
                    }

                    if (!IsAllowedToReady(shipOrderInDb))
                    {
                        throw new Exception("Cannot ready for now. You must finish all the instruction in this work order.");
                    }
                    else if (IsPending(shipOrderInDb))
                    {
                        shipOrderInDb.Status = FBAStatus.Pending;
                        shipOrderInDb.OperationLog = "Submited by " + _userName;
                    }
                    else
                    {
                        shipOrderInDb.Status = FBAStatus.Ready;
                        shipOrderInDb.OperationLog = "Ready by " + _userName;
                    }
                }
            }

            _context.SaveChanges();
        }

        private void UpdateWOInfo(FBAShipOrder shipOrderInDb, string pickMan, string instructor, string location)
        {
            shipOrderInDb.PickMan = pickMan;
            shipOrderInDb.Instructor = instructor;
            shipOrderInDb.Lot = location;
        }

        private bool IsPending(FBAShipOrder shipOrderInDb)
        {
            var result = false;
            foreach (var s in shipOrderInDb.ChargingItemDetails)
            {
                if (s.HandlingStatus == FBAStatus.Pending)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        private bool IsAllowedToReady(FBAShipOrder shipOrderInDb)
        {
            var result = true;
            foreach (var s in shipOrderInDb.ChargingItemDetails)
            {
                if (s.HandlingStatus == FBAStatus.New || s.HandlingStatus == FBAStatus.ReturnedOrder || s.HandlingStatus == FBAStatus.Updated || s.HandlingStatus == FBAStatus.Pending || s.HandlingStatus == FBAStatus.Confirmed)
                {
                    result = false;
                    break;
                }
            }
            return result;
        }
    }

    public class WarehouseOutboundLog
    {
        public int Id { get; set; }

        public string BatchNumber { get; set; }

        public string SubCustomer { get; set; }

        public string Department { get; set; }

        public string WarehouseOrderType { get; set; }  

        public string Status { get; set; }

        public string OrderNumber { get; set; }

        public string BOLNumber { get; set; }

        public string Lot { get; set; }

        public string PickNumber { get; set; }

        public string CustomerCode { get; set; }

        public string OrderType { get; set; }

        public string Destination { get; set; }

        public string PickMan { get; set; }

        public int TotalCtns { get; set; }

        public int TotalPlts { get; set; }

        public string Carrier { get; set; }

        public string ETS { get; set; }

        public string PlaceBy { get; set; }

        public string OperationLog { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime ReadyTime { get; set; }

        public DateTime ShipDate { get; set; }

        public DateTime PlaceTime { get; set; }

        public DateTime StartedTime { get; set; }

        public DateTime ReleaseTime { get; set; }

        public string ConfirmedBy { get; set; }

        public string ReleasedBy { get; set; }

        public string Instruction { get; set; }

        public string ETSTimeRange { get; set; }
    }
}
