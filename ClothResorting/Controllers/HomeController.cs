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

            //var cartonInDb = _context.FBACartonLocations
            //    .Include(x => x.FBAPickDetailCartons)
            //    .Include(x => x.FBAPickDetails)
            //    .Where(x => x.Id > 0);

            //var pickCartonsIndB = _context.FBAPickDetailCartons
            //    .Include(x => x.FBAPickDetail.FBAShipOrder)
            //    .Include(x => x.FBACartonLocation)
            //    .Where(x => x.Id > 0);

            //var pickDetailsInDb = _context.FBAPickDetails
            //    .Include(x => x.FBAShipOrder)
            //    .Include(x => x.FBAPickDetailCartons)
            //    .Where(x => x.Id > 0);

            //foreach (var p in pickCartonsIndB)
            //{
            //    if (p.FBAPickDetail.FBAShipOrder.Status == FBAStatus.Shipped)
            //    {
            //        p.FBACartonLocation.ShippedCtns += p.PickCtns;
            //    }
            //    else
            //    {
            //        p.FBACartonLocation.PickingCtns += p.PickCtns;
            //    }
            //}

            //foreach (var p in pickDetailsInDb)
            //{
            //    if (p.FBACartonLocation != null)
            //    {
            //        if (p.FBAShipOrder.Status == FBAStatus.Shipped)
            //        {
            //            p.FBACartonLocation.ShippedCtns += p.ActualQuantity;
            //        }
            //        else
            //        {
            //            p.FBACartonLocation.PickingCtns += p.ActualQuantity;
            //        }
            //    }
            //}

            DateTime receiveDate;

            var containersInDb = _context.Containers.Where(x => x.Id > 0);

            foreach(var c in containersInDb)
            {
                c.ReceivedDate = UnifyTime(c.ReceivedDate);
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