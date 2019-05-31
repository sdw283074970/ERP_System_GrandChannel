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
using System.Web;

namespace ClothResorting.Controllers.Api
{
    public class ReceivingReportController : ApiController
    {
        private ApplicationDbContext _context;
        private string _userName;

        public ReceivingReportController()
        {
            _context = new ApplicationDbContext();
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0];
        }

        // GET /receivingreport/?preid={preId}&container={container}
        [HttpGet]
        public IHttpActionResult GetAllCartonDetails([FromUri]int preid, [FromUri]string container)
        {
            var cartonDetails = _context.RegularCartonDetails
                .Include(c => c.POSummary.PreReceiveOrder)
                .Where(c => c.POSummary.PreReceiveOrder.Id == preid
                    && c.POSummary.Container == container)
                .Select(Mapper.Map<RegularCartonDetail, RegularCartonDetailDto>)
                .ToList();

            return Ok(cartonDetails);
        }

        // PUT /receivingreport/?preId={preId}&container={container}
        [HttpPut]
        public void UpdateComment([FromUri]int preId, [FromUri]string container,[FromBody]IEnumerable<PreInfo> objArray)
        {
            var cartonDetailsInDb = _context.RegularCartonDetails
                .Include(x => x.POSummary.PreReceiveOrder)
                .Where(x => x.POSummary.PreReceiveOrder.Id == preId
                    && x.POSummary.Container == container);

            foreach(var o in objArray)
            {
                var detail = cartonDetailsInDb.SingleOrDefault(x => x.Id == o.Id);

                detail.Adjustor = _userName;
                detail.Comment = o.Comment;
                detail.PreLocation = o.PreLocation;
            }

            _context.SaveChanges();

            //var cartonDetailInDb = _context.RegularCartonDetails.Find(obj.Id);

            //cartonDetailInDb.Comment = obj.Comment;
            //cartonDetailInDb.Adjustor = _userName;

            //_context.SaveChanges();
        }
    }

    public class PreInfo
    {
        public int Id { get; set; }

        public string PreLocation { get; set; }

        public string Comment { get; set; }
    }
}
