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

namespace ClothResorting.Helpers.FBAHelper
{
    public class FBAInventoryHelper
    {
        private ApplicationDbContext _context;
        private string _path = "";
        private _Application _excel;
        private Workbook _wb;
        private Worksheet _ws;

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

            var originalPlts = palletLocationsList.Sum(x => x.ActualPlts);

            //计算原有箱数减去每次发出的箱数并放到列表中
            foreach (var cartonLocation in cartonLocationsInDb)
            {
                var originalQuantity = cartonLocation.ActualQuantity;

                if (cartonLocation.Location == FBAStatus.InPallet)
                {
                    foreach(var pickCarton in cartonLocation.FBAPickDetailCartons)
                    {
                        cartonLocation.ActualQuantity -= pickCarton.PickCtns;
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
                        Type = cartonLocation.Location == FBAStatus.InPallet ? FBAStatus.InPallet : FBAStatus.LossCtn,
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
            info.OriginalLossCtns = residualInventoryList.Sum(x => x.OriginalQuantity);
            info.CurrentLossCtns = residualInventoryList.Sum(x => x.ResidualQuantity);
            info.TotalResidualCBM = residualInventoryList.Sum(x => x.ResidualCBM);
            info.TotalResidualQuantity = residualInventoryList.Sum(x => x.ResidualQuantity);
            info.CloseDate = closeDate;

            return info;
        }

        //读取库存报告模板并另存报告,返回完整储存路径
        public string GenerateFBAInventoryReport(FBAInventoryInfo info)
        {
            _ws = _wb.Worksheets[1];

            _ws.Cells[4, 2] = info.Customer;
            _ws.Cells[4, 4] = info.CloseDate.ToString("yyyy-MM-dd");

            _ws.Cells[6, 2] = info.OriginalPlts;
            _ws.Cells[6, 4] = info.CurrentPlts;
            _ws.Cells[6, 6] = info.OriginalLossCtns;
            _ws.Cells[6, 8] = info.CurrentLossCtns;

            var startRow = 9;

            foreach(var i in info.FBAResidualInventories)
            {
                _ws.Cells[startRow, 1] = i.Container;
                _ws.Cells[startRow, 2] = i.Type;
                _ws.Cells[startRow, 3] = i.ShipmentId;
                _ws.Cells[startRow, 4] = i.AmzRefId;
                _ws.Cells[startRow, 5] = i.WarehouseCode;
                _ws.Cells[startRow, 6] = i.GrossWeightPerCtn;
                _ws.Cells[startRow, 7] = i.CBMPerCtn;
                _ws.Cells[startRow, 8] = i.ResidualCBM;
                _ws.Cells[startRow, 9] = i.ResidualQuantity;
                _ws.Cells[startRow, 10] = i.Location;
            }

            var path = @"D:\InventoryReport\FBA- " + info.Customer + " -InventoryReport- " + DateTime.Now.ToString("yyyyMMddhhmmssffff") + ".xls";

            _wb.SaveAs(path, Type.Missing, "", "", Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange, 1, false, Type.Missing, Type.Missing, Type.Missing);

            _excel.Quit();

            return path;
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

        public int CurrentLossCtns { get; set; }

        public float TotalResidualCBM { get; set; }

        public float TotalResidualQuantity { get; set; }

        public DateTime CloseDate { get; set; }

        public List<FBAResidualInventory> FBAResidualInventories { get; set; }
    }
}