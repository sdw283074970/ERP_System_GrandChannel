using ClothResorting.Models;
using System.Linq;
using System.Web.Mvc;
using ClothResorting.Models.StaticClass;
using System.Data.Entity;
using ClothResorting.Helpers;
using System;

namespace ClothResorting.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private ApplicationDbContext _context;
        private string _userName;

        public HomeController()
        {
            _context = new ApplicationDbContext();
            _userName = System.Web.HttpContext.Current.User.Identity.Name;
        }

        public ActionResult Index()
        {
            if (User.IsInRole(RoleName.CanOperateAsT3) || User.IsInRole(RoleName.CanOperateAsT4) || User.IsInRole(RoleName.CanOperateAsT5) || User.IsInRole(RoleName.CanDeleteEverything))
            {
                return View();
            }
            else if (User.IsInRole(RoleName.CanOperateAsT2))
            {
                return View("~/Views/Warehouse/Index.cshtml");
            }
            else if (User.IsInRole(RoleName.CanViewAsClientOnly))
            {
                var user = _context.Users
                    .SingleOrDefault(x => x.UserName == _userName);

                var userId = user.Id;
                var model = new UserViewModel { UserId = userId };

                return View("~/Views/CustomerClient/Index.cshtml", model);
            }
            else
            {
                return RedirectToAction("Denied", "Home");
            }
        }

        public ActionResult Test()
        {
            //var adjustShipOrder = new ShipOrder {
            //    Address = "NA",
            //    OrderPurchaseOrder = "FixAndAdjust20190906",
            //    Vendor = "Free Country",
            //    OrderType = "Regular",
            //    PickTicketsRange = "Fix",
            //    Operator = "Stone",
            //    CreateDate = DateTime.Now.ToString("yyyy-MM-dd")
            //};

            var adjustShipOrder = _context.ShipOrders.Find(978);
            adjustShipOrder.Status = "Picking";

            var wrongPickedLocationsInDb = _context.PickDetails
                .Include(x => x.FCRegularLocationDetail)
                .Where(x => x.PickCtns != 0 && x.PickPcs == 0);

            foreach(var wrong in wrongPickedLocationsInDb)
            {
                var w = wrong.FCRegularLocationDetail;
                if (w.AvailablePcs != 0)
                {
                    _context.PickDetails.Add(
                        new PickDetail
                        {
                            CartonRange = w.CartonRange,
                            Container = w.Container,
                            UPCNumber = w.UPCNumber,
                            PurchaseOrder = w.PurchaseOrder,
                            Style = w.Style,
                            Color = w.Color,
                            SizeBundle = w.SizeBundle,
                            PcsBundle = w.PcsBundle,
                            PcsPerCarton = w.PcsPerCaron,
                            PickCtns = 0,
                            PickPcs = w.AvailablePcs,
                            CustomerCode = w.CustomerCode,
                            Location = w.Location,
                            ShipOrder = adjustShipOrder
                        }
                    );

                    w.PickingPcs += w.AvailablePcs;
                    w.AvailablePcs = 0;
                }


                var ps = _context.FCRegularLocationDetails
                    .Where(x => x.Cartons == 0 && x.AvailablePcs != 0 && x.CartonRange == w.CartonRange && x.Container == w.Container && x.Batch == w.Batch && x.Location == w.Location);

                foreach(var p in ps)
                {
                    if (p.AvailablePcs != 0)
                    {
                        _context.PickDetails.Add(
                            new PickDetail
                            {
                                CartonRange = p.CartonRange,
                                Container = p.Container,
                                UPCNumber = p.UPCNumber,
                                PurchaseOrder = p.PurchaseOrder,
                                Style = p.Style,
                                Color = p.Color,
                                SizeBundle = p.SizeBundle,
                                PcsBundle = p.PcsBundle,
                                PcsPerCarton = p.PcsPerCaron,
                                PickCtns = 0,
                                PickPcs = p.AvailablePcs,
                                Location = p.Location,
                                CustomerCode = p.CustomerCode,
                                ShipOrder = adjustShipOrder
                            }
                        );

                        p.PickingPcs += p.AvailablePcs;
                        p.AvailablePcs = 0;
                    }
                }
            }

            _context.SaveChanges();

            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Denied()
        {
            return View();
        }

        private string UnifyTime(string dateString)
        {
            var dateArray = dateString.Split('/');
            var MM = "0";
            var dd = "0";
            var yyyy = "20";

            if (dateArray[0].Length == 1)
            {
                MM = MM + dateArray[0].ToString();
            }
            else
            {
                MM = dateArray[0].ToString();
            }

            if (dateArray[1].Length == 1)
            {
                dd = dd + dateArray[1].ToString();
            }
            else
            {
                dd = dateArray[1].ToString();
            }

            if (dateArray[2].Length == 2)
            {
                yyyy = yyyy + dateArray[2].ToString();
            }
            else if (dateArray[2].Length != 4)
            {
                yyyy = yyyy + dateArray[2][0].ToString() + dateArray[2][1].ToString();
            }
            else
            {
                yyyy = dateArray[2].ToString();
            }

            return yyyy + "-" + MM + "-" + dd;
        }

        private FCRegularLocationDetail CreateRegularLocationV2(RegularCartonDetail cartonDetailInDb)
        {
            var result = new FCRegularLocationDetail
            {
                Container = cartonDetailInDb.POSummary.Container,
                PurchaseOrder = cartonDetailInDb.PurchaseOrder,
                Style = cartonDetailInDb.Style,
                Color = cartonDetailInDb.Color,
                CustomerCode = cartonDetailInDb.Customer,
                SizeBundle = cartonDetailInDb.SizeBundle,
                PcsBundle = cartonDetailInDb.PcsBundle,
                Cartons = cartonDetailInDb.ToBeAllocatedCtns,
                Quantity = cartonDetailInDb.ToBeAllocatedPcs,
                Location = "FLOOR",
                PcsPerCaron = cartonDetailInDb.PcsPerCarton,
                Status = "In Stock",
                AvailableCtns = cartonDetailInDb.ToBeAllocatedCtns,
                PickingCtns = 0,
                ShippedCtns = 0,
                AvailablePcs = cartonDetailInDb.ToBeAllocatedPcs,
                PickingPcs = 0,
                ShippedPcs = 0,
                PreReceiveOrder = cartonDetailInDb.POSummary.PreReceiveOrder,
                RegularCaronDetail = cartonDetailInDb,
                CartonRange = cartonDetailInDb.CartonRange,
                Allocator = _userName,
                Batch = cartonDetailInDb.Batch,
                Vendor = cartonDetailInDb.Vendor
            };

            cartonDetailInDb.ToBeAllocatedCtns = 0;
            cartonDetailInDb.ToBeAllocatedPcs = 0;

            return result;
        }
    }
}