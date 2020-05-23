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

        }

        public int Code { get; set; }

        public string ResultStatus { get; set; }

        public string Message { get; set; }

        public dynamic InnerMessage { get; set; }
    }

    public class JsonResponseInnerMessage
    {
        public string Field { get; set; }

        public string Message { get; set; }
    }
}