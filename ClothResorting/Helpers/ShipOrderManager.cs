using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using ClothResorting.Models.StaticClass;

namespace ClothResorting.Helpers
{
    public class ShipOrderManager
    {
        private ApplicationDbContext _context;
        private string _userName;

        public ShipOrderManager()
        {
            _context = new ApplicationDbContext();
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0];
        }

        //确认发货的方法
        public void ConfirmAndShip(int shipOrderId)
        {
            var pickDetailsInDb = _context.PickDetails
                .Include(x => x.ShipOrder)
                .Include(x => x.FCRegularLocationDetail)
                .Include(x => x.ReplenishmentLocationDetail.PurchaseOrderInventory)
                .Include(x => x.ReplenishmentLocationDetail.SpeciesInventory)
                .Where(x => x.ShipOrder.Id == shipOrderId
                    && x.Status == Status.Picking);

            var shipOrderInDb = pickDetailsInDb.First().ShipOrder;
            var orderType = shipOrderInDb.OrderType;

            foreach (var pickDetail in pickDetailsInDb)
            {
                //发货Regular Oder的方法
                if (orderType == OrderType.Prepack)
                {
                    var locationDetail = pickDetail.FCRegularLocationDetail;

                    locationDetail.ShippedCtns += pickDetail.PickCtns;
                    locationDetail.ShippedPcs += pickDetail.PickPcs;

                    locationDetail.PickingCtns -= pickDetail.PickCtns;
                    locationDetail.PickingPcs -= pickDetail.PickPcs;

                    if (locationDetail.PickingCtns == 0 && locationDetail.AvailableCtns == 0)
                    {
                        locationDetail.Status = Status.Shipped;
                    }
                }
                else if (orderType == OrderType.Replenishment)      //发货Replenishment Order的方法
                {
                    var locationDetail = pickDetail.ReplenishmentLocationDetail;

                    locationDetail.ShippedCtns += pickDetail.PickCtns;
                    locationDetail.ShippedPcs += pickDetail.PickPcs;

                    locationDetail.PickingCtns -= pickDetail.PickCtns;
                    locationDetail.PickingPcs -= pickDetail.PickPcs;

                    locationDetail.SpeciesInventory.ShippedPcs += pickDetail.PickPcs;
                    locationDetail.SpeciesInventory.PickingPcs -= pickDetail.PickPcs;

                    locationDetail.PurchaseOrderInventory.ShippedPcs += pickDetail.PickPcs;
                    locationDetail.PurchaseOrderInventory.PickingPcs -= pickDetail.PickPcs;

                    if (locationDetail.PickingCtns == 0 && locationDetail.AvailableCtns == 0)
                    {
                        locationDetail.Status = Status.Shipped;
                    }
                }
            }

            shipOrderInDb.Status = Status.Shipped;
            shipOrderInDb.ShippingMan = _userName;

            _context.SaveChanges();
        }

        //取消订单方法
        public void CancelShipOrder(int shipOrderId)
        {
            var pickDetailsInDb = _context.PickDetails
                .Include(x => x.ShipOrder)
                .Include(x => x.FCRegularLocationDetail)
                .Include(x => x.ReplenishmentLocationDetail.PurchaseOrderInventory)
                .Include(x => x.ReplenishmentLocationDetail.SpeciesInventory)
                .Where(x => x.ShipOrder.Id == shipOrderId);

            var shipOrderInDb = _context.ShipOrders.Find(shipOrderId);

            var orderType = shipOrderInDb.OrderType;

            //取消Regular Oder的方法
            if (orderType == OrderType.Prepack && shipOrderInDb.Status != Status.Shipped)
            {
                foreach (var pickDetail in pickDetailsInDb)
                {
                    var locationDetail = pickDetail.FCRegularLocationDetail;

                    locationDetail.AvailableCtns += pickDetail.PickCtns;
                    locationDetail.AvailablePcs += pickDetail.PickPcs;

                    locationDetail.PickingCtns -= pickDetail.PickCtns;
                    locationDetail.PickingPcs -= pickDetail.PickPcs;

                    if (locationDetail.PickingCtns == 0 && locationDetail.AvailableCtns != 0)
                    {
                        locationDetail.Status = Status.InStock;
                    }
                }
            }
            else if (orderType == OrderType.Replenishment && shipOrderInDb.Status != Status.Shipped)
            {
                foreach(var pickDetail in pickDetailsInDb)
                {
                    var locationDetail = pickDetail.ReplenishmentLocationDetail;

                    locationDetail.AvailableCtns += pickDetail.PickCtns;
                    locationDetail.AvailablePcs += pickDetail.PickPcs;

                    locationDetail.PickingCtns -= pickDetail.PickCtns;
                    locationDetail.PickingPcs -= pickDetail.PickPcs;

                    locationDetail.SpeciesInventory.AvailablePcs += pickDetail.PickPcs;
                    locationDetail.SpeciesInventory.PickingPcs -= pickDetail.PickPcs;

                    locationDetail.PurchaseOrderInventory.AvailablePcs += pickDetail.PickPcs;
                    locationDetail.PurchaseOrderInventory.PickingPcs -= pickDetail.PickPcs;

                    if (locationDetail.PickingCtns == 0 && locationDetail.AvailableCtns != 0)
                    {
                        locationDetail.Status = Status.InStock;
                    }
                }
            }

            var diagnosticsInDb = _context.PullSheetDiagnostics
                .Include(x => x.ShipOrder)
                .Where(x => x.ShipOrder.Id == shipOrderId);

            _context.PickDetails.RemoveRange(pickDetailsInDb);
            _context.PullSheetDiagnostics.RemoveRange(diagnosticsInDb);
            _context.SaveChanges();

            _context.ShipOrders.Remove(shipOrderInDb);
            _context.SaveChanges();
        }
    }
}