using Lib_File;
using SmartDyeing.FADM_Control;
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
    public partial class ABSStep : Form
    {
        //定义是添加还是修改:true:添加;false:修改
        private bool _b_insertOrUpdate = false;
        //代码
        private string _s_code = null;

        //定义ABS流程控件
        ABSConfig _page_AbsProcess = null;

        public ABSStep(string s_stepNum, string s_technologyName, string s_code, string s_stirringRate,string s_stirringTime, string s_drainTime,string s_parallelizingDishTime,string s_pumpingTime,string s_startingWavelength,string s_endWavelength,string s_wavelengthInterval,string s_dosage, bool b_insertOrUpdate, ABSConfig _Page_AbsProcess)
        {
            InitializeComponent();

            txt_StepNum.Text = s_stepNum;
            cbo_TechnologyName.Text = s_technologyName;
            txt_StirringRate.Text = s_stirringRate;
            txt_StirringTime.Text = s_stirringTime;
            txt_DrainTime.Text = s_drainTime;
            txt_ParallelizingDishTime.Text = s_parallelizingDishTime;
            txt_PumpingTime.Text= s_pumpingTime;
            txt_StartingWavelength.Text = Lib_Card.Configure.Parameter.Other_StartWave.ToString();
            txt_EndWavelength.Text  = Lib_Card.Configure.Parameter.Other_EndWave.ToString();
            txt_WavelengthInterval.Text = Lib_Card.Configure.Parameter.Other_IntWave.ToString();
            txt_Dosage.Text = s_dosage;
            _s_code = s_code;
            _b_insertOrUpdate = b_insertOrUpdate;
            _page_AbsProcess = _Page_AbsProcess;
            

            if (_b_insertOrUpdate)
            {
                //检查步号
                string s_sql = "SELECT StepNum FROM Abs_process WHERE Code = '" + _s_code + "' ORDER BY StepNum";
                DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                int i_step = 1;
                foreach (DataRow dr in dt_data.Rows)
                {
                    if (dr[0] is DBNull)
                        break;
                    int i_stepnow = Convert.ToInt16(dr[0]);
                    if (i_step != i_stepnow)
                    {
                        break;
                    }
                    i_step++;

                }

                txt_StepNum.Text = i_step.ToString();
            }

            foreach (Control c in this.Controls)
            {
                if (c is TextBox)
                {
                    if (c.Name == "txt_Dosage")
                    {
                        c.KeyPress += TextBox_KeyPress1;
                    }
                    else
                    {
                        c.KeyPress += TextBox_KeyPress;
                    }
                    c.KeyDown += TextBox_KeyDown;
                }
                else if(c is ComboBox)
                {
                    c.KeyDown += TextBox_KeyDown;
                }
            }
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            if (cbo_TechnologyName.Text == "加药"|| cbo_TechnologyName.Text == "加溶解剂")
            {
                if (txt_StepNum.Text != "" && txt_StepNum.Text != null && cbo_TechnologyName.Text != "" &&
                    cbo_TechnologyName.Text != null && txt_StirringRate.Text != "" && txt_StirringRate.Text != null &&
                    txt_Dosage.Text != "" && txt_Dosage.Text != null &&
                    txt_StartingWavelength.Text != "" && txt_StartingWavelength.Text != null &&
                    txt_EndWavelength.Text != "" && txt_EndWavelength.Text != null &&
                    txt_WavelengthInterval.Text != "" && txt_WavelengthInterval.Text != null)
                {
                    if (_b_insertOrUpdate)
                    {
                        string s_sql = null;
                        s_sql = "INSERT INTO Abs_process (StepNum, TechnologyName," +
                                           " Code,StirringRate,Dosage,StartingWavelength,EndWavelength,WavelengthInterval) VALUES('" + txt_StepNum.Text + "'," +
                                           "'" + cbo_TechnologyName.Text + "','"  + _s_code + "','" + txt_StirringRate.Text + "','" + txt_Dosage.Text + "','" + txt_StartingWavelength.Text + "','" + txt_EndWavelength.Text + "','" + txt_WavelengthInterval.Text + "');";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                        //删除新增为空的数据
                        s_sql = "delete from Abs_process where  StepNum is null and Code = '" + _s_code + "'; ";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                    }
                    else
                    {
                        string s_sql = "UPDATE Abs_process SET TechnologyName = '" + cbo_TechnologyName.Text + "'," +
                                           " StirringRate = '" + txt_StirringRate.Text + "', Dosage = '" + txt_Dosage.Text + "', StartingWavelength = '" + txt_StartingWavelength.Text + "', EndWavelength = '" + txt_EndWavelength.Text + "', WavelengthInterval = '" + txt_WavelengthInterval.Text + "' ,PumpingTime=NULL,StirringTime=NULL,DrainTime=NULL,ParallelizingDishTime=NULL  WHERE StepNum = '" + txt_StepNum.Text + "'" +
                                           " AND Code = '" + _s_code + "';";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                    }
                    _page_AbsProcess.ABSProcessShow(txt_StepNum.Text);


                    this.Close();
                }
                else
                {
                    FADM_Form.CustomMessageBox.Show("请先编写完资料再保存！", "操作异常", MessageBoxButtons.OK, false);
                }
            }
            else if (cbo_TechnologyName.Text == "加水")
            {
                if (txt_StepNum.Text != "" && txt_StepNum.Text != null && cbo_TechnologyName.Text != "" &&
                    cbo_TechnologyName.Text != null && txt_StirringRate.Text != "" && txt_StirringRate.Text != null)
                {
                    if (_b_insertOrUpdate)
                    {
                        string s_sql = null;
                        s_sql = "INSERT INTO Abs_process (StepNum, TechnologyName," +
                                           " Code,StirringRate) VALUES('" + txt_StepNum.Text + "'," +
                                           "'" + cbo_TechnologyName.Text + "','" + _s_code + "','" + txt_StirringRate.Text  + "');";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                        //删除新增为空的数据
                        s_sql = "delete from Abs_process where  StepNum is null and Code = '" + _s_code + "'; ";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                    }
                    else
                    {
                        string s_sql = "UPDATE Abs_process SET TechnologyName = '" + cbo_TechnologyName.Text + "'," +
                                           " StirringRate = '" + txt_StirringRate.Text  + "' ,PumpingTime=NULL,StirringTime=NULL,DrainTime=NULL,ParallelizingDishTime=NULL,StartingWavelength=NULL,EndWavelength=NULL,WavelengthInterval=NULL,Dosage=NULL WHERE StepNum = '" + txt_StepNum.Text + "'" +
                                           " AND Code = '" + _s_code + "';";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                    }

                    _page_AbsProcess.ABSProcessShow(txt_StepNum.Text);


                    this.Close();
                }
                else
                {
                    FADM_Form.CustomMessageBox.Show("请先编写完资料再保存！", "操作异常", MessageBoxButtons.OK, false);
                }
            }
            else if (cbo_TechnologyName.Text == "抽染液")
            {
                if (txt_StepNum.Text != "" && txt_StepNum.Text != null && cbo_TechnologyName.Text != "" &&
                    cbo_TechnologyName.Text != null)
                {
                    if (_b_insertOrUpdate)
                    {
                        string s_sql = null;
                        s_sql = "INSERT INTO Abs_process (StepNum, TechnologyName," +
                                           " Code) VALUES('" + txt_StepNum.Text + "'," +
                                           "'" + cbo_TechnologyName.Text + "','" + _s_code  + "');";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                        //删除新增为空的数据
                        s_sql = "delete from Abs_process where  StepNum is null and Code = '" + _s_code + "'; ";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                    }
                    else
                    {
                        string s_sql = "UPDATE Abs_process SET TechnologyName = '" + cbo_TechnologyName.Text  + "',PumpingTime=NULL,StirringTime=NULL,DrainTime=NULL,ParallelizingDishTime=NULL,StartingWavelength=NULL,EndWavelength=NULL,WavelengthInterval=NULL,Dosage=NULL,StirringRate=NULL WHERE StepNum = '" + txt_StepNum.Text + "'" +
                                           " AND Code = '" + _s_code + "';";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                    }

                    _page_AbsProcess.ABSProcessShow(txt_StepNum.Text);


                    this.Close();
                }
                else
                {
                    FADM_Form.CustomMessageBox.Show("请先编写完资料再保存！", "操作异常", MessageBoxButtons.OK, false);
                }
            }

            else if (cbo_TechnologyName.Text == "搅拌")
            {
                if (txt_StepNum.Text != "" && txt_StepNum.Text != null && cbo_TechnologyName.Text != "" &&
                    cbo_TechnologyName.Text != null && txt_StirringRate.Text != "" && txt_StirringRate.Text != null &&
                    txt_StirringTime.Text != "" && txt_StirringTime.Text != null)
                {
                    if (_b_insertOrUpdate)
                    {
                        string s_sql = null;
                        s_sql = "INSERT INTO Abs_process (StepNum, TechnologyName," +
                                           " Code,StirringRate,StirringTime) VALUES('" + txt_StepNum.Text + "'," +
                                           "'" + cbo_TechnologyName.Text + "','" + _s_code + "','" + txt_StirringRate.Text + "','" + txt_StirringTime.Text + "');";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                        //删除新增为空的数据
                        s_sql = "delete from Abs_process where  StepNum is null and Code = '" + _s_code + "'; ";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                    }
                    else
                    {
                        string s_sql = "UPDATE Abs_process SET TechnologyName = '" + cbo_TechnologyName.Text + "'," +
                                           " StirringRate = '" + txt_StirringRate.Text + "', StirringTime = '" + txt_StirringTime.Text + "',PumpingTime=NULL,DrainTime=NULL,ParallelizingDishTime=NULL,StartingWavelength=NULL,EndWavelength=NULL,WavelengthInterval=NULL,Dosage=NULL WHERE StepNum = '" + txt_StepNum.Text + "'" +
                                           " AND Code = '" + _s_code + "';";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                    }

                    _page_AbsProcess.ABSProcessShow(txt_StepNum.Text);


                    this.Close();
                }
                else
                {
                    FADM_Form.CustomMessageBox.Show("请先编写完资料再保存！", "操作异常", MessageBoxButtons.OK, false);
                }
            }

            else if (cbo_TechnologyName.Text == "排液")
            {
                if (txt_StepNum.Text != "" && txt_StepNum.Text != null && cbo_TechnologyName.Text != "" &&
                    cbo_TechnologyName.Text != null && txt_StirringRate.Text != "" && txt_StirringRate.Text != null &&
                    txt_DrainTime.Text != "" && txt_DrainTime.Text != null &&
                    txt_PumpingTime.Text != "" && txt_PumpingTime.Text != null &&
                    txt_ParallelizingDishTime.Text != "" && txt_ParallelizingDishTime.Text != null)
                {
                    if (_b_insertOrUpdate)
                    {
                        string s_sql = null;
                        s_sql = "INSERT INTO Abs_process (StepNum, TechnologyName," +
                                           " Code,StirringRate,DrainTime,ParallelizingDishTime,PumpingTime) VALUES('" + txt_StepNum.Text + "'," +
                                           "'" + cbo_TechnologyName.Text + "','" + _s_code + "','" + txt_StirringRate.Text + "','" + txt_DrainTime.Text + "','" + txt_ParallelizingDishTime.Text + "','" + txt_PumpingTime.Text + "');";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                        //删除新增为空的数据
                        s_sql = "delete from Abs_process where  StepNum is null and Code = '" + _s_code + "'; ";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                    }
                    else
                    {
                        string s_sql = "UPDATE Abs_process SET TechnologyName = '" + cbo_TechnologyName.Text + "'," +
                                           " StirringRate = '" + txt_StirringRate.Text + "', DrainTime = '" + txt_DrainTime.Text + "', ParallelizingDishTime = '" + txt_ParallelizingDishTime.Text + "',PumpingTime='"+ txt_PumpingTime.Text+"',StirringTime=NULL,StartingWavelength=NULL,EndWavelength=NULL,WavelengthInterval=NULL,Dosage=NULL WHERE StepNum = '" + txt_StepNum.Text + "'" +
                                           " AND Code = '" + _s_code + "';";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                    }

                    _page_AbsProcess.ABSProcessShow(txt_StepNum.Text);


                    this.Close();
                }
                else
                {
                    FADM_Form.CustomMessageBox.Show("请先编写完资料再保存！", "操作异常", MessageBoxButtons.OK, false);
                }
            }

            else if (cbo_TechnologyName.Text == "测吸光度")
            {
                if (txt_StepNum.Text != "" && txt_StepNum.Text != null && cbo_TechnologyName.Text != "" &&
                    cbo_TechnologyName.Text != null && txt_PumpingTime.Text != "" && txt_PumpingTime.Text != null &&
                    txt_StartingWavelength.Text != "" && txt_StartingWavelength.Text != null &&
                    txt_EndWavelength.Text != "" && txt_EndWavelength.Text != null &&
                    txt_WavelengthInterval.Text != "" && txt_WavelengthInterval.Text != null)
                {
                    if (_b_insertOrUpdate)
                    {
                        string s_sql = null;
                        s_sql = "INSERT INTO Abs_process (StepNum, TechnologyName," +
                                           " Code,PumpingTime,StartingWavelength,EndWavelength,WavelengthInterval) VALUES('" + txt_StepNum.Text + "'," +
                                           "'" + cbo_TechnologyName.Text + "','" + _s_code + "','" + txt_PumpingTime.Text + "','" + txt_StartingWavelength.Text + "','" + txt_EndWavelength.Text + "','" + txt_WavelengthInterval.Text + "');";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                        //删除新增为空的数据
                        s_sql = "delete from Abs_process where  StepNum is null and Code = '" + _s_code + "'; ";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                    }
                    else
                    {
                        string s_sql = "UPDATE Abs_process SET TechnologyName = '" + cbo_TechnologyName.Text + "'," +
                                           " PumpingTime = '" + txt_PumpingTime.Text + "', StartingWavelength = '" + txt_StartingWavelength.Text + "', EndWavelength = '" + txt_EndWavelength.Text + "', WavelengthInterval = '" + txt_WavelengthInterval.Text + "',StirringTime=NULL,DrainTime=NULL,ParallelizingDishTime=NULL,Dosage=NULL,StirringRate=NULL WHERE StepNum = '" + txt_StepNum.Text + "'" +
                                           " AND Code = '" + _s_code + "';";
                        FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);
                    }

                    _page_AbsProcess.ABSProcessShow(txt_StepNum.Text);


                    this.Close();
                }
                else
                {
                    FADM_Form.CustomMessageBox.Show("请先编写完资料再保存！", "操作异常", MessageBoxButtons.OK, false);
                }
            }
        }

        void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = SmartDyeing.FADM_Object.MyTextBoxCheck.NumberTextbox_KeyPress(e);
        }

        //输入检查
        void TextBox_KeyPress1(object sender, KeyPressEventArgs e)
        {
            e.Handled = SmartDyeing.FADM_Object.MyTextBoxCheck.NumberDotTextbox_KeyPress(sender, e);
        }

        void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    Control[] c = { cbo_TechnologyName, txt_StirringRate, txt_StirringTime, txt_DrainTime, txt_ParallelizingDishTime, txt_PumpingTime, txt_StartingWavelength, txt_EndWavelength, txt_WavelengthInterval, txt_Dosage };
                    for (int i = 0; i < c.Length; i++)
                    {
                        try
                        {
                            TextBox txt = (TextBox)sender;
                            if (txt.Text == "")
                                return;
                            if (txt.Name == c[i].Name)
                            {
                                //判断是否最后一个
                                if (i == c.Length - 1)
                                {
                                    btn_Save.Focus();
                                    return;
                                }
                                else
                                {
                                    for (int j = i; j < c.Length-1; j++)
                                    {
                                        if (!c[j + 1].Enabled)
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            c[j + 1].Focus();
                                            return;
                                        }
                                    }
                                    if(txt.Focused)
                                    {
                                        btn_Save.Focus();
                                        return;
                                    }
                                }
                            }
                        }
                        catch
                        {
                            try
                            {
                                ComboBox txt = (ComboBox)sender;
                                if (txt.Text == "")
                                    return;
                                if (txt.Name == c[i].Name)
                                {
                                    //判断是否最后一个
                                    if (i == c.Length - 1)
                                    {
                                        btn_Save.Focus();
                                        return;
                                    }
                                    else
                                    {
                                        for (int j = i; j < c.Length-1; j++)
                                        {
                                            if (!c[j + 1].Enabled)
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                c[j + 1].Focus();
                                                return;
                                            }
                                        }
                                        if (txt.Focused)
                                        {
                                            btn_Save.Focus();
                                            return;
                                        }
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void cbo_TechnologyName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbo_TechnologyName.Text == "加药" )
            {
                txt_StirringRate.Enabled = true;
                txt_StirringTime.Enabled = false;
                txt_DrainTime.Enabled = false;
                txt_ParallelizingDishTime.Enabled = false;
                txt_PumpingTime.Enabled = false;
                txt_StartingWavelength.Enabled = false;
                txt_EndWavelength.Enabled = false;
                txt_WavelengthInterval.Enabled = false;
                txt_Dosage.Enabled = true;
            }
            else if(cbo_TechnologyName.Text == "加水")
            {
                txt_StirringRate.Enabled = true;
                txt_StirringTime.Enabled = false;
                txt_DrainTime.Enabled = false;
                txt_ParallelizingDishTime.Enabled = false;
                txt_PumpingTime.Enabled = false;
                txt_StartingWavelength.Enabled = false;
                txt_EndWavelength.Enabled = false;
                txt_WavelengthInterval.Enabled = false;
                txt_Dosage.Enabled = false;
            }
            else if (cbo_TechnologyName.Text == "抽染液")
            {
                txt_StirringRate.Enabled = false;
                txt_StirringTime.Enabled = false;
                txt_DrainTime.Enabled = false;
                txt_ParallelizingDishTime.Enabled = false;
                txt_PumpingTime.Enabled = false;
                txt_StartingWavelength.Enabled = false;
                txt_EndWavelength.Enabled = false;
                txt_WavelengthInterval.Enabled = false;
                txt_Dosage.Enabled = false;
            }
            else if (cbo_TechnologyName.Text == "搅拌")
            {
                txt_StirringRate.Enabled = true;
                txt_StirringTime.Enabled = true;
                txt_DrainTime.Enabled = false;
                txt_ParallelizingDishTime.Enabled = false;
                txt_PumpingTime.Enabled = false;
                txt_StartingWavelength.Enabled = false;
                txt_EndWavelength.Enabled = false;
                txt_WavelengthInterval.Enabled = false;
                txt_Dosage.Enabled = false;
            }
            else if (cbo_TechnologyName.Text == "排液")
            {
                txt_StirringRate.Enabled = true;
                txt_StirringTime.Enabled = false;
                txt_DrainTime.Enabled = true;
                txt_ParallelizingDishTime.Enabled = true;
                txt_PumpingTime.Enabled = true;
                txt_StartingWavelength.Enabled = false;
                txt_EndWavelength.Enabled = false;
                txt_WavelengthInterval.Enabled = false;
                txt_Dosage.Enabled = false;
            }
            else if (cbo_TechnologyName.Text == "测吸光度")
            {
                txt_StirringRate.Enabled = false;
                txt_StirringTime.Enabled = false;
                txt_DrainTime.Enabled = false;
                txt_ParallelizingDishTime.Enabled = false;
                txt_PumpingTime.Enabled = true;
                txt_StartingWavelength.Enabled = false;
                txt_EndWavelength.Enabled = false;
                txt_WavelengthInterval.Enabled = false;
                txt_Dosage.Enabled = false;
            }
            else if (cbo_TechnologyName.Text == "加溶解剂")
            {
                txt_StirringRate.Enabled = true;
                txt_StirringTime.Enabled = false;
                txt_DrainTime.Enabled = false;
                txt_ParallelizingDishTime.Enabled = false;
                txt_PumpingTime.Enabled = false;
                txt_StartingWavelength.Enabled = false;
                txt_EndWavelength.Enabled = false;
                txt_WavelengthInterval.Enabled = false;
                txt_Dosage.Enabled = true;
            }
        }
    }
}
