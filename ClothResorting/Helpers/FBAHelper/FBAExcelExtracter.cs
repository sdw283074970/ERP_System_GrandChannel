using ClothResorting.Models;
using ClothResorting.Models.FBAModels;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using ClothResorting.Models.FBAModels.StaticModels;
using ClothResorting.Models.StaticClass;

namespace ClothResorting.Helpers.FBAHelper
{
    public class FBAExcelExtracter
    {
        //全局变量
        #region
        private ApplicationDbContext _context;
        private string _path = "";
        private _Application _excel;
        private Workbook _wb;
        private Worksheet _ws;
        private DateTime _dateTimeNow;
        private string _userName;
        #endregion

        //构造器
        #region
        public FBAExcelExtracter()
        {
            _context = new ApplicationDbContext();
            _dateTimeNow = DateTime.Now;
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0] == "" ? (HttpContext.Current.Request.Headers.Get("AppUser") == null ? "" : HttpContext.Current.Request.Headers.Get("AppUser")) : HttpContext.Current.User.Identity.Name.Split('@')[0];

        }

        public FBAExcelExtracter(string path)
        {
            _context = new ApplicationDbContext();
            _path = path;
            _dateTimeNow = DateTime.Now;
            _excel = new Application();
            _wb = _excel.Workbooks.Open(_path);
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0] == "" ? (HttpContext.Current.Request.Headers.Get("AppUser") == null ? "" : HttpContext.Current.Request.Headers.Get("AppUser")) : HttpContext.Current.User.Identity.Name.Split('@')[0];

        }
        #endregion

        //抽取FBA通用PackingList模板
        public List<FBAOrderDetail> ExtractFBAPackingListTemplate(string grandNumber, int masterOrderId)
        {
            var orderDetailsList = new List<FBAOrderDetail>();
            _ws = _wb.Worksheets[1];

            var masterOrderInDb = _context.FBAMasterOrders.SingleOrDefault(x => masterOrderId == 0 ? x.GrandNumber == grandNumber : x.Id == masterOrderId);

            var countOfOrderDetail = 0;
            var index = 2;

            var shipmentId = string.Empty;
            var amzRefId = string.Empty;
            var lotSize = string.Empty;
            var warehouseCode = string.Empty;
            var howToDeliver = string.Empty;
            var grossWeight = 0f;
            var cbm = 0f;
            var quantity = 0;
            var remark = string.Empty;

            //扫描有多少个OrderDetail对象(行数-1)
            while(_ws.Cells[index, 1].Value2 != null)
            {
                countOfOrderDetail++;
                index++;
            }

            for(int i = 0; i < countOfOrderDetail; i++)
            {
                try
                {
                    shipmentId = _ws.Cells[i + 2, 1].Value2.ToString();
                    amzRefId = _ws.Cells[i + 2, 2].Value2 == null ? "NA" : _ws.Cells[i + 2, 2].Value2.ToString();
                    lotSize = _ws.Cells[i + 2, 3].Value2.ToString().ToUpper();
                    //lotSize = _ws.Cells[i + 2, 3].Value2 == null ? "NA" : _ws.Cells[i + 2, 3].Value2.ToString();
                    warehouseCode = _ws.Cells[i + 2, 4].Value2 == null ? "NA" : _ws.Cells[i + 2, 4].Value2.ToString();
                    howToDeliver = _ws.Cells[i + 2, 5].Value2 == null ? "NA" : _ws.Cells[i + 2, 5].Value2.ToString();
                    grossWeight = _ws.Cells[i + 2, 6].Value2 == null ? 0 : (float)_ws.Cells[i + 2, 6].Value2;
                    cbm = _ws.Cells[i + 2, 7].Value2 == null ? 0 : (float)_ws.Cells[i + 2, 7].Value2;
                    quantity = (int)_ws.Cells[i + 2, 8].Value2;
                    remark = _ws.Cells[i + 2, 9].Value2 == null ? "NA" : _ws.Cells[i + 2, 9].Value2.ToString();
                }
                catch(Exception e)
                {
                    throw new Exception(e.Message);
                }

                var orderDetail = new FBAOrderDetail();

                orderDetail.AssembleFirstStringPart(shipmentId, amzRefId, warehouseCode);
                orderDetail.AssembleSecontStringPart(lotSize, howToDeliver, remark);
                orderDetail.AssembleNumberPart(grossWeight, cbm, quantity);

                orderDetail.Container = masterOrderInDb.Container == "" ? "NULL" : masterOrderInDb.Container;
                orderDetail.GrandNumber = masterOrderInDb.GrandNumber;
                orderDetail.FBAMasterOrder = masterOrderInDb;

                orderDetailsList.Add(orderDetail);
            }

            _context.FBAOrderDetails.AddRange(orderDetailsList);
            _context.SaveChanges();

            return orderDetailsList;
        }

        //抽取BOL模板
        public IList<FBABOLDetail> ExtractBOLTemplate()
        {
            var bolDetailList = new List<FBABOLDetail>();
            _ws = _wb.Worksheets[1];

            var count = 0;
            var index = 2;

            while(_ws.Cells[index, 1].Value2 != null)
            {
                count += 1;
                index += 1;
            }

            for(int i = 0; i < count; i++)
            {
                var bol = new FBABOLDetail {
                    CustomerOrderNumber = _ws.Cells[i + 2, 1].Value2.ToString(),
                    Contianer = _ws.Cells[i + 2, 2].Value2.ToString(),
                    CartonQuantity = (int)(_ws.Cells[i + 2, 3].Value2 ?? 0),
                    Weight = (float)(_ws.Cells[i + 2, 4].Value2 ?? 0),
                    ActualPallets = (int)(_ws.Cells[i + 2, 5].Value2 ?? 0),
                    Location = _ws.Cells[i + 2, 6].Value2 == null ? "NA" : _ws.Cells[i + 2, 6].ToString()
                };

                bolDetailList.Add(bol);
            }

            return bolDetailList;
        }

        //抽取FBA通用拣货模板
        public void ExtractFBAPickingListTemplate(int shipOrderId)
        {
            var shipOrderInDb = _context.FBAShipOrders.Find(shipOrderId);
            var pickingList = new List<FBAPickingItem>();
            var pickDetailList = new List<FBAPickDetail>();
            var diagnosticsList = new List<PullSheetDiagnostic>();
            var pickDetailCartonList = new List<FBAPickDetailCarton>();

            _ws = _wb.Worksheets[1];

            var countOfEntries = 0;
            
            while(_ws.Cells[2 + countOfEntries, 1].Value2 != null)
            {
                countOfEntries += 1;
            }

            for(int i = 0; i < countOfEntries; i++)
            {
                try
                {
                    pickingList.Add(new FBAPickingItem
                    {
                        CustomerCode = _ws.Cells[i + 2, 1].Value2.ToString(),
                        ProductSku = _ws.Cells[i + 2, 2].Value2.ToString(),
                        PickCtns = (int)_ws.Cells[i + 2, 3].Value2
                    });
                }
                catch(Exception e)
                {
                    throw new Exception("Check row " + (i + 1) + ". Please make sure there is no empty cell in this line.");
                }
            }

            var inventoryInDb = _context.FBACartonLocations
                .Include(x => x.FBAOrderDetail.FBAMasterOrder.Customer)
                .Include(x => x.FBAPallet.FBAPalletLocations)
                .Where(x => x.AvailableCtns > 0);

            foreach(var p in pickingList)
            {
                var cartonLocationsInDb = inventoryInDb
                    .Where(x => x.FBAOrderDetail.FBAMasterOrder.Customer.CustomerCode == p.CustomerCode
                        && x.ShipmentId == p.ProductSku);

                //库存缺失诊断
                if (cartonLocationsInDb.Count() == 0)
                {
                    diagnosticsList.Add(new PullSheetDiagnostic {
                        FBAShipOrder = shipOrderInDb,
                        DiagnosticDate = DateTime.Now.ToString("MM/dd/yyyy"),
                        Type = "Missing",
                        Description = "Missing detected. Please check SKU=<font color='red'>" + p.ProductSku + "</font> under Customer Code=<font color='red'>" + p.CustomerCode + "</font>"
                    });

                    continue;
                }

                var targetCtns = p.PickCtns;

                foreach(var c in cartonLocationsInDb)
                {
                    if (targetCtns == 0)
                    {
                        break;
                    }

                    if (c.AvailableCtns <= targetCtns && c.AvailableCtns != 0)
                    {
                        var pickDetail = CreateFBAPickDetail(c, c.AvailableCtns, pickDetailCartonList);
                        pickDetail.FBAShipOrder = shipOrderInDb;
                        pickDetailList.Add(pickDetail);
                        targetCtns -= c.AvailableCtns;
                        c.PickingCtns += c.AvailableCtns;
                        c.AvailableCtns = 0;
                    }
                    else if (c.AvailableCtns > targetCtns)
                    {
                        var pickDetail = CreateFBAPickDetail(c, targetCtns, pickDetailCartonList);
                        pickDetail.FBAShipOrder = shipOrderInDb;
                        pickDetailList.Add(pickDetail);
                        c.AvailableCtns -= targetCtns;
                        c.PickingCtns += targetCtns;
                        targetCtns = 0;
                    }
                }

                //缺货诊断
                if (targetCtns > 0)
                {
                    pickDetailList.Last().PickableCtns += targetCtns;

                    diagnosticsList.Add(new PullSheetDiagnostic
                    {
                        FBAShipOrder = shipOrderInDb,
                        Type = "Shortage",
                        DiagnosticDate = DateTime.Now.ToString("MM/dd/yyyy"),
                        Description = "Shortage detected. Please check SKU=<font color='red'>" + p.ProductSku + "</font>. Shortage Ctns:<font color='red'>" + targetCtns + "</font>. Collected Ctns:< font color = 'red' > " + (p.PickCtns - targetCtns) + " </font>."
                    });
                }
            }
            shipOrderInDb.Status = FBAStatus.Picking;

            if (diagnosticsList.Count != 0)
            {
                _context.PullSheetDiagnostics.AddRange(diagnosticsList);
            }

            _context.FBAPickDetails.AddRange(pickDetailList);
            _context.FBAPickDetailCartons.AddRange(pickDetailCartonList);
            _context.SaveChanges();

            AdjustRelationship();
        }

        private FBAPickDetail CreateFBAPickDetail(FBACartonLocation cartonLocation, int ctns, IList<FBAPickDetailCarton> pickDetailCartonList)
        {
            //如果不是放在托盘上的货，则直接建立pickdetail和cartondetail的关系
            if (cartonLocation.Status != "InPallet")
            {
                var location = CombineLocation(cartonLocation.FBAPallet.FBAPalletLocations.Select(x => x.Location).ToList());

                var pickDetail = new FBAPickDetail
                {
                    Location = location,
                    GrandNumber = cartonLocation.GrandNumber,
                    Container = cartonLocation.Container,
                    ShipmentId = cartonLocation.ShipmentId,
                    AmzRefId = cartonLocation.AmzRefId,
                    WarehouseCode = cartonLocation.WarehouseCode,
                    ActualCBM = cartonLocation.CBMPerCtn * ctns,
                    Size = " ",
                    ActualGrossWeight = cartonLocation.GrossWeightPerCtn * ctns,
                    ActualQuantity = ctns,
                    OrderType = FBAOrderType.Standard,
                    HowToDeliver = cartonLocation.HowToDeliver,
                    Status = FBAStatus.Picking,
                    PickableCtns = ctns,
                    FBACartonLocation = cartonLocation
                };

                pickDetailCartonList.Add(new FBAPickDetailCarton
                {
                    PickCtns = ctns,
                    FBAPickDetail = pickDetail,
                    FBACartonLocation = cartonLocation
                });

                return pickDetail;
            }
            //如果是托盘里的carton，先找到其托盘对象，再在托盘对象中拣货
            else
            {
                var palletLocation = cartonLocation.FBAPallet.FBAPalletLocations.First();
                var pickDetail = new FBAPickDetail
                {
                    Location = palletLocation.Location,
                    GrandNumber = palletLocation.GrandNumber,
                    Container = palletLocation.Container,
                    ShipmentId = palletLocation.ShipmentId,
                    AmzRefId = palletLocation.AmzRefId,
                    WarehouseCode = palletLocation.WarehouseCode,
                    ActualCBM = cartonLocation.CBMPerCtn * ctns,
                    Size = " ",
                    ActualGrossWeight = cartonLocation.GrossWeightPerCtn * ctns,
                    ActualQuantity = ctns,
                    OrderType = FBAOrderType.Standard,
                    HowToDeliver = palletLocation.HowToDeliver,
                    Status = FBAStatus.Picking,
                    PickableCtns = ctns,
                    FBAPalletLocation = palletLocation
                };

                pickDetailCartonList.Add(new FBAPickDetailCarton {
                    PickCtns = ctns,
                    FBAPickDetail = pickDetail,
                    FBACartonLocation = cartonLocation
                });

                return pickDetail;
            }
        }

        //重新调整Entiy对象关系。上面的批量取货只考虑了货为散箱库存。现在保留以上，修正托盘库存的对象与拣货列表的关系
        public void AdjustRelationship()
        {
            //找出需要调整的拣货对象
            var pickDetailsInDb = _context.FBAPickDetails
                .Include(x => x.FBACartonLocation.FBAPallet.FBAPalletLocations)
                .Include(x => x.FBAPickDetailCartons)
                .Where(x => x.FBACartonLocation.Status == "InPallet"
                    && x.FBAPickDetailCartons == null);

            var pickDetailCartonList = new List<FBAPickDetailCarton>();

            foreach(var p in pickDetailsInDb)
            {
                var newPickDeatilCarton = new FBAPickDetailCarton {
                    PickCtns = p.ActualQuantity,
                    FBAPickDetail = p,
                    FBACartonLocation = p.FBACartonLocation
                };

                pickDetailCartonList.Add(newPickDeatilCarton);

                p.FBAPalletLocation = p.FBACartonLocation.FBAPallet.FBAPalletLocations.First();
                p.FBACartonLocation = null;
            }

            _context.FBAPickDetailCartons.AddRange(pickDetailCartonList);
            _context.SaveChanges();
        }

        public string CombineLocation(IList<string> locationList)
        {
            if (locationList.Count != 0)
            {
                var result = locationList.First();

                for (int i = 1; i < locationList.Count; i++)
                {
                    result = result + "/" + locationList[i];
                }

                return result;
            }
            else
            {
                return "未分配库位";
            }

        }
    }

    public class FBAPickingItem
    {
        public string CustomerCode { get; set; }

        public string ProductSku { get; set; }

        public int PickCtns { get; set; }
    }
}