using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClothResorting.Controllers
{
    public class EFolderController : Controller
    {
        // GET: EFolder
        public ActionResult Index()
        {
            return View();
        }

        // GET FBAEFolder页面
        public ActionResult FBAEFolder()
        {
            return View();
        }
    }
}