using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Helpers
{
    public class StringParser
    {
        //解析pre-location字符串
        public IEnumerable<PreLocation> ParseStrToPreLoc(string str)
        {
            if (str == null)
                return null;

            var list = new List<PreLocation>();

            //当字符串没有‘;’结尾的时候，说明只返回一个预分配库位对象
            //if (!str.Contains(";"))
            //{
            //    list.Add(new PreLocation {
            //        Ctns = int.Parse(str.Split(':')[1]),
            //        Plts = str.Split(':')[1].Contains("*") ? (int.Parse(str.Split(':')[1].Split('8')[1])) : 1,
            //        Location = str.Split(':')[0]
            //    });

            //    return list;
            //}

            var strArray = str.Split(';');

            //去掉最后一个空对象 
            var al = new ArrayList(strArray);
            al.RemoveAt(strArray.Length - 1);
            strArray = (string[])al.ToArray(typeof(string));

            //为每一个preloc对象分离属性
            foreach(var s in strArray)
            {
                list.Add(new PreLocation {
                    Location = s.Split(':')[0],
                    Ctns = int.Parse(s.Split(':')[1].Split('*')[0]),
                    Plts = s.Split(':')[1].Contains("*") ? (int.Parse(s.Split(':')[1].Split('*')[1])) : 1
                });
            }

            return list;
        }

        //解析pre-location对象
        public string ParsePreLocToStr(IEnumerable<PreLocation> list)
        {
            var str = string.Empty;

            foreach(var p in list)
            {
                if (p.Plts == 1)
                    str += p.Location + ":" + p.Ctns.ToString() + ";";
                else
                    str += p.Location + ":" + p.Ctns.ToString() + "*" + p.Plts + ";";
            }

            return str;
        }
    }

    public class PreLocation
    {
        public string Location { get; set; }

        public int Ctns { get; set; }

        public int Plts { get; set; }
    }
}