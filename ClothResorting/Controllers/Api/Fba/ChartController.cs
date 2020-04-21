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
        // GET /api/fba/chart/?timeUnit={timeUnit}&timeCount={timeCount}
        [HttpGet]
        public IHttpActionResult GetChartData([FromUri]string timeUnit, [FromUri]int timeCount)
        {
            var former = new ChartFormer();
            var resultList = new List<InboundAndOutboundChartData>();

            resultList.Add(former.GetInboundAndOutboundPltsChartData(DateTime.Today, timeUnit, timeCount));
            resultList.Add(former.GetInboundAndOutboundCtnsChartData(DateTime.Today, timeUnit, timeCount));
            resultList.Add(former.GetInboundAndOutboundIncomesChartData(DateTime.Today, timeUnit, timeCount));
            resultList.Add(former.GetInboundAndOutboundCostsChartData(DateTime.Today, timeUnit, timeCount));
            resultList.Add(former.GetInboundAndOutboundProfitsChartData(DateTime.Today, timeUnit, timeCount));

            return Ok(resultList);
        }
    }
}
