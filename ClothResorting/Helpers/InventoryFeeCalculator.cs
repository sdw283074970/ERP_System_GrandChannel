using ClothResorting.Models;
using ClothResorting.Models.FBAModels;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace ClothResorting.Helpers
{
    public class InventoryFeeCalculator
    {
        //全局变量
        private FBADbContext _context;
        private string _path = "";
        private _Application _excel;
        private Workbook _wb;
        private Worksheet _ws;

        //构造器
        public InventoryFeeCalculator()
        {
            _context = new FBADbContext();
        }

        public InventoryFeeCalculator(string path)
        {
            _context = new FBADbContext();
            _path = path;
            _excel = new Application();
            _wb = _excel.Workbooks.Open(_path);
        }

        //调整Excel的主方法
        public void RecalculateInventoryFeeInExcel(IEnumerable<ChargeMethod> chargeMethods, string lastBillingDate, string currentBillingDate)
        {
            _ws = _wb.Worksheets[1];
            var countOfEntries = -1;
            var index = 1;
            while(index > 0)
            {
                if (_ws.Cells[index, 1].Value2 != null)
                {
                    countOfEntries++;
                    index++;
                }
                else
                {
                    index = 1;
                    break;
                }
            }

            for(int i = 0; i < countOfEntries; i++)
            {
                var inboundDate = _ws.Cells[i + 2, 7].Value2.ToString();
                string outboundDate = _ws.Cells[i + 2, 8].Value2 == null ? null : _ws.Cells[i + 2, 8].Value2.ToString();
                var totalPlts = _ws.Cells[i + 2, 6].Value2 == 0 || _ws.Cells[i + 2, 6].Value2 == null ? 1 : (int)_ws.Cells[i + 2, 6].Value2;
                var storedWeek = CalculateNunmberOfWeek(inboundDate, outboundDate, lastBillingDate, currentBillingDate);
                double storageCharge = 0;

                foreach(var method in chargeMethods)
                {
                    if (storedWeek >= method.WeekNumber)
                    {
                        storageCharge += method.WeekNumber * method.Fee * totalPlts;
                        storedWeek -= method.WeekNumber;
                    }
                    else if (storedWeek < method.WeekNumber)
                    {
                        storageCharge += storedWeek * method.Fee * totalPlts;
                        break;
                    }
                }

                _ws.Cells[i] = storageCharge;
            }

            _wb.Save();
        }

        //输入两个日期字符串以及账单日范围，算出有多少周
        public int CalculateNunmberOfWeek(string inboundDate, string outboundDate, string lastBillingDate, string currentBillingDate)
        {
            DateTime startDt;
            DateTime endDt;

            //首先将M/d/yyyy格式的日期转换为DateTime的格式
            DateTime inboundDt;
            DateTime outboundDt;
            DateTime lastBillingDt;
            DateTime currentBillingDt;

            //当outbound为空时，将今天当作outbound
            if (outboundDate == null)
            {
                outboundDate = DateTime.Now.ToString("M/d/yyyy");
            }

            DateTime.TryParseExact(inboundDate, "M/d/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out inboundDt);
            DateTime.TryParseExact(outboundDate, "M/d/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out outboundDt);
            DateTime.TryParseExact(lastBillingDate, "MMddyyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out lastBillingDt);
            DateTime.TryParseExact(currentBillingDate, "MMddyyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out currentBillingDt);

            //通过对比，得出有效的开始截至日期范围
            if (DateTime.Compare(inboundDt, lastBillingDt) > 0)
            {
                startDt = inboundDt;
            }
            else
            {
                startDt = lastBillingDt;
            }

            if (DateTime.Compare(outboundDt, currentBillingDt) > 0)
            {
                endDt = currentBillingDt;
            }
            else
            {
                endDt = outboundDt;
            }

            var timeSpan = endDt.Subtract(startDt);
            var weeks = Math.Ceiling((double)timeSpan.Days / 7);

            return (int)weeks;
        }
    }
}