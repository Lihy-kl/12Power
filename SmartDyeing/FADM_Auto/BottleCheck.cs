using SmartDyeing.FADM_Object;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using SmartDyeing.FADM_Control;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using System.Runtime.Remoting.Lifetime;
using Lib_Card;
using Lib_Card.ADT8940A1.Module.Home;

namespace SmartDyeing.FADM_Auto
{
    /// <summary>
    /// 针检
    /// </summary>
    internal class BottleCheck
    {
        /// <summary>
        /// 针检
        /// </summary>
        public void Check()
        {
            try
            {

                FADM_Object.Communal.WriteDripWait(false);
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "针检启动");
                FADM_Object.Communal.WriteMachineStatus(6);
                MyModbusFun.Reset();

                MyModbusFun.SetBatchStart();
                //FADM_Auto.Reset.IOReset();  //这里要待定 看下怎么改
                //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                //{
                //    Lib_Card.ADT8940A1.Module.Home.Home.Home_XYZFinish = false;
                //}
                /*if (!Lib_Card.ADT8940A1.Module.Home.Home.Home_XYZFinish)
                {
                    //回零
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "回零启动");
                    int reSuccess = MyModbusFun.goHome();//回零 方法里异常直接抛出
                    if (reSuccess == 0)
                    {
                        Lib_Card.ADT8940A1.Module.Home.Home.Home_XYZFinish = true;
                        Console.WriteLine("回零成功");
                    }
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "回零完成");
                }*/


                //Lib_SerialPort.Balance.METTLER.bReSetSign = true;

                //针检
                DataTable dt_bottle_check = FADM_Object.Communal._fadmSqlserver.GetData(
                    "SELECT BottleNum FROM bottle_check ORDER BY BottleNum;");

                List<int> lis_ints = new List<int>();
                string s_bottleNo = "";
                foreach (DataRow dr in dt_bottle_check.Rows)
                {
                    int i_bottleNo = Convert.ToInt16(dr["BottleNum"]);

                    DataTable dataTable = FADM_Object.Communal._fadmSqlserver.GetData(
                        "SELECT CurrentWeight FROM bottle_details WHERE BottleNum = '" +
                        i_bottleNo + "';");

                    double d_currentWeight = Convert.ToDouble(dataTable.Rows[0][0]);

                    if (d_currentWeight >= Lib_Card.Configure.Parameter.Other_Bottle_MinWeight)
                    {
                        ////判断染色线程是否需要用机械手
                        //if (null != FADM_Object.Communal.ReadDyeThread())
                        //{
                        //    FADM_Object.Communal.WriteDripWait(true);

                        //    while (true)
                        //    {
                        //        if (false == FADM_Object.Communal.ReadDripWait())
                        //            break;
                        //        Thread.Sleep(1);
                        //    }
                        //}
                        int i_res = CheckProcess(i_bottleNo, false, 0);
                        if (0 == i_res)
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE bottle_check SET Successed = 1, Finish = 1 WHERE BottleNum = '" + i_bottleNo + "';");
                        }
                        else
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE bottle_check SET Successed = 0, Finish = 1 WHERE BottleNum = '" + i_bottleNo + "';");
                        }
                    }
                    else
                    {
                        lis_ints.Add(i_bottleNo);
                        s_bottleNo += i_bottleNo.ToString() + ";";
                    }

                }

                if (lis_ints.Count == 0)
                    goto label3;

                s_bottleNo = s_bottleNo.Remove(s_bottleNo.Length - 1);
                FADM_Object.MyAlarm myAlarm;
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    myAlarm = new FADM_Object.MyAlarm(s_bottleNo + "号母液瓶液量过低，是否继续针检?(继续针检请点是，退出针检请点否)", "普通针检", true, 1);
                else
                    myAlarm = new FADM_Object.MyAlarm("The liquid level in bottle " + s_bottleNo + " is too low. Do you want to continue the needle inspection? " +
                        "(To continue the needle examination, please click Yes, and to exit the needle examination, please click No)", "Regular needle examination", true, 1);


                if (2 == myAlarm._i_alarm_Choose)
                    goto label1;
                label2:
                foreach (int i in lis_ints)
                {
                    ////判断染色线程是否需要用机械手
                    //if (null != FADM_Object.Communal.ReadDyeThread())
                    //{
                    //    FADM_Object.Communal.WriteDripWait(true);

                    //    while (true)
                    //    {
                    //        if (false == FADM_Object.Communal.ReadDripWait())
                    //            break;
                    //        Thread.Sleep(1);
                    //    }
                    //}
                    int iRes = CheckProcess(i, false, 0);
                    if (0 == iRes)
                    {
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE bottle_check SET Successed = 1, Finish = 1 WHERE BottleNum = '" + i + "';");
                    }
                    else
                    {
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE bottle_check SET Successed = 0, Finish = 1 WHERE BottleNum = '" + i + "';");
                    }
                }
            label3:
                dt_bottle_check = FADM_Object.Communal._fadmSqlserver.GetData(
                    "SELECT BottleNum FROM bottle_check WHERE Successed = 0 ORDER BY BottleNum;");
                if (0 == dt_bottle_check.Rows.Count)
                    goto label1;


                s_bottleNo = "";
                lis_ints = new List<int>();

                foreach (DataRow dr in dt_bottle_check.Rows)
                {
                    int iBottleNo = Convert.ToInt16(dr["BottleNum"]);

                    lis_ints.Add(iBottleNo);
                    s_bottleNo += iBottleNo.ToString() + ";";
                }

                s_bottleNo = s_bottleNo.Remove(s_bottleNo.Length - 1);
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    myAlarm = new FADM_Object.MyAlarm(s_bottleNo + "号母液瓶针检失败，是否继续?(继续针检请点是，退出针检请点否)", "普通针检", true, 1);
                else
                    myAlarm = new FADM_Object.MyAlarm(s_bottleNo + " bottle needle inspection failed, do you want to continue? " +
                        "(To continue the needle examination, please click Yes, and to exit the needle examination, please click No)", "Regular needle examination", true, 1);
                while (true)
                {
                    if (0 != myAlarm._i_alarm_Choose)
                        break;
                    Thread.Sleep(1);
                }

                if (2 == myAlarm._i_alarm_Choose)
                    goto label1;


                goto label2;

            label1:
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找待机位");
                //int i_mRes = MyModbusFun.TargetMove(3,0,1);
                //if (-2 == i_mRes)
                //    throw new Exception("收到退出消息");
                //不回待机位，失能关闭
                MyModbusFun.Power(2);
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达待机位");
                //MyModbusFun.buzzer_On();
                //Thread.Sleep(Convert.ToInt32(Lib_Card.Configure.Parameter.Delay_Buzzer_Finish * 1000));
                //MyModbusFun.buzzer_OFF();

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "针检完成");
                FADM_Object.Communal.WriteMachineStatus(0);
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    new FADM_Object.MyAlarm("针检完成", 1);
                else
                    new FADM_Object.MyAlarm("Needle examination completed", 1);
            }
            catch (Exception ex)
            {
                string[] strArray = { "", "" };
                if ("收到退出消息" == ex.Message)
                {
                    //解锁6工位
                    MyModbusFun.UnLockSix();
                    FADM_Object.Communal._b_stop = false;
                    MyModbusFun.MyMachineReset(); //里面会改状态WriteMachineStatus(0)
                    return;

                }
                else 
                {
                    //解锁6工位
                    MyModbusFun.UnLockSix();
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
                                    string strErr = SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew[ia_errArray[i]];
                                    string Str = "INSERT INTO alarm_table" +
                                     "(MyDate,MyTime,AlarmHead,AlarmDetails)" +
                                     " VALUES( '" +
                                     String.Format("{0:d}", DateTime.Now) + "','" +
                                     String.Format("{0:T}", DateTime.Now) + "','" +
                                     "针检" + "','" +
                                      strErr + "(Test)');";
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(Str);

                                    string s = CardObject.InsertD(strErr, Lib_Card.Configure.Parameter.Other_Language == 0?" 针检":"Check");
                                    if (!lis_err.Contains(s))
                                        lis_err.Add(s);
                                    //while (true)
                                    //{
                                    //    Thread.Sleep(1);
                                    //    if (Lib_Card.CardObject.keyValuePairs[s].Choose != 0)
                                    //        break;

                                    //}

                                    //int _i_alarm_Choose = Lib_Card.CardObject.keyValuePairs[s].Choose;
                                    //CardObject.DeleteD(s);

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
                        string Str = "INSERT INTO alarm_table" +
                                 "(MyDate,MyTime,AlarmHead,AlarmDetails)" +
                                 " VALUES( '" +
                                 String.Format("{0:d}", DateTime.Now) + "','" +
                                 String.Format("{0:T}", DateTime.Now) + "','" +
                                 "针检" + "','" +
                                  ex.ToString() + "(Test)');";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(Str);
                        string str = ex.Message;
                        if (Lib_Card.Configure.Parameter.Other_Language == 1)
                        {
                            if (SmartDyeing.FADM_Object.Communal._dic_warning.ContainsKey(ex.Message))
                            {
                                //如果存在就替换英文
                                str = SmartDyeing.FADM_Object.Communal._dic_warning[ex.Message];
                            }
                        }
                        new FADM_Object.MyAlarm(str, "Check", false, 0);
                    }
                }
               
                //FADM_Object.Communal.WriteMachineStatus(8);
                //if (Lib_Card.Configure.Parameter.Other_Language == 0)
                //    FADM_Form.CustomMessageBox.Show(ex.Message == "-2" ? strArray[1] : ex.ToString(), "普通针检", MessageBoxButtons.OK, true);
                //else
                //{
                //    string str = ex.Message == "-2" ? strArray[1] : ex.ToString();
                //    if (SmartDyeing.FADM_Object.Communal._dic_warning.ContainsKey(ex.Message))
                //    {
                //        //如果存在就替换英文
                //        str = SmartDyeing.FADM_Object.Communal._dic_warning[ex.Message];
                //    }
                //    FADM_Form.CustomMessageBox.Show(str, "Regular needle examination", MessageBoxButtons.OK, true);
                //}


            }
        }

        /// <summary>
        /// ABS针检流程
        /// </summary>
        /// <param name="i_bottleNo">瓶号</param>
        /// <returns>0：成功；-1：失败</returns>
        /// <exception cref="Exception"></exception>
        private int MyABSDripCheckProcess(int i_bottleNo, bool b_drip, int i_lowSrart)
        {
            int i_xStart = 0, i_yStart = 0;
            int i_mRes = -1;
            if (Communal._b_isGetWetClamp)
            {
                //3.放夹子
                FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "放夹子启动");
                //int i_xStart = 0, i_yStart = 0;
                //计算湿布布夹子位置
                MyModbusFun.CalTarget(9, 0, ref i_xStart, ref i_yStart);
                int i_mRes2 = MyModbusFun.PutClamp(i_xStart, i_yStart);
                if (-2 == i_mRes2)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "放夹子完成");
            }

            if (Communal._b_isGetDryClamp)
            {
                //3.放夹子
                FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "放夹子启动");
                //int i_xStart = 0, i_yStart = 0;
                //计算干布夹子位置
                MyModbusFun.CalTarget(8, 0, ref i_xStart, ref i_yStart);
                int iMRes1 = MyModbusFun.PutClamp(i_xStart, i_yStart);
                if (-2 == iMRes1)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "放夹子完成");
            }
            if (Communal._b_isGetPowerClamp)
            {
                //3.放夹子
                FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "放夹子启动");
                //int i_xStart = 0, i_yStart = 0;
                MyModbusFun.CalTarget(14, 0, ref i_xStart, ref i_yStart);
                int i_mRes1 = MyModbusFun.PutClamp(i_xStart, i_yStart);
                if (-2 == i_mRes1)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "放夹子完成");
            }

            if (Communal._b_isGetPHClamp)
            {
                //3.放夹子
                FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "放夹子启动");
                //int i_xStart = 0, i_yStart = 0;
                MyModbusFun.CalTarget(17, 0, ref i_xStart, ref i_yStart);
                int i_mRes1 = MyModbusFun.PutClamp(i_xStart, i_yStart);
                if (-2 == i_mRes1)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "放夹子完成");
            }

            if (!Communal._b_isGetSyringes)
            {
                FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "拿针筒启动");
                //计算针筒位置
                MyModbusFun.CalTarget(11, 0, ref i_xStart, ref i_yStart);
                int i_mRes3 = MyModbusFun.GetSyringes(i_xStart, i_yStart);
                if (-2 == i_mRes3)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "拿针筒完成");
            }

            int i_res = 0;
        label11:


            ////判断有异常 松开失能 推送了
            //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
            //{
            //    //富士伺服在下面判断 天平状态 原有不动 绿维的放在上面 并且置位 是否回原点 绿维的放移动机械手前面

            //    //判断是否异常
            //    FADM_Object.Communal.BalanceState("针检");
            //}

            //移动到母液瓶
            if (i_bottleNo == 888)
            {
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找1号(吸光度杯)");
                FADM_Object.Communal._i_optBottleNum = i_bottleNo;
                i_mRes = MyModbusFun.TargetMove(10, 1, 0);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达1号(吸光度杯)");
            }
            else
            {
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找3号(吸光度杯)");
                FADM_Object.Communal._i_optBottleNum = i_bottleNo;
                i_mRes = MyModbusFun.TargetMove(10, 3, 0);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达3号(吸光度杯)");
            }

            //抽液
            DataTable dt_bottle_details = FADM_Object.Communal._fadmSqlserver.GetData(
                "SELECT SyringeType FROM bottle_details WHERE BottleNum = '" + i_bottleNo + "';");



            if ("小针筒" == Convert.ToString(dt_bottle_details.Rows[0][0]) || "Little Syringe" == Convert.ToString(dt_bottle_details.Rows[0][0]))
            {
                int i_pulse = Lib_Card.Configure.Parameter.Correcting_S_Pulse + 10000;
                //label2:
                try
                {
                    //多抽5000脉冲用于到天平滴掉
                    int i_totalPulse = i_pulse - Lib_Card.Configure.Parameter.Other_Z_BackPulse + 5000;
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抽液启动(" + i_totalPulse + ")");

                    //1号吸光度杯停止搅拌
                    int[] values = new int[1];
                    values[0] = 1;

                    if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                    {
                        FADM_Object.Communal._tcpModBusAbs.ReConnect();
                    }
                    if (i_bottleNo == 888)
                        FADM_Object.Communal._tcpModBusAbs.Write(13094, values);
                    else
                        FADM_Object.Communal._tcpModBusAbs.Write(13114, values);

                    Thread.Sleep(2000);

                    i_mRes = MyModbusFun.AbsExtract(i_totalPulse, true, 0); //抽液 排空

                    values[0] = 0;
                    if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                    {
                        FADM_Object.Communal._tcpModBusAbs.ReConnect();
                    }
                    if (i_bottleNo == 888)
                        FADM_Object.Communal._tcpModBusAbs.Write(13094, values);
                    else
                        FADM_Object.Communal._tcpModBusAbs.Write(13114, values);

                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                }
                catch (Exception ex)
                {
                    if ("未发现针筒" == ex.Message)
                    {
                        //滴液时针检处理
                        if (i_lowSrart == 2)
                        {
                            FADM_Object.MyAlarm myAlarm;
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm(i_bottleNo + "号母液瓶未找到针筒，是否继续执行?(继续寻找请点是，退出针检请点否)", "普通针检", true, 1);
                            else
                                myAlarm = new FADM_Object.MyAlarm("The syringe for bottle " + i_bottleNo + " was not found.Do you want to continue? " +
                                    "(To continue searching, please click Yes.To exit the needle test, please click No)", "Regular needle examination", true, 1);
                            while (true)
                            {
                                if (0 != myAlarm._i_alarm_Choose)
                                    break;
                                Thread.Sleep(1);
                            }

                            if (1 == myAlarm._i_alarm_Choose)
                                goto label11;
                            else
                                return -3;
                        }
                        //后处理时针检处理
                        else if (false == b_drip)
                        {
                            return -3;
                        }
                        else
                        {
                            return -2;
                        }
                    }
                    else
                        throw;

                }
            }
            else
            {
                int i_pulse = Lib_Card.Configure.Parameter.Correcting_B_Pulse + 10000;
                //FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抽液启动(" + i_pulse + ")");
                //label3:
                try
                {
                    //多抽5000脉冲用于到天平滴掉
                    int i_totalPulse = i_pulse - Lib_Card.Configure.Parameter.Other_Z_BackPulse + 5000;
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抽液启动(" + i_totalPulse + ")");
                    //1号吸光度杯停止搅拌
                    int[] values = new int[1];
                    values[0] = 1;
                    if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                    {
                        FADM_Object.Communal._tcpModBusAbs.ReConnect();
                    }

                    FADM_Object.Communal._tcpModBusAbs.Write(13094, values);

                    i_mRes = MyModbusFun.AbsExtract(i_totalPulse, true, 1);

                    values[0] = 0;
                    if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                    {
                        FADM_Object.Communal._tcpModBusAbs.ReConnect();
                    }

                    FADM_Object.Communal._tcpModBusAbs.Write(13094, values);

                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                }
                catch (Exception ex)
                {

                    if ("未发现针筒" == ex.Message)
                    {
                        if (false == b_drip || i_lowSrart == 2)
                        {
                            FADM_Object.MyAlarm myAlarm;
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm(i_bottleNo + "号母液瓶未找到针筒，是否继续执行?(继续寻找请点是，退出针检请点否)", "普通针检", true, 1);
                            else
                                myAlarm = new FADM_Object.MyAlarm("The syringe for bottle " + i_bottleNo + " was not found.Do you want to continue? " +
                                    "(To continue searching, please click Yes.To exit the needle test, please click No)", "Regular needle examination", true, 1);

                            while (true)
                            {
                                if (0 != myAlarm._i_alarm_Choose)
                                    break;
                                Thread.Sleep(1);
                            }

                            if (1 == myAlarm._i_alarm_Choose)
                                goto label11;
                            else
                                //throw new Exception("收到退出消息");
                                return -3;
                        }
                        else
                        {
                            return -2;
                        }

                    }
                    else
                        throw;


                }
            }




            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抽液完成");

            //移动到天平位

            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找天平位");

            //判断是否异常

            //if ((Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 0)|| Lib_Card.Configure.Parameter.Machine_Type == 1)
            {
                //富士伺服在下面判断 天平状态 原有不动 绿维的放在上面 并且置位 是否回原点

                //判断是否异常
                FADM_Object.Communal.BalanceState("针检");
            }
            //调零
            //Lib_SerialPort.Balance.METTLER.bZeroSign = true;

            i_mRes = MyModbusFun.TargetMove(2, 0, 0);
            if (-2 == i_mRes)
                throw new Exception("收到退出消息");

            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达天平位");




            if ("小针筒" == Convert.ToString(dt_bottle_details.Rows[0][0]) || "Little Syringe" == Convert.ToString(dt_bottle_details.Rows[0][0]))
            {
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注废液5000到天平启动(" + (Lib_Card.Configure.Parameter.Correcting_S_Pulse + 10000).ToString() + ")");
                i_mRes = MyModbusFun.Shove(Lib_Card.Configure.Parameter.Correcting_S_Pulse + 10000, 0);//天平位注液5000
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注废液5000完成");

            }
            else
            {
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注废液5000到天平启动(" + (Lib_Card.Configure.Parameter.Correcting_B_Pulse + 10000).ToString() + ")");
                i_mRes = MyModbusFun.Shove(Lib_Card.Configure.Parameter.Correcting_B_Pulse + 10000, 1);//天平位注液5000
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注废液5000完成");
            }

            //记录初始天平读数
            double d_balanceValue0 = FADM_Object.Communal.SteBalance();
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平读数：" + d_balanceValue0);

            //注液
            //Lib_Card.ADT8940A1.Module.Infusion.Infusion infusion = new Lib_Card.ADT8940A1.Module.Infusion.Infusion_Up();
            if ("小针筒" == Convert.ToString(dt_bottle_details.Rows[0][0]) || "Little Syringe" == Convert.ToString(dt_bottle_details.Rows[0][0]))
            {
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液启动(10000)");

                i_mRes = MyModbusFun.Shove(10000, 0);//z轴走到-10000的位置
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
            }
            else
            {
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液启动(10000)");
                i_mRes = MyModbusFun.Shove(10000, 1);//z轴走到-10000的位置

                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
            }
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液完成");

            //读取天平数据
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数启动");
            double d_blRRead = FADM_Object.Communal.SteBalance();
            double d_blWeight = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blRRead - d_balanceValue0)) : Convert.ToDouble(string.Format("{0:F3}", d_blRRead - d_balanceValue0));
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数：" + d_blRRead + ",实际重量：" + d_blWeight);


            FADM_Object.Communal._fadmSqlserver.ReviseData(
                "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_blWeight + " WHERE BottleNum = '" + i_bottleNo + "';");



            //验证
            int i_adjust = 0;



            if ("小针筒" == Convert.ToString(dt_bottle_details.Rows[0][0]) || "Little Syringe" == Convert.ToString(dt_bottle_details.Rows[0][0]))
            {
                if (0 < d_blWeight)
                    i_adjust = Convert.ToInt32(Lib_Card.Configure.Parameter.Correcting_S_Pulse / d_blWeight);

                int i_rPulse = 10000 - Convert.ToInt32(i_adjust * Lib_Card.Configure.Parameter.Correcting_S_Weight);
                if (0 >= i_rPulse)
                    i_rPulse = 0;
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "验证启动(" + i_rPulse + ")");
                i_mRes = MyModbusFun.Shove(i_rPulse, 0);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
            }
            else
            {
                if (0 < d_blWeight)
                    i_adjust = Convert.ToInt32(Lib_Card.Configure.Parameter.Correcting_B_Pulse / d_blWeight);
                int i_rPulse = 10000 - Convert.ToInt32(i_adjust * Lib_Card.Configure.Parameter.Correcting_B_Weight);
                if (0 >= i_rPulse)
                    i_rPulse = 0;
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "验证启动(" + i_rPulse + ")");
                i_mRes = MyModbusFun.Shove(i_rPulse, 1);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
            }

            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "验证完成");


            //读取验证重量
            Thread thread = new Thread(() =>
            {
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数启动");

                double d_blReRead = FADM_Object.Communal.SteBalance();
                double d_blRWeight = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blReRead - d_blRRead)) : Convert.ToDouble(string.Format("{0:F3}", d_blReRead - d_blRRead));

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数：" + d_blReRead + ",实际重量：" + d_blRWeight);

                FADM_Object.Communal._fadmSqlserver.ReviseData(
                    "UPDATE bottle_details SET CurrentWeight = CurrentWeight - '" + d_blRWeight + "' WHERE BottleNum = '" + i_bottleNo + "';");
                double d_blRErr = 0;
                if ("小针筒" == Convert.ToString(dt_bottle_details.Rows[0][0]) || "Little Syringe" == Convert.ToString(dt_bottle_details.Rows[0][0]))
                {
                    d_blRErr = Convert.ToDouble(string.Format("{0:F3}", d_blRWeight - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                }
                else
                {
                    d_blRErr = Convert.ToDouble(string.Format("{0:F3}", d_blRWeight - Lib_Card.Configure.Parameter.Correcting_B_Weight));
                }

                d_blRErr = d_blRErr < 0 ? -d_blRErr : d_blRErr;

                if (d_blRErr > Lib_Card.Configure.Parameter.Other_AErr_Correcting)
                {
                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                        "UPDATE bottle_details SET AdjustSuccess = 0 WHERE BottleNum = '" + i_bottleNo + "';");
                    i_res = -1;
                }
                else
                {
                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                        "UPDATE bottle_details SET  LastAdjustWeight = CurrentAdjustWeight, " +
                        "CurrentAdjustWeight =" + d_blWeight + ", " + "AdjustValue = " + i_adjust + ", " +
                        "AdjustSuccess = 1 WHERE BottleNum = '" + i_bottleNo + "';");


                    i_res = 0;
                }

            });
            thread.Start();
            //移动到母液瓶

            thread.Join();

            //解锁6工位
            MyModbusFun.UnLockSix();

            if (i_res == -1)
            {
                if (FADM_Object.Communal._b_isHasWashSyringe)
                {
                    //移动到洗针位置
                    i_mRes = MyModbusFun.TargetMove(12, 0, 0);
                    //执行洗针动作
                    MyModbusFun.WashSyringes(0);
                }

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找放针母液瓶");
                FADM_Object.Communal._i_optBottleNum = i_bottleNo;
                i_mRes = MyModbusFun.TargetMove(11, 0, 0);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达放针母液瓶");

                //放瓶

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "放针启动");

                if ("小针筒" == Convert.ToString(dt_bottle_details.Rows[0][0]) || "Little Syringe" == Convert.ToString(dt_bottle_details.Rows[0][0]))
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
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "放针完成");
                Communal._b_isGetSyringes = false;
            }

            //复位
            //Lib_SerialPort.Balance.METTLER.bReSetSign = true;
            return i_res;

        }


        /// <summary>
        /// 针检流程
        /// </summary>
        /// <param name="i_bottleNo">瓶号</param>
        /// <returns>0：成功；-1：失败</returns>
        /// <exception cref="Exception"></exception>
        private int MyDripCheckProcess(int i_bottleNo, bool b_drip, int i_lowSrart)
        {
            int i_res = 0;

            //抽液
            DataTable dt_bottle_details = FADM_Object.Communal._fadmSqlserver.GetData(
                "SELECT SyringeType FROM bottle_details WHERE BottleNum = '" + i_bottleNo + "';");

            if ("计量泵" == Convert.ToString(dt_bottle_details.Rows[0][0]) || "Metering Pump" == Convert.ToString(dt_bottle_details.Rows[0][0]))
            {
                //如果AB助剂
                return i_res;
            }
            else
            {
            label11:

                //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                //{
                //    //富士伺服在下面判断 天平状态 原有不动 绿维的放在上面 并且置位 是否回原点 绿维的放移动机械手前面

                //    //判断是否异常
                //    FADM_Object.Communal.BalanceState("针检");
                //}


                //移动到母液瓶
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找" + i_bottleNo + "号母液瓶");
                FADM_Object.Communal._i_optBottleNum = i_bottleNo;
                int i_mRes = MyModbusFun.TargetMove(0, i_bottleNo, 1);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达" + i_bottleNo + "号母液瓶");




                if ("小针筒" == Convert.ToString(dt_bottle_details.Rows[0][0]) || "Little Syringe" == Convert.ToString(dt_bottle_details.Rows[0][0]))
                {
                    int i_pulse = Lib_Card.Configure.Parameter.Correcting_S_Pulse + 10000;
                    //label2:
                    try
                    {
                        //多抽5000脉冲用于到天平滴掉
                        int i_totalPulse = i_pulse - Lib_Card.Configure.Parameter.Other_Z_BackPulse + 5000;
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抽液启动(" + i_totalPulse + ")");

                        i_mRes = MyModbusFun.Extract(i_totalPulse, true, 0); //抽液 排空
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");

                        //判断洗针筒字典是否存在需要洗针的
                        if (FADM_Object.Communal._b_isHasWashSyringe)
                        {
                            List<int> value = new List<int>();
                            //可以放针
                            value.Add(1);
                            //可以洗
                            value.Add(1);
                            if (Communal.GetValueWash(i_bottleNo.ToString(), out value))
                            {
                                //如果之前标记过夹不到针筒，现在抽液完成，可改为夹到针筒
                                if (value[1] == 0)
                                {
                                    value[1] = 1;
                                    Communal.AddOrUpdateWash(i_bottleNo.ToString(), value);
                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if ("未发现针筒" == ex.Message)
                        {
                            //判断洗针筒字典是否存在需要洗针的
                            if (FADM_Object.Communal._b_isHasWashSyringe)
                            {
                                List<int> value = new List<int>();
                                //可以放针
                                value.Add(1);
                                //可以洗
                                value.Add(1);
                                if (Communal.GetValueWash(i_bottleNo.ToString(), out value))
                                {
                                    //标记夹不到针筒
                                    value[1] = 0;
                                    Communal.AddOrUpdateWash(i_bottleNo.ToString(), value);
                                }
                            }
                            //滴液时针检处理
                            if (i_lowSrart == 2)
                            {
                                FADM_Object.MyAlarm myAlarm;
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    myAlarm = new FADM_Object.MyAlarm(i_bottleNo + "号母液瓶未找到针筒，是否继续执行?(继续寻找请点是，退出针检请点否)", "普通针检", true, 1, true);
                                else
                                    myAlarm = new FADM_Object.MyAlarm("The syringe for bottle " + i_bottleNo + " was not found.Do you want to continue? " +
                                        "(To continue searching, please click Yes.To exit the needle test, please click No)", "Regular needle examination", true, 1, true);
                                while (true)
                                {
                                    if (0 != myAlarm._i_alarm_Choose)
                                        break;
                                    Thread.Sleep(1);
                                }

                                if (1 == myAlarm._i_alarm_Choose)
                                    goto label11;
                                else
                                    return -3;
                            }
                            //后处理时针检处理
                            else if (false == b_drip)
                            {
                                return -3;
                            }
                            else
                            {
                                return -2;
                            }
                        }
                        else if ("取针筒时抓手关失败" == ex.Message)
                        {
                            //判断洗针筒字典是否存在需要洗针的
                            if (FADM_Object.Communal._b_isHasWashSyringe)
                            {
                                List<int> value = new List<int>();
                                //可以放针
                                value.Add(1);
                                //可以洗
                                value.Add(1);
                                if (Communal.GetValueWash(i_bottleNo.ToString(), out value))
                                {
                                    //标记夹不到针筒
                                    value[1] = 0;
                                    Communal.AddOrUpdateWash(i_bottleNo.ToString(), value);
                                }
                            }
                            //滴液时针检处理
                            if (i_lowSrart == 2)
                            {
                                FADM_Object.MyAlarm myAlarm;
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    myAlarm = new FADM_Object.MyAlarm(i_bottleNo + "号母液瓶取针筒时抓手关失败，是否继续执行?(继续寻找请点是，退出针检请点否)", "普通针检", true, 1, true);
                                else
                                    myAlarm = new FADM_Object.MyAlarm(" bottle " + i_bottleNo + " The gripper failed to close when taking out the syringe,Do you want to continue? " +
                                        "(To continue searching, please click Yes.To exit the needle test, please click No)", "Regular needle examination", true, 1, true);
                                while (true)
                                {
                                    if (0 != myAlarm._i_alarm_Choose)
                                        break;
                                    Thread.Sleep(1);
                                }

                                if (1 == myAlarm._i_alarm_Choose)
                                    goto label11;
                                else
                                    return -3;
                            }
                            //后处理时针检处理
                            else if (false == b_drip)
                            {
                                return -3;
                            }
                            else
                            {
                                return -2;
                            }
                        }
                        else if ("取针筒时气缸下失败" == ex.Message)
                        {
                            //判断洗针筒字典是否存在需要洗针的
                            if (FADM_Object.Communal._b_isHasWashSyringe)
                            {
                                List<int> value = new List<int>();
                                //可以放针
                                value.Add(1);
                                //可以洗
                                value.Add(1);
                                if (Communal.GetValueWash(i_bottleNo.ToString(), out value))
                                {
                                    //标记夹不到针筒
                                    value[1] = 0;
                                    Communal.AddOrUpdateWash(i_bottleNo.ToString(), value);
                                }
                            }
                            //滴液时针检处理
                            if (i_lowSrart == 2)
                            {
                                FADM_Object.MyAlarm myAlarm;
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    myAlarm = new FADM_Object.MyAlarm(i_bottleNo + "号母液瓶取针筒时气缸下失败，是否继续执行?(继续寻找请点是，退出针检请点否)", "普通针检", true, 1, true);
                                else
                                    myAlarm = new FADM_Object.MyAlarm(" bottle " + i_bottleNo + " The cylinder failed to descend when removing the syringe,Do you want to continue? " +
                                        "(To continue searching, please click Yes.To exit the needle test, please click No)", "Regular needle examination", true, 1, true);
                                while (true)
                                {
                                    if (0 != myAlarm._i_alarm_Choose)
                                        break;
                                    Thread.Sleep(1);
                                }

                                if (1 == myAlarm._i_alarm_Choose)
                                    goto label11;
                                else
                                    return -3;
                            }
                            //后处理时针检处理
                            else if (false == b_drip)
                            {
                                return -3;
                            }
                            else
                            {
                                return -2;
                            }
                        }
                        else
                            throw;

                    }
                }
                else
                {
                    int i_pulse = Lib_Card.Configure.Parameter.Correcting_B_Pulse + 10000;
                    //FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抽液启动(" + i_pulse + ")");
                    //label3:
                    try
                    {
                        //多抽5000脉冲用于到天平滴掉
                        int i_totalPulse = i_pulse - Lib_Card.Configure.Parameter.Other_Z_BackPulse + 5000;
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抽液启动(" + i_totalPulse + ")");
                        i_mRes = MyModbusFun.Extract(i_totalPulse, true, 1);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");

                        //判断洗针筒字典是否存在需要洗针的
                        if (FADM_Object.Communal._b_isHasWashSyringe)
                        {
                            List<int> value = new List<int>();
                            //可以放针
                            value.Add(1);
                            //可以洗
                            value.Add(1);
                            if (Communal.GetValueWash(i_bottleNo.ToString(), out value))
                            {
                                //如果之前标记过夹不到针筒，现在抽液完成，可改为夹到针筒
                                if (value[1] == 0)
                                {
                                    value[1] = 1;
                                    Communal.AddOrUpdateWash(i_bottleNo.ToString(), value);
                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {

                        if ("未发现针筒" == ex.Message)
                        {
                            //判断洗针筒字典是否存在需要洗针的
                            if (FADM_Object.Communal._b_isHasWashSyringe)
                            {
                                List<int> value = new List<int>();
                                //可以放针
                                value.Add(1);
                                //可以洗
                                value.Add(1);
                                if (Communal.GetValueWash(i_bottleNo.ToString(), out value))
                                {
                                    //标记夹不到针筒
                                    value[1] = 0;
                                    Communal.AddOrUpdateWash(i_bottleNo.ToString(), value);
                                }
                            }

                            if (false == b_drip || i_lowSrart == 2)
                            {
                                FADM_Object.MyAlarm myAlarm;
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    myAlarm = new FADM_Object.MyAlarm(i_bottleNo + "号母液瓶未找到针筒，是否继续执行?(继续寻找请点是，退出针检请点否)", "普通针检", true, 1, true);
                                else
                                    myAlarm = new FADM_Object.MyAlarm("The syringe for bottle " + i_bottleNo + " was not found.Do you want to continue? " +
                                        "(To continue searching, please click Yes.To exit the needle test, please click No)", "Regular needle examination", true, 1, true);

                                while (true)
                                {
                                    if (0 != myAlarm._i_alarm_Choose)
                                        break;
                                    Thread.Sleep(1);
                                }

                                if (1 == myAlarm._i_alarm_Choose)
                                    goto label11;
                                else
                                    //throw new Exception("收到退出消息");
                                    return -3;
                            }
                            else
                            {
                                return -2;
                            }

                        }
                        else if ("取针筒时抓手关失败" == ex.Message)
                        {
                            //判断洗针筒字典是否存在需要洗针的
                            if (FADM_Object.Communal._b_isHasWashSyringe)
                            {
                                List<int> value = new List<int>();
                                //可以放针
                                value.Add(1);
                                //可以洗
                                value.Add(1);
                                if (Communal.GetValueWash(i_bottleNo.ToString(), out value))
                                {
                                    //标记夹不到针筒
                                    value[1] = 0;
                                    Communal.AddOrUpdateWash(i_bottleNo.ToString(), value);
                                }
                            }
                            //滴液时针检处理
                            if (i_lowSrart == 2)
                            {
                                FADM_Object.MyAlarm myAlarm;
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    myAlarm = new FADM_Object.MyAlarm(i_bottleNo + "号母液瓶取针筒时抓手关失败，是否继续执行?(继续寻找请点是，退出针检请点否)", "普通针检", true, 1, true);
                                else
                                    myAlarm = new FADM_Object.MyAlarm(" bottle " + i_bottleNo + " The gripper failed to close when taking out the syringe,Do you want to continue? " +
                                        "(To continue searching, please click Yes.To exit the needle test, please click No)", "Regular needle examination", true, 1, true);
                                while (true)
                                {
                                    if (0 != myAlarm._i_alarm_Choose)
                                        break;
                                    Thread.Sleep(1);
                                }

                                if (1 == myAlarm._i_alarm_Choose)
                                    goto label11;
                                else
                                    return -3;
                            }
                            //后处理时针检处理
                            else if (false == b_drip)
                            {
                                return -3;
                            }
                            else
                            {
                                return -2;
                            }
                        }
                        else if ("取针筒时气缸下失败" == ex.Message)
                        {
                            //判断洗针筒字典是否存在需要洗针的
                            if (FADM_Object.Communal._b_isHasWashSyringe)
                            {
                                List<int> value = new List<int>();
                                //可以放针
                                value.Add(1);
                                //可以洗
                                value.Add(1);
                                if (Communal.GetValueWash(i_bottleNo.ToString(), out value))
                                {
                                    //标记夹不到针筒
                                    value[1] = 0;
                                    Communal.AddOrUpdateWash(i_bottleNo.ToString(), value);
                                }
                            }
                            //滴液时针检处理
                            if (i_lowSrart == 2)
                            {
                                FADM_Object.MyAlarm myAlarm;
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    myAlarm = new FADM_Object.MyAlarm(i_bottleNo + "号母液瓶取针筒时气缸下失败，是否继续执行?(继续寻找请点是，退出针检请点否)", "普通针检", true, 1, true);
                                else
                                    myAlarm = new FADM_Object.MyAlarm(" bottle " + i_bottleNo + " The cylinder failed to descend when removing the syringe,Do you want to continue? " +
                                        "(To continue searching, please click Yes.To exit the needle test, please click No)", "Regular needle examination", true, 1, true);
                                while (true)
                                {
                                    if (0 != myAlarm._i_alarm_Choose)
                                        break;
                                    Thread.Sleep(1);
                                }

                                if (1 == myAlarm._i_alarm_Choose)
                                    goto label11;
                                else
                                    return -3;
                            }
                            //后处理时针检处理
                            else if (false == b_drip)
                            {
                                return -3;
                            }
                            else
                            {
                                return -2;
                            }
                        }
                        else
                            throw;


                    }
                }




                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抽液完成");

                //移动到天平位

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找天平位");
                //if ((Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 0) || Lib_Card.Configure.Parameter.Machine_Type == 1)
                {
                    //富士伺服在下面判断 天平状态 原有不动 绿维的放在上面 并且置位 是否回原点

                    //判断是否异常
                    FADM_Object.Communal.BalanceState("针检");
                }
               

                //调零
                //Lib_SerialPort.Balance.METTLER.bZeroSign = true;

                i_mRes = MyModbusFun.TargetMove(2, 0, 0);
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

                if ("小针筒" == Convert.ToString(dt_bottle_details.Rows[0][0]) || "Little Syringe" == Convert.ToString(dt_bottle_details.Rows[0][0]))
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注废液5000到天平启动(" + (Lib_Card.Configure.Parameter.Correcting_S_Pulse + 10000).ToString() + ")");
                    i_mRes = MyModbusFun.Shove(Lib_Card.Configure.Parameter.Correcting_S_Pulse + 10000, 0);//天平位注液5000
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注废液5000完成");

                }
                else
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注废液5000到天平启动(" + (Lib_Card.Configure.Parameter.Correcting_B_Pulse + 10000).ToString() + ")");
                    i_mRes = MyModbusFun.Shove(Lib_Card.Configure.Parameter.Correcting_B_Pulse + 10000, 1);//天平位注液5000
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注废液5000完成");
                }

                //记录初始天平读数
                double d_balanceValue0 = FADM_Object.Communal.SteBalance();
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平读数：" + d_balanceValue0);

                //注液
                //Lib_Card.ADT8940A1.Module.Infusion.Infusion infusion = new Lib_Card.ADT8940A1.Module.Infusion.Infusion_Up();
                if ("小针筒" == Convert.ToString(dt_bottle_details.Rows[0][0]) || "Little Syringe" == Convert.ToString(dt_bottle_details.Rows[0][0]))
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液启动(10000)");

                    i_mRes = MyModbusFun.Shove(10000, 0);//z轴走到-10000的位置
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                }
                else
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液启动(10000)");
                    i_mRes = MyModbusFun.Shove(10000, 1);//z轴走到-10000的位置

                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                }
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液完成");

                //读取天平数据
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数启动");
                double d_blRRead = FADM_Object.Communal.SteBalance();
                double d_blWeight = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blRRead - d_balanceValue0)) : Convert.ToDouble(string.Format("{0:F3}", d_blRRead - d_balanceValue0));
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数：" + d_blRRead + ",实际重量：" + d_blWeight);


                FADM_Object.Communal._fadmSqlserver.ReviseData(
                    "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_blWeight + " WHERE BottleNum = '" + i_bottleNo + "';");



                //验证
                int i_adjust = 0;



                if ("小针筒" == Convert.ToString(dt_bottle_details.Rows[0][0]) || "Little Syringe" == Convert.ToString(dt_bottle_details.Rows[0][0]))
                {
                    if (0 < d_blWeight)
                        i_adjust = Convert.ToInt32(Lib_Card.Configure.Parameter.Correcting_S_Pulse / d_blWeight);

                    int i_rPulse = 10000 - Convert.ToInt32(i_adjust * Lib_Card.Configure.Parameter.Correcting_S_Weight);
                    if (0 >= i_rPulse)
                        i_rPulse = 0;
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "验证启动(" + i_rPulse + ")");
                    i_mRes = MyModbusFun.Shove(i_rPulse, 0);
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                }
                else
                {
                    if (0 < d_blWeight)
                        i_adjust = Convert.ToInt32(Lib_Card.Configure.Parameter.Correcting_B_Pulse / d_blWeight);
                    int i_rPulse = 10000 - Convert.ToInt32(i_adjust * Lib_Card.Configure.Parameter.Correcting_B_Weight);
                    if (0 >= i_rPulse)
                        i_rPulse = 0;
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "验证启动(" + i_rPulse + ")");
                    i_mRes = MyModbusFun.Shove(i_rPulse, 1);
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                }

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "验证完成");


                //读取验证重量
                Thread thread = new Thread(() =>
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数启动");

                    double d_blReRead = FADM_Object.Communal.SteBalance();
                    double d_blRWeight = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blReRead - d_blRRead)) : Convert.ToDouble(string.Format("{0:F3}", d_blReRead - d_blRRead));

                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数：" + d_blReRead + ",实际重量：" + d_blRWeight);

                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - '" + d_blRWeight + "' WHERE BottleNum = '" + i_bottleNo + "';");
                    double d_blRErr = 0;
                    if ("小针筒" == Convert.ToString(dt_bottle_details.Rows[0][0]) || "Little Syringe" == Convert.ToString(dt_bottle_details.Rows[0][0]))
                    {
                        d_blRErr = Convert.ToDouble(string.Format("{0:F3}", d_blRWeight - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                    }
                    else
                    {
                        d_blRErr = Convert.ToDouble(string.Format("{0:F3}", d_blRWeight - Lib_Card.Configure.Parameter.Correcting_B_Weight));
                    }

                    d_blRErr = d_blRErr < 0 ? -d_blRErr : d_blRErr;

                    if (d_blRErr > Lib_Card.Configure.Parameter.Other_AErr_Correcting)
                    {
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE bottle_details SET AdjustSuccess = 0 WHERE BottleNum = '" + i_bottleNo + "';");
                        i_res = -1;
                    }
                    else
                    {
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE bottle_details SET  LastAdjustWeight = CurrentAdjustWeight, " +
                            "CurrentAdjustWeight =" + d_blWeight + ", " + "AdjustValue = " + i_adjust + ", " +
                            "AdjustSuccess = 1 WHERE BottleNum = '" + i_bottleNo + "';");


                        i_res = 0;
                    }

                });
                thread.Start();
                //移动到母液瓶

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找" + i_bottleNo + "号母液瓶");
                FADM_Object.Communal._i_optBottleNum = i_bottleNo;
                i_mRes = MyModbusFun.TargetMove(0, i_bottleNo, 0);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达" + i_bottleNo + "号母液瓶");

                //放瓶

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", i_bottleNo + "号母液瓶放针启动");

                if ("小针筒" == Convert.ToString(dt_bottle_details.Rows[0][0]) || "Little Syringe" == Convert.ToString(dt_bottle_details.Rows[0][0]))
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
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", i_bottleNo + "号母液瓶放针完成");
                thread.Join();
                //解锁6工位
                MyModbusFun.UnLockSix();
                //复位
                //Lib_SerialPort.Balance.METTLER.bReSetSign = true;
                return i_res;
            }

        }


        /// <summary>
        /// 滴液针检
        /// </summary>
        /// <param name="i_bottleNo">瓶号</param>
        /// <returns>0：针检成功；-1：针检失败</returns>
        public int MyDripCheck(int i_bottleNo, bool b_drip, int i_lowSrart)
        {
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", i_bottleNo + "号母液瓶针检启动");
            int iRes;
            if (i_bottleNo == 999 || i_bottleNo == 888)
            {
                iRes = MyABSDripCheckProcess(i_bottleNo, b_drip, i_lowSrart);
            }
            else
            {
                iRes = MyDripCheckProcess(i_bottleNo, b_drip, i_lowSrart);
            }
            if (-2 == iRes)
            {
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", i_bottleNo + "号母液瓶未发现针筒，针检退出");
            }
            else
            {
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", i_bottleNo + "号母液瓶针检完成");
            }
            return iRes;
        }


        /// <summary>
        /// 针检流程
        /// </summary>
        /// <param name="i_bottleNo">瓶号</param>
        /// <returns>0：成功；-1：失败</returns>
        /// <exception cref="Exception"></exception>
        private int CheckProcess(int i_bottleNo, bool b_drip, int i_lowSrart)
        {

            int i_res = 0;

            //抽液
            DataTable dt_bottle_details = FADM_Object.Communal._fadmSqlserver.GetData(
                "SELECT SyringeType FROM bottle_details WHERE BottleNum = '" + i_bottleNo + "';");


            if ("计量泵" == Convert.ToString(dt_bottle_details.Rows[0][0]) || "Metering Pump" == Convert.ToString(dt_bottle_details.Rows[0][0]))
            {
                //如果AB助剂
                return i_res;
            }
            else
            {

                //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                //{
                //    //富士伺服在下面判断 天平状态 原有不动 绿维的放在上面 并且置位 是否回原点 绿维的放移动机械手前面

                //    //判断是否异常
                //    FADM_Object.Communal.BalanceState("针检");
                //}

                //移动到母液瓶
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找" + i_bottleNo + "号母液瓶");
                FADM_Object.Communal._i_optBottleNum = i_bottleNo;
                int i_mRes = MyModbusFun.TargetMove(0, i_bottleNo, 1);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达" + i_bottleNo + "号母液瓶");



                if ("小针筒" == Convert.ToString(dt_bottle_details.Rows[0][0]) || "Little Syringe" == Convert.ToString(dt_bottle_details.Rows[0][0]))
                {
                    int i_pulse = Lib_Card.Configure.Parameter.Correcting_S_Pulse + 10000;
                label2:
                    try
                    {
                        //多抽5000脉冲用于到天平滴掉
                        int i_totalPulse = i_pulse - Lib_Card.Configure.Parameter.Other_Z_BackPulse + 5000;
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抽液启动(" + i_totalPulse + ")");
                        i_mRes = MyModbusFun.Extract(i_totalPulse, true, 0); //排空
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");

                        //判断洗针筒字典是否存在需要洗针的
                        if (FADM_Object.Communal._b_isHasWashSyringe)
                        {
                            List<int> value = new List<int>();
                            //可以放针
                            value.Add(1);
                            //可以洗
                            value.Add(1);
                            if (Communal.GetValueWash(i_bottleNo.ToString(), out value))
                            {
                                //如果之前标记过夹不到针筒，现在抽液完成，可改为夹到针筒
                                if (value[1] == 0)
                                {
                                    value[1] = 1;
                                    Communal.AddOrUpdateWash(i_bottleNo.ToString(), value);
                                }

                            }
                        }

                        //FADM_Object.Communal.WriteMachineStatus(0);
                        //return 0;
                    }
                    catch (Exception ex)
                    {
                        if ("未发现针筒" == ex.Message)
                        {
                            //判断洗针筒字典是否存在需要洗针的
                            if (FADM_Object.Communal._b_isHasWashSyringe)
                            {
                                List<int> value = new List<int>();
                                //可以放针
                                value.Add(1);
                                //可以洗
                                value.Add(1);
                                if (Communal.GetValueWash(i_bottleNo.ToString(), out value))
                                {
                                    //标记夹不到针筒
                                    value[1] = 0;
                                    Communal.AddOrUpdateWash(i_bottleNo.ToString(), value);
                                }
                            }
                            if (false == b_drip || i_lowSrart == 2)
                            {
                                FADM_Object.MyAlarm myAlarm;
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    myAlarm = new FADM_Object.MyAlarm(i_bottleNo + "号母液瓶未找到针筒，是否继续执行?(继续寻找请点是，退出针检请点否)", "普通针检", true, 1, true);
                                else
                                    myAlarm = new FADM_Object.MyAlarm(i_bottleNo + " bottle did not find a syringe. Do you want to continue? " +
                                        "(To continue searching, please click Yes. To exit the needle test, please click No)", "Regular needle examination", true, 1, true);
                                while (true)
                                {
                                    if (0 != myAlarm._i_alarm_Choose)
                                        break;
                                    Thread.Sleep(1);
                                }

                                if (1 == myAlarm._i_alarm_Choose)
                                    goto label2;
                                else
                                    throw new Exception("收到退出消息");
                            }
                            else
                            {
                                return -2;
                            }
                        }
                        else if ("取针筒时抓手关失败" == ex.Message)
                        {
                            //判断洗针筒字典是否存在需要洗针的
                            if (FADM_Object.Communal._b_isHasWashSyringe)
                            {
                                List<int> value = new List<int>();
                                //可以放针
                                value.Add(1);
                                //可以洗
                                value.Add(1);
                                if (Communal.GetValueWash(i_bottleNo.ToString(), out value))
                                {
                                    //标记夹不到针筒
                                    value[1] = 0;
                                    Communal.AddOrUpdateWash(i_bottleNo.ToString(), value);
                                }
                            }

                            if (false == b_drip)
                            {
                                FADM_Object.MyAlarm myAlarm;
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    myAlarm = new FADM_Object.MyAlarm(i_bottleNo + "号母液瓶取针筒时抓手关失败，是否继续执行 ? (继续寻找请点是，退出针检请点否)", "普通针检", true, 1, true);
                                else
                                    myAlarm = new FADM_Object.MyAlarm(i_bottleNo + " bottle The gripper failed to close when taking out the syringe. Do you want to continue? " +
                                      "(To continue searching, please click Yes. To exit the needle test, please click No)", "Regular needle examination", true, 1, true);
                                while (true)
                                {
                                    if (0 != myAlarm._i_alarm_Choose)
                                        break;
                                    Thread.Sleep(1);
                                }

                                if (1 == myAlarm._i_alarm_Choose)
                                    goto label2;
                                else
                                    throw new Exception("收到退出消息");
                            }
                            else
                            {
                                return -2;
                            }

                        }
                        else if ("取针筒时气缸下失败" == ex.Message)
                        {
                            //判断洗针筒字典是否存在需要洗针的
                            if (FADM_Object.Communal._b_isHasWashSyringe)
                            {
                                List<int> value = new List<int>();
                                //可以放针
                                value.Add(1);
                                //可以洗
                                value.Add(1);
                                if (Communal.GetValueWash(i_bottleNo.ToString(), out value))
                                {
                                    //标记夹不到针筒
                                    value[1] = 0;
                                    Communal.AddOrUpdateWash(i_bottleNo.ToString(), value);
                                }
                            }

                            if (false == b_drip)
                            {
                                FADM_Object.MyAlarm myAlarm;
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    myAlarm = new FADM_Object.MyAlarm(i_bottleNo + "号母液瓶取针筒时气缸下失败，是否继续执行 ? (继续寻找请点是，退出针检请点否)", "普通针检", true, 1, true);
                                else
                                    myAlarm = new FADM_Object.MyAlarm(i_bottleNo + " bottle The cylinder failed to descend when removing the syringe. Do you want to continue? " +
                                      "(To continue searching, please click Yes. To exit the needle test, please click No)", "Regular needle examination", true, 1, true);
                                while (true)
                                {
                                    if (0 != myAlarm._i_alarm_Choose)
                                        break;
                                    Thread.Sleep(1);
                                }

                                if (1 == myAlarm._i_alarm_Choose)
                                    goto label2;
                                else
                                    throw new Exception("收到退出消息");
                            }
                            else
                            {
                                return -2;
                            }

                        }
                        else
                            throw;

                    }
                }
                else
                {
                    int i_pulse = Lib_Card.Configure.Parameter.Correcting_B_Pulse + 10000;
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抽液启动(" + i_pulse + ")");
                label3:
                    try
                    {
                        //多抽5000脉冲用于到天平滴掉
                        int i_totalPulse = i_pulse - Lib_Card.Configure.Parameter.Other_Z_BackPulse + 5000;
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抽液启动(" + i_totalPulse + ")");
                        i_mRes = MyModbusFun.Extract(i_totalPulse, true, 1);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");

                        //判断洗针筒字典是否存在需要洗针的
                        if (FADM_Object.Communal._b_isHasWashSyringe)
                        {
                            List<int> value = new List<int>();
                            //可以放针
                            value.Add(1);
                            //可以洗
                            value.Add(1);
                            if (Communal.GetValueWash(i_bottleNo.ToString(), out value))
                            {
                                //如果之前标记过夹不到针筒，现在抽液完成，可改为夹到针筒
                                if (value[1] == 0)
                                {
                                    value[1] = 1;
                                    Communal.AddOrUpdateWash(i_bottleNo.ToString(), value);
                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {

                        if ("未发现针筒" == ex.Message)
                        {
                            //判断洗针筒字典是否存在需要洗针的
                            if (FADM_Object.Communal._b_isHasWashSyringe)
                            {
                                List<int> value = new List<int>();
                                //可以放针
                                value.Add(1);
                                //可以洗
                                value.Add(1);
                                if (Communal.GetValueWash(i_bottleNo.ToString(), out value))
                                {
                                    //标记夹不到针筒
                                    value[1] = 0;
                                    Communal.AddOrUpdateWash(i_bottleNo.ToString(), value);
                                }
                            }

                            if (false == b_drip)
                            {
                                FADM_Object.MyAlarm myAlarm;
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    myAlarm = new FADM_Object.MyAlarm(i_bottleNo + "号母液瓶未找到针筒，是否继续执行 ? (继续寻找请点是，退出针检请点否)", "普通针检", true, 1, true);
                                else
                                    myAlarm = new FADM_Object.MyAlarm(i_bottleNo + " bottle did not find a syringe. Do you want to continue? " +
                                      "(To continue searching, please click Yes. To exit the needle test, please click No)", "Regular needle examination", true, 1, true);
                                while (true)
                                {
                                    if (0 != myAlarm._i_alarm_Choose)
                                        break;
                                    Thread.Sleep(1);
                                }

                                if (1 == myAlarm._i_alarm_Choose)
                                    goto label3;
                                else
                                    throw new Exception("收到退出消息");
                            }
                            else
                            {
                                return -2;
                            }

                        }
                        else if ("取针筒时抓手关失败" == ex.Message)
                        {
                            //判断洗针筒字典是否存在需要洗针的
                            if (FADM_Object.Communal._b_isHasWashSyringe)
                            {
                                List<int> value = new List<int>();
                                //可以放针
                                value.Add(1);
                                //可以洗
                                value.Add(1);
                                if (Communal.GetValueWash(i_bottleNo.ToString(), out value))
                                {
                                    //标记夹不到针筒
                                    value[1] = 0;
                                    Communal.AddOrUpdateWash(i_bottleNo.ToString(), value);
                                }
                            }

                            if (false == b_drip)
                            {
                                FADM_Object.MyAlarm myAlarm;
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    myAlarm = new FADM_Object.MyAlarm(i_bottleNo + "号母液瓶取针筒时抓手关失败，是否继续执行 ? (继续寻找请点是，退出针检请点否)", "普通针检", true, 1, true);
                                else
                                    myAlarm = new FADM_Object.MyAlarm(i_bottleNo + " bottle The gripper failed to close when taking out the syringe. Do you want to continue? " +
                                      "(To continue searching, please click Yes. To exit the needle test, please click No)", "Regular needle examination", true, 1, true);
                                while (true)
                                {
                                    if (0 != myAlarm._i_alarm_Choose)
                                        break;
                                    Thread.Sleep(1);
                                }

                                if (1 == myAlarm._i_alarm_Choose)
                                    goto label3;
                                else
                                    throw new Exception("收到退出消息");
                            }
                            else
                            {
                                return -2;
                            }

                        }
                        else if ("取针筒时气缸下失败" == ex.Message)
                        {
                            //判断洗针筒字典是否存在需要洗针的
                            if (FADM_Object.Communal._b_isHasWashSyringe)
                            {
                                List<int> value = new List<int>();
                                //可以放针
                                value.Add(1);
                                //可以洗
                                value.Add(1);
                                if (Communal.GetValueWash(i_bottleNo.ToString(), out value))
                                {
                                    //标记夹不到针筒
                                    value[1] = 0;
                                    Communal.AddOrUpdateWash(i_bottleNo.ToString(), value);
                                }
                            }

                            if (false == b_drip)
                            {
                                FADM_Object.MyAlarm myAlarm;
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    myAlarm = new FADM_Object.MyAlarm(i_bottleNo + "号母液瓶取针筒时气缸下失败，是否继续执行 ? (继续寻找请点是，退出针检请点否)", "普通针检", true, 1, true);
                                else
                                    myAlarm = new FADM_Object.MyAlarm(i_bottleNo + " bottle The cylinder failed to descend when removing the syringe. Do you want to continue? " +
                                      "(To continue searching, please click Yes. To exit the needle test, please click No)", "Regular needle examination", true, 1, true);
                                while (true)
                                {
                                    if (0 != myAlarm._i_alarm_Choose)
                                        break;
                                    Thread.Sleep(1);
                                }

                                if (1 == myAlarm._i_alarm_Choose)
                                    goto label3;
                                else
                                    throw new Exception("收到退出消息");
                            }
                            else
                            {
                                return -2;
                            }

                        }
                        else
                            throw;


                    }
                }

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抽液完成");

                //移动到天平位
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找天平位");


                //if ((Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 0) || Lib_Card.Configure.Parameter.Machine_Type == 1)
                {
                    //富士伺服在下面判断 天平状态 原有不动 绿维的放在上面 并且置位 是否回原点

                    //判断是否异常
                    FADM_Object.Communal.BalanceState("针检");
                }
                //调零
                if (FADM_Object.Communal._b_isZero)//清零
                    MyModbusFun.ClearBalance();//调用清零 发送指令成功就返回。下面在判断读数

                i_mRes = MyModbusFun.TargetMove(2, 0, 0);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");

                /*int index = 0;
                //往上抽600脉冲
                MyModbusFun.GetZPosition(ref index);
                Thread.Sleep(1000);
                MyModbusFun.Shove(index-600);
                Thread.Sleep(1000);*/

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达天平位");
                if (FADM_Object.Communal._b_isZero)
                {
                    while (true)
                    {
                        //判断是否成功清零
                        if (Convert.ToDouble(FADM_Object.Communal._s_balanceValue) == 0.0)
                        {
                            break;
                        }
                        else
                        {
                            //再次发调零
                            MyModbusFun.ClearBalance();//调用清零 发送指令成功就返回。下面在判断读数
                            Thread.Sleep(2000);
                            //Lib_SerialPort.Balance.METTLER.bZeroSign = true;
                        }
                        Thread.Sleep(1);
                    }
                }
                if ("小针筒" == Convert.ToString(dt_bottle_details.Rows[0][0]) || "Little Syringe" == Convert.ToString(dt_bottle_details.Rows[0][0]))
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注废液5000到天平启动(" + (Lib_Card.Configure.Parameter.Correcting_S_Pulse + 10000).ToString() + ")");
                    i_mRes = MyModbusFun.Shove(Lib_Card.Configure.Parameter.Correcting_S_Pulse + 10000, 0);//天平位注液5000
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注废液5000完成");

                }
                else
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注废液5000到天平启动(" + (Lib_Card.Configure.Parameter.Correcting_B_Pulse + 10000).ToString() + ")");
                    i_mRes = MyModbusFun.Shove(Lib_Card.Configure.Parameter.Correcting_B_Pulse + 10000, 1);//天平位注液5000
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注废液5000完成");
                }

                //label31:
                //    Thread.Sleep(Convert.ToInt32(Lib_Card.Configure.Parameter.Delay_Balance_Read * 1000));
                //    double dblBalanceValue31 = Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal._s_balanceValue));
                //    Thread.Sleep(Convert.ToInt32(Lib_Card.Configure.Parameter.Delay_Balance_Read * 1000));
                //    double dblBalanceValue32 = Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal._s_balanceValue));


                //    double dblDif31 = Convert.ToDouble(string.Format("{0:F3}", dblBalanceValue31 - dblBalanceValue32));
                //    dblDif31 = dblDif31 < 0 ? -dblDif31 : dblDif31;

                //    if (dblDif31 > Lib_Card.Configure.Parameter.Other_Stable_Value)
                //        goto label31;

                //记录初始天平读数
                double d_blBalanceValue0 = FADM_Object.Communal.SteBalance(); ;
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平读数：" + d_blBalanceValue0);

                //注液
                if ("小针筒" == Convert.ToString(dt_bottle_details.Rows[0][0]) || "Little Syringe" == Convert.ToString(dt_bottle_details.Rows[0][0]))
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液启动(10000)");

                    i_mRes = MyModbusFun.Shove(10000, 0);//z轴走到-100000的位置
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                }
                else
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液启动(10000)");
                    i_mRes = MyModbusFun.Shove(10000, 1);//z轴走到-100000的位置
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                }

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液完成");

                //读取天平数据
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数启动");
                double d_blRRead = FADM_Object.Communal.SteBalance();
                double d_blWeight = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blRRead - d_blBalanceValue0)) : Convert.ToDouble(string.Format("{0:F3}", d_blRRead - d_blBalanceValue0));

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数：" + d_blRRead + ",实际重量：" + d_blWeight);

                FADM_Object.Communal._fadmSqlserver.ReviseData(
                    "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_blWeight + " WHERE BottleNum = '" + i_bottleNo + "';");


                if (FADM_Object.Communal._b_isZero)//清零
                    MyModbusFun.ClearBalance();//调用清零 发送指令成功就返回。下面在判断读数

                if (FADM_Object.Communal._b_isZero)
                {
                    while (true)
                    {
                        //判断是否成功清零
                        if (Convert.ToDouble(FADM_Object.Communal._s_balanceValue) == 0.0)
                        {
                            break;
                        }
                        else
                        {
                            //再次发调零
                            MyModbusFun.ClearBalance();//调用清零 发送指令成功就返回。下面在判断读数
                            Thread.Sleep(2000);
                            //Lib_SerialPort.Balance.METTLER.bZeroSign = true;
                        }
                        Thread.Sleep(1);
                    }
                }


                //验证
                int i_adjust = 0;

                if ("小针筒" == Convert.ToString(dt_bottle_details.Rows[0][0]) || "Little Syringe" == Convert.ToString(dt_bottle_details.Rows[0][0]))
                {
                    if (0 < d_blWeight)
                        i_adjust = Convert.ToInt32(Lib_Card.Configure.Parameter.Correcting_S_Pulse / d_blWeight);

                    int iRPulse = 10000 - Convert.ToInt32(i_adjust * Lib_Card.Configure.Parameter.Correcting_S_Weight);
                    if (0 >= iRPulse)
                        iRPulse = 0;
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "验证启动(" + iRPulse + ")");

                    /*index = 0;
                    //往上抽600脉冲
                    MyModbusFun.GetZPosition(ref index);
                    Thread.Sleep(1000);
                    MyModbusFun.Shove(index - 600);
                    Thread.Sleep(1000);*/

                    i_mRes = MyModbusFun.Shove(iRPulse, 0);
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                }
                else
                {
                    if (0 < d_blWeight)
                        i_adjust = Convert.ToInt32(Lib_Card.Configure.Parameter.Correcting_B_Pulse / d_blWeight);
                    int iRPulse = 10000 - Convert.ToInt32(i_adjust * Lib_Card.Configure.Parameter.Correcting_B_Weight);
                    if (0 >= iRPulse)
                        iRPulse = 0;
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "验证启动(" + iRPulse + ")");
                    i_mRes = MyModbusFun.Shove(iRPulse, 1);
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                }

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "验证完成");


                //读取验证重量
                Thread thread = new Thread(() =>
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数启动");


                    double d_blReRead = Communal.SteBalance();
                    double d_blRWeight = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blReRead - d_blRRead)) : Convert.ToDouble(string.Format("{0:F3}", d_blReRead - d_blRRead));

                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数：" + d_blReRead + ",实际重量：" + d_blRWeight);

                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - '" + d_blRWeight + "' WHERE BottleNum = '" + i_bottleNo + "';");
                    double d_blRErr = 0;
                    if ("小针筒" == Convert.ToString(dt_bottle_details.Rows[0][0]) || "Little Syringe" == Convert.ToString(dt_bottle_details.Rows[0][0]))
                    {
                        d_blRErr = Convert.ToDouble(string.Format("{0:F3}", d_blRWeight - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                    }
                    else
                    {
                        d_blRErr = Convert.ToDouble(string.Format("{0:F3}", d_blRWeight - Lib_Card.Configure.Parameter.Correcting_B_Weight));
                    }

                    d_blRErr = d_blRErr < 0 ? -d_blRErr : d_blRErr;

                    if (d_blRErr > Lib_Card.Configure.Parameter.Other_AErr_Correcting)
                    {
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE bottle_details SET AdjustSuccess = 0 WHERE BottleNum = '" + i_bottleNo + "';");
                        i_res = -1;
                    }
                    else
                    {
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE bottle_details SET  LastAdjustWeight = CurrentAdjustWeight, " +
                            "CurrentAdjustWeight =" + d_blWeight + ", " + "AdjustValue = " + i_adjust + ", " +
                            "AdjustSuccess = 1 WHERE BottleNum = '" + i_bottleNo + "';");


                        i_res = 0;
                    }

                });
                thread.Start();
                //移动到母液瓶

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找" + i_bottleNo + "号母液瓶");
                FADM_Object.Communal._i_optBottleNum = i_bottleNo;
                i_mRes = MyModbusFun.TargetMove(0, i_bottleNo, 0);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达" + i_bottleNo + "号母液瓶");

                //放瓶

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", i_bottleNo + "号母液瓶放针启动");

                if ("小针筒" == Convert.ToString(dt_bottle_details.Rows[0][0]) || "Little Syringe" == Convert.ToString(dt_bottle_details.Rows[0][0]))
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
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", i_bottleNo + "号母液瓶放针完成");
                thread.Join();
                //解锁6工位
                MyModbusFun.UnLockSix();
                if (FADM_Object.Communal._b_isZero)
                {
                    MyModbusFun.ResetBalance(); //天平复位
                }
                return i_res;

            }
        }

       
    }
}
