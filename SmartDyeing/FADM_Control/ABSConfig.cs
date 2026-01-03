using SmartDyeing.FADM_Form;
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
    public partial class ABSConfig : UserControl
    {
        public ABSConfig()
        {
            InitializeComponent();
            ABSCodeShow("");
        }

        /// <summary>
        /// 显示ABS代码
        /// </summary>
        /// <param name="s_brewingCode">选中的ABS代码</param>
        /// <returns>0:正常;-1:异常</returns>
        private int ABSCodeShow(string s_brewingCode)
        {
            try
            {
                //获取调液流程代码
                string s_sql = "SELECT Code  FROM Abs_process group by Code;";
                DataTable dt_brewcode = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                //捆绑
                dgv_Dye_Code.DataSource = new DataView(dt_brewcode);

                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    //设置标题文字
                    dgv_Dye_Code.Columns[0].HeaderCell.Value = "ABS代码";

                }
                else
                {
                    dgv_Dye_Code.Columns[0].HeaderCell.Value = "ABSCode";

                }

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

                //设置当前选中行
                for (int i = 0; i < dgv_Dye_Code.Rows.Count; i++)
                {
                    string s_temp = dgv_Dye_Code.Rows[i].Cells[0].Value.ToString();
                    if (s_temp == s_brewingCode)
                    {
                        dgv_Dye_Code.CurrentCell = dgv_Dye_Code.Rows[i].Cells[0];
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
        /// 显示ABS流程
        /// </summary>
        /// <param name="s_stepNum">当前选中的步号</param>
        /// <returns>0:正常;-1:异常</returns>
        public int ABSProcessShow(string s_stepNum)
        {
            try
            {
                //获取当前调液代码的调液流程
                string s_sql = "SELECT *  FROM Abs_process WHERE" +
                                   " Code = '" + txt_Dye_Code.Text + "' Order BY StepNum ; ";
                DataTable dt_brewprocess = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                //捆绑
                dgv_Child_DyeData.DataSource = new DataView(dt_brewprocess);

                //if (Lib_Card.Configure.Parameter.Other_Language == 0)
                //{
                    //设置标题栏名称
                    string[] sa_lineName = { "步号", "操作类型", "搅拌速度", "搅拌时间(s)", "排液时间(s)", "排比色皿时间(s)", "抽液时间(s)", "开始波长", "结束波长", "波长间隔", "加药量(g)" };
                    for (int i = 0; i < 11; i++)
                    {
                        dgv_Child_DyeData.Columns[i].HeaderCell.Value = sa_lineName[i];
                        //设置标题宽度
                        dgv_Child_DyeData.Columns[i].Width = (dgv_Child_DyeData.Width - 2) / 11;
                        //关闭点击标题自动排序功能
                        dgv_Child_DyeData.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                    }
                //}
                //else
                //{
                //    //设置标题栏名称
                //    string[] sa_lineName = { "StepNumber", "OperationType", "Percentage(%)/time(s) " };
                //    for (int i = 0; i < 3; i++)
                //    {
                //        dgv_Child_DyeData.Columns[i].HeaderCell.Value = sa_lineName[i];
                //        //设置标题宽度
                //        dgv_Child_DyeData.Columns[i].Width = (dgv_Child_DyeData.Width - 2) / 3;
                //        //关闭点击标题自动排序功能
                //        dgv_Child_DyeData.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                //    }

                //}
                //设置标题居中显示
                dgv_Child_DyeData.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                //设置标题字体
                dgv_Child_DyeData.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 14.25F);

                //设置内容居中显示
                dgv_Child_DyeData.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                //设置内容字体
                dgv_Child_DyeData.RowsDefaultCellStyle.Font = new Font("宋体", 14.25F);

                //设置行高
                dgv_Child_DyeData.RowTemplate.Height = 30;

                //设置当前选中行
                for (int i = 0; i < dgv_Child_DyeData.Rows.Count; i++)
                {
                    string s_temp = dgv_Child_DyeData.Rows[i].Cells[0].Value.ToString();
                    if (s_temp == s_stepNum)
                    {
                        dgv_Child_DyeData.CurrentCell = dgv_Child_DyeData.Rows[i].Cells[0];
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

        private void btn_DyeingCodeAdd_Click(object sender, EventArgs e)
        {
            txt_Dye_Code.Text = null;
            txt_Dye_Code.Enabled = true;
            txt_Dye_Code.Focus();
            btn_DyeingCodeDelete.Enabled = false;
            btn_DyeingProcessAdd.Enabled = false;
            btn_DyeingProcessUpdate.Enabled = false;
            btn_DyeingProcessDelete.Enabled = false;
        }

        private void btn_DyeingCodeDelete_Click(object sender, EventArgs e)
        {
            string s_sql = "DELETE FROM Abs_process" +
                               " WHERE Code = '" + txt_Dye_Code.Text + "';";
            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
            try
            {
                ABSCodeShow(dgv_Dye_Code.Rows[dgv_Dye_Code.CurrentCell.RowIndex - 1].Cells[0].Value.ToString());
            }
            catch
            {
                ABSCodeShow("");
            }
        }

        private void btn_DyeingProcessAdd_Click(object sender, EventArgs e)
        {
            string s_stepNum = (dgv_Child_DyeData.Rows.Count + 1).ToString();
            ABSStep form_AbsStep = new ABSStep(s_stepNum, "", txt_Dye_Code.Text, "", "", "", "", "", "", "", "", "", true, this);
            form_AbsStep.Show();
        }

        private void btn_DyeingProcessUpdate_Click(object sender, EventArgs e)
        {
            string s_stepNum = dgv_Child_DyeData.CurrentRow.Cells[0].Value.ToString();
            string s_technologyName = dgv_Child_DyeData.CurrentRow.Cells[1].Value.ToString().Trim();
            string s_stirringRate = dgv_Child_DyeData.CurrentRow.Cells[2].Value.ToString();
            string s_stirringTime = dgv_Child_DyeData.CurrentRow.Cells[3].Value.ToString();
            string s_drainTime = dgv_Child_DyeData.CurrentRow.Cells[4].Value.ToString();
            string s_parallelizingDishTime = dgv_Child_DyeData.CurrentRow.Cells[5].Value.ToString();
            string s_pumpingTime = dgv_Child_DyeData.CurrentRow.Cells[6].Value.ToString();
            string s_startingWavelength = dgv_Child_DyeData.CurrentRow.Cells[7].Value.ToString();
            string s_endWavelength = dgv_Child_DyeData.CurrentRow.Cells[8].Value.ToString();
            string s_wavelengthInterval = dgv_Child_DyeData.CurrentRow.Cells[9].Value.ToString();
            string s_dosage = dgv_Child_DyeData.CurrentRow.Cells[10].Value.ToString();
            ABSStep form_AbsStep = new ABSStep(s_stepNum, s_technologyName, txt_Dye_Code.Text, s_stirringRate, s_stirringTime, s_drainTime, s_parallelizingDishTime, s_pumpingTime, s_startingWavelength, s_endWavelength, s_wavelengthInterval, s_dosage, false, this);
            form_AbsStep.Show();
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

            string s_sql = "DELETE FROM Abs_process WHERE" +
                               " StepNum = '" + dgv_Child_DyeData.CurrentRow.Cells[0].Value.ToString() + "'" +
                               " AND Code = '" + txt_Dye_Code.Text + "' ;";
            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
            try
            {
                ABSProcessShow(dgv_Child_DyeData.Rows[dgv_Child_DyeData.CurrentCell.RowIndex - 1].Cells[0].Value.ToString());
            }
            catch
            {
                ABSProcessShow("");
            }
        }

        private void btn_Copy_Click(object sender, EventArgs e)
        {

        }

        private void txt_Dye_Code_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    if (txt_Dye_Code.Enabled && txt_Dye_Code.Text != null && txt_Dye_Code.Text != "")
                    {
                        //检索是否存在这个调液流程代码
                        for (int i = 0; i < dgv_Dye_Code.Rows.Count; i++)
                        {
                            if (txt_Dye_Code.Text == dgv_Dye_Code[0, i].Value.ToString())
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("ABS代码重复,请重新输入",
                                    "温馨提示", MessageBoxButtons.OK, false))
                                    {
                                        txt_Dye_Code.Text = null;
                                        txt_Dye_Code.Focus();
                                        return;
                                    }
                                }
                                else
                                {
                                    if (DialogResult.OK == FADM_Form.CustomMessageBox.Show("ABS code is duplicated, please re-enter it",
                                    "Tips", MessageBoxButtons.OK, false))
                                    {
                                        txt_Dye_Code.Text = null;
                                        txt_Dye_Code.Focus();
                                        return;
                                    }
                                }
                            }
                        }
                        //将新的调液流程代码写入数据库
                        string s_sql = "INSERT INTO Abs_process" +
                                           " (Code) VALUES( '" + txt_Dye_Code.Text + "' );";
                        FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                        //更新调液流程代码表
                        ABSCodeShow(txt_Dye_Code.Text);



                        //显示新增的调液流程
                        ABSProcessShow("");

                        //打开按钮
                        btn_DyeingProcessAdd.Enabled = true;
                        btn_DyeingProcessDelete.Enabled = true;
                        btn_DyeingProcessUpdate.Enabled = true;
                        txt_Dye_Code.Enabled = false;

                        //设置添加按钮焦点
                        btn_DyeingProcessAdd.Focus();
                    }
                    break;
                default:
                    break;
            }
        }

        private void dgv_Dye_Code_CurrentCellChanged(object sender, EventArgs e)
        {
            try
            {
                if (dgv_Dye_Code.CurrentCell != null)
                {
                    txt_Dye_Code.Text = dgv_Dye_Code.CurrentCell.Value.ToString();
                    ABSProcessShow("");
                    btn_DyeingCodeDelete.Enabled = true;
                    btn_DyeingProcessAdd.Enabled = true;
                    btn_DyeingProcessDelete.Enabled = true;
                    btn_DyeingProcessUpdate.Enabled = true;
                }
            }
            catch
            {
                txt_Dye_Code.Text = null;
                ABSProcessShow("");
                btn_DyeingCodeDelete.Enabled = false;
                btn_DyeingProcessAdd.Enabled = false;
                btn_DyeingProcessDelete.Enabled = false;
                btn_DyeingProcessUpdate.Enabled = false;
            }
        }
    }
}
