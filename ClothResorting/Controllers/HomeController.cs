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
            var prereceiveOrdersInDb = _context.PreReceiveOrders
                .Include(x => x.POSummaries)
                .Where(x => x.POSummaries.Count > 0);

            foreach(var order in prereceiveOrdersInDb)
            {
                var index = 1;

                foreach(var po in order.POSummaries)
                {
                    po.Batch = index.ToString();
                    index += 1;
                }
            }

            _context.SaveChanges();

            var cartonDetailsInDb = _context.RegularCartonDetails
                .Include(x => x.FCRegularLocationDetail)
                .Include(x => x.POSummary)
                .Where(x => x.Id > 0);

            foreach(var carton in cartonDetailsInDb)
            {
                carton.Batch = carton.POSummary.Batch;
                foreach (var location in carton.FCRegularLocationDetail)
                {
                    location.Batch = carton.Batch;
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