using ClothResorting.FilterAttribute;
using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Data.Entity;

namespace ClothResorting.Controllers
{
    [CustomerFilter]
    public class CustomerClientController : Controller
    {
        private ApplicationDbContext _context;
        private string _userName;

        public CustomerClientController()
        {
            _context = new ApplicationDbContext();
            _userName = System.Web.HttpContext.Current.User.Identity.Name;
        }

        // GET: CustomerClient
        public ActionResult Index()
        {
            var user = _context.Users
                .SingleOrDefault(x => x.UserName == _userName);

            var userId = user.Id;

            var model = new UserViewModel { UserId = userId };

            return View(model);
        }

        // GET: FBAClientIndex
        public ActionResult FBAClientIndex()
        {
            return View();
        }

        // GET: FBAClientMasterOrder
        public ActionResult FBAClientMasterOrder()
        {
            return View();
        }

        // GET: FBAClientOrderDetail
        public ActionResult FBAClientOrderDetail()
        {
            return View();
        }

        // GET: 库存报告页面
        public ActionResult FBAClientInventoryReport()
        {
            return View();
        }
    }

    public class UserViewModel
    {
        public string UserId { get; set; }
    }
}