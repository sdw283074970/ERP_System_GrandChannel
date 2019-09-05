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
                .Where(x => x.ShipOrder.Id == shipOrderId);

            var shipOrderInDb = pickDetailsInDb.First().ShipOrder;
            //var createBy = shipOrderInDb.Operator;

            //if (_userName != createBy)
            //{
            //    throw new Exception("This ship order is created by " + createBy + ". Cannot be shipped by other operator.");
            //}

            var orderType = shipOrderInDb.OrderType;

            foreach (var pickDetail in pickDetailsInDb)
            {
                //发货Regular Oder的方法
                if (orderType == OrderType.Regular)
                {
                    var locationDetailInDb = pickDetail.FCRegularLocationDetail;

                    var parasiticLocationDetail = _context.FCRegularLocationDetails
                        .Where(x => x.Container == locationDetailInDb.Container
                            && x.CartonRange == locationDetailInDb.CartonRange
                            && x.Batch == locationDetailInDb.Batch)
                        .ToList();

                    locationDetailInDb.ShippedPcs += pickDetail.PickPcs;
                    locationDetailInDb.PickingPcs -= pickDetail.PickPcs;
                    locationDetailInDb.ShippedCtns += pickDetail.PickCtns;
                    locationDetailInDb.PickingCtns -= pickDetail.PickCtns;

                    ////如果该拣货对象的库存不存在寄生对象的情况，则箱数正常从拣货箱数扣除并加倒
                    //if (parasiticLocationDetail.Count() == 1)
                    //{
                    //    locationDetailInDb.ShippedCtns += pickDetail.PickCtns;
                    //    locationDetailInDb.PickingCtns -= pickDetail.PickCtns;
                    //}
                    ////否则，先查找到宿主并扣除宿主拣货箱数
                    //else
                    //{
                    //    //如果当前对象就是宿主对象，正常除拣货箱数
                    //    if (locationDetailInDb.Cartons != 0)
                    //    {
                    //        locationDetailInDb.ShippedCtns += pickDetail.PickCtns;
                    //        locationDetailInDb.PickingCtns -= pickDetail.PickCtns;
                    //    }
                    //    //否则找到宿主对象，与其比较谁的应发箱数最大，并将这个数字更新到已发箱数中
                    //    else
                    //    {
                    //        AdjustMainShippedCartons(_context, locationDetailInDb, parasiticLocationDetail);
                    //    }
                    //}

                    if (locationDetailInDb.PickingPcs == 0 && locationDetailInDb.AvailablePcs == 0)
                    {
                        locationDetailInDb.Status = Status.Shipped;
                    }
                    else if (locationDetailInDb.PickingPcs == 0 && locationDetailInDb.AvailablePcs != 0)
                    {
                        locationDetailInDb.Status = Status.InStock;
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

                    //同时将Pick Detail中的所有条目添加到Outbound History中
                    var skuInDb = _context.SpeciesInventories
                        .SingleOrDefault(x => x.PurchaseOrder == pickDetail.PurchaseOrder
                        && x.Style == pickDetail.Style
                        && x.Color == pickDetail.Color
                        && x.Size == pickDetail.SizeBundle);

                    _context.OutboundHistories.Add(new OutboundHistory {
                        FromLocation = pickDetail.Location,
                        OutboundDate = DateTime.Now,
                        OrderPurchaseOrder = shipOrderInDb.OrderPurchaseOrder,
                        SpeciesInventory = skuInDb,
                        OutboundPcs = pickDetail.PickPcs
                    });

                    if (locationDetail.PickingPcs == 0 && locationDetail.AvailablePcs == 0)
                    {
                        locationDetail.Status = Status.Shipped;
                    }
                    else if (locationDetail.PickingPcs == 0 && locationDetail.AvailablePcs != 0)
                    {
                        locationDetail.Status = Status.InStock;
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
            if (orderType == OrderType.Regular && shipOrderInDb.Status != Status.Shipped)
            {
                foreach (var pickDetail in pickDetailsInDb)
                {
                    var locationDetailInDb = pickDetail.FCRegularLocationDetail;
                    var parasiticLocationDetail = _context.FCRegularLocationDetails
                        .Where(x => x.Container == locationDetailInDb.Container
                            && x.CartonRange == locationDetailInDb.CartonRange
                            && x.Batch == locationDetailInDb.Batch)
                        .ToList();

                    locationDetailInDb.AvailablePcs += pickDetail.PickPcs;
                    locationDetailInDb.PickingPcs -= pickDetail.PickPcs;

                    //如果该拣货对象的库存不存在寄生对象的情况，则箱数正常返回到库存
                    if (parasiticLocationDetail.Count() == 1)
                    {
                        locationDetailInDb.AvailableCtns += pickDetail.PickCtns;
                        locationDetailInDb.PickingCtns -= pickDetail.PickCtns;
                    }
                    //否则为寄生/宿主对象，按照已返回的件数重新计算箱数
                    else
                    {
                        AdjustMainAvailableCartons(_context, locationDetailInDb);
                    }

                    //更改状态
                    if (locationDetailInDb.PickingPcs == 0 && locationDetailInDb.AvailablePcs != 0)
                    {
                        locationDetailInDb.Status = Status.InStock;
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

        private void AdjustMainAvailableCartons(ApplicationDbContext context, FCRegularLocationDetail locationDetailInDb)
        {
            //如果其本身就是宿主对象
            if (locationDetailInDb.Cartons != 0)
            {
                var originalAvailableCtns = locationDetailInDb.AvailableCtns;
                var remainableCtns = Math.Max(locationDetailInDb.AvailableCtns, Ceiling(locationDetailInDb.AvailablePcs, locationDetailInDb.PcsPerCaron));

                locationDetailInDb.AvailableCtns = remainableCtns;
                locationDetailInDb.PickingCtns = locationDetailInDb.Cartons - locationDetailInDb.AvailableCtns - locationDetailInDb.ShippedCtns;
            }
            else
            {
                //查找当前对象的宿主对象
                try
                {
                    var mainLocationInDbtest = context.FCRegularLocationDetails
                        .SingleOrDefault(x => x.Container == locationDetailInDb.Container
                            && x.CartonRange == locationDetailInDb.CartonRange
                            && x.Batch == locationDetailInDb.Batch
                            && x.Location == locationDetailInDb.Location
                            && x.Cartons != 0);
                }
                catch(Exception e)
                {
                    throw new Exception(e.Message + " Container=" + locationDetailInDb.Container + ", Carton Range=" + locationDetailInDb.CartonRange + ", Batch=" + locationDetailInDb.Batch + ", Location=" + locationDetailInDb.Location);
                }

                var mainLocationInDb = context.FCRegularLocationDetails
                    .SingleOrDefault(x => x.Container == locationDetailInDb.Container
                        && x.CartonRange == locationDetailInDb.CartonRange
                        && x.Batch == locationDetailInDb.Batch
                        && x.Location == locationDetailInDb.Location
                        && x.Cartons != 0);

                var originaAvailableCtns = mainLocationInDb.AvailableCtns;
                var remainableCtns = Math.Max(mainLocationInDb.AvailableCtns, Ceiling(locationDetailInDb.AvailablePcs, locationDetailInDb.PcsPerCaron));

                mainLocationInDb.AvailableCtns = remainableCtns;
                mainLocationInDb.PickingCtns = mainLocationInDb.Cartons - mainLocationInDb.AvailableCtns - mainLocationInDb.ShippedCtns;
            }
        }


        public void RemovePickDetail(ApplicationDbContext context, PickDetail pickDetailInDb, int putBackCtns, int putBackPcs, string orderType)
        {
            if (orderType == OrderType.Regular)
            {
                pickDetailInDb.FCRegularLocationDetail.AvailableCtns += putBackCtns;
                pickDetailInDb.FCRegularLocationDetail.PickingCtns -= putBackCtns;

                pickDetailInDb.FCRegularLocationDetail.AvailablePcs += putBackPcs;
                pickDetailInDb.FCRegularLocationDetail.PickingPcs -= putBackPcs;

                //刷新被放回地点状态
                pickDetailInDb.Status = RefreshRegularStatus(pickDetailInDb.FCRegularLocationDetail);

                //如果放回的箱数件数刚好等于拣货的箱数件数，那么移除拣货记录
                if (putBackCtns == pickDetailInDb.PickCtns && putBackPcs == pickDetailInDb.PickPcs)
                {
                    context.PickDetails.Remove(pickDetailInDb);
                }
                //否则仍然保留拣货记录
                else
                {
                    pickDetailInDb.PickCtns -= putBackCtns;
                    pickDetailInDb.PickPcs -= putBackPcs;
                }
            }
            else if (orderType == OrderType.Permanent)
            {
                //如果放回的件数刚好等于拣货的件数，那么移除拣货记录
                if (putBackPcs == pickDetailInDb.PickPcs)
                {
                    context.PickDetails.Remove(pickDetailInDb);
                }
                //否则仍然保留拣货记录
                else
                {
                    pickDetailInDb.PickPcs -= putBackPcs;
                }
            }
        }

        private void AdjustMainShippedCartons(ApplicationDbContext context, FCRegularLocationDetail locationDetailInDb, IEnumerable<FCRegularLocationDetail> parasiticLocationDetail)
        {
            //如果其本身就是宿主对象
            if (locationDetailInDb.Cartons != 0)
            {
                var originaShippedCtns = locationDetailInDb.ShippedCtns;
                var updatedShippedCtns = locationDetailInDb.Cartons;

                foreach (var location in parasiticLocationDetail)
                {
                    updatedShippedCtns = Math.Min(updatedShippedCtns, location.ShippedPcs / location.PcsPerCaron);
                }

                locationDetailInDb.ShippedCtns = updatedShippedCtns;
                locationDetailInDb.PickingCtns -= updatedShippedCtns - originaShippedCtns;
            }
            else
            {
                //查找当前对象的宿主对象
                try
                {
                    var mainLocationInDbtest = context.FCRegularLocationDetails
                        .SingleOrDefault(x => x.Container == locationDetailInDb.Container
                            && x.CartonRange == locationDetailInDb.CartonRange
                            && x.Batch == locationDetailInDb.Batch
                            && x.Location == locationDetailInDb.Location
                            && x.Cartons != 0);
                }
                catch(Exception e)
                {
                    throw new Exception(e.Message + " Container=" + locationDetailInDb.Container + ", Carton Range=" + locationDetailInDb.CartonRange + ", Batch=" + locationDetailInDb.Batch + ", Location=" + locationDetailInDb.Location);
                }

                var mainLocationInDb = context.FCRegularLocationDetails
                    .SingleOrDefault(x => x.Container == locationDetailInDb.Container
                        && x.CartonRange == locationDetailInDb.CartonRange
                        && x.Batch == locationDetailInDb.Batch
                        && x.Location == locationDetailInDb.Location
                        && x.Cartons != 0);

                if (mainLocationInDb == null)
                {
                    throw new Exception("Did not find the main object. Please check container=" + locationDetailInDb.Container + ", Carton Range=" + locationDetailInDb.CartonRange + ", Batch=" + locationDetailInDb.Batch);
                }

                var originaShippedCtns = mainLocationInDb.ShippedCtns;
                var updatedShippedCtns = mainLocationInDb.Cartons;

                foreach (var location in parasiticLocationDetail)
                {
                    //如果当前对象的pcspercarton是0，说明是全为0的无效储存对象，直接跳过
                    if (location.PcsPerCaron == 0)
                    {
                        continue;
                    }
                    else
                    {
                        updatedShippedCtns = Math.Min(updatedShippedCtns, location.ShippedPcs / location.PcsPerCaron);
                    }
                }

                mainLocationInDb.ShippedCtns = updatedShippedCtns;
                mainLocationInDb.PickingCtns -= updatedShippedCtns - originaShippedCtns;
            }
        }

        private int Ceiling(int a, int b)
        {
            if (a % b == 0)
            {
                return a / b;
            }
            else
            {
                return a / b + 1;
            }
        }

        private string RefreshRegularStatus(FCRegularLocationDetail location)
        {
            if (location.PickingCtns != 0 || location.PickingPcs != 0)
            {
                return Status.Picking;
            }
            else if (location.PickingCtns == 0 && location.PickingPcs == 0 && location.AvailableCtns == 0 && location.AvailablePcs == 0)
            {
                return Status.Shipped;
            }
            else
            {
                return Status.InStock;
            }
        }
    }
}