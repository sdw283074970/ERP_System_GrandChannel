using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.Helpers
{
    public class CartonsCalculator
    {
        private ApplicationDbContext _context;

        public CartonsCalculator()
        {
            _context = new ApplicationDbContext();
        }

        //按照PO, Style, Size, Color筛选出CartonBreakdown的结果List, 再通过给出需求件数, 得出RetrievingRecord结果列表, 这个列表就是取货单列表
        public IEnumerable<RetrievingRecord> CalculateCartons (List<CartonBreakDown> cartonInDb, int? targetQuantity, LoadPlanRecord loadPlan)
        {
            var result = new List<RetrievingRecord>();
            var index = 0;
            var originalTargetQuantity = targetQuantity;
            
            //存货先进先出
            cartonInDb.OrderBy(c => c.ReceivedDate);

            while(targetQuantity > 0 && index < cartonInDb.Count)
            {
                var sumOfCartons = 0;
                var isOpened = false;
                int? retrievedPcs = 0;    //实际拿出来的货
                var carton = cartonInDb[index];
                var available = carton.AvailablePcs;
                var pcsPerCtn = carton.PcsPerCartons;
                //如果该项储存单位cartonbreakdown存货小于取货目标数，则把箱子全拿走
                if (available - targetQuantity <= 0)
                {
                    //检查是否有打开的箱子
                    isOpened = CheckIfOpened(available, pcsPerCtn);

                    //向上取整获得箱数
                    sumOfCartons = (int)Math.Ceiling((double)carton.AvailablePcs / (double)carton.PcsPerCartons);

                    retrievedPcs = carton.AvailablePcs;

                    //将结果传入总结果
                    result.Add(new RetrievingRecord
                    {
                        Location = carton.Location,
                        PurchaseOrderNumber = carton.PurchaseNumber,
                        Style = carton.Style,
                        Color = carton.Color,
                        Size = carton.Size,
                        TargetPcs = originalTargetQuantity,
                        IsOpened = isOpened,
                        RetrivedPcs = retrievedPcs,
                        TotalOfCartons = carton.CartonNumberRangeTo - carton.CartonNumberRangeFrom + 1,
                        NumberOfCartons = sumOfCartons,
                        RetrievedDate = DateTime.Today,
                        LoadPlanRecord = loadPlan,
                        ShoulReturnPcs = (int)retrievedPcs - (int)originalTargetQuantity
                    });

                    //分别在cartonDetail、packingList、PrereceiveOrder相关Pcs存货中减去拿走的件数
                    SyncPcs(carton, carton.AvailablePcs);

                    //分别在cartonDetail、packingList、PrereceiveOrder相关Pcs存货中减去拿走的件数
                    SyncCtns(carton, sumOfCartons);

                    carton.AvailablePcs = 0;
                    index += 1;

                    loadPlan.OutBoundCtns += sumOfCartons;
                    loadPlan.OutBoundPcs += (int)retrievedPcs;

                    _context.SaveChanges();
                }
                //如果该项储存单位cartonbreakdown存货大于取货目标数，则计算具体拿几箱
                else
                {
                    //检查是否有打开的箱子
                    isOpened = CheckIfOpened(available, pcsPerCtn);

                    //如果有打开的箱子，则优先拿走打开的箱子，再拿其他的
                    if (isOpened)
                    {
                        //查询打开的箱子中还有多少件衣服
                        var res = carton.AvailablePcs % carton.PcsPerCartons;

                        sumOfCartons += 1;      //直接拿走打开的箱子
                        targetQuantity -= res;

                        //再算上剩下应该拿的箱子
                        sumOfCartons += (int)Math.Ceiling((double)targetQuantity / (double)pcsPerCtn);
                        retrievedPcs = sumOfCartons * pcsPerCtn + res;

                        result.Add(new RetrievingRecord {
                            Location = carton.Location,
                            PurchaseOrderNumber = carton.PurchaseNumber,
                            Style = carton.Style,
                            Color = carton.Color,
                            Size = carton.Size,
                            TargetPcs = originalTargetQuantity,
                            IsOpened = isOpened,
                            RetrivedPcs = retrievedPcs,
                            TotalOfCartons = carton.CartonNumberRangeTo - carton.CartonNumberRangeFrom + 1,
                            NumberOfCartons = sumOfCartons,
                            RetrievedDate = DateTime.Today,
                            LoadPlanRecord = loadPlan,
                            ShoulReturnPcs = (int)retrievedPcs - (int)originalTargetQuantity
                        });

                        carton.AvailablePcs -= retrievedPcs;
                        SyncPcs(carton, retrievedPcs);
                        SyncCtns(carton, sumOfCartons);
                        index += 1;

                        loadPlan.OutBoundCtns += sumOfCartons;
                        loadPlan.OutBoundPcs += (int)retrievedPcs;

                        _context.SaveChanges();
                    }
                    //如果没有打开的箱子，则直接数箱子拿走
                    else
                    {
                        //算出应该拿的箱子数量
                        sumOfCartons += (int)Math.Ceiling((double)targetQuantity / (double)pcsPerCtn);
                        retrievedPcs = sumOfCartons * pcsPerCtn;

                        result.Add(new RetrievingRecord
                        {
                            Location = carton.Location,
                            PurchaseOrderNumber = carton.PurchaseNumber,
                            Style = carton.Style,
                            Color = carton.Color,
                            Size = carton.Size,
                            TargetPcs = originalTargetQuantity,
                            IsOpened = isOpened,
                            RetrivedPcs = retrievedPcs,
                            TotalOfCartons = carton.CartonNumberRangeTo - carton.CartonNumberRangeFrom + 1,
                            NumberOfCartons = sumOfCartons,
                            RetrievedDate = DateTime.Today,
                            LoadPlanRecord = loadPlan,
                            ShoulReturnPcs = (int)retrievedPcs - (int)originalTargetQuantity
                        });

                        carton.AvailablePcs -= retrievedPcs;
                        SyncPcs(carton, retrievedPcs);
                        SyncCtns(carton, sumOfCartons);
                        index += 1;

                        loadPlan.OutBoundCtns += sumOfCartons;
                        loadPlan.OutBoundPcs += (int)retrievedPcs;

                        _context.SaveChanges();
                    }
                }
            }
            return result;
        }

        //用于判断是否有打开的箱子
        private bool CheckIfOpened(int? available, int? pcsPerCtn)
        {
            if (available % pcsPerCtn == 0)
                return false;
            else
                return true;
        }

        //让其他各项减去pcs达到同步数量的目的
        public void SyncPcs(CartonBreakDown carton, int? pcs)
        {
            carton.SilkIconCartonDetail.AvailablePcs -= pcs;
            carton.SilkIconPackingList.AvailablePcs -= pcs;
            carton.SilkIconPackingList.SilkIconPreReceiveOrder.AvailablePcs -= pcs;
        }

        //让其他各项减去Ctns达到同步数量的目的
        public void SyncCtns(CartonBreakDown carton, int? ctns)
        {
            carton.SilkIconCartonDetail.Available -= ctns;
            carton.SilkIconPackingList.Available -= ctns;
            carton.SilkIconPackingList.SilkIconPreReceiveOrder.Available -= ctns;
        }
    }
}