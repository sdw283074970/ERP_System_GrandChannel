using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Office.Interop.Excel;
using ClothResorting.Models;
using ClothResorting.Models.DataTransferModels;
using System.Diagnostics;
using System.Data.Entity;
using ClothResorting.Models.StaticClass;

namespace ClothResorting.Helpers
{
    public class LoadPlanExtracter
    {
        //全局变量
        #region
        private ApplicationDbContext _context;
        private string _path = "";
        private _Application _excel;
        private Workbook _wb;
        private Worksheet _ws;
        private DateTime timeNow = DateTime.Now;
        private string _userName;
        #endregion

        //构造器
        public LoadPlanExtracter(string path)
        {
            _context = new ApplicationDbContext();
            _path = path;
            _excel = new Application();
            _wb = _excel.Workbooks.Open(_path);
            _ws = _wb.Worksheets[1];
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0];

        }

        //读取xlsx文件,分析内容并抽取PickRequest对象
        public IEnumerable<PickRequest> GetPickRequestsFromXlsx()
        {
            var PickRequestList = new List<PickRequest>();
            var sumOfGroups = 0;
            var startRow = 1;
            var n = 1;

            //计算有多少groups
            while (n > 0)
            {
                var cpt_1 = _ws.Cells[n, 1].Value2;
                var cpt_2 = _ws.Cells[n + 1, 1].Value2;

                if (cpt_1 == null)
                {
                    sumOfGroups += 1;
                }

                if (cpt_1 == null && cpt_2 == null)
                {
                    break;
                }
                n += 1;
            }

            //遍历每一组，为每一组生成一个PickRequest对象放入pickRequestList中
            for (int i = 1; i <= sumOfGroups; i++)
            {
                //扫描每组有多少个size
                int sumOfSize = 0;
                int k = 0;

                while(_ws.Cells[startRow + 3 , 2 + k].Value != null)
                {
                    sumOfSize += 1;
                    k += 1;
                }

                for (int j = 0; j < sumOfSize; j++)
                {
                    PickRequestList.Add(new PickRequest
                    {
                        PurchaseOrder = _ws.Cells[startRow, 2].Value2 == null ? "" : _ws.Cells[startRow, 2].Value2.ToString(),
                        OrderPurchaseOrder = _ws.Cells[startRow, 6].Value2 == null ? "" : _ws.Cells[startRow, 6].Value2.ToString(),
                        Style = _ws.Cells[startRow + 1, 2].Value2 == null ? "" : _ws.Cells[startRow + 1, 2].Value2.ToString(),
                        Color = _ws.Cells[startRow + 2, 2].Value2 == null ? "" : _ws.Cells[startRow + 2, 2].Value2.ToString(),
                        Size = _ws.Cells[startRow + 3, 2 + j].Value2,
                        TargetPcs = (int)_ws.Cells[startRow + 4, 2 + j].Value2
                    });
                }

                startRow += 6;
            }

            return PickRequestList;
        }

        //抽取LoadPlan的新模板，执行Replenishment订单的出货方法
        public void PickReplenishmentLoadPlan(int shipOrderId)
        {
            var loadPlan = _context.ShipOrders.Find(shipOrderId);
            var pickingList = new List<PickDetail>();
            var skuCount = 0;
            var index = 1;
            var replenishmentInventoryInDb = _context.ReplenishmentLocationDetails
                .Where(x => x.AvailablePcs > 0);

            //扫描有多少种需要拣货的SKU
            while (_ws.Cells[index, 1].Value2 != null)
            {
                skuCount += 1;
                index += 3;
            }

            //为每一种SKU备货
            for (int i = 1; i <= skuCount; i++)
            {
                //为每一种SKU扫描需求的Size
                index = 4;      //size列的起始点
                var startRow = (i - 1) * 3 + 1;
                string style = _ws.Cells[startRow + 1, 1].Value2.ToString();
                string color = _ws.Cells[startRow + 1, 2].Value2.ToString();
                string purchaseOrder = _ws.Cells[startRow + 1, 3].Value2.ToString();
                var sizeCount = 0;
                var sizeList = new List<SizeRatio>();

                //扫描每一种SKU有多少种Size
                while (_ws.Cells[startRow, index].Value2 != null)
                {
                    sizeCount += 1;
                    index += 1;
                }

                //扫描每一种需求的Size名称和件数
                for (int j = 0; j < sizeCount; j++)
                {
                    sizeList.Add(new SizeRatio
                    {
                        SizeName = _ws.Cells[startRow, 4 + j].Value2.ToString(),
                        Count = (int)_ws.Cells[startRow + 1, 4 + j].Value2
                    });
                }

                //为该SKU中的每一种size备货
                foreach(var size in sizeList)
                {
                    //挑选出库存中所有符合条件的库存对象
                    var poolLocation = replenishmentInventoryInDb
                        .Include(x => x.PurchaseOrderInventory)
                        .Include(x => x.SpeciesInventory)
                        .Where(x => x.PurchaseOrder == purchaseOrder
                            && x.Style == style && x.Color == color && x.Size == size.SizeName)
                        .OrderByDescending(x => x.InboundDate);

                    var targetPcs = size.Count;

                    foreach(var location in poolLocation)
                    {
                        //如果当前库位储存量小于目标数量，则全部拿走，否则只拿走需要的
                        if (location.AvailablePcs < targetPcs)
                        {
                            location.PickingPcs += location.AvailablePcs;
                            targetPcs -= location.AvailablePcs;
                            pickingList.Add(new PickDetail
                            {
                                CartonRange = Status.NotAvailable,
                                PurchaseOrder = location.PurchaseOrder,
                                Style = location.Style,
                                Color = location.Color,
                                SizeBundle = location.Size,
                                PcsBundle = Status.NotAvailable,
                                CustomerCode = Status.NotAvailable,
                                PickDate = DateTime.Now.ToString("MM/dd/yyyy"),
                                Status = Status.Picking,
                                Container = Status.NotAvailable,
                                Location = location.Location,
                                PcsPerCarton = 0,
                                PickPcs = location.AvailablePcs,
                                PickCtns = 0,
                                ShipOrder = loadPlan,
                                LocationDetailId = location.Id
                            });

                            //调整purchaseOrderInvemtory和speciesInventory的库存数量、在拣数量
                            location.PurchaseOrderInventory.AvailablePcs -= location.AvailablePcs;
                            location.PurchaseOrderInventory.PickingPcs += location.AvailablePcs;

                            location.SpeciesInventory.AvailablePcs -= location.AvailablePcs;
                            location.SpeciesInventory.PickingPcs += location.AvailablePcs;

                            location.AvailablePcs = 0;
                        }
                        else
                        {
                            location.PickingPcs += targetPcs;
                            location.AvailablePcs -= targetPcs;
                            pickingList.Add(new PickDetail
                            {
                                CartonRange = Status.NotAvailable,
                                PurchaseOrder = location.PurchaseOrder,
                                Style = location.Style,
                                Color = location.Color,
                                SizeBundle = location.Size,
                                PcsBundle = Status.NotAvailable,
                                CustomerCode = Status.NotAvailable,
                                PickDate = DateTime.Now.ToString("MM/dd/yyyy"),
                                Status = Status.Picking,
                                Container = Status.NotAvailable,
                                Location = location.Location,
                                PcsPerCarton = 0,
                                PickPcs = targetPcs,
                                PickCtns = 0,
                                ShipOrder = loadPlan,
                                LocationDetailId = location.Id
                            });
                            //调整purchaseOrderInvemtory和speciesInventory的库存数量、在拣数量
                            location.PurchaseOrderInventory.AvailablePcs -= targetPcs;
                            location.PurchaseOrderInventory.PickingPcs += targetPcs;

                            location.SpeciesInventory.AvailablePcs -= targetPcs;
                            location.SpeciesInventory.PickingPcs += targetPcs;

                            targetPcs = 0;
                            break;      //当目标件数为0时，停止拿货
                        }
                    }

                    //如果所有现有库存都拿完了但是目标件数还没手机齐，则生成缺货记录
                    if (targetPcs > 0)
                    {
                        //生成缺货记录
                    }
                }
            }
            _context.PickDetails.AddRange(pickingList);
            _context.SaveChanges();
        }
    }
}