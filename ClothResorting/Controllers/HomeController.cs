using ClothResorting.Helpers;
using ClothResorting.Models;
using ClothResorting.Models.FBAModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using ClothResorting.Helpers.FBAHelper;
using ClothResorting.Models.FBAModels.StaticModels;
using ClothResorting.Models.StaticClass;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Globalization;
using ClothResorting.Helpers.DPHelper;
using AutoMapper;
using Newtonsoft.Json;
using ClothResorting.Controllers.Api.Fba;

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
            //var shipOrderList = new List<FBAShipOrder>();

            //var awDate = new DateTime(2019, 04, 29, 0, 0, 0, 0);
            //var wdDate = new DateTime(2019, 05, 06, 0, 0, 0, 0);
            //var pgDate = new DateTime(2019, 04, 30, 0, 0, 0, 0);

            //var shipOrderInDb = _context.FBAShipOrders
            //    .Include(x => x.InvoiceDetails)
            //    .Where(s => (s.CustomerCode == "AW" && s.ShipDate >= awDate) 
            //        || (s.CustomerCode == "FTC" && s.ShipDate >= awDate) 
            //        || (s.CustomerCode == "WD" && s.ShipDate >= wdDate) 
            //        || (s.CustomerCode == "GRNDPG" && s.ShipDate >= pgDate));

            //foreach (var s in shipOrderInDb)
            //{
            //    foreach (var i in s.InvoiceDetails)
            //    {
            //        i.Amount = i.Quantity * i.Rate;
            //    }
            //}

            //_context.SaveChanges();

            //var api = new InvoiceApi();

            //var list = shipOrderInDb.ToList();

            //foreach (var s in shipOrderInDb)
            //{
            //    api.CloseShipOrder(_context, s, User.Identity.Name.Split('@')[0], s.ShipOrderNumber, FBAInvoiceType.ShipOrder, s.CloseDate, true);
            //}

            //_context.SaveChanges();

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
    }
}