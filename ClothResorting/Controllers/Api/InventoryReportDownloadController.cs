using ClothResorting.Helpers;
using ClothResorting.Models;
using System;
using System.Collections.Generic;
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
        public void DownloadInventoryReport([FromUri]string vendor, [FromUri]string container, [FromUri]string purchaseOrder, [FromUri]string style, [FromUri]string color, [FromUri]string customer, [FromUri]string size, [FromUri]string location, [FromUri]bool isShipped, [FromUri]bool isReplenishment)
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

                generator.GenerateInventoryReportExcelFile(inventoryList, vendor);
            }
            else
            {
                var locationDetails = _context.FCRegularLocationDetails.Where(x => x.Vendor == vendor).ToList();

                if (container != "NULL")
                {
                    locationDetails = locationDetails.Where(x => x.Container == container).ToList();
                }

                if (purchaseOrder != "NULL")
                {
                    locationDetails = locationDetails.Where(x => x.PurchaseOrder == purchaseOrder).ToList();
                }

                if (style != "NULL")
                {
                    locationDetails = locationDetails.Where(x => x.Style == style).ToList();
                }

                if (color != "NULL")
                {
                    locationDetails = locationDetails.Where(x => x.Color == color).ToList();
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

                //将搜索结果中的相同项合并

                foreach (var loc in locationDetails)
                {
                    if (loc.AvailablePcs == 0)
                    {
                        continue;
                    }

                    var inventoryInReport = inventoryList.SingleOrDefault(x => x.PurchaseOrder == loc.PurchaseOrder
                        && x.Style == loc.Style
                        && x.Color == loc.Color
                        && x.SizeBundle == loc.SizeBundle
                        && x.Pack == loc.PcsBundle);

                    if (inventoryInReport == null)
                    {
                        inventoryList.Add(new InventoryReportDetail
                        {
                            Container = loc.Container,
                            Status = loc.Status,
                            PurchaseOrder = loc.PurchaseOrder,
                            Style = loc.Style,
                            Color = loc.Color,
                            SizeBundle = loc.SizeBundle,
                            Pack = loc.PcsBundle,
                            Cartons = loc.Cartons,
                            Quantity = loc.Quantity,
                            AvailableCtns = loc.AvailableCtns,
                            AvailablePcs = loc.AvailablePcs,
                            Batch = loc.Batch
                        });
                    }
                    else
                    {
                        inventoryInReport.Cartons += loc.Cartons;
                        inventoryInReport.Quantity += loc.Quantity;
                        inventoryInReport.AvailableCtns += loc.AvailableCtns;
                        inventoryInReport.AvailablePcs += loc.AvailablePcs;
                    }
                }

                generator.GenerateInventoryReportExcelFile(inventoryList, vendor);
            }
        }
    }
}
