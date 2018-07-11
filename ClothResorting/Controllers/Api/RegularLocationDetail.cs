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

        // GET /api/regularlocationdetail/?preid={id}&po={po}
        [HttpGet]
        public IHttpActionResult GetRegularLocationDetail([FromUri]string po)
        {
            var result = new List<RegularLocationDetail>();

            var query = _context.RegularLocationDetails
                .Include(c => c.PurchaseOrderInventory)
                .Where(c => c.PurchaseOrder == po)
                .ToList();

            result.AddRange(query);

            var resultDto = Mapper.Map<List<RegularLocationDetail>, List<RegularLocationDetailDto>>(result);

            return Ok(resultDto);
        }

        // POST /api/locationdetail/?po={po}
        [HttpPost]
        public IHttpActionResult CreateLocationDetails([FromUri]string po)
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

            excel.ExtractRegularLocationDetail(po);

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
                .SingleOrDefault(c => c.PurchaseOrder == po);

            var sumOfCartons = result.Sum(c => c.OrgNumberOfCartons);
            var sumOfPcs = result.Sum(c => c.OrgPcs);
            
            purchaseOrderSummary.Available -= sumOfCartons;

            purchaseOrderSummary.AvailablePcs -= sumOfPcs;

            _context.SaveChanges();

            return Created(Request.RequestUri + "/" + 333, resultDto);
        }
    }
}