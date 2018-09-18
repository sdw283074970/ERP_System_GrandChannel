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
using ClothResorting.Helpers;

namespace ClothResorting.Controllers.Api
{
    public class PickDetailController : ApiController
    {
        private ApplicationDbContext _context;

        public PickDetailController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/pickdetail/{id}(pullsheetid)
        [HttpGet]
        public IHttpActionResult GetAllPickDetail([FromUri]int id)
        {
            return Ok(_context.PickDetails
                .Include(x => x.PullSheet)
                .Where(x => x.PullSheet.Id == id)
                .Select(Mapper.Map<PickDetail, PickDetailDto>));
        }

        // GET /api/pickdetail/
        [HttpGet]
        public IHttpActionResult DownloadPullSheetTemplate()
        {
            var downloader = new Downloader();
            downloader.DownloadFromServer("PullSheet-Template.xlsx", @"D:\Template\");
            return Ok();
        }

        // POST /api/pickdetail/{id}(pullsheetid)
        [HttpPost]
        public void ExtractPullSheetExcel([FromUri]int id)
        {
            var fileSavePath = "";

            //方法1：写入磁盘系统
            //从httpRequest中获取文件并写入磁盘系统
            var filesGetter = new FilesGetter();

            fileSavePath = filesGetter.GetAndSaveFileFromHttpRequest(@"D:\TempFiles\");

            if (fileSavePath == "")
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            var excel = new ExcelExtracter(fileSavePath);

            excel.ExtractPullSheet(id);
        }

        // PUT /api/pickdetail/?pullSheetId={pullSheetId}&pickingMan={pickingMan}&pickDate={pickDate}
        [HttpPut]
        public void UpdatePickingInfo([FromUri]int pullSheetId, [FromUri]string pickingMan, [FromUri]string pickDate)
        {
            var pullSheetInDb = _context.PullSheets.Find(pullSheetId);

            pullSheetInDb.PickingMan = pickingMan;
            pullSheetInDb.PickDate = pickDate;

            _context.SaveChanges();
        }


    }
}
