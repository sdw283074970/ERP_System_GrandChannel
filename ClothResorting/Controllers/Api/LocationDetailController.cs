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
    public class LocationDetailController : ApiController
    {
        private ApplicationDbContext _context;

        public LocationDetailController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/regularlocationdetail/?preid={id}&po={po}
        [HttpGet]
        public IHttpActionResult GetRegularLocationDetail([FromUri]PreIdPoJsonObj obj)
        {
            var result = new List<LocationDetail>();

            var query = _context.LocationDetails
                .Include(c => c.PurchaseOrderSummary.PreReceiveOrder)
                .Where(c => c.PurchaseOrder == obj.Po && c.PurchaseOrderSummary.PreReceiveOrder.Id == obj.PreId)
                .ToList();

            result.AddRange(query);

            var resultDto = Mapper.Map<List<LocationDetail>, List<LocationDetailDto>>(result);

            return Ok(resultDto);
        }

        // POST /api/regularlocationdetail/?preid={id}&po={po}
        [HttpPost]
        public IHttpActionResult CreateRegularLocationDetails([FromUri]PreIdPoJsonObj obj)
        {
            var fileSavePath = "";

            //从httpRequest中获取文件并写入磁盘系统
            var filesGetter = new FilesGetter();

            fileSavePath = filesGetter.GetAndSaveFileFromHttpRequest(@"D:\TempFiles\");

            if(fileSavePath == "")
            {
                return BadRequest();
            }

            //从上传的文件中抽取LocationDetails
            var excel = new ExcelExtracter(fileSavePath);

            excel.ExtractReplenishimentLocationDetail(obj.PreId, obj.Po);

            //EF无法准确通过datetime查询对象，只能通过按inbound时间分组获取对象
            var group = _context.LocationDetails
                .GroupBy(c => c.InboundDate).ToList();

            var groupCount = group.Count;

            var count = group[groupCount - 1].Count();

            var result = _context.LocationDetails
                .OrderByDescending(c => c.Id)
                .Take(count)
                .OrderBy(c => c.Id)
                .ToList();

            var resultDto = Mapper.Map<List<LocationDetail>, List<LocationDetailDto>>(result);

            //将该po的available箱数件数减去入库后的箱数件数，并更新该po的入库件数
            var purchaseOrderSummary = _context.PurchaseOrderSummaries
                .Include(c => c.PreReceiveOrder)
                .SingleOrDefault(c => c.PurchaseOrder == obj.Po && c.PreReceiveOrder.Id == obj.PreId);

            var sumOfCartons = result.Sum(c => c.OrgNumberOfCartons);
            var sumOfPcs = result.Sum(c => c.OrgPcs);

            purchaseOrderSummary.InventoryCtn += sumOfCartons;
            purchaseOrderSummary.Available -= sumOfCartons;
            purchaseOrderSummary.PreReceiveOrder.Available -= sumOfCartons;

            purchaseOrderSummary.InventoryPcs += sumOfPcs;
            purchaseOrderSummary.AvailablePcs -= sumOfPcs;
            purchaseOrderSummary.PreReceiveOrder.AvailablePcs -= sumOfPcs;
            purchaseOrderSummary.PreReceiveOrder.InvPcs += sumOfPcs;

            _context.SaveChanges();

            return Created(Request.RequestUri + "/" + 333, resultDto);
        }
    }
}
