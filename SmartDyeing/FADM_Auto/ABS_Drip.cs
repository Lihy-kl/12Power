
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
    internal class ABS_Drip
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
                if(lis_iUse.Count==0)
                {
                    break;
                }

                Thread.Sleep(1000);
            }
        }


        public void DripLiquid(object o_BatchName)
        {
            try
            {

                FADM_Object.Communal.WriteDripWait(false);
                _b_dripErr = false;
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", o_BatchName + "滴液启动");
                FADM_Object.Communal._fadmSqlserver.ReviseData(
                    "UPDATE abs_drop_head SET StartTime = '" + DateTime.Now + "' WHERE BatchName = '" + o_BatchName + "';");
                FADM_Object.Communal.WriteMachineStatus(7);
                int i_mRes = 0;



                MyModbusFun.SetBatchStart();

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


                //if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Count > 0)
                {
                    //先检查后处理杯位历史状态，如果要洗杯，要等全部前洗杯完成后才能进行滴液
                    

                    DataTable dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
                       "SELECT * FROM abs_drop_head WHERE BatchName = '" + o_BatchName + "' AND CupFinish = 0    ORDER BY CupNum;");
                    FADM_Object.Communal._b_use[0] = false;
                    FADM_Object.Communal._b_use[1] = false;
                    FADM_Object.Communal._b_use[2] = false;
                    FADM_Object.Communal._b_use[3] = false;
                    foreach (DataRow dataRow in dt_drop_head.Rows)
                    {
                        int i_cupNum = Convert.ToInt16(dataRow["CupNum"]);
                        FADM_Object.Communal._b_use[i_cupNum-1] = true;
                        //写入当前杯号的配方代码和染固色工艺
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE abs_cup_details SET  Statues = '检查待机状态'  WHERE CupNum = " + i_cupNum + ";");
                    }



                label1:
                    foreach (DataRow dataRow in dt_drop_head.Rows)
                    {
                        int i_cupNum = Convert.ToInt16(dataRow["CupNum"]);
                        //等待杯子进入待机状态
                        int i_state = Convert.ToInt16(MyAbsorbance._abs_Temps[i_cupNum - 1]._s_currentState);
                        if (1 != i_state)
                        {
                            goto label1;
                        }
                    }
                    //洗杯杯号
                    List<int> lis_iWash = new List<int>();
                    //判断杯子历史状态
                    List<int> lis_iUse = new List<int>();
                    foreach (DataRow dataRow in dt_drop_head.Rows)
                    {
                        int i_cupNum = Convert.ToInt16(dataRow["CupNum"]);
                        int i_state = Convert.ToInt16(MyAbsorbance._abs_Temps[i_cupNum - 1]._s_history); ;
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                             "UPDATE abs_cup_details SET Statues = '检查历史状态' WHERE CupNum = " + i_cupNum + ";");

                        if (0 != i_state)
                        {
                            if (!lis_iWash.Contains(i_cupNum))
                                lis_iWash.Add(i_cupNum);
                            //发送洗杯
                            if (FADM_Object.Communal._b_absErr)
                            {
                                new FADM_Object.MyAlarm( "吸光度机通讯异常，异常退出", "温馨提示");
                                throw new Exception("收到退出消息");
                            }


                            //等待没有交互时再发送停止
                            while (true)
                            {
                                if (MyAbsorbance._abs_Temps[i_cupNum - 1]._s_request == "0")
                                {
                                    break;
                                }
                            }

                            //先发一个停止，再发一个洗杯
                            int[] values1 = new int[1];
                            values1[0] = 2;
                            if (i_cupNum == 1)
                                FADM_Object.Communal._tcpModBusAbs.Write(13088, values1);
                            else if (i_cupNum == 2)
                                FADM_Object.Communal._tcpModBusAbs.Write(13098, values1);
                            else if (i_cupNum == 3)
                                FADM_Object.Communal._tcpModBusAbs.Write(13108, values1);
                            else if (i_cupNum == 4)
                                FADM_Object.Communal._tcpModBusAbs.Write(13118, values1);

                            //判断待机后再发洗杯
                            while (true)
                            {
                                if (MyAbsorbance._abs_Temps[i_cupNum - 1]._s_currentState == "1")
                                    break;
                            }

                            

                            string s_sql1 = "UPDATE abs_cup_details SET Statues='洗杯',IsUsing = 1,Type=0  WHERE CupNum = " + i_cupNum + " ;";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql1);

                            SmartDyeing.FADM_Auto.MyAbsorbance.Generate(2, i_cupNum);
                            SmartDyeing.FADM_Auto.MyAbsorbance.SendData(i_cupNum);

                            //释放机械手
                            FADM_Object.Communal.WriteDripWait(true);

                            Thread.Sleep(1000);
                        }
                    }
                    


                    //等待洗杯完成
                    
                    while(true)
                    {
                        if (lis_iWash.Contains(1))
                        {
                            if (MyAbsorbance._abs_Temps[0]._s_currentState == "1")
                            {
                                //判断是否整个洗杯流程完成
                                string s_sql = "SELECT *  FROM Abs_details where CupNum = " + 1 + " And Finish = 0 order by StepNum;";

                                DataTable dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                                if (dt_temp.Rows.Count == 0)
                                    lis_iWash.Remove(1);
                            }
                        }
                        else if (lis_iWash.Contains(3))
                        {
                            if (MyAbsorbance._abs_Temps[2]._s_currentState == "1")
                            {
                                //判断是否整个洗杯流程完成
                                string s_sql = "SELECT *  FROM Abs_details where CupNum = " + 3 + " And Finish = 0 order by StepNum;";

                                DataTable dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                                if (dt_temp.Rows.Count == 0)
                                    lis_iWash.Remove(3);
                            }
                        }
                        if (lis_iWash.Count == 0)
                            break;
                        Thread.Sleep(1000);
                    }
                    


                    //写入滴液状态
                    foreach (DataRow dataRow in dt_drop_head.Rows)
                    {
                        int i_cupNum = Convert.ToInt16(dataRow["CupNum"]);
                        //配液
                        SmartDyeing.FADM_Auto.MyAbsorbance.Generate(3, i_cupNum);
                        SmartDyeing.FADM_Auto.MyAbsorbance.SendData(i_cupNum);
                    }

                    if (FADM_Object.Communal._b_use[0])
                    {
                        //等待准备状态
                        while (true)
                        {
                            //判断两个杯是否都可以注液
                            if (MyAbsorbance._abs_Temps[0]._s_request == "4")
                                break;

                            Thread.Sleep(1000);
                        }
                    }
                    if (FADM_Object.Communal._b_use[2])
                    {
                        //等待准备状态
                        while (true)
                        {
                            //判断两个杯是否都可以注液
                            if (MyAbsorbance._abs_Temps[2]._s_request == "4")
                                break;

                            Thread.Sleep(1000);
                        }
                    }

                    //FADM_Object.Communal._fadmSqlserver.ReviseData(
                    //         "UPDATE abs_cup_details SET Statues = '运行中' WHERE CupNum = " + 1 + ";");

                }

                //回零               
                string s_homeErr = "";

                MyModbusFun.Reset();


                this.DripProcess(o_BatchName);



                //回到停止位
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找待机位");
                FADM_Object.Communal._i_optBottleNum = 0;
                FADM_Object.Communal._i_OptCupNum = 0;
                if (Lib_Card.Configure.Parameter.Other_IsOnlyDrip == 1)
                {
                    i_mRes = MyModbusFun.TargetMove(3, 0, 1);
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                }
                else
                {
                    //不回待机位，失能关闭
                    MyModbusFun.Power(2);
                }
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达待机位");
                MyModbusFun.SetBatchClose(); //设置关闭批次


               

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", o_BatchName + "批次滴液完成");
                FADM_Object.Communal.WriteMachineStatus(0);
                _i_dripType = 0;

                if (FADM_Object.Communal._b_use[0])
                {
                    //插入到虚拟瓶号中，瓶号为888，插入助剂编号为ABS888
                    FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from assistant_details where AssistantCode = 'ABS888'");

                    List<string> lis_data = new List<string>();
                    lis_data.Add("ABS888");
                    lis_data.Add(o_BatchName.ToString());//批次号保存进去
                    lis_data.Add("1");//杯号保存进去
                    lis_data.Add("其他染料");
                    lis_data.Add("%");
                    lis_data.Add("0");
                    lis_data.Add("0");
                    lis_data.Add("24");
                    lis_data.Add("0");
                    lis_data.Add("0");
                    //如果是新增
                    string s_sql = "INSERT INTO assistant_details" +
                                       " (AssistantCode, AssistantBarCode, AssistantName, AssistantType, UnitOfAccount," +
                                       " AllowMinColoringConcentration, AllowMaxColoringConcentration, TermOfValidity," +
                                       " Intensity, Cost) VALUES('" + lis_data[0] + "','" + lis_data[1] + "','" + lis_data[2] + "'," +
                                       "'" + lis_data[3] + "','" + lis_data[4] + "','" + lis_data[5] + "','" + lis_data[6] + "'," +
                                       "'" + lis_data[7] + "','" + lis_data[8] + "','" + lis_data[9] + "');";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                    lis_data.Clear();
                    lis_data.Add("888");
                    lis_data.Add("ABS888");
                    lis_data.Add(FADM_Object.Communal._d_abs_mixture.ToString("f6"));//设定浓度
                    lis_data.Add("500");//当前液量
                    lis_data.Add("小针筒");//针筒类型
                    lis_data.Add("0.1");//当前液量
                    lis_data.Add("其他");//开料流程
                    lis_data.Add("0");//原瓶号
                    lis_data.Add("1000");//
                    lis_data.Add(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));//开料日期
                    lis_data.Add(FADM_Object.Communal._d_abs_mixture.ToString("f6"));//实际浓度
                    FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from bottle_details where BottleNum = 888");

                    //如果是新增
                    s_sql = "INSERT INTO bottle_details (" +
                                " BottleNum, AssistantCode, SettingConcentration," +
                                " CurrentWeight, SyringeType, DropMinWeight," +
                                " BrewingCode, OriginalBottleNum, AllowMaxWeight," +
                                " BrewingData, RealConcentration) VALUES( '" + lis_data[0] + "'," +
                                " '" + lis_data[1] + "','" + lis_data[2] + "'," +
                                " '" + lis_data[3] + "','" + lis_data[4] + "'," +
                                " '" + lis_data[5] + "','" + lis_data[6] + "'," +
                                " '" + lis_data[7] + "','" + lis_data[8] + "'," +
                                "'" + lis_data[9] + "','" + lis_data[10] + "');";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "INSERT INTO abs_wait_list(BottleNum, InsertDate,Type) VALUES('" + 888 + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',0);");

                    //插入到ABS测量等待列表第一条记录


                    //复位请求
                    int[] values = new int[1];
                    values[0] = 0;
                    if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                    {
                        FADM_Object.Communal._tcpModBusAbs.ReConnect();
                    }

                    FADM_Object.Communal._tcpModBusAbs.Write(13189, values);

                    //复位加药完成
                    values[0] = 2;
                    if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                    {
                        FADM_Object.Communal._tcpModBusAbs.ReConnect();
                    }
                    FADM_Object.Communal._tcpModBusAbs.Write(13089, values);

                    //Lib_SerialPort.Balance.METTLER.bReSetSign = true;
                }
                if (FADM_Object.Communal._b_use[2])
                {
                    //插入到虚拟瓶号中，瓶号为999，插入助剂编号为ABS999
                    FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from assistant_details where AssistantCode = 'ABS999'");

                    List<string> lis_data = new List<string>();
                    lis_data.Add("ABS999");
                    lis_data.Add(o_BatchName.ToString());//批次号保存进去
                    lis_data.Add("3");//杯号保存进去
                    lis_data.Add("其他染料");
                    lis_data.Add("%");
                    lis_data.Add("0");
                    lis_data.Add("0");
                    lis_data.Add("24");
                    lis_data.Add("0");
                    lis_data.Add("0");
                    //如果是新增
                    string s_sql = "INSERT INTO assistant_details" +
                                       " (AssistantCode, AssistantBarCode, AssistantName, AssistantType, UnitOfAccount," +
                                       " AllowMinColoringConcentration, AllowMaxColoringConcentration, TermOfValidity," +
                                       " Intensity, Cost) VALUES('" + lis_data[0] + "','" + lis_data[1] + "','" + lis_data[2] + "'," +
                                       "'" + lis_data[3] + "','" + lis_data[4] + "','" + lis_data[5] + "','" + lis_data[6] + "'," +
                                       "'" + lis_data[7] + "','" + lis_data[8] + "','" + lis_data[9] + "');";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                    lis_data.Clear();
                    lis_data.Add("999");
                    lis_data.Add("ABS999");
                    lis_data.Add(FADM_Object.Communal._d_abs_mixture_2.ToString("f6"));//设定浓度
                    lis_data.Add("500");//当前液量
                    lis_data.Add("小针筒");//针筒类型
                    lis_data.Add("0.1");//当前液量
                    lis_data.Add("其他");//开料流程
                    lis_data.Add("0");//原瓶号
                    lis_data.Add("1000");//
                    lis_data.Add(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));//开料日期
                    lis_data.Add(FADM_Object.Communal._d_abs_mixture_2.ToString("f6"));//实际浓度
                    FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from bottle_details where BottleNum = 999");

                    //如果是新增
                    s_sql = "INSERT INTO bottle_details (" +
                                " BottleNum, AssistantCode, SettingConcentration," +
                                " CurrentWeight, SyringeType, DropMinWeight," +
                                " BrewingCode, OriginalBottleNum, AllowMaxWeight," +
                                " BrewingData, RealConcentration) VALUES( '" + lis_data[0] + "'," +
                                " '" + lis_data[1] + "','" + lis_data[2] + "'," +
                                " '" + lis_data[3] + "','" + lis_data[4] + "'," +
                                " '" + lis_data[5] + "','" + lis_data[6] + "'," +
                                " '" + lis_data[7] + "','" + lis_data[8] + "'," +
                                "'" + lis_data[9] + "','" + lis_data[10] + "');";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "INSERT INTO abs_wait_list(BottleNum, InsertDate,Type) VALUES('" + 999 + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',0);");

                    //插入到ABS测量等待列表第一条记录


                    //复位请求
                    int[] values = new int[1];
                    values[0] = 0;
                    if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                    {
                        FADM_Object.Communal._tcpModBusAbs.ReConnect();
                    }

                    FADM_Object.Communal._tcpModBusAbs.Write(13209, values);

                    //复位加药完成
                    values[0] = 2;
                    if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                    {
                        FADM_Object.Communal._tcpModBusAbs.ReConnect();
                    }
                    FADM_Object.Communal._tcpModBusAbs.Write(13109, values);

                    //Lib_SerialPort.Balance.METTLER.bReSetSign = true;
                }

                _b_dripStop = false;

                new FADM_Object.MyAlarm("批次滴液完成", 1);

                SmartDyeing.FADM_Control.ABS_Formula.P_bl_update = true;
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

                    if (FADM_Auto.ABS_Drip._b_dripErr)
                    {
                        FADM_Auto.Reset.MoveData(o_BatchName.ToString());
                    }

                    SmartDyeing.FADM_Control.ABS_Formula.P_bl_update = true;
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
                            if ( SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.ContainsKey(ia_errArray[i]))
                            {
                                string s_err = SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew[ia_errArray[i]];
                                string s_sql = "INSERT INTO alarm_table" +
                                 "(MyDate,MyTime,AlarmHead,AlarmDetails)" +
                                 " VALUES( '" +
                                 String.Format("{0:d}", DateTime.Now) + "','" +
                                 String.Format("{0:T}", DateTime.Now) + "','" +
                                 "滴液" + "','" +
                                  s_err  + "(Test)');";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                string s_insert = Lib_Card.CardObject.InsertD(s_err, Lib_Card.Configure.Parameter.Other_Language == 0 ? " 滴液":"Drip");
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
                        for(int p= lis_err.Count-1;p>=0;p--)
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

                    new FADM_Object.MyAlarm(ex.Message == "-2" ? sa_array[1] : ex.ToString(), "Drip", false, 0);
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

        private void DripProcess(object oBatchName)
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
               "SELECT * FROM abs_drop_head WHERE   BatchName = '" + oBatchName + "' And Step = 1 order by CupNum;");

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
            string s_sql = "SELECT * FROM abs_drop_head WHERE BatchName = '" + oBatchName + "' And CupFinish = 0  ORDER BY CupNum;";
            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

            foreach (DataRow row in dt_drop_head.Rows)
            {
                s_sql = "SELECT * FROM abs_drop_details WHERE BatchName = '" + oBatchName + "' AND " +
                "CupNum =" + row["CupNum"].ToString()+ " And BottleNum <= " + Lib_Card.Configure.Parameter.Machine_Bottle_Total + ";";
                DataTable dt_drop_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                if(dt_drop_details.Rows.Count ==0  && row["AddWaterChoose"].ToString() !="1")
                {
                    int i_cup = Convert.ToInt32(row["CupNum"].ToString());

                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                       "UPDATE abs_drop_head SET DescribeChar = '滴液成功', FinishTime = '" + DateTime.Now + "', Step = 2,CupFinish = 1  " +
                       "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + i_cup + ";");

                    //FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                }
            }

            //加水
             s_sql = "SELECT * FROM abs_drop_head WHERE BatchName = '" + oBatchName + "' AND " +
                "AddWaterChoose = 1 AND AddWaterFinish = 0  ORDER BY CupNum;";
            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);



            if (dt_drop_head.Rows.Count > 0)
            {
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
                    //如果前洗杯并且没洗杯完成就不加水
                    if (_lis_ForwordwashCup.Contains(i_cupNo))
                    {
                        if(!_lis_addwashCupFinish.Contains(i_cupNo))
                        {
                            continue;
                        }
                    }
                    //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                    //{
                    //    //富士伺服在下面判断 天平状态 原有不动 绿维的放在上面 并且置位 是否回原点 绿维的放移动机械手前面

                    //    //判断是否异常
                    //    FADM_Object.Communal.BalanceState("滴液");
                    //}

                    double d_blObjectW = Convert.ToDouble(row["ObjectAddWaterWeight"]);
                    if (d_blObjectW > 0)
                    {

                        //把实际加水杯号记录
                        lis_actualAddWaterCup.Add(i_cupNo);

                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找" + i_cupNo + "号吸光度配液杯");
                        int i_reSuccess2 = MyModbusFun.TargetMove(10, i_cupNo, 1);
                        if (-2 == i_reSuccess2)
                            throw new Exception("收到退出消息");
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达" + i_cupNo + "号吸光度配液杯");

                        if (_b_dripStop)
                        {
                            FADM_Object.Communal._b_stop = true;
                        }
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand",  i_cupNo + "号吸光度配液杯加水启动");
                        double d_addWaterTime = MyModbusFun.GetWaterTime(d_blObjectW);//加水时间

                        FADM_Object.Communal.AddWater(d_addWaterTime);
                        
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", i_cupNo + "号吸光度配液杯加水完成");
                    }
                    //如果勾选加水，但实际计算出来加水量为0，直接完成
                    else
                    {
                        s_sql = "UPDATE abs_drop_head SET AddWaterFinish = 1 WHERE " +
                    "BatchName = '" + oBatchName + "' AND  CupNum = " + i_cupNo + ";";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                        DataTable dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM abs_drop_details WHERE BatchName = '" + oBatchName.ToString() + "' AND Finish = 0" + " AND CupNum = " + i_cupNo + ";");
                        //查询一下加水没完成的也不置为完成
                        DataTable dt_drop_details4 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM abs_drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND AddWaterChoose = 1 AND AddWaterFinish =0" + " AND CupNum = " + i_cupNo + ";");
                        if (dt_drop_details3.Rows.Count == 0 && dt_drop_details4.Rows.Count == 0)
                        {
                            //置为完成
                            s_sql = "UPDATE abs_drop_head SET CupFinish = 1 WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cupNo + "; ";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                            bool b_fail = true;

                            s_sql = "SELECT abs_drop_details.CupNum as CupNum, " +
                                        "abs_drop_details.BottleNum as BottleNum, " +
                                        "abs_drop_details.ObjectDropWeight as ObjectDropWeight, " +
                                        "abs_drop_details.RealDropWeight as RealDropWeight, " +
                                        "bottle_details.SyringeType as SyringeType " +
                                        "FROM abs_drop_details left join bottle_details on " +
                                        "bottle_details.BottleNum = abs_drop_details.BottleNum " +
                                        "WHERE abs_drop_details.BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cupNo + ";";
                            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                            foreach (DataRow dr in dt_drop_head.Rows)
                            {
                                double d_blRealErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"]))) : Convert.ToDouble(string.Format("{0:F3}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])));
                                d_blRealErr = d_blRealErr < 0 ? -d_blRealErr : d_blRealErr;
                                if (d_blRealErr > Lib_Card.Configure.Parameter.Other_AErr_Drip)
                                    b_fail = false;
                            }

                            dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                    "SELECT * FROM abs_drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cupNo + ";");
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
                                    //if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(i_cupNo))
                                    //{
                                    //    FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                                    //}
                                  //  FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  //"UPDATE cup_details SET Statues = '滴液成功' WHERE CupNum = " + i_cup + ";");
                                }
                                else
                                {
                                    s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                            ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    //FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    //"UPDATE cup_details SET Statues = '滴液失败' WHERE CupNum = " + i_cup + ";");
                                }
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE abs_drop_head SET DescribeChar = '" + s_describe + "',DescribeChar_EN = '" + s_describe_EN + "', FinishTime = '" + DateTime.Now + "', Step = 2 " +
                               "WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cup + ";");
                            }
                        }
                    }
                }
                if (lis_actualAddWaterCup.Count > 0)
                {
                    //移动到天平位

                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找天平位");

                    //if ((Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 0)|| Lib_Card.Configure.Parameter.Machine_Type == 1)
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

                    double d_blBalanceValue0 = SteBalance();
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平读数：" + d_blBalanceValue0);

                    double d_addWaterTime = MyModbusFun.GetWaterTime(Lib_Card.Configure.Parameter.Correcting_Water_RWeight);//加水时间 校正加水时间

                    FADM_Object.Communal.AddWater(d_addWaterTime);

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

                    s_sql = "SELECT * FROM abs_drop_head WHERE BatchName = '" + oBatchName + "' AND ObjectAddWaterWeight != 0  And AddWaterFinish = 0;";
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
                        Double d_objAddWaiterWeight = Convert.ToDouble(dataRow["ObjectAddWaterWeight"].ToString()) * d_blDif_span;
                        //if (d_blWeight == 0)
                        //{
                        //    FADM_Object.Communal._fadmSqlserver.ReviseData(
                        //                          "UPDATE cup_details SET TotalWeight =  " + string.Format("{0:F3}", 0) + " WHERE CupNum = " + dataRow["CupNum"] + " ;");
                        //}
                        //else
                        //{
                        //    FADM_Object.Communal._fadmSqlserver.ReviseData(
                        //  "UPDATE cup_details SET TotalWeight =  " + string.Format("{0:F3}", d_objAddWaiterWeight) + " WHERE CupNum = " + dataRow["CupNum"] + " ;");
                        //}

                        //if (Convert.ToDouble(dataRow["ObjectAddWaterWeight"].ToString()) > 20)
                        //{
                        //    if (System.Math.Abs(d_blWE * 100 / (Convert.ToDouble(dataRow["TotalWeight"].ToString()))) > Lib_Card.Configure.Parameter.Other_AErr_DripWater || d_blWeight == 0)
                        //    {
                        //        b_fail = true;
                        //        s_failC += dataRow["CupNum"].ToString() + ",";
                        //    }
                        //}
                        //else
                        //{
                        //    if (System.Math.Abs(Convert.ToDouble(dataRow["ObjectAddWaterWeight"].ToString()) * d_blDif * 100) / (Convert.ToDouble(dataRow["TotalWeight"].ToString())) > Lib_Card.Configure.Parameter.Other_AErr_DripWater || d_blWeight == 0)
                        //    {
                        //        b_fail = true;
                        //        s_failC += dataRow["CupNum"].ToString() + ",";
                        //    }
                        //}

                        if(System.Math.Abs(d_blDif) *100 > Lib_Card.Configure.Parameter.Other_AErr_DripWater)
                        {
                            b_fail = true;
                            s_failC += dataRow["CupNum"].ToString() + ",";
                        }
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
                        s_sql = "UPDATE abs_drop_head SET RealAddWaterWeight = 0, AddWaterFinish = 1 WHERE " +
                        "BatchName = '" + oBatchName + "' AND AddWaterChoose = 1 AND CupFinish = 0   AND ObjectAddWaterWeight > 0 AND ObjectAddWaterWeight <= 20 And AddWaterFinish = 0 And CupNum in (" + s_cupAddWater + ");";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                    }
                    else
                    {
                        s_sql = "UPDATE abs_drop_head SET RealAddWaterWeight = (ObjectAddWaterWeight * " + d_blDif_span + "), AddWaterFinish = 1 WHERE " +
                        "BatchName = '" + oBatchName + "' AND AddWaterChoose = 1 AND CupFinish = 0   AND ObjectAddWaterWeight > 0 AND ObjectAddWaterWeight <= 20  And AddWaterFinish = 0 And CupNum in (" + s_cupAddWater + ");";

                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                    }

                    //复检天平重量为0时，实际加水量为0
                    if (d_blWeight == 0)
                    {
                        s_sql = "UPDATE abs_drop_head SET RealAddWaterWeight = 0, AddWaterFinish = 1 WHERE " +
                        "BatchName = '" + oBatchName + "' AND AddWaterChoose = 1 AND CupFinish = 0   AND ObjectAddWaterWeight > 20  And AddWaterFinish = 0 And CupNum in (" + s_cupAddWater + ");";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                    }
                    else
                    {
                        s_sql = "UPDATE abs_drop_head SET RealAddWaterWeight = (ObjectAddWaterWeight * " + d_blDif_span + "), AddWaterFinish = 1 WHERE " +
                        "BatchName = '" + oBatchName + "' AND AddWaterChoose = 1 AND CupFinish = 0   AND ObjectAddWaterWeight > 20  And AddWaterFinish = 0 And CupNum in (" + s_cupAddWater + ");";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
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
                        {
                            if (FADM_Object.Communal._b_use[0])
                            {
                                Thread thread33 = new Thread(() =>
                            {
                                //等待没有交互时再发送停止
                                //while (true)
                                //{
                                //    if (MyAbsorbance._abs_Temps[Convert.ToInt32(_cup.NO) - 1]._s_request == "0")
                                //    {
                                //        break;
                                //    }
                                //}

                                //先发一个停止，再发一个洗杯
                                int[] values1 = new int[1];
                                values1[0] = 2;
                                FADM_Object.Communal._tcpModBusAbs.Write(13088, values1);

                                //判断待机后再发洗杯
                                while (true)
                                {
                                    if (MyAbsorbance._abs_Temps[0]._s_currentState == "1")
                                        break;
                                }

                                s_sql = "UPDATE abs_cup_details SET Statues='洗杯',IsUsing = 1,Type=0  WHERE CupNum = " + 1 + " ;";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                SmartDyeing.FADM_Auto.MyAbsorbance.Generate(2, 1);
                                SmartDyeing.FADM_Auto.MyAbsorbance.SendData(1);




                            });
                                thread33.Start();
                            }
                            if (FADM_Object.Communal._b_use[2])
                            {
                                Thread thread33 = new Thread(() =>
                                {
                                    //等待没有交互时再发送停止
                                    //while (true)
                                    //{
                                    //    if (MyAbsorbance._abs_Temps[Convert.ToInt32(_cup.NO) - 1]._s_request == "0")
                                    //    {
                                    //        break;
                                    //    }
                                    //}

                                    //先发一个停止，再发一个洗杯
                                    int[] values1 = new int[1];
                                    values1[0] = 2;
                                    FADM_Object.Communal._tcpModBusAbs.Write(13108, values1);

                                    //判断待机后再发洗杯
                                    while (true)
                                    {
                                        if (MyAbsorbance._abs_Temps[2]._s_currentState == "1")
                                            break;
                                    }

                                    s_sql = "UPDATE abs_cup_details SET Statues='洗杯',IsUsing = 1,Type=0  WHERE CupNum = " + 3 + " ;";
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                    SmartDyeing.FADM_Auto.MyAbsorbance.Generate(2, 3);
                                    SmartDyeing.FADM_Auto.MyAbsorbance.SendData(3);




                                });
                                thread33.Start();
                            }
                            throw new Exception("收到退出消息");
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
                    }

                    //当加水在最后时，要重新判断一下
                    if (FADM_Object.Communal._b_isFinishSend)
                    {
                        foreach (int ic in lis_actualAddWaterCup)
                        {


                            DataTable dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                                    "SELECT * FROM abs_drop_details WHERE BatchName = '" + oBatchName.ToString() + "' AND Finish = 0" + " AND CupNum = " + ic + ";");
                            //查询一下加水没完成的也不置为完成
                            DataTable dt_drop_details4 = FADM_Object.Communal._fadmSqlserver.GetData(
                                    "SELECT * FROM abs_drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND AddWaterChoose = 1 AND AddWaterFinish =0" + " AND CupNum = " + ic + ";");
                            if (dt_drop_details3.Rows.Count == 0 && dt_drop_details4.Rows.Count == 0)
                            {
                                //置为完成
                                s_sql = "UPDATE abs_drop_head SET CupFinish = 1 WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + ic + "; ";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);


                                bool b_fail1 = true;

                                s_sql = "SELECT abs_drop_details.CupNum as CupNum, " +
                                            "abs_drop_details.BottleNum as BottleNum, " +
                                            "abs_drop_details.ObjectDropWeight as ObjectDropWeight, " +
                                            "abs_drop_details.RealDropWeight as RealDropWeight, " +
                                            "bottle_details.SyringeType as SyringeType " +
                                            "FROM abs_drop_details left join bottle_details on " +
                                            "bottle_details.BottleNum = abs_drop_details.BottleNum " +
                                            "WHERE abs_drop_details.BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + ic + ";";
                                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                                foreach (DataRow dr in dt_drop_head.Rows)
                                {
                                    double d_blRealErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                                    Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"]))) : Convert.ToDouble(string.Format("{0:F3}",
                                    Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])));
                                    d_blRealErr = d_blRealErr < 0 ? -d_blRealErr : d_blRealErr;
                                    if (d_blRealErr > Lib_Card.Configure.Parameter.Other_AErr_Drip)
                                        b_fail1 = false;
                                }

                                dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                        "SELECT * FROM abs_drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + ic + ";");
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
                                        //if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(ic))
                                        //{
                                        //    FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                                        //}
                                      //  FADM_Object.Communal._fadmSqlserver.ReviseData(
                                      //"UPDATE cup_details SET Statues = '滴液成功' WHERE CupNum = " + i_cup + ";");
                                    }
                                    else
                                    {
                                        s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                        s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                         ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                        //FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        //"UPDATE cup_details SET Statues = '滴液失败' WHERE CupNum = " + i_cup + ";");
                                    }
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE abs_drop_head SET DescribeChar = '" + s_describe + "',DescribeChar_EN = '" + s_describe_EN + "', FinishTime = '" + DateTime.Now + "', Step = 2 " +
                                   "WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cup + ";");
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
            s_sql = "SELECT * FROM abs_drop_details WHERE BatchName = '" + oBatchName + "' AND " +
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
            s_sql = "SELECT * FROM abs_drop_details WHERE BatchName = '" + oBatchName + "' AND CupNum = " + i_minCupNo + " AND " +
                "Finish = 0 AND UnitOfAccount = '" + s_unitA + "' AND MinWeight = " + i_lowSrart + " AND " +
                "BottleNum <= " + Lib_Card.Configure.Parameter.Machine_Bottle_Total + "   And IsDrop != 0 ORDER BY BottleNum;";
            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (0 == dt_drop_head.Rows.Count)
            {
                //当前杯已加完
                goto label3;
            }

            int i_minBottleNo = Convert.ToInt32(dt_drop_head.Rows[0]["BottleNum"]);

            s_sql = "SELECT * FROM abs_drop_details WHERE BatchName = '" + oBatchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
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
            DateTime dateTime = dt_bottle_details.Rows[0]["LastUseTime"] is DBNull ? DateTime.Now :
                Convert.ToDateTime(dt_bottle_details.Rows[0]["LastUseTime"]);
            double weight = Convert.ToDouble(dt_bottle_details.Rows[0]["CurrentWeight"]);
            bool b_lCheckSuccess = (Convert.ToString(dt_bottle_details.Rows[0]["AdjustSuccess"]) == "1");
            string s_syringeType = Convert.ToString(dt_bottle_details.Rows[0]["SyringeType"]);
            string s_dripReserveFirst = "0";
            if(!(dt_bottle_details.Rows[0]["DripReserveFirst"] is DBNull))
            {
                s_dripReserveFirst = dt_bottle_details.Rows[0]["DripReserveFirst"].ToString();
            }
            object lastUseTimeObj = dt_bottle_details.Rows[0]["LastUseTime"];
            DateTime dateTime11 = (lastUseTimeObj == DBNull.Value || lastUseTimeObj == null)
                ? DateTime.Now
                : Convert.ToDateTime(lastUseTimeObj);
            double weight11 = Convert.ToDouble(dt_bottle_details.Rows[0]["CurrentWeight"]);




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
                                s_sql = "UPDATE abs_drop_details SET MinWeight = 1 WHERE BatchName = '" + oBatchName + "' AND MinWeight=0 And " +
                                "BottleNum = " + i_minBottleNo + " AND Finish = 0 ;";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                break;
                            }
                        }
                        else
                        {
                            s_sql = "UPDATE abs_drop_details SET MinWeight = 1 WHERE BatchName = '" + oBatchName + "' AND MinWeight=0 And " +
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
                                s_sql = "UPDATE abs_drop_details SET MinWeight = 3 WHERE BatchName = '" + oBatchName + "' AND  MinWeight=0 And " +
                                "BottleNum = " + i_minBottleNo + " AND Finish = 0 ;";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                break;
                            }
                        }
                        else
                        {
                            s_sql = "UPDATE abs_drop_details SET MinWeight = 3 WHERE BatchName = '" + oBatchName + "' AND  MinWeight=0 And " +
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

            //针检
            if ((0 >= i_adjust || false == b_lCheckSuccess) && !b_checkFail)
            {
            label8:
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
                        myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + "号母液瓶针检失败，是否继续?(继续针检请点是，退出针检请点否)", "滴液针检", true, 1);
                    else
                        myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + " bottle needle inspection failed, do you want to continue? " +
                            "(To continue the needle examination, please click Yes, and to exit the needle examination, please click No)", "Drip needle examination", true, 1);
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
                        goto label8;
                    else
                    {
                        b_checkFail = true;
                    }
                }
                else if (-2 == i_res)
                {
                    s_sql = "UPDATE abs_drop_details SET MinWeight = 2 WHERE BatchName = '" + oBatchName + "' AND " +
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

            if (_b_dripStop)
            {
                FADM_Object.Communal._b_stop = true;
            }
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
            int i_ret=FADM_Object.Communal.AddMacOther(o,ref dic_return,3,0);
            //夹不到针筒
            if (i_ret == -1)
            {
                if (i_lowSrart == 2)
                {
                    FADM_Object.Communal.WriteDripWait(true);
                    FADM_Object.MyAlarm myAlarm;
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + "号母液瓶未找到针筒，是否继续执行?(继续寻找请点是，退出滴液请点否)", "滴液", true, 1);
                    else
                        myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + " bottle did not find a syringe. Do you want to continue? " +
                            "(To continue searching, please click Yes. To exit Drip, please click No)", "Drip", true, 1);
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
                        goto label3;
                    else
                        throw new Exception("收到退出消息");
                }
                else
                {
                    s_sql = "UPDATE abs_drop_details SET MinWeight = 2 WHERE BatchName = '" + oBatchName + "' AND " +
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
                        //double d_blRErr = 0;
                        //if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                        //    d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                        //else
                        //    d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight));
                        
                        ////查询开料日期
                        //DataTable dt_bottle_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                        //            "SELECT * FROM bottle_details WHERE  BottleNum = " + i_minBottleNo + ";");

                        //if (0.00 != kvp.Value)
                        //{
                        //    FADM_Object.Communal._fadmSqlserver.ReviseData(
                        //    "UPDATE abs_drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + d_blRErr + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                        //    "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                        //    "CupNum = " + kvp.Key + ";");

                        //    DataTable dt_drop_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                        //        "SELECT * FROM abs_drop_details WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND CupNum = " + kvp.Key + ";");
                        //    //FADM_Object.Communal._fadmSqlserver.ReviseData(
                        //    //    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + dt_drop_details2.Rows[0]["RealDropWeight"] + " WHERE CupNum = " + kvp.Key + ";");

                        //    //母液瓶扣减
                        //    FADM_Object.Communal._fadmSqlserver.ReviseData(
                        //        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + dt_drop_details2.Rows[0]["RealDropWeight"] + " " +
                        //        "WHERE BottleNum = '" + i_minBottleNo + "';");
                        //}
                        //else
                        //{
                        //    FADM_Object.Communal._fadmSqlserver.ReviseData(
                        //   "UPDATE abs_drop_details SET Finish = 1,RealDropWeight = 0.00 " + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                        //   "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                        //   "CupNum = " + kvp.Key + ";");
                        //}

                        DataTable dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM abs_drop_details WHERE BatchName = '" + oBatchName.ToString() + "' AND Finish = 0" + " AND CupNum = " + kvp.Key + ";");
                        //查询一下加水没完成的也不置为完成
                        DataTable dt_drop_details4 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM abs_drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND AddWaterChoose = 1 AND AddWaterFinish =0" + " AND CupNum = " + kvp.Key + ";");
                        if (dt_drop_details3.Rows.Count == 0 && dt_drop_details4.Rows.Count == 0)
                        {
                            //置为完成
                            s_sql = "UPDATE abs_drop_head SET CupFinish = 1 WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + kvp.Key + "; ";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);


                            bool b_fail = true;

                             s_sql = "SELECT abs_drop_details.CupNum as CupNum, " +
                                         "abs_drop_details.BottleNum as BottleNum, " +
                                         "abs_drop_details.ObjectDropWeight as ObjectDropWeight, " +
                                         "abs_drop_details.RealDropWeight as RealDropWeight, " +
                                         "bottle_details.SyringeType as SyringeType " +
                                         "FROM abs_drop_details left join bottle_details on " +
                                         "bottle_details.BottleNum = abs_drop_details.BottleNum " +
                                         "WHERE abs_drop_details.BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + kvp.Key + ";";
                            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                            foreach (DataRow dr in dt_drop_head.Rows)
                            {
                                double d_blRealErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"]))) : Convert.ToDouble(string.Format("{0:F3}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])));
                                d_blRealErr = d_blRealErr < 0 ? -d_blRealErr : d_blRealErr;
                                if (d_blRealErr > Lib_Card.Configure.Parameter.Other_AErr_Drip)
                                    b_fail = false;
                            }

                            dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                    "SELECT * FROM abs_drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + kvp.Key + ";");
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
                                    //if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(kvp.Key))
                                    //{
                                    //    FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                                    //}
                                  //  FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  //"UPDATE cup_details SET Statues = '滴液成功' WHERE CupNum = " + i_cup + ";");
                                }
                                else
                                {
                                    s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                            ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    //FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    //"UPDATE cup_details SET Statues = '滴液失败' WHERE CupNum = " + i_cup + ";");
                                }
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE abs_drop_head SET DescribeChar = '" + s_describe + "',DescribeChar_EN = '" + s_describe_EN + "', FinishTime = '" + DateTime.Now + "', Step = 2 " +
                               "WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cup + ";");
                            }
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
                        //double d_blRErr = 0;
                        //if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                        //    d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                        //else
                        //    d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight)) : Convert.ToDouble(string.Format("{0:F3}", kvp.Value - Lib_Card.Configure.Parameter.Correcting_B_Weight));

                        ////查询开料日期
                        //DataTable dt_bottle_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                        //            "SELECT * FROM bottle_details WHERE  BottleNum = " + i_minBottleNo + ";");

                        //if (0.00 != kvp.Value)
                        //{
                        //    FADM_Object.Communal._fadmSqlserver.ReviseData(
                        //    "UPDATE abs_drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + d_blRErr + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                        //    "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                        //    "CupNum = " + kvp.Key + ";");

                        //    DataTable dt_drop_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                        //        "SELECT * FROM abs_drop_details WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND CupNum = " + kvp.Key + ";");
                        //    //FADM_Object.Communal._fadmSqlserver.ReviseData(
                        //    //    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + dt_drop_details2.Rows[0]["RealDropWeight"] + " WHERE CupNum = " + kvp.Key + ";");

                        //    //母液瓶扣减
                        //    FADM_Object.Communal._fadmSqlserver.ReviseData(
                        //        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + dt_drop_details2.Rows[0]["RealDropWeight"] + " " +
                        //        "WHERE BottleNum = '" + i_minBottleNo + "';");
                        //}
                        //else
                        //{
                        //    FADM_Object.Communal._fadmSqlserver.ReviseData(
                        //   "UPDATE abs_drop_details SET Finish = 1,RealDropWeight = 0.00 " + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                        //   "WHERE BatchName = '" + oBatchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                        //   "CupNum = " + kvp.Key + ";");
                        //}

                        DataTable dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM abs_drop_details WHERE BatchName = '" + oBatchName.ToString() + "' AND Finish = 0" + " AND CupNum = " + kvp.Key + ";");
                        //查询一下加水没完成的也不置为完成
                        DataTable dt_drop_details4 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND AddWaterChoose = 1 AND AddWaterFinish =0" + " AND CupNum = " + kvp.Key + ";");
                        if (dt_drop_details3.Rows.Count == 0 && dt_drop_details4.Rows.Count == 0)
                        {
                            //置为完成
                            s_sql = "UPDATE abs_drop_head SET CupFinish = 1 WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + kvp.Key + "; ";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);


                            bool b_fail = true;

                            s_sql = "SELECT abs_drop_details.CupNum as CupNum, " +
                                        "abs_drop_details.BottleNum as BottleNum, " +
                                        "abs_drop_details.ObjectDropWeight as ObjectDropWeight, " +
                                        "abs_drop_details.RealDropWeight as RealDropWeight, " +
                                        "bottle_details.SyringeType as SyringeType " +
                                        "FROM abs_drop_details left join bottle_details on " +
                                        "bottle_details.BottleNum = abs_drop_details.BottleNum " +
                                        "WHERE abs_drop_details.BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + kvp.Key + ";";
                            dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                            foreach (DataRow dr in dt_drop_head.Rows)
                            {
                                double d_blRealErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"]))) : Convert.ToDouble(string.Format("{0:F3}",
                                Convert.ToDouble(dr["ObjectDropWeight"]) - Convert.ToDouble(dr["RealDropWeight"])));
                                d_blRealErr = d_blRealErr < 0 ? -d_blRealErr : d_blRealErr;
                                if (d_blRealErr > Lib_Card.Configure.Parameter.Other_AErr_Drip)
                                    b_fail = false;
                            }

                            dt_drop_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                    "SELECT * FROM abs_drop_head WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + kvp.Key + ";");
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
                                    //if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(kvp.Key))
                                    //{
                                    //    FADM_Object.Communal._lis_dripSuccessCup.Add(i_cup);
                                    //}
                                  //  FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  //"UPDATE cup_details SET Statues = '滴液成功' WHERE CupNum = " + i_cup + ";");
                                }
                                else
                                {
                                    s_describe = "滴液失败!目标加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                            ",实际加水:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    s_describe_EN = "Drip Fail !ObjectAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_objWater) : string.Format("{0:F3}", d_objWater)) +
                                                     ",RealAddWaterWeight:" + (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", d_realWater) : string.Format("{0:F3}", d_realWater));
                                    //FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    //"UPDATE cup_details SET Statues = '滴液失败' WHERE CupNum = " + i_cup + ";");
                                }
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE abs_drop_head SET DescribeChar = '" + s_describe + "',DescribeChar_EN = '" + s_describe_EN + "', FinishTime = '" + DateTime.Now + "', Step = 2 " +
                               "WHERE BatchName = '" + oBatchName.ToString() + "' AND CupNum = " + i_cup + ";");
                            }
                        }

                    }
                }
                

                //更新需要加药第一杯脉冲
                s_sql = "UPDATE abs_drop_details SET NeedPulse = " + Communal._i_needPulse+" WHERE BatchName = '" + oBatchName + "'  And " +
                                "BottleNum = " + i_minBottleNo + " AND Finish = 0  And CupNum = "+Communal._i_needPulseCupNumber+";";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                //把剩余没有滴液的Min置为状态4
                s_sql = "UPDATE abs_drop_details SET MinWeight = 4 WHERE BatchName = '" + oBatchName + "'  And " +
                                "BottleNum = " + i_minBottleNo + " AND Finish = 0 ;";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                MyAlarm myAlarm;
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + "号母液瓶预滴液数值太小,请检查实际是否液量过低?(继续执行请点是)", "Drip", i_minBottleNo, 2, 20);
                else
                    myAlarm = new FADM_Object.MyAlarm( " The number of pre-drops in mother liquor bottle "+i_minBottleNo +"  is too small, please check whether the actual amount of liquid is too low" +
                        "( Continue to perform please click Yes)", "Drip", i_minBottleNo, 2, 20);

                //回一次原点再继续，担心失步导致后续母液抽不了
                int i_state = MyModbusFun.goHome();
                if (0 != i_state && -2 != i_state)
                    throw new Exception("驱动异常");

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
            s_sql = "SELECT BottleNum FROM abs_drop_details WHERE BatchName = '" + oBatchName + "' AND Finish = 0 AND MinWeight = 1 AND " +
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
                    b_chooseNo = true;
                }
            }

        //添加超出生命周期(过期)
        label17:
            s_sql = "SELECT BottleNum FROM abs_drop_details WHERE BatchName = '" + oBatchName + "' AND Finish = 0 AND MinWeight = 3 AND " +
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
                    b_chooseNo = true;
                }
            }


        //添加找不到针筒的
        label16:
            s_sql = "SELECT BottleNum FROM abs_drop_details WHERE BatchName = '" + oBatchName + "' AND Finish = 0 AND MinWeight = 2 AND " +
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
                FADM_Object.MyAlarm myAlarm;
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    myAlarm = new FADM_Object.MyAlarm(s_alarmBottleNo + "号母液瓶未检测到针筒，是否继续滴液 ? ", "滴液", true, 1);
                else
                    myAlarm = new FADM_Object.MyAlarm(s_alarmBottleNo + " bottle did not find a syringe. Do you want to continue? ", "Drip", true, 1);
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
            FADM_Object.Communal.WriteDripWait(true);
            if (!b_chooseNo)
            {
                lab_Re:
                //判断是否全部完成，等待是否还有洗杯没完成的
                s_sql = "SELECT * FROM abs_drop_details WHERE BatchName = '" + oBatchName + "' AND Finish = 0  AND " +
                   "BottleNum <= '" + Lib_Card.Configure.Parameter.Machine_Bottle_Total + "' ;";
                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                if (dt_drop_head.Rows.Count > 0)
                {
                    
                    foreach (DataRow dataRow in dt_drop_head.Rows)
                    {
                        if (dataRow["IsDrop"].ToString() == "1" && dataRow["MinWeight"].ToString() != "4")
                        {
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
                    s_sql = "SELECT * FROM abs_drop_head WHERE BatchName = '" + oBatchName + "' AND " +
                "AddWaterChoose = 1 AND AddWaterFinish = 0  ORDER BY CupNum;";
                    dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                    if(dt_drop_head.Rows.Count>0)
                    {
                        goto lab_again;
                    }
                }
            }
            if (null != thread)
                thread.Join();

            //if (FADM_Object.Communal._b_isFinishSend)
            {
                //把由于超期，液量低跳过的所有置为不合格
                FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE abs_drop_head SET DescribeChar = '滴液失败',DescribeChar_EN = 'Drip Fail',CupFinish = 1, FinishTime = '" + DateTime.Now + "', Step = 2 " +
                               "WHERE BatchName = '" + oBatchName + "' AND CupFinish != 1;");

                //获取滴液不合格记录
                dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(
               "SELECT * from abs_drop_head where DescribeChar like '%滴液失败%'" +
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


                if (FADM_Auto.ABS_Drip._b_dripErr == false)
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
                        List<int> lis_iWash = new List<int>();
                        if (1 == myAlarm._i_alarm_Choose)
                        {
                            //先把滴液区域重置
                            //滴液区
                            for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                            {
                                {

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE abs_drop_head SET CupFinish = 0, AddWaterFinish = 0, RealAddWaterWeight = 0.00 , Step = 1,DescribeChar=null " +
                                        "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + lis_cupFailT[i] + ";");

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE abs_drop_details SET Finish = 0, MinWeight = 0, RealDropWeight = 0.00 , NeedPulse = 0" +
                                        "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + lis_cupFailT[i] + ";");
                                    int Num = lis_cupFailT[i];

                                    if (!lis_iWash.Contains(Num))
                                        lis_iWash.Add(Num);

                                    //先发一个停止，再发一个洗杯
                                    int[] values1 = new int[1];
                                    values1[0] = 2;
                                    if (Convert.ToInt32(Num) == 1)
                                        FADM_Object.Communal._tcpModBusAbs.Write(13088, values1);
                                    else if (Convert.ToInt32(Num) == 2)
                                        FADM_Object.Communal._tcpModBusAbs.Write(13098, values1);
                                    else if (Convert.ToInt32(Num) == 3)
                                        FADM_Object.Communal._tcpModBusAbs.Write(13108, values1);
                                    else if (Convert.ToInt32(Num) == 4)
                                        FADM_Object.Communal._tcpModBusAbs.Write(13118, values1);

                                    //判断待机后再发洗杯
                                    while (true)
                                    {
                                        if (MyAbsorbance._abs_Temps[Convert.ToInt32(Num) - 1]._s_currentState == "1")
                                            break;
                                    }

                                    //发送洗杯
                                    SmartDyeing.FADM_Auto.MyAbsorbance.Generate(2, Num);
                                    SmartDyeing.FADM_Auto.MyAbsorbance.SendData(Num);
                                }



                            }

                            while (true)
                            {
                                if (lis_iWash.Contains(1))
                                {
                                    if (MyAbsorbance._abs_Temps[0]._s_currentState == "1")
                                    {
                                        //判断是否整个洗杯流程完成
                                         s_sql = "SELECT *  FROM Abs_details where CupNum = " + 1 + " And Finish = 0 order by StepNum;";

                                        DataTable dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                                        if (dt_temp.Rows.Count == 0)
                                            lis_iWash.Remove(1);
                                    }
                                }
                                else if (lis_iWash.Contains(3))
                                {
                                    if (MyAbsorbance._abs_Temps[2]._s_currentState == "1")
                                    {
                                        //判断是否整个洗杯流程完成
                                         s_sql = "SELECT *  FROM Abs_details where CupNum = " + 3 + " And Finish = 0 order by StepNum;";

                                        DataTable dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                                        if (dt_temp.Rows.Count == 0)
                                            lis_iWash.Remove(3);
                                    }
                                }
                                if (lis_iWash.Count == 0)
                                    break;
                                Thread.Sleep(1000);
                            }



                            //写入滴液状态
                            for (int i = lis_cupFailT.Count - 1; i >= 0; i--)
                            {
                                int i_cupNum = lis_cupFailT[i];
                                //配液
                                SmartDyeing.FADM_Auto.MyAbsorbance.Generate(3, i_cupNum);
                                SmartDyeing.FADM_Auto.MyAbsorbance.SendData(i_cupNum);
                            }

                            if (lis_cupFailT.Contains(1))
                            {
                                //等待准备状态
                                while (true)
                                {
                                    //判断两个杯是否都可以注液
                                    if (MyAbsorbance._abs_Temps[0]._s_request == "4")
                                        break;

                                    Thread.Sleep(1000);
                                }
                            }
                            if (lis_cupFailT.Contains(3))
                            {
                                //等待准备状态
                                while (true)
                                {
                                    //判断两个杯是否都可以注液
                                    if (MyAbsorbance._abs_Temps[2]._s_request == "4")
                                        break;

                                    Thread.Sleep(1000);
                                }
                            }
                            //重新滴液时重新清一下
                            this.DripProcess(oBatchName);
                        }
                        else
                        {
                            if(lis_cupFailT.Contains(1))
                            {
                                FADM_Object.Communal._b_use[0] = false;

                                Thread thread33 = new Thread(() =>
                                {
                                    //等待没有交互时再发送停止
                                    //while (true)
                                    //{
                                    //    if (MyAbsorbance._abs_Temps[Convert.ToInt32(_cup.NO) - 1]._s_request == "0")
                                    //    {
                                    //        break;
                                    //    }
                                    //}

                                    //先发一个停止，再发一个洗杯
                                    int[] values1 = new int[1];
                                    values1[0] = 2;
                                        FADM_Object.Communal._tcpModBusAbs.Write(13088, values1);

                                    //判断待机后再发洗杯
                                    while (true)
                                    {
                                        if (MyAbsorbance._abs_Temps[0]._s_currentState == "1")
                                            break;
                                    }

                                    s_sql = "UPDATE abs_cup_details SET Statues='洗杯',IsUsing = 1,Type=0  WHERE CupNum = " + 1+ " ;";
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                    SmartDyeing.FADM_Auto.MyAbsorbance.Generate(2, 1);
                                    SmartDyeing.FADM_Auto.MyAbsorbance.SendData(1);




                                });
                                thread33.Start();
                            }
                            if (lis_cupFailT.Contains(3))
                            {
                                FADM_Object.Communal._b_use[2] = false;

                                Thread thread33 = new Thread(() =>
                                {
                                    //等待没有交互时再发送停止
                                    //while (true)
                                    //{
                                    //    if (MyAbsorbance._abs_Temps[Convert.ToInt32(_cup.NO) - 1]._s_request == "0")
                                    //    {
                                    //        break;
                                    //    }
                                    //}

                                    //先发一个停止，再发一个洗杯
                                    int[] values1 = new int[1];
                                    values1[0] = 2;
                                    FADM_Object.Communal._tcpModBusAbs.Write(13108, values1);

                                    //判断待机后再发洗杯
                                    while (true)
                                    {
                                        if (MyAbsorbance._abs_Temps[2]._s_currentState == "1")
                                            break;
                                    }

                                    s_sql = "UPDATE abs_cup_details SET Statues='洗杯',IsUsing = 1,Type=0  WHERE CupNum = " + 3 + " ;";
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                    SmartDyeing.FADM_Auto.MyAbsorbance.Generate(2, 3);
                                    SmartDyeing.FADM_Auto.MyAbsorbance.SendData(3);




                                });
                                thread33.Start();
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
                 "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'abs_drop_head';");
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
                   "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'abs_drop_details';");
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
                   "SELECT * FROM abs_drop_head WHERE   BatchName = '" + oBatchName + "' ;");

                foreach (DataRow row in dt_drop_head.Rows)
                {
                    if (lis_cupSuc.Contains(Convert.ToInt32(row["CupNum"].ToString())))
                    {
                        if (Convert.ToInt32(row["CupNum"].ToString()) == 1)
                        {
                            //计算混合液浓度
                            FADM_Object.Communal._d_abs_mixture = 0;
                            DataTable dt_drop_head_suc = FADM_Object.Communal._fadmSqlserver.GetData(
                       "SELECT * FROM abs_drop_head WHERE   BatchName = '" + oBatchName + "' AND CupNum = " + row["CupNum"].ToString() + " ;");

                            DataTable dt_drop_details_suc = FADM_Object.Communal._fadmSqlserver.GetData(
                       "SELECT * FROM abs_drop_details WHERE   BatchName = '" + oBatchName + "' AND CupNum = " + row["CupNum"].ToString() + " ;");
                            //混合液总重量
                            double d_sum = 0;

                            //混合液纯含量
                            double d_wei = 0;

                            //配方用量
                            double d_sum_do = 0;

                            if (dt_drop_head_suc.Rows.Count > 0)
                            {
                                d_sum += Convert.ToDouble(dt_drop_head_suc.Rows[0]["RealAddWaterWeight"].ToString());
                            }
                            foreach (DataRow row_suc in dt_drop_details_suc.Rows)
                            {
                                d_sum += Convert.ToDouble(row_suc["RealDropWeight"].ToString());
                            }
                            foreach (DataRow row_suc in dt_drop_details_suc.Rows)
                            {
                                d_sum_do += Convert.ToDouble(row_suc["FormulaDosage"].ToString());
                            }
                            foreach (DataRow row_suc in dt_drop_details_suc.Rows)
                            {
                                d_wei += Convert.ToDouble(row_suc["RealDropWeight"].ToString()) * Convert.ToDouble(row_suc["RealConcentration"].ToString());
                            }
                            //if (d_sum > 0)
                            //    FADM_Object.Communal._d_abs_mixture = d_wei / d_sum;
                            // FADM_Object.Communal._d_abs_mixture = 0.01;
                            FADM_Object.Communal._d_abs_mixture = Convert.ToDouble(dt_drop_head_suc.Rows[0]["ClothWeight"].ToString()) /
                        Convert.ToDouble(dt_drop_head_suc.Rows[0]["TotalWeight"].ToString());

                            //FADM_Object.Communal._d_abs_mixture = Convert.ToDouble(dt_drop_head_suc.Rows[0]["TotalWeight"].ToString()) * 0.01 * d_sum_do * 0.01;

                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "INSERT INTO abs_history_head (" + s_columnHead + ") (SELECT " + s_columnHead + " FROM abs_drop_head " +
                            "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + row["CupNum"].ToString() + ") ;");

                            //FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "INSERT INTO history_head (" + s_columnHead + ") (SELECT " + s_columnHead + " FROM drop_head " +
                            //    "WHERE BatchName = '" + o_BatchName + "' AND CupNum >= " + _i_cupMin + " AND CupNum <= " + _i_cupMax + s_te + ");");


                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "INSERT INTO abs_history_details (" + s_columnDetails + ") (SELECT " + s_columnDetails + " FROM abs_drop_details " +
                               "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + row["CupNum"].ToString() + ") ;");

                            //滴液
                            //if (SmartDyeing.FADM_Object.Communal._lis_dripCupNum.Contains(Convert.ToInt32(row["CupNum"].ToString())))
                            {
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                      "DELETE FROM abs_drop_head WHERE CupNum = " + row["CupNum"].ToString() + " AND BatchName = '" + oBatchName + "' ;");
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "DELETE FROM abs_drop_details WHERE CupNum = " + row["CupNum"].ToString() + " AND BatchName = '" + oBatchName + "' ;");


                                //FADM_Object.Communal._fadmSqlserver.ReviseData(
                                //    "UPDATE cup_details SET FormulaCode = null, " +
                                //    "DyeingCode = null, IsUsing = 0, Statues = '待机', " +
                                //    "StartTime = null, SetTemp = null, StepNum = null, TotalWeight = null, " +
                                //    "TotalStep = null, TechnologyName = null, StepStartTime = null, SetTime = null,RecordIndex = 0 WHERE CupNum = " +
                                //    row["CupNum"].ToString() + " ;");
                            }
                        }
                        else
                        {
                            //计算混合液浓度
                            FADM_Object.Communal._d_abs_mixture_2 = 0;
                            DataTable dt_drop_head_suc = FADM_Object.Communal._fadmSqlserver.GetData(
                       "SELECT * FROM abs_drop_head WHERE   BatchName = '" + oBatchName + "' AND CupNum = " + row["CupNum"].ToString() + " ;");

                            DataTable dt_drop_details_suc = FADM_Object.Communal._fadmSqlserver.GetData(
                       "SELECT * FROM abs_drop_details WHERE   BatchName = '" + oBatchName + "' AND CupNum = " + row["CupNum"].ToString() + " ;");
                            //混合液总重量
                            double d_sum = 0;

                            //混合液纯含量
                            double d_wei = 0;

                            //配方用量
                            double d_sum_do = 0;

                            foreach (DataRow row_suc in dt_drop_details_suc.Rows)
                            {
                                d_sum_do += Convert.ToDouble(row_suc["FormulaDosage"].ToString());
                            }

                            //if (dt_drop_head_suc.Rows.Count > 0)
                            //{
                            //    d_sum += Convert.ToDouble(dt_drop_head_suc.Rows[0]["RealAddWaterWeight"].ToString());
                            //}
                            //foreach (DataRow row_suc in dt_drop_details_suc.Rows)
                            //{
                            //    d_sum += Convert.ToDouble(row_suc["RealDropWeight"].ToString());
                            //}
                            //foreach (DataRow row_suc in dt_drop_details_suc.Rows)
                            //{
                            //    d_wei += Convert.ToDouble(row_suc["RealDropWeight"].ToString()) * Convert.ToDouble(row_suc["RealConcentration"].ToString());
                            //}
                            //if (d_sum > 0)
                            //    FADM_Object.Communal._d_abs_mixture_2 = d_wei / d_sum;
                            FADM_Object.Communal._d_abs_mixture_2 = Convert.ToDouble(dt_drop_head_suc.Rows[0]["ClothWeight"].ToString()) /
                                   Convert.ToDouble(dt_drop_head_suc.Rows[0]["TotalWeight"].ToString());

                            //FADM_Object.Communal._d_abs_mixture_2 = Convert.ToDouble(dt_drop_head_suc.Rows[0]["TotalWeight"].ToString()) * 0.01 * d_sum_do * 0.01;
                            // FADM_Object.Communal._d_abs_mixture_2 = 0.01;
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "INSERT INTO abs_history_head (" + s_columnHead + ") (SELECT " + s_columnHead + " FROM abs_drop_head " +
                            "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + row["CupNum"].ToString() + ") ;");

                            //FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "INSERT INTO history_head (" + s_columnHead + ") (SELECT " + s_columnHead + " FROM drop_head " +
                            //    "WHERE BatchName = '" + o_BatchName + "' AND CupNum >= " + _i_cupMin + " AND CupNum <= " + _i_cupMax + s_te + ");");


                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "INSERT INTO abs_history_details (" + s_columnDetails + ") (SELECT " + s_columnDetails + " FROM abs_drop_details " +
                               "WHERE BatchName = '" + oBatchName + "' AND CupNum = " + row["CupNum"].ToString() + ") ;");

                            //滴液
                            //if (SmartDyeing.FADM_Object.Communal._lis_dripCupNum.Contains(Convert.ToInt32(row["CupNum"].ToString())))
                            {
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                      "DELETE FROM abs_drop_head WHERE CupNum = " + row["CupNum"].ToString() + " AND BatchName = '" + oBatchName + "' ;");
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "DELETE FROM abs_drop_details WHERE CupNum = " + row["CupNum"].ToString() + " AND BatchName = '" + oBatchName + "' ;");


                                //FADM_Object.Communal._fadmSqlserver.ReviseData(
                                //    "UPDATE cup_details SET FormulaCode = null, " +
                                //    "DyeingCode = null, IsUsing = 0, Statues = '待机', " +
                                //    "StartTime = null, SetTemp = null, StepNum = null, TotalWeight = null, " +
                                //    "TotalStep = null, TechnologyName = null, StepStartTime = null, SetTime = null,RecordIndex = 0 WHERE CupNum = " +
                                //    row["CupNum"].ToString() + " ;");
                            }
                        }
                    }
                }


            }

        }

        

    }
}
