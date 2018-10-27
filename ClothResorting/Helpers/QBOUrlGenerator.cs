using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Helpers
{
    public class QBOUrlGenerator
    {
        public static string QueryRequestUrl(string baseUrl, string realmId, string queryStatement)
        {
            return baseUrl + "/v3/company/" + realmId + "/query?query=" + queryStatement;
        }

        public static string CreateRequestUrl(string baseUrl, string realmId, string entity)
        {
            var requestid = GuidGenerator.GenerateGuid();

            return baseUrl + "/v3/company/" + realmId + "/" + entity + "?requestid=" + requestid;
        }
    }
}