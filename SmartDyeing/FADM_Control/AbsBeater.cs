using SmartDyeing.FADM_Auto;
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

namespace SmartDyeing.FADM_Control
{
    public partial class AbsBeater : UserControl
    {
        private SmartDyeing.FADM_Control.Cup _cup = null;

        public AbsBeater()
        {
            InitializeComponent();

            cup1.MouseDown += Cup_MouseDown;
            cup1.ContextMenuStrip = this.contextMenuStrip1;

            cup2.MouseDown += Cup_MouseDown;
            cup2.ContextMenuStrip = this.contextMenuStrip1;

            cup3.MouseDown += Cup_MouseDown;
            cup3.ContextMenuStrip = this.contextMenuStrip1;

            cup4.MouseDown += Cup_MouseDown;
            cup4.ContextMenuStrip = this.contextMenuStrip1;
        }

        void Cup_MouseDown(object sender, MouseEventArgs e)
        {
            _cup = (SmartDyeing.FADM_Control.Cup)sender;
        }

        private void tsm_Online_Click(object sender, EventArgs e)
        {
            string s_sql1 = "update abs_cup_details set Enable=1,Statues = '待机',IsUsing = 0  where CupNum = " + _cup.NO + " and Statues ='下线'; ";
            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql1);
        }

        private void tsm_Offline_Click(object sender, EventArgs e)
        {
            string s_sql1 = "UPDATE abs_cup_details SET IsUsing = 0, Statues = '下线',Enable=0 " +
                " WHERE CupNum = " + _cup.NO + " AND Statues = '待机'  and IsUsing = 0;";
            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql1);
        }

        private void tsm_Stop_Click(object sender, EventArgs e)
        {
            string s_sql1 = "SELECT *  FROM abs_cup_details where CupNum = " + _cup.NO + ";";

            DataTable dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql1);
            if (dt_temp.Rows.Count > 0)
            {
                //if (dt_temp.Rows[0]["Statues"].ToString() == "加水" || dt_temp.Rows[0]["Statues"].ToString() == "加药")
                    //FADM_Object.Communal._b_stop = true;
            }

            if (FADM_Object.Communal._b_absErr)
                return;

            Thread thread = new Thread(() =>
            {
                //等待没有交互时再发送停止
                //while (true)
                //{
                //    if (MyAbsorbance._abs_Temps[Convert.ToInt32(_cup.NO) - 1]._s_request == "0")
                //    {
                //        break;
                //    }
                //}

                //先发一个停止，再发一个洗杯
                int[] values1 = new int[1];
                values1[0] = 2;
                if (Convert.ToInt32(_cup.NO) == 1)
                    FADM_Object.Communal._tcpModBusAbs.Write(13088, values1);
                else if (Convert.ToInt32(_cup.NO) == 2)
                    FADM_Object.Communal._tcpModBusAbs.Write(13098, values1);
                else if (Convert.ToInt32(_cup.NO) == 3)
                    FADM_Object.Communal._tcpModBusAbs.Write(13108, values1);
                else if (Convert.ToInt32(_cup.NO) == 4)
                    FADM_Object.Communal._tcpModBusAbs.Write(13118, values1);

                //判断待机后再发洗杯
                while (true)
                {
                    if (MyAbsorbance._abs_Temps[Convert.ToInt32(_cup.NO) - 1]._s_currentState == "1")
                        break;
                }

                string s_sql = "UPDATE abs_cup_details SET Statues='洗杯',IsUsing = 1,Type=0  WHERE CupNum = " + _cup.NO + " ;";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                SmartDyeing.FADM_Auto.MyAbsorbance.Generate(2, Convert.ToInt32(_cup.NO));
                SmartDyeing.FADM_Auto.MyAbsorbance.SendData(Convert.ToInt32(_cup.NO));



                
            });
            thread.Start();
            
        }

        private void tsm_Reset_Click(object sender, EventArgs e)
        {
            string s_sql1 = "update abs_cup_details set Enable=1,IsUsing = 0,Statues = '待机',Cooperate=0  where CupNum = " + _cup.NO + " ; ";
            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql1);

            s_sql1 = "Delete from Abs_details  where CupNum = " + _cup.NO + " ; ";
            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql1);
        }

        private void tsm_Wash_Click(object sender, EventArgs e)
        {
            string s_sql1 = "SELECT *  FROM abs_cup_details where CupNum = " + _cup.NO + ";";

            DataTable dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(s_sql1);
            if (dt_temp.Rows.Count > 0)
            {
                //if (dt_temp.Rows[0]["Statues"].ToString() == "加水" || dt_temp.Rows[0]["Statues"].ToString() == "加药")
                //FADM_Object.Communal._b_stop = true;
            }

            if (FADM_Object.Communal._b_absErr)
                return;

            Thread thread = new Thread(() =>
            {
                //等待没有交互时再发送停止
                //while (true)
                //{
                //    if (MyAbsorbance._abs_Temps[Convert.ToInt32(_cup.NO) - 1]._s_request == "0")
                //    {
                //        break;
                //    }
                //}

                //先发一个停止，再发一个洗杯
                int[] values1 = new int[1];
                values1[0] = 2;
                if (Convert.ToInt32(_cup.NO) == 1)
                    FADM_Object.Communal._tcpModBusAbs.Write(13088, values1);
                else if (Convert.ToInt32(_cup.NO) == 2)
                    FADM_Object.Communal._tcpModBusAbs.Write(13098, values1);
                else if (Convert.ToInt32(_cup.NO) == 3)
                    FADM_Object.Communal._tcpModBusAbs.Write(13108, values1);
                else if (Convert.ToInt32(_cup.NO) == 4)
                    FADM_Object.Communal._tcpModBusAbs.Write(13118, values1);

                //判断待机后再发洗杯
                while (true)
                {
                    if (MyAbsorbance._abs_Temps[Convert.ToInt32(_cup.NO) - 1]._s_currentState == "1")
                        break;
                }

                string s_sql = "UPDATE abs_cup_details SET Statues='溶解剂洗杯',IsUsing = 1,Type=0  WHERE CupNum = " + _cup.NO + " ;";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                SmartDyeing.FADM_Auto.MyAbsorbance.Generate(4, Convert.ToInt32(_cup.NO));
                SmartDyeing.FADM_Auto.MyAbsorbance.SendData(Convert.ToInt32(_cup.NO));




            });
            thread.Start();
        }
    }
}
