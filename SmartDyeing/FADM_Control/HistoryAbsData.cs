using BLECode;
using CHNSpec.Device.Bluetooth;
using CHNSpec.Device.Models;
using CHNSpec.Device.Models.Enums;
using HslControls;
using HslControls.Charts;
using Lib_DataBank.MySQL;
using Lib_File;
using SmartDyeing.FADM_Auto;
using SmartDyeing.FADM_Form;
using SmartDyeing.FADM_Object;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Resources;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SmartDyeing.FADM_Control
{
    // 存储点数据的类

    public partial class HistoryAbsData : UserControl
    {
        // 存储所有点的列表
        private List<LabPoint> _points = new List<LabPoint>();
        private int _nextPointIndex = 1;

        //// 输入控件
        //private TextBox txtA;
        //private TextBox txtB;
        //private Button btnAddPoint;
        //private Button btnClearPoints;
        private ListBox lstPoints;
        private Panel _panel3; // 绘图专用的 Panel

        DateTime[] times;

        int i_start;
        int i_end;
        int i_int;

        bool b_isRecord = true;

        Main _main;
        public HistoryAbsData(Main m)
        {
            try
            {
                InitializeComponent();
                InitializeComponents();

                //if (double.TryParse("50", out double a) && double.TryParse("60", out double b))
                //{
                //    // 限制 a/b 范围在 [-128, 127]
                //    a = Clamp(a, -128, 127);
                //    b = Clamp(b, -128, 127);

                //    _points.Add(new LabPoint(a, b, _nextPointIndex));
                //    lstPoints.Items.Add($"点 {_nextPointIndex}: a={a:F1}, b={b:F1}");
                //    _nextPointIndex++;

                //    _panel3.Invalidate(); // 触发 Panel 重绘
                //}
                //else
                //{
                //    //MessageBox.Show("请输入有效的数字！");
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            _main = m;

            //ShowHeader();
            InitChart();

        }

        private void btn_Record_Select_Click(object sender, EventArgs e)
        {
            ShowHeader();
            DropRecordHeadShow();
        }

        private void btn_Record_Delete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgv_DropRecord.CurrentRow != null)
                {

                    //如果选中行
                    if (dgv_DropRecord.SelectedRows.Count > 0)
                    {

                        string s_finishtime = dgv_DropRecord.CurrentRow.Cells[3].Value.ToString();
                        string s_assistantCode = dgv_DropRecord.CurrentRow.Cells[0].Value.ToString();

                        string s_sql = "SELECT * FROM history_abs" +
                                   " Where FinishTime = '" + s_finishtime + "';";
                        DataTable dt_history_abs = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                        if (dt_history_abs.Rows.Count > 0)
                        {
                            DateTime dateTime = Convert.ToDateTime(dt_history_abs.Rows[0]["FinishTime"].ToString());
                            string s_time = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                            s_time = s_time.Replace(" ", "");
                            s_time = s_time.Replace("-", "");
                            s_time = s_time.Replace(":", "");

                            s_sql = "SELECT *  FROM assistant_details WHERE AssistantCode = '" + s_assistantCode + "';";
                            DataTable dt_assistant_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                            //MessageBox.Show(s_time);
                            string s_assName = "";
                            if (dt_assistant_details.Rows.Count > 0)
                            {
                                s_assName = dt_assistant_details.Rows[0]["AssistantName"].ToString();
                            }
                            string s_data = dt_history_abs.Rows[0]["Abs"] is DBNull ? "" : dt_history_abs.Rows[0]["Abs"].ToString();
                            if (s_data != "")
                                s_data = s_data.Substring(0, s_data.Length - 2);
                            string[] sa_arr = s_data.Split('/');

                            if (s_data != "")
                            {
                                using (StreamWriter writer = new StreamWriter(FADM_Object.Communal._s_absPath + s_assistantCode + "-" + dt_history_abs.Rows[0]["BottleNum"].ToString() + "-" + s_time + ".JTDAT"))
                                {
                                    string s_h = "                \"Time\":\" " + dateTime.ToString("yyyy-MM-dd    HH:mm:ss").Substring(2, 17) + "\",\n";
                                    string s = "[";
                                    for (int i = 0; i < sa_arr.Count(); i++)
                                    {

                                        s += "[" + (Convert.ToInt32(dt_history_abs.Rows[0]["StartWave"]) + i * Convert.ToInt32(dt_history_abs.Rows[0]["IntWave"])) + "," + sa_arr[i] + "],";
                                    }
                                    s = s.Substring(0, s.Length - 1);
                                    s += "]";

                                    string s_w = "{\n" + s_h + "                " + "\"Data\":" + s + "\n}";
                                    //输出抬头明细
                                    writer.WriteLine(s_w);

                                    FADM_Form.CustomMessageBox.Show("导出成功", "温馨提示", MessageBoxButtons.OK, true);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show("导出失败", "温馨提示", MessageBoxButtons.OK, true);
            }


        }

        private void ShowHeader()
        {

            dgv_DropRecord.Columns.Clear();
            dgv_DropRecord.Columns.Add("", "");
            dgv_DropRecord.Columns.Add("", "");
            dgv_DropRecord.Columns.Add("", "");
            dgv_DropRecord.Columns.Add("", "");
            dgv_DropRecord.Columns.Add("", "");
            dgv_DropRecord.Columns.Add("", "");
            dgv_DropRecord.Columns.Add("", "");


            if (Lib_Card.Configure.Parameter.Other_Language == 0)
            {
                //设置标题文字
                dgv_DropRecord.Columns[0].HeaderCell.Value = "助剂代码";
                dgv_DropRecord.Columns[1].HeaderCell.Value = "工位";
                dgv_DropRecord.Columns[2].HeaderCell.Value = "瓶号";
                dgv_DropRecord.Columns[3].HeaderCell.Value = "时间";
                dgv_DropRecord.Columns[4].HeaderCell.Value = "L";
                dgv_DropRecord.Columns[5].HeaderCell.Value = "A";
                dgv_DropRecord.Columns[6].HeaderCell.Value = "B";
                //设置标题字体
                dgv_DropRecord.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 14.25F);
                //设置内容字体
                dgv_DropRecord.RowsDefaultCellStyle.Font = new Font("宋体", 14.25F);
            }
            else
            {
                dgv_DropRecord.Columns[0].HeaderCell.Value = "AssistantCode";
                dgv_DropRecord.Columns[1].HeaderCell.Value = "CupNumber";
                dgv_DropRecord.Columns[2].HeaderCell.Value = "BottleNumber";
                dgv_DropRecord.Columns[3].HeaderCell.Value = "Time";
                dgv_DropRecord.Columns[4].HeaderCell.Value = "L";
                dgv_DropRecord.Columns[5].HeaderCell.Value = "A";
                dgv_DropRecord.Columns[6].HeaderCell.Value = "B";
                //设置标题字体
                dgv_DropRecord.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 7.5F);
                //设置内容字体
                dgv_DropRecord.RowsDefaultCellStyle.Font = new Font("宋体", 10.5F);
            }


            //设置标题宽度
            dgv_DropRecord.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgv_DropRecord.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgv_DropRecord.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgv_DropRecord.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgv_DropRecord.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgv_DropRecord.Columns[5].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgv_DropRecord.Columns[6].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;


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

            dgv_DropRecord.ClearSelection();

            //dataGridView1.Columns.Clear();
            //if (Lib_Card.Configure.Parameter.Other_Language == 0)
            //{
            //    string[] sa_lineName = { "助染剂代码", "助染剂名称", "用量", "单位", "瓶号", "设定浓度", "实际浓度", "目标滴液", "实际滴液" };
            //    for (int i = 0; i < sa_lineName.Count(); i++)
            //    {
            //        dataGridView1.Columns.Add("", "");
            //        dataGridView1.Columns[i].HeaderCell.Value = sa_lineName[i];
            //        //关闭点击标题自动排序功能
            //        dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;

            //        dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;


            //    }
            //    //设置标题字体
            //    dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 14.25F);
            //    //设置内容字体
            //    dataGridView1.RowsDefaultCellStyle.Font = new Font("宋体", 14.25F);
            //}
            //else
            //{
            //    string[] sa_lineName = { "DyeingAuxiliariesCode", "DyeingAuxiliariesName", "DosageOfFormula", "Units", "BottleNumber", "SetConcentration", "ActualConcentration", "TargetVolume", "ActualVolume" };
            //    for (int i = 0; i < sa_lineName.Count(); i++)
            //    {
            //        dataGridView1.Columns.Add("", "");
            //        dataGridView1.Columns[i].HeaderCell.Value = sa_lineName[i];
            //        //关闭点击标题自动排序功能
            //        dataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            //        dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            //    }
            //    //设置标题字体
            //    dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 7.5F);
            //    //设置内容字体
            //    dataGridView1.RowsDefaultCellStyle.Font = new Font("宋体", 10.5F);
            //}
            ////设置标题居中显示
            //dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;



            ////设置内容居中显示
            //dataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;



            //设置行高
            //dataGridView1.RowTemplate.Height = 30;



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
                    s_sql = "SELECT AssistantCode,CupNum, BottleNum, FinishTime,Stand,L,A,B FROM history_abs WHERE" +
                                " FinishTime > CONVERT(varchar,GETDATE(),23) And Type !=2 And Type !=4 ORDER BY FinishTime DESC;";
                    dt_data = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                }
                else if (rdo_Record_All.Checked)
                {
                    s_sql = "SELECT AssistantCode,CupNum, BottleNum, FinishTime,Stand,L,A,B FROM history_abs " +
                                " WHERE FinishTime != ''  And Type !=2 And Type !=4 ORDER BY FinishTime DESC;";
                    dt_data = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                }
                else if(rdo_Record_condition.Checked)
                {
                    string s_str = null;
                    if (txt_Record_CupNum.Text != null && txt_Record_CupNum.Text != "")
                    {
                        s_str = (" AssistantCode = '" + txt_Record_CupNum.Text + "' AND");
                    }

                    if (txt_RecordBottleNum.Text != null && txt_RecordBottleNum.Text != "")
                    {
                        s_str = (" BottleNum = '" + txt_RecordBottleNum.Text + "' AND");
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

                    s_sql = "SELECT AssistantCode,CupNum, BottleNum, FinishTime,Stand,L,A,B FROM history_abs  Where " + s_str + " And Type !=2 And Type !=4 " +
                               " ORDER BY FinishTime DESC;";
                    dt_data = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                }
                else
                {
                    s_sql = "SELECT AssistantCode,CupNum, BottleNum, FinishTime,Stand,L,A,B FROM history_abs  Where  Stand = 1 " +
                               " ORDER BY FinishTime DESC;";
                    dt_data = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                }

                //捆绑
                //dgv_DropRecord.DataSource = dt_data;

                //捆绑
                for (int i = 0; i < dt_data.Rows.Count; i++)
                {
                    dgv_DropRecord.Rows.Add(dt_data.Rows[i][0].ToString(), dt_data.Rows[i][1].ToString(), dt_data.Rows[i][2].ToString(), dt_data.Rows[i][3].ToString(),
                        Convert.ToDouble(dt_data.Rows[i][5]).ToString("F2"), Convert.ToDouble(dt_data.Rows[i][6]).ToString("F2"), Convert.ToDouble(dt_data.Rows[i][7]).ToString("F2"));
                    if (!(dt_data.Rows[i]["Stand"] is DBNull))
                    {
                        if (dt_data.Rows[i]["Stand"].ToString().Contains("1"))
                        {
                            dgv_DropRecord.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                        }
                    }
                }

                dgv_DropRecord.ClearSelection();
            }
            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.Message, "DropRecordHeadShow", MessageBoxButtons.OK, true);
            }
        }



        private void dgv_DropRecord_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgv_DropRecord.ClearSelection();
        }

        private void rdo_Record_All_CheckedChanged(object sender, EventArgs e)
        {
            if (rdo_Record_All.Checked)
            {
                txt_Record_CupNum.Enabled = false;
                dt_Record_Start.Enabled = false;
                dt_Record_End.Enabled = false;
                txt_RecordBottleNum.Enabled = false;
                //btn_Record_Delete.Visible = false;
            }
        }

        private void rdo_Record_condition_CheckedChanged(object sender, EventArgs e)
        {
            if (rdo_Record_condition.Checked)
            {
                txt_Record_CupNum.Enabled = true;
                dt_Record_Start.Enabled = true;
                dt_Record_End.Enabled = true;
                txt_RecordBottleNum.Enabled = true;

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
                txt_Record_CupNum.Enabled = false;
                dt_Record_Start.Enabled = false;
                dt_Record_End.Enabled = false;
                txt_RecordBottleNum.Enabled = false;
                //btn_Record_Delete.Visible = false;
            }
        }

        private void DetailsShow()
        {
            try
            {
                _points.Clear();
                lstPoints.Items.Clear();
                _nextPointIndex = 1;
                _panel3.Invalidate(); // 触发 Panel 重绘


                txt_dL.Text = "";
                txt_dA.Text = "";
                txt_dB.Text = "";
                txt_dE.Text = "";

                txt_RealConcentration.Text = "";
                txt_BottleNum.Text = "";
                txt_BrewingData.Text = "";
                label20.Text = "";
                label15.Text = "";
                label16.Text = "";
                textBox4.Text = "";
                txt_BaseData.Text = "";
                textBox1.Text = "";

                txt_FormulaCode.Text = "";
                txt_VersionNum.Text = "";
                txt_ClothWeight.Text = "";
                txt_TotalWeight.Text = "";
                txt_BathRatio.Text = "";
                chk_AddWaterChoose.Checked = false;
                dgv_FormulaData.Rows.Clear();
                dgv_FormulaData.Enabled = false;
                if (chart.Series.Count > 0)
                {
                    chart.Series.Clear();
                    chart.MouseMove -= new MouseEventHandler(chart1_MouseMove);
                    chart.MouseWheel -= new MouseEventHandler(chart1_MouseMove);
                }
                toolTip1.RemoveAll();

                //读取选中行对应的配方资料
                string s_finishtime = dgv_DropRecord.CurrentRow.Cells[3].Value.ToString();
                string s_sql = "SELECT * FROM history_abs" +
                                   " Where FinishTime = '" + s_finishtime + "';";
                DataTable dt_history_abs = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                string s_stand = "";
                string s_test = "";
                //查询标样数据
                if (dt_history_abs.Rows.Count > 0)
                {
                    i_start = Convert.ToInt32(dt_history_abs.Rows[0]["StartWave"]);
                    i_end = Convert.ToInt32(dt_history_abs.Rows[0]["EndWave"]);
                    i_int = Convert.ToInt32(dt_history_abs.Rows[0]["IntWave"]);

                    //string s_result = dt_history_abs.Rows[0]["Result"] is DBNull ? "" : dt_history_abs.Rows[0]["Result"].ToString();
                    //if (s_result.Length > 4)
                    //    textBox1.Text = s_result.Substring(0, 4);
                    //else
                    //    textBox1.Text = s_result;
                    if (!(dt_history_abs.Rows[0]["TotalTime"] is DBNull))
                    {
                        string s_temp = Convert.ToInt32(dt_history_abs.Rows[0]["TotalTime"]) / 60 / 60 + ":" + Convert.ToInt32(dt_history_abs.Rows[0]["TotalTime"]) % (60 * 60) / 60 + ":" + Convert.ToInt32(dt_history_abs.Rows[0]["TotalTime"]) % (60 * 60) % 60;
                        textBox1.Text = s_temp;
                    }

                    textBox3.Text = dt_history_abs.Rows[0]["RealSampleDosage"].ToString();
                    textBox2.Text = dt_history_abs.Rows[0]["RealAdditivesDosage"].ToString();

                    string s_data = dt_history_abs.Rows[0]["Abs"] is DBNull ? "" : dt_history_abs.Rows[0]["Abs"].ToString();
                    if (s_data != "")
                        s_data = s_data.Substring(0, s_data.Length - 2);
                    string[] sa_arr = s_data.Split('/');
                    //判断是否是否标样
                    if (dt_history_abs.Rows[0]["Type"].ToString() == "3")
                    {


                        s_stand = dt_history_abs.Rows[0]["Abs"] is DBNull ? "" : dt_history_abs.Rows[0]["Abs"].ToString();
                    }
                    else
                    {

                        s_test = dt_history_abs.Rows[0]["Abs"] is DBNull ? "" : dt_history_abs.Rows[0]["Abs"].ToString();

                        //如果是混合液，查询标样就是根据配方代码，查询history_abs来获取
                        if (Convert.ToInt32(dt_history_abs.Rows[0]["BottleNum"].ToString()) == 888 || Convert.ToInt32(dt_history_abs.Rows[0]["BottleNum"].ToString()) == 999)
                        {
                            //显示滴液配方记录
                            s_sql = "SELECT *  FROM abs_history_head WHERE BatchName = '" + dt_history_abs.Rows[0]["BatchName"].ToString() + "' And CupNum=" + dt_history_abs.Rows[0]["DripNum"].ToString() + " And FormulaCode = '" + dt_history_abs.Rows[0]["FormulaCode"].ToString() + "';";
                            DataTable dt_abs_history_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                            if (dt_abs_history_head.Rows.Count > 0)
                            {
                                txt_FormulaCode.Text = dt_abs_history_head.Rows[0]["FormulaCode"].ToString();
                                txt_VersionNum.Text = dt_abs_history_head.Rows[0]["VersionNum"].ToString();
                                txt_ClothWeight.Text = dt_abs_history_head.Rows[0]["ClothWeight"].ToString();
                                txt_BathRatio.Text = dt_abs_history_head.Rows[0]["BathRatio"].ToString();
                                txt_TotalWeight.Text = dt_abs_history_head.Rows[0]["TotalWeight"].ToString();
                                chk_AddWaterChoose.Checked = (dt_abs_history_head.Rows[0]["AddWaterChoose"].ToString() == "False" || dt_abs_history_head.Rows[0]["AddWaterChoose"].ToString() == "0" ? false : true);
                            }

                            s_sql = "SELECT *  FROM abs_history_details WHERE BatchName = '" + dt_history_abs.Rows[0]["BatchName"].ToString() + "' And CupNum=" + dt_history_abs.Rows[0]["DripNum"].ToString() + " And FormulaCode = '" + dt_history_abs.Rows[0]["FormulaCode"].ToString() + "' order by IndexNum;";
                            DataTable dt_abs_history_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                            if (dt_abs_history_details.Rows.Count > 0)
                            {

                                //显示详细信息
                                for (int j = 0; j < dt_abs_history_details.Rows.Count; j++)
                                {
                                    dgv_FormulaData.Rows.Add(dt_abs_history_details.Rows[j]["IndexNum"].ToString(),
                                                             dt_abs_history_details.Rows[j]["AssistantCode"].ToString(),
                                                             dt_abs_history_details.Rows[j]["AssistantName"].ToString(),
                                                             dt_abs_history_details.Rows[j]["FormulaDosage"].ToString(),
                                                             dt_abs_history_details.Rows[j]["UnitOfAccount"].ToString(),
                                                             null,
                                                             dt_abs_history_details.Rows[j]["SettingConcentration"].ToString(),
                                                             dt_abs_history_details.Rows[j]["RealConcentration"].ToString(),
                                                             dt_abs_history_details.Rows[j]["ObjectDropWeight"].ToString(),
                                                             dt_abs_history_details.Rows[j]["RealDropWeight"].ToString(), "0.00");

                                    //显示瓶号
                                    s_sql = "SELECT BottleNum,SettingConcentration,RealConcentration,DropMinWeight" +
                                                " FROM bottle_details WHERE" +
                                                " AssistantCode = '" + dgv_FormulaData[1, j].Value.ToString() + "'" +
                                                " AND RealConcentration != 0 Order BY BottleNum ;";
                                    DataTable dt_bottlenum = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                                    DataGridViewComboBoxCell dd = (DataGridViewComboBoxCell)dgv_FormulaData[5, j];
                                    List<string> lis_bottleNum = new List<string>();

                                    bool b_exist = false;
                                    foreach (DataRow mdr in dt_bottlenum.Rows)
                                    {
                                        string s_num = mdr[0].ToString();

                                        lis_bottleNum.Add(s_num);

                                        if ((dt_abs_history_details.Rows[j]["BottleNum"]).ToString() == s_num)
                                        {
                                            b_exist = true;
                                        }

                                    }

                                    dd.Value = null;
                                    dd.DataSource = lis_bottleNum;

                                    if (b_exist)
                                    {
                                        dd.Value = (dt_abs_history_details.Rows[j]["BottleNum"]).ToString();
                                    }
                                    else
                                    {
                                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                            FADM_Form.CustomMessageBox.Show((dt_abs_history_details.Rows[j]["BottleNum"]).ToString() +
                                                         "号母液瓶不存在", "温馨提示", MessageBoxButtons.OK, false);
                                        else
                                            FADM_Form.CustomMessageBox.Show((dt_abs_history_details.Rows[j]["BottleNum"]).ToString() +
                                                         " Mother liquor bottle number does not exist", "Tips", MessageBoxButtons.OK, false);
                                    }


                                    //显示是否手动选瓶
                                    DataGridViewCheckBoxCell dc = (DataGridViewCheckBoxCell)dgv_FormulaData[11, j];
                                    dc.Value = dt_abs_history_details.Rows[j]["BottleSelection"].ToString() == "0" ? 0 : 1;

                                    //dgv_FormulaData.Enabled = true;
                                    //dgv_FormulaData.CurrentCell = dgv_FormulaData[1, 0];
                                    //dgv_FormulaData.Focus();
                                    //return;
                                }

                                //for (int i = 0; i < dt_abs_history_details.Rows.Count; i++)
                                //{
                                //    dgv_FormulaData.Rows.Add();
                                //    dgv_FormulaData.Rows[i].Cells[0].Value = dt_abs_history_details.Rows[i]["IndexNum"];
                                //    dgv_FormulaData.Rows[i].Cells[1].Value = dt_abs_history_details.Rows[i]["AssistantCode"];
                                //    dgv_FormulaData.Rows[i].Cells[2].Value = dt_abs_history_details.Rows[i]["AssistantName"];
                                //    dgv_FormulaData.Rows[i].Cells[3].Value = dt_abs_history_details.Rows[i]["FormulaDosage"];
                                //    dgv_FormulaData.Rows[i].Cells[4].Value = dt_abs_history_details.Rows[i]["UnitOfAccount"];
                                //   // dgv_FormulaData.Rows[i].Cells[5].Value = dt_abs_history_details.Rows[i]["BottleNum"];
                                //    dgv_FormulaData.Rows[i].Cells[6].Value = dt_abs_history_details.Rows[i]["SettingConcentration"];
                                //    dgv_FormulaData.Rows[i].Cells[7].Value = dt_abs_history_details.Rows[i]["RealConcentration"];
                                //    dgv_FormulaData.Rows[i].Cells[8].Value = dt_abs_history_details.Rows[i]["ObjectDropWeight"];
                                //    dgv_FormulaData.Rows[i].Cells[9].Value = dt_abs_history_details.Rows[i]["RealDropWeight"];
                                //    //dgv_FormulaData.Rows[i].Cells[10].Value = dt_abs_history_details.Rows[i]["IndexNum"];
                                //    //dgv_FormulaData.Rows[i].Cells[11].Value = dt_abs_history_details.Rows[i]["AssistantCode"];
                                //    //dgv_FormulaData.Rows[i].Cells[11].Value = dt_abs_history_details.Rows[i]["AssistantCode"];
                                //    //  dgv_FormulaData.Rows.Add(
                                //    //  dt_abs_history_details.Rows[i]["IndexNum"] is DBNull ? "" : dt_abs_history_details.Rows[i]["IndexNum"].ToString(),
                                //    //  dt_abs_history_details.Rows[i]["AssistantCode"] is DBNull ? "" : dt_abs_history_details.Rows[i]["AssistantCode"].ToString(),
                                //    //  dt_abs_history_details.Rows[i]["AssistantName"] is DBNull ? "" : dt_abs_history_details.Rows[i]["AssistantName"].ToString(),
                                //    //  dt_abs_history_details.Rows[i]["FormulaDosage"] is DBNull ? "" : dt_abs_history_details.Rows[i]["FormulaDosage"].ToString(),
                                //    //  dt_abs_history_details.Rows[i]["UnitOfAccount"] is DBNull ? "" : dt_abs_history_details.Rows[i]["UnitOfAccount"].ToString(),
                                //    //  dt_abs_history_details.Rows[i]["BottleNum"] is DBNull ? "" : dt_abs_history_details.Rows[i]["BottleNum"].ToString(),
                                //    //  dt_abs_history_details.Rows[i]["SettingConcentration"] is DBNull ? "" : dt_abs_history_details.Rows[i]["SettingConcentration"].ToString(),
                                //    //  dt_abs_history_details.Rows[i]["RealConcentration"] is DBNull ? "" : dt_abs_history_details.Rows[i]["RealConcentration"].ToString(),
                                //    //  dt_abs_history_details.Rows[i]["ObjectDropWeight"] is DBNull ? "" : dt_abs_history_details.Rows[i]["ObjectDropWeight"].ToString(),
                                //    //  dt_abs_history_details.Rows[i]["RealDropWeight"] is DBNull ? "" : dt_abs_history_details.Rows[i]["RealDropWeight"].ToString(),
                                //    //// dt_abs_history_details.Rows[i]["RealDropWeight"] is DBNull ? "" : dt_abs_history_details.Rows[i]["RealDropWeight"].ToString(),
                                //    //  //dt_abs_history_details.Rows[i]["BottleSelection"] is DBNull ?  false:  dt_abs_history_details.Rows[i]["BottleSelection"].ToString(),
                                //    // dt_abs_history_details.Rows[i]["BottleSelection"].ToString() == "0" ? 0 : 1,
                                //    //  dt_abs_history_details.Rows[i]["BrewingData"] is DBNull ? "" : dt_abs_history_details.Rows[i]["BrewingData"].ToString());

                                //    DataGridViewCheckBoxCell dc = (DataGridViewCheckBoxCell)dgv_FormulaData[11, i];
                                //    dc.Value = dt_abs_history_details.Rows[i]["BottleSelection"].ToString() == "0" ? 0 : 1;


                                //}
                            }
                            //先查询标准吸光度
                            s_sql = "SELECT *  FROM history_abs WHERE FormulaCode = '" + dt_history_abs.Rows[0]["FormulaCode"].ToString() + "' And Stand=1;";
                            DataTable dt_stand_history_abs = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                            if (dt_stand_history_abs.Rows.Count > 0)
                            {

                                txt_BottleNum.Text = dt_history_abs.Rows[0]["BottleNum"].ToString();
                                txt_BrewingData.Text = dt_history_abs.Rows[0]["BrewingData"].ToString();
                                txt_RealConcentration.Text = "";


                                label20.Text = "";
                                textBox4.Text = "";
                                txt_BaseData.Text = "";

                                string s_data1;
                                s_data1 = dt_stand_history_abs.Rows[0]["Abs"] is DBNull ? "" : dt_stand_history_abs.Rows[0]["Abs"].ToString();

                                if (s_data1 != "")
                                {
                                    string s_L = dt_history_abs.Rows[0]["L"] is DBNull ? "" : dt_history_abs.Rows[0]["L"].ToString();
                                    if (!(dt_history_abs.Rows[0]["L"] is DBNull) && !(dt_stand_history_abs.Rows[0]["L"] is DBNull))
                                    {
                                        double db_dL = Convert.ToDouble(dt_history_abs.Rows[0]["L"].ToString()) - Convert.ToDouble(dt_stand_history_abs.Rows[0]["L"].ToString());
                                        txt_dL.Text = db_dL.ToString("f2");

                                    }

                                    string s_A = dt_history_abs.Rows[0]["A"] is DBNull ? "" : dt_history_abs.Rows[0]["A"].ToString();
                                    if (!(dt_history_abs.Rows[0]["A"] is DBNull) && !(dt_stand_history_abs.Rows[0]["A"] is DBNull))
                                        txt_dA.Text = (Convert.ToDouble(dt_history_abs.Rows[0]["A"].ToString()) - Convert.ToDouble(dt_stand_history_abs.Rows[0]["A"].ToString())).ToString("f2");
                                    string s_B = dt_history_abs.Rows[0]["B"] is DBNull ? "" : dt_history_abs.Rows[0]["B"].ToString();
                                    if (!(dt_history_abs.Rows[0]["B"] is DBNull) && !(dt_stand_history_abs.Rows[0]["B"] is DBNull))
                                        txt_dB.Text = (Convert.ToDouble(dt_history_abs.Rows[0]["B"].ToString()) - Convert.ToDouble(dt_stand_history_abs.Rows[0]["B"].ToString())).ToString("f2");
                                    if (!(dt_history_abs.Rows[0]["L"] is DBNull) && !(dt_stand_history_abs.Rows[0]["L"] is DBNull)
                                        && !(dt_history_abs.Rows[0]["A"] is DBNull) && !(dt_stand_history_abs.Rows[0]["A"] is DBNull)
                                        && !(dt_history_abs.Rows[0]["B"] is DBNull) && !(dt_stand_history_abs.Rows[0]["B"] is DBNull))
                                    {
                                        double d_cmc = MyAbsorbance.CalculateCMC(Convert.ToDouble(dt_stand_history_abs.Rows[0]["L"].ToString()), Convert.ToDouble(dt_stand_history_abs.Rows[0]["A"].ToString()), Convert.ToDouble(dt_stand_history_abs.Rows[0]["B"].ToString()), Convert.ToDouble(dt_history_abs.Rows[0]["L"].ToString()), Convert.ToDouble(dt_history_abs.Rows[0]["A"].ToString()), Convert.ToDouble(dt_history_abs.Rows[0]["B"].ToString()), 2, 1);
                                        txt_dE.Text = d_cmc.ToString("f2");

                                    }


                                    s_data1 = s_data1.Substring(0, s_data1.Length - 2);


                                }
                                string[] sa_arr1 = s_data1.Split('/');

                                //当标样数据不存在时，只显示试样
                                if (sa_arr1.Count() == 0 || s_data1 == "")
                                {

                                }
                                else
                                {
                                    //当数据相等时，一起显示
                                    if (sa_arr1.Count() == sa_arr.Count())
                                    {

                                    }
                                    //当数据不一致时，直接返回，曲线也不显示
                                    else
                                    {
                                        FADM_Form.CustomMessageBox.Show("标样采集点与试样采集点不一致", "温馨提示", MessageBoxButtons.OK, true);
                                        return;
                                    }
                                }
                                s_stand = dt_stand_history_abs.Rows[0]["Abs"] is DBNull ? "" : dt_stand_history_abs.Rows[0]["Abs"].ToString();

                                int i_index = 0;
                                for (int i = 0; i < sa_arr.Count(); i++)
                                {
                                    if (Convert.ToInt32(dt_history_abs.Rows[0]["StartWave"]) + i * Convert.ToInt32(dt_history_abs.Rows[0]["IntWave"]) >= 400)
                                    {
                                        i_index = i;
                                        break;
                                    }
                                }
                                if (sa_arr1.Count() == 0 || s_data1 == "")
                                {

                                }
                                else
                                {
                                    double[] doublesS = new double[sa_arr.Count() - i_index];
                                    double[] doublesT = new double[sa_arr.Count() - i_index];
                                    for (int i = 0; i < sa_arr.Count() - i_index; i++)
                                    {
                                        doublesS[i] = Convert.ToDouble(sa_arr[i + i_index]);
                                    }
                                    for (int i = 0; i < sa_arr1.Count() - i_index; i++)
                                    {
                                        doublesT[i] = Convert.ToDouble(sa_arr1[i + i_index]);
                                    }
                                    //textBox5.Text = MyAbsorbance.SWL(doublesS, doublesT).ToString("F2") + "%";
                                    textBox6.Text = MyAbsorbance.SUM(doublesS, doublesT, i_start, i_end, Convert.ToInt32(dt_history_abs.Rows[0]["IntWave"])).ToString("F2") + "%";
                                    //textBox7.Text = MyAbsorbance.WSUM(doublesS, doublesT, i_start, i_end, Convert.ToInt32(dt_history_abs.Rows[0]["IntWave"]), 10).ToString("F2") + "%";
                                    //textBox8.Text = MyAbsorbance.SUM1(doublesS, doublesT, i_start, i_end, Convert.ToInt32(dt_history_abs.Rows[0]["IntWave"])).ToString("F2") + "%";
                                    //textBox9.Text = MyAbsorbance.WSUM1(doublesS, doublesT, i_start, i_end, Convert.ToInt32(dt_history_abs.Rows[0]["IntWave"]), 10).ToString("F2") + "%";

                                }
                            }
                        }
                        else
                        {
                            //先查询标准吸光度
                            s_sql = "SELECT *  FROM bottle_details WHERE BottleNum = '" + dt_history_abs.Rows[0]["BottleNum"].ToString() + "';";
                            DataTable dt_bottle = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                            if (dt_bottle.Rows.Count > 0)
                            {

                                txt_BottleNum.Text = dt_history_abs.Rows[0]["BottleNum"].ToString();
                                txt_BrewingData.Text = dt_history_abs.Rows[0]["BrewingData"].ToString();
                                txt_RealConcentration.Text = dt_history_abs.Rows[0]["RealConcentration"].ToString();



                                s_sql = "SELECT *  FROM assistant_details WHERE AssistantCode = '" + dt_bottle.Rows[0]["AssistantCode"].ToString() + "';";
                                DataTable dt_assistant_details = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                                if (dt_assistant_details.Rows.Count > 0)
                                {
                                    label20.Text = dt_assistant_details.Rows[0]["UnitOfAccount"].ToString();
                                    textBox4.Text = dt_assistant_details.Rows[0]["AssistantName"].ToString();
                                }
                                int TermOfValidity = dt_assistant_details.Rows[0]["TermOfValidity"] is DBNull ? 0 : Convert.ToInt32(dt_assistant_details.Rows[0]["TermOfValidity"]);
                                int termSeconds = TermOfValidity * 3600;
                                TimeSpan ts = Convert.ToDateTime(dt_history_abs.Rows[0]["FinishTime"]) - Convert.ToDateTime(dt_history_abs.Rows[0]["BrewingData"]);
                                int secondsLeft = termSeconds - (int)ts.TotalSeconds;
                                string sign = secondsLeft < 0 ? "-" : "";
                                TimeSpan absLeft = TimeSpan.FromSeconds(Math.Abs(secondsLeft));
                                txt_BaseData.Text = $"{sign}{absLeft.Days}天{absLeft.Hours:D2}:{absLeft.Minutes:D2}:{absLeft.Seconds:D2}";


                                string s_data1;
                                //if (dt_history_abs.Rows[0]["CupNum"].ToString() == "2")
                                //{
                                s_data1 = dt_assistant_details.Rows[0]["Abs"] is DBNull ? "" : dt_assistant_details.Rows[0]["Abs"].ToString();
                                //}
                                //else
                                //{
                                //    s_data1 = dt_assistant_details.Rows[0]["Abs2"] is DBNull ? "" : dt_assistant_details.Rows[0]["Abs2"].ToString();
                                //}
                                if (s_data1 != "")
                                {
                                    string s_L = dt_history_abs.Rows[0]["L"] is DBNull ? "" : dt_history_abs.Rows[0]["L"].ToString();
                                    //if (dt_history_abs.Rows[0]["CupNum"].ToString() == "2")
                                    //{
                                    if (!(dt_history_abs.Rows[0]["L"] is DBNull) && !(dt_assistant_details.Rows[0]["L"] is DBNull))
                                    {
                                        double db_dL = Convert.ToDouble(dt_history_abs.Rows[0]["L"].ToString()) - Convert.ToDouble(dt_assistant_details.Rows[0]["L"].ToString());
                                        txt_dL.Text = db_dL.ToString("f2");
                                        //if(Math.Abs(db_dL) >0.3)
                                        //{
                                        //    label15.Text = "不合格";
                                        //    label15.BackColor = Color.Red;

                                        //}
                                        //else
                                        //{
                                        //    label15.Text = "合格";
                                        //    label15.BackColor = Color.Lime;
                                        //}

                                    }
                                    //}
                                    //else
                                    //{
                                    //    if (!(dt_history_abs.Rows[0]["L"] is DBNull) && !(dt_assistant_details.Rows[0]["L2"] is DBNull))
                                    //    {
                                    //        double db_dL = Convert.ToDouble(dt_history_abs.Rows[0]["L"].ToString()) - Convert.ToDouble(dt_assistant_details.Rows[0]["L2"].ToString());
                                    //        txt_dL.Text = db_dL.ToString("f2");
                                    //        //if(Math.Abs(db_dL) >0.3)
                                    //        //{
                                    //        //    label15.Text = "不合格";
                                    //        //    label15.BackColor = Color.Red;

                                    //        //}
                                    //        //else
                                    //        //{
                                    //        //    label15.Text = "合格";
                                    //        //    label15.BackColor = Color.Lime;
                                    //        //}

                                    //    }
                                    //}
                                    //if (dt_history_abs.Rows[0]["CupNum"].ToString() == "2")
                                    //{
                                    string s_A = dt_history_abs.Rows[0]["A"] is DBNull ? "" : dt_history_abs.Rows[0]["A"].ToString();
                                    if (!(dt_history_abs.Rows[0]["A"] is DBNull) && !(dt_assistant_details.Rows[0]["A"] is DBNull))
                                        txt_dA.Text = (Convert.ToDouble(dt_history_abs.Rows[0]["A"].ToString()) - Convert.ToDouble(dt_assistant_details.Rows[0]["A"].ToString())).ToString("f2");
                                    string s_B = dt_history_abs.Rows[0]["B"] is DBNull ? "" : dt_history_abs.Rows[0]["B"].ToString();
                                    if (!(dt_history_abs.Rows[0]["B"] is DBNull) && !(dt_assistant_details.Rows[0]["B"] is DBNull))
                                        txt_dB.Text = (Convert.ToDouble(dt_history_abs.Rows[0]["B"].ToString()) - Convert.ToDouble(dt_assistant_details.Rows[0]["B"].ToString())).ToString("f2");
                                    if (!(dt_history_abs.Rows[0]["L"] is DBNull) && !(dt_assistant_details.Rows[0]["L"] is DBNull)
                                        && !(dt_history_abs.Rows[0]["A"] is DBNull) && !(dt_assistant_details.Rows[0]["A"] is DBNull)
                                        && !(dt_history_abs.Rows[0]["B"] is DBNull) && !(dt_assistant_details.Rows[0]["B"] is DBNull))
                                    {
                                        double d_cmc = MyAbsorbance.CalculateCMC(Convert.ToDouble(dt_assistant_details.Rows[0]["L"].ToString()), Convert.ToDouble(dt_assistant_details.Rows[0]["A"].ToString()), Convert.ToDouble(dt_assistant_details.Rows[0]["B"].ToString()), Convert.ToDouble(dt_history_abs.Rows[0]["L"].ToString()), Convert.ToDouble(dt_history_abs.Rows[0]["A"].ToString()), Convert.ToDouble(dt_history_abs.Rows[0]["B"].ToString()), 2, 1);
                                        txt_dE.Text = d_cmc.ToString("f2");
                                        //if (Math.Abs(d_cmc) > 0.3)
                                        //{
                                        //    label16.Text = "不合格";
                                        //    label16.BackColor = Color.Red;

                                        //}
                                        //else
                                        //{
                                        //    label16.Text = "合格";
                                        //    label16.BackColor = Color.Lime;
                                        //}

                                        if (double.TryParse(dt_history_abs.Rows[0]["A"].ToString(), out double a) && double.TryParse(dt_history_abs.Rows[0]["B"].ToString(), out double b))
                                        {
                                            // 限制 a/b 范围在 [-128, 127]
                                            a = Clamp(a, -128, 127);
                                            b = Clamp(b, -128, 127);

                                            _points.Add(new LabPoint(a, b, _nextPointIndex));
                                            lstPoints.Items.Add($"点 {_nextPointIndex}: a={a:F1}, b={b:F1}");
                                            _nextPointIndex++;

                                            _panel3.Invalidate(); // 触发 Panel 重绘
                                        }
                                        else
                                        {
                                            //MessageBox.Show("请输入有效的数字！");
                                        }

                                        if (double.TryParse(dt_assistant_details.Rows[0]["A"].ToString(), out double c) && double.TryParse(dt_assistant_details.Rows[0]["B"].ToString(), out double d))
                                        {
                                            // 限制 a/b 范围在 [-128, 127]
                                            a = Clamp(c, -128, 127);
                                            b = Clamp(d, -128, 127);

                                            _points.Add(new LabPoint(a, b, _nextPointIndex));
                                            lstPoints.Items.Add($"点 {_nextPointIndex}: a={a:F1}, b={b:F1}");
                                            _nextPointIndex++;

                                            _panel3.Invalidate(); // 触发 Panel 重绘
                                        }
                                        else
                                        {
                                            //MessageBox.Show("请输入有效的数字！");
                                        }


                                    }
                                    //}
                                    //else
                                    //{
                                    //    string s_A = dt_history_abs.Rows[0]["A"] is DBNull ? "" : dt_history_abs.Rows[0]["A"].ToString();
                                    //    if (!(dt_history_abs.Rows[0]["A"] is DBNull) && !(dt_assistant_details.Rows[0]["A2"] is DBNull))
                                    //        txt_dA.Text = (Convert.ToDouble(dt_history_abs.Rows[0]["A"].ToString()) - Convert.ToDouble(dt_assistant_details.Rows[0]["A2"].ToString())).ToString("f2");
                                    //    string s_B = dt_history_abs.Rows[0]["B"] is DBNull ? "" : dt_history_abs.Rows[0]["B"].ToString();
                                    //    if (!(dt_history_abs.Rows[0]["B"] is DBNull) && !(dt_assistant_details.Rows[0]["B2"] is DBNull))
                                    //        txt_dB.Text = (Convert.ToDouble(dt_history_abs.Rows[0]["B"].ToString()) - Convert.ToDouble(dt_assistant_details.Rows[0]["B2"].ToString())).ToString("f2");
                                    //    if (!(dt_history_abs.Rows[0]["L"] is DBNull) && !(dt_assistant_details.Rows[0]["L2"] is DBNull)
                                    //        && !(dt_history_abs.Rows[0]["A"] is DBNull) && !(dt_assistant_details.Rows[0]["A2"] is DBNull)
                                    //        && !(dt_history_abs.Rows[0]["B"] is DBNull) && !(dt_assistant_details.Rows[0]["B2"] is DBNull))
                                    //    {
                                    //        double d_cmc = MyAbsorbance.CalculateCMC(Convert.ToDouble(dt_assistant_details.Rows[0]["L2"].ToString()), Convert.ToDouble(dt_assistant_details.Rows[0]["A2"].ToString()), Convert.ToDouble(dt_assistant_details.Rows[0]["B2"].ToString()), Convert.ToDouble(dt_history_abs.Rows[0]["L"].ToString()), Convert.ToDouble(dt_history_abs.Rows[0]["A"].ToString()), Convert.ToDouble(dt_history_abs.Rows[0]["B"].ToString()), 2, 1);
                                    //        txt_dE.Text = d_cmc.ToString("f2");
                                    //        //if (Math.Abs(d_cmc) > 0.3)
                                    //        //{
                                    //        //    label16.Text = "不合格";
                                    //        //    label16.BackColor = Color.Red;

                                    //        //}
                                    //        //else
                                    //        //{
                                    //        //    label16.Text = "合格";
                                    //        //    label16.BackColor = Color.Lime;
                                    //        //}
                                    //    }
                                    //}

                                    s_data1 = s_data1.Substring(0, s_data1.Length - 2);
                                    //if (dt_history_abs.Rows[0]["CupNum"].ToString() == "2")
                                    //{

                                    //}
                                    //else
                                    //{
                                    //    if (!(dt_assistant_details.Rows[0]["L"] is DBNull))
                                    //        txt_SL.Text = Convert.ToDouble(dt_assistant_details.Rows[0]["L2"].ToString()).ToString("f2");
                                    //    if (!(dt_assistant_details.Rows[0]["A"] is DBNull))
                                    //        txt_SA.Text = Convert.ToDouble(dt_assistant_details.Rows[0]["A2"].ToString()).ToString("f2");
                                    //    if (!(dt_assistant_details.Rows[0]["B"] is DBNull))
                                    //        txt_SB.Text = Convert.ToDouble(dt_assistant_details.Rows[0]["B2"].ToString()).ToString("f2");
                                    //}
                                }
                                string[] sa_arr1 = s_data1.Split('/');

                                //当标样数据不存在时，只显示试样
                                if (sa_arr1.Count() == 0 || s_data1 == "")
                                {

                                }
                                else
                                {
                                    //当数据相等时，一起显示
                                    if (sa_arr1.Count() == sa_arr.Count())
                                    {

                                    }
                                    //当数据不一致时，直接返回，曲线也不显示
                                    else
                                    {
                                        FADM_Form.CustomMessageBox.Show("标样采集点与试样采集点不一致", "温馨提示", MessageBoxButtons.OK, true);
                                        return;
                                    }
                                }
                                //if (dt_history_abs.Rows[0]["CupNum"].ToString() == "2")
                                s_stand = dt_assistant_details.Rows[0]["Abs"] is DBNull ? "" : dt_assistant_details.Rows[0]["Abs"].ToString();
                                //else
                                //    s_stand = dt_assistant_details.Rows[0]["Abs2"] is DBNull ? "" : dt_assistant_details.Rows[0]["Abs2"].ToString();

                                int i_index = 0;
                                for (int i = 0; i < sa_arr.Count(); i++)
                                {
                                    if (Convert.ToInt32(dt_history_abs.Rows[0]["StartWave"]) + i * Convert.ToInt32(dt_history_abs.Rows[0]["IntWave"]) >= 400)
                                    {
                                        i_index = i;
                                        break;
                                    }
                                }
                                if (sa_arr1.Count() == 0 || s_data1 == "")
                                {

                                }
                                else
                                {
                                    double[] doublesS = new double[sa_arr.Count() - i_index];
                                    double[] doublesT = new double[sa_arr.Count() - i_index];
                                    for (int i = 0; i < sa_arr.Count() - i_index; i++)
                                    {
                                        doublesS[i] = Convert.ToDouble(sa_arr[i + i_index]);
                                    }
                                    for (int i = 0; i < sa_arr1.Count() - i_index; i++)
                                    {
                                        doublesT[i] = Convert.ToDouble(sa_arr1[i + i_index]);
                                    }
                                    textBox5.Text = MyAbsorbance.SWL(doublesS, doublesT).ToString("F2") + "%";
                                    textBox6.Text = MyAbsorbance.SUM(doublesS, doublesT, i_start, i_end, Convert.ToInt32(dt_history_abs.Rows[0]["IntWave"])).ToString("F2") + "%";
                                    textBox7.Text = MyAbsorbance.WSUM(doublesS, doublesT, i_start, i_end, Convert.ToInt32(dt_history_abs.Rows[0]["IntWave"]), 10).ToString("F2") + "%";


                                }
                            }
                        }
                    }

                }

                //显示表头

                if (dt_history_abs.Rows.Count > 0)
                {
                    //读取曲线数据并显示
                    //if (s_stand != "")
                    //{
                    //    Show(s_stand);
                    if (s_test != "")
                    {
                        InitChart();
                        Show1(s_test);
                        if (s_stand != "")
                            Show(s_stand);
                    }
                    //}




                }
            }
            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.Message, "DetailsShow", MessageBoxButtons.OK, true);
            }
        }





        private void Show(string s_data)
        {
            s_data = s_data.Substring(0, s_data.Length - 2);
            string[] sa_arr = s_data.Split('/');

            //times = new DateTime[sa_arr.Count()];
            //for (int i = 0; i < sa_arr.Count(); i++)
            //{
            //    times[i] = dateTime.AddSeconds((i - sa_arr.Count()) * 30);
            //}

            AddSeries("标样", Color.Red);

            Series series = chart.Series[1];
            //series.Points.AddXY(0, 0);

            for (int i = 0; i < sa_arr.Count(); i++)
            {
                series.Points.AddXY(Convert.ToDouble(i_start + i * i_int), Convert.ToDouble(sa_arr[i]));
            }
            chart.MouseMove += new MouseEventHandler(chart1_MouseMove);

            //chart.MouseWheel += new System.Windows.Forms.MouseEventHandler(chart1_Mouselheel);


        }

        private void Show1(string s_data)
        {
            s_data = s_data.Substring(0, s_data.Length - 2);
            string[] sa_arr = s_data.Split('/');



            AddSeries("试样", Color.Blue);

            Series series = chart.Series[0];


            for (int i = 0; i < sa_arr.Count(); i++)
            {
                series.Points.AddXY(Convert.ToDouble(i_start + i * i_int), Convert.ToDouble(sa_arr[i]));
            }
            chart.MouseMove += new MouseEventHandler(chart1_MouseMove);

            //chart.MouseWheel += new System.Windows.Forms.MouseEventHandler(chart1_Mouselheel);


        }



        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (b_isRecord)
                {
                    if (chart.Series.Count > 0)
                    {
                        chart.ChartAreas[0].CursorX.SetCursorPixelPosition(new PointF(e.X, e.Y), true);
                        chart.ChartAreas[0].CursorY.SetCursorPixelPosition(new PointF(e.X, e.Y), true);
                        if (Convert.ToInt32(chart.ChartAreas[0].CursorX.Position.ToString()) < (i_start + chart.Series[0].Points.Count * i_int) && Convert.ToInt32(chart.ChartAreas[0].CursorX.Position.ToString()) > 0)
                        {
                            int i_index = 0;
                            for (int i = 0; i < chart.Series[0].Points.Count; i++)
                            {
                                if (Convert.ToInt32(chart.ChartAreas[0].CursorX.Position.ToString()) < i_start + i_int * i)
                                {
                                    i_index = i - 1; break;
                                }
                            }

                            if (Convert.ToInt32(chart.ChartAreas[0].CursorX.Position.ToString()) > i_start + i_int * (chart.Series[0].Points.Count - 1))
                            {
                                i_index = chart.Series[0].Points.Count - 1;
                            }
                            if (chart.Series.Count == 1)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    toolTip1.SetToolTip(chart, string.Format("波长:{0},试样吸光度值:{1}", chart.Series[0].Points[i_index].XValue,
                                    chart.Series[0].Points[i_index].YValues[0]));
                                else
                                    toolTip1.SetToolTip(chart, string.Format("Wave:{0},Test Abs:{1}", chart.Series[0].Points[Convert.ToInt32(chart.ChartAreas[0].CursorX.Position.ToString()) - 1].XValue,
                                    chart.Series[0].Points[Convert.ToInt32(chart.ChartAreas[0].CursorX.Position.ToString()) - 1].YValues[0]));
                            }
                            else
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    toolTip1.SetToolTip(chart, string.Format("波长:{0},试样吸光度值:{1},标样吸光度值:{2}", chart.Series[0].Points[i_index].XValue,
                                    chart.Series[0].Points[i_index].YValues[0],
                                    chart.Series[1].Points[i_index].YValues[0]));
                                else
                                    toolTip1.SetToolTip(chart, string.Format("Wave:{0},Test Abs:{1},standard Abs:{2}", chart.Series[0].Points[Convert.ToInt32(chart.ChartAreas[0].CursorX.Position.ToString()) - 1].XValue,
                                    chart.Series[0].Points[Convert.ToInt32(chart.ChartAreas[0].CursorX.Position.ToString()) - 1].YValues[0],
                                    chart.Series[1].Points[Convert.ToInt32(chart.ChartAreas[0].CursorX.Position.ToString()) - 1].YValues[0]));
                            }
                        }
                    }
                }
                else
                {
                    if (chart.Series.Count > 0)
                    {
                        chart.ChartAreas[0].CursorX.SetCursorPixelPosition(new PointF(e.X, e.Y), true);
                        chart.ChartAreas[0].CursorY.SetCursorPixelPosition(new PointF(e.X, e.Y), true);
                        int cursorX = Convert.ToInt32(chart.ChartAreas[0].CursorX.Position.ToString());
                        int pointCount = chart.Series[0].Points.Count;
                        if (cursorX < (i_start + pointCount * i_int) && cursorX > 0)
                        {
                            int i_index = 0;
                            for (int i = 0; i < pointCount; i++)
                            {
                                if (cursorX < i_start + i_int * i)
                                {
                                    i_index = i - 1; break;
                                }
                            }
                            if (cursorX > i_start + i_int * (pointCount - 1))
                            {
                                i_index = pointCount - 1;
                            }

                            // 组装所有曲线的提示
                            StringBuilder sb = new StringBuilder();
                            double wave = chart.Series[0].Points[i_index].XValue;
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            {
                                sb.AppendFormat("波长:{0}", wave);
                                for (int s = 0; s < chart.Series.Count; s++)
                                {
                                    var series = chart.Series[s];
                                    // 取曲线名和Y值
                                    sb.AppendFormat(",{0}:{1}", series.Name+"吸光度值", series.Points[i_index].YValues[0]);
                                }
                            }
                            else
                            {
                                sb.AppendFormat("Wave:{0}", wave);
                                for (int s = 0; s < chart.Series.Count; s++)
                                {
                                    var series = chart.Series[s];
                                    sb.AppendFormat(",{0}:{1}", series.Name, series.Points[i_index].YValues[0]);
                                }
                            }
                            toolTip1.SetToolTip(chart, sb.ToString());
                        }
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
        }


        private void CreateChart()
        {
            chart = new Chart();
            this.panel1.Controls.Clear();
            this.panel1.Controls.Add(chart);
            chart.Dock = DockStyle.Fill;
            chart.Visible = true;

            ChartArea chartArea = new ChartArea();
            //chartArea.Name = "FirstArea";
            chartArea.AxisX.Interval = 40;
            //chartArea.AxisX.IntervalOffset = 40;
            chartArea.AxisX.Minimum = 300;

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
                chartArea.AxisY.Title = @"吸光度";
                chartArea.AxisY.LineWidth = 2;
                chartArea.AxisY.LineColor = Color.Black;
                chartArea.AxisY.Enabled = AxisEnabled.True;


                chartArea.AxisX.Title = @"波长";
                chartArea.AxisX.IsLabelAutoFit = true;
                chartArea.AxisX.LabelAutoFitMinFontSize = 5;
                chartArea.AxisX.LabelStyle.Angle = -15;
            }
            else
            {
                // Axis
                chartArea.AxisY.Title = @"Abs";
                chartArea.AxisY.LineWidth = 2;
                chartArea.AxisY.LineColor = Color.Black;
                chartArea.AxisY.Enabled = AxisEnabled.True;


                chartArea.AxisX.Title = @"Wave length";
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

            chart.ChartAreas[0].AxisY.IsStartedFromZero = true;

            Legend Legend = new Legend("L1");
            Legend.DockedToChartArea = chart.Name;
            Legend.Font = new Font("宋体", 14.25F);
            chart.Legends.Add(Legend);
        }

        private void InitChart()
        {
            CreateChart();
        }

        private void dgv_DropRecord_CurrentCellChanged(object sender, EventArgs e)
        {
            if (b_isRecord)
            {
                if (dgv_DropRecord.CurrentRow != null)
                    DetailsShow();
            }

        }

        private void Btn_SetStand_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgv_DropRecord.CurrentRow != null)
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    {
                        DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("确定把当前记录设为标样吗?", "设定标样", MessageBoxButtons.YesNo, true);

                        if (dialogResult == DialogResult.Yes)
                        {
                            //如果选中行
                            if (dgv_DropRecord.SelectedRows.Count > 0)
                            {
                                string s_Ass = dgv_DropRecord.CurrentRow.Cells[0].Value.ToString();
                                string s_cupNum = dgv_DropRecord.CurrentRow.Cells[1].Value.ToString();
                                string s_bottleNum = dgv_DropRecord.CurrentRow.Cells[2].Value.ToString();
                                string s_finishTime = dgv_DropRecord.CurrentRow.Cells[3].Value.ToString();

                                if (Convert.ToInt32(s_bottleNum) == 888 || Convert.ToInt32(s_bottleNum) == 999)
                                {
                                    string s_sql = "SELECT *  FROM history_abs WHERE FinishTime = '" + s_finishTime + "' And CupNum = " + s_cupNum + " And BottleNum =" + s_bottleNum + ";";
                                    DataTable dt_history_abs = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                                    if (dt_history_abs.Rows.Count > 0)
                                    {
                                        //删除原来的标样
                                        FADM_Object.Communal._fadmSqlserver.ReviseData("Update history_abs Set Stand = 0  where FormulaCode = '" + dt_history_abs.Rows[0]["FormulaCode"].ToString() + "' ;");

                                        //标记为标样
                                        FADM_Object.Communal._fadmSqlserver.ReviseData("Update history_abs Set Stand = 1  WHERE FinishTime = '" + s_finishTime + "' And CupNum = " + s_cupNum + " And BottleNum =" + s_bottleNum + ";");
                                    }
                                }
                                else
                                {

                                    //保存到助剂表
                                    string s_sql = "SELECT *  FROM bottle_details WHERE BottleNum = '" + s_bottleNum + "';";
                                    DataTable dt_bottle = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                                    if (dt_bottle.Rows.Count > 0)
                                    {
                                        s_sql = "SELECT *  FROM history_abs WHERE FinishTime = '" + s_finishTime + "' And CupNum = " + s_cupNum + " And BottleNum =" + s_bottleNum + ";";
                                        DataTable dt_history_abs = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                                        if (dt_history_abs.Rows.Count > 0)
                                        {

                                            FADM_Object.Communal._fadmSqlserver.ReviseData("Update assistant_details Set Abs = '" + dt_history_abs.Rows[0]["Abs"].ToString() + "',L=" + dt_history_abs.Rows[0]["L"].ToString() + ",A=" + dt_history_abs.Rows[0]["A"].ToString() + ",B=" + dt_history_abs.Rows[0]["B"].ToString() + " where AssistantCode = '" + dt_bottle.Rows[0]["AssistantCode"].ToString() + "';");

                                            //
                                            //FADM_Object.Communal._fadmSqlserver.ReviseData("Update assistant_details Set Abs2 = '" + dt_history_abs.Rows[0]["Abs"].ToString() + "',L2=" + dt_history_abs.Rows[0]["L"].ToString() + ",A2=" + dt_history_abs.Rows[0]["A"].ToString() + ",B2=" + dt_history_abs.Rows[0]["B"].ToString() + " where AssistantCode = '" + dt_bottle.Rows[0]["AssistantCode"].ToString() + "';");


                                            //删除原来的标样
                                            FADM_Object.Communal._fadmSqlserver.ReviseData("Update history_abs Set Stand = 0  where AssistantCode = '" + s_Ass + "' ;");

                                            //标记为标样
                                            FADM_Object.Communal._fadmSqlserver.ReviseData("Update history_abs Set Stand = 1  WHERE FinishTime = '" + s_finishTime + "' And CupNum = " + s_cupNum + " And BottleNum =" + s_bottleNum + ";");

                                            DropRecordHeadShow();
                                        }
                                    }
                                }
                            }
                            //按照时间删除
                            else
                            {
                                FADM_Form.CustomMessageBox.Show("请选择操作行", "DetailsShow", MessageBoxButtons.OK, true);
                            }


                        }
                    }
                    else
                    {
                        DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("Are you sure to set the current record as the standard?", "Set standard sample", MessageBoxButtons.YesNo, true);

                        if (dialogResult == DialogResult.Yes)
                        {
                            //如果选中行
                            if (dgv_DropRecord.SelectedRows.Count > 0)
                            {
                                string s_cupNum = dgv_DropRecord.CurrentRow.Cells[1].Value.ToString();
                                string s_bottleNum = dgv_DropRecord.CurrentRow.Cells[2].Value.ToString();
                                string s_finishTime = dgv_DropRecord.CurrentRow.Cells[3].Value.ToString();

                                //保存到助剂表
                                string s_sql = "SELECT *  FROM bottle_details WHERE BottleNum = '" + s_bottleNum + "';";
                                DataTable dt_bottle = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                                if (dt_bottle.Rows.Count > 0)
                                {
                                    s_sql = "SELECT *  FROM history_abs WHERE FinishTime = '" + s_finishTime + "' And CupNum = " + s_cupNum + " And BottleNum =" + s_bottleNum + ";";
                                    DataTable dt_history_abs = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                                    if (dt_history_abs.Rows.Count > 0)
                                    {
                                        FADM_Object.Communal._fadmSqlserver.ReviseData("Update assistant_details Set Abs = '" + dt_history_abs.Rows[0]["Abs"].ToString() + "',L=" + dt_history_abs.Rows[0]["L"].ToString() + ",A=" + dt_history_abs.Rows[0]["A"].ToString() + ",B=" + dt_history_abs.Rows[0]["B"].ToString() + " where AssistantCode = '" + dt_bottle.Rows[0]["AssistantCode"].ToString() + "';");
                                    }
                                }


                            }
                            //按照时间删除
                            else
                            {
                                FADM_Form.CustomMessageBox.Show("Please select an action line", "DetailsShow", MessageBoxButtons.OK, true);
                            }


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.Message, "Btn_SetStand_Click", MessageBoxButtons.OK, true);
            }
        }

        private void dgv_DropRecord_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dgv_DropRecord.CurrentCell == null)
            {
                return;
            }
            string assCode = dgv_DropRecord.CurrentRow.Cells[0].Value.ToString();
            if (string.IsNullOrEmpty(assCode))
            {
                return;
            }
            dgv_DropRecord.Columns.Clear();
            dgv_DropRecord.Columns.Add("", "");
            dgv_DropRecord.Columns.Add("", "");
            dgv_DropRecord.Columns.Add("", "");
            dgv_DropRecord.Columns.Add("", "");
            dgv_DropRecord.Columns.Add("", "");
            dgv_DropRecord.Columns.Add("", "");
            dgv_DropRecord.Columns.Add("", "");
            dgv_DropRecord.Columns.Add("", "");
            dgv_DropRecord.Columns[0].HeaderCell.Value = "助剂代码";
            dgv_DropRecord.Columns[1].HeaderCell.Value = "L";
            dgv_DropRecord.Columns[2].HeaderCell.Value = "A";
            dgv_DropRecord.Columns[3].HeaderCell.Value = "时间";
            dgv_DropRecord.Columns[4].HeaderCell.Value = "B";
            dgv_DropRecord.Columns[5].HeaderCell.Value = "dL";
            dgv_DropRecord.Columns[6].HeaderCell.Value = "dA";
            dgv_DropRecord.Columns[7].HeaderCell.Value = "dB";


            dgv_DropRecord.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgv_DropRecord.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgv_DropRecord.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgv_DropRecord.Columns[3].Visible = false;
            dgv_DropRecord.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgv_DropRecord.Columns[5].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgv_DropRecord.Columns[6].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgv_DropRecord.Columns[7].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgv_DropRecord.Rows.Clear();

            string s_sql = "SELECT AssistantCode,Stand,L,A,B,FinishTime FROM history_abs " +
                               " WHERE FinishTime != ''  And Type !=2 And Type !=4 AND AssistantCode = '" + assCode + "' ORDER BY FinishTime DESC;";
            DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            DataRow dataRow = dt_data.Select($"Stand = 1")[0];
            double l = Convert.ToDouble(dataRow["L"]);
            double a = Convert.ToDouble(dataRow["A"]);
            double b = Convert.ToDouble(dataRow["B"]);
            for (int i = 0; i < dt_data.Rows.Count; i++)
            {
                dgv_DropRecord.Rows.Add(dt_data.Rows[i][0].ToString(),
                     Convert.ToDouble(dt_data.Rows[i][2]).ToString("F2"),
                     Convert.ToDouble(dt_data.Rows[i][3]).ToString("F2"),
                     dt_data.Rows[i][5],
                     Convert.ToDouble(dt_data.Rows[i][4]).ToString("F2"),
                    (Convert.ToDouble(dt_data.Rows[i][2]) - l).ToString("F2"),
                    (Convert.ToDouble(dt_data.Rows[i][3]) - a).ToString("F2"),
                    (Convert.ToDouble(dt_data.Rows[i][4]) - b).ToString("F2"));
                if (!(dt_data.Rows[i]["Stand"] is DBNull))
                {
                    if (dt_data.Rows[i]["Stand"].ToString().Contains("1"))
                    {
                        dgv_DropRecord.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                    }
                }
            }


        }

        private void dgv_FormulaData_RowLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (!(dgv_DropRecord.SelectedRows.Count > 0))
            {
                this.dgv_FormulaData.EndEdit();
                if (dgv_FormulaData[1, dgv_FormulaData.CurrentRow.Index].Value == null ||
                    dgv_FormulaData[3, dgv_FormulaData.CurrentRow.Index].Value == null)
                {
                    return;
                }

                UpdataFormulaData(dgv_FormulaData.CurrentRow.Index);
            }
        }

        /// <summary>
        /// 更新配方表
        /// </summary>
        /// <param name="_CurrentRowIndex">当前行号</param>
        private void UpdataFormulaData(int _CurrentRowIndex)
        {
            try
            {

                DataTable dt_bottlenum = new DataTable();

                if (_CurrentRowIndex >= dgv_FormulaData.Rows.Count - 1)
                {
                    return;
                }
                if (dgv_FormulaData[3, _CurrentRowIndex].Value == null)
                {
                    return;
                }


                string s_sql = null;
                if (txt_VersionNum.Text != null)
                {
                    s_sql = "SELECT *  FROM abs_formula_details WHERE FormulaCode = '" + txt_FormulaCode.Text + "' AND" +
                                " VersionNum = '" + txt_VersionNum.Text + "' AND IndexNum ='" + dgv_FormulaData[0, _CurrentRowIndex].Value.ToString() + "'  order by IndexNum; ";
                    DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                    if (dt_data.Rows.Count > 0)
                    {
                        string s_code = (dt_data.Rows[0][dt_data.Columns["AssistantCode"]]).ToString();
                        if (s_code != dgv_FormulaData[1, _CurrentRowIndex].Value.ToString())
                        {

                            dgv_FormulaData[10, _CurrentRowIndex].Value = 0;
                        }
                    }
                }



                //获取染助剂资料
                s_sql = "SELECT *  FROM assistant_details WHERE" +
                            " AssistantCode = '" + dgv_FormulaData[1, _CurrentRowIndex].Value.ToString() + "' ; ";

                DataTable dt_assistantdetails = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                if (dt_assistantdetails.Rows.Count > 0)
                {
                    dgv_FormulaData[4, _CurrentRowIndex].Value = (dt_assistantdetails.Rows[0][5].ToString());
                    dgv_FormulaData[2, _CurrentRowIndex].Value = dt_assistantdetails.Rows[0][3].ToString();
                    dgv_FormulaData[9, _CurrentRowIndex].Value = "0.00";
                    dgv_FormulaData[10, _CurrentRowIndex].Value = "0.00";
                    //获取当前染助剂所有母液瓶资料
                    s_sql = "SELECT BottleNum, SettingConcentration ,RealConcentration, DropMinWeight" +
                                " FROM bottle_details WHERE" +
                                " AssistantCode = '" + dgv_FormulaData[1, _CurrentRowIndex].Value.ToString() + "'" +
                                " AND RealConcentration != 0 Order BY BottleNum ;";
                    dt_bottlenum = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                    //未找到一个合适的瓶
                    if (dt_bottlenum.Rows.Count == 0)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("当前染助剂代码未发现母液瓶！", "温馨提示", MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("No mother liquor bottle found for the current dyeing agent code！", "Tips", MessageBoxButtons.OK, false);

                        for (int i = 1; i < dgv_FormulaData.Columns.Count - 1; i++)
                        {
                            dgv_FormulaData.CurrentRow.Cells[i].Value = null;
                        }

                        return;
                    }
                    List<string> lis_bottleNum = new List<string>();
                    foreach (DataRow mdr in dt_bottlenum.Rows)
                    {
                        lis_bottleNum.Add(mdr[0].ToString());
                    }
                    DataGridViewComboBoxCell dd = (DataGridViewComboBoxCell)dgv_FormulaData[5, _CurrentRowIndex];
                    for (int j = 0; j < lis_bottleNum.Count; j++)
                    {
                        for (int i = 0; i < dd.Items.Count; i++)
                        {
                            if (dd.Items[i].ToString() == lis_bottleNum[j])
                            {
                                goto next;
                            }
                        }
                        dd.Value = null;
                        dd.DataSource = lis_bottleNum;
                        break;
                    next:
                        continue;
                    }

                    dt_bottlenum.Clear();



                    //跟据设定浓度重新排序
                    s_sql = "SELECT BottleNum, SettingConcentration, RealConcentration, DropMinWeight" +
                                " FROM bottle_details WHERE" +
                                " AssistantCode = '" + dgv_FormulaData[1, _CurrentRowIndex].Value.ToString() + "'" +
                                " AND RealConcentration != 0 Order BY SettingConcentration DESC;";

                    dt_bottlenum = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                    for (int i = 0; i < dt_bottlenum.Rows.Count; i++)
                    {
                        double d_objectDropWeight = 0;
                        //判断是否需要自动选瓶
                        if (dgv_FormulaData[11, _CurrentRowIndex].Value == null ||
                            dgv_FormulaData[11, _CurrentRowIndex].Value.ToString() == "0")
                        {
                            //需要自动选瓶
                            if (dgv_FormulaData.Rows[_CurrentRowIndex].Cells[4].Value != null)
                            {
                                if (dgv_FormulaData.Rows[_CurrentRowIndex].Cells[4].Value.ToString() == "%")
                                {
                                    //染料
                                    d_objectDropWeight = (Convert.ToDouble(txt_ClothWeight.Text) *
                                        Convert.ToDouble(dgv_FormulaData[3, _CurrentRowIndex].Value.ToString()) /
                                        Convert.ToDouble(dt_bottlenum.Rows[i][2].ToString()));
                                }
                                else
                                {
                                    //助剂
                                    d_objectDropWeight = (Convert.ToDouble(txt_TotalWeight.Text) *
                                        Convert.ToDouble(dgv_FormulaData[3, _CurrentRowIndex].Value.ToString()) /
                                        Convert.ToDouble(dt_bottlenum.Rows[i][2].ToString()));

                                }
                                if (Convert.ToDouble(String.Format("{0:F3}", d_objectDropWeight)) >=
                                    Convert.ToDouble(String.Format("{0:F3}", dt_bottlenum.Rows[i][3])))
                                {
                                    dd.Value = dt_bottlenum.Rows[i][0].ToString();
                                    dgv_FormulaData[6, _CurrentRowIndex].Value = dt_bottlenum.Rows[i][1].ToString();
                                    dgv_FormulaData[7, _CurrentRowIndex].Value = dt_bottlenum.Rows[i][2].ToString();
                                    dgv_FormulaData[8, _CurrentRowIndex].Value = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? String.Format("{0:F}", d_objectDropWeight) : String.Format("{0:F3}", d_objectDropWeight);

                                    break;
                                }
                                else
                                {
                                    if (i == dt_bottlenum.Rows.Count - 1)
                                    {
                                        if (d_objectDropWeight >= 0.1)
                                        {
                                            dd.Value = dt_bottlenum.Rows[i][0].ToString();
                                            dgv_FormulaData[6, _CurrentRowIndex].Value = dt_bottlenum.Rows[i][1].ToString();
                                            dgv_FormulaData[7, _CurrentRowIndex].Value = dt_bottlenum.Rows[i][2].ToString();
                                            dgv_FormulaData[8, _CurrentRowIndex].Value = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? String.Format("{0:F}", d_objectDropWeight) : String.Format("{0:F3}", d_objectDropWeight);

                                        }
                                        else
                                        {
                                            dd.Value = null;
                                            dgv_FormulaData[6, _CurrentRowIndex].Value = null;
                                            dgv_FormulaData[7, _CurrentRowIndex].Value = null;
                                            dgv_FormulaData[8, _CurrentRowIndex].Value = null;
                                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                                FADM_Form.CustomMessageBox.Show("第" + dgv_FormulaData[0, _CurrentRowIndex].Value.ToString() + "行目标滴液量小于0.1!", "温馨提示", MessageBoxButtons.OK, false);
                                            else
                                                FADM_Form.CustomMessageBox.Show("The target droplet volume in line " + dgv_FormulaData[0, _CurrentRowIndex].Value.ToString() + "  is less than 0.1!", "温馨提示", MessageBoxButtons.OK, false);

                                        }
                                    }
                                }



                            }

                        }
                        else
                        {
                            //不需要自动选瓶

                            //获取当前染助剂所有母液瓶资料
                            foreach (DataRow mdr in dt_bottlenum.Rows)
                            {
                                if (dd.Value.ToString() == mdr[0].ToString())
                                {
                                    dgv_FormulaData[5, _CurrentRowIndex].Value = mdr[0].ToString();
                                    dgv_FormulaData[6, _CurrentRowIndex].Value = mdr[1].ToString();
                                    dgv_FormulaData[7, _CurrentRowIndex].Value = mdr[2].ToString();

                                    break;
                                }
                            }

                            //计算目标滴液量
                            if (dgv_FormulaData[4, _CurrentRowIndex].Value != null)
                            {

                                if (dgv_FormulaData[4, _CurrentRowIndex].Value.ToString() == "%")
                                {
                                    //染料
                                    d_objectDropWeight = (Convert.ToDouble(txt_ClothWeight.Text) *
                                                       Convert.ToDouble(dgv_FormulaData[3, _CurrentRowIndex].Value.ToString()) /
                                                       Convert.ToDouble(dgv_FormulaData[7, _CurrentRowIndex].Value.ToString()));
                                }
                                else
                                {
                                    //助剂
                                    d_objectDropWeight = (Convert.ToDouble(txt_TotalWeight.Text) *
                                                       Convert.ToDouble(dgv_FormulaData[3, _CurrentRowIndex].Value.ToString()) /
                                                       Convert.ToDouble(dgv_FormulaData[7, _CurrentRowIndex].Value.ToString()));

                                }

                                dgv_FormulaData[8, _CurrentRowIndex].Value = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? String.Format("{0:F}", d_objectDropWeight) : String.Format("{0:F3}", d_objectDropWeight);


                                break;
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //if (Lib_Card.Configure.Parameter.Other_Language == 0)
                //    FADM_Form.CustomMessageBox.Show(ex.Message, "更新配方表", MessageBoxButtons.OK, false);
                //else
                //    FADM_Form.CustomMessageBox.Show(ex.Message, "Update recipe table", MessageBoxButtons.OK, false);
            }
        }




        private void Btn_Change_Click(object sender, EventArgs e)
        {
            if (dgv_FormulaData.Rows.Count > 1)
            {
                foreach (DataGridViewRow dataRow in dgv_FormulaData.Rows)
                {
                    if (dataRow.Index < dgv_FormulaData.Rows.Count - 1)
                        dataRow.Cells[9].Value = "0.00";
                }
                dgv_DropRecord.ClearSelection();
                dgv_FormulaData.Enabled = true;
            }


        }



        private void Btn_SendUV_Click(object sender, EventArgs e)
        {
            Send("abs_formula_head", "abs_formula_details");
        }

        private void Btn_SendDrop_Click(object sender, EventArgs e)
        {
            Send("formula_head", "formula_details");
        }

        private void Send(string head, string details)
        {
            if (FADM_Object.Communal._s_operator == "工程师" || FADM_Object.Communal._s_operator.Equals("主管") || FADM_Object.Communal._s_operator.Equals("管理用户"))
            {
                return;
            }

            try
            {
                string s_maxVerNum = "";
                this.dgv_FormulaData.EndEdit();
                Dictionary<string, string> dic_mydic = new Dictionary<string, string>();
            again:
                foreach (DataGridViewRow dgvr in dgv_FormulaData.Rows)
                {
                    if (dgvr.Index < dgv_FormulaData.Rows.Count - 1)
                    {
                        if (dgvr.Cells[1].Value != null)
                        {
                            string s_sql1 = "SELECT *  FROM assistant_details WHERE" +
                                               " AssistantCode = '" + dgvr.Cells[1].Value.ToString() + "' ; ";

                            DataTable dt_assistant = FADM_Object.Communal._fadmSqlserver.GetData(s_sql1);

                            if (dt_assistant.Rows.Count <= 0)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    if (DialogResult.OK == FADM_Form.CustomMessageBox.Show(dgvr.Cells[1].Value.ToString() +
                                    "染助剂代码不存在,请重新输入！", "输入异常", MessageBoxButtons.OK, false))
                                    {
                                        return;
                                    }
                                }
                                else
                                {
                                    if (DialogResult.OK == FADM_Form.CustomMessageBox.Show(dgvr.Cells[1].Value.ToString() +
                                    "Dyeing agent code does not exist, please re-enter！", "Input exception", MessageBoxButtons.OK, false))
                                    {
                                        return;
                                    }
                                }
                            }

                            if (dic_mydic.ContainsKey(dgvr.Cells[1].Value.ToString()))
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    if (DialogResult.OK == FADM_Form.CustomMessageBox.Show(dgvr.Cells[1].Value.ToString() +
                                                                    "染助剂代码重复,请重新输入！", "输入异常", MessageBoxButtons.OK, false))
                                    {
                                        return;
                                    }
                                }
                                else
                                {
                                    if (DialogResult.OK == FADM_Form.CustomMessageBox.Show(dgvr.Cells[1].Value.ToString() +
                                                                    "Dyeing agent code is duplicate, please re-enter！", "Input exception", MessageBoxButtons.OK, false))
                                    {
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                dic_mydic.Add(dgvr.Cells[1].Value.ToString(), dgvr.Cells[1].Value.ToString());
                            }

                        }
                    }
                    for (int i = 0; i < dgv_FormulaData.Columns.Count - 2; i++)
                    {

                        if (dgvr.Cells[i].Value == null || Convert.ToString(dgvr.Cells[i].Value) == "")
                        {
                            try
                            {
                                dgv_FormulaData.Rows.Remove(dgvr);
                                goto again;
                            }
                            catch
                            {
                                break;
                            }
                        }
                        if (i == 8)
                        {
                            if (Convert.ToDouble(dgvr.Cells[8].Value) < 0.1)
                            {

                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    FADM_Form.CustomMessageBox.Show("少于最低滴液量0.1，请核对配方！", "温馨提示", MessageBoxButtons.OK, false);
                                else
                                    FADM_Form.CustomMessageBox.Show("Less than the minimum droplet volume of 0.1, please verify the formula！", "Tips", MessageBoxButtons.OK, false);
                                return;
                            }
                        }
                    }
                }



                if (dgv_FormulaData.Rows.Count == 1)
                {

                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show("当前为空配方,禁止保存!", "温馨提示", MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("The current formula is empty, saving is prohibited!", "Tips", MessageBoxButtons.OK, false);


                    return;
                }

                foreach (DataGridViewRow dgvr in dgv_FormulaData.Rows)
                {
                    UpdataFormulaData(dgvr.Index);
                }

                double d_allDropWeight = 0;

                string d_addWaterWeight = "0.00";
                string s_testTubeObjectAddWaterWeight = "0.00";
                //遍历所有的目标滴液量
                foreach (DataGridViewRow dr in dgv_FormulaData.Rows)
                {
                    //不计算粉重
                    if (Convert.ToInt16(dr.Cells[5].Value) != 200 && Convert.ToInt16(dr.Cells[5].Value) != 201)
                    {
                        d_allDropWeight += Convert.ToDouble(dr.Cells[8].Value);
                    }
                }



                if (d_allDropWeight > Convert.ToDouble(txt_TotalWeight.Text))
                {

                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show("总目标滴液量大于总浴量,请检查配方", "配方异常", MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("The total target droplet volume is greater than the total bath volume, please check the formula", "Formula abnormality", MessageBoxButtons.OK, false);
                    return;
                }








                // 添加配方
                string FormulaName = string.Empty;
                string ClothType = string.Empty;
                string Customer = string.Empty;
                string CupCode = string.Empty;
                double Non_AnhydrationWR = 0;

                //搜索当前配方最大版本号
                string s_sql = "SELECT VersionNum, State,FormulaName,ClothType,Customer,CupCode,Non_AnhydrationWR FROM " + head + " WHERE" +
                                   " FormulaCode = '" + txt_FormulaCode.Text + "'" +
                                   " ORDER BY VersionNum DESC ;";
                DataTable dt_ver = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                if (dt_ver.Rows.Count == 0)
                {
                    txt_VersionNum.Text = "0";

                }
                else
                {
                    string s_versionNum = (Convert.ToInt16(dt_ver.Rows[0][dt_ver.Columns[0]])).ToString();
                    string s_state = dt_ver.Rows[0][dt_ver.Columns[1]].ToString();
                    FormulaName = dt_ver.Rows[0][dt_ver.Columns[2]].ToString();
                    ClothType = dt_ver.Rows[0][dt_ver.Columns[3]].ToString();
                    Customer = dt_ver.Rows[0][dt_ver.Columns[4]].ToString();
                    CupCode = dt_ver.Rows[0][dt_ver.Columns[5]].ToString();
                    Non_AnhydrationWR = Convert.ToDouble(dt_ver.Rows[0][dt_ver.Columns[6]]);
                    if (txt_VersionNum.Text == s_versionNum && (s_state == "已滴定配方" || s_state == "dropped"))
                    {
                        txt_VersionNum.Text = (Convert.ToInt16(dt_ver.Rows[0][dt_ver.Columns[0]]) + 1).ToString();

                    }
                    else
                    {
                        txt_VersionNum.Text = (Convert.ToInt16(dt_ver.Rows[0][dt_ver.Columns[0]])).ToString();
                        s_sql = "DELETE FROM " + details + " WHERE" +
                                   " FormulaCode = '" + txt_FormulaCode.Text + "' AND" +
                                   " VersionNum = '" + txt_VersionNum.Text + "' ;";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                        s_sql = "DELETE FROM " + head + " WHERE" +
                                 " FormulaCode = '" + txt_FormulaCode.Text + "' AND" +
                                 " VersionNum = '" + txt_VersionNum.Text + "' ;";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        s_maxVerNum = txt_VersionNum.Text;
                    }

                }

                string state = string.Empty;
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    state = "尚未滴液";
                else
                    state = "Undropped";

                //计算加水重量
                if (chk_AddWaterChoose.Checked)
                {
                    d_addWaterWeight = Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ?
                        String.Format("{0:F}",
                        Convert.ToDouble(txt_TotalWeight.Text) - d_allDropWeight -
                        Convert.ToDouble(txt_ClothWeight.Text) * Non_AnhydrationWR)
                        :
                        String.Format("{0:F3}",
                        Convert.ToDouble(txt_TotalWeight.Text) - d_allDropWeight -
                        Convert.ToDouble(txt_ClothWeight.Text) * Non_AnhydrationWR);
                }


                //string P_str_sql_3 = "SELECT BottleMinWeight FROM other_parameters WHERE MyID = 1;";
                //DataTable _dt_data = FADM_Object.Communal._fadmSqlserver.GetData(P_str_sql_3);

                double d_bl_bottleAlarmWeight = Lib_Card.Configure.Parameter.Other_Bottle_MinWeight;

                //P_str_sql_3 = "SELECT MachineType FROM machine_parameters WHERE MyID = 1;";
                //_dt_data = FADM_Object.Communal._fadmSqlserver.GetData(P_str_sql_3);

                int P_int_MachineType = Lib_Card.Configure.Parameter.Machine_Bottle_Total;

                string s_bottleLower = null;
                string s_logPastDue = ""; //超过有效时间的染助剂
                                          //添加进配方浏览详细表
                foreach (DataGridViewRow dr in dgv_FormulaData.Rows)
                {
                    if (dr.Index < dgv_FormulaData.RowCount - 1)
                    {
                        List<string> lis_detail = new List<string>();
                        lis_detail.Add(txt_FormulaCode.Text);
                        lis_detail.Add(txt_VersionNum.Text);
                        foreach (DataGridViewColumn dc in dgv_FormulaData.Columns)
                        {
                            try
                            {


                                if (dc.Index == 11)
                                {
                                    if (dgv_FormulaData[dc.Index, dr.Index].Value == null || dgv_FormulaData[dc.Index, dr.Index].Value.ToString() == "")
                                    {
                                        lis_detail.Add("0");
                                        continue;
                                    }
                                    lis_detail.Add(dgv_FormulaData[dc.Index, dr.Index].Value.ToString());
                                    continue;
                                }
                                else if (dc.Index != 10 && dc.Index != 12)
                                    lis_detail.Add(dgv_FormulaData[dc.Index, dr.Index].Value.ToString());
                            }
                            catch
                            {
                                //存在空白行
                                goto head;
                            }
                        }


                        string s_sql_0 = "INSERT INTO " + details + " (" +
                                             " FormulaCode, VersionNum, IndexNum, AssistantCode,AssistantName," +
                                             " FormulaDosage, UnitOfAccount, BottleNum, SettingConcentration," +
                                             " RealConcentration,  ObjectDropWeight, RealDropWeight," +
                                             " BottleSelection) VALUES( '" + lis_detail[0] + "', '" + lis_detail[1] + "'," +
                                             " '" + lis_detail[2] + "', '" + lis_detail[3] + "', '" + lis_detail[4] + "', '" + lis_detail[5] + "'," +
                                             " '" + lis_detail[6] + "', '" + lis_detail[7] + "', '" + lis_detail[8] + "', '" + lis_detail[9] + "'," +
                                             " '" + lis_detail[10] + "', '" + lis_detail[11] + "', '" + lis_detail[12] + "');";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql_0);

                        if (Convert.ToInt16(lis_detail[7]) <= P_int_MachineType)
                        {

                            s_sql_0 = "SELECT CurrentWeight FROM bottle_details WHERE" +
                                          " BottleNum = '" + lis_detail[7] + "';";

                            DataTable dt_currentWeight = FADM_Object.Communal._fadmSqlserver.GetData(s_sql_0);

                            double d_bl_CurrentWeight = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", dt_currentWeight.Rows[0][0]) : string.Format("{0:F}", dt_currentWeight.Rows[0][0]));

                            if (d_bl_CurrentWeight <= d_bl_bottleAlarmWeight)
                            {
                                s_bottleLower += (lis_detail[7] + " ");
                            }

                            s_sql_0 = "SELECT BrewingData FROM bottle_details WHERE" +
                                          " BottleNum = '" + lis_detail[7] + "';";

                            DataTable BrewingData = FADM_Object.Communal._fadmSqlserver.GetData(s_sql_0);
                            DateTime brewTime = Convert.ToDateTime(BrewingData.Rows[0][0]);   //调液日期 
                            s_sql_0 = "SELECT TermOfValidity  FROM assistant_details WHERE" +
                                         " AssistantCode = '" + lis_detail[3] + "' ; ";
                            DataTable dt_assistant = FADM_Object.Communal._fadmSqlserver.GetData(s_sql_0);
                            string s_termOfValidity = dt_assistant.Rows[0][0].ToString();//染助剂有效期限
                                                                                         //获取当前时间
                            DateTime timeNow = DateTime.Now;
                            //计算时间差
                            UInt32 timeDifference = Convert.ToUInt32(timeNow.Subtract(brewTime).Duration().TotalSeconds);

                            if (timeDifference > Convert.ToUInt32(s_termOfValidity) * 60 * 60)
                            {
                                s_logPastDue += (lis_detail[7] + "  ");
                            }

                        }

                    }

                }

            head:
                List<string> lis_head = new List<string>();
                lis_head.Add(txt_FormulaCode.Text);
                lis_head.Add(txt_VersionNum.Text);
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    lis_head.Add(state);
                else
                {
                    if (state == "Undropped")
                    {
                        lis_head.Add("尚未滴液");
                    }
                    else
                    {
                        lis_head.Add("已滴定配方");
                    }
                }
                lis_head.Add(FormulaName);
                lis_head.Add(ClothType);
                lis_head.Add(Customer);
                lis_head.Add(chk_AddWaterChoose.Checked == false ? "0" : "1");
                lis_head.Add("0");
                lis_head.Add(txt_ClothWeight.Text);
                lis_head.Add(txt_BathRatio.Text);
                lis_head.Add(txt_TotalWeight.Text);

                lis_head.Add(FADM_Object.Communal._s_operator);
                lis_head.Add(CupCode);
                lis_head.Add(DateTime.Now.ToString());
                lis_head.Add(d_addWaterWeight);
                lis_head.Add(s_testTubeObjectAddWaterWeight);

                lis_head.Add(Non_AnhydrationWR.ToString());

                lis_head.Add("滴液");

                // 添加进配方浏览表头
                string s_sql_1 = "INSERT INTO " + head + " (" +
                                     " FormulaCode, VersionNum, State, FormulaName," +
                                     " ClothType,Customer,AddWaterChoose,CompoundBoardChoose,ClothWeight," +
                                     " BathRatio,TotalWeight,Operator,CupCode,CreateTime," +
                                     " ObjectAddWaterWeight,TestTubeObjectAddWaterWeight,Non_AnhydrationWR,Stage) VALUES('" + lis_head[0] + "'," +
                                     " '" + lis_head[1] + "', '" + lis_head[2] + "', '" + lis_head[3] + "', " +
                                     " '" + lis_head[4] + "', '" + lis_head[5] + "', '" + lis_head[6] + "', " +
                                     " '" + lis_head[7] + "', '" + lis_head[8] + "', '" + lis_head[9] + "', " +
                                     " '" + lis_head[10] + "', '" + lis_head[11] + "', '" + lis_head[12] + "', " +
                                     " '" + lis_head[13] + "', '" + lis_head[14] + "', '" + lis_head[15] + "', '" + lis_head[16] + "', '" + lis_head[17] + "');";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql_1);









                if (s_bottleLower != null) //安全存量真才会进行弹框
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show(s_bottleLower + "号母液瓶液量不足！", "母液量不足", MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show(" Insufficient liquid volume in the " + s_bottleLower + " mother liquor bottle！", "Insufficient mother liquor volume", MessageBoxButtons.OK, false);
                }
                //这里加个生命周期检查 过期了就提示
                if (!string.IsNullOrEmpty(s_logPastDue))
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show(s_logPastDue + "号母液瓶过期！", "过期", MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show(s_logPastDue + "mother liquor bottle expired！", "expire", MessageBoxButtons.OK, false);
                }
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    if (head == "abs_formula_head")
                    {
                        FADM_Form.CustomMessageBox.Show("发送至吸光度机完成", "温馨提示", MessageBoxButtons.OK, false);
                    }
                    else
                    {
                        FADM_Form.CustomMessageBox.Show("发送至滴液机完成", "温馨提示", MessageBoxButtons.OK, false);
                    }

                }

                else
                {
                    FADM_Form.CustomMessageBox.Show("Save completed", "Tips", MessageBoxButtons.OK, false);
                }










            }
            catch (Exception ex)
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    FADM_Form.CustomMessageBox.Show(ex.Message, "发送至XX机点击事件", MessageBoxButtons.OK, false);
                else
                    FADM_Form.CustomMessageBox.Show(ex.Message, "Save Click Event", MessageBoxButtons.OK, false);
            }
        }

        private void Rdo_Record_CheckedChanged(object sender, EventArgs e)
        {
            if (Rdo_Record.Checked)
            {
                b_isRecord = true;
                dgv_DropRecord.ContextMenuStrip = null;
            }
            else
            {
                b_isRecord = false;
                dgv_DropRecord.ContextMenuStrip = contextMenuStrip1;
                InitChart();
                myDataGridView1.Rows.Clear();
            }
        }
        //记录组合模式下标样数据
        string _s_data=null;
        string _s_L = null;
        string _s_A = null;
        string _s_B = null;

        private void Tsm_Insert_Click(object sender, EventArgs e)
        {
            if (dgv_DropRecord.CurrentRow == null)
            {
                return;
            }

            txt_dL.Text = "";
            txt_dA.Text = "";
            txt_dB.Text = "";
            txt_dE.Text = "";

            txt_RealConcentration.Text = "";
            txt_BottleNum.Text = "";
            txt_BrewingData.Text = "";
            label20.Text = "";
            label15.Text = "";
            label16.Text = "";
            textBox4.Text = "";
            txt_BaseData.Text = "";
            textBox1.Text = "";

            txt_FormulaCode.Text = "";
            txt_VersionNum.Text = "";
            txt_ClothWeight.Text = "";
            txt_TotalWeight.Text = "";
            txt_BathRatio.Text = "";
            chk_AddWaterChoose.Checked = false;
            dgv_FormulaData.Rows.Clear();
            dgv_FormulaData.Enabled = false;
            //if (chart.Series.Count > 0)
            //{
            //    chart.Series.Clear();
            //    chart.MouseMove -= new MouseEventHandler(chart1_MouseMove);
            //    chart.MouseWheel -= new MouseEventHandler(chart1_MouseMove);
            //}
            //toolTip1.RemoveAll();

            //读取选中行对应的配方资料
            string s_finishtime = dgv_DropRecord.CurrentRow.Cells[3].Value.ToString();
            string s_sql = "SELECT * FROM history_abs" +
                               " Where FinishTime = '" + s_finishtime + "';";
            DataTable dt_history_abs = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (dt_history_abs.Rows.Count > 0)
            {
                i_start = Convert.ToInt32(dt_history_abs.Rows[0]["StartWave"]);
                i_end = Convert.ToInt32(dt_history_abs.Rows[0]["EndWave"]);
                i_int = Convert.ToInt32(dt_history_abs.Rows[0]["IntWave"]);
                string s_data = dt_history_abs.Rows[0]["Abs"] is DBNull ? "" : dt_history_abs.Rows[0]["Abs"].ToString();
                Show(s_data, dt_history_abs.Rows[0]["AssistantCode"].ToString());

                if (double.TryParse(dt_history_abs.Rows[0]["A"].ToString(), out double a) && double.TryParse(dt_history_abs.Rows[0]["B"].ToString(), out double b))
                {
                    // 限制 a/b 范围在 [-128, 127]
                    a = Clamp(a, -128, 127);
                    b = Clamp(b, -128, 127);

                    _points.Add(new LabPoint(a, b, _nextPointIndex));
                    lstPoints.Items.Add($"点 {_nextPointIndex}: a={a:F1}, b={b:F1}");
                    _nextPointIndex++;

                    _panel3.Invalidate(); // 触发 Panel 重绘
                }
                else
                {
                    //MessageBox.Show("请输入有效的数字！");
                }
                //设置为标样
                if(chart.Series.Count == 1)
                {
                    _s_data = s_data;
                    _s_data = _s_data.Substring(0, _s_data.Length - 2);
                    _s_L = dt_history_abs.Rows[0]["L"].ToString();
                    _s_A = dt_history_abs.Rows[0]["A"].ToString();
                    _s_B = dt_history_abs.Rows[0]["B"].ToString();
                    myDataGridView1.Rows.Add("1", dt_history_abs.Rows[0]["L"].ToString(), dt_history_abs.Rows[0]["A"].ToString(), dt_history_abs.Rows[0]["B"].ToString(),"-","-","-", "-", "-", "-","-");
                    //textBox5.Text = MyAbsorbance.SWL(doublesS, doublesT).ToString("F2") + "%";
                    //textBox6.Text = MyAbsorbance.SUM(doublesS, doublesT, i_start, i_end, Convert.ToInt32(dt_history_abs.Rows[0]["IntWave"])).ToString("F2") + "%";
                    //textBox7.Text = MyAbsorbance.WSUM(doublesS, doublesT, i_start, i_end, Convert.ToInt32(dt_history_abs.Rows[0]["IntWave"]), 10).ToString("F2") + "%";

                }
                else
                {
                    
                    s_data = s_data.Substring(0, s_data.Length - 2);
                    string[] sa_arr1 = s_data.Split('/');

                    string[] sa_arrS = _s_data.Split('/');

                    int i_index = 0;
                    for (int i = 0; i < sa_arr1.Count(); i++)
                    {
                        if (Convert.ToInt32(dt_history_abs.Rows[0]["StartWave"]) + i * Convert.ToInt32(dt_history_abs.Rows[0]["IntWave"]) >= 400)
                        {
                            i_index = i;
                            break;
                        }
                    }

                    double[] doublesS = new double[sa_arrS.Count() - i_index];
                    double[] doublesT = new double[sa_arr1.Count() - i_index];
                    for (int i = 0; i < sa_arrS.Count() - i_index; i++)
                    {
                        doublesS[i] = Convert.ToDouble(sa_arrS[i + i_index]);
                    }
                    for (int i = 0; i < sa_arr1.Count() - i_index; i++)
                    {
                        doublesT[i] = Convert.ToDouble(sa_arr1[i + i_index]);
                    }
                    double d_cmc = MyAbsorbance.CalculateCMC(Convert.ToDouble(_s_L), Convert.ToDouble(_s_A), Convert.ToDouble(_s_B), Convert.ToDouble(dt_history_abs.Rows[0]["L"].ToString()), Convert.ToDouble(dt_history_abs.Rows[0]["A"].ToString()), Convert.ToDouble(dt_history_abs.Rows[0]["B"].ToString()), 2, 1);
                    string de = d_cmc.ToString("f2");
                    double db_dL = Convert.ToDouble(dt_history_abs.Rows[0]["L"].ToString()) - Convert.ToDouble(_s_L);
                    string dl = db_dL.ToString("f2");
                    double db_dA = Convert.ToDouble(dt_history_abs.Rows[0]["A"].ToString()) - Convert.ToDouble(_s_A);
                    string da = db_dA.ToString("f2");
                    double db_dB = Convert.ToDouble(dt_history_abs.Rows[0]["B"].ToString()) - Convert.ToDouble(_s_B);
                    string db = db_dB.ToString("f2");
                    string swl = MyAbsorbance.SWL(doublesT, doublesS).ToString("F2") + "%";
                    string sum = MyAbsorbance.SUM(doublesT, doublesS, i_start, i_end, Convert.ToInt32(dt_history_abs.Rows[0]["IntWave"])).ToString("F2") + "%";
                    string wsum = MyAbsorbance.WSUM(doublesT, doublesS, i_start, i_end, Convert.ToInt32(dt_history_abs.Rows[0]["IntWave"]), 10).ToString("F2") + "%";
                    myDataGridView1.Rows.Add(chart.Series.Count.ToString(), dt_history_abs.Rows[0]["L"].ToString(), dt_history_abs.Rows[0]["A"].ToString(), dt_history_abs.Rows[0]["B"].ToString(),
                        dl, da, db, de,swl, sum,wsum);
                   
                }
            }


        }
        private static readonly Random _rand = new Random();
        private void Show(string s_data, string note)
        {
           
            s_data = s_data.Substring(0, s_data.Length - 2);
            string[] sa_arr = s_data.Split('/');

            //times = new DateTime[sa_arr.Count()];
            //for (int i = 0; i < sa_arr.Count(); i++)
            //{
            //    times[i] = dateTime.AddSeconds((i - sa_arr.Count()) * 30);
            //}
            int lenght = chart.Series.Count;
            AddSeries(lenght+1 +"-"+note, Color.FromArgb(255,
                            _rand.Next(0, 256),
                            _rand.Next(0, 256),
                            _rand.Next(0, 256)
                     ));
            
            Series series = chart.Series[lenght];
            //series.Points.AddXY(0, 0);

            for (int i = 0; i < sa_arr.Count(); i++)
            {
                series.Points.AddXY(Convert.ToDouble(i_start + i * i_int), Convert.ToDouble(sa_arr[i]));
            }
            chart.MouseMove += new MouseEventHandler(chart1_MouseMove);

            //chart.MouseWheel += new System.Windows.Forms.MouseEventHandler(chart1_Mouselheel);


        }

        private void Tsm_Reset_Click(object sender, EventArgs e)
        {
            InitChart();

            _points.Clear();
            lstPoints.Items.Clear();
            _nextPointIndex = 1;
            _panel3.Invalidate(); // 触发 Panel 重绘

            myDataGridView1.Rows.Clear();
        }

        // 初始化所有控件
        private void InitializeComponents()
        {
            //// ---------- 初始化绘图 Panel ----------
            //_drawingPanel = new Panel
            //{
            //    Location = this.panel3.Location,
            //    Size = this.panel3.Size,
            //    BorderStyle = BorderStyle.FixedSingle // 可选：显示 Panel 边界
            //};
            //_drawingPanel.Paint += DrawingPanel_Paint; // 绑定 Paint 事件
            //this.panel3.Controls.Add(_drawingPanel);

            //// ---------- 初始化输入控件 ----------
            //// a 值标签 + 输入框
            //Label lblA = new Label
            //{
            //    Text = "a 值:",
            //    Location = new Point(500, 100),
            //    Size = new Size(50, 20)
            //};
            //txtA = new TextBox
            //{
            //    Location = new Point(550, 100),
            //    Size = new Size(60, 20),
            //    Text = "0"
            //};

            //// b 值标签 + 输入框
            //Label lblB = new Label
            //{
            //    Text = "b 值:",
            //    Location = new Point(500, 130),
            //    Size = new Size(50, 20)
            //};
            //txtB = new TextBox
            //{
            //    Location = new Point(550, 130),
            //    Size = new Size(60, 20),
            //    Text = "0"
            //};

            //// 添加点按钮
            //btnAddPoint = new Button
            //{
            //    Text = "添加点",
            //    Location = new Point(550, 160),
            //    Size = new Size(60, 30)
            //};
            //btnAddPoint.Click += BtnAddPoint_Click;

            //// 清除所有点按钮
            //btnClearPoints = new Button
            //{
            //    Text = "清除所有点",
            //    Location = new Point(500, 200),
            //    Size = new Size(110, 30)
            //};
            //btnClearPoints.Click += BtnClearPoints_Click;

            //// 已添加点的列表
            //Label lblPoints = new Label
            //{
            //    Text = "已添加的点:",
            //    Location = new Point(500, 240),
            //    Size = new Size(110, 20)
            //};
            lstPoints = new ListBox
            {
                Location = new Point(500, 260),
                Size = new Size(150, 150)
            };

            // ---------- 2. 初始化 panel3（假设通过设计器拖放，名称为 panel3） ----------
            _panel3 = this.grp_FormulaData.Controls["panel3"] as Panel; // 从窗体控件集合中获取 panel3
            if (_panel3 != null)
            {
                //_panel3.DoubleBuffered = true; // panel3 自身双缓冲（减少绘图闪烁）
                _panel3.Paint += Panel3_Paint; // 绑定 Paint 事件
            }
            else
            {
                // 若 panel3 不存在，创建一个示例 panel3（可根据实际需求替换为设计器拖放）
                _panel3 = new Panel
                {
                    Location = new Point(20, 20),
                    Size = new Size(450, 550),
                    BorderStyle = BorderStyle.FixedSingle,
                    Name = "panel3" // 命名为 panel3，确保和需求一致
                };
                _panel3.Paint += Panel3_Paint;
                Controls.Add(_panel3);
            }


            // 将控件添加到窗体
            //Controls.Add(lblA);
            //Controls.Add(txtA);
            //Controls.Add(lblB);
            //Controls.Add(txtB);
            //Controls.Add(btnAddPoint);
            //Controls.Add(btnClearPoints);
            //Controls.Add(lblPoints);
            Controls.Add(lstPoints);
        }

        // ---------- panel3 的 Paint 事件：核心绘图逻辑 ----------
        private void Panel3_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias; // 抗锯齿，使图形更平滑

            // 基于 panel3 的尺寸计算色轮参数（中心 + 半径）
            int centerX = _panel3.Width / 2;
            int centerY = _panel3.Height / 2;
            int radius = Math.Min(centerX, centerY) - 20; // 色轮半径（留 20px 边距）

            // 1. 绘制 L*a*b* 色轮
            DrawColorWheel(g, centerX, centerY, radius);

            // 2. 绘制坐标轴（+a/-a, +b/-b）
            DrawCoordinates(g, centerX, centerY, radius);

            // 3. 绘制 L 轴灰度条（位于 panel3 右侧内部）
            //DrawLScaleInPanel(g, _panel3);

            // 4. 绘制所有已添加的点
            DrawAllPoints(g, centerX, centerY, radius);

            // 5. 绘制说明文字
            DrawLabels(g, centerX, centerY, radius);
        }

        // Panel 的 Paint 事件：绘制色轮、坐标、点等
        //private void DrawingPanel_Paint(object sender, PaintEventArgs e)
        //{
        //    Graphics g = e.Graphics;
        //    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; // 抗锯齿

        //    int centerX = _drawingPanel.Width / 2;   // Panel 中心 X
        //    int centerY = _drawingPanel.Height / 2;  // Panel 中心 Y
        //    int radius = Math.Min(centerX, centerY) - 20; // 色轮半径（避免超出 Panel）

        //    // 1. 绘制色轮
        //    DrawColorWheel(g, centerX, centerY, radius);

        //    // 2. 绘制坐标轴（+a/-a, +b/-b）
        //    DrawCoordinates(g, centerX, centerY, radius);

        //    // 3. 绘制 L 轴灰度条（位于 Panel 右侧）
        //    int lScaleX = _drawingPanel.Right + 10;
        //    int lScaleY = _drawingPanel.Top;
        //    DrawLScale(g, lScaleX, lScaleY);

        //    // 4. 绘制所有已添加的点
        //    DrawAllPoints(g, centerX, centerY, radius);

        //    // 5. 绘制说明文字
        //    DrawLabels(g, centerX, centerY, radius);
        //}

        // 绘制 L*a*b* 色轮
        private void DrawColorWheel(Graphics g, int centerX, int centerY, int radius)
        {
            // 用小扇形填充色轮（步长 1°，保证平滑）
            for (int angle = 0; angle < 360; angle++)
            {
                using (GraphicsPath path = new GraphicsPath())
                {
                    // 计算色轮边缘点
                    float x1 = centerX + (float)(radius * Math.Cos(angle * Math.PI / 180));
                    float y1 = centerY + (float)(radius * Math.Sin(angle * Math.PI / 180));

                    path.AddLine(centerX, centerY, x1, y1);
                    path.AddArc(centerX - radius, centerY - radius,
                                radius * 2, radius * 2, angle, 1);

                    // 下一个角度的点
                    float x2 = centerX + (float)(radius * Math.Cos((angle + 1) * Math.PI / 180));
                    float y2 = centerY + (float)(radius * Math.Sin((angle + 1) * Math.PI / 180));

                    path.AddLine(x2, y2, centerX, centerY);

                    // 计算当前角度对应的 a/b 值，生成颜色
                    double a = 100 * Math.Cos(angle * Math.PI / 180);
                    double b = 100 * Math.Sin(angle * Math.PI / 180);
                    Color color = LabToRgb(70, a, b);

                    using (SolidBrush brush = new SolidBrush(color))
                    {
                        g.FillPath(brush, path);
                    }
                }
            }

            // 绘制中心灰圆（a=0, b=0）
            using (SolidBrush brush = new SolidBrush(LabToRgb(70, 0, 0)))
            {
                g.FillEllipse(brush, centerX - 30, centerY - 30, 60, 60);
            }

            // 绘制色轮边框
            using (Pen pen = new Pen(Color.Black, 2))
            {
                g.DrawEllipse(pen, centerX - radius, centerY - radius, radius * 2, radius * 2);
            }
        }

        // 绘制坐标轴（+a/-a, +b/-b 标注）
        private void DrawCoordinates(Graphics g, int centerX, int centerY, int radius)
        {
            // 绘制 a 轴（水平：红-绿）
            using (Pen pen = new Pen(Color.Black, 1))
            {
                g.DrawLine(pen, centerX - radius, centerY, centerX + radius, centerY);
            }

            // 绘制 b 轴（垂直：黄-蓝）
            using (Pen pen = new Pen(Color.Black, 1))
            {
                g.DrawLine(pen, centerX, centerY - radius, centerX, centerY + radius);
            }

            // 绘制刻度线
            int tickCount = 8;
            for (int i = 1; i <= tickCount; i++)
            {
                // a 轴刻度
                int x = (int)(centerX - radius + (i * 2 * radius / (tickCount + 1)));
                g.DrawLine(Pens.Black, x, centerY - 5, x, centerY + 5);

                // b 轴刻度
                int y = (int)(centerY - radius + (i * 2 * radius / (tickCount + 1)));
                g.DrawLine(Pens.Black, centerX - 5, y, centerX + 5, y);
            }

            // 绘制坐标标签
            using (Font font = new Font("Arial", 10, FontStyle.Bold))
            {
                g.DrawString("+a", font, Brushes.Black, centerX + radius + 5, centerY - 10);
                g.DrawString("-a", font, Brushes.Black, centerX - radius - 25, centerY - 10);
                g.DrawString("+b", font, Brushes.Black, centerX - 10, centerY - radius - 15);
                g.DrawString("-b", font, Brushes.Black, centerX - 10, centerY + radius + 5);
            }

            // 绘制数值范围（-128 ~ 127）
            using (Font font = new Font("Arial", 8))
            {
                g.DrawString("127", font, Brushes.Black, centerX + radius - 15, centerY + 10);
                g.DrawString("-128", font, Brushes.Black, centerX - radius + 5, centerY + 10);
                g.DrawString("127", font, Brushes.Black, centerX + 10, centerY - radius + 5);
                g.DrawString("-128", font, Brushes.Black, centerX + 10, centerY + radius - 10);
            }
        }

        // 绘制所有已添加的点
        private void DrawAllPoints(Graphics g, int centerX, int centerY, int radius)
        {
            foreach (LabPoint point in _points)
            {
                // 归一化 a/b 值到 [-1, 1]
                double normalizedA = point.A / 128.0;
                double normalizedB = point.B / 128.0;

                // 计算极径（饱和度，限制在色轮内）
                double r = Math.Sqrt(normalizedA * normalizedA + normalizedB * normalizedB);
                r = Math.Min(r, 1.0);

                // 计算极角（色相）
                double angle = Math.Atan2(normalizedB, normalizedA) * 180 / Math.PI;
                if (angle < 0) angle += 360;

                // 极坐标转直角坐标
                int pointX = (int)(centerX + r * radius * Math.Cos(angle * Math.PI / 180));
                int pointY = (int)(centerY + r * radius * Math.Sin(angle * Math.PI / 180));

                // 绘制点的黑色外环
                using (Pen pen = new Pen(Color.Black, 2))
                {
                    g.DrawEllipse(pen, pointX - 6, pointY - 6, 12, 12);
                }

                // 绘制点的颜色（根据 a/b 计算）
                Color pointColor = LabToRgb(70, point.A, point.B);
                using (SolidBrush brush = new SolidBrush(pointColor))
                {
                    g.FillEllipse(brush, pointX - 4, pointY - 4, 8, 8);
                }

                // 绘制点的编号
                using (Font font = new Font("Arial", 8, FontStyle.Bold))
                {
                    string label = point.Index.ToString();
                    SizeF textSize = g.MeasureString(label, font);
                    g.DrawString(label, font, Brushes.White,
                        pointX - textSize.Width / 2,
                        pointY - textSize.Height / 2);
                }
            }
        }

        // 绘制 L 轴灰度条（L=0 ~ L=100）
        private void DrawLScale(Graphics g, int x, int y)
        {
            int width = 40;
            int height = 400;

            // 从 L=100（上）到 L=0（下）绘制灰度渐变
            for (int i = 0; i < height; i++)
            {
                double l = 100 - (i / (double)height) * 100;
                Color gray = LabToRgb(l, 0, 0); // a=0, b=0 时为纯灰度
                using (SolidBrush brush = new SolidBrush(gray))
                {
                    g.FillRectangle(brush, x, y + i, width, 1);
                }
            }

            // 绘制灰度条边框
            using (Pen pen = new Pen(Color.Black, 1))
            {
                g.DrawRectangle(pen, x, y, width, height);
            }

            // 绘制 L 值标签
            using (Font font = new Font("Arial", 8))
            {
                g.DrawString("L=100", font, Brushes.Black, x + width + 5, y);
                g.DrawString("L=0", font, Brushes.Black, x + width + 5, y + height - 15);
            }
        }

        // 绘制说明文字
        private void DrawLabels(Graphics g, int centerX, int centerY, int radius)
        {
            using (Font font = new Font("Arial", 10))
            {
                g.DrawString("L*a*b* 色轮", font, Brushes.Black, centerX - 50, centerY + radius + 20);
                g.DrawString("红", font, Brushes.Black, centerX + radius + 15, centerY - 20);
                g.DrawString("绿", font, Brushes.Black, centerX - radius - 20, centerY - 20);
                g.DrawString("黄", font, Brushes.Black, centerX + 20, centerY - radius - 20);
                g.DrawString("蓝", font, Brushes.Black, centerX + 20, centerY + radius + 5);
            }
        }

        // 「添加点」按钮点击事件
        //private void BtnAddPoint_Click(object sender, EventArgs e)
        //{
        //    if (double.TryParse(txtA.Text, out double a) && double.TryParse(txtB.Text, out double b))
        //    {
        //        // 限制 a/b 范围在 [-128, 127]
        //        a = Clamp(a, -128, 127);
        //        b = Clamp(b, -128, 127);

        //        _points.Add(new LabPoint(a, b, _nextPointIndex));
        //        lstPoints.Items.Add($"点 {_nextPointIndex}: a={a:F1}, b={b:F1}");
        //        _nextPointIndex++;

        //        _drawingPanel.Invalidate(); // 触发 Panel 重绘
        //    }
        //    else
        //    {
        //        MessageBox.Show("请输入有效的数字！");
        //    }
        //}

        // 「清除所有点」按钮点击事件
        private void BtnClearPoints_Click(object sender, EventArgs e)
        {
            //_points.Clear();
            //lstPoints.Items.Clear();
            //_nextPointIndex = 1;
            //_drawingPanel.Invalidate(); // 触发 Panel 重绘
        }

        // L*a*b* → RGB 颜色转换（精确转换）
        private Color LabToRgb(double l, double a, double b)
        {
            // Lab → XYZ
            double y = (l + 16) / 116.0;
            double x = a / 500.0 + y;
            double z = y - b / 200.0;

            // 反向压缩（非线性 → 线性）
            x = x > 0.206893 ? Math.Pow(x, 3) : (x - 16.0 / 116.0) / 7.787;
            y = l > 8 ? Math.Pow(y, 3) : l / 903.3;
            z = z > 0.206893 ? Math.Pow(z, 3) : (z - 16.0 / 116.0) / 7.787;

            // 应用 D65 参考白点
            x *= 0.95047;
            z *= 1.08883;

            // XYZ → sRGB（线性转换）
            double r = x * 3.2406 + y * -1.5372 + z * -0.4986;
            double g = x * -0.9689 + y * 1.8758 + z * 0.0415;
            double bColor = x * 0.0557 + y * -0.2040 + z * 1.0570;

            // 伽马校正（线性 → 非线性，匹配显示器）
            r = r > 0.0031308 ? 1.055 * Math.Pow(r, 1.0 / 2.4) - 0.055 : 12.92 * r;
            g = g > 0.0031308 ? 1.055 * Math.Pow(g, 1.0 / 2.4) - 0.055 : 12.92 * g;
            bColor = bColor > 0.0031308 ? 1.055 * Math.Pow(bColor, 1.0 / 2.4) - 0.055 : 12.92 * bColor;

            // 范围限制 & 转字节
            int rByte = Clamp((int)(r * 255), 0, 255);
            int gByte = Clamp((int)(g * 255), 0, 255);
            int bByte = Clamp((int)(bColor * 255), 0, 255);

            return Color.FromArgb(rByte, gByte, bByte);
        }

        // 限制值在 [min, max] 范围内
        private int Clamp(int value, int min, int max)
        {
            return Math.Min(Math.Max(value, min), max);
        }

        // 限制值在 [min, max] 范围内
        private double Clamp(double value, double min, double max)
        {
            return Math.Min(Math.Max(value, min), max);
        }
    }

    public class LabPoint
    {
        public double A { get; set; }
        public double B { get; set; }
        public int Index { get; set; }

        public LabPoint(double a, double b, int index)
        {
            A = a;
            B = b;
            Index = index;
        }
    }
}
