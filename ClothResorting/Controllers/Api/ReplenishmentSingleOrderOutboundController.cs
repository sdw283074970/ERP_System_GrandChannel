using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Helpers;
using ClothResorting.Models;
using ClothResorting.Models.DataTransferModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ClothResorting.Controllers.Api
{
    public class ReplenishmentSingleOrderOutboundController : ApiController
    {
        private ApplicationDbContext _context;

        public ReplenishmentSingleOrderOutboundController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/replineshmentSingleOrder
        [HttpPost]
        public IHttpActionResult CreateSingleReplenishmentOrderRecord([FromBody]PickRequest request)
        {
            var list = new List<PickRequest>();
            var recorder = new OutboundRecorder();

            list.Add(request);

            var result = recorder.OutputReplenishmentOrderIORecord(list);
            var resultDto = Mapper.Map<IEnumerable<PermanentLocIORecord>, IEnumerable<PermanentLocIORecordDto>>(result);

            return Created(Request.RequestUri + "/" + 555, resultDto);
        }
    }
}
