using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Helpers
{
    public class ChargingItemGenerator
    {
        private ApplicationDbContext _context;

        public ChargingItemGenerator()
        {
            _context = new ApplicationDbContext();
        }

        //当新FBA用户被建立时调用此方法，自动为每个客户添加默认的价格
        public void GenerateChargingItems(ApplicationDbContext context, UpperVendor customer)
        {
            var chargingItemList = new List<ChargingItem>();

            //接收服务
            chargingItemList.Add(new ChargingItem {
                ChargingType = "Receiving",
                Name = "打托整柜货FCL/FTL",
                Unit = "PLT",
                Rate = 10,
                Description = "货物接收服务包括接收预报入库*的货物，不包括入库理货。每个托盘货物毛重不得大于660KGS (1455 lb)；其他情况另议。",
                UpperVendor = customer
            });

            chargingItemList.Add(new ChargingItem
            {
                ChargingType = "Receiving",
                Name = "20GP散箱整柜货",
                Unit = "CONTAINER",
                Rate = 0,
                Description = "货物接收服务包括接收预报入库*的货物，不包括入库理货。",
                UpperVendor = customer
            });

            chargingItemList.Add(new ChargingItem
            {
                ChargingType = "Receiving",
                Name = "40GP散箱整柜货",
                Unit = "CONTAINER",
                Rate = 350,
                Description = "货物接收服务包括接收预报入库*的货物，不包括入库理货。",
                UpperVendor = customer
            });

            chargingItemList.Add(new ChargingItem
            {
                ChargingType = "Receiving",
                Name = "40HC散箱整柜货",
                Unit = "CONTAINER",
                Rate = 350,
                Description = "货物接收服务包括接收预报入库*的货物，不包括入库理货。",
                UpperVendor = customer
            });

            chargingItemList.Add(new ChargingItem
            {
                ChargingType = "Receiving",
                Name = "45GP散箱整柜货",
                Unit = "CONTAINER",
                Rate = 0,
                Description = "货物接收服务包括接收预报入库*的货物，不包括入库理货。",
                UpperVendor = customer
            });

            chargingItemList.Add(new ChargingItem
            {
                ChargingType = "Receiving",
                Name = "收货文件服务",
                Unit = "ORDER",
                Rate = 0,
                Description = "收货文件服务包括接收客人提供的装箱清单Packing List、入库单，准备收货单等相关收货文件。",
                UpperVendor = customer
            });

            //入库理货服务
            chargingItemList.Add(new ChargingItem
            {
                ChargingType = "Operation",
                Name = "入库理货服务",
                Unit = "CTN",
                Rate = 0,
                Description = "入仓理货服务包含按照分货标理货、按照装箱单/入库单清点、存储至库位或上架。适用于货柜/卡车到货的单SKU货品 或 带有分货标的多SKU货品。多SKU货品入仓，分货标必须符合仓库理货标准*，否则加收特殊理货附加费1。",
                UpperVendor = customer
            });

            chargingItemList.Add(new ChargingItem
            {
                ChargingType = "Operation",
                Name = "特殊理货附加费1",
                Unit = "CTN",
                Rate = 0.5,
                Description = "需要理货，但分货标*不符合仓库理货标准 或 无分货标的货品额外收取特殊理货附加费1。",
                UpperVendor = customer
            });

            chargingItemList.Add(new ChargingItem
            {
                ChargingType = "Operation",
                Name = "特殊理货附加费2",
                Unit = "CTN",
                Rate = 0.3,
                Description = "货品计划入库数量和实际到货数量不符，默认以仓库清点的实际到货数量为准。客户若要求仓库安排重新清点实际到货数量*，则加收特殊理货附加费2（费用计数按实际到货数量）。",
                UpperVendor = customer
            });

            chargingItemList.Add(new ChargingItem
            {
                ChargingType = "Operation",
                Name = "特殊理货附加费3",
                Unit = "CTN",
                Rate = 0.3,
                Description = "乱序摆放货物、不同的SKU或者FBA SHIPMENT ID混装在集装箱/车厢中不同的位置会造成卸货理货困难，按实际总数计算，加收特殊理货附加费3。",
                UpperVendor = customer
            });

            chargingItemList.Add(new ChargingItem
            {
                ChargingType = "Operation",
                Name = "箱数超标附加费",
                Unit = "CTN",
                Rate = 0.3,
                Description = "箱数大于1000箱时，超出部分每箱加收箱数超标附加费。",
                UpperVendor = customer
            });

            chargingItemList.Add(new ChargingItem
            {
                ChargingType = "Operation",
                Name = "分货附加费",
                Unit = "SKU",
                Rate = 5,
                Description = "同一票货物入仓有10个以上不同品种需要分货打托，从第10个品种起，每额外品种加收$5.00。",
                UpperVendor = customer
            });

            chargingItemList.Add(new ChargingItem
            {
                ChargingType = "Operation",
                Name = "超重附加费",
                Unit = "OTHER",
                Rate = 0.5,
                Description = "单箱重量大于30KG，超出部分以5KG为单位加收$0.50/5KG。",
                UpperVendor = customer
            });

            //仓储服务
            chargingItemList.Add(new ChargingItem
            {
                ChargingType = "Storage",
                Name = "转运货物临时仓储服务",
                Unit = "STORAGE",
                Rate = 0,
                Description = "1.适用于到仓前已经向仓库下达发货指令的货物；2.货物必须用托盘装载储存。收货时未打托的货品必须打托才可进入转运货物临时仓储区；3.价格仅适用于标准托盘 *，特殊情况需要提前商议确认",
                UpperVendor = customer
            });

            //出库操作服务
            chargingItemList.Add(new ChargingItem
            {
                ChargingType = "Shipping",
                Name = "打托+整托出库服务",
                Unit = "PLT",
                Rate = 14,
                Description = "适用于已经打托的货物。服务包括从库位拉货和装车；不包括准备出库文件、打托和卡车运输*。",
                UpperVendor = customer
            });

            chargingItemList.Add(new ChargingItem
            {
                ChargingType = "Shipping",
                Name = "整托出库服务",
                Unit = "PLT",
                Rate = 4,
                Description = "适用于已经打托的货物。服务包括从库位拉货和装车；不包括准备出库文件、打托和卡车运输*。 该服务最低收费$20.00/单",
                UpperVendor = customer
            });

            chargingItemList.Add(new ChargingItem
            {
                ChargingType = "Shipping",
                Name = "发货文件服务",
                Unit = "ORDER",
                Rate = 14,
                Description = "发货文件服务包括接收客人发货指令/转运单/订单、出具拣货单、系统批量签出、制作提单和向客人发送交货证明POD等。第一单免费。如果客人出具BOL，此服务免费。",
                UpperVendor = customer
            });

            context.ChargingItems.AddRange(chargingItemList);
        }
    }
}