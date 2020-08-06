using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Models.StaticClass
{
    public static class StaticCollections
    {
        public static List<string> requestIds;

        public static IList<string> GetRequestIds()
        {
            return requestIds;
        }

        public static void AddRequestId(string id)
        {
            requestIds.Add(id);
        }
    }
}