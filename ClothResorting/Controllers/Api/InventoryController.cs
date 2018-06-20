using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Data.Entity;
using System.Web.Http;
using ClothResorting.Models.ApiTransformModels;

namespace ClothResorting.Controllers.Api
{
    public class InventoryController : ApiController
    {
        private ApplicationDbContext _context;

        public InventoryController()
        {
            _context = new ApplicationDbContext();
        }

        //GET /api/Inventory/获取所有的PreReceiveOrders
        public IHttpActionResult GetPrereceiveOrder()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var preReceiveOrderLists = _context.PreReceiveOrders
                .Select(Mapper.Map<PreReceiveOrder, PreReceiveOrdersDto>);

            return Ok(preReceiveOrderLists);
        }

        //GET /api/Inventory/id/输入PreReceiveOrder的Id，返回所有的Po细节概览
        public IHttpActionResult GetPurchaseOrderDetail(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var purchaseOrderDetails = _context.PurchaseOrderSummarys
                .Where(s => s.PreReceiveOrder.Id == id)
                .Select(Mapper.Map<PurchaseOrderSummary, PurchaseOrderSummaryDto>);

            return Ok(purchaseOrderDetails);
        }


        // POST /api/Inventory/输入cartondetail的id，返回该id下箱子中所有的CartonBreakDown的所有细节
        [HttpPost]
        public IHttpActionResult GetCartonDetail([FromBody]int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var cartonInDb = _context.CartonDetails
                .Include(s => s.PurchaseOrderSummary.PreReceiveOrder)
                .SingleOrDefault(s => s.Id == id);

            //需要确保返回的breakdown结果与该cartondetail属于同一个po以及preReceivedOrder下
            var cartonNumberRangeTo = cartonInDb.CartonNumberRangeTo;
            var po = cartonInDb.PurchaseOrder;
            var preId = cartonInDb.PurchaseOrderSummary.PreReceiveOrder.Id;

            if (cartonNumberRangeTo != null)
            {
                var cartons = _context.CartonBreakDowns
                    .Include(c => c.CartonDetail.PurchaseOrderSummary.PreReceiveOrder)
                    .Where(c => c.CartonNumberRangeTo == cartonNumberRangeTo
                        && c.PurchaseOrder == po
                        && c.CartonDetail.PurchaseOrderSummary.PreReceiveOrder.Id == preId)
                    .Select(Mapper.Map<CartonBreakDown, CartonBreakDownDto>);
                return Created(new Uri(Request.RequestUri + "/" + "cartondetailid=" + id), cartons);
            }
            else
            {
                return BadRequest();
            }
        }

        // DELETE /api/Inventory
        [HttpDelete]
        public void RemoveAllRecord()
        {
            //移除数据库中[ClothSorting].[dbo].[LocationDetails]
            var locationDetails = _context.LocationDetails.Where(c => c.Id > 0);
            _context.LocationDetails.RemoveRange(locationDetails);

            //移除数据库中[ClothSorting].[dbo].[SizeRatios]
            var sizeRatios = _context.SizeRatios.Where(c => c.Id > 0);
            _context.SizeRatios.RemoveRange(sizeRatios);

            //移除数据库中[ClothSorting].[dbo].[CartonBreakDowns]
            var cartonBreakdowns = _context.CartonBreakDowns.Where(c => c.Id > 0);
            _context.CartonBreakDowns.RemoveRange(cartonBreakdowns);


            //移除数据库中[ClothSorting].[dbo].[CartonDetails]
            var cartonDetails = _context.CartonDetails.Where(c => c.Id > 0);
            _context.CartonDetails.RemoveRange(cartonDetails);

            //移除数据库中[ClothSorting].[dbo].[Measurements]
            var measurements = _context.Measurements.Where(c => c.Id > 0);
            _context.Measurements.RemoveRange(measurements);

            //移除数据库中[ClothSorting].[dbo].[PackingLists]
            var packingLists = _context.PurchaseOrderSummarys.Where(c => c.Id > 0);
            _context.PurchaseOrderSummarys.RemoveRange(packingLists);

            //移除数据库中[ClothSorting].[dbo].[PreReceiveOrders]
            var preReceiveOrders = _context.PreReceiveOrders.Where(c => c.Id > 0);
            _context.PreReceiveOrders.RemoveRange(preReceiveOrders);

            //移除数据库中[ClothSorting].[dbo].[RetrievingRecords]
            var retrievingRecords = _context.RetrievingRecords.Where(c => c.Id > 0);
            _context.RetrievingRecords.RemoveRange(retrievingRecords);

            //移除数据库中[ClothSorting].[dbo].[LoadPlanRecords]
            var loadPlanRecords = _context.LoadPlanRecords.Where(c => c.Id > 0);
            _context.LoadPlanRecords.RemoveRange(loadPlanRecords);

            _context.SaveChanges();
        }
    }
}
