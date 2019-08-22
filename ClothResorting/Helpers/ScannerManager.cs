using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CoreScanner;

namespace ClothResorting.Helpers
{
    public class ScannerManager
    {
        CCoreScannerClass m_pCoreScanner;

        public ScannerManager()
        {
            m_pCoreScanner = new CCoreScannerClass(); //create COM object
        }

        public string Scan()
        {
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
            return outXML;
        }

        void OnBarcodeEvent(short eventType, ref string pscanData)
        {
            string barcode = pscanData;
        }

        /* Event registration for COM Service */
        //m_pCoreScanner.ImageEvent += new
        //CoreScanner._ICoreScannerEvents_ImageEventEventHandler(OnImageEvent);
        //m_pCoreScanner.VideoEvent += new
        //CoreScanner._ICoreScannerEvents_VideoEventEventHandler(OnVideoEvent);
        //m_pCoreScanner.BarcodeEvent += new _ICoreScannerEvents_BarcodeEventEventHandler(OnBarcodeEvent);
        //m_pCoreScanner.PNPEvent += new
        //CoreScanner._ICoreScannerEvents_PNPEventEventHandler(OnPNPEvent);
        //m_pCoreScanner.ScanRMDEvent += new
        //CoreScanner._ICoreScannerEvents_ScanRMDEventEventHandler(OnScanRMDEvent);
        //m_pCoreScanner.CommandResponseEvent += new
        //CoreScanner._ICoreScannerEvents_CommandResponseEventEventHandler(OnCommandResponseEvent);
        //m_pCoreScanner.IOEvent += new
        //CoreScanner._ICoreScannerEvents_IOEventEventHandler(OnIOEvent);
    }
}