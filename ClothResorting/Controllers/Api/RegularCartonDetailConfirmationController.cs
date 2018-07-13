using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;

namespace ClothResorting.Controllers.Api
{
    public class RegularCartonDetailConfirmationController : ApiController
    {
        private ApplicationDbContext _context;
        private DateTime _timeNow;

        public RegularCartonDetailConfirmationController()
        {
            _context = new ApplicationDbContext();
            _timeNow = DateTime.Now;
        }

        // PUT /api/RegularCartonDetailConfirmation
        //将所有被选中的对象(id数组)视为正常正确收货，即实际收货数量及库存数量直接等于应收货数量
        [HttpPut]
        public void UpdateReceiving([FromBody]int[] arr)
        {
            var firstId = arr[0];
            var lastId = arr.Last();

            var regularCartonDetailInDbs = _context.RegularCartonDetails
                .Include(c => c.POSummary.PreReceiveOrder)
                .Where(c => c.Id >= firstId && c.Id <= lastId);

            foreach(var id in arr)
            {
                var regularCaronDetailInDb = regularCartonDetailInDbs
                    .Include(c => c.POSummary.PreReceiveOrder)
                    .SingleOrDefault(c => c.Id == id);

                regularCaronDetailInDb.ActualPcs = regularCaronDetailInDb.Quantity;
                regularCaronDetailInDb.ActualCtns = regularCaronDetailInDb.Cartons;
                regularCaronDetailInDb.InboundDate = _timeNow;

                // 同步POSummary
                regularCaronDetailInDb.POSummary.ActualPcs += regularCaronDetailInDb.Quantity;
                regularCaronDetailInDb.POSummary.ActualCtns += regularCaronDetailInDb.Cartons;

                // 同步PreReceiveOrder
                regularCaronDetailInDb.POSummary.PreReceiveOrder.ActualReceivedPcs += regularCaronDetailInDb.Quantity;
                regularCaronDetailInDb.POSummary.PreReceiveOrder.ActualReceivedCtns += regularCaronDetailInDb.Cartons;
            }

            _context.SaveChanges();
        }
    }
}
