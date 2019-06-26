using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Data.Entity;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Models.StaticClass;
using ClothResorting.Models.ApiTransformModels;
using ClothResorting.Helpers;
using System.Threading.Tasks;

namespace ClothResorting.Controllers.Api
{
    public class InvoiceDetailController : ApiController
    {
        private ApplicationDbContext _context;

        public InvoiceDetailController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/invoiceDetail/{id}
        [HttpGet]
        public IHttpActionResult GetInvoiceInfo([FromUri]int id)
        {
            var invoiceInDb = _context.Invoices.Find(id);

            var reference = Status.Unknown;
            var ctns = 0;
            var pcs = 0;

            if (invoiceInDb.PurchaseOrder == "")
            {
                if (invoiceInDb.Container != "")
                {
                    reference = "Container#:" + invoiceInDb.Container;

                    var cartonDeatilList = _context.RegularCartonDetails
                        .Where(x => x.Container == invoiceInDb.Container)
                        .ToList();

                    ctns = cartonDeatilList.Count == 0 ? 0 : cartonDeatilList.Sum(x => x.ActualCtns);
                    pcs = cartonDeatilList.Count == 0 ? 0 : cartonDeatilList.Sum(x => x.ActualPcs);
                }
            }
            else
            {
                reference = "PO#:" + invoiceInDb.PurchaseOrder;

                var pickDetailList = _context.PickDetails
                    .Include(x => x.ShipOrder)
                    .Where(x => x.ShipOrder.OrderPurchaseOrder.Contains(invoiceInDb.PurchaseOrder))
                    .ToList();

                ctns = pickDetailList.Count == 0 ? 0 : pickDetailList.Sum(x => x.PickCtns);
                pcs = pickDetailList.Count == 0 ? 0 : pickDetailList.Sum(x => x.PickPcs);
            }

            var annoyObj = new {
                BillTo = "BILL TO: " + invoiceInDb.BillTo,
                ShipTo = "SHIP TO: " + invoiceInDb.ShipTo,
                ShipDate = "SHIP DATE: " + invoiceInDb.ShipDate,
                ShipVia = "SHIP VIA: " + invoiceInDb.ShipVia,
                Reference = reference,
                Ctns = ctns,
                Pcs = pcs
            };

            return Ok(annoyObj);
        }

        // GET /api/invocieDetail/?invoiceId={invoiceId}
        [HttpGet]
        public IHttpActionResult GetInvoiceDetails([FromUri]int invoiceId)
        {
            var resultDto = _context.InvoiceDetails
                .Include(x => x.Invoice)
                .Where(x => x.Invoice.Id == invoiceId)
                .Select(Mapper.Map<InvoiceDetail, InvoiceDetailDto>);

            return Ok(resultDto);
        }

        // GET /api/invoiceDetail/?vendor={vendor}&departmentCode={departmentCode}
        [HttpGet]
        public IHttpActionResult GetChargingTypes([FromUri]string vendor, [FromUri]string departmentCode)
        {
            var typesGroup = _context.ChargingItems
                .Include(x => x.UpperVendor)
                .Where(x => x.UpperVendor.Name == vendor && x.UpperVendor.DepartmentCode == departmentCode)
                .GroupBy(x => x.ChargingType);

            var typesList = new List<string>();

            foreach (var g in typesGroup)
            {
                typesList.Add(g.First().ChargingType);
            }

            return Ok(typesList);
        }

        // PUT /api/invoiceDetail/
        //GET方法中的URL如果不重新编码，无法传带&符号的参数，只能通过POST中的Body传参
        [HttpPut]
        public IHttpActionResult GetChargingNames([FromBody]VendorDepartmentCodeTypeJsonObj obj)
        {
            var nameList = new List<string>();

            var chargingItemInDb = _context.ChargingItems
                .Include(x => x.UpperVendor)
                .Where(x => x.ChargingType == obj.Item
                    && x.UpperVendor.Name == obj.Vendor
                    && x.UpperVendor.DepartmentCode == obj.DepartmentCode);

            foreach (var c in chargingItemInDb)
            {
                nameList.Add(c.Name);
            }

            return Ok(nameList);
        }

        // POST /api/invoiceDetail/
        [HttpPost]
        public IHttpActionResult CreateNewChargingDetail([FromBody]InvoiceDetailJsonObj obj)
        {
            var invoiceInDb = _context.Invoices.Find(obj.invoiceId);

            var newInvoiceDetail = new InvoiceDetail
            {
                Activity = obj.Activity,
                ChargingType = obj.ChargingType,
                Unit = obj.Unit,
                Rate = obj.Rate,
                Amount = obj.Amount,
                Quantity = obj.Quantity,
                Memo = obj.Memo,
                Invoice = invoiceInDb
            };

            invoiceInDb.TotalDue += obj.Amount;

            _context.InvoiceDetails.Add(newInvoiceDetail);
            _context.SaveChanges();

            var sampleDto = Mapper.Map<InvoiceDetail, InvoiceDetailDto>(_context.InvoiceDetails.OrderBy(x => x.Id).First());

            return Created(Request.RequestUri + "/" + sampleDto.Id, sampleDto);
        }

        // POST /api/invoiceDetail/?vendor={vendor}&departmentCode={departmentCode}
        //GET方法中的URL无法传带&符号的参数，只能通过POST中的Body传参
        [HttpPost]
        public IHttpActionResult GetChargingItemDetail([FromUri]string vendor, [FromUri]string departmentCode, [FromBody]string itemName)
        {
            var chargingItemInDb = _context.ChargingItems
                .Include(x => x.UpperVendor)
                .SingleOrDefault(x => x.Name == itemName
                    && x.UpperVendor.Name == vendor
                    && x.UpperVendor.DepartmentCode == departmentCode);

            var annoyObj = new
            {
                Rate = chargingItemInDb.Rate,
                Unit = chargingItemInDb.Unit,
                Description = chargingItemInDb.Description
            };

            return Ok(annoyObj);
        }

        // DELETE /api/invoiceDetail/{id}
        [HttpDelete]
        public async Task DeleteInvoiceDetail([FromUri]int id)
        {
            var invoiceDetailInDb = _context.InvoiceDetails
                .Include(x => x.Invoice)
                .SingleOrDefault(x => x.Id == id);

            var logger = new Logger(_context);
            var invoiceDetailDto = Mapper.Map<InvoiceDetail, InvoiceDetailDto>(invoiceDetailInDb);
            await logger.AddDeletedLogAsync<InvoiceDetail>(invoiceDetailDto, "Deleted invoice detail directly", null, OperationLevel.High);

            _context.InvoiceDetails.Remove(invoiceDetailInDb);
            _context.SaveChanges();

            //if (invoiceDetailInDb.Invoice != null)
            //{
            //    var invoiceId = invoiceDetailInDb.Invoice.Id;
            //    var invoiceInDb = _context.Invoices
            //        .Include(x => x.InvoiceDetails)
            //        .SingleOrDefault(x => x.Id == invoiceId);

            //    invoiceInDb.TotalDue = invoiceInDb.InvoiceDetails.Sum(x => x.Amount);

            //    _context.SaveChanges();
            //}
        }
    }
}
