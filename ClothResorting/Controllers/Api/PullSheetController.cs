using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Models;
using ClothResorting.Models.ApiTransformModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Data.Entity;
using System.Web.Http;

namespace ClothResorting.Controllers.Api
{
    public class PullSheetController : ApiController
    {
        private ApplicationDbContext _context;

        public PullSheetController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/pullsheet/
        public IHttpActionResult GetAllPullSheet()
        {
            var resultDto = _context.PullSheets.OrderByDescending(x => x.Id)
                .Where(x => x.Id > 0).Select(Mapper.Map<PullSheet, PullSheetDto>);

            return Ok(resultDto);
        }

        // POST /api/pullsheet/
        [HttpPost]
        public IHttpActionResult CreateNewPullSheet([FromBody]PickTiketsRangeJsonObj obj)
        {
            _context.PullSheets.Add(new PullSheet {
                PickTicketsRange = obj.Range,
                CreateDate = DateTime.Now.ToString("MM/dd/yyyy"),
                Status = "New Create"
            });

            _context.SaveChanges();

            var result = _context.PullSheets.OrderByDescending(x => x.Id).First();
            var resultDto = Mapper.Map<PullSheet, PullSheetDto>(result);

            return Created(Request.RequestUri + "/" + result.Id, resultDto);
        }

        // PUT /api/pullsheet/{id}(pullSheetId)
        [HttpPut]
        public void ShipPullSheet([FromUri]int id)
        {
            var pickDetailsInDb = _context.PickDetails
                .Include(x => x.PullSheet)
                .Where(x => x.PullSheet.Id == id
                    && x.Status == "Picking");

            var pullSheetInDb = _context.PullSheets.Find(id);
            
            var locationDeatilsInDb = _context.FCRegularLocationDetails
                .Where(x => x.Id > 0)
                .ToList();

            //此处应简化数据库查询
            foreach(var pickDetail in pickDetailsInDb)
            {
                var locationDetail = locationDeatilsInDb.SingleOrDefault(x => x.Id == pickDetail.LocationDetailId);

                locationDetail.ShippedCtns += pickDetail.PickCtns;
                locationDetail.ShippedPcs += pickDetail.PickPcs;

                locationDetail.PickingCtns -= pickDetail.PickCtns;
                locationDetail.PickingPcs -= pickDetail.PickPcs;

                pickDetail.Status = "Shipped";

                if (locationDetail.AvailableCtns == 0 && locationDetail.PickingCtns == 0)
                {
                    locationDetail.Status = "Shipped";
                }
            }

            pullSheetInDb.Status = "Shipped";

            _context.SaveChanges();
        }

        // DELETE /api/pullsheet/{id}(pullSheetId)
        [HttpDelete]
        public void CancelPullSheet([FromUri]int id)
        {
            var pickDetailsInDb = _context.PickDetails
                .Include(x => x.PullSheet)
                .Where(x => x.PullSheet.Id == id
                    && x.Status == "Picking");

            var locationDeatilsInDb = _context.FCRegularLocationDetails
                .Where(x => x.Id > 0)
                .ToList();

            foreach (var pickDetail in pickDetailsInDb)
            {
                var locationDetail = locationDeatilsInDb.SingleOrDefault(x => x.Id == pickDetail.LocationDetailId);

                locationDetail.AvailableCtns += pickDetail.PickCtns;
                locationDetail.AvailablePcs += pickDetail.PickPcs;

                locationDetail.PickingCtns -= pickDetail.PickCtns;
                locationDetail.PickingPcs -= pickDetail.PickPcs;

                if (locationDetail.PickingCtns == 0 && locationDetail.AvailableCtns != 0)
                {
                    locationDetail.Status = "In Stock";
                }
            }

            var pullSheetInDb = _context.PullSheets.Find(id);

            _context.PickDetails.RemoveRange(pickDetailsInDb);
            _context.SaveChanges();

            _context.PullSheets.Remove(pullSheetInDb);
            _context.SaveChanges();
        }
    }
}
