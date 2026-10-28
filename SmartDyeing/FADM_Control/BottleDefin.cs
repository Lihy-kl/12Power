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
    public partial class BottleDefin : UserControl
    {
        /// <summary>
        /// 定义新增标志位
        /// </summary>
        private bool _b_insert = false;

        public BottleDefin()
        {
            InitializeComponent();

            // 获取染助剂代码表头
            string s_sql = "SELECT AssistantCode, AssistantName FROM" +
                               " assistant_details ;";
            DataTable dt_assistant = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

            //染助剂代码可选项添加全部染助剂代码
            foreach (DataRow mDr in dt_assistant.Rows)
            {
                cbo_AssistantCode.Items.Add(mDr[dt_assistant.Columns[0]].ToString());
            }

            //获取调液流程代码
            s_sql = "SELECT * FROM brewing_code;";
            DataTable dt_brewCode = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

            //调液流程代码可选项添加全部调液流程
            foreach (DataRow mDr in dt_brewCode.Rows)
            {
                cbo_BrewingCode.Items.Add(mDr[dt_brewCode.Columns[0]].ToString());
            }

            if (!Communal._b_isAloneDripReserve)
            {
                lab_DripReserveFirst.Visible = false;
                cbo_DripReserveFirst.Visible = false;
            }

            //显示母液瓶表
            BottleHeadShow("");

            foreach (Control c in this.grp_BottleDetails.Controls)
            {
                if (c is TextBox)
                {
                    c.KeyPress += TextBox_KeyPress;
                }
            }
        }

        //输入检查
        void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox txt = (TextBox)sender;
            if (txt.Name == "txt_SettingConcentration" || txt.Name == "txt_DropMinWeight")
            {
                e.Handled = SmartDyeing.FADM_Object.MyTextBoxCheck.NumberDotTextbox_KeyPress(sender, e);
            }
            else
            {
                e.Handled = SmartDyeing.FADM_Object.MyTextBoxCheck.NumberTextbox_KeyPress(e);
            }
        }

        /// <summary>
        /// 显示母液瓶表
        /// </summary>
        /// <returns>0:正常;-1异常</returns>
        private int BottleHeadShow(string _BottleNum)
        {
            try
            {
                //获取母液瓶表头


                string s_sql = "SELECT bottle_details.BottleNum," +
                                   "bottle_details.AssistantCode," +
                                   "assistant_details.AssistantName," +
                                   "bottle_details.SettingConcentration " +
                                   "FROM bottle_details  LEFT JOIN assistant_details ON assistant_details.AssistantCode=bottle_details.AssistantCode where bottle_details.MachineID=" + FADM_Object.Communal._i_CurrentBrewNum + 
                                   " ORDER BY bottle_details.BottleNum;";
                DataTable dt_bottlehead = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                //捆绑
                dgv_Bottle.DataSource = new DataView(dt_bottlehead);

                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    //设置标题文字
                    dgv_Bottle.Columns[0].HeaderCell.Value = "瓶号";
                    dgv_Bottle.Columns[1].HeaderCell.Value = "染助剂代码";
                    dgv_Bottle.Columns[2].HeaderCell.Value = "染助剂名称";
                    dgv_Bottle.Columns[3].HeaderCell.Value = "设定浓度";
                    //设置标题字体
                    dgv_Bottle.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 14.25F);
                    //设置内容字体
                    dgv_Bottle.RowsDefaultCellStyle.Font = new Font("宋体", 14.25F);
                }
                else
                {
                    //设置标题文字
                    dgv_Bottle.Columns[0].HeaderCell.Value = "BottleNumber";
                    dgv_Bottle.Columns[1].HeaderCell.Value = "DyeingAuxiliariesCode";
                    dgv_Bottle.Columns[2].HeaderCell.Value = "DyeingAuxiliariesName";
                    dgv_Bottle.Columns[3].HeaderCell.Value = "SetConcentration";
                    //设置标题字体
                    dgv_Bottle.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 10.5F);
                    //设置内容字体
                    dgv_Bottle.RowsDefaultCellStyle.Font = new Font("宋体", 10.5F);
                }

                //设置标题宽度
                dgv_Bottle.Columns[0].Width = 100;
                dgv_Bottle.Columns[1].Width = 200;
                dgv_Bottle.Columns[2].Width = 200;
                dgv_Bottle.Columns[3].Width = 165;



                //关闭染助剂代码和设定浓度自动排序功能
                dgv_Bottle.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgv_Bottle.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;

                //设置标题居中显示
                dgv_Bottle.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

             

                //设置内容居中显示
                dgv_Bottle.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

               

                //设置行高
                dgv_Bottle.RowTemplate.Height = 30;

                //设置当前选中行
                for (int i = 0; i < dgv_Bottle.Rows.Count; i++)
                {
                    string s = dgv_Bottle.Rows[i].Cells[0].Value.ToString();
                    if (s == _BottleNum)
                    {
                        dgv_Bottle.CurrentCell = dgv_Bottle.Rows[i].Cells[0];
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

        //显示母液瓶详细信息
        private int BottleDetailsShow(string _BottleNum)
        {
            try
            {
                _b_insert = false;
                //获取当前母液瓶的资料
                string s_sql = "SELECT *  FROM bottle_details" +
                                   " WHERE BottleNum = " + Convert.ToInt16(dgv_Bottle.CurrentRow.Cells[0].Value.ToString()) + "  And MachineID = "+ FADM_Object.Communal._i_CurrentBrewNum+"; ";
                DataTable dt_bottledetails = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                foreach (DataColumn mDc in dt_bottledetails.Columns)
                {
                    string s_name = "txt_" + mDc.Caption.ToString();
                    foreach (Control c in this.grp_BottleDetails.Controls)
                    {
                        if (c is TextBox && c.Name == s_name)
                        {
                            c.Text = dt_bottledetails.Rows[0][mDc].ToString();
                            break;
                        }

                        if (c.Name == "txt_BottleNum")
                        {
                            c.Enabled = false;
                        }
                        else
                        {
                            c.Enabled = true;
                        }

                    }
                    switch (s_name)
                    {
                        case "txt_AssistantCode":
                            cbo_AssistantCode.Text = dt_bottledetails.Rows[0][mDc].ToString();
                            break;
                        case "txt_SyringeType":
                            cbo_SyringeType.Text = dt_bottledetails.Rows[0][mDc].ToString();
                            break;
                        case "txt_BrewingCode":
                            cbo_BrewingCode.Text = dt_bottledetails.Rows[0][mDc].ToString();
                            break;
                        case "txt_OriginalBottleNum":
                            cbo_OriginalBottleNum.Items.Clear();
                            if (dt_bottledetails.Rows[0][mDc].ToString() != "0")
                            {
                                cbo_OriginalBottleNum.Items.Add(dt_bottledetails.Rows[0][mDc].ToString());
                                cbo_OriginalBottleNum.Text = dt_bottledetails.Rows[0][mDc].ToString();
                            }
                            break;
                        case "txt_BrewingData":
                            dtp_BrewingData.Text = dt_bottledetails.Rows[0][mDc].ToString();
                            break;
                        case "txt_DripReserveFirst":
                            if (dt_bottledetails.Rows[0][mDc] is DBNull)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    cbo_DripReserveFirst.Text = "否";
                                }
                                else
                                {
                                    cbo_DripReserveFirst.Text = "No";
                                }
                            }
                            else
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    if (dt_bottledetails.Rows[0][mDc].ToString() == "1")
                                        cbo_DripReserveFirst.Text = "是";
                                    else
                                        cbo_DripReserveFirst.Text = "否";
                                }
                                else
                                {
                                    if (dt_bottledetails.Rows[0][mDc].ToString() == "1")
                                        cbo_DripReserveFirst.Text = "Yes";
                                    else
                                        cbo_DripReserveFirst.Text = "No";
                                }
                            }
                            break;
                        case "txt_Flowability":
                            if (dt_bottledetails.Rows[0][mDc] is DBNull)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    cbo_Flowability.Text = "中";
                                }
                                else
                                {
                                    cbo_Flowability.Text = "中";
                                }
                            }
                            else
                            {
                                cbo_Flowability.Text = dt_bottledetails.Rows[0][mDc].ToString();
                            }
                            break;
                        case "txt_WashSyringeSpan":
                            txt_WashSyringeSpan.Text = dt_bottledetails.Rows[0][mDc].ToString();
                            break;
                        default:
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

        private void btn_Insert_Click(object sender, EventArgs e)
        {
            foreach (Control c in this.grp_AssistantDetails.Controls)
            {
                if (c is RadioButton)
                {
                    c.Enabled = false;
                }
                else if (c is TextBox)
                {
                    c.Text = null;
                }
            }

            foreach (Control c in this.grp_BottleDetails.Controls)
            {
                if (c is TextBox || c is ComboBox || c is DateTimePicker)
                {
                    if (c.Name == "txt_BottleNum")
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
            txt_BottleNum.Focus();

            _b_insert = true;
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            //检查是否所有资料都已填写
            foreach (Control c in this.grp_BottleDetails.Controls)
            {
                if ((c is TextBox || (c is ComboBox && c.Name != "cbo_OriginalBottleNum")) &&
                    (c.Text == "" || c.Text == null))
                {
                    if (c.Name == "cbo_DripReserveFirst")
                    {
                        if (Communal._b_isAloneDripReserve)
                        {
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                FADM_Form.CustomMessageBox.Show("请完善所有资料后再点存档", "温馨提示", MessageBoxButtons.OK, false);
                            else
                                FADM_Form.CustomMessageBox.Show("Please complete all the information before clicking on the archive", "Tips", MessageBoxButtons.OK, false);
                            return;
                        }
                    }
                    else
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("请完善所有资料后再点存档", "温馨提示", MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("Please complete all the information before clicking on the archive", "Tips", MessageBoxButtons.OK, false);
                        return;
                    }
                }
            }

            string s_sql = "SELECT * FROM bottle_details WHERE AssistantCode = '" + cbo_AssistantCode.Text + "'" +
                               " AND SettingConcentration = '" + txt_SettingConcentration.Text + "' And MachineID = "+ FADM_Object.Communal._i_CurrentBrewNum+";";
            DataTable dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            if (dt_temp.Rows.Count > 0 && Convert.ToInt16(dt_temp.Rows[0]["BottleNum"]) != Convert.ToInt16(txt_BottleNum.Text))
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    FADM_Form.CustomMessageBox.Show("当前染助剂代码已存在该浓度,请核对!", "温馨提示",MessageBoxButtons.OK,false);
                else
                    FADM_Form.CustomMessageBox.Show("The current dyeing agent code already has this concentration, please verify!", "Tips", MessageBoxButtons.OK, false);
                return;
            }

            //判断G/L或WATER单位是否只有一个母液瓶
            List<string> lis_ass = new List<string>();
            string s_sql_ass = "SELECT * FROM assistant_details WHERE UnitOfAccount collate Chinese_PRC_CS_AS = 'G/L' OR UnitOfAccount  collate Chinese_PRC_CS_AS = 'Water' ;";
            DataTable dt_temp_ass = FADM_Object.Communal._fadmSqlserver.GetData(s_sql_ass);
            foreach (DataRow row in dt_temp_ass.Rows)
            {
                lis_ass.Add(row["AssistantCode"].ToString());
            }

            //如果是这两个特殊单位代码，就要判断是否瓶号是否一致
            if(lis_ass.Contains(cbo_AssistantCode.Text))
            {
                //查询此助剂是否存在母液瓶
                string s_sql_bottle = "SELECT * FROM bottle_details WHERE AssistantCode = '"+ cbo_AssistantCode.Text+ "' And MachineID =  " + FADM_Object.Communal._i_CurrentBrewNum + ";";
                
                DataTable dt_temp_bottle = FADM_Object.Communal._fadmSqlserver.GetData(s_sql_bottle);
                if(dt_temp_bottle.Rows.Count >0)
                {
                    if(txt_BottleNum.Text != dt_temp_bottle.Rows[0]["BottleNum"].ToString())
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            FADM_Form.CustomMessageBox.Show("此助剂只能存在一个母液瓶!", "温馨提示", MessageBoxButtons.OK, false);
                        else
                            FADM_Form.CustomMessageBox.Show("This additive can only be present in one mother liquor bottle", "Tips", MessageBoxButtons.OK, false);
                        
                        return;
                    }
                }
            }

            //s_sql = "SELECT * FROM machine_parameters WHERE MyID = 1;";
            //DataTable P_dt_machine = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
            int i_machine = Lib_Card.Configure.Parameter.Machine_Bottle_Total;
            //if (P_dt_machine.Rows.Count > 0)
            //{
            //    i_machine = Convert.ToInt16(P_dt_machine.Rows[0]["MachineType"]);
            //}

            //if (Convert.ToInt16(txt_BottleNum.Text) > i_machine)
            //{

            //}

            //将资料保存在List中
            List<string> lis_data = new List<string>();
            lis_data.Add(txt_BottleNum.Text);
            lis_data.Add(cbo_AssistantCode.Text);
            lis_data.Add((Convert.ToDouble(txt_SettingConcentration.Text)).ToString());
            lis_data.Add(txt_CurrentWeight.Text);
            lis_data.Add(cbo_SyringeType.Text);
            lis_data.Add(txt_DropMinWeight.Text);
            lis_data.Add(cbo_BrewingCode.Text);
            if (cbo_OriginalBottleNum.Text == "" || cbo_OriginalBottleNum.Text == null)
            {
                lis_data.Add("0");
            }
            else
            {
                lis_data.Add(cbo_OriginalBottleNum.Text);
            }
            lis_data.Add(txt_AllowMaxWeight.Text);
            lis_data.Add(dtp_BrewingData.Text);

            if (Convert.ToInt16(txt_BottleNum.Text) > i_machine)
            {
                lis_data.Add(txt_SettingConcentration.Text);
            }
            else
            {
                if (_b_insert)
                {
                    string s_real = "0.00";
                    if (rdo_4.Checked)
                    {
                        s_real = (Convert.ToDouble(txt_SettingConcentration.Text)).ToString();
                    }
                    if (FADM_Object.Communal._b_isSaveRealConcentration)
                    {
                        s_real = (Convert.ToDouble(txt_SettingConcentration.Text)).ToString();
                    }

                    lis_data.Add(s_real);
                }
                else
                {
                    s_sql = "SELECT * FROM  bottle_details WHERE BottleNum ='" + lis_data[0]  +"' And MachineID =  " + FADM_Object.Communal._i_CurrentBrewNum + ";";
                    DataTable dt_bottle = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                    string s_real = Convert.ToString(dt_bottle.Rows[0]["RealConcentration"]);
                    if (rdo_4.Checked)
                    {
                        s_real = (Convert.ToDouble(txt_SettingConcentration.Text)).ToString();
                    }
                    if (FADM_Object.Communal._b_isSaveRealConcentration)
                    {
                        s_real = (Convert.ToDouble(txt_SettingConcentration.Text)).ToString();
                    }

                    lis_data.Add(s_real);
                }
            }
            if (cbo_DripReserveFirst.Text == "是" || cbo_DripReserveFirst.Text == "Yes")
            {
                lis_data.Add("1");
            }
            else
            {
                lis_data.Add("0");
            }
            lis_data.Add(txt_WashSyringeSpan.Text);
            lis_data.Add(FADM_Object.Communal._i_CurrentBrewNum.ToString());
            lis_data.Add(cbo_Flowability.Text);
            if (_b_insert)
            {
                //如果是新增
                s_sql = "INSERT INTO bottle_details (" +
                            " BottleNum, AssistantCode, SettingConcentration," +
                            " CurrentWeight, SyringeType, DropMinWeight," +
                            " BrewingCode, OriginalBottleNum, AllowMaxWeight," +
                            " BrewingData, RealConcentration,DripReserveFirst,WashSyringeSpan,MachineID,Flowability) VALUES( '" + lis_data[0] + "'," +
                            " '" + lis_data[1] + "','" + lis_data[2] + "'," +
                            " '" + lis_data[3] + "','" + lis_data[4] + "'," +
                            " '" + lis_data[5] + "','" + lis_data[6] + "'," +
                            " '" + lis_data[7] + "','" + lis_data[8] + "'," +
                            "'" + lis_data[9] + "','" + lis_data[10] + "','" + lis_data[11] + "','" + lis_data[12] + "','" + lis_data[13] + "','" + lis_data[14] + "');";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);


                //更新母液瓶资料表
                BottleHeadShow(txt_BottleNum.Text);

                this.btn_Insert.Focus();
            }
            else
            {

                //如果是修改

                //先判断设定浓度有没有改变
                s_sql = "SELECT * FROM bottle_details WHERE BottleNum ='" + lis_data[0] + "' And MachineID =  " + FADM_Object.Communal._i_CurrentBrewNum + ";";

                DataTable dt_bottle = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                string s_setcon = Convert.ToString(dt_bottle.Rows[0]["SettingConcentration"]);

                if (s_setcon != txt_SettingConcentration.Text && lis_data[0] != "200" && lis_data[0] != "201"
                    && rdo_4.Checked == false && Convert.ToInt16(txt_BottleNum.Text) <= i_machine)
                {

                    string s_real = "0.00";

                    if (FADM_Object.Communal._b_isSaveRealConcentration)
                    {
                        s_real = (Convert.ToDouble(txt_SettingConcentration.Text)).ToString();
                    }

                    s_sql = "UPDATE bottle_details Set" +
                                " AssistantCode = '" + lis_data[1] + "'," +
                                " SettingConcentration = '" + lis_data[2] + "'," +
                                " RealConcentration =  '" + s_real + "'," +
                                " CurrentWeight= '" + lis_data[3] + "'," +
                                " SyringeType = '" + lis_data[4] + "'," +
                                " DropMinWeight = '" + lis_data[5] + "'," +
                                " BrewingCode = '" + lis_data[6] + "'," +
                                " OriginalBottleNum = '" + lis_data[7] + "'," +
                                " AllowMaxWeight ='" + lis_data[8] + "'," +
                                " BrewingData = '" + lis_data[9] + "'," +
                                " DripReserveFirst = '" + lis_data[11] + "'," +
                                " WashSyringeSpan = '" + lis_data[12] + "'," +
                                " Flowability = '" + lis_data[14] + "'," +
                                " AdjustSuccess = 0" +
                                " WHERE BottleNum ='" + lis_data[0] + "' ;";
                }
                else
                {
                    s_sql = "UPDATE bottle_details Set" +
                                " AssistantCode = '" + lis_data[1] + "'," +
                                " SettingConcentration = '" + lis_data[2] + "'," +
                                " CurrentWeight = '" + lis_data[3] + "'," +
                                " SyringeType = '" + lis_data[4] + "'," +
                                " DropMinWeight = '" + lis_data[5] + "'," +
                                " BrewingCode = '" + lis_data[6] + "'," +
                                " OriginalBottleNum = '" + lis_data[7] + "'," +
                                " AllowMaxWeight ='" + lis_data[8] + "'," +
                                " BrewingData = '" + lis_data[9] + "'," +
                                " RealConcentration = '" + lis_data[10] + "'," +
                                " DripReserveFirst = '" + lis_data[11] + "'," +
                                " WashSyringeSpan = '" + lis_data[12] + "'," +
                                " Flowability = '" + lis_data[14] + "'," +
                                " AdjustSuccess = 0" +
                                " WHERE BottleNum ='" + lis_data[0] + "' And MachineID =  " + FADM_Object.Communal._i_CurrentBrewNum + ";";
                }
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                BottleHeadShow(txt_BottleNum.Text);
            }
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "保存" + txt_BottleNum.Text + "母液资料");
            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                FADM_Form.CustomMessageBox.Show("保存成功!", "温馨提示", MessageBoxButtons.OK,  false);
            else
                FADM_Form.CustomMessageBox.Show("Successfully saved!", "Tips", MessageBoxButtons.OK, false);


        }

        private void btn_Delete_Click(object sender, EventArgs e)
        {
            string s_sql = "DELETE FROM bottle_details" +
                               " WHERE BottleNum = '" + txt_BottleNum.Text + "' And MachineID =  " + FADM_Object.Communal._i_CurrentBrewNum + ";";
            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "删除"+ txt_BottleNum.Text+"母液资料");
            try
            {
                BottleHeadShow(dgv_Bottle.Rows[dgv_Bottle.CurrentCell.RowIndex - 1].Cells[0].Value.ToString());
            }
            catch
            {
                BottleHeadShow("");
            }
        }

        private void dgv_Bottle_CurrentCellChanged(object sender, EventArgs e)
        {
            try
            {
                if (dgv_Bottle.CurrentCell != null)
                {
                    string s = dgv_Bottle.CurrentCell.Value.ToString();
                    BottleDetailsShow(s);
                    _b_insert = false;
                }
            }
            catch
            {
                foreach (Control c in this.grp_BottleDetails.Controls)
                {
                    if (c is TextBox || c is ComboBox || c is DateTimePicker)
                    {
                        c.Text = null;
                    }
                }
            }
        }

        private void txt_BottleNum_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    if (txt_BottleNum.Enabled && txt_BottleNum.Text != null && txt_BottleNum.Text != "")
                    {
                        if (Convert.ToInt16(txt_BottleNum.Text) > 0)
                        {
                            txt_BottleNum.Text = Convert.ToInt16(txt_BottleNum.Text).ToString();
                            //检索是否存在这个瓶号
                            for (int i = 0; i < dgv_Bottle.Rows.Count; i++)
                            {
                                if (txt_BottleNum.Text == dgv_Bottle[0, i].Value.ToString())
                                {
                                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    {
                                        if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("母液瓶号重复,请重新输入", "温馨提示", MessageBoxButtons.OK, false))
                                        {
                                            txt_BottleNum.Text = null;
                                            txt_BottleNum.Focus();
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("Mother liquor bottle number is duplicate, please re-enter", "Tips", MessageBoxButtons.OK, false))
                                        {
                                            txt_BottleNum.Text = null;
                                            txt_BottleNum.Focus();
                                            return;
                                        }
                                    }
                                }
                            }

                            //打开按钮失能
                            foreach (Control c in this.grp_BottleDetails.Controls)
                            {
                                if (c.Name == "txt_BottleNum")
                                {
                                    c.Enabled = false;
                                }
                                else
                                {
                                    c.Enabled = true;
                                }
                            }
                            //自动填充无用参数
                            txt_CurrentWeight.Text = "0";
                            txt_DropMinWeight.Text = "1";
                            cbo_SyringeType.Text = "小针筒";
                            txt_AllowMaxWeight.Text = "1000";
                            //染助剂代码设置焦点
                            cbo_AssistantCode.Focus();
                        }
                    }
                    break;
                default:

                    break;
            }
        }

        private void cbo_AssistantCode_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    txt_SettingConcentration.Focus();
                    break;
                default: break;
            }
        }

        private void cbo_AssistantCode_Leave(object sender, EventArgs e)
        {
            if ((txt_AssistantName.Text == null || txt_AssistantName.Text == "") && (cbo_AssistantCode.Text != null && cbo_AssistantCode.Text != ""))
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("未找到该染助剂代码,请检查后重新输入",
                    "温馨提示", MessageBoxButtons.OK, false))
                    {
                        cbo_AssistantCode.Text = null;
                        cbo_AssistantCode.Focus();
                    }
                }
                else
                {
                    if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("The dyeing agent code was not found. Please check and re-enter it",
                    "Tips", MessageBoxButtons.OK, false))
                    {
                        cbo_AssistantCode.Text = null;
                        cbo_AssistantCode.Focus();
                    }
                }
            }
            cbo_OriginalBottleNum_Click(null, null);
        }

        private void cbo_AssistantCode_TextChanged(object sender, EventArgs e)
        {
            try
            {
                //获取当前染助剂代码的资料
                string s_sql = "SELECT *  FROM assistant_details WHERE" +
                                   " AssistantCode = '" + cbo_AssistantCode.Text + "' ; ";

                DataTable dt_assistantdetails = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);


                foreach (DataColumn mDc in dt_assistantdetails.Columns)
                {
                    string s_name = "txt_" + mDc.Caption.ToString();
                    foreach (Control c in this.grp_AssistantDetails.Controls)
                    {
                        if (c is TextBox && c.Name == s_name)
                        {
                            c.Text = dt_assistantdetails.Rows[0][mDc].ToString();
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

                }
                return;
            }
            catch
            {
                foreach (Control c in this.grp_AssistantDetails.Controls)
                {
                    if (c is RadioButton)
                    {
                        c.Enabled = false;
                    }
                    else if (c is TextBox)
                    {
                        c.Text = null;
                    }
                }
            }
        }

        private void txt_SettingConcentration_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    txt_CurrentWeight.Focus();
                    break;
                default: break;
            }
        }

        private void txt_SettingConcentration_Leave(object sender, EventArgs e)
        {
            try
            {
                string s_sql = "SELECT BottleNum FROM bottle_details WHERE" +
                                   " AssistantCode = '" + cbo_AssistantCode.Text + "' AND" +
                                   " SettingConcentration = '" + Convert.ToDouble(txt_SettingConcentration.Text) + "' And MachineID = "+ FADM_Object.Communal._i_CurrentBrewNum +
                                   " GROUP BY BottleNum ;";

                DataTable dt_settingconcentration = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                if (dt_settingconcentration.Rows.Count > 0)
                {
                    string s_temp = dt_settingconcentration.Rows[0][0].ToString();
                    if (s_temp != txt_BottleNum.Text)
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        {
                            if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("当前设定浓度与" + s_temp + "号母液瓶设定浓度相同," +
                            "请检查后重新输入", "温馨提示", MessageBoxButtons.OK, false))
                            {
                                txt_SettingConcentration.Text = null;
                                txt_SettingConcentration.Focus();
                                return;
                            }
                        }
                        else
                        {

                            if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("The dyeing agent code was not found. Please check and re-enter the current set concentration that is the same as the set concentration in the" + s_temp + "mother liquor bottle," +
                            " Please check and re-enter", "Tips", MessageBoxButtons.OK, false))
                            {
                                txt_SettingConcentration.Text = null;
                                txt_SettingConcentration.Focus();
                                return;
                            }
                        }
                    }

                }

            }
            catch
            {
                return;
            }
        }

        private void txt_CurrentWeight_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    cbo_SyringeType.Focus();
                    break;
                default: break;
            }
        }

        private void cbo_SyringeType_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    txt_DropMinWeight.Focus();
                    break;
                default: break;
            }
        }

        private void txt_DropMinWeight_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    cbo_BrewingCode.Focus();
                    break;
                default: break;
            }
        }

        private void cbo_BrewingCode_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    cbo_OriginalBottleNum_Click(null, null);
                    cbo_OriginalBottleNum.Focus();
                    break;
                default: break;
            }
        }

        private void cbo_OriginalBottleNum_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    txt_AllowMaxWeight.Focus();
                    break;
                default: break;
            }
        }

        private void txt_AllowMaxWeight_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    try
                    {
                        if (txt_AllowMaxWeight.Text != null && txt_AllowMaxWeight.Text != "")
                        {
                            if (Convert.ToInt16(txt_AllowMaxWeight.Text) > 1000)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                    FADM_Form.CustomMessageBox.Show("允许最大量超过1000,请检查!", "温馨提示", MessageBoxButtons.OK, false);
                                else
                                    FADM_Form.CustomMessageBox.Show("Allow maximum quantity exceeding 1000, please check!", "Tips", MessageBoxButtons.OK, false);
                                txt_AllowMaxWeight = null;
                                return;
                            }

                            txt_WashSyringeSpan.Focus();

                        }
                        else
                        {
                            return;
                        }
                    }
                    catch
                    { }

                    break;
                default: break;
            }
        }

        private void cbo_OriginalBottleNum_Click(object sender, EventArgs e)
        {
            try
            {
                string s_sql = "SELECT BottleNum FROM bottle_details WHERE" +
                                   " AssistantCode = '" + cbo_AssistantCode.Text + "' AND" +
                                   " SettingConcentration > '" + Convert.ToDouble(txt_SettingConcentration.Text) + "' And MachineID = " + FADM_Object.Communal._i_CurrentBrewNum +
                                   " GROUP BY BottleNum ;";
                DataTable dt_originalbottlenum = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                cbo_OriginalBottleNum.Items.Clear();
                foreach (DataRow mdr in dt_originalbottlenum.Rows)
                {
                    cbo_OriginalBottleNum.Items.Add(mdr[0].ToString());
                }
            }
            catch
            {
                cbo_OriginalBottleNum.Items.Clear();
                cbo_OriginalBottleNum.Text = null;
            }
        }

        private void cbo_DripReserveFirst_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    try
                    {
                        if (cbo_DripReserveFirst.Text != null && cbo_DripReserveFirst.Text != "")
                        {
                            if (!cbo_Flowability.Visible)
                                btn_Save.Focus();
                            else
                                cbo_Flowability.Focus();
                        }
                        else
                        {
                            return;
                        }
                    }
                    catch
                    { }

                    break;
                default: break;
            }
        }

        private void txt_WashSyringeSpan_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    try
                    {
                        if (Communal._b_isAloneDripReserve)
                        {
                            cbo_DripReserveFirst.Focus();
                        }
                        else
                        {
                            btn_Save.Focus();
                        }
                    }
                    catch
                    { }

                    break;
                default: break;
            }
        }

        private void cbo_Flowability_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    try
                    {
                        if (cbo_Flowability.Text != null && cbo_Flowability.Text != "")
                        {
                            btn_Save.Focus();
                        }
                        else
                        {
                            return;
                        }
                    }
                    catch
                    { }

                    break;
                default: break;
            }
        }

        private void cbo_SyringeType_Leave(object sender, EventArgs e)
        {
            try
            {
                if (cbo_SyringeType.Text == "粉桶")
                {
                    lab_Flowability.Visible = true;
                    cbo_Flowability.Visible = true;
                }
                else
                {
                    lab_Flowability.Visible = false;
                    cbo_Flowability.Visible = false;
                    cbo_Flowability.Text = "中";
                }
            }
            catch
            {

            }
        }

        private void cbo_SyringeType_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if(cbo_SyringeType.Text == "粉桶")
                {
                    lab_Flowability.Visible = true;
                    cbo_Flowability.Visible = true;
                }
                else
                {
                    lab_Flowability.Visible = false;
                    cbo_Flowability.Visible = false;
                    cbo_Flowability.Text = "中";
                }
            }
            catch
            {
                
            }
        }
    }
}
