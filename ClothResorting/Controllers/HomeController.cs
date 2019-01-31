using ClothResorting.Helpers;
using ClothResorting.Models;
using ClothResorting.Models.FBAModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClothResorting.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private ApplicationDbContext _context;


        public HomeController()
        {
            _context = new ApplicationDbContext();

        }

        public ActionResult Index()
        {

            return View();
        }

        public ActionResult Test()
        {
            var generator = new PDFGenerator();
            var bolDetailList = new List<FBABOLDetail>();

            bolDetailList.Add(new FBABOLDetail() {
                CustoerOrderNumber = "SKU12344556678",
                CartonQuantity = 300,
                Location = "QQQ",
                Contianer = "TEST123456789",
                Weight = 200f,
                PalletQuantity = 0
            });

            generator.GenerateFBABOL(3, bolDetailList);

            ViewBag.Message = "Your application description page.";

            return View();  
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}