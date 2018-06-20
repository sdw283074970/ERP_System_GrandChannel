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

        // GET /api/locationdetail/?preid={id}&po={po}
        [HttpGet]
        public IHttpActionResult GetLocationDetail([FromUri]PreIdPoJsonObj obj)
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

        // POST /api/locationdetail
        [HttpPost]
        public IHttpActionResult GreateLocationDetails([FromUri]PreIdPoJsonObj obj)
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

            excel.ExtractLocationDetail(obj.PreId, obj.Po);

            var time = _context.LocationDetails.OrderByDescending(c => c.Id).First().InboundDate;

            var result = _context.LocationDetails.Where(c => c.InboundDate == time).ToList();

            var resultDto = Mapper.Map<List<LocationDetail>, List<LocationDetailDto>>(result);

            //将该po的available箱数件数减去入库后的箱数件数，并更新该po的入库件数
            var purchaseOrderSummary = _context.PurchaseOrderSummarys
                .SingleOrDefault(c => c.PurchaseOrder == obj.Po && c.PreReceiveOrder.Id == obj.PreId);

            var sumOfCartons = result.Sum(c => c.NumberOfCartons);
            var sumOfPcs = result.Sum(c => c.Pcs);

            purchaseOrderSummary.InventoryCtn += sumOfCartons;
            purchaseOrderSummary.Available -= sumOfCartons;

            purchaseOrderSummary.InventoryPcs += sumOfPcs;
            purchaseOrderSummary.AvailablePcs -= sumOfPcs;

            _context.SaveChanges();

            return Created(Request.RequestUri + "/" + 333, resultDto);
        }
    }
}
