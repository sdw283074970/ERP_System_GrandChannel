using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using ClothResorting.Models;
using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Models.StaticClass;

namespace ClothResorting.Controllers.Api
{
    public class FCRegularLocationDetailController : ApiController
    {
        private ApplicationDbContext _context;

        public FCRegularLocationDetailController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/fcregularlocationdetail/?preId={preid}&container={container}&batch={batch}&po={po}&style={style}&color={color}&sku={sku}&size={size}
        public IHttpActionResult GetAllLocationDetail([FromUri]int preId, [FromUri]string container, [FromUri]string batch, [FromUri]string po, [FromUri]string style, [FromUri]string color, [FromUri]string sku, [FromUri]string size)
        {
            var resultDto = _context.FCRegularLocationDetails
                .Include(c => c.PreReceiveOrder)
                .Where(c => c.PreReceiveOrder.Id == preId)
                .ToList()
                .Select(Mapper.Map<FCRegularLocationDetail, FCRegularLocationDetailDto>);

            if (container != "NULL")
            {
                resultDto = resultDto.Where(x => x.Container == container);
            }

            if (batch != "NULL")
            {
                resultDto = resultDto.Where(x => x.Batch == batch);
            }

            if (po != "NULL")
            {
                resultDto = resultDto.Where(x => x.PurchaseOrder == po);
            }

            if (style != "NULL")
            {
                resultDto = resultDto.Where(x => x.Style == style);
            }

            if (color != "NULL")
            {
                resultDto = resultDto.Where(x => x.Color == color);
            }

            if (sku != "NULL")
            {
                resultDto = resultDto.Where(x => x.CustomerCode == sku);
            }

            if (size != "NULL")
            {
                resultDto = resultDto.Where(x => x.SizeBundle == size);
            }

            return Ok(resultDto);
        }

        // DELETE /api/FCRegularLocationDetail/{id} 删除库存记录，将记录的箱数件数移回SKU待分配
        [HttpDelete]
        public void RelocateLocation([FromUri]int id)
        {
            RelocateSingleId(id, _context);

            _context.SaveChanges();
        }

        // DELETE /api/FcRegularlocationDetail/ 批量删除记录, 将删除的记录移回SKU待分配
        [HttpDelete]
        public void RelocatedSelectedId([FromBody]int[] arr)
        {
            foreach(var id in arr)
            {
                RelocateSingleId(id, _context);
            }

            _context.SaveChanges();
        }

        //对单个Id对象移库的方法
        private void RelocateSingleId(int id, ApplicationDbContext context)
        {
            var locationInDb = context.FCRegularLocationDetails
                .Include(x => x.PreReceiveOrder)
                .Include(x => x.RegularCaronDetail)
                .SingleOrDefault(x => x.Id == id);

            //检查当前移库对象是否有正在拣货的寄生对象，如果有则抛出异常
            var parasitcItemsInDb = context.FCRegularLocationDetails
                .Where(x => x.Container == locationInDb.Container
                    && x.CartonRange == locationInDb.CartonRange
                    && x.Batch == locationInDb.Batch);

            foreach(var item in parasitcItemsInDb)
            {
                if (item.PickingPcs != 0)
                {
                    throw new Exception("Cannot relocate item PO:" + item.PurchaseOrder + " Style=:" + item.Style + " Color:" + item.Color + " Size:" + item.SizeBundle + ". Because certain items under carton range: " + item.CartonRange + " Batch:" + item.Batch + " is in picking.");
                }
            }

            var preId = locationInDb.PreReceiveOrder.Id;

            //首先将宿主箱返回到待分配状态
            locationInDb.RegularCaronDetail.ToBeAllocatedCtns += locationInDb.AvailableCtns;
            locationInDb.RegularCaronDetail.ToBeAllocatedPcs += locationInDb.AvailablePcs;
            locationInDb.RegularCaronDetail.Status = "Reallocating";

            locationInDb.AvailablePcs = 0;
            locationInDb.AvailableCtns = 0;
            locationInDb.Status = "Reallocated";

            if (locationInDb.ShippedPcs == 0)
            {
                context.FCRegularLocationDetails.Remove(locationInDb);
            }

            //然后找到所有寄生箱对象，即找到在同一箱的其他size库存(range相同且可用箱数为0的对象)
            var locationsInDb = context.FCRegularLocationDetails
                .Include(x => x.RegularCaronDetail.POSummary.PreReceiveOrder)
                .Where(x => x.RegularCaronDetail.POSummary.PreReceiveOrder.Id == preId
                    && x.Batch == locationInDb.Batch
                    && x.CartonRange == locationInDb.CartonRange
                    && x.Cartons == 0);

            foreach (var location in locationsInDb)
            {
                var cartonDetailInDb = location.RegularCaronDetail;

                var availableCtns = location.AvailableCtns;
                var availablePcs = location.AvailablePcs;
                var shippedPcs = location.ShippedPcs;

                //当pickingCtns不为0时，说明有货正在拣，不能进行移库操作。此项限制在前端完成
                //当库存剩余为0且没有货在拣，也不能进行移库操作。此项限制在前端完成

                cartonDetailInDb.ToBeAllocatedCtns += availableCtns;
                cartonDetailInDb.ToBeAllocatedPcs += availablePcs;
                cartonDetailInDb.Status = Status.Reallocating;

                location.AvailableCtns = 0;
                location.AvailablePcs = 0;
                location.Status = Status.Reallocated;

                //当正在拣货数量不为零时，不能移库（在前端实现）
                //当有库存没有已发出去的货时，删除库存记录(否则不删除记录)，将库存记录的剩余库存移至SKU待分配页面
                if (shippedPcs == 0)
                {
                    context.FCRegularLocationDetails.Remove(location);
                }
            }
        }
    }
}
