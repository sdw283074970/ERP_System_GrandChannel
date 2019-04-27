using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Data.Entity;
using ClothResorting.Models.FBAModels.StaticModels;
using AutoMapper;
using ClothResorting.Models.FBAModels;

namespace ClothResorting.Controllers.Api.Warehouse
{
    public class WarehouseIndexController : ApiController
    {
        private ApplicationDbContext _context;
        private string _userName;

        public WarehouseIndexController()
        {
            _context = new ApplicationDbContext();
            _userName = HttpContext.Current.User.Identity.Name.Split('@').First();
        }

        // GET /api/warehouseIndex/
        [HttpGet]
        public IHttpActionResult GetAllWarehouseOrder()
        {
            //将FBA运单转成outbound work order
            var list = new List<WarehouseOrder>();

            var ordersInDb = _context.FBAShipOrders
                .Include(x => x.FBAPickDetails)
                .Where(x => x.Status != FBAStatus.NewCreated && x.Status != FBAStatus.Picking);

            foreach(var o in ordersInDb)
            {
                var order = Mapper.Map<FBAShipOrder, WarehouseOrder>(o);

                order.Department = "FBA";
                order.WarehouseOrderType = "Outbound";
                order.ETS = o.ETS.ToString("yyyy-MM-dd") + " " + o.ETSTimeRange;
                order.TotalCtns = o.FBAPickDetails.Sum(x => x.ActualQuantity);
                order.TotalPlts = o.FBAPickDetails.Sum(x => x.ActualPlts);
                order.ShipDate = o.ShipDate;
                order.OrderNumber = o.ShipOrderNumber;

                list.Add(order);
            }

            return Ok(list);
        }
    }

    public class WarehouseOrder
    {
        public int Id { get; set; }

        public string Department { get; set; }

        public string WarehouseOrderType { get; set; }

        public string Status { get; set; }

        public string OrderNumber { get; set; }

        public string BOLNumber { get; set; }

        public string PickNumber { get; set; }

        public string CustomerCode { get; set; }

        public string OrderType { get; set; }

        public string Destination { get; set; }

        public int TotalCtns { get; set; }

        public int TotalPlts { get; set; }

        public string Carrier { get; set; }

        public string ETS { get; set; }

        public string PlaceBy { get; set; }

        public string OperationLog { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime ReadyTime { get; set; }

        public DateTime ShipDate { get; set; }

        public DateTime PlaceTime { get; set; }

        public string ReleasedBy { get; set; }

        public string Instruction { get; set; }
    }
}
