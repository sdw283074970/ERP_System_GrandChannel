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

    public class CompanyFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!(HttpContext.Current.User.IsInRole(RoleName.CanOperateAsT3) || HttpContext.Current.User.IsInRole(RoleName.CanOperateAsT4) || HttpContext.Current.User.IsInRole(RoleName.CanOperateAsT2)))
            {
                filterContext.Result = new RedirectToRouteResult("Denied", new System.Web.Routing.RouteValueDictionary(new { Controller = "Home", Action = "Denied" }), false);
            }

            base.OnActionExecuting(filterContext);
        }
    }

    public class OfficeFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!(HttpContext.Current.User.IsInRole(RoleName.CanOperateAsT3) || HttpContext.Current.User.IsInRole(RoleName.CanOperateAsT4)))
            {
                filterContext.Result = new RedirectToRouteResult("Denied", new System.Web.Routing.RouteValueDictionary(new { Controller = "Home", Action = "Denied" }), false);
            }

            base.OnActionExecuting(filterContext);
        }
    }

    public class FBAWHFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!(HttpContext.Current.User.IsInRole(RoleName.CanOperateAsT3) || HttpContext.Current.User.IsInRole(RoleName.CanOperateAsT2)))
            {
                filterContext.Result = new RedirectToRouteResult("Denied", new System.Web.Routing.RouteValueDictionary(new { Controller = "Home", Action = "Denied" }), false);
            }

            base.OnActionExecuting(filterContext);
        }
    }

    public class FDFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (HttpContext.Current.User.IsInRole(RoleName.CanOperateAsT5))
            {
                filterContext.Result = new RedirectToRouteResult("Denied", new System.Web.Routing.RouteValueDictionary(new { Controller = "Home", Action = "Denied" }), false);
            }

            base.OnActionExecuting(filterContext);
        }
    }

    public class GDFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!HttpContext.Current.User.IsInRole(RoleName.CanOperateAsT4))
            {
                filterContext.Result = new RedirectToRouteResult("Denied", new System.Web.Routing.RouteValueDictionary(new { Controller = "Home", Action = "Denied" }), false);
            }

            base.OnActionExecuting(filterContext);
        }
    }

    public class FBAFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!HttpContext.Current.User.IsInRole(RoleName.CanDeleteEverything))
            {
                filterContext.Result = new RedirectToRouteResult("Denied", new System.Web.Routing.RouteValueDictionary(new { Controller = "Home", Action = "Denied" }), false);
            }

            base.OnActionExecuting(filterContext);
        }
    }

    public class WarehouseFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!HttpContext.Current.User.IsInRole(RoleName.CanOperateAsT2))
            {
                filterContext.Result = new RedirectToRouteResult("Denied", new RouteValueDictionary(new
                {
                    controller = "Home",
                    action = "Denied",
                    area = ""
                }), false);
            }

            base.OnActionExecuting(filterContext);
        }
    }

    public class CustomerFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!HttpContext.Current.User.IsInRole(RoleName.CanViewAsClientOnly))
            {
                filterContext.Result = new RedirectToRouteResult("Denied", new RouteValueDictionary(new
                {
                    controller = "Home",
                    action = "Denied",
                    area = ""
                }), false);
            }

            base.OnActionExecuting(filterContext);
        }
    }
} 