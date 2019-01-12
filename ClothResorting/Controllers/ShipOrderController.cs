using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClothResorting.Controllers
{
    public class ShipOrderController : Controller
    {
        // GET: ShipOrder
        public ActionResult Index()
        {
            return View();
        }

        // GET FBA发货单页面
        public ActionResult FBAShipOrder()
        {
            return View();
        }

        // GET FBA拣货页面
        public ActionResult FBAPickDetail()
        {
            return View();
        }
    }
}