using ClothResorting.Models;
using System.Linq;
using System.Web.Mvc;
using ClothResorting.Models.StaticClass;
using System.Data.Entity;
using CoreScanner;

namespace ClothResorting.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private ApplicationDbContext _context;
        private string _userName;
        static CCoreScannerClass cCoreScannerClass;

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
            cCoreScannerClass = new CCoreScannerClass();

            short[] scannerTypes = new short[1]; // Scanner Types you are interested in
            scannerTypes[0] = 1; // 1 for all scanner types
            short numberOfScannerTypes = 1; // Size of the scannerTypes array
            int status; // Extended API return code

            cCoreScannerClass.Open(0, scannerTypes, numberOfScannerTypes, out status);

            //short numberOfScanners; // Number of scanners expect to be used
            //int[] connectedScannerIDList = new int[255];
            //// List of scanner IDs to be returned
            //string outXML; //Scanner details output

            //cCoreScannerClass.GetScanners(out numberOfScanners, connectedScannerIDList, out outXML, out status);

            // Let's beep the beeper
            //int opcode = 6000; // Method for Beep the beeper
            string outXML; // Output
            //string inXML = "<inArgs>" +
            //        "<scannerID>2</scannerID>" + // The scanner you need to beep
            //        "<cmdArgs>" +
            //            "<arg-int>3</arg-int>" + // 4 high short beep pattern
            //        "</cmdArgs>" +
            //    "</inArgs>";
            //cCoreScannerClass.ExecCommand(opcode, ref inXML, out outXML, out status);

            // Let's set the UPC-A enable/disable
            var opcode = 5004; // Method for Set the scanner attributes
            string inXML = "<inArgs>" +
                "<scannerID>1</scannerID>" +
                // The scanner you need to get the information (above)
                "<cmdArgs>" +
                "<arg-xml>" +
                "<attrib_list>" +
                "<attribute>" +
                "<id>1</id>" +
                // Attribute number for UPC-A
                "<datatype>F</datatype>" +
                "<value>False</value>" +
                "</attribute>" +
                "</attrib_list>" +
                "</arg-xml>" +
                "</cmdArgs>" +
                "</inArgs>";
            cCoreScannerClass.ExecCommand(opcode, ref inXML, out outXML, out status);

            ViewBag.Message = outXML;

            //ViewBag.Message = "Your application description page.";

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