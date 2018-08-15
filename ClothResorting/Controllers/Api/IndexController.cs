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
    }
}
