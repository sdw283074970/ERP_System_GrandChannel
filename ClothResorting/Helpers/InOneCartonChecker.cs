using ClothResorting.Models;
using ClothResorting.Models.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace ClothResorting.Helpers
{
    public class InOneCartonChecker
    {
        private ApplicationDbContext _context;

        public InOneCartonChecker()
        {
            _context = new ApplicationDbContext();
        }

        //由于客户给的packinglist中有多种不同style和color的pcs在同一箱的情况，需要算法将这种情况甄别并合并
        public void ReplaceRepeatedEntry()
        {
            var preReceive = _context.SilkIconPreReceiveOrders
                .Include(c => c.SilkIconPackingLists.Select(s => s.SilkIconCartonDetails))
                .OrderByDescending(c => c.Id).First();

            var packLists = preReceive.SilkIconPackingLists.ToList();

            foreach(var pl in packLists)
            {
                var cartons = pl.SilkIconCartonDetails.ToList();
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
    }
}