using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Helpers;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Newtonsoft.Json;
using System.Web.Http.ModelBinding;

namespace ClothResorting.Controllers.Api.Filters
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext.ModelState.IsValid == false)
            {
                var modelState = actionContext.ModelState;
                var innerMessages = new List<JsonResponseInnerMessage>();

                foreach(var m in modelState)
                {
                    if (m.Value.Errors.Count != 0)
                    {
                        foreach (var e in m.Value.Errors)
                            innerMessages.Add(new JsonResponseInnerMessage { Field = m.Key, Message = e.ErrorMessage });
                    }
                }

                actionContext.Response = actionContext.Request.CreateResponse<JsonResponse>(HttpStatusCode.BadRequest, new JsonResponse { Code = 503, ValidationStatus = "Failed", Message = "Faild to validate request body. See inner message.", InnerMessage = innerMessages });
                //actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest, actionContext.ModelState);
            }
        }
    }
}