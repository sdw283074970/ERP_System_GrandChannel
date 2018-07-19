using ClothResorting.Models;
using ClothResorting.Models.ApiTransformModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using ClothResorting.Helpers;
using AutoMapper;
using ClothResorting.Dtos;

namespace ClothResorting.Controllers.Api
{
    public class FCRegularLocationAllocatingBatchController : ApiController
    {
        private ApplicationDbContext _context;
        private DateTime _timeNow;

        public FCRegularLocationAllocatingBatchController()
        {
            _context = new ApplicationDbContext();
            _timeNow = DateTime.Now;
        }

        // POST /api/FCRegularLocationAllocatingBatch
        [HttpPost]
        public IHttpActionResult CreateBatchLocationDetail([FromBody]ArrPreIdLocationJsonObj obj)
        {
            var locationDeatilList = new List<FCRegularLocationDetail>();
            var regularCartonDetailsIndb = _context.RegularCartonDetails
                .Include(c => c.POSummary.PreReceiveOrder)
                .Where(c => c.POSummary.PreReceiveOrder.Id == obj.PreId);

            foreach(var id in obj.Arr)
            {
                var regularCartonDetail = regularCartonDetailsIndb.SingleOrDefault(c => c.Id == id);

                locationDeatilList.Add(new FCRegularLocationDetail {
                    Container = regularCartonDetail.POSummary.Container,
                    PurchaseOrder = regularCartonDetail.PurchaseOrder,
                    Style = regularCartonDetail.Style,
                    Color = regularCartonDetail.Color,
                    CustomerCode = regularCartonDetail.Customer,
                    SizeBundle = regularCartonDetail.SizeBundle,
                    PcsBundle = regularCartonDetail.PcsBundle,
                    Cartons = regularCartonDetail.ToBeAllocatedCtns,
                    Quantity = regularCartonDetail.ToBeAllocatedPcs,
                    PcsPerCaron = regularCartonDetail.PcsPerCarton,
                    Status = "New Inbound",
                    Location = obj.Location,
                    InboundDate = _timeNow,
                    PreReceiveOrder = regularCartonDetailsIndb.First().POSummary.PreReceiveOrder
                });

                regularCartonDetailsIndb.SingleOrDefault(c => c.Id == id).ToBeAllocatedCtns = 0;
                regularCartonDetailsIndb.SingleOrDefault(c => c.Id == id).ToBeAllocatedPcs = 0;
                regularCartonDetailsIndb.SingleOrDefault(c => c.Id == id).Status = "Allocated";
            }

            _context.FCRegularLocationDetails.AddRange(locationDeatilList);
            _context.SaveChanges();

            //获取刚写入数据库的记录
            var latestRecords = _context.FCRegularLocationDetails.OrderByDescending(c => c.Id).Take(locationDeatilList.Count);
            var breaker = new CartonBreaker();

            foreach (var record in latestRecords)
            {
                breaker.BreakCartonBundle(record);
            }

            var recordsDto = Mapper.Map<List<FCRegularLocationDetail>, List<FCRegularLocationDetailDto>>(latestRecords.ToList());

            return Created(Request.RequestUri + "/" + latestRecords.OrderBy(c => c.Id).First().Id + ":" + latestRecords.First().Id, recordsDto);
        }
    }
}
