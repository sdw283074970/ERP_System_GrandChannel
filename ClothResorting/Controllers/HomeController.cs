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

namespace ClothResorting.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private ApplicationDbContext _context;


        public HomeController()
        {
            _context = new ApplicationDbContext();

        }

        public ActionResult Index()
        {

            return View();
        }

        public ActionResult Test()
        {
            var customerList = _context.UpperVendors
                .Include(x => x.ChargingItems)
                .Where(x => x.DepartmentCode == "FBA" && x.ChargingItems.Count == 0);

            var generator = new ChargingItemGenerator();

            foreach(var customer in customerList)
            {
                generator.GenerateChargingItems(_context, customer);
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
    }
}