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
    public partial class DyeingConfiguration : UserControl
    {
        List<string> _lis_code = new List<string>();

        //处理工艺字典
        Dictionary<string, string> _dic_dyeCode = new Dictionary<string, string>();
        public DyeingConfiguration()
        {
            InitializeComponent();
            UpdateListAndDyeCode();
            DyeCodeShow("");

            if (!FADM_Object.Communal._b_isCloseAddMed)
                checkBox1.Visible = false;
        }

        /// <summary>
        /// 显示染色流程代码
        /// </summary>
        /// <param name="_DyeingCode">选中的详细代码</param>
        /// <returns>0:正常;-1:异常</returns>
        private int DyeCodeShow(string s_dyeCode)
        {
            string s_sql = "SELECT Code  FROM dyeing_process where Type = 1 group by Code  Order By Code;";
            DataTable dt_dyeingcode = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

            //捆绑
            dgv_Dye_Code.DataSource = new DataView(dt_dyeingcode);
            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                //设置标题文字
                dgv_Dye_Code.Columns[0].HeaderCell.Value = "染色工艺代码";
            else
                dgv_Dye_Code.Columns[0].HeaderCell.Value = "DyeingProcessCode";

            //设置标题宽度
            dgv_Dye_Code.Columns[0].Width = dgv_Dye_Code.Width - 3;



            //关闭点击标题自动排序功能
            dgv_Dye_Code.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;

            //设置标题居中显示
            dgv_Dye_Code.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            //设置标题字体
            dgv_Dye_Code.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 14.25F);

            //设置内容居中显示
            dgv_Dye_Code.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            //设置内容字体
            dgv_Dye_Code.RowsDefaultCellStyle.Font = new Font("宋体", 14.25F);

            //设置行高
            dgv_Dye_Code.RowTemplate.Height = 30;

            try
            {
                //设置当前选中行
                for (int i = 0; i < dgv_Dye_Code.Rows.Count; i++)
                {
                    string s = dgv_Dye_Code.Rows[i].Cells[0].Value.ToString();
                    if (s == s_dyeCode)
                    {
                        dgv_Dye_Code.CurrentCell = dgv_Dye_Code.Rows[i].Cells[0];
                        break;
                    }
                }
                if (s_dyeCode == "")
                {
                    txt_Notes.Text = "";
                }
                else
                {
                    if (_dic_dyeCode.ContainsKey(txt_Dye_Code.Text))
                        txt_Notes.Text = _dic_dyeCode[txt_Dye_Code.Text];
                    else
                    {
                        txt_Notes.Text = "";
                    }
                }

                return 0;
            }
            catch
            {
                return -1;
            }
        }

        private void btn_BrewingCodeAdd_Click(object sender, EventArgs e)
        {
            txt_Dye_Code.Text = null;
            txt_Dye_Code.Enabled = true;
            txt_Dye_Code.Focus();

            //btn_DyeingProcessAdd.Enabled = false;
            //btn_DyeingProcessDelete.Enabled = false;
            //btn_DyeingProcessUpdate.Enabled = false;
            //btn_DyeingCodeDelete.Enabled = false;
        }

        private void btn_DyeingCodeDelete_Click(object sender, EventArgs e)
        {
            if (dgv_Dye_Code.SelectedRows.Count == 0)
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    FADM_Form.CustomMessageBox.Show("请先选择操作行", "操作异常", MessageBoxButtons.OK, false);
                else
                    FADM_Form.CustomMessageBox.Show("Please select the operation line first", "Abnormal operation", MessageBoxButtons.OK, false);
                return;
            }
            string s_sql = "select COUNT(*) from formula_head where DyeingCode in( select DyeingCode from dyeing_code where Code = '" + txt_Dye_Code.Text + "') and State = '已滴定配方'";
            DataTable dt_formula_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (dt_formula_head.Rows.Count > 0)
            {
                if (dt_formula_head.Rows[0][0].ToString() != "0")
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    {
                        if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("此工艺已有完成滴液记录，不能修改",
                                        "温馨提示", MessageBoxButtons.OK, false))
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("This process has completed the droplet record and cannot be modified",
                                        "Tips", MessageBoxButtons.OK, false))
                        {
                            return;
                        }
                    }
                }
            }

            s_sql = "DELETE FROM dyeing_process" +
                               " WHERE Code = '" + txt_Dye_Code.Text + "';";
            if (FADM_Object.Communal._b_isJustShowInfo)
            {
                FADM_Object.Communal._fadmSqlserver.ReviseData_show(s_sql);
            }
            else
            {
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
            }

            //重新排序
            s_sql = "Select DyeingCode FROM dyeing_code" +
                               " WHERE Code = '" + txt_Dye_Code.Text + "' group by DyeingCode;";
            dt_formula_head = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

            foreach (DataRow dr in dt_formula_head.Rows)
            {
                //获取要删除是第几个步骤，删除后，后边Step就要减1
                s_sql = "Select Step,Type FROM dyeing_code" +
                               " WHERE Code = '" + txt_Dye_Code.Text + "' and DyeingCode = '" + dr[0].ToString() + "';";
                DataTable P_dt1 = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                //如果删除的是染色工艺，会把整个大工艺删除
                if (P_dt1.Rows[0][1].ToString() == "1")
                {
                    s_sql = "DELETE FROM dyeing_code" +
                                   " WHERE  DyeingCode = '" + dr[0].ToString() + "';";
                    if (FADM_Object.Communal._b_isJustShowInfo)
                    {
                        FADM_Object.Communal._fadmSqlserver.ReviseData_show(s_sql);
                    }
                    else
                    {
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                    }
                }
                else
                {

                    //先删除
                    s_sql = "DELETE FROM dyeing_code" +
                                   " WHERE Code = '" + txt_Dye_Code.Text + "' and DyeingCode = '" + dr[0].ToString() + "';";
                    if (FADM_Object.Communal._b_isJustShowInfo)
                    {
                        FADM_Object.Communal._fadmSqlserver.ReviseData_show(s_sql);
                    }
                    else
                    {
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                    }

                    //修改比删除Step大的序号
                    s_sql = "Update dyeing_code Set Step = Step - 1" +
                                   " WHERE  DyeingCode = '" + dr[0].ToString() + "' and Step >" + P_dt1.Rows[0][0].ToString() + " ;";
                    if (FADM_Object.Communal._b_isJustShowInfo)
                    {
                        FADM_Object.Communal._fadmSqlserver.ReviseData_show(s_sql);
                    }
                    else
                    {
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                    }
                }
            }

            UpdateListAndDyeCode();
            try
            {
                DyeCodeShow(dgv_Dye_Code.Rows[dgv_Dye_Code.CurrentCell.RowIndex - 1].Cells[0].Value.ToString());
            }
            catch
            {
                DyeCodeShow("");
            }
        }

        private void btn_DyeingProcessAdd_Click(object sender, EventArgs e)
        {
            string s_sql = "select COUNT(*) from formula_head where DyeingCode in( select DyeingCode from dyeing_code where Code = '" + txt_Dye_Code.Text + "') and State = '已滴定配方'";
            DataTable dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (dt_temp.Rows.Count > 0)
            {
                if (dt_temp.Rows[0][0].ToString() != "0")
                {
                    //if (DialogResult.OK ==  FADM_Form.CustomMessageBox.Show("此工艺已有完成滴液记录，不能修改",
                    //                    "温馨提示", MessageBoxButtons.OK, false))
                    //{
                    //    return;
                    //}

                    ReName rename = new ReName(2, txt_Dye_Code.Text);
                    rename.ShowDialog();
                    if (rename.DialogResult != DialogResult.OK)
                    {
                        return;
                    }

                    ////写入数据库
                    //string P_str_sql1 = "INSERT INTO dyeing_Remark" +
                    //                       " (Code,Remark) VALUES( '" + FADM_Object.Communal._s_reName + "' ,'" + txt_Notes.Text + "');";
                    //FADM_Object.Communal._fadmSqlserver.ReviseData(P_str_sql1);

                    UpdateListAndDyeCode();
                    DyeCodeShow(FADM_Object.Communal._s_reName);
                }
            }

            string s_stepNum = (dgv_Child_DyeData.Rows.Count + 1).ToString();
            DyeingStep form_DyeingStep = new DyeingStep(1, s_stepNum, "", "", "", "", txt_Dye_Code.Text, "", txt_Notes.Text, true, null, this, null);
            form_DyeingStep.Show();
        }

        /// <summary>
        /// 显示染色步骤
        /// </summary>
        /// <param name="_StepNum">当前选中的步号</param>
        /// <returns>0:正常;-1:异常</returns>
        public int ChildDyeDataShow(string _StepNum)
        {
            try
            {
                //获取当前调液代码的调液流程
                //获取当前调液代码的调液流程
                string s_sql = "SELECT Type  FROM dyeing_process WHERE" +
                                   " Code = '" + txt_Dye_Code.Text + "' And OpenMedicine = 1 Order By StepNum ; ";
                DataTable dt_dyeingprocess = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                if (dt_dyeingprocess.Rows.Count > 0)
                {
                    checkBox1.Checked = true;
                }
                else
                {
                    checkBox1.Checked = false;
                }


                //获取当前调液代码的调液流程
                s_sql = "SELECT StepNum,TechnologyName,Temp,Rate,ProportionOrTime,Rev  FROM dyeing_process WHERE" +
                                   " Code = '" + txt_Dye_Code.Text + "' Order By StepNum ; ";
                dt_dyeingprocess = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                //捆绑
                dgv_Child_DyeData.DataSource = new DataView(dt_dyeingprocess);

                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    //设置标题栏名称
                    string[] sa_lineName = { "步号", "操作类型", "温度", "速率", "百分比(%)/时间(m)", "转速" };
                    for (int i = 0; i < 6; i++)
                    {
                        dgv_Child_DyeData.Columns[i].HeaderCell.Value = sa_lineName[i];
                        //设置标题宽度
                        dgv_Child_DyeData.Columns[i].Width = (dgv_Child_DyeData.Width - 2) / 6;
                        //关闭点击标题自动排序功能
                        dgv_Child_DyeData.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                    }
                    //设置标题字体
                    dgv_Child_DyeData.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 14.25F);
                    //设置内容字体
                    dgv_Child_DyeData.RowsDefaultCellStyle.Font = new Font("宋体", 14.25F);
                }
                else
                {
                    //设置标题栏名称
                    string[] sa_lineName = { "StepNumber", "OperationFlow", "SettingTemperature", "TemperatureRate", "Percentage(%)/time(m)", "Speed" };
                    for (int i = 0; i < 6; i++)
                    {
                        dgv_Child_DyeData.Columns[i].HeaderCell.Value = sa_lineName[i];
                        //设置标题宽度
                        dgv_Child_DyeData.Columns[i].Width = (dgv_Child_DyeData.Width - 2) / 6;
                        //关闭点击标题自动排序功能
                        dgv_Child_DyeData.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                    }
                    //设置标题字体
                    dgv_Child_DyeData.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 10.5F);
                    //设置内容字体
                    dgv_Child_DyeData.RowsDefaultCellStyle.Font = new Font("宋体", 10.5F);
                }
                //设置标题居中显示
                dgv_Child_DyeData.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

               

                //设置内容居中显示
                dgv_Child_DyeData.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

              

                //设置行高
                dgv_Child_DyeData.RowTemplate.Height = 30;

                ////设置当前选中行
                //for (int i = 0; i < dgv_Child_DyeData.Rows.Count; i++)
                //{
                //    string s = dgv_Child_DyeData.Rows[i].Cells[0].Value.ToString();
                //    if (s == _StepNum)
                //    {
                //        dgv_Child_DyeData.CurrentCell = dgv_Child_DyeData.Rows[i].Cells[0];
                //        break;
                //    }
                //}

                dgv_Child_DyeData.ClearSelection();

                return 0;
            }
            catch
            {
                return -1;
            }
        }

        private void btn_DyeingProcessUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgv_Child_DyeData.SelectedRows.Count == 0)
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show("请先选择操作行", "操作异常", MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("Please select the operation line first", "Abnormal operation", MessageBoxButtons.OK, false);
                    return;
                }

                string s_sql = "select COUNT(*) from formula_head where DyeingCode in( select DyeingCode from dyeing_code where Code = '" + txt_Dye_Code.Text + "') and State = '已滴定配方'";
                DataTable dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                if (dt_temp.Rows[0][0].ToString() != "0")
                {
                    //if (DialogResult.OK ==  FADM_Form.CustomMessageBox.Show("此工艺已有完成滴液记录，不能修改",
                    //                    "温馨提示", MessageBoxButtons.OK, false))
                    //{
                    //    return;
                    //}

                    ReName rename = new ReName(2, txt_Dye_Code.Text);
                    rename.ShowDialog();
                    if (rename.DialogResult != DialogResult.OK)
                    {
                        return;
                    }
                    ////写入数据库
                    //string P_str_sql1 = "INSERT INTO dyeing_Remark" +
                    //                       " (Code,Remark) VALUES( '" + FADM_Object.Communal._s_reName + "' ,'" + txt_Notes.Text + "');";
                    //FADM_Object.Communal._fadmSqlserver.ReviseData(P_str_sql1);

                    UpdateListAndDyeCode();

                    int i_index = dgv_Child_DyeData.CurrentRow.Index;

                    DyeCodeShow(FADM_Object.Communal._s_reName);
                    dgv_Child_DyeData.CurrentCell = dgv_Child_DyeData[0, i_index];
                }

                string s_stepNum = dgv_Child_DyeData.CurrentRow.Cells[0].Value.ToString();
                string s_technologyName = dgv_Child_DyeData.CurrentRow.Cells[1].Value.ToString().Trim();
                string s_proportionOrTime = "";
                string s_temp = "";
                string s_rate = "";
                string s_rev = "";
                if (dgv_Child_DyeData.CurrentRow.Cells[2].Value != null)
                {
                    s_temp = dgv_Child_DyeData.CurrentRow.Cells[2].Value.ToString();
                }
                if (dgv_Child_DyeData.CurrentRow.Cells[3].Value != null)
                {
                    s_rate = dgv_Child_DyeData.CurrentRow.Cells[3].Value.ToString();
                }
                if (dgv_Child_DyeData.CurrentRow.Cells[4].Value != null)
                {
                    s_proportionOrTime = dgv_Child_DyeData.CurrentRow.Cells[4].Value.ToString();
                }
                if (dgv_Child_DyeData.CurrentRow.Cells[5].Value != null)
                {
                    s_rev = dgv_Child_DyeData.CurrentRow.Cells[5].Value.ToString();
                }
                DyeingStep form_DyeingStep = new DyeingStep(1, s_stepNum, s_technologyName, s_proportionOrTime, s_temp, s_rate, txt_Dye_Code.Text, s_rev, txt_Notes.Text, false, null, this, null);
                form_DyeingStep.Show();
            }
            catch
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    FADM_Form.CustomMessageBox.Show("未发现可修改的行,请先添加！", "操作异常", MessageBoxButtons.OK, false);
                else
                    FADM_Form.CustomMessageBox.Show("No modifiable rows found, please add first！", "Abnormal operation", MessageBoxButtons.OK, false);

            }
        }

        private void btn_DyeingProcessDelete_Click(object sender, EventArgs e)
        {
            if (dgv_Child_DyeData.SelectedRows.Count == 0)
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    FADM_Form.CustomMessageBox.Show("请先选择操作行", "操作异常", MessageBoxButtons.OK, false);
                else
                    FADM_Form.CustomMessageBox.Show("Please select the operation line first", "Abnormal operation", MessageBoxButtons.OK, false);
                return;
            }
            string s_sql = "select COUNT(*) from formula_head where DyeingCode in( select DyeingCode from dyeing_code where Code = '" + txt_Dye_Code.Text + "') and State = '已滴定配方'";
            DataTable dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (dt_temp.Rows[0][0].ToString() != "0")
            {
                //if (DialogResult.OK ==  FADM_Form.CustomMessageBox.Show("此工艺已有完成滴液记录，不能修改",
                //                    "温馨提示", MessageBoxButtons.OK, false))
                //{
                //    return;
                //}

                ReName rename = new ReName(2, txt_Dye_Code.Text);
                rename.ShowDialog();
                if (rename.DialogResult != DialogResult.OK)
                {
                    return;
                }
                ////写入数据库
                //string P_str_sql1 = "INSERT INTO dyeing_Remark" +
                //                       " (Code,Remark) VALUES( '" + FADM_Object.Communal._s_reName + "' ,'" + txt_Notes.Text + "');";
                //FADM_Object.Communal._fadmSqlserver.ReviseData(P_str_sql1);

                UpdateListAndDyeCode();

                int i_index = dgv_Child_DyeData.CurrentRow.Index;

                DyeCodeShow(FADM_Object.Communal._s_reName);
                dgv_Child_DyeData.CurrentCell = dgv_Child_DyeData[0, i_index];
            }

            s_sql = "select DyeingCode from dyeing_code where Code = '" + txt_Dye_Code.Text + "'";
            dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            //如果存在染固色代码使用，最后一个记录就不能删除
            if (dt_temp.Rows.Count > 0)
            {
                if (dgv_Child_DyeData.Rows.Count == 1)
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show("不能删除最后一条记录，如要删除，请把工艺删除", "操作异常", MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("The last record cannot be deleted. If you want to delete it, please delete the process", "Abnormal operation", MessageBoxButtons.OK, false);
                    return;
                }
            }

            s_sql = "DELETE FROM dyeing_process WHERE" +
                              " StepNum = '" + dgv_Child_DyeData.CurrentRow.Cells[0].Value.ToString() + "'" +
                              " AND Code = '" + txt_Dye_Code.Text + "' ;";
            if (FADM_Object.Communal._b_isJustShowInfo)
            {
                FADM_Object.Communal._fadmSqlserver.ReviseData_show(s_sql);
            }
            else
            {
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
            }
            try
            {
                ChildDyeDataShow(dgv_Child_DyeData.Rows[dgv_Child_DyeData.CurrentCell.RowIndex - 1].Cells[0].Value.ToString());
            }
            catch
            {
                ChildDyeDataShow("");
            }
        }

        private void txt_Dye_Code_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    if (txt_Dye_Code.Enabled && txt_Dye_Code.Text != null && txt_Dye_Code.Text != "")
                    {
                        //检索是否存在这个代码
                        string s_sql = "SELECT Code  FROM dyeing_process where Code = '" + txt_Dye_Code.Text + "';";
                        DataTable dt_dyeingcode = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);


                        if (dt_dyeingcode.Rows.Count > 0)
                        {
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            {
                                if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("代码重复,请重新输入",
                                "温馨提示", MessageBoxButtons.OK, false))
                                {
                                    txt_Dye_Code.Text = null;
                                    txt_Dye_Code.Focus();
                                    return;
                                }
                            }
                            else
                            {
                                if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("Code duplication, please re-enter",
                                "Tips", MessageBoxButtons.OK, false))
                                {
                                    txt_Dye_Code.Text = null;
                                    txt_Dye_Code.Focus();
                                    return;
                                }
                            }
                        }

                        txt_Dye_Code.Enabled = false;

                        //设置添加按钮焦点
                        txt_Notes.Enabled = true;
                        txt_Notes.Focus();
                    }
                    break;
                default:
                    break;
            }
        }

        private void txt_Notes_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    if (txt_Notes.Enabled && txt_Notes.Text != null && txt_Notes.Text != "")
                    {
                        //检索是否存在这个代码
                        string s_sql = "SELECT Code  FROM dyeing_process where Code = '" + txt_Dye_Code.Text + "';";
                        DataTable dt_dyeingcode = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);


                        if (dt_dyeingcode.Rows.Count > 0)

                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("代码重复,请重新输入",
                                    "温馨提示", MessageBoxButtons.OK, false))
                                    {
                                        txt_Dye_Code.Text = null;
                                        txt_Dye_Code.Focus();
                                        return;
                                    }
                                }
                                else
                                {
                                    if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("Code duplication, please re-enter",
                                    "Tips", MessageBoxButtons.OK, false))
                                    {
                                        txt_Dye_Code.Text = null;
                                        txt_Dye_Code.Focus();
                                        return;
                                    }
                                }
                            }
                        


                        //FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from dyeing_Remark where Code = '" +
                        //                        txt_Dye_Code.Text + "' ;");
                        ////写入数据库
                        //string P_str_sql1 = "INSERT INTO dyeing_Remark" +
                        //                       " (Code,Remark) VALUES( '" + txt_Dye_Code.Text + "' ,'" + txt_Notes.Text + "');";
                        //FADM_Object.Communal._fadmSqlserver.ReviseData(P_str_sql1);


                        s_sql = "INSERT INTO dyeing_process" +
                                           " (Code,Type,Remark) VALUES( '" + txt_Dye_Code.Text + "' ,1,'" + txt_Notes.Text + "');";
                        if (FADM_Object.Communal._b_isJustShowInfo)
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData_show(s_sql);
                        }
                        else
                        {
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                        }


                        ////更新调液流程代码表
                        UpdateListAndDyeCode();
                        DyeCodeShow(txt_Dye_Code.Text);


                        //打开按钮
                        btn_DyeingProcessAdd.Enabled = true;
                        btn_DyeingProcessDelete.Enabled = true;
                        btn_DyeingProcessUpdate.Enabled = true;
                        txt_Dye_Code.Enabled = false;
                        txt_Notes.Enabled = false;

                        //设置添加按钮焦点
                        btn_DyeingProcessAdd.Focus();

                        FADM_Control.Formula.updateloadCraft(); //配方界面更新下

                    }
                    break;
                default:
                    break;
            }
        }

        private void UpdateListAndDyeCode()
        {
            _lis_code.Clear();

            string s_sql = "SELECT Code  FROM dyeing_process where Type = 1 group by Code;";
            DataTable dt_dyeingcode = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            for (int i = 0; i < dt_dyeingcode.Rows.Count; i++)
            {
                _lis_code.Add(dt_dyeingcode.Rows[i][0].ToString());
            }

            //for (int i = 0; i < dgv_DyeDetails.Rows.Count; i++)
            //{
            //    DataGridViewComboBoxCell dd = (DataGridViewComboBoxCell)dgv_DyeDetails[0, i];
            //    dd.DataSource = null;
            //    dd.DataSource = _lis_code;
            //}

            _dic_dyeCode.Clear();
            s_sql = "SELECT Code,Remark  FROM dyeing_process where Type = 1 group by Code,Remark ;";
            dt_dyeingcode = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            for (int i = 0; i < dt_dyeingcode.Rows.Count; i++)
            {
                _dic_dyeCode.Add(dt_dyeingcode.Rows[i]["Code"].ToString(), dt_dyeingcode.Rows[i]["Remark"].ToString());
            }

        }

        private void dgv_Dye_Code_CurrentCellChanged(object sender, EventArgs e)
        {
            try
            {
                if (dgv_Dye_Code.CurrentCell != null)
                {
                    txt_Dye_Code.Text = dgv_Dye_Code.CurrentCell.Value.ToString();
                    if (txt_Dye_Code.Text == "")
                    {
                        txt_Notes.Text = "";
                    }
                    else
                    {
                        if (_dic_dyeCode.ContainsKey(txt_Dye_Code.Text))
                            txt_Notes.Text = _dic_dyeCode[txt_Dye_Code.Text];
                        else
                        {
                            txt_Notes.Text = "";
                        }
                    }

                    ChildDyeDataShow("");
                    btn_DyeingProcessAdd.Enabled = true;
                    btn_DyeingProcessUpdate.Enabled = true;
                    btn_DyeingProcessDelete.Enabled = true;
                    dgv_Child_DyeData.ClearSelection();
                }
            }
            catch
            {
                txt_Dye_Code.Text = null;
                txt_Notes.Text = "";
                ChildDyeDataShow("");
                btn_DyeingProcessAdd.Enabled = false;
                btn_DyeingProcessUpdate.Enabled = false;
                btn_DyeingProcessDelete.Enabled = false;
                dgv_Child_DyeData.ClearSelection();
            }
        }

        private void btn_Copy_Click(object sender, EventArgs e)
        {
            try

            {
                ReName rename = new ReName(2, txt_Dye_Code.Text);
                rename.ShowDialog();
                if (rename.DialogResult != DialogResult.OK)
                {
                    return;
                }

                UpdateListAndDyeCode();

                int i_index = dgv_Child_DyeData.CurrentRow.Index;

                DyeCodeShow(FADM_Object.Communal._s_reName);
                dgv_Child_DyeData.CurrentCell = dgv_Child_DyeData[0, i_index];

            }
            catch { }
        }

        private void btn_Insert_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgv_Child_DyeData.SelectedRows.Count == 0)
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show("请先选择操作行", "操作异常", MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("Please select the operation line first", "Abnormal operation", MessageBoxButtons.OK, false);
                    return;
                }

                string s_sql = "select COUNT(*) from formula_head where DyeingCode in( select DyeingCode from dyeing_code where Code = '" + txt_Dye_Code.Text + "') and State = '已滴定配方'";
                DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                if (dt_data.Rows[0][0].ToString() != "0")
                {
                    //if (DialogResult.OK ==  FADM_Form.CustomMessageBox.Show("此工艺已有完成滴液记录，不能修改",
                    //                    "温馨提示", MessageBoxButtons.OK, false))
                    //{
                    //    return;
                    //}

                    ReName rename = new ReName(2, txt_Dye_Code.Text);
                    rename.ShowDialog();
                    if (rename.DialogResult != DialogResult.OK)
                    {
                        return;
                    }

                    UpdateListAndDyeCode();

                    int i_index = dgv_Child_DyeData.CurrentRow.Index;

                    DyeCodeShow(FADM_Object.Communal._s_reName);
                    dgv_Child_DyeData.CurrentCell = dgv_Child_DyeData[0, i_index];
                }

                string s_stepNum = dgv_Child_DyeData.CurrentRow.Cells[0].Value.ToString();
                //把步号加1
                if (FADM_Object.Communal._b_isJustShowInfo)
                {
                    FADM_Object.Communal._fadmSqlserver.ReviseData_show("Update dyeing_process Set StepNum = StepNum + 1 where StepNum >= " + s_stepNum + " AND Code = '" + txt_Dye_Code.Text + "';");
                }
                else
                {
                    FADM_Object.Communal._fadmSqlserver.ReviseData("Update dyeing_process Set StepNum = StepNum + 1 where StepNum >= " + s_stepNum + " AND Code = '" + txt_Dye_Code.Text + "';");
                }
                string s_technologyName = "";
                string s_proportionOrTime = "1";
                string s_temp = "";
                string s_rate = "";
                string s_rev = "";
                DyeingStep form_DyeingStep = new DyeingStep(1, s_stepNum, s_technologyName, s_proportionOrTime, s_temp, s_rate, txt_Dye_Code.Text, s_rev, txt_Notes.Text, true, null,  this, null);
                form_DyeingStep.Show();
            }
            catch
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    FADM_Form.CustomMessageBox.Show("未发现可修改的行,请先添加！", "操作异常", MessageBoxButtons.OK, false);
                else
                    FADM_Form.CustomMessageBox.Show("No modifiable rows found, please add first！", "Abnormal operation", MessageBoxButtons.OK, false);
            }
        }

        private void checkBox1_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                Communal._fadmSqlserver.ReviseData("Update dyeing_process Set OpenMedicine=1 where Code = '" + txt_Dye_Code.Text + "';");
            }
            else
            {
                Communal._fadmSqlserver.ReviseData("Update dyeing_process Set OpenMedicine=0 where Code = '" + txt_Dye_Code.Text + "';");
            }
        }
    }
}
