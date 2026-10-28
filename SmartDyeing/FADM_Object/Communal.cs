using BLECode;
using CHNSpec.Device.Bluetooth;
using CHNSpec.Device.COM;
using CHNSpec.Device.Models;
using CHNSpec.Device.Models.Enums;
using Lib_DataBank.MySQL;
using SmartDyeing.FADM_Auto;
using SmartDyeing.FADM_Control;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SmartDyeing.FADM_Object.Communal;


namespace SmartDyeing.FADM_Object
{
    public class Communal
    {
        //蓝牙分光仪
        public static BluetoothHelper _helper = new BluetoothHelper();

        //分光仪
        public static SerialPortHelper _Comhelper = new SerialPortHelper();

        private static readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

        private static readonly ReaderWriterLockSlim _rwWaterLock = new ReaderWriterLockSlim();

        /// <summary>
        /// 全自动滴液机数据库对象
        /// </summary>
        public static Lib_DataBank.SQLServer _fadmSqlserver { get; set; }

        /// <summary>
        /// 梅特勒天平对象
        /// </summary>
        public static Lib_SerialPort.Balance.METTLER Mettler { get; set; }

        /// <summary>
        /// 日本星光天平对象
        /// </summary>
        public static Lib_SerialPort.Balance.SHINKO Shinko { get; set; }


        //
        /// <summary>
        /// plc modbus tcp对象
        /// </summary>
        public static TCPModBus _tcpModBus { get; set; }

        /// <summary>
        /// 开料机对象（TCP）
        /// </summary>
        public static TCPModBus _tcpModBusBrew { get; set; }

        /// <summary>
        /// 开料机对象(串口)
        /// </summary>
        public static Lib_SerialPort.HMI.BrewHMI MyBrew { get; set; } = null;

        /// <summary>
        /// 称粉机对象
        /// </summary>
        public static HMITCPModBus Powder { get; set; } = null;


        /// <summary>
        /// 吸光度机对象
        /// </summary>
        public static TCPModBus _tcpModBusAbs { get; set; }


        public static SmartDyeing.FADM_Object.MyRegister _softReg { get; set; }

        public static string _s_machineCode { get; set; }



        public static string _s_version { get; set; }


        /// <summary>
        /// plc天平读数
        /// </summary>
        public static string _s_balanceValue { get; set; }

        /// <summary>
        /// 天平复检重量
        /// </summary>
        public static double _d_reviewBalance { get; set; }


        /// <summary>
        /// 机台状态：
        /// 0：待机
        /// 1：手动回零
        /// 2：定坐标
        /// 3：联动
        /// 4：定点移动
        /// 5：校正
        /// 6：针检
        /// 7：滴液
        /// 8：异常
        /// 9：滴液完成
        /// 10:复位
        /// 11:自检
        /// </summary>
        private static int _i_machineStatus { get; set; }

        public static int ReadMachineStatus()
        {
            _rwLock.EnterReadLock();
            try
            {
                return _i_machineStatus;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        public static void WriteMachineStatus(int value)
        {
            _rwLock.EnterWriteLock();
            try
            {
                _i_machineStatus = value;
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        //string参数是母液瓶号，List<int> 里有两个数据，第一个值是是否可以放针，第二个参数是是否已经夹过没针筒(这个判断能不能去洗针筒)
        private static readonly ConcurrentDictionary<string, List<int>> _sharedDictionary =
    new ConcurrentDictionary<string, List<int>>();

        // 线程安全的添加操作
        public static void AddOrUpdateWash(string key, List<int> value)
        {
            try
            {
                _sharedDictionary.AddOrUpdate(key, value, (k, old) => value);
            }
            catch  { }
        }

        // 线程安全的读取操作
        public static bool GetValueWash(string key, out List<int> value)
        {
            try
            {
                if (_sharedDictionary.TryGetValue(key, out value))
                {
                    return true;
                }
                return false;
            }
            catch
            {
                value = null;
                return false; }
        }

        public static void DeleteWash(string key)
        {
            try
            {
                _sharedDictionary.TryRemove(key, out List<int> value);
            }
            catch { }
        }

        public static string GetFirstWash()
        {
            try
            {
                foreach (string s in _sharedDictionary.Keys)
                {
                    if (_sharedDictionary[s][1] == 1)
                    {
                        return s;
                    }
                }
                return "0";
            }
            catch { return "0"; }
        }

        public static double dBalanceValue = 0.00;

        /// <summary>
        /// 操作母液瓶号
        /// </summary>
        public static int _i_optBottleNum { get; set; }

        /// <summary>
        /// 操作滴液杯号
        /// </summary>
        public static int _i_OptCupNum { get; set; }

        /// <summary>
        /// 开关盖杯号
        /// </summary>
        public static int _i_OptCoverCupNum { get; set; }

        /// <summary>
        /// 上下气缸类型
        /// </summary>
        public static int _i_cylinderVersion = 1;

        /// <summary>
        /// 自检数组
        /// </summary>
        //public static double[] _da_self = {   11.00, 5.00, 2.00, 0.50, 4.00 };


        //public static double[] _da_self = {1.00, 11.00, 5.00, 2.00, 0.50, 1.00 };

        public static double[] _da_self = { 1.00, 0.50, 2.00, 5.00, 11.00, 2.00 };


        //public static double[] _da_self = { 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00, 1.00 };
        /// <summary>
        /// 加水方式
        /// 0:不带罐
        /// 1：带罐
        /// </summary>
        public static int _i_waterWay = 1;

        /// <summary>
        /// 手自动状态
        /// </summary>
        public static bool _b_auto = true;


        /// <summary>
        /// 天平报警
        /// </summary>
        public static bool _b_balanceAlarm = false;

        /// <summary>
        /// 废液桶最大重量
        /// </summary>
        public static double _d_balanceMaxWeight = 1000;

        /// <summary>
        /// 暂停
        /// </summary>
        public static bool _b_pause = false;

        /// <summary>
        /// 停止
        /// </summary>
        public static bool _b_stop = false;

        /// <summary>
        /// 注液类型
        /// 0：气缸上限位注液
        /// 1：气缸中限位注液
        /// </summary>
        public static int _i_infusionType = 0;

        /// <summary>
        /// 染色机状态
        /// 0：下线
        /// 1：上线
        /// </summary>
        public static int[] _ia_dyeStatus = { 0, 0, 0, 0, 0, 0 };

        /// <summary>
        /// 染色机通讯失败时间
        /// </summary>
        public static string[] _sa_dyeConFTime = { "", "", "", "", "", "" };

        /// <summary>
        /// 染色机通讯失败次数
        /// </summary>
        public static int[] _ai_iArray = { 0, 0, 0, 0, 0, 0 };

        /// <summary>
        /// 报警
        /// </summary>
        public static string[] _sa_dyeAlarm = { "", "", "", "", "", "" };

        /// <summary>
        /// 报警
        /// </summary>
        public static int[] _ia_alarmNum = { 0,0,0,0,0,0 };

        /// <summary>
        /// 报警(翻转缸)
        /// </summary>
        public static int[] _ia_alarmNum1 = new int[Lib_Card.Configure.Parameter.Machine_Cup_Total];

        /// <summary>
        /// 报警(翻转缸)
        /// </summary>
        public static string[] _sa_dyeAlarm1 = new string[Lib_Card.Configure.Parameter.Machine_Cup_Total];

        /// <summary>
        /// 报警(翻转缸)
        /// </summary>
        public static int[] _ia_alarmNum2 = new int[Lib_Card.Configure.Parameter.Machine_Cup_Total];

        /// <summary>
        /// 报警(翻转缸)
        /// </summary>
        public static string[] _sa_dyeAlarm2 = new string[Lib_Card.Configure.Parameter.Machine_Cup_Total];

        /// <summary>
        /// 自动开料报警1
        /// </summary>
        public static int _i_alarmAutoBrew1=0 ;

        public static string _s_AlarmAutoBrew1="";

        /// <summary>
        /// 自动开料报警2
        /// </summary>
        public static int _i_alarmAutoBrew2 = 0;

        public static string _s_AlarmAutoBrew2 = "";

        /// <summary>
        /// 染色机对象
        /// </summary>
        //public static Lib_SerialPort.HMI.HMI DyeHMI { get; set; } = null;

        /// <summary>
        /// 染色机对象tcp通讯
        /// </summary>
        public static HMITCPModBus _tcpDyeHMI1 { get; set; } = null;

        /// <summary>
        /// 染色机对象tcp通讯
        /// </summary>
        public static HMITCPModBus _tcpDyeHMI2 { get; set; } = null;

        /// <summary>
        /// 染色机对象tcp通讯
        /// </summary>
        public static HMITCPModBus _tcpDyeHMI3 { get; set; } = null;

        /// <summary>
        /// 染色机对象tcp通讯
        /// </summary>
        public static HMITCPModBus _tcpDyeHMI4 { get; set; } = null;

        /// <summary>
        /// 染色机对象tcp通讯
        /// </summary>
        public static HMITCPModBus _tcpDyeHMI5 { get; set; } = null;

        /// <summary>
        /// 染色机对象tcp通讯
        /// </summary>
        public static HMITCPModBus _tcpDyeHMI6 { get; set; } = null;


        /// <summary>
        /// 称布触摸屏对象
        /// </summary>
        public static HMITCPModBus HMIBaClo { get; set; } = null;
        





        /// <summary>
        /// 开料机对象
        /// </summary>
        //public static Lib_SerialPort.HMI.BrewHMI MyBrew { get; set; } = null;

        /// <summary>
        /// 扫码枪对象
        /// </summary>
        //public static Lib_SerialPort.Scan.Scan MyScan { get; set; } = null;



        /// <summary>
        /// 滴液完成的杯号
        /// </summary>
        public static List<int> _lis_dripSuccessCup = new List<int>() ;

        /// <summary>
        /// 滴液失败的杯号
        /// </summary>
        public static List<int> _lis_dripFailCup = new List<int>();

        /// <summary>
        /// 滴液失败完成的杯号
        /// </summary>
        public static List<int> _lis_dripFailCupFinish = new List<int>();

        /// <summary>
        /// 染色停止的杯号
        /// </summary>
        public static List<int> _lis_dripStopCup = new List<int>();

        /// <summary>
        /// 需要洗杯的杯号
        /// </summary>
        public static List<int> _lis_washCup = new List<int>();

        /// <summary>
        /// 洗杯完成的杯号
        /// </summary>
        public static List<int> _lis_washCupFinish = new List<int>();

        /// <summary>
        /// 洗杯完成的杯号(使用在加水时区分)
        /// </summary>
        public static List<int> _lis_addwashCupFinish = new List<int>();

        /// <summary>
        /// 前洗杯的杯号
        /// </summary>
        public static List<int> _lis_ForwordwashCup = new List<int>();

        /// <summary>
        /// 已发启动信号
        /// </summary>
        public static List<int> _lis_SendReadyCup = new List<int>();

        /// <summary>
        /// 高温洗杯的杯号
        /// </summary>
        public static List<int> _lis_HighWashCup = new List<int>();

        /// <summary>
        /// 重新下发的杯号
        /// </summary>
        public static List<int> _lis_ReSendCup = new List<int>();

        /// <summary>
        /// 打板机急停状态  true 急停按下 false 急停松开
        /// </summary>
        public static bool _b_isDyeStop = false;

        /// <summary>
        /// 打板机是否有急停  true 有 false 无
        /// </summary>
        public static bool _b_isHaveDyeStop = false;

        /// <summary>
        /// 是否有自动开料  true 有 false 无
        /// </summary>
        public static bool _b_isHaveAutoBrew = false;

        /// <summary>
        /// 是否有自动洗瓶  true 有 false 无
        /// </summary>
        public static bool _b_isHaveAutoWashBottle = false;

        /// <summary>
        /// 自动开料是否洗针筒  true 有 false 无
        /// </summary>
        public static bool _b_isAutoWashSys = true;

        /// <summary>
        /// 染色线程
        /// </summary>
        private static Thread DyeThread = null;
        public static Thread ReadDyeThread()
        {
            _rwLock.EnterReadLock();
            try
            {
                return DyeThread;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        public static void WriteDyeThread(Thread value)
        {
            _rwLock.EnterWriteLock();
            try
            {
                DyeThread = value;
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 自动开料加水线程
        /// </summary>
        private static Thread WaterThread = null;
        public static Thread ReadWaterThread()
        {
            _rwWaterLock.EnterReadLock();
            try
            {
                return WaterThread;
            }
            finally
            {
                _rwWaterLock.ExitReadLock();
            }
        }

        public static void WriteWaterThread(Thread value)
        {
            _rwWaterLock.EnterWriteLock();
            try
            {
                WaterThread = value;
            }
            finally
            {
                _rwWaterLock.ExitWriteLock();
            }
        }



        /// <summary>
        /// 滴液线程可以等待标志位
        /// </summary>
        private static bool _b_dripWait = false;
        public static bool ReadDripWait()
        {
            _rwLock.EnterReadLock();
            try
            {
                return _b_dripWait;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        public static void WriteDripWait(bool value)
        {
            _rwLock.EnterWriteLock();
            try
            {
                _b_dripWait = value;
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }


        /// <summary>
        /// 开料机通讯中断标志位
        /// </summary>
        public static bool _b_brewErr = false;

        /// <summary>
        /// 吸光度机通讯中断标志位
        /// </summary>
        public static bool _b_absErr = false;


        /// <summary>
        /// 是否打开等待列表页面
        /// </summary>
        public static bool _b_isOpenWaitList = false;

        /// <summary>
        /// 工艺重名名
        /// </summary>
        public static string _s_reName = null;


        /// <summary>
        /// 是否是染固色完成正在完成
        /// </summary>
        public static bool _b_finshRun = true;


        /// <summary>
        /// 是否正在备份数据
        /// </summary>
        public static bool _b_isBackUp = false;

        /// <summary>
        /// 是否点击开始滴液或自动滴液
        /// </summary>
        public static bool _b_isDripping = false;

        /// <summary>
        /// 当前操作员
        /// </summary>
        public static string _s_operator = null;

        /// <summary>
        /// 液量低是否滴液
        /// </summary>
        public static bool _b_isLowDrip = false;

        /// <summary>
        /// 超出生命周期是否滴液
        /// </summary>
        public static bool _b_isOutDrip = false;

        /// 液量低是否允许滴液
        /// </summary>
        public static bool _b_isLowDripAllow = false;

        /// <summary>
        /// 超出生命周期是否允许滴液
        /// </summary>
        public static bool _b_isOutDripAllow = false;

        /// <summary>
        /// 满量程滴液
        /// </summary>
        public static bool _b_isFullDrip = true;

        /// <summary>
        /// 是否复检天平清零
        /// </summary>
        public static bool _b_isZero = false;

        /// <summary>
        /// 显示注册信息
        /// </summary>
        public static string _s_regInfo = null;

        /// <summary>
        /// 是否倒序
        /// </summary>
        public static bool _b_isDesc = false;

        /// <summary>
        /// 滴液区杯号
        /// </summary>
        public static List<int> _lis_dripCupNum = new List<int>();


        /// <summary>
        /// 逻辑滴液区杯号 为了配合称布
        /// </summary>
        public static Dictionary<int, List<int>> my_lis_dripCupNum = new Dictionary<int, List<int>>();

        /// <summary>
        /// 后处理杯号
        /// </summary>
        public static List<int> _lis_dyeCupNum = new List<int>();

        /// <summary>
        /// 副杯杯号
        /// </summary>
        public static List<int> _lis_deputyCupNum = new List<int>();

        /// <summary>
        /// 主杯杯号
        /// </summary>
        public static List<int> _lis_firstCupNum = new List<int>();

        /// <summary>
        /// 主副杯对应
        /// </summary>
        public static Dictionary<int, int> _dic_first_second = new Dictionary<int, int>();

        /// <summary>
        /// 精密机杯号
        /// </summary>
        public static List<int> _lis_PrecisionCupNum = new List<int>();

        /// <summary>
        /// 大小杯对应
        /// </summary>
        public static Dictionary<int, int> _dic_big_small_cup = new Dictionary<int, int>();

        /// <summary>
        /// 杯号和序号对应
        /// </summary>
        public static Dictionary<int, int> _dic_cup_index = new Dictionary<int, int>();

        /// <summary>
        /// 后处理杯号和序号对应（用于后处理杯号状态显示）
        /// </summary>
        public static Dictionary<int, int> _dic_dyecup_index = new Dictionary<int, int>();

        /// <summary>
        /// 后处理杯号类型
        /// </summary>
        public static Dictionary<int, int> _dic_dyeType = new Dictionary<int, int>();

        /// <summary>
        /// 自动开料机延时发送停止，正在加粉，加液移动母液瓶时需要完成才能停止
        /// </summary>
        public static List<int> _b_SendStopCupNum = new List<int>();

        public static bool _b_registerOld = false;//老校验方式

        public static bool _b_isDripAll = false;//是否分区域滴液，false:分区域;true:全部一起滴液

        public static bool _b_isAssitantFirst = false;//是否先滴助剂，false:先滴染料;true:先滴助剂

        public static bool _b_isNeedCheck = true;//开料完是否需要针检

        public static bool _b_isUseMotherDate = false;//是否使用开料瓶号开料日期

        public static bool _b_isFinishSend = true;//是否单杯完成就下发

        public static bool _b_isUpdateNotDripList = false;//是否需要刷新尚未滴液列表

        public static bool _b_isNetWork = false;//网络推送

        public static bool _b_isDripReserveFirst = true;//是否先滴预留到天平

        public static bool _b_isDebug = false;//是否调试模式

        public static bool _b_isBalanceInDrip = true;//天平是否放置滴液区

        public static bool _b_isCupAreaOnly = false;//配液杯是否独立页面

        public static bool _b_isUseAutoChoose = true;//是否启动自动选择

        public static int _i_AskTimes = 2;//自动选择轮询时间

        public static bool _b_isNewSet = true;//母液区摆设(0:以前传统滴液机母液区摆设 1:PLC版母液区摆设,原点在10号母液瓶)

        public static bool _b_isGetDryClamp = false;//是否拿着干布夹子

        public static bool _b_isGetWetClamp = false;//是否拿着湿布夹子

        public static bool _b_isGetPowerClamp = false;//是否拿着粉筒夹子

        public static bool _b_isGetPHClamp = false;//是否拿着PH夹子

        public static bool _b_isGetSyringes = false;//是否拿着针筒(ABS抽液针筒)

        public static bool _b_isSaveRealConcentration = false;//保存时是否将设定浓度保存到实际浓度中

        public static bool _b_isUseClamp = false;//是否使用夹子自动放布

        public static bool _b_isUseClampOut = false;//是否使用夹子自动出布

        public static bool _b_isUseClampOutBig = true;//4,6杯大杯是否使用夹子自动出布

        public static bool _b_isUsePH = false;//是否使用自动测PH

        public static bool _b_isPHSys = false;//PH计类型，0小针筒 1大针筒

        public static bool _b_isUseAuto = true;//是否使用自动启动

        public static bool _b_isexpired = false;//是否过期

        public static bool _b_closebrew = false;//关闭开料机通讯

        public static bool _b_isAutoAbs = true;//有吸光度机时，是否开料就加入检测

        public static bool[] _b_use = { false, false, false, false };//判断当前轮吸光度混合液是否滴液

        public static int _i_needPulse = 0;//当前母液瓶第一杯虚假脉冲
        public static int _i_needPulseCupNumber = 0;//当前母液瓶第一杯杯号

        public static List<int> _lis_warmBottle = new List<int>();//记录一轮循环预滴液时判断液量低

        public static bool _b_getFile = true;//是否开始拿文件
        public static bool _b_getFile_sec = true;//是否开始拿文件
        public static bool[] _b_isFullTest = { false, false };//是否吸光度全测量

        public static double _d_ppm = 30;//吸光度稀释ppm值

        public static double _d_absMax = 0.004;//吸光度对比值

        public static double _d_TestSpan = 30;//吸光度测试间隔

        public static double _d_abs_total = 50;//吸光度总液量

        public static int _i_abs_DissolvingType = 0;//0:溶解剂加水口 1:加水口 2:针筒

        public static bool _b_isMove = false;//后处理是否移动过，用于判断是否需要回待机位

        public static string  _s_absPath = "";//吸光度结果导出路径

        public static bool _b_isWaitDrip = false;//是否等待滴液

        public static bool _b_isAllowDrip = false;//是否可以滴液

        public static double _d_abs_sub = 40;//吸光度白点差值判断范围

        public static double _d_abs_mixture = 0;//混合液计算的浓度

        public static double _d_abs_mixture_2 = 0;//另外一个杯混合液计算的浓度

        public static bool _b_isUnitChange = false;//助剂可以切换%单位

        public static string URL = ""; //服务器url

        public static int _b_isDyMin = 0; //滴液位置从多少开始


        public static int _b_DyeCupNum = 0; //打板的杯数


        public static int _i_Head_Ip = 40;//表头开始识别码开始位置
        public static int _i_Head_Ip_Len = 40;//表头开始识别码长度

        public static int _i_Head_FormulaCode = 40;//表头配方代码开始位置
        public static int _i_Head_FormulaCode_Len = 40;//表头配方代码长度

        public static int _i_Head_VersionNum = 40;//表头配方版本开始位置
        public static int _i_Head_VersionNum_Len = 40;//表头配方版本长度

        public static int _i_Head_Count = 40;//表头染助劑支數开始位置
        public static int _i_Head_Count_Len = 40;//表头染助劑支數长度

        public static int _i_Head_Unit = 40;//表头配方單位开始位置
        public static int _i_Head_Unit_Len = 40;//表头配方單位长度

        public static int _i_Head_Index = 40;//表头滴定序號开始位置
        public static int _i_Head_Index_Len = 40;//表头滴定序號长度

        public static int _i_Head_FormulaName = 40;//表头配方名称开始位置
        public static int _i_Head_FormulaName_Len = 40;//表头配方名称长度

        public static int _i_Head_ClothWeight = 40;//表头布重开始位置
        public static int _i_Head_ClothWeight_Len = 40;//表头布重长度

        public static int _i_Head_TotalWeight = 40;//表头总浴量开始位置
        public static int _i_Head_TotalWeight_Len = 40;//表头总浴量长度

        public static int _i_Head_AddWater = 40;//表头是否加水开始位置
        public static int _i_Head_AddWater_Len = 40;//表头是否加水长度

        public static int _i_Head_ConAdd = 40;//表头是否续加开始位置
        public static int _i_Head_ConAdd_Len = 40;//表头是否续加长度

        public static int _i_Head_MNum = 40;//表头机台编号开始位置
        public static int _i_Head_MNum_Len = 40;//表头机台编号长度

        public static int _i_Head_Date = 40;//表头日期开始位置
        public static int _i_Head_Date_Len = 40;//表头日期长度

        public static int _i_Head_Code = 40;//表头染程开始位置
        public static int _i_Head_Code_Len = 40;//表头染程长度

        public static int _i_Head_Type = 40;//表头配方類型开始位置
        public static int _i_Head_Type_Len = 40;//表头配方類型长度

        public static int _i_Head_Drip = 40;//表头是否滴過开始位置
        public static int _i_Head_Drip_Len = 40;//表头是否滴過长度

        public static int _i_Head_SIN = 40;//表头结束识别码开始位置
        public static int _i_Head_SIN_Len = 40;//表头结束识别码长度

        public static int _i_Detail_Ip = 40;//详情开始识别码开始位置
        public static int _i_Detail_Ip_Len = 40;//详情开始识别码长度

        public static int _i_Detail_FormulaCode = 40;//详情配方代码开始位置
        public static int _i_Detail_FormulaCode_Len = 40;//详情配方代码长度

        public static int _i_Detail_VersionNum = 40;//详情配方版本开始位置
        public static int _i_Detail_VersionNum_Len = 40;//详情配方版本长度

        public static int _i_Detail_Index = 40;//详情滴定序號开始位置
        public static int _i_Detail_Index_Len = 40;//详情滴定序號长度

        public static int _i_Detail_Unit = 40;//详情配方單位开始位置
        public static int _i_Detail_Unit_Len = 40;//详情配方單位长度

        public static int _i_Detail_Num = 40;//详情库存编号开始位置
        public static int _i_Detail_Num_Len = 40;//详情库存编号长度

        public static int _i_Detail_AssistantCode = 40;//详情助剂代码开始位置
        public static int _i_Detail_AssistantCode_Len = 40;//详情助剂代码长度

        public static int _i_Detail_RealConcentration = 40;//详情助剂浓度开始位置
        public static int _i_Detail_RealConcentration_Len = 40;//详情助剂浓度长度

        public static int _i_Detail_SIN = 40;//详情结束识别码开始位置
        public static int _i_Detail_SIN_Len = 40;//详情结束识别码长度

        /// <summary>
        /// 滴液过程是否先加水后加染料
        /// </summary>
        public static bool _b_isAddWaterFirst = true;

        /// <summary>
        /// 排序
        /// </summary>
        public static bool _b_isSort = true;

        /// <summary>
        /// 滴液区需要手动输入杯号
        /// </summary>
        public static bool _b_isDripNeedCupNum = false;

        /// <summary>
        /// 是否使用称布系统
        /// </summary>
        public static bool _b_isUseCloth = false;

        /// <summary>
        /// 是否使用称粉机
        /// </summary>
        public static bool _b_isUsePower = false;

        /// <summary>
        /// 称粉机是否使用AB粉
        /// </summary>
        public static bool _b_PowerAB = false;

        /// <summary>
        /// 母液瓶右击时，是否母液瓶显示状态
        /// </summary>
        public static bool _b_isShowBottleStatus = false;

        /// <summary>
        /// 母液瓶独立使用预滴液功能
        /// </summary>
        public static bool _b_isAloneDripReserve = false;

        /// <summary>
        /// 是否自动弹出待办事项
        /// </summary>
        public static bool _b_NeedTodo = false;

        /// <summary>
        /// 是否修正数据
        /// </summary>
        public static bool _b_isCorrect = false;

        /// <summary>
        /// 滴液过程，存在空杯是否可以加入批次
        /// </summary>
        public static bool _b_isDripAddBatch = false;

        /// <summary>
        /// 流量计脉冲
        /// </summary>
        public static int i_flowPulse = 0;

        /// <summary>
        /// 
        /// <summary>
        /// 是否使用流量计系统
        /// </summary>
        public static bool _b_isUseLLJ = false;

        /// <summary>
        /// 是否使用新的水校正方式
        /// </summary>
        public static bool _b_isUseNewCorrectingWater = false;

        /// <summary>
        /// 是否加水复称
        /// </summary>
        public static bool _b_isWaterRecheck = true;

        /// <summary>
        /// 是否使用转盘水校正
        /// </summary>
        public static bool _b_isUseTurntableCorrectingWater = false;

        /// <summary>
        /// 是否有洗针筒
        /// </summary>
        public static bool _b_isHasWashSyringe = false;

        /// <summary>
        /// 是否有带盖加药
        /// </summary>
        public static bool _b_isCloseAddMed = false;


        /// <summary>
        /// 是否滴液优先后处理
        /// </summary>
        public static bool _b_isDripPriority = true;

        /// <summary>
        /// 是否屏蔽保存配方按钮
        /// </summary>
        public static bool _b_isBlockSaveButton = false;

        /// <summary>
        /// 是否60秒记录一次温度，默认30
        /// </summary>
        public static bool _b_is60Mark = false;


        /// <summary>
        /// 染助剂资料页面是否显示G/L或者WATER单位
        /// </summary>
        public static bool _b_isAssShowWater = false;

        /// <summary>
        ///是否只用于显示信息(把所有操作都屏蔽)
        /// </summary>
        public static bool _b_isJustShowInfo = false;

        /// <summary>
        ///保存最少滴液量报警
        /// </summary>
        public static double _d_AlarmDropWeight = 0.1;

        /// <summary>
        ///机台编号
        /// </summary>
        public static string _s_MachineNumber = "01";


        /// <summary>
        /// 是否在放布确认前可添加副杯滴液
        /// </summary>
        public static bool _b_isNeedConfirm = false;

        /// <summary>
        /// 配方界面上的浴比更改后 底下的所有工艺浴比跟着变化
        /// </summary>
        public static bool _b_isBathRatioTxtDyBath = false;

        /// <summary>
        /// 是否单独使用开料机(只有开料机)
        /// </summary>
        public static bool _b_isUseBrewOnly = false;

        /// <summary>
        /// 天平复称时是否先稳定再读数
        /// </summary>
        public static bool _b_isStableRead = false;

        /// <summary>
        /// 是否高温洗杯
        /// </summary>
        public static bool _b_isHighWash = false;

        /// <summary>
        ///是否使用分光仪测量LAB
        /// </summary>
        public static bool _b_isUseLAB = false;

        /// <summary>
        /// 分光仪通讯方式 0 蓝牙 1 usb
        /// </summary>
        public static int _i_LabMode = 0;

        /// <summary>
        ///助剂页面是否按照助剂代码来排序
        /// </summary>
        public static bool _b_isAssOrderByCode = false;


        /// <summary>
        /// 区域1触摸屏版本
        /// </summary>
        public static string _s_TouchVer1 = "";

        /// <summary>
        /// 区域1板卡1版本
        /// </summary>
        public static string _s_CardOneVer1 = "";

        /// <summary>
        /// 区域2板卡2版本
        /// </summary>
        public static string _s_CardTwoVer1 = "";

        /// <summary>
        /// 区域2触摸屏版本
        /// </summary>
        public static string _s_TouchVer2 = "";

        /// <summary>
        /// 区域2板卡1版本
        /// </summary>
        public static string _s_CardOneVer2 = "";

        /// <summary>
        /// 区域2板卡2版本
        /// </summary>
        public static string _s_CardTwoVer2 = "";

        /// <summary>
        /// 区域3触摸屏版本
        /// </summary>
        public static string _s_TouchVer3 = "";

        /// <summary>
        /// 区域3板卡1版本
        /// </summary>
        public static string _s_CardOneVer3 = "";

        /// <summary>
        /// 区域3板卡2版本
        /// </summary>
        public static string _s_CardTwoVer3 = "";

        /// <summary>
        /// 区域4触摸屏版本
        /// </summary>
        public static string _s_TouchVer4 = "";

        /// <summary>
        /// 区域4板卡1版本
        /// </summary>
        public static string _s_CardOneVer4 = "";

        /// <summary>
        /// 区域4板卡2版本
        /// </summary>
        public static string _s_CardTwoVer4 = "";

        /// <summary>
        /// 区域5触摸屏版本
        /// </summary>
        public static string _s_TouchVer5 = "";

        /// <summary>
        /// 区域5板卡1版本
        /// </summary>
        public static string _s_CardOneVer5 = "";

        /// <summary>
        /// 区域5板卡2版本
        /// </summary>
        public static string _s_CardTwoVer5 = "";

        /// <summary>
        /// 区域6触摸屏版本
        /// </summary>
        public static string _s_TouchVer6 = "";

        /// <summary>
        /// 区域6板卡1版本
        /// </summary>
        public static string _s_CardOneVer6 = "";

        /// <summary>
        /// 区域6板卡2版本
        /// </summary>
        public static string _s_CardTwoVer6 = "";

        /// <summary>
        /// 最大母液瓶Y轴坐标
        /// </summary>
        public static int _i_Max_Y = 0;

        /// <summary>
        /// 是否显示取小样测PH工艺
        /// </summary>
        public static bool _b_isShowSample = false;

        /// <summary>
        /// 是否有AB助剂
        /// </summary>
        public static bool _b_isUseABAssistant = false;

        /// <summary>
        /// AB助剂数量
        /// </summary>
        public static int _i_ABAssistantCount = 2;

        /// <summary>
        /// 抛出异常中英文对接
        /// </summary>
        public static Dictionary<string, string> _dic_warning = new Dictionary<string, string>();

        /// <summary>
        /// 后处理加药处理
        /// </summary>
        public static Dictionary<int, List<int>> _dic_cup_bottle = new Dictionary<int, List<int>>();



        private static bool _b_readTcpState = true; //进行Plc访问的可允许证

        /// <summary>
        /// 动作错误编号
        /// </summary>
        public static Dictionary<int, string> _dic_errModbusNo = new Dictionary<int, string>();

        /// <summary>
        /// 动作错误编号
        /// </summary>
        public static Dictionary<int, string> _dic_errModbusNoNew = new Dictionary<int, string>();

        /// <summary>
        /// 是否正在写入交互信息
        /// </summary>
        public static bool _b_isWriting = false;

        /// <summary>
        /// 是否已刷新新数据
        /// </summary>
        public static bool _b_isUpdateNewData = false;

        /// <summary>
        /// 调试页面下900-949地址数据
        /// </summary>
        public static int[] _ia_d900 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,0,0,0,0,0,0,0,0,0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,0};

        /// <summary>
        /// 调试页面吸光度数据13188-13227地址数据
        /// </summary>
        public static int[] _ia_d13188 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        /// <summary>
        /// 调试页面吸光度数据13088开始100个寄存器地址数据
        /// </summary>
        public static int[] _ia_d13088 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                           0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

        /// <summary>
        /// 调试页面自动开料机22688-22762地址数据
        /// </summary>
        public static int[] _ia_d22688 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,0, 0, 0, 0, 0, 0, 0, 0, 0, 0,0, 0, 0, 0, 0, 0, 0, 0, 0, 0,0, 0, 0, 0, 0, 0, 0, 0, 0, 0,0,0,0,0,0 };

        /// <summary>
        /// 调试页面自动洗瓶22938-22947地址数据
        /// </summary>
        public static int[] _ia_d22938 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        /// <summary>
        /// 吸光度机是否使用水来测试基准点
        /// </summary>
        public static bool _b_isUseWaterTestBase = false;


        /// <summary>
        /// ABS调试页面是否打开
        /// </summary>
        public static bool _b_IsABSOpen = false;

        /// <summary>
        /// 蜂鸣器状态 0:初始状态 1:已打开 2：已关闭
        /// </summary>
        public static int _i_bType = 0;

        /// <summary>
        /// 蜂鸣器打开是否发送 
        /// </summary>
        public static bool _b_isBSendOpen = false;

        /// <summary>
        /// 蜂鸣器关闭是否发送 
        /// </summary>
        public static bool _b_isBSendClose = false;

        /// <summary>
        /// PLC版本号
        /// </summary>
        public static string _s_plcVersion = "";

        /// <summary>
        /// 开料机版本号
        /// </summary>
        public static string _s_brewVersion = "";

        /// <summary>
        /// 是否批量开料模式
        /// </summary>
        public static bool _b_isBatBrew = true ;

        /// <summary>
        /// 是否需要冷却
        /// </summary>
        public static bool _b_isNeedCool = false;

        /// <summary>
        /// 开始批量开料模式
        /// </summary>
        public static bool _b_isStartBatBrew = false;

        /// <summary>
        /// 开料批次搬移
        /// </summary>
        public static bool _b_isMoveing = false;

        /// <summary>
        /// 开料机状态(0待机 1:补水前状态 2:加粉完成到出瓶前 3:出瓶中 4:开料完成重新放针)
        /// </summary>
        public static int _i_BrewStatus = 0;


        /// <summary>
        /// 母液区第一个瓶号
        /// </summary>
        public static int _i_MotherBottleNum = 61;

        /// <summary>
        /// 粉筒与母液瓶是否分区域
        /// </summary>
        public static bool _b_isPartition = false;

        /// <summary>
        /// 自动开料是否需要搬运针筒
        /// </summary>
        public static bool _b_isNeedMoveSy = true;

        /// <summary>
        /// 当前一轮自动开料总步数
        /// </summary>
        public static int _i_BrewTotalStep = 0;

        /// <summary>
        /// 对应开料机数量
        /// </summary>
        public static int _i_TotalBrewMac = 1;

        /// <summary>
        /// 当前开料机编号
        /// </summary>
        public static int _i_CurrentBrewNum = 1;

        /// <summary>
        /// 使用原针筒开稀
        /// </summary>
        public static bool _b_OriginalSyBrew = false;



        public static bool ReadTcpStatus()
        {
            _rwLock.EnterReadLock();
            try
            {
                return _b_readTcpState;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }
        public static void WriteTcpStatus(bool value)
        {
            _rwLock.EnterWriteLock();
            try
            {
                _b_readTcpState = value;
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 吸光度数据交互结构
        /// </summary>
        public struct AbsData
        {
            /// <summary>
            /// 工位状态
            /// </summary>
            public string _s_currentState;

            /// <summary>
            /// 动作请求
            /// </summary>
            public string _s_request;

            /// <summary>
            /// 保存数据请求
            /// </summary>
            public string _s_datarequest;

            /// <summary>
            /// 母液瓶号
            /// </summary>
            public string _s_boottlenum;

            /// <summary>
            /// 数据数量
            /// </summary>
            public string _s_totaldata;

            /// <summary>
            /// 报警
            /// </summary>
            public string _s_warm;

            //申请加药状态 0 代表可接收信号写入数据库 1代表写入数据库完成 2 代表执行对应操作完成(接收到2后通过刷新才能变为0)
            public int _i_requesadd;

            //历史状态 0 正常结束 1需要前洗杯
            public string _s_history;
        }


        public struct sAddArg
        {
            public string _obj_batchName;//  _obj_batchName：所属批次
            public string _s_syringeType;// _s_syringeType：针筒类型
            public int _i_minBottleNo;// _i_minBottleNo：母液瓶号
            public int _i_pulseT;// _i_pulseT：总脉冲
            public int _i_adjust;// _i_adjust：校正脉冲
            public string _s_unitOfAccount;//  _s_unitOfAccount：单位

            public Dictionary<int, int> _dic_pulse;//  _dic_pulse：配液杯对应加药脉冲
            public Dictionary<int, double> _dic_water;//  directoryWeight：配液杯对应加水重
            public Dictionary<int, int> _dic_step;//  _dic_pulse：配液杯对应步号

            public int _i_AddType;// _i_Type：吸光度加药类型 1是母液 2 是溶解剂

            public int _i_stationnum;// _i_stationnum：自动开料工位号
        }

        /// <summary>
        /// 加药封装
        /// Type：类型，0代表滴液调用 1代表后处理调用
        ///  i_special:是否跳过天平复称和预滴液 1是跳过 0是不跳过
        /// _i_minBottleNo：母液瓶号
        /// _s_syringeType：针筒类型
        /// _i_pulseT：总脉冲
        /// _i_adjust:校正脉冲
        ///  _s_unitOfAccount：单位
        ///  iLowSrart：当前状态
        ///  _obj_batchName：所属批次
        ///  _dic_pulse：配液杯对应加药脉冲
        ///  directoryWeight：配液杯对应加药重
        ///  lWater:后处理加药时先加水的重量
        /// </summary>
        /// /// <returns>0：正常；-1：夹不到针筒退出；-2：收到退出消息</returns>

        public static int AddMac(sAddArg o, ref Dictionary<int, double> directoryReturn, int i_type, int i_special, int i_lowSrart)
        {
            List<int> lis_data = new List<int>();
            string s_batchName = o._obj_batchName;
            int i_minBottleNo = o._i_minBottleNo;
            string s_syringeType = o._s_syringeType;
            int i_pulseT = o._i_pulseT;
            int i_adjust = o._i_adjust;
            string s_unitOfAccount = o._s_unitOfAccount;

            Dictionary<int, int> dic_pulse = o._dic_pulse;
            Dictionary<int, double> dic_water = o._dic_water;
            Dictionary<int, int> dic_step = o._dic_step;

            Thread thread = null;
            //记录这次抽液脉冲
            int i_lastExtractionPulse = 0;
            //记录当前母液瓶存量
            double d_cu = 0.0;

            string s_sql = "SELECT * FROM bottle_details WHERE BottleNum = " + i_minBottleNo + ";";
            DataTable dt_bottle_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

            if(dt_bottle_details.Rows.Count >0)
            {
                d_cu = Convert.ToDouble(dt_bottle_details.Rows[0]["CurrentWeight"].ToString());
            }

            //如果是母液瓶999就先去拿针筒
            if (i_minBottleNo == 999)
            {
                int i_xStart = 0, i_yStart = 0;
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
                    try
                    {
                        int i_mRes3 = MyModbusFun.GetSyringes(i_xStart, i_yStart);
                        if (-2 == i_mRes3)
                            throw new Exception("收到退出消息");
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
                                if (Communal.GetValueWash(i_minBottleNo.ToString(), out value))
                                {
                                    //标记夹不到针筒
                                    value[1] = 0;
                                    Communal.AddOrUpdateWash(i_minBottleNo.ToString(), value);
                                }
                            }
                            return -1;

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
                                if (Communal.GetValueWash(i_minBottleNo.ToString(), out value))
                                {
                                    //标记夹不到针筒
                                    value[1] = 0;
                                    Communal.AddOrUpdateWash(i_minBottleNo.ToString(), value);
                                }
                            }
                            return -1;

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
                                if (Communal.GetValueWash(i_minBottleNo.ToString(), out value))
                                {
                                    //标记夹不到针筒
                                    value[1] = 0;
                                    Communal.AddOrUpdateWash(i_minBottleNo.ToString(), value);
                                }
                            }
                            return -1;

                        }
                        else
                            throw;
                    }
                    
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "拿针筒完成");
                }
            }

        label9:
            //移动到母液瓶
            if (FADM_Auto.Drip._b_dripStop)
            {
                FADM_Object.Communal._b_stop = true;
            }

            //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
            //{
            //    //富士伺服在下面判断 天平状态 原有不动 绿维的放在上面 并且置位 是否回原点 绿维的放移动机械手前面

            //    //判断是否异常
            //    FADM_Object.Communal.BalanceState("滴液");
            //}

            int i_mRes;
            if (i_minBottleNo == 999)
            {
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找1号(吸光度杯)");
                FADM_Object.Communal._i_optBottleNum = i_minBottleNo;
                i_mRes = MyModbusFun.TargetMove(10, 1, 0);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达1号(吸光度杯)");
            }
            else
            {

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找" + i_minBottleNo + "号母液瓶");
                FADM_Object.Communal._i_optBottleNum = i_minBottleNo;
                i_mRes = MyModbusFun.TargetMove(0, i_minBottleNo, 1);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达" + i_minBottleNo + "号母液瓶");
            }

            //抽液
            if (FADM_Auto.Drip._b_dripStop)
            {
                FADM_Object.Communal._b_stop = true;
            }
            int iExtractionPulse = 0;
            if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
            {
                iExtractionPulse = (i_pulseT + i_adjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1));
                //FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "脉冲(" + i_pulseT + ")"+ "校正值(" + i_adjust + ")" + "Other_Z_BackPulse(" + Lib_Card.Configure.Parameter.Other_Z_BackPulse + ")" + "Other_S_MaxPulse(" + Lib_Card.Configure.Parameter.Other_S_MaxPulse + ")" + "_b_isFullDrip(" + (Communal._b_isFullDrip?"1":"0") + ")" + "_b_isDripReserveFirst(" + (FADM_Object.Communal._b_isDripReserveFirst ? "1" : "0") + ")");
                if (iExtractionPulse - Lib_Card.Configure.Parameter.Other_Z_BackPulse > Lib_Card.Configure.Parameter.Other_S_MaxPulse)
                {
                    if (FADM_Object.Communal._b_isFullDrip)
                    {
                        //超出单针筒上限
                        iExtractionPulse = Lib_Card.Configure.Parameter.Other_S_MaxPulse + Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                    }
                    else
                    {
                        iExtractionPulse = i_adjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1);
                        int i_temp = iExtractionPulse;
                        for (int i = 0; i < dic_pulse.Count; i++)
                        {
                            var v1 = dic_pulse.ElementAt(i);

                            i_temp = iExtractionPulse;

                            if (i_temp + v1.Value - Lib_Card.Configure.Parameter.Other_Z_BackPulse > Lib_Card.Configure.Parameter.Other_S_MaxPulse)
                            {
                                //如果需加量大于10g，直接先加
                                //if(v1.Value/1.0/ _i_adjust > Lib_Card.Configure.Parameter.Other_SplitValue)
                                if (i_temp == i_adjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1))
                                {
                                    iExtractionPulse = Lib_Card.Configure.Parameter.Other_S_MaxPulse + Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                                }
                                else
                                {
                                    //如果一筒抽不完就分2次，一次抽完就留下一次独立抽
                                    if (v1.Value + i_adjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1) - Lib_Card.Configure.Parameter.Other_Z_BackPulse > Lib_Card.Configure.Parameter.Other_S_MaxPulse)
                                    {
                                        iExtractionPulse = Lib_Card.Configure.Parameter.Other_S_MaxPulse + Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                                    }
                                }
                                break;
                            }
                            else
                            {
                                iExtractionPulse += v1.Value;
                            }
                        }
                    }
                }
                if (FADM_Object.Communal._b_isDripReserveFirst)
                    iExtractionPulse += i_adjust;
                //需要重新判断是否液量低
                if(i_lowSrart ==0)
                {
                    //先计算上次母液扣减量，滴液和复检量
                    d_cu = d_cu - (i_lastExtractionPulse - (i_lastExtractionPulse==0?0:i_adjust)) * 1.0 / i_adjust;
                    //判断下次是否液量不足
                    if (d_cu-  iExtractionPulse *1.0/ i_adjust < Lib_Card.Configure.Parameter.Other_Bottle_MinWeight && FADM_Object.Communal._b_isLowDrip)
                    {
                        //等待复称完成
                        if (null != thread)
                            thread.Join();
                        //解锁6工位
                        MyModbusFun.UnLockSix();
                        for (int i = 0; i < lis_data.Count; i++)
                        {
                            //对应值复称值写入
                            directoryReturn.Add(lis_data[i], _d_reviewBalance);
                        }

                        lis_data.Clear();

                        //记录第一个杯需加量脉冲
                        _i_needPulse = dic_pulse.First().Value;
                        _i_needPulseCupNumber = dic_pulse.First().Key;
                        MyModbusFun.MyMachineReset1(); //复位
                        return -2;

                    }
                }
                i_lastExtractionPulse = iExtractionPulse;
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抽液启动(" + iExtractionPulse + ")");

            label10:
                try
                {
                    iExtractionPulse = iExtractionPulse - Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                    if (i_type == 1)
                        i_mRes = MyModbusFun.Extract(iExtractionPulse, s_unitOfAccount.Equals("g/l") ? true : false, 0); //抽液
                    else
                    {
                        if (i_minBottleNo == 999)
                        {
                            //1号吸光度杯停止搅拌
                            int[] values = new int[1];
                            values[0] = 1;
                            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                            {
                                FADM_Object.Communal._tcpModBusAbs.ReConnect();
                            }

                            FADM_Object.Communal._tcpModBusAbs.Write(806, values);

                            i_mRes = MyModbusFun.AbsExtract(iExtractionPulse, true, 0); //抽液

                            values[0] = 0;
                            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                            {
                                FADM_Object.Communal._tcpModBusAbs.ReConnect();
                            }
                            FADM_Object.Communal._tcpModBusAbs.Write(806, values);
                        }

                        else
                            i_mRes = MyModbusFun.Extract(iExtractionPulse, true, 0); //抽液
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
                        if (Communal.GetValueWash(i_minBottleNo.ToString(), out value))
                        {
                            //如果之前标记过夹不到针筒，现在抽液完成，可改为夹到针筒
                            if (value[1] == 0)
                            {
                                value[1] = 1;
                                Communal.AddOrUpdateWash(i_minBottleNo.ToString(), value);
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
                            if (Communal.GetValueWash(i_minBottleNo.ToString(), out value))
                            {
                                //标记夹不到针筒
                                value[1] = 0;
                                Communal.AddOrUpdateWash(i_minBottleNo.ToString(), value);
                            }
                        }
                        return -1;

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
                            if (Communal.GetValueWash(i_minBottleNo.ToString(), out value))
                            {
                                //标记夹不到针筒
                                value[1] = 0;
                                Communal.AddOrUpdateWash(i_minBottleNo.ToString(), value);
                            }
                        }
                        return -1;

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
                            if (Communal.GetValueWash(i_minBottleNo.ToString(), out value))
                            {
                                //标记夹不到针筒
                                value[1] = 0;
                                Communal.AddOrUpdateWash(i_minBottleNo.ToString(), value);
                            }
                        }
                        return -1;

                    }
                    else
                        throw;
                }

            }
            else
            {
                iExtractionPulse = (i_pulseT + i_adjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1));
                if (iExtractionPulse - Lib_Card.Configure.Parameter.Other_Z_BackPulse > Lib_Card.Configure.Parameter.Other_B_MaxPulse)
                {
                    if (FADM_Object.Communal._b_isFullDrip)
                    {
                        //超出单针筒上限
                        iExtractionPulse = Lib_Card.Configure.Parameter.Other_B_MaxPulse + Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                    }
                    else
                    {
                        iExtractionPulse = i_adjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1);
                        int i_temp = iExtractionPulse;
                        for (int i = 0; i < dic_pulse.Count; i++)
                        {
                            var v1 = dic_pulse.ElementAt(i);

                            i_temp = iExtractionPulse;

                            if (i_temp + v1.Value - Lib_Card.Configure.Parameter.Other_Z_BackPulse > Lib_Card.Configure.Parameter.Other_B_MaxPulse)
                            {
                                ////如果需加量大于10g，直接先加
                                //if (v1.Value / 1.0 / _i_adjust >= Lib_Card.Configure.Parameter.Other_SplitValue)
                                if (i_temp == i_adjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1))
                                {
                                    iExtractionPulse = Lib_Card.Configure.Parameter.Other_B_MaxPulse + Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                                }
                                else
                                {
                                    if (v1.Value + i_adjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1) - Lib_Card.Configure.Parameter.Other_Z_BackPulse > Lib_Card.Configure.Parameter.Other_B_MaxPulse)
                                    {
                                        iExtractionPulse = Lib_Card.Configure.Parameter.Other_B_MaxPulse + Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                                    }
                                }
                                break;
                            }
                            else
                            {
                                iExtractionPulse += v1.Value;
                            }
                        }
                    }
                }
                if (FADM_Object.Communal._b_isDripReserveFirst)
                    iExtractionPulse += i_adjust;
                if (i_lowSrart == 0)
                {
                    //先计算上次母液扣减量，滴液和复检量
                    d_cu = d_cu - (i_lastExtractionPulse - (i_lastExtractionPulse == 0 ? 0 : i_adjust)) * 1.0 / i_adjust;
                    //判断下次是否液量不足
                    if (d_cu - iExtractionPulse * 1.0 / i_adjust < Lib_Card.Configure.Parameter.Other_Bottle_MinWeight && FADM_Object.Communal._b_isLowDrip)
                    {
                        //等待复称完成
                        if (null != thread)
                            thread.Join();
                        //解锁6工位
                        MyModbusFun.UnLockSix();
                        for (int i = 0; i < lis_data.Count; i++)
                        {
                            //对应值复称值写入
                            directoryReturn.Add(lis_data[i], _d_reviewBalance);
                        }

                        lis_data.Clear();

                        //记录第一个杯需加量脉冲
                        _i_needPulse = dic_pulse.First().Value;
                        _i_needPulseCupNumber = dic_pulse.First().Key;
                        MyModbusFun.MyMachineReset1(); //复位
                        return -3;

                    }
                }
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抽液启动(" + iExtractionPulse + ")");

            label11:
                try
                {
                    iExtractionPulse = iExtractionPulse - Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                    if (i_type == 1)
                        i_mRes = MyModbusFun.Extract(iExtractionPulse, s_unitOfAccount.Equals("g/l") ? true : false, 1); //抽液
                    else
                    {
                        if (i_minBottleNo == 999)
                        {
                            //1号吸光度杯停止搅拌
                            int[] values = new int[1];
                            values[0] = 1;
                            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                            {
                                FADM_Object.Communal._tcpModBusAbs.ReConnect();
                            }

                            FADM_Object.Communal._tcpModBusAbs.Write(806, values);

                            i_mRes = MyModbusFun.AbsExtract(iExtractionPulse, true, 1); //抽液

                            values[0] = 0;
                            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                            {
                                FADM_Object.Communal._tcpModBusAbs.ReConnect();
                            }

                            FADM_Object.Communal._tcpModBusAbs.Write(806, values);
                        }
                        else
                            i_mRes = MyModbusFun.Extract(iExtractionPulse, true, 1); //抽液
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
                        if (Communal.GetValueWash(i_minBottleNo.ToString(), out value))
                        {
                            //如果之前标记过夹不到针筒，现在抽液完成，可改为夹到针筒
                            if (value[1] == 0)
                            {
                                value[1] = 1;
                                Communal.AddOrUpdateWash(i_minBottleNo.ToString(), value);
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
                            if (Communal.GetValueWash(i_minBottleNo.ToString(), out value))
                            {
                                //标记夹不到针筒
                                value[1] = 0;
                                Communal.AddOrUpdateWash(i_minBottleNo.ToString(), value);
                            }
                        }
                        return -1;


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
                            if (Communal.GetValueWash(i_minBottleNo.ToString(), out value))
                            {
                                //标记夹不到针筒
                                value[1] = 0;
                                Communal.AddOrUpdateWash(i_minBottleNo.ToString(), value);
                            }
                        }
                        return -1;


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
                            if (Communal.GetValueWash(i_minBottleNo.ToString(), out value))
                            {
                                //标记夹不到针筒
                                value[1] = 0;
                                Communal.AddOrUpdateWash(i_minBottleNo.ToString(), value);
                            }
                        }
                        return -1;


                    }
                    else
                        throw;
                }
            }
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抽液完成");

            if (null != thread)
                thread.Join();
            //解锁6工位
            MyModbusFun.UnLockSix();
            for (int i = 0; i < lis_data.Count; i++)
            {
                //对应值复称值写入
                directoryReturn.Add(lis_data[i], _d_reviewBalance);
            }

            lis_data.Clear();

            //Lib_SerialPort.Balance.METTLER.bReSetSign = true;

            if (FADM_Object.Communal._b_isDripReserveFirst)
            {



                //移动到天平位，先滴预留量
                if (FADM_Auto.Drip._b_dripStop)
                {
                    FADM_Object.Communal._b_stop = true;
                }
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找天平位");
                //判断是否异常

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

                double d_blBalanceValueStart;
                if (Lib_Card.Configure.Parameter.Machine_Type == 0)
                {
                    d_blBalanceValueStart = Lib_Card.Configure.Parameter.Machine_BalanceType == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal.dBalanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal.dBalanceValue));
                }
                else
                {
                    d_blBalanceValueStart = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal._s_balanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal._s_balanceValue));
                }

                int i_zPulse1 = 0;
                //label98:
                if (-1 == MyModbusFun.GetZPosition(ref i_zPulse1))
                    throw new Exception("驱动异常");
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "Z轴脉冲" + i_zPulse1);
                //    if (i_zPulse1 > 0)
                //        goto label98;
                int i_infusionPulse1 = 0;
                if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                    i_infusionPulse1 = i_zPulse1 - i_adjust;
                else
                    i_infusionPulse1 = i_zPulse1 - i_adjust;

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "废液注液启动(" + i_infusionPulse1 + ")");
                if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                    i_mRes = MyModbusFun.Shove(i_infusionPulse1, 0);
                else
                    i_mRes = MyModbusFun.Shove(i_infusionPulse1, 1);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "废液注液完成");

                //等待一秒就当数据稳定
                Thread.Sleep(1000);

                double d_blBalanceValueEnd;
                if (Lib_Card.Configure.Parameter.Machine_Type == 0)
                {
                    d_blBalanceValueEnd = Lib_Card.Configure.Parameter.Machine_BalanceType == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal.dBalanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal.dBalanceValue));
                }
                else
                {
                    d_blBalanceValueEnd = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal._s_balanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal._s_balanceValue));
                }
                double d_blDif = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blBalanceValueEnd - d_blBalanceValueStart)) : Convert.ToDouble(string.Format("{0:F3}", d_blBalanceValueEnd - d_blBalanceValueStart));

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "预滴量:" + d_blDif);
                //解锁6工位
                MyModbusFun.UnLockSix();

                //预滴量大于0小于2才扣减
                if (d_blDif > 0 && d_blDif < 2)
                {
                    //母液瓶扣减1克
                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_blDif + "  " +
                        "WHERE BottleNum = '" + i_minBottleNo + "';");
                }
                //滴到废液桶时，数据差小于0.5，就当液量不够，提示
                if (d_blDif < 0.5)
                {
                    //记录第一个杯需加量脉冲
                    _i_needPulse = dic_pulse.First().Value;
                    _i_needPulseCupNumber = dic_pulse.First().Key;
                    MyModbusFun.MyMachineReset1(); //复位
                    return -2;
                }

            }

            //移动到配液杯

            List<int> lis_ints = new List<int>();
        label12:
            if (FADM_Auto.Drip._b_dripStop)
            {
                FADM_Object.Communal._b_stop = true;
            }
            int i_zPulse = 0;
        label99:
            if (-1 == MyModbusFun.GetZPosition(ref i_zPulse))
                throw new Exception("驱动异常");
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "Z轴脉冲" + i_zPulse);
            //if (i_zPulse > 0)
            //    goto label99;
            if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
            {

                if (i_adjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1) == i_zPulse)
                {
                    //当前针筒已滴完，进入复检
                    goto label13;
                }
            }
            else
            {
                if (i_adjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1) == i_zPulse)
                {
                    //当前针筒已滴完，进入复检
                    goto label13;
                }

            }

            if (dic_pulse.Count == 0)
            {
                //如果已经没有要添加的杯，就直接进去
                goto label13;
            }
            if (i_type == 1)
            {
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找" + dic_pulse.First().Key + "号配液杯");

                FADM_Object.Communal._i_OptCupNum = dic_pulse.First().Key;
                i_mRes = MyModbusFun.TargetMove(1, dic_pulse.First().Key, 0);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达" + dic_pulse.First().Key + "号配液杯");
            }
            else
            {
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找" + dic_pulse.First().Key + "号(吸光度杯)");

                FADM_Object.Communal._i_OptCupNum = dic_pulse.First().Key;
                i_mRes = MyModbusFun.TargetMove(10, dic_pulse.First().Key, 0);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达" + dic_pulse.First().Key + "号(吸光度杯)");
            }

            //判断是否加水


            if (0 < dic_water[dic_pulse.First().Key])
            {
                //new Water().Add(lWater.First(), "Dail");
                double d_addWaterTime = MyModbusFun.GetWaterTime(dic_water[dic_pulse.First().Key]);//加水时间
                if (d_addWaterTime <= 32)
                {
                    i_mRes = MyModbusFun.AddWater(d_addWaterTime);
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                }
                else
                {
                    double d = 32;
                    while (true)
                    {
                        if (d_addWaterTime > 32)
                        {
                            //每次减32s
                            i_mRes = MyModbusFun.AddWater(d);
                            if (-2 == i_mRes)
                                throw new Exception("收到退出消息");
                        }
                        else
                        {
                            i_mRes = MyModbusFun.AddWater(d_addWaterTime);
                            if (-2 == i_mRes)
                                throw new Exception("收到退出消息");
                            break;
                        }
                        d_addWaterTime -= d;
                    }
                }
                //加完水后会把加水量置为0，否则一针筒加药加不完的第二针筒加药时会重复加水
                if (i_type == 1)
                {
                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                    "UPDATE cup_details SET TotalWeight = TotalWeight + " + dic_water[dic_pulse.First().Key] + " WHERE CupNum = " + dic_pulse.First().Key + ";");
                }

                dic_water[dic_pulse.First().Key] = 0;
            }

            //注液
            if (FADM_Auto.Drip._b_dripStop)
            {
                FADM_Object.Communal._b_stop = true;
            }
            int iInfusionPulse = 0;
            if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
            {
                if (i_zPulse - i_adjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1) >= dic_pulse.First().Value)
                {
                    //剩余量大于等于需加量
                    iInfusionPulse = i_zPulse - dic_pulse.First().Value;


                    ////母液瓶扣减
                    //FADM_Object.Communal._fadmSqlserver.ReviseData(
                    //    "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + directoryWeight.First().Value + " " +
                    //    "WHERE BottleNum = '" + _i_minBottleNo + "';");


                    ////置位完成标志位
                    //FADM_Object.Communal._fadmSqlserver.ReviseData(
                    //    "UPDATE drop_details SET Finish = 1 WHERE BatchName = '" + _obj_batchName + "' AND " +
                    //    "BottleNum = " + _i_minBottleNo + " AND CupNum = " + _dic_pulse.First().Key + ";");

                    i_pulseT -= dic_pulse.First().Value;
                    lis_data.Add(dic_pulse.First().Key);
                    dic_pulse.Remove(dic_pulse.First().Key);
                }
                else
                {
                    iInfusionPulse = i_adjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1);
                    dic_pulse[dic_pulse.First().Key] -= (i_zPulse - i_adjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1));
                    i_pulseT -= (i_zPulse - i_adjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1));
                }



                if (i_type == 1)
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液启动(" + iInfusionPulse + ")");
                    if (i_special == 1)
                    {
                        i_mRes = MyModbusFun.DyeShove(iInfusionPulse, 0);
                    }
                    else
                    {
                        i_mRes = MyModbusFun.Shove(iInfusionPulse, 0);
                    }
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                }
                else
                {
                    if (Lib_Card.Configure.Parameter.Machine_SlopDown == 1)
                    {
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液启动(" + iInfusionPulse + ")");
                        i_mRes = MyModbusFun.AbsShove(iInfusionPulse, 0);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");
                    }
                    else
                    {
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液启动(" + iInfusionPulse + ")");
                        i_mRes = MyModbusFun.Shove(iInfusionPulse, 0);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");
                    }
                }

            }
            else
            {
                if (i_zPulse - i_adjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1) >= dic_pulse.First().Value)
                {
                    //剩余量大于等于需加量
                    iInfusionPulse = i_zPulse - dic_pulse.First().Value;

                    ////母液瓶扣减
                    //FADM_Object.Communal._fadmSqlserver.ReviseData(
                    //    "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + directoryWeight.First().Value + " " +
                    //    "WHERE BottleNum = '" + _i_minBottleNo + "';");

                    ////置位完成标志位
                    //FADM_Object.Communal._fadmSqlserver.ReviseData(
                    //    "UPDATE drop_details SET Finish = 1 WHERE BatchName = '" + _obj_batchName + "' AND " +
                    //    "BottleNum = " + _i_minBottleNo + " AND CupNum = " + _dic_pulse.First().Key + ";");


                    i_pulseT -= dic_pulse.First().Value;
                    lis_data.Add(dic_pulse.First().Key);
                    dic_pulse.Remove(dic_pulse.First().Key);
                }
                else
                {
                    iInfusionPulse = i_adjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1);
                    dic_pulse[dic_pulse.First().Key] -= (i_zPulse - i_adjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1));
                    i_pulseT -= (i_zPulse - i_adjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1));
                }
                if (i_type == 1)
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液启动(" + iInfusionPulse + ")");
                    if (i_special == 1)
                    {
                        i_mRes = MyModbusFun.DyeShove(iInfusionPulse, 1);
                    }
                    else
                    {
                        i_mRes = MyModbusFun.Shove(iInfusionPulse, 1);
                    }
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                }
                else
                {
                    if (Lib_Card.Configure.Parameter.Machine_SlopDown == 1)
                    {
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液启动(" + iInfusionPulse + ")");
                        i_mRes = MyModbusFun.AbsShove(iInfusionPulse, 1);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");
                    }
                    else
                    {
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液启动(" + iInfusionPulse + ")");
                        i_mRes = MyModbusFun.Shove(iInfusionPulse,1);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");
                    }
                }
            }


            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液完成");


            goto label12;

        //移动到天平位
        label13:
            if (i_special == 1|| i_special == 2)
            {
                if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                    _d_reviewBalance = Lib_Card.Configure.Parameter.Correcting_S_Weight;
                else
                    _d_reviewBalance = Lib_Card.Configure.Parameter.Correcting_B_Weight;

                //带带盖加药时，需要洗针筒才放回去
                if (FADM_Object.Communal._b_isHasWashSyringe)
                {
                    if (i_special == 1)
                    {
                        //移动到洗针位置
                        i_mRes = MyModbusFun.TargetMove(12, 0, 0);
                        if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                        {
                            MyModbusFun.WashSyringes(0);
                        }
                        else
                        {
                            MyModbusFun.WashSyringes(1);
                        }
                    }
                }
            }
            else
            {
                if (FADM_Auto.Drip._b_dripStop)
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

                //记录初始天平读数
                double d_blBalanceValue3 = 0;
                if (FADM_Object.Communal._b_isStableRead)
                    d_blBalanceValue3 = SteBalance();
                else
                {
                    if (Lib_Card.Configure.Parameter.Machine_Type == 0)
                    {
                        d_blBalanceValue3 = Lib_Card.Configure.Parameter.Machine_BalanceType == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal.dBalanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal.dBalanceValue));
                    }
                    else
                    {
                        d_blBalanceValue3 = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal._s_balanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal._s_balanceValue));
                    }
                }
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平读数：" + d_blBalanceValue3);

                //验证
                if (FADM_Auto.Drip._b_dripStop)
                {
                    FADM_Object.Communal._b_stop = true;
                }
                if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                {
                    iInfusionPulse = i_adjust;
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "验证启动(" + iInfusionPulse + ")");
                    i_mRes = MyModbusFun.Shove(iInfusionPulse, 0);
                    if (-1 == i_mRes)
                        throw new Exception("驱动异常");
                    else if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                }
                else
                {
                    iInfusionPulse = i_adjust;
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "验证启动(" + iInfusionPulse + ")");
                    i_mRes = MyModbusFun.Shove(iInfusionPulse, 1);
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                }
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "验证完成");

                //读取验证重量
                sArg sArg = new sArg();
                sArg._i_minBottleNo = i_minBottleNo;
                sArg._d_blBalanceValue3 = d_blBalanceValue3;
                sArg._s_syringeType = s_syringeType;
                sArg._s_unitOfAccount = s_unitOfAccount;
                thread = new Thread(WaitBalance);
                thread.Start(sArg);
            }


            //判断当前母液是否滴完
            if (0 < dic_pulse.Count)
            {
                goto label9;
            }
            //如果带盖加药，前置加药完成，方针前先洗针
            if(i_special == 1)
            {
                for (int i = 0; i < lis_data.Count; i++)
                {
                    //对应值复称值写入
                    directoryReturn.Add(lis_data[i], _d_reviewBalance);
                }
                //循环判断是否加药完成
                foreach (KeyValuePair<int, double> kvp in directoryReturn)
                {
                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                          "UPDATE dye_details SET  Finish = 1 WHERE StepNum = " + dic_step[kvp.Key]  + " AND " +
                          "CupNum = " + kvp.Key + ";");

                    //查询另外一杯是否完成加药
                    DataTable dt_d = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM dye_details WHERE  BatchName !='0' And StepNum = " + dic_step[kvp.Key] + " AND " +
                          "CupNum = " + SmartDyeing.FADM_Object.Communal._dic_first_second[kvp.Key] + ";");
                    if (dt_d.Rows.Count > 0)
                    {
                        if (dt_d.Rows[0]["Finish"].ToString() == "1")
                        {
                            //由于触摸屏反应太慢，导致连续加药时没及时反应，必须等打板机步号和上位机步号一致才能完成加药
                            while (true)
                            {
                                if (dic_step[kvp.Key] == FADM_Auto.Dye._cup_Temps[SmartDyeing.FADM_Object.Communal._dic_first_second[kvp.Key] - 1]._i_stepNum
                                    && 2 == FADM_Auto.Dye._cup_Temps[SmartDyeing.FADM_Object.Communal._dic_first_second[kvp.Key] - 1]._i_staus)
                                {
                                    //复位加药启动信号
                                    int[] ia_zero = new int[1];
                                    //
                                    ia_zero[0] = 0;
                                    FADM_Auto.Dye.DyeHMIWrite(kvp.Key, 509, 309, ia_zero);

                                    //发送加药完成
                                    ia_zero = new int[1];
                                    //
                                    ia_zero[0] = 2;

                                    FADM_Auto.Dye.DyeHMIWrite(kvp.Key, 115, 114, ia_zero);

                                    break;
                                }
                                else
                                {
                                    Thread.Sleep(100);
                                }
                            }
                            //判断下一步是否需要关盖
                            DataTable dt_s = FADM_Object.Communal._fadmSqlserver.GetData("Select * from dye_details where  BatchName !='0' And CupNum = " + kvp.Key + " And StepNum = " + (dic_step[kvp.Key] + 1));
                            if (dt_s.Rows.Count > 0)
                            {
                                if (dt_s.Rows[0]["TechnologyName"].ToString() == "搅拌" || dt_s.Rows[0]["TechnologyName"].ToString() == "冷行" || dt_s.Rows[0]["TechnologyName"].ToString() == "温控")
                                {
                                    int i_cupNo = kvp.Key;
                                    //判断是否开盖状态
                                    if (FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._i_cupCover == 2)
                                    {
                                        //判断是否翻转缸，如果是就执行关盖动作
                                        //关盖
                                        if (SmartDyeing.FADM_Object.Communal._dic_dyeType[i_cupNo] == 1 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_cupNo] == 2 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_cupNo] == 3 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_cupNo] == 4 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_cupNo] == 5)
                                        {
                                            //复位加药启动信号
                                            int[] ia_zero1 = new int[1];
                                            //
                                            ia_zero1[0] = 0;


                                            FADM_Auto.Dye.DyeOpenOrCloseCover(i_cupNo, 1);
                                            Thread.Sleep(1000);
                                            Communal._fadmSqlserver.ReviseData("Update  cup_details set CoverStatus = 1 where CupNum = " + i_cupNo);

                                            FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._i_cupCover = 1;
                                        }
                                    }
                                    //if (b_sec)
                                    {
                                        //判断是否开盖状态
                                        if (FADM_Auto.Dye._cup_Temps[SmartDyeing.FADM_Object.Communal._dic_first_second[i_cupNo] - 1]._i_cupCover == 2)
                                        {
                                            if (SmartDyeing.FADM_Object.Communal._dic_dyeType[_dic_first_second[i_cupNo]] == 1 || SmartDyeing.FADM_Object.Communal._dic_dyeType[_dic_first_second[i_cupNo]] == 2 || SmartDyeing.FADM_Object.Communal._dic_dyeType[_dic_first_second[i_cupNo]] == 3 || SmartDyeing.FADM_Object.Communal._dic_dyeType[_dic_first_second[i_cupNo]] == 4 || SmartDyeing.FADM_Object.Communal._dic_dyeType[_dic_first_second[i_cupNo]] == 5)
                                            {
                                                //复位加药启动信号
                                                int[] ia_zero1 = new int[1];
                                                //
                                                ia_zero1[0] = 0;


                                                FADM_Auto.Dye.DyeOpenOrCloseCover(_dic_first_second[i_cupNo], 1);
                                                Thread.Sleep(1000);
                                                Communal._fadmSqlserver.ReviseData("Update  cup_details set CoverStatus = 1 where CupNum = " + _dic_first_second[i_cupNo]);

                                                FADM_Auto.Dye._cup_Temps[_dic_first_second[i_cupNo] - 1]._i_cupCover = 1;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        while (true)
                        {
                            if (dic_step[kvp.Key] == FADM_Auto.Dye._cup_Temps[kvp.Key - 1]._i_stepNum && 2 == FADM_Auto.Dye._cup_Temps[kvp.Key - 1]._i_staus)
                            {
                                //复位加药启动信号
                                int[] ia_zero = new int[1];
                                //
                                ia_zero[0] = 0;
                                FADM_Auto.Dye.DyeHMIWrite(kvp.Key, 509, 309, ia_zero);

                                //发送加药完成
                                ia_zero = new int[1];
                                //
                                ia_zero[0] = 2;

                                FADM_Auto.Dye.DyeHMIWrite(kvp.Key, 115, 114, ia_zero);
                                break;
                            }
                            else
                            {
                                Thread.Sleep(100);
                            }
                        }
                        //判断下一步是否需要关盖
                        DataTable dt_s = FADM_Object.Communal._fadmSqlserver.GetData("Select * from dye_details where  BatchName !='0' And CupNum = " + kvp.Key + " And StepNum = " + (dic_step[kvp.Key] + 1));
                        if (dt_s.Rows.Count > 0)
                        {
                            if (dt_s.Rows[0]["TechnologyName"].ToString() == "搅拌" || dt_s.Rows[0]["TechnologyName"].ToString() == "冷行" || dt_s.Rows[0]["TechnologyName"].ToString() == "温控")
                            {
                                int i_cupNo = kvp.Key;
                                //判断是否开盖状态
                                if (FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._i_cupCover == 2)
                                {
                                    //判断是否翻转缸，如果是就执行关盖动作
                                    //关盖
                                    if (SmartDyeing.FADM_Object.Communal._dic_dyeType[i_cupNo] == 1 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_cupNo] == 2 
                                        || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_cupNo] == 3 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_cupNo] == 4 
                                        || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_cupNo] == 5)
                                    {
                                        //复位加药启动信号
                                        int[] ia_zero1 = new int[1];
                                        //
                                        ia_zero1[0] = 0;


                                        FADM_Auto.Dye.DyeOpenOrCloseCover(i_cupNo, 1);
                                        Thread.Sleep(1000);
                                        Communal._fadmSqlserver.ReviseData("Update  cup_details set CoverStatus = 1 where CupNum = " + i_cupNo);

                                        FADM_Auto.Dye._cup_Temps[i_cupNo - 1]._i_cupCover = 1;
                                    }
                                }
                            }
                        }
                    }

                    

                }
                //判断洗针筒字典是否存在需要洗针的
                if (FADM_Object.Communal._b_isHasWashSyringe)
                {
                    //移动到洗针位置
                    i_mRes = MyModbusFun.TargetMove(12, 0, 0);
                    if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                    {
                        MyModbusFun.WashSyringes(0);
                    }
                    else
                    {
                        MyModbusFun.WashSyringes(1);
                    }
                }
            }

            //移动到母液瓶
            if (FADM_Auto.Drip._b_dripStop)
            {
                FADM_Object.Communal._b_stop = true;
            }
            if (i_minBottleNo == 999)
            {
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找放针母液瓶");
                FADM_Object.Communal._i_optBottleNum = i_minBottleNo;
                i_mRes = MyModbusFun.TargetMove(11, 0, 0);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达放针母液瓶");

            }
            else
            {
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找" + i_minBottleNo + "号母液瓶");
                FADM_Object.Communal._i_optBottleNum = i_minBottleNo;
                i_mRes = MyModbusFun.TargetMove(0, i_minBottleNo, 0);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达" + i_minBottleNo + "号母液瓶");
            }

            //放针
            if (FADM_Auto.Drip._b_dripStop)
            {
                FADM_Object.Communal._b_stop = true;
            }
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", i_minBottleNo + "号母液瓶放针启动");

            if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
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
            Communal._b_isGetSyringes = false;
            if (null != thread)
                thread.Join();
            //解锁6工位
            MyModbusFun.UnLockSix();

            if (i_special != 1)
            {
                for (int i = 0; i < lis_data.Count; i++)
                {
                    //对应值复称值写入
                    directoryReturn.Add(lis_data[i], _d_reviewBalance);
                }
            }

            return 0;
        }

        /// <summary>
        /// 加药封装
        /// i_type：类型，1代表后处理  2吸光度加药 3吸光度配方抽液
        ///  i_special:是否跳过天平复称和预滴液 1是跳过 0是不跳过
        /// _i_minBottleNo：母液瓶号
        /// _s_syringeType：针筒类型
        /// _i_pulseT：总脉冲
        /// _i_adjust:校正脉冲
        ///  _s_unitOfAccount：单位
        ///  iLowSrart：当前状态
        ///  _obj_batchName：所属批次
        ///  _dic_pulse：配液杯对应加药脉冲
        ///  directoryWeight：配液杯对应加药重
        ///  lWater:后处理加药时先加水的重量
        /// </summary>
        /// /// <returns>0：正常；-1：夹不到针筒退出；-2：收到退出消息</returns>

        public static int AddMacOther(sAddArg o, ref Dictionary<int, double> directoryReturn, int i_type, int i_special)
        {
            List<int> lis_data = new List<int>();
            string s_batchName = o._obj_batchName;
            int i_minBottleNo = o._i_minBottleNo;
            string s_syringeType = o._s_syringeType;
            int i_pulseT = o._i_pulseT;
            int i_adjust = o._i_adjust;
            string s_unitOfAccount = o._s_unitOfAccount;
            int _i_AddType = o._i_AddType;

            Dictionary<int, int> dic_pulse = o._dic_pulse;
            Dictionary<int, double> dic_water = o._dic_water;
            Dictionary<int, int> dic_step = o._dic_step;
            Thread thread = null;

            //如果是母液瓶999就先去拿针筒
            if (i_minBottleNo == 999 || i_minBottleNo == 888)
            {
                int i_xStart = 0, i_yStart = 0;
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
                    try
                    {
                        int i_mRes3 = MyModbusFun.GetSyringes(i_xStart, i_yStart);
                        if (-2 == i_mRes3)
                            throw new Exception("收到退出消息");
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
                                if (Communal.GetValueWash(i_minBottleNo.ToString(), out value))
                                {
                                    //标记夹不到针筒
                                    value[1] = 0;
                                    Communal.AddOrUpdateWash(i_minBottleNo.ToString(), value);
                                }
                            }
                            return -1;

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
                                if (Communal.GetValueWash(i_minBottleNo.ToString(), out value))
                                {
                                    //标记夹不到针筒
                                    value[1] = 0;
                                    Communal.AddOrUpdateWash(i_minBottleNo.ToString(), value);
                                }
                            }
                            return -1;

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
                                if (Communal.GetValueWash(i_minBottleNo.ToString(), out value))
                                {
                                    //标记夹不到针筒
                                    value[1] = 0;
                                    Communal.AddOrUpdateWash(i_minBottleNo.ToString(), value);
                                }
                            }
                            return -1;

                        }
                        else
                            throw;
                    }

                    FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "拿针筒完成");
                }
            }

            //是否已经选择过
            bool b_choose = false;
            //判断是否已经液量过度已经报警还继续的
            DataTable dt_bottle_details3 = FADM_Object.Communal._fadmSqlserver.GetData(
                            "SELECT * FROM drop_details WHERE  BottleNum = " + i_minBottleNo + " And MinWeight = 3;");
            if (dt_bottle_details3.Rows.Count > 0)
            {
                b_choose = true;
            }

        label9:

            ////判断是否液量过低
            //if (i_minBottleNo != 999)
            //{
            //    if (!b_choose)
            //    {
            //        if (FADM_Object.Communal._b_isLowDrip)
            //        {
            //            DataTable dt_bottle_details4 = FADM_Object.Communal._fadmSqlserver.GetData(
            //                "SELECT * FROM bottle_details WHERE  BottleNum = " + i_minBottleNo + ";");
            //            if (dt_bottle_details4.Rows.Count > 0)
            //            {
            //                double d_blCW = Convert.ToDouble(string.Format("{0:F3}", dt_bottle_details4.Rows[0]["CurrentWeight"]));
            //                if (d_blCW < Lib_Card.Configure.Parameter.Other_Bottle_MinWeight)
            //                {
            //                    return -3;
            //                }
            //            }
            //        }
            //    }
            //}

            if (null != thread)
                thread.Join();
            //解锁6工位
            MyModbusFun.UnLockSix();
            for (int i = 0; i < lis_data.Count; i++)
            {
                //对应值复称值写入
                directoryReturn.Add(lis_data[i], _d_reviewBalance);
                //后处理滴液
                if (i_type == 1)
                {
                    double d_blRErr = 0;
                    if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                        d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                    else
                        d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_B_Weight)) : Convert.ToDouble(string.Format("{0:F3}", _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_B_Weight));
                    ;

                    //查询开料日期
                    DataTable dt_bottle_details = FADM_Object.Communal._fadmSqlserver.GetData(
            "SELECT bottle_details.*,UnitOfAccount  FROM bottle_details left join assistant_details   on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE  bottle_details.BottleNum = " + i_minBottleNo + ";");

                    if (0.00 != _d_reviewBalance)
                    {
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                          "UPDATE dye_details SET RealDropWeight = ObjectDropWeight + IsNULL(Compensation,0.0) + " + d_blRErr + ", Cooperate = 0 , Finish = 1 "
                          + " ,BrewingData = '" + dt_bottle_details.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '"
                          + (dt_bottle_details.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                          "WHERE StepNum = " + dic_step[lis_data[i]] + " AND BottleNum = " + i_minBottleNo + " AND " +
                          "CupNum = " + lis_data[i] + ";");

                        //更新后先把加药量取出
                        string s_sql2 = "SELECT RealDropWeight,ObjectDropWeight FROM dye_details WHERE StepNum = " + dic_step[lis_data[i]] + " AND BottleNum = " + i_minBottleNo + " AND " +
                          "CupNum = " + lis_data[i] + ";";
                        DataTable dt_dye_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql2);

                        if (dt_dye_details.Rows.Count > 0)
                        {

                            //记录是否修正数据
                            bool b_change = false;
                            double d_realDrop = 0.0;
                            if (FADM_Object.Communal._b_isCorrect)
                            {
                                //如果实际滴液量为负数，就改为0
                                if (Convert.ToDouble(dt_dye_details.Rows[0]["RealDropWeight"]) < 0)
                                {
                                    b_change = true;
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                              "UPDATE dye_details SET RealDropWeight = 0, Cooperate = 0 , Finish = 1 " + " ,BrewingData = '"
                              + dt_bottle_details.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '"
                              + (dt_bottle_details.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                              "WHERE StepNum = " + dic_step[lis_data[i]] + " AND BottleNum = " + i_minBottleNo + " AND " +
                              "CupNum = " + lis_data[i] + ";");
                                }

                                //判断当目标滴液量大于1g，而且实际滴液误差小于等于0.05的，全部置为0.02
                                if (Convert.ToDouble(dt_dye_details.Rows[0]["ObjectDropWeight"]) > 1)
                                {
                                    if (d_blRErr <= 0.05 && d_blRErr > 0.02)
                                    {
                                        b_change = true;
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                              "UPDATE dye_details SET RealDropWeight = ObjectDropWeight + IsNULL(Compensation,0.0) + " + 0.02 + ", Cooperate = 0 , Finish = 1 " + " ,BrewingData = '"
                              + dt_bottle_details.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '" + (dt_bottle_details.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                              "WHERE StepNum = " + dic_step[lis_data[i]] + " AND BottleNum = " + i_minBottleNo + " AND " +
                              "CupNum = " + lis_data[i] + ";");
                                        d_realDrop = Convert.ToDouble(dt_dye_details.Rows[0]["ObjectDropWeight"]) + 0.02;
                                    }
                                    else if (d_blRErr < -0.02 && d_blRErr >= -0.05)
                                    {
                                        b_change = true;
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                              "UPDATE dye_details SET RealDropWeight = ObjectDropWeight + IsNULL(Compensation,0.0) + " + (-0.02) + ", Cooperate = 0 , Finish = 1 " + " ,BrewingData = '"
                              + dt_bottle_details.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '"
                              + (dt_bottle_details.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                              "WHERE StepNum = " + dic_step[lis_data[i]] + " AND BottleNum = " + i_minBottleNo + " AND " +
                              "CupNum = " + lis_data[i] + ";");
                                        d_realDrop = Convert.ToDouble(dt_dye_details.Rows[0]["ObjectDropWeight"]) - 0.02;
                                    }
                                }
                            }
                            if (b_change)
                            {
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE cup_details SET TotalWeight = TotalWeight+ " + d_realDrop + " WHERE CupNum = " + lis_data[i] + ";");

                                //母液瓶扣减
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_realDrop + " " +
                                    "WHERE BottleNum = '" + i_minBottleNo + "';");
                            }
                            else
                            {

                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE cup_details SET TotalWeight = TotalWeight + " + dt_dye_details.Rows[0][0].ToString() + " WHERE CupNum = " + lis_data[i] + ";");

                                //母液瓶扣减
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + dt_dye_details.Rows[0][0].ToString() + " " +
                                    "WHERE BottleNum = '" + i_minBottleNo + "';");
                            }
                        }
                    }
                    else
                    {
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                          "UPDATE dye_details SET RealDropWeight = 0.00, Cooperate = 0 , Finish = 1 " + " ,BrewingData = '"
                          + dt_bottle_details.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '"
                          + (dt_bottle_details.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                          "WHERE StepNum = " + dic_step[lis_data[i]] + " AND BottleNum = " + i_minBottleNo + " AND " +
                          "CupNum = " + lis_data[i] + ";");
                    }


                }
                //吸光度加药
                else if (i_type == 2)
                {
                    double d_blRErr = 0;
                    if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                        d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                            _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                    else
                        d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                            _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_B_Weight)) : Convert.ToDouble(string.Format("{0:F3}", _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_B_Weight));
                    ;


                    if (0.00 != _d_reviewBalance)
                    {
                        //FADM_Object.Communal._fadmSqlserver.ReviseData(
                        //  "UPDATE dye_details SET RealDropWeight = ObjectDropWeight + IsNULL(Compensation,0.0) + " + d_blRErr + ", Cooperate = 0 , Finish = 1 " + " ,BrewingData = '" + dt_bottle_details.Rows[0]["BrewingData"].ToString() + "' " +
                        //  "WHERE StepNum = " + dic_step[kvp.Key] + " AND BottleNum = " + i_bottleNo + " AND " +
                        //  "CupNum = " + kvp.Key + ";");

                        if (_i_AddType == 1)
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                              "UPDATE abs_cup_details SET RealSampleDosage = SampleDosage + " + d_blRErr +
                              " WHERE CupNum = " + lis_data[i] + ";");
                        }
                        else
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                              "UPDATE abs_cup_details SET RealAdditivesDosage = AdditivesDosage + " + d_blRErr +
                              " WHERE CupNum = " + lis_data[i] + ";");
                        }


                        //更新后先把加药量取出
                        string s_sql2 = null;
                        if (_i_AddType == 1)
                        {
                            s_sql2 = "SELECT RealSampleDosage FROM abs_cup_details WHERE  CupNum = " + lis_data[i] + ";";
                        }
                        else
                        {
                            s_sql2 = "SELECT RealAdditivesDosage FROM abs_cup_details WHERE  CupNum = " + lis_data[i] + ";";
                        }
                        DataTable dt_dye_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql2);

                        if (dt_dye_details.Rows.Count > 0)
                        {
                            //母液瓶扣减
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + dt_dye_details.Rows[0][0].ToString() + " " +
                                "WHERE BottleNum = '" + i_minBottleNo + "';");
                        }
                    }
                    else
                    {
                        if (_i_AddType == 1)
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                              "UPDATE abs_cup_details SET RealSampleDosage = SampleDosage + " + d_blRErr +
                              " WHERE CupNum = " + lis_data[i] + ";");
                        }
                        else
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                              "UPDATE abs_cup_details SET RealAdditivesDosage = AdditivesDosage + " + d_blRErr +
                              " WHERE CupNum = " + lis_data[i] + ";");
                        }
                    }
                }
                //吸光度配方滴液
                else if (i_type == 3)
                {
                    double d_blRErr = 0;
                    if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                        d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                            _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                    else
                        d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                            _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_B_Weight)) : Convert.ToDouble(string.Format("{0:F3}", _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_B_Weight));

                    //查询开料日期
                    DataTable dt_bottle_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM bottle_details WHERE  BottleNum = " + i_minBottleNo + ";");

                    if (0.00 != _d_reviewBalance)
                    {
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                        "UPDATE abs_drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + d_blRErr + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                        "WHERE BatchName = '" + s_batchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                        "CupNum = " + lis_data[i] + ";");

                        DataTable dt_drop_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                            "SELECT * FROM abs_drop_details WHERE BatchName = '" + s_batchName + "' AND BottleNum = " + i_minBottleNo + " AND CupNum = " + lis_data[i] + ";");
                        //FADM_Object.Communal._fadmSqlserver.ReviseData(
                        //    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + dt_drop_details2.Rows[0]["RealDropWeight"] + " WHERE CupNum = " + kvp.Key + ";");

                        //母液瓶扣减
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + dt_drop_details2.Rows[0]["RealDropWeight"] + " " +
                            "WHERE BottleNum = '" + i_minBottleNo + "';");
                    }
                    else
                    {
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                       "UPDATE abs_drop_details SET Finish = 1,RealDropWeight = 0.00 " + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                       "WHERE BatchName = '" + s_batchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                       "CupNum = " + lis_data[i] + ";");
                    }
                }
                //double d_blRErr = 0;
                //if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                //    d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                //else
                //    d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_B_Weight)) : Convert.ToDouble(string.Format("{0:F3}", _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_B_Weight));

                ////查询开料日期
                //DataTable dt_bottle_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                //            "SELECT * FROM bottle_details WHERE  BottleNum = " + i_minBottleNo + ";");

                //if (0.00 != _d_reviewBalance)
                //{
                //    FADM_Object.Communal._fadmSqlserver.ReviseData(
                //    "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + d_blRErr + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                //    "WHERE BatchName = '" + s_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                //    "CupNum = " + lis_data[i] + ";");

                //    DataTable dt_drop_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                //        "SELECT * FROM drop_details WHERE BatchName = '" + s_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND CupNum = " + lis_data[i] + ";");
                //    if (dt_drop_details2.Rows.Count > 0)
                //    {

                //        //记录是否修正数据
                //        bool b_change = false;
                //        double d_realDrop = 0.0;
                //        if (FADM_Object.Communal._b_isCorrect)
                //        {
                //            //如果实际滴液量为负数，就改为0
                //            if (Convert.ToDouble(dt_drop_details2.Rows[0]["RealDropWeight"]) < 0)
                //            {
                //                b_change = true;
                //                FADM_Object.Communal._fadmSqlserver.ReviseData(
                //        "UPDATE drop_details SET Finish = 1,RealDropWeight = 0   ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                //        "WHERE BatchName = '" + s_batchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                //        "CupNum = " + lis_data[i] + ";");
                //            }

                //            //判断当目标滴液量大于1g，而且实际滴液误差小于等于0.05的，全部置为0.02
                //            if (Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) > 1)
                //            {
                //                if (d_blRErr <= 0.05 && d_blRErr > 0.02)
                //                {
                //                    b_change = true;
                //                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                //        "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + 0.02 + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                //        "WHERE BatchName = '" + s_batchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                //        "CupNum = " + lis_data[i] + ";");
                //                    d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) + 0.02;
                //                }
                //                else if (d_blRErr < -0.02 && d_blRErr >= -0.05)
                //                {
                //                    b_change = true;
                //                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                //        "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + (-0.02) + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                //        "WHERE BatchName = '" + s_batchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                //        "CupNum = " + lis_data[i] + ";");
                //                    d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) - 0.02;
                //                }
                //            }
                //        }
                //        if (b_change)
                //        {
                //            FADM_Object.Communal._fadmSqlserver.ReviseData(
                //            "UPDATE cup_details SET TotalWeight = TotalWeight+ " + d_realDrop + " WHERE CupNum = " + lis_data[i] + ";");

                //            //母液瓶扣减
                //            FADM_Object.Communal._fadmSqlserver.ReviseData(
                //                "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_realDrop + " " +
                //                "WHERE BottleNum = '" + i_minBottleNo + "';");
                //        }
                //        else
                //        {
                //            FADM_Object.Communal._fadmSqlserver.ReviseData(
                //            "UPDATE cup_details SET TotalWeight = TotalWeight+ " + dt_drop_details2.Rows[0]["RealDropWeight"] + " WHERE CupNum = " + lis_data[i] + ";");

                //            //母液瓶扣减
                //            FADM_Object.Communal._fadmSqlserver.ReviseData(
                //                "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + dt_drop_details2.Rows[0]["RealDropWeight"] + " " +
                //                "WHERE BottleNum = '" + i_minBottleNo + "';");
                //        }
                //    }
                //}
                //else
                //{
                //    FADM_Object.Communal._fadmSqlserver.ReviseData(
                //   "UPDATE drop_details SET Finish = 1,RealDropWeight = 0.00 " + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                //   "WHERE BatchName = '" + s_batchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                //   "CupNum = " + lis_data[i] + ";");
                //}
            }
            lis_data.Clear();

            //移动到母液瓶
            if (FADM_Auto.Drip._b_dripStop || FADM_Auto.ABS_Drip._b_dripStop)
            {
                FADM_Object.Communal._b_stop = true;
            }

            //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
            //{
            //    //富士伺服在下面判断 天平状态 原有不动 绿维的放在上面 并且置位 是否回原点 绿维的放移动机械手前面

            //    //判断是否异常
            //    FADM_Object.Communal.BalanceState("滴液");
            //}

            int i_mRes;
            if (i_minBottleNo == 888)
            {
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找1号(吸光度杯)");
                FADM_Object.Communal._i_optBottleNum = i_minBottleNo;
                i_mRes = MyModbusFun.TargetMove(10, 1, 0);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达1号(吸光度杯)");
            }
            else if (i_minBottleNo == 999)
            {
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找3号(吸光度杯)");
                FADM_Object.Communal._i_optBottleNum = i_minBottleNo;
                i_mRes = MyModbusFun.TargetMove(10, 3, 0);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达3号(吸光度杯)");
            }
            else
            {

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找" + i_minBottleNo + "号母液瓶");
                FADM_Object.Communal._i_optBottleNum = i_minBottleNo;
                i_mRes = MyModbusFun.TargetMove(0, i_minBottleNo, 1);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达" + i_minBottleNo + "号母液瓶");
            }

            //抽液
            if (FADM_Auto.Drip._b_dripStop || FADM_Auto.ABS_Drip._b_dripStop)
            {
                FADM_Object.Communal._b_stop = true;
            }
            int iExtractionPulse = 0;
            if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
            {
                iExtractionPulse = (i_pulseT + i_adjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1));
                //FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "脉冲(" + i_pulseT + ")"+ "校正值(" + i_adjust + ")" + "Other_Z_BackPulse(" + Lib_Card.Configure.Parameter.Other_Z_BackPulse + ")" + "Other_S_MaxPulse(" + Lib_Card.Configure.Parameter.Other_S_MaxPulse + ")" + "_b_isFullDrip(" + (Communal._b_isFullDrip?"1":"0") + ")" + "_b_isDripReserveFirst(" + (FADM_Object.Communal._b_isDripReserveFirst ? "1" : "0") + ")");
                if (iExtractionPulse - Lib_Card.Configure.Parameter.Other_Z_BackPulse > Lib_Card.Configure.Parameter.Other_S_MaxPulse)
                {
                    if (FADM_Object.Communal._b_isFullDrip)
                    {
                        //超出单针筒上限
                        iExtractionPulse = Lib_Card.Configure.Parameter.Other_S_MaxPulse + Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                    }
                    else
                    {
                        iExtractionPulse = i_adjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1);
                        int i_temp = iExtractionPulse;
                        for (int i = 0; i < dic_pulse.Count; i++)
                        {
                            var v1 = dic_pulse.ElementAt(i);

                            i_temp = iExtractionPulse;

                            if (i_temp + v1.Value - Lib_Card.Configure.Parameter.Other_Z_BackPulse > Lib_Card.Configure.Parameter.Other_S_MaxPulse)
                            {
                                //如果需加量大于10g，直接先加
                                //if(v1.Value/1.0/ _i_adjust > Lib_Card.Configure.Parameter.Other_SplitValue)
                                if (i_temp == i_adjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1))
                                {
                                    iExtractionPulse = Lib_Card.Configure.Parameter.Other_S_MaxPulse + Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                                }
                                else
                                {
                                    //如果一筒抽不完就分2次，一次抽完就留下一次独立抽
                                    if (v1.Value + i_adjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1) - Lib_Card.Configure.Parameter.Other_Z_BackPulse > Lib_Card.Configure.Parameter.Other_S_MaxPulse)
                                    {
                                        iExtractionPulse = Lib_Card.Configure.Parameter.Other_S_MaxPulse + Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                                    }
                                }
                                break;
                            }
                            else
                            {
                                iExtractionPulse += v1.Value;
                            }
                        }
                    }
                }
                if (FADM_Object.Communal._b_isDripReserveFirst)
                    iExtractionPulse += i_adjust;
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抽液启动(" + iExtractionPulse + ")");

            label10:
                try
                {
                    iExtractionPulse = iExtractionPulse - Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                    if (i_type == 1 || i_type == 3)
                    {
                        if (s_unitOfAccount == "g/l")
                            i_mRes = MyModbusFun.Extract(iExtractionPulse,true, 0); //抽液
                        else
                            i_mRes = MyModbusFun.Extract(iExtractionPulse,true, 0); //抽液
                    }
                    else
                    {
                        if (i_minBottleNo == 888)
                        {
                            //1号吸光度杯停止搅拌
                            int[] values = new int[1];
                            values[0] = 1;
                            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                            {
                                FADM_Object.Communal._tcpModBusAbs.ReConnect();
                            }

                            FADM_Object.Communal._tcpModBusAbs.Write(13094, values);

                            Thread.Sleep(2000);

                            i_mRes = MyModbusFun.AbsExtract(iExtractionPulse, true, 0); //抽液

                            values[0] = 0;
                            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                            {
                                FADM_Object.Communal._tcpModBusAbs.ReConnect();
                            }
                            FADM_Object.Communal._tcpModBusAbs.Write(13094, values);
                        }
                        else if (i_minBottleNo == 999)
                        {
                            //1号吸光度杯停止搅拌
                            int[] values = new int[1];
                            values[0] = 1;
                            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                            {
                                FADM_Object.Communal._tcpModBusAbs.ReConnect();
                            }

                            FADM_Object.Communal._tcpModBusAbs.Write(13114, values);

                            Thread.Sleep(2000);

                            i_mRes = MyModbusFun.AbsExtract(iExtractionPulse, true, 0); //抽液

                            values[0] = 0;
                            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                            {
                                FADM_Object.Communal._tcpModBusAbs.ReConnect();
                            }
                            FADM_Object.Communal._tcpModBusAbs.Write(13114, values);
                        }

                        else
                            i_mRes = MyModbusFun.Extract(iExtractionPulse, true, 0); //抽液
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
                        if (Communal.GetValueWash(i_minBottleNo.ToString(), out value))
                        {
                            //如果之前标记过夹不到针筒，现在抽液完成，可改为夹到针筒
                            if (value[1] == 0)
                            {
                                value[1] = 1;
                                Communal.AddOrUpdateWash(i_minBottleNo.ToString(), value);
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
                            if (Communal.GetValueWash(i_minBottleNo.ToString(), out value))
                            {
                                //标记夹不到针筒
                                value[1] = 0;
                                Communal.AddOrUpdateWash(i_minBottleNo.ToString(), value);
                            }
                        }
                        return -1;

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
                            if (Communal.GetValueWash(i_minBottleNo.ToString(), out value))
                            {
                                //标记夹不到针筒
                                value[1] = 0;
                                Communal.AddOrUpdateWash(i_minBottleNo.ToString(), value);
                            }
                        }
                        return -1;

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
                            if (Communal.GetValueWash(i_minBottleNo.ToString(), out value))
                            {
                                //标记夹不到针筒
                                value[1] = 0;
                                Communal.AddOrUpdateWash(i_minBottleNo.ToString(), value);
                            }
                        }
                        return -1;

                    }
                    else
                        throw;
                }

            }
            else
            {
                iExtractionPulse = (i_pulseT + i_adjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1));
                if (iExtractionPulse - Lib_Card.Configure.Parameter.Other_Z_BackPulse > Lib_Card.Configure.Parameter.Other_B_MaxPulse)
                {
                    if (FADM_Object.Communal._b_isFullDrip)
                    {
                        //超出单针筒上限
                        iExtractionPulse = Lib_Card.Configure.Parameter.Other_B_MaxPulse + Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                    }
                    else
                    {
                        iExtractionPulse = i_adjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1);
                        int i_temp = iExtractionPulse;
                        for (int i = 0; i < dic_pulse.Count; i++)
                        {
                            var v1 = dic_pulse.ElementAt(i);

                            i_temp = iExtractionPulse;

                            if (i_temp + v1.Value - Lib_Card.Configure.Parameter.Other_Z_BackPulse > Lib_Card.Configure.Parameter.Other_B_MaxPulse)
                            {
                                ////如果需加量大于10g，直接先加
                                //if (v1.Value / 1.0 / _i_adjust >= Lib_Card.Configure.Parameter.Other_SplitValue)
                                if (i_temp == i_adjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1))
                                {
                                    iExtractionPulse = Lib_Card.Configure.Parameter.Other_B_MaxPulse + Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                                }
                                else
                                {
                                    if (v1.Value + i_adjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1) - Lib_Card.Configure.Parameter.Other_Z_BackPulse > Lib_Card.Configure.Parameter.Other_B_MaxPulse)
                                    {
                                        iExtractionPulse = Lib_Card.Configure.Parameter.Other_B_MaxPulse + Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                                    }
                                }
                                break;
                            }
                            else
                            {
                                iExtractionPulse += v1.Value;
                            }
                        }
                    }
                }
                if (FADM_Object.Communal._b_isDripReserveFirst)
                    iExtractionPulse += i_adjust;
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抽液启动(" + iExtractionPulse + ")");

            label11:
                try
                {
                    iExtractionPulse = iExtractionPulse - Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                    if (i_type == 1 || i_type == 3)
                    {
                        if (s_unitOfAccount == "g/l")
                            i_mRes = MyModbusFun.Extract(iExtractionPulse, true, 1); //抽液
                        else
                            i_mRes = MyModbusFun.Extract(iExtractionPulse, true, 1); //抽液
                    }
                    else
                    {
                        if (i_minBottleNo == 888)
                        {
                            //1号吸光度杯停止搅拌
                            int[] values = new int[1];
                            values[0] = 1;
                            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                            {
                                FADM_Object.Communal._tcpModBusAbs.ReConnect();
                            }

                            FADM_Object.Communal._tcpModBusAbs.Write(13094, values);
                            Thread.Sleep(2000);
                            i_mRes = MyModbusFun.AbsExtract(iExtractionPulse, true, 1); //抽液

                            values[0] = 0;
                            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                            {
                                FADM_Object.Communal._tcpModBusAbs.ReConnect();
                            }

                            FADM_Object.Communal._tcpModBusAbs.Write(13094, values);
                        }
                        else if (i_minBottleNo == 999)
                        {
                            //1号吸光度杯停止搅拌
                            int[] values = new int[1];
                            values[0] = 1;
                            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                            {
                                FADM_Object.Communal._tcpModBusAbs.ReConnect();
                            }

                            FADM_Object.Communal._tcpModBusAbs.Write(13114, values);
                            Thread.Sleep(2000);
                            i_mRes = MyModbusFun.AbsExtract(iExtractionPulse, true, 1); //抽液

                            values[0] = 0;
                            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                            {
                                FADM_Object.Communal._tcpModBusAbs.ReConnect();
                            }

                            FADM_Object.Communal._tcpModBusAbs.Write(13114, values);
                        }
                        else
                            i_mRes = MyModbusFun.Extract(iExtractionPulse, true, 1); //抽液
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
                        if (Communal.GetValueWash(i_minBottleNo.ToString(), out value))
                        {
                            //如果之前标记过夹不到针筒，现在抽液完成，可改为夹到针筒
                            if (value[1] == 0)
                            {
                                value[1] = 1;
                                Communal.AddOrUpdateWash(i_minBottleNo.ToString(), value);
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
                            if (Communal.GetValueWash(i_minBottleNo.ToString(), out value))
                            {
                                //标记夹不到针筒
                                value[1] = 0;
                                Communal.AddOrUpdateWash(i_minBottleNo.ToString(), value);
                            }
                        }
                        return -1;


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
                            if (Communal.GetValueWash(i_minBottleNo.ToString(), out value))
                            {
                                //标记夹不到针筒
                                value[1] = 0;
                                Communal.AddOrUpdateWash(i_minBottleNo.ToString(), value);
                            }
                        }
                        return -1;


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
                            if (Communal.GetValueWash(i_minBottleNo.ToString(), out value))
                            {
                                //标记夹不到针筒
                                value[1] = 0;
                                Communal.AddOrUpdateWash(i_minBottleNo.ToString(), value);
                            }
                        }
                        return -1;


                    }
                    else
                        throw;
                }
            }
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抽液完成");





            //Lib_SerialPort.Balance.METTLER.bReSetSign = true;

            if (FADM_Object.Communal._b_isDripReserveFirst)
            {



                //移动到天平位，先滴预留量
                if (FADM_Auto.Drip._b_dripStop || FADM_Auto.ABS_Drip._b_dripStop)
                {
                    FADM_Object.Communal._b_stop = true;
                }
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找天平位");
                //判断是否异常

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

                double d_blBalanceValueStart;
                if (Lib_Card.Configure.Parameter.Machine_Type == 0)
                {
                    d_blBalanceValueStart = Lib_Card.Configure.Parameter.Machine_BalanceType == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal.dBalanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal.dBalanceValue));
                }
                else
                {
                    d_blBalanceValueStart = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal._s_balanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal._s_balanceValue));
                }

                int i_zPulse1 = 0;
                //label98:
                if (-1 == MyModbusFun.GetZPosition(ref i_zPulse1))
                    throw new Exception("驱动异常");
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "Z轴脉冲" + i_zPulse1);
                //    if (i_zPulse1 > 0)
                //        goto label98;
                int i_infusionPulse1 = 0;
                if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                    i_infusionPulse1 = i_zPulse1 - i_adjust;
                else
                    i_infusionPulse1 = i_zPulse1 - i_adjust;

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "废液注液启动(" + i_infusionPulse1 + ")");
                if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                    i_mRes = MyModbusFun.Shove(i_infusionPulse1, 0);
                else
                    i_mRes = MyModbusFun.Shove(i_infusionPulse1, 1);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "废液注液完成");

                //等待一秒就当数据稳定
                Thread.Sleep(1000);

                double d_blBalanceValueEnd;
                if (Lib_Card.Configure.Parameter.Machine_Type == 0)
                {
                    d_blBalanceValueEnd = Lib_Card.Configure.Parameter.Machine_BalanceType == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal.dBalanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal.dBalanceValue));
                }
                else
                {
                    d_blBalanceValueEnd = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal._s_balanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal._s_balanceValue));
                }
                double d_blDif = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blBalanceValueEnd - d_blBalanceValueStart)) : Convert.ToDouble(string.Format("{0:F3}", d_blBalanceValueEnd - d_blBalanceValueStart));

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "预滴量:" + d_blDif);

                //解锁6工位
                MyModbusFun.UnLockSix();

                //预滴量大于0小于2才扣减
                if (d_blDif > 0 && d_blDif < 2)
                {
                    //母液瓶扣减1克
                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                        "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_blDif + "  " +
                        "WHERE BottleNum = '" + i_minBottleNo + "';");
                }
                //滴到废液桶时，数据差小于0.5，就当液量不够，提示
                if (d_blDif < 0.5)
                {
                    //记录第一个杯需加量脉冲
                    _i_needPulse = dic_pulse.First().Value;
                    _i_needPulseCupNumber = dic_pulse.First().Key;

                    if (FADM_Object.Communal._b_isHasWashSyringe)
                    {
                        if (i_minBottleNo == 888 || i_minBottleNo == 999)
                        {
                            //移动到洗针位置
                            i_mRes = MyModbusFun.TargetMove(12, 0, 0);

                            //执行洗针动作
                            MyModbusFun.WashSyringes(0);
                        }
                    }

                    MyModbusFun.MyMachineReset1(); //复位
                    return -2;
                }

            }

            //移动到配液杯

            List<int> lis_ints = new List<int>();
        label12:
            if (FADM_Auto.Drip._b_dripStop || FADM_Auto.ABS_Drip._b_dripStop)
            {
                FADM_Object.Communal._b_stop = true;
            }
            int i_zPulse = 0;
        label99:
            if (-1 == MyModbusFun.GetZPosition(ref i_zPulse))
                throw new Exception("驱动异常");
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "Z轴脉冲" + i_zPulse);
            //if (i_zPulse > 0)
            //    goto label99;
            if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
            {

                if (i_adjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1) == i_zPulse)
                {
                    //当前针筒已滴完，进入复检
                    goto label13;
                }
            }
            else
            {
                if (i_adjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1) == i_zPulse)
                {
                    //当前针筒已滴完，进入复检
                    goto label13;
                }

            }

            if (dic_pulse.Count == 0)
            {
                //如果已经没有要添加的杯，就直接进去
                goto label13;
            }
            if (i_type == 1)
            {
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找" + dic_pulse.First().Key + "号配液杯");

                FADM_Object.Communal._i_OptCupNum = dic_pulse.First().Key;
                i_mRes = MyModbusFun.TargetMove(1, dic_pulse.First().Key, 0);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达" + dic_pulse.First().Key + "号配液杯");
            }
            else if (i_type == 2 || i_type == 3)
            {
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找" + dic_pulse.First().Key + "号(吸光度杯)");

                FADM_Object.Communal._i_OptCupNum = dic_pulse.First().Key;
                i_mRes = MyModbusFun.TargetMove(10, dic_pulse.First().Key, 0);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达" + dic_pulse.First().Key + "号(吸光度杯)");
            }

            //判断是否加水


            if (0 < dic_water[dic_pulse.First().Key])
            {
                //new Water().Add(lWater.First(), "Dail");
                double d_addWaterTime = MyModbusFun.GetWaterTime(dic_water[dic_pulse.First().Key]);//加水时间
                if (d_addWaterTime <= 32)
                {
                    i_mRes = MyModbusFun.AddWater(d_addWaterTime);
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                }
                else
                {
                    double d = 32;
                    while (true)
                    {
                        if (d_addWaterTime > 32)
                        {
                            //每次减32s
                            i_mRes = MyModbusFun.AddWater(d);
                            if (-2 == i_mRes)
                                throw new Exception("收到退出消息");
                        }
                        else
                        {
                            i_mRes = MyModbusFun.AddWater(d_addWaterTime);
                            if (-2 == i_mRes)
                                throw new Exception("收到退出消息");
                            break;
                        }
                        d_addWaterTime -= d;
                    }
                }
                //加完水后会把加水量置为0，否则一针筒加药加不完的第二针筒加药时会重复加水
                if (i_type == 1)
                {
                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                    "UPDATE cup_details SET TotalWeight = TotalWeight + " + dic_water[dic_pulse.First().Key] + " WHERE CupNum = " + dic_pulse.First().Key + ";");
                }


                dic_water[dic_pulse.First().Key] = 0;
            }

            //注液
            if (FADM_Auto.Drip._b_dripStop || FADM_Auto.ABS_Drip._b_dripStop)
            {
                FADM_Object.Communal._b_stop = true;
            }
            int iInfusionPulse = 0;
            if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
            {
                if (i_zPulse - i_adjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1) >= dic_pulse.First().Value)
                {
                    //剩余量大于等于需加量
                    iInfusionPulse = i_zPulse - dic_pulse.First().Value;


                    ////母液瓶扣减
                    //FADM_Object.Communal._fadmSqlserver.ReviseData(
                    //    "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + directoryWeight.First().Value + " " +
                    //    "WHERE BottleNum = '" + _i_minBottleNo + "';");


                    ////置位完成标志位
                    //FADM_Object.Communal._fadmSqlserver.ReviseData(
                    //    "UPDATE drop_details SET Finish = 1 WHERE BatchName = '" + _obj_batchName + "' AND " +
                    //    "BottleNum = " + _i_minBottleNo + " AND CupNum = " + _dic_pulse.First().Key + ";");

                    i_pulseT -= dic_pulse.First().Value;
                    lis_data.Add(dic_pulse.First().Key);
                    dic_pulse.Remove(dic_pulse.First().Key);
                }
                else
                {
                    iInfusionPulse = i_adjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1);
                    dic_pulse[dic_pulse.First().Key] -= (i_zPulse - i_adjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1));
                    i_pulseT -= (i_zPulse - i_adjust * (Lib_Card.Configure.Parameter.Correcting_S_Weight + 1));
                }



                if (i_type == 1)
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液启动(" + iInfusionPulse + ")");
                    if (i_special == 1)
                    {
                        i_mRes = MyModbusFun.DyeShove(iInfusionPulse, 0);
                    }
                    else
                    {
                        i_mRes = MyModbusFun.Shove(iInfusionPulse, 0);
                    }
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                }
                else
                {
                    if (Lib_Card.Configure.Parameter.Machine_SlopDown == 1)
                    {
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液启动(" + iInfusionPulse + ")");
                        i_mRes = MyModbusFun.AbsShove(iInfusionPulse, 0);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");
                    }
                    else
                    {
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液启动(" + iInfusionPulse + ")");
                        i_mRes = MyModbusFun.Shove(iInfusionPulse, 0 );
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");
                    }
                }

            }
            else
            {
                if (i_zPulse - i_adjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1) >= dic_pulse.First().Value)
                {
                    //剩余量大于等于需加量
                    iInfusionPulse = i_zPulse - dic_pulse.First().Value;

                    ////母液瓶扣减
                    //FADM_Object.Communal._fadmSqlserver.ReviseData(
                    //    "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + directoryWeight.First().Value + " " +
                    //    "WHERE BottleNum = '" + _i_minBottleNo + "';");

                    ////置位完成标志位
                    //FADM_Object.Communal._fadmSqlserver.ReviseData(
                    //    "UPDATE drop_details SET Finish = 1 WHERE BatchName = '" + _obj_batchName + "' AND " +
                    //    "BottleNum = " + _i_minBottleNo + " AND CupNum = " + _dic_pulse.First().Key + ";");


                    i_pulseT -= dic_pulse.First().Value;
                    lis_data.Add(dic_pulse.First().Key);
                    dic_pulse.Remove(dic_pulse.First().Key);
                }
                else
                {
                    iInfusionPulse = i_adjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1);
                    dic_pulse[dic_pulse.First().Key] -= (i_zPulse - i_adjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1));
                    i_pulseT -= (i_zPulse - i_adjust * (Lib_Card.Configure.Parameter.Correcting_B_Weight + 1));
                }
                if (i_type == 1)
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液启动(" + iInfusionPulse + ")");
                    if (i_special == 1)
                    {
                        i_mRes = MyModbusFun.DyeShove(iInfusionPulse, 1);
                    }
                    else
                    {
                        i_mRes = MyModbusFun.Shove(iInfusionPulse, 1);
                    }
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                }
                else
                {
                    if (Lib_Card.Configure.Parameter.Machine_SlopDown == 1)
                    {
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液启动(" + iInfusionPulse + ")");
                        i_mRes = MyModbusFun.AbsShove(iInfusionPulse, 1);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");
                    }
                    else
                    {
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液启动(" + iInfusionPulse + ")");
                        i_mRes = MyModbusFun.Shove(iInfusionPulse, 1);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");
                    }
                }
            }


            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液完成");


            goto label12;

        //移动到天平位
        label13:
            if (i_special == 1 || i_special == 2)
            {
                if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                    _d_reviewBalance = Lib_Card.Configure.Parameter.Correcting_S_Weight;
                else
                    _d_reviewBalance = Lib_Card.Configure.Parameter.Correcting_B_Weight;
            }
            else
            {
                if (FADM_Auto.Drip._b_dripStop || FADM_Auto.ABS_Drip._b_dripStop)
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

                //记录初始天平读数
                double d_blBalanceValue3 = 0;
                if (FADM_Object.Communal._b_isStableRead)
                    d_blBalanceValue3 = SteBalance();
                else
                {
                    if (Lib_Card.Configure.Parameter.Machine_Type == 0)
                    {
                        d_blBalanceValue3 = Lib_Card.Configure.Parameter.Machine_BalanceType == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal.dBalanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal.dBalanceValue));
                    }
                    else
                    {
                        d_blBalanceValue3 = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal._s_balanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal._s_balanceValue));
                    }
                }
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平读数：" + d_blBalanceValue3);

                //验证
                if (FADM_Auto.Drip._b_dripStop || FADM_Auto.ABS_Drip._b_dripStop)
                {
                    FADM_Object.Communal._b_stop = true;
                }
                if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                {
                    iInfusionPulse = i_adjust;
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "验证启动(" + iInfusionPulse + ")");
                    i_mRes = MyModbusFun.Shove(iInfusionPulse, 0);
                    if (-1 == i_mRes)
                        throw new Exception("驱动异常");
                    else if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                }
                else
                {
                    iInfusionPulse = i_adjust;
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "验证启动(" + iInfusionPulse + ")");
                    i_mRes = MyModbusFun.Shove(iInfusionPulse, 1);
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                }
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "验证完成");

                if (i_minBottleNo == 999 || i_minBottleNo == 888)
                {
                    //if (FADM_Object.Communal._b_isHasWashSyringe)
                    //{
                    //    //移动到洗针位置
                    //    i_mRes = MyModbusFun.TargetMove(12, 0, 0);
                    //    //执行洗针动作
                    //    MyModbusFun.WashSyringes();
                    //}

                    //FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找放针母液瓶");
                    //FADM_Object.Communal._i_optBottleNum = i_minBottleNo;
                    //i_mRes = MyModbusFun.TargetMove(11, 0, 0);
                    //if (-2 == i_mRes)
                    //    throw new Exception("收到退出消息");

                    //FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达放针母液瓶");

                }
                else
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找" + i_minBottleNo + "号母液瓶");
                    FADM_Object.Communal._i_optBottleNum = i_minBottleNo;
                    i_mRes = MyModbusFun.TargetMove(0, i_minBottleNo, 0);
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达" + i_minBottleNo + "号母液瓶");
                }

                //读取验证重量
                sArg sArg = new sArg();
                sArg._i_minBottleNo = i_minBottleNo;
                sArg._d_blBalanceValue3 = d_blBalanceValue3;
                sArg._s_syringeType = s_syringeType;
                sArg._s_unitOfAccount = s_unitOfAccount;
                thread = new Thread(WaitBalance);
                thread.Start(sArg);
            }


            //判断当前母液是否滴完
            if (0 < dic_pulse.Count)
            {
                goto label9;
            }

            //移动到母液瓶
            if (FADM_Auto.Drip._b_dripStop || FADM_Auto.ABS_Drip._b_dripStop)
            {
                FADM_Object.Communal._b_stop = true;
            }
            if (i_minBottleNo == 999 || i_minBottleNo == 888)
            {
                if (FADM_Object.Communal._b_isHasWashSyringe)
                {
                    //移动到洗针位置
                    i_mRes = MyModbusFun.TargetMove(12, 0, 0);
                    //执行洗针动作
                    MyModbusFun.WashSyringes(0);
                }

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找放针母液瓶");
                FADM_Object.Communal._i_optBottleNum = i_minBottleNo;
                i_mRes = MyModbusFun.TargetMove(11, 0, 0);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达放针母液瓶");

            }
            else
            {
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找" + i_minBottleNo + "号母液瓶");
                FADM_Object.Communal._i_optBottleNum = i_minBottleNo;
                i_mRes = MyModbusFun.TargetMove(0, i_minBottleNo, 0);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达" + i_minBottleNo + "号母液瓶");
            }

            //放针
            if (FADM_Auto.Drip._b_dripStop || FADM_Auto.ABS_Drip._b_dripStop)
            {
                FADM_Object.Communal._b_stop = true;
            }
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", i_minBottleNo + "号母液瓶放针启动");

            if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
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
            Communal._b_isGetSyringes = false;
            if (null != thread)
                thread.Join();
            //解锁6工位
            MyModbusFun.UnLockSix();
            for (int i = 0; i < lis_data.Count; i++)
            {
                //对应值复称值写入
                directoryReturn.Add(lis_data[i], _d_reviewBalance);

                //后处理滴液
                if (i_type == 1)
                {
                    double d_blRErr = 0;
                    if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                        d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                    else
                        d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_B_Weight)) : Convert.ToDouble(string.Format("{0:F3}", _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_B_Weight));
                    ;

                    //查询开料日期
                    DataTable dt_bottle_details = FADM_Object.Communal._fadmSqlserver.GetData(
            "SELECT bottle_details.*,UnitOfAccount  FROM bottle_details left join assistant_details   on bottle_details.AssistantCode = assistant_details.AssistantCode WHERE  bottle_details.BottleNum = " + i_minBottleNo + ";");

                    if (0.00 != _d_reviewBalance)
                    {
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                          "UPDATE dye_details SET RealDropWeight = ObjectDropWeight + IsNULL(Compensation,0.0) + " + d_blRErr + ", Cooperate = 0 , Finish = 1 "
                          + " ,BrewingData = '" + dt_bottle_details.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '"
                          + (dt_bottle_details.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                          "WHERE StepNum = " + dic_step[lis_data[i]] + " AND BottleNum = " + i_minBottleNo + " AND " +
                          "CupNum = " + lis_data[i] + ";");

                        //更新后先把加药量取出
                        string s_sql2 = "SELECT RealDropWeight,ObjectDropWeight FROM dye_details WHERE StepNum = " + dic_step[lis_data[i]] + " AND BottleNum = " + i_minBottleNo + " AND " +
                          "CupNum = " + lis_data[i] + ";";
                        DataTable dt_dye_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql2);

                        if (dt_dye_details.Rows.Count > 0)
                        {

                            //记录是否修正数据
                            bool b_change = false;
                            double d_realDrop = 0.0;
                            if (FADM_Object.Communal._b_isCorrect)
                            {
                                //如果实际滴液量为负数，就改为0
                                if (Convert.ToDouble(dt_dye_details.Rows[0]["RealDropWeight"]) < 0)
                                {
                                    b_change = true;
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                              "UPDATE dye_details SET RealDropWeight = 0, Cooperate = 0 , Finish = 1 " + " ,BrewingData = '"
                              + dt_bottle_details.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '"
                              + (dt_bottle_details.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                              "WHERE StepNum = " + dic_step[lis_data[i]] + " AND BottleNum = " + i_minBottleNo + " AND " +
                              "CupNum = " + lis_data[i] + ";");
                                }

                                //判断当目标滴液量大于1g，而且实际滴液误差小于等于0.05的，全部置为0.02
                                if (Convert.ToDouble(dt_dye_details.Rows[0]["ObjectDropWeight"]) > 1)
                                {
                                    if (d_blRErr <= 0.05 && d_blRErr > 0.02)
                                    {
                                        b_change = true;
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                              "UPDATE dye_details SET RealDropWeight = ObjectDropWeight + IsNULL(Compensation,0.0) + " + 0.02 + ", Cooperate = 0 , Finish = 1 " + " ,BrewingData = '"
                              + dt_bottle_details.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '" + (dt_bottle_details.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                              "WHERE StepNum = " + dic_step[lis_data[i]] + " AND BottleNum = " + i_minBottleNo + " AND " +
                              "CupNum = " + lis_data[i] + ";");
                                        d_realDrop = Convert.ToDouble(dt_dye_details.Rows[0]["ObjectDropWeight"]) + 0.02;
                                    }
                                    else if (d_blRErr < -0.02 && d_blRErr >= -0.05)
                                    {
                                        b_change = true;
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                              "UPDATE dye_details SET RealDropWeight = ObjectDropWeight + IsNULL(Compensation,0.0) + " + (-0.02) + ", Cooperate = 0 , Finish = 1 " + " ,BrewingData = '"
                              + dt_bottle_details.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '"
                              + (dt_bottle_details.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                              "WHERE StepNum = " + dic_step[lis_data[i]] + " AND BottleNum = " + i_minBottleNo + " AND " +
                              "CupNum = " + lis_data[i] + ";");
                                        d_realDrop = Convert.ToDouble(dt_dye_details.Rows[0]["ObjectDropWeight"]) - 0.02;
                                    }
                                }
                            }
                            if (b_change)
                            {
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE cup_details SET TotalWeight = TotalWeight+ " + d_realDrop + " WHERE CupNum = " + lis_data[i] + ";");

                                //母液瓶扣减
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_realDrop + " " +
                                    "WHERE BottleNum = '" + i_minBottleNo + "';");
                            }
                            else
                            {

                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE cup_details SET TotalWeight = TotalWeight + " + dt_dye_details.Rows[0][0].ToString() + " WHERE CupNum = " + lis_data[i] + ";");

                                //母液瓶扣减
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + dt_dye_details.Rows[0][0].ToString() + " " +
                                    "WHERE BottleNum = '" + i_minBottleNo + "';");
                            }
                        }
                    }
                    else
                    {
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                          "UPDATE dye_details SET RealDropWeight = 0.00, Cooperate = 0 , Finish = 1 " + " ,BrewingData = '"
                          + dt_bottle_details.Rows[0]["BrewingData"].ToString() + "' " + " ,StandError = '"
                          + (dt_bottle_details.Rows[0]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip) + "' " +
                          "WHERE StepNum = " + dic_step[lis_data[i]] + " AND BottleNum = " + i_minBottleNo + " AND " +
                          "CupNum = " + lis_data[i] + ";");
                    }


                }
                //吸光度加药
                else if (i_type == 2)
                {
                    double d_blRErr = 0;
                    if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                        d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                            _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                    else
                        d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                            _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_B_Weight)) : Convert.ToDouble(string.Format("{0:F3}", _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_B_Weight));
                    ;


                    if (0.00 != _d_reviewBalance)
                    {
                        //FADM_Object.Communal._fadmSqlserver.ReviseData(
                        //  "UPDATE dye_details SET RealDropWeight = ObjectDropWeight + IsNULL(Compensation,0.0) + " + d_blRErr + ", Cooperate = 0 , Finish = 1 " + " ,BrewingData = '" + dt_bottle_details.Rows[0]["BrewingData"].ToString() + "' " +
                        //  "WHERE StepNum = " + dic_step[kvp.Key] + " AND BottleNum = " + i_bottleNo + " AND " +
                        //  "CupNum = " + kvp.Key + ";");

                        if (_i_AddType == 1)
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                              "UPDATE abs_cup_details SET RealSampleDosage = SampleDosage + " + d_blRErr +
                              " WHERE CupNum = " + lis_data[i] + ";");
                        }
                        else
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                              "UPDATE abs_cup_details SET RealAdditivesDosage = AdditivesDosage + " + d_blRErr +
                              " WHERE CupNum = " + lis_data[i] + ";");
                        }


                        //更新后先把加药量取出
                        string s_sql2 = null;
                        if (_i_AddType == 1)
                        {
                            s_sql2 = "SELECT RealSampleDosage FROM abs_cup_details WHERE  CupNum = " + lis_data[i] + ";";
                        }
                        else
                        {
                            s_sql2 = "SELECT RealAdditivesDosage FROM abs_cup_details WHERE  CupNum = " + lis_data[i] + ";";
                        }
                        DataTable dt_dye_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql2);

                        if (dt_dye_details.Rows.Count > 0)
                        {
                            //母液瓶扣减
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + dt_dye_details.Rows[0][0].ToString() + " " +
                                "WHERE BottleNum = '" + i_minBottleNo + "';");
                        }
                    }
                    else
                    {
                        if (_i_AddType == 1)
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                              "UPDATE abs_cup_details SET RealSampleDosage = SampleDosage + " + d_blRErr +
                              " WHERE CupNum = " + lis_data[i] + ";");
                        }
                        else
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                              "UPDATE abs_cup_details SET RealAdditivesDosage = AdditivesDosage + " + d_blRErr +
                              " WHERE CupNum = " + lis_data[i] + ";");
                        }
                    }
                }
                //吸光度配方滴液
                else if (i_type == 3)
                {
                    double d_blRErr = 0;
                    if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                        d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                            _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                    else
                        d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}",
                            _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_B_Weight)) : Convert.ToDouble(string.Format("{0:F3}", _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_B_Weight));

                    //查询开料日期
                    DataTable dt_bottle_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM bottle_details WHERE  BottleNum = " + i_minBottleNo + ";");

                    if (0.00 != _d_reviewBalance)
                    {
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                        "UPDATE abs_drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + d_blRErr + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                        "WHERE BatchName = '" + s_batchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                        "CupNum = " + lis_data[i] + ";");

                        DataTable dt_drop_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                            "SELECT * FROM abs_drop_details WHERE BatchName = '" + s_batchName + "' AND BottleNum = " + i_minBottleNo + " AND CupNum = " + lis_data[i] + ";");
                        //FADM_Object.Communal._fadmSqlserver.ReviseData(
                        //    "UPDATE cup_details SET TotalWeight = TotalWeight+ " + dt_drop_details2.Rows[0]["RealDropWeight"] + " WHERE CupNum = " + kvp.Key + ";");

                        //母液瓶扣减
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + dt_drop_details2.Rows[0]["RealDropWeight"] + " " +
                            "WHERE BottleNum = '" + i_minBottleNo + "';");
                    }
                    else
                    {
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                       "UPDATE abs_drop_details SET Finish = 1,RealDropWeight = 0.00 " + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                       "WHERE BatchName = '" + s_batchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                       "CupNum = " + lis_data[i] + ";");
                    }
                }

                //double d_blRErr = 0;
                //if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                //    d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_S_Weight));
                //else
                //    d_blRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_B_Weight)) : Convert.ToDouble(string.Format("{0:F3}", _d_reviewBalance - Lib_Card.Configure.Parameter.Correcting_B_Weight));

                ////查询开料日期
                //DataTable dt_bottle_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                //            "SELECT * FROM bottle_details WHERE  BottleNum = " + i_minBottleNo + ";");

                //if (0.00 != _d_reviewBalance)
                //{
                //    FADM_Object.Communal._fadmSqlserver.ReviseData(
                //    "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + d_blRErr + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                //    "WHERE BatchName = '" + s_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND " +
                //    "CupNum = " + lis_data[i] + ";");

                //    DataTable dt_drop_details2 = FADM_Object.Communal._fadmSqlserver.GetData(
                //        "SELECT * FROM drop_details WHERE BatchName = '" + s_batchName.ToString() + "' AND BottleNum = " + i_minBottleNo + " AND CupNum = " + lis_data[i] + ";");
                //    if (dt_drop_details2.Rows.Count > 0)
                //    {

                //        //记录是否修正数据
                //        bool b_change = false;
                //        double d_realDrop = 0.0;
                //        if (FADM_Object.Communal._b_isCorrect)
                //        {
                //            //如果实际滴液量为负数，就改为0
                //            if (Convert.ToDouble(dt_drop_details2.Rows[0]["RealDropWeight"]) < 0)
                //            {
                //                b_change = true;
                //                FADM_Object.Communal._fadmSqlserver.ReviseData(
                //        "UPDATE drop_details SET Finish = 1,RealDropWeight = 0   ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                //        "WHERE BatchName = '" + s_batchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                //        "CupNum = " + lis_data[i] + ";");
                //            }

                //            //判断当目标滴液量大于1g，而且实际滴液误差小于等于0.05的，全部置为0.02
                //            if (Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) > 1)
                //            {
                //                if (d_blRErr <= 0.05 && d_blRErr > 0.02)
                //                {
                //                    b_change = true;
                //                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                //        "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + 0.02 + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                //        "WHERE BatchName = '" + s_batchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                //        "CupNum = " + lis_data[i] + ";");
                //                    d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) + 0.02;
                //                }
                //                else if (d_blRErr < -0.02 && d_blRErr >= -0.05)
                //                {
                //                    b_change = true;
                //                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                //        "UPDATE drop_details SET Finish = 1,RealDropWeight = ObjectDropWeight + " + (-0.02) + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                //        "WHERE BatchName = '" + s_batchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                //        "CupNum = " + lis_data[i] + ";");
                //                    d_realDrop = Convert.ToDouble(dt_drop_details2.Rows[0]["ObjectDropWeight"]) - 0.02;
                //                }
                //            }
                //        }
                //        if (b_change)
                //        {
                //            FADM_Object.Communal._fadmSqlserver.ReviseData(
                //            "UPDATE cup_details SET TotalWeight = TotalWeight+ " + d_realDrop + " WHERE CupNum = " + lis_data[i] + ";");

                //            //母液瓶扣减
                //            FADM_Object.Communal._fadmSqlserver.ReviseData(
                //                "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_realDrop + " " +
                //                "WHERE BottleNum = '" + i_minBottleNo + "';");
                //        }
                //        else
                //        {
                //            FADM_Object.Communal._fadmSqlserver.ReviseData(
                //            "UPDATE cup_details SET TotalWeight = TotalWeight+ " + dt_drop_details2.Rows[0]["RealDropWeight"] + " WHERE CupNum = " + lis_data[i] + ";");

                //            //母液瓶扣减
                //            FADM_Object.Communal._fadmSqlserver.ReviseData(
                //                "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + dt_drop_details2.Rows[0]["RealDropWeight"] + " " +
                //                "WHERE BottleNum = '" + i_minBottleNo + "';");
                //        }
                //    }
                //}
                //else
                //{
                //    FADM_Object.Communal._fadmSqlserver.ReviseData(
                //   "UPDATE drop_details SET Finish = 1,RealDropWeight = 0.00 " + " ,BrewingData = '" + dt_bottle_details2.Rows[0]["BrewingData"].ToString() + "' " +
                //   "WHERE BatchName = '" + s_batchName + "' AND BottleNum = " + i_minBottleNo + " AND " +
                //   "CupNum = " + lis_data[i] + ";");
                //}
            }

            return 0;
        }


        /// <summary>
        /// 加药封装
        /// Type：类型，0代表滴液调用 1代表后处理调用
        ///  i_special:是否跳过天平复称和预滴液 1是跳过 0是不跳过
        /// _i_minBottleNo：母液瓶号
        /// _s_syringeType：针筒类型
        /// _i_pulseT：总脉冲
        /// _i_adjust:校正脉冲
        ///  _s_unitOfAccount：单位
        ///  iLowSrart：当前状态
        ///  _obj_batchName：所属批次
        ///  _dic_pulse：配液杯对应加药脉冲
        ///  directoryWeight：配液杯对应加药重
        ///  lWater:后处理加药时先加水的重量
        /// </summary>
        /// /// <returns>0：正常；-1：夹不到针筒退出；-2：收到退出消息</returns>

        public static int BrewAddMac(sAddArg o, ref Dictionary<int, double> directoryReturn, int i_type, int i_special)
        {
            List<int> lis_data = new List<int>();
            string s_batchName = o._obj_batchName;
            int i_minBottleNo = o._i_minBottleNo;
            string s_syringeType = o._s_syringeType;
            int i_pulseT = o._i_pulseT;
            int i_adjust = o._i_adjust;
            string s_unitOfAccount = o._s_unitOfAccount;

            Dictionary<int, int> dic_pulse = o._dic_pulse;
            Dictionary<int, double> dic_water = o._dic_water;
            Dictionary<int, int> dic_step = o._dic_step;
            int i_stationnum= o._i_stationnum;
            Thread thread = null;

            //记录初始天平读数
            double d_blBalanceValue3 = 0;
            if (FADM_Object.Communal._b_isStableRead)
                d_blBalanceValue3 = SteBalance();
            else
            {
                if (Lib_Card.Configure.Parameter.Machine_Type == 0)
                {
                    d_blBalanceValue3 = Lib_Card.Configure.Parameter.Machine_BalanceType == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal.dBalanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal.dBalanceValue));
                }
                else
                {
                    d_blBalanceValue3 = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal._s_balanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal._s_balanceValue));
                }
            }
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平读数：" + d_blBalanceValue3);

            int i_xStart = 0;
            int i_yStart = 0;
            int i_mRes3;

            if (!FADM_Object.Communal._b_OriginalSyBrew)
            {
                //把原有针筒先移走
                if (FADM_Object.Communal._b_isNeedMoveSy)
                {
                    //计算针筒位置
                    MyModbusFun.CalTarget(0, i_minBottleNo, ref i_xStart, ref i_yStart);


                lab1:
                    try
                    {
                        i_mRes3 = MyModbusFun.GetSyringes(i_xStart, i_yStart);
                        if (-2 == i_mRes3)
                            throw new Exception("收到退出消息");
                    }
                    catch (Exception ex)
                    {
                        if ("未发现针筒" == ex.Message || "未发现抓手" == ex.Message)
                        {
                            FADM_Object.MyAlarm myAlarm;
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + "号母液瓶未发现针筒，请确认无误后点确定", "BrewAdd", 2, 2, 100);
                            else
                                myAlarm = new FADM_Object.MyAlarm("No syringe was found in the" + i_minBottleNo + "  mother liquor bottle. Please confirm if there are no errors and click OK", "BrewAdd", 2, 2, 100);

                            while (true)
                            {
                                if (myAlarm._i_alarm_Choose != 0)
                                    break;
                                Thread.Sleep(1);
                            }

                            goto lab1;
                        }
                        else if ("取针筒时抓手关失败" == ex.Message)
                        {
                            FADM_Object.MyAlarm myAlarm;
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + "号母液瓶取针筒时抓手关失败，请确认无误后点确定", "BrewAdd", 2, 2, 100);
                            else
                                myAlarm = new FADM_Object.MyAlarm("No syringe was found in the" + i_minBottleNo + "  mother liquor bottle. Please confirm if there are no errors and click OK", "BrewAdd", 2, 2, 100);

                            while (true)
                            {
                                if (myAlarm._i_alarm_Choose != 0)
                                    break;
                                Thread.Sleep(1);
                            }
                            goto lab1;

                        }
                        else if ("取针筒时气缸下失败" == ex.Message)
                        {
                            FADM_Object.MyAlarm myAlarm;
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + "号母液瓶取针筒时气缸下失败，请确认无误后点确定", "BrewAdd", 2, 2, 100);
                            else
                                myAlarm = new FADM_Object.MyAlarm("No syringe was found in the" + i_minBottleNo + "  mother liquor bottle. Please confirm if there are no errors and click OK", "BrewAdd", 2, 2, 100);

                            while (true)
                            {
                                if (myAlarm._i_alarm_Choose != 0)
                                    break;
                                Thread.Sleep(1);
                            }
                            goto lab1;

                        }
                        else
                            throw;
                    }


                    i_mRes3 = MyModbusFun.TargetMove(19, 11, 1);
                    if (-2 == i_mRes3)
                        throw new Exception("收到退出消息");

                    //放针
                    i_mRes3 = MyModbusFun.Put(0);
                    if (-2 == i_mRes3)
                        throw new Exception("收到退出消息");
                }


            //抓取抽液针筒
            lab2:
                //if (!Communal._b_isGetSyringes)
                {
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "拿针筒启动");
                    //int i_xStart = 0;
                    //int i_yStart = 0;
                    //计算针筒位置
                    MyModbusFun.CalTarget(11, 0, ref i_xStart, ref i_yStart);
                    try
                    {
                        i_mRes3 = MyModbusFun.GetSyringes(i_xStart, i_yStart);
                        if (-2 == i_mRes3)
                            throw new Exception("收到退出消息");
                    }
                    catch (Exception ex)
                    {
                        if ("未发现针筒" == ex.Message || "未发现抓手" == ex.Message)
                        {
                            FADM_Object.MyAlarm myAlarm;
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm("抽液针筒取针筒时未发现针筒，请确认无误后点确定", "BrewAdd", 2, 2, 100);

                            else
                                myAlarm = new FADM_Object.MyAlarm("No syringe was found in the" + i_minBottleNo + "  mother liquor bottle. Please confirm if there are no errors and click OK", "BrewAdd", 2, 2, 100);

                            while (true)
                            {
                                if (myAlarm._i_alarm_Choose != 0)
                                    break;
                                Thread.Sleep(1);
                            }

                            goto lab2;
                        }
                        else if ("取针筒时抓手关失败" == ex.Message)
                        {
                            FADM_Object.MyAlarm myAlarm;
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm("抽液针筒取针筒时抓手关失败，请确认无误后点确定", "BrewAdd", 2, 2, 100);
                            else
                                myAlarm = new FADM_Object.MyAlarm("No syringe was found in the" + i_minBottleNo + "  mother liquor bottle. Please confirm if there are no errors and click OK", "BrewAdd", 2, 2, 100);

                            while (true)
                            {
                                if (myAlarm._i_alarm_Choose != 0)
                                    break;
                                Thread.Sleep(1);
                            }
                            goto lab2;

                        }
                        else if ("取针筒时气缸下失败" == ex.Message)
                        {
                            FADM_Object.MyAlarm myAlarm;
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm("抽液针筒取针筒时气缸下失败，请确认无误后点确定", "BrewAdd", 2, 2, 100);
                            else
                                myAlarm = new FADM_Object.MyAlarm("No syringe was found in the" + i_minBottleNo + "  mother liquor bottle. Please confirm if there are no errors and click OK", "BrewAdd", 2, 2, 100);

                            while (true)
                            {
                                if (myAlarm._i_alarm_Choose != 0)
                                    break;
                                Thread.Sleep(1);
                            }
                            goto lab2;

                        }
                        else
                            throw;
                    }
                    FADM_Object.Communal._b_isGetSyringes = false;

                    FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "拿针筒完成");
                }
            }

        label9:


            int i_mRes;
            if (i_minBottleNo == 999)
            {
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找1号(吸光度杯)");
                FADM_Object.Communal._i_optBottleNum = i_minBottleNo;
                i_mRes = MyModbusFun.TargetMove(10, 1, 0);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达1号(吸光度杯)");
            }
            else
            {

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找" + i_minBottleNo + "号母液瓶");
                FADM_Object.Communal._i_optBottleNum = i_minBottleNo;
                i_mRes = MyModbusFun.TargetMove(0, i_minBottleNo, 1);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达" + i_minBottleNo + "号母液瓶");
            }

            int iExtractionPulse = 0;
            if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
            {
                iExtractionPulse = (i_pulseT );
                //FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "脉冲(" + i_pulseT + ")"+ "校正值(" + i_adjust + ")" + "Other_Z_BackPulse(" + Lib_Card.Configure.Parameter.Other_Z_BackPulse + ")" + "Other_S_MaxPulse(" + Lib_Card.Configure.Parameter.Other_S_MaxPulse + ")" + "_b_isFullDrip(" + (Communal._b_isFullDrip?"1":"0") + ")" + "_b_isDripReserveFirst(" + (FADM_Object.Communal._b_isDripReserveFirst ? "1" : "0") + ")");
                if (iExtractionPulse - Lib_Card.Configure.Parameter.Other_Z_BackPulse > Lib_Card.Configure.Parameter.Other_S_MaxPulse)
                {
                    //超出单针筒上限
                    iExtractionPulse = Lib_Card.Configure.Parameter.Other_S_MaxPulse + Lib_Card.Configure.Parameter.Other_Z_BackPulse;

                }
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抽液启动(" + iExtractionPulse + ")");

            label10:
                try
                {
                    iExtractionPulse = iExtractionPulse - Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                    if (i_type == 1)
                        i_mRes = MyModbusFun.Extract(iExtractionPulse, s_unitOfAccount.Equals("g/l") ? true : false, 0); //抽液
                    else
                    {
                        if (i_minBottleNo == 999)
                        {
                            //1号吸光度杯停止搅拌
                            int[] values = new int[1];
                            values[0] = 1;
                            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                            {
                                FADM_Object.Communal._tcpModBusAbs.ReConnect();
                            }

                            FADM_Object.Communal._tcpModBusAbs.Write(806, values);

                            i_mRes = MyModbusFun.AbsExtract(iExtractionPulse, true, 0); //抽液

                            values[0] = 0;
                            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                            {
                                FADM_Object.Communal._tcpModBusAbs.ReConnect();
                            }
                            FADM_Object.Communal._tcpModBusAbs.Write(806, values);
                        }

                        else
                            i_mRes = MyModbusFun.Extract(iExtractionPulse, true, 0); //抽液
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
                        if (Communal.GetValueWash(i_minBottleNo.ToString(), out value))
                        {
                            //如果之前标记过夹不到针筒，现在抽液完成，可改为夹到针筒
                            if (value[1] == 0)
                            {
                                value[1] = 1;
                                Communal.AddOrUpdateWash(i_minBottleNo.ToString(), value);
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
                            if (Communal.GetValueWash(i_minBottleNo.ToString(), out value))
                            {
                                //标记夹不到针筒
                                value[1] = 0;
                                Communal.AddOrUpdateWash(i_minBottleNo.ToString(), value);
                            }
                        }
                        return -1;

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
                            if (Communal.GetValueWash(i_minBottleNo.ToString(), out value))
                            {
                                //标记夹不到针筒
                                value[1] = 0;
                                Communal.AddOrUpdateWash(i_minBottleNo.ToString(), value);
                            }
                        }
                        return -1;

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
                            if (Communal.GetValueWash(i_minBottleNo.ToString(), out value))
                            {
                                //标记夹不到针筒
                                value[1] = 0;
                                Communal.AddOrUpdateWash(i_minBottleNo.ToString(), value);
                            }
                        }
                        return -1;

                    }
                    else
                        throw;
                }

            }
            else
            {
                iExtractionPulse = (i_pulseT );
                if (iExtractionPulse - Lib_Card.Configure.Parameter.Other_Z_BackPulse > Lib_Card.Configure.Parameter.Other_B_MaxPulse)
                {

                    //超出单针筒上限
                    iExtractionPulse = Lib_Card.Configure.Parameter.Other_B_MaxPulse + Lib_Card.Configure.Parameter.Other_Z_BackPulse;

                }
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抽液启动(" + iExtractionPulse + ")");

            label11:
                try
                {
                    iExtractionPulse = iExtractionPulse - Lib_Card.Configure.Parameter.Other_Z_BackPulse;
                    if (i_type == 1)
                        i_mRes = MyModbusFun.Extract(iExtractionPulse, s_unitOfAccount.Equals("g/l") ? true : false, 1); //抽液
                    else
                    {
                        if (i_minBottleNo == 999)
                        {
                            //1号吸光度杯停止搅拌
                            int[] values = new int[1];
                            values[0] = 1;
                            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                            {
                                FADM_Object.Communal._tcpModBusAbs.ReConnect();
                            }

                            FADM_Object.Communal._tcpModBusAbs.Write(806, values);

                            i_mRes = MyModbusFun.AbsExtract(iExtractionPulse, true, 1); //抽液

                            values[0] = 0;
                            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                            {
                                FADM_Object.Communal._tcpModBusAbs.ReConnect();
                            }

                            FADM_Object.Communal._tcpModBusAbs.Write(806, values);
                        }
                        else
                            i_mRes = MyModbusFun.Extract(iExtractionPulse, true, 1); //抽液
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
                        if (Communal.GetValueWash(i_minBottleNo.ToString(), out value))
                        {
                            //如果之前标记过夹不到针筒，现在抽液完成，可改为夹到针筒
                            if (value[1] == 0)
                            {
                                value[1] = 1;
                                Communal.AddOrUpdateWash(i_minBottleNo.ToString(), value);
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
                            if (Communal.GetValueWash(i_minBottleNo.ToString(), out value))
                            {
                                //标记夹不到针筒
                                value[1] = 0;
                                Communal.AddOrUpdateWash(i_minBottleNo.ToString(), value);
                            }
                        }
                        return -1;


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
                            if (Communal.GetValueWash(i_minBottleNo.ToString(), out value))
                            {
                                //标记夹不到针筒
                                value[1] = 0;
                                Communal.AddOrUpdateWash(i_minBottleNo.ToString(), value);
                            }
                        }
                        return -1;


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
                            if (Communal.GetValueWash(i_minBottleNo.ToString(), out value))
                            {
                                //标记夹不到针筒
                                value[1] = 0;
                                Communal.AddOrUpdateWash(i_minBottleNo.ToString(), value);
                            }
                        }
                        return -1;


                    }
                    else
                        throw;
                }
            }
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抽液完成");




            //移动到配液杯

            List<int> lis_ints = new List<int>();
        label12:
            //if (FADM_Auto.Drip._b_dripStop)
            //{
            //    FADM_Object.Communal._b_stop = true;
            //}
            int i_zPulse = 0;
        label99:
            if (-1 == MyModbusFun.GetZPosition(ref i_zPulse))
                throw new Exception("驱动异常");
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "Z轴脉冲" + i_zPulse);
            //if (i_zPulse > 0)
            //    goto label99;


            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找开稀工位");

            //FADM_Object.Communal._i_OptCupNum = dic_pulse.First().Key;
            i_mRes = MyModbusFun.TargetMove(16, 6, 1); ;
            if (-2 == i_mRes)
                throw new Exception("收到退出消息");
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达开稀工位");





            int iInfusionPulse = 0;
            if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
            {
                if (i_zPulse >= dic_pulse.First().Value)
                {
                    //剩余量大于等于需加量
                    iInfusionPulse = i_zPulse - dic_pulse.First().Value;

                    i_pulseT -= dic_pulse.First().Value;
                    lis_data.Add(dic_pulse.First().Key);
                    dic_pulse.Remove(dic_pulse.First().Key);
                }
                else
                {
                    iInfusionPulse = 0;
                    dic_pulse[dic_pulse.First().Key] -= i_zPulse ;
                    i_pulseT -= i_zPulse ;
                }



                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液启动(" + iInfusionPulse + ")");

                i_mRes = MyModbusFun.BrewShove(iInfusionPulse, 0);

                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");


            }
            else
            {
                if (i_zPulse  >= dic_pulse.First().Value)
                {
                    //剩余量大于等于需加量
                    iInfusionPulse = i_zPulse - dic_pulse.First().Value;


                    i_pulseT -= dic_pulse.First().Value;
                    lis_data.Add(dic_pulse.First().Key);
                    dic_pulse.Remove(dic_pulse.First().Key);
                }
                else
                {
                    iInfusionPulse = 0;
                    dic_pulse[dic_pulse.First().Key] -= i_zPulse;
                    i_pulseT -= i_zPulse;
                }

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液启动(" + iInfusionPulse + ")");

                i_mRes = MyModbusFun.BrewShove(iInfusionPulse, 1);

                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");

            }


            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "注液完成");

            //判断当前母液是否滴完
            if (0 < dic_pulse.Count)
            {
                goto label9;
            }

            //读取验证重量
            sArg sArg = new sArg();
            sArg._i_minBottleNo = i_minBottleNo;
            sArg._d_blBalanceValue3 = d_blBalanceValue3;
            sArg._s_syringeType = s_syringeType;
            sArg._s_unitOfAccount = s_unitOfAccount;
            sArg._i_stationNum = i_stationnum;
            thread = new Thread(GetBalance);
            thread.Start(sArg);


            if (FADM_Object.Communal._b_OriginalSyBrew)
            {

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找" + i_minBottleNo + "号母液瓶");
                FADM_Object.Communal._i_optBottleNum = i_minBottleNo;
                i_mRes = MyModbusFun.TargetMove(0, i_minBottleNo, 0);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达" + i_minBottleNo + "号母液瓶");



                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", i_minBottleNo + "号母液瓶放针启动");

                if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
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
            }
            else
            {
                //清洗抽液针筒

                i_mRes = MyModbusFun.TargetMove(12, 0, 1);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");

                MyModbusFun.WashSyringes(1);

                //放回抽液针筒
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "寻找抽液针筒位置");
                i_mRes = MyModbusFun.TargetMove(11, 0, 0);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");

                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "抵达抽液针筒位置");

                int iMRes_1 = MyModbusFun.Put(1);
                if (-2 == iMRes_1)
                    throw new Exception("收到退出消息");

                Communal._b_isGetSyringes = false;

                if (FADM_Object.Communal._b_isNeedMoveSy)
                {
                    //移回原来针筒
                    //计算针筒位置


                    MyModbusFun.CalTarget(19, 11, ref i_xStart, ref i_yStart);
                lab3:
                    try
                    {
                        i_mRes3 = MyModbusFun.GetSyringes(i_xStart, i_yStart);
                        if (-2 == i_mRes3)
                            throw new Exception("收到退出消息");
                    }
                    catch (Exception ex)
                    {
                        if ("未发现针筒" == ex.Message || "未发现抓手" == ex.Message)
                        {
                            FADM_Object.MyAlarm myAlarm;
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + "号母液瓶放针位未发现针筒，请确认无误后点确定", "BrewAdd", 2, 2, 100);
                            else
                                myAlarm = new FADM_Object.MyAlarm("No syringe was found in the" + i_minBottleNo + "  mother liquor bottle. Please confirm if there are no errors and click OK", "BrewAdd", 2, 2, 100);

                            while (true)
                            {
                                if (myAlarm._i_alarm_Choose != 0)
                                    break;
                                Thread.Sleep(1);
                            }

                            goto lab3;
                        }
                        else if ("取针筒时抓手关失败" == ex.Message)
                        {
                            FADM_Object.MyAlarm myAlarm;
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + "号母液瓶放针位取针筒时抓手关失败，请确认无误后点确定", "BrewAdd", 2, 2, 100);
                            else
                                myAlarm = new FADM_Object.MyAlarm("No syringe was found in the" + i_minBottleNo + "  mother liquor bottle. Please confirm if there are no errors and click OK", "BrewAdd", 2, 2, 100);

                            while (true)
                            {
                                if (myAlarm._i_alarm_Choose != 0)
                                    break;
                                Thread.Sleep(1);
                            }
                            goto lab3;

                        }
                        else if ("取针筒时气缸下失败" == ex.Message)
                        {
                            FADM_Object.MyAlarm myAlarm;
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                myAlarm = new FADM_Object.MyAlarm(i_minBottleNo + "号母液瓶放针位取针筒时气缸下失败，请确认无误后点确定", "BrewAdd", 2, 2, 100);
                            else
                                myAlarm = new FADM_Object.MyAlarm("No syringe was found in the" + i_minBottleNo + "  mother liquor bottle. Please confirm if there are no errors and click OK", "BrewAdd", 2, 2, 100);

                            while (true)
                            {
                                if (myAlarm._i_alarm_Choose != 0)
                                    break;
                                Thread.Sleep(1);
                            }
                            goto lab3;

                        }
                        else
                            throw;
                    }
                    FADM_Object.Communal._b_isGetSyringes = false;

                    i_mRes3 = MyModbusFun.TargetMove(0, i_minBottleNo, 1);
                    if (-2 == i_mRes3)
                        throw new Exception("收到退出消息");

                    //放针
                    i_mRes3 = MyModbusFun.Put(0);
                    if (-2 == i_mRes3)
                        throw new Exception("收到退出消息");
                }
            }


            //读取加药量
            for (int i = 0; i < lis_data.Count; i++)
            {
                //对应值复称值写入
                directoryReturn.Add(lis_data[i], _d_reviewBalance);
            }
            thread.Join();
            return 0;
        }

        /// <summary>
        /// 移动母液瓶 1母液区到洗瓶位  2洗瓶位到开料工位 3开料工位到洗瓶位 4开料工位到母液区 5废液桶到校正工位 6校正工位到废液桶
        /// 7洗瓶位到母液区 8母液区到开料工位
        /// </summary>
        public static void MoveBottle(int i_cupNo, int i_type)
        {
            try
            {
                int i_cupNO = i_cupNo;
                int i_xStart = 0, i_yStart = 0;
                int i_xEnd = 0, i_yEnd = 0;
                int i_mRes = 0;
                //等待机械手状态
                while (true)
                {
                    //判断当前机台状态
                    //滴液过程，可抢占线程
                    if (7 == FADM_Object.Communal.ReadMachineStatus())
                    {
                        if (FADM_Object.Communal.ReadDripWait())
                            break;
                    }
                    //待机状态下直接进入
                    else if (0 == FADM_Object.Communal.ReadMachineStatus())
                        break;
                    //其他状态下直接等待完成后再进行

                    if (i_type == 5 || i_type == 6)
                        break;

                    Thread.Sleep(1);
                }


            label3:
                try
                {
                    if (i_type == 1)
                    {
                        if (!Communal._b_SendStopCupNum.Contains(6))
                            Communal._b_SendStopCupNum.Add(6);
                    }
                    else if (i_type == 2 || i_type == 3)
                    {
                        if (!Communal._b_SendStopCupNum.Contains(i_cupNo))
                            Communal._b_SendStopCupNum.Add(i_cupNo);
                        if (!Communal._b_SendStopCupNum.Contains(6))
                            Communal._b_SendStopCupNum.Add(6);
                    }
                    else if (i_type == 4)
                    {
                        if (!Communal._b_SendStopCupNum.Contains(i_cupNo))
                            Communal._b_SendStopCupNum.Add(i_cupNo);
                    }
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "拿粉筒夹子启动");

                    //i_mRes = MyModbusFun.TargetMove(3, 0, 1);
                    //if (-2 == i_mRes)
                    //    throw new Exception("收到退出消息");

                    if (Communal._b_isGetWetClamp)
                    {
                        //3.放夹子
                        FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "放夹子启动");
                        //int i_xStart = 0, i_yStart = 0;
                        //计算湿布布夹子位置
                        MyModbusFun.CalTarget(9, 0, ref i_xStart, ref i_yStart);
                        i_mRes = MyModbusFun.PutClamp(i_xStart, i_yStart);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");
                        FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "放夹子完成");
                    }

                    if (Communal._b_isGetDryClamp)
                    {
                        //3.放夹子
                        FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "放夹子启动");
                        //计算干布夹子位置
                        MyModbusFun.CalTarget(8, 0, ref i_xStart, ref i_yStart);
                        i_mRes = MyModbusFun.PutClamp(i_xStart, i_yStart);
                        if (-2 == i_mRes)
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
                    if (!Communal._b_isGetPowerClamp)
                    {
                        //计算粉筒夹子位置
                        MyModbusFun.CalTarget(14, 0, ref i_xStart, ref i_yStart);
                        i_mRes = MyModbusFun.GetClamp(i_xStart, i_yStart, 3);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");
                    }

                    //i_mRes = MyModbusFun.TargetMove(3, 0, 1);
                    //if (-2 == i_mRes)
                    //    throw new Exception("收到退出消息");

                    //int i_state = MyModbusFun.TargetMoveRelative(3, Convert.ToInt32(8000), Convert.ToInt32(8400), Convert.ToInt32(140000));
                    //if (i_state != 0 && i_state != -2)
                    //    throw new Exception("驱动异常");

                    //MyModbusFun.CalTarget(8, 0, ref i_xStart, ref i_yStart);
                    //i_mRes = MyModbusFun.PutClamp(i_xStart, i_yStart);
                    //if (-2 == i_mRes)
                    //    throw new Exception("收到退出消息");

                }
                catch (Exception ex)
                {

                    if ("未发现抓手" == ex.Message)
                    {
                        FADM_Object.MyAlarm myAlarm;

                            //FADM_Object.Communal._fadmSqlserver.ReviseData(
                            //          "UPDATE brew_station_details SET  Cooperate = 11 WHERE  Station = " + i_cupNo + " ;");
                            //if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            //    myAlarm = new FADM_Object.MyAlarm("洗瓶工位取瓶未发现抓手，是否继续执行?(继续执行请点是)", "MoveBottle", i_cupNo, 2, 54, true);
                            //else
                            //    myAlarm = new FADM_Object.MyAlarm("Wash Bottle Station did not find a tongs, do you want to continue? " +
                            //        "( Continue to perform please click Yes)", "MoveBottle", i_cupNo, 2, 54, true);
                       

                        throw new Exception("-97");
                        return;
                    }
                    else if ("取布夹具时抓手关失败" == ex.Message)
                    {
                        FADM_Object.MyAlarm myAlarm;
                       
                            //FADM_Object.Communal._fadmSqlserver.ReviseData(
                            //          "UPDATE brew_station_details SET  Cooperate = 11 WHERE  Station = " + i_cupNo + " ;");
                            //if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            //    myAlarm = new FADM_Object.MyAlarm("洗瓶工位取夹具时抓手关失败，是否继续执行?(继续执行请点是)", "MoveBottle", i_cupNo, 2, 54, true);
                            //else
                            //    myAlarm = new FADM_Object.MyAlarm("Wash Bottle Station The gripper failed to close when taking the fabric fixture, do you want to continue? " +
                            //        "( Continue to perform please click Yes)", "MoveBottle", i_cupNo, 2, 54, true);
                        

                        throw new Exception("-98");
                        return;
                    }
                    else if ("取布夹具时气缸下失败" == ex.Message)
                    {
                        FADM_Object.MyAlarm myAlarm;
                        
                            //FADM_Object.Communal._fadmSqlserver.ReviseData(
                            //          "UPDATE brew_station_details SET  Cooperate = 11 WHERE  Station = " + i_cupNo + " ;");
                            //if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            //    myAlarm = new FADM_Object.MyAlarm("洗瓶工位取夹具时气缸下失败，是否继续执行?(继续执行请点是)", "MoveBottle", i_cupNo, 2, 54, true);
                            //else
                            //    myAlarm = new FADM_Object.MyAlarm("Wash Bottle Station The cylinder failed to drop when taking the fabric fixture, do you want to continue? " +
                            //        "( Continue to perform please click Yes)", "MoveBottle", i_cupNo, 2, 54, true);
                        

                        throw new Exception("-99");
                        return;
                    }
                    else if ("发现杯盖或针筒" == ex.Message)
                    {
                        FADM_Object.MyAlarm myAlarm;
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            myAlarm = new FADM_Object.MyAlarm("请先拿住针筒或杯盖，然后点确定", "MoveBottle", true, 1);
                        else
                            myAlarm = new FADM_Object.MyAlarm("Please hold the syringe or cup lid first, and then confirm", "MoveBottle", true, 1);
                        while (true)

                        {
                            if (0 != myAlarm._i_alarm_Choose)
                                break;
                            Thread.Sleep(1);
                        }
                        if (Lib_Card.Configure.Parameter.Machine_Type == 0)
                        {
                            //抓手开
                            Lib_Card.ADT8940A1.OutPut.Tongs.Tongs tongs = new Lib_Card.ADT8940A1.OutPut.Tongs.Tongs_Basic();
                            if (-1 == tongs.Tongs_Off())
                            {
                                FADM_Form.CustomMessageBox.Show("驱动异常", "Tongs_Off", MessageBoxButtons.OK, false);
                            }
                            //等5秒后继续
                            Thread.Sleep(5000);
                            goto label3;
                        }
                        else
                        {
                            //抓手开
                            int[] ia_array = new int[1];
                            ia_array[0] = 7;

                            int i_state = FADM_Object.Communal._tcpModBus.Write(811, ia_array);
                            //等5秒后继续
                            Thread.Sleep(5000);
                            goto label3;
                        }
                    }
                    else
                        throw;
                }


                FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "拿夹子完成");



                FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", i_cupNo + "号工位启动取瓶启动");

                DataTable dt_cupordye_details = FADM_Object.Communal._fadmSqlserver.GetData(
        "SELECT top 1 * FROM brew_station_details WHERE Station = " + i_cupNo + "  ;");

                if (dt_cupordye_details.Rows.Count > 0)
                {
                    ////调用取瓶动作快
                    //i_mRes = MyModbusFun.GetOrPutBottle(i_xStart, i_yStart,2);
                    //if (-2 == i_mRes)
                    //    throw new Exception("收到退出消息");

                    //2移动到取瓶位置
                    if (i_type == 1)
                    {
                        //计算目标位置
                        MyModbusFun.CalTarget(0, Convert.ToInt32(dt_cupordye_details.Rows[0]["BottleNum"].ToString()), ref i_xStart, ref i_yStart);
                        MyModbusFun.CalTarget(15, 0, ref i_xEnd, ref i_yEnd);

                    }
                    else if (i_type == 2)
                    {


                        //计算目标位置
                        MyModbusFun.CalTarget(15, 0, ref i_xStart, ref i_yStart);
                        MyModbusFun.CalTarget(2, 0, ref i_xEnd, ref i_yEnd);
                        ////拿不到转盘实际位置，发异常重新交互
                        //if (AutoBrew._cup_Temps[i_cupNo - 1]._i_position == 0)
                        //{
                        //    throw new Exception("-97");
                        //}
                    }
                    else if (i_type == 3)
                    {


                        //计算目标位置
                        MyModbusFun.CalTarget(2, 0, ref i_xStart, ref i_yStart);
                        MyModbusFun.CalTarget(15, 0, ref i_xEnd, ref i_yEnd);
                        //拿不到转盘实际位置，发异常重新交互
                        //if (AutoBrew._cup_Temps[i_cupNo - 1]._i_position == 0)
                        //{
                        //    throw new Exception("-97");
                        //}
                    }
                    else if (i_type == 4)
                    {
                        //计算目标位置
                        MyModbusFun.CalTarget(16, AutoBrew._cup_Temps[i_cupNo - 1]._i_position, ref i_xStart, ref i_yStart);
                        MyModbusFun.CalTarget(0, Convert.ToInt32(dt_cupordye_details.Rows[0]["BottleNum"].ToString()), ref i_xEnd, ref i_yEnd);
                        //拿不到转盘实际位置，发异常重新交互
                        if (AutoBrew._cup_Temps[i_cupNo - 1]._i_position == 0)
                        {
                            throw new Exception("-97");
                        }
                    }
                    else if (i_type == 5)
                    {
                        //计算目标位置
                        MyModbusFun.CalTarget(16, 6, ref i_xStart, ref i_yStart);
                        MyModbusFun.CalTarget(21, 0, ref i_xEnd, ref i_yEnd);
                        
                    }
                    else if (i_type == 6)
                    {
                        //计算目标位置
                        MyModbusFun.CalTarget(21, 0, ref i_xStart, ref i_yStart);
                        MyModbusFun.CalTarget(16, 6, ref i_xEnd, ref i_yEnd);

                    }
                    else if (i_type == 7)
                    {
                        //计算目标位置
                        MyModbusFun.CalTarget(15, 0, ref i_xStart, ref i_yStart);
                        MyModbusFun.CalTarget(0, Convert.ToInt32(dt_cupordye_details.Rows[0]["BottleNum"].ToString()), ref i_xEnd, ref i_yEnd);

                    }

                    else if (i_type == 8)
                    {
                        //计算目标位置
                        MyModbusFun.CalTarget(0, Convert.ToInt32(dt_cupordye_details.Rows[0]["BottleNum"].ToString()), ref i_xStart, ref i_yStart);
                        MyModbusFun.CalTarget(2, 0, ref i_xEnd, ref i_yEnd);

                    }

                    MyModbusFun.MoveBottle(i_xStart, i_yStart, i_xEnd, i_yEnd, i_type);



                }

                FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", "移动母液瓶完成");
            }
            catch (Exception ex)
            {

                if ("收到退出消息" == ex.Message)
                {
                    FADM_Object.Communal._b_stop = false;
                    //new Reset().MachineReset(0);
                    MyModbusFun.MyMachineReset(); //复位
                }
                else if ("-97" == ex.Message || "-98" == ex.Message || "放瓶时气缸下失败" == ex.Message || "放瓶失败" == ex.Message 
                    || "在洗瓶工位取瓶失败" == ex.Message || "在转盘工位取瓶失败" == ex.Message || "放瓶到洗瓶工位失败" == ex.Message || "放瓶到转盘工位失败" == ex.Message)
                {
                    throw;
                }

                else
                {
                    FADM_Object.Communal.WriteMachineStatus(8);

                    if (ex.Message.Equals("-2"))
                    {
                        throw;

                    }
                    else
                    {
                        string s_sql = "INSERT INTO alarm_table" +
                                 "(MyDate,MyTime,AlarmHead,AlarmDetails)" +
                                 " VALUES( '" +
                                 String.Format("{0:d}", DateTime.Now) + "','" +
                                 String.Format("{0:T}", DateTime.Now) + "','" +
                                 "PutOrGetCloth" + "','" +
                                  ex.ToString() + "(Test)');";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                        new FADM_Object.MyAlarm(ex.ToString(), "PutOrGetCloth", false, 0);
                    }

                    //string[] strArray = { "", "" };
                    //if (ex.Message.Equals("-2"))
                    //{
                    //    //根据编号读取异常信息
                    //    MyModbusFun.GetErrMsg(ref strArray);
                    //}

                    //FADM_Object.Communal.WriteMachineStatus(8);

                    //if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    //    FADM_Form.CustomMessageBox.Show(ex.Message == "-2" ? strArray[1] : ex.ToString(), "SwitchCover", MessageBoxButtons.OK, true);
                    //else
                    //{
                    //    string s_message = ex.Message;
                    //    if (SmartDyeing.FADM_Object.Communal._dic_warning.ContainsKey(ex.Message))
                    //    {
                    //        //如果存在就替换英文
                    //        s_message = SmartDyeing.FADM_Object.Communal._dic_warning[ex.Message];
                    //    }
                    //    FADM_Form.CustomMessageBox.Show(s_message, "SwitchCover", MessageBoxButtons.OK, true);
                    //}
                }
            }
        }

        public static int AddWater(double d_addWaterTime)
        {
            int i_mRes = -1;
            if (Communal._b_isUseLLJ)
            {
                //脉冲清零
                MyModbusFun.ClearFlow();
            }
            if (Lib_Card.Configure.Parameter.Other_WaterMagnification == 1)
            {
                i_mRes = MyModbusFun.AddWater(d_addWaterTime);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
            }
            else
            {
                if (Lib_Card.Configure.Parameter.Machine_Type == 0)
                {
                    i_mRes = MyModbusFun.AddWater(d_addWaterTime);
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                }
                else
                {
                    if (d_addWaterTime <= 32)
                    {
                        i_mRes = MyModbusFun.AddWater(d_addWaterTime);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");
                    }
                    else
                    {
                        double d = 32;
                        while (true)
                        {
                            if (d_addWaterTime > 32)
                            {
                                //每次减32s
                                i_mRes = MyModbusFun.AddWater(d);
                                if (-2 == i_mRes)
                                    throw new Exception("收到退出消息");
                            }
                            else
                            {
                                i_mRes = MyModbusFun.AddWater(d_addWaterTime);
                                if (-2 == i_mRes)
                                    throw new Exception("收到退出消息");
                                break;
                            }
                            d_addWaterTime -= d;
                        }
                    }
                }
            }
            //Thread.Sleep(2000);
            //MyModbusFun.ReadFlow();
            ////计算理论重量
            ////理论重量 = 脉冲*系数 - 4.6 + power(脉冲，1/4）
            //double d_ = Communal.i_flowPulse * Lib_Card.Configure.Parameter.Correcting_FlowPulse_Value - 4.6 + Math.Pow(Communal.i_flowPulse, 1 / 4);
            //double d_need = Lib_Card.Configure.Parameter.Correcting_Water_Value * d_addWaterTime;
            //FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "理论重量：" + string.Format("{0:F3}", d_) + "需加量："+ string.Format("{0:F3}", d_need) + "脉冲值：" + Communal.i_flowPulse);
            return 0;
        }

        public static int AddDissolving(double d_addWaterTime)
        {
            int i_mRes = -1;
            if (Lib_Card.Configure.Parameter.Other_WaterMagnification == 1)
            {
                i_mRes = MyModbusFun.AddDissolving(d_addWaterTime);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
            }
            else
            {
                if (Lib_Card.Configure.Parameter.Machine_Type == 0)
                {
                    i_mRes = MyModbusFun.AddDissolving(d_addWaterTime);
                    if (-2 == i_mRes)
                        throw new Exception("收到退出消息");
                }
                else
                {
                    if (d_addWaterTime <= 32)
                    {
                        i_mRes = MyModbusFun.AddDissolving(d_addWaterTime);
                        if (-2 == i_mRes)
                            throw new Exception("收到退出消息");
                    }
                    else
                    {
                        double d = 32;
                        while (true)
                        {
                            if (d_addWaterTime > 32)
                            {
                                //每次减32s
                                i_mRes = MyModbusFun.AddDissolving(d);
                                if (-2 == i_mRes)
                                    throw new Exception("收到退出消息");
                            }
                            else
                            {
                                i_mRes = MyModbusFun.AddDissolving(d_addWaterTime);
                                if (-2 == i_mRes)
                                    throw new Exception("收到退出消息");
                                break;
                            }
                            d_addWaterTime -= d;
                        }
                    }
                }
            }

            return 0;
        }

        public struct sArg
        {
            public string _s_syringeType;//针筒类型
            public int _i_minBottleNo;//母液瓶号
            public double _d_blBalanceValue3;//天平初始读数
            public string _s_unitOfAccount;//单位

            public int _i_stationNum;//工位号
        }

        private static void WaitBalance(object o)
        {
            sArg sArg = (sArg)o;
            double d_temp = sArg._d_blBalanceValue3;
            int i_minBottleNo = sArg._i_minBottleNo;
            string s_syringeType=sArg._s_syringeType;
            string s_unitOfAccount = sArg._s_unitOfAccount;
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数启动");
            double d_w = SteBalance();
            if (Math.Abs(d_w - d_temp) > 20)
            {
                d_temp = d_w;
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平复称异常，把实际重量置为0");
            }
            double d_blWeight = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_w - d_temp)) : Convert.ToDouble(string.Format("{0:F3}", d_w - d_temp));
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数：" + d_w + ",实际重量：" + d_blWeight);
            _d_reviewBalance = d_blWeight;

            //母液瓶扣减
            double dblRErr = 0;
            if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                dblRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blWeight - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", d_blWeight - Lib_Card.Configure.Parameter.Correcting_S_Weight));
            else
                dblRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blWeight - Lib_Card.Configure.Parameter.Correcting_B_Weight)) : Convert.ToDouble(string.Format("{0:F3}", d_blWeight - Lib_Card.Configure.Parameter.Correcting_B_Weight));
            ;
            if (System.Math.Abs(dblRErr) > (s_unitOfAccount == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip))

            {
                //母液瓶扣减
                FADM_Object.Communal._fadmSqlserver.ReviseData(
                    "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_blWeight + ",  AdjustSuccess = 0 " +
                    "WHERE BottleNum = '" + i_minBottleNo + "';");
            }
            else
            {
                //母液瓶扣减
                FADM_Object.Communal._fadmSqlserver.ReviseData(
                    "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_blWeight + " ,  AdjustSuccess = 1 " +
                    "WHERE BottleNum = '" + i_minBottleNo + "';");
            }
        }

        private static void GetBalance(object o)
        {
            sArg sArg = (sArg)o;
            double d_temp = sArg._d_blBalanceValue3;
            int i_minBottleNo = sArg._i_minBottleNo;
            string s_syringeType = sArg._s_syringeType;
            string s_unitOfAccount = sArg._s_unitOfAccount;
            int i_stationnum = sArg._i_stationNum;
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数启动");
            double d_w = SteBalance();
            //if (Math.Abs(d_w - d_temp) > 20)
            //{
            //    d_temp = d_w;
            //    FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平复称异常，把实际重量置为0");
            //}
            double d_blWeight = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_w - d_temp)) : Convert.ToDouble(string.Format("{0:F3}", d_w - d_temp));
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "天平稳定读数：" + d_w + ",实际重量：" + d_blWeight);
            _d_reviewBalance = d_blWeight;

            //母液瓶扣减
            double dblRErr = 0;
            if ("小针筒" == s_syringeType || "Little Syringe" == s_syringeType)
                dblRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blWeight - Lib_Card.Configure.Parameter.Correcting_S_Weight)) : Convert.ToDouble(string.Format("{0:F3}", d_blWeight - Lib_Card.Configure.Parameter.Correcting_S_Weight));
            else
                dblRErr = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blWeight - Lib_Card.Configure.Parameter.Correcting_B_Weight)) : Convert.ToDouble(string.Format("{0:F3}", d_blWeight - Lib_Card.Configure.Parameter.Correcting_B_Weight));
            ;
            
                //母液瓶扣减
                FADM_Object.Communal._fadmSqlserver.ReviseData(
                    "UPDATE bottle_details SET CurrentWeight = CurrentWeight - " + d_blWeight  +
                    " WHERE BottleNum = '" + i_minBottleNo + "';");

            int i_c = (int)(_d_reviewBalance * 1000);
            int i_d0 = 0, i_d1 = 0, i_d2 = 0;
            i_d0 = i_c;
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


            int[] ia_zero = new int[2];

            ia_zero[0] = i_d1;
            ia_zero[1] = i_d2;
            Dye.BrewAndWashHMIWrite(22326 + 40 * (i_stationnum - 1), ia_zero);

            //加粉完成
            ia_zero = new int[1];
            ia_zero[0] = 2;
            Dye.BrewAndWashHMIWrite(22289 + 40 * (i_stationnum - 1), ia_zero);

        }

        //天平稳定数据
        public static double SteBalance()
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
            label15:
                Thread.Sleep(Convert.ToInt32(Lib_Card.Configure.Parameter.Delay_Balance_Read * 1000));
                double d_blBalanceValue4 = Lib_Card.Configure.Parameter.Machine_BalanceType == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal.dBalanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal.dBalanceValue));
                Thread.Sleep(Convert.ToInt32(Lib_Card.Configure.Parameter.Delay_Balance_Read * 1000));
                double d_blBalanceValue5 = Lib_Card.Configure.Parameter.Machine_BalanceType == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal.dBalanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal.dBalanceValue));


                double d_blDif = Lib_Card.Configure.Parameter.Machine_BalanceType == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blBalanceValue4 - d_blBalanceValue5)) : Convert.ToDouble(string.Format("{0:F3}", d_blBalanceValue4 - d_blBalanceValue5));
                d_blDif = d_blDif < 0 ? -d_blDif : d_blDif;

                if (d_blDif > Lib_Card.Configure.Parameter.Other_Stable_Value || d_blBalanceValue4 > 6000 || d_blBalanceValue5 > 6000)
                {
                label4:
                    if (6666 == FADM_Object.Communal.dBalanceValue)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            new FADM_Object.MyAlarm("天平通讯异常,检查恢复后请点是", "滴液", false, 1);
                        else
                            new FADM_Object.MyAlarm("Abnormal communication on the balance,Click Yes after checking the recovery", "Drip", false, 1);
                        goto label4;
                    }
                    else if (7777 == FADM_Object.Communal.dBalanceValue)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            new FADM_Object.MyAlarm("开机未拿走废液桶,请先拿走废液桶，等待天平清零后重新放置废液桶，然后点确定", "滴液", false, 1);
                        else
                            new FADM_Object.MyAlarm("The balance was turned on but the waste liquid tank was not taken away,Please take away the waste liquid bucket first, wait for the balance to be cleared and re-place the waste liquid bucket, and then click OK", "Drip", false, 1);
                        goto label4;
                    }
                    else if (8888 == FADM_Object.Communal.dBalanceValue)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            new FADM_Object.MyAlarm("天平超下限,检查恢复后请点是", "滴液", false, 1);
                        else
                            new FADM_Object.MyAlarm("Balance exceeds the lower limit,Click Yes after checking the recovery", "Drip", false, 1);
                        goto label4;
                    }
                    else if (9999 == FADM_Object.Communal.dBalanceValue)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            new FADM_Object.MyAlarm("天平超上限,检查恢复后请点是", "滴液", false, 1);
                        else
                            new FADM_Object.MyAlarm("The balance exceeds the upper limit,Click Yes after checking the recovery", "Drip", false, 1);

                        goto label4;
                    }
                    //FADM_Object.Communal.BalanceState("滴液");
                    goto label15;
                }

                double d_blRRead = Lib_Card.Configure.Parameter.Machine_BalanceType == 0 ? Convert.ToDouble(string.Format("{0:F2}", (d_blBalanceValue4 + d_blBalanceValue5) / 2)) : Convert.ToDouble(string.Format("{0:F3}", (d_blBalanceValue4 + d_blBalanceValue5) / 2));

                return d_blRRead;
            }
            else
            {
            label15:
                Thread.Sleep(Convert.ToInt32(Lib_Card.Configure.Parameter.Delay_Balance_Read * 1000));
                double d_blBalanceValue4 = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal._s_balanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal._s_balanceValue));
                Thread.Sleep(Convert.ToInt32(Lib_Card.Configure.Parameter.Delay_Balance_Read * 1000));
                double d_blBalanceValue5 = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", FADM_Object.Communal._s_balanceValue)) : Convert.ToDouble(string.Format("{0:F3}", FADM_Object.Communal._s_balanceValue));


                double d_blDif = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", d_blBalanceValue4 - d_blBalanceValue5)) : Convert.ToDouble(string.Format("{0:F3}", d_blBalanceValue4 - d_blBalanceValue5));
                d_blDif = d_blDif < 0 ? -d_blDif : d_blDif;

                if (d_blDif > Lib_Card.Configure.Parameter.Other_Stable_Value || d_blBalanceValue4 > 6000 || d_blBalanceValue5 > 6000)
                {
                label4:
                    if ("6666" == FADM_Object.Communal._s_balanceValue)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            new FADM_Object.MyAlarm("天平通讯异常,检查恢复后请点是", "滴液", false, 1);
                        else
                            new FADM_Object.MyAlarm("Abnormal communication on the balance,Click Yes after checking the recovery", "Drip", false, 1);
                        goto label4;
                    }
                    else if ("7777" == FADM_Object.Communal._s_balanceValue)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            new FADM_Object.MyAlarm("开机未拿走废液桶,请先拿走废液桶，等待天平清零后重新放置废液桶，然后点确定", "滴液", false, 1);
                        else
                            new FADM_Object.MyAlarm("The balance was turned on but the waste liquid tank was not taken away,Please take away the waste liquid bucket first, wait for the balance to be cleared and re-place the waste liquid bucket, and then click OK", "Drip", false, 1);
                        goto label4;
                    }
                    else if ("8888" == FADM_Object.Communal._s_balanceValue)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            new FADM_Object.MyAlarm("天平超下限,检查恢复后请点是", "滴液", false, 1);
                        else
                            new FADM_Object.MyAlarm("Balance exceeds the lower limit,Click Yes after checking the recovery", "Drip", false, 1);
                        goto label4;
                    }
                    else if ("9999" == FADM_Object.Communal._s_balanceValue)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            new FADM_Object.MyAlarm("天平超上限,检查恢复后请点是", "滴液", false, 1);
                        else
                            new FADM_Object.MyAlarm("The balance exceeds the upper limit,Click Yes after checking the recovery", "Drip", false, 1);

                        goto label4;
                    }
                    //FADM_Object.Communal.BalanceState("滴液");
                    goto label15;
                }

                double d_blRRead = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? Convert.ToDouble(string.Format("{0:F2}", (d_blBalanceValue4 + d_blBalanceValue5) / 2)) : Convert.ToDouble(string.Format("{0:F3}", (d_blBalanceValue4 + d_blBalanceValue5) / 2));

                return d_blRRead;
            }
        }


        //移动到天平前，判断天平状态 
        public static void BalanceState(string s)
        {
            
                int i_mRes = MyModbusFun.CheckBalance();//检查天平
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");
            
        }
    }
}
