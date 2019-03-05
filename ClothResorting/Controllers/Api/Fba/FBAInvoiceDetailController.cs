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

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBAInvoiceDetailController : ApiController
    {
        private ApplicationDbContext _context;

        public FBAInvoiceDetailController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/FBAInvoiceDetail/?customerId={customerId}&reference={reference}&invoiceType={invoiceType}
        [HttpGet]
        public IHttpActionResult GetInvoiceDetails([FromUri]int customerId, [FromUri]string reference, [FromUri]string invoiceType)
        {
            if (invoiceType == "MasterOrder")
            {
                return Ok(_context.InvoiceDetails
                    .Include(x => x.FBAMasterOrder)
                    .Where(x => x.FBAMasterOrder.Container == reference)
                    .Select(Mapper.Map<InvoiceDetail, InvoiceDetailDto>));
            }
            else if (invoiceType == "ShipOrder")
            {
                return Ok(_context.InvoiceDetails
                    .Include(x => x.FBAShipOrder)
                    .Where(x => x.FBAShipOrder.ShipOrderNumber == reference)
                    .Select(Mapper.Map<InvoiceDetail, InvoiceDetailDto>));
            }
            else
            {
                return Ok();
            }
        }

        // GET /api/FBAInvoiceDetail/?customerId={customerId}&reference={reference}&invoiceType={invoiceType}ajaxStep={ajaxStep}
        [HttpGet]
        public IHttpActionResult GetCtnsAndPlts([FromUri]int customerId, [FromUri]string reference, [FromUri]string invoiceType, [FromUri]int ajaxStep)
        {
            switch(ajaxStep)
            {
                case 0:        //ajax 第0步 获取该单号下的托盘总数和箱子总数
                    {
                        if (invoiceType == "MasterOrder")
                        {
                            var plts = _context.FBAPallets
                                .Where(x => x.Container == reference)
                                .Sum(x => x.ActualPallets);

                            var ctns = _context.FBAOrderDetails
                                .Where(x => x.Container == reference)
                                .Sum(x => x.ActualQuantity);

                            return Ok(new { Pallets = plts, Cartons = ctns});
                        }
                        else if (invoiceType == "ShipOrder")
                        {
                            var plts = _context.FBAPickDetails
                                .Include(x => x.FBAShipOrder)
                                .Where(x => x.FBAShipOrder.ShipOrderNumber == reference)
                                .Sum(x => x.ActualPlts);

                            var ctns = _context.FBAPickDetails
                                .Include(x => x.FBAShipOrder)
                                .Where(x => x.FBAShipOrder.ShipOrderNumber == reference)
                                .Sum(x => x.ActualQuantity);

                            return Ok(new { Pallets = plts, Cartons = ctns });
                        }
                        else
                        {
                            return Ok();
                        }
                    }
            }

            return Ok();
        }

        // GET /api/FBAInvoiceDetail/?customerId={customerId}  Ajax第1步，获取收费项目分类
        [HttpGet]
        public IHttpActionResult GetChargingTypes([FromUri]int customerId)
        {
            var typesGroup = _context.ChargingItems
                .Include(x => x.UpperVendor)
                .Where(x => x.UpperVendor.Id == customerId)
                .GroupBy(x => x.ChargingType);

            var typesList = new List<string>();

            foreach (var g in typesGroup)
            {
                typesList.Add(g.First().ChargingType);
            }

            return Ok(typesList);
        }

        // GET /api/FBAinvoiceDetail/?customerId={customerId}&chargingType={chargingType}             Ajax2, 改变下拉菜单的收费类型，获取该收费类型的所有选项
        [HttpGet]
        public IHttpActionResult GetChargingItems([FromUri]int customerId, [FromUri]string chargingType)
        {
            var nameList = new List<string>();

            var chargingItemInDb = _context.ChargingItems
                .Include(x => x.UpperVendor)
                .Where(x => x.ChargingType == chargingType
                    && x.UpperVendor.Id == customerId);

            foreach (var c in chargingItemInDb)
            {
                nameList.Add(c.Name);
            }

            return Ok(nameList);
        }

        // GET /api/FBAInvoiceDetail/?customerId={customerId}&itemName={itemName}
        //名称可能包含特殊符号，只能走body
        [HttpGet]
        public IHttpActionResult GetRate([FromUri]int customerId, [FromUri]string itemName)
        {
            var chargingItemInDb = _context.ChargingItems
                .Include(x => x.UpperVendor)
                .SingleOrDefault(x => x.Name == itemName
                    && x.UpperVendor.Id == customerId);

            var annoyObj = new
            {
                Rate = chargingItemInDb.Rate,
                Unit = chargingItemInDb.Unit,
                Description = chargingItemInDb.Description
            };

            return Ok(annoyObj);
        }
    }
}
