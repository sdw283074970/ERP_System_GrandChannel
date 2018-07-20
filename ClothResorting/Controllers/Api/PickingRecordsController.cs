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
    public class PickingRecordsController : ApiController
    {
        private ApplicationDbContext _context;
        private DateTime _timeNow;

        public PickingRecordsController()
        {
            _context = new ApplicationDbContext();
            _timeNow = DateTime.Now;
        }

        // GET /api/pickingrecords/{id}
        public IHttpActionResult GetAllPickingRecords([FromUri]int id)
        {
            var result = _context.PickingRecords
                .Include(c => c.PickingList)
                .Where(c => c.PickingList.Id == id)
                .ToList();

            var resultDto = Mapper.Map<List<PickingRecord>, List<PickingRecordDto>>(result);

            return Ok(resultDto);
        }

        // POST /api/pickingrecords/
        [HttpPost]
        public IHttpActionResult CreatePickingtRecords([FromBody]IdCpoJsonObj obj)
        {
            var pickingRecordList = new List<PickingRecord>();
            var pickingListInDb = _context.PickingLists
                .Include(c => c.PreReceiveOrder)
                .SingleOrDefault(c => c.Id == obj.PickingListId);

            var preid = pickingListInDb.PreReceiveOrder.Id;
            var opo = pickingListInDb.OrderPurchaseOrder;

            var locationDetailsInDb = _context.FCRegularLocationDetails
                .Include(c => c.PreReceiveOrder)
                .Where(c => c.PreReceiveOrder.Id == preid
                    && c.PurchaseOrder == obj.PurchaseOrder);

            foreach(var location in locationDetailsInDb)
            {
                location.Status = "Picking";

                pickingRecordList.Add(new PickingRecord {
                    Container = location.Container,
                    PurchaseOrder = location.PurchaseOrder,
                    Style = location.Style,
                    Color = location.Color,
                    SizeBundle = location.SizeBundle,
                    PcsBundle = location.PcsBundle,
                    CustomerCode = location.CustomerCode,
                    Cartons = location.Cartons,
                    Quantity = location.Quantity,
                    PcsPerCarton = location.PcsPerCaron,
                    Location = location.Location,
                    PickingDate = _timeNow,
                    PickingList = pickingListInDb,
                    OrderPurchaseOrder = opo,
                    FCRegularLocationDetail = location
                });
            }

            _context.PickingRecords.AddRange(pickingRecordList);
            _context.SaveChanges();

            var result = _context.PickingRecords.OrderByDescending(c => c.Id).Take(pickingRecordList.Count).ToList();

            var allResult = _context.PickingRecords
                .Include(c => c.PickingList)
                .Where(c => c.PickingList.Id == obj.PickingListId)
                .ToList();

            var allResultDto = Mapper.Map<List<PickingRecord>, List<PickingRecordDto>>(allResult);

            return Created(Request.RequestUri + "/" + result.Last().Id + ":" + result.First().Id, allResultDto);
        }

    }
}
