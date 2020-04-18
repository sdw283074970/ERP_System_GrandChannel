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
                return Ok(former.GetInboundAndOutboundPltsChartData(DateTime.Now));
            }
            else if (operation == "GetInboundAndOutboundCtnsData")
            {
                return Ok(former.GetInboundAndOutboundCtnsChartData(DateTime.Now));
            }
            else if (operation == "GetInboundAndOutboundIncomesData")
            {
                return Ok(former.GetInboundAndOutboundIncomesChartData(DateTime.Now));
            }
            else if (operation == "GetInboundAndOutboundCostsData")
            {
                return Ok(former.GetInboundAndOutboundCostsChartData(DateTime.Now));
            }
            else if (operation == "GetInboundAndOutboundProfitsData")
            {
                return Ok(former.GetInboundAndOutboundProfitsChartData(DateTime.Now));
            }

            return Ok();
        }
    }
}
