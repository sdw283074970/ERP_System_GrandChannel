using ClothResorting.FilterAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClothResorting.Controllers
{
    [FBAWHFilter]
    public class WarehouseController : Controller
    {
        // GET: Warehouse
        public ActionResult Index()
        {
            return View();
        }

        // GET: inbound log
        public ActionResult InboundLog()
        {
            return View();
        }

        // GET: ReceivingPage
        public ActionResult FBAReceiving()
        {
            return View();
        }
    }
}