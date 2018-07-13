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

namespace ClothResorting.Controllers.Api
{
    public class FCRegularCartonDetailsController : ApiController
    {
        private ApplicationDbContext _context;

        public FCRegularCartonDetailsController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/fcregularcartondetails/{id}
        public IHttpActionResult GetRegularCartonDetails(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var resultDto = _context.RegularCartonDetails
                .Include(c => c.POSummary)
                .Where(c => c.POSummary.Id == id)
                .Select(Mapper.Map<RegularCartonDetail, RegularCartonDetailDto>);

            return Ok(resultDto);
        }

        // PUT /api/fcregularcartondetails
        [HttpPut]
        public void UpdateReceivedCtns([FromBody]int[] arr)
        {
            var id = arr[0];
            var changeValue = arr[1];

            var regularCartonDetailInDb = _context.RegularCartonDetails
                .Include(c => c.POSummary.PreReceiveOrder)
                .SingleOrDefault(c => c.Id == id);

            regularCartonDetailInDb.ActualCtns += changeValue;
            regularCartonDetailInDb.ActualPcs += changeValue * regularCartonDetailInDb.PcsPerCarton;

            //同步POSummary的箱数、件数
            regularCartonDetailInDb.POSummary.ActualCtns += changeValue;
            regularCartonDetailInDb.POSummary.ActualPcs += changeValue * regularCartonDetailInDb.PcsPerCarton;

            //同步PrereceiveOrder的箱数、件数
            regularCartonDetailInDb.POSummary.PreReceiveOrder.ActualReceivedCtns += changeValue;
            regularCartonDetailInDb.POSummary.PreReceiveOrder.ActualReceivedPcs += changeValue * regularCartonDetailInDb.PcsPerCarton;

            _context.SaveChanges();
        }
    }
}
