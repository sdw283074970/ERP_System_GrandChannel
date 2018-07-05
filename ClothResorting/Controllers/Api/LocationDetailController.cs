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
        private bool _isUnDoable = false;

        public LocationDetailController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/regularlocationdetail/?preid={id}&po={po}
        [HttpGet]
        public IHttpActionResult GetRegularLocationDetail([FromUri]PreIdPoJsonObj obj)
        {
            var result = new List<LocationDetail>();

            var query = _context.LocationDetails
                .Include(c => c.PurchaseOrderInventory)
                .Where(c => c.PurchaseOrder == obj.Po)
                .OrderByDescending(c => c.Id)
                .ToList();

            result.AddRange(query);

            var resultDto = Mapper.Map<List<LocationDetail>, List<LocationDetailDto>>(result);

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

            var resultDto = Mapper.Map<List<LocationDetail>, List<LocationDetailDto>>(result);

            //将批量导入的Species数据的箱数件数更新至purchaseOrderInventoryInDb中
            var purchaseOrderInventoryInDb = _context.PurchaseOrderInventories
                .Include(c => c.LocationDetails)
                .SingleOrDefault(c => c.PurchaseOrder == po);

            var sumOfCartons = result.Sum(c => c.OrgNumberOfCartons);
            var sumOfPcs = result.Sum(c => c.OrgPcs);

            purchaseOrderInventoryInDb.InvCtns += sumOfCartons;
            purchaseOrderInventoryInDb.InvPcs += sumOfPcs;

            _context.SaveChanges();

            //仅测试用
            var tester = new Tester();
            tester.CreatePermanentLocForEachSpecies(_context);

            //强行关闭进程
            var killer = new ExcelKiller();

            killer.Dispose();

            //标记以上整个操作为可撤销
            _isUnDoable = true;

            return Created(Request.RequestUri + "/" + 333, resultDto);
        }

        // DELETE /api/locationdetail
        [HttpDelete]
        public void Undo()
        {
            //如果为可撤销操作，则按照时间分组，在数据库中删除掉最新时间组的所有对象
            if (_isUnDoable == true)
            {
                var group = _context.LocationDetails
                    .GroupBy(c => c.InboundDate).ToList();

                var groupCount = group.Count;
                var count = group[groupCount - 1].Count();
                var result = _context.LocationDetails
                    .OrderByDescending(c => c.Id)
                    .Take(count);

                _isUnDoable = false;
                _context.LocationDetails.RemoveRange(result);
                _context.SaveChanges();
            }
        }
    }
}
