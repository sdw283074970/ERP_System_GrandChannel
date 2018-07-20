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

namespace ClothResorting.Controllers.Api
{
    public class PickingListManagementController : ApiController
    {
        private ApplicationDbContext _context;

        public PickingListManagementController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/pickinglistmanagement/{id}(preReceivedId)
        public IHttpActionResult GetAllPackingList([FromUri]int id)
        {
            var result = _context.PickingLists
                .Include(c => c.PreReceiveOrder)
                .Where(c => c.PreReceiveOrder.Id == id)
                .ToList();

            var resultDto = Mapper.Map<List<PickingList>, List<PickingListDto>>(result);

            return Ok(resultDto);
        }

        // POST /api/pickinglistmanagement/
        [HttpPost]
        public IHttpActionResult CreatePickingList([FromBody]OpoRangePreidJsonObj obj)
        {
            var preReceiveOrderInDb = _context.PreReceiveOrders.Find(obj.Preid);

            _context.PickingLists.Add(new PickingList {
                CreateDate = DateTime.Now,
                OrderPurchaseOrder = obj.OrderPurchaseOrder,
                PickTicketsRange = obj.Range,
                PreReceiveOrder = preReceiveOrderInDb,
                Status = "Picking"
            });

            _context.SaveChanges();

            var latestRecord = _context.PickingLists.OrderByDescending(c => c.Id).First();

            var latestRecordDto = Mapper.Map<PickingList, PickingListDto>(latestRecord);

            return Created(Request.RequestUri + "/" + latestRecord.Id, latestRecordDto);
        }

        // PUT /api/picklinglistmanagement/{id}(pickingListId)
        [HttpPut]
        public void UpdatePickingListStatus([FromUri]int id)
        {
            var pickingRecordsInDb = _context.PickingRecords
                .Include(c => c.FCRegularLocationDetail)
                .Include(c => c.PickingList)
                .Where(c => c.PickingList.Id == id);

            foreach(var record in pickingRecordsInDb)
            {
                record.FCRegularLocationDetail.Status = "Shipped";
            }

            var pickingListInDb = _context.PickingLists.Find(id);

            pickingListInDb.Status = "Shipped";

            _context.SaveChanges();
        }
    }
}
