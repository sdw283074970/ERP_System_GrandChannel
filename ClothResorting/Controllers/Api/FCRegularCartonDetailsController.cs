using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using AutoMapper;
using ClothResorting.Dtos;

namespace ClothResorting.Controllers.Api
{
    public class FCRegularCartonDetailsController : ApiController
    {
        private ApplicationDbContext _context;
        private DateTime _timeNow;

        public FCRegularCartonDetailsController()
        {
            _context = new ApplicationDbContext();
            _timeNow = DateTime.Now;
        }

        // GET /api/fcregularcartondetails/{id}
        public IHttpActionResult GetRegularCartonDetails(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var resultDto = _context.RegularCartonDetails
                .Include(c => c.POSummary)
                .Where(c => c.POSummary.Id == id)
                .Select(Mapper.Map<RegularCartonDetail, RegularCartonDetailDto>);

            return Ok(resultDto);
        }

        // PUT /api/fcregularcartondetails 收货算法
        // 考虑到同一个箱子中有多种SKU的情况，操作第一种SKU收货要对同箱的其他SKU同时执行收货，以CartonRange判定是否为同箱SKU
        [HttpPut]
        public void UpdateReceivedCtns([FromBody]int[] arr)
        {
            var id = arr[0];
            var changeValue = arr[1];

            var cartonRange = _context.RegularCartonDetails
                .Include(c => c.POSummary.PreReceiveOrder)
                .SingleOrDefault(c => c.Id == id)
                .CartonRange;

            var poSummaryId = _context.RegularCartonDetails
                .Include(c => c.POSummary.PreReceiveOrder)
                .SingleOrDefault(c => c.Id == id)
                .POSummary
                .Id;

            var inOneBoxSKUs = _context.RegularCartonDetails
                .Include(c => c.POSummary.PreReceiveOrder)
                .Where(x => x.CartonRange == cartonRange
                    && x.POSummary.Id == poSummaryId);

            var index = 1;

            foreach(var regularCartonDetailInDb in inOneBoxSKUs)
            {
                //将状态改为待分配
                regularCartonDetailInDb.Status = "To Be Allocated";

                //只为第一个SKU同步箱数
                if (index == 1)
                {
                    regularCartonDetailInDb.POSummary.ActualCtns += changeValue;
                    regularCartonDetailInDb.POSummary.PreReceiveOrder.ActualReceivedCtns += changeValue;
                    regularCartonDetailInDb.ActualCtns += changeValue;
                    regularCartonDetailInDb.ToBeAllocatedCtns += changeValue;
                }

                //更新待分配的件数和箱数
                regularCartonDetailInDb.ToBeAllocatedPcs += changeValue * regularCartonDetailInDb.PcsPerCarton;

                //更新已收货件数箱数
                regularCartonDetailInDb.ActualPcs += changeValue * regularCartonDetailInDb.PcsPerCarton;

                //同步POSummary的件数
                regularCartonDetailInDb.POSummary.ActualPcs += changeValue * regularCartonDetailInDb.PcsPerCarton;

                //同步PrereceiveOrder的件数
                regularCartonDetailInDb.POSummary.PreReceiveOrder.ActualReceivedPcs += changeValue * regularCartonDetailInDb.PcsPerCarton;

                regularCartonDetailInDb.InboundDate = _timeNow;

                index++;
            }
            
            _context.SaveChanges();
        }
    }
}
