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
            var palletsInDb = _context.FBAPallets
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.Id > 0);

            foreach(var p in palletsInDb)
            {
                if (p.FBAMasterOrder == null)
                {
                    var masterInDb = _context.FBAMasterOrders
                        .SingleOrDefault(x => x.Container == p.Container);

                    p.FBAMasterOrder = masterInDb;
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
    }
}