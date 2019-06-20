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

namespace ClothResorting.Controllers.Api
{
    public class ContainerController : ApiController
    {
        private ApplicationDbContext _context;

        public ContainerController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /container/?container={container}
        [HttpGet]
        public IHttpActionResult GetContainerInfo([FromUri]string container)
        {
            var containerInDb = _context.Containers.SingleOrDefault(x => x.ContainerNumber == container);

            if (containerInDb == null)
            {
                throw new Exception("Container infomation is not found. Please assign container again in PO summary page.");
            }

            return Ok(Mapper.Map<Container, ContainerDto>(containerInDb));
        }

        // GET /container/?container={container}&operation={SKU}
        [HttpGet]
        public IHttpActionResult GetSKUInfo([FromUri]string container, [FromUri]string operation)
        {
            var cartonDetails = _context.RegularCartonDetails
                .Where(x => x.Container == container)
                .ToList();

            if (operation == "SKU")
            {
                return Ok(CountNumberOfSKU(cartonDetails));
            }

            return Ok();
        }

        // PUT /container/
        [HttpPut]
        public void UpdateContainerInfo(ContainerInfoJsonObj obj)
        {
            var containerInDb = _context.Containers.SingleOrDefault(x => x.ContainerNumber == obj.ContainerNumber);
            var preId = obj.PreId;
            var preReceiveOrderInDb = _context.PreReceiveOrders.Find(preId);

            preReceiveOrderInDb.CustomerName = obj.Vendor;

            containerInDb.Vendor = obj.Vendor;
            containerInDb.InboundDate = obj.ReceivedDate;
            containerInDb.Reference = obj.Reference;
            containerInDb.ReceiptNumber = obj.ReceiptNumber;
            containerInDb.Remark = obj.Remark;

            _context.SaveChanges();
        }

        int CountNumberOfSKU(IEnumerable<RegularCartonDetail> cartonDetails)
        {
            var skuList = new List<RegularCartonDetail>();

            foreach(var c in cartonDetails)
            {
                var sameSku = skuList.SingleOrDefault(x => x.PurchaseOrder == c.PurchaseOrder
                    && x.Color == c.Color
                    && x.Style == c.Style
                    && x.SizeBundle == c.SizeBundle);

                if (sameSku == null)
                {
                    skuList.Add(c);
                }
            }

            return skuList.Count;
        }
    }
}
