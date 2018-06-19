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
        public ActionResult PackingListOverview()
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

        // 搜索页面
        public ActionResult Search()
        {
            return View();
        }

        // 从库存取货页面
        public ActionResult RetrievingDetail()
        {
            return View();
        }

        // 散货收取/添加库存页面
        public ActionResult NewBulkload()
        {
            return View();
        }

        // CartonBreakdown查询可用库存去向页面
        public ActionResult CartonBreakdownOutbound()
        {
            return View();
        }
        
        // LocationDetail 批量上传/查询库存细节页面
        public ActionResult LocationDetail()
        {
            return View();
        }
    }
}