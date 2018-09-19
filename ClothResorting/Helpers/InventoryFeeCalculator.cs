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
        public void RecalculateInventoryFeeInExcel(IEnumerable<ChargeMethod> chargeMethods, string timeUnit, string lastBillingDate, string currentBillingDate)
        {
            //首先将ChargeMehods表按照时间顺序排序
            chargeMethods = chargeMethods.OrderBy(x => x.From);

            _ws = _wb.Worksheets[1];
            var countOfEntries = -7;
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
                var inboundDate = _ws.Cells[i + 7, 7].Value.ToString("MM/dd/yyyy");
                string outboundDate = _ws.Cells[i + 7, 8].Value2 == null ? null : _ws.Cells[i + 7, 8].Value.ToString("MM/dd/yyyy");
                var totalPlts = _ws.Cells[i + 7, 6].Value2 == 0 || _ws.Cells[i + 7, 6].Value2 == null ? 1 : (int)_ws.Cells[i + 7, 6].Value2;
                var storedDuration = CalculateDuration(timeUnit, inboundDate, outboundDate, lastBillingDate, currentBillingDate);
                double storageCharge = 0;
                double lastFee = 0;

                foreach(var method in chargeMethods)
                {
                    if (storedDuration >= method.Duration)
                    {
                        storageCharge += method.Duration * method.Fee * totalPlts;
                        storedDuration -= method.Duration;
                        lastFee = method.Fee;
                    }
                    else if (storedDuration < method.Duration)
                    {
                        storageCharge += storedDuration * method.Fee * totalPlts;
                        break;
                    }
                }

                if (storedDuration != 0)
                {
                    storageCharge += storedDuration * lastFee * totalPlts;
                }

                _ws.Cells[i + 7, 9] = storageCharge;
            }

            //打上账单日
            _ws.Cells[countOfEntries + 7, 2] = "Last Billing Date:";
            _ws.Cells[countOfEntries + 7, 3] = lastBillingDate;

            _ws.Cells[countOfEntries + 7, 5] = "Current Billing Date:";
            _ws.Cells[countOfEntries + 7, 6] = currentBillingDate;

            _wb.Save();
            _excel.Quit();
        }

        //输入两个日期字符串以及账单日范围，算出有多少周
        public int CalculateDuration(string timeUnit, string inboundDate, string outboundDate, string lastBillingDate, string currentBillingDate)
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
                outboundDate = DateTime.Now.ToString("MM/dd/yyyy");
            }

            DateTime.TryParseExact(inboundDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out inboundDt);
            DateTime.TryParseExact(outboundDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out outboundDt);
            DateTime.TryParseExact(lastBillingDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out lastBillingDt);
            DateTime.TryParseExact(currentBillingDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out currentBillingDt);

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
            double duration = 0;

            if (timeUnit == "Week")
            {
                duration = Math.Ceiling((double)timeSpan.Days / 7);
            }
            else
            {
                duration = Math.Ceiling((double)timeSpan.Days / 7);
            }

            return (int)duration;
        }
    }
}