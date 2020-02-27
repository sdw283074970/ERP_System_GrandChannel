using ClothResorting.Dtos.Fba;
using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using AutoMapper;
using ClothResorting.Models.FBAModels;

namespace ClothResorting.Helpers.FBAHelper
{
    public class FBAGetter
    {
        private ApplicationDbContext _context;

        public FBAGetter()
        {
            _context = new ApplicationDbContext();
        }

        public IList<FBAMasterOrderDto> GetAllMaterOrders() {
            var masterOrders = _context.FBAMasterOrders
                .Include(x => x.InvoiceDetails)
                .Include(x => x.FBAOrderDetails)
                .Include(x => x.FBAPallets)
                .ToList();

            var skuList = new List<int>();

            foreach (var m in masterOrders)
            {
                m.TotalAmount = (float)m.InvoiceDetails.Sum(x => x.Amount);
                m.TotalCost = (float)m.InvoiceDetails.Sum(x => x.Cost);
                m.TotalCBM = m.FBAOrderDetails.Sum(x => x.CBM);
                m.TotalCtns = m.FBAOrderDetails.Sum(x => x.Quantity);
                m.ActualCBM = m.FBAOrderDetails.Sum(x => x.ActualCBM);
                m.ActualCtns = m.FBAOrderDetails.Sum(x => x.ActualQuantity);
                m.ActualPlts = m.FBAPallets.Sum(x => x.ActualPallets);
                skuList.Add(m.FBAOrderDetails.GroupBy(x => x.ShipmentId).Count());
            }

            var resultDto = Mapper.Map<IList<FBAMasterOrder>, IList<FBAMasterOrderDto>>(masterOrders);

            for (int i = 0; i < masterOrders.Count; i++)
            {
                resultDto[i].SKUNumber = skuList[i];
                resultDto[i].Net = resultDto[i].TotalAmount - resultDto[i].TotalCost;
            }

            return resultDto;
        }

        public IList<FBAMasterOrderDto> GetFilteredMasterOrder(Filter filter) {
            var masterOrders = GetAllMaterOrders();

            masterOrders = masterOrders.Where(x => (filter.Status.Count() == 0 ? true : filter.Status.Contains(x.Status))
                && (filter.CustomerCodes.Count() == 0 ? true : filter.CustomerCodes.Contains(x.CustomerCode))
                && (filter.InvoiceStatus.Count() == 0 ? true : filter.InvoiceStatus.Contains(x.InvoiceStatus)))
                .ToList();

            if (!string.IsNullOrEmpty(filter.SortBy))
                masterOrders = filter.IsDesc ? masterOrders.OrderByDescending(x => x.GetType().GetProperty(filter.SortBy).GetValue(x, null)).ToList() : masterOrders.OrderBy(x => x.GetType().GetProperty(filter.SortBy).GetValue(x, null)).ToList();

            return masterOrders;
        }

        public IList<FBAShipOrderDto> GetAllShipOrders() {
            var shipOrders = _context.FBAShipOrders
                .Include(x => x.InvoiceDetails)
                .Include(x => x.FBAPickDetails)
                .ToList();

            foreach (var s in shipOrders)
            {
                s.TotalAmount = (float)s.InvoiceDetails.Sum(x => x.Amount);
                s.TotalCost = (float)s.InvoiceDetails.Sum(x => x.Cost);
                s.TotalCtns = s.FBAPickDetails.Sum(x => x.ActualQuantity);
                s.TotalPlts = s.FBAPickDetails.Sum(x => x.ActualPlts);
                s.ETSTimeRange = s.ETS.ToString("yyyy-MM-dd") + " " + s.ETSTimeRange;
            }

            var dto = Mapper.Map<IList<FBAShipOrder>, IList<FBAShipOrderDto>>(shipOrders);

            foreach (var d in dto)
            {
                d.Net = d.TotalAmount - d.TotalCost;
            }

            return dto;
        }

        public IList<FBAShipOrderDto> GetFilteredShipOrder(Filter filter)
        {
            var shipOrders = GetAllShipOrders();

            shipOrders = shipOrders.Where(x => (filter.Status.Count() == 0 ? true : filter.Status.Contains(x.Status))
                && (filter.CustomerCodes.Count() == 0 ? true : filter.CustomerCodes.Contains(x.CustomerCode))
                && (filter.InvoiceStatus.Count() == 0 ? true : filter.InvoiceStatus.Contains(x.InvoiceStatus)))
                .ToList();

            if (!string.IsNullOrEmpty(filter.SortBy))
                shipOrders = filter.IsDesc ? shipOrders.OrderByDescending(x => x.GetType().GetProperty(filter.SortBy).GetValue(x, null)).ToList() : shipOrders.OrderBy(x => x.GetType().GetProperty(filter.SortBy).GetValue(x, null)).ToList();

            return shipOrders;
        }
    }

    public class Filter 
    {
        public string[] Status { get; set; }

        public string[] CustomerCodes { get; set; }

        public string[] InvoiceStatus { get; set; }

        public string SortBy { get; set; }

        public bool IsDesc { get; set; }
    }
}