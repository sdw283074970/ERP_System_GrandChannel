using AutoMapper;
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
            var dtos = _context.UpperVendors
                .Where(x => x.DepartmentCode == DepartmentCode.FBA)
                .Select(Mapper.Map<UpperVendor, UpperVendorDto>)
                .ToList();

            //统计在拣货的数量
            var pickDetails = _context.FBAPickDetails
                .Include(x => x.FBAShipOrder)
                .Where(x => x.FBAShipOrder.Status != FBAStatus.Shipped)
                .ToList();

            var customerGroupPickDetails = pickDetails.GroupBy(x => x.FBAShipOrder.CustomerCode);

            foreach(var c in customerGroupPickDetails)
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

            foreach(var c in cartonLocationGroup)
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

            //统计未付款发票数量
            var payableMasterOrderInvoice = _context.FBAMasterOrders
                .Include(x => x.Customer)
                .Where(x => x.InvoiceStatus == "Await")
                .ToList();

            var masterOrderInvoiceGroup = payableMasterOrderInvoice.GroupBy(x => x.Customer.CustomerCode);

            foreach(var m in masterOrderInvoiceGroup)
            {
                var dto = dtos.SingleOrDefault(x => x.CustomerCode == m.First().Customer.CustomerCode);
                dto.PayableInvoices = m.Count();
            }

            var payableShipOrderInvoice = _context.FBAShipOrders
                .Where(x => x.InvoiceStatus == "Await")
                .ToList();

            var shipOrderInvoiceGroup = payableShipOrderInvoice.GroupBy(x => x.CustomerCode);

            foreach(var s in shipOrderInvoiceGroup)
            {
                var dto = dtos.SingleOrDefault(x => x.CustomerCode == s.First().CustomerCode);

                if (dto == null)
                    continue;

                dto.PayableInvoices += s.Count();
            }

            return Ok(dtos.OrderByDescending(x => x.Id));
        }

        // GET /api/fba/fbaindex/?customerId={customerId}&startDate={startDate}&closeDate={closeDate}
        [HttpGet]
        public IHttpActionResult GetExportedFilePath([FromUri]int customerId, [FromUri]DateTime startDate, [FromUri]DateTime closeDate, [FromUri]bool ifShowUnclosed)
        {
            var templatePath = @"D:\Template\FBA-InvoiceReport-Template.xls";

            var excelGenerator = new FBAInvoiceHelper(templatePath);

            //如果customerId等于0说明是要所有客户的记录
            if (customerId == 0)
            {
                var info = excelGenerator.GetAllFBACustomerChargingReportFromDate(startDate, closeDate);

                var path = excelGenerator.GenerateExcelFileForAllCustomerAndReturnPath(info);

                return Ok(path);
            }
            else if (ifShowUnclosed)
            {
                var info = excelGenerator.GetAllChargingReportFormDateRangeAndCustomerId(customerId, startDate, closeDate);

                var path = excelGenerator.GenerateExcelFileAndReturnPath(info);

                return Ok(path);
            }
            else
            {
                var info = excelGenerator.GetChargingReportFormDateRangeAndCustomerId(customerId, startDate, closeDate);

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

    }
}
