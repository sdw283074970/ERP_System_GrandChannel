using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using AutoMapper;
using ClothResorting.Models.FBAModels;
using ClothResorting.Dtos.Fba;
using ClothResorting.Helpers;
using ClothResorting.Helpers.FBAHelper;
using ClothResorting.Models.FBAModels.BaseClass;

namespace ClothResorting.Controllers.Api.Fba
{
    public class MasterDetailController : ApiController
    {
        private ApplicationDbContext _context;

        public MasterDetailController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/fba/masterdetail/?grandNumber={grandNumber}
        [HttpGet]
        public IHttpActionResult GetMasterDetails([FromUri]string grandNumber)
        {
            return Ok(_context.FBAOrderDetails
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.GrandNumber == grandNumber)
                .Select(Mapper.Map<FBAOrderDetail, FBAOrderDetailDto>));
        }

        // GET /api/fba/masterdetail/?orderDetailId={orderDetailId}
        [HttpGet]
        public IHttpActionResult GetOrderDetail([FromUri]int orderDetailId)
        {
            return Ok(Mapper.Map<FBAOrderDetail, FBAOrderDetailDto>(_context.FBAOrderDetails.Find(orderDetailId)));
        }

        // POST /api/fbva/masterdetail/?grandNumber={grandNumber}
        [HttpPost]
        public void UploadFBATemplate([FromUri]string grandNumber)
        {
            //从httpRequest中获取文件并写入磁盘系统
            var filesGetter = new FilesGetter();

            var fileSavePath = filesGetter.GetAndSaveFileFromHttpRequest(@"D:\TempFiles\");

            if (fileSavePath == "")
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            var excel = new FBAExcelExtracter(fileSavePath);

            excel.ExtractFBAPackingListTemplate(grandNumber);

            //强行关闭进程
            var killer = new ExcelKiller();

            killer.Dispose();
        }

        // PUT /api/fba/masterdetail/?orderDetailId={orderDetailId}
        [HttpPut]
        public void UpdateInfo([FromUri]int orderDetailId, [FromBody]BaseFBAOrderDetail obj)
        {
            var orderDetailInDb = _context.FBAOrderDetails.Find(orderDetailId);
            
            //如果该detail被分配，则禁止更改实收数据
            if (orderDetailInDb.ComsumedQuantity != 0)
            {
                throw new Exception("Cannot update info because this item has been comsumed.");
            }

            orderDetailInDb.ActualQuantity = obj.ActualQuantity;
            
            orderDetailInDb.ActualGrossWeight = obj.ActualGrossWeight;
            
            orderDetailInDb.ActualCBM = obj.ActualCBM;
            
            orderDetailInDb.Comment = obj.Comment;

            _context.SaveChanges();
        }

        // PUT /appi/fba/masterDetail/?grandNumber={grandNumber}
        [HttpPut]
        public void UpdateReceiving([FromUri]string grandNumber)
        {
            var orderDetailsInDb = _context.FBAOrderDetails
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.FBAMasterOrder.GrandNumber == grandNumber);

            foreach(var detail in orderDetailsInDb)
            {
                if (detail.ActualQuantity == 0)
                {
                    detail.ActualCBM = detail.CBM;
                    detail.ActualGrossWeight = detail.GrossWeight;
                    detail.ActualQuantity = detail.Quantity;
                }
            }

            _context.SaveChanges();
        }
    }
}
