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
            var cartonLocationsInDb = _context.FBACartonLocations
                .Include(x => x.FBAOrderDetail)
                .Where(x => x.Id > 0);

            foreach(var c in cartonLocationsInDb)
            {
                c.ActualQuantity = c.FBAOrderDetail.ActualQuantity;
                c.AvailableCtns = c.FBAOrderDetail.ActualQuantity - c.PickingCtns - c.ShippedCtns;
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