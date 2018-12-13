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

        // POST /api/pickdetail/?container={container}&customer={customer}&purchaseOrder={purchaseOrder}&style={style}&color={color}&size={size}
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

            _context.PickDetails.AddRange(pickList);
            _context.SaveChanges();
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


    }
}
