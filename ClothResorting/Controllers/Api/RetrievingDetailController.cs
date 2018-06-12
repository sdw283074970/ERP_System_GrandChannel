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
                OutBoundDate = DateTime.Today,
                OutBoundPcs = 0,
                OutBoundCtns = 0
            };

            var cartonBreakdowns = _context.CartonBreakDowns
                .Include(c => c.SilkIconCartonDetail)
                .Include(c => c.SilkIconPackingList.SilkIconPreReceiveOrder)
                .Where(c => c.PurchaseNumber == obj.PurchaseOrder && c.AvailablePcs != 0);

            cartonBreakdowns = cartonBreakdowns.Where(c => c.Style == obj.Style);
            cartonBreakdowns = cartonBreakdowns.Where(c => c.Color == obj.Color);
            var cartons = cartonBreakdowns.Where(c => c.Size == obj.Size).ToList();

            var calculator = new CartonsCalculator();

            var query = calculator.CalculateCartons(cartons, obj.GrandTotal, loadPlan).ToList();

            _context.LoadPlanRecords.Add(loadPlan);
            _context.SaveChanges();

            result.AddRange(Mapper.Map<List<RetrievingRecord>, List<RetrievingRecordDto>>(query));

            return Created(Request.RequestUri + "/" + 123, result);
        }
    }
}
