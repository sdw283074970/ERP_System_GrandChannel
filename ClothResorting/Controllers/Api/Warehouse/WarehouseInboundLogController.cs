using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Data.Entity;
using ClothResorting.Models.FBAModels.StaticModels;
using AutoMapper;
using ClothResorting.Models.FBAModels;
using ClothResorting.Models.StaticClass;
using ClothResorting.Dtos.Fba;

namespace ClothResorting.Controllers.Api.Warehouse
{
    public class WarehouseInboundLogController : ApiController
    {
        private ApplicationDbContext _context;

        public WarehouseInboundLogController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/warehouseinboundlog/
        [HttpGet]
        public IHttpActionResult GetAllProcessingMasterOrders()
        {
            var masterOrders = _context.FBAMasterOrders
                .Include(x => x.InvoiceDetails)
                .Include(x => x.FBAOrderDetails)
                .Include(x => x.FBAPallets)
                .Where(x => x.Status == FBAStatus.Processing || x.Status == FBAStatus.Finished)
                .ToList();

            var skuList = new List<int>();

            foreach (var m in masterOrders)
            {
                m.TotalAmount = (float)m.InvoiceDetails.Sum(x => x.Amount);
                m.TotalCBM = m.FBAOrderDetails.Sum(x => x.CBM);
                m.TotalCtns = m.FBAOrderDetails.Sum(x => x.Quantity);
                m.ActualCBM = m.FBAOrderDetails.Sum(x => x.ActualCBM);
                m.ActualCtns = m.FBAOrderDetails.Sum(x => x.ActualQuantity);
                m.ActualPlts = m.FBAPallets.Sum(x => x.ActualPallets);
                skuList.Add(m.FBAOrderDetails.GroupBy(x => x.ShipmentId).Count());
            }

            var resultDto = Mapper.Map<IList<FBAMasterOrder>, IList<FBAMasterOrderDto>>(masterOrders);

            for (int i = 0; i < masterOrders.Count; i++)
            {
                resultDto[i].SKUNumber = skuList[i];
            }

            return Ok();
        }
    }

    class InboundLog
    {
        public int Id { get; set; }

        public string Status { get; set; }

        public string Department { get; set; }

        public string Customer { get; set; }

        public string InboundType { get; set; }
    }
}
