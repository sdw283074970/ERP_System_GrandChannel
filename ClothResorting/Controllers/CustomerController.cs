using ClothResorting.FilterAttribute;
using ClothResorting.Helpers;
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
            var oauth = new IntuitOAuthor();
            var authURL = oauth.ConnectToQB();

            Response.Redirect(authURL);

            //return View("~/Views/ThirdPartyLogistics/InvoiceManagement.cshtml");
            return View();
        }
    }
}