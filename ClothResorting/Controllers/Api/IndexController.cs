using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Helpers;
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
    public class IndexController : ApiController
    {
        private ApplicationDbContext _context;

        public IndexController()
        {
            _context = new ApplicationDbContext();
        }

        //GET /api/index/?departmentCode={departmentCode}&timeFrom={timeFrom}&timeTo={timeTo}
        public IHttpActionResult GetLatestMonthPrereceiveOrder([FromUri]string departmentCode, [FromUri]DateTime timeFrom, [FromUri]DateTime timeTo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = GetPreReceiveOrdersByTime(departmentCode, timeFrom, timeTo);

            return Ok(result);
        }

        // POST /api/index/?orderType={orderType}&vendor={vendor}
        [HttpPost]
        public IHttpActionResult CreateNewPrereceiveOrder([FromUri]string orderType, [FromUri]string vendor)
        {
            var extractor = new ExcelExtracter();
            extractor.CreatePreReceiveOrder(orderType, vendor);

            var sample = _context.PreReceiveOrders.OrderByDescending(x => x.Id).First();
            var sampleDto = Mapper.Map<PreReceiveOrder, PreReceiveOrdersDto>(sample);

            return Created(Request.RequestUri + "/" + sample.Id, sampleDto);
        }

        // PUT /api/index/?preId={preId}&inboundDate={inboundDate}
        [HttpPut]
        public void UpdatePrereceiveOrder([FromUri]int preId, [FromUri]DateTime inboundDate)
        {
            var preInDb = _context.PreReceiveOrders.Find(preId);
            preInDb.InboundDate = inboundDate;
            _context.SaveChanges();
        }

        // POST /api/index/{preid} 删除当前preid下的所有空的POSummary对象
        [HttpDelete]
        public void DeleteCurrentPackingList([FromUri]int id)
        {
            var poSummariesInDb = _context.POSummaries
                .Include(x => x.PreReceiveOrder)
                .Include(x => x.RegularCartonDetails)
                .Where(x => x.PreReceiveOrder.Id == id
                    && x.RegularCartonDetails.Count == 0);

            _context.POSummaries.RemoveRange(poSummariesInDb);
            _context.SaveChanges();
        }

        IEnumerable<PreReceiveOrdersDto> GetPreReceiveOrdersByTime(string departmentCode, DateTime timeFrom, DateTime timeTo)
        {
            timeTo = timeTo.AddDays(1);

            var preReceiveOrderLists = _context.PreReceiveOrders
                .Include(x => x.UpperVendor)
                .Include(x => x.POSummaries.Select(c => c.RegularCartonDetails))
                .Where(x => x.UpperVendor.DepartmentCode == departmentCode
                    && x.CreatDate >= timeFrom
                    && x.CreatDate < timeTo)
                .ToList();

            var regualrCarton = _context.RegularCartonDetails
                .Include(x => x.POSummary.PreReceiveOrder)
                .Where(x => x.POSummary.PreReceiveOrder.CreatDate >= timeFrom
                    && x.POSummary.PreReceiveOrder.CreatDate < timeTo)
                .ToList();

            foreach (var p in preReceiveOrderLists)
            {
                if (p.POSummaries.Count != 0)
                {
                    try
                    {
                        p.ActualReceivedCtns = regualrCarton.Where(x => x.POSummary.PreReceiveOrder.Id == p.Id).Sum(x => x.ActualCtns);
                        p.ActualReceivedPcs = regualrCarton.Where(x => x.POSummary.PreReceiveOrder.Id == p.Id).Sum(x => x.ActualPcs);
                        p.TotalCartons = regualrCarton.Where(x => x.POSummary.PreReceiveOrder.Id == p.Id).Sum(x => x.Cartons);
                        p.TotalPcs = regualrCarton.Where(x => x.POSummary.PreReceiveOrder.Id == p.Id).Sum(x => x.Quantity);
                    }
                    catch (Exception e)
                    {
                        continue;
                    }
                }
            }

            var result = Mapper.Map<IEnumerable<PreReceiveOrder>, IEnumerable<PreReceiveOrdersDto>>(preReceiveOrderLists);

            return result;
        }
    }
}
