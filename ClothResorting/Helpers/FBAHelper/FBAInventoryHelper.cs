using ClothResorting.Models;
using System;
using System.Collections.Generic;
using Microsoft.Office.Interop.Excel;
using System.Linq;
using System.Web;
using System.Data.Entity;
using ClothResorting.Models.StaticClass;
using ClothResorting.Models.FBAModels.StaticModels;
using System.Globalization;
using System.IO;
using System.Threading;

namespace ClothResorting.Helpers.FBAHelper
{
    public class FBAInventoryHelper
    {
        private ApplicationDbContext _context;
        private string _path = "";
        private _Application _excel;
        private Workbook _wb;
        private Worksheet _ws;
        private delegate void QuitHandler();
        //private delegate void SaveAsHandler(object Filename, object FileFormat, object Password, object WriteResPassword, object ReadOnlyRecommended, object CreateBackup, XlSaveAsAccessMode AccessMode = XlSaveAsAccessMode.xlNoChange, object ConflictResolution = null, object AddToMru = null, object TextCodepage = null, object TextVisualLayout = null, object Local = null);

        public FBAInventoryHelper()
        {
            _context = new ApplicationDbContext();
        }

        public FBAInventoryHelper(string templatePath)
        {
            _context = new ApplicationDbContext();
            _path = templatePath;
            _excel = new Application();
            _wb = _excel.Workbooks.Open(_path);
        }

        //输入截止日期和客户代码，返回到截止日期时FBA的库存列表
        public FBAInventoryInfo GetFBAInventoryResidualInfo(string customerCode, DateTime closeDate)
        {
            var residualInventoryList = new List<FBAResidualInventory>();

            //获取在指定日期前已发出的拣货列表
            var pickDetailList = _context.FBAPickDetails
                .Include(x => x.FBAPickDetailCartons)
                .Include(x => x.FBAShipOrder)
                .Where(x => x.FBAShipOrder.ShipDate <= closeDate);

            //获取在指定日期之前入库的总箱数库存列表
            var cartonLocationsInDb = _context.FBACartonLocations
                .Include(x => x.FBAPallet.FBAPalletLocations)
                .Include(x => x.FBAOrderDetail.FBAMasterOrder.Customer)
                .Include(x => x.FBAPickDetailCartons)
                .Include(x => x.FBAPickDetails)
                .Where(x => x.FBAOrderDetail.FBAMasterOrder.InboundDate <= closeDate && x.FBAOrderDetail.FBAMasterOrder.Customer.CustomerCode == customerCode);

            //获取在指定日期前入库的托盘库存列表
            var palletLocationsInDb = _context.FBAPalletLocations
                .Include(x => x.FBAMasterOrder)
                .Include(x => x.FBAPickDetails)
                .Where(x => x.FBAMasterOrder.InboundDate <= closeDate && x.FBAMasterOrder.Customer.CustomerCode == customerCode);

            var palletLocationsList = palletLocationsInDb.ToList();
            var cartonLocationList = cartonLocationsInDb.ToList();

            var originalPlts = palletLocationsList.Sum(x => x.ActualPlts);
            var originalLossCtns = cartonLocationList.Where(x => x.Status == FBAStatus.InPallet).Sum(x => x.ActualQuantity);
            var currentLossCtns = originalLossCtns;

            //计算原有箱数减去每次发出的箱数并放到列表中
            foreach (var cartonLocation in cartonLocationsInDb)
            {
                var originalQuantity = cartonLocation.ActualQuantity;

                if (cartonLocation.Location == FBAStatus.InPallet)
                {
                    foreach(var pickCarton in cartonLocation.FBAPickDetailCartons)
                    {
                        cartonLocation.ActualQuantity -= pickCarton.PickCtns;
                        currentLossCtns -= pickCarton.PickCtns;
                    }
                }
                else
                {
                    foreach(var pickcarton in cartonLocation.FBAPickDetails)
                    {
                        cartonLocation.ActualQuantity -= pickcarton.ActualQuantity;
                    }
                }

                if (cartonLocation.ActualQuantity != 0)
                {
                    residualInventoryList.Add(new FBAResidualInventory {
                        Id = cartonLocation.Id,
                        Container = cartonLocation.Container,
                        Type = cartonLocation.Location == "Pallet" ? FBAStatus.InPallet : FBAStatus.LossCtn,
                        ShipmentId = cartonLocation.ShipmentId,
                        AmzRefId = cartonLocation.AmzRefId,
                        WarehouseCode = cartonLocation.WarehouseCode,
                        GrossWeightPerCtn = cartonLocation.GrossWeightPerCtn,
                        CBMPerCtn = cartonLocation.CBMPerCtn,
                        ResidualCBM = cartonLocation.CBMPerCtn * cartonLocation.ActualQuantity,
                        ResidualQuantity = cartonLocation.ActualQuantity,
                        OriginalQuantity = originalQuantity,
                        Location = cartonLocation.Location == FBAStatus.InPallet ? CombineLocation(cartonLocation.FBAPallet.FBAPalletLocations.Select(x => x.Location).ToList()) : cartonLocation.Location,
                    });
                }
            }

            //计算原有托盘数减去所有发出的托盘数
            foreach(var plt in palletLocationsInDb)
            {
                foreach(var pick in plt.FBAPickDetails)
                {
                    plt.ActualPlts -= pick.ActualPlts;
                }
            }

            var info = new FBAInventoryInfo();

            info.FBAResidualInventories = residualInventoryList;
            info.Customer = customerCode;

            info.OriginalPlts = originalPlts;
            info.CurrentPlts = palletLocationsList.Sum(x => x.ActualPlts);
            info.OriginalLossCtns = originalLossCtns;
            info.CurrentLooseCtns = currentLossCtns;

            info.TotalResidualCBM = residualInventoryList.Sum(x => x.ResidualCBM);
            info.TotalResidualQuantity = residualInventoryList.Sum(x => x.ResidualQuantity);
            info.CloseDate = closeDate;

            return info;
        }

        //读取库存报告模板并另存报告,返回完整储存路径
        public void GenerateFBAInventoryReport(FBAInventoryInfo info)
        {
            _ws = _wb.Worksheets[1];

            _ws.Cells[4, 2] = info.Customer;
            _ws.Cells[4, 4] = info.CloseDate.ToString("yyyy-MM-dd");

            _ws.Cells[6, 2] = info.CurrentPlts;
            _ws.Cells[6, 4] = info.TotalResidualQuantity;
            _ws.Cells[6, 6] = info.CurrentLooseCtns;
            _ws.Cells[6, 8] = info.TotalResidualQuantity - info.CurrentLooseCtns;

            var startRow = 9;

            foreach(var i in info.FBAResidualInventories)
            {
                _ws.Cells[startRow, 1] = i.Container;
                _ws.Cells[startRow, 2] = i.Type;
                _ws.Cells[startRow, 3] = i.ShipmentId;
                _ws.Cells[startRow, 4] = i.AmzRefId;
                _ws.Cells[startRow, 5] = i.WarehouseCode;
                _ws.Cells[startRow, 6] = Math.Round(i.GrossWeightPerCtn, 2);
                _ws.Cells[startRow, 7] = Math.Round(i.CBMPerCtn, 2);
                _ws.Cells[startRow, 8] = Math.Round(i.ResidualCBM, 2);
                _ws.Cells[startRow, 9] = Math.Round((double)i.ResidualQuantity, 2);
                _ws.Cells[startRow, 10] = i.Location;

                startRow += 1;
            }

            var fullPath = @"D:\InventoryReport\FBA- " + info.Customer + " -InventoryReport- " + DateTime.Now.ToString("yyyyMMddhhmmssffff") + ".xls";

            _wb.SaveAs(fullPath, Type.Missing, "", "", Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange, 1, false, Type.Missing, Type.Missing, Type.Missing);

            _excel.Quit();

            //使用主线程调用委托，等于用主线程调用Quick()方法，以达到阻塞线程的目的
            var handler = new QuitHandler(_excel.Quit);
            handler.Invoke();

            var response = HttpContext.Current.Response;
            var downloadFile = new FileInfo(fullPath);
            response.ClearHeaders();
            response.Buffer = false;
            response.ContentType = "application/octet-stream";
            response.AppendHeader("Content-Disposition", "attachment; filename=" + info.Customer + " Inventory Report - " + HttpUtility.UrlEncode(DateTime.Now.ToString("yyyyMMddhhmmss") + ".xls", System.Text.Encoding.UTF8));
            response.Clear();
            response.AppendHeader("Content-Length", downloadFile.Length.ToString());
            response.WriteFile(downloadFile.FullName);
            response.Flush();
            response.Close();
            response.End();

            var killer = new ExcelKiller();

            killer.Dispose();
        }

        //返回库存CBM不为0的FBA客户列表
        public IList<FBAInventoryInfo> ReturnNonZeroCBMInventoryInfo(string closeDate)
        {
            DateTime closeDateInDateTime;
            DateTime.TryParseExact(closeDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out closeDateInDateTime);

            var customerCodeList = _context.UpperVendors.Where(x => x.DepartmentCode == "FBA").Select(x => x.CustomerCode).ToList();
            var resultList = new List<FBAInventoryInfo>();

            foreach(var code in customerCodeList)
            {
                var info = GetFBAInventoryResidualInfo(code, closeDateInDateTime);
                if (info.FBAResidualInventories.Count != 0)
                {
                    resultList.Add(info);
                }
            }

            return resultList;
        }

        public string CombineLocation(IList<string> locationList)
        {
            var result = locationList.First();

            for(int i = 1; i < locationList.Count; i++)
            {
                result = result + "/" + locationList[i];
            }

            return result;
        }
    }

    public class FBAResidualInventory
    {
        public int Id { get; set; }

        public string Container { get; set; }

        public string Type { get; set; }

        public string ShipmentId { get; set; }

        public string AmzRefId { get; set; }

        public string WarehouseCode { get; set; }

        public float GrossWeightPerCtn { get; set; }

        public float CBMPerCtn { get; set; }

        public int OriginalQuantity { get; set; }

        public float ResidualCBM { get; set; }

        public int ResidualQuantity { get; set; }

        public string Location { get; set; }
    }

    public class FBAInventoryInfo
    {
        public string Customer { get; set; }

        public int OriginalPlts { get; set; }

        public int CurrentPlts { get; set; }

        public int OriginalLossCtns { get; set; }

        public int CurrentLooseCtns { get; set; }

        public float TotalResidualCBM { get; set; }

        public float TotalResidualQuantity { get; set; }

        public DateTime CloseDate { get; set; }

        public List<FBAResidualInventory> FBAResidualInventories { get; set; }
    }
}