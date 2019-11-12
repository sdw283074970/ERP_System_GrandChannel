using ClothResorting.Models.FBAModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Helpers.FBAHelper
{
    public class FBAWOHelper
    {
        public IList<FBABOLDetail> GenerateFBABOLList(IEnumerable<FBAPickDetail> pickDetailsInDb)
        {
            var bolList = new List<FBABOLDetail>();

            foreach (var pickDetail in pickDetailsInDb)
            {
                if (pickDetail.FBAPalletLocation != null)
                {
                    var cartonInPickList = pickDetail.FBAPickDetailCartons.ToList();
                    for (int i = 0; i < cartonInPickList.Count; i++)
                    {
                        var plt = 0;
                        var isMainItem = true;
                        //只有托盘中的第一项物品显示托盘数，其他物品不显示并在生成PDF的时候取消表格顶线，99999用于区分是否是同一托盘的非首项
                        if (i == 0)
                        {
                            plt = pickDetail.PltsFromInventory;
                        }
                        else
                        {
                            isMainItem = false;
                        }

                        bolList.Add(new FBABOLDetail
                        {
                            ParentPalletId = pickDetail.FBAPalletLocation.Id,
                            CustomerOrderNumber = cartonInPickList[i].FBACartonLocation.ShipmentId,
                            Contianer = pickDetail.Container,
                            CartonQuantity = cartonInPickList[i].PickCtns,
                            AmzRef = cartonInPickList[i].FBACartonLocation.AmzRefId,
                            ActualPallets = plt,
                            Weight = cartonInPickList[i].FBACartonLocation.GrossWeightPerCtn * cartonInPickList[i].PickCtns,
                            Location = pickDetail.Location,
                            IsMainItem = isMainItem
                        });
                    }
                }
                else
                {
                    bolList.Add(new FBABOLDetail
                    {
                        CustomerOrderNumber = pickDetail.ShipmentId,
                        Contianer = pickDetail.Container,
                        CartonQuantity = pickDetail.ActualQuantity,
                        AmzRef = pickDetail.AmzRefId,
                        ActualPallets = 0,
                        Weight = pickDetail.ActualGrossWeight,
                        Location = pickDetail.Location
                    });
                }
            }

            return bolList;
        }
    }
}