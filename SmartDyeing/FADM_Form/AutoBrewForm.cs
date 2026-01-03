using Org.BouncyCastle.Utilities.Collections;
using SmartDyeing.FADM_Auto;
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

namespace SmartDyeing.FADM_Form
{
    public partial class AutoBrewForm : Form
    {
        private SmartDyeing.FADM_Control.Cup _cup = null;
        public AutoBrewForm()
        {
            InitializeComponent();

            ShowHeader();
            DropRecordHeadShow();

            cup1.MouseDown += Cup_MouseDown;
            cup1.ContextMenuStrip = this.contextMenuStrip1;

            cup2.MouseDown += Cup_MouseDown;
            cup2.ContextMenuStrip = this.contextMenuStrip1;

            cup3.MouseDown += Cup_MouseDown;
            cup3.ContextMenuStrip = this.contextMenuStrip1;

            cup4.MouseDown += Cup_MouseDown;
            cup4.ContextMenuStrip = this.contextMenuStrip1;

            cup5.MouseDown += Cup_MouseDown;
            cup5.ContextMenuStrip = this.contextMenuStrip1;

            cup6.MouseDown += Cup_MouseDown;
            cup6.ContextMenuStrip = this.contextMenuStrip1;
        }

        void Cup_MouseDown(object sender, MouseEventArgs e)
        {
            _cup = (SmartDyeing.FADM_Control.Cup)sender;
        }

        private void tsm_Online_Click(object sender, EventArgs e)
        {
            string s_sql1 = "update brew_station_details set Statues = '待机',IsUsing = 0  where Station = " + _cup.NO + " and Statues ='下线'; ";
            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql1);
        }

        private void tsm_Offline_Click(object sender, EventArgs e)
        {
            string s_sql = "UPDATE brew_station_details SET IsUsing = 0, Statues = '下线' " +
                " WHERE Station = " + _cup.NO + " AND Statues = '待机'  and IsUsing = 0;";
            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
        }

        private void tsm_Stop_Click(object sender, EventArgs e)
        {
            FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", _cup.NO + "号自动开料工位停止");
            //发送停止，然后重置数据
            Thread thread = new Thread(() =>
            {
                //发送停止
                int[] values = new int[1];
                values[0] = 2;
                if (!FADM_Object.Communal._tcpModBus._b_Connect)
                {
                    FADM_Object.Communal._tcpModBus.ReConnect();
                }

                while(true)
                {
                    if (!Communal._b_SendStopCupNum.Contains(Convert.ToInt32(_cup.NO)))
                        break;
                    Thread.Sleep(100);
                }

                DataTable dt_Six = FADM_Object.Communal._fadmSqlserver.GetData(
             "SELECT top 1 * FROM brew_station_details WHERE  Station = 6 Order By ReceptionTime ;");
                int i_washNum = 0;
                if(dt_Six.Rows.Count>0)
                {
                    i_washNum = Convert.ToInt32(dt_Six.Rows[0]["BrewNum"].ToString());
                }
                int i_brewNum = 0;
                if (AutoBrew._bottle_Wash[0]._i_statues != 1)
                {
                    //查询对应开料工位
                    DataTable dt_cupordye_details_sec = FADM_Object.Communal._fadmSqlserver.GetData(
             "SELECT top 1 * FROM brew_station_details WHERE  Station = 6 Order By ReceptionTime ;");
                    if (dt_cupordye_details_sec.Rows.Count > 0)
                    {
                        i_brewNum = Convert.ToInt32(dt_cupordye_details_sec.Rows[0]["BrewNum"].ToString());
                    }
                }
                //把洗瓶工位也停止了
                if(Convert.ToInt32(_cup.NO) == i_brewNum)
                {
                    FADM_Object.Communal._tcpModBus.Write(22888, values);
                    Thread.Sleep(1000);

                    string s_sql2 = "UPDATE brew_station_details SET Statues='待机',IsUsing = 0,Cooperate=0  WHERE Station = 6 And BrewNum = "+ i_brewNum+";";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql2);

                }
                if (Convert.ToInt32(_cup.NO) == 1)
                    FADM_Object.Communal._tcpModBus.Write(22288, values);
                else if (Convert.ToInt32(_cup.NO) == 2)
                    FADM_Object.Communal._tcpModBus.Write(22328, values);
                else if (Convert.ToInt32(_cup.NO) == 3)
                    FADM_Object.Communal._tcpModBus.Write(22368, values);
                else if (Convert.ToInt32(_cup.NO) == 4)
                    FADM_Object.Communal._tcpModBus.Write(22408, values);
                else if (Convert.ToInt32(_cup.NO) == 5)
                    FADM_Object.Communal._tcpModBus.Write(22448, values);
                else if (Convert.ToInt32(_cup.NO) == 6)
                {
                    if (AutoBrew._bottle_Wash[0]._i_statues != 1)
                    {
                        //查询对应开料工位
                        DataTable dt_cupordye_details_sec = FADM_Object.Communal._fadmSqlserver.GetData(
                 "SELECT top 1 * FROM brew_station_details WHERE  Station = 6 Order By ReceptionTime ;");
                        if (dt_cupordye_details_sec.Rows.Count > 0)
                        {
                            if (Convert.ToInt32(dt_cupordye_details_sec.Rows[0]["BrewNum"].ToString()) == 1)
                                FADM_Object.Communal._tcpModBus.Write(22288, values);
                            else if (Convert.ToInt32(dt_cupordye_details_sec.Rows[0]["BrewNum"].ToString()) == 2)
                                FADM_Object.Communal._tcpModBus.Write(22328, values);
                            else if (Convert.ToInt32(dt_cupordye_details_sec.Rows[0]["BrewNum"].ToString()) == 3)
                                FADM_Object.Communal._tcpModBus.Write(22368, values);
                            else if (Convert.ToInt32(dt_cupordye_details_sec.Rows[0]["BrewNum"].ToString()) == 4)
                                FADM_Object.Communal._tcpModBus.Write(22408, values);
                            else if (Convert.ToInt32(dt_cupordye_details_sec.Rows[0]["BrewNum"].ToString()) == 5)
                                FADM_Object.Communal._tcpModBus.Write(22448, values);

                            Thread.Sleep(1000);

                            string s_sql1 = "UPDATE brew_station_details SET Statues='待机',IsUsing = 0,Cooperate=0  WHERE Station = " + dt_cupordye_details_sec.Rows[0]["BrewNum"].ToString() + " ;";
                            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql1);
                        }
                    }
                    //判断是否正在运行，如果是，要把对应开料工位也要停止
                    FADM_Object.Communal._tcpModBus.Write(22888, values);
                }

                Thread.Sleep(1000);
                if (Convert.ToInt32(_cup.NO) == 6)
                {
                    string s_sql = "UPDATE brew_station_details SET Statues='待机',IsUsing = 0,Cooperate=0  WHERE Station = 6 And BrewNum = " + i_washNum + ";";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                }
                else
                {
                    string s_sql = "UPDATE brew_station_details SET Statues='待机',IsUsing = 0,Cooperate=0  WHERE Station = " + _cup.NO + " ;";
                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                }
            });
            thread.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(
                        "SELECT * FROM brew_station_details order by Station;");
            foreach (DataRow dr1 in dt_data.Rows)
            {
                if (dr1["Station"].ToString() == "1")
                {
                    label1.Text = dr1["Statues"].ToString();
                }
                if (dr1["Station"].ToString() == "2")
                {
                    label2.Text = dr1["Statues"].ToString();
                }
                if (dr1["Station"].ToString() == "3")
                {
                    label3.Text = dr1["Statues"].ToString();
                }
                if (dr1["Station"].ToString() == "4")
                {
                    label4.Text = dr1["Statues"].ToString();
                }
                if (dr1["Station"].ToString() == "5")
                {
                    label5.Text = dr1["Statues"].ToString();
                }
                if (dr1["Station"].ToString() == "6")
                {
                    label6.Text = dr1["Statues"].ToString();
                }
            }
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




                s_sql = "SELECT BottleNum,CONVERT(varchar, InsertDate, 121) FROM Brew_wait_list order by InsertDate;";
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

        private void timer2_Tick(object sender, EventArgs e)
        {
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
                        "INSERT INTO Brew_wait_list(BottleNum, InsertDate,Type) VALUES('" + i + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',0);");
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
                        FADM_Object.Communal._fadmSqlserver.ReviseData("Delete from Brew_wait_list where InsertDate = '" + s_Date + "'");
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
    }
}
