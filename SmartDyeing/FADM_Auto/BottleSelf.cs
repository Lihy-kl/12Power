using Lib_Card;
using SmartDyeing.FADM_Object;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartDyeing.FADM_Auto
{
    /// <summary>
    /// 自检
    /// </summary>
    internal class BottleSelf
    {
        public void Self()
        {
            try
            {

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "自检启动");
                FADM_Object.Communal.WriteMachineStatus(11);
                MyModbusFun.Reset();
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

                //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                //{
                //    Lib_Card.ADT8940A1.Module.Home.Home.Home_XYZFinish = false;
                //}

                //Lib_SerialPort.Balance.METTLER.bReSetSign = true;

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

                    double d_blCurrentWeight = Convert.ToDouble(dataTable.Rows[0][0]);

                    if (d_blCurrentWeight >= Lib_Card.Configure.Parameter.Other_Bottle_MinWeight)
                    {

                        int i_res = SelfProcess(i_bottleNo);
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
                    myAlarm = new FADM_Object.MyAlarm(s_bottleNo + "号母液瓶液量过低，是否继续自检?(继续自检请点是，退出自检请点否)", "自检", true, 1);
                else
                    myAlarm = new FADM_Object.MyAlarm("The liquid level in bottle " + s_bottleNo + " is too low. Do you want to continue the needle inspection? " +
                        "(To continue the needle examination, please click Yes, and to exit the needle examination, please click No)", "SelfChecking", true, 1);

                while (true)
                {
                    if (0 != myAlarm._i_alarm_Choose)
                        break;
                    Thread.Sleep(1);
                }

                if (2 == myAlarm._i_alarm_Choose)
                    goto label1;
                label2:
                foreach (int i in lis_ints)
                {
                    int iRes = SelfProcess(i);
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
                    myAlarm = new FADM_Object.MyAlarm(s_bottleNo + "号母液瓶自检失败，是否继续自检?(继续自检请点是，退出自检请点否)", "自检", true, 1);
                else
                    myAlarm = new FADM_Object.MyAlarm( "The self check of the "+s_bottleNo +" mother liquor bottle has failed. Do you want to continue the self check? (To continue the self-test, click Yes, and to exit the self-test, click No.)", "Self", true, 1);


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
                //int i_mRes = MyModbusFun.TargetMove(3, 0, 1);
                //if (-2 == i_mRes)
                //    throw new Exception("收到退出消息");

                //不回待机位，失能关闭
                MyModbusFun.Power(2);
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达待机位");
                //MyModbusFun.buzzer_On();
                //Thread.Sleep(Convert.ToInt32(Lib_Card.Configure.Parameter.Delay_Buzzer_Finish * 1000));
                //MyModbusFun.buzzer_OFF();

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "自检完成");
                FADM_Object.Communal.WriteMachineStatus(0);
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    new FADM_Object.MyAlarm("自检完成", 1);
                else
                    new FADM_Object.MyAlarm("Self inspection completed", 1);

            }
            catch (Exception ex)
            {

                if ("收到退出消息" == ex.Message)
                {
                    FADM_Object.Communal._b_stop = false;
                    //解锁6工位
                    MyModbusFun.UnLockSix();
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

                        List<string> stringErr = new List<string>();
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
                                     "自检" + "','" +
                                      s_err + "(Test)');";
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                    string s = CardObject.InsertD(s_err, Lib_Card.Configure.Parameter.Other_Language == 0?" 自检": " Self");
                                    if (!stringErr.Contains(s))
                                        stringErr.Add(s);
                                    //while (true)
                                    //{
                                    //    Thread.Sleep(1);
                                    //    if (Lib_Card.CardObject.keyValuePairs[s_sql].Choose != 0)
                                    //        break;

                                    //}

                                    //int _i_alarm_Choose = Lib_Card.CardObject.keyValuePairs[s_sql].Choose;
                                    //CardObject.DeleteD(s_sql);

                                }

                            }
                        }

                        while (true)
                        {
                            for (int p = stringErr.Count - 1; p >= 0; p--)
                            {
                                if (Lib_Card.CardObject.keyValuePairs[stringErr[p]].Choose != 0)
                                {
                                    CardObject.DeleteD(stringErr[p]);
                                    stringErr.Remove(stringErr[p]);
                                }
                            }
                            if (stringErr.Count == 0)
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
                                 "自检" + "','" +
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

                        new FADM_Object.MyAlarm(str, "Self", false, 0);
                    }
                }

                //FADM_Object.Communal.WriteMachineStatus(8);
                //if (Lib_Card.Configure.Parameter.Other_Language == 0)
                //    FADM_Form.CustomMessageBox.Show(ex.Message == "-2" ? strArray[1] : ex.ToString(), "普通自检", MessageBoxButtons.OK, true);
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


        private int SelfProcess(int i_bottleNo)
        {

            int i_res = 0;

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

            //抽液
            DataTable dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(
                "SELECT bottle_details.SyringeType, bottle_details.AdjustValue,assistant_details.UnitOfAccount FROM bottle_details LEFT JOIN assistant_details ON assistant_details.AssistantCode=bottle_details.AssistantCode WHERE bottle_details.BottleNum = '" + i_bottleNo + "';");

            if ("计量泵" == Convert.ToString(dt_temp.Rows[0][0]) || "Metering Pump" == Convert.ToString(dt_temp.Rows[0][0]))
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
                //    FADM_Object.Communal.BalanceState("自检");
                //}

                //移动到母液瓶
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找" + i_bottleNo + "号母液瓶");
                FADM_Object.Communal._i_optBottleNum = i_bottleNo;
                int i_mRes = MyModbusFun.TargetMove(0, i_bottleNo, 1);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达" + i_bottleNo + "号母液瓶");




                int i_pulse = 0;
                List<int> lis_ints = new List<int>();
                foreach (double d in FADM_Object.Communal._da_self)
                {
                    int i_data = Convert.ToInt32(d * Convert.ToDouble(dt_temp.Rows[0]["AdjustValue"]));
                    lis_ints.Add(i_data);
                    i_pulse += i_data;
                }



                if ("小针筒" == Convert.ToString(dt_temp.Rows[0][0]) || "Little Syringe" == Convert.ToString(dt_temp.Rows[0][0]))
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抽液启动(" + i_pulse + ")");
                label2:
                    try
                    {
                        if ("g/l" == Convert.ToString(dt_temp.Rows[0]["UnitOfAccount"]))
                        {
                            int i_totalPulse = i_pulse - Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                            i_mRes = MyModbusFun.Extract(i_totalPulse, true, 0); //排空

                        }
                        else
                        {
                            int i_totalPulse = i_pulse - Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                            i_mRes = MyModbusFun.Extract(i_totalPulse, false, 0); //排空
                        }
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
                            FADM_Object.MyAlarm myAlarm;
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm(i_bottleNo + "号母液瓶未找到针筒，是否继续执行?(继续寻找请点是，退出自检请点否)", "自检", true, 1, true);
                            else
                                myAlarm = new FADM_Object.MyAlarm(i_bottleNo + " bottle did not find a syringe. Do you want to continue? " +
                                    "(To continue searching, please click Yes. To exit the needle test, please click No)", "SelfChecking", true, 1, true);

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
                            FADM_Object.MyAlarm myAlarm;
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm(i_bottleNo + "号母液瓶取针筒时抓手关失败，是否继续执行?(继续寻找请点是，退出自检请点否)", "自检", true, 1, true);
                            else
                                myAlarm = new FADM_Object.MyAlarm(i_bottleNo + " bottle The gripper failed to close when taking out the syringe. Do you want to continue? " +
                                    "(To continue searching, please click Yes. To exit the needle test, please click No)", "SelfChecking", true, 1, true);

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
                            FADM_Object.MyAlarm myAlarm;
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm(i_bottleNo + "号母液瓶取针筒时气缸下失败，是否继续执行?(继续寻找请点是，退出自检请点否)", "自检", true, 1, true);
                            else
                                myAlarm = new FADM_Object.MyAlarm(i_bottleNo + " bottle The cylinder failed to descend when removing the syringe. Do you want to continue? " +
                                    "(To continue searching, please click Yes. To exit the needle test, please click No)", "SelfChecking", true, 1, true);

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
                            throw;

                    }
                }
                else
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抽液启动(" + i_pulse + ")");
                label3:
                    try
                    {
                        if ("g/l" == Convert.ToString(dt_temp.Rows[0]["UnitOfAccount"]))
                        {
                            int i_totalPulse = i_pulse - Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                            i_mRes = MyModbusFun.Extract(i_totalPulse, true, 1); //排空
                        }
                        else
                        {
                            int i_totalPulse = i_pulse - Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                            i_mRes = MyModbusFun.Extract(i_totalPulse, false, 1); //排空
                        }
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

                            FADM_Object.MyAlarm myAlarm;
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm(i_bottleNo + "号母液瓶未找到针筒，是否继续执行?(继续寻找请点是，退出自检请点否)", "自检", true, 1, true);
                            else
                                myAlarm = new FADM_Object.MyAlarm(i_bottleNo + " bottle did not find a syringe. Do you want to continue? " +
                                    "(To continue searching, please click Yes. To exit the needle test, please click No)", "SelfChecking", true, 1, true);

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

                            FADM_Object.MyAlarm myAlarm;
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm(i_bottleNo + "号母液瓶取针筒时抓手关失败，是否继续执行?(继续寻找请点是，退出自检请点否)", "自检", true, 1, true);
                            else
                                myAlarm = new FADM_Object.MyAlarm(i_bottleNo + " bottle The gripper failed to close when taking out the syringe. Do you want to continue? " +
                                    "(To continue searching, please click Yes. To exit the needle test, please click No)", "SelfChecking", true, 1, true);

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

                            FADM_Object.MyAlarm myAlarm;
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm(i_bottleNo + "号母液瓶取针筒时气缸下失败，是否继续执行?(继续寻找请点是，退出自检请点否)", "自检", true, 1, true);
                            else
                                myAlarm = new FADM_Object.MyAlarm(i_bottleNo + " bottle The cylinder failed to descend when removing the syringe. Do you want to continue? " +
                                    "(To continue searching, please click Yes. To exit the needle test, please click No)", "SelfChecking", true, 1, true);

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
                    FADM_Object.Communal.BalanceState("自检");
                }


                i_mRes = MyModbusFun.TargetMove(2, 0, 0);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达天平位");


                /*int index = 0;
                //往上抽600脉冲
                MyModbusFun.GetZPosition(ref index);
                Thread.Sleep(1000);
                MyModbusFun.Shove(index - 600);
                Thread.Sleep(1000);*/


                double d_blBalanceValue0 = 0;
                bool b_selfFail = false;


                for (int i = 0; i < lis_ints.Count - 1; i++)
                {
                    if (i != 0)
                    {
                        //if (FADM_Object.Communal._b_isZero)//清零
                        //    MyModbusFun.ClearBalance();//调用清零 发送指令成功就返回。下面在判断读数
                        //Thread.Sleep(1500);

                        //Lib_SerialPort.Balance.METTLER.bZeroSign = true; ;
                        //Thread.Sleep(200);
                    }
                    if (FADM_Object.Communal._b_isZero)
                    {
                        while (true)
                        {
                            if (0 == Convert.ToDouble(FADM_Object.Communal._s_balanceValue) || i == 0)
                                break;
                            else if (0 != Convert.ToDouble(FADM_Object.Communal._s_balanceValue) && i != 0)
                            {
                                MyModbusFun.ClearBalance();//调用清零 发送指令成功就返回。下面在判断读数
                                Thread.Sleep(2000);
                            }
                            Thread.Sleep(1);
                        }
                    }

                //记录初始天平读数
                la:
                    d_blBalanceValue0 = FADM_Object.Communal.SteBalance();
                    if (d_blBalanceValue0 > 6000)
                        goto la;
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平读数：" + d_blBalanceValue0);
                    //注液
                    i_pulse -= lis_ints[i];



                    if ("小针筒" == Convert.ToString(dt_temp.Rows[0][0]) || "Little Syringe" == Convert.ToString(dt_temp.Rows[0][0]))
                    {
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液启动(" + i_pulse + ")");
                        i_mRes = MyModbusFun.Shove(i_pulse, 0);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");
                    }
                    else
                    {
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液启动(" + i_pulse + ")");
                        i_mRes = MyModbusFun.Shove(i_pulse, 1);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");
                    }

                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液完成");

                    /*Thread.Sleep(1000);
                    //在往上走100 脉冲。 把外面的一滴吸回去
                    i_mRes = MyModbusFun.Shove(-i_pulse-600);*/

                    if (i < lis_ints.Count - 2)
                    {
                        //读取天平数据
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数启动");
                        double d_blRRead = FADM_Object.Communal.SteBalance();
                        double d_blWeight = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blRRead - d_blBalanceValue0)) : Convert.ToDouble(string.Format("{0:F3}", d_blRRead - d_blBalanceValue0));
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数：" + d_blRRead + ",实际重量：" + d_blWeight);

                        if (i > 0)
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_blWeight + ", " +
                            "SelfChecking" + (i) + " = " + string.Format("{0:F3}", d_blWeight) + " WHERE BottleNum = '" + i_bottleNo + "';");

                            double d_blRErr = Convert.ToDouble(string.Format("{0:F3}", d_blWeight - FADM_Object.Communal._da_self[i]));
                            d_blRErr = d_blRErr < 0 ? -d_blRErr : d_blRErr;
                            if (d_blRErr > Lib_Card.Configure.Parameter.Other_AErr_Drip)
                                b_selfFail = true;
                        }
                    }
                }

                //读取验证重量
                Thread thread = new Thread(() =>
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数启动");
                    double d_blRRead = FADM_Object.Communal.SteBalance();
                    double d_blWeight = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blRRead - d_blBalanceValue0)) : Convert.ToDouble(string.Format("{0:F3}", d_blRRead - d_blBalanceValue0));
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数：" + d_blRRead + ",实际重量：" + d_blWeight);
                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - '" + d_blWeight + "', " +
                        "SelfChecking4 = " + string.Format("{0:F3}", d_blWeight) + "  WHERE BottleNum = '" + i_bottleNo + "';");

                    double d_blRErr = Convert.ToDouble(string.Format("{0:F3}", d_blWeight - FADM_Object.Communal._da_self[4]));
                    d_blRErr = d_blRErr < 0 ? -d_blRErr : d_blRErr;

                    if (d_blRErr > Lib_Card.Configure.Parameter.Other_AErr_Drip || b_selfFail)
                    {
                        i_res = 1;
                    }
                    else
                    {
                        i_res = 0;
                    }

                });
                thread.Start();
                //移动到母液瓶
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找" + i_bottleNo + "号母液瓶");
                i_mRes = MyModbusFun.TargetMove(0, i_bottleNo, 0);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达" + i_bottleNo + "号母液瓶");

                //放瓶
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", i_bottleNo + "号母液瓶放针启动");

                if ("小针筒" == Convert.ToString(dt_temp.Rows[0][0]) || "Little Syringe" == Convert.ToString(dt_temp.Rows[0][0]))
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

                //更新自检记录到自检表
                string s_sql = "SELECT  SelfChecking1,SelfChecking2,SelfChecking3,SelfChecking4,AdjustValue,CurrentAdjustWeight FROM  bottle_details where BottleNum = '" + i_bottleNo + "';";
                DataTable dt_bottle_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                FADM_Object.Communal._fadmSqlserver.ReviseData("insert into self_table(Date,SelfChecking1,SelfChecking2,SelfChecking3,SelfChecking4,AdjustValue,CurrentAdjustWeight,BottleNum) values ('"
                    + System.DateTime.Now.ToString() + "','" + dt_bottle_details.Rows[0][0].ToString() + "','" + dt_bottle_details.Rows[0][1].ToString() + "','" + dt_bottle_details.Rows[0][2].ToString() + "','" + dt_bottle_details.Rows[0][3].ToString() + "','" + dt_bottle_details.Rows[0][4].ToString() + "','" + dt_bottle_details.Rows[0][5].ToString() + "'," + i_bottleNo.ToString() + ")");

                if (FADM_Object.Communal._b_isZero)
                {
                    MyModbusFun.ResetBalance(); //天平复位
                }

                //Lib_SerialPort.Balance.METTLER.bReSetSign = true;
                //Thread.Sleep(500);

                return i_res;

            }
        }
    }
}
