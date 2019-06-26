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

        //GET /api/index/?departmentCode={departmentCode} 获取指定部门的所有的PreReceiveOrders(work order)
        public IHttpActionResult GetPrereceiveOrder([FromUri]string departmentCode)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var preReceiveOrderLists = _context.PreReceiveOrders
                .Include(x => x.UpperVendor)
                .Include(x => x.POSummaries.Select(c => c.RegularCartonDetails))
                .Where(x => x.UpperVendor.DepartmentCode == departmentCode);

            var regualrCartonInDb = _context.RegularCartonDetails
                .Include(x => x.POSummary.PreReceiveOrder);

            foreach (var p in preReceiveOrderLists)
            {
                if (p.POSummaries.Count != 0)
                {
                    try
                    {
                        p.ActualReceivedCtns = regualrCartonInDb.Where(x => x.POSummary.PreReceiveOrder.Id == p.Id).Sum(x => x.ActualCtns);
                        p.ActualReceivedPcs = regualrCartonInDb.Where(x => x.POSummary.PreReceiveOrder.Id == p.Id).Sum(x => x.ActualPcs);
                        p.TotalCartons = regualrCartonInDb.Where(x => x.POSummary.PreReceiveOrder.Id == p.Id).Sum(x => x.Cartons);
                        p.TotalPcs = regualrCartonInDb.Where(x => x.POSummary.PreReceiveOrder.Id == p.Id).Sum(x => x.Quantity);
                    }
                    catch(Exception e)
                    {
                        continue;
                    }
                }
            }

            var result = Mapper.Map<IEnumerable<PreReceiveOrder>, IEnumerable<PreReceiveOrdersDto>>(preReceiveOrderLists);

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
    }
}
