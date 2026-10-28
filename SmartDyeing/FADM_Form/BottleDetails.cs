using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartDyeing.FADM_Form
{
    public partial class BottleDetails : Form
    {


        /// <summary>
        /// 声明瓶号变量
        /// </summary>
        private int _i_bottleNum;

        

        public BottleDetails(int i_bottleNum)
        {
            InitializeComponent();
            _i_bottleNum = i_bottleNum;
        }

        private void BottleDetails_Load(object sender, EventArgs e)
        {
            //获取母液瓶资料
            string s_sql = "SELECT *  FROM bottle_details" +
                               " WHERE BottleNum = " + _i_bottleNum + "  And MachineID = " + FADM_Object.Communal._i_CurrentBrewNum+"; ";
            DataTable dt_bottledetails = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

            //显示母液瓶资料
            if (dt_bottledetails.Rows.Count > 0)
            {
                foreach (DataColumn mDc in dt_bottledetails.Columns)
                {
                    string s_name = "txt_" + mDc.Caption.ToString();
                    foreach (Control c in this.grp_BottleDetails.Controls)
                    {
                        if (c is TextBox && c.Name == s_name)
                        {
                            c.Text = dt_bottledetails.Rows[0][mDc].ToString();
                        }
                    }
                }

                //获取染助剂资料
                s_sql = "SELECT *  FROM assistant_details WHERE" +
                            " AssistantCode = '" + txt_AssistantCode.Text + "' ; ";

                DataTable dt_assistantdetails = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);
                if (dt_assistantdetails.Rows.Count > 0)
                {
                    foreach (DataColumn mDc in dt_assistantdetails.Columns)
                    {
                        string name = "txt_" + mDc.Caption.ToString();
                        foreach (Control c in this.grp_AdjustParameters.Controls)
                        {
                            if (c is TextBox && c.Name == name)
                            {
                                c.Text = dt_assistantdetails.Rows[0][mDc].ToString();
                            }
                        }
                        if (name == "txt_UnitOfAccount")
                        {
                            txt_UnitOfAccount.Text = dt_assistantdetails.Rows[0][mDc].ToString();
                        }
                    }
                }

                if(txt_TermOfValidity.Text != "" && txt_BrewingData.Text != "")
                {
                    DateTime dateTime1 = Convert.ToDateTime(txt_BrewingData.Text);
                    DateTime dateTime2 = DateTime.Now;
                    TimeSpan ts = dateTime2 - dateTime1;
                    TimeSpan ts1 = TimeSpan.FromHours(Convert.ToInt32(txt_TermOfValidity.Text)); // 
                    if (ts > ts1)
                    {
                        //已过期
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        {
                            txt_LeftTime.Text = "已过期";
                        }
                        else
                        {
                            txt_LeftTime.Text = "Expired";
                        }
                    }
                    else
                    {
                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                        {
                            string s_temp = Convert.ToInt32((ts1-ts).TotalSeconds) / 60 / 60 + "时" + Convert.ToInt32((ts1 - ts).TotalSeconds) % (60 * 60) / 60 + "分" + Convert.ToInt32((ts1 - ts).TotalSeconds) % (60 * 60) % 60 + "秒";
                            txt_LeftTime.Text = s_temp;
                        }
                        else
                        {
                            string s_temp = Convert.ToInt32((ts1 - ts).TotalSeconds) / 60 / 60 + "H" + Convert.ToInt32((ts1 - ts).TotalSeconds) % (60 * 60) / 60 + "M" + Convert.ToInt32((ts1 - ts).TotalSeconds) % (60 * 60) % 60 + "S";
                            txt_LeftTime.Text = s_temp;
                        }
                    }
                }

                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                {
                    //更新自检值大小
                    lb_SelfChecking1.Text = "自检" + SmartDyeing.FADM_Object.Communal._da_self[1].ToString() + "g结果:";
                    lb_SelfChecking2.Text = "自检" + SmartDyeing.FADM_Object.Communal._da_self[2].ToString() + "g结果:";
                    lb_SelfChecking3.Text = "自检" + SmartDyeing.FADM_Object.Communal._da_self[3].ToString() + "g结果:";
                    lb_SelfChecking4.Text = "自检" + SmartDyeing.FADM_Object.Communal._da_self[4].ToString() + "g结果:";
                }
                else
                {
                    //更新自检值大小
                    lb_SelfChecking1.Text = "Self-checking(" + SmartDyeing.FADM_Object.Communal._da_self[1].ToString() + "g):";
                    lb_SelfChecking2.Text = "Self-checking(" + SmartDyeing.FADM_Object.Communal._da_self[2].ToString() + "g):";
                    lb_SelfChecking3.Text = "Self-checking(" + SmartDyeing.FADM_Object.Communal._da_self[3].ToString() + "g):";
                    lb_SelfChecking4.Text = "Self-checking(" + SmartDyeing.FADM_Object.Communal._da_self[4].ToString() + "g):";
                }
            }
        }
    }
}
