using Lib_DataBank.MySQL;
using Lib_File;
using Newtonsoft.Json.Linq;
using SmartDyeing.FADM_Object;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SmartDyeing.FADM_Auto.Dye;
using static SmartDyeing.FADM_Object.Communal;
using static System.Windows.Forms.AxHost;

namespace SmartDyeing.FADM_Auto
{
    internal class MyAbsorbance
    {
        //读取错误累计次数
        public static int _i_errCount = 0;

        public static AbsData[] _abs_Temps = new AbsData[4];

        /// <summary>
        /// 波长
        /// </summary>
        public static List<double> BC = new List<double>();

        /// <summary>
        /// 是否断电
        /// </summary>
        public static int _i_block = 0;

        /// <summary>
        /// 判断是否主动测白点时需要洗杯，当洗杯完后需要主动测白点
        /// </summary>
        public static bool[] _b_wash = { false, false,false, false };

        static int _i_max1 = 0;
        static double _d_absMax1 = 0.0;
        static string _s_abs1 = "";
        static int _i_reCount1 = 0;
        static int _i_max2 = 0;
        static double _d_absMax2 = 0.0;
        static string _s_abs2 = "";
        static int _i_reCount2 = 0;


       

        public static void Abs()
        {
            //if (_abs_Temps[j]._s_datarequest == "1")
            {
                int j = 1;
                //查询当前杯是检测标准样还是测试样
                string s_sql = "SELECT * from abs_cup_details WHERE CupNum = " + (j + 1) + ";";

                DataTable dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);


                //计算
                string[] sa_e1_t = "16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383/16383".Split('/');
                List<string> lis_sta_e1_t = sa_e1_t.ToList();
                string[] sa_e2_t = "2276/2808/3601/4139/4421/4513/2451/2946/3653/4484/5460/6390/7288/8268/9089/9835/10546/11084/11525/11426/4641/4917/5176/5391/5440/5309/4992/4608/4219/3838/3514/3151/2764/2429/2146/2006/1988/2067/2168/2238/2262/2295/2457/2858/3686/5214/3355/4728/6213/7509/8686/9545/10239/10792/11271/11720/12124/12420/12631/12770/12858/12940/13053/13170/13362/13601/13949/14356/14906/17022/19149/20623/21126".Split('/');
                List<string> lis_sta_e2_t = sa_e2_t.ToList();
                string[] sa_wl_t = "340/345/350/355/360/365/370/375/380/385/390/395/400/405/410/415/420/425/430/435/440/445/450/455/460/465/470/475/480/485/490/495/500/505/510/515/520/525/530/535/540/545/550/555/560/565/570/575/580/585/590/595/600/605/610/615/620/625/630/635/640/645/650/655/660/665/670/675/680/685/690/695/700".Split('/');
                List<string> lis_sta_wl_t = sa_wl_t.ToList();

                List<int> lis_e1 = new List<int>();
                List<int> lis_e2 = new List<int>();
                List<int> lis_wl = new List<int>();
                for (int i = 0; i < lis_sta_e1_t.Count; i++)
                {
                    lis_e1.Add(Convert.ToInt32(lis_sta_e1_t[i]));
                    lis_e2.Add(Convert.ToInt32(lis_sta_e2_t[i]));
                    lis_wl.Add(Convert.ToInt32(lis_sta_wl_t[i]));
                }

                if (dt_temp.Rows.Count > 0)
                {
                    {

                        //查询当前杯是检测纯水基准样还是测试样
                        s_sql = "SELECT * from standard where Type = 1 ;";

                        DataTable dt_standard = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                        //查找上一次母液瓶检测数据
                        s_sql = "SELECT * from bottle_details where BottleNum = " + dt_temp.Rows[0]["BottleNum"].ToString() + " ;";

                        DataTable dt_bottle_Info = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                        if (dt_standard.Rows.Count > 0 && dt_bottle_Info.Rows.Count > 0)
                        {


                            //计算
                            string[] sa_e1 = dt_standard.Rows[0]["E1"].ToString().Split('/');
                            List<string> lis_sta_e1 = sa_e1.ToList();
                            string[] sa_e2 = dt_standard.Rows[0]["E2"].ToString().Split('/');
                            List<string> lis_sta_e2 = sa_e2.ToList();
                            string[] sa_wl = dt_standard.Rows[0]["WL"].ToString().Split('/');
                            List<string> lis_sta_wl = sa_wl.ToList();

                            List<double> XGD = new List<double>();
                            List<double> XGD_Cal = new List<double>();

                            //if (sa_e1.Length != lis_e2.Count)
                            //{
                            //    continue;
                            //}
                            //计算系数
                            List<double> lis_dis = new List<double>();
                            for (int i = 0; i < sa_e1.Length; i++)
                            {
                                lis_dis.Add(1.0);
                            }
                            //判断是否上次保存了测量值白点数据
                            if (dt_bottle_Info.Rows[0]["StandardE1"] is DBNull || dt_bottle_Info.Rows[0]["StandardE2"] is DBNull || dt_bottle_Info.Rows[0]["WL"] is DBNull)
                            { }
                            else
                            {
                                ////查找上一次白点数据
                                //string[] sa_bottle_e1 = dt_bottle_Info.Rows[0]["StandardE1"].ToString().Split('/');
                                //List<string> lis_sta_bottle_e1 = sa_bottle_e1.ToList();
                                //string[] sa_bottle_e2 = dt_bottle_Info.Rows[0]["StandardE2"].ToString().Split('/');
                                //List<string> lis_sta_bottle_e2 = sa_bottle_e2.ToList();
                                //string[] sa_bottle_wl = dt_bottle_Info.Rows[0]["WL"].ToString().Split('/');
                                //List<string> lis_sta_bottle_wl = sa_bottle_wl.ToList();

                                //if (sa_bottle_wl.Length == sa_wl.Length && sa_bottle_wl[0] == sa_wl[0])
                                //{

                                //    for (int i = 0; i < sa_e1.Length; i++)
                                //    {
                                //        lis_dis[i] = Convert.ToDouble(lis_sta_e2[i]) / Convert.ToDouble(lis_sta_bottle_e2[i]);
                                //    }
                                //    //string ss = "";
                                //    //for (int i = 0; i < sa_e1.Length; i++)
                                //    //{
                                //    //    ss += lis_dis[i].ToString() + "/";
                                //    //}
                                //    //Lib_Log.Log.writeLogException(ss);
                                //}
                            }
                            BC.Clear();
                            //spectrumData.Clear();
                            //解析数据
                            for (int i = 0; i < sa_e1.Length; i++)
                            {
                                //计算
                                double d = (lis_e2[i] / Convert.ToDouble(lis_sta_e2[i])) / (lis_e1[i] / Convert.ToDouble(lis_sta_e1[i]));
                                double x;
                                if (d < 0.0001)
                                    x = 4;
                                else
                                    x = -Math.Log10(d);
                                //修正比例
                                x *= lis_dis[i];

                                XGD.Add(x);
                                if (Convert.ToDouble(lis_sta_wl[i]) >= 400)
                                {
                                    XGD_Cal.Add(x);
                                }
                                BC.Add(Convert.ToInt32(dt_temp.Rows[0]["StartWave"].ToString()) + Convert.ToInt32(dt_temp.Rows[0]["IntWave"].ToString()) * i);
                            }
                            string s_xgd = "";

                            double d_max = -999;
                            int i_max = 0;
                            for (int i = 0; i < XGD.Count; i++)
                            {
                                s_xgd += XGD[i].ToString("0.000000") + "/";

                                //查找波峰值
                                if (XGD[i] > d_max)
                                {
                                    d_max = XGD[i];
                                    i_max = i;
                                }
                            }

                            if (j == 0)
                            {
                                _i_max1 = i_max;
                                _d_absMax1 = d_max;
                            }
                            else
                            {
                                _i_max2 = i_max;
                                _d_absMax2 = d_max;
                            }
                            if (s_xgd != "")
                            {
                                s_xgd.Remove(s_xgd.Length - 1, 1);
                            }

                            //记录测量结果E1，E2
                            string s_e1 = "", s_e2 = "", s_wl = "";
                            for (int i = 0; i < lis_wl.Count; i++)
                            {
                                s_wl += lis_wl[i].ToString() + "/";
                            }
                            for (int i = 0; i < lis_e1.Count; i++)
                            {
                                s_e1 += lis_e1[i].ToString() + "/";
                            }
                            for (int i = 0; i < lis_e2.Count; i++)
                            {
                                s_e2 += lis_e2[i].ToString() + "/";
                            }
                            if (s_wl != "")
                            {
                                s_wl = s_wl.Remove(s_wl.Length - 1);
                            }
                            if (s_e1 != "")
                            {
                                s_e1 = s_e1.Remove(s_e1.Length - 1);
                            }
                            if (s_e2 != "")
                            {
                                s_e2 = s_e2.Remove(s_e2.Length - 1);
                            }

                            //// 计算XYZ值


                            double[] doubles = XGD_Cal.ToArray();
                            (double L, double A, double B) = CalculateLAB(doubles, Convert.ToInt32(dt_temp.Rows[0]["StartWave"].ToString()), Convert.ToInt32(dt_temp.Rows[0]["EndWave"].ToString()), Convert.ToInt32(dt_temp.Rows[0]["IntWave"].ToString()), 10);

                            //// 转换为LAB值
                            //(double L, double A, double B, double X, double Y, double Z) = ConvertToLAB(doubles);

                            //Console.WriteLine($"L: {L}, A: {A}, B: {B}");

                            //double L = 0.0; double A = 0.0; double B= 0.0;
                            double d_compensate = 0.0;
                            //判断当前检测是开料还是开稀
                            DataTable dt_bottle_detail = FADM_Object.Communal._fadmSqlserver.GetData("SELECT * from bottle_details where BottleNum = '" + dt_temp.Rows[0]["BottleNum"].ToString() + "';");
                            if (dt_bottle_detail.Rows.Count > 0)
                            {
                                if (dt_bottle_detail.Rows[0]["OriginalBottleNum"].ToString() == "0")
                                {
                                    //d_compensate = 0.0;
                                    d_compensate = Convert.ToDouble(dt_bottle_detail.Rows[0]["RealConcentration"].ToString());
                                }
                                else
                                {

                                    //获取标准曲线，然后计算补偿值
                                    s_sql = "SELECT *  FROM assistant_details WHERE AssistantCode = '" + dt_bottle_detail.Rows[0]["AssistantCode"].ToString() + "';";
                                    DataTable dt_assistant_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                                    double d_max_temp = -999;
                                    if (dt_assistant_details.Rows.Count > 0)
                                    {
                                        string s_data1 = dt_assistant_details.Rows[0]["Abs"] is DBNull ? "" : dt_assistant_details.Rows[0]["Abs"].ToString();
                                        if (s_data1 != "")
                                        {
                                            s_data1 = s_data1.Substring(0, s_data1.Length - 2);
                                            string[] sa_arr1 = s_data1.Split('/');
                                            for (int i = 0; i < sa_arr1.Count(); i++)
                                            {

                                                //查找波峰值
                                                if (Convert.ToDouble(sa_arr1[i]) > d_max_temp)
                                                {
                                                    d_max_temp = Convert.ToDouble(sa_arr1[i]);
                                                }
                                            }

                                            //计算补偿系数
                                            //d_compensate=(d_max_temp- d_max) / d_max_temp;

                                            d_compensate = 1 - (d_max_temp - d_max) / d_max_temp;

                                            d_compensate = Convert.ToDouble(dt_bottle_detail.Rows[0]["RealConcentration"].ToString()) * d_compensate;

                                        }
                                    }
                                    else
                                    {
                                        d_compensate = Convert.ToDouble(dt_bottle_detail.Rows[0]["RealConcentration"].ToString());
                                    }
                                }
                            }
                            else
                            {
                                d_compensate = Convert.ToDouble(dt_bottle_detail.Rows[0]["RealConcentration"].ToString());
                            }
                            //当正常测量时，不再保存修正系数
                            if (dt_temp.Rows[0]["Type"].ToString() == "11")
                            {


                                FADM_Object.Communal._fadmSqlserver.ReviseData("Update  bottle_details Set Abs = '" + s_xgd + "',L='" + L.ToString() + "',A='" + A.ToString() + "',B='" + B.ToString() + "',StandardE1='" + dt_standard.Rows[0]["E1"].ToString() + "',StandardE2='" + dt_standard.Rows[0]["E2"].ToString() + "',E1='" + s_e1 + "',E2='" + s_e2 + "',WL='" + dt_standard.Rows[0]["WL"].ToString() + "',RealConcentration='" + d_compensate.ToString("f6") + "' where BottleNum = '" + dt_temp.Rows[0]["BottleNum"].ToString() + "';");

                            }
                            else
                            {

                                FADM_Object.Communal._fadmSqlserver.ReviseData("Update  bottle_details Set Abs = '" + s_xgd + "',L='" + L.ToString() + "',A='" + A.ToString() + "',B='" + B.ToString() + "',StandardE1='" + dt_standard.Rows[0]["E1"].ToString() + "',StandardE2='" + dt_standard.Rows[0]["E2"].ToString() + "',E1='" + s_e1 + "',E2='" + s_e2 + "',WL='" + dt_standard.Rows[0]["WL"].ToString() + "' where BottleNum = '" + dt_temp.Rows[0]["BottleNum"].ToString() + "';");
                            }

                            //拷贝到历史表
                            DataTable dt_Temp = FADM_Object.Communal._fadmSqlserver.GetData(
                                 "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'abs_cup_details';");
                            string s_columnDetails = null;
                            foreach (DataRow row in dt_Temp.Rows)
                            {
                                if (Convert.ToString(row[0]) != "Cooperate" && Convert.ToString(row[0]) != "NeedPulse" && Convert.ToString(row[0]) != "Choose"
                                    && Convert.ToString(row[0]) != "WaterFinish" && Convert.ToString(row[0]) != "TotalWeight")
                                    s_columnDetails += Convert.ToString(row[0]) + ", ";
                            }
                            s_columnDetails = s_columnDetails.Remove(s_columnDetails.Length - 2);

                            s_sql = "SELECT *  FROM bottle_details WHERE BottleNum = '" + dt_temp.Rows[0]["BottleNum"].ToString() + "';";
                            DataTable dt_bottle = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);



                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "INSERT INTO history_abs (" + s_columnDetails + ",A,B,L,FinishTime,Abs,BrewingData,RealConcentration,AssistantCode,BaseTestTime,StandardE1,StandardE2,E1,E2,WL) (SELECT " + s_columnDetails + ",'" + A.ToString() + "' as A," + "'" + B.ToString() + "' as B," + "'" + L.ToString() + "' as L," + "'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' as FinishTime," + "'" + s_xgd.ToString() + "' as Abs,'" + dt_bottle.Rows[0]["BrewingData"].ToString() + "' as BrewingData," + dt_bottle.Rows[0]["RealConcentration"].ToString() + " as RealConcentration,'" + dt_bottle.Rows[0]["AssistantCode"].ToString() + "' as AssistantCode,'" + dt_standard.Rows[0]["FinishTime"].ToString() + " ' as BaseTestTime,'" + dt_standard.Rows[0]["E1"].ToString() + "' as StandardE1,'" + dt_standard.Rows[0]["E2"].ToString() + "' as StandardE2,'" + s_e1 + "' as E1,'" + s_e2 + "' as E2,'" + dt_standard.Rows[0]["WL"].ToString() + "' as WL" + " FROM abs_cup_details " +
                                "WHERE CupNum =" + (j + 1) + ") ;");


                        }
                    }
                }
            }
        }

        /// <summary>
        /// Type 的值
        /// 2 时间到了测白点->到5
        ///6 时间到了润白点->到2
        ///4主动测白点
        ///5润测量点->到1
        ///7主动润白点->到4
        ///1测试点
        ///8时间到了润白点（测补偿点）->到9
        ///9时间到了测白点，然后测补偿测试点->到10
        ///10润测试点(补偿)->到11
        ///11测试点(补偿)
        /// </summary>
        public static void Absorbance()
        {
            try
            {
                _i_errCount = 0;
                //吸光度机通讯
                while (true)
                {
                    int[] ia_array = new int[40];
                    //int i_state = FADM_Object.Communal._tcpModBusAbs.Read(13188, 40, ref ia_array);
                    //if (i_state != -1)
                    ia_array = Communal._ia_d13188;
                    {
                        _abs_Temps[0]._s_currentState= ia_array[0].ToString();
                        _abs_Temps[0]._s_request = ia_array[1].ToString();
                        _abs_Temps[0]._s_datarequest = ia_array[2].ToString();
                        _abs_Temps[0]._s_boottlenum = ia_array[3].ToString();
                        _abs_Temps[0]._s_totaldata = ia_array[4].ToString();
                        _abs_Temps[0]._s_warm = ia_array[5].ToString();
                        _abs_Temps[0]._s_history = ia_array[7].ToString();

                        _abs_Temps[1]._s_currentState = ia_array[10].ToString();
                        _abs_Temps[1]._s_request = ia_array[11].ToString();
                        _abs_Temps[1]._s_datarequest = ia_array[12].ToString();
                        _abs_Temps[1]._s_boottlenum = ia_array[13].ToString();
                        _abs_Temps[1]._s_totaldata = ia_array[14].ToString();
                        _abs_Temps[1]._s_warm = ia_array[15].ToString();

                        _i_block = Convert.ToInt32( ia_array[6].ToString());
                        _abs_Temps[1]._s_history = ia_array[17].ToString();

                        _abs_Temps[2]._s_currentState = ia_array[20].ToString();
                        _abs_Temps[2]._s_request = ia_array[21].ToString();
                        _abs_Temps[2]._s_datarequest = ia_array[22].ToString();
                        _abs_Temps[2]._s_boottlenum = ia_array[23].ToString();
                        _abs_Temps[2]._s_totaldata = ia_array[24].ToString();
                        _abs_Temps[2]._s_warm = ia_array[25].ToString();
                        _abs_Temps[2]._s_history = ia_array[27].ToString();

                        _abs_Temps[3]._s_currentState = ia_array[30].ToString();
                        _abs_Temps[3]._s_request = ia_array[31].ToString();
                        _abs_Temps[3]._s_datarequest = ia_array[32].ToString();
                        _abs_Temps[3]._s_boottlenum = ia_array[33].ToString();
                        _abs_Temps[3]._s_totaldata = ia_array[34].ToString();
                        _abs_Temps[3]._s_warm = ia_array[35].ToString();
                        _abs_Temps[3]._s_history = ia_array[37].ToString();
                    }
                    //else
                    //{
                    //    //FADM_Object.Communal._tcpModBusBrew._b_Connect = false;
                    //    FADM_Object.Communal._tcpModBusAbs.ReConnect();
                    //    _i_errCount++;
                    //    if (_i_errCount > 5)
                    //    {
                    //        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    //            throw new Exception("吸光度机通讯异常");
                    //        else
                    //            throw new Exception("The absorbent communication is abnormal");
                    //    }
                    //    else
                    //    {
                    //        continue;
                    //    }
                    //}
                    for (int j = 0; j < 4; j++)
                    {
                        //读取到异常信息
                        if (_abs_Temps[j]._s_currentState == "4")
                        {
                            string s_w = "";
                            //显示报警信息
                            if(_abs_Temps[j]._s_warm =="1")
                            {
                                s_w = "分光仪通信异常";
                            }
                            else if (_abs_Temps[j]._s_warm == "2")
                            {
                                s_w = "数据读取异常";
                            }
                            else if (_abs_Temps[j]._s_warm == "3")
                            {
                                s_w = "数据转换失败";
                            }
                            else if (_abs_Temps[j]._s_warm == "4")
                            {
                                s_w = "设置开始波长失败";
                            }
                            else if (_abs_Temps[j]._s_warm == "5")
                            {
                                s_w = "设置结束波长失败";
                            }
                            else if (_abs_Temps[j]._s_warm == "6")
                            {
                                s_w = "设置采集间隔失败";
                            }
                            else if (_abs_Temps[j]._s_warm == "7")
                            {
                                s_w = "采集波长数据失败";
                            }
                            else if (_abs_Temps[j]._s_warm == "8")
                            {
                                s_w = "调零失败";
                            }
                            else if (_abs_Temps[j]._s_warm == "9")
                            {
                                s_w = "基线测试异常";
                            }
                            else if (_abs_Temps[j]._s_warm == "10")
                            {
                                s_w = "开盖异常";
                            }
                            else if (_abs_Temps[j]._s_warm == "11")
                            {
                                s_w = "关盖异常";
                            }
                            else if (_abs_Temps[j]._s_warm == "12")
                            {
                                s_w = "关盖信号已通";
                            }
                            else if (_abs_Temps[j]._s_warm == "13")
                            {
                                s_w = "开盖信号已通";
                            }
                            else if (_abs_Temps[j]._s_warm == "20")
                            {
                                s_w = "到1号检测位异常";
                            }
                            else if (_abs_Temps[j]._s_warm == "21")
                            {
                                s_w = "到2号检测位异常";
                            }

                            new FADM_Object.MyAlarm((j + 1) + "号(吸光度)"+s_w, "温馨提示");
                            //复位状态位
                            //清空请求
                            int[] values11 = new int[1];
                            values11[0] = 0;
                            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                            {
                                FADM_Object.Communal._tcpModBusAbs.ReConnect();
                            }
                            if (j == 0)
                                FADM_Object.Communal._tcpModBusAbs.Write(13188, values11);
                            else if(j == 1)
                                FADM_Object.Communal._tcpModBusAbs.Write(13198, values11);
                            else if (j == 2)
                                FADM_Object.Communal._tcpModBusAbs.Write(13208, values11);
                            else if (j == 3)
                                FADM_Object.Communal._tcpModBusAbs.Write(13218, values11);

                            string s_sql = "UPDATE abs_cup_details SET Statues='异常',Cooperate=0  WHERE CupNum = " + (j + 1) + " ;";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        }

                        if (_abs_Temps[j]._s_request == "1")
                        {
                            //加药
                            if (_abs_Temps[j]._i_requesadd == 0)
                            {
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  "UPDATE abs_cup_details SET Cooperate = 1 WHERE CupNum = " + (j+1)  + " And Cooperate = 5 ;");

                                //检查是否已把数据库更新
                                DataTable dt_cup = _fadmSqlserver.GetData("Select * from abs_cup_details WHERE CupNum = " + (j+1) + " ;");
                                if (dt_cup.Rows.Count > 0)
                                {
                                    if (dt_cup.Rows[0]["Cooperate"].ToString() == "1")
                                        _abs_Temps[j]._i_requesadd = 1;
                                }
                            }
                        }
                        else if (_abs_Temps[j]._s_request == "2")
                        {
                            //加水
                            if (_abs_Temps[j]._i_requesadd == 0)
                            {
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  "UPDATE abs_cup_details SET Cooperate = 3 WHERE CupNum = " + (j + 1) + ";");

                                //检查是否已把数据库更新
                                DataTable dt_cup = _fadmSqlserver.GetData("Select * from abs_cup_details WHERE CupNum = " + (j + 1) + " ;");
                                if (dt_cup.Rows.Count > 0)
                                {
                                    if (dt_cup.Rows[0]["Cooperate"].ToString() == "3")
                                        _abs_Temps[j]._i_requesadd = 1;
                                }
                            }
                        }
                        else if (_abs_Temps[j]._s_request == "3")
                        {
                            //泄压
                            if (_abs_Temps[j]._i_requesadd == 0)
                            {
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  "UPDATE abs_cup_details SET Cooperate = 4 WHERE CupNum = " + (j + 1) + ";");

                                //检查是否已把数据库更新
                                DataTable dt_cup = _fadmSqlserver.GetData("Select * from abs_cup_details WHERE CupNum = " + (j + 1) + " ;");
                                if (dt_cup.Rows.Count > 0)
                                {
                                    if (dt_cup.Rows[0]["Cooperate"].ToString() == "4")
                                        _abs_Temps[j]._i_requesadd = 1;
                                }
                            }
                        }
                        else if (_abs_Temps[j]._s_request == "6")
                        {
                            //洗杯加溶解剂
                            if (_abs_Temps[j]._i_requesadd == 0)
                            {
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                  "UPDATE abs_cup_details SET Cooperate = 6 WHERE CupNum = " + (j + 1) + ";");

                                //检查是否已把数据库更新
                                DataTable dt_cup = _fadmSqlserver.GetData("Select * from abs_cup_details WHERE CupNum = " + (j + 1) + " ;");
                                if (dt_cup.Rows.Count > 0)
                                {
                                    if (dt_cup.Rows[0]["Cooperate"].ToString() == "6")
                                        _abs_Temps[j]._i_requesadd = 1;
                                }
                            }
                        }
                        else
                        {
                            _abs_Temps[j]._i_requesadd = 0;
                        }

                        //当测吸光度工艺时，测量结束后先有申请保存数据，清空好保存数据后再会返回完成
                        if (_abs_Temps[j]._s_datarequest == "1")
                        {
                            //查询当前杯是检测标准样还是测试样
                            string s_sql = "SELECT * from abs_cup_details WHERE CupNum = " + (j + 1) + ";";

                            DataTable dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                            //清空请求
                            int[] values = new int[1];
                            values[0] = 0;
                            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                            {
                                FADM_Object.Communal._tcpModBusAbs.ReConnect();
                            }
                            if (j  == 0)
                                FADM_Object.Communal._tcpModBusAbs.Write(13190, values);
                            else if (j == 1)
                                FADM_Object.Communal._tcpModBusAbs.Write(13200, values);
                            else if (j == 2)
                                FADM_Object.Communal._tcpModBusAbs.Write(13210, values);
                            else if (j == 3)
                                FADM_Object.Communal._tcpModBusAbs.Write(13220, values);

                            //读取300个寄存器数据
                            int[] ia_data1 = new int[100];
                            FADM_Object.Communal._tcpModBusAbs.Read(13388, 100, ref ia_data1);
                            int[] ia_data2 = new int[100];
                            FADM_Object.Communal._tcpModBusAbs.Read(13488, 100, ref ia_data2);
                            int[] ia_data3 = new int[100];
                            FADM_Object.Communal._tcpModBusAbs.Read(13588, 100, ref ia_data3);
                            int[] ia_data4 = new int[100];
                            FADM_Object.Communal._tcpModBusAbs.Read(13688, 100, ref ia_data4);
                            int[] ia_data5 = new int[100];
                            FADM_Object.Communal._tcpModBusAbs.Read(13788, 100, ref ia_data5);
                            int[] ia_data6 = new int[100];
                            FADM_Object.Communal._tcpModBusAbs.Read(13888, 100, ref ia_data6);
                            List<int> lis_data = new List<int>();
                            for (int i = 0; i < ia_data1.Length;)
                            {
                                int a11 = ia_data1[i];
                                int a12 = ia_data1[i + 1];
                                int d_b;
                                if (a11 < 0)
                                {
                                    d_b = ((a12 + 1) * 65536 + a11);
                                }
                                else
                                {
                                    d_b = (a12 * 65536 + a11);
                                }
                                lis_data.Add(d_b);
                                i += 2;
                            }
                            for (int i = 0; i < ia_data2.Length;)
                            {
                                int a11 = ia_data2[i];
                                int a12 = ia_data2[i + 1];
                                int d_b;
                                if (a11 < 0)
                                {
                                    d_b = ((a12 + 1) * 65536 + a11);
                                }
                                else
                                {
                                    d_b = (a12 * 65536 + a11);
                                }
                                lis_data.Add(d_b);
                                i += 2;
                            }
                            for (int i = 0; i < ia_data3.Length;)
                            {
                                int a11 = ia_data3[i];
                                int a12 = ia_data3[i + 1];
                                int d_b;
                                if (a11 < 0)
                                {
                                    d_b = ((a12 + 1) * 65536 + a11);
                                }
                                else
                                {
                                    d_b = (a12 * 65536 + a11);
                                }
                                lis_data.Add(d_b);
                                i += 2;
                            }
                            for (int i = 0; i < ia_data4.Length;)
                            {
                                int a11 = ia_data4[i];
                                int a12 = ia_data4[i + 1];
                                int d_b;
                                if (a11 < 0)
                                {
                                    d_b = ((a12 + 1) * 65536 + a11);
                                }
                                else
                                {
                                    d_b = (a12 * 65536 + a11);
                                }
                                lis_data.Add(d_b);
                                i += 2;
                            }

                            for (int i = 0; i < ia_data5.Length;)
                            {
                                int a11 = ia_data5[i];
                                int a12 = ia_data5[i + 1];
                                int d_b;
                                if (a11 < 0)
                                {
                                    d_b = ((a12 + 1) * 65536 + a11);
                                }
                                else
                                {
                                    d_b = (a12 * 65536 + a11);
                                }
                                lis_data.Add(d_b);
                                i += 2;
                            }

                            for (int i = 0; i < ia_data6.Length;)
                            {
                                int a11 = ia_data6[i];
                                int a12 = ia_data6[i + 1];
                                int d_b;
                                if (a11 < 0)
                                {
                                    d_b = ((a12 + 1) * 65536 + a11);
                                }
                                else
                                {
                                    d_b = (a12 * 65536 + a11);
                                }
                                lis_data.Add(d_b);
                                i += 2;
                            }
                            List<int> lis_e1 = new List<int>();
                            List<int> lis_e2 = new List<int>();
                            List<int> lis_wl = new List<int>();
                            for (int i = 0; i < lis_data.Count; i++)
                            {
                                if ((i + 1) % 3 == 1)
                                {
                                    lis_e1.Add(lis_data[i]);
                                }
                                else if ((i + 1) % 3 == 2)
                                {
                                    lis_e2.Add(lis_data[i]);
                                }
                                else if ((i + 1) % 3 == 0)
                                {
                                    lis_wl.Add(lis_data[i]);
                                    //当波长为0就结束取数
                                    if (lis_data[i] == 0 && i != 2)
                                    {
                                        //移除最后一个加入的数值
                                        lis_wl.RemoveAt(lis_wl.Count - 1);
                                        lis_e2.RemoveAt(lis_e2.Count - 1);
                                        lis_e1.RemoveAt(lis_e1.Count - 1);
                                        break;
                                    }
                                }
                            }

                            //读取基线数据
                            //读取300个寄存器数据
                            int[] ia_data1_stand = new int[100];
                            FADM_Object.Communal._tcpModBusAbs.Read(13988, 100, ref ia_data1_stand);
                            int[] ia_data2_stand = new int[100];
                            FADM_Object.Communal._tcpModBusAbs.Read(14088, 100, ref ia_data2_stand);
                            int[] ia_data3_stand = new int[100];
                            FADM_Object.Communal._tcpModBusAbs.Read(14188, 100, ref ia_data3_stand);
                            int[] ia_data4_stand = new int[100];
                            FADM_Object.Communal._tcpModBusAbs.Read(14288, 100, ref ia_data4_stand);
                            int[] ia_data5_stand = new int[100];
                            FADM_Object.Communal._tcpModBusAbs.Read(14388, 100, ref ia_data5_stand);
                            int[] ia_data6_stand = new int[100];
                            FADM_Object.Communal._tcpModBusAbs.Read(14488, 100, ref ia_data6_stand);
                            List<int> lis_data_stand = new List<int>();
                            for (int i = 0; i < ia_data1_stand.Length;)
                            {
                                int a11 = ia_data1_stand[i];
                                int a12 = ia_data1_stand[i + 1];
                                int d_b;
                                if (a11 < 0)
                                {
                                    d_b = ((a12 + 1) * 65536 + a11);
                                }
                                else
                                {
                                    d_b = (a12 * 65536 + a11);
                                }
                                lis_data_stand.Add(d_b);
                                i += 2;
                            }
                            for (int i = 0; i < ia_data2_stand.Length;)
                            {
                                int a11 = ia_data2_stand[i];
                                int a12 = ia_data2_stand[i + 1];
                                int d_b;
                                if (a11 < 0)
                                {
                                    d_b = ((a12 + 1) * 65536 + a11);
                                }
                                else
                                {
                                    d_b = (a12 * 65536 + a11);
                                }
                                lis_data_stand.Add(d_b);
                                i += 2;
                            }
                            for (int i = 0; i < ia_data3_stand.Length;)
                            {
                                int a11 = ia_data3_stand[i];
                                int a12 = ia_data3_stand[i + 1];
                                int d_b;
                                if (a11 < 0)
                                {
                                    d_b = ((a12 + 1) * 65536 + a11);
                                }
                                else
                                {
                                    d_b = (a12 * 65536 + a11);
                                }
                                lis_data_stand.Add(d_b);
                                i += 2;
                            }
                            for (int i = 0; i < ia_data4_stand.Length;)
                            {
                                int a11 = ia_data4_stand[i];
                                int a12 = ia_data4_stand[i + 1];
                                int d_b;
                                if (a11 < 0)
                                {
                                    d_b = ((a12 + 1) * 65536 + a11);
                                }
                                else
                                {
                                    d_b = (a12 * 65536 + a11);
                                }
                                lis_data_stand.Add(d_b);
                                i += 2;
                            }

                            for (int i = 0; i < ia_data5_stand.Length;)
                            {
                                int a11 = ia_data5_stand[i];
                                int a12 = ia_data5_stand[i + 1];
                                int d_b;
                                if (a11 < 0)
                                {
                                    d_b = ((a12 + 1) * 65536 + a11);
                                }
                                else
                                {
                                    d_b = (a12 * 65536 + a11);
                                }
                                lis_data_stand.Add(d_b);
                                i += 2;
                            }

                            for (int i = 0; i < ia_data6_stand.Length;)
                            {
                                int a11 = ia_data6_stand[i];
                                int a12 = ia_data6_stand[i + 1];
                                int d_b;
                                if (a11 < 0)
                                {
                                    d_b = ((a12 + 1) * 65536 + a11);
                                }
                                else
                                {
                                    d_b = (a12 * 65536 + a11);
                                }
                                lis_data_stand.Add(d_b);
                                i += 2;
                            }
                            List<int> lis_e1_stand = new List<int>();
                            List<int> lis_e2_stand = new List<int>();
                            List<int> lis_wl_stand = new List<int>();
                            for (int i = 0; i < lis_data_stand.Count; i++)
                            {
                                if ((i + 1) % 3 == 1)
                                {
                                    lis_e1_stand.Add(lis_data_stand[i]);
                                }
                                else if ((i + 1) % 3 == 2)
                                {
                                    lis_e2_stand.Add(lis_data_stand[i]);
                                }
                                else if ((i + 1) % 3 == 0)
                                {
                                    lis_wl_stand.Add(lis_data_stand[i]);
                                    //当波长为0就结束取数
                                    if (lis_data_stand[i] == 0 && i != 2)
                                    {
                                        //移除最后一个加入的数值
                                        lis_wl_stand.RemoveAt(lis_wl_stand.Count - 1);
                                        lis_e2_stand.RemoveAt(lis_e2_stand.Count - 1);
                                        lis_e1_stand.RemoveAt(lis_e1_stand.Count - 1);
                                        break;
                                    }
                                }
                            }
                            string s_e1_stand = "", s_e2_stand = "", s_wl_stand = "";
                            for (int i = 0; i < lis_wl_stand.Count; i++)
                            {
                                s_wl_stand += lis_wl_stand[i].ToString() + "/";
                            }
                            for (int i = 0; i < lis_e1_stand.Count; i++)
                            {
                                s_e1_stand += lis_e1_stand[i].ToString() + "/";
                            }
                            for (int i = 0; i < lis_e2_stand.Count; i++)
                            {
                                s_e2_stand += lis_e2_stand[i].ToString() + "/";
                            }
                            if (s_wl_stand != "")
                            {
                                s_wl_stand = s_wl_stand.Remove(s_wl_stand.Length - 1);
                            }
                            if (s_e1_stand != "")
                            {
                                s_e1_stand = s_e1_stand.Remove(s_e1_stand.Length - 1);
                            }
                            if (s_e2_stand != "")
                            {
                                s_e2_stand = s_e2_stand.Remove(s_e2_stand.Length - 1);
                            }

                            s_sql = "insert into  history_absstandard(E1,E2,WL,FinishTime,Type) Values ('" + s_e1_stand + "','" + s_e2_stand + "','" + s_wl_stand + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',1);";

                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                            FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from standard where Type = 1");

                            s_sql = "insert into  standard(E1,E2,WL,FinishTime,Type) Values ('" + s_e1_stand + "','" + s_e2_stand + "','" + s_wl_stand + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',1);";

                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                            if (dt_temp.Rows.Count > 0)
                            {
                                //纯水基准样
                                if (dt_temp.Rows[0]["Type"].ToString() == "2" || dt_temp.Rows[0]["Type"].ToString() == "9")
                                {
                                    string s_e1 = "", s_e2 = "", s_wl = "";
                                    for (int i = 0; i < lis_wl.Count; i++)
                                    {
                                        s_wl += lis_wl[i].ToString() + "/";
                                    }
                                    for (int i = 0; i < lis_e1.Count; i++)
                                    {
                                        s_e1 += lis_e1[i].ToString() + "/";
                                    }
                                    for (int i = 0; i < lis_e2.Count; i++)
                                    {
                                        s_e2 += lis_e2[i].ToString() + "/";
                                    }
                                    if (s_wl != "")
                                    {
                                        s_wl = s_wl.Remove(s_wl.Length - 1);
                                    }
                                    if (s_e1 != "")
                                    {
                                        s_e1 = s_e1.Remove(s_e1.Length - 1);
                                    }
                                    if (s_e2 != "")
                                    {
                                        s_e2 = s_e2.Remove(s_e2.Length - 1);
                                    }

                                    string s_info = "";

                                    string s_dic = "";

                                    s_sql = "insert into  history_absstandard(E1,E2,WL,FinishTime,Type) Values ('" + s_e1 + "','" + s_e2 + "','" + s_wl + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',0);";

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                    //判断这次是否第一次测白点
                                    DataTable dt_temp_first = FADM_Object.Communal._fadmSqlserver.GetData("SELECT * from standard where Type = 0;");
                                    if (dt_temp_first.Rows.Count == 0)
                                    {
                                        s_sql = "insert into  standard(E1,E2,WL,FinishTime,Type) Values ('" + s_e1 + "','" + s_e2 + "','" + s_wl + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',0);";

                                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                        s_info = "";
                                    }
                                    else
                                    {
                                        if(FADM_Object.Communal._fadmSqlserver.GetData("select * from standard where Type = 1").Rows.Count>0)
                                        FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from standard where Type = 0 ");

                                        //把上一次数据替换
                                        FADM_Object.Communal._fadmSqlserver.ReviseData("Update standard set Type = 0 where Type = 1 ");

                                        s_sql = "insert into  standard(E1,E2,WL,FinishTime,Type) Values ('" + s_e1 + "','" + s_e2 + "','" + s_wl + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',1);";

                                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                        //查询第一次测的数据
                                        //计算
                                        string[] sa_e1 = dt_temp_first.Rows[0]["E1"].ToString().Split('/');
                                        List<string> lis_sta_e1 = sa_e1.ToList();
                                        string[] sa_e2 = dt_temp_first.Rows[0]["E2"].ToString().Split('/');
                                        List<string> lis_sta_e2 = sa_e2.ToList();
                                        string[] sa_wl = dt_temp_first.Rows[0]["WL"].ToString().Split('/');
                                        List<string> lis_sta_wl = sa_wl.ToList();
                                        double d_e1 = -999;
                                        int i_wl = 0;
                                        double d_e12 = -999;
                                        int i_wl2 = 0;
                                        for (int i = 0; i < lis_sta_wl.Count; i++)
                                        {
                                            if (Convert.ToDouble(lis_sta_e2[i]) > d_e1)
                                            {
                                                d_e1 = Convert.ToDouble(lis_sta_e2[i]);
                                                i_wl = Convert.ToInt32(lis_sta_wl[i]);
                                            }
                                        }
                                        s_info += "第一次测量数据最大值：" + d_e1 + "波长：" + i_wl + ";";

                                        for (int i = 0; i < lis_e2.Count; i++)
                                        {
                                            if (Convert.ToDouble(lis_e2[i]) > d_e12)
                                            {
                                                d_e12 = Convert.ToDouble(lis_e2[i]);
                                                i_wl2 = Convert.ToInt32(lis_wl[i]);
                                            }
                                        }
                                        s_info += "第二次测量数据最大值：" + d_e12 + "波长：" + i_wl2 + ";";

                                        for (int i = 0; i < lis_sta_wl.Count; i++)
                                        {
                                            s_dic += Convert.ToInt32(lis_e2[i]) - Convert.ToInt32(lis_sta_e2[i]) + "/";
                                        }
                                    }

                                    //发送启动
                                    int[] values_Zero = new int[1];
                                    values_Zero[0] = 0;
                                    if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                                    {
                                        FADM_Object.Communal._tcpModBusAbs.ReConnect();
                                    }
                                    FADM_Object.Communal._tcpModBusAbs.Write(13194, values_Zero);

                                    //拷贝到历史表
                                    DataTable dt_Temp = FADM_Object.Communal._fadmSqlserver.GetData(
                                         "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'abs_cup_details';");
                                    string s_columnDetails = null;
                                    foreach (DataRow row in dt_Temp.Rows)
                                    {
                                        if (Convert.ToString(row[0]) != "Cooperate" && Convert.ToString(row[0]) != "NeedPulse" && Convert.ToString(row[0]) != "Choose"
                                            && Convert.ToString(row[0]) != "WaterFinish" && Convert.ToString(row[0]) != "TotalWeight")
                                            s_columnDetails += Convert.ToString(row[0]) + ", ";
                                    }
                                    s_columnDetails = s_columnDetails.Remove(s_columnDetails.Length - 2);


                                    

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                         "INSERT INTO history_abs (" + s_columnDetails + ",FinishTime,Result,Abs) (SELECT " + s_columnDetails + ",'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' as FinishTime" + ",'" + s_info + "' as Result" + ",'" + s_dic + "' as Abs" + " FROM abs_cup_details " +
                                         "WHERE CupNum =" + (j + 1) + ") ;");

                                    FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", (j + 1) + "号(吸光度)检测完成");

                                    //new FADM_Object.MyAlarm((j + 1) + "号(吸光度)检测完成", "温馨提示");

                                    string s_insert = Lib_Card.CardObject.InsertD((j + 1) + "号(吸光度)检测完成", "温馨提示");
                                    Thread.Sleep(2000);
                                    Lib_Card.CardObject.DeleteD(s_insert);
                                    
                                }
                                //主动测试基准点
                                else if (dt_temp.Rows[0]["Type"].ToString() == "4")
                                {
                                    string s_e1 = "", s_e2 = "", s_wl = "";
                                    for (int i = 0; i < lis_wl.Count; i++)
                                    {
                                        s_wl += lis_wl[i].ToString() + "/";
                                    }
                                    for (int i = 0; i < lis_e1.Count; i++)
                                    {
                                        s_e1 += lis_e1[i].ToString() + "/";
                                    }
                                    for (int i = 0; i < lis_e2.Count; i++)
                                    {
                                        s_e2 += lis_e2[i].ToString() + "/";
                                    }
                                    if (s_wl != "")
                                    {
                                        s_wl = s_wl.Remove(s_wl.Length - 1);
                                    }
                                    if (s_e1 != "")
                                    {
                                        s_e1 = s_e1.Remove(s_e1.Length - 1);
                                    }
                                    if (s_e2 != "")
                                    {
                                        s_e2 = s_e2.Remove(s_e2.Length - 1);
                                    }

                                    s_sql = "insert into  history_absstandard(E1,E2,WL,FinishTime,Type) Values ('" + s_e1 + "','" + s_e2 + "','" + s_wl + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',0);";

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                    string s_info = "";
                                    string s_dic = "";
                                    //判断这次是否第一次测白点
                                    DataTable dt_temp_first = FADM_Object.Communal._fadmSqlserver.GetData("SELECT * from standard where Type = 0;");
                                    if (dt_temp_first.Rows.Count == 0)
                                    {
                                        s_sql = "insert into  standard(E1,E2,WL,FinishTime,Type) Values ('" + s_e1 + "','" + s_e2 + "','" + s_wl + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',0);";

                                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                        s_info = "";
                                    }
                                    else
                                    {
                                        if (FADM_Object.Communal._fadmSqlserver.GetData("select * from standard where Type = 1").Rows.Count > 0)
                                            FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from standard where Type = 0 ");

                                        //把上一次数据替换
                                        FADM_Object.Communal._fadmSqlserver.ReviseData("Update standard set Type = 0 where Type = 1 ");

                                        s_sql = "insert into  standard(E1,E2,WL,FinishTime,Type) Values ('" + s_e1 + "','" + s_e2 + "','" + s_wl + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',1);";

                                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                        //查询第一次测的数据
                                        //计算
                                        string[] sa_e1 = dt_temp_first.Rows[0]["E1"].ToString().Split('/');
                                        List<string> lis_sta_e1 = sa_e1.ToList();
                                        string[] sa_e2 = dt_temp_first.Rows[0]["E2"].ToString().Split('/');
                                        List<string> lis_sta_e2 = sa_e2.ToList();
                                        string[] sa_wl = dt_temp_first.Rows[0]["WL"].ToString().Split('/');
                                        List<string> lis_sta_wl = sa_wl.ToList();
                                        double d_e1 = -999;
                                        int i_wl = 0;
                                        double d_e12 = -999;
                                        int i_wl2 = 0;
                                        for (int i = 0; i < lis_sta_wl.Count; i++)
                                        {
                                            if (Convert.ToDouble(lis_sta_e2[i]) > d_e1)
                                            {
                                                d_e1 = Convert.ToDouble(lis_sta_e2[i]);
                                                i_wl = Convert.ToInt32(lis_sta_wl[i]);
                                            }
                                        }
                                        s_info += "第一次测量数据最大值：" + d_e1 + "波长：" + i_wl + ";";

                                        for (int i = 0; i < lis_e2.Count; i++)
                                        {
                                            if (Convert.ToDouble(lis_e2[i]) > d_e12)
                                            {
                                                d_e12 = Convert.ToDouble(lis_e2[i]);
                                                i_wl2 = Convert.ToInt32(lis_wl[i]);
                                            }
                                        }
                                        s_info += "第二次测量数据最大值：" + d_e12 + "波长：" + i_wl2 + ";";

                                        for (int i = 0; i < lis_sta_wl.Count; i++)
                                        {
                                            s_dic += Convert.ToInt32(lis_e2[i]) - Convert.ToInt32(lis_sta_e2[i]) + "/";
                                        }

                                    }

                                    //发送启动
                                    int[] values_Zero = new int[1];
                                    values_Zero[0] = 0;
                                    if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                                    {
                                        FADM_Object.Communal._tcpModBusAbs.ReConnect();
                                    }
                                    FADM_Object.Communal._tcpModBusAbs.Write(13194, values_Zero);

                                    //拷贝到历史表
                                    DataTable dt_Temp = FADM_Object.Communal._fadmSqlserver.GetData(
                                         "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'abs_cup_details';");
                                    string s_columnDetails = null;
                                    foreach (DataRow row in dt_Temp.Rows)
                                    {
                                        if (Convert.ToString(row[0]) != "Cooperate" && Convert.ToString(row[0]) != "NeedPulse" && Convert.ToString(row[0]) != "Choose"
                                            && Convert.ToString(row[0]) != "WaterFinish" && Convert.ToString(row[0]) != "TotalWeight")
                                            s_columnDetails += Convert.ToString(row[0]) + ", ";
                                    }
                                    s_columnDetails = s_columnDetails.Remove(s_columnDetails.Length - 2);

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                         "INSERT INTO history_abs (" + s_columnDetails + ",FinishTime,Result,Abs) (SELECT " + s_columnDetails + ",'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' as FinishTime" + ",'" + s_info + "' as Result" + ",'" + s_dic + "' as Abs" + " FROM abs_cup_details " +
                                         "WHERE CupNum =" + (j + 1) + ") ;");

                                    FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", (j + 1) + "号(吸光度)检测完成");

                                    //new FADM_Object.MyAlarm((j + 1) + "号(吸光度)检测完成", "温馨提示");

                                    string s_insert = Lib_Card.CardObject.InsertD((j + 1) + "号(吸光度)检测完成", "温馨提示");
                                    Thread.Sleep(2000);
                                    Lib_Card.CardObject.DeleteD(s_insert);
                                    
                                }
                                //测试样
                                else
                                {

                                    //插入standard

                                    //查询当前杯是检测纯水基准样还是测试样
                                    s_sql = "SELECT * from standard where Type = 1 ;";

                                    DataTable dt_standard = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                                    //查找上一次母液瓶检测数据
                                    s_sql = "SELECT * from bottle_details where BottleNum = "+ dt_temp.Rows[0]["BottleNum"].ToString()+" ;";

                                    DataTable dt_bottle_Info = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                                    if (dt_standard.Rows.Count > 0 && dt_bottle_Info.Rows.Count >0)
                                    {
                                        

                                        //计算
                                        string[] sa_e1 = dt_standard.Rows[0]["E1"].ToString().Split('/');
                                        List<string> lis_sta_e1 = sa_e1.ToList();
                                        string[] sa_e2 = dt_standard.Rows[0]["E2"].ToString().Split('/');
                                        List<string> lis_sta_e2 = sa_e2.ToList();
                                        string[] sa_wl = dt_standard.Rows[0]["WL"].ToString().Split('/');
                                        List<string> lis_sta_wl = sa_wl.ToList();

                                        List<double> XGD = new List<double>();
                                        List<double> XGD_Cal = new List<double>();

                                        if (sa_e1.Length != lis_e2.Count)
                                        {
                                            continue;
                                        }
                                        //计算系数
                                        List<double> lis_dis = new List<double>();
                                        for (int i = 0; i < sa_e1.Length; i++)
                                        {
                                            lis_dis.Add(1.0);
                                        }
                                        ////判断是否上次保存了测量值白点数据
                                        //if (dt_bottle_Info.Rows[0]["StandardE1"] is DBNull || dt_bottle_Info.Rows[0]["StandardE2"] is DBNull || dt_bottle_Info.Rows[0]["WL"] is DBNull)
                                        //{ }
                                        //else
                                        //{
                                        //    //查找上一次白点数据
                                        //    string[] sa_bottle_e1 = dt_bottle_Info.Rows[0]["StandardE1"].ToString().Split('/');
                                        //    List<string> lis_sta_bottle_e1 = sa_bottle_e1.ToList();
                                        //    string[] sa_bottle_e2 = dt_bottle_Info.Rows[0]["StandardE2"].ToString().Split('/');
                                        //    List<string> lis_sta_bottle_e2 = sa_bottle_e2.ToList();
                                        //    string[] sa_bottle_wl = dt_bottle_Info.Rows[0]["WL"].ToString().Split('/');
                                        //    List<string> lis_sta_bottle_wl = sa_bottle_wl.ToList();

                                        //    if (sa_bottle_wl.Length == sa_wl.Length && sa_bottle_wl[0] == sa_wl[0])
                                        //    {
                                                
                                        //        for (int i = 0; i < sa_e1.Length; i++)
                                        //        {
                                        //            lis_dis[i] = Convert.ToDouble(lis_sta_e2[i])/ Convert.ToDouble(lis_sta_bottle_e2[i]);
                                        //        }
                                        //        //string ss = "";
                                        //        //for (int i = 0; i < sa_e1.Length; i++)
                                        //        //{
                                        //        //    ss += lis_dis[i].ToString() + "/";
                                        //        //}
                                        //        //Lib_Log.Log.writeLogException(ss);
                                        //    }
                                        //}
                                        BC.Clear();
                                        //spectrumData.Clear();
                                        //解析数据
                                        for (int i = 0; i < sa_e1.Length; i++)
                                        {
                                            //计算
                                            double d = (lis_e2[i] / Convert.ToDouble(lis_sta_e2[i])) / (lis_e1[i] / Convert.ToDouble(lis_sta_e1[i]));
                                            double x;
                                            if (d < 0.0001)
                                                x = 4;
                                            else
                                                x = -Math.Log10(d);
                                            //修正比例
                                            x *= lis_dis[i];

                                            XGD.Add(x);
                                            if (Convert.ToDouble(lis_sta_wl[i]) >= 400)
                                            {
                                                XGD_Cal.Add(x);
                                            }
                                            BC.Add(Convert.ToInt32(dt_temp.Rows[0]["StartWave"].ToString()) + Convert.ToInt32(dt_temp.Rows[0]["IntWave"].ToString()) * i);
                                        }
                                        string s_xgd = "";

                                        double d_max = -999;
                                        int i_max = 0;
                                        for (int i = 0; i < XGD.Count; i++)
                                        {
                                            s_xgd += XGD[i].ToString("0.000000") + "/";

                                            //查找波峰值
                                            if (XGD[i] > d_max)
                                            {
                                                d_max = XGD[i];
                                                i_max = i;
                                            }
                                        }

                                        if (j == 0)
                                        {
                                            _i_max1 = i_max;
                                            _d_absMax1 = d_max;
                                        }
                                        else
                                        {
                                            _i_max2 = i_max;
                                            _d_absMax2 = d_max;
                                        }
                                        if (s_xgd != "")
                                        {
                                            s_xgd.Remove(s_xgd.Length - 1, 1);
                                        }

                                        //记录测量结果E1，E2
                                        string s_e1 = "", s_e2 = "", s_wl = "";
                                        for (int i = 0; i < lis_wl.Count; i++)
                                        {
                                            s_wl += lis_wl[i].ToString() + "/";
                                        }
                                        for (int i = 0; i < lis_e1.Count; i++)
                                        {
                                            s_e1 += lis_e1[i].ToString() + "/";
                                        }
                                        for (int i = 0; i < lis_e2.Count; i++)
                                        {
                                            s_e2 += lis_e2[i].ToString() + "/";
                                        }
                                        if (s_wl != "")
                                        {
                                            s_wl = s_wl.Remove(s_wl.Length - 1);
                                        }
                                        if (s_e1 != "")
                                        {
                                            s_e1 = s_e1.Remove(s_e1.Length - 1);
                                        }
                                        if (s_e2 != "")
                                        {
                                            s_e2 = s_e2.Remove(s_e2.Length - 1);
                                        }

                                        //// 计算XYZ值


                                        double[] doubles = XGD_Cal.ToArray();
                                        (double L, double A, double B) = CalculateLAB(doubles, Convert.ToInt32(dt_temp.Rows[0]["StartWave"].ToString()), Convert.ToInt32(dt_temp.Rows[0]["EndWave"].ToString()), Convert.ToInt32(dt_temp.Rows[0]["IntWave"].ToString()), 10);

                                        //// 转换为LAB值
                                        //(double L, double A, double B, double X, double Y, double Z) = ConvertToLAB(doubles);

                                        //Console.WriteLine($"L: {L}, A: {A}, B: {B}");

                                        //double L = 0.0; double A = 0.0; double B= 0.0;
                                        double d_compensate = 0.0;
                                        //判断当前检测是开料还是开稀
                                        DataTable dt_bottle_detail = FADM_Object.Communal._fadmSqlserver.GetData("SELECT * from bottle_details where BottleNum = '" + dt_temp.Rows[0]["BottleNum"].ToString() + "';");
                                        if (dt_bottle_detail.Rows.Count > 0)
                                        {
                                            if (dt_bottle_detail.Rows[0]["OriginalBottleNum"].ToString() == "0")
                                            {
                                                //d_compensate = 0.0;
                                                d_compensate = Convert.ToDouble(dt_bottle_detail.Rows[0]["RealConcentration"].ToString());
                                            }
                                            else
                                            {

                                                //获取标准曲线，然后计算补偿值
                                                s_sql = "SELECT *  FROM assistant_details WHERE AssistantCode = '" + dt_bottle_detail.Rows[0]["AssistantCode"].ToString() + "';";
                                                DataTable dt_assistant_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                                                double d_max_temp = -999;
                                                if (dt_assistant_details.Rows.Count > 0)
                                                {
                                                    string s_data1 = dt_assistant_details.Rows[0]["Abs"] is DBNull ? "" : dt_assistant_details.Rows[0]["Abs"].ToString();
                                                    if (s_data1 != "")
                                                    {
                                                        s_data1 = s_data1.Substring(0, s_data1.Length - 2);
                                                        string[] sa_arr1 = s_data1.Split('/');
                                                        for (int i = 0; i < sa_arr1.Count(); i++)
                                                        {

                                                            //查找波峰值
                                                            if (Convert.ToDouble(sa_arr1[i]) > d_max_temp)
                                                            {
                                                                d_max_temp = Convert.ToDouble(sa_arr1[i]);
                                                            }
                                                        }

                                                        //计算补偿系数
                                                        //d_compensate=(d_max_temp- d_max) / d_max_temp;

                                                        d_compensate = 1 - (d_max_temp - d_max) / d_max_temp;

                                                        d_compensate = Convert.ToDouble(dt_bottle_detail.Rows[0]["RealConcentration"].ToString()) * d_compensate;

                                                    }
                                                }
                                                else
                                                {
                                                    d_compensate = Convert.ToDouble(dt_bottle_detail.Rows[0]["RealConcentration"].ToString());
                                                }
                                            }
                                        }
                                        else
                                        {
                                            d_compensate = Convert.ToDouble(dt_bottle_detail.Rows[0]["RealConcentration"].ToString());
                                        }
                                        //当正常测量时，不再保存修正系数
                                        if (dt_temp.Rows[0]["Type"].ToString() == "11")
                                        {
                                            

                                            FADM_Object.Communal._fadmSqlserver.ReviseData("Update  bottle_details Set Abs = '" + s_xgd + "',L='" + L.ToString() + "',A='" + A.ToString() + "',B='" + B.ToString() + "',StandardE1='" + dt_standard.Rows[0]["E1"].ToString() + "',StandardE2='" + dt_standard.Rows[0]["E2"].ToString() + "',E1='" + s_e1 + "',E2='" + s_e2 + "',WL='" + dt_standard.Rows[0]["WL"].ToString() + "',RealConcentration='" + d_compensate.ToString("f6") + "' where BottleNum = '" + dt_temp.Rows[0]["BottleNum"].ToString() + "';");
                                            
                                        }
                                        else
                                        {
                                            
                                            FADM_Object.Communal._fadmSqlserver.ReviseData("Update  bottle_details Set Abs = '" + s_xgd + "',L='" + L.ToString() + "',A='" + A.ToString() + "',B='" + B.ToString() + "',StandardE1='" + dt_standard.Rows[0]["E1"].ToString() + "',StandardE2='" + dt_standard.Rows[0]["E2"].ToString() + "',E1='" + s_e1 + "',E2='" + s_e2 + "',WL='" + dt_standard.Rows[0]["WL"].ToString() + "' where BottleNum = '" + dt_temp.Rows[0]["BottleNum"].ToString() + "';");
                                        }

                                        //拷贝到历史表
                                        DataTable dt_Temp = FADM_Object.Communal._fadmSqlserver.GetData(
                                             "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'abs_cup_details';");
                                        string s_columnDetails = null;
                                        foreach (DataRow row in dt_Temp.Rows)
                                        {
                                            if (Convert.ToString(row[0]) != "Cooperate" && Convert.ToString(row[0]) != "NeedPulse" && Convert.ToString(row[0]) != "Choose"
                                                && Convert.ToString(row[0]) != "WaterFinish" && Convert.ToString(row[0]) != "TotalWeight")
                                                s_columnDetails += Convert.ToString(row[0]) + ", ";
                                        }
                                        s_columnDetails = s_columnDetails.Remove(s_columnDetails.Length - 2);

                                        s_sql = "SELECT *  FROM bottle_details WHERE BottleNum = '" + dt_temp.Rows[0]["BottleNum"].ToString() + "';";
                                        DataTable dt_bottle = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                                        //获取判断是否混合液
                                        if (Convert.ToInt32(dt_temp.Rows[0]["BottleNum"].ToString()) == 888 || Convert.ToInt32(dt_temp.Rows[0]["BottleNum"].ToString()) == 999)
                                        {
                                            s_sql = "SELECT *  FROM assistant_details WHERE AssistantCode = '" + dt_bottle.Rows[0]["AssistantCode"].ToString() + "';";
                                            DataTable dt_assistant_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                                            //获取配方代码，通过批次和滴液杯号获取
                                            s_sql = "SELECT *  FROM abs_history_head WHERE BatchName = '" + dt_assistant_details.Rows[0]["AssistantBarCode"].ToString() + "' And CupNum = "+ dt_assistant_details.Rows[0]["AssistantName"].ToString()+";";
                                            DataTable dt_his_drop = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                "INSERT INTO history_abs (" + s_columnDetails + ",A,B,L,FinishTime,Abs,BrewingData,RealConcentration,AssistantCode,BaseTestTime," +
                                                "StandardE1,StandardE2,E1,E2,WL,FormulaCode,BatchName,DripNum) (SELECT " + s_columnDetails + ",'" + A.ToString() + "' as A," + "'" 
                                                + B.ToString() + "' as B," + "'" + L.ToString() + "' as L," + "'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' as FinishTime," + "'" 
                                                + s_xgd.ToString() + "' as Abs,'" + dt_bottle.Rows[0]["BrewingData"].ToString() + "' as BrewingData," + dt_bottle.Rows[0]["RealConcentration"].ToString() 
                                                + " as RealConcentration,'" + dt_his_drop.Rows[0]["FormulaCode"].ToString() + "' as AssistantCode,'" + dt_standard.Rows[0]["FinishTime"].ToString() 
                                                + " ' as BaseTestTime,'" + dt_standard.Rows[0]["E1"].ToString() + "' as StandardE1,'" + dt_standard.Rows[0]["E2"].ToString() + "' as StandardE2,'" 
                                                + s_e1 + "' as E1,'" + s_e2 + "' as E2,'" + dt_standard.Rows[0]["WL"].ToString() + "' as WL,'"
                                                + dt_his_drop.Rows[0]["FormulaCode"].ToString() + "' as FormulaCode,'" + dt_his_drop.Rows[0]["BatchName"].ToString() + "' as BatchName,'" + dt_his_drop.Rows[0]["CupNum"].ToString() + "' as DripNum" + " FROM abs_cup_details " +
                                                "WHERE CupNum =" + (j + 1) + ") ;");

                                        }
                                        else
                                        {
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                "INSERT INTO history_abs (" + s_columnDetails + ",A,B,L,FinishTime,Abs,BrewingData,RealConcentration,AssistantCode,BaseTestTime,StandardE1,StandardE2,E1,E2,WL) (SELECT " + s_columnDetails + ",'" + A.ToString() + "' as A," + "'" + B.ToString() + "' as B," + "'" + L.ToString() + "' as L," + "'" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' as FinishTime," + "'" + s_xgd.ToString() + "' as Abs,'" + dt_bottle.Rows[0]["BrewingData"].ToString() + "' as BrewingData," + dt_bottle.Rows[0]["RealConcentration"].ToString() + " as RealConcentration,'" + dt_bottle.Rows[0]["AssistantCode"].ToString() + "' as AssistantCode,'" + dt_standard.Rows[0]["FinishTime"].ToString() + " ' as BaseTestTime,'" + dt_standard.Rows[0]["E1"].ToString() + "' as StandardE1,'" + dt_standard.Rows[0]["E2"].ToString() + "' as StandardE2,'" + s_e1 + "' as E1,'" + s_e2 + "' as E2,'" + dt_standard.Rows[0]["WL"].ToString() + "' as WL" + " FROM abs_cup_details " +
                                                "WHERE CupNum =" + (j + 1) + ") ;");
                                        }


                                    }
                                }
                            }
                        }

                        //判断是否接收到完成
                        if (_abs_Temps[j]._s_currentState == "3")
                        {
                            //记录第一个没完成，而且步骤一致的保存数据
                            int i_status = 0;
                            int[] ia_array1 = new int[40];
                            int i_state1 = FADM_Object.Communal._tcpModBusAbs.Read(13088, 40, ref ia_array1);
                            if (i_state1 == -1)
                            {
                                continue;
                            }

                            if (j  == 0)
                            {
                                i_status = ia_array1[4];
                            }
                            else if (j == 1)
                            {
                                i_status = ia_array1[14];
                            }
                            else if (j == 2)
                            {
                                i_status = ia_array1[24];
                            }
                            else if (j == 3)
                            {
                                i_status = ia_array1[34];
                            }
                            //查找数据库，第一个没完成的
                            string s_sql = "SELECT *  FROM Abs_details where CupNum = " + (j + 1) + " And Finish = 0 order by StepNum;";

                            DataTable dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                            if (dt_temp.Rows.Count > 0)
                            {


                                //更新完成时间，扣减液量
                                if (i_status == 6 && dt_temp.Rows[0]["TechnologyName"].ToString() == "测吸光度")
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                      "UPDATE abs_cup_details SET TotalWeight -= " + Convert.ToInt32(dt_temp.Rows[0]["PumpingTime"].ToString()) + " WHERE CupNum = " + (j + 1) + ";");

                                    //置为完成
                                    s_sql = "update Abs_details set FinishTime = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',Finish = 1 where CupNum = " + (j + 1) + " And StepNum = " + dt_temp.Rows[0]["StepNum"].ToString();
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                }
                                else if ((i_status == 4 && dt_temp.Rows[0]["TechnologyName"].ToString() == "搅拌")
                                    || (i_status == 3 && dt_temp.Rows[0]["TechnologyName"].ToString() == "抽染液")
                                    || (i_status == 2 && dt_temp.Rows[0]["TechnologyName"].ToString() == "加水")
                                    || (i_status == 1 && dt_temp.Rows[0]["TechnologyName"].ToString() == "加药")
                                    || (i_status == 8 && dt_temp.Rows[0]["TechnologyName"].ToString() == "加溶解剂"))
                                {
                                    //置为完成
                                    s_sql = "update Abs_details set FinishTime = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',Finish = 1 where CupNum = " + (j + 1) + " And StepNum = " + dt_temp.Rows[0]["StepNum"].ToString();
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                }
                                else if ((i_status == 5 && dt_temp.Rows[0]["TechnologyName"].ToString() == "排液"))
                                {
                                    int i_drainTime = 0;
                                    if (Convert.ToInt32(dt_temp.Rows[0]["DrainTime"].ToString()) == 0)
                                    {
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                      "UPDATE abs_cup_details SET TotalWeight = 0 WHERE CupNum = " + (j + 1) + ";");
                                    }
                                    else
                                    {
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                      "UPDATE abs_cup_details SET TotalWeight -= " + Convert.ToInt32(dt_temp.Rows[0]["DrainTime"].ToString()) + " WHERE CupNum = " + (j + 1) + ";");
                                    }

                                    //置为完成
                                    s_sql = "update Abs_details set FinishTime = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',Finish = 1 where CupNum = " + (j + 1) + " And StepNum = " + dt_temp.Rows[0]["StepNum"].ToString();
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                }
                            }

                            int[] values11 = new int[1];
                            values11[0] = 0;
                            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                            {
                                FADM_Object.Communal._tcpModBusAbs.ReConnect();
                            }
                            if (j  == 0)
                                FADM_Object.Communal._tcpModBusAbs.Write(13188, values11);
                            else if (j == 1)
                                FADM_Object.Communal._tcpModBusAbs.Write(13198, values11);
                            else if (j == 2)
                                FADM_Object.Communal._tcpModBusAbs.Write(13208, values11);
                            else if (j == 3)
                                FADM_Object.Communal._tcpModBusAbs.Write(13218, values11);





                            //没有下一步，就变为待机
                            if (SendData(j + 1) == 0)
                            {
                                //
                                //判断是否主动测白点前洗杯
                                if (MyAbsorbance._b_wash[Convert.ToInt32(j + 1) - 1])
                                {
                                    MyAbsorbance._b_wash[Convert.ToInt32(j + 1) - 1] = false;
                                    string s_sql_b;
                                    //DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("请选择测试基准点母液(选择溶解剂请点是，选择水请点否)", "温馨提示", MessageBoxButtons.YesNo, true);
                                    //if (dialogResult == DialogResult.Yes)
                                    //{
                                    //    //找到DNF溶解剂
                                    //    s_sql_b = "SELECT bottle_details.*  FROM bottle_details left join assistant_details on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE assistant_details.UnitOfAccount collate Chinese_PRC_CS_AS = 'G/L';";
                                    //}
                                    //else if (dialogResult == DialogResult.No)
                                    //{
                                    //    //找到水
                                    //    s_sql_b = "SELECT bottle_details.*  FROM bottle_details left join assistant_details on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE assistant_details.UnitOfAccount = 'Water';";
                                    //}
                                    //else
                                    //{
                                    //    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    //  "UPDATE abs_cup_details SET IsUsing = 0,Statues='待机' WHERE CupNum = " + (j + 1) + ";");
                                    //    continue;
                                    //}

                                    if (FADM_Object.Communal._b_isUseWaterTestBase)
                                    {
                                        //找到水
                                        s_sql_b = "SELECT bottle_details.*  FROM bottle_details left join assistant_details on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE assistant_details.UnitOfAccount = 'Water';";
                                    }
                                    else
                                    {
                                        //找到DNF溶解剂
                                        s_sql_b = "SELECT bottle_details.*  FROM bottle_details left join assistant_details on bottle_details.AssistantCode = assistant_details.AssistantCode " +
                                            "WHERE assistant_details.UnitOfAccount collate Chinese_PRC_CS_AS = 'G/L'  Order By bottle_details.BottleNum;";
                                    }

                                    DataTable dt_temp_b = FADM_Object.Communal._fadmSqlserver.GetData(s_sql_b);
                                    if (dt_temp_b.Rows.Count == 0)
                                    {
                                        FADM_Form.CustomMessageBox.Show("不存在母液瓶号，不能测试", "TestBaseAbs",
                                MessageBoxButtons.OK, false);
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                      "UPDATE abs_cup_details SET IsUsing = 0,Statues='待机' WHERE CupNum = " + (j + 1) + ";");
                                        continue;
                                    }
                                    else
                                    {
                                        SmartDyeing.FADM_Auto.MyAbsorbance.Calculate(0, Convert.ToInt32(dt_temp_b.Rows[0]["BottleNum"].ToString()), (j + 1), 4, FADM_Object.Communal._d_abs_total);

                                        SmartDyeing.FADM_Auto.MyAbsorbance.Generate(0, (j + 1));
                                        SmartDyeing.FADM_Auto.MyAbsorbance.SendData((j + 1));

                                       
                                        continue;
                                    }
                                }

                                //计算总用时
                                string s_sql_2_1 = "SELECT *  FROM Abs_details where CupNum = " + (j + 1) + "  order by StepNum;";

                                DataTable dt_temp_2_1 = FADM_Object.Communal._fadmSqlserver.GetData(s_sql_2_1);

                                foreach (DataRow dr in dt_temp_2_1.Rows)
                                {
                                    if(dr["TechnologyName"].ToString() == "测吸光度")
                                    {
                                        DateTime dateTime1 = Convert.ToDateTime(dt_temp_2_1.Rows[0]["StartTime"].ToString());
                                        DateTime dateTime2 = Convert.ToDateTime(Convert.ToDateTime(dt_temp_2_1.Rows[dt_temp_2_1.Rows.Count - 1]["FinishTime"].ToString()));
                                        TimeSpan ts = dateTime2 - dateTime1;
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                              "UPDATE history_abs SET TotalTime = "+ts.TotalSeconds+" WHERE CupNum = " + (j + 1) + " and  FinishTime = (select MAX(FinishTime) from history_abs where CupNum = "+ (j + 1) + ");");
                                        break;
                                    }
                                }

                                //判断一下当前杯是否测试纯水，如果是就进行测试样
                                string s_sql_1 = "SELECT * from abs_cup_details WHERE CupNum = " + (j + 1) + ";";
                                DataTable dt_temp_1 = FADM_Object.Communal._fadmSqlserver.GetData(s_sql_1);
                                if (dt_temp_1.Rows.Count > 0)
                                {
                                    //时间到了，测完白点要先润测试点
                                    if (dt_temp_1.Rows[0]["Type"].ToString() == "2" || dt_temp_1.Rows[0]["Type"].ToString() == "9")
                                    {
                                        DataTable dt_temp_first = FADM_Object.Communal._fadmSqlserver.GetData("SELECT * from standard order by Type;");
                                        if (dt_temp_first.Rows.Count == 2)
                                        {
                                            //比较两次白点差值
                                            //查询第一次测的数据
                                            //计算
                                            string[] sa_e1 = dt_temp_first.Rows[0]["E1"].ToString().Split('/');
                                            List<string> lis_sta_e1 = sa_e1.ToList();
                                            string[] sa_e2 = dt_temp_first.Rows[0]["E2"].ToString().Split('/');
                                            List<string> lis_sta_e2 = sa_e2.ToList();
                                            string[] sa_wl = dt_temp_first.Rows[0]["WL"].ToString().Split('/');
                                            List<string> lis_sta_wl = sa_wl.ToList();

                                            string[] sa_e12 = dt_temp_first.Rows[1]["E1"].ToString().Split('/');
                                            List<string> lis_sta_e12 = sa_e12.ToList();
                                            string[] sa_e22 = dt_temp_first.Rows[1]["E2"].ToString().Split('/');
                                            List<string> lis_sta_e22 = sa_e22.ToList();
                                            string[] sa_wl2 = dt_temp_first.Rows[1]["WL"].ToString().Split('/');
                                            List<string> lis_sta_wl2 = sa_wl2.ToList();

                                            bool b_flag = false;
                                            int i_sub = 0;
                                            string s_wl = "";
                                            for (int i = 0; i < lis_sta_wl.Count; i++)
                                            {
                                                if (Math.Abs(Convert.ToInt32(lis_sta_e22[i]) - Convert.ToInt32(lis_sta_e2[i])) > Math.Abs(i_sub))
                                                {
                                                    s_wl = lis_sta_wl[i];
                                                    i_sub = Convert.ToInt32(lis_sta_e22[i]) - Convert.ToInt32(lis_sta_e2[i]);
                                                }
                                            }
                                            for (int i = 0; i < lis_sta_wl.Count; i++)
                                            {
                                                if (Math.Abs(Convert.ToInt32(lis_sta_e22[i]) - Convert.ToInt32(lis_sta_e2[i])) > FADM_Object.Communal._d_abs_sub)
                                                {
                                                    b_flag = true;
                                                    break;
                                                }
                                            }
                                            if (b_flag)
                                            {
                                                FADM_Object.MyAlarm myAlarm;
                                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                                    myAlarm = new FADM_Object.MyAlarm("测试基准点误差过大，是否继续使用此数据作为基准点数据,最大差值在波长" + s_wl + "，差值为" + i_sub.ToString() + "(重新测量基本点请点是，使用此数据为基准点请点否)?", "吸光度", true, 1);
                                                else
                                                    myAlarm = new FADM_Object.MyAlarm("测试基准点误差过大，是否继续使用此数据作为基准点数据,最大差值在波长" + s_wl + "，差值为" + i_sub.ToString() + "(重新测量基本点请点是，使用此数据为基准点请点否)?", "吸光度", true, 1);

                                                while (true)
                                                {
                                                    if (0 != myAlarm._i_alarm_Choose)
                                                        break;
                                                    Thread.Sleep(1);
                                                }

                                                if (myAlarm._i_alarm_Choose == 1)
                                                {

                                                    //测试第二次白点
                                                    string s_sql1;
                                                    if (dt_temp_1.Rows[0]["Type"].ToString() == "2")
                                                        s_sql1 = "Update abs_cup_details set Statues='运行中',AdditivesDosage='" + string.Format("{0:F3}", FADM_Object.Communal._d_abs_total) + "',Pulse=0,Cooperate=5,Type=2,RealSampleDosage=0.0,RealAdditivesDosage=0.0 where CupNum = '" + (j + 1) + "';";
                                                    else
                                                        s_sql1 = "Update abs_cup_details set Statues='运行中',AdditivesDosage='" + string.Format("{0:F3}", FADM_Object.Communal._d_abs_total) + "',Pulse=0,Cooperate=5,Type=9,RealSampleDosage=0.0,RealAdditivesDosage=0.0 where CupNum = '" + (j + 1) + "';";
                                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql1);

                                                    SmartDyeing.FADM_Auto.MyAbsorbance.Generate(0, (j + 1));
                                                    SmartDyeing.FADM_Auto.MyAbsorbance.SendData((j + 1));

                                                    continue;
                                                }
                                                else
                                                {
                                                    //继续运行
                                                }
                                            }

                                            string s_sql_2 = "SELECT bottle_details.*,assistant_details.AllowMinColoringConcentration,assistant_details.AllowMaxColoringConcentration,assistant_details.AssistantType,assistant_details.UnitOfAccount  FROM bottle_details left join assistant_details on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE bottle_details.BottleNum = " + dt_temp_1.Rows[0]["BottleNum"].ToString() + ";";
                                            DataTable dt_temp_2 = FADM_Object.Communal._fadmSqlserver.GetData(s_sql_2);

                                            string s_unitOfAccount = dt_temp_2.Rows[0]["UnitOfAccount"].ToString();
                                            string s_assistantType = dt_temp_2.Rows[0]["AssistantType"].ToString();
                                            string s_realConcentration = dt_temp_2.Rows[0]["RealConcentration"].ToString();
                                            string s_settingConcentration = dt_temp_2.Rows[0]["SettingConcentration"].ToString();
                                            string s_compensate = dt_temp_2.Rows[0]["Compensate"].ToString() == "" ? "0" : dt_temp_2.Rows[0]["Compensate"].ToString();

                                            //活性用水稀释
                                            if (s_assistantType.Contains("活性"))
                                            {
                                                //找到母液水剂母液瓶号
                                                string s_sql_3 = "SELECT bottle_details.*  FROM bottle_details left join assistant_details on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE assistant_details.UnitOfAccount = 'Water';";

                                                DataTable dt_temp_3 = FADM_Object.Communal._fadmSqlserver.GetData(s_sql_3);
                                                if (dt_temp_3.Rows.Count == 0)
                                                {
                                                    FADM_Form.CustomMessageBox.Show("不存在水剂母液瓶号，不能测试", "TestAbs",
                                            MessageBoxButtons.OK, false);
                                                    return;
                                                }
                                                else
                                                {
                                                    if (dt_temp_1.Rows[0]["Type"].ToString() == "2")
                                                        SmartDyeing.FADM_Auto.MyAbsorbance.Calculate(Convert.ToInt32(dt_temp_1.Rows[0]["BottleNum"].ToString()), Convert.ToInt32(dt_temp_3.Rows[0]["BottleNum"].ToString()), (j + 1), 1, FADM_Object.Communal._d_abs_total);
                                                    else
                                                        SmartDyeing.FADM_Auto.MyAbsorbance.Calculate(Convert.ToInt32(dt_temp_1.Rows[0]["BottleNum"].ToString()), Convert.ToInt32(dt_temp_3.Rows[0]["BottleNum"].ToString()), (j + 1), 11, FADM_Object.Communal._d_abs_total);
                                                    //生成测量工艺
                                                    SmartDyeing.FADM_Auto.MyAbsorbance.Generate(1, (j + 1));
                                                    SmartDyeing.FADM_Auto.MyAbsorbance.SendData((j + 1));
                                                }
                                            }
                                            //其他使用溶解剂稀释
                                            else
                                            {
                                                string s_sql_3 = "SELECT bottle_details.*  FROM bottle_details left join assistant_details on bottle_details.AssistantCode = assistant_details.AssistantCode " +
                                                    "WHERE assistant_details.UnitOfAccount collate Chinese_PRC_CS_AS = 'G/L'  Order By bottle_details.BottleNum;";

                                                DataTable dt_temp_3 = FADM_Object.Communal._fadmSqlserver.GetData(s_sql_3);
                                                if (dt_temp_3.Rows.Count == 0)
                                                {
                                                    FADM_Form.CustomMessageBox.Show("不存在溶解剂母液瓶号，不能测试", "TestAbs",
                                            MessageBoxButtons.OK, false);
                                                    continue;
                                                }
                                                else
                                                {
                                                    if (dt_temp_1.Rows[0]["Type"].ToString() == "2")
                                                        SmartDyeing.FADM_Auto.MyAbsorbance.Calculate(Convert.ToInt32(dt_temp_1.Rows[0]["BottleNum"].ToString()), Convert.ToInt32(dt_temp_3.Rows[0]["BottleNum"].ToString()), (j + 1), 1, FADM_Object.Communal._d_abs_total);
                                                    else
                                                        SmartDyeing.FADM_Auto.MyAbsorbance.Calculate(Convert.ToInt32(dt_temp_1.Rows[0]["BottleNum"].ToString()), Convert.ToInt32(dt_temp_3.Rows[0]["BottleNum"].ToString()), (j + 1), 11, FADM_Object.Communal._d_abs_total);
                                                    //生成测量工艺
                                                    SmartDyeing.FADM_Auto.MyAbsorbance.Generate(1, (j + 1));
                                                    SmartDyeing.FADM_Auto.MyAbsorbance.SendData((j + 1));


                                                }
                                            }

                                        }
                                        else if (dt_temp_first.Rows.Count == 1)
                                        {
                                            //如果只有第一次白点记录，就直接测第二次白点
                                            if (dt_temp_first.Rows[0]["Type"].ToString() == "0")
                                            {
                                                string s_sql1;
                                                if (dt_temp_1.Rows[0]["Type"].ToString() == "2")
                                                {
                                                    //测试第二次白点
                                                    s_sql1 = "Update abs_cup_details set Statues='运行中',AdditivesDosage='" + string.Format("{0:F3}", FADM_Object.Communal._d_abs_total) + "',Pulse=0,Cooperate=5,Type=2,RealSampleDosage=0.0,RealAdditivesDosage=0.0 where CupNum = '" + (j + 1) + "';";
                                                }
                                                else
                                                {
                                                    //测试第二次白点
                                                    s_sql1 = "Update abs_cup_details set Statues='运行中',AdditivesDosage='" + string.Format("{0:F3}", FADM_Object.Communal._d_abs_total) + "',Pulse=0,Cooperate=5,Type=9,RealSampleDosage=0.0,RealAdditivesDosage=0.0 where CupNum = '" + (j + 1) + "';";
                                                }
                                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql1);
                                                
                                                SmartDyeing.FADM_Auto.MyAbsorbance.Generate(0, (j + 1));
                                                SmartDyeing.FADM_Auto.MyAbsorbance.SendData((j + 1));
                                            }
                                            else
                                            {
                                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                  "UPDATE abs_cup_details SET IsUsing = 0,Statues='待机' WHERE CupNum = " + (j + 1) + ";");
                                            }

                                        }
                                        else
                                            //保存数据
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                  "UPDATE abs_cup_details SET IsUsing = 0,Statues='待机' WHERE CupNum = " + (j + 1) + ";");

                                    }
                                    else if (dt_temp_1.Rows[0]["Type"].ToString() == "4")
                                    {
                                        DataTable dt_temp_first = FADM_Object.Communal._fadmSqlserver.GetData("SELECT * from standard order by Type;");
                                        if (dt_temp_first.Rows.Count == 2)
                                        {
                                            //比较两次白点差值
                                            //查询第一次测的数据
                                            //计算
                                            string[] sa_e1 = dt_temp_first.Rows[0]["E1"].ToString().Split('/');
                                            List<string> lis_sta_e1 = sa_e1.ToList();
                                            string[] sa_e2 = dt_temp_first.Rows[0]["E2"].ToString().Split('/');
                                            List<string> lis_sta_e2 = sa_e2.ToList();
                                            string[] sa_wl = dt_temp_first.Rows[0]["WL"].ToString().Split('/');
                                            List<string> lis_sta_wl = sa_wl.ToList();

                                            string[] sa_e12 = dt_temp_first.Rows[1]["E1"].ToString().Split('/');
                                            List<string> lis_sta_e12 = sa_e12.ToList();
                                            string[] sa_e22 = dt_temp_first.Rows[1]["E2"].ToString().Split('/');
                                            List<string> lis_sta_e22 = sa_e22.ToList();
                                            string[] sa_wl2 = dt_temp_first.Rows[1]["WL"].ToString().Split('/');
                                            List<string> lis_sta_wl2 = sa_wl2.ToList();

                                            bool b_flag = false;
                                            int i_sub = 0;
                                            string s_wl = "";
                                            for (int i = 0; i < lis_sta_wl.Count; i++)
                                            {
                                                if (Math.Abs(Convert.ToInt32(lis_sta_e22[i]) - Convert.ToInt32(lis_sta_e2[i])) > Math.Abs(i_sub))
                                                {
                                                    s_wl = lis_sta_wl[i];
                                                    i_sub = Convert.ToInt32(lis_sta_e22[i]) - Convert.ToInt32(lis_sta_e2[i]);
                                                }
                                            }

                                            for (int i = 0; i < lis_sta_wl.Count; i++)
                                            {
                                                if (Math.Abs(Convert.ToInt32(lis_sta_e22[i]) - Convert.ToInt32(lis_sta_e2[i])) > FADM_Object.Communal._d_abs_sub)
                                                {
                                                    b_flag = true;
                                                    break;
                                                }
                                            }
                                            if (b_flag)
                                            {
                                                FADM_Object.MyAlarm myAlarm;
                                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                                    myAlarm = new FADM_Object.MyAlarm("测试基准点误差过大，是否继续使用此数据作为基准点数据,最大差值在波长" + s_wl + "，差值为" + i_sub.ToString() + "(重新测量基本点请点是，使用此数据为基准点请点否)?", "吸光度", true, 1);
                                                else
                                                    myAlarm = new FADM_Object.MyAlarm("测试基准点误差过大，是否继续使用此数据作为基准点数据,最大差值在波长" + s_wl + "，差值为" + i_sub.ToString() + "(重新测量基本点请点是，使用此数据为基准点请点否)?", "吸光度", true, 1);

                                                while (true)
                                                {
                                                    if (0 != myAlarm._i_alarm_Choose)
                                                        break;
                                                    Thread.Sleep(1);
                                                }

                                                if (myAlarm._i_alarm_Choose == 1)
                                                {
                                                    //测试第二次白点
                                                    string s_sql1 = "Update abs_cup_details set Statues='运行中',AdditivesDosage='" + string.Format("{0:F3}", FADM_Object.Communal._d_abs_total) + "',Pulse=0,Cooperate=5,Type=4,RealSampleDosage=0.0,RealAdditivesDosage=0.0 where CupNum = '" + (j + 1) + "';";
                                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql1);

                                                    SmartDyeing.FADM_Auto.MyAbsorbance.Generate(0, (j + 1));
                                                    SmartDyeing.FADM_Auto.MyAbsorbance.SendData((j + 1));

                                                    continue;
                                                }
                                                else
                                                {
                                                    //继续运行
                                                }
                                            }

                                            //保存数据
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                  "UPDATE abs_cup_details SET IsUsing = 0,Statues='待机' WHERE CupNum = " + (j + 1) + ";");

                                        }

                                        else if (dt_temp_first.Rows.Count == 1)
                                        {
                                            //如果只有第一次白点记录，就直接测第二次白点
                                            if (dt_temp_first.Rows[0]["Type"].ToString() == "0")
                                            {
                                                //测试第二次白点
                                                string s_sql1 = "Update abs_cup_details set Statues='运行中',AdditivesDosage='" + string.Format("{0:F3}", FADM_Object.Communal._d_abs_total) + "',Pulse=0,Cooperate=5,Type=4,RealSampleDosage=0.0,RealAdditivesDosage=0.0 where CupNum = '" + (j + 1) + "';";
                                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql1);

                                                SmartDyeing.FADM_Auto.MyAbsorbance.Generate(0, (j + 1));
                                                SmartDyeing.FADM_Auto.MyAbsorbance.SendData((j + 1));
                                            }
                                            else
                                            {
                                                //保存数据
                                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                      "UPDATE abs_cup_details SET IsUsing = 0,Statues='待机' WHERE CupNum = " + (j + 1) + ";");
                                            }

                                        }
                                        else
                                            //保存数据
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                                  "UPDATE abs_cup_details SET IsUsing = 0,Statues='待机' WHERE CupNum = " + (j + 1) + ";");
                                    }
                                    else
                                        //保存数据
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                              "UPDATE abs_cup_details SET IsUsing = 0,Statues='待机' WHERE CupNum = " + (j + 1) + ";");
                                }
                                else
                                    //保存数据
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                          "UPDATE abs_cup_details SET IsUsing = 0,Statues='待机' WHERE CupNum = " + (j + 1) + ";");
                                continue;
                            }
                        }

                        
                    }
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                FADM_Object.Communal._b_absErr = true;
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    new SmartDyeing.FADM_Object.MyAlarm(ex.Message, "吸光度机通讯" , false, 1);
                else
                    new SmartDyeing.FADM_Object.MyAlarm(ex.Message, "Absorbance machine communication", false, 1);

               
            }
        }

        /// <summary>
        /// 发送每一步工序
        /// </summary>
        public static int SendData(int i_cupNum)
        {
            //查找下一步
            string s_sql = "SELECT *  FROM Abs_details where CupNum = "+ i_cupNum+ " And Finish = 0 order by StepNum;";

            DataTable dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

            if (dt_temp.Rows.Count == 0)
                return 0;
            else
            {
                double d_dis = 1;
                //记录开始时间
                s_sql = "update Abs_details set StartTime = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' where CupNum = " + i_cupNum + " And StepNum = " + dt_temp.Rows[0]["StepNum"].ToString();
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                              "UPDATE abs_cup_details SET Statues='"+ dt_temp.Rows[0]["TechnologyName"].ToString()+"'  WHERE CupNum = " + i_cupNum + ";");

                if (dt_temp.Rows[0]["TechnologyName"].ToString() == "加药")
                {
                    //重新计算加药量

                    //查询剩余液量
                    s_sql = "SELECT * from abs_cup_details where CupNum = " + i_cupNum + " ;";

                    DataTable dt_abs = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                    //判断是2,4号杯测量时加药才调用，1,3号配液时不计算
                    if (i_cupNum == 2 || i_cupNum == 4)
                        SmartDyeing.FADM_Auto.MyAbsorbance.Calculate(Convert.ToInt32(dt_abs.Rows[0]["BottleNum"].ToString()), Convert.ToInt32(dt_abs.Rows[0]["AdditivesNum"].ToString()), i_cupNum, Convert.ToInt32(dt_abs.Rows[0]["Type"].ToString()), Convert.ToDouble(dt_temp.Rows[0]["Dosage"].ToString()));

                    //发送启动
                    int[] values1 = new int[5];
                    values1[0] = 1;
                    values1[1] = 0;
                    values1[2] = 0;
                    values1[3] = 0;
                    values1[4] = 1;
                    if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                    {
                        FADM_Object.Communal._tcpModBusAbs.ReConnect();
                    }

                    //写入参数数据
                    int d_1_ = 0;
                    d_1_ = Convert.ToInt32(dt_temp.Rows[0]["StirringRate"].ToString()) / 65536;
                    int i_d_11_ = Convert.ToInt32(dt_temp.Rows[0]["StirringRate"].ToString()) % 65536;

                    int d_2_ = 0;
                    d_2_ = Lib_Card.Configure.Parameter.Other_StartWave / 65536;
                    int i_d_22_ = Lib_Card.Configure.Parameter.Other_StartWave % 65536;
                    

                    int d_3_ = 0;
                    d_3_ = Lib_Card.Configure.Parameter.Other_EndWave / 65536;
                    int i_d_33_ = Lib_Card.Configure.Parameter.Other_EndWave % 65536;
                   

                    int d_4_ = 0;
                    d_4_ = Lib_Card.Configure.Parameter.Other_IntWave / 65536;
                    int i_d_44_ = Lib_Card.Configure.Parameter.Other_IntWave % 65536;
                    

                    int[] ia_array1 = new int[] { i_d_11_, d_1_ };
                    if (i_cupNum == 1)
                        FADM_Object.Communal._tcpModBusAbs.Write(15296, ia_array1);
                    else if (i_cupNum == 2)
                        FADM_Object.Communal._tcpModBusAbs.Write(15346, ia_array1);
                    else if (i_cupNum == 3)
                        FADM_Object.Communal._tcpModBusAbs.Write(15396, ia_array1);
                    else if (i_cupNum == 4)
                        FADM_Object.Communal._tcpModBusAbs.Write(15446, ia_array1);

                    int[] ia_array2 = new int[] { i_d_22_, d_2_, i_d_33_, d_3_, i_d_44_, d_4_ };
                    if (i_cupNum == 1)
                        FADM_Object.Communal._tcpModBusAbs.Write(15288, ia_array2);
                    else if (i_cupNum == 2)
                        FADM_Object.Communal._tcpModBusAbs.Write(15338, ia_array2);
                    else if (i_cupNum == 3)
                        FADM_Object.Communal._tcpModBusAbs.Write(15388, ia_array2);
                    else if (i_cupNum == 4)
                        FADM_Object.Communal._tcpModBusAbs.Write(15438, ia_array2);

                    if (i_cupNum == 1)
                        FADM_Object.Communal._tcpModBusAbs.Write(13088, values1);
                    else if (i_cupNum == 2)
                        FADM_Object.Communal._tcpModBusAbs.Write(13098, values1);
                    else if (i_cupNum == 3)
                        FADM_Object.Communal._tcpModBusAbs.Write(13108, values1);
                    else if (i_cupNum == 4)
                        FADM_Object.Communal._tcpModBusAbs.Write(13118, values1);
                }
                else if (dt_temp.Rows[0]["TechnologyName"].ToString() == "加溶解剂")
                {
                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                              "UPDATE abs_cup_details SET AdditivesDosage='" + Convert.ToDouble(dt_temp.Rows[0]["Dosage"].ToString()) + "'  WHERE CupNum = " + i_cupNum + ";");
                    //发送启动
                    int[] values1 = new int[5];
                    values1[0] = 1;
                    values1[1] = 0;
                    values1[2] = 0;
                    values1[3] = 0;
                    values1[4] = 8;
                    if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                    {
                        FADM_Object.Communal._tcpModBusAbs.ReConnect();
                    }

                    //写入参数数据
                    int d_1_ = 0;
                    d_1_ = Convert.ToInt32(dt_temp.Rows[0]["StirringRate"].ToString()) / 65536;
                    int i_d_11_ = Convert.ToInt32(dt_temp.Rows[0]["StirringRate"].ToString()) % 65536;

                    int d_2_ = 0;
                    d_2_ = Lib_Card.Configure.Parameter.Other_StartWave / 65536;
                    int i_d_22_ = Lib_Card.Configure.Parameter.Other_StartWave % 65536;


                    int d_3_ = 0;
                    d_3_ = Lib_Card.Configure.Parameter.Other_EndWave / 65536;
                    int i_d_33_ = Lib_Card.Configure.Parameter.Other_EndWave % 65536;


                    int d_4_ = 0;
                    d_4_ = Lib_Card.Configure.Parameter.Other_IntWave / 65536;
                    int i_d_44_ = Lib_Card.Configure.Parameter.Other_IntWave % 65536;


                    int[] ia_array1 = new int[] { i_d_11_, d_1_ };
                    if (i_cupNum == 1)
                        FADM_Object.Communal._tcpModBusAbs.Write(15296, ia_array1);
                    else if (i_cupNum == 2)
                        FADM_Object.Communal._tcpModBusAbs.Write(15346, ia_array1);
                    else if (i_cupNum == 3)
                        FADM_Object.Communal._tcpModBusAbs.Write(15396, ia_array1);
                    else if (i_cupNum == 4)
                        FADM_Object.Communal._tcpModBusAbs.Write(15446, ia_array1);

                    int[] ia_array2 = new int[] { i_d_22_, d_2_, i_d_33_, d_3_, i_d_44_, d_4_ };
                    if (i_cupNum == 1)
                        FADM_Object.Communal._tcpModBusAbs.Write(15288, ia_array2);
                    else if (i_cupNum == 2)
                        FADM_Object.Communal._tcpModBusAbs.Write(15338, ia_array2);
                    else if (i_cupNum == 3)
                        FADM_Object.Communal._tcpModBusAbs.Write(15388, ia_array2);
                    else if (i_cupNum == 4)
                        FADM_Object.Communal._tcpModBusAbs.Write(15438, ia_array2);

                    if (i_cupNum == 1)
                        FADM_Object.Communal._tcpModBusAbs.Write(13088, values1);
                    else if (i_cupNum == 2)
                        FADM_Object.Communal._tcpModBusAbs.Write(13098, values1);
                    else if (i_cupNum == 3)
                        FADM_Object.Communal._tcpModBusAbs.Write(13108, values1);
                    else if (i_cupNum == 4)
                        FADM_Object.Communal._tcpModBusAbs.Write(13118, values1);
                }
                else if (dt_temp.Rows[0]["TechnologyName"].ToString() == "加水")
                {
                    //发送启动
                    int[] values1 = new int[5];
                    values1[0] = 1;
                    values1[1] = 0;
                    values1[2] = 0;
                    values1[3] = 0;
                    values1[4] = 2;
                    if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                    {
                        FADM_Object.Communal._tcpModBusAbs.ReConnect();
                    }

                    //写入参数数据
                    int d_1_ = 0;
                    d_1_ = Convert.ToInt32(dt_temp.Rows[0]["StirringRate"].ToString()) / 65536;
                    int i_d_11_ = Convert.ToInt32(dt_temp.Rows[0]["StirringRate"].ToString()) % 65536;

                    int[] ia_array1 = new int[] { i_d_11_, d_1_ };
                    if (i_cupNum == 1)
                        FADM_Object.Communal._tcpModBusAbs.Write(15296, ia_array1);
                    else if (i_cupNum == 2)
                        FADM_Object.Communal._tcpModBusAbs.Write(15346, ia_array1);
                    else if (i_cupNum == 3)
                        FADM_Object.Communal._tcpModBusAbs.Write(15396, ia_array1);
                    else if (i_cupNum == 4)
                        FADM_Object.Communal._tcpModBusAbs.Write(15446, ia_array1);

                    if (i_cupNum == 1)
                        FADM_Object.Communal._tcpModBusAbs.Write(13088, values1);
                    else if (i_cupNum == 2)
                        FADM_Object.Communal._tcpModBusAbs.Write(13098, values1);
                    else if (i_cupNum == 3)
                        FADM_Object.Communal._tcpModBusAbs.Write(13108, values1);
                    else if (i_cupNum == 4)
                        FADM_Object.Communal._tcpModBusAbs.Write(13118, values1);
                }
                else if (dt_temp.Rows[0]["TechnologyName"].ToString() == "抽染液")
                {
                    //发送启动
                    int[] values1 = new int[5];
                    values1[0] = 1;
                    values1[1] = 0;
                    values1[2] = 0;
                    values1[3] = 0;
                    values1[4] = 3;
                    if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                    {
                        FADM_Object.Communal._tcpModBusAbs.ReConnect();
                    }

                    if (i_cupNum == 1)
                        FADM_Object.Communal._tcpModBusAbs.Write(13088, values1);
                    else if (i_cupNum == 2)
                        FADM_Object.Communal._tcpModBusAbs.Write(13098, values1);
                    else if (i_cupNum == 3)
                        FADM_Object.Communal._tcpModBusAbs.Write(13108, values1);
                    else if (i_cupNum == 4)
                        FADM_Object.Communal._tcpModBusAbs.Write(13118, values1);
                }
                else if (dt_temp.Rows[0]["TechnologyName"].ToString() == "搅拌")
                {
                    //发送启动
                    int[] values1 = new int[5];
                    values1[0] = 1;
                    values1[1] = 0;
                    values1[2] = 0;
                    values1[3] = 0;
                    values1[4] = 4;
                    if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                    {
                        FADM_Object.Communal._tcpModBusAbs.ReConnect();
                    }

                    //写入参数数据
                    int d_1_ = 0;
                    d_1_ = Convert.ToInt32(dt_temp.Rows[0]["StirringRate"].ToString()) / 65536;
                    int i_d_11_ = Convert.ToInt32(dt_temp.Rows[0]["StirringRate"].ToString()) % 65536;

                    int d_2_ = 0;
                    d_2_ = Convert.ToInt32(dt_temp.Rows[0]["StirringTime"].ToString()) / 65536;
                    int i_d_22_ = Convert.ToInt32(dt_temp.Rows[0]["StirringTime"].ToString()) % 65536;

                    int[] ia_array1 = new int[] { i_d_11_, d_1_ , i_d_22_, d_2_ };
                    if (i_cupNum == 1)
                        FADM_Object.Communal._tcpModBusAbs.Write(15296, ia_array1);
                    else if (i_cupNum == 2)
                        FADM_Object.Communal._tcpModBusAbs.Write(15346, ia_array1);
                    else if (i_cupNum == 3)
                        FADM_Object.Communal._tcpModBusAbs.Write(15396, ia_array1);
                    else if (i_cupNum == 4)
                        FADM_Object.Communal._tcpModBusAbs.Write(15446, ia_array1);

                    if (i_cupNum == 1)
                        FADM_Object.Communal._tcpModBusAbs.Write(13088, values1);
                    else if (i_cupNum == 2)
                        FADM_Object.Communal._tcpModBusAbs.Write(13098, values1);
                    else if (i_cupNum == 3)
                        FADM_Object.Communal._tcpModBusAbs.Write(13108, values1);
                    else if (i_cupNum == 4)
                        FADM_Object.Communal._tcpModBusAbs.Write(13118, values1);
                }
                else if (dt_temp.Rows[0]["TechnologyName"].ToString() == "排液")
                {
                    //发送启动
                    int[] values1 = new int[5];
                    values1[0] = 1;
                    values1[1] = 0;
                    values1[2] = 0;
                    values1[3] = 0;
                    values1[4] = 5;
                    if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                    {
                        FADM_Object.Communal._tcpModBusAbs.ReConnect();
                    }

                    int d_0_ = 0;
                    d_0_ = Convert.ToInt32(Convert.ToDouble(dt_temp.Rows[0]["PumpingTime"].ToString()) / d_dis) / 65536;
                    int i_d_00_ = Convert.ToInt32(Convert.ToDouble(dt_temp.Rows[0]["PumpingTime"].ToString()) / d_dis) % 65536;

                    //写入参数数据
                    int d_1_ = 0;
                    d_1_ = Convert.ToInt32(dt_temp.Rows[0]["StirringRate"].ToString()) / 65536;
                    int i_d_11_ = Convert.ToInt32(dt_temp.Rows[0]["StirringRate"].ToString()) % 65536;

                    int d_2_ = 0;
                    int i_d_22_ = 0;

                    //当排液时间为0时，用总液量来算排液时间，这个由于测吸光度后洗杯需要使用计算液量
                    int i_drainTime = 0;
                    if(Convert.ToInt32(dt_temp.Rows[0]["DrainTime"].ToString()) == 0)
                    {
                        //查询剩余液量
                        s_sql = "SELECT * from abs_cup_details where CupNum = " + i_cupNum + " ;";

                        DataTable dt_abs = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                        double d_tatal = Convert.ToDouble(dt_abs.Rows[0]["TotalWeight"].ToString());
                        if (d_tatal < 10)
                            d_tatal = 10;
                        i_drainTime = (int)(d_tatal);
                    }
                    else
                    {
                        i_drainTime = Convert.ToInt32(dt_temp.Rows[0]["DrainTime"].ToString());
                    }
                    i_drainTime = (int)(i_drainTime / d_dis);
                    int d_3_ = 0;
                    d_3_ = i_drainTime / 65536;
                    int i_d_33_ = i_drainTime % 65536;

                    int d_4_ = 0;
                    d_4_ = Convert.ToInt32(Convert.ToDouble(dt_temp.Rows[0]["ParallelizingDishTime"].ToString())/d_dis) / 65536;
                    int i_d_44_ = Convert.ToInt32(Convert.ToDouble(dt_temp.Rows[0]["ParallelizingDishTime"].ToString()) / d_dis) % 65536;

                    int[] ia_array1 = new int[] { i_d_00_, d_0_, i_d_11_, d_1_, i_d_22_, d_2_, i_d_33_, d_3_, i_d_44_, d_4_ };
                    if (i_cupNum == 1)
                        FADM_Object.Communal._tcpModBusAbs.Write(15294, ia_array1);
                    else if (i_cupNum == 2)
                        FADM_Object.Communal._tcpModBusAbs.Write(15344, ia_array1);
                    else if (i_cupNum == 3)
                        FADM_Object.Communal._tcpModBusAbs.Write(15394, ia_array1);
                    else if (i_cupNum == 4)
                        FADM_Object.Communal._tcpModBusAbs.Write(15444, ia_array1);

                    if (i_cupNum == 1)
                        FADM_Object.Communal._tcpModBusAbs.Write(13088, values1);
                    else if (i_cupNum == 2)
                        FADM_Object.Communal._tcpModBusAbs.Write(13098, values1);
                    else if (i_cupNum == 3)
                        FADM_Object.Communal._tcpModBusAbs.Write(13108, values1);
                    else if (i_cupNum == 4)
                        FADM_Object.Communal._tcpModBusAbs.Write(13118, values1);
                }
                else if (dt_temp.Rows[0]["TechnologyName"].ToString() == "测吸光度")
                {
                    //更新测试波长和间隔
                    s_sql = "Update abs_cup_details set StartWave='" + dt_temp.Rows[0]["StartingWavelength"].ToString() + "',EndWave='" + dt_temp.Rows[0]["EndWavelength"].ToString() + "',IntWave='" + dt_temp.Rows[0]["WavelengthInterval"].ToString()  + "' where CupNum = '" + i_cupNum + "';";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                    //发送启动
                    int[] values1 = new int[5];
                    values1[0] = 1;
                    values1[1] = 0;
                    values1[2] = 0;
                    values1[3] = 0;
                    values1[4] = 6;
                    if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                    {
                        FADM_Object.Communal._tcpModBusAbs.ReConnect();
                    }

                    //写入参数数据
                    int d_1_ = 0;
                    d_1_ = Lib_Card.Configure.Parameter.Other_StartWave / 65536;
                    int i_d_11_ = Lib_Card.Configure.Parameter.Other_StartWave % 65536;

                    int d_2_ = 0;
                    d_2_ = Lib_Card.Configure.Parameter.Other_EndWave / 65536;
                    int i_d_22_ = Lib_Card.Configure.Parameter.Other_EndWave % 65536;

                    int d_3_ = 0;
                    d_3_ = Lib_Card.Configure.Parameter.Other_IntWave / 65536;
                    int i_d_33_ = Lib_Card.Configure.Parameter.Other_IntWave % 65536;

                    int d_4_ = 0;
                    d_4_ = Convert.ToInt32(Convert.ToDouble(dt_temp.Rows[0]["PumpingTime"].ToString()) / d_dis) / 65536;
                    int i_d_44_ = Convert.ToInt32(Convert.ToDouble(dt_temp.Rows[0]["PumpingTime"].ToString()) / d_dis) % 65536;

                    int[] ia_array1 = new int[] { i_d_11_, d_1_, i_d_22_, d_2_, i_d_33_, d_3_, i_d_44_, d_4_ };
                    if (i_cupNum == 1)
                        FADM_Object.Communal._tcpModBusAbs.Write(15288, ia_array1);
                    else if (i_cupNum == 2)
                        FADM_Object.Communal._tcpModBusAbs.Write(15338, ia_array1);
                    else if (i_cupNum == 3)
                        FADM_Object.Communal._tcpModBusAbs.Write(15388, ia_array1);
                    else if (i_cupNum == 4)
                        FADM_Object.Communal._tcpModBusAbs.Write(15438, ia_array1);

                    if (i_cupNum == 1)
                        FADM_Object.Communal._tcpModBusAbs.Write(13088, values1);
                    else if (i_cupNum == 2)
                        FADM_Object.Communal._tcpModBusAbs.Write(13098, values1);
                    else if (i_cupNum == 3)
                        FADM_Object.Communal._tcpModBusAbs.Write(13108, values1);
                    else if (i_cupNum == 4)
                        FADM_Object.Communal._tcpModBusAbs.Write(13118, values1);
                }

                return 1;
            }
        }
        /// <summary>
        /// 生成工艺保存到Abs_details表
        /// </summary>
        /// /// <param name="i_type">类型：0 测白点  1 母液测量 2 停止</param>
        /// /// <param name="i_cupNum">杯号</param>
        /// 
        public static void Generate(int i_type,int i_cupNum)
        {
            //生成测白点工艺
            int i_s = 0; int i_e = 0; int i_int = 0;
            string s_sql;
            if (i_type == 0)
                s_sql = "select * from Abs_process where Code = '测白点' order by StepNum";
            else if (i_type == 1)
            {
                s_sql = "SELECT * from abs_cup_details WHERE CupNum = " + i_cupNum + ";";

                DataTable dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                
                //获取检测瓶号
                if (dt_temp.Rows.Count > 0)
                {
                    DataTable dt_temp_bottle = FADM_Object.Communal._fadmSqlserver.GetData("select * from bottle_details where BottleNum = '" + dt_temp.Rows[0]["BottleNum"].ToString() + "'");
                    if(dt_temp_bottle.Rows.Count > 0)
                    {
                        if (dt_temp_bottle.Rows[0]["AbsCode"] is DBNull)
                        {
                            s_sql = "select * from Abs_process where Code = '测试' order by StepNum";
                        }
                        else
                        {
                            s_sql = "select * from Abs_process where Code = '" + dt_temp_bottle.Rows[0]["AbsCode"].ToString() + "' order by StepNum";
                        }

                        //DataTable dt_temp_Ass = FADM_Object.Communal._fadmSqlserver.GetData("select * from assistant_details where AssistantCode = '" + dt_temp_bottle.Rows[0]["AssistantCode"].ToString() + "'");
                        //if(dt_temp_Ass.Rows.Count >0)
                        {
                            i_s = Lib_Card.Configure.Parameter.Other_StartWave;
                            i_e = Lib_Card.Configure.Parameter.Other_EndWave;
                            i_int = Lib_Card.Configure.Parameter.Other_IntWave;
                        }
                    }
                    else
                    {
                        s_sql = "select * from Abs_process where Code = '测试' order by StepNum";
                    }
                }
                else
                {
                    s_sql = "select * from Abs_process where Code = '测试' order by StepNum";
                }
            }
            else if (i_type == 2)
                s_sql = "select * from Abs_process where Code = '停止' order by StepNum";
            else if (i_type == 4)
                s_sql = "select * from Abs_process where Code = '溶解剂洗杯' order by StepNum";
            else
                s_sql = "select * from Abs_process where Code = '抽混合液' order by StepNum";
            DataTable dataTable = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

            //删除详细数据里数据
            FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from Abs_details where CupNum = " + i_cupNum + "");
            string s_guid = Guid.NewGuid().ToString("N");
            foreach (DataRow dr in dataTable.Rows)
            {
                if (dr["TechnologyName"].ToString() == "加药"|| dr["TechnologyName"].ToString() == "加溶解剂")
                {
                    s_sql = "Insert into Abs_details(CupNum,Finish,Cooperate,GUID,StepNum,TechnologyName,Code,StirringRate,Dosage,StartingWavelength,EndWavelength,WavelengthInterval) " +
                        "Values(" + i_cupNum+",0,0,'"+ s_guid + "'," + dr["StepNum"].ToString()+",'"+ dr["TechnologyName"].ToString() + "','" + dr["Code"].ToString() + "',"
                        + dr["StirringRate"].ToString()+ ","+ dr["Dosage"].ToString() + "," + (i_s == 0?dr["StartingWavelength"].ToString(): i_s.ToString()) + "," 
                        + (i_e == 0 ? dr["EndWavelength"].ToString() : i_e.ToString()) + "," + (i_int == 0 ? dr["WavelengthInterval"].ToString() : i_int.ToString()) + ")";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                }
                else if (dr["TechnologyName"].ToString() == "加水")
                {
                    s_sql = "Insert into Abs_details(CupNum,Finish,Cooperate,GUID,StepNum,TechnologyName,Code,StirringRate) Values(" + i_cupNum + ",0,0,'" + s_guid + "'," + dr["StepNum"].ToString() + ",'" + dr["TechnologyName"].ToString() + "','" + dr["Code"].ToString() + "'," + dr["StirringRate"].ToString()  + ")";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                }
                else if (dr["TechnologyName"].ToString() == "抽染液")
                {
                    s_sql = "Insert into Abs_details(CupNum,Finish,Cooperate,GUID,StepNum,TechnologyName,Code) Values(" + i_cupNum + ",0,0,'" + s_guid + "'," + dr["StepNum"].ToString() + ",'" + dr["TechnologyName"].ToString() + "','" + dr["Code"].ToString() + "')";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                }
                else if (dr["TechnologyName"].ToString() == "搅拌")
                {
                    s_sql = "Insert into Abs_details(CupNum,Finish,Cooperate,GUID,StepNum,TechnologyName,Code,StirringRate,StirringTime) Values(" + i_cupNum + ",0,0,'" + s_guid + "'," + dr["StepNum"].ToString() + ",'" + dr["TechnologyName"].ToString() + "','" + dr["Code"].ToString() + "'," + dr["StirringRate"].ToString() + "," + dr["StirringTime"].ToString() + ")";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                }
                else if (dr["TechnologyName"].ToString() == "排液")
                {
                    s_sql = "Insert into Abs_details(CupNum,Finish,Cooperate,GUID,StepNum,TechnologyName,Code,StirringRate,DrainTime,ParallelizingDishTime,PumpingTime) Values(" + i_cupNum + ",0,0,'" + s_guid + "'," + dr["StepNum"].ToString() + ",'" + dr["TechnologyName"].ToString() + "','" + dr["Code"].ToString() + "'," + dr["StirringRate"].ToString() + "," + dr["DrainTime"].ToString() + "," + dr["ParallelizingDishTime"].ToString() + "," + dr["PumpingTime"].ToString() + ")";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                }
                else if (dr["TechnologyName"].ToString() == "测吸光度")
                {
                    s_sql = "Insert into Abs_details(CupNum,Finish,Cooperate,GUID,StepNum,TechnologyName,Code,PumpingTime,StartingWavelength,EndWavelength,WavelengthInterval) " +
                        "Values(" + i_cupNum + ",0,0,'" + s_guid + "'," + dr["StepNum"].ToString() + ",'" + dr["TechnologyName"].ToString() + "','" + dr["Code"].ToString() + "'," 
                        + dr["PumpingTime"].ToString() + "," + (i_s == 0 ? dr["StartingWavelength"].ToString() : i_s.ToString()) + "," + (i_e == 0 ? dr["EndWavelength"].ToString() : i_e.ToString()) 
                        + "," + (i_int == 0 ? dr["WavelengthInterval"].ToString() : i_int.ToString()) + ")";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                }

            }
        }

        /// <summary>
        /// 计算加药量
        /// </summary>
        /// 
        public static void Calculate(int i_bottleNum,int i_additivesNum ,int i_cupNum,int i_type,double d_total)
        {
            //2.计算需要添加的用量并保存到表_i_nBottleNum
            string s_sql = "SELECT bottle_details.*,assistant_details.AllowMinColoringConcentration,assistant_details.AllowMaxColoringConcentration,assistant_details.AssistantType,assistant_details.UnitOfAccount  FROM bottle_details left join assistant_details on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE bottle_details.BottleNum = " + i_bottleNum + ";";
            DataTable dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

            string s_unitOfAccount = dt_temp.Rows[0]["UnitOfAccount"].ToString();
            string s_assistantType = dt_temp.Rows[0]["AssistantType"].ToString();
            string s_realConcentration = dt_temp.Rows[0]["RealConcentration"].ToString();
            string s_settingConcentration = dt_temp.Rows[0]["SettingConcentration"].ToString();
            string s_compensate = dt_temp.Rows[0]["Compensate"].ToString() == "" ? "0" : dt_temp.Rows[0]["Compensate"].ToString();

            //
            double d_stotal = d_total * (FADM_Object.Communal._d_ppm / 10000);
            //母液重量
            double d_dosage = d_stotal / Convert.ToDouble(s_realConcentration);
            double d_water = d_total - d_dosage;
            double d_t = d_dosage * Convert.ToDouble(s_compensate);
            d_dosage = d_dosage * (1 + Convert.ToDouble(s_compensate));
            d_water -= d_t;

            if(i_type == 6 || i_type == 4 || i_type == 2)
            {
                d_dosage = 0;
                d_water = d_total;
            }

            //更新数据库
            s_sql = "Update abs_cup_details set Statues='加药',IsUsing=1,BottleNum= " + i_bottleNum + ",SampleDosage='" + string.Format("{0:F3}", d_dosage) + "',AdditivesNum = '" + i_additivesNum + "',StartWave='" + Lib_Card.Configure.Parameter.Other_StartWave + "',EndWave='" + Lib_Card.Configure.Parameter.Other_EndWave + "',IntWave='" + Lib_Card.Configure.Parameter.Other_IntWave + "',AdditivesDosage='" + string.Format("{0:F3}", d_water) + "',Pulse=0,Cooperate=5,Type ="+ i_type+",RealSampleDosage=0.0,RealAdditivesDosage=0.0 where CupNum = '" + i_cupNum + "';";
            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
        }

        //    #region 标准观察者色度匹配函数 
        //    // CIE 1964 10°标准观察者色度匹配函数
        //    private static readonly Dictionary<int, (double X, double Y, double Z)> CIE_1964_10 = new Dictionary<int, (double X, double Y, double Z)>
        //{

        //    { 360, (0.000000122200, 0.000000013398, 0.000000535027) },
        //    { 361, (0.000000185138, 0.000000020294, 0.000000810720) },
        //    { 362, (0.000000278830, 0.000000030560, 0.000001221200) },
        //    { 363, (0.000000417470, 0.000000045740, 0.000001828700) },
        //    { 364, (0.000000621330, 0.000000068050, 0.000002722200) },
        //    { 365, (0.000000919270, 0.000000100650, 0.000004028300) },
        //    { 366, (0.000001351980, 0.000000147980, 0.000005925700) },
        //    { 367, (0.000001976540, 0.000000216270, 0.000008665100) },
        //    { 368, (0.000002872500, 0.000000314200, 0.000012596000) },
        //    { 369, (0.000004149500, 0.000000453700, 0.000018201000) },
        //    { 370, (0.000005958600, 0.000000651100, 0.000026143700) },
        //    { 371, (0.000008505600, 0.000000928800, 0.000037330000) },
        //    { 372, (0.000012068600, 0.000001317500, 0.000052987000) },
        //    { 373, (0.000017022600, 0.000001857200, 0.000074764000) },
        //    { 374, (0.000023868000, 0.000002602000, 0.000104870000) },
        //    { 375, (0.000033266000, 0.000003625000, 0.000146220000) },
        //    { 376, (0.000046087000, 0.000005019000, 0.000202660000) },
        //    { 377, (0.000063472000, 0.000006907000, 0.000279230000) },
        //    { 378, (0.000086892000, 0.000009449000, 0.000382450000) },
        //    { 379, (0.000118246000, 0.000012848000, 0.000520720000) },
        //    { 380, (0.000159952000, 0.000017364000, 0.000704776000) },
        //    { 381, (0.000215080000, 0.000023327000, 0.000948230000) },
        //    { 382, (0.000287490000, 0.000031150000, 0.001268200000) },
        //    { 383, (0.000381990000, 0.000041350000, 0.001686100000) },
        //    { 384, (0.000504550000, 0.000054560000, 0.002228500000) },
        //    { 385, (0.000662440000, 0.000071560000, 0.002927800000) },
        //    { 386, (0.000864500000, 0.000093300000, 0.003823700000) },
        //    { 387, (0.001121500000, 0.000120870000, 0.004964200000) },
        //    { 388, (0.001446160000, 0.000155640000, 0.006406700000) },
        //    { 389, (0.001853590000, 0.000199200000, 0.008219300000) },
        //    { 390, (0.002361600000, 0.000253400000, 0.010482200000) },
        //    { 391, (0.002990600000, 0.000320200000, 0.013289000000) },
        //    { 392, (0.003764500000, 0.000402400000, 0.016747000000) },
        //    { 393, (0.004710200000, 0.000502300000, 0.020980000000) },
        //    { 394, (0.005858100000, 0.000623200000, 0.026127000000) },
        //    { 395, (0.007242300000, 0.000768500000, 0.032344000000) },
        //    { 396, (0.008899600000, 0.000941700000, 0.039802000000) },
        //    { 397, (0.010870900000, 0.001147800000, 0.048691000000) },
        //    { 398, (0.013198900000, 0.001390300000, 0.059210000000) },
        //    { 399, (0.015929200000, 0.001674000000, 0.071576000000) },
        //    { 400, (0.019109700000, 0.002004400000, 0.086010900000) },
        //    { 401, (0.022788000000, 0.002386000000, 0.102740000000) },
        //    { 402, (0.027011000000, 0.002822000000, 0.122000000000) },
        //    { 403, (0.031829000000, 0.003319000000, 0.144020000000) },
        //    { 404, (0.037278000000, 0.003880000000, 0.168990000000) },
        //    { 405, (0.043400000000, 0.004509000000, 0.197120000000) },
        //    { 406, (0.050223000000, 0.005209000000, 0.228570000000) },
        //    { 407, (0.057764000000, 0.005985000000, 0.263470000000) },
        //    { 408, (0.066038000000, 0.006833000000, 0.301900000000) },
        //    { 409, (0.075033000000, 0.007757000000, 0.343870000000) },
        //    { 410, (0.084736000000, 0.008756000000, 0.389366000000) },
        //    { 411, (0.095041000000, 0.009816000000, 0.437970000000) },
        //    { 412, (0.105836000000, 0.010918000000, 0.489220000000) },
        //    { 413, (0.117066000000, 0.012058000000, 0.542900000000) },
        //    { 414, (0.128682000000, 0.013237000000, 0.598810000000) },
        //    { 415, (0.140638000000, 0.014456000000, 0.656760000000) },
        //    { 416, (0.152893000000, 0.015717000000, 0.716580000000) },
        //    { 417, (0.165416000000, 0.017025000000, 0.778120000000) },
        //    { 418, (0.178191000000, 0.018399000000, 0.841310000000) },
        //    { 419, (0.191214000000, 0.019848000000, 0.906110000000) },
        //    { 420, (0.204492000000, 0.021391000000, 0.972542000000) },
        //    { 421, (0.217650000000, 0.022992000000, 1.038900000000) },
        //    { 422, (0.230267000000, 0.024598000000, 1.103100000000) },
        //    { 423, (0.242311000000, 0.026213000000, 1.165100000000) },
        //    { 424, (0.253793000000, 0.027841000000, 1.224900000000) },
        //    { 425, (0.264737000000, 0.029497000000, 1.282500000000) },
        //    { 426, (0.275195000000, 0.031195000000, 1.338200000000) },
        //    { 427, (0.285301000000, 0.032927000000, 1.392600000000) },
        //    { 428, (0.295143000000, 0.034738000000, 1.446100000000) },
        //    { 429, (0.304869000000, 0.036654000000, 1.499400000000) },
        //    { 430, (0.314679000000, 0.038676000000, 1.553480000000) },
        //    { 431, (0.324355000000, 0.040792000000, 1.607200000000) },
        //    { 432, (0.333570000000, 0.042946000000, 1.658900000000) },
        //    { 433, (0.342243000000, 0.045114000000, 1.708200000000) },
        //    { 434, (0.350312000000, 0.047333000000, 1.754800000000) },
        //    { 435, (0.357719000000, 0.049602000000, 1.798500000000) },
        //    { 436, (0.364482000000, 0.051934000000, 1.839200000000) },
        //    { 437, (0.370493000000, 0.054337000000, 1.876600000000) },
        //    { 438, (0.375727000000, 0.056822000000, 1.910500000000) },
        //    { 439, (0.380158000000, 0.059399000000, 1.940800000000) },
        //    { 440, (0.383734000000, 0.062077000000, 1.967280000000) },
        //    { 441, (0.386327000000, 0.064737000000, 1.989100000000) },
        //    { 442, (0.387858000000, 0.067285000000, 2.005700000000) },
        //    { 443, (0.388396000000, 0.069764000000, 2.017400000000) },
        //    { 444, (0.387978000000, 0.072218000000, 2.024400000000) },
        //    { 445, (0.386726000000, 0.074704000000, 2.027300000000) },
        //    { 446, (0.384696000000, 0.077272000000, 2.026400000000) },
        //    { 447, (0.382006000000, 0.079979000000, 2.022300000000) },
        //    { 448, (0.378709000000, 0.082874000000, 2.015300000000) },
        //    { 449, (0.374915000000, 0.086000000000, 2.006000000000) },
        //    { 450, (0.370702000000, 0.089456000000, 1.994800000000) },
        //    { 451, (0.366089000000, 0.092947000000, 1.981400000000) },
        //    { 452, (0.361045000000, 0.096275000000, 1.965300000000) },
        //    { 453, (0.355518000000, 0.099535000000, 1.946400000000) },
        //    { 454, (0.349486000000, 0.102829000000, 1.924800000000) },
        //    { 455, (0.342957000000, 0.106256000000, 1.900700000000) },
        //    { 456, (0.335893000000, 0.109901000000, 1.874100000000) },
        //    { 457, (0.328284000000, 0.113835000000, 1.845100000000) },
        //    { 458, (0.320150000000, 0.118167000000, 1.813900000000) },
        //    { 459, (0.311475000000, 0.122932000000, 1.780600000000) },
        //    { 460, (0.302273000000, 0.128201000000, 1.745370000000) },
        //    { 461, (0.292858000000, 0.133457000000, 1.709100000000) },
        //    { 462, (0.283502000000, 0.138323000000, 1.672300000000) },
        //    { 463, (0.274044000000, 0.143042000000, 1.634700000000) },
        //    { 464, (0.264263000000, 0.147787000000, 1.595600000000) },
        //    { 465, (0.254085000000, 0.152761000000, 1.554900000000) },
        //    { 466, (0.243392000000, 0.158102000000, 1.512200000000) },
        //    { 467, (0.232187000000, 0.163941000000, 1.467300000000) },
        //    { 468, (0.220488000000, 0.170362000000, 1.419900000000) },
        //    { 469, (0.208198000000, 0.177425000000, 1.370000000000) },
        //    { 470, (0.195618000000, 0.185190000000, 1.317560000000) },
        //    { 471, (0.183034000000, 0.193025000000, 1.262400000000) },
        //    { 472, (0.170222000000, 0.200313000000, 1.205000000000) },
        //    { 473, (0.157348000000, 0.207156000000, 1.146600000000) },
        //    { 474, (0.144650000000, 0.213644000000, 1.088000000000) },
        //    { 475, (0.132349000000, 0.219940000000, 1.030200000000) },
        //    { 476, (0.120584000000, 0.226170000000, 0.973830000000) },
        //    { 477, (0.109456000000, 0.232467000000, 0.919430000000) },
        //    { 478, (0.099042000000, 0.239025000000, 0.867460000000) },
        //    { 479, (0.089388000000, 0.245997000000, 0.818280000000) },
        //    { 480, (0.080507000000, 0.253589000000, 0.772125000000) },
        //    { 481, (0.072034000000, 0.261876000000, 0.728290000000) },
        //    { 482, (0.063710000000, 0.270643000000, 0.686040000000) },
        //    { 483, (0.055694000000, 0.279645000000, 0.645530000000) },
        //    { 484, (0.048117000000, 0.288694000000, 0.606850000000) },
        //    { 485, (0.041072000000, 0.297665000000, 0.570060000000) },
        //    { 486, (0.034642000000, 0.306469000000, 0.535220000000) },
        //    { 487, (0.028896000000, 0.315035000000, 0.502340000000) },
        //    { 488, (0.023876000000, 0.323335000000, 0.471400000000) },
        //    { 489, (0.019628000000, 0.331366000000, 0.442390000000) },
        //    { 490, (0.016172000000, 0.339133000000, 0.415254000000) },
        //    { 491, (0.013300000000, 0.347860000000, 0.390024000000) },
        //    { 492, (0.010759000000, 0.358326000000, 0.366399000000) },
        //    { 493, (0.008542000000, 0.370001000000, 0.344015000000) },
        //    { 494, (0.006661000000, 0.382464000000, 0.322689000000) },
        //    { 495, (0.005132000000, 0.395379000000, 0.302356000000) },
        //    { 496, (0.003982000000, 0.408482000000, 0.283036000000) },
        //    { 497, (0.003239000000, 0.421588000000, 0.264816000000) },
        //    { 498, (0.002934000000, 0.434619000000, 0.247848000000) },
        //    { 499, (0.003114000000, 0.447601000000, 0.232318000000) },
        //    { 500, (0.003816000000, 0.460777000000, 0.218502000000) },
        //    { 501, (0.005095000000, 0.474340000000, 0.205851000000) },
        //    { 502, (0.006936000000, 0.488200000000, 0.193596000000) },
        //    { 503, (0.009299000000, 0.502340000000, 0.181736000000) },
        //    { 504, (0.012147000000, 0.516740000000, 0.170281000000) },
        //    { 505, (0.015444000000, 0.531360000000, 0.159249000000) },
        //    { 506, (0.019156000000, 0.546190000000, 0.148673000000) },
        //    { 507, (0.023250000000, 0.561180000000, 0.138609000000) },
        //    { 508, (0.027690000000, 0.576290000000, 0.129096000000) },
        //    { 509, (0.032444000000, 0.591500000000, 0.120215000000) },
        //    { 510, (0.037465000000, 0.606741000000, 0.112044000000) },
        //    { 511, (0.042956000000, 0.622150000000, 0.104710000000) },
        //    { 512, (0.049114000000, 0.637830000000, 0.098196000000) },
        //    { 513, (0.055920000000, 0.653710000000, 0.092361000000) },
        //    { 514, (0.063349000000, 0.669680000000, 0.087088000000) },
        //    { 515, (0.071358000000, 0.685660000000, 0.082248000000) },
        //    { 516, (0.079901000000, 0.701550000000, 0.077744000000) },
        //    { 517, (0.088909000000, 0.717230000000, 0.073456000000) },
        //    { 518, (0.098293000000, 0.732570000000, 0.069268000000) },
        //    { 519, (0.107949000000, 0.747460000000, 0.065060000000) },
        //    { 520, (0.117749000000, 0.761757000000, 0.060709000000) },
        //    { 521, (0.127839000000, 0.775340000000, 0.056457000000) },
        //    { 522, (0.138450000000, 0.788220000000, 0.052609000000) },
        //    { 523, (0.149516000000, 0.800460000000, 0.049122000000) },
        //    { 524, (0.161041000000, 0.812140000000, 0.045954000000) },
        //    { 525, (0.172953000000, 0.823330000000, 0.043050000000) },
        //    { 526, (0.185209000000, 0.834120000000, 0.040368000000) },
        //    { 527, (0.197755000000, 0.844600000000, 0.037839000000) },
        //    { 528, (0.210538000000, 0.854870000000, 0.035384000000) },
        //    { 529, (0.223460000000, 0.865040000000, 0.032949000000) },
        //    { 530, (0.236491000000, 0.875211000000, 0.030451000000) },
        //    { 531, (0.249633000000, 0.885370000000, 0.028029000000) },
        //    { 532, (0.262972000000, 0.895370000000, 0.025862000000) },
        //    { 533, (0.276515000000, 0.905150000000, 0.023920000000) },
        //    { 534, (0.290269000000, 0.914650000000, 0.022174000000) },
        //    { 535, (0.304213000000, 0.923810000000, 0.020584000000) },
        //    { 536, (0.318361000000, 0.932550000000, 0.019127000000) },
        //    { 537, (0.332705000000, 0.940810000000, 0.017740000000) },
        //    { 538, (0.347232000000, 0.948520000000, 0.016403000000) },
        //    { 539, (0.361926000000, 0.955600000000, 0.015064000000) },
        //    { 540, (0.376772000000, 0.961988000000, 0.013676000000) },
        //    { 541, (0.391683000000, 0.967540000000, 0.012308000000) },
        //    { 542, (0.406594000000, 0.972230000000, 0.011056000000) },
        //    { 543, (0.421539000000, 0.976170000000, 0.009915000000) },
        //    { 544, (0.436517000000, 0.979460000000, 0.008872000000) },
        //    { 545, (0.451584000000, 0.982200000000, 0.007918000000) },
        //    { 546, (0.466782000000, 0.984520000000, 0.007030000000) },
        //    { 547, (0.482147000000, 0.986520000000, 0.006223000000) },
        //    { 548, (0.497738000000, 0.988320000000, 0.005453000000) },
        //    { 549, (0.513606000000, 0.990020000000, 0.004714000000) },
        //    { 550, (0.529826000000, 0.991761000000, 0.003988000000) },
        //    { 551, (0.546440000000, 0.993530000000, 0.003289000000) },
        //    { 552, (0.563426000000, 0.995230000000, 0.002646000000) },
        //    { 553, (0.580726000000, 0.996770000000, 0.002063000000) },
        //    { 554, (0.598290000000, 0.998090000000, 0.001533000000) },
        //    { 555, (0.616053000000, 0.999110000000, 0.001091000000) },
        //    { 556, (0.633948000000, 0.999770000000, 0.000711000000) },
        //    { 557, (0.651901000000, 1.000000000000, 0.000407000000) },
        //    { 558, (0.669824000000, 0.999710000000, 0.000184000000) },
        //    { 559, (0.687632000000, 0.998850000000, 0.000047000000) },
        //    { 560, (0.705224000000, 0.997340000000, 0.000000000000) },
        //    { 561, (0.722773000000, 0.995260000000, 0.000000000000) },
        //    { 562, (0.740483000000, 0.992740000000, 0.000000000000) },
        //    { 563, (0.758273000000, 0.989750000000, 0.000000000000) },
        //    { 564, (0.776083000000, 0.986300000000, 0.000000000000) },
        //    { 565, (0.793832000000, 0.982380000000, 0.000000000000) },
        //    { 566, (0.811436000000, 0.977980000000, 0.000000000000) },
        //    { 567, (0.828822000000, 0.973110000000, 0.000000000000) },
        //    { 568, (0.845879000000, 0.967740000000, 0.000000000000) },
        //    { 569, (0.862525000000, 0.961890000000, 0.000000000000) },
        //    { 570, (0.878655000000, 0.955552000000, 0.000000000000) },
        //    { 571, (0.894208000000, 0.948601000000, 0.000000000000) },
        //    { 572, (0.909206000000, 0.940981000000, 0.000000000000) },
        //    { 573, (0.923672000000, 0.932798000000, 0.000000000000) },
        //    { 574, (0.937638000000, 0.924158000000, 0.000000000000) },
        //    { 575, (0.951162000000, 0.915175000000, 0.000000000000) },
        //    { 576, (0.964283000000, 0.905954000000, 0.000000000000) },
        //    { 577, (0.977068000000, 0.896608000000, 0.000000000000) },
        //    { 578, (0.989590000000, 0.887249000000, 0.000000000000) },
        //    { 579, (1.001910000000, 0.877986000000, 0.000000000000) },
        //    { 580, (1.014160000000, 0.868934000000, 0.000000000000) },
        //    { 581, (1.026500000000, 0.860164000000, 0.000000000000) },
        //    { 582, (1.038800000000, 0.851519000000, 0.000000000000) },
        //    { 583, (1.051000000000, 0.842963000000, 0.000000000000) },
        //    { 584, (1.062900000000, 0.834393000000, 0.000000000000) },
        //    { 585, (1.074300000000, 0.825623000000, 0.000000000000) },
        //    { 586, (1.085200000000, 0.816764000000, 0.000000000000) },
        //    { 587, (1.095200000000, 0.807544000000, 0.000000000000) },
        //    { 588, (1.104200000000, 0.797947000000, 0.000000000000) },
        //    { 589, (1.112000000000, 0.787893000000, 0.000000000000) },
        //    { 590, (1.118520000000, 0.777405000000, 0.000000000000) },
        //    { 591, (1.123800000000, 0.766490000000, 0.000000000000) },
        //    { 592, (1.128000000000, 0.755309000000, 0.000000000000) },
        //    { 593, (1.131100000000, 0.743845000000, 0.000000000000) },
        //    { 594, (1.133200000000, 0.732190000000, 0.000000000000) },
        //    { 595, (1.134300000000, 0.720353000000, 0.000000000000) },
        //    { 596, (1.134300000000, 0.708281000000, 0.000000000000) },
        //    { 597, (1.133300000000, 0.696055000000, 0.000000000000) },
        //    { 598, (1.131200000000, 0.683621000000, 0.000000000000) },
        //    { 599, (1.128100000000, 0.671048000000, 0.000000000000) },
        //    { 600, (1.123990000000, 0.658341000000, 0.000000000000) },
        //    { 601, (1.118900000000, 0.645545000000, 0.000000000000) },
        //    { 602, (1.112900000000, 0.632718000000, 0.000000000000) },
        //    { 603, (1.105900000000, 0.619815000000, 0.000000000000) },
        //    { 604, (1.098000000000, 0.606887000000, 0.000000000000) },
        //    { 605, (1.089100000000, 0.593878000000, 0.000000000000) },
        //    { 606, (1.079200000000, 0.580781000000, 0.000000000000) },
        //    { 607, (1.068400000000, 0.567653000000, 0.000000000000) },
        //    { 608, (1.056700000000, 0.554490000000, 0.000000000000) },
        //    { 609, (1.044000000000, 0.541228000000, 0.000000000000) },
        //    { 610, (1.030480000000, 0.527963000000, 0.000000000000) },
        //    { 611, (1.016000000000, 0.514634000000, 0.000000000000) },
        //    { 612, (1.000800000000, 0.501363000000, 0.000000000000) },
        //    { 613, (0.984790000000, 0.488124000000, 0.000000000000) },
        //    { 614, (0.968080000000, 0.474935000000, 0.000000000000) },
        //    { 615, (0.950740000000, 0.461834000000, 0.000000000000) },
        //    { 616, (0.932800000000, 0.448823000000, 0.000000000000) },
        //    { 617, (0.914340000000, 0.435917000000, 0.000000000000) },
        //    { 618, (0.895390000000, 0.423153000000, 0.000000000000) },
        //    { 619, (0.876030000000, 0.410526000000, 0.000000000000) },
        //    { 620, (0.856297000000, 0.398057000000, 0.000000000000) },
        //    { 621, (0.836350000000, 0.385835000000, 0.000000000000) },
        //    { 622, (0.816290000000, 0.373951000000, 0.000000000000) },
        //    { 623, (0.796050000000, 0.362311000000, 0.000000000000) },
        //    { 624, (0.775610000000, 0.350863000000, 0.000000000000) },
        //    { 625, (0.754930000000, 0.339554000000, 0.000000000000) },
        //    { 626, (0.733990000000, 0.328309000000, 0.000000000000) },
        //    { 627, (0.712780000000, 0.317118000000, 0.000000000000) },
        //    { 628, (0.691290000000, 0.305936000000, 0.000000000000) },
        //    { 629, (0.669520000000, 0.294737000000, 0.000000000000) },
        //    { 630, (0.647467000000, 0.283493000000, 0.000000000000) },
        //    { 631, (0.625110000000, 0.272222000000, 0.000000000000) },
        //    { 632, (0.602520000000, 0.260990000000, 0.000000000000) },
        //    { 633, (0.579890000000, 0.249877000000, 0.000000000000) },
        //    { 634, (0.557370000000, 0.238946000000, 0.000000000000) },
        //    { 635, (0.535110000000, 0.228254000000, 0.000000000000) },
        //    { 636, (0.513240000000, 0.217853000000, 0.000000000000) },
        //    { 637, (0.491860000000, 0.207780000000, 0.000000000000) },
        //    { 638, (0.471080000000, 0.198072000000, 0.000000000000) },
        //    { 639, (0.450960000000, 0.188748000000, 0.000000000000) },
        //    { 640, (0.431567000000, 0.179828000000, 0.000000000000) },
        //    { 641, (0.412870000000, 0.171285000000, 0.000000000000) },
        //    { 642, (0.394750000000, 0.163059000000, 0.000000000000) },
        //    { 643, (0.377210000000, 0.155151000000, 0.000000000000) },
        //    { 644, (0.360190000000, 0.147535000000, 0.000000000000) },
        //    { 645, (0.343690000000, 0.140211000000, 0.000000000000) },
        //    { 646, (0.327690000000, 0.133170000000, 0.000000000000) },
        //    { 647, (0.312170000000, 0.126400000000, 0.000000000000) },
        //    { 648, (0.297110000000, 0.119892000000, 0.000000000000) },
        //    { 649, (0.282500000000, 0.113640000000, 0.000000000000) },
        //    { 650, (0.268329000000, 0.107633000000, 0.000000000000) },
        //    { 651, (0.254590000000, 0.101870000000, 0.000000000000) },
        //    { 652, (0.241300000000, 0.096347000000, 0.000000000000) },
        //    { 653, (0.228480000000, 0.091063000000, 0.000000000000) },
        //    { 654, (0.216140000000, 0.086010000000, 0.000000000000) },
        //    { 655, (0.204300000000, 0.081187000000, 0.000000000000) },
        //    { 656, (0.192950000000, 0.076583000000, 0.000000000000) },
        //    { 657, (0.182110000000, 0.072198000000, 0.000000000000) },
        //    { 658, (0.171770000000, 0.068024000000, 0.000000000000) },
        //    { 659, (0.161920000000, 0.064052000000, 0.000000000000) },
        //    { 660, (0.152568000000, 0.060281000000, 0.000000000000) },
        //    { 661, (0.143670000000, 0.056697000000, 0.000000000000) },
        //    { 662, (0.135200000000, 0.053292000000, 0.000000000000) },
        //    { 663, (0.127130000000, 0.050059000000, 0.000000000000) },
        //    { 664, (0.119480000000, 0.046998000000, 0.000000000000) },
        //    { 665, (0.112210000000, 0.044096000000, 0.000000000000) },
        //    { 666, (0.105310000000, 0.041345000000, 0.000000000000) },
        //    { 667, (0.098786000000, 0.038750700000, 0.000000000000) },
        //    { 668, (0.092610000000, 0.036297800000, 0.000000000000) },
        //    { 669, (0.086773000000, 0.033983200000, 0.000000000000) },
        //    { 670, (0.081260600000, 0.031800400000, 0.000000000000) },
        //    { 671, (0.076048000000, 0.029739500000, 0.000000000000) },
        //    { 672, (0.071114000000, 0.027791800000, 0.000000000000) },
        //    { 673, (0.066454000000, 0.025955100000, 0.000000000000) },
        //    { 674, (0.062062000000, 0.024226300000, 0.000000000000) },
        //    { 675, (0.057930000000, 0.022601700000, 0.000000000000) },
        //    { 676, (0.054050000000, 0.021077900000, 0.000000000000) },
        //    { 677, (0.050412000000, 0.019650500000, 0.000000000000) },
        //    { 678, (0.047006000000, 0.018315300000, 0.000000000000) },
        //    { 679, (0.043823000000, 0.017068600000, 0.000000000000) },
        //    { 680, (0.040850800000, 0.015905100000, 0.000000000000) },
        //    { 681, (0.038072000000, 0.014818300000, 0.000000000000) },
        //    { 682, (0.035468000000, 0.013800800000, 0.000000000000) },
        //    { 683, (0.033031000000, 0.012849500000, 0.000000000000) },
        //    { 684, (0.030753000000, 0.011960700000, 0.000000000000) },
        //    { 685, (0.028623000000, 0.011130300000, 0.000000000000) },
        //    { 686, (0.026635000000, 0.010355500000, 0.000000000000) },
        //    { 687, (0.024781000000, 0.009633200000, 0.000000000000) },
        //    { 688, (0.023052000000, 0.008959900000, 0.000000000000) },
        //    { 689, (0.021441000000, 0.008332400000, 0.000000000000) },
        //    { 690, (0.019941300000, 0.007748800000, 0.000000000000) },
        //    { 691, (0.018544000000, 0.007204600000, 0.000000000000) },
        //    { 692, (0.017241000000, 0.006697500000, 0.000000000000) },
        //    { 693, (0.016027000000, 0.006225100000, 0.000000000000) },
        //    { 694, (0.014896000000, 0.005785000000, 0.000000000000) },
        //    { 695, (0.013842000000, 0.005375100000, 0.000000000000) },
        //    { 696, (0.012862000000, 0.004994100000, 0.000000000000) },
        //    { 697, (0.011949000000, 0.004639200000, 0.000000000000) },
        //    { 698, (0.011100000000, 0.004309300000, 0.000000000000) },
        //    { 699, (0.010311000000, 0.004002800000, 0.000000000000) },
        //    { 700, (0.009576880000, 0.003717740000, 0.000000000000) },
        //    { 701, (0.008894000000, 0.003452620000, 0.000000000000) },
        //    { 702, (0.008258100000, 0.003205830000, 0.000000000000) },
        //    { 703, (0.007666400000, 0.002976230000, 0.000000000000) },
        //    { 704, (0.007116300000, 0.002762810000, 0.000000000000) },
        //    { 705, (0.006605200000, 0.002564560000, 0.000000000000) },
        //    { 706, (0.006130600000, 0.002380480000, 0.000000000000) },
        //    { 707, (0.005690300000, 0.002209710000, 0.000000000000) },
        //    { 708, (0.005281900000, 0.002051320000, 0.000000000000) },
        //    { 709, (0.004903300000, 0.001904490000, 0.000000000000) },
        //    { 710, (0.004552630000, 0.001768470000, 0.000000000000) },
        //    { 711, (0.004227500000, 0.001642360000, 0.000000000000) },
        //    { 712, (0.003925800000, 0.001525350000, 0.000000000000) },
        //    { 713, (0.003645700000, 0.001416720000, 0.000000000000) },
        //    { 714, (0.003385900000, 0.001315950000, 0.000000000000) },
        //    { 715, (0.003144700000, 0.001222390000, 0.000000000000) },
        //    { 716, (0.002920800000, 0.001135550000, 0.000000000000) },
        //    { 717, (0.002713000000, 0.001054940000, 0.000000000000) },
        //    { 718, (0.002520200000, 0.000980140000, 0.000000000000) },
        //    { 719, (0.002341100000, 0.000910660000, 0.000000000000) },
        //    { 720, (0.002174960000, 0.000846190000, 0.000000000000) },
        //    { 721, (0.002020600000, 0.000786290000, 0.000000000000) },
        //    { 722, (0.001877300000, 0.000730680000, 0.000000000000) },
        //    { 723, (0.001744100000, 0.000678990000, 0.000000000000) },
        //    { 724, (0.001620500000, 0.000631010000, 0.000000000000) },
        //    { 725, (0.001505700000, 0.000586440000, 0.000000000000) },
        //    { 726, (0.001399200000, 0.000545110000, 0.000000000000) },
        //    { 727, (0.001300400000, 0.000506720000, 0.000000000000) },
        //    { 728, (0.001208700000, 0.000471110000, 0.000000000000) },
        //    { 729, (0.001123600000, 0.000438050000, 0.000000000000) },
        //    { 730, (0.001044760000, 0.000407410000, 0.000000000000) },
        //    { 731, (0.000971560000, 0.000378962000, 0.000000000000) },
        //    { 732, (0.000903600000, 0.000352543000, 0.000000000000) },
        //    { 733, (0.000840480000, 0.000328001000, 0.000000000000) },
        //    { 734, (0.000781870000, 0.000305208000, 0.000000000000) },
        //    { 735, (0.000727450000, 0.000284041000, 0.000000000000) },
        //    { 736, (0.000676900000, 0.000264375000, 0.000000000000) },
        //    { 737, (0.000629960000, 0.000246109000, 0.000000000000) },
        //    { 738, (0.000586370000, 0.000229143000, 0.000000000000) },
        //    { 739, (0.000545870000, 0.000213376000, 0.000000000000) },
        //    { 740, (0.000508258000, 0.000198730000, 0.000000000000) },
        //    { 741, (0.000473300000, 0.000185115000, 0.000000000000) },
        //    { 742, (0.000440800000, 0.000172454000, 0.000000000000) },
        //    { 743, (0.000410580000, 0.000160678000, 0.000000000000) },
        //    { 744, (0.000382490000, 0.000149730000, 0.000000000000) },
        //    { 745, (0.000356380000, 0.000139550000, 0.000000000000) },
        //    { 746, (0.000332110000, 0.000130086000, 0.000000000000) },
        //    { 747, (0.000309550000, 0.000121290000, 0.000000000000) },
        //    { 748, (0.000288580000, 0.000113106000, 0.000000000000) },
        //    { 749, (0.000269090000, 0.000105501000, 0.000000000000) },
        //    { 750, (0.000250969000, 0.000098428000, 0.000000000000) },
        //    { 751, (0.000234130000, 0.000091853000, 0.000000000000) },
        //    { 752, (0.000218470000, 0.000085738000, 0.000000000000) },
        //    { 753, (0.000203910000, 0.000080048000, 0.000000000000) },
        //    { 754, (0.000190350000, 0.000074751000, 0.000000000000) },
        //    { 755, (0.000177730000, 0.000069819000, 0.000000000000) },
        //    { 756, (0.000165970000, 0.000065222000, 0.000000000000) },
        //    { 757, (0.000155020000, 0.000060939000, 0.000000000000) },
        //    { 758, (0.000144800000, 0.000056942000, 0.000000000000) },
        //    { 759, (0.000135280000, 0.000053217000, 0.000000000000) },
        //    { 760, (0.000126390000, 0.000049737000, 0.000000000000) },
        //    { 761, (0.000118100000, 0.000046491000, 0.000000000000) },
        //    { 762, (0.000110370000, 0.000043464000, 0.000000000000) },
        //    { 763, (0.000103150000, 0.000040635000, 0.000000000000) },
        //    { 764, (0.000096427000, 0.000038000000, 0.000000000000) },
        //    { 765, (0.000090151000, 0.000035540500, 0.000000000000) },
        //    { 766, (0.000084294000, 0.000033244800, 0.000000000000) },
        //    { 767, (0.000078830000, 0.000031100600, 0.000000000000) },
        //    { 768, (0.000073729000, 0.000029099000, 0.000000000000) },
        //    { 769, (0.000068969000, 0.000027230700, 0.000000000000) },
        //    { 770, (0.000064525800, 0.000025486000, 0.000000000000) },
        //    { 771, (0.000060376000, 0.000023856100, 0.000000000000) },
        //    { 772, (0.000056500000, 0.000022333200, 0.000000000000) },
        //    { 773, (0.000052880000, 0.000020910400, 0.000000000000) },
        //    { 774, (0.000049498000, 0.000019580800, 0.000000000000) },
        //    { 775, (0.000046339000, 0.000018338400, 0.000000000000) },
        //    { 776, (0.000043389000, 0.000017177700, 0.000000000000) },
        //    { 777, (0.000040634000, 0.000016093400, 0.000000000000) },
        //    { 778, (0.000038060000, 0.000015080000, 0.000000000000) },
        //    { 779, (0.000035657000, 0.000014133600, 0.000000000000) },
        //    { 780, (0.000033411700, 0.000013249000, 0.000000000000) },
        //    { 781, (0.000031315000, 0.000012422600, 0.000000000000) },
        //    { 782, (0.000029355000, 0.000011649900, 0.000000000000) },
        //    { 783, (0.000027524000, 0.000010927700, 0.000000000000) },
        //    { 784, (0.000025811000, 0.000010251900, 0.000000000000) },
        //    { 785, (0.000024209000, 0.000009619600, 0.000000000000) },
        //    { 786, (0.000022711000, 0.000009028100, 0.000000000000) },
        //    { 787, (0.000021308000, 0.000008474000, 0.000000000000) },
        //    { 788, (0.000019994000, 0.000007954800, 0.000000000000) },
        //    { 789, (0.000018764000, 0.000007468600, 0.000000000000) },
        //    { 790, (0.000017611500, 0.000007012800, 0.000000000000) },
        //    { 791, (0.000016532000, 0.000006585800, 0.000000000000) },
        //    { 792, (0.000015521000, 0.000006185700, 0.000000000000) },
        //    { 793, (0.000014574000, 0.000005810700, 0.000000000000) },
        //    { 794, (0.000013686000, 0.000005459000, 0.000000000000) },
        //    { 795, (0.000012855000, 0.000005129800, 0.000000000000) },
        //    { 796, (0.000012075000, 0.000004820600, 0.000000000000) },
        //    { 797, (0.000011345000, 0.000004531200, 0.000000000000) },
        //    { 798, (0.000010659000, 0.000004259100, 0.000000000000) },
        //    { 799, (0.000010017000, 0.000004004200, 0.000000000000) },
        //    { 800, (0.000009413630, 0.000003764730, 0.000000000000) },
        //    { 801, (0.000008847900, 0.000003539950, 0.000000000000) },
        //    { 802, (0.000008317100, 0.000003329140, 0.000000000000) },
        //    { 803, (0.000007819000, 0.000003131150, 0.000000000000) },
        //    { 804, (0.000007351600, 0.000002945290, 0.000000000000) },
        //    { 805, (0.000006913000, 0.000002770810, 0.000000000000) },
        //    { 806, (0.000006501500, 0.000002607050, 0.000000000000) },
        //    { 807, (0.000006115300, 0.000002453290, 0.000000000000) },
        //    { 808, (0.000005752900, 0.000002308940, 0.000000000000) },
        //    { 809, (0.000005412700, 0.000002173380, 0.000000000000) },
        //    { 810, (0.000005093470, 0.000002046130, 0.000000000000) },
        //    { 811, (0.000004793800, 0.000001926620, 0.000000000000) },
        //    { 812, (0.000004512500, 0.000001814400, 0.000000000000) },
        //    { 813, (0.000004248300, 0.000001708950, 0.000000000000) },
        //    { 814, (0.000004000200, 0.000001609880, 0.000000000000) },
        //    { 815, (0.000003767100, 0.000001516770, 0.000000000000) },
        //    { 816, (0.000003548000, 0.000001429210, 0.000000000000) },
        //    { 817, (0.000003342100, 0.000001346860, 0.000000000000) },
        //    { 818, (0.000003148500, 0.000001269450, 0.000000000000) },
        //    { 819, (0.000002966500, 0.000001196620, 0.000000000000) },
        //    { 820, (0.000002795310, 0.000001128090, 0.000000000000) },
        //    { 821, (0.000002634500, 0.000001063680, 0.000000000000) },
        //    { 822, (0.000002483400, 0.000001003130, 0.000000000000) },
        //    { 823, (0.000002341400, 0.000000946220, 0.000000000000) },
        //    { 824, (0.000002207800, 0.000000892630, 0.000000000000) },
        //    { 825, (0.000002082000, 0.000000842160, 0.000000000000) },
        //    { 826, (0.000001963600, 0.000000794640, 0.000000000000) },
        //    { 827, (0.000001851900, 0.000000749780, 0.000000000000) },
        //    { 828, (0.000001746500, 0.000000707440, 0.000000000000) },
        //    { 829, (0.000001647100, 0.000000667480, 0.000000000000) },
        //    { 830, (0.000001553140, 0.000000629700, 0.000000000000) },

        // };

        //    // CIE 1931 2°标准观察者色度匹配函数
        //    private static readonly Dictionary<int, (double X, double Y, double Z)> CIE_1931_2 = new Dictionary<int, (double X, double Y, double Z)>
        //{
        //    { 360, (0.000129900000, 0.000003917000, 0.000606100000) },
        //    { 361, (0.000145847000, 0.000004393581, 0.000680879200) },
        //    { 362, (0.000163802100, 0.000004929604, 0.000765145600) },
        //    { 363, (0.000184003700, 0.000005532136, 0.000860012400) },
        //    { 364, (0.000206690200, 0.000006208245, 0.000966592800) },
        //    { 365, (0.000232100000, 0.000006965000, 0.001086000000) },
        //    { 366, (0.000260728000, 0.000007813219, 0.001220586000) },
        //    { 367, (0.000293075000, 0.000008767336, 0.001372729000) },
        //    { 368, (0.000329388000, 0.000009839844, 0.001543579000) },
        //    { 369, (0.000369914000, 0.000011043230, 0.001734286000) },
        //    { 370, (0.000414900000, 0.000012390000, 0.001946000000) },
        //    { 371, (0.000464158700, 0.000013886410, 0.002177777000) },
        //    { 372, (0.000518986000, 0.000015557280, 0.002435809000) },
        //    { 373, (0.000581854000, 0.000017442960, 0.002731953000) },
        //    { 374, (0.000655234700, 0.000019583750, 0.003078064000) },
        //    { 375, (0.000741600000, 0.000022020000, 0.003486000000) },
        //    { 376, (0.000845029600, 0.000024839650, 0.003975227000) },
        //    { 377, (0.000964526800, 0.000028041260, 0.004540880000) },
        //    { 378, (0.001094949000, 0.000031531040, 0.005158320000) },
        //    { 379, (0.001231154000, 0.000035215210, 0.005802907000) },
        //    { 380, (0.001368000000, 0.000039000000, 0.006450001000) },
        //    { 381, (0.001502050000, 0.000042826400, 0.007083216000) },
        //    { 382, (0.001642328000, 0.000046914600, 0.007745488000) },
        //    { 383, (0.001802382000, 0.000051589600, 0.008501152000) },
        //    { 384, (0.001995757000, 0.000057176400, 0.009414544000) },
        //    { 385, (0.002236000000, 0.000064000000, 0.010549990000) },
        //    { 386, (0.002535385000, 0.000072344210, 0.011965800000) },
        //    { 387, (0.002892603000, 0.000082212240, 0.013655870000) },
        //    { 388, (0.003300829000, 0.000093508160, 0.015588050000) },
        //    { 389, (0.003753236000, 0.000106136100, 0.017730150000) },
        //    { 390, (0.004243000000, 0.000120000000, 0.020050010000) },
        //    { 391, (0.004762389000, 0.000134984000, 0.022511360000) },
        //    { 392, (0.005330048000, 0.000151492000, 0.025202880000) },
        //    { 393, (0.005978712000, 0.000170208000, 0.028279720000) },
        //    { 394, (0.006741117000, 0.000191816000, 0.031897040000) },
        //    { 395, (0.007650000000, 0.000217000000, 0.036210000000) },
        //    { 396, (0.008751373000, 0.000246906700, 0.041437710000) },
        //    { 397, (0.010028880000, 0.000281240000, 0.047503720000) },
        //    { 398, (0.011421700000, 0.000318520000, 0.054119880000) },
        //    { 399, (0.012869010000, 0.000357266700, 0.060998030000) },
        //    { 400, (0.014310000000, 0.000396000000, 0.067850010000) },
        //    { 401, (0.015704430000, 0.000433714700, 0.074486320000) },
        //    { 402, (0.017147440000, 0.000473024000, 0.081361560000) },
        //    { 403, (0.018781220000, 0.000517876000, 0.089153640000) },
        //    { 404, (0.020748010000, 0.000572218700, 0.098540480000) },
        //    { 405, (0.023190000000, 0.000640000000, 0.110200000000) },
        //    { 406, (0.026207360000, 0.000724560000, 0.124613300000) },
        //    { 407, (0.029782480000, 0.000825500000, 0.141701700000) },
        //    { 408, (0.033880920000, 0.000941160000, 0.161303500000) },
        //    { 409, (0.038468240000, 0.001069880000, 0.183256800000) },
        //    { 410, (0.043510000000, 0.001210000000, 0.207400000000) },
        //    { 411, (0.048995600000, 0.001362091000, 0.233692100000) },
        //    { 412, (0.055022600000, 0.001530752000, 0.262611400000) },
        //    { 413, (0.061718800000, 0.001720368000, 0.294774600000) },
        //    { 414, (0.069212000000, 0.001935323000, 0.330798500000) },
        //    { 415, (0.077630000000, 0.002180000000, 0.371300000000) },
        //    { 416, (0.086958110000, 0.002454800000, 0.416209100000) },
        //    { 417, (0.097176720000, 0.002764000000, 0.465464200000) },
        //    { 418, (0.108406300000, 0.003117800000, 0.519694800000) },
        //    { 419, (0.120767200000, 0.003526400000, 0.579530300000) },
        //    { 420, (0.134380000000, 0.004000000000, 0.645600000000) },
        //    { 421, (0.149358200000, 0.004546240000, 0.718483800000) },
        //    { 422, (0.165395700000, 0.005159320000, 0.796713300000) },
        //    { 423, (0.181983100000, 0.005829280000, 0.877845900000) },
        //    { 424, (0.198611000000, 0.006546160000, 0.959439000000) },
        //    { 425, (0.214770000000, 0.007300000000, 1.039050100000) },
        //    { 426, (0.230186800000, 0.008086507000, 1.115367300000) },
        //    { 427, (0.244879700000, 0.008908720000, 1.188497100000) },
        //    { 428, (0.258777300000, 0.009767680000, 1.258123300000) },
        //    { 429, (0.271807900000, 0.010664430000, 1.323929600000) },
        //    { 430, (0.283900000000, 0.011600000000, 1.385600000000) },
        //    { 431, (0.294943800000, 0.012573170000, 1.442635200000) },
        //    { 432, (0.304896500000, 0.013582720000, 1.494803500000) },
        //    { 433, (0.313787300000, 0.014629680000, 1.542190300000) },
        //    { 434, (0.321645400000, 0.015715090000, 1.584880700000) },
        //    { 435, (0.328500000000, 0.016840000000, 1.622960000000) },
        //    { 436, (0.334351300000, 0.018007360000, 1.656404800000) },
        //    { 437, (0.339210100000, 0.019214480000, 1.685295900000) },
        //    { 438, (0.343121300000, 0.020453920000, 1.709874500000) },
        //    { 439, (0.346129600000, 0.021718240000, 1.730382100000) },
        //    { 440, (0.348280000000, 0.023000000000, 1.747060000000) },
        //    { 441, (0.349599900000, 0.024294610000, 1.760044600000) },
        //    { 442, (0.350147400000, 0.025610240000, 1.769623300000) },
        //    { 443, (0.350013000000, 0.026958570000, 1.776263700000) },
        //    { 444, (0.349287000000, 0.028351250000, 1.780433400000) },
        //    { 445, (0.348060000000, 0.029800000000, 1.782600000000) },
        //    { 446, (0.346373300000, 0.031310830000, 1.782968200000) },
        //    { 447, (0.344262400000, 0.032883680000, 1.781699800000) },
        //    { 448, (0.341808800000, 0.034521120000, 1.779198200000) },
        //    { 449, (0.339094100000, 0.036225710000, 1.775867100000) },
        //    { 450, (0.336200000000, 0.038000000000, 1.772110000000) },
        //    { 451, (0.333197700000, 0.039846670000, 1.768258900000) },
        //    { 452, (0.330041100000, 0.041768000000, 1.764039000000) },
        //    { 453, (0.326635700000, 0.043766000000, 1.758943800000) },
        //    { 454, (0.322886800000, 0.045842670000, 1.752466300000) },
        //    { 455, (0.318700000000, 0.048000000000, 1.744100000000) },
        //    { 456, (0.314025100000, 0.050243680000, 1.733559500000) },
        //    { 457, (0.308884000000, 0.052573040000, 1.720858100000) },
        //    { 458, (0.303290400000, 0.054980560000, 1.705936900000) },
        //    { 459, (0.297257900000, 0.057458720000, 1.688737200000) },
        //    { 460, (0.290800000000, 0.060000000000, 1.669200000000) },
        //    { 461, (0.283970100000, 0.062601970000, 1.647528700000) },
        //    { 462, (0.276721400000, 0.065277520000, 1.623412700000) },
        //    { 463, (0.268917800000, 0.068042080000, 1.596022300000) },
        //    { 464, (0.260422700000, 0.070911090000, 1.564528000000) },
        //    { 465, (0.251100000000, 0.073900000000, 1.528100000000) },
        //    { 466, (0.240847500000, 0.077016000000, 1.486111400000) },
        //    { 467, (0.229851200000, 0.080266400000, 1.439521500000) },
        //    { 468, (0.218407200000, 0.083666800000, 1.389879900000) },
        //    { 469, (0.206811500000, 0.087232800000, 1.338736200000) },
        //    { 470, (0.195360000000, 0.090980000000, 1.287640000000) },
        //    { 471, (0.184213600000, 0.094917550000, 1.237422300000) },
        //    { 472, (0.173327300000, 0.099045840000, 1.187824300000) },
        //    { 473, (0.162688100000, 0.103367400000, 1.138761100000) },
        //    { 474, (0.152283300000, 0.107884600000, 1.090148000000) },
        //    { 475, (0.142100000000, 0.112600000000, 1.041900000000) },
        //    { 476, (0.132178600000, 0.117532000000, 0.994197600000) },
        //    { 477, (0.122569600000, 0.122674400000, 0.947347300000) },
        //    { 478, (0.113275200000, 0.127992800000, 0.901453100000) },
        //    { 479, (0.104297900000, 0.133452800000, 0.856619300000) },
        //    { 480, (0.095640000000, 0.139020000000, 0.812950100000) },
        //    { 481, (0.087299550000, 0.144676400000, 0.770517300000) },
        //    { 482, (0.079308040000, 0.150469300000, 0.729444800000) },
        //    { 483, (0.071717760000, 0.156461900000, 0.689913600000) },
        //    { 484, (0.064580990000, 0.162717700000, 0.652104900000) },
        //    { 485, (0.057950010000, 0.169300000000, 0.616200000000) },
        //    { 486, (0.051862110000, 0.176243100000, 0.582328600000) },
        //    { 487, (0.046281520000, 0.183558100000, 0.550416200000) },
        //    { 488, (0.041150880000, 0.191273500000, 0.520337600000) },
        //    { 489, (0.036412830000, 0.199418000000, 0.491967300000) },
        //    { 490, (0.032010000000, 0.208020000000, 0.465180000000) },
        //    { 491, (0.027917200000, 0.217119900000, 0.439924600000) },
        //    { 492, (0.024144400000, 0.226734500000, 0.416183600000) },
        //    { 493, (0.020687000000, 0.236857100000, 0.393882200000) },
        //    { 494, (0.017540400000, 0.247481200000, 0.372945900000) },
        //    { 495, (0.014700000000, 0.258600000000, 0.353300000000) },
        //    { 496, (0.012161790000, 0.270184900000, 0.334857800000) },
        //    { 497, (0.009919960000, 0.282293900000, 0.317552100000) },
        //    { 498, (0.007967240000, 0.295050500000, 0.301337500000) },
        //    { 499, (0.006296346000, 0.308578000000, 0.286168600000) },
        //    { 500, (0.004900000000, 0.323000000000, 0.272000000000) },
        //    { 501, (0.003777173000, 0.338402100000, 0.258817100000) },
        //    { 502, (0.002945320000, 0.354685800000, 0.246483800000) },
        //    { 503, (0.002424880000, 0.371698600000, 0.234771800000) },
        //    { 504, (0.002236293000, 0.389287500000, 0.223453300000) },
        //    { 505, (0.002400000000, 0.407300000000, 0.212300000000) },
        //    { 506, (0.002925520000, 0.425629900000, 0.201169200000) },
        //    { 507, (0.003836560000, 0.444309600000, 0.190119600000) },
        //    { 508, (0.005174840000, 0.463394400000, 0.179225400000) },
        //    { 509, (0.006982080000, 0.482939500000, 0.168560800000) },
        //    { 510, (0.009300000000, 0.503000000000, 0.158200000000) },
        //    { 511, (0.012149490000, 0.523569300000, 0.148138300000) },
        //    { 512, (0.015535880000, 0.544512000000, 0.138375800000) },
        //    { 513, (0.019477520000, 0.565690000000, 0.128994200000) },
        //    { 514, (0.023992770000, 0.586965300000, 0.120075100000) },
        //    { 515, (0.029100000000, 0.608200000000, 0.111700000000) },
        //    { 516, (0.034814850000, 0.629345600000, 0.103904800000) },
        //    { 517, (0.041120160000, 0.650306800000, 0.096667480000) },
        //    { 518, (0.047985040000, 0.670875200000, 0.089982720000) },
        //    { 519, (0.055378610000, 0.690842400000, 0.083845310000) },
        //    { 520, (0.063270000000, 0.710000000000, 0.078249990000) },
        //    { 521, (0.071635010000, 0.728185200000, 0.073208990000) },
        //    { 522, (0.080462240000, 0.745463600000, 0.068678160000) },
        //    { 523, (0.089739960000, 0.761969400000, 0.064567840000) },
        //    { 524, (0.099456450000, 0.777836800000, 0.060788350000) },
        //    { 525, (0.109600000000, 0.793200000000, 0.057250010000) },
        //    { 526, (0.120167400000, 0.808110400000, 0.053904350000) },
        //    { 527, (0.131114500000, 0.822496200000, 0.050746640000) },
        //    { 528, (0.142367900000, 0.836306800000, 0.047752760000) },
        //    { 529, (0.153854200000, 0.849491600000, 0.044898590000) },
        //    { 530, (0.165500000000, 0.862000000000, 0.042160000000) },
        //    { 531, (0.177257100000, 0.873810800000, 0.039507280000) },
        //    { 532, (0.189140000000, 0.884962400000, 0.036935640000) },
        //    { 533, (0.201169400000, 0.895493600000, 0.034458360000) },
        //    { 534, (0.213365800000, 0.905443200000, 0.032088720000) },
        //    { 535, (0.225749900000, 0.914850100000, 0.029840000000) },
        //    { 536, (0.238320900000, 0.923734800000, 0.027711810000) },
        //    { 537, (0.251066800000, 0.932092400000, 0.025694440000) },
        //    { 538, (0.263992200000, 0.939922600000, 0.023787160000) },
        //    { 539, (0.277101700000, 0.947225200000, 0.021989250000) },
        //    { 540, (0.290400000000, 0.954000000000, 0.020300000000) },
        //    { 541, (0.303891200000, 0.960256100000, 0.018718050000) },
        //    { 542, (0.317572600000, 0.966007400000, 0.017240360000) },
        //    { 543, (0.331438400000, 0.971260600000, 0.015863640000) },
        //    { 544, (0.345482800000, 0.976022500000, 0.014584610000) },
        //    { 545, (0.359700000000, 0.980300000000, 0.013400000000) },
        //    { 546, (0.374083900000, 0.984092400000, 0.012307230000) },
        //    { 547, (0.388639600000, 0.987418200000, 0.011301880000) },
        //    { 548, (0.403378400000, 0.990312800000, 0.010377920000) },
        //    { 549, (0.418311500000, 0.992811600000, 0.009529306000) },
        //    { 550, (0.433449900000, 0.994950100000, 0.008749999000) },
        //    { 551, (0.448795300000, 0.996710800000, 0.008035200000) },
        //    { 552, (0.464336000000, 0.998098300000, 0.007381600000) },
        //    { 553, (0.480064000000, 0.999112000000, 0.006785400000) },
        //    { 554, (0.495971300000, 0.999748200000, 0.006242800000) },
        //    { 555, (0.512050100000, 1.000000000000, 0.005749999000) },
        //    { 556, (0.528295900000, 0.999856700000, 0.005303600000) },
        //    { 557, (0.544691600000, 0.999304600000, 0.004899800000) },
        //    { 558, (0.561209400000, 0.998325500000, 0.004534200000) },
        //    { 559, (0.577821500000, 0.996898700000, 0.004202400000) },
        //    { 560, (0.594500000000, 0.995000000000, 0.003900000000) },
        //    { 561, (0.611220900000, 0.992600500000, 0.003623200000) },
        //    { 562, (0.627975800000, 0.989742600000, 0.003370600000) },
        //    { 563, (0.644760200000, 0.986444400000, 0.003141400000) },
        //    { 564, (0.661569700000, 0.982724100000, 0.002934800000) },
        //    { 565, (0.678400000000, 0.978600000000, 0.002749999000) },
        //    { 566, (0.695239200000, 0.974083700000, 0.002585200000) },
        //    { 567, (0.712058600000, 0.969171200000, 0.002438600000) },
        //    { 568, (0.728828400000, 0.963856800000, 0.002309400000) },
        //    { 569, (0.745518800000, 0.958134900000, 0.002196800000) },
        //    { 570, (0.762100000000, 0.952000000000, 0.002100000000) },
        //    { 571, (0.778543200000, 0.945450400000, 0.002017733000) },
        //    { 572, (0.794825600000, 0.938499200000, 0.001948200000) },
        //    { 573, (0.810926400000, 0.931162800000, 0.001889800000) },
        //    { 574, (0.826824800000, 0.923457600000, 0.001840933000) },
        //    { 575, (0.842500000000, 0.915400000000, 0.001800000000) },
        //    { 576, (0.857932500000, 0.907006400000, 0.001766267000) },
        //    { 577, (0.873081600000, 0.898277200000, 0.001737800000) },
        //    { 578, (0.887894400000, 0.889204800000, 0.001711200000) },
        //    { 579, (0.902318100000, 0.879781600000, 0.001683067000) },
        //    { 580, (0.916300000000, 0.870000000000, 0.001650001000) },
        //    { 581, (0.929799500000, 0.859861300000, 0.001610133000) },
        //    { 582, (0.942798400000, 0.849392000000, 0.001564400000) },
        //    { 583, (0.955277600000, 0.838622000000, 0.001513600000) },
        //    { 584, (0.967217900000, 0.827581300000, 0.001458533000) },
        //    { 585, (0.978600000000, 0.816300000000, 0.001400000000) },
        //    { 586, (0.989385600000, 0.804794700000, 0.001336667000) },
        //    { 587, (0.999548800000, 0.793082000000, 0.001270000000) },
        //    { 588, (1.009089200000, 0.781192000000, 0.001205000000) },
        //    { 589, (1.018006400000, 0.769154700000, 0.001146667000) },
        //    { 590, (1.026300000000, 0.757000000000, 0.001100000000) },
        //    { 591, (1.033982700000, 0.744754100000, 0.001068800000) },
        //    { 592, (1.040986000000, 0.732422400000, 0.001049400000) },
        //    { 593, (1.047188000000, 0.720003600000, 0.001035600000) },
        //    { 594, (1.052466700000, 0.707496500000, 0.001021200000) },
        //    { 595, (1.056700000000, 0.694900000000, 0.001000000000) },
        //    { 596, (1.059794400000, 0.682219200000, 0.000968640000) },
        //    { 597, (1.061799200000, 0.669471600000, 0.000929920000) },
        //    { 598, (1.062806800000, 0.656674400000, 0.000886880000) },
        //    { 599, (1.062909600000, 0.643844800000, 0.000842560000) },
        //    { 600, (1.062200000000, 0.631000000000, 0.000800000000) },
        //    { 601, (1.060735200000, 0.618155500000, 0.000760960000) },
        //    { 602, (1.058443600000, 0.605314400000, 0.000723680000) },
        //    { 603, (1.055224400000, 0.592475600000, 0.000685920000) },
        //    { 604, (1.050976800000, 0.579637900000, 0.000645440000) },
        //    { 605, (1.045600000000, 0.566800000000, 0.000600000000) },
        //    { 606, (1.039036900000, 0.553961100000, 0.000547866700) },
        //    { 607, (1.031360800000, 0.541137200000, 0.000491600000) },
        //    { 608, (1.022666200000, 0.528352800000, 0.000435400000) },
        //    { 609, (1.013047700000, 0.515632300000, 0.000383466700) },
        //    { 610, (1.002600000000, 0.503000000000, 0.000340000000) },
        //    { 611, (0.991367500000, 0.490468800000, 0.000307253300) },
        //    { 612, (0.979331400000, 0.478030400000, 0.000283160000) },
        //    { 613, (0.966491600000, 0.465677600000, 0.000265440000) },
        //    { 614, (0.952847900000, 0.453403200000, 0.000251813300) },
        //    { 615, (0.938400000000, 0.441200000000, 0.000240000000) },
        //    { 616, (0.923194000000, 0.429080000000, 0.000229546700) },
        //    { 617, (0.907244000000, 0.417036000000, 0.000220640000) },
        //    { 618, (0.890502000000, 0.405032000000, 0.000211960000) },
        //    { 619, (0.872920000000, 0.393032000000, 0.000202186700) },
        //    { 620, (0.854449900000, 0.381000000000, 0.000190000000) },
        //    { 621, (0.835084000000, 0.368918400000, 0.000174213300) },
        //    { 622, (0.814946000000, 0.356827200000, 0.000155640000) },
        //    { 623, (0.794186000000, 0.344776800000, 0.000135960000) },
        //    { 624, (0.772954000000, 0.332817600000, 0.000116853300) },
        //    { 625, (0.751400000000, 0.321000000000, 0.000100000000) },
        //    { 626, (0.729583600000, 0.309338100000, 0.000086133330) },
        //    { 627, (0.707588800000, 0.297850400000, 0.000074600000) },
        //    { 628, (0.685602200000, 0.286593600000, 0.000065000000) },
        //    { 629, (0.663810400000, 0.275624500000, 0.000056933330) },
        //    { 630, (0.642400000000, 0.265000000000, 0.000049999990) },
        //    { 631, (0.621514900000, 0.254763200000, 0.000044160000) },
        //    { 632, (0.601113800000, 0.244889600000, 0.000039480000) },
        //    { 633, (0.581105200000, 0.235334400000, 0.000035720000) },
        //    { 634, (0.561397700000, 0.226052800000, 0.000032640000) },
        //    { 635, (0.541900000000, 0.217000000000, 0.000030000000) },
        //    { 636, (0.522599500000, 0.208161600000, 0.000027653330) },
        //    { 637, (0.503546400000, 0.199548800000, 0.000025560000) },
        //    { 638, (0.484743600000, 0.191155200000, 0.000023640000) },
        //    { 639, (0.466193900000, 0.182974400000, 0.000021813330) },
        //    { 640, (0.447900000000, 0.175000000000, 0.000020000000) },
        //    { 641, (0.429861300000, 0.167223500000, 0.000018133330) },
        //    { 642, (0.412098000000, 0.159646400000, 0.000016200000) },
        //    { 643, (0.394644000000, 0.152277600000, 0.000014200000) },
        //    { 644, (0.377533300000, 0.145125900000, 0.000012133330) },
        //    { 645, (0.360800000000, 0.138200000000, 0.000010000000) },
        //    { 646, (0.344456300000, 0.131500300000, 0.000007733333) },
        //    { 647, (0.328516800000, 0.125024800000, 0.000005400000) },
        //    { 648, (0.313019200000, 0.118779200000, 0.000003200000) },
        //    { 649, (0.298001100000, 0.112769100000, 0.000001333333) },
        //    { 650, (0.283500000000, 0.107000000000, 0.000000000000) },
        //    { 651, (0.269544800000, 0.101476200000, 0.000000000000) },
        //    { 652, (0.256118400000, 0.096188640000, 0.000000000000) },
        //    { 653, (0.243189600000, 0.091122960000, 0.000000000000) },
        //    { 654, (0.230727200000, 0.086264850000, 0.000000000000) },
        //    { 655, (0.218700000000, 0.081600000000, 0.000000000000) },
        //    { 656, (0.207097100000, 0.077120640000, 0.000000000000) },
        //    { 657, (0.195923200000, 0.072825520000, 0.000000000000) },
        //    { 658, (0.185170800000, 0.068710080000, 0.000000000000) },
        //    { 659, (0.174832300000, 0.064769760000, 0.000000000000) },
        //    { 660, (0.164900000000, 0.061000000000, 0.000000000000) },
        //    { 661, (0.155366700000, 0.057396210000, 0.000000000000) },
        //    { 662, (0.146230000000, 0.053955040000, 0.000000000000) },
        //    { 663, (0.137490000000, 0.050673760000, 0.000000000000) },
        //    { 664, (0.129146700000, 0.047549650000, 0.000000000000) },
        //    { 665, (0.121200000000, 0.044580000000, 0.000000000000) },
        //    { 666, (0.113639700000, 0.041758720000, 0.000000000000) },
        //    { 667, (0.106465000000, 0.039084960000, 0.000000000000) },
        //    { 668, (0.099690440000, 0.036563840000, 0.000000000000) },
        //    { 669, (0.093330610000, 0.034200480000, 0.000000000000) },
        //    { 670, (0.087400000000, 0.032000000000, 0.000000000000) },
        //    { 671, (0.081900960000, 0.029962610000, 0.000000000000) },
        //    { 672, (0.076804280000, 0.028076640000, 0.000000000000) },
        //    { 673, (0.072077120000, 0.026329360000, 0.000000000000) },
        //    { 674, (0.067686640000, 0.024708050000, 0.000000000000) },
        //    { 675, (0.063600000000, 0.023200000000, 0.000000000000) },
        //    { 676, (0.059806850000, 0.021800770000, 0.000000000000) },
        //    { 677, (0.056282160000, 0.020501120000, 0.000000000000) },
        //    { 678, (0.052971040000, 0.019281080000, 0.000000000000) },
        //    { 679, (0.049818610000, 0.018120690000, 0.000000000000) },
        //    { 680, (0.046770000000, 0.017000000000, 0.000000000000) },
        //    { 681, (0.043784050000, 0.015903790000, 0.000000000000) },
        //    { 682, (0.040875360000, 0.014837180000, 0.000000000000) },
        //    { 683, (0.038072640000, 0.013810680000, 0.000000000000) },
        //    { 684, (0.035404610000, 0.012834780000, 0.000000000000) },
        //    { 685, (0.032900000000, 0.011920000000, 0.000000000000) },
        //    { 686, (0.030564190000, 0.011068310000, 0.000000000000) },
        //    { 687, (0.028380560000, 0.010273390000, 0.000000000000) },
        //    { 688, (0.026344840000, 0.009533311000, 0.000000000000) },
        //    { 689, (0.024452750000, 0.008846157000, 0.000000000000) },
        //    { 690, (0.022700000000, 0.008210000000, 0.000000000000) },
        //    { 691, (0.021084290000, 0.007623781000, 0.000000000000) },
        //    { 692, (0.019599880000, 0.007085424000, 0.000000000000) },
        //    { 693, (0.018237320000, 0.006591476000, 0.000000000000) },
        //    { 694, (0.016987170000, 0.006138485000, 0.000000000000) },
        //    { 695, (0.015840000000, 0.005723000000, 0.000000000000) },
        //    { 696, (0.014790640000, 0.005343059000, 0.000000000000) },
        //    { 697, (0.013831320000, 0.004995796000, 0.000000000000) },
        //    { 698, (0.012948680000, 0.004676404000, 0.000000000000) },
        //    { 699, (0.012129200000, 0.004380075000, 0.000000000000) },
        //    { 700, (0.011359160000, 0.004102000000, 0.000000000000) },
        //    { 701, (0.010629350000, 0.003838453000, 0.000000000000) },
        //    { 702, (0.009938846000, 0.003589099000, 0.000000000000) },
        //    { 703, (0.009288422000, 0.003354219000, 0.000000000000) },
        //    { 704, (0.008678854000, 0.003134093000, 0.000000000000) },
        //    { 705, (0.008110916000, 0.002929000000, 0.000000000000) },
        //    { 706, (0.007582388000, 0.002738139000, 0.000000000000) },
        //    { 707, (0.007088746000, 0.002559876000, 0.000000000000) },
        //    { 708, (0.006627313000, 0.002393244000, 0.000000000000) },
        //    { 709, (0.006195408000, 0.002237275000, 0.000000000000) },
        //    { 710, (0.005790346000, 0.002091000000, 0.000000000000) },
        //    { 711, (0.005409826000, 0.001953587000, 0.000000000000) },
        //    { 712, (0.005052583000, 0.001824580000, 0.000000000000) },
        //    { 713, (0.004717512000, 0.001703580000, 0.000000000000) },
        //    { 714, (0.004403507000, 0.001590187000, 0.000000000000) },
        //    { 715, (0.004109457000, 0.001484000000, 0.000000000000) },
        //    { 716, (0.003833913000, 0.001384496000, 0.000000000000) },
        //    { 717, (0.003575748000, 0.001291268000, 0.000000000000) },
        //    { 718, (0.003334342000, 0.001204092000, 0.000000000000) },
        //    { 719, (0.003109075000, 0.001122744000, 0.000000000000) },
        //    { 720, (0.002899327000, 0.001047000000, 0.000000000000) },
        //    { 721, (0.002704348000, 0.000976589600, 0.000000000000) },
        //    { 722, (0.002523020000, 0.000911108800, 0.000000000000) },
        //    { 723, (0.002354168000, 0.000850133200, 0.000000000000) },
        //    { 724, (0.002196616000, 0.000793238400, 0.000000000000) },
        //    { 725, (0.002049190000, 0.000740000000, 0.000000000000) },
        //    { 726, (0.001910960000, 0.000690082700, 0.000000000000) },
        //    { 727, (0.001781438000, 0.000643310000, 0.000000000000) },
        //    { 728, (0.001660110000, 0.000599496000, 0.000000000000) },
        //    { 729, (0.001546459000, 0.000558454700, 0.000000000000) },
        //    { 730, (0.001439971000, 0.000520000000, 0.000000000000) },
        //    { 731, (0.001340042000, 0.000483913600, 0.000000000000) },
        //    { 732, (0.001246275000, 0.000450052800, 0.000000000000) },
        //    { 733, (0.001158471000, 0.000418345200, 0.000000000000) },
        //    { 734, (0.001076430000, 0.000388718400, 0.000000000000) },
        //    { 735, (0.000999949300, 0.000361100000, 0.000000000000) },
        //    { 736, (0.000928735800, 0.000335383500, 0.000000000000) },
        //    { 737, (0.000862433200, 0.000311440400, 0.000000000000) },
        //    { 738, (0.000800750300, 0.000289165600, 0.000000000000) },
        //    { 739, (0.000743396000, 0.000268453900, 0.000000000000) },
        //    { 740, (0.000690078600, 0.000249200000, 0.000000000000) },
        //    { 741, (0.000640515600, 0.000231301900, 0.000000000000) },
        //    { 742, (0.000594502100, 0.000214685600, 0.000000000000) },
        //    { 743, (0.000551864600, 0.000199288400, 0.000000000000) },
        //    { 744, (0.000512429000, 0.000185047500, 0.000000000000) },
        //    { 745, (0.000476021300, 0.000171900000, 0.000000000000) },
        //    { 746, (0.000442453600, 0.000159778100, 0.000000000000) },
        //    { 747, (0.000411511700, 0.000148604400, 0.000000000000) },
        //    { 748, (0.000382981400, 0.000138301600, 0.000000000000) },
        //    { 749, (0.000356649100, 0.000128792500, 0.000000000000) },
        //    { 750, (0.000332301100, 0.000120000000, 0.000000000000) },
        //    { 751, (0.000309758600, 0.000111859500, 0.000000000000) },
        //    { 752, (0.000288887100, 0.000104322400, 0.000000000000) },
        //    { 753, (0.000269539400, 0.000097335600, 0.000000000000) },
        //    { 754, (0.000251568200, 0.000090845870, 0.000000000000) },
        //    { 755, (0.000234826100, 0.000084800000, 0.000000000000) },
        //    { 756, (0.000219171000, 0.000079146670, 0.000000000000) },
        //    { 757, (0.000204525800, 0.000073858000, 0.000000000000) },
        //    { 758, (0.000190840500, 0.000068916000, 0.000000000000) },
        //    { 759, (0.000178065400, 0.000064302670, 0.000000000000) },
        //    { 760, (0.000166150500, 0.000060000000, 0.000000000000) },
        //    { 761, (0.000155023600, 0.000055981870, 0.000000000000) },
        //    { 762, (0.000144621900, 0.000052225600, 0.000000000000) },
        //    { 763, (0.000134909800, 0.000048718400, 0.000000000000) },
        //    { 764, (0.000125852000, 0.000045447470, 0.000000000000) },
        //    { 765, (0.000117413000, 0.000042400000, 0.000000000000) },
        //    { 766, (0.000109551500, 0.000039561040, 0.000000000000) },
        //    { 767, (0.000102224500, 0.000036915120, 0.000000000000) },
        //    { 768, (0.000095394450, 0.000034448680, 0.000000000000) },
        //    { 769, (0.000089023900, 0.000032148160, 0.000000000000) },
        //    { 770, (0.000083075270, 0.000030000000, 0.000000000000) },
        //    { 771, (0.000077512690, 0.000027991250, 0.000000000000) },
        //    { 772, (0.000072313040, 0.000026113560, 0.000000000000) },
        //    { 773, (0.000067457780, 0.000024360240, 0.000000000000) },
        //    { 774, (0.000062928440, 0.000022724610, 0.000000000000) },
        //    { 775, (0.000058706520, 0.000021200000, 0.000000000000) },
        //    { 776, (0.000054770280, 0.000019778550, 0.000000000000) },
        //    { 777, (0.000051099180, 0.000018452850, 0.000000000000) },
        //    { 778, (0.000047676540, 0.000017216870, 0.000000000000) },
        //    { 779, (0.000044485670, 0.000016064590, 0.000000000000) },
        //    { 780, (0.000041509940, 0.000014990000, 0.000000000000) },
        //    { 781, (0.000038733240, 0.000013987280, 0.000000000000) },
        //    { 782, (0.000036142030, 0.000013051550, 0.000000000000) },
        //    { 783, (0.000033723520, 0.000012178180, 0.000000000000) },
        //    { 784, (0.000031464870, 0.000011362540, 0.000000000000) },
        //    { 785, (0.000029353260, 0.000010600000, 0.000000000000) },
        //    { 786, (0.000027375730, 0.000009885877, 0.000000000000) },
        //    { 787, (0.000025524330, 0.000009217304, 0.000000000000) },
        //    { 788, (0.000023793760, 0.000008592362, 0.000000000000) },
        //    { 789, (0.000022178700, 0.000008009133, 0.000000000000) },
        //    { 790, (0.000020673830, 0.000007465700, 0.000000000000) },
        //    { 791, (0.000019272260, 0.000006959567, 0.000000000000) },
        //    { 792, (0.000017966400, 0.000006487995, 0.000000000000) },
        //    { 793, (0.000016749910, 0.000006048699, 0.000000000000) },
        //    { 794, (0.000015616480, 0.000005639396, 0.000000000000) },
        //    { 795, (0.000014559770, 0.000005257800, 0.000000000000) },
        //    { 796, (0.000013573870, 0.000004901771, 0.000000000000) },
        //    { 797, (0.000012654360, 0.000004569720, 0.000000000000) },
        //    { 798, (0.000011797230, 0.000004260194, 0.000000000000) },
        //    { 799, (0.000010998440, 0.000003971739, 0.000000000000) },
        //    { 800, (0.000010253980, 0.000003702900, 0.000000000000) },
        //    { 801, (0.000009559646, 0.000003452163, 0.000000000000) },
        //    { 802, (0.000008912044, 0.000003218302, 0.000000000000) },
        //    { 803, (0.000008308358, 0.000003000300, 0.000000000000) },
        //    { 804, (0.000007745769, 0.000002797139, 0.000000000000) },
        //    { 805, (0.000007221456, 0.000002607800, 0.000000000000) },
        //    { 806, (0.000006732475, 0.000002431220, 0.000000000000) },
        //    { 807, (0.000006276423, 0.000002266531, 0.000000000000) },
        //    { 808, (0.000005851304, 0.000002113013, 0.000000000000) },
        //    { 809, (0.000005455118, 0.000001969943, 0.000000000000) },
        //    { 810, (0.000005085868, 0.000001836600, 0.000000000000) },
        //    { 811, (0.000004741466, 0.000001712230, 0.000000000000) },
        //    { 812, (0.000004420236, 0.000001596228, 0.000000000000) },
        //    { 813, (0.000004120783, 0.000001488090, 0.000000000000) },
        //    { 814, (0.000003841716, 0.000001387314, 0.000000000000) },
        //    { 815, (0.000003581652, 0.000001293400, 0.000000000000) },
        //    { 816, (0.000003339127, 0.000001205820, 0.000000000000) },
        //    { 817, (0.000003112949, 0.000001124143, 0.000000000000) },
        //    { 818, (0.000002902121, 0.000001048009, 0.000000000000) },
        //    { 819, (0.000002705645, 0.000000977058, 0.000000000000) },
        //    { 820, (0.000002522525, 0.000000910930, 0.000000000000) },
        //    { 821, (0.000002351726, 0.000000849251, 0.000000000000) },
        //    { 822, (0.000002192415, 0.000000791721, 0.000000000000) },
        //    { 823, (0.000002043902, 0.000000738090, 0.000000000000) },
        //    { 824, (0.000001905497, 0.000000688110, 0.000000000000) },
        //    { 825, (0.000001776509, 0.000000641530, 0.000000000000) },
        //    { 826, (0.000001656215, 0.000000598089, 0.000000000000) },
        //    { 827, (0.000001544022, 0.000000557575, 0.000000000000) },
        //    { 828, (0.000001439440, 0.000000519808, 0.000000000000) },
        //    { 829, (0.000001341977, 0.000000484612, 0.000000000000) },
        //    { 830, (0.000001251141, 0.000000451810, 0.000000000000) },
        //};

        //    //CIE 标准光源 D65功率分布
        //    private static readonly Dictionary<int, double> CIE_std_D65 = new Dictionary<int, double>
        //{
        //    { 300,000.03410}, { 301,000.36014}, { 302,000.68618}, { 303,001.01222},
        //    { 304,001.33826}, { 305,001.66430}, { 306,001.99034}, { 307,002.31638},
        //    { 308,002.64242}, { 309,002.96846}, { 310,003.29450}, { 311,004.98865},
        //    { 312,006.68280}, { 313,008.37695}, { 314,010.07110}, { 315,011.76520},
        //    { 316,013.45940}, { 317,015.15350}, { 318,016.84770}, { 319,018.54180},
        //    { 320,020.23600}, { 321,021.91770}, { 322,023.59950}, { 323,025.28120},
        //    { 324,026.96300}, { 325,028.64470}, { 326,030.32650}, { 327,032.00820},
        //    { 328,033.69000}, { 329,035.37170}, { 330,037.05350}, { 331,037.34300},
        //    { 332,037.63260}, { 333,037.92210}, { 334,038.21160}, { 335,038.50110},
        //    { 336,038.79070}, { 337,039.08020}, { 338,039.36970}, { 339,039.65930},
        //    { 340,039.94880}, { 341,040.44510}, { 342,040.94140}, { 343,041.43770},
        //    { 344,041.93400}, { 345,042.43020}, { 346,042.92650}, { 347,043.42280},
        //    { 348,043.91910}, { 349,044.41540}, { 350,044.91170}, { 351,045.08440},
        //    { 352,045.25700}, { 353,045.42970}, { 354,045.60230}, { 355,045.77500},
        //    { 356,045.94770}, { 357,046.12030}, { 358,046.29300}, { 359,046.46560},
        //    { 360,046.63830}, { 361,047.18340}, { 362,047.72850}, { 363,048.27350},
        //    { 364,048.81860}, { 365,049.36370}, { 366,049.90880}, { 367,050.45390},
        //    { 368,050.99890}, { 369,051.54400}, { 370,052.08910}, { 371,051.87770},
        //    { 372,051.66640}, { 373,051.45500}, { 374,051.24370}, { 375,051.03230},
        //    { 376,050.82090}, { 377,050.60960}, { 378,050.39820}, { 379,050.18690},
        //    { 380,049.97550}, { 381,050.44280}, { 382,050.91000}, { 383,051.37730},
        //    { 384,051.84460}, { 385,052.31180}, { 386,052.77910}, { 387,053.24640},
        //    { 388,053.71370}, { 389,054.18090}, { 390,054.64820}, { 391,057.45890},
        //    { 392,060.26950}, { 393,063.08020}, { 394,065.89090}, { 395,068.70150},
        //    { 396,071.51220}, { 397,074.32290}, { 398,077.13360}, { 399,079.94420},
        //    { 400,082.75490}, { 401,083.62800}, { 402,084.50110}, { 403,085.37420},
        //    { 404,086.24730}, { 405,087.12040}, { 406,087.99360}, { 407,088.86670},
        //    { 408,089.73980}, { 409,090.61290}, { 410,091.48600}, { 411,091.68060},
        //    { 412,091.87520}, { 413,092.06970}, { 414,092.26430}, { 415,092.45890},
        //    { 416,092.65350}, { 417,092.84810}, { 418,093.04260}, { 419,093.23720},
        //    { 420,093.43180}, { 421,092.75680}, { 422,092.08190}, { 423,091.40690},
        //    { 424,090.73200}, { 425,090.05700}, { 426,089.38210}, { 427,088.70710},
        //    { 428,088.03220}, { 429,087.35720}, { 430,086.68230}, { 431,088.50060},
        //    { 432,090.31880}, { 433,092.13710}, { 434,093.95540}, { 435,095.77360},
        //    { 436,097.59190}, { 437,099.41020}, { 438,101.22800}, { 439,103.04700},
        //    { 440,104.86500}, { 441,106.07900}, { 442,107.29400}, { 443,108.50800},
        //    { 444,109.72200}, { 445,110.93600}, { 446,112.15100}, { 447,113.36500},
        //    { 448,114.57900}, { 449,115.79400}, { 450,117.00800}, { 451,117.08800},
        //    { 452,117.16900}, { 453,117.24900}, { 454,117.33000}, { 455,117.41000},
        //    { 456,117.49000}, { 457,117.57100}, { 458,117.65100}, { 459,117.73200},
        //    { 460,117.81200}, { 461,117.51700}, { 462,117.22200}, { 463,116.92700},
        //    { 464,116.63200}, { 465,116.33600}, { 466,116.04100}, { 467,115.74600},
        //    { 468,115.45100}, { 469,115.15600}, { 470,114.86100}, { 471,114.96700},
        //    { 472,115.07300}, { 473,115.18000}, { 474,115.28600}, { 475,115.39200},
        //    { 476,115.49800}, { 477,115.60400}, { 478,115.71100}, { 479,115.81700},
        //    { 480,115.92300}, { 481,115.21200}, { 482,114.50100}, { 483,113.78900},
        //    { 484,113.07800}, { 485,112.36700}, { 486,111.65600}, { 487,110.94500},
        //    { 488,110.23300}, { 489,109.52200}, { 490,108.81100}, { 491,108.86500},
        //    { 492,108.92000}, { 493,108.97400}, { 494,109.02800}, { 495,109.08200},
        //    { 496,109.13700}, { 497,109.19100}, { 498,109.24500}, { 499,109.30000},
        //    { 500,109.35400}, { 501,109.19900}, { 502,109.04400}, { 503,108.88800},
        //    { 504,108.73300}, { 505,108.57800}, { 506,108.42300}, { 507,108.26800},
        //    { 508,108.11200}, { 509,107.95700}, { 510,107.80200}, { 511,107.50100},
        //    { 512,107.20000}, { 513,106.89800}, { 514,106.59700}, { 515,106.29600},
        //    { 516,105.99500}, { 517,105.69400}, { 518,105.39200}, { 519,105.09100},
        //    { 520,104.79000}, { 521,105.08000}, { 522,105.37000}, { 523,105.66000},
        //    { 524,105.95000}, { 525,106.23900}, { 526,106.52900}, { 527,106.81900},
        //    { 528,107.10900}, { 529,107.39900}, { 530,107.68900}, { 531,107.36100},
        //    { 532,107.03200}, { 533,106.70400}, { 534,106.37500}, { 535,106.04700},
        //    { 536,105.71900}, { 537,105.39000}, { 538,105.06200}, { 539,104.73300},
        //    { 540,104.40500}, { 541,104.36900}, { 542,104.33300}, { 543,104.29700},
        //    { 544,104.26100}, { 545,104.22500}, { 546,104.19000}, { 547,104.15400},
        //    { 548,104.11800}, { 549,104.08200}, { 550,104.04600}, { 551,103.64100},
        //    { 552,103.23700}, { 553,102.83200}, { 554,102.42800}, { 555,102.02300},
        //    { 556,101.61800}, { 557,101.21400}, { 558,100.80900}, { 559,100.40500},
        //    { 560,100.00000}, { 561,099.63340}, { 562,099.26680}, { 563,098.90030},
        //    { 564,098.53370}, { 565,098.16710}, { 566,097.80050}, { 567,097.43390},
        //    { 568,097.06740}, { 569,096.70080}, { 570,096.33420}, { 571,096.27960},
        //    { 572,096.22500}, { 573,096.17030}, { 574,096.11570}, { 575,096.06110},
        //    { 576,096.00650}, { 577,095.95190}, { 578,095.89720}, { 579,095.84260},
        //    { 580,095.78800}, { 581,095.07780}, { 582,094.36750}, { 583,093.65730},
        //    { 584,092.94700}, { 585,092.23680}, { 586,091.52660}, { 587,090.81630},
        //    { 588,090.10610}, { 589,089.39580}, { 590,088.68560}, { 591,088.81770},
        //    { 592,088.94970}, { 593,089.08180}, { 594,089.21380}, { 595,089.34590},
        //    { 596,089.47800}, { 597,089.61000}, { 598,089.74210}, { 599,089.87410},
        //    { 600,090.00620}, { 601,089.96550}, { 602,089.92480}, { 603,089.88410},
        //    { 604,089.84340}, { 605,089.80260}, { 606,089.76190}, { 607,089.72120},
        //    { 608,089.68050}, { 609,089.63980}, { 610,089.59910}, { 611,089.40910},
        //    { 612,089.21900}, { 613,089.02900}, { 614,088.83890}, { 615,088.64890},
        //    { 616,088.45890}, { 617,088.26880}, { 618,088.07880}, { 619,087.88870},
        //    { 620,087.69870}, { 621,087.25770}, { 622,086.81670}, { 623,086.37570},
        //    { 624,085.93470}, { 625,085.49360}, { 626,085.05260}, { 627,084.61160},
        //    { 628,084.17060}, { 629,083.72960}, { 630,083.28860}, { 631,083.32970},
        //    { 632,083.37070}, { 633,083.41180}, { 634,083.45280}, { 635,083.49390},
        //    { 636,083.53500}, { 637,083.57600}, { 638,083.61710}, { 639,083.65810},
        //    { 640,083.69920}, { 641,083.33200}, { 642,082.96470}, { 643,082.59750},
        //    { 644,082.23020}, { 645,081.86300}, { 646,081.49580}, { 647,081.12850},
        //    { 648,080.76130}, { 649,080.39400}, { 650,080.02680}, { 651,080.04560},
        //    { 652,080.06440}, { 653,080.08310}, { 654,080.10190}, { 655,080.12070},
        //    { 656,080.13950}, { 657,080.15830}, { 658,080.17700}, { 659,080.19580},
        //    { 660,080.21460}, { 661,080.42090}, { 662,080.62720}, { 663,080.83360},
        //    { 664,081.03990}, { 665,081.24620}, { 666,081.45250}, { 667,081.65880},
        //    { 668,081.86520}, { 669,082.07150}, { 670,082.27780}, { 671,081.87840},
        //    { 672,081.47910}, { 673,081.07970}, { 674,080.68040}, { 675,080.28100},
        //    { 676,079.88160}, { 677,079.48230}, { 678,079.08290}, { 679,078.68360},
        //    { 680,078.28420}, { 681,077.42790}, { 682,076.57160}, { 683,075.71530},
        //    { 684,074.85900}, { 685,074.00270}, { 686,073.14650}, { 687,072.29020},
        //    { 688,071.43390}, { 689,070.57760}, { 690,069.72130}, { 691,069.91010},
        //    { 692,070.09890}, { 693,070.28760}, { 694,070.47640}, { 695,070.66520},
        //    { 696,070.85400}, { 697,071.04280}, { 698,071.23150}, { 699,071.42030},
        //    { 700,071.60910}, { 701,071.88310}, { 702,072.15710}, { 703,072.43110},
        //    { 704,072.70510}, { 705,072.97900}, { 706,073.25300}, { 707,073.52700},
        //    { 708,073.80100}, { 709,074.07500}, { 710,074.34900}, { 711,073.07450},
        //    { 712,071.80000}, { 713,070.52550}, { 714,069.25100}, { 715,067.97650},
        //    { 716,066.70200}, { 717,065.42750}, { 718,064.15300}, { 719,062.87850},
        //    { 720,061.60400}, { 721,062.43220}, { 722,063.26030}, { 723,064.08850},
        //    { 724,064.91660}, { 725,065.74480}, { 726,066.57300}, { 727,067.40110},
        //    { 728,068.22930}, { 729,069.05740}, { 730,069.88560}, { 731,070.40570},
        //    { 732,070.92590}, { 733,071.44600}, { 734,071.96620}, { 735,072.48630},
        //    { 736,073.00640}, { 737,073.52660}, { 738,074.04670}, { 739,074.56690},
        //    { 740,075.08700}, { 741,073.93760}, { 742,072.78810}, { 743,071.63870},
        //    { 744,070.48930}, { 745,069.33980}, { 746,068.19040}, { 747,067.04100},
        //    { 748,065.89160}, { 749,064.74210}, { 750,063.59270}, { 751,061.87520},
        //    { 752,060.15780}, { 753,058.44030}, { 754,056.72290}, { 755,055.00540},
        //    { 756,053.28800}, { 757,051.57050}, { 758,049.85310}, { 759,048.13560},
        //    { 760,046.41820}, { 761,048.45690}, { 762,050.49560}, { 763,052.53440},
        //    { 764,054.57310}, { 765,056.61180}, { 766,058.65050}, { 767,060.68920},
        //    { 768,062.72800}, { 769,064.76670}, { 770,066.80540}, { 771,066.46310},
        //    { 772,066.12090}, { 773,065.77860}, { 774,065.43640}, { 775,065.09410},
        //    { 776,064.75180}, { 777,064.40960}, { 778,064.06730}, { 779,063.72510},
        //    { 780,063.38280}, { 781,063.47490}, { 782,063.56700}, { 783,063.65920},
        //    { 784,063.75130}, { 785,063.84340}, { 786,063.93550}, { 787,064.02760},
        //    { 788,064.11980}, { 789,064.21190}, { 790,064.30400}, { 791,063.81880},
        //    { 792,063.33360}, { 793,062.84840}, { 794,062.36320}, { 795,061.87790},
        //    { 796,061.39270}, { 797,060.90750}, { 798,060.42230}, { 799,059.93710},
        //    { 800,059.45190}, { 801,058.70260}, { 802,057.95330}, { 803,057.20400},
        //    { 804,056.45470}, { 805,055.70540}, { 806,054.95620}, { 807,054.20690},
        //    { 808,053.45760}, { 809,052.70830}, { 810,051.95900}, { 811,052.50720},
        //    { 812,053.05530}, { 813,053.60350}, { 814,054.15160}, { 815,054.69980},
        //    { 816,055.24800}, { 817,055.79610}, { 818,056.34430}, { 819,056.89240},
        //    { 820,057.44060}, { 821,057.72780}, { 822,058.01500}, { 823,058.30220},
        //    { 824,058.58940}, { 825,058.87650}, { 826,059.16370}, { 827,059.45090},
        //    { 828,059.73810}, { 829,060.02530}, { 830,060.31250},
        //};
        //    #endregion

        //    #region 计算LAB
        //    /// <summary>
        //    /// 计算LAB值
        //    /// </summary>
        //    /// <param name="absorbanceData">吸光度数组</param>
        //    /// <param name="startWavelength">起始波长</param>
        //    /// <param name="endWavelength">结束波长</param>
        //    /// <param name="interval">波长间隔</param>
        //    /// <param name="angle">观察者角度</param>
        //    /// <returns>LAB值</returns>
        //    public static (double L, double A, double B) CalculateLAB(double[] absorbanceData, int startWavelength, int endWavelength, int interval, int angle)
        //    {
        //        var cieData = angle == 10 ? CIE_1964_10 : CIE_1931_2;

        //        // 假设的参考白色标准反射率数据
        //        double[] referenceWhite = new double[absorbanceData.Length];
        //        for (int i = 0; i < referenceWhite.Length; i++)
        //        {
        //            referenceWhite[i] = 1.0; // 假设完全反射
        //        }

        //        // 转换吸光度为反射率
        //        double[] reflectance = new double[absorbanceData.Length];
        //        for (int i = 0; i < absorbanceData.Length; i++)
        //        {
        //            reflectance[i] = Math.Pow(10, -absorbanceData[i]);
        //        }

        //        // 获取 CIE 色度匹配函数的 Y 值集合，并确保波长匹配
        //        var cieYValues = cieData.Where(kvp => CIE_std_D65.ContainsKey(kvp.Key))
        //                                .Select(kvp => kvp.Value.Y)
        //                                .ToList();

        //        // 获取匹配波长的 D65 标准光源光谱功率分布值集合
        //        var d65Values = cieData.Keys.Where(wavelength => CIE_std_D65.ContainsKey(wavelength))
        //                                    .Select(wavelength => CIE_std_D65[wavelength])
        //                                    .ToList();

        //        Console.WriteLine("D65 Values: " + string.Join(", ", d65Values));

        //        //  将两个集合按顺序配对
        //        var pairedValues = cieYValues.Zip(d65Values, (cieY, d65) => new { cieY, d65 }).ToList();

        //        //  计算每对元素的乘积
        //        var products = pairedValues.Select(p => p.cieY * p.d65).ToList();

        //        // 对所有乘积求和
        //        var sum = products.Sum();


        //        // 计算 k 值
        //        double k = (100.0 / sum) * interval;

        //        // 计算XYZ值
        //        double X = 0, Y = 0, Z = 0;

        //        for (int i = 0; i < reflectance.Length; i++)
        //        {
        //            int wavelength = startWavelength + i * interval;
        //            if (cieData.ContainsKey(wavelength) && CIE_std_D65.ContainsKey(wavelength))
        //            {
        //                var (x, y, z) = cieData[wavelength];
        //                double stdD65 = CIE_std_D65[wavelength];

        //                X += reflectance[i] * x * stdD65;
        //                Y += reflectance[i] * y * stdD65;
        //                Z += reflectance[i] * z * stdD65;
        //            }
        //        }
        //        X *= k;
        //        Y *= k;
        //        Z *= k;
        //        // 转换XYZ值为LAB值
        //        double Xn = 95.047, Yn = 100.000, Zn = 108.883; // D65标准光源下的参考白色

        //        if (angle == 10)
        //        {
        //            Xn = 94.81;
        //            Yn = 100.000;
        //            Zn = 107.31;
        //        }
        //        else
        //        {
        //            Xn = 95.047;
        //            Yn = 100.000;
        //            Zn = 108.883;
        //        }
        //        double fx = f(X / Xn);
        //        double fy = f(Y / Yn);
        //        double fz = f(Z / Zn);

        //        double L = 116 * fy - 16;
        //        double A = 500 * (fx - fy);
        //        double B = 200 * (fy - fz);

        //        return (L, A, B);
        //    }

        //    private static double f(double t)
        //    {
        //        return t > Math.Pow(6.0 / 29.0, 3) ? Math.Pow(t, 1.0 / 3.0) : (1.0 / 3.0) * Math.Pow(29.0 / 6.0, 2) * t + 4.0 / 29.0;
        //    }

        //    #endregion

        //    #region 计算DECMC
        //    public static double CalculateCMC(double LStarTarget, double aStarTarget, double bStarTarget,
        //                                      double LStarSample, double aStarSample, double bStarSample,
        //                                      double lFactor, double cFactor)
        //    {
        //        // 计算差值  
        //        double DL = LStarSample - LStarTarget;
        //        double Da = aStarSample - aStarTarget;
        //        double Db = bStarSample - bStarTarget;

        //        // 计算C*ab  
        //        double CStarAbTarget = Math.Sqrt(Math.Pow(aStarTarget, 2) + Math.Pow(bStarTarget, 2));
        //        double CStarAbSample = Math.Sqrt(Math.Pow(aStarSample, 2) + Math.Pow(bStarSample, 2));
        //        double DCab = CStarAbSample - CStarAbTarget;

        //        //计算hab
        //        double HabT = CalculateHAB(aStarTarget, bStarTarget);
        //        double HabS = CalculateHAB(aStarSample, bStarSample);
        //        double DHab = ((HabS - HabT) <= 180 ? CalculateDHAB(Da, Db, DCab) : -CalculateDHAB(Da, Db, DCab));

        //        // 计算SL, SC, SH, F, T  
        //        double SL = CalculateSL(LStarTarget);
        //        double SC = CalculateSC(CStarAbTarget);
        //        double T = CalculateT(HabT);
        //        double F = CalculateF(CStarAbTarget);
        //        double SH = SC * (1 + T * F - F);
        //        double FValue = CalculateF(CStarAbTarget);

        //        // 计算DECMC  DEcmc = [(DL*/lSL)2 + (DC*ab/cSC)2 + (DH*ab/SH)2]1/2
        //        double DECMC = Math.Sqrt(Math.Pow(DL / (lFactor * SL), 2) +
        //                                 Math.Pow(DCab / (cFactor * SC), 2) +
        //                                 Math.Pow(DHab / SH, 2));

        //        return DECMC;
        //    }
        //    private static double CalculateSL(double LStar)
        //    {
        //        if (LStar < 16)
        //        {
        //            return 0.511;
        //        }
        //        return 0.040975 * LStar / (1 + 0.01765 * LStar);
        //    }



        //    /// <summary>  
        //    /// 根据A和B的值计算角度。  
        //    /// </summary>  
        //    /// <param name="A">A</param>  
        //    /// <param name="B">B</param>  
        //    /// <returns>计算得到的角度（度）</returns>  
        //    private static double CalculateHAB(double A, double B)
        //    {
        //        if (A == 0 && B == 0)
        //        {
        //            return 0;
        //        }
        //        else if (A == 0 && B > 0)
        //        {
        //            return 90;
        //        }
        //        else if (A == 0 && B < 0)
        //        {
        //            return 270;
        //        }
        //        //else if (aStarTarget > 0 && bStarTarget >= 0)
        //        //{
        //        //    return (360 * Math.Atan2(bStarTarget, aStarTarget) / Math.PI / 2);
        //        //}
        //        //else if(aStarTarget < 0)
        //        //{
        //        //    return (180+360 * Math.Atan2(bStarTarget, aStarTarget) / Math.PI / 2);
        //        //}
        //        else
        //        {
        //            // 处理标样A不为0的情况  
        //            double angleRadians = Math.Atan2(B, A); // 使用Atan2来处理所有象限  
        //            double angleDegrees = angleRadians * (180 / Math.PI); // 将弧度转换为度  

        //            // 如果B5是负数，我们不需要额外加180或360，因为Atan2已经处理了象限  
        //            // 但如果需要特定的输出范围（例如0-360度），我们可以添加适当的调整  
        //            // 例如，确保角度在0-360度之间  
        //            if (angleDegrees < 0)
        //            {
        //                angleDegrees += 360;
        //            }

        //            return angleDegrees;
        //        }
        //    }

        //    private static double CalculateDHAB(double DA, double DB, double DCab)
        //    {
        //        double DHAB = Math.Sqrt(Math.Pow(DA, 2) + Math.Pow(DB, 2) - Math.Pow(DCab, 2));
        //        return DHAB;
        //    }

        //    private static double CalculateT(double HabT)
        //    {
        //        double value = 0;
        //        if (HabT > 164 && HabT < 345)
        //        {
        //            value = 0.56 + Math.Abs(0.2 * Math.Cos(Math.PI / (180 / (168 + HabT))));

        //        }
        //        else
        //        {
        //            value = 0.36 + Math.Abs(0.4 * Math.Cos(Math.PI / (180 / (35 + HabT))));
        //        }
        //        return value;
        //    }

        //    private static double CalculateSC(double CStarAb)
        //    {
        //        return 0.0638 * CStarAb / (1 + 0.0131 * CStarAb) + 0.638;
        //    }

        //    private static double CalculateF(double CStarAb)
        //    {
        //        double d0 = Math.Pow(CStarAb, 4);
        //        double d1 = Math.Pow(CStarAb, 4) + 1900;
        //        double d = Math.Sqrt(d0 / d1);
        //        return d;
        //    }

        //    #endregion

        //    #region 试样相对于标样的色度力份/强度/着色力
        //    // 计算 K/S 值
        //    private static double CalculateKS(double reflectance)
        //    {
        //        return Math.Round(Math.Pow(1 - reflectance, 2) / (reflectance), 6);
        //    }

        //    // 计算色差强度（百分比形式）
        //    public static double CalculateColorStrength(double[] sampleAbsorbance, double[] standardAbsorbance)
        //    {
        //        if (sampleAbsorbance.Length != standardAbsorbance.Length)
        //        {
        //            throw new ArgumentException("样品和标准的吸光度数据长度必须相同");
        //        }

        //        // 找到最大吸收波长的数据
        //        int maxAbsorbanceIndexSample = Array.IndexOf(sampleAbsorbance, sampleAbsorbance.Max());
        //        int maxAbsorbanceIndexStandard = Array.IndexOf(standardAbsorbance, standardAbsorbance.Max());

        //        // 将吸光度转化为反射率
        //        double sampleReflectance = Math.Round(Math.Pow(10, -sampleAbsorbance[maxAbsorbanceIndexStandard]), 6);
        //        double standardReflectance = Math.Round(Math.Pow(10, -standardAbsorbance[maxAbsorbanceIndexStandard]), 6);

        //        // 计算色差强度
        //        double ksSample = Math.Round(CalculateKS(sampleReflectance), 6);
        //        double ksStandard = Math.Round(CalculateKS(standardReflectance), 6);

        //        return Math.Round((ksSample / ksStandard) * 100, 6);
        //    }

        //    // 计算色差强度（百分比形式）
        //    /// <summary>
        //    /// 计算色差强度
        //    /// </summary>
        //    /// <param name="sampleAbsorbance">试样的吸光度数据</param>
        //    /// <param name="standardAbsorbance">标样的吸光度数据</param>
        //    /// <returns>色差强度（百分比形式）</returns>
        //    public static double KS(double[] sampleAbsorbance, double[] standardAbsorbance)
        //    {
        //        if (sampleAbsorbance.Length != standardAbsorbance.Length)
        //        {
        //            throw new ArgumentException("样品和标准的吸光度数据长度必须相同");
        //        }

        //        double totalStrength = 0;
        //        double totalStandard = 0;
        //        int length = sampleAbsorbance.Length;

        //        for (int i = 0; i < length; i++)
        //        {
        //            // 将吸光度转化为反射率
        //            double sampleReflectance = Math.Pow(10, -sampleAbsorbance[i]);
        //            double standardReflectance = Math.Pow(10, -standardAbsorbance[i]);

        //            // 计算 K/S 值
        //            double ksSample = CalculateKS(sampleReflectance);
        //            double ksStandard = CalculateKS(standardReflectance);

        //            // 计算并累加色差强度
        //            totalStrength += ksSample;
        //            totalStandard += ksStandard;
        //        }



        //        // 返回平均色差强度
        //        return (totalStrength / totalStandard) * 100;
        //    }

        //    #endregion


        #region 标准观察者色度匹配函数 
        // CIE 1964 10°标准观察者色度匹配函数
        private static readonly Dictionary<int, (double X, double Y, double Z)> CIE_1964_10 = new Dictionary<int, (double X, double Y, double Z)>
    {

        { 360, (0.000000122200, 0.000000013398, 0.000000535027) },
        { 361, (0.000000185138, 0.000000020294, 0.000000810720) },
        { 362, (0.000000278830, 0.000000030560, 0.000001221200) },
        { 363, (0.000000417470, 0.000000045740, 0.000001828700) },
        { 364, (0.000000621330, 0.000000068050, 0.000002722200) },
        { 365, (0.000000919270, 0.000000100650, 0.000004028300) },
        { 366, (0.000001351980, 0.000000147980, 0.000005925700) },
        { 367, (0.000001976540, 0.000000216270, 0.000008665100) },
        { 368, (0.000002872500, 0.000000314200, 0.000012596000) },
        { 369, (0.000004149500, 0.000000453700, 0.000018201000) },
        { 370, (0.000005958600, 0.000000651100, 0.000026143700) },
        { 371, (0.000008505600, 0.000000928800, 0.000037330000) },
        { 372, (0.000012068600, 0.000001317500, 0.000052987000) },
        { 373, (0.000017022600, 0.000001857200, 0.000074764000) },
        { 374, (0.000023868000, 0.000002602000, 0.000104870000) },
        { 375, (0.000033266000, 0.000003625000, 0.000146220000) },
        { 376, (0.000046087000, 0.000005019000, 0.000202660000) },
        { 377, (0.000063472000, 0.000006907000, 0.000279230000) },
        { 378, (0.000086892000, 0.000009449000, 0.000382450000) },
        { 379, (0.000118246000, 0.000012848000, 0.000520720000) },
        { 380, (0.000159952000, 0.000017364000, 0.000704776000) },
        { 381, (0.000215080000, 0.000023327000, 0.000948230000) },
        { 382, (0.000287490000, 0.000031150000, 0.001268200000) },
        { 383, (0.000381990000, 0.000041350000, 0.001686100000) },
        { 384, (0.000504550000, 0.000054560000, 0.002228500000) },
        { 385, (0.000662440000, 0.000071560000, 0.002927800000) },
        { 386, (0.000864500000, 0.000093300000, 0.003823700000) },
        { 387, (0.001121500000, 0.000120870000, 0.004964200000) },
        { 388, (0.001446160000, 0.000155640000, 0.006406700000) },
        { 389, (0.001853590000, 0.000199200000, 0.008219300000) },
        { 390, (0.002361600000, 0.000253400000, 0.010482200000) },
        { 391, (0.002990600000, 0.000320200000, 0.013289000000) },
        { 392, (0.003764500000, 0.000402400000, 0.016747000000) },
        { 393, (0.004710200000, 0.000502300000, 0.020980000000) },
        { 394, (0.005858100000, 0.000623200000, 0.026127000000) },
        { 395, (0.007242300000, 0.000768500000, 0.032344000000) },
        { 396, (0.008899600000, 0.000941700000, 0.039802000000) },
        { 397, (0.010870900000, 0.001147800000, 0.048691000000) },
        { 398, (0.013198900000, 0.001390300000, 0.059210000000) },
        { 399, (0.015929200000, 0.001674000000, 0.071576000000) },
        { 400, (0.019109700000, 0.002004400000, 0.086010900000) },
        { 401, (0.022788000000, 0.002386000000, 0.102740000000) },
        { 402, (0.027011000000, 0.002822000000, 0.122000000000) },
        { 403, (0.031829000000, 0.003319000000, 0.144020000000) },
        { 404, (0.037278000000, 0.003880000000, 0.168990000000) },
        { 405, (0.043400000000, 0.004509000000, 0.197120000000) },
        { 406, (0.050223000000, 0.005209000000, 0.228570000000) },
        { 407, (0.057764000000, 0.005985000000, 0.263470000000) },
        { 408, (0.066038000000, 0.006833000000, 0.301900000000) },
        { 409, (0.075033000000, 0.007757000000, 0.343870000000) },
        { 410, (0.084736000000, 0.008756000000, 0.389366000000) },
        { 411, (0.095041000000, 0.009816000000, 0.437970000000) },
        { 412, (0.105836000000, 0.010918000000, 0.489220000000) },
        { 413, (0.117066000000, 0.012058000000, 0.542900000000) },
        { 414, (0.128682000000, 0.013237000000, 0.598810000000) },
        { 415, (0.140638000000, 0.014456000000, 0.656760000000) },
        { 416, (0.152893000000, 0.015717000000, 0.716580000000) },
        { 417, (0.165416000000, 0.017025000000, 0.778120000000) },
        { 418, (0.178191000000, 0.018399000000, 0.841310000000) },
        { 419, (0.191214000000, 0.019848000000, 0.906110000000) },
        { 420, (0.204492000000, 0.021391000000, 0.972542000000) },
        { 421, (0.217650000000, 0.022992000000, 1.038900000000) },
        { 422, (0.230267000000, 0.024598000000, 1.103100000000) },
        { 423, (0.242311000000, 0.026213000000, 1.165100000000) },
        { 424, (0.253793000000, 0.027841000000, 1.224900000000) },
        { 425, (0.264737000000, 0.029497000000, 1.282500000000) },
        { 426, (0.275195000000, 0.031195000000, 1.338200000000) },
        { 427, (0.285301000000, 0.032927000000, 1.392600000000) },
        { 428, (0.295143000000, 0.034738000000, 1.446100000000) },
        { 429, (0.304869000000, 0.036654000000, 1.499400000000) },
        { 430, (0.314679000000, 0.038676000000, 1.553480000000) },
        { 431, (0.324355000000, 0.040792000000, 1.607200000000) },
        { 432, (0.333570000000, 0.042946000000, 1.658900000000) },
        { 433, (0.342243000000, 0.045114000000, 1.708200000000) },
        { 434, (0.350312000000, 0.047333000000, 1.754800000000) },
        { 435, (0.357719000000, 0.049602000000, 1.798500000000) },
        { 436, (0.364482000000, 0.051934000000, 1.839200000000) },
        { 437, (0.370493000000, 0.054337000000, 1.876600000000) },
        { 438, (0.375727000000, 0.056822000000, 1.910500000000) },
        { 439, (0.380158000000, 0.059399000000, 1.940800000000) },
        { 440, (0.383734000000, 0.062077000000, 1.967280000000) },
        { 441, (0.386327000000, 0.064737000000, 1.989100000000) },
        { 442, (0.387858000000, 0.067285000000, 2.005700000000) },
        { 443, (0.388396000000, 0.069764000000, 2.017400000000) },
        { 444, (0.387978000000, 0.072218000000, 2.024400000000) },
        { 445, (0.386726000000, 0.074704000000, 2.027300000000) },
        { 446, (0.384696000000, 0.077272000000, 2.026400000000) },
        { 447, (0.382006000000, 0.079979000000, 2.022300000000) },
        { 448, (0.378709000000, 0.082874000000, 2.015300000000) },
        { 449, (0.374915000000, 0.086000000000, 2.006000000000) },
        { 450, (0.370702000000, 0.089456000000, 1.994800000000) },
        { 451, (0.366089000000, 0.092947000000, 1.981400000000) },
        { 452, (0.361045000000, 0.096275000000, 1.965300000000) },
        { 453, (0.355518000000, 0.099535000000, 1.946400000000) },
        { 454, (0.349486000000, 0.102829000000, 1.924800000000) },
        { 455, (0.342957000000, 0.106256000000, 1.900700000000) },
        { 456, (0.335893000000, 0.109901000000, 1.874100000000) },
        { 457, (0.328284000000, 0.113835000000, 1.845100000000) },
        { 458, (0.320150000000, 0.118167000000, 1.813900000000) },
        { 459, (0.311475000000, 0.122932000000, 1.780600000000) },
        { 460, (0.302273000000, 0.128201000000, 1.745370000000) },
        { 461, (0.292858000000, 0.133457000000, 1.709100000000) },
        { 462, (0.283502000000, 0.138323000000, 1.672300000000) },
        { 463, (0.274044000000, 0.143042000000, 1.634700000000) },
        { 464, (0.264263000000, 0.147787000000, 1.595600000000) },
        { 465, (0.254085000000, 0.152761000000, 1.554900000000) },
        { 466, (0.243392000000, 0.158102000000, 1.512200000000) },
        { 467, (0.232187000000, 0.163941000000, 1.467300000000) },
        { 468, (0.220488000000, 0.170362000000, 1.419900000000) },
        { 469, (0.208198000000, 0.177425000000, 1.370000000000) },
        { 470, (0.195618000000, 0.185190000000, 1.317560000000) },
        { 471, (0.183034000000, 0.193025000000, 1.262400000000) },
        { 472, (0.170222000000, 0.200313000000, 1.205000000000) },
        { 473, (0.157348000000, 0.207156000000, 1.146600000000) },
        { 474, (0.144650000000, 0.213644000000, 1.088000000000) },
        { 475, (0.132349000000, 0.219940000000, 1.030200000000) },
        { 476, (0.120584000000, 0.226170000000, 0.973830000000) },
        { 477, (0.109456000000, 0.232467000000, 0.919430000000) },
        { 478, (0.099042000000, 0.239025000000, 0.867460000000) },
        { 479, (0.089388000000, 0.245997000000, 0.818280000000) },
        { 480, (0.080507000000, 0.253589000000, 0.772125000000) },
        { 481, (0.072034000000, 0.261876000000, 0.728290000000) },
        { 482, (0.063710000000, 0.270643000000, 0.686040000000) },
        { 483, (0.055694000000, 0.279645000000, 0.645530000000) },
        { 484, (0.048117000000, 0.288694000000, 0.606850000000) },
        { 485, (0.041072000000, 0.297665000000, 0.570060000000) },
        { 486, (0.034642000000, 0.306469000000, 0.535220000000) },
        { 487, (0.028896000000, 0.315035000000, 0.502340000000) },
        { 488, (0.023876000000, 0.323335000000, 0.471400000000) },
        { 489, (0.019628000000, 0.331366000000, 0.442390000000) },
        { 490, (0.016172000000, 0.339133000000, 0.415254000000) },
        { 491, (0.013300000000, 0.347860000000, 0.390024000000) },
        { 492, (0.010759000000, 0.358326000000, 0.366399000000) },
        { 493, (0.008542000000, 0.370001000000, 0.344015000000) },
        { 494, (0.006661000000, 0.382464000000, 0.322689000000) },
        { 495, (0.005132000000, 0.395379000000, 0.302356000000) },
        { 496, (0.003982000000, 0.408482000000, 0.283036000000) },
        { 497, (0.003239000000, 0.421588000000, 0.264816000000) },
        { 498, (0.002934000000, 0.434619000000, 0.247848000000) },
        { 499, (0.003114000000, 0.447601000000, 0.232318000000) },
        { 500, (0.003816000000, 0.460777000000, 0.218502000000) },
        { 501, (0.005095000000, 0.474340000000, 0.205851000000) },
        { 502, (0.006936000000, 0.488200000000, 0.193596000000) },
        { 503, (0.009299000000, 0.502340000000, 0.181736000000) },
        { 504, (0.012147000000, 0.516740000000, 0.170281000000) },
        { 505, (0.015444000000, 0.531360000000, 0.159249000000) },
        { 506, (0.019156000000, 0.546190000000, 0.148673000000) },
        { 507, (0.023250000000, 0.561180000000, 0.138609000000) },
        { 508, (0.027690000000, 0.576290000000, 0.129096000000) },
        { 509, (0.032444000000, 0.591500000000, 0.120215000000) },
        { 510, (0.037465000000, 0.606741000000, 0.112044000000) },
        { 511, (0.042956000000, 0.622150000000, 0.104710000000) },
        { 512, (0.049114000000, 0.637830000000, 0.098196000000) },
        { 513, (0.055920000000, 0.653710000000, 0.092361000000) },
        { 514, (0.063349000000, 0.669680000000, 0.087088000000) },
        { 515, (0.071358000000, 0.685660000000, 0.082248000000) },
        { 516, (0.079901000000, 0.701550000000, 0.077744000000) },
        { 517, (0.088909000000, 0.717230000000, 0.073456000000) },
        { 518, (0.098293000000, 0.732570000000, 0.069268000000) },
        { 519, (0.107949000000, 0.747460000000, 0.065060000000) },
        { 520, (0.117749000000, 0.761757000000, 0.060709000000) },
        { 521, (0.127839000000, 0.775340000000, 0.056457000000) },
        { 522, (0.138450000000, 0.788220000000, 0.052609000000) },
        { 523, (0.149516000000, 0.800460000000, 0.049122000000) },
        { 524, (0.161041000000, 0.812140000000, 0.045954000000) },
        { 525, (0.172953000000, 0.823330000000, 0.043050000000) },
        { 526, (0.185209000000, 0.834120000000, 0.040368000000) },
        { 527, (0.197755000000, 0.844600000000, 0.037839000000) },
        { 528, (0.210538000000, 0.854870000000, 0.035384000000) },
        { 529, (0.223460000000, 0.865040000000, 0.032949000000) },
        { 530, (0.236491000000, 0.875211000000, 0.030451000000) },
        { 531, (0.249633000000, 0.885370000000, 0.028029000000) },
        { 532, (0.262972000000, 0.895370000000, 0.025862000000) },
        { 533, (0.276515000000, 0.905150000000, 0.023920000000) },
        { 534, (0.290269000000, 0.914650000000, 0.022174000000) },
        { 535, (0.304213000000, 0.923810000000, 0.020584000000) },
        { 536, (0.318361000000, 0.932550000000, 0.019127000000) },
        { 537, (0.332705000000, 0.940810000000, 0.017740000000) },
        { 538, (0.347232000000, 0.948520000000, 0.016403000000) },
        { 539, (0.361926000000, 0.955600000000, 0.015064000000) },
        { 540, (0.376772000000, 0.961988000000, 0.013676000000) },
        { 541, (0.391683000000, 0.967540000000, 0.012308000000) },
        { 542, (0.406594000000, 0.972230000000, 0.011056000000) },
        { 543, (0.421539000000, 0.976170000000, 0.009915000000) },
        { 544, (0.436517000000, 0.979460000000, 0.008872000000) },
        { 545, (0.451584000000, 0.982200000000, 0.007918000000) },
        { 546, (0.466782000000, 0.984520000000, 0.007030000000) },
        { 547, (0.482147000000, 0.986520000000, 0.006223000000) },
        { 548, (0.497738000000, 0.988320000000, 0.005453000000) },
        { 549, (0.513606000000, 0.990020000000, 0.004714000000) },
        { 550, (0.529826000000, 0.991761000000, 0.003988000000) },
        { 551, (0.546440000000, 0.993530000000, 0.003289000000) },
        { 552, (0.563426000000, 0.995230000000, 0.002646000000) },
        { 553, (0.580726000000, 0.996770000000, 0.002063000000) },
        { 554, (0.598290000000, 0.998090000000, 0.001533000000) },
        { 555, (0.616053000000, 0.999110000000, 0.001091000000) },
        { 556, (0.633948000000, 0.999770000000, 0.000711000000) },
        { 557, (0.651901000000, 1.000000000000, 0.000407000000) },
        { 558, (0.669824000000, 0.999710000000, 0.000184000000) },
        { 559, (0.687632000000, 0.998850000000, 0.000047000000) },
        { 560, (0.705224000000, 0.997340000000, 0.000000000000) },
        { 561, (0.722773000000, 0.995260000000, 0.000000000000) },
        { 562, (0.740483000000, 0.992740000000, 0.000000000000) },
        { 563, (0.758273000000, 0.989750000000, 0.000000000000) },
        { 564, (0.776083000000, 0.986300000000, 0.000000000000) },
        { 565, (0.793832000000, 0.982380000000, 0.000000000000) },
        { 566, (0.811436000000, 0.977980000000, 0.000000000000) },
        { 567, (0.828822000000, 0.973110000000, 0.000000000000) },
        { 568, (0.845879000000, 0.967740000000, 0.000000000000) },
        { 569, (0.862525000000, 0.961890000000, 0.000000000000) },
        { 570, (0.878655000000, 0.955552000000, 0.000000000000) },
        { 571, (0.894208000000, 0.948601000000, 0.000000000000) },
        { 572, (0.909206000000, 0.940981000000, 0.000000000000) },
        { 573, (0.923672000000, 0.932798000000, 0.000000000000) },
        { 574, (0.937638000000, 0.924158000000, 0.000000000000) },
        { 575, (0.951162000000, 0.915175000000, 0.000000000000) },
        { 576, (0.964283000000, 0.905954000000, 0.000000000000) },
        { 577, (0.977068000000, 0.896608000000, 0.000000000000) },
        { 578, (0.989590000000, 0.887249000000, 0.000000000000) },
        { 579, (1.001910000000, 0.877986000000, 0.000000000000) },
        { 580, (1.014160000000, 0.868934000000, 0.000000000000) },
        { 581, (1.026500000000, 0.860164000000, 0.000000000000) },
        { 582, (1.038800000000, 0.851519000000, 0.000000000000) },
        { 583, (1.051000000000, 0.842963000000, 0.000000000000) },
        { 584, (1.062900000000, 0.834393000000, 0.000000000000) },
        { 585, (1.074300000000, 0.825623000000, 0.000000000000) },
        { 586, (1.085200000000, 0.816764000000, 0.000000000000) },
        { 587, (1.095200000000, 0.807544000000, 0.000000000000) },
        { 588, (1.104200000000, 0.797947000000, 0.000000000000) },
        { 589, (1.112000000000, 0.787893000000, 0.000000000000) },
        { 590, (1.118520000000, 0.777405000000, 0.000000000000) },
        { 591, (1.123800000000, 0.766490000000, 0.000000000000) },
        { 592, (1.128000000000, 0.755309000000, 0.000000000000) },
        { 593, (1.131100000000, 0.743845000000, 0.000000000000) },
        { 594, (1.133200000000, 0.732190000000, 0.000000000000) },
        { 595, (1.134300000000, 0.720353000000, 0.000000000000) },
        { 596, (1.134300000000, 0.708281000000, 0.000000000000) },
        { 597, (1.133300000000, 0.696055000000, 0.000000000000) },
        { 598, (1.131200000000, 0.683621000000, 0.000000000000) },
        { 599, (1.128100000000, 0.671048000000, 0.000000000000) },
        { 600, (1.123990000000, 0.658341000000, 0.000000000000) },
        { 601, (1.118900000000, 0.645545000000, 0.000000000000) },
        { 602, (1.112900000000, 0.632718000000, 0.000000000000) },
        { 603, (1.105900000000, 0.619815000000, 0.000000000000) },
        { 604, (1.098000000000, 0.606887000000, 0.000000000000) },
        { 605, (1.089100000000, 0.593878000000, 0.000000000000) },
        { 606, (1.079200000000, 0.580781000000, 0.000000000000) },
        { 607, (1.068400000000, 0.567653000000, 0.000000000000) },
        { 608, (1.056700000000, 0.554490000000, 0.000000000000) },
        { 609, (1.044000000000, 0.541228000000, 0.000000000000) },
        { 610, (1.030480000000, 0.527963000000, 0.000000000000) },
        { 611, (1.016000000000, 0.514634000000, 0.000000000000) },
        { 612, (1.000800000000, 0.501363000000, 0.000000000000) },
        { 613, (0.984790000000, 0.488124000000, 0.000000000000) },
        { 614, (0.968080000000, 0.474935000000, 0.000000000000) },
        { 615, (0.950740000000, 0.461834000000, 0.000000000000) },
        { 616, (0.932800000000, 0.448823000000, 0.000000000000) },
        { 617, (0.914340000000, 0.435917000000, 0.000000000000) },
        { 618, (0.895390000000, 0.423153000000, 0.000000000000) },
        { 619, (0.876030000000, 0.410526000000, 0.000000000000) },
        { 620, (0.856297000000, 0.398057000000, 0.000000000000) },
        { 621, (0.836350000000, 0.385835000000, 0.000000000000) },
        { 622, (0.816290000000, 0.373951000000, 0.000000000000) },
        { 623, (0.796050000000, 0.362311000000, 0.000000000000) },
        { 624, (0.775610000000, 0.350863000000, 0.000000000000) },
        { 625, (0.754930000000, 0.339554000000, 0.000000000000) },
        { 626, (0.733990000000, 0.328309000000, 0.000000000000) },
        { 627, (0.712780000000, 0.317118000000, 0.000000000000) },
        { 628, (0.691290000000, 0.305936000000, 0.000000000000) },
        { 629, (0.669520000000, 0.294737000000, 0.000000000000) },
        { 630, (0.647467000000, 0.283493000000, 0.000000000000) },
        { 631, (0.625110000000, 0.272222000000, 0.000000000000) },
        { 632, (0.602520000000, 0.260990000000, 0.000000000000) },
        { 633, (0.579890000000, 0.249877000000, 0.000000000000) },
        { 634, (0.557370000000, 0.238946000000, 0.000000000000) },
        { 635, (0.535110000000, 0.228254000000, 0.000000000000) },
        { 636, (0.513240000000, 0.217853000000, 0.000000000000) },
        { 637, (0.491860000000, 0.207780000000, 0.000000000000) },
        { 638, (0.471080000000, 0.198072000000, 0.000000000000) },
        { 639, (0.450960000000, 0.188748000000, 0.000000000000) },
        { 640, (0.431567000000, 0.179828000000, 0.000000000000) },
        { 641, (0.412870000000, 0.171285000000, 0.000000000000) },
        { 642, (0.394750000000, 0.163059000000, 0.000000000000) },
        { 643, (0.377210000000, 0.155151000000, 0.000000000000) },
        { 644, (0.360190000000, 0.147535000000, 0.000000000000) },
        { 645, (0.343690000000, 0.140211000000, 0.000000000000) },
        { 646, (0.327690000000, 0.133170000000, 0.000000000000) },
        { 647, (0.312170000000, 0.126400000000, 0.000000000000) },
        { 648, (0.297110000000, 0.119892000000, 0.000000000000) },
        { 649, (0.282500000000, 0.113640000000, 0.000000000000) },
        { 650, (0.268329000000, 0.107633000000, 0.000000000000) },
        { 651, (0.254590000000, 0.101870000000, 0.000000000000) },
        { 652, (0.241300000000, 0.096347000000, 0.000000000000) },
        { 653, (0.228480000000, 0.091063000000, 0.000000000000) },
        { 654, (0.216140000000, 0.086010000000, 0.000000000000) },
        { 655, (0.204300000000, 0.081187000000, 0.000000000000) },
        { 656, (0.192950000000, 0.076583000000, 0.000000000000) },
        { 657, (0.182110000000, 0.072198000000, 0.000000000000) },
        { 658, (0.171770000000, 0.068024000000, 0.000000000000) },
        { 659, (0.161920000000, 0.064052000000, 0.000000000000) },
        { 660, (0.152568000000, 0.060281000000, 0.000000000000) },
        { 661, (0.143670000000, 0.056697000000, 0.000000000000) },
        { 662, (0.135200000000, 0.053292000000, 0.000000000000) },
        { 663, (0.127130000000, 0.050059000000, 0.000000000000) },
        { 664, (0.119480000000, 0.046998000000, 0.000000000000) },
        { 665, (0.112210000000, 0.044096000000, 0.000000000000) },
        { 666, (0.105310000000, 0.041345000000, 0.000000000000) },
        { 667, (0.098786000000, 0.038750700000, 0.000000000000) },
        { 668, (0.092610000000, 0.036297800000, 0.000000000000) },
        { 669, (0.086773000000, 0.033983200000, 0.000000000000) },
        { 670, (0.081260600000, 0.031800400000, 0.000000000000) },
        { 671, (0.076048000000, 0.029739500000, 0.000000000000) },
        { 672, (0.071114000000, 0.027791800000, 0.000000000000) },
        { 673, (0.066454000000, 0.025955100000, 0.000000000000) },
        { 674, (0.062062000000, 0.024226300000, 0.000000000000) },
        { 675, (0.057930000000, 0.022601700000, 0.000000000000) },
        { 676, (0.054050000000, 0.021077900000, 0.000000000000) },
        { 677, (0.050412000000, 0.019650500000, 0.000000000000) },
        { 678, (0.047006000000, 0.018315300000, 0.000000000000) },
        { 679, (0.043823000000, 0.017068600000, 0.000000000000) },
        { 680, (0.040850800000, 0.015905100000, 0.000000000000) },
        { 681, (0.038072000000, 0.014818300000, 0.000000000000) },
        { 682, (0.035468000000, 0.013800800000, 0.000000000000) },
        { 683, (0.033031000000, 0.012849500000, 0.000000000000) },
        { 684, (0.030753000000, 0.011960700000, 0.000000000000) },
        { 685, (0.028623000000, 0.011130300000, 0.000000000000) },
        { 686, (0.026635000000, 0.010355500000, 0.000000000000) },
        { 687, (0.024781000000, 0.009633200000, 0.000000000000) },
        { 688, (0.023052000000, 0.008959900000, 0.000000000000) },
        { 689, (0.021441000000, 0.008332400000, 0.000000000000) },
        { 690, (0.019941300000, 0.007748800000, 0.000000000000) },
        { 691, (0.018544000000, 0.007204600000, 0.000000000000) },
        { 692, (0.017241000000, 0.006697500000, 0.000000000000) },
        { 693, (0.016027000000, 0.006225100000, 0.000000000000) },
        { 694, (0.014896000000, 0.005785000000, 0.000000000000) },
        { 695, (0.013842000000, 0.005375100000, 0.000000000000) },
        { 696, (0.012862000000, 0.004994100000, 0.000000000000) },
        { 697, (0.011949000000, 0.004639200000, 0.000000000000) },
        { 698, (0.011100000000, 0.004309300000, 0.000000000000) },
        { 699, (0.010311000000, 0.004002800000, 0.000000000000) },
        { 700, (0.009576880000, 0.003717740000, 0.000000000000) },
        { 701, (0.008894000000, 0.003452620000, 0.000000000000) },
        { 702, (0.008258100000, 0.003205830000, 0.000000000000) },
        { 703, (0.007666400000, 0.002976230000, 0.000000000000) },
        { 704, (0.007116300000, 0.002762810000, 0.000000000000) },
        { 705, (0.006605200000, 0.002564560000, 0.000000000000) },
        { 706, (0.006130600000, 0.002380480000, 0.000000000000) },
        { 707, (0.005690300000, 0.002209710000, 0.000000000000) },
        { 708, (0.005281900000, 0.002051320000, 0.000000000000) },
        { 709, (0.004903300000, 0.001904490000, 0.000000000000) },
        { 710, (0.004552630000, 0.001768470000, 0.000000000000) },
        { 711, (0.004227500000, 0.001642360000, 0.000000000000) },
        { 712, (0.003925800000, 0.001525350000, 0.000000000000) },
        { 713, (0.003645700000, 0.001416720000, 0.000000000000) },
        { 714, (0.003385900000, 0.001315950000, 0.000000000000) },
        { 715, (0.003144700000, 0.001222390000, 0.000000000000) },
        { 716, (0.002920800000, 0.001135550000, 0.000000000000) },
        { 717, (0.002713000000, 0.001054940000, 0.000000000000) },
        { 718, (0.002520200000, 0.000980140000, 0.000000000000) },
        { 719, (0.002341100000, 0.000910660000, 0.000000000000) },
        { 720, (0.002174960000, 0.000846190000, 0.000000000000) },
        { 721, (0.002020600000, 0.000786290000, 0.000000000000) },
        { 722, (0.001877300000, 0.000730680000, 0.000000000000) },
        { 723, (0.001744100000, 0.000678990000, 0.000000000000) },
        { 724, (0.001620500000, 0.000631010000, 0.000000000000) },
        { 725, (0.001505700000, 0.000586440000, 0.000000000000) },
        { 726, (0.001399200000, 0.000545110000, 0.000000000000) },
        { 727, (0.001300400000, 0.000506720000, 0.000000000000) },
        { 728, (0.001208700000, 0.000471110000, 0.000000000000) },
        { 729, (0.001123600000, 0.000438050000, 0.000000000000) },
        { 730, (0.001044760000, 0.000407410000, 0.000000000000) },
        { 731, (0.000971560000, 0.000378962000, 0.000000000000) },
        { 732, (0.000903600000, 0.000352543000, 0.000000000000) },
        { 733, (0.000840480000, 0.000328001000, 0.000000000000) },
        { 734, (0.000781870000, 0.000305208000, 0.000000000000) },
        { 735, (0.000727450000, 0.000284041000, 0.000000000000) },
        { 736, (0.000676900000, 0.000264375000, 0.000000000000) },
        { 737, (0.000629960000, 0.000246109000, 0.000000000000) },
        { 738, (0.000586370000, 0.000229143000, 0.000000000000) },
        { 739, (0.000545870000, 0.000213376000, 0.000000000000) },
        { 740, (0.000508258000, 0.000198730000, 0.000000000000) },
        { 741, (0.000473300000, 0.000185115000, 0.000000000000) },
        { 742, (0.000440800000, 0.000172454000, 0.000000000000) },
        { 743, (0.000410580000, 0.000160678000, 0.000000000000) },
        { 744, (0.000382490000, 0.000149730000, 0.000000000000) },
        { 745, (0.000356380000, 0.000139550000, 0.000000000000) },
        { 746, (0.000332110000, 0.000130086000, 0.000000000000) },
        { 747, (0.000309550000, 0.000121290000, 0.000000000000) },
        { 748, (0.000288580000, 0.000113106000, 0.000000000000) },
        { 749, (0.000269090000, 0.000105501000, 0.000000000000) },
        { 750, (0.000250969000, 0.000098428000, 0.000000000000) },
        { 751, (0.000234130000, 0.000091853000, 0.000000000000) },
        { 752, (0.000218470000, 0.000085738000, 0.000000000000) },
        { 753, (0.000203910000, 0.000080048000, 0.000000000000) },
        { 754, (0.000190350000, 0.000074751000, 0.000000000000) },
        { 755, (0.000177730000, 0.000069819000, 0.000000000000) },
        { 756, (0.000165970000, 0.000065222000, 0.000000000000) },
        { 757, (0.000155020000, 0.000060939000, 0.000000000000) },
        { 758, (0.000144800000, 0.000056942000, 0.000000000000) },
        { 759, (0.000135280000, 0.000053217000, 0.000000000000) },
        { 760, (0.000126390000, 0.000049737000, 0.000000000000) },
        { 761, (0.000118100000, 0.000046491000, 0.000000000000) },
        { 762, (0.000110370000, 0.000043464000, 0.000000000000) },
        { 763, (0.000103150000, 0.000040635000, 0.000000000000) },
        { 764, (0.000096427000, 0.000038000000, 0.000000000000) },
        { 765, (0.000090151000, 0.000035540500, 0.000000000000) },
        { 766, (0.000084294000, 0.000033244800, 0.000000000000) },
        { 767, (0.000078830000, 0.000031100600, 0.000000000000) },
        { 768, (0.000073729000, 0.000029099000, 0.000000000000) },
        { 769, (0.000068969000, 0.000027230700, 0.000000000000) },
        { 770, (0.000064525800, 0.000025486000, 0.000000000000) },
        { 771, (0.000060376000, 0.000023856100, 0.000000000000) },
        { 772, (0.000056500000, 0.000022333200, 0.000000000000) },
        { 773, (0.000052880000, 0.000020910400, 0.000000000000) },
        { 774, (0.000049498000, 0.000019580800, 0.000000000000) },
        { 775, (0.000046339000, 0.000018338400, 0.000000000000) },
        { 776, (0.000043389000, 0.000017177700, 0.000000000000) },
        { 777, (0.000040634000, 0.000016093400, 0.000000000000) },
        { 778, (0.000038060000, 0.000015080000, 0.000000000000) },
        { 779, (0.000035657000, 0.000014133600, 0.000000000000) },
        { 780, (0.000033411700, 0.000013249000, 0.000000000000) },
        { 781, (0.000031315000, 0.000012422600, 0.000000000000) },
        { 782, (0.000029355000, 0.000011649900, 0.000000000000) },
        { 783, (0.000027524000, 0.000010927700, 0.000000000000) },
        { 784, (0.000025811000, 0.000010251900, 0.000000000000) },
        { 785, (0.000024209000, 0.000009619600, 0.000000000000) },
        { 786, (0.000022711000, 0.000009028100, 0.000000000000) },
        { 787, (0.000021308000, 0.000008474000, 0.000000000000) },
        { 788, (0.000019994000, 0.000007954800, 0.000000000000) },
        { 789, (0.000018764000, 0.000007468600, 0.000000000000) },
        { 790, (0.000017611500, 0.000007012800, 0.000000000000) },
        { 791, (0.000016532000, 0.000006585800, 0.000000000000) },
        { 792, (0.000015521000, 0.000006185700, 0.000000000000) },
        { 793, (0.000014574000, 0.000005810700, 0.000000000000) },
        { 794, (0.000013686000, 0.000005459000, 0.000000000000) },
        { 795, (0.000012855000, 0.000005129800, 0.000000000000) },
        { 796, (0.000012075000, 0.000004820600, 0.000000000000) },
        { 797, (0.000011345000, 0.000004531200, 0.000000000000) },
        { 798, (0.000010659000, 0.000004259100, 0.000000000000) },
        { 799, (0.000010017000, 0.000004004200, 0.000000000000) },
        { 800, (0.000009413630, 0.000003764730, 0.000000000000) },
        { 801, (0.000008847900, 0.000003539950, 0.000000000000) },
        { 802, (0.000008317100, 0.000003329140, 0.000000000000) },
        { 803, (0.000007819000, 0.000003131150, 0.000000000000) },
        { 804, (0.000007351600, 0.000002945290, 0.000000000000) },
        { 805, (0.000006913000, 0.000002770810, 0.000000000000) },
        { 806, (0.000006501500, 0.000002607050, 0.000000000000) },
        { 807, (0.000006115300, 0.000002453290, 0.000000000000) },
        { 808, (0.000005752900, 0.000002308940, 0.000000000000) },
        { 809, (0.000005412700, 0.000002173380, 0.000000000000) },
        { 810, (0.000005093470, 0.000002046130, 0.000000000000) },
        { 811, (0.000004793800, 0.000001926620, 0.000000000000) },
        { 812, (0.000004512500, 0.000001814400, 0.000000000000) },
        { 813, (0.000004248300, 0.000001708950, 0.000000000000) },
        { 814, (0.000004000200, 0.000001609880, 0.000000000000) },
        { 815, (0.000003767100, 0.000001516770, 0.000000000000) },
        { 816, (0.000003548000, 0.000001429210, 0.000000000000) },
        { 817, (0.000003342100, 0.000001346860, 0.000000000000) },
        { 818, (0.000003148500, 0.000001269450, 0.000000000000) },
        { 819, (0.000002966500, 0.000001196620, 0.000000000000) },
        { 820, (0.000002795310, 0.000001128090, 0.000000000000) },
        { 821, (0.000002634500, 0.000001063680, 0.000000000000) },
        { 822, (0.000002483400, 0.000001003130, 0.000000000000) },
        { 823, (0.000002341400, 0.000000946220, 0.000000000000) },
        { 824, (0.000002207800, 0.000000892630, 0.000000000000) },
        { 825, (0.000002082000, 0.000000842160, 0.000000000000) },
        { 826, (0.000001963600, 0.000000794640, 0.000000000000) },
        { 827, (0.000001851900, 0.000000749780, 0.000000000000) },
        { 828, (0.000001746500, 0.000000707440, 0.000000000000) },
        { 829, (0.000001647100, 0.000000667480, 0.000000000000) },
        { 830, (0.000001553140, 0.000000629700, 0.000000000000) },

     };

        // CIE 1931 2°标准观察者色度匹配函数
        private static readonly Dictionary<int, (double X, double Y, double Z)> CIE_1931_2 = new Dictionary<int, (double X, double Y, double Z)>
    {
        { 360, (0.000129900000, 0.000003917000, 0.000606100000) },
        { 361, (0.000145847000, 0.000004393581, 0.000680879200) },
        { 362, (0.000163802100, 0.000004929604, 0.000765145600) },
        { 363, (0.000184003700, 0.000005532136, 0.000860012400) },
        { 364, (0.000206690200, 0.000006208245, 0.000966592800) },
        { 365, (0.000232100000, 0.000006965000, 0.001086000000) },
        { 366, (0.000260728000, 0.000007813219, 0.001220586000) },
        { 367, (0.000293075000, 0.000008767336, 0.001372729000) },
        { 368, (0.000329388000, 0.000009839844, 0.001543579000) },
        { 369, (0.000369914000, 0.000011043230, 0.001734286000) },
        { 370, (0.000414900000, 0.000012390000, 0.001946000000) },
        { 371, (0.000464158700, 0.000013886410, 0.002177777000) },
        { 372, (0.000518986000, 0.000015557280, 0.002435809000) },
        { 373, (0.000581854000, 0.000017442960, 0.002731953000) },
        { 374, (0.000655234700, 0.000019583750, 0.003078064000) },
        { 375, (0.000741600000, 0.000022020000, 0.003486000000) },
        { 376, (0.000845029600, 0.000024839650, 0.003975227000) },
        { 377, (0.000964526800, 0.000028041260, 0.004540880000) },
        { 378, (0.001094949000, 0.000031531040, 0.005158320000) },
        { 379, (0.001231154000, 0.000035215210, 0.005802907000) },
        { 380, (0.001368000000, 0.000039000000, 0.006450001000) },
        { 381, (0.001502050000, 0.000042826400, 0.007083216000) },
        { 382, (0.001642328000, 0.000046914600, 0.007745488000) },
        { 383, (0.001802382000, 0.000051589600, 0.008501152000) },
        { 384, (0.001995757000, 0.000057176400, 0.009414544000) },
        { 385, (0.002236000000, 0.000064000000, 0.010549990000) },
        { 386, (0.002535385000, 0.000072344210, 0.011965800000) },
        { 387, (0.002892603000, 0.000082212240, 0.013655870000) },
        { 388, (0.003300829000, 0.000093508160, 0.015588050000) },
        { 389, (0.003753236000, 0.000106136100, 0.017730150000) },
        { 390, (0.004243000000, 0.000120000000, 0.020050010000) },
        { 391, (0.004762389000, 0.000134984000, 0.022511360000) },
        { 392, (0.005330048000, 0.000151492000, 0.025202880000) },
        { 393, (0.005978712000, 0.000170208000, 0.028279720000) },
        { 394, (0.006741117000, 0.000191816000, 0.031897040000) },
        { 395, (0.007650000000, 0.000217000000, 0.036210000000) },
        { 396, (0.008751373000, 0.000246906700, 0.041437710000) },
        { 397, (0.010028880000, 0.000281240000, 0.047503720000) },
        { 398, (0.011421700000, 0.000318520000, 0.054119880000) },
        { 399, (0.012869010000, 0.000357266700, 0.060998030000) },
        { 400, (0.014310000000, 0.000396000000, 0.067850010000) },
        { 401, (0.015704430000, 0.000433714700, 0.074486320000) },
        { 402, (0.017147440000, 0.000473024000, 0.081361560000) },
        { 403, (0.018781220000, 0.000517876000, 0.089153640000) },
        { 404, (0.020748010000, 0.000572218700, 0.098540480000) },
        { 405, (0.023190000000, 0.000640000000, 0.110200000000) },
        { 406, (0.026207360000, 0.000724560000, 0.124613300000) },
        { 407, (0.029782480000, 0.000825500000, 0.141701700000) },
        { 408, (0.033880920000, 0.000941160000, 0.161303500000) },
        { 409, (0.038468240000, 0.001069880000, 0.183256800000) },
        { 410, (0.043510000000, 0.001210000000, 0.207400000000) },
        { 411, (0.048995600000, 0.001362091000, 0.233692100000) },
        { 412, (0.055022600000, 0.001530752000, 0.262611400000) },
        { 413, (0.061718800000, 0.001720368000, 0.294774600000) },
        { 414, (0.069212000000, 0.001935323000, 0.330798500000) },
        { 415, (0.077630000000, 0.002180000000, 0.371300000000) },
        { 416, (0.086958110000, 0.002454800000, 0.416209100000) },
        { 417, (0.097176720000, 0.002764000000, 0.465464200000) },
        { 418, (0.108406300000, 0.003117800000, 0.519694800000) },
        { 419, (0.120767200000, 0.003526400000, 0.579530300000) },
        { 420, (0.134380000000, 0.004000000000, 0.645600000000) },
        { 421, (0.149358200000, 0.004546240000, 0.718483800000) },
        { 422, (0.165395700000, 0.005159320000, 0.796713300000) },
        { 423, (0.181983100000, 0.005829280000, 0.877845900000) },
        { 424, (0.198611000000, 0.006546160000, 0.959439000000) },
        { 425, (0.214770000000, 0.007300000000, 1.039050100000) },
        { 426, (0.230186800000, 0.008086507000, 1.115367300000) },
        { 427, (0.244879700000, 0.008908720000, 1.188497100000) },
        { 428, (0.258777300000, 0.009767680000, 1.258123300000) },
        { 429, (0.271807900000, 0.010664430000, 1.323929600000) },
        { 430, (0.283900000000, 0.011600000000, 1.385600000000) },
        { 431, (0.294943800000, 0.012573170000, 1.442635200000) },
        { 432, (0.304896500000, 0.013582720000, 1.494803500000) },
        { 433, (0.313787300000, 0.014629680000, 1.542190300000) },
        { 434, (0.321645400000, 0.015715090000, 1.584880700000) },
        { 435, (0.328500000000, 0.016840000000, 1.622960000000) },
        { 436, (0.334351300000, 0.018007360000, 1.656404800000) },
        { 437, (0.339210100000, 0.019214480000, 1.685295900000) },
        { 438, (0.343121300000, 0.020453920000, 1.709874500000) },
        { 439, (0.346129600000, 0.021718240000, 1.730382100000) },
        { 440, (0.348280000000, 0.023000000000, 1.747060000000) },
        { 441, (0.349599900000, 0.024294610000, 1.760044600000) },
        { 442, (0.350147400000, 0.025610240000, 1.769623300000) },
        { 443, (0.350013000000, 0.026958570000, 1.776263700000) },
        { 444, (0.349287000000, 0.028351250000, 1.780433400000) },
        { 445, (0.348060000000, 0.029800000000, 1.782600000000) },
        { 446, (0.346373300000, 0.031310830000, 1.782968200000) },
        { 447, (0.344262400000, 0.032883680000, 1.781699800000) },
        { 448, (0.341808800000, 0.034521120000, 1.779198200000) },
        { 449, (0.339094100000, 0.036225710000, 1.775867100000) },
        { 450, (0.336200000000, 0.038000000000, 1.772110000000) },
        { 451, (0.333197700000, 0.039846670000, 1.768258900000) },
        { 452, (0.330041100000, 0.041768000000, 1.764039000000) },
        { 453, (0.326635700000, 0.043766000000, 1.758943800000) },
        { 454, (0.322886800000, 0.045842670000, 1.752466300000) },
        { 455, (0.318700000000, 0.048000000000, 1.744100000000) },
        { 456, (0.314025100000, 0.050243680000, 1.733559500000) },
        { 457, (0.308884000000, 0.052573040000, 1.720858100000) },
        { 458, (0.303290400000, 0.054980560000, 1.705936900000) },
        { 459, (0.297257900000, 0.057458720000, 1.688737200000) },
        { 460, (0.290800000000, 0.060000000000, 1.669200000000) },
        { 461, (0.283970100000, 0.062601970000, 1.647528700000) },
        { 462, (0.276721400000, 0.065277520000, 1.623412700000) },
        { 463, (0.268917800000, 0.068042080000, 1.596022300000) },
        { 464, (0.260422700000, 0.070911090000, 1.564528000000) },
        { 465, (0.251100000000, 0.073900000000, 1.528100000000) },
        { 466, (0.240847500000, 0.077016000000, 1.486111400000) },
        { 467, (0.229851200000, 0.080266400000, 1.439521500000) },
        { 468, (0.218407200000, 0.083666800000, 1.389879900000) },
        { 469, (0.206811500000, 0.087232800000, 1.338736200000) },
        { 470, (0.195360000000, 0.090980000000, 1.287640000000) },
        { 471, (0.184213600000, 0.094917550000, 1.237422300000) },
        { 472, (0.173327300000, 0.099045840000, 1.187824300000) },
        { 473, (0.162688100000, 0.103367400000, 1.138761100000) },
        { 474, (0.152283300000, 0.107884600000, 1.090148000000) },
        { 475, (0.142100000000, 0.112600000000, 1.041900000000) },
        { 476, (0.132178600000, 0.117532000000, 0.994197600000) },
        { 477, (0.122569600000, 0.122674400000, 0.947347300000) },
        { 478, (0.113275200000, 0.127992800000, 0.901453100000) },
        { 479, (0.104297900000, 0.133452800000, 0.856619300000) },
        { 480, (0.095640000000, 0.139020000000, 0.812950100000) },
        { 481, (0.087299550000, 0.144676400000, 0.770517300000) },
        { 482, (0.079308040000, 0.150469300000, 0.729444800000) },
        { 483, (0.071717760000, 0.156461900000, 0.689913600000) },
        { 484, (0.064580990000, 0.162717700000, 0.652104900000) },
        { 485, (0.057950010000, 0.169300000000, 0.616200000000) },
        { 486, (0.051862110000, 0.176243100000, 0.582328600000) },
        { 487, (0.046281520000, 0.183558100000, 0.550416200000) },
        { 488, (0.041150880000, 0.191273500000, 0.520337600000) },
        { 489, (0.036412830000, 0.199418000000, 0.491967300000) },
        { 490, (0.032010000000, 0.208020000000, 0.465180000000) },
        { 491, (0.027917200000, 0.217119900000, 0.439924600000) },
        { 492, (0.024144400000, 0.226734500000, 0.416183600000) },
        { 493, (0.020687000000, 0.236857100000, 0.393882200000) },
        { 494, (0.017540400000, 0.247481200000, 0.372945900000) },
        { 495, (0.014700000000, 0.258600000000, 0.353300000000) },
        { 496, (0.012161790000, 0.270184900000, 0.334857800000) },
        { 497, (0.009919960000, 0.282293900000, 0.317552100000) },
        { 498, (0.007967240000, 0.295050500000, 0.301337500000) },
        { 499, (0.006296346000, 0.308578000000, 0.286168600000) },
        { 500, (0.004900000000, 0.323000000000, 0.272000000000) },
        { 501, (0.003777173000, 0.338402100000, 0.258817100000) },
        { 502, (0.002945320000, 0.354685800000, 0.246483800000) },
        { 503, (0.002424880000, 0.371698600000, 0.234771800000) },
        { 504, (0.002236293000, 0.389287500000, 0.223453300000) },
        { 505, (0.002400000000, 0.407300000000, 0.212300000000) },
        { 506, (0.002925520000, 0.425629900000, 0.201169200000) },
        { 507, (0.003836560000, 0.444309600000, 0.190119600000) },
        { 508, (0.005174840000, 0.463394400000, 0.179225400000) },
        { 509, (0.006982080000, 0.482939500000, 0.168560800000) },
        { 510, (0.009300000000, 0.503000000000, 0.158200000000) },
        { 511, (0.012149490000, 0.523569300000, 0.148138300000) },
        { 512, (0.015535880000, 0.544512000000, 0.138375800000) },
        { 513, (0.019477520000, 0.565690000000, 0.128994200000) },
        { 514, (0.023992770000, 0.586965300000, 0.120075100000) },
        { 515, (0.029100000000, 0.608200000000, 0.111700000000) },
        { 516, (0.034814850000, 0.629345600000, 0.103904800000) },
        { 517, (0.041120160000, 0.650306800000, 0.096667480000) },
        { 518, (0.047985040000, 0.670875200000, 0.089982720000) },
        { 519, (0.055378610000, 0.690842400000, 0.083845310000) },
        { 520, (0.063270000000, 0.710000000000, 0.078249990000) },
        { 521, (0.071635010000, 0.728185200000, 0.073208990000) },
        { 522, (0.080462240000, 0.745463600000, 0.068678160000) },
        { 523, (0.089739960000, 0.761969400000, 0.064567840000) },
        { 524, (0.099456450000, 0.777836800000, 0.060788350000) },
        { 525, (0.109600000000, 0.793200000000, 0.057250010000) },
        { 526, (0.120167400000, 0.808110400000, 0.053904350000) },
        { 527, (0.131114500000, 0.822496200000, 0.050746640000) },
        { 528, (0.142367900000, 0.836306800000, 0.047752760000) },
        { 529, (0.153854200000, 0.849491600000, 0.044898590000) },
        { 530, (0.165500000000, 0.862000000000, 0.042160000000) },
        { 531, (0.177257100000, 0.873810800000, 0.039507280000) },
        { 532, (0.189140000000, 0.884962400000, 0.036935640000) },
        { 533, (0.201169400000, 0.895493600000, 0.034458360000) },
        { 534, (0.213365800000, 0.905443200000, 0.032088720000) },
        { 535, (0.225749900000, 0.914850100000, 0.029840000000) },
        { 536, (0.238320900000, 0.923734800000, 0.027711810000) },
        { 537, (0.251066800000, 0.932092400000, 0.025694440000) },
        { 538, (0.263992200000, 0.939922600000, 0.023787160000) },
        { 539, (0.277101700000, 0.947225200000, 0.021989250000) },
        { 540, (0.290400000000, 0.954000000000, 0.020300000000) },
        { 541, (0.303891200000, 0.960256100000, 0.018718050000) },
        { 542, (0.317572600000, 0.966007400000, 0.017240360000) },
        { 543, (0.331438400000, 0.971260600000, 0.015863640000) },
        { 544, (0.345482800000, 0.976022500000, 0.014584610000) },
        { 545, (0.359700000000, 0.980300000000, 0.013400000000) },
        { 546, (0.374083900000, 0.984092400000, 0.012307230000) },
        { 547, (0.388639600000, 0.987418200000, 0.011301880000) },
        { 548, (0.403378400000, 0.990312800000, 0.010377920000) },
        { 549, (0.418311500000, 0.992811600000, 0.009529306000) },
        { 550, (0.433449900000, 0.994950100000, 0.008749999000) },
        { 551, (0.448795300000, 0.996710800000, 0.008035200000) },
        { 552, (0.464336000000, 0.998098300000, 0.007381600000) },
        { 553, (0.480064000000, 0.999112000000, 0.006785400000) },
        { 554, (0.495971300000, 0.999748200000, 0.006242800000) },
        { 555, (0.512050100000, 1.000000000000, 0.005749999000) },
        { 556, (0.528295900000, 0.999856700000, 0.005303600000) },
        { 557, (0.544691600000, 0.999304600000, 0.004899800000) },
        { 558, (0.561209400000, 0.998325500000, 0.004534200000) },
        { 559, (0.577821500000, 0.996898700000, 0.004202400000) },
        { 560, (0.594500000000, 0.995000000000, 0.003900000000) },
        { 561, (0.611220900000, 0.992600500000, 0.003623200000) },
        { 562, (0.627975800000, 0.989742600000, 0.003370600000) },
        { 563, (0.644760200000, 0.986444400000, 0.003141400000) },
        { 564, (0.661569700000, 0.982724100000, 0.002934800000) },
        { 565, (0.678400000000, 0.978600000000, 0.002749999000) },
        { 566, (0.695239200000, 0.974083700000, 0.002585200000) },
        { 567, (0.712058600000, 0.969171200000, 0.002438600000) },
        { 568, (0.728828400000, 0.963856800000, 0.002309400000) },
        { 569, (0.745518800000, 0.958134900000, 0.002196800000) },
        { 570, (0.762100000000, 0.952000000000, 0.002100000000) },
        { 571, (0.778543200000, 0.945450400000, 0.002017733000) },
        { 572, (0.794825600000, 0.938499200000, 0.001948200000) },
        { 573, (0.810926400000, 0.931162800000, 0.001889800000) },
        { 574, (0.826824800000, 0.923457600000, 0.001840933000) },
        { 575, (0.842500000000, 0.915400000000, 0.001800000000) },
        { 576, (0.857932500000, 0.907006400000, 0.001766267000) },
        { 577, (0.873081600000, 0.898277200000, 0.001737800000) },
        { 578, (0.887894400000, 0.889204800000, 0.001711200000) },
        { 579, (0.902318100000, 0.879781600000, 0.001683067000) },
        { 580, (0.916300000000, 0.870000000000, 0.001650001000) },
        { 581, (0.929799500000, 0.859861300000, 0.001610133000) },
        { 582, (0.942798400000, 0.849392000000, 0.001564400000) },
        { 583, (0.955277600000, 0.838622000000, 0.001513600000) },
        { 584, (0.967217900000, 0.827581300000, 0.001458533000) },
        { 585, (0.978600000000, 0.816300000000, 0.001400000000) },
        { 586, (0.989385600000, 0.804794700000, 0.001336667000) },
        { 587, (0.999548800000, 0.793082000000, 0.001270000000) },
        { 588, (1.009089200000, 0.781192000000, 0.001205000000) },
        { 589, (1.018006400000, 0.769154700000, 0.001146667000) },
        { 590, (1.026300000000, 0.757000000000, 0.001100000000) },
        { 591, (1.033982700000, 0.744754100000, 0.001068800000) },
        { 592, (1.040986000000, 0.732422400000, 0.001049400000) },
        { 593, (1.047188000000, 0.720003600000, 0.001035600000) },
        { 594, (1.052466700000, 0.707496500000, 0.001021200000) },
        { 595, (1.056700000000, 0.694900000000, 0.001000000000) },
        { 596, (1.059794400000, 0.682219200000, 0.000968640000) },
        { 597, (1.061799200000, 0.669471600000, 0.000929920000) },
        { 598, (1.062806800000, 0.656674400000, 0.000886880000) },
        { 599, (1.062909600000, 0.643844800000, 0.000842560000) },
        { 600, (1.062200000000, 0.631000000000, 0.000800000000) },
        { 601, (1.060735200000, 0.618155500000, 0.000760960000) },
        { 602, (1.058443600000, 0.605314400000, 0.000723680000) },
        { 603, (1.055224400000, 0.592475600000, 0.000685920000) },
        { 604, (1.050976800000, 0.579637900000, 0.000645440000) },
        { 605, (1.045600000000, 0.566800000000, 0.000600000000) },
        { 606, (1.039036900000, 0.553961100000, 0.000547866700) },
        { 607, (1.031360800000, 0.541137200000, 0.000491600000) },
        { 608, (1.022666200000, 0.528352800000, 0.000435400000) },
        { 609, (1.013047700000, 0.515632300000, 0.000383466700) },
        { 610, (1.002600000000, 0.503000000000, 0.000340000000) },
        { 611, (0.991367500000, 0.490468800000, 0.000307253300) },
        { 612, (0.979331400000, 0.478030400000, 0.000283160000) },
        { 613, (0.966491600000, 0.465677600000, 0.000265440000) },
        { 614, (0.952847900000, 0.453403200000, 0.000251813300) },
        { 615, (0.938400000000, 0.441200000000, 0.000240000000) },
        { 616, (0.923194000000, 0.429080000000, 0.000229546700) },
        { 617, (0.907244000000, 0.417036000000, 0.000220640000) },
        { 618, (0.890502000000, 0.405032000000, 0.000211960000) },
        { 619, (0.872920000000, 0.393032000000, 0.000202186700) },
        { 620, (0.854449900000, 0.381000000000, 0.000190000000) },
        { 621, (0.835084000000, 0.368918400000, 0.000174213300) },
        { 622, (0.814946000000, 0.356827200000, 0.000155640000) },
        { 623, (0.794186000000, 0.344776800000, 0.000135960000) },
        { 624, (0.772954000000, 0.332817600000, 0.000116853300) },
        { 625, (0.751400000000, 0.321000000000, 0.000100000000) },
        { 626, (0.729583600000, 0.309338100000, 0.000086133330) },
        { 627, (0.707588800000, 0.297850400000, 0.000074600000) },
        { 628, (0.685602200000, 0.286593600000, 0.000065000000) },
        { 629, (0.663810400000, 0.275624500000, 0.000056933330) },
        { 630, (0.642400000000, 0.265000000000, 0.000049999990) },
        { 631, (0.621514900000, 0.254763200000, 0.000044160000) },
        { 632, (0.601113800000, 0.244889600000, 0.000039480000) },
        { 633, (0.581105200000, 0.235334400000, 0.000035720000) },
        { 634, (0.561397700000, 0.226052800000, 0.000032640000) },
        { 635, (0.541900000000, 0.217000000000, 0.000030000000) },
        { 636, (0.522599500000, 0.208161600000, 0.000027653330) },
        { 637, (0.503546400000, 0.199548800000, 0.000025560000) },
        { 638, (0.484743600000, 0.191155200000, 0.000023640000) },
        { 639, (0.466193900000, 0.182974400000, 0.000021813330) },
        { 640, (0.447900000000, 0.175000000000, 0.000020000000) },
        { 641, (0.429861300000, 0.167223500000, 0.000018133330) },
        { 642, (0.412098000000, 0.159646400000, 0.000016200000) },
        { 643, (0.394644000000, 0.152277600000, 0.000014200000) },
        { 644, (0.377533300000, 0.145125900000, 0.000012133330) },
        { 645, (0.360800000000, 0.138200000000, 0.000010000000) },
        { 646, (0.344456300000, 0.131500300000, 0.000007733333) },
        { 647, (0.328516800000, 0.125024800000, 0.000005400000) },
        { 648, (0.313019200000, 0.118779200000, 0.000003200000) },
        { 649, (0.298001100000, 0.112769100000, 0.000001333333) },
        { 650, (0.283500000000, 0.107000000000, 0.000000000000) },
        { 651, (0.269544800000, 0.101476200000, 0.000000000000) },
        { 652, (0.256118400000, 0.096188640000, 0.000000000000) },
        { 653, (0.243189600000, 0.091122960000, 0.000000000000) },
        { 654, (0.230727200000, 0.086264850000, 0.000000000000) },
        { 655, (0.218700000000, 0.081600000000, 0.000000000000) },
        { 656, (0.207097100000, 0.077120640000, 0.000000000000) },
        { 657, (0.195923200000, 0.072825520000, 0.000000000000) },
        { 658, (0.185170800000, 0.068710080000, 0.000000000000) },
        { 659, (0.174832300000, 0.064769760000, 0.000000000000) },
        { 660, (0.164900000000, 0.061000000000, 0.000000000000) },
        { 661, (0.155366700000, 0.057396210000, 0.000000000000) },
        { 662, (0.146230000000, 0.053955040000, 0.000000000000) },
        { 663, (0.137490000000, 0.050673760000, 0.000000000000) },
        { 664, (0.129146700000, 0.047549650000, 0.000000000000) },
        { 665, (0.121200000000, 0.044580000000, 0.000000000000) },
        { 666, (0.113639700000, 0.041758720000, 0.000000000000) },
        { 667, (0.106465000000, 0.039084960000, 0.000000000000) },
        { 668, (0.099690440000, 0.036563840000, 0.000000000000) },
        { 669, (0.093330610000, 0.034200480000, 0.000000000000) },
        { 670, (0.087400000000, 0.032000000000, 0.000000000000) },
        { 671, (0.081900960000, 0.029962610000, 0.000000000000) },
        { 672, (0.076804280000, 0.028076640000, 0.000000000000) },
        { 673, (0.072077120000, 0.026329360000, 0.000000000000) },
        { 674, (0.067686640000, 0.024708050000, 0.000000000000) },
        { 675, (0.063600000000, 0.023200000000, 0.000000000000) },
        { 676, (0.059806850000, 0.021800770000, 0.000000000000) },
        { 677, (0.056282160000, 0.020501120000, 0.000000000000) },
        { 678, (0.052971040000, 0.019281080000, 0.000000000000) },
        { 679, (0.049818610000, 0.018120690000, 0.000000000000) },
        { 680, (0.046770000000, 0.017000000000, 0.000000000000) },
        { 681, (0.043784050000, 0.015903790000, 0.000000000000) },
        { 682, (0.040875360000, 0.014837180000, 0.000000000000) },
        { 683, (0.038072640000, 0.013810680000, 0.000000000000) },
        { 684, (0.035404610000, 0.012834780000, 0.000000000000) },
        { 685, (0.032900000000, 0.011920000000, 0.000000000000) },
        { 686, (0.030564190000, 0.011068310000, 0.000000000000) },
        { 687, (0.028380560000, 0.010273390000, 0.000000000000) },
        { 688, (0.026344840000, 0.009533311000, 0.000000000000) },
        { 689, (0.024452750000, 0.008846157000, 0.000000000000) },
        { 690, (0.022700000000, 0.008210000000, 0.000000000000) },
        { 691, (0.021084290000, 0.007623781000, 0.000000000000) },
        { 692, (0.019599880000, 0.007085424000, 0.000000000000) },
        { 693, (0.018237320000, 0.006591476000, 0.000000000000) },
        { 694, (0.016987170000, 0.006138485000, 0.000000000000) },
        { 695, (0.015840000000, 0.005723000000, 0.000000000000) },
        { 696, (0.014790640000, 0.005343059000, 0.000000000000) },
        { 697, (0.013831320000, 0.004995796000, 0.000000000000) },
        { 698, (0.012948680000, 0.004676404000, 0.000000000000) },
        { 699, (0.012129200000, 0.004380075000, 0.000000000000) },
        { 700, (0.011359160000, 0.004102000000, 0.000000000000) },
        { 701, (0.010629350000, 0.003838453000, 0.000000000000) },
        { 702, (0.009938846000, 0.003589099000, 0.000000000000) },
        { 703, (0.009288422000, 0.003354219000, 0.000000000000) },
        { 704, (0.008678854000, 0.003134093000, 0.000000000000) },
        { 705, (0.008110916000, 0.002929000000, 0.000000000000) },
        { 706, (0.007582388000, 0.002738139000, 0.000000000000) },
        { 707, (0.007088746000, 0.002559876000, 0.000000000000) },
        { 708, (0.006627313000, 0.002393244000, 0.000000000000) },
        { 709, (0.006195408000, 0.002237275000, 0.000000000000) },
        { 710, (0.005790346000, 0.002091000000, 0.000000000000) },
        { 711, (0.005409826000, 0.001953587000, 0.000000000000) },
        { 712, (0.005052583000, 0.001824580000, 0.000000000000) },
        { 713, (0.004717512000, 0.001703580000, 0.000000000000) },
        { 714, (0.004403507000, 0.001590187000, 0.000000000000) },
        { 715, (0.004109457000, 0.001484000000, 0.000000000000) },
        { 716, (0.003833913000, 0.001384496000, 0.000000000000) },
        { 717, (0.003575748000, 0.001291268000, 0.000000000000) },
        { 718, (0.003334342000, 0.001204092000, 0.000000000000) },
        { 719, (0.003109075000, 0.001122744000, 0.000000000000) },
        { 720, (0.002899327000, 0.001047000000, 0.000000000000) },
        { 721, (0.002704348000, 0.000976589600, 0.000000000000) },
        { 722, (0.002523020000, 0.000911108800, 0.000000000000) },
        { 723, (0.002354168000, 0.000850133200, 0.000000000000) },
        { 724, (0.002196616000, 0.000793238400, 0.000000000000) },
        { 725, (0.002049190000, 0.000740000000, 0.000000000000) },
        { 726, (0.001910960000, 0.000690082700, 0.000000000000) },
        { 727, (0.001781438000, 0.000643310000, 0.000000000000) },
        { 728, (0.001660110000, 0.000599496000, 0.000000000000) },
        { 729, (0.001546459000, 0.000558454700, 0.000000000000) },
        { 730, (0.001439971000, 0.000520000000, 0.000000000000) },
        { 731, (0.001340042000, 0.000483913600, 0.000000000000) },
        { 732, (0.001246275000, 0.000450052800, 0.000000000000) },
        { 733, (0.001158471000, 0.000418345200, 0.000000000000) },
        { 734, (0.001076430000, 0.000388718400, 0.000000000000) },
        { 735, (0.000999949300, 0.000361100000, 0.000000000000) },
        { 736, (0.000928735800, 0.000335383500, 0.000000000000) },
        { 737, (0.000862433200, 0.000311440400, 0.000000000000) },
        { 738, (0.000800750300, 0.000289165600, 0.000000000000) },
        { 739, (0.000743396000, 0.000268453900, 0.000000000000) },
        { 740, (0.000690078600, 0.000249200000, 0.000000000000) },
        { 741, (0.000640515600, 0.000231301900, 0.000000000000) },
        { 742, (0.000594502100, 0.000214685600, 0.000000000000) },
        { 743, (0.000551864600, 0.000199288400, 0.000000000000) },
        { 744, (0.000512429000, 0.000185047500, 0.000000000000) },
        { 745, (0.000476021300, 0.000171900000, 0.000000000000) },
        { 746, (0.000442453600, 0.000159778100, 0.000000000000) },
        { 747, (0.000411511700, 0.000148604400, 0.000000000000) },
        { 748, (0.000382981400, 0.000138301600, 0.000000000000) },
        { 749, (0.000356649100, 0.000128792500, 0.000000000000) },
        { 750, (0.000332301100, 0.000120000000, 0.000000000000) },
        { 751, (0.000309758600, 0.000111859500, 0.000000000000) },
        { 752, (0.000288887100, 0.000104322400, 0.000000000000) },
        { 753, (0.000269539400, 0.000097335600, 0.000000000000) },
        { 754, (0.000251568200, 0.000090845870, 0.000000000000) },
        { 755, (0.000234826100, 0.000084800000, 0.000000000000) },
        { 756, (0.000219171000, 0.000079146670, 0.000000000000) },
        { 757, (0.000204525800, 0.000073858000, 0.000000000000) },
        { 758, (0.000190840500, 0.000068916000, 0.000000000000) },
        { 759, (0.000178065400, 0.000064302670, 0.000000000000) },
        { 760, (0.000166150500, 0.000060000000, 0.000000000000) },
        { 761, (0.000155023600, 0.000055981870, 0.000000000000) },
        { 762, (0.000144621900, 0.000052225600, 0.000000000000) },
        { 763, (0.000134909800, 0.000048718400, 0.000000000000) },
        { 764, (0.000125852000, 0.000045447470, 0.000000000000) },
        { 765, (0.000117413000, 0.000042400000, 0.000000000000) },
        { 766, (0.000109551500, 0.000039561040, 0.000000000000) },
        { 767, (0.000102224500, 0.000036915120, 0.000000000000) },
        { 768, (0.000095394450, 0.000034448680, 0.000000000000) },
        { 769, (0.000089023900, 0.000032148160, 0.000000000000) },
        { 770, (0.000083075270, 0.000030000000, 0.000000000000) },
        { 771, (0.000077512690, 0.000027991250, 0.000000000000) },
        { 772, (0.000072313040, 0.000026113560, 0.000000000000) },
        { 773, (0.000067457780, 0.000024360240, 0.000000000000) },
        { 774, (0.000062928440, 0.000022724610, 0.000000000000) },
        { 775, (0.000058706520, 0.000021200000, 0.000000000000) },
        { 776, (0.000054770280, 0.000019778550, 0.000000000000) },
        { 777, (0.000051099180, 0.000018452850, 0.000000000000) },
        { 778, (0.000047676540, 0.000017216870, 0.000000000000) },
        { 779, (0.000044485670, 0.000016064590, 0.000000000000) },
        { 780, (0.000041509940, 0.000014990000, 0.000000000000) },
        { 781, (0.000038733240, 0.000013987280, 0.000000000000) },
        { 782, (0.000036142030, 0.000013051550, 0.000000000000) },
        { 783, (0.000033723520, 0.000012178180, 0.000000000000) },
        { 784, (0.000031464870, 0.000011362540, 0.000000000000) },
        { 785, (0.000029353260, 0.000010600000, 0.000000000000) },
        { 786, (0.000027375730, 0.000009885877, 0.000000000000) },
        { 787, (0.000025524330, 0.000009217304, 0.000000000000) },
        { 788, (0.000023793760, 0.000008592362, 0.000000000000) },
        { 789, (0.000022178700, 0.000008009133, 0.000000000000) },
        { 790, (0.000020673830, 0.000007465700, 0.000000000000) },
        { 791, (0.000019272260, 0.000006959567, 0.000000000000) },
        { 792, (0.000017966400, 0.000006487995, 0.000000000000) },
        { 793, (0.000016749910, 0.000006048699, 0.000000000000) },
        { 794, (0.000015616480, 0.000005639396, 0.000000000000) },
        { 795, (0.000014559770, 0.000005257800, 0.000000000000) },
        { 796, (0.000013573870, 0.000004901771, 0.000000000000) },
        { 797, (0.000012654360, 0.000004569720, 0.000000000000) },
        { 798, (0.000011797230, 0.000004260194, 0.000000000000) },
        { 799, (0.000010998440, 0.000003971739, 0.000000000000) },
        { 800, (0.000010253980, 0.000003702900, 0.000000000000) },
        { 801, (0.000009559646, 0.000003452163, 0.000000000000) },
        { 802, (0.000008912044, 0.000003218302, 0.000000000000) },
        { 803, (0.000008308358, 0.000003000300, 0.000000000000) },
        { 804, (0.000007745769, 0.000002797139, 0.000000000000) },
        { 805, (0.000007221456, 0.000002607800, 0.000000000000) },
        { 806, (0.000006732475, 0.000002431220, 0.000000000000) },
        { 807, (0.000006276423, 0.000002266531, 0.000000000000) },
        { 808, (0.000005851304, 0.000002113013, 0.000000000000) },
        { 809, (0.000005455118, 0.000001969943, 0.000000000000) },
        { 810, (0.000005085868, 0.000001836600, 0.000000000000) },
        { 811, (0.000004741466, 0.000001712230, 0.000000000000) },
        { 812, (0.000004420236, 0.000001596228, 0.000000000000) },
        { 813, (0.000004120783, 0.000001488090, 0.000000000000) },
        { 814, (0.000003841716, 0.000001387314, 0.000000000000) },
        { 815, (0.000003581652, 0.000001293400, 0.000000000000) },
        { 816, (0.000003339127, 0.000001205820, 0.000000000000) },
        { 817, (0.000003112949, 0.000001124143, 0.000000000000) },
        { 818, (0.000002902121, 0.000001048009, 0.000000000000) },
        { 819, (0.000002705645, 0.000000977058, 0.000000000000) },
        { 820, (0.000002522525, 0.000000910930, 0.000000000000) },
        { 821, (0.000002351726, 0.000000849251, 0.000000000000) },
        { 822, (0.000002192415, 0.000000791721, 0.000000000000) },
        { 823, (0.000002043902, 0.000000738090, 0.000000000000) },
        { 824, (0.000001905497, 0.000000688110, 0.000000000000) },
        { 825, (0.000001776509, 0.000000641530, 0.000000000000) },
        { 826, (0.000001656215, 0.000000598089, 0.000000000000) },
        { 827, (0.000001544022, 0.000000557575, 0.000000000000) },
        { 828, (0.000001439440, 0.000000519808, 0.000000000000) },
        { 829, (0.000001341977, 0.000000484612, 0.000000000000) },
        { 830, (0.000001251141, 0.000000451810, 0.000000000000) },
    };

        //CIE 标准光源 D65功率分布
        private static readonly Dictionary<int, double> CIE_std_D65 = new Dictionary<int, double>
    {
        { 300,000.03410}, { 301,000.36014}, { 302,000.68618}, { 303,001.01222},
        { 304,001.33826}, { 305,001.66430}, { 306,001.99034}, { 307,002.31638},
        { 308,002.64242}, { 309,002.96846}, { 310,003.29450}, { 311,004.98865},
        { 312,006.68280}, { 313,008.37695}, { 314,010.07110}, { 315,011.76520},
        { 316,013.45940}, { 317,015.15350}, { 318,016.84770}, { 319,018.54180},
        { 320,020.23600}, { 321,021.91770}, { 322,023.59950}, { 323,025.28120},
        { 324,026.96300}, { 325,028.64470}, { 326,030.32650}, { 327,032.00820},
        { 328,033.69000}, { 329,035.37170}, { 330,037.05350}, { 331,037.34300},
        { 332,037.63260}, { 333,037.92210}, { 334,038.21160}, { 335,038.50110},
        { 336,038.79070}, { 337,039.08020}, { 338,039.36970}, { 339,039.65930},
        { 340,039.94880}, { 341,040.44510}, { 342,040.94140}, { 343,041.43770},
        { 344,041.93400}, { 345,042.43020}, { 346,042.92650}, { 347,043.42280},
        { 348,043.91910}, { 349,044.41540}, { 350,044.91170}, { 351,045.08440},
        { 352,045.25700}, { 353,045.42970}, { 354,045.60230}, { 355,045.77500},
        { 356,045.94770}, { 357,046.12030}, { 358,046.29300}, { 359,046.46560},
        { 360,046.63830}, { 361,047.18340}, { 362,047.72850}, { 363,048.27350},
        { 364,048.81860}, { 365,049.36370}, { 366,049.90880}, { 367,050.45390},
        { 368,050.99890}, { 369,051.54400}, { 370,052.08910}, { 371,051.87770},
        { 372,051.66640}, { 373,051.45500}, { 374,051.24370}, { 375,051.03230},
        { 376,050.82090}, { 377,050.60960}, { 378,050.39820}, { 379,050.18690},
        { 380,049.97550}, { 381,050.44280}, { 382,050.91000}, { 383,051.37730},
        { 384,051.84460}, { 385,052.31180}, { 386,052.77910}, { 387,053.24640},
        { 388,053.71370}, { 389,054.18090}, { 390,054.64820}, { 391,057.45890},
        { 392,060.26950}, { 393,063.08020}, { 394,065.89090}, { 395,068.70150},
        { 396,071.51220}, { 397,074.32290}, { 398,077.13360}, { 399,079.94420},
        { 400,082.75490}, { 401,083.62800}, { 402,084.50110}, { 403,085.37420},
        { 404,086.24730}, { 405,087.12040}, { 406,087.99360}, { 407,088.86670},
        { 408,089.73980}, { 409,090.61290}, { 410,091.48600}, { 411,091.68060},
        { 412,091.87520}, { 413,092.06970}, { 414,092.26430}, { 415,092.45890},
        { 416,092.65350}, { 417,092.84810}, { 418,093.04260}, { 419,093.23720},
        { 420,093.43180}, { 421,092.75680}, { 422,092.08190}, { 423,091.40690},
        { 424,090.73200}, { 425,090.05700}, { 426,089.38210}, { 427,088.70710},
        { 428,088.03220}, { 429,087.35720}, { 430,086.68230}, { 431,088.50060},
        { 432,090.31880}, { 433,092.13710}, { 434,093.95540}, { 435,095.77360},
        { 436,097.59190}, { 437,099.41020}, { 438,101.22800}, { 439,103.04700},
        { 440,104.86500}, { 441,106.07900}, { 442,107.29400}, { 443,108.50800},
        { 444,109.72200}, { 445,110.93600}, { 446,112.15100}, { 447,113.36500},
        { 448,114.57900}, { 449,115.79400}, { 450,117.00800}, { 451,117.08800},
        { 452,117.16900}, { 453,117.24900}, { 454,117.33000}, { 455,117.41000},
        { 456,117.49000}, { 457,117.57100}, { 458,117.65100}, { 459,117.73200},
        { 460,117.81200}, { 461,117.51700}, { 462,117.22200}, { 463,116.92700},
        { 464,116.63200}, { 465,116.33600}, { 466,116.04100}, { 467,115.74600},
        { 468,115.45100}, { 469,115.15600}, { 470,114.86100}, { 471,114.96700},
        { 472,115.07300}, { 473,115.18000}, { 474,115.28600}, { 475,115.39200},
        { 476,115.49800}, { 477,115.60400}, { 478,115.71100}, { 479,115.81700},
        { 480,115.92300}, { 481,115.21200}, { 482,114.50100}, { 483,113.78900},
        { 484,113.07800}, { 485,112.36700}, { 486,111.65600}, { 487,110.94500},
        { 488,110.23300}, { 489,109.52200}, { 490,108.81100}, { 491,108.86500},
        { 492,108.92000}, { 493,108.97400}, { 494,109.02800}, { 495,109.08200},
        { 496,109.13700}, { 497,109.19100}, { 498,109.24500}, { 499,109.30000},
        { 500,109.35400}, { 501,109.19900}, { 502,109.04400}, { 503,108.88800},
        { 504,108.73300}, { 505,108.57800}, { 506,108.42300}, { 507,108.26800},
        { 508,108.11200}, { 509,107.95700}, { 510,107.80200}, { 511,107.50100},
        { 512,107.20000}, { 513,106.89800}, { 514,106.59700}, { 515,106.29600},
        { 516,105.99500}, { 517,105.69400}, { 518,105.39200}, { 519,105.09100},
        { 520,104.79000}, { 521,105.08000}, { 522,105.37000}, { 523,105.66000},
        { 524,105.95000}, { 525,106.23900}, { 526,106.52900}, { 527,106.81900},
        { 528,107.10900}, { 529,107.39900}, { 530,107.68900}, { 531,107.36100},
        { 532,107.03200}, { 533,106.70400}, { 534,106.37500}, { 535,106.04700},
        { 536,105.71900}, { 537,105.39000}, { 538,105.06200}, { 539,104.73300},
        { 540,104.40500}, { 541,104.36900}, { 542,104.33300}, { 543,104.29700},
        { 544,104.26100}, { 545,104.22500}, { 546,104.19000}, { 547,104.15400},
        { 548,104.11800}, { 549,104.08200}, { 550,104.04600}, { 551,103.64100},
        { 552,103.23700}, { 553,102.83200}, { 554,102.42800}, { 555,102.02300},
        { 556,101.61800}, { 557,101.21400}, { 558,100.80900}, { 559,100.40500},
        { 560,100.00000}, { 561,099.63340}, { 562,099.26680}, { 563,098.90030},
        { 564,098.53370}, { 565,098.16710}, { 566,097.80050}, { 567,097.43390},
        { 568,097.06740}, { 569,096.70080}, { 570,096.33420}, { 571,096.27960},
        { 572,096.22500}, { 573,096.17030}, { 574,096.11570}, { 575,096.06110},
        { 576,096.00650}, { 577,095.95190}, { 578,095.89720}, { 579,095.84260},
        { 580,095.78800}, { 581,095.07780}, { 582,094.36750}, { 583,093.65730},
        { 584,092.94700}, { 585,092.23680}, { 586,091.52660}, { 587,090.81630},
        { 588,090.10610}, { 589,089.39580}, { 590,088.68560}, { 591,088.81770},
        { 592,088.94970}, { 593,089.08180}, { 594,089.21380}, { 595,089.34590},
        { 596,089.47800}, { 597,089.61000}, { 598,089.74210}, { 599,089.87410},
        { 600,090.00620}, { 601,089.96550}, { 602,089.92480}, { 603,089.88410},
        { 604,089.84340}, { 605,089.80260}, { 606,089.76190}, { 607,089.72120},
        { 608,089.68050}, { 609,089.63980}, { 610,089.59910}, { 611,089.40910},
        { 612,089.21900}, { 613,089.02900}, { 614,088.83890}, { 615,088.64890},
        { 616,088.45890}, { 617,088.26880}, { 618,088.07880}, { 619,087.88870},
        { 620,087.69870}, { 621,087.25770}, { 622,086.81670}, { 623,086.37570},
        { 624,085.93470}, { 625,085.49360}, { 626,085.05260}, { 627,084.61160},
        { 628,084.17060}, { 629,083.72960}, { 630,083.28860}, { 631,083.32970},
        { 632,083.37070}, { 633,083.41180}, { 634,083.45280}, { 635,083.49390},
        { 636,083.53500}, { 637,083.57600}, { 638,083.61710}, { 639,083.65810},
        { 640,083.69920}, { 641,083.33200}, { 642,082.96470}, { 643,082.59750},
        { 644,082.23020}, { 645,081.86300}, { 646,081.49580}, { 647,081.12850},
        { 648,080.76130}, { 649,080.39400}, { 650,080.02680}, { 651,080.04560},
        { 652,080.06440}, { 653,080.08310}, { 654,080.10190}, { 655,080.12070},
        { 656,080.13950}, { 657,080.15830}, { 658,080.17700}, { 659,080.19580},
        { 660,080.21460}, { 661,080.42090}, { 662,080.62720}, { 663,080.83360},
        { 664,081.03990}, { 665,081.24620}, { 666,081.45250}, { 667,081.65880},
        { 668,081.86520}, { 669,082.07150}, { 670,082.27780}, { 671,081.87840},
        { 672,081.47910}, { 673,081.07970}, { 674,080.68040}, { 675,080.28100},
        { 676,079.88160}, { 677,079.48230}, { 678,079.08290}, { 679,078.68360},
        { 680,078.28420}, { 681,077.42790}, { 682,076.57160}, { 683,075.71530},
        { 684,074.85900}, { 685,074.00270}, { 686,073.14650}, { 687,072.29020},
        { 688,071.43390}, { 689,070.57760}, { 690,069.72130}, { 691,069.91010},
        { 692,070.09890}, { 693,070.28760}, { 694,070.47640}, { 695,070.66520},
        { 696,070.85400}, { 697,071.04280}, { 698,071.23150}, { 699,071.42030},
        { 700,071.60910}, { 701,071.88310}, { 702,072.15710}, { 703,072.43110},
        { 704,072.70510}, { 705,072.97900}, { 706,073.25300}, { 707,073.52700},
        { 708,073.80100}, { 709,074.07500}, { 710,074.34900}, { 711,073.07450},
        { 712,071.80000}, { 713,070.52550}, { 714,069.25100}, { 715,067.97650},
        { 716,066.70200}, { 717,065.42750}, { 718,064.15300}, { 719,062.87850},
        { 720,061.60400}, { 721,062.43220}, { 722,063.26030}, { 723,064.08850},
        { 724,064.91660}, { 725,065.74480}, { 726,066.57300}, { 727,067.40110},
        { 728,068.22930}, { 729,069.05740}, { 730,069.88560}, { 731,070.40570},
        { 732,070.92590}, { 733,071.44600}, { 734,071.96620}, { 735,072.48630},
        { 736,073.00640}, { 737,073.52660}, { 738,074.04670}, { 739,074.56690},
        { 740,075.08700}, { 741,073.93760}, { 742,072.78810}, { 743,071.63870},
        { 744,070.48930}, { 745,069.33980}, { 746,068.19040}, { 747,067.04100},
        { 748,065.89160}, { 749,064.74210}, { 750,063.59270}, { 751,061.87520},
        { 752,060.15780}, { 753,058.44030}, { 754,056.72290}, { 755,055.00540},
        { 756,053.28800}, { 757,051.57050}, { 758,049.85310}, { 759,048.13560},
        { 760,046.41820}, { 761,048.45690}, { 762,050.49560}, { 763,052.53440},
        { 764,054.57310}, { 765,056.61180}, { 766,058.65050}, { 767,060.68920},
        { 768,062.72800}, { 769,064.76670}, { 770,066.80540}, { 771,066.46310},
        { 772,066.12090}, { 773,065.77860}, { 774,065.43640}, { 775,065.09410},
        { 776,064.75180}, { 777,064.40960}, { 778,064.06730}, { 779,063.72510},
        { 780,063.38280}, { 781,063.47490}, { 782,063.56700}, { 783,063.65920},
        { 784,063.75130}, { 785,063.84340}, { 786,063.93550}, { 787,064.02760},
        { 788,064.11980}, { 789,064.21190}, { 790,064.30400}, { 791,063.81880},
        { 792,063.33360}, { 793,062.84840}, { 794,062.36320}, { 795,061.87790},
        { 796,061.39270}, { 797,060.90750}, { 798,060.42230}, { 799,059.93710},
        { 800,059.45190}, { 801,058.70260}, { 802,057.95330}, { 803,057.20400},
        { 804,056.45470}, { 805,055.70540}, { 806,054.95620}, { 807,054.20690},
        { 808,053.45760}, { 809,052.70830}, { 810,051.95900}, { 811,052.50720},
        { 812,053.05530}, { 813,053.60350}, { 814,054.15160}, { 815,054.69980},
        { 816,055.24800}, { 817,055.79610}, { 818,056.34430}, { 819,056.89240},
        { 820,057.44060}, { 821,057.72780}, { 822,058.01500}, { 823,058.30220},
        { 824,058.58940}, { 825,058.87650}, { 826,059.16370}, { 827,059.45090},
        { 828,059.73810}, { 829,060.02530}, { 830,060.31250},
    };
        #endregion
        private const int Decimal = 6;
        #region 计算LAB
        /// <summary>
        /// 计算LAB值
        /// </summary>
        /// <param name="absorbanceData">吸光度数组</param>
        /// <param name="startWavelength">起始波长</param>
        /// <param name="endWavelength">结束波长</param>
        /// <param name="interval">波长间隔</param>
        /// <param name="angle">观察者角度</param>
        /// <returns>LAB值</returns>
        public static (double L, double A, double B) CalculateLAB(double[] absorbanceData, int startWavelength, int endWavelength, int interval, int angle)
        {
            var cieData = angle == 10 ? CIE_1964_10 : CIE_1931_2;

            // 假设的参考白色标准反射率数据
            double[] referenceWhite = new double[absorbanceData.Length];
            for (int i = 0; i < referenceWhite.Length; i++)
            {
                referenceWhite[i] = 1.0; // 假设完全反射
            }

            // 转换吸光度为反射率
            double[] reflectance = new double[absorbanceData.Length];
            for (int i = 0; i < absorbanceData.Length; i++)
            {
                reflectance[i] = Math.Round(Math.Pow(10, -absorbanceData[i]), Decimal);
            }

            // 获取 CIE 色度匹配函数的 Y 值集合，并确保波长匹配
            var cieYValues = cieData.Where(kvp => CIE_std_D65.ContainsKey(kvp.Key))
                                    .Select(kvp => Math.Round(kvp.Value.Y, Decimal))
                                    .ToList();

            // 获取匹配波长的 D65 标准光源光谱功率分布值集合
            var d65Values = cieData.Keys.Where(wavelength => CIE_std_D65.ContainsKey(wavelength))
                                        .Select(wavelength => Math.Round(CIE_std_D65[wavelength], Decimal))
                                        .ToList();

            Console.WriteLine("D65 Values: " + string.Join(", ", d65Values));

            //  将两个集合按顺序配对
            var pairedValues = cieYValues.Zip(d65Values, (cieY, d65) => new { cieY, d65 }).ToList();

            //  计算每对元素的乘积
            var products = pairedValues.Select(p => Math.Round(p.cieY * p.d65, Decimal)).ToList();

            // 对所有乘积求和
            var sum = Math.Round(products.Sum(), Decimal);


            // 计算 k 值
            double k = Math.Round((100.0 / sum) * interval, Decimal);

            // 计算XYZ值
            double X1 = 0, Y1 = 0, Z1 = 0;

            for (int i = 0; i < reflectance.Length; i++)
            {
                int wavelength = startWavelength + i * interval;
                if (cieData.ContainsKey(wavelength) && CIE_std_D65.ContainsKey(wavelength))
                {
                    double x = Math.Round(cieData[wavelength].X, Decimal);
                    double y = Math.Round(cieData[wavelength].Y, Decimal);
                    double z = Math.Round(cieData[wavelength].Z, Decimal);

                    double stdD65 = Math.Round(CIE_std_D65[wavelength], Decimal);

                    X1 += Math.Round(reflectance[i] * x * stdD65, Decimal);
                    Y1 += Math.Round(reflectance[i] * y * stdD65, Decimal);
                    Z1 += Math.Round(reflectance[i] * z * stdD65, Decimal);
                }
            }
            X1 = Math.Round(X1 * k, Decimal);
            Y1 = Math.Round(Y1 * k, Decimal);
            Z1 = Math.Round(Z1 * k, Decimal);
            // 转换XYZ值为LAB值
            double Xn = 95.047, Yn = 100.000, Zn = 108.883; // D65标准光源下的参考白色

            if (angle == 10)
            {
                Xn = 94.81;
                Yn = 100.000;
                Zn = 107.31;
            }
            else
            {
                Xn = 95.047;
                Yn = 100.000;
                Zn = 108.883;
            }
            double fx = Math.Round(f(X1 / Xn), Decimal);
            double fy = Math.Round(f(Y1 / Yn), Decimal);
            double fz = Math.Round(f(Z1 / Zn), Decimal);

            double L = Math.Round(116 * fy - 16, Decimal);
            double A = Math.Round(500 * (fx - fy), Decimal);
            double B = Math.Round(200 * (fy - fz), Decimal);

            return (L, A, B);
        }

        private static double f(double t)
        {
            return t > Math.Round(Math.Pow(6.0 / 29.0, 3), Decimal) ?
                Math.Round(Math.Pow(t, 1.0 / 3.0), Decimal) :
                Math.Round((1.0 / 3.0) * Math.Pow(29.0 / 6.0, 2) * t + 4.0 / 29.0, Decimal);
        }

        #endregion

        #region 计算DECMC
        public static double CalculateCMC(double LStarTarget, double aStarTarget, double bStarTarget,
                                          double LStarSample, double aStarSample, double bStarSample,
                                          double lFactor, double cFactor)
        {
            // 计算差值  
            double DL = Math.Round(LStarSample - LStarTarget, Decimal);
            double Da = Math.Round(aStarSample - aStarTarget, Decimal);
            double Db = Math.Round(bStarSample - bStarTarget, Decimal);

            // 计算C*ab  
            double CStarAbTarget = Math.Round(Math.Sqrt(Math.Pow(aStarTarget, 2) + Math.Pow(bStarTarget, 2)), Decimal);
            double CStarAbSample = Math.Round(Math.Sqrt(Math.Pow(aStarSample, 2) + Math.Pow(bStarSample, 2)), Decimal);
            double DCab = Math.Round(CStarAbSample - CStarAbTarget, Decimal);

            //计算hab
            double HabT = CalculateHAB(aStarTarget, bStarTarget);
            double HabS = CalculateHAB(aStarSample, bStarSample);
            double DHab = ((HabS - HabT) <= 180 ? CalculateDHAB(Da, Db, DCab) : -CalculateDHAB(Da, Db, DCab));

            // 计算SL, SC, SH, F, T  
            double SL = CalculateSL(LStarTarget);
            double SC = CalculateSC(CStarAbTarget);
            double T = CalculateT(HabT);
            double F = CalculateF(CStarAbTarget);
            double SH = Math.Round(SC * (1 + T * F - F), Decimal);
            double FValue = CalculateF(CStarAbTarget);

            // 计算DECMC  DEcmc = [(DL*/lSL)2 + (DC*ab/cSC)2 + (DH*ab/SH)2]1/2
            double DECMC = Math.Round(Math.Sqrt(Math.Pow(DL / (lFactor * SL), 2) +
                                     Math.Pow(DCab / (cFactor * SC), 2) +
                                     Math.Pow(DHab / SH, 2)), Decimal);

            return DECMC;
        }
        private static double CalculateSL(double LStar)
        {
            if (LStar < 16)
            {
                return 0.511;
            }
            return Math.Round(0.040975 * LStar / (1 + 0.01765 * LStar), Decimal);
        }



        /// <summary>  
        /// 根据A和B的值计算角度。  
        /// </summary>  
        /// <param name="A">A</param>  
        /// <param name="B">B</param>  
        /// <returns>计算得到的角度（度）</returns>  
        private static double CalculateHAB(double A, double B)
        {
            if (A == 0 && B == 0)
            {
                return 0;
            }
            else if (A == 0 && B > 0)
            {
                return 90;
            }
            else if (A == 0 && B < 0)
            {
                return 270;
            }
            //else if (aStarTarget > 0 && bStarTarget >= 0)
            //{
            //    return (360 * Math.Atan2(bStarTarget, aStarTarget) / Math.PI / 2);
            //}
            //else if(aStarTarget < 0)
            //{
            //    return (180+360 * Math.Atan2(bStarTarget, aStarTarget) / Math.PI / 2);
            //}
            else
            {
                // 处理标样A不为0的情况  
                double angleRadians = Math.Atan2(B, A); // 使用Atan2来处理所有象限  
                double angleDegrees = angleRadians * (180 / Math.PI); // 将弧度转换为度  

                // 如果B5是负数，我们不需要额外加180或360，因为Atan2已经处理了象限  
                // 但如果需要特定的输出范围（例如0-360度），我们可以添加适当的调整  
                // 例如，确保角度在0-360度之间  
                if (angleDegrees < 0)
                {
                    angleDegrees += 360;
                }

                return Math.Round(angleDegrees, Decimal);
            }
        }

        private static double CalculateDHAB(double DA, double DB, double DCab)
        {
            double DHAB = Math.Round(Math.Sqrt(Math.Pow(DA, 2) + Math.Pow(DB, 2) - Math.Pow(DCab, 2)), Decimal);
            return DHAB;
        }

        private static double CalculateT(double HabT)
        {
            double value = 0;
            if (HabT > 164 && HabT < 345)
            {
                value = 0.56 + Math.Abs(0.2 * Math.Cos(Math.PI / (180 / (168 + HabT))));

            }
            else
            {
                value = 0.36 + Math.Abs(0.4 * Math.Cos(Math.PI / (180 / (35 + HabT))));
            }
            return Math.Round(value, Decimal);
        }

        private static double CalculateSC(double CStarAb)
        {
            return Math.Round(0.0638 * CStarAb / (1 + 0.0131 * CStarAb) + 0.638, Decimal);
        }

        private static double CalculateF(double CStarAb)
        {
            double d0 = Math.Round(Math.Pow(CStarAb, 4), Decimal);
            double d1 = Math.Round(Math.Pow(CStarAb, 4) + 1900, Decimal);
            double d = Math.Round(Math.Sqrt(d0 / d1), Decimal);
            return d;
        }

        #endregion

        #region 试样相对于标样的色度力份/强度/着色力
        // 计算 K/S 值
        private static double CalculateKS(double reflectance)
        {
            return Math.Round(Math.Pow(1 - reflectance, 2) / (2 * reflectance), Decimal);
        }

        /// <summary>
        /// 强度SWL(最大吸收波长处)
        /// </summary>
        /// <param name="sampleAbsorbance">试样吸光度</param>
        /// <param name="standardAbsorbance">标样吸光度</param>
        /// <returns>强度SWL百分比</returns>
        /// <exception cref="ArgumentException"></exception>
        public static double SWL(double[] sampleAbsorbance, double[] standardAbsorbance)
        {
            if (sampleAbsorbance.Length != standardAbsorbance.Length)
            {
                throw new ArgumentException("样品和标准的吸光度数据长度必须相同");
            }

            // 找到最大吸收波长的数据
            int maxAbsorbanceIndexSample = Array.IndexOf(sampleAbsorbance, sampleAbsorbance.Max());
            int maxAbsorbanceIndexStandard = Array.IndexOf(standardAbsorbance, standardAbsorbance.Max());
            double sampleReflectance;
            double standardReflectance;
            // 将吸光度转化为反射率
            if (maxAbsorbanceIndexSample != maxAbsorbanceIndexStandard)
            {
                sampleReflectance = Math.Round(Math.Pow(10, -sampleAbsorbance[maxAbsorbanceIndexStandard]), Decimal);
                standardReflectance = Math.Round(Math.Pow(10, -standardAbsorbance[maxAbsorbanceIndexStandard]), Decimal);
            }
            else
            {
                sampleReflectance = Math.Round(Math.Pow(10, -sampleAbsorbance[maxAbsorbanceIndexSample]), Decimal);
                standardReflectance = Math.Round(Math.Pow(10, -standardAbsorbance[maxAbsorbanceIndexStandard]), Decimal);
            }

            // 计算色差强度
            double ksSample = Math.Round(CalculateKS(sampleReflectance), Decimal);
            double ksStandard = Math.Round(CalculateKS(standardReflectance), Decimal);

            return Math.Round((ksSample / ksStandard) * 100, Decimal);
        }


        /// <summary>
        /// 强度总和
        /// </summary>
        /// <param name="sampleAbsorbance">试样吸光度</param>
        /// <param name="standardAbsorbance">标样吸光度</param>
        /// <param name="startWavelength">起始波长</param>
        /// <param name="endWavelength">结束波长</param>
        /// <param name="interval">波长间隔</param>
        /// <returns>强度SUM百分比</returns>
        /// <exception cref="ArgumentException"></exception>
        public static double SUM(double[] sampleAbsorbance, double[] standardAbsorbance, int startWavelength, int endWavelength, int interval)
        {
            if (sampleAbsorbance.Length != standardAbsorbance.Length)
            {
                throw new ArgumentException("样品和标准的吸光度数据长度必须相同");
            }

            double totalStrength = 0;
            double totalStandard = 0;
            int length = sampleAbsorbance.Length;

            for (int i = 0; i < length; i++)
            {
                int currentWavelength = startWavelength + i * interval;
                if (currentWavelength >= 400 && currentWavelength <= 700)
                {
                    // 将吸光度转化为反射率
                    double sampleReflectance = Math.Round(Math.Pow(10, -sampleAbsorbance[i]), Decimal);
                    double standardReflectance = Math.Round(Math.Pow(10, -standardAbsorbance[i]), Decimal);

                    // 计算 K/S 值
                    double ksSample = CalculateKS(sampleReflectance);
                    double ksStandard = CalculateKS(standardReflectance);

                    // 计算并累加色差强度
                    totalStrength = Math.Round(totalStrength + ksSample, Decimal);
                    totalStandard = Math.Round(totalStandard + ksStandard, Decimal);
                }
            }

            // 返回平均色差强度
            return Math.Round((totalStrength / totalStandard) * 100, Decimal);
        }

        /// <summary>
        /// 强度WSUM
        /// </summary>
        /// <param name="sampleAbsorbance">试样吸光度</param>
        /// <param name="standardAbsorbance">标样吸光度</param>
        /// <param name="startWavelength">起始波长</param>
        /// <param name="endWavelength">结束波长</param>
        /// <param name="interval">波长间隔</param>
        /// <returns>强度WSUM百分比</returns>
        /// <exception cref="ArgumentException"></exception>
        public static double WSUM(double[] sampleAbsorbance, double[] standardAbsorbance, int startWavelength, int endWavelength, int interval, int angle)
        {
            if (sampleAbsorbance.Length != standardAbsorbance.Length)
            {
                throw new ArgumentException("样品和标准的吸光度数据长度必须相同");
            }

            double totalStrength = 0;
            double totalStandard = 0;
            int length = sampleAbsorbance.Length;

            for (int i = 0; i < length; i++)
            {
                int currentWavelength = startWavelength + i * interval;
                if (currentWavelength >= 400 && currentWavelength <= 700)
                {
                    // 将吸光度转化为反射率
                    double sampleReflectance = Math.Round(Math.Pow(10, -sampleAbsorbance[i]), Decimal);
                    double standardReflectance = Math.Round(Math.Pow(10, -standardAbsorbance[i]), Decimal);

                    Dictionary<int, (double X, double Y, double Z)> cieData = angle == 10 ? CIE_1964_10 : CIE_1931_2;
                    double cieX = Math.Round(cieData[currentWavelength].X, Decimal);
                    double cieY = Math.Round(cieData[currentWavelength].Y, Decimal);
                    double cieZ = Math.Round(cieData[currentWavelength].Z, Decimal);
                    double cieStdD65 = Math.Round(CIE_std_D65[currentWavelength], Decimal);
                    double fx = Math.Round(cieStdD65 * cieX, Decimal);
                    double fy = Math.Round(cieStdD65 * cieY, Decimal);
                    double fz = Math.Round(cieStdD65 * cieZ, Decimal);

                    // 计算 K/S 值
                    double ksSample = Math.Round(CalculateKS(sampleReflectance) * (fx + fy + fz), Decimal);
                    double ksStandard = Math.Round(CalculateKS(standardReflectance) * (fx + fy + fz), Decimal);

                    // 计算并累加色差强度
                    totalStrength = Math.Round(totalStrength + ksSample, Decimal);
                    totalStandard = Math.Round(totalStandard + ksStandard, Decimal);
                }
            }

            // 返回平均色差强度
            return Math.Round((totalStrength / totalStandard) * 100, Decimal);
        }



        public static double SUM1(double[] sampleAbsorbance, double[] standardAbsorbance, int startWavelength, int endWavelength, int interval)
        {
            if (sampleAbsorbance.Length != standardAbsorbance.Length)
            {
                throw new ArgumentException("样品和标准的吸光度数据长度必须相同");
            }

            double totalStrength = 0;
            double totalStandard = 0;
            int length = sampleAbsorbance.Length;

            // 找到波峰索引
            int peakIndex = Array.IndexOf(sampleAbsorbance, sampleAbsorbance.Max());
            int peakWavelength = startWavelength + peakIndex * interval;
            int minWavelength = peakWavelength - 100;
            int maxWavelength = peakWavelength + 100;

            for (int i = 0; i < length; i++)
            {
                int currentWavelength = startWavelength + i * interval;
                if (currentWavelength >= minWavelength && currentWavelength <= maxWavelength)
                {
                    double sampleReflectance = Math.Round(Math.Pow(10, -sampleAbsorbance[i]), Decimal);
                    double standardReflectance = Math.Round(Math.Pow(10, -standardAbsorbance[i]), Decimal);

                    double ksSample = CalculateKS(sampleReflectance);
                    double ksStandard = CalculateKS(standardReflectance);

                    totalStrength = Math.Round(totalStrength + ksSample, Decimal);
                    totalStandard = Math.Round(totalStandard + ksStandard, Decimal);
                }
            }

            return totalStandard == 0 ? 0 : Math.Round((totalStrength / totalStandard) * 100, Decimal);
        }

        /// <summary>
        /// 强度WSUM
        /// </summary>
        public static double WSUM1(double[] sampleAbsorbance, double[] standardAbsorbance, int startWavelength, int endWavelength, int interval, int angle)
        {
            if (sampleAbsorbance.Length != standardAbsorbance.Length)
            {
                throw new ArgumentException("样品和标准的吸光度数据长度必须相同");
            }

            double totalStrength = 0;
            double totalStandard = 0;
            int length = sampleAbsorbance.Length;

            // 找到波峰索引
            int peakIndex = Array.IndexOf(sampleAbsorbance, sampleAbsorbance.Max());
            int peakWavelength = startWavelength + peakIndex * interval;
            int minWavelength = peakWavelength - 100;
            int maxWavelength = peakWavelength + 100;

            for (int i = 0; i < length; i++)
            {
                int currentWavelength = startWavelength + i * interval;
                if (currentWavelength >= minWavelength && currentWavelength <= maxWavelength)
                {
                    double sampleReflectance = Math.Round(Math.Pow(10, -sampleAbsorbance[i]), Decimal);
                    double standardReflectance = Math.Round(Math.Pow(10, -standardAbsorbance[i]), Decimal);

                    Dictionary<int, (double X, double Y, double Z)> cieData = angle == 10 ? CIE_1964_10 : CIE_1931_2;
                    double cieX = Math.Round(cieData[currentWavelength].X, Decimal);
                    double cieY = Math.Round(cieData[currentWavelength].Y, Decimal);
                    double cieZ = Math.Round(cieData[currentWavelength].Z, Decimal);
                    double cieStdD65 = Math.Round(CIE_std_D65[currentWavelength], Decimal);
                    double fx = Math.Round(cieStdD65 * cieX, Decimal);
                    double fy = Math.Round(cieStdD65 * cieY, Decimal);
                    double fz = Math.Round(cieStdD65 * cieZ, Decimal);

                    double ksSample = Math.Round(CalculateKS(sampleReflectance) * (fx + fy + fz), Decimal);
                    double ksStandard = Math.Round(CalculateKS(standardReflectance) * (fx + fy + fz), Decimal);

                    totalStrength = Math.Round(totalStrength + ksSample, Decimal);
                    totalStandard = Math.Round(totalStandard + ksStandard, Decimal);
                }
            }

            return totalStandard == 0 ? 0 : Math.Round((totalStrength / totalStandard) * 100, Decimal);
        }
        #endregion

    }
}
