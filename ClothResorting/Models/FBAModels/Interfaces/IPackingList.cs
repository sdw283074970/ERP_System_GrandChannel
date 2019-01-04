using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClothResorting.Models.FBAModels.Interfaces
{
    interface IPackingList
    {
        string Container { get; set; }

        string FBAShipmentId { get; set; }

        string AmzRefId { get; set; }

        string FBACode { get; set; }
    }
}
