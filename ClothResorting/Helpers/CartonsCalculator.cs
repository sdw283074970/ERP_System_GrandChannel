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
        public IEnumerable<RetrievingRecord> CalculateCartons (List<CartonBreakDown> cartonBreakdownInDb, int targetPcsRemains, LoadPlanRecord loadPlan)
        {
            var result = new List<RetrievingRecord>();
            var index = 0;
            var originalTargetQuantity = targetPcsRemains;
            
            //存货先进先出
            cartonBreakdownInDb.OrderBy(c => c.ReceivedDate).OrderByDescending(c => c.Id);

            //检查是否拿够货
            while(targetPcsRemains > 0 && index < cartonBreakdownInDb.Count)
            {
                //定义局部变量
                var sumOfCartons = 0;
                var isOpened = false;
                int retrievedPcs = 0;    //实际拿出来的货
                var cartonBreakdown = cartonBreakdownInDb[index];
                var available = (int)cartonBreakdown.AvailablePcs;
                var pcsPerCtn = (int)cartonBreakdown.PcsPerCartons;
                int shouldReturn = 0;
                int resPcs = 0;

                //如果该项储存单位cartonbreakdown存货小于取货目标数，则把箱子全拿走
                if (available - targetPcsRemains <= 0)
                {
                    //检查是否有打开的箱子
                    isOpened = CheckIfOpened(available, pcsPerCtn);

                    //向上取整获得箱数
                    sumOfCartons = (int)Math.Ceiling((double)available / (double)cartonBreakdown.PcsPerCartons);

                    retrievedPcs = available;       //所有库存都被拿走

                    //将结果传入总结果
                    result.Add(new RetrievingRecord
                    {
                        Location = cartonBreakdown.Location,
                        PurchaseOrder = cartonBreakdown.PurchaseOrder,
                        Style = cartonBreakdown.Style,
                        Color = cartonBreakdown.Color,
                        Size = cartonBreakdown.Size,
                        TargetPcs = originalTargetQuantity,
                        IsOpened = isOpened,
                        RetrivedPcs = retrievedPcs,
                        TotalOfCartons = cartonBreakdown.CartonNumberRangeTo - cartonBreakdown.CartonNumberRangeFrom + 1,
                        NumberOfCartons = sumOfCartons,
                        RetrievedDate = DateTime.Now,
                        LoadPlanRecord = loadPlan,
                        ShoulReturnPcs = 0,
                        Shortage = 0
                    });

                    targetPcsRemains -= retrievedPcs;
                    cartonBreakdown.AvailablePcs -= retrievedPcs;

                    //分别在cartonDetail、packingList、PrereceiveOrder相关Pcs存货中减去拿走的件数
                    SyncPcs(cartonBreakdown, available);

                    //分别在cartonDetail、packingList、PrereceiveOrder相关Pcs存货中减去拿走的件数
                    SyncCtns(cartonBreakdown, sumOfCartons);
                    
                    loadPlan.OutBoundCtns += sumOfCartons;
                    loadPlan.OutBoundPcs += retrievedPcs;

                    _context.SaveChanges();

                    index += 1;
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
                        resPcs = (int)cartonBreakdown.AvailablePcs % (int)cartonBreakdown.PcsPerCartons;

                        //检查箱子中的数量够不够需求
                        if (resPcs > targetPcsRemains)       //如果比需要的数量多，则只拿pcs不拿箱子，拿了就结束，输出结果
                        {
                            retrievedPcs = targetPcsRemains;
                            sumOfCartons = 0;

                            //输出一个取货记录结果
                            result.Add(new RetrievingRecord
                            {
                                Location = cartonBreakdown.Location,
                                PurchaseOrder = cartonBreakdown.PurchaseOrder,
                                Style = cartonBreakdown.Style,
                                Color = cartonBreakdown.Color,
                                Size = cartonBreakdown.Size,
                                TargetPcs = originalTargetQuantity,
                                IsOpened = isOpened,
                                RetrivedPcs = retrievedPcs,
                                TotalOfCartons = cartonBreakdown.CartonNumberRangeTo - cartonBreakdown.CartonNumberRangeFrom + 1,
                                NumberOfCartons = sumOfCartons,
                                RetrievedDate = DateTime.Now,
                                LoadPlanRecord = loadPlan,
                                ShoulReturnPcs = 0
                            });

                            cartonBreakdown.AvailablePcs -= retrievedPcs;
                            targetPcsRemains -= retrievedPcs;
                            SyncPcs(cartonBreakdown, retrievedPcs);
                            SyncCtns(cartonBreakdown, sumOfCartons);

                            loadPlan.OutBoundCtns += sumOfCartons;
                            loadPlan.OutBoundPcs += retrievedPcs;

                            _context.SaveChanges();

                            index += 1;

                            return result;
                        }
                        else        //否则直接拿走箱子
                        {
                            sumOfCartons += 1;      //直接拿走打开的箱子
                            targetPcsRemains -= resPcs;
                            retrievedPcs = resPcs;
                        }
                    }

                    //如果没有打开的箱子，或已经处理了有打开箱子的情况，则直接数箱子拿走
                    //只有当目标数量没拿够的情况下，才继续对箱数、实际收取pcs做调整，否则直接输出
                    if (targetPcsRemains != 0)
                    {
                        //向上取整，算上剩下应该拿的箱子数量和pcs数量
                        sumOfCartons += (int)Math.Ceiling((double)targetPcsRemains / (double)pcsPerCtn);
                        retrievedPcs = sumOfCartons * pcsPerCtn + resPcs;

                        //如果拿多了且确实之前没拿够，当场开箱还一箱回去，只取出需要的pcs
                        if (retrievedPcs - targetPcsRemains > 0 && targetPcsRemains != 0)
                        {
                            shouldReturn = retrievedPcs - targetPcsRemains;       //需要返回库存的pcs数量
                            retrievedPcs = targetPcsRemains + resPcs;      //只取需要的，即当前部分加上之前已打开的箱子的Pcs
                            sumOfCartons -= 1;      //还箱子
                        }
                    }

                    //输出一个取货记录结果
                    result.Add(new RetrievingRecord
                    {
                        Location = cartonBreakdown.Location,
                        PurchaseOrder = cartonBreakdown.PurchaseOrder,
                        Style = cartonBreakdown.Style,
                        Color = cartonBreakdown.Color,
                        Size = cartonBreakdown.Size,
                        TargetPcs = originalTargetQuantity,
                        IsOpened = isOpened,
                        RetrivedPcs = retrievedPcs,
                        TotalOfCartons = cartonBreakdown.CartonNumberRangeTo - cartonBreakdown.CartonNumberRangeFrom + 1,
                        NumberOfCartons = sumOfCartons,
                        RetrievedDate = DateTime.Now,
                        LoadPlanRecord = loadPlan,
                        ShoulReturnPcs = 0
                    });

                    cartonBreakdown.AvailablePcs -= retrievedPcs;
                    targetPcsRemains -= retrievedPcs;
                    SyncPcs(cartonBreakdown, retrievedPcs);
                    SyncCtns(cartonBreakdown, sumOfCartons);
                    index += 1;

                    loadPlan.OutBoundCtns += sumOfCartons;
                    loadPlan.OutBoundPcs += retrievedPcs;

                    _context.SaveChanges();

                    index += 1;
                }
            }

            //如果目标件数没有剩余或小于0，则说明库存量满足需求，甚至有多的
            if (targetPcsRemains <= 0)
                return result;
            //当目标件数仍然有剩余，则说明库存货物不足
            else
            {
                var carton = cartonBreakdownInDb[0];

                //将短缺信息返回至结果并储存到数据库中
                result.Add(new RetrievingRecord
                {
                    Location = carton.Location,
                    PurchaseOrder = carton.PurchaseOrder,
                    Style = carton.Style,
                    Color = carton.Color,
                    Size = carton.Size,
                    Shortage = targetPcsRemains,
                    RetrievedDate = DateTime.Now
                });
                return result;
            }
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
            carton.CartonDetail.AvailablePcs -= pcs;
            carton.PackingList.AvailablePcs -= pcs;
            carton.PackingList.PreReceiveOrder.AvailablePcs -= pcs;
        }

        //让其他各项减去Ctns达到同步数量的目的
        public void SyncCtns(CartonBreakDown carton, int? ctns)
        {
            carton.CartonDetail.Available -= ctns;
            carton.PackingList.Available -= ctns;
            carton.PackingList.PreReceiveOrder.Available -= ctns;
        }
    }
}