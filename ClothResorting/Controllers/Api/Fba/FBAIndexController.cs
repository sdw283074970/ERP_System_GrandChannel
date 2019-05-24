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

        // GET /api/fba/index
        [HttpGet]
        public IHttpActionResult GetActiveCustomers()
        {
            return Ok(_context.UpperVendors
                .Where(x => x.Status == Status.Active && x.DepartmentCode == DepartmentCode.FBA)
                .Select(Mapper.Map<UpperVendor, UpperVendorDto>));
        }

        // GET /api/fba/fbaindex/?customerId={customerId}&startDate={startDate}&closeDate={closeDate}
        [HttpGet]
        public IHttpActionResult GetExportedFilePath([FromUri]int customerId, [FromUri]DateTime startDate, [FromUri]DateTime closeDate)
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
