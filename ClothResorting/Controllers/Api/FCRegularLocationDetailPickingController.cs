using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using AutoMapper;
using ClothResorting.Dtos;

namespace ClothResorting.Controllers.Api
{
    public class FCRegularLocationDetailPickingController : ApiController
    {
        private ApplicationDbContext _context;

        public FCRegularLocationDetailPickingController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/FCRegularLocationDetailPicking
        public IHttpActionResult GetAllPicking([FromUri]int preId, [FromUri]string container, [FromUri]string batch, [FromUri]string po, [FromUri]string style, [FromUri]string color, [FromUri]string sku, [FromUri]string size)
        {
            var resultDto = _context.FCRegularLocationDetails
                .Include(c => c.PreReceiveOrder)
                .Where(c => c.PreReceiveOrder.Id == preId
                    && c.Status == "Picking")
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
    }
}
