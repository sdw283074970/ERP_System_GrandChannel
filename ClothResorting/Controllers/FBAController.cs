using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClothResorting.Controllers
{
    public class FBAController : Controller
    {
        // GET: FBA
        public ActionResult Index()
        {
            return View();
        }

        //收费模板页面
        public ActionResult StorageCharge()
        {
            return View();
        }

        //收费细则页面
        public ActionResult ChargeMethods()
        {
            return View();
        }
    }
}