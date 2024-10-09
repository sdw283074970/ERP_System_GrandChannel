﻿using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Dtos.Fba;
using ClothResorting.Helpers.FBAHelper;
using ClothResorting.Models;
using ClothResorting.Models.FBAModels;
using ClothResorting.Models.StaticClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using ClothResorting.Models.FBAModels.StaticModels;
using Aspose.Cells;

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBAIndexController : ApiController
    {
        private ApplicationDbContext _context;

        public FBAIndexController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/fba/index/?userId={userId}
        [HttpGet]
        public IHttpActionResult GetActiveCustomersByUser([FromUri]string userId)
        {
            return Ok(_context.UpperVendors
                .Include(x => x.ApplicationUser)
                .Where(x => x.Status == Status.Active && x.DepartmentCode == DepartmentCode.FBA && x.ApplicationUser.Id == userId)
                .Select(Mapper.Map<UpperVendor, UpperVendorDto>));
        }

        //GET /api/fba/index/?customerId={customerId}
        [HttpGet]
        public IHttpActionResult GetCustomerInfoByCustomerId([FromUri]int customerId)
        {
            var dto = Mapper.Map<UpperVendor, UpperVendorDto>(_context.UpperVendors.SingleOrDefault(x => x.Id == customerId));

            return Ok(dto);
        }

        // GET /api/fba/index
        [HttpGet]
        public IHttpActionResult GetAllCustomers()
        {
            var dtos = GetAllCustomerDtos();

            return Ok(dtos.OrderByDescending(x => x.Id));
        }

        [HttpGet]
        public IHttpActionResult GetExcelUrl([FromUri]string operation)
        {
            if (operation == "INSTOCKCUSTOMER")
            {
                var dtos = GetAllCustomerDtos();
                var templatePath = @"E:\Template\Customer-Info-Template.xlsx";
                var path = GenerateExcelFile(templatePath, dtos);
                return Ok(path);
            }

            return Ok("No operation applied.");
        }

        // GET /api/fba/fbaindex/?operation={foo}
        [HttpPost]
        public IHttpActionResult GetExportedInvoiceFilePath([FromUri]string operation, [FromBody]ExpenseQueryData data)
        {
            if (operation != "GetInvoice")
                throw new Exception("Invailed operation code: " + operation);

            var templatePath = @"E:\Template\FBA-InvoiceReport-Template.xls";

            var excelGenerator = new FBAInvoiceHelper(templatePath);

            var startDate = new DateTime(data.StartDate.Year, data.StartDate.Month, data.StartDate.Day);
            var closeDate = new DateTime(data.CloseDate.Year, data.CloseDate.Month, data.CloseDate.Day).AddDays(1);

            var warehouseLocationsInDb = _context.WarehouseLocations.Select(x => x.WarehouseCode).ToArray();
            var warehouseLocations = data.WarehouseLocation.Length > 0 ? data.WarehouseLocation : warehouseLocationsInDb;

            var customerId = 0;

            if (data.CustomerCode != "ALL")
            {
                customerId = _context.UpperVendors.SingleOrDefault(x => x.CustomerCode == data.CustomerCode).Id;
            }

            //如果customerId等于0说明是要所有客户的记录,包括没有关闭的订单
            if (customerId == 0)
            {
                var info = excelGenerator.GetAllFBACustomerChargingReportFromDate(startDate, closeDate, warehouseLocations);

                var path = excelGenerator.GenerateExcelFileForAllCustomerAndReturnPath(info);

                return Ok(path);
            }
            else if (data.IfShowUnclosed)
            {
                var info = excelGenerator.GetAllChargingReportFormDateRangeAndCustomerId(customerId, startDate, closeDate, warehouseLocations);

                var path = excelGenerator.GenerateExcelFileAndReturnPath(info);

                return Ok(path);
            }
            else  // 仅仅生成已关闭的订单
            {
                var info = excelGenerator.GetClosedChargingReportFormDateRangeAndCustomerId(customerId, startDate, closeDate, warehouseLocations);

                var path = excelGenerator.GenerateExcelFileAndReturnPath(info);

                return Ok(path);
            }
        }

        // GET /api/fba/fbaindex/?customerId={customerId}&startDate={startDate}&closeDate={closeDate}
        [HttpGet]
        public IHttpActionResult GetExportedFilePath([FromUri]int customerId, [FromUri]DateTime startDate, [FromUri]DateTime closeDate, [FromUri]bool ifShowUnclosed)
        {
            var templatePath = @"E:\Template\FBA-InvoiceReport-Template.xls";

            var excelGenerator = new FBAInvoiceHelper(templatePath);

            var warehouseLocationsInDb = _context.WarehouseLocations.Select(x => x.WarehouseCode).ToArray();

            //如果customerId等于0说明是要所有客户的记录,包括没有关闭的订单
            if (customerId == 0)
            {
                var info = excelGenerator.GetAllFBACustomerChargingReportFromDate(startDate, closeDate, warehouseLocationsInDb);

                var path = excelGenerator.GenerateExcelFileForAllCustomerAndReturnPath(info);

                return Ok(path);
            }
            else if (ifShowUnclosed)
            {
                var info = excelGenerator.GetAllChargingReportFormDateRangeAndCustomerId(customerId, startDate, closeDate, warehouseLocationsInDb);

                var path = excelGenerator.GenerateExcelFileAndReturnPath(info);

                return Ok(path);
            }
            else  // 仅仅生成已关闭的订单
            {
                var info = excelGenerator.GetClosedChargingReportFormDateRangeAndCustomerId(customerId, startDate, closeDate, warehouseLocationsInDb);

                var path = excelGenerator.GenerateExcelFileAndReturnPath(info);

                return Ok(path);
            }
        }

        // GET /api/fba/fbaindex/?customerCode={customerCode}&startDate={startDate}&closeDate={closeDate}
        [HttpGet]
        public IHttpActionResult GetExportedFilePathByCustomerCode([FromUri]string customerCode, [FromUri]DateTime startDate, [FromUri]DateTime closeDate, [FromUri]bool ifShowUnclosed)
        {
            var templatePath = @"E:\Template\FBA-InvoiceReport-Template.xls";

            var excelGenerator = new FBAInvoiceHelper(templatePath);

            startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day);
            closeDate = new DateTime(closeDate.Year, closeDate.Month, closeDate.Day).AddDays(1);

            var customerId = 0;

            if (customerCode != "ALL")
            {
                customerId = _context.UpperVendors.SingleOrDefault(x => x.CustomerCode == customerCode).Id;
            }

            var warehouseLocationsInDb = _context.WarehouseLocations.Select(x => x.WarehouseCode).ToArray();

            //如果customerId等于0说明是要所有客户的记录
            if (customerId == 0)
            {
                var info = excelGenerator.GetAllFBACustomerChargingReportFromDate(startDate, closeDate, warehouseLocationsInDb);

                var path = excelGenerator.GenerateExcelFileForAllCustomerAndReturnPath(info);

                return Ok(path);
            }
            else if (ifShowUnclosed)
            {
                var info = excelGenerator.GetAllChargingReportFormDateRangeAndCustomerId(customerId, startDate, closeDate, warehouseLocationsInDb);

                var path = excelGenerator.GenerateExcelFileAndReturnPath(info);

                return Ok(path);
            }
            else
            {
                var info = excelGenerator.GetClosedChargingReportFormDateRangeAndCustomerId(customerId, startDate, closeDate, warehouseLocationsInDb);

                var path = excelGenerator.GenerateExcelFileAndReturnPath(info);

                return Ok(path);
            }
        }

        // POST /api/fba/index/?requestId={requestId}
        [HttpPost]
        public IHttpActionResult PushDataFromFrontierSystem([FromUri]string requestId, [FromBody]FBAMasterOrderAPIDto order)
        {
            if (ModelState.IsValid)
                return Ok();
            else
                throw new Exception("Invalid!");
        }

        private IEnumerable<UpperVendorDto> GetAllCustomerDtos()
        {
            var dtos = _context.UpperVendors
                .Include(x => x.ApplicationUser)
                .Where(x => x.DepartmentCode == DepartmentCode.FBA)
                .Select(Mapper.Map<UpperVendor, UpperVendorDto>)
                .ToList();

            //统计在拣货的数量只有 shipped 算作已出库，不计入在拣数量，其他都算processing
            var processingPickDetails = _context.FBAPickDetails
                .Include(x => x.FBAShipOrder)
                .Where(x => x.FBAShipOrder.Status != FBAStatus.Shipped)
                .ToList();

            var customerGroupPickDetails = processingPickDetails.GroupBy(x => x.FBAShipOrder.CustomerCode);

            foreach (var c in customerGroupPickDetails)
            {
                var dto = dtos.SingleOrDefault(x => x.CustomerCode == c.First().FBAShipOrder.CustomerCode);
                dto.ProcessingCtns = c.Sum(x => x.ActualQuantity);
                dto.ProcessingPlts = c.Sum(x => x.PltsFromInventory);
            }

            //统计库存剩余箱数
            var cartonLocations = _context.FBACartonLocations
                .Include(x => x.FBAOrderDetail.FBAMasterOrder.Customer)
                .Where(x => x.AvailableCtns != 0)
                .ToList();

            var cartonLocationGroup = cartonLocations.GroupBy(x => x.FBAOrderDetail.FBAMasterOrder.Customer.CustomerCode);

            foreach (var c in cartonLocationGroup)
            {
                var dto = dtos.SingleOrDefault(x => x.CustomerCode == c.First().FBAOrderDetail.FBAMasterOrder.Customer.CustomerCode);
                dto.InstockCtns = c.Sum(x => x.AvailableCtns);
            }

            //统计库存剩余托盘数
            var palletLocations = _context.FBAPalletLocations
                .Include(x => x.FBAPallet.FBAMasterOrder.Customer)
                .Where(x => x.AvailablePlts != 0)
                .ToList();

            var palletLocationGroup = palletLocations.GroupBy(x => x.FBAPallet.FBAMasterOrder.Customer.CustomerCode);

            foreach (var p in palletLocationGroup)
            {
                var dto = dtos.SingleOrDefault(x => x.CustomerCode == p.First().FBAPallet.FBAMasterOrder.Customer.CustomerCode);
                dto.InstockPlts = p.Sum(x => x.AvailablePlts);
            }

            //统计未付款发票数量以及未付款金额
            var payableMasterOrderInvoice = _context.FBAMasterOrders
                .Include(x => x.Customer)
                .Include(x => x.InvoiceDetails)
                .Where(x => x.InvoiceStatus == "Await" || x.InvoiceStatus == FBAStatus.Generated)
                .ToList();

            var masterOrderInvoiceGroup = payableMasterOrderInvoice.GroupBy(x => x.Customer.CustomerCode);

            foreach (var m in masterOrderInvoiceGroup)
            {
                var dto = dtos.SingleOrDefault(x => x.CustomerCode == m.First().Customer.CustomerCode);
                dto.PayableInvoices = m.Count();
                dto.PayableAmounts = (float)m.Sum(x => x.InvoiceDetails.Sum(c => c.Amount));
            }

            var payableShipOrderInvoice = _context.FBAShipOrders
                .Include(x => x.InvoiceDetails)
                .Where(x => x.InvoiceStatus == "Await" || x.InvoiceStatus == FBAStatus.Generated)
                .ToList();

            var shipOrderInvoiceGroup = payableShipOrderInvoice.GroupBy(x => x.CustomerCode);

            foreach (var s in shipOrderInvoiceGroup)
            {
                var dto = dtos.SingleOrDefault(x => x.CustomerCode == s.First().CustomerCode);

                if (dto == null)
                    continue;

                dto.PayableInvoices += s.Count();
                dto.PayableAmounts += (float)s.Sum(x => x.InvoiceDetails.Sum(c => c.Amount));
            }

            return dtos;
        }

        private string GenerateExcelFile(string templatePath, IEnumerable<UpperVendorDto> dtos)
        {
            var asposeWb = new Workbook(templatePath);
            var asposeWs = asposeWb.Worksheets[0];

            var index = 1;

            foreach (var dto in dtos)
            {
                if (dto.InstockCtns == 0 && dto.InstockPlts == 0 && dto.ProcessingCtns == 0 && dto.ProcessingPlts == 0)
                {
                    continue;
                }

                asposeWs.Cells[index, 0].PutValue(dto.Id);
                asposeWs.Cells[index, 1].PutValue(dto.Name);
                asposeWs.Cells[index, 2].PutValue(dto.CustomerCode);
                asposeWs.Cells[index, 3].PutValue(dto.ProcessingCtns);
                asposeWs.Cells[index, 4].PutValue(dto.ProcessingPlts);
                asposeWs.Cells[index, 5].PutValue(dto.InstockCtns);
                asposeWs.Cells[index, 6].PutValue(dto.InstockPlts);
                asposeWs.Cells[index, 7].PutValue(dto.PayableInvoices);
                asposeWs.Cells[index, 8].PutValue(dto.PayableAmounts);

                index++;
            }

            var xlsxPath = @"E:\TempFiles\Customer-Info-" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx";
            asposeWb.Save(xlsxPath, SaveFormat.Xlsx);

            //var wb = new Workbook(xlsxPath);
            //var pdfPath = @"E:\usprime\SOA\SOA-" + soa.customerName + "-" + soa.fromDate.ToString("yyyyMMdd") + "-" + soa.toDate.ToString("yyyyMMdd") + ".pdf";
            //wb.Save(pdfPath, SaveFormat.Pdf);

            //_wb.ExportAsFixedFormat(XlFixedFormatType.xlTypePDF, pdfPath);
            return xlsxPath;
        }
    }

    public class ExpenseQueryData
    {
        public string CustomerCode { get; set; }

        public string[] WarehouseLocation { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime CloseDate { get; set; }

        public bool IfShowUnclosed { get; set; }
    }
}
