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
    public class GeneralLocManagementController : ApiController
    {
        private ApplicationDbContext _context;

        public GeneralLocManagementController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/generallocmanagement/
        [HttpGet]
        public IHttpActionResult GetAllGeneralLocationSummay()
        {
            return Ok(_context.GeneralLocationSummaries
                .Where(x => x.Id > 0)
                .OrderByDescending(x => x.Id)
                .Select(Mapper.Map<GeneralLocationSummary, GeneralLocationSummaryDto>));
        }

        // POST /api/generallocmanagement/?inboundDate={inboundDate}
        [HttpPost]
        public void CreateNewGeneralLocationSummaryAndDetail([FromUri]string inboundDate)
        {
            var fileSavePath = "";
            var filesGetter = new FilesGetter();

            fileSavePath = filesGetter.GetAndSaveFileFromHttpRequest(@"D:\TempFiles\");

            if (fileSavePath == "")
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            var excel = new ExcelExtracter(fileSavePath);

            excel.UploadReplenishimentLocationDetail(inboundDate, fileSavePath.Split('\\').Last().Split('.').First());
        }
    }
}
