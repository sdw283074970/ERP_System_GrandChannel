using ClothResorting.Models.StaticClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClothResorting.FilterAttribute
{
    public class OfficeFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!HttpContext.Current.User.IsInRole(RoleName.CanOperateAsT3))
            {
                filterContext.Result = new RedirectToRouteResult("Denied", new System.Web.Routing.RouteValueDictionary(new { Controller = "Home", Action = "Denied"}), false);
            }
            base.OnActionExecuting(filterContext);
        }
    }
}