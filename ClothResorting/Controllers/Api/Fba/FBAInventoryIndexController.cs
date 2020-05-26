using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using ClothResorting.Helpers.FBAHelper;
using ClothResorting.Helpers;
using ClothResorting.Models.FBAModels.StaticModels;

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBAInventoryIndexController : ApiController
    {
        private ApplicationDbContext _context;
        private delegate string GenerateFBAInventoryReportHandler(FBAInventoryInfo customerInventoryList);
        private delegate void DownloadGeneralFileFromServer(string downloadSourchPath, string prefix, string suffix);

        public FBAInventoryIndexController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/fba/fbainventoryindex/?customerCode={customerCode}&closeDate={closeDate} 下载FBA库存报告
        [HttpGet]
        public IHttpActionResult DownloadInventoryReport([FromUri]string customerCode, [FromUri]DateTime startDate, [FromUri]DateTime closeDate, [FromUri]string operation)
        {
            var templatePath = @"D:\Template\FBA-Inventory-Template.xls";

            var helper = new FBAInventoryHelper(templatePath);

            var customerInventoryList = helper.GetFBAInventoryResidualInfo(customerCode, startDate, closeDate);

            if (operation == FBAOperation.Download)
            {
                helper.GenerateFBAInventoryReport(customerInventoryList);

                return Ok();
            }
            else if (operation == "DownloadFile")
            {
                return Ok(helper.GenerateAndReturnFBAInventoryReportPath(customerInventoryList));
            }

            return Ok(customerInventoryList.FBACtnInventories);

            //阻塞线程，等待生成EXCEL方法执行完毕返回路径字符串后才执行下面的步骤

            //var handler = new GenerateFBAInventoryReportHandler(helper.GenerateFBAInventoryReport);

            //var downloadSourcePath = handler.Invoke(customerInventoryList);

            //var downloadHandler = new DownloadGeneralFileFromServer(downloader.DownloadGeneralFileFromServer);

            //downloadHandler.Invoke(downloadSourcePath, customerCode + " Inventory Report", ".xls");
        }

        // GET /api/fba/fbainventoryindex/?customerCode={customerCode}&startDate={startDate}&closeDate={closeDate}
        [HttpGet]
        public IHttpActionResult GetRemainCustomerList([FromUri]string customerCode, [FromUri]DateTime startDate, [FromUri]DateTime closeDate)
        {
            var helper = new FBAInventoryHelper();
            if (customerCode == "ALL")
            {
                var customerInventoryList = helper.ReturnNonZeroCBMInventoryInfo(startDate, closeDate);

                return Ok(customerInventoryList);
            }
            else
            {
                var list = new List<FBAInventoryInfo>();
                //var result = helper.ReturnInventoryInfoByCustomerCode(customerCode, closeDate);
                var result = helper.GetFBAInventoryResidualInfo(customerCode, startDate, closeDate);
                list.Add(result);

                return Ok(list);
            }
        }
    }
}
