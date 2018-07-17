using ClothResorting.Models;
using ClothResorting.Models.ApiTransformModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;

namespace ClothResorting.Controllers.Api
{
    public class FCPurchaseOrderOverviewController : ApiController
    {
        private ApplicationDbContext _context;

        public FCPurchaseOrderOverviewController()
        {
            _context = new ApplicationDbContext();
        }

        // PUT /api/fcpurchaseorderoverview 更新pl的container信息
        [HttpPut]
        public void UpdateContainer([FromBody]IntArrayIntString obj)
        {
            var arr = obj.arr;
            var container = obj.container;
            var preId = obj.preId;

            var poSummariesInDb = _context.POSummaries
                .Include(c => c.PreReceiveOrder)
                .Where(c => c.PreReceiveOrder.Id == preId);

            foreach(var id in arr)
            {
                poSummariesInDb.SingleOrDefault(c => c.Id == id).Container = container;
            }
            _context.SaveChanges();
        }
    }
}
