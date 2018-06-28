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
        public void CreatePermanentLocForEachSpecies(ApplicationDbContext context)
        {
            var speciesInDb = context.SpeciesInventories.Where(c => c.Id > 0);
            var permanentLocList = new List<PermanentLocation>();

            foreach(var species in speciesInDb)
            {
                permanentLocList.Add(new PermanentLocation {
                    PurchaseOrder = "TEST",
                    Style = species.Style,
                    Color = species.Color,
                    Size = species.Size,
                    Location = "A-9999-B",
                    Vender = "SILKICON",
                    Quantity = 0
                });
            }

            context.PermanentLocations.AddRange(permanentLocList);
            context.SaveChanges();
        }
    }
}