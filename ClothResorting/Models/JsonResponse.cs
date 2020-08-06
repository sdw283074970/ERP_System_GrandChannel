using ClothResorting.Controllers.Api.Fba;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models
{
    public class JsonResponse
    {
        public JsonResponse()
        {
            InnerMessage = null;
        }

        public int Code { get; set; }

        public string ValidationStatus { get; set; }

        public string Message { get; set; }

        public dynamic InnerMessage { get; set; }

        public dynamic Body { get; set; }

        public IList<PickingStatus> PickingStatus { get; set; }
    }

    public class JsonResponseInnerMessage
    {
        public string Field { get; set; }

        public string Message { get; set; }
    }
}