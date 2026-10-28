namespace Lib_Card.Configure
{
    public class Parameter
    {
        #region 机台参数

        /// <summary>
        /// 母液瓶总数
        /// </summary>
        public static int Machine_Bottle_Total { get; set; }

        /// <summary>
        /// 母液瓶列数
        /// </summary>
        public static int Machine_Bottle_Column { get; set; }

        /// <summary>
        /// 配液杯总数
        /// </summary>
        public static int Machine_Cup_Total { get; set; }

        /// <summary>
        /// 打板机布局(横放一般可放6组打板机)
        /// 0：竖放
        /// 1：横放
        /// </summary>
        public static int Machine_Cup_Layout { get; set; }

        /// <summary>
        /// 是否两个泄压
        /// 0：1个泄压
        /// 1：2个泄压
        /// </summary>
        public static int Machine_Decompression { get; set; }

        /// <summary>
        /// 开料机类型
        /// 0：触摸屏
        /// 1：PLC
        /// 2：威纶
        /// 3：台达
        /// </summary>
        public static int Machine_Opening_Type { get; set; }

        /// <summary>
        /// 是否单双气缸
        /// 0：单气缸
        /// 1：双气缸
        /// </summary>
        public static int Machine_CylinderVersion { get; set; }

        /// <summary>
        /// 屏蔽针筒感应器
        /// 0：不屏蔽
        /// 1：屏蔽
        /// </summary>
        public static int Machine_isSyringe { get; set; }

        /// <summary>
        /// 天平类型
        /// 0：梅特勒
        /// 1：日本新光
        /// 2：AMD
        /// </summary>
        public static int Machine_BalanceType { get; set; }

        /// <summary>
        /// 天平是否千分位
        /// 0：不是
        /// 1：是
        /// </summary>
        public static int Machine_IsThousandsBalance { get; set; }

        /// <summary>
        /// 报急停或前光幕
        /// 0：急停
        /// 1：前光幕
        /// </summary>
        public static int Machine_IsStopOrFront { get; set; }

        /// <summary>
        /// 报左门或前光幕
        /// 0：左门
        /// 1：左光幕
        /// </summary>
        public static int Machine_IsLeftDoor { get; set; }

        /// <summary>
        /// 报右门或前光幕
        /// 0：右门
        /// 1：右光幕
        /// </summary>
        public static int Machine_IsRightDoor { get; set; }

        /// <summary>
        /// 抓手单双中继
        /// 0：单
        /// 1：双
        /// </summary>
        public static int Machine_TongsVersion { get; set; }

        /// <summary>
        /// 搅拌配置常开点
        /// 0：闭
        /// 1：开
        /// </summary>
        public static int Machine_BlenderVersion { get; set; }

        /// <summary>
        /// 是否气缸中
        /// 0：否
        /// 1：是
        /// </summary>
        public static int Machine_MidCylinder { get; set; }

        /// <summary>
        /// 阻挡气缸配置
        /// 0：单线圈
        /// 1：双线圈
        /// </summary>
        public static int Machine_BlockCylinder { get; set; }

        /// <summary>
        /// Z轴电机配置配置
        /// 0：步进
        /// 1：伺服
        /// </summary>
        public static int Machine_ZType { get; set; }

        /// <summary>
        /// 放杯盖时是否打开失能
        /// 0：不打开
        /// 1：打开
        /// </summary>
        public static int Machine_CloseCoverType { get; set; }

        /// <summary>
        /// 干布区域列数
        /// </summary>
        public static int Machine_AreaDryCloth_Col { get; set; }

        /// <summary>
        /// 湿布区域列数
        /// </summary>
        public static int Machine_AreaWetCloth_Col { get; set; }

        /// <summary>
        /// 是否使用后光幕
        /// 0：不使用
        /// 1：使用
        /// </summary>
        public static int Machine_UseBack { get; set; }

        /// <summary>
        /// 是否使用气缸慢速中
        /// 0：不使用
        /// 1：使用
        /// </summary>
        public static int Machine_SlopDown { get; set; }

        /// <summary>
        /// 滴液机类型
        /// 0：板卡版
        /// 1：PLC版
        /// </summary>
        public static int Machine_Type  { get; set; }


        /// <summary>
        /// 滴液机电机类型
        /// 0：富士
        /// 1：绿维
        /// </summary>
        public static int Machine_Type_Lv { get; set; }

      

        /// <summary>
        /// 区域1类型
        /// 0：无
        /// 1：前处理区
        /// 2：滴液区
        /// 3：打板区
        /// </summary>
        public static int Machine_Area1_Type { get; set; }

        /// <summary>
        /// 打板机类型
        /// 0：转子机
        /// 1：6杯摇摆机
        /// 2：12杯摇摆机
        /// 3：4杯摇摆机
        /// 4：16杯摇摆机
        /// 5：10杯摇摆机
        /// </summary>
        public static int Machine_Area1_DyeType { get; set; }

        /// <summary>
        /// 区域1行数
        /// </summary>
        public static int Machine_Area1_Row { get; set; }


        
        /// <summary>
        /// 打板机滴液区布局(横放一般可放6组打板机)
        /// 1：竖放
        /// 0：横放
        /// </summary>
        public static int Machine_Area1_Layout { get; set; }

        /// <summary>
        /// 区域1列数
        /// </summary>
        public static int Machine_Area1_Col { get; set; }

        /// <summary>
        /// 区域1最小杯号
        /// </summary>
        public static int Machine_Area1_CupMin { get; set; }

        /// <summary>
        /// 区域1最大杯号
        /// </summary>
        public static int Machine_Area1_CupMax { get; set; }

        /// <summary>
        /// 区域1是否大杯子 1:是  0:否
        /// </summary>
        public static int Machine_Area1_Big { get; set; }

        /// <summary>
        /// 区域2类型
        /// 0：无
        /// 1：前处理区
        /// 2：滴液区
        /// 3：打板区
        /// </summary>
        public static int Machine_Area2_Type { get; set; }

        /// <summary>
        /// 打板机类型
        /// 0：转子机
        /// 1：6杯摇摆机
        /// 2：12杯摇摆机
        /// 3：4杯摇摆机
        /// 4：16杯摇摆机
        /// 5：10杯摇摆机
        /// </summary>
        public static int Machine_Area2_DyeType { get; set; }

        /// <summary>
        /// 区域2行数
        /// </summary>
        public static int Machine_Area2_Row { get; set; }

        /// <summary>
        /// 区域2列数
        /// </summary>
        public static int Machine_Area2_Col { get; set; }

        /// <summary>
        /// 打板机滴液区布局(横放一般可放6组打板机)
        /// 1：竖放
        /// 0：横放
        /// </summary>
        public static int Machine_Area2_Layout { get; set; }

        /// <summary>
        /// 区域2最小杯号
        /// </summary>
        public static int Machine_Area2_CupMin { get; set; }

        /// <summary>
        /// 区域2最大杯号
        /// </summary>
        public static int Machine_Area2_CupMax { get; set; }

        /// <summary>
        /// 区域2是否大杯子 1:是  0:否
        /// </summary>
        public static int Machine_Area2_Big { get; set; }

        /// <summary>
        /// 区域3类型
        /// 0：无
        /// 1：前处理区
        /// 2：滴液区
        /// 3：打板区 
        /// </summary>
        public static int Machine_Area3_Type { get; set; }

        /// <summary>
        /// 打板机类型
        /// 0：转子机
        /// 1：6杯摇摆机
        /// 2：12杯摇摆机
        /// 3：4杯摇摆机
        /// 4：16杯摇摆机
        /// 5：10杯摇摆机
        /// </summary>
        public static int Machine_Area3_DyeType { get; set; }

        /// <summary>
        /// 区域3行数
        /// </summary>
        public static int Machine_Area3_Row { get; set; }

        /// <summary>
        /// 区域3列数
        /// </summary>
        public static int Machine_Area3_Col { get; set; }

        /// <summary>
        /// 打板机滴液区布局(横放一般可放6组打板机)
        /// 1：竖放
        /// 0：横放
        /// </summary>
        public static int Machine_Area3_Layout { get; set; }

        /// <summary>
        /// 区域3最小杯号
        /// </summary>
        public static int Machine_Area3_CupMin { get; set; }

        /// <summary>
        /// 区域3最大杯号
        /// </summary>
        public static int Machine_Area3_CupMax { get; set; }

        /// <summary>
        /// 区域3是否大杯子 1:是  0:否
        /// </summary>
        public static int Machine_Area3_Big { get; set; }

        /// <summary>
        /// 区域4类型
        /// 0：无
        /// 1：前处理区
        /// 2：滴液区
        /// 3：打板区
        /// </summary>
        public static int Machine_Area4_Type { get; set; }

        /// <summary>
        /// 打板机类型
        /// 0：转子机
        /// 1：6杯摇摆机
        /// 2：12杯摇摆机
        /// 3：4杯摇摆机
        /// 4：16杯摇摆机
        /// 5：10杯摇摆机
        /// </summary>
        public static int Machine_Area4_DyeType { get; set; }

        /// <summary>
        /// 区域4行数
        /// </summary>
        public static int Machine_Area4_Row { get; set; }

        /// <summary>
        /// 区域4列数
        /// </summary>
        public static int Machine_Area4_Col { get; set; }

        /// <summary>
        /// 打板机滴液区布局(横放一般可放6组打板机)
        /// 1：竖放
        /// 0：横放
        /// </summary>
        public static int Machine_Area4_Layout { get; set; }

        /// <summary>
        /// 区域4最小杯号
        /// </summary>
        public static int Machine_Area4_CupMin { get; set; }

        /// <summary>
        /// 区域4最大杯号
        /// </summary>
        public static int Machine_Area4_CupMax { get; set; }

        /// <summary>
        /// 区域4是否大杯子 1:是  0:否
        /// </summary>
        public static int Machine_Area4_Big { get; set; }

        /// <summary>
        /// 区域5类型
        /// 0：无
        /// 1：前处理区
        /// 2：滴液区
        /// 3：打板区
        /// </summary>
        public static int Machine_Area5_Type { get; set; }

        /// <summary>
        /// 打板机类型
        /// 0：转子机
        /// 1：6杯摇摆机
        /// 2：12杯摇摆机
        /// 3：4杯摇摆机
        /// 4：16杯摇摆机
        /// 5：10杯摇摆机
        /// </summary>
        public static int Machine_Area5_DyeType { get; set; }

        /// <summary>
        /// 区域5行数
        /// </summary>
        public static int Machine_Area5_Row { get; set; }

        /// <summary>
        /// 区域5列数
        /// </summary>
        public static int Machine_Area5_Col { get; set; }

        /// <summary>
        /// 打板机滴液区布局(横放一般可放6组打板机)
        /// 1：竖放
        /// 0：横放
        /// </summary>
        public static int Machine_Area5_Layout { get; set; }

        /// <summary>
        /// 区域5最小杯号
        /// </summary>
        public static int Machine_Area5_CupMin { get; set; }

        /// <summary>
        /// 区域5最大杯号
        /// </summary>
        public static int Machine_Area5_CupMax { get; set; }

        /// <summary>
        /// 区域5是否大杯子 1:是  0:否
        /// </summary>
        public static int Machine_Area5_Big { get; set; }

        /// <summary>
        /// 区域6类型
        /// 0：无
        /// 1：前处理区
        /// 2：滴液区
        /// 3：打板区
        /// </summary>
        public static int Machine_Area6_Type { get; set; }

        /// <summary>
        /// 打板机类型
        /// 0：转子机
        /// 1：6杯摇摆机
        /// 2：12杯摇摆机
        /// 3：4杯摇摆机
        /// 4：16杯摇摆机
        /// 5：10杯摇摆机
        /// </summary>
        public static int Machine_Area6_DyeType { get; set; }

        /// <summary>
        /// 区域6行数
        /// </summary>
        public static int Machine_Area6_Row { get; set; }

        /// <summary>
        /// 区域6列数
        /// </summary>
        public static int Machine_Area6_Col { get; set; }

        /// <summary>
        /// 打板机滴液区布局(横放一般可放6组打板机)
        /// 1：竖放
        /// 0：横放
        /// </summary>
        public static int Machine_Area6_Layout { get; set; }

        /// <summary>
        /// 区域6最小杯号
        /// </summary>
        public static int Machine_Area6_CupMin { get; set; }

        /// <summary>
        /// 区域6最大杯号
        /// </summary>
        public static int Machine_Area6_CupMax { get; set; }

        /// <summary>
        /// 区域6是否大杯子 1:是  0:否
        /// </summary>
        public static int Machine_Area6_Big { get; set; }


        /// <summary>
        /// 干布区域1类型
        /// 0：无
        /// 1：有
        /// </summary>
        public static int Machine_AreaDryCloth1_Type { get; set; }


        /// <summary>
        /// 干布区域1行数
        /// </summary>
        public static int Machine_AreaDryCloth1_Row { get; set; }
        

        /// <summary>
        /// 干布区域1最小杯号
        /// </summary>
        public static int Machine_AreaDryCloth1_CupMin { get; set; }

        /// <summary>
        /// 干布区域1最大杯号
        /// </summary>
        public static int Machine_AreaDryCloth1_CupMax { get; set; }


        /// <summary>
        /// 干布区域2类型
        /// 0：无
        /// 1：有
        /// </summary>
        public static int Machine_AreaDryCloth2_Type { get; set; }


        /// <summary>
        /// 干布区域2行数
        /// </summary>
        public static int Machine_AreaDryCloth2_Row { get; set; }


        /// <summary>
        /// 干布区域2最小杯号
        /// </summary>
        public static int Machine_AreaDryCloth2_CupMin { get; set; }

        /// <summary>
        /// 干布区域2最大杯号
        /// </summary>
        public static int Machine_AreaDryCloth2_CupMax { get; set; }

        /// <summary>
        /// 干布区域3类型
        /// 0：无
        /// 1：有
        /// </summary>
        public static int Machine_AreaDryCloth3_Type { get; set; }


        /// <summary>
        /// 干布区域3行数
        /// </summary>
        public static int Machine_AreaDryCloth3_Row { get; set; }


        /// <summary>
        /// 干布区域3最小杯号
        /// </summary>
        public static int Machine_AreaDryCloth3_CupMin { get; set; }

        /// <summary>
        /// 干布区域3最大杯号
        /// </summary>
        public static int Machine_AreaDryCloth3_CupMax { get; set; }

        /// <summary>
        /// 湿布区域1类型
        /// 0：无
        /// 1：有
        /// </summary>
        public static int Machine_AreaWetCloth1_Type { get; set; }


        /// <summary>
        /// 湿布区域1行数
        /// </summary>
        public static int Machine_AreaWetCloth1_Row { get; set; }


        /// <summary>
        /// 湿布区域1最小杯号
        /// </summary>
        public static int Machine_AreaWetCloth1_CupMin { get; set; }

        /// <summary>
        /// 湿布区域1最大杯号
        /// </summary>
        public static int Machine_AreaWetCloth1_CupMax { get; set; }

        /// <summary>
        /// 湿布区域2类型
        /// 0：无
        /// 1：有
        /// </summary>
        public static int Machine_AreaWetCloth2_Type { get; set; }


        /// <summary>
        /// 湿布区域2行数
        /// </summary>
        public static int Machine_AreaWetCloth2_Row { get; set; }


        /// <summary>
        /// 湿布区域2最小杯号
        /// </summary>
        public static int Machine_AreaWetCloth2_CupMin { get; set; }

        /// <summary>
        /// 湿布区域2最大杯号
        /// </summary>
        public static int Machine_AreaWetCloth2_CupMax { get; set; }

        /// <summary>
        /// 湿布区域3类型
        /// 0：无
        /// 1：有
        /// </summary>
        public static int Machine_AreaWetCloth3_Type { get; set; }


        /// <summary>
        /// 湿布区域3行数
        /// </summary>
        public static int Machine_AreaWetCloth3_Row { get; set; }


        /// <summary>
        /// 湿布区域3最小杯号
        /// </summary>
        public static int Machine_AreaWetCloth3_CupMin { get; set; }

        /// <summary>
        /// 湿布区域3最大杯号
        /// </summary>
        public static int Machine_AreaWetCloth3_CupMax { get; set; }

        /// <summary>
        /// 测PH区域1类型
        /// 0：无
        /// 1：有
        /// </summary>
        public static int Machine_TestPH1_Type { get; set; }


        /// <summary>
        /// 测PH区域1行数
        /// </summary>
        public static int Machine_TestPH1_Row { get; set; }


        /// <summary>
        /// 测PH区域1最小杯号
        /// </summary>
        public static int Machine_TestPH1_CupMin { get; set; }

        /// <summary>
        /// 测PH区域1最大杯号
        /// </summary>
        public static int Machine_TestPH1_CupMax { get; set; }

        /// <summary>
        /// 测PH区域2类型
        /// 0：无
        /// 1：有
        /// </summary>
        public static int Machine_TestPH2_Type { get; set; }


        /// <summary>
        /// 测PH区域2行数
        /// </summary>
        public static int Machine_TestPH2_Row { get; set; }


        /// <summary>
        /// 测PH区域2最小杯号
        /// </summary>
        public static int Machine_TestPH2_CupMin { get; set; }

        /// <summary>
        /// 测PH区域2最大杯号
        /// </summary>
        public static int Machine_TestPH2_CupMax { get; set; }

        /// <summary>
        /// 测PH区域3类型
        /// 0：无
        /// 1：有
        /// </summary>
        public static int Machine_TestPH3_Type { get; set; }


        /// <summary>
        /// 测PH区域3行数
        /// </summary>
        public static int Machine_TestPH3_Row { get; set; }


        /// <summary>
        /// 测PH区域3最小杯号
        /// </summary>
        public static int Machine_TestPH3_CupMin { get; set; }

        /// <summary>
        /// 测PH区域3最大杯号
        /// </summary>
        public static int Machine_TestPH3_CupMax { get; set; }

        #endregion

        #region 回零参数

        /// <summary>
        /// X轴回零起始速度
        /// </summary>
        public static int Home_X_LSpeed { get; set; }

        /// <summary>
        /// X轴回零驱动速度
        /// </summary>
        public static int Home_X_HSpeed { get; set; }

        /// <summary>
        /// X轴回零加速度
        /// </summary>
        public static int Home_X_USpeed { get; set; }

        /// <summary>
        /// X轴回零爬行速度
        /// </summary>
        public static int Home_X_CSpeed { get; set; }

        /// <summary>
        /// X轴回零偏移量
        /// </summary>
        public static int Home_X_Offset { get; set; }

        /// <summary>
        /// Y轴回零起始速度
        /// </summary>
        public static int Home_Y_LSpeed { get; set; }

        /// <summary>
        /// Y轴回零驱动速度
        /// </summary>
        public static int Home_Y_HSpeed { get; set; }

        /// <summary>
        /// Y轴回零加速度
        /// </summary>
        public static int Home_Y_USpeed { get; set; }

        /// <summary>
        /// Y轴回零爬行速度
        /// </summary>
        public static int Home_Y_CSpeed { get; set; }

        /// <summary>
        /// Y轴回零偏移量
        /// </summary>
        public static int Home_Y_Offset { get; set; }

        /// <summary>
        /// Z轴回零起始速度
        /// </summary>
        public static int Home_Z_LSpeed { get; set; }

        /// <summary>
        /// Z轴回零驱动速度
        /// </summary>
        public static int Home_Z_HSpeed { get; set; }

        /// <summary>
        /// Z轴回零加速度
        /// </summary>
        public static int Home_Z_USpeed { get; set; }

        /// <summary>
        /// Z轴回零爬行速度
        /// </summary>
        public static int Home_Z_CSpeed { get; set; }

        /// <summary>
        /// Z轴回零偏移量
        /// </summary>
        public static int Home_Z_Offset { get; set; }

        /// <summary>
        /// 转盘回零起始速度
        /// </summary>
        public static int Home_T_LSpeed { get; set; }

        /// <summary>
        /// 转盘回零驱动速度
        /// </summary>
        public static int Home_T_HSpeed { get; set; }

        /// <summary>
        /// 转盘回零加速度
        /// </summary>
        public static int Home_T_USpeed { get; set; }

        /// <summary>
        /// 转盘回零爬行速度
        /// </summary>
        public static int Home_T_CSpeed { get; set; }

        /// <summary>
        /// 转盘回零偏移量
        /// </summary>
        public static int Home_T_Offset { get; set; }

        #endregion

        #region 运动参数

        /// <summary>
        /// X轴运行起始速度
        /// </summary>
        public static int Move_X_LSpeed { get; set; }

        /// <summary>
        /// X轴运行驱动速度
        /// </summary>
        public static int Move_X_HSpeed { get; set; }

        /// <summary>
        /// X轴运行加减速时间
        /// </summary>
        public static double Move_X_UTime { get; set; }


        /// <summary>
        /// Y轴运行起始速度
        /// </summary>
        public static int Move_Y_LSpeed { get; set; }

        /// <summary>
        /// Y轴运行驱动速度
        /// </summary>
        public static int Move_Y_HSpeed { get; set; }

        /// <summary>
        /// Y轴运行加减速时间
        /// </summary>
        public static double Move_Y_UTime { get; set; }


        /// <summary>
        /// 小针筒运行起始速度
        /// </summary>
        public static int Move_S_LSpeed { get; set; }

        /// <summary>
        /// 小针筒运行驱动速度
        /// </summary>
        public static int Move_S_HSpeed { get; set; }

        /// <summary>
        /// 小针筒运行慢速驱动速度
        /// </summary>
        public static int Move_S_MinHSpeed { get; set; }

        /// <summary>
        /// 小针筒运行加减速时间
        /// </summary>
        public static double Move_S_UTime { get; set; }

        /// <summary>
        /// 大针筒运行起始速度
        /// </summary>
        public static int Move_B_LSpeed { get; set; }

        /// <summary>
        /// 大针筒运行驱动速度
        /// </summary>
        public static int Move_B_HSpeed { get; set; }

        /// <summary>
        /// 大针筒运行慢速驱动速度
        /// </summary>
        public static int Move_B_MinHSpeed { get; set; }

        /// <summary>
        /// 大针筒运行加减速时间
        /// </summary>
        public static double Move_B_UTime { get; set; }

        /// <summary>
        /// 转盘运行起始速度
        /// </summary>
        public static int Move_T_LSpeed { get; set; }

        /// <summary>
        /// 转盘运行驱动速度
        /// </summary>
        public static int Move_T_HSpeed { get; set; }

        /// <summary>
        /// 转盘运行加减速时间
        /// </summary>
        public static double Move_T_UTime { get; set; }

        #endregion

        #region 坐标参数

        /// <summary>
        /// 首母液瓶X轴坐标
        /// </summary>
        public static int Coordinate_Bottle_X { get; set; }

        /// <summary>
        /// 首母液瓶Y轴坐标
        /// </summary>
        public static int Coordinate_Bottle_Y { get; set; }

        /// <summary>
        /// 首母液瓶X轴坐标 (如果粉罐和母液区分两个区域，这个就是第一个母液瓶坐标) 
        /// </summary>
        public static int Coordinate_MotherBottle_X { get; set; }

        /// <summary>
        /// 首母液瓶Y轴坐标 (如果粉罐和母液区分两个区域，这个就是第一个母液瓶坐标) 
        /// </summary>
        public static int Coordinate_MotherBottle_Y { get; set; }

        /// <summary>
        /// 母液瓶间隔坐标
        /// </summary>
        public static int Coordinate_Bottle_Interval { get; set; }

        /// <summary>
        /// 区域1最小配液杯X轴坐标
        /// </summary>
        public static int Coordinate_Area1_X { get; set; }

        /// <summary>
        /// 区域1最小配液杯Y轴坐标
        /// </summary>
        public static int Coordinate_Area1_Y { get; set; }

        /// <summary>
        /// 区域1X轴间隔
        /// </summary>
        public static int Coordinate_Area1_IntervalX { get; set; }

        /// <summary>
        /// 区域1Y轴间隔
        /// </summary>
        public static int Coordinate_Area1_IntervalY { get; set; }

        /// <summary>
        /// 区域2最小配液杯X轴坐标
        /// </summary>
        public static int Coordinate_Area2_X { get; set; }

        /// <summary>
        /// 区域2最小配液杯Y轴坐标
        /// </summary>
        public static int Coordinate_Area2_Y { get; set; }

        /// <summary>
        /// 区域2X轴间隔
        /// </summary>
        public static int Coordinate_Area2_IntervalX { get; set; }

        /// <summary>
        /// 区域2Y轴间隔
        /// </summary>
        public static int Coordinate_Area2_IntervalY { get; set; }

        /// <summary>
        /// 区域3最小配液杯X轴坐标
        /// </summary>
        public static int Coordinate_Area3_X { get; set; }

        /// <summary>
        /// 区域3最小配液杯Y轴坐标
        /// </summary>
        public static int Coordinate_Area3_Y { get; set; }

        /// <summary>
        /// 区域3X轴间隔
        /// </summary>
        public static int Coordinate_Area3_IntervalX { get; set; }

        /// <summary>
        /// 区域3Y轴间隔
        /// </summary>
        public static int Coordinate_Area3_IntervalY { get; set; }

        /// <summary>
        /// 区域4最小配液杯X轴坐标
        /// </summary>
        public static int Coordinate_Area4_X { get; set; }

        /// <summary>
        /// 区域4最小配液杯Y轴坐标
        /// </summary>
        public static int Coordinate_Area4_Y { get; set; }

        /// <summary>
        /// 区域4X轴间隔
        /// </summary>
        public static int Coordinate_Area4_IntervalX { get; set; }

        /// <summary>
        /// 区域4Y轴间隔
        /// </summary>
        public static int Coordinate_Area4_IntervalY { get; set; }

        /// <summary>
        /// 区域5最小配液杯X轴坐标
        /// </summary>
        public static int Coordinate_Area5_X { get; set; }

        /// <summary>
        /// 区域5最小配液杯Y轴坐标
        /// </summary>
        public static int Coordinate_Area5_Y { get; set; }

        /// <summary>
        /// 区域5X轴间隔
        /// </summary>
        public static int Coordinate_Area5_IntervalX { get; set; }

        /// <summary>
        /// 区域5Y轴间隔
        /// </summary>
        public static int Coordinate_Area5_IntervalY { get; set; }

        /// <summary>
        /// 区域6最小配液杯X轴坐标
        /// </summary>
        public static int Coordinate_Area6_X { get; set; }

        /// <summary>
        /// 区域6最小配液杯Y轴坐标
        /// </summary>
        public static int Coordinate_Area6_Y { get; set; }

        /// <summary>
        /// 区域6X轴间隔
        /// </summary>
        public static int Coordinate_Area6_IntervalX { get; set; }

        /// <summary>
        /// 区域6Y轴间隔
        /// </summary>
        public static int Coordinate_Area6_IntervalY { get; set; }

        /// <summary>
        /// 杯盖区域最小配液杯X轴坐标
        /// </summary>
        public static int Coordinate_AreaCover_X { get; set; }

        /// <summary>
        /// 杯盖区域最小配液杯Y轴坐标
        /// </summary>
        public static int Coordinate_AreaCover_Y { get; set; }

        /// <summary>
        /// 杯盖区域X轴间隔
        /// </summary>
        public static int Coordinate_AreaCover_IntervalX { get; set; }

        /// <summary>
        /// 杯盖区域Y轴间隔
        /// </summary>
        public static int Coordinate_AreaCover_IntervalY { get; set; }

        /// <summary>
        /// 干布区域最小X轴坐标
        /// </summary>
        public static int Coordinate_AreaDryCloth_X { get; set; }

        /// <summary>
        /// 干布区域最小Y轴坐标
        /// </summary>
        public static int Coordinate_AreaDryCloth_Y { get; set; }

        /// <summary>
        /// 干布区域X轴间隔
        /// </summary>
        public static int Coordinate_AreaDryCloth_IntervalX { get; set; }

        /// <summary>
        /// 干布区域Y轴间隔
        /// </summary>
        public static int Coordinate_AreaDryCloth_IntervalY { get; set; }

        /// <summary>
        /// 湿布区域最小X轴坐标
        /// </summary>
        public static int Coordinate_AreaWetCloth_X { get; set; }

        /// <summary>
        /// 湿布区域最小Y轴坐标
        /// </summary>
        public static int Coordinate_AreaWetCloth_Y { get; set; }

        /// <summary>
        /// 湿布区域X轴间隔
        /// </summary>
        public static int Coordinate_AreaWetCloth_IntervalX { get; set; }

        /// <summary>
        /// 湿布区域X轴组间间隔
        /// </summary>
        public static int Coordinate_AreaWetCloth_IntervalX_S { get; set; }


        /// <summary>
        /// 湿布区域Y轴间隔
        /// </summary>
        public static int Coordinate_AreaWetCloth_IntervalY { get; set; }

        /// <summary>
        /// 湿布区域Y轴组间间隔
        /// </summary>
        public static int Coordinate_AreaWetCloth_IntervalY_S { get; set; }


        /// <summary>
        /// 干布夹子X轴坐标
        /// </summary>
        public static int Coordinate_DryClamp_X { get; set; }

        /// <summary>
        /// 干布夹子Y轴坐标
        /// </summary>
        public static int Coordinate_DryClamp_Y { get; set; }

        /// <summary>
        /// 湿布夹子X轴坐标
        /// </summary>
        public static int Coordinate_WetClamp_X { get; set; }

        /// <summary>
        /// 湿布夹子Y轴坐标
        /// </summary>
        public static int Coordinate_WetClamp_Y { get; set; }

        /// <summary>
        /// 抽液针筒X轴坐标
        /// </summary>
        public static int Coordinate_Syringes_X { get; set; }

        /// <summary>
        /// 抽液针筒Y轴坐标
        /// </summary>
        public static int Coordinate_Syringes_Y { get; set; }

        /// <summary>
        /// 吸光度1号杯X轴坐标
        /// </summary>
        public static int Coordinate_Abs1_X { get; set; }

        /// <summary>
        /// 吸光度1号杯Y轴坐标
        /// </summary>
        public static int Coordinate_Abs1_Y { get; set; }

        /// <summary>
        /// 吸光度2号杯X轴坐标
        /// </summary>
        public static int Coordinate_Abs2_X { get; set; }

        /// <summary>
        /// 吸光度2号杯Y轴坐标
        /// </summary>
        public static int Coordinate_Abs2_Y { get; set; }


        /// <summary>
        /// 吸光度3号杯X轴坐标
        /// </summary>
        public static int Coordinate_Abs3_X { get; set; }

        /// <summary>
        /// 吸光度3号杯Y轴坐标
        /// </summary>
        public static int Coordinate_Abs3_Y { get; set; }

        /// <summary>
        /// 吸光度4号杯X轴坐标
        /// </summary>
        public static int Coordinate_Abs4_X { get; set; }

        /// <summary>
        /// 吸光度4号杯Y轴坐标
        /// </summary>
        public static int Coordinate_Abs4_Y { get; set; }


        /// <summary>
        /// 1号杯X坐标
        /// </summary>
        public static int Coordinate_Cup1_IntervalX { get; set; }

        /// <summary>
        /// 1号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup1_IntervalY { get; set; }

        /// <summary>
        /// 2号杯X坐标
        /// </summary>
        public static int Coordinate_Cup2_IntervalX { get; set; }

        /// <summary>
        /// 2号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup2_IntervalY { get; set; }

        /// <summary>
        /// 3号杯X坐标
        /// </summary>
        public static int Coordinate_Cup3_IntervalX { get; set; }

        /// <summary>
        /// 3号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup3_IntervalY { get; set; }

        /// <summary>
        /// 4号杯X坐标
        /// </summary>
        public static int Coordinate_Cup4_IntervalX { get; set; }

        /// <summary>
        /// 4号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup4_IntervalY { get; set; }

        /// <summary>
        /// 5号杯X坐标
        /// </summary>
        public static int Coordinate_Cup5_IntervalX { get; set; }

        /// <summary>
        /// 5号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup5_IntervalY { get; set; }

        /// <summary>
        /// 6号杯X坐标
        /// </summary>
        public static int Coordinate_Cup6_IntervalX { get; set; }

        /// <summary>
        /// 6号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup6_IntervalY { get; set; }

        /// <summary>
        /// 7号杯X坐标
        /// </summary>
        public static int Coordinate_Cup7_IntervalX { get; set; }

        /// <summary>
        /// 7号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup7_IntervalY { get; set; }

        /// <summary>
        /// 8号杯X坐标
        /// </summary>
        public static int Coordinate_Cup8_IntervalX { get; set; }

        /// <summary>
        /// 8号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup8_IntervalY { get; set; }

        /// <summary>
        /// 9号杯X坐标
        /// </summary>
        public static int Coordinate_Cup9_IntervalX { get; set; }

        /// <summary>
        /// 9号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup9_IntervalY { get; set; }

        /// <summary>
        /// 10号杯X坐标
        /// </summary>
        public static int Coordinate_Cup10_IntervalX { get; set; }

        /// <summary>
        /// 10号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup10_IntervalY { get; set; }

        /// <summary>
        /// 11号杯X坐标
        /// </summary>
        public static int Coordinate_Cup11_IntervalX { get; set; }

        /// <summary>
        /// 11号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup11_IntervalY { get; set; }

        /// <summary>
        /// 12号杯X坐标
        /// </summary>
        public static int Coordinate_Cup12_IntervalX { get; set; }

        /// <summary>
        /// 12号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup12_IntervalY { get; set; }

        /// <summary>
        /// 13号杯X坐标
        /// </summary>
        public static int Coordinate_Cup13_IntervalX { get; set; }

        /// <summary>
        /// 13号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup13_IntervalY { get; set; }

        /// <summary>
        /// 14号杯X坐标
        /// </summary>
        public static int Coordinate_Cup14_IntervalX { get; set; }

        /// <summary>
        /// 14号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup14_IntervalY { get; set; }

        /// <summary>
        /// 15号杯X坐标
        /// </summary>
        public static int Coordinate_Cup15_IntervalX { get; set; }

        /// <summary>
        /// 15号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup15_IntervalY { get; set; }

        /// <summary>
        /// 16号杯X坐标
        /// </summary>
        public static int Coordinate_Cup16_IntervalX { get; set; }

        /// <summary>
        /// 16号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup16_IntervalY { get; set; }

        /// <summary>
        /// 17号杯X坐标
        /// </summary>
        public static int Coordinate_Cup17_IntervalX { get; set; }

        /// <summary>
        /// 17号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup17_IntervalY { get; set; }

        /// <summary>
        /// 18号杯X坐标
        /// </summary>
        public static int Coordinate_Cup18_IntervalX { get; set; }

        /// <summary>
        /// 18号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup18_IntervalY { get; set; }

        /// <summary>
        /// 19号杯X坐标
        /// </summary>
        public static int Coordinate_Cup19_IntervalX { get; set; }

        /// <summary>
        /// 19号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup19_IntervalY { get; set; }

        /// <summary>
        /// 20号杯X坐标
        /// </summary>
        public static int Coordinate_Cup20_IntervalX { get; set; }

        /// <summary>
        /// 20号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup20_IntervalY { get; set; }

        /// <summary>
        /// 21号杯X坐标
        /// </summary>
        public static int Coordinate_Cup21_IntervalX { get; set; }

        /// <summary>
        /// 21号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup21_IntervalY { get; set; }

        /// <summary>
        /// 22号杯X坐标
        /// </summary>
        public static int Coordinate_Cup22_IntervalX { get; set; }

        /// <summary>
        /// 22号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup22_IntervalY { get; set; }

        /// <summary>
        /// 23号杯X坐标
        /// </summary>
        public static int Coordinate_Cup23_IntervalX { get; set; }

        /// <summary>
        /// 23号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup23_IntervalY { get; set; }

        /// <summary>
        /// 24号杯X坐标
        /// </summary>
        public static int Coordinate_Cup24_IntervalX { get; set; }

        /// <summary>
        /// 24号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup24_IntervalY { get; set; }

        /// <summary>
        /// 25号杯X坐标
        /// </summary>
        public static int Coordinate_Cup25_IntervalX { get; set; }

        /// <summary>
        /// 25号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup25_IntervalY { get; set; }

        /// <summary>
        /// 26号杯X坐标
        /// </summary>
        public static int Coordinate_Cup26_IntervalX { get; set; }

        /// <summary>
        /// 26号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup26_IntervalY { get; set; }

        /// <summary>
        /// 27号杯X坐标
        /// </summary>
        public static int Coordinate_Cup27_IntervalX { get; set; }

        /// <summary>
        /// 27号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup27_IntervalY { get; set; }

        /// <summary>
        /// 28号杯X坐标
        /// </summary>
        public static int Coordinate_Cup28_IntervalX { get; set; }

        /// <summary>
        /// 28号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup28_IntervalY { get; set; }

        /// <summary>
        /// 29号杯X坐标
        /// </summary>
        public static int Coordinate_Cup29_IntervalX { get; set; }

        /// <summary>
        /// 29号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup29_IntervalY { get; set; }

        /// <summary>
        /// 30号杯X坐标
        /// </summary>
        public static int Coordinate_Cup30_IntervalX { get; set; }

        /// <summary>
        /// 30号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup30_IntervalY { get; set; }

        /// <summary>
        /// 31号杯X坐标
        /// </summary>
        public static int Coordinate_Cup31_IntervalX { get; set; }

        /// <summary>
        /// 31号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup31_IntervalY { get; set; }

        /// <summary>
        /// 32号杯X坐标
        /// </summary>
        public static int Coordinate_Cup32_IntervalX { get; set; }

        /// <summary>
        /// 32号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup32_IntervalY { get; set; }

        /// <summary>
        /// 33号杯X坐标
        /// </summary>
        public static int Coordinate_Cup33_IntervalX { get; set; }

        /// <summary>
        /// 33号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup33_IntervalY { get; set; }

        /// <summary>
        /// 34号杯X坐标
        /// </summary>
        public static int Coordinate_Cup34_IntervalX { get; set; }

        /// <summary>
        /// 34号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup34_IntervalY { get; set; }

        /// <summary>
        /// 35号杯X坐标
        /// </summary>
        public static int Coordinate_Cup35_IntervalX { get; set; }

        /// <summary>
        /// 35号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup35_IntervalY { get; set; }

        /// <summary>
        /// 36号杯X坐标
        /// </summary>
        public static int Coordinate_Cup36_IntervalX { get; set; }

        /// <summary>
        /// 36号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup36_IntervalY { get; set; }

        /// <summary>
        /// 37号杯X坐标
        /// </summary>
        public static int Coordinate_Cup37_IntervalX { get; set; }

        /// <summary>
        /// 37号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup37_IntervalY { get; set; }

        /// <summary>
        /// 38号杯X坐标
        /// </summary>
        public static int Coordinate_Cup38_IntervalX { get; set; }

        /// <summary>
        /// 38号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup38_IntervalY { get; set; }

        /// <summary>
        /// 39号杯X坐标
        /// </summary>
        public static int Coordinate_Cup39_IntervalX { get; set; }

        /// <summary>
        /// 39号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup39_IntervalY { get; set; }

        /// <summary>
        /// 40号杯X坐标
        /// </summary>
        public static int Coordinate_Cup40_IntervalX { get; set; }

        /// <summary>
        /// 40号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup40_IntervalY { get; set; }

        /// <summary>
        /// 41号杯X坐标
        /// </summary>
        public static int Coordinate_Cup41_IntervalX { get; set; }

        /// <summary>
        /// 41号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup41_IntervalY { get; set; }

        /// <summary>
        /// 42号杯X坐标
        /// </summary>
        public static int Coordinate_Cup42_IntervalX { get; set; }

        /// <summary>
        /// 42号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup42_IntervalY { get; set; }

        /// <summary>
        /// 43号杯X坐标
        /// </summary>
        public static int Coordinate_Cup43_IntervalX { get; set; }

        /// <summary>
        /// 43号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup43_IntervalY { get; set; }

        /// <summary>
        /// 44号杯X坐标
        /// </summary>
        public static int Coordinate_Cup44_IntervalX { get; set; }

        /// <summary>
        /// 44号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup44_IntervalY { get; set; }

        /// <summary>
        /// 45号杯X坐标
        /// </summary>
        public static int Coordinate_Cup45_IntervalX { get; set; }

        /// <summary>
        /// 45号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup45_IntervalY { get; set; }

        /// <summary>
        /// 46号杯X坐标
        /// </summary>
        public static int Coordinate_Cup46_IntervalX { get; set; }

        /// <summary>
        /// 46号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup46_IntervalY { get; set; }

        /// <summary>
        /// 47号杯X坐标
        /// </summary>
        public static int Coordinate_Cup47_IntervalX { get; set; }

        /// <summary>
        /// 47号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup47_IntervalY { get; set; }

        /// <summary>
        /// 48号杯X坐标
        /// </summary>
        public static int Coordinate_Cup48_IntervalX { get; set; }

        /// <summary>
        /// 48号杯Y坐标
        /// </summary>
        public static int Coordinate_Cup48_IntervalY { get; set; }



        /// <summary>
        /// 1号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover1_IntervalX { get; set; }

        /// <summary>
        /// 1号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover1_IntervalY { get; set; }

        /// <summary>
        /// 2号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover2_IntervalX { get; set; }

        /// <summary>
        /// 2号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover2_IntervalY { get; set; }

        /// <summary>
        /// 3号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover3_IntervalX { get; set; }

        /// <summary>
        /// 3号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover3_IntervalY { get; set; }

        /// <summary>
        /// 4号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover4_IntervalX { get; set; }

        /// <summary>
        /// 4号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover4_IntervalY { get; set; }

        /// <summary>
        /// 5号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover5_IntervalX { get; set; }

        /// <summary>
        /// 5号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover5_IntervalY { get; set; }

        /// <summary>
        /// 6号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover6_IntervalX { get; set; }

        /// <summary>
        /// 6号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover6_IntervalY { get; set; }

        /// <summary>
        /// 7号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover7_IntervalX { get; set; }


        /// <summary>
        /// 7号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover7_IntervalY { get; set; }

        /// <summary>
        /// 8号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover8_IntervalX { get; set; }

        /// <summary>
        /// 8号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover8_IntervalY { get; set; }

        /// <summary>
        /// 9号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover9_IntervalX { get; set; }

        /// <summary>
        /// 9号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover9_IntervalY { get; set; }

        /// <summary>
        /// 10号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover10_IntervalX { get; set; }

        /// <summary>
        /// 10号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover10_IntervalY { get; set; }

        /// <summary>
        /// 1号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover11_IntervalX { get; set; }

        /// <summary>
        /// 11号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover11_IntervalY { get; set; }

        /// <summary>
        /// 12号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover12_IntervalX { get; set; }

        /// <summary>
        /// 12号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover12_IntervalY { get; set; }

        /// <summary>
        /// 13号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover13_IntervalX { get; set; }

        /// <summary>
        /// 13号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover13_IntervalY { get; set; }

        /// <summary>
        /// 14号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover14_IntervalX { get; set; }

        /// <summary>
        /// 14号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover14_IntervalY { get; set; }

        /// <summary>
        /// 15号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover15_IntervalX { get; set; }

        /// <summary>
        /// 15号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover15_IntervalY { get; set; }

        /// <summary>
        /// 16号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover16_IntervalX { get; set; }

        /// <summary>
        /// 16号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover16_IntervalY { get; set; }

        /// <summary>
        /// 17号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover17_IntervalX { get; set; }


        /// <summary>
        /// 17号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover17_IntervalY { get; set; }

        /// <summary>
        /// 18号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover18_IntervalX { get; set; }

        /// <summary>
        /// 18号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover18_IntervalY { get; set; }

        /// <summary>
        /// 19号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover19_IntervalX { get; set; }

        /// <summary>
        /// 19号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover19_IntervalY { get; set; }

        /// <summary>
        /// 20号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover20_IntervalX { get; set; }

        /// <summary>
        /// 20号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover20_IntervalY { get; set; }

        /// <summary>
        /// 21号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover21_IntervalX { get; set; }

        /// <summary>
        /// 21号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover21_IntervalY { get; set; }

        /// <summary>
        /// 22号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover22_IntervalX { get; set; }

        /// <summary>
        /// 22号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover22_IntervalY { get; set; }

        /// <summary>
        /// 23号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover23_IntervalX { get; set; }

        /// <summary>
        /// 23号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover23_IntervalY { get; set; }

        /// <summary>
        /// 24号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover24_IntervalX { get; set; }

        /// <summary>
        /// 24号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover24_IntervalY { get; set; }

        /// <summary>
        /// 25号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover25_IntervalX { get; set; }

        /// <summary>
        /// 25号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover25_IntervalY { get; set; }

        /// <summary>
        /// 26号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover26_IntervalX { get; set; }

        /// <summary>
        /// 26号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover26_IntervalY { get; set; }

        /// <summary>
        /// 27号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover27_IntervalX { get; set; }


        /// <summary>
        /// 27号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover27_IntervalY { get; set; }

        /// <summary>
        /// 28号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover28_IntervalX { get; set; }

        /// <summary>
        /// 28号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover28_IntervalY { get; set; }

        /// <summary>
        /// 29号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover29_IntervalX { get; set; }

        /// <summary>
        /// 29号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover29_IntervalY { get; set; }

        /// <summary>
        /// 30号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover30_IntervalX { get; set; }

        /// <summary>
        /// 30号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover30_IntervalY { get; set; }

        /// <summary>
        /// 31号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover31_IntervalX { get; set; }

        /// <summary>
        /// 31号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover31_IntervalY { get; set; }

        /// <summary>
        /// 32号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover32_IntervalX { get; set; }

        /// <summary>
        /// 32号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover32_IntervalY { get; set; }

        /// <summary>
        /// 33号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover33_IntervalX { get; set; }

        /// <summary>
        /// 33号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover33_IntervalY { get; set; }

        /// <summary>
        /// 34号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover34_IntervalX { get; set; }

        /// <summary>
        /// 34号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover34_IntervalY { get; set; }

        /// <summary>
        /// 35号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover35_IntervalX { get; set; }

        /// <summary>
        /// 35号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover35_IntervalY { get; set; }

        /// <summary>
        /// 36号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover36_IntervalX { get; set; }

        /// <summary>
        /// 36号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover36_IntervalY { get; set; }

        /// <summary>
        /// 37号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover37_IntervalX { get; set; }


        /// <summary>
        /// 37号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover37_IntervalY { get; set; }

        /// <summary>
        /// 38号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover38_IntervalX { get; set; }

        /// <summary>
        /// 38号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover38_IntervalY { get; set; }

        /// <summary>
        /// 39号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover39_IntervalX { get; set; }

        /// <summary>
        /// 39号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover39_IntervalY { get; set; }

        /// <summary>
        /// 40号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover40_IntervalX { get; set; }

        /// <summary>
        /// 40号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover40_IntervalY { get; set; }

        /// <summary>
        /// 41号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover41_IntervalX { get; set; }

        /// <summary>
        /// 41号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover41_IntervalY { get; set; }

        /// <summary>
        /// 42号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover42_IntervalX { get; set; }

        /// <summary>
        /// 42号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover42_IntervalY { get; set; }

        /// <summary>
        /// 43号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover43_IntervalX { get; set; }

        /// <summary>
        /// 43号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover43_IntervalY { get; set; }

        /// <summary>
        /// 44号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover44_IntervalX { get; set; }

        /// <summary>
        /// 44号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover44_IntervalY { get; set; }

        /// <summary>
        /// 45号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover45_IntervalX { get; set; }

        /// <summary>
        /// 45号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover45_IntervalY { get; set; }

        /// <summary>
        /// 46号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover46_IntervalX { get; set; }

        /// <summary>
        /// 46号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover46_IntervalY { get; set; }

        /// <summary>
        /// 47号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover47_IntervalX { get; set; }


        /// <summary>
        /// 47号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover47_IntervalY { get; set; }

        /// <summary>
        /// 48号杯盖X坐标
        /// </summary>
        public static int Coordinate_CupCover48_IntervalX { get; set; }

        /// <summary>
        /// 48号杯盖Y坐标
        /// </summary>
        public static int Coordinate_CupCover48_IntervalY { get; set; }


        /// <summary>
        /// 干布区域1最小配液杯X轴坐标
        /// </summary>
        public static int Coordinate_AreaDryCloth1_X { get; set; }

        /// <summary>
        /// 干布区域1最小配液杯Y轴坐标
        /// </summary>
        public static int Coordinate_AreaDryCloth1_Y { get; set; }

        /// <summary>
        /// 干布区域1X轴间隔
        /// </summary>
        public static int Coordinate_AreaDryCloth1_IntervalX { get; set; }

        /// <summary>
        /// 干布区域1Y轴间隔
        /// </summary>
        public static int Coordinate_AreaDryCloth1_IntervalY { get; set; }

        /// <summary>
        /// 干布区域2最小配液杯X轴坐标
        /// </summary>
        public static int Coordinate_AreaDryCloth2_X { get; set; }

        /// <summary>
        /// 干布区域2最小配液杯Y轴坐标
        /// </summary>
        public static int Coordinate_AreaDryCloth2_Y { get; set; }

        /// <summary>
        /// 干布区域2X轴间隔
        /// </summary>
        public static int Coordinate_AreaDryCloth2_IntervalX { get; set; }

        /// <summary>
        /// 干布区域2Y轴间隔
        /// </summary>
        public static int Coordinate_AreaDryCloth2_IntervalY { get; set; }

        /// <summary>
        /// 干布区域3最小配液杯X轴坐标
        /// </summary>
        public static int Coordinate_AreaDryCloth3_X { get; set; }

        /// <summary>
        /// 干布区域3最小配液杯Y轴坐标
        /// </summary>
        public static int Coordinate_AreaDryCloth3_Y { get; set; }

        /// <summary>
        /// 干布区域3X轴间隔
        /// </summary>
        public static int Coordinate_AreaDryCloth3_IntervalX { get; set; }

        /// <summary>
        /// 干布区域3Y轴间隔
        /// </summary>
        public static int Coordinate_AreaDryCloth3_IntervalY { get; set; }

        /// <summary>
        /// 湿布区域1最小配液杯X轴坐标
        /// </summary>
        public static int Coordinate_AreaWetCloth1_X { get; set; }

        /// <summary>
        /// 湿布区域1最小配液杯Y轴坐标
        /// </summary>
        public static int Coordinate_AreaWetCloth1_Y { get; set; }

        /// <summary>
        /// 湿布区域1X轴间隔
        /// </summary>
        public static int Coordinate_AreaWetCloth1_IntervalX { get; set; }

        /// <summary>
        /// 湿布区域1Y轴间隔
        /// </summary>
        public static int Coordinate_AreaWetCloth1_IntervalY { get; set; }

        /// <summary>
        /// 湿布区域2最小配液杯X轴坐标
        /// </summary>
        public static int Coordinate_AreaWetCloth2_X { get; set; }

        /// <summary>
        /// 湿布区域2最小配液杯Y轴坐标
        /// </summary>
        public static int Coordinate_AreaWetCloth2_Y { get; set; }

        /// <summary>
        /// 湿布区域2X轴间隔
        /// </summary>
        public static int Coordinate_AreaWetCloth2_IntervalX { get; set; }

        /// <summary>
        /// 湿布区域2Y轴间隔
        /// </summary>
        public static int Coordinate_AreaWetCloth2_IntervalY { get; set; }

        /// <summary>
        /// 湿布区域3最小配液杯X轴坐标
        /// </summary>
        public static int Coordinate_AreaWetCloth3_X { get; set; }

        /// <summary>
        /// 湿布区域3最小配液杯Y轴坐标
        /// </summary>
        public static int Coordinate_AreaWetCloth3_Y { get; set; }

        /// <summary>
        /// 湿布区域3X轴间隔
        /// </summary>
        public static int Coordinate_AreaWetCloth3_IntervalX { get; set; }

        /// <summary>
        /// 湿布区域3Y轴间隔
        /// </summary>
        public static int Coordinate_AreaWetCloth3_IntervalY { get; set; }

        /// <summary>
        /// 测PH区域1最小配液杯X轴坐标
        /// </summary>
        public static int Coordinate_TestPH1_X { get; set; }

        /// <summary>
        /// 测PH区域1最小配液杯Y轴坐标
        /// </summary>
        public static int Coordinate_TestPH1_Y { get; set; }

        /// <summary>
        /// 测PH区域1X轴间隔
        /// </summary>
        public static int Coordinate_TestPH1_IntervalX { get; set; }

        /// <summary>
        /// 测PH区域1Y轴间隔
        /// </summary>
        public static int Coordinate_TestPH1_IntervalY { get; set; }

        /// <summary>
        /// 测PH区域2最小配液杯X轴坐标
        /// </summary>
        public static int Coordinate_TestPH2_X { get; set; }

        /// <summary>
        /// 测PH区域2最小配液杯Y轴坐标
        /// </summary>
        public static int Coordinate_TestPH2_Y { get; set; }

        /// <summary>
        /// 测PH区域2X轴间隔
        /// </summary>
        public static int Coordinate_TestPH2_IntervalX { get; set; }

        /// <summary>
        /// 测PH区域2Y轴间隔
        /// </summary>
        public static int Coordinate_TestPH2_IntervalY { get; set; }

        /// <summary>
        /// 测PH区域3最小配液杯X轴坐标
        /// </summary>
        public static int Coordinate_TestPH3_X { get; set; }

        /// <summary>
        /// 测PH区域3最小配液杯Y轴坐标
        /// </summary>
        public static int Coordinate_TestPH3_Y { get; set; }

        /// <summary>
        /// 测PH区域3X轴间隔
        /// </summary>
        public static int Coordinate_TestPH3_IntervalX { get; set; }

        /// <summary>
        /// 测PH区域3Y轴间隔
        /// </summary>
        public static int Coordinate_TestPH3_IntervalY { get; set; }


        /// <summary>
        /// 杯盖区域行数
        /// </summary>
        public static int Machine_AreaCover_Row { get; set; }

        /// <summary>
        /// 配液杯泄压脉冲
        /// </summary>
        public static int Coordinate_Cup_Decompression { get; set; }

        /// <summary>
        /// 配液杯泄压脉冲(右)
        /// </summary>
        public static int Coordinate_Cup_Decompression_Right { get; set; }
        

        /// <summary>
        /// 天平X轴坐标
        /// </summary>
        public static int Coordinate_Balance_X { get; set; }

        /// <summary>
        /// 天平Y轴坐标
        /// </summary>
        public static int Coordinate_Balance_Y { get; set; }

        /// <summary>
        /// 待机位X轴坐标
        /// </summary>
        public static int Coordinate_Standby_X { get; set; }

        /// <summary>
        /// 待机位Y轴坐标
        /// </summary>
        public static int Coordinate_Standby_Y { get; set; }

        /// <summary>
        /// 清洗针筒X轴坐标
        /// </summary>
        public static int Coordinate_Wash_X { get; set; }

        /// <summary>
        /// 清洗针筒Y轴坐标
        /// </summary>
        public static int Coordinate_Wash_Y { get; set; }

        /// <summary>
        /// 粉筒夹子X轴坐标
        /// </summary>
        public static int Coordinate_PowerClamp_X { get; set; }

        /// <summary>
        /// 粉筒夹子Y轴坐标
        /// </summary>
        public static int Coordinate_PowerClamp_Y { get; set; }

        /// <summary>
        /// 清洗母液瓶Y轴坐标
        /// </summary>
        public static int Coordinate_WashBottle_Y { get; set; }

        /// <summary>
        /// 清洗母液瓶X轴坐标
        /// </summary>
        public static int Coordinate_WashBottle_X { get; set; }


        /// <summary>
        /// 1号开料位X坐标
        /// </summary>
        public static int Coordinate_Brew1_IntervalX { get; set; }

        /// <summary>
        /// 1号开料位Y坐标
        /// </summary>
        public static int Coordinate_Brew1_IntervalY { get; set; }

        /// <summary>
        /// 2号开料位X坐标
        /// </summary>
        public static int Coordinate_Brew2_IntervalX { get; set; }

        /// <summary>
        /// 2号开料位Y坐标
        /// </summary>
        public static int Coordinate_Brew2_IntervalY { get; set; }

        /// <summary>
        /// 3号开料位X坐标
        /// </summary>
        public static int Coordinate_Brew3_IntervalX { get; set; }

        /// <summary>
        /// 3号开料位Y坐标
        /// </summary>
        public static int Coordinate_Brew3_IntervalY { get; set; }

        /// <summary>
        /// 4号开料位X坐标
        /// </summary>
        public static int Coordinate_Brew4_IntervalX { get; set; }

        /// <summary>
        /// 4号开料位Y坐标
        /// </summary>
        public static int Coordinate_Brew4_IntervalY { get; set; }

        /// <summary>
        /// 5号开料位X坐标
        /// </summary>
        public static int Coordinate_Brew5_IntervalX { get; set; }

        /// <summary>
        /// 5号开料位Y坐标
        /// </summary>
        public static int Coordinate_Brew5_IntervalY { get; set; }

        /// <summary>
        /// 6号开料位X坐标
        /// </summary>
        public static int Coordinate_Brew6_IntervalX { get; set; }

        /// <summary>
        /// 6号开料位Y坐标
        /// </summary>
        public static int Coordinate_Brew6_IntervalY { get; set; }

        /// <summary>
        /// 1号放针位X坐标
        /// </summary>
        public static int Coordinate_PutSyringes1_IntervalX { get; set; }

        /// <summary>
        /// 1号放针位Y坐标
        /// </summary>
        public static int Coordinate_PutSyringes1_IntervalY { get; set; }

        /// <summary>
        /// 2号放针位X坐标
        /// </summary>
        public static int Coordinate_PutSyringes2_IntervalX { get; set; }

        /// <summary>
        /// 2号放针位Y坐标
        /// </summary>
        public static int Coordinate_PutSyringes2_IntervalY { get; set; }

        /// <summary>
        /// 3号放针位X坐标
        /// </summary>
        public static int Coordinate_PutSyringes3_IntervalX { get; set; }

        /// <summary>
        /// 3号放针位Y坐标
        /// </summary>
        public static int Coordinate_PutSyringes3_IntervalY { get; set; }

        /// <summary>
        /// 4号放针位X坐标
        /// </summary>
        public static int Coordinate_PutSyringes4_IntervalX { get; set; }

        /// <summary>
        /// 4号放针位Y坐标
        /// </summary>
        public static int Coordinate_PutSyringes4_IntervalY { get; set; }

        /// <summary>
        /// 5号放针位X坐标
        /// </summary>
        public static int Coordinate_PutSyringes5_IntervalX { get; set; }

        /// <summary>
        /// 5号放针位Y坐标
        /// </summary>
        public static int Coordinate_PutSyringes5_IntervalY { get; set; }

        /// <summary>
        /// 6号放针位X坐标
        /// </summary>
        public static int Coordinate_PutSyringes6_IntervalX { get; set; }

        /// <summary>
        /// 6号放针位Y坐标
        /// </summary>
        public static int Coordinate_PutSyringes6_IntervalY { get; set; }

        /// <summary>
        /// 7号放针位X坐标
        /// </summary>
        public static int Coordinate_PutSyringes7_IntervalX { get; set; }

        /// <summary>
        /// 7号放针位Y坐标
        /// </summary>
        public static int Coordinate_PutSyringes7_IntervalY { get; set; }

        /// <summary>
        /// 8号放针位X坐标
        /// </summary>
        public static int Coordinate_PutSyringes8_IntervalX { get; set; }

        /// <summary>
        /// 8号放针位Y坐标
        /// </summary>
        public static int Coordinate_PutSyringes8_IntervalY { get; set; }

        /// <summary>
        /// 9号放针位X坐标
        /// </summary>
        public static int Coordinate_PutSyringes9_IntervalX { get; set; }

        /// <summary>
        /// 9号放针位Y坐标
        /// </summary>
        public static int Coordinate_PutSyringes9_IntervalY { get; set; }

        /// <summary>
        /// 10号放针位X坐标
        /// </summary>
        public static int Coordinate_PutSyringes10_IntervalX { get; set; }

        /// <summary>
        /// 10号放针位Y坐标
        /// </summary>
        public static int Coordinate_PutSyringes10_IntervalY { get; set; }

        /// <summary>
        /// 11号放针位X坐标
        /// </summary>
        public static int Coordinate_PutSyringes11_IntervalX { get; set; }

        /// <summary>
        /// 11号放针位Y坐标
        /// </summary>
        public static int Coordinate_PutSyringes11_IntervalY { get; set; }

        /// <summary>
        /// PH计X坐标
        /// </summary>
        public static int Coordinate_PH_IntervalX { get; set; }

        /// <summary>
        /// PH计Y坐标
        /// </summary>
        public static int Coordinate_PH_IntervalY { get; set; }

        /// <summary>
        /// 盐度计X坐标
        /// </summary>
        public static int Coordinate_Salinity_IntervalX { get; set; }

        /// <summary>
        /// 盐度计Y坐标
        /// </summary>
        public static int Coordinate_Salinity_IntervalY { get; set; }

        /// <summary>
        /// 加水校正位置X坐标
        /// </summary>
        public static int Coordinate_AddWaterCorrecting_IntervalX { get; set; }

        /// <summary>
        /// 加水校正位置Y坐标
        /// </summary>
        public static int Coordinate_AddWaterCorrecting_IntervalY { get; set; }

        #endregion

        #region 校正参数

        /// <summary>
        /// 大针筒校正脉冲
        /// </summary>
        public static int Correcting_B_Pulse { get; set; }

        /// <summary>
        /// 大针筒验证重量
        /// </summary>
        public static int Correcting_B_Weight { get; set; }

        /// <summary>
        /// 小针筒校正脉冲
        /// </summary>
        public static int Correcting_S_Pulse { get; set; }

        /// <summary>
        /// 小针筒验证重量
        /// </summary>
        public static int Correcting_S_Weight { get; set; }

        /// <summary>
        /// 加水校正时间
        /// </summary>
        public static int Correcting_Water_Time { get; set; }

        /// <summary>
        /// 加水校正重量
        /// </summary>
        public static int Correcting_Water_RWeight { get; set; }

        /// <summary>
        /// 加水校正值
        /// </summary>
        public static double Correcting_Water_Value { get; set; }

        /// <summary>
        /// 流量计校正值
        /// </summary>
        public static double Correcting_FlowPulse_Value { get; set; }

        /// <summary>
        /// 加水首秒重量
        /// </summary>
        public static double Correcting_Water_FWeight { get; set; }

        /// <summary>
        /// 加溶解剂校正时间
        /// </summary>
        public static int Correcting_Dissolving_Time { get; set; }

        /// <summary>
        /// 加溶解剂校正重量
        /// </summary>
        public static int Correcting_Dissolving_RWeight { get; set; }

        /// <summary>
        /// 加溶解剂校正值
        /// </summary>
        public static double Correcting_Dissolving_Value { get; set; }

        /// <summary>
        /// 加溶解剂首秒重量
        /// </summary>
        public static double Correcting_Dissolving_FWeight { get; set; }

        #endregion

        #region 时间参数

        /// <summary>
        /// 上下气缸延时
        /// </summary>
        public static double Delay_Cylinder { get; set; }

        /// <summary>
        /// 抓手延时
        /// </summary>
        public static double Delay_Tongs { get;set; }

        /// <summary>
        /// 针筒检测延时
        /// </summary>
        public static double Delay_Syringe { get; set; }

        /// <summary>
        /// 接液盘延时
        /// </summary>
        public static double Delay_Tray { get; set; }

        /// <summary>
        /// 泄压检测延时
        /// </summary>
        public static double Delay_Decompression { get; set; }

        /// <summary>
        /// 泄压时间
        /// </summary>
        public static double Delay_DecoTime { get; set; }

        /// <summary>
        /// 阻挡延时
        /// </summary>
        public static double Delay_Block { get; set; }

        /// <summary>
        /// 排水延时
        /// </summary>
        public static double Delay_Apocenosis { get; set; }

        /// <summary>
        /// 转盘上下延时
        /// </summary>
        public static double Delay_Turntable { get; set; }

        /// <summary>
        /// 天平清零延时
        /// </summary>
        public static double Delay_Balance_Reset{get; set;}

        /// <summary>
        /// 天平读数延时
        /// </summary>
        public static double Delay_Balance_Read { get; set; }

        /// <summary>
        /// 完成报警延时
        /// </summary>
        public static double Delay_Buzzer_Finish { get; set; }


        #endregion

        #region 其他参数

        /// <summary>
        /// 校正允许误差
        /// </summary>
        public static double Other_AErr_Correcting { get; set; }

        /// <summary>
        /// 滴液允许误差
        /// </summary>
        public static double Other_AErr_Drip { get; set; }

        /// <summary>
        /// 助剂滴液允许误差
        /// </summary>
        public static double Other_AssAErr_Drip { get; set; }

        /// <summary>
        /// 加水允许误差
        /// </summary>
        public static double Other_AErr_Water { get; set; }

        /// <summary>
        /// 滴液时加水允许误差
        /// </summary>
        public static double Other_AErr_DripWater { get; set; }

        /// <summary>
        /// 加水系数
        /// </summary>
        public static double Other_Coefficient_Water { get; set; }

        /// <summary>
        /// 加水系数(小值)
        /// </summary>
        public static double Other_Coefficient_Water_Low { get; set; }


        /// <summary>
        /// 母液瓶库存量报警值
        /// </summary>
        public static double Other_Bottle_AlarmWeight { get; set; }

        /// <summary>
        /// 母液瓶库存量最低值
        /// </summary>
        public static double Other_Bottle_MinWeight { get; set; }

        /// <summary>
        /// 大针筒Z轴最大脉冲
        /// </summary>
        public static int Other_B_MaxPulse { get; set; }

        /// <summary>
        /// 小针筒Z轴最大脉冲
        /// </summary>
        public static int Other_S_MaxPulse { get; set; }

        /// <summary>
        /// 排空Z轴上移脉冲
        /// </summary>
        public static int Other_Z_UpPulse { get; set; }

        /// <summary>
        /// 排空Z轴下压脉冲
        /// </summary>
        public static int Other_Z_DownPulse { get; set; }

        /// <summary>
        /// 抽液完成反推脉冲
        /// </summary>
        public static int Other_Z_BackPulse { get; set; }

        /// <summary>
        /// 洗杯加水量
        /// </summary>
        public static double Other_AddWater { get; set; }

        /// <summary>
        /// 洗杯加水量(大杯)
        /// </summary>
        public static double Other_AddWater_Big { get; set; }

        /// <summary>
        /// 高温洗杯加水量
        /// </summary>
        public static double Other_HighAddWater { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public static double Other_SplitValue { get; set; }

        /// <summary>
        /// 液桶最大重量
        /// </summary>
        public static double Other_BalanceMaxWeight { get; set; }

        /// <summary>
        /// 抽液时是否先升气缸
        /// 0：边升边反推
        /// 1：反推完再升气缸
        /// </summary>
        public static int Other_Push { get; set; }

        /// <summary>
        /// 滴液杯位最大重量
        /// </summary>
        public static double Other_DripMaxWeight { get; set; }

        /// <summary>
        /// 后处理杯位最大重量
        /// </summary>
        public static double Other_HandleMaxWeight { get; set; }

        /// <summary>
        /// 后处理杯位最大重量(大杯)
        /// </summary>
        public static double Other_HandleMaxWeight_Big { get; set; }

        /// <summary>
        /// 输入染料报警值
        /// </summary>
        public static double Other_DyeAlarmWeight { get; set; }

        /// <summary>
        /// 输入助剂报警值
        /// </summary>
        public static double Other_AdditivesAlarmWeight { get; set; }

        /// <summary>
        /// 默认非脱水水比
        /// </summary>
        public static double Other_Default_Non_AnhydrationWR { get; set; }

        /// <summary>
        /// 默认脱水水比
        /// </summary>
        public static double Other_Default_AnhydrationWR { get; set; }


        /// <summary>
        /// 脱水水比是否跳过  默认是0   输入配方的时候,非脱水水比和脱水水比选项如果有值 则跳过该两选项
        /// 如果是1的话 则不跳过这两选项
        /// </summary>
        public static int Other_Default_N_A_Tpye { get; set; }

        /// <summary>
        /// 输入布重报警值
        /// </summary>
        public static double Other_ClothAlarmWeight { get; set; }

        #endregion

        /// <summary>
        /// 扫码枪是否存在
        /// 0：不存在
        /// 1：存在
        /// </summary>
        public static int Other_Scan { get; set; }

        /// <summary>
        /// 语言
        /// zh - 中文
        /// en - English
        /// </summary>
        public static int Other_Language { get; set; }

        /// <summary>
        /// 是否传统滴液机
        /// 1 - 是
        /// 0 - 不是(有打板区域)
        /// </summary>
        public static int Other_IsOnlyDrip { get; set; }

        /// <summary>
        /// 是否使用实际位置
        /// 0：不使用(每次运动前先回一次原点)
        /// 1：使用
        /// </summary>
        public static int Other_ActualPosition { get; set; }


        /// <summary>
        /// 稳定读数判断值
        /// </summary>
        public static double Other_Stable_Value { get; set; }

        /// <summary>
        /// 是否使用吸光度机
        /// 0：不使用
        /// 1：使用
        /// </summary>
        public static int Other_UseAbs { get; set; }

        /// <summary>
        /// 开始波长
        /// </summary>
        public static int Other_StartWave { get; set; }

        /// <summary>
        /// 结束波长
        /// </summary>
        public static int Other_EndWave { get; set; }

        /// <summary>
        /// 波长间隔
        /// </summary>
        public static int Other_IntWave { get; set; }

        /// <summary>
        /// 搅拌时间
        /// </summary>
        public static int Other_StirTime { get; set; }

        /// <summary>
        /// 洗杯搅拌时间
        /// </summary>
        public static int Other_WashStirTime { get; set; }

        /// <summary>
        /// 抽液时间
        /// </summary>
        public static int Other_AspirationTime { get; set; }

        /// <summary>
        /// 校验抽液时间
        /// </summary>
        public static int Other_CalAspirationTime { get; set; }

        /// <summary>
        /// 复测抽液时间
        /// </summary>
        public static int Other_ReAspirationTime { get; set; }

        /// <summary>
        /// 吸光度机洗杯加水量
        /// </summary>
        public static double Other_AbsAddWater { get; set; }

        /// <summary>
        /// 合夹夹布脉冲
        /// </summary>
        public static int Other_ClosePulse { get; set; }

        /// <summary>
        /// 开夹放布脉冲
        /// </summary>
        public static int Other_OpenPulse { get; set; }

        /// <summary>
        /// 备布区下探开夹脉冲
        /// </summary>
        public static int Other_ClothDownPulse { get; set; }

        /// <summary>
        /// 染杯口下探开夹脉冲
        /// </summary>
        public static int Other_CupDownPulse { get; set; }

        /// <summary>
        /// 加水放大倍数 0放大1000倍 1是放大100倍
        /// </summary>
        public static int Other_WaterMagnification { get; set; }

        /// <summary>
        /// 出布时气缸位置 0阻挡位 1慢速中
        /// </summary>
        public static int Other_OutClothPosition { get; set; }

        /// <summary>
        /// 洗针次数
        /// </summary>
        public static int Other_WashTime { get; set; }

        /// <summary>
        /// 是否配置抓手撑盖选项
        /// </summary>
        public static int Other_Tongs_Decompression { get; set; }

        /// <summary>
        /// 撑盖Y轴位置偏移
        /// </summary>
        public static int Other_SupportCoverY { get; set; }

        /// <summary>
        /// 是否启用阀岛
        /// </summary>
        public static int Other_ValveTerminal { get; set; }


        /// <summary>
        /// 开盖是否要伸出接液盘
        /// </summary>
        public static int Other_CoverTray { get; set; }

        /// <summary>
        /// 出布时是否进行两次出布动作
        /// </summary>
        public static int Other_RepeatGetCloth { get; set; }

        /// <summary>
        /// 50g布夹放布后回零点 0=需要回原点，1=不回原点
        /// </summary>
        public static int Other_OutClothZero { get; set; }

        /// <summary>
        /// 慢速中气缸常开常闭切换0=气阀常开（常用），1=气阀常闭
        /// </summary>
        public static int Other_ValveTerminalOpen { get; set; }

        /// <summary>
        /// 布重上限
        /// </summary>
        public static double Other_ClothUp { get; set; }

        /// <summary>
        /// 布重下限
        /// </summary>
        public static double Other_ClothDown { get; set; }

        /// <summary>
        /// 合夹夹布脉冲上限
        /// </summary>
        public static int Other_ClothPulseUp { get; set; }

        /// <summary>
        /// 合夹夹布脉冲下限
        /// </summary>
        public static int Other_ClothPulseDown { get; set; }

        /// <summary>
        /// 测PH抽液脉冲
        /// </summary>
        public static int Other_PHExtractPluse { get; set; }

        /// <summary>
        /// 干布区取布位置
        /// </summary>
        public static int Other_GetDryCloth { get; set; }

        /// <summary>
        /// 转盘的X轴起始位置
        /// </summary>
        public static int Other_TurntableXStart { get; set; }

        /// <summary>
        /// 转盘的X轴终点位置
        /// </summary>
        public static int Other_TurntableXEnd { get; set; }

        /// <summary>
        /// 转盘的Y轴起始位置
        /// </summary>
        public static int Other_TurntableYStart { get; set; }

        

        /// <summary>
        /// 转盘的Y轴终点位置
        /// </summary>
        public static int Other_TurntableYEnd { get; set; }

        /// <summary>
        /// 夹紧母液瓶脉冲
        /// </summary>
        public static int Other_TightBottlePluse { get; set; }

        /// <summary>
        /// 松开母液瓶脉冲
        /// </summary>
        public static int Other_LooseBottlePluse { get; set; }

        /// <summary>
        /// 加粉超时时间
        /// </summary>
        public static int Other_PowerOverTime { get; set; }

        /// <summary>
        /// 粉罐上抽脉冲
        /// </summary>
        public static int Other_PowerPluse { get; set; }

        /// <summary>
        /// 高度偏差脉冲
        /// </summary>
        public static int Other_CylinderHeightDeviationPulse { get; set; }

        /// <summary>
        /// 气缸中位置 |mm
        /// </summary>
        public static int Other_PositionInCylinder { get; set; }

        /// <summary>
        /// 气缸慢速中1位置 |mm
        /// </summary>
        public static int Other_PositionInCylinderSlow1 { get; set; }

        /// <summary>
        /// 气缸慢速中2位置 |mm
        /// </summary>
        public static int Other_PositionInCylinderSlow2 { get; set; }

        /// <summary>
        /// 气缸慢速中3位置 |mm
        /// </summary>
        public static int Other_PositionInCylinderSlow3 { get; set; }

        /// <summary>
        /// 气缸到阻挡位位置 |mm
        /// </summary>
        public static int Other_CylinderStopPosition { get; set; }

        /// <summary>
        /// 气缸下位置 |mm
        /// </summary>
        public static int Other_CylinderDownPosition { get; set; }

        /// <summary>
        /// 气缸定位范围 |mm
        /// </summary>
        public static int Other_CylinderPositioningRange { get; set; }

        /// <summary>
        /// 是否使用拉线编码器
        /// </summary>
        public static int Other_UseCylinderPositioningEncoder { get; set; }

    }
}
