using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Models.ApiTransformModels;
using System.Globalization;
using Microsoft.Office.Interop.Excel;
using ClothResorting.Models.StaticClass;
using System.Web;
using ClothResorting.Helpers;

namespace ClothResorting.Controllers.Api
{
    public class InvoiceManagementController : ApiController
    {
        private ApplicationDbContext _context;
        private string _userName;

        public InvoiceManagementController()
        {
            _context = new ApplicationDbContext();
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0] == "" ? (HttpContext.Current.Request.Headers.Get("AppUser") == null ? "" : HttpContext.Current.Request.Headers.Get("AppUser").Split('@')[0]) : HttpContext.Current.User.Identity.Name.Split('@')[0];
        }

        // GET /api/invoiceManagement/?vendor={vendor}&departmentCode={departmentCode}
        [HttpGet]
        public IHttpActionResult GetInvoiceList([FromUri]string vendor, [FromUri]string departmentCode)
        {
            var resultList = _context.Invoices
                .Include(x => x.UpperVendor)
                .Include(x => x.InvoiceDetails)
                .Where(x => x.UpperVendor.Name == vendor && x.UpperVendor.DepartmentCode == departmentCode)
                .ToList();

            foreach(var r in resultList)
            {
                r.TotalDue = r.InvoiceDetails.Sum(x => x.Amount);
            }

            var resultDto = Mapper.Map<IEnumerable<Models.Invoice>, IEnumerable<InvoiceDto>>(resultList);

            return Ok(resultDto);
        }

        // GET /api/invoicemanagement/?invoiceId={invoiceId}
        [HttpGet]
        public IHttpActionResult GetInvoice([FromUri]int invoiceId)
        {
            var invoiceInDb = _context.Invoices.Find(invoiceId);

            return Ok(Mapper.Map<Models.Invoice, InvoiceDto>(invoiceInDb));
        }

        // POST /api/invoicemanagement/
        [HttpPost]
        public IHttpActionResult CreateNewInvoice([FromBody]CreateNewInvoiceJsonObj obj)
        {
            var vendorInDb = _context.UpperVendors.SingleOrDefault(x => x.Name == obj.Vendor && x.DepartmentCode == obj.DepartmentCode);

            var newInvoice = new Models.Invoice
            {
                InvoiceNumber = "Unsynchronised",
                InvoiceType = "",
                InvoiceDate = obj.InvoiceDate,
                TotalDue = 0,
                PurchaseOrder = obj.PurchaseOrder,
                DueDate = obj.DueDate,
                Enclosed = obj.Enclosed,
                ShipDate = obj.ShipDate,
                ShipVia = obj.ShipVia,
                BillTo = obj.BillTo,
                ShipTo = obj.ShipTo,
                Currency = obj.Currency,
                UpperVendor = vendorInDb,
                Container = obj.Container,
                RequestId = "",
                CreatedBy = _userName,
                CreatedDate = DateTime.Now
            };


            if (obj.Container != "" && obj.PurchaseOrder != "")
            {
                newInvoice.InvoiceType = InvoiceType.Operation;
            }
            else if (obj.Container != "")
            {
                newInvoice.InvoiceType = InvoiceType.Receiving;
            }
            else if (obj.PurchaseOrder != "")
            {
                newInvoice.InvoiceType = InvoiceType.OperationAndShipping;
            }

            _context.Invoices.Add(newInvoice);
            _context.SaveChanges();

            var sampleDto = Mapper.Map<Models.Invoice, InvoiceDto>(_context.Invoices.OrderByDescending(x => x.Id).First());

            return Created(Request.RequestUri + "/" + sampleDto.Id, sampleDto);
        }

        // POST /api/invoicemanagement/?vendor={vendor}
        [HttpPost]
        public IHttpActionResult SyncAllInvoicesByVendor([FromUri]string vendor)
        {
            var service = new QBOServiceManager();

            service.SyncInvoiceFromQBO(vendor);

            return Created(Request.RequestUri, "Sync Success!");
        }

        // POST /api/invoicemanagement/{id}
        [HttpPut]
        public void UpdateNewInvoice([FromUri]int id, [FromBody]CreateNewInvoiceJsonObj obj)
        {
            var invoiceInDb = _context.Invoices.Find(id);

            //invoiceInDb.InvoiceNumber = obj.InvoiceNumber;
            invoiceInDb.InvoiceDate = obj.InvoiceDate;
            invoiceInDb.PurchaseOrder = obj.PurchaseOrder;
            invoiceInDb.DueDate = obj.DueDate;
            invoiceInDb.Enclosed = obj.Enclosed;
            invoiceInDb.ShipDate = obj.ShipDate;
            invoiceInDb.ShipVia = obj.ShipVia;
            invoiceInDb.BillTo = obj.BillTo;
            invoiceInDb.ShipTo = obj.ShipTo;
            invoiceInDb.Currency = obj.Currency;
            invoiceInDb.Container = obj.Container;
            
            if (obj.Container != "" && obj.PurchaseOrder != "")
            {
                invoiceInDb.InvoiceType = InvoiceType.Operation;
            }
            else if (obj.Container != "")
            {
                invoiceInDb.InvoiceType = InvoiceType.Receiving;
            }
            else if (obj.PurchaseOrder != "")
            {
                invoiceInDb.InvoiceType = InvoiceType.OperationAndShipping;
            }

            _context.SaveChanges();
        }

        // DELETE /api/invoicemanagement/{id}
        public void DeleteInvoice([FromUri]int id)
        {
            var invoiceInDb = _context.Invoices
                .Include(x => x.InvoiceDetails)
                .SingleOrDefault(x => x.Id == id);

            var invoiceDetailInDb = invoiceInDb.InvoiceDetails;

            _context.InvoiceDetails.RemoveRange(invoiceDetailInDb);
            _context.Invoices.Remove(invoiceInDb);
            _context.SaveChanges();
        }
    }
}
