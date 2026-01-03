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
    public partial class EightBeater : UserControl
    {
        private Cup _cup = null;

        public EightBeater()
        {
            InitializeComponent();

            foreach (Control c in this.groupBox1.Controls)
            {
                if (c is Cup)
                {
                    c.MouseDown += Cup_MouseDown;
                    c.ContextMenuStrip = this.contextMenuStrip1;
                }
            }
        }

        //输入检查
        void Cup_MouseDown(object sender, MouseEventArgs e)
        {
            _cup = (Cup)sender;
        }

        private void tsm_Online_Click(object sender, EventArgs e)
        {
            string s_sql1 = "update cup_details set Enable=1,Statues = '待机' where CupNum = " + _cup.NO + " and Statues ='下线'; ";
            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql1);

            if (Lib_Card.Configure.Parameter.Other_Language == 0)
            {
                string s_min, s_max;
                if (this.groupBox1.Text.Contains("一号"))
                {
                    s_min = Lib_Card.Configure.Parameter.Machine_Area1_CupMin.ToString();
                    s_max = Lib_Card.Configure.Parameter.Machine_Area1_CupMax.ToString();
                }
                else if (this.groupBox1.Text.Contains("二号"))
                {
                    s_min = Lib_Card.Configure.Parameter.Machine_Area2_CupMin.ToString();
                    s_max = Lib_Card.Configure.Parameter.Machine_Area2_CupMax.ToString();
                }
                else if (this.groupBox1.Text.Contains("三号"))
                {
                    s_min = Lib_Card.Configure.Parameter.Machine_Area3_CupMin.ToString();
                    s_max = Lib_Card.Configure.Parameter.Machine_Area3_CupMax.ToString();
                }
                else if (this.groupBox1.Text.Contains("四号"))
                {
                    s_min = Lib_Card.Configure.Parameter.Machine_Area4_CupMin.ToString();
                    s_max = Lib_Card.Configure.Parameter.Machine_Area4_CupMax.ToString();
                }
                else if (this.groupBox1.Text.Contains("五号"))
                {
                    s_min = Lib_Card.Configure.Parameter.Machine_Area5_CupMin.ToString();
                    s_max = Lib_Card.Configure.Parameter.Machine_Area5_CupMax.ToString();
                }
                else
                {
                    s_min = Lib_Card.Configure.Parameter.Machine_Area6_CupMin.ToString();
                    s_max = Lib_Card.Configure.Parameter.Machine_Area6_CupMax.ToString();
                }

                if (this.groupBox1.Text.Contains("一号"))
                {
                    FADM_Object.Communal._ia_dyeStatus[0] = 1;
                }
                else if (this.groupBox1.Text.Contains("二号"))
                {
                    FADM_Object.Communal._ia_dyeStatus[1] = 1;
                }
                else if (this.groupBox1.Text.Contains("三号"))
                {
                    FADM_Object.Communal._ia_dyeStatus[2] = 1;
                }
                else if (this.groupBox1.Text.Contains("四号"))
                {
                    FADM_Object.Communal._ia_dyeStatus[3] = 1;
                }
                else if (this.groupBox1.Text.Contains("五号"))
                {
                    FADM_Object.Communal._ia_dyeStatus[4] = 1;
                }
                else
                {
                    FADM_Object.Communal._ia_dyeStatus[5] = 1;
                }
            }
            else
            {
                string s_min, s_max;
                if (this.groupBox1.Text.Contains("No.1"))
                {
                    s_min = Lib_Card.Configure.Parameter.Machine_Area1_CupMin.ToString();
                    s_max = Lib_Card.Configure.Parameter.Machine_Area1_CupMax.ToString();
                }
                else if (this.groupBox1.Text.Contains("No.2"))
                {
                    s_min = Lib_Card.Configure.Parameter.Machine_Area2_CupMin.ToString();
                    s_max = Lib_Card.Configure.Parameter.Machine_Area2_CupMax.ToString();
                }
                else if (this.groupBox1.Text.Contains("No.3"))
                {
                    s_min = Lib_Card.Configure.Parameter.Machine_Area3_CupMin.ToString();
                    s_max = Lib_Card.Configure.Parameter.Machine_Area3_CupMax.ToString();
                }
                else if (this.groupBox1.Text.Contains("No.4"))
                {
                    s_min = Lib_Card.Configure.Parameter.Machine_Area4_CupMin.ToString();
                    s_max = Lib_Card.Configure.Parameter.Machine_Area4_CupMax.ToString();
                }
                else if (this.groupBox1.Text.Contains("No.5"))
                {
                    s_min = Lib_Card.Configure.Parameter.Machine_Area5_CupMin.ToString();
                    s_max = Lib_Card.Configure.Parameter.Machine_Area5_CupMax.ToString();
                }
                else
                {
                    s_min = Lib_Card.Configure.Parameter.Machine_Area6_CupMin.ToString();
                    s_max = Lib_Card.Configure.Parameter.Machine_Area6_CupMax.ToString();
                }

                if (this.groupBox1.Text.Contains("No.1"))
                {
                    FADM_Object.Communal._ia_dyeStatus[0] = 1;
                }
                else if (this.groupBox1.Text.Contains("No.2"))
                {
                    FADM_Object.Communal._ia_dyeStatus[1] = 1;
                }
                else if (this.groupBox1.Text.Contains("No.3"))
                {
                    FADM_Object.Communal._ia_dyeStatus[2] = 1;
                }
                else if (this.groupBox1.Text.Contains("No.4"))
                {
                    FADM_Object.Communal._ia_dyeStatus[3] = 1;
                }
                else if (this.groupBox1.Text.Contains("No.5"))
                {
                    FADM_Object.Communal._ia_dyeStatus[4] = 1;
                }
                else
                {
                    FADM_Object.Communal._ia_dyeStatus[5] = 1;
                }
            }

        }

        private void tsm_Offline_Click(object sender, EventArgs e)
        {


            string s_sql = "UPDATE cup_details SET FormulaCode = null,  Enable = 0, " +
                "DyeingCode = null, IsUsing = 0, Statues = '下线', " +
                "StartTime = null, SetTemp = null, StepNum = null, TotalWeight = null, " +
                "TotalStep = null, TechnologyName = null, StepStartTime = null, SetTime = null,RecordIndex = 0, Cooperate = 0 WHERE CupNum = " + _cup.NO + " AND Statues = '待机'  and IsUsing = 0;";
            FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

            string s_min, s_max;
            if (this.groupBox1.Text.Contains("一号"))
            {
                s_min = Lib_Card.Configure.Parameter.Machine_Area1_CupMin.ToString();
                s_max = Lib_Card.Configure.Parameter.Machine_Area1_CupMax.ToString();
            }
            else if (this.groupBox1.Text.Contains("二号"))
            {
                s_min = Lib_Card.Configure.Parameter.Machine_Area2_CupMin.ToString();
                s_max = Lib_Card.Configure.Parameter.Machine_Area2_CupMax.ToString();
            }
            else if (this.groupBox1.Text.Contains("三号"))
            {
                s_min = Lib_Card.Configure.Parameter.Machine_Area3_CupMin.ToString();
                s_max = Lib_Card.Configure.Parameter.Machine_Area3_CupMax.ToString();
            }
            else if (this.groupBox1.Text.Contains("四号"))
            {
                s_min = Lib_Card.Configure.Parameter.Machine_Area4_CupMin.ToString();
                s_max = Lib_Card.Configure.Parameter.Machine_Area4_CupMax.ToString();
            }
            else if (this.groupBox1.Text.Contains("五号"))
            {
                s_min = Lib_Card.Configure.Parameter.Machine_Area5_CupMin.ToString();
                s_max = Lib_Card.Configure.Parameter.Machine_Area5_CupMax.ToString();
            }
            else
            {
                s_min = Lib_Card.Configure.Parameter.Machine_Area6_CupMin.ToString();
                s_max = Lib_Card.Configure.Parameter.Machine_Area6_CupMax.ToString();
            }
            DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(
                "SELECT * FROM cup_details WHERE CupNum >= " + s_min + " and CupNum <=" + s_max + ";");

            foreach (DataRow row in dt_data.Rows)
            {
                int iEnable = Convert.ToInt16(row["Enable"]);
                if (1 == iEnable)
                {
                    return;
                }
            }

            if (this.groupBox1.Text.Contains("一号"))
            {
                FADM_Object.Communal._ia_dyeStatus[0] = 0;

                FADM_Object.Communal._tcpDyeHMI1._b_isSendCoverStatus1 = false;
                FADM_Object.Communal._tcpDyeHMI1._b_isSendCoverStatus2 = false;
                FADM_Object.Communal._tcpDyeHMI1._b_isSendCoverStatus3 = false;
                FADM_Object.Communal._tcpDyeHMI1._b_isSendCoverStatus4 = false;
                FADM_Object.Communal._tcpDyeHMI1._b_isSendCoverStatus5 = false;
                FADM_Object.Communal._tcpDyeHMI1._b_isSendCoverStatus6 = false;
                FADM_Object.Communal._tcpDyeHMI1._b_isSendCoverStatus7 = false;
                FADM_Object.Communal._tcpDyeHMI1._b_isSendCoverStatus8 = false;
                FADM_Object.Communal._tcpDyeHMI1._b_isSendCoverStatus9 = false;
                FADM_Object.Communal._tcpDyeHMI1._b_isSendCoverStatus10 = false;
                FADM_Object.Communal._tcpDyeHMI1._b_isSendCoverStatus11 = false;
                FADM_Object.Communal._tcpDyeHMI1._b_isSendCoverStatus12 = false;

                for (int pp = 0; pp < FADM_Object.Communal._tcpDyeHMI1._b_isSendCoverStatus.Length; pp++)
                {
                    FADM_Object.Communal._tcpDyeHMI1._b_isSendCoverStatus[pp] = false;
                }

                FADM_Object.Communal._tcpDyeHMI1._b_isGetVer = false;
                FADM_Object.Communal._s_TouchVer1 = "";
                FADM_Object.Communal._s_CardOneVer1 = "";
                FADM_Object.Communal._s_CardTwoVer1 = "";
            }
            else if (this.groupBox1.Text.Contains("二号"))
            {
                FADM_Object.Communal._ia_dyeStatus[1] = 0;

                FADM_Object.Communal._tcpDyeHMI2._b_isSendCoverStatus1 = false;
                FADM_Object.Communal._tcpDyeHMI2._b_isSendCoverStatus2 = false;
                FADM_Object.Communal._tcpDyeHMI2._b_isSendCoverStatus3 = false;
                FADM_Object.Communal._tcpDyeHMI2._b_isSendCoverStatus4 = false;
                FADM_Object.Communal._tcpDyeHMI2._b_isSendCoverStatus5 = false;
                FADM_Object.Communal._tcpDyeHMI2._b_isSendCoverStatus6 = false;
                FADM_Object.Communal._tcpDyeHMI2._b_isSendCoverStatus7 = false;
                FADM_Object.Communal._tcpDyeHMI2._b_isSendCoverStatus8 = false;
                FADM_Object.Communal._tcpDyeHMI2._b_isSendCoverStatus9 = false;
                FADM_Object.Communal._tcpDyeHMI2._b_isSendCoverStatus10 = false;
                FADM_Object.Communal._tcpDyeHMI2._b_isSendCoverStatus11 = false;
                FADM_Object.Communal._tcpDyeHMI2._b_isSendCoverStatus12 = false;

                for (int pp = 0; pp < FADM_Object.Communal._tcpDyeHMI2._b_isSendCoverStatus.Length; pp++)
                {
                    FADM_Object.Communal._tcpDyeHMI2._b_isSendCoverStatus[pp] = false;
                }

                FADM_Object.Communal._tcpDyeHMI2._b_isGetVer = false;
                FADM_Object.Communal._s_TouchVer2 = "";
                FADM_Object.Communal._s_CardOneVer2 = "";
                FADM_Object.Communal._s_CardTwoVer2 = "";
            }
            else if (this.groupBox1.Text.Contains("三号"))
            {
                FADM_Object.Communal._ia_dyeStatus[2] = 0;

                FADM_Object.Communal._tcpDyeHMI3._b_isSendCoverStatus1 = false;
                FADM_Object.Communal._tcpDyeHMI3._b_isSendCoverStatus2 = false;
                FADM_Object.Communal._tcpDyeHMI3._b_isSendCoverStatus3 = false;
                FADM_Object.Communal._tcpDyeHMI3._b_isSendCoverStatus4 = false;
                FADM_Object.Communal._tcpDyeHMI3._b_isSendCoverStatus5 = false;
                FADM_Object.Communal._tcpDyeHMI3._b_isSendCoverStatus6 = false;
                FADM_Object.Communal._tcpDyeHMI3._b_isSendCoverStatus7 = false;
                FADM_Object.Communal._tcpDyeHMI3._b_isSendCoverStatus8 = false;
                FADM_Object.Communal._tcpDyeHMI3._b_isSendCoverStatus9 = false;
                FADM_Object.Communal._tcpDyeHMI3._b_isSendCoverStatus10 = false;
                FADM_Object.Communal._tcpDyeHMI3._b_isSendCoverStatus11 = false;
                FADM_Object.Communal._tcpDyeHMI3._b_isSendCoverStatus12 = false;

                for (int pp = 0; pp < FADM_Object.Communal._tcpDyeHMI3._b_isSendCoverStatus.Length; pp++)
                {
                    FADM_Object.Communal._tcpDyeHMI3._b_isSendCoverStatus[pp] = false;
                }

                FADM_Object.Communal._tcpDyeHMI3._b_isGetVer = false;
                FADM_Object.Communal._s_TouchVer3 = "";
                FADM_Object.Communal._s_CardOneVer3 = "";
                FADM_Object.Communal._s_CardTwoVer3 = "";
            }
            else if (this.groupBox1.Text.Contains("四号"))
            {
                FADM_Object.Communal._ia_dyeStatus[3] = 0;

                FADM_Object.Communal._tcpDyeHMI4._b_isSendCoverStatus1 = false;
                FADM_Object.Communal._tcpDyeHMI4._b_isSendCoverStatus2 = false;
                FADM_Object.Communal._tcpDyeHMI4._b_isSendCoverStatus3 = false;
                FADM_Object.Communal._tcpDyeHMI4._b_isSendCoverStatus4 = false;
                FADM_Object.Communal._tcpDyeHMI4._b_isSendCoverStatus5 = false;
                FADM_Object.Communal._tcpDyeHMI4._b_isSendCoverStatus6 = false;
                FADM_Object.Communal._tcpDyeHMI4._b_isSendCoverStatus7 = false;
                FADM_Object.Communal._tcpDyeHMI4._b_isSendCoverStatus8 = false;
                FADM_Object.Communal._tcpDyeHMI4._b_isSendCoverStatus9 = false;
                FADM_Object.Communal._tcpDyeHMI4._b_isSendCoverStatus10 = false;
                FADM_Object.Communal._tcpDyeHMI4._b_isSendCoverStatus11 = false;
                FADM_Object.Communal._tcpDyeHMI4._b_isSendCoverStatus12 = false;

                for (int pp = 0; pp < FADM_Object.Communal._tcpDyeHMI4._b_isSendCoverStatus.Length; pp++)
                {
                    FADM_Object.Communal._tcpDyeHMI4._b_isSendCoverStatus[pp] = false;
                }

                FADM_Object.Communal._tcpDyeHMI4._b_isGetVer = false;
                FADM_Object.Communal._s_TouchVer4 = "";
                FADM_Object.Communal._s_CardOneVer4 = "";
                FADM_Object.Communal._s_CardTwoVer4 = "";
            }
            else if (this.groupBox1.Text.Contains("五号"))
            {
                FADM_Object.Communal._ia_dyeStatus[4] = 0;

                FADM_Object.Communal._tcpDyeHMI5._b_isSendCoverStatus1 = false;
                FADM_Object.Communal._tcpDyeHMI5._b_isSendCoverStatus2 = false;
                FADM_Object.Communal._tcpDyeHMI5._b_isSendCoverStatus3 = false;
                FADM_Object.Communal._tcpDyeHMI5._b_isSendCoverStatus4 = false;
                FADM_Object.Communal._tcpDyeHMI5._b_isSendCoverStatus5 = false;
                FADM_Object.Communal._tcpDyeHMI5._b_isSendCoverStatus6 = false;
                FADM_Object.Communal._tcpDyeHMI5._b_isSendCoverStatus7 = false;
                FADM_Object.Communal._tcpDyeHMI5._b_isSendCoverStatus8 = false;
                FADM_Object.Communal._tcpDyeHMI5._b_isSendCoverStatus9 = false;
                FADM_Object.Communal._tcpDyeHMI5._b_isSendCoverStatus10 = false;
                FADM_Object.Communal._tcpDyeHMI5._b_isSendCoverStatus11 = false;
                FADM_Object.Communal._tcpDyeHMI5._b_isSendCoverStatus12 = false;

                for (int pp = 0; pp < FADM_Object.Communal._tcpDyeHMI5._b_isSendCoverStatus.Length; pp++)
                {
                    FADM_Object.Communal._tcpDyeHMI5._b_isSendCoverStatus[pp] = false;
                }

                FADM_Object.Communal._tcpDyeHMI5._b_isGetVer = false;
                FADM_Object.Communal._s_TouchVer5 = "";
                FADM_Object.Communal._s_CardOneVer5 = "";
                FADM_Object.Communal._s_CardTwoVer5 = "";
            }
            else
            {
                FADM_Object.Communal._ia_dyeStatus[5] = 0;

                FADM_Object.Communal._tcpDyeHMI6._b_isSendCoverStatus1 = false;
                FADM_Object.Communal._tcpDyeHMI6._b_isSendCoverStatus2 = false;
                FADM_Object.Communal._tcpDyeHMI6._b_isSendCoverStatus3 = false;
                FADM_Object.Communal._tcpDyeHMI6._b_isSendCoverStatus4 = false;
                FADM_Object.Communal._tcpDyeHMI6._b_isSendCoverStatus5 = false;
                FADM_Object.Communal._tcpDyeHMI6._b_isSendCoverStatus6 = false;
                FADM_Object.Communal._tcpDyeHMI6._b_isSendCoverStatus7 = false;
                FADM_Object.Communal._tcpDyeHMI6._b_isSendCoverStatus8 = false;
                FADM_Object.Communal._tcpDyeHMI6._b_isSendCoverStatus9 = false;
                FADM_Object.Communal._tcpDyeHMI6._b_isSendCoverStatus10 = false;
                FADM_Object.Communal._tcpDyeHMI6._b_isSendCoverStatus11 = false;
                FADM_Object.Communal._tcpDyeHMI6._b_isSendCoverStatus12 = false;

                for (int pp = 0; pp < FADM_Object.Communal._tcpDyeHMI6._b_isSendCoverStatus.Length; pp++)
                {
                    FADM_Object.Communal._tcpDyeHMI6._b_isSendCoverStatus[pp] = false;
                }

                FADM_Object.Communal._tcpDyeHMI6._b_isGetVer = false;
                FADM_Object.Communal._s_TouchVer6 = "";
                FADM_Object.Communal._s_CardOneVer6 = "";
                FADM_Object.Communal._s_CardTwoVer6 = "";
            }

            Thread.Sleep(2000);

            if (this.groupBox1.Text.Contains("一号"))
            {
               // FADM_Object.Communal._fadmSqlserver.DeleteSpeechInfo("1号打板机通讯异常");
                Lib_Card.CardObject.DeleteD(FADM_Object.Communal._sa_dyeConFTime[0]);
            }
            else if (this.groupBox1.Text.Contains("二号"))
            {
                //  FADM_Object.Communal._fadmSqlserver.DeleteSpeechInfo("2号打板机通讯异常");
                Lib_Card.CardObject.DeleteD(FADM_Object.Communal._sa_dyeConFTime[1]);
            }
            else if (this.groupBox1.Text.Contains("三号"))
            {
                //FADM_Object.Communal._fadmSqlserver.DeleteSpeechInfo("3号打板机通讯异常");
                Lib_Card.CardObject.DeleteD(FADM_Object.Communal._sa_dyeConFTime[2]);
            }
            else if (this.groupBox1.Text.Contains("四号"))
            {
                //FADM_Object.Communal._fadmSqlserver.DeleteSpeechInfo("4号打板机通讯异常");
                Lib_Card.CardObject.DeleteD(FADM_Object.Communal._sa_dyeConFTime[3]);
            }
            else if (this.groupBox1.Text.Contains("五号"))
            {
                //FADM_Object.Communal._fadmSqlserver.DeleteSpeechInfo("5号打板机通讯异常");
                Lib_Card.CardObject.DeleteD(FADM_Object.Communal._sa_dyeConFTime[4]);
            }
            else
            {
                //FADM_Object.Communal._fadmSqlserver.DeleteSpeechInfo("6号打板机通讯异常");
                Lib_Card.CardObject.DeleteD(FADM_Object.Communal._sa_dyeConFTime[5]);
            }



        }

        private void tsm_AllOnline_Click(object sender, EventArgs e)
        {
            string s_min, s_max;
            if (this.groupBox1.Text.Contains("一号"))
            {
                s_min = Lib_Card.Configure.Parameter.Machine_Area1_CupMin.ToString();
                s_max = Lib_Card.Configure.Parameter.Machine_Area1_CupMax.ToString();
            }
            else if (this.groupBox1.Text.Contains("二号"))
            {
                s_min = Lib_Card.Configure.Parameter.Machine_Area2_CupMin.ToString();
                s_max = Lib_Card.Configure.Parameter.Machine_Area2_CupMax.ToString();
            }
            else if (this.groupBox1.Text.Contains("三号"))
            {
                s_min = Lib_Card.Configure.Parameter.Machine_Area3_CupMin.ToString();
                s_max = Lib_Card.Configure.Parameter.Machine_Area3_CupMax.ToString();
            }
            else if (this.groupBox1.Text.Contains("四号"))
            {
                s_min = Lib_Card.Configure.Parameter.Machine_Area4_CupMin.ToString();
                s_max = Lib_Card.Configure.Parameter.Machine_Area4_CupMax.ToString();
            }
            else if (this.groupBox1.Text.Contains("五号"))
            {
                s_min = Lib_Card.Configure.Parameter.Machine_Area5_CupMin.ToString();
                s_max = Lib_Card.Configure.Parameter.Machine_Area5_CupMax.ToString();
            }
            else
            {
                s_min = Lib_Card.Configure.Parameter.Machine_Area6_CupMin.ToString();
                s_max = Lib_Card.Configure.Parameter.Machine_Area6_CupMax.ToString();
            }

            if (this.groupBox1.Text.Contains("一号"))
            {
                string s_sql = "update cup_details set Enable=1,Statues = '待机' where CupNum >= " + s_min + " and CupNum <= " + s_max + " and Statues ='下线'; ";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                FADM_Object.Communal._ia_dyeStatus[0] = 1;
            }
            else if (this.groupBox1.Text.Contains("二号"))
            {
                string s_sql = "update cup_details set Enable=1,Statues = '待机' where CupNum >= " + s_min + " and CupNum <= " + s_max + " and Statues ='下线'; ";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                FADM_Object.Communal._ia_dyeStatus[1] = 1;
            }
            else if (this.groupBox1.Text.Contains("三号"))
            {
                string s_sql = "update cup_details set Enable=1,Statues = '待机' where CupNum >= " + s_min + " and CupNum <= " + s_max + " and Statues ='下线'; ";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                FADM_Object.Communal._ia_dyeStatus[2] = 1;
            }
            else if (this.groupBox1.Text.Contains("四号"))
            {
                string s_sql = "update cup_details set Enable=1,Statues = '待机' where CupNum >= " + s_min + " and CupNum <= " + s_max + " and Statues ='下线'; ";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                FADM_Object.Communal._ia_dyeStatus[3] = 1;
            }
            else if (this.groupBox1.Text.Contains("五号"))
            {
                string s_sql = "update cup_details set Enable=1,Statues = '待机' where CupNum >= " + s_min + " and CupNum <= " + s_max + " and Statues ='下线'; ";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                FADM_Object.Communal._ia_dyeStatus[4] = 1;
            }
            else
            {
                string s_sql = "update cup_details set Enable=1,Statues = '待机' where CupNum >= " + s_min + " and CupNum <= " + s_max + " and Statues ='下线'; ";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                FADM_Object.Communal._ia_dyeStatus[5] = 1;
            }
        }

        private void tsm_AllOffline_Click(object sender, EventArgs e)
        {
            string s_min, s_max;
            if (this.groupBox1.Text.Contains("一号"))
            {
                s_min = Lib_Card.Configure.Parameter.Machine_Area1_CupMin.ToString();
                s_max = Lib_Card.Configure.Parameter.Machine_Area1_CupMax.ToString();
            }
            else if (this.groupBox1.Text.Contains("二号"))
            {
                s_min = Lib_Card.Configure.Parameter.Machine_Area2_CupMin.ToString();
                s_max = Lib_Card.Configure.Parameter.Machine_Area2_CupMax.ToString();
            }
            else if (this.groupBox1.Text.Contains("三号"))
            {
                s_min = Lib_Card.Configure.Parameter.Machine_Area3_CupMin.ToString();
                s_max = Lib_Card.Configure.Parameter.Machine_Area3_CupMax.ToString();
            }
            else if (this.groupBox1.Text.Contains("四号"))
            {
                s_min = Lib_Card.Configure.Parameter.Machine_Area4_CupMin.ToString();
                s_max = Lib_Card.Configure.Parameter.Machine_Area4_CupMax.ToString();
            }
            else if (this.groupBox1.Text.Contains("五号"))
            {
                s_min = Lib_Card.Configure.Parameter.Machine_Area5_CupMin.ToString();
                s_max = Lib_Card.Configure.Parameter.Machine_Area5_CupMax.ToString();
            }
            else
            {
                s_min = Lib_Card.Configure.Parameter.Machine_Area6_CupMin.ToString();
                s_max = Lib_Card.Configure.Parameter.Machine_Area6_CupMax.ToString();
            }

            if (this.groupBox1.Text.Contains("一号"))
            {

                string s_sql = "UPDATE cup_details SET FormulaCode = null,  Enable = 0, " +
              "DyeingCode = null, IsUsing = 0, Statues = '下线', " +
              "StartTime = null, SetTemp = null, StepNum = null, TotalWeight = null, " +
              "TotalStep = null, TechnologyName = null, StepStartTime = null, SetTime = null,RecordIndex = 0, Cooperate = 0 WHERE CupNum >= " + s_min + " and CupNum <= " + s_max + " and Statues ='待机' and IsUsing = 0;";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(
              "SELECT * FROM cup_details WHERE CupNum >= " + s_min + " and CupNum <=" + s_max + " and Enable = 1;");

                if (dt_data.Rows.Count > 0)
                    return;
                FADM_Object.Communal._ia_dyeStatus[0] = 0;
                Thread.Sleep(2000);
                Lib_Card.CardObject.DeleteD(FADM_Object.Communal._sa_dyeConFTime[0]);

                FADM_Object.Communal._tcpDyeHMI1._b_isSendCoverStatus1 = false;
                FADM_Object.Communal._tcpDyeHMI1._b_isSendCoverStatus2 = false;
                FADM_Object.Communal._tcpDyeHMI1._b_isSendCoverStatus3 = false;
                FADM_Object.Communal._tcpDyeHMI1._b_isSendCoverStatus4 = false;
                FADM_Object.Communal._tcpDyeHMI1._b_isSendCoverStatus5 = false;
                FADM_Object.Communal._tcpDyeHMI1._b_isSendCoverStatus6 = false;
                FADM_Object.Communal._tcpDyeHMI1._b_isSendCoverStatus7 = false;
                FADM_Object.Communal._tcpDyeHMI1._b_isSendCoverStatus8 = false;
                FADM_Object.Communal._tcpDyeHMI1._b_isSendCoverStatus9 = false;
                FADM_Object.Communal._tcpDyeHMI1._b_isSendCoverStatus10 = false;
                FADM_Object.Communal._tcpDyeHMI1._b_isSendCoverStatus11 = false;
                FADM_Object.Communal._tcpDyeHMI1._b_isSendCoverStatus12 = false;

                for (int pp = 0; pp < FADM_Object.Communal._tcpDyeHMI1._b_isSendCoverStatus.Length; pp++)
                {
                    FADM_Object.Communal._tcpDyeHMI1._b_isSendCoverStatus[pp] = false;
                }

                FADM_Object.Communal._tcpDyeHMI1._b_isGetVer = false;
                FADM_Object.Communal._s_TouchVer1 = "";
                FADM_Object.Communal._s_CardOneVer1 = "";
                FADM_Object.Communal._s_CardTwoVer1 = "";
            }
            else if (this.groupBox1.Text.Contains("二号"))
            {
                string s_sql = "UPDATE cup_details SET FormulaCode = null,  Enable = 0, " +
              "DyeingCode = null, IsUsing = 0, Statues = '下线', " +
              "StartTime = null, SetTemp = null, StepNum = null, TotalWeight = null, " +
              "TotalStep = null, TechnologyName = null, StepStartTime = null, SetTime = null,RecordIndex = 0, Cooperate = 0 WHERE CupNum >= " + s_min + " and CupNum <= " + s_max + " and Statues ='待机' and IsUsing = 0;";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(
               "SELECT * FROM cup_details WHERE CupNum >= " + s_min + " and CupNum <=" + s_max + " and Enable = 1;");

                if (dt_data.Rows.Count > 0)
                    return;
                FADM_Object.Communal._ia_dyeStatus[1] = 0;
                Thread.Sleep(2000);
                Lib_Card.CardObject.DeleteD(FADM_Object.Communal._sa_dyeConFTime[1]);

                FADM_Object.Communal._tcpDyeHMI2._b_isSendCoverStatus1 = false;
                FADM_Object.Communal._tcpDyeHMI2._b_isSendCoverStatus2 = false;
                FADM_Object.Communal._tcpDyeHMI2._b_isSendCoverStatus3 = false;
                FADM_Object.Communal._tcpDyeHMI2._b_isSendCoverStatus4 = false;
                FADM_Object.Communal._tcpDyeHMI2._b_isSendCoverStatus5 = false;
                FADM_Object.Communal._tcpDyeHMI2._b_isSendCoverStatus6 = false;
                FADM_Object.Communal._tcpDyeHMI2._b_isSendCoverStatus7 = false;
                FADM_Object.Communal._tcpDyeHMI2._b_isSendCoverStatus8 = false;
                FADM_Object.Communal._tcpDyeHMI2._b_isSendCoverStatus9 = false;
                FADM_Object.Communal._tcpDyeHMI2._b_isSendCoverStatus10 = false;
                FADM_Object.Communal._tcpDyeHMI2._b_isSendCoverStatus11 = false;
                FADM_Object.Communal._tcpDyeHMI2._b_isSendCoverStatus12 = false;

                for (int pp = 0; pp < FADM_Object.Communal._tcpDyeHMI2._b_isSendCoverStatus.Length; pp++)
                {
                    FADM_Object.Communal._tcpDyeHMI2._b_isSendCoverStatus[pp] = false;
                }

                FADM_Object.Communal._tcpDyeHMI2._b_isGetVer = false;
                FADM_Object.Communal._s_TouchVer2 = "";
                FADM_Object.Communal._s_CardOneVer2 = "";
                FADM_Object.Communal._s_CardTwoVer2 = "";
            }
            else if (this.groupBox1.Text.Contains("三号"))
            {
                string s_sql = "UPDATE cup_details SET FormulaCode = null,  Enable = 0, " +
              "DyeingCode = null, IsUsing = 0, Statues = '下线', " +
              "StartTime = null, SetTemp = null, StepNum = null, TotalWeight = null, " +
              "TotalStep = null, TechnologyName = null, StepStartTime = null, SetTime = null,RecordIndex = 0, Cooperate = 0 WHERE CupNum >= " + s_min + " and CupNum <= " + s_max + " and Statues ='待机' and IsUsing = 0;";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(
             "SELECT * FROM cup_details WHERE CupNum >= " + s_min + " and CupNum <=" + s_max + " and Enable = 1;");

                if (dt_data.Rows.Count > 0)
                    return;
                FADM_Object.Communal._ia_dyeStatus[2] = 0;
                Thread.Sleep(2000);
                Lib_Card.CardObject.DeleteD(FADM_Object.Communal._sa_dyeConFTime[2]);

                FADM_Object.Communal._tcpDyeHMI3._b_isSendCoverStatus1 = false;
                FADM_Object.Communal._tcpDyeHMI3._b_isSendCoverStatus2 = false;
                FADM_Object.Communal._tcpDyeHMI3._b_isSendCoverStatus3 = false;
                FADM_Object.Communal._tcpDyeHMI3._b_isSendCoverStatus4 = false;
                FADM_Object.Communal._tcpDyeHMI3._b_isSendCoverStatus5 = false;
                FADM_Object.Communal._tcpDyeHMI3._b_isSendCoverStatus6 = false;
                FADM_Object.Communal._tcpDyeHMI3._b_isSendCoverStatus7 = false;
                FADM_Object.Communal._tcpDyeHMI3._b_isSendCoverStatus8 = false;
                FADM_Object.Communal._tcpDyeHMI3._b_isSendCoverStatus9 = false;
                FADM_Object.Communal._tcpDyeHMI3._b_isSendCoverStatus10 = false;
                FADM_Object.Communal._tcpDyeHMI3._b_isSendCoverStatus11 = false;
                FADM_Object.Communal._tcpDyeHMI3._b_isSendCoverStatus12 = false;

                for (int pp = 0; pp < FADM_Object.Communal._tcpDyeHMI3._b_isSendCoverStatus.Length; pp++)
                {
                    FADM_Object.Communal._tcpDyeHMI3._b_isSendCoverStatus[pp] = false;
                }

                FADM_Object.Communal._tcpDyeHMI3._b_isGetVer = false;
                FADM_Object.Communal._s_TouchVer3 = "";
                FADM_Object.Communal._s_CardOneVer3 = "";
                FADM_Object.Communal._s_CardTwoVer3 = "";
            }
            else if (this.groupBox1.Text.Contains("四号"))
            {
                string s_sql = "UPDATE cup_details SET FormulaCode = null,  Enable = 0, " +
              "DyeingCode = null, IsUsing = 0, Statues = '下线', " +
              "StartTime = null, SetTemp = null, StepNum = null, TotalWeight = null, " +
              "TotalStep = null, TechnologyName = null, StepStartTime = null, SetTime = null,RecordIndex = 0, Cooperate = 0 WHERE CupNum >= " + s_min + " and CupNum <= " + s_max + " and Statues ='待机' and IsUsing = 0;";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(
             "SELECT * FROM cup_details WHERE CupNum >= " + s_min + " and CupNum <=" + s_max + " and Enable = 1;");

                if (dt_data.Rows.Count > 0)
                    return;
                FADM_Object.Communal._ia_dyeStatus[3] = 0;
                Thread.Sleep(2000);
                Lib_Card.CardObject.DeleteD(FADM_Object.Communal._sa_dyeConFTime[3]);

                FADM_Object.Communal._tcpDyeHMI4._b_isSendCoverStatus1 = false;
                FADM_Object.Communal._tcpDyeHMI4._b_isSendCoverStatus2 = false;
                FADM_Object.Communal._tcpDyeHMI4._b_isSendCoverStatus3 = false;
                FADM_Object.Communal._tcpDyeHMI4._b_isSendCoverStatus4 = false;
                FADM_Object.Communal._tcpDyeHMI4._b_isSendCoverStatus5 = false;
                FADM_Object.Communal._tcpDyeHMI4._b_isSendCoverStatus6 = false;
                FADM_Object.Communal._tcpDyeHMI4._b_isSendCoverStatus7 = false;
                FADM_Object.Communal._tcpDyeHMI4._b_isSendCoverStatus8 = false;
                FADM_Object.Communal._tcpDyeHMI4._b_isSendCoverStatus9 = false;
                FADM_Object.Communal._tcpDyeHMI4._b_isSendCoverStatus10 = false;
                FADM_Object.Communal._tcpDyeHMI4._b_isSendCoverStatus11 = false;
                FADM_Object.Communal._tcpDyeHMI4._b_isSendCoverStatus12 = false;

                for (int pp = 0; pp < FADM_Object.Communal._tcpDyeHMI4._b_isSendCoverStatus.Length; pp++)
                {
                    FADM_Object.Communal._tcpDyeHMI4._b_isSendCoverStatus[pp] = false;
                }

                FADM_Object.Communal._tcpDyeHMI4._b_isGetVer = false;
                FADM_Object.Communal._s_TouchVer4 = "";
                FADM_Object.Communal._s_CardOneVer4 = "";
                FADM_Object.Communal._s_CardTwoVer4 = "";
            }
            else if (this.groupBox1.Text.Contains("五号"))
            {
                string s_sql = "UPDATE cup_details SET FormulaCode = null,  Enable = 0, " +
              "DyeingCode = null, IsUsing = 0, Statues = '下线', " +
              "StartTime = null, SetTemp = null, StepNum = null, TotalWeight = null, " +
              "TotalStep = null, TechnologyName = null, StepStartTime = null, SetTime = null,RecordIndex = 0, Cooperate = 0 WHERE CupNum >= " + s_min + " and CupNum <= " + s_max + " and Statues ='待机' and IsUsing = 0;";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(
             "SELECT * FROM cup_details WHERE CupNum >= " + s_min + " and CupNum <=" + s_max + " and Enable = 1;");

                if (dt_data.Rows.Count > 0)
                    return;
                FADM_Object.Communal._ia_dyeStatus[4] = 0;
                Thread.Sleep(2000);
                Lib_Card.CardObject.DeleteD(FADM_Object.Communal._sa_dyeConFTime[4]);

                FADM_Object.Communal._tcpDyeHMI5._b_isSendCoverStatus1 = false;
                FADM_Object.Communal._tcpDyeHMI5._b_isSendCoverStatus2 = false;
                FADM_Object.Communal._tcpDyeHMI5._b_isSendCoverStatus3 = false;
                FADM_Object.Communal._tcpDyeHMI5._b_isSendCoverStatus4 = false;
                FADM_Object.Communal._tcpDyeHMI5._b_isSendCoverStatus5 = false;
                FADM_Object.Communal._tcpDyeHMI5._b_isSendCoverStatus6 = false;
                FADM_Object.Communal._tcpDyeHMI5._b_isSendCoverStatus7 = false;
                FADM_Object.Communal._tcpDyeHMI5._b_isSendCoverStatus8 = false;
                FADM_Object.Communal._tcpDyeHMI5._b_isSendCoverStatus9 = false;
                FADM_Object.Communal._tcpDyeHMI5._b_isSendCoverStatus10 = false;
                FADM_Object.Communal._tcpDyeHMI5._b_isSendCoverStatus11 = false;
                FADM_Object.Communal._tcpDyeHMI5._b_isSendCoverStatus12 = false;

                for (int pp = 0; pp < FADM_Object.Communal._tcpDyeHMI5._b_isSendCoverStatus.Length; pp++)
                {
                    FADM_Object.Communal._tcpDyeHMI5._b_isSendCoverStatus[pp] = false;
                }

                FADM_Object.Communal._tcpDyeHMI5._b_isGetVer = false;
                FADM_Object.Communal._s_TouchVer5 = "";
                FADM_Object.Communal._s_CardOneVer5 = "";
                FADM_Object.Communal._s_CardTwoVer5 = "";
            }
            else 
            {
                string s_sql = "UPDATE cup_details SET FormulaCode = null,  Enable = 0, " +
              "DyeingCode = null, IsUsing = 0, Statues = '下线', " +
              "StartTime = null, SetTemp = null, StepNum = null, TotalWeight = null, " +
              "TotalStep = null, TechnologyName = null, StepStartTime = null, SetTime = null,RecordIndex = 0, Cooperate = 0 WHERE CupNum >= " + s_min + " and CupNum <= " + s_max + " and Statues ='待机' and IsUsing = 0;";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(
             "SELECT * FROM cup_details WHERE CupNum >= " + s_min + " and CupNum <=" + s_max + " and Enable = 1;");

                if (dt_data.Rows.Count > 0)
                    return;
                FADM_Object.Communal._ia_dyeStatus[5] = 0;
                Thread.Sleep(2000);
                Lib_Card.CardObject.DeleteD(FADM_Object.Communal._sa_dyeConFTime[5]);

                FADM_Object.Communal._tcpDyeHMI6._b_isSendCoverStatus1 = false;
                FADM_Object.Communal._tcpDyeHMI6._b_isSendCoverStatus2 = false;
                FADM_Object.Communal._tcpDyeHMI6._b_isSendCoverStatus3 = false;
                FADM_Object.Communal._tcpDyeHMI6._b_isSendCoverStatus4 = false;
                FADM_Object.Communal._tcpDyeHMI6._b_isSendCoverStatus5 = false;
                FADM_Object.Communal._tcpDyeHMI6._b_isSendCoverStatus6 = false;
                FADM_Object.Communal._tcpDyeHMI6._b_isSendCoverStatus7 = false;
                FADM_Object.Communal._tcpDyeHMI6._b_isSendCoverStatus8 = false;
                FADM_Object.Communal._tcpDyeHMI6._b_isSendCoverStatus9 = false;
                FADM_Object.Communal._tcpDyeHMI6._b_isSendCoverStatus10 = false;
                FADM_Object.Communal._tcpDyeHMI6._b_isSendCoverStatus11 = false;
                FADM_Object.Communal._tcpDyeHMI6._b_isSendCoverStatus12 = false;

                for (int pp = 0; pp < FADM_Object.Communal._tcpDyeHMI6._b_isSendCoverStatus.Length; pp++)
                {
                    FADM_Object.Communal._tcpDyeHMI6._b_isSendCoverStatus[pp] = false;
                }

                FADM_Object.Communal._tcpDyeHMI6._b_isGetVer = false;
                FADM_Object.Communal._s_TouchVer6 = "";
                FADM_Object.Communal._s_CardOneVer6 = "";
                FADM_Object.Communal._s_CardTwoVer6 = "";
            }
        }

        private void tsm_Stop_Click(object sender, EventArgs e)
        {
            //DataTable dataTable = FADM_Object.Communal._fadmSqlserver.GetData(
            // "SELECT * FROM drop_head WHERE CupNum = " + _cup.NO + ";");
            //if (dataTable.Rows.Count > 0)
            //{
            DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(
                    "SELECT * FROM cup_details WHERE CupNum = " + _cup.NO + ";");
            string s_status = Convert.ToString(dt_data.Rows[0]["Statues"]);
            if ("下线" != s_status)
            {
                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    if (("滴液" == s_status && FADM_Object.Communal.ReadMachineStatus() != 7) || "滴液" != s_status)
                    {
                        DialogResult dialogResult = FADM_Form.CustomMessageBox.Show(_cup.NO + "号配液杯确定停止吗?(确认停止请点是，取消停止请点否)", "温馨提示", MessageBoxButtons.YesNo, true);
                        if (dialogResult == DialogResult.Yes)
                        {
                            Thread thread = new Thread(StopCup);
                            thread.Start(_cup.NO);
                        }
                    }
                }
                else
                {
                    if (("Drip" == s_status && FADM_Object.Communal.ReadMachineStatus() != 7) || "Drip" != s_status)
                    {
                        DialogResult dialogResult = FADM_Form.CustomMessageBox.Show(_cup.NO + "Are you sure to stop the dispensing cup number " + _cup.NO + " ? (To confirm the stop, please click Yes. To cancel the stop, please click No)", "Tips", MessageBoxButtons.YesNo, true);
                        if (dialogResult == DialogResult.Yes)
                        {
                            Thread thread = new Thread(StopCup);
                            thread.Start(_cup.NO);
                        }
                    }
                }

            }
            //}
        }

        private void StopCup(object oCupNo)
        {
            //while (true)
            //{
            //    DataTable dataTable = FADM_Object.Communal._fadmSqlserver.GetData(
            //        "SELECT * FROM dye_details WHERE CupNum = " + oCupNo + " AND Cooperate != 0");
            //    if (dataTable.Rows.Count == 0)
            //    {
            //        DataTable dataTable1 = FADM_Object.Communal._fadmSqlserver.GetData(
            //             "SELECT * FROM cup_details WHERE CupNum = " + oCupNo + " AND Cooperate != 0");
            //        if (dataTable1.Rows.Count == 0)
            //            break;
            //    }

            //    Thread.Sleep(1);
            //}

            FADM_Object.Communal._lis_dripStopCup.Add(Convert.ToInt16(oCupNo));
        }

        private void tsm_IsFix_Click(object sender, EventArgs e)
        {
            if (tsm_IsFix.Checked)
            {
                string s_sql = "update cup_details set IsFixed=0 where CupNum = " + _cup.NO + " and Statues = '待机'; ";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
            }
            else
            {
                string s_sql = "update cup_details set IsFixed=1 where CupNum = " + _cup.NO + " and Statues = '待机'; ";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(
                "SELECT * FROM cup_details WHERE CupNum = " + _cup.NO + ";");
            try
            {
                if (dt_data.Rows.Count > 0)
                {
                    if (dt_data.Rows[0]["IsFixed"].ToString() == "1")
                    {
                        tsm_IsFix.Checked = true;
                    }
                    else
                    {
                        tsm_IsFix.Checked = false;
                    }
                }

                //判断是否有高温洗杯功能
                if (!FADM_Object.Communal._b_isHighWash)
                {
                    tsm_HighWash.Visible = false;
                }
            }
            catch { }
        }

        private void tsm_ReSend_Click(object sender, EventArgs e)
        {
            DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(
                "SELECT * FROM cup_details WHERE CupNum = " + _cup.NO + ";");

            try
            {
                if (dt_data.Rows.Count > 0)
                {
                    if (dt_data.Rows[0]["Statues"].ToString() == "滴液成功"|| dt_data.Rows[0]["Statues"].ToString() == "滴液失败")
                    {
                        if (!FADM_Object.Communal._lis_dripSuccessCup.Contains(Convert.ToInt32(_cup.NO)))
                        {
                            FADM_Object.Communal._lis_dripSuccessCup.Add(Convert.ToInt32(_cup.NO));
                        }
                    }
                }
            }
            catch { }
        }

        private void tsm_HighWash_Click(object sender, EventArgs e)
        {
            DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(
                    "SELECT * FROM cup_details WHERE CupNum = " + _cup.NO + ";");
            string s_status = Convert.ToString(dt_data.Rows[0]["Statues"]);
            if ("待机" == s_status)
            {
                FADM_Object.Communal._lis_HighWashCup.Add(Convert.ToInt16(_cup.NO));

            }
        }
    }
}
