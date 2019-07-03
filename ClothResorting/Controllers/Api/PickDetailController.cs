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

        // GET /api/pickdetail/?customer={customer}&purchaseOrder={purchaseOrder}&style={style}&color={color}&size={size}
        [HttpGet]
        public IHttpActionResult GetActivePermanentTable([FromUri]string customer, [FromUri]string purchaseOrder, [FromUri]string style, [FromUri]string color, [FromUri]string size)
        {
            var permanentSKUsInDb = _context.PermanentSKUs
                .Include(x => x.InboundLogs)
                .Include(x => x.OutboundLogs.Select(c => c.ShipOrder))
                .Where(x => x.Status == Status.Active);
                //.Select(Mapper.Map<PermanentSKU, PermanentSKUDto>);

            if (customer != null)
            {
                permanentSKUsInDb = permanentSKUsInDb.Where(x => x.Vendor == customer);
            }

            if (purchaseOrder != null)

            {
                permanentSKUsInDb = permanentSKUsInDb.Where(x => x.PurchaseOrder == purchaseOrder);
            }

            if (style != null)
            {
                permanentSKUsInDb = permanentSKUsInDb.Where(x => x.Style == style);
            }

            if (color != null)
            {
                permanentSKUsInDb = permanentSKUsInDb.Where(x => x.Color == color);
            }

            if (size != null)
            {
                permanentSKUsInDb = permanentSKUsInDb.Where(x => x.Size == size);
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
        public void PickByConditions([FromUri]int shipOrderId, [FromUri]string container, [FromUri]string customer, [FromUri]string purchaseOrder, [FromUri]string style, [FromUri]string color, [FromUri]string size)
        {
            var locationsInDb = _context.FCRegularLocationDetails.Where(x => x.AvailablePcs != 0);
            var shipOrderInDb = _context.ShipOrders.Find(shipOrderId);
            var pickList = new List<PickDetail>();

            if (container != "NULL")
            {
                locationsInDb = locationsInDb.Where(x => x.Container == container);
            }

            if (customer != "NULL")
            {
                locationsInDb = locationsInDb.Where(x => x.CustomerCode == customer);
            }

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

            _context.PickDetails.AddRange(pickList);
            _context.SaveChanges();
        }

        // POST /api/pickdetail/?shipOrderId={shipOrderId}
        [HttpPost]
        public IHttpActionResult PickPermanentSKUs([FromUri]int shipOrderId, [FromBody]IEnumerable<PermanentSKUPickInfo> objArray)
        {
            var shipOrderInDb = _context.ShipOrders.Find(shipOrderId);
            var permanentSKUsInDb = _context.PermanentSKUs
                .Where(x => x.Status == Status.Active);
            var pickDetailList = new List<PickDetail>();

            foreach(var o in objArray)
            {
                var sku = permanentSKUsInDb.SingleOrDefault(x => x.Id == o.LocId);

                pickDetailList.Add(new PickDetail {
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

            _context.PickDetails.AddRange(pickDetailList);
            _context.SaveChanges();

            var result = Mapper.Map<IEnumerable<PickDetail>, IEnumerable<PickDetailDto>>(pickDetailList);

            return Created(Request.RequestUri, result);
        }

        // POST /api/pickdetail/{id}(shipOrderId)
        [HttpPost]
        public void ExtractPullSheetExcel([FromUri]int id)
        {
            var fileSavePath = "";

            //方法1：写入磁盘系统
            //从httpRequest中获取文件并写入磁盘系统
            var filesGetter = new FilesGetter();

            fileSavePath = filesGetter.GetAndSaveFileFromHttpRequest(@"D:\TempFiles\");

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
    }

    public class PermanentSKUPickInfo
    {
        public int LocId { get; set; }

        public int Pcs { get; set; }
    }
}
