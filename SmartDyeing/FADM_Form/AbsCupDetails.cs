using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartDyeing.FADM_Form
{
    public partial class AbsCupDetails : Form
    {
        private int iCupNo;
        public AbsCupDetails(int iCupNo)
        {
            InitializeComponent();
            this.iCupNo = iCupNo;
            
        }

        private void DripCupDetails_Load(object sender, EventArgs e)
        {
            DataTable dt_data = new DataTable();
            DataRow dr;
            if (Lib_Card.Configure.Parameter.Other_Language == 0)
            {
                //建立Column例，可以指明例的类型,这里用的是默认的string
                dt_data.Columns.Add(new DataColumn("工艺"));
                dt_data.Columns.Add(new DataColumn("是否完成"));
                dt_data.Columns.Add(new DataColumn("开始时间"));
                dt_data.Columns.Add(new DataColumn("结束时间"));

                //设置标题字体
                dgv_CupDetails.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 14.25F);

                //设置内容字体
                dgv_CupDetails.RowsDefaultCellStyle.Font = new Font("宋体", 14.25F);

            }
            else
            {
                dt_data.Columns.Add(new DataColumn("Type"));
                dt_data.Columns.Add(new DataColumn("Finish"));
                dt_data.Columns.Add(new DataColumn("StartTime"));
                dt_data.Columns.Add(new DataColumn("EndTime"));

                //设置标题字体
                dgv_CupDetails.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 10.5F);

                //设置内容字体
                dgv_CupDetails.RowsDefaultCellStyle.Font = new Font("宋体", 10.5F);
            }
            //获取要显示的行数

            //设置标题居中显示
            dgv_CupDetails.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            //设置标题字体
            dgv_CupDetails.ColumnHeadersDefaultCellStyle.Font = new Font("宋体", 14.25F);

            //设置内容居中显示
            dgv_CupDetails.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            //设置内容字体
            dgv_CupDetails.RowsDefaultCellStyle.Font = new Font("宋体", 14.25F);

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

            string s_sql1 = "SELECT * FROM abs_cup_details WHERE" +
                        " CupNum = '" + this.iCupNo +
                        "'  ;";

            DataTable dt_data12 = FADM_Object.Communal._fadmSqlserver.GetData(s_sql1);
            if (dt_data12.Rows.Count > 0)
            {
                txt_CupNum.Text = dt_data12.Rows[0]["BottleNum"].ToString();
                textBox1.Text = dt_data12.Rows[0]["SampleDosage"].ToString();
                textBox2.Text = dt_data12.Rows[0]["RealSampleDosage"].ToString();
                textBox4.Text = dt_data12.Rows[0]["AdditivesDosage"].ToString();
                textBox3.Text = dt_data12.Rows[0]["RealAdditivesDosage"].ToString();
            }

            //获取当前批次当前杯号信息
            string s_sql = "SELECT * FROM Abs_details WHERE" +
                        " CupNum = '" + this.iCupNo +
                        "'  ORDER BY StepNum;";

            DataTable dt_data1 = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

            int i_line = dt_data1.Rows.Count;
            if (Lib_Card.Configure.Parameter.Other_Language == 0)
            {
                for (int i = 1; i <= i_line; i++)
                {
                    dr = dt_data.NewRow();
                    dr[0] = dt_data1.Rows[i - 1]["TechnologyName"];
                    if (dt_data1.Rows[i - 1]["Finish"].ToString() == "1")
                    {
                        dr[1] = "是";
                    }
                    else 
                    {
                        dr[1] = "否";
                    }
                    dr[2] = dt_data1.Rows[i - 1]["StartTime"];
                    dr[3] = dt_data1.Rows[i - 1]["FinishTime"];
                    dt_data.Rows.Add(dr);
                }
            }
            else
            {
                for (int i = 1; i <= i_line; i++)
                {
                    dr = dt_data.NewRow();
                    dr[0] = dt_data1.Rows[i - 1]["TechnologyName"];
                    if (dt_data1.Rows[i - 1]["Finish"].ToString() == "1")
                    {
                        dr[1] = "Yes";
                    }
                    else
                    {
                        dr[1] = "No";
                    }
                    dr[2] = dt_data1.Rows[i - 1]["StartTime"];
                    dr[3] = dt_data1.Rows[i - 1]["FinishTime"];
                    dt_data.Rows.Add(dr);
                }
            }

            ////获取当前批次当前杯号信息
            //P_str_sql = "SELECT * FROM dye_details WHERE" +
            //            " CupNum = '" + this.iCupNo +
            //            "' AND BottleNum > 0 AND ( BottleNum <= " + P_int_maxbottle + "" +
            //            " ) ORDER BY StepNum;";

            //_dt_data = FADM_Object.Communal._fadmSqlserver.GetData(P_str_sql);

            //int Line1 = _dt_data.Rows.Count;
            //for (int i = 1; i <= Line1; i++)
            //{
            //    dr = dt.NewRow();
            //    dr[0] = _dt_data.Rows[i - 1]["Code"];
            //    dr[1] = _dt_data.Rows[i - 1]["BottleNum"];
            //    dr[2] = _dt_data.Rows[i - 1]["FormulaDosage"];
            //    dr[3] = _dt_data.Rows[i - 1]["ObjectDropWeight"];
            //    dr[4] = _dt_data.Rows[i - 1]["RealDropWeight"];
            //    dt.Rows.Add(dr);
            //}

            //捆绑
            dgv_CupDetails.DataSource = new DataView(dt_data);
            //预留滚动条空间
            int i_headspace = 1;
            if (i_line > 11)
            {
                i_headspace = 5;
            }
            for (int i = 0; i < 4; i++)
            {
                dgv_CupDetails.Columns[i].Width = dgv_CupDetails.Width / 4 - i_headspace;
            }
            dgv_CupDetails.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv_CupDetails.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

        }
    }
}
