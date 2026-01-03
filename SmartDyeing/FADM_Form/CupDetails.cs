using HslControls;
using SmartDyeing.FADM_Control;
using SmartDyeing.FADM_Object;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Linq;
using static SmartDyeing.FADM_Control.CurveControl;

namespace SmartDyeing.FADM_Form
{
    public partial class CupDetails : Form
    {
        DateTime[] _times;
        private int _i_cupNo;
        public CupDetails(int CupNo)
        {
            try
            {
                InitializeComponent();
                //this.Text = CupNo + "号杯详细资料";
                txt_CupNum.Text = CupNo.ToString();
                this._i_cupNo = CupNo;
                Curve2D cuv2D = new Curve2D();
                List<float> list_values = new List<float>();
                //string s = Lib_File.Txt.ReadTXT(CupNo);
                //if (s != "" && s != null)
                //{
                //    s = s.Substring(0, s.Length - 1);
                //    string[] arr = s.Split('@');

                //    float[] temp = new float[arr.Count()];
                //    DateTime[] times = new DateTime[arr.Count()];

                //    for (int i = 0; i < arr.Count(); i++)
                //    {
                //        temp[i] = Convert.ToSingle(arr[i]);
                //        times[i] = DateTime.Now.AddSeconds((i - arr.Count()) * 30);
                //    }

                //    // 显示出数据信息来

                //    // 设置曲线属性，名称，数据，颜色，是否平滑，格式化显示文本
                //    hslCurveHistory1.SetLeftCurve("温度", temp, Color.Blue, HslControls.CurveStyle.Curve, "{0:F1} ℃");
                //    hslCurveHistory1.SetDateTimes(times);
                //    hslCurveHistory1.RenderCurveUI(true);

                //}

                //string sMarkStep = Lib_File.Txt.ReadMarkTXT(CupNo);
                //if (sMarkStep != "" && sMarkStep != null)
                //{
                //    bool b = true;
                //    sMarkStep = sMarkStep.Substring(0, sMarkStep.Length - 1);
                //    string[] arr = sMarkStep.Split('@');
                //    for (int i = 0; i < arr.Count(); i++)
                //    {
                //        string sName = arr[i].Substring(0, arr[i].IndexOf(","));
                //        string sNum = arr[i].Substring(arr[i].IndexOf(",") + 1, arr[i].Count() - arr[i].IndexOf(",") - 1);

                //        hslCurveHistory1.AddMarkText(new HslControls.HslMarkText()
                //        {
                //            Index = Convert.ToInt32(sNum) - 1,
                //            CurveKey = "温度",
                //            MarkText = sName,
                //            PositionStyle = b ? MarkTextPositionStyle.Up : MarkTextPositionStyle.Down,
                //            CircleBrush = Brushes.Red,
                //            TextBrush = Brushes.Red
                //        }); ; ;
                //        b = !b;
                //    }
                //}
                double newtotalTimeInSeconds = 0;
                string s_cupno = Lib_File.Txt.ReadTXT(CupNo);
                if (s_cupno != "" && s_cupno != null)
                {
                    s_cupno = s_cupno.Substring(0, s_cupno.Length - 1);
                    string[] sa_arr = s_cupno.Split('@');

                    InitChart();

                    _times = new DateTime[sa_arr.Count()];

                    for (int i = 0; i < sa_arr.Count(); i++)
                    {
                        if (FADM_Object.Communal._b_is60Mark)
                        {
                            _times[i] = DateTime.Now.AddSeconds((i - sa_arr.Count()) * 60);
                        }
                        else
                        {
                            _times[i] = DateTime.Now.AddSeconds((i - sa_arr.Count()) * 30);
                        }
                    }


                    AddSeries("实际", Color.Red);


                    Series series = chart.Series[0];

                    for (int i = 0; i < sa_arr.Count(); i++)
                    {
                        series.Points.AddXY(Convert.ToDouble(i + 1), Convert.ToDouble(sa_arr[i]));
                    }
                    double totalTimeInSeconds = 0; // 每条曲线的总计用时（秒）
                    if (FADM_Object.Communal._b_is60Mark)
                    {
                        totalTimeInSeconds = sa_arr.Length * 60; // 每个点代表60秒
                    }
                    else
                    {
                        totalTimeInSeconds = sa_arr.Length * 30; // 每个点代表30秒
                    }
                                                             // 将总计用时转换为时分秒
                    newtotalTimeInSeconds = totalTimeInSeconds;
                    TimeSpan totalTime = TimeSpan.FromSeconds(totalTimeInSeconds);
                    string totalTimeFormatted = $"{totalTime.Hours:D2}小时 {totalTime.Minutes:D2}分钟 {totalTime.Seconds:D2}秒";
                    // 在图例中显示每条曲线的总计用时
                    LegendItem legendItem = new LegendItem
                    {
                        Name = series.Name,
                        Color = series.Color,
                        BorderColor = series.BorderColor,
                        BorderWidth = series.BorderWidth,
                        MarkerStyle = series.MarkerStyle,
                        MarkerSize = series.MarkerSize,
                        MarkerColor = series.MarkerColor,
                        MarkerBorderColor = series.MarkerBorderColor,
                        MarkerBorderWidth = series.MarkerBorderWidth,
                        ShadowColor = series.ShadowColor,
                        ShadowOffset = series.ShadowOffset,
                        Tag = series.Tag,
                        ToolTip = series.ToolTip
                    };
                    // 使用 LegendItem.Cells 设置文本
                    legendItem.Cells.Add(LegendCellType.Text, $"{series.Name} - 总计: {totalTimeFormatted}", ContentAlignment.MiddleLeft);
                    chart.Legends[0].CustomItems.Add(legendItem);


                    /*if (_maxTimes.Length < _times.Length)
                    {
                        _maxTimes = _times;
                    }*/

                    chart.MouseMove += new MouseEventHandler(chart1_MouseMove);
                    chart.MouseWheel += new System.Windows.Forms.MouseEventHandler(chart1_Mouselheel);
                }

                string s_markStep = Lib_File.Txt.ReadMarkTXT(CupNo);
                if (s_markStep != "" && s_markStep != null)
                {
                    s_markStep = s_markStep.Substring(0, s_markStep.Length - 1);
                    string[] sa_arr = s_markStep.Split('@');
                    for (int i = 0; i < sa_arr.Count(); i++)
                    {
                        string s_name = sa_arr[i].Substring(0, sa_arr[i].IndexOf(","));
                        string s_num = sa_arr[i].Substring(sa_arr[i].IndexOf(",") + 1, sa_arr[i].Count() - sa_arr[i].IndexOf(",") - 1);

                        if (Convert.ToInt32(s_num) <= chart.Series[0].Points.Count)
                        {
                            chart.Series[0].Points[Convert.ToInt32(s_num) - 1].MarkerColor = Color.Red;
                            chart.Series[0].Points[Convert.ToInt32(s_num) - 1].MarkerSize = 10;
                            chart.Series[0].Points[Convert.ToInt32(s_num) - 1].MarkerStyle = MarkerStyle.Triangle;
                            chart.Series[0].Points[Convert.ToInt32(s_num) - 1].Label = s_name;
                            chart.Series[0].Points[Convert.ToInt32(s_num) - 1].Font = new Font("Consolas", 12f);
                            chart.Series[0].Points[Convert.ToInt32(s_num) - 1].LabelForeColor = Color.Red;
                        }
                    }
                }
                //显示下预览曲线
                //获取批次资料表头
                string s_sql = "SELECT CupNum, FormulaCode, VersionNum" +
                                   " FROM drop_head where CupNum = "+ CupNo + " ;";
                DataTable dt_formula = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                List<ProcessStep> list = new List<ProcessStep>();

                if (dt_formula.Rows.Count > 0) {
                    string FormulaCode = dt_formula.Rows[0]["FormulaCode"].ToString();
                    string VersionNum = dt_formula.Rows[0]["VersionNum"].ToString();
                    s_sql = "SELECT FormulaCode,VersionNum,StepNum,TechnologyName,Temp,TempSpeed,Time,RotorSpeed,Code, DyeType,AssistantCode,FormulaDosage,UnitOfAccount,BottleNum,SettingConcentration,RealConcentration,AssistantName,ObjectDropWeight,RealDropWeight,BottleSelection FROM dyeing_details where FormulaCode = '" + FormulaCode + "' and VersionNum = '" + VersionNum + "' order by StepNum asc ;";
                    DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                    foreach (DataRow dr in dt_data.Rows)
                    {
                        string DyeType = dr["DyeType"].ToString();
                        string Code = dr["Code"].ToString();

                        ProcessStep processSte = new ProcessStep();
                        processSte.StepName = dr["TechnologyName"].ToString();

                        if (dr["TechnologyName"].ToString().Trim().Equals("加A") || dr["TechnologyName"].ToString().Trim().Equals("加B") || dr["TechnologyName"].ToString().Trim().Equals("加C") || dr["TechnologyName"].ToString().Trim().Equals("加D") || dr["TechnologyName"].ToString().Trim().Equals("加E") || dr["TechnologyName"].ToString().Trim().Equals("加F") || dr["TechnologyName"].ToString().Trim().Equals("加G") || dr["TechnologyName"].ToString().Trim().Equals("加H") || dr["TechnologyName"].ToString().Trim().Equals("加I") || dr["TechnologyName"].ToString().Trim().Equals("加J") || dr["TechnologyName"].ToString().Trim().Equals("加K") || dr["TechnologyName"].ToString().Trim().Equals("加L") || dr["TechnologyName"].ToString().Trim().Equals("加M") || dr["TechnologyName"].ToString().Trim().Equals("加N"))
                        {


                            //processSte.Duration = 5;
                            list.Add(processSte);
                            continue;
                        }
                        if (dr["TechnologyName"].ToString().Trim().Equals("加水"))
                        {
                            list.Add(processSte);
                            continue;
                        }

                        if (dr["Temp"].ToString() != null && dr["Temp"].ToString().Length > 0)
                        {
                            processSte.TargetTemperature = Convert.ToDouble(dr["Temp"].ToString());
                        }
                        if (dr["TempSpeed"].ToString() != null && dr["TempSpeed"].ToString().Length > 0)
                        {
                            processSte.HeatingRate = Convert.ToDouble(dr["TempSpeed"].ToString());
                        }
                        if (dr["Time"].ToString() != null && dr["Time"].ToString().Length > 0)
                        {
                            processSte.Duration = Convert.ToDouble(dr["Time"].ToString());
                        }
                        list.Add(processSte);
                    }

                    ProcessStep[] processSteps = list.ToArray();
                    // 生成chartData
                    CurveControl.chartData chartData = GenerateChartData(processSteps);
                    string temperature = chartData.temperature;
                    string craft = chartData.craft;



                    string[] sa_arr = temperature.Split('@');
                    //_times = new DateTime[sa_arr.Count()];
                    //for (int i = 0; i < sa_arr.Count(); i++)
                    //{
                    //    _times[i] = DateTime.Now.AddSeconds((i - sa_arr.Count()) * 30);
                    //}


                    AddSeries("理论", Color.Blue);
                    List<ProcessStep> list_need = new List<ProcessStep>();
                    //计算剩余工艺需要时间
                    s_sql = "SELECT FormulaCode,VersionNum,StepNum,TechnologyName,Temp,TempSpeed,Time,RotorSpeed,Code, DyeType,AssistantCode,FormulaDosage,UnitOfAccount,BottleNum,SettingConcentration,RealConcentration,AssistantName,ObjectDropWeight,RealDropWeight,BottleSelection FROM dye_details where CupNum = '" + CupNo + "' and Finish = '0' order by StepNum ;";
                    DataTable dt_data_Need = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                    foreach (DataRow dr in dt_data_Need.Rows)
                    {
                        string DyeType = dr["DyeType"].ToString();
                        string Code = dr["Code"].ToString();

                        ProcessStep processSte = new ProcessStep();
                        processSte.StepName = dr["TechnologyName"].ToString();

                        if (dr["TechnologyName"].ToString().Trim().Equals("加A") || dr["TechnologyName"].ToString().Trim().Equals("加B") || dr["TechnologyName"].ToString().Trim().Equals("加C") || dr["TechnologyName"].ToString().Trim().Equals("加D") || dr["TechnologyName"].ToString().Trim().Equals("加E") || dr["TechnologyName"].ToString().Trim().Equals("加F") || dr["TechnologyName"].ToString().Trim().Equals("加G") || dr["TechnologyName"].ToString().Trim().Equals("加H") || dr["TechnologyName"].ToString().Trim().Equals("加I") || dr["TechnologyName"].ToString().Trim().Equals("加J") || dr["TechnologyName"].ToString().Trim().Equals("加K") || dr["TechnologyName"].ToString().Trim().Equals("加L") || dr["TechnologyName"].ToString().Trim().Equals("加M") || dr["TechnologyName"].ToString().Trim().Equals("加N"))
                        {


                            //processSte.Duration = 5;
                            list_need.Add(processSte);
                            continue;
                        }
                        if (dr["TechnologyName"].ToString().Trim().Equals("加水"))
                        {
                            list_need.Add(processSte);
                            continue;
                        }

                        if (dr["Temp"].ToString() != null && dr["Temp"].ToString().Length > 0)
                        {
                            processSte.TargetTemperature = Convert.ToDouble(dr["Temp"].ToString());
                        }
                        if (dr["TempSpeed"].ToString() != null && dr["TempSpeed"].ToString().Length > 0)
                        {
                            processSte.HeatingRate = Convert.ToDouble(dr["TempSpeed"].ToString());
                        }
                        if (dr["Time"].ToString() != null && dr["Time"].ToString().Length > 0)
                        {
                            processSte.Duration = Convert.ToDouble(dr["Time"].ToString());
                        }
                        list_need.Add(processSte);
                    }

                    ProcessStep[] processSteps_need = list_need.ToArray();
                    // 生成chartData
                    CurveControl.chartData chartData_need = GenerateChartData(processSteps_need);
                    string temperature_need = chartData_need.temperature;
                    string craft_need = chartData_need.craft;



                    string[] sa_arr_need = temperature_need.Split('@');


                    Series series = chart.Series[1];

                    for (int i = 0; i < sa_arr.Count(); i++)
                    {
                        series.Points.AddXY(Convert.ToDouble(i + 1), Convert.ToDouble(sa_arr[i]));
                    }

                    double totalTimeInSeconds = 0; // 每条曲线的总计用时（秒）
                    if (FADM_Object.Communal._b_is60Mark)
                    {
                        totalTimeInSeconds = sa_arr.Length * 60; // 每个点代表60秒
                    }
                    else
                    {
                        totalTimeInSeconds = sa_arr.Length * 30; // 每个点代表30秒
                    }
                                                             // 将总计用时转换为时分秒
                    double time = totalTimeInSeconds - newtotalTimeInSeconds;
                    // 获取当前时间
                    DateTime now = DateTime.Now;
                    TimeSpan duration = TimeSpan.FromSeconds(sa_arr_need.Length*(FADM_Object.Communal._b_is60Mark? 60: 30));
                    DateTime futureTime = now + duration;
                    string cc = futureTime.ToString("HH:mm:ss");

                    TimeSpan totalTime = TimeSpan.FromSeconds(totalTimeInSeconds);
                    string totalTimeFormatted = $"{totalTime.Hours:D2}小时 {totalTime.Minutes:D2}分钟 {totalTime.Seconds:D2}秒";
                    // 在图例中显示每条曲线的总计用时
                    LegendItem legendItem = new LegendItem
                    {
                        Name = series.Name,
                        Color = series.Color,
                        BorderColor = series.BorderColor,
                        BorderWidth = series.BorderWidth,
                        MarkerStyle = series.MarkerStyle,
                        MarkerSize = series.MarkerSize,
                        MarkerColor = series.MarkerColor,
                        MarkerBorderColor = series.MarkerBorderColor,
                        MarkerBorderWidth = series.MarkerBorderWidth,
                        ShadowColor = series.ShadowColor,
                        ShadowOffset = series.ShadowOffset,
                        Tag = series.Tag,
                        ToolTip = series.ToolTip
                    };
                    // 使用 LegendItem.Cells 设置文本
                    legendItem.Cells.Add(LegendCellType.Text, $"{series.Name} - 总计: {totalTimeFormatted} -预计完成时间:{cc}", ContentAlignment.MiddleLeft);
                    chart.Legends[0].CustomItems.Add(legendItem);

                    chart.MouseClick += Chart1_MouseClick;

                    sa_arr = craft.Split('@');
                    for (int i = 0; i < sa_arr.Count(); i++)
                    {
                        string s_name = sa_arr[i].Substring(0, sa_arr[i].IndexOf(","));
                        string s_num = sa_arr[i].Substring(sa_arr[i].IndexOf(",") + 1, sa_arr[i].Count() - sa_arr[i].IndexOf(",") - 1);

                        if (Convert.ToInt32(s_num) <= chart.Series[1].Points.Count)
                        {
                            chart.Series[1].Points[Convert.ToInt32(s_num) - 1].MarkerColor = Color.Blue;
                            chart.Series[1].Points[Convert.ToInt32(s_num) - 1].MarkerSize = 10;
                            chart.Series[1].Points[Convert.ToInt32(s_num) - 1].MarkerStyle = MarkerStyle.Triangle;
                            chart.Series[1].Points[Convert.ToInt32(s_num) - 1].Label = s_name;
                            chart.Series[1].Points[Convert.ToInt32(s_num) - 1].Font = new Font("Consolas", 12f);
                            chart.Series[1].Points[Convert.ToInt32(s_num) - 1].LabelForeColor = Color.Blue;
                        }
                    }

                }


            }
            catch (Exception ex)
            {
                Lib_Log.Log.writeLogException("CupDetails：" + ex.ToString());
            }
        }
        private void Chart1_MouseClick(object sender, MouseEventArgs e)
        {
            //MessageBox.Show("点击了图例！");
            var legend = chart.Legends[0];
             var legendPosition = legend.Position;
             var legendRect = new RectangleF(
                 legendPosition.X,
                 legendPosition.Y,
                 50,
                 50);
            //e.Location.X+190

            //MessageBox.Show(e.Location.X.ToString()+"--"+ e.Location.Y.ToString());
            if (e.Location.X >351 && e.Location.Y<40)
             {
                //MessageBox.Show("隐藏！");
                chart.Series[1].Points.Clear();
             }
            /*if (legendRect.Contains(e.Location))
            {
                // 用户点击了图例
                MessageBox.Show("点击了图例！");
            }*/
        }




        public chartData GenerateChartData(ProcessStep[] processSteps)
        {
            StringBuilder temperatureBuilder = new StringBuilder();
            StringBuilder craftBuilder = new StringBuilder();
            double currentTemperature = 25.0; // 常温起步
            int timePoint = 0; // 从第0个点开始
            double ambientTemperature = 25.0; // 常温
            double coolingConstant = 0.1; // 冷却常数

            foreach (var step in processSteps)
            {
                double fixedDuration = 0;
                double duration = 0;
                // 在每个步骤开始时记录工艺步骤和当前时间点
                if (timePoint == 0)
                    craftBuilder.Append($"{step.StepName},{1}@");
                else
                    craftBuilder.Append($"{step.StepName},{timePoint}@");

                switch (step.StepName)
                {
                    case "放布":
                    case "出布":
                    case "取小样":
                    case "测PH":
                        // 固定时间设定为3分钟
                        fixedDuration = step.Duration ?? 3;
                        for (int i = 0; i < fixedDuration * (FADM_Object.Communal._b_is60Mark ? 1 : 2); i++) // 每分钟记录两个温度值
                        {
                            currentTemperature -= coolingConstant * (currentTemperature - ambientTemperature) / (FADM_Object.Communal._b_is60Mark ? 1 : 2); // 根据牛顿冷却定律降温
                            temperatureBuilder.Append($"{currentTemperature}@");
                            timePoint++;
                        }
                        break;
                    case "加A":
                    case "加B":
                    case "加C":
                    case "加D":
                    case "加E":
                    case "加F":
                    case "加G":
                    case "加H":
                    case "加I":
                    case "加J":
                    case "加K":
                    case "加L":
                    case "加M":
                    case "加N":
                        // 固定时间设定为0.5分钟
                        fixedDuration = step.Duration ?? 0.5;
                        for (int i = 0; i < fixedDuration * (FADM_Object.Communal._b_is60Mark ? 1 : 2); i++) // 每分钟记录两个温度值
                        {
                            currentTemperature -= coolingConstant * (currentTemperature - ambientTemperature) / (FADM_Object.Communal._b_is60Mark ? 1 : 2); // 根据牛顿冷却定律降温
                            temperatureBuilder.Append($"{currentTemperature}@");
                            timePoint++;
                        }
                        break;

                    case "洗杯":
                        // 固定时间设定为10分钟
                        fixedDuration = step.Duration ?? 10;
                        for (int i = 0; i < fixedDuration * (FADM_Object.Communal._b_is60Mark ? 1 : 2); i++) // 每分钟记录两个温度值
                        {
                            currentTemperature -= coolingConstant * (currentTemperature - ambientTemperature) / (FADM_Object.Communal._b_is60Mark ? 1 : 2); // 根据牛顿冷却定律降温
                            temperatureBuilder.Append($"{currentTemperature}@");
                            timePoint++;
                        }
                        break;

                    case "排液":
                        // 使用结构体中的时间参数，默认0.25分钟
                        duration = step.Duration ?? 0.25;
                        for (int i = 0; i < duration * (FADM_Object.Communal._b_is60Mark ? 1 : 2); i++) // 每分钟记录两个温度值
                        {
                            currentTemperature -= coolingConstant * (currentTemperature - ambientTemperature) / (FADM_Object.Communal._b_is60Mark ? 1 : 2); // 根据牛顿冷却定律降温
                            temperatureBuilder.Append($"{currentTemperature}@");
                            timePoint++;
                        }
                        break;
                    case "加水":
                        // 使用结构体中的时间参数，默认1分钟
                        duration = step.Duration ?? 1;
                        for (int i = 0; i < duration * (FADM_Object.Communal._b_is60Mark ? 1 : 2); i++) // 每分钟记录两个温度值
                        {
                            currentTemperature -= coolingConstant * (currentTemperature - ambientTemperature) / (FADM_Object.Communal._b_is60Mark ? 1 : 2); // 根据牛顿冷却定律降温
                            temperatureBuilder.Append($"{currentTemperature}@");
                            timePoint++;
                        }
                        break;
                    case "冷行":
                        // 使用结构体中的时间参数，默认5分钟
                        duration = step.Duration ?? 5;
                        for (int i = 0; i < duration * (FADM_Object.Communal._b_is60Mark ? 1 : 2); i++) // 每分钟记录两个温度值
                        {
                            currentTemperature -= coolingConstant * (currentTemperature - ambientTemperature) / (FADM_Object.Communal._b_is60Mark ? 1 : 2); // 根据牛顿冷却定律降温
                            temperatureBuilder.Append($"{currentTemperature}@");
                            timePoint++;
                        }
                        break;
                    case "搅拌":
                        // 使用结构体中的时间参数，默认5分钟
                        duration = step.Duration ?? 5;
                        for (int i = 0; i < duration * (FADM_Object.Communal._b_is60Mark ? 1 : 2); i++) // 每分钟记录两个温度值
                        {
                            currentTemperature -= coolingConstant * (currentTemperature - ambientTemperature) / (FADM_Object.Communal._b_is60Mark ? 1 : 2); // 根据牛顿冷却定律降温
                            temperatureBuilder.Append($"{currentTemperature}@");
                            timePoint++;
                        }
                        break;
                    case "温控":
                        // 使用结构体中的目标温度，保温时间，升温速率
                        double targetTemperature = step.TargetTemperature ?? 100;
                        double holdTime = step.Duration ?? 120;
                        double heatingRate = step.HeatingRate ?? 1;

                        // 升温或降温阶段
                        double temperatureDifference = targetTemperature - currentTemperature;
                        double heatingTime = Math.Abs(temperatureDifference) / heatingRate; // 转换为分钟
                        int heatingPoints = (int)(heatingTime * (FADM_Object.Communal._b_is60Mark ? 1 : 2)); // 每分钟记录两个温度值
                        for (int i = 0; i < heatingPoints; i++)
                        {
                            currentTemperature += (temperatureDifference > 0 ? heatingRate : -heatingRate) / (FADM_Object.Communal._b_is60Mark ? 1 : 2); // 每分钟升温或降温
                            temperatureBuilder.Append($"{currentTemperature}@");
                            timePoint++;
                        }

                        // 保温阶段
                        int holdPoints = (int)(holdTime * (FADM_Object.Communal._b_is60Mark ? 1 : 2)); // 每分钟记录两个温度值
                        for (int i = 0; i < holdPoints; i++)
                        {
                            temperatureBuilder.Append($"{targetTemperature}@");
                            timePoint++;
                        }
                        break;

                    default:
                        throw new ArgumentException($"未知的工艺步骤: {step.StepName}");
                }
            }

            return new chartData
            {
                temperature = temperatureBuilder.ToString().TrimEnd('@'),
                craft = craftBuilder.ToString().TrimEnd('@')
            };
        }
        private void CupDetails_Load(object sender, EventArgs e)
        {
            try
            {
                DataTable dt_data = new DataTable();
                DataRow dr;
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    //建立Column例，可以指明例的类型,这里用的是默认的string
                    dt_data.Columns.Add(new DataColumn("类型"));
                    dt_data.Columns.Add(new DataColumn("瓶号"));
                    dt_data.Columns.Add(new DataColumn("配方用量"));
                    dt_data.Columns.Add(new DataColumn("需加重量"));
                    dt_data.Columns.Add(new DataColumn("实加重量"));

                    //设置标题字体
                    dgv_CupDetails.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 14.25F);

                    //设置内容字体
                    dgv_CupDetails.RowsDefaultCellStyle.Font = new Font("宋体", 14.25F);

                }
                else
                {
                    dt_data.Columns.Add(new DataColumn("Type"));
                    dt_data.Columns.Add(new DataColumn("BottleNum"));
                    dt_data.Columns.Add(new DataColumn("DosageOfFormula"));
                    dt_data.Columns.Add(new DataColumn("TargetVolume"));
                    dt_data.Columns.Add(new DataColumn("ActualVolume"));

                    //设置标题字体
                    dgv_CupDetails.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 10.5F);

                    //设置内容字体
                    dgv_CupDetails.RowsDefaultCellStyle.Font = new Font("宋体", 10.5F);
                }
                //获取要显示的行数

                //设置标题居中显示
                dgv_CupDetails.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;



                //设置内容居中显示
                dgv_CupDetails.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;



                //设置行高
                dgv_CupDetails.RowTemplate.Height = 30;

                ////获取当前批次号
                //string P_str_sql = "SELECT BatchName FROM enabled_set" +
                //                  " WHERE MyID = 1 ";

                //DataTable _dt_data = FADM_Object.Communal._fadmSqlserver.GetData(P_str_sql);

                //string P_str_batch = Convert.ToString(_dt_data.Rows[0][_dt_data.Columns[0]]);

                //P_str_sql = "SELECT * FROM machine_parameters" +
                //            " WHERE MyID = 1 ";
                //_dt_data = FADM_Object.Communal._fadmSqlserver.GetData(P_str_sql);
                int i_maxcup = Lib_Card.Configure.Parameter.Machine_Cup_Total;
                int i_maxbottle = Lib_Card.Configure.Parameter.Machine_Bottle_Total;
                List<int> ints = new List<int>();

                //获取当前批次当前杯号信息
                string s_sql = "SELECT * FROM drop_details WHERE" +
                            " CupNum = '" + this._i_cupNo +
                            "' AND (BottleNum > 0 AND ( BottleNum <= " + i_maxbottle + "" +
                            " ) Or BottleNum = 200 Or BottleNum =201) ORDER BY BottleNum;";

                DataTable dt_data1 = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                int i_line = dt_data1.Rows.Count;
                for (int i = 1; i <= i_line; i++)
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    {
                        dr = dt_data.NewRow();
                        dr[0] = "滴液";
                        dr[1] = dt_data1.Rows[i - 1]["BottleNum"];
                        dr[2] = dt_data1.Rows[i - 1]["FormulaDosage"];
                        dr[3] = dt_data1.Rows[i - 1]["ObjectDropWeight"];
                        dr[4] = dt_data1.Rows[i - 1]["RealDropWeight"];
                        dt_data.Rows.Add(dr);

                        
                    }
                    else
                    {
                        dr = dt_data.NewRow();
                        dr[0] = "Drip";
                        dr[1] = dt_data1.Rows[i - 1]["BottleNum"];
                        dr[2] = dt_data1.Rows[i - 1]["FormulaDosage"];
                        dr[3] = dt_data1.Rows[i - 1]["ObjectDropWeight"];
                        dr[4] = dt_data1.Rows[i - 1]["RealDropWeight"];
                        dt_data.Rows.Add(dr);
                    }
                    //判断是否合格
                    if (dt_data1.Rows[i-1]["Finish"].ToString() == "1")
                    {
                        if (dt_data1.Rows[i - 1]["ObjectDropWeight"] is DBNull || dt_data1.Rows[i - 1]["RealDropWeight"] is DBNull)
                        {
                            //dgv_FormulaData.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                            ints.Add(0);
                        }
                        else
                        {
                            double d_error = Lib_Card.Configure.Parameter.Other_AErr_Drip;
                            if (!(dt_data1.Rows[i - 1]["StandError"] is DBNull))
                            {
                                d_error = Convert.ToDouble(dt_data1.Rows[i - 1]["StandError"]);
                            }
                            else
                            {
                                d_error = (dt_data1.Rows[i - 1]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AssAErr_Drip);
                            }
                            if ((int)(Math.Abs(Convert.ToDouble(dt_data1.Rows[i - 1]["ObjectDropWeight"]) - Convert.ToDouble(dt_data1.Rows[i - 1]["RealDropWeight"])) * 1000) > (int)(d_error * 1000))
                            {
                                //dgv_FormulaData.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                                ints.Add(0);
                            }
                            else
                            {
                                ints.Add(1);
                            }
                        }
                    }
                    else
                    {
                        ints.Add(1);
                    }
                }

                //获取当前批次当前杯号信息
                s_sql = "SELECT * FROM dye_details WHERE" +
                            " CupNum = '" + this._i_cupNo +
                            "' AND (BottleNum > 0 AND ( BottleNum <= " + i_maxbottle + "" +
                            " ) Or BottleNum = 200 Or BottleNum =201)  ORDER BY StepNum;";

                dt_data1 = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                int i_line1 = dt_data1.Rows.Count;
                for (int i = 1; i <= i_line1; i++)
                {
                    double d = 0.0;
                    if (dt_data1.Rows[i - 1]["Compensation"] is DBNull)
                    {
                    }
                    else
                    {
                        d = Convert.ToDouble(dt_data1.Rows[i - 1]["Compensation"].ToString());
                    }
                    dr = dt_data.NewRow();
                    dr[0] = dt_data1.Rows[i - 1]["Code"];
                    dr[1] = dt_data1.Rows[i - 1]["BottleNum"];
                    dr[2] = dt_data1.Rows[i - 1]["FormulaDosage"];
                    dr[3] = (Convert.ToDouble(dt_data1.Rows[i - 1]["ObjectDropWeight"].ToString()) + d).ToString("F2");
                    dr[4] = dt_data1.Rows[i - 1]["RealDropWeight"];
                    dt_data.Rows.Add(dr);

                    //判断是否合格
                    if (dt_data1.Rows[i - 1]["Finish"].ToString() == "1")
                    {
                        if (dt_data1.Rows[i - 1]["ObjectDropWeight"] is DBNull || dt_data1.Rows[i - 1]["RealDropWeight"] is DBNull)
                        {
                            //dgv_FormulaData.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                            ints.Add(0);
                        }
                        else
                        {
                            double d_error = Lib_Card.Configure.Parameter.Other_AErr_Drip;
                            if (!(dt_data1.Rows[i - 1]["StandError"] is DBNull))
                            {
                                d_error = Convert.ToDouble(dt_data1.Rows[i - 1]["StandError"]);
                            }
                            else
                            {
                                d_error = (dt_data1.Rows[i - 1]["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AErr_Drip);
                            }
                            if ((int)(Math.Abs(Convert.ToDouble(dt_data1.Rows[i - 1]["ObjectDropWeight"]) - Convert.ToDouble(dt_data1.Rows[i - 1]["RealDropWeight"])) * 1000) > (int)(d_error * 1000))
                            {
                                //dgv_FormulaData.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                                ints.Add(0);
                            }
                            else
                            {
                                ints.Add(1);
                            }
                        }
                    }
                    else
                    {
                        ints.Add(1);
                    }
                }

                //捆绑
                dgv_CupDetails.DataSource = new DataView(dt_data);
                //预留滚动条空间
                int i_headspace = 1;
                if (i_line > 11)
                {
                    i_headspace = 5;
                }
                for (int i = 0; i < 5; i++)
                {
                    dgv_CupDetails.Columns[i].Width = dgv_CupDetails.Width / 5 - i_headspace;
                }
                dgv_CupDetails.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv_CupDetails.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;


                s_sql = "SELECT * FROM drop_head WHERE" +
                            " CupNum = '" + this._i_cupNo + "' ;";

                dt_data1 = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                if (dt_data1.Rows.Count > 0)
                {

                    txt_FormulaCode.Text = Convert.ToString(dt_data1.Rows[0][dt_data1.Columns["FormulaCode"]]);

                    txt_VersionNum.Text = Convert.ToString(dt_data1.Rows[0][dt_data1.Columns["VersionNum"]]);

                    txt_objectWater.Text = Convert.ToString(dt_data1.Rows[0][dt_data1.Columns["ObjectAddWaterWeight"]]);

                    txt_realWater.Text = Convert.ToString(dt_data1.Rows[0][dt_data1.Columns["RealAddWaterWeight"]]);

                    if(Convert.ToDouble(dt_data1.Rows[0][dt_data1.Columns["ObjectAddWaterWeight"]]) >0)
                    {
                        if (dt_data1.Rows[0]["AddWaterFinish"].ToString() == "1")
                        {
                            double d_error = Lib_Card.Configure.Parameter.Other_AErr_DripWater;
                            if (!(dt_data1.Rows[0]["WaterStandError"] is DBNull))
                            {
                                d_error = Convert.ToDouble(dt_data1.Rows[0]["WaterStandError"]);
                            }
                            double d_totalWeight = Convert.ToDouble(dt_data1.Rows[0]["TotalWeight"]);
                            double d_allDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                        d_totalWeight * Convert.ToDouble(d_error / 100.00)) : string.Format("{0:F3}",
                                        d_totalWeight * Convert.ToDouble(d_error / 100.00)));

                            if (d_allDif < Math.Abs(Convert.ToDouble(dt_data1.Rows[0][dt_data1.Columns["RealAddWaterWeight"]])- Convert.ToDouble(dt_data1.Rows[0][dt_data1.Columns["ObjectAddWaterWeight"]])))
                            {
                                txt_realWater.BackColor = Color.Red;
                            }
                        }

                    }


                    s_sql = "SELECT SUM(CAST(ISNULL(ObjectDropWeight,0.00) as numeric(18,2))) FROM drop_details WHERE" +
                                " CupNum = '" + this._i_cupNo +
                                "' AND BottleNum > " + i_maxbottle + ";";

                    dt_data1 = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                }

                for (int i = 0; i < ints.Count; i++)
                {
                    if (ints[i] == 0)
                        dgv_CupDetails.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                }

                //string s = Lib_File.Txt.ReadTXT(this.iCupNo);
                //if (s != "" && s != null)
                //{
                //    s = s.Substring(0, s.Length - 1);
                //    string[] arr = s.Split('@');

                //    float[] temp = new float[arr.Count()];
                //    DateTime[] times = new DateTime[arr.Count()];

                //    for (int i = 0; i < arr.Count(); i++)
                //    {
                //        temp[i] = Convert.ToSingle(arr[i]);
                //        times[i] = DateTime.Now.AddSeconds((i - arr.Count()) * 30);
                //    }

                //    // 显示出数据信息来

                //    // 显示出数据信息来
                //    Invoke(new Action(() =>
                //    {
                //        // 设置曲线属性，名称，数据，颜色，是否平滑，格式化显示文本
                //        hslCurveHistory1.SetLeftCurve("温度", temp, Color.Blue, HslControls.CurveStyle.Curve, "{0:F1} ℃");
                //        hslCurveHistory1.SetDateTimes(times);
                //        hslCurveHistory1.RenderCurveUI(true);
                //    }));

                //}

                //string sMarkStep = Lib_File.Txt.ReadMarkTXT(this.iCupNo);
                //if (sMarkStep != "" && sMarkStep != null)
                //{
                //    bool b = true;
                //    sMarkStep = sMarkStep.Substring(0, sMarkStep.Length - 1);
                //    string[] arr = sMarkStep.Split('@');
                //    for (int i = 0; i < arr.Count(); i++)
                //    {
                //        string sName = arr[i].Substring(0, arr[i].IndexOf(","));
                //        string sNum = arr[i].Substring(arr[i].IndexOf(",") + 1, arr[i].Count() - arr[i].IndexOf(",") - 1);

                //        hslCurveHistory1.AddMarkText(new HslControls.HslMarkText()
                //        {
                //            Index = Convert.ToInt32(sNum) - 1,
                //            CurveKey = "温度",
                //            MarkText = sName,
                //            PositionStyle = b ? MarkTextPositionStyle.Up : MarkTextPositionStyle.Down,
                //            CircleBrush = Brushes.Red,
                //            TextBrush = Brushes.Red
                //        }); ; ;
                //        b = !b;
                //    }
                //}
            }
            catch (Exception ex)
            {
                Lib_Log.Log.writeLogException("CupDetails_Load：" + ex.ToString());
            }

        }

        private void Show(string s)
        {
            s = s.Substring(0, s.Length - 1);
            string[] arr = s.Split('@');

            _times = new DateTime[arr.Count()];
            for (int i = 0; i < arr.Count(); i++)
            {
                _times[i] = DateTime.Now.AddSeconds((i - arr.Count()) * (FADM_Object.Communal._b_is60Mark?60: 30));
            }

            AddSeries("温度", Color.Red);

            Series series = chart.Series[0];
            //series.Points.AddXY(0, 0);

            for (int i = 0; i < arr.Count(); i++)
            {
                series.Points.AddXY(Convert.ToDouble(i + 1), Convert.ToDouble(arr[i]));
            }
            chart.MouseMove += new MouseEventHandler(chart1_MouseMove);

            chart.MouseWheel += new System.Windows.Forms.MouseEventHandler(chart1_Mouselheel);


        }

        private void chart1_Mouselheel(object sender, MouseEventArgs e)
        {
            var chart1 = (System.Windows.Forms.DataVisualization.Charting.Chart)sender;
            var xAxis = chart1.ChartAreas[0].AxisX;
            var yAxis = chart1.ChartAreas[0].AxisY;

            try
            {
                if (e.Delta < 0) // Scrolled down.
                {
                    xAxis.ScaleView.ZoomReset();
                    yAxis.ScaleView.ZoomReset();
                }
                else if (e.Delta > 0) // Scrolled up.
                {
                    var xMin = xAxis.ScaleView.ViewMinimum;
                    var xMax = xAxis.ScaleView.ViewMaximum;
                    var yMin = yAxis.ScaleView.ViewMinimum;
                    var yMax = yAxis.ScaleView.ViewMaximum;

                    var posXStart = xAxis.PixelPositionToValue(e.Location.X) - (xMax - xMin) / 4;
                    var posXFinish = xAxis.PixelPositionToValue(e.Location.X) + (xMax - xMin) / 4;
                    var posYStart = yAxis.PixelPositionToValue(e.Location.Y) - (yMax - yMin) / 4;
                    var posYFinish = yAxis.PixelPositionToValue(e.Location.Y) + (yMax - yMin) / 4;

                    xAxis.ScaleView.Zoom(posXStart, posXFinish);
                    yAxis.ScaleView.Zoom(posYStart, posYFinish);
                }
            }
            catch { }
        }

        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (chart.Series.Count > 0)
                {
                    chart.ChartAreas[0].CursorX.SetCursorPixelPosition(new PointF(e.X, e.Y), true);
                    chart.ChartAreas[0].CursorY.SetCursorPixelPosition(new PointF(e.X, e.Y), true);
                    if (Convert.ToInt32(chart.ChartAreas[0].CursorX.Position.ToString()) < chart.Series[0].Points.Count && Convert.ToInt32(chart.ChartAreas[0].CursorX.Position.ToString()) > 0)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            toolTip1.SetToolTip(chart, string.Format("序号:{0},时间:{1}, 温度:{2}", chart.Series[0].Points[Convert.ToInt32(chart.ChartAreas[0].CursorX.Position.ToString()) - 1].XValue, _times[Convert.ToInt32(chart.ChartAreas[0].CursorX.Position.ToString()) - 1].ToString(),
                            chart.Series[0].Points[Convert.ToInt32(chart.ChartAreas[0].CursorX.Position.ToString()) - 1].YValues[0]));
                        else
                            toolTip1.SetToolTip(chart, string.Format("Index:{0},Time:{1}, Temperature:{2}", chart.Series[0].Points[Convert.ToInt32(chart.ChartAreas[0].CursorX.Position.ToString()) - 1].XValue, _times[Convert.ToInt32(chart.ChartAreas[0].CursorX.Position.ToString()) - 1].ToString(),
                            chart.Series[0].Points[Convert.ToInt32(chart.ChartAreas[0].CursorX.Position.ToString()) - 1].YValues[0]));
                    }
                }
            }
            catch { }

        }

        private void AddSeries(string seriersName, Color serierscolor)
        {
            Series series = new Series(seriersName);
            //图表类型  设置为样条图曲线
            series.ChartType = SeriesChartType.Line;
            //series.IsXValueIndexed = true;
            series.XValueType = ChartValueType.Double;
            series.MarkerStyle = MarkerStyle.Circle;
            series.MarkerColor = Color.Black;
            //设置点的大小
            series.MarkerSize = 3;
            //设置曲线的颜色
            series.Color = serierscolor;
            //设置曲线宽度
            series.BorderWidth = 2;
            series.CustomProperties = "PointWidth=2";
            series.IsValueShownAsLabel = false;//是否显示点的值

            chart.Series.Add(series);
            series.Legend = chart.Legends[0].Name;

        }


        private void CreateChart()
        {
            chart = new Chart();
            this.panel1.Controls.Add(chart);
            chart.Dock = DockStyle.Fill;
            chart.Visible = true;

            ChartArea chartArea = new ChartArea();
            //chartArea.Name = "FirstArea";
            chartArea.AxisX.Interval = 40;
            chartArea.AxisX.IntervalOffset = 40;

            chartArea.CursorX.IsUserEnabled = true;
            chartArea.CursorX.IsUserSelectionEnabled = true;
            chartArea.CursorX.SelectionColor = Color.SkyBlue;
            chartArea.CursorY.IsUserEnabled = true;
            chartArea.CursorY.AutoScroll = true;
            chartArea.CursorY.IsUserSelectionEnabled = true;
            chartArea.CursorY.SelectionColor = Color.SkyBlue;

            //chartArea.CursorX.IntervalType = DateTimeIntervalType._b_auto;
            chartArea.AxisX.ScaleView.Zoomable = false;//是否可以放大X轴
            chartArea.AxisX.ScrollBar.ButtonStyle = ScrollBarButtonStyles.All;//启用X轴滚动条按钮

            chartArea.BackColor = Color.White;                      //背景色
            //chartArea.BackSecondaryColor = Color.White;                 //渐变背景色
            //chartArea.BackGradientStyle = GradientStyle.TopBottom;      //渐变方式
            //chartArea.BackHatchStyle = ChartHatchStyle.None;            //背景阴影
            chartArea.BorderDashStyle = ChartDashStyle.NotSet;          //边框线样式
            //chartArea.BorderWidth = 1;                                  //边框宽度
            chartArea.BorderColor = Color.Black;
            //chartArea.AxisX.ArrowStyle = AxisArrowStyle.Lines;//坐标轴是否有箭头
            //chartArea.AxisY.ArrowStyle = AxisArrowStyle.Lines;//坐标轴是否有箭头




            //chartArea.AxisX.
            chartArea.AxisX.MajorGrid.Enabled = true;
            chartArea.AxisY.MajorGrid.Enabled = true;
            if (Lib_Card.Configure.Parameter.Other_Language == 0)
            {
                // Axis
                chartArea.AxisY.Title = @"温度(℃)";
                chartArea.AxisY.LineWidth = 2;
                chartArea.AxisY.LineColor = Color.Black;
                chartArea.AxisY.Enabled = AxisEnabled.True;


                chartArea.AxisX.Title = @"时间(X30s)";
                chartArea.AxisX.IsLabelAutoFit = true;
                chartArea.AxisX.LabelAutoFitMinFontSize = 5;
                chartArea.AxisX.LabelStyle.Angle = -15;
            }
            else
            {
                // Axis
                chartArea.AxisY.Title = @"Temperature(℃)";
                chartArea.AxisY.LineWidth = 2;
                chartArea.AxisY.LineColor = Color.Black;
                chartArea.AxisY.Enabled = AxisEnabled.True;


                chartArea.AxisX.Title = @"Time(X30s)";
                chartArea.AxisX.IsLabelAutoFit = true;
                chartArea.AxisX.LabelAutoFitMinFontSize = 5;
                chartArea.AxisX.LabelStyle.Angle = -15;
            }

            chartArea.AxisX.LabelStyle.IsEndLabelVisible = true;        //show the last label


            chartArea.AxisX.LineWidth = 2;
            chartArea.AxisX.LineColor = Color.Black;
            chartArea.AxisX.Enabled = AxisEnabled.True;


            chartArea.Position.Height = 85;
            chartArea.Position.Width = 95;
            chartArea.Position.X = 0;
            chartArea.Position.Y = 13;

            chart.ChartAreas.Add(chartArea);
            chart.BackGradientStyle = GradientStyle.TopBottom;
            //图表的边框颜色、
            chart.BorderlineColor = Color.FromArgb(26, 59, 105);
            //图表的边框线条样式
            chart.BorderlineDashStyle = ChartDashStyle.Solid;
            //图表边框线条的宽度
            chart.BorderlineWidth = 2;
            //图表边框的皮肤
            chart.BorderSkin.SkinStyle = BorderSkinStyle.Emboss;
            //chart.ChartAreas[0].AxisX.LabelStyle.IsStaggered = true;

            chart.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart.ChartAreas[0].AxisY.ScaleView.Zoomable = true;


            // 添加图例
            Legend legend = new Legend
            {
                Docking = Docking.Top,
                Alignment = StringAlignment.Far,
                LegendStyle = LegendStyle.Row,
                BorderColor = Color.Black,
                BorderWidth = 1,
                BorderDashStyle = ChartDashStyle.Solid
            };
            chart.Legends.Add(legend);
        }

        private void InitChart()
        {
            CreateChart();
        }
    }
}
