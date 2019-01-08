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

        // GET /api/fba/masterdetail/{id}
        [HttpGet]
        public IHttpActionResult GetMasterDetails([FromUri]int id)
        {
            return Ok(_context.FBAOrderDetails
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.FBAMasterOrder.Id == id)
                .Select(Mapper.Map<FBAOrderDetail, FBAOrderDetailDto>));
        }

        // POST /api/fbva/masterdetail/?masterOrderId={masterOrderId}
        [HttpPost]
        public void UploadFBATemplate([FromUri]int masterOrderId)
        {
            //从httpRequest中获取文件并写入磁盘系统
            var filesGetter = new FilesGetter();

            var fileSavePath = filesGetter.GetAndSaveFileFromHttpRequest(@"D:\TempFiles\");

            if (fileSavePath == "")
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            var excel = new FBAExcelExtracter(fileSavePath);

            excel.ExtractFBAPackingListTemplate(masterOrderId);

            //强行关闭进程
            var killer = new ExcelKiller();

            killer.Dispose();
        }

        // PUT /api/fba/masterdetail/?orderDetailId={orderDetailId}
        [HttpPut]
        public void UpdateInfo([FromUri]int orderDetailId, [FromBody]BaseFBAOrderDetail obj)
        {
            var orderDetailInDb = _context.FBAOrderDetails.Find(orderDetailId);

            orderDetailInDb.ActualQuantity = obj.ActualQuantity;
            orderDetailInDb.ActualGrossWeight = obj.ActualGrossWeight;
            orderDetailInDb.ActualCBM = obj.ActualCBM;
            orderDetailInDb.Comment = obj.Comment;

            _context.SaveChanges();
        }

        // PUT /appi/fba/masterDetail/?masterOrderId={masterOrderId}
        [HttpPut]
        public void UpdateReceiving([FromUri]int masterOrderId)
        {
            var orderDetailsInDb = _context.FBAOrderDetails
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.FBAMasterOrder.Id == masterOrderId);

            foreach(var detail in orderDetailsInDb)
            {
                detail.ActualCBM = detail.CBM;
                detail.ActualGrossWeight = detail.GrossWeight;
                detail.ActualQuantity = detail.Quantity;
            }

            _context.SaveChanges();
        }
    }
}
