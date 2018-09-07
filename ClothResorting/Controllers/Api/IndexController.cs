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
        public ApplicationDbContext _context;

        public IndexController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/index/
        [HttpPost]
        public IHttpActionResult CreateNewPrereceiveOrder()
        {
            var extractor = new ExcelExtracter();
            extractor.CreatePreReceiveOrder();

            var sample = _context.PreReceiveOrders.OrderByDescending(x => x.Id).First();
            var sampleDto = Mapper.Map<PreReceiveOrder, PreReceiveOrdersDto>(sample);

            return Created(Request.RequestUri + "/" + sample.Id, sampleDto);
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
