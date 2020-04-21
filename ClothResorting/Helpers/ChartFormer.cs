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

        public IList<TimeZone> GenerateXAxisTimeZone(DateTime today, string timeUnit, int timeCount)
        {
            var list = new List<TimeZone>();
            var lastDate = today;
            var timeDiff = 1;
            
            if (timeUnit == "Week")
            {
                lastDate = GetFirstDayOfWeek(today).AddDays(6);
                timeDiff = 7;
            }
            else if (timeUnit == "Month")
            {
                lastDate = new DateTime(today.Year, today.Month, 1).AddMonths(1).AddDays(-1);
            }

            for (var i = 0; i < timeCount; i++)
            {
                if (timeUnit == "Month")
                {
                    var startDate = new DateTime(lastDate.Year, lastDate.Month, 1);
                    list.Add(new TimeZone 
                    {
                        EndDate = lastDate,
                        StartDate = startDate
                    });

                    lastDate = startDate.AddDays(-1);
                }
                else
                {
                    list.Add(new TimeZone
                    {
                        EndDate = lastDate,
                        StartDate = lastDate.AddDays(-timeDiff + 1)
                    });

                    lastDate = lastDate.AddDays(-timeDiff);
                }
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
        }

        public string[] ConvertTimeZoneListToArray(IList<TimeZone> timeZoneList)
        {
            var list = new List<string>();

            foreach(var w in timeZoneList)
            {
                if (w.StartDate == w.EndDate)
                    list.Add(w.EndDate.ToString("dd MMM"));
                else 
                    list.Add(w.StartDate.ToString("dd MMM") + " - " + w.EndDate.ToString("dd MMM"));
            }

            return list.ToArray();
        }

        public InboundAndOutboundChartData GetInboundAndOutboundPltsChartData(DateTime dt, string timeUnit, int timeCount)
        {
            var timeZoneList = GenerateXAxisTimeZone(dt, timeUnit, timeCount);
            var timeZoneArray = ConvertTimeZoneListToArray(timeZoneList);

            var result = new InboundAndOutboundChartData { XAxisData =  timeZoneArray };
            var firstDate = timeZoneList.First().StartDate;
            var lastDate = timeZoneList.Last().EndDate.AddDays(1);

            var inboundPltsInDb = _context.FBAPallets
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.FBAMasterOrder.InboundDate >= firstDate && x.FBAMasterOrder.InboundDate < lastDate);

            var outboundPltsInDb = _context.FBAPickDetails
                .Include(x => x.FBAShipOrder)
                .Where(x => x.FBAShipOrder.ReleasedDate >= firstDate && x.FBAShipOrder.ReleasedDate < lastDate);

            var inboundPltsStack = new Stack<float>();
            var outboundPltsStack = new Stack<float>();

            foreach (var w in timeZoneList)
            {
                w.EndDate = w.EndDate.AddDays(1);

                var inboundPltsItems = inboundPltsInDb.Where(x => x.FBAMasterOrder.InboundDate >= w.StartDate && x.FBAMasterOrder.InboundDate < w.EndDate).ToList();
                var inboundPlts = inboundPltsItems == null ? 0 : inboundPltsItems.Sum(x => x.ActualPallets);
                inboundPltsStack.Push(inboundPlts);

                var outboundPltsItems = outboundPltsInDb.Where(x => x.FBAShipOrder.ReleasedDate >= w.StartDate && x.FBAShipOrder.ReleasedDate < w.EndDate).ToList();
                var outboundPlts = outboundPltsItems == null ? 0 : outboundPltsItems.Sum(x => x.PltsFromInventory);
                outboundPltsStack.Push(outboundPlts);
            }

            result.InboundData = inboundPltsStack.Reverse().ToArray();
            result.OutboundData = outboundPltsStack.Reverse().ToArray();
            result.DataType = "PltsData";

            return result;
        }

        public InboundAndOutboundChartData GetInboundAndOutboundCtnsChartData(DateTime dt, string timeUnit, int timeCount)
        {
            var timeZoneList = GenerateXAxisTimeZone(dt, timeUnit, timeCount);
            var timeZoneArray = ConvertTimeZoneListToArray(timeZoneList);

            var result = new InboundAndOutboundChartData { XAxisData = timeZoneArray };
            var firstDate = timeZoneList.First().StartDate;
            var lastDate = timeZoneList.Last().EndDate.AddDays(1);

            var inboundInDb = _context.FBAOrderDetails
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.FBAMasterOrder.InboundDate >= firstDate && x.FBAMasterOrder.InboundDate < lastDate);

            var outboundInDb = _context.FBAPickDetails
                .Include(x => x.FBAShipOrder)
                .Where(x => x.FBAShipOrder.ReleasedDate >= firstDate && x.FBAShipOrder.ReleasedDate < lastDate);

            var inboundStack = new Stack<float>();
            var outboundStack = new Stack<float>();

            foreach (var w in timeZoneList)
            {
                w.EndDate = w.EndDate.AddDays(1);
                var inboundItems = inboundInDb.Where(x => x.FBAMasterOrder.InboundDate >= w.StartDate && x.FBAMasterOrder.InboundDate < w.EndDate).ToList();
                var inbound = inboundItems == null ? 0 : inboundItems.Sum(x => x.ActualQuantity);
                inboundStack.Push(inbound);

                var outboundItems = outboundInDb.Where(x => x.FBAShipOrder.ReleasedDate >= w.StartDate && x.FBAShipOrder.ReleasedDate < w.EndDate).ToList();
                var outbound = outboundItems == null ? 0 : outboundItems.Sum(x => x.ActualQuantity);
                outboundStack.Push(outbound);
            }

            result.InboundData = inboundStack.Reverse().ToArray();
            result.OutboundData = outboundStack.Reverse().ToArray();
            result.DataType = "CtnsData";

            return result;
        }

        public InboundAndOutboundChartData GetInboundAndOutboundIncomesChartData(DateTime dt, string timeUnit, int timeCount)
        {
            var timeZoneList = GenerateXAxisTimeZone(dt, timeUnit, timeCount);
            var timeZoneArray = ConvertTimeZoneListToArray(timeZoneList);

            var result = new InboundAndOutboundChartData { XAxisData = timeZoneArray };
            var firstDate = timeZoneList.First().StartDate;
            var lastDate = timeZoneList.Last().EndDate.AddDays(1);

            var inboundInDb = _context.InvoiceDetails
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.FBAMasterOrder.InboundDate >= firstDate && x.FBAMasterOrder.InboundDate < lastDate);

            var outboundInDb = _context.InvoiceDetails
                .Include(x => x.FBAShipOrder)
                .Where(x => x.FBAShipOrder.ReleasedDate >= firstDate && x.FBAShipOrder.ReleasedDate < lastDate);

            var inboundStack = new Stack<float>();
            var outboundStack = new Stack<float>();

            foreach (var w in timeZoneList)
            {
                w.EndDate = w.EndDate.AddDays(1);
                var inboundItems = inboundInDb.Where(x => x.FBAMasterOrder.InboundDate >= w.StartDate && x.FBAMasterOrder.InboundDate < w.EndDate).ToList();
                var inbound = inboundItems == null ? 0 : (float)inboundItems.Sum(x => x.Amount);
                inboundStack.Push(inbound);

                var outboundItems = outboundInDb.Where(x => x.FBAShipOrder.ReleasedDate >= w.StartDate && x.FBAShipOrder.ReleasedDate < w.EndDate).ToList();
                var outbound = outboundItems == null ? 0 : (float)outboundItems.Sum(x => x.Amount);
                outboundStack.Push(outbound);
            }

            result.InboundData = inboundStack.Reverse().ToArray();
            result.OutboundData = outboundStack.Reverse().ToArray();
            result.DataType = "IncomesData";

            return result;
        }

        public InboundAndOutboundChartData GetInboundAndOutboundCostsChartData(DateTime dt, string timeUnit, int timeCount)
        {
            var timeZoneList = GenerateXAxisTimeZone(dt, timeUnit, timeCount);
            var timeZoneArray = ConvertTimeZoneListToArray(timeZoneList);

            var result = new InboundAndOutboundChartData { XAxisData = timeZoneArray };
            var firstDate = timeZoneList.First().StartDate;
            var lastDate = timeZoneList.Last().EndDate.AddDays(1);

            var inboundInDb = _context.InvoiceDetails
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.FBAMasterOrder.InboundDate >= firstDate && x.FBAMasterOrder.InboundDate < lastDate);

            var outboundInDb = _context.InvoiceDetails
                .Include(x => x.FBAShipOrder)
                .Where(x => x.FBAShipOrder.ReleasedDate >= firstDate && x.FBAShipOrder.ReleasedDate < lastDate);

            var inboundStack = new Stack<float>();
            var outboundStack = new Stack<float>();

            foreach (var w in timeZoneList)
            {
                w.EndDate = w.EndDate.AddDays(1);
                var inboundItems = inboundInDb.Where(x => x.FBAMasterOrder.InboundDate >= w.StartDate && x.FBAMasterOrder.InboundDate < w.EndDate).ToList();
                var inbound = inboundItems == null ? 0 : (float)inboundItems.Sum(x => x.Cost);
                inboundStack.Push(inbound);

                var outboundItems = outboundInDb.Where(x => x.FBAShipOrder.ReleasedDate >= w.StartDate && x.FBAShipOrder.ReleasedDate < w.EndDate).ToList();
                var outbound = outboundItems == null ? 0 : (float)outboundItems.Sum(x => x.Cost);
                outboundStack.Push(outbound);
            }

            result.InboundData = inboundStack.Reverse().ToArray();
            result.OutboundData = outboundStack.Reverse().ToArray();
            result.DataType = "CostsData";

            return result;
        }

        public InboundAndOutboundChartData GetInboundAndOutboundProfitsChartData(DateTime dt, string timeUnit, int timeCount)
        {
            var timeZoneList = GenerateXAxisTimeZone(dt, timeUnit, timeCount);
            var timeZoneArray = ConvertTimeZoneListToArray(timeZoneList);

            var result = new InboundAndOutboundChartData { XAxisData = timeZoneArray };
            var firstDate = timeZoneList.First().StartDate;
            var lastDate = timeZoneList.Last().EndDate.AddDays(1);

            var inboundInDb = _context.InvoiceDetails
                .Include(x => x.FBAMasterOrder)
                .Where(x => x.FBAMasterOrder.InboundDate >= firstDate && x.FBAMasterOrder.InboundDate < lastDate);

            var outboundInDb = _context.InvoiceDetails
                .Include(x => x.FBAShipOrder)
                .Where(x => x.FBAShipOrder.ReleasedDate >= firstDate && x.FBAShipOrder.ReleasedDate < lastDate);

            var inboundStack = new Stack<float>();
            var outboundStack = new Stack<float>();

            foreach (var w in timeZoneList)
            {
                w.EndDate = w.EndDate.AddDays(1);
                var inboundItems = inboundInDb.Where(x => x.FBAMasterOrder.InboundDate >= w.StartDate && x.FBAMasterOrder.InboundDate < w.EndDate).ToList();
                var inbound = inboundItems == null ? 0 : ((float)inboundItems.Sum(x => x.Amount) - (float)inboundItems.Sum(x => x.Cost));
                inboundStack.Push(inbound);

                var outboundItems = outboundInDb.Where(x => x.FBAShipOrder.ReleasedDate >= w.StartDate && x.FBAShipOrder.ReleasedDate < w.EndDate).ToList();
                var outbound = outboundItems == null ? 0 : ((float)outboundItems.Sum(x => x.Amount) - (float)outboundItems.Sum(x => x.Cost));
                outboundStack.Push(outbound);
            }

            result.InboundData = inboundStack.Reverse().ToArray();
            result.OutboundData = outboundStack.Reverse().ToArray();
            result.DataType = "ProfitsData";

            return result;
        }
    }

    public class TimeZone
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