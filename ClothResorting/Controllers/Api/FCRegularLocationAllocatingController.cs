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
using ClothResorting.Helpers;

namespace ClothResorting.Controllers.Api
{
    public class FCRegularLocationAllocatingController : ApiController
    {
        private ApplicationDbContext _context;

        public FCRegularLocationAllocatingController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/fcregularlocationallocating/{preid} 获取还没有被分配的SKU
        public IHttpActionResult GetUnallocatedCartons([FromUri]int id)
        {
            var result = _context.RegularCartonDetails
                .Include(c => c.POSummary.PreReceiveOrder)
                .Where(c => c.POSummary.PreReceiveOrder.Id == id
                    && c.ToBeAllocatedCtns != 0)
                .Select(Mapper.Map<RegularCartonDetail, RegularCartonDetailDto>);

            return Ok(result);
        }

        // POST /api/fcregularlocationallocating/ 根据传入数据分解已收货对象
        [HttpPost]
        public IHttpActionResult CreateRegularStock([FromBody]FCRegularLocationAllocatingJsonObj obj)
        {
            var cartonDetailInDb = _context.RegularCartonDetails
                .Include(c => c.POSummary.PreReceiveOrder)
                .SingleOrDefault(c => c.Id == obj.Id);

            cartonDetailInDb.Status = "Allocating";

            cartonDetailInDb.ToBeAllocatedCtns -= obj.Cartons;
            cartonDetailInDb.ToBeAllocatedPcs -= obj.Cartons * cartonDetailInDb.PcsPerCarton;

            _context.FCRegularLocationDetails.Add(new FCRegularLocationDetail {
                Container = cartonDetailInDb.POSummary.Container,
                PurchaseOrder = cartonDetailInDb.PurchaseOrder,
                Style = cartonDetailInDb.Style,
                Color = cartonDetailInDb.Color,
                CustomerCode = cartonDetailInDb.Customer,
                SizeBundle = cartonDetailInDb.SizeBundle,
                PcsBundle = cartonDetailInDb.PcsBundle,
                Cartons = obj.Cartons,
                Quantity = obj.Cartons * cartonDetailInDb.PcsPerCarton,
                Location = obj.Location,
                PcsPerCaron = cartonDetailInDb.PcsPerCarton,
                Status = "New Inbound",
                InboundDate = DateTime.Now,
                PreReceiveOrder = cartonDetailInDb.POSummary.PreReceiveOrder
            });

            _context.SaveChanges();

            var latestRecord = _context.FCRegularLocationDetails.OrderByDescending(c => c.Id).First();

            var breaker = new CartonBreaker();

            breaker.BreakCartonBundle(latestRecord);

            var latestRecordDto = Mapper.Map<FCRegularLocationDetail, FCRegularLocationDetailDto>(latestRecord);

            return Created(Request.RequestUri + "/" + latestRecordDto.Id, latestRecordDto);
        }
    }
}
