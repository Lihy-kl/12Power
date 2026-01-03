using Lib_DataBank.MySQL;
using Lib_File;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartDyeing.FADM_Object
{
    internal class IndependentDye
    {
        
        public IndependentDye(List<FADM_Object.Data> lis_datas, int i,int i_type)
        {
            if (lis_datas.Count != 12)
            {
                if (++FADM_Object.Communal._ai_iArray[i] > 5)
                {
                    
                    // FADM_Object.Communal._fadmSqlserver.InsertSpeechInfo((i_erea + 1) + "号打板机通讯异常");
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Object.Communal._sa_dyeConFTime[i] = Lib_Card.CardObject.InsertD((i + 1) + "号打板机通讯异常", "Dye");
                    else
                        FADM_Object.Communal._sa_dyeConFTime[i] = Lib_Card.CardObject.InsertD("Communication abnormality of " + (i + 1) + " board making machine", "Dye");
                }
                return;
            }
            else
            {
                FADM_Object.Communal._ai_iArray[i] = 0;
                if (FADM_Object.Communal._sa_dyeConFTime[i] != "")
                    Lib_Card.CardObject.DeleteD(FADM_Object.Communal._sa_dyeConFTime[i]);
                FADM_Object.Communal._sa_dyeConFTime[i] = "";
            }

            for (int j = 0; j < (i_type==7?8:lis_datas.Count); j++)
            {
                int i_cupmin = 0;
                if (i == 0)
                {
                    i_cupmin = Lib_Card.Configure.Parameter.Machine_Area1_CupMin;
                }
                else if (i == 1)
                {
                    i_cupmin = Lib_Card.Configure.Parameter.Machine_Area2_CupMin;
                }
                else if (i == 2)
                {
                    i_cupmin = Lib_Card.Configure.Parameter.Machine_Area3_CupMin;
                }
                else if (i == 3)
                {
                    i_cupmin = Lib_Card.Configure.Parameter.Machine_Area4_CupMin;
                }
                else if (i == 4)
                {
                    i_cupmin = Lib_Card.Configure.Parameter.Machine_Area5_CupMin;
                }
                else if (i == 5)
                {
                    i_cupmin = Lib_Card.Configure.Parameter.Machine_Area6_CupMin;
                }

                int i_cupNo = i_cupmin  + j;

                //主杯数据
                FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._s_temp = string.Format("{0:F1}", Convert.ToDouble(lis_datas[j]._s_realTem) / 10.0);

                string[] sa_statues = { "", "待机", "未完成", "完成", "异常"};
                string[] sa_technology = { "", "入杯", "加药", "温控", "冷行", "出杯","保温运行" };

                //更新报警
                int i_warm_temp = Convert.ToInt32(lis_datas[j]._s_Warm);
                //主杯
                i_warm_temp = Convert.ToInt32(lis_datas[j]._s_Warm);



                if (i_warm_temp != FADM_Object.Communal._ia_alarmNum1[i_cupNo - 1])
                {
                    //原来没有报警
                    if (FADM_Object.Communal._ia_alarmNum1[i_cupNo - 1] == 0)
                    {
                        FADM_Object.Communal._ia_alarmNum1[i_cupNo - 1] = i_warm_temp;
                        string s_alarm = "";
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        {
                            if(i_warm_temp == 1)
                            {
                                s_alarm += "转盘电机异常报警";
                            }
                            else if (i_warm_temp == 2)
                            {
                                s_alarm += "加药头气缸下降超时";
                            }
                            else if (i_warm_temp == 3)
                            {
                                s_alarm += "加药头气缸上升超时";
                            }
                            else if (i_warm_temp == 4)
                            {
                                s_alarm += "超温";
                            }
                            else if (i_warm_temp == 5)
                            {
                                s_alarm += "上一次工艺未出杯";
                            }
                            s_alarm = i_cupNo + "号杯" + s_alarm;
                        }
                        else
                        {
                            if (i_warm_temp == 1)
                            {
                                s_alarm += "Rotary motor abnormal alarm";
                            }
                            else if (i_warm_temp == 2)
                            {
                                s_alarm += "Dosing head cylinder descent timeout";
                            }
                            else if (i_warm_temp == 3)
                            {
                                s_alarm += "Dosing head cylinder rise timeout";
                            }
                            else if (i_warm_temp == 4)
                            {
                                s_alarm += "overtemperature";
                            }
                            else if (i_warm_temp == 5)
                            {
                                s_alarm += "The last process did not come out of the cup";
                            }
                            s_alarm = i_cupNo + " Cup " + s_alarm;
                        }
                        s_alarm = s_alarm.Remove(s_alarm.Length - 1, 1);


                        FADM_Object.Communal._sa_dyeAlarm1[i_cupNo - 1] = Lib_Card.CardObject.InsertD(s_alarm, "Dye");
                    }
                    //原来有报警
                    else
                    {
                        FADM_Object.Communal._ia_alarmNum1[i_cupNo - 1] = i_warm_temp;
                        Lib_Card.CardObject.DeleteD(FADM_Object.Communal._sa_dyeAlarm1[i_cupNo - 1]);
                        if (i_warm_temp != 0)
                        {
                            string s_alarm = "";
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            {
                                if (i_warm_temp == 1)
                                {
                                    s_alarm += "转盘电机异常报警";
                                }
                                else if (i_warm_temp == 2)
                                {
                                    s_alarm += "加药头气缸下降超时";
                                }
                                else if (i_warm_temp == 3)
                                {
                                    s_alarm += "加药头气缸上升超时";
                                }
                                else if (i_warm_temp == 4)
                                {
                                    s_alarm += "超温";
                                }
                                else if (i_warm_temp == 5)
                                {
                                    s_alarm += "上一次工艺未出杯";
                                }
                                s_alarm = i_cupNo + "号杯" + s_alarm;
                            }
                            else
                            {
                                if (i_warm_temp == 1)
                                {
                                    s_alarm += "Rotary motor abnormal alarm";
                                }
                                else if (i_warm_temp == 2)
                                {
                                    s_alarm += "Dosing head cylinder descent timeout";
                                }
                                else if (i_warm_temp == 3)
                                {
                                    s_alarm += "Dosing head cylinder rise timeout";
                                }
                                else if (i_warm_temp == 4)
                                {
                                    s_alarm += "overtemperature";
                                }
                                else if (i_warm_temp == 5)
                                {
                                    s_alarm += "The last process did not come out of the cup";
                                }
                                s_alarm = i_cupNo + " Cup " + s_alarm;
                            }
                            s_alarm = s_alarm.Remove(s_alarm.Length - 1, 1);

                            FADM_Object.Communal._sa_dyeAlarm1[i_cupNo - 1] = Lib_Card.CardObject.InsertD(s_alarm, "Dye");
                        }
                    }
                }




                if ("0" != lis_datas[j]._s_currentState)
                {
                    FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._b_start = true;
                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "UPDATE cup_details SET RealTemp = '" + FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._s_temp + "' "  +
                                        "WHERE CupNum = " + i_cupNo + " AND Statues != '下线' ;");

                    if ("5" != lis_datas[j]._s_currentState)
                    {
                        FADM_Auto.Dye._ia_keepwarm[i_cupNo - 1] = false;

                       // //运行非温控
                       // FADM_Object.Communal._fadmSqlserver.ReviseData(
                       //"UPDATE cup_details SET StepStartTime = '" + DateTime.Now + "' " +
                       //"WHERE CupNum = " + i_cupNo + " AND Statues != '下线'  AND TechnologyName != '" +
                       //sa_technology[Convert.ToInt16(lis_datas[j]._s_currentCraft)] + "';");


                    }
                    else
                    {
                        if (!FADM_Auto.Dye._ia_keepwarm[i_cupNo - 1])
                        {
                            FADM_Auto.Dye._ia_keepwarm[i_cupNo - 1] = true;
                            //保温运行
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE cup_details SET " +
                            "StepStartTime = '" + DateTime.Now + "',TechnologyName = '保温运行'  " +
                            "WHERE CupNum = " + i_cupNo + " AND TechnologyName != '保温运行' AND Statues != '下线';");
                        }


                    }

                }


                //当前杯染固色完成
                if ("1" == lis_datas[j]._s_isTotalFinish || "2" == lis_datas[j]._s_isTotalFinish
                    || "3" == lis_datas[j]._s_isTotalFinish || "4" == lis_datas[j]._s_isTotalFinish
                     || "5" == lis_datas[j]._s_isTotalFinish || "6" == lis_datas[j]._s_isTotalFinish)
                {
                    FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._b_start = false;

                    //清空全部完成标记位
                    int[] ia_zero = new int[1];
                    ia_zero[0] = 0;
                    DyeHMIWriteSigle(i, j, 4110, 100, ia_zero);

                    //if ("1" == lis_datas[j]._s_isTotalFinish)
                    {
                        Thread thread = new Thread(Finish);
                        thread.Start(i_cupNo);
                    }


                    continue;
                }




                
                //入杯，加入播报
                if ("1" == lis_datas[j]._s_currentCraft)
                {
                    int i_ear = 0;
                    int i_ind = 0;
                    if (i_cupNo >= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area1_CupMin.ToString()) && i_cupNo <= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area1_CupMax.ToString()))
                    {
                        i_ear = 1;
                        i_ind = i_cupNo - Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area1_CupMin.ToString()) + 1;
                    }
                    else if (i_cupNo >= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area2_CupMin.ToString()) && i_cupNo <= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area2_CupMax.ToString()))
                    {
                        i_ear = 2;
                        i_ind = i_cupNo - Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area2_CupMin.ToString()) + 1;
                    }
                    else if (i_cupNo >= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area3_CupMin.ToString()) && i_cupNo <= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area3_CupMax.ToString()))
                    {
                        i_ear = 3;
                        i_ind = i_cupNo - Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area3_CupMin.ToString()) + 1;
                    }
                    else if (i_cupNo >= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area4_CupMin.ToString()) && i_cupNo <= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area4_CupMax.ToString()))
                    {
                        i_ear = 4;
                        i_ind = i_cupNo - Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area4_CupMin.ToString()) + 1;
                    }
                    else if (i_cupNo >= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area5_CupMin.ToString()) && i_cupNo <= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area5_CupMax.ToString()))
                    {
                        i_ear = 5;
                        i_ind = i_cupNo - Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area5_CupMin.ToString()) + 1;
                    }
                    else if (i_cupNo >= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area6_CupMin.ToString()) && i_cupNo <= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area6_CupMax.ToString()))
                    {
                        i_ear = 6;
                        i_ind = i_cupNo - Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area5_CupMin.ToString()) + 1;
                    }

                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._s_inTime = Lib_Card.CardObject.InsertD(i_ear + "号区域"+ i_ind+"号杯入杯", "Dye");
                    else
                        FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._s_inTime = Lib_Card.CardObject.InsertD("Area "+ i_ear + " cup "+ i_ind+" In", "Dye");

                }
                //出杯，加入播报
                else if ("5" == lis_datas[j]._s_currentCraft)
                {
                    int i_ear = 0;
                    int i_ind = 0;
                    if (i_cupNo >= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area1_CupMin.ToString()) && i_cupNo <= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area1_CupMax.ToString()))
                    {
                        i_ear = 1;
                        i_ind = i_cupNo - Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area1_CupMin.ToString()) + 1;
                    }
                    else if (i_cupNo >= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area2_CupMin.ToString()) && i_cupNo <= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area2_CupMax.ToString()))
                    {
                        i_ear = 2;
                        i_ind = i_cupNo - Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area2_CupMin.ToString()) + 1;
                    }
                    else if (i_cupNo >= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area3_CupMin.ToString()) && i_cupNo <= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area3_CupMax.ToString()))
                    {
                        i_ear = 3;
                        i_ind = i_cupNo - Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area3_CupMin.ToString()) + 1;
                    }
                    else if (i_cupNo >= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area4_CupMin.ToString()) && i_cupNo <= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area4_CupMax.ToString()))
                    {
                        i_ear = 4;
                        i_ind = i_cupNo - Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area4_CupMin.ToString()) + 1;
                    }
                    else if (i_cupNo >= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area5_CupMin.ToString()) && i_cupNo <= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area5_CupMax.ToString()))
                    {
                        i_ear = 5;
                        i_ind = i_cupNo - Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area5_CupMin.ToString()) + 1;
                    }
                    else if (i_cupNo >= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area6_CupMin.ToString()) && i_cupNo <= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area6_CupMax.ToString()))
                    {
                        i_ear = 6;
                        i_ind = i_cupNo - Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area5_CupMin.ToString()) + 1;
                    }
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._s_outTime = Lib_Card.CardObject.InsertD(i_ear + "号区域" + i_ind + "号杯出杯", "Dye");
                    else
                        FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._s_outTime = Lib_Card.CardObject.InsertD("Area " + i_ear + " cup " + i_ind + " Out", "Dye");


                }
                else
                {
                    if (FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._s_inTime != null)
                        Lib_Card.CardObject.DeleteD(FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._s_inTime);
                    FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._s_inTime = null;
                    if (FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._s_outTime != null)
                        Lib_Card.CardObject.DeleteD(FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._s_outTime);
                    FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._s_outTime = null;

                }


                //停止染色的杯号
                if (FADM_Object.Communal._lis_dripStopCup.Contains(i_cupNo))
                {
                    //停止信号
                    int[] ia_zero = new int[1];
                    ia_zero[0] = 2;
                    DyeHMIWriteSigle(i, j, 1101, 100, ia_zero);

                    FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", i_cupNo + "号配液杯染固色停止启动");


                    //滴液完成数组移除当前杯号
                    FADM_Object.Communal._lis_dripStopCup.Remove(i_cupNo);
                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                              "UPDATE cup_details SET TotalWeight = 0, StepStartTime = '" + DateTime.Now + "' WHERE CupNum = " + i_cupNo + ";");

                    FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._b_start = false;
                    Thread thread = new Thread(Finish);
                    thread.Start(i_cupNo);

                    continue;
                }

                //下发总工艺
                if (FADM_Object.Communal._lis_dripSuccessCup.Contains(i_cupNo))
                {
                    //把对应杯号温度曲线描点数据清空
                    Txt.DeleteTXT(i_cupNo);
                    Txt.DeleteMarkTXT(i_cupNo);
                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                   "UPDATE cup_details SET RecordIndex = 0 WHERE CupNum = " +
                   i_cupNo + "  ;");

                    FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._b_start = true;

                    //查询当前工艺总步号
                    string s_sql = "SELECT * FROM dye_details WHERE CupNum = " + i_cupNo + " ORDER BY StepNum DESC;";
                    DataTable dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                    if (dt_drop_head.Rows.Count == 0)
                    {
                        FADM_Object.Communal._lis_dripSuccessCup.Remove(i_cupNo);
                        continue;
                    }
                    else
                    {

                        //发送总步号，发送启动
                        int[] ia_zero = new int[1];
                        ia_zero[0] = Convert.ToInt32(dt_drop_head.Rows[0]["StepNum"].ToString());
                        DyeHMIWriteSigle(i, j, 1111, 100, ia_zero);

                        ia_zero[0] = 1;
                        DyeHMIWriteSigle(i, j, 1101, 100, ia_zero);

                        //修改总步号
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                        "UPDATE cup_details SET TotalStep = " + dt_drop_head.Rows[0]["StepNum"]+ ",StartTime = '" + DateTime.Now + "',FormulaCode = '" + dt_drop_head.Rows[0]["FormulaCode"] + "' WHERE CupNum = " + i_cupNo + ";");
                        FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", i_cupNo + "号配液杯染固色启动");

                        FADM_Object.Communal._lis_dripSuccessCup.Remove(i_cupNo);
                    }
                }



                //等待数据,下发下一个步骤
                if ("1" == lis_datas[j]._s_waitData)
                {



                    string s_sql;

                    //滴液成功
                    //if ("1" == lis_datas[j]._s_dripFail)
                    {
                        if (FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._b_tagging == false)
                        {


                            //复位等待数据
                            int[] ai_zero = new int[1];
                            ai_zero[0] = 0;
                            DyeHMIWriteSigle(i, j, 4108, 100, ai_zero);

                            //查找第一个没完成的工艺
                            s_sql = "SELECT * FROM dye_details WHERE CupNum = " + i_cupNo + " AND " +
                                   "Finish = 0 Order by StepNum;";
                            DataTable dt_dye_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                            if (0 == dt_dye_details.Rows.Count)
                                continue;
                            if ("0" != lis_datas[j]._s_currentStepNum)
                            {
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE dye_details SET FinishTime = '" + DateTime.Now + "', Finish = 1 WHERE CupNum = " + i_cupNo +
                                    " AND StepNum = " + (Convert.ToInt16(dt_dye_details.Rows[0]["StepNum"].ToString())) + ";");

                                if (dt_dye_details.Rows[0]["TechnologyName"].ToString() == "加A" || "Add A" == dt_dye_details.Rows[0]["TechnologyName"].ToString())
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE dye_details SET RealDropWeight = '" + Convert.ToDouble(lis_datas[j]._s_A).ToString("f2") + "' WHERE CupNum = " + i_cupNo +
                                    " AND StepNum = " + (Convert.ToInt16(dt_dye_details.Rows[0]["StepNum"].ToString())) + ";");
                                }
                                else if (dt_dye_details.Rows[0]["TechnologyName"].ToString() == "加B" || "Add B" == dt_dye_details.Rows[0]["TechnologyName"].ToString()
                                    || dt_dye_details.Rows[0]["TechnologyName"].ToString() == "加C" || "Add C" == dt_dye_details.Rows[0]["TechnologyName"].ToString()
                                    || dt_dye_details.Rows[0]["TechnologyName"].ToString() == "加D" || "Add D" == dt_dye_details.Rows[0]["TechnologyName"].ToString()
                                    || dt_dye_details.Rows[0]["TechnologyName"].ToString() == "加E" || "Add E" == dt_dye_details.Rows[0]["TechnologyName"].ToString()
                                    || dt_dye_details.Rows[0]["TechnologyName"].ToString() == "加F" || "Add F" == dt_dye_details.Rows[0]["TechnologyName"].ToString()
                                    || dt_dye_details.Rows[0]["TechnologyName"].ToString() == "加G" || "Add G" == dt_dye_details.Rows[0]["TechnologyName"].ToString()
                                    || dt_dye_details.Rows[0]["TechnologyName"].ToString() == "加H" || "Add H" == dt_dye_details.Rows[0]["TechnologyName"].ToString()
                                    || dt_dye_details.Rows[0]["TechnologyName"].ToString() == "加I" || "Add I" == dt_dye_details.Rows[0]["TechnologyName"].ToString()
                                    || dt_dye_details.Rows[0]["TechnologyName"].ToString() == "加J" || "Add J" == dt_dye_details.Rows[0]["TechnologyName"].ToString()
                                    || dt_dye_details.Rows[0]["TechnologyName"].ToString() == "加K" || "Add K" == dt_dye_details.Rows[0]["TechnologyName"].ToString()
                                    || dt_dye_details.Rows[0]["TechnologyName"].ToString() == "加L" || "Add L" == dt_dye_details.Rows[0]["TechnologyName"].ToString()
                                    || dt_dye_details.Rows[0]["TechnologyName"].ToString() == "加M" || "Add M" == dt_dye_details.Rows[0]["TechnologyName"].ToString()
                                    || dt_dye_details.Rows[0]["TechnologyName"].ToString() == "加N" || "Add N" == dt_dye_details.Rows[0]["TechnologyName"].ToString()
                                    )
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE dye_details SET RealDropWeight = '" + Convert.ToDouble(lis_datas[j]._s_B).ToString("f2") + "' WHERE CupNum = " + i_cupNo +
                                    " AND StepNum = " + (Convert.ToInt16(dt_dye_details.Rows[0]["StepNum"].ToString())) + ";");
                                }

                                //查找第一个没完成的工艺
                                s_sql = "SELECT * FROM dye_details WHERE CupNum = " + i_cupNo + " AND " +
                                           "Finish = 0 Order by StepNum;";
                                dt_dye_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                                if (0 == dt_dye_details.Rows.Count)
                                    continue;
                            }

                            string s_technologyName = dt_dye_details.Rows[0]["TechnologyName"].ToString();

                            FADM_Object.Communal._fadmSqlserver.InsertRun(
                            "Dail", i_cupNo + "号配液杯执行(" + Convert.ToInt32(dt_dye_details.Rows[0]["StepNum"]) + ":" + s_technologyName + ")");
                            //if (Convert.ToInt16(dt_dye_details.Rows[0]["StepNum"].ToString()) != 1)
                            //    FADM_Object.Communal._fadmSqlserver.ReviseData(
                            //        "UPDATE dye_details SET FinishTime = '" + DateTime.Now + "', Finish = 1 WHERE CupNum = " + i_cupNo +
                            //        " AND StepNum = " + (Convert.ToInt16(dt_dye_details.Rows[0]["StepNum"].ToString())-1) + ";");

                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE dye_details SET StartTime = '" + DateTime.Now + "' WHERE CupNum = " + i_cupNo +
                                " AND StepNum = " + (Convert.ToInt16(dt_dye_details.Rows[0]["StepNum"].ToString())) + ";");



                            if (Convert.ToInt16(dt_dye_details.Rows[0]["Temp"]) == 0)
                            {
                                if (Convert.ToInt16(dt_dye_details.Rows[0]["Time"]) == 0)
                                {
                                    s_sql = "UPDATE cup_details SET Statues = '" + dt_dye_details.Rows[0]["Code"] +
                                        "',TechnologyName='"+dt_dye_details.Rows[0]["TechnologyName"] +
                                        "',StepNum='" + dt_dye_details.Rows[0]["StepNum"] + "',StepStartTime = '" + DateTime.Now + "' ,SetTemp = null, SetTime = null WHERE CupNum = " + i_cupNo + ";";
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                }
                                else
                                {
                                    s_sql = "UPDATE cup_details SET Statues = '" + dt_dye_details.Rows[0]["Code"] +
                                        "',TechnologyName='"+dt_dye_details.Rows[0]["TechnologyName"] +
                                        "',StepNum='" + dt_dye_details.Rows[0]["StepNum"] + "',StepStartTime = '" + DateTime.Now + "' ,SetTemp = null, SetTime = '" + Convert.ToInt32(dt_dye_details.Rows[0]["Time"]) + "' WHERE CupNum = " + i_cupNo + ";";
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                }
                            }
                            else
                            {
                                if (Convert.ToInt16(dt_dye_details.Rows[0]["Time"]) == 0)
                                {
                                    s_sql = "UPDATE cup_details SET Statues = '" + dt_dye_details.Rows[0]["Code"] + "',TechnologyName='"+dt_dye_details.Rows[0]["TechnologyName"] +
                                        "',StepNum='" + dt_dye_details.Rows[0]["StepNum"] + "',SetTemp = '" +
                                        Convert.ToInt32(dt_dye_details.Rows[0]["Temp"]) + "',StepStartTime = '" + DateTime.Now + "' , SetTime = null WHERE CupNum = " + i_cupNo + ";";
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                }
                                else
                                {
                                    s_sql = "UPDATE cup_details SET Statues = '" + dt_dye_details.Rows[0]["Code"] +
                                        "',TechnologyName='"+dt_dye_details.Rows[0]["TechnologyName"] +
                                        "',StepNum='" + dt_dye_details.Rows[0]["StepNum"] + "',SetTemp = '" + Convert.ToInt32(dt_dye_details.Rows[0]["Temp"]) + "',StepStartTime = '" + DateTime.Now + "' , SetTime = '" +
                                        Convert.ToInt32(dt_dye_details.Rows[0]["Time"]) + "' WHERE CupNum = " + i_cupNo + ";";
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                }
                            }
                            if(dt_dye_details.Rows.Count > 0)
                            FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._s_technologyName = dt_dye_details.Rows[0]["TechnologyName"].ToString();
                            FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._b_tagging = true;


                            




                            //下发下一步工艺
                            ai_zero = new int[11];
                            //当前名称
                            if ("冷行" == s_technologyName || "Cool line" == s_technologyName)
                                ai_zero[0] = 0x04;
                            else if ("温控" == s_technologyName || "Temperature control" == s_technologyName)
                                ai_zero[0] = 0x03;

                            else if ("放布" == s_technologyName || "Entering the fabric" == s_technologyName)
                            {
                                ai_zero[0] = 0x01;
                            }

                            else if ("出布" == s_technologyName || "Outgoing fabric" == s_technologyName)
                                ai_zero[0] = 0x05;
                            else if ("加A" == s_technologyName || "Add A" == s_technologyName
                                || "加B" == s_technologyName || "Add B" == s_technologyName
                                ||"加C" == s_technologyName || "Add C" == s_technologyName 
                                || "加D" == s_technologyName || "Add D" == s_technologyName
                                || "加E" == s_technologyName || "Add E" == s_technologyName
                                || "加F" == s_technologyName || "Add F" == s_technologyName
                                || "加G" == s_technologyName || "Add G" == s_technologyName
                                || "加H" == s_technologyName || "Add H" == s_technologyName
                                || "加I" == s_technologyName || "Add I" == s_technologyName
                                || "加J" == s_technologyName || "Add J" == s_technologyName
                                || "加K" == s_technologyName || "Add K" == s_technologyName
                                || "加L" == s_technologyName || "Add L" == s_technologyName
                                || "加M" == s_technologyName || "Add M" == s_technologyName
                                || "加N" == s_technologyName || "Add N" == s_technologyName)
                                ai_zero[0] = 0x02;
                            //其他
                            else
                                ai_zero[0] = 0x06;

                            //if(i_type == 6)
                            //{
                            //    //12杯精密机先跳过加药
                            //    if(ai_zero[0] == 0x02)
                            //    {
                            //        ai_zero[0] = 0x06;
                            //    }
                            //}
                            //启动
                            ai_zero[1] = 1;

                            //如果瓶号是200就是加A
                            if(ai_zero[0] == 0x02)
                            {
                                if ("加A" == s_technologyName || "Add A" == s_technologyName)
                                {
                                    int i_w = Convert.ToInt32(Convert.ToDouble(dt_dye_details.Rows[0]["ObjectDropWeight"]) * 100);
                                    int i_d0 = 0, i_d1 = 0, i_d2 = 0;
                                    i_d0 = i_w;
                                    i_d2 = i_d0 / 65536;
                                    if (i_d0 < 0) //负数脉冲
                                    {
                                        if (i_d2 == 0)
                                        {
                                            i_d2 = -1;
                                        }
                                        else
                                        {
                                            if (Math.Abs(i_d0) > 65536)
                                            {
                                                i_d2 = i_d2 + -1;
                                            }
                                        }
                                    }
                                    else
                                    {  //正数脉冲
                                        i_d2 = i_d0 / 65536;
                                    }
                                    i_d1 = i_d0 % 65536;
                                    ai_zero[2] = i_d1;
                                    ai_zero[3] = i_d2;
                                    ai_zero[4] = 0;
                                    ai_zero[5] = 0;
                                }
                                else
                                {
                                    ai_zero[2] = 0;
                                    ai_zero[3] = 0;
                                    int i_w = Convert.ToInt32(Convert.ToDouble(dt_dye_details.Rows[0]["ObjectDropWeight"]) * 100);
                                    int i_d0 = 0, i_d1 = 0, i_d2 = 0;
                                    i_d0 = i_w;
                                    i_d2 = i_d0 / 65536;
                                    if (i_d0 < 0) //负数脉冲
                                    {
                                        if (i_d2 == 0)
                                        {
                                            i_d2 = -1;
                                        }
                                        else
                                        {
                                            if (Math.Abs(i_d0) > 65536)
                                            {
                                                i_d2 = i_d2 + -1;
                                            }
                                        }
                                    }
                                    else
                                    {  //正数脉冲
                                        i_d2 = i_d0 / 65536;
                                    }
                                    i_d1 = i_d0 % 65536;

                                    ai_zero[4] = i_d1;
                                    ai_zero[5] = i_d2;
                                }
                            }
                            else
                            {
                                ai_zero[2] = 0;
                                ai_zero[3] = 0;
                                ai_zero[4] = 0;
                                ai_zero[5] = 0;
                            }

                            //目标温度
                            if (true)
                            {
                                int i_w = Convert.ToInt32(Convert.ToDouble(dt_dye_details.Rows[0]["Temp"]) * 10);
                                int i_d0 = 0, i_d1 = 0, i_d2 = 0;
                                i_d0 = i_w;
                                i_d2 = i_d0 / 65536;
                                if (i_d0 < 0) //负数脉冲
                                {
                                    if (i_d2 == 0)
                                    {
                                        i_d2 = -1;
                                    }
                                    else
                                    {
                                        if (Math.Abs(i_d0) > 65536)
                                        {
                                            i_d2 = i_d2 + -1;
                                        }
                                    }
                                }
                                else
                                {  //正数脉冲
                                    i_d2 = i_d0 / 65536;
                                }
                                i_d1 = i_d0 % 65536;
                                ai_zero[6] = i_d1;
                                ai_zero[7] = i_d2;
                            }


                            //保温时间/分
                            if (true)
                            {
                                int i_w = Convert.ToInt32(Convert.ToDouble(dt_dye_details.Rows[0]["Time"]));
                                int i_d0 = 0, i_d1 = 0, i_d2 = 0;
                                i_d0 = i_w;
                                i_d2 = i_d0 / 65536;
                                if (i_d0 < 0) //负数脉冲
                                {
                                    if (i_d2 == 0)
                                    {
                                        i_d2 = -1;
                                    }
                                    else
                                    {
                                        if (Math.Abs(i_d0) > 65536)
                                        {
                                            i_d2 = i_d2 + -1;
                                        }
                                    }
                                }
                                else
                                {  //正数脉冲
                                    i_d2 = i_d0 / 65536;
                                }
                                i_d1 = i_d0 % 65536;
                                ai_zero[8] = i_d1;
                                ai_zero[9] = i_d2;
                            }

                            //温度速率
                            ai_zero[10] = Convert.ToInt32(Convert.ToDouble(dt_dye_details.Rows[0]["TempSpeed"]) * 10);




                            DyeHMIWriteSigle(i, j, 1100, 100, ai_zero);
                        }
                    }



                    continue;

                }


            }
        }

        private void Finish(object obj_cupNo)
        {


            DataTable dt_drop_head = FADM_Object.Communal._fadmSqlserver.GetData("SELECT * FROM drop_head WHERE CupNum = " + obj_cupNo + ";");
            if (dt_drop_head.Rows.Count > 0)
            {
                if (dt_drop_head.Rows[0]["BatchName"] != System.DBNull.Value)
                {
                    FADM_Object.Communal._b_finshRun = false;
                    //染色正常结束
                    //复位当前杯使用状态
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", obj_cupNo + "号配液杯染固色完成");
                    else
                        FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "Dyeing and fixation of solution cup " + obj_cupNo + " completed");

                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                      " UPDATE dye_details SET FinishTime = '" + DateTime.Now + "', Finish = 1  WHERE CupNum = " +
                      obj_cupNo + "  and StepNum = (select MAX(StepNum) from dye_details where CupNum = " + obj_cupNo + ");");

                    //拷贝到历史表
                    DataTable dt_Temp = FADM_Object.Communal._fadmSqlserver.GetData(
                         "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'dye_details';");
                    string s_columnDetails = null;
                    foreach (DataRow row in dt_Temp.Rows)
                    {
                        if (Convert.ToString(row[0]) != "Cooperate" && Convert.ToString(row[0]) != "NeedPulse" && Convert.ToString(row[0]) != "Choose"
                            && Convert.ToString(row[0]) != "WaterFinish")
                            s_columnDetails += Convert.ToString(row[0]) + ", ";
                    }
                    s_columnDetails = s_columnDetails.Remove(s_columnDetails.Length - 2);

                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                        "INSERT INTO history_dye (" + s_columnDetails + ") (SELECT " + s_columnDetails + " FROM dye_details " +
                        "WHERE CupNum =" + obj_cupNo + " AND BatchName != '0') ;");
                    DataTable dt_drop_head2 = FADM_Object.Communal._fadmSqlserver.GetData(
                          "SELECT * FROM drop_head WHERE CupNum =" + obj_cupNo + ";");
                    if (dt_drop_head2.Rows.Count > 0)
                    {

                        string s_temp = Txt.ReadTXT(Convert.ToInt32(obj_cupNo));
                        if (!string.IsNullOrEmpty(s_temp))
                            FADM_Object.Communal._fadmSqlserver.SetImage(s_temp, Convert.ToInt32(obj_cupNo), Convert.ToString(dt_drop_head2.Rows[0]["BatchName"]));

                    }


                    string s_txt = Txt.ReadMarkTXT(Convert.ToInt32(obj_cupNo));

                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                        "UPDATE history_head SET MarkStep = '" + s_txt + "' WHERE CupNum = " + obj_cupNo + " AND BatchName = '" + dt_drop_head2.Rows[0]["BatchName"].ToString() + "';");
                }

                //清空表
                FADM_Object.Communal._fadmSqlserver.ReviseData(
                     "DELETE FROM drop_head WHERE CupNum =" + obj_cupNo + " AND BatchName != '0' ;");
                FADM_Object.Communal._fadmSqlserver.ReviseData(
                    "DELETE FROM drop_details WHERE CupNum =" + obj_cupNo + " AND BatchName != '0';");
                FADM_Object.Communal._fadmSqlserver.ReviseData(
                    "DELETE FROM dye_details WHERE CupNum =" + obj_cupNo + " AND BatchName != '0';");


                Txt.DeleteTXT(Convert.ToInt32(obj_cupNo));
                Txt.DeleteMarkTXT(Convert.ToInt32(obj_cupNo));



                FADM_Control.Formula.P_bl_update = true;
                //FADM_Object.Communal._fadmSqlserver.InsertSpeechInfo(i_cupNo + "号配液杯染固色完成");

                //FADM_Object.Communal._fadmSqlserver.DeleteSpeechInfo(i_cupNo + "号配液杯染固色完成");
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    string s_insert = Lib_Card.CardObject.InsertD(obj_cupNo + "号配液杯染固色完成", "Dye");
                    Lib_Card.CardObject.DeleteD(s_insert);
                }
                else
                {
                    string s_insert = Lib_Card.CardObject.InsertD(obj_cupNo + " The dyeing and fixing of the solution cup have been completed", "Dye");
                    Lib_Card.CardObject.DeleteD(s_insert);
                }
                FADM_Object.Communal._b_finshRun = true;

            }
            //复位当前杯使用状态
            FADM_Object.Communal._fadmSqlserver.ReviseData(
                "UPDATE cup_details SET FormulaCode = null, " +
                "DyeingCode = null, IsUsing = 0, Statues = '待机', " +
                "StartTime = null, SetTemp = null, StepNum = null, TotalWeight = null, " +
                "TotalStep = null, TechnologyName = null, StepStartTime = null, SetTime = null,RecordIndex = 0, Cooperate = 0 WHERE CupNum = " + obj_cupNo + " AND Statues != '下线';");
            FADM_Auto.Dye._cup_Temps[Convert.ToInt32(obj_cupNo) - 1]._b_finish = false;

        }

        public static void DyeHMIWriteSigle(int i_erea, int i_index, int i_start, int i_num, int[] ia_values)
        {

            if (i_erea == 0)
            {
                if (!FADM_Object.Communal._tcpDyeHMI1._b_Connect)
                {
                    FADM_Object.Communal._tcpDyeHMI1.ReConnect();
                }
                FADM_Object.Communal._tcpDyeHMI1.Write(i_start + i_num * i_index, ia_values);
            }
            else if (i_erea == 1)
            {
                if (!FADM_Object.Communal._tcpDyeHMI2._b_Connect)
                {
                    FADM_Object.Communal._tcpDyeHMI2.ReConnect();
                }
                FADM_Object.Communal._tcpDyeHMI2.Write(i_start + i_num * i_index, ia_values);
            }
            else if (i_erea == 2)
            {
                if (!FADM_Object.Communal._tcpDyeHMI3._b_Connect)
                {
                    FADM_Object.Communal._tcpDyeHMI3.ReConnect();
                }
                FADM_Object.Communal._tcpDyeHMI3.Write(i_start + i_num * i_index, ia_values);
            }
            else if (i_erea == 3)
            {
                if (!FADM_Object.Communal._tcpDyeHMI4._b_Connect)
                {
                    FADM_Object.Communal._tcpDyeHMI4.ReConnect();
                }
                FADM_Object.Communal._tcpDyeHMI4.Write(i_start + i_num * i_index, ia_values);
            }
            else if (i_erea == 4)
            {
                if (!FADM_Object.Communal._tcpDyeHMI5._b_Connect)
                {
                    FADM_Object.Communal._tcpDyeHMI5.ReConnect();
                }
                FADM_Object.Communal._tcpDyeHMI5.Write(i_start + i_num * i_index, ia_values);
            }
            else if (i_erea == 5)
            {
                if (!FADM_Object.Communal._tcpDyeHMI6._b_Connect)
                {
                    FADM_Object.Communal._tcpDyeHMI6.ReConnect();
                }
                FADM_Object.Communal._tcpDyeHMI6.Write(i_start + i_num * i_index, ia_values);
            }
        }
    }
}
