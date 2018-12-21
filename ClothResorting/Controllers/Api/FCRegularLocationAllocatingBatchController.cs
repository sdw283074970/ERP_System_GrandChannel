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

        // POST /api/FCRegularLocationAllocatingBatch/?container={container}&batch={batch}&po={po}&style={style}&color={color}&sku={sku}&size={size}
        [HttpPost]
        public IHttpActionResult CreateBatchLocationDetail([FromBody]ArrPreIdLocationJsonObj obj, [FromUri]string container, [FromUri]string batch, [FromUri]string po, [FromUri]string style, [FromUri]string color, [FromUri]string sku, [FromUri]string size)
        {
            var locationDeatilList = new List<FCRegularLocationDetail>();
            var regularCartonDetailsIndb = _context.RegularCartonDetails
                .Include(c => c.POSummary.PreReceiveOrder)
                .Where(c => c.POSummary.PreReceiveOrder.Id == obj.PreId);

            var prereceiveOrder = regularCartonDetailsIndb.First().POSummary.PreReceiveOrder;

            foreach (var id in obj.Arr)
            {
                var regularCartonDetail = regularCartonDetailsIndb.SingleOrDefault(c => c.Id == id);

                if (regularCartonDetail.ToBeAllocatedCtns != 0)     //只有当待分配箱数不等于0的时候才在库存中建立对象。防止用户多次点击。
                {
                    if (regularCartonDetail.Container == null || regularCartonDetail.Container == "Unknown")
                    {
                        throw new Exception("Invalid contaier number. Container must be assigned first.");
                    }

                    var purchaseOrder = regularCartonDetail.PurchaseOrder;
                    var cartonRange = regularCartonDetail.CartonRange;

                    var inOneBoxSKU = regularCartonDetailsIndb
                        .Where(X => X.PurchaseOrder == purchaseOrder
                            && X.Batch == regularCartonDetail.Batch
                            && X.CartonRange == cartonRange);

                    foreach (var inBoxSKU in inOneBoxSKU)
                    {
                        locationDeatilList.Add(new FCRegularLocationDetail
                        {
                            Container = inBoxSKU.Container,
                            PurchaseOrder = inBoxSKU.PurchaseOrder,
                            Style = inBoxSKU.Style,
                            Color = inBoxSKU.Color,
                            CustomerCode = inBoxSKU.Customer,
                            SizeBundle = inBoxSKU.SizeBundle,
                            PcsBundle = inBoxSKU.PcsBundle,
                            Cartons = inBoxSKU.ToBeAllocatedCtns,
                            Quantity = inBoxSKU.ToBeAllocatedPcs,
                            PcsPerCaron = inBoxSKU.PcsPerCarton,
                            Status = Status.InStock,
                            Location = obj.Location,
                            AvailableCtns = inBoxSKU.ToBeAllocatedCtns,
                            PickingCtns = 0,
                            ShippedCtns = 0,
                            AvailablePcs = inBoxSKU.ToBeAllocatedPcs,
                            PickingPcs = 0,
                            ShippedPcs = 0,
                            InboundDate = _timeNow,
                            PreReceiveOrder = prereceiveOrder,
                            RegularCaronDetail = inBoxSKU,
                            CartonRange = cartonRange,
                            Allocator = _userName,
                            Vendor = inBoxSKU.Vendor,
                            Batch = inBoxSKU.Batch
                        });

                        inBoxSKU.ToBeAllocatedCtns = 0;
                        inBoxSKU.ToBeAllocatedPcs = 0;
                        inBoxSKU.Status = Status.Allocated;
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
            var resultDto = _context.RegularCartonDetails
                .Include(c => c.POSummary.PreReceiveOrder)
                .Where(c => c.POSummary.PreReceiveOrder.Id == obj.PreId
                    && (c.ToBeAllocatedPcs != 0 || c.ToBeAllocatedCtns != 0))
                .Select(Mapper.Map<RegularCartonDetail, RegularCartonDetailDto>);

            if (container != "NULL")
            {
                resultDto = resultDto.Where(x => x.Container == container);
            }

            if (batch != "NULL")
            {
                resultDto = resultDto.Where(x => x.Batch == batch);
            }

            if (po != "NULL")
            {
                resultDto = resultDto.Where(x => x.PurchaseOrder == po);
            }

            if (style != "NULL")
            {
                resultDto = resultDto.Where(x => x.Style == style);
            }

            if (color != "NULL")
            {
                resultDto = resultDto.Where(x => x.Color == color);
            }

            if (sku != "NULL")
            {
                resultDto = resultDto.Where(x => x.Customer == sku);
            }

            if (size != "NULL")
            {
                resultDto = resultDto.Where(x => x.SizeBundle == size);
            }

            if (resultDto.Count() == 0)
            {
                return Ok(resultDto);
            }
            else
            {
                return Created(Request.RequestUri + "/" + resultDto.OrderBy(c => c.Id).First().Id + ":" + resultDto.OrderByDescending(x => x.Id).First().Id, resultDto);
            }
        }
    }
}
