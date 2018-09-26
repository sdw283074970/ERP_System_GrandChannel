using ClothResorting.Models;
using ClothResorting.Models.DataTransferModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Web.Http;
using System.Net;

namespace ClothResorting.Helpers
{
    public class OutboundRecorder
    {
        private ApplicationDbContext _context;
        private DateTime _timeNow = DateTime.Now;
        private DbSynchronizer _sync;

        public OutboundRecorder()
        {
            _context = new ApplicationDbContext();
            _sync = new DbSynchronizer();
        }

        //输入pickRequests集合，经过算法，输出PermanentLocIORecord移库/出库记录
        public IEnumerable<PermanentLocIORecord> OutputReplenishmentOrderIORecord(IEnumerable<PickRequest> requests)
        {
            var records = new List<PermanentLocIORecord>();

            foreach (var request in requests)
            {
                //抓取货物之前必须先添加永久库位，有专门的用户页面可供手动添加，强依赖
                var permanentLocInDb = _context.PermanentLocations
                    .Where(c => c.Id > 0)
                    .SingleOrDefault(c => c.PurchaseOrder == request.PurchaseOrder
                        && c.Style == request.Style
                        && c.Color == request.Color
                        && c.Size == request.Size);

                //抓取货物之前必须添加货物种类到数据库，在入库时会自动添加，强依赖
                var speciesInDb = _context.SpeciesInventories
                    .SingleOrDefault(c => c.PurchaseOrder == request.PurchaseOrder
                        && c.Style == request.Style
                        && c.Color == request.Color
                        && c.Size == request.Size);

                //抓取货物之前必须有PO容器，有专门的用户页面可供手动添加，强依赖
                var purchaserOrderInventoryInDb = _context.PurchaseOrderInventories
                        .SingleOrDefault(c => c.PurchaseOrder == request.PurchaseOrder);

                var targetPcs = request.TargetPcs;

                //如果要抓取的货物在库存中没有PO记录、种类记录或永久库位记录，则生成缺货记录
                if (permanentLocInDb == null || speciesInDb == null || purchaserOrderInventoryInDb == null)
                {
                    records.Add(new PermanentLocIORecord {
                        PermanentLoc = "N/A",
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
                        TargetBalance = targetPcs,
                        OperationDate = _timeNow
                    });
                }
                else    //否则正常抓取
                {
                    //当目标件数大于等于库存件数时，执行此循环
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
                            //调整目标件数
                            targetPcs -= permanentLocInDb.Quantity;
                            //调整永久库位件数
                            permanentLocInDb.Quantity = 0;
                            //刷新种类件数统计数据
                            _sync.SyncSpeciesInvenory(speciesInDb.Id);
                            //刷新Po件数统计数据
                            _sync.SyncPurchaseOrderInventory(purchaserOrderInventoryInDb.Id);

                            //如果永久库存存量为0且targetpcs也为0，则跳出循环
                            if (permanentLocInDb.Quantity == targetPcs)
                            {
                                break;
                            }
                        }
                        //如果固定地点留存件数为0，则先查找库存其他地方是否有余货，有则补货，没有则生成缺货记录。此条为补货/移库记录
                        else
                        {
                            var replenishments = _context.ReplenishmentLocationDetails
                                .Include(c => c.PurchaseOrderInventory)
                                .Where(c => c.PurchaseOrder == request.PurchaseOrder
                                && c.Style == request.Style
                                && c.Color == request.Color
                                && c.Size == request.Size
                                && c.AvailablePcs != 0)
                                .OrderBy(c => c.InboundDate)    //先进先出
                                .ThenBy(c => c.Id).ToList();

                            //如果备选库存地点数量不为0，则调货，生成移库记录
                            if (replenishments.Count != 0)
                            {
                                var selfLocation = replenishments.SingleOrDefault(c => c.Location == permanentLocInDb.Location);

                                //首先判断永久库位本身是否被当作普通库位使用，如果有货，则直接原地转移库存
                                if (selfLocation != null)
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
                                        InvChange = selfLocation.AvailablePcs,
                                        InvAfter = selfLocation.AvailablePcs + permanentLocInDb.Quantity,
                                        FromLocation = selfLocation.Location,
                                        TargetBalance = targetPcs,
                                        OperationDate = _timeNow,
                                        PermanentLocation = permanentLocInDb
                                    };

                                    records.Add(record);

                                    //移库不造成件数总数的变化

                                    //调整永久库位件数
                                    permanentLocInDb.Quantity = selfLocation.AvailablePcs;
                                    //调整原库位的件数
                                    selfLocation.AvailablePcs = 0;

                                    _context.SaveChanges();
                                }
                                else    //如果有货永久库位没被当作普通库位，则在其他库位寻找商品
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
                                        InvBefore = permanentLocInDb.Quantity,
                                        InvChange = replenishment.AvailablePcs,
                                        InvAfter = replenishment.AvailablePcs + permanentLocInDb.Quantity,
                                        FromLocation = replenishment.Location,
                                        TargetBalance = targetPcs,
                                        OperationDate = _timeNow,
                                        PermanentLocation = permanentLocInDb
                                    };

                                    records.Add(record);

                                    //移库不造成件数总数的变化

                                    //调整永久库位件数
                                    permanentLocInDb.Quantity = replenishment.AvailablePcs;
                                    //调整原库位的件数
                                    replenishment.AvailablePcs = 0;

                                    _context.SaveChanges();
                                }
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

                        //调整永久库位剩余件数
                        permanentLocInDb.Quantity -= targetPcs;
                        //刷新种类件数统计数据
                        _sync.SyncSpeciesInvenory(speciesInDb.Id);
                        //刷新Po件数统计数据
                        _sync.SyncPurchaseOrderInventory(purchaserOrderInventoryInDb.Id);
                        //调整目标抓取件数
                        targetPcs = 0;

                        records.Add(record);
                    }

                    //当最后永久库位留存件数小于30件时，搜寻整个库存可用记录，如有可用，则调取移库。此条为移库记录
                    if (targetPcs == 0 && permanentLocInDb.Quantity < 30)
                    {
                        var replenishments = _context.ReplenishmentLocationDetails
                            .Include(c => c.PurchaseOrderInventory)
                            .Where(c => c.PurchaseOrder == request.PurchaseOrder
                            && c.Style == request.Style
                            && c.Color == request.Color
                            && c.Size == request.Size
                            && c.AvailablePcs != 0)
                            .OrderBy(c => c.InboundDate)    //先进先出
                            .ThenBy(c => c.Id).ToList();
                        if (replenishments.Count != 0)
                        {
                            var replenishmentFromOtherLoc = replenishments.First();
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
                                InvChange = replenishmentFromOtherLoc.AvailablePcs,
                                InvAfter = replenishmentFromOtherLoc.AvailablePcs + permanentLocInDb.Quantity,
                                FromLocation = replenishmentFromOtherLoc.Location,
                                TargetBalance = targetPcs,
                                OperationDate = _timeNow,
                                PermanentLocation = permanentLocInDb
                            };

                            records.Add(record);
                            //移库不造成库存件数的出库变化

                            //调整永久库位的件数
                            permanentLocInDb.Quantity += replenishmentFromOtherLoc.AvailablePcs;
                            //调整原库位的件数
                            replenishmentFromOtherLoc.AvailablePcs = 0;

                            _context.SaveChanges();
                        }
                    }
                }
            }

            _context.PermanentLocIORecord.AddRange(records);
            _context.SaveChanges();
            return records;
        }
    }
}