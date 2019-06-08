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
using ClothResorting.Helpers;
using System.Web;
using ClothResorting.Models.StaticClass;

namespace ClothResorting.Controllers.Api
{
    public class FCRegularLocationAllocatingController : ApiController
    {
        private ApplicationDbContext _context;
        private string _userName;

        public FCRegularLocationAllocatingController()
        {
            _context = new ApplicationDbContext();
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0];
        }

        // GET /api/fcregularlocationallocating/?preId={preid}&container={container}&batch={batch}&po={po}&style={style}&color={color}&sku={sku}&size={size}/ 获取还没有被分配的SKU
        public IHttpActionResult GetUnallocatedCartons([FromUri]int preId, [FromUri]string container, [FromUri]string batch, [FromUri]string po, [FromUri]string style, [FromUri]string color, [FromUri]string sku, [FromUri]string size)
        {
            var resultDto = _context.RegularCartonDetails
                .Include(c => c.POSummary.PreReceiveOrder)
                .Where(c => c.POSummary.PreReceiveOrder.Id == preId
                    && (c.ToBeAllocatedPcs != 0 || c.ToBeAllocatedCtns != 0)
                    && c.OrderType != OrderType.Replenishment)      //这种入库方法不支持补货类型的订单(有另外一套体系)
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

            return Ok(resultDto);
        }

        // POST /api/fcregularlocationallocating/?container={container}&batch={batch}&po={po}&style={style}&color={color}&sku={sku}&size={size} 根据传入数据分解已收货对象，obj.id为cartondetail的id，cartondetail在此处用作记录待分配箱数及件数
        [HttpPost]
        public IHttpActionResult CreateRegularStock([FromBody]IEnumerable<FCRegularLocationAllocatingJsonObj> objArray, [FromUri]string container, [FromUri]string batch, [FromUri]string po, [FromUri]string style, [FromUri]string color, [FromUri]string sku, [FromUri]string size)
        {
            var preId = objArray.First().PreId;
            var preReceiveOrderInDb = _context.PreReceiveOrders.Find(preId);

            foreach (var obj in objArray)
            {
                CreateRegularLocation(_context, preReceiveOrderInDb, obj.Id, obj.Cartons, obj.Location);
            }

            var latestRecord = _context.FCRegularLocationDetails.OrderByDescending(c => c.Id).First();

            //打散箱子算法中存在同时占用同一个context的问题，目前也没有必要打散
            //var breaker = new CartonBreaker(_context);

            //breaker.BreakCartonBundle(latestRecord);

            var latestRecordDto = Mapper.Map<FCRegularLocationDetail, FCRegularLocationDetailDto>(latestRecord);

            //返回剩下仍然未分配的结果
            var resultDto = _context.RegularCartonDetails
                .Include(c => c.POSummary.PreReceiveOrder)
                .Where(c => c.POSummary.PreReceiveOrder.Id == preId
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

            try
            {
                return Created(Request.RequestUri + "/" + latestRecordDto.Id, resultDto);
            }
            catch (Exception e)
            {
                return Ok();
                throw new Exception("Success! All cartons have been allocated.");
            }
        }

        // POST /api/fcregularlocationallocating/?deatilId={detailId}&operation={operation}
        [HttpPost]
        public IHttpActionResult ApplyPreallocatingString([FromUri]int detailId, [FromUri]string operation)
        {
            if (operation == "Apply")
            {
                var cartonDetailInDb = _context.RegularCartonDetails
                    .Include(x => x.POSummary.PreReceiveOrder)
                    .SingleOrDefault(x => x.Id == detailId);

                var preReceiveOrderInDb = cartonDetailInDb.POSummary.PreReceiveOrder;

                var parser = new StringParser();

                //如果预分配字符串为空则报错
                if (cartonDetailInDb.PreLocation == "" || cartonDetailInDb.PreLocation == " " || cartonDetailInDb.PreLocation == null)
                {
                    throw new Exception("Pre-location cannot be null");
                }

                //如果该detail对象已经有部分被手工分配了则报错
                if(cartonDetailInDb.Cartons != cartonDetailInDb.ToBeAllocatedCtns)
                {
                    throw new Exception("Cannot apply pre-location string cuz part of cartons have been allocated");
                }

                var list = parser.ParseStrToPreLoc(cartonDetailInDb.PreLocation);
                var totalCtns = SumTotalCartons(list);

                //如果pre-allocating string中的总箱数大于deatils的可分配箱数则报错
                if (totalCtns > cartonDetailInDb.ToBeAllocatedCtns)
                {
                    throw new Exception("Cannot apply pre-location string cuz not enough ctns available");
                }

                //开始分配库位
                foreach(var l in list)
                {
                    CreateRegularLocation(_context, preReceiveOrderInDb, detailId, l.Ctns * l.Plts, l.Location);
                }
            }

            return Created(Request.RequestUri, " ");
        }

        private void CreateRegularLocation(ApplicationDbContext context, PreReceiveOrder preReceiveOrderInDb, int deatilId, int cartons, string location)
        {
            var cartonRange = context.RegularCartonDetails
                .Include(c => c.POSummary.PreReceiveOrder)
                .SingleOrDefault(c => c.Id == deatilId)
                .CartonRange;

            var poSummaryId = context.RegularCartonDetails
                .Include(c => c.POSummary.PreReceiveOrder)
                .SingleOrDefault(c => c.Id == deatilId)
                .POSummary
                .Id;

            var regularCartonDetail = context.RegularCartonDetails.Find(deatilId);

            var inOneBoxSKUs = context.RegularCartonDetails
                .Include(c => c.POSummary.PreReceiveOrder)
                .Where(c => c.CartonRange == cartonRange
                    && c.POSummary.Id == poSummaryId
                    && c.Batch == regularCartonDetail.Batch);


            var index = 1;      //用来甄别多种SKU在同一箱的情况

            foreach (var cartonDetailInDb in inOneBoxSKUs)
            {
                cartonDetailInDb.Status = "Allocating";

                if (cartonDetailInDb.Container == null || cartonDetailInDb.Container == "Unknown")
                {
                    throw new Exception("Invalid contaier number. Container must be assigned first.");
                }

                if (index == 1)
                {
                    //当理论入库数量小于实际可用数量的时候，入库实际数量
                    var allocatedPcs = cartons * cartonDetailInDb.PcsPerCarton < cartonDetailInDb.ToBeAllocatedPcs ? cartons * cartonDetailInDb.PcsPerCarton : cartonDetailInDb.ToBeAllocatedPcs;
                    cartonDetailInDb.ToBeAllocatedCtns -= cartons;
                    cartonDetailInDb.ToBeAllocatedPcs -= allocatedPcs;

                    context.FCRegularLocationDetails.Add(new FCRegularLocationDetail
                    {
                        Container = cartonDetailInDb.POSummary.Container,
                        PurchaseOrder = cartonDetailInDb.PurchaseOrder,
                        Style = cartonDetailInDb.Style,
                        Color = cartonDetailInDb.Color,
                        CustomerCode = cartonDetailInDb.Customer,
                        SizeBundle = cartonDetailInDb.SizeBundle,
                        PcsBundle = cartonDetailInDb.PcsBundle,
                        Cartons = cartons,
                        Quantity = allocatedPcs,
                        Location = location,
                        PcsPerCaron = cartonDetailInDb.PcsPerCarton,
                        Status = "In Stock",
                        AvailableCtns = cartons,
                        PickingCtns = 0,
                        ShippedCtns = 0,
                        AvailablePcs = allocatedPcs,
                        PickingPcs = 0,
                        ShippedPcs = 0,
                        PreReceiveOrder = preReceiveOrderInDb,
                        RegularCaronDetail = cartonDetailInDb,
                        CartonRange = cartonRange,
                        Allocator = _userName,
                        Batch = cartonDetailInDb.Batch,
                        Vendor = cartonDetailInDb.Vendor
                    });

                    index++;
                }
                else
                {
                    cartonDetailInDb.ToBeAllocatedPcs -= cartons * cartonDetailInDb.PcsPerCarton;

                    context.FCRegularLocationDetails.Add(new FCRegularLocationDetail
                    {
                        Container = cartonDetailInDb.POSummary.Container,
                        PurchaseOrder = cartonDetailInDb.PurchaseOrder,
                        Style = cartonDetailInDb.Style,
                        Color = cartonDetailInDb.Color,
                        CustomerCode = cartonDetailInDb.Customer,
                        SizeBundle = cartonDetailInDb.SizeBundle,
                        PcsBundle = cartonDetailInDb.PcsBundle,
                        Cartons = 0,
                        Quantity = cartons * cartonDetailInDb.PcsPerCarton,
                        Location = location,
                        PcsPerCaron = cartonDetailInDb.PcsPerCarton,
                        Status = "In Stock",
                        AvailableCtns = 0,
                        PickingCtns = 0,
                        ShippedCtns = 0,
                        AvailablePcs = cartons * cartonDetailInDb.PcsPerCarton,
                        PickingPcs = 0,
                        ShippedPcs = 0,
                        PreReceiveOrder = preReceiveOrderInDb,
                        RegularCaronDetail = cartonDetailInDb,
                        CartonRange = cartonRange,
                        Allocator = _userName,
                        Batch = cartonDetailInDb.Batch,
                        Vendor = cartonDetailInDb.Vendor
                    });
                }

                //if (cartonDetailInDb.ToBeAllocatedCtns == 0)
                //{
                //    cartonDetailInDb.Status = "Allocated";
                //}

            }

            context.SaveChanges();
        }

        private int SumTotalCartons(IEnumerable<PreLocation> list)
        {
            var totalCtns = 0;

            foreach(var l in list)
            {
                totalCtns += l.Ctns * l.Plts;
            }

            return totalCtns;
        }
    }
}
