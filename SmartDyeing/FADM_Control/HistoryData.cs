using BLECode;
using CHNSpec.Device.Bluetooth;
using CHNSpec.Device.COM.Demo;
using CHNSpec.Device.Models;
using CHNSpec.Device.Models.Enums;
using HslControls;
using HslControls.Charts;
using PdfTableExportTool;
using SmartDyeing.FADM_Form;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static SmartDyeing.FADM_Control.CurveControl;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SmartDyeing.FADM_Control
{
    public partial class HistoryData : UserControl
    {
        DateTime[] times;
        bool _b_show=true;
        public static string SoftwareName { get { return "DeviceMeasure"; } }
        public static string ApplyMyDocuments { get => $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\{SoftwareName}\\"; }
        public static string SettingFileName { get => Path.Combine(ApplyMyDocuments, "setting.txt"); }

        public static string MeasureFileName { get => Path.Combine(ApplyMyDocuments, "measure.txt"); }



        public  Dictionary<string, NoActivateForm> noActivateFormsList = new Dictionary<string, NoActivateForm>();

        private static IList<string> temData;

        private bool triggerKeyPress = false;

        /// <summary>
        /// 蓝牙设备列表
        /// </summary>
        List<DeviceInfo> bluetoothList = new List<DeviceInfo>();

        //public BluetoothHelper _helper = new BluetoothHelper();

        List<string> _lis_bluetoothNane = new List<string>();
        bool _b_isShowButton = false;

        string _s_batch = null;
        string _s_formulaCode = null;
        string _s_versionNum = null;
        string _s_cupNum = null;
        //测量次数
        int _i_nCount = 0;
        DateTime[] _times;

        // 字体大小的上下限（避免过小/过大）
        private const int MinFontSize = 6;
        private const int MaxFontSize = 16;
        // 单元格内边距（避免文本贴边）
        private const int CellPadding = 5;

        /// <summary>
        /// true表示正在查找蓝牙设备，false表示已停止查找
        /// </summary>
        public bool _b_isDiscovering = false;

        /// <summary>
        /// 初始化DataGridView并添加测试数据
        /// </summary>
        private void InitDataGridView()
        {
            dgv_Details.Dock = DockStyle.Fill;
            dgv_Details.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv_Details.RowTemplate.Height = 30;

            // 添加列
            dgv_Details.Columns.Add("Col1", "测试列1");
            dgv_Details.Columns.Add("Col2", "测试列2");

            // 添加测试数据（包含长短不同的文本）
            dgv_Details.Rows.Add("短文本", "这是一段很长的文本，测试字体自动缩小效果");
            dgv_Details.Rows.Add("中等长度的文本内容", "超超超超超超长的文本内容示例，用于验证字体自适应功能");
            dgv_Details.Rows.Add("1234567890", "abcdefghijklmnopqrstuvwxyz");
        }

        /// <summary>
        /// 单元格绘制时调整字体大小
        /// </summary>
        private void dgv_Details_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // 跳过表头和空单元格
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.Value == null) return;

            string cellText = e.Value.ToString();
            if (string.IsNullOrEmpty(cellText)) return;

            // 获取单元格可用宽度（减去内边距）
            int cellWidth = e.CellBounds.Width - 2 * CellPadding;
            if (cellWidth <= 0) return;

            // 计算适配的字体大小
            float fontSize = GetAdaptiveFontSize(cellText, cellWidth, e.CellStyle.Font.FontFamily);

            // 创建一个临时字体对象（不修改全局样式）
            using (Font tempFont = new Font(e.CellStyle.Font.FontFamily, fontSize, FontStyle.Regular))
            {
                // 自己绘制文本，不依赖 DataGridView 默认绘制
                e.PaintBackground(e.CellBounds, true);
                TextRenderer.DrawText(e.Graphics, cellText, tempFont, e.CellBounds,
                                      e.CellStyle.ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                e.Handled = true; // 阻止默认绘制
            }

            //// 应用新字体（避免重复设置）
            //if (Math.Abs(e.CellStyle.Font.Size - fontSize) > 0.1)
            //{
            //    e.CellStyle.Font = new Font(e.CellStyle.Font.FontFamily, fontSize, FontStyle.Regular);
            //    // 刷新单元格（确保样式生效）
            //    dgv_Details.InvalidateCell(e.ColumnIndex, e.RowIndex);
            //}
        }

        /// <summary>
        /// 计算适配单元格宽度的字体大小
        /// </summary>
        /// <param name="text">单元格文本</param>
        /// <param name="maxWidth">单元格可用宽度</param>
        /// <param name="fontFamily">字体家族（保持字体样式一致）</param>
        /// <returns>适配的字体大小</returns>
        private float GetAdaptiveFontSize(string text, int maxWidth, FontFamily fontFamily)
        {
            // 初始字体大小（从最大值开始尝试）
            float fontSize = MaxFontSize;
            Size textSize;

            do
            {
                // 测量当前字体大小的文本宽度
                using (Font font = new Font(fontFamily, fontSize, FontStyle.Regular))
                {
                    textSize = TextRenderer.MeasureText(text, font);
                }

                // 如果文本宽度超过限制，缩小字体
                if (textSize.Width > maxWidth)
                {
                    fontSize -= 0.5f; // 步长可根据需求调整
                }
                else
                {
                    break; // 适配成功
                }

            } while (fontSize >= MinFontSize); // 不低于最小字体

            return Math.Max(fontSize, MinFontSize); // 确保不小于最小值
        }

        /// <summary>
        /// 单元格内容变更时刷新
        /// </summary>
        private void dgv_Details_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                dgv_Details.InvalidateCell(e.ColumnIndex, e.RowIndex);
            }
        }

        /// <summary>
        /// 控件大小变更时刷新所有单元格
        /// </summary>
        private void dgv_Details_Resize(object sender, EventArgs e)
        {
            dgv_Details.Refresh();
        }


        Main _main;
        public HistoryData(Main m)
        {
            try
            {
                InitializeComponent();
                txt_Record_Code.Leave += new EventHandler(comboBox2_Leave);
                txt_Record_Code.KeyPress += dy_nodelist_comboBox2_KeyPress;

                this.Load += MyUserControl_Load;

                // 绑定事件
                dgv_Details.CellPainting += dgv_Details_CellPainting;
                dgv_Details.CellValueChanged += dgv_Details_CellValueChanged;
                dgv_Details.Resize += dgv_Details_Resize;


            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            _main = m;
            //if (main.BtnUserSwitching.Text == "管理用户")
            //{
            //    btn_Record_Delete.Visible = true;
            //}
            ShowHeader();
            InitChart();
            //// 关键事件：控件大小/内容变化时，先调列宽再调字体
            //dgv_Details.Resize += (s, e) => { AdjustColumnWidthToFitAll(); dgv_Details.Refresh(); };
            //dgv_Details.CellValueChanged += (s, e) => { AdjustColumnWidthToFitAll(); dgv_Details.InvalidateCell(e.ColumnIndex, e.RowIndex); };
            //dgv_Details.CellPainting += dgv_Details_CellPainting;

        }

        private void MyUserControl_Load(object sender, EventArgs e)
        {
            Form parentForm = this.FindForm();
            if (parentForm != null)
            {
                parentForm.LocationChanged += ParentForm_LocationChanged;
            }
        }
        private void ParentForm_LocationChanged(object sender, EventArgs e)
        {
            foreach (KeyValuePair<string, NoActivateForm> pair in this.noActivateFormsList)
            {
                pair.Value.Visible = false;
                pair.Value.Close();
            }
        }

        private void comboBox2_Leave(object sender, EventArgs e)
        {
            System.Windows.Forms.TextBox cc = (System.Windows.Forms.TextBox)sender;
            if (noActivateFormsList.ContainsKey(Convert.ToString(cc.Name)))
            {
                noActivateFormsList[Convert.ToString(cc.Name)].Visible = false;
                noActivateFormsList[Convert.ToString(cc.Name)].Close();
            }
        }

        private void ThreadBlueToothCon()
        {
            if (FADM_Object.Communal._i_LabMode == 0)
            {
                bool b_find = false;
                string s_path = Environment.CurrentDirectory + "\\Config\\Config.ini";
                string s_name = Lib_File.Ini.GetIni("BlueTooth", "Name", "", s_path);
                if (s_name == "")
                {
                    Lib_File.Ini.WriteIni("BlueTooth", "Name", "", s_path);
                }

                _lis_bluetoothNane.Clear();
                bluetoothList.Clear();
                txt_TotalTime.Text = string.Empty;

                SmartDyeing.FADM_Object.Communal._helper.bleCode.StartBleDeviceWatcher("CM");

                Thread.Sleep(5000);
                DeviceInfo deviceInfo = null; ;
                for (int i = 0; i < bluetoothList.Count; i++)
                {
                    deviceInfo = bluetoothList[i];
                    if (deviceInfo.Name == s_name)
                    {
                        b_find = true;
                        break;
                    }
                }

                if (deviceInfo == null)
                {
                    return;
                }
                if (!b_find)
                {
                    return;
                }
                bool b_result = SmartDyeing.FADM_Object.Communal._helper.OpenBluetooth(deviceInfo.DeviceId, deviceInfo.Name);
                if (!b_result)
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show("连接分光仪失败", "温馨提示", MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("Failed to connect the spectrometer", "Tips", MessageBoxButtons.OK, false);
                    return;
                }
                _b_isShowButton = true;
            }
            else if (FADM_Object.Communal._i_LabMode == 1)
            {
                //初始状态
                SmartDyeing.FADM_Object.Communal._Comhelper.StopUSBWatcher();
                SmartDyeing.FADM_Object.Communal._Comhelper.CloseSerialPort();

                //USB监听
                SmartDyeing.FADM_Object.Communal._Comhelper.StartUSBWatcher();
                Thread.Sleep(500);//创建串口监听后需要等待一定的时候后才能打开串口
                                  //打开串口
                string portName = string.Empty;
                SmartDyeing.FADM_Object.Communal._Comhelper.OpenSerialPort(ref portName);
            }
        }

        private static void CreateFile(string s_message)
        {
            string s_filePath = ApplyMyDocuments;
            if (!Directory.Exists(s_filePath))
                Directory.CreateDirectory(s_filePath);
            string s_filePathName = Path.Combine(s_filePath, "measurestart.txt");
            try
            {
                using (var fileStream = File.Create(s_filePathName))
                {
                    byte[] byta_info = new UTF8Encoding(true).GetBytes(s_message);
                    fileStream.Write(byta_info, 0, byta_info.Length);
                }
            }
            catch (Exception)
            {

            }
        }

        private static void CreateSettingFile(string s_starttype)
        {
            string s_filePath = ApplyMyDocuments;
            if (!Directory.Exists(s_filePath))
                Directory.CreateDirectory(s_filePath);
            string filePathName = SettingFileName;
            try
            {
                if (File.Exists(filePathName))
                {
                    string s_res = File.ReadAllText(filePathName);
                    string[] sa_settings = s_res.Split(',');
                    if (sa_settings.Length > 0)
                    {
                        for (int i = 0; i < sa_settings.Length; i++)
                        {
                            if (sa_settings[i]?.StartsWith("starttype:") == true)
                            {
                                sa_settings[i] = $"starttype:{s_starttype}";
                            }
                        }
                        File.WriteAllText(filePathName, string.Join(",", sa_settings));
                    }
                }
                else
                {
                    //starttype 0 不显示 ， 1 显示， 2 连接不显示，不连接显示
                    string s_message = $"starttype:{SettingFileName},connectstatus:0";
                    File.WriteAllText(filePathName, s_message);
                }
            }
            catch (Exception)
            {


            }
        }

        private void btn_Record_Select_Click(object sender, EventArgs e)
        {
            DropRecordHeadShow();
        }

        private void btn_Record_Delete_Click(object sender, EventArgs e)
        {
            if (FADM_Object.Communal._s_operator != "管理用户" && FADM_Object.Communal._s_operator != "工程师")
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    FADM_Form.CustomMessageBox.Show("非管理员用户不能删除！", "温馨提示", MessageBoxButtons.OK, false);
                else
                    FADM_Form.CustomMessageBox.Show("Non administrator users cannot delete！", "Tips", MessageBoxButtons.OK, false);
                btn_Record_Delete.Visible = false;
                return;
            }
            try
            {
                if (dgv_DropRecord.CurrentRow != null)
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    {
                        DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("确定删除吗?", "删除历史记录", MessageBoxButtons.YesNo, true);

                        if (dialogResult == DialogResult.Yes)
                        {
                            //如果选中行
                            if (dgv_DropRecord.SelectedRows.Count > 0)
                            {
                                string s_formulaCode = dgv_DropRecord.CurrentRow.Cells[0].Value.ToString();
                                string s_versionNum = dgv_DropRecord.CurrentRow.Cells[1].Value.ToString();
                                string s_finishTime = dgv_DropRecord.CurrentRow.Cells[2].Value.ToString();
                                string s_cupNum = dgv_DropRecord.CurrentRow.Cells[3].Value.ToString();
                                string s_batch = dgv_DropRecord.CurrentRow.Cells[4].Value.ToString();

                                //string P_str_sql = "SELECT BatchName FROM history_head WHERE" +
                                //                   " FormulaCode = '" + FormulaCode + "' AND" +
                                //                   " VersionNum = '" + VersionNum + "' AND" +
                                //                   //" FinishTime = '" + FinishTime + "' AND" +
                                //                   " CupNum = " + CupNum + ";";

                                //DataTable _dt_data = FADM_Object.Communal._fadmSqlserver.GetData(P_str_sql);

                                //string P_str_batch = Convert.ToString(_dt_data.Rows[0][_dt_data.Columns[0]]);

                                string s_sql = "DELETE FROM history_head WHERE" +
                                            " FormulaCode = '" + s_formulaCode + "' AND" +
                                            " VersionNum = '" + s_versionNum + "' AND" +
                                            //" FinishTime = '" + FinishTime + "' AND" +
                                            " BatchName = '" + s_batch + "' AND" +
                                            " CupNum = " + s_cupNum + ";";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                s_sql = "DELETE FROM history_details WHERE" +
                                            " FormulaCode = '" + s_formulaCode + "' AND" +
                                            " VersionNum = '" + s_versionNum + "' AND" +
                                            " BatchName = '" + s_batch + "' AND" +
                                            " CupNum = " + s_cupNum + ";";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                s_sql = "DELETE FROM history_dye WHERE" +
                                            " FormulaCode = '" + s_formulaCode + "' AND" +
                                            " VersionNum = '" + s_versionNum + "' AND" +
                                            " BatchName = '" + s_batch + "' AND" +
                                            " CupNum = " + s_cupNum + ";";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                            }
                            //按照时间删除
                            else
                            {
                                //先把时间段内数据查询出来
                                string s_str = "Select BatchName from history_head Where ";
                                if (dt_Record_Start.Text != null && dt_Record_Start.Text != "")
                                {
                                    s_str += (" FinishTime >= '" + dt_Record_Start.Text + "' AND");
                                }
                                else
                                {
                                    return;
                                }

                                if (dt_Record_End.Text != null && dt_Record_End.Text != "")
                                {
                                    s_str += (" FinishTime <= '" + dt_Record_End.Text + "' ");
                                }
                                else
                                {
                                    return;
                                }

                                //
                                string s_sql = "DELETE FROM history_dye WHERE BatchName  in(" + s_str + ") ;";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                s_sql = "DELETE FROM history_details WHERE BatchName  in(" + s_str + ") ;";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                s_sql = "DELETE FROM history_head WHERE BatchName  in(" + s_str + ") ;";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                            }

                            DropRecordHeadShow();

                        }
                    }
                    else
                    {
                        DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("Are you sure to delete it?", "Delete History", MessageBoxButtons.YesNo, true);

                        if (dialogResult == DialogResult.Yes)
                        {
                            //如果选中行
                            if (dgv_DropRecord.SelectedRows.Count > 0)
                            {
                                string s_formulaCode = dgv_DropRecord.CurrentRow.Cells[0].Value.ToString();
                                string s_versionNum = dgv_DropRecord.CurrentRow.Cells[1].Value.ToString();
                                string s_finishTime = dgv_DropRecord.CurrentRow.Cells[2].Value.ToString();
                                string s_cupNum = dgv_DropRecord.CurrentRow.Cells[3].Value.ToString();
                                string s_batch = dgv_DropRecord.CurrentRow.Cells[4].Value.ToString();

                                //string P_str_sql = "SELECT BatchName FROM history_head WHERE" +
                                //                   " FormulaCode = '" + FormulaCode + "' AND" +
                                //                   " VersionNum = '" + VersionNum + "' AND" +
                                //                   //" FinishTime = '" + FinishTime + "' AND" +
                                //                   " CupNum = " + CupNum + ";";

                                //DataTable _dt_data = FADM_Object.Communal._fadmSqlserver.GetData(P_str_sql);

                                //string P_str_batch = Convert.ToString(_dt_data.Rows[0][_dt_data.Columns[0]]);

                                string s_sql = "DELETE FROM history_head WHERE" +
                                            " FormulaCode = '" + s_formulaCode + "' AND" +
                                            " VersionNum = '" + s_versionNum + "' AND" +
                                            //" FinishTime = '" + FinishTime + "' AND" +
                                            " BatchName = '" + s_batch + "' AND" +
                                            " CupNum = " + s_cupNum + ";";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                s_sql = "DELETE FROM history_details WHERE" +
                                            " FormulaCode = '" + s_formulaCode + "' AND" +
                                            " VersionNum = '" + s_versionNum + "' AND" +
                                            " BatchName = '" + s_batch + "' AND" +
                                            " CupNum = " + s_cupNum + ";";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                s_sql = "DELETE FROM history_dye WHERE" +
                                            " FormulaCode = '" + s_formulaCode + "' AND" +
                                            " VersionNum = '" + s_versionNum + "' AND" +
                                            " BatchName = '" + s_batch + "' AND" +
                                            " CupNum = " + s_cupNum + ";";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                            }
                            //按照时间删除
                            else
                            {
                                //先把时间段内数据查询出来
                                string s_str = "Select BatchName from history_head Where ";
                                if (dt_Record_Start.Text != null && dt_Record_Start.Text != "")
                                {
                                    s_str += (" FinishTime >= '" + dt_Record_Start.Text + "' AND");
                                }
                                else
                                {
                                    return;
                                }

                                if (dt_Record_End.Text != null && dt_Record_End.Text != "")
                                {
                                    s_str += (" FinishTime <= '" + dt_Record_End.Text + "' ");
                                }
                                else
                                {
                                    return;
                                }

                                //
                                string s_sql = "DELETE FROM history_dye WHERE BatchName  in(" + s_str + ") ;";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                s_sql = "DELETE FROM history_details WHERE BatchName  in(" + s_str + ") ;";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                s_sql = "DELETE FROM history_head WHERE BatchName  in(" + s_str + ") ;";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                            }

                            DropRecordHeadShow();

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.Message, "btn_Record_Delete_Click", MessageBoxButtons.OK, true);
            }
        }

        private void ShowHeader()
        {
            if (Lib_Card.Configure.Parameter.Other_Language == 0)
            {
                string[] sa_lineName = { "工艺代码", "步号", "名称", "助染剂代码", "助染剂名称", "用量", "单位", "瓶号", "设定浓度", "实际浓度", "目标滴液", "实际滴液", "目标温度", "速率", "时间", "目标加水", "转速", "开始时间", "工艺时长" };
                for (int i = 0; i < sa_lineName.Count(); i++)
                {
                    dgv_Details.Columns.Add("", "");
                    dgv_Details.Columns[i].HeaderCell.Value = sa_lineName[i];
                    //关闭点击标题自动排序功能
                    dgv_Details.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                    //dgv_Details.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }
                //设置标题字体
                dgv_Details.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 12.8F);
                //设置内容字体
                dgv_Details.RowsDefaultCellStyle.Font = new Font("宋体", 12.8F);

                
            }
            else
            {
                string[] sa_lineName = { "ProcessCode", "StepNumber", "name", "DyeingAuxiliariesCode", "DyeingAuxiliariesName", "DosageOfFormula", "Units", "BottleNumber", "SetConcentration", "ActualConcentration", "TargetVolume", "ActualVolume", "TargetTemperature", "Rate", "Time", "TargetWaterAddition", "Speed", "Time-on", "Duration" };
                for (int i = 0; i < sa_lineName.Count(); i++)
                {
                    dgv_Details.Columns.Add("", "");
                    dgv_Details.Columns[i].HeaderCell.Value = sa_lineName[i];
                    //关闭点击标题自动排序功能
                    dgv_Details.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                    dgv_Details.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }
                //设置标题字体
                dgv_Details.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 7.5F);
                //设置内容字体
                dgv_Details.RowsDefaultCellStyle.Font = new Font("宋体", 10.5F);
            }
            int totalWidth = dgv_Details.Width - 20; // 预留边框空间

            ////设置标题宽度
            dgv_Details.Columns[0].Width = (int)(totalWidth * 0.09);
            dgv_Details.Columns[1].Width = (int)(totalWidth * 0.04);
            dgv_Details.Columns[2].Width = (int)(totalWidth * 0.04);
            dgv_Details.Columns[3].Width = (int)(totalWidth * 0.05);
            dgv_Details.Columns[4].Width = (int)(totalWidth * 0.09);
            dgv_Details.Columns[5].Width = (int)(totalWidth * 0.05);
            dgv_Details.Columns[6].Width = (int)(totalWidth * 0.035);
            dgv_Details.Columns[7].Width = (int)(totalWidth * 0.035);
            dgv_Details.Columns[8].Width = (int)(totalWidth * 0.05);
            dgv_Details.Columns[9].Width = (int)(totalWidth * 0.07);
            dgv_Details.Columns[10].Width = (int)(totalWidth * 0.05);
            dgv_Details.Columns[11].Width = (int)(totalWidth * 0.05);
            dgv_Details.Columns[12].Width = (int)(totalWidth * 0.05);
            dgv_Details.Columns[13].Width = (int)(totalWidth * 0.04);
            dgv_Details.Columns[14].Width = (int)(totalWidth * 0.04);
            dgv_Details.Columns[15].Width = (int)(totalWidth * 0.05);
            dgv_Details.Columns[16].Width = (int)(totalWidth * 0.04);
            dgv_Details.Columns[17].Width = (int)(totalWidth * 0.07);
            dgv_Details.Columns[18].Width = (int)(totalWidth * 0.07);
            //设置标题居中显示
            dgv_Details.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

          

            //设置内容居中显示
            dgv_Details.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

          

            //设置行高
            dgv_Details.RowTemplate.Height = 30;


            dgv_DropRecord.Columns.Add("", "");
            dgv_DropRecord.Columns.Add("", "");
            dgv_DropRecord.Columns.Add("", "");
            dgv_DropRecord.Columns.Add("", "");
            dgv_DropRecord.Columns.Add("", "");
            if (Lib_Card.Configure.Parameter.Other_Language == 0)
            {
                //设置标题文字
                dgv_DropRecord.Columns[0].HeaderCell.Value = "配方代码";
                dgv_DropRecord.Columns[1].HeaderCell.Value = "版本";
                dgv_DropRecord.Columns[2].HeaderCell.Value = "时间/时期";
                dgv_DropRecord.Columns[3].HeaderCell.Value = "杯位";
                dgv_DropRecord.Columns[4].HeaderCell.Value = "批次号";
                //设置标题字体
                dgv_DropRecord.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 14.25F);
                //设置内容字体
                dgv_DropRecord.RowsDefaultCellStyle.Font = new Font("宋体", 12.5F);
            }
            else
            {
                dgv_DropRecord.Columns[0].HeaderCell.Value = "RecipeCode";
                dgv_DropRecord.Columns[1].HeaderCell.Value = "Version";
                dgv_DropRecord.Columns[2].HeaderCell.Value = "Date/Time";
                dgv_DropRecord.Columns[3].HeaderCell.Value = "CupNumber";
                dgv_DropRecord.Columns[4].HeaderCell.Value = "BatchNumber";
                //设置标题字体
                dgv_DropRecord.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 7.5F);
                //设置内容字体
                dgv_DropRecord.RowsDefaultCellStyle.Font = new Font("宋体", 10.5F);
            }


            //设置标题宽度
            dgv_DropRecord.Columns[0].Width = 240;
            dgv_DropRecord.Columns[1].Width = 53;
            dgv_DropRecord.Columns[2].Width = 190;
            dgv_DropRecord.Columns[3].Width = 53;
            //if (dgv_FormulaData.Rows.Count > 5)
            //{
            //    dgv_DropRecord.Columns[4].Width = 515;
            //}
            //else
            //{
            dgv_DropRecord.Columns[4].Width = 135;
            //}


            //关闭自动排序功能
            for (int i = 0; i < dgv_DropRecord.Columns.Count; i++)
            {
                dgv_DropRecord.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }


            //设置标题居中显示
            dgv_DropRecord.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

          

            //设置内容居中显示
            dgv_DropRecord.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

          

            //设置行高
            dgv_DropRecord.RowTemplate.Height = 30;
        }

        /// <summary>
        /// 显示滴液记录资料
        /// </summary>
        /// <returns>0:正常;-1异常</returns>
        private void DropRecordHeadShow()
        {
            try
            {
                dgv_DropRecord.Rows.Clear();

                string s_sql = null;
                DataTable dt_data = new DataTable();


                //获取配方浏览资料表头
                if (rdo_Record_Now.Checked)
                {
                    s_sql = "SELECT FormulaCode, VersionNum, FinishTime, CupNum," +
                                " BatchName,DescribeChar FROM history_head WHERE" +
                                " FinishTime > CONVERT(varchar,GETDATE(),23) ORDER BY MyID DESC;";
                    dt_data = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                }
                else if (rdo_Record_All.Checked)
                {
                    s_sql = "SELECT FormulaCode, VersionNum, FinishTime, CupNum," +
                                " BatchName,DescribeChar FROM history_head" +
                                " WHERE FinishTime != '' ORDER BY MyID DESC;";
                    dt_data = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                }
                else
                {
                    string s_str = null;
                    if (txt_Record_Operator.Text != null && txt_Record_Operator.Text != "")
                    {
                        s_str = (" Operator = '" + txt_Record_Operator.Text + "' AND");
                    }
                    if (txt_Record_CupNum.Text != null && txt_Record_CupNum.Text != "")
                    {
                        s_str = (" CupNum = '" + txt_Record_CupNum.Text + "' AND");
                    }
                    if (txt_Record_Code.Text != null && txt_Record_Code.Text != "")
                    {
                        s_str += (" FormulaCode = '" + txt_Record_Code.Text + "' AND");
                    }
                    if (dt_Record_Start.Text != null && dt_Record_Start.Text != "")
                    {
                        s_str += (" FinishTime >= '" + dt_Record_Start.Text + "' AND");
                    }
                    else
                    {
                        return;
                    }

                    if (dt_Record_End.Text != null && dt_Record_End.Text != "")
                    {
                        s_str += (" FinishTime <= '" + dt_Record_End.Text + "' ");
                    }
                    else
                    {
                        return;
                    }

                    s_sql = "SELECT FormulaCode, VersionNum, FinishTime, CupNum," +
                               " BatchName,DescribeChar FROM history_head Where" + s_str + "" +
                               " ORDER BY MyID DESC;";
                    dt_data = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                }

                //捆绑
                string s_r;
                int i_success = 0;
                int i_fails = 0;
                for (int i = 0; i < dt_data.Rows.Count; i++)
                {
                    dgv_DropRecord.Rows.Add(dt_data.Rows[i][0].ToString(), dt_data.Rows[i][1].ToString(), dt_data.Rows[i][2].ToString(), dt_data.Rows[i][3].ToString(), dt_data.Rows[i][4].ToString());
                    if (dt_data.Rows[i][5].ToString().Contains("成功"))
                    {
                        //dgv_DropRecord.Rows[i].DefaultCellStyle.BackColor = Color.Lime;
                        i_success++;
                    }
                    else
                    {
                        dgv_DropRecord.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                        i_fails++;
                    }
                }
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    s_r = "成功:" + i_success.ToString() + " 失败:" + i_fails.ToString();
                else
                    s_r = "Success:" + i_success.ToString() + " Fail:" + i_fails.ToString();
                txt_R.Text = s_r;

                dgv_DropRecord.ClearSelection();

                //P_str_sql = "SELECT FinishTime FROM" +
                //            " history_head ORDER BY FinishTime DESC ;";
                //_dt_data = FADM_Object.Communal._fadmSqlserver.GetData(P_str_sql);
                //if (_dt_data.Rows.Count > 0)
                //{
                //    try
                //    {
                //        dt_Record_Start.MinDate = Convert.ToDateTime(_dt_data.Rows[_dt_data.Rows.Count - 1][_dt_data.Columns[0]]);
                //        dt_Record_Start.MaxDate = Convert.ToDateTime(_dt_data.Rows[0][_dt_data.Columns[0]]);
                //        dt_Record_Start.Value = Convert.ToDateTime(_dt_data.Rows[_dt_data.Rows.Count - 1][_dt_data.Columns[0]]);
                //        dt_Record_End.MinDate = Convert.ToDateTime(_dt_data.Rows[_dt_data.Rows.Count - 1][_dt_data.Columns[0]]);
                //        dt_Record_End.MaxDate = Convert.ToDateTime(_dt_data.Rows[0][_dt_data.Columns[0]]);
                //        dt_Record_End.Value = Convert.ToDateTime(_dt_data.Rows[0][_dt_data.Columns[0]]);
                //    }
                //    catch
                //    {

                //    }
                //}
                //else
                //{
                //    try
                //    {
                //        dt_Record_Start.MinDate = Convert.ToDateTime("2021-01-01");
                //        dt_Record_Start.MaxDate = DateTime.Now;
                //        dt_Record_Start.Value = DateTime.Now;
                //        dt_Record_End.MinDate = Convert.ToDateTime("2021-01-01");
                //        dt_Record_End.MaxDate = DateTime.Now;
                //        dt_Record_End.Value = DateTime.Now;
                //    }
                //    catch
                //    {

                //    }
                //}

            }
            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.Message, "DropRecordHeadShow", MessageBoxButtons.OK, true);
            }
        }

        private void dgv_DropRecord_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dgv_DropRecord_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgv_DropRecord.ClearSelection();
        }

        private void rdo_Record_All_CheckedChanged(object sender, EventArgs e)
        {
            if (rdo_Record_All.Checked)
            {
                txt_Record_Operator.Enabled = false;
                txt_Record_Code.Enabled = false;
                txt_Record_CupNum.Enabled = false;
                dt_Record_Start.Enabled = false;
                dt_Record_End.Enabled = false;
                btn_Record_Delete.Visible = false;
            }
        }

        private void rdo_Record_condition_CheckedChanged(object sender, EventArgs e)
        {
            if (rdo_Record_condition.Checked)
            {
                txt_Record_Operator.Enabled = true;
                txt_Record_Code.Enabled = true;
                txt_Record_CupNum.Enabled = true;
                dt_Record_Start.Enabled = true;
                dt_Record_End.Enabled = true;

                if (FADM_Object.Communal._s_operator == "管理用户" || FADM_Object.Communal._s_operator == "工程师")
                {
                    btn_Record_Delete.Visible = true;
                }
            }
        }

        private void rdo_Record_Now_CheckedChanged(object sender, EventArgs e)
        {
            if (rdo_Record_Now.Checked)
            {
                txt_Record_Operator.Enabled = false;
                txt_Record_Code.Enabled = false;
                txt_Record_CupNum.Enabled = false;
                dt_Record_Start.Enabled = false;
                dt_Record_End.Enabled = false;
                btn_Record_Delete.Visible = false;
            }
        }

        private void DetailsShow()
        {
            try
            {
                txt_RealAddWaterWeight.BackColor = TextBox.DefaultBackColor;

                dgv_Details.Rows.Clear();

                if (chart.Series.Count > 0)
                {
                    chart.Legends.Clear();
                    // 添加图例
                    System.Windows.Forms.DataVisualization.Charting.Legend legend = new System.Windows.Forms.DataVisualization.Charting.Legend
                    {
                        Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Top,
                        Alignment = StringAlignment.Far,
                        LegendStyle = System.Windows.Forms.DataVisualization.Charting.LegendStyle.Row,
                        BorderColor = Color.Black,
                        BorderWidth = 1,
                        BorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid
                    };
                    chart.Legends.Add(legend);

                    chart.Series.Clear();
                    chart.MouseMove -= new MouseEventHandler(chart1_MouseMove);
                    chart.MouseWheel -= new MouseEventHandler(chart1_MouseMove);
                    chart.MouseClick -= Chart1_MouseClick;
                }
                toolTip1.RemoveAll();

                //读取选中行对应的配方资料
                string s_formulaCode = dgv_DropRecord.CurrentRow.Cells[0].Value.ToString();
                string s_versionNum = dgv_DropRecord.CurrentRow.Cells[1].Value.ToString();
                string s_finishtime = dgv_DropRecord.CurrentRow.Cells[2].Value.ToString();
                string s_cup = dgv_DropRecord.CurrentRow.Cells[3].Value.ToString();
                string s_batchname = dgv_DropRecord.CurrentRow.Cells[4].Value.ToString();
                string s_sql = "SELECT * FROM history_head" +
                                   " Where FormulaCode = '" + s_formulaCode + "'" +
                                   " AND VersionNum = '" + s_versionNum + "'" +
                                   " AND FinishTime = '" + s_finishtime + "'" +
                                   " AND CupNum = " + s_cup + ";";
                DataTable dt_formulahead = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                s_sql = "SELECT * FROM history_details" +
                            " Where FormulaCode = '" + s_formulaCode + "'" +
                            " AND VersionNum = '" + s_versionNum + "'" +
                            " AND BatchName = '" + s_batchname + "'" +
                            " AND CupNum = " + s_cup + " ;";
                DataTable dt_formuladetail = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                s_sql = "SELECT * FROM history_dye" +
                            " Where FormulaCode = '" + s_formulaCode + "'" +
                            " AND VersionNum = '" + s_versionNum + "'" +
                            " AND BatchName = '" + s_batchname + "'" +
                            " AND CupNum = " + s_cup + " order by StepNum ;";
                DataTable dt_dye_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                string s_finishTime = "";
                string s_totalFinishTime = "";

                if (dt_formulahead.Rows.Count > 0)
                {
                    //显示表头
                    foreach (DataColumn mDc in dt_formulahead.Columns)
                    {
                        string s_name = "txt_" + mDc.Caption.ToString();
                        foreach (Control c in this.grp_FormulaData.Controls)
                        {
                            if ((c is TextBox || c is ComboBox) && c.Name == s_name)
                            {
                                c.Text = dt_formulahead.Rows[0][mDc].ToString();
                                break;
                            }
                        }
                        if (s_name == "txt_AddWaterChoose")
                        {
                            chk_AddWaterChoose.Checked = (dt_formulahead.Rows[0][mDc].ToString() == "False" || dt_formulahead.Rows[0][mDc].ToString() == "0" ? false : true);
                        }
                    }
                    if (Convert.ToDouble(dt_formulahead.Rows[0][dt_formulahead.Columns["ObjectAddWaterWeight"]]) > 0)
                    {

                        double d_error = Lib_Card.Configure.Parameter.Other_AErr_DripWater;
                        if (!(dt_formulahead.Rows[0]["WaterStandError"] is DBNull))
                        {
                            d_error = Convert.ToDouble(dt_formulahead.Rows[0]["WaterStandError"]);
                        }
                        double d_totalWeight = Convert.ToDouble(dt_formulahead.Rows[0]["TotalWeight"]);
                        double d_allDif = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}",
                                    d_totalWeight * Convert.ToDouble(d_error / 100.00)) : string.Format("{0:F3}",
                                    d_totalWeight * Convert.ToDouble(d_error / 100.00)));

                        if ((int)(d_allDif*1000) < (int)(Math.Abs(Convert.ToDouble(dt_formulahead.Rows[0][dt_formulahead.Columns["RealAddWaterWeight"]]) - Convert.ToDouble(dt_formulahead.Rows[0][dt_formulahead.Columns["ObjectAddWaterWeight"]]))*1000))
                        {
                            txt_RealAddWaterWeight.BackColor = Color.Red;
                        }


                    }
                }

                

                if (Lib_Card.Configure.Parameter.Other_Language != 0)
                {
                    //中文换英文
                    if (txt_State.Text == "尚未滴液")
                    {
                        txt_State.Text = "Undropped";
                    }
                    else if (txt_State.Text == "已滴定配方")
                    {
                        txt_State.Text = "dropped";
                    }
                }

                s_totalFinishTime = dt_formulahead.Rows[0]["FinishTime"].ToString();



                for (int i = 0; i < dt_formuladetail.Rows.Count; i++)
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    {
                        dgv_Details.Rows.Add("滴液",
                        (i + 1).ToString(),
                        "滴液",
                        dt_formuladetail.Rows[i]["AssistantCode"] is DBNull ? "" : dt_formuladetail.Rows[i]["AssistantCode"].ToString(),
                        dt_formuladetail.Rows[i]["AssistantName"] is DBNull ? "" : dt_formuladetail.Rows[i]["AssistantName"].ToString(),
                        dt_formuladetail.Rows[i]["FormulaDosage"] is DBNull ? "" : dt_formuladetail.Rows[i]["FormulaDosage"].ToString(),
                        dt_formuladetail.Rows[i]["UnitOfAccount"] is DBNull ? "" : dt_formuladetail.Rows[i]["UnitOfAccount"].ToString(),
                        dt_formuladetail.Rows[i]["BottleNum"] is DBNull ? "" : dt_formuladetail.Rows[i]["BottleNum"].ToString(),
                        dt_formuladetail.Rows[i]["SettingConcentration"] is DBNull ? "" : dt_formuladetail.Rows[i]["SettingConcentration"].ToString(),
                        dt_formuladetail.Rows[i]["RealConcentration"] is DBNull ? "" : dt_formuladetail.Rows[i]["RealConcentration"].ToString(),
                        dt_formuladetail.Rows[i]["ObjectDropWeight"] is DBNull ? "" : dt_formuladetail.Rows[i]["ObjectDropWeight"].ToString(),
                        dt_formuladetail.Rows[i]["RealDropWeight"] is DBNull ? "" : dt_formuladetail.Rows[i]["RealDropWeight"].ToString(),
                        "",
                        "",
                        "",
                        "",
                        ""
                        );
                        if(dt_formuladetail.Rows[i]["ObjectDropWeight"] is DBNull || dt_formuladetail.Rows[i]["RealDropWeight"] is DBNull)
                        {
                            dgv_Details.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                        }
                        else
                        {
                            double d_error = Lib_Card.Configure.Parameter.Other_AErr_Drip;
                            if (!(dt_formuladetail.Rows[i]["StandError"] is DBNull))
                            {
                                d_error = Convert.ToDouble(dt_formuladetail.Rows[i]["StandError"]);
                            }
                            if((int)(Math.Abs(Convert.ToDouble(dt_formuladetail.Rows[i]["ObjectDropWeight"])- Convert.ToDouble(dt_formuladetail.Rows[i]["RealDropWeight"]))*1000)> (int)(d_error*1000))
                            {
                                dgv_Details.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                            }
                        }
                    }
                    else
                    {
                        dgv_Details.Rows.Add("Drip",
                        (i + 1).ToString(),
                        "Drip",
                        dt_formuladetail.Rows[i]["AssistantCode"] is DBNull ? "" : dt_formuladetail.Rows[i]["AssistantCode"].ToString(),
                        dt_formuladetail.Rows[i]["AssistantName"] is DBNull ? "" : dt_formuladetail.Rows[i]["AssistantName"].ToString(),
                        dt_formuladetail.Rows[i]["FormulaDosage"] is DBNull ? "" : dt_formuladetail.Rows[i]["FormulaDosage"].ToString(),
                        dt_formuladetail.Rows[i]["UnitOfAccount"] is DBNull ? "" : dt_formuladetail.Rows[i]["UnitOfAccount"].ToString(),
                        dt_formuladetail.Rows[i]["BottleNum"] is DBNull ? "" : dt_formuladetail.Rows[i]["BottleNum"].ToString(),
                        dt_formuladetail.Rows[i]["SettingConcentration"] is DBNull ? "" : dt_formuladetail.Rows[i]["SettingConcentration"].ToString(),
                        dt_formuladetail.Rows[i]["RealConcentration"] is DBNull ? "" : dt_formuladetail.Rows[i]["RealConcentration"].ToString(),
                        dt_formuladetail.Rows[i]["ObjectDropWeight"] is DBNull ? "" : dt_formuladetail.Rows[i]["ObjectDropWeight"].ToString(),
                        dt_formuladetail.Rows[i]["RealDropWeight"] is DBNull ? "" : dt_formuladetail.Rows[i]["RealDropWeight"].ToString(),
                        "",
                        "",
                        "",
                        "",
                        ""
                        );
                        if (dt_formuladetail.Rows[i]["ObjectDropWeight"] is DBNull || dt_formuladetail.Rows[i]["RealDropWeight"] is DBNull)
                        {
                            dgv_Details.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                        }
                        else
                        {
                            double d_error = Lib_Card.Configure.Parameter.Other_AErr_Drip;
                            if (!(dt_formuladetail.Rows[i]["StandError"] is DBNull))
                            {
                                d_error = Convert.ToDouble(dt_formuladetail.Rows[i]["StandError"]);
                            }
                            if ((int)(Math.Abs(Convert.ToDouble(dt_formuladetail.Rows[i]["ObjectDropWeight"]) - Convert.ToDouble(dt_formuladetail.Rows[i]["RealDropWeight"])) * 1000) > (int)(d_error * 1000))
                            {
                                dgv_Details.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                            }
                        }
                    }
                }
                for (int i = 0; i < dt_dye_details.Rows.Count; i++)
                {
                    s_finishTime = dt_dye_details.Rows[i]["FinishTime"] is DBNull ? "" : dt_dye_details.Rows[i]["FinishTime"].ToString();
                    string s_ti = "";
                    if (s_finishTime != "")
                    {
                        s_totalFinishTime = s_finishTime;
                    }
                    if (dt_dye_details.Rows[i]["StartTime"] is DBNull || s_finishTime == "")
                    {
                        s_ti = "";
                    }
                    else
                    {
                        DateTime dateTime1 = Convert.ToDateTime(dt_dye_details.Rows[i]["StartTime"].ToString());
                        DateTime dateTime2 = Convert.ToDateTime(s_finishTime);
                        TimeSpan ts = dateTime2 - dateTime1;
                        string s_temp = Convert.ToInt32(ts.TotalSeconds) / 60 / 60 + ":" + Convert.ToInt32(ts.TotalSeconds) % (60 * 60) / 60 + ":" + Convert.ToInt32(ts.TotalSeconds) % (60 * 60) % 60 ;
                        s_ti = s_temp;
                    }

                    if (dt_dye_details.Rows[i]["TechnologyName"].ToString() == "冷行" || dt_dye_details.Rows[i]["TechnologyName"].ToString() == "排液" || dt_dye_details.Rows[i]["TechnologyName"].ToString() == "洗杯" || dt_dye_details.Rows[i]["TechnologyName"].ToString() == "搅拌")
                    {
                        dgv_Details.Rows.Add(dt_dye_details.Rows[i]["Code"] is DBNull ? "" : dt_dye_details.Rows[i]["Code"].ToString(),
                            (i + 1 + dt_formuladetail.Rows.Count).ToString(),
                            dt_dye_details.Rows[i]["TechnologyName"] is DBNull ? "" : dt_dye_details.Rows[i]["TechnologyName"].ToString(),
                       "",
                       "",
                       "",
                       "",
                       "",
                       "",
                       "",
                       "",
                       "",
                       "",
                       "",
                       dt_dye_details.Rows[i]["Time"] is DBNull ? "" : dt_dye_details.Rows[i]["Time"].ToString(),
                       "",
                       dt_dye_details.Rows[i]["RotorSpeed"] is DBNull ? "" : dt_dye_details.Rows[i]["RotorSpeed"].ToString(),
                       dt_dye_details.Rows[i]["StartTime"] is DBNull ? "" : Convert.ToDateTime(dt_dye_details.Rows[i]["StartTime"].ToString()).ToString("T"),
                       s_ti
                       );
                    }
                    else if (dt_dye_details.Rows[i]["TechnologyName"].ToString() == "加水")
                    {
                        dgv_Details.Rows.Add(dt_dye_details.Rows[i]["Code"] is DBNull ? "" : dt_dye_details.Rows[i]["Code"].ToString(),
                            (i + 1 + dt_formuladetail.Rows.Count).ToString(),
                            dt_dye_details.Rows[i]["TechnologyName"] is DBNull ? "" : dt_dye_details.Rows[i]["TechnologyName"].ToString(),
                       "", 
                       "",
                       "",
                       "",
                       "",
                       "",
                       "",
                       "",
                       "",
                       "",
                       "",
                       "",
                       dt_dye_details.Rows[i]["ObjectWaterWeight"] is DBNull ? "" : dt_dye_details.Rows[i]["ObjectWaterWeight"].ToString(),
                       dt_dye_details.Rows[i]["RotorSpeed"] is DBNull ? "" : dt_dye_details.Rows[i]["RotorSpeed"].ToString(),
                       dt_dye_details.Rows[i]["StartTime"] is DBNull ? "" : Convert.ToDateTime(dt_dye_details.Rows[i]["StartTime"].ToString()).ToString("T"),
                       s_ti
                       );
                    }
                    else if (dt_dye_details.Rows[i]["TechnologyName"].ToString() == "放布" || dt_dye_details.Rows[i]["TechnologyName"].ToString() == "出布"
                         || dt_dye_details.Rows[i]["TechnologyName"].ToString() == "测PH" || dt_dye_details.Rows[i]["TechnologyName"].ToString() == "取小样")
                    {
                        dgv_Details.Rows.Add(dt_dye_details.Rows[i]["Code"] is DBNull ? "" : dt_dye_details.Rows[i]["Code"].ToString(),
                            (i + 1 + dt_formuladetail.Rows.Count).ToString(),
                            dt_dye_details.Rows[i]["TechnologyName"] is DBNull ? "" : dt_dye_details.Rows[i]["TechnologyName"].ToString(),
                       "",
                       "",
                       "",
                       "",
                       "",
                       "",
                       "",
                       "",
                       "",
                       "",
                       "",
                       "",
                       "",
                       dt_dye_details.Rows[i]["RotorSpeed"] is DBNull ? "" : dt_dye_details.Rows[i]["RotorSpeed"].ToString(),
                       dt_dye_details.Rows[i]["StartTime"] is DBNull ? "" : Convert.ToDateTime(dt_dye_details.Rows[i]["StartTime"].ToString()).ToString("T"),
                       s_ti
                       );
                    }
                    else if (dt_dye_details.Rows[i]["TechnologyName"].ToString() == "温控")
                    {
                        dgv_Details.Rows.Add(dt_dye_details.Rows[i]["Code"] is DBNull ? "" : dt_dye_details.Rows[i]["Code"].ToString(),
                           (i + 1 + dt_formuladetail.Rows.Count).ToString(),
                           dt_dye_details.Rows[i]["TechnologyName"] is DBNull ? "" : dt_dye_details.Rows[i]["TechnologyName"].ToString(),
                      "",
                      "",
                      "",
                      "",
                      "",
                      "",
                      "",
                      "",
                      "",
                      dt_dye_details.Rows[i]["Temp"] is DBNull ? "" : dt_dye_details.Rows[i]["Temp"].ToString(),
                      dt_dye_details.Rows[i]["TempSpeed"] is DBNull ? "" : dt_dye_details.Rows[i]["TempSpeed"].ToString(),
                      dt_dye_details.Rows[i]["Time"] is DBNull ? "" : dt_dye_details.Rows[i]["Time"].ToString(),
                      "",
                      dt_dye_details.Rows[i]["RotorSpeed"] is DBNull ? "" : dt_dye_details.Rows[i]["RotorSpeed"].ToString(),
                       dt_dye_details.Rows[i]["StartTime"] is DBNull ? "" : Convert.ToDateTime(dt_dye_details.Rows[i]["StartTime"].ToString()).ToString("T"),
                       s_ti
                      );
                    }

                    //加药
                    else
                    {
                        double d_temp = 0.0;
                        if (dt_dye_details.Rows[i]["Compensation"] is DBNull)
                        {
                        }
                        else
                        {
                            d_temp = Convert.ToDouble(dt_dye_details.Rows[i]["Compensation"].ToString());
                        }
                        dgv_Details.Rows.Add(dt_dye_details.Rows[i]["Code"] is DBNull ? "" : dt_dye_details.Rows[i]["Code"].ToString(),
                           (i + 1 + dt_formuladetail.Rows.Count).ToString(),
                           dt_dye_details.Rows[i]["TechnologyName"] is DBNull ? "" : dt_dye_details.Rows[i]["TechnologyName"].ToString(),
                        dt_dye_details.Rows[i]["AssistantCode"] is DBNull ? "" : dt_dye_details.Rows[i]["AssistantCode"].ToString(),
                        dt_dye_details.Rows[i]["AssistantName"] is DBNull ? "" : dt_dye_details.Rows[i]["AssistantName"].ToString(),
                        dt_dye_details.Rows[i]["FormulaDosage"] is DBNull ? "" : dt_dye_details.Rows[i]["FormulaDosage"].ToString(),
                        dt_dye_details.Rows[i]["UnitOfAccount"] is DBNull ? "" : dt_dye_details.Rows[i]["UnitOfAccount"].ToString(),
                        dt_dye_details.Rows[i]["BottleNum"] is DBNull ? "" : dt_dye_details.Rows[i]["BottleNum"].ToString(),
                        dt_dye_details.Rows[i]["SettingConcentration"] is DBNull ? "" : dt_dye_details.Rows[i]["SettingConcentration"].ToString(),
                        dt_dye_details.Rows[i]["RealConcentration"] is DBNull ? "" : dt_dye_details.Rows[i]["RealConcentration"].ToString(),
                        dt_dye_details.Rows[i]["ObjectDropWeight"] is DBNull ? "" : (Convert.ToDouble(dt_dye_details.Rows[i]["ObjectDropWeight"].ToString()) + d_temp).ToString("F2") ,
                        dt_dye_details.Rows[i]["RealDropWeight"] is DBNull ? "" : dt_dye_details.Rows[i]["RealDropWeight"].ToString(),
                        "",
                        "",
                        "",
                      dt_dye_details.Rows[i]["ObjectWaterWeight"] is DBNull ? "" : dt_dye_details.Rows[i]["ObjectWaterWeight"].ToString(),
                      "",
                       dt_dye_details.Rows[i]["StartTime"] is DBNull ? "" : Convert.ToDateTime(dt_dye_details.Rows[i]["StartTime"].ToString()).ToString("T"),
                       s_ti
                      );
                        if (dt_dye_details.Rows[i]["Finish"].ToString() == "1")
                        {
                            if (dt_dye_details.Rows[i]["ObjectDropWeight"] is DBNull || dt_dye_details.Rows[i]["RealDropWeight"] is DBNull)
                            {
                                dgv_Details.Rows[i + dt_formuladetail.Rows.Count].DefaultCellStyle.BackColor = Color.Red;
                            }
                            else
                            {
                                double d_error = Lib_Card.Configure.Parameter.Other_AErr_Drip;
                                if (!(dt_dye_details.Rows[i]["StandError"] is DBNull))
                                {
                                    d_error = Convert.ToDouble(dt_dye_details.Rows[i]["StandError"]);
                                }
                                if ((int)(Math.Abs(Convert.ToDouble(dt_dye_details.Rows[i]["ObjectDropWeight"]) - Convert.ToDouble(dt_dye_details.Rows[i]["RealDropWeight"]))*1000) > (int)(d_error*1000))
                                {
                                    dgv_Details.Rows[i + dt_formuladetail.Rows.Count].DefaultCellStyle.BackColor = Color.Red;
                                }
                            }
                        }
                    }
                }

                if (dt_formulahead.Rows[0]["StartTime"] is DBNull || s_totalFinishTime == "")
                {
                    txt_TotalTime.Text = "";
                }
                else
                {
                    DateTime dateTime1 = Convert.ToDateTime(dt_formulahead.Rows[0]["StartTime"].ToString());
                    DateTime dateTime2 = Convert.ToDateTime(s_totalFinishTime);
                    TimeSpan ts = dateTime2 - dateTime1;
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    {
                        string s_temp = Convert.ToInt32(ts.TotalSeconds) / 60 / 60 + "时" + Convert.ToInt32(ts.TotalSeconds) % (60 * 60) / 60 + "分" + Convert.ToInt32(ts.TotalSeconds) % (60 * 60) % 60 + "秒";
                        txt_TotalTime.Text = s_temp;
                    }
                    else
                    {
                        string s_temp = Convert.ToInt32(ts.TotalSeconds) / 60 / 60 + "H" + Convert.ToInt32(ts.TotalSeconds) % (60 * 60) / 60 + "M" + Convert.ToInt32(ts.TotalSeconds) % (60 * 60) % 60 + "S";
                        txt_TotalTime.Text = s_temp;
                    }
                }
                if (dt_formulahead.Rows[0]["StartTime"] is DBNull)
                {
                    txt_Start.Text = "";
                }
                else
                {
                    txt_Start.Text = Convert.ToDateTime(dt_formulahead.Rows[0]["StartTime"].ToString()).ToString("d");
                    
                }

                ////读取曲线数据并显示
                //string sPro = FADM_Object.Communal._fadmSqlserver.GetImage(Convert.ToInt32(cup), batchname);
                //hslCurveHistory1.RemoveAllCurve();
                //hslCurveHistory1.RemoveAllMarkText();
                //hslCurveHistory1.RenderCurveUI(false);
                //if (sPro != "" && strFinishTime != "")
                //{
                //    DateTime dateTime = Convert.ToDateTime(strFinishTime);

                //    Show(sPro, dateTime);

                //    if (P_dt_formulahead.Rows[0]["MarkStep"] != System.DBNull.Value)
                //    {
                //        string sMarkStep = P_dt_formulahead.Rows[0]["MarkStep"].ToString();
                //        if (sMarkStep != "")
                //        {
                //            bool b = true;
                //            sMarkStep = sMarkStep.Substring(0, sMarkStep.Length - 1);
                //            if (sMarkStep != "")
                //            {
                //                string[] arr = sMarkStep.Split('@');
                //                for (int i = 0; i < arr.Count(); i++)
                //                {
                //                    string sName = arr[i].Substring(0, arr[i].IndexOf(","));
                //                    string sNum = arr[i].Substring(arr[i].IndexOf(",") + 1, arr[i].Count() - arr[i].IndexOf(",") - 1);

                //                    hslCurveHistory1.AddMarkText(new HslControls.HslMarkText()
                //                    {
                //                        Index = Convert.ToInt32(sNum) - 1,
                //                        CurveKey = "温度",
                //                        MarkText = sName,
                //                        PositionStyle = b ? MarkTextPositionStyle.Up : MarkTextPositionStyle.Down,
                //                        CircleBrush = Brushes.Red,
                //                        TextBrush = Brushes.Red
                //                    }); ; ;
                //                    b = !b;
                //                }
                //            }
                //        }
                //    }
                //}

                //读取曲线数据并显示
                string s_pro = FADM_Object.Communal._fadmSqlserver.GetImage(Convert.ToInt32(s_cup), s_batchname);
                if (s_pro != "" && s_finishTime != "")
                {
                    DateTime dateTime = Convert.ToDateTime(s_finishTime);

                    Show(s_pro, dateTime);

                    if (dt_formulahead.Rows[0]["MarkStep"] != System.DBNull.Value)
                    {
                        string s_markStep = dt_formulahead.Rows[0]["MarkStep"].ToString();
                        if (s_markStep != "")
                        {
                            s_markStep = s_markStep.Substring(0, s_markStep.Length - 1);
                            if (s_markStep != "")
                            {
                                string[] sa_arr = s_markStep.Split('@');
                                for (int i = 0; i < sa_arr.Count(); i++)
                                {
                                    string s_name = sa_arr[i].Substring(0, sa_arr[i].IndexOf(","));
                                    string s_num = sa_arr[i].Substring(sa_arr[i].IndexOf(",") + 1, sa_arr[i].Count() - sa_arr[i].IndexOf(",") - 1);

                                    if (Convert.ToInt32(s_num) <= chart.Series[0].Points.Count)
                                    {
                                        chart.Series[0].Points[Convert.ToInt32(s_num) - 1].MarkerColor = Color.Blue;
                                        chart.Series[0].Points[Convert.ToInt32(s_num) - 1].MarkerSize = 10;
                                        chart.Series[0].Points[Convert.ToInt32(s_num) - 1].MarkerStyle = MarkerStyle.Triangle;
                                        chart.Series[0].Points[Convert.ToInt32(s_num) - 1].Label = s_name;
                                        chart.Series[0].Points[Convert.ToInt32(s_num) - 1].Font = new Font("Consolas", 12f);
                                        chart.Series[0].Points[Convert.ToInt32(s_num) - 1].LabelForeColor = Color.Blue;
                                    }
                                }
                            }
                        }
                    }

                    ////获取批次资料表头
                    //string s_sql = "SELECT CupNum, FormulaCode, VersionNum" +
                    //                   " FROM drop_head where CupNum = " + CupNo + " ;";
                    //DataTable dt_formula = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                    List<ProcessStep> list = new List<ProcessStep>();

                    if (dt_dye_details.Rows.Count > 0)
                    {
                        string FormulaCode = dt_dye_details.Rows[0]["FormulaCode"].ToString();
                        string VersionNum = dt_dye_details.Rows[0]["VersionNum"].ToString();
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


                        AddSeries("理论", Color.Blue);

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
                        //double time = totalTimeInSeconds - newtotalTimeInSeconds;
                        // 获取当前时间
                        //DateTime now = DateTime.Now;
                        //TimeSpan duration = TimeSpan.FromSeconds(time);
                        //DateTime futureTime = now + duration;
                        //string cc = futureTime.ToString("HH:mm:ss");

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
                        legendItem.Cells.Add(LegendCellType.Text, $"{series.Name} - 总计: {totalTimeFormatted} ", ContentAlignment.MiddleLeft);
                        chart.Legends[0].CustomItems.Add(legendItem);

                        //chart.MouseClick += Chart1_MouseClick;

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

                dgv_Details.ClearSelection();
            }
            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.Message, "DetailsShow", MessageBoxButtons.OK, true);
            }
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

        private void dgv_DropRecord_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // DetailsShow();
        }

        //private void Show(string s, DateTime dateTime)
        //{
        //    s = s.Substring(0, s.Length - 1);
        //    string[] arr = s.Split('@');

        //    float[] temp = new float[arr.Count()];
        //    DateTime[] times = new DateTime[arr.Count()];

        //    for (int i = 0; i < arr.Count(); i++)
        //    {
        //        temp[i] = Convert.ToSingle(arr[i]);
        //        times[i] = dateTime.AddSeconds((i - arr.Count()) * 30);
        //    }

        //    // 显示出数据信息来
        //    Invoke(new Action(() =>
        //    {
        //        // 设置曲线属性，名称，数据，颜色，是否平滑，格式化显示文本
        //        hslCurveHistory1.SetLeftCurve("温度", temp, Color.Blue, HslControls.CurveStyle.Curve, "{0:F1} ℃");
        //        hslCurveHistory1.SetDateTimes(times);
        //        hslCurveHistory1.RenderCurveUI(true);
        //    }));
        //}

        private void Show(string s_data, DateTime dateTime)
        {
            s_data = s_data.Substring(0, s_data.Length - 1);
            string[] sa_arr = s_data.Split('@');

            times = new DateTime[sa_arr.Count()];
            for (int i = 0; i < sa_arr.Count(); i++)
            {
                if (FADM_Object.Communal._b_is60Mark)
                {
                    times[i] = dateTime.AddSeconds((i - sa_arr.Count()) * 60);
                }
                else
                {
                    times[i] = dateTime.AddSeconds((i - sa_arr.Count()) * 30);
                }
            }
            
                AddSeries("温度", Color.Red);

            Series series = chart.Series[0];
            //series.Points.AddXY(0, 0);

            for (int i = 0; i < sa_arr.Count(); i++)
            {
                series.Points.AddXY(Convert.ToDouble(i + 1), Convert.ToDouble(sa_arr[i]));
            }
            chart.MouseMove += new MouseEventHandler(chart1_MouseMove);

            chart.MouseWheel += new System.Windows.Forms.MouseEventHandler(chart1_Mouselheel);

            chart.MouseClick += Chart1_MouseClick;



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
            if (_b_show)
            {
                if (chart.Series.Count > 1)
                {
                    if (e.Location.X > 351 && e.Location.Y < 40)
                    {
                        //MessageBox.Show("隐藏！");
                        chart.Series[1].Points.Clear();
                    }
                }
                _b_show = !_b_show;
            }
            else
            {
                _b_show = !_b_show;
                DetailsShow();
            }
            /*if (legendRect.Contains(e.Location))
            {
                // 用户点击了图例
                MessageBox.Show("点击了图例！");
            }*/
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
                            toolTip1.SetToolTip(chart, string.Format("序号:{0},时间:{1}, 温度:{2}", chart.Series[0].Points[Convert.ToInt32(chart.ChartAreas[0].CursorX.Position.ToString()) - 1].XValue, times[Convert.ToInt32(chart.ChartAreas[0].CursorX.Position.ToString()) - 1].ToString(),
                            chart.Series[0].Points[Convert.ToInt32(chart.ChartAreas[0].CursorX.Position.ToString()) - 1].YValues[0]));
                        else
                            toolTip1.SetToolTip(chart, string.Format("Index:{0},Time:{1}, Temperature:{2}", chart.Series[0].Points[Convert.ToInt32(chart.ChartAreas[0].CursorX.Position.ToString()) - 1].XValue, times[Convert.ToInt32(chart.ChartAreas[0].CursorX.Position.ToString()) - 1].ToString(),
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

                if (FADM_Object.Communal._b_is60Mark)
                {
                    chartArea.AxisX.Title = @"时间(X60s)";
                }
                else
                {
                    chartArea.AxisX.Title = @"时间(X30s)";
                }
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

                if (FADM_Object.Communal._b_is60Mark)
                {
                    chartArea.AxisX.Title = @"Time(X60s)";
                }
                else
                {
                    chartArea.AxisX.Title = @"Time(X30s)";
                }
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

        private void dgv_DropRecord_CurrentCellChanged(object sender, EventArgs e)
        {
            if (dgv_DropRecord.CurrentRow != null)
                DetailsShow();
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            try
            {

                if (dgv_DropRecord.CurrentRow != null)
                {

                    //如果选中行
                    if (dgv_DropRecord.SelectedRows.Count > 0)
                    {
                        _s_formulaCode = dgv_DropRecord.CurrentRow.Cells[0].Value.ToString();
                        _s_versionNum = dgv_DropRecord.CurrentRow.Cells[1].Value.ToString();
                        string s_finishTime = dgv_DropRecord.CurrentRow.Cells[2].Value.ToString();
                        _s_cupNum = dgv_DropRecord.CurrentRow.Cells[3].Value.ToString();
                        _s_batch = dgv_DropRecord.CurrentRow.Cells[4].Value.ToString();

                        if (FADM_Object.Communal._i_LabMode == 0)
                        {
                            if (SmartDyeing.FADM_Object.Communal._helper.Send_MeasureCmd(EnumMeasure_Mode.SCI, 0))
                            {
                                //测量下发成功
                                btn_save.Visible = false;
                                _i_nCount = 0;
                                timer2.Enabled = true;
                            }
                            else
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    FADM_Form.CustomMessageBox.Show("测量下发失败", "温馨提示", MessageBoxButtons.OK, false);
                                else
                                    FADM_Form.CustomMessageBox.Show("Measurement distribution failed", "Tips", MessageBoxButtons.OK, false);

                            }
                        }
                        else if (FADM_Object.Communal._i_LabMode == 1)
                        {
                            if (SmartDyeing.FADM_Object.Communal._Comhelper.Send_MeasureCmd(EnumMeasure_Mode.SCI))
                            {
                                //测量下发成功
                                btn_save.Visible = false;
                                _i_nCount = 0;
                                timer2.Enabled = true;
                            }
                            else
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    FADM_Form.CustomMessageBox.Show("测量下发失败", "温馨提示", MessageBoxButtons.OK, false);
                                else
                                    FADM_Form.CustomMessageBox.Show("Measurement distribution failed", "Tips", MessageBoxButtons.OK, false);

                            }
                        }
                    }
                    else
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("请选择保存光谱记录", "温馨提示", MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("Please choose to save spectral records", "Tips", MessageBoxButtons.OK, false);
                    }
                }
                else
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show("请选择保存光谱记录", "温馨提示", MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("Please choose to save spectral records", "Tips", MessageBoxButtons.OK, false);
                }
            }
            catch { }
        }

        private void HistoryData_Load(object sender, EventArgs e)
        {
            //cmb_multilingual.SelectedIndex = 0;

            if (FADM_Object.Communal._b_isUseLAB && FADM_Object.Communal._i_LabMode == 0)
            {

                #region 蓝牙回调

                if (SmartDyeing.FADM_Object.Communal._helper.bleCode != null)
                {
                    //查找到的蓝牙设备
                    SmartDyeing.FADM_Object.Communal._helper.bleCode.Added = AddBluetoothDevice;

                    //停止查找蓝牙设备
                    SmartDyeing.FADM_Object.Communal._helper.bleCode.WatcherStopped = WatcherStopped;

                    //查找完设备完成
                    SmartDyeing.FADM_Object.Communal._helper.bleCode.EnumerationCompleted = EnumerationCompleted;
                }
                #endregion
            }

            if (FADM_Object.Communal._b_isUseLAB)
            {
                //订阅仪器连接状态
                DeviceCallback.ConnectionChangeCallback = (state) =>
                {
                    this.Invoke(new Action(() =>
                    {
                        if (FADM_Object.Communal._i_LabMode == 1)
                        {
                            if (state)
                            {
                                btn_save.Visible = true;
                            }
                        }
                        //if (cmb_multilingual.SelectedIndex == 0)
                        //{
                        //lab_state.Text = state ? "已连接" : "未连接";
                        //}
                        //else
                        //{
                        //    lab_state.Text = state ? "Connected" : "Not connected";
                        //}
                    }));

                };


                //订阅测量状态
                DeviceCallback.MeasureCallback = (state, result) =>
                {
                    this.Invoke(new Action(() =>
                    {
                        if (FADM_Object.Communal._i_LabMode == 0)
                        {
                            string s_msg = string.Empty;
                            if (state)
                            {
                                //if (cmb_multilingual.SelectedIndex == 0)
                                {
                                    string s_spectrum = string.Empty;
                                    foreach (var item in result.spectrums)
                                    {
                                        s_spectrum += "测量模式：" + item.measure_mode.ToString() + Environment.NewLine;
                                        float[] f = new float[item.spectral_data.Count() - 12];
                                        for (int i = 4; i < item.spectral_data.Count() - 8; i++)
                                        {
                                            f[i - 4] = item.spectral_data[i];
                                            s_msg += item.spectral_data[i].ToString() + ",";
                                        }
                                        s_spectrum += "光谱信息：" + string.Join(",", item.spectral_data) + Environment.NewLine;
                                    }
                                }

                            }
                            else
                            {
                                {
                                    s_msg = "测量失败" + Environment.NewLine + Environment.NewLine;
                                }
                            }

                            if (!s_msg.Contains("测量失败"))
                            {
                                //保存到数据库
                                string s_sql = "Update history_head set Spectrum = '" + s_msg +
                            "' WHERE BatchName = '" + _s_batch + "' and FormulaCode = '" + _s_formulaCode + "' and VersionNum = " + _s_versionNum + " and CupNum = " + _s_cupNum + ";";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                timer2.Enabled = false;
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    FADM_Form.CustomMessageBox.Show("保存成功", "温馨提示", MessageBoxButtons.OK, false);
                                else
                                    FADM_Form.CustomMessageBox.Show("Successfully saved", "Tips", MessageBoxButtons.OK, false);
                                btn_save.Visible = true;


                            }
                            else
                            {
                                timer2.Enabled = true;
                                //MessageBox.Show("测量失败");
                            }
                        }
                        else if (FADM_Object.Communal._i_LabMode == 1)
                        {
                            ColorFormula formula = new ColorFormula(EnumLightSource.D65, EnumAngle.TenDegree);
                            string s_msg;
                            if (state)
                            {
                                //if (cmb_multilingual.SelectedIndex == 0)
                                {
                                    string spectrum = string.Empty;
                                    foreach (var item in result.spectrums)
                                    {
                                        spectrum += "测量模式：" + item.measure_mode.ToString() + Environment.NewLine;

                                        formula.GetSpectrum2XYZ(item.spectral_data, out double _X, out double _Y, out double _Z);
                                        //if (chkLightSource.Checked)
                                        spectrum += "光源：" + (EnumLightSource.D65).ToDescription() + Environment.NewLine;
                                        //if (chkAngle.Checked)
                                        spectrum += "角度：" + (EnumAngle.TenDegree).ToDescription() + Environment.NewLine;
                                        //if (chkXYZ.Checked)
                                        spectrum += "XYZ：" + _X.ToString("F4") + " , " + _Y.ToString("F4") + " , " + _Z.ToString("F4") + Environment.NewLine;
                                        //if (chkLab.Checked)
                                        {
                                            formula.GetXYZ2LABCH(_X, _Y, _Z, out double _L, out double _a, out double _b, out double _C, out double _H);
                                            spectrum += "LabCH：" + _L.ToString("F4") + " , " + _a.ToString("F4") + " , " + _b.ToString("F4") + " , " + _C.ToString("F4") + " , " + _H.ToString("F4") + Environment.NewLine;
                                        }

                                        //if (chkSpectral.Checked)
                                        spectrum += "光谱信息：" + string.Join(",", item.spectral_data) + Environment.NewLine;
                                    }
                                    s_msg = $"测量成功{Environment.NewLine}UV模式：{result.uv_mode + Environment.NewLine}口径大小：{result.caliber_size + Environment.NewLine}开始波长：{result.wave_start + Environment.NewLine}波长间隔：{result.wave_interval + Environment.NewLine}波长数量：{result.wave_number + Environment.NewLine}{spectrum}";
                                }

                            }
                            else
                            {
                                {
                                    s_msg = "测量失败" + Environment.NewLine + Environment.NewLine;
                                }
                            }

                            if (!s_msg.Contains("测量失败"))
                            {
                                //保存到数据库
                                string s_sql = "Update history_head set Spectrum = '" + s_msg +
                            "' WHERE BatchName = '" + _s_batch + "' and FormulaCode = '" + _s_formulaCode + "' and VersionNum = " + _s_versionNum + " and CupNum = " + _s_cupNum + ";";
                                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                timer2.Enabled = false;
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    FADM_Form.CustomMessageBox.Show("保存成功", "温馨提示", MessageBoxButtons.OK, false);
                                else
                                    FADM_Form.CustomMessageBox.Show("Successfully saved", "Tips", MessageBoxButtons.OK, false);
                                btn_save.Visible = true;


                            }
                            else
                            {
                                timer2.Enabled = true;
                                //MessageBox.Show("测量失败");
                            }
                        }



                        //txt_TotalTime.Text = msg ;

                    }));
                };

                //订阅校准状态
                DeviceCallback.CalibrateCallback = (state) =>
                {
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show(state ? "校准成功" : "校准失败", "温馨提示", MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show(state ? "Calibration Successful" : "Calibration Failed", "Tips", MessageBoxButtons.OK, false);
                    }
                };

                Thread P_thd2 = new Thread(ThreadBlueToothCon);
                P_thd2.IsBackground = true;
                P_thd2.Start();

                if (FADM_Object.Communal._i_LabMode == 1)
                {
                    timer1.Enabled = false;
                }
            }
            else
            {
                timer1.Enabled = false;
                timer2.Enabled = false;
            }
        }

        /// <summary>
        /// 新增蓝牙设备
        /// </summary>
        private void AddBluetoothDevice(DeviceInfo data)
        {
            if (!IsHandleCreated) return;


            DeviceInfo deviceInfo = new DeviceInfo()
            {
                Address = data.Address,
                DeviceId = data.DeviceId,
                IsPaired = data.IsPaired,
                Name = data.Name,
                State = ConnectionStatus.Disconnected,
                Type = data.Type,
            };
            if (!bluetoothList.Contains(deviceInfo))
            {
                bluetoothList.Add(deviceInfo);
                this.Invoke(new Action(() =>
                {
                    if (!IsHandleCreated) return;

                    //listBox1.Items.Add(deviceInfo.Name);
                    _lis_bluetoothNane.Add(deviceInfo.Name);
                }));
            }
        }

        /// <summary>
        /// 停止查找蓝牙设备
        /// </summary>
        private void WatcherStopped()
        {
            if (!IsHandleCreated) return;

            this.Invoke(new Action(() =>
            {

            }));
        }

        /// <summary>
        /// 查找完设备完成
        /// </summary>
        private void EnumerationCompleted()
        {
            if (!IsHandleCreated) return;
            this.Invoke(new Action(() =>
            {

            }));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(_b_isShowButton || SmartDyeing.FADM_Object.Communal._helper.bleCode.IsConnected)
            {
                btn_save.Visible = true;
            }
            timer1.Enabled= false;
        }

        private void Btn_Derive_Click(object sender, EventArgs e)
        {
            string s_filePath = Environment.CurrentDirectory + "\\App_Data\\" + "自检数据.txt";
            if (File.Exists(s_filePath))
            {
                File.Delete(s_filePath);
            }

            string s_sql = null;
            string s_str = null;
            DataTable dt_data = new DataTable();

            string s_strTemp = null; ;
            if (dt_Record_Start.Text != null && dt_Record_Start.Text != "")
            {
                s_strTemp += (" Date >= '" + dt_Record_Start.Text + "' AND");
            }
            else
            {
                return;
            }

            if (dt_Record_End.Text != null && dt_Record_End.Text != "")
            {
                s_strTemp += (" Date <= '" + dt_Record_End.Text + "' ");
            }
            else
            {
                return;
            }


            s_sql = "SELECT * FROM self_table WHERE" + s_strTemp;
            ;
            dt_data = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);


            s_str += "        自检时间  ";
            s_str += "         瓶号";
            s_str += "    自检1";
            s_str += "    自检2";
            s_str += "    自检3";
            s_str += "    自检4";
            s_str += "    针检重量";
            s_str += "    脉冲";
            s_str += "\n";

            for (int i = 0; i < dt_data.Rows.Count; i++)
            {
                s_str += dt_data.Rows[i]["Date"].ToString();
                s_str += "    ";
                s_str += dt_data.Rows[i]["BottleNum"].ToString();
                s_str += "      ";
                s_str += dt_data.Rows[i]["SelfChecking1"].ToString();
                s_str += "    ";
                s_str += dt_data.Rows[i]["SelfChecking2"].ToString();
                s_str += "      ";
                s_str += dt_data.Rows[i]["SelfChecking3"].ToString();
                s_str += "      ";
                s_str += dt_data.Rows[i]["SelfChecking4"].ToString();
                s_str += "        ";
                s_str += dt_data.Rows[i]["CurrentAdjustWeight"].ToString();
                s_str += "        ";
                s_str += dt_data.Rows[i]["AdjustValue"].ToString();
                s_str += "\n";
            }
            byte[] byta_self = System.Text.Encoding.Default.GetBytes(s_str.ToString());
            FileStream fs = new FileStream(s_filePath, FileMode.Append, FileAccess.Write);
            fs.Write(byta_self, 0, byta_self.Length);
            fs.Close();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (FADM_Object.Communal._i_LabMode == 1)
            {
                _i_nCount++;
                if (_i_nCount > 1)
                {
                    timer2.Enabled = false;
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show("测量失败", "温馨提示", MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("Measurement failed", "Tips", MessageBoxButtons.OK, false);
                    btn_save.Visible = true;

                }
                else
                {
                    //再次发送测量命令
                    //if (SmartDyeing.FADM_Object.Communal._helper.bleCode.IsConnected)
                    {
                        if (SmartDyeing.FADM_Object.Communal._Comhelper.Send_MeasureCmd(EnumMeasure_Mode.SCI))
                        {
                            //发送测量信息
                        }
                    }
                }
            }
            else if (FADM_Object.Communal._i_LabMode == 0)
            {
                _i_nCount++;
                if (_i_nCount > 1)
                {
                    timer2.Enabled = false;
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show("测量失败", "温馨提示", MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("Measurement failed", "Tips", MessageBoxButtons.OK, false);
                    btn_save.Visible = true;

                }
                else
                {
                    //再次发送测量命令
                    if (SmartDyeing.FADM_Object.Communal._helper.bleCode.IsConnected)
                    {
                        if (SmartDyeing.FADM_Object.Communal._helper.Send_MeasureCmd(EnumMeasure_Mode.SCI, 0))
                        {
                            //发送测量信息
                        }
                    }
                }
            }
        }

        private void btn_Record_Print_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgv_DropRecord.CurrentRow != null)
                {
                    if (dgv_DropRecord.SelectedRows.Count > 0)
                    {
                        string s_formulaCode = dgv_DropRecord.CurrentRow.Cells[0].Value.ToString();
                        string s_versionNum = dgv_DropRecord.CurrentRow.Cells[1].Value.ToString();
                        string s_finishTime = dgv_DropRecord.CurrentRow.Cells[2].Value.ToString();
                        string s_cupNum = dgv_DropRecord.CurrentRow.Cells[3].Value.ToString();
                        string s_batch = dgv_DropRecord.CurrentRow.Cells[4].Value.ToString();

                        FADM_Form.ReportPrint r = new ReportPrint(s_batch, s_cupNum);
                        r.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.Message, "btn_Record_Print_Click", MessageBoxButtons.OK, true);
            }
        }

        private void dy_nodelist_comboBox2_KeyUp(object sender, KeyEventArgs e)
        {

            if (triggerKeyPress)
            {
                triggerKeyPress = false;
                return;
            }
            System.Windows.Forms.TextBox cc = (System.Windows.Forms.TextBox)sender;
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Left)
            {
                if (noActivateFormsList[Convert.ToString(cc.Name)].lb_End_Stations.SelectedIndex > 0)
                    noActivateFormsList[Convert.ToString(cc.Name)].lb_End_Stations.SelectedIndex--;
            }
            else if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Right)
            {
                if (noActivateFormsList[Convert.ToString(cc.Name)].lb_End_Stations.SelectedIndex < noActivateFormsList[Convert.ToString(cc.Name)].lb_End_Stations.Items.Count - 1)
                    noActivateFormsList[Convert.ToString(cc.Name)].lb_End_Stations.SelectedIndex++;
            }//回车
            else if (e.KeyCode == Keys.Enter)
            {
                if (noActivateFormsList.ContainsKey(Convert.ToString(cc.Name)) && noActivateFormsList[Convert.ToString(cc.Name)].Focused && noActivateFormsList[Convert.ToString(cc.Name)].Visible)
                {
                    string info = noActivateFormsList[Convert.ToString(cc.Name)].lb_End_Stations.SelectedItem as string;
                    txt_Record_Code.Text = info;

                }
                else if (noActivateFormsList.ContainsKey(Convert.ToString(cc.Name)) && !noActivateFormsList[Convert.ToString(cc.Name)].Focused)
                {
                    //noActivateFormsList[Convert.ToInt32(cc.Name)].TopMost = true;
                    //noActivateFormsList[Convert.ToInt32(cc.Name)].Focus();
                    noActivateFormsList[Convert.ToString(cc.Name)].Visible = false;
                    noActivateFormsList[Convert.ToString(cc.Name)].Close();
                    NoActivateForm n = null;
                    n = new NoActivateForm();
                    n.Width = cc.Width;
                    n.lb_End_Stations.Width = cc.Width;
                    n.Width = cc.Width;
                    Point button1ScreenPos = cc.PointToScreen(Point.Empty);
                    n.lb_End_Stations.AccessibleName = cc.Name;
                    n.lb_End_Stations.KeyDown += new KeyEventHandler(tttt);
                    n.lb_End_Stations.DoubleClick += new EventHandler(listBox1_DoubleClick);
                    n.StartPosition = FormStartPosition.Manual;
                    n.Location = new Point(button1ScreenPos.X, button1ScreenPos.Y + 27);
                    n.Show();
                    noActivateFormsList[Convert.ToString(cc.Name)] = n;
                    IList<string> list = GetStations(cc.Text);
                    if (list.Count > 0)
                    {
                        n.lb_End_Stations.DataSource = list;
                    }
                }
                else if (!noActivateFormsList.ContainsKey(Convert.ToString(cc.Name)))
                {
                    NoActivateForm n = null;
                    n = new NoActivateForm();
                    n.Width = cc.Width;
                    n.lb_End_Stations.Width = cc.Width;
                    n.lb_End_Stations.AccessibleName = cc.Name;
                    Point button1ScreenPos = cc.PointToScreen(Point.Empty);
                    n.lb_End_Stations.AccessibleName = cc.Name;
                    n.lb_End_Stations.KeyDown += new KeyEventHandler(tttt);
                    n.lb_End_Stations.DoubleClick += new EventHandler(listBox1_DoubleClick);
                    n.StartPosition = FormStartPosition.Manual;
                    n.Location = new Point(button1ScreenPos.X, button1ScreenPos.Y + 27);
                    n.Show();
                    noActivateFormsList.Add(Convert.ToString(cc.Name), n);
                    IList<string> list = GetStations(cc.Text);
                    if (list.Count > 0)
                    {
                        n.lb_End_Stations.DataSource = list;
                    }

                }
            }
            else
            {
                Point button1ScreenPos = cc.PointToScreen(Point.Empty);
                NoActivateForm n = null;
                //noActivateFormsList.Add(Convert.ToInt32(cc.Name), n);

                if (noActivateFormsList.ContainsKey(Convert.ToString(cc.Name)))
                {
                    noActivateFormsList[Convert.ToString(cc.Name)].Visible = false;
                    noActivateFormsList[Convert.ToString(cc.Name)].Close();

                    n = noActivateFormsList[Convert.ToString(cc.Name)];
                    n = new NoActivateForm();
                    n.Width = cc.Width;
                    n.lb_End_Stations.Width = cc.Width;
                    n.lb_End_Stations.AccessibleName = cc.Name;
                    noActivateFormsList[Convert.ToString(cc.Name)] = n;
                }
                else
                {
                    n = new NoActivateForm();
                    n.Width = cc.Width;
                    n.lb_End_Stations.Width = cc.Width;
                    n.lb_End_Stations.AccessibleName = cc.Name;
                    noActivateFormsList.Add(Convert.ToString(cc.Name), n);
                }

                //n.lb_End_Stations.DrawItem += new DrawItemEventHandler(ListBox_StationDatas_DrawItem);
                //n.Focus();
                n.lb_End_Stations.KeyDown += new KeyEventHandler(tttt);
                n.lb_End_Stations.DoubleClick += new EventHandler(listBox1_DoubleClick);
                n.StartPosition = FormStartPosition.Manual;
                n.Location = new Point(button1ScreenPos.X, button1ScreenPos.Y + 27);
                n.Show();

                IList<string> list = GetStations(cc.Text);
                if (list.Count > 0)
                {
                    n.lb_End_Stations.DataSource = list;
                }
            }
        }

        private void tttt(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                System.Windows.Forms.ListBox cc = (System.Windows.Forms.ListBox)sender;
                string info = noActivateFormsList[Convert.ToString(cc.AccessibleName)].lb_End_Stations.SelectedItem as string;
                txt_Record_Code.Text = info;
                noActivateFormsList[Convert.ToString(cc.AccessibleName)].Visible = false;
                noActivateFormsList[Convert.ToString(cc.AccessibleName)].Close();
            }
        }
        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            System.Windows.Forms.ListBox cc = (System.Windows.Forms.ListBox)sender;
            // 确保双击的是ListBox中的项
            if (cc.SelectedIndex != -1)
            {
                // 获取被双击的项
                string selectedItem = cc.SelectedItem.ToString();

                txt_Record_Code.Text = selectedItem;
                noActivateFormsList[Convert.ToString(cc.AccessibleName)].Visible = false;
                noActivateFormsList[Convert.ToString(cc.AccessibleName)].Close();
                // 可以在这里添加代码来处理双击事件，例如弹出消息框显示项
            }
        }

        public IList<string> GetStations(string filter)
        {
            IList<string> results = new List<string>();
            string s_sql = "SELECT DISTINCT FormulaCode" +
                               "  FROM history_head " +
                               " group BY FormulaCode ;";
            DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            foreach (DataRow dr in dt_data.Rows)
            {
                results.Add(Convert.ToString(dr[0]));
            }
            /*return results.Where(
                f =>
                (f.Substring(0, filter.Length) == filter)).ToList<string>();*/

            return results.Where(item => item.Contains(filter)).ToList();

        }
        private void dy_nodelist_comboBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox box = (TextBox)sender;

            switch (e.KeyChar)
            {
                case (char)Keys.Enter:
                    triggerKeyPress = true;
                    string type = box.Name; //dy_type_comboBox1.Text;
                    if (noActivateFormsList.ContainsKey(Convert.ToString(box.Name)) && noActivateFormsList[Convert.ToString(box.Name)].Visible)
                    {
                        string info = noActivateFormsList[Convert.ToString(box.Name)].lb_End_Stations.SelectedItem as string;
                        box.Text = info;
                        box.SelectionStart = box.Text.Length;
                        box.SelectionLength = 0;
                        noActivateFormsList[Convert.ToString(box.Name)].Visible = false;
                        noActivateFormsList[Convert.ToString(box.Name)].Close();
                        DropRecordHeadShow();
                       // e.Handled = true;
                    }
                    break;
                default:
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {

                if (dgv_DropRecord.CurrentRow != null)
                {

                    //如果选中行
                    if (dgv_DropRecord.SelectedRows.Count > 0)
                    {
                        _s_formulaCode = dgv_DropRecord.CurrentRow.Cells[0].Value.ToString();
                        _s_versionNum = dgv_DropRecord.CurrentRow.Cells[1].Value.ToString();
                        string s_finishTime = dgv_DropRecord.CurrentRow.Cells[2].Value.ToString();
                        _s_cupNum = dgv_DropRecord.CurrentRow.Cells[3].Value.ToString();
                        _s_batch = dgv_DropRecord.CurrentRow.Cells[4].Value.ToString();

                        string sql = "Update history_head set Result = '" + txt_Result.Text + "' where BatchName = '" + _s_batch + "' And CupNum = " + _s_cupNum;
                        FADM_Object.Communal._fadmSqlserver.ReviseData(sql);

                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("保存完成", "温馨提示", MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("Save Finish", "Tips", MessageBoxButtons.OK, false);
                    }
                    else
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("请选择保存记录", "温馨提示", MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("Please choose to save  records", "Tips", MessageBoxButtons.OK, false);
                    }
                }
               
            }
            catch { }
        }

        private void Btn_PDF_Click(object sender, EventArgs e)
        {
            try
            {
                //// 执行导出
                string outputPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";
                var exporter = new PdfTableExporter();

                DataTable dt_data = new DataTable();
                string s_sql = "";

                //获取配方浏览资料表头
                if (rdo_Record_Now.Checked)
                {
                    s_sql = "SELECT StartTime as 时间,FormulaCode as 配方代码, ClothType as 布种, FormulaName as 配方名称, IsAutoIn as 自动, Result as 备注,Operator as 操作员 FROM history_head WHERE" +
                                " FinishTime > CONVERT(varchar,GETDATE(),23) ORDER BY MyID DESC;";
                    dt_data = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                }
                else if (rdo_Record_All.Checked)
                {
                    s_sql = "SELECT StartTime as 时间,FormulaCode as 配方代码, ClothType as 布种, FormulaName as 配方名称, IsAutoIn as 自动, Result as 备注,Operator as 操作员 FROM history_head" +
                                " WHERE FinishTime != '' ORDER BY MyID DESC;";
                    dt_data = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                }
                else
                {
                    string s_str = null;
                    if (txt_Record_Operator.Text != null && txt_Record_Operator.Text != "")
                    {
                        s_str = (" Operator = '" + txt_Record_Operator.Text + "' AND");
                    }
                    if (txt_Record_CupNum.Text != null && txt_Record_CupNum.Text != "")
                    {
                        s_str = (" CupNum = '" + txt_Record_CupNum.Text + "' AND");
                    }
                    if (txt_Record_Code.Text != null && txt_Record_Code.Text != "")
                    {
                        s_str += (" FormulaCode = '" + txt_Record_Code.Text + "' AND");
                    }
                    if (dt_Record_Start.Text != null && dt_Record_Start.Text != "")
                    {
                        s_str += (" FinishTime >= '" + dt_Record_Start.Text + "' AND");
                    }
                    else
                    {
                        return;
                    }

                    if (dt_Record_End.Text != null && dt_Record_End.Text != "")
                    {
                        s_str += (" FinishTime <= '" + dt_Record_End.Text + "' ");
                    }
                    else
                    {
                        return;
                    }

                    s_sql = "SELECT StartTime as 时间,FormulaCode as 配方代码, ClothType as 布种, FormulaName as 配方名称, IsAutoIn as 自动, Result as 备注,Operator as 操作员 FROM history_head Where" + s_str + "" +
                               " ORDER BY MyID DESC;";
                    dt_data = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                }

                exporter.Demo(dt_data, outputPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"导出失败: {ex.Message}");
            }

            return;
        }
    }
}
