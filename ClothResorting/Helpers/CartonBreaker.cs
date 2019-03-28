using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace ClothResorting.Helpers
{
    public class CartonBreaker
    {
        private ApplicationDbContext _context;
        private string _userName;

        public CartonBreaker()
        {
            _context = new ApplicationDbContext();
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0];
        }

        //用来将SizeBundle和PcsBundle打碎成独立的条目
        public void BreakPrePack(int locationId)
        {
            var cartonLocation = _context.FCRegularLocationDetails
                .Include(x => x.RegularCaronDetail.POSummary.PreReceiveOrder)
                .Include(x => x.PreReceiveOrder)
                .SingleOrDefault(x => x.Id == locationId);

            var sizeArry = cartonLocation.SizeBundle.Split(' ');
            var pcsArr = cartonLocation.PcsBundle.Split(' ');

            var newCartonDetailList = new List<RegularCartonDetail>();
            var breakedItemList = new List<FCRegularLocationDetail>();

            for (int i = 0; i < sizeArry.Count(); i++)
            {
                var currentPcs = int.Parse(pcsArr[i]);

                if (currentPcs != 0)
                {
                    var currentSize = sizeArry[i];
                    var sameLocationDetail = _context.FCRegularLocationDetails
                        .Include(x => x.RegularCaronDetail)
                        .Where(x => x.Container == cartonLocation.Container
                            && x.PurchaseOrder == cartonLocation.PurchaseOrder
                            && x.Style == cartonLocation.Style
                            && x.Color == cartonLocation.Color
                            && x.CartonRange == cartonLocation.CartonRange + "(Broke)" 
                            && x.SizeBundle == currentSize
                            && x.Batch == cartonLocation.Batch + "(Broke)");

                    var sameLocation = sameLocationDetail.Where(x => x.Location == cartonLocation.Location);

                    //为每一个Size检测是否已经存在分裂后的库存,没有的话就新建一个库存和cartonDetail，否则直接加上去
                    if (sameLocationDetail.Count() == 0)
                    {
                        var newCartonDetail = CreateNewRegularCartonDetail(cartonLocation, sizeArry[i], pcsArr[i], currentPcs);
                        
                        var newLocation = new FCRegularLocationDetail();

                        newLocation.Container = cartonLocation.Container;
                        newLocation.Vendor = cartonLocation.Vendor;
                        newLocation.Status = "In Stock";
                        newLocation.PurchaseOrder = cartonLocation.PurchaseOrder;
                        newLocation.Style = cartonLocation.Style;
                        newLocation.Color = cartonLocation.Color;
                        newLocation.CustomerCode = cartonLocation.CustomerCode;
                        newLocation.InboundDate = cartonLocation.InboundDate;
                        newLocation.Allocator = _userName;
                        newLocation.Location = cartonLocation.Location;

                        newLocation.SizeBundle = sizeArry[i];
                        newLocation.PcsBundle = currentPcs.ToString();
                        newLocation.PcsPerCaron = currentPcs;

                        newLocation.CartonRange = cartonLocation.CartonRange + "(Broke)";
                        newLocation.Batch = cartonLocation.Batch + "(Broke)";

                        newLocation.Cartons = 0;
                        newLocation.Quantity = currentPcs * cartonLocation.AvailableCtns;

                        newLocation.AvailableCtns = 0;
                        newLocation.ShippedCtns = 0;
                        newLocation.PickingCtns = 0;

                        newLocation.AvailablePcs = currentPcs * cartonLocation.AvailableCtns;
                        newLocation.PickingPcs = 0;
                        newLocation.ShippedPcs = 0;

                        newLocation.PickDetails = null;
                        newLocation.RegularCaronDetail = cartonLocation.RegularCaronDetail;
                        newLocation.PreReceiveOrder = cartonLocation.RegularCaronDetail.POSummary.PreReceiveOrder;

                        newLocation.RegularCaronDetail = newCartonDetail;

                        newCartonDetailList.Add(newCartonDetail);
                        breakedItemList.Add(newLocation);
                    }
                    //已经有其他库存的Broke对象但没有选中库存的Broke对象时，新建选中对象的location库位对象，并调整已存在的cartonDetail对象的值
                    else if (sameLocation.Count() == 0)
                    {
                        //调整已经存在的broke CartonDetail对象值
                        sameLocationDetail.First().RegularCaronDetail.Quantity = currentPcs * cartonLocation.AvailableCtns;
                        sameLocationDetail.First().RegularCaronDetail.ActualPcs = currentPcs * cartonLocation.AvailableCtns;
                        sameLocationDetail.First().RegularCaronDetail.Adjustor = _userName;

                        if(sameLocationDetail.First().RegularCaronDetail.Cartons != 0)
                        {
                            sameLocationDetail.First().RegularCaronDetail.Cartons += cartonLocation.AvailableCtns;
                            sameLocationDetail.First().RegularCaronDetail.ActualCtns += cartonLocation.AvailableCtns;
                        }

                        //新建location对象
                        var newLocation = new FCRegularLocationDetail();

                        newLocation.Container = cartonLocation.Container;
                        newLocation.Vendor = cartonLocation.Vendor;
                        newLocation.Status = "In Stock";
                        newLocation.PurchaseOrder = cartonLocation.PurchaseOrder;
                        newLocation.Style = cartonLocation.Style;
                        newLocation.Color = cartonLocation.Color;
                        newLocation.CustomerCode = cartonLocation.CustomerCode;
                        newLocation.InboundDate = cartonLocation.InboundDate;
                        newLocation.Allocator = _userName;
                        newLocation.Location = cartonLocation.Location;

                        newLocation.SizeBundle = sizeArry[i];
                        newLocation.PcsBundle = currentPcs.ToString();
                        newLocation.PcsPerCaron = currentPcs;

                        newLocation.CartonRange = cartonLocation.CartonRange + "(Broke)";
                        newLocation.Batch = cartonLocation.Batch + "(Broke)";

                        newLocation.Cartons = 0;
                        newLocation.Quantity = currentPcs * cartonLocation.AvailableCtns;

                        newLocation.AvailableCtns = 0;
                        newLocation.ShippedCtns = 0;
                        newLocation.PickingCtns = 0;

                        newLocation.AvailablePcs = currentPcs * cartonLocation.AvailableCtns;
                        newLocation.PickingPcs = 0;
                        newLocation.ShippedPcs = 0;

                        newLocation.PickDetails = null;
                        newLocation.RegularCaronDetail = cartonLocation.RegularCaronDetail;
                        newLocation.PreReceiveOrder = cartonLocation.RegularCaronDetail.POSummary.PreReceiveOrder;

                        newLocation.RegularCaronDetail = sameLocationDetail.First().RegularCaronDetail;

                        breakedItemList.Add(newLocation);
                    }
                    //如果已经把当前对象break过了，则找到break过的对象，直接调整
                    else if (sameLocation.Count() != 0)
                    {
                        sameLocation.First().Quantity += currentPcs * cartonLocation.AvailableCtns;
                        sameLocation.First().AvailablePcs += currentPcs * cartonLocation.AvailableCtns;

                        sameLocation.First().RegularCaronDetail.Quantity += currentPcs * cartonLocation.AvailableCtns;
                        sameLocation.First().RegularCaronDetail.ActualPcs += currentPcs * cartonLocation.AvailableCtns;

                        if (sameLocation.First().Cartons != 0)
                        {
                            sameLocation.First().Cartons += cartonLocation.AvailableCtns;
                            sameLocation.First().AvailableCtns += cartonLocation.AvailableCtns;

                            sameLocation.First().RegularCaronDetail.Cartons += cartonLocation.AvailableCtns;
                            sameLocation.First().RegularCaronDetail.ActualCtns += cartonLocation.AvailableCtns;
                        }
                    }
                }
            }

            //将新建列表中的第一项对象当作宿主对象
            if (newCartonDetailList.Count != 0)
            {
                newCartonDetailList.First().Cartons = cartonLocation.AvailableCtns;
                newCartonDetailList.First().ActualCtns = cartonLocation.AvailableCtns;
            }

            if (breakedItemList.Count != 0)
            {
                breakedItemList.First().Cartons = cartonLocation.AvailableCtns;
                breakedItemList.First().AvailableCtns = cartonLocation.AvailableCtns;
            }

            //保留之前的记录，调整库存Available值和cartonDetail的实际收货箱数和件数
            cartonLocation.RegularCaronDetail.ActualCtns -= cartonLocation.AvailableCtns;
            cartonLocation.RegularCaronDetail.ActualPcs -= cartonLocation.AvailablePcs;
            cartonLocation.RegularCaronDetail.Cartons -= cartonLocation.AvailableCtns;
            cartonLocation.RegularCaronDetail.Quantity -= cartonLocation.AvailablePcs;

            cartonLocation.Cartons -= cartonLocation.AvailableCtns;
            cartonLocation.Quantity -= cartonLocation.AvailablePcs;
            cartonLocation.AvailableCtns = 0;
            cartonLocation.AvailablePcs = 0;

            if (cartonLocation.Status != "Picking")
            {
                cartonLocation.Status = "Shipped";
            }

            _context.RegularCartonDetails.AddRange(newCartonDetailList);
            _context.FCRegularLocationDetails.AddRange(breakedItemList);

            _context.SaveChanges();
        }

        private RegularCartonDetail CreateNewRegularCartonDetail(FCRegularLocationDetail cartonLocation, string sizeBundle, string pcsBundle, int currentPcs)
        {
            return new RegularCartonDetail
            {
                PurchaseOrder = cartonLocation.PurchaseOrder,
                Style = cartonLocation.Style,
                Customer = cartonLocation.CustomerCode,
                CartonRange = cartonLocation.CartonRange + "(Broke)",
                SizeBundle = sizeBundle,
                PcsBundle = pcsBundle,
                PcsPerCarton = currentPcs,
                Quantity = currentPcs * cartonLocation.AvailableCtns,
                POSummary = cartonLocation.RegularCaronDetail.POSummary,
                Color = cartonLocation.Color,
                ActualPcs = currentPcs * cartonLocation.AvailableCtns,
                ToBeAllocatedCtns = 0,
                ToBeAllocatedPcs = 0,
                Status = "Allocated",
                Container = cartonLocation.Container,
                OrderType = "Solid Pack",
                Comment = "Break from pre-pack",
                Receiver = _userName,
                Adjustor = _userName,
                Operator = _userName,
                Vendor = cartonLocation.RegularCaronDetail.Vendor,
                Batch = cartonLocation.RegularCaronDetail.Batch + "(Broke)"
            };
        }
    }
}