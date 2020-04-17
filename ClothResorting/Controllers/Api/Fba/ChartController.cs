using ClothResorting.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ClothResorting.Controllers.Api.Fba
{
    public class ChartController : ApiController
    {
        // GET /api/fba/chart/?operation={operation}
        [HttpGet]
        public IHttpActionResult GetChartData([FromUri]string operation)
        {
            var former = new ChartFormer();

            if (operation == "GetInboundAndOutboundPltsData")
            {
                return Ok(former.GetInboundAndOutboundChartData(DateTime.Now));
            }

            return Ok();
        }
    }
}
