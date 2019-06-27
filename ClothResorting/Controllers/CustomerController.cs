using ClothResorting.FilterAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClothResorting.Controllers
{
    [OfficeFilter]
    public class CustomerController : Controller
    {
        // GET: Customers
        public ActionResult Index()
        {
            return View();
        }

        // GET: ChargeItems
        public ActionResult ChargeItemLists()
        {
            return View();
        }

        public ActionResult ManageInvoices()
        {
            return View("~/Views/ThirdPartyLogistics/InvoiceManagement.cshtml");
        }
    }
}