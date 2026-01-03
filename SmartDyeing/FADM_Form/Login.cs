using Lib_Card.ADT8940A1.OutPut.Blender;
using Lib_DataBank;
using Lib_File;
using Newtonsoft.Json.Linq;
using SmartDyeing.FADM_Object;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace SmartDyeing.FADM_Form
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("驱动异常", "Drive abnormality");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("未检查到板卡", "No board detected");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("未安装端口驱动程序", "Port driver not installed");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("PCI桥存在故障", "PCI bridge faulty");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("拨码开关设置重复", "Repeated DIP switch settings");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("拨码开关读取异常", "Abnormal reading of dial switch");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("板卡清除缓存异常", "Board clearing cache exception");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("X轴设定脉冲工作方式异常", "Abnormal working mode of X-axis pulse setting");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("X轴设定加速度异常", "X-axis set acceleration abnormality");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("Y轴设定脉冲工作方式异常", "Abnormal working mode of Y-axis pulse setting");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("Y轴设定加速度异常", "Y-axis set acceleration abnormality");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("Z轴设定脉冲工作方式异常", "Abnormal working mode of Z-axis pulse setting");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("Z轴设定加速度异常", "Z-axis set acceleration abnormality");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("X轴伺服器报警", "X-axis server alarm");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("X轴正限位已通", "The X-axis positive limit has been activated");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("X轴反限位已通", "X-axis reverse limit has been activated");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("X轴准备信号未接通", "X-axis preparation signal not connected");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("气缸下已接通", "Connected under the cylinder");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("Y轴伺服器报警", "Y-axis server alarm");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("Y轴正限位已通", "The Y-axis positive limit has been activated");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("Y轴反限位已通", "Y-axis reverse limit has been activated");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("Y轴准备信号未接通", "Y-axis preparation signal not connected");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("Z轴反限位已通", "Z-axis positive limit has been activated");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("X轴矢能未接通", "X-axis vector energy not connected");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("Y轴矢能未接通", "Y-axis vector energy not connected");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("气缸上超时", "Cylinder Up timeout");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("非法瓶号", "Illegal bottle number");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("气缸未在上限位", "The cylinder is not in the upper limit position");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("X轴正在运行", "X-axis is running");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("Y轴正在运行", "Y-axis is running");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("接液盘未收回", "The liquid receiving tray has not been retracted");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("接液盘未伸出", "The liquid receiving tray is not extended");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("Z轴正在运行", "Z-axis is running");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("转盘轴设定脉冲工作方式异常", "The pulse working mode of the turntable shaft setting is abnormal");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("转盘轴设定加速度异常", "Abnormal acceleration of the turntable shaft setting");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("转盘正在运行", "The turntable is running");
            SmartDyeing.FADM_Object.Communal._dic_warning.Add("加水正在运行", "Adding water is running");


            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(50, "X轴伺服报警");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(51, "X轴正在运行");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(52, "Y轴伺服报警");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(53, "Y轴正在运行");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(54, "提示有针筒提示拿住针筒(等待用户选择)");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(56, "抓手A打开异常");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(57, "抓手B打开异常");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(58, "抓手A夹紧异常");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(59, "抓手B夹紧异常");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(60, "气缸上异常");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(61, "气缸下异常");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(62, "光幕1遮挡");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(63, "光幕2遮挡");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(64, "上下限位异常");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(65, "X轴驱动器异常");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(66, "X轴正限位异常");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(67, "X轴失能异常");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(68, "Y轴驱动器异常");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(69, "Y轴正限位异常");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(70, "Y轴失能异常");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(71, "X轴设置回零信号异常");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(72, "X轴设置回零速度异常。");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(73, "Y轴设置回零信号异常。");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(74, "Y轴设置回零信号异常。");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(75, "接液盘伸出异常");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(76, "接液盘收回异常。");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(77, "Z轴反限位异常");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(78, "未发现针筒，继续执行请点 是，退出执行请点 否。(等待用户选择)");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(79, "未检测到针筒");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(80, "Z轴设置回零信号异常。");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(81, "Z轴设置回零速度异常。");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(82, "X轴反限位异常");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(83, "Y轴反限位异常");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(84, "Z轴正在运行");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(85, "急停");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(86, "发现针筒或杯盖");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(87, "泄压气缸上限位异常");
            SmartDyeing.FADM_Object.Communal._dic_errModbusNo.Add(88, "泄压气缸下限位异常");

            if (Lib_Card.Configure.Parameter.Other_Language == 0)
            {
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1, "X轴伺服器报警");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2, "X轴使能打开超时");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11, "X轴正在运行");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(12, "X轴使能关闭超时");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(13, "X轴报警复位失败");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(21, "X轴通讯异常");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(22, "X轴正反向限位已通");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(31, "X轴反向限位已通");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(32, "X轴正向限位已通");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(101, "Y轴伺服器报警");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(102, "Y轴使能打开超时");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(111, "Y轴正在运行");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(112, "Y轴使能关闭超时");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(113, "Y轴报警复位失败");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(121, "Y轴通讯异常");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(122, "Y轴正反向限位已通");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(131, "Y轴反向限位已通");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(132, "Y轴正向限位已通");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(201, "Z轴伺服器报警");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(202, "Z轴使能打开超时");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(211, "Z轴正在运行");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(212, "Z轴使能关闭超时");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(213, "Z轴报警复位失败");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(221, "Z轴通讯异常");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(222, "Z轴正反向限位已通");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(231, "Z轴反向限位已通");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(232, "Z轴正向限位已通");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(241, "Z轴目标位置超行程");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(242, "Z轴计算脉冲超量程");


                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(301, "气缸上超时");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(302, "气缸下已通");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(311, "气缸单输出点不存在气缸中信号");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(312, "气缸中超时");


                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(321, "气缸上已通");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(322, "气缸下超时");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(401, "抓手A打开超时");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(402, "抓手B打开超时");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(411, "抓手A关闭超时");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(412, "抓手B关闭超时");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(421, "抓手A开到位信号已通");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(422, "抓手B开到位信号已通");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(423, "抓手A泄压超时");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(424, "抓手B泄压超时");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(425, "抓手泄压超时");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(501, "接液盘伸出超时");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(502, "接液盘回限位已通");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(511, "接液盘收回超时");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(512, "接液盘出限位已通");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(601, "泄压气缸上超时");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(602, "泄压气缸下已通");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(611, "泄压气缸下超时");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(612, "泄压气缸上已通");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2001, "非法杯号");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2002, "非法区域");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2101, "未发现针筒");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2102, "取针筒时抓手关失败");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2103, "取针筒时气缸下失败");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2401, "发现针筒");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2701, "发现杯盖或针筒");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2702, "未发现杯盖");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2703, "配液杯取盖失败");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2704, "放盖区取盖失败");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2705, "关盖失败");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2706, "放盖失败");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2707, "二次关盖复压失败");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2708, "二次关盖失败");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2709, "二次关盖未发现杯盖");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2710, "移动前抓手打开异常");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2711, "移动前抓手打开关闭都发现异常");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2712, "感应杯盖时打开抓手失败");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2713, "配液杯取盖失败抓手关失败");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2714, "放盖区取盖失败抓手关失败");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2715, "放盖后抓手打开失败");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2716, "关盖时抓手打开失败");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2717, "关盖时撑盖开失败");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2718, "关盖撑盖时气缸下失败");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2719, "开盖前染杯无上锁止");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2720, "关盖前染杯无上锁止");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2730, "关盖时染杯无上锁止");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(3301, "天平通讯异常,检查恢复后请点是");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(3302, "天平开机未拿走废液桶,请先拿走废液桶，等待天平清零后重新放置废液桶，然后点确定");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(3303, "天平超下限,检查恢复后请点是");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(3304, "天平超上限,检查恢复后请点是");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(3305, "请先清空废液桶，然后点确定");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(3306, "请先拿走废液桶，等待天平自动归零后，再放置废液桶");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1101, "阻挡回限位已通");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1102, "阻挡伸出超时");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1111, "阻挡出限位已通");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1112, "阻挡收回超时");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1113, "气缸在阻挡位置");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(4501, "未发现抓手");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(4502, "取布夹具时抓手关失败");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(4503, "取布夹具时气缸下失败");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(4504, "称粉电机回原点失败");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(5411, "放瓶时气缸下失败");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(5412, "放瓶失败");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(5413, "在洗瓶工位取瓶失败");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(5414, "在转盘工位取瓶失败");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(5415, "放瓶到洗瓶工位失败");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(5416, "放瓶到转盘工位失败");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(5421, "加粉超时");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1120, "气缸阻挡位与气缸下同时接通");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1121, "气缸单输出点不存在气缸慢速中信号");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1122, "气缸阻挡限位已通");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1123, "气缸慢速中超时");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1124, "气缸阻挡和气缸下限位同时接通");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1125, "气缸上和气缸阻挡限位同时接通");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1126, "气缸上和气缸下限位同时接通");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1127, "气缸上与慢速中限位同时接通");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1128, "慢速中限位和气缸阻挡位同时接通");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1129, "慢速中限位与气缸下已通");


                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1131, "气缸到阻挡位超时");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1118, "气缸慢速中2超时");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1119, "气缸慢速中3超时");


                //大于10000就是询问
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10301, "气缸上超时,请检查，排除异常请点是，退出运行请点否");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10302, "气缸下已通,请检查，排除异常请点是，退出运行请点否");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10312, "气缸中超时,请检查，排除异常请点是，退出运行请点否");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10321, "气缸上已通,请检查，排除异常请点是，退出运行请点否");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10322, "气缸下超时,请检查，排除异常请点是，退出运行请点否");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10401, "抓手A打开超时,请检查，排除异常请点是，退出运行请点否");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10402, "抓手B打开超时,请检查，排除异常请点是，退出运行请点否");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10421, "抓手A开到位信号已通,请检查，排除异常请点是，退出运行请点否");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10422, "抓手B开到位信号已通,请检查，排除异常请点是，退出运行请点否");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10423, "抓手A泄压超时,请检查，排除异常请点是，退出运行请点否");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10424, "抓手B泄压超时,请检查，排除异常请点是，退出运行请点否");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10425, "抓手泄压超时,请检查，排除异常请点是，退出运行请点否");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10411, "抓手A关闭超时,请检查，排除异常请点是，退出运行请点否");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10412, "抓手B关闭超时,请检查，排除异常请点是，退出运行请点否");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10501, "接液盘伸出超时,请检查，排除异常请点是，退出运行请点否");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10502, "接液盘回限位已通,请检查，排除异常请点是，退出运行请点否");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10511, "接液盘收回超时,请检查，排除异常请点是，退出运行请点否");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10512, "接液盘出限位已通,请检查，排除异常请点是，退出运行请点否");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10601, "泄压气缸上超时,请检查，排除异常请点是，退出运行请点否");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10602, "泄压气缸下已通,请检查，排除异常请点是，退出运行请点否");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10611, "泄压气缸下超时,请检查，排除异常请点是，退出运行请点否");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10612, "泄压气缸上已通,请检查，排除异常请点是，退出运行请点否");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(12401, "发现针筒,点击确定10秒后打开抓手");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(12701, "发现杯盖或针筒");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(13301, "天平通讯异常,检查恢复后请点是");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(13302, "天平开机未拿走废液桶,请先拿走废液桶，等待天平清零后重新放置废液桶，然后点确定");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(13303, "天平超下限,检查恢复后请点是");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(13304, "天平超上限,检查恢复后请点是");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(13305, "请先清空废液桶，然后点确定");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(13306, "请先拿走废液桶，等待天平自动归零后，再放置废液桶");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11101, "阻挡回限位已通,请检查，排除异常请点是，退出运行请点否");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11102, "阻挡伸出超时,请检查，排除异常请点是，退出运行请点否");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11111, "阻挡出限位已通,请检查，排除异常请点是，退出运行请点否");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11112, "阻挡收回超时,请检查，排除异常请点是，退出运行请点否");



                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11120, "气缸阻挡位与气缸下同时接通,请检查，排除异常请点是，退出运行请点否");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11122, "气缸阻挡限位已通,请检查，排除异常请点是，退出运行请点否");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11123, "气缸慢速中超时,请检查，排除异常请点是，退出运行请点否");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11124, "气缸阻挡和气缸下限位同时接通,请检查，排除异常请点是，退出运行请点否");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11125, "气缸上和气缸阻挡限位同时接通,请检查，排除异常请点是，退出运行请点否");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11126, "气缸上和气缸下限位同时接通,请检查，排除异常请点是，退出运行请点否");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11127, "气缸上与慢速中限位同时接通,请检查，排除异常请点是，退出运行请点否");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11128, "慢速中限位和气缸阻挡位同时接通,请检查，排除异常请点是，退出运行请点否");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11129, "慢速中限位与气缸下同时接通,请检查，排除异常请点是，退出运行请点否");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11131, "气缸到阻挡位超时,请检查，排除异常请点是，退出运行请点否");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11118, "气缸慢速中2超时,请检查，排除异常请点是，退出运行请点否");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11119, "气缸慢速中3超时,请检查，排除异常请点是，退出运行请点否");
            }
            else
            {
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1, "X axis server alarm");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2, "X-axis enable open timeout");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11, "X-axis is running");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(12, "X-axis enable close timeout");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(13, "X-axis alarm reset failed");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(21, "X-axis communication abnormality");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(22, "The X-axis forward and reverse limit has been activated");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(31, "The X-axis reverse limit has been activated");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(32, "The forward limit of the X-axis has been activated");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(101, "Y axis server alarm");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(102, "Y-axis enable open timeout");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(111, "Y-axis is running");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(112, "Y-axis enable close timeout");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(113, "Y-axis alarm reset failed");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(121, "Y-axis communication abnormality");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(122, "The Y-axis forward and reverse limit has been activated");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(131, "The Y-axis reverse limit has been activated");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(132, "The forward limit of the Y-axis has been activated");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(201, "Z axis server alarm");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(202, "Z-axis enable open timeout");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(211, "Z-axis is running");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(212, "Z-axis enable close timeout");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(213, "Z-axis alarm reset failed");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(221, "Z-axis communication abnormality");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(222, "The Z-axis forward and reverse limit has been activated");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(231, "The Z-axis reverse limit has been activated");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(232, "The forward limit of the Z-axis has been activated");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(241, "Z-axis target position overtravel");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(242, "Z-axis calculation pulse over range");


                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(301, "Overtime on cylinder");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(302, "The cylinder is already connected below");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(311, "There is no signal in the cylinder at the single output point of the cylinder");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(312, "Cylinder center timeout");


                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(321, "The upper position of the cylinder has been connected");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(322, "Cylinder down in place timeout");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(401, "Opening gripper A timed out");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(402, "Opening gripper B timed out");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(411, "Closing gripper A timed out");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(412, "Closing gripper B timed out");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(421, "The signal for gripper A to be in position has been activated");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(422, "The signal for gripper B to be in position has been activated");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(423, "Gripper A has exceeded the pressure relief time limit");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(424, "Gripper B has exceeded the pressure relief time limit");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(425, "The gripper has exceeded the pressure relief time limit");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(501, "The extension of the liquid tray timeout");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(502, "The limit position of the liquid receiving tray has been opened");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(511, "The retraction of the liquid tray timeout");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(512, "The limit of the liquid receiving tray has been opened");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(601, "Time out on the up pressure relief cylinder");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(602, "The under pressure relief cylinder is already connected");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(611, "Overtime under pressure relief cylinder");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(612, "The up pressure relief cylinder is already connected");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2001, "Illegal cup number");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2002, "Illegal Area");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2101, "No syringe found");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2102, "The gripper failed to close when taking out the syringe");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2103, "The cylinder failed to descend when removing the syringe");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2401, "syringe found");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2701, "Discovering cup lids or syringes");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2702, "No cup lid found");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2703, "Failed to remove cap from dispensing cup");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2704, "Failed to remove the cover from the cover placement area");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2705, "Closing failure");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2706, "Failed to place lid into lid area");


                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2707, "Failed to close the cover again");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2708, "Failed to close the cover twice");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2709, "No lid was found after closing the lid again");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2710, "The gripper opens abnormally before moving");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2711, "Abnormalities were found when the front gripper was opened or closed");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2712, "The gripper failed to open when sensing the cup lid");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2713, "The lid of the liquid preparation cup failed to be removed and the gripper failed to close");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2714, "The lid removal in the lid placement area failed and the gripper failed to close");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2715, "The gripper failed to open after the cover was placed");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2716, "The handle failed to open when closing the cover");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2717, "The lid failed to open when it was closed");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2718, "The cylinder failed to descend when closing and supporting the cover");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2719, "The dyeing cup was not locked before opening the lid");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2720, "The dyeing cup was not locked before the lid was closed");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(2730, "When closing the lid, the dye cup was not locked");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(3301, "Abnormal communication on the balance,Click Yes after checking the recovery");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(3302, "The balance was turned on but the waste liquid tank was not taken away,Please take away the waste liquid bucket first, wait for the balance to be cleared and re-place the waste liquid bucket, and then click OK");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(3303, "Balance exceeds the lower limit,Click Yes after checking the recovery");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(3304, "The balance exceeds the upper limit,Click Yes after checking the recovery");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(3305, "Please empty the waste liquid tank first，And then click OK");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(3306, "Please remove the waste liquid bucket first and wait for the balance to automatically return to zero before placing the waste liquid bucket");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1101, "Blocking back limit has been activated");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1102, "Blocking extension timeout");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1111, "Blocking out the limit has been activated");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1112, "Block retrieval timeout");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1113, "Cylinder in blocking position");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(4501, "No gripper found");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(4502, "The gripper failed to close when taking the fabric fixture");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(4503, "The cylinder failed to drop when taking the fabric fixture");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(4504, "The powder weighing motor failed to return to the origin");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(5411, "The cylinder failed to drop when placing the bottle");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(5412, "Failed to place the bottle");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(5413, "Failed to retrieve the bottle at the bottle washing station");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(5414, "Failed to retrieve the bottle at the turntable station");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(5415, "Failed to place the bottle at the bottle washing station");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(5416, "Failed to place the bottle at the turntable station");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(5421, "The time limit for adding followers");


                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1120, "The cylinder block position is connected simultaneously with the cylinder bottom");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1121, "There is no cylinder slow signal at the single output point of the cylinder");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1122, "The cylinder blocking limit has been activated");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1123, "Cylinder slow speed timeout");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1124, "Simultaneously connect the cylinder block and cylinder lower limit position");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1125, "Simultaneously connect the cylinder and the cylinder blocking limit");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1126, "Simultaneously connect the upper and lower limit positions of the cylinder");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1127, "Simultaneously connect the cylinder to the slow speed limit position");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1128, "Simultaneously connect the slow speed limit and cylinder block positions");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1129, "Slow speed middle limit and cylinder lower already connected");


                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1131, "Cylinder timeout to block position");



                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1118, "Cylinder slow two speed timeout");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(1119, "Cylinder slow three speed timeout");

                //大于10000就是询问
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10301, "Overtime on cylinder,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10302, "The cylinder is already connected below,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10312, "Cylinder center timeout,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10321, "The upper position of the cylinder has been connected,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10322, "Cylinder down in place timeout,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10401, "Opening gripper A timed out,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10402, "Opening gripper B timed out,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10421, "The signal for gripper A to be in position has been activated,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10422, "The signal for gripper B to be in position has been activated,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10423, "Gripper A has exceeded the pressure relief time limit,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10424, "Gripper B has exceeded the pressure relief time limit,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10425, "The gripper has exceeded the pressure relief time limit,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10411, "Closing gripper A timed out,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10412, "Closing gripper B timed out,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10501, "The extension of the liquid tray timeout,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10502, "The limit position of the liquid receiving tray has been opened,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10511, "The retraction of the liquid tray timeout,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10512, "The limit of the liquid receiving tray has been opened,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10601, "Time out on the up pressure relief cylinder,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10602, "The under pressure relief cylinder is already connected,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10611, "Overtime under pressure relief cylinder,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(10612, "The up pressure relief cylinder is already connected,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(12401, "syringe found,Click OK and open the gripper 10 seconds later");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(12701, "Discovering cup lids or syringes");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(13301, "Abnormal communication on the balance,Click Yes after checking the recovery");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(13302, "The balance was turned on but the waste liquid tank was not taken away,Please take away the waste liquid bucket first, wait for the balance to be cleared and re-place the waste liquid bucket, and then click OK");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(13303, "Balance exceeds the lower limit,Click Yes after checking the recovery");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(13304, "The balance exceeds the upper limit,Click Yes after checking the recovery");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(13305, "Please empty the waste liquid tank first，And then click OK");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(13306, "Please remove the waste liquid bucket first and wait for the balance to automatically return to zero before placing the waste liquid bucket");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11101, "Blocking back limit has been activated,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11102, "Blocking extension timeout,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11111, "Blocking out the limit has been activated,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11112, "Block retrieval timeout,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");



                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11120, "The cylinder block position is connected simultaneously with the cylinder bottom,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11122, "The cylinder blocking limit has been activated,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11123, "Cylinder slow speed timeout,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11124, "Simultaneously connect the cylinder block and cylinder lower limit position,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11125, "Simultaneously connect the cylinder and the cylinder blocking limit,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11126, "Simultaneously connect the upper and lower limit positions of the cylinder,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11127, "Simultaneously connect the cylinder to the slow speed limit position,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11128, "Simultaneously connect the slow speed limit and cylinder block positions,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11129, "Slow speed middle limit and cylinder lower already connected,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11131, "Cylinder timeout to block position,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");

                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11118, "Cylinder slow two speed timeout,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");
                SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Add(11119, "Cylinder slow three speed timeout,Please check and rule out any abnormalities. Click Yes. Click No to exit the operation");

            }

        }

        private void Login_Load(object sender, EventArgs e)
        {
            LoadPar();
            
            //Text();
        }

        //private void Text() {
        //    int[] _ia_array = { 0, 0 };
        //    int state = FADM_Object.Communal._tcpModBus.Read(901, 2, ref _ia_array);
        //    if (state != -1)
        //    {
        //        double b = 0.0;
        //        if (_ia_array[0]<0) {
        //            b = ((_ia_array[1]+1) * 65536 + _ia_array[0]) / 1000.0;
        //        }
        //        else
        //        {
        //            b = (_ia_array[1] * 65536 + _ia_array[0]) / 1000.0;
        //        }
        //        FADM_Object.Communal._s_balanceValue = Convert.ToString(b);
        //        Console.WriteLine(b);
        //    }
        //}



        /// <summary>
        /// 配置参数
        /// </summary>
        private void LoadPar()
        {

            FADM_Object.Communal._softReg = new SmartDyeing.FADM_Object.MyRegister();
            FADM_Object.Communal._s_machineCode = FADM_Object.Communal._softReg.GetMNum();

            Abort ab = new Abort();
            string version = ab.getVersion();
            ab = null;
            FADM_Object.Communal._s_version = version; //获取版本号

            //配置数据库
            try
            {
                /*SqlConnection SqlCon = new SqlConnection(scsb.ToString());
                // 打开数据库
                SqlCon.Open();
                string ppp = "2";
                string find = "SELECT * FROM dbo.bottle_details WHERE BottleNum = '" + ppp + "'";*/

                // 2、创建用于执行SQL查询语句的对象
                // SqlCommand cmd = new SqlCommand(find, SqlCon);//创建执行语句// 参数1：SQL语句字符串。参数2：已经打开的数据库

                // 3、执行对象的SQL查询语句并接受结果
                /*  SqlDataReader dr = cmd.ExecuteReader();//执行

                  // 4、读取结果的数据
                  while (dr.Read())
                  {
                      Console.WriteLine("ID: " + dr["AssistantCode"].ToString());
                  }
  */


                string s_path = Environment.CurrentDirectory + "\\Config\\DataBase.ini";
                Lib_DataBank.SQLServer.SQLServerCon con = new Lib_DataBank.SQLServer.SQLServerCon()
                {
                    Server = Lib_File.Ini.GetIni("FADM", "Server", s_path),
                    Port = Lib_File.Ini.GetIni("FADM", "Port", s_path),
                    Database = Lib_File.Ini.GetIni("FADM", "Database", s_path),
                    UserName = Lib_File.Ini.GetIni("FADM", "UserName", s_path),
                    Password = Lib_File.Ini.GetIni("FADM", "Password", s_path)
                };

                FADM_Object.Communal._fadmSqlserver = new Lib_DataBank.SQLServer(con);
                FADM_Object.Communal._fadmSqlserver.Open();
                FADM_Object.Communal._fadmSqlserver.Close();

                //Lib_DataBank.AAndB aAndB = new Lib_DataBank.AAndB();
                //aAndB.MB = "3";
                //aAndB.XS = "2";
                //aAndB.SJ =Convert.ToDateTime( "2024-12-16 10:15:09.000");
                //FADM_Object.Communal._fadmSqlserver.Update(aAndB);

                //SqlConnectionStringBuilder scsb = new SqlConnectionStringBuilder();
                //scsb.DataSource = "MS-BLRIIRQRCRHU\\SQLEXPRESS2";    // 设置数据源服务器
                //scsb.UserID = "sa";     // 设置用户名
                //scsb.Password = "myadmin"; // 密码
                //scsb.InitialCatalog = "dorp_system"; // 设置要访问的数据库
                //FADM_Object.Communal._fadmSqlserver = new Lib_DataBank.SQLServer(scsb.ToString());
                //FADM_Object.Communal._fadmSqlserver.Open();
                //FADM_Object.Communal._fadmSqlserver.Close();

                string s_path2 = Environment.CurrentDirectory + "\\Config\\Config.ini";
                string url = Lib_File.Ini.GetIni("info", "url", "0", s_path2);
                FADM_Object.Communal.URL = url;

                string s_newSet = Lib_File.Ini.GetIni("Setting", "IsNewSet", "1", s_path2);
                if (s_newSet == "0")
                {
                    FADM_Object.Communal._b_isNewSet = false;
                }

                string s_JustShowInfo = Lib_File.Ini.GetIni("Setting", "IsJustShowInfo", "0", s_path2);
                if (s_JustShowInfo == "1")
                {
                    FADM_Object.Communal._b_isJustShowInfo = true;
                }
                FADM_Object.Communal._fadmSqlserver.SetShowInfo(FADM_Object.Communal._b_isJustShowInfo);


                ////开启线程 校验表和表字段
                //Thread P_thd = new Thread(new ParameterizedThreadStart(verifyTableSuccess));
                //P_thd.IsBackground = false;
                //P_thd.Start(con);

            }
            catch (Exception ex)
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    FADM_Form.CustomMessageBox.Show(ex.Message, "测试数据库", MessageBoxButtons.OK, false);
                else
                    FADM_Form.CustomMessageBox.Show(ex.Message, "Test database", MessageBoxButtons.OK, false);
                System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id).Kill();
            }
            if (!FADM_Object.Communal._b_isJustShowInfo)
            {
                //板卡版
                if (Lib_Card.Configure.Parameter.Machine_Type == 0)
                {

                    //获取ADT8940A1_IO
                    try
                    {
                        Lib_Card.ADT8940A1.ADT8940A1_IO adt8940a1 = new Lib_Card.ADT8940A1.ADT8940A1_IO();
                        string sPath = Environment.CurrentDirectory + "\\Config\\ADT8940A1_IO.ini";

                        foreach (PropertyInfo info in adt8940a1.GetType().GetProperties())
                        {
                            char[] separator = { '_' };
                            string head = info.Name.Split(separator)[0];
                            try
                            {
                                int value = Convert.ToInt32(Lib_File.Ini.GetIni(head, info.Name, sPath));
                                if (0 > value)
                                {
                                    FADM_Form.CustomMessageBox.Show(info.Name + " = " + value, "ADT8940A1_IO文件异常",
                                       MessageBoxButtons.OK, false);
                                    System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id).Kill();
                                }
                                adt8940a1.GetType().GetProperty(info.Name).SetValue(adt8940a1, value);
                            }
                            catch (Exception ex)
                            {
                                if (ex.Message != "Input string was not in a correct format." && ex.Message != "输入字符串的格式不正确。")
                                    throw;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        FADM_Form.CustomMessageBox.Show(ex.Message, "ADT8940A1_IO", MessageBoxButtons.OK, false);
                        System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id).Kill();
                    }

                    //板卡配置
                    try
                    {
                        Lib_Card.CardObject.OA1 = new Lib_Card.ADT8940A1.ADT8940A1_Card();
                        Lib_Card.CardObject.OA1.CardInit();

                        if (Lib_Card.Configure.Parameter.Machine_BlenderVersion == 0)
                        {
                            Lib_Card.ADT8940A1.OutPut.Blender.Blender blender = new Lib_Card.ADT8940A1.OutPut.Blender.Blender_Basic();
                            if (-1 == blender.Blender_Off())
                                throw new Exception("驱动异常");
                        }
                    }
                    catch (Exception ex)
                    {

                        FADM_Form.CustomMessageBox.Show(ex.Message, "测试板卡", MessageBoxButtons.OK, false);
                        System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id).Kill();
                    }

                    int x = 0;
                    int y = 0;
                    if (FADM_Object.Communal._b_isNewSet)
                        MyModbusFun.CalTarget(0, Lib_Card.Configure.Parameter.Machine_Bottle_Total, ref x, ref y);
                    else
                        MyModbusFun.CalTarget(0, 1, ref x, ref y);
                    FADM_Object.Communal._i_Max_Y = y + 10000;

                    if (Lib_Card.Configure.Parameter.Machine_BalanceType == 0)
                    {
                        //天平配置
                        try
                        {
                            FADM_Object.Communal.Mettler = new Lib_SerialPort.Balance.METTLER
                            {
                                PortName = "COM4",
                                BaudRate = Lib_SerialPort.BaudRates.BR_9600,
                                DataBits = Lib_SerialPort.DataBits.Eight,
                                StopBits = System.IO.Ports.StopBits.One,
                                Parity = System.IO.Ports.Parity.None
                            };

                            FADM_Object.Communal.Mettler.Open();
                        }
                        catch (Exception ex)
                        {
                            FADM_Form.CustomMessageBox.Show(ex.Message, "测试天平", MessageBoxButtons.OK, false);

                            System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id).Kill();
                        }
                    }
                    else
                    {
                        //天平配置
                        try
                        {
                            FADM_Object.Communal.Shinko = new Lib_SerialPort.Balance.SHINKO
                            {
                                PortName = "COM4",
                                BaudRate = Lib_SerialPort.BaudRates.BR_1200,
                                DataBits = Lib_SerialPort.DataBits.Eight,
                                StopBits = System.IO.Ports.StopBits.One,
                                Parity = System.IO.Ports.Parity.None
                            };

                            FADM_Object.Communal.Shinko.Open();
                        }
                        catch (Exception ex)
                        {
                            FADM_Form.CustomMessageBox.Show(ex.Message, "测试天平", MessageBoxButtons.OK, false);

                            System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id).Kill();
                        }
                    }
                }


                //PLC版
                else
                {

                    try
                    {

                        string s_path = Environment.CurrentDirectory + "\\Config\\Config.ini";
                        string s_server = Lib_File.Ini.GetIni("PLC", "IP", s_path);
                        string s_port = Lib_File.Ini.GetIni("PLC", "Port", s_path);

                        string s_isUseBrewOnly = Lib_File.Ini.GetIni("Setting", "IsUseBrewOnly", "0", s_path);
                        if (s_isUseBrewOnly == "1")
                        {
                            FADM_Object.Communal._b_isUseBrewOnly = true;
                        }

                        //连接plc
                        FADM_Object.Communal._tcpModBus = new TCPModBus();
                        FADM_Object.Communal._tcpModBus._i_port = Convert.ToInt32(s_port);
                        FADM_Object.Communal._tcpModBus._s_IP = s_server;
                        if (FADM_Object.Communal._tcpModBus.Connect() == -1)
                        {
                            if (!FADM_Object.Communal._b_isUseBrewOnly)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    FADM_Form.CustomMessageBox.Show("连接PLC失败,请检查!", "设备", MessageBoxButtons.OK, false);
                                else
                                    FADM_Form.CustomMessageBox.Show("Connection to PLC failed, please check!", "Equipment", MessageBoxButtons.OK, false);
                                System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id).Kill();
                            }
                        }

                        //吸光度配置
                        if (Lib_Card.Configure.Parameter.Other_UseAbs == 1)
                        {
                            FADM_Object.Communal._tcpModBusAbs = FADM_Object.Communal._tcpModBus;

                        }
                    }
                    catch
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("连接PLC失败,请检查!", "设备", MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("Connection to PLC failed, please check!", "Equipment", MessageBoxButtons.OK, false);
                        System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id).Kill();
                    }
                    if (!FADM_Object.Communal._b_isUseBrewOnly)
                    {
                        //清除动作错误编号
                        MyModbusFun.ClearError();
                    }


                    //获取参数 写给plc寄存器 d 1000开始
                    int i_home_X_LSpeed = Lib_Card.Configure.Parameter.Home_X_LSpeed;//回零起始速度X轴
                    int i_d0 = 0;
                    int i_d0_0 = 0;
                    this.ComParment(i_home_X_LSpeed, ref i_d0, ref i_d0_0);

                    int i_home_Y_LSpeed = Lib_Card.Configure.Parameter.Home_Y_LSpeed;//回零起始速度Y轴
                    int i_d2 = 0;
                    int i_d2_2 = 0;
                    this.ComParment(i_home_Y_LSpeed, ref i_d2, ref i_d2_2);

                    int i_home_Z_LSpeed = Lib_Card.Configure.Parameter.Home_Z_LSpeed;//回零起始速度Z轴
                    int i_d4 = 0;
                    int i_d4_4 = 0;
                    this.ComParment(i_home_Z_LSpeed, ref i_d4, ref i_d4_4);

                    int i_home_X_HSpeed = Lib_Card.Configure.Parameter.Home_X_HSpeed;//回零驱动速度X轴
                    int i_d6 = 0;
                    int i_d6_6 = 0;
                    this.ComParment(i_home_X_HSpeed, ref i_d6, ref i_d6_6);

                    int i_home_Y_HSpeed = Lib_Card.Configure.Parameter.Home_Y_HSpeed;//回零驱动速度Y轴
                    int i_d8 = 0;
                    int i_d8_8 = 0;
                    this.ComParment(i_home_Y_HSpeed, ref i_d8, ref i_d8_8);

                    int i_home_Z_HSpeed = Lib_Card.Configure.Parameter.Home_Z_HSpeed;//回零驱动速度Z轴
                    int i_d10 = 0;
                    int i_d10_10 = 0;
                    this.ComParment(i_home_Z_HSpeed, ref i_d10, ref i_d10_10);

                    int i_home_X_USpeed = Lib_Card.Configure.Parameter.Home_X_USpeed;//回零加速度X轴
                    int i_d12 = 0;
                    int i_d12_12 = 0;
                    this.ComParment(i_home_X_USpeed, ref i_d12, ref i_d12_12);

                    int i_home_Y_USpeed = Lib_Card.Configure.Parameter.Home_Y_USpeed;//回零加速度Y轴
                    int i_d14 = 0;
                    int i_d14_14 = 0;
                    this.ComParment(i_home_Y_USpeed, ref i_d14, ref i_d14_14);

                    int i_home_Z_USpeed = Lib_Card.Configure.Parameter.Home_Z_USpeed;//回零加速度Z轴
                    int i_d16 = 0;
                    int i_d16_16 = 0;
                    this.ComParment(i_home_Z_USpeed, ref i_d16, ref i_d16_16);


                    int i_home_X_CSpeed = Lib_Card.Configure.Parameter.Home_X_CSpeed;//爬行速度X轴
                    int i_d18 = 0;
                    int i_d18_18 = 0;
                    this.ComParment(i_home_X_CSpeed, ref i_d18, ref i_d18_18);

                    int i_home_Y_CSpeed = Lib_Card.Configure.Parameter.Home_Y_CSpeed;//爬行速度Y轴
                    int i_d20 = 0;
                    int i_d20_20 = 0;
                    this.ComParment(i_home_Y_CSpeed, ref i_d20, ref i_d20_20);

                    int i_home_Z_CSpeed = Lib_Card.Configure.Parameter.Home_Z_CSpeed;//爬行速度Z轴
                    int i_d22 = 0;
                    int i_d22_22 = 0;
                    this.ComParment(i_home_Z_CSpeed, ref i_d22, ref i_d22_22);


                    int i_home_X_Offset = Lib_Card.Configure.Parameter.Home_X_Offset;//偏移量X轴
                    int i_d24 = 0;
                    int i_d24_24 = 0;
                    this.ComParment(i_home_X_Offset, ref i_d24, ref i_d24_24);

                    int i_home_Y_Offset = Lib_Card.Configure.Parameter.Home_Y_Offset;//偏移量Y轴
                    int i_d26 = 0;
                    int i_d26_26 = 0;
                    this.ComParment(i_home_Y_Offset, ref i_d26, ref i_d26_26);

                    int i_home_Z_Offset = Lib_Card.Configure.Parameter.Home_Z_Offset;//偏移量Z轴
                    int i_d28 = 0;
                    int i_d28_28 = 0;
                    this.ComParment(i_home_Z_Offset, ref i_d28, ref i_d28_28);


                    //运动参数设置
                    int i_move_X_LSpeed = Lib_Card.Configure.Parameter.Move_X_LSpeed;//X轴起始速度
                    int i_d30 = 0;
                    int i_d30_30 = 0;
                    this.ComParment(i_move_X_LSpeed, ref i_d30, ref i_d30_30);

                    int i_move_X_HSpeed = Lib_Card.Configure.Parameter.Move_X_HSpeed;//X轴驱动速度
                    int i_d32 = 0;
                    int i_d32_32 = 0;
                    this.ComParment(i_move_X_HSpeed, ref i_d32, ref i_d32_32);

                    double i_move_X_UTime = Lib_Card.Configure.Parameter.Move_X_UTime;//X轴加减速时间
                    int i_d34 = 0;
                    int i_d34_34 = 0;
                    this.ComParment(Convert.ToInt32(i_move_X_UTime), ref i_d34, ref i_d34_34);


                    int i_move_Y_LSpeed = Lib_Card.Configure.Parameter.Move_Y_LSpeed;//Y轴起始速度
                    int i_d36 = 0;
                    int i_d36_36 = 0;
                    this.ComParment(i_move_Y_LSpeed, ref i_d36, ref i_d36_36);

                    int i_move_Y_HSpeed = Lib_Card.Configure.Parameter.Move_Y_HSpeed;//Y轴驱动速度
                    int i_d38 = 0;
                    int i_d38_38 = 0;
                    this.ComParment(i_move_Y_HSpeed, ref i_d38, ref i_d38_38);

                    double i_move_Y_UTime = Lib_Card.Configure.Parameter.Move_Y_UTime;//Y轴加减速时间
                    int i_d40 = 0;
                    int i_d40_40 = 0;
                    this.ComParment(Convert.ToInt32(i_move_Y_UTime), ref i_d40, ref i_d40_40);


                    int i_move_S_LSpeed = Lib_Card.Configure.Parameter.Move_S_LSpeed;//小针筒起始速度
                    int i_d42 = 0;
                    int i_d42_42 = 0;
                    this.ComParment(i_move_S_LSpeed, ref i_d42, ref i_d42_42);

                    int i_move_S_HSpeed = Lib_Card.Configure.Parameter.Move_S_HSpeed;//小针筒驱动速度
                    int i_d44 = 0;
                    int i_d44_44 = 0;
                    this.ComParment(i_move_S_HSpeed, ref i_d44, ref i_d44_44);

                    double i_move_S_UTime = Lib_Card.Configure.Parameter.Move_S_UTime;//小针筒加减速时间
                    int i_d46 = 0;
                    int i_d46_46 = 0;
                    this.ComParment(Convert.ToInt32(i_move_S_UTime), ref i_d46, ref i_d46_46);


                    int i_move_B_LSpeed = Lib_Card.Configure.Parameter.Move_B_LSpeed;//大针筒起始速度
                    int i_d48 = 0;
                    int i_d48_48 = 0;
                    this.ComParment(i_move_B_LSpeed, ref i_d48, ref i_d48_48);

                    int i_move_B_HSpeed = Lib_Card.Configure.Parameter.Move_B_HSpeed;//大针筒驱动速度
                    int i_d50 = 0;
                    int i_d50_50 = 0;
                    this.ComParment(i_move_B_HSpeed, ref i_d50, ref i_d50_50);

                    double i_move_B_UTime = Lib_Card.Configure.Parameter.Move_B_UTime;//大针筒加减速时间
                    int i_d52 = 0;
                    int i_d52_52 = 0;
                    this.ComParment(Convert.ToInt32(i_move_B_UTime), ref i_d52, ref i_d52_52);


                    double i_delay_Cylinder = Lib_Card.Configure.Parameter.Delay_Cylinder;//气缸检测延时(秒)
                    int i_d54 = 0;
                    int i_d54_54 = 0;
                    this.ComParment(Convert.ToInt32(i_delay_Cylinder * 1000), ref i_d54, ref i_d54_54);
                    ;
                    double i_delay_Tongs = Lib_Card.Configure.Parameter.Delay_Tongs;//抓手检测延时(秒)
                    int i_d56 = 0;
                    int i_d56_56 = 0;
                    this.ComParment(Convert.ToInt32(i_delay_Tongs * 1000), ref i_d56, ref i_d56_56);

                    double i_delay_Syringe = Lib_Card.Configure.Parameter.Delay_Syringe;//针检检测延时(秒)
                    int i_d58 = 0;
                    int i_d58_58 = 0;
                    this.ComParment(Convert.ToInt32(i_delay_Syringe * 1000), ref i_d58, ref i_d58_58);

                    double i_delay_Tray = Lib_Card.Configure.Parameter.Delay_Tray;//接液盘检测延时(秒)
                    int i_d60 = 0;
                    int i_d60_60 = 0;
                    this.ComParment(Convert.ToInt32(i_delay_Tray * 1000), ref i_d60, ref i_d60_60);

                    double i_delay_Balance_Reset = Lib_Card.Configure.Parameter.Delay_Balance_Reset;//天平清零延时(秒)
                    int i_d62 = 0;
                    int i_d62_62 = 0;
                    this.ComParment(Convert.ToInt32(i_delay_Balance_Reset * 1000), ref i_d62, ref i_d62_62);

                    double i_delay_Balance_Read = Lib_Card.Configure.Parameter.Delay_Balance_Read;//天平读数延时(秒)
                    int i_d64 = 0;
                    int i_d64_64 = 0;
                    this.ComParment(Convert.ToInt32(i_delay_Balance_Read * 1000), ref i_d64, ref i_d64_64);

                    double i_delay_Buzzer_Finish = Lib_Card.Configure.Parameter.Delay_Buzzer_Finish;//完成报警延时(秒)
                    int i_d66 = 0;
                    int i_d66_66 = 0;
                    this.ComParment(Convert.ToInt32(i_delay_Buzzer_Finish * 1000), ref i_d66, ref i_d66_66);

                    int i_other_Z_UpPulse = Lib_Card.Configure.Parameter.Other_Z_UpPulse;//Z轴排空时上移脉冲
                    int i_d68 = 0;
                    int i_d68_68 = 0;
                    this.ComParment(i_other_Z_UpPulse, ref i_d68, ref i_d68_68);

                    int i_other_Z_DownPulse = Lib_Card.Configure.Parameter.Other_Z_DownPulse;//Z轴排空时下压脉冲
                    int i_d70 = 0;
                    int i_d70_70 = 0;
                    this.ComParment(i_other_Z_DownPulse, ref i_d70, ref i_d70_70);

                    int i_other_Z_BackPulse = Lib_Card.Configure.Parameter.Other_Z_BackPulse;//Z轴抽液完反推脉冲
                    int i_d72 = 0;
                    int i_d72_72 = 0;
                    this.ComParment(i_other_Z_BackPulse, ref i_d72, ref i_d72_72);

                    int i_cylinderVersion = Lib_Card.Configure.Parameter.Machine_CylinderVersion;//气缸单双中继
                    int i_d74 = 0;
                    int i_d74_74 = 0;
                    this.ComParment(i_cylinderVersion, ref i_d74, ref i_d74_74);

                    int i_isSyringe = Lib_Card.Configure.Parameter.Machine_isSyringe;//屏蔽针筒感应器
                    int i_d76 = 0;
                    int i_d76_76 = 0;
                    this.ComParment(i_isSyringe, ref i_d76, ref i_d76_76);

                    int i_other_Push = Lib_Card.Configure.Parameter.Other_Push;//抽液时是否先升气缸 0：边升边反推  1：反推完再升气缸
                    int i_d78 = 0;
                    int i_d78_78 = 0;
                    this.ComParment(i_other_Push, ref i_d78, ref i_d78_78);

                    int i_machine_BalanceType = Lib_Card.Configure.Parameter.Machine_BalanceType;//天平类型
                    int i_d80 = 0;
                    int i_d80_80 = 0;
                    this.ComParment(i_machine_BalanceType, ref i_d80, ref i_d80_80);

                    int i_coordinate_Balance_X = Convert.ToInt32(Lib_Card.Configure.Parameter.Coordinate_Balance_X);//天平X坐标
                    int i_d82 = 0;
                    int i_d82_82 = 0;
                    this.ComParment(i_coordinate_Balance_X, ref i_d82, ref i_d82_82);

                    int x = 0;
                    int y = 0;
                    if (FADM_Object.Communal._b_isNewSet)
                        MyModbusFun.CalTarget(0, Lib_Card.Configure.Parameter.Machine_Bottle_Total, ref x, ref y);
                    else
                        MyModbusFun.CalTarget(0, 1, ref x, ref y);
                    FADM_Object.Communal._i_Max_Y = y + 10000;

                    int i_coordinate_Balance_Y = Convert.ToInt32(Lib_Card.Configure.Parameter.Coordinate_Balance_Y);//天平Y坐标
                    int i_d84 = 0;
                    int i_d84_84 = 0;
                    this.ComParment(i_coordinate_Balance_Y, ref i_d84, ref i_d84_84);



                    int i_machine_Decompression = Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_Decompression);//泄压数量
                    int i_d86 = 0;
                    int i_d86_86 = 0;
                    this.ComParment(i_machine_Decompression, ref i_d86, ref i_d86_86);

                    int i_delay_Decompression = Convert.ToInt32(Lib_Card.Configure.Parameter.Delay_Decompression);//泄压延时
                    int i_d88 = 0;
                    int i_d88_88 = 0;
                    this.ComParment(i_delay_Decompression * 1000, ref i_d88, ref i_d88_88);

                    int i_machine_TongsVersion = Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_TongsVersion);//抓手单双中继
                    int i_d90 = 0;
                    int i_d90_90 = 0;
                    this.ComParment(i_machine_TongsVersion, ref i_d90, ref i_d90_90);

                    int i_machine_BlenderVersion = Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_BlenderVersion);//搅拌配置常开点
                    int i_d92 = 0;
                    int i_d92_92 = 0;
                    this.ComParment(i_machine_BlenderVersion, ref i_d92, ref i_d92_92);

                    int i_coordinate_Standby_X = Convert.ToInt32(Lib_Card.Configure.Parameter.Coordinate_Standby_X);//待机位X坐标
                    int i_d94 = 0;
                    int i_d94_94 = 0;
                    this.ComParment(i_coordinate_Standby_X, ref i_d94, ref i_d94_94);

                    int i_coordinate_Standby_Y = Convert.ToInt32(Lib_Card.Configure.Parameter.Coordinate_Standby_Y);//待机位Y坐标
                    int i_d96 = 0;
                    int i_d96_96 = 0;
                    this.ComParment(i_coordinate_Standby_Y, ref i_d96, ref i_d96_96);

                    int i_machine_MidCylinder = Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_MidCylinder);//是否气缸中
                    int i_d98 = 0;
                    int i_d98_98 = 0;
                    this.ComParment(i_machine_MidCylinder, ref i_d98, ref i_d98_98);

                    int i_machine_BlockCylinder = Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_BlockCylinder);//阻挡气缸配置
                    int i_d100 = 0;
                    int i_d100_100 = 0;
                    this.ComParment(i_machine_BlockCylinder, ref i_d100, ref i_d100_100);

                    double i_delay_Block = Lib_Card.Configure.Parameter.Delay_Block;//阻挡气缸检测延时(秒)
                    int i_d102 = 0;
                    int i_d102_102 = 0;
                    this.ComParment(Convert.ToInt32(i_delay_Block * 1000), ref i_d102, ref i_d102_102);

                    int i_machine_ZType = Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_ZType);//Z轴电机配置
                    int i_d104 = 0;
                    int i_d104_104 = 0;
                    this.ComParment(i_machine_ZType, ref i_d104, ref i_d104_104);

                    int i_other_S_MaxPulse = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_S_MaxPulse);//小针筒Z轴最大脉冲
                    int i_d106 = 0;
                    int i_d106_106 = 0;
                    this.ComParment(i_other_S_MaxPulse + 5000, ref i_d106, ref i_d106_106);

                    int i_other_B_MaxPulse = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_B_MaxPulse);//大针筒Z轴最大脉冲
                    int i_d108 = 0;
                    int i_d108_108 = 0;
                    this.ComParment(i_other_B_MaxPulse + 5000, ref i_d108, ref i_d108_108);

                    int i_machine_CloseCoverType = Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_CloseCoverType);//放盖失能打开
                    int i_d110 = 0;
                    int i_d110_110 = 0;
                    this.ComParment(i_machine_CloseCoverType, ref i_d110, ref i_d110_110);

                    int i_other_BalanceMaxWeight = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_BalanceMaxWeight);//废液桶最大液量
                    int i_d112 = 0;
                    int i_d112_112 = 0;
                    this.ComParment(i_other_BalanceMaxWeight, ref i_d112, ref i_d112_112);

                    int i_machine_UseBack = Convert.ToInt32(Lib_Card.Configure.Parameter.Machine_UseBack);//是否使用后光幕
                    int i_d114 = 0;
                    int i_d114_114 = 0;
                    this.ComParment(i_machine_UseBack, ref i_d114, ref i_d114_114);

                    int i_d116 = 0;
                    int i_d116_116 = 0;

                    int i_d118 = 0;
                    int i_d118_118 = 0;
                    if (i_machine_UseBack == 1)
                    {
                        if (FADM_Object.Communal._b_isNewSet)
                            this.ComParment(Convert.ToInt32(Lib_Card.Configure.Parameter.Coordinate_Bottle_Y) + 5 * Convert.ToInt32(Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval), ref i_d118, ref i_d118_118);
                        else
                        {
                            MyModbusFun.CalTarget(0, Lib_Card.Configure.Parameter.Machine_Bottle_Total, ref x, ref y);
                            this.ComParment(y + 5 * Convert.ToInt32(Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval), ref i_d118, ref i_d118_118);
                        }
                    }

                    int i_move_S_MinHSpeed = Convert.ToInt32(Lib_Card.Configure.Parameter.Move_S_MinHSpeed);//小针筒运行慢速驱动速度
                    int i_d120 = 0;
                    int i_d120_120 = 0;
                    this.ComParment(i_move_S_MinHSpeed, ref i_d120, ref i_d120_120);

                    int i_move_B_MinHSpeed = Convert.ToInt32(Lib_Card.Configure.Parameter.Move_B_MinHSpeed);//大针筒运行慢速驱动速度
                    int i_d122 = 0;
                    int i_d122_122 = 0;
                    this.ComParment(i_move_B_MinHSpeed, ref i_d122, ref i_d122_122);

                    int i_other_ClosePulse = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_ClosePulse);//合夹夹布脉冲
                    int i_d148 = 0;
                    int i_d148_148 = 0;
                    this.ComParment(i_other_ClosePulse, ref i_d148, ref i_d148_148);

                    int i_other_OpenPulse = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_OpenPulse);//开夹放布脉冲
                    int i_d150 = 0;
                    int i_d150_150 = 0;
                    this.ComParment(i_other_OpenPulse, ref i_d150, ref i_d150_150);

                    int i_other_ClothDownPulse = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_ClothDownPulse);//备布区下探开夹脉冲
                    int i_d152 = 0;
                    int i_d152_152 = 0;
                    this.ComParment(i_other_ClothDownPulse, ref i_d152, ref i_d152_152);

                    int i_other_CupDownPulse = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_CupDownPulse);//染杯口下探开夹脉冲
                    int i_d154 = 0;
                    int i_d154_154 = 0;
                    this.ComParment(i_other_CupDownPulse, ref i_d154, ref i_d154_154);

                    int i_other_WaterMagnification = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_WaterMagnification);//加水放大倍数 0放大1000倍 1是放大10倍
                    int i_d156 = 0;
                    int i_d156_156 = 0;
                    this.ComParment(i_other_WaterMagnification, ref i_d156, ref i_d156_156);

                    int i_other_OutClothPosition = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_OutClothPosition);//出布时气缸位置 0阻挡位 1慢速中
                    int i_d158 = 0;
                    int i_d158_158 = 0;
                    this.ComParment(i_other_OutClothPosition, ref i_d158, ref i_d158_158);

                    int i_other_WashTime = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_WashTime);//洗针次数
                    int i_d160 = 0;
                    int i_d160_160 = 0;
                    this.ComParment(i_other_WashTime, ref i_d160, ref i_d160_160);

                    int i_other_Tongs_Decompression = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_Tongs_Decompression);//是否配置抓手撑盖
                    int i_d162 = 0;
                    int i_d162_162 = 0;
                    this.ComParment(i_other_Tongs_Decompression, ref i_d162, ref i_d162_162);

                    int i_other_SupportCoverY = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_SupportCoverY);//撑盖Y轴位置偏移
                    int i_d164 = 0;
                    int i_d164_164 = 0;
                    this.ComParment(i_other_SupportCoverY, ref i_d164, ref i_d164_164);

                    int i_other_ValveTerminal = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_ValveTerminal);//是否启用阀岛
                    int i_d166 = 0;
                    int i_d166_166 = 0;
                    this.ComParment(i_other_ValveTerminal, ref i_d166, ref i_d166_166);

                    int i_other_CoverTray = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_CoverTray);//开盖是否要伸出接液盘
                    int i_d168 = 0;
                    int i_d168_168 = 0;
                    this.ComParment(i_other_CoverTray, ref i_d168, ref i_d168_168);

                    int i_other_RepeatGetCloth = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_RepeatGetCloth);//开盖是否要伸出接液盘
                    int i_d170 = 0;
                    int i_d170_170 = 0;
                    this.ComParment(i_other_RepeatGetCloth, ref i_d170, ref i_d170_170);

                    int i_other_UseAbs = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_UseAbs);//是否有吸光度
                    int i_d176 = 0;
                    int i_d176_176 = 0;
                    this.ComParment(i_other_UseAbs, ref i_d176, ref i_d176_176);

                    int i_other_OutClothZero = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_OutClothZero);//50g布夹放布后回零点
                    int i_d178 = 0;
                    int i_d178_178 = 0;
                    this.ComParment(i_other_OutClothZero, ref i_d178, ref i_d178_178);

                    int i_other_ValveTerminalOpen = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_ValveTerminalOpen);//慢速中气缸常开常闭切换
                    int i_d180 = 0;
                    int i_d180_180 = 0;
                    this.ComParment(i_other_ValveTerminalOpen, ref i_d180, ref i_d180_180);

                    int i_other_ClothUp = (int)(Convert.ToDouble(Lib_Card.Configure.Parameter.Other_ClothUp) * 100);//布重上限
                    int i_d182 = 0;
                    int i_d182_182 = 0;
                    this.ComParment(i_other_ClothUp, ref i_d182, ref i_d182_182);

                    int i_other_ClothDown = (int)(Convert.ToDouble(Lib_Card.Configure.Parameter.Other_ClothDown) * 100);//布重下限
                    int i_d184 = 0;
                    int i_d184_184 = 0;
                    this.ComParment(i_other_ClothDown, ref i_d184, ref i_d184_184);

                    int i_other_ClothPulseUp = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_ClothPulseUp);//合夹夹布脉冲上限
                    int i_d186 = 0;
                    int i_d186_186 = 0;
                    this.ComParment(i_other_ClothPulseUp, ref i_d186, ref i_d186_186);

                    int i_other_ClothPulseDown = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_ClothPulseDown);//合夹夹布脉冲下限
                    int i_d188 = 0;
                    int i_d188_188 = 0;
                    this.ComParment(i_other_ClothPulseDown, ref i_d188, ref i_d188_188);

                    int i_other_GetDryCloth = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_GetDryCloth);//干布区取布位置
                    int i_d190 = 0;
                    int i_d190_190 = 0;
                    this.ComParment(i_other_GetDryCloth, ref i_d190, ref i_d190_190);

                    int i_other_TurntableXStart = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_TurntableXStart);//转盘的X轴起始位置
                    int i_d192 = 0;
                    int i_d192_192 = 0;
                    this.ComParment(i_other_TurntableXStart, ref i_d192, ref i_d192_192);

                    int i_other_TurntableXEnd = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_TurntableXEnd);//转盘的X轴终点位置
                    int i_d194 = 0;
                    int i_d194_194 = 0;
                    this.ComParment(i_other_TurntableXEnd, ref i_d194, ref i_d194_194);

                    int i_other_TurntableYStart = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_TurntableYStart);//转盘的Y轴起始位置
                    int i_d196 = 0;
                    int i_d196_196 = 0;
                    this.ComParment(i_other_TurntableYStart, ref i_d196, ref i_d196_196);

                    int i_other_TurntableYEnd = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_TurntableYEnd);//转盘的Y轴终点位置
                    int i_d198 = 0;
                    int i_d198_198 = 0;
                    this.ComParment(i_other_TurntableYEnd, ref i_d198, ref i_d198_198);

                    int i_other_TightBottlePluse = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_TightBottlePluse);//夹紧母液瓶脉冲
                    int i_d200 = 0;
                    int i_d200_200 = 0;
                    this.ComParment(i_other_TightBottlePluse, ref i_d200, ref i_d200_200);

                    int i_other_LooseBottlePluse = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_LooseBottlePluse);//松开母液瓶脉冲
                    int i_d202 = 0;
                    int i_d202_202 = 0;
                    this.ComParment(i_other_LooseBottlePluse, ref i_d202, ref i_d202_202);

                    int i_other_PowerOverTime = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_PowerOverTime);//加粉超时时间
                    int i_d204 = 0;
                    int i_d204_204 = 0;
                    this.ComParment(i_other_PowerOverTime, ref i_d204, ref i_d204_204);

                    int i_other_PowerPluse = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_PowerPluse);//粉罐上抽脉冲
                    int i_d206 = 0;
                    int i_d206_206 = 0;
                    this.ComParment(i_other_PowerPluse, ref i_d206, ref i_d206_206);

                    if (Convert.ToInt32(Lib_Card.Configure.Parameter.Correcting_B_Pulse) < 0)
                    {
                        FADM_Form.CustomMessageBox.Show("大针筒校正脉冲错误!", "设备", MessageBoxButtons.OK, false);
                        System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id).Kill();
                    }

                    if (Convert.ToInt32(Lib_Card.Configure.Parameter.Correcting_S_Pulse) < 0)
                    {
                        FADM_Form.CustomMessageBox.Show("小针筒校正脉冲错误!", "设备", MessageBoxButtons.OK, false);
                        System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id).Kill();
                    }

                    if (Convert.ToInt32(Lib_Card.Configure.Parameter.Other_B_MaxPulse) < 0)
                    {
                        FADM_Form.CustomMessageBox.Show("大针筒Z轴最大脉冲错误!", "设备", MessageBoxButtons.OK, false);
                        System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id).Kill();
                    }

                    if (Convert.ToInt32(Lib_Card.Configure.Parameter.Other_S_MaxPulse) < 0)
                    {
                        FADM_Form.CustomMessageBox.Show("小针筒Z轴最大脉冲错误!", "设备", MessageBoxButtons.OK, false);
                        System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id).Kill();
                    }

                    if (Convert.ToInt32(Lib_Card.Configure.Parameter.Other_Z_UpPulse) < 0)
                    {
                        FADM_Form.CustomMessageBox.Show("排空Z轴上移脉冲错误!", "设备", MessageBoxButtons.OK, false);
                        System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id).Kill();
                    }

                    if (Convert.ToInt32(Lib_Card.Configure.Parameter.Other_Z_DownPulse) > 0)
                    {
                        FADM_Form.CustomMessageBox.Show("排空Z轴下压脉冲错误!", "设备", MessageBoxButtons.OK, false);
                        System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id).Kill();
                    }

                    if (Convert.ToInt32(Lib_Card.Configure.Parameter.Other_Z_BackPulse) > 0)
                    {
                        FADM_Form.CustomMessageBox.Show("抽液完成反推脉冲错误!", "设备", MessageBoxButtons.OK, false);
                        System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id).Kill();
                    }

                    if (Convert.ToInt32(Lib_Card.Configure.Parameter.Home_Z_Offset) < 0)
                    {
                        FADM_Form.CustomMessageBox.Show("Z轴回零偏移量错误!", "设备", MessageBoxButtons.OK, false);
                        System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id).Kill();
                    }


                    int[] ia_array = {i_d0,i_d0_0,i_d2,i_d2_2,i_d4,i_d4_4,i_d6,i_d6_6,i_d8,i_d8_8,i_d10,i_d10_10,i_d12,i_d12_12,
                i_d14,i_d14_14,i_d16,i_d16_16,i_d18,i_d18_18,i_d20,i_d20_20,i_d22,i_d22_22,i_d24,i_d24_24,i_d26,i_d26_26,
                i_d28,i_d28_28,i_d30,i_d30_30,i_d32,i_d32_32,i_d34,i_d34_34,i_d36,i_d36_36,i_d38,i_d38_38,i_d40,i_d40_40,
                i_d42,i_d42_42,i_d44,i_d44_44,i_d46,i_d46_46,i_d48,i_d48_48,i_d50,i_d50_50,i_d52,i_d52_52,i_d54,i_d54_54,
                i_d56,i_d56_56,i_d58,i_d58_58,i_d60,i_d60_60,i_d62,i_d62_62,i_d64,i_d64_64,i_d66,i_d66_66,i_d68,i_d68_68,
                i_d70,i_d70_70,i_d72,i_d72_72,i_d74,i_d74_74,i_d76,i_d76_76,i_d78,i_d78_78,i_d80,i_d80_80,i_d82,i_d82_82,
                i_d84,i_d84_84,i_d86,i_d86_86,i_d88,i_d88_88,i_d90,i_d90_90,i_d92,i_d92_92,i_d94,i_d94_94,i_d96,i_d96_96,
                i_d98,i_d98_98,i_d100,i_d100_100,i_d102,i_d102_102,i_d104,i_d104_104,i_d106,i_d106_106,i_d108,i_d108_108,
                i_d110,i_d110_110,i_d112,i_d112_112,i_d114,i_d114_114,i_d116,i_d116_116,i_d118,i_d118_118};
                    if (!FADM_Object.Communal._b_isUseBrewOnly)
                    {
                        int i_c = FADM_Object.Communal._tcpModBus.Write(1000, ia_array);
                        if (i_c == -1)
                        {
                            FADM_Form.CustomMessageBox.Show("写入参数失败!", "设备", MessageBoxButtons.OK, false);
                            System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id).Kill();
                        }

                        int[] ia_array1 = { i_d120, i_d120_120, i_d122, i_d122_122 };

                        int i_c1 = FADM_Object.Communal._tcpModBus.Write(1120, ia_array1);
                        if (i_c1 == -1)
                        {
                            FADM_Form.CustomMessageBox.Show("写入参数失败!", "设备", MessageBoxButtons.OK, false);
                            System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id).Kill();
                        }

                        int[] ia_array2 = { i_d148, i_d148_148, i_d150, i_d150_150, i_d152, i_d152_152, i_d154, i_d154_154, i_d156, i_d156_156, i_d158
                                , i_d158_158, i_d160, i_d160_160, i_d162, i_d162_162,i_d164,i_d164_164,i_d166,i_d166_166,i_d168,i_d168_168,i_d170,i_d170_170,
                            0,0,0,0,i_d176,i_d176_176,i_d178,i_d178_178,i_d180,i_d180_180,i_d182,i_d182_182,i_d184,i_d184_184,i_d186,i_d186_186,i_d188,i_d188_188
                        ,i_d190,i_d190_190,i_d192,i_d192_192,i_d194,i_d194_194,i_d196,i_d196_196,i_d198,i_d198_198,i_d200,i_d200_200,i_d202,i_d202_202,i_d204,i_d204_204,i_d206,i_d206_206};

                        int i_c2 = FADM_Object.Communal._tcpModBus.Write(1148, ia_array2);
                        if (i_c2 == -1)
                        {
                            FADM_Form.CustomMessageBox.Show("写入参数失败!", "设备", MessageBoxButtons.OK, false);
                            System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id).Kill();
                        }
                    }

                    //获取IO_Mapping
                    try
                    {
                        IOMapping IO = new IOMapping();
                        string s_path = Environment.CurrentDirectory + "\\Config\\IO_Mapping.ini";

                        foreach (PropertyInfo info in IO.GetType().GetProperties())
                        {
                            char[] ca_separator = { '_' };
                            string s_head = info.Name.Split(ca_separator)[0];
                            try
                            {
                                int i_value = Convert.ToInt32(Lib_File.Ini.GetIni(s_head, info.Name, s_path));
                                //if (0 > value)
                                //{
                                //    FADM_Form.CustomMessageBox.Show(info.Name + " = " + value, "IO_Mapping文件异常",
                                //       MessageBoxButtons.OK, false);
                                //    System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id).Kill();
                                //}
                                IO.GetType().GetProperty(info.Name).SetValue(IO, i_value);
                            }
                            catch (Exception ex)
                            {
                                if (ex.Message != "Input string was not in a correct format." && ex.Message != "输入字符串的格式不正确。")
                                    throw;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show(ex.Message, "IO_Mapping", MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show(ex.Message, "IO_Mapping", MessageBoxButtons.OK, false);
                        System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id).Kill();
                    }

                    //输入点
                    int i_inPut_X_APulse = Convert.ToInt32(IOMapping.InPut_X_APulse);//X轴A相反馈脉冲
                    int i_i0 = 0;
                    int i_i0_0 = 0;
                    this.ComParment(i_inPut_X_APulse, ref i_i0, ref i_i0_0);

                    int i_inPut_X_BPulse = Convert.ToInt32(IOMapping.InPut_X_BPulse);//X轴B相反馈脉冲
                    int i_i2 = 0;
                    int i_i2_2 = 0;
                    this.ComParment(i_inPut_X_BPulse, ref i_i2, ref i_i2_2);

                    int i_inPut_Y_APulse = Convert.ToInt32(IOMapping.InPut_Y_APulse);//Y轴A相反馈脉冲
                    int i_i4 = 0;
                    int i_i4_4 = 0;
                    this.ComParment(i_inPut_Y_APulse, ref i_i4, ref i_i4_4);

                    int i_inPut_Y_BPulse = Convert.ToInt32(IOMapping.InPut_Y_BPulse);//Y轴B相反馈脉冲
                    int i_i6 = 0;
                    int i_i6_6 = 0;
                    this.ComParment(i_inPut_Y_BPulse, ref i_i6, ref i_i6_6);

                    int i_inPut_Z_APulse = Convert.ToInt32(IOMapping.InPut_Z_APulse);//Z轴A相反馈脉冲
                    int i_i8 = 0;
                    int i_i8_8 = 0;
                    this.ComParment(i_inPut_Z_APulse, ref i_i8, ref i_i8_8);

                    int i_inPut_Z_BPulse = Convert.ToInt32(IOMapping.InPut_Y_BPulse);//Z轴B相反馈脉冲
                    int i_i10 = 0;
                    int i_i10_10 = 0;
                    this.ComParment(i_inPut_Z_BPulse, ref i_i10, ref i_i10_10);

                    int i_inPut_X_Alarm = Convert.ToInt32(IOMapping.InPut_X_Alarm);//X轴异常
                    int i_i12 = 0;
                    int i_i12_12 = 0;
                    this.ComParment(i_inPut_X_Alarm, ref i_i12, ref i_i12_12);

                    int i_inPut_Y_Alarm = Convert.ToInt32(IOMapping.InPut_Y_Alarm);//Y轴异常
                    int i_i14 = 0;
                    int i_i14_14 = 0;
                    this.ComParment(i_inPut_Y_Alarm, ref i_i14, ref i_i14_14);

                    int i_inPut_Z_Alarm = Convert.ToInt32(IOMapping.InPut_Z_Alarm);//Z轴异常
                    int i_i16 = 0;
                    int i_i16_16 = 0;
                    this.ComParment(i_inPut_Z_Alarm, ref i_i16, ref i_i16_16);

                    int i_inPut_X_Ready = Convert.ToInt32(IOMapping.InPut_X_Ready);//X轴准备好
                    int i_i18 = 0;
                    int i_i18_18 = 0;
                    this.ComParment(i_inPut_X_Ready, ref i_i18, ref i_i18_18);

                    int i_inPut_Y_Ready = Convert.ToInt32(IOMapping.InPut_Y_Ready);//Y轴准备好
                    int i_i20 = 0;
                    int i_i20_20 = 0;
                    this.ComParment(i_inPut_Y_Ready, ref i_i20, ref i_i20_20);

                    int i_inPut_Z_Ready = Convert.ToInt32(IOMapping.InPut_Z_Ready);//Z轴准备好
                    int i22 = 0;
                    int i22_22 = 0;
                    this.ComParment(i_inPut_Z_Ready, ref i22, ref i22_22);

                    int i_inPut_X_Corotation = Convert.ToInt32(IOMapping.InPut_X_Corotation);//X轴正限位
                    int i_i24 = 0;
                    int i_i24_24 = 0;
                    this.ComParment(i_inPut_X_Corotation, ref i_i24, ref i_i24_24);

                    int i_inPut_X_Reverse = Convert.ToInt32(IOMapping.InPut_X_Reverse);//X轴正限位
                    int i_i26 = 0;
                    int i_i26_26 = 0;
                    this.ComParment(i_inPut_X_Reverse, ref i_i26, ref i_i26_26);

                    int i_inPut_X_Origin = Convert.ToInt32(IOMapping.InPut_X_Origin);//X轴原点
                    int i_i28 = 0;
                    int i_i28_28 = 0;
                    this.ComParment(i_inPut_X_Origin, ref i_i28, ref i_i28_28);

                    int i_inPut_Y_Corotation = Convert.ToInt32(IOMapping.InPut_Y_Corotation);//Y轴正限位
                    int i_i30 = 0;
                    int i_i30_30 = 0;
                    this.ComParment(i_inPut_Y_Corotation, ref i_i30, ref i_i30_30);

                    int i_inPut_Y_Reverse = Convert.ToInt32(IOMapping.InPut_Y_Reverse);//Y轴正限位
                    int i_i32 = 0;
                    int i_i32_32 = 0;
                    this.ComParment(i_inPut_Y_Reverse, ref i_i32, ref i_i32_32);

                    int i_inPut_Y_Origin = Convert.ToInt32(IOMapping.InPut_X_Origin);//Y轴原点
                    int i_i34 = 0;
                    int i_i34_34 = 0;
                    this.ComParment(i_inPut_Y_Origin, ref i_i34, ref i_i34_34);

                    int i_inPut_Z_Corotation = Convert.ToInt32(IOMapping.InPut_Z_Corotation);//Z轴正限位
                    int i_i36 = 0;
                    int i_i36_36 = 0;
                    this.ComParment(i_inPut_Z_Corotation, ref i_i36, ref i_i36_36);

                    int i_inPut_Z_Reverse = Convert.ToInt32(IOMapping.InPut_Z_Reverse);//Z轴反限位
                    int i_i38 = 0;
                    int i_i38_38 = 0;
                    this.ComParment(i_inPut_Z_Reverse, ref i_i38, ref i_i38_38);

                    int i_inPut_Z_Origin = Convert.ToInt32(IOMapping.InPut_Z_Origin);//Z轴原点
                    int i_i40 = 0;
                    int i_i40_40 = 0;
                    this.ComParment(i_inPut_Z_Origin, ref i_i40, ref i_i40_40);

                    int i_inPut_Stop = Convert.ToInt32(IOMapping.InPut_Stop);//前光幕
                    int i_i42 = 0;
                    int i_i42_42 = 0;
                    this.ComParment(i_inPut_Stop, ref i_i42, ref i_i42_42);

                    int i_inPut_Sunx_A = Convert.ToInt32(IOMapping.InPut_Sunx_A);//左光幕
                    int i_i44 = 0;
                    int i_i44_44 = 0;
                    this.ComParment(i_inPut_Sunx_A, ref i_i44, ref i_i44_44);

                    int i_inPut_Sunx_B = Convert.ToInt32(IOMapping.InPut_Sunx_B);//右光幕
                    int i_i46 = 0;
                    int i_i46_46 = 0;
                    this.ComParment(i_inPut_Sunx_B, ref i_i46, ref i_i46_46);

                    int i_inPut_Syringe = Convert.ToInt32(IOMapping.InPut_Syringe);//针筒
                    int i_i48 = 0;
                    int i_i48_48 = 0;
                    this.ComParment(i_inPut_Syringe, ref i_i48, ref i_i48_48);

                    int i_inPut_Tongs_A = Convert.ToInt32(IOMapping.InPut_Tongs_A);//抓手A
                    int i_i50 = 0;
                    int i_i50_50 = 0;
                    this.ComParment(i_inPut_Tongs_A, ref i_i50, ref i_i50_50);

                    int i_inPut_Tongs_B = Convert.ToInt32(IOMapping.InPut_Tongs_B);//抓手B
                    int i_i52 = 0;
                    int i_i52_52 = 0;
                    this.ComParment(i_inPut_Tongs_B, ref i_i52, ref i_i52_52);

                    int i_inPut_Cylinder_Up = Convert.ToInt32(IOMapping.InPut_Cylinder_Up);//上限位
                    int i_i54 = 0;
                    int i_i54_54 = 0;
                    this.ComParment(i_inPut_Cylinder_Up, ref i_i54, ref i_i54_54);

                    int i_inPut_Cylinder_Mid = Convert.ToInt32(IOMapping.InPut_Cylinder_Mid);//中限位
                    int i_i56 = 0;
                    int i_i56_56 = 0;
                    this.ComParment(i_inPut_Cylinder_Mid, ref i_i56, ref i_i56_56);

                    int i_inPut_Cylinder_Down = Convert.ToInt32(IOMapping.InPut_Cylinder_Down);//下限位
                    int i_i58 = 0;
                    int i_i58_58 = 0;
                    this.ComParment(i_inPut_Cylinder_Down, ref i_i58, ref i_i58_58);

                    int i_inPut_Tray_Out = Convert.ToInt32(IOMapping.InPut_Tray_Out);//接液盘出
                    int i_i60 = 0;
                    int i_i60_60 = 0;
                    this.ComParment(i_inPut_Tray_Out, ref i_i60, ref i_i60_60);

                    int i_inPut_Tray_In = Convert.ToInt32(IOMapping.InPut_Tray_In);//接液盘回
                    int i_i62 = 0;
                    int i_i62_62 = 0;
                    this.ComParment(i_inPut_Tray_In, ref i_i62, ref i_i62_62);

                    int i_inPut_Decompression_Up = Convert.ToInt32(IOMapping.InPut_Decompression_Up);//泄压上限位
                    int i_i64 = 0;
                    int i_i64_64 = 0;
                    this.ComParment(i_inPut_Decompression_Up, ref i_i64, ref i_i64_64);

                    int i_inPut_Decompression_Down = Convert.ToInt32(IOMapping.InPut_Decompression_Down);//泄压下限位
                    int i_i66 = 0;
                    int i_i66_66 = 0;
                    this.ComParment(i_inPut_Decompression_Down, ref i_i66, ref i_i66_66);

                    int i_inPut_Block_Out = Convert.ToInt32(IOMapping.InPut_Block_Out);//阻挡出限位
                    int i_i68 = 0;
                    int i_i68_68 = 0;
                    this.ComParment(i_inPut_Block_Out, ref i_i68, ref i_i68_68);

                    int i_inPut_Block_In = Convert.ToInt32(IOMapping.InPut_Block_In);//阻挡回限位
                    int i_i70 = 0;
                    int i_i70_70 = 0;
                    this.ComParment(i_inPut_Block_In, ref i_i70, ref i_i70_70);

                    int i_inPut_Slow_Cylinder_Mid = Convert.ToInt32(IOMapping.InPut_Slow_Cylinder_Mid);//气缸慢速中限位
                    int i_i72 = 0;
                    int i_i72_72 = 0;
                    this.ComParment(i_inPut_Slow_Cylinder_Mid, ref i_i72, ref i_i72_72);

                    int i_inPut_Cylinder_Block = Convert.ToInt32(IOMapping.InPut_Cylinder_Block);//气缸阻挡限位
                    int i_i74 = 0;
                    int i_i74_74 = 0;
                    this.ComParment(i_inPut_Cylinder_Block, ref i_i74, ref i_i74_74);

                   

                    int i_inPut_Back = Convert.ToInt32(IOMapping.InPut_Back);//后光幕
                    int i_i76 = 0;
                    int i_i76_76 = 0;
                    this.ComParment(i_inPut_Back, ref i_i76, ref i_i76_76);

                    int i_InPut_SupportCover = Convert.ToInt32(IOMapping.InPut_SupportCover);//撑盖开到位
                    int i_i78 = 0;
                    int i_i78_78 = 0;
                    this.ComParment(i_InPut_SupportCover, ref i_i78, ref i_i78_78);

                    int i_inPut_Slow_Cylinder_Mid_2 = Convert.ToInt32(IOMapping.InPut_Slow_Cylinder_Mid_2);//气缸慢速中2限位
                    int i_i80 = 0;
                    int i_i80_80 = 0;
                    this.ComParment(i_inPut_Slow_Cylinder_Mid_2, ref i_i80, ref i_i80_80);

                    int i_inPut_Slow_Cylinder_Mid_3 = Convert.ToInt32(IOMapping.InPut_Slow_Cylinder_Mid_3);//气缸慢速中3限位
                    int i_i82 = 0;
                    int i_i82_82 = 0;
                    this.ComParment(i_inPut_Slow_Cylinder_Mid_3, ref i_i82, ref i_i82_82);

                    int i_inPut_Dye_Stop = Convert.ToInt32(IOMapping.InPut_Dye_Stop);//转子缸急停
                    int i_i84 = 0;
                    int i_i84_84 = 0;
                    this.ComParment(i_inPut_Dye_Stop, ref i_i84, ref i_i84_84);




                    //int i_inPut_Tongs_B_Decompression = Convert.ToInt32(IOMapping.InPut_Tongs_B_Decompression);//抓手B泄压
                    //int i_i80 = 0;
                    //int i_i80_80 = 0;
                    //this.ComParment(i_inPut_Tongs_B_Decompression, ref i_i80, ref i_i80_80);

                    int[] ia_iarray = {i_i0,i_i0_0,i_i2,i_i2_2,i_i4,i_i4_4,i_i6,i_i6_6,i_i8,i_i8_8,i_i10,i_i10_10,i_i12,i_i12_12,
                i_i14,i_i14_14,i_i16,i_i16_16,i_i18,i_i18_18,i_i20,i_i20_20,i22,i22_22,i_i24,i_i24_24,i_i26,i_i26_26,
                i_i28,i_i28_28,i_i30,i_i30_30,i_i32,i_i32_32,i_i34,i_i34_34,i_i36,i_i36_36,i_i38,i_i38_38,i_i40,i_i40_40,
                i_i42,i_i42_42,i_i44,i_i44_44,i_i46,i_i46_46,i_i48,i_i48_48,i_i50,i_i50_50,i_i52,i_i52_52,i_i54,i_i54_54,
                i_i56,i_i56_56,i_i58,i_i58_58,i_i60,i_i60_60,i_i62,i_i62_62,i_i64,i_i64_64,i_i66,i_i66_66,i_i68,i_i68_68,
                        i_i70,i_i70_70,i_i72,i_i72_72,i_i74,i_i74_74,i_i76,i_i76_76,i_i78,i_i78_78,i_i80,i_i80_80,i_i82,i_i82_82,i_i84,i_i84_84};
                    if (!FADM_Object.Communal._b_isUseBrewOnly)
                    {
                        int i_ic = FADM_Object.Communal._tcpModBus.Write(3500, ia_iarray);
                        if (i_ic == -1)
                        {
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                FADM_Form.CustomMessageBox.Show("写入参数失败!", "设备", MessageBoxButtons.OK, false);
                            else
                                FADM_Form.CustomMessageBox.Show("Failed to write parameters!", "Equipment", MessageBoxButtons.OK, false);

                            System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id).Kill();
                        }
                    }

                    //输出点
                    int i_outPut_X_Pulse = Convert.ToInt32(IOMapping.OutPut_X_Pulse);//X轴脉冲
                    int i_o0 = 0;
                    int i_o0_0 = 0;
                    this.ComParment(i_outPut_X_Pulse, ref i_o0, ref i_o0_0);

                    int i_outPut_X_Direction = Convert.ToInt32(IOMapping.OutPut_X_Direction);//X轴方向
                    int i_o2 = 0;
                    int i_o2_2 = 0;
                    this.ComParment(i_outPut_X_Direction, ref i_o2, ref i_o2_2);

                    int i_outPut_Y_Pulse = Convert.ToInt32(IOMapping.OutPut_Y_Pulse);//Y轴脉冲
                    int i_o4 = 0;
                    int i_o4_4 = 0;
                    this.ComParment(i_outPut_Y_Pulse, ref i_o4, ref i_o4_4);

                    int i_outPut_Y_Direction = Convert.ToInt32(IOMapping.OutPut_Y_Direction);//Y轴方向
                    int i_o6 = 0;
                    int i_o6_6 = 0;
                    this.ComParment(i_outPut_Y_Direction, ref i_o6, ref i_o6_6);

                    int i_outPut_Z_Pulse = Convert.ToInt32(IOMapping.OutPut_Z_Pulse);//Z轴脉冲
                    int i_o8 = 0;
                    int i_o8_8 = 0;
                    this.ComParment(i_outPut_Z_Pulse, ref i_o8, ref i_o8_8);

                    int i_outPut_Z_Direction = Convert.ToInt32(IOMapping.OutPut_Z_Direction);//Z轴方向
                    int i_o10 = 0;
                    int i_o10_10 = 0;
                    this.ComParment(i_outPut_Z_Direction, ref i_o10, ref i_o10_10);

                    int i_outPut_X_Power = Convert.ToInt32(IOMapping.OutPut_X_Power);//X轴使能
                    int i_o12 = 0;
                    int i_o12_12 = 0;
                    this.ComParment(i_outPut_X_Power, ref i_o12, ref i_o12_12);

                    int i_outPut_Y_Power = Convert.ToInt32(IOMapping.OutPut_X_Power);//Y轴使能
                    int i_o14 = 0;
                    int i_o14_14 = 0;
                    this.ComParment(i_outPut_Y_Power, ref i_o14, ref i_o14_14);

                    int i_outPut_Z_Power = Convert.ToInt32(IOMapping.OutPut_Z_Power);//Z轴使能
                    int i_o16 = 0;
                    int i_o16_16 = 0;
                    this.ComParment(i_outPut_Z_Power, ref i_o16, ref i_o16_16);

                    int i_outPut_X_Reset = Convert.ToInt32(IOMapping.OutPut_X_Reset);//X轴复位
                    int i_o18 = 0;
                    int i_o18_18 = 0;
                    this.ComParment(i_outPut_X_Reset, ref i_o18, ref i_o18_18);

                    int i_outPut_Y_Reset = Convert.ToInt32(IOMapping.OutPut_Y_Reset);//Y轴复位
                    int i_o20 = 0;
                    int i_o20_20 = 0;
                    this.ComParment(i_outPut_Y_Reset, ref i_o20, ref i_o20_20);

                    int i_outPut_Z_Reset = Convert.ToInt32(IOMapping.OutPut_Z_Reset);//Z轴复位
                    int i_o22 = 0;
                    int i_o22_22 = 0;
                    this.ComParment(i_outPut_Z_Reset, ref i_o22, ref i_o22_22);

                    int i_outPut_Blender = Convert.ToInt32(IOMapping.OutPut_Blender);//搅拌停
                    int i_o24 = 0;
                    int i_o24_24 = 0;
                    this.ComParment(i_outPut_Blender, ref i_o24, ref i_o24_24);

                    int i_outPut_Buzzer = Convert.ToInt32(IOMapping.OutPut_Buzzer);//蜂鸣器(报警)
                    int i_o26 = 0;
                    int i_o26_26 = 0;
                    this.ComParment(i_outPut_Buzzer, ref i_o26, ref i_o26_26);

                    int i_outPut_TongsOff = Convert.ToInt32(IOMapping.OutPut_TongsOff);//抓手合
                    int i_o28 = 0;
                    int i_o28_28 = 0;
                    this.ComParment(i_outPut_TongsOff, ref i_o28, ref i_o28_28);

                    int i_outPut_TongsOn = Convert.ToInt32(IOMapping.OutPut_TongsOn);//抓手开
                    int i_o30 = 0;
                    int i_o30_30 = 0;
                    this.ComParment(i_outPut_TongsOn, ref i_o30, ref i_o30_30);

                    int i_outPut_Cylinder_Up = Convert.ToInt32(IOMapping.OutPut_Cylinder_Up);//气缸上
                    int i_o32 = 0;
                    int i_o32_32 = 0;
                    this.ComParment(i_outPut_Cylinder_Up, ref i_o32, ref i_o32_32);

                    int i_outPut_Cylinder_Down = Convert.ToInt32(IOMapping.OutPut_Cylinder_Down);//气缸下
                    int i_o34 = 0;
                    int i_o34_34 = 0;
                    this.ComParment(i_outPut_Cylinder_Down, ref i_o34, ref i_o34_34);

                    int i_outPut_Tray = Convert.ToInt32(IOMapping.OutPut_Tray);//接液盘
                    int i_o36 = 0;
                    int i_o36_36 = 0;
                    this.ComParment(i_outPut_Tray, ref i_o36, ref i_o36_36);

                    int i_outPut_Waste = Convert.ToInt32(IOMapping.OutPut_Waste);//抽废液
                    int i_o38 = 0;
                    int i_o38_38 = 0;
                    this.ComParment(i_outPut_Waste, ref i_o38, ref i_o38_38);

                    int i_outPut_Water = Convert.ToInt32(IOMapping.OutPut_Water);//加水
                    int i_o40 = 0;
                    int i_o40_40 = 0;
                    this.ComParment(i_outPut_Water, ref i_o40, ref i_o40_40);

                    int i_outPut_Decompression = Convert.ToInt32(IOMapping.OutPut_Decompression);//泄压
                    int i_o42 = 0;
                    int i_o42_42 = 0;
                    this.ComParment(i_outPut_Decompression, ref i_o42, ref i_o42_42);

                    int i_outPut_Red = Convert.ToInt32(IOMapping.OutPut_Red);//红灯
                    int i_o44 = 0;
                    int i_o44_44 = 0;
                    this.ComParment(i_outPut_Red, ref i_o44, ref i_o44_44);

                    int i_outPut_Green = Convert.ToInt32(IOMapping.OutPut_Green);//绿灯
                    int i_o46 = 0;
                    int i_o46_46 = 0;
                    this.ComParment(i_outPut_Green, ref i_o46, ref i_o46_46);

                    int i_outPut_Block_Out = Convert.ToInt32(IOMapping.OutPut_Block_Out);//阻挡出
                    int i_o48 = 0;
                    int i_o48_48 = 0;
                    this.ComParment(i_outPut_Block_Out, ref i_o48, ref i_o48_48);

                    int i_outPut_Block_In = Convert.ToInt32(IOMapping.OutPut_Block_In);//阻挡回
                    int i_o50 = 0;
                    int i_o50_50 = 0;
                    this.ComParment(i_outPut_Block_In, ref i_o50, ref i_o50_50);

                    int i_outPut_Slow_Cylinder = Convert.ToInt32(IOMapping.OutPut_Slow_Cylinder);//气缸慢下阀
                    int i_o52 = 0;
                    int i_o52_52 = 0;
                    this.ComParment(i_outPut_Slow_Cylinder, ref i_o52, ref i_o52_52);

                    int i_outPut_Wash_In = Convert.ToInt32(IOMapping.OutPut_Wash_In);//洗针进水阀
                    int i_o54 = 0;
                    int i_o54_54 = 0;
                    this.ComParment(i_outPut_Wash_In, ref i_o54, ref i_o54_54);

                    int i_outPut_Wash_Out = Convert.ToInt32(IOMapping.OutPut_Wash_Out);//洗针排水阀
                    int i_o56 = 0;
                    int i_o56_56 = 0;
                    this.ComParment(i_outPut_Wash_Out, ref i_o56, ref i_o56_56);

                    int i_outPut_Wash_Blow = Convert.ToInt32(IOMapping.OutPut_Wash_Blow);//洗针吹气阀
                    int i_o58 = 0;
                    int i_o58_58 = 0;
                    this.ComParment(i_outPut_Wash_Blow, ref i_o58, ref i_o58_58);

                    int i_outPut_Tongs_Decompression = Convert.ToInt32(IOMapping.OutPut_Tongs_Decompression);//抓手泄压阀
                    int i_o60 = 0;
                    int i_o60_60 = 0;
                    this.ComParment(i_outPut_Tongs_Decompression, ref i_o60, ref i_o60_60);

                    if (!FADM_Object.Communal._b_isUseBrewOnly)
                    {
                        int[] ia_oarray = {i_o0,i_o0_0,i_o2,i_o2_2,i_o4,i_o4_4,i_o6,i_o6_6,i_o8,i_o8_8,i_o10,i_o10_10,i_o12,i_o12_12,
                i_o14,i_o14_14,i_o16,i_o16_16,i_o18,i_o18_18,i_o20,i_o20_20,i_o22,i_o22_22,i_o24,i_o24_24,i_o26,i_o26_26,
                i_o28,i_o28_28,i_o30,i_o30_30,i_o32,i_o32_32,i_o34,i_o34_34,i_o36,i_o36_36,i_o38,i_o38_38,i_o40,i_o40_40,
                i_o42,i_o42_42,i_o44,i_o44_44,i_o46,i_o46_46,i_o48,i_o48_48,i_o50,i_o50_50,i_o52,i_o52_52,i_o54,i_o54_54,i_o56,i_o56_56,i_o58,i_o58_58,i_o60,i_o60_60};

                        int i_oc = FADM_Object.Communal._tcpModBus.Write(3900, ia_oarray);
                        if (i_oc == -1)
                        {
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                FADM_Form.CustomMessageBox.Show("写入参数失败!", "设备", MessageBoxButtons.OK, false);
                            else
                                FADM_Form.CustomMessageBox.Show("Failed to write parameters!", "Equipment", MessageBoxButtons.OK, false);
                            System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id).Kill();
                        }
                    }
                }


                ////重置选择信息为否，可处理上次没正常退出plc块
                //lab814:
                //    int[] array4 = { 2 };
                //    int state = FADM_Object.Communal._tcpModBus.Write(814, array4);
                //    if (state == -1)
                //        goto lab814;


                for (int i = 0; i < 6; i++)
                {
                    string s_min = "", s_max = "";
                    if (i == 0)
                    {
                        s_min = Lib_Card.Configure.Parameter.Machine_Area1_CupMin.ToString();
                        s_max = Lib_Card.Configure.Parameter.Machine_Area1_CupMax.ToString();
                    }
                    else if (i == 1)
                    {
                        s_min = Lib_Card.Configure.Parameter.Machine_Area2_CupMin.ToString();
                        s_max = Lib_Card.Configure.Parameter.Machine_Area2_CupMax.ToString();
                    }
                    else if (i == 2)
                    {
                        s_min = Lib_Card.Configure.Parameter.Machine_Area3_CupMin.ToString();
                        s_max = Lib_Card.Configure.Parameter.Machine_Area3_CupMax.ToString();
                    }
                    else if (i == 3)
                    {
                        s_min = Lib_Card.Configure.Parameter.Machine_Area4_CupMin.ToString();
                        s_max = Lib_Card.Configure.Parameter.Machine_Area4_CupMax.ToString();
                    }
                    else if (i == 4)
                    {
                        s_min = Lib_Card.Configure.Parameter.Machine_Area5_CupMin.ToString();
                        s_max = Lib_Card.Configure.Parameter.Machine_Area5_CupMax.ToString();
                    }
                    else if (i == 5)
                    {
                        s_min = Lib_Card.Configure.Parameter.Machine_Area6_CupMin.ToString();
                        s_max = Lib_Card.Configure.Parameter.Machine_Area6_CupMax.ToString();
                    }

                    DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(
                   "SELECT * FROM cup_details WHERE CupNum >= " + s_min + " and CupNum <=" + s_max + ";");

                    foreach (DataRow row in dt_data.Rows)
                    {
                        int i_enable = Convert.ToInt16(row["Enable"]);
                        if (1 == i_enable)
                        {
                            FADM_Object.Communal._ia_dyeStatus[i] = 1;
                            break;
                        }
                    }
                }


                //染色机配置
                try
                {
                    //FADM_Object.Communal.DyeHMI = new Lib_SerialPort.HMI.HMI
                    //{
                    //    PortName = "COM3",
                    //    BaudRate = Lib_SerialPort.BaudRates.BR_9600,
                    //    DataBits = Lib_SerialPort.DataBits.Eight,
                    //    StopBits = System.IO.Ports.StopBits.Two,
                    //    Parity = System.IO.Ports.Parity.None
                    //};

                    //FADM_Object.Communal.DyeHMI.Open();
                    string s_path = Environment.CurrentDirectory + "\\Config\\Config.ini";
                    string s_server1 = Lib_File.Ini.GetIni("HMI1", "IP", s_path);
                    string s_port1 = Lib_File.Ini.GetIni("HMI1", "Port", s_path);
                    string s_server2 = Lib_File.Ini.GetIni("HMI2", "IP", s_path);
                    string s_port2 = Lib_File.Ini.GetIni("HMI2", "Port", s_path);
                    string s_server3 = Lib_File.Ini.GetIni("HMI3", "IP", s_path);
                    string s_port3 = Lib_File.Ini.GetIni("HMI3", "Port", s_path);
                    string s_server4 = Lib_File.Ini.GetIni("HMI4", "IP", s_path);
                    string s_port4 = Lib_File.Ini.GetIni("HMI4", "Port", s_path);
                    string s_server5 = Lib_File.Ini.GetIni("HMI5", "IP", s_path);
                    string s_port5 = Lib_File.Ini.GetIni("HMI5", "Port", s_path);
                    string s_server6 = Lib_File.Ini.GetIni("HMI6", "IP", s_path);
                    string s_port6 = Lib_File.Ini.GetIni("HMI6", "Port", s_path);
                    string HMIBaClo_IP = Lib_File.Ini.GetIni("HMIBaClo", "IP", s_path);
                    string HMIBaClo_s_port6 = Lib_File.Ini.GetIni("HMIBaClo", "Port", s_path);
                    string Power_IP = Lib_File.Ini.GetIni("Power", "IP", s_path);
                    string Power_port = Lib_File.Ini.GetIni("Power", "Port", s_path);

                    string s_isUseCloth = Lib_File.Ini.GetIni("Setting", "IsUseCloth", "0", s_path);
                    if (s_isUseCloth == "1")
                    {
                        FADM_Object.Communal._b_isUseCloth = true;
                    }

                    string s_isUsePower = Lib_File.Ini.GetIni("Setting", "IsUsePower", "0", s_path);
                    if (s_isUsePower == "1")
                    {
                        FADM_Object.Communal._b_isUsePower = true;
                    }

                    if (Lib_Card.Configure.Parameter.Machine_Area1_Type == 3)
                    {
                        FADM_Object.Communal._tcpDyeHMI1 = new HMITCPModBus();
                        FADM_Object.Communal._tcpDyeHMI1._i_port = Convert.ToInt32(s_port1);
                        FADM_Object.Communal._tcpDyeHMI1._s_ip = s_server1;
                        FADM_Object.Communal._tcpDyeHMI1.Connect();
                    }
                    if (Lib_Card.Configure.Parameter.Machine_Area2_Type == 3)
                    {
                        FADM_Object.Communal._tcpDyeHMI2 = new HMITCPModBus();
                        FADM_Object.Communal._tcpDyeHMI2._i_port = Convert.ToInt32(s_port2);
                        FADM_Object.Communal._tcpDyeHMI2._s_ip = s_server2;
                        FADM_Object.Communal._tcpDyeHMI2.Connect();
                    }
                    if (Lib_Card.Configure.Parameter.Machine_Area3_Type == 3)
                    {
                        FADM_Object.Communal._tcpDyeHMI3 = new HMITCPModBus();
                        FADM_Object.Communal._tcpDyeHMI3._i_port = Convert.ToInt32(s_port3);
                        FADM_Object.Communal._tcpDyeHMI3._s_ip = s_server3;
                        FADM_Object.Communal._tcpDyeHMI3.Connect();
                    }
                    if (Lib_Card.Configure.Parameter.Machine_Area4_Type == 3)
                    {
                        FADM_Object.Communal._tcpDyeHMI4 = new HMITCPModBus();
                        FADM_Object.Communal._tcpDyeHMI4._i_port = Convert.ToInt32(s_port4);
                        FADM_Object.Communal._tcpDyeHMI4._s_ip = s_server4;
                        FADM_Object.Communal._tcpDyeHMI4.Connect();
                    }
                    if (Lib_Card.Configure.Parameter.Machine_Area5_Type == 3)
                    {
                        FADM_Object.Communal._tcpDyeHMI5 = new HMITCPModBus();
                        FADM_Object.Communal._tcpDyeHMI5._i_port = Convert.ToInt32(s_port5);
                        FADM_Object.Communal._tcpDyeHMI5._s_ip = s_server5;
                        FADM_Object.Communal._tcpDyeHMI5.Connect();
                    }
                    if (Lib_Card.Configure.Parameter.Machine_Area6_Type == 3)
                    {
                        FADM_Object.Communal._tcpDyeHMI6 = new HMITCPModBus();
                        FADM_Object.Communal._tcpDyeHMI6._i_port = Convert.ToInt32(s_port6);
                        FADM_Object.Communal._tcpDyeHMI6._s_ip = s_server6;
                        FADM_Object.Communal._tcpDyeHMI6.Connect();
                    }
                    if (HMIBaClo_IP != "" && HMIBaClo_IP.Length > 0 && HMIBaClo_s_port6 != "" && HMIBaClo_s_port6.Length > 0)
                    {
                        if (Communal._b_isUseCloth)
                        {
                            FADM_Object.Communal.HMIBaClo = new HMITCPModBus();
                            FADM_Object.Communal.HMIBaClo._i_port = Convert.ToInt32(HMIBaClo_s_port6);
                            FADM_Object.Communal.HMIBaClo._s_ip = HMIBaClo_IP;
                            FADM_Object.Communal.HMIBaClo.Connect();
                        }

                    }

                    if (Communal._b_isUsePower)
                    {
                        FADM_Object.Communal.Powder = new HMITCPModBus();
                        FADM_Object.Communal.Powder._i_port = Convert.ToInt32(Power_port);
                        FADM_Object.Communal.Powder._s_ip = Power_IP;
                        FADM_Object.Communal.Powder.Connect();
                    }


                }
                catch (Exception ex)
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show(ex.Message, "测试染色机", MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show(ex.Message, "Test dyeing machine", MessageBoxButtons.OK, false);
                    System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id).Kill();
                }
                if (!FADM_Object.Communal._b_isHaveAutoBrew)
                {
                    //开料机配置
                    try
                    {
                        //使用TCP通讯
                        if (0 == Lib_Card.Configure.Parameter.Machine_Opening_Type || 1 == Lib_Card.Configure.Parameter.Machine_Opening_Type)
                        {
                            string s_path = Environment.CurrentDirectory + "\\Config\\Config.ini";
                            string s_server1 = Lib_File.Ini.GetIni("HMIBrew", "IP", s_path);
                            string s_port1 = Lib_File.Ini.GetIni("HMIBrew", "Port", s_path);

                            FADM_Object.Communal._tcpModBusBrew = new TCPModBus();
                            FADM_Object.Communal._tcpModBusBrew._i_port = Convert.ToInt32(s_port1);
                            FADM_Object.Communal._tcpModBusBrew._s_IP = s_server1;
                            FADM_Object.Communal._tcpModBusBrew.Connect();
                        }
                        //使用串口通讯
                        else
                        {

                            FADM_Object.Communal.MyBrew = new Lib_SerialPort.HMI.BrewHMI
                            {
                                PortName = "COM2",
                                BaudRate = Lib_SerialPort.BaudRates.BR_9600,
                                DataBits = Lib_SerialPort.DataBits.Eight,
                                StopBits = System.IO.Ports.StopBits.Two,
                                Parity = System.IO.Ports.Parity.None
                            };
                            FADM_Object.Communal.MyBrew.Open();
                            byte[] b_send = new byte[6];
                            //威纶
                            if (2 == Lib_Card.Configure.Parameter.Machine_Opening_Type)
                            {
                                b_send[0] = 0x01;
                                b_send[1] = 0x06;
                                b_send[2] = ((10999) / 256);
                                b_send[3] = ((10999) % 256);
                                b_send[4] = Convert.ToByte(Lib_Card.Configure.Parameter.Machine_Bottle_Total / 256);
                                b_send[5] = Convert.ToByte(Lib_Card.Configure.Parameter.Machine_Bottle_Total % 256);
                            }
                            //台达
                            else
                            {
                                b_send[0] = 0x01;
                                b_send[1] = 0x06;
                                b_send[2] = ((3000) / 256);
                                b_send[3] = ((3000) % 256);
                                b_send[4] = Convert.ToByte(Lib_Card.Configure.Parameter.Machine_Bottle_Total / 256);
                                b_send[5] = Convert.ToByte(Lib_Card.Configure.Parameter.Machine_Bottle_Total % 256);
                            }

                            FADM_Object.Communal.MyBrew.WriteAndRead(b_send);

                        }
                    }
                    catch (Exception ex)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show(ex.Message, "测试开料机", MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show(ex.Message, "Testing the cutting machine", MessageBoxButtons.OK, false);
                        System.Diagnostics.Process.GetProcessById(System.Diagnostics.Process.GetCurrentProcess().Id).Kill();
                    }
                }

                


                FADM_Object.Communal._fadmSqlserver.ReviseData(
                "TRUNCATE TABLE SpeechInfo;");

            }

        }

        /// <summary>
        /// d1 
        /// </summary>
        /// <param name="i_d1"></param>
        /// <param name="i_d2"></param>
        public void ComParment(int i_d0, ref int i_d1,ref int i_d2) {
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
        }


        private void BtnLogOn_Click(object sender, EventArgs e)
        {

            //FADM_Object.Communal._fadmSqlserver.ReviseData(
            //            "INSERT INTO abs_wait_list(BottleNum, InsertDate) VALUES('" + 1 + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "');");
            //string s_sql_QQ = "SELECT top 1 * FROM abs_wait_list  order by InsertDate;";
            //DataTable dt_data_QQ = FADM_Object.Communal._fadmSqlserver.GetData(s_sql_QQ);
            //if (dt_data_QQ.Rows.Count > 0)
            //{
            //    string ss =Convert.ToDateTime(dt_data_QQ.Rows[0]["InsertDate"]).ToString("yyyy-MM-dd HH:mm:ss.fff");
            //    string s_sql = "Delete from abs_wait_list where BottleNum = " + 1 + " And InsertDate = '" + dt_data_QQ.Rows[0]["InsertDate"].ToString() + "';";
            //    FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from abs_wait_list where BottleNum = " + 1 + " And InsertDate = '" + Convert.ToDateTime(dt_data_QQ.Rows[0]["InsertDate"]).ToString("yyyy-MM-dd HH:mm:ss.fff") + "';");
            //}

            //DataTable dt_head_11 = FADM_Object.Communal._fadmSqlserver.GetData("select * from cup_details where CupNum = 1");
            //DataTable dt_head_12 = FADM_Object.Communal._fadmSqlserver.GetData("select * from cup_details where CupNum = 2");
            //dt_head_12.Rows.Add(dt_head_11.Rows[0].ItemArray);

            //MessageBox.Show(FADM_Object.Communal._i_Max_Y.ToString());

            string s_str = Convert.ToString(5, 2).PadLeft(15, '0');
            char[] ca_cc = s_str.ToArray();
            //s_str = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //判断App_Data文件夹是否存在
            string s_filePath = Environment.CurrentDirectory + "\\App_Data\\";
            if (!Directory.Exists(s_filePath))
            {
                Directory.CreateDirectory(s_filePath);
            }
            FADM_Object.Communal._s_operator = "123";
            //判断是否存在表
            try
            {
                DataTable dt_head = FADM_Object.Communal._fadmSqlserver.GetData("select COUNT(*) from sysobjects where id = object_id('drop_system.dbo.formula_head_temp')");
                if (dt_head.Rows[0][0].ToString() == "0")
                {
                    FADM_Object.Communal._fadmSqlserver.ReviseData("select * into   formula_head_temp from  formula_head where 1=0");
                }

                dt_head = FADM_Object.Communal._fadmSqlserver.GetData("select COUNT(*) from sysobjects where id = object_id('drop_system.dbo.formula_details_temp')");
                if (dt_head.Rows[0][0].ToString() == "0")
                {
                    FADM_Object.Communal._fadmSqlserver.ReviseData("select * into   formula_details_temp from  formula_details where 1=0");
                }

                dt_head = FADM_Object.Communal._fadmSqlserver.GetData("select COUNT(*) from sysobjects where id = object_id('drop_system.dbo.formula_handle_details_temp')");
                if (dt_head.Rows[0][0].ToString() == "0")
                {
                    FADM_Object.Communal._fadmSqlserver.ReviseData("select * into   formula_handle_details_temp from  formula_handle_details where 1=0");
                }

                dt_head = Communal._fadmSqlserver.GetData("select COUNT(*) from sysobjects where id = object_id('drop_system.dbo.dyeing_Remark')");
                if (dt_head.Rows[0][0].ToString() == "0")
                {
                    Communal._fadmSqlserver.ReviseData("CREATE TABLE [dbo].[dyeing_Remark]([Code] [nvarchar](50) NULL,[Remark] [nvarchar](50) NULL) ON [PRIMARY]");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'dyeing_process' AND COLUMN_NAME = 'Rev'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE dyeing_process ADD Rev int null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'dye_details' AND COLUMN_NAME = 'Compensation'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE dye_details ADD Compensation numeric(8, 2) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'history_dye' AND COLUMN_NAME = 'Compensation'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE history_dye ADD Compensation numeric(8, 2) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'enabled_set' AND COLUMN_NAME = 'txt_FormulaGroup'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE enabled_set ADD txt_FormulaGroup tinyint null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("select COUNT(*) from sysobjects where id = object_id('drop_system.dbo.brew_run_table')");
                if (dt_head.Rows[0][0].ToString() == "0")
                {

                    Communal._fadmSqlserver.ReviseData("CREATE TABLE [dbo].[brew_run_table]([MyID] [int] IDENTITY(1,1) NOT NULL,[MyDateTime] [datetime2](0) NULL,[State] [nvarchar](10) NULL,[Info] [nvarchar](500) NULL) ON [PRIMARY]");
                }

                dt_head = Communal._fadmSqlserver.GetData("select COUNT(*) from sysobjects where id = object_id('drop_system.dbo.formula_group')");
                if (dt_head.Rows[0][0].ToString() == "0")
                {

                    Communal._fadmSqlserver.ReviseData("CREATE TABLE [dbo].[formula_group]([Id] [int] IDENTITY(1,1) NOT NULL,[group_Name] [nvarchar](50) NULL,[node] [int] NULL,[AssistantCode] [nvarchar](50) NULL,[AssistantName] [nvarchar](50) NULL,[createTime] [datetime] NULL,[UnitOfAccount] [nvarchar](11) NULL) ON [PRIMARY]");
                }

                dt_head = Communal._fadmSqlserver.GetData("select COUNT(*) from sysobjects where id = object_id('drop_system.dbo.self_table')");
                if (dt_head.Rows[0][0].ToString() == "0")
                {

                    Communal._fadmSqlserver.ReviseData("CREATE TABLE [dbo].[self_table]([Date] [datetime] NULL,[SelfChecking1] [nvarchar](11) NULL,[SelfChecking2] [nvarchar](11) NULL,[SelfChecking3] [nvarchar](11) NULL,[SelfChecking4] [nvarchar](11) NULL,[BottleNum] [int] NULL,[CurrentAdjustWeight] [float] NULL,[AdjustValue] [float] NULL) ON [PRIMARY]");
                }

                dt_head = Communal._fadmSqlserver.GetData("select COUNT(*) from sysobjects where id = object_id('drop_system.dbo.self_table')");
                if (dt_head.Rows[0][0].ToString() == "0")
                {

                    Communal._fadmSqlserver.ReviseData("CREATE TABLE [dbo].[self_table]([Date] [datetime] NULL,[SelfChecking1] [nvarchar](11) NULL,[SelfChecking2] [nvarchar](11) NULL,[SelfChecking3] [nvarchar](11) NULL,[SelfChecking4] [nvarchar](11) NULL,[BottleNum] [int] NULL,[CurrentAdjustWeight] [float] NULL,[AdjustValue] [float] NULL) ON [PRIMARY]");
                }

                dt_head = Communal._fadmSqlserver.GetData("select COUNT(*) from sysobjects where id = object_id('drop_system.dbo.LimitTable')");
                if (dt_head.Rows[0][0].ToString() == "0")
                {

                    Communal._fadmSqlserver.ReviseData("CREATE TABLE [dbo].[LimitTable]([Type] [int] NULL,[Min] [numeric](8, 2) NULL,[Max] [numeric](8, 2) NULL,[Name] [nvarchar](50) NULL,[Value] [numeric](8, 2) NULL) ON [PRIMARY]");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'dyeing_process' AND COLUMN_NAME = 'Remark'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE dyeing_process ADD Remark [nvarchar](50) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'formula_details' AND COLUMN_NAME = 'BrewingData'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE formula_details ADD BrewingData [datetime2](7) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'formula_details_temp' AND COLUMN_NAME = 'BrewingData'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE formula_details_temp ADD BrewingData [datetime2](7) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'history_details' AND COLUMN_NAME = 'BrewingData'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE history_details ADD BrewingData [datetime2](7) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'dye_details' AND COLUMN_NAME = 'BrewingData'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE dye_details ADD BrewingData [datetime2](7) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'drop_details' AND COLUMN_NAME = 'BrewingData'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE drop_details ADD BrewingData [datetime2](7) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'history_dye' AND COLUMN_NAME = 'BrewingData'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE history_dye ADD BrewingData [datetime2](7) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'enabled_set' AND COLUMN_NAME = 'txt_CupCode'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE enabled_set ADD txt_CupCode tinyint null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'history_head' AND COLUMN_NAME = 'DescribeChar_EN'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE history_head ADD DescribeChar_EN nvarchar(200) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'drop_head' AND COLUMN_NAME = 'DescribeChar_EN'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE drop_head ADD DescribeChar_EN nvarchar(200) null ");
                }

                Communal._fadmSqlserver.ReviseData("update assistant_details set AllowMaxColoringConcentration =0 ,AllowMinColoringConcentration = 0 where AllowMaxColoringConcentration = 10 and AllowMinColoringConcentration = 0.0001");

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'cup_details' AND COLUMN_NAME = 'CoverStatus'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE cup_details ADD CoverStatus int null ");
                    Communal._fadmSqlserver.ReviseData("Update  cup_details set CoverStatus = 1");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'dye_details' AND COLUMN_NAME = 'ReceptionTime'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE dye_details ADD ReceptionTime [datetime2](0) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'dye_details' AND COLUMN_NAME = 'DyeType'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE dye_details ADD DyeType int null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'history_dye' AND COLUMN_NAME = 'ReceptionTime'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE history_dye ADD ReceptionTime [datetime2](0) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'history_dye' AND COLUMN_NAME = 'DyeType'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE history_dye ADD DyeType int null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'cup_details' AND COLUMN_NAME = 'ReceptionTime'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE cup_details ADD ReceptionTime [datetime2](0) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'cup_details' AND COLUMN_NAME = 'DyeType'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE cup_details ADD DyeType int null ");
                }
                
                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'formula_head' AND COLUMN_NAME = 'HandleBRList'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE formula_head ADD HandleBRList nvarchar(200) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'history_head' AND COLUMN_NAME = 'HandleBRList'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE history_head ADD HandleBRList nvarchar(200) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'drop_head' AND COLUMN_NAME = 'HandleBRList'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE drop_head ADD HandleBRList nvarchar(200) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'formula_head_temp' AND COLUMN_NAME = 'HandleBRList'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE formula_head_temp ADD HandleBRList nvarchar(200) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'dyeing_code' AND COLUMN_NAME = 'IndexNum'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE dyeing_code ADD IndexNum int null ");
                    //把旧数据重新插入Index值

                    string s_sql = "SELECT DyeingCode  FROM dyeing_code group by DyeingCode;";
                    DataTable dt_Dyeingcode = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                    foreach(DataRow dr in dt_Dyeingcode.Rows)
                    {
                        int i_index = 1;

                        s_sql = "select * from dyeing_code where DyeingCode ='" + dr[0].ToString() + "' order by Step,Type;";
                        DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                        foreach (DataRow drCode in dt_data.Rows)
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData("Update dyeing_code set IndexNum = " + i_index + " where DyeingCode = '" + drCode["DyeingCode"].ToString()
                                + "' and Type ='" + drCode["Type"].ToString() + "' and Step = '" + drCode["Step"].ToString() + "' and Code = '" + drCode["Code"].ToString() + "';");
                            i_index++;
                        }



                    }
                }

                //修改部分数据类型
                if (Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 1)
                {
                    dt_head = Communal._fadmSqlserver.GetData("select a.scale from syscolumns   a left join sysobjects d on  a.id = d.id where d.name = 'drop_details' and a.name = 'ObjectDropWeight' and a.scale is not null");
                    if (dt_head.Rows.Count > 0)
                    {
                        if (dt_head.Rows[0][0].ToString() == "2")
                        {
                            Communal._fadmSqlserver.ReviseData("alter table drop_details alter column ObjectDropWeight numeric(10, 3) ");
                        }
                    }

                    dt_head = Communal._fadmSqlserver.GetData("select a.scale from syscolumns   a left join sysobjects d on  a.id = d.id where d.name = 'drop_details' and a.name = 'RealDropWeight' and a.scale is not null");
                    if (dt_head.Rows.Count > 0)
                    {
                        if (dt_head.Rows[0][0].ToString() == "2")
                        {
                            Communal._fadmSqlserver.ReviseData("alter table drop_details alter column RealDropWeight numeric(10, 3) ");
                        }
                    }

                    dt_head = Communal._fadmSqlserver.GetData("select a.scale from syscolumns   a left join sysobjects d on  a.id = d.id where d.name = 'drop_head' and a.name = 'ObjectAddWaterWeight' and a.scale is not null");
                    if (dt_head.Rows.Count > 0)
                    {
                        if (dt_head.Rows[0][0].ToString() == "2")
                        {
                            Communal._fadmSqlserver.ReviseData("alter table drop_head alter column ObjectAddWaterWeight numeric(10, 3) ");
                        }
                    }

                    dt_head = Communal._fadmSqlserver.GetData("select a.scale from syscolumns   a left join sysobjects d on  a.id = d.id where d.name = 'drop_head' and a.name = 'RealAddWaterWeight' and a.scale is not null");
                    if (dt_head.Rows.Count > 0)
                    {
                        if (dt_head.Rows[0][0].ToString() == "2")
                        {
                            Communal._fadmSqlserver.ReviseData("alter table drop_head alter column RealAddWaterWeight numeric(10, 3) ");
                        }
                    }

                    dt_head = Communal._fadmSqlserver.GetData("select a.scale from syscolumns   a left join sysobjects d on  a.id = d.id where d.name = 'dye_details' and a.name = 'ObjectDropWeight' and a.scale is not null");
                    if (dt_head.Rows.Count > 0)
                    {
                        if (dt_head.Rows[0][0].ToString() == "2")
                        {
                            Communal._fadmSqlserver.ReviseData("alter table dye_details alter column ObjectDropWeight numeric(10, 3) ");
                        }
                    }

                    dt_head = Communal._fadmSqlserver.GetData("select a.scale from syscolumns   a left join sysobjects d on  a.id = d.id where d.name = 'dye_details' and a.name = 'RealDropWeight' and a.scale is not null");
                    if (dt_head.Rows.Count > 0)
                    {
                        if (dt_head.Rows[0][0].ToString() == "2")
                        {
                            Communal._fadmSqlserver.ReviseData("alter table dye_details alter column RealDropWeight numeric(10, 3) ");
                        }
                    }

                    dt_head = Communal._fadmSqlserver.GetData("select a.scale from syscolumns   a left join sysobjects d on  a.id = d.id where d.name = 'dye_details' and a.name = 'ObjectWaterWeight' and a.scale is not null");
                    if (dt_head.Rows.Count > 0)
                    {
                        if (dt_head.Rows[0][0].ToString() == "2")
                        {
                            Communal._fadmSqlserver.ReviseData("alter table dye_details alter column ObjectWaterWeight numeric(10, 3) ");
                        }
                    }

                    dt_head = Communal._fadmSqlserver.GetData("select a.scale from syscolumns   a left join sysobjects d on  a.id = d.id where d.name = 'dye_details' and a.name = 'Compensation' and a.scale is not null");
                    if (dt_head.Rows.Count > 0)
                    {
                        if (dt_head.Rows[0][0].ToString() == "2")
                        {
                            Communal._fadmSqlserver.ReviseData("alter table dye_details alter column Compensation numeric(10, 3) ");
                        }
                    }

                    dt_head = Communal._fadmSqlserver.GetData("select a.scale from syscolumns   a left join sysobjects d on  a.id = d.id where d.name = 'formula_handle_details' and a.name = 'ObjectDropWeight' and a.scale is not null");
                    if (dt_head.Rows.Count > 0)
                    {
                        if (dt_head.Rows[0][0].ToString() == "2")
                        {
                            Communal._fadmSqlserver.ReviseData("alter table formula_handle_details alter column ObjectDropWeight numeric(10, 3) ");
                        }
                    }

                    dt_head = Communal._fadmSqlserver.GetData("select a.scale from syscolumns   a left join sysobjects d on  a.id = d.id where d.name = 'formula_handle_details' and a.name = 'RealDropWeight' and a.scale is not null");
                    if (dt_head.Rows.Count > 0)
                    {
                        if (dt_head.Rows[0][0].ToString() == "2")
                        {
                            Communal._fadmSqlserver.ReviseData("alter table formula_handle_details alter column RealDropWeight numeric(10, 3) ");
                        }
                    }

                    dt_head = Communal._fadmSqlserver.GetData("select a.scale from syscolumns   a left join sysobjects d on  a.id = d.id where d.name = 'formula_handle_details_temp' and a.name = 'ObjectDropWeight' and a.scale is not null");
                    if (dt_head.Rows.Count > 0)
                    {
                        if (dt_head.Rows[0][0].ToString() == "2")
                        {
                            Communal._fadmSqlserver.ReviseData("alter table formula_handle_details_temp alter column ObjectDropWeight numeric(10, 3) ");
                        }
                    }

                    dt_head = Communal._fadmSqlserver.GetData("select a.scale from syscolumns   a left join sysobjects d on  a.id = d.id where d.name = 'formula_handle_details_temp' and a.name = 'RealDropWeight' and a.scale is not null");
                    if (dt_head.Rows.Count > 0)
                    {
                        if (dt_head.Rows[0][0].ToString() == "2")
                        {
                            Communal._fadmSqlserver.ReviseData("alter table formula_handle_details_temp alter column RealDropWeight numeric(10, 3) ");
                        }
                    }

                    dt_head = Communal._fadmSqlserver.GetData("select a.scale from syscolumns   a left join sysobjects d on  a.id = d.id where d.name = 'history_dye' and a.name = 'ObjectDropWeight' and a.scale is not null");
                    if (dt_head.Rows.Count > 0)
                    {
                        if (dt_head.Rows[0][0].ToString() == "2")
                        {
                            Communal._fadmSqlserver.ReviseData("alter table history_dye alter column ObjectDropWeight numeric(10, 3) ");
                        }
                    }

                    dt_head = Communal._fadmSqlserver.GetData("select a.scale from syscolumns   a left join sysobjects d on  a.id = d.id where d.name = 'history_dye' and a.name = 'RealDropWeight' and a.scale is not null");
                    if (dt_head.Rows.Count > 0)
                    {
                        if (dt_head.Rows[0][0].ToString() == "2")
                        {
                            Communal._fadmSqlserver.ReviseData("alter table history_dye alter column RealDropWeight numeric(10, 3) ");
                        }
                    }

                    dt_head = Communal._fadmSqlserver.GetData("select a.scale from syscolumns   a left join sysobjects d on  a.id = d.id where d.name = 'history_dye' and a.name = 'ObjectWaterWeight' and a.scale is not null");
                    if (dt_head.Rows.Count > 0)
                    {
                        if (dt_head.Rows[0][0].ToString() == "2")
                        {
                            Communal._fadmSqlserver.ReviseData("alter table history_dye alter column ObjectWaterWeight numeric(10, 3) ");
                        }
                    }
                }

                dt_head = Communal._fadmSqlserver.GetData("select COUNT(*) from sysobjects where id = object_id('drop_system.dbo.pre_brew')");
                if (dt_head.Rows[0][0].ToString() == "0")
                {

                    Communal._fadmSqlserver.ReviseData("CREATE TABLE [dbo].[pre_brew]([BottleNum] [int] NULL,[RealConcentration] [float] NULL,[CurrentWeight] [numeric](8, 2) NULL,[BrewingData] [datetime2](7) NULL) ON [PRIMARY]");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'drop_details' AND COLUMN_NAME = 'NeedPulse'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE drop_details ADD NeedPulse int null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'dye_details' AND COLUMN_NAME = 'NeedPulse'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE dye_details ADD NeedPulse int null ");
                }
                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'dye_details' AND COLUMN_NAME = 'WaterFinish'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE dye_details ADD WaterFinish int null ");
                }
                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'dye_details' AND COLUMN_NAME = 'Choose'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE dye_details ADD Choose int null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("select COUNT(*) from sysobjects where id = object_id('drop_system.dbo.standard')");
                if (dt_head.Rows[0][0].ToString() == "0")
                {

                    Communal._fadmSqlserver.ReviseData("CREATE TABLE [dbo].[standard]([E1] [nvarchar](1000) NULL,[E2] [nvarchar](1000) NULL,[WL] [nvarchar](1000) NULL,[FinishTime] [datetime2](0) NULL) ON [PRIMARY]");
                }

                dt_head = Communal._fadmSqlserver.GetData("select COUNT(*) from sysobjects where id = object_id('drop_system.dbo.history_absstandard')");
                if (dt_head.Rows[0][0].ToString() == "0")
                {

                    Communal._fadmSqlserver.ReviseData("CREATE TABLE [dbo].[history_absstandard]([E1] [nvarchar](1000) NULL,[E2] [nvarchar](1000) NULL,[WL] [nvarchar](1000) NULL,[FinishTime] [datetime2](0) NULL,[Type] [int] NULL) ON [PRIMARY]");
                }

                dt_head = Communal._fadmSqlserver.GetData("select COUNT(*) from sysobjects where id = object_id('drop_system.dbo.abs_cup_details')");
                if (dt_head.Rows[0][0].ToString() == "0")
                {

                    Communal._fadmSqlserver.ReviseData("CREATE TABLE [dbo].[abs_cup_details]([CupNum] [int] NOT NULL,[Enable] [tinyint] NULL,[IsUsing] [tinyint] NULL,[Statues] [nvarchar](50) NULL,[BottleNum] [int] NULL,[SampleDosage] [decimal](18, 3) NULL,[RealSampleDosage] [decimal](18, 3) NULL,[AdditivesNum] [int] NULL,[AdditivesDosage] [decimal](18, 3) NULL,[RealAdditivesDosage] [decimal](18, 3) NULL,[Pulse] [int] NULL,[Cooperate] [int] NULL,[Type] [int] NULL,[StartWave] [int] NULL,[EndWave] [int] NULL,[IntWave] [int] NULL,[TotalWeight] [decimal](18, 3) NULL) ON [PRIMARY]");

                    //插入数据

                    Communal._fadmSqlserver.ReviseData("INSERT INTO abs_cup_details(CupNum,Enable,IsUsing,Statues,BottleNum,AdditivesNum) VALUES (1,1,0,'待机',0,0)");
                    Communal._fadmSqlserver.ReviseData("INSERT INTO abs_cup_details(CupNum,Enable,IsUsing,Statues,BottleNum,AdditivesNum) VALUES (2,1,0,'待机',0,0)");
                }

                dt_head = Communal._fadmSqlserver.GetData("select COUNT(*) from sysobjects where id = object_id('drop_system.dbo.history_abs')");
                if (dt_head.Rows[0][0].ToString() == "0")
                {

                    Communal._fadmSqlserver.ReviseData("CREATE TABLE [dbo].[history_abs]([CupNum] [int] NOT NULL,[Enable] [tinyint] NULL,[IsUsing] [tinyint] NULL,[Statues] [nvarchar](50) NULL,[BottleNum] [int] NULL,[SampleDosage] [decimal](18, 3) NULL,[RealSampleDosage] [decimal](18, 3) NULL,[AdditivesNum] [int] NULL,[AdditivesDosage] [decimal](18, 3) NULL,[RealAdditivesDosage] [decimal](18, 3) NULL,[Pulse] [int] NULL,[Cooperate] [int] NULL,[Type] [int] NULL,[Abs] [nvarchar](1000) NULL,[L] [nvarchar](50) NULL,[A] [nvarchar](50) NULL,[B] [nvarchar](50) NULL,[FinishTime] [datetime2](0) NULL,[StartWave] [int] NULL,[EndWave] [int] NULL,[IntWave] [int] NULL,[Result] [nvarchar](500) NULL,[BrewingData] [datetime2](7) NULL,[RealConcentration] [float] NULL,[AssistantCode] [nvarchar](50) NULL,[Stand] [int] NULL) ON [PRIMARY]");
                }

                dt_head = Communal._fadmSqlserver.GetData("select COUNT(*) from sysobjects where id = object_id('drop_system.dbo.abs_wait_list')");
                if (dt_head.Rows[0][0].ToString() == "0")
                {

                    Communal._fadmSqlserver.ReviseData("CREATE TABLE [dbo].[abs_wait_list]([BottleNum] [int] NULL,[InsertDate] [datetime2](7) NULL) ON [PRIMARY]");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'assistant_details' AND COLUMN_NAME = 'Abs'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE assistant_details ADD Abs nvarchar(1000) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'assistant_details' AND COLUMN_NAME = 'L'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE assistant_details ADD L decimal(18, 3) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'assistant_details' AND COLUMN_NAME = 'A'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE assistant_details ADD A decimal(18, 3) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'assistant_details' AND COLUMN_NAME = 'B'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE assistant_details ADD B decimal(18, 3) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'bottle_details' AND COLUMN_NAME = 'Abs'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE bottle_details ADD Abs nvarchar(1000) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'bottle_details' AND COLUMN_NAME = 'L'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE bottle_details ADD L decimal(18, 3) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'bottle_details' AND COLUMN_NAME = 'A'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE bottle_details ADD A decimal(18, 3) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'bottle_details' AND COLUMN_NAME = 'B'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE bottle_details ADD B decimal(18, 3) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'bottle_details' AND COLUMN_NAME = 'Compensate'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE bottle_details ADD Compensate decimal(18, 3) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'standard' AND COLUMN_NAME = 'Type'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE standard ADD Type int null ");
                    Communal._fadmSqlserver.ReviseData("Update standard set Type =1 ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'abs_wait_list' AND COLUMN_NAME = 'Type'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE abs_wait_list ADD Type int null ");
                    Communal._fadmSqlserver.ReviseData("Update abs_wait_list set Type =0 ");
                }


                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'dyeing_details' AND COLUMN_NAME = 'No'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE dyeing_details ADD No int null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'brewing_process' AND COLUMN_NAME = 'Ratio'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE brewing_process ADD Ratio int null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'drop_details' AND COLUMN_NAME = 'StandError'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE drop_details ADD StandError nvarchar(12) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'history_details' AND COLUMN_NAME = 'StandError'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE history_details ADD StandError nvarchar(12) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'dye_details' AND COLUMN_NAME = 'StandError'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE dye_details ADD StandError nvarchar(12) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'history_dye' AND COLUMN_NAME = 'StandError'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE history_dye ADD StandError nvarchar(12) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'bottle_details' AND COLUMN_NAME = 'Status'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE bottle_details ADD Status int null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'drop_head' AND COLUMN_NAME = 'WaterStandError'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE drop_head ADD WaterStandError nvarchar(12) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'history_head' AND COLUMN_NAME = 'WaterStandError'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE history_head ADD WaterStandError nvarchar(12) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'bottle_details' AND COLUMN_NAME = 'DripReserveFirst'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE bottle_details ADD DripReserveFirst int null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'dyeing_code' AND COLUMN_NAME = 'IsUse'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE dyeing_code ADD IsUse int null ");
                    Communal._fadmSqlserver.ReviseData("Update dyeing_code Set IsUse=1");

                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'formula_head' AND COLUMN_NAME = 'ClothNum'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE formula_head ADD ClothNum int null ");

                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'formula_head_temp' AND COLUMN_NAME = 'ClothNum'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE formula_head_temp ADD ClothNum int null ");

                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'formula_head' AND COLUMN_NAME = 'IsAutoIn'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE formula_head ADD IsAutoIn int null ");
                    Communal._fadmSqlserver.ReviseData("Update formula_head Set IsAutoIn=0");

                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'formula_head_temp' AND COLUMN_NAME = 'IsAutoIn'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE formula_head_temp ADD IsAutoIn int null ");

                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'drop_head' AND COLUMN_NAME = 'IsAutoIn'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE drop_head ADD IsAutoIn int null ");
                    Communal._fadmSqlserver.ReviseData("Update drop_head Set IsAutoIn=0");

                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'drop_head' AND COLUMN_NAME = 'ClothNum'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE drop_head ADD ClothNum int null ");

                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'history_head' AND COLUMN_NAME = 'ClothNum'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE history_head ADD ClothNum int null");

                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'history_head' AND COLUMN_NAME = 'IsAutoIn'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE history_head ADD IsAutoIn int null");
                    Communal._fadmSqlserver.ReviseData("Update history_head Set IsAutoIn=0");

                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'bottle_details' AND COLUMN_NAME = 'WashSyringeSpan'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE bottle_details ADD WashSyringeSpan int null ");
                    Communal._fadmSqlserver.ReviseData("Update bottle_details Set WashSyringeSpan=24");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'bottle_details' AND COLUMN_NAME = 'LastWashTime'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE bottle_details ADD LastWashTime [datetime2](0) null ");
                    Communal._fadmSqlserver.ReviseData("Update bottle_details Set LastWashTime='"+ DateTime.Now+"'");
                    
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'formula_handle_details' AND COLUMN_NAME = 'No'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE formula_handle_details ADD No int null ");

                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'formula_handle_details_temp' AND COLUMN_NAME = 'No'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE formula_handle_details_temp ADD No int null ");

                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'assistant_details' AND COLUMN_NAME = 'Reweigh'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE assistant_details ADD Reweigh int null ");
                    Communal._fadmSqlserver.ReviseData("Update assistant_details Set Reweigh=1");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'dyeing_process' AND COLUMN_NAME = 'OpenMedicine'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE dyeing_process ADD OpenMedicine int null ");
                    Communal._fadmSqlserver.ReviseData("Update dyeing_process Set OpenMedicine=0");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'formula_head' AND COLUMN_NAME = 'DyeingCodeRemark'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE formula_head ADD DyeingCodeRemark nvarchar(50) null ");

                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'formula_head_temp' AND COLUMN_NAME = 'DyeingCodeRemark'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE formula_head_temp ADD DyeingCodeRemark nvarchar(50) null ");

                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'drop_head' AND COLUMN_NAME = 'DyeingCodeRemark'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE drop_head ADD DyeingCodeRemark nvarchar(50) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'history_head' AND COLUMN_NAME = 'DyeingCodeRemark'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE history_head ADD DyeingCodeRemark nvarchar(50) null");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'formula_head' AND COLUMN_NAME = 'Recoloration'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE formula_head ADD Recoloration nvarchar(50) null ");

                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'formula_head_temp' AND COLUMN_NAME = 'Recoloration'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE formula_head_temp ADD Recoloration nvarchar(50) null ");

                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'drop_head' AND COLUMN_NAME = 'Recoloration'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE drop_head ADD Recoloration nvarchar(50) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'history_head' AND COLUMN_NAME = 'Recoloration'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE history_head ADD Recoloration nvarchar(50) null");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'drop_head' AND COLUMN_NAME = 'VatNumber'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE drop_head ADD VatNumber nvarchar(50) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'history_head' AND COLUMN_NAME = 'VatNumber'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE history_head ADD VatNumber nvarchar(50) null");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'enabled_set' AND COLUMN_NAME = 'txt_Recoloration'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE enabled_set ADD txt_Recoloration tinyint null ");
                    Communal._fadmSqlserver.ReviseData("Update enabled_set set txt_Recoloration = 0 ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'dyeing_code' AND COLUMN_NAME = 'Remark'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE dyeing_code ADD Remark nvarchar(50) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'history_head' AND COLUMN_NAME = 'Result'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE history_head ADD Result nvarchar(200) null");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'enabled_set' AND COLUMN_NAME = 'txt_Colorists'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE enabled_set ADD txt_Colorists tinyint null ");
                    Communal._fadmSqlserver.ReviseData("Update enabled_set set txt_Colorists = 0 ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'formula_head' AND COLUMN_NAME = 'Colorists'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE formula_head ADD Colorists nvarchar(50) null ");

                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'formula_head_temp' AND COLUMN_NAME = 'Colorists'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE formula_head_temp ADD Colorists nvarchar(50) null ");

                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'drop_head' AND COLUMN_NAME = 'Colorists'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE drop_head ADD Colorists nvarchar(50) null ");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'history_head' AND COLUMN_NAME = 'Colorists'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE history_head ADD Colorists nvarchar(50) null");
                }

                dt_head = Communal._fadmSqlserver.GetData("select COUNT(*) from sysobjects where id = object_id('drop_system.dbo.Colorists_table')");
                if (dt_head.Rows[0][0].ToString() == "0")
                {

                    Communal._fadmSqlserver.ReviseData("CREATE TABLE [dbo].[Colorists_table]([Coloristsname] [nvarchar](100) NULL) ON [PRIMARY]");
                }

                dt_head = Communal._fadmSqlserver.GetData("SELECT *FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'wait_list' AND COLUMN_NAME = 'ClothNum'");
                if (dt_head.Rows.Count == 0)
                {
                    Communal._fadmSqlserver.ReviseData("ALTER TABLE wait_list ADD ClothNum int null ");
                }

            }
            catch { }

            SmartDyeing.FADM_Object.Communal._lis_dripCupNum.Clear();
            SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Clear();
            SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Clear();
            SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Clear();
            SmartDyeing.FADM_Object.Communal._dic_dyeType.Clear();
            SmartDyeing.FADM_Object.Communal._dic_first_second.Clear();
            SmartDyeing.FADM_Object.Communal._dic_big_small_cup.Clear();
            SmartDyeing.FADM_Object.Communal._dic_cup_index.Clear();
            SmartDyeing.FADM_Object.Communal._lis_PrecisionCupNum.Clear();
            Communal._dic_dyecup_index.Clear();
            int i_cupmin = 0;
            int i_cupmax = 0;
            List<int> lis_cup = new List<int>();
            List<int> lis_type = new List<int>();
            //判断配置文件是否正确
            //1.确保杯号顺序添加
            if (Lib_Card.Configure.Parameter.Machine_Area1_Type != 0)
            {
                if(Lib_Card.Configure.Parameter.Machine_Area1_CupMin>= Lib_Card.Configure.Parameter.Machine_Area1_CupMax)
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show("配置文件异常，请确保文件正确！", "登录异常",
                   MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("Configuration file exception, please ensure the file is correct！", "Login exception",
                   MessageBoxButtons.OK, false);
                    return;
                }

                i_cupmin = Lib_Card.Configure.Parameter.Machine_Area1_CupMin;
                i_cupmax = Lib_Card.Configure.Parameter.Machine_Area1_CupMax;
                for (int i = i_cupmin; i <= i_cupmax; i++)
                {
                    lis_cup.Add(i);
                    lis_type.Add(Lib_Card.Configure.Parameter.Machine_Area1_Type);
                    if(Communal._dic_cup_index.Count == 0)
                    {
                        Communal._dic_cup_index.Add(i, 0);
                    }
                    else
                    {
                        Communal._dic_cup_index.Add(i, Communal._dic_cup_index.Count);
                    }
                    Communal._dic_big_small_cup.Add(i, Lib_Card.Configure.Parameter.Machine_Area1_Big);
                    if (Lib_Card.Configure.Parameter.Machine_Area1_Type == 2)
                    {
                        SmartDyeing.FADM_Object.Communal._lis_dripCupNum.Add(i);
                        SmartDyeing.FADM_Object.Communal._dic_first_second.Add(i, 0);
                    }
                    if (Lib_Card.Configure.Parameter.Machine_Area1_Type == 3)
                    {
                        if (Communal._dic_dyecup_index.Count == 0)
                        {
                            Communal._dic_dyecup_index.Add(i, 0);
                        }
                        else
                        {
                            Communal._dic_dyecup_index.Add(i, Communal._dic_dyecup_index.Count);
                        }
                        SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Add(i);
                        SmartDyeing.FADM_Object.Communal._dic_dyeType.Add(i, Lib_Card.Configure.Parameter.Machine_Area1_DyeType);
                        if(Lib_Card.Configure.Parameter.Machine_Area1_DyeType==2
                            || Lib_Card.Configure.Parameter.Machine_Area1_DyeType == 5)
                        {
                            try
                            {
                                if((i- i_cupmin)%2==0)
                                {
                                    SmartDyeing.FADM_Object.Communal._dic_first_second.Add(i, i + 1);
                                }
                                else
                                {
                                    SmartDyeing.FADM_Object.Communal._dic_first_second.Add(i, i - 1);
                                }
                            }
                            catch
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    FADM_Form.CustomMessageBox.Show("配置文件异常，请确保文件正确！", "登录异常",
                               MessageBoxButtons.OK, false);
                                else
                                    FADM_Form.CustomMessageBox.Show("Configuration file exception, please ensure the file is correct！", "Login exception",
                               MessageBoxButtons.OK, false);
                                return;
                            }

                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 1);
                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 3);
                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 5);
                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 7);
                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 9);
                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 11);

                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin );
                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin + 2);
                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin + 4);
                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin + 6);
                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin + 8);
                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin + 10);
                        }
                        else
                        {
                            SmartDyeing.FADM_Object.Communal._dic_first_second.Add(i, 0);
                        }

                        //新增精密机杯号
                        if (Lib_Card.Configure.Parameter.Machine_Area1_DyeType == 6|| Lib_Card.Configure.Parameter.Machine_Area1_DyeType == 7)
                        {
                            SmartDyeing.FADM_Object.Communal._lis_PrecisionCupNum.Add(i);
                        }
                    }
                }
            }
            if (Lib_Card.Configure.Parameter.Machine_Area2_Type != 0)
            {
                if (Lib_Card.Configure.Parameter.Machine_Area2_CupMin >= Lib_Card.Configure.Parameter.Machine_Area2_CupMax)
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show("配置文件异常，请确保文件正确！", "登录异常",
                   MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("Configuration file exception, please ensure the file is correct！", "Login exception",
                   MessageBoxButtons.OK, false);
                    return;
                }
                if (Lib_Card.Configure.Parameter.Machine_Area2_Type != 2)
                {
                    if (i_cupmax + 1 != Lib_Card.Configure.Parameter.Machine_Area2_CupMin)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("配置文件异常，请确保文件正确！", "登录异常",
                       MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("Configuration file exception, please ensure the file is correct！", "Login exception",
                       MessageBoxButtons.OK, false);
                        return;
                    }
                }
                else
                {
                    if (i_cupmax>= Lib_Card.Configure.Parameter.Machine_Area2_CupMin)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("配置文件异常，请确保文件正确！", "登录异常",
                       MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("Configuration file exception, please ensure the file is correct！", "Login exception",
                       MessageBoxButtons.OK, false);
                        return;
                    }
                }
                i_cupmin = Lib_Card.Configure.Parameter.Machine_Area2_CupMin;
                i_cupmax = Lib_Card.Configure.Parameter.Machine_Area2_CupMax;
                for (int i = i_cupmin; i <= i_cupmax; i++)
                {
                    lis_cup.Add(i);
                    lis_type.Add(Lib_Card.Configure.Parameter.Machine_Area2_Type);
                    if (Communal._dic_cup_index.Count == 0)
                    {
                        Communal._dic_cup_index.Add(i, 0);
                    }
                    else
                    {
                        Communal._dic_cup_index.Add(i, Communal._dic_cup_index.Count);
                    }
                    Communal._dic_big_small_cup.Add(i, Lib_Card.Configure.Parameter.Machine_Area2_Big);
                    if (Lib_Card.Configure.Parameter.Machine_Area2_Type == 2)
                    {
                        SmartDyeing.FADM_Object.Communal._lis_dripCupNum.Add(i);
                        SmartDyeing.FADM_Object.Communal._dic_first_second.Add(i, 0);
                    }
                    if (Lib_Card.Configure.Parameter.Machine_Area2_Type == 3)
                    {
                        if (Communal._dic_dyecup_index.Count == 0)
                        {
                            Communal._dic_dyecup_index.Add(i, 0);
                        }
                        else
                        {
                            Communal._dic_dyecup_index.Add(i, Communal._dic_dyecup_index.Count);
                        }
                        SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Add(i);
                        SmartDyeing.FADM_Object.Communal._dic_dyeType.Add(i, Lib_Card.Configure.Parameter.Machine_Area2_DyeType);
                        if (Lib_Card.Configure.Parameter.Machine_Area2_DyeType == 2|| Lib_Card.Configure.Parameter.Machine_Area2_DyeType == 5)
                        {
                            try
                            {
                                if ((i - i_cupmin) % 2 == 0)
                                {
                                    SmartDyeing.FADM_Object.Communal._dic_first_second.Add(i, i + 1);
                                }
                                else
                                {
                                    SmartDyeing.FADM_Object.Communal._dic_first_second.Add(i, i - 1);
                                }
                            }
                            catch
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    FADM_Form.CustomMessageBox.Show("配置文件异常，请确保文件正确！", "登录异常",
                               MessageBoxButtons.OK, false);
                                else
                                    FADM_Form.CustomMessageBox.Show("Configuration file exception, please ensure the file is correct！", "Login exception",
                               MessageBoxButtons.OK, false);
                                return;
                            }

                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 1);
                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 3);
                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 5);
                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 7);
                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 9);
                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 11);



                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin);
                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin + 2);
                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin + 4);
                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin + 6);
                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin + 8);
                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin + 10);
                        }

                        else
                        {
                            SmartDyeing.FADM_Object.Communal._dic_first_second.Add(i, 0);
                        }

                        //新增精密机杯号
                        if (Lib_Card.Configure.Parameter.Machine_Area2_DyeType == 6|| Lib_Card.Configure.Parameter.Machine_Area2_DyeType == 7)
                        {
                            SmartDyeing.FADM_Object.Communal._lis_PrecisionCupNum.Add(i);
                        }
                    }
                }
            }
            if (Lib_Card.Configure.Parameter.Machine_Area3_Type != 0)
            {
                if (Lib_Card.Configure.Parameter.Machine_Area3_CupMin >= Lib_Card.Configure.Parameter.Machine_Area3_CupMax)
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show("配置文件异常，请确保文件正确！", "登录异常",
                   MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("Configuration file exception, please ensure the file is correct！", "Login exception",
                   MessageBoxButtons.OK, false);
                    return;
                }
                if (Lib_Card.Configure.Parameter.Machine_Area3_Type != 2)
                {
                    if (i_cupmax + 1 != Lib_Card.Configure.Parameter.Machine_Area3_CupMin)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("配置文件异常，请确保文件正确！", "登录异常",
                       MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("Configuration file exception, please ensure the file is correct！", "Login exception",
                       MessageBoxButtons.OK, false);
                        return;
                    }
                }
                else
                {
                    if (i_cupmax >= Lib_Card.Configure.Parameter.Machine_Area3_CupMin)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("配置文件异常，请确保文件正确！", "登录异常",
                       MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("Configuration file exception, please ensure the file is correct！", "Login exception",
                       MessageBoxButtons.OK, false);
                        return;
                    }
                }
                i_cupmin = Lib_Card.Configure.Parameter.Machine_Area3_CupMin;
                i_cupmax = Lib_Card.Configure.Parameter.Machine_Area3_CupMax;
                for (int i = i_cupmin; i <= i_cupmax; i++)
                {
                    lis_cup.Add(i);
                    lis_type.Add(Lib_Card.Configure.Parameter.Machine_Area3_Type);
                    if (Communal._dic_cup_index.Count == 0)
                    {
                        Communal._dic_cup_index.Add(i, 0);
                    }
                    else
                    {
                        Communal._dic_cup_index.Add(i, Communal._dic_cup_index.Count);
                    }
                    Communal._dic_big_small_cup.Add(i, Lib_Card.Configure.Parameter.Machine_Area3_Big);
                    if (Lib_Card.Configure.Parameter.Machine_Area3_Type == 2)
                    {
                        SmartDyeing.FADM_Object.Communal._lis_dripCupNum.Add(i);
                        SmartDyeing.FADM_Object.Communal._dic_first_second.Add(i, 0);
                    }
                    if (Lib_Card.Configure.Parameter.Machine_Area3_Type == 3)
                    {
                        if (Communal._dic_dyecup_index.Count == 0)
                        {
                            Communal._dic_dyecup_index.Add(i, 0);
                        }
                        else
                        {
                            Communal._dic_dyecup_index.Add(i, Communal._dic_dyecup_index.Count);
                        }
                        SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Add(i);
                        SmartDyeing.FADM_Object.Communal._dic_dyeType.Add(i, Lib_Card.Configure.Parameter.Machine_Area3_DyeType);
                        if (Lib_Card.Configure.Parameter.Machine_Area3_DyeType == 2 || Lib_Card.Configure.Parameter.Machine_Area3_DyeType == 5)
                        {
                            try
                            {
                                if ((i - i_cupmin) % 2 == 0)
                                {
                                    SmartDyeing.FADM_Object.Communal._dic_first_second.Add(i, i + 1);
                                }
                                else
                                {
                                    SmartDyeing.FADM_Object.Communal._dic_first_second.Add(i, i - 1);
                                }
                            }
                            catch
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    FADM_Form.CustomMessageBox.Show("配置文件异常，请确保文件正确！", "登录异常",
                               MessageBoxButtons.OK, false);
                                else
                                    FADM_Form.CustomMessageBox.Show("Configuration file exception, please ensure the file is correct！", "Login exception",
                               MessageBoxButtons.OK, false);
                                return;
                            }

                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 1);
                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 3);
                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 5);
                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 7);
                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 9);
                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 11);



                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin);
                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin + 2);
                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin + 4);
                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin + 6);
                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin + 8);
                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin + 10);
                        }
                        else
                        {
                            SmartDyeing.FADM_Object.Communal._dic_first_second.Add(i, 0);
                        }

                        //新增精密机杯号
                        if (Lib_Card.Configure.Parameter.Machine_Area3_DyeType == 6 || Lib_Card.Configure.Parameter.Machine_Area3_DyeType == 7)
                        {
                            SmartDyeing.FADM_Object.Communal._lis_PrecisionCupNum.Add(i);
                        }
                    }
                }
            }
            if (Lib_Card.Configure.Parameter.Machine_Area4_Type != 0)
            {
                if (Lib_Card.Configure.Parameter.Machine_Area4_CupMin >= Lib_Card.Configure.Parameter.Machine_Area4_CupMax)
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show("配置文件异常，请确保文件正确！", "登录异常",
                   MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("Configuration file exception, please ensure the file is correct！", "Login exception",
                   MessageBoxButtons.OK, false);
                    return;
                }
                if (Lib_Card.Configure.Parameter.Machine_Area4_Type != 2)
                {

                    if (i_cupmax + 1 != Lib_Card.Configure.Parameter.Machine_Area4_CupMin)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("配置文件异常，请确保文件正确！", "登录异常",
                       MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("Configuration file exception, please ensure the file is correct！", "Login exception",
                       MessageBoxButtons.OK, false);
                        return;
                    }
                }
                else
                {
                    if (i_cupmax >= Lib_Card.Configure.Parameter.Machine_Area4_CupMin)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("配置文件异常，请确保文件正确！", "登录异常",
                       MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("Configuration file exception, please ensure the file is correct！", "Login exception",
                       MessageBoxButtons.OK, false);
                        return;
                    }
                }
                i_cupmin = Lib_Card.Configure.Parameter.Machine_Area4_CupMin;
                i_cupmax = Lib_Card.Configure.Parameter.Machine_Area4_CupMax;
                for (int i = i_cupmin; i <= i_cupmax; i++)
                {
                    lis_cup.Add(i);
                    if (Communal._dic_cup_index.Count == 0)
                    {
                        Communal._dic_cup_index.Add(i, 0);
                    }
                    else
                    {
                        Communal._dic_cup_index.Add(i, Communal._dic_cup_index.Count);
                    }
                    lis_type.Add(Lib_Card.Configure.Parameter.Machine_Area4_Type);
                    Communal._dic_big_small_cup.Add(i, Lib_Card.Configure.Parameter.Machine_Area4_Big);
                    if (Lib_Card.Configure.Parameter.Machine_Area4_Type == 2)
                    {
                        SmartDyeing.FADM_Object.Communal._lis_dripCupNum.Add(i);
                        SmartDyeing.FADM_Object.Communal._dic_first_second.Add(i, 0);
                    }
                    if (Lib_Card.Configure.Parameter.Machine_Area4_Type == 3)
                    {
                        if (Communal._dic_dyecup_index.Count == 0)
                        {
                            Communal._dic_dyecup_index.Add(i, 0);
                        }
                        else
                        {
                            Communal._dic_dyecup_index.Add(i, Communal._dic_dyecup_index.Count);
                        }
                        SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Add(i);
                        SmartDyeing.FADM_Object.Communal._dic_dyeType.Add(i, Lib_Card.Configure.Parameter.Machine_Area4_DyeType);
                        if (Lib_Card.Configure.Parameter.Machine_Area4_DyeType == 2 || Lib_Card.Configure.Parameter.Machine_Area4_DyeType == 5)
                        {
                            try
                            {
                                if ((i - i_cupmin) % 2 == 0)
                                {
                                    SmartDyeing.FADM_Object.Communal._dic_first_second.Add(i, i + 1);
                                }
                                else
                                {
                                    SmartDyeing.FADM_Object.Communal._dic_first_second.Add(i, i - 1);
                                }
                            }
                            catch
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    FADM_Form.CustomMessageBox.Show("配置文件异常，请确保文件正确！", "登录异常",
                               MessageBoxButtons.OK, false);
                                else
                                    FADM_Form.CustomMessageBox.Show("Configuration file exception, please ensure the file is correct！", "Login exception",
                               MessageBoxButtons.OK, false);
                                return;
                            }

                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 1);
                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 3);
                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 5);
                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 7);
                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 9);
                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 11);



                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin);
                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin + 2);
                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin + 4);
                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin + 6);
                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin + 8);
                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin + 10);
                        }


                        else
                        {
                            SmartDyeing.FADM_Object.Communal._dic_first_second.Add(i, 0);
                        }

                        //新增精密机杯号
                        if (Lib_Card.Configure.Parameter.Machine_Area4_DyeType == 6 || Lib_Card.Configure.Parameter.Machine_Area4_DyeType == 7)
                        {
                            SmartDyeing.FADM_Object.Communal._lis_PrecisionCupNum.Add(i);
                        }
                    }
                }
            }
            if (Lib_Card.Configure.Parameter.Machine_Area5_Type != 0)
            {
                if (Lib_Card.Configure.Parameter.Machine_Area5_CupMin >= Lib_Card.Configure.Parameter.Machine_Area5_CupMax)
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show("配置文件异常，请确保文件正确！", "登录异常",
                   MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("Configuration file exception, please ensure the file is correct！", "Login exception",
                   MessageBoxButtons.OK, false);
                    return;
                }
                if (Lib_Card.Configure.Parameter.Machine_Area5_Type != 2)
                {
                    if (i_cupmax + 1 != Lib_Card.Configure.Parameter.Machine_Area5_CupMin)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("配置文件异常，请确保文件正确！", "登录异常",
                       MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("Configuration file exception, please ensure the file is correct！", "Login exception",
                       MessageBoxButtons.OK, false);
                        return;
                    }
                }
                else
                {
                    if (i_cupmax >= Lib_Card.Configure.Parameter.Machine_Area5_CupMin)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("配置文件异常，请确保文件正确！", "登录异常",
                       MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("Configuration file exception, please ensure the file is correct！", "Login exception",
                       MessageBoxButtons.OK, false);
                        return;
                    }
                }
                i_cupmin = Lib_Card.Configure.Parameter.Machine_Area5_CupMin;
                i_cupmax = Lib_Card.Configure.Parameter.Machine_Area5_CupMax;
                for (int i = i_cupmin; i <= i_cupmax; i++)
                {
                    lis_cup.Add(i);
                    if (Communal._dic_cup_index.Count == 0)
                    {
                        Communal._dic_cup_index.Add(i, 0);
                    }
                    else
                    {
                        Communal._dic_cup_index.Add(i, Communal._dic_cup_index.Count);
                    }
                    lis_type.Add(Lib_Card.Configure.Parameter.Machine_Area5_Type);
                    Communal._dic_big_small_cup.Add(i, Lib_Card.Configure.Parameter.Machine_Area5_Big);
                    if (Lib_Card.Configure.Parameter.Machine_Area5_Type == 2)
                    {
                        SmartDyeing.FADM_Object.Communal._lis_dripCupNum.Add(i);
                        SmartDyeing.FADM_Object.Communal._dic_first_second.Add(i, 0);
                    }
                    if (Lib_Card.Configure.Parameter.Machine_Area5_Type == 3)
                    {
                        if (Communal._dic_dyecup_index.Count == 0)
                        {
                            Communal._dic_dyecup_index.Add(i, 0);
                        }
                        else
                        {
                            Communal._dic_dyecup_index.Add(i, Communal._dic_dyecup_index.Count);
                        }
                        SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Add(i);
                        SmartDyeing.FADM_Object.Communal._dic_dyeType.Add(i, Lib_Card.Configure.Parameter.Machine_Area5_DyeType);
                        if (Lib_Card.Configure.Parameter.Machine_Area5_DyeType == 2 || Lib_Card.Configure.Parameter.Machine_Area5_DyeType == 5)
                        {
                            try
                            {
                                if ((i - i_cupmin) % 2 == 0)
                                {
                                    SmartDyeing.FADM_Object.Communal._dic_first_second.Add(i, i + 1);
                                }
                                else
                                {
                                    SmartDyeing.FADM_Object.Communal._dic_first_second.Add(i, i - 1);
                                }
                            }
                            catch
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    FADM_Form.CustomMessageBox.Show("配置文件异常，请确保文件正确！", "登录异常",
                               MessageBoxButtons.OK, false);
                                else
                                    FADM_Form.CustomMessageBox.Show("Configuration file exception, please ensure the file is correct！", "Login exception",
                               MessageBoxButtons.OK, false);
                                return;
                            }

                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 1);
                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 3);
                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 5);
                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 7);
                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 9);
                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 11);



                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin);
                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin + 2);
                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin + 4);
                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin + 6);
                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin + 8);
                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin + 10);
                        }
                        else
                        {
                            SmartDyeing.FADM_Object.Communal._dic_first_second.Add(i, 0);
                        }

                        //新增精密机杯号
                        if (Lib_Card.Configure.Parameter.Machine_Area5_DyeType == 6 || Lib_Card.Configure.Parameter.Machine_Area5_DyeType == 7)
                        {
                            SmartDyeing.FADM_Object.Communal._lis_PrecisionCupNum.Add(i);
                        }
                    }
                }
            }
            if (Lib_Card.Configure.Parameter.Machine_Area6_Type != 0)
            {
                if (Lib_Card.Configure.Parameter.Machine_Area6_CupMin >= Lib_Card.Configure.Parameter.Machine_Area6_CupMax)
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show("配置文件异常，请确保文件正确！", "登录异常",
                   MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("Configuration file exception, please ensure the file is correct！", "Login exception",
                   MessageBoxButtons.OK, false);
                    return;
                }
                if (Lib_Card.Configure.Parameter.Machine_Area6_Type != 2)
                {
                    if (i_cupmax + 1 != Lib_Card.Configure.Parameter.Machine_Area6_CupMin)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("配置文件异常，请确保文件正确！", "登录异常",
                       MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("Configuration file exception, please ensure the file is correct！", "Login exception",
                       MessageBoxButtons.OK, false);
                        return;
                    }
                }
                else
                {
                    if (i_cupmax >= Lib_Card.Configure.Parameter.Machine_Area6_CupMin)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("配置文件异常，请确保文件正确！", "登录异常",
                       MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("Configuration file exception, please ensure the file is correct！", "Login exception",
                       MessageBoxButtons.OK, false);
                        return;
                    }
                }
                i_cupmin = Lib_Card.Configure.Parameter.Machine_Area6_CupMin;
                i_cupmax = Lib_Card.Configure.Parameter.Machine_Area6_CupMax;
                for (int i = i_cupmin; i <= i_cupmax; i++)
                {
                    lis_cup.Add(i);
                    if (Communal._dic_cup_index.Count == 0)
                    {
                        Communal._dic_cup_index.Add(i, 0);
                    }
                    else
                    {
                        Communal._dic_cup_index.Add(i, Communal._dic_cup_index.Count);
                    }
                    lis_type.Add(Lib_Card.Configure.Parameter.Machine_Area6_Type);
                    Communal._dic_big_small_cup.Add(i, Lib_Card.Configure.Parameter.Machine_Area6_Big);
                    if (Lib_Card.Configure.Parameter.Machine_Area6_Type == 2)
                    {
                        SmartDyeing.FADM_Object.Communal._lis_dripCupNum.Add(i);
                        SmartDyeing.FADM_Object.Communal._dic_first_second.Add(i, 0);
                    }
                    if (Lib_Card.Configure.Parameter.Machine_Area6_Type == 3)
                    {
                        if (Communal._dic_dyecup_index.Count == 0)
                        {
                            Communal._dic_dyecup_index.Add(i, 0);
                        }
                        else
                        {
                            Communal._dic_dyecup_index.Add(i, Communal._dic_dyecup_index.Count);
                        }
                        SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Add(i);
                        SmartDyeing.FADM_Object.Communal._dic_dyeType.Add(i, Lib_Card.Configure.Parameter.Machine_Area6_DyeType);
                        if (Lib_Card.Configure.Parameter.Machine_Area6_DyeType == 2 || Lib_Card.Configure.Parameter.Machine_Area6_DyeType == 5)
                        {
                            try
                            {
                                if ((i - i_cupmin) % 2 == 0)
                                {
                                    SmartDyeing.FADM_Object.Communal._dic_first_second.Add(i, i + 1);
                                }
                                else
                                {
                                    SmartDyeing.FADM_Object.Communal._dic_first_second.Add(i, i - 1);
                                }
                            }
                            catch
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    FADM_Form.CustomMessageBox.Show("配置文件异常，请确保文件正确！", "登录异常",
                               MessageBoxButtons.OK, false);
                                else
                                    FADM_Form.CustomMessageBox.Show("Configuration file exception, please ensure the file is correct！", "Login exception",
                               MessageBoxButtons.OK, false);
                                return;
                            }

                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 1);
                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 3);
                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 5);
                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 7);
                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 9);
                            //SmartDyeing.FADM_Object.Communal._lis_deputyCupNum.Add(i_cupmin + 11);



                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin);
                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin + 2);
                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin + 4);
                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin + 6);
                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin + 8);
                            //SmartDyeing.FADM_Object.Communal._lis_firstCupNum.Add(i_cupmin + 10);
                        }
                        else
                        {
                            SmartDyeing.FADM_Object.Communal._dic_first_second.Add(i, 0);
                        }

                        //新增精密机杯号
                        if (Lib_Card.Configure.Parameter.Machine_Area6_DyeType == 6 || Lib_Card.Configure.Parameter.Machine_Area6_DyeType == 7)
                        {
                            SmartDyeing.FADM_Object.Communal._lis_PrecisionCupNum.Add(i);
                        }
                    }
                }
            }
            if (lis_cup[0] != 1)
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    FADM_Form.CustomMessageBox.Show("配置文件异常，请确保文件正确！", "登录异常",
               MessageBoxButtons.OK, false);
                else
                    FADM_Form.CustomMessageBox.Show("Configuration file exception, please ensure the file is correct！", "Login exception",
               MessageBoxButtons.OK, false);
                return;
            }
            //2.确保与数据库杯号信息一致
            DataTable dt_data1 = FADM_Object.Communal._fadmSqlserver.GetData("SELECT * FROM cup_details where CupNum is not null and Type is not null  order by CupNum;");
            int i_nRetset = 0;
            //配液杯数量不一致
            if (dt_data1.Rows.Count == 0 || dt_data1.Rows.Count != lis_cup.Count)
            {
                i_nRetset = 1;
            }
            if (i_nRetset != 1)
            {
                int i_num = 0;
                foreach (DataRow dr in dt_data1.Rows)
                {
                    if (dr["CupNum"].ToString() != lis_cup[i_num].ToString() || dr["Type"].ToString() != lis_type[i_num].ToString())
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        {
                            DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("数据库配液杯信息与配置文件不符，是否重置数据库配液杯信息?(重置数据库配液杯信息请点是，否则请点否)", "配置检查", MessageBoxButtons.YesNo, true);

                            //重置
                            if (dialogResult == DialogResult.Yes)
                            {
                                i_nRetset = 2;
                                break;

                            }
                            //不重置
                            else
                            {
                                return;
                            }
                        }
                        else
                        {
                            DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("The database liquid dispensing cup information does not match the configuration file. Do you want to reset the database liquid dispensing cup information? (Please click Yes to reset the database dispensing cup information, otherwise click No)", "Configuration Check", MessageBoxButtons.YesNo, true);

                            //重置
                            if (dialogResult == DialogResult.Yes)
                            {
                                i_nRetset = 2;
                                break;

                            }
                            //不重置
                            else
                            {
                                return;
                            }
                        }
                    }
                    i_num++;
                }
            }
            if (i_nRetset != 0)
            {
                if (i_nRetset == 1)
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    {
                        DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("数据库配液杯信息与配置文件不符，是否重置数据库配液杯信息?(重置数据库配液杯信息请点是，否则请点否)", "配置检查", MessageBoxButtons.YesNo, true);


                        //
                        if (dialogResult == DialogResult.No)
                        {
                            return;
                        }
                    }
                    else
                    {
                        DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("The database liquid dispensing cup information does not match the configuration file. Do you want to reset the database liquid dispensing cup information? (Please click Yes to reset the database dispensing cup information, otherwise click No)", "Configuration Check", MessageBoxButtons.YesNo, true);


                        //
                        if (dialogResult == DialogResult.No)
                        {
                            return;
                        }
                    }
                }
                //读取配置
                List<string> lis_temp = new List<string>();
                List<string> lis_typee = new List<string>();
                if (Lib_Card.Configure.Parameter.Machine_Area1_Type != 0)
                {
                    for (int i = Lib_Card.Configure.Parameter.Machine_Area1_CupMin; i <= Lib_Card.Configure.Parameter.Machine_Area1_CupMax; i++)
                    {
                        lis_temp.Add(i.ToString());
                        lis_typee.Add(Lib_Card.Configure.Parameter.Machine_Area1_Type.ToString());
                    }
                }
                if (Lib_Card.Configure.Parameter.Machine_Area2_Type != 0)
                {
                    for (int i = Lib_Card.Configure.Parameter.Machine_Area2_CupMin; i <= Lib_Card.Configure.Parameter.Machine_Area2_CupMax; i++)
                    {
                        lis_temp.Add(i.ToString());
                        lis_typee.Add(Lib_Card.Configure.Parameter.Machine_Area2_Type.ToString());
                    }
                }
                if (Lib_Card.Configure.Parameter.Machine_Area3_Type != 0)
                {
                    for (int i = Lib_Card.Configure.Parameter.Machine_Area3_CupMin; i <= Lib_Card.Configure.Parameter.Machine_Area3_CupMax; i++)
                    {
                        lis_temp.Add(i.ToString());
                        lis_typee.Add(Lib_Card.Configure.Parameter.Machine_Area3_Type.ToString());
                    }
                }
                if (Lib_Card.Configure.Parameter.Machine_Area4_Type != 0)
                {
                    for (int i = Lib_Card.Configure.Parameter.Machine_Area4_CupMin; i <= Lib_Card.Configure.Parameter.Machine_Area4_CupMax; i++)
                    {
                        lis_temp.Add(i.ToString());
                        lis_typee.Add(Lib_Card.Configure.Parameter.Machine_Area4_Type.ToString());
                    }
                }
                if (Lib_Card.Configure.Parameter.Machine_Area5_Type != 0)
                {
                    for (int i = Lib_Card.Configure.Parameter.Machine_Area5_CupMin; i <= Lib_Card.Configure.Parameter.Machine_Area5_CupMax; i++)
                    {
                        lis_temp.Add(i.ToString());
                        lis_typee.Add(Lib_Card.Configure.Parameter.Machine_Area5_Type.ToString());
                    }
                }
                if (Lib_Card.Configure.Parameter.Machine_Area6_Type != 0)
                {
                    for (int i = Lib_Card.Configure.Parameter.Machine_Area6_CupMin; i <= Lib_Card.Configure.Parameter.Machine_Area6_CupMax; i++)
                    {
                        lis_temp.Add(i.ToString());
                        lis_typee.Add(Lib_Card.Configure.Parameter.Machine_Area6_Type.ToString());
                    }
                }

                //for (int i = 1; i < lis_temp.Count; i++)
                //{
                //    if (Convert.ToInt32(lis_temp[i]) - Convert.ToInt32(lis_temp[i - 1]) != 1)
                //    {
                //        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                //            FADM_Form.CustomMessageBox.Show("配置文件异常，请确保文件正确！", "登录异常",
                //       MessageBoxButtons.OK, false);
                //        else
                //            FADM_Form.CustomMessageBox.Show("Configuration file exception, please ensure the file is correct！", "Login exception",
                //       MessageBoxButtons.OK, false);
                //        return;
                //    }
                //}
                //删除数据库表信息
                string s_sql = "delete from cup_details ; ";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                for (int i = 0; i < lis_temp.Count; i++)
                {
                    s_sql = " insert into cup_details(CupNum,IsFixed,Enable,IsUsing,Type,Statues,CoverStatus)values(" + lis_temp[i] + ",0,1,0," + lis_typee[i] + ",'待机',1 );";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                }
                return;
            }

            if (string.IsNullOrEmpty(TxtName.Text) || string.IsNullOrEmpty(TxtPassword.Text))
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    FADM_Form.CustomMessageBox.Show("用户名或密码不能为空！", "登录异常", MessageBoxButtons.OK, false);
                else
                    FADM_Form.CustomMessageBox.Show("The username or password cannot be empty！", "Login exception", MessageBoxButtons.OK, false);

            }
            else
            {
                if (TxtName.Text == "123" && TxtPassword.Text == "123")
                    this.DialogResult = DialogResult.OK;
                else
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show("用户名或密码错误！", "登录异常", MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("Incorrect username or password！", "Login exception", MessageBoxButtons.OK, false);
                    TxtPassword.Text = null;
                }

            }

            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                          "UPDATE dye_details SET Cooperate = 0 WHERE Cooperate in(5,6,7,8,9) ;");
            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                          "UPDATE cup_details SET Cooperate = 0  ;");
            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                          "UPDATE brew_station_details SET Cooperate = 0  where Cooperate = 11;");
            if (Communal._b_isUseCloth && FADM_Object.Communal.HMIBaClo._b_Connect) {
                //跟称布那里对接下
                //滴料区区域数量
                int dyCount = 0;
                int[] ia_values2 = new int[19];
                //ia_values2[0] 滴料区区域数量
                int cc = 1;
                int cc2 = 11;
                Communal.my_lis_dripCupNum.Clear();
                if (Lib_Card.Configure.Parameter.Machine_Area1_Type == 2)
                {

                    ia_values2[cc] = Lib_Card.Configure.Parameter.Machine_Area1_CupMax - Lib_Card.Configure.Parameter.Machine_Area1_CupMin + 1;
                    ia_values2[cc2] = Lib_Card.Configure.Parameter.Machine_Area1_CupMin;
                    dyCount++;
                    Communal.my_lis_dripCupNum.Add(cc, new List<int>() { cc, ia_values2[cc] });
                    cc = cc + 1;
                    cc2 = cc2 + 1;
                }
                if (Lib_Card.Configure.Parameter.Machine_Area2_Type == 2)
                {
                    ia_values2[cc] = Lib_Card.Configure.Parameter.Machine_Area2_CupMax - Lib_Card.Configure.Parameter.Machine_Area2_CupMin + 1;
                    ia_values2[cc2] = Lib_Card.Configure.Parameter.Machine_Area2_CupMin;
                    Communal.my_lis_dripCupNum.Add(cc, new List<int>() { Communal.my_lis_dripCupNum.ContainsKey(cc - 1) ? Communal.my_lis_dripCupNum[cc - 1][1] + 1 : 1, Communal.my_lis_dripCupNum.ContainsKey(cc - 1) ? Communal.my_lis_dripCupNum[cc - 1][1] + ia_values2[cc] : ia_values2[cc] });
                    cc = cc + 1;
                    cc2 = cc2 + 1;
                    dyCount++;
                }
                if (Lib_Card.Configure.Parameter.Machine_Area3_Type == 2)
                {
                    ia_values2[cc] = Lib_Card.Configure.Parameter.Machine_Area3_CupMax - Lib_Card.Configure.Parameter.Machine_Area3_CupMin + 1;
                    ia_values2[cc2] = Lib_Card.Configure.Parameter.Machine_Area3_CupMin;
                    Communal.my_lis_dripCupNum.Add(cc, new List<int>() { Communal.my_lis_dripCupNum.ContainsKey(cc - 1) ? Communal.my_lis_dripCupNum[cc - 1][1] + 1 : 1, Communal.my_lis_dripCupNum.ContainsKey(cc - 1) ? Communal.my_lis_dripCupNum[cc - 1][1] + ia_values2[cc] : ia_values2[cc] });
                    cc = cc + 1;
                    cc2 = cc2 + 1;
                    dyCount++;
                }
                if (Lib_Card.Configure.Parameter.Machine_Area4_Type == 2)
                {
                    ia_values2[cc] = Lib_Card.Configure.Parameter.Machine_Area4_CupMax - Lib_Card.Configure.Parameter.Machine_Area4_CupMin + 1;
                    ia_values2[cc2] = Lib_Card.Configure.Parameter.Machine_Area4_CupMin;
                    Communal.my_lis_dripCupNum.Add(cc, new List<int>() { Communal.my_lis_dripCupNum.ContainsKey(cc - 1) ? Communal.my_lis_dripCupNum[cc - 1][1] + 1 : 1, Communal.my_lis_dripCupNum.ContainsKey(cc - 1) ? Communal.my_lis_dripCupNum[cc - 1][1] + ia_values2[cc] : ia_values2[cc] });
                    cc = cc + 1;
                    cc2 = cc2 + 1;
                    dyCount++;
                }
                if (Lib_Card.Configure.Parameter.Machine_Area5_Type == 2)
                {
                    ia_values2[cc] = Lib_Card.Configure.Parameter.Machine_Area5_CupMax - Lib_Card.Configure.Parameter.Machine_Area5_CupMin + 1;
                    ia_values2[cc2] = Lib_Card.Configure.Parameter.Machine_Area5_CupMin;
                    Communal.my_lis_dripCupNum.Add(cc, new List<int>() { Communal.my_lis_dripCupNum.ContainsKey(cc - 1) ? Communal.my_lis_dripCupNum[cc - 1][1] + 1 : 1, Communal.my_lis_dripCupNum.ContainsKey(cc - 1) ? Communal.my_lis_dripCupNum[cc - 1][1] + ia_values2[cc] : ia_values2[cc] });
                    cc = cc + 1;
                    cc2 = cc2 + 1;
                    dyCount++;
                }
                if (Lib_Card.Configure.Parameter.Machine_Area6_Type == 2)
                {
                    ia_values2[cc] = Lib_Card.Configure.Parameter.Machine_Area6_CupMax - Lib_Card.Configure.Parameter.Machine_Area6_CupMin + 1;
                    ia_values2[cc2] = Lib_Card.Configure.Parameter.Machine_Area6_CupMin;
                    Communal.my_lis_dripCupNum.Add(cc, new List<int>() { Communal.my_lis_dripCupNum.ContainsKey(cc - 1) ? Communal.my_lis_dripCupNum[cc - 1][1] + 1 : 1, Communal.my_lis_dripCupNum.ContainsKey(cc - 1) ? Communal.my_lis_dripCupNum[cc - 1][1] + ia_values2[cc] : ia_values2[cc] });
                    cc = cc + 1;
                    cc2 = cc2 + 1;
                    dyCount++;
                }
                ia_values2[0] = dyCount; //滴料区区域数量
                int statte = FADM_Object.Communal.HMIBaClo.Write(10000 + 10100 - 1, ia_values2);
                if (statte == -1)
                {
                    FADM_Form.CustomMessageBox.Show("写入滴液区配置参数表失败", "温馨提示", MessageBoxButtons.OK, false);
                }
            }

            string s_path = Environment.CurrentDirectory + "\\Config\\Config.ini";
            string s_balanceInDrip = Lib_File.Ini.GetIni("Setting", "IsBalanceInDrip", "1", s_path);

            string s_isHaveAutoBrew = Lib_File.Ini.GetIni("Setting", "IsHaveAutoBrew", "0", s_path);
            if (s_isHaveAutoBrew == "1")
            {
                FADM_Object.Communal._b_isHaveAutoBrew = true;
            }

            if (FADM_Object.Communal._b_isHaveAutoBrew)
            {
                //先把数据读出来，把交互请求清空
                //读取自动开料数据
                int[] ia_array_Brew = new int[1];
                int i_state_Brew = FADM_Object.Communal._tcpModBus.Read(22289 , 1, ref ia_array_Brew);
                if (i_state_Brew != -1)
                {
                    if (ia_array_Brew[0] == 4 || ia_array_Brew[0] == 5|| ia_array_Brew[0] == 6)
                    {
                        int[] ii = new int[1];
                        ii[0] = 3;
                        FADM_Object.Communal._tcpModBus.Write(22289, ii);
                    }
                }

                i_state_Brew = FADM_Object.Communal._tcpModBus.Read(22289+40, 1, ref ia_array_Brew);
                if (i_state_Brew != -1)
                {
                    if (ia_array_Brew[0] == 4 || ia_array_Brew[0] == 5 || ia_array_Brew[0] == 6)
                    {
                        int[] ii = new int[1];
                        ii[0] = 3;
                        FADM_Object.Communal._tcpModBus.Write(22289+40, ii);
                    }
                }

                i_state_Brew = FADM_Object.Communal._tcpModBus.Read(22289 + 40*2, 1, ref ia_array_Brew);
                if (i_state_Brew != -1)
                {
                    if (ia_array_Brew[0] == 4 || ia_array_Brew[0] == 5 || ia_array_Brew[0] == 6)
                    {
                        int[] ii = new int[1];
                        ii[0] = 3;
                        FADM_Object.Communal._tcpModBus.Write(22289 + 40*2, ii);
                    }
                }

                i_state_Brew = FADM_Object.Communal._tcpModBus.Read(22289 + 40*3, 1, ref ia_array_Brew);
                if (i_state_Brew != -1)
                {
                    if (ia_array_Brew[0] == 4 || ia_array_Brew[0] == 5 || ia_array_Brew[0] == 6)
                    {
                        int[] ii = new int[1];
                        ii[0] = 3;
                        FADM_Object.Communal._tcpModBus.Write(22289 + 40*3, ii);
                    }
                }

                i_state_Brew = FADM_Object.Communal._tcpModBus.Read(22289 + 40*4, 1, ref ia_array_Brew);
                if (i_state_Brew != -1)
                {
                    if (ia_array_Brew[0] == 4 || ia_array_Brew[0] == 5 || ia_array_Brew[0] == 6)
                    {
                        int[] ii = new int[1];
                        ii[0] = 3;
                        FADM_Object.Communal._tcpModBus.Write(22289 + 40*4, ii);
                    }
                }
            }
        }

        private void Login_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    BtnLogOn_Click(sender, e);
                    break;
                default:
                    break;
            }
        }

        //读取云端数据库表字段
        private void verifyTableSuccess(object oo)
        {
            try
            {
                string s_path = Environment.CurrentDirectory + "\\DataBase.json";
                if (!File.Exists(s_path))
                {
                    FileStream fs = File.Create(s_path);
                    fs.Close();
                }
                string content = File.ReadAllText(s_path);
                JObject oldobj = null;
                if (content.Length > 0)
                {
                    oldobj = JObject.Parse(content);
                }
                JObject obj = null;
                Boolean isNetWork = true;
                IDictionary<string, string> dic_parameters = new Dictionary<string, string>();
                try
                {
                    HttpWebResponse response = HttpUtil.CreatePostHttpResponse(FADM_Object.Communal.URL+"/outer/product/getDyDataTC", dic_parameters, 15000, null, null);
                    Stream st = response.GetResponseStream();
                    StreamReader reader = new StreamReader(st);
                    string s_msg = reader.ReadToEnd();
                    obj = JObject.Parse(s_msg);
                    obj = JObject.Parse((string)obj["msg"]);
                }
                catch (Exception ex)
                {
                    isNetWork = false;
                    if (oldobj == null)
                    {
                        return;
                    }
                    obj = oldobj;
                }

                string updateTime = (string)obj["updateTime"]; //最后一次更新的时间
                string oldupdateTime = "";
                if (oldobj != null)
                {
                    oldupdateTime = (string)oldobj["updateTime"]; //最后一次更新的时间
                }
                if (!isNetWork && ((string)oldobj["verifySuccess"]).Equals("0"))
                {
                    disJson(obj, oo);
                }
                else
                {

                    if (updateTime != oldupdateTime)
                    {
                        disJson(obj, oo);
                    }
                }
            }
            catch (WebException ex)
            {

                //没网读取本地配置文件吧
                Console.WriteLine(ex.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void disJson(JObject obj, Object oo)
        {

            Console.WriteLine(DateTime.Now);
            JArray array = (JArray)obj["tableList"];
            foreach (JObject o in array)
            {
                string key = o.Properties().Select(p => p.Name).FirstOrDefault();
                JArray array2 = (JArray)o[key];
                string columnStr = "";
                List<string> list = new List<string>();
                foreach (JObject o2 in array2)
                {
                    string key2 = o2.Properties().Select(p => p.Name).FirstOrDefault();
                    string value = (string)o2[key2];
                    columnStr += key2.Replace("[", "").Replace("]", "") + " " + value.Replace("[", "").Replace("]", "");
                    list.Add(key2.Replace("[", "").Replace("]", "") + "-" + value.Replace("[", "").Replace("]", ""));
                    columnStr += ",";
                }
                columnStr = columnStr.Substring(0, columnStr.Length - 1);
                Lib_DataBank.SQLServer.SQLServerCon sQLServerCon = (Lib_DataBank.SQLServer.SQLServerCon)oo;
                //DatabaseUpdater databaseUpdater = new DatabaseUpdater(oo.ToString());
                DatabaseUpdater databaseUpdater = new DatabaseUpdater(sQLServerCon);
                databaseUpdater.CreateTableIfNotExists(key, columnStr); //表是否存在
                foreach (string columnS in list)
                {
                    if (columnS.Contains("PRIMARY KEY"))
                    {
                        continue;
                    }
                    string[] array3 = columnS.Split('-');
                    databaseUpdater.AddColumnIfNotExists(key, array3[0], array3[1]); //表是否存在
                    //Console.WriteLine(columnS);
                    Thread.Sleep(3);
                }
            }
            obj["verifySuccess"] = "1";
            Console.WriteLine("=======数据库字段全部完成");
            Console.WriteLine(DateTime.Now);
            File.WriteAllText(Environment.CurrentDirectory + "\\DataBase.json", obj.ToString());
        }
    }
}
