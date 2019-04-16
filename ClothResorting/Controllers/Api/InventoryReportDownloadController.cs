using ClothResorting.Helpers;
using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ClothResorting.Controllers.Api
{
    public class InventoryReportDownloadController : ApiController
    {
        private ApplicationDbContext _context;

        public InventoryReportDownloadController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/inventorysearch/?vendor={vendor}&container={container}&purchaseOrder={po}&style={style}&color={color}&customer={customer}&size={size}&location={location}&isShipped={isShipped}&isReplenishment={isReplenishment}
        [HttpGet]
        public IHttpActionResult DownloadInventoryReport([FromUri]string vendor, [FromUri]string container, [FromUri]string purchaseOrder, [FromUri]string style, [FromUri]string color, [FromUri]string customer, [FromUri]string size, [FromUri]string location, [FromUri]bool isShipped, [FromUri]bool isReplenishment)
        {
            var generator = new ExcelGenerator();
            var inventoryList = new List<InventoryReportDetail>();

            if (isReplenishment)
            {
                var replenishmentLocationDetails = _context.ReplenishmentLocationDetails.Where(x => x.Vendor == vendor).ToList();
                var replenishmentResult = new List<InventoryReportDetail>();

                if (purchaseOrder != "NULL")
                {
                    replenishmentLocationDetails = replenishmentLocationDetails.Where(x => x.PurchaseOrder == purchaseOrder).ToList();
                }

                if (style != "NULL")
                {
                    replenishmentLocationDetails = replenishmentLocationDetails.Where(x => x.Style == style).ToList();
                }

                if (color != "NULL")
                {
                    replenishmentLocationDetails = replenishmentLocationDetails.Where(x => x.Color == color).ToList();
                }

                if (size != "NULL")
                {
                    replenishmentLocationDetails = replenishmentLocationDetails.Where(x => x.Size == size).ToList();
                }

                if (location != "NULL")
                {
                    replenishmentLocationDetails = replenishmentLocationDetails.Where(x => x.Location == location).ToList();
                }

                if (!isShipped)
                {
                    replenishmentLocationDetails = replenishmentLocationDetails.Where(x => x.AvailablePcs != 0).ToList();
                }

                //将搜索结果中的相同项合并

                foreach (var rep in replenishmentLocationDetails)
                {
                    if (rep.AvailablePcs == 0)
                    {
                        continue;
                    }

                    var inventoryInReport = inventoryList.SingleOrDefault(x => x.PurchaseOrder == rep.PurchaseOrder
                        && x.Style == rep.Style
                        && x.Color == rep.Color
                        && x.SizeBundle == rep.Size);

                    if (inventoryInReport == null)
                    {
                        inventoryList.Add(new InventoryReportDetail
                        {
                            Status = rep.Status,
                            PurchaseOrder = rep.PurchaseOrder,
                            Style = rep.Style,
                            Color = rep.Color,
                            SizeBundle = rep.Size,
                            Quantity = rep.Quantity,
                            AvailablePcs = rep.AvailablePcs
                        });
                    }
                    else
                    {
                        inventoryInReport.Quantity += rep.Quantity;
                        inventoryInReport.AvailablePcs += rep.AvailablePcs;
                    }
                }

                var path = generator.GenerateInventoryReportExcelFile(inventoryList, vendor);

                return Ok(path);

            }
            else
            {
                var locationDetails = _context.FCRegularLocationDetails.Where(x => x.Vendor == vendor).ToList();

                if (container != "NULL")
                {
                    locationDetails = locationDetails.Where(x => x.Container.Contains(container)).ToList();
                }

                if (purchaseOrder != "NULL")
                {
                    locationDetails = locationDetails.Where(x => x.PurchaseOrder.Contains(purchaseOrder)).ToList();
                }

                if (style != "NULL")
                {
                    locationDetails = locationDetails.Where(x => x.Style.Contains(style)).ToList();
                }

                if (color != "NULL")
                {
                    locationDetails = locationDetails.Where(x => x.Color.Contains(color)).ToList();
                }

                if (customer != "NULL")
                {
                    locationDetails = locationDetails.Where(x => x.CustomerCode == customer).ToList();
                }

                if (size != "NULL")
                {
                    locationDetails = locationDetails.Where(x => x.SizeBundle == size).ToList();
                }

                if (location != "NULL")
                {
                    locationDetails = locationDetails.Where(x => x.Location == location).ToList();
                }

                if (!isShipped)
                {
                    locationDetails = locationDetails.Where(x => x.AvailablePcs != 0).ToList();
                }

                //按照收货时间出记录
                //var containerList = new List<string>();

                //var containerInDb = _context.Containers.Where(x => x.ReceivedDate != null && x.ReceivedDate != " ");

                //var timeLine = new DateTime(2019, 1, 1, 0, 0, 0, 0);

                //foreach (var c in containerInDb)
                //{
                //    DateTime receiveDate;

                //    if (c.ReceivedDate.Split('-').First().Length == 4)
                //    {
                //        DateTime.TryParseExact(c.ReceivedDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out receiveDate);
                //    }
                //    else
                //    {
                //        throw new Exception(c.ReceivedDate);
                //    }

                //    if (receiveDate < timeLine)
                //    {
                //        containerList.Add(c.ContainerNumber);
                //    }
                //}

                //var locationList = new List<FCRegularLocationDetail>();

                //foreach (var l in locationDetails)
                //{
                //    if (containerList.Contains(l.Container) && l.Container != "FONTANA TRANSFER SWIMWEAR" && l.Container != "FONTANA TRANSFER")
                //    {
                //        locationList.Add(l);
                //    }
                //}

                var templatePath = @"D:\Template\InventoryReportV2.xlsx";
                var generator2 = new ExcelGenerator(templatePath);
                var path = generator2.GenerateInventoryReportExcelFileV2(locationList, "");

                return Ok(path);
                //将搜索结果中的相同项合并

                //foreach (var loc in locationDetails)
                //{
                //    if (loc.AvailablePcs == 0)
                //    {
                //        continue;
                //    }

                //    var inventoryInReport = inventoryList.SingleOrDefault(x => x.PurchaseOrder == loc.PurchaseOrder
                //        && x.Style == loc.Style
                //        && x.Color == loc.Color
                //        && x.SizeBundle == loc.SizeBundle
                //        && x.Pack == loc.PcsBundle);

                //    if (inventoryInReport == null)
                //    {
                //        inventoryList.Add(new InventoryReportDetail
                //        {
                //            Container = loc.Container,
                //            Status = loc.Status,
                //            PurchaseOrder = loc.PurchaseOrder,
                //            Style = loc.Style,
                //            Color = loc.Color,
                //            SizeBundle = loc.SizeBundle,
                //            Pack = loc.PcsBundle,
                //            Cartons = loc.Cartons,
                //            Quantity = loc.Quantity,
                //            AvailableCtns = loc.AvailableCtns,
                //            AvailablePcs = loc.AvailablePcs,
                //            Batch = loc.Batch
                //        });
                //    }
                //    else
                //    {
                //        inventoryInReport.Cartons += loc.Cartons;
                //        inventoryInReport.Quantity += loc.Quantity;
                //        inventoryInReport.AvailableCtns += loc.AvailableCtns;
                //        inventoryInReport.AvailablePcs += loc.AvailablePcs;
                //    }
                //}

                //generator.GenerateInventoryReportExcelFile(inventoryList, vendor);
            }
        }


    }
}
