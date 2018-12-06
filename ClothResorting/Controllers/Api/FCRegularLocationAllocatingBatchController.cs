using ClothResorting.Models;
using ClothResorting.Models.ApiTransformModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using ClothResorting.Helpers;
using AutoMapper;
using ClothResorting.Dtos;
using System.Web;
using ClothResorting.Models.StaticClass;

namespace ClothResorting.Controllers.Api
{
    public class FCRegularLocationAllocatingBatchController : ApiController
    {
        private ApplicationDbContext _context;
        private DateTime _timeNow;
        private string _userName;

        public FCRegularLocationAllocatingBatchController()
        {
            _context = new ApplicationDbContext();
            _timeNow = DateTime.Now;
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0];
        }

        // POST /api/FCRegularLocationAllocatingBatch
        [HttpPost]
        public IHttpActionResult CreateBatchLocationDetail([FromBody]ArrPreIdLocationJsonObj obj)
        {
            var locationDeatilList = new List<FCRegularLocationDetail>();
            var regularCartonDetailsIndb = _context.RegularCartonDetails
                .Include(c => c.POSummary.PreReceiveOrder)
                .Where(c => c.POSummary.PreReceiveOrder.Id == obj.PreId);

            var prereceiveOrder = regularCartonDetailsIndb.First().POSummary.PreReceiveOrder;

            foreach (var id in obj.Arr)
            {
                var regularCartonDetail = regularCartonDetailsIndb.SingleOrDefault(c => c.Id == id);

                if (regularCartonDetail.ToBeAllocatedCtns != 0)     //只有当待分配箱数不等于0的适合才在库存中建立对象。防止用户多次点击。
                {
                    if (regularCartonDetail.Container == null || regularCartonDetail.Container == "Unknown")
                    {
                        throw new Exception("Invalid contaier number. Container must be assigned first.");
                    }

                    var purchaseOrder = regularCartonDetail.PurchaseOrder;
                    var color = regularCartonDetail.Color;
                    var style = regularCartonDetail.Style;
                    var cartonRange = regularCartonDetail.CartonRange;

                    var inOneBoxSKU = regularCartonDetailsIndb
                        .Where(X => X.PurchaseOrder == purchaseOrder
                            && X.Batch == regularCartonDetail.Batch
                            && X.CartonRange == cartonRange);

                    foreach (var sku in inOneBoxSKU)
                    {
                        locationDeatilList.Add(new FCRegularLocationDetail
                        {
                            Container = sku.Container,
                            PurchaseOrder = purchaseOrder,
                            Style = style,
                            Color = color,
                            CustomerCode = sku.Customer,
                            SizeBundle = sku.SizeBundle,
                            PcsBundle = sku.PcsBundle,
                            Cartons = sku.ToBeAllocatedCtns,
                            Quantity = sku.ToBeAllocatedPcs,
                            PcsPerCaron = sku.PcsPerCarton,
                            Status = Status.InStock,
                            Location = obj.Location,
                            AvailableCtns = sku.ToBeAllocatedCtns,
                            PickingCtns = 0,
                            ShippedCtns = 0,
                            AvailablePcs = sku.ToBeAllocatedPcs,
                            PickingPcs = 0,
                            ShippedPcs = 0,
                            InboundDate = _timeNow,
                            PreReceiveOrder = prereceiveOrder,
                            RegularCaronDetail = sku,
                            CartonRange = cartonRange,
                            Allocator = _userName,
                            Vendor = sku.Vendor
                        });

                        sku.ToBeAllocatedCtns = 0;
                        sku.ToBeAllocatedPcs = 0;
                        sku.Status = Status.Allocated;
                    }
                }
                //locationDeatilList.Add(new FCRegularLocationDetail {
                //    Container = regularCartonDetail.POSummary.Container,
                //    PurchaseOrder = regularCartonDetail.PurchaseOrder,
                //    Style = regularCartonDetail.Style,
                //    Color = regularCartonDetail.Color,
                //    CustomerCode = regularCartonDetail.Customer,
                //    SizeBundle = regularCartonDetail.SizeBundle,
                //    PcsBundle = regularCartonDetail.PcsBundle,
                //    Cartons = regularCartonDetail.ToBeAllocatedCtns,
                //    Quantity = regularCartonDetail.ToBeAllocatedPcs,
                //    PcsPerCaron = regularCartonDetail.PcsPerCarton,
                //    Status = "In Stock",
                //    Location = obj.Location,
                //    AvailableCtns = regularCartonDetail.ToBeAllocatedCtns,
                //    PickingCtns = 0,
                //    ShippedCtns = 0,
                //    AvailablePcs = regularCartonDetail.Quantity,
                //    PickingPcs = 0,
                //    ShippedPcs = 0,
                //    InboundDate = _timeNow,
                //    PreReceiveOrder = regularCartonDetailsIndb.First().POSummary.PreReceiveOrder,
                //    RegularCaronDetail = regularCartonDetail
                //});

                //regularCartonDetailsIndb.SingleOrDefault(c => c.Id == id).ToBeAllocatedCtns = 0;
                //regularCartonDetailsIndb.SingleOrDefault(c => c.Id == id).ToBeAllocatedPcs = 0;
                //regularCartonDetailsIndb.SingleOrDefault(c => c.Id == id).Status = "Allocated";
            }

            _context.FCRegularLocationDetails.AddRange(locationDeatilList);
            _context.SaveChanges();

            ////获取刚写入数据库的记录
            //var latestRecords = _context.FCRegularLocationDetails.OrderByDescending(c => c.Id).Take(locationDeatilList.Count);

            //打散箱子算法中存在同时占用同一个context的问题，目前也没有必要打散
            //var breaker = new CartonBreaker(_context);

            //foreach (var record in latestRecords)
            //{
            //    breaker.BreakCartonBundle(record);
            //}

            //var recordsDto = Mapper.Map<List<FCRegularLocationDetail>, List<FCRegularLocationDetailDto>>(latestRecords.ToList());

            //返回剩下的所有仍然未分配的结果
            var result = _context.RegularCartonDetails
                .Include(c => c.POSummary.PreReceiveOrder)
                .Where(c => c.POSummary.PreReceiveOrder.Id == obj.PreId
                    && (c.ToBeAllocatedPcs != 0 || c.ToBeAllocatedCtns != 0))
                .Select(Mapper.Map<RegularCartonDetail, RegularCartonDetailDto>);

            try
            {
                return Created(Request.RequestUri + "/" + result.OrderBy(c => c.Id).First().Id + ":" + result.OrderByDescending(x => x.Id).First().Id, result);

            }
            catch(Exception e)
            {
                throw new Exception("Success! All cartons have been allocated.");
            }
        }
    }
}
