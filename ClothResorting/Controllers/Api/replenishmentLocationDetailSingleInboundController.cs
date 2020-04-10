using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Models;
using ClothResorting.Models.ApiTransformModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using ClothResorting.Helpers;
using ClothResorting.Models.StaticClass;
using System.Web;

namespace ClothResorting.Controllers.Api
{
    public class ReplenishmentLocationDetailSingleInboundController : ApiController
    {
        //        private ApplicationDbContext _context;
        //        private DateTime _timeNow = DateTime.Now;
        //        private DbSynchronizer _sync;
        //        private string _userName;

        //        public ReplenishmentLocationDetailSingleInboundController()
        //        {
        //            _context = new ApplicationDbContext();
        //            _sync = new DbSynchronizer();
        //            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0] == "" ? (HttpContext.Current.Request.Headers.Get("AppUser") == null ? "" : HttpContext.Current.Request.Headers.Get("AppUser")) : HttpContext.Current.User.Identity.Name.Split('@')[0];
        //        }

        //        // POST /api/ReplenishmentLocationDetailSingleInbound/
        //        [HttpPost]
        //        public IHttpActionResult CreateSingleInboundRecord([FromBody]SingleInboundJsonObj obj)
        //        {
        //            var purchaseOrderInventoryInDb = _context.PurchaseOrderInventories
        //                .SingleOrDefault(c => c.PurchaseOrder == obj.PurchaseOrder);

        //            if (purchaseOrderInventoryInDb == null)
        //            {
        //                _context.PurchaseOrderInventories.Add(new PurchaseOrderInventory {
        //                    OrderType = OrderType.Replenishment,
        //                    PurchaseOrder = obj.PurchaseOrder,
        //                    AvailableCtns = 0,
        //                    AvailablePcs = 0,
        //                    PickingPcs = 0,
        //                    ShippedPcs = 0,
        //                    Vender = Vendor.SilkIcon
        //                });
        //                _context.SaveChanges();

        //                purchaseOrderInventoryInDb = _context.PurchaseOrderInventories
        //                    .SingleOrDefault(c => c.PurchaseOrder == obj.PurchaseOrder);
        //            }

        //            var speciesInDbs = _context.SpeciesInventories.Where(c => c.Id > 0);

        //            var record = new ReplenishmentLocationDetail {
        //                PurchaseOrder = obj.PurchaseOrder,
        //                Style = obj.Style,
        //                Color = obj.Color,
        //                Size = obj.Size,
        //                InboundDate = _timeNow,
        //                Quantity = obj.Quantity,
        //                AvailablePcs = obj.Quantity,
        //                Location = obj.Location,
        //                PickingCtns = 0,
        //                PickingPcs = 0,
        //                ShippedCtns = 0,
        //                ShippedPcs = 0,
        //                Operator = _userName,
        //                PurchaseOrderInventory = purchaseOrderInventoryInDb
        //            };

        //            //检查speciesInDbToList中是否有这种类型的记录。有则直接在种类的基础上加pcs数量
        //            var speciesInDb = speciesInDbs
        //                .SingleOrDefault(c => c.PurchaseOrder == record.PurchaseOrder
        //                    && c.Style == record.Style
        //                    && c.Color == record.Color
        //                    && c.Size == record.Size);

        //            //如果没有则说明这是新种类，添加进数据库
        //            if (speciesInDb == null)
        //            {
        //                _context.SpeciesInventories.Add(new SpeciesInventory {
        //                    PurchaseOrder = obj.PurchaseOrder,
        //                    Style = obj.Style,
        //                    Color = obj.Color,
        //                    Size = obj.Size,
        //                    AdjPcs = obj.Quantity,
        //                    OrgPcs = obj.Quantity,
        //                    AvailablePcs = obj.Quantity,
        //                    PurchaseOrderInventory = purchaseOrderInventoryInDb
        //                });

        //                purchaseOrderInventoryInDb.AvailablePcs += obj.Quantity;
        //                _context.ReplenishmentLocationDetails.Add(record);
        //                _context.SaveChanges();
        //            }
        //            else//如果有，则调整该种类的pcs数据
        //            {
        //                //入库操作时使用同步器反而会增加数据库读写次数
        //                //_sync.SyncSpeciesInvenory(speciesInDb.Id);
        //                //_sync.SyncPurchaseOrderInventory(purchaseOrderInventoryInDb.Id);

        //                purchaseOrderInventoryInDb.AvailablePcs += obj.Quantity;

        //                speciesInDb.OrgPcs += obj.Quantity;
        //                speciesInDb.AdjPcs += obj.Quantity;
        //                speciesInDb.AvailablePcs += obj.Quantity;

        //                _context.ReplenishmentLocationDetails.Add(record);
        //                _context.SaveChanges();
        //            }

        //            GlobalVariable.IsUndoable = true;

        //            var sample = _context.ReplenishmentLocationDetails
        //                .OrderByDescending(c => c.Id)
        //                .First();

        //            //每添加一次单条inbound记录，返回所有结果，实现局部刷新表格
        //            var result = new List<ReplenishmentLocationDetail>();

        //            var query = _context.ReplenishmentLocationDetails
        //                .Include(c => c.PurchaseOrderInventory)
        //                .Where(c => c.PurchaseOrder == obj.PurchaseOrder)
        //                .OrderByDescending(c => c.Id)
        //                .ToList();

        //            result.AddRange(query);

        //            var resultDto = Mapper.Map<List<ReplenishmentLocationDetail>, List<ReplenishmentLocationDetailDto>>(result);

        //            return Created(Request.RequestUri + "/" + sample.Id, resultDto);
        //        }
    }
}
