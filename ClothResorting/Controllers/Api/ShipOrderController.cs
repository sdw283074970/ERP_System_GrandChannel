using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Models;
using ClothResorting.Models.ApiTransformModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Data.Entity;
using System.Web.Http;
using System.Web;

namespace ClothResorting.Controllers.Api
{
    public class ShipOrderController : ApiController
    {
        private ApplicationDbContext _context;
        private string _userName;

        public ShipOrderController()
        {
            _context = new ApplicationDbContext();
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0];
        }

        // GET /api/ShipOrder/ 查询
        public IHttpActionResult GetAllShipOrder()
        {
            var resultDto = _context.ShipOrders.OrderByDescending(x => x.Id)
                .Where(x => x.Id > 0).Select(Mapper.Map<ShipOrder, ShipOrderDto>);

            return Ok(resultDto);
        }

        // POST /api/ShipOrder/ 新建
        [HttpPost]
        public IHttpActionResult CreateNewShipOrder([FromBody]PickTiketsRangeJsonObj obj)
        {
            _context.ShipOrders.Add(new ShipOrder {
                OrderPurchaseOrder = obj.OrderPurchaseOrder,
                Customer = obj.Customer,
                Address = obj.Address,
                PickTicketsRange = obj.PickTicketsRange,
                CreateDate = DateTime.Now.ToString("MM/dd/yyyy"),
                PickDate = "Unassigned",
                PickingMan = "Unassigned",
                Status = "New Create",
                Operator = _userName,
                Vendor = obj.Vendor,
                ShippingMan = ""
            });

            _context.SaveChanges();

            var result = _context.ShipOrders.OrderByDescending(x => x.Id).First();
            var resultDto = Mapper.Map<ShipOrder, ShipOrderDto>(result);

            return Created(Request.RequestUri + "/" + result.Id, resultDto);
        }

        // PUT /api/ShipOrder/{id}(shipOrderId) 发货
        [HttpPut]
        public void ShipShipOrder([FromUri]int id)
        {
            var pickDetailsInDb = _context.PickDetails
                .Include(x => x.ShipOrder)
                .Where(x => x.ShipOrder.Id == id
                    && x.Status == "Picking");

            var shipOrderInDb = _context.ShipOrders.Find(id);
            
            var locationDeatilsInDb = _context.FCRegularLocationDetails
                .Where(x => x.Id > 0)
                .ToList();

            //此处应简化数据库查询
            foreach(var pickDetail in pickDetailsInDb)
            {
                var locationDetail = locationDeatilsInDb.SingleOrDefault(x => x.Id == pickDetail.LocationDetailId);

                locationDetail.ShippedCtns += pickDetail.PickCtns;
                locationDetail.ShippedPcs += pickDetail.PickPcs;

                locationDetail.PickingCtns -= pickDetail.PickCtns;
                locationDetail.PickingPcs -= pickDetail.PickPcs;

                pickDetail.Status = "Shipped";

                if (locationDetail.AvailableCtns == 0 && locationDetail.PickingCtns == 0)
                {
                    locationDetail.Status = "Shipped";
                }
            }

            shipOrderInDb.Status = "Shipped";
            shipOrderInDb.ShippingMan = _userName;

            _context.SaveChanges();
        }
        
        // PUT /api/shiporder/?shipOrderId={id}
        [HttpPut]
        public void ChangeStatus([FromUri]int shipOrderId)
        {
            var shipOrderInDb = _context.ShipOrders.Find(shipOrderId);
            if (shipOrderInDb.Status == "Picking")
            {
                shipOrderInDb.Status = "Ready";
            }
            else if (shipOrderInDb.Status == "Ready")
            {
                shipOrderInDb.Status = "Picking";
            }
            _context.SaveChanges();
        }

        // DELETE /api/shipOrder/{id}(shipOrderId) 移除
        [HttpDelete]
        public void CancelShipOrder([FromUri]int id)
        {
            var pickDetailsInDb = _context.PickDetails
                .Include(x => x.ShipOrder)
                .Where(x => x.ShipOrder.Id == id
                    && x.Status == "Picking");

            var locationDeatilsInDb = _context.FCRegularLocationDetails
                .Where(x => x.Id > 0)
                .ToList();

            foreach (var pickDetail in pickDetailsInDb)
            {
                var locationDetail = locationDeatilsInDb.SingleOrDefault(x => x.Id == pickDetail.LocationDetailId);

                locationDetail.AvailableCtns += pickDetail.PickCtns;
                locationDetail.AvailablePcs += pickDetail.PickPcs;

                locationDetail.PickingCtns -= pickDetail.PickCtns;
                locationDetail.PickingPcs -= pickDetail.PickPcs;

                if (locationDetail.PickingCtns == 0 && locationDetail.AvailableCtns != 0)
                {
                    locationDetail.Status = "In Stock";
                }
            }

            var shipOrderInDb = _context.ShipOrders.Find(id);

            var diagnosticsInDb = _context.PullSheetDiagnostics
                .Include(x => x.ShipOrder)
                .Where(x => x.ShipOrder.Id == id);

            _context.PickDetails.RemoveRange(pickDetailsInDb);
            _context.PullSheetDiagnostics.RemoveRange(diagnosticsInDb);
            _context.SaveChanges();

            _context.ShipOrders.Remove(shipOrderInDb);
            _context.SaveChanges();
        }
    }
}
