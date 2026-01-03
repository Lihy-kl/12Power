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
    public partial class AssistantDefin : UserControl
    {
        /// <summary>
        /// 定义新增标志位
        /// </summary>
        private bool _b_insert = false;

        public AssistantDefin()
        {
            InitializeComponent();

            //更新当面界面编号
            //Class_Module.MyModule.Module_ConNum = 2;

            AssistantHeadShow("");

            foreach (Control c in this.Controls)
            {
                if (c is TextBox)
                {
                    c.KeyPress += TextBox_KeyPress;
                }
            }

            if(!FADM_Object.Communal._b_isAssShowWater)
            {
                rdo_3.Visible = false;
                rdo_4.Visible = false;
            }
        }

        //输入检查
        void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox txt = (TextBox)sender;
            if (txt.Name != "txt_AssistantCode" && txt.Name != "txt_AssistantBarCode" && txt.Name != "txt_AssistantName")
            {
                if (txt.Name != "txt_TermOfValidity")
                {
                    e.Handled = SmartDyeing.FADM_Object.MyTextBoxCheck.NumberDotTextbox_KeyPress(sender, e);
                }
                else
                {
                    e.Handled = SmartDyeing.FADM_Object.MyTextBoxCheck.NumberTextbox_KeyPress(e);
                }
            }
        }

        /// <summary>
        /// 显示染助剂表头
        /// </summary>
        /// <returns>0:正常;-1异常</returns>
        private int AssistantHeadShow(string _AssistantCode)
        {
            try
            {
                //获取染助剂代码表头
                string s_sql = "SELECT AssistantCode, AssistantName FROM" +
                                   " assistant_details order by " + (FADM_Object.Communal._b_isAssOrderByCode ? "AssistantCode;" : "ID;");
                DataTable dt_assistant = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                //捆绑
                dgv_Assistant.DataSource = new DataView(dt_assistant);

                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    //设置标题文字
                    dgv_Assistant.Columns[0].HeaderCell.Value = "染助剂代码";
                    dgv_Assistant.Columns[1].HeaderCell.Value = "染助剂名称";
                    //设置标题字体
                    dgv_Assistant.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 14.25F);
                    //设置内容字体
                    dgv_Assistant.RowsDefaultCellStyle.Font = new Font("宋体", 14.25F);
                }
                else
                {
                    dgv_Assistant.Columns[0].HeaderCell.Value = "DyeingAuxiliariesCode";
                    dgv_Assistant.Columns[1].HeaderCell.Value = "DyeingAuxiliariesName";
                    dgv_Assistant.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 10.5F);
                    dgv_Assistant.RowsDefaultCellStyle.Font = new Font("宋体", 10.5F);

                }

                //设置标题宽度
                dgv_Assistant.Columns[0].Width = 200;
                if (dgv_Assistant.Rows.Count > 30)
                {
                    dgv_Assistant.Columns[1].Width = 404;
                }
                else
                {
                    dgv_Assistant.Columns[1].Width = 424;
                }


                //关闭染助剂名称自动排序功能
                dgv_Assistant.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;

                //设置标题居中显示
                dgv_Assistant.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

               

                //设置内容居中显示
                dgv_Assistant.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

               

                //设置行高
                dgv_Assistant.RowTemplate.Height = 30;

                //设置当前选中行
                for (int i = 0; i < dgv_Assistant.Rows.Count; i++)
                {
                    string s_assistan = dgv_Assistant.Rows[i].Cells[0].Value.ToString();
                    if (s_assistan == _AssistantCode)
                    {
                        dgv_Assistant.CurrentCell = dgv_Assistant.Rows[i].Cells[0];
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

        //显示染助剂详细信息
        private int AssistantDetailsShow(string _AssistantCode)
        {
            try
            {
                _b_insert = false;
                //获取当前染助剂代码的资料
                string s_sql = "SELECT *  FROM assistant_details WHERE" +
                                  " AssistantCode = '" + dgv_Assistant.CurrentRow.Cells[0].Value.ToString() + "' ; ";

                DataTable dt_assistantdetails = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                foreach (DataColumn mDc in dt_assistantdetails.Columns)
                {
                    string s_name = "txt_" + mDc.Caption.ToString();
                    foreach (Control c in this.grp_AssistantDetails.Controls)
                    {
                        if (c is TextBox && c.Name == s_name)
                        {
                            if (dt_assistantdetails.Rows[0][mDc] is DBNull)
                                c.Text = "";
                            else
                                c.Text = dt_assistantdetails.Rows[0][mDc].ToString();
                        }

                        if (c.Name == "txt_AssistantCode")
                        {
                            c.Enabled = false;
                        }
                        else
                        {
                            c.Enabled = true;
                        }

                    }
                    if (s_name == "txt_UnitOfAccount")
                    {
                        if (dt_assistantdetails.Rows[0][mDc].ToString() == "%")
                        {
                            rdo_1.Checked = true;
                        }
                        else if (dt_assistantdetails.Rows[0][mDc].ToString() == "g/l")
                        {
                            rdo_2.Checked = true;
                        }
                        else if (dt_assistantdetails.Rows[0][mDc].ToString() == "G/L")
                        {
                            rdo_3.Checked = true;
                        }
                        else
                        {
                            rdo_4.Checked = true;
                        }
                    }
                    if (s_name == "txt_AssistantType")
                    {
                        cbo_AssistantType.Text = dt_assistantdetails.Rows[0][mDc].ToString();
                    }

                    if (s_name == "txt_Reweigh")
                    {
                        if (dt_assistantdetails.Rows[0][mDc] is DBNull)
                        {
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            {
                                cbo_Reweigh.Text = "否";
                            }
                            else
                            {
                                cbo_Reweigh.Text = "No";
                            }
                        }
                        else
                        {
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            {
                                if (dt_assistantdetails.Rows[0][mDc].ToString() == "1")
                                    cbo_Reweigh.Text = "是";
                                else
                                    cbo_Reweigh.Text = "否";
                            }
                            else
                            {
                                if (dt_assistantdetails.Rows[0][mDc].ToString() == "1")
                                    cbo_Reweigh.Text = "Yes";
                                else
                                    cbo_Reweigh.Text = "No";
                            }
                        }
                    }
                }
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        private void btn_Insert_Click(object sender, EventArgs e)
        {
            foreach (Control c in this.grp_AssistantDetails.Controls)
            {
                if (c is RadioButton)
                {
                    c.Enabled = false;
                }
                else if (c is TextBox || c is ComboBox)
                {
                    if (c.Name == "txt_AssistantCode")
                    {
                        c.Enabled = true;
                    }
                    else
                    {
                        c.Enabled = false;
                    }
                    c.Text = null;
                }
            }
            //设置焦点
            txt_AssistantCode.Focus();

            _b_insert = true;
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            //检查是否所有资料都已填写
            foreach (Control c in this.grp_AssistantDetails.Controls)
            {
                if ((c is TextBox || c is ComboBox) && (c.Text == "" || c.Text == null))
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        FADM_Form.CustomMessageBox.Show("请完善所有资料后再点存档", "温馨提示", MessageBoxButtons.OK, false);
                    else
                        FADM_Form.CustomMessageBox.Show("Please complete all the information before clicking on the archive", "Tips", MessageBoxButtons.OK, false);
                    return;
                }
            }

            //将资料保存在List中
            List<string> lis_data = new List<string>();
            lis_data.Add(txt_AssistantCode.Text);
            lis_data.Add(txt_AssistantBarCode.Text);
            lis_data.Add(txt_AssistantName.Text);
            lis_data.Add(cbo_AssistantType.Text);
            if (rdo_1.Checked)
            {
                lis_data.Add(rdo_1.Text);
            }
            else if (rdo_2.Checked)
            {
                lis_data.Add(rdo_2.Text);
            }
            else if (rdo_3.Checked)
            {
                lis_data.Add(rdo_3.Text);
            }
            else
            {
                lis_data.Add(rdo_4.Text);
            }
            lis_data.Add(txt_AllowMinColoringConcentration.Text);
            lis_data.Add(txt_AllowMaxColoringConcentration.Text);
            lis_data.Add(txt_TermOfValidity.Text);
            lis_data.Add(txt_Intensity.Text);
            lis_data.Add(txt_Cost.Text);

            //判断是否只有多于一个 G/L或WATER

            if (lis_data[4] == "G/L" || lis_data[4] == "Water")
            {
                string s_sql_assistant_details = "Select * from assistant_details where UnitOfAccount  collate Chinese_PRC_CS_AS ='" + lis_data[4]+ "' ;";
                DataTable dt_ass = FADM_Object.Communal._fadmSqlserver.GetData(s_sql_assistant_details);
                if(dt_ass.Rows.Count>0)
                {
                    //判断是否助剂代码一样(修改时需要判断，新增时，助剂代码重复已判断)
                    if (dt_ass.Rows[0]["AssistantCode"].ToString() != txt_AssistantCode.Text)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("只能存在一个此单位助剂代码!", "温馨提示", MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("Only one agent code for this unit can exist", "Tips", MessageBoxButtons.OK, false);
                        return;
                    }
                }
            }

            if (cbo_Reweigh.Text == "是" || cbo_Reweigh.Text == "Yes")
            {
                lis_data.Add("1");
            }
            else
            {
                lis_data.Add("0");
            }

            if (_b_insert)
            {
                //如果是新增
                string s_sql = "INSERT INTO assistant_details" +
                                   " (AssistantCode, AssistantBarCode, AssistantName, AssistantType, UnitOfAccount," +
                                   " AllowMinColoringConcentration, AllowMaxColoringConcentration, TermOfValidity," +
                                   " Intensity, Cost,Reweigh) VALUES('" + lis_data[0] + "','" + lis_data[1] + "','" + lis_data[2] + "'," +
                                   "'" + lis_data[3] + "','" + lis_data[4] + "','" + lis_data[5] + "','" + lis_data[6] + "'," +
                                   "'" + lis_data[7] + "','" + lis_data[8] + "','" + lis_data[9] + "','" + lis_data[10] + "');";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                //更新染助剂资料表
                AssistantHeadShow(txt_AssistantCode.Text);

                this.btn_Insert.Focus();
            }
            else
            {
                //如果是修改
                string s_sql = "UPDATE assistant_details Set" +
                                   " AssistantBarCode = '" + lis_data[1] + "', AssistantName = '" + lis_data[2] + "'," +
                                   " AssistantType= '" + lis_data[3] + "', UnitOfAccount = '" + lis_data[4] + "'," +
                                   " AllowMinColoringConcentration = '" + lis_data[5] + "'," +
                                   " AllowMaxColoringConcentration = '" + lis_data[6] + "', TermOfValidity = '" + lis_data[7] + "'," +
                                   " Intensity ='" + lis_data[8] + "',Cost = '" + lis_data[9] + "',Reweigh = '" + lis_data[10] + "' WHERE AssistantCode ='" + lis_data[0] + "' ;";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                AssistantHeadShow(txt_AssistantCode.Text);
            }
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "保存" + txt_AssistantCode.Text + "助剂代码资料");
            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                FADM_Form.CustomMessageBox.Show("保存成功!", "温馨提示", MessageBoxButtons.OK, false);
            else
                FADM_Form.CustomMessageBox.Show("Successfully saved!", "Tips", MessageBoxButtons.OK, false);
        }

        private void btn_Delete_Click(object sender, EventArgs e)
        {
            string s_sql = "DELETE FROM assistant_details" +
                               " WHERE AssistantCode = '" + txt_AssistantCode.Text + "';";
            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

            s_sql = "DELETE FROM bottle_details" +
                        " WHERE AssistantCode = '" + txt_AssistantCode.Text + "';";
            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "删除" + txt_AssistantCode.Text + "助剂代码资料");
            try
            {
                AssistantHeadShow(dgv_Assistant.Rows[dgv_Assistant.CurrentCell.RowIndex - 1].Cells[0].Value.ToString());
            }
            catch
            {
                AssistantHeadShow("");
            }
        }

        private void dgv_Assistant_CurrentCellChanged(object sender, EventArgs e)
        {
            try
            {
                if (dgv_Assistant.CurrentCell != null)
                {
                    string s_temp = dgv_Assistant.CurrentCell.Value.ToString();
                    AssistantDetailsShow(s_temp);
                    _b_insert = false;
                }
            }
            catch
            {
                foreach (Control c in this.grp_AssistantDetails.Controls)
                {
                    if (c is TextBox || c is ComboBox)
                    {
                        c.Text = null;
                    }
                }
            }
        }

        private void txt_AssistantCode_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    if (txt_AssistantCode.Enabled && txt_AssistantCode.Text != null && txt_AssistantCode.Text != "")
                    {
                        //检索是否存在这个染助剂名称
                        for (int i = 0; i < dgv_Assistant.Rows.Count; i++)
                        {
                            if (txt_AssistantCode.Text.ToLower() == dgv_Assistant[0, i].Value.ToString().ToLower())
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("染助剂代码重复,请重新输入", "温馨提示", MessageBoxButtons.OK, false))
                                    {
                                        txt_AssistantCode.Text = null;
                                        txt_AssistantCode.Focus();
                                        return;
                                    }
                                }
                                else
                                {
                                    if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("Dyeing agent code is duplicate, please re-enter", "Tips", MessageBoxButtons.OK, false))
                                    {
                                        txt_AssistantCode.Text = null;
                                        txt_AssistantCode.Focus();
                                        return;
                                    }
                                }
                            }
                        }

                        //打开按钮失能
                        foreach (Control c in this.grp_AssistantDetails.Controls)
                        {
                            if (c.Name == "txt_AssistantCode")
                            {
                                c.Enabled = false;
                            }
                            else
                            {
                                c.Enabled = true;
                            }
                        }
                        //自动填充无用参数
                        txt_AssistantBarCode.Text = txt_AssistantCode.Text;
                        txt_AllowMinColoringConcentration.Text = "0";
                        txt_AllowMaxColoringConcentration.Text = "0";
                        txt_Cost.Text = "100";
                        txt_Intensity.Text = "100";
                        //染助剂名称设置焦点
                        txt_AssistantName.Focus();

                    }
                    break;
                default:
                    break;
            }
        }

        private void rdo_1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    txt_TermOfValidity.Focus();
                    break;
                default: break;
            }
        }

        private void rdo_2_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    txt_TermOfValidity.Focus();
                    break;
                default: break;
            }
        }

        private void rdo_3_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    txt_TermOfValidity.Focus();
                    break;
                default: break;
            }
        }

        private void rdo_4_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    txt_TermOfValidity.Focus();
                    break;
                default: break;
            }
        }

        private void txt_TermOfValidity_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    btn_Save.Focus();
                    break;
                default: break;
            }
        }
    }
}
