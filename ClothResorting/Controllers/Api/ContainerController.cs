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

        // PUT /container/
        [HttpPut]
        public void UpdateContainerInfo(ContainerInfoJsonObj obj)
        {
            var containerInDb = _context.Containers.SingleOrDefault(x => x.ContainerNumber == obj.ContainerNumber);

            containerInDb.Vendor = obj.Vendor;
            containerInDb.ReceivedDate = obj.ReceivedDate;
            containerInDb.Reference = obj.Reference;
            containerInDb.ReceiptNumber = obj.ReceiptNumber;
            containerInDb.Remark = obj.Remark;

            _context.SaveChanges();
        }
    }
}
