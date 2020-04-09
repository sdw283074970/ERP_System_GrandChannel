using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private CrossReferenceTransfer _transfer;

        public FCRegularLocationAllocatingController()
        {
            _context = new ApplicationDbContext();
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0] == "" ? (HttpContext.Current.Request.Headers.Get("AppUser") == null ? "" : HttpContext.Current.Request.Headers.Get("AppUser").Split('@')[0]) : HttpContext.Current.User.Identity.Name.Split('@')[0];
            _transfer = new CrossReferenceTransfer(_context);
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

        // GET /api/fcregularlocationallocating/?preId={preid}
        [HttpGet]
        public IHttpActionResult GetDetectedPermanentList([FromUri]int preId)
        {
            var detectedSKUs = _context.RegularCartonDetails
                .Include(x => x.POSummary.PreReceiveOrder)
                .Include(x => x.PermanentSKU)
                .Where(x => x.POSummary.PreReceiveOrder.Id == preId
                    && (x.ToPermanentCtns != 0 || x.ToPermanentPcs != 0));

            var detectedList = new List<DetectedSKU>();

            foreach(var d in detectedSKUs)
            {
                detectedList.Add(new DetectedSKU {
                    Id = d.Id,
                    OrgPo = d.PurchaseOrder,
                    OrgStyle = d.Style,
                    OrgColor = d.Color,
                    OrgSize = d.SizeBundle,
                    NewPo = d.PermanentSKU.PurchaseOrder,
                    NewStyle = d.PermanentSKU.Style,
                    NewColor = d.PermanentSKU.Color,
                    NewSize = d.PermanentSKU.Size,
                    ToPermanentCtns = d.ToPermanentCtns,
                    ToPermanentPcs = d.ToPermanentPcs,
                    Location = d.PermanentSKU.Location
                });
            }

            return Ok(detectedList);
        }

        // POST /api/fcregularlocationallocating/?container={container}&batch={batch}&po={po}&style={style}&color={color}&sku={sku}&size={size} 根据传入数据分解已收货对象，obj.id为cartondetail的id，cartondetail在此处用作记录待分配箱数及件数
        [HttpPost]
        public IHttpActionResult CreateRegularStock([FromBody]IEnumerable<FCRegularLocationAllocatingJsonObj> objArray, [FromUri]string container, [FromUri]string batch, [FromUri]string po, [FromUri]string style, [FromUri]string color, [FromUri]string sku, [FromUri]string size)
        {
            var preId = objArray.First().PreId;
            var cartonDetailsInDb = _context.RegularCartonDetails
                .Include(x => x.POSummary.PreReceiveOrder)
                .Where(x => x.POSummary.PreReceiveOrder.Id == preId);
            var list = new List<FCRegularLocationDetail>();

            foreach (var obj in objArray)
            {
                var detailInDb = cartonDetailsInDb.SingleOrDefault(x => x.Id == obj.Id);
                list.AddRange(CreateRegularLocation(_context, detailInDb, obj.Cartons, obj.Location));
            }

            _context.FCRegularLocationDetails.AddRange(list);
            _context.SaveChanges();

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
            var locationList = new List<FCRegularLocationDetail>();

            if (operation == "Apply")
            {
                var cartonDetailInDb = _context.RegularCartonDetails
                    .Include(x => x.POSummary.PreReceiveOrder)
                    .SingleOrDefault(x => x.Id == detailId);

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
                    locationList.AddRange(CreateRegularLocation(_context, cartonDetailInDb, l.Ctns * l.Plts, l.Location));
                }
            }

            _context.FCRegularLocationDetails.AddRange(locationList);
            _context.SaveChanges();

            return Created(Request.RequestUri, " ");
        }

        // POST /api/fcregularlocationallocating/?preId={preId}&operation={operation}
        [HttpPost]
        public IHttpActionResult QuickAllocating([FromUri]int preId, [FromUri]string operation)
        {
            var regularCartonsInDb = _context.RegularCartonDetails
                .Include(x => x.POSummary.PreReceiveOrder)
                .Where(x => x.POSummary.PreReceiveOrder.Id == preId)
                .Where(x => x.ToBeAllocatedPcs > 0 || x.ToBeAllocatedCtns > 0);

            var locationList = new List<FCRegularLocationDetail>();

            if (operation == "QuickAllocating")
            {
                foreach (var r in regularCartonsInDb)
                {
                    locationList.Add(CreateRegularLocationV2(r));
                }
            }
            else if (operation == "ApplyAll")
            {
                var appliableCartonDetails = regularCartonsInDb
                    .Where(x => x.ToBeAllocatedCtns == x.ActualCtns 
                        && x.ToBeAllocatedPcs == x.ActualPcs 
                        && (x.ActualPcs > 0 || x.ActualCtns > 0)
                        && !(x.PreLocation == "" || x.PreLocation == " " || x.PreLocation == null));

                var parser = new StringParser();

                foreach (var a in appliableCartonDetails)
                {
                    var list = parser.ParseStrToPreLoc(a.PreLocation);
                    var totalCtns = SumTotalCartons(list);

                    //如果pre-allocating string中的总箱数大于deatils的可分配箱数则跳过
                    if (totalCtns > a.ToBeAllocatedCtns)
                    {
                        continue;
                    }

                    //开始分配库位
                    foreach (var l in list)
                    {
                        locationList.AddRange(CreateRegularLocation(_context, a, l.Ctns * l.Plts, l.Location));
                    }
                }
            }

            if (locationList.Any())
            {
                _context.FCRegularLocationDetails.AddRange(locationList);
                _context.SaveChanges();
            }

            var result = Mapper.Map<IEnumerable<FCRegularLocationDetail>, IEnumerable<FCRegularLocationDetailDto>>(locationList);

            return Created(Request.RequestUri, result);
        }

        // PUT /api/fcregularlocationallocating/?preId={preId}
        [HttpPut]
        public void DetectAndAllocatePermanentSKUs([FromUri]int preId)
        {
            var vendor = _context.PreReceiveOrders.Find(preId).CustomerName;

            var cartonDetailsInDb = _context.RegularCartonDetails
                .Include(x => x.POSummary.PreReceiveOrder)
                .Where(x => x.POSummary.PreReceiveOrder.Id == preId && x.ToBeAllocatedPcs != 0);

            var permanentSKUsInDb = _context.PermanentSKUs.Where(x => x.Vendor == vendor);

            TransferRegularToPermanent(cartonDetailsInDb, permanentSKUsInDb);
        }

        private void TransferRegularToPermanent(IEnumerable<RegularCartonDetail> cartonDetailsInDb, IEnumerable<PermanentSKU> permanentSKUsInDb)
        {
            foreach(var c in cartonDetailsInDb)
            {
                var sku = FindPermanentSKU(c, permanentSKUsInDb);

                if (sku != null)
                {
                    c.PermanentSKU = sku;
                    c.ToPermanentCtns = c.ToBeAllocatedCtns;
                    c.ToPermanentPcs = c.ToBeAllocatedPcs;
                    c.ToBeAllocatedCtns = 0;
                    c.ToBeAllocatedPcs = 0;
                }
            }

            _context.SaveChanges();
        }

        private PermanentSKU FindPermanentSKU(RegularCartonDetail cartonDetail, IEnumerable<PermanentSKU> permanentSKUsInDb)
        {
            //cartonDetail.PurchaseOrder = _transfer.TransName("PurchaseOrder", cartonDetail.PurchaseOrder);
            //cartonDetail.Style = _transfer.TransName("Style", cartonDetail.Style);
            //cartonDetail.Color = _transfer.TransName("Color", cartonDetail.Color);
            //cartonDetail.SizeBundle = _transfer.TransName("Size", cartonDetail.SizeBundle);

            var po = _transfer.TransName("PurchaseOrder", cartonDetail.PurchaseOrder);
            var style = _transfer.TransName("Style", cartonDetail.Style);
            var color = _transfer.TransName("Color", cartonDetail.Color);
            var size = _transfer.TransName("Size", cartonDetail.SizeBundle);

            var sku = permanentSKUsInDb.SingleOrDefault(x => x.PurchaseOrder == po
                && x.Style == style
                && x.Color == color
                && x.Size == size
                && x.Status == Status.Active);

            return sku;
        }

        private IEnumerable<FCRegularLocationDetail> CreateRegularLocation(ApplicationDbContext context, RegularCartonDetail detailInDb, int cartons, string location)
        {
            var cartonRange = context.RegularCartonDetails
                .Include(c => c.POSummary.PreReceiveOrder)
                .SingleOrDefault(c => c.Id == detailInDb.Id)
                .CartonRange;

            var poSummaryId = context.RegularCartonDetails
                .Include(c => c.POSummary.PreReceiveOrder)
                .SingleOrDefault(c => c.Id == detailInDb.Id)
                .POSummary
                .Id;

            var inOneBoxSKUs = context.RegularCartonDetails
                .Include(c => c.POSummary.PreReceiveOrder)
                .Where(c => c.CartonRange == cartonRange
                    && c.POSummary.Id == poSummaryId
                    && c.Batch == detailInDb.Batch
                    && (c.Cartons != 0 || c.Quantity != 0));

            var list = new List<FCRegularLocationDetail>();

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

                    list.Add(new FCRegularLocationDetail
                    {
                        Container = cartonDetailInDb.POSummary.Container,
                        PurchaseOrder = cartonDetailInDb.PurchaseOrder,
                        Style = cartonDetailInDb.Style,
                        UPCNumber = cartonDetailInDb.UPCNumber,
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
                        PreReceiveOrder = detailInDb.POSummary.PreReceiveOrder,
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

                    list.Add(new FCRegularLocationDetail
                    {
                        Container = cartonDetailInDb.POSummary.Container,
                        PurchaseOrder = cartonDetailInDb.PurchaseOrder,
                        Style = cartonDetailInDb.Style,
                        Color = cartonDetailInDb.Color,
                        CustomerCode = cartonDetailInDb.Customer,
                        SizeBundle = cartonDetailInDb.SizeBundle,
                        PcsBundle = cartonDetailInDb.PcsBundle,
                        UPCNumber = cartonDetailInDb.UPCNumber,
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
                        PreReceiveOrder = detailInDb.POSummary.PreReceiveOrder,
                        RegularCaronDetail = cartonDetailInDb,
                        CartonRange = cartonRange,
                        Allocator = _userName,
                        Batch = cartonDetailInDb.Batch,
                        Vendor = cartonDetailInDb.Vendor
                    });
                }
            }

            return list;
        }

        private FCRegularLocationDetail CreateRegularLocationV2(RegularCartonDetail cartonDetailInDb)
        {
            var result = new FCRegularLocationDetail
            {
                Container = cartonDetailInDb.POSummary.Container,
                PurchaseOrder = cartonDetailInDb.PurchaseOrder,
                Style = cartonDetailInDb.Style,
                UPCNumber = cartonDetailInDb.UPCNumber,
                Color = cartonDetailInDb.Color,
                CustomerCode = cartonDetailInDb.Customer,
                SizeBundle = cartonDetailInDb.SizeBundle,
                PcsBundle = cartonDetailInDb.PcsBundle,
                Cartons = cartonDetailInDb.ToBeAllocatedCtns,
                Quantity = cartonDetailInDb.ToBeAllocatedPcs,
                Location = "FLOOR",
                PcsPerCaron = cartonDetailInDb.PcsPerCarton,
                Status = "In Stock",
                AvailableCtns = cartonDetailInDb.ToBeAllocatedCtns,
                PickingCtns = 0,
                ShippedCtns = 0,
                AvailablePcs = cartonDetailInDb.ToBeAllocatedPcs,
                PickingPcs = 0,
                ShippedPcs = 0,
                PreReceiveOrder = cartonDetailInDb.POSummary.PreReceiveOrder,
                RegularCaronDetail = cartonDetailInDb,
                CartonRange = cartonDetailInDb.CartonRange,
                Allocator = _userName,
                Batch = cartonDetailInDb.Batch,
                Vendor = cartonDetailInDb.Vendor
            };

            cartonDetailInDb.ToBeAllocatedCtns = 0;
            cartonDetailInDb.ToBeAllocatedPcs = 0;

            return result;
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

    public class DetectedSKU
    {
        public int Id { get; set; }

        public string OrgPo { get; set; }

        public string OrgStyle { get; set; }

        public string OrgColor { get; set; }

        public string OrgSize { get; set; }

        public string NewPo { get; set; }

        public string NewStyle { get; set; }

        public string NewColor { get; set; }

        public string NewSize { get; set; }

        public int ToPermanentCtns { get; set; }

        public int ToPermanentPcs { get; set; }

        public string Location { get; set; }
    }
}
