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

            //var list = new List<FBAPickDetailCarton>();
            //var cartonsInDb = _context.FBACartonLocations
            //    .Include(x => x.FBAPickDetails)
            //    .Include(x => x.FBAPickDetailCartons)
            //    .Include(x => x.FBAPallet.FBAPalletLocations)
            //    .Where(x => x.FBAPickDetails.Count != 0 && x.FBAPickDetailCartons.Count != 0)
            //    .ToList();

            //var pickDetails = _context.FBAPickDetails
            //    .Include(x => x.FBACartonLocation)
            //    .Include(x => x.FBAPickDetailCartons)
            //    .Include(x => x.FBAPalletLocation)
            //    .Where(x => x.FBACartonLocation == null && x.FBAPalletLocation == null)
            //    .ToList();

            //foreach (var c in cartonsInDb)
            //{
            //    foreach(var p in c.FBAPickDetails)
            //    {
            //        var newPickCarton = new FBAPickDetailCarton {
            //            PickCtns = p.PickableCtns,
            //            FBACartonLocation = c,
            //            FBAPickDetail = p
            //        };
            //        list.Add(newPickCarton);
            //        p.FBAPalletLocation = c.FBAPallet.FBAPalletLocations.First();
            //    }
            //    c.FBAPickDetails = null;
            //}

            //_context.FBAPickDetailCartons.AddRange(list);
            //_context.SaveChanges();
            var cartonInDb = _context.FBACartonLocations
                .Include(x => x.FBAPickDetailCartons)
                .Include(x => x.FBAPickDetails)
                .Where(x => x.Id > 0);

            var pickCartonsIndB = _context.FBAPickDetailCartons
                .Include(x => x.FBAPickDetail.FBAShipOrder)
                .Include(x => x.FBACartonLocation)
                .Where(x => x.Id > 0);

            var pickDetailsInDb = _context.FBAPickDetails
                .Include(x => x.FBAShipOrder)
                .Include(x => x.FBAPickDetailCartons)
                .Where(x => x.Id > 0);

            foreach (var p in pickCartonsIndB)
            {
                if (p.FBAPickDetail.FBAShipOrder.Status == FBAStatus.Shipped)
                {
                    p.FBACartonLocation.ShippedCtns += p.PickCtns;
                }
                else
                {
                    p.FBACartonLocation.PickingCtns += p.PickCtns;
                }
            }

            foreach (var p in pickDetailsInDb)
            {
                if (p.FBACartonLocation != null)
                {
                    if (p.FBAShipOrder.Status == FBAStatus.Shipped)
                    {
                        p.FBACartonLocation.ShippedCtns += p.ActualQuantity;
                    }
                    else
                    {
                        p.FBACartonLocation.PickingCtns += p.ActualQuantity;
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
    }
}