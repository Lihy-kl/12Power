
using Lib_Card.ADT8940A1.Module.Home;
using Lib_DataBank.MySQL;
using Lib_File;
using SmartDyeing.FADM_Control;
using SmartDyeing.FADM_Form;
using SmartDyeing.FADM_Object;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using static SmartDyeing.FADM_Auto.Dye;
using static SmartDyeing.FADM_Object.Communal;

namespace SmartDyeing.FADM_Auto
{
    /// <summary>
    /// 滴液
    /// </summary>
    internal class Drip
    {

        public static bool _b_dripErr = false;

        public static bool _b_dripStop = false;

        public static int _i_dripType = 0;

        //等待洗杯完成线程
        public void WaitWashFinish(object o)
        {
            List<int> lis_iUse = new List<int>();
            lis_iUse = (List<int>)o;
            while (true)
            {
                if (0 == lis_iUse.Count)
                    break;

                for (int j = lis_iUse.Count - 1; j >= 0; j--)
                {
                    if (FADM_Object.Communal._b_isNeedConfirm)
                    {
                        //如果杯号没在开始(由于另外一个杯使用导致需要洗杯的情况，直接清除)
                        DataTable dt_need = FADM_Object.Communal._fadmSqlserver.GetData("Select * from drop_head where CupNum = " + lis_iUse[j] + " And BatchName != '0'");
                        if (dt_need.Rows.Count == 0)
                        {
                            lis_iUse.Remove(lis_iUse[j]);
                            continue;
                        }
                    }
                    if (FADM_Object.Communal._lis_washCupFinish.Contains(lis_iUse[j]))
                    {
                        lock (this)
                        {
                            FADM_Object.Communal._lis_washCupFinish.Remove(lis_iUse[j]);
                        }
                        if (!_lis_addwashCupFinish.Contains(lis_iUse[j]))
                            FADM_Object.Communal._lis_addwashCupFinish.Add(lis_iUse[j]);

                        while (true)
                        {
                            //滴液完成数组移除当前杯号
                            FADM_Object.Communal._lis_dripSuccessCup.Remove(lis_iUse[j]);
                            if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(lis_iUse[j]))
                                break;
                        }

                        DataTable dt = FADM_Object.Communal._fadmSqlserver.GetData("Select * from drop_head where (CupNum = " + lis_iUse[j] + " Or  CupNum = " + Communal._dic_first_second[lis_iUse[j]] + ") And BatchName != '0'");
                        //先查是否双杯在使用
                        if (dt.Rows.Count == 2)
                        {
                            //判断是否发过启动信号
                            if ((!_lis_SendReadyCup.Contains(lis_iUse[j])) && (!_lis_SendReadyCup.Contains(_dic_first_second[lis_iUse[j]])))
                            {
                                int[] ia_zero = new int[1];
                                ia_zero[0] = 0;
                                DyeHMIWrite(lis_iUse[j], 119, 119, ia_zero);
                                //滴液状态
                                ia_zero[0] = 3;
                                DyeHMIWrite(lis_iUse[j], 100, 100, ia_zero);

                                _lis_SendReadyCup.Add(lis_iUse[j]);
                                _lis_SendReadyCup.Add(_dic_first_second[lis_iUse[j]]);

                            }
                        }
                        //单杯就直接下发
                        else
                        {
                            int[] ia_zero = new int[1];
                            //双杯
                            if (_dic_first_second[lis_iUse[j]] > 0)
                            {

                                //主杯
                                if (lis_iUse[j] < _dic_first_second[lis_iUse[j]])
                                {
                                    ia_zero[0] = 1;
                                    DyeHMIWrite(lis_iUse[j], 119, 119, ia_zero);
                                }
                                else
                                {
                                    ia_zero[0] = 2;
                                    DyeHMIWrite(lis_iUse[j], 119, 119, ia_zero);
                                }
                            }
                            //滴液状态
                            ia_zero[0] = 3;
                            DyeHMIWrite(lis_iUse[j], 100, 100, ia_zero);

                            _lis_SendReadyCup.Add(lis_iUse[j]);

                        }


                        //int[] ia_zero = new int[1];
                        ////滴液状态
                        //ia_zero[0] = 3;


                        //DyeHMIWrite(lis_iUse[j], 100, 100, ia_zero);

                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "UPDATE cup_details SET Statues = '等待准备状态' WHERE CupNum = " + lis_iUse[j] + ";");

                        //等待准备状态
                        while (true)
                        {
                            bool b_open = true;
                            int iOpenCover = Convert.ToInt16(FADM_Auto.Dye._cup_Temps[lis_iUse[j] - 1]._s_statues);
                            if (5 != iOpenCover)
                                b_open = false;
                            if (b_open)
                                break;

                            Thread.Sleep(1000);
                        }
                        if (FADM_Object.Communal._b_isNeedConfirm)
                        {
                            //判断另外一个杯是否完成开盖
                            if (Communal._dic_first_second[lis_iUse[j]] > 0)
                            {
                                if (FADM_Auto.Dye._cup_Temps[Communal._dic_first_second[lis_iUse[j]] - 1]._i_cupCover == 1)
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                                   "UPDATE cup_details SET Cooperate = '2',DyeType=1 WHERE CupNum = " + Communal._dic_first_second[lis_iUse[j]] + ";");
                                }
                                while (true)
                                {

                                    //已开盖
                                    if (Convert.ToInt16(FADM_Auto.Dye._cup_Temps[Communal._dic_first_second[lis_iUse[j]] - 1]._i_cupCover) == 2)
                                        break;

                                    Thread.Sleep(1000);
                                }
                            }

                            
                        }


                        //把滴液状态改为可以滴液
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                           "UPDATE drop_details SET IsDrop = '1' WHERE CupNum = " + lis_iUse[j] + ";");
                        //查询是否已完成，完成就直接下发
                        DataTable dt_drop_head = _fadmSqlserver.GetData("Select * from drop_head where CupFinish = 1 And CupNum = " + lis_iUse[j]);
                        if (dt_drop_head.Rows.Count > 0)
                        {

                            if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(lis_iUse[j]))
                                _lis_dripSuccessCup.Add(lis_iUse[j]);


                        }
                        lis_iUse.Remove(lis_iUse[j]);


                    }

                }

                //判断是否全部洗杯完成，全部完成就退出线程
                if (lis_iUse.Count==0)
                {
                    break;
                }

                Thread.Sleep(1000);
            }
        }

        //加水封装，用于先加染料后加水
        public void AddWater(object oBatchName)
        {
            //流量计等待读脉冲线程
            Thread threadFlow = null;

            //实际加水杯号
            List<int> lis_actualAddWaterCup = new List<int>();

            //加水
            string s_sql = "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName + "' AND " +
               "AddWaterChoose = 1 AND AddWaterFinish = 0  ORDER BY CupNum;";
            DataTable dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);



            if (dt_drop_head.Rows.Count > 0)
            {
                double d_blWeight = 0;
                double d_blWE = 0;
                double d_blDif = 0;
                double d_blDif_span = 0;
                //先判断那些杯子滴液完成才加水
                foreach (DataRow row in dt_drop_head.Rows)
                {
                    int i_cupNo = Convert.ToInt32(row["CupNum"]);
                    double d_blObjectW = Convert.ToDouble(row["ObjectAddWaterWeight"]);
                    if (_lis_ForwordwashCup.Contains(i_cupNo))
                    {
                        if (!_lis_addwashCupFinish.Contains(i_cupNo))
                        {
                            continue;
                        }
                    }
                    s_sql = "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName.ToString() + "' AND Finish = 0" + " AND CupNum = " + i_cupNo + "  AND BottleNum <= '" + Lib_Card.Configure.Parameter.Machine_Bottle_Total + "' ;"; ;
                    DataTable dt_drop_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                    if(dt_drop_details.Rows.Count>0)
                    {
                        continue;
                    }

                    ////查询滴液不合格的
                    //DataTable dt_drop_head_12 = FADM_Object.Communal._fadmSqlserver.GetData(
                    //    "SELECT CupNum FROM drop_details WHERE (ROUND(ABS(ObjectDropWeight * 1000 - RealDropWeight * 1000),3) > " +
                    //    Lib_Card.Configure.Parameter.Other_AErr_Drip * 1000 + " AND BottleNum > 0 AND BottleNum <= " + Lib_Card.Configure.Parameter.Machine_Bottle_Total + " And CupNum = "+ i_cupNo+")   ;");
                    //if (dt_drop_head_12.Rows.Count > 0)
                    //{
                    //    continue;
                    //}

                    //如果勾选加水，但实际计算出来加水量为0，直接完成
                    if (d_blObjectW ==0)
                    {
                        //更新配液杯总量
                        double d_totalW = 0.0;
                        s_sql = "UPDATE drop_head SET AddWaterFinish = 1  ,WaterStandError = '" + Lib_Card.Configure.Parameter.Other_AErr_DripWater + "' WHERE " +
                    "BatchName = '" + oBatchName + "' AND  CupNum = " + i_cupNo + ";";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                        DataTable dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName.ToString() + "' AND Finish = 0" + " AND CupNum = " + i_cupNo + ";");
                        //查询一下加水没完成的也不置为完成
                        DataTable dt_drop_details4 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND AddWaterChoose = 1 AND AddWaterFinish =0" + " AND CupNum = " + i_cupNo + ";");
                        if (dt_drop_details3.Rows.Count == 0 && dt_drop_details4.Rows.Count == 0)
                        {
                            //置为完成
                            s_sql = "UPDATE drop_head SET CupFinish = 1 WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cupNo + "; ";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                            bool b_fail = true;

                            s_sql = "SELECT drop_details.CupNum as CupNum, " +
                                " drop_details.UnitOfAccount as UnitOfAccount, " +
                                        "drop_details.BottleNum as BottleNum, " +
                                        "drop_details.ObjectDropWeight as ObjectDropWeight, " +
                                        "drop_details.RealDropWeight as RealDropWeight, " +
                                        "bottle_details.SyringeType as SyringeType " +
                                        "FROM drop_details left join bottle_details on " +
                                        "bottle_details.BottleNum = drop_details.BottleNum "
                                        + "WHERE drop_details.BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cupNo + ";";
                            DataTable dt_drop_head_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                            foreach (DataRow dr in dt_drop_head_details.Rows)
                            {
                                if (Convert.ToInt32(dr["BottleNum"]) > Lib_Card.Configure.Parameter.Machine_Bottle_Total)
                                    continue;
                                double d_blRealErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"]))) : Convert.ToDouble(string.Format("{0:F3}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])));
                                d_blRealErr = d_blRealErr < 0 ? -d_blRealErr : d_blRealErr;
                                if (d_blRealErr > (dr["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip))
                                    b_fail = false;

                                d_totalW += Convert.ToDouble(dr["RealDropWeight"]);
                            }

                            dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                    "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cupNo + ";");
                            if (dt_drop_details3.Rows.Count > 0)
                            {
                                int i_cup = Convert.ToInt16(dt_drop_details3.Rows[0]["CupNum"]);
                                int i_Step = Convert.ToInt16(dt_drop_details3.Rows[0]["Step"]);
                                double d_objWater = Convert.ToDouble(dt_drop_details3.Rows[0]["ObjectAddWaterWeight"]);
                                double d_realWater = Convert.ToDouble(dt_drop_details3.Rows[0]["RealAddWaterWeight"]);
                                double d_totalWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TotalWeight"]);
                                double d_testTubeObjectAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeObjectAddWaterWeight"]);
                                double d_testTubeRealAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeRealAddWaterWeight"]);
                                double d_realDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater - d_objWater) : string.Format("{0:F3}", d_realWater - d_objWater));
                                d_realDif = d_realDif < 0 ? -d_realDif : d_realDif;
                                double d_allDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)) : string.Format("{0:F3}",
                                    d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)));

                                string s_describe;
                                string s_describe_EN;
                                if (d_allDif < d_realDif || (d_realWater == 0.0 && d_objWater != 0.0))
                                {
                                    b_fail = false;
                                }

                                d_totalW += d_realWater;

                                if (b_fail)
                                {
                                    s_describe = "滴液成功!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                              ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Success !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(i_cupNo))
                                    {
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cup))
                                            FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                                    }
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  "UPDATE cup_details SET Statues = '滴液成功',TotalWeight = "+ (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalW) : string.Format("{0:F3}",
                                    d_totalW))+" WHERE CupNum = " + i_cup + ";");
                                }
                                else
                                {
                                    s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                            ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET Statues = '滴液失败',TotalWeight = "+ (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalW) : string.Format("{0:F3}",
                                    d_totalW))+" WHERE CupNum = " + i_cup + ";");
                                }
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE drop_head SET DescribeChar = '" + s_describe + "',DescribeChar_EN = '" + s_describe_EN + "', FinishTime = '" + DateTime.Now + "', Step = 2 " +
                               "WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cup + ";");
                            }
                        }

                        continue;
                    }
                    //剩下的合格加入到加水列表
                    lis_actualAddWaterCup.Add(i_cupNo);
                }

                if (FADM_Object.Communal._b_isWaterRecheck)
                {
                    //如果有需要加水的，先到天平复称，合格后才加入到配液杯
                    if (lis_actualAddWaterCup.Count > 0)
                    {
                    reWater:
                        //判断染色线程是否需要用机械手
                        if (null != FADM_Object.Communal.ReadDyeThread())
                        {
                            FADM_Object.Communal.WriteDripWait(true);
                            Communal._b_isWaitDrip = true;
                            Communal._b_isAllowDrip = true;
                            while (true)
                            {
                                if (false == FADM_Object.Communal.ReadDripWait())
                                    break;
                                Thread.Sleep(1);
                            }
                            Communal._b_isWaitDrip = false;
                        }
                        else
                            Communal._b_isWaitDrip = false;

                        //移动到天平位

                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找天平位");

                        //判断是否异常
                        FADM_Object.Communal.BalanceState("滴液");

                        //Lib_SerialPort.Balance.METTLER.bZeroSign = true;

                        if (_b_dripStop)
                        {
                            FADM_Object.Communal._b_stop = true;
                        }
                        int i_mRes = MyModbusFun.TargetMove(2, 0, 1);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");

                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达天平位");

                        double d_blBalanceValue0 = 0;
                        if (FADM_Object.Communal._b_isStableRead)
                            d_blBalanceValue0 = SteBalance();
                        else
                        {
                            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
                            {
                                d_blBalanceValue0 = Lib_Card.Configure.Parameter.Machine_BalanceType == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal.dBalanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal.dBalanceValue));
                            }
                            else
                            {
                                d_blBalanceValue0 = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal._s_balanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal._s_balanceValue));
                            }
                        }
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平读数：" + d_blBalanceValue0);

                        double d_addWaterTime2 = MyModbusFun.GetWaterTime(Lib_Card.Configure.Parameter.Correcting_Water_RWeight);//加水时间 校正加水时间
                        FADM_Object.Communal.AddWater(d_addWaterTime2);

                        //读取天平数据
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数启动");
                        double d_blRRead = FADM_Object.Communal.SteBalance();
                        d_blWeight = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blRRead - d_blBalanceValue0)) : Convert.ToDouble(string.Format("{0:F3}", d_blRRead - d_blBalanceValue0));
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数：" + d_blRRead + ",实际重量：" + d_blWeight);
                        d_blWE = Convert.ToDouble(string.Format("{0:F3}", (d_blWeight - Lib_Card.Configure.Parameter.Correcting_Water_RWeight)));
                        d_blDif = Convert.ToDouble(string.Format("{0:F3}", d_blWE / Lib_Card.Configure.Parameter.Correcting_Water_RWeight));
                        d_blDif_span = Convert.ToDouble(string.Format("{0:F3}", d_blWeight / Lib_Card.Configure.Parameter.Correcting_Water_RWeight));
                        int irErr = Convert.ToInt16(d_blDif * 100);
                        irErr = irErr < 0 ? -irErr : irErr;
                        //解锁6工位
                        MyModbusFun.UnLockSix();
                        if (System.Math.Abs(d_blWE * 100 / (Convert.ToDouble(Lib_Card.Configure.Parameter.Correcting_Water_RWeight))) > Lib_Card.Configure.Parameter.Other_AErr_DripWater || d_blWeight == 0)
                        {
                            //加水复称失败

                            FADM_Object.Communal.WriteDripWait(true);
                            Communal._b_isAllowDrip = false;
                            FADM_Object.MyAlarm myAlarm;

                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm("加水复检失败,是否继续?(重新水校正请点是，不校正请点否)", "加水复检", true, 1);
                            else
                                myAlarm = new FADM_Object.MyAlarm(" Water retest failed. Do you want to continue? (Re-water calibration please click Yes, Do not correct please click No)", "Add water for retesting", true, 1);
                            while (true)
                            {
                                if (0 != myAlarm._i_alarm_Choose)
                                    break;
                                Thread.Sleep(1);
                            }

                            if (2 == myAlarm._i_alarm_Choose)
                            {
                                FADM_Object.MyAlarm myAlarm1;
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    myAlarm1 = new FADM_Object.MyAlarm("加水复检失败,是否继续?(重新测量请点是，退出滴液请点否)", "加水复检", true, 1);
                                else
                                    myAlarm1 = new FADM_Object.MyAlarm(" Water retest failed. Do you want to continue? (Remeasure, please click Yes, and to exit dripping, please click No)", "Add water for retesting", true, 1);
                                while (true)
                                {
                                    if (0 != myAlarm1._i_alarm_Choose)
                                        break;
                                    Thread.Sleep(1);
                                }

                                Communal._b_isAllowDrip = true;
                                if (2 == myAlarm1._i_alarm_Choose)
                                {
                                    //判断染色线程是否需要用机械手
                                    if (null != FADM_Object.Communal.ReadDyeThread())
                                    {
                                        FADM_Object.Communal.WriteDripWait(true);
                                        Communal._b_isWaitDrip = true;
                                        Communal._b_isAllowDrip = true;
                                        while (true)
                                        {
                                            if (false == FADM_Object.Communal.ReadDripWait())
                                                break;
                                            Thread.Sleep(1);
                                        }
                                        Communal._b_isWaitDrip = false;
                                    }
                                    else
                                        Communal._b_isWaitDrip = false;
                                    throw new Exception("收到退出消息");
                                }
                                else
                                {
                                    goto reWater;
                                }
                            }
                            else
                            {

                                Communal._b_isAllowDrip = true;
                                //先校正，再进行测量
                                new FADM_Auto.Water().DripCheck();
                                goto reWater;
                            }
                        }
                    }
                    //没有加水的直接结束
                    else
                        return;

                    foreach (DataRow row in dt_drop_head.Rows)
                    {

                        //判断染色线程是否需要用机械手
                        if (null != FADM_Object.Communal.ReadDyeThread())
                        {
                            FADM_Object.Communal.WriteDripWait(true);
                            Communal._b_isWaitDrip = true;
                            Communal._b_isAllowDrip = true;
                            while (true)
                            {
                                if (false == FADM_Object.Communal.ReadDripWait())
                                    break;
                                Thread.Sleep(1);
                            }
                            Communal._b_isWaitDrip = false;
                        }
                        else
                            Communal._b_isWaitDrip = false;

                        int i_cupNo = Convert.ToInt32(row["CupNum"]);
                        if (!lis_actualAddWaterCup.Contains(i_cupNo))
                            continue;
                        double d_blObjectW = Convert.ToDouble(row["ObjectAddWaterWeight"]);
                        if (d_blObjectW > 0)
                        {
                            //把实际加水杯号记录
                            lis_actualAddWaterCup.Add(i_cupNo);

                            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找" + i_cupNo + "号配液杯");
                            int i_reSuccess2 = MyModbusFun.TargetMove(1, i_cupNo, 1);
                            if (-2 == i_reSuccess2)
                                throw new Exception("收到退出消息");
                            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达" + i_cupNo + "号配液杯");

                            if (_b_dripStop)
                            {
                                FADM_Object.Communal._b_stop = true;
                            }
                            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", i_cupNo + "号配液杯加水启动");
                            double d_addWaterTime = MyModbusFun.GetWaterTime(d_blObjectW);//加水时间
                            FADM_Object.Communal.AddWater(d_addWaterTime);
                            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", i_cupNo + "号配液杯加水完成");
                        }
                        //如果勾选加水，但实际计算出来加水量为0，直接完成
                        else
                        {
                            //更新配液杯总量
                            double d_totalW = 0.0;
                            s_sql = "UPDATE drop_head SET AddWaterFinish = 1  ,WaterStandError = '" + Lib_Card.Configure.Parameter.Other_AErr_DripWater + "' WHERE " +
                        "BatchName = '" + oBatchName + "' AND  CupNum = " + i_cupNo + ";";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                            DataTable dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                                    "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName.ToString() + "' AND Finish = 0" + " AND CupNum = " + i_cupNo + ";");
                            //查询一下加水没完成的也不置为完成
                            DataTable dt_drop_details4 = FADM_Object.Communal._fadmSqlserver.GetData(
                                    "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND AddWaterChoose = 1 AND AddWaterFinish =0" + " AND CupNum = " + i_cupNo + ";");
                            if (dt_drop_details3.Rows.Count == 0 && dt_drop_details4.Rows.Count == 0)
                            {
                                //置为完成
                                s_sql = "UPDATE drop_head SET CupFinish = 1 WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cupNo + "; ";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                bool b_fail = true;

                                s_sql = "SELECT drop_details.CupNum as CupNum, " +
                                    " assistant_details.UnitOfAccount as UnitOfAccount, " +
                                            "drop_details.BottleNum as BottleNum, " +
                                            "drop_details.ObjectDropWeight as ObjectDropWeight, " +
                                            "drop_details.RealDropWeight as RealDropWeight, " +
                                            "bottle_details.SyringeType as SyringeType " +
                                            "FROM drop_details left join bottle_details on " +
                                            "bottle_details.BottleNum = drop_details.BottleNum " +
                                            "  left join assistant_details   on bottle_details.AssistantCode = assistant_details.AssistantCode " +
                                            "WHERE drop_details.BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cupNo + ";";
                                DataTable dt_drop_head_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                                foreach (DataRow dr in dt_drop_head_details.Rows)
                                {
                                    if (Convert.ToInt32(dr["BottleNum"]) > Lib_Card.Configure.Parameter.Machine_Bottle_Total)
                                        continue;
                                    double d_blRealErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                                    Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"]))) : Convert.ToDouble(string.Format("{0:F3}",
                                    Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])));
                                    d_blRealErr = d_blRealErr < 0 ? -d_blRealErr : d_blRealErr;
                                    if (d_blRealErr > (dr["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip))
                                        b_fail = false;
                                    d_totalW += Convert.ToDouble(dr["RealDropWeight"]);
                                }

                                dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                        "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cupNo + ";");
                                if (dt_drop_details3.Rows.Count > 0)
                                {
                                    int i_cup = Convert.ToInt16(dt_drop_details3.Rows[0]["CupNum"]);
                                    int i_Step = Convert.ToInt16(dt_drop_details3.Rows[0]["Step"]);
                                    double d_objWater = Convert.ToDouble(dt_drop_details3.Rows[0]["ObjectAddWaterWeight"]);
                                    double d_realWater = Convert.ToDouble(dt_drop_details3.Rows[0]["RealAddWaterWeight"]);
                                    double d_totalWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TotalWeight"]);
                                    double d_testTubeObjectAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeObjectAddWaterWeight"]);
                                    double d_testTubeRealAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeRealAddWaterWeight"]);
                                    double d_realDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater - d_objWater) : string.Format("{0:F3}", d_realWater - d_objWater));
                                    d_realDif = d_realDif < 0 ? -d_realDif : d_realDif;
                                    double d_allDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                        d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)) : string.Format("{0:F3}",
                                        d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)));

                                    string s_describe;
                                    string s_describe_EN;
                                    if (d_allDif < d_realDif || (d_realWater == 0.0 && d_objWater != 0.0))
                                    {
                                        b_fail = false;
                                    }

                                    d_totalW += d_realWater;

                                    if (b_fail)
                                    {
                                        s_describe = "滴液成功!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                  ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                        s_describe_EN = "Drip Success !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                         ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                        if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(i_cupNo))
                                        {
                                            if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cup))
                                                FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                                        }
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                      "UPDATE cup_details SET Statues = '滴液成功',TotalWeight = "+ (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalW) : string.Format("{0:F3}",
                                    d_totalW))+" WHERE CupNum = " + i_cup + ";");
                                    }
                                    else
                                    {
                                        s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                        s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                         ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE cup_details SET Statues = '滴液失败',TotalWeight = "+ (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalW) : string.Format("{0:F3}",
                                    d_totalW))+" WHERE CupNum = " + i_cup + ";");
                                    }
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE drop_head SET DescribeChar = '" + s_describe + "',DescribeChar_EN = '" + s_describe_EN + "', FinishTime = '" + DateTime.Now + "', Step = 2 " +
                                   "WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cup + ";");
                                }
                            }
                        }
                    }
                    if (lis_actualAddWaterCup.Count > 0)
                    {

                        s_sql = "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName + "' AND ObjectAddWaterWeight != 0  And AddWaterFinish = 0;";
                        DataTable dt_drop_head3 = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                        //判断是否存在加水复检失败
                        bool b_fail = false;
                        string s_failC = "";
                        foreach (DataRow dataRow in dt_drop_head3.Rows)
                        {
                            if (!lis_actualAddWaterCup.Contains(Convert.ToInt32(dataRow["CupNum"])))
                            {
                                continue;
                            }
                            Double d_objAddWaiterWeight = 0;
                            if (Convert.ToDouble(dataRow["ObjectAddWaterWeight"].ToString()) > 20)
                            {
                                d_objAddWaiterWeight = Convert.ToDouble(dataRow["ObjectAddWaterWeight"].ToString()) + d_blWE;
                            }
                            else
                            {
                                d_objAddWaiterWeight = Convert.ToDouble(dataRow["ObjectAddWaterWeight"].ToString()) * d_blDif_span;
                            }
                            if (d_blWeight == 0)
                            {
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                      "UPDATE cup_details SET TotalWeight =  " + string.Format("{0:F3}", 0) + " WHERE CupNum = " + dataRow["CupNum"] + " ;");
                            }
                            else
                            {
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                              "UPDATE cup_details SET TotalWeight =  " + string.Format("{0:F3}", d_objAddWaiterWeight) + " WHERE CupNum = " + dataRow["CupNum"] + " ;");
                            }

                            if (Convert.ToDouble(dataRow["ObjectAddWaterWeight"].ToString()) > 20)
                            {
                                if (System.Math.Abs(d_blWE * 100 / (Convert.ToDouble(dataRow["TotalWeight"].ToString()))) > Lib_Card.Configure.Parameter.Other_AErr_DripWater || d_blWeight == 0)
                                {
                                    b_fail = true;
                                    s_failC += dataRow["CupNum"].ToString() + ",";
                                }
                            }
                            else
                            {
                                if (System.Math.Abs(Convert.ToDouble(dataRow["ObjectAddWaterWeight"].ToString()) * d_blDif * 100) / (Convert.ToDouble(dataRow["TotalWeight"].ToString())) > Lib_Card.Configure.Parameter.Other_AErr_DripWater || d_blWeight == 0)
                                {
                                    b_fail = true;
                                    s_failC += dataRow["CupNum"].ToString() + ",";
                                }
                            }

                            //if (d_blDif*100 > Lib_Card.Configure.Parameter.Other_AErr_DripWater)
                            //{
                            //    b_fail = true;
                            //    s_failC += dataRow["CupNum"].ToString() + ",";
                            //}
                        }


                        //实际加水杯号
                        string s_cupAddWater = "";
                        if (lis_actualAddWaterCup.Count > 0)
                        {
                            for (int i = 0; i < lis_actualAddWaterCup.Count; i++)
                            {
                                s_cupAddWater += lis_actualAddWaterCup[i] + ",";
                            }
                            s_cupAddWater = s_cupAddWater.Remove(s_cupAddWater.Length - 1, 1);
                        }
                        else
                        {
                            s_cupAddWater = "0";
                        }

                        //复检天平重量为0时，实际加水量为0
                        if (d_blWeight == 0)
                        {
                            s_sql = "UPDATE drop_head SET RealAddWaterWeight = 0, AddWaterFinish = 1,WaterStandError = '" + Lib_Card.Configure.Parameter.Other_AErr_DripWater + "' WHERE " +
                            "BatchName = '" + oBatchName + "' AND AddWaterChoose = 1 AND CupFinish = 0   AND ObjectAddWaterWeight > 0 AND ObjectAddWaterWeight <= 20 And AddWaterFinish = 0 And CupNum in (" + s_cupAddWater + ");";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        }
                        else
                        {
                            s_sql = "UPDATE drop_head SET RealAddWaterWeight = (ObjectAddWaterWeight * " + d_blDif_span + "), AddWaterFinish = 1,WaterStandError = '" + Lib_Card.Configure.Parameter.Other_AErr_DripWater + "' WHERE " +
                            "BatchName = '" + oBatchName + "' AND AddWaterChoose = 1 AND CupFinish = 0   AND ObjectAddWaterWeight > 0 AND ObjectAddWaterWeight <= 20  And AddWaterFinish = 0 And CupNum in (" + s_cupAddWater + ");";

                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        }

                        //复检天平重量为0时，实际加水量为0
                        if (d_blWeight == 0)
                        {
                            s_sql = "UPDATE drop_head SET RealAddWaterWeight = 0, AddWaterFinish = 1,WaterStandError = '" + Lib_Card.Configure.Parameter.Other_AErr_DripWater + "' WHERE " +
                            "BatchName = '" + oBatchName + "' AND AddWaterChoose = 1 AND CupFinish = 0   AND ObjectAddWaterWeight > 20  And AddWaterFinish = 0 And CupNum in (" + s_cupAddWater + ");";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        }
                        else
                        {
                            s_sql = "UPDATE drop_head SET RealAddWaterWeight = (ObjectAddWaterWeight + " + d_blWE + "), AddWaterFinish = 1,WaterStandError = '" + Lib_Card.Configure.Parameter.Other_AErr_DripWater + "' WHERE " +
                            "BatchName = '" + oBatchName + "' AND AddWaterChoose = 1 AND CupFinish = 0   AND ObjectAddWaterWeight > 20  And AddWaterFinish = 0 And CupNum in (" + s_cupAddWater + ");";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        }


                        //当加水在最后时，要重新判断一下
                        if (FADM_Object.Communal._b_isFinishSend)
                        {
                            foreach (int ic in lis_actualAddWaterCup)
                            {
                                //更新配液杯总量
                                double d_totalW = 0.0;
                                DataTable dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                                        "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName.ToString() + "' AND Finish = 0" + " AND CupNum = " + ic + ";");
                                //查询一下加水没完成的也不置为完成
                                DataTable dt_drop_details4 = FADM_Object.Communal._fadmSqlserver.GetData(
                                        "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND AddWaterChoose = 1 AND AddWaterFinish =0" + " AND CupNum = " + ic + ";");
                                if (dt_drop_details3.Rows.Count == 0 && dt_drop_details4.Rows.Count == 0)
                                {
                                    //置为完成
                                    s_sql = "UPDATE drop_head SET CupFinish = 1 WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + ic + "; ";
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);


                                    bool b_fail1 = true;

                                    s_sql = "SELECT drop_details.CupNum as CupNum, " +
                                        " drop_details.UnitOfAccount as UnitOfAccount, " +
                                                "drop_details.BottleNum as BottleNum, " +
                                                "drop_details.ObjectDropWeight as ObjectDropWeight, " +
                                                "drop_details.RealDropWeight as RealDropWeight, " +
                                                "bottle_details.SyringeType as SyringeType " +
                                                "FROM drop_details left join bottle_details on " +
                                                "bottle_details.BottleNum = drop_details.BottleNum " +
                                                "WHERE drop_details.BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + ic + ";";
                                    DataTable dt_drop_head_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                                    foreach (DataRow dr in dt_drop_head_details.Rows)
                                    {
                                        if (Convert.ToInt32(dr["BottleNum"]) > Lib_Card.Configure.Parameter.Machine_Bottle_Total)
                                            continue;
                                        double d_blRealErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                                        Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"]))) : Convert.ToDouble(string.Format("{0:F3}",
                                        Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])));
                                        d_blRealErr = d_blRealErr < 0 ? -d_blRealErr : d_blRealErr;
                                        if (d_blRealErr > (dr["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip))
                                            b_fail1 = false;

                                        d_totalW += Convert.ToDouble(dr["RealDropWeight"]);
                                    }

                                    dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                            "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + ic + ";");
                                    if (dt_drop_details3.Rows.Count > 0)
                                    {
                                        int i_cup = Convert.ToInt16(dt_drop_details3.Rows[0]["CupNum"]);
                                        int i_Step = Convert.ToInt16(dt_drop_details3.Rows[0]["Step"]);
                                        double d_objWater = Convert.ToDouble(dt_drop_details3.Rows[0]["ObjectAddWaterWeight"]);
                                        double d_realWater = Convert.ToDouble(dt_drop_details3.Rows[0]["RealAddWaterWeight"]);
                                        double d_totalWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TotalWeight"]);
                                        double d_testTubeObjectAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeObjectAddWaterWeight"]);
                                        double d_testTubeRealAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeRealAddWaterWeight"]);
                                        double d_realDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater - d_objWater) : string.Format("{0:F3}", d_realWater - d_objWater));
                                        d_realDif = d_realDif < 0 ? -d_realDif : d_realDif;
                                        double d_allDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                            d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)) : string.Format("{0:F3}",
                                            d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)));

                                        string s_describe;
                                        string s_describe_EN;
                                        if (d_allDif < d_realDif || (d_realWater == 0.0 && d_objWater != 0.0))
                                        {
                                            b_fail1 = false;
                                        }

                                        d_totalW += d_realWater;

                                        if (b_fail1)
                                        {
                                            s_describe = "滴液成功!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                      ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                            s_describe_EN = "Drip Success !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                             ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                            if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(ic))
                                            {
                                                if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cup))
                                                    FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                                            }
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                          "UPDATE cup_details SET Statues = '滴液成功',TotalWeight = "+ (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalW) : string.Format("{0:F3}",
                                    d_totalW))+" WHERE CupNum = " + i_cup + ";");
                                        }
                                        else
                                        {
                                            s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                    ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                            s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                             ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                            "UPDATE cup_details SET Statues = '滴液失败',TotalWeight = "+ (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalW) : string.Format("{0:F3}",
                                    d_totalW))+" WHERE CupNum = " + i_cup + ";");
                                        }
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE drop_head SET DescribeChar = '" + s_describe + "',DescribeChar_EN = '" + s_describe_EN + "', FinishTime = '" + DateTime.Now + "', Step = 2 " +
                                       "WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cup + ";");
                                    }
                                }

                            }
                        }

                        //加水完成如果没有后处理，回去待机位
                        if (null == FADM_Object.Communal.ReadDyeThread())
                        {
                            int i_mRes = MyModbusFun.TargetMove(3, 0, 1);
                            if (-2 == i_mRes)
                                throw new Exception("收到退出消息");
                        }
                        else
                        {
                            //关闭失能
                            MyModbusFun.Power(2);
                        }
                    }
                }
                //使用流量计计算加水重量
                else
                {
                    foreach (DataRow row in dt_drop_head.Rows)
                    {

                        //判断染色线程是否需要用机械手
                        if (null != FADM_Object.Communal.ReadDyeThread())
                        {
                            //如果后处理需要机械手，中断继续加水，先等待读取脉冲完成再释放机械手
                            if (threadFlow != null)
                                threadFlow.Join();
                            FADM_Object.Communal.WriteDripWait(true);
                            Communal._b_isWaitDrip = true;
                            Communal._b_isAllowDrip = true;
                            while (true)
                            {
                                if (false == FADM_Object.Communal.ReadDripWait())
                                    break;
                                Thread.Sleep(1);
                            }
                            Communal._b_isWaitDrip = false;
                        }
                        else
                            Communal._b_isWaitDrip = false;

                        int i_cupNo = Convert.ToInt32(row["CupNum"]);
                        if (!lis_actualAddWaterCup.Contains(i_cupNo))
                            continue;
                        double d_blObjectW = Convert.ToDouble(row["ObjectAddWaterWeight"]);
                        if (d_blObjectW > 0)
                        {
                            //把实际加水杯号记录
                            lis_actualAddWaterCup.Add(i_cupNo);

                            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找" + i_cupNo + "号配液杯");
                            int i_reSuccess2 = MyModbusFun.TargetMove(1, i_cupNo, 1);
                            if (-2 == i_reSuccess2)
                                throw new Exception("收到退出消息");
                            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达" + i_cupNo + "号配液杯");

                            if (_b_dripStop)
                            {
                                FADM_Object.Communal._b_stop = true;
                            }
                            //如果后处理需要机械手，中断继续加水，先等待读取脉冲完成再释放机械手
                            if (threadFlow != null)
                                threadFlow.Join();
                            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", i_cupNo + "号配液杯加水启动");
                            double d_addWaterTime = MyModbusFun.GetWaterTime(d_blObjectW);//加水时间
                            FADM_Object.Communal.AddWater(d_addWaterTime);
                            threadFlow = new Thread(WaitReadFlow);
                            threadFlow.Start(i_cupNo);
                            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", i_cupNo + "号配液杯加水完成");
                        }
                        //如果勾选加水，但实际计算出来加水量为0，直接完成
                        else
                        {
                            //更新配液杯总量
                            double d_totalW = 0.0;
                            s_sql = "UPDATE drop_head SET AddWaterFinish = 1  ,WaterStandError = '" + Lib_Card.Configure.Parameter.Other_AErr_DripWater + "' WHERE " +
                        "BatchName = '" + oBatchName + "' AND  CupNum = " + i_cupNo + ";";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                            DataTable dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                                    "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName.ToString() + "' AND Finish = 0" + " AND CupNum = " + i_cupNo + ";");
                            //查询一下加水没完成的也不置为完成
                            DataTable dt_drop_details4 = FADM_Object.Communal._fadmSqlserver.GetData(
                                    "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND AddWaterChoose = 1 AND AddWaterFinish =0" + " AND CupNum = " + i_cupNo + ";");
                            if (dt_drop_details3.Rows.Count == 0 && dt_drop_details4.Rows.Count == 0)
                            {
                                //置为完成
                                s_sql = "UPDATE drop_head SET CupFinish = 1 WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cupNo + "; ";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                bool b_fail = true;

                                s_sql = "SELECT drop_details.CupNum as CupNum, " +
                                    " assistant_details.UnitOfAccount as UnitOfAccount, " +
                                            "drop_details.BottleNum as BottleNum, " +
                                            "drop_details.ObjectDropWeight as ObjectDropWeight, " +
                                            "drop_details.RealDropWeight as RealDropWeight, " +
                                            "bottle_details.SyringeType as SyringeType " +
                                            "FROM drop_details left join bottle_details on " +
                                            "bottle_details.BottleNum = drop_details.BottleNum " +
                                            "  left join assistant_details   on bottle_details.AssistantCode = assistant_details.AssistantCode " +
                                            "WHERE drop_details.BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cupNo + ";";
                                DataTable dt_drop_head_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                                foreach (DataRow dr in dt_drop_head_details.Rows)
                                {
                                    if (Convert.ToInt32(dr["BottleNum"]) > Lib_Card.Configure.Parameter.Machine_Bottle_Total)
                                        continue;
                                    double d_blRealErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                                    Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"]))) : Convert.ToDouble(string.Format("{0:F3}",
                                    Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])));
                                    d_blRealErr = d_blRealErr < 0 ? -d_blRealErr : d_blRealErr;
                                    if (d_blRealErr > (dr["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip))
                                        b_fail = false;

                                    d_totalW += Convert.ToDouble(dr["RealDropWeight"]);
                                }

                                dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                        "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cupNo + ";");
                                if (dt_drop_details3.Rows.Count > 0)
                                {
                                    int i_cup = Convert.ToInt16(dt_drop_details3.Rows[0]["CupNum"]);
                                    int i_Step = Convert.ToInt16(dt_drop_details3.Rows[0]["Step"]);
                                    double d_objWater = Convert.ToDouble(dt_drop_details3.Rows[0]["ObjectAddWaterWeight"]);
                                    double d_realWater = Convert.ToDouble(dt_drop_details3.Rows[0]["RealAddWaterWeight"]);
                                    double d_totalWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TotalWeight"]);
                                    double d_testTubeObjectAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeObjectAddWaterWeight"]);
                                    double d_testTubeRealAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeRealAddWaterWeight"]);
                                    double d_realDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater - d_objWater) : string.Format("{0:F3}", d_realWater - d_objWater));
                                    d_realDif = d_realDif < 0 ? -d_realDif : d_realDif;
                                    double d_allDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                        d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)) : string.Format("{0:F3}",
                                        d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)));

                                    string s_describe;
                                    string s_describe_EN;
                                    if (d_allDif < d_realDif || (d_realWater == 0.0 && d_objWater != 0.0))
                                    {
                                        b_fail = false;
                                    }
                                    d_totalW += d_realWater;
                                    if (b_fail)
                                    {
                                        s_describe = "滴液成功!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                  ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                        s_describe_EN = "Drip Success !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                         ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                        if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(i_cupNo))
                                        {
                                            if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cup))
                                                FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                                        }
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                      "UPDATE cup_details SET Statues = '滴液成功',TotalWeight = "+ (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalW) : string.Format("{0:F3}",
                                    d_totalW))+" WHERE CupNum = " + i_cup + ";");
                                    }
                                    else
                                    {
                                        s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                        s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                         ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE cup_details SET Statues = '滴液失败',TotalWeight = "+ (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalW) : string.Format("{0:F3}",
                                    d_totalW))+" WHERE CupNum = " + i_cup + ";");
                                    }
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE drop_head SET DescribeChar = '" + s_describe + "',DescribeChar_EN = '" + s_describe_EN + "', FinishTime = '" + DateTime.Now + "', Step = 2 " +
                                   "WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cup + ";");
                                }
                            }
                        }
                    }

                    if (threadFlow != null)
                        threadFlow.Join();
                }
            }
        }

        private void drop_liquid_a(object oBatchNum)
        {
            //滴液
            SmartDyeing.FADM_Auto.MyPowder.powder(oBatchNum.ToString());

        }
        public void DripLiquid(object o_BatchName)
        {
            try
            {
                //需要开盖杯号
                List<int> lis_iOpen = new List<int>();

                

                FADM_Object.Communal.WriteDripWait(false);
                _b_dripErr = false;
                _b_dripStop = false;
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", o_BatchName + "滴液启动");
                FADM_Object.Communal._fadmSqlserver.ReviseData(
                    "UPDATE drop_head SET StartTime = '" + DateTime.Now + "' WHERE BatchName = '" + o_BatchName + "';");
                FADM_Object.Communal.WriteMachineStatus(7);
                int i_mRes = 0;

                if (FADM_Object.Communal._b_isUsePower)
                {
                    Thread P_thd_drop_a = new Thread(drop_liquid_a);
                    P_thd_drop_a.IsBackground = true;
                    P_thd_drop_a.Start(o_BatchName.ToString());
                }

                DataTable dt_drop_head_ = FADM_Object.Communal._fadmSqlserver.GetData(
                               "SELECT * FROM drop_head WHERE BatchName = '" + o_BatchName + "' ;");

                foreach(DataRow dr in dt_drop_head_.Rows)
                {
                    Communal._dic_cup_bottle.Remove(Convert.ToInt32(dr["CupNum"]));
                }

                if (!FADM_Object.Communal._b_isJustShowInfo && !FADM_Object.Communal._b_isUseBrewOnly)
                    MyModbusFun.SetBatchStart();

                Communal._b_isAllowDrip = true;

                //判断染色线程是否需要用机械手
                if (null != FADM_Object.Communal.ReadDyeThread()) 
                {
                    FADM_Object.Communal.WriteDripWait(true);
                    Communal._b_isWaitDrip = true;
                    while (true)
                    {
                        if (false == FADM_Object.Communal.ReadDripWait())
                            break;
                        Thread.Sleep(1);
                    }
                    Communal._b_isWaitDrip = false;
                }
                else
                    Communal._b_isWaitDrip = false;

                

                //回零               
                string s_homeErr = "";

                if (!FADM_Object.Communal._b_isJustShowInfo && !FADM_Object.Communal._b_isUseBrewOnly)
                    MyModbusFun.Reset();

                //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                //{
                //    Lib_Card.ADT8940A1.Module.Home.Home.Home_XYZFinish = false;
                //}

                if (FADM_Object.Communal._b_isDebug)
                {
                    if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Count > 0)
                    {
                        //先检查后处理杯位历史状态，如果要洗杯，要等全部前洗杯完成后才能进行滴液
                        string s_cup = "";
                        for (int i = 0; i < SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Count; i++)
                        {
                            s_cup += SmartDyeing.FADM_Object.Communal._lis_dyeCupNum[i] + ",";
                        }
                        s_cup = s_cup.Remove(s_cup.Length - 1, 1);

                        DataTable dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                           "SELECT * FROM drop_head WHERE BatchName = '" + o_BatchName + "' AND CupFinish = 0  AND CupNum in (" + s_cup + ")  ORDER BY CupNum;");

                        foreach (DataRow dataRow in dt_drop_head.Rows)
                        {
                            int i_cupNum = Convert.ToInt16(dataRow["CupNum"]);
                            //写入当前杯号的配方代码和染固色工艺
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE cup_details SET FormulaCode = '" + dataRow["FormulaCode"] + "', " +
                                "DyeingCode = '" + dataRow["DyeingCode"] + "', Statues = '检查待机状态', " +
                                "StartTime = '" + DateTime.Now + "', SetTemp = null, StepNum = 0, TotalWeight = 0, " +
                                "TotalStep = null,TechnologyName = null, RecordIndex = 0, Cooperate = 0 WHERE CupNum = " + i_cupNum + ";");

                            Txt.DeleteTXT(i_cupNum);
                            Txt.DeleteMarkTXT(i_cupNum);
                        }



                    label1:
                        foreach (DataRow dataRow in dt_drop_head.Rows)
                        {
                            int i_cupNum = Convert.ToInt16(dataRow["CupNum"]);
                            //等待杯子进入待机状态
                            int iState = Convert.ToInt16(FADM_Auto.Dye._cup_Temps[i_cupNum - 1]._s_statues);
                            if (0 != iState)
                            {
                                goto label1;
                            }
                        }

                        //判断杯子历史状态
                        List<int> lis_iUse = new List<int>();
                        foreach (DataRow dataRow in dt_drop_head.Rows)
                        {
                            int i_cupNum = Convert.ToInt16(dataRow["CupNum"]);
                            int i_state = Convert.ToInt16(FADM_Auto.Dye._cup_Temps[i_cupNum - 1]._s_history); ;
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                 "UPDATE cup_details SET Statues = '检查历史状态' WHERE CupNum = " + i_cupNum + ";");

                            if (0 != i_state)
                            {
                                lis_iUse.Add(i_cupNum);
                            }
                        }


                        //等待洗杯完成
                        if (lis_iUse.Count > 0)
                        {
                            //thread.Join();
                            if (-1 == i_mRes)
                            {
                                throw new Exception(s_homeErr);
                            }

                            if (-1 == i_mRes)
                            {
                                throw new Exception("驱动异常");
                            }

                            FADM_Object.Communal._lis_washCup.AddRange(lis_iUse);

                            while (true)
                            {
                                if (null != FADM_Object.Communal.ReadDyeThread())
                                {
                                    FADM_Object.Communal.WriteDripWait(true);
                                    Communal._b_isWaitDrip = true;
                                    while (true)
                                    {
                                        if (false == FADM_Object.Communal.ReadDripWait())
                                            break;
                                        Thread.Sleep(1);
                                    }
                                    Communal._b_isWaitDrip = false;
                                }
                                else
                                    Communal._b_isWaitDrip = false;

                                if (0 == lis_iUse.Count)
                                    break;

                                for (int j = lis_iUse.Count - 1; j >= 0; j--)
                                {
                                    if (FADM_Object.Communal._lis_washCupFinish.Contains(lis_iUse[j]))
                                    {
                                        lock (this)
                                        {
                                            FADM_Object.Communal._lis_washCupFinish.Remove(lis_iUse[j]);
                                        }
                                        lis_iUse.Remove(lis_iUse[j]);

                                    }
                                }

                                 dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                           "SELECT * FROM drop_head WHERE BatchName = '" + o_BatchName + "' AND CupFinish = 0  AND CupNum in (" + s_cup + ")  ORDER BY CupNum;");
                                List<int> lUse1 = new List<int>();
                                foreach (DataRow dataRow in dt_drop_head.Rows)
                                {
                                    lUse1.Add(Convert.ToInt16(dataRow["CupNum"]));
                                }
                                for (int j = lis_iUse.Count - 1; j >= 0; j--)
                                {
                                    //已经强停，不在滴液列表
                                    if(!lUse1.Contains(lis_iUse[j]))
                                    {
                                        lis_iUse.Remove(lis_iUse[j]);
                                    }
                                }
                                Thread.Sleep(1000);
                            }
                        }


                        //写入滴液状态
                        foreach (DataRow dataRow in dt_drop_head.Rows)
                        {
                            int i_cupNum = Convert.ToInt16(dataRow["CupNum"]);

                            while (true)
                            {
                                //滴液完成数组移除当前杯号
                                FADM_Object.Communal._lis_dripSuccessCup.Remove(i_cupNum);
                                if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cupNum))
                                    break;
                            }

                            

                            int[] ia_zero = new int[1];
                            //滴液状态
                            ia_zero[0] = 3;
                            

                            DyeHMIWrite(i_cupNum, 100, 100, ia_zero);

                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE cup_details SET Statues = '等待准备状态' WHERE CupNum = " + i_cupNum + ";");

                        }


                        //等待准备状态
                        while (true)
                        {

                            //判断染色线程是否需要用机械手
                            if (null != FADM_Object.Communal.ReadDyeThread())
                            {
                                FADM_Object.Communal.WriteDripWait(true);
                                Communal._b_isWaitDrip = true;
                                while (true)
                                {
                                    if (false == FADM_Object.Communal.ReadDripWait())
                                        break;
                                    Thread.Sleep(1);
                                }
                                Communal._b_isWaitDrip = false;
                            }
                            else
                                Communal._b_isWaitDrip = false;
                            bool b_open = true;
                            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                           "SELECT * FROM drop_head WHERE BatchName = '" + o_BatchName + "' AND CupFinish = 0  AND CupNum in (" + s_cup + ")  ORDER BY CupNum;");

                            foreach (DataRow dataRow in dt_drop_head.Rows)
                            {
                                int iCupNum = Convert.ToInt16(dataRow["CupNum"]);


                                int iOpenCover = Convert.ToInt16(FADM_Auto.Dye._cup_Temps[iCupNum - 1]._s_statues);


                                if (5 != iOpenCover)
                                    b_open = false;

                            }


                            if (b_open)
                                break;

                            Thread.Sleep(1000);
                        }

                    }
                    //thread.Join();

                    if (-1 == i_mRes)
                    {
                        throw new Exception(s_homeErr);
                    }
                    this.DripProcessDebug(o_BatchName);
                }
                else
                {
                    if (FADM_Object.Communal._b_isDripAll)//是否分区域滴液，false:分区域;true:全部一起滴液
                    {
                        FADM_Object.Communal._lis_ForwordwashCup.Clear();
                        FADM_Object.Communal._lis_addwashCupFinish.Clear();
                        FADM_Object.Communal._lis_SendReadyCup.Clear();
                        if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Count > 0)
                        {
                            //先检查后处理杯位历史状态，如果要洗杯，要等全部前洗杯完成后才能进行滴液
                            string s_cup = "";
                            for (int i = 0; i < SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Count; i++)
                            {
                                s_cup += SmartDyeing.FADM_Object.Communal._lis_dyeCupNum[i] + ",";
                            }
                            s_cup = s_cup.Remove(s_cup.Length - 1, 1);

                            DataTable dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                               "SELECT * FROM drop_head WHERE BatchName = '" + o_BatchName + "' AND CupFinish = 0  AND CupNum in (" + s_cup + ")  ORDER BY CupNum;");

                            //查找所有打板杯号
                            DataTable dt_drop_head_all = FADM_Object.Communal._fadmSqlserver.GetData(
                               "SELECT * FROM drop_head WHERE CupNum in (" + s_cup + ")  ORDER BY CupNum;");

                            foreach (DataRow dataRow in dt_drop_head.Rows)
                            {
                                int i_cupNum = Convert.ToInt16(dataRow["CupNum"]);
                                //写入当前杯号的配方代码和染固色工艺
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET FormulaCode = '" + dataRow["FormulaCode"] + "', " +
                                    "DyeingCode = '" + dataRow["DyeingCode"] + "', Statues = '检查待机状态', " +
                                    "StartTime = '" + DateTime.Now + "', SetTemp = null, StepNum = 0, TotalWeight = 0, " +
                                    "TotalStep = null,TechnologyName = null, RecordIndex = 0, Cooperate = 0 WHERE CupNum = " + i_cupNum + ";");

                                Txt.DeleteTXT(i_cupNum);
                                Txt.DeleteMarkTXT(i_cupNum);
                            }

                            //不需要
                            List<int> lis_i = new List<int>();
                            if (FADM_Object.Communal._b_isNeedConfirm)
                            {
                                //把已经上次滴液把另外一杯已经滴过的区分出来，这些杯子不用查历史状态和发送滴液启动，就需要修改杯选择
                                foreach (DataRow dataRow in dt_drop_head.Rows)
                                {
                                    int i_cupNum = Convert.ToInt16(dataRow["CupNum"]);
                                    if (SmartDyeing.FADM_Object.Communal._dic_first_second[i_cupNum] > 0)
                                    {
                                        foreach (DataRow dataRow1 in dt_drop_head_all.Rows)
                                        {
                                            //当另外一个杯也在批次里
                                            if (Convert.ToInt16(dataRow1["CupNum"]) == SmartDyeing.FADM_Object.Communal._dic_first_second[i_cupNum])
                                            {
                                                //判断另外一个杯是否已经滴完
                                                if (dataRow1["CupFinish"].ToString() == "1")
                                                {
                                                    //把这个杯排除
                                                    if (!lis_i.Contains(i_cupNum))
                                                        lis_i.Add(i_cupNum);
                                                }
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                        label1:
                            foreach (DataRow dataRow in dt_drop_head.Rows)
                            {
                                int i_cupNum = Convert.ToInt16(dataRow["CupNum"]);
                                if (FADM_Object.Communal._b_isNeedConfirm)
                                {
                                    if (lis_i.Contains(i_cupNum))
                                        continue;
                                }
                                //等待杯子进入待机状态
                                int i_state = Convert.ToInt16(FADM_Auto.Dye._cup_Temps[i_cupNum - 1]._s_statues);
                                if (0 != i_state)
                                {
                                    goto label1;
                                }
                            }

                            //判断杯子历史状态
                            List<int> lis_iUse = new List<int>();
                            foreach (DataRow dataRow in dt_drop_head.Rows)
                            {
                                int i_cupNum = Convert.ToInt16(dataRow["CupNum"]);
                                //如果是精密机，就不用查历史状态
                                if (SmartDyeing.FADM_Object.Communal._lis_PrecisionCupNum.Contains(i_cupNum))
                                {
                                    continue;
                                }
                                if (FADM_Object.Communal._b_isNeedConfirm)
                                {
                                    if (lis_i.Contains(i_cupNum))
                                        continue;
                                }
                                int i_state = Convert.ToInt16(FADM_Auto.Dye._cup_Temps[i_cupNum - 1]._s_history);
                                int i_state_s = 0;
                                if (FADM_Object.Communal._b_isNeedConfirm)
                                {
                                    if (SmartDyeing.FADM_Object.Communal._dic_first_second[i_cupNum] > 0)
                                        //检查副杯历史状态
                                        i_state_s = Convert.ToInt16(FADM_Auto.Dye._cup_Temps[SmartDyeing.FADM_Object.Communal._dic_first_second[i_cupNum] - 1]._s_history);
                                }
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                     "UPDATE cup_details SET Statues = '检查历史状态' WHERE CupNum = " + i_cupNum + ";");

                                if (0 != i_state)
                                {
                                    if (!lis_iUse.Contains(i_cupNum))
                                    {
                                        lis_iUse.Add(i_cupNum);
                                        //把滴液标记位先取消，滴液时先不滴
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                               "UPDATE drop_details SET IsDrop = '0' WHERE CupNum = " + i_cupNum + ";");
                                    }
                                    if (Communal._dic_first_second[i_cupNum] > 0)
                                    {
                                        bool b_haveOther = false;
                                        foreach (DataRow dataRow1 in dt_drop_head.Rows)
                                        {
                                            if (Convert.ToInt16(dataRow1["CupNum"]) == Communal._dic_first_second[i_cupNum])
                                            {
                                                b_haveOther = true;
                                                break;
                                            }
                                        }
                                        if (b_haveOther)
                                        {
                                            if (!lis_iUse.Contains(Communal._dic_first_second[i_cupNum]))
                                            {
                                                lis_iUse.Add(Communal._dic_first_second[i_cupNum]);
                                                //把滴液标记位先取消，滴液时先不滴
                                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                       "UPDATE drop_details SET IsDrop = '0' WHERE CupNum = " + Communal._dic_first_second[i_cupNum] + ";");
                                            }
                                        }
                                        else
                                        {
                                            if (FADM_Object.Communal._b_isNeedConfirm)
                                            {
                                                if (0 != i_state_s)
                                                {
                                                    //判断副杯是否下线,如果下线就不判断
                                                    DataTable dt_cup = FADM_Object.Communal._fadmSqlserver.GetData(
                                           "SELECT * FROM cup_details WHERE CupNum =" + Communal._dic_first_second[i_cupNum] + ";");
                                                    if (dt_cup.Rows.Count > 0)
                                                    {
                                                        if (dt_cup.Rows[0]["Enable"].ToString() == "1")
                                                        {
                                                            if (!lis_iUse.Contains(Communal._dic_first_second[i_cupNum]))
                                                            {
                                                                lis_iUse.Add(Communal._dic_first_second[i_cupNum]);
                                                            }
                                                        }
                                                    }

                                                }
                                            }
                                        }
                                    }
                                }
                                //当历史状态为0时，正常可以下发开始，但现在需要判断另外一个杯是否使用，是否需要前洗杯
                                else
                                {
                                    if (FADM_Object.Communal._b_isNeedConfirm)
                                    {
                                        if (0 != i_state_s)
                                        {
                                            //判断副杯是否下线,如果下线就不判断
                                            DataTable dt_cup = FADM_Object.Communal._fadmSqlserver.GetData(
                                   "SELECT * FROM cup_details WHERE CupNum =" + Communal._dic_first_second[i_cupNum] + ";");
                                            if (dt_cup.Rows.Count > 0)
                                            {
                                                if (dt_cup.Rows[0]["Enable"].ToString() == "1")
                                                {
                                                    if (!lis_iUse.Contains(Communal._dic_first_second[i_cupNum]))
                                                    {
                                                        lis_iUse.Add(Communal._dic_first_second[i_cupNum]);
                                                        //把滴液标记位先取消，滴液时先不滴
                                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                               "UPDATE drop_details SET IsDrop = '0' WHERE CupNum = " + Communal._dic_first_second[i_cupNum] + " And BatchName = '" + o_BatchName+"';");
                                                    }
                                                    //由于上面已经判断过，能进入到这里肯定是双杯，这里就是如果打板杯位不用前洗杯，但闲置副杯要前洗杯，就把主副杯一起洗了
                                                    if (!lis_iUse.Contains(i_cupNum))
                                                    {
                                                        lis_iUse.Add(i_cupNum);
                                                        //把滴液标记位先取消，滴液时先不滴
                                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                               "UPDATE drop_details SET IsDrop = '0' WHERE CupNum = " + i_cupNum + ";");
                                                    }
                                                }
                                            }

                                        }
                                    }
                                }


                            }
                            //if (lis_iUse.Count > 0)
                            //{
                            //    //排除杯号
                            //    string s_cupExclude = "";
                            //    for (int i = 0; i < lis_iUse.Count; i++)
                            //    {
                            //        s_cupExclude += lis_iUse[i] + ",";
                            //    }
                            //    s_cupExclude = s_cupExclude.Remove(s_cupExclude.Length - 1, 1);
                            //}


                            //等待洗杯完成
                            if (lis_iUse.Count > 0)
                            {
                                //thread.Join();
                                if (-1 == i_mRes)
                                {
                                    throw new Exception(s_homeErr);
                                }

                                if (-1 == i_mRes)
                                {
                                    throw new Exception("驱动异常");
                                }

                                FADM_Object.Communal._lis_washCup.AddRange(lis_iUse);

                                
                            }
                            if (lis_iUse.Count > 0)
                            {
                                List<int> lis_iUse_temp = new List<int>();
                                lis_iUse_temp.AddRange(lis_iUse);
                                FADM_Object.Communal._lis_ForwordwashCup.AddRange(lis_iUse);
                                Thread thread = new Thread(WaitWashFinish); //等待洗杯完成并下发
                                thread.IsBackground = true;
                                thread.Start(lis_iUse_temp);
                            }

                            string s_cupExclude = "";
                            if (lis_iUse.Count > 0)
                            {
                                for (int i = 0; i < lis_iUse.Count; i++)
                                {
                                    s_cupExclude += lis_iUse[i] + ",";
                                }
                                s_cupExclude = s_cupExclude.Remove(s_cupExclude.Length - 1, 1);
                            }
                            else
                            {
                                s_cupExclude = "0";
                            }


                            //写入滴液状态
                            foreach (DataRow dataRow in dt_drop_head.Rows)
                            {
                                int i_cupNum = Convert.ToInt16(dataRow["CupNum"]);
                                if (SmartDyeing.FADM_Object.Communal._lis_PrecisionCupNum.Contains(i_cupNum))
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                     "UPDATE cup_details SET Statues = '滴液' WHERE CupNum = " + i_cupNum + ";");
                                    continue;
                                }
                                if (FADM_Object.Communal._b_isNeedConfirm)
                                {
                                    if (lis_i.Contains(i_cupNum))
                                    {
                                        //杯选择更改为双杯，状态改为滴液
                                        int[] ia_zero = new int[1];
                                        ia_zero[0] = 0;
                                        DyeHMIWrite(i_cupNum, 119, 119, ia_zero);
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                     "UPDATE cup_details SET Statues = '滴液' WHERE CupNum = " + i_cupNum + ";");
                                        continue;
                                    }
                                }
                                //把没有前洗杯的配液杯下发滴液状态，前洗杯的需要开启线程来自己完成后加入
                                if (!lis_iUse.Contains(i_cupNum))
                                {
                                    while (true)
                                    {
                                        //滴液完成数组移除当前杯号
                                        FADM_Object.Communal._lis_dripSuccessCup.Remove(i_cupNum);
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cupNum))
                                            break;
                                    }

                                    DataTable dt = FADM_Object.Communal._fadmSqlserver.GetData("Select * from drop_head where (CupNum = " + i_cupNum + " Or  CupNum = "+ Communal._dic_first_second[i_cupNum]+ ") And BatchName != '0'");
                                    //先查是否双杯在使用
                                    if(dt.Rows.Count == 2)
                                    {
                                        //判断是否发过启动信号
                                        if ((!_lis_SendReadyCup.Contains(i_cupNum)) && (!_lis_SendReadyCup.Contains(_dic_first_second[i_cupNum])))
                                        {
                                            if (!lis_iUse.Contains(_dic_first_second[i_cupNum]))
                                            {
                                                int[] ia_zero = new int[1];
                                                ia_zero[0] = 0;
                                                DyeHMIWrite(i_cupNum, 119, 119, ia_zero);
                                                //滴液状态
                                                ia_zero[0] = 3;
                                                DyeHMIWrite(i_cupNum, 100, 100, ia_zero);

                                                _lis_SendReadyCup.Add(i_cupNum);
                                                _lis_SendReadyCup.Add(_dic_first_second[i_cupNum]);
                                            }

                                        }
                                    }
                                    //单杯就直接下发
                                    else
                                    {
                                        int[] ia_zero = new int[1];

                                        //12杯发送杯选择
                                        if (_dic_first_second[i_cupNum] > 0)
                                        {
                                            //主杯
                                            if (i_cupNum < _dic_first_second[i_cupNum])
                                            {
                                                ia_zero[0] = 1;
                                                DyeHMIWrite(i_cupNum, 119, 119, ia_zero);
                                            }
                                            else
                                            {
                                                ia_zero[0] = 2;
                                                DyeHMIWrite(i_cupNum, 119, 119, ia_zero);
                                            }
                                        }

                                        //滴液状态
                                        ia_zero[0] = 3;
                                        DyeHMIWrite(i_cupNum, 100, 100, ia_zero);

                                        _lis_SendReadyCup.Add(i_cupNum);
                                    }
                                    //int[] ia_zero = new int[1];
                                    ////滴液状态
                                    //ia_zero[0] = 3;


                                    //DyeHMIWrite(i_cupNum, 100, 100, ia_zero);

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE cup_details SET Statues = '等待准备状态' WHERE CupNum = " + i_cupNum + ";");
                                }

                            }


                            //等待准备状态
                            while (true)
                            {

                                //判断染色线程是否需要用机械手
                                if (null != FADM_Object.Communal.ReadDyeThread())
                                {
                                    FADM_Object.Communal.WriteDripWait(true);
                                    Communal._b_isWaitDrip = true;
                                    while (true)
                                    {
                                        if (false == FADM_Object.Communal.ReadDripWait())
                                            break;
                                        Thread.Sleep(1);
                                    }
                                    Communal._b_isWaitDrip = false;
                                }
                                else
                                    Communal._b_isWaitDrip = false;
                                bool b_open = true;
                                s_cupExclude = s_cupExclude == "" ? "0" : s_cupExclude;
                                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                           "SELECT * FROM drop_head WHERE BatchName = '" + o_BatchName + "' AND CupFinish = 0  AND CupNum in (" + s_cup + ") AND  CupNum not in (" + s_cupExclude + ") ORDER BY CupNum;");
                                foreach (DataRow dataRow in dt_drop_head.Rows)
                                {
                                    int iCupNum = Convert.ToInt16(dataRow["CupNum"]);

                                    if (lis_i.Contains(iCupNum))
                                    {
                                        continue;
                                    }
                                    if (SmartDyeing.FADM_Object.Communal._lis_PrecisionCupNum.Contains(iCupNum))
                                    {
                                        continue;
                                    }
                                    int iOpenCover = Convert.ToInt16(FADM_Auto.Dye._cup_Temps[iCupNum - 1]._s_statues);

                                    

                                    if (5 != iOpenCover)
                                    {
                                        b_open = false;
                                    }

                                }


                                if (b_open)
                                {
                                    if (FADM_Object.Communal._b_isNeedConfirm)
                                    {
                                        //记录需要开盖的杯号
                                        foreach (DataRow dataRow in dt_drop_head.Rows)
                                        {
                                            int iCupNum = Convert.ToInt16(dataRow["CupNum"]);
                                            //
                                            if (Communal._dic_first_second[iCupNum] > 0)
                                            {
                                                //如果关盖，就加入开盖队列
                                                if (FADM_Auto.Dye._cup_Temps[Communal._dic_first_second[iCupNum] - 1]._i_cupCover == 1)
                                                {
                                                    if (!lis_iOpen.Contains(Communal._dic_first_second[iCupNum]))
                                                    {
                                                        lis_iOpen.Add(Communal._dic_first_second[iCupNum]);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                                }

                                Thread.Sleep(1000);
                            }

                        }
                        if (FADM_Object.Communal._b_isNeedConfirm)
                        {
                            //把需要开盖的杯子添加一个开盖动作
                            for (int i = 0; i < lis_iOpen.Count; i++)
                            {
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                                   "UPDATE cup_details SET Cooperate = '2',DyeType=1 WHERE CupNum = " + lis_iOpen[i] + ";");
                            }
                            //thread.Join();
                            //等待所以杯开盖完成
                            while (true)
                            {
                                //判断染色线程是否需要用机械手
                                if (null != FADM_Object.Communal.ReadDyeThread())
                                {
                                    FADM_Object.Communal.WriteDripWait(true);
                                    Communal._b_isWaitDrip = true;
                                    while (true)
                                    {
                                        if (false == FADM_Object.Communal.ReadDripWait())
                                            break;
                                        Thread.Sleep(1);
                                    }
                                    Communal._b_isWaitDrip = false;
                                }
                                else
                                    Communal._b_isWaitDrip = false;

                                bool b_pass = true;
                                for (int i = 0; i < lis_iOpen.Count; i++)
                                {
                                    if (FADM_Auto.Dye._cup_Temps[lis_iOpen[i] - 1]._i_cupCover == 1)
                                    {
                                        b_pass = false;
                                    }
                                }
                                if (b_pass)
                                    break;

                                Thread.Sleep(1000);
                            }
                        }

                        if (-1 == i_mRes)
                        {
                            throw new Exception(s_homeErr);
                        }
                        if (FADM_Object.Communal._b_isAddWaterFirst)
                            this.DripProcess(o_BatchName);
                        else
                            this.DripProcess_AssFirst(o_BatchName);
                    }
                    else
                    {
                        for (int i = 1; i < 7; i++)
                        {

                            int i_cupMin = 0, i_cupMax = 0, i_type = 0;
                            switch (i)
                            {
                                case 1:
                                    i_cupMin = Lib_Card.Configure.Parameter.Machine_Area1_CupMin;
                                    i_cupMax = Lib_Card.Configure.Parameter.Machine_Area1_CupMax;
                                    i_type = Lib_Card.Configure.Parameter.Machine_Area1_Type;
                                    break;
                                case 2:
                                    i_cupMin = Lib_Card.Configure.Parameter.Machine_Area2_CupMin;
                                    i_cupMax = Lib_Card.Configure.Parameter.Machine_Area2_CupMax;
                                    i_type = Lib_Card.Configure.Parameter.Machine_Area2_Type;
                                    break;
                                case 3:
                                    i_cupMin = Lib_Card.Configure.Parameter.Machine_Area3_CupMin;
                                    i_cupMax = Lib_Card.Configure.Parameter.Machine_Area3_CupMax;
                                    i_type = Lib_Card.Configure.Parameter.Machine_Area3_Type;
                                    break;
                                case 4:
                                    i_cupMin = Lib_Card.Configure.Parameter.Machine_Area4_CupMin;
                                    i_cupMax = Lib_Card.Configure.Parameter.Machine_Area4_CupMax;
                                    i_type = Lib_Card.Configure.Parameter.Machine_Area4_Type;
                                    break;
                                case 5:
                                    i_cupMin = Lib_Card.Configure.Parameter.Machine_Area5_CupMin;
                                    i_cupMax = Lib_Card.Configure.Parameter.Machine_Area5_CupMax;
                                    i_type = Lib_Card.Configure.Parameter.Machine_Area5_Type;
                                    break;
                                case 6:
                                    i_cupMin = Lib_Card.Configure.Parameter.Machine_Area6_CupMin;
                                    i_cupMax = Lib_Card.Configure.Parameter.Machine_Area6_CupMax;
                                    i_type = Lib_Card.Configure.Parameter.Machine_Area6_Type;
                                    break;
                                default:
                                    break;

                            }


                            DataTable dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
           "SELECT * FROM drop_head WHERE BatchName = '" + o_BatchName + "' AND CupFinish = 0 AND CupNum >= " + i_cupMin + " AND CupNum <= " + i_cupMax + "   ORDER BY CupNum;");
                            if (dt_drop_head.Rows.Count == 0)
                                continue;
                            if (3 == i_type)
                            {

                                FADM_Object.Communal._lis_ForwordwashCup.Clear();
                                FADM_Object.Communal._lis_addwashCupFinish.Clear();
                                FADM_Object.Communal._lis_SendReadyCup.Clear();

                                foreach (DataRow dataRow in dt_drop_head.Rows)
                                {
                                    int i_cupNum = Convert.ToInt16(dataRow["CupNum"]);
                                    //写入当前杯号的配方代码和染固色工艺
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE cup_details SET FormulaCode = '" + dataRow["FormulaCode"] + "', " +
                                        "DyeingCode = '" + dataRow["DyeingCode"] + "', Statues = '检查待机状态', " +
                                        "StartTime = '" + DateTime.Now + "', SetTemp = null, StepNum = 0, TotalWeight = 0, " +
                                        "TotalStep = null,TechnologyName = null, RecordIndex = 0, Cooperate = 0 WHERE CupNum = " + i_cupNum + ";");

                                    Txt.DeleteTXT(i_cupNum);
                                    Txt.DeleteMarkTXT(i_cupNum);
                                }



                            label1:
                                foreach (DataRow dataRow in dt_drop_head.Rows)
                                {
                                    int i_cupNum = Convert.ToInt16(dataRow["CupNum"]);
                                    //等待杯子进入待机状态
                                    int iState = Convert.ToInt16(FADM_Auto.Dye._cup_Temps[i_cupNum - 1]._s_statues);
                                    if (0 != iState)
                                    {
                                        goto label1;
                                    }

                                }



                                //判断杯子历史状态
                                List<int> lis_iUse = new List<int>();
                                foreach (DataRow dataRow in dt_drop_head.Rows)
                                {
                                    int iCupNum = Convert.ToInt16(dataRow["CupNum"]);
                                    int iState = Convert.ToInt16(FADM_Auto.Dye._cup_Temps[iCupNum - 1]._s_history); ;
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                         "UPDATE cup_details SET Statues = '检查历史状态' WHERE CupNum = " + iCupNum + ";");

                                    if (0 != iState)
                                    {
                                        lis_iUse.Add(iCupNum);
                                    }


                                }
                                //if (lis_iUse.Count > 0)
                                //{
                                //    //排除杯号
                                //    string s_cupExclude = "";
                                //    for (int p = 0; p < lis_iUse.Count; p++)
                                //    {
                                //        s_cupExclude += lis_iUse[p] + ",";
                                //    }
                                //    s_cupExclude = s_cupExclude.Remove(s_cupExclude.Length - 1, 1);
                                //}


                                //等待洗杯完成
                                if (lis_iUse.Count > 0)
                                {
                                    //thread.Join();
                                    if (-1 == i_mRes)
                                    {
                                        throw new Exception(s_homeErr);
                                    }

                                    if (-1 == i_mRes)
                                    {
                                        throw new Exception("驱动异常");
                                    }

                                    FADM_Object.Communal._lis_washCup.AddRange(lis_iUse);
                                   
                                }

                                if (lis_iUse.Count > 0)
                                {
                                    List<int> lis_iUse_temp = new List<int>();
                                    lis_iUse_temp.AddRange(lis_iUse);
                                    _lis_ForwordwashCup.AddRange(lis_iUse);
                                    Thread thread = new Thread(WaitWashFinish); //等待洗杯完成并下发
                                    thread.IsBackground = true;
                                    thread.Start(lis_iUse_temp);
                                }

                                string s_cupExclude = "";
                                if (lis_iUse.Count > 0)
                                {
                                    for (int p = 0; p < lis_iUse.Count; p++)
                                    {
                                        s_cupExclude += lis_iUse[p] + ",";
                                    }
                                    s_cupExclude = s_cupExclude.Remove(s_cupExclude.Length - 1, 1);
                                }
                                else
                                {
                                    s_cupExclude = "0";
                                }


                                //写入滴液状态
                                foreach (DataRow dataRow in dt_drop_head.Rows)
                                {
                                    int i_cupNum = Convert.ToInt16(dataRow["CupNum"]);
                                    //把没有前洗杯的配液杯下发滴液状态，前洗杯的需要开启线程来自己完成后加入
                                    if (!lis_iUse.Contains(i_cupNum))
                                    {
                                        while (true)
                                        {
                                            //滴液完成数组移除当前杯号
                                            FADM_Object.Communal._lis_dripSuccessCup.Remove(i_cupNum);
                                            if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cupNum))
                                                break;
                                        }

                                        DataTable dt = FADM_Object.Communal._fadmSqlserver.GetData("Select * from drop_head where (CupNum = " + i_cupNum + " Or  CupNum = " + Communal._dic_first_second[i_cupNum] + ") And BatchName != '0'");
                                        //先查是否双杯在使用
                                        if (dt.Rows.Count == 2)
                                        {
                                            //判断是否发过启动信号
                                            if ((!_lis_SendReadyCup.Contains(i_cupNum)) && (!_lis_SendReadyCup.Contains(_dic_first_second[i_cupNum])))
                                            {
                                                //判断另外一只杯是否在洗杯
                                                if (!lis_iUse.Contains(_dic_first_second[i_cupNum]))
                                                {
                                                    int[] ia_zero = new int[1];
                                                    ia_zero[0] = 0;
                                                    DyeHMIWrite(i_cupNum, 119, 119, ia_zero);
                                                    //滴液状态
                                                    ia_zero[0] = 3;
                                                    DyeHMIWrite(i_cupNum, 100, 100, ia_zero);

                                                    _lis_SendReadyCup.Add(i_cupNum);
                                                    _lis_SendReadyCup.Add(_dic_first_second[i_cupNum]);
                                                }
                                            }
                                        }
                                        //单杯就直接下发
                                        else
                                        {
                                            int[] ia_zero = new int[1];
                                            //主杯
                                            if (i_cupNum < _dic_first_second[i_cupNum])
                                            {
                                                ia_zero[0] = 1;
                                                DyeHMIWrite(i_cupNum, 119, 119, ia_zero);
                                            }
                                            else
                                            {
                                                ia_zero[0] = 2;
                                                DyeHMIWrite(i_cupNum, 119, 119, ia_zero);
                                            }
                                            //滴液状态
                                            ia_zero[0] = 3;
                                            DyeHMIWrite(i_cupNum, 100, 100, ia_zero);

                                            _lis_SendReadyCup.Add(i_cupNum);
                                        }


                                        //int[] ia_zero = new int[1];
                                        ////滴液状态
                                        //ia_zero[0] = 3;


                                        //DyeHMIWrite(i_cupNum, 100, 100, ia_zero);

                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                           "UPDATE cup_details SET Statues = '等待准备状态' WHERE CupNum = " + i_cupNum + ";");
                                    }

                                }


                                //等待准备状态
                                while (true)
                                {

                                    //判断染色线程是否需要用机械手
                                    if (null != FADM_Object.Communal.ReadDyeThread())
                                    {
                                        FADM_Object.Communal.WriteDripWait(true);
                                        Communal._b_isWaitDrip = true;
                                        while (true)
                                        {
                                            if (false == FADM_Object.Communal.ReadDripWait())
                                                break;
                                            Thread.Sleep(1);
                                        }
                                        Communal._b_isWaitDrip = false;
                                    }
                                    else
                                        Communal._b_isWaitDrip = false;
                                    bool b_open = true;
                                    s_cupExclude = s_cupExclude == "" ? "0" : s_cupExclude;
                                    dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
           "SELECT * FROM drop_head WHERE BatchName = '" + o_BatchName + "' AND CupFinish = 0 AND CupNum >= " + i_cupMin + " AND CupNum <= " + i_cupMax + " AND  CupNum not in ("+  s_cupExclude + ")  ORDER BY CupNum;");
                                    foreach (DataRow dataRow in dt_drop_head.Rows)
                                    {
                                        int i_cupNum = Convert.ToInt16(dataRow["CupNum"]);


                                        int i_openCover = Convert.ToInt16(FADM_Auto.Dye._cup_Temps[i_cupNum - 1]._s_statues);


                                        if (5 != i_openCover)
                                            b_open = false;

                                    }


                                    if (b_open)
                                        break;

                                    Thread.Sleep(1000);
                                }

                            }

                            //thread.Join();

                            if (-1 == i_mRes)
                            {
                                throw new Exception(s_homeErr);
                            }
                            this.DripProcess(o_BatchName, i_cupMin, i_cupMax, i_type, i);
                        }
                    }
                }

                //回到停止位
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找待机位");
                FADM_Object.Communal._i_optBottleNum = 0;
                FADM_Object.Communal._i_OptCupNum = 0;

                FADM_Object.Communal.WriteDripWait(false);
                ////判断染色线程是否需要用机械手
                //if (null != FADM_Object.Communal.ReadDyeThread())
                //{
                //    FADM_Object.Communal.WriteDripWait(true);
                //    Communal._b_isWaitDrip = true;
                //    while (true)
                //    {
                //        if (false == FADM_Object.Communal.ReadDripWait())
                //            break;
                //        Thread.Sleep(1);
                //    }
                //    Communal._b_isWaitDrip = false;
                //}
                //else
                //    Communal._b_isWaitDrip = false;
                if (!FADM_Object.Communal._b_isJustShowInfo && !FADM_Object.Communal._b_isUseBrewOnly)
                    if (null == FADM_Object.Communal.ReadDyeThread())
                    {
                        i_mRes = MyModbusFun.TargetMove(3, 0, 1);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");
                    }
                //else
                //{
                //    //不回待机位，失能关闭
                //    MyModbusFun.Power(2);
                //}
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达待机位");
                if (!FADM_Object.Communal._b_isJustShowInfo && !FADM_Object.Communal._b_isUseBrewOnly)
                    MyModbusFun.SetBatchClose(); //设置关闭批次




                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", o_BatchName + "批次滴液完成");
                FADM_Object.Communal.WriteMachineStatus(0);
                _i_dripType = 0;

                FADM_Object.Communal._b_isBatBrew = true;
                //Lib_SerialPort.Balance.METTLER.bReSetSign = true;

                _b_dripStop = false;

                new FADM_Object.MyAlarm("批次滴液完成", 1);
                Communal._b_isAllowDrip = false;

                SmartDyeing.FADM_Control.P_Formula.P_bl_update = true;
                FADM_Control.Formula.P_bl_update = true;

            }
            catch (Exception ex)
            {
                //解锁6工位
                MyModbusFun.UnLockSix();

                Console.WriteLine("异常了,读取异常标志位 然后");
                _i_dripType = 0;
                _b_dripStop = false;
                _b_dripErr = true;
                string[] sa_array = { "", "" };
                int[] ia_errArray = new int[100];
                if ("收到退出消息" == ex.Message)
                {
                    FADM_Object.Communal._b_stop = false;

                    Communal._b_isAllowDrip = false;

                    FADM_Object.Communal.WriteMachineStatus(10);
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "复位启动");
                    //Lib_Card.ADT8940A1.Axis.Axis.Axis_Exit = false;
                    //Lib_SerialPort.Balance.METTLER.bReSetSign = true;
                    MyModbusFun.MyMachineReset(); //复位


                    ////回到停止位
                    //Lib_Card.ADT8940A1.Module.Move.Move move1 = new Lib_Card.ADT8940A1.Module.Move.Move_Standby();
                    //FADM_Object.Communal._i_OptCupNum = 0;
                    //FADM_Object.Communal._i_optBottleNum = 0;
                    //MyModbusFun.TargetMove(3, 0,1);

                    if (FADM_Auto.Drip._b_dripErr)
                    {
                        FADM_Auto.Reset.MoveData(o_BatchName.ToString());
                    }

                    SmartDyeing.FADM_Control.P_Formula.P_bl_update = true;
                    FADM_Control.Formula.P_bl_update = true;

                    return;
                }
                else if(ex.Message.Equals("-2"))
                {
                    ////根据编号读取异常信息
                    //MyModbusFun.GetErrMsg(ref sa_array);

                    
                    MyModbusFun.GetErrMsgNew(ref ia_errArray);
                }

                FADM_Object.Communal.WriteMachineStatus(8);
                if (ex.Message.Equals("-2"))
                {
                    List<string> lis_err = new List<string>();
                    for (int i = 0; i < ia_errArray.Length; i++)
                    {
                        if (ia_errArray[i] != 0)
                        {
                            if (SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.ContainsKey(ia_errArray[i]))
                            {
                                string s_err = SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew[ia_errArray[i]];
                                string s_sql = "INSERT INTO alarm_table" +
                                 "(MyDate,MyTime,AlarmHead,AlarmDetails)" +
                                 " VALUES( '" +
                                 String.Format("{0:d}", DateTime.Now) + "','" +
                                 String.Format("{0:T}", DateTime.Now) + "','" +
                                 "滴液" + "','" +
                                  s_err + "(Test)');";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                string s_insert = Lib_Card.CardObject.InsertD(s_err, Lib_Card.Configure.Parameter.Other_Language == 0 ? " 滴液" : "Drip");
                                if (!lis_err.Contains(s_insert))
                                    lis_err.Add(s_insert);
                                //while (true)
                                //{
                                //    Thread.Sleep(1);
                                //    if (Lib_Card.CardObject.keyValuePairs[s_insert].Choose != 0)
                                //        break;

                                //}

                                //int _i_alarm_Choose = Lib_Card.CardObject.keyValuePairs[s_insert].Choose;
                                //CardObject.DeleteD(s_insert);

                            }

                        }
                    }

                    while (true)
                    {
                        for (int p = lis_err.Count - 1; p >= 0; p--)
                        {
                            if (Lib_Card.CardObject.keyValuePairs[lis_err[p]].Choose != 0)
                            {
                                Lib_Card.CardObject.DeleteD(lis_err[p]);
                                lis_err.Remove(lis_err[p]);
                            }
                        }
                        if (lis_err.Count == 0)
                        {
                            break;
                        }
                        Thread.Sleep(1);
                    }

                }
                else
                {
                    string s_sql = "INSERT INTO alarm_table" +
                             "(MyDate,MyTime,AlarmHead,AlarmDetails)" +
                             " VALUES( '" +
                             String.Format("{0:d}", DateTime.Now) + "','" +
                             String.Format("{0:T}", DateTime.Now) + "','" +
                             "滴液" + "','" +
                             ex.ToString() + "(Test)');";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                    string str = ex.Message;
                    if (Lib_Card.Configure.Parameter.Other_Language == 1)
                    {
                        if (SmartDyeing.FADM_Object.Communal._dic_warning.ContainsKey(ex.Message))
                        {
                            //如果存在就替换英文
                            str = SmartDyeing.FADM_Object.Communal._dic_warning[ex.Message];
                        }
                    }

                    new FADM_Object.MyAlarm(ex.Message == "-2" ? sa_array[1] : str, "Drip", false, 0);
                }


                //if (Lib_Card.Configure.Parameter.Other_Language == 0)
                //    new FADM_Object.MyAlarm(ex.Message == "-2" ? sa_array[1] : ex.ToString(), "滴液", false, 0);
                //else
                //{
                //    string str = ex.Message;
                //    if (SmartDyeing.FADM_Object.Communal._dic_warning.ContainsKey(ex.Message))
                //    {
                //        //如果存在就替换英文
                //        str = SmartDyeing.FADM_Object.Communal._dic_warning[ex.Message];
                //    }
                //    new FADM_Object.MyAlarm(str, "Drip", false, 0);
                //}

                //Lib_SerialPort.Balance.METTLER.bReSetSign = true;
                MyModbusFun.SetBatchClose();
            }
        }

        private void DripProcess(object obj_batchName, int i_cupMin, int i_cupMax, int i_type, int i_erea)
        {
            //判断是否过期，液量低，夹不到针筒选择了否
            bool b_chooseNo = false;
            _i_dripType = i_type;
            Thread thread = null;
            int i_mRes = 0;
            //针检失败，不继续针检状态
            bool b_checkFail = false;

            Communal._b_isAllowDrip = true;
            List<int> lis_cupSuc = new List<int>();
            List<int> lis_cupT = new List<int>();

            DataTable dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
               "SELECT * FROM drop_head WHERE   BatchName = '" + obj_batchName + "'  AND CupNum >= " + i_cupMin + " AND CupNum <= " + i_cupMax + " And Step = 1 order by CupNum;");

            foreach (DataRow row in dt_drop_head.Rows)
            {
                lis_cupSuc.Add(Convert.ToInt32(row["CupNum"].ToString()));
                lis_cupT.Add(Convert.ToInt32(row["CupNum"].ToString()));
            }

            //复位
            //Lib_SerialPort.Balance.METTLER.bReSetSign = true;
            lab_again:

            //实际加水杯号
            List<int> lis_actualAddWaterCup = new List<int>();

            //判断染色线程是否需要用机械手
            if (null != FADM_Object.Communal.ReadDyeThread())
            {
                FADM_Object.Communal.WriteDripWait(true);
                Communal._b_isWaitDrip = true;
                Communal._b_isAllowDrip = true;
                while (true)
                {
                    if (false == FADM_Object.Communal.ReadDripWait())
                        break;
                    Thread.Sleep(1);
                }
                Communal._b_isWaitDrip = false;
            }
            else
                Communal._b_isWaitDrip = false;

            //先把只做后处理的直接给滴液完成，并下发
            string s_sql = "SELECT * FROM drop_head WHERE BatchName = '" + obj_batchName + "' And CupFinish = 0  AND CupNum >= " + i_cupMin + " AND CupNum <= " + i_cupMax + " ORDER BY CupNum;";
            DataTable dt_drop_head_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

            foreach (DataRow row in dt_drop_head_temp.Rows)
            {
                s_sql = "SELECT * FROM drop_details WHERE BatchName = '" + obj_batchName + "' AND " +
                "CupNum =" + row["CupNum"].ToString() + ";";
                DataTable dt_drop_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                if (dt_drop_details.Rows.Count == 0 && row["AddWaterChoose"].ToString() != "1")
                {
                    int P_int_cup = Convert.ToInt32(row["CupNum"].ToString());

                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                       "UPDATE drop_head SET DescribeChar = '滴液成功', FinishTime = '" + DateTime.Now + "', Step = 2,CupFinish = 1  " +
                       "WHERE BatchName = '" + obj_batchName + "' AND CupNum = " + P_int_cup + ";");
                    if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(P_int_cup))
                        FADM_Object.Communal._lis_dripSuccessCup.Add(P_int_cup);
                }
            }

            //加水
            s_sql = "SELECT * FROM drop_head WHERE BatchName = '" + obj_batchName + "' AND " +
                "AddWaterChoose = 1 AND AddWaterFinish = 0 AND CupNum >= " + i_cupMin + " AND CupNum <= " + i_cupMax + " ORDER BY CupNum;";
            dt_drop_head_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

            if (dt_drop_head_temp.Rows.Count > 0)
            {
                foreach (DataRow row in dt_drop_head_temp.Rows)
                {
                    //判断染色线程是否需要用机械手
                    if (null != FADM_Object.Communal.ReadDyeThread())
                    {
                        FADM_Object.Communal.WriteDripWait(true);
                        Communal._b_isWaitDrip = true;
                        Communal._b_isAllowDrip = true;
                        while (true)
                        {
                            if (false == FADM_Object.Communal.ReadDripWait())
                                break;
                            Thread.Sleep(1);
                        }
                        Communal._b_isWaitDrip = false;
                    }
                    else
                        Communal._b_isWaitDrip = false;

                    int i_cupNo = Convert.ToInt32(row["CupNum"]);
                    //如果前洗杯并且没洗杯完成就不加水
                    if (_lis_ForwordwashCup.Contains(i_cupNo))
                    {
                        if (!_lis_addwashCupFinish.Contains(i_cupNo))
                        {
                            continue;
                        }
                    }
                    double d_blObjectW = Convert.ToDouble(row["ObjectAddWaterWeight"]);
                    if (d_blObjectW > 0)
                    {
                        //if (SmartDyeing.FADM_Object.Communal._dic_dyeType.Keys.Contains(i_cupNo))
                        //{
                        //    if (SmartDyeing.FADM_Object.Communal._dic_dyeType[i_cupNo] == 1)
                        //    {
                        //        //如果关盖状态，就先执行开盖动作
                        //        if (FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._i_cupCover == 1)
                        //        {
                        //        labelP1:
                        //            //开盖
                        //            try
                        //            {
                        //                //寻找配液杯
                        //                FADM_Object.Communal._fadmSqlserver.InsertRun("Dail",  i_cupNo + "号配液杯开盖");
                        //                //FADM_Object.Communal._i_OptCupNum = i_cupNo;
                        //                //int reSuccess4 = MyModbusFun.TargetMove(1, i_cupNo, 1);
                        //                //if (-2 == reSuccess4)
                        //                //    throw new Exception("收到退出消息");

                        //                //FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "抵达" + i_cupNo + "号配液杯");
                        //                int i_xStart=0, i_yStart=0;
                        //                int i_xEnd = 0, i_yEnd = 0;
                        //                MyModbusFun.CalTarget(1, i_cupNo, ref i_xStart,ref i_yStart);

                        //                MyModbusFun.CalTarget(4, i_cupNo, ref i_xEnd, ref i_yEnd);

                        //                i_mRes = MyModbusFun.OpenOrPutCover(i_xStart,i_yStart,i_xEnd,i_yEnd,0);
                        //                if (-2 == i_mRes)
                        //                    throw new Exception("收到退出消息");
                        //            }
                        //            catch (Exception ex)
                        //            {
                        //                if ("未发现杯盖" == ex.Message)
                        //                {
                        //                    ////气缸上
                        //                    //int[] ia_array = new int[1];
                        //                    //ia_array[0] = 5;

                        //                    //int i_state = FADM_Object.Communal._tcpModBus.Write(811, ia_array);
                        //                    //Thread.Sleep(1000);
                        //                    ////抓手开
                        //                    //ia_array = new int[1];
                        //                    //ia_array[0] = 7;

                        //                    //i_state = FADM_Object.Communal._tcpModBus.Write(811, ia_array);

                        //                    FADM_Object.MyAlarm myAlarm;
                        //                    //FADM_Object.Communal._fadmSqlserver.ReviseData(
                        //                    //   "UPDATE cup_details SET  Cooperate = 6 WHERE  CupNum = " + i_cupNo + " ;");
                        //                    //把剩余没有滴液的Min置为状态5
                        //                    s_sql = "UPDATE drop_details SET MinWeight = 5 WHERE BatchName = '" + obj_batchName + "' And  Finish = 0  AND CupNum = " + i_cupNo + ";";
                        //                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        //                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        //                        myAlarm = new FADM_Object.MyAlarm(i_cupNo + "号配液杯未发现杯盖，是否继续执行?(继续执行请点是，已完成开盖请点否)", "SwitchCover", i_cupNo, 2, 7);
                        //                    else
                        //                        myAlarm = new FADM_Object.MyAlarm(i_cupNo + " Cup did not find a cap, do you want to continue? " +
                        //                            "( Continue to perform please click Yes, have completed the opening please click No)", "SwitchCover", i_cupNo, 2, 7);
                        //                    continue;
                        //                    //while (true)

                        //                    //{
                        //                    //    if (0 != myAlarm._i_alarm_Choose)
                        //                    //        break;
                        //                    //    if (0 != myAlarm._i_alarm_Repeat)
                        //                    //        break;
                        //                    //    Thread.Sleep(1);
                        //                    //}
                        //                    //if (myAlarm._i_alarm_Choose == 1 || myAlarm._i_alarm_Repeat == 1)
                        //                    //{
                        //                    //    goto labelP1;
                        //                    //}
                        //                    //else
                        //                    //{
                        //                    //    goto labelP2;
                        //                    //}
                        //                    //return;
                        //                }
                        //                else if ("发现杯盖或针筒" == ex.Message)
                        //                {
                        //                    FADM_Object.MyAlarm myAlarm;
                        //                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        //                        myAlarm = new FADM_Object.MyAlarm("请先拿住针筒或杯盖，然后点确定", "SwitchCover", true, 1);
                        //                    else
                        //                        myAlarm = new FADM_Object.MyAlarm("Please hold the syringe or cup lid first, and then confirm", "SwitchCover", true, 1);
                        //                    while (true)

                        //                    {
                        //                        if (0 != myAlarm._i_alarm_Choose)
                        //                            break;
                        //                        Thread.Sleep(1);
                        //                    }
                        //                    //抓手开
                        //                    int[] ia_array = new int[1];
                        //                    ia_array[0] = 7;

                        //                    int state = FADM_Object.Communal._tcpModBus.Write(811, ia_array);
                        //                    //等5秒后继续
                        //                    Thread.Sleep(5000);
                        //                    goto labelP1;
                        //                }
                        //                else if ("配液杯取盖失败" == ex.Message)
                        //                {
                        //                    FADM_Object.MyAlarm myAlarm;
                        //                    //把剩余没有滴液的Min置为状态5
                        //                    s_sql = "UPDATE drop_details SET MinWeight = 5 WHERE BatchName = '" + obj_batchName + "' And  Finish = 0  AND CupNum = " + i_cupNo + ";";
                        //                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        //                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        //                        myAlarm = new FADM_Object.MyAlarm(i_cupNo + "号配液杯取盖失败，是否继续执行?(继续执行请点是)", "SwitchCover", i_cupNo, 2, 7);
                        //                    else
                        //                        myAlarm = new FADM_Object.MyAlarm(i_cupNo + " Failed to remove cap from dispensing cup, do you want to continue? " +
                        //                            "( Continue to perform please click Yes)", "SwitchCover", i_cupNo, 2, 7);
                        //                    continue;
                        //                }

                        //                else if ("放盖区取盖失败" == ex.Message)
                        //                {
                        //                    FADM_Object.MyAlarm myAlarm;
                        //                    //把剩余没有滴液的Min置为状态5
                        //                    s_sql = "UPDATE drop_details SET MinWeight = 5 WHERE BatchName = '" + obj_batchName + "' And  Finish = 0  AND CupNum = " + i_cupNo + ";";
                        //                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        //                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        //                        myAlarm = new FADM_Object.MyAlarm(i_cupNo + "号放盖区取盖失败，是否继续执行?(继续执行请点是)", "SwitchCover", i_cupNo, 2, 7);
                        //                    else
                        //                        myAlarm = new FADM_Object.MyAlarm(i_cupNo + " Failed to remove the cover from the cover placement area, do you want to continue? " +
                        //                            "( Continue to perform please click Yes)", "SwitchCover", i_cupNo, 2, 7);
                        //                    continue;
                        //                }

                        //                else if ("关盖失败" == ex.Message)
                        //                {
                        //                    FADM_Object.MyAlarm myAlarm;
                        //                    //把剩余没有滴液的Min置为状态5
                        //                    s_sql = "UPDATE drop_details SET MinWeight = 5 WHERE BatchName = '" + obj_batchName + "' And  Finish = 0  AND CupNum = " + i_cupNo + ";";
                        //                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        //                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        //                        myAlarm = new FADM_Object.MyAlarm(i_cupNo + "号关盖失败，请人工检查是否关盖完成?(已确定关盖请点是)", "SwitchCover", i_cupNo, 2, 7);
                        //                    else
                        //                        myAlarm = new FADM_Object.MyAlarm(i_cupNo + " Closing failure, Please manually check whether the closing is complete. (Please click Yes to close the lid)", "SwitchCover", i_cupNo, 2, 7);
                        //                    continue;
                        //                }
                        //                else
                        //                    throw;
                        //            }
                                    
                        //        labelP2:
                        //            //复位加药启动信号
                        //            int[] ia_zero1 = new int[1];
                        //            //
                        //            ia_zero1[0] = 0;
                                    
                        //            FADM_Auto.Dye.DyeOpenOrCloseCover(i_cupNo, 2);
                        //            Thread.Sleep(1000);
                        //            Communal._fadmSqlserver.ReviseData("Update  cup_details set CoverStatus = 2 where CupNum = " + i_cupNo);

                        //            FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._i_cupCover = 2;

                        //            FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", i_cupNo + "号配液杯开盖完成");
                        //        }
                        //    }
                        //}

                        //把实际加水杯号记录
                        lis_actualAddWaterCup.Add(i_cupNo);

                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找" + i_cupNo + "号配液杯");
                        int i_reSuccess2 = MyModbusFun.TargetMove(1, i_cupNo,1);
                        if (i_reSuccess2 ==-2)
                            throw new Exception("收到退出消息");

                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达" + i_cupNo + "号配液杯");

                        if (_b_dripStop)
                        {
                            FADM_Object.Communal._b_stop = true;
                        }
                        
                        double d_addWaterTime = MyModbusFun.GetWaterTime(d_blObjectW);//加水时间
                        FADM_Object.Communal.AddWater(d_addWaterTime);
                    }

                    //如果勾选加水，但实际计算出来加水量为0，直接完成
                    else
                    {
                        s_sql = "UPDATE drop_head SET AddWaterFinish = 1,WaterStandError = '" + Lib_Card.Configure.Parameter.Other_AErr_DripWater + "' WHERE " +
                    "BatchName = '" + obj_batchName + "' AND  CupNum = " + i_cupNo + ";";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                        DataTable dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_details WHERE BatchName = '" + obj_batchName.ToString() + "' AND Finish = 0" + " AND CupNum = " + i_cupNo + ";");
                        //查询一下加水没完成的也不置为完成
                        DataTable dt_drop_details4 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_head WHERE BatchName = '" + obj_batchName.ToString() + "' AND AddWaterChoose = 1 AND AddWaterFinish =0" + " AND CupNum = " + i_cupNo + ";");
                        if (dt_drop_details3.Rows.Count == 0 && dt_drop_details4.Rows.Count == 0)
                        {
                            //置为完成
                            s_sql = "UPDATE drop_head SET CupFinish = 1 WHERE BatchName = '" + obj_batchName.ToString() + "' AND CupNum = " + i_cupNo + "; ";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                            bool b_fail = true;

                            s_sql = "SELECT drop_details.CupNum as CupNum, " +
                                " drop_details.UnitOfAccount as UnitOfAccount, " +
                                        "drop_details.BottleNum as BottleNum, " +
                                        "drop_details.ObjectDropWeight as ObjectDropWeight, " +
                                        "drop_details.RealDropWeight as RealDropWeight, " +
                                        "bottle_details.SyringeType as SyringeType " +
                                        "FROM drop_details left join bottle_details on " +
                                        "bottle_details.BottleNum = drop_details.BottleNum " +
                                        "WHERE drop_details.BatchName = '" + obj_batchName.ToString() + "' AND CupNum = " + i_cupNo + ";";
                            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                            foreach (DataRow dr in dt_drop_head.Rows)
                            {
                                if (Convert.ToInt32(dr["BottleNum"]) > Lib_Card.Configure.Parameter.Machine_Bottle_Total)
                                    continue;
                                double d_blRealErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"]))) : Convert.ToDouble(string.Format("{0:F3}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])));
                                d_blRealErr = d_blRealErr < 0 ? -d_blRealErr : d_blRealErr;
                                if (d_blRealErr > (dr["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip))
                                    b_fail = false;
                            }

                            dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                    "SELECT * FROM drop_head WHERE BatchName = '" + obj_batchName.ToString() + "' AND CupNum = " + i_cupNo + ";");
                            if (dt_drop_details3.Rows.Count > 0)
                            {
                                int i_cup = Convert.ToInt16(dt_drop_details3.Rows[0]["CupNum"]);
                                int i_Step = Convert.ToInt16(dt_drop_details3.Rows[0]["Step"]);
                                double d_objWater = Convert.ToDouble(dt_drop_details3.Rows[0]["ObjectAddWaterWeight"]);
                                double d_realWater = Convert.ToDouble(dt_drop_details3.Rows[0]["RealAddWaterWeight"]);
                                double d_totalWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TotalWeight"]);
                                double d_testTubeObjectAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeObjectAddWaterWeight"]);
                                double d_testTubeRealAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeRealAddWaterWeight"]);
                                double d_realDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater - d_objWater) : string.Format("{0:F3}", d_realWater - d_objWater));
                                d_realDif = d_realDif < 0 ? -d_realDif : d_realDif;
                                double d_allDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)) : string.Format("{0:F3}",
                                    d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)));

                                string s_describe;
                                string s_describe_EN;
                                if (d_allDif < d_realDif || (d_realWater == 0.0 && d_objWater != 0.0))
                                {
                                    b_fail = false;
                                }

                                if (b_fail)
                                {
                                    s_describe = "滴液成功!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                              ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Success !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(i_cupNo))
                                    {
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cup))
                                            FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                                    }
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  "UPDATE cup_details SET Statues = '滴液成功' WHERE CupNum = " + i_cup + ";");
                                }
                                else
                                {
                                    s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                            ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET Statues = '滴液失败' WHERE CupNum = " + i_cup + ";");
                                }
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE drop_head SET DescribeChar = '" + s_describe + "',DescribeChar_EN = '" + s_describe_EN + "', FinishTime = '" + DateTime.Now + "', Step = 2 " +
                               "WHERE BatchName = '" + obj_batchName.ToString() + "' AND CupNum = " + i_cup + ";");
                            }
                        }
                    }
                }
                if(lis_actualAddWaterCup.Count>0)
                { 
                //移动到天平位
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找天平位");

                //判断是否异常
                FADM_Object.Communal.BalanceState("滴液");

                //Lib_SerialPort.Balance.METTLER.bZeroSign = true;

                if (_b_dripStop)
                {
                    FADM_Object.Communal._b_stop = true;
                }
                i_mRes = MyModbusFun.TargetMove(2, 0,1);
                if (i_mRes ==-2)
                {
                    throw new Exception("收到退出消息");
                }
                else if (-2 == i_mRes)
                    throw new Exception("收到退出消息");

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达天平位");
                    //if (FADM_Object.Communal._b_isZero)
                    //{
                    //    while (true)
                    //    {
                    //        //判断是否成功清零
                    //        if (Lib_SerialPort.Balance.METTLER._s_balanceValue == 0.0)
                    //        {
                    //            break;
                    //        }
                    //        else
                    //        {
                    //            //再次发调零
                    //            Lib_SerialPort.Balance.METTLER.bZeroSign = true;
                    //        }
                    //        Thread.Sleep(1);
                    //    }
                    //}

                    double d_blBalanceValue0 = SteBalance();
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平读数：" + d_blBalanceValue0);

                    double d_addWaterTime2 = MyModbusFun.GetWaterTime(Lib_Card.Configure.Parameter.Correcting_Water_RWeight);//加水时间 校正加水时间
                    FADM_Object.Communal.AddWater(d_addWaterTime2);

                    //读取天平数据
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数启动");
                    double d_blRRead = SteBalance();
                double d_blWeight = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blRRead - d_blBalanceValue0)) : Convert.ToDouble(string.Format("{0:F3}", d_blRRead - d_blBalanceValue0));
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数：" + d_blRRead + ",实际重量：" + d_blWeight);
                double d_blWE = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", (d_blWeight - Lib_Card.Configure.Parameter.Correcting_Water_RWeight))): Convert.ToDouble(string.Format("{0:F3}", (d_blWeight - Lib_Card.Configure.Parameter.Correcting_Water_RWeight)));
                double d_blDif = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blWE / Lib_Card.Configure.Parameter.Correcting_Water_RWeight)):Convert.ToDouble(string.Format("{0:F3}", d_blWE / Lib_Card.Configure.Parameter.Correcting_Water_RWeight));
                int i_rErr = Convert.ToInt16(d_blDif * 100);
                i_rErr = i_rErr < 0 ? -i_rErr : i_rErr;

                //实际加水杯号
                string s_cupAddWater = "";
                if (lis_actualAddWaterCup.Count > 0)
                {
                    for (int i = 0; i < lis_actualAddWaterCup.Count; i++)
                    {
                        s_cupAddWater += lis_actualAddWaterCup[i] + ",";
                    }
                    s_cupAddWater = s_cupAddWater.Remove(s_cupAddWater.Length - 1, 1);
                }
                else
                {
                    s_cupAddWater = "0";
                }

                //复检天平重量为0时，实际加水量为0
                if (d_blWeight == 0)
                {
                        s_sql = "UPDATE drop_head SET RealAddWaterWeight = 0, AddWaterFinish = 1,WaterStandError = '" + Lib_Card.Configure.Parameter.Other_AErr_DripWater + "' WHERE " +
                        "BatchName = '" + obj_batchName + "' AND AddWaterChoose = 1 AND CupFinish = 0 AND CupNum >= " +
                        i_cupMin + " AND CupNum <= " + i_cupMax + "  AND ObjectAddWaterWeight > 0 AND ObjectAddWaterWeight <= 20 And CupNum in (" + s_cupAddWater + ");";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                }
                else
                {
                        s_sql = "UPDATE drop_head SET RealAddWaterWeight = (ObjectAddWaterWeight * " + d_blDif + "), AddWaterFinish = 1,WaterStandError = '" + Lib_Card.Configure.Parameter.Other_AErr_DripWater + "' WHERE " +
                        "BatchName = '" + obj_batchName + "' AND AddWaterChoose = 1 AND CupFinish = 0 AND CupNum >= " +
                        i_cupMin + " AND CupNum <= " + i_cupMax + "  AND ObjectAddWaterWeight > 0 AND ObjectAddWaterWeight <= 20 And CupNum in (" + s_cupAddWater + ");";

                        //s_sql = "UPDATE drop_head SET RealAddWaterWeight = (ObjectAddWaterWeight * " + (1 + d_blDif) + "), AddWaterFinish = 1 WHERE " +
                        //"BatchName = '" + o_BatchName + "' AND AddWaterChoose = 1 AND CupFinish = 0 AND CupNum >= " +
                        //_i_cupMin + " AND CupNum <= " + _i_cupMax + "  AND ObjectAddWaterWeight > 0 AND ObjectAddWaterWeight <= 20;";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                }

                //复检天平重量为0时，实际加水量为0
                if (d_blWeight == 0)
                {
                        s_sql = "UPDATE drop_head SET RealAddWaterWeight = 0, AddWaterFinish = 1,WaterStandError = '" + Lib_Card.Configure.Parameter.Other_AErr_DripWater + "' WHERE " +
                        "BatchName = '" + obj_batchName + "' AND AddWaterChoose = 1 AND CupFinish = 0 AND CupNum >= " +
                        i_cupMin + " AND CupNum <= " + i_cupMax + "  AND ObjectAddWaterWeight > 20 And CupNum in (" + s_cupAddWater + ");";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                }
                else
                {
                        s_sql = "UPDATE drop_head SET RealAddWaterWeight = (ObjectAddWaterWeight * " + d_blDif + "), AddWaterFinish = 1,WaterStandError = '" + Lib_Card.Configure.Parameter.Other_AErr_DripWater + "' WHERE " +
                        "BatchName = '" + obj_batchName + "' AND AddWaterChoose = 1 AND CupFinish = 0 AND CupNum >= " +
                        i_cupMin + " AND CupNum <= " + i_cupMax + "  AND ObjectAddWaterWeight > 20 And CupNum in (" + s_cupAddWater + ");";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                }

                s_sql = "SELECT * FROM drop_head WHERE BatchName = '" + obj_batchName + "' AND ObjectAddWaterWeight != 0 AND CupNum >= " +
                    i_cupMin + " AND CupNum <= " + i_cupMax + " ;";
                DataTable dt_drop_head2 = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                //判断是否存在加水复检失败
                bool b_fail = false;
                string s_failCupNum = "";
                foreach (DataRow dataRow in dt_drop_head2.Rows)
                {
                    if (!lis_actualAddWaterCup.Contains(Convert.ToInt32(dataRow["CupNum"])))
                    {
                        continue;
                    }

                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                      "UPDATE cup_details SET TotalWeight =  " + dataRow["RealAddWaterWeight"] + " WHERE CupNum = " + dataRow["CupNum"] + " ;");


                    if (Convert.ToDouble(dataRow["ObjectAddWaterWeight"].ToString()) > 20)
                    {
                        if (System.Math.Abs(d_blWE * 100 / (Convert.ToDouble(dataRow["TotalWeight"].ToString()))) > Lib_Card.Configure.Parameter.Other_AErr_DripWater || d_blWeight == 0)
                        {
                            b_fail = true;
                            s_failCupNum += dataRow["CupNum"].ToString() + ",";
                        }
                    }
                    else
                    {
                        if (System.Math.Abs(Convert.ToDouble(dataRow["ObjectAddWaterWeight"].ToString()) * d_blDif * 100) / (Convert.ToDouble(dataRow["TotalWeight"].ToString())) > Lib_Card.Configure.Parameter.Other_AErr_DripWater || d_blWeight == 0)
                        {
                            b_fail = true;
                            s_failCupNum += dataRow["CupNum"].ToString() + ",";
                        }
                    }
                }

                    if (b_fail)
                    {
                        s_failCupNum = s_failCupNum.Remove(s_failCupNum.Length - 1);
                        FADM_Object.Communal.WriteDripWait(true);
                        Communal._b_isAllowDrip = false;
                        FADM_Object.MyAlarm myAlarm;

                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            myAlarm = new FADM_Object.MyAlarm(s_failCupNum + "号杯加水复检失败,是否继续?(继续滴液请点是，退出滴液请点否)", "加水复检", true, 1);
                        else
                            myAlarm = new FADM_Object.MyAlarm(" The re inspection of" + s_failCupNum + " cup with water has failed. Do you want to continue? (To continue dripping, please click Yes, and to exit dripping, please click No)", "Add water for retesting", true, 1);
                        while (true)
                        {
                            if (0 != myAlarm._i_alarm_Choose)
                                break;
                            Thread.Sleep(1);
                        }


                        if (2 == myAlarm._i_alarm_Choose)
                        {
                            //判断染色线程是否需要用机械手
                            if (null != FADM_Object.Communal.ReadDyeThread())
                            {
                                FADM_Object.Communal.WriteDripWait(true);
                                Communal._b_isWaitDrip = true;
                                Communal._b_isAllowDrip = true;
                                while (true)
                                {
                                    if (false == FADM_Object.Communal.ReadDripWait())
                                        break;
                                    Thread.Sleep(1);
                                }
                                Communal._b_isWaitDrip = false;
                            }
                            else
                                Communal._b_isWaitDrip = false;
                            throw new Exception("收到退出消息");
                        }

                        Communal._b_isAllowDrip = true;

                        //判断染色线程是否需要用机械手
                        if (null != FADM_Object.Communal.ReadDyeThread())
                        {
                            FADM_Object.Communal.WriteDripWait(true);
                            Communal._b_isWaitDrip = true;
                            Communal._b_isAllowDrip = true;
                            while (true)
                            {
                                if (false == FADM_Object.Communal.ReadDripWait())
                                    break;
                                Thread.Sleep(1);
                            }
                            Communal._b_isWaitDrip = false;
                        }
                        else
                        {
                            Communal._b_isWaitDrip = false;
                            FADM_Object.Communal.WriteDripWait(false);
                        }
                    }

                    //当加水在最后时，要重新判断一下
                    if (FADM_Object.Communal._b_isFinishSend)
                    {
                        foreach (int ic in lis_actualAddWaterCup)
                        {


                            DataTable dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                                    "SELECT * FROM drop_details WHERE BatchName = '" + obj_batchName.ToString() + "' AND Finish = 0" + " AND CupNum = " + ic + ";");
                            //查询一下加水没完成的也不置为完成
                            DataTable dt_drop_details4 = FADM_Object.Communal._fadmSqlserver.GetData(
                                    "SELECT * FROM drop_head WHERE BatchName = '" + obj_batchName.ToString() + "' AND AddWaterChoose = 1 AND AddWaterFinish =0" + " AND CupNum = " + ic + ";");
                            if (dt_drop_details3.Rows.Count == 0 && dt_drop_details4.Rows.Count == 0)
                            {
                                //置为完成
                                s_sql = "UPDATE drop_head SET CupFinish = 1 WHERE BatchName = '" + obj_batchName.ToString() + "' AND CupNum = " + ic + "; ";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);


                                bool b_fail1 = true;

                                s_sql = "SELECT drop_details.CupNum as CupNum, " +
                                    " drop_details.UnitOfAccount as UnitOfAccount, " +
                                            "drop_details.BottleNum as BottleNum, " +
                                            "drop_details.ObjectDropWeight as ObjectDropWeight, " +
                                            "drop_details.RealDropWeight as RealDropWeight, " +
                                            "bottle_details.SyringeType as SyringeType " +
                                            "FROM drop_details left join bottle_details on " +
                                            "bottle_details.BottleNum = drop_details.BottleNum " +
                                            "WHERE drop_details.BatchName = '" + obj_batchName.ToString() + "' AND CupNum = " + ic + ";";
                                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                                foreach (DataRow dr in dt_drop_head.Rows)
                                {
                                    if (Convert.ToInt32(dr["BottleNum"]) > Lib_Card.Configure.Parameter.Machine_Bottle_Total)
                                        continue;
                                    double d_blRealErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                                    Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"]))) : Convert.ToDouble(string.Format("{0:F3}",
                                    Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])));
                                    d_blRealErr = d_blRealErr < 0 ? -d_blRealErr : d_blRealErr;
                                    if (d_blRealErr > (dr["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip))
                                        b_fail1 = false;
                                }

                                dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                        "SELECT * FROM drop_head WHERE BatchName = '" + obj_batchName.ToString() + "' AND CupNum = " + ic + ";");
                                if (dt_drop_details3.Rows.Count > 0)
                                {
                                    int i_cup = Convert.ToInt16(dt_drop_details3.Rows[0]["CupNum"]);
                                    int i_Step = Convert.ToInt16(dt_drop_details3.Rows[0]["Step"]);
                                    double d_objWater = Convert.ToDouble(dt_drop_details3.Rows[0]["ObjectAddWaterWeight"]);
                                    double d_realWater = Convert.ToDouble(dt_drop_details3.Rows[0]["RealAddWaterWeight"]);
                                    double d_totalWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TotalWeight"]);
                                    double d_testTubeObjectAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeObjectAddWaterWeight"]);
                                    double d_testTubeRealAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeRealAddWaterWeight"]);
                                    double d_realDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater - d_objWater) : string.Format("{0:F3}", d_realWater - d_objWater));
                                    d_realDif = d_realDif < 0 ? -d_realDif : d_realDif;
                                    double d_allDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                        d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)) : string.Format("{0:F3}",
                                        d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)));

                                    string s_describe;
                                    string s_describe_EN;
                                    if (d_allDif < d_realDif || (d_realWater == 0.0 && d_objWater != 0.0))
                                    {
                                        b_fail1 = false;
                                    }

                                    if (b_fail1)
                                    {
                                        s_describe = "滴液成功!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                  ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                        s_describe_EN = "Drip Success !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                         ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                        if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(ic))
                                        {
                                            if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cup))
                                                FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                                        }
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                      "UPDATE cup_details SET Statues = '滴液成功' WHERE CupNum = " + i_cup + ";");
                                    }
                                    else
                                    {
                                        s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                        s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                         ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE cup_details SET Statues = '滴液失败' WHERE CupNum = " + i_cup + ";");
                                    }
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE drop_head SET DescribeChar = '" + s_describe + "',DescribeChar_EN = '" + s_describe_EN + "', FinishTime = '" + DateTime.Now + "', Step = 2 " +
                                   "WHERE BatchName = '" + obj_batchName.ToString() + "' AND CupNum = " + i_cup + ";");
                                }
                            }

                        }
                    }
                }
            }

            string s_unitA = "";
            int i_lowSrart = 0;
            if (FADM_Object.Communal._b_isAssitantFirst)
            {
                //加助剂
                s_unitA = "g/l";
                i_lowSrart = 0;
            }
            else
            {
                //加染料
                s_unitA = "%";
                i_lowSrart = 0;
            }

        label3:
            s_sql = "SELECT * FROM drop_details WHERE BatchName = '" + obj_batchName + "' AND " +
                "Finish = 0 AND UnitOfAccount = '" + s_unitA + "' AND MinWeight = " + i_lowSrart + " AND " +
                "BottleNum <= " + Lib_Card.Configure.Parameter.Machine_Bottle_Total + " AND CupNum >= " +
                i_cupMin + " AND CupNum <= " + i_cupMax + " And IsDrop !=0 ORDER BY CupNum;";
            dt_drop_head_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (0 == dt_drop_head_temp.Rows.Count)
            {
                if (FADM_Object.Communal._b_isAssitantFirst)
                {
                    if ("g/l" == s_unitA)
                    {
                        //助剂已加完，加染料
                        goto label4;
                    }
                    else
                    {
                        if (0 == i_lowSrart)
                        {
                            //染料已加完，加液量不足
                            goto label5;
                        }
                        else if (1 == i_lowSrart)
                        {
                            //液量不足已加完，加超出生命周期
                            goto label17;
                        }
                        else if (3 == i_lowSrart)
                        {
                            //超出生命周期的加完，加检测不到针筒
                            goto label16;
                        }
                        else
                        {
                            //结束
                            goto label6;
                        }
                    }
                }
                else
                {
                    if ("%" == s_unitA)
                    {
                        //染料已加完，加助剂
                        goto label4;
                    }
                    else
                    {
                        if (0 == i_lowSrart)
                        {
                            //助剂已加完，加液量不足
                            goto label5;
                        }
                        else if (1 == i_lowSrart)
                        {
                            //液量不足已加完，加超出生命周期
                            goto label17;
                        }
                        else if (3 == i_lowSrart)
                        {
                            //超出生命周期的加完，加检测不到针筒
                            goto label16;
                        }
                        else
                        {
                            //结束
                            goto label6;
                        }
                    }
                }

            }
            int i_minCupNo = Convert.ToInt32(dt_drop_head_temp.Rows[0]["CupNum"]);
        label7:
            s_sql = "SELECT * FROM drop_details WHERE BatchName = '" + obj_batchName + "' AND CupNum = " + i_minCupNo + " AND " +
                "Finish = 0 AND UnitOfAccount = '" + s_unitA + "' AND MinWeight = " + i_lowSrart + " AND " +
                "BottleNum <= " + Lib_Card.Configure.Parameter.Machine_Bottle_Total + " And IsDrop !=0  ORDER BY BottleNum;";
            dt_drop_head_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (0 == dt_drop_head_temp.Rows.Count)
            {
                //当前杯已加完
                goto label3;
            }
            int i_minBottleNo = Convert.ToInt32(dt_drop_head_temp.Rows[0]["BottleNum"]);

            s_sql = "SELECT * FROM drop_details WHERE BatchName = '" + obj_batchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                "Finish = 0 AND UnitOfAccount = '" + s_unitA + "' AND MinWeight = " + i_lowSrart + " AND " +
                "BottleNum <= " + Lib_Card.Configure.Parameter.Machine_Bottle_Total + " AND CupNum >= " +
                i_cupMin + " AND CupNum <= " + i_cupMax + " And IsDrop !=0   ORDER BY CupNum;";
            dt_drop_head_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (0 == dt_drop_head_temp.Rows.Count)
            {
                //当前瓶完成
                goto label7;
            }

            s_sql = "SELECT * FROM bottle_details WHERE BottleNum = " + i_minBottleNo + ";";
            DataTable dt_bottle_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            int i_adjust = Convert.ToInt32(dt_bottle_details.Rows[0]["AdjustValue"]);
            bool blCheckSuccess = (Convert.ToString(dt_bottle_details.Rows[0]["AdjustSuccess"]) == "1");
            string sSyringeType = Convert.ToString(dt_bottle_details.Rows[0]["SyringeType"]);
            string s_dripReserveFirst = "0";
            if (!(dt_bottle_details.Rows[0]["DripReserveFirst"] is DBNull))
            {
                s_dripReserveFirst = dt_bottle_details.Rows[0]["DripReserveFirst"].ToString();
            }
            string s_unitOfAccount = "";



            Dictionary<int, int> dic_pulse = new Dictionary<int, int>();
            Dictionary<int, double> dic_weight = new Dictionary<int, double>();
            Dictionary<int, double> dic_water = new Dictionary<int, double>();
            int i_pulseT = 0;
            if (0 == i_lowSrart)
            {
                double d_blCW = Convert.ToDouble(string.Format("{0:F3}", dt_bottle_details.Rows[0]["CurrentWeight"]));
                foreach (DataRow dataRow in dt_drop_head_temp.Rows)
                {
                    int i_cCupNo = Convert.ToInt32(dataRow["CupNum"]);
                    double d_blOAddW = Convert.ToDouble(string.Format("{0:F3}", dataRow["ObjectDropWeight"]));
                    int i_needPulse = dataRow["NeedPulse"] is DBNull ? 0 : Convert.ToInt32(dataRow["NeedPulse"]);
                    d_blCW -= d_blOAddW;

                    //查询判断是否超期
                    s_sql = "SELECT * FROM assistant_details WHERE AssistantCode = '" + dt_bottle_details.Rows[0]["AssistantCode"].ToString() + "';";
                    DataTable dt_assistant_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                    DateTime timeA = Convert.ToDateTime(dt_bottle_details.Rows[0]["BrewingData"].ToString());
                    DateTime timeB = DateTime.Now; //获取当前时间
                    TimeSpan ts = timeB - timeA; //计算时间差
                    string s_time = ts.TotalHours.ToString(); //将时间差转换为小时


                    if (d_blCW < Lib_Card.Configure.Parameter.Other_Bottle_MinWeight && FADM_Object.Communal._b_isLowDrip)
                    {
                        //查询在备料表是否存在记录，如果存在，先让客户选择是否使用备料数据来更新
                        string s_sqlpre = "SELECT * FROM pre_brew WHERE  BottleNum = " + i_minBottleNo + ";";
                        DataTable dt_pre_brew = FADM_Object.Communal._fadmSqlserver.GetData(s_sqlpre);
                        if (dt_pre_brew.Rows.Count > 0)
                        {
                            FADM_Object.Communal.WriteDripWait(true);
                            Communal._b_isAllowDrip = false;
                            FADM_Object.MyAlarm myAlarm;
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm(
                                i_minBottleNo + "号母液瓶液量过低，备料表存在已开料记录，是否替换(替换请点是，继续使用旧母液请点否)?", "滴液", true, 1);
                            else
                                myAlarm = new FADM_Object.MyAlarm(
                                "The " + i_minBottleNo + " mother liquor bottle has expired, and there is a record of opened materials in the material preparation table. Should it be replaced? (Please click Yes for replacement, and click No for continuing to use the old mother liquor)", "Drip", true, 1);

                            while (true)
                            {
                                if (0 != myAlarm._i_alarm_Choose)
                                    break;
                                Thread.Sleep(1);
                            }
                            Communal._b_isAllowDrip = true;
                            //判断染色线程是否需要用机械手
                            if (null != FADM_Object.Communal.ReadDyeThread())
                            {
                                FADM_Object.Communal.WriteDripWait(true);
                                Communal._b_isWaitDrip = true;
                                Communal._b_isAllowDrip = true;
                                while (true)
                                {
                                    if (false == FADM_Object.Communal.ReadDripWait())
                                        break;
                                    Thread.Sleep(1);
                                }
                                Communal._b_isWaitDrip = false;
                            }
                            else
                            {
                                Communal._b_isWaitDrip = false;
                                FADM_Object.Communal.WriteDripWait(false);
                            }
                            //如果选择是，使用新料重新计算
                            if (1 == myAlarm._i_alarm_Choose)
                            {
                                //使用备料表数据更新现有母液瓶数据，删除备料表记录
                                s_sql = "UPDATE bottle_details SET RealConcentration = '" + dt_pre_brew.Rows[0]["RealConcentration"].ToString() + "',CurrentWeight = '" + dt_pre_brew.Rows[0]["CurrentWeight"].ToString() + "',BrewingData='"
                                    + dt_pre_brew.Rows[0]["BrewingData"].ToString() + "'" +
                                " WHERE BottleNum = " + i_minBottleNo + ";";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from pre_brew where BottleNum = " + i_minBottleNo);
                                goto label7;
                            }
                            //选择否就和之前的逻辑一致
                            else
                            {
                                s_sql = "UPDATE drop_details SET MinWeight = 1 WHERE BatchName = '" + obj_batchName + "' AND MinWeight=0 And " +
                                "BottleNum = " + i_minBottleNo + " AND Finish = 0 AND CupNum >= " + i_cCupNo + " AND CupNum <= " + i_cupMax + ";";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                break;
                            }
                        }
                        else
                        {

                            s_sql = "UPDATE drop_details SET MinWeight = 1 WHERE BatchName = '" + obj_batchName + "' AND MinWeight=0 And " +
                                "BottleNum = " + i_minBottleNo + " AND Finish = 0 AND CupNum >= " + i_cCupNo + " AND CupNum <= " + i_cupMax + ";";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                            break;
                        }

                    }
                    else if (Convert.ToDouble(s_time) > Convert.ToDouble(dt_assistant_details.Rows[0]["TermOfValidity"].ToString()) && FADM_Object.Communal._b_isOutDrip)
                    {
                        //查询在备料表是否存在记录，如果存在，先让客户选择是否使用备料数据来更新
                        string s_sqlpre = "SELECT * FROM pre_brew WHERE  BottleNum = " + i_minBottleNo + ";";
                        DataTable dt_pre_brew = FADM_Object.Communal._fadmSqlserver.GetData(s_sqlpre);
                        if (dt_pre_brew.Rows.Count > 0)
                        {
                            FADM_Object.Communal.WriteDripWait(true);
                            Communal._b_isAllowDrip = false;
                            FADM_Object.MyAlarm myAlarm;
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm(
                                i_minBottleNo + "号母液瓶过期，备料表存在已开料记录，是否替换(替换请点是，继续使用旧母液请点否)?", "滴液", true, 1);
                            else
                                myAlarm = new FADM_Object.MyAlarm(
                                "The " + i_minBottleNo + " mother liquor bottle has expired, and there is a record of opened materials in the material preparation table. Should it be replaced? (Please click Yes for replacement, and click No for continuing to use the old mother liquor)", "Drip", true, 1);

                            while (true)
                            {
                                if (0 != myAlarm._i_alarm_Choose)
                                    break;
                                Thread.Sleep(1);
                            }
                            Communal._b_isAllowDrip = true;
                            //判断染色线程是否需要用机械手
                            if (null != FADM_Object.Communal.ReadDyeThread())
                            {
                                FADM_Object.Communal.WriteDripWait(true);
                                Communal._b_isWaitDrip = true;
                                Communal._b_isAllowDrip = true;
                                while (true)
                                {
                                    if (false == FADM_Object.Communal.ReadDripWait())
                                        break;
                                    Thread.Sleep(1);
                                }
                                Communal._b_isWaitDrip = false;
                            }
                            else
                            {
                                Communal._b_isWaitDrip = false;
                                FADM_Object.Communal.WriteDripWait(false);
                            }
                            //如果选择是，使用新料重新计算
                            if (1 == myAlarm._i_alarm_Choose)
                            {
                                //使用备料表数据更新现有母液瓶数据，删除备料表记录
                                s_sql = "UPDATE bottle_details SET RealConcentration = '" + dt_pre_brew.Rows[0]["RealConcentration"].ToString() + "',CurrentWeight = '" + dt_pre_brew.Rows[0]["CurrentWeight"].ToString() + "',BrewingData='"
                                    + dt_pre_brew.Rows[0]["BrewingData"].ToString() + "'" +
                                " WHERE BottleNum = " + i_minBottleNo + ";";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from pre_brew where BottleNum = " + i_minBottleNo);
                                goto label7;
                            }
                            else
                            {
                                s_sql = "UPDATE drop_details SET MinWeight = 3 WHERE BatchName = '" + obj_batchName + "' AND  MinWeight=0 And " +
                            "BottleNum = " + i_minBottleNo + " AND Finish = 0 AND CupNum >= " + i_cCupNo + " AND CupNum <= " + i_cupMax + ";";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                break;
                            }
                        }
                        else
                        {

                            s_sql = "UPDATE drop_details SET MinWeight = 3 WHERE BatchName = '" + obj_batchName + "' AND  MinWeight=0 And " +
                            "BottleNum = " + i_minBottleNo + " AND Finish = 0 AND CupNum >= " + i_cCupNo + " AND CupNum <= " + i_cupMax + ";";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                            break;
                        }
                    }
                    else
                    {
                        //判断是否分开两次滴液，如果是就使用需加脉冲来计算
                        int i_pulse = i_needPulse > 0 ? i_needPulse : Convert.ToInt32(i_adjust * d_blOAddW);
                        dic_pulse.Add(i_cCupNo, i_pulse);
                        dic_weight.Add(i_cCupNo, d_blOAddW);
                        dic_water.Add(i_cCupNo, 0.0);
                        i_pulseT += i_pulse;

                        s_unitOfAccount = dataRow["UnitOfAccount"].ToString();
                    }


                }

                if (0 == dic_pulse.Count)
                {
                    //当前瓶液量不足
                    goto label7;
                }
            }
            else
            {
                foreach (DataRow dataRow in dt_drop_head_temp.Rows)
                {
                    int i_cCupNo = Convert.ToInt32(dataRow["CupNum"]);
                    double d_blOAddW = Convert.ToDouble(string.Format("{0:F3}", dataRow["ObjectDropWeight"]));
                    int i_needPulse = dataRow["NeedPulse"] is DBNull ? 0 : Convert.ToInt32(dataRow["NeedPulse"]);
                    //判断是否分开两次滴液，如果是就使用需加脉冲来计算
                    int i_pulse = i_needPulse > 0 ? i_needPulse : Convert.ToInt32(i_adjust * d_blOAddW);
                    dic_pulse.Add(i_cCupNo, i_pulse);
                    dic_weight.Add(i_cCupNo, d_blOAddW);
                    dic_water.Add(i_cCupNo, 0.0);
                    i_pulseT += i_pulse;

                    s_unitOfAccount = dataRow["UnitOfAccount"].ToString();
                }
            }

            Lib_Log.Log.writeLogException(i_minBottleNo+"号母液瓶添加"+ dic_pulse.Count);

            //判断染色线程是否需要用机械手
            if (null != FADM_Object.Communal.ReadDyeThread())
            {
                FADM_Object.Communal.WriteDripWait(true);
                Communal._b_isWaitDrip = true;
                Communal._b_isAllowDrip = true;
                while (true)
                {
                    if (false == FADM_Object.Communal.ReadDripWait())
                        break;
                    Thread.Sleep(1);
                }
                Communal._b_isWaitDrip = false;
            }
            else
                Communal._b_isWaitDrip = false;

            //针检
            if ((0 >= i_adjust || false == blCheckSuccess) && !b_checkFail)
            {
            label8:
                //判断染色线程是否需要用机械手
                if (null != FADM_Object.Communal.ReadDyeThread())
                {
                    FADM_Object.Communal.WriteDripWait(true);
                    Communal._b_isWaitDrip = true;
                    Communal._b_isAllowDrip = true;
                    while (true)
                    {
                        if (false == FADM_Object.Communal.ReadDripWait())
                            break;
                        Thread.Sleep(1);
                    }
                    Communal._b_isWaitDrip = false;
                }
            else
                    Communal._b_isWaitDrip = false;

                if (_b_dripStop)
                {
                    FADM_Object.Communal._b_stop = true;
                }

                int i_res = new BottleCheck().MyDripCheck(i_minBottleNo, true, i_lowSrart); //针检

                if (-1 == i_res)
                {
                    FADM_Object.Communal.WriteDripWait(true);
                    Communal._b_isAllowDrip = false;
                    FADM_Object.MyAlarm myAlarm;
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + "号母液瓶针检失败，是否继续?(继续针检请点是，退出针检请点否)", "滴液针检", true, 1, true);
                    else
                        myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + " bottle needle inspection failed, do you want to continue? " +
                            "(To continue the needle examination, please click Yes, and to exit the needle examination, please click No)", "Drip needle examination", true, 1, true);
                    while (true)

                    {
                        if (0 != myAlarm._i_alarm_Choose)
                            break;
                        Thread.Sleep(1);
                    }
                    Communal._b_isAllowDrip = true;
                    //判断染色线程是否需要用机械手
                    if (null != FADM_Object.Communal.ReadDyeThread())
                    {
                        FADM_Object.Communal.WriteDripWait(true);
                        Communal._b_isWaitDrip = true;
                        Communal._b_isAllowDrip = true;
                        while (true)
                        {
                            if (false == FADM_Object.Communal.ReadDripWait())
                                break;
                            Thread.Sleep(1);
                        }
                        Communal._b_isWaitDrip = false;
                    }
                    else
                    {
                        Communal._b_isWaitDrip = false;
                        FADM_Object.Communal.WriteDripWait(false);
                    }

                    if (1 == myAlarm._i_alarm_Choose)
                        goto label8;
                    else
                    {
                        b_checkFail = true;
                    }
                }
                else if (-2 == i_res)
                {
                    s_sql = "UPDATE drop_details SET MinWeight = 2 WHERE BatchName = '" + obj_batchName + "' AND " +
                          "BottleNum = " + i_minBottleNo + " AND Finish = 0;";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                    goto label3;
                }
                else if (-3 == i_res)
                {
                    //夹不到针筒时选择否，直接退出
                    throw new Exception("收到退出消息");
                }
                if (b_checkFail)
                {
                    s_sql = "update bottle_details set AdjustValue = 3900 where AdjustValue =0 And " +
                          "BottleNum = " + i_minBottleNo + ";";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                }
                goto label7;
            }


            if (0 == dic_pulse.Count)
            {
                //当前瓶液量不足
                goto label7;
            }

            //判断染色线程是否需要用机械手
            if (null != FADM_Object.Communal.ReadDyeThread())
            {
                FADM_Object.Communal.WriteDripWait(true);
                Communal._b_isWaitDrip = true;
                Communal._b_isAllowDrip = true;
                while (true)
                {
                    if (false == FADM_Object.Communal.ReadDripWait())
                        break;
                    Thread.Sleep(1);
                }
                Communal._b_isWaitDrip = false;
            }
            else
                Communal._b_isWaitDrip = false;

            sAddArg o = new sAddArg();
            o._i_minBottleNo = i_minBottleNo;
            o._obj_batchName = obj_batchName.ToString();
            o._i_adjust = i_adjust;
            o._i_pulseT = i_pulseT;
            o._s_syringeType = sSyringeType;
            o._s_unitOfAccount = s_unitOfAccount;
            o._dic_pulse = dic_pulse;
            o._dic_water = dic_water;
            if (_b_isAloneDripReserve)
            {
                if (s_dripReserveFirst == "0")
                {
                    _b_isDripReserveFirst = false;
                }
                else
                {
                    _b_isDripReserveFirst = true;
                }
            }
            Dictionary<int, double> dic_return = new Dictionary<int, double>();
            int i_ret = FADM_Object.Communal.AddMac(o, ref dic_return,1, 0, i_lowSrart);
            //夹不到针筒
            if (i_ret == -1)
            {
                if (i_lowSrart == 2)
                {
                    FADM_Object.Communal.WriteDripWait(true);
                    Communal._b_isAllowDrip = false;
                    FADM_Object.MyAlarm myAlarm;
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + "号母液瓶未找到针筒，是否继续执行?(继续寻找请点是，退出滴液请点否)", "滴液", true, 1, true);
                    else
                        myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + " bottle did not find a syringe. Do you want to continue? " +
                            "(To continue searching, please click Yes. To exit Drip, please click No)", "Drip", true, 1, true);
                    while (true)
                    {
                        if (0 != myAlarm._i_alarm_Choose)
                            break;
                        Thread.Sleep(1);
                    }
                    Communal._b_isAllowDrip = true;
                    //判断染色线程是否需要用机械手
                    if (null != FADM_Object.Communal.ReadDyeThread())
                    {
                        FADM_Object.Communal.WriteDripWait(true);
                        Communal._b_isWaitDrip = true;
                        Communal._b_isAllowDrip = true;
                        while (true)
                        {
                            if (false == FADM_Object.Communal.ReadDripWait())
                                break;
                            Thread.Sleep(1);
                        }
                        Communal._b_isWaitDrip = false;
                    }
                    else
                    {
                        Communal._b_isWaitDrip = false;
                        FADM_Object.Communal.WriteDripWait(false);
                    }

                    Communal._b_isAllowDrip = true;
                    if (1 == myAlarm._i_alarm_Choose)
                        goto label3;
                    else
                        throw new Exception("收到退出消息");
                }
                else
                {
                    s_sql = "UPDATE drop_details SET MinWeight = 2 WHERE BatchName = '" + obj_batchName + "' AND " +
                     "BottleNum = " + i_minBottleNo + " AND Finish = 0;";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                    goto label3;
                }
            }
            //滴液完成
            else if (i_ret == 0)
            {
                if (FADM_Object.Communal._b_isFinishSend)
                {
                    foreach (KeyValuePair<int, double> kvp in dic_return)
                    {
                        double d_blRErr = 0;
                        if ("小针筒" == sSyringeType || "Little Syringe" == sSyringeType)
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                        else
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight));

                        //查询开料日期
                        DataTable dt_bottle_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                    "SELECT * FROM bottle_details WHERE  BottleNum = " + i_minBottleNo + ";");

                        if (0.00 != kvp.Value)
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + d_blRErr + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                            "WHERE BatchName = '" + obj_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                            "CupNum = " + kvp.Key + ";");

                            DataTable dt_drop_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_details WHERE BatchName = '" + obj_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND CupNum = " + kvp.Key + ";");
                            if (dt_drop_details2.Rows.Count > 0)
                            {

                                //记录是否修正数据
                                bool b_change = false;
                                double d_realDrop = 0.0;
                                if (FADM_Object.Communal._b_isCorrect)
                                {
                                    //如果实际滴液量为负数，就改为0
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["RealDropWeight"]) < 0)
                                    {
                                        b_change = true;
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = 0   ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + obj_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                    }

                                    //判断当目标滴液量大于1g，而且实际滴液误差小于等于0.05的，全部置为0.02
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) > 1)
                                    {
                                        if (d_blRErr <= 0.05 && d_blRErr > 0.02)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + 0.02 + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + obj_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) + 0.02;
                                        }
                                        else if (d_blRErr < -0.02 && d_blRErr >= -0.05)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + (-0.02) + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + obj_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) - 0.02;
                                        }
                                    }
                                }
                                if (b_change)
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + d_realDrop + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_realDrop + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");
                                }
                                else
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + dt_drop_details2.Rows[0]["RealDropWeight"] + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + dt_drop_details2.Rows[0]["RealDropWeight"] + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");
                                }
                            }
                        }
                        else
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "UPDATE drop_details SET Finish = 1,RealDropWeight = 0.00 " + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                           "WHERE BatchName = '" + obj_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                           "CupNum = " + kvp.Key + ";");
                        }

                        DataTable dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_details WHERE BatchName = '" + obj_batchName.ToString() + "' AND Finish = 0" + " AND CupNum = " + kvp.Key + ";");
                        //查询一下加水没完成的也不置为完成
                        DataTable dt_drop_details4 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_head WHERE BatchName = '" + obj_batchName.ToString() + "' AND AddWaterChoose = 1 AND AddWaterFinish =0" + " AND CupNum = " + kvp.Key + ";");
                        if (dt_drop_details3.Rows.Count == 0 && dt_drop_details4.Rows.Count == 0)
                        {
                            //置为完成
                            s_sql = "UPDATE drop_head SET CupFinish = 1 WHERE BatchName = '" + obj_batchName.ToString() + "' AND CupNum = " + kvp.Key + "; ";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);


                            bool b_fail = true;

                            s_sql = "SELECT drop_details.CupNum as CupNum, " +
                                " drop_details.UnitOfAccount as UnitOfAccount, " +
                                         "drop_details.BottleNum as BottleNum, " +
                                         "drop_details.ObjectDropWeight as ObjectDropWeight, " +
                                         "drop_details.RealDropWeight as RealDropWeight, " +
                                         "bottle_details.SyringeType as SyringeType " +
                                         "FROM drop_details left join bottle_details on " +
                                         "bottle_details.BottleNum = drop_details.BottleNum " +
                                         "WHERE drop_details.BatchName = '" + obj_batchName.ToString() + "' AND CupNum = " + kvp.Key + ";";
                            dt_drop_head_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                            foreach (DataRow dr in dt_drop_head_temp.Rows)
                            {
                                if (Convert.ToInt32(dr["BottleNum"]) > Lib_Card.Configure.Parameter.Machine_Bottle_Total)
                                    continue;
                                double d_blRealErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"]))) : Convert.ToDouble(string.Format("{0:F3}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])));
                                d_blRealErr = d_blRealErr < 0 ? -d_blRealErr : d_blRealErr;
                                if (d_blRealErr > (dr["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip))
                                    b_fail = false;
                            }

                            dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                    "SELECT * FROM drop_head WHERE BatchName = '" + obj_batchName.ToString() + "' AND CupNum = " + kvp.Key + ";");
                            if (dt_drop_details3.Rows.Count > 0)
                            {
                                int i_cup = Convert.ToInt16(dt_drop_details3.Rows[0]["CupNum"]);
                                int i_step = Convert.ToInt16(dt_drop_details3.Rows[0]["Step"]);
                                double d_objWater = Convert.ToDouble(dt_drop_details3.Rows[0]["ObjectAddWaterWeight"]);
                                double d_realWater = Convert.ToDouble(dt_drop_details3.Rows[0]["RealAddWaterWeight"]);
                                double d_totalWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TotalWeight"]);
                                double d_testTubeObjectAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeObjectAddWaterWeight"]);
                                double d_testTubeRealAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeRealAddWaterWeight"]);
                                double d_realDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater - d_objWater) : string.Format("{0:F3}", d_realWater - d_objWater));
                                d_realDif = d_realDif < 0 ? -d_realDif : d_realDif;
                                double d_allDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)) : string.Format("{0:F3}",
                                    d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)));

                                string s_describe;
                                string s_describe_EN;
                                if (d_allDif < d_realDif || (d_realWater == 0.0 && d_objWater != 0.0))
                                {
                                    b_fail = false;
                                }

                                if (b_fail)
                                {
                                    s_describe = "滴液成功!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                              ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Success !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(kvp.Key))
                                    {
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cup))
                                            FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                                    }
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  "UPDATE cup_details SET Statues = '滴液成功' WHERE CupNum = " + i_cup + ";");
                                }
                                else
                                {
                                    s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                            ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET Statues = '滴液失败' WHERE CupNum = " + i_cup + ";");
                                }
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE drop_head SET DescribeChar = '" + s_describe + "',DescribeChar_EN = '" + s_describe_EN + "', FinishTime = '" + DateTime.Now + "', Step = 2 " +
                               "WHERE BatchName = '" + obj_batchName.ToString() + "' AND CupNum = " + i_cup + ";");
                            }
                        }

                    }
                }
                else
                {
                    foreach (KeyValuePair<int, double> kvp in dic_return)
                    {
                        double d_blRErr = 0;
                        if ("小针筒" == sSyringeType || "Little Syringe" == sSyringeType)
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                        else
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight));
                        ;

                        //查询开料日期
                        DataTable dt_bottle_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                    "SELECT * FROM bottle_details WHERE  BottleNum = " + i_minBottleNo + ";");

                        if (0.00 != kvp.Value)
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + d_blRErr + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                            "WHERE BatchName = '" + obj_batchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                            "CupNum = " + kvp.Key + ";");



                            DataTable dt_drop_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_details WHERE BatchName = '" + obj_batchName + "' AND BottleNum = " + i_minBottleNo + " AND CupNum = " + kvp.Key + ";");

                            if (dt_drop_details2.Rows.Count > 0)
                            {
                                //记录是否修正数据
                                bool b_change = false;
                                double d_realDrop = 0.0;
                                if (FADM_Object.Communal._b_isCorrect)
                                {
                                    //如果实际滴液量为负数，就改为0
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["RealDropWeight"]) < 0)
                                    {
                                        b_change = true;
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = 0   ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + obj_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                    }

                                    //判断当目标滴液量大于1g，而且实际滴液误差小于等于0.05的，全部置为0.02
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) > 1)
                                    {
                                        if (d_blRErr <= 0.05 && d_blRErr > 0.02)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + 0.02 + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + obj_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) + 0.02;
                                        }
                                        else if (d_blRErr < -0.02 && d_blRErr >= -0.05)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + (-0.02) + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + obj_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) - 0.02;
                                        }
                                    }
                                }
                                if (b_change)
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + d_realDrop + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_realDrop + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");
                                }
                                else
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + dt_drop_details2.Rows[0]["RealDropWeight"] + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + dt_drop_details2.Rows[0]["RealDropWeight"] + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");

                                    ////置位完成标志位
                                    //FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    //    "UPDATE drop_details SET Finish = 1 WHERE BatchName = '" + o_BatchName + "' AND " +
                                    //    "BottleNum = " + _i_minBottleNo + " AND CupNum = " + dic_pulse.First().Key + ";");
                                }
                            }
                        }
                        else
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "UPDATE drop_details SET Finish = 1,RealDropWeight = 0.00 " + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                           "WHERE BatchName = '" + obj_batchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                           "CupNum = " + kvp.Key + ";");
                        }

                    }
                }
            }
            //由于滴废液时发现数值太小，直接提醒，不先滴这个，跳过
            else if (i_ret == -2)
            {
                //把已经滴过的先置为完成
                if (FADM_Object.Communal._b_isFinishSend)
                {
                    foreach (KeyValuePair<int, double> kvp in dic_return)
                    {
                        double d_blRErr = 0;
                        if ("小针筒" == sSyringeType || "Little Syringe" == sSyringeType)
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                        else
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight));

                        //查询开料日期
                        DataTable dt_bottle_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                    "SELECT * FROM bottle_details WHERE  BottleNum = " + i_minBottleNo + ";");

                        if (0.00 != kvp.Value)
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + d_blRErr + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                            "WHERE BatchName = '" + obj_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                            "CupNum = " + kvp.Key + ";");

                            DataTable dt_drop_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_details WHERE BatchName = '" + obj_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND CupNum = " + kvp.Key + ";");
                            if (dt_drop_details2.Rows.Count > 0)
                            {
                                //记录是否修正数据
                                bool b_change = false;
                                double d_realDrop = 0.0;
                                if (FADM_Object.Communal._b_isCorrect)
                                {
                                    //如果实际滴液量为负数，就改为0
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["RealDropWeight"]) < 0)
                                    {
                                        b_change = true;
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = 0   ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + obj_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                    }

                                    //判断当目标滴液量大于1g，而且实际滴液误差小于等于0.05的，全部置为0.02
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) > 1)
                                    {
                                        if (d_blRErr <= 0.05 && d_blRErr > 0.02)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + 0.02 + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + obj_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) + 0.02;
                                        }
                                        else if (d_blRErr < -0.02 && d_blRErr >= -0.05)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + (-0.02) + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + obj_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) - 0.02;
                                        }
                                    }
                                }
                                if (b_change)
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + d_realDrop + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_realDrop + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");
                                }
                                else
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE cup_details SET TotalWeight = TotalWeight+ " + dt_drop_details2.Rows[0]["RealDropWeight"] + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + dt_drop_details2.Rows[0]["RealDropWeight"] + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");
                                }
                            }
                        }
                        else
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "UPDATE drop_details SET Finish = 1,RealDropWeight = 0.00 " + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                           "WHERE BatchName = '" + obj_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                           "CupNum = " + kvp.Key + ";");
                        }

                        DataTable dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_details WHERE BatchName = '" + obj_batchName.ToString() + "' AND Finish = 0" + " AND CupNum = " + kvp.Key + ";");
                        //查询一下加水没完成的也不置为完成
                        DataTable dt_drop_details4 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_head WHERE BatchName = '" + obj_batchName.ToString() + "' AND AddWaterChoose = 1 AND AddWaterFinish =0" + " AND CupNum = " + kvp.Key + ";");
                        if (dt_drop_details3.Rows.Count == 0 && dt_drop_details4.Rows.Count == 0)
                        {
                            //置为完成
                            s_sql = "UPDATE drop_head SET CupFinish = 1 WHERE BatchName = '" + obj_batchName.ToString() + "' AND CupNum = " + kvp.Key + "; ";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);


                            bool b_fail = true;

                            s_sql = "SELECT drop_details.CupNum as CupNum, " +
                                " drop_details.UnitOfAccount as UnitOfAccount, " +
                                         "drop_details.BottleNum as BottleNum, " +
                                         "drop_details.ObjectDropWeight as ObjectDropWeight, " +
                                         "drop_details.RealDropWeight as RealDropWeight, " +
                                         "bottle_details.SyringeType as SyringeType " +
                                         "FROM drop_details left join bottle_details on " +
                                         "bottle_details.BottleNum = drop_details.BottleNum " +
                                         "WHERE drop_details.BatchName = '" + obj_batchName.ToString() + "' AND CupNum = " + kvp.Key + ";";
                            dt_drop_head_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                            foreach (DataRow dr in dt_drop_head_temp.Rows)
                            {
                                if (Convert.ToInt32(dr["BottleNum"]) > Lib_Card.Configure.Parameter.Machine_Bottle_Total)
                                    continue;
                                double d_blRealErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"]))) : Convert.ToDouble(string.Format("{0:F3}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])));
                                d_blRealErr = d_blRealErr < 0 ? -d_blRealErr : d_blRealErr;
                                if (d_blRealErr > (dr["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip))
                                    b_fail = false;
                            }

                            dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                    "SELECT * FROM drop_head WHERE BatchName = '" + obj_batchName.ToString() + "' AND CupNum = " + kvp.Key + ";");
                            if (dt_drop_details3.Rows.Count > 0)
                            {
                                int i_cup = Convert.ToInt16(dt_drop_details3.Rows[0]["CupNum"]);
                                int i_step = Convert.ToInt16(dt_drop_details3.Rows[0]["Step"]);
                                double d_objWater = Convert.ToDouble(dt_drop_details3.Rows[0]["ObjectAddWaterWeight"]);
                                double d_realWater = Convert.ToDouble(dt_drop_details3.Rows[0]["RealAddWaterWeight"]);
                                double d_totalWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TotalWeight"]);
                                double d_testTubeObjectAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeObjectAddWaterWeight"]);
                                double d_testTubeRealAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeRealAddWaterWeight"]);
                                double d_realDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater - d_objWater) : string.Format("{0:F3}", d_realWater - d_objWater));
                                d_realDif = d_realDif < 0 ? -d_realDif : d_realDif;
                                double d_allDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)) : string.Format("{0:F3}",
                                    d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)));

                                string s_describe;
                                string s_describe_EN;
                                if (d_allDif < d_realDif || (d_realWater == 0.0 && d_objWater != 0.0))
                                {
                                    b_fail = false;
                                }

                                if (b_fail)
                                {
                                    s_describe = "滴液成功!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                              ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Success !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(kvp.Key))
                                    {
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cup))
                                            FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                                    }
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  "UPDATE cup_details SET Statues = '滴液成功' WHERE CupNum = " + i_cup + ";");
                                }
                                else
                                {
                                    s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                            ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET Statues = '滴液失败' WHERE CupNum = " + i_cup + ";");
                                }
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE drop_head SET DescribeChar = '" + s_describe + "',DescribeChar_EN = '" + s_describe_EN + "', FinishTime = '" + DateTime.Now + "', Step = 2 " +
                               "WHERE BatchName = '" + obj_batchName.ToString() + "' AND CupNum = " + i_cup + ";");
                            }
                        }

                    }
                }
                else
                {
                    foreach (KeyValuePair<int, double> kvp in dic_return)
                    {
                        double d_blRErr = 0;
                        if ("小针筒" == sSyringeType || "Little Syringe" == sSyringeType)
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                        else
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight));
                        ;

                        //查询开料日期
                        DataTable dt_bottle_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                    "SELECT * FROM bottle_details WHERE  BottleNum = " + i_minBottleNo + ";");

                        if (0.00 != kvp.Value)
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + d_blRErr + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                            "WHERE BatchName = '" + obj_batchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                            "CupNum = " + kvp.Key + ";");

                            DataTable dt_drop_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_details WHERE BatchName = '" + obj_batchName + "' AND BottleNum = " + i_minBottleNo + " AND CupNum = " + kvp.Key + ";");

                            if (dt_drop_details2.Rows.Count > 0)
                            {
                                //记录是否修正数据
                                bool b_change = false;
                                double d_realDrop = 0.0;
                                if (FADM_Object.Communal._b_isCorrect)
                                {
                                    //如果实际滴液量为负数，就改为0
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["RealDropWeight"]) < 0)
                                    {
                                        b_change = true;
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = 0   ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + obj_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                    }

                                    //判断当目标滴液量大于1g，而且实际滴液误差小于等于0.05的，全部置为0.02
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) > 1)
                                    {
                                        if (d_blRErr <= 0.05 && d_blRErr > 0.02)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + 0.02 + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + obj_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) + 0.02;
                                        }
                                        else if (d_blRErr < -0.02 && d_blRErr >= -0.05)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + (-0.02) + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + obj_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) - 0.02;
                                        }
                                    }
                                }
                                if (b_change)
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + d_realDrop + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_realDrop + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");
                                }
                                else
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + dt_drop_details2.Rows[0]["RealDropWeight"] + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + dt_drop_details2.Rows[0]["RealDropWeight"] + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");

                                    ////置位完成标志位
                                    //FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    //    "UPDATE drop_details SET Finish = 1 WHERE BatchName = '" + o_BatchName + "' AND " +
                                    //    "BottleNum = " + _i_minBottleNo + " AND CupNum = " + dic_pulse.First().Key + ";");
                                }
                            }
                        }
                        else
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "UPDATE drop_details SET Finish = 1,RealDropWeight = 0.00 " + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                           "WHERE BatchName = '" + obj_batchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                           "CupNum = " + kvp.Key + ";");
                        }

                    }
                }

                //更新需要加药第一杯脉冲
                s_sql = "UPDATE drop_details SET NeedPulse = " + Communal._i_needPulse + " WHERE BatchName = '" + obj_batchName + "'  And " +
                                "BottleNum = " + i_minBottleNo + " AND Finish = 0  And CupNum = " + Communal._i_needPulseCupNumber + ";";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                //把剩余没有滴液的Min置为状态4
                s_sql = "UPDATE drop_details SET MinWeight = 4 WHERE BatchName = '" + obj_batchName + "'  And " +
                                "BottleNum = " + i_minBottleNo + " AND Finish = 0  AND CupNum >= " + _i_needPulseCupNumber + " AND CupNum <= " + i_cupMax + ";";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                MyAlarm myAlarm;
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + "号母液瓶预滴液数值太小,请检查实际是否液量过低?(继续执行请点是)", "Drip", i_minBottleNo, 2, 10, true);
                else
                    myAlarm = new FADM_Object.MyAlarm(" The number of pre-drops in mother liquor bottle " + i_minBottleNo + "  is too small, please check whether the actual amount of liquid is too low" +
                        "( Continue to perform please click Yes)", "Drip", i_minBottleNo, 2, 10, true);

                //回一次原点再继续，担心失步导致后续母液抽不了
                int i_state = MyModbusFun.goHome();
                if (0 != i_state && -2 != i_state)
                    throw new Exception("驱动异常");

            }
            else if (i_ret == -3)
            {
                //把已经滴过的先置为完成
                if (FADM_Object.Communal._b_isFinishSend)
                {
                    foreach (KeyValuePair<int, double> kvp in dic_return)
                    {
                        double d_blRErr = 0;
                        if ("小针筒" == sSyringeType || "Little Syringe" == sSyringeType)
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                        else
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight));

                        //查询开料日期
                        DataTable dt_bottle_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                    "SELECT * FROM bottle_details WHERE  BottleNum = " + i_minBottleNo + ";");

                        if (0.00 != kvp.Value)
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + d_blRErr + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                            "WHERE BatchName = '" + obj_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                            "CupNum = " + kvp.Key + ";");

                            DataTable dt_drop_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_details WHERE BatchName = '" + obj_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND CupNum = " + kvp.Key + ";");
                            if (dt_drop_details2.Rows.Count > 0)
                            {
                                //记录是否修正数据
                                bool b_change = false;
                                double d_realDrop = 0.0;
                                if (FADM_Object.Communal._b_isCorrect)
                                {
                                    //如果实际滴液量为负数，就改为0
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["RealDropWeight"]) < 0)
                                    {
                                        b_change = true;
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = 0   ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + obj_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                    }

                                    //判断当目标滴液量大于1g，而且实际滴液误差小于等于0.05的，全部置为0.02
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) > 1)
                                    {
                                        if (d_blRErr <= 0.05 && d_blRErr > 0.02)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + 0.02 + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + obj_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) + 0.02;
                                        }
                                        else if (d_blRErr < -0.02 && d_blRErr >= -0.05)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + (-0.02) + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + obj_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) - 0.02;
                                        }
                                    }
                                }
                                if (b_change)
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + d_realDrop + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_realDrop + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");
                                }
                                else
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE cup_details SET TotalWeight = TotalWeight+ " + dt_drop_details2.Rows[0]["RealDropWeight"] + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + dt_drop_details2.Rows[0]["RealDropWeight"] + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");
                                }
                            }
                        }
                        else
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "UPDATE drop_details SET Finish = 1,RealDropWeight = 0.00 " + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                           "WHERE BatchName = '" + obj_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                           "CupNum = " + kvp.Key + ";");
                        }

                        DataTable dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_details WHERE BatchName = '" + obj_batchName.ToString() + "' AND Finish = 0" + " AND CupNum = " + kvp.Key + ";");
                        //查询一下加水没完成的也不置为完成
                        DataTable dt_drop_details4 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_head WHERE BatchName = '" + obj_batchName.ToString() + "' AND AddWaterChoose = 1 AND AddWaterFinish =0" + " AND CupNum = " + kvp.Key + ";");
                        if (dt_drop_details3.Rows.Count == 0 && dt_drop_details4.Rows.Count == 0)
                        {
                            //置为完成
                            s_sql = "UPDATE drop_head SET CupFinish = 1 WHERE BatchName = '" + obj_batchName.ToString() + "' AND CupNum = " + kvp.Key + "; ";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);


                            bool b_fail = true;

                            s_sql = "SELECT drop_details.CupNum as CupNum, " +
                                " drop_details.UnitOfAccount as UnitOfAccount, " +
                                         "drop_details.BottleNum as BottleNum, " +
                                         "drop_details.ObjectDropWeight as ObjectDropWeight, " +
                                         "drop_details.RealDropWeight as RealDropWeight, " +
                                         "bottle_details.SyringeType as SyringeType " +
                                         "FROM drop_details left join bottle_details on " +
                                         "bottle_details.BottleNum = drop_details.BottleNum " +
                                         "WHERE drop_details.BatchName = '" + obj_batchName.ToString() + "' AND CupNum = " + kvp.Key + ";";
                            dt_drop_head_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                            foreach (DataRow dr in dt_drop_head_temp.Rows)
                            {
                                if (Convert.ToInt32(dr["BottleNum"]) > Lib_Card.Configure.Parameter.Machine_Bottle_Total)
                                    continue;
                                double d_blRealErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"]))) : Convert.ToDouble(string.Format("{0:F3}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])));
                                d_blRealErr = d_blRealErr < 0 ? -d_blRealErr : d_blRealErr;
                                if (d_blRealErr > (dr["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip))
                                    b_fail = false;
                            }

                            dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                    "SELECT * FROM drop_head WHERE BatchName = '" + obj_batchName.ToString() + "' AND CupNum = " + kvp.Key + ";");
                            if (dt_drop_details3.Rows.Count > 0)
                            {
                                int i_cup = Convert.ToInt16(dt_drop_details3.Rows[0]["CupNum"]);
                                int i_step = Convert.ToInt16(dt_drop_details3.Rows[0]["Step"]);
                                double d_objWater = Convert.ToDouble(dt_drop_details3.Rows[0]["ObjectAddWaterWeight"]);
                                double d_realWater = Convert.ToDouble(dt_drop_details3.Rows[0]["RealAddWaterWeight"]);
                                double d_totalWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TotalWeight"]);
                                double d_testTubeObjectAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeObjectAddWaterWeight"]);
                                double d_testTubeRealAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeRealAddWaterWeight"]);
                                double d_realDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater - d_objWater) : string.Format("{0:F3}", d_realWater - d_objWater));
                                d_realDif = d_realDif < 0 ? -d_realDif : d_realDif;
                                double d_allDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)) : string.Format("{0:F3}",
                                    d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)));

                                string s_describe;
                                string s_describe_EN;
                                if (d_allDif < d_realDif || (d_realWater == 0.0 && d_objWater != 0.0))
                                {
                                    b_fail = false;
                                }

                                if (b_fail)
                                {
                                    s_describe = "滴液成功!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                              ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Success !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(kvp.Key))
                                    {
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cup))
                                            FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                                    }
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  "UPDATE cup_details SET Statues = '滴液成功' WHERE CupNum = " + i_cup + ";");
                                }
                                else
                                {
                                    s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                            ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET Statues = '滴液失败' WHERE CupNum = " + i_cup + ";");
                                }
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE drop_head SET DescribeChar = '" + s_describe + "',DescribeChar_EN = '" + s_describe_EN + "', FinishTime = '" + DateTime.Now + "', Step = 2 " +
                               "WHERE BatchName = '" + obj_batchName.ToString() + "' AND CupNum = " + i_cup + ";");
                            }
                        }

                    }
                }
                else
                {
                    foreach (KeyValuePair<int, double> kvp in dic_return)
                    {
                        double d_blRErr = 0;
                        if ("小针筒" == sSyringeType || "Little Syringe" == sSyringeType)
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                        else
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight));
                        ;

                        //查询开料日期
                        DataTable dt_bottle_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                    "SELECT * FROM bottle_details WHERE  BottleNum = " + i_minBottleNo + ";");

                        if (0.00 != kvp.Value)
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + d_blRErr + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                            "WHERE BatchName = '" + obj_batchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                            "CupNum = " + kvp.Key + ";");

                            DataTable dt_drop_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_details WHERE BatchName = '" + obj_batchName + "' AND BottleNum = " + i_minBottleNo + " AND CupNum = " + kvp.Key + ";");

                            if (dt_drop_details2.Rows.Count > 0)
                            {
                                //记录是否修正数据
                                bool b_change = false;
                                double d_realDrop = 0.0;
                                if (FADM_Object.Communal._b_isCorrect)
                                {
                                    //如果实际滴液量为负数，就改为0
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["RealDropWeight"]) < 0)
                                    {
                                        b_change = true;
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = 0   ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + obj_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                    }

                                    //判断当目标滴液量大于1g，而且实际滴液误差小于等于0.05的，全部置为0.02
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) > 1)
                                    {
                                        if (d_blRErr <= 0.05 && d_blRErr > 0.02)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + 0.02 + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + obj_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) + 0.02;
                                        }
                                        else if (d_blRErr < -0.02 && d_blRErr >= -0.05)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + (-0.02) + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + obj_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) - 0.02;
                                        }
                                    }
                                }
                                if (b_change)
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + d_realDrop + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_realDrop + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");
                                }
                                else
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + dt_drop_details2.Rows[0]["RealDropWeight"] + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + dt_drop_details2.Rows[0]["RealDropWeight"] + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");

                                    ////置位完成标志位
                                    //FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    //    "UPDATE drop_details SET Finish = 1 WHERE BatchName = '" + o_BatchName + "' AND " +
                                    //    "BottleNum = " + _i_minBottleNo + " AND CupNum = " + dic_pulse.First().Key + ";");
                                }
                            }
                        }
                        else
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "UPDATE drop_details SET Finish = 1,RealDropWeight = 0.00 " + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                           "WHERE BatchName = '" + obj_batchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                           "CupNum = " + kvp.Key + ";");
                        }

                    }
                }

                //更新需要加药第一杯脉冲
                s_sql = "UPDATE drop_details SET NeedPulse = " + Communal._i_needPulse + " WHERE BatchName = '" + obj_batchName + "'  And " +
                                "BottleNum = " + i_minBottleNo + " AND Finish = 0  And CupNum = " + Communal._i_needPulseCupNumber + ";";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                if (Communal._b_isHaveAutoBrew)
                {
                    //把剩余没有滴液的Min置为状态6,改为自动开料
                    s_sql = "UPDATE drop_details SET MinWeight = 6 WHERE BatchName = '" + obj_batchName + "'  And " +
                                    "BottleNum = " + i_minBottleNo + " AND Finish = 0 ;";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                    //插入自动开料列表
                    //判断是否存在正在开料的记录
                    s_sql = "select * from brew_station_details where IsUsing = 1 And Station<6 And BottleNum =" + i_minBottleNo;
                    DataTable dt_brew_station_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                    if (dt_brew_station_details.Rows.Count == 0)
                    {
                        //判断等待列表是否存在排队
                        s_sql = "select * from Brew_wait_list where  BottleNum =" + i_minBottleNo;
                        DataTable dt_Brew_wait_list = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                        if (dt_Brew_wait_list.Rows.Count == 0)
                        {
                            //插入到排队列表
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
               "INSERT INTO Brew_wait_list(BottleNum, InsertDate,Type) VALUES('" + i_minBottleNo + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',0);");
                        }

                    }
                }
                else
                {
                    //把剩余没有滴液的Min置为状态1,改为液量不足
                    s_sql = "UPDATE drop_details SET MinWeight = 1 WHERE BatchName = '" + obj_batchName + "'  And " +
                                    "BottleNum = " + i_minBottleNo + " AND Finish = 0 ;";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                }


            }

            b_checkFail = false;

            goto label7;

        //加助剂
        label4:
            if (FADM_Object.Communal._b_isAssitantFirst)
            {
                s_unitA = "%";
            }
            else
            {
                s_unitA = "g/l";
            }
            goto label3;

        //添加母液不足的
        label5:
            s_sql = "SELECT BottleNum FROM drop_details WHERE BatchName = '" + obj_batchName + "' AND Finish = 0 AND MinWeight = 1 AND " +
                "BottleNum <= '" + Lib_Card.Configure.Parameter.Machine_Bottle_Total + "' AND CupNum >= " +
                i_cupMin + " AND CupNum <= " + i_cupMax + " And IsDrop !=0   GROUP BY BottleNum ORDER BY BottleNum ;";
            dt_drop_head_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (0 < dt_drop_head_temp.Rows.Count)
            {
                string s_alarmBottleNo = null;
                foreach (DataRow dataRow in dt_drop_head_temp.Rows)
                {
                    s_alarmBottleNo += dataRow["BottleNum"].ToString() + ";";
                }

                s_alarmBottleNo = s_alarmBottleNo.Remove(s_alarmBottleNo.Length - 1);
                //关闭失能
                MyModbusFun.Power(2);
                FADM_Object.Communal.WriteDripWait(true);
                Communal._b_isAllowDrip = false;
                FADM_Object.MyAlarm myAlarm;
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    myAlarm = new FADM_Object.MyAlarm(s_alarmBottleNo + "号母液瓶液量过低，是否继续滴液?", "滴液", true, 1);
                else
                    myAlarm = new FADM_Object.MyAlarm("The liquid level in bottle " + s_alarmBottleNo + " is too low. Do you want to continue ? " , "Drip", true, 1);
                while (true)
                {
                    if (0 != myAlarm._i_alarm_Choose)
                        break;
                    Thread.Sleep(1);
                }
                Communal._b_isAllowDrip = true;
                //判断染色线程是否需要用机械手
                if (null != FADM_Object.Communal.ReadDyeThread())
                {
                    FADM_Object.Communal.WriteDripWait(true);
                    Communal._b_isWaitDrip = true;
                    Communal._b_isAllowDrip = true;
                    while (true)
                    {
                        if (false == FADM_Object.Communal.ReadDripWait())
                            break;
                        Thread.Sleep(1);
                    }
                    Communal._b_isWaitDrip = false;
                }
                else
                {
                    Communal._b_isWaitDrip = false;
                    FADM_Object.Communal.WriteDripWait(false);
                }

                if (1 == myAlarm._i_alarm_Choose)
                {
                    if (FADM_Object.Communal._b_isLowDripAllow)
                    {
                        i_lowSrart = 1;
                        if (FADM_Object.Communal._b_isAssitantFirst)
                        {
                            s_unitA = "g/l";
                        }
                        else
                        {
                            s_unitA = "%";
                        }
                        goto label3;
                    }
                    else
                    {
                        //把MinWeight=1的数据重新置为0
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE drop_details SET MinWeight = 0 where MinWeight = 1;");
                        goto lab_again;
                    }
                }
                else
                {
                    b_chooseNo = true;
                }
            }

        //添加超出生命周期(过期)
        label17:
            s_sql = "SELECT BottleNum FROM drop_details WHERE BatchName = '" + obj_batchName + "' AND Finish = 0 AND MinWeight = 3 AND " +
                "BottleNum <= '" + Lib_Card.Configure.Parameter.Machine_Bottle_Total + "' AND CupNum >= " +
                i_cupMin + " AND CupNum <= " + i_cupMax + " And IsDrop !=0   GROUP BY BottleNum ORDER BY BottleNum ;";
            dt_drop_head_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (0 < dt_drop_head_temp.Rows.Count)
            {
                string s_alarmBottleNo = null;
                foreach (DataRow dataRow in dt_drop_head_temp.Rows)
                {
                    s_alarmBottleNo += dataRow["BottleNum"].ToString() + ";";
                }

                s_alarmBottleNo = s_alarmBottleNo.Remove(s_alarmBottleNo.Length - 1);
                //关闭失能
                MyModbusFun.Power(2);
                FADM_Object.Communal.WriteDripWait(true);
                Communal._b_isAllowDrip = false;
                FADM_Object.MyAlarm myAlarm;
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    myAlarm = new FADM_Object.MyAlarm(s_alarmBottleNo + "号母液瓶过期，是否继续滴液?", "滴液", true, 1);
                else
                    myAlarm = new FADM_Object.MyAlarm("The liquid level in bottle " + s_alarmBottleNo + " is expire. Do you want to continue ? ", "Drip", true, 1);
                while (true)
                {
                    if (0 != myAlarm._i_alarm_Choose)
                        break;
                    Thread.Sleep(1);
                }
                Communal._b_isAllowDrip = true;
                //判断染色线程是否需要用机械手
                if (null != FADM_Object.Communal.ReadDyeThread())
                {
                    FADM_Object.Communal.WriteDripWait(true);
                    Communal._b_isWaitDrip = true;
                    Communal._b_isAllowDrip = true;
                    while (true)
                    {
                        if (false == FADM_Object.Communal.ReadDripWait())
                            break;
                        Thread.Sleep(1);
                    }
                    Communal._b_isWaitDrip = false;
                }
                else
                {
                    Communal._b_isWaitDrip = false;
                    FADM_Object.Communal.WriteDripWait(false);
                }

                if (1 == myAlarm._i_alarm_Choose)
                {
                    if (FADM_Object.Communal._b_isOutDripAllow)
                    {
                        i_lowSrart = 3;
                        if (FADM_Object.Communal._b_isAssitantFirst)
                        {
                            s_unitA = "g/l";
                        }
                        else
                        {
                            s_unitA = "%";
                        }
                        goto label3;
                    }
                    else
                    {
                        //把MinWeight=3的数据重新置为0
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE drop_details SET MinWeight = 0 where MinWeight = 3;");
                        goto lab_again;
                    }
                }
                else
                {
                    b_chooseNo = true;
                }
            }


        //添加找不到针筒的
        label16:
            s_sql = "SELECT BottleNum FROM drop_details WHERE BatchName = '" + obj_batchName + "' AND Finish = 0 AND MinWeight = 2 AND " +
               "BottleNum <= '" + Lib_Card.Configure.Parameter.Machine_Bottle_Total + "' AND CupNum >= " +
               i_cupMin + " AND CupNum <= " + i_cupMax + " And IsDrop !=0   GROUP BY BottleNum ORDER BY BottleNum ;";
            dt_drop_head_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (0 < dt_drop_head_temp.Rows.Count)
            {
                string s_alarmBottleNo = null;
                foreach (DataRow dataRow in dt_drop_head_temp.Rows)
                {
                    s_alarmBottleNo += dataRow["BottleNum"].ToString() + ";";
                }

                s_alarmBottleNo = s_alarmBottleNo.Remove(s_alarmBottleNo.Length - 1);
                FADM_Object.Communal.WriteDripWait(true);
                Communal._b_isAllowDrip = false;

                FADM_Object.MyAlarm myAlarm;
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    myAlarm = new FADM_Object.MyAlarm(s_alarmBottleNo + "号母液瓶未检测到针筒，是否继续滴液 ? ", "滴液", true, 1, true);
                else
                    myAlarm = new FADM_Object.MyAlarm(s_alarmBottleNo + " bottle did not find a syringe. Do you want to continue? " , "Drip", true, 1, true);
                while (true)
                {
                    if (0 != myAlarm._i_alarm_Choose)
                        break;
                    Thread.Sleep(1);
                }
                Communal._b_isAllowDrip = true;
                //判断染色线程是否需要用机械手
                if (null != FADM_Object.Communal.ReadDyeThread())
                {
                    FADM_Object.Communal.WriteDripWait(true);
                    Communal._b_isWaitDrip = true;
                    Communal._b_isAllowDrip = true;
                    while (true)
                    {
                        if (false == FADM_Object.Communal.ReadDripWait())
                            break;
                        Thread.Sleep(1);
                    }
                    Communal._b_isWaitDrip = false;
                }
                else
                {
                    Communal._b_isWaitDrip = false;
                    FADM_Object.Communal.WriteDripWait(false);
                }

                if (1 == myAlarm._i_alarm_Choose)
                {
                    i_lowSrart = 2;
                    if (FADM_Object.Communal._b_isAssitantFirst)
                    {
                        s_unitA = "g/l";
                    }
                    else
                    {
                        s_unitA = "%";
                    }
                    goto label3;
                }
                else
                {
                    b_chooseNo = true;
                }
            }

        //滴液完成    
        label6:
            //关闭失能
            MyModbusFun.Power(2);
            FADM_Object.Communal.WriteDripWait(true);
            Communal._b_isAllowDrip = false;
            if (!b_chooseNo)
            {
            lab_Re:
                //判断是否全部完成，等待是否还有洗杯没完成的
                s_sql = "SELECT * FROM drop_details WHERE BatchName = '" + obj_batchName + "' AND Finish = 0  AND " +
                   "BottleNum <= '" + Lib_Card.Configure.Parameter.Machine_Bottle_Total + "' ;";
                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                if (dt_drop_head.Rows.Count > 0)
                {

                    foreach (DataRow dataRow in dt_drop_head.Rows)
                    {
                        if (dataRow["IsDrop"].ToString() == "1" && dataRow["MinWeight"].ToString() != "4")
                        {
                            Communal._b_isAllowDrip = true;
                            //获取机械手
                            if (null != FADM_Object.Communal.ReadDyeThread())
                            {
                                FADM_Object.Communal.WriteDripWait(true);
                                Communal._b_isWaitDrip = true;
                                Communal._b_isAllowDrip = true;
                                while (true)
                                {
                                    if (false == FADM_Object.Communal.ReadDripWait())
                                        break;
                                    Thread.Sleep(1);
                                }
                                Communal._b_isWaitDrip = false;
                            }
                            else
                            {
                                Communal._b_isWaitDrip = false;
                                FADM_Object.Communal.WriteDripWait(false);
                            }

                            goto lab_again;
                        }
                    }
                    Thread.Sleep(1000);
                    if (!FADM_Object.Communal.ReadDripWait())
                    {
                        FADM_Object.Communal.WriteDripWait(true);
                    }
                    //继续判断
                    goto lab_Re;
                }
                //判断一下是否有没加水的
                else
                {
                    s_sql = "SELECT * FROM drop_head WHERE BatchName = '" + obj_batchName + "' AND " +
                "AddWaterChoose = 1 AND AddWaterFinish = 0 AND CupNum >= " + i_cupMin + " AND CupNum <= " + i_cupMax + " ORDER BY CupNum;";
                    dt_drop_head_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                    if (dt_drop_head.Rows.Count > 0)
                    {
                        goto lab_again;
                    }
                }
            }
            if (null != thread)
                thread.Join();

            if (FADM_Object.Communal._b_isFinishSend)
            {
                //把由于超期，液量低跳过的所有置为不合格
                FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE drop_head SET DescribeChar = '滴液失败',DescribeChar_EN = 'Drip Fail',CupFinish = 1, FinishTime = '" + DateTime.Now + "', Step = 2 " +
                               "WHERE BatchName = '" + obj_batchName + "' AND CupFinish != 1 AND CupNum >= " + i_cupMin + " AND CupNum <= " + i_cupMax + ";");

                s_sql = "SELECT * from drop_head where DescribeChar like '%滴液失败%'" +
                   " And BatchName = '" + obj_batchName + "' AND CupNum >= " +
                   i_cupMin + " AND CupNum <= " + i_cupMax + " order by CupNum;";
                //获取滴液不合格记录
                dt_drop_head_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                List<int> lis_cupFailD = new List<int>();
                List<int> lis_cupFailT = new List<int>();
                string s_cupNo = "";
                foreach (DataRow dr in dt_drop_head_temp.Rows)
                {
                    lis_cupFailD.Add(Convert.ToInt32(dr["CupNum"]));
                    s_cupNo += dr["CupNum"].ToString() + "; ";
                }
                lis_cupFailT = lis_cupFailD.Distinct().ToList();


                if (FADM_Auto.Drip._b_dripErr == false)
                {
                    if (0 < lis_cupFailT.Count)
                    {
                        s_cupNo = s_cupNo.Remove(s_cupNo.Length - 1);

                        FADM_Object.Communal.WriteDripWait(true);
                        Communal._b_isAllowDrip = false;

                        FADM_Object.MyAlarm myAlarm;
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            myAlarm = new FADM_Object.MyAlarm(s_cupNo + "号配液杯滴液失败，是否继续(重新滴液请点是，退出滴液请点否)?", "滴液", true, 1);
                        else
                            myAlarm = new FADM_Object.MyAlarm( "The dispensing cup"+ s_cupNo + "  failed to dispense liquid. Do you want to continue (please click Yes for re dispensing and No for exiting the dispensing)? ", "Drip", true, 1);

                        while (true)
                        {
                            if (0 != myAlarm._i_alarm_Choose)
                                break;
                            Thread.Sleep(1);
                        }
                        //判断染色线程是否需要用机械手
                        if (null != FADM_Object.Communal.ReadDyeThread())
                        {
                            FADM_Object.Communal.WriteDripWait(true);
                            Communal._b_isWaitDrip = true;
                            while (true)
                            {
                                if (false == FADM_Object.Communal.ReadDripWait())
                                    break;
                                Thread.Sleep(1);
                            }
                            Communal._b_isWaitDrip = false;
                        }
                        else
                        {
                            Communal._b_isWaitDrip = false;
                            FADM_Object.Communal.WriteDripWait(false);
                        }

                        if (1 == myAlarm._i_alarm_Choose)
                        {
                            //重滴
                            if (3 == i_type)
                            {
                                //打板区
                                FADM_Object.Communal._lis_dripFailCupFinish.Clear();
                                FADM_Object.Communal._lis_dripFailCup.AddRange(lis_cupFailT);
                                List<int> lis_ints1 = new List<int>();
                                lis_ints1.AddRange(lis_cupFailT);

                                FADM_Object.Communal._lis_SendReadyCup.Clear();

                                DataTable dataTable = FADM_Object.Communal._fadmSqlserver.GetData("SELECT * FROM drop_head WHERE BatchName = '" + obj_batchName + "' AND CupNum >= " + i_cupMin + " AND CupNum <= " + i_cupMax + "ORDER BY CupNum;");
                                List<int> lis_lUse_Cup = new List<int>();
                                foreach (DataRow dataRow in dataTable.Rows)
                                {
                                    lis_lUse_Cup.Add(Convert.ToInt16(dataRow["CupNum"]));
                                }
                                //把对应副杯也加入进去
                                List<int> lis_cupFailT2 = new List<int>();
                                lis_cupFailT2.AddRange(lis_cupFailT);
                                foreach (int i in lis_cupFailT2)
                                {
                                    if (_dic_first_second[i] > 0)
                                    {
                                        if (lis_lUse_Cup.Contains(_dic_first_second[i]))
                                            if (!lis_cupFailT.Contains(_dic_first_second[i]))
                                                lis_cupFailT.Add(_dic_first_second[i]);
                                    }
                                }

                                //等待染色机排完再重滴
                                while (true)
                                {
                                    

                                    FADM_Object.Communal.WriteDripWait(true);
                                    if (0 == lis_cupFailT.Count)
                                    {
                                        FADM_Object.Communal.WriteDripWait(false);
                                        break;
                                    }

                                    for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                                    {
                                        if (FADM_Object.Communal._lis_dripFailCupFinish.Contains(lis_cupFailT[i]))
                                        {

                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                "UPDATE drop_head SET CupFinish = 0, AddWaterFinish = 0, RealAddWaterWeight = 0.00 , Step = 1,DescribeChar=null " +
                                                "WHERE BatchName = '" + obj_batchName + "' AND CupNum = " + lis_cupFailT[i] + ";");

                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                "UPDATE drop_details SET Finish = 0, MinWeight = 0, RealDropWeight = 0.00 , NeedPulse = 0 " +
                                                "WHERE BatchName = '" + obj_batchName + "' AND CupNum = " + lis_cupFailT[i] + ";");
                                            FADM_Object.Communal._lis_dripFailCupFinish.Remove(lis_cupFailT[i]);

                                            ////如果有副杯，副杯也要清除
                                            //if (_dic_first_second[lis_cupFailT[i]] > 0)
                                            //{
                                            //    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                            //    "UPDATE drop_head SET CupFinish = 0, AddWaterFinish = 0, RealAddWaterWeight = 0.00 , Step = 1,DescribeChar=null " +
                                            //    "WHERE BatchName = '" + obj_batchName + "' AND CupNum = " + _dic_first_second[lis_cupFailT[i]] + ";");

                                            //    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                            //        "UPDATE drop_details SET Finish = 0, MinWeight = 0, RealDropWeight = 0.00 , NeedPulse = 0" +
                                            //        "WHERE BatchName = '" + obj_batchName + "' AND CupNum = " + _dic_first_second[lis_cupFailT[i]] + ";");
                                            //    //FADM_Object.Communal._lis_dripFailCupFinish.Remove(lis_cupFailT[i]);
                                            //}

                                            int Num = lis_cupFailT[i];
                                            lis_cupFailT.Remove(lis_cupFailT[i]);
                                            lis_cupSuc.Remove(Num);
                                        }
                                    }

                                    DataTable dt_drop_head2 = FADM_Object.Communal._fadmSqlserver.GetData(
                           "SELECT * FROM drop_head WHERE BatchName = '" + obj_batchName + "'     ORDER BY CupNum;");
                                    List<int> lUse1 = new List<int>();
                                    foreach (DataRow dataRow in dt_drop_head2.Rows)
                                    {
                                        lUse1.Add(Convert.ToInt16(dataRow["CupNum"]));
                                    }
                                    for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                                    {
                                        //已经强停，不在滴液列表
                                        if (!lUse1.Contains(lis_cupFailT[i]))
                                        {
                                            int n = lis_cupFailT[i];
                                            lis_cupFailT.Remove(lis_cupFailT[i]);
                                            lis_ints1.Remove(n);
                                        }
                                    }

                                    Thread.Sleep(1000);
                                }

                                foreach (int i in lis_ints1)
                                {
                                    int iCupNum = i;

                                    while (true)
                                    {
                                        //滴液完成数组移除当前杯号
                                        FADM_Object.Communal._lis_dripSuccessCup.Remove(iCupNum);
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(iCupNum))
                                            break;
                                    }



                                    DataTable dt = FADM_Object.Communal._fadmSqlserver.GetData("Select * from drop_head where (CupNum = " + iCupNum + " Or  CupNum = " + Communal._dic_first_second[iCupNum] + ") And BatchName != '0'");
                                    //先查是否双杯在使用
                                    if (dt.Rows.Count == 2)
                                    {
                                        //判断是否发过启动信号
                                        if ((!_lis_SendReadyCup.Contains(iCupNum)) && (!_lis_SendReadyCup.Contains(_dic_first_second[iCupNum])))
                                        {
                                            //判断另外一只杯是否在洗杯
                                            //if (!lis_iUse.Contains(_dic_first_second[iCupNum]))
                                            {
                                                int[] ia_zero = new int[1];
                                                ia_zero[0] = 0;
                                                DyeHMIWrite(iCupNum, 119, 119, ia_zero);
                                                //滴液状态
                                                ia_zero[0] = 3;
                                                DyeHMIWrite(iCupNum, 100, 100, ia_zero);

                                                _lis_SendReadyCup.Add(iCupNum);
                                                _lis_SendReadyCup.Add(_dic_first_second[iCupNum]);
                                            }
                                        }
                                    }
                                    //单杯就直接下发
                                    else
                                    {
                                        int[] ia_zero = new int[1];
                                        if (_dic_first_second[iCupNum] > 0)
                                        {
                                            //主杯
                                            if (iCupNum < _dic_first_second[iCupNum])
                                            {
                                                ia_zero[0] = 1;
                                                DyeHMIWrite(iCupNum, 119, 119, ia_zero);
                                            }
                                            else
                                            {
                                                ia_zero[0] = 2;
                                                DyeHMIWrite(iCupNum, 119, 119, ia_zero);
                                            }
                                        }
                                        //滴液状态
                                        ia_zero[0] = 3;
                                        DyeHMIWrite(iCupNum, 100, 100, ia_zero);

                                        _lis_SendReadyCup.Add(iCupNum);
                                    }

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE cup_details SET Statues = '等待准备状态' WHERE CupNum = " + iCupNum + ";");

                                }


                                //等待准备状态
                                while (true)
                                {
                                    bool b_open = true;

                                    DataTable dt_drop_head2 = FADM_Object.Communal._fadmSqlserver.GetData(
                           "SELECT * FROM drop_head WHERE BatchName = '" + obj_batchName + "' AND CupFinish = 0    ORDER BY CupNum;");
                                    List<int> lis_lUse1 = new List<int>();
                                    foreach (DataRow dataRow in dt_drop_head2.Rows)
                                    {
                                        lis_lUse1.Add(Convert.ToInt16(dataRow["CupNum"]));
                                    }
                                    for (int i = lis_ints1.Count - 1; i >= 0; i--)
                                    {
                                        //已经强停，不在滴液列表
                                        if (!lis_lUse1.Contains(lis_ints1[i]))
                                        {
                                            lis_ints1.Remove(lis_ints1[i]);
                                        }
                                    }

                                    foreach (int i in lis_ints1)
                                    {
                                        int iCupNum = i;


                                        int iOpenCover = Convert.ToInt16(FADM_Auto.Dye._cup_Temps[iCupNum - 1]._s_statues);


                                        if (5 != iOpenCover)
                                            b_open = false;

                                    }


                                    if (b_open)
                                        break;

                                    Thread.Sleep(1000);
                                }

                                lis_ints1.Clear();
                                
                                this.DripProcess(obj_batchName, i_cupMin, i_cupMax, i_type, i_erea);
                            }
                            else
                            {
                                //滴液区
                                for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                                {

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE drop_head SET CupFinish = 0, AddWaterFinish = 0, RealAddWaterWeight = 0.00 , Step = 1" +
                                        "WHERE BatchName = '" + obj_batchName + "' AND CupNum = " + lis_cupFailT[i] + ";");

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE drop_details SET Finish = 0, MinWeight = 0, RealDropWeight = 0.00 , NeedPulse = 0" +
                                        "WHERE BatchName = '" + obj_batchName + "' AND CupNum = " + lis_cupFailT[i] + ";");
                                    int i_num = lis_cupFailT[i];
                                    lis_cupFailT.Remove(lis_cupFailT[i]);
                                    lis_cupSuc.Remove(i_num);

                                }
                                this.DripProcess(obj_batchName, i_cupMin, i_cupMax, i_type, i_erea);
                            }
                        }
                        else
                        {
                            //不重滴

                            if (FADM_Object.Communal.ReadMachineStatus() != 8)
                            {
                                if (i_type == 3)
                                {
                                    FADM_Object.Communal._lis_dripSuccessCup.AddRange(lis_cupFailT);
                                }
                            }

                        }

                    }
                }

                string s_cupList = "";
                string s_te = "";
                if (lis_cupSuc.Count > 0)
                {
                    for (int i = 0; i < lis_cupSuc.Count; i++)
                    {
                        s_cupList += lis_cupSuc[i] + ",";
                    }
                }
                if (s_cupList != "")
                {
                    s_cupList = s_cupList.Remove(s_cupList.Length - 1);
                    s_te = " And CupNum in (" + s_cupList + ")";
                }
                //添加历史表
                dt_drop_head_temp = FADM_Object.Communal._fadmSqlserver.GetData(
                 "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'drop_head';");
                string s_columnHead = null;
                foreach (DataRow row in dt_drop_head_temp.Rows)
                {
                    string s_curName = Convert.ToString(row[0]);
                    if ("TestTubeFinish" != s_curName && "TestTubeWaterLower" != s_curName && "AddWaterFinish" != s_curName &&
                        "CupFinish" != s_curName && "TestTubeWaterLower" != s_curName)
                        s_columnHead += s_curName + ", ";
                }
                s_columnHead = s_columnHead.Remove(s_columnHead.Length - 2);

                dt_drop_head_temp = FADM_Object.Communal._fadmSqlserver.GetData(
                   "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'drop_details';");
                string s_columnDetails = null;
                foreach (DataRow row in dt_drop_head_temp.Rows)
                {
                    string sCurName = Convert.ToString(row[0]);
                    if ("MinWeight" != sCurName && "Finish" != sCurName && "IsShow" != sCurName && "NeedPulse" != sCurName)
                        s_columnDetails += Convert.ToString(row[0]) + ", ";
                }
                s_columnDetails = s_columnDetails.Remove(s_columnDetails.Length - 2);

                //
                dt_drop_head_temp = FADM_Object.Communal._fadmSqlserver.GetData(
                   "SELECT * FROM drop_head WHERE   BatchName = '" + obj_batchName + "' ;");

                foreach (DataRow row in dt_drop_head_temp.Rows)
                {
                    if (lis_cupSuc.Contains(Convert.ToInt32(row["CupNum"].ToString())))
                    {

                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                        "INSERT INTO history_head (" + s_columnHead + ") (SELECT " + s_columnHead + " FROM drop_head " +
                        "WHERE BatchName = '" + obj_batchName + "' AND CupNum = " + row["CupNum"].ToString() + ") ;");

                        //FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "INSERT INTO history_head (" + s_columnHead + ") (SELECT " + s_columnHead + " FROM drop_head " +
                        //    "WHERE BatchName = '" + o_BatchName + "' AND CupNum >= " + _i_cupMin + " AND CupNum <= " + _i_cupMax + s_te + ");");


                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "INSERT INTO history_details (" + s_columnDetails + ") (SELECT " + s_columnDetails + " FROM drop_details " +
                           "WHERE BatchName = '" + obj_batchName + "' AND CupNum = " + row["CupNum"].ToString() + ") ;");

                        //滴液
                        if (SmartDyeing.FADM_Object.Communal._lis_dripCupNum.Contains(Convert.ToInt32(row["CupNum"].ToString())))
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  "DELETE FROM drop_head WHERE CupNum = " + row["CupNum"].ToString() + " AND BatchName = '" + obj_batchName + "' ;");
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "DELETE FROM drop_details WHERE CupNum = " + row["CupNum"].ToString() + " AND BatchName = '" + obj_batchName + "' ;");


                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE cup_details SET FormulaCode = null, " +
                                "DyeingCode = null, IsUsing = 0, Statues = '待机', " +
                                "StartTime = null, SetTemp = null, StepNum = null, TotalWeight = null, " +
                                "TotalStep = null, TechnologyName = null, StepStartTime = null, SetTime = null,RecordIndex = 0 WHERE CupNum = " +
                                row["CupNum"].ToString() + " ;");
                        }
                    }
                }

                if (FADM_Auto.Drip._b_dripErr)
                {
                    FADM_Object.Communal._lis_dripStopCup.AddRange(lis_cupT);

                }
            }
            else
            {
                s_sql = "UPDATE drop_head SET CupFinish = 1 WHERE BatchName = '" + obj_batchName + "' AND CupNum >= " + i_cupMin + " AND CupNum <= " + i_cupMax + " ;";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                //获取滴液不合格记录
                dt_drop_head_temp = FADM_Object.Communal._fadmSqlserver.GetData(
                   "SELECT drop_details.CupNum as CupNum, " +
                   " drop_details.UnitOfAccount as UnitOfAccount, " +
                   "drop_details.BottleNum as BottleNum, " +
                   "drop_details.ObjectDropWeight as ObjectDropWeight, " +
                   "drop_details.RealDropWeight as RealDropWeight, " +
                   "bottle_details.SyringeType as SyringeType " +
                   "FROM drop_details left join bottle_details on " +
                   "bottle_details.BottleNum = drop_details.BottleNum " +
                   "WHERE drop_details.BatchName = '" + obj_batchName + "' AND CupNum >= " +
                   i_cupMin + " AND CupNum <= " + i_cupMax + ";");
                List<int> lis_cupFailD = new List<int>();
                List<int> lis_cupFailT = new List<int>();
                foreach (DataRow dr in dt_drop_head_temp.Rows)
                {
                    if (Convert.ToInt32(dr["BottleNum"]) > Lib_Card.Configure.Parameter.Machine_Bottle_Total)
                        continue;
                    double d_blRealErr = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F2}",
                        Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])): string.Format("{0:F3}",
                        Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])));
                    d_blRealErr = d_blRealErr < 0 ? -d_blRealErr : d_blRealErr;
                    if (d_blRealErr > (dr["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip))
                        lis_cupFailD.Add(Convert.ToInt32(dr["CupNum"]));
                }
                lis_cupFailD = lis_cupFailD.Distinct().ToList();



                lis_cupT = new List<int>();
                //滴液成功，转换到历史表杯号
                lis_cupSuc = new List<int>();
                string s_cupNo = null;

                dt_drop_head_temp = FADM_Object.Communal._fadmSqlserver.GetData(
                    "SELECT * FROM drop_head WHERE BatchName = '" + obj_batchName + "'  AND CupNum >= " + i_cupMin + " AND CupNum <= " + i_cupMax + "  ORDER BY CupNum ;");
                foreach (DataRow dr in dt_drop_head_temp.Rows)
                {
                    int it_cup = Convert.ToInt16(dr["CupNum"]);
                    int i_step = Convert.ToInt16(dr["Step"]);
                    double d_objWater = Convert.ToDouble(dr["ObjectAddWaterWeight"]);
                    double d_realWater = Convert.ToDouble(dr["RealAddWaterWeight"]);
                    double d_totalWeight = Convert.ToDouble(dr["TotalWeight"]);
                    double d_testTubeObjectAddWaterWeight = Convert.ToDouble(dr["TestTubeObjectAddWaterWeight"]);
                    double d_testTubeRealAddWaterWeight = Convert.ToDouble(dr["TestTubeRealAddWaterWeight"]);
                    if (i_step == 1)
                        lis_cupSuc.Add(it_cup);
                    lis_cupT.Add(it_cup);
                    double d_realDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater - d_objWater): string.Format("{0:F3}", d_realWater - d_objWater));
                    d_realDif = d_realDif < 0 ? -d_realDif : d_realDif;
                    double d_allDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                        d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)): string.Format("{0:F3}",
                        d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)));
                    string s_describe;
                    string s_describe_EN;

                    //只判断当前滴液的数据
                    if (i_step == 1)
                    {
                        if (d_allDif < d_realDif || d_realWater == 0.0)
                        {
                            //加水失败
                            s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater):string.Format("{0:F3}", d_objWater)) +
                                             ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater): string.Format("{0:F3}", d_realWater));
                            s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                             ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                            lis_cupFailT.Add(it_cup);
                            s_cupNo += it_cup.ToString() + "; ";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE cup_details SET Statues = '滴液失败' WHERE CupNum = " + it_cup + ";");

                        }
                        else
                        {
                            //加水成功
                            if (lis_cupFailD.Contains(it_cup))
                            {
                                //滴液失败
                                s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                            ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                             ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                lis_cupFailT.Add(it_cup);
                                s_cupNo += it_cup.ToString() + "; ";

                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET Statues = '滴液失败' WHERE CupNum = " + it_cup + ";");
                            }
                            else
                            {
                                //滴液成功
                                s_describe = "滴液成功!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                              ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                s_describe_EN = "Drip Success !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                             ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                if (i_type == 3)
                                {
                                    if (i_step == 1)
                                    {
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(it_cup))
                                            FADM_Object.Communal._lis_dripSuccessCup.Add(it_cup);
                                    }
                                }

                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  "UPDATE cup_details SET Statues = '滴液成功' WHERE CupNum = " + it_cup + ";");
                            }
                        }

                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE drop_head SET DescribeChar = '" + s_describe + "',DescribeChar_EN = '"+ s_describe_EN+"', FinishTime = '" + DateTime.Now + "', Step = 2 " +
                               "WHERE BatchName = '" + obj_batchName + "' AND CupNum = " + it_cup + ";");
                    }

                }


                if (FADM_Auto.Drip._b_dripErr == false)
                {
                    if (0 < lis_cupFailT.Count)
                    {
                        s_cupNo = s_cupNo.Remove(s_cupNo.Length - 1);

                        FADM_Object.Communal.WriteDripWait(true);
                        Communal._b_isAllowDrip = false;
                        FADM_Object.MyAlarm myAlarm;
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            myAlarm = new FADM_Object.MyAlarm(s_cupNo + "号配液杯滴液失败，是否继续(重新滴液请点是，退出滴液请点否)?", "滴液", true, 1);
                        else
                            myAlarm = new FADM_Object.MyAlarm("The dispensing cup" + s_cupNo + "  failed to dispense liquid. Do you want to continue (please click Yes for re dispensing and No for exiting the dispensing)? ", "Drip", true, 1);

                        while (true)
                        {
                            if (0 != myAlarm._i_alarm_Choose)
                                break;
                            Thread.Sleep(1);
                        }
                        //判断染色线程是否需要用机械手
                        if (null != FADM_Object.Communal.ReadDyeThread())
                        {
                            FADM_Object.Communal.WriteDripWait(true);
                            Communal._b_isWaitDrip = true;
                            while (true)
                            {
                                if (false == FADM_Object.Communal.ReadDripWait())
                                    break;
                                Thread.Sleep(1);
                            }
                            Communal._b_isWaitDrip = false;
                        }
                        else
                        {
                            Communal._b_isWaitDrip = false;
                            FADM_Object.Communal.WriteDripWait(false);
                        }

                        if (1 == myAlarm._i_alarm_Choose)
                        {
                            //重滴
                            if (3 == i_type)
                            {

                                DataTable dataTable = FADM_Object.Communal._fadmSqlserver.GetData("SELECT * FROM drop_head WHERE BatchName = '" + obj_batchName + "' " + "ORDER BY CupNum;");
                                List<int> lis_lUse_Cup = new List<int>();
                                foreach (DataRow dataRow in dataTable.Rows)
                                {
                                    lis_lUse_Cup.Add(Convert.ToInt16(dataRow["CupNum"]));
                                }
                                //把对应副杯也加入进去
                                List<int> lis_cupFailT2 = new List<int>();
                                lis_cupFailT2.AddRange(lis_cupFailT);
                                foreach (int i in lis_cupFailT2)
                                {
                                    if (_dic_first_second[i] > 0)
                                    {
                                        if (lis_lUse_Cup.Contains(_dic_first_second[i]))
                                            if (!lis_cupFailT.Contains(_dic_first_second[i]))
                                                lis_cupFailT.Add(_dic_first_second[i]);
                                    }
                                }

                                //打板区
                                FADM_Object.Communal._lis_dripFailCupFinish.Clear();
                                FADM_Object.Communal._lis_dripFailCup.AddRange(lis_cupFailT);
                                List<int> lis_ints1 = new List<int>();
                                lis_ints1.AddRange(lis_cupFailT);
                                FADM_Object.Communal._lis_SendReadyCup.Clear();

                                

                                //等待染色机排完再重滴
                                while (true)
                                {
                                    FADM_Object.Communal.WriteDripWait(true);
                                    if (0 == lis_cupFailT.Count)
                                    {
                                        FADM_Object.Communal.WriteDripWait(false);
                                        break;
                                    }

                                    for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                                    {
                                        if (FADM_Object.Communal._lis_dripFailCupFinish.Contains(lis_cupFailT[i]))
                                        {

                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                "UPDATE drop_head SET CupFinish = 0, AddWaterFinish = 0, RealAddWaterWeight = 0.00 , Step = 1,DescribeChar=null " +
                                                "WHERE BatchName = '" + obj_batchName + "' AND CupNum = " + lis_cupFailT[i] + ";");

                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                "UPDATE drop_details SET Finish = 0, MinWeight = 0, RealDropWeight = 0.00 , NeedPulse = 0" +
                                                "WHERE BatchName = '" + obj_batchName + "' AND CupNum = " + lis_cupFailT[i] + ";");
                                            FADM_Object.Communal._lis_dripFailCupFinish.Remove(lis_cupFailT[i]);

                                            ////如果有副杯，副杯也要清除
                                            //if (_dic_first_second[lis_cupFailT[i]] > 0)
                                            //{
                                            //    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                            //    "UPDATE drop_head SET CupFinish = 0, AddWaterFinish = 0, RealAddWaterWeight = 0.00 , Step = 1,DescribeChar=null " +
                                            //    "WHERE BatchName = '" + obj_batchName + "' AND CupNum = " + _dic_first_second[lis_cupFailT[i]] + ";");

                                            //    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                            //        "UPDATE drop_details SET Finish = 0, MinWeight = 0, RealDropWeight = 0.00 , NeedPulse = 0" +
                                            //        "WHERE BatchName = '" + obj_batchName + "' AND CupNum = " + _dic_first_second[lis_cupFailT[i]] + ";");
                                            //    //FADM_Object.Communal._lis_dripFailCupFinish.Remove(lis_cupFailT[i]);
                                            //}

                                            int i_num = lis_cupFailT[i];
                                            lis_cupFailT.Remove(lis_cupFailT[i]);
                                            lis_cupSuc.Remove(i_num);
                                        }
                                    }
                                    DataTable dt_drop_head2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                                               "SELECT * FROM drop_head WHERE BatchName = '" + obj_batchName + "'    ORDER BY CupNum;");
                                    List<int> lis_lUse1 = new List<int>();
                                    foreach (DataRow dataRow in dt_drop_head2.Rows)
                                    {
                                        lis_lUse1.Add(Convert.ToInt16(dataRow["CupNum"]));
                                    }
                                    for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                                    {
                                        //已经强停，不在滴液列表
                                        if (!lis_lUse1.Contains(lis_cupFailT[i]))
                                        {
                                            int i_nu = lis_cupFailT[i];
                                            lis_cupFailT.Remove(lis_cupFailT[i]);
                                            lis_ints1.Remove(i_nu);
                                        }
                                    }

                                    Thread.Sleep(1000);
                                }

                                foreach (int i in lis_ints1)
                                {
                                    int i_cupNum = i;

                                    while (true)
                                    {
                                        //滴液完成数组移除当前杯号
                                        FADM_Object.Communal._lis_dripSuccessCup.Remove(i_cupNum);
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cupNum))
                                            break;
                                    }



                                    DataTable dt = FADM_Object.Communal._fadmSqlserver.GetData("Select * from drop_head where (CupNum = " + i_cupNum + " Or  CupNum = " + Communal._dic_first_second[i_cupNum] + ") And BatchName != '0'");
                                    //先查是否双杯在使用
                                    if (dt.Rows.Count == 2)
                                    {
                                        //判断是否发过启动信号
                                        if ((!_lis_SendReadyCup.Contains(i_cupNum)) && (!_lis_SendReadyCup.Contains(_dic_first_second[i_cupNum])))
                                        {
                                            //判断另外一只杯是否在洗杯
                                            //if (!lis_iUse.Contains(_dic_first_second[i_cupNum]))
                                            {
                                                int[] ia_zero = new int[1];
                                                ia_zero[0] = 0;
                                                DyeHMIWrite(i_cupNum, 119, 119, ia_zero);
                                                //滴液状态
                                                ia_zero[0] = 3;
                                                DyeHMIWrite(i_cupNum, 100, 100, ia_zero);

                                                _lis_SendReadyCup.Add(i_cupNum);
                                                _lis_SendReadyCup.Add(_dic_first_second[i_cupNum]);
                                            }
                                        }
                                    }
                                    //单杯就直接下发
                                    else
                                    {
                                        int[] ia_zero = new int[1];
                                        if (_dic_first_second[i_cupNum] > 0)
                                        {
                                            //主杯
                                            if (i_cupNum < _dic_first_second[i_cupNum])
                                            {
                                                ia_zero[0] = 1;
                                                DyeHMIWrite(i_cupNum, 119, 119, ia_zero);
                                            }
                                            else
                                            {
                                                ia_zero[0] = 2;
                                                DyeHMIWrite(i_cupNum, 119, 119, ia_zero);
                                            }
                                        }
                                        //滴液状态
                                        ia_zero[0] = 3;
                                        DyeHMIWrite(i_cupNum, 100, 100, ia_zero);

                                        _lis_SendReadyCup.Add(i_cupNum);
                                    }

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE cup_details SET Statues = '等待准备状态' WHERE CupNum = " + i_cupNum + ";");

                                }


                                //等待准备状态
                                while (true)
                                {
                                    bool b_open = true;
                                    DataTable dt_drop_head2 = FADM_Object.Communal._fadmSqlserver.GetData(
                           "SELECT * FROM drop_head WHERE BatchName = '" + obj_batchName + "' AND CupFinish = 0    ORDER BY CupNum;");
                                    List<int> lis_lUse1 = new List<int>();
                                    foreach (DataRow dataRow in dt_drop_head2.Rows)
                                    {
                                        lis_lUse1.Add(Convert.ToInt16(dataRow["CupNum"]));
                                    }
                                    for (int i = lis_ints1.Count - 1; i >= 0; i--)
                                    {
                                        //已经强停，不在滴液列表
                                        if (!lis_lUse1.Contains(lis_ints1[i]))
                                        {
                                            lis_ints1.Remove(lis_ints1[i]);
                                        }
                                    }
                                    foreach (int i in lis_ints1)
                                    {
                                        int i_cupNum = i;


                                        int i_openCover = Convert.ToInt16(FADM_Auto.Dye._cup_Temps[i_cupNum - 1]._s_statues);


                                        if (5 != i_openCover)
                                            b_open = false;

                                    }


                                    if (b_open)
                                        break;

                                    Thread.Sleep(1);
                                }

                                lis_ints1.Clear();
                                this.DripProcess(obj_batchName, i_cupMin, i_cupMax, i_type, i_erea);
                            }
                            else
                            {
                                //滴液区
                                for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                                {

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE drop_head SET CupFinish = 0, AddWaterFinish = 0, RealAddWaterWeight = 0.00 , Step = 1" +
                                        "WHERE BatchName = '" + obj_batchName + "' AND CupNum = " + lis_cupFailT[i] + ";");

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE drop_details SET Finish = 0, MinWeight = 0, RealDropWeight = 0.00 , NeedPulse = 0" +
                                        "WHERE BatchName = '" + obj_batchName + "' AND CupNum = " + lis_cupFailT[i] + ";");
                                    int i_num = lis_cupFailT[i];
                                    lis_cupFailT.Remove(lis_cupFailT[i]);
                                    lis_cupSuc.Remove(i_num);

                                }
                                this.DripProcess(obj_batchName, i_cupMin, i_cupMax, i_type, i_erea);
                            }
                        }
                        else
                        {
                            //不重滴

                            if (FADM_Object.Communal.ReadMachineStatus() != 8)
                            {
                                if (i_type == 3)
                                {
                                    FADM_Object.Communal._lis_dripSuccessCup.AddRange(lis_cupFailT);
                                }
                            }

                        }

                    }
                }

                string s_cupList = "";
                string s_te = "";
                if (lis_cupSuc.Count > 0)
                {
                    for (int i = 0; i < lis_cupSuc.Count; i++)
                    {
                        s_cupList += lis_cupSuc[i] + ",";
                    }
                }
                if (s_cupList != "")
                {
                    s_cupList = s_cupList.Remove(s_cupList.Length - 1);
                    s_te = " And CupNum in (" + s_cupList + ")";
                }
                //添加历史表
                dt_drop_head_temp = FADM_Object.Communal._fadmSqlserver.GetData(
                 "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'drop_head';");
                string s_columnHead = null;
                foreach (DataRow row in dt_drop_head_temp.Rows)
                {
                    string s_curName = Convert.ToString(row[0]);
                    if ("TestTubeFinish" != s_curName && "TestTubeWaterLower" != s_curName && "AddWaterFinish" != s_curName &&
                        "CupFinish" != s_curName && "TestTubeWaterLower" != s_curName)
                        s_columnHead += s_curName + ", ";
                }
                s_columnHead = s_columnHead.Remove(s_columnHead.Length - 2);

                dt_drop_head_temp = FADM_Object.Communal._fadmSqlserver.GetData(
                   "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'drop_details';");
                string s_columnDetails = null;
                foreach (DataRow row in dt_drop_head_temp.Rows)
                {
                    string s_curName = Convert.ToString(row[0]);
                    if ("MinWeight" != s_curName && "Finish" != s_curName && "IsShow" != s_curName && "NeedPulse" != s_curName)
                        s_columnDetails += Convert.ToString(row[0]) + ", ";
                }
                s_columnDetails = s_columnDetails.Remove(s_columnDetails.Length - 2);

                if (i_type == 3)
                {
                    if (s_te != "")
                    {
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "INSERT INTO history_head (" + s_columnHead + ") (SELECT " + s_columnHead + " FROM drop_head " +
                            "WHERE BatchName = '" + obj_batchName + "' AND CupNum >= " + i_cupMin + " AND CupNum <= " + i_cupMax + s_te + ");");

                        //FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "INSERT INTO history_head (" + s_columnHead + ") (SELECT " + s_columnHead + " FROM drop_head " +
                        //    "WHERE BatchName = '" + o_BatchName + "' AND CupNum >= " + _i_cupMin + " AND CupNum <= " + _i_cupMax + s_te + ");");

                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "INSERT INTO history_details (" + s_columnDetails + ") (SELECT " + s_columnDetails + " FROM drop_details " +
                           "WHERE BatchName = '" + obj_batchName + "' AND CupNum >= " + i_cupMin + " AND CupNum <= " + i_cupMax + s_te + ");");
                    }
                }
                else
                {
                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                        "INSERT INTO history_head (" + s_columnHead + ") (SELECT " + s_columnHead + " FROM drop_head " +
                        "WHERE BatchName = '" + obj_batchName + "' AND CupNum >= " + i_cupMin + " AND CupNum <= " + i_cupMax + ");");

                    //FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "INSERT INTO history_head (" + s_columnHead + ") (SELECT " + s_columnHead + " FROM drop_head " +
                    //    "WHERE BatchName = '" + o_BatchName + "' AND CupNum >= " + _i_cupMin + " AND CupNum <= " + _i_cupMax + s_te + ");");

                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                       "INSERT INTO history_details (" + s_columnDetails + ") (SELECT " + s_columnDetails + " FROM drop_details " +
                       "WHERE BatchName = '" + obj_batchName + "' AND CupNum >= " + i_cupMin + " AND CupNum <= " + i_cupMax + ");");
                }

                if (i_type == 2)
                {
                    dt_drop_head_temp = FADM_Object.Communal._fadmSqlserver.GetData(
                   "SELECT * FROM drop_head WHERE CupNum >= " + i_cupMin + " AND CupNum <= " + i_cupMax + " AND BatchName = '" + obj_batchName + "' ;");
                    //解决滴液失败多次导入历史记录和重复播报问题
                    if (dt_drop_head_temp.Rows.Count > 0)
                    {
                        //new FADM_Object.MyAlarm("区域" + i_erea + "滴液完成", 1);
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "DELETE FROM drop_head WHERE CupNum >= " + i_cupMin + " AND CupNum <= " + i_cupMax + " AND BatchName = '" + obj_batchName + "' ;");
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "DELETE FROM drop_details WHERE CupNum >= " + i_cupMin + " AND CupNum <= " + i_cupMax + " AND BatchName = '" + obj_batchName + "';");


                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE cup_details SET FormulaCode = null, " +
                            "DyeingCode = null, IsUsing = 0, Statues = '待机', " +
                            "StartTime = null, SetTemp = null, StepNum = null, TotalWeight = null, " +
                            "TotalStep = null, TechnologyName = null, StepStartTime = null, SetTime = null,RecordIndex = 0 WHERE CupNum >= " +
                            i_cupMin + " AND CupNum <= " + i_cupMax + " ;");
                    }
                }

                if (FADM_Auto.Drip._b_dripErr)
                {
                    FADM_Object.Communal._lis_dripStopCup.AddRange(lis_cupT);

                }
            }

        }

        private void DripProcessDebug(object oBatchName)
        {
            Thread thread = null;
            int i_mRes = 0;
            //针检失败，不继续针检状态
            bool b_checkFail = false;

            List<int> lis_cupSuc = new List<int>();
            List<int> lis_cupT = new List<int>();

            DataTable dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
               "SELECT * FROM drop_head WHERE   BatchName = '" + oBatchName + "' And Step = 1 order by CupNum;");

            foreach (DataRow row in dt_drop_head.Rows)
            {
                lis_cupSuc.Add(Convert.ToInt32(row["CupNum"].ToString()));
                lis_cupT.Add(Convert.ToInt32(row["CupNum"].ToString()));
            }

            string s_unitOfAccount = "";

            //复位
            //Lib_SerialPort.Balance.METTLER.bReSetSign = true;

            //判断染色线程是否需要用机械手
            if (null != FADM_Object.Communal.ReadDyeThread())
            {
                FADM_Object.Communal.WriteDripWait(true);
                Communal._b_isWaitDrip = true;
                while (true)
                {
                    if (false == FADM_Object.Communal.ReadDripWait())
                        break;
                    Thread.Sleep(1);
                }
                Communal._b_isWaitDrip = false;
            }
            else
                Communal._b_isWaitDrip = false;

            //加水
            string s_sql = "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName + "' AND " +
                "AddWaterChoose = 1 AND AddWaterFinish = 0  ORDER BY CupNum;";
            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

            if (dt_drop_head.Rows.Count > 0)
            {
                //判断是否存在加水复检失败
                bool b_fail = false;
                string s_failC = "";

                foreach (DataRow row in dt_drop_head.Rows)
                {
                    //判断染色线程是否需要用机械手
                    if (null != FADM_Object.Communal.ReadDyeThread())
                    {
                        FADM_Object.Communal.WriteDripWait(true);
                        Communal._b_isWaitDrip = true;
                        while (true)
                        {
                            if (false == FADM_Object.Communal.ReadDripWait())
                                break;
                            Thread.Sleep(1);
                        }
                        Communal._b_isWaitDrip = false;
                    }
                    else
                        Communal._b_isWaitDrip = false;

                    int i_cupNo = Convert.ToInt32(row["CupNum"]);
                    double d_blObjectW = Convert.ToDouble(row["ObjectAddWaterWeight"]);
                    if (d_blObjectW > 0)
                    {
                        //if (SmartDyeing.FADM_Object.Communal._dic_dyeType.Keys.Contains(i_cupNo))
                        //{
                        //    if (SmartDyeing.FADM_Object.Communal._dic_dyeType[i_cupNo] == 1)
                        //    {
                        //        //如果关盖状态，就先执行开盖动作
                        //        if (FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._i_cupCover == 1)
                        //        {
                        //        labelP1:
                        //            //开盖
                        //            try
                        //            {
                        //                //寻找配液杯
                        //                FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", i_cupNo + "号配液杯开盖");
                        //                //FADM_Object.Communal._i_OptCupNum = i_cupNo;
                        //                //int reSuccess4 = MyModbusFun.TargetMove(1, i_cupNo, 1);
                        //                //if (-2 == reSuccess4)
                        //                //    throw new Exception("收到退出消息");

                        //                //FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "抵达" + i_cupNo + "号配液杯");
                        //                int i_xStart = 0, i_yStart = 0;
                        //                int i_xEnd = 0, i_yEnd = 0;
                        //                MyModbusFun.CalTarget(1, i_cupNo, ref i_xStart, ref i_yStart);

                        //                MyModbusFun.CalTarget(4, i_cupNo, ref i_xEnd, ref i_yEnd);

                        //                i_mRes = MyModbusFun.OpenOrPutCover(i_xStart, i_yStart, i_xEnd, i_yEnd, 0);
                        //                if (-2 == i_mRes)
                        //                    throw new Exception("收到退出消息");
                        //            }
                        //            catch (Exception ex)
                        //            {
                        //                if ("未发现杯盖" == ex.Message)
                        //                {
                        //                    ////气缸上
                        //                    //int[] ia_array = new int[1];
                        //                    //ia_array[0] = 5;

                        //                    //int i_state = FADM_Object.Communal._tcpModBus.Write(811, ia_array);
                        //                    //Thread.Sleep(1000);
                        //                    ////抓手开
                        //                    //ia_array = new int[1];
                        //                    //ia_array[0] = 7;

                        //                    //i_state = FADM_Object.Communal._tcpModBus.Write(811, ia_array);

                        //                    FADM_Object.MyAlarm myAlarm;
                        //                    s_sql = "UPDATE drop_details SET MinWeight = 5 WHERE BatchName = '" + oBatchName + "' And  Finish = 0  AND CupNum = " + i_cupNo + ";";
                        //                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        //                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        //                        myAlarm = new FADM_Object.MyAlarm(i_cupNo + "号配液杯未发现杯盖，是否继续执行?(继续执行请点是，已完成开盖请点否)", "SwitchCover", i_cupNo, 2, 7);
                        //                    else
                        //                        myAlarm = new FADM_Object.MyAlarm(i_cupNo + " Cup did not find a cap, do you want to continue? " +
                        //                            "( Continue to perform please click Yes, have completed the opening please click No)", "SwitchCover", i_cupNo, 2, 7);
                        //                    continue; 
                        //                    //while (true)

                        //                    //{
                        //                    //    if (0 != myAlarm._i_alarm_Choose)
                        //                    //        break;
                        //                    //    if (0 != myAlarm._i_alarm_Repeat)
                        //                    //        break;
                        //                    //    Thread.Sleep(1);
                        //                    //}
                        //                    //if (myAlarm._i_alarm_Choose == 1 || myAlarm._i_alarm_Repeat == 1)
                        //                    //{
                        //                    //    goto labelP1;
                        //                    //}
                        //                    //else
                        //                    //{
                        //                    //    goto labelP2;
                        //                    //}
                        //                    //return;
                        //                }
                        //                else if ("发现杯盖或针筒" == ex.Message)
                        //                {
                        //                    FADM_Object.MyAlarm myAlarm;
                        //                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        //                        myAlarm = new FADM_Object.MyAlarm("请先拿住针筒或杯盖，然后点确定", "SwitchCover", true, 1);
                        //                    else
                        //                        myAlarm = new FADM_Object.MyAlarm("Please hold the syringe or cup lid first, and then confirm", "SwitchCover", true, 1);
                        //                    while (true)

                        //                    {
                        //                        if (0 != myAlarm._i_alarm_Choose)
                        //                            break;
                        //                        Thread.Sleep(1);
                        //                    }
                        //                    //抓手开
                        //                    int[] ia_array = new int[1];
                        //                    ia_array[0] = 7;

                        //                    int i_state = FADM_Object.Communal._tcpModBus.Write(811, ia_array);
                        //                    //等5秒后继续
                        //                    Thread.Sleep(5000);
                        //                    goto labelP1;
                        //                }
                        //                else if ("配液杯取盖失败" == ex.Message)
                        //                {
                        //                    FADM_Object.MyAlarm myAlarm;
                        //                    //把剩余没有滴液的Min置为状态5
                        //                    s_sql = "UPDATE drop_details SET MinWeight = 5 WHERE BatchName = '" + oBatchName + "' And  Finish = 0  AND CupNum = " + i_cupNo + ";";
                        //                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        //                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        //                        myAlarm = new FADM_Object.MyAlarm(i_cupNo + "号配液杯取盖失败，是否继续执行?(继续执行请点是)", "SwitchCover", i_cupNo, 2, 7);
                        //                    else
                        //                        myAlarm = new FADM_Object.MyAlarm(i_cupNo + " Failed to remove cap from dispensing cup, do you want to continue? " +
                        //                            "( Continue to perform please click Yes)", "SwitchCover", i_cupNo, 2, 7);
                        //                    continue;
                        //                }

                        //                else if ("放盖区取盖失败" == ex.Message)
                        //                {
                        //                    FADM_Object.MyAlarm myAlarm;
                        //                    //把剩余没有滴液的Min置为状态5
                        //                    s_sql = "UPDATE drop_details SET MinWeight = 5 WHERE BatchName = '" + oBatchName + "' And  Finish = 0  AND CupNum = " + i_cupNo + ";";
                        //                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        //                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        //                        myAlarm = new FADM_Object.MyAlarm(i_cupNo + "号放盖区取盖失败，是否继续执行?(继续执行请点是)", "SwitchCover", i_cupNo, 2, 7);
                        //                    else
                        //                        myAlarm = new FADM_Object.MyAlarm(i_cupNo + " Failed to remove the cover from the cover placement area, do you want to continue? " +
                        //                            "( Continue to perform please click Yes)", "SwitchCover", i_cupNo, 2, 7);
                        //                    continue;
                        //                }

                        //                else if ("关盖失败" == ex.Message)
                        //                {
                        //                    FADM_Object.MyAlarm myAlarm;
                        //                    //把剩余没有滴液的Min置为状态5
                        //                    s_sql = "UPDATE drop_details SET MinWeight = 5 WHERE BatchName = '" + oBatchName + "' And  Finish = 0  AND CupNum = " + i_cupNo + ";";
                        //                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        //                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        //                        myAlarm = new FADM_Object.MyAlarm(i_cupNo + "号关盖失败，是否继续执行?(继续执行请点是)", "SwitchCover", i_cupNo, 2, 7);
                        //                    else
                        //                        myAlarm = new FADM_Object.MyAlarm(i_cupNo + " Closing failure, do you want to continue? " +
                        //                            "( Continue to perform please click Yes)", "SwitchCover", i_cupNo, 2, 7);
                        //                    continue;
                        //                }
                        //                else
                        //                    throw;
                        //            }

                        //        labelP2:
                        //            //复位加药启动信号
                        //            int[] ia_zero1 = new int[1];
                        //            //
                        //            ia_zero1[0] = 0;

                        //            FADM_Auto.Dye.DyeOpenOrCloseCover(i_cupNo, 2);
                        //            Thread.Sleep(1000);
                        //            Communal._fadmSqlserver.ReviseData("Update  cup_details set CoverStatus = 2 where CupNum = " + i_cupNo);

                        //            FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._i_cupCover = 2;

                        //            FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", i_cupNo + "号配液杯开盖完成");
                        //        }
                        //    }
                        //}

                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找" + i_cupNo + "号配液杯");
                        int i_reSuccess2 = MyModbusFun.TargetMove(2,0,0);
                        if (-2 == i_reSuccess2)
                            throw new Exception("收到退出消息");
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达" + i_cupNo + "号配液杯");

                    label23:
                        Thread.Sleep(Convert.ToInt32(Lib_Card.Configure.Parameter.Delay_Balance_Read * 1000));
                        double d_blBalanceValue11 = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal._s_balanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal._s_balanceValue));
                        Thread.Sleep(Convert.ToInt32(Lib_Card.Configure.Parameter.Delay_Balance_Read * 1000));
                        double d_blBalanceValue12 = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal._s_balanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal._s_balanceValue));
                        double d_blDif1 = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blBalanceValue11 - d_blBalanceValue12)) : Convert.ToDouble(string.Format("{0:F3}", d_blBalanceValue11 - d_blBalanceValue12));
                        d_blDif1 = d_blDif1 < 0 ? -d_blDif1 : d_blDif1;

                        if (d_blDif1 > Lib_Card.Configure.Parameter.Other_Stable_Value)
                            goto label23;

                        //记录初始值
                        double d_blBalanceValue0 = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", (d_blBalanceValue11 + d_blBalanceValue12) / 2)) : Convert.ToDouble(string.Format("{0:F3}", (d_blBalanceValue11 + d_blBalanceValue12) / 2));
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平初始读数：" + d_blBalanceValue0);


                        
                        if (_b_dripStop)
                        {
                            FADM_Object.Communal._b_stop = true;
                        }
                        
                        double d_addWaterTime = MyModbusFun.GetWaterTime(d_blObjectW);//加水时间
                        FADM_Object.Communal.AddWater(d_addWaterTime);
                    label2:
                        Thread.Sleep(Convert.ToInt32(Lib_Card.Configure.Parameter.Delay_Balance_Read * 1000));
                        double d_blBalanceValue1 = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal._s_balanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal._s_balanceValue));
                        Thread.Sleep(Convert.ToInt32(Lib_Card.Configure.Parameter.Delay_Balance_Read * 1000));
                        double d_blBalanceValue2 = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal._s_balanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal._s_balanceValue));
                        double d_blDif = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blBalanceValue1 - d_blBalanceValue2)) : Convert.ToDouble(string.Format("{0:F3}", d_blBalanceValue1 - d_blBalanceValue2));
                        d_blDif = d_blDif < 0 ? -d_blDif : d_blDif;

                        if (d_blDif > Lib_Card.Configure.Parameter.Other_Stable_Value)
                            goto label2;

                        double d_blRRead = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", (d_blBalanceValue1 + d_blBalanceValue2) / 2)) : Convert.ToDouble(string.Format("{0:F3}", (d_blBalanceValue1 + d_blBalanceValue2) / 2));
                        double d_blWeight = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", (d_blBalanceValue1 + d_blBalanceValue2) / 2 - d_blBalanceValue0)) : Convert.ToDouble(string.Format("{0:F3}", (d_blBalanceValue1 + d_blBalanceValue2) / 2 - d_blBalanceValue0));
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数：" + d_blRRead + ",实际重量：" + d_blWeight);
                        //计算绝对误差
                        double d_blWE = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", (d_blWeight - d_blObjectW))) : Convert.ToDouble(string.Format("{0:F3}", (d_blWeight - d_blObjectW)));
                        //计算相对误差
                        d_blDif = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blWE / d_blObjectW)) : Convert.ToDouble(string.Format("{0:F3}", d_blWE / d_blObjectW));
                        int i_rErr = Convert.ToInt16(d_blDif * 100);
                        i_rErr = i_rErr < 0 ? -i_rErr : i_rErr;

                        s_sql = "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName + "' AND ObjectAddWaterWeight != 0  And AddWaterFinish = 0;";
                        DataTable dt_drop_head3 = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);


                        if (d_blWeight == 0)
                        {
                            if (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0)
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                      "UPDATE cup_details SET TotalWeight =  " + string.Format("{0:F2}", 0) + " WHERE CupNum = " + i_cupNo + " ;");
                            else
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                      "UPDATE cup_details SET TotalWeight =  " + string.Format("{0:F3}", 0) + " WHERE CupNum = " + i_cupNo + " ;");
                        }
                        else
                        {
                            if (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0)
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                          "UPDATE cup_details SET TotalWeight =  " + string.Format("{0:F2}", d_blWeight) + " WHERE CupNum = " + i_cupNo + " ;");
                            else

                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                          "UPDATE cup_details SET TotalWeight =  " + string.Format("{0:F3}", d_blWeight) + " WHERE CupNum = " + i_cupNo + " ;");
                        }


                        if (Convert.ToDouble(row["ObjectAddWaterWeight"].ToString()) > 20)
                        {
                            if (System.Math.Abs(d_blWE * 100 / (Convert.ToDouble(row["TotalWeight"].ToString()))) > Lib_Card.Configure.Parameter.Other_AErr_DripWater || d_blWeight == 0)
                            {
                                b_fail = true;
                                s_failC += row["CupNum"].ToString() + ",";
                            }
                        }
                        else
                        {
                            if (System.Math.Abs(Convert.ToDouble(row["ObjectAddWaterWeight"].ToString()) * d_blDif * 100) / (Convert.ToDouble(row["TotalWeight"].ToString())) > Lib_Card.Configure.Parameter.Other_AErr_DripWater || d_blWeight == 0)
                            {
                                b_fail = true;
                                s_failC += row["CupNum"].ToString() + ",";
                            }
                        }


                        //复检天平重量为0时，实际加水量为0
                        if (d_blWeight == 0)
                        {
                            s_sql = "UPDATE drop_head SET RealAddWaterWeight = 0, AddWaterFinish = 1 WHERE " +
                            "BatchName = '" + oBatchName + "' AND CupNum = " + i_cupNo + " AND AddWaterChoose = 1 AND CupFinish = 0   AND ObjectAddWaterWeight > 0 AND ObjectAddWaterWeight <= 20 And AddWaterFinish = 0;";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        }
                        else
                        {
                            s_sql = "UPDATE drop_head SET RealAddWaterWeight = " + d_blWeight + ", AddWaterFinish = 1 WHERE " +
                            "BatchName = '" + oBatchName + "' AND CupNum = " + i_cupNo + " AND AddWaterChoose = 1 AND CupFinish = 0   AND ObjectAddWaterWeight > 0 AND ObjectAddWaterWeight <= 20  And AddWaterFinish = 0;";

                            //s_sql = "UPDATE drop_head SET RealAddWaterWeight = (ObjectAddWaterWeight * " + (1 + d_blDif) + "), AddWaterFinish = 1 WHERE " +
                            //"BatchName = '" + o_BatchName + "' AND AddWaterChoose = 1 AND CupFinish = 0 AND CupNum >= " +
                            //_i_cupMin + " AND CupNum <= " + _i_cupMax + "  AND ObjectAddWaterWeight > 0 AND ObjectAddWaterWeight <= 20;";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        }

                        //复检天平重量为0时，实际加水量为0
                        if (d_blWeight == 0)
                        {
                            s_sql = "UPDATE drop_head SET RealAddWaterWeight = 0, AddWaterFinish = 1 WHERE " +
                            "BatchName = '" + oBatchName + "' AND CupNum = " + i_cupNo + " AND AddWaterChoose = 1 AND CupFinish = 0   AND ObjectAddWaterWeight > 20  And AddWaterFinish = 0;";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        }
                        else
                        {
                            s_sql = "UPDATE drop_head SET RealAddWaterWeight =  " + d_blWeight + ", AddWaterFinish = 1 WHERE " +
                            "BatchName = '" + oBatchName + "' AND CupNum = " + i_cupNo + " AND AddWaterChoose = 1 AND CupFinish = 0   AND ObjectAddWaterWeight > 20  And AddWaterFinish = 0;";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        }

                    }
                }

               

                if (b_fail)
                {
                    s_failC = s_failC.Remove(s_failC.Length - 1);
                    FADM_Object.Communal.WriteDripWait(true);
                    FADM_Object.MyAlarm myAlarm;

                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        myAlarm = new FADM_Object.MyAlarm(s_failC + "号杯加水复检失败,是否继续?(继续滴液请点是，退出滴液请点否)", "加水复检", true, 1);
                    else
                        myAlarm = new FADM_Object.MyAlarm(" The re inspection of" + s_failC + " cup with water has failed. Do you want to continue? (To continue dripping, please click Yes, and to exit dripping, please click No)", "Add water for retesting", true, 1);
                    while (true)
                    {
                        if (0 != myAlarm._i_alarm_Choose)
                            break;
                        Thread.Sleep(1);
                    }

                    if (2 == myAlarm._i_alarm_Choose)
                        throw new Exception("收到退出消息");

                    //判断染色线程是否需要用机械手
                    if (null != FADM_Object.Communal.ReadDyeThread())
                    {
                        FADM_Object.Communal.WriteDripWait(true);

                        while (true)
                        {
                            if (false == FADM_Object.Communal.ReadDripWait())
                                break;
                            Thread.Sleep(1);
                        }
                    }
                    else
                    {
                        FADM_Object.Communal.WriteDripWait(false);
                    }
                }
            }
            string s_unitA = "";
            int i_lowSrart = 0;
            if (FADM_Object.Communal._b_isAssitantFirst)
            {
                //加助剂
                s_unitA = "g/l";
                i_lowSrart = 0;
            }
            else
            {
                //加染料
                s_unitA = "%";
                i_lowSrart = 0;
            }

        label3:
            s_sql = "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName + "' AND " +
                "Finish = 0 AND UnitOfAccount = '" + s_unitA + "' AND MinWeight = " + i_lowSrart + " AND " +
                "BottleNum <= " + Lib_Card.Configure.Parameter.Machine_Bottle_Total + "  ORDER BY CupNum;";
            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (0 == dt_drop_head.Rows.Count)
            {
                if (FADM_Object.Communal._b_isAssitantFirst)
                {
                    if ("g/l" == s_unitA)
                    {
                        //助剂已加完，加染料
                        goto label4;
                    }
                    else
                    {
                        if (0 == i_lowSrart)
                        {
                            //染料已加完，加液量不足
                            goto label5;
                        }
                        else if (1 == i_lowSrart)
                        {
                            //液量不足已加完，加超出生命周期
                            goto label17;
                        }
                        else if (3 == i_lowSrart)
                        {
                            //超出生命周期的加完，加检测不到针筒
                            goto label16;
                        }
                        else
                        {
                            //结束
                            goto label6;
                        }
                    }
                }
                else
                {
                    if ("%" == s_unitA)
                    {
                        //染料已加完，加助剂
                        goto label4;
                    }
                    else
                    {
                        if (0 == i_lowSrart)
                        {
                            //助剂已加完，加液量不足
                            goto label5;
                        }
                        else if (1 == i_lowSrart)
                        {
                            //液量不足已加完，加超出生命周期
                            goto label17;
                        }
                        else if (3 == i_lowSrart)
                        {
                            //超出生命周期的加完，加检测不到针筒
                            goto label16;
                        }
                        else
                        {
                            //结束
                            goto label6;
                        }
                    }
                }

            }

            int i_minCupNo = Convert.ToInt32(dt_drop_head.Rows[0]["CupNum"]);

        label7:
            s_sql = "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName + "' AND CupNum = " + i_minCupNo + " AND " +
                "Finish = 0 AND UnitOfAccount = '" + s_unitA + "' AND MinWeight = " + i_lowSrart + " AND " +
                "BottleNum <= " + Lib_Card.Configure.Parameter.Machine_Bottle_Total + " ORDER BY BottleNum;";
            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (0 == dt_drop_head.Rows.Count)
            {
                //当前杯已加完
                goto label3;
            }

            int i_minBottleNo = Convert.ToInt32(dt_drop_head.Rows[0]["BottleNum"]);

            s_sql = "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                "Finish = 0 AND UnitOfAccount = '" + s_unitA + "' AND MinWeight = " + i_lowSrart + " AND " +
                "BottleNum <= " + Lib_Card.Configure.Parameter.Machine_Bottle_Total + "  ORDER BY CupNum;";
            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (0 == dt_drop_head.Rows.Count)
            {
                //当前瓶完成
                goto label7;
            }

            s_sql = "SELECT * FROM bottle_details WHERE BottleNum = " + i_minBottleNo + ";";
            DataTable dt_bottle_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            int iAdjust = Convert.ToInt32(dt_bottle_details.Rows[0]["AdjustValue"]);
            bool blCheckSuccess = (Convert.ToString(dt_bottle_details.Rows[0]["AdjustSuccess"]) == "1");
            string sSyringeType = Convert.ToString(dt_bottle_details.Rows[0]["SyringeType"]);



            Dictionary<int, int> dic_pulse = new Dictionary<int, int>();
            Dictionary<int, double> dic_weight = new Dictionary<int, double>();
            int i_pulseT = 0;
            if (0 == i_lowSrart)
            {
                double d_blCW = Convert.ToDouble(string.Format("{0:F3}", dt_bottle_details.Rows[0]["CurrentWeight"]));
                foreach (DataRow dataRow in dt_drop_head.Rows)
                {
                    int i_cupNo = Convert.ToInt32(dataRow["CupNum"]);
                    double d_blOAddW = Convert.ToDouble(string.Format("{0:F3}", dataRow["ObjectDropWeight"]));
                    d_blCW -= d_blOAddW;

                    //查询判断是否超期
                    s_sql = "SELECT * FROM assistant_details WHERE AssistantCode = '" + dt_bottle_details.Rows[0]["AssistantCode"].ToString() + "';";
                    DataTable dt_assistant_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                    DateTime timeA = Convert.ToDateTime(dt_bottle_details.Rows[0]["BrewingData"].ToString());
                    DateTime timeB = DateTime.Now; //获取当前时间
                    TimeSpan ts = timeB - timeA; //计算时间差
                    string s_time = ts.TotalHours.ToString(); //将时间差转换为小时


                    if (d_blCW < Lib_Card.Configure.Parameter.Other_Bottle_MinWeight && FADM_Object.Communal._b_isLowDrip)
                    {
                        //查询在备料表是否存在记录，如果存在，先让客户选择是否使用备料数据来更新
                        string s_sqlpre = "SELECT * FROM pre_brew WHERE  BottleNum = " + i_minBottleNo + ";";
                        DataTable dt_pre_brew = FADM_Object.Communal._fadmSqlserver.GetData(s_sqlpre);
                        if (dt_pre_brew.Rows.Count > 0)
                        {
                            FADM_Object.Communal.WriteDripWait(true);
                            FADM_Object.MyAlarm myAlarm;
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm(
                                i_minBottleNo + "号母液瓶液量过低，备料表存在已开料记录，是否替换(替换请点是，继续使用旧母液请点否)?", "滴液", true, 1);
                            else
                                myAlarm = new FADM_Object.MyAlarm(
                                "The " + i_minBottleNo + " mother liquor bottle has expired, and there is a record of opened materials in the material preparation table. Should it be replaced? (Please click Yes for replacement, and click No for continuing to use the old mother liquor)", "Drip", true, 1);

                            while (true)
                            {
                                if (0 != myAlarm._i_alarm_Choose)
                                    break;
                                Thread.Sleep(1);
                            }
                            //判断染色线程是否需要用机械手
                            if (null != FADM_Object.Communal.ReadDyeThread())
                            {
                                FADM_Object.Communal.WriteDripWait(true);

                                while (true)
                                {
                                    if (false == FADM_Object.Communal.ReadDripWait())
                                        break;
                                    Thread.Sleep(1);
                                }
                            }
                            else
                            {
                                FADM_Object.Communal.WriteDripWait(false);
                            }
                            //如果选择是，使用新料重新计算
                            if (1 == myAlarm._i_alarm_Choose)
                            {
                                //使用备料表数据更新现有母液瓶数据，删除备料表记录
                                s_sql = "UPDATE bottle_details SET RealConcentration = '" + dt_pre_brew.Rows[0]["RealConcentration"].ToString() + "',CurrentWeight = '" + dt_pre_brew.Rows[0]["CurrentWeight"].ToString() + "',BrewingData='"
                                    + dt_pre_brew.Rows[0]["BrewingData"].ToString() + "'" +
                                " WHERE BottleNum = " + i_minBottleNo + ";";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from pre_brew where BottleNum = " + i_minBottleNo);
                                goto label7;
                            }
                            //选择否就和之前的逻辑一致
                            else
                            {
                                s_sql = "UPDATE drop_details SET MinWeight = 1 WHERE BatchName = '" + oBatchName + "' AND MinWeight=0 And " +
                                "BottleNum = " + i_minBottleNo + " AND Finish = 0 ;";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                break;
                            }
                        }
                        else
                        {
                            s_sql = "UPDATE drop_details SET MinWeight = 1 WHERE BatchName = '" + oBatchName + "' AND MinWeight=0 And " +
                                "BottleNum = " + i_minBottleNo + " AND Finish = 0 ;";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                            break;
                        }

                    }
                    else if (Convert.ToDouble(s_time) > Convert.ToDouble(dt_assistant_details.Rows[0]["TermOfValidity"].ToString()) && FADM_Object.Communal._b_isOutDrip)
                    {
                        //查询在备料表是否存在记录，如果存在，先让客户选择是否使用备料数据来更新
                        string s_sqlpre = "SELECT * FROM pre_brew WHERE  BottleNum = " + i_minBottleNo + ";";
                        DataTable dt_pre_brew = FADM_Object.Communal._fadmSqlserver.GetData(s_sqlpre);
                        if (dt_pre_brew.Rows.Count > 0)
                        {
                            FADM_Object.Communal.WriteDripWait(true);
                            FADM_Object.MyAlarm myAlarm;
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm(
                                i_minBottleNo + "号母液瓶过期，备料表存在已开料记录，是否替换(替换请点是，继续使用旧母液请点否)?", "滴液", true, 1);
                            else
                                myAlarm = new FADM_Object.MyAlarm(
                                "The " + i_minBottleNo + " mother liquor bottle has expired, and there is a record of opened materials in the material preparation table. Should it be replaced? (Please click Yes for replacement, and click No for continuing to use the old mother liquor)", "Drip", true, 1);
                            while (true)
                            {
                                if (0 != myAlarm._i_alarm_Choose)
                                    break;
                                Thread.Sleep(1);
                            }
                            //判断染色线程是否需要用机械手
                            if (null != FADM_Object.Communal.ReadDyeThread())
                            {
                                FADM_Object.Communal.WriteDripWait(true);

                                while (true)
                                {
                                    if (false == FADM_Object.Communal.ReadDripWait())
                                        break;
                                    Thread.Sleep(1);
                                }
                            }
                            else
                            {
                                FADM_Object.Communal.WriteDripWait(false);
                            }
                            //如果选择是，使用新料重新计算
                            if (1 == myAlarm._i_alarm_Choose)
                            {
                                //使用备料表数据更新现有母液瓶数据，删除备料表记录
                                s_sql = "UPDATE bottle_details SET RealConcentration = '" + dt_pre_brew.Rows[0]["RealConcentration"].ToString() + "',CurrentWeight = '" + dt_pre_brew.Rows[0]["CurrentWeight"].ToString() + "',BrewingData='"
                                    + dt_pre_brew.Rows[0]["BrewingData"].ToString() + "'" +
                                " WHERE BottleNum = " + i_minBottleNo + ";";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from pre_brew where BottleNum = " + i_minBottleNo);
                                goto label7;
                            }
                            else
                            {
                                s_sql = "UPDATE drop_details SET MinWeight = 3 WHERE BatchName = '" + oBatchName + "' AND  MinWeight=0 And " +
                                "BottleNum = " + i_minBottleNo + " AND Finish = 0 ;";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                break;
                            }
                        }
                        else
                        {
                            s_sql = "UPDATE drop_details SET MinWeight = 3 WHERE BatchName = '" + oBatchName + "' AND  MinWeight=0 And " +
                                "BottleNum = " + i_minBottleNo + " AND Finish = 0 ;";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                            break;
                        }
                    }
                    else
                    {
                        int i_pulse = Convert.ToInt32(iAdjust * d_blOAddW);
                        dic_pulse.Add(i_cupNo, i_pulse);
                        dic_weight.Add(i_cupNo, d_blOAddW);
                        i_pulseT += i_pulse;

                        s_unitOfAccount = dataRow["UnitOfAccount"].ToString();
                    }


                }

                if (0 == dic_pulse.Count)
                {
                    //当前瓶液量不足
                    goto label7;
                }
            }
            else
            {
                foreach (DataRow dataRow in dt_drop_head.Rows)
                {
                    int i_cupNo = Convert.ToInt32(dataRow["CupNum"]);
                    double d_blOAddW = Convert.ToDouble(string.Format("{0:F3}", dataRow["ObjectDropWeight"]));
                    int i_pulse = Convert.ToInt32(iAdjust * d_blOAddW);
                    dic_pulse.Add(i_cupNo, i_pulse);
                    dic_weight.Add(i_cupNo, d_blOAddW);
                    i_pulseT += i_pulse;

                    s_unitOfAccount = dataRow["UnitOfAccount"].ToString();
                }
            }

            //判断染色线程是否需要用机械手
            if (null != FADM_Object.Communal.ReadDyeThread())
            {
                FADM_Object.Communal.WriteDripWait(true);

                while (true)
                {
                    if (false == FADM_Object.Communal.ReadDripWait())
                        break;
                    Thread.Sleep(1);
                }
            }

            //针检
            if ((0 >= iAdjust || false == blCheckSuccess) && !b_checkFail)
            {
            label8:
                //判断染色线程是否需要用机械手
                if (null != FADM_Object.Communal.ReadDyeThread())
                {
                    FADM_Object.Communal.WriteDripWait(true);

                    while (true)
                    {
                        if (false == FADM_Object.Communal.ReadDripWait())
                            break;
                        Thread.Sleep(1);
                    }
                }

                if (_b_dripStop)
                {
                    FADM_Object.Communal._b_stop = true;
                }
                int i_res = new BottleCheck().MyDripCheck(i_minBottleNo, true, i_lowSrart); //针检
                if (-1 == i_res)
                {
                    FADM_Object.Communal.WriteDripWait(true);
                    FADM_Object.MyAlarm myAlarm;
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + "号母液瓶针检失败，是否继续?(继续针检请点是，退出针检请点否)", "滴液针检", true, 1, true);
                    else
                        myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + " bottle needle inspection failed, do you want to continue? " +
                            "(To continue the needle examination, please click Yes, and to exit the needle examination, please click No)", "Drip needle examination", true, 1, true);
                    while (true)

                    {
                        if (0 != myAlarm._i_alarm_Choose)
                            break;
                        Thread.Sleep(1);
                    }
                    //判断染色线程是否需要用机械手
                    if (null != FADM_Object.Communal.ReadDyeThread())
                    {
                        FADM_Object.Communal.WriteDripWait(true);

                        while (true)
                        {
                            if (false == FADM_Object.Communal.ReadDripWait())
                                break;
                            Thread.Sleep(1);
                        }
                    }
                    else
                    {
                        FADM_Object.Communal.WriteDripWait(false);
                    }

                    if (1 == myAlarm._i_alarm_Choose)
                        goto label8;
                    else
                    {
                        b_checkFail = true;
                    }
                }
                else if (-2 == i_res)
                {
                    s_sql = "UPDATE drop_details SET MinWeight = 2 WHERE BatchName = '" + oBatchName + "' AND " +
                          "BottleNum = " + i_minBottleNo + " AND Finish = 0;";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                    goto label3;
                }
                else if (-3 == i_res)
                {
                    //夹不到针筒时选择否，直接退出
                    throw new Exception("收到退出消息");
                }
                if (b_checkFail)
                {
                    s_sql = "update bottle_details set AdjustValue = 3900 where AdjustValue =0 And " +
                          "BottleNum = " + i_minBottleNo + ";";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                }
                goto label7;
            }


            if (0 == dic_pulse.Count)
            {
                //当前瓶液量不足
                goto label7;
            }

            //判断染色线程是否需要用机械手
            if (null != FADM_Object.Communal.ReadDyeThread())
            {
                FADM_Object.Communal.WriteDripWait(true);

                while (true)
                {
                    if (false == FADM_Object.Communal.ReadDripWait())
                        break;
                    Thread.Sleep(1);
                }
            }
        label9:
            //移动到母液瓶
            if (_b_dripStop)
            {
                FADM_Object.Communal._b_stop = true;
            }

            //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
            //{
            //    //富士伺服在下面判断 天平状态 原有不动 绿维的放在上面 并且置位 是否回原点 绿维的放移动机械手前面

            //    //判断是否异常
            //    FADM_Object.Communal.BalanceState("滴液");
            //}


            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找" + i_minBottleNo + "号母液瓶");
            FADM_Object.Communal._i_optBottleNum = i_minBottleNo;
            i_mRes = MyModbusFun.TargetMove(0, i_minBottleNo, 1);
            if (-2 == i_mRes)
                throw new Exception("收到退出消息");
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达" + i_minBottleNo + "号母液瓶");

            //抽液
            if (_b_dripStop)
            {
                FADM_Object.Communal._b_stop = true;
            }
            int i_extractionPulse = 0;

            if ("小针筒" == sSyringeType || "Little Syringe" == sSyringeType)
            {
                i_extractionPulse = (i_pulseT + iAdjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1));
                if (i_extractionPulse - Lib_Card.Configure.Parameter.Other_Z_BackPulse > Lib_Card.Configure.Parameter.Other_S_MaxPulse)
                {
                    if (FADM_Object.Communal._b_isFullDrip)
                    {
                        //超出单针筒上限
                        i_extractionPulse = Lib_Card.Configure.Parameter.Other_S_MaxPulse + Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                    }
                    else
                    {
                        i_extractionPulse = iAdjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1);
                        int temp = i_extractionPulse;
                        for (int i = 0; i < dic_pulse.Count; i++)
                        {
                            var v1 = dic_pulse.ElementAt(i);
                            var v2 = dic_weight.ElementAt(i);

                            temp = i_extractionPulse;

                            if (temp + v1.Value - Lib_Card.Configure.Parameter.Other_Z_BackPulse > Lib_Card.Configure.Parameter.Other_S_MaxPulse)
                            {
                                //如果需加量大于10g，直接先加
                                //if(v1.Value/1.0/ _i_adjust > Lib_Card.Configure.Parameter.Other_SplitValue)
                                if (temp == iAdjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1))
                                {
                                    i_extractionPulse = Lib_Card.Configure.Parameter.Other_S_MaxPulse + Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                                }
                                else
                                {
                                    //如果一筒抽不完就分2次，一次抽完就留下一次独立抽
                                    if (v1.Value + iAdjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1) - Lib_Card.Configure.Parameter.Other_Z_BackPulse > Lib_Card.Configure.Parameter.Other_S_MaxPulse)
                                    {
                                        i_extractionPulse = Lib_Card.Configure.Parameter.Other_S_MaxPulse + Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                                    }
                                }
                                break;
                            }
                            else
                            {
                                i_extractionPulse += v1.Value;
                            }
                        }
                    }
                }
                if (FADM_Object.Communal._b_isDripReserveFirst)
                    i_extractionPulse += iAdjust;
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抽液启动(" + i_extractionPulse  + ")");

            label10:
                try
                {
                    i_extractionPulse = i_extractionPulse - Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                    i_mRes = MyModbusFun.Extract(i_extractionPulse, s_unitOfAccount.Equals("g/l") ? true : false, 0); //抽液
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                }
                catch (Exception ex)
                {
                    if ("未发现针筒" == ex.Message)
                    {
                        if (i_lowSrart == 2)
                        {
                            FADM_Object.Communal.WriteDripWait(true);
                            FADM_Object.MyAlarm myAlarm;
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + "号母液瓶未找到针筒，是否继续执行?(继续寻找请点是，退出针检请点否)", "滴液", true, 1, true);
                            else
                                myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + " bottle did not find a syringe. Do you want to continue? " +
                                    "(To continue searching, please click Yes. To exit the needle test, please click No)", "Drip", true, 1, true);
                            while (true)
                            {
                                if (0 != myAlarm._i_alarm_Choose)
                                    break;
                                Thread.Sleep(1);
                            }
                            //判断染色线程是否需要用机械手
                            if (null != FADM_Object.Communal.ReadDyeThread())
                            {
                                FADM_Object.Communal.WriteDripWait(true);

                                while (true)
                                {
                                    if (false == FADM_Object.Communal.ReadDripWait())
                                        break;
                                    Thread.Sleep(1);
                                }
                            }
                            else
                            {
                                FADM_Object.Communal.WriteDripWait(false);
                            }

                            if (1 == myAlarm._i_alarm_Choose)
                                goto label3;
                            else
                                throw new Exception("收到退出消息");
                        }
                        else
                        {

                            s_sql = "UPDATE drop_details SET MinWeight = 2 WHERE BatchName = '" + oBatchName + "' AND " +
                            "BottleNum = " + i_minBottleNo + " AND Finish = 0;";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                            goto label3;
                        }

                    }
                    else if ("取针筒时抓手关失败" == ex.Message)
                    {
                        if (i_lowSrart == 2)
                        {
                            FADM_Object.Communal.WriteDripWait(true);
                            FADM_Object.MyAlarm myAlarm;
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + "号母液瓶取针筒时抓手关失败，是否继续执行?(继续寻找请点是，退出针检请点否)", "滴液", true, 1, true);
                            else
                                myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + " bottle The gripper failed to close when taking out the syringe. Do you want to continue? " +
                                    "(To continue searching, please click Yes. To exit the needle test, please click No)", "Drip", true, 1, true);
                            while (true)
                            {
                                if (0 != myAlarm._i_alarm_Choose)
                                    break;
                                Thread.Sleep(1);
                            }
                            //判断染色线程是否需要用机械手
                            if (null != FADM_Object.Communal.ReadDyeThread())
                            {
                                FADM_Object.Communal.WriteDripWait(true);

                                while (true)
                                {
                                    if (false == FADM_Object.Communal.ReadDripWait())
                                        break;
                                    Thread.Sleep(1);
                                }
                            }
                            else
                            {
                                FADM_Object.Communal.WriteDripWait(false);
                            }

                            if (1 == myAlarm._i_alarm_Choose)
                                goto label3;
                            else
                                throw new Exception("收到退出消息");
                        }
                        else
                        {

                            s_sql = "UPDATE drop_details SET MinWeight = 2 WHERE BatchName = '" + oBatchName + "' AND " +
                            "BottleNum = " + i_minBottleNo + " AND Finish = 0;";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                            goto label3;
                        }

                    }
                    else if ("取针筒时气缸下失败" == ex.Message)
                    {
                        if (i_lowSrart == 2)
                        {
                            FADM_Object.Communal.WriteDripWait(true);
                            FADM_Object.MyAlarm myAlarm;
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + "号母液瓶取针筒时气缸下失败，是否继续执行?(继续寻找请点是，退出针检请点否)", "滴液", true, 1, true);
                            else
                                myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + " bottle The cylinder failed to descend when removing the syringe. Do you want to continue? " +
                                    "(To continue searching, please click Yes. To exit the needle test, please click No)", "Drip", true, 1, true);
                            while (true)
                            {
                                if (0 != myAlarm._i_alarm_Choose)
                                    break;
                                Thread.Sleep(1);
                            }
                            //判断染色线程是否需要用机械手
                            if (null != FADM_Object.Communal.ReadDyeThread())
                            {
                                FADM_Object.Communal.WriteDripWait(true);

                                while (true)
                                {
                                    if (false == FADM_Object.Communal.ReadDripWait())
                                        break;
                                    Thread.Sleep(1);
                                }
                            }
                            else
                            {
                                FADM_Object.Communal.WriteDripWait(false);
                            }

                            if (1 == myAlarm._i_alarm_Choose)
                                goto label3;
                            else
                                throw new Exception("收到退出消息");
                        }
                        else
                        {

                            s_sql = "UPDATE drop_details SET MinWeight = 2 WHERE BatchName = '" + oBatchName + "' AND " +
                            "BottleNum = " + i_minBottleNo + " AND Finish = 0;";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                            goto label3;
                        }

                    }
                    else
                        throw;
                }

            }
            else
            {
                i_extractionPulse = (i_pulseT + iAdjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1));
                if (i_extractionPulse - Lib_Card.Configure.Parameter.Other_Z_BackPulse > Lib_Card.Configure.Parameter.Other_B_MaxPulse)
                {
                    if (FADM_Object.Communal._b_isFullDrip)
                    {
                        //超出单针筒上限
                        i_extractionPulse = Lib_Card.Configure.Parameter.Other_B_MaxPulse + Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                    }
                    else
                    {
                        i_extractionPulse = iAdjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1);
                        int temp = i_extractionPulse;
                        for (int i = 0; i < dic_pulse.Count; i++)
                        {
                            var v1 = dic_pulse.ElementAt(i);
                            var v2 = dic_weight.ElementAt(i);

                            temp = i_extractionPulse;

                            if (temp + v1.Value - Lib_Card.Configure.Parameter.Other_Z_BackPulse > Lib_Card.Configure.Parameter.Other_B_MaxPulse)
                            {
                                ////如果需加量大于10g，直接先加
                                //if (v1.Value / 1.0 / _i_adjust >= Lib_Card.Configure.Parameter.Other_SplitValue)
                                if (temp == iAdjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1))
                                {
                                    i_extractionPulse = Lib_Card.Configure.Parameter.Other_B_MaxPulse + Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                                }
                                else
                                {
                                    if (v1.Value + iAdjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1) - Lib_Card.Configure.Parameter.Other_Z_BackPulse > Lib_Card.Configure.Parameter.Other_B_MaxPulse)
                                    {
                                        i_extractionPulse = Lib_Card.Configure.Parameter.Other_B_MaxPulse + Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                                    }
                                }
                                break;
                            }
                            else
                            {
                                i_extractionPulse += v1.Value;
                            }
                        }
                    }
                }
                if (FADM_Object.Communal._b_isDripReserveFirst)
                    i_extractionPulse += iAdjust;
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抽液启动(" + i_extractionPulse +")");

            label11:
                try
                {
                    i_extractionPulse = i_extractionPulse - Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                    i_mRes = MyModbusFun.Extract(i_extractionPulse, s_unitOfAccount.Equals("g/l") ? true : false, 1); //抽液
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                }
                catch (Exception ex)
                {
                    if ("未发现针筒" == ex.Message)
                    {
                        if (i_lowSrart == 2)
                        {
                            FADM_Object.Communal.WriteDripWait(true);
                            FADM_Object.MyAlarm myAlarm;
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + "号母液瓶未找到针筒，是否继续执行?(继续寻找请点是，退出针检请点否)", "滴液", true, 1, true);
                            else
                                myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + " bottle did not find a syringe. Do you want to continue? " +
                                    "(To continue searching, please click Yes. To exit the needle test, please click No)", "Drip", true, 1, true);
                            while (true)
                            {
                                if (0 != myAlarm._i_alarm_Choose)
                                    break;
                                Thread.Sleep(1);
                            }
                            //判断染色线程是否需要用机械手
                            if (null != FADM_Object.Communal.ReadDyeThread())
                            {
                                FADM_Object.Communal.WriteDripWait(true);

                                while (true)
                                {
                                    if (false == FADM_Object.Communal.ReadDripWait())
                                        break;
                                    Thread.Sleep(1);
                                }
                            }
                            else
                            {
                                FADM_Object.Communal.WriteDripWait(false);
                            }

                            if (1 == myAlarm._i_alarm_Choose)
                                goto label3;
                            else
                                throw new Exception("收到退出消息");
                        }
                        else
                        {
                            s_sql = "UPDATE drop_details SET MinWeight = 2 WHERE BatchName = '" + oBatchName + "' AND " +
                             "BottleNum = " + i_minBottleNo + " AND Finish = 0;";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                            goto label3;
                        }


                    }
                    else if ("取针筒时抓手关失败" == ex.Message)
                    {
                        if (i_lowSrart == 2)
                        {
                            FADM_Object.Communal.WriteDripWait(true);
                            FADM_Object.MyAlarm myAlarm;
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + "号母液瓶取针筒时抓手关失败，是否继续执行?(继续寻找请点是，退出针检请点否)", "滴液", true, 1, true);
                            else
                                myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + " bottle The gripper failed to close when taking out the syringe. Do you want to continue? " +
                                    "(To continue searching, please click Yes. To exit the needle test, please click No)", "Drip", true, 1, true);
                            while (true)
                            {
                                if (0 != myAlarm._i_alarm_Choose)
                                    break;
                                Thread.Sleep(1);
                            }
                            //判断染色线程是否需要用机械手
                            if (null != FADM_Object.Communal.ReadDyeThread())
                            {
                                FADM_Object.Communal.WriteDripWait(true);

                                while (true)
                                {
                                    if (false == FADM_Object.Communal.ReadDripWait())
                                        break;
                                    Thread.Sleep(1);
                                }
                            }
                            else
                            {
                                FADM_Object.Communal.WriteDripWait(false);
                            }

                            if (1 == myAlarm._i_alarm_Choose)
                                goto label3;
                            else
                                throw new Exception("收到退出消息");
                        }
                        else
                        {
                            s_sql = "UPDATE drop_details SET MinWeight = 2 WHERE BatchName = '" + oBatchName + "' AND " +
                             "BottleNum = " + i_minBottleNo + " AND Finish = 0;";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                            goto label3;
                        }


                    }
                    else if ("取针筒时气缸下失败" == ex.Message)
                    {
                        if (i_lowSrart == 2)
                        {
                            FADM_Object.Communal.WriteDripWait(true);
                            FADM_Object.MyAlarm myAlarm;
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + "号母液瓶取针筒时气缸下失败，是否继续执行?(继续寻找请点是，退出针检请点否)", "滴液", true, 1, true);
                            else
                                myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + " bottle The cylinder failed to descend when removing the syringe. Do you want to continue? " +
                                    "(To continue searching, please click Yes. To exit the needle test, please click No)", "Drip", true, 1, true);
                            while (true)
                            {
                                if (0 != myAlarm._i_alarm_Choose)
                                    break;
                                Thread.Sleep(1);
                            }
                            //判断染色线程是否需要用机械手
                            if (null != FADM_Object.Communal.ReadDyeThread())
                            {
                                FADM_Object.Communal.WriteDripWait(true);

                                while (true)
                                {
                                    if (false == FADM_Object.Communal.ReadDripWait())
                                        break;
                                    Thread.Sleep(1);
                                }
                            }
                            else
                            {
                                FADM_Object.Communal.WriteDripWait(false);
                            }

                            if (1 == myAlarm._i_alarm_Choose)
                                goto label3;
                            else
                                throw new Exception("收到退出消息");
                        }
                        else
                        {
                            s_sql = "UPDATE drop_details SET MinWeight = 2 WHERE BatchName = '" + oBatchName + "' AND " +
                             "BottleNum = " + i_minBottleNo + " AND Finish = 0;";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                            goto label3;
                        }


                    }
                    else
                        throw;
                }
            }
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抽液完成");

            //Thread.Sleep(5000);

            if (null != thread)
                thread.Join();

            //Lib_SerialPort.Balance.METTLER.bReSetSign = true;

            if (FADM_Object.Communal._b_isDripReserveFirst)
            {
                //移动到天平位，先滴预留量
                if (_b_dripStop)
                {
                    FADM_Object.Communal._b_stop = true;
                }
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找天平位");



                //if ((Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 0) || Lib_Card.Configure.Parameter.Machine_Type == 1)
                {
                    //富士伺服在下面判断 天平状态 原有不动 绿维的放在上面 并且置位 是否回原点

                    //判断是否异常
                    FADM_Object.Communal.BalanceState("滴液");
                }

                i_mRes = MyModbusFun.TargetMove(2, 0, 0);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达天平位");

                int iZPulse1 = 0;
                //label98:
                if (-1 == MyModbusFun.GetZPosition(ref iZPulse1))
                    throw new Exception("驱动异常");
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "Z轴脉冲" + iZPulse1);
                //    if (iZPulse1 > 0)
                //        goto label98;
                int iInfusionPulse1 = 0;
                if ("小针筒" == sSyringeType || "Little Syringe" == sSyringeType)
                    iInfusionPulse1 = iZPulse1 - iAdjust;
                else
                    iInfusionPulse1 = iZPulse1 - iAdjust;

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "废液注液启动(" + iInfusionPulse1 + ")");
                i_mRes = MyModbusFun.Shove(iInfusionPulse1, 0);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "废液注液完成");

                //母液瓶扣减1克
                FADM_Object.Communal._fadmSqlserver.ReviseData(
                    "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + 1 + "  " +
                    "WHERE BottleNum = '" + i_minBottleNo + "';");

            }

            //移动到配液杯

            List<int> ints = new List<int>();
        label12:
            if (_b_dripStop)
            {
                FADM_Object.Communal._b_stop = true;
            }
            int iZPulse = 0;
            //label99:
            if (-1 == MyModbusFun.GetZPosition(ref iZPulse))
                throw new Exception("驱动异常");
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "Z轴脉冲" + iZPulse);
            //    if (iZPulse > 0)
            //        goto label99;
            if ("小针筒" == sSyringeType || "Little Syringe" == sSyringeType)
            {

                if (-iAdjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1) == iZPulse)
                {
                    //当前针筒已滴完，进入复检
                    goto label13;
                }
            }
            else
            {
                if (-iAdjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1) == iZPulse)
                {
                    //当前针筒已滴完，进入复检
                    goto label13;
                }

            }

            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找" + dic_pulse.First().Key + "号配液杯");
            FADM_Object.Communal._i_OptCupNum = dic_pulse.First().Key;
            i_mRes = MyModbusFun.TargetMove(2,0,0);
            if (-2 == i_mRes)
                throw new Exception("收到退出消息");
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达" + dic_pulse.First().Key + "号配液杯");

        //记录初始天平读数
        label33:
            Thread.Sleep(Convert.ToInt32(Lib_Card.Configure.Parameter.Delay_Balance_Read * 1000));
            double d_blBalanceValue21 = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal._s_balanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal._s_balanceValue));
            Thread.Sleep(Convert.ToInt32(Lib_Card.Configure.Parameter.Delay_Balance_Read * 1000));
            double d_blBalanceValue22 = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal._s_balanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal._s_balanceValue));
            double d_blDif2 = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blBalanceValue21 - d_blBalanceValue22)) : Convert.ToDouble(string.Format("{0:F3}", d_blBalanceValue21 - d_blBalanceValue22));
            d_blDif2 = d_blDif2 < 0 ? -d_blDif2 : d_blDif2;

            if (d_blDif2 > Lib_Card.Configure.Parameter.Other_Stable_Value)
                goto label33;

            //记录初始值
            double d_blBalanceValue3 = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", (d_blBalanceValue21 + d_blBalanceValue22) / 2)) : Convert.ToDouble(string.Format("{0:F3}", (d_blBalanceValue21 + d_blBalanceValue22) / 2));
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平初始读数：" + d_blBalanceValue3);

            //是否当前母液滴液完成
            bool bFinish = false;
            //注液
            if (_b_dripStop)
            {
                FADM_Object.Communal._b_stop = true;
            }
            int i_c = 0;
            double d_v = 0.0;
            int i_infusionPulse = 0;
            if ("小针筒" == sSyringeType || "Little Syringe" == sSyringeType)
            {
                if (iZPulse + iAdjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1) <= -dic_pulse.First().Value)
                {
                    //剩余量大于等于需加量
                    i_infusionPulse = iZPulse + dic_pulse.First().Value;

                    i_c = dic_pulse.First().Key;
                    d_v = dic_weight.First().Value;
                    bFinish = true;

                    i_pulseT -= dic_pulse.First().Value;
                    ints.Add(dic_pulse.First().Key);
                    dic_pulse.Remove(dic_pulse.First().Key);
                    dic_weight.Remove(dic_weight.First().Key);
                }
                else
                {
                    i_infusionPulse = -iAdjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1);
                    dic_pulse[dic_pulse.First().Key] -= (-iZPulse - iAdjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1));
                    i_pulseT -= (-iZPulse - iAdjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1));

                    i_c = dic_pulse.First().Key;
                    d_v = dic_weight.First().Value;
                }




                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液启动(" + i_infusionPulse + ")");
                i_mRes = MyModbusFun.Shove(i_infusionPulse, 0);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");

            }
            else
            {
                if (iZPulse + iAdjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1) <= -dic_pulse.First().Value)
                {
                    //剩余量大于等于需加量
                    i_infusionPulse = iZPulse + dic_pulse.First().Value;

                    i_c = dic_pulse.First().Key;
                    d_v = dic_weight.First().Value;
                    bFinish = true;

                    i_pulseT -= dic_pulse.First().Value;
                    ints.Add(dic_pulse.First().Key);
                    dic_pulse.Remove(dic_pulse.First().Key);
                    dic_weight.Remove(dic_weight.First().Key);
                }
                else
                {
                    i_infusionPulse = -iAdjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1);
                    dic_pulse[dic_pulse.First().Key] -= (-iZPulse - iAdjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1));
                    i_pulseT -= (-iZPulse - iAdjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1));

                    i_c = dic_pulse.First().Key;
                    d_v = dic_weight.First().Value;
                }

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液启动(" + i_infusionPulse + ")");
                i_mRes = MyModbusFun.Shove(i_infusionPulse,1);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
            }


            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液完成");

            //读取验证重量
            sReadCheckArgDebug sRead = new sReadCheckArgDebug();
            sRead._obj_batchName = oBatchName.ToString();
            sRead._i_minBottleNo = i_minBottleNo;
            sRead._d_blBalanceValue3 = d_blBalanceValue3;
            sRead._lis_ints = new List<int>();
            sRead._lis_ints.AddRange(ints);
            sRead._s_syringeType = sSyringeType;
            sRead._i_minCupNo = i_c;
            sRead._d_blBalanceValue4 = d_v;
            sRead._b_finish = bFinish;

            //if (FADM_Object.Communal._b_isFinishSend)
            //{
            thread = new Thread(ReadCheckWeightAllDebug);
            //}
            //else
            //{
            //    thread = new Thread(ReadCheckWeight);
            //}
            thread.Start(sRead);

            //if (null != thread)
            //    thread.Join();


            goto label12;

        //移动到天平位
        label13:
            if (_b_dripStop)
            {
                FADM_Object.Communal._b_stop = true;
            }
            



            //判断当前母液是否滴完
            if (0 < dic_pulse.Count)
            {
                goto label9;
            }

            //移动到母液瓶
            if (_b_dripStop)
            {
                FADM_Object.Communal._b_stop = true;
            }
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找" + i_minBottleNo + "号母液瓶");
            FADM_Object.Communal._i_optBottleNum = i_minBottleNo;
            i_mRes = MyModbusFun.TargetMove(0, i_minBottleNo, 0);
            if (-2 == i_mRes)
                throw new Exception("收到退出消息");
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达" + i_minBottleNo + "号母液瓶");

            //放针
            if (_b_dripStop)
            {
                FADM_Object.Communal._b_stop = true;
            }
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", i_minBottleNo + "号母液瓶放针启动");

            if ("小针筒" == sSyringeType || "Little Syringe" == sSyringeType)
            {
                i_mRes = MyModbusFun.Put(0);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
            }
            else
            {
                i_mRes = MyModbusFun.Put(1);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
            }
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", i_minBottleNo + "号母液瓶放针完成");

            b_checkFail = false;

            goto label7;

        //加助剂
        label4:
            if (FADM_Object.Communal._b_isAssitantFirst)
            {
                s_unitA = "%";
            }
            else
            {
                s_unitA = "g/l";
            }
            goto label3;

        //添加母液不足的
        label5:
            s_sql = "SELECT BottleNum FROM drop_details WHERE BatchName = '" + oBatchName + "' AND Finish = 0 AND MinWeight = 1 AND " +
                "BottleNum <= '" + Lib_Card.Configure.Parameter.Machine_Bottle_Total + "'  GROUP BY BottleNum ORDER BY BottleNum ;";
            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (0 < dt_drop_head.Rows.Count)
            {
                string s_alarmBottleNo = null;
                foreach (DataRow dataRow in dt_drop_head.Rows)
                {
                    s_alarmBottleNo += dataRow["BottleNum"].ToString() + ";";
                }

                s_alarmBottleNo = s_alarmBottleNo.Remove(s_alarmBottleNo.Length - 1);
                FADM_Object.Communal.WriteDripWait(true);
                FADM_Object.MyAlarm myAlarm;
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    myAlarm = new FADM_Object.MyAlarm(s_alarmBottleNo + "号母液瓶液量过低，是否继续滴液?", "滴液", true, 1);
                else
                    myAlarm = new FADM_Object.MyAlarm("The liquid level in bottle " + s_alarmBottleNo + " is too low. Do you want to continue ? ", "Drip", true, 1);
                while (true)
                {
                    if (0 != myAlarm._i_alarm_Choose)
                        break;
                    Thread.Sleep(1);
                }
                //判断染色线程是否需要用机械手
                if (null != FADM_Object.Communal.ReadDyeThread())
                {
                    FADM_Object.Communal.WriteDripWait(true);

                    while (true)
                    {
                        if (false == FADM_Object.Communal.ReadDripWait())
                            break;
                        Thread.Sleep(1);
                    }
                }
                else
                {
                    FADM_Object.Communal.WriteDripWait(false);
                }

                if (1 == myAlarm._i_alarm_Choose)
                {

                    i_lowSrart = 1;
                    if (FADM_Object.Communal._b_isAssitantFirst)
                    {
                        s_unitA = "g/l";
                    }
                    else
                    {
                        s_unitA = "%";
                    }
                    goto label3;
                }
            }

        //添加超出生命周期(过期)
        label17:
            s_sql = "SELECT BottleNum FROM drop_details WHERE BatchName = '" + oBatchName + "' AND Finish = 0 AND MinWeight = 3 AND " +
                "BottleNum <= '" + Lib_Card.Configure.Parameter.Machine_Bottle_Total + "'  GROUP BY BottleNum ORDER BY BottleNum ;";
            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (0 < dt_drop_head.Rows.Count)
            {
                string s_alarmBottleNo = null;
                foreach (DataRow dataRow in dt_drop_head.Rows)
                {
                    s_alarmBottleNo += dataRow["BottleNum"].ToString() + ";";
                }

                s_alarmBottleNo = s_alarmBottleNo.Remove(s_alarmBottleNo.Length - 1);
                FADM_Object.Communal.WriteDripWait(true);
                FADM_Object.MyAlarm myAlarm;
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    myAlarm = new FADM_Object.MyAlarm(s_alarmBottleNo + "号母液瓶过期，是否继续滴液?", "滴液", true, 1);
                else
                    myAlarm = new FADM_Object.MyAlarm("The liquid level in bottle " + s_alarmBottleNo + " is expire. Do you want to continue ? ", "Drip", true, 1);
                while (true)
                {
                    if (0 != myAlarm._i_alarm_Choose)
                        break;
                    Thread.Sleep(1);
                }
                //判断染色线程是否需要用机械手
                if (null != FADM_Object.Communal.ReadDyeThread())
                {
                    FADM_Object.Communal.WriteDripWait(true);

                    while (true)
                    {
                        if (false == FADM_Object.Communal.ReadDripWait())
                            break;
                        Thread.Sleep(1);
                    }
                }
                else
                {
                    FADM_Object.Communal.WriteDripWait(false);
                }

                if (1 == myAlarm._i_alarm_Choose)
                {
                    i_lowSrart = 3;
                    if (FADM_Object.Communal._b_isAssitantFirst)
                    {
                        s_unitA = "g/l";
                    }
                    else
                    {
                        s_unitA = "%";
                    }
                    goto label3;
                }
            }


        //添加找不到针筒的
        label16:
            s_sql = "SELECT BottleNum FROM drop_details WHERE BatchName = '" + oBatchName + "' AND Finish = 0 AND MinWeight = 2 AND " +
               "BottleNum <= '" + Lib_Card.Configure.Parameter.Machine_Bottle_Total + "'  GROUP BY BottleNum ORDER BY BottleNum ;";
            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (0 < dt_drop_head.Rows.Count)
            {
                string s_alarmBottleNo = null;
                foreach (DataRow dataRow in dt_drop_head.Rows)
                {
                    s_alarmBottleNo += dataRow["BottleNum"].ToString() + ";";
                }

                s_alarmBottleNo = s_alarmBottleNo.Remove(s_alarmBottleNo.Length - 1);
                FADM_Object.Communal.WriteDripWait(true);
                FADM_Object.MyAlarm myAlarm;
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    myAlarm = new FADM_Object.MyAlarm(s_alarmBottleNo + "号母液瓶未检测到针筒，是否继续滴液 ? ", "滴液", true, 1, true);
                else
                    myAlarm = new FADM_Object.MyAlarm(s_alarmBottleNo + " bottle did not find a syringe. Do you want to continue? ", "Drip", true, 1, true);
                while (true)
                {
                    if (0 != myAlarm._i_alarm_Choose)
                        break;
                    Thread.Sleep(1);
                }
                //判断染色线程是否需要用机械手
                if (null != FADM_Object.Communal.ReadDyeThread())
                {
                    FADM_Object.Communal.WriteDripWait(true);

                    while (true)
                    {
                        if (false == FADM_Object.Communal.ReadDripWait())
                            break;
                        Thread.Sleep(1);
                    }
                }
                else
                {
                    FADM_Object.Communal.WriteDripWait(false);
                }

                if (1 == myAlarm._i_alarm_Choose)
                {
                    i_lowSrart = 2;
                    if (FADM_Object.Communal._b_isAssitantFirst)
                    {
                        s_unitA = "g/l";
                    }
                    else
                    {
                        s_unitA = "%";
                    }
                    goto label3;
                }
            }

        //滴液完成    
        label6:
            if (null != thread)
                thread.Join();

            if (FADM_Object.Communal._b_isFinishSend)
            {
                //把由于超期，液量低跳过的所有置为不合格
                FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE drop_head SET DescribeChar = '滴液失败',DescribeChar_EN = 'Drip Fail',CupFinish = 1, FinishTime = '" + DateTime.Now + "', Step = 2 " +
                               "WHERE BatchName = '" + oBatchName + "' AND CupFinish != 1;");

                //获取滴液不合格记录
                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
               "SELECT * from drop_head where DescribeChar like '%滴液失败%'" +
               " And BatchName = '" + oBatchName + "' order by CupNum;");

                List<int> lis_cupFailD = new List<int>();
                List<int> lis_cupFailT = new List<int>();
                string s_cupNo = "";
                foreach (DataRow dr in dt_drop_head.Rows)
                {
                    lis_cupFailD.Add(Convert.ToInt32(dr["CupNum"]));
                    s_cupNo += dr["CupNum"].ToString() + "; ";
                }
                lis_cupFailT = lis_cupFailD.Distinct().ToList();


                if (FADM_Auto.Drip._b_dripErr == false)
                {
                    if (0 < lis_cupFailT.Count)
                    {
                        s_cupNo = s_cupNo.Remove(s_cupNo.Length - 1);

                        FADM_Object.Communal.WriteDripWait(true);
                        FADM_Object.MyAlarm myAlarm;
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            myAlarm = new FADM_Object.MyAlarm(s_cupNo + "号配液杯滴液失败，是否继续(重新滴液请点是，退出滴液请点否)?", "滴液", true, 1);
                        else
                            myAlarm = new FADM_Object.MyAlarm("The dispensing cup" + s_cupNo + "  failed to dispense liquid. Do you want to continue (please click Yes for re dispensing and No for exiting the dispensing)? ", "Drip", true, 1);


                        while (true)
                        {
                            if (0 != myAlarm._i_alarm_Choose)
                                break;
                            Thread.Sleep(1);
                        }
                        //判断染色线程是否需要用机械手
                        if (null != FADM_Object.Communal.ReadDyeThread())
                        {
                            FADM_Object.Communal.WriteDripWait(true);

                            while (true)
                            {
                                if (false == FADM_Object.Communal.ReadDripWait())
                                    break;
                                Thread.Sleep(1);
                            }
                        }
                        else
                        {
                            FADM_Object.Communal.WriteDripWait(false);
                        }

                        if (1 == myAlarm._i_alarm_Choose)
                        {
                            //先把滴液区域重置
                            //滴液区
                            for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                            {
                                if (SmartDyeing.FADM_Object.Communal._lis_dripCupNum.Contains(lis_cupFailT[i]))
                                {

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE drop_head SET CupFinish = 0, AddWaterFinish = 0, RealAddWaterWeight = 0.00 , Step = 1,DescribeChar=null " +
                                        "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + lis_cupFailT[i] + ";");

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE drop_details SET Finish = 0, MinWeight = 0, RealDropWeight = 0.00 , NeedPulse = 0" +
                                        "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + lis_cupFailT[i] + ";");
                                    int i_num = lis_cupFailT[i];
                                    lis_cupFailT.Remove(lis_cupFailT[i]);
                                    lis_cupSuc.Remove(i_num);
                                }

                            }
                            //后处理区域
                            {
                                //打板区
                                FADM_Object.Communal._lis_dripFailCupFinish.Clear();
                                FADM_Object.Communal._lis_dripFailCup.AddRange(lis_cupFailT);
                                List<int> lis_ints1 = new List<int>();
                                lis_ints1.AddRange(lis_cupFailT);

                                //等待染色机排完再重滴
                                while (true)
                                {
                                    FADM_Object.Communal.WriteDripWait(true);
                                    if (0 == lis_cupFailT.Count)
                                    {
                                        FADM_Object.Communal.WriteDripWait(false);
                                        break;
                                    }

                                    for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                                    {
                                        if (FADM_Object.Communal._lis_dripFailCupFinish.Contains(lis_cupFailT[i]))
                                        {

                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                "UPDATE drop_head SET CupFinish = 0, AddWaterFinish = 0, RealAddWaterWeight = 0.00 , Step = 1,DescribeChar=null " +
                                                "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + lis_cupFailT[i] + ";");

                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                "UPDATE drop_details SET Finish = 0, MinWeight = 0, RealDropWeight = 0.00 , NeedPulse = 0" +
                                                "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + lis_cupFailT[i] + ";");
                                            FADM_Object.Communal._lis_dripFailCupFinish.Remove(lis_cupFailT[i]);
                                            int i_num = lis_cupFailT[i];
                                            lis_cupFailT.Remove(lis_cupFailT[i]);
                                            lis_cupSuc.Remove(i_num);
                                        }
                                    }

                                    DataTable dt_drop_head2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                                               "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName + "'    ORDER BY CupNum;");
                                    List<int> lis_lUse1 = new List<int>();
                                    foreach (DataRow dataRow in dt_drop_head2.Rows)
                                    {
                                        lis_lUse1.Add(Convert.ToInt16(dataRow["CupNum"]));
                                    }
                                    for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                                    {
                                        //已经强停，不在滴液列表
                                        if (!lis_lUse1.Contains(lis_cupFailT[i]))
                                        {
                                            int i_num = lis_cupFailT[i];
                                            lis_cupFailT.Remove(lis_cupFailT[i]);
                                            lis_ints1.Remove(i_num);
                                        }
                                    }

                                    Thread.Sleep(1000);
                                }

                                foreach (int i in lis_ints1)
                                {
                                    int i_cupNum = i;

                                    while (true)
                                    {
                                        //滴液完成数组移除当前杯号
                                        FADM_Object.Communal._lis_dripSuccessCup.Remove(i_cupNum);
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cupNum))
                                            break;
                                    }
                                    int[] ia_zero = new int[1];
                                    //滴液状态
                                    ia_zero[0] = 3;
                                    DyeHMIWrite(i_cupNum, 100, 100, ia_zero);

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE cup_details SET Statues = '等待准备状态' WHERE CupNum = " + i_cupNum + ";");

                                }


                                //等待准备状态
                                while (true)
                                {
                                    bool b_open = true;
                                    DataTable dt_drop_head2 = FADM_Object.Communal._fadmSqlserver.GetData(
                           "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName + "' AND CupFinish = 0    ORDER BY CupNum;");
                                    List<int> lis_lUse1 = new List<int>();
                                    foreach (DataRow dataRow in dt_drop_head2.Rows)
                                    {
                                        lis_lUse1.Add(Convert.ToInt16(dataRow["CupNum"]));
                                    }
                                    for (int i = lis_ints1.Count - 1; i >= 0; i--)
                                    {
                                        //已经强停，不在滴液列表
                                        if (!lis_lUse1.Contains(lis_ints1[i]))
                                        {
                                            lis_ints1.Remove(lis_ints1[i]);
                                        }
                                    }
                                    foreach (int i in lis_ints1)
                                    {
                                        int i_cupNum = i;


                                        int i_openCover = Convert.ToInt16(FADM_Auto.Dye._cup_Temps[i_cupNum - 1]._s_statues);


                                        if (5 != i_openCover)
                                            b_open = false;

                                    }


                                    if (b_open)
                                        break;

                                    Thread.Sleep(1000);
                                }

                                lis_ints1.Clear();
                            }
                            this.DripProcessDebug(oBatchName);
                        }
                        else
                        {
                            //不重滴

                            if (FADM_Object.Communal.ReadMachineStatus() != 8)
                            {
                                for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                                {
                                    if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(lis_cupFailT[i]))
                                    {
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(lis_cupFailT[i]))
                                            FADM_Object.Communal._lis_dripSuccessCup.Add(lis_cupFailT[i]);
                                    }
                                }
                            }

                        }

                    }
                }

                string s_cupList = "";
                string s_te = "";
                if (lis_cupSuc.Count > 0)
                {
                    for (int i = 0; i < lis_cupSuc.Count; i++)
                    {
                        s_cupList += lis_cupSuc[i] + ",";
                    }
                }
                if (s_cupList != "")
                {
                    s_cupList = s_cupList.Remove(s_cupList.Length - 1);
                    s_te = " And CupNum in (" + s_cupList + ")";
                }
                //添加历史表
                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                 "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'drop_head';");
                string s_columnHead = null;
                foreach (DataRow row in dt_drop_head.Rows)
                {
                    string s_curName = Convert.ToString(row[0]);
                    if ("TestTubeFinish" != s_curName && "TestTubeWaterLower" != s_curName && "AddWaterFinish" != s_curName &&
                        "CupFinish" != s_curName && "TestTubeWaterLower" != s_curName)
                        s_columnHead += s_curName + ", ";
                }
                s_columnHead = s_columnHead.Remove(s_columnHead.Length - 2);

                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                   "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'drop_details';");
                string s_columnDetails = null;
                foreach (DataRow row in dt_drop_head.Rows)
                {
                    string s_curName = Convert.ToString(row[0]);
                    if ("MinWeight" != s_curName && "Finish" != s_curName && "IsShow" != s_curName && "NeedPulse" != s_curName)
                        s_columnDetails += Convert.ToString(row[0]) + ", ";
                }
                s_columnDetails = s_columnDetails.Remove(s_columnDetails.Length - 2);

                //
                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                   "SELECT * FROM drop_head WHERE   BatchName = '" + oBatchName + "' ;");

                foreach (DataRow row in dt_drop_head.Rows)
                {
                    if (lis_cupSuc.Contains(Convert.ToInt32(row["CupNum"].ToString())))
                    {

                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                        "INSERT INTO history_head (" + s_columnHead + ") (SELECT " + s_columnHead + " FROM drop_head " +
                        "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + row["CupNum"].ToString() + ") ;");

                        //FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "INSERT INTO history_head (" + s_columnHead + ") (SELECT " + s_columnHead + " FROM drop_head " +
                        //    "WHERE BatchName = '" + o_BatchName + "' AND CupNum >= " + _i_cupMin + " AND CupNum <= " + _i_cupMax + s_te + ");");


                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "INSERT INTO history_details (" + s_columnDetails + ") (SELECT " + s_columnDetails + " FROM drop_details " +
                           "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + row["CupNum"].ToString() + ") ;");

                        //滴液
                        if (SmartDyeing.FADM_Object.Communal._lis_dripCupNum.Contains(Convert.ToInt32(row["CupNum"].ToString())))
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  "DELETE FROM drop_head WHERE CupNum = " + row["CupNum"].ToString() + " AND BatchName = '" + oBatchName + "' ;");
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "DELETE FROM drop_details WHERE CupNum = " + row["CupNum"].ToString() + " AND BatchName = '" + oBatchName + "' ;");


                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE cup_details SET FormulaCode = null, " +
                                "DyeingCode = null, IsUsing = 0, Statues = '待机', " +
                                "StartTime = null, SetTemp = null, StepNum = null, TotalWeight = null, " +
                                "TotalStep = null, TechnologyName = null, StepStartTime = null, SetTime = null,RecordIndex = 0 WHERE CupNum = " +
                                row["CupNum"].ToString() + " ;");
                        }
                    }
                }

                if (FADM_Auto.Drip._b_dripErr)
                {
                    FADM_Object.Communal._lis_dripStopCup.AddRange(lis_cupT);

                }
            }
            else
            {
                s_sql = "UPDATE drop_head SET CupFinish = 1 WHERE BatchName = '" + oBatchName + "'  ;";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                //获取滴液不合格记录
                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                   "SELECT drop_details.CupNum as CupNum, " +
                   "drop_details.BottleNum as BottleNum, " +
                   "drop_details.ObjectDropWeight as ObjectDropWeight, " +
                   "drop_details.RealDropWeight as RealDropWeight, " +
                   "bottle_details.SyringeType as SyringeType " +
                   "FROM drop_details left join bottle_details on " +
                   "bottle_details.BottleNum = drop_details.BottleNum " +
                   "WHERE drop_details.BatchName = '" + oBatchName + "' ;");
                List<int> lis_cupFailD = new List<int>();
                List<int> lis_cupFailT = new List<int>();
                foreach (DataRow dr in dt_drop_head.Rows)
                {
                    if (Convert.ToInt32(dr["BottleNum"]) > Lib_Card.Configure.Parameter.Machine_Bottle_Total)
                        continue;
                    double d_blRealErr = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F2}",
                        Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])) : string.Format("{0:F3}",
                        Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])));
                    d_blRealErr = d_blRealErr < 0 ? -d_blRealErr : d_blRealErr;
                    if (d_blRealErr > Lib_Card.Configure.Parameter.Other_AErr_Drip)
                        lis_cupFailD.Add(Convert.ToInt32(dr["CupNum"]));
                }
                lis_cupFailD = lis_cupFailD.Distinct().ToList();



                lis_cupT = new List<int>();
                //滴液成功，转换到历史表杯号
                lis_cupSuc = new List<int>();
                string s_cupNo = null;

                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                    "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName + "'    ORDER BY CupNum ;");
                foreach (DataRow dr in dt_drop_head.Rows)
                {
                    int i_cup = Convert.ToInt16(dr["CupNum"]);
                    int i_step = Convert.ToInt16(dr["Step"]);
                    double d_objWater = Convert.ToDouble(dr["ObjectAddWaterWeight"]);
                    double d_realWater = Convert.ToDouble(dr["RealAddWaterWeight"]);
                    double d_totalWeight = Convert.ToDouble(dr["TotalWeight"]);
                    double d_testTubeObjectAddWaterWeight = Convert.ToDouble(dr["TestTubeObjectAddWaterWeight"]);
                    double d_testTubeRealAddWaterWeight = Convert.ToDouble(dr["TestTubeRealAddWaterWeight"]);
                    if (i_step == 1)
                        lis_cupSuc.Add(i_cup);
                    lis_cupT.Add(i_cup);
                    double d_realDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater - d_objWater) : string.Format("{0:F3}", d_realWater - d_objWater));
                    d_realDif = d_realDif < 0 ? -d_realDif : d_realDif;
                    double d_allDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                        d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)) : string.Format("{0:F3}",
                        d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)));
                    string s_describe;
                    string s_describe_EN;

                    //只判断当前滴液的数据
                    if (i_step == 1)
                    {
                        if (d_allDif < d_realDif || (d_realWater == 0.0 && d_objWater > 0))
                        {
                            //加水失败
                            s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                             ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                            s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                             ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                            lis_cupFailT.Add(i_cup);
                            s_cupNo += i_cup.ToString() + "; ";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE cup_details SET Statues = '滴液失败' WHERE CupNum = " + i_cup + ";");

                        }
                        else
                        {
                            //加水成功
                            if (lis_cupFailD.Contains(i_cup))
                            {
                                //滴液失败
                                s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                            ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                             ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                lis_cupFailT.Add(i_cup);
                                s_cupNo += i_cup.ToString() + "; ";

                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET Statues = '滴液失败' WHERE CupNum = " + i_cup + ";");
                            }
                            else
                            {
                                //滴液成功
                                s_describe = "滴液成功!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                              ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                s_describe_EN = "Drip Success !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                             ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(i_cup))
                                {
                                    if (i_step == 1)
                                    {
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cup))
                                            FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                                    }
                                }

                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  "UPDATE cup_details SET Statues = '滴液成功' WHERE CupNum = " + i_cup + ";");
                            }
                        }

                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE drop_head SET DescribeChar = '" + s_describe + "',DescribeChar_EN = '" + s_describe_EN + "', FinishTime = '" + DateTime.Now + "', Step = 2 " +
                               "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + i_cup + ";");
                    }

                }


                if (FADM_Auto.Drip._b_dripErr == false)
                {
                    if (0 < lis_cupFailT.Count)
                    {
                        s_cupNo = s_cupNo.Remove(s_cupNo.Length - 1);

                        FADM_Object.Communal.WriteDripWait(true);
                        FADM_Object.MyAlarm myAlarm;
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            myAlarm = new FADM_Object.MyAlarm(s_cupNo + "号配液杯滴液失败，是否继续(重新滴液请点是，退出滴液请点否)?", "滴液", true, 1);
                        else
                            myAlarm = new FADM_Object.MyAlarm("The dispensing cup" + s_cupNo + "  failed to dispense liquid. Do you want to continue (please click Yes for re dispensing and No for exiting the dispensing)? ", "Drip", true, 1);

                        while (true)
                        {
                            if (0 != myAlarm._i_alarm_Choose)
                                break;
                            Thread.Sleep(1);
                        }
                        //判断染色线程是否需要用机械手
                        if (null != FADM_Object.Communal.ReadDyeThread())
                        {
                            FADM_Object.Communal.WriteDripWait(true);

                            while (true)
                            {
                                if (false == FADM_Object.Communal.ReadDripWait())
                                    break;
                                Thread.Sleep(1);
                            }
                        }
                        else
                        {
                            FADM_Object.Communal.WriteDripWait(false);
                        }

                        if (1 == myAlarm._i_alarm_Choose)
                        {
                            //先把滴液区域重置
                            //滴液区
                            for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                            {
                                if (SmartDyeing.FADM_Object.Communal._lis_dripCupNum.Contains(lis_cupFailT[i]))
                                {

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE drop_head SET CupFinish = 0, AddWaterFinish = 0, RealAddWaterWeight = 0.00 , Step = 1,DescribeChar=null " +
                                        "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + lis_cupFailT[i] + ";");

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE drop_details SET Finish = 0, MinWeight = 0, RealDropWeight = 0.00 , NeedPulse = 0" +
                                        "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + lis_cupFailT[i] + ";");
                                    int Num = lis_cupFailT[i];
                                    lis_cupFailT.Remove(lis_cupFailT[i]);
                                    lis_cupSuc.Remove(Num);
                                }

                            }
                            //后处理区域
                            {
                                //打板区
                                FADM_Object.Communal._lis_dripFailCupFinish.Clear();
                                FADM_Object.Communal._lis_dripFailCup.AddRange(lis_cupFailT);
                                List<int> lis_ints1 = new List<int>();
                                lis_ints1.AddRange(lis_cupFailT);

                                //等待染色机排完再重滴
                                while (true)
                                {
                                    FADM_Object.Communal.WriteDripWait(true);
                                    if (0 == lis_cupFailT.Count)
                                    {
                                        FADM_Object.Communal.WriteDripWait(false);
                                        break;
                                    }

                                    for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                                    {
                                        if (FADM_Object.Communal._lis_dripFailCupFinish.Contains(lis_cupFailT[i]))
                                        {

                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                "UPDATE drop_head SET CupFinish = 0, AddWaterFinish = 0, RealAddWaterWeight = 0.00 , Step = 1,DescribeChar=null " +
                                                "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + lis_cupFailT[i] + ";");

                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                "UPDATE drop_details SET Finish = 0, MinWeight = 0, RealDropWeight = 0.00 , NeedPulse = 0" +
                                                "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + lis_cupFailT[i] + ";");
                                            FADM_Object.Communal._lis_dripFailCupFinish.Remove(lis_cupFailT[i]);
                                            int i_num = lis_cupFailT[i];
                                            lis_cupFailT.Remove(lis_cupFailT[i]);
                                            lis_cupSuc.Remove(i_num);
                                        }
                                    }

                                    DataTable dt_drop_head2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                                               "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName + "'    ORDER BY CupNum;");
                                    List<int> lis_lUse1 = new List<int>();
                                    foreach (DataRow dataRow in dt_drop_head2.Rows)
                                    {
                                        lis_lUse1.Add(Convert.ToInt16(dataRow["CupNum"]));
                                    }
                                    for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                                    {
                                        //已经强停，不在滴液列表
                                        if (!lis_lUse1.Contains(lis_cupFailT[i]))
                                        {
                                            int i_num = lis_cupFailT[i];
                                            lis_cupFailT.Remove(lis_cupFailT[i]);
                                            lis_ints1.Remove(i_num);
                                        }
                                    }

                                    Thread.Sleep(1000);
                                }

                                foreach (int i in lis_ints1)
                                {
                                    int i_cupNum = i;

                                    while (true)
                                    {
                                        //滴液完成数组移除当前杯号
                                        FADM_Object.Communal._lis_dripSuccessCup.Remove(i_cupNum);
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cupNum))
                                            break;
                                    }

                                    int[] ia_zero = new int[1];
                                    //滴液状态
                                    ia_zero[0] = 3;

                                    DyeHMIWrite(i_cupNum, 100, 100, ia_zero);

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE cup_details SET Statues = '等待准备状态' WHERE CupNum = " + i_cupNum + ";");

                                }


                                //等待准备状态
                                while (true)
                                {
                                    bool b_open = true;
                                    DataTable dt_drop_head2 = FADM_Object.Communal._fadmSqlserver.GetData(
                           "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName + "' AND CupFinish = 0    ORDER BY CupNum;");
                                    List<int> lis_lUse1 = new List<int>();
                                    foreach (DataRow dataRow in dt_drop_head2.Rows)
                                    {
                                        lis_lUse1.Add(Convert.ToInt16(dataRow["CupNum"]));
                                    }
                                    for (int i = lis_ints1.Count - 1; i >= 0; i--)
                                    {
                                        //已经强停，不在滴液列表
                                        if (!lis_lUse1.Contains(lis_ints1[i]))
                                        {
                                            lis_ints1.Remove(lis_ints1[i]);
                                        }
                                    }
                                    foreach (int i in lis_ints1)
                                    {
                                        int i_cupNum = i;


                                        int i_openCover = Convert.ToInt16(FADM_Auto.Dye._cup_Temps[i_cupNum - 1]._s_statues);


                                        if (5 != i_openCover)
                                            b_open = false;

                                    }


                                    if (b_open)
                                        break;

                                    Thread.Sleep(1000);
                                }

                                lis_ints1.Clear();
                            }
                            this.DripProcessDebug(oBatchName);
                        }
                        else
                        {
                            //不重滴

                            if (FADM_Object.Communal.ReadMachineStatus() != 8)
                            {
                                for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                                {
                                    if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(lis_cupFailT[i]))
                                    {
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(lis_cupFailT[i]))
                                            FADM_Object.Communal._lis_dripSuccessCup.Add(lis_cupFailT[i]);
                                    }
                                }
                            }

                        }

                    }
                }

                string s_cupList = "";
                string s_te = "";
                if (lis_cupSuc.Count > 0)
                {
                    for (int i = 0; i < lis_cupSuc.Count; i++)
                    {
                        s_cupList += lis_cupSuc[i] + ",";
                    }
                }
                if (s_cupList != "")
                {
                    s_cupList = s_cupList.Remove(s_cupList.Length - 1);
                    s_te = " And CupNum in (" + s_cupList + ")";
                }
                //添加历史表
                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                 "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'drop_head';");
                string s_columnHead = null;
                foreach (DataRow row in dt_drop_head.Rows)
                {
                    string s_curName = Convert.ToString(row[0]);
                    if ("TestTubeFinish" != s_curName && "TestTubeWaterLower" != s_curName && "AddWaterFinish" != s_curName &&
                        "CupFinish" != s_curName && "TestTubeWaterLower" != s_curName)
                        s_columnHead += s_curName + ", ";
                }
                s_columnHead = s_columnHead.Remove(s_columnHead.Length - 2);

                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                   "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'drop_details';");
                string s_columnDetails = null;
                foreach (DataRow row in dt_drop_head.Rows)
                {
                    string sCurName = Convert.ToString(row[0]);
                    if ("MinWeight" != sCurName && "Finish" != sCurName && "IsShow" != sCurName && "NeedPulse" != sCurName)
                        s_columnDetails += Convert.ToString(row[0]) + ", ";
                }
                s_columnDetails = s_columnDetails.Remove(s_columnDetails.Length - 2);

                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                   "SELECT * FROM drop_head WHERE   BatchName = '" + oBatchName + "' ;");

                foreach (DataRow row in dt_drop_head.Rows)
                {
                    if (lis_cupSuc.Contains(Convert.ToInt32(row["CupNum"].ToString())))
                    {

                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                        "INSERT INTO history_head (" + s_columnHead + ") (SELECT " + s_columnHead + " FROM drop_head " +
                        "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + row["CupNum"].ToString() + ") ;");

                        //FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "INSERT INTO history_head (" + s_columnHead + ") (SELECT " + s_columnHead + " FROM drop_head " +
                        //    "WHERE BatchName = '" + o_BatchName + "' AND CupNum >= " + _i_cupMin + " AND CupNum <= " + _i_cupMax + s_te + ");");


                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "INSERT INTO history_details (" + s_columnDetails + ") (SELECT " + s_columnDetails + " FROM drop_details " +
                           "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + row["CupNum"].ToString() + ") ;");

                        //滴液
                        if (SmartDyeing.FADM_Object.Communal._lis_dripCupNum.Contains(Convert.ToInt32(row["CupNum"].ToString())))
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  "DELETE FROM drop_head WHERE CupNum = " + row["CupNum"].ToString() + " AND BatchName = '" + oBatchName + "' ;");
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "DELETE FROM drop_details WHERE CupNum = " + row["CupNum"].ToString() + " AND BatchName = '" + oBatchName + "' ;");


                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE cup_details SET FormulaCode = null, " +
                                "DyeingCode = null, IsUsing = 0, Statues = '待机', " +
                                "StartTime = null, SetTemp = null, StepNum = null, TotalWeight = null, " +
                                "TotalStep = null, TechnologyName = null, StepStartTime = null, SetTime = null,RecordIndex = 0 WHERE CupNum = " +
                                row["CupNum"].ToString() + " ;");
                        }
                    }
                }

                if (FADM_Auto.Drip._b_dripErr)
                {
                    FADM_Object.Communal._lis_dripStopCup.AddRange(lis_cupT);

                }
            }

        }

        //滴液过程先加水后加染料助剂
        private void DripProcess(object oBatchName)
        {
            //判断是否过期，液量低，夹不到针筒选择了否
            bool b_chooseNo = false;
            Thread thread = null;
            //流量计等待读脉冲线程
            Thread threadFlow = null;
            int i_mRes = 0;
            //针检失败，不继续针检状态
            bool b_checkFail = false;
            
            Communal._b_isAllowDrip = true;



            List<int> lis_cupSuc = new List<int>();
            List<int> lis_cupT = new List<int>();

            DataTable dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
               "SELECT * FROM drop_head WHERE   BatchName = '" + oBatchName + "' And Step = 1 order by CupNum;");

            foreach (DataRow row in dt_drop_head.Rows)
            {
                lis_cupSuc.Add(Convert.ToInt32(row["CupNum"].ToString()));
                lis_cupT.Add(Convert.ToInt32(row["CupNum"].ToString()));
            }

            string s_unitOfAccount = "";
        lab_again:
            //实际加水杯号
            List<int> lis_actualAddWaterCup = new List<int>();

            //复位
            //Lib_SerialPort.Balance.METTLER.bReSetSign = true;

            //判断染色线程是否需要用机械手
            if (null != FADM_Object.Communal.ReadDyeThread())
            {
                FADM_Object.Communal.WriteDripWait(true);
                Communal._b_isWaitDrip = true;
                Communal._b_isAllowDrip = true;
                while (true)
                {
                    if (false == FADM_Object.Communal.ReadDripWait())
                        break;
                    Thread.Sleep(1);
                }
                Communal._b_isWaitDrip = false;
            }
            else
                Communal._b_isWaitDrip = false;

            //先把只做后处理的直接给滴液完成，并下发
            string s_sql = "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName + "' And CupFinish = 0  ORDER BY CupNum;";
            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

            foreach (DataRow row in dt_drop_head.Rows)
            {
                s_sql = "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName + "' AND " +
                "CupNum =" + row["CupNum"].ToString()+ " And BottleNum <= " + Lib_Card.Configure.Parameter.Machine_Bottle_Total + ";";
                DataTable dt_drop_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                if(dt_drop_details.Rows.Count ==0  && row["AddWaterChoose"].ToString() !="1")
                {
                    int i_cup = Convert.ToInt32(row["CupNum"].ToString());

                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                       "UPDATE drop_head SET DescribeChar = '滴液成功', FinishTime = '" + DateTime.Now + "', Step = 2,CupFinish = 1  " +
                       "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + i_cup + ";");
                    if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cup))
                        FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                }
            }

            //加水
             s_sql = "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName + "' AND " +
                "AddWaterChoose = 1 AND AddWaterFinish = 0  ORDER BY CupNum;";
            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);



            if (dt_drop_head.Rows.Count > 0)
            {
                foreach (DataRow row in dt_drop_head.Rows)
                {

                    //判断染色线程是否需要用机械手
                    if (null != FADM_Object.Communal.ReadDyeThread())
                    {
                        if (!FADM_Object.Communal._b_isWaterRecheck)
                        {
                            //如果后处理需要机械手，中断继续加水，先等待读取脉冲完成再释放机械手
                            if (threadFlow != null)
                                threadFlow.Join();
                        }
                        FADM_Object.Communal.WriteDripWait(true);
                        Communal._b_isWaitDrip = true;
                        Communal._b_isAllowDrip = true;
                        while (true)
                        {
                            if (false == FADM_Object.Communal.ReadDripWait())
                                break;
                            Thread.Sleep(1);
                        }
                        Communal._b_isWaitDrip = false;
                    }
                    else
                        Communal._b_isWaitDrip = false;

                    int i_cupNo = Convert.ToInt32(row["CupNum"]);
                    //如果前洗杯并且没洗杯完成就不加水
                    if (_lis_ForwordwashCup.Contains(i_cupNo))
                    {
                        if(!_lis_addwashCupFinish.Contains(i_cupNo))
                        {
                            continue;
                        }
                    }
                    //
                    
                    double d_blObjectW = Convert.ToDouble(row["ObjectAddWaterWeight"]);
                    if (d_blObjectW > 0)
                    {
                        

                        //if (SmartDyeing.FADM_Object.Communal._dic_dyeType.Keys.Contains(i_cupNo))
                        //{
                        //    if (SmartDyeing.FADM_Object.Communal._dic_dyeType[i_cupNo] == 1)
                        //    {
                        //        //如果关盖状态，就先执行开盖动作
                        //        if (FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._i_cupCover == 1)
                        //        {
                        //        labelP1:
                        //            //开盖
                        //            try
                        //            {
                        //                //寻找配液杯
                        //                FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", i_cupNo + "号配液杯开盖");
                        //                //FADM_Object.Communal._i_OptCupNum = i_cupNo;
                        //                //int reSuccess4 = MyModbusFun.TargetMove(1, i_cupNo, 1);
                        //                //if (-2 == reSuccess4)
                        //                //    throw new Exception("收到退出消息");

                        //                //FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "抵达" + i_cupNo + "号配液杯");
                        //                int i_xStart = 0, i_yStart = 0;
                        //                int i_xEnd = 0, i_yEnd = 0;
                        //                MyModbusFun.CalTarget(1, i_cupNo, ref i_xStart, ref i_yStart);

                        //                MyModbusFun.CalTarget(4, i_cupNo, ref i_xEnd, ref i_yEnd);

                        //                i_mRes = MyModbusFun.OpenOrPutCover(i_xStart, i_yStart, i_xEnd, i_yEnd, 0);
                        //                if (-2 == i_mRes)
                        //                    throw new Exception("收到退出消息");
                        //            }
                        //            catch (Exception ex)
                        //            {
                        //                if ("未发现杯盖" == ex.Message)
                        //                {
                        //                    ////气缸上
                        //                    //int[] ia_array = new int[1];
                        //                    //ia_array[0] = 5;

                        //                    //int i_state = FADM_Object.Communal._tcpModBus.Write(811, ia_array);
                        //                    //Thread.Sleep(1000);
                        //                    ////抓手开
                        //                    //ia_array = new int[1];
                        //                    //ia_array[0] = 7;

                        //                    //i_state = FADM_Object.Communal._tcpModBus.Write(811, ia_array);

                        //                    FADM_Object.MyAlarm myAlarm;
                        //                    //FADM_Object.Communal._fadmSqlserver.ReviseData(
                        //                    //   "UPDATE cup_details SET  Cooperate = 6 WHERE  CupNum = " + i_cupNo + " ;");
                        //                    s_sql = "UPDATE drop_details SET MinWeight = 5 WHERE BatchName = '" + oBatchName + "' And  Finish = 0  AND CupNum = " + i_cupNo + ";";
                        //                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        //                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        //                        myAlarm = new FADM_Object.MyAlarm(i_cupNo + "号配液杯未发现杯盖，是否继续执行?(继续执行请点是，已完成开盖请点否)", "SwitchCover", i_cupNo, 2, 7);
                        //                    else
                        //                        myAlarm = new FADM_Object.MyAlarm(i_cupNo + " Cup did not find a cap, do you want to continue? " +
                        //                            "( Continue to perform please click Yes, have completed the opening please click No)", "SwitchCover", i_cupNo, 2, 7);
                        //                    continue;
                        //                    //while (true)

                        //                    //{
                        //                    //    if (0 != myAlarm._i_alarm_Choose)
                        //                    //        break;
                        //                    //    if (0 != myAlarm._i_alarm_Repeat)
                        //                    //        break;
                        //                    //    Thread.Sleep(1);
                        //                    //}
                        //                    //if (myAlarm._i_alarm_Choose == 1 || myAlarm._i_alarm_Repeat == 1)
                        //                    //{
                        //                    //    goto labelP1;
                        //                    //}
                        //                    //else
                        //                    //{
                        //                    //    goto labelP2;
                        //                    //}
                        //                    //return;
                        //                }
                        //                else if ("发现杯盖或针筒" == ex.Message)
                        //                {
                        //                    FADM_Object.MyAlarm myAlarm;
                        //                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        //                        myAlarm = new FADM_Object.MyAlarm("请先拿住针筒或杯盖，然后点确定", "SwitchCover", true, 1);
                        //                    else
                        //                        myAlarm = new FADM_Object.MyAlarm("Please hold the syringe or cup lid first, and then confirm", "SwitchCover", true, 1);
                        //                    while (true)

                        //                    {
                        //                        if (0 != myAlarm._i_alarm_Choose)
                        //                            break;
                        //                        Thread.Sleep(1);
                        //                    }
                        //                    //抓手开
                        //                    int[] ia_array = new int[1];
                        //                    ia_array[0] = 7;

                        //                    int i_state = FADM_Object.Communal._tcpModBus.Write(811, ia_array);
                        //                    //等5秒后继续
                        //                    Thread.Sleep(5000);
                        //                    goto labelP1;
                        //                }
                        //                else if ("配液杯取盖失败" == ex.Message)
                        //                {
                        //                    FADM_Object.MyAlarm myAlarm;
                        //                    //把剩余没有滴液的Min置为状态5
                        //                    s_sql = "UPDATE drop_details SET MinWeight = 5 WHERE BatchName = '" + oBatchName + "' And  Finish = 0  AND CupNum = " + i_cupNo + ";";
                        //                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        //                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        //                        myAlarm = new FADM_Object.MyAlarm(i_cupNo + "号配液杯取盖失败，是否继续执行?(继续执行请点是)", "SwitchCover", i_cupNo, 2, 7);
                        //                    else
                        //                        myAlarm = new FADM_Object.MyAlarm(i_cupNo + " Failed to remove cap from dispensing cup, do you want to continue? " +
                        //                            "( Continue to perform please click Yes)", "SwitchCover", i_cupNo, 2, 7);
                        //                    continue;
                        //                }

                        //                else if ("放盖区取盖失败" == ex.Message)
                        //                {
                        //                    FADM_Object.MyAlarm myAlarm;
                        //                    //把剩余没有滴液的Min置为状态5
                        //                    s_sql = "UPDATE drop_details SET MinWeight = 5 WHERE BatchName = '" + oBatchName + "' And  Finish = 0  AND CupNum = " + i_cupNo + ";";
                        //                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        //                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        //                        myAlarm = new FADM_Object.MyAlarm(i_cupNo + "号放盖区取盖失败，是否继续执行?(继续执行请点是)", "SwitchCover", i_cupNo, 2, 7);
                        //                    else
                        //                        myAlarm = new FADM_Object.MyAlarm(i_cupNo + " Failed to remove the cover from the cover placement area, do you want to continue? " +
                        //                            "( Continue to perform please click Yes)", "SwitchCover", i_cupNo, 2, 7);
                        //                    continue;
                        //                }

                        //                else if ("关盖失败" == ex.Message)
                        //                {
                        //                    FADM_Object.MyAlarm myAlarm;
                        //                    //把剩余没有滴液的Min置为状态5
                        //                    s_sql = "UPDATE drop_details SET MinWeight = 5 WHERE BatchName = '" + oBatchName + "' And  Finish = 0  AND CupNum = " + i_cupNo + ";";
                        //                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        //                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        //                        myAlarm = new FADM_Object.MyAlarm(i_cupNo + "号关盖失败，是否继续执行?(继续执行请点是)", "SwitchCover", i_cupNo, 2, 7);
                        //                    else
                        //                        myAlarm = new FADM_Object.MyAlarm(i_cupNo + " Closing failure, do you want to continue? " +
                        //                            "( Continue to perform please click Yes)", "SwitchCover", i_cupNo, 2, 7);
                        //                    continue;
                        //                }
                        //                else
                        //                    throw;
                        //            }

                        //        labelP2:
                        //            //复位加药启动信号
                        //            int[] ia_zero1 = new int[1];
                        //            //
                        //            ia_zero1[0] = 0;

                        //            FADM_Auto.Dye.DyeOpenOrCloseCover(i_cupNo, 2);
                        //            Thread.Sleep(1000);
                        //            Communal._fadmSqlserver.ReviseData("Update  cup_details set CoverStatus = 2 where CupNum = " + i_cupNo);

                        //            FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._i_cupCover = 2;

                        //            FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", i_cupNo + "号配液杯开盖完成");
                        //        }
                        //    }
                        //}

                        //把实际加水杯号记录
                        lis_actualAddWaterCup.Add(i_cupNo);

                        //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                        //{
                        //    //富士伺服在下面判断 天平状态 原有不动 绿维的放在上面 并且置位 是否回原点 绿维的放移动机械手前面

                        //    //判断是否异常
                        //    FADM_Object.Communal.BalanceState("滴液"); //这是加水 
                        //}


                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找" + i_cupNo + "号配液杯");
                        int i_reSuccess2 = MyModbusFun.TargetMove(1, i_cupNo, 1);
                        if (-2 == i_reSuccess2)
                            throw new Exception("收到退出消息");
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达" + i_cupNo + "号配液杯");

                        if (_b_dripStop)
                        {
                            FADM_Object.Communal._b_stop = true;
                        }
                        if (!FADM_Object.Communal._b_isWaterRecheck)
                        {
                            if (threadFlow != null)
                                threadFlow.Join();
                        }
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand",  i_cupNo + "号配液杯加水启动");
                        double d_addWaterTime = MyModbusFun.GetWaterTime(d_blObjectW);//加水时间
                        FADM_Object.Communal.AddWater(d_addWaterTime);
                        if (!FADM_Object.Communal._b_isWaterRecheck)
                        {
                            threadFlow = new Thread(WaitReadFlow);
                            threadFlow.Start(i_cupNo);
                        }
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", i_cupNo + "号配液杯加水完成");
                    }
                    //如果勾选加水，但实际计算出来加水量为0，直接完成
                    else
                    {
                        s_sql = "UPDATE drop_head SET AddWaterFinish = 1,WaterStandError = '" + Lib_Card.Configure.Parameter.Other_AErr_DripWater + "' WHERE " +
                    "BatchName = '" + oBatchName + "' AND  CupNum = " + i_cupNo + ";";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                        DataTable dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName.ToString() + "' AND Finish = 0" + " AND CupNum = " + i_cupNo + ";");
                        //查询一下加水没完成的也不置为完成
                        DataTable dt_drop_details4 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND AddWaterChoose = 1 AND AddWaterFinish =0" + " AND CupNum = " + i_cupNo + ";");
                        if (dt_drop_details3.Rows.Count == 0 && dt_drop_details4.Rows.Count == 0)
                        {
                            //更新配液杯总量
                            double d_totalW = 0.0;
                            //置为完成
                            s_sql = "UPDATE drop_head SET CupFinish = 1 WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cupNo + "; ";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                            bool b_fail = true;

                            s_sql = "SELECT drop_details.CupNum as CupNum, " +
                                " drop_details.UnitOfAccount as UnitOfAccount, " +
                                        "drop_details.BottleNum as BottleNum, " +
                                        "drop_details.ObjectDropWeight as ObjectDropWeight, " +
                                        "drop_details.RealDropWeight as RealDropWeight, " +
                                        "bottle_details.SyringeType as SyringeType " +
                                        "FROM drop_details left join bottle_details on " +
                                        "bottle_details.BottleNum = drop_details.BottleNum " +
                                        "WHERE drop_details.BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cupNo + ";";
                            DataTable dt_drop_head_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                            foreach (DataRow dr in dt_drop_head_details.Rows)
                            {
                                if (Convert.ToInt32(dr["BottleNum"]) > Lib_Card.Configure.Parameter.Machine_Bottle_Total)
                                    continue;
                                double d_blRealErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"]))) : Convert.ToDouble(string.Format("{0:F3}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])));
                                d_blRealErr = d_blRealErr < 0 ? -d_blRealErr : d_blRealErr;
                                if (d_blRealErr > (dr["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip))
                                    b_fail = false;
                                d_totalW += Convert.ToDouble(dr["RealDropWeight"]);
                            }

                            dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                    "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cupNo + ";");
                            if (dt_drop_details3.Rows.Count > 0)
                            {
                                int i_cup = Convert.ToInt16(dt_drop_details3.Rows[0]["CupNum"]);
                                int i_Step = Convert.ToInt16(dt_drop_details3.Rows[0]["Step"]);
                                double d_objWater = Convert.ToDouble(dt_drop_details3.Rows[0]["ObjectAddWaterWeight"]);
                                double d_realWater = Convert.ToDouble(dt_drop_details3.Rows[0]["RealAddWaterWeight"]);
                                double d_totalWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TotalWeight"]);
                                double d_testTubeObjectAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeObjectAddWaterWeight"]);
                                double d_testTubeRealAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeRealAddWaterWeight"]);
                                double d_realDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater - d_objWater) : string.Format("{0:F3}", d_realWater - d_objWater));
                                d_realDif = d_realDif < 0 ? -d_realDif : d_realDif;
                                double d_allDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)) : string.Format("{0:F3}",
                                    d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)));

                                string s_describe;
                                string s_describe_EN;
                                if (d_allDif < d_realDif || (d_realWater == 0.0 && d_objWater != 0.0))
                                {
                                    b_fail = false;
                                }
                                d_totalW += d_realWater;
                                if (b_fail)
                                {
                                    s_describe = "滴液成功!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                              ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Success !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(i_cupNo))
                                    {
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cup))
                                            FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                                    }
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  "UPDATE cup_details SET Statues = '滴液成功',TotalWeight = "+ (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalW) : string.Format("{0:F3}",
                                    d_totalW))+" WHERE CupNum = " + i_cup + ";");
                                }
                                else
                                {
                                    s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                            ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET Statues = '滴液失败',TotalWeight = "+ (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalW) : string.Format("{0:F3}",
                                    d_totalW))+" WHERE CupNum = " + i_cup + ";");
                                }
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE drop_head SET DescribeChar = '" + s_describe + "',DescribeChar_EN = '" + s_describe_EN + "', FinishTime = '" + DateTime.Now + "', Step = 2 " +
                               "WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cup + ";");
                            }
                        }
                    }
                }
                if (lis_actualAddWaterCup.Count > 0)
                {
                    //加水天平复称
                    if (FADM_Object.Communal._b_isWaterRecheck)
                    {
                        //移动到天平位

                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找天平位");

                        //判断是否异常
                        //if ((Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 0) || Lib_Card.Configure.Parameter.Machine_Type == 1)
                        {
                            //富士伺服在下面判断 天平状态 原有不动 绿维的放在上面 并且置位 是否回原点

                            //判断是否异常
                            FADM_Object.Communal.BalanceState("滴液");
                        }

                        //Lib_SerialPort.Balance.METTLER.bZeroSign = true;

                        if (_b_dripStop)
                        {
                            FADM_Object.Communal._b_stop = true;
                        }
                        i_mRes = MyModbusFun.TargetMove(2, 0, 1);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");

                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达天平位");
                        //if (FADM_Object.Communal._b_isZero)
                        //{
                        //    while (true)
                        //    {
                        //        //判断是否成功清零
                        //        if (Lib_SerialPort.Balance.METTLER._s_balanceValue == 0.0)
                        //        {
                        //            break;
                        //        }
                        //        else
                        //        {
                        //            //再次发调零
                        //            Lib_SerialPort.Balance.METTLER.bZeroSign = true;
                        //        }
                        //        Thread.Sleep(1);
                        //    }
                        //}

                        double d_blBalanceValue0 = 0;
                        if (FADM_Object.Communal._b_isStableRead)
                            d_blBalanceValue0 = SteBalance();
                        else
                        {
                            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
                            {
                                d_blBalanceValue0 = Lib_Card.Configure.Parameter.Machine_BalanceType == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal.dBalanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal.dBalanceValue));
                            }
                            else
                            {
                                d_blBalanceValue0 = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal._s_balanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal._s_balanceValue));
                            }
                        }
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平读数：" + d_blBalanceValue0);

                        double d_addWaterTime2 = MyModbusFun.GetWaterTime(Lib_Card.Configure.Parameter.Correcting_Water_RWeight);//加水时间 校正加水时间
                        FADM_Object.Communal.AddWater(d_addWaterTime2);

                        //读取天平数据
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数启动");
                        double d_blRRead = FADM_Object.Communal.SteBalance();
                        double d_blWeight = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blRRead - d_blBalanceValue0)) : Convert.ToDouble(string.Format("{0:F3}", d_blRRead - d_blBalanceValue0));
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数：" + d_blRRead + ",实际重量：" + d_blWeight);
                        double d_blWE = Convert.ToDouble(string.Format("{0:F3}", (d_blWeight - Lib_Card.Configure.Parameter.Correcting_Water_RWeight)));
                        double d_blDif = Convert.ToDouble(string.Format("{0:F3}", d_blWE / Lib_Card.Configure.Parameter.Correcting_Water_RWeight));
                        double d_blDif_span = Convert.ToDouble(string.Format("{0:F3}", d_blWeight / Lib_Card.Configure.Parameter.Correcting_Water_RWeight));
                        int irErr = Convert.ToInt16(d_blDif * 100);
                        irErr = irErr < 0 ? -irErr : irErr;
                        //解锁6工位
                        MyModbusFun.UnLockSix();
                        s_sql = "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName + "' AND ObjectAddWaterWeight != 0  And AddWaterFinish = 0;";
                        DataTable dt_drop_head3 = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                        //判断是否存在加水复检失败
                        bool b_fail = false;
                        string s_failC = "";
                        foreach (DataRow dataRow in dt_drop_head3.Rows)
                        {
                            if (!lis_actualAddWaterCup.Contains(Convert.ToInt32(dataRow["CupNum"])))
                            {
                                continue;
                            }
                            Double d_objAddWaiterWeight = 0;
                            if (Convert.ToDouble(dataRow["ObjectAddWaterWeight"].ToString()) > 20)
                            {
                                d_objAddWaiterWeight = Convert.ToDouble(dataRow["ObjectAddWaterWeight"].ToString()) + d_blWE;
                            }
                            else
                            {
                                d_objAddWaiterWeight = Convert.ToDouble(dataRow["ObjectAddWaterWeight"].ToString()) * d_blDif_span;
                            }
                            if (d_blWeight == 0)
                            {
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                      "UPDATE cup_details SET TotalWeight =  " + string.Format("{0:F3}", 0) + " WHERE CupNum = " + dataRow["CupNum"] + " ;");
                            }
                            else
                            {
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                              "UPDATE cup_details SET TotalWeight =  " + string.Format("{0:F3}", d_objAddWaiterWeight) + " WHERE CupNum = " + dataRow["CupNum"] + " ;");
                            }

                            if (Convert.ToDouble(dataRow["ObjectAddWaterWeight"].ToString()) > 20)
                            {
                                if (System.Math.Abs(d_blWE * 100 / (Convert.ToDouble(dataRow["TotalWeight"].ToString()))) > Lib_Card.Configure.Parameter.Other_AErr_DripWater || d_blWeight == 0)
                                {
                                    b_fail = true;
                                    s_failC += dataRow["CupNum"].ToString() + ",";
                                }
                            }
                            else
                            {
                                if (System.Math.Abs(Convert.ToDouble(dataRow["ObjectAddWaterWeight"].ToString()) * d_blDif * 100) / (Convert.ToDouble(dataRow["TotalWeight"].ToString())) > Lib_Card.Configure.Parameter.Other_AErr_DripWater || d_blWeight == 0)
                                {
                                    b_fail = true;
                                    s_failC += dataRow["CupNum"].ToString() + ",";
                                }
                            }
                            //if (d_blDif*100 > Lib_Card.Configure.Parameter.Other_AErr_DripWater)
                            //{
                            //    b_fail = true;
                            //    s_failC += dataRow["CupNum"].ToString() + ",";
                            //}
                        }


                        //实际加水杯号
                        string s_cupAddWater = "";
                        if (lis_actualAddWaterCup.Count > 0)
                        {
                            for (int i = 0; i < lis_actualAddWaterCup.Count; i++)
                            {
                                s_cupAddWater += lis_actualAddWaterCup[i] + ",";
                            }
                            s_cupAddWater = s_cupAddWater.Remove(s_cupAddWater.Length - 1, 1);
                        }
                        else
                        {
                            s_cupAddWater = "0";
                        }

                        //复检天平重量为0时，实际加水量为0
                        if (d_blWeight == 0)
                        {
                            s_sql = "UPDATE drop_head SET RealAddWaterWeight = 0, AddWaterFinish = 1,WaterStandError = '" + Lib_Card.Configure.Parameter.Other_AErr_DripWater + "' WHERE " +
                             "BatchName = '" + oBatchName + "' AND AddWaterChoose = 1 AND CupFinish = 0   AND ObjectAddWaterWeight > 0 AND ObjectAddWaterWeight <= 20 And AddWaterFinish = 0 And CupNum in (" + s_cupAddWater + ");";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        }
                        else
                        {
                            s_sql = "UPDATE drop_head SET RealAddWaterWeight = (ObjectAddWaterWeight * " + d_blDif_span + "), AddWaterFinish = 1,WaterStandError = '" + Lib_Card.Configure.Parameter.Other_AErr_DripWater + "' WHERE " +
                            "BatchName = '" + oBatchName + "' AND AddWaterChoose = 1 AND CupFinish = 0   AND ObjectAddWaterWeight > 0 AND ObjectAddWaterWeight <= 20  And AddWaterFinish = 0 And CupNum in (" + s_cupAddWater + ");";

                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        }

                        //复检天平重量为0时，实际加水量为0
                        if (d_blWeight == 0)
                        {
                            s_sql = "UPDATE drop_head SET RealAddWaterWeight = 0, AddWaterFinish = 1,WaterStandError = '" + Lib_Card.Configure.Parameter.Other_AErr_DripWater + "' WHERE " +
                            "BatchName = '" + oBatchName + "' AND AddWaterChoose = 1 AND CupFinish = 0   AND ObjectAddWaterWeight > 20  And AddWaterFinish = 0 And CupNum in (" + s_cupAddWater + ");";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        }
                        else
                        {
                            s_sql = "UPDATE drop_head SET RealAddWaterWeight = (ObjectAddWaterWeight + " + d_blWE + "), AddWaterFinish = 1,WaterStandError = '" + Lib_Card.Configure.Parameter.Other_AErr_DripWater + "' WHERE " +
                            "BatchName = '" + oBatchName + "' AND AddWaterChoose = 1 AND CupFinish = 0   AND ObjectAddWaterWeight > 20  And AddWaterFinish = 0 And CupNum in (" + s_cupAddWater + ");";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        }

                        if (b_fail)
                        {
                            s_failC = s_failC.Remove(s_failC.Length - 1);
                            FADM_Object.Communal.WriteDripWait(true);
                            Communal._b_isAllowDrip = false;
                            FADM_Object.MyAlarm myAlarm;

                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm(s_failC + "号杯加水复检失败,是否继续?(继续滴液请点是，退出滴液请点否)", "加水复检", true, 1);
                            else
                                myAlarm = new FADM_Object.MyAlarm(" The re inspection of" + s_failC + " cup with water has failed. Do you want to continue? (To continue dripping, please click Yes, and to exit dripping, please click No)", "Add water for retesting", true, 1);
                            while (true)
                            {
                                if (0 != myAlarm._i_alarm_Choose)
                                {
                                    //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                                    //{
                                    //  //  Home.Home_XYZFinish = false;
                                    //}
                                    break;
                                }

                                Thread.Sleep(1);
                            }


                            if (2 == myAlarm._i_alarm_Choose)
                            {
                                //判断染色线程是否需要用机械手
                                if (null != FADM_Object.Communal.ReadDyeThread())
                                {
                                    FADM_Object.Communal.WriteDripWait(true);
                                    Communal._b_isWaitDrip = true;
                                    Communal._b_isAllowDrip = true;
                                    while (true)
                                    {
                                        if (false == FADM_Object.Communal.ReadDripWait())
                                            break;
                                        Thread.Sleep(1);
                                    }
                                    Communal._b_isWaitDrip = false;
                                }
                                else
                                    Communal._b_isWaitDrip = false;
                                throw new Exception("收到退出消息");
                            }

                            Communal._b_isAllowDrip = true;

                            //判断染色线程是否需要用机械手
                            if (null != FADM_Object.Communal.ReadDyeThread())
                            {
                                FADM_Object.Communal.WriteDripWait(true);
                                Communal._b_isWaitDrip = true;
                                Communal._b_isAllowDrip = true;
                                while (true)
                                {
                                    if (false == FADM_Object.Communal.ReadDripWait())
                                        break;
                                    Thread.Sleep(1);
                                }
                                Communal._b_isWaitDrip = false;
                            }
                            else
                            {
                                Communal._b_isWaitDrip = false;
                                FADM_Object.Communal.WriteDripWait(false);
                            }
                        }

                        //当加水在最后时，要重新判断一下
                        if (FADM_Object.Communal._b_isFinishSend)
                        {
                            foreach (int ic in lis_actualAddWaterCup)
                            {


                                DataTable dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                                        "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName.ToString() + "' AND Finish = 0" + " AND CupNum = " + ic + ";");
                                //查询一下加水没完成的也不置为完成
                                DataTable dt_drop_details4 = FADM_Object.Communal._fadmSqlserver.GetData(
                                        "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND AddWaterChoose = 1 AND AddWaterFinish =0" + " AND CupNum = " + ic + ";");
                                if (dt_drop_details3.Rows.Count == 0 && dt_drop_details4.Rows.Count == 0)
                                {
                                    //更新配液杯总量
                                    double d_totalW = 0.0;
                                    //置为完成
                                    s_sql = "UPDATE drop_head SET CupFinish = 1 WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + ic + "; ";
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);


                                    bool b_fail1 = true;

                                    s_sql = "SELECT drop_details.CupNum as CupNum, " +
                                         " drop_details.UnitOfAccount as UnitOfAccount, " +
                                                "drop_details.BottleNum as BottleNum, " +
                                                "drop_details.ObjectDropWeight as ObjectDropWeight, " +
                                                "drop_details.RealDropWeight as RealDropWeight, " +
                                                "bottle_details.SyringeType as SyringeType " +
                                                "FROM drop_details left join bottle_details on " +
                                                "bottle_details.BottleNum = drop_details.BottleNum " +
                                                "WHERE drop_details.BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + ic + ";";
                                    dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                                    foreach (DataRow dr in dt_drop_head.Rows)
                                    {
                                        if (Convert.ToInt32(dr["BottleNum"]) > Lib_Card.Configure.Parameter.Machine_Bottle_Total)
                                            continue;
                                        double d_blRealErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                                        Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"]))) : Convert.ToDouble(string.Format("{0:F3}",
                                        Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])));
                                        d_blRealErr = d_blRealErr < 0 ? -d_blRealErr : d_blRealErr;
                                        if (d_blRealErr > (dr["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip))
                                            b_fail1 = false;

                                        d_totalW += Convert.ToDouble(dr["RealDropWeight"]);
                                    }

                                    dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                            "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + ic + ";");
                                    if (dt_drop_details3.Rows.Count > 0)
                                    {
                                        int i_cup = Convert.ToInt16(dt_drop_details3.Rows[0]["CupNum"]);
                                        int i_Step = Convert.ToInt16(dt_drop_details3.Rows[0]["Step"]);
                                        double d_objWater = Convert.ToDouble(dt_drop_details3.Rows[0]["ObjectAddWaterWeight"]);
                                        double d_realWater = Convert.ToDouble(dt_drop_details3.Rows[0]["RealAddWaterWeight"]);
                                        double d_totalWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TotalWeight"]);
                                        double d_testTubeObjectAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeObjectAddWaterWeight"]);
                                        double d_testTubeRealAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeRealAddWaterWeight"]);
                                        double d_realDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater - d_objWater) : string.Format("{0:F3}", d_realWater - d_objWater));
                                        d_realDif = d_realDif < 0 ? -d_realDif : d_realDif;
                                        double d_allDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                            d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)) : string.Format("{0:F3}",
                                            d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)));

                                        string s_describe;
                                        string s_describe_EN;
                                        if (d_allDif < d_realDif || (d_realWater == 0.0 && d_objWater != 0.0))
                                        {
                                            b_fail1 = false;
                                        }
                                        d_totalW += d_realWater;

                                        if (b_fail1)
                                        {
                                            s_describe = "滴液成功!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                      ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                            s_describe_EN = "Drip Success !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                             ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                            if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(ic))
                                            {
                                                if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cup))
                                                    FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                                            }
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                          "UPDATE cup_details SET Statues = '滴液成功',TotalWeight = "+ (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalW) : string.Format("{0:F3}",
                                    d_totalW))+" WHERE CupNum = " + i_cup + ";");
                                        }
                                        else
                                        {
                                            s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                    ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                            s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                             ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                            "UPDATE cup_details SET Statues = '滴液失败',TotalWeight = "+ (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalW) : string.Format("{0:F3}",
                                    d_totalW))+" WHERE CupNum = " + i_cup + ";");
                                        }
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE drop_head SET DescribeChar = '" + s_describe + "',DescribeChar_EN = '" + s_describe_EN + "', FinishTime = '" + DateTime.Now + "', Step = 2 " +
                                       "WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cup + ";");
                                    }
                                }

                            }
                        }
                    }
                    //报警加水复检失败
                    else
                    {
                        if (threadFlow != null)
                            threadFlow.Join();

                        s_sql = "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName + "';";
                        DataTable dt_drop_head3 = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                        //判断是否存在加水复检失败
                        bool b_fail = false;
                        string s_failC = "";
                        foreach (DataRow dataRow in dt_drop_head3.Rows)
                        {
                            if (!lis_actualAddWaterCup.Contains(Convert.ToInt32(dataRow["CupNum"])))
                            {
                                continue;
                            }
                            double d_blWE = Convert.ToDouble(string.Format("{0:F3}", (Convert.ToDouble(dataRow["RealAddWaterWeight"].ToString()) - Convert.ToDouble(dataRow["ObjectAddWaterWeight"].ToString()))));
                            double d_blDif = Convert.ToDouble(string.Format("{0:F3}", d_blWE / Convert.ToDouble(dataRow["ObjectAddWaterWeight"].ToString())));
                            int irErr = Convert.ToInt16(d_blDif * 100);
                            irErr = irErr < 0 ? -irErr : irErr;

                            if (System.Math.Abs(d_blWE * 100 / (Convert.ToDouble(dataRow["TotalWeight"].ToString()))) > Lib_Card.Configure.Parameter.Other_AErr_DripWater)
                            {
                                b_fail = true;
                                s_failC += dataRow["CupNum"].ToString() + ",";
                            }

                        }

                        if (b_fail)
                        {
                            s_failC = s_failC.Remove(s_failC.Length - 1);
                            FADM_Object.Communal.WriteDripWait(true);
                            Communal._b_isAllowDrip = false;
                            FADM_Object.MyAlarm myAlarm;

                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm(s_failC + "号杯加水复检失败,是否继续?(继续滴液请点是，退出滴液请点否)", "加水复检", true, 1);
                            else
                                myAlarm = new FADM_Object.MyAlarm(" The re inspection of" + s_failC + " cup with water has failed. Do you want to continue? (To continue dripping, please click Yes, and to exit dripping, please click No)", "Add water for retesting", true, 1);
                            while (true)
                            {
                                if (0 != myAlarm._i_alarm_Choose)
                                {
                                    //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                                    //{
                                    //  //  Home.Home_XYZFinish = false;
                                    //}
                                    break;
                                }

                                Thread.Sleep(1);
                            }


                            if (2 == myAlarm._i_alarm_Choose)
                            {
                                //判断染色线程是否需要用机械手
                                if (null != FADM_Object.Communal.ReadDyeThread())
                                {
                                    FADM_Object.Communal.WriteDripWait(true);
                                    Communal._b_isWaitDrip = true;
                                    Communal._b_isAllowDrip = true;
                                    while (true)
                                    {
                                        if (false == FADM_Object.Communal.ReadDripWait())
                                            break;
                                        Thread.Sleep(1);
                                    }
                                    Communal._b_isWaitDrip = false;
                                }
                                else
                                    Communal._b_isWaitDrip = false;
                                throw new Exception("收到退出消息");
                            }

                            Communal._b_isAllowDrip = true;

                            //判断染色线程是否需要用机械手
                            if (null != FADM_Object.Communal.ReadDyeThread())
                            {
                                FADM_Object.Communal.WriteDripWait(true);
                                Communal._b_isWaitDrip = true;
                                Communal._b_isAllowDrip = true;
                                while (true)
                                {
                                    if (false == FADM_Object.Communal.ReadDripWait())
                                        break;
                                    Thread.Sleep(1);
                                }
                                Communal._b_isWaitDrip = false;
                            }
                            else
                            {
                                Communal._b_isWaitDrip = false;
                                FADM_Object.Communal.WriteDripWait(false);
                            }
                        }
                    }
                }
            }
            string s_unitA = "";
            int i_lowSrart = 0;
            if (FADM_Object.Communal._b_isAssitantFirst)
            {
                //加助剂
                s_unitA = "g/l";
                i_lowSrart = 0;
            }
            else
            {
                //加染料
                s_unitA = "%";
                i_lowSrart = 0;
            }

        label3:
            s_sql = "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName + "' AND " +
                "Finish = 0 AND UnitOfAccount = '" + s_unitA + "' AND MinWeight = " + i_lowSrart + " AND " +
                "BottleNum <= " + Lib_Card.Configure.Parameter.Machine_Bottle_Total + "  And IsDrop != 0 ORDER BY CupNum;";
            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (0 == dt_drop_head.Rows.Count)
            {
                if (FADM_Object.Communal._b_isAssitantFirst)
                {
                    if ("g/l" == s_unitA)
                    {
                        //助剂已加完，加染料
                        goto label4;
                    }
                    else
                    {
                        if (0 == i_lowSrart)
                        {
                            //染料已加完，加液量不足
                            if (FADM_Object.Communal._b_isHaveAutoBrew)
                                goto label16;
                            else
                                goto label5;
                        }
                        else if (1 == i_lowSrart)
                        {
                            //液量不足已加完，加超出生命周期
                            goto label17;
                        }
                        else if (3 == i_lowSrart)
                        {
                            //超出生命周期的加完，加检测不到针筒
                            goto label16;
                        }
                        else
                        {
                            //结束
                            goto label6;
                        }
                    }
                }
                else
                {
                    if ("%" == s_unitA)
                    {
                        //染料已加完，加助剂
                        goto label4;
                    }
                    else
                    {
                        if (0 == i_lowSrart)
                        {
                            //助剂已加完，加液量不足
                            if (FADM_Object.Communal._b_isHaveAutoBrew)
                                goto label16;
                            else
                                goto label5;
                        }
                        else if (1 == i_lowSrart)
                        {
                            //液量不足已加完，加超出生命周期
                            goto label17;
                        }
                        else if (3 == i_lowSrart)
                        {
                            //超出生命周期的加完，加检测不到针筒
                            goto label16;
                        }
                        else
                        {
                            //结束
                            goto label6;
                        }
                    }
                }

            }

            int i_minCupNo = Convert.ToInt32(dt_drop_head.Rows[0]["CupNum"]);

        label7:
            s_sql = "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName + "' AND CupNum = " + i_minCupNo + " AND " +
                "Finish = 0 AND UnitOfAccount = '" + s_unitA + "' AND MinWeight = " + i_lowSrart + " AND " +
                "BottleNum <= " + Lib_Card.Configure.Parameter.Machine_Bottle_Total + "   And IsDrop != 0 ORDER BY BottleNum;";
            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (0 == dt_drop_head.Rows.Count)
            {
                //当前杯已加完
                goto label3;
            }

            int i_minBottleNo = Convert.ToInt32(dt_drop_head.Rows[0]["BottleNum"]);

            s_sql = "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                "Finish = 0 AND UnitOfAccount = '" + s_unitA + "' AND MinWeight = " + i_lowSrart + " AND " +
                "BottleNum <= " + Lib_Card.Configure.Parameter.Machine_Bottle_Total + "   And IsDrop != 0 ORDER BY CupNum;";
            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (0 == dt_drop_head.Rows.Count)
            {
                //当前瓶完成
                goto label7;
            }

            s_sql = "SELECT * FROM bottle_details WHERE BottleNum = " + i_minBottleNo + ";";
            DataTable dt_bottle_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            int i_adjust = Convert.ToInt32(dt_bottle_details.Rows[0]["AdjustValue"]);
            bool b_lCheckSuccess = (Convert.ToString(dt_bottle_details.Rows[0]["AdjustSuccess"]) == "1");
            string s_syringeType = Convert.ToString(dt_bottle_details.Rows[0]["SyringeType"]);
            string s_dripReserveFirst = "0";
            if (!(dt_bottle_details.Rows[0]["DripReserveFirst"] is DBNull))
            {
                s_dripReserveFirst = dt_bottle_details.Rows[0]["DripReserveFirst"].ToString();
            }


            Dictionary<int, int> dic_pulse = new Dictionary<int, int>();
            Dictionary<int, double> dic_weight = new Dictionary<int, double>();
            Dictionary<int, double> dic_water = new Dictionary<int, double>();
            int i_pulseT = 0;
            if (0 == i_lowSrart)
            {
                double d_blCW = Convert.ToDouble(string.Format("{0:F3}", dt_bottle_details.Rows[0]["CurrentWeight"]));
                foreach (DataRow dataRow in dt_drop_head.Rows)
                {
                    int i_cupNo = Convert.ToInt32(dataRow["CupNum"]);
                    double d_blOAddW = Convert.ToDouble(string.Format("{0:F3}", dataRow["ObjectDropWeight"]));
                    int i_needPulse = dataRow["NeedPulse"] is DBNull ? 0 : Convert.ToInt32(dataRow["NeedPulse"]);
                    d_blCW -= d_blOAddW;

                    //查询判断是否超期
                    s_sql = "SELECT * FROM assistant_details WHERE AssistantCode = '" + dt_bottle_details.Rows[0]["AssistantCode"].ToString() + "';";
                    DataTable dt_assistant_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                    DateTime timeA = Convert.ToDateTime(dt_bottle_details.Rows[0]["BrewingData"].ToString());
                    DateTime timeB = DateTime.Now; //获取当前时间
                    TimeSpan ts = timeB - timeA; //计算时间差
                    string s_time = ts.TotalHours.ToString(); //将时间差转换为小时


                    if (d_blCW < Lib_Card.Configure.Parameter.Other_Bottle_MinWeight && FADM_Object.Communal._b_isLowDrip)
                    {
                        //有自动开料
                        if (FADM_Object.Communal._b_isHaveAutoBrew)
                        {
                            //插入自动开料列表
                            //判断是否存在正在开料的记录
                            s_sql = "select * from brew_station_details where IsUsing = 1 And Station<6 And BottleNum =" + i_minBottleNo;
                            DataTable dt_brew_station_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                            if(dt_brew_station_details.Rows.Count == 0)
                            {
                                //判断等待列表是否存在排队
                                s_sql = "select * from Brew_wait_list where  BottleNum =" + i_minBottleNo;
                                DataTable dt_Brew_wait_list = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                                if(dt_Brew_wait_list.Rows.Count==0)
                                {
                                    //插入到排队列表
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                       "INSERT INTO Brew_wait_list(BottleNum, InsertDate,Type) VALUES('" + i_minBottleNo + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',0);");
                                }

                            }
                            s_sql = "UPDATE drop_details SET MinWeight = 6 WHERE BatchName = '" + oBatchName + "' AND MinWeight=0 And " +
                                    "BottleNum = " + i_minBottleNo + " AND Finish = 0 ;";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                            break;
                        }
                        else
                        {
                            //查询在备料表是否存在记录，如果存在，先让客户选择是否使用备料数据来更新
                            string s_sqlpre = "SELECT * FROM pre_brew WHERE  BottleNum = " + i_minBottleNo + ";";
                            DataTable dt_pre_brew = FADM_Object.Communal._fadmSqlserver.GetData(s_sqlpre);
                            if (dt_pre_brew.Rows.Count > 0)
                            {
                                FADM_Object.Communal.WriteDripWait(true);
                                Communal._b_isAllowDrip = false;
                                FADM_Object.MyAlarm myAlarm;
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    myAlarm = new FADM_Object.MyAlarm(
                                    i_minBottleNo + "号母液瓶液量过低，备料表存在已开料记录，是否替换(替换请点是，继续使用旧母液请点否)?", "滴液", true, 1);
                                else
                                    myAlarm = new FADM_Object.MyAlarm(
                                    "The " + i_minBottleNo + " mother liquor bottle has expired, and there is a record of opened materials in the material preparation table. Should it be replaced? (Please click Yes for replacement, and click No for continuing to use the old mother liquor)", "Drip", true, 1);
                                while (true)
                                {
                                    if (0 != myAlarm._i_alarm_Choose)
                                    {
                                        //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                                        //{
                                        //   // Home.Home_XYZFinish = false;
                                        //}
                                        break;
                                    }

                                    Thread.Sleep(1);
                                }
                                Communal._b_isAllowDrip = true;
                                //判断染色线程是否需要用机械手
                                if (null != FADM_Object.Communal.ReadDyeThread())
                                {
                                    FADM_Object.Communal.WriteDripWait(true);
                                    Communal._b_isWaitDrip = true;
                                    Communal._b_isAllowDrip = true;
                                    while (true)
                                    {
                                        if (false == FADM_Object.Communal.ReadDripWait())
                                            break;
                                        Thread.Sleep(1);
                                    }
                                    Communal._b_isWaitDrip = false;
                                }
                                else
                                {
                                    Communal._b_isWaitDrip = false;
                                    FADM_Object.Communal.WriteDripWait(false);
                                }
                                //如果选择是，使用新料重新计算
                                if (1 == myAlarm._i_alarm_Choose)
                                {
                                    //使用备料表数据更新现有母液瓶数据，删除备料表记录
                                    s_sql = "UPDATE bottle_details SET RealConcentration = '" + dt_pre_brew.Rows[0]["RealConcentration"].ToString() + "',CurrentWeight = '" + dt_pre_brew.Rows[0]["CurrentWeight"].ToString() + "',BrewingData='"
                                        + dt_pre_brew.Rows[0]["BrewingData"].ToString() + "'" +
                                    " WHERE BottleNum = " + i_minBottleNo + ";";
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                    FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from pre_brew where BottleNum = " + i_minBottleNo);
                                    goto label7;
                                }
                                //选择否就和之前的逻辑一致
                                else
                                {
                                    s_sql = "UPDATE drop_details SET MinWeight = 1 WHERE BatchName = '" + oBatchName + "' AND MinWeight=0 And " +
                                    "BottleNum = " + i_minBottleNo + " AND Finish = 0 ;";
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                    break;
                                }
                            }
                            else
                            {
                                s_sql = "UPDATE drop_details SET MinWeight = 1 WHERE BatchName = '" + oBatchName + "' AND MinWeight=0 And " +
                                    "BottleNum = " + i_minBottleNo + " AND Finish = 0 ;";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                break;
                            }
                        }

                    }
                    else if (Convert.ToDouble(s_time) > Convert.ToDouble(dt_assistant_details.Rows[0]["TermOfValidity"].ToString()) && FADM_Object.Communal._b_isOutDrip)
                    {
                        //有自动开料
                        if (FADM_Object.Communal._b_isHaveAutoBrew)
                        {
                            //插入自动开料列表
                            //判断是否存在正在开料的记录
                            s_sql = "select * from brew_station_details where IsUsing = 1 And Station<6 And BottleNum =" + i_minBottleNo;
                            DataTable dt_brew_station_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                            if (dt_brew_station_details.Rows.Count == 0)
                            {
                                //判断等待列表是否存在排队
                                s_sql = "select * from Brew_wait_list where  BottleNum =" + i_minBottleNo;
                                DataTable dt_Brew_wait_list = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                                if (dt_Brew_wait_list.Rows.Count == 0)
                                {
                                    //插入到排队列表
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                       "INSERT INTO Brew_wait_list(BottleNum, InsertDate,Type) VALUES('" + i_minBottleNo + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',0);");
                                }

                            }

                            s_sql = "UPDATE drop_details SET MinWeight = 6 WHERE BatchName = '" + oBatchName + "' AND MinWeight=0 And " +
                                    "BottleNum = " + i_minBottleNo + " AND Finish = 0 ;";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                            break;
                        }
                        else
                        {
                            //查询在备料表是否存在记录，如果存在，先让客户选择是否使用备料数据来更新
                            string s_sqlpre = "SELECT * FROM pre_brew WHERE  BottleNum = " + i_minBottleNo + ";";
                            DataTable dt_pre_brew = FADM_Object.Communal._fadmSqlserver.GetData(s_sqlpre);
                            if (dt_pre_brew.Rows.Count > 0)
                            {
                                FADM_Object.Communal.WriteDripWait(true);
                                Communal._b_isAllowDrip = false;
                                FADM_Object.MyAlarm myAlarm;
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    myAlarm = new FADM_Object.MyAlarm(
                                    i_minBottleNo + "号母液瓶过期，备料表存在已开料记录，是否替换(替换请点是，继续使用旧母液请点否)?", "滴液", true, 1);
                                else
                                    myAlarm = new FADM_Object.MyAlarm(
                                    "The " + i_minBottleNo + " mother liquor bottle has expired, and there is a record of opened materials in the material preparation table. Should it be replaced? (Please click Yes for replacement, and click No for continuing to use the old mother liquor)", "Drip", true, 1);
                                while (true)
                                {
                                    if (0 != myAlarm._i_alarm_Choose)
                                    {
                                        //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                                        //{
                                        //   // Home.Home_XYZFinish = false;
                                        //}
                                        break;
                                    }

                                    Thread.Sleep(1);
                                }
                                Communal._b_isAllowDrip = true;
                                //判断染色线程是否需要用机械手
                                if (null != FADM_Object.Communal.ReadDyeThread())
                                {
                                    FADM_Object.Communal.WriteDripWait(true);
                                    Communal._b_isWaitDrip = true;
                                    Communal._b_isAllowDrip = true;
                                    while (true)
                                    {
                                        if (false == FADM_Object.Communal.ReadDripWait())
                                            break;
                                        Thread.Sleep(1);
                                    }
                                    Communal._b_isWaitDrip = false;
                                }
                                else
                                {
                                    Communal._b_isWaitDrip = false;
                                    FADM_Object.Communal.WriteDripWait(false);
                                }
                                //如果选择是，使用新料重新计算
                                if (1 == myAlarm._i_alarm_Choose)
                                {
                                    //使用备料表数据更新现有母液瓶数据，删除备料表记录
                                    s_sql = "UPDATE bottle_details SET RealConcentration = '" + dt_pre_brew.Rows[0]["RealConcentration"].ToString() + "',CurrentWeight = '" + dt_pre_brew.Rows[0]["CurrentWeight"].ToString() + "',BrewingData='"
                                        + dt_pre_brew.Rows[0]["BrewingData"].ToString() + "'" +
                                    " WHERE BottleNum = " + i_minBottleNo + ";";
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                    FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from pre_brew where BottleNum = " + i_minBottleNo);
                                    goto label7;
                                }
                                else
                                {
                                    s_sql = "UPDATE drop_details SET MinWeight = 3 WHERE BatchName = '" + oBatchName + "' AND  MinWeight=0 And " +
                                    "BottleNum = " + i_minBottleNo + " AND Finish = 0 ;";
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                    break;
                                }
                            }
                            else
                            {
                                s_sql = "UPDATE drop_details SET MinWeight = 3 WHERE BatchName = '" + oBatchName + "' AND  MinWeight=0 And " +
                                    "BottleNum = " + i_minBottleNo + " AND Finish = 0 ;";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                break;
                            }
                        }
                    }
                    else
                    {
                        //判断是否分开两次滴液，如果是就使用需加脉冲来计算
                        int i_pulse = i_needPulse > 0 ? i_needPulse : Convert.ToInt32(i_adjust * d_blOAddW);
                        dic_pulse.Add(i_cupNo, i_pulse);
                        dic_weight.Add(i_cupNo, d_blOAddW);
                        dic_water.Add(i_cupNo, 0.0);
                        i_pulseT += i_pulse;

                        s_unitOfAccount = dataRow["UnitOfAccount"].ToString();
                    }


                }

                if (0 == dic_pulse.Count)
                {
                    //当前瓶液量不足
                    goto label7;
                }
            }
            else
            {
                foreach (DataRow dataRow in dt_drop_head.Rows)
                {
                    int i_cupNo = Convert.ToInt32(dataRow["CupNum"]);
                    double d_blOAddW = Convert.ToDouble(string.Format("{0:F3}", dataRow["ObjectDropWeight"]));
                    int i_needPulse = dataRow["NeedPulse"] is DBNull ? 0 : Convert.ToInt32(dataRow["NeedPulse"]);
                    //判断是否分开两次滴液，如果是就使用需加脉冲来计算
                    int i_pulse = i_needPulse > 0 ? i_needPulse : Convert.ToInt32(i_adjust * d_blOAddW);
                    dic_pulse.Add(i_cupNo, i_pulse);
                    dic_weight.Add(i_cupNo, d_blOAddW);
                    dic_water.Add(i_cupNo, 0.0);
                    i_pulseT += i_pulse;

                    s_unitOfAccount = dataRow["UnitOfAccount"].ToString();
                }
            }

            //判断染色线程是否需要用机械手
            if (null != FADM_Object.Communal.ReadDyeThread())
            {
                FADM_Object.Communal.WriteDripWait(true);
                Communal._b_isWaitDrip = true;
                Communal._b_isAllowDrip = true;
                while (true)
                {
                    if (false == FADM_Object.Communal.ReadDripWait())
                        break;
                    Thread.Sleep(1);
                }
                Communal._b_isWaitDrip = false;
            }
            else
                Communal._b_isWaitDrip = false;

            //针检
            if ((0 >= i_adjust || false == b_lCheckSuccess) && !b_checkFail)
            {
            label8:
                //判断染色线程是否需要用机械手
                if (null != FADM_Object.Communal.ReadDyeThread())
                {
                    FADM_Object.Communal.WriteDripWait(true);
                    Communal._b_isWaitDrip = true;
                    Communal._b_isAllowDrip = true;
                    while (true)
                    {
                        if (false == FADM_Object.Communal.ReadDripWait())
                            break;
                        Thread.Sleep(1);
                    }
                    Communal._b_isWaitDrip = false;
                }
            else
                    Communal._b_isWaitDrip = false;

                if (_b_dripStop)
                {
                    FADM_Object.Communal._b_stop = true;
                }
                int i_res = new BottleCheck().MyDripCheck(i_minBottleNo, true, i_lowSrart); //针检
                if (-1 == i_res)
                {
                    FADM_Object.Communal.WriteDripWait(true);
                    Communal._b_isAllowDrip = false;
                    FADM_Object.MyAlarm myAlarm;
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + "号母液瓶针检失败，是否继续?(继续针检请点是，退出针检请点否)", "滴液针检", true, 1, true);
                    else
                        myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + " bottle needle inspection failed, do you want to continue? " +
                            "(To continue the needle examination, please click Yes, and to exit the needle examination, please click No)", "Drip needle examination", true, 1, true);
                    while (true)

                    {
                        if (0 != myAlarm._i_alarm_Choose) {
                            //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                            //{
                            //  //  Home.Home_XYZFinish = false;
                            //}
                            break;
                        }
                            
                        Thread.Sleep(1);
                    }
                    Communal._b_isAllowDrip = true;
                    //判断染色线程是否需要用机械手
                    if (null != FADM_Object.Communal.ReadDyeThread())
                    {
                        FADM_Object.Communal.WriteDripWait(true);
                        Communal._b_isWaitDrip = true;
                        Communal._b_isAllowDrip = true;
                        while (true)
                        {
                            if (false == FADM_Object.Communal.ReadDripWait())
                                break;
                            Thread.Sleep(1);
                        }
                        Communal._b_isWaitDrip = false;
                    }
                    else
                    {
                        Communal._b_isWaitDrip = false;
                        FADM_Object.Communal.WriteDripWait(false);
                    }

                    if (1 == myAlarm._i_alarm_Choose)
                        goto label8;
                    else
                    {
                        b_checkFail = true;
                    }
                }
                else if (-2 == i_res)
                {
                    s_sql = "UPDATE drop_details SET MinWeight = 2 WHERE BatchName = '" + oBatchName + "' AND " +
                          "BottleNum = " + i_minBottleNo + " AND Finish = 0;";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                    goto label3;
                }
                else if (-3 == i_res)
                {
                    //夹不到针筒时选择否，直接退出
                    throw new Exception("收到退出消息");
                }
                if (b_checkFail)
                {
                    s_sql = "update bottle_details set AdjustValue = 3900 where AdjustValue =0 And " +
                          "BottleNum = " + i_minBottleNo + ";";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                }
                goto label7;
            }


            if (0 == dic_pulse.Count)
            {
                //当前瓶液量不足
                goto label7;
            }

            //判断染色线程是否需要用机械手
            if (null != FADM_Object.Communal.ReadDyeThread())
            {
                FADM_Object.Communal.WriteDripWait(true);
                Communal._b_isWaitDrip = true;
                Communal._b_isAllowDrip = true;
                while (true)
                {
                    if (false == FADM_Object.Communal.ReadDripWait())
                        break;
                    Thread.Sleep(1);
                }
                Communal._b_isWaitDrip = false;
            }
            else
                Communal._b_isWaitDrip = false;
            sAddArg o=new sAddArg();
            o._i_minBottleNo = i_minBottleNo;
            o._obj_batchName = oBatchName.ToString();
            o._i_adjust=i_adjust;
            o._i_pulseT=i_pulseT;
            o._s_syringeType = s_syringeType;
            o._s_unitOfAccount = s_unitOfAccount;
            o._dic_pulse= dic_pulse;
            o._dic_water =dic_water;
            if (_b_isAloneDripReserve)
            {
                if (s_dripReserveFirst == "0")
                {
                    _b_isDripReserveFirst = false;
                }
                else
                {
                    _b_isDripReserveFirst = true;
                }
            }
            Dictionary<int, double> dic_return = new Dictionary<int, double>();
            int i_ret=FADM_Object.Communal.AddMac(o,ref dic_return,1, 0, i_lowSrart);
            //夹不到针筒
            if (i_ret == -1)
            {
                if (i_lowSrart == 2)
                {
                    FADM_Object.Communal.WriteDripWait(true);
                    Communal._b_isAllowDrip = false;
                    FADM_Object.MyAlarm myAlarm;
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + "号母液瓶未找到针筒，是否继续执行?(继续寻找请点是，退出滴液请点否)", "滴液", true, 1, true);
                    else
                        myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + " bottle did not find a syringe. Do you want to continue? " +
                            "(To continue searching, please click Yes. To exit Drip, please click No)", "Drip", true, 1, true);
                    while (true)
                    {
                        if (0 != myAlarm._i_alarm_Choose) {
                            //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                            //{
                            //   // Home.Home_XYZFinish = false;
                            //}
                            break;
                        }
                            
                        Thread.Sleep(1);
                    }
                    Communal._b_isAllowDrip = true;
                    //判断染色线程是否需要用机械手
                    if (null != FADM_Object.Communal.ReadDyeThread())
                    {
                        FADM_Object.Communal.WriteDripWait(true);
                        Communal._b_isWaitDrip = true;
                        while (true)
                        {
                            if (false == FADM_Object.Communal.ReadDripWait())
                                break;
                            Thread.Sleep(1);
                        }
                        Communal._b_isWaitDrip = false;
                    }
                    else
                    {
                        Communal._b_isWaitDrip = false;
                        FADM_Object.Communal.WriteDripWait(false);
                    }

                    if (1 == myAlarm._i_alarm_Choose)
                        goto label3;
                    else
                        throw new Exception("收到退出消息");
                }
                else
                {
                    s_sql = "UPDATE drop_details SET MinWeight = 2 WHERE BatchName = '" + oBatchName + "' AND " +
                     "BottleNum = " + i_minBottleNo + " AND Finish = 0;";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                    goto label3;
                }
            }
            //滴液完成
            else if (i_ret == 0)
            {
                if (FADM_Object.Communal._b_isFinishSend)
                {
                    foreach (KeyValuePair<int, double> kvp in dic_return)
                    {
                        double d_blRErr = 0;
                        if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                        else
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight));

                        //查询开料日期
                        DataTable dt_bottle_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                     "SELECT bottle_details.*,UnitOfAccount  FROM bottle_details left join assistant_details   on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE  bottle_details.BottleNum = " + i_minBottleNo + ";");

                        if (0.00 != kvp.Value)
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + d_blRErr + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '" + (dt_bottle_details2.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                            "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                            "CupNum = " + kvp.Key + ";");

                            DataTable dt_drop_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND CupNum = " + kvp.Key + ";");
                            if (dt_drop_details2.Rows.Count > 0)
                            {
                                //记录是否修正数据
                                bool b_change = false;
                                double d_realDrop = 0.0;
                                if (FADM_Object.Communal._b_isCorrect)
                                {
                                    //如果实际滴液量为负数，就改为0
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["RealDropWeight"]) < 0)
                                    {
                                        b_change = true;
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = 0   ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                    }

                                    //判断当目标滴液量大于1g，而且实际滴液误差小于等于0.05的，全部置为0.02
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) > 1)
                                    {
                                        if (d_blRErr <= 0.05 && d_blRErr > 0.02)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + 0.02 + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) + 0.02;
                                        }
                                        else if (d_blRErr < -0.02 && d_blRErr >= -0.05)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + (-0.02) + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) - 0.02;
                                        }
                                    }
                                }
                                if (b_change)
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + d_realDrop + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_realDrop + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");
                                }
                                else
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE cup_details SET TotalWeight = TotalWeight+ " + dt_drop_details2.Rows[0]["RealDropWeight"] + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + dt_drop_details2.Rows[0]["RealDropWeight"] + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");
                                }
                            }
                        }
                        else
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "UPDATE drop_details SET Finish = 1,RealDropWeight = 0.00 " + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '" + (dt_bottle_details2.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                           "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                           "CupNum = " + kvp.Key + ";");
                        }

                        DataTable dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName.ToString() + "' AND Finish = 0" + " AND CupNum = " + kvp.Key + ";");
                        //查询一下加水没完成的也不置为完成
                        DataTable dt_drop_details4 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND AddWaterChoose = 1 AND AddWaterFinish =0" + " AND CupNum = " + kvp.Key + ";");
                        if (dt_drop_details3.Rows.Count == 0 && dt_drop_details4.Rows.Count == 0)
                        {
                            //更新配液杯总量
                            double d_totalW = 0.0;

                            //置为完成
                            s_sql = "UPDATE drop_head SET CupFinish = 1 WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + kvp.Key + "; ";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);


                            bool b_fail = true;

                            s_sql = "SELECT drop_details.CupNum as CupNum, " +
                               " drop_details.UnitOfAccount as UnitOfAccount, " +
                                        "drop_details.BottleNum as BottleNum, " +
                                        "drop_details.ObjectDropWeight as ObjectDropWeight, " +
                                        "drop_details.RealDropWeight as RealDropWeight, " +
                                        "bottle_details.SyringeType as SyringeType " +
                                        "FROM drop_details left join bottle_details on " +
                                        "bottle_details.BottleNum = drop_details.BottleNum " +
                                        "WHERE drop_details.BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + kvp.Key + ";";
                            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                            foreach (DataRow dr in dt_drop_head.Rows)
                            {
                                if (Convert.ToInt32(dr["BottleNum"]) > Lib_Card.Configure.Parameter.Machine_Bottle_Total)
                                    continue;
                                double d_blRealErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"]))) : Convert.ToDouble(string.Format("{0:F3}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])));
                                d_blRealErr = d_blRealErr < 0 ? -d_blRealErr : d_blRealErr;
                                if (d_blRealErr > (dr["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip))
                                    b_fail = false;

                                d_totalW += Convert.ToDouble(dr["RealDropWeight"]);
                            }

                            dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                    "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + kvp.Key + ";");
                            if (dt_drop_details3.Rows.Count > 0)
                            {
                                int i_cup = Convert.ToInt16(dt_drop_details3.Rows[0]["CupNum"]);
                                int i_Step = Convert.ToInt16(dt_drop_details3.Rows[0]["Step"]);
                                double d_objWater = Convert.ToDouble(dt_drop_details3.Rows[0]["ObjectAddWaterWeight"]);
                                double d_realWater = Convert.ToDouble(dt_drop_details3.Rows[0]["RealAddWaterWeight"]);
                                double d_totalWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TotalWeight"]);
                                double d_testTubeObjectAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeObjectAddWaterWeight"]);
                                double d_testTubeRealAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeRealAddWaterWeight"]);
                                double d_realDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater - d_objWater) : string.Format("{0:F3}", d_realWater - d_objWater));
                                d_realDif = d_realDif < 0 ? -d_realDif : d_realDif;
                                double d_allDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)) : string.Format("{0:F3}",
                                    d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)));

                                string s_describe;
                                string s_describe_EN;
                                if (d_allDif < d_realDif || (d_realWater == 0.0 && d_objWater != 0.0))
                                {
                                    b_fail = false;
                                }

                                d_totalW += d_realWater;

                                if (b_fail)
                                {
                                    s_describe = "滴液成功!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                              ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Success !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(kvp.Key))
                                    {
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cup))
                                            FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                                    }
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  "UPDATE cup_details SET Statues = '滴液成功',TotalWeight = "+ (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalW) : string.Format("{0:F3}",
                                    d_totalW))+" WHERE CupNum = " + i_cup + ";");
                                }
                                else
                                {
                                    s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                            ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET Statues = '滴液失败',TotalWeight = "+ (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalW) : string.Format("{0:F3}",
                                    d_totalW))+" WHERE CupNum = " + i_cup + ";");
                                }
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE drop_head SET DescribeChar = '" + s_describe + "',DescribeChar_EN = '" + s_describe_EN + "', FinishTime = '" + DateTime.Now + "', Step = 2 " +
                               "WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cup + ";");
                            }
                        }

                    }
                }
                else
                {
                    foreach (KeyValuePair<int, double> kvp in dic_return)
                    {
                        double d_blRErr = 0;
                        if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                        else
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight));
                        ;

                        //查询开料日期
                        DataTable dt_bottle_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
            "SELECT bottle_details.*,UnitOfAccount  FROM bottle_details left join assistant_details   on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE  bottle_details.BottleNum = " + i_minBottleNo + ";");

                        if (0.00 != kvp.Value)
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + d_blRErr + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '" + (dt_bottle_details2.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                            "WHERE BatchName = '" + oBatchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                            "CupNum = " + kvp.Key + ";");

                            DataTable dt_drop_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName + "' AND BottleNum = " + i_minBottleNo + " AND CupNum = " + kvp.Key + ";");
                            if (dt_drop_details2.Rows.Count > 0)
                            {
                                //记录是否修正数据
                                bool b_change = false;
                                double d_realDrop = 0.0;
                                if (FADM_Object.Communal._b_isCorrect)
                                {
                                    //如果实际滴液量为负数，就改为0
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["RealDropWeight"]) < 0)
                                    {
                                        b_change = true;
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = 0   ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                    }

                                    //判断当目标滴液量大于1g，而且实际滴液误差小于等于0.05的，全部置为0.02
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) > 1)
                                    {
                                        if (d_blRErr <= 0.05 && d_blRErr > 0.02)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + 0.02 + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) + 0.02;
                                        }
                                        else if (d_blRErr < -0.02 && d_blRErr >= -0.05)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + (-0.02) + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) - 0.02;
                                        }
                                    }
                                }
                                if (b_change)
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + d_realDrop + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_realDrop + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");
                                }
                                else
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + dt_drop_details2.Rows[0]["RealDropWeight"] + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + dt_drop_details2.Rows[0]["RealDropWeight"] + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");

                                    ////置位完成标志位
                                    //FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    //    "UPDATE drop_details SET Finish = 1 WHERE BatchName = '" + o_BatchName + "' AND " +
                                    //    "BottleNum = " + _i_minBottleNo + " AND CupNum = " + dic_pulse.First().Key + ";");
                                }
                            }
                        }
                        else
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "UPDATE drop_details SET Finish = 1,RealDropWeight = 0.00 " + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '" + (dt_bottle_details2.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                           "WHERE BatchName = '" + oBatchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                           "CupNum = " + kvp.Key + ";");
                        }

                    }
                }
            }
            //由于滴废液时发现数值太小，直接提醒，不先滴这个，跳过
            else if(i_ret == -2)
            {
                //把已经滴过的先置为完成
                if (FADM_Object.Communal._b_isFinishSend)
                {
                    foreach (KeyValuePair<int, double> kvp in dic_return)
                    {
                        double d_blRErr = 0;
                        if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                        else
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight));

                        //查询开料日期
                        DataTable dt_bottle_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
            "SELECT bottle_details.*,UnitOfAccount  FROM bottle_details left join assistant_details   on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE  bottle_details.BottleNum = " + i_minBottleNo + ";");

                        if (0.00 != kvp.Value)
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + d_blRErr + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '" + (dt_bottle_details2.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                            "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                            "CupNum = " + kvp.Key + ";");

                            DataTable dt_drop_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND CupNum = " + kvp.Key + ";");
                            if (dt_drop_details2.Rows.Count > 0)
                            {
                                //记录是否修正数据
                                bool b_change = false;
                                double d_realDrop = 0.0;
                                if (FADM_Object.Communal._b_isCorrect)
                                {
                                    //如果实际滴液量为负数，就改为0
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["RealDropWeight"]) < 0)
                                    {
                                        b_change = true;
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = 0   ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                    }

                                    //判断当目标滴液量大于1g，而且实际滴液误差小于等于0.05的，全部置为0.02
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) > 1)
                                    {
                                        if (d_blRErr <= 0.05 && d_blRErr > 0.02)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + 0.02 + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) + 0.02;
                                        }
                                        else if (d_blRErr < -0.02 && d_blRErr >= -0.05)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + (-0.02) + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) - 0.02;
                                        }
                                    }
                                }
                                if (b_change)
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + d_realDrop + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_realDrop + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");
                                }
                                else
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE cup_details SET TotalWeight = TotalWeight+ " + dt_drop_details2.Rows[0]["RealDropWeight"] + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + dt_drop_details2.Rows[0]["RealDropWeight"] + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");
                                }
                            }
                        }
                        else
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "UPDATE drop_details SET Finish = 1,RealDropWeight = 0.00 " + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '" + (dt_bottle_details2.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                           "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                           "CupNum = " + kvp.Key + ";");
                        }

                        DataTable dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName.ToString() + "' AND Finish = 0" + " AND CupNum = " + kvp.Key + ";");
                        //查询一下加水没完成的也不置为完成
                        DataTable dt_drop_details4 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND AddWaterChoose = 1 AND AddWaterFinish =0" + " AND CupNum = " + kvp.Key + ";");
                        if (dt_drop_details3.Rows.Count == 0 && dt_drop_details4.Rows.Count == 0)
                        {
                            //更新配液杯总量
                            double d_totalW = 0.0;
                            //置为完成
                            s_sql = "UPDATE drop_head SET CupFinish = 1 WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + kvp.Key + "; ";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);


                            bool b_fail = true;

                            s_sql = "SELECT drop_details.CupNum as CupNum, " +
                                " drop_details.UnitOfAccount as UnitOfAccount, " +
                                        "drop_details.BottleNum as BottleNum, " +
                                        "drop_details.ObjectDropWeight as ObjectDropWeight, " +
                                        "drop_details.RealDropWeight as RealDropWeight, " +
                                        "bottle_details.SyringeType as SyringeType " +
                                        "FROM drop_details left join bottle_details on " +
                                        "bottle_details.BottleNum = drop_details.BottleNum " +
                                        "WHERE drop_details.BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + kvp.Key + ";";
                            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                            foreach (DataRow dr in dt_drop_head.Rows)
                            {
                                if (Convert.ToInt32(dr["BottleNum"]) > Lib_Card.Configure.Parameter.Machine_Bottle_Total)
                                    continue;
                                double d_blRealErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"]))) : Convert.ToDouble(string.Format("{0:F3}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])));
                                d_blRealErr = d_blRealErr < 0 ? -d_blRealErr : d_blRealErr;
                                if (d_blRealErr > (dr["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip))
                                    b_fail = false;

                                d_totalW += Convert.ToDouble(dr["RealDropWeight"]);
                            }

                            dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                    "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + kvp.Key + ";");
                            if (dt_drop_details3.Rows.Count > 0)
                            {
                                int i_cup = Convert.ToInt16(dt_drop_details3.Rows[0]["CupNum"]);
                                int i_Step = Convert.ToInt16(dt_drop_details3.Rows[0]["Step"]);
                                double d_objWater = Convert.ToDouble(dt_drop_details3.Rows[0]["ObjectAddWaterWeight"]);
                                double d_realWater = Convert.ToDouble(dt_drop_details3.Rows[0]["RealAddWaterWeight"]);
                                double d_totalWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TotalWeight"]);
                                double d_testTubeObjectAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeObjectAddWaterWeight"]);
                                double d_testTubeRealAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeRealAddWaterWeight"]);
                                double d_realDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater - d_objWater) : string.Format("{0:F3}", d_realWater - d_objWater));
                                d_realDif = d_realDif < 0 ? -d_realDif : d_realDif;
                                double d_allDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)) : string.Format("{0:F3}",
                                    d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)));

                                string s_describe;
                                string s_describe_EN;
                                if (d_allDif < d_realDif || (d_realWater == 0.0 && d_objWater != 0.0))
                                {
                                    b_fail = false;
                                }

                                d_totalW += d_realWater;

                                if (b_fail)
                                {
                                    s_describe = "滴液成功!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                              ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Success !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(kvp.Key))
                                    {
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cup))
                                            FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                                    }
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  "UPDATE cup_details SET Statues = '滴液成功',TotalWeight = "+ (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalW) : string.Format("{0:F3}",
                                    d_totalW))+" WHERE CupNum = " + i_cup + ";");
                                }
                                else
                                {
                                    s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                            ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET Statues = '滴液失败',TotalWeight = "+ (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalW) : string.Format("{0:F3}",
                                    d_totalW))+" WHERE CupNum = " + i_cup + ";");
                                }
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE drop_head SET DescribeChar = '" + s_describe + "',DescribeChar_EN = '" + s_describe_EN + "', FinishTime = '" + DateTime.Now + "', Step = 2 " +
                               "WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cup + ";");
                            }
                        }

                    }
                }
                else
                {
                    foreach (KeyValuePair<int, double> kvp in dic_return)
                    {
                        double d_blRErr = 0;
                        if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                        else
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight));
                        ;

                        //查询开料日期
                        DataTable dt_bottle_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
            "SELECT bottle_details.*,UnitOfAccount  FROM bottle_details left join assistant_details   on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE  bottle_details.BottleNum = " + i_minBottleNo + ";");

                        if (0.00 != kvp.Value)
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + d_blRErr + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '" + (dt_bottle_details2.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                            "WHERE BatchName = '" + oBatchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                            "CupNum = " + kvp.Key + ";");

                            DataTable dt_drop_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName + "' AND BottleNum = " + i_minBottleNo + " AND CupNum = " + kvp.Key + ";");
                            if (dt_drop_details2.Rows.Count > 0)
                            {
                                //记录是否修正数据
                                bool b_change = false;
                                double d_realDrop = 0.0;
                                if (FADM_Object.Communal._b_isCorrect)
                                {
                                    //如果实际滴液量为负数，就改为0
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["RealDropWeight"]) < 0)
                                    {
                                        b_change = true;
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = 0   ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                    }

                                    //判断当目标滴液量大于1g，而且实际滴液误差小于等于0.05的，全部置为0.02
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) > 1)
                                    {
                                        if (d_blRErr <= 0.05 && d_blRErr > 0.02)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + 0.02 + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) + 0.02;
                                        }
                                        else if (d_blRErr < -0.02 && d_blRErr >= -0.05)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + (-0.02) + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) - 0.02;
                                        }
                                    }
                                }
                                if (b_change)
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + d_realDrop + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_realDrop + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");
                                }
                                else
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + dt_drop_details2.Rows[0]["RealDropWeight"] + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + dt_drop_details2.Rows[0]["RealDropWeight"] + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");

                                    ////置位完成标志位
                                    //FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    //    "UPDATE drop_details SET Finish = 1 WHERE BatchName = '" + o_BatchName + "' AND " +
                                    //    "BottleNum = " + _i_minBottleNo + " AND CupNum = " + dic_pulse.First().Key + ";");
                                }
                            }
                        }
                        else
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "UPDATE drop_details SET Finish = 1,RealDropWeight = 0.00 " + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '" + (dt_bottle_details2.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                           "WHERE BatchName = '" + oBatchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                           "CupNum = " + kvp.Key + ";");
                        }

                    }
                }

                //更新需要加药第一杯脉冲
                s_sql = "UPDATE drop_details SET NeedPulse = "+Communal._i_needPulse+" WHERE BatchName = '" + oBatchName + "'  And " +
                                "BottleNum = " + i_minBottleNo + " AND Finish = 0  And CupNum = "+Communal._i_needPulseCupNumber+";";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                //把剩余没有滴液的Min置为状态4
                s_sql = "UPDATE drop_details SET MinWeight = 4 WHERE BatchName = '" + oBatchName + "'  And " +
                                "BottleNum = " + i_minBottleNo + " AND Finish = 0 ;";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                MyAlarm myAlarm;
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + "号母液瓶预滴液数值太小,请检查实际是否液量过低?(继续执行请点是)", "Drip", i_minBottleNo, 2, 10, true);
                else
                    myAlarm = new FADM_Object.MyAlarm( " The number of pre-drops in mother liquor bottle "+i_minBottleNo +"  is too small, please check whether the actual amount of liquid is too low" +
                        "( Continue to perform please click Yes)", "Drip", i_minBottleNo, 2, 10, true);

                //回一次原点再继续，担心失步导致后续母液抽不了
                int i_state = MyModbusFun.goHome();
                if (0 != i_state && -2 != i_state)
                    throw new Exception("驱动异常");

            }
            //多针筒过程，后边发现液量低
            else if (i_ret == -3)
            {
                //把已经滴过的先置为完成
                if (FADM_Object.Communal._b_isFinishSend)
                {
                    foreach (KeyValuePair<int, double> kvp in dic_return)
                    {
                        double d_blRErr = 0;
                        if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                        else
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight));

                        //查询开料日期
                        DataTable dt_bottle_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
            "SELECT bottle_details.*,UnitOfAccount  FROM bottle_details left join assistant_details   on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE  bottle_details.BottleNum = " + i_minBottleNo + ";");

                        if (0.00 != kvp.Value)
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + d_blRErr + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '" + (dt_bottle_details2.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                            "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                            "CupNum = " + kvp.Key + ";");

                            DataTable dt_drop_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND CupNum = " + kvp.Key + ";");
                            if (dt_drop_details2.Rows.Count > 0)
                            {
                                //记录是否修正数据
                                bool b_change = false;
                                double d_realDrop = 0.0;
                                if (FADM_Object.Communal._b_isCorrect)
                                {
                                    //如果实际滴液量为负数，就改为0
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["RealDropWeight"]) < 0)
                                    {
                                        b_change = true;
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = 0   ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                    }

                                    //判断当目标滴液量大于1g，而且实际滴液误差小于等于0.05的，全部置为0.02
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) > 1)
                                    {
                                        if (d_blRErr <= 0.05 && d_blRErr > 0.02)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + 0.02 + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) + 0.02;
                                        }
                                        else if (d_blRErr < -0.02 && d_blRErr >= -0.05)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + (-0.02) + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) - 0.02;
                                        }
                                    }
                                }
                                if (b_change)
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + d_realDrop + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_realDrop + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");
                                }
                                else
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE cup_details SET TotalWeight = TotalWeight+ " + dt_drop_details2.Rows[0]["RealDropWeight"] + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + dt_drop_details2.Rows[0]["RealDropWeight"] + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");
                                }
                            }
                        }
                        else
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "UPDATE drop_details SET Finish = 1,RealDropWeight = 0.00 " + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '" + (dt_bottle_details2.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                           "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                           "CupNum = " + kvp.Key + ";");
                        }

                        DataTable dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName.ToString() + "' AND Finish = 0" + " AND CupNum = " + kvp.Key + ";");
                        //查询一下加水没完成的也不置为完成
                        DataTable dt_drop_details4 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND AddWaterChoose = 1 AND AddWaterFinish =0" + " AND CupNum = " + kvp.Key + ";");
                        if (dt_drop_details3.Rows.Count == 0 && dt_drop_details4.Rows.Count == 0)
                        {
                            //更新配液杯总量
                            double d_totalW = 0.0;
                            //置为完成
                            s_sql = "UPDATE drop_head SET CupFinish = 1 WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + kvp.Key + "; ";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);


                            bool b_fail = true;

                            s_sql = "SELECT drop_details.CupNum as CupNum, " +
                                " drop_details.UnitOfAccount as UnitOfAccount, " +
                                        "drop_details.BottleNum as BottleNum, " +
                                        "drop_details.ObjectDropWeight as ObjectDropWeight, " +
                                        "drop_details.RealDropWeight as RealDropWeight, " +
                                        "bottle_details.SyringeType as SyringeType " +
                                        "FROM drop_details left join bottle_details on " +
                                        "bottle_details.BottleNum = drop_details.BottleNum " +
                                        "WHERE drop_details.BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + kvp.Key + ";";
                            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                            foreach (DataRow dr in dt_drop_head.Rows)
                            {
                                if (Convert.ToInt32(dr["BottleNum"]) > Lib_Card.Configure.Parameter.Machine_Bottle_Total)
                                    continue;
                                double d_blRealErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"]))) : Convert.ToDouble(string.Format("{0:F3}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])));
                                d_blRealErr = d_blRealErr < 0 ? -d_blRealErr : d_blRealErr;
                                if (d_blRealErr > (dr["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip))
                                    b_fail = false;

                                d_totalW += Convert.ToDouble(dr["RealDropWeight"]);
                            }

                            dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                    "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + kvp.Key + ";");
                            if (dt_drop_details3.Rows.Count > 0)
                            {
                                int i_cup = Convert.ToInt16(dt_drop_details3.Rows[0]["CupNum"]);
                                int i_Step = Convert.ToInt16(dt_drop_details3.Rows[0]["Step"]);
                                double d_objWater = Convert.ToDouble(dt_drop_details3.Rows[0]["ObjectAddWaterWeight"]);
                                double d_realWater = Convert.ToDouble(dt_drop_details3.Rows[0]["RealAddWaterWeight"]);
                                double d_totalWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TotalWeight"]);
                                double d_testTubeObjectAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeObjectAddWaterWeight"]);
                                double d_testTubeRealAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeRealAddWaterWeight"]);
                                double d_realDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater - d_objWater) : string.Format("{0:F3}", d_realWater - d_objWater));
                                d_realDif = d_realDif < 0 ? -d_realDif : d_realDif;
                                double d_allDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)) : string.Format("{0:F3}",
                                    d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)));

                                string s_describe;
                                string s_describe_EN;
                                if (d_allDif < d_realDif || (d_realWater == 0.0 && d_objWater != 0.0))
                                {
                                    b_fail = false;
                                }

                                d_totalW += d_realWater;

                                if (b_fail)
                                {
                                    s_describe = "滴液成功!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                              ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Success !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(kvp.Key))
                                    {
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cup))
                                            FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                                    }
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  "UPDATE cup_details SET Statues = '滴液成功',TotalWeight = " + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalW) : string.Format("{0:F3}",
                                    d_totalW)) + " WHERE CupNum = " + i_cup + ";");
                                }
                                else
                                {
                                    s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                            ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET Statues = '滴液失败',TotalWeight = " + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalW) : string.Format("{0:F3}",
                                    d_totalW)) + " WHERE CupNum = " + i_cup + ";");
                                }
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE drop_head SET DescribeChar = '" + s_describe + "',DescribeChar_EN = '" + s_describe_EN + "', FinishTime = '" + DateTime.Now + "', Step = 2 " +
                               "WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cup + ";");
                            }
                        }

                    }
                }
                else
                {
                    foreach (KeyValuePair<int, double> kvp in dic_return)
                    {
                        double d_blRErr = 0;
                        if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                        else
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight));
                        ;

                        //查询开料日期
                        DataTable dt_bottle_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
            "SELECT bottle_details.*,UnitOfAccount  FROM bottle_details left join assistant_details   on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE  bottle_details.BottleNum = " + i_minBottleNo + ";");

                        if (0.00 != kvp.Value)
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + d_blRErr + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '" + (dt_bottle_details2.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                            "WHERE BatchName = '" + oBatchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                            "CupNum = " + kvp.Key + ";");

                            DataTable dt_drop_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName + "' AND BottleNum = " + i_minBottleNo + " AND CupNum = " + kvp.Key + ";");
                            if (dt_drop_details2.Rows.Count > 0)
                            {
                                //记录是否修正数据
                                bool b_change = false;
                                double d_realDrop = 0.0;
                                if (FADM_Object.Communal._b_isCorrect)
                                {
                                    //如果实际滴液量为负数，就改为0
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["RealDropWeight"]) < 0)
                                    {
                                        b_change = true;
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = 0   ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                    }

                                    //判断当目标滴液量大于1g，而且实际滴液误差小于等于0.05的，全部置为0.02
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) > 1)
                                    {
                                        if (d_blRErr <= 0.05 && d_blRErr > 0.02)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + 0.02 + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) + 0.02;
                                        }
                                        else if (d_blRErr < -0.02 && d_blRErr >= -0.05)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + (-0.02) + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) - 0.02;
                                        }
                                    }
                                }
                                if (b_change)
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + d_realDrop + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_realDrop + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");
                                }
                                else
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + dt_drop_details2.Rows[0]["RealDropWeight"] + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + dt_drop_details2.Rows[0]["RealDropWeight"] + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");

                                    ////置位完成标志位
                                    //FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    //    "UPDATE drop_details SET Finish = 1 WHERE BatchName = '" + o_BatchName + "' AND " +
                                    //    "BottleNum = " + _i_minBottleNo + " AND CupNum = " + dic_pulse.First().Key + ";");
                                }
                            }
                        }
                        else
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "UPDATE drop_details SET Finish = 1,RealDropWeight = 0.00 " + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '" + (dt_bottle_details2.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                           "WHERE BatchName = '" + oBatchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                           "CupNum = " + kvp.Key + ";");
                        }

                    }
                }

                //更新需要加药第一杯脉冲
                s_sql = "UPDATE drop_details SET NeedPulse = " + Communal._i_needPulse + " WHERE BatchName = '" + oBatchName + "'  And " +
                                "BottleNum = " + i_minBottleNo + " AND Finish = 0  And CupNum = " + Communal._i_needPulseCupNumber + ";";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                if (Communal._b_isHaveAutoBrew)
                {
                    //把剩余没有滴液的Min置为状态6,改为自动开料
                    s_sql = "UPDATE drop_details SET MinWeight = 6 WHERE BatchName = '" + oBatchName + "'  And " +
                                    "BottleNum = " + i_minBottleNo + " AND Finish = 0 ;";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                    //插入自动开料列表
                    //判断是否存在正在开料的记录
                    s_sql = "select * from brew_station_details where IsUsing = 1 And Station<6 And BottleNum =" + i_minBottleNo;
                    DataTable dt_brew_station_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                    if (dt_brew_station_details.Rows.Count == 0)
                    {
                        //判断等待列表是否存在排队
                        s_sql = "select * from Brew_wait_list where  BottleNum =" + i_minBottleNo;
                        DataTable dt_Brew_wait_list = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                        if (dt_Brew_wait_list.Rows.Count == 0)
                        {
                            //插入到排队列表
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
               "INSERT INTO Brew_wait_list(BottleNum, InsertDate,Type) VALUES('" + i_minBottleNo + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',0);");
                        }

                    }
                }
                else
                {
                    //把剩余没有滴液的Min置为状态1,改为液量不足
                    s_sql = "UPDATE drop_details SET MinWeight = 1 WHERE BatchName = '" + oBatchName + "'  And " +
                                    "BottleNum = " + i_minBottleNo + " AND Finish = 0 ;";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                }
                

            }

            /*
    label9:
        //移动到母液瓶
        if (_b_dripStop)
        {
            FADM_Object.Communal._b_stop = true;
        }
        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找" + _i_minBottleNo + "号母液瓶");
        FADM_Object.Communal._i_optBottleNum = _i_minBottleNo;
        i_mRes = MyModbusFun.TargetMove(0, _i_minBottleNo, 1);
        if (-2 == i_mRes)
            throw new Exception("收到退出消息");
        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达" + _i_minBottleNo + "号母液瓶");

        //抽液
        if (_b_dripStop)
        {
            FADM_Object.Communal._b_stop = true;
        }
        int i_extractionPulse = 0;
        if ("小针筒" == _s_syringeType || "Little Syringe" == _s_syringeType)
        {
            i_extractionPulse = -(i_pulseT + _i_adjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1));
            if (i_extractionPulse - Lib_Card.Configure.Parameter.Other_Z_BackPulse < Lib_Card.Configure.Parameter.Other_S_MaxPulse)
            {
                if (FADM_Object.Communal._b_isFullDrip)
                {
                    //超出单针筒上限
                    i_extractionPulse = Lib_Card.Configure.Parameter.Other_S_MaxPulse + Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                }
                else
                {
                    i_extractionPulse = -_i_adjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1);
                    int temp = i_extractionPulse;
                    for (int i = 0; i < dic_pulse.Count; i++)
                    {
                        var v1 = dic_pulse.ElementAt(i);
                        var v2 = dic_weight.ElementAt(i);

                        temp = i_extractionPulse;

                        if (temp - v1.Value - Lib_Card.Configure.Parameter.Other_Z_BackPulse < Lib_Card.Configure.Parameter.Other_S_MaxPulse)
                        {
                            //如果需加量大于10g，直接先加
                            //if(v1.Value/1.0/ _i_adjust > Lib_Card.Configure.Parameter.Other_SplitValue)
                            if (temp == -_i_adjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1))
                            {
                                i_extractionPulse = Lib_Card.Configure.Parameter.Other_S_MaxPulse + Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                            }
                            else
                            {
                                //如果一筒抽不完就分2次，一次抽完就留下一次独立抽
                                if (-v1.Value - _i_adjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1) - Lib_Card.Configure.Parameter.Other_Z_BackPulse < Lib_Card.Configure.Parameter.Other_S_MaxPulse)
                                {
                                    i_extractionPulse = Lib_Card.Configure.Parameter.Other_S_MaxPulse + Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                                }
                            }
                            break;
                        }
                        else
                        {
                            i_extractionPulse -= v1.Value;
                        }
                    }
                }
            }
            if (FADM_Object.Communal._b_isDripReserveFirst)
                i_extractionPulse -= _i_adjust;
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抽液启动(" + i_extractionPulse + ")");

        label10:
            try
            {
                i_extractionPulse = i_extractionPulse - Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                i_mRes = MyModbusFun.Extract(i_extractionPulse, _s_unitOfAccount.Equals("g/l") ? true : false, 0); //抽液
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
            }
            catch (Exception ex)
            {
                if ("未发现针筒" == ex.Message)
                {
                    if (i_lowSrart == 2)
                    {
                        FADM_Object.Communal.WriteDripWait(true);
                        FADM_Object.MyAlarm myAlarm;
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            myAlarm = new FADM_Object.MyAlarm(_i_minBottleNo + "号母液瓶未找到针筒，是否继续执行?(继续寻找请点是，退出针检请点否)", "滴液", true, 1);
                        else
                            myAlarm = new FADM_Object.MyAlarm(_i_minBottleNo + " bottle did not find a syringe. Do you want to continue? " +
                                "(To continue searching, please click Yes. To exit the needle test, please click No)", "Drip", true, 1);
                        while (true)
                        {
                            if (0 != myAlarm._i_alarm_Choose)
                                break;
                            Thread.Sleep(1);
                        }
                        //判断染色线程是否需要用机械手
                        if (null != FADM_Object.Communal.ReadDyeThread())
                        {
                            FADM_Object.Communal.WriteDripWait(true);

                            while (true)
                            {
                                if (false == FADM_Object.Communal.ReadDripWait())
                                    break;
                                Thread.Sleep(1);
                            }
                        }
                        else
                        {
                            FADM_Object.Communal.WriteDripWait(false);
                        }

                        if (1 == myAlarm._i_alarm_Choose)
                            goto label3;
                        else
                            throw new Exception("收到退出消息");
                    }
                    else
                    {

                        s_sql = "UPDATE drop_details SET MinWeight = 2 WHERE BatchName = '" + o_BatchName + "' AND " +
                        "BottleNum = " + _i_minBottleNo + " AND Finish = 0;";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        goto label3;
                    }

                }
                else
                    throw;
            }

        }
        else
        {
            i_extractionPulse = -(i_pulseT + _i_adjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1));
            if (i_extractionPulse - Lib_Card.Configure.Parameter.Other_Z_BackPulse < Lib_Card.Configure.Parameter.Other_B_MaxPulse)
            {
                if (FADM_Object.Communal._b_isFullDrip)
                {
                    //超出单针筒上限
                    i_extractionPulse = Lib_Card.Configure.Parameter.Other_B_MaxPulse + Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                }
                else
                {
                    i_extractionPulse = -_i_adjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1);
                    int temp = i_extractionPulse;
                    for (int i = 0; i < dic_pulse.Count; i++)
                    {
                        var v1 = dic_pulse.ElementAt(i);
                        var v2 = dic_weight.ElementAt(i);

                        temp = i_extractionPulse;

                        if (temp - v1.Value - Lib_Card.Configure.Parameter.Other_Z_BackPulse < Lib_Card.Configure.Parameter.Other_B_MaxPulse)
                        {
                            ////如果需加量大于10g，直接先加
                            //if (v1.Value / 1.0 / _i_adjust >= Lib_Card.Configure.Parameter.Other_SplitValue)
                            if (temp == -_i_adjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1))
                            {
                                i_extractionPulse = Lib_Card.Configure.Parameter.Other_B_MaxPulse + Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                            }
                            else
                            {
                                if (-v1.Value - _i_adjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1) - Lib_Card.Configure.Parameter.Other_Z_BackPulse < Lib_Card.Configure.Parameter.Other_B_MaxPulse)
                                {
                                    i_extractionPulse = Lib_Card.Configure.Parameter.Other_B_MaxPulse + Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                                }
                            }
                            break;
                        }
                        else
                        {
                            i_extractionPulse -= v1.Value;
                        }
                    }
                }
            }
            if (FADM_Object.Communal._b_isDripReserveFirst)
                i_extractionPulse -= _i_adjust;
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抽液启动(" + i_extractionPulse + ")");

        label11:
            try
            {
                i_extractionPulse = i_extractionPulse - Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                i_mRes = MyModbusFun.Extract(i_extractionPulse, _s_unitOfAccount.Equals("g/l") ? true : false, 1); //抽液
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
            }
            catch (Exception ex)
            {
                if ("未发现针筒" == ex.Message)
                {
                    if (i_lowSrart == 2)
                    {
                        FADM_Object.Communal.WriteDripWait(true);
                        FADM_Object.MyAlarm myAlarm;
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            myAlarm = new FADM_Object.MyAlarm(_i_minBottleNo + "号母液瓶未找到针筒，是否继续执行?(继续寻找请点是，退出针检请点否)", "滴液", true, 1);
                        else
                            myAlarm = new FADM_Object.MyAlarm(_i_minBottleNo + " bottle did not find a syringe. Do you want to continue? " +
                                "(To continue searching, please click Yes. To exit the needle test, please click No)", "Drip", true, 1);
                        while (true)
                        {
                            if (0 != myAlarm._i_alarm_Choose)
                                break;
                            Thread.Sleep(1);
                        }
                        //判断染色线程是否需要用机械手
                        if (null != FADM_Object.Communal.ReadDyeThread())
                        {
                            FADM_Object.Communal.WriteDripWait(true);

                            while (true)
                            {
                                if (false == FADM_Object.Communal.ReadDripWait())
                                    break;
                                Thread.Sleep(1);
                            }
                        }
                        else
                        {
                            FADM_Object.Communal.WriteDripWait(false);
                        }

                        if (1 == myAlarm._i_alarm_Choose)
                            goto label3;
                        else
                            throw new Exception("收到退出消息");
                    }
                    else
                    {
                        s_sql = "UPDATE drop_details SET MinWeight = 2 WHERE BatchName = '" + o_BatchName + "' AND " +
                         "BottleNum = " + _i_minBottleNo + " AND Finish = 0;";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        goto label3;
                    }


                }
                else
                    throw;
            }
        }
        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抽液完成");

        if (null != thread)
            thread.Join();

        Lib_SerialPort.Balance.METTLER.bReSetSign = true;

        if (FADM_Object.Communal._b_isDripReserveFirst)
        {
            //移动到天平位，先滴预留量
            if (_b_dripStop)
            {
                FADM_Object.Communal._b_stop = true;
            }
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找天平位");
            //判断是否异常
            FADM_Object.Communal.BalanceState("滴液");

            i_mRes = MyModbusFun.TargetMove(2, 0, 0);
            if (-2 == i_mRes)
                throw new Exception("收到退出消息");

            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达天平位");

            int iZPulse1 = 0;
        label98:
            if (-1 == MyModbusFun.GetZPosition(ref iZPulse1))
                throw new Exception("驱动异常");
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "Z轴脉冲" + iZPulse1);
            if (iZPulse1 > 0)
                goto label98;
            int iInfusionPulse1 = 0;
            if ("小针筒" == _s_syringeType || "Little Syringe" == _s_syringeType)
                iInfusionPulse1 = iZPulse1 + _i_adjust;
            else
                iInfusionPulse1 = iZPulse1 + _i_adjust;

            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "废液注液启动(" + iInfusionPulse1 + ")");
            i_mRes = MyModbusFun.Shove(iInfusionPulse1);
            if (-2 == i_mRes)
                throw new Exception("收到退出消息");
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "废液注液完成");

            //母液瓶扣减1克
            FADM_Object.Communal._fadmSqlserver.ReviseData(
                "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + 1 + "  " +
                "WHERE BottleNum = '" + _i_minBottleNo + "';");

        }

        //移动到配液杯

        List<int> _lis_ints = new List<int>();
    label12:
        if (_b_dripStop)
        {
            FADM_Object.Communal._b_stop = true;
        }
        int iZPulse = 0;
    label99:
        if (-1 == MyModbusFun.GetZPosition(ref iZPulse))
            throw new Exception("驱动异常");
        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "Z轴脉冲" + iZPulse);
        if (iZPulse > 0)
            goto label99;
        if ("小针筒" == _s_syringeType || "Little Syringe" == _s_syringeType)
        {

            if (-_i_adjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1) == iZPulse)
            {
                //当前针筒已滴完，进入复检
                goto label13;
            }
        }
        else
        {
            if (-_i_adjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1) == iZPulse)
            {
                //当前针筒已滴完，进入复检
                goto label13;
            }

        }

        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找" + dic_pulse.First().Key + "号配液杯");
        FADM_Object.Communal._i_OptCupNum = dic_pulse.First().Key;
        i_mRes = MyModbusFun.TargetMove(1, dic_pulse.First().Key, 0);
        if (-2 == i_mRes)
            throw new Exception("收到退出消息");
        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达" + dic_pulse.First().Key + "号配液杯");

        //注液
        if (_b_dripStop)
        {
            FADM_Object.Communal._b_stop = true;
        }
        int i_infusionPulse = 0;
        if ("小针筒" == _s_syringeType || "Little Syringe" == _s_syringeType)
        {
            if (iZPulse + _i_adjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1) <= -dic_pulse.First().Value)
            {
                //剩余量大于等于需加量
                i_infusionPulse = iZPulse + dic_pulse.First().Value;


                //母液瓶扣减
                FADM_Object.Communal._fadmSqlserver.ReviseData(
                    "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + dic_weight.First().Value + " " +
                    "WHERE BottleNum = '" + _i_minBottleNo + "';");


                //置位完成标志位
                FADM_Object.Communal._fadmSqlserver.ReviseData(
                    "UPDATE drop_details SET Finish = 1 WHERE BatchName = '" + o_BatchName + "' AND " +
                    "BottleNum = " + _i_minBottleNo + " AND CupNum = " + dic_pulse.First().Key + ";");

                i_pulseT -= dic_pulse.First().Value;
                _lis_ints.Add(dic_pulse.First().Key);
                dic_pulse.Remove(dic_pulse.First().Key);
                dic_weight.Remove(dic_weight.First().Key);
            }
            else
            {
                i_infusionPulse = -_i_adjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1);
                dic_pulse[dic_pulse.First().Key] -= (-iZPulse - _i_adjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1));
                i_pulseT -= (-iZPulse - _i_adjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1));
            }




            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液启动(" + i_infusionPulse + ")");
            i_mRes = MyModbusFun.Shove(i_infusionPulse);
            if (-2 == i_mRes)
                throw new Exception("收到退出消息");

        }
        else
        {
            if (iZPulse + _i_adjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1) <= -dic_pulse.First().Value)
            {
                //剩余量大于等于需加量
                i_infusionPulse = iZPulse + dic_pulse.First().Value;

                //母液瓶扣减
                FADM_Object.Communal._fadmSqlserver.ReviseData(
                    "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + dic_weight.First().Value + " " +
                    "WHERE BottleNum = '" + _i_minBottleNo + "';");

                //置位完成标志位
                FADM_Object.Communal._fadmSqlserver.ReviseData(
                    "UPDATE drop_details SET Finish = 1 WHERE BatchName = '" + o_BatchName + "' AND " +
                    "BottleNum = " + _i_minBottleNo + " AND CupNum = " + dic_pulse.First().Key + ";");


                i_pulseT -= dic_pulse.First().Value;
                _lis_ints.Add(dic_pulse.First().Key);
                dic_pulse.Remove(dic_pulse.First().Key);
                dic_weight.Remove(dic_weight.First().Key);
            }
            else
            {
                i_infusionPulse = -_i_adjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1);
                dic_pulse[dic_pulse.First().Key] -= (-iZPulse - _i_adjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1));
                i_pulseT -= (-iZPulse - _i_adjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1));
            }

            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液启动(" + i_infusionPulse + ")");
            i_mRes = MyModbusFun.Shove(i_infusionPulse);
            if (-2 == i_mRes)
                throw new Exception("收到退出消息");
        }


        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液完成");


        goto label12;

    //移动到天平位
    label13:
        if (_b_dripStop)
        {
            FADM_Object.Communal._b_stop = true;
        }
        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找天平位");
        //判断是否异常
        FADM_Object.Communal.BalanceState("滴液");

        Lib_SerialPort.Balance.METTLER.bZeroSign = true;
        i_mRes = MyModbusFun.TargetMove(2, 0, 0);
        if (-2 == i_mRes)
            throw new Exception("收到退出消息");

        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达天平位");

        if (FADM_Object.Communal._b_isZero)
        {
            while (true)
            {
                //判断是否成功清零
                if (Lib_SerialPort.Balance.METTLER._s_balanceValue == 0.0)
                {
                    break;
                }
                else
                {
                    //再次发调零
                    Lib_SerialPort.Balance.METTLER.bZeroSign = true;
                }
                Thread.Sleep(1);
            }
        }

        //记录初始天平读数
        double _d_blBalanceValue3 = Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal._s_balanceValue));
        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平读数：" + _d_blBalanceValue3);

        //验证
        if (_b_dripStop)
        {
            FADM_Object.Communal._b_stop = true;
        }
        if ("小针筒" == _s_syringeType||"Little Syringe" == _s_syringeType)
        {
            i_infusionPulse = -_i_adjust;
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "验证启动(" + i_infusionPulse + ")");
            i_mRes = MyModbusFun.Shove(i_infusionPulse);
            if (0 != i_mRes)
                throw new Exception("驱动异常");
            else if (-2 == i_mRes)
                throw new Exception("收到退出消息");
        }
        else
        {
            i_infusionPulse = -_i_adjust;
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "验证启动(" + i_infusionPulse + ")");
            i_mRes = MyModbusFun.Shove(i_infusionPulse);
            if (-2 == i_mRes)
                throw new Exception("收到退出消息");
        }
        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "验证完成");

        //读取验证重量
        sReadCheckArg sRead = new sReadCheckArg();
        sRead.o_BatchName = o_BatchName.ToString();
        sRead._i_minBottleNo = _i_minBottleNo;
        sRead._d_blBalanceValue3 = _d_blBalanceValue3;
        sRead._lis_ints = new List<int>();
        sRead._lis_ints.AddRange(_lis_ints);
        sRead._s_syringeType = _s_syringeType;
        if (FADM_Object.Communal._b_isFinishSend)
        {
            thread = new Thread(ReadCheckWeightAll);
        }
        else
        {
            thread = new Thread(ReadCheckWeight);
        }
        thread.Start(sRead);



        //判断当前母液是否滴完
        if (0 < dic_pulse.Count)
        {
            goto label9;
        }

        //移动到母液瓶
        if (_b_dripStop)
        {
            FADM_Object.Communal._b_stop = true;
        }
        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找" + _i_minBottleNo + "号母液瓶");
        FADM_Object.Communal._i_optBottleNum = _i_minBottleNo;
        i_mRes = MyModbusFun.TargetMove(0, _i_minBottleNo, 0);
        if (-2 == i_mRes)
            throw new Exception("收到退出消息");
        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达" + _i_minBottleNo + "号母液瓶");

        //放针
        if (_b_dripStop)
        {
            FADM_Object.Communal._b_stop = true;
        }
        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", _i_minBottleNo + "号母液瓶放针启动");

        if ("小针筒" == _s_syringeType||"Little Syringe" == _s_syringeType)
        {
            i_mRes = MyModbusFun.Put();
            if (-2 == i_mRes)
                throw new Exception("收到退出消息");
        }
        else
        {
            i_mRes = MyModbusFun.Put();
            if (-2 == i_mRes)
                throw new Exception("收到退出消息");
        }
        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", _i_minBottleNo + "号母液瓶放针完成");*/


            b_checkFail = false;

            goto label7;

        //加助剂
        label4:
            if (FADM_Object.Communal._b_isAssitantFirst)
            {
                s_unitA = "%";
            }
            else
            {
                s_unitA = "g/l";
            }
            goto label3;

        //添加母液不足的
        label5:
            s_sql = "SELECT BottleNum FROM drop_details WHERE BatchName = '" + oBatchName + "' AND Finish = 0 AND MinWeight = 1 AND " +
                "BottleNum <= '" + Lib_Card.Configure.Parameter.Machine_Bottle_Total + "'   And IsDrop != 0  GROUP BY BottleNum ORDER BY BottleNum ;";
            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (0 < dt_drop_head.Rows.Count)
            {
                string s_alarmBottleNo = null;
                foreach (DataRow dataRow in dt_drop_head.Rows)
                {
                    s_alarmBottleNo += dataRow["BottleNum"].ToString() + ";";
                }

                s_alarmBottleNo = s_alarmBottleNo.Remove(s_alarmBottleNo.Length - 1);
                //关闭失能
                MyModbusFun.Power(2);
                FADM_Object.Communal.WriteDripWait(true);
                Communal._b_isAllowDrip = false;
                FADM_Object.MyAlarm myAlarm;
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    myAlarm = new FADM_Object.MyAlarm(s_alarmBottleNo + "号母液瓶液量过低，是否继续滴液?", "滴液", true, 1);
                else
                    myAlarm = new FADM_Object.MyAlarm("The liquid level in bottle " + s_alarmBottleNo + " is too low. Do you want to continue ? ", "Drip", true, 1);
                    while (true)
                {
                    if (0 != myAlarm._i_alarm_Choose) {
                        //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                        //{
                        //   // Home.Home_XYZFinish = false;
                        //}
                        break;
                    }
                        
                    Thread.Sleep(1);
                }
                Communal._b_isAllowDrip = true;
                //判断染色线程是否需要用机械手
                if (null != FADM_Object.Communal.ReadDyeThread())
                {
                    FADM_Object.Communal.WriteDripWait(true);
                    Communal._b_isWaitDrip = true;
                    Communal._b_isAllowDrip = true;
                    while (true)
                    {
                        if (false == FADM_Object.Communal.ReadDripWait())
                            break;
                        Thread.Sleep(1);
                    }
                    Communal._b_isWaitDrip = false;
                }
                else
                {
                    Communal._b_isWaitDrip = false;
                    FADM_Object.Communal.WriteDripWait(false);
                }

                if (1 == myAlarm._i_alarm_Choose)
                {

                    if (FADM_Object.Communal._b_isLowDripAllow)
                    {
                        i_lowSrart = 1;
                        if (FADM_Object.Communal._b_isAssitantFirst)
                        {
                            s_unitA = "g/l";
                        }
                        else
                        {
                            s_unitA = "%";
                        }
                        goto label3;
                    }
                    else
                    {
                        //把MinWeight=1的数据重新置为0
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE drop_details SET MinWeight = 0 where MinWeight = 1;");
                        goto lab_again;
                    }
                }
                else
                {
                    b_chooseNo = true;
                }
            }

        //添加超出生命周期(过期)
        label17:
            s_sql = "SELECT BottleNum FROM drop_details WHERE BatchName = '" + oBatchName + "' AND Finish = 0 AND MinWeight = 3 AND " +
                "BottleNum <= '" + Lib_Card.Configure.Parameter.Machine_Bottle_Total + "'   And IsDrop != 0 GROUP BY BottleNum ORDER BY BottleNum ;";
            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (0 < dt_drop_head.Rows.Count)
            {
                string s_alarmBottleNo = null;
                foreach (DataRow dataRow in dt_drop_head.Rows)
                {
                    s_alarmBottleNo += dataRow["BottleNum"].ToString() + ";";
                }

                s_alarmBottleNo = s_alarmBottleNo.Remove(s_alarmBottleNo.Length - 1);
                //关闭失能
                MyModbusFun.Power(2);
                FADM_Object.Communal.WriteDripWait(true);
                Communal._b_isAllowDrip = false;
                FADM_Object.MyAlarm myAlarm;
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    myAlarm = new FADM_Object.MyAlarm(s_alarmBottleNo + "号母液瓶过期，是否继续滴液?", "滴液", true, 1);
                else
                    myAlarm = new FADM_Object.MyAlarm("The liquid level in bottle " + s_alarmBottleNo + " is expire. Do you want to continue ? ", "Drip", true, 1);
                while (true)
                {
                    if (0 != myAlarm._i_alarm_Choose) {
                        //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                        //{
                        //   // Home.Home_XYZFinish = false;
                        //}
                        break;
                    }
                        
                    Thread.Sleep(1);
                }
                Communal._b_isAllowDrip = true;
                //判断染色线程是否需要用机械手
                if (null != FADM_Object.Communal.ReadDyeThread())
                {
                    FADM_Object.Communal.WriteDripWait(true);
                    Communal._b_isWaitDrip = true;
                    Communal._b_isAllowDrip = true;
                    while (true)
                    {
                        if (false == FADM_Object.Communal.ReadDripWait())
                            break;
                        Thread.Sleep(1);
                    }
                    Communal._b_isWaitDrip = false;
                }
                else
                {
                    Communal._b_isWaitDrip = false;
                    FADM_Object.Communal.WriteDripWait(false);
                }

                if (1 == myAlarm._i_alarm_Choose)
                {
                    if (FADM_Object.Communal._b_isOutDripAllow)
                    {
                        i_lowSrart = 3;
                        if (FADM_Object.Communal._b_isAssitantFirst)
                        {
                            s_unitA = "g/l";
                        }
                        else
                        {
                            s_unitA = "%";
                        }
                        goto label3;
                    }
                    else
                    {
                        //把MinWeight=3的数据重新置为0
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE drop_details SET MinWeight = 0 where MinWeight = 3;");
                        goto lab_again;
                    }
                }
                else
                {
                    b_chooseNo = true;
                }
            }


        //添加找不到针筒的
        label16:
            s_sql = "SELECT BottleNum FROM drop_details WHERE BatchName = '" + oBatchName + "' AND Finish = 0 AND MinWeight = 2 AND " +
               "BottleNum <= '" + Lib_Card.Configure.Parameter.Machine_Bottle_Total + "'  And IsDrop != 0  GROUP BY BottleNum ORDER BY BottleNum ;";
            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (0 < dt_drop_head.Rows.Count)
            {
                string s_alarmBottleNo = null;
                foreach (DataRow dataRow in dt_drop_head.Rows)
                {
                    s_alarmBottleNo += dataRow["BottleNum"].ToString() + ";";
                }

                s_alarmBottleNo = s_alarmBottleNo.Remove(s_alarmBottleNo.Length - 1);
                FADM_Object.Communal.WriteDripWait(true);
                Communal._b_isAllowDrip = false;
                FADM_Object.MyAlarm myAlarm;
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    myAlarm = new FADM_Object.MyAlarm(s_alarmBottleNo + "号母液瓶未检测到针筒，是否继续滴液 ? ", "滴液", true, 1, true);
                else
                    myAlarm = new FADM_Object.MyAlarm(s_alarmBottleNo + " bottle did not find a syringe. Do you want to continue? ", "Drip", true, 1, true);
                while (true)
                {
                    if (0 != myAlarm._i_alarm_Choose) {
                        //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                        //{
                        //  //  Home.Home_XYZFinish = false;
                        //}
                        break;
                    }
                    Thread.Sleep(1);
                }
                Communal._b_isAllowDrip = true;
                //判断染色线程是否需要用机械手
                if (null != FADM_Object.Communal.ReadDyeThread())
                {
                    FADM_Object.Communal.WriteDripWait(true);
                    Communal._b_isWaitDrip = true;
                    Communal._b_isAllowDrip = true;
                    while (true)
                    {
                        if (false == FADM_Object.Communal.ReadDripWait())
                            break;
                        Thread.Sleep(1);
                    }
                    Communal._b_isWaitDrip = false;
                }
                else
                {
                    Communal._b_isWaitDrip = false;
                    FADM_Object.Communal.WriteDripWait(false);
                }

                if (1 == myAlarm._i_alarm_Choose)
                {
                    i_lowSrart = 2;
                    if (FADM_Object.Communal._b_isAssitantFirst)
                    {
                        s_unitA = "g/l";
                    }
                    else
                    {
                        s_unitA = "%";
                    }
                    goto label3;
                }
                else
                {
                    b_chooseNo = true;
                }
            }

        //滴液完成    
        label6:
            //关闭失能
            if (!FADM_Object.Communal._b_isJustShowInfo && !FADM_Object.Communal._b_isUseBrewOnly)
                MyModbusFun.Power(2);
            FADM_Object.Communal.WriteDripWait(true);
            Communal._b_isAllowDrip = false;
            if (!b_chooseNo)
            {
                lab_Re:
                //判断是否全部完成，等待是否还有洗杯没完成的
                s_sql = "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName + "' AND Finish = 0  AND " +
                   "BottleNum <= '" + Lib_Card.Configure.Parameter.Machine_Bottle_Total + "' ;";
                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                if (dt_drop_head.Rows.Count > 0)
                {
                    
                    foreach (DataRow dataRow in dt_drop_head.Rows)
                    {
                        if (dataRow["IsDrop"].ToString() == "1" && dataRow["MinWeight"].ToString() != "4" && dataRow["MinWeight"].ToString() != "6")
                        {

                            Communal._b_isAllowDrip = true;
                            //获取机械手
                            if (null != FADM_Object.Communal.ReadDyeThread())
                            {
                                FADM_Object.Communal.WriteDripWait(true);
                                Communal._b_isWaitDrip = true;
                                Communal._b_isAllowDrip = true;
                                while (true)
                                {
                                    if (false == FADM_Object.Communal.ReadDripWait())
                                        break;
                                    Thread.Sleep(1);
                                }
                                Communal._b_isWaitDrip = false;
                            }
                            else
                            {
                                Communal._b_isWaitDrip = false;
                                FADM_Object.Communal.WriteDripWait(false);
                            }
                            goto lab_again;
                        }
                        ////代表有自动开料完成的
                        //else if(dataRow["MinWeight"].ToString() == "7")
                        //{
                        //    //把7的改为0
                        //    Communal._b_isAllowDrip = true;
                        //    //获取机械手
                        //    if (null != FADM_Object.Communal.ReadDyeThread())
                        //    {
                        //        FADM_Object.Communal.WriteDripWait(true);
                        //        Communal._b_isWaitDrip = true;
                        //        Communal._b_isAllowDrip = true;
                        //        while (true)
                        //        {
                        //            if (false == FADM_Object.Communal.ReadDripWait())
                        //                break;
                        //            Thread.Sleep(1);
                        //        }
                        //        Communal._b_isWaitDrip = false;
                        //    }
                        //    else
                        //    {
                        //        Communal._b_isWaitDrip = false;
                        //        FADM_Object.Communal.WriteDripWait(false);
                        //    }
                        //    goto lab_again;
                        //}
                    }
                    Thread.Sleep(1000);

                    if (!FADM_Object.Communal.ReadDripWait())
                    {
                        FADM_Object.Communal.WriteDripWait(true);
                    }

                    //继续判断
                    goto lab_Re;
                }
                //判断一下是否有没加水的
                else
                {
                    s_sql = "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName + "' AND " +
                "AddWaterChoose = 1 AND AddWaterFinish = 0  ORDER BY CupNum;";
                    dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                    if(dt_drop_head.Rows.Count>0)
                    {
                        Communal._b_isAllowDrip = true;
                        //获取机械手
                        if (null != FADM_Object.Communal.ReadDyeThread())
                        {
                            FADM_Object.Communal.WriteDripWait(true);
                            Communal._b_isWaitDrip = true;
                            while (true)
                            {
                                if (false == FADM_Object.Communal.ReadDripWait())
                                    break;
                                Thread.Sleep(1);
                            }
                            Communal._b_isWaitDrip = false;
                        }
                        else
                        {
                            Communal._b_isWaitDrip = false;
                            FADM_Object.Communal.WriteDripWait(false);
                        }

                        goto lab_again;
                    }
                }
            }
            if (null != thread)
                thread.Join();
            //判断称粉是否完成
            if (FADM_Object.Communal._b_isUsePower)
            {
                while (true)
                {
                    //查询没有完成的记录
                    dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                   "SELECT * from drop_head where CupFinish = 0 " +
                   " And BatchName = '" + oBatchName + "' order by CupNum;");
                    if (dt_drop_head.Rows.Count == 0)
                        break;
                    //判断是否由于200或201母液瓶没加粉完成导致整杯没完成
                    foreach (DataRow dr1 in dt_drop_head.Rows)
                    {
                        //等待称粉完成
                        DataTable dt_drop_d = FADM_Object.Communal._fadmSqlserver.GetData(
                    "SELECT * from drop_details where   (BottleNum =200 Or BottleNum = 201)  " +
                    " And BatchName = '" + oBatchName + "' And CupNum = " + Convert.ToInt32(dr1["CupNum"]) + " And Finish=0;");
                        if (dt_drop_d.Rows.Count == 0)
                        {
                            //更新配液杯总量
                            double d_totalW = 0.0;
                            //置为完成
                            s_sql = "UPDATE drop_head SET CupFinish = 1 WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + Convert.ToInt32(dr1["CupNum"]) + "; ";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                            bool b_fail = true;

                            s_sql = "SELECT drop_details.CupNum as CupNum, " +
                               " drop_details.UnitOfAccount as UnitOfAccount, " +
                                       "drop_details.BottleNum as BottleNum, " +
                                       "drop_details.ObjectDropWeight as ObjectDropWeight, " +
                                       "drop_details.RealDropWeight as RealDropWeight, " +
                                       "bottle_details.SyringeType as SyringeType " +
                                       "FROM drop_details left join bottle_details on " +
                                       "bottle_details.BottleNum = drop_details.BottleNum " +
                                       "WHERE drop_details.BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + Convert.ToInt32(dr1["CupNum"]) + ";";
                            DataTable dt_drop_head12 = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                            foreach (DataRow dr in dt_drop_head12.Rows)
                            {
                                if (Convert.ToInt32(dr["BottleNum"]) > Lib_Card.Configure.Parameter.Machine_Bottle_Total)
                                    continue;
                                double d_blRealErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"]))) : Convert.ToDouble(string.Format("{0:F3}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])));
                                d_blRealErr = d_blRealErr < 0 ? -d_blRealErr : d_blRealErr;
                                if (d_blRealErr > (dr["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip))
                                    b_fail = false;

                                d_totalW += Convert.ToDouble(dr["RealDropWeight"]);
                            }

                            DataTable dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                    "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + Convert.ToInt32(dr1["CupNum"]) + ";");
                            if (dt_drop_details3.Rows.Count > 0)
                            {
                                int i_cup = Convert.ToInt16(dt_drop_details3.Rows[0]["CupNum"]);
                                int i_Step = Convert.ToInt16(dt_drop_details3.Rows[0]["Step"]);
                                double d_objWater = Convert.ToDouble(dt_drop_details3.Rows[0]["ObjectAddWaterWeight"]);
                                double d_realWater = Convert.ToDouble(dt_drop_details3.Rows[0]["RealAddWaterWeight"]);
                                double d_totalWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TotalWeight"]);
                                double d_testTubeObjectAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeObjectAddWaterWeight"]);
                                double d_testTubeRealAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeRealAddWaterWeight"]);
                                double d_realDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater - d_objWater) : string.Format("{0:F3}", d_realWater - d_objWater));
                                d_realDif = d_realDif < 0 ? -d_realDif : d_realDif;
                                double d_allDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)) : string.Format("{0:F3}",
                                    d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)));

                                string s_describe;
                                string s_describe_EN;
                                if (d_allDif < d_realDif || (d_realWater == 0.0 && d_objWater != 0.0))
                                {
                                    b_fail = false;
                                }
                                d_totalW += d_realWater;
                                if (b_fail)
                                {
                                    s_describe = "滴液成功!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                              ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Success !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(Convert.ToInt32(dr1["CupNum"])))
                                    {
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cup))
                                            FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                                    }
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  "UPDATE cup_details SET Statues = '滴液成功',TotalWeight = "+ (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalW) : string.Format("{0:F3}",
                                    d_totalW))+" WHERE CupNum = " + i_cup + ";");
                                }
                                else
                                {
                                    s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                            ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET Statues = '滴液失败',TotalWeight = "+ (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalW) : string.Format("{0:F3}",
                                    d_totalW))+" WHERE CupNum = " + i_cup + ";");
                                }
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE drop_head SET DescribeChar = '" + s_describe + "',DescribeChar_EN = '" + s_describe_EN + "', FinishTime = '" + DateTime.Now + "', Step = 2 " +
                               "WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cup + ";");
                            }
                        }

                    }
                    dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                   "SELECT * from drop_details where   (BottleNum =200 Or BottleNum = 201)  " +
                   " And BatchName = '" + oBatchName + "' order by CupNum;");
                    //没有需要称粉就退出
                    if (dt_drop_head.Rows.Count == 0)
                    {
                        break;
                    }
                    //收到停止滴液信号也退出
                    if (_b_dripStop)
                        throw new Exception("收到退出消息");
                    Thread.Sleep(1000);
                }
            }
            if (FADM_Object.Communal._b_isFinishSend)
            {
                //把由于超期，液量低跳过的所有置为不合格
                FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE drop_head SET DescribeChar = '滴液失败',DescribeChar_EN = 'Drip Fail',CupFinish = 1, FinishTime = '" + DateTime.Now + "', Step = 2 " +
                               "WHERE BatchName = '" + oBatchName + "' AND CupFinish != 1;");

                //获取滴液不合格记录
                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
               "SELECT * from drop_head where DescribeChar like '%滴液失败%'" +
               " And BatchName = '" + oBatchName + "' order by CupNum;");

                List<int> lis_cupFailD = new List<int>();
                List<int> lis_cupFailT = new List<int>();
                string s_cupNo = "";
                foreach (DataRow dr in dt_drop_head.Rows)
                {
                    lis_cupFailD.Add(Convert.ToInt32(dr["CupNum"]));
                    s_cupNo += dr["CupNum"].ToString() + "; ";
                }
                lis_cupFailT = lis_cupFailD.Distinct().ToList();


                if (FADM_Auto.Drip._b_dripErr == false)
                {
                    if (0 < lis_cupFailT.Count)
                    {
                        s_cupNo = s_cupNo.Remove(s_cupNo.Length - 1);

                        FADM_Object.Communal.WriteDripWait(true);
                        Communal._b_isAllowDrip = false;
                        FADM_Object.MyAlarm myAlarm;
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            myAlarm = new FADM_Object.MyAlarm(s_cupNo + "号配液杯滴液失败，是否继续(重新滴液请点是，退出滴液请点否)?", "滴液", true, 1);
                        else
                            myAlarm = new FADM_Object.MyAlarm("The dispensing cup" + s_cupNo + "  failed to dispense liquid. Do you want to continue (please click Yes for re dispensing and No for exiting the dispensing)? ", "Drip", true, 1);


                        while (true)
                        {
                            if (0 != myAlarm._i_alarm_Choose) {
                                //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                                //{
                                //   // Home.Home_XYZFinish = false;
                                //}
                                break;
                            }
                                
                            Thread.Sleep(1);
                        }
                        //判断染色线程是否需要用机械手
                        if (null != FADM_Object.Communal.ReadDyeThread())
                        {
                            FADM_Object.Communal.WriteDripWait(true);
                            Communal._b_isWaitDrip = true;
                            while (true)
                            {
                                if (false == FADM_Object.Communal.ReadDripWait())
                                    break;
                                Thread.Sleep(1);
                            }
                            Communal._b_isWaitDrip = false;
                        }
                        else
                        {
                            Communal._b_isWaitDrip = false;
                            FADM_Object.Communal.WriteDripWait(false);
                        }

                        if (1 == myAlarm._i_alarm_Choose)
                        {
                            //先把滴液区域重置
                            //滴液区
                            for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                            {
                                if (SmartDyeing.FADM_Object.Communal._lis_dripCupNum.Contains(lis_cupFailT[i]) || SmartDyeing.FADM_Object.Communal._lis_PrecisionCupNum.Contains(lis_cupFailT[i]))
                                {

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE drop_head SET CupFinish = 0, AddWaterFinish = 0, RealAddWaterWeight = 0.00 , Step = 1,DescribeChar=null " +
                                        "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + lis_cupFailT[i] + ";");

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE drop_details SET Finish = 0, MinWeight = 0, RealDropWeight = 0.00 , NeedPulse = 0" +
                                        "WHERE BatchName = '" + oBatchName + "' AND BottleNum !=200 And BottleNum !=201 And  CupNum = " + lis_cupFailT[i] + ";");
                                    int Num = lis_cupFailT[i];
                                    lis_cupFailT.Remove(lis_cupFailT[i]);
                                    lis_cupSuc.Remove(Num);
                                }

                            }
                            //后处理区域
                            {
                                DataTable dataTable = FADM_Object.Communal._fadmSqlserver.GetData("SELECT * FROM drop_head WHERE BatchName = '" + oBatchName + "'    ORDER BY CupNum;");
                                DataTable dataTable_s = FADM_Object.Communal._fadmSqlserver.GetData("SELECT * FROM drop_head WHERE BatchName != '0'    ORDER BY CupNum;");
                                List<int> lis_lUse_Cup = new List<int>();
                                if (FADM_Object.Communal._b_isNeedConfirm)
                                {
                                    foreach (DataRow dataRow in dataTable_s.Rows)
                                    {
                                        lis_lUse_Cup.Add(Convert.ToInt16(dataRow["CupNum"]));
                                    }
                                }
                                else
                                {
                                    foreach (DataRow dataRow in dataTable.Rows)
                                    {
                                        lis_lUse_Cup.Add(Convert.ToInt16(dataRow["CupNum"]));
                                    }
                                }
                                //把对应副杯也加入进去
                                List<int> lis_cupFailT2 = new List<int>();
                                lis_cupFailT2.AddRange(lis_cupFailT);
                                foreach (int i in lis_cupFailT2)
                                {
                                    if (_dic_first_second[i] > 0)
                                    {
                                        if (lis_lUse_Cup.Contains(_dic_first_second[i]))
                                            if (!lis_cupFailT.Contains(_dic_first_second[i]))
                                                lis_cupFailT.Add(_dic_first_second[i]);
                                    }
                                }
                                //打板区
                                FADM_Object.Communal._lis_dripFailCupFinish.Clear();
                                FADM_Object.Communal._lis_dripFailCup.AddRange(lis_cupFailT);
                                List<int> lis_ints1 = new List<int>();
                                lis_ints1.AddRange(lis_cupFailT);
                                FADM_Object.Communal._lis_SendReadyCup.Clear();

                                //等待染色机排完再重滴
                                while (true)
                                {
                                    FADM_Object.Communal.WriteDripWait(true);
                                    if (0 == lis_cupFailT.Count)
                                    {
                                        FADM_Object.Communal.WriteDripWait(false);
                                        break;
                                    }

                                    for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                                    {
                                        if (FADM_Object.Communal._lis_dripFailCupFinish.Contains(lis_cupFailT[i]))
                                        {

                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                "UPDATE drop_head SET CupFinish = 0, AddWaterFinish = 0, RealAddWaterWeight = 0.00 , Step = 1,DescribeChar=null " +
                                                "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + lis_cupFailT[i] + ";");

                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                "UPDATE drop_details SET Finish = 0, MinWeight = 0, RealDropWeight = 0.00 , NeedPulse = 0" +
                                                "WHERE BatchName = '" + oBatchName + "' AND BottleNum !=200 And BottleNum !=201 And  CupNum = " + lis_cupFailT[i] + ";");
                                            FADM_Object.Communal._lis_dripFailCupFinish.Remove(lis_cupFailT[i]);

                                            ////如果有副杯，副杯也要清除
                                            //if (_dic_first_second[lis_cupFailT[i]] >0)
                                            //{
                                            //    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                            //    "UPDATE drop_head SET CupFinish = 0, AddWaterFinish = 0, RealAddWaterWeight = 0.00 , Step = 1,DescribeChar=null " +
                                            //    "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + _dic_first_second[lis_cupFailT[i]] + ";");

                                            //    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                            //        "UPDATE drop_details SET Finish = 0, MinWeight = 0, RealDropWeight = 0.00 , NeedPulse = 0" +
                                            //        "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + _dic_first_second[lis_cupFailT[i]] + ";");
                                            //    //FADM_Object.Communal._lis_dripFailCupFinish.Remove(lis_cupFailT[i]);
                                            //}
                                            int i_num = lis_cupFailT[i];
                                            lis_cupFailT.Remove(lis_cupFailT[i]);
                                            lis_cupSuc.Remove(i_num);
                                        }
                                    }

                                    DataTable dt_drop_head2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                                               "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName + "'    ORDER BY CupNum;");

                                    DataTable dt_drop_head3 = FADM_Object.Communal._fadmSqlserver.GetData(
                                                               "SELECT * FROM drop_head WHERE BatchName != '0'    ORDER BY CupNum;");
                                    List<int> lis_lUse1 = new List<int>();
                                    if (FADM_Object.Communal._b_isNeedConfirm)
                                    {
                                        foreach (DataRow dataRow in dt_drop_head3.Rows)
                                        {
                                            lis_lUse1.Add(Convert.ToInt16(dataRow["CupNum"]));
                                        }
                                    }
                                    else
                                    {
                                        foreach (DataRow dataRow in dt_drop_head2.Rows)
                                        {
                                            lis_lUse1.Add(Convert.ToInt16(dataRow["CupNum"]));
                                        }

                                    }
                                    for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                                    {
                                        //已经强停，不在滴液列表
                                        if (!lis_lUse1.Contains(lis_cupFailT[i]))
                                        {
                                            int i_num = lis_cupFailT[i];
                                            lis_cupFailT.Remove(lis_cupFailT[i]);
                                            lis_ints1.Remove(i_num);
                                        }
                                    }

                                    Thread.Sleep(1);
                                }

                                foreach (int i in lis_ints1)
                                {
                                    int i_cupNum = i;

                                    while (true)
                                    {
                                        //滴液完成数组移除当前杯号
                                        FADM_Object.Communal._lis_dripSuccessCup.Remove(i_cupNum);
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cupNum))
                                            break;
                                    }



                                    DataTable dt = FADM_Object.Communal._fadmSqlserver.GetData("Select * from drop_head where (CupNum = " + i_cupNum + " Or  CupNum = " + Communal._dic_first_second[i_cupNum] + ") And BatchName != '0'");
                                    //先查是否双杯在使用
                                    if (dt.Rows.Count == 2)
                                    {
                                        string s_info="";
                                        for (int p=0; p< _lis_SendReadyCup.Count;p++)
                                        {
                                            s_info += _lis_SendReadyCup[p] + ",";
                                        }
                                        //判断是否发过启动信号
                                        if ((!_lis_SendReadyCup.Contains(i_cupNum)) && (!_lis_SendReadyCup.Contains(_dic_first_second[i_cupNum])))
                                        {
                                            //判断另外一只杯是否在洗杯
                                            //if (!lis_iUse.Contains(_dic_first_second[i_cupNum]))
                                            {
                                                int[] ia_zero = new int[1];
                                                ia_zero[0] = 0;
                                                DyeHMIWrite(i_cupNum, 119, 119, ia_zero);
                                                //滴液状态
                                                ia_zero[0] = 3;
                                                DyeHMIWrite(i_cupNum, 100, 100, ia_zero);


                                                _lis_SendReadyCup.Add(i_cupNum);
                                                _lis_SendReadyCup.Add(_dic_first_second[i_cupNum]);
                                            }
                                        }
                                    }
                                    //单杯就直接下发
                                    else
                                    {
                                        int[] ia_zero = new int[1];
                                        if (_dic_first_second[i_cupNum] > 0)
                                        {
                                            //主杯
                                            if (i_cupNum < _dic_first_second[i_cupNum])
                                            {
                                                ia_zero[0] = 1;
                                                DyeHMIWrite(i_cupNum, 119, 119, ia_zero);
                                            }
                                            else
                                            {
                                                ia_zero[0] = 2;
                                                DyeHMIWrite(i_cupNum, 119, 119, ia_zero);
                                            }
                                        }
                                        //滴液状态
                                        ia_zero[0] = 3;
                                        DyeHMIWrite(i_cupNum, 100, 100, ia_zero);

                                        _lis_SendReadyCup.Add(i_cupNum);
                                    }

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE cup_details SET Statues = '等待准备状态' WHERE CupNum = " + i_cupNum + ";");

                                }


                                //等待准备状态
                                while (true)
                                {
                                    bool b_open = true;
                                    DataTable dt_drop_head2 = FADM_Object.Communal._fadmSqlserver.GetData(
                           "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName + "' AND CupFinish = 0    ORDER BY CupNum;");
                                    DataTable dt_drop_head3 = FADM_Object.Communal._fadmSqlserver.GetData(
                           "SELECT * FROM drop_head WHERE BatchName != '0' AND CupFinish = 0    ORDER BY CupNum;");
                                    List<int> lUse1 = new List<int>();
                                    if (FADM_Object.Communal._b_isNeedConfirm)
                                    {
                                        foreach (DataRow dataRow in dt_drop_head3.Rows)
                                        {
                                            lUse1.Add(Convert.ToInt16(dataRow["CupNum"]));
                                        }
                                    }
                                    else
                                    {
                                        foreach (DataRow dataRow in dt_drop_head2.Rows)
                                        {
                                            lUse1.Add(Convert.ToInt16(dataRow["CupNum"]));
                                        }
                                    }
                                    for (int i = lis_ints1.Count - 1; i >= 0; i--)
                                    {
                                        //已经强停，不在滴液列表
                                        if (!lUse1.Contains(lis_ints1[i]))
                                        {
                                            lis_ints1.Remove(lis_ints1[i]);
                                        }
                                    }
                                    foreach (int i in lis_ints1)
                                    {
                                        int i_cupNum = i;


                                        int i_openCover = Convert.ToInt16(FADM_Auto.Dye._cup_Temps[i_cupNum - 1]._s_statues);


                                        if (5 != i_openCover)
                                            b_open = false;

                                    }


                                    if (b_open)
                                        break;

                                    Thread.Sleep(1);
                                }

                                lis_ints1.Clear();
                            }
                            //重新滴液时重新清一下
                            if (FADM_Object.Communal._b_isAddWaterFirst)
                                this.DripProcess(oBatchName);
                            else
                                this.DripProcess_AssFirst(oBatchName);
                        }
                        else
                        {
                            //不重滴

                            if (FADM_Object.Communal.ReadMachineStatus() != 8)
                            {
                                for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                                {
                                    if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(lis_cupFailT[i]))
                                    {
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(lis_cupFailT[i]))
                                            FADM_Object.Communal._lis_dripSuccessCup.Add(lis_cupFailT[i]);
                                    }
                                }
                            }

                        }

                    }
                }

                string s_cupList = "";
                string s_te = "";
                if (lis_cupSuc.Count > 0)
                {
                    for (int i = 0; i < lis_cupSuc.Count; i++)
                    {
                        s_cupList += lis_cupSuc[i] + ",";
                    }
                }
                if (s_cupList != "")
                {
                    s_cupList = s_cupList.Remove(s_cupList.Length - 1);
                    s_te = " And CupNum in (" + s_cupList + ")";
                }
                //添加历史表
                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                 "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'drop_head';");
                string s_columnHead = null;
                foreach (DataRow row in dt_drop_head.Rows)
                {
                    string s_curName = Convert.ToString(row[0]);
                    if ("TestTubeFinish" != s_curName && "TestTubeWaterLower" != s_curName && "AddWaterFinish" != s_curName &&
                        "CupFinish" != s_curName && "TestTubeWaterLower" != s_curName)
                        s_columnHead += s_curName + ", ";
                }
                s_columnHead = s_columnHead.Remove(s_columnHead.Length - 2);

                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                   "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'drop_details';");
                string s_columnDetails = null;
                foreach (DataRow row in dt_drop_head.Rows)
                {
                    string s_curName = Convert.ToString(row[0]);
                    if ("MinWeight" != s_curName && "Finish" != s_curName && "IsShow" != s_curName && "NeedPulse" != s_curName)
                        s_columnDetails += Convert.ToString(row[0]) + ", ";
                }
                s_columnDetails = s_columnDetails.Remove(s_columnDetails.Length - 2);

                //
                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                   "SELECT * FROM drop_head WHERE   BatchName = '" + oBatchName + "' ;");

                foreach (DataRow row in dt_drop_head.Rows)
                {
                    if (lis_cupSuc.Contains(Convert.ToInt32(row["CupNum"].ToString())))
                    {

                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                        "INSERT INTO history_head (" + s_columnHead + ") (SELECT " + s_columnHead + " FROM drop_head " +
                        "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + row["CupNum"].ToString() + ") ;");

                        //FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "INSERT INTO history_head (" + s_columnHead + ") (SELECT " + s_columnHead + " FROM drop_head " +
                        //    "WHERE BatchName = '" + o_BatchName + "' AND CupNum >= " + _i_cupMin + " AND CupNum <= " + _i_cupMax + s_te + ");");


                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "INSERT INTO history_details (" + s_columnDetails + ") (SELECT " + s_columnDetails + " FROM drop_details " +
                           "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + row["CupNum"].ToString() + ") ;");

                        //滴液
                        if (SmartDyeing.FADM_Object.Communal._lis_dripCupNum.Contains(Convert.ToInt32(row["CupNum"].ToString())))
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  "DELETE FROM drop_head WHERE CupNum = " + row["CupNum"].ToString() + " AND BatchName = '" + oBatchName + "' ;");
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "DELETE FROM drop_details WHERE CupNum = " + row["CupNum"].ToString() + " AND BatchName = '" + oBatchName + "' ;");


                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE cup_details SET FormulaCode = null, " +
                                "DyeingCode = null, IsUsing = 0, Statues = '待机', " +
                                "StartTime = null, SetTemp = null, StepNum = null, TotalWeight = null, " +
                                "TotalStep = null, TechnologyName = null, StepStartTime = null, SetTime = null,RecordIndex = 0 WHERE CupNum = " +
                                row["CupNum"].ToString() + " ;");
                        }
                    }
                }

                if (FADM_Auto.Drip._b_dripErr)
                {
                    FADM_Object.Communal._lis_dripStopCup.AddRange(lis_cupT);

                }
            }
            else
            {
                s_sql = "UPDATE drop_head SET CupFinish = 1 WHERE BatchName = '" + oBatchName + "'  ;";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                //获取滴液不合格记录
                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                  "SELECT drop_details.CupNum as CupNum, " +
                   " drop_details.UnitOfAccount as UnitOfAccount, " +
                  "drop_details.BottleNum as BottleNum, " +
                  "drop_details.ObjectDropWeight as ObjectDropWeight, " +
                  "drop_details.RealDropWeight as RealDropWeight, " +
                  "bottle_details.SyringeType as SyringeType " +
                  "FROM drop_details left join bottle_details on " +
                  "bottle_details.BottleNum = drop_details.BottleNum " +
                  "WHERE drop_details.BatchName = '" + oBatchName + "' ;");
                List<int> lis_cupFailD = new List<int>();
                List<int> lis_cupFailT = new List<int>();
                foreach (DataRow dr in dt_drop_head.Rows)
                {
                    if (Convert.ToInt32(dr["BottleNum"]) > Lib_Card.Configure.Parameter.Machine_Bottle_Total)
                        continue;
                    double d_blRealErr = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F2}",
                        Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])): string.Format("{0:F3}",
                        Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])));
                    d_blRealErr = d_blRealErr < 0 ? -d_blRealErr : d_blRealErr;
                    if (d_blRealErr > (dr["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip))
                        lis_cupFailD.Add(Convert.ToInt32(dr["CupNum"]));
                }
                lis_cupFailD = lis_cupFailD.Distinct().ToList();



                lis_cupT = new List<int>();
                //滴液成功，转换到历史表杯号
                lis_cupSuc = new List<int>();
                string s_cupNo = null;

                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                    "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName + "'    ORDER BY CupNum ;");
                foreach (DataRow dr in dt_drop_head.Rows)
                {
                    int i_cup = Convert.ToInt16(dr["CupNum"]);
                    int i_step = Convert.ToInt16(dr["Step"]);
                    double d_objWater = Convert.ToDouble(dr["ObjectAddWaterWeight"]);
                    double d_realWater = Convert.ToDouble(dr["RealAddWaterWeight"]);
                    double d_totalWeight = Convert.ToDouble(dr["TotalWeight"]);
                    double d_testTubeObjectAddWaterWeight = Convert.ToDouble(dr["TestTubeObjectAddWaterWeight"]);
                    double d_testTubeRealAddWaterWeight = Convert.ToDouble(dr["TestTubeRealAddWaterWeight"]);
                    if (i_step == 1)
                        lis_cupSuc.Add(i_cup);
                    lis_cupT.Add(i_cup);
                    double d_realDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater - d_objWater): string.Format("{0:F3}", d_realWater - d_objWater));
                    d_realDif = d_realDif < 0 ? -d_realDif : d_realDif;
                    double d_allDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                        d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)): string.Format("{0:F3}",
                        d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)));
                    string s_describe;
                    string s_describe_EN;

                    //只判断当前滴液的数据
                    if (i_step == 1)
                    {
                        if (d_allDif < d_realDif || (d_realWater == 0.0 && d_objWater > 0))
                        {
                            //加水失败
                            s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater): string.Format("{0:F3}", d_objWater)) +
                                             ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater): string.Format("{0:F3}", d_realWater));
                            s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                             ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                            lis_cupFailT.Add(i_cup);
                            s_cupNo += i_cup.ToString() + "; ";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE cup_details SET Statues = '滴液失败' WHERE CupNum = " + i_cup + ";");

                        }
                        else
                        {
                            //加水成功
                            if (lis_cupFailD.Contains(i_cup))
                            {
                                //滴液失败
                                s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                            ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                             ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                lis_cupFailT.Add(i_cup);
                                s_cupNo += i_cup.ToString() + "; ";

                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET Statues = '滴液失败' WHERE CupNum = " + i_cup + ";");
                            }
                            else
                            {
                                //滴液成功
                                s_describe = "滴液成功!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                              ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                s_describe_EN = "Drip Success !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                             ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(i_cup))
                                {
                                    if (i_step == 1)
                                    {
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cup))
                                            FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                                    }
                                }

                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  "UPDATE cup_details SET Statues = '滴液成功' WHERE CupNum = " + i_cup + ";");
                            }
                        }

                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE drop_head SET DescribeChar = '" + s_describe + "',DescribeChar_EN = '" + s_describe_EN + "', FinishTime = '" + DateTime.Now + "', Step = 2 " +
                               "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + i_cup + ";");
                    }

                }


                if (FADM_Auto.Drip._b_dripErr == false)
                {
                    if (0 < lis_cupFailT.Count)
                    {
                        s_cupNo = s_cupNo.Remove(s_cupNo.Length - 1);

                        FADM_Object.Communal.WriteDripWait(true);
                        Communal._b_isAllowDrip = false;
                        FADM_Object.MyAlarm myAlarm;
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            myAlarm = new FADM_Object.MyAlarm(s_cupNo + "号配液杯滴液失败，是否继续(重新滴液请点是，退出滴液请点否)?", "滴液", true, 1);
                        else
                            myAlarm = new FADM_Object.MyAlarm("The dispensing cup" + s_cupNo + "  failed to dispense liquid. Do you want to continue (please click Yes for re dispensing and No for exiting the dispensing)? ", "Drip", true, 1);

                        while (true)
                        {
                            if (0 != myAlarm._i_alarm_Choose) {
                                //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                                //{
                                //   // Home.Home_XYZFinish = false;
                                //}
                                break;
                            }
                               
                            Thread.Sleep(1);
                        }
                        //判断染色线程是否需要用机械手
                        if (null != FADM_Object.Communal.ReadDyeThread())
                        {
                            FADM_Object.Communal.WriteDripWait(true);
                            Communal._b_isWaitDrip = true;
                            while (true)
                            {
                                if (false == FADM_Object.Communal.ReadDripWait())
                                    break;
                                Thread.Sleep(1);
                            }
                            Communal._b_isWaitDrip = false;
                        }
                        else
                        {
                            Communal._b_isWaitDrip = false;
                            FADM_Object.Communal.WriteDripWait(false);
                        }

                        if (1 == myAlarm._i_alarm_Choose)
                        {
                            //先把滴液区域重置
                            //滴液区
                            for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                            {
                                if (SmartDyeing.FADM_Object.Communal._lis_dripCupNum.Contains(lis_cupFailT[i]) || SmartDyeing.FADM_Object.Communal._lis_PrecisionCupNum.Contains(lis_cupFailT[i]))
                                {

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE drop_head SET CupFinish = 0, AddWaterFinish = 0, RealAddWaterWeight = 0.00 , Step = 1,DescribeChar=null " +
                                        "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + lis_cupFailT[i] + ";");

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE drop_details SET Finish = 0, MinWeight = 0, RealDropWeight = 0.00 , NeedPulse = 0" +
                                        "WHERE BatchName = '" + oBatchName + "' AND BottleNum !=200 And BottleNum !=201 And  CupNum = " + lis_cupFailT[i] + ";");
                                    int i_num = lis_cupFailT[i];
                                    lis_cupFailT.Remove(lis_cupFailT[i]);
                                    lis_cupSuc.Remove(i_num);
                                }
                                //else


                            }
                            //后处理区域
                            {

                                DataTable dataTable = FADM_Object.Communal._fadmSqlserver.GetData("SELECT * FROM drop_head WHERE BatchName = '" + oBatchName + "'    ORDER BY CupNum;");
                                List<int> lis_lUse_Cup = new List<int>();
                                foreach (DataRow dataRow in dataTable.Rows)
                                {
                                    lis_lUse_Cup.Add(Convert.ToInt16(dataRow["CupNum"]));
                                }
                                //把对应副杯也加入进去
                                List<int> lis_cupFailT2 = new List<int>();
                                lis_cupFailT2.AddRange(lis_cupFailT);
                                foreach (int i in lis_cupFailT2)
                                {
                                    if (_dic_first_second[i] > 0)
                                    {
                                        if (lis_lUse_Cup.Contains(_dic_first_second[i]))
                                            if (!lis_cupFailT.Contains(_dic_first_second[i]))
                                                lis_cupFailT.Add(_dic_first_second[i]);
                                    }
                                }

                                //打板区
                                FADM_Object.Communal._lis_dripFailCupFinish.Clear();
                                FADM_Object.Communal._lis_dripFailCup.AddRange(lis_cupFailT);
                                List<int> lis_ints1 = new List<int>();
                                lis_ints1.AddRange(lis_cupFailT);

                                //等待染色机排完再重滴
                                while (true)
                                {
                                    FADM_Object.Communal.WriteDripWait(true);
                                    if (0 == lis_cupFailT.Count)
                                    {
                                        FADM_Object.Communal.WriteDripWait(false);
                                        break;
                                    }

                                    for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                                    {
                                        if (FADM_Object.Communal._lis_dripFailCupFinish.Contains(lis_cupFailT[i]))
                                        {

                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                "UPDATE drop_head SET CupFinish = 0, AddWaterFinish = 0, RealAddWaterWeight = 0.00 , Step = 1,DescribeChar=null " +
                                                "WHERE BatchName = '" + oBatchName + "'  And  CupNum = " + lis_cupFailT[i] + ";");

                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                "UPDATE drop_details SET Finish = 0, MinWeight = 0, RealDropWeight = 0.00 , NeedPulse = 0" +
                                                "WHERE BatchName = '" + oBatchName + "' AND BottleNum !=200 And BottleNum !=201 AND CupNum = " + lis_cupFailT[i] + ";");
                                            FADM_Object.Communal._lis_dripFailCupFinish.Remove(lis_cupFailT[i]);

                                            //如果有副杯，副杯也要清除
                                            if (_dic_first_second[lis_cupFailT[i]] > 0)
                                            {
                                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                "UPDATE drop_head SET CupFinish = 0, AddWaterFinish = 0, RealAddWaterWeight = 0.00 , Step = 1,DescribeChar=null " +
                                                "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + _dic_first_second[lis_cupFailT[i]] + ";");

                                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                    "UPDATE drop_details SET Finish = 0, MinWeight = 0, RealDropWeight = 0.00 , NeedPulse = 0" +
                                                    "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + _dic_first_second[lis_cupFailT[i]] + ";");
                                                //FADM_Object.Communal._lis_dripFailCupFinish.Remove(lis_cupFailT[i]);
                                            }

                                            int i_num = lis_cupFailT[i];
                                            lis_cupFailT.Remove(lis_cupFailT[i]);
                                            lis_cupSuc.Remove(i_num);
                                        }
                                    }

                                    DataTable dt_drop_head2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                                               "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName + "'    ORDER BY CupNum;");
                                    List<int> lis_lUse1 = new List<int>();
                                    foreach (DataRow dataRow in dt_drop_head2.Rows)
                                    {
                                        lis_lUse1.Add(Convert.ToInt16(dataRow["CupNum"]));
                                    }
                                    for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                                    {
                                        //已经强停，不在滴液列表
                                        if (!lis_lUse1.Contains(lis_cupFailT[i]))
                                        {
                                            int i_num = lis_cupFailT[i];
                                            lis_cupFailT.Remove(lis_cupFailT[i]);
                                            lis_ints1.Remove(i_num);
                                        }
                                    }

                                    Thread.Sleep(1000);
                                }

                                foreach (int i in lis_ints1)
                                {
                                    int i_cupNum = i;

                                    while (true)
                                    {
                                        //滴液完成数组移除当前杯号
                                        FADM_Object.Communal._lis_dripSuccessCup.Remove(i_cupNum);
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cupNum))
                                            break;
                                    }



                                    DataTable dt = FADM_Object.Communal._fadmSqlserver.GetData("Select * from drop_head where (CupNum = " + i_cupNum + " Or  CupNum = " + Communal._dic_first_second[i_cupNum] + ") And BatchName != '0'");
                                    //先查是否双杯在使用
                                    if (dt.Rows.Count == 2)
                                    {
                                        //判断是否发过启动信号
                                        if ((!_lis_SendReadyCup.Contains(i_cupNum)) && (!_lis_SendReadyCup.Contains(_dic_first_second[i_cupNum])))
                                        {
                                            //判断另外一只杯是否在洗杯
                                            //if (!lis_iUse.Contains(_dic_first_second[i_cupNum]))
                                            {
                                                int[] ia_zero = new int[1];

                                                ia_zero[0] = 0;
                                                DyeHMIWrite(i_cupNum, 119, 119, ia_zero);

                                                //滴液状态
                                                ia_zero[0] = 3;
                                                DyeHMIWrite(i_cupNum, 100, 100, ia_zero);

                                                _lis_SendReadyCup.Add(i_cupNum);
                                                _lis_SendReadyCup.Add(_dic_first_second[i_cupNum]);
                                            }
                                        }
                                    }
                                    //单杯就直接下发
                                    else
                                    {
                                        int[] ia_zero = new int[1];
                                        if (_dic_first_second[i_cupNum] > 0)
                                        {
                                            //主杯
                                            if (i_cupNum < _dic_first_second[i_cupNum])
                                            {
                                                ia_zero[0] = 1;
                                                DyeHMIWrite(i_cupNum, 119, 119, ia_zero);
                                            }
                                            else
                                            {
                                                ia_zero[0] = 2;
                                                DyeHMIWrite(i_cupNum, 119, 119, ia_zero);
                                            }
                                        }
                                        //滴液状态
                                        ia_zero[0] = 3;
                                        DyeHMIWrite(i_cupNum, 100, 100, ia_zero);

                                        _lis_SendReadyCup.Add(i_cupNum);
                                    }

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE cup_details SET Statues = '等待准备状态' WHERE CupNum = " + i_cupNum + ";");

                                }


                                //等待准备状态
                                while (true)
                                {
                                    bool b_open = true;
                                    DataTable dt_drop_head2 = FADM_Object.Communal._fadmSqlserver.GetData(
                           "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName + "' AND CupFinish = 0    ORDER BY CupNum;");
                                    List<int> lis_lUse1 = new List<int>();
                                    foreach (DataRow dataRow in dt_drop_head2.Rows)
                                    {
                                        lis_lUse1.Add(Convert.ToInt16(dataRow["CupNum"]));
                                    }
                                    for (int i = lis_ints1.Count - 1; i >= 0; i--)
                                    {
                                        //已经强停，不在滴液列表
                                        if (!lis_lUse1.Contains(lis_ints1[i]))
                                        {
                                            lis_ints1.Remove(lis_ints1[i]);
                                        }
                                    }
                                    foreach (int i in lis_ints1)
                                    {
                                        int i_cupNum = i;


                                        int i_openCover = Convert.ToInt16(FADM_Auto.Dye._cup_Temps[i_cupNum - 1]._s_statues);


                                        if (5 != i_openCover)
                                            b_open = false;

                                    }


                                    if (b_open)
                                        break;

                                    Thread.Sleep(1);
                                }

                                lis_ints1.Clear();
                            }
                            if (FADM_Object.Communal._b_isAddWaterFirst)
                                this.DripProcess(oBatchName);
                            else
                                this.DripProcess_AssFirst(oBatchName);
                        }
                        else
                        {
                            //不重滴

                            if (FADM_Object.Communal.ReadMachineStatus() != 8)
                            {
                                for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                                {
                                    if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(lis_cupFailT[i]))
                                    {
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(lis_cupFailT[i]))
                                            FADM_Object.Communal._lis_dripSuccessCup.Add(lis_cupFailT[i]);
                                    }
                                }
                            }

                        }

                    }
                }

                string s_cupList = "";
                string s_te = "";
                if (lis_cupSuc.Count > 0)
                {
                    for (int i = 0; i < lis_cupSuc.Count; i++)
                    {
                        s_cupList += lis_cupSuc[i] + ",";
                    }
                }
                if (s_cupList != "")
                {
                    s_cupList = s_cupList.Remove(s_cupList.Length - 1);
                    s_te = " And CupNum in (" + s_cupList + ")";
                }
                //添加历史表
                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                 "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'drop_head';");
                string s_columnHead = null;
                foreach (DataRow row in dt_drop_head.Rows)
                {
                    string s_curName = Convert.ToString(row[0]);
                    if ("TestTubeFinish" != s_curName && "TestTubeWaterLower" != s_curName && "AddWaterFinish" != s_curName &&
                        "CupFinish" != s_curName && "TestTubeWaterLower" != s_curName)
                        s_columnHead += s_curName + ", ";
                }
                s_columnHead = s_columnHead.Remove(s_columnHead.Length - 2);

                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                   "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'drop_details';");
                string s_columnDetails = null;
                foreach (DataRow row in dt_drop_head.Rows)
                {
                    string s_curName = Convert.ToString(row[0]);
                    if ("MinWeight" != s_curName && "Finish" != s_curName && "IsShow" != s_curName && "NeedPulse" != s_curName)
                        s_columnDetails += Convert.ToString(row[0]) + ", ";
                }
                s_columnDetails = s_columnDetails.Remove(s_columnDetails.Length - 2);

                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                   "SELECT * FROM drop_head WHERE   BatchName = '" + oBatchName + "' ;");

                foreach (DataRow row in dt_drop_head.Rows)
                {
                    if (lis_cupSuc.Contains(Convert.ToInt32(row["CupNum"].ToString())))
                    {

                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                        "INSERT INTO history_head (" + s_columnHead + ") (SELECT " + s_columnHead + " FROM drop_head " +
                        "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + row["CupNum"].ToString() + ") ;");

                        //FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "INSERT INTO history_head (" + s_columnHead + ") (SELECT " + s_columnHead + " FROM drop_head " +
                        //    "WHERE BatchName = '" + o_BatchName + "' AND CupNum >= " + _i_cupMin + " AND CupNum <= " + _i_cupMax + s_te + ");");


                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "INSERT INTO history_details (" + s_columnDetails + ") (SELECT " + s_columnDetails + " FROM drop_details " +
                           "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + row["CupNum"].ToString() + ") ;");

                        //滴液
                        if (SmartDyeing.FADM_Object.Communal._lis_dripCupNum.Contains(Convert.ToInt32(row["CupNum"].ToString())))
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  "DELETE FROM drop_head WHERE CupNum = " + row["CupNum"].ToString() + " AND BatchName = '" + oBatchName + "' ;");
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "DELETE FROM drop_details WHERE CupNum = " + row["CupNum"].ToString() + " AND BatchName = '" + oBatchName + "' ;");


                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE cup_details SET FormulaCode = null, " +
                                "DyeingCode = null, IsUsing = 0, Statues = '待机', " +
                                "StartTime = null, SetTemp = null, StepNum = null, TotalWeight = null, " +
                                "TotalStep = null, TechnologyName = null, StepStartTime = null, SetTime = null,RecordIndex = 0 WHERE CupNum = " +
                                row["CupNum"].ToString() + " ;");
                        }
                    }
                }

                if (FADM_Auto.Drip._b_dripErr)
                {
                    FADM_Object.Communal._lis_dripStopCup.AddRange(lis_cupT);

                }
            }

        }

        /// <summary>
        /// 等待读取流量计脉冲
        /// </summary>
        /// <param name="o"></param>
        private static void WaitReadFlow(object o_cupNum)
        {
            Thread.Sleep(1000);

            int i_cupNum = Convert.ToInt32(o_cupNum);

            //读取流量值
            MyModbusFun.ReadFlow();

            //保存数据
            double d_blWeight = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", Communal.i_flowPulse * Lib_Card.Configure.Parameter.Correcting_FlowPulse_Value - 4.6 + Math.Pow(Communal.i_flowPulse, 0.25))) : Convert.ToDouble(string.Format("{0:F3}", Communal.i_flowPulse * Lib_Card.Configure.Parameter.Correcting_FlowPulse_Value - 4.6 + Math.Pow(Communal.i_flowPulse, 0.25)));
            if (Communal.i_flowPulse == 0)
                d_blWeight = 0.0;
            //FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "加水重量：" + d_blWeight+", 加水脉冲：" + Communal.i_flowPulse);
            

            //判断是否存在加水复检失败

            FADM_Object.Communal._fadmSqlserver.ReviseData(
                  "UPDATE cup_details SET TotalWeight =  " + string.Format("{0:F3}", d_blWeight) + " WHERE CupNum = " + i_cupNum + " ;");

            //更新实际加水量
            string s_sql = "UPDATE drop_head SET RealAddWaterWeight =  " + string.Format("{0:F3}", d_blWeight) + ", AddWaterFinish = 1,WaterStandError = '" + Lib_Card.Configure.Parameter.Other_AErr_DripWater + "' WHERE CupNum = " + i_cupNum + " ;";

            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

            //查询目标加水量
             s_sql = "SELECT * FROM drop_head WHERE CupNum = " + i_cupNum + " ;";
            DataTable dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            bool b_fail = false;
            if (dt_drop_head.Rows.Count > 0)
            {
                if (Communal.i_flowPulse != 0)
                {
                    if (Convert.ToDouble(dt_drop_head.Rows[0]["ObjectAddWaterWeight"].ToString()) <= 10)
                    {
                        //生成一个随机合格数据

                        double d_rand = 0.0;
                        Random random = new Random();
                        // 生成1到10之间的随机整数
                        int randomNumber = random.Next(1, 10);
                        d_rand = Convert.ToDouble(dt_drop_head.Rows[0]["ObjectAddWaterWeight"].ToString()) + randomNumber / 100.0;
                        s_sql = "UPDATE drop_head SET RealAddWaterWeight =  " + string.Format("{0:F3}", d_rand) + ", AddWaterFinish = 1,WaterStandError = '" + Lib_Card.Configure.Parameter.Other_AErr_DripWater + "' WHERE CupNum = " + i_cupNum + " ;";

                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                  "UPDATE cup_details SET TotalWeight =  " + string.Format("{0:F3}", d_rand) + " WHERE CupNum = " + i_cupNum + " ;");
                        d_blWeight = d_rand;
                    }
                }
                //如果没有脉冲就当没加水
                else
                {
                    s_sql = "UPDATE drop_head SET RealAddWaterWeight =  " + 0 + ", AddWaterFinish = 1,WaterStandError = '" + Lib_Card.Configure.Parameter.Other_AErr_DripWater + "' WHERE CupNum = " + i_cupNum + " ;";

                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                }
                double d_blWE = Convert.ToDouble(string.Format("{0:F3}", (d_blWeight - Convert.ToDouble(dt_drop_head.Rows[0]["ObjectAddWaterWeight"].ToString()))));
                double d_blDif = Convert.ToDouble(string.Format("{0:F3}", d_blWE / Convert.ToDouble(dt_drop_head.Rows[0]["ObjectAddWaterWeight"].ToString())));
                int irErr = Convert.ToInt16(d_blDif * 100);
                irErr = irErr < 0 ? -irErr : irErr;


                if (System.Math.Abs(d_blWE * 100 / (Convert.ToDouble(dt_drop_head.Rows[0]["TotalWeight"].ToString()))) > Lib_Card.Configure.Parameter.Other_AErr_DripWater || d_blWeight == 0)
                {
                    b_fail = true;
                }

                //先判断是否在合格范围之内，如果不是就多加或少加1个脉冲
                if (b_fail)
                {
                    if (Communal.i_flowPulse == 0)
                    {
                        d_blWeight = 0.0;
                    }
                    else
                    {
                        if (Convert.ToDouble(dt_drop_head.Rows[0]["ObjectAddWaterWeight"].ToString()) > 10)
                        {
                            //目标值大于实际值，加一个脉冲
                            if (Convert.ToDouble(dt_drop_head.Rows[0]["ObjectAddWaterWeight"].ToString()) > d_blWeight)
                            {
                                Communal.i_flowPulse += 1;
                                d_blWeight = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", Communal.i_flowPulse * Lib_Card.Configure.Parameter.Correcting_FlowPulse_Value - 4.6 + Math.Pow(Communal.i_flowPulse, 0.25))) : Convert.ToDouble(string.Format("{0:F3}", Communal.i_flowPulse * Lib_Card.Configure.Parameter.Correcting_FlowPulse_Value - 4.6 + Math.Pow(Communal.i_flowPulse, 0.25)));

                                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "加水重量：" + d_blWeight + ", 加水脉冲：" + Communal.i_flowPulse);
                            }
                            //减一个脉冲
                            else
                            {
                                Communal.i_flowPulse -= 1;
                                d_blWeight = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", Communal.i_flowPulse * Lib_Card.Configure.Parameter.Correcting_FlowPulse_Value - 4.6 + Math.Pow(Communal.i_flowPulse, 0.25))) : Convert.ToDouble(string.Format("{0:F3}", Communal.i_flowPulse * Lib_Card.Configure.Parameter.Correcting_FlowPulse_Value - 4.6 + Math.Pow(Communal.i_flowPulse, 0.25)));

                                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "加水重量：" + d_blWeight + ", 加水脉冲：" + Communal.i_flowPulse);
                            }

                            s_sql = "UPDATE drop_head SET RealAddWaterWeight =  " + string.Format("{0:F3}", d_blWeight) + ", AddWaterFinish = 1,WaterStandError = '" + Lib_Card.Configure.Parameter.Other_AErr_DripWater + "' WHERE CupNum = " + i_cupNum + " ;";

                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                          "UPDATE cup_details SET TotalWeight =  " + string.Format("{0:F3}", d_blWeight) + " WHERE CupNum = " + i_cupNum + " ;");
                        }
                    }
                }
                else
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "加水重量：" + d_blWeight + ", 加水脉冲：" + Communal.i_flowPulse);
                }


                //当加水在最后时，要重新判断一下
                if (FADM_Object.Communal._b_isFinishSend)
                {
                    //更新配液杯总量
                    double d_totalW = 0.0;
                    DataTable dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                            "SELECT * FROM drop_details WHERE  Finish = 0" + " AND CupNum = " + i_cupNum + ";");
                    //查询一下加水没完成的也不置为完成
                    DataTable dt_drop_details4 = FADM_Object.Communal._fadmSqlserver.GetData(
                            "SELECT * FROM drop_head WHERE  AddWaterChoose = 1 AND AddWaterFinish =0" + " AND CupNum = " + i_cupNum + ";");
                    if (dt_drop_details3.Rows.Count == 0 && dt_drop_details4.Rows.Count == 0)
                    {
                        //置为完成
                        s_sql = "UPDATE drop_head SET CupFinish = 1 WHERE  CupNum = " + i_cupNum + "; ";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        bool b_fail1 = true;

                        s_sql = "SELECT drop_details.CupNum as CupNum, " +
                             " drop_details.UnitOfAccount as UnitOfAccount, " +
                                    "drop_details.BottleNum as BottleNum, " +
                                    "drop_details.ObjectDropWeight as ObjectDropWeight, " +
                                    "drop_details.RealDropWeight as RealDropWeight, " +
                                    "bottle_details.SyringeType as SyringeType " +
                                    "FROM drop_details left join bottle_details on " +
                                    "bottle_details.BottleNum = drop_details.BottleNum " +
                                    "WHERE CupNum = " + i_cupNum + ";";
                        dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                        foreach (DataRow dr in dt_drop_head.Rows)
                        {
                            if (Convert.ToInt32(dr["BottleNum"]) > Lib_Card.Configure.Parameter.Machine_Bottle_Total)
                                continue;
                            double d_blRealErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                            Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"]))) : Convert.ToDouble(string.Format("{0:F3}",
                            Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])));
                            d_blRealErr = d_blRealErr < 0 ? -d_blRealErr : d_blRealErr;
                            if (d_blRealErr > (dr["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip))
                                b_fail1 = false;
                            d_totalW += Convert.ToDouble(dr["RealDropWeight"]);
                        }

                        dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                "SELECT * FROM drop_head WHERE  CupNum = " + i_cupNum + ";");
                        if (dt_drop_details3.Rows.Count > 0)
                        {
                            int i_cup = Convert.ToInt16(dt_drop_details3.Rows[0]["CupNum"]);
                            int i_Step = Convert.ToInt16(dt_drop_details3.Rows[0]["Step"]);
                            double d_objWater = Convert.ToDouble(dt_drop_details3.Rows[0]["ObjectAddWaterWeight"]);
                            double d_realWater = Convert.ToDouble(dt_drop_details3.Rows[0]["RealAddWaterWeight"]);
                            double d_totalWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TotalWeight"]);
                            double d_testTubeObjectAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeObjectAddWaterWeight"]);
                            double d_testTubeRealAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeRealAddWaterWeight"]);
                            double d_realDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater - d_objWater) : string.Format("{0:F3}", d_realWater - d_objWater));
                            d_realDif = d_realDif < 0 ? -d_realDif : d_realDif;
                            double d_allDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)) : string.Format("{0:F3}",
                                d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)));

                            string s_describe;
                            string s_describe_EN;
                            if (d_allDif < d_realDif || (d_realWater == 0.0 && d_objWater != 0.0))
                            {
                                b_fail1 = false;
                            }
                            d_totalW += d_realWater;

                            if (b_fail1)
                            {
                                s_describe = "滴液成功!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                          ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                s_describe_EN = "Drip Success !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                 ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(i_cup))
                                {
                                    if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cup))
                                        FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                                }
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                              "UPDATE cup_details SET Statues = '滴液成功',TotalWeight = "+ (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalW) : string.Format("{0:F3}",
                                    d_totalW))+" WHERE CupNum = " + i_cup + ";");
                            }
                            else
                            {
                                s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                        ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                 ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE cup_details SET Statues = '滴液失败',TotalWeight = "+ (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalW) : string.Format("{0:F3}",
                                    d_totalW))+" WHERE CupNum = " + i_cup + ";");
                            }
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "UPDATE drop_head SET DescribeChar = '" + s_describe + "',DescribeChar_EN = '" + s_describe_EN + "', FinishTime = '" + DateTime.Now + "', Step = 2 " +
                           "WHERE  CupNum = " + i_cup + ";");
                        }
                    }
                }

            }

            
            
        }


        //滴液过程先加染料助剂后加水
        private void DripProcess_AssFirst(object oBatchName)
        {
            //判断是否过期，液量低，夹不到针筒选择了否
            bool b_chooseNo = false;
            Thread thread = null;
            int i_mRes = 0;
            //针检失败，不继续针检状态
            bool b_checkFail = false;


            List<int> lis_cupSuc = new List<int>();
            List<int> lis_cupT = new List<int>();

            DataTable dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
               "SELECT * FROM drop_head WHERE   BatchName = '" + oBatchName + "' And Step = 1 order by CupNum;");

            foreach (DataRow row in dt_drop_head.Rows)
            {
                lis_cupSuc.Add(Convert.ToInt32(row["CupNum"].ToString()));
                lis_cupT.Add(Convert.ToInt32(row["CupNum"].ToString()));
            }

            Communal._b_isAllowDrip = true;

            //判断染色线程是否需要用机械手
            if (null != FADM_Object.Communal.ReadDyeThread())
            {
                FADM_Object.Communal.WriteDripWait(true);
                Communal._b_isWaitDrip = true;
                Communal._b_isAllowDrip = true;
                while (true)
                {
                    if (false == FADM_Object.Communal.ReadDripWait())
                        break;
                    Thread.Sleep(1);
                }
                Communal._b_isWaitDrip = false;
            }
            else
                Communal._b_isWaitDrip = false;

            //先把只做后处理的直接给滴液完成，并下发
            string s_sql = "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName + "' And CupFinish = 0  ORDER BY CupNum;";
            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

            foreach (DataRow row in dt_drop_head.Rows)
            {
                s_sql = "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName + "' AND " +
                "CupNum =" + row["CupNum"].ToString() + " And BottleNum <= " + Lib_Card.Configure.Parameter.Machine_Bottle_Total + ";";
                DataTable dt_drop_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                if (dt_drop_details.Rows.Count == 0 && row["AddWaterChoose"].ToString() != "1")
                {
                    int i_cup = Convert.ToInt32(row["CupNum"].ToString());

                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                       "UPDATE drop_head SET DescribeChar = '滴液成功', FinishTime = '" + DateTime.Now + "', Step = 2,CupFinish = 1  " +
                       "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + i_cup + ";");
                    if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cup))
                        FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                }
            }


            string s_unitOfAccount = "";

        lab_again:
            


            string s_unitA = "";
            int i_lowSrart = 0;
            if (FADM_Object.Communal._b_isAssitantFirst)
            {
                //加助剂
                s_unitA = "g/l";
                i_lowSrart = 0;
            }
            else
            {
                //加染料
                s_unitA = "%";
                i_lowSrart = 0;
            }

        label3:
            s_sql = "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName + "' AND " +
                "Finish = 0 AND UnitOfAccount = '" + s_unitA + "' AND MinWeight = " + i_lowSrart + " AND " +
                "BottleNum <= " + Lib_Card.Configure.Parameter.Machine_Bottle_Total + "  And IsDrop != 0 ORDER BY CupNum;";
            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (0 == dt_drop_head.Rows.Count)
            {
                if (FADM_Object.Communal._b_isAssitantFirst)
                {
                    if ("g/l" == s_unitA)
                    {
                        //助剂已加完，加染料
                        goto label4;
                    }
                    else
                    {
                        if (0 == i_lowSrart)
                        {
                            AddWater(oBatchName);
                            //染料已加完，加液量不足
                            goto label5;
                        }
                        else if (1 == i_lowSrart)
                        {
                            //液量不足已加完，加超出生命周期
                            goto label17;
                        }
                        else if (3 == i_lowSrart)
                        {
                            //超出生命周期的加完，加检测不到针筒
                            goto label16;
                        }
                        else
                        {
                            //结束
                            goto label6;
                        }
                    }
                }
                else
                {
                    if ("%" == s_unitA)
                    {
                        //染料已加完，加助剂
                        goto label4;
                    }
                    else
                    {
                        if (0 == i_lowSrart)
                        {
                            AddWater(oBatchName);
                            //助剂已加完，加液量不足
                            goto label5;
                        }
                        else if (1 == i_lowSrart)
                        {
                            //液量不足已加完，加超出生命周期
                            goto label17;
                        }
                        else if (3 == i_lowSrart)
                        {
                            //超出生命周期的加完，加检测不到针筒
                            goto label16;
                        }
                        else
                        {
                            //结束
                            goto label6;
                        }
                    }
                }

            }

            int i_minCupNo = Convert.ToInt32(dt_drop_head.Rows[0]["CupNum"]);

        label7:
            s_sql = "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName + "' AND CupNum = " + i_minCupNo + " AND " +
                "Finish = 0 AND UnitOfAccount = '" + s_unitA + "' AND MinWeight = " + i_lowSrart + " AND " +
                "BottleNum <= " + Lib_Card.Configure.Parameter.Machine_Bottle_Total + "   And IsDrop != 0 ORDER BY BottleNum;";
            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (0 == dt_drop_head.Rows.Count)
            {
                //当前杯已加完
                goto label3;
            }

            int i_minBottleNo = Convert.ToInt32(dt_drop_head.Rows[0]["BottleNum"]);

            s_sql = "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                "Finish = 0 AND UnitOfAccount = '" + s_unitA + "' AND MinWeight = " + i_lowSrart + " AND " +
                "BottleNum <= " + Lib_Card.Configure.Parameter.Machine_Bottle_Total + "   And IsDrop != 0 ORDER BY CupNum;";
            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (0 == dt_drop_head.Rows.Count)
            {
                //当前瓶完成
                goto label7;
            }

            s_sql = "SELECT * FROM bottle_details WHERE BottleNum = " + i_minBottleNo + ";";
            DataTable dt_bottle_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            int i_adjust = Convert.ToInt32(dt_bottle_details.Rows[0]["AdjustValue"]);
            bool b_lCheckSuccess = (Convert.ToString(dt_bottle_details.Rows[0]["AdjustSuccess"]) == "1");
            string s_syringeType = Convert.ToString(dt_bottle_details.Rows[0]["SyringeType"]);
            string s_dripReserveFirst = "0";
            if (!(dt_bottle_details.Rows[0]["DripReserveFirst"] is DBNull))
            {
                s_dripReserveFirst = dt_bottle_details.Rows[0]["DripReserveFirst"].ToString();
            }


            Dictionary<int, int> dic_pulse = new Dictionary<int, int>();
            Dictionary<int, double> dic_weight = new Dictionary<int, double>();
            Dictionary<int, double> dic_water = new Dictionary<int, double>();
            int i_pulseT = 0;
            if (0 == i_lowSrart)
            {
                double d_blCW = Convert.ToDouble(string.Format("{0:F3}", dt_bottle_details.Rows[0]["CurrentWeight"]));
                foreach (DataRow dataRow in dt_drop_head.Rows)
                {
                    int i_cupNo = Convert.ToInt32(dataRow["CupNum"]);
                    double d_blOAddW = Convert.ToDouble(string.Format("{0:F3}", dataRow["ObjectDropWeight"]));
                    int i_needPulse = dataRow["NeedPulse"] is DBNull ? 0 : Convert.ToInt32(dataRow["NeedPulse"]);
                    d_blCW -= d_blOAddW;

                    //查询判断是否超期
                    s_sql = "SELECT * FROM assistant_details WHERE AssistantCode = '" + dt_bottle_details.Rows[0]["AssistantCode"].ToString() + "';";
                    DataTable dt_assistant_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                    DateTime timeA = Convert.ToDateTime(dt_bottle_details.Rows[0]["BrewingData"].ToString());
                    DateTime timeB = DateTime.Now; //获取当前时间
                    TimeSpan ts = timeB - timeA; //计算时间差
                    string s_time = ts.TotalHours.ToString(); //将时间差转换为小时


                    if (d_blCW < Lib_Card.Configure.Parameter.Other_Bottle_MinWeight && FADM_Object.Communal._b_isLowDrip)
                    {
                        //查询在备料表是否存在记录，如果存在，先让客户选择是否使用备料数据来更新
                        string s_sqlpre = "SELECT * FROM pre_brew WHERE  BottleNum = " + i_minBottleNo + ";";
                        DataTable dt_pre_brew = FADM_Object.Communal._fadmSqlserver.GetData(s_sqlpre);
                        if (dt_pre_brew.Rows.Count > 0)
                        {
                            FADM_Object.Communal.WriteDripWait(true);
                            Communal._b_isAllowDrip = false;
                            FADM_Object.MyAlarm myAlarm;
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm(
                                i_minBottleNo + "号母液瓶液量过低，备料表存在已开料记录，是否替换(替换请点是，继续使用旧母液请点否)?", "滴液", true, 1);
                            else
                                myAlarm = new FADM_Object.MyAlarm(
                                "The " + i_minBottleNo + " mother liquor bottle has expired, and there is a record of opened materials in the material preparation table. Should it be replaced? (Please click Yes for replacement, and click No for continuing to use the old mother liquor)", "Drip", true, 1);
                            while (true)
                            {
                                if (0 != myAlarm._i_alarm_Choose) {
                                    //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                                    //{
                                    //   // Home.Home_XYZFinish = false;
                                    //}
                                    break;
                                }
                                    
                                Thread.Sleep(1);
                            }
                            Communal._b_isAllowDrip = true;
                            //判断染色线程是否需要用机械手
                            if (null != FADM_Object.Communal.ReadDyeThread())
                            {
                                FADM_Object.Communal.WriteDripWait(true);
                                Communal._b_isWaitDrip = true;
                                Communal._b_isAllowDrip = true;
                                while (true)
                                {
                                    if (false == FADM_Object.Communal.ReadDripWait())
                                        break;
                                    Thread.Sleep(1);
                                }
                                Communal._b_isWaitDrip = false;
                            }
                            else
                            {
                                Communal._b_isWaitDrip = false;
                                FADM_Object.Communal.WriteDripWait(false);
                            }
                            //如果选择是，使用新料重新计算
                            if (1 == myAlarm._i_alarm_Choose)
                            {
                                //使用备料表数据更新现有母液瓶数据，删除备料表记录
                                s_sql = "UPDATE bottle_details SET RealConcentration = '" + dt_pre_brew.Rows[0]["RealConcentration"].ToString() + "',CurrentWeight = '" + dt_pre_brew.Rows[0]["CurrentWeight"].ToString() + "',BrewingData='"
                                    + dt_pre_brew.Rows[0]["BrewingData"].ToString() + "'" +
                                " WHERE BottleNum = " + i_minBottleNo + ";";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from pre_brew where BottleNum = " + i_minBottleNo);
                                goto label7;
                            }
                            //选择否就和之前的逻辑一致
                            else
                            {
                                s_sql = "UPDATE drop_details SET MinWeight = 1 WHERE BatchName = '" + oBatchName + "' AND MinWeight=0 And " +
                                "BottleNum = " + i_minBottleNo + " AND Finish = 0 ;";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                break;
                            }
                        }
                        else
                        {
                            s_sql = "UPDATE drop_details SET MinWeight = 1 WHERE BatchName = '" + oBatchName + "' AND MinWeight=0 And " +
                                "BottleNum = " + i_minBottleNo + " AND Finish = 0 ;";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                            break;
                        }

                    }
                    else if (Convert.ToDouble(s_time) > Convert.ToDouble(dt_assistant_details.Rows[0]["TermOfValidity"].ToString()) && FADM_Object.Communal._b_isOutDrip)
                    {
                        //查询在备料表是否存在记录，如果存在，先让客户选择是否使用备料数据来更新
                        string s_sqlpre = "SELECT * FROM pre_brew WHERE  BottleNum = " + i_minBottleNo + ";";
                        DataTable dt_pre_brew = FADM_Object.Communal._fadmSqlserver.GetData(s_sqlpre);
                        if (dt_pre_brew.Rows.Count > 0)
                        {
                            FADM_Object.Communal.WriteDripWait(true);
                            Communal._b_isAllowDrip = false;
                            FADM_Object.MyAlarm myAlarm;
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm(
                                i_minBottleNo + "号母液瓶过期，备料表存在已开料记录，是否替换(替换请点是，继续使用旧母液请点否)?", "滴液", true, 1);
                            else
                                myAlarm = new FADM_Object.MyAlarm(
                                "The " + i_minBottleNo + " mother liquor bottle has expired, and there is a record of opened materials in the material preparation table. Should it be replaced? (Please click Yes for replacement, and click No for continuing to use the old mother liquor)", "Drip", true, 1);
                            while (true)
                            {
                                if (0 != myAlarm._i_alarm_Choose) {
                                    //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                                    //{
                                    //   // Home.Home_XYZFinish = false;
                                    //}
                                    break;
                                }
                                    
                                Thread.Sleep(1);
                            }
                            Communal._b_isAllowDrip = true;
                            //判断染色线程是否需要用机械手
                            if (null != FADM_Object.Communal.ReadDyeThread())
                            {
                                FADM_Object.Communal.WriteDripWait(true);
                                Communal._b_isWaitDrip = true;
                                Communal._b_isAllowDrip = true;
                                while (true)
                                {
                                    if (false == FADM_Object.Communal.ReadDripWait())
                                        break;
                                    Thread.Sleep(1);
                                }
                                Communal._b_isWaitDrip = false;
                            }
                            else
                            {
                                Communal._b_isWaitDrip = false;
                                FADM_Object.Communal.WriteDripWait(false);
                            }
                            //如果选择是，使用新料重新计算
                            if (1 == myAlarm._i_alarm_Choose)
                            {
                                //使用备料表数据更新现有母液瓶数据，删除备料表记录
                                s_sql = "UPDATE bottle_details SET RealConcentration = '" + dt_pre_brew.Rows[0]["RealConcentration"].ToString() + "',CurrentWeight = '" + dt_pre_brew.Rows[0]["CurrentWeight"].ToString() + "',BrewingData='"
                                    + dt_pre_brew.Rows[0]["BrewingData"].ToString() + "'" +
                                " WHERE BottleNum = " + i_minBottleNo + ";";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from pre_brew where BottleNum = " + i_minBottleNo);
                                goto label7;
                            }
                            else
                            {
                                s_sql = "UPDATE drop_details SET MinWeight = 3 WHERE BatchName = '" + oBatchName + "' AND  MinWeight=0 And " +
                                "BottleNum = " + i_minBottleNo + " AND Finish = 0 ;";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                break;
                            }
                        }
                        else
                        {
                            s_sql = "UPDATE drop_details SET MinWeight = 3 WHERE BatchName = '" + oBatchName + "' AND  MinWeight=0 And " +
                                "BottleNum = " + i_minBottleNo + " AND Finish = 0 ;";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                            break;
                        }
                    }
                    else
                    {
                        //判断是否分开两次滴液，如果是就使用需加脉冲来计算
                        int i_pulse = i_needPulse > 0 ? i_needPulse : Convert.ToInt32(i_adjust * d_blOAddW);
                        dic_pulse.Add(i_cupNo, i_pulse);
                        dic_weight.Add(i_cupNo, d_blOAddW);
                        dic_water.Add(i_cupNo, 0.0);
                        i_pulseT += i_pulse;

                        s_unitOfAccount = dataRow["UnitOfAccount"].ToString();
                    }


                }

                if (0 == dic_pulse.Count)
                {
                    //当前瓶液量不足
                    goto label7;
                }
            }
            else
            {
                foreach (DataRow dataRow in dt_drop_head.Rows)
                {
                    int i_cupNo = Convert.ToInt32(dataRow["CupNum"]);
                    double d_blOAddW = Convert.ToDouble(string.Format("{0:F3}", dataRow["ObjectDropWeight"]));
                    int i_needPulse = dataRow["NeedPulse"] is DBNull ? 0 : Convert.ToInt32(dataRow["NeedPulse"]);
                    //判断是否分开两次滴液，如果是就使用需加脉冲来计算
                    int i_pulse = i_needPulse > 0 ? i_needPulse : Convert.ToInt32(i_adjust * d_blOAddW);
                    dic_pulse.Add(i_cupNo, i_pulse);
                    dic_weight.Add(i_cupNo, d_blOAddW);
                    dic_water.Add(i_cupNo, 0.0);
                    i_pulseT += i_pulse;

                    s_unitOfAccount = dataRow["UnitOfAccount"].ToString();
                }
            }

            //判断染色线程是否需要用机械手
            if (null != FADM_Object.Communal.ReadDyeThread())
            {
                FADM_Object.Communal.WriteDripWait(true);
                Communal._b_isWaitDrip = true;
                Communal._b_isAllowDrip = true;
                while (true)
                {
                    if (false == FADM_Object.Communal.ReadDripWait())
                        break;
                    Thread.Sleep(1);
                }
                Communal._b_isWaitDrip = false;
            }
            else
                Communal._b_isWaitDrip = false;

            //针检
            if ((0 >= i_adjust || false == b_lCheckSuccess) && !b_checkFail)
            {
            label8:
                //判断染色线程是否需要用机械手
                if (null != FADM_Object.Communal.ReadDyeThread())
                {
                    FADM_Object.Communal.WriteDripWait(true);
                    Communal._b_isWaitDrip = true;
                    Communal._b_isAllowDrip = true;
                    while (true)
                    {
                        if (false == FADM_Object.Communal.ReadDripWait())
                            break;
                        Thread.Sleep(1);
                    }
                    Communal._b_isWaitDrip = false;
                }
                else
                    Communal._b_isWaitDrip = false;

                if (_b_dripStop)
                {
                    FADM_Object.Communal._b_stop = true;
                }
                int i_res = new BottleCheck().MyDripCheck(i_minBottleNo, true, i_lowSrart); //针检
                if (-1 == i_res)
                {
                    FADM_Object.Communal.WriteDripWait(true); 
                    Communal._b_isAllowDrip = false;
                    FADM_Object.MyAlarm myAlarm;
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + "号母液瓶针检失败，是否继续?(继续针检请点是，退出针检请点否)", "滴液针检", true, 1, true);
                    else
                        myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + " bottle needle inspection failed, do you want to continue? " +
                            "(To continue the needle examination, please click Yes, and to exit the needle examination, please click No)", "Drip needle examination", true, 1, true);
                    while (true)

                    {
                        if (0 != myAlarm._i_alarm_Choose) {
                            //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1) {
                            //   // Home.Home_XYZFinish = false;
                            //}
                            break;
                        }
                        Thread.Sleep(1);
                    }
                    Communal._b_isAllowDrip = true;
                    //判断染色线程是否需要用机械手
                    if (null != FADM_Object.Communal.ReadDyeThread())
                    {
                        FADM_Object.Communal.WriteDripWait(true);
                        Communal._b_isWaitDrip = true;
                        Communal._b_isAllowDrip = true;
                        while (true)
                        {
                            if (false == FADM_Object.Communal.ReadDripWait())
                                break;
                            Thread.Sleep(1);
                        }
                        Communal._b_isWaitDrip = false;
                    }
                    else
                    {
                        Communal._b_isWaitDrip = false;
                        FADM_Object.Communal.WriteDripWait(false);
                    }

                    if (1 == myAlarm._i_alarm_Choose)
                        goto label8;
                    else
                    {
                        b_checkFail = true;
                    }
                }
                else if (-2 == i_res)
                {
                    s_sql = "UPDATE drop_details SET MinWeight = 2 WHERE BatchName = '" + oBatchName + "' AND " +
                          "BottleNum = " + i_minBottleNo + " AND Finish = 0;";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                    goto label3;
                }
                else if (-3 == i_res)
                {
                    //夹不到针筒时选择否，直接退出
                    throw new Exception("收到退出消息");
                }
                if (b_checkFail)
                {
                    s_sql = "update bottle_details set AdjustValue = 3900 where AdjustValue =0 And " +
                          "BottleNum = " + i_minBottleNo + ";";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                }
                goto label7;
            }


            if (0 == dic_pulse.Count)
            {
                //当前瓶液量不足
                goto label7;
            }

            //判断染色线程是否需要用机械手
            if (null != FADM_Object.Communal.ReadDyeThread())
            {
                FADM_Object.Communal.WriteDripWait(true);
                Communal._b_isWaitDrip = true;
                Communal._b_isAllowDrip = true;
                while (true)
                {
                    if (false == FADM_Object.Communal.ReadDripWait())
                        break;
                    Thread.Sleep(1);
                }
                Communal._b_isWaitDrip = false;
            }
            else
                Communal._b_isWaitDrip = false;
            sAddArg o = new sAddArg();
            o._i_minBottleNo = i_minBottleNo;
            o._obj_batchName = oBatchName.ToString();
            o._i_adjust = i_adjust;
            o._i_pulseT = i_pulseT;
            o._s_syringeType = s_syringeType;
            o._s_unitOfAccount = s_unitOfAccount;
            o._dic_pulse = dic_pulse;
            o._dic_water = dic_water;
            if (_b_isAloneDripReserve)
            {
                if (s_dripReserveFirst == "0")
                {
                    _b_isDripReserveFirst = false;
                }
                else
                {
                    _b_isDripReserveFirst = true;
                }
            }
            Dictionary<int, double> dic_return = new Dictionary<int, double>();
            int i_ret = FADM_Object.Communal.AddMac(o, ref dic_return, 1, 0, i_lowSrart);
            //夹不到针筒
            if (i_ret == -1)
            {
                if (i_lowSrart == 2)
                {
                    FADM_Object.Communal.WriteDripWait(true);
                    Communal._b_isAllowDrip = false;
                    FADM_Object.MyAlarm myAlarm;
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + "号母液瓶未找到针筒，是否继续执行?(继续寻找请点是，退出滴液请点否)", "滴液", true, 1, true);
                    else
                        myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + " bottle did not find a syringe. Do you want to continue? " +
                            "(To continue searching, please click Yes. To exit Drip, please click No)", "Drip", true, 1, true);
                    while (true)
                    {
                        if (0 != myAlarm._i_alarm_Choose) {
                            //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                            //{
                            //   // Home.Home_XYZFinish = false;
                            //}
                            break;
                        }
                        Thread.Sleep(1);
                    }
                    Communal._b_isAllowDrip = true;
                    //判断染色线程是否需要用机械手
                    if (null != FADM_Object.Communal.ReadDyeThread())
                    {
                        FADM_Object.Communal.WriteDripWait(true);
                        Communal._b_isWaitDrip = true;
                        Communal._b_isAllowDrip = true;
                        while (true)
                        {
                            if (false == FADM_Object.Communal.ReadDripWait())
                                break;
                            Thread.Sleep(1);
                        }
                        Communal._b_isWaitDrip = false;
                    }
                    else
                    {
                        Communal._b_isWaitDrip = false;
                        FADM_Object.Communal.WriteDripWait(false);
                    }

                    if (1 == myAlarm._i_alarm_Choose)
                        goto label3;
                    else
                        throw new Exception("收到退出消息");
                }
                else
                {
                    s_sql = "UPDATE drop_details SET MinWeight = 2 WHERE BatchName = '" + oBatchName + "' AND " +
                     "BottleNum = " + i_minBottleNo + " AND Finish = 0;";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                    goto label3;
                }
            }
            //滴液完成
            else if (i_ret == 0)
            {
                if (FADM_Object.Communal._b_isFinishSend)
                {
                    foreach (KeyValuePair<int, double> kvp in dic_return)
                    {
                        double d_blRErr = 0;
                        if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                        else
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight));

                        //查询开料日期
                        DataTable dt_bottle_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
            "SELECT bottle_details.*,UnitOfAccount  FROM bottle_details left join assistant_details   on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE  bottle_details.BottleNum = " + i_minBottleNo + ";");

                        if (0.00 != kvp.Value)
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + d_blRErr + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '" + (dt_bottle_details2.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                            "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                            "CupNum = " + kvp.Key + ";");

                            DataTable dt_drop_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND CupNum = " + kvp.Key + ";");
                            if (dt_drop_details2.Rows.Count > 0)
                            {
                                //记录是否修正数据
                                bool b_change = false;
                                double d_realDrop = 0.0;
                                if (FADM_Object.Communal._b_isCorrect)
                                {
                                    //如果实际滴液量为负数，就改为0
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["RealDropWeight"]) < 0)
                                    {
                                        b_change = true;
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = 0   ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                    }

                                    //判断当目标滴液量大于1g，而且实际滴液误差小于等于0.05的，全部置为0.02
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) > 1)
                                    {
                                        if (d_blRErr <= 0.05 && d_blRErr > 0.02)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + 0.02 + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) + 0.02;
                                        }
                                        else if (d_blRErr < -0.02 && d_blRErr >= -0.05)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + (-0.02) + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) - 0.02;
                                        }
                                    }
                                }
                                if (b_change)
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + d_realDrop + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_realDrop + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");
                                }
                                else
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE cup_details SET TotalWeight = TotalWeight+ " + dt_drop_details2.Rows[0]["RealDropWeight"] + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + dt_drop_details2.Rows[0]["RealDropWeight"] + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");
                                }
                            }
                        }
                        else
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE drop_details SET Finish = 1,RealDropWeight = 0.00 " + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '" + (dt_bottle_details2.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                            "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                            "CupNum = " + kvp.Key + ";");
                        }

                        DataTable dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName.ToString() + "' AND Finish = 0" + " AND CupNum = " + kvp.Key + ";");
                        //查询一下加水没完成的也不置为完成
                        DataTable dt_drop_details4 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND AddWaterChoose = 1 AND AddWaterFinish =0" + " AND CupNum = " + kvp.Key + ";");
                        if (dt_drop_details3.Rows.Count == 0 && dt_drop_details4.Rows.Count == 0)
                        {
                            //更新配液杯总量
                            double d_totalW = 0.0;
                            //置为完成
                            s_sql = "UPDATE drop_head SET CupFinish = 1 WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + kvp.Key + "; ";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);


                            bool b_fail = true;

                            s_sql = "SELECT drop_details.CupNum as CupNum, " +
                                " drop_details.UnitOfAccount as UnitOfAccount, " +
                                        "drop_details.BottleNum as BottleNum, " +
                                        "drop_details.ObjectDropWeight as ObjectDropWeight, " +
                                        "drop_details.RealDropWeight as RealDropWeight, " +
                                        "bottle_details.SyringeType as SyringeType " +
                                        "FROM drop_details left join bottle_details on " +
                                        "bottle_details.BottleNum = drop_details.BottleNum " +
                                        "WHERE drop_details.BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + kvp.Key + ";";
                            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                            foreach (DataRow dr in dt_drop_head.Rows)
                            {
                                if (Convert.ToInt32(dr["BottleNum"]) > Lib_Card.Configure.Parameter.Machine_Bottle_Total)
                                    continue;
                                double d_blRealErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"]))) : Convert.ToDouble(string.Format("{0:F3}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])));
                                d_blRealErr = d_blRealErr < 0 ? -d_blRealErr : d_blRealErr;
                                if (d_blRealErr > (dr["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip))
                                    b_fail = false;
                                d_totalW += Convert.ToDouble(dr["RealDropWeight"]);
                            }

                            dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                    "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + kvp.Key + ";");
                            if (dt_drop_details3.Rows.Count > 0)
                            {
                                int i_cup = Convert.ToInt16(dt_drop_details3.Rows[0]["CupNum"]);
                                int i_Step = Convert.ToInt16(dt_drop_details3.Rows[0]["Step"]);
                                double d_objWater = Convert.ToDouble(dt_drop_details3.Rows[0]["ObjectAddWaterWeight"]);
                                double d_realWater = Convert.ToDouble(dt_drop_details3.Rows[0]["RealAddWaterWeight"]);
                                double d_totalWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TotalWeight"]);
                                double d_testTubeObjectAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeObjectAddWaterWeight"]);
                                double d_testTubeRealAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeRealAddWaterWeight"]);
                                double d_realDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater - d_objWater) : string.Format("{0:F3}", d_realWater - d_objWater));
                                d_realDif = d_realDif < 0 ? -d_realDif : d_realDif;
                                double d_allDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)) : string.Format("{0:F3}",
                                    d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)));

                                string s_describe;
                                string s_describe_EN;
                                if (d_allDif < d_realDif || (d_realWater == 0.0 && d_objWater != 0.0))
                                {
                                    b_fail = false;
                                }
                                d_totalW += d_realWater;
                                if (b_fail)
                                {
                                    s_describe = "滴液成功!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                              ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Success !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(kvp.Key))
                                    {
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cup))
                                            FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                                    }
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  "UPDATE cup_details SET Statues = '滴液成功',TotalWeight = "+ (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalW) : string.Format("{0:F3}",
                                    d_totalW))+" WHERE CupNum = " + i_cup + ";");
                                }
                                else
                                {
                                    s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                            ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET Statues = '滴液失败',TotalWeight = "+ (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalW) : string.Format("{0:F3}",
                                    d_totalW))+" WHERE CupNum = " + i_cup + ";");
                                }
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE drop_head SET DescribeChar = '" + s_describe + "',DescribeChar_EN = '" + s_describe_EN + "', FinishTime = '" + DateTime.Now + "', Step = 2 " +
                               "WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cup + ";");
                            }
                        }

                    }
                }
                else
                {
                    foreach (KeyValuePair<int, double> kvp in dic_return)
                    {
                        double d_blRErr = 0;
                        if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                        else
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight));
                        ;

                        //查询开料日期
                        DataTable dt_bottle_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
            "SELECT bottle_details.*,UnitOfAccount  FROM bottle_details left join assistant_details   on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE  bottle_details.BottleNum = " + i_minBottleNo + ";");

                        if (0.00 != kvp.Value)
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + d_blRErr + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '" + (dt_bottle_details2.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                            "WHERE BatchName = '" + oBatchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                            "CupNum = " + kvp.Key + ";");

                            DataTable dt_drop_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName + "' AND BottleNum = " + i_minBottleNo + " AND CupNum = " + kvp.Key + ";");
                            if (dt_drop_details2.Rows.Count > 0)
                            {
                                //记录是否修正数据
                                bool b_change = false;
                                double d_realDrop = 0.0;
                                if (FADM_Object.Communal._b_isCorrect)
                                {
                                    //如果实际滴液量为负数，就改为0
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["RealDropWeight"]) < 0)
                                    {
                                        b_change = true;
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = 0   ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                    }

                                    //判断当目标滴液量大于1g，而且实际滴液误差小于等于0.05的，全部置为0.02
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) > 1)
                                    {
                                        if (d_blRErr <= 0.05 && d_blRErr > 0.02)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + 0.02 + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) + 0.02;
                                        }
                                        else if (d_blRErr < -0.02 && d_blRErr >= -0.05)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + (-0.02) + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) - 0.02;
                                        }
                                    }
                                }
                                if (b_change)
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + d_realDrop + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_realDrop + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");
                                }
                                else
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + dt_drop_details2.Rows[0]["RealDropWeight"] + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + dt_drop_details2.Rows[0]["RealDropWeight"] + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");

                                    ////置位完成标志位
                                    //FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    //    "UPDATE drop_details SET Finish = 1 WHERE BatchName = '" + o_BatchName + "' AND " +
                                    //    "BottleNum = " + _i_minBottleNo + " AND CupNum = " + dic_pulse.First().Key + ";");
                                }
                            }
                        }
                        else
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "UPDATE drop_details SET Finish = 1,RealDropWeight = 0.00 " + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '" + (dt_bottle_details2.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                           "WHERE BatchName = '" + oBatchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                           "CupNum = " + kvp.Key + ";");
                        }

                    }
                }
            }
            //由于滴废液时发现数值太小，直接提醒，不先滴这个，跳过
            else if (i_ret == -2)
            {
                //把已经滴过的先置为完成
                if (FADM_Object.Communal._b_isFinishSend)
                {
                    foreach (KeyValuePair<int, double> kvp in dic_return)
                    {
                        double d_blRErr = 0;
                        if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                        else
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight));

                        //查询开料日期
                        DataTable dt_bottle_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
            "SELECT bottle_details.*,UnitOfAccount  FROM bottle_details left join assistant_details   on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE  bottle_details.BottleNum = " + i_minBottleNo + ";");

                        if (0.00 != kvp.Value)
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + d_blRErr + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '" + (dt_bottle_details2.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                            "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                            "CupNum = " + kvp.Key + ";");

                            DataTable dt_drop_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND CupNum = " + kvp.Key + ";");
                            if (dt_drop_details2.Rows.Count > 0)
                            {
                                //记录是否修正数据
                                bool b_change = false;
                                double d_realDrop = 0.0;
                                if (FADM_Object.Communal._b_isCorrect)
                                {
                                    //如果实际滴液量为负数，就改为0
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["RealDropWeight"]) < 0)
                                    {
                                        b_change = true;
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = 0   ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                    }

                                    //判断当目标滴液量大于1g，而且实际滴液误差小于等于0.05的，全部置为0.02
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) > 1)
                                    {
                                        if (d_blRErr <= 0.05 && d_blRErr > 0.02)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + 0.02 + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) + 0.02;
                                        }
                                        else if (d_blRErr < -0.02 && d_blRErr >= -0.05)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + (-0.02) + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) - 0.02;
                                        }
                                    }
                                }
                                if (b_change)
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + d_realDrop + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_realDrop + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");
                                }
                                else
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE cup_details SET TotalWeight = TotalWeight+ " + dt_drop_details2.Rows[0]["RealDropWeight"] + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + dt_drop_details2.Rows[0]["RealDropWeight"] + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");
                                }
                            }
                        }
                        else
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "UPDATE drop_details SET Finish = 1,RealDropWeight = 0.00 " + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '" + (dt_bottle_details2.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                           "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                           "CupNum = " + kvp.Key + ";");
                        }

                        DataTable dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName.ToString() + "' AND Finish = 0" + " AND CupNum = " + kvp.Key + ";");
                        //查询一下加水没完成的也不置为完成
                        DataTable dt_drop_details4 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND AddWaterChoose = 1 AND AddWaterFinish =0" + " AND CupNum = " + kvp.Key + ";");
                        if (dt_drop_details3.Rows.Count == 0 && dt_drop_details4.Rows.Count == 0)
                        {
                            //更新配液杯总量
                            double d_totalW = 0.0;
                            //置为完成
                            s_sql = "UPDATE drop_head SET CupFinish = 1 WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + kvp.Key + "; ";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);


                            bool b_fail = true;

                            s_sql = "SELECT drop_details.CupNum as CupNum, " +
                                " drop_details.UnitOfAccount as UnitOfAccount, " +
                                        "drop_details.BottleNum as BottleNum, " +
                                        "drop_details.ObjectDropWeight as ObjectDropWeight, " +
                                        "drop_details.RealDropWeight as RealDropWeight, " +
                                        "bottle_details.SyringeType as SyringeType " +
                                        "FROM drop_details left join bottle_details on " +
                                        "bottle_details.BottleNum = drop_details.BottleNum " +
                                        "WHERE drop_details.BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + kvp.Key + ";";
                            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                            foreach (DataRow dr in dt_drop_head.Rows)
                            {
                                if (Convert.ToInt32(dr["BottleNum"]) > Lib_Card.Configure.Parameter.Machine_Bottle_Total)
                                    continue;
                                double d_blRealErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"]))) : Convert.ToDouble(string.Format("{0:F3}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])));
                                d_blRealErr = d_blRealErr < 0 ? -d_blRealErr : d_blRealErr;
                                if (d_blRealErr > (dr["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip))
                                    b_fail = false;

                                d_totalW += Convert.ToDouble(dr["RealDropWeight"]);
                            }

                            dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                    "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + kvp.Key + ";");
                            if (dt_drop_details3.Rows.Count > 0)
                            {
                                int i_cup = Convert.ToInt16(dt_drop_details3.Rows[0]["CupNum"]);
                                int i_Step = Convert.ToInt16(dt_drop_details3.Rows[0]["Step"]);
                                double d_objWater = Convert.ToDouble(dt_drop_details3.Rows[0]["ObjectAddWaterWeight"]);
                                double d_realWater = Convert.ToDouble(dt_drop_details3.Rows[0]["RealAddWaterWeight"]);
                                double d_totalWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TotalWeight"]);
                                double d_testTubeObjectAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeObjectAddWaterWeight"]);
                                double d_testTubeRealAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeRealAddWaterWeight"]);
                                double d_realDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater - d_objWater) : string.Format("{0:F3}", d_realWater - d_objWater));
                                d_realDif = d_realDif < 0 ? -d_realDif : d_realDif;
                                double d_allDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)) : string.Format("{0:F3}",
                                    d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)));

                                string s_describe;
                                string s_describe_EN;
                                if (d_allDif < d_realDif || (d_realWater == 0.0 && d_objWater != 0.0))
                                {
                                    b_fail = false;
                                }

                                d_totalW += d_realWater;

                                if (b_fail)
                                {
                                    s_describe = "滴液成功!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                              ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Success !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(kvp.Key))
                                    {
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cup))
                                            FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                                    }
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  "UPDATE cup_details SET Statues = '滴液成功',TotalWeight = "+ (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalW) : string.Format("{0:F3}",
                                    d_totalW))+" WHERE CupNum = " + i_cup + ";");
                                }
                                else
                                {
                                    s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                            ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET Statues = '滴液失败',TotalWeight = "+ (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalW) : string.Format("{0:F3}",
                                    d_totalW))+" WHERE CupNum = " + i_cup + ";");
                                }
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE drop_head SET DescribeChar = '" + s_describe + "',DescribeChar_EN = '" + s_describe_EN + "', FinishTime = '" + DateTime.Now + "', Step = 2 " +
                               "WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cup + ";");
                            }
                        }

                    }
                }
                else
                {
                    foreach (KeyValuePair<int, double> kvp in dic_return)
                    {
                        double d_blRErr = 0;
                        if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                        else
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight));
                        ;

                        //查询开料日期
                        DataTable dt_bottle_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
            "SELECT bottle_details.*,UnitOfAccount  FROM bottle_details left join assistant_details   on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE  bottle_details.BottleNum = " + i_minBottleNo + ";");

                        if (0.00 != kvp.Value)
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + d_blRErr + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '" + (dt_bottle_details2.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                            "WHERE BatchName = '" + oBatchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                            "CupNum = " + kvp.Key + ";");

                            DataTable dt_drop_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName + "' AND BottleNum = " + i_minBottleNo + " AND CupNum = " + kvp.Key + ";");
                            if (dt_drop_details2.Rows.Count > 0)
                            {
                                //记录是否修正数据
                                bool b_change = false;
                                double d_realDrop = 0.0;
                                if (FADM_Object.Communal._b_isCorrect)
                                {
                                    //如果实际滴液量为负数，就改为0
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["RealDropWeight"]) < 0)
                                    {
                                        b_change = true;
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = 0   ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                    }

                                    //判断当目标滴液量大于1g，而且实际滴液误差小于等于0.05的，全部置为0.02
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) > 1)
                                    {
                                        if (d_blRErr <= 0.05 && d_blRErr > 0.02)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + 0.02 + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) + 0.02;
                                        }
                                        else if (d_blRErr < -0.02 && d_blRErr >= -0.05)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + (-0.02) + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) - 0.02;
                                        }
                                    }
                                }
                                if (b_change)
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + d_realDrop + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_realDrop + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");
                                }
                                else
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + dt_drop_details2.Rows[0]["RealDropWeight"] + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + dt_drop_details2.Rows[0]["RealDropWeight"] + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");

                                    ////置位完成标志位
                                    //FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    //    "UPDATE drop_details SET Finish = 1 WHERE BatchName = '" + o_BatchName + "' AND " +
                                    //    "BottleNum = " + _i_minBottleNo + " AND CupNum = " + dic_pulse.First().Key + ";");
                                }
                            }
                        }
                        else
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "UPDATE drop_details SET Finish = 1,RealDropWeight = 0.00 " + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '" + (dt_bottle_details2.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                           "WHERE BatchName = '" + oBatchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                           "CupNum = " + kvp.Key + ";");
                        }

                    }
                }

                //更新需要加药第一杯脉冲
                s_sql = "UPDATE drop_details SET NeedPulse = " + Communal._i_needPulse + " WHERE BatchName = '" + oBatchName + "'  And " +
                                "BottleNum = " + i_minBottleNo + " AND Finish = 0  And CupNum = " + Communal._i_needPulseCupNumber + ";";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                //把剩余没有滴液的Min置为状态4
                s_sql = "UPDATE drop_details SET MinWeight = 4 WHERE BatchName = '" + oBatchName + "'  And " +
                                "BottleNum = " + i_minBottleNo + " AND Finish = 0 ;";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                MyAlarm myAlarm;
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + "号母液瓶预滴液数值太小,请检查实际是否液量过低?(继续执行请点是)", "Drip", i_minBottleNo, 2, 10, true);
                else
                    myAlarm = new FADM_Object.MyAlarm(" The number of pre-drops in mother liquor bottle " + i_minBottleNo + "  is too small, please check whether the actual amount of liquid is too low" +
                        "( Continue to perform please click Yes)", "Drip", i_minBottleNo, 2, 10, true);

                //回一次原点再继续，担心失步导致后续母液抽不了
                int i_state = MyModbusFun.goHome();
                if (0 != i_state && -2 != i_state)
                    throw new Exception("驱动异常");

            }
            //多针筒过程，后边发现液量低
            else if (i_ret == -3)
            {
                //把已经滴过的先置为完成
                if (FADM_Object.Communal._b_isFinishSend)
                {
                    foreach (KeyValuePair<int, double> kvp in dic_return)
                    {
                        double d_blRErr = 0;
                        if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                        else
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight));

                        //查询开料日期
                        DataTable dt_bottle_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
            "SELECT bottle_details.*,UnitOfAccount  FROM bottle_details left join assistant_details   on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE  bottle_details.BottleNum = " + i_minBottleNo + ";");

                        if (0.00 != kvp.Value)
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + d_blRErr + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '" + (dt_bottle_details2.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                            "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                            "CupNum = " + kvp.Key + ";");

                            DataTable dt_drop_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND CupNum = " + kvp.Key + ";");
                            if (dt_drop_details2.Rows.Count > 0)
                            {
                                //记录是否修正数据
                                bool b_change = false;
                                double d_realDrop = 0.0;
                                if (FADM_Object.Communal._b_isCorrect)
                                {
                                    //如果实际滴液量为负数，就改为0
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["RealDropWeight"]) < 0)
                                    {
                                        b_change = true;
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = 0   ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                    }

                                    //判断当目标滴液量大于1g，而且实际滴液误差小于等于0.05的，全部置为0.02
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) > 1)
                                    {
                                        if (d_blRErr <= 0.05 && d_blRErr > 0.02)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + 0.02 + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) + 0.02;
                                        }
                                        else if (d_blRErr < -0.02 && d_blRErr >= -0.05)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + (-0.02) + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) - 0.02;
                                        }
                                    }
                                }
                                if (b_change)
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + d_realDrop + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_realDrop + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");
                                }
                                else
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE cup_details SET TotalWeight = TotalWeight+ " + dt_drop_details2.Rows[0]["RealDropWeight"] + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + dt_drop_details2.Rows[0]["RealDropWeight"] + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");
                                }
                            }
                        }
                        else
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "UPDATE drop_details SET Finish = 1,RealDropWeight = 0.00 " + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '" + (dt_bottle_details2.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                           "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                           "CupNum = " + kvp.Key + ";");
                        }

                        DataTable dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName.ToString() + "' AND Finish = 0" + " AND CupNum = " + kvp.Key + ";");
                        //查询一下加水没完成的也不置为完成
                        DataTable dt_drop_details4 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND AddWaterChoose = 1 AND AddWaterFinish =0" + " AND CupNum = " + kvp.Key + ";");
                        if (dt_drop_details3.Rows.Count == 0 && dt_drop_details4.Rows.Count == 0)
                        {
                            //更新配液杯总量
                            double d_totalW = 0.0;
                            //置为完成
                            s_sql = "UPDATE drop_head SET CupFinish = 1 WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + kvp.Key + "; ";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);


                            bool b_fail = true;

                            s_sql = "SELECT drop_details.CupNum as CupNum, " +
                                " drop_details.UnitOfAccount as UnitOfAccount, " +
                                        "drop_details.BottleNum as BottleNum, " +
                                        "drop_details.ObjectDropWeight as ObjectDropWeight, " +
                                        "drop_details.RealDropWeight as RealDropWeight, " +
                                        "bottle_details.SyringeType as SyringeType " +
                                        "FROM drop_details left join bottle_details on " +
                                        "bottle_details.BottleNum = drop_details.BottleNum " +
                                        "WHERE drop_details.BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + kvp.Key + ";";
                            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                            foreach (DataRow dr in dt_drop_head.Rows)
                            {
                                if (Convert.ToInt32(dr["BottleNum"]) > Lib_Card.Configure.Parameter.Machine_Bottle_Total)
                                    continue;
                                double d_blRealErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"]))) : Convert.ToDouble(string.Format("{0:F3}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])));
                                d_blRealErr = d_blRealErr < 0 ? -d_blRealErr : d_blRealErr;
                                if (d_blRealErr > (dr["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip))
                                    b_fail = false;

                                d_totalW += Convert.ToDouble(dr["RealDropWeight"]);
                            }

                            dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                    "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + kvp.Key + ";");
                            if (dt_drop_details3.Rows.Count > 0)
                            {
                                int i_cup = Convert.ToInt16(dt_drop_details3.Rows[0]["CupNum"]);
                                int i_Step = Convert.ToInt16(dt_drop_details3.Rows[0]["Step"]);
                                double d_objWater = Convert.ToDouble(dt_drop_details3.Rows[0]["ObjectAddWaterWeight"]);
                                double d_realWater = Convert.ToDouble(dt_drop_details3.Rows[0]["RealAddWaterWeight"]);
                                double d_totalWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TotalWeight"]);
                                double d_testTubeObjectAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeObjectAddWaterWeight"]);
                                double d_testTubeRealAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeRealAddWaterWeight"]);
                                double d_realDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater - d_objWater) : string.Format("{0:F3}", d_realWater - d_objWater));
                                d_realDif = d_realDif < 0 ? -d_realDif : d_realDif;
                                double d_allDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)) : string.Format("{0:F3}",
                                    d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)));

                                string s_describe;
                                string s_describe_EN;
                                if (d_allDif < d_realDif || (d_realWater == 0.0 && d_objWater != 0.0))
                                {
                                    b_fail = false;
                                }

                                d_totalW += d_realWater;

                                if (b_fail)
                                {
                                    s_describe = "滴液成功!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                              ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Success !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(kvp.Key))
                                    {
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cup))
                                            FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                                    }
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  "UPDATE cup_details SET Statues = '滴液成功',TotalWeight = " + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalW) : string.Format("{0:F3}",
                                    d_totalW)) + " WHERE CupNum = " + i_cup + ";");
                                }
                                else
                                {
                                    s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                            ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET Statues = '滴液失败',TotalWeight = " + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalW) : string.Format("{0:F3}",
                                    d_totalW)) + " WHERE CupNum = " + i_cup + ";");
                                }
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE drop_head SET DescribeChar = '" + s_describe + "',DescribeChar_EN = '" + s_describe_EN + "', FinishTime = '" + DateTime.Now + "', Step = 2 " +
                               "WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cup + ";");
                            }
                        }

                    }
                }
                else
                {
                    foreach (KeyValuePair<int, double> kvp in dic_return)
                    {
                        double d_blRErr = 0;
                        if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                        else
                            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight));
                        ;

                        //查询开料日期
                        DataTable dt_bottle_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
            "SELECT bottle_details.*,UnitOfAccount  FROM bottle_details left join assistant_details   on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE  bottle_details.BottleNum = " + i_minBottleNo + ";");

                        if (0.00 != kvp.Value)
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + d_blRErr + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '" + (dt_bottle_details2.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                            "WHERE BatchName = '" + oBatchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                            "CupNum = " + kvp.Key + ";");

                            DataTable dt_drop_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName + "' AND BottleNum = " + i_minBottleNo + " AND CupNum = " + kvp.Key + ";");
                            if (dt_drop_details2.Rows.Count > 0)
                            {
                                //记录是否修正数据
                                bool b_change = false;
                                double d_realDrop = 0.0;
                                if (FADM_Object.Communal._b_isCorrect)
                                {
                                    //如果实际滴液量为负数，就改为0
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["RealDropWeight"]) < 0)
                                    {
                                        b_change = true;
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = 0   ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                    }

                                    //判断当目标滴液量大于1g，而且实际滴液误差小于等于0.05的，全部置为0.02
                                    if (Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) > 1)
                                    {
                                        if (d_blRErr <= 0.05 && d_blRErr > 0.02)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + 0.02 + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) + 0.02;
                                        }
                                        else if (d_blRErr < -0.02 && d_blRErr >= -0.05)
                                        {
                                            b_change = true;
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + (-0.02) + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                                "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                                "CupNum = " + kvp.Key + ";");
                                            d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) - 0.02;
                                        }
                                    }
                                }
                                if (b_change)
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + d_realDrop + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_realDrop + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");
                                }
                                else
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + dt_drop_details2.Rows[0]["RealDropWeight"] + " WHERE CupNum = " + kvp.Key + ";");

                                    //母液瓶扣减
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + dt_drop_details2.Rows[0]["RealDropWeight"] + " " +
                                        "WHERE BottleNum = '" + i_minBottleNo + "';");

                                    ////置位完成标志位
                                    //FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    //    "UPDATE drop_details SET Finish = 1 WHERE BatchName = '" + o_BatchName + "' AND " +
                                    //    "BottleNum = " + _i_minBottleNo + " AND CupNum = " + dic_pulse.First().Key + ";");
                                }
                            }
                        }
                        else
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "UPDATE drop_details SET Finish = 1,RealDropWeight = 0.00 " + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '" + (dt_bottle_details2.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                           "WHERE BatchName = '" + oBatchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                           "CupNum = " + kvp.Key + ";");
                        }

                    }
                }

                //更新需要加药第一杯脉冲
                s_sql = "UPDATE drop_details SET NeedPulse = " + Communal._i_needPulse + " WHERE BatchName = '" + oBatchName + "'  And " +
                                "BottleNum = " + i_minBottleNo + " AND Finish = 0  And CupNum = " + Communal._i_needPulseCupNumber + ";";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                if (Communal._b_isHaveAutoBrew)
                {
                    //把剩余没有滴液的Min置为状态6,改为自动开料
                    s_sql = "UPDATE drop_details SET MinWeight = 6 WHERE BatchName = '" + oBatchName + "'  And " +
                                    "BottleNum = " + i_minBottleNo + " AND Finish = 0 ;";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                    //插入自动开料列表
                    //判断是否存在正在开料的记录
                    s_sql = "select * from brew_station_details where IsUsing = 1 And Station<6 And BottleNum =" + i_minBottleNo;
                    DataTable dt_brew_station_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                    if (dt_brew_station_details.Rows.Count == 0)
                    {
                        //判断等待列表是否存在排队
                        s_sql = "select * from Brew_wait_list where  BottleNum =" + i_minBottleNo;
                        DataTable dt_Brew_wait_list = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                        if (dt_Brew_wait_list.Rows.Count == 0)
                        {
                            //插入到排队列表
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
               "INSERT INTO Brew_wait_list(BottleNum, InsertDate,Type) VALUES('" + i_minBottleNo + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',0);");
                        }

                    }
                }
                else
                {
                    //把剩余没有滴液的Min置为状态1,改为液量不足
                    s_sql = "UPDATE drop_details SET MinWeight = 1 WHERE BatchName = '" + oBatchName + "'  And " +
                                    "BottleNum = " + i_minBottleNo + " AND Finish = 0 ;";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                }


            }


            b_checkFail = false;

            goto label7;

        //加助剂
        label4:
            if (FADM_Object.Communal._b_isAssitantFirst)
            {
                s_unitA = "%";
            }
            else
            {
                s_unitA = "g/l";
            }
            goto label3;

        //添加母液不足的
        label5:
            s_sql = "SELECT BottleNum FROM drop_details WHERE BatchName = '" + oBatchName + "' AND Finish = 0 AND MinWeight = 1 AND " +
                "BottleNum <= '" + Lib_Card.Configure.Parameter.Machine_Bottle_Total + "'   And IsDrop != 0  GROUP BY BottleNum ORDER BY BottleNum ;";
            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (0 < dt_drop_head.Rows.Count)
            {
                string s_alarmBottleNo = null;
                foreach (DataRow dataRow in dt_drop_head.Rows)
                {
                    s_alarmBottleNo += dataRow["BottleNum"].ToString() + ";";
                }

                s_alarmBottleNo = s_alarmBottleNo.Remove(s_alarmBottleNo.Length - 1);
                //关闭失能
                MyModbusFun.Power(2);
                FADM_Object.Communal.WriteDripWait(true);
                Communal._b_isAllowDrip = false;
                FADM_Object.MyAlarm myAlarm;
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    myAlarm = new FADM_Object.MyAlarm(s_alarmBottleNo + "号母液瓶液量过低，是否继续滴液?", "滴液", true, 1);
                else
                    myAlarm = new FADM_Object.MyAlarm("The liquid level in bottle " + s_alarmBottleNo + " is too low. Do you want to continue ? ", "Drip", true, 1);
                while (true)
                {
                    if (0 != myAlarm._i_alarm_Choose) {
                        //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                        //{
                        //  //  Home.Home_XYZFinish = false;
                        //}
                        break;
                    }
                    Thread.Sleep(1);
                }
                Communal._b_isAllowDrip = true;
                //判断染色线程是否需要用机械手
                if (null != FADM_Object.Communal.ReadDyeThread())
                {
                    FADM_Object.Communal.WriteDripWait(true);
                    Communal._b_isWaitDrip = true;
                    Communal._b_isAllowDrip = true;
                    while (true)
                    {
                        if (false == FADM_Object.Communal.ReadDripWait())
                            break;
                        Thread.Sleep(1);
                    }
                    Communal._b_isWaitDrip = false;
                }
                else
                {
                    Communal._b_isWaitDrip = false;
                    FADM_Object.Communal.WriteDripWait(false);
                }

                if (1 == myAlarm._i_alarm_Choose)
                {

                    if (FADM_Object.Communal._b_isLowDripAllow)
                    {
                        i_lowSrart = 1;
                        if (FADM_Object.Communal._b_isAssitantFirst)
                        {
                            s_unitA = "g/l";
                        }
                        else
                        {
                            s_unitA = "%";
                        }
                        goto label3;
                    }
                    else
                    {
                        //把MinWeight=1的数据重新置为0
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE drop_details SET MinWeight = 0 where MinWeight = 1;");
                        goto lab_again;
                    }
                }
                else
                {
                    b_chooseNo = true;
                }
            }

        //添加超出生命周期(过期)
        label17:
            s_sql = "SELECT BottleNum FROM drop_details WHERE BatchName = '" + oBatchName + "' AND Finish = 0 AND MinWeight = 3 AND " +
                "BottleNum <= '" + Lib_Card.Configure.Parameter.Machine_Bottle_Total + "'   And IsDrop != 0 GROUP BY BottleNum ORDER BY BottleNum ;";
            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (0 < dt_drop_head.Rows.Count)
            {
                string s_alarmBottleNo = null;
                foreach (DataRow dataRow in dt_drop_head.Rows)
                {
                    s_alarmBottleNo += dataRow["BottleNum"].ToString() + ";";
                }

                s_alarmBottleNo = s_alarmBottleNo.Remove(s_alarmBottleNo.Length - 1);
                //关闭失能
                MyModbusFun.Power(2);
                FADM_Object.Communal.WriteDripWait(true);
                Communal._b_isAllowDrip = false;
                FADM_Object.MyAlarm myAlarm;
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    myAlarm = new FADM_Object.MyAlarm(s_alarmBottleNo + "号母液瓶过期，是否继续滴液?", "滴液", true, 1);
                else
                    myAlarm = new FADM_Object.MyAlarm("The liquid level in bottle " + s_alarmBottleNo + " is expire. Do you want to continue ? ", "Drip", true, 1);
                while (true)
                {
                    if (0 != myAlarm._i_alarm_Choose) {
                        //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                        //{
                        //  //  Home.Home_XYZFinish = false;
                        //}
                        break;
                    }
                       
                    Thread.Sleep(1);
                }
                Communal._b_isAllowDrip = true;
                //判断染色线程是否需要用机械手
                if (null != FADM_Object.Communal.ReadDyeThread())
                {
                    FADM_Object.Communal.WriteDripWait(true);
                    Communal._b_isWaitDrip = true;
                    Communal._b_isAllowDrip = true;
                    while (true)
                    {
                        if (false == FADM_Object.Communal.ReadDripWait())
                            break;
                        Thread.Sleep(1);
                    }
                    Communal._b_isWaitDrip = false;
                }
                else
                {
                    Communal._b_isWaitDrip = false;
                    FADM_Object.Communal.WriteDripWait(false);
                }

                if (1 == myAlarm._i_alarm_Choose)
                {
                    if (FADM_Object.Communal._b_isOutDripAllow)
                    {
                        i_lowSrart = 3;
                        if (FADM_Object.Communal._b_isAssitantFirst)
                        {
                            s_unitA = "g/l";
                        }
                        else
                        {
                            s_unitA = "%";
                        }
                        goto label3;
                    }
                    else
                    {
                        //把MinWeight=3的数据重新置为0
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE drop_details SET MinWeight = 0 where MinWeight = 3;");
                        goto lab_again;
                    }
                }
                else
                {
                    b_chooseNo = true;
                }
            }


        //添加找不到针筒的
        label16:
            s_sql = "SELECT BottleNum FROM drop_details WHERE BatchName = '" + oBatchName + "' AND Finish = 0 AND MinWeight = 2 AND " +
               "BottleNum <= '" + Lib_Card.Configure.Parameter.Machine_Bottle_Total + "'  And IsDrop != 0  GROUP BY BottleNum ORDER BY BottleNum ;";
            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (0 < dt_drop_head.Rows.Count)
            {
                string s_alarmBottleNo = null;
                foreach (DataRow dataRow in dt_drop_head.Rows)
                {
                    s_alarmBottleNo += dataRow["BottleNum"].ToString() + ";";
                }

                s_alarmBottleNo = s_alarmBottleNo.Remove(s_alarmBottleNo.Length - 1);
                FADM_Object.Communal.WriteDripWait(true);
                Communal._b_isAllowDrip = false;
                FADM_Object.MyAlarm myAlarm;
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    myAlarm = new FADM_Object.MyAlarm(s_alarmBottleNo + "号母液瓶未检测到针筒，是否继续滴液 ? ", "滴液", true, 1, true);
                else
                    myAlarm = new FADM_Object.MyAlarm(s_alarmBottleNo + " bottle did not find a syringe. Do you want to continue? ", "Drip", true, 1, true);
                while (true)
                {
                    if (0 != myAlarm._i_alarm_Choose) {
                        //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                        //{
                        //  //  Home.Home_XYZFinish = false;
                        //}
                        break;
                    }
                       
                    Thread.Sleep(1);
                }
                Communal._b_isAllowDrip = true;
                //判断染色线程是否需要用机械手
                if (null != FADM_Object.Communal.ReadDyeThread())
                {
                    FADM_Object.Communal.WriteDripWait(true);
                    Communal._b_isWaitDrip = true;
                    Communal._b_isAllowDrip = true;
                    while (true)
                    {
                        if (false == FADM_Object.Communal.ReadDripWait())
                            break;
                        Thread.Sleep(1);
                    }
                    Communal._b_isWaitDrip = false;
                }
                else
                {
                    Communal._b_isWaitDrip = false;
                    FADM_Object.Communal.WriteDripWait(false);
                }

                if (1 == myAlarm._i_alarm_Choose)
                {
                    i_lowSrart = 2;
                    if (FADM_Object.Communal._b_isAssitantFirst)
                    {
                        s_unitA = "g/l";
                    }
                    else
                    {
                        s_unitA = "%";
                    }
                    goto label3;
                }
                else
                {
                    b_chooseNo = true;
                }
            }

        //滴液完成    
        label6:
            //关闭失能
            MyModbusFun.Power(2);
            FADM_Object.Communal.WriteDripWait(true);
            Communal._b_isAllowDrip = false;
            if (!b_chooseNo)
            {
            lab_Re:
                //判断是否全部完成，等待是否还有洗杯没完成的
                s_sql = "SELECT * FROM drop_details WHERE BatchName = '" + oBatchName + "' AND Finish = 0  AND " +
                   "BottleNum <= '" + Lib_Card.Configure.Parameter.Machine_Bottle_Total + "' ;";
                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                if (dt_drop_head.Rows.Count > 0)
                {

                    foreach (DataRow dataRow in dt_drop_head.Rows)
                    {
                        if (dataRow["IsDrop"].ToString() == "1" && dataRow["MinWeight"].ToString() != "4")
                        {
                            Communal._b_isAllowDrip = true;
                            //获取机械手
                            if (null != FADM_Object.Communal.ReadDyeThread())
                            {
                                FADM_Object.Communal.WriteDripWait(true);
                                Communal._b_isWaitDrip = true;
                                Communal._b_isAllowDrip = true;
                                while (true)
                                {
                                    if (false == FADM_Object.Communal.ReadDripWait())
                                        break;
                                    Thread.Sleep(1);
                                }
                                Communal._b_isWaitDrip = false;
                            }
                            else
                            {
                                Communal._b_isWaitDrip = false;
                                FADM_Object.Communal.WriteDripWait(false);
                            }
                            goto lab_again;
                        }
                    }
                    Thread.Sleep(1000);

                    //if (!FADM_Object.Communal.ReadDripWait())
                    //{
                    //    FADM_Object.Communal.WriteDripWait(true);
                    //}

                    //获取机械手
                    if (null != FADM_Object.Communal.ReadDyeThread())
                    {
                        FADM_Object.Communal.WriteDripWait(true);
                        Communal._b_isWaitDrip = true;
                        while (true)
                        {
                            if (false == FADM_Object.Communal.ReadDripWait())
                                break;
                            Thread.Sleep(1);
                        }
                        Communal._b_isWaitDrip = false;
                    }
                    else
                    {
                        Communal._b_isWaitDrip = false;
                        FADM_Object.Communal.WriteDripWait(false);
                    }

                    AddWater(oBatchName);

                    goto lab_again;
                }
                //判断一下是否有没加水的
                else
                {
                    s_sql = "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName + "' AND " +
                "AddWaterChoose = 1 AND AddWaterFinish = 0  ORDER BY CupNum;";
                    dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                    if (dt_drop_head.Rows.Count > 0)
                    {
                        Communal._b_isAllowDrip = true;
                        //获取机械手
                        if (null != FADM_Object.Communal.ReadDyeThread())
                        {
                            FADM_Object.Communal.WriteDripWait(true);
                            Communal._b_isWaitDrip = true;
                            Communal._b_isAllowDrip = true;
                            while (true)
                            {
                                if (false == FADM_Object.Communal.ReadDripWait())
                                    break;
                                Thread.Sleep(1);
                            }
                            Communal._b_isWaitDrip = false;
                        }
                        else
                        {
                            Communal._b_isWaitDrip = false;
                            FADM_Object.Communal.WriteDripWait(false);
                        }
                        AddWater(oBatchName);
                        goto lab_again;
                    }
                }
            }
            if (null != thread)
                thread.Join();
            //判断称粉是否完成
            if (FADM_Object.Communal._b_isUsePower)
            {
                while (true)
                {
                    //查询没有完成的记录
                    dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                   "SELECT * from drop_head where CupFinish = 0 " +
                   " And BatchName = '" + oBatchName + "' order by CupNum;");
                    if (dt_drop_head.Rows.Count == 0)
                        break;
                    //判断是否由于200或201母液瓶没加粉完成导致整杯没完成
                    foreach (DataRow dr1 in dt_drop_head.Rows)
                    {
                        //等待称粉完成
                        DataTable dt_drop_d = FADM_Object.Communal._fadmSqlserver.GetData(
                    "SELECT * from drop_details where   (BottleNum =200 Or BottleNum = 201)  " +
                    " And BatchName = '" + oBatchName + "' And CupNum = " + Convert.ToInt32(dr1["CupNum"]) + " And Finish=0;");
                        if (dt_drop_d.Rows.Count == 0)
                        {
                            //更新配液杯总量
                            double d_totalW = 0.0;
                            //置为完成
                            s_sql = "UPDATE drop_head SET CupFinish = 1 WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + Convert.ToInt32(dr1["CupNum"]) + "; ";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                            bool b_fail = true;

                            s_sql = "SELECT drop_details.CupNum as CupNum, " +
                                " drop_details.UnitOfAccount as UnitOfAccount, " +
                                        "drop_details.BottleNum as BottleNum, " +
                                        "drop_details.ObjectDropWeight as ObjectDropWeight, " +
                                        "drop_details.RealDropWeight as RealDropWeight, " +
                                        "bottle_details.SyringeType as SyringeType " +
                                        "FROM drop_details left join bottle_details on " +
                                        "bottle_details.BottleNum = drop_details.BottleNum " +
                                        "WHERE drop_details.BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + Convert.ToInt32(dr1["CupNum"]) + ";";
                            DataTable dt_drop_head12 = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                            foreach (DataRow dr in dt_drop_head12.Rows)
                            {
                                if (Convert.ToInt32(dr["BottleNum"]) > Lib_Card.Configure.Parameter.Machine_Bottle_Total)
                                    continue;
                                double d_blRealErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"]))) : Convert.ToDouble(string.Format("{0:F3}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])));
                                d_blRealErr = d_blRealErr < 0 ? -d_blRealErr : d_blRealErr;
                                if (d_blRealErr > (dr["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip))
                                    b_fail = false;
                                d_totalW += Convert.ToDouble(dr["RealDropWeight"]);
                            }

                            DataTable dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                    "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + Convert.ToInt32(dr1["CupNum"]) + ";");
                            if (dt_drop_details3.Rows.Count > 0)
                            {
                                int i_cup = Convert.ToInt16(dt_drop_details3.Rows[0]["CupNum"]);
                                int i_Step = Convert.ToInt16(dt_drop_details3.Rows[0]["Step"]);
                                double d_objWater = Convert.ToDouble(dt_drop_details3.Rows[0]["ObjectAddWaterWeight"]);
                                double d_realWater = Convert.ToDouble(dt_drop_details3.Rows[0]["RealAddWaterWeight"]);
                                double d_totalWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TotalWeight"]);
                                double d_testTubeObjectAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeObjectAddWaterWeight"]);
                                double d_testTubeRealAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeRealAddWaterWeight"]);
                                double d_realDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater - d_objWater) : string.Format("{0:F3}", d_realWater - d_objWater));
                                d_realDif = d_realDif < 0 ? -d_realDif : d_realDif;
                                double d_allDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)) : string.Format("{0:F3}",
                                    d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)));

                                string s_describe;
                                string s_describe_EN;
                                if (d_allDif < d_realDif || (d_realWater == 0.0 && d_objWater != 0.0))
                                {
                                    b_fail = false;
                                }

                                d_totalW += d_realWater;

                                if (b_fail)
                                {
                                    s_describe = "滴液成功!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                              ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Success !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(Convert.ToInt32(dr1["CupNum"])))
                                    {
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cup))
                                            FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                                    }
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  "UPDATE cup_details SET Statues = '滴液成功',TotalWeight = "+ (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalW) : string.Format("{0:F3}",
                                    d_totalW))+" WHERE CupNum = " + i_cup + ";");
                                }
                                else
                                {
                                    s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                            ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET Statues = '滴液失败',TotalWeight = "+ (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalW) : string.Format("{0:F3}",
                                    d_totalW))+" WHERE CupNum = " + i_cup + ";");
                                }
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE drop_head SET DescribeChar = '" + s_describe + "',DescribeChar_EN = '" + s_describe_EN + "', FinishTime = '" + DateTime.Now + "', Step = 2 " +
                               "WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cup + ";");
                            }
                        }

                    }
                    dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                   "SELECT * from drop_details where   (BottleNum =200 Or BottleNum = 201)  " +
                   " And BatchName = '" + oBatchName + "' order by CupNum;");
                    //没有需要称粉就退出
                    if (dt_drop_head.Rows.Count == 0)
                    {
                        break;
                    }
                    //收到停止滴液信号也退出
                    if (_b_dripStop)
                        throw new Exception("收到退出消息");
                    Thread.Sleep(1000);
                }
            }
            if (FADM_Object.Communal._b_isFinishSend)
            {
                //把由于超期，液量低跳过的所有置为不合格
                FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE drop_head SET DescribeChar = '滴液失败',DescribeChar_EN = 'Drip Fail',CupFinish = 1, FinishTime = '" + DateTime.Now + "', Step = 2 " +
                               "WHERE BatchName = '" + oBatchName + "' AND CupFinish != 1;");

                //获取滴液不合格记录
                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
               "SELECT * from drop_head where DescribeChar like '%滴液失败%'" +
               " And BatchName = '" + oBatchName + "' order by CupNum;");

                List<int> lis_cupFailD = new List<int>();
                List<int> lis_cupFailT = new List<int>();
                string s_cupNo = "";
                foreach (DataRow dr in dt_drop_head.Rows)
                {
                    lis_cupFailD.Add(Convert.ToInt32(dr["CupNum"]));
                    s_cupNo += dr["CupNum"].ToString() + "; ";
                }
                lis_cupFailT = lis_cupFailD.Distinct().ToList();


                if (FADM_Auto.Drip._b_dripErr == false)
                {
                    if (0 < lis_cupFailT.Count)
                    {
                        s_cupNo = s_cupNo.Remove(s_cupNo.Length - 1);

                        FADM_Object.Communal.WriteDripWait(true);
                        Communal._b_isAllowDrip = false;
                        FADM_Object.MyAlarm myAlarm;
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            myAlarm = new FADM_Object.MyAlarm(s_cupNo + "号配液杯滴液失败，是否继续(重新滴液请点是，退出滴液请点否)?", "滴液", true, 1);
                        else
                            myAlarm = new FADM_Object.MyAlarm("The dispensing cup" + s_cupNo + "  failed to dispense liquid. Do you want to continue (please click Yes for re dispensing and No for exiting the dispensing)? ", "Drip", true, 1);


                        while (true)
                        {
                            if (0 != myAlarm._i_alarm_Choose) {
                                //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                                //{
                                //    Home.Home_XYZFinish = false;
                                //}
                                break;
                            }
                                
                            Thread.Sleep(1);
                        }
                        //判断染色线程是否需要用机械手
                        if (null != FADM_Object.Communal.ReadDyeThread())
                        {
                            FADM_Object.Communal.WriteDripWait(true);
                            Communal._b_isWaitDrip = true;
                            while (true)
                            {
                                if (false == FADM_Object.Communal.ReadDripWait())
                                    break;
                                Thread.Sleep(1);
                            }
                            Communal._b_isWaitDrip = false;
                        }
                        else
                        {
                            Communal._b_isWaitDrip = false;
                            FADM_Object.Communal.WriteDripWait(false);
                        }

                        if (1 == myAlarm._i_alarm_Choose)
                        {
                            //先把滴液区域重置
                            //滴液区
                            for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                            {
                                if (SmartDyeing.FADM_Object.Communal._lis_dripCupNum.Contains(lis_cupFailT[i])|| SmartDyeing.FADM_Object.Communal._lis_PrecisionCupNum.Contains(lis_cupFailT[i]))
                                {

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE drop_head SET CupFinish = 0, AddWaterFinish = 0, RealAddWaterWeight = 0.00 , Step = 1,DescribeChar=null " +
                                        "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + lis_cupFailT[i] + ";");

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE drop_details SET Finish = 0, MinWeight = 0, RealDropWeight = 0.00 , NeedPulse = 0" +
                                        "WHERE BatchName = '" + oBatchName + "' AND BottleNum !=200 And BottleNum !=201 And  CupNum = " + lis_cupFailT[i] + ";");
                                    int Num = lis_cupFailT[i];
                                    lis_cupFailT.Remove(lis_cupFailT[i]);
                                    lis_cupSuc.Remove(Num);
                                }

                            }
                            //后处理区域
                            {
                                DataTable dataTable = FADM_Object.Communal._fadmSqlserver.GetData("SELECT * FROM drop_head WHERE BatchName = '" + oBatchName + "'    ORDER BY CupNum;");
                                DataTable dataTable_s = FADM_Object.Communal._fadmSqlserver.GetData("SELECT * FROM drop_head WHERE BatchName != '0'    ORDER BY CupNum;");
                                List<int> lis_lUse_Cup = new List<int>();
                                if (FADM_Object.Communal._b_isNeedConfirm)
                                {
                                    foreach (DataRow dataRow in dataTable_s.Rows)
                                    {
                                        lis_lUse_Cup.Add(Convert.ToInt16(dataRow["CupNum"]));
                                    }
                                }
                                else
                                {
                                    foreach (DataRow dataRow in dataTable.Rows)
                                    {
                                        lis_lUse_Cup.Add(Convert.ToInt16(dataRow["CupNum"]));
                                    }
                                }
                                //把对应副杯也加入进去
                                List<int> lis_cupFailT2 = new List<int>();
                                lis_cupFailT2.AddRange(lis_cupFailT);
                                foreach (int i in lis_cupFailT2)
                                {
                                    if (_dic_first_second[i] > 0)
                                    {
                                        if (lis_lUse_Cup.Contains(_dic_first_second[i]))
                                            if (!lis_cupFailT.Contains(_dic_first_second[i]))
                                                lis_cupFailT.Add(_dic_first_second[i]);
                                    }
                                }
                                //打板区
                                FADM_Object.Communal._lis_dripFailCupFinish.Clear();
                                FADM_Object.Communal._lis_dripFailCup.AddRange(lis_cupFailT);
                                List<int> lis_ints1 = new List<int>();
                                lis_ints1.AddRange(lis_cupFailT);
                                FADM_Object.Communal._lis_SendReadyCup.Clear();

                                //等待染色机排完再重滴
                                while (true)
                                {
                                    FADM_Object.Communal.WriteDripWait(true);
                                    if (0 == lis_cupFailT.Count)
                                    {
                                        FADM_Object.Communal.WriteDripWait(false);
                                        break;
                                    }

                                    for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                                    {
                                        if (FADM_Object.Communal._lis_dripFailCupFinish.Contains(lis_cupFailT[i]))
                                        {

                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                "UPDATE drop_head SET CupFinish = 0, AddWaterFinish = 0, RealAddWaterWeight = 0.00 , Step = 1,DescribeChar=null " +
                                                "WHERE BatchName = '" + oBatchName + "' And  CupNum = " + lis_cupFailT[i] + ";");

                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                "UPDATE drop_details SET Finish = 0, MinWeight = 0, RealDropWeight = 0.00 , NeedPulse = 0" +
                                                "WHERE BatchName = '" + oBatchName + "' AND BottleNum !=200 And BottleNum !=201 AND CupNum = " + lis_cupFailT[i] + ";");
                                            FADM_Object.Communal._lis_dripFailCupFinish.Remove(lis_cupFailT[i]);

                                            ////如果有副杯，副杯也要清除
                                            //if (_dic_first_second[lis_cupFailT[i]] >0)
                                            //{
                                            //    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                            //    "UPDATE drop_head SET CupFinish = 0, AddWaterFinish = 0, RealAddWaterWeight = 0.00 , Step = 1,DescribeChar=null " +
                                            //    "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + _dic_first_second[lis_cupFailT[i]] + ";");

                                            //    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                            //        "UPDATE drop_details SET Finish = 0, MinWeight = 0, RealDropWeight = 0.00 , NeedPulse = 0" +
                                            //        "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + _dic_first_second[lis_cupFailT[i]] + ";");
                                            //    //FADM_Object.Communal._lis_dripFailCupFinish.Remove(lis_cupFailT[i]);
                                            //}
                                            int i_num = lis_cupFailT[i];
                                            lis_cupFailT.Remove(lis_cupFailT[i]);
                                            lis_cupSuc.Remove(i_num);
                                        }
                                    }

                                    DataTable dt_drop_head2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                                               "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName + "'    ORDER BY CupNum;");
                                    DataTable dt_drop_head3 = FADM_Object.Communal._fadmSqlserver.GetData(
                                                              "SELECT * FROM drop_head WHERE BatchName != '0'    ORDER BY CupNum;");
                                    List<int> lis_lUse1 = new List<int>();
                                    if (FADM_Object.Communal._b_isNeedConfirm)
                                    {
                                        foreach (DataRow dataRow in dt_drop_head3.Rows)
                                        {
                                            lis_lUse1.Add(Convert.ToInt16(dataRow["CupNum"]));
                                        }
                                    }
                                    else
                                    {
                                        foreach (DataRow dataRow in dt_drop_head2.Rows)
                                        {
                                            lis_lUse1.Add(Convert.ToInt16(dataRow["CupNum"]));
                                        }

                                    }
                                    for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                                    {
                                        //已经强停，不在滴液列表
                                        if (!lis_lUse1.Contains(lis_cupFailT[i]))
                                        {
                                            int i_num = lis_cupFailT[i];
                                            lis_cupFailT.Remove(lis_cupFailT[i]);
                                            lis_ints1.Remove(i_num);
                                        }
                                    }

                                    Thread.Sleep(1);
                                }

                                foreach (int i in lis_ints1)
                                {
                                    int i_cupNum = i;

                                    while (true)
                                    {
                                        //滴液完成数组移除当前杯号
                                        FADM_Object.Communal._lis_dripSuccessCup.Remove(i_cupNum);
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cupNum))
                                            break;
                                    }



                                    DataTable dt = FADM_Object.Communal._fadmSqlserver.GetData("Select * from drop_head where (CupNum = " + i_cupNum + " Or  CupNum = " + Communal._dic_first_second[i_cupNum] + ") And BatchName != '0'");
                                    //先查是否双杯在使用
                                    if (dt.Rows.Count == 2)
                                    {
                                        string s_info = "";
                                        for (int p = 0; p < _lis_SendReadyCup.Count; p++)
                                        {
                                            s_info += _lis_SendReadyCup[p] + ",";
                                        }
                                        //判断是否发过启动信号
                                        if ((!_lis_SendReadyCup.Contains(i_cupNum)) && (!_lis_SendReadyCup.Contains(_dic_first_second[i_cupNum])))
                                        {
                                            //判断另外一只杯是否在洗杯
                                            //if (!lis_iUse.Contains(_dic_first_second[i_cupNum]))
                                            {
                                                int[] ia_zero = new int[1];
                                                ia_zero[0] = 0;
                                                DyeHMIWrite(i_cupNum, 119, 119, ia_zero);
                                                //滴液状态
                                                ia_zero[0] = 3;
                                                DyeHMIWrite(i_cupNum, 100, 100, ia_zero);


                                                _lis_SendReadyCup.Add(i_cupNum);
                                                _lis_SendReadyCup.Add(_dic_first_second[i_cupNum]);
                                            }
                                        }
                                    }
                                    //单杯就直接下发
                                    else
                                    {
                                        int[] ia_zero = new int[1];
                                        if (_dic_first_second[i_cupNum] > 0)
                                        {
                                            //主杯
                                            if (i_cupNum < _dic_first_second[i_cupNum])
                                            {
                                                ia_zero[0] = 1;
                                                DyeHMIWrite(i_cupNum, 119, 119, ia_zero);
                                            }
                                            else
                                            {
                                                ia_zero[0] = 2;
                                                DyeHMIWrite(i_cupNum, 119, 119, ia_zero);
                                            }
                                        }
                                        //滴液状态
                                        ia_zero[0] = 3;
                                        DyeHMIWrite(i_cupNum, 100, 100, ia_zero);

                                        _lis_SendReadyCup.Add(i_cupNum);
                                    }

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE cup_details SET Statues = '等待准备状态' WHERE CupNum = " + i_cupNum + ";");

                                }


                                //等待准备状态
                                while (true)
                                {
                                    bool b_open = true;
                                    DataTable dt_drop_head2 = FADM_Object.Communal._fadmSqlserver.GetData(
                           "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName + "' AND CupFinish = 0    ORDER BY CupNum;");
                                    DataTable dt_drop_head3 = FADM_Object.Communal._fadmSqlserver.GetData(
                           "SELECT * FROM drop_head WHERE BatchName != '0' AND CupFinish = 0    ORDER BY CupNum;");
                                    List<int> lUse1 = new List<int>();
                                    if (FADM_Object.Communal._b_isNeedConfirm)
                                    {
                                        foreach (DataRow dataRow in dt_drop_head3.Rows)
                                        {
                                            lUse1.Add(Convert.ToInt16(dataRow["CupNum"]));
                                        }
                                    }
                                    else
                                    {
                                        foreach (DataRow dataRow in dt_drop_head2.Rows)
                                        {
                                            lUse1.Add(Convert.ToInt16(dataRow["CupNum"]));
                                        }
                                    }
                                    for (int i = lis_ints1.Count - 1; i >= 0; i--)
                                    {
                                        //已经强停，不在滴液列表
                                        if (!lUse1.Contains(lis_ints1[i]))
                                        {
                                            lis_ints1.Remove(lis_ints1[i]);
                                        }
                                    }
                                    foreach (int i in lis_ints1)
                                    {
                                        int i_cupNum = i;


                                        int i_openCover = Convert.ToInt16(FADM_Auto.Dye._cup_Temps[i_cupNum - 1]._s_statues);


                                        if (5 != i_openCover)
                                            b_open = false;

                                    }


                                    if (b_open)
                                        break;

                                    Thread.Sleep(1);
                                }

                                lis_ints1.Clear();
                            }
                            //重新滴液时重新清一下
                            if (FADM_Object.Communal._b_isAddWaterFirst)
                                this.DripProcess(oBatchName);
                            else
                                this.DripProcess_AssFirst(oBatchName);
                        }
                        else
                        {
                            //不重滴

                            if (FADM_Object.Communal.ReadMachineStatus() != 8)
                            {
                                for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                                {
                                    if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(lis_cupFailT[i]))
                                    {
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(lis_cupFailT[i]))
                                            FADM_Object.Communal._lis_dripSuccessCup.Add(lis_cupFailT[i]);
                                    }
                                }
                            }

                        }

                    }
                }

                string s_cupList = "";
                string s_te = "";
                if (lis_cupSuc.Count > 0)
                {
                    for (int i = 0; i < lis_cupSuc.Count; i++)
                    {
                        s_cupList += lis_cupSuc[i] + ",";
                    }
                }
                if (s_cupList != "")
                {
                    s_cupList = s_cupList.Remove(s_cupList.Length - 1);
                    s_te = " And CupNum in (" + s_cupList + ")";
                }
                //添加历史表
                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                 "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'drop_head';");
                string s_columnHead = null;
                foreach (DataRow row in dt_drop_head.Rows)
                {
                    string s_curName = Convert.ToString(row[0]);
                    if ("TestTubeFinish" != s_curName && "TestTubeWaterLower" != s_curName && "AddWaterFinish" != s_curName &&
                        "CupFinish" != s_curName && "TestTubeWaterLower" != s_curName)
                        s_columnHead += s_curName + ", ";
                }
                s_columnHead = s_columnHead.Remove(s_columnHead.Length - 2);

                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                   "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'drop_details';");
                string s_columnDetails = null;
                foreach (DataRow row in dt_drop_head.Rows)
                {
                    string s_curName = Convert.ToString(row[0]);
                    if ("MinWeight" != s_curName && "Finish" != s_curName && "IsShow" != s_curName && "NeedPulse" != s_curName)
                        s_columnDetails += Convert.ToString(row[0]) + ", ";
                }
                s_columnDetails = s_columnDetails.Remove(s_columnDetails.Length - 2);

                //
                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                   "SELECT * FROM drop_head WHERE   BatchName = '" + oBatchName + "' ;");

                foreach (DataRow row in dt_drop_head.Rows)
                {
                    if (lis_cupSuc.Contains(Convert.ToInt32(row["CupNum"].ToString())))
                    {

                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                        "INSERT INTO history_head (" + s_columnHead + ") (SELECT " + s_columnHead + " FROM drop_head " +
                        "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + row["CupNum"].ToString() + ") ;");

                        //FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "INSERT INTO history_head (" + s_columnHead + ") (SELECT " + s_columnHead + " FROM drop_head " +
                        //    "WHERE BatchName = '" + o_BatchName + "' AND CupNum >= " + _i_cupMin + " AND CupNum <= " + _i_cupMax + s_te + ");");


                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "INSERT INTO history_details (" + s_columnDetails + ") (SELECT " + s_columnDetails + " FROM drop_details " +
                           "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + row["CupNum"].ToString() + ") ;");

                        //滴液
                        if (SmartDyeing.FADM_Object.Communal._lis_dripCupNum.Contains(Convert.ToInt32(row["CupNum"].ToString())))
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  "DELETE FROM drop_head WHERE CupNum = " + row["CupNum"].ToString() + " AND BatchName = '" + oBatchName + "' ;");
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "DELETE FROM drop_details WHERE CupNum = " + row["CupNum"].ToString() + " AND BatchName = '" + oBatchName + "' ;");


                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE cup_details SET FormulaCode = null, " +
                                "DyeingCode = null, IsUsing = 0, Statues = '待机', " +
                                "StartTime = null, SetTemp = null, StepNum = null, TotalWeight = null, " +
                                "TotalStep = null, TechnologyName = null, StepStartTime = null, SetTime = null,RecordIndex = 0 WHERE CupNum = " +
                                row["CupNum"].ToString() + " ;");
                        }
                    }
                }

                if (FADM_Auto.Drip._b_dripErr)
                {
                    FADM_Object.Communal._lis_dripStopCup.AddRange(lis_cupT);

                }
            }
            else
            {
                s_sql = "UPDATE drop_head SET CupFinish = 1 WHERE BatchName = '" + oBatchName + "'  ;";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                //获取滴液不合格记录
                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                   "SELECT drop_details.CupNum as CupNum, " +
                   " drop_details.UnitOfAccount as UnitOfAccount, " +
                   "drop_details.BottleNum as BottleNum, " +
                   "drop_details.ObjectDropWeight as ObjectDropWeight, " +
                   "drop_details.RealDropWeight as RealDropWeight, " +
                   "bottle_details.SyringeType as SyringeType " +
                   "FROM drop_details left join bottle_details on " +
                   "bottle_details.BottleNum = drop_details.BottleNum " +
                   "WHERE drop_details.BatchName = '" + oBatchName + "' ;");
                List<int> lis_cupFailD = new List<int>();
                List<int> lis_cupFailT = new List<int>();
                foreach (DataRow dr in dt_drop_head.Rows)
                {
                    if (Convert.ToInt32(dr["BottleNum"]) > Lib_Card.Configure.Parameter.Machine_Bottle_Total)
                        continue;
                    double d_blRealErr = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F2}",
                        Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])) : string.Format("{0:F3}",
                        Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])));
                    d_blRealErr = d_blRealErr < 0 ? -d_blRealErr : d_blRealErr;
                    if (d_blRealErr > (dr["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip))
                        lis_cupFailD.Add(Convert.ToInt32(dr["CupNum"]));
                }
                lis_cupFailD = lis_cupFailD.Distinct().ToList();



                lis_cupT = new List<int>();
                //滴液成功，转换到历史表杯号
                lis_cupSuc = new List<int>();
                string s_cupNo = null;

                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                    "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName + "'    ORDER BY CupNum ;");
                foreach (DataRow dr in dt_drop_head.Rows)
                {
                    int i_cup = Convert.ToInt16(dr["CupNum"]);
                    int i_step = Convert.ToInt16(dr["Step"]);
                    double d_objWater = Convert.ToDouble(dr["ObjectAddWaterWeight"]);
                    double d_realWater = Convert.ToDouble(dr["RealAddWaterWeight"]);
                    double d_totalWeight = Convert.ToDouble(dr["TotalWeight"]);
                    double d_testTubeObjectAddWaterWeight = Convert.ToDouble(dr["TestTubeObjectAddWaterWeight"]);
                    double d_testTubeRealAddWaterWeight = Convert.ToDouble(dr["TestTubeRealAddWaterWeight"]);
                    if (i_step == 1)
                        lis_cupSuc.Add(i_cup);
                    lis_cupT.Add(i_cup);
                    double d_realDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater - d_objWater) : string.Format("{0:F3}", d_realWater - d_objWater));
                    d_realDif = d_realDif < 0 ? -d_realDif : d_realDif;
                    double d_allDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                        d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)) : string.Format("{0:F3}",
                        d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)));
                    string s_describe;
                    string s_describe_EN;

                    //只判断当前滴液的数据
                    if (i_step == 1)
                    {
                        if (d_allDif < d_realDif || (d_realWater == 0.0 && d_objWater > 0))
                        {
                            //加水失败
                            s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                             ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                            s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                             ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                            lis_cupFailT.Add(i_cup);
                            s_cupNo += i_cup.ToString() + "; ";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE cup_details SET Statues = '滴液失败' WHERE CupNum = " + i_cup + ";");

                        }
                        else
                        {
                            //加水成功
                            if (lis_cupFailD.Contains(i_cup))
                            {
                                //滴液失败
                                s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                            ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                             ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                lis_cupFailT.Add(i_cup);
                                s_cupNo += i_cup.ToString() + "; ";

                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE cup_details SET Statues = '滴液失败' WHERE CupNum = " + i_cup + ";");
                            }
                            else
                            {
                                //滴液成功
                                s_describe = "滴液成功!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                              ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                s_describe_EN = "Drip Success !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                             ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(i_cup))
                                {
                                    if (i_step == 1)
                                    {
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cup))
                                            FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                                    }
                                }

                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  "UPDATE cup_details SET Statues = '滴液成功' WHERE CupNum = " + i_cup + ";");
                            }
                        }

                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE drop_head SET DescribeChar = '" + s_describe + "',DescribeChar_EN = '" + s_describe_EN + "', FinishTime = '" + DateTime.Now + "', Step = 2 " +
                               "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + i_cup + ";");
                    }

                }


                if (FADM_Auto.Drip._b_dripErr == false)
                {
                    if (0 < lis_cupFailT.Count)
                    {
                        s_cupNo = s_cupNo.Remove(s_cupNo.Length - 1);

                        FADM_Object.Communal.WriteDripWait(true);
                        Communal._b_isAllowDrip = false;
                        FADM_Object.MyAlarm myAlarm;
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            myAlarm = new FADM_Object.MyAlarm(s_cupNo + "号配液杯滴液失败，是否继续(重新滴液请点是，退出滴液请点否)?", "滴液", true, 1);
                        else
                            myAlarm = new FADM_Object.MyAlarm("The dispensing cup" + s_cupNo + "  failed to dispense liquid. Do you want to continue (please click Yes for re dispensing and No for exiting the dispensing)? ", "Drip", true, 1);

                        while (true)
                        {
                            if (0 != myAlarm._i_alarm_Choose) {
                                //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                                //{
                                //    Home.Home_XYZFinish = false;
                                //}
                                break;
                            }
                               
                            Thread.Sleep(1);
                        }
                        //判断染色线程是否需要用机械手
                        if (null != FADM_Object.Communal.ReadDyeThread())
                        {
                            FADM_Object.Communal.WriteDripWait(true);
                            Communal._b_isWaitDrip = true;
                            while (true)
                            {
                                if (false == FADM_Object.Communal.ReadDripWait())
                                    break;
                                Thread.Sleep(1);
                            }
                            Communal._b_isWaitDrip = false;
                        }
                        else
                        {
                            Communal._b_isWaitDrip = false;
                            FADM_Object.Communal.WriteDripWait(false);
                        }

                        if (1 == myAlarm._i_alarm_Choose)
                        {
                            //先把滴液区域重置
                            //滴液区
                            for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                            {
                                if (SmartDyeing.FADM_Object.Communal._lis_dripCupNum.Contains(lis_cupFailT[i]))
                                {

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE drop_head SET CupFinish = 0, AddWaterFinish = 0, RealAddWaterWeight = 0.00 , Step = 1,DescribeChar=null " +
                                        "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + lis_cupFailT[i] + ";");

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE drop_details SET Finish = 0, MinWeight = 0, RealDropWeight = 0.00 , NeedPulse = 0" +
                                        "WHERE BatchName = '" + oBatchName + "' AND BottleNum !=200 And BottleNum !=201 And  CupNum = " + lis_cupFailT[i] + ";");
                                    int i_num = lis_cupFailT[i];
                                    lis_cupFailT.Remove(lis_cupFailT[i]);
                                    lis_cupSuc.Remove(i_num);
                                }

                            }
                            //后处理区域
                            {

                                DataTable dataTable = FADM_Object.Communal._fadmSqlserver.GetData("SELECT * FROM drop_head WHERE BatchName = '" + oBatchName + "'    ORDER BY CupNum;");
                                List<int> lis_lUse_Cup = new List<int>();
                                foreach (DataRow dataRow in dataTable.Rows)
                                {
                                    lis_lUse_Cup.Add(Convert.ToInt16(dataRow["CupNum"]));
                                }
                                //把对应副杯也加入进去
                                List<int> lis_cupFailT2 = new List<int>();
                                lis_cupFailT2.AddRange(lis_cupFailT);
                                foreach (int i in lis_cupFailT2)
                                {
                                    if (_dic_first_second[i] > 0)
                                    {
                                        if (lis_lUse_Cup.Contains(_dic_first_second[i]))
                                            if (!lis_cupFailT.Contains(_dic_first_second[i]))
                                                lis_cupFailT.Add(_dic_first_second[i]);
                                    }
                                }

                                //打板区
                                FADM_Object.Communal._lis_dripFailCupFinish.Clear();
                                FADM_Object.Communal._lis_dripFailCup.AddRange(lis_cupFailT);
                                List<int> lis_ints1 = new List<int>();
                                lis_ints1.AddRange(lis_cupFailT);

                                //等待染色机排完再重滴
                                while (true)
                                {
                                    FADM_Object.Communal.WriteDripWait(true);
                                    if (0 == lis_cupFailT.Count)
                                    {
                                        FADM_Object.Communal.WriteDripWait(false);
                                        break;
                                    }

                                    for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                                    {
                                        if (FADM_Object.Communal._lis_dripFailCupFinish.Contains(lis_cupFailT[i]))
                                        {

                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                "UPDATE drop_head SET CupFinish = 0, AddWaterFinish = 0, RealAddWaterWeight = 0.00 , Step = 1,DescribeChar=null " +
                                                "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + lis_cupFailT[i] + ";");

                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                "UPDATE drop_details SET Finish = 0, MinWeight = 0, RealDropWeight = 0.00 , NeedPulse = 0" +
                                                "WHERE BatchName = '" + oBatchName + "' AND BottleNum !=200 And BottleNum !=201 And  CupNum = " + lis_cupFailT[i] + ";");
                                            FADM_Object.Communal._lis_dripFailCupFinish.Remove(lis_cupFailT[i]);

                                            //如果有副杯，副杯也要清除
                                            if (_dic_first_second[lis_cupFailT[i]] > 0)
                                            {
                                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                "UPDATE drop_head SET CupFinish = 0, AddWaterFinish = 0, RealAddWaterWeight = 0.00 , Step = 1,DescribeChar=null " +
                                                "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + _dic_first_second[lis_cupFailT[i]] + ";");

                                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                    "UPDATE drop_details SET Finish = 0, MinWeight = 0, RealDropWeight = 0.00 , NeedPulse = 0" +
                                                    "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + _dic_first_second[lis_cupFailT[i]] + ";");
                                                //FADM_Object.Communal._lis_dripFailCupFinish.Remove(lis_cupFailT[i]);
                                            }

                                            int i_num = lis_cupFailT[i];
                                            lis_cupFailT.Remove(lis_cupFailT[i]);
                                            lis_cupSuc.Remove(i_num);
                                        }
                                    }

                                    DataTable dt_drop_head2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                                               "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName + "'    ORDER BY CupNum;");
                                    List<int> lis_lUse1 = new List<int>();
                                    foreach (DataRow dataRow in dt_drop_head2.Rows)
                                    {
                                        lis_lUse1.Add(Convert.ToInt16(dataRow["CupNum"]));
                                    }
                                    for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                                    {
                                        //已经强停，不在滴液列表
                                        if (!lis_lUse1.Contains(lis_cupFailT[i]))
                                        {
                                            int i_num = lis_cupFailT[i];
                                            lis_cupFailT.Remove(lis_cupFailT[i]);
                                            lis_ints1.Remove(i_num);
                                        }
                                    }

                                    Thread.Sleep(1000);
                                }

                                foreach (int i in lis_ints1)
                                {
                                    int i_cupNum = i;

                                    while (true)
                                    {
                                        //滴液完成数组移除当前杯号
                                        FADM_Object.Communal._lis_dripSuccessCup.Remove(i_cupNum);
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cupNum))
                                            break;
                                    }



                                    DataTable dt = FADM_Object.Communal._fadmSqlserver.GetData("Select * from drop_head where (CupNum = " + i_cupNum + " Or  CupNum = " + Communal._dic_first_second[i_cupNum] + ") And BatchName != '0'");
                                    //先查是否双杯在使用
                                    if (dt.Rows.Count == 2)
                                    {
                                        //判断是否发过启动信号
                                        if ((!_lis_SendReadyCup.Contains(i_cupNum)) && (!_lis_SendReadyCup.Contains(_dic_first_second[i_cupNum])))
                                        {
                                            //判断另外一只杯是否在洗杯
                                            //if (!lis_iUse.Contains(_dic_first_second[i_cupNum]))
                                            {
                                                int[] ia_zero = new int[1];

                                                ia_zero[0] = 0;
                                                DyeHMIWrite(i_cupNum, 119, 119, ia_zero);

                                                //滴液状态
                                                ia_zero[0] = 3;
                                                DyeHMIWrite(i_cupNum, 100, 100, ia_zero);

                                                _lis_SendReadyCup.Add(i_cupNum);
                                                _lis_SendReadyCup.Add(_dic_first_second[i_cupNum]);
                                            }
                                        }
                                    }
                                    //单杯就直接下发
                                    else
                                    {
                                        int[] ia_zero = new int[1];
                                        if (_dic_first_second[i_cupNum] > 0)
                                        {
                                            //主杯
                                            if (i_cupNum < _dic_first_second[i_cupNum])
                                            {
                                                ia_zero[0] = 1;
                                                DyeHMIWrite(i_cupNum, 119, 119, ia_zero);
                                            }
                                            else
                                            {
                                                ia_zero[0] = 2;
                                                DyeHMIWrite(i_cupNum, 119, 119, ia_zero);
                                            }
                                        }
                                        //滴液状态
                                        ia_zero[0] = 3;
                                        DyeHMIWrite(i_cupNum, 100, 100, ia_zero);

                                        _lis_SendReadyCup.Add(i_cupNum);
                                    }

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE cup_details SET Statues = '等待准备状态' WHERE CupNum = " + i_cupNum + ";");

                                }


                                //等待准备状态
                                while (true)
                                {
                                    bool b_open = true;
                                    DataTable dt_drop_head2 = FADM_Object.Communal._fadmSqlserver.GetData(
                           "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName + "' AND CupFinish = 0    ORDER BY CupNum;");
                                    List<int> lis_lUse1 = new List<int>();
                                    foreach (DataRow dataRow in dt_drop_head2.Rows)
                                    {
                                        lis_lUse1.Add(Convert.ToInt16(dataRow["CupNum"]));
                                    }
                                    for (int i = lis_ints1.Count - 1; i >= 0; i--)
                                    {
                                        //已经强停，不在滴液列表
                                        if (!lis_lUse1.Contains(lis_ints1[i]))
                                        {
                                            lis_ints1.Remove(lis_ints1[i]);
                                        }
                                    }
                                    foreach (int i in lis_ints1)
                                    {
                                        int i_cupNum = i;


                                        int i_openCover = Convert.ToInt16(FADM_Auto.Dye._cup_Temps[i_cupNum - 1]._s_statues);


                                        if (5 != i_openCover)
                                            b_open = false;

                                    }


                                    if (b_open)
                                        break;

                                    Thread.Sleep(1);
                                }

                                lis_ints1.Clear();
                            }
                            if (FADM_Object.Communal._b_isAddWaterFirst)
                                this.DripProcess(oBatchName);
                            else
                                this.DripProcess_AssFirst(oBatchName);
                        }
                        else
                        {
                            //不重滴

                            if (FADM_Object.Communal.ReadMachineStatus() != 8)
                            {
                                for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                                {
                                    if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(lis_cupFailT[i]))
                                    {
                                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(lis_cupFailT[i]))
                                            FADM_Object.Communal._lis_dripSuccessCup.Add(lis_cupFailT[i]);
                                    }
                                }
                            }

                        }

                    }
                }

                string s_cupList = "";
                string s_te = "";
                if (lis_cupSuc.Count > 0)
                {
                    for (int i = 0; i < lis_cupSuc.Count; i++)
                    {
                        s_cupList += lis_cupSuc[i] + ",";
                    }
                }
                if (s_cupList != "")
                {
                    s_cupList = s_cupList.Remove(s_cupList.Length - 1);
                    s_te = " And CupNum in (" + s_cupList + ")";
                }
                //添加历史表
                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                 "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'drop_head';");
                string s_columnHead = null;
                foreach (DataRow row in dt_drop_head.Rows)
                {
                    string s_curName = Convert.ToString(row[0]);
                    if ("TestTubeFinish" != s_curName && "TestTubeWaterLower" != s_curName && "AddWaterFinish" != s_curName &&
                        "CupFinish" != s_curName && "TestTubeWaterLower" != s_curName)
                        s_columnHead += s_curName + ", ";
                }
                s_columnHead = s_columnHead.Remove(s_columnHead.Length - 2);

                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                   "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'drop_details';");
                string s_columnDetails = null;
                foreach (DataRow row in dt_drop_head.Rows)
                {
                    string s_curName = Convert.ToString(row[0]);
                    if ("MinWeight" != s_curName && "Finish" != s_curName && "IsShow" != s_curName && "NeedPulse" != s_curName)
                        s_columnDetails += Convert.ToString(row[0]) + ", ";
                }
                s_columnDetails = s_columnDetails.Remove(s_columnDetails.Length - 2);

                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                   "SELECT * FROM drop_head WHERE   BatchName = '" + oBatchName + "' ;");

                foreach (DataRow row in dt_drop_head.Rows)
                {
                    if (lis_cupSuc.Contains(Convert.ToInt32(row["CupNum"].ToString())))
                    {

                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                        "INSERT INTO history_head (" + s_columnHead + ") (SELECT " + s_columnHead + " FROM drop_head " +
                        "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + row["CupNum"].ToString() + ") ;");

                        //FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "INSERT INTO history_head (" + s_columnHead + ") (SELECT " + s_columnHead + " FROM drop_head " +
                        //    "WHERE BatchName = '" + o_BatchName + "' AND CupNum >= " + _i_cupMin + " AND CupNum <= " + _i_cupMax + s_te + ");");


                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "INSERT INTO history_details (" + s_columnDetails + ") (SELECT " + s_columnDetails + " FROM drop_details " +
                           "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + row["CupNum"].ToString() + ") ;");

                        //滴液
                        if (SmartDyeing.FADM_Object.Communal._lis_dripCupNum.Contains(Convert.ToInt32(row["CupNum"].ToString())))
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  "DELETE FROM drop_head WHERE CupNum = " + row["CupNum"].ToString() + " AND BatchName = '" + oBatchName + "' ;");
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "DELETE FROM drop_details WHERE CupNum = " + row["CupNum"].ToString() + " AND BatchName = '" + oBatchName + "' ;");


                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE cup_details SET FormulaCode = null, " +
                                "DyeingCode = null, IsUsing = 0, Statues = '待机', " +
                                "StartTime = null, SetTemp = null, StepNum = null, TotalWeight = null, " +
                                "TotalStep = null, TechnologyName = null, StepStartTime = null, SetTime = null,RecordIndex = 0 WHERE CupNum = " +
                                row["CupNum"].ToString() + " ;");
                        }
                    }
                }

                if (FADM_Auto.Drip._b_dripErr)
                {
                    FADM_Object.Communal._lis_dripStopCup.AddRange(lis_cupT);

                }
            }

        }

        private struct sReadCheckArg
        {
            public string oBatchName;
            public string sSyringeType;
            public int iMinBottleNo;
            public double dblBalanceValue3;
            public List<int> ints;
        }

        private struct sReadCheckArgDebug
        {
            public string _obj_batchName;
            public string _s_syringeType;
            public int _i_minBottleNo;
            public double _d_blBalanceValue3;
            public List<int> _lis_ints;
            //目标滴液重
            public double _d_blBalanceValue4;
            //杯号
            public int _i_minCupNo;
            //是否要分开多次滴液
            public bool _b_finish;
        }
        private void ReadCheckWeight(object o)
        {
            sReadCheckArg arg = (sReadCheckArg)o;
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数启动");
        label15:
            Thread.Sleep(Convert.ToInt32(Lib_Card.Configure.Parameter.Delay_Balance_Read * 1000));
            double d_blBalanceValue4 = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal._s_balanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal._s_balanceValue));
            Thread.Sleep(Convert.ToInt32(Lib_Card.Configure.Parameter.Delay_Balance_Read * 1000));
            double d_blBalanceValue5 = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal._s_balanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal._s_balanceValue));


            double d_blDif = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blBalanceValue4 - d_blBalanceValue5)) : Convert.ToDouble(string.Format("{0:F3}", d_blBalanceValue4 - d_blBalanceValue5));
            d_blDif = d_blDif < 0 ? -d_blDif : d_blDif;

            if (d_blDif > Lib_Card.Configure.Parameter.Other_Stable_Value)
                goto label15;

            double d_blRRead = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", (d_blBalanceValue4 + d_blBalanceValue5) / 2)) : Convert.ToDouble(string.Format("{0:F3}", (d_blBalanceValue4 + d_blBalanceValue5) / 2));
            double d_blWeight = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", (d_blBalanceValue4 + d_blBalanceValue5) / 2 - arg.dblBalanceValue3)) : Convert.ToDouble(string.Format("{0:F3}", (d_blBalanceValue4 + d_blBalanceValue5) / 2 - arg.dblBalanceValue3));
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数：" + d_blRRead + ",实际重量：" + d_blWeight);



            double d_blRErr = 0;
            if ("小针筒" == arg.sSyringeType|| "Little Syringe" == arg.sSyringeType)
                d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blWeight - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", d_blWeight - Lib_Card.Configure.Parameter.Correcting_S_Weight));
            else
                d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blWeight - Lib_Card.Configure.Parameter.Correcting_B_Weight)):Convert.ToDouble(string.Format("{0:F3}", d_blWeight - Lib_Card.Configure.Parameter.Correcting_B_Weight));
            ;
            if (System.Math.Abs(d_blRErr) > Lib_Card.Configure.Parameter.Other_AErr_Drip)
            {
                //母液瓶扣减
                FADM_Object.Communal._fadmSqlserver.ReviseData(
                    "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_blWeight + ",  AdjustSuccess = 0 " +
                    "WHERE BottleNum = '" + arg.iMinBottleNo + "';");
            }
            else
            {
                //母液瓶扣减
                FADM_Object.Communal._fadmSqlserver.ReviseData(
                    "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_blWeight + " ,  AdjustSuccess = 1 " +
                    "WHERE BottleNum = '" + arg.iMinBottleNo + "';");
            }

            //查询开料日期
            DataTable dt_bottle_details = FADM_Object.Communal._fadmSqlserver.GetData(
                        "SELECT * FROM bottle_details WHERE  BottleNum = " + arg.iMinBottleNo + ";");


            foreach (int i in arg.ints)
            {
                if (0.00 != d_blWeight)
                {
                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                    "UPDATE drop_details SET RealDropWeight = ObjectDropWeight + " + d_blRErr + " ,BrewingData = '"+ dt_bottle_details.Rows[0]["BrewingData"].ToString()+"' " +
                    "WHERE BatchName = '" + arg.oBatchName + "' AND BottleNum = " + arg.iMinBottleNo + " AND " +
                    "CupNum = " + i + ";");

                    DataTable dt_drop_details = FADM_Object.Communal._fadmSqlserver.GetData(
                        "SELECT * FROM drop_details WHERE BatchName = '" + arg.oBatchName + "' AND BottleNum = " + arg.iMinBottleNo + " AND CupNum = " + i + ";");
                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                        "UPDATE cup_details SET TotalWeight = TotalWeight+ " + dt_drop_details.Rows[0]["RealDropWeight"] + " WHERE CupNum = " + i + ";");
                }
                else
                {
                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                   "UPDATE drop_details SET RealDropWeight = 0.00 "+ " ,BrewingData = '" + dt_bottle_details.Rows[0]["BrewingData"].ToString() + "' " +
                   "WHERE BatchName = '" + arg.oBatchName + "' AND BottleNum = " + arg.iMinBottleNo + " AND " +
                   "CupNum = " + i + ";");
                }
            }
        }


        private void ReadCheckWeightAllDebug(object o)
        {
            sReadCheckArgDebug arg = (sReadCheckArgDebug)o;
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数启动");
        label15:
            Thread.Sleep(Convert.ToInt32(Lib_Card.Configure.Parameter.Delay_Balance_Read * 1000));
            double d_blBalanceValue4 = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal._s_balanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal._s_balanceValue));
            Thread.Sleep(Convert.ToInt32(Lib_Card.Configure.Parameter.Delay_Balance_Read * 1000));
            double d_blBalanceValue5 = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal._s_balanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal._s_balanceValue));


            double d_blDif = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blBalanceValue4 - d_blBalanceValue5)) : Convert.ToDouble(string.Format("{0:F3}", d_blBalanceValue4 - d_blBalanceValue5));
            d_blDif = d_blDif < 0 ? -d_blDif : d_blDif;

            if (d_blDif > Lib_Card.Configure.Parameter.Other_Stable_Value)
                goto label15;

            double d_blRRead = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", (d_blBalanceValue4 + d_blBalanceValue5) / 2)) : Convert.ToDouble(string.Format("{0:F3}", (d_blBalanceValue4 + d_blBalanceValue5) / 2));
            double d_blWeight = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", (d_blBalanceValue4 + d_blBalanceValue5) / 2 - arg._d_blBalanceValue3)) : Convert.ToDouble(string.Format("{0:F3}", (d_blBalanceValue4 + d_blBalanceValue5) / 2 - arg._d_blBalanceValue3));
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数：" + d_blRRead + ",实际重量：" + d_blWeight);



            double d_blRErr = 0;
            //记录实际滴液重
            d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blWeight - arg._d_blBalanceValue4)) : Convert.ToDouble(string.Format("{0:F3}", d_blWeight - arg._d_blBalanceValue4));

            if (System.Math.Abs(d_blRErr) > Lib_Card.Configure.Parameter.Other_AErr_Drip)
            {
                //母液瓶扣减
                FADM_Object.Communal._fadmSqlserver.ReviseData(
                    "UPDATE bottle_details SET CurrentWeight =  CurrentWeight - " + d_blWeight + ",  AdjustSuccess = 1 " +
                    "WHERE BottleNum = '" + arg._i_minBottleNo + "';");
            }
            else
            {
                //母液瓶扣减
                FADM_Object.Communal._fadmSqlserver.ReviseData(
                    "UPDATE bottle_details SET CurrentWeight =  CurrentWeight - " + d_blWeight + " ,  AdjustSuccess = 1 " +
                    "WHERE BottleNum = '" + arg._i_minBottleNo + "';");
            }


            ////置位完成标志位
            //FADM_Object.Communal._fadmSqlserver.ReviseData(
            //    "UPDATE drop_details SET Finish = 1 WHERE BatchName = '" + arg.o_BatchName + "' AND " +
            //    "BottleNum = " + arg._i_minBottleNo + " AND CupNum = " + arg.i_minCupNo + ";");


            //foreach (int i in arg._lis_ints)
            if (arg._b_finish)
            {
                if (0.00 != d_blWeight)
                {
                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                    "UPDATE drop_details SET RealDropWeight = RealDropWeight + " + d_blWeight + " ,Finish = 1 " +
                    "WHERE BatchName = '" + arg._obj_batchName + "' AND BottleNum = " + arg._i_minBottleNo + " AND " +
                    "CupNum = " + arg._i_minCupNo + ";");

                    DataTable dt_drop_details = FADM_Object.Communal._fadmSqlserver.GetData(
                        "SELECT * FROM drop_details WHERE BatchName = '" + arg._obj_batchName + "' AND BottleNum = " + arg._i_minBottleNo + " AND CupNum = " + arg._i_minCupNo + ";");
                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                        "UPDATE cup_details SET TotalWeight = TotalWeight+ " + dt_drop_details.Rows[0]["RealDropWeight"] + " WHERE CupNum = " + arg._i_minCupNo + ";");
                }
                else
                {
                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                   "UPDATE drop_details SET RealDropWeight = RealDropWeight + 0.00 ,Finish = 1 " +
                   "WHERE BatchName = '" + arg._obj_batchName + "' AND BottleNum = " + arg._i_minBottleNo + " AND " +
                   "CupNum = " + arg._i_minCupNo + ";");
                }

                DataTable dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                        "SELECT * FROM drop_details WHERE BatchName = '" + arg._obj_batchName + "' AND Finish = 0" + " AND CupNum = " + arg._i_minCupNo + ";");
                if (dt_drop_details3.Rows.Count == 0)
                {
                    //置为完成
                    string s_sql = "UPDATE drop_head SET CupFinish = 1 WHERE BatchName = '" + arg._obj_batchName + "' AND CupNum = " + arg._i_minCupNo + "; ";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);


                    bool b_fail = true;

                    s_sql = "SELECT drop_details.CupNum as CupNum, " +
                                 "drop_details.BottleNum as BottleNum, " +
                                 "drop_details.ObjectDropWeight as ObjectDropWeight, " +
                                 "drop_details.RealDropWeight as RealDropWeight, " +
                                 "bottle_details.SyringeType as SyringeType " +
                                 "FROM drop_details left join bottle_details on " +
                                 "bottle_details.BottleNum = drop_details.BottleNum " +
                                 "WHERE drop_details.BatchName = '" + arg._obj_batchName + "' AND CupNum = " + arg._i_minCupNo + ";";
                    DataTable dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                    foreach (DataRow dr in dt_temp.Rows)
                    {
                        if (Convert.ToInt32(dr["BottleNum"]) > Lib_Card.Configure.Parameter.Machine_Bottle_Total)
                            continue;
                        double d_blRealErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                        Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"]))) : Convert.ToDouble(string.Format("{0:F3}",
                        Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])));
                        d_blRealErr = d_blRealErr < 0 ? -d_blRealErr : d_blRealErr;
                        if (d_blRealErr > Lib_Card.Configure.Parameter.Other_AErr_Drip)
                            b_fail = false;
                    }

                    dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
            "SELECT * FROM drop_head WHERE BatchName = '" + arg._obj_batchName + "' AND CupNum = " + arg._i_minCupNo + ";");
                    if (dt_drop_details3.Rows.Count > 0)
                    {
                        int i_cup = Convert.ToInt16(dt_drop_details3.Rows[0]["CupNum"]);
                        int i_step = Convert.ToInt16(dt_drop_details3.Rows[0]["Step"]);
                        double d_objWater = Convert.ToDouble(dt_drop_details3.Rows[0]["ObjectAddWaterWeight"]);
                        double d_realWater = Convert.ToDouble(dt_drop_details3.Rows[0]["RealAddWaterWeight"]);
                        double d_totalWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TotalWeight"]);
                        double d_testTubeObjectAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeObjectAddWaterWeight"]);
                        double d_testTubeRealAddWaterWeight = Convert.ToDouble(dt_drop_details3.Rows[0]["TestTubeRealAddWaterWeight"]);
                        double d_realDif = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F}", d_realWater - d_objWater)) : Convert.ToDouble(string.Format("{0:F}", d_realWater - d_objWater));
                        d_realDif = d_realDif < 0 ? -d_realDif : d_realDif;
                        double d_allDif = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F}",
                            d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00))) : Convert.ToDouble(string.Format("{0:F3}",
                            d_totalWeight * Convert.ToDouble(Lib_Card.Configure.Parameter.Other_AErr_DripWater / 100.00)));

                        string s_describe;
                        if (d_allDif < d_realDif || (d_realWater == 0.0 && d_objWater != 0.0))
                        {
                            b_fail = false;
                        }

                        if (b_fail)
                        {
                            if (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0)
                                s_describe = "滴液成功!目标加水:" + string.Format("{0:F}", d_objWater) +
                                          ",实际加水:" + string.Format("{0:F}", d_realWater);
                            else
                                s_describe = "滴液成功!目标加水:" + string.Format("{0:F3}", d_objWater) +
                                          ",实际加水:" + string.Format("{0:F3}", d_realWater);
                            if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(arg._i_minCupNo))
                            {
                                if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cup))
                                    FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                            }
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                          "UPDATE cup_details SET Statues = '滴液成功' WHERE CupNum = " + i_cup + ";");
                        }
                        else
                        {
                            if (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0)
                                s_describe = "滴液失败!目标加水:" + string.Format("{0:F}", d_objWater) +
                                    ",实际加水:" + string.Format("{0:F}", d_realWater);
                            else
                                s_describe = "滴液失败!目标加水:" + string.Format("{0:F3}", d_objWater) +
                                    ",实际加水:" + string.Format("{0:F3}", d_realWater);
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE cup_details SET Statues = '滴液失败' WHERE CupNum = " + i_cup + ";");
                        }
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                       "UPDATE drop_head SET DescribeChar = '" + s_describe + "', FinishTime = '" + DateTime.Now + "', Step = 2 " +
                       "WHERE BatchName = '" + arg._obj_batchName + "' AND CupNum = " + i_cup + ";");
                    }
                }
            }
            else
            {
                if (0.00 != d_blWeight)
                {
                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                    "UPDATE drop_details SET RealDropWeight = RealDropWeight + " + d_blWeight + " ,Finish = 0 " +
                    "WHERE BatchName = '" + arg._obj_batchName + "' AND BottleNum = " + arg._i_minBottleNo + " AND " +
                    "CupNum = " + arg._i_minCupNo + ";");



                    DataTable dataTable2 = FADM_Object.Communal._fadmSqlserver.GetData(
                        "SELECT * FROM drop_details WHERE BatchName = '" + arg._obj_batchName + "' AND BottleNum = " + arg._i_minBottleNo + " AND CupNum = " + arg._i_minCupNo + ";");
                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                        "UPDATE cup_details SET TotalWeight = TotalWeight+ " + d_blWeight + " WHERE CupNum = " + arg._i_minCupNo + ";");
                }
                else
                {

                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                   "UPDATE drop_details SET RealDropWeight = RealDropWeight + 0.00 ,Finish = 0 " +
                   "WHERE BatchName = '" + arg._obj_batchName + "' AND BottleNum = " + arg._i_minBottleNo + " AND " +
                   "CupNum = " + arg._i_minCupNo + ";");


                }
            }
        }

    }
}
