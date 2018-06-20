using ClothResorting.Models;
using ClothResorting.Models.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace ClothResorting.Helpers
{
    public class CartonChecker
    {
        private ApplicationDbContext _context;
        private PreReceiveOrder _preReceive;

        public CartonChecker()
        {
            _context = new ApplicationDbContext();
            _preReceive = _context.PreReceiveOrders
                .Include(c => c.PurchaseOrderSummary.Select(s => s.CartonDetails))
                .OrderByDescending(c => c.Id).First();
        }

        //由于客户给的packinglist中有多种不同style和color的pcs在同一箱的情况，需要算法将这种情况甄别并合并
        public void ReplaceRepeatedEntry()
        {
            var purchaseOrderSummarys = _preReceive.PurchaseOrderSummary.ToList();

            foreach(var pos in purchaseOrderSummarys)
            {
                var cartons = pos.CartonDetails.ToList();
                var validObj = 0;       //即入箱的第一种商品对象的索引

                for (int i = 1; i < cartons.Count; i++)
                {
                    if (cartons[i].SumOfCarton == cartons[validObj].SumOfCarton 
                        && cartons[i].CartonNumberRangeTo == cartons[validObj].CartonNumberRangeTo)
                    {
                        cartons[i].SumOfCarton = 0;
                    }
                    else
                    {
                        validObj = i;
                    }
                }
            }

            _context.SaveChanges();
        }

        //根据CartonDetail内容是否有RunCode来确定Po类型
        public void CheckRunCode()
        {
            var purchaseOrderSummarys = _preReceive.PurchaseOrderSummary.ToList();

            foreach(var pos in purchaseOrderSummarys)
            {
                var cartons = pos.CartonDetails.ToList();

                foreach(var carton in cartons)
                {
                    if (carton.RunCode != "")
                    {
                        pos.OrderType = "Regular";
                    }
                }
            }

            _context.SaveChanges();
        }
    }
}