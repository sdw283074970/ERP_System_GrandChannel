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
    //已收取的cartons总数
    public class CartonDetailReceivingController : ApiController
    {
        private ApplicationDbContext _context;

        public CartonDetailReceivingController()
        {
            _context = new ApplicationDbContext();
        }

        // POST /api/totalcartonreceived
        [HttpPost]
        public IHttpActionResult UpdateReceivedCartonBreakdowns([FromBody]PurchaseOrderReceivedCartons poCartons)
        {
            //根据po获取数据库的packing list对象
            var packingListInDb = _context.PackingLists
                .SingleOrDefault(s => s.PurchaseOrder == poCartons.PurchaseOrder);

            packingListInDb.ActualReceived += poCartons.ReceivedCartons;
            packingListInDb.Available += poCartons.ReceivedCartons;
            
            _context.SaveChanges();

            return Ok(packingListInDb);
        }
    }
}
