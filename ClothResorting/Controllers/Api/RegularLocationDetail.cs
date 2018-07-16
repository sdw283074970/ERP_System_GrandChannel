using ClothResorting.Dtos;
using ClothResorting.Models;
using ClothResorting.Models.ApiTransformModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using AutoMapper;
using System.Web;
using ClothResorting.Helpers;


namespace ClothResorting.Controllers.Api
{
    public class RegularLocationDetailController : ApiController
    {
        private ApplicationDbContext _context;

        public RegularLocationDetailController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/regularlocationdetail/?preid={id}
        [HttpGet]
        public IHttpActionResult GetRegularLocationDetail([FromUri]string preid)
        {
            var result = new List<FCRegularLocation>();

            var query = _context.FCRegularLocations
                .Include(c => c.PreReceiveOrder)
                .Where(c => c.PurchaseOrder == preid)
                .ToList();

            result.AddRange(query);

            var resultDto = Mapper.Map<List<FCRegularLocation>, List<FCRegularLocationDto>>(result);

            return Ok(resultDto);
        }

        // POST /api/locationdetail/?po={po}
        [HttpPost]
        public IHttpActionResult CreateLocationDetails([FromUri]int preid)
        {
            var fileSavePath = "";

            //从httpRequest中获取文件并写入磁盘系统
            var filesGetter = new FilesGetter();

            fileSavePath = filesGetter.GetAndSaveFileFromHttpRequest(@"D:\TempFiles\");

            if (fileSavePath == "")
            {
                return BadRequest();
            }

            //从上传的文件中抽取LocationDetails
            var excel = new ExcelExtracter(fileSavePath);

            excel.ExtractRegularLocationDetail("test");

            //EF无法准确通过datetime查询对象，只能通过按inbound时间分组获取对象
            var group = _context.RegularLocationDetails
                .GroupBy(c => c.InboundDate).ToList();

            var groupCount = group.Count;

            var count = group[groupCount - 1].Count();

            var result = _context.RegularLocationDetails
                .OrderByDescending(c => c.Id)
                .Take(count)
                .OrderBy(c => c.Id)
                .ToList();

            var resultDto = Mapper.Map<List<RegularLocationDetail>, List<RegularLocationDetailDto>>(result);

            //将该po的available箱数件数减去入库后的箱数件数，并更新该po的入库件数
            var purchaseOrderSummary = _context.PurchaseOrderSummaries
                .SingleOrDefault(c => c.PurchaseOrder == "test");

            var sumOfCartons = result.Sum(c => c.OrgNumberOfCartons);
            var sumOfPcs = result.Sum(c => c.OrgPcs);
            
            purchaseOrderSummary.Available -= sumOfCartons;

            purchaseOrderSummary.AvailablePcs -= sumOfPcs;

            _context.SaveChanges();

            return Created(Request.RequestUri + "/" + 333, resultDto);
        }
    }
}