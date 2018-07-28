using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Models;
using ClothResorting.Models.ApiTransformModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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

        // POST /api/pullsheet/{string}(pick tickets range)
        [HttpPost]
        public IHttpActionResult CreateNewPullSheet([FromBody]PickTiketsRangeJsonObj obj)
        {
            _context.PullSheets.Add(new PullSheet {
                PickTicketsRange = obj.Range,
                CreateDate = DateTime.Now.ToString("mm/dd/yyyy"),
                Status = "New Create"
            });

            _context.SaveChanges();

            var result = _context.PullSheets.OrderByDescending(x => x.Id).First();
            var resultDto = Mapper.Map<PullSheet, PullSheetDto>(result);

            return Created(Request.RequestUri + "/" + result.Id, resultDto);
        }
    }
}
