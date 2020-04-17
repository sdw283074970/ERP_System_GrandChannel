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

        public InboundAndOutboundChartData GetInboundAndOutboundChartData(DateTime dt)
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

            var inboundPltsStack = new Stack<int>();
            var outboundPltsStack = new Stack<int>();

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
        public string[] XAxisData { get; set; }

        public int[] InboundData { get; set; }

        public int[] OutboundData { get; set; }
    }
}