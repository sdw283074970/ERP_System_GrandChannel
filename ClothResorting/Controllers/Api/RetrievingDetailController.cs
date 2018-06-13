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
using ClothResorting.Dtos;
using AutoMapper;

namespace ClothResorting.Controllers.Api
{
    public class RetrievingDetailController : ApiController
    {
        private ApplicationDbContext _context;

        public RetrievingDetailController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/RetrievingDetail 给出拣货要求，输出拣货单
        [HttpPost]
        public IHttpActionResult CreateRetrievingRecord([FromBody]RetrievingRequestJsonObj obj)
        {
            var result = new List<RetrievingRecordDto>();
            var loadPlan = new LoadPlanRecord {
                PurchaseOrder = obj.PurchaseOrder,
                OutBoundDate = DateTime.Now,
                OutBoundPcs = 0,
                OutBoundCtns = 0
            };

            var cartonBreakdowns = _context.CartonBreakDowns
                .Include(c => c.CartonDetail)
                .Include(c => c.PackingList.PreReceiveOrder)
                .Where(c => c.PurchaseOrder == obj.PurchaseOrder 
                    && c.AvailablePcs != 0 
                    && c.RunCode == "");

            //分别按照style、color、size筛选
            cartonBreakdowns = cartonBreakdowns.Where(c => c.Style == obj.Style);
            cartonBreakdowns = cartonBreakdowns.Where(c => c.Color == obj.Color);
            var cartons = cartonBreakdowns.Where(c => c.Size == obj.Size).ToList();

            var calculator = new CartonsCalculator();

            //调用Helper中CartonsCalculator的方法
            var query = calculator.CalculateCartons(cartons, obj.GrandTotal, loadPlan).ToList();

            _context.LoadPlanRecords.Add(loadPlan);
            _context.RetrievingRecords.AddRange(query);
            _context.SaveChanges();

            var count = query.Count;

            var q = _context.RetrievingRecords
                .OrderByDescending(r => r.Id)
                .Select(Mapper.Map<RetrievingRecord, RetrievingRecordDto>)
                .Take(count)
                .OrderBy(r => r.Id);

            return Created(Request.RequestUri + "/" + 222, q);
        }
    }
}
