using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Helpers
{
    public class CartonBreaker
    {
        private ApplicationDbContext _context;

        public CartonBreaker(ApplicationDbContext context)
        {
            _context = context;
        }

        //用来将SizeBundle和PcsBundle打碎成独立的条目
        public void BreakCartonBundle(FCRegularLocationDetail cartonLocation)
        {
            var sizeArry = cartonLocation.SizeBundle.Split(' ');
            var pcsArr = cartonLocation.PcsBundle.Split(' ');

            var cartonInsideList = new List<CartonInside>();

            for(int i = 0; i < sizeArry.Count()- 1; i++)
            {
                if (int.Parse(pcsArr[i]) != 0)
                {
                    cartonInsideList.Add(new CartonInside
                    {
                        Size = sizeArry[i],
                        Quantity = int.Parse(pcsArr[i]) * cartonLocation.Cartons,
                        FCRegularLocationDetail = cartonLocation
                    });
                }
            }

            _context.CartonInsides.AddRange(cartonInsideList);
            _context.SaveChanges();
        }
    }
}