using AutoMapper;
using ClothResorting.Dtos.Fba;
using ClothResorting.Helpers;
using ClothResorting.Models;
using ClothResorting.Models.FBAModels;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Data.Entity;
using ClothResorting.Models.FBAModels.StaticModels;

namespace ClothResorting.Controllers.Api.Fba
{
    public class FBAShipOrderController : ApiController
    {
        private ApplicationDbContext _context;
        private string _userName;

        public FBAShipOrderController()
        {
            _context = new ApplicationDbContext();
            _userName = HttpContext.Current.User.Identity.Name.Split('@')[0];
        }

        // GET /api/fba/fbashiporder/
        [HttpGet]
        public IHttpActionResult GetAllFBAShipOrder()
        {
            return Ok(_context.FBAShipOrders.Select(Mapper.Map<FBAShipOrder, FBAShipOrderDto>));
        }

        // GET /api/fba/fbashiporder/?shipOrderId={shipOrderId}
        [HttpGet]
        public IHttpActionResult GetBolFileName([FromUri]int shipOrderId)
        {
            var pickDetailsInDb = _context.FBAPickDetails
                .Include(x => x.FBAShipOrder)
                .Include(x => x.FBAPickDetailCartons)
                .Include(x => x.FBAPalletLocation.FBAPallet.FBACartonLocations)
                .Where(x => x.FBAShipOrder.Id == shipOrderId)
                .ToList();

            var bolList = GenerateFBABOLList(pickDetailsInDb);

            var generator = new PDFGenerator();

            var fileName = generator.GenerateFBABOL(shipOrderId, bolList);

            return Ok(fileName);
        }

        // POST /api/fba/fbashiporder/
        [HttpPost]
        public IHttpActionResult CreateNewShipOrder([FromBody]ShipOrderDto obj)
        {
            var shipOrder = new FBAShipOrder();
            var ets = new DateTime();

            DateTime.TryParseExact(obj.ETS, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out ets);

            shipOrder.AssembleBaseInfo(obj.ShipOrderNumber, obj.CustomerCode, obj.OrderType, obj.Destination, obj.PickReference);
            shipOrder.CreateBy = _userName;
            shipOrder.ShipDate = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            shipOrder.BOLNumber = obj.BOLNumber;
            shipOrder.Carrier = obj.Carrier;
            shipOrder.ETS = ets;

            _context.FBAShipOrders.Add(shipOrder);
            _context.SaveChanges();

            var sampleDto = Mapper.Map<FBAShipOrder, FBAShipOrderDto>(_context.FBAShipOrders.OrderByDescending(x => x.Id).First());

            return Created(Request.RequestUri + "/" + sampleDto.Id, sampleDto);
        }

        // PUT /api/fba/fbashiporder/?shipOrderId={shipOrderId}&operation={operation}
        [HttpPut]
        public void ChangeShipOrderStatus([FromUri]int shipOrderId, [FromUri]string operation)
        {
            var shipOrderInDb = _context.FBAShipOrders
                .Include(x => x.FBAPickDetails)
                .SingleOrDefault(x => x.Id == shipOrderId);

            if (shipOrderInDb.FBAPickDetails.Count() == 0)
            {
                throw new Exception("Cannot operate an empty ship order.");
            }

            if (operation == FBAOperation.ChangeStatus)
            {
                if (shipOrderInDb.Status == FBAStatus.Picking)
                {
                    shipOrderInDb.Status = FBAStatus.Ready;
                }
                else if (shipOrderInDb.Status == FBAStatus.Ready)
                {
                    shipOrderInDb.Status = FBAStatus.Picking;
                }
            }
            else if (operation == FBAOperation.ShipOrder)
            {
                foreach(var pickDetailInDb in shipOrderInDb.FBAPickDetails)
                {
                    ShipPickDetail(_context, pickDetailInDb.Id);
                }

                shipOrderInDb.Status = FBAStatus.Shipped;
            }

            _context.SaveChanges();
        }

        // DELETE /api/fba/fbashiporder/?shipOrderId={shipOrderId}
        [HttpDelete]
        public void DeleteShipOrder([FromUri]int shipOrderId)
        {
            var pickDetailsInDb = _context.FBAPickDetails
                .Include(x => x.FBAShipOrder)
                .Where(x => x.FBAShipOrder.Id == shipOrderId);

            var shipOrderInDb = _context.FBAShipOrders.Find(shipOrderId);

            var fbaPickDetailAPI = new FBAPickDetailController();

            foreach(var detail in pickDetailsInDb)
            {
                fbaPickDetailAPI.RemovePickDetail(_context, detail.Id);
            }

            _context.FBAShipOrders.Remove(shipOrderInDb);
            _context.SaveChanges();
        }

        private void ShipPickDetail(ApplicationDbContext context, int pickDetailId)
        {
            var pickDetailInDb = context.FBAPickDetails
                .Include(x => x.FBAPalletLocation.FBAPallet.FBACartonLocations)
                .Include(x => x.FBACartonLocation)
                .Include(x => x.FBAPickDetailCartons)
                .SingleOrDefault(x => x.Id == pickDetailId);

            if (pickDetailInDb.FBAPalletLocation != null)       //pickDetail是拣货pallet的情况
            {
                //将库存中的该pallet标记发货
                pickDetailInDb.FBAPalletLocation.ShippedPlts += pickDetailInDb.ActualPlts;
                pickDetailInDb.FBAPalletLocation.PickingPlts -= pickDetailInDb.ActualPlts;

                //将pallet中的carton也标记发货
                foreach(var cartonLocationInDb in pickDetailInDb.FBAPalletLocation.FBAPallet.FBACartonLocations)
                {
                    ShipCartonsInPallet(context, pickDetailInDb);
                }

                //更新在库状态
                if (pickDetailInDb.FBAPalletLocation.PickingPlts == 0 && pickDetailInDb.FBAPalletLocation.AvailablePlts != 0)
                {
                    pickDetailInDb.FBAPalletLocation.Status = FBAStatus.InStock;
                }
                else if (pickDetailInDb.FBAPalletLocation.PickingPlts == 0 && pickDetailInDb.FBAPalletLocation.AvailablePlts == 0)
                {
                    pickDetailInDb.FBAPalletLocation.Status = FBAStatus.Shipped;
                }
            }
            else if (pickDetailInDb.FBACartonLocation != null)      //pickDetail是直接拣货carton的情况
            {
                //直接将carton发货
                ShipPickDetailCartons(pickDetailInDb.FBACartonLocation, pickDetailInDb);
            }
        }

        private void ShipPickDetailCartons(FBACartonLocation cartonLocationInDb, FBAPickDetail pickDetailInDb)
        {
            cartonLocationInDb.ShippedCtns += pickDetailInDb.ActualQuantity;
            cartonLocationInDb.PickingCtns -= pickDetailInDb.ActualQuantity;

            if (cartonLocationInDb.PickingCtns == 0 && cartonLocationInDb.AvailableCtns != 0)
            {
                cartonLocationInDb.Status = FBAStatus.InStock;
            }
            else if (cartonLocationInDb.PickingCtns == 0 && cartonLocationInDb.AvailableCtns == 0)
            {
                cartonLocationInDb.Status = FBAStatus.Shipped;
            }
        }

        private void ShipCartonsInPallet(ApplicationDbContext context, FBAPickDetail pickDetailInDb)
        {
            var cartonsInPalletInDb = context.FBAPickDetailCartons
                .Include(x => x.FBAPickDetail)
                .Include(x => x.FBACartonLocation)
                .Where(x => x.FBAPickDetail.Id == pickDetailInDb.Id);

            foreach(var carton in cartonsInPalletInDb)
            {
                carton.FBACartonLocation.ShippedCtns += carton.PickCtns;
                carton.FBACartonLocation.PickingCtns -= carton.PickCtns;

                if (carton.FBACartonLocation.PickingCtns == 0 && carton.FBACartonLocation.AvailableCtns != 0)
                {
                    carton.FBACartonLocation.Status = FBAStatus.InStock;
                }
                else if (carton.FBACartonLocation.PickingCtns == 0 && carton.FBACartonLocation.AvailableCtns == 0)
                {
                    carton.FBACartonLocation.Status = FBAStatus.Shipped;
                }
            }
        }

        private IList<FBABOLDetail> GenerateFBABOLList(IEnumerable<FBAPickDetail> pickDetailsInDb)
        {
            var bolList = new List<FBABOLDetail>();

            foreach (var pickDetail in pickDetailsInDb)
            {
                if (pickDetail.FBAPalletLocation != null)
                {
                    var cartonInPickList = pickDetail.FBAPickDetailCartons.ToList();
                    for (int i = 0; i < cartonInPickList.Count; i++)
                    {
                        var plt = 0;

                        //只有托盘中的第一项物品显示托盘数，其他物品不显示并在生成PDF的时候取消表格顶线，99999用于区分是否是同一托盘的非首项
                        if (i == 0)
                        {
                            plt = pickDetail.ActualPlts;
                        }
                        else
                        {
                            plt = 99999;
                        }

                        bolList.Add(new FBABOLDetail
                        {
                            CustoerOrderNumber = cartonInPickList[i].FBACartonLocation.ShipmentId,
                            Contianer = pickDetail.Container,
                            CartonQuantity = cartonInPickList[i].PickCtns,
                            PalletQuantity = plt,
                            Weight = cartonInPickList[i].FBACartonLocation.GrossWeightPerCtn * cartonInPickList[i].PickCtns,
                            Location = pickDetail.Location
                        });
                    }
                }
                else
                {
                    bolList.Add(new FBABOLDetail
                    {
                        CustoerOrderNumber = pickDetail.ShipmentId,
                        Contianer = pickDetail.Container,
                        CartonQuantity = pickDetail.ActualQuantity,
                        PalletQuantity = 0,
                        Weight = pickDetail.ActualGrossWeight,
                        Location = pickDetail.Location
                    });
                }
            }

            return bolList;
        }
    }

    public class ShipOrderDto
    {
        public string ShipOrderNumber { get; set; }

        public string CustomerCode { get; set; }

        public string OrderType { get; set; }

        public string Destination { get; set; }

        public string PickReference { get; set; }

        public DateTime CreateDate { get; set; }

        public string CreateBy { get; set; }

        public DateTime PickDate { get; set; }

        public DateTime ShipDate { get; set; }

        public string PickMan { get; set; }

        public string Status { get; set; }

        public string ShippedBy { get; set; }

        public string BOLNumber { get; set; }

        public string Carrier { get; set; }

        public string ETS { get; set; }
    }
}
