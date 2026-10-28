using SmartDyeing.FADM_Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Threading;
using System.Windows.Forms;
using Lib_Card;
using Newtonsoft.Json.Linq;

namespace SmartDyeing.FADM_Auto
{
    internal class Water
    {
        public void Check()
        {
            try
            {
                FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "水校正启动");

                MyModbusFun.Reset();
                //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                //{
                //    Lib_Card.ADT8940A1.Module.Home.Home.Home_XYZFinish = false;
                //}
                FADM_Object.Communal.WriteMachineStatus(5);
                //回零
                //if (!Lib_Card.ADT8940A1.Module.Home.Home.Home_XYZFinish)
                //{
                //    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "回零启动");
                //    int reSuccess = MyModbusFun.goHome();//回零 方法里异常直接抛出
                //    if (reSuccess == 0)
                //    {
                //        Lib_Card.ADT8940A1.Module.Home.Home.Home_XYZFinish = true;
                //        Console.WriteLine("回零成功");
                //    }

                //    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "回零完成");
                //}
                

                //Lib_SerialPort.Balance.METTLER.bReSetSign = true;
                Thread.Sleep(500);


                //判断是否异常
                FADM_Object.Communal.BalanceState("水校正");
                double d_blBalanceValue0 = 0;

                //如果转盘水校正，先读初始值
                if (FADM_Object.Communal._b_isUseTurntableCorrectingWater)
                {
                    //记录初始天平读数
                    d_blBalanceValue0 = FADM_Object.Communal.SteBalance(); ;
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平读数：" + d_blBalanceValue0);
                }

                int i_count = 0;
                int i_mRes = -1;

            label4:
                if (FADM_Object.Communal._b_isUseNewCorrectingWater)
                {
                    //Lib_SerialPort.Balance.METTLER.bZeroSign = true;

                    if (FADM_Object.Communal._b_isUseTurntableCorrectingWater)
                    {
                        //移动母液瓶
                        try
                        {
                            //执行母液瓶移动
                            Communal.MoveBottle(6, 5);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message == "-97")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "在转盘工位取瓶失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "放瓶时气缸下失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else
                                throw;
                        }

                        //移动到水校正位置
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找水校正位置");
                        i_mRes = MyModbusFun.TargetMove(21, 0, 1);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达水校正位置");

                        //

                    }
                    else
                    {
                        //移动到天平位
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找天平位");
                        i_mRes = MyModbusFun.TargetMove(2, 0, 1);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达天平位");
                    }

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

                    if (!FADM_Object.Communal._b_isUseTurntableCorrectingWater)
                    {
                        //记录初始天平读数
                        d_blBalanceValue0 = FADM_Object.Communal.SteBalance(); ;
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平读数：" + d_blBalanceValue0);
                    }



                    //伸出接液盘

                    //加水
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "加水启动(" + Lib_Card.Configure.Parameter.Correcting_Water_Time + "s)");

                    double d_addWaterTime = Lib_Card.Configure.Parameter.Correcting_Water_Time;
                    FADM_Object.Communal.AddWater(d_addWaterTime);


                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "加水完成");

                    if (FADM_Object.Communal._b_isUseTurntableCorrectingWater)
                    {
                        //移动母液瓶
                        try
                        {
                            //执行母液瓶移动
                            Communal.MoveBottle(6, 6);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message == "-97")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "在转盘工位取瓶失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "放瓶时气缸下失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else
                                throw;
                        }
                    }

                    //读取校正重量
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数启动");
                    double d_blRRead = FADM_Object.Communal.SteBalance();
                    double d_blWeightT = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blRRead - d_blBalanceValue0)) : Convert.ToDouble(string.Format("{0:F3}", d_blRRead - d_blBalanceValue0));
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数：" + d_blRRead + ",实际重量：" + d_blWeightT);
                    double d_blAdjust = Convert.ToDouble(string.Format("{0:F3}", d_blWeightT / Convert.ToDouble(Lib_Card.Configure.Parameter.Correcting_Water_Time)));
                    double d_blAdjust_Pulse = 0;
                    if (Communal._b_isUseLLJ)
                    {
                        //计算脉冲系数
                        MyModbusFun.ReadFlow();
                        if (Communal.i_flowPulse == 0)
                        {
                            d_blAdjust_Pulse = 0.6;
                        }
                        else
                        {
                            d_blAdjust_Pulse = Convert.ToDouble(string.Format("{0:F3}", d_blWeightT / Communal.i_flowPulse));
                        }
                        //d_blAdjust_Pulse = Convert.ToDouble(string.Format("{0:F3}", d_blWeightT / Communal.i_flowPulse));
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "脉冲：" + Communal.i_flowPulse + ",实际重量：" + d_blWeightT);
                    }
                    if (d_blAdjust <= 0.0 && i_count < 4)
                    {
                        i_count++;
                        goto label4;
                    }
                    if (i_count == 4)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            new FADM_Object.MyAlarm("没有水，停止水校正", 1);
                        else
                            new FADM_Object.MyAlarm("There is no water. Stop the water calibration", 1);
                        FADM_Object.Communal.WriteMachineStatus(0);

                        //解锁6工位
                        MyModbusFun.UnLockSix();
                        return;
                    }

                    d_blBalanceValue0 = d_blRRead;

                    //验证
                    double d_blTime = 0;
                    d_blTime = Lib_Card.Configure.Parameter.Correcting_Water_RWeight / d_blAdjust;

                    if (FADM_Object.Communal._b_isUseTurntableCorrectingWater)
                    {
                        //移动母液瓶
                        try
                        {
                            //执行母液瓶移动
                            Communal.MoveBottle(6, 5);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message == "-97")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "在转盘工位取瓶失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "放瓶时气缸下失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else
                                throw;
                        }

                        //移动到水校正位置
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找水校正位置");
                        i_mRes = MyModbusFun.TargetMove(21, 0, 1);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达水校正位置");

                        //

                    }

                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "加水启动(" + string.Format("{0:F3}", d_blTime) + "s)");
                    d_addWaterTime = d_blTime;
                    FADM_Object.Communal.AddWater(d_blTime);

                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "加水完成");

                    if (FADM_Object.Communal._b_isUseTurntableCorrectingWater)
                    {
                        //移动母液瓶
                        try
                        {
                            //执行母液瓶移动
                            Communal.MoveBottle(6, 6);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message == "-97")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "在转盘工位取瓶失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "放瓶时气缸下失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else
                                throw;
                        }
                    }

                    //读取校正重量
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数启动");
                    d_blRRead = FADM_Object.Communal.SteBalance();
                    double d_blWeightC = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blRRead - d_blBalanceValue0)) : Convert.ToDouble(string.Format("{0:F3}", d_blRRead - d_blBalanceValue0));
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数：" + d_blRRead + ",实际重量：" + d_blWeightC);
                    if (Communal._b_isUseLLJ)
                    {
                        //计算脉冲系数
                        MyModbusFun.ReadFlow();
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "脉冲：" + Communal.i_flowPulse + ",实际重量：" + d_blWeightC);
                    }
                    
                    int i_rErr = Convert.ToInt16((d_blWeightC - Lib_Card.Configure.Parameter.Correcting_Water_RWeight) / Lib_Card.Configure.Parameter.Correcting_Water_RWeight * 100);
                    i_rErr = i_rErr < 0 ? -i_rErr : i_rErr;
                    //解锁6工位
                    MyModbusFun.UnLockSix();
                    if (i_rErr > Lib_Card.Configure.Parameter.Other_AErr_Water)
                    {
                    lable5:
                        FADM_Object.MyAlarm myAlarm;
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            myAlarm = new FADM_Object.MyAlarm("水校正失败，是否继续?(继续水校正请点是，退出水校正请点否)", "加水校正", true, 1);
                        else
                            myAlarm = new FADM_Object.MyAlarm("Water calibration failed, do you want to continue? (To continue with water calibration, please click Yes. To exit water calibration, please click No)", "Water correction", true, 1);
                        while (true)
                        {
                            if (0 != myAlarm._i_alarm_Choose)
                                break;
                            Thread.Sleep(1);
                        }

                        if (1 == myAlarm._i_alarm_Choose)
                            goto label4;

                    }
                    else
                    {
                        //Lib_Card.Configure.Parameter.Correcting_Water_FWeight = Convert.ToDouble(string.Format("{0:F3}", d_blWeightF));
                        Lib_Card.Configure.Parameter.Correcting_Water_Value = Convert.ToDouble(string.Format("{0:F3}", d_blAdjust));
                        //Lib_File.Ini.WriteIni("Correcting", "Correcting_Water_FWeight", string.Format("{0:F3}", d_blWeightF), Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Correcting", "Correcting_Water_Value", string.Format("{0:F3}", d_blAdjust), Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        if (Communal._b_isUseLLJ)
                        {
                            Lib_Card.Configure.Parameter.Correcting_FlowPulse_Value = Convert.ToDouble(string.Format("{0:F3}", d_blAdjust_Pulse));
                            Lib_File.Ini.WriteIni("Correcting", "Correcting_FlowPulse_Value", string.Format("{0:F3}", d_blAdjust_Pulse), Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        }
                    }

                    if (Communal._b_isGetPowerClamp)
                    {
                        int i_xStart = 0, i_yStart = 0;
                        //3.放夹子
                        FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "放夹子启动");
                        //int i_xStart = 0, i_yStart = 0;
                        MyModbusFun.CalTarget(14, 0, ref i_xStart, ref i_yStart);
                        int i_mRes1 = MyModbusFun.PutClamp(i_xStart, i_yStart);
                        if (-2 == i_mRes1)
                            throw new Exception("收到退出消息");
                        FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "放夹子完成");
                    }


                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找待机位");
                    FADM_Object.Communal._i_OptCupNum = 0;
                    FADM_Object.Communal._i_optBottleNum = 0;
                    i_mRes = MyModbusFun.TargetMove(3, 0, 1);
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                    //不回待机位，失能关闭
                    MyModbusFun.Power(2);
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达待机位");

                    //报警动作待定
                    /*Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer buzzer = new Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer_Basic();
                    if (-1 == buzzer.Buzzer_On())
                        throw new Exception("驱动异常");
                    Thread.Sleep(Convert.ToInt32(Lib_Card.Configure.Parameter.Delay_Buzzer_Finish * 1000));
                    if (-1 == buzzer.Buzzer_Off())
                        throw new Exception("驱动异常");*/

                    FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "水校正完成");
                    FADM_Object.Communal.WriteMachineStatus(0);
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        new FADM_Object.MyAlarm("水校正完成", 1);
                    else
                        new FADM_Object.MyAlarm("Water correction completed", 1);

                    //Lib_SerialPort.Balance.METTLER.bReSetSign = true;
                }
                else
                {
                    //Lib_SerialPort.Balance.METTLER.bZeroSign = true;

                    if (FADM_Object.Communal._b_isUseTurntableCorrectingWater)
                    {
                        //移动母液瓶
                        try
                        {
                            //执行母液瓶移动
                            Communal.MoveBottle(6, 5);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message == "-97")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "在转盘工位取瓶失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "放瓶时气缸下失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else
                                throw;
                        }

                        //移动到水校正位置
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找水校正位置");
                        i_mRes = MyModbusFun.TargetMove(21, 0, 1);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达水校正位置");

                        //

                    }
                    else
                    {
                        //移动到天平位
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找天平位");
                        i_mRes = MyModbusFun.TargetMove(2, 0, 1);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达天平位");
                    }

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

                    if (!FADM_Object.Communal._b_isUseTurntableCorrectingWater)
                    {
                        //记录初始天平读数
                        d_blBalanceValue0 = FADM_Object.Communal.SteBalance(); ;
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平读数：" + d_blBalanceValue0);
                    }

                    //伸出接液盘

                    double d_addWaterTime2 = MyModbusFun.GetWaterTime(Lib_Card.Configure.Parameter.Correcting_Water_RWeight);//加水时间 校正加水时间
                    i_mRes = MyModbusFun.AddWater(1); //加水1秒
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");

                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "加水完成");

                    if (FADM_Object.Communal._b_isUseTurntableCorrectingWater)
                    {
                        //移动母液瓶
                        try
                        {
                            //执行母液瓶移动
                            Communal.MoveBottle(6, 6);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message == "-97")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "在转盘工位取瓶失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "放瓶时气缸下失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else
                                throw;
                        }
                    }

                    //读取首秒重量
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数启动");
                    double d_blRRead = FADM_Object.Communal.SteBalance();
                    double d_blWeightF = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blRRead - d_blBalanceValue0)) : Convert.ToDouble(string.Format("{0:F3}", d_blRRead - d_blBalanceValue0));
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数：" + d_blRRead + ",实际重量：" + d_blWeightF);

                    if (d_blWeightF <= 0.0 && i_count < 4)
                    {
                        i_count++;
                        goto label4;
                    }
                    if (i_count == 4)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            new FADM_Object.MyAlarm("没有水，停止水校正", 1);
                        else
                            new FADM_Object.MyAlarm("There is no water. Stop the water calibration", 1);
                        FADM_Object.Communal.WriteMachineStatus(0);
                        //解锁6工位
                        MyModbusFun.UnLockSix();
                        return;
                    }

                    //if (d_blWeightF <= 0.0)
                    //{
                    //    goto label4;
                    //}


                    if (FADM_Object.Communal._b_isUseTurntableCorrectingWater)
                    {
                        //移动母液瓶
                        try
                        {
                            //执行母液瓶移动
                            Communal.MoveBottle(6, 5);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message == "-97")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "在转盘工位取瓶失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "放瓶时气缸下失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else
                                throw;
                        }

                        //移动到水校正位置
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找水校正位置");
                        i_mRes = MyModbusFun.TargetMove(21, 0, 1);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达水校正位置");

                        //

                    }
                    //加水
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "加水启动(" + Lib_Card.Configure.Parameter.Correcting_Water_Time + "s)");

                    double d_addWaterTime = Lib_Card.Configure.Parameter.Correcting_Water_Time;
                    FADM_Object.Communal.AddWater(d_addWaterTime);
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "加水完成");

                    if (FADM_Object.Communal._b_isUseTurntableCorrectingWater)
                    {
                        //移动母液瓶
                        try
                        {
                            //执行母液瓶移动
                            Communal.MoveBottle(6, 6);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message == "-97")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "在转盘工位取瓶失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "放瓶时气缸下失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else
                                throw;
                        }
                    }

                    //读取校正重量
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数启动");
                    d_blRRead = FADM_Object.Communal.SteBalance();
                    double d_blWeightT = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blRRead - d_blBalanceValue0)) : Convert.ToDouble(string.Format("{0:F3}", d_blRRead - d_blBalanceValue0));
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数：" + d_blRRead + ",实际重量：" + d_blWeightT);
                    double d_blAdjust = Convert.ToDouble(string.Format("{0:F3}", d_blWeightT / Convert.ToDouble(Lib_Card.Configure.Parameter.Correcting_Water_Time)));
                    //if (d_blAdjust <= 0.0)
                    //{
                    //    goto label4;
                    //}

                    if (d_blAdjust <= 0.0 && i_count < 4)
                    {
                        i_count++;
                        goto label4;
                    }
                    if (i_count == 4)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            new FADM_Object.MyAlarm("没有水，停止水校正", 1);
                        else
                            new FADM_Object.MyAlarm("There is no water. Stop the water calibration", 1);
                        FADM_Object.Communal.WriteMachineStatus(0);
                        //解锁6工位
                        MyModbusFun.UnLockSix();
                        return;
                    }

                    d_blBalanceValue0 = d_blRRead;

                    //验证
                    double d_blTime = 0;
                    if (0 == FADM_Object.Communal._i_waterWay)
                    {
                        if (Lib_Card.Configure.Parameter.Correcting_Water_RWeight > d_blWeightF)
                            d_blTime = (Lib_Card.Configure.Parameter.Correcting_Water_RWeight - d_blWeightF) / d_blAdjust + 1;
                        else
                        {
                            d_blTime = Lib_Card.Configure.Parameter.Correcting_Water_RWeight / d_blAdjust;
                            if (d_blTime < 0.01)
                                d_blTime = 0.01;
                        }
                    }
                    else
                    {
                        if (Lib_Card.Configure.Parameter.Correcting_Water_RWeight > 13)
                            d_blTime = (Lib_Card.Configure.Parameter.Correcting_Water_RWeight - d_blWeightF) / d_blAdjust + 1;
                        else
                            d_blTime = Lib_Card.Configure.Parameter.Correcting_Water_RWeight / d_blAdjust + Lib_Card.Configure.Parameter.Other_Coefficient_Water;
                    }

                    if (FADM_Object.Communal._b_isUseTurntableCorrectingWater)
                    {
                        //移动母液瓶
                        try
                        {
                            //执行母液瓶移动
                            Communal.MoveBottle(6, 5);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message == "-97")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "在转盘工位取瓶失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "放瓶时气缸下失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else
                                throw;
                        }

                        //移动到水校正位置
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找水校正位置");
                        i_mRes = MyModbusFun.TargetMove(21, 0, 1);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达水校正位置");

                        //

                    }

                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "加水启动(" + string.Format("{0:F3}", d_blTime) + "s)");

                    FADM_Object.Communal.AddWater(d_blTime);

                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "加水完成");

                    if (FADM_Object.Communal._b_isUseTurntableCorrectingWater)
                    {
                        //移动母液瓶
                        try
                        {
                            //执行母液瓶移动
                            Communal.MoveBottle(6, 6);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message == "-97")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "在转盘工位取瓶失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "放瓶时气缸下失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else
                                throw;
                        }
                    }

                    //读取校正重量
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数启动");
                    d_blRRead = FADM_Object.Communal.SteBalance();
                    double d_blWeightC = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blRRead - d_blBalanceValue0)) : Convert.ToDouble(string.Format("{0:F3}", d_blRRead - d_blBalanceValue0));
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数：" + d_blRRead + ",实际重量：" + d_blWeightC);
                    

                    int i_rErr = Convert.ToInt16((d_blWeightC - Lib_Card.Configure.Parameter.Correcting_Water_RWeight) / Lib_Card.Configure.Parameter.Correcting_Water_RWeight * 100);
                    i_rErr = i_rErr < 0 ? -i_rErr : i_rErr;
                    //解锁6工位
                    MyModbusFun.UnLockSix();
                    if (i_rErr > Lib_Card.Configure.Parameter.Other_AErr_Water)
                    {
                    lable5:
                        FADM_Object.MyAlarm myAlarm;
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            myAlarm = new FADM_Object.MyAlarm("水校正失败，是否继续?(继续水校正请点是，退出水校正请点否)", "加水校正", true, 1);
                        else
                            myAlarm = new FADM_Object.MyAlarm("Water calibration failed, do you want to continue? (To continue with water calibration, please click Yes. To exit water calibration, please click No)", "Water correction", true, 1);
                        while (true)
                        {
                            if (0 != myAlarm._i_alarm_Choose)
                                break;
                            Thread.Sleep(1);
                        }

                        if (1 == myAlarm._i_alarm_Choose)
                            goto label4;

                    }
                    else
                    {
                        Lib_Card.Configure.Parameter.Correcting_Water_FWeight = Convert.ToDouble(string.Format("{0:F3}", d_blWeightF));
                        Lib_Card.Configure.Parameter.Correcting_Water_Value = Convert.ToDouble(string.Format("{0:F3}", d_blAdjust));
                        Lib_File.Ini.WriteIni("Correcting", "Correcting_Water_FWeight", string.Format("{0:F3}", d_blWeightF), Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Correcting", "Correcting_Water_Value", string.Format("{0:F3}", d_blAdjust), Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    }

                    if (Communal._b_isGetPowerClamp)
                    {
                        int i_xStart = 0, i_yStart = 0;
                        //3.放夹子
                        FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "放夹子启动");
                        //int i_xStart = 0, i_yStart = 0;
                        MyModbusFun.CalTarget(14, 0, ref i_xStart, ref i_yStart);
                        int i_mRes1 = MyModbusFun.PutClamp(i_xStart, i_yStart);
                        if (-2 == i_mRes1)
                            throw new Exception("收到退出消息");
                        FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "放夹子完成");
                    }

                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找待机位");
                    FADM_Object.Communal._i_OptCupNum = 0;
                    FADM_Object.Communal._i_optBottleNum = 0;
                    i_mRes = MyModbusFun.TargetMove(3, 0, 1);
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                    //不回待机位，失能关闭
                    MyModbusFun.Power(2);
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达待机位");

                    //报警动作待定
                    /*Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer buzzer = new Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer_Basic();
                    if (-1 == buzzer.Buzzer_On())
                        throw new Exception("驱动异常");
                    Thread.Sleep(Convert.ToInt32(Lib_Card.Configure.Parameter.Delay_Buzzer_Finish * 1000));
                    if (-1 == buzzer.Buzzer_Off())
                        throw new Exception("驱动异常");*/

                    FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "水校正完成");
                    FADM_Object.Communal.WriteMachineStatus(0);
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        new FADM_Object.MyAlarm("水校正完成", 1);
                    else
                        new FADM_Object.MyAlarm("Water correction completed", 1);

                    //Lib_SerialPort.Balance.METTLER.bReSetSign = true;
                }
            }
            catch (Exception ex)
            {
                //解锁6工位
                MyModbusFun.UnLockSix();

                if (Communal._b_isGetPowerClamp)
                {
                    int i_xStart = 0, i_yStart = 0;
                    //3.放夹子
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "放夹子启动");
                    //int i_xStart = 0, i_yStart = 0;
                    MyModbusFun.CalTarget(14, 0, ref i_xStart, ref i_yStart);
                    int i_mRes1 = MyModbusFun.PutClamp(i_xStart, i_yStart);
                    if (-2 == i_mRes1)
                        throw new Exception("收到退出消息");
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "放夹子完成");
                }
                // FADM_Object.Communal.WriteMachineStatus(8);
                //if (Lib_Card.Configure.Parameter.Other_Language == 0)
                //    FADM_Form.CustomMessageBox.Show(ex.Message, "水校正", MessageBoxButtons.OK, true);
                //else
                //{
                //    string str = ex.Message;
                //    if (SmartDyeing.FADM_Object.Communal._dic_warning.ContainsKey(ex.Message))
                //    {
                //        //如果存在就替换英文
                //        str = SmartDyeing.FADM_Object.Communal._dic_warning[ex.Message];
                //    }
                //    FADM_Form.CustomMessageBox.Show(str, "Water correction", MessageBoxButtons.OK, true);
                //}

                //else
                {
                    FADM_Object.Communal.WriteMachineStatus(8);
                    if (ex.Message.Equals("-2"))
                    {
                        int[] ia_errArray = new int[100];
                        MyModbusFun.GetErrMsgNew(ref ia_errArray);

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
                                     "水校正" + "','" +
                                      s_err + "(Test)');";
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                    string s_insert = CardObject.InsertD(s_err, Lib_Card.Configure.Parameter.Other_Language == 0 ? " 水校正": "Water correction");
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
                                    CardObject.DeleteD(lis_err[p]);
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
                                 "水校正" + "','" +
                                  ex.ToString() + "(Test)');";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                        new FADM_Object.MyAlarm(ex.ToString(), "水校正", false, 0);
                    }
                }

                //Lib_SerialPort.Balance.METTLER.bReSetSign = true;
            }
        }

        public void DripCheck()
        {
            try
            {
                FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "滴液水校正启动");

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
                    //Communal._b_isAllowDrip = false;
                }
                else
                {
                    Communal._b_isWaitDrip = false;
                    FADM_Object.Communal.WriteDripWait(false);
                }

                //MyModbusFun.Reset();
                //FADM_Object.Communal.WriteMachineStatus(5);
                //回零
                //if (!Lib_Card.ADT8940A1.Module.Home.Home.Home_XYZFinish)
                //{
                //    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "回零启动");
                //    int reSuccess = MyModbusFun.goHome();//回零 方法里异常直接抛出
                //    if (reSuccess == 0)
                //    {
                //        Lib_Card.ADT8940A1.Module.Home.Home.Home_XYZFinish = true;
                //        Console.WriteLine("回零成功");
                //    }

                //    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "回零完成");
                //}


                //Lib_SerialPort.Balance.METTLER.bReSetSign = true;
                Thread.Sleep(500);




                int i_mRes = -1;
                double d_blBalanceValue0 = 0;
            label4:
                //判断是否异常
                FADM_Object.Communal.BalanceState("水校正");

                //如果转盘水校正，先读初始值
                if (FADM_Object.Communal._b_isUseTurntableCorrectingWater)
                {
                    //记录初始天平读数
                    d_blBalanceValue0 = FADM_Object.Communal.SteBalance(); ;
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平读数：" + d_blBalanceValue0);
                }

                if (FADM_Object.Communal._b_isUseNewCorrectingWater)
                {
                    //Lib_SerialPort.Balance.METTLER.bZeroSign = true;
                    if (FADM_Object.Communal._b_isUseTurntableCorrectingWater)
                    {
                        //移动母液瓶
                        try
                        {
                            //执行母液瓶移动
                            Communal.MoveBottle(6, 5);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message == "-97")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "在转盘工位取瓶失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "放瓶时气缸下失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else
                                throw;
                        }

                        //移动到水校正位置
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找水校正位置");
                        i_mRes = MyModbusFun.TargetMove(21, 0, 1);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达水校正位置");

                        //

                    }
                    else
                    {
                        //移动到天平位
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找天平位");
                        i_mRes = MyModbusFun.TargetMove(2, 0, 1);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达天平位");
                    }

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
                    if (!FADM_Object.Communal._b_isUseTurntableCorrectingWater)
                    {
                        //记录初始天平读数
                        d_blBalanceValue0 = FADM_Object.Communal.SteBalance(); ;
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平读数：" + d_blBalanceValue0);
                    }

                    //伸出接液盘

                    //加水
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "加水启动(" + Lib_Card.Configure.Parameter.Correcting_Water_Time + "s)");

                    double d_addWaterTime = Lib_Card.Configure.Parameter.Correcting_Water_Time;

                    FADM_Object.Communal.AddWater(d_addWaterTime);

                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "加水完成");

                    if (FADM_Object.Communal._b_isUseTurntableCorrectingWater)
                    {
                        //移动母液瓶
                        try
                        {
                            //执行母液瓶移动
                            Communal.MoveBottle(6, 6);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message == "-97")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "在转盘工位取瓶失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "放瓶时气缸下失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else
                                throw;
                        }
                    }

                    //读取校正重量
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数启动");
                    double d_blRRead = FADM_Object.Communal.SteBalance();
                    double d_blWeightT = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blRRead - d_blBalanceValue0)) : Convert.ToDouble(string.Format("{0:F3}", d_blRRead - d_blBalanceValue0));
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数：" + d_blRRead + ",实际重量：" + d_blWeightT);
                    double d_blAdjust = Convert.ToDouble(string.Format("{0:F3}", d_blWeightT / Convert.ToDouble(Lib_Card.Configure.Parameter.Correcting_Water_Time)));
                    double d_blAdjust_Pulse = 0;
                    if (Communal._b_isUseLLJ)
                    {
                        //计算脉冲系数
                        MyModbusFun.ReadFlow();
                         d_blAdjust_Pulse = Convert.ToDouble(string.Format("{0:F3}", d_blWeightT / Communal.i_flowPulse));
                    }
                    if (d_blAdjust <= 0.0)
                    {
                        goto label4;
                    }

                    d_blBalanceValue0 = d_blRRead;

                    //验证
                    double d_blTime = 0;

                    d_blTime = Lib_Card.Configure.Parameter.Correcting_Water_RWeight / d_blAdjust;

                    if (FADM_Object.Communal._b_isUseTurntableCorrectingWater)
                    {
                        //移动母液瓶
                        try
                        {
                            //执行母液瓶移动
                            Communal.MoveBottle(6, 5);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message == "-97")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "在转盘工位取瓶失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "放瓶时气缸下失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else
                                throw;
                        }

                        //移动到水校正位置
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找水校正位置");
                        i_mRes = MyModbusFun.TargetMove(21, 0, 1);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达水校正位置");

                        //

                    }

                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "加水启动(" + string.Format("{0:F3}", d_blTime) + "s)");

                    FADM_Object.Communal.AddWater(d_blTime);

                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "加水完成");

                    if (FADM_Object.Communal._b_isUseTurntableCorrectingWater)
                    {
                        //移动母液瓶
                        try
                        {
                            //执行母液瓶移动
                            Communal.MoveBottle(6, 6);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message == "-97")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "在转盘工位取瓶失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "放瓶时气缸下失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else
                                throw;
                        }
                    }

                    //读取校正重量
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数启动");
                    d_blRRead = FADM_Object.Communal.SteBalance();
                    double d_blWeightC = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blRRead - d_blBalanceValue0)) : Convert.ToDouble(string.Format("{0:F3}", d_blRRead - d_blBalanceValue0));
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数：" + d_blRRead + ",实际重量：" + d_blWeightC);
                    int i_rErr = Convert.ToInt16((d_blWeightC - Lib_Card.Configure.Parameter.Correcting_Water_RWeight) / Lib_Card.Configure.Parameter.Correcting_Water_RWeight * 100);
                    i_rErr = i_rErr < 0 ? -i_rErr : i_rErr;
                    //解锁6工位
                    MyModbusFun.UnLockSix();
                    if (i_rErr > Lib_Card.Configure.Parameter.Other_AErr_Water)
                    {
                    lable5:
                        FADM_Object.Communal.WriteDripWait(true);
                        Communal._b_isAllowDrip = false;
                        FADM_Object.MyAlarm myAlarm;
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            myAlarm = new FADM_Object.MyAlarm("水校正失败，是否继续?(继续水校正请点是，退出水校正请点否)", "加水校正", true, 1);
                        else
                            myAlarm = new FADM_Object.MyAlarm("Water calibration failed, do you want to continue? (To continue with water calibration, please click Yes. To exit water calibration, please click No)", "Water correction", true, 1);
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
                            //Communal._b_isAllowDrip = false;
                        }
                        else
                        {
                            Communal._b_isWaitDrip = false;
                            FADM_Object.Communal.WriteDripWait(false);
                        }

                        if (1 == myAlarm._i_alarm_Choose)
                            goto label4;
                        FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "滴液水校正完成");
                    }
                    else
                    {
                        //Lib_Card.Configure.Parameter.Correcting_Water_FWeight = Convert.ToDouble(string.Format("{0:F3}", d_blWeightF));
                        Lib_Card.Configure.Parameter.Correcting_Water_Value = Convert.ToDouble(string.Format("{0:F3}", d_blAdjust));
                        //Lib_File.Ini.WriteIni("Correcting", "Correcting_Water_FWeight", string.Format("{0:F3}", d_blWeightF), Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Correcting", "Correcting_Water_Value", string.Format("{0:F3}", d_blAdjust), Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        if (Communal._b_isUseLLJ)
                        {
                            Lib_Card.Configure.Parameter.Correcting_FlowPulse_Value = Convert.ToDouble(string.Format("{0:F3}", d_blAdjust_Pulse));
                            Lib_File.Ini.WriteIni("Correcting", "Correcting_FlowPulse_Value", string.Format("{0:F3}", d_blAdjust_Pulse), Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        }
                        FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "滴液水校正完成");
                    }
                    if (Communal._b_isGetPowerClamp)
                    {
                        int i_xStart = 0, i_yStart = 0;
                        //3.放夹子
                        FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "放夹子启动");
                        //int i_xStart = 0, i_yStart = 0;
                        MyModbusFun.CalTarget(14, 0, ref i_xStart, ref i_yStart);
                        int i_mRes1 = MyModbusFun.PutClamp(i_xStart, i_yStart);
                        if (-2 == i_mRes1)
                            throw new Exception("收到退出消息");
                        FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "放夹子完成");
                    }

                    //FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找待机位");
                    //FADM_Object.Communal._i_OptCupNum = 0;
                    //FADM_Object.Communal._i_optBottleNum = 0;
                    //i_mRes = MyModbusFun.TargetMove(3, 0, 1);
                    //if (-2 == i_mRes)
                    //    throw new Exception("收到退出消息");
                    ////不回待机位，失能关闭
                    //MyModbusFun.Power(2);
                    //FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达待机位");

                    //报警动作待定
                    /*Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer buzzer = new Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer_Basic();
                    if (-1 == buzzer.Buzzer_On())
                        throw new Exception("驱动异常");
                    Thread.Sleep(Convert.ToInt32(Lib_Card.Configure.Parameter.Delay_Buzzer_Finish * 1000));
                    if (-1 == buzzer.Buzzer_Off())
                        throw new Exception("驱动异常");*/

                    //FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "滴液水校正完成");
                    //FADM_Object.Communal.WriteMachineStatus(0);
                    //if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    //    new FADM_Object.MyAlarm("水校正完成", 1);
                    //else
                    //    new FADM_Object.MyAlarm("Water correction completed", 1);

                    //Lib_SerialPort.Balance.METTLER.bReSetSign = true;
                }
                else
                {
                    //Lib_SerialPort.Balance.METTLER.bZeroSign = true;

                    if (FADM_Object.Communal._b_isUseTurntableCorrectingWater)
                    {
                        //移动母液瓶
                        try
                        {
                            //执行母液瓶移动
                            Communal.MoveBottle(6, 5);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message == "-97")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "在转盘工位取瓶失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "放瓶时气缸下失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else
                                throw;
                        }

                        //移动到水校正位置
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找水校正位置");
                        i_mRes = MyModbusFun.TargetMove(21, 0, 1);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达水校正位置");

                        //

                    }
                    else
                    {
                        //移动到天平位
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找天平位");
                        i_mRes = MyModbusFun.TargetMove(2, 0, 1);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达天平位");
                    }

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

                    if (!FADM_Object.Communal._b_isUseTurntableCorrectingWater)
                    {
                        //记录初始天平读数
                        d_blBalanceValue0 = FADM_Object.Communal.SteBalance(); ;
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平读数：" + d_blBalanceValue0);
                    }

                    //伸出接液盘

                    double d_addWaterTime2 = MyModbusFun.GetWaterTime(Lib_Card.Configure.Parameter.Correcting_Water_RWeight);//加水时间 校正加水时间
                    i_mRes = MyModbusFun.AddWater(1); //加水1秒
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");

                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "加水完成");

                    if (FADM_Object.Communal._b_isUseTurntableCorrectingWater)
                    {
                        //移动母液瓶
                        try
                        {
                            //执行母液瓶移动
                            Communal.MoveBottle(6, 6);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message == "-97")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "在转盘工位取瓶失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "放瓶时气缸下失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else
                                throw;
                        }
                    }

                    //读取首秒重量
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数启动");
                    double d_blRRead = FADM_Object.Communal.SteBalance();
                    double d_blWeightF = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blRRead - d_blBalanceValue0)) : Convert.ToDouble(string.Format("{0:F3}", d_blRRead - d_blBalanceValue0));
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数：" + d_blRRead + ",实际重量：" + d_blWeightF);
                    if (d_blWeightF <= 0.0)
                    {
                        goto label4;
                    }
                    if (FADM_Object.Communal._b_isUseTurntableCorrectingWater)
                    {
                        //移动母液瓶
                        try
                        {
                            //执行母液瓶移动
                            Communal.MoveBottle(6, 5);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message == "-97")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "在转盘工位取瓶失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "放瓶时气缸下失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else
                                throw;
                        }

                        //移动到水校正位置
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找水校正位置");
                        i_mRes = MyModbusFun.TargetMove(21, 0, 1);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达水校正位置");

                        //

                    }
                    //加水
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "加水启动(" + Lib_Card.Configure.Parameter.Correcting_Water_Time + "s)");

                    double d_addWaterTime = Lib_Card.Configure.Parameter.Correcting_Water_Time;
                    FADM_Object.Communal.AddWater(d_addWaterTime);
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "加水完成");

                    if (FADM_Object.Communal._b_isUseTurntableCorrectingWater)
                    {
                        //移动母液瓶
                        try
                        {
                            //执行母液瓶移动
                            Communal.MoveBottle(6, 6);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message == "-97")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "在转盘工位取瓶失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "放瓶时气缸下失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else
                                throw;
                        }
                    }

                    //读取校正重量
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数启动");
                    d_blRRead = FADM_Object.Communal.SteBalance();
                    double d_blWeightT = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blRRead - d_blBalanceValue0)) : Convert.ToDouble(string.Format("{0:F3}", d_blRRead - d_blBalanceValue0));
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数：" + d_blRRead + ",实际重量：" + d_blWeightT);
                    double d_blAdjust = Convert.ToDouble(string.Format("{0:F3}", d_blWeightT / Convert.ToDouble(Lib_Card.Configure.Parameter.Correcting_Water_Time)));
                    if (d_blAdjust <= 0.0)
                    {
                        goto label4;
                    }

                    d_blBalanceValue0 = d_blRRead;

                    

                    //验证
                    double d_blTime = 0;
                    if (0 == FADM_Object.Communal._i_waterWay)
                    {
                        if (Lib_Card.Configure.Parameter.Correcting_Water_RWeight > d_blWeightF)
                            d_blTime = (Lib_Card.Configure.Parameter.Correcting_Water_RWeight - d_blWeightF) / d_blAdjust + 1;
                        else
                        {
                            d_blTime = Lib_Card.Configure.Parameter.Correcting_Water_RWeight / d_blAdjust;
                            if (d_blTime < 0.01)
                                d_blTime = 0.01;
                        }
                    }
                    else
                    {
                        if (Lib_Card.Configure.Parameter.Correcting_Water_RWeight > 13)
                            d_blTime = (Lib_Card.Configure.Parameter.Correcting_Water_RWeight - d_blWeightF) / d_blAdjust + 1;
                        else
                            d_blTime = Lib_Card.Configure.Parameter.Correcting_Water_RWeight / d_blAdjust + Lib_Card.Configure.Parameter.Other_Coefficient_Water;
                    }
                    if (FADM_Object.Communal._b_isUseTurntableCorrectingWater)
                    {
                        //移动母液瓶
                        try
                        {
                            //执行母液瓶移动
                            Communal.MoveBottle(6, 5);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message == "-97")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "在转盘工位取瓶失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "放瓶时气缸下失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else
                                throw;
                        }

                        //移动到水校正位置
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找水校正位置");
                        i_mRes = MyModbusFun.TargetMove(21, 0, 1);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达水校正位置");

                        //

                    }
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "加水启动(" + string.Format("{0:F3}", d_blTime) + "s)");

                    FADM_Object.Communal.AddWater(d_blTime);

                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "加水完成");

                    if (FADM_Object.Communal._b_isUseTurntableCorrectingWater)
                    {
                        //移动母液瓶
                        try
                        {
                            //执行母液瓶移动
                            Communal.MoveBottle(6, 6);
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message == "-97")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "在转盘工位取瓶失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else if (ex.Message == "放瓶时气缸下失败")
                            {
                                //结束
                                //解锁6工位
                                MyModbusFun.UnLockSix();
                                return;
                            }
                            else
                                throw;
                        }
                    }

                    //读取校正重量
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数启动");
                    d_blRRead = FADM_Object.Communal.SteBalance();
                    double d_blWeightC = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blRRead - d_blBalanceValue0)) : Convert.ToDouble(string.Format("{0:F3}", d_blRRead - d_blBalanceValue0));
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数：" + d_blRRead + ",实际重量：" + d_blWeightC);
                    int i_rErr = Convert.ToInt16((d_blWeightC - Lib_Card.Configure.Parameter.Correcting_Water_RWeight) / Lib_Card.Configure.Parameter.Correcting_Water_RWeight * 100);
                    i_rErr = i_rErr < 0 ? -i_rErr : i_rErr;
                    //解锁6工位
                    MyModbusFun.UnLockSix();
                    if (i_rErr > Lib_Card.Configure.Parameter.Other_AErr_Water)
                    {
                    lable5:
                        FADM_Object.Communal.WriteDripWait(true);
                        Communal._b_isAllowDrip = false;
                        FADM_Object.MyAlarm myAlarm;
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            myAlarm = new FADM_Object.MyAlarm("水校正失败，是否继续?(继续水校正请点是，退出水校正请点否)", "加水校正", true, 1);
                        else
                            myAlarm = new FADM_Object.MyAlarm("Water calibration failed, do you want to continue? (To continue with water calibration, please click Yes. To exit water calibration, please click No)", "Water correction", true, 1);
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
                            //Communal._b_isAllowDrip = false;
                        }
                        else
                        {
                            Communal._b_isWaitDrip = false;
                            FADM_Object.Communal.WriteDripWait(false);
                        }

                        if (1 == myAlarm._i_alarm_Choose)
                            goto label4;
                        FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "滴液水校正完成");
                    }
                    else
                    {
                        Lib_Card.Configure.Parameter.Correcting_Water_FWeight = Convert.ToDouble(string.Format("{0:F3}", d_blWeightF));
                        Lib_Card.Configure.Parameter.Correcting_Water_Value = Convert.ToDouble(string.Format("{0:F3}", d_blAdjust));
                        Lib_File.Ini.WriteIni("Correcting", "Correcting_Water_FWeight", string.Format("{0:F3}", d_blWeightF), Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Correcting", "Correcting_Water_Value", string.Format("{0:F3}", d_blAdjust), Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "滴液水校正完成");
                    }

                    if (Communal._b_isGetPowerClamp)
                    {
                        int i_xStart = 0, i_yStart = 0;
                        //3.放夹子
                        FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "放夹子启动");
                        //int i_xStart = 0, i_yStart = 0;
                        MyModbusFun.CalTarget(14, 0, ref i_xStart, ref i_yStart);
                        int i_mRes1 = MyModbusFun.PutClamp(i_xStart, i_yStart);
                        if (-2 == i_mRes1)
                            throw new Exception("收到退出消息");
                        FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "放夹子完成");
                    }

                    //FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找待机位");
                    //FADM_Object.Communal._i_OptCupNum = 0;
                    //FADM_Object.Communal._i_optBottleNum = 0;
                    //i_mRes = MyModbusFun.TargetMove(3, 0, 1);
                    //if (-2 == i_mRes)
                    //    throw new Exception("收到退出消息");
                    ////不回待机位，失能关闭
                    //MyModbusFun.Power(2);
                    //FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达待机位");

                    //报警动作待定
                    /*Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer buzzer = new Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer_Basic();
                    if (-1 == buzzer.Buzzer_On())
                        throw new Exception("驱动异常");
                    Thread.Sleep(Convert.ToInt32(Lib_Card.Configure.Parameter.Delay_Buzzer_Finish * 1000));
                    if (-1 == buzzer.Buzzer_Off())
                        throw new Exception("驱动异常");*/

                    //FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "滴液水校正完成");
                    //FADM_Object.Communal.WriteMachineStatus(0);
                    //if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    //    new FADM_Object.MyAlarm("水校正完成", 1);
                    //else
                    //    new FADM_Object.MyAlarm("Water correction completed", 1);

                    //Lib_SerialPort.Balance.METTLER.bReSetSign = true;
                }
            }
            catch (Exception ex)
            {
                //解锁6工位
                MyModbusFun.UnLockSix();

                if (Communal._b_isGetPowerClamp)
                {
                    int i_xStart = 0, i_yStart = 0;
                    //3.放夹子
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "放夹子启动");
                    //int i_xStart = 0, i_yStart = 0;
                    MyModbusFun.CalTarget(14, 0, ref i_xStart, ref i_yStart);
                    int i_mRes1 = MyModbusFun.PutClamp(i_xStart, i_yStart);
                    if (-2 == i_mRes1)
                        throw new Exception("收到退出消息");
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "放夹子完成");
                }
                // FADM_Object.Communal.WriteMachineStatus(8);
                //if (Lib_Card.Configure.Parameter.Other_Language == 0)
                //    FADM_Form.CustomMessageBox.Show(ex.Message, "水校正", MessageBoxButtons.OK, true);
                //else
                //{
                //    string str = ex.Message;
                //    if (SmartDyeing.FADM_Object.Communal._dic_warning.ContainsKey(ex.Message))
                //    {
                //        //如果存在就替换英文
                //        str = SmartDyeing.FADM_Object.Communal._dic_warning[ex.Message];
                //    }
                //    FADM_Form.CustomMessageBox.Show(str, "Water correction", MessageBoxButtons.OK, true);
                //}

                //else
                {
                    FADM_Object.Communal.WriteMachineStatus(8);
                    if (ex.Message.Equals("-2"))
                    {
                        int[] ia_errArray = new int[100];
                        MyModbusFun.GetErrMsgNew(ref ia_errArray);

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
                                     "水校正" + "','" +
                                      s_err + "(Test)');";
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                    string s_insert = CardObject.InsertD(s_err, Lib_Card.Configure.Parameter.Other_Language == 0 ? " 水校正" : "Water correction");
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
                                    CardObject.DeleteD(lis_err[p]);
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
                                 "水校正" + "','" +
                                  ex.ToString() + "(Test)');";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                        new FADM_Object.MyAlarm(ex.ToString(), "水校正", false, 0);
                    }
                }

                //Lib_SerialPort.Balance.METTLER.bReSetSign = true;
            }
        }


        public void Recheck(double d_blWeight)
        {
            try
            {
                FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "水验证启动");

                MyModbusFun.Reset();
                FADM_Object.Communal.WriteMachineStatus(12);
                //FADM_Auto.Reset.IOReset();
                //回零
                //if (!Lib_Card.ADT8940A1.Module.Home.Home.Home_XYZFinish)
                //{
                //    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "回零启动");
                //    //Lib_Card.ADT8940A1.Module.Home.Home home = new Lib_Card.ADT8940A1.Module.Home.Home_Condition();
                //    //if (-1 == home.Home_XYZ(FADM_Object.Communal._i_cylinderVersion))
                //    //    throw new Exception("驱动异常");
                //    //Lib_Card.ADT8940A1.Module.Home.Home.Home_XYZFinish = true;

                //    int reSuccess = MyModbusFun.goHome();//回零 方法里异常直接抛出
                //    if (reSuccess == 0)
                //    {
                //        Lib_Card.ADT8940A1.Module.Home.Home.Home_XYZFinish = true;
                //        Console.WriteLine("回零成功");
                //    }
                //    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "回零完成");
                //}
                
                //Lib_SerialPort.Balance.METTLER.bReSetSign = true;
                Thread.Sleep(500);


                //判断是否异常
                FADM_Object.Communal.BalanceState("水检查");

                double d_blBalanceValue0 = 0;
                int i_mRes = -1;

                //如果转盘水校正，先读初始值
                if (FADM_Object.Communal._b_isUseTurntableCorrectingWater)
                {
                    //记录初始天平读数
                    d_blBalanceValue0 = FADM_Object.Communal.SteBalance(); ;
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平读数：" + d_blBalanceValue0);
                }

                //Lib_SerialPort.Balance.METTLER.bZeroSign = true;

                if (FADM_Object.Communal._b_isUseTurntableCorrectingWater)
                {
                    //移动母液瓶
                    try
                    {
                        //执行母液瓶移动
                        Communal.MoveBottle(6, 5);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message == "-97")
                        {
                            //结束
                            //解锁6工位
                            MyModbusFun.UnLockSix();
                            return;
                        }
                        else if (ex.Message == "在转盘工位取瓶失败")
                        {
                            //结束
                            //解锁6工位
                            MyModbusFun.UnLockSix();
                            return;
                        }
                        else if (ex.Message == "放瓶时气缸下失败")
                        {
                            //结束
                            //解锁6工位
                            MyModbusFun.UnLockSix();
                            return;
                        }
                        else
                            throw;
                    }

                    //移动到水校正位置
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找水校正位置");
                    i_mRes = MyModbusFun.TargetMove(21, 0, 1);
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达水校正位置");

                    //

                }
                else
                {
                    //移动到天平位
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找天平位");
                    i_mRes = MyModbusFun.TargetMove(2, 0, 1);
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达天平位");
                }

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

                if (!FADM_Object.Communal._b_isUseTurntableCorrectingWater)
                {
                    //记录初始天平读数
                    d_blBalanceValue0 = FADM_Object.Communal.SteBalance();
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平读数：" + d_blBalanceValue0);
                }

                ////加水
                //Add(d_blWeight, "RobotHand");

                double d_addWaterTime = MyModbusFun.GetWaterTime(d_blWeight);//加水时间
                FADM_Object.Communal.AddWater(d_addWaterTime);

                if (FADM_Object.Communal._b_isUseTurntableCorrectingWater)
                {
                    //移动母液瓶
                    try
                    {
                        //执行母液瓶移动
                        Communal.MoveBottle(6, 6);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message == "-97")
                        {
                            //结束
                            //解锁6工位
                            MyModbusFun.UnLockSix();
                            return;
                        }
                        else if (ex.Message == "在转盘工位取瓶失败")
                        {
                            //结束
                            //解锁6工位
                            MyModbusFun.UnLockSix();
                            return;
                        }
                        else if (ex.Message == "放瓶时气缸下失败")
                        {
                            //结束
                            //解锁6工位
                            MyModbusFun.UnLockSix();
                            return;
                        }
                        else
                            throw;
                    }
                }

                //天平读数
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数启动");
                double d_blRRead = FADM_Object.Communal.SteBalance();
                double d_blRWeight = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blRRead - d_blBalanceValue0)) : Convert.ToDouble(string.Format("{0:F3}", d_blRRead - d_blBalanceValue0));
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数：" + d_blRRead + ",实际重量：" + d_blRWeight);
                //解锁6工位
                MyModbusFun.UnLockSix();

                //Lib_Card.ADT8940A1.OutPut.Tray.Tray tray = new Lib_Card.ADT8940A1.OutPut.Tray.Tray_Condition();

                //if (-1 == tray.Tray_Off())
                //    throw new Exception("驱动异常");

                if (Communal._b_isUseLLJ)
                {
                    MyModbusFun.ReadFlow();
                    //计算理论重量
                    //理论重量 = 脉冲*系数 - 4.6 + power(脉冲，1/4）
                    double d_ = Communal.i_flowPulse * Lib_Card.Configure.Parameter.Correcting_FlowPulse_Value - 4.6 + Math.Pow(Communal.i_flowPulse, 0.25);
                    double d_need = Lib_Card.Configure.Parameter.Correcting_Water_Value * d_addWaterTime;
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "理论重量：" + string.Format("{0:F3}", d_) + "需加量：" + string.Format("{0:F3}", d_need) + "脉冲值：" + Communal.i_flowPulse);

                    string s_sql = "INSERT INTO LLJ" +
                                     "(Weight,LLWeight,Pulse,Time,LLC,AddC)" +
                                     " VALUES( '" +
                                     string.Format("{0:F3}", d_blRWeight) + "','" +
                                     string.Format("{0:F3}", d_) + "','" +
                                     Communal.i_flowPulse.ToString() + "','" +
                                      DateTime.Now.ToString() + "','" +
                                      string.Format("{0:F3}", Lib_Card.Configure.Parameter.Correcting_FlowPulse_Value) + "','" +
                                      string.Format("{0:F3}", Lib_Card.Configure.Parameter.Correcting_Water_Value) + "');";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);


                }

                if (Communal._b_isGetPowerClamp)
                {
                    int i_xStart=0, i_yStart=0;
                    //3.放夹子
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "放夹子启动");
                    //int i_xStart = 0, i_yStart = 0;
                    MyModbusFun.CalTarget(14, 0, ref i_xStart, ref i_yStart);
                    int i_mRes1 = MyModbusFun.PutClamp(i_xStart, i_yStart);
                    if (-2 == i_mRes1)
                        throw new Exception("收到退出消息");
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "放夹子完成");
                }

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找待机位");
                FADM_Object.Communal._i_OptCupNum = 0;
                FADM_Object.Communal._i_optBottleNum = 0;
                //move = new Lib_Card.ADT8940A1.Module.Move.Move_Standby();
                //i_mRes = move.TargetMove(FADM_Object.Communal._i_cylinderVersion, 0);
                //if (-1 == i_mRes)
                //    throw new Exception("驱动异常");
                //else if (-2 == i_mRes)
                //    throw new Exception("收到退出消息");

                i_mRes = MyModbusFun.TargetMove(3, 0, 1);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");

                //不回待机位，失能关闭
                MyModbusFun.Power(2);

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达待机位");

                //Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer buzzer = new Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer_Basic();
                //if (-1 == buzzer.Buzzer_On())
                //    throw new Exception("驱动异常");
                Thread.Sleep(Convert.ToInt32(Lib_Card.Configure.Parameter.Delay_Buzzer_Finish * 1000));
                //if (-1 == buzzer.Buzzer_Off())
                //    throw new Exception("驱动异常");

                FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "水验证完成");
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    new FADM_Object.MyAlarm("水验证完成,实际加水量：" + d_blRWeight + "g", 1);
                else

                    new FADM_Object.MyAlarm("Water verification completed, actual water added：" + d_blRWeight + "g", 1);

                FADM_Object.Communal.WriteMachineStatus(0);

                //Lib_SerialPort.Balance.METTLER.bReSetSign = true;
            }
            catch (Exception ex)
            {
                //解锁6工位
                MyModbusFun.UnLockSix();

                if (Communal._b_isGetPowerClamp)
                {
                    int i_xStart = 0, i_yStart = 0;
                    //3.放夹子
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "放夹子启动");
                    //int i_xStart = 0, i_yStart = 0;
                    MyModbusFun.CalTarget(14, 0, ref i_xStart, ref i_yStart);
                    int i_mRes1 = MyModbusFun.PutClamp(i_xStart, i_yStart);
                    if (-2 == i_mRes1)
                        throw new Exception("收到退出消息");
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "放夹子完成");
                }
                // FADM_Object.Communal.WriteMachineStatus(8);
                //if (Lib_Card.Configure.Parameter.Other_Language == 0)
                //    FADM_Form.CustomMessageBox.Show(ex.Message, "水校正", MessageBoxButtons.OK, true);
                //else
                //{
                //    string str = ex.Message;
                //    if (SmartDyeing.FADM_Object.Communal._dic_warning.ContainsKey(ex.Message))
                //    {
                //        //如果存在就替换英文
                //        str = SmartDyeing.FADM_Object.Communal._dic_warning[ex.Message];
                //    }
                //    FADM_Form.CustomMessageBox.Show(str, "Water correction", MessageBoxButtons.OK, true);
                //}

                {
                    FADM_Object.Communal.WriteMachineStatus(8);
                    if (ex.Message.Equals("-2"))
                    {
                        int[] ia_errArray = new int[100];
                        MyModbusFun.GetErrMsgNew(ref ia_errArray);

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
                                     "水验证" + "','" +
                                      s_err + "(Test)');";
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                    string s_insert = CardObject.InsertD(s_err, Lib_Card.Configure.Parameter.Other_Language == 0 ? " 水验证": "Water verification");
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
                                    CardObject.DeleteD(lis_err[p]);
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
                                 "水验证" + "','" +
                                  ex.ToString() + "(Test)');";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                        string str = ex.Message;
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        {
                            if (SmartDyeing.FADM_Object.Communal._dic_warning.ContainsKey(ex.Message))
                            {
                                //如果存在就替换英文
                                str = SmartDyeing.FADM_Object.Communal._dic_warning[ex.Message];
                            }
                        }

                        new FADM_Object.MyAlarm(str, "水验证", false, 0);
                    }
                }
                //Lib_SerialPort.Balance.METTLER.bReSetSign = true;
            }

        }

        public void Add(double dblTime, string sName)
        {

            //伸出接液盘
            Lib_Card.ADT8940A1.OutPut.Tray.Tray tray = new Lib_Card.ADT8940A1.OutPut.Tray.Tray_Condition();
            if (-1 == tray.Tray_On())
                throw new Exception("驱动异常");

            //计算加水时间
            //double dblTime = 0;
            //if (0 == FADM_Object.Communal.WaterWay)
            //{
            //    if (Lib_Card.Configure.Parameter.Correcting_Water_RWeight > Lib_Card.Configure.Parameter.Correcting_Water_FWeight)
            //        dblTime = (Lib_Card.Configure.Parameter.Correcting_Water_RWeight - Lib_Card.Configure.Parameter.Correcting_Water_FWeight) / Lib_Card.Configure.Parameter.Correcting_Water_Value + 1;
            //    else
            //    {
            //        dblTime = Lib_Card.Configure.Parameter.Correcting_Water_RWeight / Lib_Card.Configure.Parameter.Correcting_Water_Value;
            //        if (dblTime < 0.01)
            //            dblTime = 0.01;
            //    }
            //}
            //else
            //{
            //    if (dblWeight > 13)
            //        dblTime = (dblWeight - Lib_Card.Configure.Parameter.Correcting_Water_FWeight) / Lib_Card.Configure.Parameter.Correcting_Water_Value + 1;
            //    else
            //        dblTime = dblWeight / Lib_Card.Configure.Parameter.Correcting_Water_Value + Lib_Card.Configure.Parameter.Other_Coefficient_Water;
            //}
            FADM_Object.Communal._fadmSqlserver.InsertRun(sName, "加水启动(" + string.Format("{0:F2}", dblTime) + "s)");
            Lib_Card.ADT8940A1.OutPut.Water.Water water = new Lib_Card.ADT8940A1.OutPut.Water.Water_Condition();
            if (-1 == water.Water_On())
                throw new Exception("驱动异常");

            if (dblTime > 0)
                Thread.Sleep(Convert.ToInt32(dblTime * 1000));

            if (-1 == water.Water_Off())
                throw new Exception("驱动异常");


            FADM_Object.Communal._fadmSqlserver.InsertRun(sName, "加水完成");
        }


    }
}
