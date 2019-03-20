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
                    //为每一个Size新建一个库存和cartonDetail
                    var newCartonDetail = new RegularCartonDetail {
                        PurchaseOrder = cartonLocation.PurchaseOrder,
                        Style = cartonLocation.Style,
                        Customer = cartonLocation.CustomerCode,
                        CartonRange = cartonLocation.CartonRange + "(Breaked)",
                        SizeBundle = sizeArry[i],
                        PcsBundle = pcsArr[i],
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

                    newLocation.CartonRange = cartonLocation.CartonRange +  "(Broke)";
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
            }

            //将新建列表中的第一项对象当作宿主对象
            newCartonDetailList.First().Cartons = cartonLocation.AvailableCtns;
            newCartonDetailList.First().ActualCtns = cartonLocation.AvailableCtns;

            breakedItemList.First().Cartons = cartonLocation.AvailableCtns;
            breakedItemList.First().AvailableCtns = cartonLocation.AvailableCtns;

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
    }
}