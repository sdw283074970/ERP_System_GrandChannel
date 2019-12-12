using AutoMapper;
using ClothResorting.Dtos.Fba;
using ClothResorting.Models;
using ClothResorting.Models.FBAModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using ClothResorting.Helpers;
using System.Web;
using System.IO;
using ClothResorting.Models.StaticClass;
using System.Globalization;
using ClothResorting.Helpers.FBAHelper;

namespace ClothResorting.Controllers.Api.Fba
{
    public class ChargeTemplateController : ApiController
    {
        private FBADbContext _context;
        private ApplicationDbContext _context2;

        public ChargeTemplateController()
        {
            _context = new FBADbContext();
            _context2 = new ApplicationDbContext();
        }

        // GET /api/fba/chargetemplate/
        [HttpGet]
        public IHttpActionResult GetAllTemplates()
        {
            var templatesDto = _context.ChargeTemplates
                .Where(x => x.Id > 0)
                .Select(Mapper.Map<ChargeTemplate, ChargeTemplateDto>);

            return Ok(templatesDto);
        }

        // POST /api/fba/chargetemplate/?templateId={templateId}&customerCode={customerCode}lastBillingDate={lastBillingDate}&currentBillingDate={currentBillingDate}&p1Discount={p1Discount}&p2Discount={p2Discount}
        [HttpPost]
        public void DownloadNewStorageReport([FromUri]int templateId, [FromUri]string customerCode, [FromUri]DateTime lastBillingDate, [FromUri]DateTime currentBillingDate, [FromUri]float p1Discount, [FromUri]float p2Discount)
        {
            DateTime closeDate = currentBillingDate;
            DateTime startDate = lastBillingDate;

            var customerId = _context2.UpperVendors.SingleOrDefault(x => x.CustomerCode == customerCode).Id;
            var generator = new FBAExcelGenerator(@"D:\Template\StorageFee-Template.xlsx");
            var fullPath = generator.GenerateStorageReport(customerId, lastBillingDate, closeDate, p1Discount, p2Discount);

            var chargeMethodsList = _context.ChargeMethods
                .Include(x => x.ChargeTemplate)
                .Where(x => x.ChargeTemplate.Id == templateId)
                .OrderBy(x => x.From)
                .ToList();

            var calculator = new InventoryFeeCalculator(fullPath);

            calculator.RecalculateInventoryFeeInExcel(chargeMethodsList, chargeMethodsList.First().TimeUnit, lastBillingDate.ToString("yyyy-MM-dd"), currentBillingDate.ToString("yyyy-MM-dd"));

            //强行关闭Excel进程
            var killer = new ExcelKiller();
            killer.Dispose();

            //在静态变量中记录下载信息
            DownloadRecord.FileName = fullPath.Split('\\').Last();
            DownloadRecord.FilePath = fullPath;
        }

        // POST /api/fba/chargetemplate/?templateName={templateName}&customer={customer}&timeUnit={timeUnit}&currency={currency}
        [HttpPost]
        public IHttpActionResult CreateNewTemplate([FromUri]string templateName, [FromUri]string customer, [FromUri]string timeUnit, [FromUri]string currency)
        {
            var newTemplate = new ChargeTemplate {
                TemplateName = templateName,
                Customer = customer,
                TimeUnit = timeUnit,
                Currency = currency
            };

            _context.ChargeTemplates.Add(newTemplate);
            _context.SaveChanges();

            var sample = _context.ChargeTemplates
                .OrderByDescending(x => x.Id)
                .First();

            return Created(Request.RequestUri + "/" + sample.Id, Mapper.Map<ChargeTemplate, ChargeTemplateDto>(sample));
        }

        // POST /api/fba/chargetemplate/?templateId={templateId}&lastBillingDate={lastBillingDate}&currentBillingDate={currentBillingDate}
        [HttpPost]
        public void UploadAndDownloadFile([FromUri]int templateId, [FromUri]string lastBillingDate, [FromUri]string currentBillingDate)
        {
            var chargeMethodsList = _context.ChargeMethods
                .Include(x => x.ChargeTemplate)
                .Where(x => x.ChargeTemplate.Id == templateId)
                .OrderBy(x => x.From)
                .ToList();

            var fileGetter = new FilesGetter();
            var path = fileGetter.GetAndSaveSingleFileFromHttpRequest(@"D:\TempFiles\");

            if (path == "")
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            var calculator = new InventoryFeeCalculator(path);

            calculator.RecalculateInventoryFeeInExcel(chargeMethodsList, chargeMethodsList.First().TimeUnit, lastBillingDate, currentBillingDate);

            //强行关闭Excel进程
            var killer = new ExcelKiller();
            killer.Dispose();

            //在静态变量中记录下载信息
            DownloadRecord.FileName = fileGetter.FileName;
            DownloadRecord.FilePath = path;
        }

        // DELETE /api/fba/chargetemplate/?templateId={templateId}
        [HttpDelete]
        public void DeleteTemplate([FromUri]int templateId)
        {
            var methodsInDb = _context.ChargeMethods
                .Include(x => x.ChargeTemplate)
                .Where(x => x.ChargeTemplate.Id == templateId);

            _context.ChargeMethods.RemoveRange(methodsInDb);
            _context.ChargeTemplates.Remove(_context.ChargeTemplates.Find(templateId));
            _context.SaveChanges();
        }
    }
}
