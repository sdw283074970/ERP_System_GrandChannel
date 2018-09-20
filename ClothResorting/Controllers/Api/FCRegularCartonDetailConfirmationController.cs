using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using System.Web;

namespace ClothResorting.Controllers.Api
{
    public class FCRegularCartonDetailConfirmationController : ApiController
    {
        private ApplicationDbContext _context;
        private DateTime _timeNow;
        private string _userName;

        public FCRegularCartonDetailConfirmationController()
        {
            _context = new ApplicationDbContext();
            _timeNow = DateTime.Now;
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0];
        }

        // PUT /api/RegularCartonDetailConfirmation
        //将所有被选中的对象(id数组)视为正常正确收货，即实际收货数量及库存数量直接等于应收货数量
        [HttpPut]
        public void UpdateReceiving([FromBody]int[] arr)
        {
            Array.Sort(arr);
            var firstId = arr[0];
            var lastId = arr.Last();

            var regularCartonDetailInDbs = _context.RegularCartonDetails
                .Include(c => c.POSummary.PreReceiveOrder)
                .Where(c => c.Id >= firstId && c.Id <= lastId);

            var poSummaryId = _context.RegularCartonDetails
                .Include(c => c.POSummary.PreReceiveOrder)
                .SingleOrDefault(c => c.Id == firstId)
                .POSummary
                .Id;

            foreach (var id in arr)
            {
                var cartonRange = _context.RegularCartonDetails
                    .Include(c => c.POSummary.PreReceiveOrder)
                    .SingleOrDefault(c => c.Id == id)
                    .CartonRange;

                var inOneBoxSKUs = _context.RegularCartonDetails
                .Include(c => c.POSummary.PreReceiveOrder)
                .Where(x => x.CartonRange == cartonRange
                    && x.POSummary.Id == poSummaryId);

                var index = 1;

                foreach(var regularCaronDetailInDb in inOneBoxSKUs)
                {
                    //更新收货人
                    regularCaronDetailInDb.Receiver = _userName;

                    //更新自身的可分配件数箱数
                    regularCaronDetailInDb.ToBeAllocatedPcs = regularCaronDetailInDb.Quantity;
                    regularCaronDetailInDb.Status = "To Be Allocated";

                    //只为第一个SKU同步箱数
                    if (index == 1)
                    {
                        regularCaronDetailInDb.ToBeAllocatedCtns = regularCaronDetailInDb.Cartons;
                        regularCaronDetailInDb.ActualCtns = regularCaronDetailInDb.Cartons;
                        regularCaronDetailInDb.POSummary.ActualCtns += regularCaronDetailInDb.Cartons;
                        regularCaronDetailInDb.POSummary.PreReceiveOrder.ActualReceivedCtns += regularCaronDetailInDb.Cartons;
                    }

                    //更新自身的实际收货件数箱数
                    regularCaronDetailInDb.ActualPcs = regularCaronDetailInDb.Quantity;
                    regularCaronDetailInDb.InboundDate = _timeNow;

                    // 同步POSummary
                    regularCaronDetailInDb.POSummary.ActualPcs += regularCaronDetailInDb.Quantity;

                    // 同步PreReceiveOrder
                    regularCaronDetailInDb.POSummary.PreReceiveOrder.ActualReceivedPcs += regularCaronDetailInDb.Quantity;

                    index++;
                }
            }

            _context.SaveChanges();
        }
    }
}
