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

        // DELETE /api/FCRegularLocationDetail/{id} 删除库存记录，将记录的箱数件数移回SKU待分配
        [HttpDelete]
        public void RelocateLocation([FromUri]int id)
        {
            var locationInDb = _context.FCRegularLocationDetails
                .Include(x => x.PreReceiveOrder)
                .Include(x => x.RegularCaronDetail)
                .SingleOrDefault(x => x.Id == id);

            var preId = locationInDb.PreReceiveOrder.Id;

            //var cartonDetailsInDb = _context.RegularCartonDetails
            //    .Include(x => x.POSummary.PreReceiveOrder)
            //    .Where(x => x.POSummary.PreReceiveOrder.Id == preId
            //        && x.PurchaseOrder == locationInDb.PurchaseOrder
            //        && x.Style == locationInDb.Style
            //        && x.Color == locationInDb.Color
            //        && x.Customer == locationInDb.CustomerCode
            //        && x.PcsPerCarton == locationInDb.PcsPerCaron);

            var cartonDetailInDb = locationInDb.RegularCaronDetail;

            var availableCtns = locationInDb.AvailableCtns;
            var availablePcs = locationInDb.AvailablePcs;
            //var pickingCtns = locationInDb.PickingCtns;
            //var pickingPcs = locationInDb.PickingPcs;
            var shippedCtns = locationInDb.ShippedCtns;
            //var shippedPcs = locationInDb.ShippedPcs;

            //当pickingCtns不为0时，说明有货正在拣，不能进行移库操作。此项限制在前端完成
            //当库存剩余为0且没有货在拣，也不能进行移库操作。此项限制在前端完成

            cartonDetailInDb.ToBeAllocatedCtns += availableCtns;
            cartonDetailInDb.ToBeAllocatedPcs += availablePcs;

            locationInDb.AvailableCtns = 0;
            locationInDb.AvailablePcs = 0;
            locationInDb.Status = "Allocating";

            //当有库存剩余且没有已发出去的货的时候，删除库存记录(否则不删除记录)，将库存记录的库存剩余移至SKU待分配页面
            if (availableCtns != 0 && shippedCtns == 0)
            {
                _context.FCRegularLocationDetails.Remove(locationInDb);
            }

            _context.SaveChanges();
        }
    }
}
