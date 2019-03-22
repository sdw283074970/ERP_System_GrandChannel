using ClothResorting.Models;
using ClothResorting.Models.StaticClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using AutoMapper;
using ClothResorting.Dtos;

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBAEFolderController : ApiController
    {
        private ApplicationDbContext _context;

        public FBAEFolderController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/fba/fbaefolder/?reference={reference}&orderType={orderType}
        [HttpGet]
        public IHttpActionResult GetFilesFromReference([FromUri]string reference, [FromUri]string orderType)
        {
            if (orderType == FBAOrderType.MasterOrder)
            {
                var filesDto = _context.EFiles
                    .Include(x => x.FBAMasterOrder)
                    .Where(x => x.FBAMasterOrder.Container == reference)
                    .Select(Mapper.Map<EFile, EFileDto>);

                return Ok(filesDto);
            }
            else if (orderType == FBAOrderType.ShipOrder)
            {
                var filesDto = _context.EFiles
                    .Include(x => x.FBAShipOrder)
                    .Where(x => x.FBAShipOrder.ShipOrderNumber == reference)
                    .Select(Mapper.Map<EFile, EFileDto>);

                return Ok(filesDto);
            }

            return Ok();
        }
    }
}
