using EasyModbus;
using HslControls;
using Lib_Card;
using Lib_Card.ADT8940A1;
using Lib_Card.ADT8940A1.Module;
using SmartDyeing.FADM_Object;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;

namespace SmartDyeing.FADM_Control
{
    public partial class Debug : UserControl
    {
        public Debug()
        {
            InitializeComponent();
            //FADM_Object.Communal.WriteTcpStatus(false);
            FADM_Object.Communal._b_auto = false;

        }

        ~Debug()
        {
            //FADM_Object.Communal.WriteTcpStatus(true);
            FADM_Object.Communal._b_auto = true;
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
            // 在此添加需要手动释放资源的代码
            //FADM_Object.Communal.WriteTcpStatus(true);
            FADM_Object.Communal._b_auto = true;
        }
   

        private int[] _ia_array = new int[1];

        private char[] _ca_cc;

        private bool _b_istrue = false;
        //异常读取次数
        int _i_count = 0;

        private void button2_Click_1(object sender, EventArgs e)
        {
            int[] ia_array = new int[11];
            int[] ia_array2 = new int[11];
            int[] ia_array3 = new int[11];
            int[] ia_array4 = new int[11];
            //int i_state = FADM_Object.Communal._tcpModBusBrew.Read(400139 , 11, ref _ia_array);
            //FADM_Object.Communal._tcpModBusBrew.Read(100139, 11, ref ia_array2);
            //FADM_Object.Communal._tcpModBusBrew.Read(40139, 11, ref ia_array3);
            int i_state = FADM_Object.Communal._tcpModBusBrew.Read(10139, 11, ref ia_array4);
            if (i_state != -1)
            {
                FADM_Object.Communal._tcpModBusBrew._b_Connect = true;
            }
            else
            {
                FADM_Object.Communal._tcpModBusBrew._b_Connect = false;

                throw new Exception("开料机通讯异常");
            }

            //try {
            //    //调用tcp
            //    int[] _ia_array = { 0, 0};
            //    int i_state = FADM_Object.Communal._tcpModBus.Read(0, 2, ref _ia_array);
            //    //int i_state = FADM_Object.Communal._tcpModBus.Write(0, _ia_array);
            //    if (i_state!=-1) {
            //        string str1 = Convert.ToString(_ia_array[0], 2);
            //        string s_str2 = Convert.ToString(_ia_array[1], 2);
            //        string str3 = str1  + s_str2;
            //        string str4 = str3.Substring(0, 2);
            //        Console.WriteLine(str1+"@@"+s_str2);
            //    }
            //} catch(Exception ex){
            //    Console.WriteLine(ex.Message);
            //}
        }

        bool _b_reconnect=false;

        private void Tmr_Tick(object sender, EventArgs e)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                Lib_Card.ADT8940A1.ADT8940A1_IO adt8940a1io = new ADT8940A1_IO();

                foreach (CheckBox checkBox in this.grp_in.Controls)
                {
                    string sName = checkBox.Name.Substring(3);

                    foreach (PropertyInfo info in adt8940a1io.GetType().GetProperties())
                    {
                        if (info.Name == sName)
                        {
                            int iRes = Lib_Card.CardObject.OA1Input.InPutStatus((int)info.GetValue(adt8940a1io));
                            if (0 == iRes)
                                checkBox.Checked = false;
                            else if (1 == iRes)
                                checkBox.Checked = true;
                            else
                            {
                                FADM_Form.CustomMessageBox.Show("驱动异常", "InPutStatus", MessageBoxButtons.OK, false);
                            }

                            break;
                        }

                    }
                }

                foreach (Button button in this.grp_out.Controls)
                {
                    string sName = button.Name.Substring(3);
                    foreach (PropertyInfo info in adt8940a1io.GetType().GetProperties())
                    {
                        if (info.Name == sName)
                        {
                            int iRes = Lib_Card.CardObject.OA1.ReadOutPut((int)info.GetValue(adt8940a1io));
                            if (0 == iRes)
                                button.ForeColor = Color.Black;
                            else if (1 == iRes)
                                button.ForeColor = Color.Red;
                            else
                            {
                                FADM_Form.CustomMessageBox.Show("驱动异常", "ReadOutPut", MessageBoxButtons.OK, false);
                            }
                            break;
                        }
                    }
                }

                int iValue = 0;
                int iAxisRes = Lib_Card.CardObject.OA1.ReadAxisSpeed(Lib_Card.ADT8940A1.ADT8940A1_IO.Axis_X, ref iValue);
                if (0 == iAxisRes)
                    TxtCSpeedX.Text = iValue.ToString();
                else
                {
                    FADM_Form.CustomMessageBox.Show("驱动异常", "ReadAxisSpeed", MessageBoxButtons.OK, false);
                }

                iAxisRes = Lib_Card.CardObject.OA1.ReadAxisActualPosition(Lib_Card.ADT8940A1.ADT8940A1_IO.Axis_X, ref iValue);
                if (0 == iAxisRes)
                    TxtRPosX.Text = iValue.ToString();
                else
                {
                    FADM_Form.CustomMessageBox.Show("驱动异常", "ReadAxisPosition", MessageBoxButtons.OK, false);
                }

                //iAxisRes = Lib_Card.CardObject.OA1.ReadAxisCommandPosition(Lib_Card.ADT8940A1.ADT8940A1_IO.Axis_X, ref iValue);
                //if (0 == iAxisRes)
                //    TxtCPosX.Text = iValue.ToString();
                //else
                //{
                //    FADM_Form.CustomMessageBox.Show("驱动异常", "ReadAxisPosition", MessageBoxButtons.OK, false);
                //}

                iAxisRes = Lib_Card.CardObject.OA1.ReadAxisSpeed(Lib_Card.ADT8940A1.ADT8940A1_IO.Axis_Y, ref iValue);
                if (0 == iAxisRes)
                    TxtCSpeedY.Text = iValue.ToString();
                else
                {
                    FADM_Form.CustomMessageBox.Show("驱动异常", ".ReadAxisSpeed", MessageBoxButtons.OK, false);
                }

                iAxisRes = Lib_Card.CardObject.OA1.ReadAxisActualPosition(Lib_Card.ADT8940A1.ADT8940A1_IO.Axis_Y, ref iValue);
                if (0 == iAxisRes)
                    TxtRPosY.Text = iValue.ToString();
                else
                {
                    FADM_Form.CustomMessageBox.Show("驱动异常", "ReadAxisPosition", MessageBoxButtons.OK, false);
                }

                //iAxisRes = Lib_Card.CardObject.OA1.ReadAxisCommandPosition(Lib_Card.ADT8940A1.ADT8940A1_IO.Axis_Y, ref iValue);
                //if (0 == iAxisRes)
                //    TxtCPosY.Text = iValue.ToString();
                //else
                //{
                //    FADM_Form.CustomMessageBox.Show("驱动异常", "ReadAxisPosition", MessageBoxButtons.OK, false);
                //}

                iAxisRes = Lib_Card.CardObject.OA1.ReadAxisSpeed(Lib_Card.ADT8940A1.ADT8940A1_IO.Axis_Z, ref iValue);
                if (0 == iAxisRes)
                    TxtCSpeedZ.Text = iValue.ToString();
                else
                {
                    FADM_Form.CustomMessageBox.Show("驱动异常", ".ReadAxisSpeed", MessageBoxButtons.OK, false);
                }

                iAxisRes = Lib_Card.CardObject.OA1.ReadAxisCommandPosition(Lib_Card.ADT8940A1.ADT8940A1_IO.Axis_Z, ref iValue);
                if (0 == iAxisRes)
                    TxtCPosZ.Text = iValue.ToString();
                else
                {
                    FADM_Form.CustomMessageBox.Show("驱动异常", "ReadAxisPosition", MessageBoxButtons.OK, false);
                }


                LabBalanceValue.Text = Lib_Card.Configure.Parameter.Machine_BalanceType == 0 ? string.Format("{0:F2}", Lib_SerialPort.Balance.METTLER.BalanceValue) : string.Format("{0:F3}", Lib_SerialPort.Balance.SHINKO.BalanceValue);
            }
            else
            {
                if (_b_reconnect)
                { return; }
                try
                {
                    //if (!_b_istrue)
                    {
                        //while (true)
                        //{
                        //    //当没进行写入时可以读取数据
                        //    if (!Communal._b_isWriting)
                        //        break;
                        //}
                        //tcp读取900到928
                        int[] ia_array = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        int i_state = 0;
                        if (!FADM_Object.Communal.ReadTcpStatus())
                        {
                            while (true)
                            {
                                if (Communal._b_isUpdateNewData)
                                    break;
                                Thread.Sleep(50);
                            }
                            ia_array = Communal._ia_d900;
                        }
                        else
                        {
                            i_state = FADM_Object.Communal._tcpModBus.Read(900, 29, ref ia_array);

                            if (Lib_Card.Configure.Parameter.Other_UseAbs == 1)
                            {
                                //读取吸光度数据
                                int[] ia_array_Abs = new int[40];
                                int i_state_Abs = FADM_Object.Communal._tcpModBusAbs.Read(13188, 40, ref ia_array_Abs);
                                if (i_state_Abs != -1)
                                {
                                    Communal._ia_d13188 = ia_array_Abs;
                                }

                                if (Communal._b_IsABSOpen)
                                {
                                    int[] ia_array_Abs2 = new int[100];
                                    int i_state_Abs2 = FADM_Object.Communal._tcpModBusAbs.Read(13088, 100, ref ia_array_Abs2);
                                    if (i_state_Abs2 != -1)
                                    {
                                        Communal._ia_d13088 = ia_array_Abs2;
                                    }
                                }
                            }

                            if(FADM_Object.Communal._b_isHaveAutoBrew)
                            {
                                //读取自动开料数据
                                int[] ia_array_Brew = new int[75];
                                int i_state_Brew = FADM_Object.Communal._tcpModBus.Read(22688, 75, ref ia_array_Brew);
                                if (i_state_Brew != -1)
                                {
                                    Communal._ia_d22688 = ia_array_Brew;
                                }
                            }

                            if (FADM_Object.Communal._b_isHaveAutoBrew)
                            {
                                //读取自动开料数据
                                int[] ia_array_WashBottle = new int[10];
                                int i_state_WashBottle = FADM_Object.Communal._tcpModBus.Read(22938, 10, ref ia_array_WashBottle);
                                if (i_state_WashBottle != -1)
                                {
                                    Communal._ia_d22938 = ia_array_WashBottle;
                                }
                            }
                        }
                        if (i_state != -1)
                        {
                            Communal._s_plcVersion =  ia_array[27].ToString("d4") + ia_array[28].ToString("d4");

                            _i_count = 0;

                            //调试页面下数据
                            //Communal._ia_d900[0]= _ia_array[0];//动作错误编号
                            //Communal._ia_d900[1] = _ia_array[1];
                            //Communal._ia_d900[2] = _ia_array[2];
                            //Communal._ia_d900[3] = _ia_array[3];
                            //Communal._ia_d900[4] = _ia_array[4];

                            Communal._b_isUpdateNewData = true;

                            int a1 = ia_array[0];//动作错误编号
                            int a2 = ia_array[1]; //电子秤数据
                            int a3 = ia_array[2]; //电子秤数据
                            int aa3 = ia_array[3]; //光幕信号
                            int a4 = ia_array[4]; //执行完成
                            int a5 = ia_array[5]; //输入点(双字)
                            int a6 = ia_array[6]; //输入点(双字)
                            int a7 = ia_array[7];//输出点(双字)
                            int a8 = ia_array[8]; //输出点(双字) 

                            int a9 = ia_array[9]; //X轴当前坐标
                            int a10 = ia_array[10];//X轴当前坐标  

                            int a11 = ia_array[11]; //Y轴当前坐标 
                            int a12 = ia_array[12]; //Y轴当前坐标 

                            int a13 = ia_array[13];//Z轴当前坐标（保留）
                            int a14 = ia_array[14];//Z轴当前坐标（保留）

                            int a15 = ia_array[15]; //X轴当前速度
                            int a16 = ia_array[16]; //X轴当前速度

                            int a17 = ia_array[17]; //Y轴当前速度
                            int a18 = ia_array[18]; //Y轴当前速度

                            int a19 = ia_array[19]; //Z轴当前速度
                            int a20 = ia_array[20]; //Z轴当前速度

                            int a23 = ia_array[23]; //
                            int a25 = ia_array[25]; //

                            double d_b = 0.0;
                            if (a2 < 0)
                            {
                                d_b = ((a3 + 1) * 65536 + a2) / 1000.0;
                            }
                            else
                            {
                                d_b = (a3 * 65536 + a2) / 1000.0;
                            }
                            //if(Lib_Card.Configure.Parameter.Machine_BalanceType == 0)
                            //{
                            //    d_b /= 10;
                            //}


                            LabBalanceValue.Text = Convert.ToString(d_b); //显示天平的数据
                            string s_str = Convert.ToString(a4, 2); //执行完成


                            char[] ca_cc = Convert.ToString(aa3, 2).PadLeft(4, '0').ToArray();
                            if (ca_cc[ca_cc.Length - 1].Equals('0'))
                            {
                                if (Lib_Card.Configure.Parameter.Machine_IsStopOrFront == 0)
                                {
                                    if (!Lib_Card.CardObject.bStopScr)
                                    {
                                        Lib_Card.CardObject.bStopScr = true;
                                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                            new FADM_Object.MyAlarm("急停已按下,请打开急停开关", 1);
                                        else
                                            new FADM_Object.MyAlarm("Emergency stop pressed,Please turn on the emergency stop switch", 1);
                                    }
                                }
                                else
                                {
                                    if (!Lib_Card.CardObject.bFront)
                                    {
                                        Lib_Card.CardObject.bFront = true;
                                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                            new FADM_Object.MyAlarm("前光幕遮挡,请离开光幕", 1);
                                        else
                                            new FADM_Object.MyAlarm("Front light curtain obstruction,Please step away from the light curtain", 1);
                                    }
                                }
                            }
                            else
                            {
                                if (Lib_Card.Configure.Parameter.Machine_IsStopOrFront == 0)
                                {
                                    Lib_Card.CardObject.bStopScr = false;
                                }
                                else
                                {
                                    Lib_Card.CardObject.bFront = false;
                                }
                            }
                            if (ca_cc[ca_cc.Length - 3].Equals('0'))
                            {
                                if (!Lib_Card.CardObject.bRight)
                                {
                                    Lib_Card.CardObject.bRight = true;
                                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    {
                                        if (Lib_Card.Configure.Parameter.Machine_IsRightDoor == 1)
                                            new FADM_Object.MyAlarm("右光幕遮挡,请离开光幕", 1);
                                        else
                                            new FADM_Object.MyAlarm("右门已打开,请关闭右门", 1);
                                    }
                                    else
                                    {
                                        if (Lib_Card.Configure.Parameter.Machine_IsRightDoor == 1)
                                            new FADM_Object.MyAlarm("Right light curtain obstruction,Please step away from the light curtain", 1);
                                        else
                                            new FADM_Object.MyAlarm("The right door is open,Please close the right door", 1);
                                    }
                                }
                            }
                            else
                            {
                                Lib_Card.CardObject.bRight = false;
                            }
                            //急停信号
                            if (ca_cc[ca_cc.Length - 2].Equals('0'))
                            {
                                if (!Lib_Card.CardObject.bLeft)
                                {
                                    Lib_Card.CardObject.bLeft = true;
                                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    {
                                        if (Lib_Card.Configure.Parameter.Machine_IsLeftDoor == 1)
                                            new FADM_Object.MyAlarm("左光幕遮挡,请离开光幕", 1);
                                        else
                                            new FADM_Object.MyAlarm("左门已打开,请关闭左门", 1);
                                    }
                                    else
                                    {
                                        if (Lib_Card.Configure.Parameter.Machine_IsLeftDoor == 1)
                                            new FADM_Object.MyAlarm("Left light curtain obstruction,Please step away from the light curtain", 1);
                                        else
                                            new FADM_Object.MyAlarm("The left door is open,Please close the left door", 1);
                                    }
                                }
                            }
                            else
                            {
                                Lib_Card.CardObject.bLeft = false;
                            }
                            if (Lib_Card.Configure.Parameter.Machine_UseBack == 1)
                            {
                                //后光幕
                                if (ca_cc[ca_cc.Length - 4].Equals('0'))
                                {
                                    if (!Lib_Card.CardObject.bBack)
                                    {
                                        Lib_Card.CardObject.bBack = true;
                                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                        {
                                            new FADM_Object.MyAlarm("后光幕遮挡,请离开光幕", 1);
                                        }
                                        else
                                        {
                                            new FADM_Object.MyAlarm("Back light curtain obstruction,Please step away from the light curtain", 1);
                                        }
                                    }
                                }
                                else
                                {
                                    Lib_Card.CardObject.bBack = false;
                                }
                            }



                            string s_str2 = Convert.ToString(a5, 2).PadLeft(16, '0');
                            if (!s_str2.Equals("0"))
                            {
                                ca_cc = s_str2.ToArray();//急停
                                ChkInPut_Stop.Checked = ca_cc[ca_cc.Length - 1].Equals('0') ? true : false; ;//急停 _ca_cc[_ca_cc.Length-1]
                                ChkInPut_Z_Corotation.Checked = ca_cc[ca_cc.Length - 3].Equals('1') ? true : false; //Z轴反限位
                                                                                                                    //_ca_cc[2]; 空 _ca_cc[_ca_cc.Length-3]
                                ChkInPut_Tray_Out.Checked = ca_cc[ca_cc.Length - 4].Equals('1') ? true : false; //接液盘出
                                ChkInPut_Tray_In.Checked = ca_cc[ca_cc.Length - 5].Equals('1') ? true : false; //接液盘回
                                ChkInPut_Syringe.Checked = ca_cc[ca_cc.Length - 6].Equals('1') ? true : false; //针筒传感器
                                ChkInPut_Tongs_A.Checked = ca_cc[ca_cc.Length - 7].Equals('1') ? true : false; //抓手A
                                ChkInPut_Tongs_B.Checked = ca_cc[ca_cc.Length - 8].Equals('1') ? true : false; //抓手B
                                ChkInPut_Cylinder_Up.Checked = ca_cc[ca_cc.Length - 9].Equals('1') ? true : false; //上限位
                                ChkInPut_Cylinder_Down.Checked = ca_cc[ca_cc.Length - 10].Equals('1') ? true : false; //下限位
                                ChkInPut_Sunx_B.Checked = ca_cc[ca_cc.Length - 11].Equals('0') ? true : false; //光幕
                                ChkInPut_Sunx_A.Checked = ca_cc[ca_cc.Length - 12].Equals('0') ? true : false; //光幕
                                ChkInPut_Cylinder_Mid.Checked = ca_cc[ca_cc.Length - 13].Equals('1') ? true : false; //气缸中限位
                                ChkInPut_Decompression_Up.Checked = ca_cc[ca_cc.Length - 14].Equals('1') ? true : false; //泄压上限位
                                ChkInPut_Decompression_Down.Checked = ca_cc[ca_cc.Length - 15].Equals('1') ? true : false; //泄压下限位
                                if (Lib_Card.Configure.Parameter.Machine_UseBack == 1)
                                {
                                    ChkInPut_Back.Checked = ca_cc[ca_cc.Length - 16].Equals('0') ? true : false; //后光幕
                                }
                            }
                            //其他输入点信息
                            s_str2 = Convert.ToString(a23, 2).PadLeft(9, '0');
                            ca_cc = s_str2.ToArray();//
                            ChkInPut_Block_Out.Checked = ca_cc[ca_cc.Length - 1].Equals('1') ? true : false; //阻挡出限位
                            ChkInPut_Block_In.Checked = ca_cc[ca_cc.Length - 2].Equals('1') ? true : false; //阻挡回限位
                            ChkInPut_Slow_Cylinder_Mid.Checked = ca_cc[ca_cc.Length - 3].Equals('1') ? true : false; //气缸慢速中限位
                            ChkInPut_Cylinder_Block.Checked = ca_cc[ca_cc.Length - 4].Equals('1') ? true : false; //气缸阻挡限位
                            ChkInPut_SupportCover.Checked = ca_cc[ca_cc.Length - 5].Equals('1') ? true : false; //撑盖开到位
                            ChkInPut_Slow_Cylinder_Mid_2.Checked = ca_cc[ca_cc.Length - 7].Equals('1') ? true : false; //气缸慢速中2限位
                            ChkInPut_Slow_Cylinder_Mid_3.Checked = ca_cc[ca_cc.Length - 8].Equals('1') ? true : false; //气缸慢速中3限位
                            if (FADM_Object.Communal._b_isHaveDyeStop)
                            {
                                ChkInPut_Dye_Stop.Checked = ca_cc[ca_cc.Length - 9].Equals('0') ? true : false; //转子缸急停
                                if((ca_cc[ca_cc.Length - 9].Equals('0') && !FADM_Object.Communal._b_isDyeStop)||(ca_cc[ca_cc.Length - 9].Equals('1') && FADM_Object.Communal._b_isDyeStop))
                                {
                                    if (FADM_Object.Communal._tcpDyeHMI1 != null)
                                        FADM_Object.Communal._tcpDyeHMI1.b_isSendStop = false;
                                    if (FADM_Object.Communal._tcpDyeHMI2 != null)
                                        FADM_Object.Communal._tcpDyeHMI2.b_isSendStop = false;
                                    if (FADM_Object.Communal._tcpDyeHMI3 != null)
                                        FADM_Object.Communal._tcpDyeHMI3.b_isSendStop = false;
                                    if (FADM_Object.Communal._tcpDyeHMI4 != null)
                                        FADM_Object.Communal._tcpDyeHMI4.b_isSendStop = false;
                                    if (FADM_Object.Communal._tcpDyeHMI5 != null)
                                        FADM_Object.Communal._tcpDyeHMI5.b_isSendStop = false;
                                    if (FADM_Object.Communal._tcpDyeHMI6 != null)
                                        FADM_Object.Communal._tcpDyeHMI6.b_isSendStop = false;
                                    FADM_Object.Communal._b_isDyeStop = ca_cc[ca_cc.Length - 9].Equals('0') ? true : false;
                                }
                            }
                            //ChkInPut_Block.Checked = ca_cc[ca_cc.Length - 5].Equals('1') ? true : false; //抓手A泄压信号
                            //ChkInPut_Block.Checked = ca_cc[ca_cc.Length - 6].Equals('1') ? true : false; //抓手B泄压信号



                            s_str = Convert.ToString(a6, 2).PadLeft(12, '0');
                            if (!s_str.Equals("0"))
                            {
                                ca_cc = s_str.ToArray();
                                ChkInPut_X_Corotation.Checked = ca_cc[ca_cc.Length - 2].Equals('1') ? true : false;  //X轴正限位
                                ChkInPut_X_Reverse.Checked = ca_cc[ca_cc.Length - 1].Equals('1') ? true : false;  //X轴反限位
                                ChkInPut_X_Origin.Checked = ca_cc[ca_cc.Length - 3].Equals('1') ? true : false;  //X转原点位
                                ChkInPut_X_Ready.Checked = ca_cc[ca_cc.Length - 4].Equals('1') ? true : false;  //X轴准备位
                                ChkInPut_X_Alarm.Checked = ca_cc[ca_cc.Length - 5].Equals('1') ? true : false;  //X轴报警
                                this.BtnOutPut_X_Power.ForeColor = ca_cc[ca_cc.Length - 6].Equals('1') ? Color.Red : Color.Black;//X轴失能
                                ChkInPut_Y_Corotation.Checked = ca_cc[ca_cc.Length - 8].Equals('1') ? true : false;  //Y轴正限位
                                ChkInPut_Y_Reverse.Checked = ca_cc[ca_cc.Length - 7].Equals('1') ? true : false;  //Y轴反限位
                                ChkInPut_Y_Origin.Checked = ca_cc[ca_cc.Length - 9].Equals('1') ? true : false;  //Y转原点位
                                ChkInPut_Y_Ready.Checked = ca_cc[ca_cc.Length - 10].Equals('1') ? true : false;  //Y轴准备位
                                ChkInPut_Y_Alarm.Checked = ca_cc[ca_cc.Length - 11].Equals('1') ? true : false;  //Y轴报警
                                this.BtnOutPut_Y_Power.ForeColor = ca_cc[ca_cc.Length - 12].Equals('1') ? Color.Red : Color.Black;//Y轴失能
                            }

                            s_str = Convert.ToString(a7, 2).PadLeft(15, '0');
                            if (!s_str.Equals("0"))
                            {
                                ca_cc = s_str.ToArray();
                                //_ca_cc[0]; Z轴脉冲 _ca_cc.Length-1
                                //_ca_cc[1];Z轴方向 _ca_cc.Length-2
                                //_ca_cc[2];空  _ca_cc.Length-3
                                //_ca_cc[3];空 _ca_cc.Length-4
                                this.BtnOutPut_Blender.ForeColor = ca_cc[ca_cc.Length - 5].Equals('0') ? Color.Red : Color.Black; //搅拌停
                                this.BtnOutPut_Waste.ForeColor = ca_cc[ca_cc.Length - 6].Equals('1') ? Color.Red : Color.Black; //抽废液
                                this.BtnOutPut_Buzzer.ForeColor = ca_cc[ca_cc.Length - 7].Equals('1') ? Color.Red : Color.Black; //抽废液
                                this.BtnOutPut_Water.ForeColor = ca_cc[ca_cc.Length - 8].Equals('1') ? Color.Red : Color.Black; //加水
                                this.BtnOutPut_Tray.ForeColor = ca_cc[ca_cc.Length - 9].Equals('1') ? Color.Red : Color.Black; //接液盘
                                this.BtnOutPut_Tongs.ForeColor = ca_cc[ca_cc.Length - 10].Equals('1') ? Color.Red : Color.Black; //抓手
                                this.BtnOutPut_Cylinder_Down.ForeColor = ca_cc[ca_cc.Length - 11].Equals('1') ? Color.Red : Color.Black; //气缸下
                                this.BtnOutPut_Decompression.ForeColor = ca_cc[ca_cc.Length - 14].Equals('1') ? Color.Red : Color.Black; //泄压下
                                this.BtnOutPut_Block_Out.ForeColor = ca_cc[ca_cc.Length - 15].Equals('1') ? Color.Red : Color.Black; //阻挡出

                            }

                            s_str = Convert.ToString(a25, 2).PadLeft(6, '0');
                            if (!s_str.Equals("0"))
                            {
                                ca_cc = s_str.ToArray();
                                this.BtnOutPut_Wash_In.ForeColor = ca_cc[ca_cc.Length - 2].Equals('1') ? Color.Red : Color.Black; //洗针进水
                                this.BtnOutPut_Wash_Out.ForeColor = ca_cc[ca_cc.Length - 3].Equals('1') ? Color.Red : Color.Black; //洗针出水
                                this.BtnOutPut_Wash_Blow.ForeColor = ca_cc[ca_cc.Length - 4].Equals('1') ? Color.Red : Color.Black; //洗针吹气
                                this.BtnOutPut_Dissolving.ForeColor = ca_cc[ca_cc.Length - 5].Equals('1') ? Color.Red : Color.Black; //洗针吹气

                            }

                            //s_str = Convert.ToString(a8, 2);//输出
                            //_ca_cc = s_str.ToArray();

                            if (a9 < 0)
                            {
                                d_b = ((a10 + 1) * 65536 + a9) / 10;
                            }
                            else
                            {
                                d_b = (a10 * 65536 + a9) / 10;
                            }
                            this.TxtRPosX.Text = Convert.ToString(d_b); //TxtRPosX X轴当前坐标


                            if (a11 < 0)
                            {
                                d_b = ((a12 + 1) * 65536 + a11) / 10;
                            }
                            else
                            {
                                d_b = (a12 * 65536 + a11) / 10;
                            }
                            this.TxtRPosY.Text = Convert.ToString(d_b); //Y轴当前坐标



                            if (a13 < 0)
                            {
                                d_b = (((a14 + 1) * 65536 + a13));
                            }
                            else
                            {
                                d_b = ((a14 * 65536 + a13));
                            }

                            this.TxtCPosZ.Text = Convert.ToString(d_b); //Z轴当前坐标

                            if (a15 < 0)
                            {
                                d_b = ((a16 + 1) * 65536 + a15) / 1000.0;
                            }
                            else
                            {
                                d_b = (a16 * 65536 + a15) / 1000.0;
                            }
                            //d_b = Convert.ToInt16(a16<<16|a15);
                            this.TxtCSpeedX.Text = Convert.ToString(d_b); //X轴当前速度

                            if (a17 < 0)
                            {
                                d_b = ((a18 + 1) * 65536 + a17) / 1000.0;
                            }
                            else
                            {
                                d_b = (a18 * 65536 + a17) / 1000.0;
                            }
                            this.TxtCSpeedY.Text = Convert.ToString(d_b); //Y轴当前速度

                            if (a19 < 0)
                            {
                                d_b = ((a20 + 1) * 65536 + a19) / 1000.0;
                            }
                            else
                            {
                                d_b = (a20 * 65536 + a19) / 1000.0;
                            }
                            this.TxtCSpeedZ.Text = Convert.ToString(d_b); //Z轴当前速度

                            if (!Communal._b_isBSendOpen)
                            {
                                if (Communal._i_bType == 1)
                                {
                                    int[] ia_array12 = { 17 };
                                    int i_state12 = FADM_Object.Communal._tcpModBus.Write(811, ia_array12);

                                    Communal._b_isBSendClose = false;
                                    Communal._b_isBSendOpen = true;
                                }
                            }

                            if (!Communal._b_isBSendClose)
                            {
                                if (Communal._i_bType == 2)
                                {
                                    int[] ia_array12 = { 18 };
                                    int i_state12 = FADM_Object.Communal._tcpModBus.Write(811, ia_array12);

                                    Communal._b_isBSendOpen = false;
                                    Communal._b_isBSendClose = true;
                                }
                            }


                        }
                        else
                        {
                            //重新连接试试
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            _i_count++;
                            if (_i_count > 5)
                            {
                                _b_reconnect = true;

                                Thread thread = new Thread(ReConnect); //线程读取数据，确定已经重连
                                thread.IsBackground = true;
                                thread.Start();
                            }
                        }
                    }

                }
                catch (Exception ex)
                {

                }
            }
        }

        private void ReConnect()
        {

            while (true)
            {
                Thread.Sleep(60);  //这里一直读 如果同时地方也同时读 就有可能造成读不到 
                FADM_Object.Communal._tcpModBus.ReConnect();
                int[] ia_array = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                int i_state = FADM_Object.Communal._tcpModBus.Read(900, 21, ref ia_array);
                if (i_state != -1)
                {
                    _i_count = 0;
                    _b_reconnect = false;
                    break;
                }

            }
        }



        private void BtnOutPut_X_Power_Click(object sender, EventArgs e)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                Lib_Card.ADT8940A1.OutPut.X_Power.X_Power xpower = new Lib_Card.ADT8940A1.OutPut.X_Power.X_Power_Basic();
                int iRes = Lib_Card.CardObject.OA1.ReadOutPut(Lib_Card.ADT8940A1.ADT8940A1_IO.OutPut_X_Power);
                if (-1 == iRes)
                {
                    FADM_Form.CustomMessageBox.Show("驱动异常", "ReadOutPut", MessageBoxButtons.OK, false);
                }
                else if (0 == iRes)
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "X矢能打开");
                    if (-1 == xpower.X_Power_On())
                    {
                        FADM_Form.CustomMessageBox.Show("驱动异常", "X_Power_On", MessageBoxButtons.OK, false);
                    }
                }
                else
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "X矢能关闭");
                    if (-1 == xpower.X_Power_Off())
                    {
                        FADM_Form.CustomMessageBox.Show("驱动异常", "X_Power_Off", MessageBoxButtons.OK, false);
                    }
                }
            }
            else
            {
                try
                {
                    _b_istrue = true;
                    //X失能
                    if (this.BtnOutPut_X_Power.ForeColor == Color.Red)
                    { //红色 上了失能 关
                        _ia_array[0] = 2;
                    }
                    else
                    {
                        _ia_array[0] = 1;
                    }
                    int i_state = FADM_Object.Communal._tcpModBus.Write(811, _ia_array);
                }
                catch (Exception ex) { }
                finally
                {
                    _b_istrue = false;
                }
            }
         }


        private void BtnOutPut_Y_Power_Click(object sender, EventArgs e)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                Lib_Card.ADT8940A1.OutPut.Y_Power.Y_Power ypower = new Lib_Card.ADT8940A1.OutPut.Y_Power.Y_Power_Basic();
                int iRes = Lib_Card.CardObject.OA1.ReadOutPut(Lib_Card.ADT8940A1.ADT8940A1_IO.OutPut_Y_Power);
                if (-1 == iRes)
                {
                    FADM_Form.CustomMessageBox.Show("驱动异常", "ReadOutPut", MessageBoxButtons.OK, false);
                }
                else if (0 == iRes)
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "Y矢能打开");
                    if (-1 == ypower.Y_Power_On())
                    {
                        FADM_Form.CustomMessageBox.Show("驱动异常", "Y_Power_On", MessageBoxButtons.OK, false);
                    }
                }
                else
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "X矢能关闭");
                    if (-1 == ypower.Y_Power_Off())
                    {
                        FADM_Form.CustomMessageBox.Show("驱动异常", "Y_Power_Off", MessageBoxButtons.OK, false);
                    }
                }
            }
            else
            {
                try
                {
                    _b_istrue = true;
                    //Y失能
                    if (this.BtnOutPut_Y_Power.ForeColor == Color.Red)
                    { //红色 上了失能 关
                        _ia_array[0] = 4;
                    }
                    else
                    {
                        _ia_array[0] = 3;
                    }
                    int i_state = FADM_Object.Communal._tcpModBus.Write(811, _ia_array);
                }
                catch { }
                finally
                {
                    _b_istrue = false;
                }
            }
            
        }

        private void BtnOutPut_Cylinder_Down_Click(object sender, EventArgs e)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                Lib_Card.ADT8940A1.OutPut.Cylinder.Cylinder cylinder;
                if (Lib_Card.Configure.Parameter.Machine_CylinderVersion == 0)
                    cylinder = new Lib_Card.ADT8940A1.OutPut.Cylinder.SingleControl.Cylinder_Basic();
                else
                    cylinder = new Lib_Card.ADT8940A1.OutPut.Cylinder.DualControl.Cylinder_Basic();

                int iRes = Lib_Card.CardObject.OA1.ReadOutPut(Lib_Card.ADT8940A1.ADT8940A1_IO.OutPut_Cylinder_Down);
                if (-1 == iRes)
                {
                    FADM_Form.CustomMessageBox.Show("驱动异常", "ReadOutPut", MessageBoxButtons.OK, false);
                }
                else if (0 == iRes)
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "气缸下");
                    if (-1 == cylinder.CylinderDown(0))
                    {
                        FADM_Form.CustomMessageBox.Show("驱动异常", "CylinderDown", MessageBoxButtons.OK, false);
                    }
                }
                else
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "气缸上");
                    if (-1 == cylinder.CylinderUp(0))
                    {
                        FADM_Form.CustomMessageBox.Show("驱动异常", "CylinderUp", MessageBoxButtons.OK, false);
                    }
                }
            }
            else
            {
                try
                {
                    _b_istrue = true;
                    if (this.BtnOutPut_Cylinder_Down.ForeColor == Color.Red)
                    {
                        _ia_array[0] = 5;
                    }
                    else
                    {
                        _ia_array[0] = 6;
                    }
                    int i_state = FADM_Object.Communal._tcpModBus.Write(811, _ia_array);

                }
                catch
                {

                }
                finally
                {
                    _b_istrue = false;
                }
            }

        }

        private void BtnOutPut_Blender_Click(object sender, EventArgs e)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                if (Lib_Card.Configure.Parameter.Machine_BlenderVersion == 0)
                {
                    Lib_Card.ADT8940A1.OutPut.Blender.Blender blender = new Lib_Card.ADT8940A1.OutPut.Blender.Blender_Basic();
                    int iRes = Lib_Card.CardObject.OA1.ReadOutPut(Lib_Card.ADT8940A1.ADT8940A1_IO.OutPut_Blender);
                    if (-1 == iRes)
                    {
                        FADM_Form.CustomMessageBox.Show("驱动异常", "ReadOutPut", MessageBoxButtons.OK, false);
                    }
                    else if (1 == iRes)
                    {
                        FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "停止搅拌打开");
                        if (-1 == blender.Blender_On())
                        {
                            FADM_Form.CustomMessageBox.Show("驱动异常", "Blender_On", MessageBoxButtons.OK, false);
                        }
                    }
                    else
                    {
                        FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "停止搅拌关闭");
                        if (-1 == blender.Blender_Off())
                        {
                            FADM_Form.CustomMessageBox.Show("驱动异常", "Blender_Off", MessageBoxButtons.OK, false);
                        }
                    }
                }
                else
                {
                    Lib_Card.ADT8940A1.OutPut.Blender.Blender blender = new Lib_Card.ADT8940A1.OutPut.Blender.Blender_Basic();
                    int iRes = Lib_Card.CardObject.OA1.ReadOutPut(Lib_Card.ADT8940A1.ADT8940A1_IO.OutPut_Blender);
                    if (-1 == iRes)
                    {
                        FADM_Form.CustomMessageBox.Show("驱动异常", "ReadOutPut", MessageBoxButtons.OK, false);
                    }
                    else if (0 == iRes)
                    {
                        FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "停止搅拌打开");
                        if (-1 == blender.Blender_On())
                        {
                            FADM_Form.CustomMessageBox.Show("驱动异常", "Blender_On", MessageBoxButtons.OK, false);
                        }
                    }
                    else
                    {
                        FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "停止搅拌关闭");
                        if (-1 == blender.Blender_Off())
                        {
                            FADM_Form.CustomMessageBox.Show("驱动异常", "Blender_Off", MessageBoxButtons.OK, false);
                        }
                    }
                }
            }
            else
            {
                try
                {
                    _b_istrue = true;
                    if (this.BtnOutPut_Blender.ForeColor == Color.Red)
                    {
                        _ia_array[0] = 10;
                    }
                    else
                    {
                        _ia_array[0] = 9;
                    }
                    int i_state = FADM_Object.Communal._tcpModBus.Write(811, _ia_array);
                }
                catch
                {
                }
                finally
                {
                    _b_istrue = false;
                }
            }
            
        }

        private void BtnOutPut_Tray_Click(object sender, EventArgs e)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                Lib_Card.ADT8940A1.OutPut.Tray.Tray tray = new Lib_Card.ADT8940A1.OutPut.Tray.Tray_Basic();
                int iRes = Lib_Card.CardObject.OA1.ReadOutPut(Lib_Card.ADT8940A1.ADT8940A1_IO.OutPut_Tray);
                if (-1 == iRes)
                {
                    FADM_Form.CustomMessageBox.Show("驱动异常", "ReadOutPut", MessageBoxButtons.OK, false);
                }
                else if (0 == iRes)
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "接液盘伸出");
                    if (-1 == tray.Tray_On())
                    {
                        FADM_Form.CustomMessageBox.Show("驱动异常", "Tray_On", MessageBoxButtons.OK, false);
                    }
                }
                else
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "接液盘收回");
                    if (-1 == tray.Tray_Off())
                    {
                        FADM_Form.CustomMessageBox.Show("驱动异常", "Tray_Off", MessageBoxButtons.OK, false);
                    }
                }
            }
            else
            {
                try
                {
                    _b_istrue = true;
                    if (this.BtnOutPut_Tray.ForeColor == Color.Red)
                    {
                        _ia_array[0] = 12;
                    }
                    else
                    {
                        _ia_array[0] = 11;
                    }
                    int i_state = FADM_Object.Communal._tcpModBus.Write(811, _ia_array);

                }
                catch { }
                finally
                {
                    _b_istrue = false;
                }
            }

            
        }

        private void BtnOutPut_Tongs_Click(object sender, EventArgs e)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                Lib_Card.ADT8940A1.OutPut.Tongs.Tongs tongs = new Lib_Card.ADT8940A1.OutPut.Tongs.Tongs_Basic();
                int iRes = Lib_Card.CardObject.OA1.ReadOutPut(Lib_Card.ADT8940A1.ADT8940A1_IO.OutPut_Tongs);
                if (-1 == iRes)
                {
                    FADM_Form.CustomMessageBox.Show("驱动异常", "ReadOutPut", MessageBoxButtons.OK, false);
                }
                else if (0 == iRes)
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "抓手关闭");
                    if (-1 == tongs.Tongs_On())
                    {
                        FADM_Form.CustomMessageBox.Show("驱动异常", "Tongs_On", MessageBoxButtons.OK, false);
                    }
                }
                else
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "抓手打开");
                    if (-1 == tongs.Tongs_Off())
                    {
                        FADM_Form.CustomMessageBox.Show("驱动异常", "Tongs_Off", MessageBoxButtons.OK, false);
                    }
                }
            }
            else
            {
                try
                {
                    _b_istrue = true;
                    if (this.BtnOutPut_Tongs.ForeColor == Color.Red)
                    {
                        _ia_array[0] = 7;
                    }
                    else
                    {
                        _ia_array[0] = 8;
                    }
                    int i_state = FADM_Object.Communal._tcpModBus.Write(811, _ia_array);

                }
                catch
                {

                }
                finally
                {
                    _b_istrue = false;
                }
            }
            
        }

        private void BtnOutPut_Block_Click(object sender, EventArgs e)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                Lib_Card.ADT8940A1.OutPut.Block.Block block = new Lib_Card.ADT8940A1.OutPut.Block.Block_Basic();
                int iRes = Lib_Card.CardObject.OA1.ReadOutPut(Lib_Card.ADT8940A1.ADT8940A1_IO.OutPut_Block_Out);
                if (-1 == iRes)
                {
                    FADM_Form.CustomMessageBox.Show("驱动异常", "ReadOutPut", MessageBoxButtons.OK, false);
                }
                else if (0 == iRes)
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "阻挡气缸伸出");
                    if (-1 == block.Block_Out())
                    {
                        FADM_Form.CustomMessageBox.Show("驱动异常", "Block_Out", MessageBoxButtons.OK, false);
                    }
                }
                else
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "阻挡气缸收回");
                    if (-1 == block.Block_In())
                    {
                        FADM_Form.CustomMessageBox.Show("驱动异常", "Block_In", MessageBoxButtons.OK, false);
                    }
                }
            }
            else
            {
                try
                {
                    _b_istrue = true;
                    if (this.BtnOutPut_Block_Out.ForeColor == Color.Red)
                    {
                        _ia_array[0] = 35;
                    }
                    else
                    {
                        _ia_array[0] = 34;
                    }
                    int i_state = FADM_Object.Communal._tcpModBus.Write(811, _ia_array);

                }
                catch { }
                finally
                {
                    _b_istrue = false;
                }
            }
            
        }

        private void BtnOutPut_Decompression_Click(object sender, EventArgs e)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                Lib_Card.ADT8940A1.OutPut.Decompression.Decompression decompression = new Lib_Card.ADT8940A1.OutPut.Decompression.Decompression_Basic();
                int iRes = Lib_Card.CardObject.OA1.ReadOutPut(Lib_Card.ADT8940A1.ADT8940A1_IO.OutPut_Decompression);
                if (-1 == iRes)
                {
                    FADM_Form.CustomMessageBox.Show("驱动异常", "ReadOutPut", MessageBoxButtons.OK, false);
                }
                else if (0 == iRes)
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "泄压气缸下");
                    if (-1 == decompression.Decompression_Down())
                    {
                        FADM_Form.CustomMessageBox.Show("驱动异常", "Decompression_Down", MessageBoxButtons.OK, false);
                    }
                }
                else
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "泄压气缸上");
                    if (-1 == decompression.Decompression_Up())
                    {
                        FADM_Form.CustomMessageBox.Show("驱动异常", "Decompression_Up", MessageBoxButtons.OK, false);
                    }
                }
            }
            else
            {
                try
                {
                    _b_istrue = true;
                    if (this.BtnOutPut_Decompression.ForeColor == Color.Red)
                    {
                        _ia_array[0] = 23;
                    }
                    else
                    {
                        _ia_array[0] = 24;
                    }
                    int i_state = FADM_Object.Communal._tcpModBus.Write(811, _ia_array);

                }
                catch { }
                finally
                {
                    _b_istrue = false;
                }
            }
            
        }

        private void BtnOutPut_Apocenosis_Click(object sender, EventArgs e)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                Lib_Card.ADT8940A1.OutPut.Apocenosis.Apocenosis apocenosis = new Lib_Card.ADT8940A1.OutPut.Apocenosis.Apocenosis_Basic();
                int iRes = Lib_Card.CardObject.OA1.ReadOutPut(Lib_Card.ADT8940A1.ADT8940A1_IO.OutPut_Apocenosis);
                if (-1 == iRes)
                {
                    FADM_Form.CustomMessageBox.Show("驱动异常", "ReadOutPut", MessageBoxButtons.OK, false);
                }
                else if (0 == iRes)
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "排液打开");
                    if (-1 == apocenosis.Apocenosis_On())
                    {
                        FADM_Form.CustomMessageBox.Show("驱动异常", "Apocenosis_On", MessageBoxButtons.OK, false);
                    }
                }
                else
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "排液关闭");
                    if (-1 == apocenosis.Apocenosis_Off())
                    {
                        FADM_Form.CustomMessageBox.Show("驱动异常", "Apocenosis_Off", MessageBoxButtons.OK, false);
                    }
                }
            }
            else
            {
                try
                {
                    _b_istrue = true;
                    if (this.BtnOutPut_Apocenosis.ForeColor == Color.Red)
                    {
                        _ia_array[0] = 20;
                    }
                    else
                    {
                        _ia_array[0] = 19;
                    }
                    int i_state = FADM_Object.Communal._tcpModBus.Write(811, _ia_array);
                }
                catch { }
                finally
                {
                    _b_istrue = false;
                }
            }
            
        }

        private void BtnOutPut_Buzzer_Click(object sender, EventArgs e)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer buzzer = new Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer_Basic();
                int iRes = Lib_Card.CardObject.OA1.ReadOutPut(Lib_Card.ADT8940A1.ADT8940A1_IO.OutPut_Buzzer);
                if (-1 == iRes)
                {
                    FADM_Form.CustomMessageBox.Show("驱动异常", "ReadOutPut", MessageBoxButtons.OK, false);
                }
                else if (0 == iRes)
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "蜂鸣器打开");
                    if (-1 == buzzer.Buzzer_On())
                    {
                        FADM_Form.CustomMessageBox.Show("驱动异常", "Buzzer_On", MessageBoxButtons.OK, false);
                    }
                }
                else
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "蜂鸣器关闭");
                    if (-1 == buzzer.Buzzer_Off())
                    {
                        FADM_Form.CustomMessageBox.Show("驱动异常", "Buzzer_Off", MessageBoxButtons.OK, false);
                    }
                }
            }
            else
            {
                try
                {
                    _b_istrue = true;
                    if (this.BtnOutPut_Buzzer.ForeColor == Color.Red)
                    {
                        _ia_array[0] = 18;
                    }
                    else
                    {
                        _ia_array[0] = 17;
                    }
                    int i_state = FADM_Object.Communal._tcpModBus.Write(811, _ia_array);
                }
                catch { }
                finally
                {
                    _b_istrue = false;
                }
            }
            
        }

        private void BtnOutPut_Water_Click(object sender, EventArgs e)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                Lib_Card.ADT8940A1.OutPut.Water.Water water = new Lib_Card.ADT8940A1.OutPut.Water.Water_Basic();
                int iRes = Lib_Card.CardObject.OA1.ReadOutPut(Lib_Card.ADT8940A1.ADT8940A1_IO.OutPut_Water);
                if (-1 == iRes)
                {
                    FADM_Form.CustomMessageBox.Show("驱动异常", "ReadOutPut", MessageBoxButtons.OK, false);
                }
                else if (0 == iRes)
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "水打开");
                    if (-1 == water.Water_On())
                    {
                        FADM_Form.CustomMessageBox.Show("驱动异常", "Water_On", MessageBoxButtons.OK, false);
                    }
                }
                else
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "水关闭");
                    if (-1 == water.Water_Off())
                    {
                        FADM_Form.CustomMessageBox.Show("驱动异常", "Water_Off", MessageBoxButtons.OK, false);
                    }
                }
            }
            else
            {
                try
                {
                    _b_istrue = true;
                    if (this.BtnOutPut_Water.ForeColor == Color.Red)
                    {
                        _ia_array[0] = 16;
                    }
                    else
                    {
                        _ia_array[0] = 15;
                    }
                    int i_state = FADM_Object.Communal._tcpModBus.Write(811, _ia_array);

                }
                catch { }
                finally
                {
                    _b_istrue = false;
                }
            }
            

        }

        private void BtnOutPut_Waste_Click(object sender, EventArgs e)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                Lib_Card.ADT8940A1.OutPut.Waste.Waste waste = new Lib_Card.ADT8940A1.OutPut.Waste.Waste_Basic();
                int iRes = Lib_Card.CardObject.OA1.ReadOutPut(Lib_Card.ADT8940A1.ADT8940A1_IO.OutPut_Waste);
                if (-1 == iRes)
                {
                    FADM_Form.CustomMessageBox.Show("驱动异常", "ReadOutPut", MessageBoxButtons.OK, false);
                }
                else if (0 == iRes)
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "废液回抽打开");
                    if (-1 == waste.Waste_On())
                    {
                        FADM_Form.CustomMessageBox.Show("驱动异常", "Waste_On", MessageBoxButtons.OK, false);
                    }
                }
                else
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "废液回抽关闭");
                    if (-1 == waste.Waste_Off())
                    {
                        FADM_Form.CustomMessageBox.Show("驱动异常", "Waste_Off", MessageBoxButtons.OK, false);
                    }
                }
            }
            else
            {
                try
                {
                    _b_istrue = true;
                    if (this.BtnOutPut_Waste.ForeColor == Color.Red)
                    {
                        _ia_array[0] = 14;
                    }
                    else
                    {
                        _ia_array[0] = 13;
                    }
                    int i_state = FADM_Object.Communal._tcpModBus.Write(811, _ia_array);
                }
                catch { }
                finally
                {
                    _b_istrue = false;
                }
            }
            
        }

        private void BtnStartMove_Click(object sender, EventArgs e)
        {
            FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "定点移动启动");
            if ((0 == FADM_Object.Communal.ReadMachineStatus() || 8 == FADM_Object.Communal.ReadMachineStatus()) && null == FADM_Object.Communal.ReadDyeThread() && FADM_Object.Communal.ReadTcpStatus())
            {
                FADM_Object.Communal.WriteDripWait(true);
                if (RdoBottle.Checked)
                {
                    if (string.IsNullOrEmpty(TxtNum.Text))
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("请输入瓶号！", "温馨提示", MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("Please enter the bottle number！", "Tips", MessageBoxButtons.OK, false);
                        FADM_Object.Communal.WriteDripWait(false);
                        return;
                    }
                    FADM_Object.Communal.WriteMachineStatus(4);
                    Thread thread = new Thread(BottleMove);
                    thread.Start();

                }
                else if (RdoCup.Checked)
                {
                    if (string.IsNullOrEmpty(TxtNum.Text))
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("请输入杯号！", "温馨提示", MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("Please enter the cup number！", "Tips", MessageBoxButtons.OK, false);
                        FADM_Object.Communal.WriteDripWait(false);
                        return;
                    }
                    FADM_Object.Communal.WriteMachineStatus(4);
                    Thread thread = new Thread(CupMove);
                    thread.Start();
                }
                else if (RdoStress.Checked)
                {
                    if (string.IsNullOrEmpty(TxtNum.Text))
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("请输入杯号！", "温馨提示", MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("Please enter the cup number！", "Tips", MessageBoxButtons.OK, false);
                        FADM_Object.Communal.WriteDripWait(false);
                        return;
                    }
                    FADM_Object.Communal.WriteMachineStatus(4);
                    Thread thread = new Thread(CupStress);
                    thread.Start();
                }
                else if (RdoBalance.Checked)
                {
                    FADM_Object.Communal.WriteMachineStatus(4);
                    Thread thread = new Thread(BalanceMove);
                    thread.Start();
                }
                else if (RdoDecompression.Checked)
                {
                    if (string.IsNullOrEmpty(TxtNum.Text))
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("请输入杯号！", "温馨提示", MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("Please enter the cup number！", "Tips", MessageBoxButtons.OK, false);
                        FADM_Object.Communal.WriteDripWait(false);
                        return;
                    }
                    FADM_Object.Communal.WriteMachineStatus(4);
                    Thread thread = new Thread(DecompressionMove);
                    thread.Start();
                }
                else if (RdoDryCloth.Checked)
                {
                    if (string.IsNullOrEmpty(TxtNum.Text))
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("请输入杯号！", "温馨提示", MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("Please enter the cup number！", "Tips", MessageBoxButtons.OK, false);
                        FADM_Object.Communal.WriteDripWait(false);
                        return;
                    }
                    FADM_Object.Communal.WriteMachineStatus(4);
                    Thread thread = new Thread(DryClothMove);
                    thread.Start();
                }
                else if (RdoWetCloth.Checked)
                {
                    if (string.IsNullOrEmpty(TxtNum.Text))
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("请输入杯号！", "温馨提示", MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("Please enter the cup number！", "Tips", MessageBoxButtons.OK, false);
                        FADM_Object.Communal.WriteDripWait(false);
                        return;
                    }
                    FADM_Object.Communal.WriteMachineStatus(4);
                    Thread thread = new Thread(WetClothMove);
                    thread.Start();
                }
                else if (RdoDryClamp.Checked)
                {
                    FADM_Object.Communal.WriteMachineStatus(4);
                    Thread thread = new Thread(DryClampMove);
                    thread.Start();
                }
                else if (RdoWetClamp.Checked)
                {
                    FADM_Object.Communal.WriteMachineStatus(4);
                    Thread thread = new Thread(WetClampMove);
                    thread.Start();
                }
                else if (RdoWash.Checked)
                {
                    FADM_Object.Communal.WriteMachineStatus(4);
                    Thread thread = new Thread(Wash);
                    thread.Start();

                }

                else if (RdoStandBy.Checked)
                {
                    FADM_Object.Communal.WriteMachineStatus(4);
                    Thread thread = new Thread(StandByMove);
                    thread.Start();
                }
                else if (RdoExtraction.Checked)
                {
                    FADM_Object.Communal.WriteMachineStatus(4);
                    Thread thread = new Thread(ExtractionMove);
                    thread.Start();
                }
                else if (RdoAbs.Checked)
                {
                    if (string.IsNullOrEmpty(TxtNum.Text))
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("请输入杯号！", "温馨提示", MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("Please enter the cup number！", "Tips", MessageBoxButtons.OK, false);
                        FADM_Object.Communal.WriteDripWait(false);
                        return;
                    }
                    FADM_Object.Communal.WriteMachineStatus(4);
                    Thread thread = new Thread(AbsMove);
                    thread.Start();
                }
                else if (RdoSupportCover.Checked)
                {
                    FADM_Object.Communal.WriteMachineStatus(4);
                    Thread thread = new Thread(SupportCoverMove);
                    thread.Start();
                }
                else if (RdoPH.Checked)
                {
                    FADM_Object.Communal.WriteMachineStatus(4);
                    Thread thread = new Thread(PHMove);
                    thread.Start();
                }
                else if (RdoSalinity.Checked)
                {
                    FADM_Object.Communal.WriteMachineStatus(4);
                    Thread thread = new Thread(SalinityMove);
                    thread.Start();
                }
                else if (RdoPower.Checked)
                {
                    FADM_Object.Communal.WriteMachineStatus(4);
                    Thread thread = new Thread(PowerMove);
                    thread.Start();
                }
                else if (RdoWashBottle.Checked)
                {
                    FADM_Object.Communal.WriteMachineStatus(4);
                    Thread thread = new Thread(WashBottleMove);
                    thread.Start();
                }
                else if (RdoBrew.Checked)
                {
                    if (string.IsNullOrEmpty(TxtNum.Text))
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("请输入杯号！", "温馨提示", MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("Please enter the cup number！", "Tips", MessageBoxButtons.OK, false);
                        FADM_Object.Communal.WriteDripWait(false);
                        return;
                    }
                    FADM_Object.Communal.WriteMachineStatus(4);
                    Thread thread = new Thread(BrewMove);
                    thread.Start();
                }

                else if (RdoSysFinish.Checked)
                {
                    if (string.IsNullOrEmpty(TxtNum.Text))
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("请输入杯号！", "温馨提示", MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("Please enter the cup number！", "Tips", MessageBoxButtons.OK, false);
                        FADM_Object.Communal.WriteDripWait(false);
                        return;
                    }
                    FADM_Object.Communal.WriteMachineStatus(4);
                    Thread thread = new Thread(SysFinishMove);
                    thread.Start();
                }
                else if (RdoPHTest.Checked)
                {
                    if (string.IsNullOrEmpty(TxtNum.Text))
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("请输入杯号！", "温馨提示", MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("Please enter the cup number！", "Tips", MessageBoxButtons.OK, false);
                        FADM_Object.Communal.WriteDripWait(false);
                        return;
                    }
                    FADM_Object.Communal.WriteMachineStatus(4);
                    Thread thread = new Thread(PHTestMove);
                    thread.Start();
                }
                else
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show("请选择目标区域！", "温馨提示", MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("Please select the target area！", "Tips", MessageBoxButtons.OK, false);
                    FADM_Object.Communal.WriteDripWait(false);
                    return;

                }

                //while (true)
                //{
                //    if (0 == FADM_Object.Communal.ReadMachineStatus() || 8 == FADM_Object.Communal.ReadMachineStatus())
                //    {
                //        FADM_Object.Communal.WriteDripWait(false);
                //        break;
                //    }
                //    Thread.Sleep(1);
                //}

                Thread threadReset = new Thread(Reset);
                threadReset.Start();
            }
            else
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    FADM_Form.CustomMessageBox.Show("机台正在运动，不能开始", "定点移动", MessageBoxButtons.OK, true);
                else
                    FADM_Form.CustomMessageBox.Show("The machine is in motion and cannot start", "Tips", MessageBoxButtons.OK, true);
            }
        }

        private void Reset()
        {
            while (true)
            {
                if (0 == FADM_Object.Communal.ReadMachineStatus() || 8 == FADM_Object.Communal.ReadMachineStatus())
                {
                    FADM_Object.Communal.WriteDripWait(false);
                    break;
                }
                Thread.Sleep(1);
            }
        }

        private void DecompressionMove()
        {
            try
            {
                if (0 >= Convert.ToInt32(TxtNum.Text) || Convert.ToInt32(TxtNum.Text) > Lib_Card.Configure.Parameter.Machine_Cup_Total)
                {
                    MessageBox.Show("杯号输入错误");
                    FADM_Object.Communal.WriteDripWait(false);
                    FADM_Object.Communal.WriteMachineStatus(0);
                    return;
                }
                if (0 != MyModbusFun.TargetMove(4, Convert.ToInt32(TxtNum.Text), 0))
                    throw new Exception("驱动异常");
                FADM_Object.Communal.WriteMachineStatus(0);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("-2"))
                {
                    FADM_Object.Communal.WriteMachineStatus(8);
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
                                 "Debug" + "','" +
                                 s_err + "(Test)');";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                string s_insert = CardObject.InsertD(s_err, " myMachineReset");
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
                    FADM_Object.Communal.WriteMachineStatus(8);
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show(ex.Message, "定点移动", MessageBoxButtons.OK, true);
                    else
                        FADM_Form.CustomMessageBox.Show(ex.Message, "Fixed-point movement", MessageBoxButtons.OK, true);
                }
            }
        }

        private void Wash()
        {
            try
            {
                if (0 != MyModbusFun.TargetMove(12, 0, 0))
                    throw new Exception("驱动异常");
                FADM_Object.Communal.WriteMachineStatus(0);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("-2"))
                {
                    FADM_Object.Communal.WriteMachineStatus(8);
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
                                 "Debug" + "','" +
                                 s_err + "(Test)');";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                string s_insert = CardObject.InsertD(s_err, " myMachineReset");
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
                    FADM_Object.Communal.WriteMachineStatus(8);
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show(ex.Message, "定点移动", MessageBoxButtons.OK, true);
                    else
                        FADM_Form.CustomMessageBox.Show(ex.Message, "Fixed-point movement", MessageBoxButtons.OK, true);
                }
            }
        }

        private void BottleMove()
        {
            try
            {

                if (0 >= Convert.ToInt32(TxtNum.Text) || Convert.ToInt32(TxtNum.Text) > Lib_Card.Configure.Parameter.Machine_Bottle_Total)
                {
                    if (FADM_Object.Communal._b_isUseABAssistant)
                    {
                        if (Convert.ToInt32(TxtNum.Text) > Lib_Card.Configure.Parameter.Machine_Bottle_Total - FADM_Object.Communal._i_ABAssistantCount)
                        {
                            MessageBox.Show("瓶号输入错误");
                            FADM_Object.Communal.WriteDripWait(false);
                            FADM_Object.Communal.WriteMachineStatus(0);
                            return;
                        }
                    }
                    else
                    {
                        MessageBox.Show("瓶号输入错误");
                        FADM_Object.Communal.WriteDripWait(false);
                        FADM_Object.Communal.WriteMachineStatus(0);
                        return;
                    }
                }

                if (0 != MyModbusFun.TargetMove(0, Convert.ToInt32(TxtNum.Text), 0))
                    throw new Exception("驱动异常");
                FADM_Object.Communal.WriteMachineStatus(0);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("-2"))
                {
                    FADM_Object.Communal.WriteMachineStatus(8);
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
                                 "Debug" + "','" +
                                 s_err + "(Test)');";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                string s_insert = CardObject.InsertD(s_err, " myMachineReset");
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
                    FADM_Object.Communal.WriteMachineStatus(8);
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show(ex.Message, "定点移动", MessageBoxButtons.OK, true);
                    else
                        FADM_Form.CustomMessageBox.Show(ex.Message, "Fixed-point movement", MessageBoxButtons.OK, true);
                }
            }
        }

        private void CupMove()
        {
            try
            {
                if (0 >= Convert.ToInt32(TxtNum.Text) || Convert.ToInt32(TxtNum.Text) > Lib_Card.Configure.Parameter.Machine_Cup_Total)
                {
                    MessageBox.Show("杯号输入错误");
                    FADM_Object.Communal.WriteDripWait(false);
                    FADM_Object.Communal.WriteMachineStatus(0);
                    return;
                }

                if (0 != MyModbusFun.TargetMove(1, Convert.ToInt32(TxtNum.Text), 0))
                    throw new Exception("驱动异常");
                FADM_Object.Communal.WriteMachineStatus(0);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("-2"))
                {
                    FADM_Object.Communal.WriteMachineStatus(8);
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
                                 "Debug" + "','" +
                                 s_err + "(Test)');";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                string s_insert = CardObject.InsertD(s_err, " myMachineReset");
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
                    FADM_Object.Communal.WriteMachineStatus(8);
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show(ex.Message, "定点移动", MessageBoxButtons.OK, true);
                    else
                        FADM_Form.CustomMessageBox.Show(ex.Message, "Fixed-point movement", MessageBoxButtons.OK, true);
                }
            }
        }

        private void TestCoordinate()
        {
            try
            {
                if (0 >= Convert.ToInt32(TxtNum.Text) || Convert.ToInt32(TxtNum.Text) > Lib_Card.Configure.Parameter.Machine_Cup_Total)
                {
                    MessageBox.Show("杯号输入错误");
                    FADM_Object.Communal.WriteDripWait(false);
                    FADM_Object.Communal.WriteMachineStatus(0);
                    return;
                }

                try
                {

                    int i_xStart = 0, i_yStart = 0;
                    int i_xEnd = 0, i_yEnd = 0;
                    if (RdoCup.Checked)
                    {
                        MyModbusFun.CalTarget(1, Convert.ToInt32(TxtNum.Text), ref i_xStart, ref i_yStart);

                        MyModbusFun.CalTarget(1, Convert.ToInt32(TxtNum.Text), ref i_xEnd, ref i_yEnd);
                    }
                    else
                    {
                        MyModbusFun.CalTarget(4, Convert.ToInt32(TxtNum.Text), ref i_xStart, ref i_yStart);

                        MyModbusFun.CalTarget(4, Convert.ToInt32(TxtNum.Text), ref i_xEnd, ref i_yEnd);
                    }

                    int i_mRes = MyModbusFun.OpenOrPutCover(i_xStart, i_yStart, i_xEnd, i_yEnd, 2);
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                }
                catch (Exception ex)
                {
                }
                FADM_Object.Communal.WriteMachineStatus(0);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("-2"))
                {
                    FADM_Object.Communal.WriteMachineStatus(8);
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
                                 "Debug" + "','" +
                                 s_err + "(Test)');";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                string s_insert = CardObject.InsertD(s_err, " myMachineReset");
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
                    FADM_Object.Communal.WriteMachineStatus(8);
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show(ex.Message, "定点移动", MessageBoxButtons.OK, true);
                    else
                        FADM_Form.CustomMessageBox.Show(ex.Message, "Fixed-point movement", MessageBoxButtons.OK, true);
                }
            }
        }

        private void DryClothMove()
        {
            try
            {
                if (0 >= Convert.ToInt32(TxtNum.Text) || Convert.ToInt32(TxtNum.Text) > Lib_Card.Configure.Parameter.Machine_Cup_Total)
                {
                    MessageBox.Show("杯号输入错误");
                    FADM_Object.Communal.WriteDripWait(false); 
                    FADM_Object.Communal.WriteMachineStatus(0);
                    return;
                }

                if (0 != MyModbusFun.TargetMove(6, Convert.ToInt32(TxtNum.Text), 0))
                    throw new Exception("驱动异常");
                FADM_Object.Communal.WriteMachineStatus(0);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("-2"))
                {
                    FADM_Object.Communal.WriteMachineStatus(8);
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
                                 "Debug" + "','" +
                                 s_err + "(Test)');";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                string s_insert = CardObject.InsertD(s_err, " myMachineReset");
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
                    FADM_Object.Communal.WriteMachineStatus(8);
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show(ex.Message, "定点移动", MessageBoxButtons.OK, true);
                    else
                        FADM_Form.CustomMessageBox.Show(ex.Message, "Fixed-point movement", MessageBoxButtons.OK, true);
                }
            }
        }

        private void WetClothMove()
        {
            try
            {
                if (0 >= Convert.ToInt32(TxtNum.Text) || Convert.ToInt32(TxtNum.Text) > Lib_Card.Configure.Parameter.Machine_Cup_Total)
                {
                    MessageBox.Show("杯号输入错误");
                    FADM_Object.Communal.WriteDripWait(false);
                    FADM_Object.Communal.WriteMachineStatus(0);
                    return;
                }

                if (0 != MyModbusFun.TargetMove(7, Convert.ToInt32(TxtNum.Text), 0))
                    throw new Exception("驱动异常");
                FADM_Object.Communal.WriteMachineStatus(0);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("-2"))
                {
                    FADM_Object.Communal.WriteMachineStatus(8);
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
                                 "Debug" + "','" +
                                 s_err + "(Test)');";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                string s_insert = CardObject.InsertD(s_err, " myMachineReset");
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
                    FADM_Object.Communal.WriteMachineStatus(8);
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show(ex.Message, "定点移动", MessageBoxButtons.OK, true);
                    else
                        FADM_Form.CustomMessageBox.Show(ex.Message, "Fixed-point movement", MessageBoxButtons.OK, true);
                }
            }
        }

        private void CupStress()
        {
            try
            {
                if (0 >= Convert.ToInt32(TxtNum.Text) || Convert.ToInt32(TxtNum.Text) > Lib_Card.Configure.Parameter.Machine_Cup_Total)
                {
                    MessageBox.Show("杯号输入错误");
                    FADM_Object.Communal.WriteDripWait(false);
                    FADM_Object.Communal.WriteMachineStatus(0);
                    return;
                }
                if (0 != MyModbusFun.TargetMove(5, Convert.ToInt32(TxtNum.Text), 0))
                    throw new Exception("驱动异常");
                FADM_Object.Communal.WriteMachineStatus(0);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("-2"))
                {
                    FADM_Object.Communal.WriteMachineStatus(8);
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
                                 "Debug" + "','" +
                                 s_err + "(Test)');";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                string s_insert = CardObject.InsertD(s_err, " myMachineReset");
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
                    FADM_Object.Communal.WriteMachineStatus(8);
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show(ex.Message, "定点移动", MessageBoxButtons.OK, true);
                    else
                        FADM_Form.CustomMessageBox.Show(ex.Message, "Fixed-point movement", MessageBoxButtons.OK, true);
                }
            }
        }

        private void BalanceMove()
        {
            try
            {
                if (0 != MyModbusFun.TargetMove(2, 0, 0))
                    throw new Exception("驱动异常");
                FADM_Object.Communal.WriteMachineStatus(0);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("-2"))
                {
                    FADM_Object.Communal.WriteMachineStatus(8);
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
                                 "Debug" + "','" +
                                 s_err + "(Test)');";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                string s_insert = CardObject.InsertD(s_err, " myMachineReset");
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
                    FADM_Object.Communal.WriteMachineStatus(8);
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show(ex.Message, "定点移动", MessageBoxButtons.OK, true);
                    else
                        FADM_Form.CustomMessageBox.Show(ex.Message, "Fixed-point movement", MessageBoxButtons.OK, true);
                }
            }
        }

        private void DryClampMove()
        {
            try
            {
                if (0 != MyModbusFun.TargetMove(8, 0, 0))
                    throw new Exception("驱动异常");
                FADM_Object.Communal.WriteMachineStatus(0);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("-2"))
                {
                    FADM_Object.Communal.WriteMachineStatus(8);
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
                                 "Debug" + "','" +
                                 s_err + "(Test)');";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                string s_insert = CardObject.InsertD(s_err, " myMachineReset");
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
                    FADM_Object.Communal.WriteMachineStatus(8);
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show(ex.Message, "定点移动", MessageBoxButtons.OK, true);
                    else
                        FADM_Form.CustomMessageBox.Show(ex.Message, "Fixed-point movement", MessageBoxButtons.OK, true);
                }
            }
        }

        private void WetClampMove()
        {
            try
            {
                if (0 != MyModbusFun.TargetMove(9, 0, 0))
                    throw new Exception("驱动异常");
                FADM_Object.Communal.WriteMachineStatus(0);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("-2"))
                {
                    FADM_Object.Communal.WriteMachineStatus(8);
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
                                 "Debug" + "','" +
                                 s_err + "(Test)');";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                string s_insert = CardObject.InsertD(s_err, " myMachineReset");
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
                    FADM_Object.Communal.WriteMachineStatus(8);
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show(ex.Message, "定点移动", MessageBoxButtons.OK, true);
                    else
                        FADM_Form.CustomMessageBox.Show(ex.Message, "Fixed-point movement", MessageBoxButtons.OK, true);
                }
            }
        }

        private void StandByMove()
        {
            try
            {
                if (0 != MyModbusFun.TargetMove(3, 0, 0))
                    throw new Exception("驱动异常");
                FADM_Object.Communal.WriteMachineStatus(0);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("-2"))
                {
                    FADM_Object.Communal.WriteMachineStatus(8);
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
                                 "Debug" + "','" +
                                 s_err + "(Test)');";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                string s_insert = CardObject.InsertD(s_err, " myMachineReset");
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
                    FADM_Object.Communal.WriteMachineStatus(8);
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show(ex.Message, "定点移动", MessageBoxButtons.OK, true);
                    else
                        FADM_Form.CustomMessageBox.Show(ex.Message, "Fixed-point movement", MessageBoxButtons.OK, true);
                }
            }
        }

        private void ExtractionMove()
        {
            try
            {
                if (0 != MyModbusFun.TargetMove(11, 0, 0))
                    throw new Exception("驱动异常");
                FADM_Object.Communal.WriteMachineStatus(0);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("-2"))
                {
                    FADM_Object.Communal.WriteMachineStatus(8);
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
                                 "Debug" + "','" +
                                 s_err + "(Test)');";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                string s_insert = CardObject.InsertD(s_err, " myMachineReset");
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
                    FADM_Object.Communal.WriteMachineStatus(8);
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show(ex.Message, "定点移动", MessageBoxButtons.OK, true);
                    else
                        FADM_Form.CustomMessageBox.Show(ex.Message, "Fixed-point movement", MessageBoxButtons.OK, true);
                }
            }
        }

        private void AbsMove()
        {
            try
            {
                if (0 >= Convert.ToInt32(TxtNum.Text) || 4 < Convert.ToInt32(TxtNum.Text))
                {
                    MessageBox.Show("杯号输入错误");
                    FADM_Object.Communal.WriteDripWait(false);
                    FADM_Object.Communal.WriteMachineStatus(0);
                    return;
                }

                if (0 != MyModbusFun.TargetMove(10, Convert.ToInt32(TxtNum.Text), 0))
                    throw new Exception("驱动异常");
                FADM_Object.Communal.WriteMachineStatus(0);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("-2"))
                {
                    FADM_Object.Communal.WriteMachineStatus(8);
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
                                 "Debug" + "','" +
                                 s_err + "(Test)');";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                string s_insert = CardObject.InsertD(s_err, " myMachineReset");
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
                    FADM_Object.Communal.WriteMachineStatus(8);
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show(ex.Message, "定点移动", MessageBoxButtons.OK, true);
                    else
                        FADM_Form.CustomMessageBox.Show(ex.Message, "Fixed-point movement", MessageBoxButtons.OK, true);
                }
            }
        }

        private void SupportCoverMove()
        {
            try
            {
                if (0 >= Convert.ToInt32(TxtNum.Text) || Convert.ToInt32(TxtNum.Text) > Lib_Card.Configure.Parameter.Machine_Cup_Total)
                {
                    MessageBox.Show("杯号输入错误");
                    FADM_Object.Communal.WriteDripWait(false);
                    FADM_Object.Communal.WriteMachineStatus(0);
                    return;
                }

                if (0 != MyModbusFun.TargetMove(13, Convert.ToInt32(TxtNum.Text), 0))
                    throw new Exception("驱动异常");
                FADM_Object.Communal.WriteMachineStatus(0);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("-2"))
                {
                    FADM_Object.Communal.WriteMachineStatus(8);
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
                                 "Debug" + "','" +
                                 s_err + "(Test)');";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                string s_insert = CardObject.InsertD(s_err, " myMachineReset");
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
                    FADM_Object.Communal.WriteMachineStatus(8);
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show(ex.Message, "定点移动", MessageBoxButtons.OK, true);
                    else
                        FADM_Form.CustomMessageBox.Show(ex.Message, "Fixed-point movement", MessageBoxButtons.OK, true);
                }
            }
        }

        private void PHMove()
        {
            try
            {
                if (0 != MyModbusFun.TargetMove(17, 0, 0))
                    throw new Exception("驱动异常");
                FADM_Object.Communal.WriteMachineStatus(0);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("-2"))
                {
                    FADM_Object.Communal.WriteMachineStatus(8);
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
                                 "Debug" + "','" +
                                 s_err + "(Test)');";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                string s_insert = CardObject.InsertD(s_err, " myMachineReset");
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
                    FADM_Object.Communal.WriteMachineStatus(8);
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show(ex.Message, "定点移动", MessageBoxButtons.OK, true);
                    else
                        FADM_Form.CustomMessageBox.Show(ex.Message, "Fixed-point movement", MessageBoxButtons.OK, true);
                }
            }
        }

        private void SalinityMove()
        {
            try
            {
                if (0 != MyModbusFun.TargetMove(18, 0, 0))
                    throw new Exception("驱动异常");
                FADM_Object.Communal.WriteMachineStatus(0);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("-2"))
                {
                    FADM_Object.Communal.WriteMachineStatus(8);
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
                                 "Debug" + "','" +
                                 s_err + "(Test)');";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                string s_insert = CardObject.InsertD(s_err, " myMachineReset");
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
                    FADM_Object.Communal.WriteMachineStatus(8);
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show(ex.Message, "定点移动", MessageBoxButtons.OK, true);
                    else
                        FADM_Form.CustomMessageBox.Show(ex.Message, "Fixed-point movement", MessageBoxButtons.OK, true);
                }
            }
        }

        private void WashBottleMove()
        {
            try
            {
                if (0 != MyModbusFun.TargetMove(15, 0, 0))
                    throw new Exception("驱动异常");
                FADM_Object.Communal.WriteMachineStatus(0);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("-2"))
                {
                    FADM_Object.Communal.WriteMachineStatus(8);
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
                                 "Debug" + "','" +
                                 s_err + "(Test)');";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                string s_insert = CardObject.InsertD(s_err, " myMachineReset");
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
                    FADM_Object.Communal.WriteMachineStatus(8);
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show(ex.Message, "定点移动", MessageBoxButtons.OK, true);
                    else
                        FADM_Form.CustomMessageBox.Show(ex.Message, "Fixed-point movement", MessageBoxButtons.OK, true);
                }
            }
        }

        private void PowerMove()
        {
            try
            {
                if (0 != MyModbusFun.TargetMove(14, 0, 0))
                    throw new Exception("驱动异常");
                FADM_Object.Communal.WriteMachineStatus(0);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("-2"))
                {
                    FADM_Object.Communal.WriteMachineStatus(8);
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
                                 "Debug" + "','" +
                                 s_err + "(Test)');";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                string s_insert = CardObject.InsertD(s_err, " myMachineReset");
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
                    FADM_Object.Communal.WriteMachineStatus(8);
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show(ex.Message, "定点移动", MessageBoxButtons.OK, true);
                    else
                        FADM_Form.CustomMessageBox.Show(ex.Message, "Fixed-point movement", MessageBoxButtons.OK, true);
                }
            }
        }

        private void BrewMove()
        {
            try
            {
                if (0 >= Convert.ToInt32(TxtNum.Text) || Convert.ToInt32(TxtNum.Text) > 6)
                {
                    MessageBox.Show("杯号输入错误");
                    FADM_Object.Communal.WriteDripWait(false);
                    FADM_Object.Communal.WriteMachineStatus(0);
                    return;
                }
                if (0 != MyModbusFun.TargetMove(16, Convert.ToInt32(TxtNum.Text), 0))
                    throw new Exception("驱动异常");
                FADM_Object.Communal.WriteMachineStatus(0);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("-2"))
                {
                    FADM_Object.Communal.WriteMachineStatus(8);
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
                                 "Debug" + "','" +
                                 s_err + "(Test)');";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                string s_insert = CardObject.InsertD(s_err, " myMachineReset");
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
                    FADM_Object.Communal.WriteMachineStatus(8);
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show(ex.Message, "定点移动", MessageBoxButtons.OK, true);
                    else
                        FADM_Form.CustomMessageBox.Show(ex.Message, "Fixed-point movement", MessageBoxButtons.OK, true);
                }
            }
        }

        private void SysFinishMove()
        {
            try
            {
                if (0 >= Convert.ToInt32(TxtNum.Text) || Convert.ToInt32(TxtNum.Text) > 6)
                {
                    MessageBox.Show("杯号输入错误");
                    FADM_Object.Communal.WriteDripWait(false);
                    FADM_Object.Communal.WriteMachineStatus(0);
                    return;
                }
                if (0 != MyModbusFun.TargetMove(19, Convert.ToInt32(TxtNum.Text), 0))
                    throw new Exception("驱动异常");
                FADM_Object.Communal.WriteMachineStatus(0);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("-2"))
                {
                    FADM_Object.Communal.WriteMachineStatus(8);
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
                                 "Debug" + "','" +
                                 s_err + "(Test)');";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                string s_insert = CardObject.InsertD(s_err, " myMachineReset");
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
                    FADM_Object.Communal.WriteMachineStatus(8);
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show(ex.Message, "定点移动", MessageBoxButtons.OK, true);
                    else
                        FADM_Form.CustomMessageBox.Show(ex.Message, "Fixed-point movement", MessageBoxButtons.OK, true);
                }
            }
        }

        private void PHTestMove()
        {
            try
            {
                if (0 >= Convert.ToInt32(TxtNum.Text) || Convert.ToInt32(TxtNum.Text) > Lib_Card.Configure.Parameter.Machine_Cup_Total)
                {
                    MessageBox.Show("杯号输入错误");
                    FADM_Object.Communal.WriteDripWait(false);
                    FADM_Object.Communal.WriteMachineStatus(0);
                    return;
                }
                if (0 != MyModbusFun.TargetMove(20, Convert.ToInt32(TxtNum.Text), 0))
                    throw new Exception("驱动异常");
                FADM_Object.Communal.WriteMachineStatus(0);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("-2"))
                {
                    FADM_Object.Communal.WriteMachineStatus(8);
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
                                 "Debug" + "','" +
                                 s_err + "(Test)');";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                string s_insert = CardObject.InsertD(s_err, " myMachineReset");
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
                    FADM_Object.Communal.WriteMachineStatus(8);
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show(ex.Message, "定点移动", MessageBoxButtons.OK, true);
                    else
                        FADM_Form.CustomMessageBox.Show(ex.Message, "Fixed-point movement", MessageBoxButtons.OK, true);
                }
            }
        }

        private void BtnHome_Click(object sender, EventArgs e)
        {
            FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "点击回零");
            _b_istrue = true;
            if ((0 == FADM_Object.Communal.ReadMachineStatus() || 8 == FADM_Object.Communal.ReadMachineStatus()) && null == FADM_Object.Communal.ReadDyeThread()&& FADM_Object.Communal.ReadTcpStatus())
            {
                FADM_Object.Communal.WriteDripWait(true);
                FADM_Object.Communal.WriteMachineStatus(1);
                Thread thread = new Thread(Home);
                thread.Start();

                Thread threadReset = new Thread(Reset);
                threadReset.Start();

            }
            else
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    FADM_Form.CustomMessageBox.Show("机台正在运动，不能开始", "回零", MessageBoxButtons.OK, true);
                else
                    FADM_Form.CustomMessageBox.Show("The machine is in motion and cannot start", "Tips", MessageBoxButtons.OK, true);
            }

        }

        private void Home()
        {
            try
            {
                //FADM_Auto.Reset.IOReset();//这里会检查是否有针筒
                int i_state = MyModbusFun.goHome();
                    if (0 != i_state && -2 != i_state)

                    throw new Exception("驱动异常");
                else
                    FADM_Object.Communal.WriteMachineStatus(0);
            }
            catch (Exception ex)
            {
                _b_istrue = false;
                FADM_Object.Communal.WriteMachineStatus(8);

                if (ex.Message.Equals("-2"))
                {
                    FADM_Object.Communal.WriteMachineStatus(8);
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
                                 "Home" + "','" +
                                 s_err + "(Test)');";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                string s_insert = CardObject.InsertD(s_err, " myMachineReset");
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
                else {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show(ex.Message, "回零", MessageBoxButtons.OK, true);
                    else
                        FADM_Form.CustomMessageBox.Show(ex.Message, "zeroing", MessageBoxButtons.OK, true);
                }

                
            }
            finally {
                _b_istrue = false;
            }
        }


        private void BtnCoordinate_Click(object sender, EventArgs e)
        {
           
        }

        private void Coordinate()
        {
         
        }

        private void BtnMove_Click(object sender, EventArgs e)
        {
            FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "点击联动");
            if ((0 == FADM_Object.Communal.ReadMachineStatus() || 8 == FADM_Object.Communal.ReadMachineStatus()) && null == FADM_Object.Communal.ReadDyeThread() && FADM_Object.Communal.ReadTcpStatus())
            {
                FADM_Object.Communal.WriteDripWait(true);
                if (RdoX.Checked)
                {
                    FADM_Object.Communal.WriteMachineStatus(3);
                    Thread thread = new Thread(X);
                    thread.Start();
                }
                else if (RdoY.Checked)
                {
                    FADM_Object.Communal.WriteMachineStatus(3);
                    Thread thread = new Thread(Y);
                    thread.Start();
                }
                else if (RdoZ.Checked)
                {
                    FADM_Object.Communal.WriteMachineStatus(3);
                    Thread thread = new Thread(Z);
                    thread.Start();
                }
                else
                {

                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show("请选择轴号！", "温馨提示", MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("Please select the axis number！", "Tips", MessageBoxButtons.OK, true);
                }
                Thread threadReset = new Thread(Reset);
                threadReset.Start();
            }
            else
            {
                FADM_Form.CustomMessageBox.Show("机台正在运动，不能开始", "联动", MessageBoxButtons.OK, true);
            }
        }

        private void X()
        {
            try
            {
                _b_istrue = false;
                int i_state = MyModbusFun.TargetMoveRelative(1, Convert.ToInt32(this.TxtPulseX.Text), Convert.ToInt32(this.TxtHSpeedX.Text), Convert.ToInt32(this.TxtUpTimeX.Text), Convert.ToInt32(this.TxtLSpeedX.Text));
                if (i_state != 0&& i_state != -2)
                    throw new Exception("驱动异常");
                else
                    FADM_Object.Communal.WriteMachineStatus(0);
            }
            catch (Exception ex)
            {
                FADM_Object.Communal.WriteMachineStatus(8);
                if (ex.Message.Equals("-2"))
                {
                    FADM_Object.Communal.WriteMachineStatus(8);
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
                                 "X" + "','" +
                                 s_err + "(Test)');";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                string s_insert = CardObject.InsertD(s_err, " myMachineReset");
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
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show(ex.Message, "联动", MessageBoxButtons.OK, true);
                    else
                        FADM_Form.CustomMessageBox.Show(ex.Message, "linkage", MessageBoxButtons.OK, true);
                }
            }
            finally {
                _b_istrue = false;
            }

        }

        private void Y()
        {
            try
            {
                _b_istrue = true;
                int state = MyModbusFun.TargetMoveRelative(2, Convert.ToInt32(this.TxtPulseY.Text), Convert.ToInt32(this.TxtHSpeedY.Text), Convert.ToInt32(this.TxtUpTimeY.Text), Convert.ToInt32(this.TxtLSpeedX.Text));
                if (state != 0 && state != -2)
                    throw new Exception("驱动异常");
                else
                    FADM_Object.Communal.WriteMachineStatus(0);
            }
            catch (Exception ex)
            {
                FADM_Object.Communal.WriteMachineStatus(8);
                if (ex.Message.Equals("-2"))
                {
                    //string[] strArray = { "", "" };
                    ////根据编号读取异常信息
                    //MyModbusFun.GetErrMsg(ref strArray);
                    //if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    //    FADM_Form.CustomMessageBox.Show(strArray[1], "联动", MessageBoxButtons.OK, true);
                    //else
                    //    FADM_Form.CustomMessageBox.Show(strArray[1], "linkage", MessageBoxButtons.OK, true);

                    int[] ErrArray = new int[100];
                    MyModbusFun.GetErrMsgNew(ref ErrArray);

                    List<string> stringErr = new List<string>();
                    for (int i = 0; i < ErrArray.Length; i++)
                    {
                        if (ErrArray[i] != 0)
                        {
                            if (SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.ContainsKey(ErrArray[i]))
                            {
                                string strErr = SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew[ErrArray[i]];
                                string Str = "INSERT INTO alarm_table" +
                                 "(MyDate,MyTime,AlarmHead,AlarmDetails)" +
                                 " VALUES( '" +
                                 String.Format("{0:d}", DateTime.Now) + "','" +
                                 String.Format("{0:T}", DateTime.Now) + "','" +
                                 "Y" + "','" +
                                 strErr + "(Test)');";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(Str);

                                string s = CardObject.InsertD(strErr, " Y");
                                if (!stringErr.Contains(s))
                                    stringErr.Add(s);
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
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show(ex.Message, "联动", MessageBoxButtons.OK, true);
                    else
                        FADM_Form.CustomMessageBox.Show(ex.Message, "linkage", MessageBoxButtons.OK, true);
                }
            }
            finally {
                _b_istrue = false;
            }
        }

        private void Z()
        {
            try
            {
                _b_istrue = true;
                int state = MyModbusFun.TargetMoveRelative(3, Convert.ToInt32(this.TxtPulseZ.Text), Convert.ToInt32(this.TxtHSpeedZ.Text), Convert.ToInt32(this.TxtUpTimeZ.Text), Convert.ToInt32(this.TxtLSpeedX.Text));
                if (state != 0 && state != -2)
                    throw new Exception("驱动异常");
                else
                    FADM_Object.Communal.WriteMachineStatus(0);
            }
            catch (Exception ex)
            {
                FADM_Object.Communal.WriteMachineStatus(8);
                if (ex.Message.Equals("-2"))
                {
                    FADM_Object.Communal.WriteMachineStatus(8);
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
                                 "Z" + "','" +
                                 s_err + "(Test)');";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                string s_insert = CardObject.InsertD(s_err, " myMachineReset");
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
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show(ex.Message, "联动", MessageBoxButtons.OK, true);
                    else
                        FADM_Form.CustomMessageBox.Show(ex.Message, "linkage", MessageBoxButtons.OK, true);
                }
            }
            finally {
                _b_istrue = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void Debug_Load(object sender, EventArgs e)
        {
            //FADM_Object.Communal._b_pause = true;
            if (Lib_Card.Configure.Parameter.Machine_Decompression == 1 || Lib_Card.Configure.Parameter.Machine_Decompression == 0)
            {
                //BtnOutPut_Decompression_Right.Visible = false;
                //ChkInPut_Decompression_Up_Right.Visible = false;
                //ChkInPut_Decompression_Down_Right.Visible = false;
            }
            if (Lib_Card.Configure.Parameter.Other_IsOnlyDrip == 1)
            {
                RdoDecompression.Enabled = false;
                BtnOutPut_Decompression.Enabled = false;
            }

            if (Lib_Card.Configure.Parameter.Machine_UseBack == 0)
            {
                ChkInPut_Back.Visible = false;
            }

            if (!FADM_Object.Communal._b_isHasWashSyringe)
            {
                RdoWash.Visible = false;
            }

            if (Lib_Card.Configure.Parameter.Other_UseAbs == 0)
            {
                RdoExtraction.Visible = false;
                RdoAbs.Visible = false;
            }

            if (!FADM_Object.Communal._b_isHaveAutoBrew)
            {
                RdoPower.Visible = false;
                RdoSysFinish.Visible = false;
                RdoWashBottle.Visible = false;
                RdoBrew.Visible = false;
            }
            if (!FADM_Object.Communal._b_isUsePH)
            {
                RdoPH.Visible = false;
                RdoPHTest.Visible = false;
                RdoSalinity.Visible = false;
            }
        }

        private void Debug_Leave(object sender, EventArgs e)
        {
            //FADM_Object.Communal._b_pause = false;
            if (Lib_Card.Configure.Parameter.Machine_IsStopOrFront == 0)
            {
                Lib_Card.CardObject.bStopScr = false;
            }
            else
            {
                Lib_Card.CardObject.bFront = false;
            }
            Lib_Card.CardObject.bRight = false;
            Lib_Card.CardObject.bLeft = false;
        }

        private void BtnOutPut_Decompression_Right_Click(object sender, EventArgs e)
        {
            //try {
            //    _b_istrue = true;
            //    if (this.BtnOutPut_Decompression_Right.ForeColor == Color.Red)
            //    {
            //        _ia_array[0] = 25;
            //    }
            //    else
            //    {
            //        _ia_array[0] = 26;
            //    }
            //    int i_state = FADM_Object.Communal._tcpModBus.Write(811, _ia_array);

            //} catch { } finally {
            //    _b_istrue = false ;
            //}
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            { }
            else
            {
                try
                {
                    _b_istrue = true;
                    _ia_array[0] = 27;
                    int i_state = FADM_Object.Communal._tcpModBus.Write(811, _ia_array);

                }
                catch { }
                finally
                {
                    _b_istrue = false;
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            { }
            else
            {
                try
                {
                    _b_istrue = true;
                    _ia_array[0] = 28;
                    int i_state = FADM_Object.Communal._tcpModBus.Write(811, _ia_array);

                }
                catch { }
                finally
                {
                    _b_istrue = false;
                }
            }
        }

        private void btn_reset_a_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    btn_reset_a.Enabled = false;
            //    _b_istrue = true;
            //    _ia_array[0] = 29;
            //    int i_state = FADM_Object.Communal._tcpModBus.Write(811, _ia_array);
            //    if (i_state != -1)
            //    {
            //        FADM_Form.CustomMessageBox.Show("清零成功!", "温馨提示", MessageBoxButtons.OK, false);
            //    }
            //    else {
            //        FADM_Form.CustomMessageBox.Show("清零失败!", "温馨提示", MessageBoxButtons.OK, false);
            //    }
            //    btn_reset_a.Enabled = true;
            //}
            //catch
            //{

            //}
            //finally
            //{
            //    _b_istrue = false;
            //}

        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            ////计算
            //string s = "-0.000648/-0.000287/-0.000333/-0.000583/-0.000394/-0.000120/-0.000225/0.000000/0.000256/-0.001036/-0.000969/-0.000590/-0.000596/-0.000544/-0.000388/-0.000527/-0.000480/0.000074/-0.000140/-0.000724/-0.000897/-0.000866/-0.000298/0.000345/-0.000111/-0.000159/0.000153/-0.000534/0.000093/-0.000353/0.000000/0.000040/0.000038/-0.000256/0.000071/-0.000653/-0.000605/0.000165/0.000075/0.000000/0.000000/-0.000074/0.000437/0.000215/-0.000281/0.000069/0.000879/-0.000331/-0.000389/0.000254/0.000125/-0.000677/-0.000305/-0.000607/-0.000543/-0.000359/-0.000473/0.000058/0.000633/-0.000343/-0.000117/0.000394/-0.000079/0.000072/0.000344/-0.000170/-0.000341/0.000379/-0.000105/-0.000071/-0.000216/0.000586/-0.000225/0.000956/0.000352/0.000243/-0.000503/-0.000339/0.000215/0.000043/0.001053";
            //string[] sa_e1 = s.Split('/');
            //List<string> lis_sta_e1 = sa_e1.ToList();
            //double[] d =new double[lis_sta_e1.Count];
            //for(int i = 0;i< lis_sta_e1.Count;i++)
            //{
            //    d[i] = Convert.ToDouble(lis_sta_e1[i]);
            //}

            //// 转换为LAB值
            //(double L, double A, double B) = ConvertToLAB(d, false);

            //Console.WriteLine($"L: {L}, A: {A}, B: {B}");
            //return;
            //FADM_Object.Communal._b_stop = true;

            //SmartDyeing.FADM_Auto.MyAbsorbance.Abs();
        }

        // CIE 1931 2° Standard Observer
        private static readonly double[] CIE_X_2 = {
        0.001368, 0.002236, 0.004243, 0.00765, 0.01431, 0.02319, 0.04351, 0.07763, 0.13438, 0.21477,
        0.2839, 0.3285, 0.34828, 0.34806, 0.3362, 0.3187, 0.2908, 0.2511, 0.19536, 0.1421,
        0.09564, 0.05795, 0.03201, 0.0147, 0.0049, 0.0024, 0.0093, 0.0291, 0.06327, 0.1096,
        0.1655, 0.22575, 0.2904, 0.3597, 0.43345, 0.51205, 0.5945, 0.6784, 0.7621, 0.8425,
        0.9163, 0.9786, 1.0263, 1.0567, 1.0622, 1.0456, 1.0026, 0.9384, 0.85445, 0.7514,
        0.6424, 0.5419, 0.4479, 0.3608, 0.2835, 0.2187, 0.1649, 0.1212, 0.0874, 0.0636,
        0.04677, 0.0329, 0.0227, 0.01584, 0.011359, 0.008111, 0.00579, 0.004109, 0.002899, 0.002049,
        0.001439, 0.000999, 0.00069, 0.000476, 0.000332, 0.000235, 0.000166, 0.000117, 0.000083, 0.000059,
        0.000042
    };

        private static readonly double[] CIE_Y_2 = {
        0.000039, 0.000064, 0.00012, 0.000217, 0.000396, 0.00064, 0.00121, 0.00218, 0.004, 0.0073,
        0.0116, 0.01684, 0.023, 0.0298, 0.038, 0.048, 0.06, 0.0739, 0.09098, 0.1126,
        0.13902, 0.1693, 0.20802, 0.2586, 0.323, 0.4073, 0.503, 0.6082, 0.71, 0.7932,
        0.862, 0.91485, 0.954, 0.9803, 0.99495, 1, 0.995, 0.9786, 0.952, 0.9154,
        0.87, 0.8163, 0.757, 0.6949, 0.631, 0.5668, 0.503, 0.4412, 0.381, 0.321,
        0.265, 0.217, 0.175, 0.1382, 0.107, 0.0816, 0.061, 0.04458, 0.032, 0.0232,
        0.017, 0.01192, 0.00821, 0.005723, 0.004102, 0.002929, 0.002091, 0.001484, 0.001047, 0.00074,
        0.00052, 0.000361, 0.000249, 0.000172, 0.00012, 0.000085, 0.00006, 0.000042, 0.00003, 0.000021,
        0.000015
    };

        private static readonly double[] CIE_Z_2 = {
        0.00645, 0.01055, 0.02005, 0.03621, 0.06785, 0.1102, 0.2074, 0.3713, 0.6456, 1.03905,
        1.3856, 1.62296, 1.74706, 1.7826, 1.77211, 1.7441, 1.6692, 1.5281, 1.28764, 1.0419,
        0.81295, 0.6162, 0.46518, 0.3533, 0.272, 0.2123, 0.1582, 0.1117, 0.07825, 0.05725,
        0.04216, 0.02984, 0.0203, 0.0134, 0.00875, 0.00575, 0.0039, 0.00275, 0.0021, 0.0018,
        0.00165, 0.0014, 0.0011, 0.001, 0.0008, 0.0006, 0.00034, 0.00024, 0.00019, 0.0001,
        0.00005, 0.00003, 0.00002, 0.00001, 0.00001, 0.00001, 0.00001, 0.00001, 0.00001, 0.00001,
        0.00001, 0.00001, 0.00001, 0.00001, 0.00001, 0.00001, 0.00001, 0.00001, 0.00001, 0.00001,
        0.00001, 0.00001, 0.00001, 0.00001, 0.00001, 0.00001, 0.00001, 0.00001, 0.00001, 0.00001,
        0.00001
    };

        // CIE 1964 10° Standard Observer
        private static readonly double[] CIE_X_10 = {
        0.000159952, 0.00066244, 0.0023616, 0.0072423, 0.0191097, 0.0434, 0.084736, 0.140638, 0.204492, 0.264737,
        0.314679, 0.357719, 0.383734, 0.386726, 0.370702, 0.342957, 0.302273, 0.254085, 0.195618, 0.132349,
        0.080507, 0.041072, 0.016172, 0.005132, 0.003816, 0.015444, 0.037465, 0.071358, 0.117749, 0.172953,
        0.236491, 0.304213, 0.376772, 0.451584, 0.529826, 0.616053, 0.705224, 0.793832, 0.878655, 0.951162,
        1.01416, 1.0743, 1.11852, 1.1343, 1.12399, 1.0891, 1.03048, 0.95074, 0.856297, 0.75493,
        0.647467, 0.53511, 0.431567, 0.34369, 0.268329, 0.2043, 0.152568, 0.11221, 0.0812606, 0.05793,
        0.0408508, 0.028623, 0.0199413, 0.013842, 0.00957688, 0.0066052, 0.00455263, 0.0031447, 0.00217496, 0.0015057,
        0.00104476, 0.00072745, 0.000508258, 0.00035638, 0.000250969, 0.00017773, 0.00012639, 0.000090151, 0.0000645258, 0.000046339,
        0.0000334117
    };

        private static readonly double[] CIE_Y_10 = {
        0.000017364, 0.00007156, 0.0002534, 0.000769, 0.0020044, 0.004509, 0.008756, 0.014456, 0.021391, 0.029497,
        0.038676, 0.049602, 0.062077, 0.074704, 0.089456, 0.106256, 0.128201, 0.152761, 0.18519, 0.21994,
        0.253589, 0.297665, 0.339133, 0.395379, 0.460777, 0.53136, 0.606741, 0.68566, 0.761757, 0.82333,
        0.875211, 0.92381, 0.961988, 0.9822, 0.991761, 0.99911, 0.99734, 0.98238, 0.955552, 0.915175,
        0.868934, 0.825623, 0.777405, 0.720353, 0.658341, 0.593878, 0.527963, 0.461834, 0.398057, 0.339554,
        0.283493, 0.228254, 0.179828, 0.140211, 0.107633, 0.081187, 0.060281, 0.044096, 0.0318004, 0.022602,
        0.0159051, 0.0111303, 0.0077488, 0.0053751, 0.00371774, 0.00256456, 0.00176847, 0.00122239, 0.00084619, 0.00058644,
        0.00040741, 0.000284041, 0.00019873, 0.00013955, 0.000098428, 0.000069819, 0.000049737, 0.0000355405, 0.000025486, 0.000018338,
        0.000013249
    };

        private static readonly double[] CIE_Z_10 = {
        0.000705224, 0.002928, 0.0104822, 0.032344, 0.0860109, 0.19712, 0.389366, 0.65676, 0.972542, 1.2825,
        1.55348, 1.7985, 1.96728, 2.0273, 1.9948, 1.9007, 1.74537, 1.5549, 1.31756, 1.0302,
        0.772125, 0.57006, 0.415254, 0.302356, 0.218502, 0.159249, 0.112044, 0.082248, 0.060709, 0.04305,
        0.030451, 0.020584, 0.013676, 0.007918, 0.003988, 0.001091, 0.000000, 0.000000, 0.000000, 0.000000,
        0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000,
        0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000,
        0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000,
        0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000,
        0.000000
    };

        // Reference white (D65)
        private const double Xn = 95.047;
        private const double Yn = 100.000;
        private const double Zn = 108.883;

        public static (double L, double A, double B) ConvertToLAB(double[] absorbance, bool use10DegreeObserver = false)
        {
            if (absorbance.Length != 81)
            {
                throw new ArgumentException("Absorbance array must have 81 elements (380nm to 780nm, 5nm interval).");
            }

            double[] CIE_X = use10DegreeObserver ? CIE_X_10 : CIE_X_2;
            double[] CIE_Y = use10DegreeObserver ? CIE_Y_10 : CIE_Y_2;
            double[] CIE_Z = use10DegreeObserver ? CIE_Z_10 : CIE_Z_2;

            double X = 0, Y = 0, Z = 0;

            for (int i = 0; i < 81; i++)
            {
                X += absorbance[i] * CIE_X[i];
                Y += absorbance[i] * CIE_Y[i];
                Z += absorbance[i] * CIE_Z[i];
            }

            // Normalize XYZ values
            X /= Yn;
            Y /= Yn;
            Z /= Yn;

            // Convert XYZ to LAB
            double fx = F(X / Xn);
            double fy = F(Y / Yn);
            double fz = F(Z / Zn);

            double L = 116 * fy - 16;
            double A = 500 * (fx - fy);
            double B = 200 * (fy - fz);

            return (L, A, B);
        }

        private static double F(double t)
        {
            const double delta = 6.0 / 29.0;
            if (t > Math.Pow(delta, 3))
            {
                return Math.Pow(t, 1.0 / 3.0);
            }
            else
            {
                return t / (3 * Math.Pow(delta, 2)) + 4.0 / 29.0;
            }
        }

        private void Slow()
        {
            CylinderMo cylinderMo = new CylinderMo();
            if (-1 == cylinderMo.CylinderSlow(Lib_Card.Configure.Parameter.Machine_CylinderVersion, 1))
                return;
        }

        private void Block()
        {
            CylinderMo cylinderMo = new CylinderMo();
            if (-1 == cylinderMo.CylinderBlock(1))
                return;
        }

        private void BtnOutPut_Slow_Click(object sender, EventArgs e)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                if (Lib_Card.Configure.Parameter.Machine_BlockCylinder != 0)
                {
                    //气缸慢速下
                    Thread thread = new Thread(Slow);
                    thread.Start();
                }
            }
            else
            {
                try
                {
                    _b_istrue = true;
                    _ia_array[0] = 36;
                    int i_state = FADM_Object.Communal._tcpModBus.Write(811, _ia_array);

                }
                catch { }
                finally
                {
                    _b_istrue = false;
                }
            }
        }

        private void BtnOutPut_Block_Cylinder_Click(object sender, EventArgs e)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                if (Lib_Card.Configure.Parameter.Machine_BlockCylinder != 0)
                {
                    //气缸到阻挡位
                    Thread thread = new Thread(Block);
                    thread.Start();
                }
            }
            else
            {
                try
                {
                    _b_istrue = true;
                    _ia_array[0] = 37;
                    int i_state = FADM_Object.Communal._tcpModBus.Write(811, _ia_array);

                }
                catch { }
                finally
                {
                    _b_istrue = false;
                }
            }
        }

        private void button2_Click_2(object sender, EventArgs e)
        {
            FADM_Object.Communal._fadmSqlserver.InsertRun("Machine", "测试启动");
            if ((0 == FADM_Object.Communal.ReadMachineStatus() || 8 == FADM_Object.Communal.ReadMachineStatus()) && null == FADM_Object.Communal.ReadDyeThread() && FADM_Object.Communal.ReadTcpStatus())
            {
                FADM_Object.Communal.WriteDripWait(true);
                if (RdoCup.Checked)
                {
                    if (string.IsNullOrEmpty(TxtNum.Text))
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("请输入杯号！", "温馨提示", MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("Please enter the cup number！", "Tips", MessageBoxButtons.OK, false);
                        FADM_Object.Communal.WriteDripWait(false);
                        return;
                    }
                    else
                    {
                        if(SmartDyeing.FADM_Object.Communal._dic_dyeType.Keys.Contains(Convert.ToInt32(TxtNum.Text)))
                        {
                            if (SmartDyeing.FADM_Object.Communal._dic_dyeType[Convert.ToInt32(TxtNum.Text)] == 0)
                                return;
                        }
                        else
                        {
                            return;
                        }
                    }
                    FADM_Object.Communal.WriteMachineStatus(13);
                    Thread thread = new Thread(TestCoordinate);
                    thread.Start();
                }
                else if (RdoDecompression.Checked)
                {
                    if (string.IsNullOrEmpty(TxtNum.Text))
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("请输入杯号！", "温馨提示", MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("Please enter the cup number！", "Tips", MessageBoxButtons.OK, false);
                        FADM_Object.Communal.WriteDripWait(false);
                        return;
                    }
                    FADM_Object.Communal.WriteMachineStatus(13);
                    Thread thread = new Thread(TestCoordinate);
                    thread.Start();
                }
                else
                {
                    return;
                }

                Thread threadReset = new Thread(Reset);
                threadReset.Start();
            }
            else
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    FADM_Form.CustomMessageBox.Show("机台正在运动，不能开始", "定点移动", MessageBoxButtons.OK, true);
                else
                    FADM_Form.CustomMessageBox.Show("The machine is in motion and cannot start", "Tips", MessageBoxButtons.OK, true);
            }
        }
        //生成坐标
        private void button4_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TxtNum.Text))
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    FADM_Form.CustomMessageBox.Show("请输入杯号！", "温馨提示", MessageBoxButtons.OK, false);
                else
                    FADM_Form.CustomMessageBox.Show("Please enter the cup number！", "Tips", MessageBoxButtons.OK, false);
                FADM_Object.Communal.WriteDripWait(false);
                return;
            }
            int i_type = 0;
            int i_min = 0;
            //确认区域
            //判断是否翻转缸
            if(Convert.ToInt32(TxtNum.Text) == Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area1_CupMin.ToString()))
            {
                if (Lib_Card.Configure.Parameter.Machine_Area1_Type == 3)
                {
                    //if(Lib_Card.Configure.Parameter.Machine_Area1_DyeType == 0)
                    //{
                    //    return;
                    //}
                }
                else
                {
                    return;
                }
                i_min = Convert.ToInt32(TxtNum.Text);
                i_type = Lib_Card.Configure.Parameter.Machine_Area1_DyeType;
                
            }
            else if (Convert.ToInt32(TxtNum.Text) == Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area2_CupMin.ToString()))
            {
                if (Lib_Card.Configure.Parameter.Machine_Area2_Type == 3)
                {
                    //if (Lib_Card.Configure.Parameter.Machine_Area2_DyeType == 0)
                    //{
                    //    return;
                    //}
                }
                else
                {
                    return;
                }
                i_min = Convert.ToInt32(TxtNum.Text);
                i_type = Lib_Card.Configure.Parameter.Machine_Area2_DyeType;

            }
            else if (Convert.ToInt32(TxtNum.Text) == Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area3_CupMin.ToString()))
            {
                if (Lib_Card.Configure.Parameter.Machine_Area3_Type == 3)
                {
                    //if (Lib_Card.Configure.Parameter.Machine_Area3_DyeType == 0)
                    //{
                    //    return;
                    //}
                }
                else
                {
                    return;
                }
                i_min = Convert.ToInt32(TxtNum.Text);
                i_type = Lib_Card.Configure.Parameter.Machine_Area3_DyeType;

            }
            else if (Convert.ToInt32(TxtNum.Text) == Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area4_CupMin.ToString()))
            {
                if (Lib_Card.Configure.Parameter.Machine_Area4_Type == 3)
                {
                    //if (Lib_Card.Configure.Parameter.Machine_Area4_DyeType == 0)
                    //{
                    //    return;
                    //}
                }
                else
                {
                    return;
                }
                i_min = Convert.ToInt32(TxtNum.Text);
                i_type = Lib_Card.Configure.Parameter.Machine_Area4_DyeType;

            }
            else if (Convert.ToInt32(TxtNum.Text) == Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area5_CupMin.ToString()))
            {
                if (Lib_Card.Configure.Parameter.Machine_Area5_Type == 3)
                {
                    //if (Lib_Card.Configure.Parameter.Machine_Area5_DyeType == 0)
                    //{
                    //    return;
                    //}
                }
                else
                {
                    return;
                }
                i_min = Convert.ToInt32(TxtNum.Text);
                i_type = Lib_Card.Configure.Parameter.Machine_Area5_DyeType;

            }
            else if (Convert.ToInt32(TxtNum.Text) == Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area6_CupMin.ToString()))
            {
                if (Lib_Card.Configure.Parameter.Machine_Area6_Type == 3)
                {
                    //if (Lib_Card.Configure.Parameter.Machine_Area6_DyeType == 0)
                    //{
                    //    return;
                    //}
                }
                else
                {
                    return;
                }
                i_min = Convert.ToInt32(TxtNum.Text);
                i_type = Lib_Card.Configure.Parameter.Machine_Area6_DyeType;

            }
            if (RdoCup.Checked)
            {
                DialogResult dialogResult;
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    dialogResult = FADM_Form.CustomMessageBox.Show("确定生成该区域对应坐标吗?", "温馨提示", MessageBoxButtons.YesNo, true);
                }
                else
                {
                    dialogResult = FADM_Form.CustomMessageBox.Show("Determine the corresponding coordinates for generating the region?", "Tips", MessageBoxButtons.YesNo, true);
                }

                if (dialogResult == DialogResult.No)
                {
                    return;
                }
                else
                {
                    //6杯摇摆
                    if (i_type == 1)
                    {
                        //计算剩余坐标
                        int x = 0; int y = 0;
                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 8000;

                        WriteCupCoordinate(i_min + 1, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 8000 + 15650;

                        WriteCupCoordinate(i_min + 2, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 8000 + 15650 + 8000;

                        WriteCupCoordinate(i_min + 3, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 8000 + 15650 + 8000 + 15650;

                        WriteCupCoordinate(i_min + 4, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 8000 + 15650 + 8000 + 15650 + 8000;

                        WriteCupCoordinate(i_min + 5, x.ToString(), y.ToString());
                    }
                    //12杯摇摆
                    else if (i_type == 2)
                    {
                        //计算剩余坐标
                        int x = 0; int y = 0;
                        x = Convert.ToInt32(TxtRPosX.Text) + 10500;
                        y = Convert.ToInt32(TxtRPosY.Text);

                        WriteCupCoordinate(i_min + 1, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 8000;

                        WriteCupCoordinate(i_min + 2, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) + 10500;
                        y = Convert.ToInt32(TxtRPosY.Text) + 8000;

                        WriteCupCoordinate(i_min + 3, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 8000 + 15600;

                        WriteCupCoordinate(i_min + 4, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) + 10500;
                        y = Convert.ToInt32(TxtRPosY.Text) + 8000 + 15600;

                        WriteCupCoordinate(i_min + 5, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 8000 + 15600 + 8000;

                        WriteCupCoordinate(i_min + 6, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) + 10500;
                        y = Convert.ToInt32(TxtRPosY.Text) + 8000 + 15600 + 8000;

                        WriteCupCoordinate(i_min + 7, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 8000 + 15600 + 8000 + 15600;

                        WriteCupCoordinate(i_min + 8, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) + 10500;
                        y = Convert.ToInt32(TxtRPosY.Text) + 8000 + 15600 + 8000 + 15600;

                        WriteCupCoordinate(i_min + 9, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 8000 + 15600 + 8000 + 15600 + 8000;

                        WriteCupCoordinate(i_min + 10, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) + 10500;
                        y = Convert.ToInt32(TxtRPosY.Text) + 8000 + 15600 + 8000 + 15600 + 8000;

                        WriteCupCoordinate(i_min + 11, x.ToString(), y.ToString());
                    }

                    //4杯摇摆
                    else if (i_type == 3)
                    {
                        //计算剩余坐标
                        int x = 0; int y = 0;
                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 12600;
                        WriteCupCoordinate(i_min + 1, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 12600 + 22900;
                        WriteCupCoordinate(i_min + 2, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 12600 + 22900 + 12600;
                        WriteCupCoordinate(i_min + 3, x.ToString(), y.ToString());
                    }
                    //10杯摇摆
                    else if (i_type == 4)
                    {
                        //计算剩余坐标
                        int x = 0; int y = 0;
                        x = Convert.ToInt32(TxtRPosX.Text) + 11300;
                        y = Convert.ToInt32(TxtRPosY.Text);
                        WriteCupCoordinate(i_min + 1, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 11700;
                        WriteCupCoordinate(i_min + 2, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) + 11300;
                        y = Convert.ToInt32(TxtRPosY.Text) + 11700;
                        WriteCupCoordinate(i_min + 3, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 11700 + 9700;
                        WriteCupCoordinate(i_min + 4, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) + 11300;
                        y = Convert.ToInt32(TxtRPosY.Text) + 11700 + 9700;
                        WriteCupCoordinate(i_min + 5, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 11700 + 9700 + 17300;
                        WriteCupCoordinate(i_min + 6, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) + 11300;
                        y = Convert.ToInt32(TxtRPosY.Text) + 11700 + 9700 + 17300;
                        WriteCupCoordinate(i_min + 7, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 11700 + 9700 + 17300 + 9300;
                        WriteCupCoordinate(i_min + 6, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) + 11300;
                        y = Convert.ToInt32(TxtRPosY.Text) + 11700 + 9700 + 17300 + 9300;
                        WriteCupCoordinate(i_min + 7, x.ToString(), y.ToString());
                    }
                    //16杯摇摆
                    else if (i_type == 5)
                    {
                        //计算剩余坐标
                        int x = 0; int y = 0;
                        x = Convert.ToInt32(TxtRPosX.Text) + 10500;
                        y = Convert.ToInt32(TxtRPosY.Text);
                        WriteCupCoordinate(i_min + 1, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) ;
                        y = Convert.ToInt32(TxtRPosY.Text)+8000;
                        WriteCupCoordinate(i_min + 2, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text)+10500;
                        y = Convert.ToInt32(TxtRPosY.Text) + 8000;
                        WriteCupCoordinate(i_min + 3, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 8000+16350;
                        WriteCupCoordinate(i_min + 4, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) + 10500;
                        y = Convert.ToInt32(TxtRPosY.Text) + 8000+16350;
                        WriteCupCoordinate(i_min + 5, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 8000 + 16350+8000;
                        WriteCupCoordinate(i_min + 6, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) + 10500;
                        y = Convert.ToInt32(TxtRPosY.Text) + 8000 + 16350+8000;
                        WriteCupCoordinate(i_min + 7, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 8000 + 16350 + 8000+16350;
                        WriteCupCoordinate(i_min + 8, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) + 10500;
                        y = Convert.ToInt32(TxtRPosY.Text) + 8000 + 16350 + 8000+16350;
                        WriteCupCoordinate(i_min + 9, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 8000 + 16350 + 8000 + 16350+8000;
                        WriteCupCoordinate(i_min + 10, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) + 10500;
                        y = Convert.ToInt32(TxtRPosY.Text) + 8000 + 16350 + 8000 + 16350+8000;
                        WriteCupCoordinate(i_min + 11, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 8000 + 16350 + 8000 + 16350 + 8000+16350;
                        WriteCupCoordinate(i_min + 12, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) + 10500;
                        y = Convert.ToInt32(TxtRPosY.Text) + 8000 + 16350 + 8000 + 16350 + 8000+16350;
                        WriteCupCoordinate(i_min + 13, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 8000 + 16350 + 8000 + 16350 + 8000 + 16350+8000;
                        WriteCupCoordinate(i_min + 14, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) + 10500;
                        y = Convert.ToInt32(TxtRPosY.Text) + 8000 + 16350 + 8000 + 16350 + 8000 + 16350+8000;
                        WriteCupCoordinate(i_min + 15, x.ToString(), y.ToString());
                    }
                    //10杯转子缸
                    else if (i_type == 0)
                    {
                        //计算剩余坐标
                        int x = 0; int y = 0;
                        x = Convert.ToInt32(TxtRPosX.Text) ;
                        y = Convert.ToInt32(TxtRPosY.Text)+12000;

                        WriteCupCoordinate(i_min + 1, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 24000;

                        WriteCupCoordinate(i_min + 2, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) ;
                        y = Convert.ToInt32(TxtRPosY.Text) + 36000;

                        WriteCupCoordinate(i_min + 3, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 48000;

                        WriteCupCoordinate(i_min + 4, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 11000;
                        y = Convert.ToInt32(TxtRPosY.Text) ;

                        WriteCupCoordinate(i_min + 5, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 11000;
                        y = Convert.ToInt32(TxtRPosY.Text) + 12000;

                        WriteCupCoordinate(i_min + 6, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 11000;
                        y = Convert.ToInt32(TxtRPosY.Text) + 24000;

                        WriteCupCoordinate(i_min + 7, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 11000;
                        y = Convert.ToInt32(TxtRPosY.Text) + 36000;

                        WriteCupCoordinate(i_min + 8, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 11000;
                        y = Convert.ToInt32(TxtRPosY.Text) + 48000;

                        WriteCupCoordinate(i_min + 9, x.ToString(), y.ToString());
                    }
                }
            }
            else if (RdoDecompression.Checked)
            {
                DialogResult dialogResult;
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    dialogResult = FADM_Form.CustomMessageBox.Show("确定生成该区域对应坐标吗?", "温馨提示", MessageBoxButtons.YesNo, true);
                }
                else
                {
                    dialogResult = FADM_Form.CustomMessageBox.Show("Determine the corresponding coordinates for generating the region?", "Tips", MessageBoxButtons.YesNo, true);
                }

                if (dialogResult == DialogResult.No)
                {
                    return;
                }
                else
                {
                    //6杯摇摆
                    if (i_type == 1)
                    {
                        //计算剩余坐标
                        int x = 0; int y = 0;
                        x = Convert.ToInt32(TxtRPosX.Text) - 10200;
                        y = Convert.ToInt32(TxtRPosY.Text);

                        WriteCupCoverCoordinate(i_min + 1, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 23975;

                        WriteCupCoordinate(i_min + 2, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 10200;
                        y = Convert.ToInt32(TxtRPosY.Text) + 23975;

                        WriteCupCoordinate(i_min + 3, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 23975 + 23650;

                        WriteCupCoordinate(i_min + 4, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 10200;
                        y = Convert.ToInt32(TxtRPosY.Text) + 23975 + 23650;

                        WriteCupCoordinate(i_min + 5, x.ToString(), y.ToString());
                    }
                    //12杯摇摆
                    else if (i_type == 2)
                    {
                        //计算剩余坐标
                        int x = 0; int y = 0;
                        x = Convert.ToInt32(TxtRPosX.Text) - 10200;
                        y = Convert.ToInt32(TxtRPosY.Text);

                        WriteCupCoverCoordinate(i_min + 1, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 10200 - 10200;
                        y = Convert.ToInt32(TxtRPosY.Text);

                        WriteCupCoverCoordinate(i_min + 2, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 6500;

                        WriteCupCoverCoordinate(i_min + 3, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 10200;
                        y = Convert.ToInt32(TxtRPosY.Text) + 6500;

                        WriteCupCoverCoordinate(i_min + 4, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 10200 - 10200;
                        y = Convert.ToInt32(TxtRPosY.Text) + 6500;

                        WriteCupCoverCoordinate(i_min + 5, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 6500 + 23400;

                        WriteCupCoverCoordinate(i_min + 6, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 10200;
                        y = Convert.ToInt32(TxtRPosY.Text) + 6500 + 23400;

                        WriteCupCoverCoordinate(i_min + 7, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 10200 - 10200;
                        y = Convert.ToInt32(TxtRPosY.Text) + 6500 + 23400;

                        WriteCupCoverCoordinate(i_min + 8, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 6500 + 23400 + 23650;

                        WriteCupCoverCoordinate(i_min + 9, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 10200;
                        y = Convert.ToInt32(TxtRPosY.Text) + 6500 + 23400 + 23650;

                        WriteCupCoverCoordinate(i_min + 10, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 10200 - 10200;
                        y = Convert.ToInt32(TxtRPosY.Text) + 6500 + 23400 + 23650;

                        WriteCupCoverCoordinate(i_min + 11, x.ToString(), y.ToString());
                    }
                    //4杯摇摆
                    else if (i_type == 3)
                    {
                        //计算剩余坐标
                        int x = 0; int y = 0;
                        x = Convert.ToInt32(TxtRPosX.Text) - 10500;
                        y = Convert.ToInt32(TxtRPosY.Text);

                        WriteCupCoverCoordinate(i_min + 1, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 35750;

                        WriteCupCoverCoordinate(i_min + 2, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 10500;
                        y = Convert.ToInt32(TxtRPosY.Text) + 35750;

                        WriteCupCoverCoordinate(i_min + 3, x.ToString(), y.ToString());
                    }
                    //10杯摇摆
                    else if (i_type == 4)
                    {
                        //计算剩余坐标
                        int x = 0; int y = 0;
                        x = Convert.ToInt32(TxtRPosX.Text) - 10500;
                        y = Convert.ToInt32(TxtRPosY.Text);

                        WriteCupCoverCoordinate(i_min + 1, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text);
                        y = Convert.ToInt32(TxtRPosY.Text) + 7500;

                        WriteCupCoverCoordinate(i_min + 2, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 10500;
                        y = Convert.ToInt32(TxtRPosY.Text) + 7500;

                        WriteCupCoverCoordinate(i_min + 3, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) + 2900;
                        y = Convert.ToInt32(TxtRPosY.Text) + 7500 + 17500;

                        WriteCupCoverCoordinate(i_min + 4, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) + 2900 - 10200;
                        y = Convert.ToInt32(TxtRPosY.Text) + 7500 + 17500;

                        WriteCupCoverCoordinate(i_min + 5, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) + 2900 - 10200 - 10200;
                        y = Convert.ToInt32(TxtRPosY.Text) + 7500 + 17500;

                        WriteCupCoverCoordinate(i_min + 6, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) + 2900;
                        y = Convert.ToInt32(TxtRPosY.Text) + 7500 + 17500 + 27000;

                        WriteCupCoverCoordinate(i_min + 7, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) + 2900 - 10200;
                        y = Convert.ToInt32(TxtRPosY.Text) + 7500 + 17500 + 27000;

                        WriteCupCoverCoordinate(i_min + 8, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) + 2900 - 10200 - 10200;
                        y = Convert.ToInt32(TxtRPosY.Text) + 7500 + 17500 + 27000;

                        WriteCupCoverCoordinate(i_min + 9, x.ToString(), y.ToString());
                    }
                    //16杯摇摆
                    else if (i_type == 5)
                    {
                        //计算剩余坐标
                        int x = 0; int y = 0;
                        x = Convert.ToInt32(TxtRPosX.Text) - 10200;
                        y = Convert.ToInt32(TxtRPosY.Text);

                        WriteCupCoverCoordinate(i_min + 1, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 10200 - 10200;
                        y = Convert.ToInt32(TxtRPosY.Text);

                        WriteCupCoverCoordinate(i_min + 2, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 10200 - 10200 - 10200;
                        y = Convert.ToInt32(TxtRPosY.Text);

                        WriteCupCoverCoordinate(i_min + 3, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 10200;
                        y = Convert.ToInt32(TxtRPosY.Text) + 24350;

                        WriteCupCoverCoordinate(i_min + 4, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 10200 - 10200;
                        y = Convert.ToInt32(TxtRPosY.Text) + 24350;

                        WriteCupCoverCoordinate(i_min + 5, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 10200 - 10200 - 10200;
                        y = Convert.ToInt32(TxtRPosY.Text) + 24350;

                        WriteCupCoverCoordinate(i_min + 6, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 10200;
                        y = Convert.ToInt32(TxtRPosY.Text) + 24350 + 24350;

                        WriteCupCoverCoordinate(i_min + 7, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 10200 - 10200;
                        y = Convert.ToInt32(TxtRPosY.Text) + 24350 + 24350;

                        WriteCupCoverCoordinate(i_min + 8, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 10200 - 10200 - 10200;
                        y = Convert.ToInt32(TxtRPosY.Text) + 24350 + 24350;

                        WriteCupCoverCoordinate(i_min + 9, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 10200;
                        y = Convert.ToInt32(TxtRPosY.Text) + 24350 + 24350 + 24350;

                        WriteCupCoverCoordinate(i_min + 10, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 10200 - 10200;
                        y = Convert.ToInt32(TxtRPosY.Text) + 24350 + 24350 + 24350;

                        WriteCupCoverCoordinate(i_min + 11, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 10200 - 10200 - 10200;
                        y = Convert.ToInt32(TxtRPosY.Text) + 24350 + 24350 + 24350;

                        WriteCupCoverCoordinate(i_min + 12, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 10200;
                        y = Convert.ToInt32(TxtRPosY.Text) + 24350 + 24350 + 24350 + 24350;

                        WriteCupCoverCoordinate(i_min + 13, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 10200 - 10200;
                        y = Convert.ToInt32(TxtRPosY.Text) + 24350 + 24350 + 24350 + 24350;

                        WriteCupCoverCoordinate(i_min + 14, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 10200 - 10200 - 10200;
                        y = Convert.ToInt32(TxtRPosY.Text) + 24350 + 24350 + 24350 + 24350;

                        WriteCupCoverCoordinate(i_min + 15, x.ToString(), y.ToString());
                    }
                    //10杯转子缸挡水板位置
                    else if (i_type == 0)
                    {
                        //计算剩余坐标
                        int x = 0; int y = 0;
                        x = Convert.ToInt32(TxtRPosX.Text) - 7000;
                        y = Convert.ToInt32(TxtRPosY.Text);

                        WriteCupCoverCoordinate(i_min + 1, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 7000 - 7000;
                        y = Convert.ToInt32(TxtRPosY.Text);

                        WriteCupCoverCoordinate(i_min + 2, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 7000 - 7000 - 7000;
                        y = Convert.ToInt32(TxtRPosY.Text);

                        WriteCupCoverCoordinate(i_min + 3, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 7000-7000-7000-7000;
                        y = Convert.ToInt32(TxtRPosY.Text) ;

                        WriteCupCoverCoordinate(i_min + 4, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) ;
                        y = Convert.ToInt32(TxtRPosY.Text) + 7000;

                        WriteCupCoverCoordinate(i_min + 5, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 7000 ;
                        y = Convert.ToInt32(TxtRPosY.Text) + 7000;

                        WriteCupCoverCoordinate(i_min + 6, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 7000-7000;
                        y = Convert.ToInt32(TxtRPosY.Text) + 7000;

                        WriteCupCoverCoordinate(i_min + 7, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 7000 - 7000-7000;
                        y = Convert.ToInt32(TxtRPosY.Text) + 7000;

                        WriteCupCoverCoordinate(i_min + 8, x.ToString(), y.ToString());

                        x = Convert.ToInt32(TxtRPosX.Text) - 7000 - 7000 - 7000-7000;
                        y = Convert.ToInt32(TxtRPosY.Text) + 7000;

                        WriteCupCoverCoordinate(i_min + 9, x.ToString(), y.ToString());
                    }
                }
            }
        }
        //写入坐标
        private void button3_Click_1(object sender, EventArgs e)
        {
            if (RdoBottle.Checked)
            {
                if (string.IsNullOrEmpty(TxtNum.Text))
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show("请输入瓶号！", "温馨提示", MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("Please enter the bottle number！", "Tips", MessageBoxButtons.OK, false);
                    FADM_Object.Communal.WriteDripWait(false);
                    return;
                }
                else
                {
                    //母液瓶首瓶才能写入
                    if(Convert.ToInt32(TxtNum.Text) == 1)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        {
                            DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("确定写入?", "温馨提示", MessageBoxButtons.YesNo, true);

                            if (dialogResult == DialogResult.No)
                            {
                                return;
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(TxtRPosX.Text) || string.IsNullOrEmpty(TxtRPosY.Text))
                                {
                                    return;
                                }

                                //读取保存上次坐标到历史
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Bottle_X", Lib_Card.Configure.Parameter.Coordinate_Bottle_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Bottle_Y", Lib_Card.Configure.Parameter.Coordinate_Bottle_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Bottle_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Bottle_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_Bottle_X = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_Bottle_Y = Convert.ToInt32(TxtRPosY.Text);
                            }
                        }
                        else
                        {
                            DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("Definite write?", "Tips", MessageBoxButtons.YesNo, true);

                            if (dialogResult == DialogResult.No)
                            {
                                return;
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(TxtRPosX.Text) || string.IsNullOrEmpty(TxtRPosY.Text))
                                {
                                    return;
                                }

                                //读取保存上次坐标到历史
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Bottle_X", Lib_Card.Configure.Parameter.Coordinate_Bottle_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Bottle_Y", Lib_Card.Configure.Parameter.Coordinate_Bottle_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Bottle_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Bottle_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_Bottle_X = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_Bottle_Y = Convert.ToInt32(TxtRPosY.Text);
                            }
                        }
                    }
                }

            }
            else if (RdoCup.Checked)
            {
                if (string.IsNullOrEmpty(TxtNum.Text))
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show("请输入杯号！", "温馨提示", MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("Please enter the cup number！", "Tips", MessageBoxButtons.OK, false);
                    FADM_Object.Communal.WriteDripWait(false);
                    return;
                }
                else
                {
                    if(Convert.ToInt32(TxtNum.Text) > Lib_Card.Configure.Parameter.Machine_Cup_Total)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("杯号输入错误！", "温馨提示", MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("The cup number is incorrect！", "Tips", MessageBoxButtons.OK, false);
                        return;
                    }
                }
                DialogResult dialogResult;
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    dialogResult = FADM_Form.CustomMessageBox.Show("确定写入?", "温馨提示", MessageBoxButtons.YesNo, true);
                }
                else
                {
                    dialogResult = FADM_Form.CustomMessageBox.Show("Definite write?", "Tips", MessageBoxButtons.YesNo, true);
                }

                if (dialogResult == DialogResult.No)
                {
                    return;
                }
                else
                {
                    //判断是否翻转缸
                    if (string.IsNullOrEmpty(TxtRPosX.Text) || string.IsNullOrEmpty(TxtRPosY.Text))
                    {
                        return;
                    }
                    if (Convert.ToInt32(TxtNum.Text) >= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area1_CupMin.ToString()) && Convert.ToInt32(TxtNum.Text) <= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area1_CupMax.ToString()))
                    {
                        //如果是滴液区，判断是否首杯，如果是才能保存
                        if (Lib_Card.Configure.Parameter.Machine_Area1_Type == 2)
                        {
                            if (Convert.ToInt32(TxtNum.Text) != Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area1_CupMin.ToString()))
                            {
                                return;
                            }

                            //读取当前坐标写入
                            Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area1_X", Lib_Card.Configure.Parameter.Coordinate_Area1_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                            Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area1_Y", Lib_Card.Configure.Parameter.Coordinate_Area1_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                            //读取当前坐标写入
                            Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area1_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                            Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area1_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                            Lib_Card.Configure.Parameter.Coordinate_Area1_X = Convert.ToInt32(TxtRPosX.Text);
                            Lib_Card.Configure.Parameter.Coordinate_Area1_Y = Convert.ToInt32(TxtRPosY.Text);
                        }
                        else if (Lib_Card.Configure.Parameter.Machine_Area1_Type == 3)
                        {
                            ////如果是转子机，判断是否首杯，如果是才能保存
                            if (Lib_Card.Configure.Parameter.Machine_Area1_DyeType == 6|| Lib_Card.Configure.Parameter.Machine_Area1_DyeType == 7)
                            {
                                if (Convert.ToInt32(TxtNum.Text) != Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area1_CupMin.ToString()))
                                {
                                    return;
                                }
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area1_X", Lib_Card.Configure.Parameter.Coordinate_Area1_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area1_Y", Lib_Card.Configure.Parameter.Coordinate_Area1_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area1_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area1_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_Area1_X = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_Area1_Y = Convert.ToInt32(TxtRPosY.Text);
                            }
                            //翻转缸
                            else
                            {
                                WriteCupCoordinate(Convert.ToInt32(TxtNum.Text), TxtRPosX.Text, TxtRPosY.Text);
                            }

                        }
                    }

                    else if (Convert.ToInt32(TxtNum.Text) >= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area2_CupMin.ToString()) && Convert.ToInt32(TxtNum.Text) <= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area2_CupMax.ToString()))
                    {
                        //如果是滴液区，判断是否首杯，如果是才能保存
                        if (Lib_Card.Configure.Parameter.Machine_Area2_Type == 2)
                        {
                            if (Convert.ToInt32(TxtNum.Text) != Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area2_CupMin.ToString()))
                            {
                                return;
                            }
                            Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area2_X", Lib_Card.Configure.Parameter.Coordinate_Area2_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                            Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area2_Y", Lib_Card.Configure.Parameter.Coordinate_Area2_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                            //读取当前坐标写入
                            Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area2_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                            Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area2_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                            Lib_Card.Configure.Parameter.Coordinate_Area2_X = Convert.ToInt32(TxtRPosX.Text);
                            Lib_Card.Configure.Parameter.Coordinate_Area2_Y = Convert.ToInt32(TxtRPosY.Text);
                        }
                        else if (Lib_Card.Configure.Parameter.Machine_Area2_Type == 3)
                        {
                            //如果是转子机，判断是否首杯，如果是才能保存
                            if (Lib_Card.Configure.Parameter.Machine_Area2_DyeType == 6|| Lib_Card.Configure.Parameter.Machine_Area2_DyeType == 7)
                            {
                                if (Convert.ToInt32(TxtNum.Text) != Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area2_CupMin.ToString()))
                                {
                                    return;
                                }
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area2_X", Lib_Card.Configure.Parameter.Coordinate_Area2_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area2_Y", Lib_Card.Configure.Parameter.Coordinate_Area2_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area2_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area2_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_Area2_X = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_Area2_Y = Convert.ToInt32(TxtRPosY.Text);
                            }
                            //翻转缸
                            else
                            {
                                WriteCupCoordinate(Convert.ToInt32(TxtNum.Text), TxtRPosX.Text, TxtRPosY.Text);
                            }

                        }
                    }

                    else if (Convert.ToInt32(TxtNum.Text) >= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area3_CupMin.ToString()) && Convert.ToInt32(TxtNum.Text) <= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area3_CupMax.ToString()))
                    {
                        //如果是滴液区，判断是否首杯，如果是才能保存
                        if (Lib_Card.Configure.Parameter.Machine_Area3_Type == 2)
                        {
                            if (Convert.ToInt32(TxtNum.Text) != Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area3_CupMin.ToString()))
                            {
                                return;
                            }
                            Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area3_X", Lib_Card.Configure.Parameter.Coordinate_Area3_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                            Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area3_Y", Lib_Card.Configure.Parameter.Coordinate_Area3_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                            //读取当前坐标写入
                            Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area3_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                            Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area3_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                            Lib_Card.Configure.Parameter.Coordinate_Area3_X = Convert.ToInt32(TxtRPosX.Text);
                            Lib_Card.Configure.Parameter.Coordinate_Area3_Y = Convert.ToInt32(TxtRPosY.Text);
                        }
                        else if (Lib_Card.Configure.Parameter.Machine_Area3_Type == 3)
                        {
                            //如果是转子机，判断是否首杯，如果是才能保存
                            if (Lib_Card.Configure.Parameter.Machine_Area3_DyeType == 6|| Lib_Card.Configure.Parameter.Machine_Area3_DyeType == 7)
                            {
                                if (Convert.ToInt32(TxtNum.Text) != Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area3_CupMin.ToString()))
                                {
                                    return;
                                }
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area3_X", Lib_Card.Configure.Parameter.Coordinate_Area3_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area3_Y", Lib_Card.Configure.Parameter.Coordinate_Area3_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area3_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area3_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_Area3_X = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_Area3_Y = Convert.ToInt32(TxtRPosY.Text);
                            }
                            //翻转缸
                            else
                            {
                                WriteCupCoordinate(Convert.ToInt32(TxtNum.Text), TxtRPosX.Text, TxtRPosY.Text);
                            }

                        }
                    }

                    else if (Convert.ToInt32(TxtNum.Text) >= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area4_CupMin.ToString()) && Convert.ToInt32(TxtNum.Text) <= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area4_CupMax.ToString()))
                    {
                        //如果是滴液区，判断是否首杯，如果是才能保存
                        if (Lib_Card.Configure.Parameter.Machine_Area4_Type == 2)
                        {
                            if (Convert.ToInt32(TxtNum.Text) != Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area4_CupMin.ToString()))
                            {
                                return;
                            }
                            Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area4_X", Lib_Card.Configure.Parameter.Coordinate_Area4_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                            Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area4_Y", Lib_Card.Configure.Parameter.Coordinate_Area4_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                            //读取当前坐标写入
                            Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area4_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                            Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area4_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                            Lib_Card.Configure.Parameter.Coordinate_Area4_X = Convert.ToInt32(TxtRPosX.Text);
                            Lib_Card.Configure.Parameter.Coordinate_Area4_Y = Convert.ToInt32(TxtRPosY.Text);
                        }
                        else if (Lib_Card.Configure.Parameter.Machine_Area4_Type == 3)
                        {
                            //如果是转子机，判断是否首杯，如果是才能保存
                            if (Lib_Card.Configure.Parameter.Machine_Area4_DyeType == 6|| Lib_Card.Configure.Parameter.Machine_Area4_DyeType == 7)
                            {
                                if (Convert.ToInt32(TxtNum.Text) != Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area4_CupMin.ToString()))
                                {
                                    return;
                                }
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area4_X", Lib_Card.Configure.Parameter.Coordinate_Area4_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area4_Y", Lib_Card.Configure.Parameter.Coordinate_Area4_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area4_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area4_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_Area4_X = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_Area4_Y = Convert.ToInt32(TxtRPosY.Text);
                            }
                            //翻转缸
                            else
                            {
                                WriteCupCoordinate(Convert.ToInt32(TxtNum.Text), TxtRPosX.Text, TxtRPosY.Text);
                            }

                        }
                    }

                    else if (Convert.ToInt32(TxtNum.Text) >= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area5_CupMin.ToString()) && Convert.ToInt32(TxtNum.Text) <= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area5_CupMax.ToString()))
                    {
                        //如果是滴液区，判断是否首杯，如果是才能保存
                        if (Lib_Card.Configure.Parameter.Machine_Area5_Type == 2)
                        {
                            if (Convert.ToInt32(TxtNum.Text) != Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area5_CupMin.ToString()))
                            {
                                return;
                            }
                            Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area5_X", Lib_Card.Configure.Parameter.Coordinate_Area5_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                            Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area5_Y", Lib_Card.Configure.Parameter.Coordinate_Area5_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                            //读取当前坐标写入
                            Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area5_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                            Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area5_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                            Lib_Card.Configure.Parameter.Coordinate_Area5_X = Convert.ToInt32(TxtRPosX.Text);
                            Lib_Card.Configure.Parameter.Coordinate_Area5_Y = Convert.ToInt32(TxtRPosY.Text);
                        }
                        else if (Lib_Card.Configure.Parameter.Machine_Area5_Type == 3)
                        {
                            //如果是转子机，判断是否首杯，如果是才能保存
                            if (Lib_Card.Configure.Parameter.Machine_Area5_DyeType == 6|| Lib_Card.Configure.Parameter.Machine_Area5_DyeType == 7)
                            {
                                if (Convert.ToInt32(TxtNum.Text) != Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area5_CupMin.ToString()))
                                {
                                    return;
                                }
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area5_X", Lib_Card.Configure.Parameter.Coordinate_Area5_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area5_Y", Lib_Card.Configure.Parameter.Coordinate_Area5_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area5_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area5_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_Area5_X = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_Area5_Y = Convert.ToInt32(TxtRPosY.Text);
                            }
                            //翻转缸
                            else
                            {
                                WriteCupCoordinate(Convert.ToInt32(TxtNum.Text), TxtRPosX.Text, TxtRPosY.Text);
                            }

                        }
                    }

                    else if (Convert.ToInt32(TxtNum.Text) >= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area6_CupMin.ToString()) && Convert.ToInt32(TxtNum.Text) <= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area6_CupMax.ToString()))
                    {
                        //如果是滴液区，判断是否首杯，如果是才能保存
                        if (Lib_Card.Configure.Parameter.Machine_Area6_Type == 2)
                        {
                            if (Convert.ToInt32(TxtNum.Text) != Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area6_CupMin.ToString()))
                            {
                                return;
                            }
                            Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area6_X", Lib_Card.Configure.Parameter.Coordinate_Area6_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                            Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area6_Y", Lib_Card.Configure.Parameter.Coordinate_Area6_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                            //读取当前坐标写入
                            Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area6_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                            Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area6_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                            Lib_Card.Configure.Parameter.Coordinate_Area6_X = Convert.ToInt32(TxtRPosX.Text);
                            Lib_Card.Configure.Parameter.Coordinate_Area6_Y = Convert.ToInt32(TxtRPosY.Text);
                        }
                        else if (Lib_Card.Configure.Parameter.Machine_Area6_Type == 3)
                        {
                            //如果是转子机，判断是否首杯，如果是才能保存
                            if (Lib_Card.Configure.Parameter.Machine_Area6_DyeType == 6|| Lib_Card.Configure.Parameter.Machine_Area6_DyeType == 7)
                            {
                                if (Convert.ToInt32(TxtNum.Text) != Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Area6_CupMin.ToString()))
                                {
                                    return;
                                }
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area6_X", Lib_Card.Configure.Parameter.Coordinate_Area6_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area6_Y", Lib_Card.Configure.Parameter.Coordinate_Area6_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area6_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Area6_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_Area6_X = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_Area6_Y = Convert.ToInt32(TxtRPosY.Text);
                            }
                            //翻转缸
                            else
                            {
                                WriteCupCoordinate(Convert.ToInt32(TxtNum.Text), TxtRPosX.Text, TxtRPosY.Text);
                            }

                        }
                    }
                    else
                    {
                        return;
                    }
                }


            }
            else if (RdoStress.Checked)
            {
                return;
            }
            else if (RdoBalance.Checked)
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("确定写入?", "温馨提示", MessageBoxButtons.YesNo, true);

                    if (dialogResult == DialogResult.No)
                    {
                        return;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(TxtRPosX.Text) || string.IsNullOrEmpty(TxtRPosY.Text))
                        {
                            return;
                        }

                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Balance_X", Lib_Card.Configure.Parameter.Coordinate_Balance_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Balance_Y", Lib_Card.Configure.Parameter.Coordinate_Balance_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                        //读取当前坐标写入
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Balance_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Balance_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_Card.Configure.Parameter.Coordinate_Balance_X = Convert.ToInt32(TxtRPosX.Text);
                        Lib_Card.Configure.Parameter.Coordinate_Balance_Y = Convert.ToInt32(TxtRPosY.Text);
                    }
                }
                else
                {
                    DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("Definite write?", "Tips", MessageBoxButtons.YesNo, true);

                    if (dialogResult == DialogResult.No)
                    {
                        return;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(TxtRPosX.Text) || string.IsNullOrEmpty(TxtRPosY.Text))
                        {
                            return;
                        }

                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Balance_X", Lib_Card.Configure.Parameter.Coordinate_Balance_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Balance_Y", Lib_Card.Configure.Parameter.Coordinate_Balance_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                        //读取当前坐标写入
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Balance_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Balance_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_Card.Configure.Parameter.Coordinate_Balance_X = Convert.ToInt32(TxtRPosX.Text);
                        Lib_Card.Configure.Parameter.Coordinate_Balance_Y = Convert.ToInt32(TxtRPosY.Text);
                    }
                }
            }
            else if (RdoDecompression.Checked)
            {
                if (string.IsNullOrEmpty(TxtNum.Text))
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show("请输入杯号！", "温馨提示", MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("Please enter the cup number！", "Tips", MessageBoxButtons.OK, false);
                    FADM_Object.Communal.WriteDripWait(false);
                    return;
                }
                else
                {
                    if (Convert.ToInt32(TxtNum.Text) > Lib_Card.Configure.Parameter.Machine_Cup_Total)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("杯号输入错误！", "温馨提示", MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("The cup number is incorrect！", "Tips", MessageBoxButtons.OK, false);
                        return;
                    }
                }

                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("确定写入?", "温馨提示", MessageBoxButtons.YesNo, true);

                    if (dialogResult == DialogResult.No)
                    {
                        return;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(TxtRPosX.Text) || string.IsNullOrEmpty(TxtRPosY.Text))
                        {
                            return;
                        }

                        WriteCupCoverCoordinate(Convert.ToInt32(TxtNum.Text), TxtRPosX.Text, TxtRPosY.Text);
                    }
                }
                else
                {
                    DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("Definite write?", "Tips", MessageBoxButtons.YesNo, true);

                    if (dialogResult == DialogResult.No)
                    {
                        return;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(TxtRPosX.Text) || string.IsNullOrEmpty(TxtRPosY.Text))
                        {
                            return;
                        }

                        WriteCupCoverCoordinate(Convert.ToInt32(TxtNum.Text), TxtRPosX.Text, TxtRPosY.Text);
                    }
                }

            }
            else if (RdoDryCloth.Checked)
            {
                if (string.IsNullOrEmpty(TxtNum.Text))
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show("请输入杯号！", "温馨提示", MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("Please enter the cup number！", "Tips", MessageBoxButtons.OK, false);
                    FADM_Object.Communal.WriteDripWait(false);
                    return;
                }

                DialogResult dialogResult;
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    dialogResult = FADM_Form.CustomMessageBox.Show("确定写入?", "温馨提示", MessageBoxButtons.YesNo, true);
                }
                else
                {
                    dialogResult = FADM_Form.CustomMessageBox.Show("Definite write?", "Tips", MessageBoxButtons.YesNo, true);
                }

                if (dialogResult == DialogResult.No)
                {
                    return;
                }
                else
                {
                    //判断是否翻转缸
                    if (string.IsNullOrEmpty(TxtRPosX.Text) || string.IsNullOrEmpty(TxtRPosY.Text))
                    {
                        return;
                    }
                    if (Convert.ToInt32(TxtNum.Text) >= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_AreaDryCloth1_CupMin.ToString()) && Convert.ToInt32(TxtNum.Text) <= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_AreaDryCloth1_CupMax.ToString()))
                    {

                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_AreaDryCloth1_X", Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth1_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_AreaDryCloth1_Y", Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth1_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                        //读取当前坐标写入
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_AreaDryCloth1_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                            Lib_File.Ini.WriteIni("Coordinate", "Coordinate_AreaDryCloth1_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                            Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth1_X = Convert.ToInt32(TxtRPosX.Text);
                            Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth1_Y = Convert.ToInt32(TxtRPosY.Text);
                    }

                    else if (Convert.ToInt32(TxtNum.Text) >= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_AreaDryCloth2_CupMin.ToString()) && Convert.ToInt32(TxtNum.Text) <= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_AreaDryCloth2_CupMax.ToString()))
                    {
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_AreaDryCloth2_X", Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth2_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_AreaDryCloth2_Y", Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth2_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                        //读取当前坐标写入
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_AreaDryCloth2_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_AreaDryCloth2_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth2_X = Convert.ToInt32(TxtRPosX.Text);
                        Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth2_Y = Convert.ToInt32(TxtRPosY.Text);
                    }
                    else if (Convert.ToInt32(TxtNum.Text) >= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_AreaDryCloth3_CupMin.ToString()) && Convert.ToInt32(TxtNum.Text) <= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_AreaDryCloth3_CupMax.ToString()))
                    {
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_AreaDryCloth3_X", Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth3_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_AreaDryCloth3_Y", Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth3_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                        //读取当前坐标写入
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_AreaDryCloth3_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_AreaDryCloth3_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth3_X = Convert.ToInt32(TxtRPosX.Text);
                        Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth3_Y = Convert.ToInt32(TxtRPosY.Text);
                    }
                    else
                    {
                        return;
                    }
                }

            }
            else if (RdoWetCloth.Checked)
            {
                if (string.IsNullOrEmpty(TxtNum.Text))
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show("请输入杯号！", "温馨提示", MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("Please enter the cup number！", "Tips", MessageBoxButtons.OK, false);
                    FADM_Object.Communal.WriteDripWait(false);
                    return;
                }
                DialogResult dialogResult;
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    dialogResult = FADM_Form.CustomMessageBox.Show("确定写入?", "温馨提示", MessageBoxButtons.YesNo, true);
                }
                else
                {
                    dialogResult = FADM_Form.CustomMessageBox.Show("Definite write?", "Tips", MessageBoxButtons.YesNo, true);
                }

                if (dialogResult == DialogResult.No)
                {
                    return;
                }
                else
                {
                    //判断是否翻转缸
                    if (string.IsNullOrEmpty(TxtRPosX.Text) || string.IsNullOrEmpty(TxtRPosY.Text))
                    {
                        return;
                    }
                    if (Convert.ToInt32(TxtNum.Text) >= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_AreaWetCloth1_CupMin.ToString()) && Convert.ToInt32(TxtNum.Text) <= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_AreaWetCloth1_CupMax.ToString()))
                    {
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_AreaWetCloth1_X", Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth1_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_AreaWetCloth1_Y", Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth1_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                        //读取当前坐标写入
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_AreaWetCloth1_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_AreaWetCloth1_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth1_X = Convert.ToInt32(TxtRPosX.Text);
                        Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth1_Y = Convert.ToInt32(TxtRPosY.Text);
                    }

                    else if (Convert.ToInt32(TxtNum.Text) >= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_AreaWetCloth2_CupMin.ToString()) && Convert.ToInt32(TxtNum.Text) <= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_AreaWetCloth2_CupMax.ToString()))
                    {
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_AreaWetCloth2_X", Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth2_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_AreaWetCloth2_Y", Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth2_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                        //读取当前坐标写入
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_AreaWetCloth2_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_AreaWetCloth2_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth2_X = Convert.ToInt32(TxtRPosX.Text);
                        Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth2_Y = Convert.ToInt32(TxtRPosY.Text);
                    }
                    else if (Convert.ToInt32(TxtNum.Text) >= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_AreaWetCloth3_CupMin.ToString()) && Convert.ToInt32(TxtNum.Text) <= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_AreaWetCloth3_CupMax.ToString()))
                    {
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_AreaWetCloth3_X", Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth3_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_AreaWetCloth3_Y", Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth3_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                        //读取当前坐标写入
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_AreaWetCloth3_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_AreaWetCloth3_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth3_X = Convert.ToInt32(TxtRPosX.Text);
                        Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth3_Y = Convert.ToInt32(TxtRPosY.Text);
                    }
                    else
                    {
                        return;
                    }
                }
            }
            else if (RdoDryClamp.Checked)
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("确定写入?", "温馨提示", MessageBoxButtons.YesNo, true);

                    if (dialogResult == DialogResult.No)
                    {
                        return;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(TxtRPosX.Text) || string.IsNullOrEmpty(TxtRPosY.Text))
                        {
                            return;
                        }

                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_DryClamp_X", Lib_Card.Configure.Parameter.Coordinate_DryClamp_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_DryClamp_Y", Lib_Card.Configure.Parameter.Coordinate_DryClamp_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                        //读取当前坐标写入
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_DryClamp_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_DryClamp_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_Card.Configure.Parameter.Coordinate_DryClamp_X = Convert.ToInt32(TxtRPosX.Text);
                        Lib_Card.Configure.Parameter.Coordinate_DryClamp_Y = Convert.ToInt32(TxtRPosY.Text);
                    }
                }
                else
                {
                    DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("Definite write?", "Tips", MessageBoxButtons.YesNo, true);

                    if (dialogResult == DialogResult.No)
                    {
                        return;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(TxtRPosX.Text) || string.IsNullOrEmpty(TxtRPosY.Text))
                        {
                            return;
                        }

                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_DryClamp_X", Lib_Card.Configure.Parameter.Coordinate_DryClamp_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_DryClamp_Y", Lib_Card.Configure.Parameter.Coordinate_DryClamp_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                        //读取当前坐标写入
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_DryClamp_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_DryClamp_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_Card.Configure.Parameter.Coordinate_DryClamp_X = Convert.ToInt32(TxtRPosX.Text);
                        Lib_Card.Configure.Parameter.Coordinate_DryClamp_Y = Convert.ToInt32(TxtRPosY.Text);
                    }
                }
            }

            else if (RdoWetClamp.Checked)
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("确定写入?", "温馨提示", MessageBoxButtons.YesNo, true);

                    if (dialogResult == DialogResult.No)
                    {
                        return;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(TxtRPosX.Text) || string.IsNullOrEmpty(TxtRPosY.Text))
                        {
                            return;
                        }

                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_WetClamp_X", Lib_Card.Configure.Parameter.Coordinate_WetClamp_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_WetClamp_Y", Lib_Card.Configure.Parameter.Coordinate_WetClamp_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                        //读取当前坐标写入
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_WetClamp_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_WetClamp_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_Card.Configure.Parameter.Coordinate_WetClamp_X = Convert.ToInt32(TxtRPosX.Text);
                        Lib_Card.Configure.Parameter.Coordinate_WetClamp_Y = Convert.ToInt32(TxtRPosY.Text);
                    }
                }
                else
                {
                    DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("Definite write?", "Tips", MessageBoxButtons.YesNo, true);

                    if (dialogResult == DialogResult.No)
                    {
                        return;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(TxtRPosX.Text) || string.IsNullOrEmpty(TxtRPosY.Text))
                        {
                            return;
                        }
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_WetClamp_X", Lib_Card.Configure.Parameter.Coordinate_WetClamp_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_WetClamp_Y", Lib_Card.Configure.Parameter.Coordinate_WetClamp_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                        //读取当前坐标写入
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_WetClamp_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_WetClamp_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_Card.Configure.Parameter.Coordinate_WetClamp_X = Convert.ToInt32(TxtRPosX.Text);
                        Lib_Card.Configure.Parameter.Coordinate_WetClamp_Y = Convert.ToInt32(TxtRPosY.Text);
                    }
                }
            }
            else if (RdoWash.Checked)
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("确定写入?", "温馨提示", MessageBoxButtons.YesNo, true);

                    if (dialogResult == DialogResult.No)
                    {
                        return;
                    }
                    else
                    {

                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Wash_X", Lib_Card.Configure.Parameter.Coordinate_Wash_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Wash_Y", Lib_Card.Configure.Parameter.Coordinate_Wash_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                        //读取当前坐标写入
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Wash_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Wash_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_Card.Configure.Parameter.Coordinate_Wash_X = Convert.ToInt32(TxtRPosX.Text);
                        Lib_Card.Configure.Parameter.Coordinate_Wash_Y = Convert.ToInt32(TxtRPosY.Text);
                    }
                }
                else
                {
                    DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("Definite write?", "Tips", MessageBoxButtons.YesNo, true);

                    if (dialogResult == DialogResult.No)
                    {
                        return;
                    }
                    else
                    {

                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Wash_X", Lib_Card.Configure.Parameter.Coordinate_Wash_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Wash_Y", Lib_Card.Configure.Parameter.Coordinate_Wash_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                        //读取当前坐标写入
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Wash_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Wash_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_Card.Configure.Parameter.Coordinate_Wash_X = Convert.ToInt32(TxtRPosX.Text);
                        Lib_Card.Configure.Parameter.Coordinate_Wash_Y = Convert.ToInt32(TxtRPosY.Text);
                    }
                }
            }
            else if (RdoStandBy.Checked)
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("确定写入?", "温馨提示", MessageBoxButtons.YesNo, true);

                    if (dialogResult == DialogResult.No)
                    {
                        return;
                    }
                    else
                    {

                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Standby_X", Lib_Card.Configure.Parameter.Coordinate_Standby_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Standby_Y", Lib_Card.Configure.Parameter.Coordinate_Standby_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                        //读取当前坐标写入
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Standby_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Standby_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_Card.Configure.Parameter.Coordinate_Standby_X = Convert.ToInt32(TxtRPosX.Text);
                        Lib_Card.Configure.Parameter.Coordinate_Standby_Y = Convert.ToInt32(TxtRPosY.Text);
                    }
                }
                else
                {
                    DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("Definite write?", "Tips", MessageBoxButtons.YesNo, true);

                    if (dialogResult == DialogResult.No)
                    {
                        return;
                    }
                    else
                    {

                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Standby_X", Lib_Card.Configure.Parameter.Coordinate_Standby_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Standby_Y", Lib_Card.Configure.Parameter.Coordinate_Standby_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                        //读取当前坐标写入
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Standby_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Standby_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_Card.Configure.Parameter.Coordinate_Standby_X = Convert.ToInt32(TxtRPosX.Text);
                        Lib_Card.Configure.Parameter.Coordinate_Standby_Y = Convert.ToInt32(TxtRPosY.Text);
                    }
                }
            }
            else if (RdoExtraction.Checked)
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("确定写入?", "温馨提示", MessageBoxButtons.YesNo, true);

                    if (dialogResult == DialogResult.No)
                    {
                        return;
                    }
                    else
                    {

                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Syringes_X", Lib_Card.Configure.Parameter.Coordinate_Syringes_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Syringes_Y", Lib_Card.Configure.Parameter.Coordinate_Syringes_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                        //读取当前坐标写入
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Syringes_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Syringes_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_Card.Configure.Parameter.Coordinate_Syringes_X = Convert.ToInt32(TxtRPosX.Text);
                        Lib_Card.Configure.Parameter.Coordinate_Syringes_Y = Convert.ToInt32(TxtRPosY.Text);
                    }
                }
                else
                {
                    DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("Definite write?", "Tips", MessageBoxButtons.YesNo, true);

                    if (dialogResult == DialogResult.No)
                    {
                        return;
                    }
                    else
                    {

                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Syringes_X", Lib_Card.Configure.Parameter.Coordinate_Syringes_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Syringes_Y", Lib_Card.Configure.Parameter.Coordinate_Syringes_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                        //读取当前坐标写入
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Syringes_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Syringes_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_Card.Configure.Parameter.Coordinate_Syringes_X = Convert.ToInt32(TxtRPosX.Text);
                        Lib_Card.Configure.Parameter.Coordinate_Syringes_Y = Convert.ToInt32(TxtRPosY.Text);
                    }
                }
            }
            else if (RdoAbs.Checked)
            {
                if (Convert.ToInt32(TxtNum.Text) >= 1 && Convert.ToInt32(TxtNum.Text) <= 4)
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    {
                        DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("确定写入?", "温馨提示", MessageBoxButtons.YesNo, true);

                        if (dialogResult == DialogResult.No)
                        {
                            return;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(TxtRPosX.Text) || string.IsNullOrEmpty(TxtRPosY.Text))
                            {
                                return;
                            }
                            if (Convert.ToInt32(TxtNum.Text) == 1)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs1_X", Lib_Card.Configure.Parameter.Coordinate_Abs1_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs1_Y", Lib_Card.Configure.Parameter.Coordinate_Abs1_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs1_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs1_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_Abs1_X = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_Abs1_Y = Convert.ToInt32(TxtRPosY.Text);
                            }
                            else if (Convert.ToInt32(TxtNum.Text) == 2)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs2_X", Lib_Card.Configure.Parameter.Coordinate_Abs2_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs2_Y", Lib_Card.Configure.Parameter.Coordinate_Abs2_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs2_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs2_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_Abs2_X = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_Abs2_Y = Convert.ToInt32(TxtRPosY.Text);
                            }
                            else if (Convert.ToInt32(TxtNum.Text) == 3)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs3_X", Lib_Card.Configure.Parameter.Coordinate_Abs3_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs3_Y", Lib_Card.Configure.Parameter.Coordinate_Abs3_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs3_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs3_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_Abs3_X = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_Abs3_Y = Convert.ToInt32(TxtRPosY.Text);
                            }
                            else if (Convert.ToInt32(TxtNum.Text) == 4)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs4_X", Lib_Card.Configure.Parameter.Coordinate_Abs4_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs4_Y", Lib_Card.Configure.Parameter.Coordinate_Abs4_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs4_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs4_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_Abs4_X = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_Abs4_Y = Convert.ToInt32(TxtRPosY.Text);
                            }
                        }
                    }
                    else
                    {
                        DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("Definite write?", "Tips", MessageBoxButtons.YesNo, true);

                        if (dialogResult == DialogResult.No)
                        {
                            return;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(TxtRPosX.Text) || string.IsNullOrEmpty(TxtRPosY.Text))
                            {
                                return;
                            }

                            if (Convert.ToInt32(TxtNum.Text) == 1)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs1_X", Lib_Card.Configure.Parameter.Coordinate_Abs1_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs1_Y", Lib_Card.Configure.Parameter.Coordinate_Abs1_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs1_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs1_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_Abs1_X = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_Abs1_Y = Convert.ToInt32(TxtRPosY.Text);
                            }
                            else if (Convert.ToInt32(TxtNum.Text) == 2)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs2_X", Lib_Card.Configure.Parameter.Coordinate_Abs2_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs2_Y", Lib_Card.Configure.Parameter.Coordinate_Abs2_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs2_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs2_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_Abs2_X = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_Abs2_Y = Convert.ToInt32(TxtRPosY.Text);
                            }
                            else if (Convert.ToInt32(TxtNum.Text) == 3)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs3_X", Lib_Card.Configure.Parameter.Coordinate_Abs3_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs3_Y", Lib_Card.Configure.Parameter.Coordinate_Abs3_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs3_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs3_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_Abs3_X = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_Abs3_Y = Convert.ToInt32(TxtRPosY.Text);
                            }
                            else if (Convert.ToInt32(TxtNum.Text) == 4)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs4_X", Lib_Card.Configure.Parameter.Coordinate_Abs4_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs4_Y", Lib_Card.Configure.Parameter.Coordinate_Abs4_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs4_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Abs4_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_Abs4_X = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_Abs4_Y = Convert.ToInt32(TxtRPosY.Text);
                            }
                        }
                    }
                }
            }
            else if (RdoBrew.Checked)
            {
                if (Convert.ToInt32(TxtNum.Text) >= 1 && Convert.ToInt32(TxtNum.Text) <= 6)
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    {
                        DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("确定写入?", "温馨提示", MessageBoxButtons.YesNo, true);

                        if (dialogResult == DialogResult.No)
                        {
                            return;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(TxtRPosX.Text) || string.IsNullOrEmpty(TxtRPosY.Text))
                            {
                                return;
                            }
                            if (Convert.ToInt32(TxtNum.Text) == 1)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew1_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Brew1_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew1_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Brew1_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew1_IntervalX", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew1_IntervalY", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_Brew1_IntervalX = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_Brew1_IntervalY = Convert.ToInt32(TxtRPosY.Text);
                            }
                            else if (Convert.ToInt32(TxtNum.Text) == 2)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew2_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Brew2_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew2_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Brew2_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew2_IntervalX", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew2_IntervalY", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_Brew2_IntervalX = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_Brew2_IntervalY = Convert.ToInt32(TxtRPosY.Text);
                            }
                            else if (Convert.ToInt32(TxtNum.Text) == 3)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew3_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Brew3_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew3_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Brew3_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew3_IntervalX", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew3_IntervalY", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_Brew3_IntervalX = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_Brew3_IntervalY = Convert.ToInt32(TxtRPosY.Text);
                            }
                            else if (Convert.ToInt32(TxtNum.Text) == 4)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew4_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Brew4_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew4_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Brew4_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew4_IntervalX", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew4_IntervalY", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_Brew4_IntervalX = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_Brew4_IntervalY = Convert.ToInt32(TxtRPosY.Text);
                            }
                            else if (Convert.ToInt32(TxtNum.Text) == 5)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew5_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Brew5_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew5_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Brew5_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew5_IntervalX", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew5_IntervalY", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_Brew5_IntervalX = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_Brew5_IntervalY = Convert.ToInt32(TxtRPosY.Text);
                            }
                            else if (Convert.ToInt32(TxtNum.Text) == 6)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew6_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Brew6_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew6_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Brew6_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew6_IntervalX", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew6_IntervalY", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_Brew6_IntervalX = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_Brew6_IntervalY = Convert.ToInt32(TxtRPosY.Text);
                            }
                        }
                    }
                    else
                    {
                        DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("Definite write?", "Tips", MessageBoxButtons.YesNo, true);

                        if (dialogResult == DialogResult.No)
                        {
                            return;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(TxtRPosX.Text) || string.IsNullOrEmpty(TxtRPosY.Text))
                            {
                                return;
                            }

                            if (Convert.ToInt32(TxtNum.Text) == 1)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew1_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Brew1_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew1_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Brew1_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew1_IntervalX", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew1_IntervalY", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_Brew1_IntervalX = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_Brew1_IntervalY = Convert.ToInt32(TxtRPosY.Text);
                            }
                            else if (Convert.ToInt32(TxtNum.Text) == 2)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew2_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Brew2_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew2_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Brew2_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew2_IntervalX", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew2_IntervalY", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_Brew2_IntervalX = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_Brew2_IntervalY = Convert.ToInt32(TxtRPosY.Text);
                            }
                            else if (Convert.ToInt32(TxtNum.Text) == 3)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew3_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Brew3_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew3_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Brew3_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew3_IntervalX", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew3_IntervalY", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_Brew3_IntervalX = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_Brew3_IntervalY = Convert.ToInt32(TxtRPosY.Text);
                            }
                            else if (Convert.ToInt32(TxtNum.Text) == 4)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew4_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Brew4_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew4_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Brew4_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew4_IntervalX", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew4_IntervalY", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_Brew4_IntervalX = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_Brew4_IntervalY = Convert.ToInt32(TxtRPosY.Text);
                            }
                            else if (Convert.ToInt32(TxtNum.Text) == 5)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew5_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Brew5_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew5_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Brew5_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew5_IntervalX", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew5_IntervalY", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_Brew5_IntervalX = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_Brew5_IntervalY = Convert.ToInt32(TxtRPosY.Text);
                            }
                            else if (Convert.ToInt32(TxtNum.Text) == 6)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew6_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Brew6_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew6_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Brew6_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew6_IntervalX", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Brew6_IntervalY", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_Brew6_IntervalX = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_Brew6_IntervalY = Convert.ToInt32(TxtRPosY.Text);
                            }
                        }
                    }
                }
            }
            else if (RdoSysFinish.Checked)
            {
                if (Convert.ToInt32(TxtNum.Text) >= 1 && Convert.ToInt32(TxtNum.Text) <= 6)
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    {
                        DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("确定写入?", "温馨提示", MessageBoxButtons.YesNo, true);

                        if (dialogResult == DialogResult.No)
                        {
                            return;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(TxtRPosX.Text) || string.IsNullOrEmpty(TxtRPosY.Text))
                            {
                                return;
                            }
                            if (Convert.ToInt32(TxtNum.Text) == 1)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes1_IntervalX", Lib_Card.Configure.Parameter.Coordinate_PutSyringes1_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes1_IntervalY", Lib_Card.Configure.Parameter.Coordinate_PutSyringes1_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes1_IntervalX", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes1_IntervalY", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_PutSyringes1_IntervalX = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_PutSyringes1_IntervalY = Convert.ToInt32(TxtRPosY.Text);
                            }
                            else if (Convert.ToInt32(TxtNum.Text) == 2)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes2_IntervalX", Lib_Card.Configure.Parameter.Coordinate_PutSyringes2_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes2_IntervalY", Lib_Card.Configure.Parameter.Coordinate_PutSyringes2_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes2_IntervalX", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes2_IntervalY", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_PutSyringes2_IntervalX = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_PutSyringes2_IntervalY = Convert.ToInt32(TxtRPosY.Text);
                            }
                            else if (Convert.ToInt32(TxtNum.Text) == 3)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes1_IntervalX", Lib_Card.Configure.Parameter.Coordinate_PutSyringes3_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes3_IntervalY", Lib_Card.Configure.Parameter.Coordinate_PutSyringes3_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes3_IntervalX", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes3_IntervalY", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_PutSyringes3_IntervalX = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_PutSyringes3_IntervalY = Convert.ToInt32(TxtRPosY.Text);
                            }
                            else if (Convert.ToInt32(TxtNum.Text) == 4)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes4_IntervalX", Lib_Card.Configure.Parameter.Coordinate_PutSyringes4_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes4_IntervalY", Lib_Card.Configure.Parameter.Coordinate_PutSyringes4_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes4_IntervalX", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes4_IntervalY", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_PutSyringes4_IntervalX = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_PutSyringes4_IntervalY = Convert.ToInt32(TxtRPosY.Text);
                            }
                            else if (Convert.ToInt32(TxtNum.Text) == 5)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes5_IntervalX", Lib_Card.Configure.Parameter.Coordinate_PutSyringes5_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes5_IntervalY", Lib_Card.Configure.Parameter.Coordinate_PutSyringes5_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes5_IntervalX", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes5_IntervalY", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_PutSyringes5_IntervalX = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_PutSyringes5_IntervalY = Convert.ToInt32(TxtRPosY.Text);
                            }
                            else if (Convert.ToInt32(TxtNum.Text) == 6)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes6_IntervalX", Lib_Card.Configure.Parameter.Coordinate_PutSyringes6_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes6_IntervalY", Lib_Card.Configure.Parameter.Coordinate_PutSyringes6_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes6_IntervalX", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes6_IntervalY", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_PutSyringes6_IntervalX = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_PutSyringes6_IntervalY = Convert.ToInt32(TxtRPosY.Text);
                            }
                        }
                    }
                    else
                    {
                        DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("Definite write?", "Tips", MessageBoxButtons.YesNo, true);

                        if (dialogResult == DialogResult.No)
                        {
                            return;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(TxtRPosX.Text) || string.IsNullOrEmpty(TxtRPosY.Text))
                            {
                                return;
                            }

                            if (Convert.ToInt32(TxtNum.Text) == 1)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes1_IntervalX", Lib_Card.Configure.Parameter.Coordinate_PutSyringes1_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes1_IntervalY", Lib_Card.Configure.Parameter.Coordinate_PutSyringes1_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes1_IntervalX", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes1_IntervalY", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_PutSyringes1_IntervalX = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_PutSyringes1_IntervalY = Convert.ToInt32(TxtRPosY.Text);
                            }
                            else if (Convert.ToInt32(TxtNum.Text) == 2)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes2_IntervalX", Lib_Card.Configure.Parameter.Coordinate_PutSyringes2_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes2_IntervalY", Lib_Card.Configure.Parameter.Coordinate_PutSyringes2_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes2_IntervalX", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes2_IntervalY", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_PutSyringes2_IntervalX = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_PutSyringes2_IntervalY = Convert.ToInt32(TxtRPosY.Text);
                            }
                            else if (Convert.ToInt32(TxtNum.Text) == 3)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes1_IntervalX", Lib_Card.Configure.Parameter.Coordinate_PutSyringes3_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes3_IntervalY", Lib_Card.Configure.Parameter.Coordinate_PutSyringes3_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes3_IntervalX", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes3_IntervalY", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_PutSyringes3_IntervalX = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_PutSyringes3_IntervalY = Convert.ToInt32(TxtRPosY.Text);
                            }
                            else if (Convert.ToInt32(TxtNum.Text) == 4)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes4_IntervalX", Lib_Card.Configure.Parameter.Coordinate_PutSyringes4_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes4_IntervalY", Lib_Card.Configure.Parameter.Coordinate_PutSyringes4_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes4_IntervalX", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes4_IntervalY", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_PutSyringes4_IntervalX = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_PutSyringes4_IntervalY = Convert.ToInt32(TxtRPosY.Text);
                            }
                            else if (Convert.ToInt32(TxtNum.Text) == 5)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes5_IntervalX", Lib_Card.Configure.Parameter.Coordinate_PutSyringes5_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes5_IntervalY", Lib_Card.Configure.Parameter.Coordinate_PutSyringes5_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes5_IntervalX", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes5_IntervalY", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_PutSyringes5_IntervalX = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_PutSyringes5_IntervalY = Convert.ToInt32(TxtRPosY.Text);
                            }
                            else if (Convert.ToInt32(TxtNum.Text) == 6)
                            {
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes6_IntervalX", Lib_Card.Configure.Parameter.Coordinate_PutSyringes6_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes6_IntervalY", Lib_Card.Configure.Parameter.Coordinate_PutSyringes6_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                                //读取当前坐标写入
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes6_IntervalX", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PutSyringes6_IntervalY", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                                Lib_Card.Configure.Parameter.Coordinate_PutSyringes6_IntervalX = Convert.ToInt32(TxtRPosX.Text);
                                Lib_Card.Configure.Parameter.Coordinate_PutSyringes6_IntervalY = Convert.ToInt32(TxtRPosY.Text);
                            }
                        }
                    }
                }
            }
            else if (RdoPHTest.Checked)
            {
                if (string.IsNullOrEmpty(TxtNum.Text))
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show("请输入杯号！", "温馨提示", MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("Please enter the cup number！", "Tips", MessageBoxButtons.OK, false);
                    FADM_Object.Communal.WriteDripWait(false);
                    return;
                }

                DialogResult dialogResult;
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    dialogResult = FADM_Form.CustomMessageBox.Show("确定写入?", "温馨提示", MessageBoxButtons.YesNo, true);
                }
                else
                {
                    dialogResult = FADM_Form.CustomMessageBox.Show("Definite write?", "Tips", MessageBoxButtons.YesNo, true);
                }

                if (dialogResult == DialogResult.No)
                {
                    return;
                }
                else
                {
                    //判断是否翻转缸
                    if (string.IsNullOrEmpty(TxtRPosX.Text) || string.IsNullOrEmpty(TxtRPosY.Text))
                    {
                        return;
                    }
                    if (Convert.ToInt32(TxtNum.Text) >= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_TestPH1_CupMin.ToString()) && Convert.ToInt32(TxtNum.Text) <= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_TestPH1_CupMax.ToString()))
                    {

                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_TestPH1_X", Lib_Card.Configure.Parameter.Coordinate_TestPH1_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_TestPH1_Y", Lib_Card.Configure.Parameter.Coordinate_TestPH1_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                        //读取当前坐标写入
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_TestPH1_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_TestPH1_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_Card.Configure.Parameter.Coordinate_TestPH1_X = Convert.ToInt32(TxtRPosX.Text);
                        Lib_Card.Configure.Parameter.Coordinate_TestPH1_Y = Convert.ToInt32(TxtRPosY.Text);
                    }

                    else if (Convert.ToInt32(TxtNum.Text) >= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_TestPH2_CupMin.ToString()) && Convert.ToInt32(TxtNum.Text) <= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_TestPH2_CupMax.ToString()))
                    {
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_TestPH2_X", Lib_Card.Configure.Parameter.Coordinate_TestPH2_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_TestPH2_Y", Lib_Card.Configure.Parameter.Coordinate_TestPH2_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                        //读取当前坐标写入
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_TestPH2_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_TestPH2_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_Card.Configure.Parameter.Coordinate_TestPH2_X = Convert.ToInt32(TxtRPosX.Text);
                        Lib_Card.Configure.Parameter.Coordinate_TestPH2_Y = Convert.ToInt32(TxtRPosY.Text);
                    }
                    else if (Convert.ToInt32(TxtNum.Text) >= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_TestPH3_CupMin.ToString()) && Convert.ToInt32(TxtNum.Text) <= Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_TestPH3_CupMax.ToString()))
                    {
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_TestPH3_X", Lib_Card.Configure.Parameter.Coordinate_TestPH3_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_TestPH3_Y", Lib_Card.Configure.Parameter.Coordinate_TestPH3_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                        //读取当前坐标写入
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_TestPH3_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_TestPH3_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_Card.Configure.Parameter.Coordinate_TestPH3_X = Convert.ToInt32(TxtRPosX.Text);
                        Lib_Card.Configure.Parameter.Coordinate_TestPH3_Y = Convert.ToInt32(TxtRPosY.Text);
                    }
                    else
                    {
                        return;
                    }
                }

            }
            else if (RdoPH.Checked)
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("确定写入?", "温馨提示", MessageBoxButtons.YesNo, true);

                    if (dialogResult == DialogResult.No)
                    {
                        return;
                    }
                    else
                    {

                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PH_IntervalX", Lib_Card.Configure.Parameter.Coordinate_PH_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PH_IntervalY", Lib_Card.Configure.Parameter.Coordinate_PH_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                        //读取当前坐标写入
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PH_IntervalX", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PH_IntervalY", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_Card.Configure.Parameter.Coordinate_PH_IntervalX = Convert.ToInt32(TxtRPosX.Text);
                        Lib_Card.Configure.Parameter.Coordinate_PH_IntervalY = Convert.ToInt32(TxtRPosY.Text);
                    }
                }
                else
                {
                    DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("Definite write?", "Tips", MessageBoxButtons.YesNo, true);

                    if (dialogResult == DialogResult.No)
                    {
                        return;
                    }
                    else
                    {

                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PH_IntervalX", Lib_Card.Configure.Parameter.Coordinate_PH_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PH_IntervalY", Lib_Card.Configure.Parameter.Coordinate_PH_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                        //读取当前坐标写入
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PH_IntervalX", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PH_IntervalY", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_Card.Configure.Parameter.Coordinate_PH_IntervalX = Convert.ToInt32(TxtRPosX.Text);
                        Lib_Card.Configure.Parameter.Coordinate_PH_IntervalY = Convert.ToInt32(TxtRPosY.Text);
                    }
                }
            }
            else if (RdoSalinity.Checked)
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("确定写入?", "温馨提示", MessageBoxButtons.YesNo, true);

                    if (dialogResult == DialogResult.No)
                    {
                        return;
                    }
                    else
                    {

                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Salinity_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Salinity_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Salinity_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Salinity_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                        //读取当前坐标写入
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Salinity_IntervalX", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Salinity_IntervalY", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_Card.Configure.Parameter.Coordinate_Salinity_IntervalX = Convert.ToInt32(TxtRPosX.Text);
                        Lib_Card.Configure.Parameter.Coordinate_Salinity_IntervalY = Convert.ToInt32(TxtRPosY.Text);
                    }
                }
                else
                {
                    DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("Definite write?", "Tips", MessageBoxButtons.YesNo, true);

                    if (dialogResult == DialogResult.No)
                    {
                        return;
                    }
                    else
                    {

                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Salinity_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Salinity_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Salinity_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Salinity_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                        //读取当前坐标写入
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Salinity_IntervalX", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Salinity_IntervalY", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_Card.Configure.Parameter.Coordinate_Salinity_IntervalX = Convert.ToInt32(TxtRPosX.Text);
                        Lib_Card.Configure.Parameter.Coordinate_Salinity_IntervalY = Convert.ToInt32(TxtRPosY.Text);
                    }
                }
            }
            else if (RdoPower.Checked)
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("确定写入?", "温馨提示", MessageBoxButtons.YesNo, true);

                    if (dialogResult == DialogResult.No)
                    {
                        return;
                    }
                    else
                    {

                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PowerClamp_X", Lib_Card.Configure.Parameter.Coordinate_PowerClamp_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PowerClamp_Y", Lib_Card.Configure.Parameter.Coordinate_PowerClamp_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                        //读取当前坐标写入
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PowerClamp_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PowerClamp_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_Card.Configure.Parameter.Coordinate_PowerClamp_X = Convert.ToInt32(TxtRPosX.Text);
                        Lib_Card.Configure.Parameter.Coordinate_PowerClamp_Y = Convert.ToInt32(TxtRPosY.Text);
                    }
                }
                else
                {
                    DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("Definite write?", "Tips", MessageBoxButtons.YesNo, true);

                    if (dialogResult == DialogResult.No)
                    {
                        return;
                    }
                    else
                    {

                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PowerClamp_X", Lib_Card.Configure.Parameter.Coordinate_PowerClamp_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PowerClamp_Y", Lib_Card.Configure.Parameter.Coordinate_PowerClamp_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                        //读取当前坐标写入
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PowerClamp_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_PowerClamp_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_Card.Configure.Parameter.Coordinate_PowerClamp_X = Convert.ToInt32(TxtRPosX.Text);
                        Lib_Card.Configure.Parameter.Coordinate_PowerClamp_Y = Convert.ToInt32(TxtRPosY.Text);
                    }
                }
            }
            else if (RdoWashBottle.Checked)
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("确定写入?", "温馨提示", MessageBoxButtons.YesNo, true);

                    if (dialogResult == DialogResult.No)
                    {
                        return;
                    }
                    else
                    {

                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_WashBottle_X", Lib_Card.Configure.Parameter.Coordinate_WashBottle_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_WashBottle_Y", Lib_Card.Configure.Parameter.Coordinate_WashBottle_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                        //读取当前坐标写入
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_WashBottle_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_WashBottle_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_Card.Configure.Parameter.Coordinate_WashBottle_X = Convert.ToInt32(TxtRPosX.Text);
                        Lib_Card.Configure.Parameter.Coordinate_WashBottle_Y = Convert.ToInt32(TxtRPosY.Text);
                    }
                }
                else
                {
                    DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("Definite write?", "Tips", MessageBoxButtons.YesNo, true);

                    if (dialogResult == DialogResult.No)
                    {
                        return;
                    }
                    else
                    {

                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_WashBottle_X", Lib_Card.Configure.Parameter.Coordinate_WashBottle_X.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_WashBottle_Y", Lib_Card.Configure.Parameter.Coordinate_WashBottle_Y.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                        //读取当前坐标写入
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_WashBottle_X", TxtRPosX.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_File.Ini.WriteIni("Coordinate", "Coordinate_WashBottle_Y", TxtRPosY.Text, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                        Lib_Card.Configure.Parameter.Coordinate_WashBottle_X = Convert.ToInt32(TxtRPosX.Text);
                        Lib_Card.Configure.Parameter.Coordinate_WashBottle_Y = Convert.ToInt32(TxtRPosY.Text);
                    }
                }
            }
            else
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    FADM_Form.CustomMessageBox.Show("请选择目标区域！", "温馨提示", MessageBoxButtons.OK, false);
                else
                    FADM_Form.CustomMessageBox.Show("Please select the target area！", "Tips", MessageBoxButtons.OK, false);
                FADM_Object.Communal.WriteDripWait(false);
                return;

            }
        }
        //写入杯子坐标
        private void WriteCupCoordinate(int i_No,string s_X,string s_Y)
        {
            switch (i_No)
            {

                case 1:

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup1_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup1_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup1_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup1_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup1_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup1_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup1_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup1_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 2:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup2_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup2_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup2_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup2_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup2_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup2_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup2_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup2_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 3:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup3_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup3_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup3_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup3_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup3_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup3_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup3_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup3_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 4:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup4_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup4_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup4_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup4_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup4_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup4_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup4_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup4_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 5:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup5_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup5_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup5_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup5_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup5_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup5_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup5_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup5_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 6:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup6_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup6_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup6_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup6_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup6_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup6_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup6_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup6_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 7:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup7_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup7_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup7_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup7_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup7_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup7_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup7_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup7_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 8:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup8_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup8_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup8_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup8_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup8_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup8_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup8_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup8_IntervalY = Convert.ToInt32(s_Y);
                    break;
                case 9:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup9_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup9_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup9_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup9_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup9_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup9_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup9_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup9_IntervalY = Convert.ToInt32(s_Y);
                    break;
                case 10:

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup10_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup10_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup10_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup10_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup10_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup10_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup10_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup10_IntervalY = Convert.ToInt32(s_Y);
                    break;
                case 11:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup11_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup11_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup11_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup11_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup11_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup11_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup11_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup11_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 12:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup12_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup12_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup12_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup12_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup12_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup12_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup12_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup12_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 13:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup13_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup13_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup13_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup13_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup13_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup13_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup13_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup13_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 14:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup14_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup14_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup14_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup14_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup14_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup14_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup14_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup14_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 15:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup15_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup15_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup15_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup15_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup15_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup15_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup15_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup15_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 16:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup16_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup16_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup16_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup16_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup16_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup16_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup16_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup16_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 17:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup17_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup17_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup17_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup17_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup17_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup17_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup17_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup17_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 18:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup18_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup18_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup18_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup18_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup18_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup18_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup18_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup18_IntervalY = Convert.ToInt32(s_Y);
                    break;
                case 19:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup19_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup19_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup19_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup19_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup19_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup19_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup19_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup19_IntervalY = Convert.ToInt32(s_Y);
                    break;
                case 20:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup20_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup20_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup20_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup20_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup20_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup20_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup20_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup20_IntervalY = Convert.ToInt32(s_Y);
                    break;
                case 21:

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup21_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup21_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup21_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup21_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");


                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup21_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup21_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup21_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup21_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 22:

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup22_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup22_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup22_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup22_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup22_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup22_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup22_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup22_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 23:

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup23_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup23_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup23_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup23_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");


                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup23_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup23_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup23_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup23_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 24:

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup24_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup24_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup24_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup24_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup24_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup24_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup24_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup24_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 25:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup25_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup25_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup25_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup25_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup25_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup25_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup25_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup25_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 26:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup26_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup26_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup26_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup26_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup26_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup26_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup26_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup26_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 27:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup27_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup27_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup27_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup27_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup27_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup27_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup27_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup27_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 28:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup28_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup28_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup28_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup28_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup28_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup28_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup28_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup28_IntervalY = Convert.ToInt32(s_Y);
                    break;
                case 29:

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup29_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup29_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup29_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup29_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup29_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup29_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup29_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup29_IntervalY = Convert.ToInt32(s_Y);
                    break;
                case 30:

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup30_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup30_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup30_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup30_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup30_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup30_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup30_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup30_IntervalY = Convert.ToInt32(s_Y);
                    break;
                case 31:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup31_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup31_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup31_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup31_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup31_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup31_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup31_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup31_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 32:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup32_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup32_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup32_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup32_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup32_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup32_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup32_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup32_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 33:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup33_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup33_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup33_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup33_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup33_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup33_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup33_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup33_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 34:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup34_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup34_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup34_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup34_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup34_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup34_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup34_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup34_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 35:

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup35_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup35_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup35_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup35_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup35_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup35_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup35_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup35_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 36:

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup36_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup36_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup36_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup36_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup36_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup36_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup36_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup36_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 37:

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup37_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup37_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup37_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup37_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup37_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup37_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup37_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup37_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 38:

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup38_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup38_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup38_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup38_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup38_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup38_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup38_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup38_IntervalY = Convert.ToInt32(s_Y);
                    break;
                case 39:

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup39_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup39_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup39_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup39_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup39_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup39_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup39_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup39_IntervalY = Convert.ToInt32(s_Y);
                    break;
                case 40:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup40_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup40_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup40_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup40_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup40_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup40_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup40_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup40_IntervalY = Convert.ToInt32(s_Y);
                    break;
                case 41:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup41_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup41_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup41_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup41_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup41_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup41_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup41_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup41_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 42:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup42_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup42_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup42_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup42_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup42_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup42_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup42_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup42_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 43:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup43_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup43_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup43_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup43_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup43_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup43_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup43_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup43_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 44:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup44_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup44_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup44_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup44_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup44_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup44_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup44_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup44_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 45:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup45_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup45_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup45_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup45_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup45_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup45_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup45_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup45_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 46:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup46_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup46_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup46_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup46_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup46_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup46_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup46_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup46_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 47:

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup47_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup47_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup47_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup47_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup47_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup47_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup47_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup47_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 48:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup48_IntervalX", Lib_Card.Configure.Parameter.Coordinate_Cup48_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup48_IntervalY", Lib_Card.Configure.Parameter.Coordinate_Cup48_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup48_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_Cup48_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_Cup48_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_Cup48_IntervalY = Convert.ToInt32(s_Y);
                    break;
                default:
                    break;
            }
        }
        //写入杯盖坐标
        private void WriteCupCoverCoordinate(int i_No,string s_X,string s_Y)
        {
            switch (i_No)
            {

                case 1:

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover1_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover1_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover1_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover1_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover1_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover1_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover1_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover1_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 2:

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover2_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover2_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover2_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover2_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover2_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover2_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover2_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover2_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 3:

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover3_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover3_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover3_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover3_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover3_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover3_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover3_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover3_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 4:

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover4_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover4_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover4_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover4_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover4_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover4_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover4_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover4_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 5:

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover5_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover5_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover5_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover5_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover5_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover5_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover5_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover5_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 6:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover6_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover6_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover6_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover6_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover6_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover6_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover6_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover6_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 7:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover7_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover7_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover7_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover7_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover7_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover7_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover7_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover7_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 8:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover8_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover8_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover8_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover8_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover8_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover8_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover8_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover8_IntervalY = Convert.ToInt32(s_Y);
                    break;
                case 9:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover9_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover9_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover9_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover9_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover9_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover9_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover9_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover9_IntervalY = Convert.ToInt32(s_Y);
                    break;
                case 10:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover10_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover10_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover10_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover10_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover10_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover10_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover10_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover10_IntervalY = Convert.ToInt32(s_Y);
                    break;
                case 11:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover11_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover11_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover11_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover11_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover11_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover11_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover11_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover11_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 12:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover12_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover12_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover12_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover12_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover12_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover12_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover12_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover12_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 13:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover13_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover13_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover13_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover13_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover13_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover13_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover13_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover13_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 14:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover14_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover14_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover14_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover14_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover14_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover14_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover14_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover14_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 15:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover15_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover15_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover15_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover15_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover15_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover15_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover15_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover15_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 16:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover16_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover16_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover16_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover16_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover16_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover16_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover16_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover16_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 17:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover17_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover17_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover17_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover17_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover17_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover17_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover17_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover17_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 18:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover18_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover18_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover18_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover18_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover18_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover18_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover18_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover18_IntervalY = Convert.ToInt32(s_Y);
                    break;
                case 19:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover19_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover19_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover19_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover19_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover19_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover19_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover19_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover19_IntervalY = Convert.ToInt32(s_Y);
                    break;
                case 20:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover20_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover20_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover20_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover20_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover20_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover20_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover20_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover20_IntervalY = Convert.ToInt32(s_Y);
                    break;
                case 21:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover21_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover21_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover21_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover21_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover21_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover21_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover21_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover21_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 22:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover22_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover22_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover22_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover22_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover22_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover22_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover22_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover22_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 23:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover23_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover23_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover23_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover23_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover23_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover23_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover23_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover23_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 24:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover24_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover24_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover24_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover24_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover24_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover24_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover24_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover24_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 25:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover25_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover25_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover25_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover25_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover25_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover25_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover25_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover25_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 26:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover26_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover26_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover26_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover26_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover26_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover26_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover26_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover26_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 27:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover27_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover27_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover27_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover27_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover27_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover27_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover27_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover27_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 28:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover28_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover28_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover28_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover28_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover28_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover28_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover28_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover28_IntervalY = Convert.ToInt32(s_Y);
                    break;
                case 29:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover29_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover29_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover29_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover29_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover29_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover29_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover29_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover29_IntervalY = Convert.ToInt32(s_Y);
                    break;
                case 30:

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover30_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover30_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover30_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover30_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");


                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover30_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover30_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover30_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover30_IntervalY = Convert.ToInt32(s_Y);
                    break;
                case 31:

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover31_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover31_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover31_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover31_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover31_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover31_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover31_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover31_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 32:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover32_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover32_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover32_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover32_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover32_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover32_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover32_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover32_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 33:

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover33_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover33_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover33_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover33_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover33_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover33_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover33_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover33_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 34:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover34_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover34_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover34_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover34_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover34_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover34_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover34_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover34_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 35:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover35_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover35_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover35_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover35_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover35_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover35_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover35_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover35_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 36:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover36_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover36_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover36_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover36_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover36_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover36_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover36_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover36_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 37:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover37_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover37_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover37_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover37_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover37_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover37_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover37_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover37_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 38:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover38_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover38_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover38_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover38_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover38_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover38_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover38_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover38_IntervalY = Convert.ToInt32(s_Y);
                    break;
                case 39:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover39_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover39_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover39_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover39_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover39_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover39_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover39_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover39_IntervalY = Convert.ToInt32(s_Y);
                    break;
                case 40:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover40_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover40_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover40_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover40_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover40_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover40_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover40_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover40_IntervalY = Convert.ToInt32(s_Y);
                    break;
                case 41:

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover41_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover41_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover41_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover41_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover41_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover41_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover41_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover41_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 42:

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover42_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover42_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover42_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover42_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover42_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover42_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover42_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover42_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 43:

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover43_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover43_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover43_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover43_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover43_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover43_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover43_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover43_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 44:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover44_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover44_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover44_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover44_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover44_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover44_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover44_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover44_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 45:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover45_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover45_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover45_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover45_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover45_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover45_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover45_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover45_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 46:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover46_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover46_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover46_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover46_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover46_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover46_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover46_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover46_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 47:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover47_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover47_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover47_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover47_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover47_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover47_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover47_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover47_IntervalY = Convert.ToInt32(s_Y);
                    break;

                case 48:
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover48_IntervalX", Lib_Card.Configure.Parameter.Coordinate_CupCover48_IntervalX.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover48_IntervalY", Lib_Card.Configure.Parameter.Coordinate_CupCover48_IntervalY.ToString(), Environment.CurrentDirectory + "\\Config\\Config.ini");

                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover48_IntervalX", s_X, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_File.Ini.WriteIni("Coordinate", "Coordinate_CupCover48_IntervalY", s_Y, Environment.CurrentDirectory + "\\Config\\parameter.ini");
                    Lib_Card.Configure.Parameter.Coordinate_CupCover48_IntervalX = Convert.ToInt32(s_X);
                    Lib_Card.Configure.Parameter.Coordinate_CupCover48_IntervalY = Convert.ToInt32(s_Y);
                    break;
                default:
                    break;
            }
        }

        private void BtnOutPut_Wash_In_Click(object sender, EventArgs e)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            { }
            else
            {
                try
                {
                    _b_istrue = true;
                    if (this.BtnOutPut_Wash_In.ForeColor == Color.Red)
                    {
                        _ia_array[0] = 45;
                    }
                    else
                    {
                        _ia_array[0] = 44;
                    }
                    int i_state = FADM_Object.Communal._tcpModBus.Write(811, _ia_array);

                }
                catch { }
                finally
                {
                    _b_istrue = false;
                }
            }
        }

        private void BtnOutPut_Wash_Out_Click(object sender, EventArgs e)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            { }
            else
            {
                try
                {
                    _b_istrue = true;
                    if (this.BtnOutPut_Wash_Out.ForeColor == Color.Red)
                    {
                        _ia_array[0] = 47;
                    }
                    else
                    {
                        _ia_array[0] = 46;
                    }
                    int i_state = FADM_Object.Communal._tcpModBus.Write(811, _ia_array);

                }
                catch { }
                finally
                {
                    _b_istrue = false;
                }
            }
        }

        private void BtnOutPut_Wash_Blow_Click(object sender, EventArgs e)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            { }
            else
            {
                try
                {
                    _b_istrue = true;
                    if (this.BtnOutPut_Wash_Blow.ForeColor == Color.Red)
                    {
                        _ia_array[0] = 49;
                    }
                    else
                    {
                        _ia_array[0] = 48;
                    }
                    int i_state = FADM_Object.Communal._tcpModBus.Write(811, _ia_array);

                }
                catch { }
                finally
                {
                    _b_istrue = false;
                }
            }
        }

        private void BtnOutPut_Dissolving_Click(object sender, EventArgs e)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                return;
            }
            else
            {
                try
                {
                    _b_istrue = true;
                    if (this.BtnOutPut_Dissolving.ForeColor == Color.Red)
                    {
                        _ia_array[0] = 51;
                    }
                    else
                    {
                        _ia_array[0] = 50;
                    }
                    int i_state = FADM_Object.Communal._tcpModBus.Write(811, _ia_array);

                }
                catch { }
                finally
                {
                    _b_istrue = false;
                }
            }
        }

        private void BtnOutPut_Slow_2_Click(object sender, EventArgs e)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                //if (Lib_Card.Configure.Parameter.Machine_BlockCylinder != 0)
                //{
                //    //气缸到阻挡位
                //    Thread thread = new Thread(Block);
                //    thread.Start();
                //}
            }
            else
            {
                try
                {
                    _b_istrue = true;
                    _ia_array[0] = 52;
                    int i_state = FADM_Object.Communal._tcpModBus.Write(811, _ia_array);

                }
                catch { }
                finally
                {
                    _b_istrue = false;
                }
            }
        }

        private void BtnOutPut_Slow_3_Click(object sender, EventArgs e)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                //if (Lib_Card.Configure.Parameter.Machine_BlockCylinder != 0)
                //{
                //    //气缸到阻挡位
                //    Thread thread = new Thread(Block);
                //    thread.Start();
                //}
            }
            else
            {
                try
                {
                    _b_istrue = true;
                    _ia_array[0] = 53;
                    int i_state = FADM_Object.Communal._tcpModBus.Write(811, _ia_array);

                }
                catch { }
                finally
                {
                    _b_istrue = false;
                }
            }
        }
    }
}
