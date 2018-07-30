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
        public ActionResult SIPurchaseOrderOverview()
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

        // LocationDetail 批量Replenishment类型po的上传/查询库存细节页面
        public ActionResult LocationDetail()
        {
            return View();
        }

        // RegularLocationDetail 批量Replenishment类型po的上传/查询库存细节页面
        public ActionResult FCRegularLocationAllocating()
        {
            return View();
        }

        // GrabReplenishmentOrder 根据Loadplan提取ReplenishmentOrder页面
        public ActionResult GrabReplenishmentOrder()
        {
            return View();
        }

        // PermanentLocManagement 固定库位的管理页面
        public ActionResult PermanentLocManagement()
        {
            return View();
        }

        // 全库的PO管理页面
        public ActionResult PurchaseOrderManagement()
        {
            return View();
        }

        // 每一个PO中所包含内容的统计页面
        public ActionResult PurchaseOrderStatistics()
        {
            return View();
        }

        // 每一个Species包含的调整记录页面
        public ActionResult AdjustmentDetail()
        {
            return View();
        }

        // 每一个Species的进库出库记录页面
        public ActionResult IOHistory()
        {
            return View();
        }

        // FC的预收货细节页面/PO统计页面
        public ActionResult FCPurchaseOrderOverview()
        {
            return View();
        }

        // FC的Regular订单详情页面
        public ActionResult FCRegularCartonDetail()
        {
            return View();
        }

        // 以集装箱为单位，FC的收货报告，允许客户的客户匿名访问
        [AllowAnonymous]
        public ActionResult FCReceivingReport()
        {
            return View();
        }

        // FC正常订单的库位分布查询
        public ActionResult FCRegularLocationDetail()
        {
            return View();
        }

        // PickingList管理页面
        public ActionResult PickingListManagement()
        {
            return View();
        }

        // PickingList记录页面
        public ActionResult PickingRecords()
        {
            return View();
        }

        // ShipOrder管理页面
        public ActionResult ShipOrder()
        {
            return View();
        }

        // PullSheet管理页面
        public ActionResult PullSheet()
        {
            return View();
        }

        // PickDetail管理页面
        public ActionResult PickDetail()
        {
            return View();
        }

        // PickDetail总览页面
        public ActionResult PickDetailSummary()
        {
            return View();
        }
    }
}