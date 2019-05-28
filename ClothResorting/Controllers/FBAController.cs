using ClothResorting.FilterAttribute;
using ClothResorting.Models.StaticClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClothResorting.Controllers
{
    [FBAFilter]
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

        //客户所有总单页面
        public ActionResult MasterOrder()
        {
            return View();
        }

        //总单详细内容页面
        public ActionResult MasterDetail()
        {
            return View();
        }

        //库内操作内容页面
        public ActionResult WarehouseOperation()
        {
            return View();
        }

        //Pallet/Carton库存分配页面
        public ActionResult FBAAllocating()
        {
            return View();
        }

        //库存查看页面
        public ActionResult Inventory()
        {
            return View("FBAInventory");
        }

        //地址簿管理页面
        public ActionResult FBAAddressManagement()
        {
            return View();
        }

        //库存查询页面
        public ActionResult FBAInventorySearch()
        {
            return View();
        }

        //库存主页
        public ActionResult FBAInventoryIndex()
        {
            return View();
        }

        //库存报告页面
        public ActionResult FBAInventoryReport()
        {
            return View();
        }

        //收费细节页面
        public ActionResult FBAInvoiceDetail()
        {
            return View();
        }

        //库存追踪页面
        public ActionResult FBAOutboundHistory()
        {
            return View();
        }

        //只读Fee页面
        public ActionResult FBAInvoiceDetailReadOnly()
        {
            return View();
        }
    }
}