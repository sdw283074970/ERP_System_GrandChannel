using ClothResorting.FilterAttribute;
using ClothResorting.Models.StaticClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClothResorting.Controllers
{
    [GDFilter]
    public class ThirdPartyLogisticsController : Controller
    {
        // 主页/预收货页面/工作订单页面
        public ActionResult Index()
        {
            if (User.IsInRole(RoleName.CanDeleteEverything))
                return View("AdminIndex");

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
        public ActionResult ReplenishmentLocationDetail()
        {
            return View();
        }

        // RegularLocationDetail 批量Replenishment类型po的上传/查询库存细节页面
        public ActionResult FCRegularLocationAllocating()
        {
            return View();
        }

        // PickReplenishmentOrder 根据Loadplan提取ReplenishmentOrder页面
        public ActionResult PickReplenishmentOrder()
        {
            return View();
        }

        // GeneralLocManagement 普通库位的管理页面
        public ActionResult GeneralLocManagement()
        {
            return View();
        }

        // GeneralLocDetail 普通库位的细节页面
        public ActionResult GeneralLocDetail()
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

            if (User.IsInRole(RoleName.CanDeleteEverything))
            {
                return View("AdminPurchaseOrderOverview");
            }

            return View();
        }

        // FC的Regular订单详情页面
        public ActionResult FCRegularCartonDetail()
        {
            if (User.IsInRole(RoleName.CanDeleteEverything))
                return View("AdminRegularCartonDetail");

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

        // ShipOrder管理页面
        public ActionResult ShipOrder()
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

        // PullSheet诊断页面
        public ActionResult PullSheetDiagnostics()
        {
            return View();
        }

        // 可编辑注释的Receiving Report页面
        public ActionResult ReceivingReport()
        {
            return View();
        }

        //库存搜索页面
        public ActionResult InventorySearch()
        {
            return View();
        }

        //装箱单搜索页面
        public ActionResult PackingListSearch()
        {
            return View();
        }

        //混合订单PO总览页面
        public ActionResult MixPurchaseOrderOverview()
        {
            return View();
        }

        //Invoice管理页面
        public ActionResult InvoiceManagement()
        {
            return View();
        }

        //ChargingItem管理页面
        public ActionResult ChargingItemManagement()
        {
            return View();
        }

        //Invoice细节页面
        public ActionResult InvoiceDetail()
        {
            return View();
        }

        //发货历史查询页面
        public ActionResult ShipDetailHistory()
        {
            return View();
        }
    }
}