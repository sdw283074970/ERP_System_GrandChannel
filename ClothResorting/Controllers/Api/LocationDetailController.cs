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
using AutoMapper;
using System.Web;
using ClothResorting.Helpers;

namespace ClothResorting.Controllers.Api
{
    public class LocationDetailController : ApiController
    {
        private ApplicationDbContext _context;
        private DbSynchronizer _sync;

        public LocationDetailController()
        {
            _context = new ApplicationDbContext();
            _sync = new DbSynchronizer();
        }

        // GET /api/regularlocationdetail/?preid={id}&po={po}
        [HttpGet]
        public IHttpActionResult GetRegularLocationDetail([FromUri]PreIdPoJsonObj obj)
        {
            var result = new List<ReplenishmentLocationDetail>();

            var query = _context.LocationDetails
                .Include(c => c.PurchaseOrderInventory)
                .Where(c => c.PurchaseOrder == obj.Po)
                .OrderByDescending(c => c.Id)
                .ToList();

            result.AddRange(query);

            var resultDto = Mapper.Map<List<ReplenishmentLocationDetail>, List<ReplenishmentLocationDetailDto>>(result);

            return Ok(resultDto);
        }

        // POST /api/regularlocationdetail/?po={po}
        [HttpPost]
        public IHttpActionResult CreateRegularLocationDetails([FromUri]string po)
        {
            var fileSavePath = "";

            //从httpRequest中获取文件并写入磁盘系统
            var filesGetter = new FilesGetter();

            fileSavePath = filesGetter.GetAndSaveFileFromHttpRequest(@"D:\TempFiles\");

            if(fileSavePath == "")
            {
                return BadRequest();
            }

            //从上传的文件中抽取LocationDetails
            var excel = new ExcelExtracter(fileSavePath);

            excel.ExtractReplenishimentLocationDetail(po);

            //EF无法准确通过datetime查询对象，只能通过按inbound时间分组获取对象
            var group = _context.LocationDetails
                .GroupBy(c => c.InboundDate).ToList();

            var groupCount = group.Count;

            var count = group[groupCount - 1].Count();

            var result = _context.LocationDetails
                .OrderByDescending(c => c.Id)
                .Take(count)
                .ToList();

            var resultDto = Mapper.Map<List<ReplenishmentLocationDetail>, List<ReplenishmentLocationDetailDto>>(result);

            //同步刷新批量导入的LocationDetail所对应purchaseOrderInventory中的pcs件数
            var purchaseOrderInventoryInDb = _context.PurchaseOrderInventories
                .SingleOrDefault(c => c.PurchaseOrder == po);

            _sync.SyncPurchaseOrderInventory(purchaseOrderInventoryInDb.Id);

            _context.SaveChanges();

            //仅测试用
            var tester = new Tester();
            tester.CreatePermanentLocForEachSpecies(_context);

            //强行关闭进程
            var killer = new ExcelKiller();

            killer.Dispose();

            //标记以上整个操作为可撤销
            GlobalVariable.IsUndoable = true;

            return Created(Request.RequestUri + "/" + 333, resultDto);
        }

        // DELETE /api/locationdetail
        [HttpDelete]
        public void Undo([FromUri]string po)
        {
            //如果为可撤销操作，则按照时间分组，在数据库中删除掉最新时间组的所有对象
            if (GlobalVariable.IsUndoable == true)
            {
                var group = _context.LocationDetails
                    .GroupBy(c => c.InboundDate).ToList();

                var groupCount = group.Count;
                var groupInDb = group[groupCount - 1];
                var count = group[groupCount - 1].Count();
                var results = _context.LocationDetails
                    .OrderByDescending(c => c.Id)
                    .Take(count);

                GlobalVariable.IsUndoable = false;      //全局静态变量，用于储存是否允许Undo操作

                _context.LocationDetails.RemoveRange(results);
                _context.SaveChanges();

                //撤销操作后重新同步各个收到UNDO操作影响的species的件数
                foreach(var result in results)
                {
                    var speciesId = _context.SpeciesInventories
                        .Single(c => c.PurchaseOrder == result.PurchaseOrder
                            && c.Style == result.Style
                            && c.Color == result.Color
                            && c.Size == result.Size)
                        .Id;

                    _sync.SyncSpeciesInvenory(speciesId);
                }

                //撤销操作后重新同步PurchaseInventory的件数
                var purchaseOrderInventoryInDb = _context.PurchaseOrderInventories
                    .SingleOrDefault(c => c.PurchaseOrder == po);

                _sync.SyncPurchaseOrderInventory(purchaseOrderInventoryInDb.Id);
            }
            else
            {
                throw new HttpUnhandledException();
            }
        }
    }
}
