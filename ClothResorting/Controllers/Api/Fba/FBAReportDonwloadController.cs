using ClothResorting.Models;
using ClothResorting.Models.StaticClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using ClothResorting.Helpers.FBAHelper;
using ClothResorting.Helpers;

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBAReportDonwloadController : ApiController
    {
        private ApplicationDbContext _context;

        public FBAReportDonwloadController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/fba/FBAReportdownload/?fullPath={fullPath}&prefix={prefix}&suffix={suffix}
        [HttpGet]
        public void DownloadChargingReport([FromUri]string fullPath, [FromUri]string prefix, [FromUri]string suffix)
        {
            var downloader = new Downloader();

            downloader.DownloadGeneralFileFromServer(fullPath, prefix, suffix);
        }

        // GET /api/fba/fbaReportdownload/?reference={reference}&invoiceType={invoiceType}
        [HttpGet]
        public IHttpActionResult DownloadChargingReportFormOrder([FromUri]string reference, [FromUri]string invoiceType)
        {
            var templatePath = @"E:\Template\FBA-InvoiceReport-Template.xls";

            var excelGenerator = new FBAInvoiceHelper(templatePath);

            var info = excelGenerator.GetChargingReportFormOrder(reference, invoiceType);

            var path = excelGenerator.GenerateExcelFileAndReturnPath(info);

            return Ok(path);
        }

    }
}
