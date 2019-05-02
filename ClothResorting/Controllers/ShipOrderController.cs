using ClothResorting.FilterAttribute;
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
            return View("~/Views/Home/Error.cshtml");
        }

        // GET FBA发货单页面
        [FBAFilter]
        public ActionResult FBAShipOrder()
        {
            return View();
        }

        // GET FBA拣货页面
        [FBAFilter]
        public ActionResult FBAPickDetail()
        {
            return View();
        }

        // GET FBA拣货页面（只读）
        [FBAWHFilter]
        public ActionResult ViewFBAPickDetail()
        {
            return View();
        }

        // GET FBA拣货页面（调节）
        [FBAWHFilter]
        public ActionResult FBAPickDetailAdjustOnly()
        {
            return View();
        }
    }
}