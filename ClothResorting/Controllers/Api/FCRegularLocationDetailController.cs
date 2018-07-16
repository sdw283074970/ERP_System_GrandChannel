using ClothResorting.Helpers;
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
    public class FCRegularLocationDetailController : ApiController
    {
        private ApplicationDbContext _context;

        public FCRegularLocationDetailController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/regularlocationdetail/?{id}
        public IHttpActionResult GetRegularLocationDetail([FromUri]int id)
        {
            var result = new List<FCRegularLocation>();

            var query = _context.FCRegularLocations
                .Include(c => c.PreReceiveOrder)
                .Where(c => c.PreReceiveOrder.Id == id)
                .ToList();

            result.AddRange(query);

            var resultDto = Mapper.Map<List<FCRegularLocation>, List<FCRegularLocationDto>>(result);

            return Ok(resultDto);
        }

        // POST /api/fcregularlocation/
        [HttpPost]
        public void CreatedFCRegularLocation([FromUri]int preId)
        {
            var fileSavePath = "";

            //从httpRequest中获取文件并写入磁盘系统
            var filesGetter = new FilesGetter();

            fileSavePath = filesGetter.GetAndSaveFileFromHttpRequest(@"D:\TempFiles\");

            if (fileSavePath == "")
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            var excel = new ExcelExtracter(fileSavePath);

            excel.ExtractFCRegularLocation(preId);
        }
    }
}
