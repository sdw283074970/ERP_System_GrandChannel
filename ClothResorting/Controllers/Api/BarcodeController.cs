using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CoreScanner;

namespace ClothResorting.Controllers.Api
{
    public class BarcodeController : ApiController
    {
        CCoreScannerClass m_pCoreScanner = new CoreScanner.CCoreScannerClass();

        [HttpGet]
        public IHttpActionResult Scan([FromUri]string operation)
        {
            var test = false;

            if (operation == "test")
                test = true;
            
            //Call Open API
            short[] scannerTypes = new short[1];//Scanner Types you are interested in
            scannerTypes[0] = 1; // 1 for all scanner types
            short numberOfScannerTypes = 1; // Size of the scannerTypes array
            int status; // Extended API return code
            m_pCoreScanner.Open(0, scannerTypes, numberOfScannerTypes, out status);



            // Subscribe for barcode events in cCoreScannerClass
            m_pCoreScanner.BarcodeEvent += new
            _ICoreScannerEvents_BarcodeEventEventHandler(OnBarcodeEvent);

            // Let's subscribe for events
            int opcode = 1001; // Method for Subscribe events
            string outXML; // XML Output
            string inXML = "<inArgs>" +
            "<cmdArgs>" +
            "<arg-int>1</arg-int>" + // Number of events you want to subscribe
            "<arg-int>1</arg-int>" + // Comma separated event IDs
            "</cmdArgs>" +
            "</inArgs>";
            m_pCoreScanner.ExecCommand(opcode, ref inXML, out outXML, out status);
            //Console.WriteLine(outXML);
            return Ok(outXML);
        }

        void OnBarcodeEvent(short eventType, ref string pscanData)
        {
            var str = pscanData;
        }
    }
}
