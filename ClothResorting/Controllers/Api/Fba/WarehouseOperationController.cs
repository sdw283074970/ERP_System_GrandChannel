using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Entity;
using AutoMapper;
using ClothResorting.Models.FBAModels;
using ClothResorting.Dtos.Fba;
using ClothResorting.Models.FBAModels.StaticModels;
using ClothResorting.Models.StaticClass;

namespace ClothResorting.Controllers.Api.Fba
{
    public class WarehouseOperationController : ApiController
    {
        private ApplicationDbContext _context;

        public WarehouseOperationController()
        {
            _context = new ApplicationDbContext();
        }

        // GET /api/warehouseoperation/?grandNumber={grandNumber}
        [HttpGet]
        public IHttpActionResult GetUnlaiedObjects([FromUri]string grandNumber)
        {
            return Ok(_context.FBAOrderDetails
                .Where(x => x.GrandNumber == grandNumber
                    && x.ComsumedQuantity < x.ActualQuantity)
                .Select(Mapper.Map<FBAOrderDetail, FBAOrderDetailDto>));
        }

        // POST /api/warehouseoperation/?masterOrderId={masterOrderId}&pltQuantity={pltQuantity}&pltSize={pltSize}&packType={packType}
        [HttpPost]
        public void CreatePallet([FromUri]string grandNumber, [FromUri]int pltQuantity, [FromUri]string pltSize, [FromUri]bool doesAppliedLabel, [FromUri]bool hasSortingMarking, [FromUri]bool isOverSizeOrOverwidth, [FromUri]string packType, [FromBody]IEnumerable<PalletInfoDto> objArray)
        {
            //进入库存的carton对象都按GW/ctn，CBM/ctn记录
            var cartonLocationList = new List<FBACartonLocation>();
            var orderDetailsInDb = _context.FBAOrderDetails
                .Where(x => x.GrandNumber == grandNumber);

            if (packType == FBAPackType.DetailPack)
            {
                foreach (var obj in objArray)
                {
                    var orderDetailInDb = orderDetailsInDb.SingleOrDefault(x => x.Id == obj.Id);

                    orderDetailInDb.ComsumedQuantity += obj.Quantity * pltQuantity;

                    if (orderDetailInDb.ComsumedQuantity > orderDetailInDb.ActualQuantity)
                    {
                        throw new Exception("Not enough quantity for comsuming. Check Id:" + obj.Id);
                    }

                    var cartonLocation = new FBACartonLocation();
                    cartonLocation.Status = FBAStatus.InPallet;
                    var ctnsPerPlt = obj.Quantity;
                    var grossWeightPerCtn = (float)Math.Round((orderDetailInDb.ActualGrossWeight / orderDetailInDb.ActualQuantity), 2);
                    var cbmPerCtn = (float)Math.Round((orderDetailInDb.ActualCBM / orderDetailInDb.ActualQuantity), 2);

                    cartonLocation.AssembleFirstStringPart(orderDetailInDb.ShipmentId, orderDetailInDb.AmzRefId, orderDetailInDb.WarehouseCode);
                    cartonLocation.AssemblePltInfo(grossWeightPerCtn, cbmPerCtn, ctnsPerPlt);

                    cartonLocation.Container = orderDetailInDb.Container;
                    //cartonLocation.AvaliableCtns = cartonLocation.ActualQuantity;
                    cartonLocation.Location = "Pallet";
                    cartonLocation.HowToDeliver = orderDetailInDb.HowToDeliver;
                    cartonLocation.GrandNumber = grandNumber;
                    cartonLocation.FBAOrderDetail = orderDetailInDb;
                    cartonLocation.ActualQuantity = ctnsPerPlt * pltQuantity;
                    cartonLocation.AvailableCtns = ctnsPerPlt * pltQuantity;

                    cartonLocationList.Add(cartonLocation);
                }

                //建立FBAPallet对象
                var pallet = new FBAPallet();
                var firstId = objArray.First().Id;
                var firstOrderDetail = orderDetailsInDb.SingleOrDefault(x => x.Id == firstId);

                pallet.AssembleFirstStringPart(DistinctStringList(cartonLocationList.Select(x => x.ShipmentId)), DistinctStringList(cartonLocationList.Select(x => x.AmzRefId)), DistinctStringList(cartonLocationList.Select(x => x.WarehouseCode)));
                pallet.AssembleActualDetails(cartonLocationList.Sum(x => x.GrossWeightPerCtn * x.CtnsPerPlt * pltQuantity), cartonLocationList.Sum(x => x.CBMPerCtn * x.CtnsPerPlt * pltQuantity), cartonLocationList.Sum(x => x.CtnsPerPlt * pltQuantity));
                pallet.AssembleBoolValue(doesAppliedLabel, hasSortingMarking, isOverSizeOrOverwidth);

                pallet.Container = firstOrderDetail.Container;
                pallet.HowToDeliver = firstOrderDetail.HowToDeliver;
                pallet.PalletSize = pltSize;
                pallet.GrandNumber = grandNumber;
                pallet.ActualPallets = pltQuantity;

                _context.FBAPallets.Add(pallet);

                foreach (var cartonLocation in cartonLocationList)
                {
                    cartonLocation.FBAPallet = pallet;
                }

                _context.FBACartonLocations.AddRange(cartonLocationList);
            }
            else
            {
                foreach (var obj in objArray)
                {
                    var orderDetailInDb = orderDetailsInDb.SingleOrDefault(x => x.Id == obj.Id);

                    orderDetailInDb.ComsumedQuantity += obj.Quantity;

                    if (orderDetailInDb.ComsumedQuantity > orderDetailInDb.ActualQuantity)
                    {
                        throw new Exception("Not enough quantity for comsuming. Check Id:" + obj.Id);
                    }

                    var cartonLocation = new FBACartonLocation();
                    cartonLocation.Status = FBAStatus.InPallet;
                    var grossWeightPerCtn = (float)Math.Round((orderDetailInDb.ActualGrossWeight / orderDetailInDb.ActualQuantity), 2);
                    var cbmPerCtn = (float)Math.Round((orderDetailInDb.ActualCBM / orderDetailInDb.ActualQuantity), 2);

                    cartonLocation.AssembleFirstStringPart(orderDetailInDb.ShipmentId, orderDetailInDb.AmzRefId, orderDetailInDb.WarehouseCode);
                    cartonLocation.AssemblePltInfo(grossWeightPerCtn, cbmPerCtn, 0);

                    cartonLocation.Container = orderDetailInDb.Container;
                    cartonLocation.Location = "Pallet";
                    cartonLocation.HowToDeliver = orderDetailInDb.HowToDeliver;
                    cartonLocation.GrandNumber = grandNumber;
                    cartonLocation.FBAOrderDetail = orderDetailInDb;
                    cartonLocation.ActualQuantity = obj.Quantity;
                    cartonLocation.AvailableCtns = obj.Quantity;

                    cartonLocationList.Add(cartonLocation);
                }

                //建立FBAPallet对象
                var pallet = new FBAPallet();
                var firstId = objArray.First().Id;
                var firstOrderDetail = orderDetailsInDb.SingleOrDefault(x => x.Id == firstId);

                pallet.AssembleFirstStringPart(DistinctStringList(cartonLocationList.Select(x => x.ShipmentId)), DistinctStringList(cartonLocationList.Select(x => x.AmzRefId)), DistinctStringList(cartonLocationList.Select(x => x.WarehouseCode)));
                pallet.ActualQuantity = cartonLocationList.Sum(x => x.ActualQuantity);
                pallet.AssembleBoolValue(doesAppliedLabel, hasSortingMarking, isOverSizeOrOverwidth);

                pallet.Container = firstOrderDetail.Container;
                pallet.HowToDeliver = firstOrderDetail.HowToDeliver;
                pallet.PalletSize = pltSize;
                pallet.GrandNumber = grandNumber;
                pallet.ActualPallets = pltQuantity;

                _context.FBAPallets.Add(pallet);

                foreach (var cartonLocation in cartonLocationList)
                {
                    cartonLocation.FBAPallet = pallet;
                }

                _context.FBACartonLocations.AddRange(cartonLocationList);
            }

            _context.SaveChanges();
        }
        
        public string DistinctStringList(IEnumerable<string> list)
        {
            var distinctString = string.Empty;

            foreach (var s in list)
            {
                if (!distinctString.Contains(s))
                {
                    distinctString += "," + s;
                }
            }

            return distinctString.Substring(1);
        }
    }

    public class PalletInfoDto
    {
        public int Id { get; set; }

        public int Quantity { get; set; }
    }
}
