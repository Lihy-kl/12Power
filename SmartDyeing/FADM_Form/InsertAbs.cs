using Lib_File;
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

namespace SmartDyeing.FADM_Form
{
    public partial class InsertAbs : Form
    {
        public InsertAbs()
        {
            InitializeComponent();
            ShowHeader();
            DropRecordHeadShow();
        }

        private void BtnBottleCheckStart_Click(object sender, EventArgs e)
        {
            try
            {
                //FADM_Object.Communal._fadmSqlserver.ReviseData("TRUNCATE TABLE abs_wait_list");

                char[] ca_chars = { ',', '，' };
                string[] sa_strings = TxtCheckBottleNo.Text.Split(ca_chars);

                char[] ca_chars1 = { '-', '-' };
                List<int> lis_ints = new List<int>();
                foreach (string s in sa_strings)
                {
                    string[] sa_strings1 = s.Split(ca_chars1);
                    if (1 == sa_strings1.Length)
                        lis_ints.Add(Convert.ToInt16(s));
                    else
                        for (int i = Convert.ToInt16(sa_strings1[0]); i <= Convert.ToInt16(sa_strings1[1]); i++)
                            lis_ints.Add(i);

                }

                lis_ints.Sort();
                lis_ints = lis_ints.ToList();

                foreach (int i in lis_ints)
                {
                    DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(
                        "SELECT * FROM bottle_details WHERE BottleNum = " + i + ";");
                    if (0 == dt_data.Rows.Count || i > Lib_Card.Configure.Parameter.Machine_Bottle_Total)
                        continue;
                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                        "INSERT INTO abs_wait_list(BottleNum, InsertDate,Type) VALUES('" + i + "','"+ DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")+"',0);");
                    Thread.Sleep(10);
                }

                FADM_Form.CustomMessageBox.Show("启动成功", "InsertAbs",
                    MessageBoxButtons.OK, false);

                DropRecordHeadShow();

            }
            catch (Exception ex)
            {
                //if (Lib_Card.Configure.Parameter.Other_Language == 0)
                //    FADM_Form.CustomMessageBox.Show(ex.Message, "BottleCheck",
                //    MessageBoxButtons.OK, false);
                //else
                //{
                //    string s_mes = ex.Message;
                //    if (SmartDyeing.FADM_Object.Communal._dic_warning.ContainsKey(ex.Message))
                //    {
                //        //如果存在就替换英文
                //        s_mes = SmartDyeing.FADM_Object.Communal._dic_warning[ex.Message];
                //    }
                //    FADM_Form.CustomMessageBox.Show(s_mes, "BottleCheck",
                //    MessageBoxButtons.OK, false);
                //}
            }
        }

        private void BtnBottleCheckPause_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgv_DropRecord.CurrentRow != null)
                {
                    if (dgv_DropRecord.SelectedRows.Count > 0)
                    {
                        string s_Date = dgv_DropRecord.CurrentRow.Cells[1].Value.ToString();
                        FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from abs_wait_list where InsertDate = '"+ s_Date+"'");
                        DropRecordHeadShow();
                    }
                }
            }
            catch (Exception ex) { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DropRecordHeadShow();
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




                s_sql = "SELECT BottleNum,CONVERT(varchar, InsertDate, 121) FROM abs_wait_list order by InsertDate;";
                dt_data = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);


                //捆绑
                string s_r;
                int i_success = 0;
                int i_fails = 0;
                for (int i = 0; i < dt_data.Rows.Count; i++)
                {
                    dgv_DropRecord.Rows.Add(dt_data.Rows[i][0].ToString(), dt_data.Rows[i][1].ToString());
                    
                }


            }
            catch (Exception ex)
            {
                FADM_Form.CustomMessageBox.Show(ex.Message, "DropRecordHeadShow", MessageBoxButtons.OK, true);
            }
        }

        private void ShowHeader()
        {
           


            dgv_DropRecord.Columns.Add("", "");
            dgv_DropRecord.Columns.Add("", "");
            if (Lib_Card.Configure.Parameter.Other_Language == 0)
            {
                //设置标题文字
                dgv_DropRecord.Columns[0].HeaderCell.Value = "瓶号";
                dgv_DropRecord.Columns[1].HeaderCell.Value = "时间";
                //设置标题字体
                dgv_DropRecord.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 14.25F);
                //设置内容字体
                dgv_DropRecord.RowsDefaultCellStyle.Font = new Font("宋体", 12.5F);
            }
            else
            {
                dgv_DropRecord.Columns[0].HeaderCell.Value = "BottleNum";
                dgv_DropRecord.Columns[1].HeaderCell.Value = "Date";
                //设置标题字体
                dgv_DropRecord.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 7.5F);
                //设置内容字体
                dgv_DropRecord.RowsDefaultCellStyle.Font = new Font("宋体", 10.5F);
            }


            //设置标题宽度
            dgv_DropRecord.Columns[0].Width = 120;
            dgv_DropRecord.Columns[1].Width = 300;


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

        private void timer1_Tick(object sender, EventArgs e)
        {
            DropRecordHeadShow();
        }
    }
}
