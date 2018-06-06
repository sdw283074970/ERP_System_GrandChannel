using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClothResorting.Controllers
{
    public class ThirdPartyLogisticsController : Controller
    {
        // 主页/预收货页面
        public ActionResult Index()
        {
            return View();
        }

        // 预收货细节页面/PO统计页面
        public ActionResult PrereceiveOrderDetail()
        {
            return View();
        }

        // 箱子数量收货页面
        public ActionResult CartonDetail()
        {
            return View();
        }

        // 详细到pcs数量的收货页面
        public ActionResult CartonBreakdown()
        {
            return View();
        }

        // 库位分配页面
        public ActionResult Location()
        {
            return View();
        }
    }
}