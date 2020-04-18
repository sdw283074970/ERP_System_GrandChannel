using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace ClothResorting.Helpers
{
    public class ChartFormer
    {
        private ApplicationDbContext _context;

        public ChartFormer()
        {
            _context = new ApplicationDbContext();
        }

        public IList<WeekZone> GenerateXAxisWeekZone(DateTime today)
        {
            var list = new List<WeekZone>();

            var lastDate = GetFirstDayOfWeek(today).AddDays(6);
            
            for(var i = 0; i < 7; i++)
            {
                list.Add(new WeekZone {
                    EndDate = lastDate,
                    StartDate = lastDate.AddDays(-6)
                });

                lastDate = lastDate.AddDays(-7);
            }

            list.Reverse();

            return list;
        }

        public DateTime GetFirstDayOfWeek(DateTime dt)
        {
            int weeknow = Convert.ToInt32(dt.DayOfWeek);
            weeknow = weeknow == 0 ? 7 : weeknow;
            int daydiff = (-1) * weeknow + 1;

            return dt.AddDays(daydiff);
            //dt = dt == null ? DateTime.Now : dt;
            //int daydiff = (int)dt.DayOfWeek - 1 < 0 ? 6 : (int)dt.DayOfWeek - 1;//如果是0结果小于0表示周日 那最后要减6天:其他天数在dayOfWeek上减1，表示回到周一
            //DateTime result = dt.AddDays(-daydiff);
            //return result;
        }

        public string[] ConvertWeekZoneListToArray(IList<WeekZone> weekZoneList)
        {
            var list = new List<string>();

            foreach(var w in weekZoneList)
            {
                list.Add(w.StartDate.ToString("dd MMM") + " - " + w.EndDate.ToString("dd MMM"));
            }

            return list.ToArray();
        }

        public InboundAndOutboundChartData GetInboundAndOutboundPltsChartData(DateTime dt)
        {
            var weekZoneList = GenerateXAxisWeekZone(dt);
            var weekZoneArray = ConvertWeekZoneListToArray(weekZoneList);

            var result = new InboundAndOutboundChartData { XAxisData =  weekZoneArray };
            var firstDate = weekZoneList.First().StartDate;
            var lastDate = weekZoneList.Last().EndDate;

            var inboundPltsInDb = _context.FBAPallets
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.FBAMasterOrder.InboundDate >= firstDate && x.FBAMasterOrder.InboundDate <= lastDate);

            var outboundPltsInDb = _context.FBAPickDetails
                .Include(x => x.FBAShipOrder)
                .Where(x => x.FBAShipOrder.ReleasedDate >= firstDate && x.FBAShipOrder.ReleasedDate <= lastDate);

            var inboundPltsStack = new Stack<float>();
            var outboundPltsStack = new Stack<float>();

            foreach (var w in weekZoneList)
            {
                var inboundPltsItems = inboundPltsInDb.Where(x => x.FBAMasterOrder.InboundDate >= w.StartDate && x.FBAMasterOrder.InboundDate <= w.EndDate).ToList();
                var inboundPlts = inboundPltsItems == null ? 0 : inboundPltsItems.Sum(x => x.ActualPallets);
                inboundPltsStack.Push(inboundPlts);

                var outboundPltsItems = outboundPltsInDb.Where(x => x.FBAShipOrder.ReleasedDate >= w.StartDate && x.FBAShipOrder.ReleasedDate <= w.EndDate).ToList();
                var outboundPlts = outboundPltsItems == null ? 0 : outboundPltsItems.Sum(x => x.PltsFromInventory);
                outboundPltsStack.Push(outboundPlts);
            }

            result.InboundData = inboundPltsStack.Reverse().ToArray();
            result.OutboundData = outboundPltsStack.Reverse().ToArray();
            result.DataType = "PltsData";

            return result;
        }

        public InboundAndOutboundChartData GetInboundAndOutboundCtnsChartData(DateTime dt)
        {
            var weekZoneList = GenerateXAxisWeekZone(dt);
            var weekZoneArray = ConvertWeekZoneListToArray(weekZoneList);

            var result = new InboundAndOutboundChartData { XAxisData = weekZoneArray };
            var firstDate = weekZoneList.First().StartDate;
            var lastDate = weekZoneList.Last().EndDate;

            var inboundInDb = _context.FBAOrderDetails
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.FBAMasterOrder.InboundDate >= firstDate && x.FBAMasterOrder.InboundDate <= lastDate);

            var outboundInDb = _context.FBAPickDetails
                .Include(x => x.FBAShipOrder)
                .Where(x => x.FBAShipOrder.ReleasedDate >= firstDate && x.FBAShipOrder.ReleasedDate <= lastDate);

            var inboundStack = new Stack<float>();
            var outboundStack = new Stack<float>();

            foreach (var w in weekZoneList)
            {
                var inboundItems = inboundInDb.Where(x => x.FBAMasterOrder.InboundDate >= w.StartDate && x.FBAMasterOrder.InboundDate <= w.EndDate).ToList();
                var inbound = inboundItems == null ? 0 : inboundItems.Sum(x => x.ActualQuantity);
                inboundStack.Push(inbound);

                var outboundItems = outboundInDb.Where(x => x.FBAShipOrder.ReleasedDate >= w.StartDate && x.FBAShipOrder.ReleasedDate <= w.EndDate).ToList();
                var outbound = outboundItems == null ? 0 : outboundItems.Sum(x => x.ActualQuantity);
                outboundStack.Push(outbound);
            }

            result.InboundData = inboundStack.Reverse().ToArray();
            result.OutboundData = outboundStack.Reverse().ToArray();
            result.DataType = "CtnsData";

            return result;
        }

        public InboundAndOutboundChartData GetInboundAndOutboundIncomesChartData(DateTime dt)
        {
            var weekZoneList = GenerateXAxisWeekZone(dt);
            var weekZoneArray = ConvertWeekZoneListToArray(weekZoneList);

            var result = new InboundAndOutboundChartData { XAxisData = weekZoneArray };
            var firstDate = weekZoneList.First().StartDate;
            var lastDate = weekZoneList.Last().EndDate;

            var inboundInDb = _context.InvoiceDetails
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.FBAMasterOrder.InboundDate >= firstDate && x.FBAMasterOrder.InboundDate <= lastDate);

            var outboundInDb = _context.InvoiceDetails
                .Include(x => x.FBAShipOrder)
                .Where(x => x.FBAShipOrder.ReleasedDate >= firstDate && x.FBAShipOrder.ReleasedDate <= lastDate);

            var inboundStack = new Stack<float>();
            var outboundStack = new Stack<float>();

            foreach (var w in weekZoneList)
            {
                var inboundItems = inboundInDb.Where(x => x.FBAMasterOrder.InboundDate >= w.StartDate && x.FBAMasterOrder.InboundDate <= w.EndDate).ToList();
                var inbound = inboundItems == null ? 0 : (float)inboundItems.Sum(x => x.Amount);
                inboundStack.Push(inbound);

                var outboundItems = outboundInDb.Where(x => x.FBAShipOrder.ReleasedDate >= w.StartDate && x.FBAShipOrder.ReleasedDate <= w.EndDate).ToList();
                var outbound = outboundItems == null ? 0 : (float)outboundItems.Sum(x => x.Amount);
                outboundStack.Push(outbound);
            }

            result.InboundData = inboundStack.Reverse().ToArray();
            result.OutboundData = outboundStack.Reverse().ToArray();
            result.DataType = "IncomesData";

            return result;
        }

        public InboundAndOutboundChartData GetInboundAndOutboundCostsChartData(DateTime dt)
        {
            var weekZoneList = GenerateXAxisWeekZone(dt);
            var weekZoneArray = ConvertWeekZoneListToArray(weekZoneList);

            var result = new InboundAndOutboundChartData { XAxisData = weekZoneArray };
            var firstDate = weekZoneList.First().StartDate;
            var lastDate = weekZoneList.Last().EndDate;

            var inboundInDb = _context.InvoiceDetails
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.FBAMasterOrder.InboundDate >= firstDate && x.FBAMasterOrder.InboundDate <= lastDate);

            var outboundInDb = _context.InvoiceDetails
                .Include(x => x.FBAShipOrder)
                .Where(x => x.FBAShipOrder.ReleasedDate >= firstDate && x.FBAShipOrder.ReleasedDate <= lastDate);

            var inboundStack = new Stack<float>();
            var outboundStack = new Stack<float>();

            foreach (var w in weekZoneList)
            {
                var inboundItems = inboundInDb.Where(x => x.FBAMasterOrder.InboundDate >= w.StartDate && x.FBAMasterOrder.InboundDate <= w.EndDate).ToList();
                var inbound = inboundItems == null ? 0 : (float)inboundItems.Sum(x => x.Cost);
                inboundStack.Push(inbound);

                var outboundItems = outboundInDb.Where(x => x.FBAShipOrder.ReleasedDate >= w.StartDate && x.FBAShipOrder.ReleasedDate <= w.EndDate).ToList();
                var outbound = outboundItems == null ? 0 : (float)outboundItems.Sum(x => x.Cost);
                outboundStack.Push(outbound);
            }

            result.InboundData = inboundStack.Reverse().ToArray();
            result.OutboundData = outboundStack.Reverse().ToArray();
            result.DataType = "CostsData";

            return result;
        }

        public InboundAndOutboundChartData GetInboundAndOutboundProfitsChartData(DateTime dt)
        {
            var weekZoneList = GenerateXAxisWeekZone(dt);
            var weekZoneArray = ConvertWeekZoneListToArray(weekZoneList);

            var result = new InboundAndOutboundChartData { XAxisData = weekZoneArray };
            var firstDate = weekZoneList.First().StartDate;
            var lastDate = weekZoneList.Last().EndDate;

            var inboundInDb = _context.InvoiceDetails
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.FBAMasterOrder.InboundDate >= firstDate && x.FBAMasterOrder.InboundDate <= lastDate);

            var outboundInDb = _context.InvoiceDetails
                .Include(x => x.FBAShipOrder)
                .Where(x => x.FBAShipOrder.ReleasedDate >= firstDate && x.FBAShipOrder.ReleasedDate <= lastDate);

            var inboundStack = new Stack<float>();
            var outboundStack = new Stack<float>();

            foreach (var w in weekZoneList)
            {
                var inboundItems = inboundInDb.Where(x => x.FBAMasterOrder.InboundDate >= w.StartDate && x.FBAMasterOrder.InboundDate <= w.EndDate).ToList();
                var inbound = inboundItems == null ? 0 : ((float)inboundItems.Sum(x => x.Amount) - (float)inboundItems.Sum(x => x.Cost));
                inboundStack.Push(inbound);

                var outboundItems = outboundInDb.Where(x => x.FBAShipOrder.ReleasedDate >= w.StartDate && x.FBAShipOrder.ReleasedDate <= w.EndDate).ToList();
                var outbound = outboundItems == null ? 0 : ((float)outboundItems.Sum(x => x.Amount) - (float)outboundItems.Sum(x => x.Cost));
                outboundStack.Push(outbound);
            }

            result.InboundData = inboundStack.Reverse().ToArray();
            result.OutboundData = outboundStack.Reverse().ToArray();
            result.DataType = "ProfitsData";

            return result;
        }
    }

    public class WeekZone
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }

    public class InboundAndOutboundChartData
    {
        public string DataType { get; set; }

        public string[] XAxisData { get; set; }

        public float[] InboundData { get; set; }

        public float[] OutboundData { get; set; }
    }
}