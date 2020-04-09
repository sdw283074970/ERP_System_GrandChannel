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
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0] == "" ? (HttpContext.Current.Request.Headers.Get("AppUser") == null ? "" : HttpContext.Current.Request.Headers.Get("AppUser").Split('@')[0]) : HttpContext.Current.User.Identity.Name.Split('@')[0];
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

                foreach(var regularCartonDetailInDb in inOneBoxSKUs)
                {
                    //只有当实收件数等于0的时候批量收货才会影响到该SKU，防止意外点击批量收获双倍添加实收货物数量
                    if (regularCartonDetailInDb.ActualPcs == 0 )
                    {
                        //更新收货人
                        regularCartonDetailInDb.Receiver = _userName;

                        //更新自身的可分配件数箱数
                        regularCartonDetailInDb.ToBeAllocatedPcs = regularCartonDetailInDb.Quantity;
                        regularCartonDetailInDb.Status = "To Be Allocated";

                        //只为第一个SKU同步箱数
                        if (index == 1)
                        {
                            regularCartonDetailInDb.ToBeAllocatedCtns = regularCartonDetailInDb.Cartons;
                            regularCartonDetailInDb.ActualCtns = regularCartonDetailInDb.Cartons;
                            regularCartonDetailInDb.POSummary.ActualCtns += regularCartonDetailInDb.Cartons;
                            regularCartonDetailInDb.POSummary.PreReceiveOrder.ActualReceivedCtns += regularCartonDetailInDb.Cartons;
                        }

                        //更新自身的实际收货件数箱数
                        regularCartonDetailInDb.ActualPcs = regularCartonDetailInDb.Quantity;

                        // 同步POSummary
                        regularCartonDetailInDb.POSummary.ActualPcs += regularCartonDetailInDb.Quantity;

                        // 同步PreReceiveOrder
                        regularCartonDetailInDb.POSummary.PreReceiveOrder.ActualReceivedPcs += regularCartonDetailInDb.Quantity;

                        index++;
                    }
                }
            }

            _context.SaveChanges();
        }
    }
}
