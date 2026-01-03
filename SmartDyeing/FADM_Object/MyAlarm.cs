using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Speech.Synthesis;
using static System.Net.Mime.MediaTypeNames;
using SmartDyeing.FADM_Auto;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using com.google.zxing;
using System.Data;

namespace SmartDyeing.FADM_Object
{
    class MyAlarm
    {
        [DllImport("user32.dll", EntryPoint = "FindWindow", CharSet = CharSet.Auto)]
        private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        public const int WM_CLOSE = 0x10;

        #region 全局变量

        /// <summary>
        /// 选择:0:未选择;1:Yes;2:No
        /// </summary>
        public int _i_alarm_Choose = 0;

        /// <summary>
        /// 是否重复 1：重复 0：不重复
        /// </summary>
        public int _i_alarm_Repeat = 0;



        #endregion

        #region 构造函数
        public MyAlarm(string s_text, int i_speech, params bool[] ba_isBroadcastW)
        {
            if (!string.IsNullOrEmpty(s_text))
            {
            label1:
                try
                {
                    string s_insert = Lib_Card.CardObject.InsertCF(s_text, Lib_Card.Configure.Parameter.Other_Language == 0 ? "温馨提示":"Tips");

                    if (s_insert == "重复"|| s_insert == "重复1")
                    {
                        return;
                    }
                    if (s_text != "右光幕遮挡,请离开光幕" && s_text != "Right light curtain obstruction,Please step away from the light curtain" 
                        && s_text != "左光幕遮挡,请离开光幕" && s_text != "Left light curtain obstruction,Please step away from the light curtain"
                        && s_text != "前光幕遮挡,请离开光幕" && s_text != "Front light curtain obstruction,Please step away from the light curtain" 
                        && s_text != "急停已按下,请打开急停开关" && s_text != "Emergency stop pressed,Please turn on the emergency stop switch" 
                        && s_text != "左门已打开,请关闭左门" && s_text != "The left door is open,Please close the left door"
                        && s_text != "右门已打开,请关闭右门" && s_text != "The right door is open,Please close the right door" 
                        && s_text != "后光幕遮挡,请离开光幕" && s_text != "Back light curtain obstruction,Please step away from the light curtain"
                        && FADM_Object.Communal._b_isNetWork)
                    {
                        //插入微信播报表 推送微信 isBroadcastW为真 保存表   isBroadcastW为假 不保存表   
                        inBroadcastW(FADM_Object.Communal._s_machineCode, s_text, s_insert, ba_isBroadcastW);
                    }

                    Thread P_thd_1 = new Thread(() =>
                    {
                        Thread P_thd_2 = new Thread(() =>
                        {
                            if (s_text == "右光幕遮挡,请离开光幕" || s_text == "Right light curtain obstruction,Please step away from the light curtain" 
                            || s_text == "左光幕遮挡,请离开光幕" || s_text == "Left light curtain obstruction,Please step away from the light curtain"
                            || s_text == "前光幕遮挡,请离开光幕" || s_text == "Front light curtain obstruction,Please step away from the light curtain"
                            || s_text == "急停已按下,请打开急停开关" || s_text == "Emergency stop pressed,Please turn on the emergency stop switch"
                            || s_text == "左门已打开,请关闭左门" || s_text == "The left door is open,Please close the left door"
                            || s_text == "右门已打开,请关闭右门" || s_text == "The right door is open,Please close the right door"
                            || s_text == "后光幕遮挡,请离开光幕" || s_text == "Back light curtain obstruction,Please step away from the light curtain")
                            {
                                //Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer buzzer = new Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer_Basic();
                                //buzzer.Buzzer_On();


                                while (true)
                                {
                                    if (s_text == "右光幕遮挡,请离开光幕" || s_text == "Right light curtain obstruction,Please step away from the light curtain" 
                                    || s_text == "右门已打开,请关闭右门" || s_text == "The right door is open,Please close the right door")
                                    {
                                        //获取光幕1状态
                                        if (!Lib_Card.CardObject.bRight)
                                        {
                                            Lib_Card.CardObject.DeleteD(s_insert);
                                            break;
                                        }
                                        if (Lib_Card.CardObject.keyValuePairs[s_insert].Choose != 0)
                                        {
                                            Lib_Card.CardObject.DeleteD(s_insert);
                                            break;
                                        }

                                    }
                                    else if (s_text == "左光幕遮挡,请离开光幕" || s_text == "Left light curtain obstruction,Please step away from the light curtain" 
                                    || s_text == "左门已打开,请关闭左门" || s_text == "The left door is open,Please close the left door")
                                    {
                                        //获取光幕2状态
                                        if (!Lib_Card.CardObject.bLeft)
                                        {
                                            Lib_Card.CardObject.DeleteD(s_insert);

                                            break;
                                        }
                                        if (Lib_Card.CardObject.keyValuePairs[s_insert].Choose != 0)
                                        {
                                            Lib_Card.CardObject.DeleteD(s_insert);
                                            break;
                                        }
                                    }
                                    else if (s_text == "前光幕遮挡,请离开光幕" || s_text == "Front light curtain obstruction,Please step away from the light curtain")
                                    {
                                        //获取光幕2状态
                                        if (!Lib_Card.CardObject.bFront)
                                        {
                                            Lib_Card.CardObject.DeleteD(s_insert);

                                            break;
                                        }
                                        if (Lib_Card.CardObject.keyValuePairs[s_insert].Choose != 0)
                                        {
                                            Lib_Card.CardObject.DeleteD(s_insert);
                                            break;
                                        }
                                    }
                                    else if (s_text == "后光幕遮挡,请离开光幕" || s_text == "Back light curtain obstruction,Please step away from the light curtain")
                                    {
                                        //获取光幕2状态
                                        if (!Lib_Card.CardObject.bBack)
                                        {
                                            Lib_Card.CardObject.DeleteD(s_insert);

                                            break;
                                        }
                                        if (Lib_Card.CardObject.keyValuePairs[s_insert].Choose != 0)
                                        {
                                            Lib_Card.CardObject.DeleteD(s_insert);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        //获取急停按钮状态
                                        if (s_text == "急停已按下,请打开急停开关" || s_text == "Emergency stop pressed,Please turn on the emergency stop switch")
                                        {
                                            if (!Lib_Card.CardObject.bStopScr)
                                            {
                                                Lib_Card.CardObject.DeleteD(s_insert);
                                                break;
                                            }
                                        }
                                        if (Lib_Card.CardObject.keyValuePairs[s_insert].Choose != 0)
                                        {
                                            Lib_Card.CardObject.DeleteD(s_insert);
                                            break;
                                        }
                                    }
                                    Thread.Sleep(1);
                                }
                                //buzzer.Buzzer_Off();
                            }
                            else
                            {
                                //触发定时关闭对话框功能
                                Thread.Sleep(1000);
                                Lib_Card.CardObject.DeleteD(s_insert);
                                kill();

                            }
                        });

                        P_thd_2.Start();



                    });


                    // 启动线程
                    P_thd_1.Start();
                }
                catch
                {
                    goto label1;
                }


            }
        }


        public MyAlarm(string s_text, string s_caption)
        {
            if (!string.IsNullOrEmpty(s_text))
            {
            label1:
                try
                {
                    Thread thread = new Thread(() =>
                    {

                        
                            string s_insert = Lib_Card.CardObject.InsertCF(s_text, s_caption);
                            if (s_insert == "重复")
                            {
                                return;
                            }

                            //Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer buzzer = new Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer_Basic();
                            //buzzer.Buzzer_On();


                            while (true)
                            {
                                Thread.Sleep(1);
                                if (Lib_Card.CardObject.keyValuePairs[s_insert].Choose != 0)
                                    break;

                            }

                            Lib_Card.CardObject.DeleteD(s_insert);


                            //buzzer.Buzzer_Off();
                        




                    });
                    thread.Start();
                }
                catch
                {
                    goto label1;
                }
            }
        }

        public MyAlarm(string s_text, string s_caption, int i_cupNum, int i_req, int i_type)
        {
            if (!string.IsNullOrEmpty(s_text))
            {
            label1:
                try
                {
                    Thread thread = new Thread(() =>
                    {


                        string s_insert = Lib_Card.CardObject.InsertCF(s_text, s_caption);
                        if (s_insert == "重复")
                        {
                            //_i_alarm_Choose = 1;
                            return;
                        }
                        else if (s_insert == "重复1")
                        {
                            if (i_type == 4)
                            {
                                // //修改为加药
                                // FADM_Object.Communal._fadmSqlserver.ReviseData(
                                //"UPDATE dye_details SET  Cooperate = 1 WHERE  Cooperate = " + 9 + " AND CupNum = " + i_cupNum + " ;");

                                //重新置0，等待获取开盖申请
                                FADM_Auto.Dye._cup_Temps[i_cupNum - 1]._i_cover = 0;

                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE cup_details SET  Cooperate = 0 WHERE  CupNum = " + i_cupNum + " ;");

                                _i_alarm_Repeat = 1;
                                return;

                            }
                            else if (i_type == 5)
                            {
                                //修改为流程加水
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE dye_details SET  Cooperate = 3 WHERE  Cooperate = " + 9 + " AND CupNum = " + i_cupNum + " ;");

                                _i_alarm_Repeat = 1;
                                return;
                            }
                            else if (i_type == 6)
                            {
                                //修改为洗杯加水
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE cup_details SET  Cooperate = 4 WHERE  CupNum = " + i_cupNum + " ;");

                                _i_alarm_Repeat = 1;
                                return;
                            }
                            else if (i_type == 7)
                            {
                                //滴液加水
                                _i_alarm_Repeat = 1;
                                return;
                            }
                            else if (i_type == 8)
                            {
                                //修改为放布
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE cup_details SET  Cooperate = 8 WHERE  CupNum = " + i_cupNum + " ;");

                                _i_alarm_Repeat = 1;
                                return;
                            }
                            else if (i_type == 9)
                            {
                                //修改为出布
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE cup_details SET  Cooperate = 9 WHERE  CupNum = " + i_cupNum + " ;");

                                _i_alarm_Repeat = 1;
                                return;
                            }
                            else if (i_type == 10)
                            {
                                //预滴液数值太小
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE drop_details SET  MinWeight = 0 WHERE  MinWeight = " + 4 + " AND BottleNum = " + i_cupNum + " ;");

                                _i_alarm_Repeat = 1;
                                return;
                            }
                            else if (i_type == 22)
                            {
                                //什么都不需要做，只是提示
                            }
                            else if (i_type == 51)
                            {
                                //修改为放瓶
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE brew_station_details SET  Cooperate = 1 WHERE  Station = " + i_cupNum + " ;");
                                _i_alarm_Repeat = 1;
                                return;
                            }

                            else if (i_type == 64)
                            {
                                //人工确定洗瓶工位有瓶
                                //发送执行完成
                                int[] ia_zero = new int[1];
                                ia_zero[0] = 8;
                                //开料工位取瓶完成
                                Dye.BrewAndWashHMIWrite(22289 + 40 * (i_cupNum - 1), ia_zero);
                                ia_zero[0] = 2;
                                //洗瓶工位放瓶完成
                                Dye.BrewAndWashHMIWrite(22891, ia_zero);
                                //修改开料工位申请
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                         "UPDATE brew_station_details SET Cooperate = 0  WHERE Station = " + i_cupNum + ";");
                                //修改洗杯工位申请
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                         "UPDATE brew_station_details SET Cooperate = 0  WHERE Station = 6;");
                            }
                            else if (i_type == 65)
                            {
                                //人工确定开料工位有瓶
                                //发送执行完成
                                int[] ia_zero = new int[1];
                                //发送执行完成
                                ia_zero[0] = 8;
                                //开料工位放瓶完成
                                Dye.BrewAndWashHMIWrite(22289 + 40 * (i_cupNum - 1), ia_zero);
                                ia_zero[0] = 2;
                                //洗瓶工位取瓶完成
                                Dye.BrewAndWashHMIWrite(22891, ia_zero);
                                //修改开料工位申请
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                         "UPDATE brew_station_details SET Cooperate = 0,Statues='运行中'  WHERE Station = " + i_cupNum + ";");
                                //修改洗杯工位申请
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                         "UPDATE brew_station_details SET Cooperate = 0,Statues='待机'  WHERE Station = 6;");
                            }
                            else if (i_type == 67)
                            {
                                //人工确认已拿走存针位置针筒
                                
                                //修改开料工位申请
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                         "UPDATE brew_station_details SET Cooperate = 13,WashSyringesFinish=0  WHERE Station = " + i_cupNum + ";");
                            }
                            else
                            {
                                FADM_Auto.Dye._cup_Temps[i_cupNum - 1]._i_cover = 0;

                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE cup_details SET  Cooperate = 0 WHERE  CupNum = " + i_cupNum + " ;");

                                _i_alarm_Repeat = 1;
                                return;
                            }
                        }
                        else {

                            if (FADM_Object.Communal._b_isNetWork) {
                                inBroadcastW(FADM_Object.Communal._s_machineCode, s_text, s_insert, true);
                            }
                           
                        }

                        //Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer buzzer = new Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer_Basic();
                        //buzzer.Buzzer_On();
                        while (true)
                        {
                            Thread.Sleep(1);
                            if (Lib_Card.CardObject.keyValuePairs[s_insert].Choose != 0)
                                break;
                            if (FADM_Object.Communal._b_isNetWork)
                            {
                                getBroadcastRe(FADM_Object.Communal._s_machineCode, s_insert, s_insert);
                                Thread.Sleep(1000);
                            }
                        }

                        _i_alarm_Choose = Lib_Card.CardObject.keyValuePairs[s_insert].Choose;

                        //正常重复执行，重置等待下一次轮询
                        if (i_req == 1)
                        {

                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE cup_details SET  Cooperate = 0 WHERE  CupNum = " + i_cupNum + " ;");

                        }
                        //选择继续执行或者已完成开关盖
                        else
                        {
                            //如果选择是，就继续
                            if (_i_alarm_Choose == 1)
                            {
                                if (i_type == 4)
                                {
                                    // //修改为加药
                                    // FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    //"UPDATE dye_details SET  Cooperate = 1 WHERE  Cooperate = " + 9 + " AND CupNum = " + i_cupNum + " ;");

                                    //重新置0，等待获取开盖申请
                                    FADM_Auto.Dye._cup_Temps[i_cupNum - 1]._i_cover = 0;

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE cup_details SET  Cooperate = 0 WHERE  CupNum = " + i_cupNum + " ;");

                                }
                                else if (i_type == 5)
                                {
                                    //修改为流程加水
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE dye_details SET  Cooperate = 3 WHERE  Cooperate = " + 9 + " AND CupNum = " + i_cupNum + " ;");
                                }
                                else if (i_type == 6)
                                {
                                    //修改为洗杯加水
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE cup_details SET  Cooperate = 4 WHERE  CupNum = " + i_cupNum + " ;");
                                }
                                else if (i_type == 7)
                                {
                                    //滴液加水
                                }
                                else if (i_type == 8 )
                                {
                                    //修改为放布
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE cup_details SET  Cooperate = 8 WHERE  CupNum = " + i_cupNum + " ;");
                                }
                                else if (i_type == 9 )
                                {
                                    //修改为出布
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE cup_details SET  Cooperate = 9 WHERE  CupNum = " + i_cupNum + " ;");
                                }
                                else if (i_type == 10)
                                {
                                    //预滴液数值太小
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE drop_details SET  MinWeight = 0 WHERE  MinWeight = " + 4 + " AND BottleNum = " + i_cupNum + " ;");

                                    _i_alarm_Repeat = 1;
                                }
                                else if (i_type == 12)
                                {
                                    //关盖失败，已把盖子放到杯中，要人工检查是否关盖正常，点击是就直接完成关盖，继续运行

                                    //复位加药启动信号
                                    int[] ia_zero = new int[1];
                                    //
                                    int i_cupNo = i_cupNum;
                                    ia_zero[0] = 0;

                                    FADM_Auto.Dye.DyeOpenOrCloseCover(i_cupNo, 1);

                                    FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._i_cover = 2;
                                    //Thread.Sleep(2000);
                                    Communal._fadmSqlserver.ReviseData("Update  cup_details set CoverStatus = 1,Cooperate=0 where CupNum = " + i_cupNo);
                                    //Communal._fadmSqlserver.ReviseData("Update  cup_details set Cooperate=0 where Cooperate = 5 and CupNum = " + i_cupNo);

                                    FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._i_cupCover = 1;
                                }
                                else if (i_type == 22)
                                {
                                    //什么都不需要做，只是提示
                                }
                                else if (i_type == 51)
                                {
                                    //修改为放瓶
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE brew_station_details SET  Cooperate = 1 WHERE  Station = " + i_cupNum + " ;");
                                }
                                else if (i_type == 64)
                                {
                                    //人工确定洗瓶工位有瓶
                                    //发送执行完成
                                    int[] ia_zero = new int[1];
                                    ia_zero[0] = 8;
                                    //开料工位取瓶完成
                                    Dye.BrewAndWashHMIWrite(22289 + 40 * (i_cupNum - 1), ia_zero);
                                    //洗瓶工位放瓶完成
                                    ia_zero[0] = 2;
                                    Dye.BrewAndWashHMIWrite(22891, ia_zero);
                                    //修改开料工位申请
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                             "UPDATE brew_station_details SET Cooperate = 0  WHERE Station = " + i_cupNum + ";");
                                    //修改洗杯工位申请
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                             "UPDATE brew_station_details SET Cooperate = 0  WHERE Station = 6;");
                                }
                                else if (i_type == 65)
                                {
                                    //人工确定开料工位有瓶
                                    //发送执行完成
                                    int[] ia_zero = new int[1];
                                    //发送执行完成
                                    ia_zero[0] = 8;
                                    //开料工位放瓶完成
                                    Dye.BrewAndWashHMIWrite(22289 + 40 * (i_cupNum - 1), ia_zero);
                                    //洗瓶工位取瓶完成
                                    ia_zero[0] = 2;
                                    Dye.BrewAndWashHMIWrite(22891, ia_zero);
                                    //修改开料工位申请
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                             "UPDATE brew_station_details SET Cooperate = 0,Statues='运行中'  WHERE Station = " + i_cupNum + ";");
                                    //修改洗杯工位申请
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                             "UPDATE brew_station_details SET Cooperate = 0,Statues='待机'  WHERE Station = 6;");
                                }
                                else if (i_type == 67)
                                {
                                    //人工确认已拿走存针位置针筒

                                    //修改开料工位申请
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                             "UPDATE brew_station_details SET Cooperate = 13,WashSyringesFinish=0  WHERE Station = " + i_cupNum + ";");
                                }
                                else
                                {
                                    FADM_Auto.Dye._cup_Temps[i_cupNum - 1]._i_cover = 0;

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE cup_details SET  Cooperate = 0 WHERE  CupNum = " + i_cupNum + " ;");
                                }
                            }
                            //选择否就直接置杯盖状态
                            else
                            {
                                //关盖完成
                                if (i_type == 1)
                                {
                                    //复位加药启动信号
                                    int[] ia_zero = new int[1];
                                    //
                                    int i_cupNo = i_cupNum;
                                    ia_zero[0] = 0;

                                    FADM_Auto.Dye.DyeOpenOrCloseCover(i_cupNo,1);

                                    FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._i_cover = 2;
                                    //Thread.Sleep(2000);
                                    Communal._fadmSqlserver.ReviseData("Update  cup_details set CoverStatus = 1,Cooperate=0 where CupNum = " + i_cupNo);
                                    //Communal._fadmSqlserver.ReviseData("Update  cup_details set Cooperate=0 where Cooperate = 5 and CupNum = " + i_cupNo);

                                    FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._i_cupCover = 1;
                                }
                                //开盖完成
                                else if (i_type == 2 || i_type == 5 || i_type == 4 || i_type == 6 || i_type == 8 || i_type == 9 || i_type == 10  || i_type == 44)
                                {
                                    //
                                    int iCupNo = i_cupNum;

                                    if (i_type == 2 || i_type == 44)
                                    {
                                        FADM_Auto.Dye.DyeOpenOrCloseCover(iCupNo, 2);

                                        FADM_Auto.Dye._cup_Temps[iCupNo - 1]._i_cover = 2;
                                        //Thread.Sleep(2000);
                                        Communal._fadmSqlserver.ReviseData("Update  cup_details set CoverStatus = 2,Cooperate=0 where CupNum = " + iCupNo);

                                        FADM_Auto.Dye._cup_Temps[iCupNo - 1]._i_cupCover = 2;
                                    }

                                    //加药
                                    else if (i_type == 4)
                                    {
                                        //     FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        //"UPDATE dye_details SET  Cooperate = 1 WHERE  Cooperate = " + 9 + " AND CupNum = " + i_cupNum + " ;");

                                        //重新置0，等待获取开盖申请
                                        FADM_Auto.Dye._cup_Temps[i_cupNum - 1]._i_cover = 0;

                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                           "UPDATE cup_details SET  Cooperate = 0 WHERE  CupNum = " + i_cupNum + " ;");
                                    }
                                    //流程加水
                                    else if (i_type == 5)
                                    {
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE dye_details SET  Cooperate = 3 WHERE  Cooperate = " + 9 + " AND CupNum = " + i_cupNum + " ;");
                                    }
                                    //洗杯加水
                                    else if (i_type == 6)
                                    {
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE cup_details SET  Cooperate = 4 WHERE  CupNum = " + i_cupNum + " ;");
                                    }
                                    else if (i_type == 8)
                                    {
                                        //修改为放布
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                           "UPDATE cup_details SET  Cooperate = 8 WHERE  CupNum = " + i_cupNum + " ;");
                                    }
                                    else if (i_type == 9)
                                    {
                                        //修改为出布
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                           "UPDATE cup_details SET  Cooperate = 9 WHERE  CupNum = " + i_cupNum + " ;");
                                    }
                                    else if (i_type == 10)
                                    {
                                        //预滴液数值太小
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE drop_details SET  MinWeight = 0 WHERE  MinWeight = " + 4 + " AND BottleNum = " + i_cupNum + " ;");

                                        _i_alarm_Repeat = 1;
                                    }
                                    else if (i_type == 51)
                                    {
                                        //修改为放瓶
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                           "UPDATE brew_station_details SET  Cooperate = 1 WHERE  Station = " + i_cupNum + " ;");
                                    }

                                }
                                //无锁止信号操作
                                else if (i_type == 3)
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE cup_details SET  Cooperate = 0 WHERE  CupNum = " + i_cupNum + " ;");
                                }
                                else if (i_type == 13 || i_type == 14)
                                {
                                    FADM_Auto.Dye._cup_Temps[i_cupNum - 1]._i_cover = 0;

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE cup_details SET  Cooperate = 0 WHERE  CupNum = " + i_cupNum + " ;");
                                }
                                else if(i_type==12)
                                {
                                    //关盖失败，已把盖子放到杯中，要人工检查是否关盖正常，点击否也直接完成关盖，继续运行

                                    //复位加药启动信号
                                    int[] ia_zero = new int[1];
                                    //
                                    int i_cupNo = i_cupNum;
                                    ia_zero[0] = 0;

                                    FADM_Auto.Dye.DyeOpenOrCloseCover(i_cupNo, 1);

                                    FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._i_cover = 2;
                                    //Thread.Sleep(2000);
                                    Communal._fadmSqlserver.ReviseData("Update  cup_details set CoverStatus = 1,Cooperate=0 where CupNum = " + i_cupNo);
                                    //Communal._fadmSqlserver.ReviseData("Update  cup_details set Cooperate=0 where Cooperate = 5 and CupNum = " + i_cupNo);

                                    FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._i_cupCover = 1;
                                }
                                else if (i_type == 22)
                                {
                                    //什么都不需要做，只是提示
                                }
                                else if (i_type == 64)
                                {
                                    //人工确定洗瓶工位有瓶
                                    //发送执行完成
                                    int[] ia_zero = new int[1];
                                    ia_zero[0] = 8;
                                    //开料工位取瓶完成
                                    Dye.BrewAndWashHMIWrite(22289 + 40 * (i_cupNum - 1), ia_zero);
                                    //洗瓶工位放瓶完成
                                    ia_zero[0] = 2;
                                    Dye.BrewAndWashHMIWrite(22891, ia_zero);
                                    //修改开料工位申请
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                             "UPDATE brew_station_details SET Cooperate = 0  WHERE Station = " + i_cupNum + ";");
                                    //修改洗杯工位申请
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                             "UPDATE brew_station_details SET Cooperate = 0  WHERE Station = 6;");
                                }
                                else if (i_type == 65)
                                {
                                    //人工确定开料工位有瓶
                                    //发送执行完成
                                    int[] ia_zero = new int[1];
                                    //发送执行完成
                                    ia_zero[0] = 8;
                                    //开料工位放瓶完成
                                    Dye.BrewAndWashHMIWrite(22289 + 40 * (i_cupNum - 1), ia_zero);
                                    //洗瓶工位取瓶完成
                                    ia_zero[0] = 2;
                                    Dye.BrewAndWashHMIWrite(22891, ia_zero);
                                    //修改开料工位申请
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                             "UPDATE brew_station_details SET Cooperate = 0,Statues='运行中'  WHERE Station = " + i_cupNum + ";");
                                    //修改洗杯工位申请
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                             "UPDATE brew_station_details SET Cooperate = 0,Statues='待机'  WHERE Station = 6;");
                                }
                                else if (i_type == 67)
                                {
                                    //人工确认已拿走存针位置针筒

                                    //修改开料工位申请
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                             "UPDATE brew_station_details SET Cooperate = 13,WashSyringesFinish=0  WHERE Station = " + i_cupNum + ";");
                                }
                                //7滴液过程加水在外边处理

                            }

                        }

                        //if (messageBoxButtons == MessageBoxButtons.YesNo)
                        {
                            //写进数据库
                            string s_sql = "INSERT INTO alarm_table" +
                                         "(MyDate,MyTime,AlarmHead,AlarmDetails)" +
                                         " VALUES( '" +
                                         String.Format("{0:d}", DateTime.Now) + "','" +
                                         String.Format("{0:T}", DateTime.Now) + "','SwitchCover','" +
                                         s_text + "(" + (_i_alarm_Choose == 1 ? "Yes" : "No") + ")');";

                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                        }

                        Lib_Card.CardObject.DeleteD(s_insert);


                        //buzzer.Buzzer_Off();





                    });
                    thread.Start();
                }
                catch
                {
                    goto label1;
                }
            }
        }

        public MyAlarm(string s_text, string s_caption, int i_cupNum, int i_req, int i_type, bool b_repeat)
        {
            if (!string.IsNullOrEmpty(s_text))
            {
            label1:
                try
                {
                    Thread thread = new Thread(() =>
                    {


                        string s_insert = Lib_Card.CardObject.InsertCF(s_text, s_caption,1);
                        if (s_insert == "重复")
                        {
                            //_i_alarm_Choose = 1;
                            return;
                        }
                        else if (s_insert == "重复1")
                        {
                            if (i_type == 4)
                            {
                                // //修改为加药
                                // FADM_Object.Communal._fadmSqlserver.ReviseData(
                                //"UPDATE dye_details SET  Cooperate = 1 WHERE  Cooperate = " + 9 + " AND CupNum = " + i_cupNum + " ;");

                                //重新置0，等待获取开盖申请
                                FADM_Auto.Dye._cup_Temps[i_cupNum - 1]._i_cover = 0;

                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE cup_details SET  Cooperate = 0 WHERE  CupNum = " + i_cupNum + " ;");

                                _i_alarm_Repeat = 1;
                                return;

                            }
                            else if (i_type == 5)
                            {
                                //修改为流程加水
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE dye_details SET  Cooperate = 3 WHERE  Cooperate = " + 9 + " AND CupNum = " + i_cupNum + " ;");

                                _i_alarm_Repeat = 1;
                                return;
                            }
                            else if (i_type == 6)
                            {
                                //修改为洗杯加水
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE cup_details SET  Cooperate = 4 WHERE  CupNum = " + i_cupNum + " ;");

                                _i_alarm_Repeat = 1;
                                return;
                            }
                            else if (i_type == 7)
                            {
                                //滴液加水
                                _i_alarm_Repeat = 1;
                                return;
                            }
                            else if (i_type == 8)
                            {
                                //修改为放布
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE cup_details SET  Cooperate = 8 WHERE  CupNum = " + i_cupNum + " ;");

                                _i_alarm_Repeat = 1;
                                return;
                            }
                            else if (i_type == 9)
                            {
                                //修改为出布
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE cup_details SET  Cooperate = 9 WHERE  CupNum = " + i_cupNum + " ;");

                                _i_alarm_Repeat = 1;
                                return;
                            }
                            else if (i_type == 10)
                            {
                                //预滴液数值太小
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE drop_details SET  MinWeight = 0 WHERE  MinWeight = " + 4 + " AND BottleNum = " + i_cupNum + " ;");

                                _i_alarm_Repeat = 1;
                                return;
                            }
                            else if (i_type == 22)
                            {
                                //什么都不需要做，只是提示
                            }
                            else if (i_type == 51)
                            {
                                //修改为放瓶
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE brew_station_details SET  Cooperate = 2 WHERE  Station = 6 and Cooperate = 11;");

                                _i_alarm_Repeat = 1;
                                return;
                            }
                            else if (i_type == 52)
                            {
                                //修改为测PH
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE cup_details SET  Cooperate = 12 WHERE  CupNum = " + i_cupNum + " ;");

                                _i_alarm_Repeat = 1;
                                return;
                            }

                            else if (i_type == 53)
                            {
                                //修改为加粉
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE brew_station_details SET  Cooperate = 3 WHERE  Station = " + i_cupNum + "  and Cooperate = 11;");

                                _i_alarm_Repeat = 1;
                                return;
                            }

                            else if (i_type == 54)
                            {
                                //修改为洗瓶工位取瓶
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE brew_station_details SET  Cooperate = 3 WHERE  Station = " + 6 + "  and Cooperate = 11;");

                                _i_alarm_Repeat = 1;
                                return;
                            }
                            else if (i_type == 55)
                            {
                                //修改为洗瓶工位放瓶瓶
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE brew_station_details SET  Cooperate = 2 WHERE  Station = " + 6 + "  and Cooperate = 11;");

                                _i_alarm_Repeat = 1;
                                return;
                            }

                            else if (i_type == 56)
                            {
                                //修改为开料完成取瓶
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE brew_station_details SET  Cooperate = 4 WHERE  Station = " + i_cupNum + "  and Cooperate = 11;");

                                _i_alarm_Repeat = 1;
                                return;
                            }

                            else if (i_type == 57)
                            {
                                //修改为开料前洗针
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE brew_station_details SET  Cooperate = 13 WHERE  Station = " + i_cupNum + "  and Cooperate = 11;");

                                _i_alarm_Repeat = 1;
                                return;
                            }
                            else if (i_type == 58)
                            {
                                //修改为开稀添加母液
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE brew_station_details SET  Cooperate = 6 WHERE  Station = " + i_cupNum + "  and Cooperate = 11;");

                                _i_alarm_Repeat = 1;
                                return;
                            }

                            else if (i_type == 61)
                            {
                                //修改为重新移动针筒
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE brew_station_details SET  Cooperate = 14 WHERE  Station = " + i_cupNum + "  and Cooperate = 11;");

                                _i_alarm_Repeat = 1;
                                return;
                            }

                            else if (i_type == 62)
                            {
                                //修改为重新开料完成取瓶
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE brew_station_details SET  Cooperate = 4 WHERE  Station = " + i_cupNum + "  and Cooperate = 11;");

                                _i_alarm_Repeat = 1;
                                return;
                            }
                            else if (i_type == 63)
                            {
                                //修改为洗瓶工位放瓶
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE brew_station_details SET  Cooperate = 2 WHERE  Station = 6  and Cooperate = 11;");

                                _i_alarm_Repeat = 1;
                                return;
                            }
                            else if (i_type == 66)
                            {
                                //修改为洗瓶工位取瓶
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE brew_station_details SET  Cooperate = 3 WHERE  Station = 6  and Cooperate = 11;");

                                _i_alarm_Repeat = 1;
                                return;
                            }
                            else
                            {
                                FADM_Auto.Dye._cup_Temps[i_cupNum - 1]._i_cover = 0;

                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE cup_details SET  Cooperate = 0 WHERE  CupNum = " + i_cupNum + " ;");

                                _i_alarm_Repeat = 1;
                                return;
                            }
                        }
                        else
                        {

                            if (FADM_Object.Communal._b_isNetWork)
                            {
                                inBroadcastW(FADM_Object.Communal._s_machineCode, s_text, s_insert, true);
                            }

                        }

                        //Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer buzzer = new Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer_Basic();
                        //buzzer.Buzzer_On();
                        while (true)
                        {
                            Thread.Sleep(1);
                            if (Lib_Card.CardObject.keyValuePairs[s_insert].Choose != 0)
                                break;
                            if (FADM_Object.Communal._b_isNetWork)
                            {
                                getBroadcastRe(FADM_Object.Communal._s_machineCode, s_insert, s_insert);
                                Thread.Sleep(1000);
                            }
                        }

                        _i_alarm_Choose = Lib_Card.CardObject.keyValuePairs[s_insert].Choose;

                        //正常重复执行，重置等待下一次轮询
                        if (i_req == 1)
                        {

                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE cup_details SET  Cooperate = 0 WHERE  CupNum = " + i_cupNum + " ;");

                        }
                        //选择继续执行或者已完成开关盖
                        else
                        {
                            //如果选择是，就继续
                            if (_i_alarm_Choose == 1)
                            {
                                if (i_type == 4)
                                {
                                    // //修改为加药
                                    // FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    //"UPDATE dye_details SET  Cooperate = 1 WHERE  Cooperate = " + 9 + " AND CupNum = " + i_cupNum + " ;");

                                    //重新置0，等待获取开盖申请
                                    FADM_Auto.Dye._cup_Temps[i_cupNum - 1]._i_cover = 0;

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE cup_details SET  Cooperate = 0 WHERE  CupNum = " + i_cupNum + " ;");

                                }
                                else if (i_type == 5)
                                {
                                    //修改为流程加水
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE dye_details SET  Cooperate = 3 WHERE  Cooperate = " + 9 + " AND CupNum = " + i_cupNum + " ;");
                                }
                                else if (i_type == 6)
                                {
                                    //修改为洗杯加水
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE cup_details SET  Cooperate = 4 WHERE  CupNum = " + i_cupNum + " ;");
                                }
                                else if (i_type == 7)
                                {
                                    //滴液加水
                                }
                                else if (i_type == 8)
                                {
                                    //修改为放布
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE cup_details SET  Cooperate = 8 WHERE  CupNum = " + i_cupNum + " ;");
                                }
                                else if (i_type == 9)
                                {
                                    //修改为出布
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE cup_details SET  Cooperate = 9 WHERE  CupNum = " + i_cupNum + " ;");
                                }
                                else if (i_type == 10)
                                {
                                    //预滴液数值太小
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE drop_details SET  MinWeight = 0 WHERE  MinWeight = " + 4 + " AND BottleNum = " + i_cupNum + " ;");

                                    _i_alarm_Repeat = 1;
                                }
                                else if (i_type == 12)
                                {
                                    //关盖失败，已把盖子放到杯中，要人工检查是否关盖正常，点击是就直接完成关盖，继续运行

                                    //复位加药启动信号
                                    int[] ia_zero = new int[1];
                                    //
                                    int i_cupNo = i_cupNum;
                                    ia_zero[0] = 0;

                                    FADM_Auto.Dye.DyeOpenOrCloseCover(i_cupNo, 1);

                                    FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._i_cover = 2;
                                    //Thread.Sleep(2000);
                                    Communal._fadmSqlserver.ReviseData("Update  cup_details set CoverStatus = 1,Cooperate=0 where CupNum = " + i_cupNo);
                                    //Communal._fadmSqlserver.ReviseData("Update  cup_details set Cooperate=0 where Cooperate = 5 and CupNum = " + i_cupNo);

                                    FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._i_cupCover = 1;
                                }
                                else if (i_type == 22)
                                {
                                    //什么都不需要做，只是提示
                                }
                                else if (i_type == 51)
                                {
                                    //修改为放瓶
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE brew_station_details SET  Cooperate = 2 WHERE  Station = 6 and Cooperate = 11;");
                                }
                                else if (i_type == 52)
                                {
                                    //修改测PH
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE cup_details SET  Cooperate = 12 WHERE  CupNum = " + i_cupNum + " ;");

                                }
                                else if (i_type == 53)
                                {
                                    //修改为加粉
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE brew_station_details SET  Cooperate = 3 WHERE  Station = " + i_cupNum + "  and Cooperate = 11;");

                                }
                                else if (i_type == 54)
                                {
                                    //修改为取瓶
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE brew_station_details SET  Cooperate = 3 WHERE  Station = " + 6 + "  and Cooperate = 11;");

                                    _i_alarm_Repeat = 1;
                                }
                                else if (i_type == 55)
                                {
                                    //修改为洗瓶工位放瓶瓶
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE brew_station_details SET  Cooperate = 2 WHERE  Station = " + 6 + "  and Cooperate = 11;");

                                }
                                else if (i_type == 56)
                                {
                                    //修改为开料完成取瓶
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE brew_station_details SET  Cooperate = 4 WHERE  Station = " + i_cupNum + "  and Cooperate = 11;");

                                }
                                else if (i_type == 57)
                                {
                                    //修改为开料前洗针
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE brew_station_details SET  Cooperate = 13 WHERE  Station = " + i_cupNum + "  and Cooperate = 11;");

                                }
                                else if (i_type == 58)
                                {
                                    //修改为开稀添加母液
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE brew_station_details SET  Cooperate = 6 WHERE  Station = " + i_cupNum + "  and Cooperate = 11;");

                                }
                                else if (i_type == 61)
                                {
                                    //修改为重新移动针筒
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE brew_station_details SET  Cooperate = 14 WHERE  Station = " + i_cupNum + "  and Cooperate = 11;");

                                }
                                else if (i_type == 62)
                                {
                                    //修改为重新开料完成取瓶
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE brew_station_details SET  Cooperate = 4 WHERE  Station = " + i_cupNum + "  and Cooperate = 11;");

                                }
                                else if (i_type == 63)
                                {
                                    //修改为洗瓶工位放瓶
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE brew_station_details SET  Cooperate = 2 WHERE  Station = 6  and Cooperate = 11;");

                                }
                                else if (i_type == 66)
                                {
                                    //修改为洗瓶工位取瓶
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE brew_station_details SET  Cooperate = 3 WHERE  Station = 6  and Cooperate = 11;");

                                }
                                else
                                {
                                    FADM_Auto.Dye._cup_Temps[i_cupNum - 1]._i_cover = 0;

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE cup_details SET  Cooperate = 0 WHERE  CupNum = " + i_cupNum + " ;");
                                }
                            }
                            //选择否就直接置杯盖状态
                            else
                            {
                                //关盖完成
                                if (i_type == 1)
                                {
                                    //复位加药启动信号
                                    int[] ia_zero = new int[1];
                                    //
                                    int i_cupNo = i_cupNum;
                                    ia_zero[0] = 0;

                                    FADM_Auto.Dye.DyeOpenOrCloseCover(i_cupNo, 1);

                                    FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._i_cover = 2;
                                    //Thread.Sleep(2000);
                                    Communal._fadmSqlserver.ReviseData("Update  cup_details set CoverStatus = 1,Cooperate=0 where CupNum = " + i_cupNo);
                                    //Communal._fadmSqlserver.ReviseData("Update  cup_details set Cooperate=0 where Cooperate = 5 and CupNum = " + i_cupNo);

                                    FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._i_cupCover = 1;
                                }
                                //开盖完成
                                else if (i_type == 2 || i_type == 5 || i_type == 4 || i_type == 6 || i_type == 8 || i_type == 9 || i_type == 10 || i_type == 44)
                                {
                                    //
                                    int iCupNo = i_cupNum;

                                    if (i_type == 2 || i_type == 44)
                                    {
                                        FADM_Auto.Dye.DyeOpenOrCloseCover(iCupNo, 2);

                                        FADM_Auto.Dye._cup_Temps[iCupNo - 1]._i_cover = 2;
                                        //Thread.Sleep(2000);
                                        Communal._fadmSqlserver.ReviseData("Update  cup_details set CoverStatus = 2,Cooperate=0 where CupNum = " + iCupNo);

                                        FADM_Auto.Dye._cup_Temps[iCupNo - 1]._i_cupCover = 2;
                                    }

                                    //加药
                                    else if (i_type == 4)
                                    {
                                        //     FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        //"UPDATE dye_details SET  Cooperate = 1 WHERE  Cooperate = " + 9 + " AND CupNum = " + i_cupNum + " ;");

                                        //重新置0，等待获取开盖申请
                                        FADM_Auto.Dye._cup_Temps[i_cupNum - 1]._i_cover = 0;

                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                           "UPDATE cup_details SET  Cooperate = 0 WHERE  CupNum = " + i_cupNum + " ;");
                                    }
                                    //流程加水
                                    else if (i_type == 5)
                                    {
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE dye_details SET  Cooperate = 3 WHERE  Cooperate = " + 9 + " AND CupNum = " + i_cupNum + " ;");
                                    }
                                    //洗杯加水
                                    else if (i_type == 6)
                                    {
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE cup_details SET  Cooperate = 4 WHERE  CupNum = " + i_cupNum + " ;");
                                    }
                                    else if (i_type == 8)
                                    {
                                        //修改为放布
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                           "UPDATE cup_details SET  Cooperate = 8 WHERE  CupNum = " + i_cupNum + " ;");
                                    }
                                    else if (i_type == 9)
                                    {
                                        //修改为出布
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                           "UPDATE cup_details SET  Cooperate = 9 WHERE  CupNum = " + i_cupNum + " ;");
                                    }
                                    else if (i_type == 10)
                                    {
                                        //预滴液数值太小
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE drop_details SET  MinWeight = 0 WHERE  MinWeight = " + 4 + " AND BottleNum = " + i_cupNum + " ;");

                                        _i_alarm_Repeat = 1;
                                    }
                                    

                                }
                                //无锁止信号操作
                                else if (i_type == 3)
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE cup_details SET  Cooperate = 0 WHERE  CupNum = " + i_cupNum + " ;");
                                }
                                else if (i_type == 13 || i_type == 14)
                                {
                                    FADM_Auto.Dye._cup_Temps[i_cupNum - 1]._i_cover = 0;

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE cup_details SET  Cooperate = 0 WHERE  CupNum = " + i_cupNum + " ;");
                                }
                                else if (i_type == 12)
                                {
                                    //关盖失败，已把盖子放到杯中，要人工检查是否关盖正常，点击否也直接完成关盖，继续运行

                                    //复位加药启动信号
                                    int[] ia_zero = new int[1];
                                    //
                                    int i_cupNo = i_cupNum;
                                    ia_zero[0] = 0;

                                    FADM_Auto.Dye.DyeOpenOrCloseCover(i_cupNo, 1);

                                    FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._i_cover = 2;
                                    //Thread.Sleep(2000);
                                    Communal._fadmSqlserver.ReviseData("Update  cup_details set CoverStatus = 1,Cooperate=0 where CupNum = " + i_cupNo);
                                    //Communal._fadmSqlserver.ReviseData("Update  cup_details set Cooperate=0 where Cooperate = 5 and CupNum = " + i_cupNo);

                                    FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._i_cupCover = 1;
                                }
                                else if (i_type == 22)
                                {
                                    //什么都不需要做，只是提示
                                }
                                else if (i_type == 51)
                                {
                                    //修改为放瓶
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE brew_station_details SET  Cooperate = 2 WHERE  Station = 6 and Cooperate = 11;");

                                }
                                else if (i_type == 52)
                                {
                                    //修改测PH
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE cup_details SET  Cooperate = 12 WHERE  CupNum = " + i_cupNum + " ;");

                                }
                                else if (i_type == 53)
                                {
                                    //修改为加粉
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE brew_station_details SET  Cooperate = 3 WHERE  Station = " + i_cupNum + "  and Cooperate = 11;");

                                }
                                else if (i_type == 54)
                                {
                                    //修改为取瓶
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE brew_station_details SET  Cooperate = 3 WHERE  Station = " + 6 + "  and Cooperate = 11;");

                                }
                                else if (i_type == 55)
                                {
                                    //修改为洗瓶工位放瓶瓶
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE brew_station_details SET  Cooperate = 2 WHERE  Station = " + 6 + "  and Cooperate = 11;");

                                }
                                else if (i_type == 56)
                                {
                                    //修改为开料完成取瓶
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE brew_station_details SET  Cooperate = 4 WHERE  Station = " + i_cupNum + "  and Cooperate = 11;");

                                }
                                else if (i_type == 57)
                                {
                                    //修改为开料前洗针
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE brew_station_details SET  Cooperate = 13 WHERE  Station = " + i_cupNum + "  and Cooperate = 11;");

                                }
                                else if (i_type == 58)
                                {
                                    //修改为开稀添加母液
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE brew_station_details SET  Cooperate = 6 WHERE  Station = " + i_cupNum + "  and Cooperate = 11;");

                                }
                                else if (i_type == 61)
                                {
                                    //修改为重新移动针筒
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE brew_station_details SET  Cooperate = 14 WHERE  Station = " + i_cupNum + "  and Cooperate = 11;");

                                }
                                else if (i_type == 62)
                                {
                                    //修改为重新开料完成取瓶
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE brew_station_details SET  Cooperate = 4 WHERE  Station = " + i_cupNum + "  and Cooperate = 11;");

                                }
                                else if (i_type == 63)
                                {
                                    //修改为洗瓶工位放瓶
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE brew_station_details SET  Cooperate = 2 WHERE  Station = 6  and Cooperate = 11;");

                                }
                                else if (i_type == 66)
                                {
                                    //修改为洗瓶工位取瓶
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE brew_station_details SET  Cooperate = 3 WHERE  Station = 6  and Cooperate = 11;");

                                }
                                //7滴液过程加水在外边处理

                            }

                        }

                        //if (messageBoxButtons == MessageBoxButtons.YesNo)
                        {
                            //写进数据库
                            string s_sql = "INSERT INTO alarm_table" +
                                         "(MyDate,MyTime,AlarmHead,AlarmDetails)" +
                                         " VALUES( '" +
                                         String.Format("{0:d}", DateTime.Now) + "','" +
                                         String.Format("{0:T}", DateTime.Now) + "','SwitchCover','" +
                                         s_text + "(" + (_i_alarm_Choose == 1 ? "Yes" : "No") + ")');";

                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                        }

                        Lib_Card.CardObject.DeleteD(s_insert);


                        //buzzer.Buzzer_Off();





                    });
                    thread.Start();
                }
                catch
                {
                    goto label1;
                }
            }
        }

        private void kill()
        {
            IntPtr ptr = FindWindow(null, "温馨提示");
            if (ptr != IntPtr.Zero)
            {
                //找到则关闭MessageBox窗口  

                PostMessage(ptr, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);


            }
        }


        public MyAlarm(string s_text, string s_caption, bool b_choose, int i_speech)
        {
            if (!string.IsNullOrEmpty(s_text))
            {
            label1:
                try
                {
                    //Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer buzzer = new Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer_Basic();
                    // buzzer.Buzzer_On();

                    string s_insert = Lib_Card.CardObject.InsertCF(s_text, s_caption);
                    string s_errcode = "";
                    JObject obj = null;
                    if (s_insert == "重复")
                    {
                        return;
                    }
                    else if (s_insert == "重复1")
                    {
                        if (b_choose)
                        {
                            //休眠1秒，等待上一个消除(原因是因为有可能已做选择，但由于没播报导致待办没消除)
                            Thread.Sleep(1000);
                            goto label1;
                        }
                    }
                    else
                    {
                        if (FADM_Object.Communal._b_isNetWork)
                        {
                            //插入微信播报表
                            inBroadcastW(FADM_Object.Communal._s_machineCode, s_text, s_insert, true);
                           
                        }
                    }

                    while (true)   //new对象  等待当前是否结果
                    {
                        Thread.Sleep(1);
                        if (Lib_Card.CardObject.keyValuePairs[s_insert].Choose != 0)
                            break;
                        if (FADM_Object.Communal._b_isNetWork)
                        {
                            //发起请求查询 
                            getBroadcastRe(FADM_Object.Communal._s_machineCode, s_insert, s_insert);
                            Thread.Sleep(1000);
                        }
                    }

                    _i_alarm_Choose = Lib_Card.CardObject.keyValuePairs[s_insert].Choose;

                    //写进数据库
                    string s_sql = "INSERT INTO alarm_table" +
                                 "(MyDate,MyTime,AlarmHead,AlarmDetails)" +
                                 " VALUES( '" +
                                 String.Format("{0:d}", DateTime.Now) + "','" +
                                 String.Format("{0:T}", DateTime.Now) + "','" +
                                 s_caption + "','" +
                                 s_text + "(" + (_i_alarm_Choose == 1 ? "Yes" : "No") + ")');";

                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                    Lib_Card.CardObject.DeleteD(s_insert);


                    // buzzer.Buzzer_Off();
                }
                catch
                {
                    goto label1;
                }
            }
        }

        public MyAlarm(string s_text, string s_caption, bool b_choose, int i_speech, bool b_repeat)
        {
            if (!string.IsNullOrEmpty(s_text))
            {
            label1:
                try
                {
                    //Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer buzzer = new Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer_Basic();
                    // buzzer.Buzzer_On();

                    string s_insert = Lib_Card.CardObject.InsertCF(s_text, s_caption,1);
                    string s_errcode = "";
                    JObject obj = null;
                    if (s_insert == "重复")
                    {
                        return;
                    }
                    else if (s_insert == "重复1")
                    {
                        if (b_choose)
                        {
                            //休眠1秒，等待上一个消除(原因是因为有可能已做选择，但由于没播报导致待办没消除)
                            Thread.Sleep(1000);
                            goto label1;
                        }
                    }
                    else
                    {
                        if (FADM_Object.Communal._b_isNetWork)
                        {
                            //插入微信播报表
                            inBroadcastW(FADM_Object.Communal._s_machineCode, s_text, s_insert, true);

                        }
                    }

                    while (true)   //new对象  等待当前是否结果
                    {
                        Thread.Sleep(1);
                        if (Lib_Card.CardObject.keyValuePairs[s_insert].Choose != 0)
                            break;
                        if (FADM_Object.Communal._b_isNetWork)
                        {
                            //发起请求查询 
                            getBroadcastRe(FADM_Object.Communal._s_machineCode, s_insert, s_insert);
                            Thread.Sleep(1000);
                        }
                    }

                    _i_alarm_Choose = Lib_Card.CardObject.keyValuePairs[s_insert].Choose;

                    //写进数据库
                    string s_sql = "INSERT INTO alarm_table" +
                                 "(MyDate,MyTime,AlarmHead,AlarmDetails)" +
                                 " VALUES( '" +
                                 String.Format("{0:d}", DateTime.Now) + "','" +
                                 String.Format("{0:T}", DateTime.Now) + "','" +
                                 s_caption + "','" +
                                 s_text + "(" + (_i_alarm_Choose == 1 ? "Yes" : "No") + ")');";

                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                    Lib_Card.CardObject.DeleteD(s_insert);


                    // buzzer.Buzzer_Off();
                }
                catch
                {
                    goto label1;
                }
            }
        }

        async public static void inBroadcastW(string machineCode, string Text, string time, params bool[] isFlag)
        {
            Console.WriteLine("插入播报表==============");
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("machineCode", machineCode);
            parameters.Add("Text", Text);
            parameters.Add("time", time);
            if (isFlag != null && isFlag.Length > 0 && isFlag[0])
            {
                parameters.Add("isFlag", "True");
            }
            await Task.Run(() => {
                try {
                    HttpWebResponse response = HttpUtil.CreatePostHttpResponse(FADM_Object.Communal.URL + "/outer/product/inBroadcastW", parameters, 15000, null, null);
                    response.GetResponseStream();
                } catch (Exception ex) { 
                }
                
            });
            /*return await Task.Run(() => {
                // htt ps://www.gz-kelian.com/outer/product/inBroadcastW  h ttp://192.168.144.105:8080/outer/product/inBroadcastW
                */
            //});
        }

        async public static void getBroadcastRe(string machineCode, string time, string s)
        {
            try
            {
                Console.WriteLine("获取播报表结果状态==============");
                IDictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("machineCode", machineCode);
                parameters.Add("time", time);
                await Task.Run(() => {
                    // htt ps://www.gz-kelian.com/outer/product/inBroadcastW  h ttp://192.168.144.105:8080/outer/product/inBroadcastW
                    HttpWebResponse response = HttpUtil.CreatePostHttpResponse(FADM_Object.Communal.URL + "/outer/product/getBroadcastRe", parameters, 15000, null, null);
                    Stream st = response.GetResponseStream();
                    StreamReader reader = new StreamReader(st);
                    string msg = reader.ReadToEnd();
                    Console.WriteLine("获取播报表结果结果" + msg);
                    JObject obj = JObject.Parse(msg);
                    if (obj["istrue"].Value<string>().Equals("true"))
                    {
                        Lib_Card.CardObject.prompt prompt = new Lib_Card.CardObject.prompt();
                        prompt = Lib_Card.CardObject.keyValuePairs[s];
                        prompt.Choose = Convert.ToInt32(obj["state"].Value<string>());
                        Lib_Card.CardObject.keyValuePairs[s] = prompt;
                    }
                });
            }
            catch
            {

            }
        }

        #endregion

        public MyAlarm(string s_text, int i_bottleNo, int i_oldSign, MessageBoxButtons messageBoxButtons)
        {
        label1:
            try
            {
                if (!string.IsNullOrEmpty(s_text))
                {
                    Thread thread = new Thread(() =>
                    {
                        string s_insert = Lib_Card.CardObject.InsertCF(s_text, "AddMedicine");
                        if (s_insert == "重复")
                        {
                            //FADM_Object.Communal._fadmSqlserver.ReviseData(
                            //   "UPDATE dye_details SET  Cooperate = 1 WHERE  Cooperate = " + i_oldSign + " AND BottleNum = " + i_bottleNo + " ;");
                            return;
                        }
                        else if (s_insert == "重复1")
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE dye_details SET  Cooperate = 1 WHERE  Cooperate = " + i_oldSign + " AND BottleNum = " + i_bottleNo + " ;");

                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE dye_details SET  Choose = 0 WHERE  Choose = 2 AND BottleNum = " + i_bottleNo + " ;");
                            return;
                        }
                        else {

                            if (s_text != "右光幕遮挡,请离开光幕" && s_text != "Right light curtain obstruction,Please step away from the light curtain"
                        && s_text != "左光幕遮挡,请离开光幕" && s_text != "Left light curtain obstruction,Please step away from the light curtain"
                        && s_text != "前光幕遮挡,请离开光幕" && s_text != "Front light curtain obstruction,Please step away from the light curtain"
                        && s_text != "急停已按下,请打开急停开关" && s_text != "Emergency stop pressed,Please turn on the emergency stop switch"
                        && s_text != "左门已打开,请关闭左门" && s_text != "The left door is open,Please close the left door"
                        && s_text != "右门已打开,请关闭右门" && s_text != "The right door is open,Please close the right door"
                        && s_text != "后光幕遮挡,请离开光幕" && s_text != "Back light curtain obstruction,Please step away from the light curtain"
                        && FADM_Object.Communal._b_isNetWork)
                            {
                                //插入微信播报表 推送微信 isBroadcastW为真 保存表   isBroadcastW为假 不保存表   
                                //插入微信播报表
                                inBroadcastW(FADM_Object.Communal._s_machineCode, s_text, s_insert, true);
                            }
                        }

                        //Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer buzzer = new Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer_Basic();
                        //buzzer.Buzzer_On();


                        while (true)
                        {
                            Thread.Sleep(1);
                            if (Lib_Card.CardObject.keyValuePairs[s_insert].Choose != 0)
                                break;

                            if (s_text != "右光幕遮挡,请离开光幕" && s_text != "Right light curtain obstruction,Please step away from the light curtain"
                        && s_text != "左光幕遮挡,请离开光幕" && s_text != "Left light curtain obstruction,Please step away from the light curtain"
                        && s_text != "前光幕遮挡,请离开光幕" && s_text != "Front light curtain obstruction,Please step away from the light curtain"
                        && s_text != "急停已按下,请打开急停开关" && s_text != "Emergency stop pressed,Please turn on the emergency stop switch"
                        && s_text != "左门已打开,请关闭左门" && s_text != "The left door is open,Please close the left door"
                        && s_text != "右门已打开,请关闭右门" && s_text != "The right door is open,Please close the right door"
                        && s_text != "后光幕遮挡,请离开光幕" && s_text != "Back light curtain obstruction,Please step away from the light curtain"
                        && FADM_Object.Communal._b_isNetWork)
                            {
                                //发起请求查询 
                                getBroadcastRe(FADM_Object.Communal._s_machineCode, s_insert, s_insert);
                                Thread.Sleep(1000);
                            }

                        }

                        _i_alarm_Choose = Lib_Card.CardObject.keyValuePairs[s_insert].Choose;
                        if (messageBoxButtons == MessageBoxButtons.YesNo)
                        {
                            if (Lib_Card.CardObject.keyValuePairs[s_insert].Choose == 2)
                            {
                                //选择否
                                Dye._dic_keyValue[i_bottleNo] = true;

                            }
                            else
                            {
                                if (i_oldSign == 5 || i_oldSign == 6)
                                {
                                    //如果点击是,在液量低和过期选项下，更新母液瓶数据，删除备料记录
                                    string s_sqlpre = "SELECT * FROM pre_brew WHERE  BottleNum = " + i_bottleNo + ";";
                                    DataTable dt_pre_brew = FADM_Object.Communal._fadmSqlserver.GetData(s_sqlpre);
                                    if (dt_pre_brew.Rows.Count > 0)
                                    {

                                        string s_sql = "UPDATE bottle_details SET RealConcentration = '" + dt_pre_brew.Rows[0]["RealConcentration"].ToString() + "',CurrentWeight = '" + dt_pre_brew.Rows[0]["CurrentWeight"].ToString() + "',BrewingData='"
                                            + dt_pre_brew.Rows[0]["BrewingData"].ToString() + "'" +
                                        " WHERE BottleNum = " + i_bottleNo + ";";
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                        FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from pre_brew where BottleNum = " + i_bottleNo);
                                    }
                                }
                            }
                        }
                        if (Lib_Card.CardObject.keyValuePairs[s_insert].Choose == 1)
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE dye_details SET  Cooperate = 1 WHERE  Cooperate = " + i_oldSign + " AND BottleNum = " + i_bottleNo + " ;");

                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE dye_details SET  Choose = 0 WHERE  Choose = 2 AND BottleNum = " + i_bottleNo + " ;");
                        }
                        else
                        {
                            if (!FADM_Object.Communal._b_isOutDripAllow)
                            {
                                //判断是否允许滴液,重新查询，看看是否过期
                                //判断当前母液瓶液量是否足够
                                string s_sql = "SELECT bottle_details.*,assistant_details.AllowMinColoringConcentration,assistant_details.AllowMaxColoringConcentration," +
                                    "assistant_details.TermOfValidity  " +
                                              "FROM bottle_details left join assistant_details on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE bottle_details.BottleNum = " + i_bottleNo + ";";
                                DataTable dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                                int i_adjust = Convert.ToInt32(dt_temp.Rows[0]["AdjustValue"]);
                                bool b_checkSuccess = (Convert.ToString(dt_temp.Rows[0]["AdjustSuccess"]) == "1");
                                string s_syringeType = Convert.ToString(dt_temp.Rows[0]["SyringeType"]);

                                double d_blCurrentWeight = Convert.ToDouble(dt_temp.Rows[0]["CurrentWeight"]);

                                double d_blCompCoefficient = Convert.ToDouble(dt_temp.Rows[0]["AllowMinColoringConcentration"]);
                                double d_blCompConstant = Convert.ToDouble(dt_temp.Rows[0]["AllowMaxColoringConcentration"]);
                                string s_termOfValidity = dt_temp.Rows[0]["TermOfValidity"].ToString();

                                DateTime timeA = Convert.ToDateTime(dt_temp.Rows[0]["BrewingData"].ToString());
                                DateTime timeB = DateTime.Now; //获取当前时间
                                TimeSpan ts = timeB - timeA; //计算时间差
                                string s_time = ts.TotalHours.ToString(); //将时间差转换为小时
                                string s_time2 = ts.TotalMinutes.ToString();



                                if (Convert.ToDouble(s_time) > Convert.ToDouble(s_termOfValidity) && FADM_Object.Communal._b_isOutDrip)
                                {
                                    //还过期，就当选择是，重新报警
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE dye_details SET  Cooperate = 1 WHERE  Cooperate = " + i_oldSign + " AND BottleNum = " + i_bottleNo + " ;");

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                           "UPDATE dye_details SET  Choose = 0 WHERE  Choose = 2 AND BottleNum = " + i_bottleNo + " ;");
                                }
                                else
                                {
                                    //忽略过期等，直接操作
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE dye_details SET  Cooperate = 1,Choose = 1 WHERE  Cooperate = " + i_oldSign + " AND BottleNum = " + i_bottleNo + " ;");

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                           "UPDATE dye_details SET  Choose = 1 WHERE  Choose = 2 AND BottleNum = " + i_bottleNo + " ;");
                                }




                            }
                            else if (!FADM_Object.Communal._b_isLowDripAllow)
                            {

                                //判断是否允许滴液,重新查询，看看是否过期
                                //判断当前母液瓶液量是否足够
                                string s_sql = "SELECT bottle_details.*,assistant_details.AllowMinColoringConcentration,assistant_details.AllowMaxColoringConcentration," +
                                    "assistant_details.TermOfValidity  " +
                                              "FROM bottle_details left join assistant_details on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE bottle_details.BottleNum = " + i_bottleNo + ";";
                                DataTable dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                                int i_adjust = Convert.ToInt32(dt_temp.Rows[0]["AdjustValue"]);
                                bool b_checkSuccess = (Convert.ToString(dt_temp.Rows[0]["AdjustSuccess"]) == "1");
                                string s_syringeType = Convert.ToString(dt_temp.Rows[0]["SyringeType"]);

                                double d_blCurrentWeight = Convert.ToDouble(dt_temp.Rows[0]["CurrentWeight"]);

                                double d_blCompCoefficient = Convert.ToDouble(dt_temp.Rows[0]["AllowMinColoringConcentration"]);
                                double d_blCompConstant = Convert.ToDouble(dt_temp.Rows[0]["AllowMaxColoringConcentration"]);
                                string s_termOfValidity = dt_temp.Rows[0]["TermOfValidity"].ToString();




                                if (d_blCurrentWeight <= Lib_Card.Configure.Parameter.Other_Bottle_MinWeight && FADM_Object.Communal._b_isLowDrip)
                                {
                                    //还过期，就当选择是，重新报警
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE dye_details SET  Cooperate = 1 WHERE  Cooperate = " + i_oldSign + " AND BottleNum = " + i_bottleNo + " ;");

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                           "UPDATE dye_details SET  Choose = 0 WHERE  Choose = 2 AND BottleNum = " + i_bottleNo + " ;");
                                }
                                else
                                {
                                    //忽略过期等，直接操作
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE dye_details SET  Cooperate = 1,Choose = 1 WHERE  Cooperate = " + i_oldSign + " AND BottleNum = " + i_bottleNo + " ;");

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                           "UPDATE dye_details SET  Choose = 1 WHERE  Choose = 2 AND BottleNum = " + i_bottleNo + " ;");
                                }
                            }
                            else
                            {
                                //忽略过期等，直接操作
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE dye_details SET  Cooperate = 1,Choose = 1 WHERE  Cooperate = " + i_oldSign + " AND BottleNum = " + i_bottleNo + " ;");

                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE dye_details SET  Choose = 1 WHERE  Choose = 2 AND BottleNum = " + i_bottleNo + " ;");
                            }
                        }
                        lock (Communal._dic_cup_bottle)
                        {
                            foreach (KeyValuePair<int, List<int>> kv in Communal._dic_cup_bottle)
                            {
                                while(kv.Value.Contains(i_bottleNo))
                                {
                                    kv.Value.Remove(i_bottleNo);
                                }
                            }
                        }

                        if (messageBoxButtons == MessageBoxButtons.YesNo)
                        {
                            //写进数据库
                            string s_sql = "INSERT INTO alarm_table" +
                                         "(MyDate,MyTime,AlarmHead,AlarmDetails)" +
                                         " VALUES( '" +
                                         String.Format("{0:d}", DateTime.Now) + "','" +
                                         String.Format("{0:T}", DateTime.Now) + "','AddMedicine','" +
                                         s_text + "(" + (_i_alarm_Choose == 1 ? "Yes" : "No") + ")');";

                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                        }
                        else
                        {
                            string s_sql = "INSERT INTO alarm_table" +
                                        "(MyDate,MyTime,AlarmHead,AlarmDetails)" +
                                        " VALUES( '" +
                                        String.Format("{0:d}", DateTime.Now) + "','" +
                                        String.Format("{0:T}", DateTime.Now) + "','AddMedicine','" +
                                        s_text + "(OK)');";

                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        }

                        Lib_Card.CardObject.DeleteD(s_insert);


                        //buzzer.Buzzer_Off();




                    });
                    thread.Start();
                }
            }
            catch
            {
                goto label1;
            }


        }

        public MyAlarm(string s_text, int i_bottleNo, int i_oldSign, MessageBoxButtons messageBoxButtons, bool b_repeat)
        {
        label1:
            try
            {
                if (!string.IsNullOrEmpty(s_text))
                {
                    Thread thread = new Thread(() =>
                    {
                        string s_insert = Lib_Card.CardObject.InsertCF(s_text, "AddMedicine",1);
                        if (s_insert == "重复")
                        {
                            //FADM_Object.Communal._fadmSqlserver.ReviseData(
                            //   "UPDATE dye_details SET  Cooperate = 1 WHERE  Cooperate = " + i_oldSign + " AND BottleNum = " + i_bottleNo + " ;");
                            return;
                        }
                        else if (s_insert == "重复1")
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE dye_details SET  Cooperate = 1 WHERE  Cooperate = " + i_oldSign + " AND BottleNum = " + i_bottleNo + " ;");

                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE dye_details SET  Choose = 0 WHERE  Choose = 2 AND BottleNum = " + i_bottleNo + " ;");
                            return;
                        }
                        else
                        {

                            if (s_text != "右光幕遮挡,请离开光幕" && s_text != "Right light curtain obstruction,Please step away from the light curtain"
                        && s_text != "左光幕遮挡,请离开光幕" && s_text != "Left light curtain obstruction,Please step away from the light curtain"
                        && s_text != "前光幕遮挡,请离开光幕" && s_text != "Front light curtain obstruction,Please step away from the light curtain"
                        && s_text != "急停已按下,请打开急停开关" && s_text != "Emergency stop pressed,Please turn on the emergency stop switch"
                        && s_text != "左门已打开,请关闭左门" && s_text != "The left door is open,Please close the left door"
                        && s_text != "右门已打开,请关闭右门" && s_text != "The right door is open,Please close the right door"
                        && s_text != "后光幕遮挡,请离开光幕" && s_text != "Back light curtain obstruction,Please step away from the light curtain"
                        && FADM_Object.Communal._b_isNetWork)
                            {
                                //插入微信播报表 推送微信 isBroadcastW为真 保存表   isBroadcastW为假 不保存表   
                                //插入微信播报表
                                inBroadcastW(FADM_Object.Communal._s_machineCode, s_text, s_insert, true);
                            }
                        }

                        //Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer buzzer = new Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer_Basic();
                        //buzzer.Buzzer_On();


                        while (true)
                        {
                            Thread.Sleep(1);
                            if (Lib_Card.CardObject.keyValuePairs[s_insert].Choose != 0)
                                break;

                            if (s_text != "右光幕遮挡,请离开光幕" && s_text != "Right light curtain obstruction,Please step away from the light curtain"
                        && s_text != "左光幕遮挡,请离开光幕" && s_text != "Left light curtain obstruction,Please step away from the light curtain"
                        && s_text != "前光幕遮挡,请离开光幕" && s_text != "Front light curtain obstruction,Please step away from the light curtain"
                        && s_text != "急停已按下,请打开急停开关" && s_text != "Emergency stop pressed,Please turn on the emergency stop switch"
                        && s_text != "左门已打开,请关闭左门" && s_text != "The left door is open,Please close the left door"
                        && s_text != "右门已打开,请关闭右门" && s_text != "The right door is open,Please close the right door"
                        && s_text != "后光幕遮挡,请离开光幕" && s_text != "Back light curtain obstruction,Please step away from the light curtain"
                        && FADM_Object.Communal._b_isNetWork)
                            {
                                //发起请求查询 
                                getBroadcastRe(FADM_Object.Communal._s_machineCode, s_insert, s_insert);
                                Thread.Sleep(1000);
                            }

                        }

                        _i_alarm_Choose = Lib_Card.CardObject.keyValuePairs[s_insert].Choose;
                        if (messageBoxButtons == MessageBoxButtons.YesNo)
                        {
                            if (Lib_Card.CardObject.keyValuePairs[s_insert].Choose == 2)
                            {
                                //选择否
                                Dye._dic_keyValue[i_bottleNo] = true;

                            }
                            else
                            {
                                if (i_oldSign == 5 || i_oldSign == 6)
                                {
                                    //如果点击是,在液量低和过期选项下，更新母液瓶数据，删除备料记录
                                    string s_sqlpre = "SELECT * FROM pre_brew WHERE  BottleNum = " + i_bottleNo + ";";
                                    DataTable dt_pre_brew = FADM_Object.Communal._fadmSqlserver.GetData(s_sqlpre);
                                    if (dt_pre_brew.Rows.Count > 0)
                                    {

                                        string s_sql = "UPDATE bottle_details SET RealConcentration = '" + dt_pre_brew.Rows[0]["RealConcentration"].ToString() + "',CurrentWeight = '" + dt_pre_brew.Rows[0]["CurrentWeight"].ToString() + "',BrewingData='"
                                            + dt_pre_brew.Rows[0]["BrewingData"].ToString() + "'" +
                                        " WHERE BottleNum = " + i_bottleNo + ";";
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                        FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from pre_brew where BottleNum = " + i_bottleNo);
                                    }
                                }
                            }
                        }
                        if (Lib_Card.CardObject.keyValuePairs[s_insert].Choose == 1)
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE dye_details SET  Cooperate = 1 WHERE  Cooperate = " + i_oldSign + " AND BottleNum = " + i_bottleNo + " ;");

                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE dye_details SET  Choose = 0 WHERE  Choose = 2 AND BottleNum = " + i_bottleNo + " ;");
                        }
                        else
                        {
                            if (!FADM_Object.Communal._b_isOutDripAllow)
                            {
                                //判断是否允许滴液,重新查询，看看是否过期
                                //判断当前母液瓶液量是否足够
                                string s_sql = "SELECT bottle_details.*,assistant_details.AllowMinColoringConcentration,assistant_details.AllowMaxColoringConcentration," +
                                    "assistant_details.TermOfValidity  " +
                                              "FROM bottle_details left join assistant_details on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE bottle_details.BottleNum = " + i_bottleNo + ";";
                                DataTable dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                                int i_adjust = Convert.ToInt32(dt_temp.Rows[0]["AdjustValue"]);
                                bool b_checkSuccess = (Convert.ToString(dt_temp.Rows[0]["AdjustSuccess"]) == "1");
                                string s_syringeType = Convert.ToString(dt_temp.Rows[0]["SyringeType"]);

                                double d_blCurrentWeight = Convert.ToDouble(dt_temp.Rows[0]["CurrentWeight"]);

                                double d_blCompCoefficient = Convert.ToDouble(dt_temp.Rows[0]["AllowMinColoringConcentration"]);
                                double d_blCompConstant = Convert.ToDouble(dt_temp.Rows[0]["AllowMaxColoringConcentration"]);
                                string s_termOfValidity = dt_temp.Rows[0]["TermOfValidity"].ToString();

                                DateTime timeA = Convert.ToDateTime(dt_temp.Rows[0]["BrewingData"].ToString());
                                DateTime timeB = DateTime.Now; //获取当前时间
                                TimeSpan ts = timeB - timeA; //计算时间差
                                string s_time = ts.TotalHours.ToString(); //将时间差转换为小时
                                string s_time2 = ts.TotalMinutes.ToString();



                                if (Convert.ToDouble(s_time) > Convert.ToDouble(s_termOfValidity) && FADM_Object.Communal._b_isOutDrip)
                                {
                                    //还过期，就当选择是，重新报警
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE dye_details SET  Cooperate = 1 WHERE  Cooperate = " + i_oldSign + " AND BottleNum = " + i_bottleNo + " ;");

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                           "UPDATE dye_details SET  Choose = 0 WHERE  Choose = 2 AND BottleNum = " + i_bottleNo + " ;");
                                }
                                else
                                {
                                    //忽略过期等，直接操作
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE dye_details SET  Cooperate = 1,Choose = 1 WHERE  Cooperate = " + i_oldSign + " AND BottleNum = " + i_bottleNo + " ;");

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                           "UPDATE dye_details SET  Choose = 1 WHERE  Choose = 2 AND BottleNum = " + i_bottleNo + " ;");
                                }




                            }
                            else if (!FADM_Object.Communal._b_isLowDripAllow)
                            {

                                //判断是否允许滴液,重新查询，看看是否过期
                                //判断当前母液瓶液量是否足够
                                string s_sql = "SELECT bottle_details.*,assistant_details.AllowMinColoringConcentration,assistant_details.AllowMaxColoringConcentration," +
                                    "assistant_details.TermOfValidity  " +
                                              "FROM bottle_details left join assistant_details on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE bottle_details.BottleNum = " + i_bottleNo + ";";
                                DataTable dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                                int i_adjust = Convert.ToInt32(dt_temp.Rows[0]["AdjustValue"]);
                                bool b_checkSuccess = (Convert.ToString(dt_temp.Rows[0]["AdjustSuccess"]) == "1");
                                string s_syringeType = Convert.ToString(dt_temp.Rows[0]["SyringeType"]);

                                double d_blCurrentWeight = Convert.ToDouble(dt_temp.Rows[0]["CurrentWeight"]);

                                double d_blCompCoefficient = Convert.ToDouble(dt_temp.Rows[0]["AllowMinColoringConcentration"]);
                                double d_blCompConstant = Convert.ToDouble(dt_temp.Rows[0]["AllowMaxColoringConcentration"]);
                                string s_termOfValidity = dt_temp.Rows[0]["TermOfValidity"].ToString();




                                if (d_blCurrentWeight <= Lib_Card.Configure.Parameter.Other_Bottle_MinWeight && FADM_Object.Communal._b_isLowDrip)
                                {
                                    //还过期，就当选择是，重新报警
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE dye_details SET  Cooperate = 1 WHERE  Cooperate = " + i_oldSign + " AND BottleNum = " + i_bottleNo + " ;");

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                           "UPDATE dye_details SET  Choose = 0 WHERE  Choose = 2 AND BottleNum = " + i_bottleNo + " ;");
                                }
                                else
                                {
                                    //忽略过期等，直接操作
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE dye_details SET  Cooperate = 1,Choose = 1 WHERE  Cooperate = " + i_oldSign + " AND BottleNum = " + i_bottleNo + " ;");

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                           "UPDATE dye_details SET  Choose = 1 WHERE  Choose = 2 AND BottleNum = " + i_bottleNo + " ;");
                                }
                            }
                            else
                            {
                                //忽略过期等，直接操作
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE dye_details SET  Cooperate = 1,Choose = 1 WHERE  Cooperate = " + i_oldSign + " AND BottleNum = " + i_bottleNo + " ;");

                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE dye_details SET  Choose = 1 WHERE  Choose = 2 AND BottleNum = " + i_bottleNo + " ;");
                            }
                        }
                        lock (Communal._dic_cup_bottle)
                        {
                            foreach (KeyValuePair<int, List<int>> kv in Communal._dic_cup_bottle)
                            {
                                while (kv.Value.Contains(i_bottleNo))
                                {
                                    kv.Value.Remove(i_bottleNo);
                                }
                            }
                        }

                        if (messageBoxButtons == MessageBoxButtons.YesNo)
                        {
                            //写进数据库
                            string s_sql = "INSERT INTO alarm_table" +
                                         "(MyDate,MyTime,AlarmHead,AlarmDetails)" +
                                         " VALUES( '" +
                                         String.Format("{0:d}", DateTime.Now) + "','" +
                                         String.Format("{0:T}", DateTime.Now) + "','AddMedicine','" +
                                         s_text + "(" + (_i_alarm_Choose == 1 ? "Yes" : "No") + ")');";

                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                        }
                        else
                        {
                            string s_sql = "INSERT INTO alarm_table" +
                                        "(MyDate,MyTime,AlarmHead,AlarmDetails)" +
                                        " VALUES( '" +
                                        String.Format("{0:d}", DateTime.Now) + "','" +
                                        String.Format("{0:T}", DateTime.Now) + "','AddMedicine','" +
                                        s_text + "(OK)');";

                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        }

                        Lib_Card.CardObject.DeleteD(s_insert);


                        //buzzer.Buzzer_Off();




                    });
                    thread.Start();
                }
            }
            catch
            {
                goto label1;
            }


        }

        public MyAlarm(string s_text, int i_cupNo, int i_oldSign)
        {
        label1:
            try
            {
                if (!string.IsNullOrEmpty(s_text))
                {
                    Thread thread = new Thread(() =>
                    {
                        string s_insert = Lib_Card.CardObject.InsertCF(s_text, "AbsAddMedicine");
                        if (s_insert == "重复")
                        {
                            //FADM_Object.Communal._fadmSqlserver.ReviseData(
                            //   "UPDATE dye_details SET  Cooperate = 1 WHERE  Cooperate = " + i_oldSign + " AND BottleNum = " + i_bottleNo + " ;");
                            return;
                        }
                        else if (s_insert == "重复1")
                        {
                            if (i_oldSign == 7 || i_oldSign == 9 || i_oldSign == 13 || i_oldSign == 14)
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE abs_cup_details SET  Cooperate = 33 WHERE  Cooperate = " + i_oldSign + " AND CupNum = " + i_cupNo + " ;");
                            else if (i_oldSign == 8 || i_oldSign == 10 || i_oldSign == 11 || i_oldSign == 12)
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE abs_cup_details SET  Cooperate = 32 WHERE  Cooperate = " + i_oldSign + " AND CupNum = " + i_cupNo + " ;");
                            return;
                        }
                        else {
                            if (s_text != "右光幕遮挡,请离开光幕" && s_text != "Right light curtain obstruction,Please step away from the light curtain"
                        && s_text != "左光幕遮挡,请离开光幕" && s_text != "Left light curtain obstruction,Please step away from the light curtain"
                        && s_text != "前光幕遮挡,请离开光幕" && s_text != "Front light curtain obstruction,Please step away from the light curtain"
                        && s_text != "急停已按下,请打开急停开关" && s_text != "Emergency stop pressed,Please turn on the emergency stop switch"
                        && s_text != "左门已打开,请关闭左门" && s_text != "The left door is open,Please close the left door"
                        && s_text != "右门已打开,请关闭右门" && s_text != "The right door is open,Please close the right door"
                        && s_text != "后光幕遮挡,请离开光幕" && s_text != "Back light curtain obstruction,Please step away from the light curtain"
                        && FADM_Object.Communal._b_isNetWork)
                            {
                                //插入微信播报表 推送微信 isBroadcastW为真 保存表   isBroadcastW为假 不保存表   
                                //插入微信播报表
                                inBroadcastW(FADM_Object.Communal._s_machineCode, s_text, s_insert, true);
                            }
                        }

                        //Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer buzzer = new Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer_Basic();
                        //buzzer.Buzzer_On();

                        //FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "1 ;");
                        while (true)
                        {
                            Thread.Sleep(1);
                            if (Lib_Card.CardObject.keyValuePairs[s_insert].Choose != 0)
                                break;

                            if (s_text != "右光幕遮挡,请离开光幕" && s_text != "Right light curtain obstruction,Please step away from the light curtain"
                        && s_text != "左光幕遮挡,请离开光幕" && s_text != "Left light curtain obstruction,Please step away from the light curtain"
                        && s_text != "前光幕遮挡,请离开光幕" && s_text != "Front light curtain obstruction,Please step away from the light curtain"
                        && s_text != "急停已按下,请打开急停开关" && s_text != "Emergency stop pressed,Please turn on the emergency stop switch"
                        && s_text != "左门已打开,请关闭左门" && s_text != "The left door is open,Please close the left door"
                        && s_text != "右门已打开,请关闭右门" && s_text != "The right door is open,Please close the right door"
                        && s_text != "后光幕遮挡,请离开光幕" && s_text != "Back light curtain obstruction,Please step away from the light curtain"
                        && FADM_Object.Communal._b_isNetWork)
                            {
                                //发起请求查询 
                                getBroadcastRe(FADM_Object.Communal._s_machineCode, s_insert, s_insert);
                                Thread.Sleep(1000);
                            }

                        }

                        _i_alarm_Choose = Lib_Card.CardObject.keyValuePairs[s_insert].Choose;
                        if (Lib_Card.CardObject.keyValuePairs[s_insert].Choose == 1)
                        {
                            if (i_oldSign == 7 || i_oldSign == 9 || i_oldSign == 13 || i_oldSign == 14)
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE abs_cup_details SET  Cooperate = 33 WHERE  Cooperate = " + i_oldSign + " AND CupNum = " + i_cupNo + " ;");
                            else if (i_oldSign == 8 || i_oldSign == 10 || i_oldSign == 11 || i_oldSign == 12)
                            {
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE abs_cup_details SET  Cooperate = 32 WHERE  Cooperate = " + i_oldSign + " AND CupNum = " + i_cupNo + " ;");
                            }
                            else if (i_oldSign == 77)
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                                      "UPDATE abs_cup_details SET  Cooperate = 88 WHERE  Cooperate = " + i_oldSign + " AND CupNum = " + i_cupNo + " ;");
                        }
                        else
                        {
                            ////忽略过期等，直接操作
                            //FADM_Object.Communal._fadmSqlserver.ReviseData(
                            //   "UPDATE dye_details SET  Cooperate = 1,Choose = 1 WHERE  Cooperate = " + i_oldSign + " AND BottleNum = " + i_bottleNo + " ;");

                            //FADM_Object.Communal._fadmSqlserver.ReviseData(
                            //       "UPDATE dye_details SET  Choose = 1 WHERE  Choose = 2 AND BottleNum = " + i_bottleNo + " ;");

                            if (i_oldSign == 7 || i_oldSign == 8 || i_oldSign == 9 || i_oldSign == 10 || i_oldSign == 77)
                            {
                                //选择否直接置为99,判断为退出
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE abs_cup_details SET  Cooperate = 99 WHERE   CupNum = " + i_cupNo + " ;");
                            }
                            else
                            {
                                //过期或液量低，选择否继续滴液
                                if (i_oldSign == 13 || i_oldSign == 14)
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE abs_cup_details SET  Cooperate = 23 WHERE  Cooperate = " + i_oldSign + " AND CupNum = " + i_cupNo + " ;");
                                else
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE abs_cup_details SET  Cooperate = 21 WHERE  Cooperate = " + i_oldSign + " AND CupNum = " + i_cupNo + " ;");
                            }
                        }

                        //if (messageBoxButtons == MessageBoxButtons.YesNo)
                        {
                            //写进数据库
                            string s_sql = "INSERT INTO alarm_table" +
                                         "(MyDate,MyTime,AlarmHead,AlarmDetails)" +
                                         " VALUES( '" +
                                         String.Format("{0:d}", DateTime.Now) + "','" +
                                         String.Format("{0:T}", DateTime.Now) + "','AbsAddMedicine','" +
                                         s_text + "(" + (_i_alarm_Choose == 1 ? "Yes" : "No") + ")');";

                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                        }
                        Lib_Card.CardObject.DeleteD(s_insert);


                        //buzzer.Buzzer_Off();




                    });
                    thread.Start();
                }
            }
            catch
            {
                goto label1;
            }


        }

        public MyAlarm(string s_text, int i_cupNo, int i_oldSign, bool b_repeat)
        {
        label1:
            try
            {
                if (!string.IsNullOrEmpty(s_text))
                {
                    Thread thread = new Thread(() =>
                    {
                        string s_insert = Lib_Card.CardObject.InsertCF(s_text, "AbsAddMedicine",1);
                        if (s_insert == "重复")
                        {
                            //FADM_Object.Communal._fadmSqlserver.ReviseData(
                            //   "UPDATE dye_details SET  Cooperate = 1 WHERE  Cooperate = " + i_oldSign + " AND BottleNum = " + i_bottleNo + " ;");
                            return;
                        }
                        else if (s_insert == "重复1")
                        {
                            if (i_oldSign == 7 || i_oldSign == 9 || i_oldSign == 13 || i_oldSign == 14)
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE abs_cup_details SET  Cooperate = 33 WHERE  Cooperate = " + i_oldSign + " AND CupNum = " + i_cupNo + " ;");
                            else if (i_oldSign == 8 || i_oldSign == 10 || i_oldSign == 11 || i_oldSign == 12)
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE abs_cup_details SET  Cooperate = 32 WHERE  Cooperate = " + i_oldSign + " AND CupNum = " + i_cupNo + " ;");
                            return;
                        }
                        else
                        {
                            if (s_text != "右光幕遮挡,请离开光幕" && s_text != "Right light curtain obstruction,Please step away from the light curtain"
                        && s_text != "左光幕遮挡,请离开光幕" && s_text != "Left light curtain obstruction,Please step away from the light curtain"
                        && s_text != "前光幕遮挡,请离开光幕" && s_text != "Front light curtain obstruction,Please step away from the light curtain"
                        && s_text != "急停已按下,请打开急停开关" && s_text != "Emergency stop pressed,Please turn on the emergency stop switch"
                        && s_text != "左门已打开,请关闭左门" && s_text != "The left door is open,Please close the left door"
                        && s_text != "右门已打开,请关闭右门" && s_text != "The right door is open,Please close the right door"
                        && s_text != "后光幕遮挡,请离开光幕" && s_text != "Back light curtain obstruction,Please step away from the light curtain"
                        && FADM_Object.Communal._b_isNetWork)
                            {
                                //插入微信播报表 推送微信 isBroadcastW为真 保存表   isBroadcastW为假 不保存表   
                                //插入微信播报表
                                inBroadcastW(FADM_Object.Communal._s_machineCode, s_text, s_insert, true);
                            }
                        }

                        //Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer buzzer = new Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer_Basic();
                        //buzzer.Buzzer_On();

                        //FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "1 ;");
                        while (true)
                        {
                            Thread.Sleep(1);
                            if (Lib_Card.CardObject.keyValuePairs[s_insert].Choose != 0)
                                break;

                            if (s_text != "右光幕遮挡,请离开光幕" && s_text != "Right light curtain obstruction,Please step away from the light curtain"
                        && s_text != "左光幕遮挡,请离开光幕" && s_text != "Left light curtain obstruction,Please step away from the light curtain"
                        && s_text != "前光幕遮挡,请离开光幕" && s_text != "Front light curtain obstruction,Please step away from the light curtain"
                        && s_text != "急停已按下,请打开急停开关" && s_text != "Emergency stop pressed,Please turn on the emergency stop switch"
                        && s_text != "左门已打开,请关闭左门" && s_text != "The left door is open,Please close the left door"
                        && s_text != "右门已打开,请关闭右门" && s_text != "The right door is open,Please close the right door"
                        && s_text != "后光幕遮挡,请离开光幕" && s_text != "Back light curtain obstruction,Please step away from the light curtain"
                        && FADM_Object.Communal._b_isNetWork)
                            {
                                //发起请求查询 
                                getBroadcastRe(FADM_Object.Communal._s_machineCode, s_insert, s_insert);
                                Thread.Sleep(1000);
                            }

                        }

                        _i_alarm_Choose = Lib_Card.CardObject.keyValuePairs[s_insert].Choose;
                        if (Lib_Card.CardObject.keyValuePairs[s_insert].Choose == 1)
                        {
                            if (i_oldSign == 7 || i_oldSign == 9 || i_oldSign == 13 || i_oldSign == 14)
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE abs_cup_details SET  Cooperate = 33 WHERE  Cooperate = " + i_oldSign + " AND CupNum = " + i_cupNo + " ;");
                            else if (i_oldSign == 8 || i_oldSign == 10 || i_oldSign == 11 || i_oldSign == 12)
                            {
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE abs_cup_details SET  Cooperate = 32 WHERE  Cooperate = " + i_oldSign + " AND CupNum = " + i_cupNo + " ;");
                            }
                            else if (i_oldSign == 77)
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                                      "UPDATE abs_cup_details SET  Cooperate = 88 WHERE  Cooperate = " + i_oldSign + " AND CupNum = " + i_cupNo + " ;");
                        }
                        else
                        {
                            ////忽略过期等，直接操作
                            //FADM_Object.Communal._fadmSqlserver.ReviseData(
                            //   "UPDATE dye_details SET  Cooperate = 1,Choose = 1 WHERE  Cooperate = " + i_oldSign + " AND BottleNum = " + i_bottleNo + " ;");

                            //FADM_Object.Communal._fadmSqlserver.ReviseData(
                            //       "UPDATE dye_details SET  Choose = 1 WHERE  Choose = 2 AND BottleNum = " + i_bottleNo + " ;");

                            if (i_oldSign == 7 || i_oldSign == 8 || i_oldSign == 9 || i_oldSign == 10 || i_oldSign == 77)
                            {
                                //选择否直接置为99,判断为退出
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE abs_cup_details SET  Cooperate = 99 WHERE   CupNum = " + i_cupNo + " ;");
                            }
                            else
                            {
                                //过期或液量低，选择否继续滴液
                                if (i_oldSign == 13 || i_oldSign == 14)
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE abs_cup_details SET  Cooperate = 23 WHERE  Cooperate = " + i_oldSign + " AND CupNum = " + i_cupNo + " ;");
                                else
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE abs_cup_details SET  Cooperate = 21 WHERE  Cooperate = " + i_oldSign + " AND CupNum = " + i_cupNo + " ;");
                            }
                        }

                        //if (messageBoxButtons == MessageBoxButtons.YesNo)
                        {
                            //写进数据库
                            string s_sql = "INSERT INTO alarm_table" +
                                         "(MyDate,MyTime,AlarmHead,AlarmDetails)" +
                                         " VALUES( '" +
                                         String.Format("{0:d}", DateTime.Now) + "','" +
                                         String.Format("{0:T}", DateTime.Now) + "','AbsAddMedicine','" +
                                         s_text + "(" + (_i_alarm_Choose == 1 ? "Yes" : "No") + ")');";

                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                        }
                        Lib_Card.CardObject.DeleteD(s_insert);


                        //buzzer.Buzzer_Off();




                    });
                    thread.Start();
                }
            }
            catch
            {
                goto label1;
            }


        }

    }
}
