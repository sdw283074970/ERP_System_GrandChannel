using ClothResorting.Models;
using ClothResorting.Models.FBAModels;
using ClothResorting.Models.FBAModels.StaticModels;
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
            var chargeMethodsList = chargeMethods.OrderBy(x => x.From).ToList();

            _ws = _wb.Worksheets[1];
            var countOfEntries = 0; //待收费条目数量
            var index = 2;

            _ws.Cells[1, 11] = timeUnit + "s Stored in Billing Period";
            _ws.Cells[1, 12] = "Charge From " + timeUnit;

            while (index > 0)
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
                var startTimeUnit = 1;
                string inboundDate = _ws.Cells[i + 2, 8].Value.ToString("MM/dd/yyyy");
                string outboundDate = _ws.Cells[i + 2, 9].Value2 == null ? null : _ws.Cells[i + 2, 9].Value.ToString("MM/dd/yyyy");
                var totalPlts = _ws.Cells[i + 2, 7].Value2 == 0 || _ws.Cells[i + 2, 7].Value2 == null ? 1 : (int)_ws.Cells[i + 2, 7].Value2;
                var storedDuration = CalculateDuration(timeUnit, inboundDate, outboundDate, lastBillingDate, currentBillingDate, out startTimeUnit);
                double storageCharge = 0;
                double lastFee = 0;

                _ws.Cells[i + 2, 11] = storedDuration;
                _ws.Cells[i + 2, 12] = startTimeUnit;

                //查找应该从第几个时间单位开始计费(查找开始计费的时间落在哪个计费区间)
                int starIndex = 0;
                for(int k = 0; k < chargeMethodsList.Count(); k++)
                {
                    if (startTimeUnit >= chargeMethodsList[k].From && startTimeUnit <= chargeMethodsList[k].To)
                    {
                        starIndex = k;
                        break;
                    }

                    if (startTimeUnit > chargeMethodsList[chargeMethodsList.Count - 1].From)
                    {
                        starIndex = chargeMethodsList.Count - 1;
                        break;
                    }
                }

                for( int j = starIndex; j < chargeMethods.Count(); j++)
                {
                    lastFee = chargeMethodsList[j].Fee;

                    if (j == starIndex && j != chargeMethods.Count() -1)
                    {
                        var currentDuration = chargeMethodsList[j].To - startTimeUnit + 1;
                        if (storedDuration >= currentDuration)
                        {
                            storageCharge += currentDuration * chargeMethodsList[j].Fee * totalPlts;
                            storedDuration -= currentDuration;
                        }
                        else
                        {
                            storageCharge += storedDuration * chargeMethodsList[j].Fee * totalPlts;
                            storedDuration = 0;
                        }
                    }
                    else
                    {
                        if (storedDuration >= chargeMethodsList[j].Duration)
                        {
                            storageCharge += chargeMethodsList[j].Duration * chargeMethodsList[j].Fee * totalPlts;
                            storedDuration -= chargeMethodsList[j].Duration;
                        }
                        else if (storedDuration < chargeMethodsList[j].Duration)
                        {
                            storageCharge += storedDuration * chargeMethodsList[j].Fee * totalPlts;
                            storedDuration = 0;
                            break;
                        }
                    }
                }

                if (storedDuration != 0)
                {
                    storageCharge += storedDuration * lastFee * totalPlts;
                }

                _ws.Cells[i + 2, 10] = storageCharge;
            }

            //打上账单日
            _ws.Cells[countOfEntries + 3, 2] = "Start Billing Date:";
            _ws.Cells[countOfEntries + 3, 3] = lastBillingDate;

            _ws.Cells[countOfEntries + 3, 8] = "Close Billing Date:";
            _ws.Cells[countOfEntries + 3, 9] = currentBillingDate;

            _wb.Save();
            _excel.Quit();
        }

        //输入两个日期字符串以及账单日范围，算出有多少周
        public int CalculateDuration(string timeUnit, string inboundDate, string outboundDate, string lastBillingDate, string currentBillingDate, out int startTimeUnit)
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

            var currentTimeSpan = endDt.Subtract(startDt);
            var totalTimeSpan = endDt.Subtract(inboundDt);
            double currentDuration = 0;
            var start = 1;

            if (timeUnit == TimeUnit.Week)
            {
                double totalDuration = Math.Ceiling(((double)totalTimeSpan.Days + 1) / 7);                //第一天总是要计费的

                //计算从入库日期开始到本账单日开始计算时间这点时间内，已计算过费用的周数，总天数拣去已收费周数等于应收费周数
                if (lastBillingDt > inboundDt)
                {
                    var pastDuration = Math.Ceiling((double)lastBillingDt.Subtract(inboundDt).Days / 7);    //已经重复记了一天，就不用再+1天了
                    currentDuration = totalDuration - pastDuration;

                    // 计算从第几个时间单位开始计费（除去账单日前计过费的时间）
                    start = (int)pastDuration + 1;
                }
                else
                {
                    currentDuration = totalDuration;
                }
            }
            else if (timeUnit == TimeUnit.Month)
            {
                double totalDuration = Math.Ceiling(((double)totalTimeSpan.Days + 1) / 30);                //第一天总是要计费的

                //计算从入库日期开始到本账单日开始计算时间这点时间内，已计算过费用的月数，总天数拣去已收费月数等于应收费月数
                if (lastBillingDt > inboundDt)
                {
                    var pastDuration = Math.Ceiling((double)lastBillingDt.Subtract(inboundDt).Days / 30);    //已经重复记了一天，就不用再+1天了
                    currentDuration = totalDuration - pastDuration;

                    // 计算从第几个时间单位开始计费（除去账单日前计过费的时间）
                    start = (int)pastDuration + 1;
                }
                else
                {
                    currentDuration = totalDuration;
                }
            }
            else if (timeUnit == TimeUnit.Day)
            {
                double totalDuration = Math.Ceiling((double)totalTimeSpan.Days + 1);                //第一天总是要计费的

                //计算从入库日期开始到本账单日开始计算时间这点时间内，已计算过费用的天数，总天数拣去已收费天数等于应收费天数
                if (lastBillingDt > inboundDt)
                {
                    var pastDuration = Math.Ceiling((double)lastBillingDt.Subtract(inboundDt).Days);    //已经重复记了一天，就不用再+1天了
                    currentDuration = totalDuration - pastDuration;

                    // 计算从第几个时间单位开始计费（除去账单日前计过费的时间）
                    start = (int)pastDuration + 1;
                }
                else
                {
                    currentDuration = totalDuration;
                }
            }

            startTimeUnit = start;

            return (int)currentDuration;
        }
    }
}