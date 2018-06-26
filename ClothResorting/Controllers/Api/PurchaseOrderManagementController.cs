using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Models;
using ClothResorting.Models.ApiTransformModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ClothResorting.Controllers.Api
{
    public class PurchaseOrderManagementController : ApiController
    {
        private ApplicationDbContext _context;

        public PurchaseOrderManagementController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/purchaseordermanagement
        public IHttpActionResult GetAllPurchaseOrderInventory()
        {
            return Ok(_context.PurchaseOrderInventories
                .Where(c => c.Id > 0)
                .Select(Mapper.Map<PurchaseOrderInventory, PurchaseOrderInventoryDto>));
        }

        // POST /api/purchaseordermanagement
        public IHttpActionResult CreateNewPOInventory([FromBody]POInvJsonObj obj)
        {
            _context.PurchaseOrderInventories.Add(new PurchaseOrderInventory {
                PurchaseOrder = obj.PurchaseOrder,
                Vender = obj.Vender,
                OrderType = obj.OrderType,
                InvCtns = 0,
                InvPcs = 0
            });

            _context.SaveChanges();

            var result = _context.PurchaseOrderInventories
                .OrderByDescending(c => c.Id)
                .First();

            return Created(Request.RequestUri + "/" + result.Id, result);
        }
    }
}
