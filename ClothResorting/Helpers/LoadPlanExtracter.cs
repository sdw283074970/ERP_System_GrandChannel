using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Office.Interop.Excel;
using ClothResorting.Models;
using ClothResorting.Models.DataTransferModels;
using System.Diagnostics;
using System.Data.Entity;

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
        #endregion

        //构造器
        public LoadPlanExtracter(string path)
        {
            _context = new ApplicationDbContext();
            _path = path;
            _excel = new Application();
            _wb = _excel.Workbooks.Open(_path);
        }

        //读取xlsx文件,分析内容并抽取PickRequest对象
        public IEnumerable<PickRequest> GetPickRequestsFromXlsx()
        {
            var PickRequestList = new List<PickRequest>();
            var sumOfWs = _wb.Worksheets.Count;

            for (int i = 1; i <= sumOfWs; i++)
            {
                _ws = _wb.Worksheets[i];
                int sumOfSize = (int)_ws.Cells[1, 4].Value2;

                for(int j = 0; j < sumOfSize; j++)
                {
                    PickRequestList.Add(new PickRequest
                    {
                        PurchaseOrder = _ws.Cells[1, 2].Value2.ToString(),
                        Style = _ws.Cells[2, 2].Value2.ToString(),
                        Color = _ws.Cells[3, 2].Value2.ToString(),
                        Size = _ws.Cells[4, 2 + j].Value2,
                        TargetPcs = (int)_ws.Cells[5, 2 + j].Value2
                    });
                }
            }
            return PickRequestList;
        }

        //输入pickRequests集合，经过算法，输出PermanentLocIORecord
        public IEnumerable<PermanentLocIORecord> OutputPermanentLocIORecord(IEnumerable<PickRequest> requests)
        {
            var records = new List<PermanentLocIORecord>();

            foreach(var request in requests)
            {
                var permanentLocInDb = _context.PermanentLocations
                    .Where(c => c.Id > 0)
                    .SingleOrDefault(c => c.PurchaseOrder == request.PurchaseOrder
                        && c.Style == request.Style
                        && c.Color == request.Color
                        && c.Size == request.Size);

                var targetPcs = request.TargetPcs;

                //当目标件数大于库存件数时，执行此循环
                while(targetPcs - permanentLocInDb.Quantity >= 0)
                {
                    //如果固定地点留存件数不为0，则全拿走，此条为出库记录
                    if (permanentLocInDb.Quantity != 0)
                    {
                        var record = new PermanentLocIORecord
                        {
                            PermanentLoc = permanentLocInDb.Location,
                            PurchaseOrder = request.PurchaseOrder,
                            Style = request.Style,
                            Color = request.Color,
                            Size = request.Size,
                            TargetPcs = targetPcs,
                            InvBefore = permanentLocInDb.Quantity,
                            InvChange = -permanentLocInDb.Quantity,
                            InvAfter = 0,
                            FromLocation = "",
                            TargetBalance = targetPcs - permanentLocInDb.Quantity,
                            OperationDate = timeNow,
                            PermanentLocation = permanentLocInDb
                        };

                        records.Add(record);
                        targetPcs -= permanentLocInDb.Quantity;
                        permanentLocInDb.Quantity = 0;
                    }
                    //如果固定地点留存件数为0，则先查找库存其他地方是否有余货，有则补货，没有则生成缺货记录。此条为补货/移库记录
                    else
                    {
                        var replenishments = _context.LocationDetails
                            .Include(c => c.PurchaseOrderSummary.PreReceiveOrder)
                            .Where(c => c.PurchaseOrder == request.PurchaseOrder
                            && c.Style == request.Style
                            && c.Color == request.Color
                            && c.Size == request.Size
                            && c.InvPcs != 0)
                            .OrderBy(c => c.InboundDate)    //先进先出
                            .ThenBy(c => c.Id).ToList();
                        
                        if (replenishments.Count != 0)
                        {
                            var replenishment = replenishments.First();
                            var record = new PermanentLocIORecord
                            {
                                PermanentLoc = permanentLocInDb.Location,
                                PurchaseOrder = request.PurchaseOrder,
                                Style = request.Style,
                                Color = request.Color,
                                Size = request.Size,
                                TargetPcs = targetPcs,
                                InvBefore = 0,
                                InvChange = replenishment.InvPcs,
                                InvAfter = replenishment.InvPcs,
                                FromLocation = replenishment.Location,
                                TargetBalance = targetPcs,
                                OperationDate = timeNow,
                                PermanentLocation = permanentLocInDb
                            };

                            records.Add(record);
                            replenishment.PurchaseOrderSummary.InventoryPcs -= replenishment.InvPcs;
                            replenishment.PurchaseOrderSummary.PreReceiveOrder.InvPcs -= replenishment.InvPcs;
                            //暂留 此处应该在该关联的PreReceiveOrder中减去相应的件数
                            permanentLocInDb.Quantity = replenishment.InvPcs;
                            replenishment.InvPcs = 0;

                            _context.SaveChanges();
                        }
                        else
                        {
                            var record = new PermanentLocIORecord
                            {
                                PermanentLoc = permanentLocInDb.Location,
                                PurchaseOrder = request.PurchaseOrder,
                                Style = request.Style,
                                Color = request.Color,
                                Size = request.Size,
                                TargetPcs = targetPcs,
                                InvBefore = 0,
                                InvChange = 0,
                                InvAfter = 0,
                                FromLocation = "Shortage",
                                TargetBalance = -targetPcs,
                                OperationDate = timeNow,
                                PermanentLocation = permanentLocInDb
                            };

                            records.Add(record);
                            break;
                        }
                    }
                }

                //当目标件数等于库存件数时，不操作，库存留空
                //当目标件数小于库存件数时
                if (targetPcs - permanentLocInDb.Quantity < 0)
                {
                    var record = new PermanentLocIORecord
                    {
                        PermanentLoc = permanentLocInDb.Location,
                        PurchaseOrder = request.PurchaseOrder,
                        Style = request.Style,
                        Color = request.Color,
                        Size = request.Size,
                        TargetPcs = targetPcs,
                        InvBefore = permanentLocInDb.Quantity,
                        InvChange = -targetPcs,
                        InvAfter = permanentLocInDb.Quantity - targetPcs,
                        FromLocation = "",
                        TargetBalance = 0,
                        OperationDate = timeNow,
                        PermanentLocation = permanentLocInDb
                    };

                    records.Add(record);
                }
            }

            _context.PermanentLocIORecord.AddRange(records);
            _context.SaveChanges();
            return records;
        }
    }
}