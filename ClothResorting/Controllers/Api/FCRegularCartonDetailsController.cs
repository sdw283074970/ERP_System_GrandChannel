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
using ClothResorting.Models.ApiTransformModels;
using System.Web;
using ClothResorting.Models.StaticClass;

namespace ClothResorting.Controllers.Api
{
    public class FCRegularCartonDetailsController : ApiController
    {
        private ApplicationDbContext _context;
        private DateTime _timeNow;
        private string _userName;

        public FCRegularCartonDetailsController()
        {
            _context = new ApplicationDbContext();
            _timeNow = DateTime.Now;
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0];
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

        // POST /api/fcregularcartondetails/
        [HttpPost]
        public IHttpActionResult CreateBulk([FromBody]RegualrCartonBulkJsonObj obj)
        {
            var poSummaryInDb = _context.POSummaries.Find(obj.Id);

            var newCartonDetail = new RegularCartonDetail
            {
                PurchaseOrder = poSummaryInDb.PurchaseOrder,
                Style = obj.Style,
                Color = obj.Color,
                Customer = poSummaryInDb.Customer,
                Container = poSummaryInDb.Container,
                CartonRange = obj.CartonRange,
                SizeBundle = obj.Size,
                PcsBundle = obj.Pcs,
                PcsPerCarton = obj.Pack,
                Quantity = 0,
                Cartons = 0,
                ActualCtns = 0,
                ActualPcs = 0,
                ToBeAllocatedCtns = 0,
                ToBeAllocatedPcs = 0,
                Status = "To Be Allocated",
                Comment = "This is a bulk sku",
                Operator = _userName,
                Receiver = "",
                Adjustor = "system",
                Vendor = poSummaryInDb.Vendor,
                Batch = poSummaryInDb.Batch,
                POSummary = poSummaryInDb
            };

            if (newCartonDetail.SizeBundle.Split(' ').Count() > 1)
            {
                newCartonDetail.OrderType = OrderType.Prepack;
            }
            else
            {
                newCartonDetail.OrderType = OrderType.SolidPack;
            }

            _context.RegularCartonDetails.Add(newCartonDetail);
            _context.SaveChanges();

            var sample = _context.RegularCartonDetails.OrderByDescending(x => x.Id).First();

            return Created(Request.RequestUri + "/" + sample.Id, Mapper.Map<RegularCartonDetail, RegularCartonDetailDto>(sample));
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
                //更新收货人
                regularCartonDetailInDb.Receiver = _userName;

                //更新调整人
                regularCartonDetailInDb.Adjustor = _userName;

                //将状态改为待分配
                regularCartonDetailInDb.Status = Status.ToBeAllocated;

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

                index++;
            }
            
            _context.SaveChanges();
        }

        // DELETE /api/fcregularcartondetails/{id}
        [HttpDelete]
        public void DeleteCartonDetail([FromUri]int id)
        {
            var regularCartonDetailInDb = _context.RegularCartonDetails
                .Include(x => x.POSummary.PreReceiveOrder)
                .SingleOrDefault(x => x.Id == id);

            regularCartonDetailInDb.POSummary.ActualCtns -= regularCartonDetailInDb.ActualCtns;
            regularCartonDetailInDb.POSummary.Cartons -= regularCartonDetailInDb.Cartons;
            regularCartonDetailInDb.POSummary.PreReceiveOrder.ActualReceivedCtns -= regularCartonDetailInDb.ActualCtns;
            regularCartonDetailInDb.POSummary.PreReceiveOrder.TotalCartons -= regularCartonDetailInDb.Cartons;

            regularCartonDetailInDb.POSummary.ActualPcs -= regularCartonDetailInDb.ActualPcs;
            regularCartonDetailInDb.POSummary.Quantity -= regularCartonDetailInDb.Quantity;
            regularCartonDetailInDb.POSummary.PreReceiveOrder.ActualReceivedPcs -= regularCartonDetailInDb.ActualPcs;
            regularCartonDetailInDb.POSummary.PreReceiveOrder.TotalPcs -= regularCartonDetailInDb.Quantity;

            try
            {
                _context.RegularCartonDetails.Remove(regularCartonDetailInDb);
                _context.SaveChanges();
            }
            catch(Exception e)
            {
                throw new Exception("Cannot delete this carton detail. Because other Location may rely on it.");
            }

        }
    }
}
