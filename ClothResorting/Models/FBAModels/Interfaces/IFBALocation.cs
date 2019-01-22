using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClothResorting.Models.FBAModels.Interfaces
{
    interface IFBALocation
    {
        string Container { get; set; }

        string ShipmentId { get; set; }

        string AmzRefId { get; set; }

        string HowToDeliver { get; set; }

        string WarehouseCode { get; set; }
    }
}
