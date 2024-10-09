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

        // GET /api/fba/chargetemplate/?customerId={customerId}
        [HttpGet]
        public IHttpActionResult GetTemplatesByCustomerId([FromUri]int customerId)
        {
            var customerCode = _context2.UpperVendors.Find(customerId).CustomerCode;
            var templatesDto = _context.ChargeTemplates
                .Where(x => x.Customer == customerCode)
                .Select(Mapper.Map<ChargeTemplate, ChargeTemplateDto>);

            return Ok(templatesDto);
        }

        // GET /api/fba/chargetemplate/?storageTempId={storageTempId}
        [HttpGet]
        public IHttpActionResult GetTemplateByTemplateId([FromUri]int storageTempId)
        {
            return Ok(Mapper.Map<ChargeTemplate, ChargeTemplateDto>(_context.ChargeTemplates.SingleOrDefault(x => x.Id == storageTempId)));
        }

        // GET /api/fba/chargetemplate/?templateId={templateId}&customerCode={customerCode}lastBillingDate={lastBillingDate}&currentBillingDate={currentBillingDate}&p1Discount={p1Discount}&p2Discount={p2Discount}&isEstimatingCharge={foo}
        [HttpGet]
        public IHttpActionResult GenerateNewStorageReport([FromUri]int templateId, [FromUri]string customerCode, [FromUri]DateTime lastBillingDate, [FromUri]DateTime currentBillingDate, [FromUri]float p1Discount, [FromUri]float p2Discount, [FromUri]bool isEstimatingCharge)
        {
            var closeDate = new DateTime(currentBillingDate.Year, currentBillingDate.Month, currentBillingDate.Day).AddDays(1);
            var startDate = new DateTime(lastBillingDate.Year, currentBillingDate.Month, currentBillingDate.Day);

            var warehouseLocationsInDb = _context2.WarehouseLocations.Select(x => x.WarehouseCode).ToArray();

            var customerId = _context2.UpperVendors.SingleOrDefault(x => x.CustomerCode == customerCode).Id;
            var generator = new FBAExcelGenerator(@"E:\Template\StorageFee-Template.xlsx");
            var fullPath = generator.GenerateStorageReport(customerId, lastBillingDate, closeDate, p1Discount, p2Discount, warehouseLocationsInDb, false);

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

            return Ok(fullPath);
        }

        // GET /api/fba/chargetemplate/?customerCode={customerCode}
        [HttpGet]
        public IHttpActionResult GetTemplatesByCustomerCode([FromUri]string customerCode)
        {
            var templatesDto = _context.ChargeTemplates
                .Where(x => x.Customer == customerCode)
                .ToList();

            var methodsDto = _context.ChargeMethods
                .Include(x => x.ChargeTemplate)
                .Where(x => x.ChargeTemplate.Customer == customerCode)
                .Select(Mapper.Map<ChargeMethod, ChargeMethodDto>);

            var results = new List<ChargeTemplatesWithDetails>();

            foreach (var t in templatesDto)
            {
                var details = _context.ChargeMethods
                    .Include(x => x.ChargeTemplate)
                    .Where(x => x.ChargeTemplate.Id == t.Id)
                    .Select(Mapper.Map<ChargeMethod, ChargeMethodDto>);

                results.Add(new ChargeTemplatesWithDetails
                {
                    Id = t.Id,
                    TemplateName = t.TemplateName,
                    Customer = t.Customer,
                    TimeUnit = t.TimeUnit,
                    Currency = t.Currency,
                    ChargeMethods = details
                });
            }

            return Ok(results);
        }

        // POST /api/fba/chargetemplate/?templateId={templateId}&customerCode={customerCode}lastBillingDate={lastBillingDate}&currentBillingDate={currentBillingDate}&p1Discount={p1Discount}&p2Discount={p2Discount}
        [HttpPost]
        public void DownloadNewStorageReport([FromUri]int templateId, [FromUri]string customerCode, [FromUri]DateTime lastBillingDate, [FromUri]DateTime currentBillingDate, [FromUri]float p1Discount, [FromUri]float p2Discount)
        {
            var closeDate = new DateTime(currentBillingDate.Year, currentBillingDate.Month, currentBillingDate.Day).AddDays(1);
            var startDate = new DateTime(lastBillingDate.Year, currentBillingDate.Month, currentBillingDate.Day);

            var warehouseLocationsInDb = _context2.WarehouseLocations.Select(x => x.WarehouseCode).ToArray();

            var customerId = _context2.UpperVendors.SingleOrDefault(x => x.CustomerCode == customerCode).Id;
            var generator = new FBAExcelGenerator(@"E:\Template\StorageFee-Template.xlsx");
            var fullPath = generator.GenerateStorageReport(customerId, startDate, closeDate, p1Discount, p2Discount, warehouseLocationsInDb, false);

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

        // POST /api/fba/chargetemplate/
        [HttpPost]
        public IHttpActionResult GenerateNewStorageReportThroughPayload([FromBody]StorageChargePayload data)
        {
            var closeDate = new DateTime(data.CurrentBillingDate.Year, data.CurrentBillingDate.Month, data.CurrentBillingDate.Day).AddDays(1);
            var startDate = new DateTime(data.LastBillingDate.Year, data.CurrentBillingDate.Month, data.CurrentBillingDate.Day);
            
            var warehouseLocationsInDb = _context2.WarehouseLocations.Select(x => x.WarehouseCode).ToArray();
            var warehouseLocations = data.WarehouseLocation.Length > 0 ? data.WarehouseLocation : warehouseLocationsInDb;

            var customerId = _context2.UpperVendors.SingleOrDefault(x => x.CustomerCode == data.CustomerCode).Id;
            var generator = new FBAExcelGenerator(@"E:\Template\StorageFee-Template.xlsx");
            var fullPath = generator.GenerateStorageReport(customerId, data.LastBillingDate, closeDate, data.P1Discount, data.P2Discount, warehouseLocations, data.IncludePrereleasedOrder);

            var chargeMethodsList = _context.ChargeMethods
                .Include(x => x.ChargeTemplate)
                .Where(x => x.ChargeTemplate.Id == data.TemplateId)
                .OrderBy(x => x.From)
                .ToList();

            var calculator = new InventoryFeeCalculator(fullPath);

            calculator.RecalculateInventoryFeeInExcel(chargeMethodsList, chargeMethodsList.First().TimeUnit, data.LastBillingDate.ToString("yyyy-MM-dd"), data.CurrentBillingDate.ToString("yyyy-MM-dd"));

            //强行关闭Excel进程
            var killer = new ExcelKiller();
            killer.Dispose();

            return Ok(fullPath);
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
            var path = fileGetter.GetAndSaveSingleFileFromHttpRequest(@"E:\TempFiles\");

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

        // PUT /api/fba/chargetemplate/?storageTempId={storageTempId}&templateName={templateName}&customer={customer}&timeUnit={timeUnit}&currency={currency}
        [HttpPut]
        public void UpdateStorageTemplate([FromUri]int storageTempId, [FromUri]string templateName, [FromUri]string customer, [FromUri]string timeUnit, [FromUri]string currency)
        {
            var tempInDb = _context.ChargeTemplates.Find(storageTempId);
            tempInDb.TemplateName = templateName;
            tempInDb.Customer = customer;
            tempInDb.TimeUnit = timeUnit;
            tempInDb.Currency = currency;

            _context.SaveChanges();
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

    public class ChargeTemplatesWithDetails
    {
        public int Id { get; set; }

        public string TemplateName { get; set; }

        public string Customer { get; set; }

        public string TimeUnit { get; set; }

        public string Currency { get; set; }

        public IEnumerable<ChargeMethodDto> ChargeMethods { get; set; }
    }

    public class StorageChargePayload
    {
        public int TemplateId { get; set; }

        public string CustomerCode { get; set; }

        public DateTime LastBillingDate { get; set; }

        public DateTime CurrentBillingDate { get; set; }

        public float P1Discount { get; set; }

        public float P2Discount { get; set; }

        public bool IncludePrereleasedOrder { get; set; }

        public string[] WarehouseLocation { get; set; }
    }


}
