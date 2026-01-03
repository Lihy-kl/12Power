using Lib_File;
using SmartDyeing.FADM_Control;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SmartDyeing.FADM_Object;
using System.IO;
using Microsoft.Win32;
using static Lib_Card.CardObject;
using EasyModbus;
using static System.Windows.Forms.AxHost;
using System.Collections;
using SmartDyeing.FADM_Auto;
using Lib_DataBank.MySQL;
using Newtonsoft.Json.Linq;

namespace SmartDyeing.FADM_Form
{
    public partial class Main : Form
    {
        [DllImport("user32.dll", EntryPoint = "FindWindow", CharSet = CharSet.Auto)]
        private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public const int WM_CLOSE = 0x10;

        Thread _thread_check = null;

        bool b = false;
        //配置需要播放次数 0是无限播放
        string s_SpeechCount;
        public Main()
        {
            InitializeComponent();
        }
        private void Main_Load(object sender, EventArgs e)
        {
            string s_path = Environment.CurrentDirectory + "\\Config\\Config.ini";
            string s_balanceInDrip = Lib_File.Ini.GetIni("Setting", "IsBalanceInDrip", "1", s_path);
            if (s_balanceInDrip == "0")
            {
                FADM_Object.Communal._b_isBalanceInDrip = false;
            }

            string s_isCupAreaOnly = Lib_File.Ini.GetIni("Setting", "IsCupAreaOnly", "0", s_path);
            if (s_isCupAreaOnly == "1")
            {
                FADM_Object.Communal._b_isCupAreaOnly = true;
            }
            FADM_Object.Communal._s_MachineNumber = Lib_File.Ini.GetIni("Setting", "MachineNumber", "01", s_path);

            string s_isUseAutoChoose = Lib_File.Ini.GetIni("Setting", "IsUseAutoChoose", "1", s_path);
            if (s_isUseAutoChoose == "0")
            {
                FADM_Object.Communal._b_isUseAutoChoose = false;
            }

            FADM_Object.Communal._i_AskTimes = Convert.ToInt32(Lib_File.Ini.GetIni("Setting", "AskTimes", "5", s_path));
            if(FADM_Object.Communal._i_AskTimes < 2)
            {
                FADM_Object.Communal._i_AskTimes = 2;
            }

            string s_newSet = Lib_File.Ini.GetIni("Setting", "IsNewSet", "1", s_path);
            if (s_newSet == "0")
            {
                FADM_Object.Communal._b_isNewSet = false;
            }

            string isUseAuto = Lib_File.Ini.GetIni("Setting", "IsUseAuto", "1", s_path);
            if (isUseAuto == "0")
            {
                FADM_Object.Communal._b_isUseAuto = false;
            }

            string s_isUseABAssistant = Lib_File.Ini.GetIni("Setting", "IsUseABAssistant", "0", s_path);
            if (s_isUseABAssistant == "1")
            {
                FADM_Object.Communal._b_isUseABAssistant = true;
            }

            string s_ABAssistantCount = Lib_File.Ini.GetIni("Setting", "ABAssistantCount", "2", s_path);
            FADM_Object.Communal._i_ABAssistantCount = Convert.ToInt32(s_ABAssistantCount);

            string s_NeedToDoTime = Lib_File.Ini.GetIni("Setting", "NeedToDoTime", "60", s_path);
            string s_NeedToDo = Lib_File.Ini.GetIni("Setting", "NeedToDo", "0", s_path);
            if (s_NeedToDo == "1")
            {
                FADM_Object.Communal._b_NeedTodo = true;
                timerNeedToDo.Interval = Convert.ToInt32(s_NeedToDoTime) * 1000;
                timerNeedToDo.Enabled = true;
            }

            string s_UpdateTime = Lib_File.Ini.GetIni("Setting", "UpdateTime", "60", s_path);
            string s_Update = Lib_File.Ini.GetIni("Setting", "Update", "0", s_path);
            if (s_Update == "1")
            {
                timer_Update.Interval = Convert.ToInt32(s_UpdateTime) * 1000;
                timer_Update.Enabled = true;
            }

            s_SpeechCount = Lib_File.Ini.GetIni("Setting", "SpeechCount", "0", s_path);

            string s_isAloneDripReserve = Lib_File.Ini.GetIni("Setting", "IsAloneDripReserve", "0", s_path);
            if (s_isAloneDripReserve == "1")
            {
                FADM_Object.Communal._b_isAloneDripReserve = true;
            }
            if (!FADM_Object.Communal._b_isJustShowInfo)
                countDown();

            string s_is60Mark = Lib_File.Ini.GetIni("Setting", "Is60Mark", "0", s_path);
            if (s_is60Mark == "1")
            {
                FADM_Object.Communal._b_is60Mark = true;
                
                TmrTemp.Interval = 60000;
                TmrTemp.Enabled = true;
            }

            string s_isHaveDyeStop = Lib_File.Ini.GetIni("Setting", "IsHaveDyeStop", "0", s_path);
            if (s_isHaveDyeStop == "1")
            {
                FADM_Object.Communal._b_isHaveDyeStop = true;
            }

            string s_isHaveAutoBrew = Lib_File.Ini.GetIni("Setting", "IsHaveAutoBrew", "0", s_path);
            if (s_isHaveAutoBrew == "1")
            {
                FADM_Object.Communal._b_isHaveAutoBrew = true;
            }

            string s_isHaveAutoWashBottle = Lib_File.Ini.GetIni("Setting", "IsHaveAutoWashBottle", "0", s_path);
            if (s_isHaveAutoWashBottle == "1")
            {
                FADM_Object.Communal._b_isHaveAutoWashBottle = true;
            }

            string s_isAutoWashSys = Lib_File.Ini.GetIni("Setting", "IsAutoWashSys", "1", s_path);
            if (s_isAutoWashSys == "0")
            {
                FADM_Object.Communal._b_isAutoWashSys = false;
            }



            BtnMain_Click(sender, e);

            if (!FADM_Object.Communal._b_isJustShowInfo && !FADM_Object.Communal._b_isUseBrewOnly)
            {
                Thread thread = new Thread(ReadBalance); //读取天平数据和光幕数据
                thread.IsBackground = true;
                thread.Start();
            }

            //Thread thread3 = new Thread(ReadBalance1); //读取天平数据和光幕数据
            //thread3.IsBackground = true;
            //thread3.Start();

            //Thread thread2 = new Thread(WaitList1); //读取天平数据和光幕数据
            //thread2.IsBackground = true;
            //thread2.Start();



            for (int i = 0; i < FADM_Auto.Dye._ia_stop.Count(); i++)
            {
                FADM_Auto.Dye._ia_stop[i] = 0;
                FADM_Auto.Dye._ia_stopSend[i] = 0;
            }

            if (!FADM_Object.Communal._b_isJustShowInfo )
            {

                Thread thread1 = new Thread(new FADM_Auto.Dye().ClothDyeing); //打板机相关线程
                thread1.IsBackground = true;
                thread1.Start();

                if (!FADM_Object.Communal._b_isJustShowInfo && !FADM_Object.Communal._b_isHaveAutoBrew)
                {
                    System.Threading.Thread P_thd_brew = new System.Threading.Thread(SmartDyeing.FADM_Auto.MyBrew.Brew);
                    P_thd_brew.IsBackground = true;
                    P_thd_brew.Start();
                }
            }

            if (Lib_Card.Configure.Parameter.Other_UseAbs == 1)
            {
                System.Threading.Thread P_thd_abs = new System.Threading.Thread(SmartDyeing.FADM_Auto.MyAbsorbance.Absorbance);
                P_thd_abs.IsBackground = true;
                P_thd_abs.Start();

                //自动插入吸光度测量
                Thread P_thd4 = new Thread(InsertAbs);
                P_thd4.IsBackground = true;
                P_thd4.Start();
            }
            else
            {
                toolStripButton1.Visible = false;
                toolStripSeparator8.Visible = false;
                MiAbsDebug.Visible = false;
                MiABSProcess.Visible = false;
            }

            //有自动开料
            if(FADM_Object.Communal._b_isHaveAutoBrew)
            {
                Thread thread1 = new Thread(new FADM_Auto.AutoBrew().Brew); //自动开料线程
                thread1.IsBackground = true;
                thread1.Start();

                Thread thread2 = new Thread(new FADM_Auto.AutoBrew().WashBottle); //自动洗瓶线程
                thread2.IsBackground = true;
                thread2.Start();

                //自动加入自动开料
                Thread P_thd4 = new Thread(InsertBrew);
                P_thd4.IsBackground = true;
                P_thd4.Start();
            }

            if(!FADM_Object.Communal._b_isCupAreaOnly)
            {
                CupStatus.Visible = false;
                toolStripSeparator11.Visible = false;
            }
            if (!FADM_Object.Communal._b_isJustShowInfo)
            {
                //播报线程
                Thread P_thd = new Thread(Speech);
                P_thd.IsBackground = true;
                P_thd.Start();

                if (FADM_Object.Communal._b_isUseAutoChoose)
                {
                    //自动点击待办事项线程
                    Thread P_thd3 = new Thread(AutoChoose);
                    P_thd3.IsBackground = true;
                    P_thd3.Start();
                }

                //自动加入批次
                Thread P_thd1 = new Thread(WaitList);
                P_thd1.IsBackground = true;
                P_thd1.Start();

                //开启线程，自动备份90天之前的数据
                Thread P_thd2 = new Thread(BackUp);
                P_thd2.IsBackground = true;
                P_thd2.Start();

                
            }

            //设置分光仪信息
            string s_isUse = Lib_File.Ini.GetIni("Spectrometer", "IsUse", s_path);
            this._s_m_route = Lib_File.Ini.GetIni("Spectrometer", "Route", s_path);
            _s_m_mode = Ini.GetIni("Spectrometer", "Mode", "0", s_path);

            double d_mytime = Convert.ToDouble(Lib_File.Ini.GetIni("Spectrometer", "Time", s_path) == "" ? "0" : Lib_File.Ini.GetIni("Spectrometer", "Time", s_path));

            if (_s_m_route != "" && s_isUse == "1")
            {
                TmrFGY.Interval = Convert.ToInt16(d_mytime * 1000);
                TmrFGY.Enabled = true;
            }

            //设置分光仪信息
            string s_isUse_ASL = Lib_File.Ini.GetIni("Spectrometer_ASL", "IsUse", "0", s_path);
            this._s_m_route_ASL = Lib_File.Ini.GetIni("Spectrometer_ASL", "Route", "", s_path);

            double d_mytime_ASL = Convert.ToDouble(Lib_File.Ini.GetIni("Spectrometer_ASL", "Time", "10", s_path) == "" ? "0" : Lib_File.Ini.GetIni("Spectrometer_ASL", "Time", "10", s_path));

            if (_s_m_route_ASL != "" && s_isUse_ASL == "1")
            {
                timer1.Interval = Convert.ToInt16(d_mytime_ASL * 1000);
                timer1.Enabled = true;
            }

            //string sPath = Environment.CurrentDirectory + "\\Config.ini";
            string s_out = Lib_File.Ini.GetIni("Setting", "IsOutDrip", "0", s_path);
            if (s_out == "0")
            {
                MiOut1.Checked = false;
                FADM_Object.Communal._b_isOutDrip = false;
            }
            else
            {
                MiOut1.Checked = true;
                FADM_Object.Communal._b_isOutDrip = true;
            }
            string s_low = Lib_File.Ini.GetIni("Setting", "IsLowDrip", "0", s_path);
            if (s_low == "0")
            {
                MiLow1.Checked = false;
                FADM_Object.Communal._b_isLowDrip = false;
            }
            else
            {
                MiLow1.Checked = true;
                FADM_Object.Communal._b_isLowDrip = true;
            }
            string s_full = Lib_File.Ini.GetIni("Setting", "IsFullDrip", "1", s_path);
            if (s_full == "0")
            {
                MiFullDrip1.Checked = false;
                FADM_Object.Communal._b_isFullDrip = false;
            }
            else
            {
                MiFullDrip1.Checked = true;
                FADM_Object.Communal._b_isFullDrip = true;
            }

            string s_outAllow = Lib_File.Ini.GetIni("Setting", "IsOutDripAllow", "1", s_path);
            if (s_outAllow == "0")
            {
                MiOutAllowDrip.Checked = false;
                FADM_Object.Communal._b_isOutDripAllow = false;
            }
            else
            {
                MiOutAllowDrip.Checked = true;
                FADM_Object.Communal._b_isOutDripAllow = true;
            }
            string s_lowAllow = Lib_File.Ini.GetIni("Setting", "IsLowDripAllow", "1", s_path);
            if (s_lowAllow == "0")
            {
                MiLowAllowDrip.Checked = false;
                FADM_Object.Communal._b_isLowDripAllow = false;
            }
            else
            {
                MiLowAllowDrip.Checked = true;
                FADM_Object.Communal._b_isLowDripAllow = true;
            }

            string s_zero = Lib_File.Ini.GetIni("Setting", "IsZero", "0", s_path);
            if (s_zero == "1")
            {
                FADM_Object.Communal._b_isZero = true;
            }

            string s_registerOld = Lib_File.Ini.GetIni("Setting", "registerOld", "0", s_path);
            if (s_registerOld == "0")
            {
                FADM_Object.Communal._b_registerOld = false;
            }
            else
            {
                FADM_Object.Communal._b_registerOld = true;
            }

            string s_isNetWork = Lib_File.Ini.GetIni("Setting", "IsNetWork", "0", s_path);
            if (s_isNetWork == "0")
            {
                FADM_Object.Communal._b_isNetWork = false;
            }
            else
            {
                FADM_Object.Communal._b_isNetWork = true;
            }

            string IsUnitChange = Lib_File.Ini.GetIni("Setting", "IsUnitChange", "0", s_path);
            if (IsUnitChange == "0")
            {
                FADM_Object.Communal._b_isUnitChange = false;
            }
            else
            {
                FADM_Object.Communal._b_isUnitChange = true;
            }

            string s_dripAll = Lib_File.Ini.GetIni("Setting", "IsDripAll", "0", s_path);
            if (s_dripAll == "1")
            {
                FADM_Object.Communal._b_isDripAll = true;
            }
            string s_assitantFirst = Lib_File.Ini.GetIni("Setting", "IsAssitantFirst", "0", s_path);
            if (s_assitantFirst == "1")
            {
                FADM_Object.Communal._b_isAssitantFirst = true;
            }
            string s_needCheck = Lib_File.Ini.GetIni("Setting", "IsNeedCheck", "1", s_path);
            if (s_needCheck == "0")
            {
                FADM_Object.Communal._b_isNeedCheck = false;
            }
            string s_isUseMotherDate = Lib_File.Ini.GetIni("Setting", "IsUseMotherDate", "0", s_path);
            if (s_isUseMotherDate == "1")
            {
                FADM_Object.Communal._b_isUseMotherDate = true;
            }
            string s_finishSend = Lib_File.Ini.GetIni("Setting", "IsFinishSend", "1", s_path);
            if (s_finishSend == "0")
            {
                FADM_Object.Communal._b_isFinishSend = false;
            }
            string s_dripReserveFirst = Lib_File.Ini.GetIni("Setting", "IsDripReserveFirst", "1", s_path);
            if (s_dripReserveFirst == "0")
            {
                FADM_Object.Communal._b_isDripReserveFirst = false;
            }
            string s_debug = Lib_File.Ini.GetIni("Setting", "IsDebug", "0", s_path);
            if (s_debug == "1")
            {
                FADM_Object.Communal._b_isDebug = true;
            }
            string s_saveRealConcentration = Lib_File.Ini.GetIni("Setting", "IsSaveRealConcentration", "0", s_path);
            if (s_saveRealConcentration == "1")
            {
                FADM_Object.Communal._b_isSaveRealConcentration = true;
            }

            string s_useClamp = Lib_File.Ini.GetIni("Setting", "IsUseClamp", "0", s_path);
            if (s_useClamp == "1")
            {
                FADM_Object.Communal._b_isUseClamp = true;
            }

            string s_useClampOut = Lib_File.Ini.GetIni("Setting", "IsUseClampOut", "0", s_path);
            if (s_useClampOut == "1")
            {
                FADM_Object.Communal._b_isUseClampOut = true;
            }

            string s_useClampOutBig = Lib_File.Ini.GetIni("Setting", "IsUseClampOutBig", "1", s_path);
            if (s_useClampOutBig == "0")
            {
                FADM_Object.Communal._b_isUseClampOutBig = false;
            }

            string s_usePH = Lib_File.Ini.GetIni("Setting", "IsUsePH", "0", s_path);
            if (s_usePH == "1")
            {
                FADM_Object.Communal._b_isUsePH = true;
            }

            string s_PHSys = Lib_File.Ini.GetIni("Setting", "IsPHSys", "0", s_path);
            if (s_PHSys == "1")
            {
                FADM_Object.Communal._b_isPHSys = true;
            }

            string IsDyMin = Lib_File.Ini.GetIni("Setting", "IsDyMin", "0", s_path);
            if (IsDyMin == "0")
            {
                FADM_Object.Communal._b_isDyMin = 0;
            }
            else
            {
                FADM_Object.Communal._b_isDyMin = Convert.ToInt32(IsDyMin);
            }

            string _b_DyeCupNum = Lib_File.Ini.GetIni("Setting", "IsDyeCupNum", "0", s_path);
            if (_b_DyeCupNum == "0")
            {
                FADM_Object.Communal._b_DyeCupNum = 0;
            }
            else
            {
                FADM_Object.Communal._b_DyeCupNum = Convert.ToInt32(_b_DyeCupNum);
            }











            string isppm = Lib_File.Ini.GetIni("Setting", "PPM", "30", s_path);
            FADM_Object.Communal._d_ppm = Convert.ToDouble(isppm);

            string absMas = Lib_File.Ini.GetIni("Setting", "absMax", "0.004", s_path);
            FADM_Object.Communal._d_absMax = Convert.ToDouble(absMas);

            string sTestSpan = Lib_File.Ini.GetIni("Setting", "TestSpan", "30", s_path);
            FADM_Object.Communal._d_TestSpan = Convert.ToDouble(sTestSpan);

            string sabs_total = Lib_File.Ini.GetIni("Setting", "AbsTotal", "50", s_path);
            FADM_Object.Communal._d_abs_total = Convert.ToDouble(sabs_total);

            string sabs_sub = Lib_File.Ini.GetIni("Setting", "AbsSub", "40", s_path);
            FADM_Object.Communal._d_abs_sub = Convert.ToDouble(sabs_sub);

            string sabs_DissolvingType = Lib_File.Ini.GetIni("Setting", "AbsDissolvingType", "0", s_path);
            FADM_Object.Communal._i_abs_DissolvingType = Convert.ToInt32(sabs_DissolvingType);

            FADM_Object.Communal._s_absPath = Lib_File.Ini.GetIni("Abs", "Path", "D:\\Abs\\", s_path);

            string s_desc = Lib_File.Ini.GetIni("Setting", "DESC", "0", s_path);
            if (s_desc == "1")
            {
                FADM_Object.Communal._b_isDesc = true;
            }

            string s_isShowSample = Lib_File.Ini.GetIni("Setting", "IsShowSample", "0", s_path);
            if (s_isShowSample == "1")
            {
                FADM_Object.Communal._b_isShowSample = true;
            }

            string s_Head_Ip = Lib_File.Ini.GetIni("CplImport", "Head_Ip", "1", s_path);
            FADM_Object.Communal._i_Head_Ip = Convert.ToInt32(s_Head_Ip);

            string s_Head_Ip_Len = Lib_File.Ini.GetIni("CplImport", "Head_Ip_Len", "4", s_path);
            FADM_Object.Communal._i_Head_Ip_Len = Convert.ToInt32(s_Head_Ip_Len);

            string s_Head_FormulaCode = Lib_File.Ini.GetIni("CplImport", "Head_FormulaCode", "5", s_path);
            FADM_Object.Communal._i_Head_FormulaCode = Convert.ToInt32(s_Head_FormulaCode);

            string s_Head_FormulaCode_Len = Lib_File.Ini.GetIni("CplImport", "Head_FormulaCode_Len", "12", s_path);
            FADM_Object.Communal._i_Head_FormulaCode_Len = Convert.ToInt32(s_Head_FormulaCode_Len);

            string s_Head_VersionNum = Lib_File.Ini.GetIni("CplImport", "Head_VersionNum", "17", s_path);
            FADM_Object.Communal._i_Head_VersionNum = Convert.ToInt32(s_Head_VersionNum);

            string s_Head_VersionNum_Len = Lib_File.Ini.GetIni("CplImport", "Head_VersionNum_Len", "2", s_path);
            FADM_Object.Communal._i_Head_VersionNum_Len = Convert.ToInt32(s_Head_VersionNum_Len);

            string s_Head_Count = Lib_File.Ini.GetIni("CplImport", "Head_Count", "19", s_path);
            FADM_Object.Communal._i_Head_Count = Convert.ToInt32(s_Head_Count);

            string s_Head_Count_Len = Lib_File.Ini.GetIni("CplImport", "Head_Count_Len", "2", s_path);
            FADM_Object.Communal._i_Head_Count_Len = Convert.ToInt32(s_Head_Count_Len);

            string s_Head_Unit = Lib_File.Ini.GetIni("CplImport", "Head_Unit", "21", s_path);
            FADM_Object.Communal._i_Head_Unit = Convert.ToInt32(s_Head_Unit);

            string s_Head_Unit_Len = Lib_File.Ini.GetIni("CplImport", "Head_Unit_Len", "1", s_path);
            FADM_Object.Communal._i_Head_Unit_Len = Convert.ToInt32(s_Head_Unit_Len);

            string s_Head_Index = Lib_File.Ini.GetIni("CplImport", "Head_Index", "22", s_path);
            FADM_Object.Communal._i_Head_Index = Convert.ToInt32(s_Head_Index);

            string s_Head_Index_Len = Lib_File.Ini.GetIni("CplImport", "Head_Index_Len", "3", s_path);
            FADM_Object.Communal._i_Head_Index_Len = Convert.ToInt32(s_Head_Index_Len);

            string s_Head_FormulaName = Lib_File.Ini.GetIni("CplImport", "Head_FormulaName", "25", s_path);
            FADM_Object.Communal._i_Head_FormulaName = Convert.ToInt32(s_Head_FormulaName);

            string s_Head_FormulaName_Len = Lib_File.Ini.GetIni("CplImport", "Head_FormulaName_Len", "24", s_path);
            FADM_Object.Communal._i_Head_FormulaName_Len = Convert.ToInt32(s_Head_FormulaName_Len);

            string s_Head_ClothWeight = Lib_File.Ini.GetIni("CplImport", "Head_ClothWeight", "49", s_path);
            FADM_Object.Communal._i_Head_ClothWeight = Convert.ToInt32(s_Head_ClothWeight);

            string s_Head_ClothWeight_Len = Lib_File.Ini.GetIni("CplImport", "Head_ClothWeight_Len", "8", s_path);
            FADM_Object.Communal._i_Head_ClothWeight_Len = Convert.ToInt32(s_Head_ClothWeight_Len);

            string s_Head_TotalWeight = Lib_File.Ini.GetIni("CplImport", "Head_TotalWeight", "57", s_path);
            FADM_Object.Communal._i_Head_TotalWeight = Convert.ToInt32(s_Head_TotalWeight);

            string s_Head_TotalWeight_Len = Lib_File.Ini.GetIni("CplImport", "Head_TotalWeight_Len", "8", s_path);
            FADM_Object.Communal._i_Head_TotalWeight_Len = Convert.ToInt32(s_Head_TotalWeight_Len);

            string s_Head_AddWater = Lib_File.Ini.GetIni("CplImport", "Head_AddWater", "65", s_path);
            FADM_Object.Communal._i_Head_AddWater = Convert.ToInt32(s_Head_AddWater);

            string s_Head_AddWater_Len = Lib_File.Ini.GetIni("CplImport", "Head_AddWater_Len", "1", s_path);
            FADM_Object.Communal._i_Head_AddWater_Len = Convert.ToInt32(s_Head_AddWater_Len);

            string s_Head_ConAdd = Lib_File.Ini.GetIni("CplImport", "Head_ConAdd", "66", s_path);
            FADM_Object.Communal._i_Head_ConAdd = Convert.ToInt32(s_Head_ConAdd);

            string s_Head_ConAdd_Len = Lib_File.Ini.GetIni("CplImport", "Head_ConAdd_Len", "1", s_path);
            FADM_Object.Communal._i_Head_ConAdd_Len = Convert.ToInt32(s_Head_ConAdd_Len);

            string s_Head_MNum = Lib_File.Ini.GetIni("CplImport", "Head_MNum", "67", s_path);
            FADM_Object.Communal._i_Head_MNum = Convert.ToInt32(s_Head_MNum);

            string s_Head_MNum_Len = Lib_File.Ini.GetIni("CplImport", "Head_MNum_Len", "2", s_path);
            FADM_Object.Communal._i_Head_MNum_Len = Convert.ToInt32(s_Head_MNum_Len);

            string s_Head_Date = Lib_File.Ini.GetIni("CplImport", "Head_Date", "69", s_path);
            FADM_Object.Communal._i_Head_Date = Convert.ToInt32(s_Head_Date);

            string s_Head_Date_Len = Lib_File.Ini.GetIni("CplImport", "Head_Date_Len", "8", s_path);
            FADM_Object.Communal._i_Head_Date_Len = Convert.ToInt32(s_Head_Date_Len);


            string s_Head_Code = Lib_File.Ini.GetIni("CplImport", "Head_Code", "77", s_path);
            FADM_Object.Communal._i_Head_Code = Convert.ToInt32(s_Head_Code);

            string s_Head_Code_Len = Lib_File.Ini.GetIni("CplImport", "Head_Code_Len", "8", s_path);
            FADM_Object.Communal._i_Head_Code_Len = Convert.ToInt32(s_Head_Code_Len);

            string s_Head_Type = Lib_File.Ini.GetIni("CplImport", "Head_Type", "85", s_path);
            FADM_Object.Communal._i_Head_Type = Convert.ToInt32(s_Head_Type);

            string s_Head_Type_Len = Lib_File.Ini.GetIni("CplImport", "Head_Type_Len", "1", s_path);
            FADM_Object.Communal._i_Head_Type_Len = Convert.ToInt32(s_Head_Type_Len);

            string s_Head_Drip = Lib_File.Ini.GetIni("CplImport", "Head_Drip", "86", s_path);
            FADM_Object.Communal._i_Head_Drip = Convert.ToInt32(s_Head_Drip);

            string s_Head_Drip_Len = Lib_File.Ini.GetIni("CplImport", "Head_Drip_Len", "1", s_path);
            FADM_Object.Communal._i_Head_Drip_Len = Convert.ToInt32(s_Head_Drip_Len);

            string s_Head_SIN = Lib_File.Ini.GetIni("CplImport", "Head_SIN", "87", s_path);
            FADM_Object.Communal._i_Head_SIN = Convert.ToInt32(s_Head_SIN);

            string s_Head_SIN_Len = Lib_File.Ini.GetIni("CplImport", "Head_SIN_Len", "2", s_path);
            FADM_Object.Communal._i_Head_SIN_Len = Convert.ToInt32(s_Head_SIN_Len);

            string s_Detail_Ip = Lib_File.Ini.GetIni("CplImport", "Detail_Ip", "1", s_path);
            FADM_Object.Communal._i_Detail_Ip = Convert.ToInt32(s_Detail_Ip);

            string s_Detail_Ip_Len = Lib_File.Ini.GetIni("CplImport", "Detail_Ip_Len", "4", s_path);
            FADM_Object.Communal._i_Detail_Ip_Len = Convert.ToInt32(s_Detail_Ip_Len);

            string s_Detail_FormulaCode = Lib_File.Ini.GetIni("CplImport", "Detail_FormulaCode", "5", s_path);
            FADM_Object.Communal._i_Detail_FormulaCode = Convert.ToInt32(s_Detail_FormulaCode);

            string s_Detail_FormulaCode_Len = Lib_File.Ini.GetIni("CplImport", "Detail_FormulaCode_Len", "12", s_path);
            FADM_Object.Communal._i_Detail_FormulaCode_Len = Convert.ToInt32(s_Detail_FormulaCode_Len);

            string s_Detail_VersionNum = Lib_File.Ini.GetIni("CplImport", "Detail_VersionNum", "17", s_path);
            FADM_Object.Communal._i_Detail_VersionNum = Convert.ToInt32(s_Detail_VersionNum);

            string s_Detail_VersionNum_Len = Lib_File.Ini.GetIni("CplImport", "Detail_VersionNum_Len", "2", s_path);
            FADM_Object.Communal._i_Detail_VersionNum_Len = Convert.ToInt32(s_Detail_VersionNum_Len);

            string s_Detail_Index = Lib_File.Ini.GetIni("CplImport", "Detail_Index", "19", s_path);
            FADM_Object.Communal._i_Detail_Index = Convert.ToInt32(s_Detail_Index);

            string s_Detail_Index_Len = Lib_File.Ini.GetIni("CplImport", "Detail_Index_Len", "2", s_path);
            FADM_Object.Communal._i_Detail_Index_Len = Convert.ToInt32(s_Detail_Index_Len);

            string s_Detail_Unit = Lib_File.Ini.GetIni("CplImport", "Detail_Unit", "21", s_path);
            FADM_Object.Communal._i_Detail_Unit = Convert.ToInt32(s_Detail_Unit);

            string s_Detail_Unit_Len = Lib_File.Ini.GetIni("CplImport", "Detail_Unit_Len", "1", s_path);
            FADM_Object.Communal._i_Detail_Unit_Len = Convert.ToInt32(s_Detail_Unit_Len);

            string s_Detail_Num = Lib_File.Ini.GetIni("CplImport", "Detail_Num", "22", s_path);
            FADM_Object.Communal._i_Detail_Num = Convert.ToInt32(s_Detail_Num);

            string s_Detail_Num_Len = Lib_File.Ini.GetIni("CplImport", "Detail_Num_Len", "3", s_path);
            FADM_Object.Communal._i_Detail_Num_Len = Convert.ToInt32(s_Detail_Num_Len);

            string s_Detail_AssistantCode = Lib_File.Ini.GetIni("CplImport", "Detail_AssistantCode", "25", s_path);
            FADM_Object.Communal._i_Detail_AssistantCode = Convert.ToInt32(s_Detail_AssistantCode);

            string s_Detail_AssistantCode_Len = Lib_File.Ini.GetIni("CplImport", "Detail_AssistantCode_Len", "8", s_path);
            FADM_Object.Communal._i_Detail_AssistantCode_Len = Convert.ToInt32(s_Detail_AssistantCode_Len);

            string s_Detail_RealConcentration = Lib_File.Ini.GetIni("CplImport", "Detail_RealConcentration", "33", s_path);
            FADM_Object.Communal._i_Detail_RealConcentration = Convert.ToInt32(s_Detail_RealConcentration);

            string s_Detail_RealConcentration_Len = Lib_File.Ini.GetIni("CplImport", "Detail_RealConcentration_Len", "9", s_path);
            FADM_Object.Communal._i_Detail_RealConcentration_Len = Convert.ToInt32(s_Detail_RealConcentration_Len);

            string s_Detail_SIN = Lib_File.Ini.GetIni("CplImport", "Detail_SIN", "42", s_path);
            FADM_Object.Communal._i_Detail_SIN = Convert.ToInt32(s_Detail_SIN);

            string s_Detail_SIN_Len = Lib_File.Ini.GetIni("CplImport", "Detail_SIN_Len", "2", s_path);
            FADM_Object.Communal._i_Detail_SIN_Len = Convert.ToInt32(s_Detail_SIN_Len);

            string s_isAddWaterFirst = Lib_File.Ini.GetIni("Setting", "IsAddWaterFirst", "1", s_path);
            if (s_isAddWaterFirst == "0")
            {
                FADM_Object.Communal._b_isAddWaterFirst = false;
            }

            string s_isSort = Lib_File.Ini.GetIni("Setting", "IsSort", "1", s_path);
            if (s_isSort == "0")
            {
                FADM_Object.Communal._b_isSort = false;
            }

            string s_isDripNeedCupNum = Lib_File.Ini.GetIni("Setting", "IsDripNeedCupNum", "0", s_path);
            if (s_isDripNeedCupNum == "1")
            {
                FADM_Object.Communal._b_isDripNeedCupNum = true;
            }

            string s_isUseCloth = Lib_File.Ini.GetIni("Setting", "IsUseCloth", "0", s_path);
            if (s_isUseCloth == "1")
            {
                FADM_Object.Communal._b_isUseCloth = true;
            }

            string s_isNeedConfirm = Lib_File.Ini.GetIni("Setting", "IsNeedConfirm", "0", s_path);
            if (s_isNeedConfirm == "1")
            {
                FADM_Object.Communal._b_isNeedConfirm = true;
            }

            string IsBathRatioTxtDyBath = Lib_File.Ini.GetIni("Setting", "IsBathRatioTxtDyBath", "0", s_path);
            if (IsBathRatioTxtDyBath == "1")
            {
                FADM_Object.Communal._b_isBathRatioTxtDyBath = true;
            }

            string s_isUseBrewOnly = Lib_File.Ini.GetIni("Setting", "IsUseBrewOnly", "0", s_path);
            if (s_isUseBrewOnly == "1")
            {
                FADM_Object.Communal._b_isUseBrewOnly = true;
            }

            string s_isStableRead = Lib_File.Ini.GetIni("Setting", "IsStableRead", "0", s_path);
            if (s_isStableRead == "1")
            {
                FADM_Object.Communal._b_isStableRead = true;
            }

            string s_isHighWash = Lib_File.Ini.GetIni("Setting", "IsHighWash", "0", s_path);
            if (s_isHighWash == "1")
            {
                FADM_Object.Communal._b_isHighWash = true;
            }

            string s_isUsePowerAB = Lib_File.Ini.GetIni("Setting", "PowerAB", "0", s_path);
            if (s_isUsePowerAB == "1")
            {
                FADM_Object.Communal._b_PowerAB = true;
            }

            string s_autoAbs = Lib_File.Ini.GetIni("Setting", "IsAutoAbs", "1", s_path);
            if (s_autoAbs == "0")
            {
                FADM_Object.Communal._b_isAutoAbs = false;
            }

            string s_isShowBottleStatus = Lib_File.Ini.GetIni("Setting", "IsShowBottleStatus", "0", s_path);
            if (s_isShowBottleStatus == "1")
            {
                FADM_Object.Communal._b_isShowBottleStatus = true;
            }

            string s_isCorrect = Lib_File.Ini.GetIni("Setting", "IsCorrect", "0", s_path);
            if (s_isCorrect == "1")
            {
                FADM_Object.Communal._b_isCorrect = true;
            }

            string s_isDripPriority = Lib_File.Ini.GetIni("Setting", "IsDripPriority", "1", s_path);
            if (s_isDripPriority == "0")
            {
                FADM_Object.Communal._b_isDripPriority = false;
            }

            string s_isSelf = Lib_File.Ini.GetIni("Setting", "SelfData", "1,0.5,2,5,11,2", s_path);
            

            string[] sa_e1 = s_isSelf.Split(',');
            FADM_Object.Communal._da_self = new double[sa_e1.Count()];
            for (int i = 0; i < sa_e1.Length; i++)
            {
                FADM_Object.Communal._da_self[i] = Convert.ToDouble(string.Format("{0:F2}", Convert.ToDouble(sa_e1[i])));
            }

            string s_isDripAddBatch = Lib_File.Ini.GetIni("Setting", "IsDripAddBatch", "0", s_path);
            if (s_isDripAddBatch == "1")
            {
                FADM_Object.Communal._b_isDripAddBatch = true;
            }

            string s_isUseLLJ = Lib_File.Ini.GetIni("Setting", "IsUseLLJ", "0", s_path);
            if (s_isUseLLJ == "1")
            {
                FADM_Object.Communal._b_isUseLLJ = true;
            }

            string s_isUseNewCorrectingWater = Lib_File.Ini.GetIni("Setting", "IsUseNewCorrectingWater", "0", s_path);
            if (s_isUseNewCorrectingWater == "1")
            {
                FADM_Object.Communal._b_isUseNewCorrectingWater = true;
            }

            string s_isUseTurntableCorrectingWater = Lib_File.Ini.GetIni("Setting", "IsUseTurntableCorrectingWater", "0", s_path);
            if (s_isUseTurntableCorrectingWater == "1")
            {
                FADM_Object.Communal._b_isUseTurntableCorrectingWater = true;
            }

            string s_isHasWashSyringe = Lib_File.Ini.GetIni("Setting", "IsHasWashSyringe", "0", s_path);
            if (s_isHasWashSyringe == "1")
            {
                FADM_Object.Communal._b_isHasWashSyringe = true;
            }

            string s_isWaterRecheck = Lib_File.Ini.GetIni("Setting", "IsWaterRecheck", "1", s_path);
            if (s_isWaterRecheck == "0")
            {
                FADM_Object.Communal._b_isWaterRecheck = false;
            }

            string s_isAssShowWater = Lib_File.Ini.GetIni("Setting", "IsAssShowWater", "0", s_path);
            if (s_isAssShowWater == "1")
            {
                FADM_Object.Communal._b_isAssShowWater = true;
            }

            string s_isBlockSaveButton = Lib_File.Ini.GetIni("Setting", "IsBlockSaveButton", "0", s_path);
            if (s_isBlockSaveButton == "1")
            {
                FADM_Object.Communal._b_isBlockSaveButton = true;
            }


            string s_AlarmDropWeight = Lib_File.Ini.GetIni("Setting", "AlarmDropWeight", "0.1", s_path);
            FADM_Object.Communal._d_AlarmDropWeight = Convert.ToDouble(s_AlarmDropWeight);

            string s_isCloseAddMed = Lib_File.Ini.GetIni("Setting", "IsCloseAddMed", "0", s_path);
            if (s_isCloseAddMed == "1")
            {
                FADM_Object.Communal._b_isCloseAddMed = true;
            }

            string s_isUseLAB = Lib_File.Ini.GetIni("Setting", "IsUseLAB", "0", s_path);
            if (s_isUseLAB == "1")
            {
                FADM_Object.Communal._b_isUseLAB = true;
            }

            string s_LabMode = Lib_File.Ini.GetIni("Setting", "LabMode", "1", s_path);
            FADM_Object.Communal._i_LabMode = Convert.ToInt32(s_LabMode);

            string s_isAssOrderByCode = Lib_File.Ini.GetIni("Setting", "IsAssOrderByCode", "0", s_path);
            if (s_isAssOrderByCode == "1")
            {
                FADM_Object.Communal._b_isAssOrderByCode = true;
            }
        }

        public void countDown()
        {
            SmartDyeing.FADM_Object.MyRegister softReg = new SmartDyeing.FADM_Object.MyRegister();
            RegistryKey retkey = Registry.CurrentUser.OpenSubKey("SOFTWARE", true).CreateSubKey("chec").CreateSubKey("Register.INI");
            foreach (string strRNum in retkey.GetSubKeyNames())
            {
                if (strRNum == softReg.GetRNum())
                {
                    this.toolStripLabel1.Text = "";
                    return;
                }
            }

            int i_usetime = 0;

            DateTime P_dt_create = new DateTime();

            DateTime P_dt_last = new DateTime();

            DateTime P_dt_now = DateTime.Now;
        again:

            try
            {
                if(Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\chec", "UseTimes", null) == null)
                {
                    //创建允许天数
                    string s_enP_int_day = FADM_Object.myAES.AesEncrypt("7");
                    Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\chec", "UseTimes", s_enP_int_day, RegistryValueKind.String);

                }
                if(Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\chec", "CreateDateTime", null)==null)
                {
                    string s_enNow = FADM_Object.myAES.AesEncrypt(DateTime.Now.ToString());
                    //修改创建时间
                    Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\chec", "CreateDateTime", s_enNow, RegistryValueKind.String);
                }
                //获取允许使用次数 天数
                string s_usetimeStr = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\chec", "UseTimes", "");
                s_usetimeStr = FADM_Object.myAES.AesDecrypt(s_usetimeStr);
                i_usetime = Convert.ToInt32(s_usetimeStr);

                //获取上次时间 
                P_dt_last = Convert.ToDateTime(Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\chec", "LastDateTime", null));

                //获取创建时间
                string s_createStr = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\chec", "CreateDateTime", "");
                s_createStr = FADM_Object.myAES.AesDecrypt(s_createStr);
                P_dt_create = Convert.ToDateTime(s_createStr);
            }
            catch (Exception ex)
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    FADM_Form.CustomMessageBox.Show("倒计时异常！", "警告", MessageBoxButtons.OK, false);
                else
                    FADM_Form.CustomMessageBox.Show("Countdown exception！", "warn", MessageBoxButtons.OK, false);
                ////创建起始时间
                //Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\chec", "CreateDateTime", P_dt_now, RegistryValueKind.String);

                ////创建上次时间
                //Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\chec", "LastDateTime", P_dt_now, RegistryValueKind.String);

                ////创建允许天数
                //Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\chec", "UseTimes", 120, RegistryValueKind.DWord);

                //goto again;
            }

            if ((P_dt_now - P_dt_create).Days < i_usetime)
            {
                Communal._b_isexpired = false;
                P_dt_create = P_dt_create.AddDays(i_usetime);
                string s_end = Convert.ToString(P_dt_create);
                s_end = s_end.Replace("/", "-");
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    this.toolStripLabel1.Text = "有效日期至:" + s_end;
                }
                else
                {
                    this.toolStripLabel1.Text = "ValidUntil:" + s_end;
                }
            }
            else
            {
                Communal._b_isexpired = true;
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    this.toolStripLabel1.Text = "软件过期,请联系供应商!";
                }
                else
                {
                    this.toolStripLabel1.Text = "The software has expired, please contact the supplier!";
                }
            }

        }
        private void Speech()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(1);
                    //查询所有播报信息

                    var sortedKeys = Lib_Card.CardObject.keyValuePairs.Keys.OrderByDescending(k => k);

                    //有报警
                    if(sortedKeys.Any())
                    {
                        bool b_true = false;
                        try
                        {
                            foreach (string i in Lib_Card.CardObject.keyValuePairs.Keys)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    if ((!Lib_Card.CardObject.keyValuePairs[i].Info.Contains("放布")) && (!Lib_Card.CardObject.keyValuePairs[i].Info.Contains("出布")))
                                        b_true = true;
                                }
                                else
                                {
                                    if ((!Lib_Card.CardObject.keyValuePairs[i].Info.Contains("cloth placement")) && (!Lib_Card.CardObject.keyValuePairs[i].Info.Contains("cup discharge")))
                                        b_true = true;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Lib_Log.Log.writeLogException("Main Speech：" + ex.ToString());
                        }
                        if (b_true)
                        {
                            if (!Communal._b_isBSendOpen)
                            {
                                Communal._i_bType = 1;
                            }
                        }
                        else
                        {
                            if (!Communal._b_isBSendClose)
                            {
                                Communal._i_bType = 2;
                            }
                        }
                    }
                    //无播报
                    else
                    {
                        if (!Communal._b_isBSendClose)
                        {
                            Communal._i_bType = 2;
                        }
                    }

                    foreach (var key in sortedKeys)
                    {
                        if (Lib_Card.CardObject.keyValuePairs.ContainsKey(key))
                        {
                            if (Lib_Card.CardObject.keyValuePairs[key].SpeechCount < Convert.ToInt32(s_SpeechCount) || Convert.ToInt32(s_SpeechCount) == 0)
                            {
                                SpeechSynthesizer speech = new SpeechSynthesizer();
                                speech.Rate = -1; //语速
                                speech.Volume = 100; //声音
                                speech.SelectVoice("Microsoft Huihui Desktop");//设置中文
                                string s_info = Lib_Card.CardObject.keyValuePairs[key].Info;
                                speech.Speak(s_info);
                                speech.Dispose();
                                //播报完后，把播报标志位改为true
                                if (!Lib_Card.CardObject.keyValuePairs[key].Speech)
                                {
                                    Lib_Card.CardObject.prompt prompt = new Lib_Card.CardObject.prompt();
                                    prompt = Lib_Card.CardObject.keyValuePairs[key];
                                    prompt.Speech = true;
                                    prompt.SpeechCount++;
                                    Lib_Card.CardObject.keyValuePairs[key] = prompt;
                                }
                                else
                                {
                                    if (Convert.ToInt32(s_SpeechCount) > 0)
                                    {
                                        Lib_Card.CardObject.prompt prompt = new Lib_Card.CardObject.prompt();
                                        prompt = Lib_Card.CardObject.keyValuePairs[key];
                                        prompt.Speech = true;
                                        prompt.SpeechCount++;
                                        Lib_Card.CardObject.keyValuePairs[key] = prompt;
                                    }
                                }
                            }
                            Thread.Sleep(200);
                        }
                    }


                }
                catch (Exception ex)
                {
                    Lib_Log.Log.writeLogException("Main Speech2：" + ex.ToString());
                }
            }
        }

        int _i_chooseTimes = 0;
        //自动选择待办事项
        private void AutoChoose()
        {
            while (true)
            {
                try
                {
                    //调试页面下不操作
                    if (!FADM_Object.Communal._b_auto)
                    {
                        //休眠60秒
                        Thread.Sleep(60000);
                        if (_i_chooseTimes % Communal._i_AskTimes == 0)
                        {

                        }
                        else
                        {
                            _i_chooseTimes++;
                        }
                    }
                    else
                    {
                        //5分钟执行一次
                        if (_i_chooseTimes % Communal._i_AskTimes == 0)
                        {
                            var sortedKeys = Lib_Card.CardObject.keyValuePairs.Keys.OrderByDescending(k => k);

                            DataTable dataTable = new DataTable();

                            if (sortedKeys.Any())
                            {
                                //查询30分钟内报警
                                string sql = "";
                                if (DateTime.Now.Hour == 0 && DateTime.Now.Minute < 30)
                                {
                                    TimeSpan ts = new TimeSpan(1, 0, 0, 0);
                                    DateTime dateTime = DateTime.Now - ts;
                                    sql = "select * from alarm_table where (MyDate = '" + dateTime.ToString("yyyy-MM-dd") + "' and MyTime >'23:30:00') or (MyDate = '" + dateTime.ToString("yyyy-MM-dd") + "')";
                                }
                                else
                                {
                                    TimeSpan ts = new TimeSpan(0, 0, 30, 0);
                                    DateTime dateTime = DateTime.Now - ts;
                                    sql = "select * from alarm_table where MyDate = '" + dateTime.ToString("yyyy-MM-dd") + "' and MyTime > '" + dateTime.ToString("HH:mm:ss") + "'";
                                }
                                //string sql = "select * from alarm_table where MyDate = ";
                                dataTable = FADM_Object.Communal._fadmSqlserver.GetData(sql);
                            }

                            foreach (var key in sortedKeys)
                            {
                                if (Lib_Card.CardObject.keyValuePairs.ContainsKey(key))
                                {
                                    //先判断是否是自动选择的待办事项，然后再判断次数
                                    if (Lib_Card.CardObject.keyValuePairs[key].Auto == 1)
                                    {
                                        int i_count = 0;
                                        foreach (DataRow dataRow in dataTable.Rows)
                                        {
                                            string s_temp = dataRow["AlarmDetails"].ToString();
                                            s_temp = s_temp.Replace("(Yes)", "");
                                            s_temp = s_temp.Replace("(No)", "");
                                            if (s_temp == Lib_Card.CardObject.keyValuePairs[key].Info)
                                            {
                                                i_count++;
                                            }
                                        }
                                        if (i_count < 3)
                                        {
                                            Lib_Card.CardObject.prompt prompt = new Lib_Card.CardObject.prompt();
                                            prompt = Lib_Card.CardObject.keyValuePairs[key];
                                            prompt.Choose = 1;
                                            Lib_Card.CardObject.keyValuePairs[key] = prompt;
                                        }
                                        else
                                        {
                                            Lib_Card.CardObject.prompt prompt = new Lib_Card.CardObject.prompt();
                                            prompt = Lib_Card.CardObject.keyValuePairs[key];
                                            prompt.Auto = 0;
                                            Lib_Card.CardObject.keyValuePairs[key] = prompt;
                                        }

                                    }
                                }
                            }
                        }
                        //休眠60秒
                        Thread.Sleep(60000);
                        _i_chooseTimes++;
                    }

                }
                catch (Exception ex)
                {
                    Lib_Log.Log.writeLogException("Main AutoChoose：" + ex.ToString());
                }
            }
        }

        public static int JudDyeingCode(string s_firstformulaCode, string s_firstver, string s_secondformulaCode, string s_secondver)
        {
            try
            {
                //先核对处理工艺步骤是否一致
                string s_sql = "SELECT  * FROM  dyeing_details where FormulaCode = '" + s_firstformulaCode + "' and VersionNum = " + s_firstver + " order by StepNum;";
                DataTable dt_data_first = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                s_sql = "SELECT  * FROM  dyeing_details where FormulaCode = '" + s_secondformulaCode + "' and VersionNum = " + s_secondver + " order by StepNum;";
                DataTable dt_data_second = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                //先判断数据量是否一致
                if (dt_data_first.Rows.Count != dt_data_second.Rows.Count)
                {
                    return -1;
                }
                else
                {
                    //核对每一步骤是否一致
                    for (int i = 0; i < dt_data_first.Rows.Count; i++)
                    {
                        //判断工艺是否一致
                        if (dt_data_first.Rows[i]["TechnologyName"].ToString() != dt_data_second.Rows[i]["TechnologyName"].ToString())
                        {
                            return -1;
                        }
                        else
                        {
                            if (dt_data_first.Rows[i]["TechnologyName"].ToString() == "温控" || dt_data_first.Rows[i]["TechnologyName"].ToString() == "Temperature control")
                            {
                                //温度，速率，时间不相等
                                if (Convert.ToDouble(dt_data_first.Rows[i]["Temp"].ToString()) != Convert.ToDouble(dt_data_second.Rows[i]["Temp"].ToString())
                                    || Convert.ToDouble(dt_data_first.Rows[i]["TempSpeed"].ToString()) != Convert.ToDouble(dt_data_second.Rows[i]["TempSpeed"].ToString())
                                    || Convert.ToDouble(dt_data_first.Rows[i]["Time"].ToString()) != Convert.ToDouble(dt_data_second.Rows[i]["Time"].ToString())
                                    )
                                {
                                    return -1;
                                }
                            }
                            else if (dt_data_first.Rows[i]["TechnologyName"].ToString() == "冷行" || dt_data_first.Rows[i]["TechnologyName"].ToString() == "洗杯" /*|| dt_data_first.Rows[i]["TechnologyName"].ToString() == "排液" */|| dt_data_first.Rows[i]["TechnologyName"].ToString() == "搅拌"
                                            || dt_data_first.Rows[i]["TechnologyName"].ToString() == "Cool line" || dt_data_first.Rows[i]["TechnologyName"].ToString() == "Wash the cup" /*|| dt_data_first.Rows[i]["TechnologyName"].ToString() == "Drainage"*/ || dt_data_first.Rows[i]["TechnologyName"].ToString() == "Stir")
                            {
                                //时间不等
                                if (Convert.ToDouble(dt_data_first.Rows[i]["Time"].ToString()) != Convert.ToDouble(dt_data_second.Rows[i]["Time"].ToString()))
                                {
                                    return -1;
                                }
                            }
                            else if ((dt_data_first.Rows[i]["TechnologyName"].ToString().Substring(0, 1) == "加" && dt_data_first.Rows[i]["TechnologyName"].ToString() != "加水" && dt_data_first.Rows[i]["TechnologyName"].ToString() != "加药")
                                            || (dt_data_first.Rows[i]["TechnologyName"].ToString() == "Add A" || dt_data_first.Rows[i]["TechnologyName"].ToString() == "Add B" || dt_data_first.Rows[i]["TechnologyName"].ToString() == "Add C" || dt_data_first.Rows[i]["TechnologyName"].ToString() == "Add D" || dt_data_first.Rows[i]["TechnologyName"].ToString() == "Add E"))
                            {
                                //查询对应加药条数
                                s_sql = "SELECT  * FROM  formula_handle_details where FormulaCode = '" + s_firstformulaCode + "' and VersionNum = " + s_firstver + " and Code = '" + dt_data_first.Rows[i]["Code"].ToString() + "' and TechnologyName = '" + dt_data_first.Rows[i]["TechnologyName"].ToString() + "';";
                                DataTable dt_data_first_handle = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                                s_sql = "SELECT  * FROM  formula_handle_details where FormulaCode = '" + s_secondformulaCode + "' and VersionNum = " + s_secondver + " and Code = '" + dt_data_second.Rows[i]["Code"].ToString() + "' and TechnologyName = '" + dt_data_second.Rows[i]["TechnologyName"].ToString() + "';";
                                DataTable dt_data_second_handle = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                                if (dt_data_first_handle.Rows.Count != dt_data_second_handle.Rows.Count)
                                {
                                    return -1;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.Message, "JudDyeingCode", MessageBoxButtons.OK, false);
                return -1;
            }

            return 0;
        }

        private void WaitList()
        {
            while (true)
            {
                try
                {
                    if (!FADM_Object.Communal._b_isOpenWaitList && (LabStatus.Text == "待机"|| LabStatus.Text == "Standby") && FADM_Object.Communal._b_finshRun && !FADM_Object.Communal._b_isDripping)
                    {
                        bool b_newInsert = false;
                        FADM_Object.Communal._b_isDripping = true;
                        //先查询空闲固定色杯号
                        string s_sqltemp = "SELECT  CupNum FROM cup_details WHERE   IsUsing = 0 and IsFixed = 1 and Statues = '待机' and enable = 1 and Type = 3 order by CupNum ;";
                        DataTable dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sqltemp);

                        foreach (DataRow Row in dt_temp.Rows)
                        {
                            string P_str_sqltemp1 = "SELECT  * FROM wait_list WHERE  CupNum = " + Row[0].ToString() + " and Type = 3  order by IndexNum;";
                            DataTable dataTabletemp1 = FADM_Object.Communal._fadmSqlserver.GetData(P_str_sqltemp1);

                            //查询是否在drop_head里存在该杯
                            string str_head = "SELECT  * FROM drop_head WHERE  CupNum = " + Row[0].ToString() + " ;";
                            DataTable dt_head = FADM_Object.Communal._fadmSqlserver.GetData(str_head);

                            if(dt_head.Rows.Count != 0)
                            {
                                continue;
                            }
                            foreach (DataRow Row1 in dataTabletemp1.Rows)
                            {
                                if (FADM_Object.Communal._b_isNeedConfirm)
                                {
                                    //判断另外一个杯子是否在用
                                    if (Communal._dic_first_second[Convert.ToInt32(Row[0].ToString())] > 0)
                                    {
                                        string str_head_s = "SELECT  * FROM drop_head WHERE  CupNum = " + Communal._dic_first_second[Convert.ToInt32(Row[0].ToString())] + "  ;";
                                        DataTable dt_head_s = FADM_Object.Communal._fadmSqlserver.GetData(str_head_s);
                                        if (dt_head_s.Rows.Count == 1)
                                        {
                                            string s_head_w = "SELECT  * FROM formula_head WHERE  FormulaCode = '" + Row1["FormulaCode"].ToString() + "' And  VersionNum ='" + Row1["VersionNum"].ToString() + "';";
                                            DataTable dt_head_w = FADM_Object.Communal._fadmSqlserver.GetData(s_head_w);

                                            //if (dt_head_s.Rows[0]["DyeingCode"].ToString() == dt_head_w.Rows[0]["DyeingCode"].ToString())
                                            if (JudDyeingCode(dt_head_s.Rows[0]["FormulaCode"].ToString(), dt_head_s.Rows[0]["VersionNum"].ToString(), dt_head_w.Rows[0]["FormulaCode"].ToString(), dt_head_w.Rows[0]["VersionNum"].ToString()) == 0)
                                            {
                                                if (dt_head_s.Rows[0]["BatchName"].ToString() == "0")
                                                {
                                                    b_newInsert = true;
                                                    //加入批次
                                                    AddDropList a = new AddDropList(dataTabletemp1.Rows[0]["FormulaCode"].ToString(), dataTabletemp1.Rows[0]["VersionNum"].ToString(), Row[0].ToString(), 3);
                                                    //删除等待列表记录
                                                    FADM_Object.Communal._fadmSqlserver.GetData("Delete from wait_list where Type = 3 and IndexNum = " + dataTabletemp1.Rows[0]["IndexNum"].ToString());
                                                    break;
                                                }
                                                else
                                                {
                                                    //当另外一个杯还是放布而且步号是1时可以加入
                                                    string s_cup_s = "SELECT  * FROM cup_details WHERE  CupNum = " + Communal._dic_first_second[Convert.ToInt32(Row[0].ToString())] + " ;";
                                                    DataTable dt_cup_s = FADM_Object.Communal._fadmSqlserver.GetData(s_cup_s);
                                                    if (dt_cup_s.Rows.Count > 0)
                                                    {
                                                        //当前工艺步骤为1时，可以加入
                                                        if (Convert.ToInt32(dt_cup_s.Rows[0]["StepNum"].ToString()) <= 1 && dt_cup_s.Rows[0]["TechnologyName"].ToString() == "放布")
                                                        {
                                                            b_newInsert = true;
                                                            //加入批次
                                                            AddDropList a = new AddDropList(dataTabletemp1.Rows[0]["FormulaCode"].ToString(), dataTabletemp1.Rows[0]["VersionNum"].ToString(), Row[0].ToString(), 3);
                                                            //删除等待列表记录
                                                            FADM_Object.Communal._fadmSqlserver.GetData("Delete from wait_list where Type = 3 and IndexNum = " + dataTabletemp1.Rows[0]["IndexNum"].ToString());
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        //另外一个杯子没在使用
                                        else
                                        {
                                            b_newInsert = true;
                                            //加入批次
                                            AddDropList a = new AddDropList(dataTabletemp1.Rows[0]["FormulaCode"].ToString(), dataTabletemp1.Rows[0]["VersionNum"].ToString(), Row[0].ToString(), 3);
                                            //删除等待列表记录
                                            FADM_Object.Communal._fadmSqlserver.GetData("Delete from wait_list where Type = 3 and IndexNum = " + dataTabletemp1.Rows[0]["IndexNum"].ToString());
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        b_newInsert = true;
                                        //加入批次
                                        AddDropList a = new AddDropList(dataTabletemp1.Rows[0]["FormulaCode"].ToString(), dataTabletemp1.Rows[0]["VersionNum"].ToString(), Row[0].ToString(), 3);
                                        //删除等待列表记录
                                        FADM_Object.Communal._fadmSqlserver.GetData("Delete from wait_list where Type = 3 and IndexNum = " + dataTabletemp1.Rows[0]["IndexNum"].ToString());

                                        break;
                                    }
                                }
                                else
                                {
                                    //判断另外一个杯子是否在用
                                    if (Communal._dic_first_second[Convert.ToInt32(Row[0].ToString())] > 0)
                                    {
                                        string str_head_s = "SELECT  * FROM drop_head WHERE  CupNum = " + Communal._dic_first_second[Convert.ToInt32(Row[0].ToString())] + "  ;";
                                        DataTable dt_head_s = FADM_Object.Communal._fadmSqlserver.GetData(str_head_s);
                                        if (dt_head_s.Rows.Count == 1)
                                        {
                                            if (dt_head_s.Rows[0]["BatchName"].ToString() == "0")
                                            {
                                                string s_head_w = "SELECT  * FROM formula_head WHERE  FormulaCode = '" + Row1["FormulaCode"].ToString() + "' And  VersionNum ='" + Row1["VersionNum"].ToString() + "';";
                                                DataTable dt_head_w = FADM_Object.Communal._fadmSqlserver.GetData(s_head_w);

                                                //if (dt_head_s.Rows[0]["DyeingCode"].ToString() == dt_head_w.Rows[0]["DyeingCode"].ToString())
                                                if (JudDyeingCode(dt_head_s.Rows[0]["FormulaCode"].ToString(), dt_head_s.Rows[0]["VersionNum"].ToString(), dt_head_w.Rows[0]["FormulaCode"].ToString(), dt_head_w.Rows[0]["VersionNum"].ToString()) == 0)
                                                {
                                                    b_newInsert = true;
                                                    //加入批次
                                                    AddDropList a = new AddDropList(dataTabletemp1.Rows[0]["FormulaCode"].ToString(), dataTabletemp1.Rows[0]["VersionNum"].ToString(), Row[0].ToString(), 3);
                                                    //删除等待列表记录
                                                    FADM_Object.Communal._fadmSqlserver.GetData("Delete from wait_list where Type = 3 and IndexNum = " + dataTabletemp1.Rows[0]["IndexNum"].ToString());
                                                    break;
                                                }
                                            }
                                        }
                                        //另外一个杯没在使用
                                        else
                                        {
                                            b_newInsert = true;
                                            //加入批次
                                            AddDropList a = new AddDropList(dataTabletemp1.Rows[0]["FormulaCode"].ToString(), dataTabletemp1.Rows[0]["VersionNum"].ToString(), Row[0].ToString(), 3);
                                            //删除等待列表记录
                                            FADM_Object.Communal._fadmSqlserver.GetData("Delete from wait_list where Type = 3 and IndexNum = " + dataTabletemp1.Rows[0]["IndexNum"].ToString());

                                            break;
                                        }
                                    }
                                    else
                                    {
                                        b_newInsert = true;
                                        //加入批次
                                        AddDropList a = new AddDropList(dataTabletemp1.Rows[0]["FormulaCode"].ToString(), dataTabletemp1.Rows[0]["VersionNum"].ToString(), Row[0].ToString(), 3);
                                        //删除等待列表记录
                                        FADM_Object.Communal._fadmSqlserver.GetData("Delete from wait_list where Type = 3 and IndexNum = " + dataTabletemp1.Rows[0]["IndexNum"].ToString());

                                        break;
                                    }
                                }
                            }
                        }

                        //查询空闲自动分配杯号（后处理类型）
                        s_sqltemp = "SELECT  CupNum FROM cup_details WHERE   IsUsing = 0 and IsFixed = 0 and Statues = '待机' and enable = 1 and Type = 3 order by CupNum ;";
                        dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sqltemp);

                        

                        int i_num = 0;
                        foreach (DataRow Row in dt_temp.Rows)
                        {
                            //查询是否在drop_head里存在该杯
                            string s_head = "SELECT  * FROM drop_head WHERE  CupNum = " + Row[0].ToString() + " ;";
                            DataTable dt_head = FADM_Object.Communal._fadmSqlserver.GetData(s_head);

                            string s_sqltemp2 = "SELECT  * FROM wait_list WHERE  CupNum = 0 and Type = 3  order by IndexNum;";
                            DataTable dt_temp2 = FADM_Object.Communal._fadmSqlserver.GetData(s_sqltemp2);

                            if (dt_temp2.Rows.Count == 0)
                                break;

                            foreach (DataRow Row1 in dt_temp2.Rows)
                            {
                                //查询配方对应染固色代码
                                string s_head_w = "SELECT  * FROM formula_head WHERE  FormulaCode = '" + Row1["FormulaCode"].ToString() + "' And  VersionNum ='" + Row1["VersionNum"].ToString() + "';";
                                DataTable dt_head_w = FADM_Object.Communal._fadmSqlserver.GetData(s_head_w);

                                //判断是否双杯
                                if (Communal._dic_first_second[Convert.ToInt32(Row[0].ToString())] > 0)
                                {
                                    string str_head_s = "SELECT  * FROM drop_head WHERE  CupNum = " + Communal._dic_first_second[Convert.ToInt32(Row[0].ToString())] + "  ;";
                                    DataTable dt_head_s = FADM_Object.Communal._fadmSqlserver.GetData(str_head_s);
                                    if(dt_head_s.Rows.Count>0 && dt_head_w.Rows.Count>0)
                                    {
                                        //判断是否染固色代码一致
                                        //if (dt_head_s.Rows[0]["DyeingCode"].ToString() == dt_head_w.Rows[0]["DyeingCode"].ToString())
                                            if(JudDyeingCode(dt_head_s.Rows[0]["FormulaCode"].ToString(), dt_head_s.Rows[0]["VersionNum"].ToString(), dt_head_w.Rows[0]["FormulaCode"].ToString(), dt_head_w.Rows[0]["VersionNum"].ToString()) ==0)
                                        {
                                            if (dt_head_s.Rows[0]["BatchName"].ToString() == "0")
                                            {
                                                b_newInsert = true;
                                                //加入批次
                                                AddDropList a = new AddDropList(Row1["FormulaCode"].ToString(), Row1["VersionNum"].ToString(), Row[0].ToString(), 3);
                                                //删除等待列表记录
                                                FADM_Object.Communal._fadmSqlserver.GetData("Delete from wait_list where Type = 3 and IndexNum = " + Row1["IndexNum"].ToString());

                                                break;
                                            }
                                            else
                                            {
                                                if (FADM_Object.Communal._b_isNeedConfirm)
                                                {
                                                    //当另外一个杯还是放布而且步号是1时可以加入
                                                    string s_cup_s = "SELECT  * FROM cup_details WHERE  CupNum = " + Communal._dic_first_second[Convert.ToInt32(Row[0].ToString())] + " ;";
                                                    DataTable dt_cup_s = FADM_Object.Communal._fadmSqlserver.GetData(s_cup_s);
                                                    if (dt_cup_s.Rows.Count > 0)
                                                    {
                                                        //当前工艺步骤为1时，可以加入
                                                        if (Convert.ToInt32(dt_cup_s.Rows[0]["StepNum"].ToString()) <= 1 && dt_cup_s.Rows[0]["TechnologyName"].ToString() == "放布")
                                                        {
                                                            b_newInsert = true;
                                                            //加入批次
                                                            AddDropList a = new AddDropList(Row1["FormulaCode"].ToString(), Row1["VersionNum"].ToString(), Row[0].ToString(), 3);
                                                            //删除等待列表记录
                                                            FADM_Object.Communal._fadmSqlserver.GetData("Delete from wait_list where Type = 3 and IndexNum = " + Row1["IndexNum"].ToString());
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        b_newInsert = true;
                                        //加入批次
                                        AddDropList a = new AddDropList(Row1["FormulaCode"].ToString(), Row1["VersionNum"].ToString(), Row[0].ToString(), 3);
                                        //删除等待列表记录
                                        FADM_Object.Communal._fadmSqlserver.GetData("Delete from wait_list where Type = 3 and IndexNum = " + Row1["IndexNum"].ToString());

                                        break;
                                    }

                                }
                                else
                                {
                                    b_newInsert = true;
                                    //加入批次
                                    AddDropList a = new AddDropList(Row1["FormulaCode"].ToString(), Row1["VersionNum"].ToString(), Row[0].ToString(), 3);
                                    //删除等待列表记录
                                    FADM_Object.Communal._fadmSqlserver.GetData("Delete from wait_list where Type = 3 and IndexNum = " + Row1["IndexNum"].ToString());

                                    break;
                                }
                            }

                        }
                        bool b_type2 = false;
                        if (FADM_Object.Communal._b_isDripNeedCupNum)
                        {
                            //查询空闲自动分配杯号（滴液类型）
                            s_sqltemp = "SELECT  CupNum FROM cup_details WHERE   IsUsing = 0 and  enable = 1 and Type = 2 order by CupNum ;";
                            dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sqltemp);

                            

                            int i_num2 = 0;
                            foreach (DataRow Row in dt_temp.Rows)
                            {
                                string s_sqltemp3 = "SELECT  * FROM wait_list WHERE  Type = 2 and CupNum = " + Row[0].ToString()+" order by IndexNum;";
                                DataTable dt_temp3 = FADM_Object.Communal._fadmSqlserver.GetData(s_sqltemp3);

                                //查询是否在drop_head里存在该杯
                                string s_head = "SELECT  * FROM drop_head WHERE  CupNum = " + Row[0].ToString() + " ;";
                                DataTable dt_head = FADM_Object.Communal._fadmSqlserver.GetData(s_head);
                                if (dt_temp3.Rows.Count > 0)
                                {
                                    if (dt_head.Rows.Count == 0)
                                    {
                                        b_newInsert = true;
                                        //加入批次
                                        AddDropList a = new AddDropList(dt_temp3.Rows[0]["FormulaCode"].ToString(), dt_temp3.Rows[0]["VersionNum"].ToString(), Row[0].ToString(), 2);
                                        //删除等待列表记录
                                        FADM_Object.Communal._fadmSqlserver.GetData("Delete from wait_list where Type = 2 and IndexNum = " + dt_temp3.Rows[0]["IndexNum"].ToString());

                                        i_num2++;
                                        b_type2 = true;
                                    }
                                }

                            }
                        }
                        else
                        {
                            //查询空闲自动分配杯号（滴液类型）
                            s_sqltemp = "SELECT  CupNum FROM cup_details WHERE   IsUsing = 0 and  enable = 1 and Type = 2 order by CupNum ;";
                            dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sqltemp);

                            string s_sqltemp3 = "SELECT  * FROM wait_list WHERE  Type = 2  order by IndexNum;";
                            DataTable dt_temp3 = FADM_Object.Communal._fadmSqlserver.GetData(s_sqltemp3);

                            int i_num2 = 0;
                            
                            foreach (DataRow Row in dt_temp.Rows)
                            {
                                if (dt_temp3.Rows.Count == 0)
                                    break;
                                //查询是否在drop_head里存在该杯
                                string s_head = "SELECT  * FROM drop_head WHERE  CupNum = " + Row[0].ToString() + " ;";
                                DataTable dt_head = FADM_Object.Communal._fadmSqlserver.GetData(s_head);

                                if (dt_temp3.Rows.Count > i_num2 && dt_head.Rows.Count == 0)
                                {
                                    b_newInsert = true;
                                    //加入批次
                                    AddDropList a = new AddDropList(dt_temp3.Rows[i_num2]["FormulaCode"].ToString(), dt_temp3.Rows[i_num2]["VersionNum"].ToString(), Row[0].ToString(), 2);
                                    //删除等待列表记录
                                    FADM_Object.Communal._fadmSqlserver.GetData("Delete from wait_list where Type = 2 and IndexNum = " + dt_temp3.Rows[i_num2]["IndexNum"].ToString());

                                    i_num2++;
                                    b_type2 = true;
                                }

                            }
                        }
                        bool b_p=false;
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        {
                            if (!toolStripLabel1.Text.Contains("过期") && !toolStripLabel1.Text.Contains("修改"))
                            {
                                b_p = true;
                            }
                        }
                        else
                        {
                            if (!toolStripLabel1.Text.Contains("expired") && !toolStripLabel1.Text.Contains("modify"))
                            {
                                b_p = true;
                            }
                        }

                        if (b_p && FADM_Object.Communal._b_isUseAuto)
                        {
                            try
                            {
                                string s_sql2 = "SELECT * FROM drop_head Where BatchName = '0' and Stage = '后处理';";
                                DataTable dt_data2 = FADM_Object.Communal._fadmSqlserver.GetData(s_sql2);
                                if (/*newInsert || */dt_data2.Rows.Count > 0)
                                {
                                    while (true)
                                    {
                                        if (LabStatus.Text == "待机" || LabStatus.Text == "Standby")
                                        {
                                            break;
                                        }
                                        Thread.Sleep(1);
                                    }
                                    if (LabStatus.Text == "待机" || LabStatus.Text == "Standby")
                                    {

                                        //获取之前批次号
                                        string s_sql = "SELECT BatchName FROM enabled_set Where MyID = 1;";
                                        DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                                        string s_batchNum_last = Convert.ToString(dt_data.Rows[0][dt_data.Columns[0]]);

                                        //计算当前批次号
                                        string s_batchNum = null;
                                        if (s_batchNum_last == "0")
                                        {
                                            //初始状态
                                            int i_no = 1;
                                            s_batchNum = DateTime.Now.ToString("yyyyMMdd") + i_no.ToString("d4");
                                        }
                                        else
                                        {
                                            string s_date = s_batchNum_last.Substring(0, 8);
                                            string s_no = s_batchNum_last.Substring(8, 4);

                                            if (s_date == DateTime.Now.ToString("yyyyMMdd"))
                                            {
                                                s_batchNum = s_date + (Convert.ToInt32(s_no) + 1).ToString("d4");
                                            }
                                            else
                                            {
                                                int i_no = 1;
                                                s_batchNum = DateTime.Now.ToString("yyyyMMdd") + i_no.ToString("d4");
                                            }
                                        }

                                        //修改当前批次号
                                        s_sql = "UPDATE enabled_set SET BatchName = '" + s_batchNum + "'" +
                                                    " WHERE MyID = 1;";
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                        //查询修改该配方为已滴定配方
                                        s_sql = "SELECT * FROM drop_head Where BatchName = '0'  and Stage = '后处理';";
                                        dt_data = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                                        List<int> lis_cup=new List<int>();
                                        foreach (DataRow dr in dt_data.Rows)
                                        {
                                            lis_cup.Add(Convert.ToInt32(dr["CupNum"]));
                                        }
                                        foreach (DataRow dr in dt_data.Rows)
                                        {

                                            if (FADM_Object.Communal._b_isNeedConfirm)
                                            {
                                                if (Communal._dic_first_second[Convert.ToInt32(dr["CupNum"])] > 0)
                                                {
                                                    //先判断另外一个杯有没在使用
                                                    s_sql = "SELECT * FROM drop_head Where CupNum = " + Communal._dic_first_second[Convert.ToInt32(dr["CupNum"])] + " And BatchName !='0';";
                                                    DataTable dt_drop_head_1 = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                                                    if (dt_drop_head_1.Rows.Count > 0)
                                                    {
                                                        string s_cup_s = "SELECT  * FROM cup_details WHERE  CupNum = " + Communal._dic_first_second[Convert.ToInt32(dr["CupNum"])] + " ;";
                                                        DataTable dt_cup_s = FADM_Object.Communal._fadmSqlserver.GetData(s_cup_s);
                                                        if (dt_cup_s.Rows.Count > 0)
                                                        {
                                                            //判断是否一样批次号
                                                            if (dt_drop_head_1.Rows[0]["BatchName"].ToString() == s_batchNum)
                                                            {

                                                            }
                                                            //不是一样批次号才判断步数
                                                            else
                                                            {
                                                                //当前工艺步骤为1时，或者批次号一致可以加入
                                                                if ((Convert.ToInt32(dt_cup_s.Rows[0]["StepNum"].ToString()) <= 1 && dt_cup_s.Rows[0]["TechnologyName"].ToString() == "放布"))
                                                                {
                                                                }
                                                                else
                                                                {
                                                                    //停止中或步号不为1
                                                                    if (Convert.ToInt32(dt_cup_s.Rows[0]["StepNum"].ToString()) > 1 || dt_cup_s.Rows[0]["Statues"].ToString() == "停止中")
                                                                    {
                                                                        continue;
                                                                    }
                                                                }
                                                            }

                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //把双杯的都判断一下，如果两个杯都有配方，就一起滴定，如果只有一个杯有配方就不自动开始
                                                if (Communal._dic_first_second[Convert.ToInt32(dr["CupNum"])] > 0)
                                                {
                                                    if (!lis_cup.Contains(Communal._dic_first_second[Convert.ToInt32(dr["CupNum"])]))
                                                    {
                                                        continue;
                                                    }

                                                }
                                            }
                                            //写入配方浏览表
                                            s_sql = "UPDATE formula_head SET State = '已滴定配方'" +
                                                       " WHERE FormulaCode = '" + dr["FormulaCode"] + "' AND" +
                                                       " VersionNum = " + dr["VersionNum"] + " ;";
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                            //修改批次表头批次号
                                            s_sql = "UPDATE drop_head SET BatchName = '" + s_batchNum + "', State = '已滴定配方',Step = 1 where  BatchName = '0' and CupNum = '" + dr["CupNum"] + "';";
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                            //修改批次详细资料表批次号
                                            s_sql = "UPDATE drop_details SET BatchName = '" + s_batchNum + "' where  BatchName = '0' and CupNum = '" + dr["CupNum"] + "';";
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                            s_sql = "UPDATE dye_details SET BatchName = '" + s_batchNum + "' where  BatchName = '0' and CupNum = '" + dr["CupNum"] + "';";
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                        }

                                        s_sql = "SELECT * FROM drop_head Where BatchName = '"+ s_batchNum+"';";
                                        DataTable dt_data1 = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                                        if (dt_data1.Rows.Count > 0)
                                        {
                                            //修改缸位

                                            foreach (DataRow dr in dt_data1.Rows)
                                            {
                                                if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(Convert.ToInt32(dr["CupNum"])))
                                                {
                                                    //查询当天当前杯位打了多少次
                                                    s_sql = "SELECT * FROM history_head where CupNum = '" + dr["CupNum"] + "' And BatchName > '" + s_batchNum.Substring(0, 8) + "' order by StartTime desc;";
                                                    DataTable dt_his = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                                                    if (dt_his.Rows.Count > 0)
                                                    {
                                                        string s = null;
                                                        if (dt_his.Rows[0]["VatNumber"] is DBNull)
                                                        {
                                                            s = FADM_Object.Communal._s_MachineNumber + "-" + s_batchNum.Substring(2, 6) + Convert.ToInt32(dr["CupNum"]).ToString("D2") + (dt_his.Rows.Count + 1).ToString("D");
                                                        }
                                                        else
                                                        {
                                                            string input = dt_his.Rows[0]["VatNumber"].ToString();
                                                            input = input.Substring(input.Length - 1, 1);
                                                            s = FADM_Object.Communal._s_MachineNumber + "-" + s_batchNum.Substring(2, 6) + Convert.ToInt32(dr["CupNum"]).ToString("D2") + (Convert.ToInt32(input) + 1).ToString("D");
                                                        }
                                                        //写入数据库
                                                        FADM_Object.Communal._fadmSqlserver.ReviseData("Update drop_head  set VatNumber = '" + s + "' where CupNum = '" + dr["CupNum"] + "';");
                                                    }
                                                    else
                                                    {
                                                        string s = FADM_Object.Communal._s_MachineNumber + "-" + s_batchNum.Substring(2, 6) + Convert.ToInt32(dr["CupNum"]).ToString("D2") + "1";
                                                        //写入数据库
                                                        FADM_Object.Communal._fadmSqlserver.ReviseData("Update drop_head  set VatNumber = '" + s + "' where CupNum = '" + dr["CupNum"] + "';");
                                                    }
                                                }
                                            }

                                            //查询是否有实际开始的，再调用开始滴液
                                            Thread thread = new Thread(() =>
                                            {
                                                new FADM_Auto.Drip().DripLiquid(s_batchNum);
                                                FADM_Control.Formula.P_bl_update = true;
                                            });
                                            thread.Start();

                                            //等待进去滴液，需要状态变成不是待机才继续,因为滴液是线程启动，有可能刚好手动点击开始滴液，导致两个线程同时执行
                                            while (true)
                                            {
                                                if (FADM_Object.Communal.ReadMachineStatus() != 0)
                                                    break;
                                                Thread.Sleep(10);
                                            }
                                        }
                                        else
                                        {
                                            //重新把序号减回去
                                            //获取之前批次号
                                            string s_sql_2 = "SELECT BatchName FROM enabled_set Where MyID = 1;";
                                            DataTable dt_data_2 = FADM_Object.Communal._fadmSqlserver.GetData(s_sql_2);
                                            string s_batchNum_last2 = Convert.ToString(dt_data_2.Rows[0][dt_data_2.Columns[0]]);

                                            //计算当前批次号
                                            string s_batchNum2 = null;
                                            if (s_batchNum_last2 == "0")
                                            {
                                                //初始状态
                                                int i_no = 1;
                                                s_batchNum2 = DateTime.Now.ToString("yyyyMMdd") + i_no.ToString("d4");
                                            }
                                            else
                                            {
                                                string s_date = s_batchNum_last2.Substring(0, 8);
                                                string s_no = s_batchNum_last2.Substring(8, 4);

                                                if (s_date == DateTime.Now.ToString("yyyyMMdd"))
                                                {
                                                    s_batchNum2 = s_date + (Convert.ToInt32(s_no) - 1).ToString("d4");
                                                }
                                                else
                                                {
                                                    int i_no = 1;
                                                    s_batchNum2 = DateTime.Now.ToString("yyyyMMdd") + i_no.ToString("d4");
                                                }
                                            }

                                            //修改当前批次号
                                            s_sql = "UPDATE enabled_set SET BatchName = '" + s_batchNum2 + "'" +
                                                        " WHERE MyID = 1;";
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                        }
                                        b_newInsert = false;
                                        FADM_Control.Formula._b_updateWait = true;
                                        FADM_Object.Communal._b_isDripping = false;
                                    }
                                }

                                if (b_type2)
                                {
                                    FADM_Control.Formula._b_updateWait = true;
                                }
                                FADM_Object.Communal._b_isDripping = false;

                            }
                            catch
                            {
                                FADM_Object.Communal._b_isDripping = false;
                            }
                        }
                        else
                        {
                            FADM_Control.Formula._b_updateWait = true;
                            FADM_Object.Communal._b_isDripping = false;
                        }
                    }



                    //1分钟一次
                    Thread.Sleep(60000);
                }
                catch
                {
                    FADM_Object.Communal._b_isDripping = false;
                }
            }
        }

        private void InsertAbs()
        {
            while (true)
            {
                try
                {

                    //查询是否有等待数据
                    string s_sql = "SELECT top 1 * FROM abs_wait_list  order by InsertDate;";
                    DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                    if (dt_data.Rows.Count > 0)
                    {
                        int _i_nBottleNum = Convert.ToInt32(dt_data.Rows[0]["BottleNum"]);
                        //1.查看是否有空闲杯子
                        if (_i_nBottleNum == 888)
                            s_sql = "SELECT * FROM abs_cup_details WHERE  Enable = 1 And IsUsing=0  And CupNum = 2  order by CupNum;";
                        else if (_i_nBottleNum == 999)
                            s_sql = "SELECT * FROM abs_cup_details WHERE  Enable = 1 And IsUsing=0  And CupNum = 4  order by CupNum;";
                        else

                            s_sql = "SELECT * FROM abs_cup_details WHERE  Enable = 1 And IsUsing=0  And (CupNum = 2 Or CupNum = 4) order by CupNum;";
                        DataTable dt_abs_cup_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                        if (dt_abs_cup_details.Rows.Count == 0)
                        {
                            goto lab_end;
                        }
                        else
                        {
                            int i_index = 0;
                            string s_cupNum = "";
                        lab_re:
                            if (i_index == dt_abs_cup_details.Rows.Count)
                            {
                                goto lab_end;
                            }
                            //判断当前工位是否待机(循环查找实际待机杯号)
                            if (MyAbsorbance._abs_Temps[Convert.ToInt32(dt_abs_cup_details.Rows[i_index]["CupNum"]) - 1]._s_currentState != "1")
                            {
                                i_index += 1;
                                goto lab_re;
                            }
                            else
                            {
                                //记录选择杯号
                                s_cupNum = dt_abs_cup_details.Rows[i_index]["CupNum"].ToString();
                            }

                            //2.计算需要添加的用量并保存到表_i_nBottleNum
                            s_sql = "SELECT bottle_details.*,assistant_details.AllowMinColoringConcentration,assistant_details.AllowMaxColoringConcentration,assistant_details.AssistantType,assistant_details.UnitOfAccount  FROM bottle_details left join assistant_details on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE bottle_details.BottleNum = " + _i_nBottleNum + ";";
                            DataTable dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                            string s_unitOfAccount = dt_temp.Rows[0]["UnitOfAccount"].ToString();
                            string s_assistantType = dt_temp.Rows[0]["AssistantType"].ToString();
                            string s_realConcentration = dt_temp.Rows[0]["RealConcentration"].ToString();
                            string s_settingConcentration = dt_temp.Rows[0]["SettingConcentration"].ToString();
                            string s_compensate = dt_temp.Rows[0]["Compensate"].ToString() == "" ? "0" : dt_temp.Rows[0]["Compensate"].ToString();
                            //if(Convert.ToDouble(s_settingConcentration) < 0.05)
                            //{
                            //    FADM_Form.CustomMessageBox.Show("浓度太小，不能测试", "TestAbs",
                            //    MessageBoxButtons.OK, false);
                            //    return;
                            //}
                            if (s_unitOfAccount != "%")
                            {
                                //FADM_Form.CustomMessageBox.Show("不是染料，不能测试", "TestAbs",
                                //MessageBoxButtons.OK, false);
                                //return;
                                FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from abs_wait_list where BottleNum = " + _i_nBottleNum + " And InsertDate = '" + Convert.ToDateTime(dt_data.Rows[0]["InsertDate"]).ToString("yyyy-MM-dd HH:mm:ss.fff") + "';");
                                goto lab_end;
                            }
                            else
                            {
                                //需要洗杯
                                if (MyAbsorbance._abs_Temps[Convert.ToInt32(s_cupNum) - 1]._s_history == "1")
                                {
                                    s_sql = "UPDATE abs_cup_details SET Statues='洗杯',IsUsing = 1,Type=0  WHERE CupNum = " + s_cupNum + " ;";
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                    //生成洗杯工艺
                                    SmartDyeing.FADM_Auto.MyAbsorbance.Generate(2, Convert.ToInt32(s_cupNum));
                                    SmartDyeing.FADM_Auto.MyAbsorbance.SendData(Convert.ToInt32(s_cupNum));

                                    goto lab_end;
                                }

                                ////查看纯净水检测是否超过30分钟
                                //s_sql = "SELECT *  FROM standard where Type = 1;";

                                //dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                                //if (dt_temp.Rows.Count == 0 || MyAbsorbance._i_block == 1)
                                //{
                                //    //    FADM_Form.CustomMessageBox.Show("不存在标准记录，先测试标准样", "TestAbs",
                                //    //MessageBoxButtons.OK, false);

                                //    //DialogResult dialogResult;
                                //    //if (MyAbsorbance._i_block == 1)
                                //    //{
                                //    //    dialogResult = FADM_Form.CustomMessageBox.Show("断电重启，先测试基准样，请选择测试基准点母液(选择溶解剂请点是，选择水请点否)", "温馨提示", MessageBoxButtons.YesNo, true);
                                //    //}
                                //    //else
                                //    //{
                                //    //    dialogResult = FADM_Form.CustomMessageBox.Show("不存在标准记录，先测试基准样，请选择测试基准点母液(选择溶解剂请点是，选择水请点否)", "温馨提示", MessageBoxButtons.YesNo, true);
                                //    //}
                                //    //if (dialogResult == DialogResult.Yes)
                                //    //{
                                //    //    //找到DNF溶解剂
                                //    //    s_sql = "SELECT bottle_details.*  FROM bottle_details left join assistant_details on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE assistant_details.UnitOfAccount collate Chinese_PRC_CS_AS = 'G/L';";
                                //    //}
                                //    //else if (dialogResult == DialogResult.No)
                                //    //{
                                //    //    //找到水
                                //    //    s_sql = "SELECT bottle_details.*  FROM bottle_details left join assistant_details on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE assistant_details.UnitOfAccount = 'Water';";
                                //    //}
                                //    //else
                                //    //{
                                //    //    goto lab_end;
                                //    //}

                                //    if (FADM_Object.Communal._b_isUseWaterTestBase)
                                //    {
                                //        //找到水
                                //        s_sql = "SELECT bottle_details.*  FROM bottle_details left join assistant_details on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE assistant_details.UnitOfAccount = 'Water';";
                                //    }
                                //    else
                                //    {
                                //        //找到DNF溶解剂
                                //        s_sql = "SELECT bottle_details.*  FROM bottle_details left join assistant_details on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE assistant_details.UnitOfAccount collate Chinese_PRC_CS_AS = 'G/L';";
                                //    }

                                //    dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                                //    if (dt_temp.Rows.Count == 0)
                                //    {
                                //        FADM_Form.CustomMessageBox.Show("不存在母液瓶号，不能测试", "TestAbs",
                                //MessageBoxButtons.OK, false);
                                //        return;
                                //    }
                                //    else
                                //    {
                                //        if (FADM_Object.Communal._fadmSqlserver.GetData("select * from standard where Type = 1").Rows.Count > 0)
                                //            FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from standard where Type = 0");
                                //        FADM_Object.Communal._fadmSqlserver.ReviseData("Update standard set Type = 0 where Type = 1");

                                //        //判断是否一样的溶解剂或水
                                //        //if (dataTable.Rows[0]["AdditivesNum"].ToString() != dt_temp.Rows[0]["BottleNum"].ToString())
                                //        {
                                //            //测试样
                                //            if (Convert.ToInt32(dt_data.Rows[0]["Type"]) == 0)
                                //            {

                                //                SmartDyeing.FADM_Auto.MyAbsorbance.Calculate(_i_nBottleNum, Convert.ToInt32(dt_temp.Rows[0]["BottleNum"].ToString()), Convert
                                //        .ToInt32(s_cupNum), 2, FADM_Object.Communal._d_abs_total);
                                //            }
                                //            //测试补偿
                                //            else
                                //            {

                                //                SmartDyeing.FADM_Auto.MyAbsorbance.Calculate(_i_nBottleNum, Convert.ToInt32(dt_temp.Rows[0]["BottleNum"].ToString()), Convert
                                //        .ToInt32(s_cupNum), 9, FADM_Object.Communal._d_abs_total);
                                //            }

                                //            //生成测量
                                //            SmartDyeing.FADM_Auto.MyAbsorbance.Generate(0, Convert.ToInt32(s_cupNum));
                                //            SmartDyeing.FADM_Auto.MyAbsorbance.SendData(Convert.ToInt32(s_cupNum));

                                //            FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from abs_wait_list where BottleNum = " + _i_nBottleNum + " And InsertDate = '" + Convert.ToDateTime(dt_data.Rows[0]["InsertDate"]).ToString("yyyy-MM-dd HH:mm:ss.fff") + "';");



                                //            goto lab_end;
                                //        }
                                //    }
                                //}
                                //else
                                //{
                                //    DateTime timeA = Convert.ToDateTime(dt_temp.Rows[0]["FinishTime"].ToString());
                                //    DateTime timeB = DateTime.Now; //获取当前时间
                                //    TimeSpan ts = timeB - timeA; //计算时间差
                                //    string s_time = ts.TotalMinutes.ToString(); //将时间差转换为小时

                                //    if (Convert.ToDouble(s_time) > FADM_Object.Communal._d_TestSpan)
                                //    {
                                //        //        FADM_Form.CustomMessageBox.Show("标准记录已超期，先测试标准样", "TestAbs",
                                //        //MessageBoxButtons.OK, false);
                                //        //        //找到母液溶解剂母液瓶号
                                //        //        s_sql = "SELECT bottle_details.*  FROM bottle_details left join assistant_details on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE assistant_details.UnitOfAccount collate Chinese_PRC_CS_AS = 'G/L';";

                                //        //DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("基准记录已超期，先测试基准样，请选择测试基准点母液(选择溶解剂请点是，选择水请点否)", "温馨提示", MessageBoxButtons.YesNo, true);
                                //        //if (dialogResult == DialogResult.Yes)
                                //        //{
                                //        //    //找到DNF溶解剂
                                //        //    s_sql = "SELECT bottle_details.*  FROM bottle_details left join assistant_details on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE assistant_details.UnitOfAccount collate Chinese_PRC_CS_AS = 'G/L';";
                                //        //}
                                //        //else if (dialogResult == DialogResult.No)
                                //        //{
                                //        //    //找到水
                                //        //    s_sql = "SELECT bottle_details.*  FROM bottle_details left join assistant_details on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE assistant_details.UnitOfAccount = 'Water';";
                                //        //}
                                //        //else
                                //        //{
                                //        //    goto lab_end;
                                //        //}

                                //        if (FADM_Object.Communal._b_isUseWaterTestBase)
                                //        {
                                //            //找到水
                                //            s_sql = "SELECT bottle_details.*  FROM bottle_details left join assistant_details on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE assistant_details.UnitOfAccount = 'Water';";
                                //        }
                                //        else
                                //        {
                                //            //找到DNF溶解剂
                                //            s_sql = "SELECT bottle_details.*  FROM bottle_details left join assistant_details on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE assistant_details.UnitOfAccount collate Chinese_PRC_CS_AS = 'G/L';";
                                //        }

                                //        dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                                //        if (dt_temp.Rows.Count == 0)
                                //        {
                                //            FADM_Form.CustomMessageBox.Show("不存在母液瓶号，不能测试", "TestAbs",
                                //    MessageBoxButtons.OK, false);
                                //            return;
                                //        }
                                //        else
                                //        {
                                //            if (FADM_Object.Communal._fadmSqlserver.GetData("select * from standard where Type = 1").Rows.Count > 0)
                                //                FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from standard where Type = 0");
                                //            FADM_Object.Communal._fadmSqlserver.ReviseData("Update standard set Type = 0 where Type = 1");
                                //            //判断是否一样的溶解剂或水
                                //            //if (dataTable.Rows[0]["AdditivesNum"].ToString() != dt_temp.Rows[0]["BottleNum"].ToString())
                                //            {
                                //                if (Convert.ToInt32(dt_data.Rows[0]["Type"]) == 0)
                                //                {

                                //                    SmartDyeing.FADM_Auto.MyAbsorbance.Calculate(_i_nBottleNum, Convert.ToInt32(dt_temp.Rows[0]["BottleNum"].ToString()), Convert
                                //        .ToInt32(s_cupNum), 2, FADM_Object.Communal._d_abs_total);
                                //                }
                                //                else
                                //                {
                                //                    SmartDyeing.FADM_Auto.MyAbsorbance.Calculate(_i_nBottleNum, Convert.ToInt32(dt_temp.Rows[0]["BottleNum"].ToString()), Convert
                                //        .ToInt32(s_cupNum), 9, FADM_Object.Communal._d_abs_total);
                                //                }

                                //                SmartDyeing.FADM_Auto.MyAbsorbance.Generate(0, Convert.ToInt32(s_cupNum));
                                //                SmartDyeing.FADM_Auto.MyAbsorbance.SendData(Convert.ToInt32(s_cupNum));

                                //                FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from abs_wait_list where BottleNum = " + _i_nBottleNum + " And InsertDate = '" + Convert.ToDateTime(dt_data.Rows[0]["InsertDate"]).ToString("yyyy-MM-dd HH:mm:ss.fff") + "';");
                                //                goto lab_end;
                                //            }

                                //        }

                                //    }
                                //}
                                //活性用水稀释
                                if (s_assistantType.Contains("活性"))
                                {
                                    //找到母液水剂母液瓶号
                                    s_sql = "SELECT bottle_details.*  FROM bottle_details left join assistant_details on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE assistant_details.UnitOfAccount = 'Water';";

                                    dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                                    if (dt_temp.Rows.Count == 0)
                                    {
                                        FADM_Form.CustomMessageBox.Show("不存在水剂母液瓶号，不能测试", "TestAbs",
                                MessageBoxButtons.OK, false);
                                        return;
                                    }
                                    else
                                    {

                                        //计算50g液体需要重量
                                        double d_stotal = FADM_Object.Communal._d_abs_total * (FADM_Object.Communal._d_ppm / 10000);
                                        //母液重量
                                        double d_dosage = d_stotal / Convert.ToDouble(s_realConcentration);
                                        double d_water = FADM_Object.Communal._d_abs_total - d_dosage;
                                        if (Convert.ToInt32(dt_data.Rows[0]["Type"]) == 0)
                                        {
                                            SmartDyeing.FADM_Auto.MyAbsorbance.Calculate(_i_nBottleNum, Convert.ToInt32(dt_temp.Rows[0]["BottleNum"].ToString()), Convert
                                        .ToInt32(s_cupNum), 1, FADM_Object.Communal._d_abs_total);
                                        }
                                        else
                                        {
                                            SmartDyeing.FADM_Auto.MyAbsorbance.Calculate(_i_nBottleNum, Convert.ToInt32(dt_temp.Rows[0]["BottleNum"].ToString()), Convert
                                         .ToInt32(s_cupNum), 11, FADM_Object.Communal._d_abs_total);
                                        }

                                        SmartDyeing.FADM_Auto.MyAbsorbance.Generate(1, Convert.ToInt32(s_cupNum));
                                        SmartDyeing.FADM_Auto.MyAbsorbance.SendData(Convert.ToInt32(s_cupNum));

                                        FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from abs_wait_list where BottleNum = " + _i_nBottleNum + " And InsertDate = '" + Convert.ToDateTime(dt_data.Rows[0]["InsertDate"]).ToString("yyyy-MM-dd HH:mm:ss.fff") + "';");

                                    }
                                }
                                //其他使用溶解剂稀释
                                else
                                {
                                    s_sql = "SELECT bottle_details.*  FROM bottle_details left join assistant_details on bottle_details.AssistantCode = assistant_details.AssistantCode " +
                                        "WHERE assistant_details.UnitOfAccount collate Chinese_PRC_CS_AS = 'G/L'  Order By bottle_details.BottleNum;";

                                    dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                                    if (dt_temp.Rows.Count == 0)
                                    {
                                        FADM_Form.CustomMessageBox.Show("不存在溶解剂母液瓶号，不能测试", "TestAbs",
                                MessageBoxButtons.OK, false);
                                        return;
                                    }
                                    else
                                    {

                                        //计算50g液体需要重量
                                        double d_stotal = FADM_Object.Communal._d_abs_total * (FADM_Object.Communal._d_ppm / 10000);
                                        //母液重量
                                        double d_dosage = d_stotal / Convert.ToDouble(s_realConcentration);
                                        double d_water = FADM_Object.Communal._d_abs_total - d_dosage;
                                        if (Convert.ToInt32(dt_data.Rows[0]["Type"]) == 0)
                                        {
                                            SmartDyeing.FADM_Auto.MyAbsorbance.Calculate(_i_nBottleNum, Convert.ToInt32(dt_temp.Rows[0]["BottleNum"].ToString()), Convert
                                        .ToInt32(s_cupNum), 1, FADM_Object.Communal._d_abs_total);
                                        }
                                        else
                                        {
                                            SmartDyeing.FADM_Auto.MyAbsorbance.Calculate(_i_nBottleNum, Convert.ToInt32(dt_temp.Rows[0]["BottleNum"].ToString()), Convert
                                        .ToInt32(s_cupNum), 11, FADM_Object.Communal._d_abs_total);
                                        }

                                        SmartDyeing.FADM_Auto.MyAbsorbance.Generate(1, Convert.ToInt32(s_cupNum));
                                        SmartDyeing.FADM_Auto.MyAbsorbance.SendData(Convert.ToInt32(s_cupNum));

                                        FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from abs_wait_list where BottleNum = " + _i_nBottleNum + " And InsertDate = '" + Convert.ToDateTime(dt_data.Rows[0]["InsertDate"]).ToString("yyyy-MM-dd HH:mm:ss.fff") + "';");
                                    }
                                }
                            }

                        }
                    }
                //
                lab_end:
                    Thread.Sleep(30000);
                }
                catch
                { }
            }
        }

        //自动加入到开料工位
        private void InsertBrew()
        {
            while (true)
            {
                try
                {
                lab_again:
                    //查询是否有等待数据
                    string s_sql = "SELECT top 1 * FROM Brew_wait_list where Type = 0 order by InsertDate;";
                    DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                    if (dt_data.Rows.Count > 0)
                    {
                    lab_select:
                        //判断是否有空闲开料工位
                        s_sql = "SELECT top 1 * FROM brew_station_details  where IsUsing = 0 and Statues = '待机' and Station < 6 order by Station;";
                        DataTable dt_data_station = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                        if (dt_data_station.Rows.Count > 0)
                        {
                            //判断工位实际plc状态是否待机
                            if(AutoBrew._cup_Temps[Convert.ToInt32( dt_data_station.Rows[0]["Station"].ToString())-1]._i_statues == 1)
                            {
                                //查找开料开稀瓶号
                                s_sql = "SELECT  * FROM bottle_details  where BottleNum =" + dt_data.Rows[0]["BottleNum"].ToString();
                                DataTable dt_data_bottle_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                                if(dt_data_bottle_details.Rows.Count>0)
                                {
                                    if (dt_data_bottle_details.Rows[0]["SyringeType"].ToString() == "粉桶")
                                    {
                                        //先删除，不做
                                        FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from Brew_wait_list  where BottleNum = " + dt_data.Rows[0]["BottleNum"].ToString());
                                        goto lab_again;
                                    }
                                    //开稀
                                    else if (Convert.ToInt32(dt_data_bottle_details.Rows[0]["OriginalBottleNum"].ToString())>0)
                                    {
                                        ////先删除，不做
                                        //FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from Brew_wait_list  where BottleNum = " + dt_data.Rows[0]["BottleNum"].ToString());
                                        //goto lab_again;

                                        //判断原瓶号是否符合开料标准
                                        s_sql = "SELECT  * FROM bottle_details  where BottleNum =" + dt_data_bottle_details.Rows[0]["OriginalBottleNum"].ToString();
                                        DataTable dt_data_bottle_details_O = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                                        if(dt_data_bottle_details_O.Rows.Count >0)
                                        {
                                            //查询判断是否超期
                                            s_sql = "SELECT * FROM assistant_details WHERE AssistantCode = '" + dt_data_bottle_details_O.Rows[0]["AssistantCode"].ToString() + "';";
                                            DataTable dt_assistant_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                                            DateTime timeA = Convert.ToDateTime(dt_data_bottle_details_O.Rows[0]["BrewingData"].ToString());
                                            DateTime timeB = DateTime.Now; //获取当前时间
                                            TimeSpan ts = timeB - timeA; //计算时间差
                                            string s_time = ts.TotalHours.ToString(); //将时间差转换为小时
                                            //判断是否过期或者液量低
                                            if (Convert.ToDouble(dt_data_bottle_details_O.Rows[0]["CurrentWeight"].ToString()) < Lib_Card.Configure.Parameter.Other_Bottle_AlarmWeight && FADM_Object.Communal._b_isLowDrip)
                                            {
                                                //将当前瓶改为待开，将原瓶加入开料
                                                DateTime time = Convert.ToDateTime(dt_data.Rows[0]["InsertDate"].ToString());
                                                time = time.AddSeconds(-1);
                                                //判断是否已经正在开料
                                                s_sql = "SELECT * FROM brew_station_details WHERE IsUsing = 1 And BottleNum = '" + dt_data_bottle_details.Rows[0]["OriginalBottleNum"].ToString() + "';";
                                                DataTable dt_brew_station_details_s = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                                                if (dt_brew_station_details_s.Rows.Count == 0)
                                                    //插入到排队列表
                                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "INSERT INTO Brew_wait_list(BottleNum, InsertDate,Type) VALUES('" + dt_data_bottle_details.Rows[0]["OriginalBottleNum"].ToString() + "','" + time.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',0);");

                                                FADM_Object.Communal._fadmSqlserver.ReviseData("Update  Brew_wait_list set Type = 1 where BottleNum = " + dt_data.Rows[0]["BottleNum"].ToString());
                                                goto lab_again;

                                            }
                                            else if (Convert.ToDouble(s_time) > Convert.ToDouble(dt_assistant_details.Rows[0]["TermOfValidity"].ToString()) && FADM_Object.Communal._b_isOutDrip)
                                            {
                                                //将当前瓶改为待开，将原瓶加入开料
                                                DateTime time = Convert.ToDateTime(dt_data.Rows[0]["InsertDate"].ToString());
                                                time = time.AddSeconds(-1);
                                                //判断是否已经正在开料
                                                s_sql = "SELECT * FROM brew_station_details WHERE IsUsing = 1 And BottleNum = '" + dt_data_bottle_details.Rows[0]["OriginalBottleNum"].ToString() + "';";
                                                DataTable dt_brew_station_details_s = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                                                if (dt_brew_station_details_s.Rows.Count == 0)
                                                    //插入到排队列表
                                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "INSERT INTO Brew_wait_list(BottleNum, InsertDate,Type) VALUES('" + dt_data_bottle_details.Rows[0]["OriginalBottleNum"].ToString() + "','" + time.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',0);");

                                                FADM_Object.Communal._fadmSqlserver.ReviseData("Update  Brew_wait_list set Type = 1 where BottleNum = " + dt_data.Rows[0]["BottleNum"].ToString());
                                                goto lab_again;
                                            }
                                            else
                                            {
                                                //判断是否已经正在开料
                                                s_sql = "SELECT * FROM brew_station_details WHERE IsUsing = 1 And BottleNum = '" + dt_data_bottle_details.Rows[0]["OriginalBottleNum"].ToString() + "';";
                                                DataTable dt_brew_station_details_s = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                                                if (dt_brew_station_details_s.Rows.Count == 0)
                                                    //插入开料工位
                                                    FADM_Object.Communal._fadmSqlserver.ReviseData("Update brew_station_details  set BottleNum = " + dt_data.Rows[0]["BottleNum"].ToString()
                                                    + ",IsUsing=1,PowderNum = " + dt_data_bottle_details.Rows[0]["OriginalBottleNum"].ToString() + ",Cooperate = 13,ReceptionTime =  '" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")
                                                    + "' where Station = " + dt_data_station.Rows[0]["Station"].ToString() + ";");
                                                FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from Brew_wait_list  where BottleNum = " + dt_data.Rows[0]["BottleNum"].ToString());
                                                goto lab_again;
                                            }
                                        }
                                    }
                                    //开料
                                    else
                                    {
                                        //查询是否有自动开料粉体
                                        s_sql = "SELECT  * FROM bottle_details  where AssistantCode ='" + dt_data_bottle_details.Rows[0]["AssistantCode"].ToString() + "' And SyringeType = '粉桶' And BottleNum <"+ Lib_Card.Configure.Parameter.Machine_Bottle_Total; 
                                        DataTable dt_data_bottle_details_1 = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                                        if(dt_data_bottle_details_1.Rows.Count >0)
                                        {
                                            //判断是否已经正在开料
                                            s_sql = "SELECT * FROM brew_station_details WHERE IsUsing = 1 And BottleNum = '" + dt_data_bottle_details.Rows[0]["OriginalBottleNum"].ToString() + "';";
                                            DataTable dt_brew_station_details_s = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                                            if (dt_brew_station_details_s.Rows.Count == 0)
                                                //插入开料工位
                                                FADM_Object.Communal._fadmSqlserver.ReviseData("Update brew_station_details  set BottleNum = " + dt_data.Rows[0]["BottleNum"].ToString()
                                                + ",IsUsing=1,PowderNum = " + dt_data_bottle_details_1.Rows[0]["BottleNum"].ToString() + ",Cooperate = 13,ReceptionTime =  '" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")
                                                + "' where Station = " + dt_data_station.Rows[0]["Station"].ToString() + ";");
                                            FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from Brew_wait_list  where BottleNum = " + dt_data.Rows[0]["BottleNum"].ToString());
                                            goto lab_again;
                                        }
                                        else
                                        {
                                             // 删除等待列表数据，不进行开料
                                            //FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from Brew_wait_list  where BottleNum = " + dt_data.Rows[0]["BottleNum"].ToString());

                                            FADM_Object.Communal._fadmSqlserver.ReviseData("Update  Brew_wait_list set Type = 1 where BottleNum = " + dt_data.Rows[0]["BottleNum"].ToString());
                                            goto lab_again;
                                        }
                                    }
                                }
                                else
                                {
                                    //删除等待列表数据，不进行开料
                                    FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from Brew_wait_list  where BottleNum = " + dt_data.Rows[0]["BottleNum"].ToString());
                                    goto lab_again;

                                }
                            }
                            //置为正在使用,重新选择工位
                            else
                            {
                                FADM_Object.Communal._fadmSqlserver.ReviseData("Update brew_station_details set IsUsing = 1 where Station = "+ dt_data_station.Rows[0]["Station"].ToString());
                                goto lab_select;
                            }
                        }
                        //没有可以使用工位
                        else
                        {
                            goto lab_end;
                        }
                    }
                //
                lab_end:
                    Thread.Sleep(10000);
                }
                catch
                { }
            }
        }

        private void ReadBalance()
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                while (true)
                {
                    Thread.Sleep(1);
                    if (Lib_Card.Configure.Parameter.Machine_BalanceType == 0)
                    {
                        //if (FADM_Object.Communal.IsZero)
                        //{
                        //    //判断是否有清零信号
                        //    if (Lib_SerialPort.Balance.METTLER.bZeroSign)
                        //    {
                        //        FADM_Object.Communal.Mettler.Zero();
                        //        Lib_SerialPort.Balance.METTLER.bZeroSign = false;
                        //    }
                        //    else if (Lib_SerialPort.Balance.METTLER.bReSetSign)
                        //    {
                        //        FADM_Object.Communal.Mettler.Reset();
                        //        Lib_SerialPort.Balance.METTLER.bReSetSign = false;
                        //    }
                        //}
                        FADM_Object.Communal.Mettler.WriteAndRead();
                        FADM_Object.Communal.dBalanceValue = Lib_SerialPort.Balance.METTLER.BalanceValue;

                    }
                    else
                    {
                        FADM_Object.Communal.Shinko.WriteAndRead();
                        FADM_Object.Communal.dBalanceValue = Lib_SerialPort.Balance.SHINKO.BalanceValue;
                    }

                }
            }
            else
            {
                while (true)
                {
                    try
                    {
                        Thread.Sleep(60);  //这里一直读 如果同时地方也同时读 就有可能造成读不到 
                                           //
                        if (FADM_Object.Communal.ReadTcpStatus() && FADM_Object.Communal._b_auto)
                        {
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

                            //Console.WriteLine("读天平");
                            //只读天平寄存器 
                            int[] ia_array = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                            int i_state = FADM_Object.Communal._tcpModBus.Read(901, 28, ref ia_array);
                            if (i_state != -1)
                            {
                                MyAlarm myAlarm = null;

                                Communal._s_plcVersion =  ia_array[26].ToString("d4") + ia_array[27].ToString("d4");

                                double d_b = 0.0;
                                if (ia_array[0] < 0)
                                {
                                    d_b = ((ia_array[1] + 1) * 65536 + ia_array[0]) / 1000.0;
                                }
                                else
                                {
                                    d_b = (ia_array[1] * 65536 + ia_array[0]) / 1000.0;
                                }
                                //if(Lib_Card.Configure.Parameter.Machine_BalanceType == 1)
                                //{
                                //    b /= 10;
                                //}




                                if (Convert.ToString(d_b).Equals("9999"))
                                {
                                    FADM_Object.Communal._s_balanceValue = "9999";
                                    //myAlarm = new FADM_Object.MyAlarm("天平超上限", "天平", true, 1);
                                }
                                else if (Convert.ToString(d_b).Equals("8888"))
                                {
                                    FADM_Object.Communal._s_balanceValue = "8888";
                                    //myAlarm = new FADM_Object.MyAlarm("天平超下限", "天平", true, 1);
                                }
                                else if (Convert.ToString(d_b).Equals("7777"))
                                {
                                    FADM_Object.Communal._s_balanceValue = "7777";
                                    //myAlarm = new FADM_Object.MyAlarm("未拿走废液桶", "天平", true, 1);
                                }
                                else if (Convert.ToString(d_b).Equals("6666"))
                                {
                                    FADM_Object.Communal._s_balanceValue = "6666";
                                    //myAlarm = new FADM_Object.MyAlarm("通讯失败 ", "天平", true, 1);
                                }
                                else
                                {
                                    FADM_Object.Communal._s_balanceValue = Convert.ToString(d_b);
                                }

                                //失能信号
                                int i_a5 = ia_array[5];
                                bool b_xpower = false;
                                bool b_ypower = false;
                                char[] ca_cc1 = Convert.ToString(i_a5, 2).PadLeft(12, '0').ToArray();
                                if (ca_cc1[ca_cc1.Length - 6].Equals('1'))
                                {
                                    b_xpower = true;
                                }
                                if (ca_cc1[ca_cc1.Length - 12].Equals('1'))
                                {
                                    b_ypower = true;
                                }
                                //if (xpower || ypower)
                                {
                                    //光幕信号 这上面的MyAlarm不要等待
                                    int i_a2 = ia_array[2];
                                    char[] ca_cc = Convert.ToString(i_a2, 2).PadLeft(4, '0').ToArray();
                                    if (ca_cc[ca_cc.Length - 1].Equals('0'))
                                    {

                                        if (Lib_Card.Configure.Parameter.Machine_IsStopOrFront == 0)
                                        {
                                            if (!Lib_Card.CardObject.bStopScr)
                                            {
                                                Lib_Card.CardObject.bStopScr = true;
                                                if (b_xpower || b_ypower)
                                                {
                                                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                                        new FADM_Object.MyAlarm("急停已按下,请打开急停开关", 1);
                                                    else
                                                        new FADM_Object.MyAlarm("Emergency stop pressed,Please turn on the emergency stop switch", 1);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (!Lib_Card.CardObject.bFront)
                                            {
                                                Lib_Card.CardObject.bFront = true;
                                                if (b_xpower || b_ypower)
                                                {
                                                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                                        new FADM_Object.MyAlarm("前光幕遮挡,请离开光幕", 1);
                                                    else
                                                        new FADM_Object.MyAlarm("Front light curtain obstruction,Please step away from the light curtain", 1);
                                                }
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
                                            if (b_xpower || b_ypower)
                                            {
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
                                    }
                                    else
                                    {
                                        Lib_Card.CardObject.bRight = false;
                                    }
                                    //左光幕信号
                                    //左光幕信号
                                    if (ca_cc[ca_cc.Length - 2].Equals('0'))
                                    {
                                        if (!Lib_Card.CardObject.bLeft)
                                        {
                                            Lib_Card.CardObject.bLeft = true;
                                            if (b_xpower || b_ypower)
                                            {
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
                                    }
                                    else
                                    {
                                        Lib_Card.CardObject.bLeft = false;
                                    }
                                    if (Lib_Card.Configure.Parameter.Machine_UseBack == 1)
                                    {
                                        if (ca_cc[ca_cc.Length - 4].Equals('0'))
                                        {
                                            if (!Lib_Card.CardObject.bBack)
                                            {
                                                Lib_Card.CardObject.bBack = true;
                                                if (b_xpower || b_ypower)
                                                {
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
                                        }
                                        else
                                        {
                                            Lib_Card.CardObject.bBack = false;
                                        }
                                    }
                                }

                                if (FADM_Object.Communal._b_isHaveDyeStop)
                                {
                                    int a23 = ia_array[22]; //
                                    string s_str2 = Convert.ToString(a23, 2).PadLeft(9, '0');
                                    char[] ca_cc = s_str2.ToArray();//
                                    if ((ca_cc[ca_cc.Length - 9].Equals('0') && !FADM_Object.Communal._b_isDyeStop) || (ca_cc[ca_cc.Length - 9].Equals('1') && FADM_Object.Communal._b_isDyeStop))
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
                            }
                            else
                            {
                                //重新连接试试
                                FADM_Object.Communal._tcpModBus.ReConnect();
                            }

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

                            if (FADM_Object.Communal._b_isHaveAutoBrew)
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
                        else if (!FADM_Object.Communal.ReadTcpStatus() || !FADM_Object.Communal._b_auto) //说明在执行动作编号后 一直循环读取完成标志位，
                        {
                            if (Lib_Card.Configure.Parameter.Machine_IsStopOrFront == 0)
                            {
                                if (Lib_Card.CardObject.bStopScr)
                                {
                                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                        new FADM_Object.MyAlarm("急停已按下,请打开急停开关", 1);
                                    else
                                        new FADM_Object.MyAlarm("Emergency stop pressed,Please turn on the emergency stop switch", 1);
                                }
                                else
                                {
                                    Lib_Card.CardObject.bStopScr = false;
                                }
                            }
                            else
                            {

                                if (Lib_Card.CardObject.bFront)
                                {
                                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                        new FADM_Object.MyAlarm("前光幕遮挡,请离开光幕", 1);
                                    else
                                        new FADM_Object.MyAlarm("Front light curtain obstruction,Please step away from the light curtain", 1);
                                }
                                else
                                {
                                    Lib_Card.CardObject.bFront = false;
                                }
                            }

                            if (Lib_Card.CardObject.bRight)
                            {
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
                            else
                            {
                                Lib_Card.CardObject.bRight = false;
                            }

                            if (Lib_Card.CardObject.bLeft)
                            {
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
                            else
                            {
                                Lib_Card.CardObject.bLeft = false;
                            }
                            if (Lib_Card.Configure.Parameter.Machine_UseBack == 1)
                            {
                                if (Lib_Card.CardObject.bBack)
                                {
                                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    {
                                        new FADM_Object.MyAlarm("后光幕遮挡,请离开光幕", 1);
                                    }
                                    else
                                    {
                                        new FADM_Object.MyAlarm("Back light curtain obstruction,Please step away from the light curtain", 1);
                                    }
                                }
                                else
                                {
                                    Lib_Card.CardObject.bBack = false;
                                }
                            }

                        }
                        else if (!FADM_Object.Communal._tcpModBus.getConnect())
                        {
                            if (Lib_Card.Configure.Parameter.Machine_IsLeftDoor == 1)
                                new FADM_Object.MyAlarm("PLC连接中断", 1);
                            else
                                new FADM_Object.MyAlarm("PLC connection interruption", 1);
                        }
                    }
                    catch (Exception ex)
                    {
                        //FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
                        Lib_Log.Log.writeLogException("ReadBalance：" + ex.ToString());
                    }

                }
            }
        }

        //private void ReadBalance1()
        //{
        //    while (true)
        //    {

        //        int[] _ia_array = { 0, 0, 0, 0, 0, 0 };
        //        int state = FADM_Object.Communal._tcpModBus.Read(901, 6, ref _ia_array);
        //        if (state == -1)
        //        {
        //        }
        //        Thread.Sleep(50);
        //    }
        //}

        //private void WriteBalance1()
        //{
        //    while (true)
        //    {
        //        int[] _ia_array = { 0 };
        //        int state = FADM_Object.Communal._tcpModBus.Write(814,  _ia_array);
        //        if (state == -1)
        //        {
        //        }
        //        Thread.Sleep(50);
        //    }
        //}

        /// <summary>
        /// 光幕遮挡
        /// </summary>
        private void lightCur()
        {
            while (true)
            {
                Thread.Sleep(100);


                if (!Lib_Card.CardObject.bRight)
                {
                    Lib_Card.CardObject.bRight = true;
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        new FADM_Object.MyAlarm("右光幕遮挡", 1);
                    else
                        new FADM_Object.MyAlarm("Right light curtain occlusion", 1);
                }

                if (!Lib_Card.CardObject.bLeft)
                {
                    Lib_Card.CardObject.bLeft = true;
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        new FADM_Object.MyAlarm("左光幕遮挡", 1);
                    else
                        new FADM_Object.MyAlarm("Left light curtain occlusion", 1);
                }

            }
        }


        private void BtnUserSwitching_Click(object sender, EventArgs e)
        {
            //AAndB a = new AAndB();
            //a.Show();
            try
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    if ("普通用户" == BtnUserSwitching.Text)
                    {
                        BtnUserSwitching.Enabled = false;
                        Admin admin = new Admin(this)
                        {
                            Owner = this
                        };
                        admin.Show();
                        admin.Focus();

                    }
                    else
                    {
                        BtnUserSwitching.Text = "普通用户";
                        toolStripSplitButton1.Enabled = false;
                        toolStripSplitButton2.Enabled = false;
                        toolStripSplitButton6.Enabled = false;
                        FADM_Object.Communal._s_operator = "123";
                    }
                }
                else
                {
                    if ("OrdinaryUsers" == BtnUserSwitching.Text)
                    {
                        BtnUserSwitching.Enabled = false;
                        Admin admin = new Admin(this)
                        {
                            Owner = this
                        };
                        admin.Show();
                        admin.Focus();

                    }
                    else
                    {
                        BtnUserSwitching.Text = "OrdinaryUsers";
                        toolStripSplitButton1.Enabled = false;
                        toolStripSplitButton2.Enabled = false;
                        toolStripSplitButton6.Enabled = false;
                        FADM_Object.Communal._s_operator = "123";
                    }

                }
            }
            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }

        }

        private void MiBrewingProcess_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Control control in this.PnlMain.Controls)
                {
                    this.PnlMain.Controls.Remove(control);
                    control.Dispose();
                }

                FADM_Control.BrewingProcess brewingProcess = new BrewingProcess();
                this.PnlMain.Controls.Add(brewingProcess);
                brewingProcess.Focus();
            }
            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }

        private void MiDyeingProcess_Click(object sender, EventArgs e)
        {
           
        }

        private void MiAssistant_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Control control in this.PnlMain.Controls)
                {
                    this.PnlMain.Controls.Remove(control);
                    control.Dispose();
                }

                FADM_Control.AssistantDefin assistantDefin = new AssistantDefin();
                this.PnlMain.Controls.Add(assistantDefin);
                assistantDefin.Focus();
            }
            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }

        private void MiBottle_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Control control in this.PnlMain.Controls)
                {
                    this.PnlMain.Controls.Remove(control);
                    control.Dispose();
                }

                FADM_Control.BottleDefin bottleDefin = new BottleDefin();
                this.PnlMain.Controls.Add(bottleDefin);
                bottleDefin.Focus();
            }
            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }

        private void MiDebug_Click(object sender, EventArgs e)
        {
            if (!FADM_Object.Communal._b_isJustShowInfo && !FADM_Object.Communal._b_isUseBrewOnly)
            {
                try
                {
                    foreach (Control control in this.PnlMain.Controls)
                    {
                        this.PnlMain.Controls.Remove(control);
                        control.Dispose();
                    }

                    FADM_Control.Debug debug = new Debug();
                    this.PnlMain.Controls.Add(debug);
                    debug.Focus();
                }
                catch (Exception ex)
                {
                    FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
                }
            }
        }

        private void MiRun_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Control control in this.PnlMain.Controls)
                {
                    this.PnlMain.Controls.Remove(control);
                    control.Dispose();
                }

                FADM_Control.Run run = new Run();
                this.PnlMain.Controls.Add(run);
                run.Focus();
            }
            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }

        }

        private void MiAlarm_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Control control in this.PnlMain.Controls)
                {
                    this.PnlMain.Controls.Remove(control);
                    control.Dispose();
                }

                FADM_Control.Alarm alarm = new Alarm();
                this.PnlMain.Controls.Add(alarm);
                alarm.Focus();
            }
            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }

        public void BtnMain_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Control control in this.PnlMain.Controls)
                {
                    this.PnlMain.Controls.Remove(control);
                    control.Dispose();
                }

                if (Lib_Card.Configure.Parameter.Other_IsOnlyDrip == 0)
                {
                    if (Communal._b_isBalanceInDrip)
                    {
                        if (Communal._b_isCupAreaOnly)
                        {
                            FADM_Control.MainBottle_Only main = new MainBottle_Only();
                            this.PnlMain.Controls.Add(main);
                            main.Focus();
                        }
                        else
                        {
                            FADM_Control.MainBottle main = new MainBottle();
                            this.PnlMain.Controls.Add(main);
                            main.Focus();
                        }
                    }
                    else
                    {
                        if (Communal._b_isCupAreaOnly)
                        {
                            FADM_Control.S_MainBottle_Only main = new S_MainBottle_Only();
                            this.PnlMain.Controls.Add(main);
                            main.Focus();
                        }
                        else
                        {
                            FADM_Control.S_MainBottle main = new S_MainBottle();
                            this.PnlMain.Controls.Add(main);
                            main.Focus();
                        }
                    }
                }
                else
                {
                    FADM_Control.P_MainBottle main = new P_MainBottle();
                    this.PnlMain.Controls.Add(main);
                    main.Focus();
                }

                if (!Cup._b_open && Screen.AllScreens.Count() > 1)
                {
                    Cup c = new Cup();
                    c.Bounds = Screen.AllScreens[1].Bounds;
                    c.StartPosition = FormStartPosition.Manual;
                    c.Location = Screen.AllScreens[1].Bounds.Location;
                    Point point = new Point(Screen.AllScreens[1].Bounds.Location.X, Screen.AllScreens[1].Bounds.Location.Y);
                    c.Location = point;
                    c.WindowState = FormWindowState.Maximized;

                    c.Show();
                    Cup._b_open = true;
                }
            }
            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }

        private void BtnDrops_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Control control in this.PnlMain.Controls)
                {
                    this.PnlMain.Controls.Remove(control);
                    control.Dispose();
                }

                if (Lib_Card.Configure.Parameter.Other_IsOnlyDrip == 1)
                {
                    FADM_Control.P_Formula formula = new P_Formula(this, FADM_Object.Communal._s_operator);
                    this.PnlMain.Controls.Add(formula);
                    formula.Focus();
                }
                else
                {
                    if (false)
                    {
                    }
                    else
                    {
                        FADM_Control.Formula formula = new Formula(this);
                        this.PnlMain.Controls.Add(formula);
                        formula.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }

        private void MiAbort_Click(object sender, EventArgs e)
        {
            try
            {
                Abort abort = new Abort();
                abort.Owner = this;
                abort.Show();
                abort.Focus();
            }
            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }

        private void MiRegister_Click(object sender, EventArgs e)
        {
            try
            {
                Register register = new Register();
                register.Owner = this;
                register.Show();
                register.Focus();
            }
            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }

        private void BtnResetBrew_Click(object sender, EventArgs e)
        {
            try
            {
                FADM_Object.Communal._b_brewErr = false;
                //FADM_Object.Communal.MyBrew.Open();
                Thread P_thd_brew = new Thread(SmartDyeing.FADM_Auto.MyBrew.Brew);
                P_thd_brew.IsBackground = true;
                P_thd_brew.Start();
            }
            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }

        }


        private void Pause()
        {

            int iXPower = Lib_Card.CardObject.OA1.ReadOutPut(Lib_Card.ADT8940A1.ADT8940A1_IO.OutPut_X_Power);
            if (-1 == iXPower)
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    FADM_Form.CustomMessageBox.Show("驱动异常", "ReadOutPut", MessageBoxButtons.OK, false);
                else
                    FADM_Form.CustomMessageBox.Show("Drive abnormality", "ReadOutPut", MessageBoxButtons.OK, false);
            }

            int iYPower = Lib_Card.CardObject.OA1.ReadOutPut(Lib_Card.ADT8940A1.ADT8940A1_IO.OutPut_Y_Power);
            if (-1 == iYPower)
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    FADM_Form.CustomMessageBox.Show("驱动异常", "ReadOutPut", MessageBoxButtons.OK, false);
                else
                    FADM_Form.CustomMessageBox.Show("Drive abnormality", "ReadOutPut", MessageBoxButtons.OK, false);
            }

            if (Lib_Card.Configure.Parameter.Other_ActualPosition == 1)
            {
                int iXReady = Lib_Card.CardObject.OA1Input.InPutStatus(Lib_Card.ADT8940A1.ADT8940A1_IO.InPut_X_Ready);
                int iYReady = Lib_Card.CardObject.OA1Input.InPutStatus(Lib_Card.ADT8940A1.ADT8940A1_IO.InPut_Y_Ready);
                if (0 == iXReady || 0 == iYReady)
                {
                    Lib_Card.ADT8940A1.Module.Home.Home.Home_XYZFinish = false;
                }
            }

            if (0 == iXPower || 0 == iYPower)
            {
                //Lib_Card.ADT8940A1.Module.Home.Home.Home_XYZFinish = false;
                //FADM_Object.Communal._fadmSqlserver.DeleteSpeechInfo("右光幕遮挡");
                //FADM_Object.Communal._fadmSqlserver.DeleteSpeechInfo("左光幕遮挡");
                //FADM_Object.Communal._fadmSqlserver.DeleteSpeechInfo("急停已按下");

                if (Lib_Card.Configure.Parameter.Other_ActualPosition == 0)
                {
                    Lib_Card.ADT8940A1.Module.Home.Home.Home_XYZFinish = false;
                }
            }
            else
            {


                if (1 == Lib_Card.CardObject.OA1Input.InPutStatus(Lib_Card.ADT8940A1.ADT8940A1_IO.InPut_Sunx_A))
                {
                    if (!Lib_Card.CardObject.bRight)
                    {
                        Lib_Card.CardObject.bRight = true;
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            new FADM_Object.MyAlarm("右光幕遮挡,请离开光幕", 1);
                        else
                            new FADM_Object.MyAlarm("Right light curtain obstruction,Please step away from the light curtain", 1);
                    }
                }
                else
                {
                    Lib_Card.CardObject.bRight = false;
                }

                if (1 == Lib_Card.CardObject.OA1Input.InPutStatus(Lib_Card.ADT8940A1.ADT8940A1_IO.InPut_Sunx_B))
                {
                    if (!Lib_Card.CardObject.bLeft)
                    {
                        Lib_Card.CardObject.bLeft = true;
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            new FADM_Object.MyAlarm("左光幕遮挡,请离开光幕", 1);
                        else
                            new FADM_Object.MyAlarm("Left light curtain obstruction,Please step away from the light curtain", 1);
                    }
                }
                else
                {
                    Lib_Card.CardObject.bLeft = false;
                }

                if (1 == Lib_Card.CardObject.OA1Input.InPutStatus(Lib_Card.ADT8940A1.ADT8940A1_IO.InPut_Stop))
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
            }

        }

        public static void OpenOrCloseBear()
        {
            if (!Communal._b_isBSendOpen)
            {
                if (Communal._i_bType == 1)
                {
                    Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer buzzer = new Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer_Basic();
                    buzzer.Buzzer_On();

                    Communal._b_isBSendClose = false;
                    Communal._b_isBSendOpen = true;
                    //Lib_Log.Log.writeLogException("开：");
                }
            }

            if (!Communal._b_isBSendClose)
            {
                if (Communal._i_bType == 2)
                {
                    Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer buzzer = new Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer_Basic();
                    buzzer.Buzzer_Off();

                    Communal._b_isBSendOpen = false;
                    Communal._b_isBSendClose = true;

                    //Lib_Log.Log.writeLogException("关：" );
                }
            }
        }

        private void TmrMain_Tick(object sender, EventArgs e)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                Thread thread = new Thread(Pause);
                thread.Start();

                OpenOrCloseBear();
            }
            try
            {
                if (Lib_Card.Configure.Parameter.Machine_Type == 0)
                {
                    if (FADM_Object.Communal.dBalanceValue > Lib_Card.Configure.Parameter.Other_BalanceMaxWeight - 100)
                        FADM_Object.Communal._b_balanceAlarm = true;
                    else
                        FADM_Object.Communal._b_balanceAlarm = false;

                    Lib_Card.ADT8940A1.Axis.Axis.Axis_Paused = FADM_Object.Communal._b_pause;

                    Lib_Card.ADT8940A1.Axis.Axis.Axis_Exit = FADM_Object.Communal._b_stop;
                }


                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    switch (FADM_Object.Communal.ReadMachineStatus())
                    {

                        case 0:

                            LabStatus.Text = "待机";
                            break;
                        case 1:
                            LabStatus.Text = "回零";
                            break;

                        case 2:
                            LabStatus.Text = "定坐标";
                            break;

                        case 3:
                            LabStatus.Text = "联动";
                            break;

                        case 4:
                            LabStatus.Text = "定点移动";
                            break;

                        case 5:
                            LabStatus.Text = "校正";
                            break;

                        case 6:
                            LabStatus.Text = "针检";
                            break;

                        case 7:
                            LabStatus.Text = "滴液";
                            break;

                        case 8:
                            LabStatus.Text = "异常";
                            break;
                        case 9:
                            LabStatus.Text = "滴液完成";
                            break;
                        case 10:
                            LabStatus.Text = "复位";
                            break;

                        case 11:
                            LabStatus.Text = "自检";
                            break;

                        case 12:
                            LabStatus.Text = "验证";
                            break;
                        case 13:
                            LabStatus.Text = "测试坐标";
                            break;
                        case 14:
                            LabStatus.Text = "清洗针筒";
                            break;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    switch (FADM_Object.Communal.ReadMachineStatus())
                    {

                        case 0:

                            LabStatus.Text = "Standby";
                            break;
                        case 1:
                            LabStatus.Text = "Homing";
                            break;

                        case 2:
                            LabStatus.Text = "FixedCoordinates";
                            break;

                        case 3:
                            LabStatus.Text = "Linkage";
                            break;

                        case 4:
                            LabStatus.Text = "Fixed-pointMovement";
                            break;

                        case 5:
                            LabStatus.Text = "Water-Checking";
                            break;

                        case 6:
                            LabStatus.Text = "Bottle-Checking";
                            break;

                        case 7:
                            LabStatus.Text = "Drip";
                            break;

                        case 8:
                            LabStatus.Text = "Exceptional";
                            break;
                        case 9:
                            LabStatus.Text = "DroppingCompleted";
                            break;
                        case 10:
                            LabStatus.Text = "Reset";
                            break;

                        case 11:
                            LabStatus.Text = "Self-checking";
                            break;

                        case 12:
                            LabStatus.Text = "Water-Correction";
                            break;
                        case 13:
                            LabStatus.Text = "Test-Coordinate";
                            break;
                        case 14:
                            LabStatus.Text = "Wash";
                            break;
                        default:
                            break;
                    }
                }

                //重启com2通讯
                if (FADM_Object.Communal._b_brewErr)
                {
                    if (BtnResetBrew.Visible == false)
                    {
                        BtnResetBrew.Visible = true;
                    }
                }
                else
                {
                    if (BtnResetBrew.Visible)
                    {
                        BtnResetBrew.Visible = false;
                    }
                }

                //重启com2通讯
                if (FADM_Object.Communal._b_absErr)
                {
                    if (BtnResetAbs.Visible == false)
                    {
                        BtnResetAbs.Visible = true;
                    }
                }
                else
                {
                    if (BtnResetAbs.Visible)
                    {
                        BtnResetAbs.Visible = false;
                    }
                }
            }

            catch (Exception ex)
            {
                //FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);

                Lib_Log.Log.writeLogException("TmrMain_Tick：" + ex.ToString());
            }
        }

        private void MiCup_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Control control in this.PnlMain.Controls)
                {
                    this.PnlMain.Controls.Remove(control);
                    control.Dispose();
                }

                FADM_Control.CupDefin cupdefin = new CupDefin();
                this.PnlMain.Controls.Add(cupdefin);
            }

            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }

        public void DetailInfo_Click(object sender, EventArgs e)
        {
            try
            {
                if (Lib_Card.Configure.Parameter.Other_IsOnlyDrip == 1)
                {
                    return;
                }
                //foreach (Control control in this.PnlMain.Controls)
                //{
                //    this.PnlMain.Controls.Remove(control);
                //    control.Dispose();
                //}

                //FADM_Control.CupListDetail cupListDetail = new();
                //this.PnlMain.Controls.Add(cupListDetail);
                if (!Cup._b_open && Screen.AllScreens.Count() > 1)
                {
                    Cup c = new Cup();
                    c.Bounds = Screen.AllScreens[1].Bounds;
                    c.StartPosition = FormStartPosition.Manual;
                    c.Location = Screen.AllScreens[1].Bounds.Location;
                    Point point = new Point(Screen.AllScreens[1].Bounds.Location.X, Screen.AllScreens[1].Bounds.Location.Y);
                    c.Location = point;
                    c.WindowState = FormWindowState.Maximized;
                    c.Show();
                    Cup._b_open = true;
                }
                else if (!Cup._b_open && Screen.AllScreens.Count() == 1)
                {
                    Cup cup = new Cup();
                    cup.Show();
                    Cup._b_open = true;
                }
                else if (Cup._b_open && Screen.AllScreens.Count() == 1)
                {
                    IntPtr ptr;
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        ptr = FindWindow(null, "配液杯详情");
                    else
                        ptr = FindWindow(null, "CupStatus");
                    if (ptr != IntPtr.Zero)
                    {
                        //先删除页面，再重新打开
                        PostMessage(ptr, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);

                        Cup cup = new Cup();
                        cup.Show();
                        Cup._b_open = true;
                    }
                }
            }

            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }

        private void TmrTemp_Tick(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < FADM_Auto.Dye._cup_Temps.Length; i++)
                {
                    if (FADM_Auto.Dye._cup_Temps[i]._b_start)
                    {
                        if (Convert.ToDouble(FADM_Auto.Dye._cup_Temps[i]._s_temp) > 0)
                        {
                            Txt.WriteTXT(i + 1, FADM_Auto.Dye._cup_Temps[i]._s_temp);
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE cup_details SET RecordIndex = RecordIndex + 1 WHERE CupNum = " + (i + 1) + ";");

                            if (FADM_Auto.Dye._cup_Temps[i]._b_tagging)
                            {
                                DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(
                                    "SELECT * FROM cup_details WHERE CupNum = " + (i + 1) + ";");

                                Txt.WriteMarkTXT(i + 1, FADM_Auto.Dye._cup_Temps[i]._s_technologyName, dt_data.Rows[0]["RecordIndex"].ToString());
                                FADM_Auto.Dye._cup_Temps[i]._b_tagging = false;
                            }
                        }
                    }

                }
            }

            catch (Exception ex)
            {
                Lib_Log.Log.writeLogException("TmrTemp_Tick：" + ex.ToString());
            }
        }
        private string _s_m_route = null;
        //选择类型 默认是0(昱泰) 1是原来其他的
        private string _s_m_mode = null;
        static ReaderWriterLockSlim _logWriteLock = new ReaderWriterLockSlim();
        private void TmrFGY_Tick(object sender, EventArgs e)
        {
            if (Communal._b_getFile)
            {
                try
                {
                    //煜泰
                    if (_s_m_mode == "0")
                    {
                        //if (File.Exists(this._s_m_route))
                        {
                            try
                            {
                                if (!_logWriteLock.IsWriteLockHeld)
                                {
                                    _logWriteLock.EnterWriteLock(); //进入写入锁
                                    string[] files = Directory.GetFiles(this._s_m_route, "DripMachine*.txt");
                                    if (files.Length > 0)
                                    {
                                        Communal._b_getFile = false;
                                        Thread th = new Thread(insert1);
                                        th.IsBackground = true;
                                        th.Start(files);
                                        //TmrFGY.Enabled = false;
                                    }
                                }

                            }
                            catch (Exception ex)
                            {
                                if (_logWriteLock.IsWriteLockHeld)
                                    _logWriteLock.ExitWriteLock(); //退出写入锁
                            }
                            finally
                            {
                                if (_logWriteLock.IsWriteLockHeld)
                                    _logWriteLock.ExitWriteLock(); //退出写入锁

                            }

                        }
                    }
                    //MAM测试仪
                    else if (_s_m_mode == "1")
                    {
                        if (File.Exists(this._s_m_route))
                        {
                            try
                            {
                                if (!_logWriteLock.IsWriteLockHeld)
                                {

                                    _logWriteLock.EnterWriteLock(); //进入写入锁
                                    string[] sa_temp = File.ReadAllLines(this._s_m_route);
                                    File.Delete(this._s_m_route);
                                    Thread th = new Thread(insert3);
                                    th.IsBackground = true;
                                    th.Start(sa_temp);
                                }

                            }
                            catch (Exception ex)
                            {
                                if (_logWriteLock.IsWriteLockHeld)
                                    _logWriteLock.ExitWriteLock(); //退出写入锁
                            }
                            finally
                            {
                                if (_logWriteLock.IsWriteLockHeld)
                                    _logWriteLock.ExitWriteLock(); //退出写入锁

                            }

                        }
                    }
                    //CptImport各欄位說明.docx
                    else if (_s_m_mode == "4")
                    {
                        if (File.Exists(this._s_m_route))
                        {
                            try
                            {
                                if (!_logWriteLock.IsWriteLockHeld)
                                {

                                    _logWriteLock.EnterWriteLock(); //进入写入锁
                                    string[] sa_temp = File.ReadAllLines(this._s_m_route);
                                    File.Delete(this._s_m_route);
                                    Thread th = new Thread(insert4);
                                    th.IsBackground = true;
                                    th.Start(sa_temp);
                                }

                            }
                            catch (Exception ex)
                            {
                                if (_logWriteLock.IsWriteLockHeld)
                                    _logWriteLock.ExitWriteLock(); //退出写入锁
                            }
                            finally
                            {
                                if (_logWriteLock.IsWriteLockHeld)
                                    _logWriteLock.ExitWriteLock(); //退出写入锁

                            }

                        }
                    }
                    //读数据库进行交互
                    else if (_s_m_mode == "5")
                    {
                        Exp_FormulaFromDataBase();
                    }
                    //自定义含后处理
                    else
                    {
                        if (File.Exists(this._s_m_route))
                        {
                            try
                            {
                                if (!_logWriteLock.IsWriteLockHeld)
                                {

                                    _logWriteLock.EnterWriteLock(); //进入写入锁
                                    string[] sa_temp = File.ReadAllLines(this._s_m_route);
                                    File.Delete(this._s_m_route);
                                    Thread th = new Thread(insert);
                                    th.IsBackground = true;
                                    th.Start(sa_temp);
                                }

                            }
                            catch (Exception ex)
                            {
                                if (_logWriteLock.IsWriteLockHeld)
                                    _logWriteLock.ExitWriteLock(); //退出写入锁
                            }
                            finally
                            {
                                if (_logWriteLock.IsWriteLockHeld)
                                    _logWriteLock.ExitWriteLock(); //退出写入锁

                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    FADM_Form.CustomMessageBox.Show(ex.Message, "TmrFGY_Tick", MessageBoxButtons.OK, true);
                    //TmrFGY.Enabled = false;
                }
            }
        }

        struct recipe
        {
            public int _i_indexNum;
            public string _s_assistantCode;
            public string _s_assistantName;
            public double _s_formulaDosage;
            public string _s_unitOfAccount;
            public int _i_bottleNum;
            public double _d_settingConcentration;
            public double _d_realConcentration;
            public double _d_objectDropWeight;
            public string _s_code;
            public string _s_technologyName;
            public string _s_no;
            //染固色子工艺名字
            public string _s_childcode;


        }

        private void insert1(object arg)
        {
            try
            {
                string[] files = (string[])arg;
                int i_ind = 0;
                foreach (var file in files)
                {
                    i_ind++;
                    if (System.IO.File.Exists(file.ToString()))
                    {

                        string[] sa_rcp = System.IO.File.ReadAllLines(file.ToString());
                        int i_currentIndex = 0;

                    label:
                        string s_formulaCode = "";
                        int i_versionNum = 0;
                        int i_num = 0;
                        string s_formulaName = "";
                        double d_clothWeight = 0;
                        double d_totalWeight = 0;
                        double d_bathRatio = 0;
                        double d_allW = 0;
                        string s_deyCode = "";
                        double d_readBathRatio = 0;
                        double d_handleBathRatio = 0;
                        double d_non_AnhydrationWR = Lib_Card.Configure.Parameter.Other_Default_Non_AnhydrationWR;
                        double d_anhydrationWR = Lib_Card.Configure.Parameter.Other_Default_AnhydrationWR;

                        for (; i_currentIndex < sa_rcp.Length; i_currentIndex++)
                        {
                            if (sa_rcp[i_currentIndex].Substring(0, 4) == "001M" && sa_rcp[i_currentIndex].Length == 86)
                            {
                                //表头资料
                                s_formulaCode = sa_rcp[i_currentIndex].Substring(4, 12).Trim();
                                i_num = Convert.ToInt16(sa_rcp[i_currentIndex].Substring(18, 2));
                                s_formulaName = sa_rcp[i_currentIndex].Substring(24, 24).Trim();
                                d_clothWeight = Convert.ToDouble(sa_rcp[i_currentIndex].Substring(48, 8));
                                d_totalWeight = Convert.ToDouble(sa_rcp[i_currentIndex].Substring(56, 8));
                                //d_readBathRatio = Convert.ToDouble(sa_rcp[i_currentIndex].Substring(86, 8));
                                //d_handleBathRatio = Convert.ToDouble(sa_rcp[i_currentIndex].Substring(94, 8));
                                //d_anhydrationWR = Convert.ToDouble(sa_rcp[i_currentIndex].Substring(102, 8));
                                //d_non_AnhydrationWR = Convert.ToDouble(sa_rcp[i_currentIndex].Substring(110, 8));
                                //s_deyCode = sa_rcp[i_currentIndex].Substring(118, 30).Trim();
                                d_bathRatio = Convert.ToDouble(string.Format("{0:F}", d_totalWeight / d_clothWeight));
                                DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(
                                    "SELECT * FROM formula_head WHERE FormulaCode ='" +
                                    s_formulaCode + "' ORDER BY VersionNum DESC");
                                if (dt_data.Rows.Count > 0)
                                {
                                    i_versionNum = (Convert.ToInt16(dt_data.Rows[0]["VersionNum"])) + 1;
                                }
                                break;
                            }
                        }

                        //先把染固色工艺全部子工艺全部记录

                        //string s_sql_Code = "select * from dyeing_code where DyeingCode ='" + s_deyCode.Trim() + "' order by IndexNum;";
                        //DataTable dt_data_Code = FADM_Object.Communal.FadmSqlserver.GetData(s_sql_Code);
                        //Dictionary<int, List<recipe>> dic_listCode = new Dictionary<int, List<recipe>>();
                        //foreach (DataRow dr in dt_data_Code.Rows)
                        //{
                        //    List<recipe> lis_temp = new List<recipe>();
                        //    dic_listCode.Add(Convert.ToInt32(dr["IndexNum"].ToString()), lis_temp);
                        //}
                        List<recipe> lis_data = new List<recipe>();

                        //判断第一个是不是染色工艺
                        bool b_dyeing = false;

                        //List<recipe> Dyelist = new List<recipe>();
                        //List<recipe> Handle1list = new List<recipe>();
                        //List<recipe> Handle2list = new List<recipe>();
                        //List<recipe> Handle3list = new List<recipe>();
                        //List<recipe> Handle4list = new List<recipe>();
                        //List<recipe> Handle5list = new List<recipe>();
                        for (; i_currentIndex < sa_rcp.Length; i_currentIndex++)
                        {
                            if (sa_rcp[i_currentIndex].Substring(0, 4) == "001C" && sa_rcp[i_currentIndex].Length > 32 /*&&
                                sa_rcp[i_currentIndex].Substring(4, 12).Trim() == s_formulaCode*/)
                            {
                                {
                                    recipe re = new recipe();
                                    re._i_indexNum = Convert.ToInt16(sa_rcp[i_currentIndex].Substring(18, 2));
                                    re._s_assistantCode = sa_rcp[i_currentIndex].Substring(24, 8).Trim();
                                    int i_nleng = sa_rcp[i_currentIndex].Length - 32;
                                    re._s_formulaDosage = Convert.ToDouble(sa_rcp[i_currentIndex].Substring(32, i_nleng));

                                    re._s_code = "";

                                    DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(
                                        "SELECT *  FROM assistant_details WHERE " +
                                         "AssistantCode = '" + re._s_assistantCode + "';");

                                    if (dt_data.Rows.Count == 0)
                                    {
                                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                        {
                                            System.IO.File.Delete(file.ToString());
                                            throw new Exception("未找到" + re._s_assistantCode + "染助剂代码");
                                        }
                                        else
                                        {
                                            System.IO.File.Delete(file.ToString());
                                            throw new Exception("not found " + re._s_assistantCode + " Dyeing agent code");
                                        }
                                    }

                                    re._s_assistantName = Convert.ToString(dt_data.Rows[0]["AssistantName"]);
                                    re._s_unitOfAccount = Convert.ToString(dt_data.Rows[0]["UnitOfAccount"]);

                                    dt_data = FADM_Object.Communal._fadmSqlserver.GetData(
                                        "SELECT *  FROM bottle_details WHERE " +
                                         "AssistantCode = '" + re._s_assistantCode + "' AND " +
                                         "RealConcentration != 0 Order BY SettingConcentration DESC;");
                                    if (dt_data.Rows.Count == 0)
                                    {
                                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                        {
                                            System.IO.File.Delete(file.ToString());
                                            throw new Exception("未找到" + re._s_assistantCode + "染助剂代码的瓶号");
                                        }
                                        else
                                        {
                                            System.IO.File.Delete(file.ToString());
                                            throw new Exception("not found " + " The bottle number of the  " + re._s_assistantCode);
                                        }
                                    }
                                    for (int i = 0; i < dt_data.Rows.Count; i++)
                                    {
                                        double d_objectW = 0;
                                        if (re._s_unitOfAccount == "%")
                                        {
                                            d_objectW = Convert.ToDouble(string.Format("{0:F}",
                                                d_clothWeight * re._s_formulaDosage / Convert.ToDouble(dt_data.Rows[i]["RealConcentration"])));
                                        }
                                        else if (re._s_unitOfAccount == "g/l")
                                        {
                                            d_objectW = Convert.ToDouble(string.Format("{0:F}",
                                                d_totalWeight * re._s_formulaDosage / Convert.ToDouble(dt_data.Rows[i]["RealConcentration"])));
                                        }
                                        else
                                        {
                                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                            {
                                                System.IO.File.Delete(file.ToString());
                                                throw new Exception(re._s_assistantCode + "染助剂的计算单位设置异常");
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(file.ToString());
                                                throw new Exception(re._s_assistantCode + " Abnormal setting of calculation unit for dyeing auxiliaries");
                                            }
                                        }

                                        if (d_objectW >= Convert.ToDouble(string.Format("{0:F}", dt_data.Rows[i]["DropMinWeight"])))
                                        {
                                            re._i_bottleNum = Convert.ToInt16(dt_data.Rows[i]["BottleNum"]);
                                            re._d_settingConcentration = Convert.ToDouble(string.Format("{0:F6}", dt_data.Rows[i]["SettingConcentration"]));
                                            re._d_realConcentration = Convert.ToDouble(string.Format("{0:F6}", dt_data.Rows[i]["RealConcentration"]));
                                            re._d_objectDropWeight = d_objectW;
                                            break;
                                        }
                                        else
                                        {
                                            if (i == dt_data.Rows.Count - 1)
                                            {
                                                if (d_objectW > 0.1)
                                                {
                                                    re._i_bottleNum = Convert.ToInt16(dt_data.Rows[i]["BottleNum"]);
                                                    re._d_settingConcentration = Convert.ToDouble(string.Format("{0:F6}", dt_data.Rows[i]["SettingConcentration"]));
                                                    re._d_realConcentration = Convert.ToDouble(string.Format("{0:F6}", dt_data.Rows[i]["RealConcentration"]));
                                                    re._d_objectDropWeight = d_objectW;
                                                    break;
                                                }
                                                else
                                                {
                                                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                                    {
                                                        System.IO.File.Delete(file.ToString());
                                                        throw new Exception(re._s_assistantCode + "染助剂滴液量小于0.1克");
                                                    }
                                                    else
                                                    {
                                                        System.IO.File.Delete(file.ToString());
                                                        throw new Exception(re._s_assistantCode + " Dyeing aids with a droplet volume of less than 0.1 grams");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    if (re._s_code.Trim() == "")
                                    {
                                        d_allW += re._d_objectDropWeight;
                                        lis_data.Add(re);
                                    }

                                }

                            }
                        }
                        string s_sql = null;
                        string s_hBRList = "";


                        {
                            string s_stage = "滴液";


                            string s_temAddWaterWeight = /*Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0*/true ? String.Format("{0:F}", d_totalWeight - d_clothWeight * d_anhydrationWR - d_allW) : String.Format("{0:F3}", d_totalWeight - d_clothWeight * d_anhydrationWR - d_allW);
                            //添加配方表
                            s_sql = "INSERT INTO formula_head (" +
                                " FormulaCode, VersionNum, FormulaName," +
                                " AddWaterChoose,ClothWeight," +
                                " TotalWeight,CreateTime," +
                                " ObjectAddWaterWeight,BathRatio,HandleBathRatio,Non_AnhydrationWR,AnhydrationWR,DyeingCode,Stage,HandleBRList) VALUES('" + s_formulaCode + "'," +
                                " '" + i_versionNum + "', '" + s_formulaName + "', 1, " +
                                " '" + d_clothWeight + "', '" + d_totalWeight + "', " +
                                " '" + DateTime.Now + "', '" + s_temAddWaterWeight + "', '" + d_bathRatio + "', '" + d_handleBathRatio + "', '" + d_non_AnhydrationWR + "', '" + d_anhydrationWR + "', '" + s_deyCode.Trim() + "', '" + s_stage + "', '" + s_hBRList + "');";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        }
                        foreach (recipe rc in lis_data)
                        {
                            s_sql = "INSERT INTO formula_details (" +
                                  " FormulaCode, VersionNum, IndexNum, AssistantCode," +
                                  " FormulaDosage, UnitOfAccount, BottleNum, SettingConcentration," +
                                  " RealConcentration, AssistantName, ObjectDropWeight) VALUES( '" +
                                  s_formulaCode + "', '" + i_versionNum + "', " +
                                  "'" + rc._i_indexNum + "', '" + rc._s_assistantCode + "', '" +
                                  rc._s_formulaDosage + "', '" + rc._s_unitOfAccount + "'," +
                                  " '" + rc._i_bottleNum + "', '" + rc._d_settingConcentration + "', '" +
                                  rc._d_realConcentration + "', '" + rc._s_assistantName + "'," +
                                  " '" + rc._d_objectDropWeight + "');";

                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        }




                        if (i_currentIndex < sa_rcp.Length - 1)
                        {
                            goto label;
                        }
                    }
                    System.IO.File.Delete(file.ToString());
                }

                Communal._b_getFile = true;

            }
            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.Message, "insert", MessageBoxButtons.OK, true);
                Communal._b_getFile = true;
            }
        }
        //爱色丽分光仪对接
        private void insert2(object arg)
        {
            try
            {
                string[] files = (string[])arg;
                int i_ind = 0;
                foreach (var file in files)
                {
                    i_ind++;
                    if (System.IO.File.Exists(file.ToString()))
                    {

                        string[] sa_rcp = System.IO.File.ReadAllLines(file.ToString());
                        int i_currentIndex = 0;

                    label:
                        string s_formulaCode = "";
                        int i_versionNum = 0;
                        int i_num = 0;
                        string s_formulaName = "";
                        double d_clothWeight = 0;
                        double d_totalWeight = 0;
                        double d_bathRatio = 0;
                        double d_allW = 0;
                        string s_deyCode = "";
                        double d_readBathRatio = 0;
                        double d_handleBathRatio = 0;
                        double d_non_AnhydrationWR = Lib_Card.Configure.Parameter.Other_Default_Non_AnhydrationWR;
                        double d_anhydrationWR = Lib_Card.Configure.Parameter.Other_Default_AnhydrationWR;

                        for (; i_currentIndex < sa_rcp.Length; i_currentIndex++)
                        {
                            if (sa_rcp[i_currentIndex].Substring(0, 4) == "001M" && sa_rcp[i_currentIndex].Length == 86)
                            {
                                //表头资料
                                s_formulaCode = sa_rcp[i_currentIndex].Substring(4, 12).Trim();
                                i_num = Convert.ToInt16(sa_rcp[i_currentIndex].Substring(18, 2));
                                s_formulaName = sa_rcp[i_currentIndex].Substring(24, 24).Trim();
                                s_formulaCode = s_formulaName;
                                d_clothWeight = 5;
                                d_totalWeight = 100;
                                //d_readBathRatio = Convert.ToDouble(sa_rcp[i_currentIndex].Substring(86, 8));
                                //d_handleBathRatio = Convert.ToDouble(sa_rcp[i_currentIndex].Substring(94, 8));
                                //d_anhydrationWR = Convert.ToDouble(sa_rcp[i_currentIndex].Substring(102, 8));
                                //d_non_AnhydrationWR = Convert.ToDouble(sa_rcp[i_currentIndex].Substring(110, 8));
                                //s_deyCode = sa_rcp[i_currentIndex].Substring(118, 30).Trim();
                                d_bathRatio = Convert.ToDouble(string.Format("{0:F}", d_totalWeight / d_clothWeight));
                                DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(
                                    "SELECT * FROM formula_head WHERE FormulaCode ='" +
                                    s_formulaCode + "' ORDER BY VersionNum DESC");
                                if (dt_data.Rows.Count > 0)
                                {
                                    i_versionNum = (Convert.ToInt16(dt_data.Rows[0]["VersionNum"])) + 1;
                                }
                                break;
                            }
                        }

                        //先把染固色工艺全部子工艺全部记录

                        //string s_sql_Code = "select * from dyeing_code where DyeingCode ='" + s_deyCode.Trim() + "' order by IndexNum;";
                        //DataTable dt_data_Code = FADM_Object.Communal.FadmSqlserver.GetData(s_sql_Code);
                        //Dictionary<int, List<recipe>> dic_listCode = new Dictionary<int, List<recipe>>();
                        //foreach (DataRow dr in dt_data_Code.Rows)
                        //{
                        //    List<recipe> lis_temp = new List<recipe>();
                        //    dic_listCode.Add(Convert.ToInt32(dr["IndexNum"].ToString()), lis_temp);
                        //}
                        List<recipe> lis_data = new List<recipe>();

                        //判断第一个是不是染色工艺
                        bool b_dyeing = false;

                        //List<recipe> Dyelist = new List<recipe>();
                        //List<recipe> Handle1list = new List<recipe>();
                        //List<recipe> Handle2list = new List<recipe>();
                        //List<recipe> Handle3list = new List<recipe>();
                        //List<recipe> Handle4list = new List<recipe>();
                        //List<recipe> Handle5list = new List<recipe>();
                        for (; i_currentIndex < sa_rcp.Length; i_currentIndex++)
                        {
                            if (sa_rcp[i_currentIndex].Substring(0, 4) == "001C" && sa_rcp[i_currentIndex].Length > 32 /*&&
                                sa_rcp[i_currentIndex].Substring(4, 12).Trim() == s_formulaCode*/)
                            {
                                {
                                    recipe re = new recipe();
                                    re._i_indexNum = Convert.ToInt16(sa_rcp[i_currentIndex].Substring(18, 2));
                                    re._s_assistantCode = sa_rcp[i_currentIndex].Substring(24, 8).Trim();
                                    int i_nleng = sa_rcp[i_currentIndex].Length - 32;
                                    re._s_formulaDosage = Convert.ToDouble(sa_rcp[i_currentIndex].Substring(32, i_nleng));

                                    re._s_code = "";

                                    DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(
                                        "SELECT *  FROM assistant_details WHERE " +
                                         "AssistantCode = '" + re._s_assistantCode + "';");

                                    if (dt_data.Rows.Count == 0)
                                    {
                                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                        {
                                            System.IO.File.Delete(file.ToString());
                                            throw new Exception("未找到" + re._s_assistantCode + "染助剂代码");
                                        }
                                        else
                                        {
                                            System.IO.File.Delete(file.ToString());
                                            throw new Exception("not found " + re._s_assistantCode + " Dyeing agent code");
                                        }
                                    }

                                    re._s_assistantName = Convert.ToString(dt_data.Rows[0]["AssistantName"]);
                                    re._s_unitOfAccount = Convert.ToString(dt_data.Rows[0]["UnitOfAccount"]);

                                    dt_data = FADM_Object.Communal._fadmSqlserver.GetData(
                                        "SELECT *  FROM bottle_details WHERE " +
                                         "AssistantCode = '" + re._s_assistantCode + "' AND " +
                                         "RealConcentration != 0 Order BY SettingConcentration DESC;");
                                    if (dt_data.Rows.Count == 0)
                                    {
                                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                        {
                                            System.IO.File.Delete(file.ToString());
                                            throw new Exception("未找到" + re._s_assistantCode + "染助剂代码的瓶号");
                                        }
                                        else
                                        {
                                            System.IO.File.Delete(file.ToString());
                                            throw new Exception("not found " + " The bottle number of the  " + re._s_assistantCode);
                                        }
                                    }
                                    for (int i = 0; i < dt_data.Rows.Count; i++)
                                    {
                                        double d_objectW = 0;
                                        if (re._s_unitOfAccount == "%")
                                        {
                                            d_objectW = Convert.ToDouble(string.Format("{0:F}",
                                                d_clothWeight * re._s_formulaDosage / Convert.ToDouble(dt_data.Rows[i]["RealConcentration"])));
                                        }
                                        else if (re._s_unitOfAccount == "g/l")
                                        {
                                            d_objectW = Convert.ToDouble(string.Format("{0:F}",
                                                d_totalWeight * re._s_formulaDosage / Convert.ToDouble(dt_data.Rows[i]["RealConcentration"])));
                                        }
                                        else
                                        {
                                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                            {
                                                System.IO.File.Delete(file.ToString());
                                                throw new Exception(re._s_assistantCode + "染助剂的计算单位设置异常");
                                            }
                                            else
                                            {
                                                System.IO.File.Delete(file.ToString());
                                                throw new Exception(re._s_assistantCode + " Abnormal setting of calculation unit for dyeing auxiliaries");
                                            }
                                        }

                                        if (d_objectW >= Convert.ToDouble(string.Format("{0:F}", dt_data.Rows[i]["DropMinWeight"])))
                                        {
                                            re._i_bottleNum = Convert.ToInt16(dt_data.Rows[i]["BottleNum"]);
                                            re._d_settingConcentration = Convert.ToDouble(string.Format("{0:F6}", dt_data.Rows[i]["SettingConcentration"]));
                                            re._d_realConcentration = Convert.ToDouble(string.Format("{0:F6}", dt_data.Rows[i]["RealConcentration"]));
                                            re._d_objectDropWeight = d_objectW;
                                            break;
                                        }
                                        else
                                        {
                                            if (i == dt_data.Rows.Count - 1)
                                            {
                                                if (d_objectW > 0.1)
                                                {
                                                    re._i_bottleNum = Convert.ToInt16(dt_data.Rows[i]["BottleNum"]);
                                                    re._d_settingConcentration = Convert.ToDouble(string.Format("{0:F6}", dt_data.Rows[i]["SettingConcentration"]));
                                                    re._d_realConcentration = Convert.ToDouble(string.Format("{0:F6}", dt_data.Rows[i]["RealConcentration"]));
                                                    re._d_objectDropWeight = d_objectW;
                                                    break;
                                                }
                                                else
                                                {
                                                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                                    {
                                                        System.IO.File.Delete(file.ToString());
                                                        throw new Exception(re._s_assistantCode + "染助剂滴液量小于0.1克");
                                                    }
                                                    else
                                                    {
                                                        System.IO.File.Delete(file.ToString());
                                                        throw new Exception(re._s_assistantCode + " Dyeing aids with a droplet volume of less than 0.1 grams");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    if (re._s_code.Trim() == "")
                                    {
                                        d_allW += re._d_objectDropWeight;
                                        lis_data.Add(re);
                                    }

                                }

                            }
                        }
                        string s_sql = null;
                        string s_hBRList = "";


                        {
                            string s_stage = "滴液";


                            string s_temAddWaterWeight = /*Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0*/true ? String.Format("{0:F}", d_totalWeight - d_clothWeight * d_anhydrationWR - d_allW) : String.Format("{0:F3}", d_totalWeight - d_clothWeight * d_anhydrationWR - d_allW);
                            //添加配方表
                            s_sql = "INSERT INTO formula_head (" +
                                " FormulaCode, VersionNum, FormulaName," +
                                " AddWaterChoose,ClothWeight," +
                                " TotalWeight,CreateTime," +
                                " ObjectAddWaterWeight,BathRatio,HandleBathRatio,Non_AnhydrationWR,AnhydrationWR,DyeingCode,Stage,HandleBRList) VALUES('" + s_formulaCode + "'," +
                                " '" + i_versionNum + "', '" + s_formulaName + "', 1, " +
                                " '" + d_clothWeight + "', '" + d_totalWeight + "', " +
                                " '" + DateTime.Now + "', '" + s_temAddWaterWeight + "', '" + d_bathRatio + "', '" + d_handleBathRatio + "', '" + d_non_AnhydrationWR + "', '" + d_anhydrationWR + "', '" + s_deyCode.Trim() + "', '" + s_stage + "', '" + s_hBRList + "');";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        }
                        foreach (recipe rc in lis_data)
                        {
                            s_sql = "INSERT INTO formula_details (" +
                                  " FormulaCode, VersionNum, IndexNum, AssistantCode," +
                                  " FormulaDosage, UnitOfAccount, BottleNum, SettingConcentration," +
                                  " RealConcentration, AssistantName, ObjectDropWeight) VALUES( '" +
                                  s_formulaCode + "', '" + i_versionNum + "', " +
                                  "'" + rc._i_indexNum + "', '" + rc._s_assistantCode + "', '" +
                                  rc._s_formulaDosage + "', '" + rc._s_unitOfAccount + "'," +
                                  " '" + rc._i_bottleNum + "', '" + rc._d_settingConcentration + "', '" +
                                  rc._d_realConcentration + "', '" + rc._s_assistantName + "'," +
                                  " '" + rc._d_objectDropWeight + "');";

                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        }




                        if (i_currentIndex < sa_rcp.Length - 1)
                        {
                            goto label;
                        }
                    }
                    System.IO.File.Delete(file.ToString());
                }

                Communal._b_getFile_sec = true;

            }
            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.Message, "insert", MessageBoxButtons.OK, true);
                Communal._b_getFile_sec = true;
            }
        }

        private void insert3(object arg)
        {
            try
            {
                string[] rcp = (string[])arg;
                int currentIndex = 0;

            label:
                string formulaCode = null;
                int versionNum = 0;
                int num = 0;
                string formulaName = null;
                double clothWeight = 0;
                double totalWeight = 0;
                double bathRatio = 0;
                double allW = 0;
                double d_non_AnhydrationWR = Lib_Card.Configure.Parameter.Other_Default_Non_AnhydrationWR;
                double d_anhydrationWR = Lib_Card.Configure.Parameter.Other_Default_AnhydrationWR;
                string sOperator = null;

                for (; currentIndex < rcp.Length; currentIndex++)
                {
                    if (rcp[currentIndex].Substring(0, 4) == "500M" && rcp[currentIndex].Length == 86)
                    {
                        //表头资料
                        formulaName = rcp[currentIndex].Substring(4, 12).Trim();
                        num = Convert.ToInt16(rcp[currentIndex].Substring(18, 2));
                        formulaCode = rcp[currentIndex].Substring(24, 24).Trim();
                        clothWeight = Convert.ToDouble(rcp[currentIndex].Substring(48, 8));
                        totalWeight = Convert.ToDouble(rcp[currentIndex].Substring(56, 8));
                        sOperator = rcp[currentIndex].Substring(76, 8).Trim();
                        bathRatio = Convert.ToDouble(string.Format("{0:F}", totalWeight / clothWeight));
                        DataTable data = FADM_Object.Communal._fadmSqlserver.GetData(
                            "SELECT * FROM formula_head WHERE FormulaCode ='" +
                            formulaCode + "' ORDER BY VersionNum DESC");
                        if (data.Rows.Count > 0)
                        {
                            versionNum = (Convert.ToInt16(data.Rows[0]["VersionNum"])) + 1;
                        }
                        break;
                    }
                }


                List<recipe> list = new List<recipe>();
                for (; currentIndex < rcp.Length; currentIndex++)
                {
                    if (rcp[currentIndex].Substring(0, 4) == "500C" && rcp[currentIndex].Length == 41 &&
                        rcp[currentIndex].Substring(4, 12).Trim() == formulaName)
                    {
                        if (rcp[currentIndex].Substring(24, 8) != "WATER   ")
                        {
                            recipe re = new recipe();
                            re._i_indexNum = Convert.ToInt16(rcp[currentIndex].Substring(18, 2));
                            re._s_assistantCode = rcp[currentIndex].Substring(24, 8);
                            re._s_formulaDosage = Convert.ToDouble(rcp[currentIndex].Substring(32, 9));

                            DataTable data = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT *  FROM assistant_details WHERE " +
                                 "AssistantCode = '" + re._s_assistantCode + "';");

                            if (data.Rows.Count == 0)
                            {
                                throw new Exception("未找到" + re._s_assistantCode + "染助剂代码");
                            }

                            re._s_assistantName = Convert.ToString(data.Rows[0]["AssistantName"]);
                            re._s_unitOfAccount = Convert.ToString(data.Rows[0]["UnitOfAccount"]);

                            data = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT *  FROM bottle_details WHERE " +
                                 "AssistantCode = '" + re._s_assistantCode + "' AND " +
                                 "RealConcentration != 0 Order BY SettingConcentration DESC;");
                            if (data.Rows.Count == 0)
                            {
                                throw new Exception("未找到" + re._s_assistantCode + "染助剂代码的瓶号");
                            }
                            for (int i = 0; i < data.Rows.Count; i++)
                            {
                                double objectW = 0;
                                if (re._s_unitOfAccount == "%")
                                {
                                    objectW = Convert.ToDouble(string.Format("{0:F}",
                                        clothWeight * re._s_formulaDosage / Convert.ToDouble(data.Rows[i]["RealConcentration"])));
                                }
                                else if (re._s_unitOfAccount == "g/l")
                                {
                                    objectW = Convert.ToDouble(string.Format("{0:F}",
                                        totalWeight * re._s_formulaDosage / Convert.ToDouble(data.Rows[i]["RealConcentration"])));
                                }
                                else
                                {
                                    throw new Exception(re._s_assistantCode + "染助剂的计算单位设置异常");
                                }

                                if (objectW >= Convert.ToDouble(string.Format("{0:F}", data.Rows[i]["DropMinWeight"])))
                                {
                                    re._i_bottleNum = Convert.ToInt16(data.Rows[i]["BottleNum"]);
                                    re._d_settingConcentration = Convert.ToDouble(string.Format("{0:F}", data.Rows[i]["SettingConcentration"]));
                                    re._d_realConcentration = Convert.ToDouble(string.Format("{0:F}", data.Rows[i]["RealConcentration"]));
                                    re._d_objectDropWeight = objectW;
                                    break;
                                }
                                else
                                {
                                    if (i == data.Rows.Count - 1)
                                    {
                                        if (objectW > 0.1)
                                        {
                                            re._i_bottleNum = Convert.ToInt16(data.Rows[i]["BottleNum"]);
                                            re._d_settingConcentration = Convert.ToDouble(string.Format("{0:F}", data.Rows[i]["SettingConcentration"]));
                                            re._d_realConcentration = Convert.ToDouble(string.Format("{0:F}", data.Rows[i]["RealConcentration"]));
                                            re._d_objectDropWeight = objectW;
                                            break;
                                        }
                                        else
                                        {
                                            throw new Exception(re._s_assistantCode + "染助剂滴液量小于0.1克");
                                        }
                                    }
                                }
                            }
                            allW += re._d_objectDropWeight;
                            list.Add(re);
                        }
                        if (list.Count >= num)
                        {
                            break;
                        }
                    }
                }

                //添加配方表
                string sql = "INSERT INTO formula_head (" +
                    " FormulaCode, VersionNum, FormulaName," +
                    " AddWaterChoose,ClothWeight," +
                    " BathRatio,TotalWeight,CreateTime," +
                    " ObjectAddWaterWeight,Non_AnhydrationWR,AnhydrationWR,Operator,Stage,HandleBathRatio) VALUES('" + formulaCode + "'," +
                    " '" + versionNum + "', '" + formulaName + "', 1, " +
                    " '" + clothWeight + "', '" + bathRatio + "', '" + totalWeight + "', " +
                    " '" + DateTime.Now + "', '" + string.Format("{0:F}", (totalWeight - allW)) + "', '" + d_non_AnhydrationWR + "', '" + d_anhydrationWR + "', '" + sOperator + "', '" + "滴液" + "', '" + "0" + "');";
                FADM_Object.Communal._fadmSqlserver.ReviseData(sql);

                foreach (recipe rc in list)
                {
                    sql = "INSERT INTO formula_details (" +
                          " FormulaCode, VersionNum, IndexNum, AssistantCode," +
                          " FormulaDosage, UnitOfAccount, BottleNum, SettingConcentration," +
                          " RealConcentration, AssistantName, ObjectDropWeight) VALUES( '" +
                          formulaCode + "', '" + versionNum + "', " +
                          "'" + rc._i_indexNum + "', '" + rc._s_assistantCode + "', '" +
                          rc._s_formulaDosage + "', '" + rc._s_unitOfAccount + "'," +
                          " '" + rc._i_bottleNum + "', '" + rc._d_settingConcentration + "', '" +
                          rc._d_realConcentration + "', '" + rc._s_assistantName + "'," +
                          " '" + rc._d_objectDropWeight + "');";

                    FADM_Object.Communal._fadmSqlserver.ReviseData(sql);
                }


                if (currentIndex < rcp.Length - 1)
                {
                    goto label;
                }

            }
            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.Message, "insert", MessageBoxButtons.OK, true);
            }
        }

        private void insert4(object arg)
        {
            try
            {
                string[] rcp = (string[])arg;
                int currentIndex = 0;

            label:
                string formulaCode = null;
                int versionNum = 0;
                int num = 0;
                string formulaName = null;
                double clothWeight = 0;
                double totalWeight = 0;
                double bathRatio = 0;
                double allW = 0;
                double d_non_AnhydrationWR = Lib_Card.Configure.Parameter.Other_Default_Non_AnhydrationWR;
                double d_anhydrationWR = Lib_Card.Configure.Parameter.Other_Default_AnhydrationWR;
                //string sOperator = null;

                //记录秤布机读取数据寄存器地址
                int i_address = 0;
                //杯号
                int i_cupNum = 0;
                //获取布重地址
                int i_readadress = 0;

                for (; currentIndex < rcp.Length; currentIndex++)
                {
                    if (rcp[currentIndex].Substring(0, 4) == "500M" && rcp[currentIndex].Length == 86)
                    {

                        //表头资料
                        formulaName = rcp[currentIndex].Substring(Communal._i_Head_FormulaName - 1, Communal._i_Head_FormulaName_Len).Trim();
                        num = Convert.ToInt16(rcp[currentIndex].Substring(Communal._i_Head_Count - 1, Communal._i_Head_Count_Len));
                        formulaCode = rcp[currentIndex].Substring(Communal._i_Head_FormulaCode - 1, Communal._i_Head_FormulaCode_Len).Trim();
                        clothWeight = Convert.ToDouble(rcp[currentIndex].Substring(Communal._i_Head_ClothWeight - 1, Communal._i_Head_ClothWeight_Len));
                        totalWeight = Convert.ToDouble(rcp[currentIndex].Substring(Communal._i_Head_TotalWeight - 1, Communal._i_Head_TotalWeight_Len));
                        //sOperator = rcp[currentIndex].Substring(76, 8).Trim();
                        bathRatio = Convert.ToDouble(string.Format("{0:F}", totalWeight / clothWeight));
                        DataTable data = FADM_Object.Communal._fadmSqlserver.GetData(
                            "SELECT * FROM formula_head WHERE FormulaCode ='" +
                            formulaCode + "' ORDER BY VersionNum DESC");
                        if (data.Rows.Count > 0)
                        {
                            versionNum = (Convert.ToInt16(data.Rows[0]["VersionNum"])) + 1;
                        }

                        if (Communal._b_isUseCloth)
                        {
                            //判断是否有秤布系统，有就把第一个布重读取，记录然后删除
                            if (Lib_Card.Configure.Parameter.Machine_Area1_Type == 2)
                            {
                                //i_count++;
                                int i_sum = Lib_Card.Configure.Parameter.Machine_Area1_CupMax - Lib_Card.Configure.Parameter.Machine_Area1_CupMin + 1;
                                //获取对应区域看看是否有记录布重
                                int[] ia_values = new int[i_sum];
                                //if (i_count == 1)
                                {
                                    int state = FADM_Object.Communal.HMIBaClo.Read(21099, i_sum, ref ia_values);
                                    if (state == -1)
                                    {
                                        FADM_Object.Communal.HMIBaClo.ReConnect();
                                        state = FADM_Object.Communal.HMIBaClo.Read(21099, i_sum, ref ia_values);
                                    }
                                    if (state == 0)
                                    {
                                        for (int i = 0; i < ia_values.Length; i++)
                                        {
                                            //如果已经有可用数据
                                            if (ia_values[i] == 1)
                                            {
                                                i_address = 21099 + i;
                                                i_cupNum = Lib_Card.Configure.Parameter.Machine_Area1_CupMin + i;
                                                i_readadress = 20999 + 2 * i;
                                                goto lab_get;
                                            }
                                        }
                                    }
                                }
                            }
                            if (Lib_Card.Configure.Parameter.Machine_Area2_Type == 2)
                            {
                                //i_count++;
                                int i_sum = Lib_Card.Configure.Parameter.Machine_Area2_CupMax - Lib_Card.Configure.Parameter.Machine_Area2_CupMin + 1;
                                //获取对应区域看看是否有记录布重
                                int[] ia_values = new int[i_sum];
                                //if (i_count == 1)
                                {
                                    int state = FADM_Object.Communal.HMIBaClo.Read(22099, i_sum, ref ia_values);
                                    if (state == -1)
                                    {
                                        FADM_Object.Communal.HMIBaClo.ReConnect();
                                        state = FADM_Object.Communal.HMIBaClo.Read(22099, i_sum, ref ia_values);
                                    }
                                    if (state == 0)
                                    {
                                        for (int i = 0; i < ia_values.Length; i++)
                                        {
                                            //如果已经有可用数据
                                            if (ia_values[i] == 1)
                                            {
                                                i_address = 22099 + i;
                                                i_cupNum = Lib_Card.Configure.Parameter.Machine_Area2_CupMin + i;
                                                i_readadress = 21999 + 2 * i;
                                                goto lab_get;
                                            }
                                        }
                                    }
                                }
                            }
                            if (Lib_Card.Configure.Parameter.Machine_Area3_Type == 2)
                            {
                                //i_count++;
                                int i_sum = Lib_Card.Configure.Parameter.Machine_Area3_CupMax - Lib_Card.Configure.Parameter.Machine_Area3_CupMin + 1;
                                //获取对应区域看看是否有记录布重
                                int[] ia_values = new int[i_sum];
                                //if (i_count == 1)
                                {
                                    int state = FADM_Object.Communal.HMIBaClo.Read(23099, i_sum, ref ia_values);
                                    if (state == -1)
                                    {
                                        FADM_Object.Communal.HMIBaClo.ReConnect();
                                        state = FADM_Object.Communal.HMIBaClo.Read(23099, i_sum, ref ia_values);
                                    }
                                    if (state == 0)
                                    {
                                        for (int i = 0; i < ia_values.Length; i++)
                                        {
                                            //如果已经有可用数据
                                            if (ia_values[i] == 1)
                                            {
                                                i_address = 23099 + i;
                                                i_cupNum = Lib_Card.Configure.Parameter.Machine_Area3_CupMin + i;
                                                i_readadress = 22999 + 2 * i;
                                                goto lab_get;
                                            }
                                        }
                                    }
                                }
                            }
                            if (Lib_Card.Configure.Parameter.Machine_Area4_Type == 2)
                            {
                                //i_count++;
                                int i_sum = Lib_Card.Configure.Parameter.Machine_Area4_CupMax - Lib_Card.Configure.Parameter.Machine_Area4_CupMin + 1;
                                //获取对应区域看看是否有记录布重
                                int[] ia_values = new int[i_sum];
                                //if (i_count == 1)
                                {
                                    int state = FADM_Object.Communal.HMIBaClo.Read(24099, i_sum, ref ia_values);
                                    if (state == -1)
                                    {
                                        FADM_Object.Communal.HMIBaClo.ReConnect();
                                        state = FADM_Object.Communal.HMIBaClo.Read(24099, i_sum, ref ia_values);
                                    }
                                    if (state == 0)
                                    {
                                        for (int i = 0; i < ia_values.Length; i++)
                                        {
                                            //如果已经有可用数据
                                            if (ia_values[i] == 1)
                                            {
                                                i_address = 24099 + i;
                                                i_cupNum = Lib_Card.Configure.Parameter.Machine_Area4_CupMin + i;
                                                i_readadress = 23999 + 2 * i;
                                                goto lab_get;
                                            }
                                        }
                                    }
                                }
                            }
                            if (Lib_Card.Configure.Parameter.Machine_Area5_Type == 2)
                            {
                                //i_count++;
                                int i_sum = Lib_Card.Configure.Parameter.Machine_Area5_CupMax - Lib_Card.Configure.Parameter.Machine_Area5_CupMin + 1;
                                //获取对应区域看看是否有记录布重
                                int[] ia_values = new int[i_sum];
                                //if (i_count == 1)
                                {
                                    int state = FADM_Object.Communal.HMIBaClo.Read(25099, i_sum, ref ia_values);
                                    if (state == -1)
                                    {
                                        FADM_Object.Communal.HMIBaClo.ReConnect();
                                        state = FADM_Object.Communal.HMIBaClo.Read(25099, i_sum, ref ia_values);
                                    }
                                    if (state == 0)
                                    {
                                        for (int i = 0; i < ia_values.Length; i++)
                                        {
                                            //如果已经有可用数据
                                            if (ia_values[i] == 1)
                                            {
                                                i_address = 25099 + i;
                                                i_cupNum = Lib_Card.Configure.Parameter.Machine_Area5_CupMin + i;
                                                i_readadress = 24999 + 2 * i;
                                                goto lab_get;
                                            }
                                        }
                                    }
                                }
                            }
                            if (Lib_Card.Configure.Parameter.Machine_Area6_Type == 2)
                            {
                                //i_count++;
                                int i_sum = Lib_Card.Configure.Parameter.Machine_Area6_CupMax - Lib_Card.Configure.Parameter.Machine_Area6_CupMin + 1;
                                //获取对应区域看看是否有记录布重
                                int[] ia_values = new int[i_sum];
                                //if (i_count == 1)
                                {
                                    int state = FADM_Object.Communal.HMIBaClo.Read(26099, i_sum, ref ia_values);
                                    if (state == -1)
                                    {
                                        FADM_Object.Communal.HMIBaClo.ReConnect();
                                        state = FADM_Object.Communal.HMIBaClo.Read(26099, i_sum, ref ia_values);
                                    }
                                    if (state == 0)
                                    {
                                        for (int i = 0; i < ia_values.Length; i++)
                                        {
                                            //如果已经有可用数据
                                            if (ia_values[i] == 1)
                                            {
                                                i_address = 26099 + i;
                                                i_cupNum = Lib_Card.Configure.Parameter.Machine_Area6_CupMin + i;
                                                i_readadress = 25999 + 2 * i;
                                                goto lab_get;
                                            }
                                        }
                                    }
                                }
                            }
                        lab_get:
                            //当判断能获取到可用布重时，读取布重
                            if (i_cupNum != 0)
                            {
                                int[] ia_values1 = new int[2];
                                int state1 = FADM_Object.Communal.HMIBaClo.Read(i_readadress, 2, ref ia_values1);
                                if (state1 == -1)
                                {
                                    FADM_Object.Communal.HMIBaClo.ReConnect();
                                    state1 = FADM_Object.Communal.HMIBaClo.Read(i_readadress, 2, ref ia_values1);
                                }
                                if (state1 == 0)
                                {
                                    double d_value = 0.0;
                                    int i_a13 = ia_values1[0];
                                    int i_a14 = ia_values1[1];
                                    if (i_a13 < 0)
                                    {
                                        d_value = (((i_a14 + 1) * 65536 + i_a13));
                                    }
                                    else
                                    {
                                        d_value = ((i_a14 * 65536 + i_a13));
                                    }
                                    //重新计算总浴量
                                    clothWeight = (double)(d_value) / 1000;
                                    totalWeight = clothWeight * bathRatio;
                                }
                            }
                        }
                        break;
                    }
                }


                List<recipe> list = new List<recipe>();
                for (; currentIndex < rcp.Length; currentIndex++)
                {
                    if (rcp[currentIndex].Substring(0, 4) == "500C" && rcp[currentIndex].Length == 41 &&
                        rcp[currentIndex].Substring(Communal._i_Detail_FormulaCode - 1, Communal._i_Detail_FormulaCode_Len).Trim() == formulaCode)
                    {
                        if (rcp[currentIndex].Substring(Communal._i_Detail_AssistantCode - 1, Communal._i_Detail_AssistantCode_Len) != "WATER   ")
                        {
                            recipe re = new recipe();
                            re._i_indexNum = Convert.ToInt16(rcp[currentIndex].Substring(Communal._i_Detail_Index - 1, Communal._i_Detail_Index_Len));
                            re._s_assistantCode = rcp[currentIndex].Substring(Communal._i_Detail_AssistantCode - 1, Communal._i_Detail_AssistantCode_Len).Trim();
                            re._s_formulaDosage = Convert.ToDouble(rcp[currentIndex].Substring(Communal._i_Detail_RealConcentration - 1, Communal._i_Detail_RealConcentration_Len));

                            DataTable data = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT *  FROM assistant_details WHERE " +
                                 "AssistantCode = '" + re._s_assistantCode + "';");

                            if (data.Rows.Count == 0)
                            {
                                throw new Exception("未找到" + re._s_assistantCode + "染助剂代码");
                            }

                            re._s_assistantName = Convert.ToString(data.Rows[0]["AssistantName"]);
                            re._s_unitOfAccount = Convert.ToString(data.Rows[0]["UnitOfAccount"]);

                            data = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT *  FROM bottle_details WHERE " +
                                 "AssistantCode = '" + re._s_assistantCode + "' AND " +
                                 "RealConcentration != 0 Order BY SettingConcentration DESC;");
                            if (data.Rows.Count == 0)
                            {
                                throw new Exception("未找到" + re._s_assistantCode + "染助剂代码的瓶号");
                            }
                            for (int i = 0; i < data.Rows.Count; i++)
                            {
                                double objectW = 0;
                                if (re._s_unitOfAccount == "%")
                                {
                                    objectW = Convert.ToDouble(string.Format("{0:F}",
                                        clothWeight * re._s_formulaDosage / Convert.ToDouble(data.Rows[i]["RealConcentration"])));
                                }
                                else if (re._s_unitOfAccount == "g/l")
                                {
                                    objectW = Convert.ToDouble(string.Format("{0:F}",
                                        totalWeight * re._s_formulaDosage / Convert.ToDouble(data.Rows[i]["RealConcentration"])));
                                }
                                else
                                {
                                    throw new Exception(re._s_assistantCode + "染助剂的计算单位设置异常");
                                }

                                if (objectW >= Convert.ToDouble(string.Format("{0:F}", data.Rows[i]["DropMinWeight"])))
                                {
                                    re._i_bottleNum = Convert.ToInt16(data.Rows[i]["BottleNum"]);
                                    re._d_settingConcentration = Convert.ToDouble(string.Format("{0:F}", data.Rows[i]["SettingConcentration"]));
                                    re._d_realConcentration = Convert.ToDouble(string.Format("{0:F}", data.Rows[i]["RealConcentration"]));
                                    re._d_objectDropWeight = objectW;
                                    break;
                                }
                                else
                                {
                                    if (i == data.Rows.Count - 1)
                                    {
                                        if (objectW > 0.1)
                                        {
                                            re._i_bottleNum = Convert.ToInt16(data.Rows[i]["BottleNum"]);
                                            re._d_settingConcentration = Convert.ToDouble(string.Format("{0:F}", data.Rows[i]["SettingConcentration"]));
                                            re._d_realConcentration = Convert.ToDouble(string.Format("{0:F}", data.Rows[i]["RealConcentration"]));
                                            re._d_objectDropWeight = objectW;
                                            break;
                                        }
                                        else
                                        {
                                            throw new Exception(re._s_assistantCode + "染助剂滴液量小于0.1克");
                                        }
                                    }
                                }
                            }
                            allW += re._d_objectDropWeight;
                            list.Add(re);
                        }
                        if (list.Count >= num)
                        {
                            break;
                        }
                    }
                }

                //添加配方表
                string sql = "INSERT INTO formula_head (" +
                    " FormulaCode, VersionNum, FormulaName," +
                    " AddWaterChoose,ClothWeight," +
                    " BathRatio,TotalWeight,CreateTime," +
                    " ObjectAddWaterWeight,Non_AnhydrationWR,AnhydrationWR,Stage,HandleBathRatio,CupNum) VALUES('" + formulaCode + "'," +
                    " '" + versionNum + "', '" + formulaName + "', 1, " +
                    " '" + clothWeight + "', '" + bathRatio + "', '" + totalWeight + "', " +
                    " '" + DateTime.Now + "', '" + string.Format("{0:F}", (totalWeight - allW)) + "', '" + d_non_AnhydrationWR + "', '" + d_anhydrationWR + "', '" + "滴液" + "', '" + "0" + "'," + i_cupNum + ");";
                FADM_Object.Communal._fadmSqlserver.ReviseData(sql);

                foreach (recipe rc in list)
                {
                    sql = "INSERT INTO formula_details (" +
                          " FormulaCode, VersionNum, IndexNum, AssistantCode," +
                          " FormulaDosage, UnitOfAccount, BottleNum, SettingConcentration," +
                          " RealConcentration, AssistantName, ObjectDropWeight) VALUES( '" +
                          formulaCode + "', '" + versionNum + "', " +
                          "'" + rc._i_indexNum + "', '" + rc._s_assistantCode + "', '" +
                          rc._s_formulaDosage + "', '" + rc._s_unitOfAccount + "'," +
                          " '" + rc._i_bottleNum + "', '" + rc._d_settingConcentration + "', '" +
                          rc._d_realConcentration + "', '" + rc._s_assistantName + "'," +
                          " '" + rc._d_objectDropWeight + "');";

                    FADM_Object.Communal._fadmSqlserver.ReviseData(sql);
                }
                //清空秤布区域
                if (Communal._b_isUseCloth)
                {
                    int[] ia_values2 = new int[1];
                    ia_values2[0] = 3;
                    int statte = FADM_Object.Communal.HMIBaClo.Write(i_address, ia_values2);
                    if (statte == -1)
                    {
                        FADM_Object.Communal.HMIBaClo.ReConnect();
                        statte = FADM_Object.Communal.HMIBaClo.Write(i_address, ia_values2);
                    }
                }

                if (currentIndex < rcp.Length - 1)
                {
                    goto label;
                }

            }
            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.Message, "insert", MessageBoxButtons.OK, true);
            }
        }

        private void Exp_FormulaFromDataBase()
        {
            try
            {
                //查询数据库导入配方
                DataTable dt_dataExp_formula_head = FADM_Object.Communal._fadmSqlserver.GetData(
                            "SELECT * FROM Exp_formula_head ");
                DataTable dt_dataExp_formula_details = FADM_Object.Communal._fadmSqlserver.GetData(
                            "SELECT * FROM Exp_formula_details ");
                DataTable dt_dataExp_formula_handle_details = FADM_Object.Communal._fadmSqlserver.GetData(
                            "SELECT * FROM Exp_formula_handle_details ");

                string s_sql="";

                foreach (DataRow dr in dt_dataExp_formula_head.Rows)
                {
                    string s_formulaCode = "";
                    int i_versionNum = 0;
                    int i_num = 0;
                    string s_formulaName = "";
                    double d_clothWeight = 0;
                    double d_totalWeight = 0;
                    double d_bathRatio = 0;
                    double d_allW = 0;
                    string s_deyCode = "";
                    double d_non_AnhydrationWR = 0;
                    double d_anhydrationWR = 0;

                    //表头资料
                    s_formulaCode = dr["FormulaCode"].ToString();
                    i_versionNum = Convert.ToInt32(dr["VersionNum"].ToString());
                    //i_num = Convert.ToInt16(sa_rcp[i_currentIndex].Substring(18, 2));
                    s_formulaName = dr["FormulaName"].ToString();
                    d_clothWeight = Convert.ToDouble(dr["ClothWeight"].ToString());
                    d_totalWeight = Convert.ToDouble(dr["TotalWeight"].ToString());
                    d_bathRatio = Convert.ToDouble(dr["BathRatio"].ToString());
                    //d_handleBathRatio = Convert.ToDouble(sa_rcp[i_currentIndex].Substring(94, 8));
                    d_anhydrationWR = /*Convert.ToDouble(dr["AnhydrationWR"].ToString())*/ Lib_Card.Configure.Parameter.Other_Default_AnhydrationWR;
                    d_non_AnhydrationWR = /*Convert.ToDouble(dr["Non_AnhydrationWR"].ToString())*/Lib_Card.Configure.Parameter.Other_Default_Non_AnhydrationWR;
                    s_deyCode = dr["DyeingCode"].ToString();

                    string s_li = dr["HandleBRList"] is DBNull ? "" : dr["HandleBRList"].ToString();
                    //染色后处理浴比值
                    List<string> _lis_hBR = new List<string>();
                    _lis_hBR.Clear();
                    if (s_li != "")
                    {
                        string[] sa_hBRList = s_li.Split('|');
                        _lis_hBR = sa_hBRList.ToList();
                    }

                    //查询染固色代码对应步骤
                    DataTable dt_datadyeing_code = FADM_Object.Communal._fadmSqlserver.GetData(
                        "SELECT * FROM dyeing_code where DyeingCode = '" + s_deyCode + "' order by IndexNum ");

                    //d_bathRatio = Convert.ToDouble(string.Format("{0:F}", d_totalWeight / d_clothWeight));

                    //记录滴液详情数据
                    List<recipe> lis_data = new List<recipe>();
                    int i_index = 0;
                    //滴液详情
                    foreach (DataRow dr_detail in dt_dataExp_formula_details.Rows)
                    {
                        if (dr_detail["FormulaCode"].ToString() == s_formulaCode)
                        {
                            i_index++;
                            recipe re = new recipe();
                            re._s_assistantCode = Convert.ToString(dr_detail["AssistantCode"]);
                            re._s_unitOfAccount = Convert.ToString(dr_detail["UnitOfAccount"]);
                            re._s_formulaDosage = Convert.ToDouble(dr_detail["FormulaDosage"]);
                            re._i_indexNum = i_index;
                            DataTable dt_data_ass = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT *  FROM assistant_details WHERE " +
                                 "AssistantCode = '" + re._s_assistantCode + "';");
                            if (dt_data_ass.Rows.Count > 0)
                            {
                                re._s_assistantName = Convert.ToString(dt_data_ass.Rows[0]["AssistantName"]);
                            }

                            DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT *  FROM bottle_details WHERE " +
                                 "AssistantCode = '" + re._s_assistantCode + "' AND " +
                                 "RealConcentration != 0 Order BY SettingConcentration DESC;");
                            if (dt_data.Rows.Count == 0)
                            {
                                s_sql = "DELETE FROM Exp_formula_handle_details WHERE" +
                                                  " FormulaCode = '" + s_formulaCode + "' AND" +
                                                  " VersionNum = '" + i_versionNum + "' ;";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                s_sql = "DELETE FROM Exp_formula_details WHERE" +
                                  " FormulaCode = '" + s_formulaCode + "' AND" +
                                  " VersionNum = '" + i_versionNum + "' ;";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                s_sql = "DELETE FROM Exp_formula_head WHERE" +
                                  " FormulaCode = '" + s_formulaCode + "' AND" +
                                  " VersionNum = '" + i_versionNum + "' ;";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    throw new Exception("未找到" + re._s_assistantCode + "染助剂代码的瓶号");
                                else

                                    throw new Exception("not found " + " The bottle number of the  " + re._s_assistantCode);
                            }
                            for (int i = 0; i < dt_data.Rows.Count; i++)
                            {
                                double d_objectW = 0;
                                if (re._s_unitOfAccount == "%")
                                {
                                    d_objectW = Convert.ToDouble(string.Format("{0:F}",
                                        d_clothWeight * re._s_formulaDosage / Convert.ToDouble(dt_data.Rows[i]["RealConcentration"])));
                                }
                                else if (re._s_unitOfAccount == "g/l"|| re._s_unitOfAccount == "G/L")
                                {
                                    d_objectW = Convert.ToDouble(string.Format("{0:F}",
                                        d_totalWeight * re._s_formulaDosage / Convert.ToDouble(dt_data.Rows[i]["RealConcentration"])));
                                }
                                else
                                {
                                    s_sql = "DELETE FROM Exp_formula_handle_details WHERE" +
                                                  " FormulaCode = '" + s_formulaCode + "' AND" +
                                                  " VersionNum = '" + i_versionNum + "' ;";
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                    s_sql = "DELETE FROM Exp_formula_details WHERE" +
                                      " FormulaCode = '" + s_formulaCode + "' AND" +
                                      " VersionNum = '" + i_versionNum + "' ;";
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                    s_sql = "DELETE FROM Exp_formula_head WHERE" +
                                      " FormulaCode = '" + s_formulaCode + "' AND" +
                                      " VersionNum = '" + i_versionNum + "' ;";
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                        throw new Exception(re._s_assistantCode + "染助剂的计算单位设置异常");
                                    else
                                        throw new Exception(re._s_assistantCode + " Abnormal setting of calculation unit for dyeing auxiliaries");
                                }

                                if (d_objectW >= Convert.ToDouble(string.Format("{0:F}", dt_data.Rows[i]["DropMinWeight"])))
                                {
                                    re._i_bottleNum = Convert.ToInt16(dt_data.Rows[i]["BottleNum"]);
                                    re._d_settingConcentration = Convert.ToDouble(string.Format("{0:F}", dt_data.Rows[i]["SettingConcentration"]));
                                    re._d_realConcentration = Convert.ToDouble(string.Format("{0:F}", dt_data.Rows[i]["RealConcentration"]));
                                    re._d_objectDropWeight = d_objectW;
                                    break;
                                }
                                else
                                {
                                    if (i == dt_data.Rows.Count - 1)
                                    {
                                        if (d_objectW > 0.1)
                                        {
                                            re._i_bottleNum = Convert.ToInt16(dt_data.Rows[i]["BottleNum"]);
                                            re._d_settingConcentration = Convert.ToDouble(string.Format("{0:F}", dt_data.Rows[i]["SettingConcentration"]));
                                            re._d_realConcentration = Convert.ToDouble(string.Format("{0:F}", dt_data.Rows[i]["RealConcentration"]));
                                            re._d_objectDropWeight = d_objectW;
                                            break;
                                        }
                                        else
                                        {
                                            s_sql = "DELETE FROM Exp_formula_handle_details WHERE" +
                                                  " FormulaCode = '" + s_formulaCode + "' AND" +
                                                  " VersionNum = '" + i_versionNum + "' ;";
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                            s_sql = "DELETE FROM Exp_formula_details WHERE" +
                                              " FormulaCode = '" + s_formulaCode + "' AND" +
                                              " VersionNum = '" + i_versionNum + "' ;";
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                            s_sql = "DELETE FROM Exp_formula_head WHERE" +
                                              " FormulaCode = '" + s_formulaCode + "' AND" +
                                              " VersionNum = '" + i_versionNum + "' ;";
                                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                                throw new Exception(re._s_assistantCode + "染助剂滴液量小于0.1克");
                                            else
                                                throw new Exception(re._s_assistantCode + " Dyeing aids with a droplet volume of less than 0.1 grams");
                                        }
                                    }
                                }
                            }
                            lis_data.Add(re);
                        }
                    }
                    //记录后处理数据
                    List<recipe> lis_data_Han = new List<recipe>();
                    //判断是滴液还是后处理
                    if (s_deyCode != "")
                    {
                        foreach (DataRow dr_dyeing_code in dt_dataExp_formula_handle_details.Rows)
                        {
                            if (dr_dyeing_code["FormulaCode"].ToString() == s_formulaCode)
                            {

                                recipe re = new recipe();
                                re._s_assistantCode = Convert.ToString(dr_dyeing_code["AssistantCode"]);
                                re._s_unitOfAccount = Convert.ToString(dr_dyeing_code["UnitOfAccount"]);
                                re._s_no = Convert.ToString(dr_dyeing_code["No"]);
                                re._s_technologyName = Convert.ToString(dr_dyeing_code["TechnologyName"]);
                                re._s_childcode = Convert.ToString(dr_dyeing_code["Code"]);
                                re._s_formulaDosage = Convert.ToDouble(dr_dyeing_code["FormulaDosage"]);


                                DataTable dt_data_ass = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT *  FROM assistant_details WHERE " +
                                 "AssistantCode = '" + re._s_assistantCode + "';");
                                if (dt_data_ass.Rows.Count > 0)
                                {
                                    re._s_assistantName = Convert.ToString(dt_data_ass.Rows[0]["AssistantName"]);
                                }

                                DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(
                                    "SELECT *  FROM bottle_details WHERE " +
                                     "AssistantCode = '" + re._s_assistantCode + "' AND " +
                                     "RealConcentration != 0 Order BY SettingConcentration DESC;");
                                if (dt_data.Rows.Count == 0)
                                {
                                    s_sql = "DELETE FROM Exp_formula_handle_details WHERE" +
                                                  " FormulaCode = '" + s_formulaCode + "' AND" +
                                                  " VersionNum = '" + i_versionNum + "' ;";
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                    s_sql = "DELETE FROM Exp_formula_details WHERE" +
                                      " FormulaCode = '" + s_formulaCode + "' AND" +
                                      " VersionNum = '" + i_versionNum + "' ;";
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                    s_sql = "DELETE FROM Exp_formula_head WHERE" +
                                      " FormulaCode = '" + s_formulaCode + "' AND" +
                                      " VersionNum = '" + i_versionNum + "' ;";
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                        throw new Exception("未找到" + re._s_assistantCode + "染助剂代码的瓶号");
                                    else

                                        throw new Exception("not found " + " The bottle number of the  " + re._s_assistantCode);
                                }
                                for (int i = 0; i < dt_data.Rows.Count; i++)
                                {
                                    double d_objectW = 0;
                                    if (re._s_unitOfAccount == "%")
                                    {
                                        d_objectW = Convert.ToDouble(string.Format("{0:F}",
                                            d_clothWeight * re._s_formulaDosage / Convert.ToDouble(dt_data.Rows[i]["RealConcentration"])));
                                    }
                                    else if (re._s_unitOfAccount == "g/l" || re._s_unitOfAccount == "G/L")
                                    {
                                        d_objectW = Convert.ToDouble(string.Format("{0:F}",
                                            d_clothWeight * Convert.ToDouble(_lis_hBR[Convert.ToInt32(re._s_no) - 1]) * re._s_formulaDosage / Convert.ToDouble(dt_data.Rows[i]["RealConcentration"])));
                                    }
                                    else
                                    {
                                        s_sql = "DELETE FROM Exp_formula_handle_details WHERE" +
                                                  " FormulaCode = '" + s_formulaCode + "' AND" +
                                                  " VersionNum = '" + i_versionNum + "' ;";
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                        s_sql = "DELETE FROM Exp_formula_details WHERE" +
                                          " FormulaCode = '" + s_formulaCode + "' AND" +
                                          " VersionNum = '" + i_versionNum + "' ;";
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                        s_sql = "DELETE FROM Exp_formula_head WHERE" +
                                          " FormulaCode = '" + s_formulaCode + "' AND" +
                                          " VersionNum = '" + i_versionNum + "' ;";
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                            throw new Exception(re._s_assistantCode + "染助剂的计算单位设置异常");
                                        else
                                            throw new Exception(re._s_assistantCode + " Abnormal setting of calculation unit for dyeing auxiliaries");
                                    }

                                    if (d_objectW >= Convert.ToDouble(string.Format("{0:F}", dt_data.Rows[i]["DropMinWeight"])))
                                    {
                                        re._i_bottleNum = Convert.ToInt16(dt_data.Rows[i]["BottleNum"]);
                                        re._d_settingConcentration = Convert.ToDouble(string.Format("{0:F}", dt_data.Rows[i]["SettingConcentration"]));
                                        re._d_realConcentration = Convert.ToDouble(string.Format("{0:F}", dt_data.Rows[i]["RealConcentration"]));
                                        re._d_objectDropWeight = d_objectW;
                                        break;
                                    }
                                    else
                                    {
                                        if (i == dt_data.Rows.Count - 1)
                                        {
                                            if (d_objectW > 0.1)
                                            {
                                                re._i_bottleNum = Convert.ToInt16(dt_data.Rows[i]["BottleNum"]);
                                                re._d_settingConcentration = Convert.ToDouble(string.Format("{0:F}", dt_data.Rows[i]["SettingConcentration"]));
                                                re._d_realConcentration = Convert.ToDouble(string.Format("{0:F}", dt_data.Rows[i]["RealConcentration"]));
                                                re._d_objectDropWeight = d_objectW;
                                                break;
                                            }
                                            else
                                            {
                                                s_sql = "DELETE FROM Exp_formula_handle_details WHERE" +
                                                  " FormulaCode = '" + s_formulaCode + "' AND" +
                                                  " VersionNum = '" + i_versionNum + "' ;";
                                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                                s_sql = "DELETE FROM Exp_formula_details WHERE" +
                                                  " FormulaCode = '" + s_formulaCode + "' AND" +
                                                  " VersionNum = '" + i_versionNum + "' ;";
                                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                                s_sql = "DELETE FROM Exp_formula_head WHERE" +
                                                  " FormulaCode = '" + s_formulaCode + "' AND" +
                                                  " VersionNum = '" + i_versionNum + "' ;";
                                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                                    throw new Exception(re._s_assistantCode + "染助剂滴液量小于0.1克");
                                                else
                                                    throw new Exception(re._s_assistantCode + " Dyeing aids with a droplet volume of less than 0.1 grams");
                                            }
                                        }
                                    }
                                }
                                lis_data_Han.Add(re);
                                if (dt_datadyeing_code.Rows.Count > 0)
                                {
                                    //判断是否第一个子工艺
                                    if (Convert.ToInt32(re._s_no) == 1)
                                    {
                                        //判断是否染色工艺
                                        if (Convert.ToInt32(dt_datadyeing_code.Rows[0]["Type"]) == 1)
                                        {
                                            d_allW += re._d_objectDropWeight;
                                        }
                                    }
                                }
                            }
                        }
                    }

                     s_sql = "DELETE FROM formula_handle_details WHERE" +
                                                  " FormulaCode = '" + s_formulaCode + "' AND" +
                                                  " VersionNum = '" + i_versionNum + "' ;";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                    s_sql = "DELETE FROM formula_details WHERE" +
                      " FormulaCode = '" + s_formulaCode + "' AND" +
                      " VersionNum = '" + i_versionNum + "' ;";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                    s_sql = "DELETE FROM formula_head WHERE" +
                      " FormulaCode = '" + s_formulaCode + "' AND" +
                      " VersionNum = '" + i_versionNum + "' ;";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                    s_sql = "DELETE FROM dyeing_details WHERE" +
                      " FormulaCode = '" + s_formulaCode + "' AND" +
                      " VersionNum = '" + i_versionNum + "' ;";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                    //插入配方头

                    string s_temAddWaterWeight = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? String.Format("{0:F}", d_totalWeight - d_clothWeight * d_anhydrationWR - d_allW) : String.Format("{0:F3}", d_totalWeight - d_clothWeight * d_anhydrationWR - d_allW);

                    s_sql = "INSERT INTO formula_head (" +
                        " FormulaCode, VersionNum, FormulaName," +
                        " AddWaterChoose,ClothWeight," +
                        " TotalWeight,CreateTime," +
                        " ObjectAddWaterWeight,BathRatio,HandleBathRatio,Non_AnhydrationWR,AnhydrationWR,DyeingCode,Stage,HandleBRList) VALUES('" + s_formulaCode + "'," +
                        " '" + i_versionNum + "', '" + s_formulaName + "', 1, " +
                        " '" + d_clothWeight + "', '" + d_totalWeight + "', " +
                        " '" + DateTime.Now + "', '" + s_temAddWaterWeight + "', '" + d_bathRatio + "', '" + 0.0 + "', '" + d_non_AnhydrationWR + "', '" + d_anhydrationWR + "', '" + s_deyCode.Trim() + "', '" + (s_deyCode != "" ? "后处理" : "滴液") + "', '" + s_li + "');";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                    //插入配方详情
                    foreach (recipe rc in lis_data)
                    {
                        s_sql = "INSERT INTO formula_details (" +
                              " FormulaCode, VersionNum, IndexNum, AssistantCode," +
                              " FormulaDosage, UnitOfAccount, BottleNum, SettingConcentration," +
                              " RealConcentration, AssistantName, ObjectDropWeight) VALUES( '" +
                              s_formulaCode + "', '" + i_versionNum + "', " +
                              "'" + rc._i_indexNum + "', '" + rc._s_assistantCode + "', '" +
                              rc._s_formulaDosage + "', '" + rc._s_unitOfAccount + "'," +
                              " '" + rc._i_bottleNum + "', '" + rc._d_settingConcentration + "', '" +
                              rc._d_realConcentration + "', '" + rc._s_assistantName + "'," +
                              " '" + rc._d_objectDropWeight + "');";

                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                    }
                    //插入formula_handle_details
                    foreach (recipe rc in lis_data_Han)
                    {
                        s_sql = "INSERT INTO formula_handle_details (" +
                          " FormulaCode, VersionNum,  AssistantCode," +
                          " FormulaDosage, UnitOfAccount, BottleNum, SettingConcentration," +
                          " RealConcentration, AssistantName, ObjectDropWeight,DyeingCode,Code,TechnologyName,No,BottleSelection,RealDropWeight) VALUES( '" +
                          s_formulaCode + "', '" + i_versionNum + "', '" + rc._s_assistantCode + "', '" +
                          rc._s_formulaDosage + "', '" + rc._s_unitOfAccount + "'," +
                          " '" + rc._i_bottleNum + "', '" + rc._d_settingConcentration + "', '" +
                          rc._d_realConcentration + "', '" + rc._s_assistantName + "'," +
                          " '" + rc._d_objectDropWeight + "', '" + s_deyCode.Trim() + "', '" + rc._s_childcode + "', '" + rc._s_technologyName
                          + "', '" + rc._s_no + "',0,0.0);";

                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                    }
                    //插入dyeing_details
                    if (s_deyCode != "")
                    {

                        int i_totalIndex = 1;
                        int i_childIndex = 1;
                        foreach (DataRow dr_dyeing_code in dt_datadyeing_code.Rows)
                        {

                            DataTable dt_datadyeing_process = FADM_Object.Communal._fadmSqlserver.GetData(
                            "SELECT * FROM dyeing_process where Code = '" + dr_dyeing_code["Code"] + "' order by StepNum ");

                            foreach (DataRow dr_dyeing_process in dt_datadyeing_process.Rows)
                            {
                                if (dr_dyeing_process["TechnologyName"].ToString() == "温控")
                                {

                                    s_sql = "INSERT INTO dyeing_details (" +
                                    "FormulaCode,VersionNum,StepNum,TechnologyName,Temp,TempSpeed,Time,RotorSpeed," +
                                    " Code, DyeType,Finish,No) VALUES( '" + s_formulaCode + "', '" + i_versionNum + "'," +
                                    " '" + i_totalIndex + "', '" + dr_dyeing_process["TechnologyName"].ToString() + "', '" + dr_dyeing_process["Temp"].ToString() + "'," +
                                    " '" + dr_dyeing_process["Rate"].ToString() + "', '" + dr_dyeing_process["ProportionOrTime"].ToString() + "', '" + dr_dyeing_process["Rev"].ToString() + "'," +
                                      " '" + dr_dyeing_process["Code"].ToString() + "', '" + dr_dyeing_process["Type"].ToString() + "',0,'" + i_childIndex + "');";
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                }
                                else if (dr_dyeing_process["TechnologyName"].ToString() == "冷行" || dr_dyeing_process["TechnologyName"].ToString() == "洗杯"
                                    /*|| dr_dyeing_process["TechnologyName"].ToString() == "排液"*/ || dr_dyeing_process["TechnologyName"].ToString() == "搅拌")
                                {

                                    s_sql = "INSERT INTO dyeing_details (" +
                                  "FormulaCode,VersionNum,StepNum,TechnologyName," +
                                    " Code, DyeType,RotorSpeed,Time,Finish,No) VALUES( '" + s_formulaCode + "', '" + i_versionNum + "'," +
                                    " '" + i_totalIndex + "', '" + dr_dyeing_process["TechnologyName"].ToString() + "', " +
                                  " '" + dr_dyeing_process["Code"].ToString() + "', '" + dr_dyeing_process["Type"].ToString()
                                  + "','" + dr_dyeing_process["Rev"].ToString() + "','" + dr_dyeing_process["ProportionOrTime"].ToString() + "',0,'" + i_childIndex + "');";
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                }
                                else if (dr_dyeing_process["TechnologyName"].ToString().Substring(0, 1) == "加" && dr_dyeing_process["TechnologyName"].ToString() != "加水"
                                    && dr_dyeing_process["TechnologyName"].ToString() != "加药")
                                {
                                    DataTable dt_dataformula_handle_details = FADM_Object.Communal._fadmSqlserver.GetData(
                            "SELECT * FROM formula_handle_details  WHERE" +
                      " FormulaCode = '" + s_formulaCode + "' AND" +
                      " VersionNum = '" + i_versionNum + "'  And Code = '"+ dr_dyeing_process["Code"].ToString()
                       + "'  And DyeingCode = '" + s_deyCode
                      + "' And TechnologyName = '" + dr_dyeing_process["TechnologyName"].ToString() + "';");




                                    foreach (DataRow dr_formula_handle_details in dt_dataformula_handle_details.Rows)
                                    {
                                        s_sql = "INSERT INTO dyeing_details (" +
                                         "FormulaCode,VersionNum,StepNum,TechnologyName,Time,RotorSpeed," +
                                               " Code, DyeType,AssistantCode,FormulaDosage,UnitOfAccount,BottleNum,SettingConcentration,RealConcentration," +
                                         "AssistantName,ObjectDropWeight,RealDropWeight,BottleSelection,MinWeight,Finish,No) VALUES( '" + s_formulaCode + "', '" + i_versionNum + "'," +
                                         " '" + i_totalIndex + "', '" + dr_dyeing_process["TechnologyName"].ToString() + "', '" + dr_dyeing_process["ProportionOrTime"].ToString()
                                         + "', '" + dr_dyeing_process["Rev"].ToString() + "', " +
                                         "   '" + dr_dyeing_process["Code"].ToString() + "', '" + dr_dyeing_process["Type"].ToString() + "'," +
                                         " '" + dr_formula_handle_details["AssistantCode"].ToString() + "', '" + dr_formula_handle_details["FormulaDosage"].ToString() +
                                               "', '" + dr_formula_handle_details["UnitOfAccount"].ToString() + "', '" + dr_formula_handle_details["BottleNum"].ToString() +
                                               "', '" + dr_formula_handle_details["SettingConcentration"].ToString() + "', '" + dr_formula_handle_details["RealConcentration"].ToString()
                                               + "', '" + dr_formula_handle_details["AssistantName"].ToString() + "', '" + dr_formula_handle_details["ObjectDropWeight"].ToString()
                                               + "', '" + dr_formula_handle_details["RealDropWeight"].ToString() + "', 0,0,0,'" + i_childIndex + "');";
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                        i_totalIndex++;
                                    }
                                    continue;
                                }
                                else if (dr_dyeing_process["TechnologyName"].ToString() == "加水")
                                {
                                    s_sql = "INSERT INTO dyeing_details (" +
                                        "FormulaCode,VersionNum,StepNum,TechnologyName," +
                                        " Code, DyeType,RotorSpeed,Time,Finish,No) VALUES( '" + s_formulaCode + "', '" + i_versionNum + "'," +
                                     " '" + i_totalIndex + "', '" + dr_dyeing_process["TechnologyName"].ToString() + "', " +
                                        " '" + dr_dyeing_process["Code"].ToString() + "', '" + dr_dyeing_process["Type"].ToString()
                                        + "','" + dr_dyeing_process["Rev"].ToString() + "','" + dr_dyeing_process["ProportionOrTime"].ToString() + "',0,'" + i_childIndex + "');";
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                }
                                else
                                {
                                    s_sql = "INSERT INTO dyeing_details (" +
                                       "FormulaCode,VersionNum,StepNum,TechnologyName," +
                                       " Code, DyeType,RotorSpeed,Finish,No) VALUES( '" + s_formulaCode + "', '" + i_versionNum + "'," +
                                    " '" + i_totalIndex + "', '" + dr_dyeing_process["TechnologyName"].ToString() + "', " +
                                       " '" + dr_dyeing_process["Code"].ToString() + "', '" + dr_dyeing_process["Type"].ToString()
                                       + "','" + dr_dyeing_process["Rev"].ToString() + "',0,'" + i_childIndex + "');";
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                                }
                                i_totalIndex++;
                            }
                            i_childIndex++;
                        }

                    }

                    s_sql = "DELETE FROM Exp_formula_handle_details WHERE" +
                                                  " FormulaCode = '" + s_formulaCode + "' AND" +
                                                  " VersionNum = '" + i_versionNum + "' ;";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                    s_sql = "DELETE FROM Exp_formula_details WHERE" +
                      " FormulaCode = '" + s_formulaCode + "' AND" +
                      " VersionNum = '" + i_versionNum + "' ;";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                    s_sql = "DELETE FROM Exp_formula_head WHERE" +
                      " FormulaCode = '" + s_formulaCode + "' AND" +
                      " VersionNum = '" + i_versionNum + "' ;";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                }


            }
            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.Message, "insert", MessageBoxButtons.OK, true);
            }
        }

        private void insert(object obj_arg)
        {
            try
            {
                string[] sa_rcp = (string[])obj_arg;
                int i_currentIndex = 0;

            label:
                string s_formulaCode = "";
                int i_versionNum = 0;
                int i_num = 0;
                string s_formulaName = "";
                double d_clothWeight = 0;
                double d_totalWeight = 0;
                double d_bathRatio = 0;
                double d_allW = 0;
                string s_deyCode = "";
                double d_readBathRatio = 0;
                double d_handleBathRatio = 0;
                double d_non_AnhydrationWR = 0;
                double d_anhydrationWR = 0;

                for (; i_currentIndex < sa_rcp.Length; i_currentIndex++)
                {
                    if (sa_rcp[i_currentIndex].Substring(0, 4) == "500M" && sa_rcp[i_currentIndex].Length == 148)
                    {
                        //表头资料
                        s_formulaCode = sa_rcp[i_currentIndex].Substring(4, 12).Trim();
                        i_num = Convert.ToInt16(sa_rcp[i_currentIndex].Substring(18, 2));
                        s_formulaName = sa_rcp[i_currentIndex].Substring(24, 24).Trim();
                        d_clothWeight = Convert.ToDouble(sa_rcp[i_currentIndex].Substring(48, 8));
                        d_totalWeight = Convert.ToDouble(sa_rcp[i_currentIndex].Substring(56, 8));
                        d_readBathRatio = Convert.ToDouble(sa_rcp[i_currentIndex].Substring(86, 8));
                        d_handleBathRatio = Convert.ToDouble(sa_rcp[i_currentIndex].Substring(94, 8));
                        d_anhydrationWR = Convert.ToDouble(sa_rcp[i_currentIndex].Substring(102, 8));
                        d_non_AnhydrationWR = Convert.ToDouble(sa_rcp[i_currentIndex].Substring(110, 8));
                        s_deyCode = sa_rcp[i_currentIndex].Substring(118, 30).Trim();
                        d_bathRatio = Convert.ToDouble(string.Format("{0:F}", d_totalWeight / d_clothWeight));
                        DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(
                            "SELECT * FROM formula_head WHERE FormulaCode ='" +
                            s_formulaCode + "' ORDER BY VersionNum DESC");
                        if (dt_data.Rows.Count > 0)
                        {
                            i_versionNum = (Convert.ToInt16(dt_data.Rows[0]["VersionNum"])) + 1;
                        }
                        break;
                    }
                }

                //先把染固色工艺全部子工艺全部记录

                string s_sql_Code = "select * from dyeing_code where DyeingCode ='" + s_deyCode.Trim() + "' order by IndexNum;";
                DataTable dt_data_Code = FADM_Object.Communal._fadmSqlserver.GetData(s_sql_Code);
                Dictionary<int, List<recipe>> dic_listCode = new Dictionary<int, List<recipe>>();
                foreach (DataRow dr in dt_data_Code.Rows)
                {
                    List<recipe> lis_temp = new List<recipe>();
                    dic_listCode.Add(Convert.ToInt32(dr["IndexNum"].ToString()), lis_temp);
                }
                List<recipe> lis_data = new List<recipe>();

                //判断第一个是不是染色工艺
                bool b_dyeing = false;

                //List<recipe> Dyelist = new List<recipe>();
                //List<recipe> Handle1list = new List<recipe>();
                //List<recipe> Handle2list = new List<recipe>();
                //List<recipe> Handle3list = new List<recipe>();
                //List<recipe> Handle4list = new List<recipe>();
                //List<recipe> Handle5list = new List<recipe>();
                for (; i_currentIndex < sa_rcp.Length; i_currentIndex++)
                {
                    if (sa_rcp[i_currentIndex].Substring(0, 4) == "500C" && sa_rcp[i_currentIndex].Length == 48 &&
                        sa_rcp[i_currentIndex].Substring(4, 12).Trim() == s_formulaCode)
                    {
                        if (sa_rcp[i_currentIndex].Substring(24, 8) != "WATER   ")
                        {
                            recipe re = new recipe();
                            re._i_indexNum = Convert.ToInt16(sa_rcp[i_currentIndex].Substring(18, 2));
                            re._s_assistantCode = sa_rcp[i_currentIndex].Substring(24, 8).Trim();
                            re._s_formulaDosage = Convert.ToDouble(sa_rcp[i_currentIndex].Substring(32, 9));
                            re._s_code = sa_rcp[i_currentIndex].Substring(41, 2).Trim();
                            re._s_technologyName = sa_rcp[i_currentIndex].Substring(43, 5).Trim();

                            DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT *  FROM assistant_details WHERE " +
                                 "AssistantCode = '" + re._s_assistantCode + "';");

                            if (dt_data.Rows.Count == 0)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    throw new Exception("未找到" + re._s_assistantCode + "染助剂代码");
                                else
                                    throw new Exception("not found " + re._s_assistantCode + " Dyeing agent code");
                            }

                            re._s_assistantName = Convert.ToString(dt_data.Rows[0]["AssistantName"]);
                            re._s_unitOfAccount = Convert.ToString(dt_data.Rows[0]["UnitOfAccount"]);

                            dt_data = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT *  FROM bottle_details WHERE " +
                                 "AssistantCode = '" + re._s_assistantCode + "' AND " +
                                 "RealConcentration != 0 Order BY SettingConcentration DESC;");
                            if (dt_data.Rows.Count == 0)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    throw new Exception("未找到" + re._s_assistantCode + "染助剂代码的瓶号");
                                else

                                    throw new Exception("not found " + " The bottle number of the  " + re._s_assistantCode);
                            }
                            for (int i = 0; i < dt_data.Rows.Count; i++)
                            {
                                double d_objectW = 0;
                                if (re._s_unitOfAccount == "%")
                                {
                                    d_objectW = Convert.ToDouble(string.Format("{0:F}",
                                        d_clothWeight * re._s_formulaDosage / Convert.ToDouble(dt_data.Rows[i]["RealConcentration"])));
                                }
                                else if (re._s_unitOfAccount == "g/l")
                                {
                                    d_objectW = Convert.ToDouble(string.Format("{0:F}",
                                        d_totalWeight * re._s_formulaDosage / Convert.ToDouble(dt_data.Rows[i]["RealConcentration"])));
                                }
                                else
                                {
                                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                        throw new Exception(re._s_assistantCode + "染助剂的计算单位设置异常");
                                    else
                                        throw new Exception(re._s_assistantCode + " Abnormal setting of calculation unit for dyeing auxiliaries");
                                }

                                if (d_objectW >= Convert.ToDouble(string.Format("{0:F}", dt_data.Rows[i]["DropMinWeight"])))
                                {
                                    re._i_bottleNum = Convert.ToInt16(dt_data.Rows[i]["BottleNum"]);
                                    re._d_settingConcentration = Convert.ToDouble(string.Format("{0:F}", dt_data.Rows[i]["SettingConcentration"]));
                                    re._d_realConcentration = Convert.ToDouble(string.Format("{0:F}", dt_data.Rows[i]["RealConcentration"]));
                                    re._d_objectDropWeight = d_objectW;
                                    break;
                                }
                                else
                                {
                                    if (i == dt_data.Rows.Count - 1)
                                    {
                                        if (d_objectW > 0.1)
                                        {
                                            re._i_bottleNum = Convert.ToInt16(dt_data.Rows[i]["BottleNum"]);
                                            re._d_settingConcentration = Convert.ToDouble(string.Format("{0:F}", dt_data.Rows[i]["SettingConcentration"]));
                                            re._d_realConcentration = Convert.ToDouble(string.Format("{0:F}", dt_data.Rows[i]["RealConcentration"]));
                                            re._d_objectDropWeight = d_objectW;
                                            break;
                                        }
                                        else
                                        {
                                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                                throw new Exception(re._s_assistantCode + "染助剂滴液量小于0.1克");
                                            else
                                                throw new Exception(re._s_assistantCode + " Dyeing aids with a droplet volume of less than 0.1 grams");
                                        }
                                    }
                                }
                            }
                            if (re._s_code.Trim() == "")
                            {
                                d_allW += re._d_objectDropWeight;
                                lis_data.Add(re);
                            }
                            else
                            {
                                d_allW += re._d_objectDropWeight;
                                int i_index = Convert.ToInt32(re._s_code.Trim());
                                //判断第一个是不是染色工艺，如果是，计算加水就要扣减

                                if (i_index == 1)
                                {
                                    if (i_index > dt_data_Code.Rows.Count)
                                    {
                                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                            throw new Exception("配方序号输入错误");
                                        else
                                            throw new Exception("Formula number input error");
                                    }
                                    else
                                    {
                                        if (Convert.ToInt32(dt_data_Code.Rows[i_index - 1]["Type"].ToString()) == 1)
                                        {
                                            d_allW += re._d_objectDropWeight;
                                            b_dyeing = true;
                                        }
                                    }
                                }
                                dic_listCode[i_index].Add(re);
                            }

                        }
                        else
                        {
                            int i_nCount = 0;
                            for (int i = 1; i <= dic_listCode.Count; i++)
                            {
                                i_nCount += dic_listCode[i].Count;
                            }
                            if (lis_data.Count + i_nCount >= i_num - 1)
                            {
                                break;
                            }
                        }
                    }
                }
                string s_sql = null;
                string s_hBRList = "";
                //先判断染色后处理加药条数是否足够
                if (s_deyCode.Trim() != "")
                {
                    string s_sql1 = "select * from dyeing_code where DyeingCode ='" + s_deyCode.Trim() + "' order by IndexNum;";
                    DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(s_sql1);

                    //先把助剂代码写入对应列表
                    foreach (DataRow dr in dt_data.Rows)
                    {
                        string s_sql2;
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)

                            s_sql2 = "SELECT TechnologyName FROM dyeing_process where Code = '" + dr[3].ToString() + "' and TechnologyName in ('加A','加B','加C','加D','加E','加F','加G','加H','加I','加J','加K','加L','加M','加N')  group  by TechnologyName;";
                        else
                            s_sql2 = "SELECT TechnologyName FROM dyeing_process where Code = '" + dr[3].ToString() + "' and TechnologyName in ('Add A','Add B','Add C','Add D','Add E','Add F','Add G','Add H','Add I','Add J','Add K','Add L','Add M','Add N')  group  by TechnologyName;";

                        DataTable dt_data2 = FADM_Object.Communal._fadmSqlserver.GetData(s_sql2);
                        if (dic_listCode[Convert.ToInt32(dr["IndexNum"].ToString())].Count != dt_data2.Rows.Count)
                        {
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                throw new Exception(s_formulaCode + "染色或后处理配方数据不匹配");
                            else
                                throw new Exception(s_formulaCode + " Mismatch in dyeing or post-processing formula data");
                        }

                    }
                    int i_nIndex = 1;
                    foreach (DataRow dr in dt_data.Rows)
                    {
                        foreach (recipe rc in dic_listCode[i_nIndex])
                        {
                            s_sql = "INSERT INTO formula_handle_details (" +
                              " FormulaCode, VersionNum,  AssistantCode," +
                              " FormulaDosage, UnitOfAccount, BottleNum, SettingConcentration," +
                              " RealConcentration, AssistantName, ObjectDropWeight,DyeingCode,Code,TechnologyName,BottleSelection,RealDropWeight) VALUES( '" +
                              s_formulaCode + "', '" + i_versionNum + "', '" + rc._s_assistantCode + "', '" +
                              rc._s_formulaDosage + "', '" + rc._s_unitOfAccount + "'," +
                              " '" + rc._i_bottleNum + "', '" + rc._d_settingConcentration + "', '" +
                              rc._d_realConcentration + "', '" + rc._s_assistantName + "'," +
                              " '" + rc._d_objectDropWeight + "', '" + s_deyCode.Trim() + "', '" + dr["Code"].ToString() + "', '" + rc._s_technologyName + "',0,0.0);";

                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        }
                        i_nIndex++;
                    }
                    //全部使用后处理浴比
                    foreach (DataRow dr in dt_data.Rows)
                    {
                        s_hBRList += d_handleBathRatio + "|";
                    }
                    //去掉最后一个分割符
                    s_hBRList = s_hBRList.Substring(0, s_hBRList.Length - 1);
                }


                //需要滴液
                if (lis_data.Count + (b_dyeing ? dic_listCode[1].Count : 0) > 0)
                {
                    string s_stage = "滴液";

                    int i_nCount = 0;
                    for (int i = 1; i <= dic_listCode.Count; i++)
                    {
                        i_nCount += dic_listCode[i].Count;
                    }
                    if (i_nCount > 0)
                    {
                        s_stage = "后处理";
                    }
                    string s_temAddWaterWeight = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? String.Format("{0:F}", d_totalWeight - d_clothWeight * d_anhydrationWR - d_allW) : String.Format("{0:F3}", d_totalWeight - d_clothWeight * d_anhydrationWR - d_allW);
                    //添加配方表
                    s_sql = "INSERT INTO formula_head (" +
                        " FormulaCode, VersionNum, FormulaName," +
                        " AddWaterChoose,ClothWeight," +
                        " TotalWeight,CreateTime," +
                        " ObjectAddWaterWeight,BathRatio,HandleBathRatio,Non_AnhydrationWR,AnhydrationWR,DyeingCode,Stage,HandleBRList) VALUES('" + s_formulaCode + "'," +
                        " '" + i_versionNum + "', '" + s_formulaName + "', 1, " +
                        " '" + d_clothWeight + "', '" + d_totalWeight + "', " +
                        " '" + DateTime.Now + "', '" + s_temAddWaterWeight + "', '" + d_readBathRatio + "', '" + d_handleBathRatio + "', '" + d_non_AnhydrationWR + "', '" + d_anhydrationWR + "', '" + s_deyCode.Trim() + "', '" + s_stage + "', '" + s_hBRList + "');";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                }
                //只做后处理
                else
                {
                    //加水不勾选，加水量为0
                    //添加配方表
                    s_sql = "INSERT INTO formula_head (" +
                        " FormulaCode, VersionNum, FormulaName," +
                        " AddWaterChoose,ClothWeight," +
                        " TotalWeight,CreateTime," +
                        " ObjectAddWaterWeight,BathRatio,HandleBathRatio,Non_AnhydrationWR,AnhydrationWR,DyeingCode,Stage,HandleBRList) VALUES('" + s_formulaCode + "'," +
                        " '" + i_versionNum + "', '" + s_formulaName + "', 0, " +
                        " '" + d_clothWeight + "', '" + d_totalWeight + "', " +
                        " '" + DateTime.Now + "', '" + "0" + "', '" + d_readBathRatio + "', '" + d_handleBathRatio + "', '" + d_non_AnhydrationWR + "', '" + d_anhydrationWR + "', '" + s_deyCode.Trim() + "', '" + "后处理" + "', '" + s_hBRList + "');";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                }

                foreach (recipe rc in lis_data)
                {
                    s_sql = "INSERT INTO formula_details (" +
                          " FormulaCode, VersionNum, IndexNum, AssistantCode," +
                          " FormulaDosage, UnitOfAccount, BottleNum, SettingConcentration," +
                          " RealConcentration, AssistantName, ObjectDropWeight) VALUES( '" +
                          s_formulaCode + "', '" + i_versionNum + "', " +
                          "'" + rc._i_indexNum + "', '" + rc._s_assistantCode + "', '" +
                          rc._s_formulaDosage + "', '" + rc._s_unitOfAccount + "'," +
                          " '" + rc._i_bottleNum + "', '" + rc._d_settingConcentration + "', '" +
                          rc._d_realConcentration + "', '" + rc._s_assistantName + "'," +
                          " '" + rc._d_objectDropWeight + "');";

                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                }




                if (i_currentIndex < sa_rcp.Length - 1)
                {
                    goto label;
                }

            }
            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.Message, "insert", MessageBoxButtons.OK, true);
            }
        }

        private void Reset()
        {
            MyModbusFun.Reset();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!FADM_Object.Communal._b_isJustShowInfo)
            {
                if (FADM_Object.Communal._b_isUseBrewOnly)
                    return;
                if (FADM_Object.Communal._b_isBackUp)
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show("正在备份数据，请勿关闭系统", "温馨提示", MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("Backing up data, please do not shut down the system", "Tips", MessageBoxButtons.OK, false);
                    e.Cancel = true;
                    return;
                }
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("确定退出吗(确定退出，请把机械手移动到安全区域)?", "温馨提示", MessageBoxButtons.YesNo, true);
                    if (dialogResult == DialogResult.Yes)
                    {
                        //把过期标记位置位，等待2秒响应发到开料机
                        Communal._b_closebrew = true;
                        Thread.Sleep(2000);


                        try
                        {
                            MyModbusFun.ResetClosing();
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.Equals("发现针筒或杯盖"))
                            {
                                FADM_Form.CustomMessageBox.Show("发现针筒或杯盖,点击确定10s后打开抓手", "温馨提示", MessageBoxButtons.OK, false);
                                Thread.Sleep(10000);
                            lab811:
                                int[] ia_array4 = { 7 };
                                int i_state = FADM_Object.Communal._tcpModBus.Write(811, ia_array4);
                                if (i_state == -1)
                                    goto lab811;
                            }
                        }

                        //FADM_Auto.Reset.IOReset();

                        e.Cancel = false;
                        System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id).Kill();

                    }
                    else
                    {
                        e.Cancel = true;
                    }
                }
                else
                {
                    DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("Are you sure to exit? (Please move the robotic arm to a safe area if you are sure to exit)?", "Tips", MessageBoxButtons.YesNo, true);
                    if (dialogResult == DialogResult.Yes)
                    {
                        try
                        {
                            MyModbusFun.ResetClosing();
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.Equals("发现针筒或杯盖"))
                            {
                                FADM_Form.CustomMessageBox.Show("Upon discovering the syringe or cup cap, click OK and open the gripper after 10 seconds", "Tips", MessageBoxButtons.OK, false);
                                Thread.Sleep(10000);
                            lab811:
                                int[] ia_array4 = { 7 };
                                int i_state = FADM_Object.Communal._tcpModBus.Write(811, ia_array4);
                                if (i_state == -1)
                                    goto lab811;
                            }
                        }

                        e.Cancel = false;
                        System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id).Kill();

                    }
                    else
                    {
                        e.Cancel = true;
                    }
                }
            }
            else
            {
                e.Cancel = false;
                System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id).Kill();
            }
        }

        private void BtnAllFormual_Click(object sender, EventArgs e)
        {

        }

        private void BackUp()
        {
            while (true)
            {
                try
                {

                    //执行备份操作
                    if (!FADM_Object.Communal._b_isBackUp)
                    {
                        FADM_Object.Communal._b_isBackUp = true;
                        string s_sql = "SELECT FormulaCode,VersionNum,CreateTime  FROM formula_head WHERE CreateTime < (SELECT DATEADD(DAY,-90,GETDATE())) ;";

                        DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                        foreach (DataRow dr in dt_data.Rows)
                        {
                            string s_str;
                            s_str = "insert into formula_details_temp select * from formula_details where FormulaCode='" + dr["FormulaCode"].ToString() + "' and VersionNum='" + dr["VersionNum"].ToString() + "';";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_str);
                            s_str = "delete from  formula_details where FormulaCode='" + dr["FormulaCode"].ToString() + "' and VersionNum='" + dr["VersionNum"].ToString() + "';";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_str);
                            s_str = "insert into formula_handle_details_temp select * from formula_handle_details where FormulaCode='" + dr["FormulaCode"].ToString() + "' and VersionNum='" + dr["VersionNum"].ToString() + "';";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_str);
                            s_str = "delete from  formula_handle_details where FormulaCode='" + dr["FormulaCode"].ToString() + "' and VersionNum='" + dr["VersionNum"].ToString() + "';";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_str);
                            s_str = "insert into formula_head_temp select * from formula_head where FormulaCode='" + dr["FormulaCode"].ToString() + "' and VersionNum='" + dr["VersionNum"].ToString() + "';";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_str);
                            s_str = "delete from formula_head where FormulaCode='" + dr["FormulaCode"].ToString() + "' and VersionNum='" + dr["VersionNum"].ToString() + "';";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_str);
                        }
                        FADM_Object.Communal._b_isBackUp = false;
                    }
                    Thread.Sleep(60000 * 60);

                }
                catch
                {
                    FADM_Object.Communal._b_isBackUp = false;
                }
            }
        }

        private void BtnLimitSet_Click(object sender, EventArgs e)
        {
            try
            {
                FADM_Form.LimitSet limitSet = new FADM_Form.LimitSet();

                limitSet.Show();
                limitSet.Focus();
            }

            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }

        private void MiLimitSet_Click(object sender, EventArgs e)
        {
            try
            {
                IntPtr ptr;
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    ptr = FindWindow(null, "限值设置");
                else
                    ptr = FindWindow(null, "LimitSettings");
                if (ptr == IntPtr.Zero)
                {
                    FADM_Form.LimitSet limitSet = new FADM_Form.LimitSet();

                    limitSet.Show();
                    limitSet.Focus();
                }
            }

            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }

        }

        private void MiHistoryPage_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Control control in this.PnlMain.Controls)
                {
                    this.PnlMain.Controls.Remove(control);
                    control.Dispose();
                }
                FADM_Control.HistoryData historyData = new FADM_Control.HistoryData(this);
                this.PnlMain.Controls.Add(historyData);
                historyData.Focus();
            }

            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }

        private void MiFormulaPage_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Control control in this.PnlMain.Controls)
                {
                    this.PnlMain.Controls.Remove(control);
                    control.Dispose();
                }
                FADM_Control.AllFormulas allFormulas = new FADM_Control.AllFormulas();
                this.PnlMain.Controls.Add(allFormulas);
                allFormulas.Focus();
            }

            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }

        private void MiOperator_Click(object sender, EventArgs e)
        {
            try
            {
                IntPtr ptr;
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    ptr = FindWindow(null, "操作员总览");
                else
                    ptr = FindWindow(null, "OperatorOverview");
                if (ptr == IntPtr.Zero)
                {
                    FADM_Form.Operator op = new FADM_Form.Operator();

                    op.Show();
                    op.Focus();
                }
            }

            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }

        private void toolStripSplitButton5_DropDownOpening(object sender, EventArgs e)
        {
            try
            {
                toolStripSplitButton5.DropDownItems.Clear();
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    if (BtnUserSwitching.Text == "普通用户")
                    {
                        string P_str_sql = "SELECT * FROM operator_table ;";
                        DataTable P_dt = FADM_Object.Communal._fadmSqlserver.GetData(P_str_sql);
                        foreach (DataRow dr in P_dt.Rows)
                        {
                            toolStripSplitButton5.DropDownItems.Add(dr[0].ToString());
                        }
                    }
                }
                else
                {
                    if (BtnUserSwitching.Text == "OrdinaryUsers")
                    {
                        string P_str_sql = "SELECT * FROM operator_table ;";
                        DataTable P_dt = FADM_Object.Communal._fadmSqlserver.GetData(P_str_sql);
                        foreach (DataRow dr in P_dt.Rows)
                        {
                            toolStripSplitButton5.DropDownItems.Add(dr[0].ToString());
                        }
                    }

                }
            }

            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }

        }

        private void toolStripSplitButton5_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                FADM_Object.Communal._s_operator = e.ClickedItem.ToString();

                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    BtnUserSwitching.Text = "普通用户";
                else
                    BtnUserSwitching.Text = "OrdinaryUsers";
                toolStripSplitButton1.Enabled = false;
                toolStripSplitButton2.Enabled = false;
                toolStripSplitButton6.Enabled = false;
                if (Lib_Card.Configure.Parameter.Other_IsOnlyDrip == 1)
                {
                    foreach (Control control in this.PnlMain.Controls)
                    {
                        this.PnlMain.Controls.Remove(control);
                        control.Dispose();
                    }
                }

                if (Lib_Card.Configure.Parameter.Other_IsOnlyDrip == 1)
                {
                    FADM_Control.P_Formula formula = new P_Formula(this, FADM_Object.Communal._s_operator);
                    this.PnlMain.Controls.Add(formula);
                    formula.Focus();
                }
                else
                {
                    if (false) {
                        
                    }
                    else {
                        if (FADM_Control.Formula._b_showRun == false)
                        {

                            FADM_Control.Formula formula = new Formula(this);
                            //this.PnlMain.Controls.Add(formula);
                            formula.Show();
                            formula.Focus();
                            IntPtr p1 = formula.Handle;
                            FADM_Control.Formula.HANDER = p1.ToInt32();
                        }
                        else
                        {
                            //WindowsForms10.Window.8.app.0.2804c64_r8_ad1
                            IntPtr ptr1 = new IntPtr(FADM_Control.Formula.HANDER);
                            SetForegroundWindow(ptr1);
                            ShowWindow(ptr1, 3);
                        }
                    }
                   
                }
            }

            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }

        private void MiOut_Click(object sender, EventArgs e)
        {
            //if ((LabStatus.Text == "待机"|| LabStatus.Text == "Standby") && null == FADM_Object.Communal.ReadDyeThread())
            //{
            //    if (MiOut.Checked)
            //    {
            //        MiOut.Checked = false;
            //        string sPath = Environment.CurrentDirectory + "\\Config\\Config.ini";
            //        Lib_File.Ini.WriteIni("Setting", "_b_isOutDrip", "0", sPath);
            //        FADM_Object.Communal._b_isOutDrip = false;
            //    }
            //    else
            //    {
            //        MiOut.Checked = true;
            //        string sPath = Environment.CurrentDirectory + "\\Config\\Config.ini";
            //        Lib_File.Ini.WriteIni("Setting", "_b_isOutDrip", "1", sPath);
            //        FADM_Object.Communal._b_isOutDrip = true;
            //    }
            //}
            //else
            //{
            //    if (Lib_Card.Configure.Parameter.Other_Language == 0)

            //        FADM_Form.CustomMessageBox.Show("待机状态下才能修改", "温馨提示", MessageBoxButtons.OK, false);
            //    else
            //        FADM_Form.CustomMessageBox.Show("Can only be modified in standby mode", "Tips", MessageBoxButtons.OK, false);

            //}
        }

        private void MiLow_Click(object sender, EventArgs e)
        {
            //if ((LabStatus.Text == "待机" || LabStatus.Text == "Standby") && null == FADM_Object.Communal.ReadDyeThread())
            //{
            //    if (MiLow.Checked)
            //    {
            //        MiLow.Checked = false;
            //        string sPath = Environment.CurrentDirectory + "\\Config\\Config.ini";
            //        Lib_File.Ini.WriteIni("Setting", "_b_isLowDrip", "0", sPath);
            //        FADM_Object.Communal._b_isLowDrip = false;
            //    }
            //    else
            //    {
            //        MiLow.Checked = true;
            //        string sPath = Environment.CurrentDirectory + "\\Config\\Config.ini";
            //        Lib_File.Ini.WriteIni("Setting", "_b_isLowDrip", "1", sPath);
            //        FADM_Object.Communal._b_isLowDrip = true;
            //    }
            //}
            //else
            //{
            //    if (Lib_Card.Configure.Parameter.Other_Language == 0)

            //        FADM_Form.CustomMessageBox.Show("待机状态下才能修改", "温馨提示", MessageBoxButtons.OK, false);
            //    else
            //        FADM_Form.CustomMessageBox.Show("Can only be modified in standby mode", "Tips", MessageBoxButtons.OK, false);
            //}
        }

        private void toolStripSplitButton5_Click(object sender, EventArgs e)
        {
            try

            {
                if (FADM_Object.Communal._s_operator == "管理用户" || FADM_Object.Communal._s_operator == "工程师")
                {
                    if (Lib_Card.Configure.Parameter.Other_IsOnlyDrip == 1)
                    {
                        foreach (Control control in this.PnlMain.Controls)
                        {
                            this.PnlMain.Controls.Remove(control);
                            control.Dispose();
                        }
                    }
                    if (Lib_Card.Configure.Parameter.Other_IsOnlyDrip == 1)
                    {
                        FADM_Control.P_Formula formula = new P_Formula(this, FADM_Object.Communal._s_operator);
                        this.PnlMain.Controls.Add(formula);
                        formula.Focus();
                    }
                    else
                    {
                        if (false)
                        {
                           
                        }
                        else {
                            if (FADM_Control.Formula._b_showRun == false)
                            {

                                FADM_Control.Formula formula = new Formula(this);
                                //this.PnlMain.Controls.Add(formula);
                                formula.Show();
                                formula.Focus();
                                IntPtr p1 = formula.Handle;
                                FADM_Control.Formula.HANDER = p1.ToInt32();
                            }
                            else
                            {
                                //WindowsForms10.Window.8.app.0.2804c64_r8_ad1
                                IntPtr ptr1 = new IntPtr(FADM_Control.Formula.HANDER);
                                SetForegroundWindow(ptr1);
                                ShowWindow(ptr1, 3);
                            }

                        }

                        
                    }
                }
            }
            catch(Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }

        private void MiFullDrip_Click(object sender, EventArgs e)
        {
            //if ((LabStatus.Text == "待机" || LabStatus.Text == "Standby") && null == FADM_Object.Communal.ReadDyeThread())
            //{
            //    if (MiFullDrip.Checked)
            //    {
            //        MiFullDrip.Checked = false;
            //        string sPath = Environment.CurrentDirectory + "\\Config\\Config.ini";
            //        Lib_File.Ini.WriteIni("Setting", "_b_isFullDrip", "0", sPath);
            //        FADM_Object.Communal._b_isFullDrip = false;
            //    }
            //    else
            //    {
            //        MiFullDrip.Checked = true;
            //        string sPath = Environment.CurrentDirectory + "\\Config\\Config.ini";
            //        Lib_File.Ini.WriteIni("Setting", "_b_isFullDrip", "1", sPath);
            //        FADM_Object.Communal._b_isFullDrip = true;
            //    }
            //}
            //else
            //{
            //    if (Lib_Card.Configure.Parameter.Other_Language == 0)

            //        FADM_Form.CustomMessageBox.Show("待机状态下才能修改", "温馨提示", MessageBoxButtons.OK, false);
            //    else
            //        FADM_Form.CustomMessageBox.Show("Can only be modified in standby mode", "Tips", MessageBoxButtons.OK, false);
            //}
        }

        private void timerReg_Tick(object sender, EventArgs e)
        {
            //删除30天前开料记录
            DateTime dt = DateTime.Now;
            DateTime dt1= DateTime.Now;
            dt1= dt.AddDays(-30);
            FADM_Object.Communal._fadmSqlserver.ReviseData("delete from brew_run_table where MyDateTime < '" + dt1.ToString() + "'");
            countDown();


        }


        private void BtnNTD_Click(object sender, EventArgs e)
        {
            try
            {
                if (NeedToDo._b_showRun == false)
                {

                    NeedToDo need = new NeedToDo();
                    need.Owner = this;
                    need.Show();
                    need.Focus();
                    NeedToDo.HANDER = need.Handle.ToInt32();
                }
                else
                {
                    IntPtr ptr1 = new IntPtr(NeedToDo.HANDER);
                    SetForegroundWindow(ptr1);
                    ShowWindow(ptr1, 1);
                }
            }

            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }

        }

        private void MiFormulaGroup_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Control control in this.PnlMain.Controls)
                {
                    this.PnlMain.Controls.Remove(control);
                    control.Dispose();
                }

                FADM_Control.FormulaGroup formulaGroup = new FormulaGroup();
                this.PnlMain.Controls.Add(formulaGroup);
                formulaGroup.Focus();
            }

            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }

        private void DyeingAndFixationProcessConfiguration_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Control control in this.PnlMain.Controls)
                {
                    this.PnlMain.Controls.Remove(control);
                    control.Dispose();
                }

                FADM_Control.DyeingAndFixationProcessConfiguration dyeingAndFixationProcessConfiguration = new DyeingAndFixationProcessConfiguration();
                this.PnlMain.Controls.Add(dyeingAndFixationProcessConfiguration);
                dyeingAndFixationProcessConfiguration.Focus();
            }

            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }

        private void DyeingProcessConfiguration_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Control control in this.PnlMain.Controls)
                {
                    this.PnlMain.Controls.Remove(control);
                    control.Dispose();
                }

                FADM_Control.DyeingConfiguration dyeingConfiguration = new DyeingConfiguration();
                this.PnlMain.Controls.Add(dyeingConfiguration);
                dyeingConfiguration.Focus();
            }

            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }

        private void PostTreatmentProcessConfiguration_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Control control in this.PnlMain.Controls)
                {
                    this.PnlMain.Controls.Remove(control);
                    control.Dispose();
                }

                FADM_Control.PostTreatmentConfiguration postTreatment = new PostTreatmentConfiguration();
                this.PnlMain.Controls.Add(postTreatment);
                postTreatment.Focus();
            }

            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }

        private void MiBrewPage_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Control control in this.PnlMain.Controls)
                {
                    this.PnlMain.Controls.Remove(control);
                    control.Dispose();
                }
                FADM_Control.BrewRun brewRun = new FADM_Control.BrewRun();
                this.PnlMain.Controls.Add(brewRun);
                brewRun.Focus();
            }

            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }

        private void MiOut1_Click(object sender, EventArgs e)
        {
            try
            {
                if ((LabStatus.Text == "待机" || LabStatus.Text == "Standby") && null == FADM_Object.Communal.ReadDyeThread())
                {
                    if (MiOut1.Checked)
                    {
                        MiOut1.Checked = false;
                        string s_path = Environment.CurrentDirectory + "\\Config\\Config.ini";
                        Lib_File.Ini.WriteIni("Setting", "IsOutDrip", "0", s_path);
                        FADM_Object.Communal._b_isOutDrip = false;
                    }
                    else
                    {
                        MiOut1.Checked = true;
                        string s_path = Environment.CurrentDirectory + "\\Config\\Config.ini";
                        Lib_File.Ini.WriteIni("Setting", "IsOutDrip", "1", s_path);
                        FADM_Object.Communal._b_isOutDrip = true;
                    }
                }
                else
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)

                        FADM_Form.CustomMessageBox.Show("待机状态下才能修改", "温馨提示", MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("Can only be modified in standby mode", "Tips", MessageBoxButtons.OK, false);

                }
            }

            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }

        private void MiLow1_Click(object sender, EventArgs e)
        {
            try
            {
                if ((LabStatus.Text == "待机" || LabStatus.Text == "Standby") && null == FADM_Object.Communal.ReadDyeThread())
                {
                    if (MiLow1.Checked)
                    {
                        MiLow1.Checked = false;
                        string s_path = Environment.CurrentDirectory + "\\Config\\Config.ini";
                        Lib_File.Ini.WriteIni("Setting", "IsLowDrip", "0", s_path);
                        FADM_Object.Communal._b_isLowDrip = false;
                    }
                    else
                    {
                        MiLow1.Checked = true;
                        string s_path = Environment.CurrentDirectory + "\\Config\\Config.ini";
                        Lib_File.Ini.WriteIni("Setting", "IsLowDrip", "1", s_path);
                        FADM_Object.Communal._b_isLowDrip = true;
                    }
                }
                else
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)

                        FADM_Form.CustomMessageBox.Show("待机状态下才能修改", "温馨提示", MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("Can only be modified in standby mode", "Tips", MessageBoxButtons.OK, false);
                }
            }

            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }

        private void MiFullDrip1_Click(object sender, EventArgs e)
        {
            try
            {
                if ((LabStatus.Text == "待机" || LabStatus.Text == "Standby") && null == FADM_Object.Communal.ReadDyeThread())
                {
                    if (MiFullDrip1.Checked)
                    {
                        MiFullDrip1.Checked = false;
                        string s_path = Environment.CurrentDirectory + "\\Config\\Config.ini";
                        Lib_File.Ini.WriteIni("Setting", "IsFullDrip", "0", s_path);
                        FADM_Object.Communal._b_isFullDrip = false;
                    }
                    else
                    {
                        MiFullDrip1.Checked = true;
                        string s_path = Environment.CurrentDirectory + "\\Config\\Config.ini";
                        Lib_File.Ini.WriteIni("Setting", "IsFullDrip", "1", s_path);
                        FADM_Object.Communal._b_isFullDrip = true;
                    }
                }
                else
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)

                        FADM_Form.CustomMessageBox.Show("待机状态下才能修改", "温馨提示", MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("Can only be modified in standby mode", "Tips", MessageBoxButtons.OK, false);
                }
            }

            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }

        private void toolStripSplitButton4_DropDownOpening(object sender, EventArgs e)
        {
            if (Lib_Card.Configure.Parameter.Other_UseAbs == 0)
                MiAbsPage.Visible = false;
        }

        private void MiAbsPage_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Control control in this.PnlMain.Controls)
                {
                    this.PnlMain.Controls.Remove(control);
                    control.Dispose();
                }
                FADM_Control.HistoryAbsData historyAbsData = new FADM_Control.HistoryAbsData(this);
                this.PnlMain.Controls.Add(historyAbsData);
                historyAbsData.Focus();
            }

            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }

        private void BtnResetAbs_Click(object sender, EventArgs e)
        {
            try
            {
                FADM_Object.Communal._b_absErr = false;
                Thread P_thd_abs = new Thread(SmartDyeing.FADM_Auto.MyAbsorbance.Absorbance);
                P_thd_abs.IsBackground = true;
                P_thd_abs.Start();
            }

            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }
        private string _s_m_route_ASL = null;
        static ReaderWriterLockSlim _logWriteLock2 = new ReaderWriterLockSlim();
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Communal._b_getFile_sec)
            {
                try
                {
                    try
                    {
                        if (!_logWriteLock2.IsWriteLockHeld)
                        {
                            _logWriteLock2.EnterWriteLock(); //进入写入锁
                            string[] files = Directory.GetFiles(this._s_m_route_ASL, "DripMachine*.txt");
                            if (files.Length > 0)
                            {
                                Communal._b_getFile_sec = false;
                                Thread th = new Thread(insert2);
                                th.IsBackground = true;
                                th.Start(files);
                                //TmrFGY.Enabled = false;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        if (_logWriteLock2.IsWriteLockHeld)
                            _logWriteLock2.ExitWriteLock(); //退出写入锁
                    }
                    finally
                    {
                        if (_logWriteLock2.IsWriteLockHeld)
                            _logWriteLock2.ExitWriteLock(); //退出写入锁

                    }


                }
                catch (Exception ex)
                {
                    FADM_Form.CustomMessageBox.Show(ex.Message, "TmrFGY_Tick", MessageBoxButtons.OK, true);
                    //TmrFGY.Enabled = false;
                }
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            foreach (Control control in this.PnlMain.Controls)
            {
                this.PnlMain.Controls.Remove(control);
                control.Dispose();
            }

            
                FADM_Control.ABS_Formula main = new ABS_Formula(this, FADM_Object.Communal._s_operator);
                this.PnlMain.Controls.Add(main);
                main.Focus();
            
        }

        private void MiOutAllowDrip_Click(object sender, EventArgs e)
        {
            try
            {
                if ((LabStatus.Text == "待机" || LabStatus.Text == "Standby") && null == FADM_Object.Communal.ReadDyeThread())
                {
                    if (MiOutAllowDrip.Checked)
                    {
                        MiOutAllowDrip.Checked = false;
                        string s_path = Environment.CurrentDirectory + "\\Config\\Config.ini";
                        Lib_File.Ini.WriteIni("Setting", "IsOutDripAllow", "0", s_path);
                        FADM_Object.Communal._b_isOutDripAllow = false;
                    }
                    else
                    {
                        MiOutAllowDrip.Checked = true;
                        string s_path = Environment.CurrentDirectory + "\\Config\\Config.ini";
                        Lib_File.Ini.WriteIni("Setting", "IsOutDripAllow", "1", s_path);
                        FADM_Object.Communal._b_isOutDripAllow = true;
                    }
                }
                else
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)

                        FADM_Form.CustomMessageBox.Show("待机状态下才能修改", "温馨提示", MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("Can only be modified in standby mode", "Tips", MessageBoxButtons.OK, false);

                }
            }

            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }

        private void MiLowAllowDrip_Click(object sender, EventArgs e)
        {
            try
            {
                if ((LabStatus.Text == "待机" || LabStatus.Text == "Standby") && null == FADM_Object.Communal.ReadDyeThread())
                {
                    if (MiLowAllowDrip.Checked)
                    {
                        MiLowAllowDrip.Checked = false;
                        string s_path = Environment.CurrentDirectory + "\\Config\\Config.ini";
                        Lib_File.Ini.WriteIni("Setting", "IsLowDripAllow", "0", s_path);
                        FADM_Object.Communal._b_isLowDripAllow = false;
                    }
                    else
                    {
                        MiLowAllowDrip.Checked = true;
                        string s_path = Environment.CurrentDirectory + "\\Config\\Config.ini";
                        Lib_File.Ini.WriteIni("Setting", "IsLowDripAllow", "1", s_path);
                        FADM_Object.Communal._b_isLowDripAllow = true;
                    }
                }
                else
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)

                        FADM_Form.CustomMessageBox.Show("待机状态下才能修改", "温馨提示", MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("Can only be modified in standby mode", "Tips", MessageBoxButtons.OK, false);
                }
            }

            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }

        private void timerNeedToDo_Tick(object sender, EventArgs e)
        {
            try
            {
                bool b_have = false;
                try
                {

                    foreach (string i in Lib_Card.CardObject.keyValuePairs.Keys)
                    {
                        if (i == null)
                        {
                            try
                            {
                                Lib_Card.CardObject.keyValuePairs.Remove(i);

                            }
                            catch
                            {
                                Lib_Log.Log.writeLogException("keyValuePairs 空值不能删除：");
                            }
                            continue;
                        }

                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        {
                            if ((!Lib_Card.CardObject.keyValuePairs[i].Info.Contains("放布")) && (!Lib_Card.CardObject.keyValuePairs[i].Info.Contains("出布")) && (!Lib_Card.CardObject.keyValuePairs[i].Info.Contains("前光幕")) && (!Lib_Card.CardObject.keyValuePairs[i].Info.Contains("后光幕")) && (!Lib_Card.CardObject.keyValuePairs[i].Info.Contains("左光幕")) && (!Lib_Card.CardObject.keyValuePairs[i].Info.Contains("右光幕")) && (!Lib_Card.CardObject.keyValuePairs[i].Info.Contains("急停")) && (!Lib_Card.CardObject.keyValuePairs[i].Info.Contains("左门")) && (!Lib_Card.CardObject.keyValuePairs[i].Info.Contains("右门")) && (!Lib_Card.CardObject.keyValuePairs[i].Info.Contains("批次滴液完成")) && (!Lib_Card.CardObject.keyValuePairs[i].Info.Contains("针检完成")) && (!Lib_Card.CardObject.keyValuePairs[i].Info.Contains("自检完成")) && (!Lib_Card.CardObject.keyValuePairs[i].Info.Contains("水校正完成")) && (!Lib_Card.CardObject.keyValuePairs[i].Info.Contains("水验证完成")))
                            {
                                b_have = true;
                                break;
                            }
                        }
                        else
                        {
                            if ((!Lib_Card.CardObject.keyValuePairs[i].Info.Contains("cloth placement")) && (!Lib_Card.CardObject.keyValuePairs[i].Info.Contains("cup discharge")) && (!Lib_Card.CardObject.keyValuePairs[i].Info.Contains("door")) && (!Lib_Card.CardObject.keyValuePairs[i].Info.Contains("obstruction")) && (!Lib_Card.CardObject.keyValuePairs[i].Info.Contains("Emergency stop")) && (!Lib_Card.CardObject.keyValuePairs[i].Info.Contains("Needle examination completed")) && (!Lib_Card.CardObject.keyValuePairs[i].Info.Contains("Self inspection completed")) && (!Lib_Card.CardObject.keyValuePairs[i].Info.Contains("Water correction completed")) && (!Lib_Card.CardObject.keyValuePairs[i].Info.Contains("Water verification completed")))
                            {
                                b_have = true;
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Lib_Log.Log.writeLogException("NeedToDo InfoUpdate：" + ex.ToString());
                }
                if (b_have)
                {
                    if (NeedToDo._b_showRun == false)
                    {

                        NeedToDo need = new NeedToDo();
                        need.Owner = this;
                        need.Show();
                        need.Focus();
                        NeedToDo.HANDER = need.Handle.ToInt32();
                    }
                    else
                    {
                        IntPtr ptr1 = new IntPtr(NeedToDo.HANDER);
                        SetForegroundWindow(ptr1);
                        ShowWindow(ptr1, 1);
                    }
                }
            }

            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }

        private void timer_Update_Tick(object sender, EventArgs e)
        {
            if (FADM_Object.Communal.ReadMachineStatus() == 0)
            {
                SmartDyeing.FADM_Control.P_Formula.P_bl_update = true;
            }
        }

        private void CupStatus_Click(object sender, EventArgs e)
        {
            IntPtr ptr;
            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                ptr = FindWindow(null, "CupArea");
            else
                ptr = FindWindow(null, "CupArea");

            if (ptr == IntPtr.Zero)
            {
                new FADM_Form.CupArea().Show();
            }
            else
            {
                //先删除页面，再重新打开
                PostMessage(ptr, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                new FADM_Form.CupArea().Show();

            }
        }

        private void MiAbsDebug_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Control control in this.PnlMain.Controls)
                {
                    this.PnlMain.Controls.Remove(control);
                    control.Dispose();
                }

                FADM_Control.AbsHMIMain absDebug = new AbsHMIMain();
                this.PnlMain.Controls.Add(absDebug);
                absDebug.Focus();
            }
            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }

        private void MiABSProcess_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Control control in this.PnlMain.Controls)
                {
                    this.PnlMain.Controls.Remove(control);
                    control.Dispose();
                }

                FADM_Control.ABSConfig absProcess = new ABSConfig();
                this.PnlMain.Controls.Add(absProcess);
                absProcess.Focus();
            }
            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.ToString(), "warm", MessageBoxButtons.OK, true);
            }
        }
    }
}
