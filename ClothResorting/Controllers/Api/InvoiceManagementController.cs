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

namespace ClothResorting.Controllers.Api
{
    public class InvoiceManagementController : ApiController
    {
        private ApplicationDbContext _context;

        public InvoiceManagementController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/invoiceManagement/?vendor={vendor}&departmentCode={departmentCode}
        [HttpGet]
        public IHttpActionResult GetInvoiceList([FromUri]string vendor, [FromUri]string departmentCode)
        {
            var resultDto = _context.Invoices
                .Include(x => x.UpperVendor)
                .Where(x => x.UpperVendor.Name == vendor && x.UpperVendor.DepartmentCode == departmentCode)
                .Select(Mapper.Map<Invoice, InvoiceDto>);

            return Ok(resultDto);
        }

        // GET /api/invoicemanagement/?invoiceId={invoiceId}
        [HttpGet]
        public IHttpActionResult GetInvoice([FromUri]int invoiceId)
        {
            var invoiceInDb = _context.Invoices.Find(invoiceId);

            return Ok(Mapper.Map<Invoice, InvoiceDto>(invoiceInDb));
        }

        // POST /api/invoicemanagement/
        [HttpPost]
        public IHttpActionResult CreateNewInvoice([FromBody]CreateNewInvoiceJsonObj obj)
        {
            var vendorInDb = _context.UpperVendors.SingleOrDefault(x => x.Name == obj.Vendor && x.DepartmentCode == obj.DepartmentCode);

            var newInvoice = new Invoice
            {
                InvoiceNumber = obj.InvoiceNumber,
                InvoiceType = "",
                InvoiceDate = obj.InvoiceDate,
                TotalDue = 0,
                Terms = obj.Terms,
                PurchaseOrder = obj.PurchaseOrder,
                DueDate = obj.DueDate,
                Enclosed = obj.Enclosed,
                ShipDate = obj.ShipDate,
                ShipVia = obj.ShipVia,
                BillTo = obj.BillTo,
                ShipTo = obj.ShipTo,
                Currency = obj.Currency,
                UpperVendor = vendorInDb,
                Container = obj.Container
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

            var sampleDto = Mapper.Map<Invoice, InvoiceDto>(_context.Invoices.OrderByDescending(x => x.Id).First());

            return Created(Request.RequestUri + "/" + sampleDto.Id, sampleDto);
        }

        // POST /api/invoicemanagement/{id}
        [HttpPut]
        public void UpdateNewInvoice([FromUri]int id, [FromBody]CreateNewInvoiceJsonObj obj)
        {
            var invoiceInDb = _context.Invoices.Find(id);

            invoiceInDb.InvoiceNumber = obj.InvoiceNumber;
            invoiceInDb.InvoiceDate = obj.InvoiceDate;
            invoiceInDb.Terms = obj.Terms;
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
