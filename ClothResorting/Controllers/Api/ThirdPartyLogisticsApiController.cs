using ClothResorting.Helpers;
using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Data.Entity;

namespace ClothResorting.Controllers.Api
{
    public class ThirdPartyLogisticsApiController : ApiController
    {
        private ApplicationDbContext _context;

        public ThirdPartyLogisticsApiController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/thirdpartylogisticsapi
        [HttpPost]
        public void SaveUploadedFileAndExtractExcel()
        {
            var fileSavePath = "";

            //方法1：写入磁盘系统
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                var httpPostedFile = HttpContext.Current.Request.Files[0];

                if (httpPostedFile != null)
                {
                    fileSavePath = @"D:\TempFiles\" + httpPostedFile.FileName;

                    httpPostedFile.SaveAs(fileSavePath);
                }
            }

            ////方法2：不写入磁盘系统
            //if(Request.Content.IsMimeMultipartContent())
            //{
            //    var streamProvider = new MultipartFormDataStreamProvider(@"~/UploadedFiles");
            //}

            //目前暂时无法处理同名文件，有空回来改

            var excel = new ExcelExtracter(fileSavePath);

            excel.CreatePreReceiveOrderAndOverView();

            excel.ExtractPackingList();

            excel.ExtractCartonDetails();

            //再次强行释放EXCEL资源(终止EXCEL进程)
            excel.Dispose();

            //因为源文件没有每一单po的总pcs数量，所以需要写算法计算
            //算法如下
            var preReceiveOrder = _context.PreReceiveOrders
                .Include(s => s.SilkIconPackingLists)
                .OrderByDescending(s => s.Id)
                .First();

            //根据每一个packinglist中的cartondetail中的每一类Pcs数量计算每一单po应收取的Pcs总数
            foreach(var pl in preReceiveOrder.SilkIconPackingLists)
            {
                pl.TotalPcs = _context.CartonDetails
                    .Include(c => c.PackingList.PreReceiveOrder)
                    .Where(c => c.PackingList.PurchaseOrder == pl.PurchaseOrder
                        && c.PackingList.PreReceiveOrder.Id == preReceiveOrder.Id)
                    .Sum(c => c.TotalPcs);
            }

            _context.SaveChanges();

            //根据每一个packinglist中po的Pcs数量计算整个pre-receive order应收取的pcs总数

            preReceiveOrder.TotalPcs = _context.PackingLists
                .Include(s => s.PreReceiveOrder)
                .Where(s => s.PreReceiveOrder.Id == preReceiveOrder.Id)
                .Sum(s => s.TotalPcs);

            _context.SaveChanges();

            //在CartonDetail中消除在【同一箱】的不同货物造成的重复计箱问题
            var checker = new InOneCartonChecker();
            checker.ReplaceRepeatedEntry();

            _context.SaveChanges();
        }
    }
}
