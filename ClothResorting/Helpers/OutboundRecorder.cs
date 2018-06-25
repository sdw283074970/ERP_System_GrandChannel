using ClothResorting.Models;
using ClothResorting.Models.DataTransferModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace ClothResorting.Helpers
{
    public class OutboundRecorder
    {
        private ApplicationDbContext _context;
        private DateTime _timeNow = DateTime.Now;

        public OutboundRecorder()
        {
            _context = new ApplicationDbContext();
        }

        //输入pickRequests集合，经过算法，输出PermanentLocIORecord
        public IEnumerable<PermanentLocIORecord> OutputReplenishmentOrderIORecord(IEnumerable<PickRequest> requests)
        {
            var records = new List<PermanentLocIORecord>();

            foreach (var request in requests)
            {
                var permanentLocInDb = _context.PermanentLocations
                    .Where(c => c.Id > 0)
                    .SingleOrDefault(c => c.PurchaseOrder == request.PurchaseOrder
                        && c.Style == request.Style
                        && c.Color == request.Color
                        && c.Size == request.Size);

                var targetPcs = request.TargetPcs;

                //当目标件数大于库存件数时，执行此循环
                while (targetPcs - permanentLocInDb.Quantity >= 0)
                {
                    //如果固定地点留存件数不为0，则全拿走，此条为出库记录
                    if (permanentLocInDb.Quantity != 0)
                    {
                        var record = new PermanentLocIORecord
                        {
                            PermanentLoc = permanentLocInDb.Location,
                            PurchaseOrder = request.PurchaseOrder,
                            OrderPurchaseOrder = request.OrderPurchaseOrder,
                            Style = request.Style,
                            Color = request.Color,
                            Size = request.Size,
                            TargetPcs = targetPcs,
                            InvBefore = permanentLocInDb.Quantity,
                            InvChange = -permanentLocInDb.Quantity,
                            InvAfter = 0,
                            FromLocation = "",
                            TargetBalance = targetPcs - permanentLocInDb.Quantity,
                            OperationDate = _timeNow,
                            PermanentLocation = permanentLocInDb
                        };

                        records.Add(record);
                        targetPcs -= permanentLocInDb.Quantity;
                        permanentLocInDb.Quantity = 0;

                        //如果永久库存存量为0且targetpcs也为0，则跳出循环
                        if (permanentLocInDb.Quantity == targetPcs)
                        {
                            break;
                        }
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
                            .ThenBy(c => c.Id)
                            .ToList();

                        //如果备选库存地点数量不为0，则调货，生成移库记录
                        if (replenishments.Count != 0)
                        {
                            var replenishment = replenishments.First();
                            var record = new PermanentLocIORecord
                            {
                                PermanentLoc = permanentLocInDb.Location,
                                PurchaseOrder = request.PurchaseOrder,
                                OrderPurchaseOrder = request.OrderPurchaseOrder,
                                Style = request.Style,
                                Color = request.Color,
                                Size = request.Size,
                                TargetPcs = targetPcs,
                                InvBefore = 0,
                                InvChange = replenishment.InvPcs,
                                InvAfter = replenishment.InvPcs,
                                FromLocation = replenishment.Location,
                                TargetBalance = targetPcs,
                                OperationDate = _timeNow,
                                PermanentLocation = permanentLocInDb
                            };

                            records.Add(record);
                            replenishment.PurchaseOrderSummary.InventoryPcs -= replenishment.InvPcs;
                            replenishment.PurchaseOrderSummary.PreReceiveOrder.InvPcs -= replenishment.InvPcs;

                            permanentLocInDb.Quantity = replenishment.InvPcs;
                            replenishment.InvPcs = 0;

                            _context.SaveChanges();
                        }
                        //否则，生成缺货记录
                        else
                        {
                            var record = new PermanentLocIORecord
                            {
                                PermanentLoc = permanentLocInDb.Location,
                                PurchaseOrder = request.PurchaseOrder,
                                OrderPurchaseOrder = request.OrderPurchaseOrder,
                                Style = request.Style,
                                Color = request.Color,
                                Size = request.Size,
                                TargetPcs = targetPcs,
                                InvBefore = 0,
                                InvChange = 0,
                                InvAfter = 0,
                                FromLocation = "Shortage",
                                TargetBalance = -targetPcs,
                                OperationDate = _timeNow,
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
                        OrderPurchaseOrder = request.OrderPurchaseOrder,
                        Style = request.Style,
                        Color = request.Color,
                        Size = request.Size,
                        TargetPcs = targetPcs,
                        InvBefore = permanentLocInDb.Quantity,
                        InvChange = -targetPcs,
                        InvAfter = permanentLocInDb.Quantity - targetPcs,
                        FromLocation = "",
                        TargetBalance = 0,
                        OperationDate = _timeNow,
                        PermanentLocation = permanentLocInDb
                    };

                    permanentLocInDb.Quantity -= targetPcs;
                    targetPcs = 0;
                    records.Add(record);
                }

                //当最后永久库位留存件数未0时，搜寻整个库存可用记录，如有可用，则调取移库。此条为移库记录
                if (targetPcs == 0 && permanentLocInDb.Quantity == 0)
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
                            OrderPurchaseOrder = request.OrderPurchaseOrder,
                            Style = request.Style,
                            Color = request.Color,
                            Size = request.Size,
                            TargetPcs = targetPcs,
                            InvBefore = 0,
                            InvChange = replenishment.InvPcs,
                            InvAfter = replenishment.InvPcs,
                            FromLocation = replenishment.Location,
                            TargetBalance = targetPcs,
                            OperationDate = _timeNow,
                            PermanentLocation = permanentLocInDb
                        };

                        records.Add(record);
                        replenishment.PurchaseOrderSummary.InventoryPcs -= replenishment.InvPcs;
                        replenishment.PurchaseOrderSummary.PreReceiveOrder.InvPcs -= replenishment.InvPcs;

                        permanentLocInDb.Quantity = replenishment.InvPcs;
                        replenishment.InvPcs = 0;

                        _context.SaveChanges();
                    }
                }
            }

            _context.PermanentLocIORecord.AddRange(records);
            _context.SaveChanges();
            return records;
        }
    }
}