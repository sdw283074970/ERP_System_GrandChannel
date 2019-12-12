using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Data.Entity;
using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Helpers;
using ClothResorting.Models.StaticClass;

namespace ClothResorting.Controllers.Api
{
    public class PickDetailController : ApiController
    {
        private ApplicationDbContext _context;

        public PickDetailController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/pickdetail/{id}(shipOrderId)
        [HttpGet]
        public IHttpActionResult GetAllPickDetail([FromUri]int id)
        {
            return Ok(_context.PickDetails
                .Include(x => x.ShipOrder)
                .Where(x => x.ShipOrder.Id == id)
                .Select(Mapper.Map<PickDetail, PickDetailDto>));
        }

        // GET /api/pickdetail/?orderType={orderType}
        [HttpGet]
        public IHttpActionResult DownloadPullSheetTemplate([FromUri]string orderType)
        {
            var downloader = new Downloader();

            if (orderType == OrderType.Regular)
            {
                downloader.DownloadFromServer("RegularPullSheet-Template.xlsx", @"D:\Template\");
            }
            else if (orderType == OrderType.Replenishment)
            {
                downloader.DownloadFromServer("ReplenishmentLoadPlan-Template.xlsx", @"D:\Template\");
            }
            return Ok();
        }

        // GET /api/pickdetail/?container={container}&customer={customer}&purchaseOrder={purchaseOrder}&style={style}&color={color}&size={size}
        [HttpGet]
        public IHttpActionResult GetRegularInventoryTable([FromUri]string container, [FromUri]string customer, [FromUri]string purchaseOrder, [FromUri]string style, [FromUri]string color, [FromUri]string size)
        {
            var regularLocationInDb = _context.FCRegularLocationDetails
                .Include(x => x.RegularCaronDetail.POSummary.PreReceiveOrder.UpperVendor)
                .Where(x => x.RegularCaronDetail.POSummary.PreReceiveOrder.UpperVendor.Name == customer)
                .ToList();

            if (container != null)
            {
                regularLocationInDb = regularLocationInDb.Where(x => x.Container.Contains(container)).ToList();
            }

            if (purchaseOrder != null)

            {
                regularLocationInDb = regularLocationInDb.Where(x => x.PurchaseOrder.Contains(purchaseOrder)).ToList();
            }

            if (style != null)
            {
                regularLocationInDb = regularLocationInDb.Where(x => x.Style.Contains(style)).ToList();
            }

            if (color != null)
            {
                regularLocationInDb = regularLocationInDb.Where(x => x.Color.Contains(color)).ToList();
            }

            if (size != null)
            {
                regularLocationInDb = regularLocationInDb.Where(x => x.SizeBundle.Contains(size)).ToList();
            }

            var result = Mapper.Map<IEnumerable<FCRegularLocationDetail>, IEnumerable<FCRegularLocationDetailDto>>(regularLocationInDb);

            return Ok(result);
        }

        // GET /api/pickdetail/?customer={customer}&purchaseOrder={purchaseOrder}&style={style}&color={color}&size={size}
        [HttpGet]
        public IHttpActionResult GetActivePermanentInventoryTable([FromUri]string customer, [FromUri]string purchaseOrder, [FromUri]string style, [FromUri]string color, [FromUri]string size)
        {
            var permanentSKUsInDb = _context.PermanentSKUs
                .Include(x => x.InboundLogs)
                .Include(x => x.OutboundLogs.Select(c => c.ShipOrder))
                .Where(x => x.Status == Status.Active);
                //.Select(Mapper.Map<PermanentSKU, PermanentSKUDto>);

            if (customer != null)
            {
                permanentSKUsInDb = permanentSKUsInDb.Where(x => x.Vendor.Contains(customer));
            }

            if (purchaseOrder != null)

            {
                permanentSKUsInDb = permanentSKUsInDb.Where(x => x.PurchaseOrder.Contains(purchaseOrder));
            }

            if (style != null)
            {
                permanentSKUsInDb = permanentSKUsInDb.Where(x => x.Style.Contains(style));
            }

            if (color != null)
            {
                permanentSKUsInDb = permanentSKUsInDb.Where(x => x.Color.Contains(color));
            }

            if (size != null)
            {
                permanentSKUsInDb = permanentSKUsInDb.Where(x => x.Size.Contains(size));
            }

            foreach (var r in permanentSKUsInDb)
            {
                r.Quantity = r.InboundLogs.Count() == 0 ? 0 : r.InboundLogs.Sum(x => x.ToPermanentPcs);
                r.ShippedPcs = r.OutboundLogs.Where(x => x.ShipOrder.Status == Status.Shipped).Count() == 0 ? 0 : r.OutboundLogs.Where(x => x.ShipOrder.Status == Status.Shipped).Sum(x => x.PickPcs);
                r.PickingPcs = r.OutboundLogs.Where(x => x.ShipOrder.Status != Status.Shipped).Count() == 0 ? 0 : r.OutboundLogs.Where(x => x.ShipOrder.Status != Status.Shipped).Sum(x => x.PickPcs);
                r.AvailablePcs = r.Quantity - r.ShippedPcs - r.PickingPcs;
            }

            var result = Mapper.Map<IEnumerable<PermanentSKU>, IEnumerable<PermanentSKUDto>>(permanentSKUsInDb);

            return Ok(result);
        }

        // POST /api/pickdetail/?shipOrderId={shipOrderId}&container={container}&customer={customer}&purchaseOrder={purchaseOrder}&style={style}&color={color}&size={size}
        [HttpPost]
        public void PickAllMatchedItemsByConditions([FromUri]int shipOrderId, [FromUri]string container, [FromUri]string customer, [FromUri]string purchaseOrder, [FromUri]string style, [FromUri]string color, [FromUri]string size)
        {
            var locationsInDb = _context.FCRegularLocationDetails.Where(x => x.AvailablePcs > 0 || x.AvailableCtns > 0);
            var shipOrderInDb = _context.ShipOrders
                .Include(x => x.PickDetails)
                .SingleOrDefault(x => x.Id == shipOrderId);
            var pickList = new List<PickDetail>();
            var orgCount = shipOrderInDb.PickDetails.Count();

            if (container != "NULL")
            {
                locationsInDb = locationsInDb.Where(x => x.Container == container);
            }

            //if (customer != "NULL")
            //{
            //    locationsInDb = locationsInDb.Where(x => x.CustomerCode == customer);
            //}

            if (purchaseOrder != "NULL")
            {
                locationsInDb = locationsInDb.Where(x => x.PurchaseOrder == purchaseOrder);
            }

            if (style != "NULL")
            {
                locationsInDb = locationsInDb.Where(x => x.Style == style);
            }

            if (color != "NULL")
            {
                locationsInDb = locationsInDb.Where(x => x.Color == color);
            }

            if (size != "NULL")
            {
                locationsInDb = locationsInDb.Where(x => x.SizeBundle == size);
            }

            //调出前1000条记录
            locationsInDb = locationsInDb.Take(1000);

            //将筛选后的所有对象出货
            foreach(var location in locationsInDb)
            {
                location.Status = Status.Picking;
                location.PickingCtns += location.AvailableCtns;
                location.PickingPcs += location.AvailablePcs;


                pickList.Add(new PickDetail {
                    Container = location.Container,
                    PurchaseOrder = location.PurchaseOrder,
                    Style = location.Style,
                    Color = location.Color,
                    UPCNumber = location.UPCNumber,
                    CustomerCode = location.CustomerCode,
                    SizeBundle = location.SizeBundle,
                    PcsBundle = location.PcsBundle,
                    PcsPerCarton = location.PcsPerCaron,
                    PickCtns = location.AvailableCtns,
                    PickPcs = location.AvailablePcs,
                    Location = location.Location,
                    Status = Status.Picking,
                    LocationDetailId = location.Id,
                    PickDate = DateTime.Now.ToString("MM/dd/yyyy"),
                    Memo = "Batch pick",
                    CartonRange =location.CartonRange,
                    ShipOrder = shipOrderInDb,
                    FCRegularLocationDetail = location
                });

                location.AvailableCtns = 0;
                location.AvailablePcs = 0;
            }

            shipOrderInDb.Status = Status.Picking;

            if (orgCount + pickList.Count() > 1000)
            {
                throw new Exception("Pick failed. The maximum capicity of pick details in one ship order is 1000. Original count before this pick: " + orgCount + ", after pick count: " + (orgCount + pickList.Count()));
            }

            _context.PickDetails.AddRange(pickList);
            _context.SaveChanges();
        }

        // POST /api/pickdetail/?shipOrderId={shipOrderId}&orderType={orderType}
        [HttpPost]
        public IHttpActionResult PickItemsByCondition([FromUri]int shipOrderId, [FromUri]string orderType, [FromBody]IEnumerable<PickInfo> objArray)
        {
            var shipOrderInDb = _context.ShipOrders.Find(shipOrderId);

            if(orderType == OrderType.Permanent)
            {
                var result = PickPermanentItem(_context, shipOrderInDb, objArray);

                shipOrderInDb.Status = Status.Picking;
                _context.SaveChanges();

                return Created(Request.RequestUri, result);
            }
            else if (orderType == OrderType.Regular)
            {
                var result = PickRegularItem(_context, shipOrderInDb, objArray);

                shipOrderInDb.Status = Status.Picking;
                _context.SaveChanges();

                return Created(Request.RequestUri, result);
            }

            _context.SaveChanges();

            return Ok();
        }

        // POST /api/pickdetail/{id}(shipOrderId)
        [HttpPost]
        public void ExtractPullSheetExcel([FromUri]int id)
        {
            var fileSavePath = "";

            //方法1：写入磁盘系统
            //从httpRequest中获取文件并写入磁盘系统
            var filesGetter = new FilesGetter();

            fileSavePath = filesGetter.GetAndSaveSingleFileFromHttpRequest(@"D:\TempFiles\");

            if (fileSavePath == "")
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            var excel = new ExcelExtracter(fileSavePath);

            excel.ExtractPullSheet(id);
        }

        // PUT /api/pickdetail/?shipOrderId={shipOrderId}&pickingMan={pickingMan}&pickDate={pickDate}
        [HttpPut]
        public void UpdatePickingInfo([FromUri]int shipOrderId, [FromUri]string pickingMan, [FromUri]string pickDate)
        {
            var shipOrderInDb = _context.ShipOrders.Find(shipOrderId);

            shipOrderInDb.PickingMan = pickingMan;
            shipOrderInDb.PickDate = pickDate;

            _context.SaveChanges();
        }

        // DELETE /api/pickdetail/?pickDetailId={pickDetailId}&putBackCtns={putBackCtns}&putBackPcs={putBackPcs}&orderType={orderType}
        [HttpDelete]
        public void RemovePickDetail([FromUri]int pickDetailId, [FromUri]int putBackCtns, [FromUri]int putBackPcs, [FromUri]string orderType)
        {
            if (orderType == OrderType.Regular)
            {
                var pickDetailInDb = _context.PickDetails
                    .Include(x => x.FCRegularLocationDetail)
                    .SingleOrDefault(x => x.Id == pickDetailId);

                pickDetailInDb.FCRegularLocationDetail.AvailableCtns += putBackCtns;
                pickDetailInDb.FCRegularLocationDetail.PickingCtns -= putBackCtns;

                pickDetailInDb.FCRegularLocationDetail.AvailablePcs += putBackPcs;
                pickDetailInDb.FCRegularLocationDetail.PickingPcs -= putBackPcs;

                //刷新被放回地点状态
                pickDetailInDb.Status = RefreshRegularStatus(pickDetailInDb.FCRegularLocationDetail);

                //如果放回的箱数件数刚好等于拣货的箱数件数，那么移除拣货记录
                if (putBackCtns == pickDetailInDb.PickCtns && putBackPcs == pickDetailInDb.PickPcs)
                {
                    _context.PickDetails.Remove(pickDetailInDb);
                }
                //否则仍然保留拣货记录
                else
                {
                    pickDetailInDb.PickCtns -= putBackCtns;
                    pickDetailInDb.PickPcs -= putBackPcs;
                }

                _context.SaveChanges();
            }
            else if (orderType == OrderType.Replenishment)
            {
                var pickDetailInDb = _context.PickDetails.Include(x => x.ReplenishmentLocationDetail).SingleOrDefault(x => x.Id == pickDetailId);

                pickDetailInDb.ReplenishmentLocationDetail.AvailablePcs += putBackPcs;
                pickDetailInDb.ReplenishmentLocationDetail.PickingPcs -= putBackPcs;

                //刷新被放回地点状态
                pickDetailInDb.Status = RefreshReplenishmentStatus(pickDetailInDb.ReplenishmentLocationDetail);

                //如果放回的件数刚好等于拣货的件数，那么移除拣货记录
                if (putBackPcs == pickDetailInDb.PickPcs)
                {
                    _context.PickDetails.Remove(pickDetailInDb);
                }
                //否则仍然保留拣货记录
                else
                {
                    pickDetailInDb.PickPcs -= putBackPcs;
                }

                _context.SaveChanges();
            }
            else if (orderType == OrderType.Permanent)
            {
                var pickDetailInDb = _context.PickDetails
                    .Include(x => x.PermanentSKU)
                    .SingleOrDefault(x => x.Id == pickDetailId);

                //如果放回的件数刚好等于拣货的件数，那么移除拣货记录
                if (putBackPcs == pickDetailInDb.PickPcs)
                {
                    _context.PickDetails.Remove(pickDetailInDb);
                }
                //否则仍然保留拣货记录
                else
                {
                    pickDetailInDb.PickPcs -= putBackPcs;
                }

                _context.SaveChanges();
            }
        }

        private string RefreshRegularStatus(FCRegularLocationDetail location)
        {
            if (location.PickingCtns != 0 || location.PickingPcs != 0)
            {
                return Status.Picking;
            }
            else if (location.PickingCtns == 0 && location.PickingPcs == 0 && location.AvailableCtns == 0 && location.AvailablePcs == 0)
            {
                return Status.Shipped;
            }
            else
            {
                return Status.InStock;
            }
        }

        private string RefreshReplenishmentStatus(ReplenishmentLocationDetail location)
        {
            if (location.PickingPcs != 0)
            {
                return Status.Picking;
            }
            else if (location.PickingPcs == 0 && location.AvailablePcs == 0)
            {
                return Status.Shipped;
            }
            else
            {
                return Status.InStock;
            }
        }

        private IEnumerable<PickDetailDto> PickPermanentItem(ApplicationDbContext context, ShipOrder shipOrderInDb, IEnumerable<PickInfo> objArray)
        {
            var permanentSKUsInDb = context.PermanentSKUs
                .Where(x => x.Status == Status.Active);
            var pickDetailList = new List<PickDetail>();

            foreach (var o in objArray)
            {
                var sku = permanentSKUsInDb.SingleOrDefault(x => x.Id == o.LocId);

                pickDetailList.Add(new PickDetail
                {
                    CartonRange = "NA",
                    Container = "NA",
                    PurchaseOrder = sku.PurchaseOrder,
                    Style = sku.Style,
                    Color = sku.Color,
                    CustomerCode = "NA",
                    SizeBundle = sku.Size,
                    PcsBundle = "NA",
                    PcsPerCarton = 0,
                    PickCtns = 0,
                    PickPcs = o.Pcs,
                    Location = sku.Location,
                    PickDate = DateTime.Now.ToString("MM/dd/yyyy"),
                    PermanentSKU = sku,
                    ShipOrder = shipOrderInDb
                });
            }

            context.PickDetails.AddRange(pickDetailList);
            context.SaveChanges();

            return Mapper.Map<IEnumerable<PickDetail>, IEnumerable<PickDetailDto>>(pickDetailList);
        }

        private IEnumerable<PickDetailDto> PickRegularItem(ApplicationDbContext context, ShipOrder shipOrderInDb, IEnumerable<PickInfo> objArray)
        {
            var regularItemsInDb = context.FCRegularLocationDetails
                .Where(x => x.AvailableCtns != 0 || x.AvailablePcs != 0);

            var pickDetailList = new List<PickDetail>();

            foreach (var o in objArray)
            {
                var pickDetailInList = pickDetailList.SingleOrDefault(x => x.Id == o.Id);
                var item = regularItemsInDb.SingleOrDefault(x => x.Id == o.Id);

                item.AvailableCtns -= o.Ctns;
                item.PickingCtns += o.Ctns;
                item.AvailablePcs -= o.Pcs;
                item.PickingPcs += o.Pcs;

                if (pickDetailInList == null)
                {
                    pickDetailList.Add(new PickDetail
                    {
                        Id = o.Id,
                        CartonRange = item.CartonRange,
                        Container = item.Container,
                        PurchaseOrder = item.PurchaseOrder,
                        Style = item.Style,
                        Color = item.Color,
                        UPCNumber = item.UPCNumber,
                        CustomerCode = item.CustomerCode,
                        SizeBundle = item.SizeBundle,
                        PcsBundle = item.PcsBundle,
                        PcsPerCarton = item.PcsPerCaron,
                        PickCtns = o.Ctns,
                        PickPcs = o.Pcs,
                        Location = item.Location,
                        PickDate = DateTime.Now.ToString("MM/dd/yyyy"),
                        FCRegularLocationDetail = item,
                        ShipOrder = shipOrderInDb
                    });
                }
                else
                {
                    pickDetailInList.PickPcs += o.Pcs;
                    pickDetailInList.PickCtns += o.Ctns;
                }
            }

            context.PickDetails.AddRange(pickDetailList);
            context.SaveChanges();

            return Mapper.Map<IEnumerable<PickDetail>, IEnumerable<PickDetailDto>>(pickDetailList);
        }
    }

    public class PickInfo
    {
        public int LocId { get; set; }

        public int Id { get; set; }

        public int Pcs { get; set; }

        public int Ctns { get; set; }
    }
}
