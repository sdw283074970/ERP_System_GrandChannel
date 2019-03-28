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
            var breaker = new CartonBreaker();

            var prePackLocation = _context.FCRegularLocationDetails
                .Where(x => x.SizeBundle.Contains(" ") && x.AvailableCtns != 0 && !x.SizeBundle.Contains("SIZE"));

            foreach(var p in prePackLocation)
            {
                breaker.BreakPrePack(p.Id);
            }

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