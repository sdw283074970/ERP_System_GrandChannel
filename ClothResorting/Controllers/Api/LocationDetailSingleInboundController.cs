using AutoMapper;
using ClothResorting.Dtos;
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
    public class LocationDetailSingleInboundController : ApiController
    {
        private ApplicationDbContext _context;
        private DateTime _timeNow = DateTime.Now;

        public LocationDetailSingleInboundController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/locationdetailsingleinbound
        [HttpPost]
        public IHttpActionResult CreateSingleInboundRecord([FromBody]SingleInboundJsonObj obj)
        {
            var purchaseOrderSummaryInDb = _context.PurchaseOrderSummaries
                .Single(c => c.PurchaseOrder == obj.PurchaseOrder);

            var record = new LocationDetail {
                PurchaseOrder = obj.PurchaseOrder,
                Style = obj.Style,
                Color = obj.Color,
                Size = obj.Size,
                InboundDate = _timeNow,
                OrgNumberOfCartons = obj.Ctns,
                InvNumberOfCartons = obj.Ctns,
                OrgPcs = obj.Quantity,
                InvPcs = obj.Quantity,
                Location = obj.Location,
                PurchaseOrderSummary = purchaseOrderSummaryInDb
            };
            
            _context.LocationDetails.Add(record);
            _context.SaveChanges();

            var sample = _context.LocationDetails
                .OrderByDescending(c => c.Id)
                .First();

            //每添加一次单条inbound记录，返回所有结果，实现局部刷新表格
            var result = new List<LocationDetail>();

            var query = _context.LocationDetails
                .Include(c => c.PurchaseOrderSummary.PreReceiveOrder)
                .Where(c => c.PurchaseOrder == obj.PurchaseOrder && c.PurchaseOrderSummary.PreReceiveOrder.Id == obj.PreId)
                .OrderByDescending(c => c.Id)
                .ToList();

            result.AddRange(query);

            var resultDto = Mapper.Map<List<LocationDetail>, List<LocationDetailDto>>(result);

            return Created(Request.RequestUri + "/" + sample.Id, resultDto);
        }
    }
}
