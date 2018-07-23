using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using ClothResorting.Models;
using AutoMapper;
using ClothResorting.Dtos;

namespace ClothResorting.Controllers.Api
{
    public class FCRegularLocationDetailController : ApiController
    {
        private ApplicationDbContext _context;

        public FCRegularLocationDetailController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/fcregularlocationdetail/{preid}
        public IHttpActionResult GetAllLocationDetail([FromUri]int id)
        {
            var resultDto = _context.FCRegularLocationDetails
                .Include(c => c.PreReceiveOrder)
                .Where(c => c.PreReceiveOrder.Id == id)
                .ToList()
                .Select(Mapper.Map<FCRegularLocationDetail, FCRegularLocationDetailDto>);

            return Ok(resultDto);
        }

        // DELETE /api/FCRegularLocationDetail/ 删除库存记录，将记录的箱数件数移回SKU待分配
        [HttpDelete]
        public void RelocateLocation([FromUri]int id)
        {
            var locationInDb = _context.FCRegularLocationDetails
                .Include(x => x.PreReceiveOrder)
                .SingleOrDefault(x => x.Id == id);

            var preId = locationInDb.PreReceiveOrder.Id;

            var cartonDetailInDb = _context.RegularCartonDetails
                .Include(x => x.POSummary.PreReceiveOrder)
                .SingleOrDefault(x => x.POSummary.PreReceiveOrder.Id == preId
                    && x.PurchaseOrder == locationInDb.PurchaseOrder
                    && x.Style == locationInDb.Style
                    && x.Color == locationInDb.Color
                    && x.Customer == locationInDb.CustomerCode);

            cartonDetailInDb.ToBeAllocatedCtns += locationInDb.Cartons;
            cartonDetailInDb.ToBeAllocatedPcs += locationInDb.Quantity;

            _context.FCRegularLocationDetails.Remove(locationInDb);
            _context.SaveChanges();
        }
    }
}
