using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Helpers
{
    public class Tester
    {
        //从类似"12-25"字符串中获取箱号范围的前段
        public int GetFrom(string cn)
        {
            string[] arr;
            if (cn.Contains('-'))
            {
                arr = cn.Split('-');
                return int.Parse(arr[0]);
            }
            else
            {
                return int.Parse(cn);
            }
        }

        public int GetTo(string cn)
        {
            string[] arr;
            if (cn.Contains('-'))
            {
                arr = cn.Split('-');
                return int.Parse(arr[1]);
            }
            else
            {
                return int.Parse(cn);
            }
        }

        //仅测试用：为每一个Species自动建立库位为TESTLOC的永久性库位
        //20180628更新：现在能检测是否会建立重复的永久库位
        //public void CreatePermanentLocForEachSpecies(ApplicationDbContext context)
        //{
        //    var speciesToList = context.SpeciesInventories.Where(c => c.Id > 0).ToList();
        //    var permanentLocToList = context.PermanentLocations.Where(c => c.Id > 0).ToList();
        //    var permanentLocList = new List<PermanentLocation>();

        //    foreach(var species in speciesToList)
        //    {
        //        //可简化
        //        //根据已有的种类来建立对应的永久库位
        //        var permanentLoc = new PermanentLocation
        //        {
        //            PurchaseOrder = species.PurchaseOrder,
        //            Style = species.Style,
        //            Color = species.Color,
        //            Size = species.Size,
        //            Location = "A-9999-B",
        //            Vender = "SILKICON",
        //            Quantity = 0
        //        };

        //        //查询永久库位中是否有4个属性与刚建立的新对象完全相同的对象
        //        var permanentInDbCheck = permanentLocToList
        //            .SingleOrDefault(c => c.PurchaseOrder == permanentLoc.PurchaseOrder
        //                && c.Style == permanentLoc.Style
        //                && c.Color == permanentLoc.Color
        //                && c.Size == permanentLoc.Size);

        //        //若没有，则添加进永久性库位
        //        if (permanentInDbCheck == null)
        //        {
        //            permanentLocList.Add(permanentLoc);
        //        }
        //    }

        //    context.PermanentLocations.AddRange(permanentLocList);
        //    context.SaveChanges();
        //}
    }
}