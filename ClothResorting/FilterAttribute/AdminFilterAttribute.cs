using ClothResorting.Models.StaticClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ClothResorting.FilterAttribute
{
    public class AdminFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!HttpContext.Current.User.IsInRole(RoleName.CanDeleteEverything))
            {
                filterContext.Result = new RedirectToRouteResult("Denied", new RouteValueDictionary(new {
                    controller = "Home",
                    action = "Denied",
                    area = ""
                }), false);
            }

            base.OnActionExecuting(filterContext);
        }
    }
}