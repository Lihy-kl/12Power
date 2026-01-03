using Lib_DataBank.MySQL;
using SmartDyeing.FADM_Form;
using SmartDyeing.FADM_Object;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartDyeing.FADM_Control
{
    public partial class DyeingAndFixationProcessConfiguration : UserControl
    {
        List<string> _lis_code = new List<string>();
        public DyeingAndFixationProcessConfiguration()
        {
            InitializeComponent();
            UpdateListAndDyeCode();
            DyeCodeShow();
            HandleCodeShow();
            DyeingCodeShow("");
            DyeDetailsShow(txt_DyeingCode.Text);
            dgv_Combination.ClearSelection();

            dgv_Dye_Set.ClearSelection();
            dgv_Handle_Set.ClearSelection();

            ChildDyeDataShow(txt_DyeingCode.Text, 1);
        }

        private void UpdateListAndDyeCode()
        {
            _lis_code.Clear();

            string s_sql = "SELECT Code  FROM dyeing_process group by Code;";
            DataTable dt_dyeingcode = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            for (int i = 0; i < dt_dyeingcode.Rows.Count; i++)
            {
                _lis_code.Add(dt_dyeingcode.Rows[i][0].ToString());
            }
            _lis_code.Sort();
            for (int i = 0; i < dgv_Combination.Rows.Count; i++)
            {
                DataGridViewComboBoxCell dd = (DataGridViewComboBoxCell)dgv_Combination[0, i];
                dd.DataSource = null;
                dd.DataSource = _lis_code;
            }

        }

        /// <summary>
        /// 显示染固色流程代码
        /// </summary>
        /// <param name="s_dyeingCode">选中的染固色流程代码</param>
        /// <returns>0:正常;-1:异常</returns>
        private int DyeingCodeShow(string s_dyeingCode)
        {
            try
            {
                //获取染固色流程代码
                string s_sql = "SELECT DyeingCode  FROM dyeing_code group by DyeingCode;";
                DataTable dt_dyeingcode = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                //捆绑
                dgv_DyeingCode.DataSource = new DataView(dt_dyeingcode);
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    //设置标题文字
                    dgv_DyeingCode.Columns[0].HeaderCell.Value = "染固色流程代码";
                else
                    dgv_DyeingCode.Columns[0].HeaderCell.Value = "DyeCode";

                //设置标题宽度
                dgv_DyeingCode.Columns[0].Width = dgv_DyeingCode.Width - 3;

                //关闭点击标题自动排序功能
                dgv_DyeingCode.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;

                //设置标题居中显示
                dgv_DyeingCode.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                //设置标题字体
                dgv_DyeingCode.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 14.25F);

                //设置内容居中显示
                dgv_DyeingCode.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                //设置内容字体
                dgv_DyeingCode.RowsDefaultCellStyle.Font = new Font("宋体", 14.25F);

                //设置行高
                dgv_DyeingCode.RowTemplate.Height = 30;

                //设置当前选中行
                for (int i = 0; i < dgv_DyeingCode.Rows.Count; i++)
                {
                    string s = dgv_DyeingCode.Rows[i].Cells[0].Value.ToString();
                    if (s == s_dyeingCode)
                    {
                        dgv_DyeingCode.CurrentCell = dgv_DyeingCode.Rows[i].Cells[0];
                        break;
                    }
                }

                return 0;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// 显示染固色代码对应详情
        /// </summary>
        /// <param name="s_dyeingCode">选中的染固色流程代码</param>
        /// <returns>0:正常;-1:异常</returns>
        private int DyeDetailsShow(string s_dyeingCode)
        {
            try
            {
                dgv_Combination.Rows.Clear();
                txt_Remark.Text = "";

                //获取染固色流程代码
                string s_sql = "SELECT Code,IsUse,Remark  FROM dyeing_code where DyeingCode = '" + s_dyeingCode + "' order by IndexNum;";
                DataTable dt_dyeingcode = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                

                //////捆绑
                //dgv_DyeDetails.DataSource = new DataView(dt_dyeingcode);
                //if()
                //dgv_Combination.Columns[0].HeaderCell.Value = "流程代码";
                //设置标题宽度
                dgv_Combination.Columns[0].Width = dgv_Combination.Width - 3;
                //关闭点击标题自动排序功能
                dgv_Combination.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;


                //设置标题居中显示
                dgv_Combination.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                //设置标题字体
                dgv_Combination.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 14.25F);

                //设置内容居中显示
                dgv_Combination.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                //设置内容字体
                dgv_Combination.RowsDefaultCellStyle.Font = new Font("宋体", 14.25F);

                //设置行高
                dgv_Combination.RowTemplate.Height = 30;
                if(dt_dyeingcode.Rows.Count>0)
                {
                    if(dt_dyeingcode.Rows[0][1].ToString()=="0")
                    {
                        checkBox1.Checked = false;
                    }
                    else
                    {
                        checkBox1.Checked = true;
                    }
                    txt_Remark.Text = dt_dyeingcode.Rows[0][2].ToString();
                }

                for (int i = 0; i < dt_dyeingcode.Rows.Count; i++)
                {
                    dgv_Combination.Rows.Add();
                    DataGridViewComboBoxCell dd = (DataGridViewComboBoxCell)dgv_Combination[0, i];
                    dd.DataSource = _lis_code;
                    if (_lis_code.Contains(dt_dyeingcode.Rows[i][0].ToString()))
                        dd.Value = dt_dyeingcode.Rows[i][0].ToString();
                }
                //新增行下拉选项
                {
                    DataGridViewComboBoxCell dd = (DataGridViewComboBoxCell)dgv_Combination[0, dgv_Combination.Rows.Count - 1];
                    dd.DataSource = _lis_code;
                }


                ////设置当前选中行
                //for (int i = 0; i < dgv_DyeDetails.Rows.Count; i++)
                //{
                //    string s_temp = dgv_DyeDetails.Rows[i].Cells[0].Value.ToString();
                //    if (s_temp == s_dyeingCode)
                //    {
                //        dgv_DyeDetails.CurrentCell = dgv_DyeDetails.Rows[i].Cells[0];
                //        break;
                //    }
                //}

                return 0;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// 显示染色步骤
        /// </summary>
        /// <param name="s_name">工艺名称或者染固色代码</param>
        /// <param name="i_type">1：代表显示染固色详情;2：代表显示工艺详情</param>
        /// <returns>0:正常;-1:异常</returns>
        public int ChildDyeDataShow(string s_name,int i_type)
        {
            try
            {
                if (i_type == 1)
                {
                    dgv_details.Rows.Clear();
                    string s_sql = "SELECT Code  FROM dyeing_code where DyeingCode = '" + s_name + "' order by IndexNum;";
                    DataTable dt_dyeingcode = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                    int num = 0;
                    foreach (DataRow dr in dt_dyeingcode.Rows)
                    {
                        string s_sql1 = "SELECT StepNum,TechnologyName,Temp,Rate,ProportionOrTime,Rev  FROM dyeing_process WHERE" +
                                       " Code = '" + dr["Code"].ToString() + "' Order By StepNum ; ";
                        DataTable dt_dyeingprocess = FADM_Object.Communal._fadmSqlserver.GetData(s_sql1);
                        foreach (DataRow dr1 in dt_dyeingprocess.Rows)
                        {
                            num ++;
                            string s_stepNum =num.ToString();
                            string s_technologyName = dr1["TechnologyName"] is DBNull ? "" : dr1["TechnologyName"].ToString();
                            string s_temp = dr1["Temp"] is DBNull ? "" : dr1["Temp"].ToString();
                            string s_rate = dr1["Rate"] is DBNull ? "" : dr1["Rate"].ToString();
                            string s_proportionOrTime = dr1["ProportionOrTime"] is DBNull ? "" : dr1["ProportionOrTime"].ToString();
                            string s_rev = dr1["Rev"] is DBNull ? "" : dr1["Rev"].ToString();

                            dgv_details.Rows.Add(s_stepNum, s_technologyName, s_temp, s_rate, s_proportionOrTime, s_rev);
                        }
                    }

                    dgv_details.ClearSelection();
                    return 0;
                }
                else
                {
                    dgv_details.Rows.Clear();

                    //获取当前调液代码的调液流程
                    string s_sql = "SELECT StepNum,TechnologyName,Temp,Rate,ProportionOrTime,Rev  FROM dyeing_process WHERE" +
                                       " Code = '" + s_name + "' Order By StepNum ; ";
                    DataTable dt_dyeingprocess = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                    foreach(DataRow dr in dt_dyeingprocess.Rows) 
                    { 
                        string s_stepNum = dr["StepNum"] is DBNull ? "": dr["StepNum"].ToString();
                        string s_technologyName = dr["TechnologyName"] is DBNull ? "" : dr["TechnologyName"].ToString();
                        string s_temp = dr["Temp"] is DBNull ? "" : dr["Temp"].ToString();
                        string s_rate = dr["Rate"] is DBNull ? "" : dr["Rate"].ToString();
                        string s_proportionOrTime = dr["ProportionOrTime"] is DBNull ? "" : dr["ProportionOrTime"].ToString();
                        string s_rev = dr["Rev"] is DBNull ? "" : dr["Rev"].ToString();

                        dgv_details.Rows.Add(s_stepNum, s_technologyName, s_temp, s_rate, s_proportionOrTime, s_rev);
                    }

                    ////捆绑
                    //dgv_details.DataSource = new DataView(dt_dyeingprocess);

                    ////设置标题栏名称
                    //string[] lineName = { "步号", "操作类型", "温度", "速率", "百分比(%)/时间(s_temp)", "转速" };
                    //for (int i = 0; i < 6; i++)
                    //{
                    //    dgv_details.Columns[i].HeaderCell.Value = lineName[i];
                    //    //设置标题宽度
                    //    dgv_details.Columns[i].Width = (dgv_details.Width - 2) / 6;
                    //    //关闭点击标题自动排序功能
                    //    dgv_details.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                    //}
                    //设置标题居中显示
                    dgv_details.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                    //设置标题字体
                    dgv_details.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 14.25F);

                    //设置内容居中显示
                    dgv_details.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                    //设置内容字体
                    dgv_details.RowsDefaultCellStyle.Font = new Font("宋体", 14.25F);

                    //设置行高
                    dgv_details.RowTemplate.Height = 30;

                    ////设置当前选中行
                    //for (int i = 0; i < dgv_Child_DyeData.Rows.Count; i++)
                    //{
                    //    string s_temp = dgv_Child_DyeData.Rows[i].Cells[0].Value.ToString();
                    //    if (s_temp == _StepNum)
                    //    {
                    //        dgv_Child_DyeData.CurrentCell = dgv_Child_DyeData.Rows[i].Cells[0];
                    //        break;
                    //    }
                    //}

                    dgv_details.ClearSelection();

                    return 0;
                }
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// 显示染色流程代码
        /// </summary>
        /// <returns>0:正常;-1:异常</returns>
        private void DyeCodeShow()
        {
            string s_sql = "SELECT Code  FROM dyeing_process where Type = 1  group by Code Order By Code;";
            DataTable dt_dyeingcode = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

            //捆绑
            dgv_Dye_Set.DataSource = new DataView(dt_dyeingcode);

            if(Lib_Card.Configure.Parameter.Other_Language == 0)
            //设置标题文字
            dgv_Dye_Set.Columns[0].HeaderCell.Value = "染色工艺代码";
            else
                dgv_Dye_Set.Columns[0].HeaderCell.Value = "DyeingProcessCode";

            //设置标题宽度
            dgv_Dye_Set.Columns[0].Width = dgv_Dye_Set.Width - 3;

            //关闭点击标题自动排序功能
            dgv_Dye_Set.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;

            //设置标题居中显示
            dgv_Dye_Set.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            //设置标题字体
            dgv_Dye_Set.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 14.25F);

            //设置内容居中显示
            dgv_Dye_Set.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            //设置内容字体
            dgv_Dye_Set.RowsDefaultCellStyle.Font = new Font("宋体", 14.25F);

            //设置行高
            dgv_DyeingCode.RowTemplate.Height = 30;
        }

        /// <summary>
        /// 显示后处理流程代码
        /// </summary>
        /// <returns>0:正常;-1:异常</returns>
        private void HandleCodeShow()
        {
            string s_sql = "SELECT Code  FROM dyeing_process where Type = 2 group by Code;";
            DataTable dt_dyeingcode = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

            //捆绑
            dgv_Handle_Set.DataSource = new DataView(dt_dyeingcode);
            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                //设置标题文字
                dgv_Handle_Set.Columns[0].HeaderCell.Value = "后处理工艺代码";
            else
                dgv_Handle_Set.Columns[0].HeaderCell.Value = "PostProcessingProcessCode";

            //设置标题宽度
            dgv_Handle_Set.Columns[0].Width = dgv_Handle_Set.Width - 3;

            //关闭点击标题自动排序功能
            dgv_Handle_Set.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;

            //设置标题居中显示
            dgv_Handle_Set.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            //设置标题字体
            dgv_Handle_Set.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 14.25F);

            //设置内容居中显示
            dgv_Handle_Set.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            //设置内容字体
            dgv_Handle_Set.RowsDefaultCellStyle.Font = new Font("宋体", 14.25F);

            //设置行高
            dgv_Handle_Set.RowTemplate.Height = 30;

        }

        private void btn_DyeingCodeAdd_Click(object sender, EventArgs e)
        {
            txt_DyeingCode.Text = null;
            txt_DyeingCode.Enabled = true;
            txt_DyeingCode.Focus();
            checkBox1.Checked = true;
        }

        private void btn_DyeingCodeDelete_Click(object sender, EventArgs e)
        {
            if (dgv_DyeingCode.SelectedRows.Count == 0)
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    FADM_Form.CustomMessageBox.Show("请先选择操作行", "操作异常", MessageBoxButtons.OK, false);
                else
                    FADM_Form.CustomMessageBox.Show("Please select the operation line first", "Abnormal operation", MessageBoxButtons.OK, false);
                return;
            }
            string s_sql = "select COUNT(*) from drop_head where DyeingCode  = '" + txt_DyeingCode.Text + "' ;";
            DataTable dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (dt_temp.Rows[0][0].ToString() != "0")
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("此工艺正在运行，禁止删除",
                                    "温馨提示", MessageBoxButtons.OK, false))
                    {
                        return;
                    }
                }
                else
                {
                    if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("This process is in operation and deletion is prohibited",
                                    "Tips", MessageBoxButtons.OK, false))
                    {
                        return;
                    }
                }
            }

            s_sql = "select * from wait_list where Type != 2;";
            dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            foreach (DataRow dr in dt_temp.Rows)
            {
                string s_code = dr["FormulaCode"].ToString();
                int i_version = Convert.ToInt16(dr["VersionNum"]);
                s_sql = "select * from formula_head where FormulaCode = '" + s_code + "' and VersionNum = " + i_version + ";";
                DataTable dt_formula_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                string s_dyeingCode = dt_formula_head.Rows[0]["DyeingCode"].ToString();
                if (s_dyeingCode == txt_DyeingCode.Text)
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    {
                        if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("此工艺正在运行，禁止删除",
                                        "温馨提示", MessageBoxButtons.OK, false))
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("This process is in operation and deletion is prohibited",
                                        "Tips", MessageBoxButtons.OK, false))
                        {
                            return;
                        }
                    }
                }

            }

            s_sql = "DELETE FROM dyeing_code" +
                               " WHERE DyeingCode = '" + txt_DyeingCode.Text + "';";
            if (FADM_Object.Communal._b_isJustShowInfo)
            {
                FADM_Object.Communal._fadmSqlserver.ReviseData_show(s_sql);
            }
            else
            {
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
            }

            s_sql = "update formula_head set DyeingCode = null,Stage='滴液'  where DyeingCode = '" + txt_DyeingCode.Text + "';";

            if (FADM_Object.Communal._b_isJustShowInfo)
            {
                FADM_Object.Communal._fadmSqlserver.ReviseData_show(s_sql);
            }
            else
            {
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
            }
            FADM_Object.Communal._b_isUpdateNotDripList = true;
            try
            {
                txt_DyeingCode.Text = dgv_DyeingCode.Rows[dgv_DyeingCode.CurrentCell.RowIndex - 1].Cells[0].Value.ToString();
                DyeingCodeShow(dgv_DyeingCode.Rows[dgv_DyeingCode.CurrentCell.RowIndex - 1].Cells[0].Value.ToString());

                DyeDetailsShow(txt_DyeingCode.Text);
                dgv_Combination.ClearSelection();

                dgv_Dye_Set.ClearSelection();
                dgv_Handle_Set.ClearSelection();

                ChildDyeDataShow(txt_DyeingCode.Text, 1);
            }
            catch
            {
                DyeingCodeShow("");
            }
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            try
            {
                if (txt_DyeingCode.Text == "")
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    {
                        if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("染固色代码为空，不能保存",
                                        "温馨提示", MessageBoxButtons.OK, false))
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("The dyeing and fixing color code is empty and cannot be saved",
                                        "Tips", MessageBoxButtons.OK, false))
                        {
                            return;
                        }
                    }
                }
                if (txt_Remark.Text == "")
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    {
                        if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("备注为空，不能保存",
                                        "温馨提示", MessageBoxButtons.OK, false))
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("Remarks is empty and cannot be saved",
                                        "Tips", MessageBoxButtons.OK, false))
                        {
                            return;
                        }
                    }
                }
                string s_sql = "select COUNT(*) from formula_head where DyeingCode  = '" + txt_DyeingCode.Text + "' and State = '已滴定配方'";
                DataTable dt_formula_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                //if (dt_temp.Rows[0][0].ToString() != "0")
                //{
                //    if (DialogResult.OK ==  FADM_Form.CustomMessageBox.Show("此工艺已有完成滴液记录，不能修改",
                //                        "温馨提示", MessageBoxButtons.OK, false))
                //    {
                //        return;
                //    }
                //}

                if (dgv_Combination.Rows.Count == 1)
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    {
                        if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("不存在处理工艺，请添加后再保存",
                                        "温馨提示", MessageBoxButtons.OK, false))
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("There is no processing technology, please add it before saving",
                                        "Tips", MessageBoxButtons.OK, false))
                        {
                            return;
                        }
                    }
                }
                //判断是否空行
                for (int i = 0; i < dgv_Combination.Rows.Count - 1; i++)
                {
                    string s_temp = dgv_Combination.Rows[i].Cells[0].Value.ToString();
                    if (s_temp == "")
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        {
                            if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("存在空行，请删除后再保存",
                                            "温馨提示", MessageBoxButtons.OK, false))
                            {
                                return;
                            }
                        }
                        else
                        {
                            if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("There are empty lines, please delete them before saving",
                                            "Tips", MessageBoxButtons.OK, false))
                            {
                                return;
                            }
                        }
                    }
                }
                //查询所有染色工艺代码
                s_sql = "SELECT Code  FROM dyeing_process WHERE Type = 1 group by Code;";
                DataTable dt_dyeingprocess = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                int n_Count = 0;
                //先判断染色工艺是否有多个，现在只允许一个
                //for (int i = 0; i < dgv_Combination.Rows.Count - 1; i++)
                //{
                //    string s_temp = dgv_Combination.Rows[i].Cells[0].Value.ToString();
                //    for (int k = 0; k < dt_dyeingprocess.Rows.Count; k++)
                //    {
                //        if (s_temp == dt_dyeingprocess.Rows[k][0].ToString())
                //        {
                //            n_Count++;
                //            break;
                //        }
                //    }
                //}
                ////判断第一个工艺是否染色工艺
                //if (dgv_Combination.Rows.Count > 1)
                //{
                //    string s_temp = dgv_Combination.Rows[0].Cells[0].Value.ToString();
                //    s_sql = "SELECT Code  FROM dyeing_process WHERE i_type = 1 and Code = '" + s_temp + "';";
                //    dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                //    if (dt_temp.Rows.Count == 0)
                //    {
                //        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                //        {
                //            if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("第一个工艺不是染色工艺，输入错误，请重新输入",
                //                        "温馨提示", MessageBoxButtons.OK, false))
                //            {
                //                return;
                //            }
                //        }
                //        else
                //        {
                //            if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("The first process is not a dyeing process, input error, please re-enter",
                //                        "Tips", MessageBoxButtons.OK, false))
                //            {
                //                return;
                //            }
                //        }
                //    }
                //}
                //if (n_Count > 1)
                //{
                //    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                //    {
                //        if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("染色工艺多于1个，请删除后再保存",
                //                        "温馨提示", MessageBoxButtons.OK, false))
                //        {
                //            return;
                //        }
                //    }
                //    else
                //    {
                //        if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("There are more than one dyeing process, please delete it before saving ",
                //                        "Tips", MessageBoxButtons.OK, false))
                //        {
                //            return;
                //        }
                //    }
                //}
                //判断第一个工艺是否染色工艺
                //if (dgv_Combination.Rows.Count > 7)
                //{
                //    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                //    {
                //        if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("最多只能输入5个后处理工艺，请重新输入",
                //                    "温馨提示", MessageBoxButtons.OK, false))
                //        {
                //            return;
                //        }
                //    }
                //    else
                //    {
                //        if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("Up to 5 post-processing processes can be entered, please re-enter",
                //                    "Tips", MessageBoxButtons.OK, false))
                //        {
                //            return;
                //        }
                //    }
                //}
                //判断工艺加药量之和是否100%
                for (int i = 0; i < dgv_Combination.Rows.Count - 1; i++)
                {
                    string s_temp = dgv_Combination.Rows[i].Cells[0].Value.ToString();
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    {
                        s_sql = "SELECT SUM(ProportionOrTime)  FROM dyeing_process WHERE" +
                                   " Code = '" + s_temp + "' and TechnologyName in('加A','加B','加C','加D','加E','加F','加G','加H','加I','加J','加K','加L','加M','加N') Group By TechnologyName ; ";
                    }
                    else
                    {
                        s_sql = "SELECT SUM(ProportionOrTime)  FROM dyeing_process WHERE" +
                                   " Code = '" + s_temp + "' and TechnologyName in('Add A','Add B','Add C','Add D','Add E','Add F','Add G','Add H','Add I','Add J','Add K','Add L','Add M','Add N') Group By TechnologyName ; ";
                    }
                    dt_formula_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                    foreach (DataRow dr in dt_formula_head.Rows)
                    {
                        if (Convert.ToInt32(dr[0].ToString()) != 100)
                        {
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            {
                                if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("子工艺添加量之和不等于100，请重新输入后再保存",
                                         "温馨提示", MessageBoxButtons.OK, false))
                                {
                                    return;
                                }
                            }
                            else
                            {
                                if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("The sum of sub process addition quantities does not equal 100. Please re-enter and save again",
                                         "Tips", MessageBoxButtons.OK, false))
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
                //判断工艺加药量之和是否100%
                for (int i = 0; i < dgv_Combination.Rows.Count - 1; i++)
                {
                    string s_temp = dgv_Combination.Rows[i].Cells[0].Value.ToString();
                    s_sql = "SELECT *  FROM dyeing_process WHERE" +
                                   " Code = '" + s_temp + "' and StepNum is not null; ";
                    dt_formula_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                    if (dt_formula_head.Rows.Count == 0)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        {
                            if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("流程工艺不存在子工艺工艺，请重新输入后再保存",
                                     "温馨提示", MessageBoxButtons.OK, false))
                            {
                                return;
                            }
                        }
                        else
                        {
                            if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("There are no sub process processes in the process technology. Please re-enter and save again",
                                     "Tips", MessageBoxButtons.OK, false))
                            {
                                return;
                            }
                        }
                    }
                }

                s_sql = "select COUNT(*) from formula_head where DyeingCode  = '" + txt_DyeingCode.Text + "' and State = '已滴定配方'";
                dt_formula_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                if (dt_formula_head.Rows[0][0].ToString() != "0")
                {
                    //if (DialogResult.OK ==  FADM_Form.CustomMessageBox.Show("此工艺已有完成滴液记录，不能修改",
                    //                    "温馨提示", MessageBoxButtons.OK, false))
                    //{
                    //    return;
                    //}

                    ReName rename = new ReName(1, txt_DyeingCode.Text);
                    rename.ShowDialog();
                    if (rename.DialogResult != DialogResult.OK)
                    {
                        return;
                    }




                    txt_DyeingCode.Text = FADM_Object.Communal._s_reName;
                }

                //删除原有固染色流程记录
                s_sql = "DELETE FROM dyeing_code" +
                                   " WHERE DyeingCode = '" + txt_DyeingCode.Text + "';";
                if (FADM_Object.Communal._b_isJustShowInfo)
                {
                    FADM_Object.Communal._fadmSqlserver.ReviseData_show(s_sql);
                }
                else
                {
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                }
                int i_nDye = 0;
                int i_nHandle = 0;

                for (int i = 0; i < dgv_Combination.Rows.Count - 1; i++)
                {
                    //判断当前是染色还是后处理
                    s_sql = "SELECT Code  FROM dyeing_process WHERE Type = 1 and Code ='"+ dgv_Combination.Rows[i].Cells[0].Value.ToString()+"'  group by Code;";
                    DataTable dt_dyeingpro = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                    string s_type = "";
                    string s_step = "";
                    if(dt_dyeingpro.Rows.Count !=0)
                    {
                        i_nDye++;
                        s_type = "1";
                        s_step = i_nDye.ToString();
                    }
                    else
                    {
                        i_nHandle++;
                        s_type = "2";
                        s_step = i_nHandle.ToString();
                    }
                    
                    s_sql = "INSERT INTO dyeing_code (DyeingCode, Type," +
                                               " Step, Code,IndexNum,IsUse,Remark) VALUES('" + txt_DyeingCode.Text + "',"+ s_type+"," + s_step + ",'" 
                                               + dgv_Combination.Rows[i].Cells[0].Value.ToString() +"',"+(i+1).ToString() +",'"+ (checkBox1.Checked?"1":"0") + "','" + txt_Remark.Text + "');";
                    if (FADM_Object.Communal._b_isJustShowInfo)
                    {
                        FADM_Object.Communal._fadmSqlserver.ReviseData_show(s_sql);
                    }
                    else
                    {
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                    }
                }


                ////一个染色工艺
                //if (n_Count == 1)
                //{
                //    s_sql = "INSERT INTO dyeing_code (s_dyeingCode, i_type," +
                //                                   " Step, Code) VALUES('" + txt_DyeingCode.Text + "',1,1,'" + dgv_Combination.Rows[0].Cells[0].Value.ToString() + "');";
                //    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                //    for (int i = 1; i < dgv_Combination.Rows.Count - 1; i++)
                //    {
                //        s_sql = "INSERT INTO dyeing_code (s_dyeingCode, i_type," +
                //                                   " Step, Code) VALUES('" + txt_DyeingCode.Text + "',2," + i.ToString() + ",'" + dgv_Combination.Rows[i].Cells[0].Value.ToString() + "');";
                //        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                //    }
                //}
                ////全部是后处理工艺
                //else
                //{
                //    if (n_Count == 0)
                //    {
                //        for (int i = 0; i < dgv_Combination.Rows.Count-1; i++)
                //        {
                //            s_sql = "INSERT INTO dyeing_code (s_dyeingCode, i_type," +
                //                                       " Step, Code) VALUES('" + txt_DyeingCode.Text + "',2," + (i+1).ToString() + ",'" + dgv_Combination.Rows[i].Cells[0].Value.ToString() + "');";
                //            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                //        }
                //    }
                //    else
                //    {
                //        for (int i = 1; i < dgv_Combination.Rows.Count-1; i++)
                //        {
                //            s_sql = "INSERT INTO dyeing_code (s_dyeingCode, i_type," +
                //                                       " Step, Code) VALUES('" + txt_DyeingCode.Text + "',2," + i.ToString() + ",'" + dgv_Combination.Rows[i].Cells[0].Value.ToString() + "');";
                //            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                //        }
                //    }
                //}
                DyeingCodeShow(txt_DyeingCode.Text);
                dgv_Combination.ClearSelection();

                dgv_Dye_Set.ClearSelection();
                dgv_Handle_Set.ClearSelection();

                ChildDyeDataShow(txt_DyeingCode.Text, 1);
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    new SmartDyeing.FADM_Object.MyAlarm("保存完成", 0);
                else
                    new SmartDyeing.FADM_Object.MyAlarm("Save completed", 0);
            }
            catch { }
        }

        private void dgv_DyeingCode_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (dgv_DyeingCode.CurrentCell != null && dgv_DyeingCode.SelectedRows.Count !=0)
                {
                    txt_DyeingCode.Text = dgv_DyeingCode.CurrentCell.Value.ToString();
                    DyeDetailsShow(txt_DyeingCode.Text);
                    dgv_Combination.ClearSelection();

                    dgv_Dye_Set.ClearSelection();
                    dgv_Handle_Set.ClearSelection();

                    ChildDyeDataShow(txt_DyeingCode.Text, 1);
                }
            }
            catch
            {
                //txt_DyeingCode.Text = null;
            }
        }

        private void dgv_Dye_Set_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (dgv_Dye_Set.CurrentCell != null && dgv_Dye_Set.SelectedRows.Count != 0)
                {
                    //txt_DyeingCode.Text = dgv_Dye_Set.CurrentCell.Value.ToString();

                    dgv_DyeingCode.ClearSelection();
                    //dgv_Dye_Set.ClearSelection();
                    dgv_Handle_Set.ClearSelection();

                    ChildDyeDataShow(dgv_Dye_Set.CurrentCell.Value.ToString(), 2);
                }
            }
            catch
            {
            }
        }

        private void dgv_Handle_Set_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (dgv_Handle_Set.CurrentCell != null && dgv_Handle_Set.SelectedRows.Count != 0)
                {
                    //txt_DyeingCode.Text = dgv_Handle_Set.CurrentCell.Value.ToString();

                    dgv_DyeingCode.ClearSelection();
                    dgv_Dye_Set.ClearSelection();
                    //dgv_Handle_Set.ClearSelection();

                    ChildDyeDataShow(dgv_Handle_Set.CurrentCell.Value.ToString(), 2);
                }
            }
            catch
            {
            }
        }

        private void txt_DyeingCode_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    if (txt_DyeingCode.Enabled && txt_DyeingCode.Text != null && txt_DyeingCode.Text != "")
                    {
                        //检索是否存在这个调液流程代码
                        for (int i = 0; i < dgv_DyeingCode.Rows.Count; i++)
                        {
                            if (txt_DyeingCode.Text == dgv_DyeingCode[0, i].Value.ToString())
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("代码重复,请重新输入",
                                    "温馨提示", MessageBoxButtons.OK, false))
                                    {
                                        txt_DyeingCode.Text = null;
                                        txt_DyeingCode.Focus();
                                        return;
                                    }
                                }
                                else
                                {
                                    if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("Code duplication, please re-enter",
                                    "Tips", MessageBoxButtons.OK, false))
                                    {
                                        txt_DyeingCode.Text = null;
                                        txt_DyeingCode.Focus();
                                        return;
                                    }
                                }
                            }
                        }

                        txt_Remark.Focus();
                    }
                    break;
                default:
                    break;
            }
        }

        private void dgv_Combination_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyData)
            {
                case Keys.Delete:
                    {
                        try
                        {
                            dgv_Combination.Rows.Remove(dgv_Combination.CurrentRow);
                        }
                        catch
                        {
                        }
                    }
                    break;
                case Keys.Insert:
                    try
                    {
                        dgv_Combination.Rows.Insert(dgv_Combination.CurrentRow.Index);
                        //新增行下拉选项
                        {
                            DataGridViewComboBoxCell dd = (DataGridViewComboBoxCell)dgv_Combination[0, dgv_Combination.CurrentRow.Index - 1];
                            dd.DataSource = _lis_code;
                        }
                    }
                    catch
                    {

                    }
                    break;
                default:

                    break;
            }
        }

        private void txt_DyeingCode_Leave(object sender, EventArgs e)
        {
            txt_DyeingCode.Enabled = false;
        }

        private void dgv_Combination_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            //新增行下拉选项
            {
                DataGridViewComboBoxCell dd = (DataGridViewComboBoxCell)dgv_Combination[0, dgv_Combination.Rows.Count - 1];
                dd.DataSource = _lis_code;
            }
        }

        private void checkBox1_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked) 
            {
                if (FADM_Object.Communal._b_isJustShowInfo)
                {
                    FADM_Object.Communal._fadmSqlserver.ReviseData_show("Update dyeing_code Set IsUse=1 where DyeingCode = '" + txt_DyeingCode.Text + "';");
                }
                else
                {
                    FADM_Object.Communal._fadmSqlserver.ReviseData("Update dyeing_code Set IsUse=1 where DyeingCode = '" + txt_DyeingCode.Text + "';");
                }
            }
            else
            {
                if (FADM_Object.Communal._b_isJustShowInfo)
                {
                    FADM_Object.Communal._fadmSqlserver.ReviseData_show("Update dyeing_code Set IsUse=0 where DyeingCode = '" + txt_DyeingCode.Text + "';");
                }
                else
                {
                    FADM_Object.Communal._fadmSqlserver.ReviseData("Update dyeing_code Set IsUse=0 where DyeingCode = '" + txt_DyeingCode.Text + "';");
                }
            }
        }

        private void txt_Remark_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    if (txt_Remark.Enabled && txt_Remark.Text != null && txt_Remark.Text != "")
                    {
                        
                        //将新的调液流程代码写入数据库
                        string s_sql = "INSERT INTO dyeing_code" +
                                           " (DyeingCode,Remark) VALUES( '" + txt_DyeingCode.Text + "','" + txt_Remark.Text + " ');";
                        if (FADM_Object.Communal._b_isJustShowInfo)
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData_show(s_sql);
                        }
                        else
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        }

                        //更新调液流程代码表
                        DyeingCodeShow(txt_DyeingCode.Text);

                        DyeDetailsShow(txt_DyeingCode.Text);
                        dgv_Combination.ClearSelection();

                        dgv_Dye_Set.ClearSelection();
                        dgv_Handle_Set.ClearSelection();

                        ChildDyeDataShow(txt_DyeingCode.Text, 1);


                        txt_DyeingCode.Enabled = false;
                        ////设置添加按钮焦点
                        //btn_DyeingProcessAdd.Focus();
                    }
                    break;
                default:
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string s_sql = "Update dyeing_code Set Remark = '" + txt_Remark.Text + "' where DyeingCode = '" + txt_DyeingCode.Text + "'";
            if (FADM_Object.Communal._b_isJustShowInfo)
            {
                FADM_Object.Communal._fadmSqlserver.ReviseData_show(s_sql);
            }
            else
            {
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
            }

            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                new SmartDyeing.FADM_Object.MyAlarm("保存完成", 0);
            else
                new SmartDyeing.FADM_Object.MyAlarm("Save completed", 0);
        }
    }
}
