using SmartDyeing.FADM_Control;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SmartDyeing.FADM_Form;
using SmartDyeing.FADM_Object;
using System.Threading;
using static SmartDyeing.FADM_Auto.Dye;

namespace SmartDyeing.FADM_Form
{
    public partial class CupArea : Form
    {

        [DllImport("user32.dll", EntryPoint = "FindWindow", CharSet = CharSet.Auto)]
        private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        public const int WM_CLOSE = 0x10;

        private List<SmartDyeing.FADM_Control.Cup> _lis_cup = new List<SmartDyeing.FADM_Control.Cup>();
        private List<Label> _lis_lab = new List<Label>();
        private List<CheckBox> _lis_chk = new List<CheckBox>();

        int _i_checkBox_num = 0;
        int _i_max_num = 0;

        private List<Label> _lis_lab_abs = new List<Label>();

        private int _i_bottleAlarmWeight,
                    _i_bottleMinWeight,
                    _i_machineType;

        public CupArea()
        {
            InitializeComponent();

            //获取机台型号
            _i_machineType = Lib_Card.Configure.Parameter.Machine_Bottle_Total;

            //获取瓶子列数
            int i_bottleLine = Lib_Card.Configure.Parameter.Machine_Bottle_Column;

            _i_bottleAlarmWeight = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_Bottle_AlarmWeight);
            _i_bottleMinWeight = Convert.ToInt32(Lib_Card.Configure.Parameter.Other_Bottle_MinWeight);
        }

        private void CupArea_Load(object sender, EventArgs e)
        {
            //判断是否加入吸光度区域
            bool b_insertAbs = false;

            if (!Communal._b_isUseClamp)
            {
                this.Size = new System.Drawing.Size(1169, 676);
            }
            else
            {
                System.Windows.Forms.GroupBox gb = new System.Windows.Forms.GroupBox();
                gb.Size = new System.Drawing.Size(475, 590);
                gb.Location = new System.Drawing.Point(10 + 375 + 10 + 375 + 10 + 375 + 10, 10);
                gb.Name = "GB";
                gb.Text = "放布区域";
                this.Controls.Add(gb);

                //放布区域显示
                int i_max = 72;
                if(Lib_Card.Configure.Parameter.Machine_AreaDryCloth3_Type==1)
                {
                    i_max = Lib_Card.Configure.Parameter.Machine_AreaDryCloth3_CupMax;
                }
                else if (Lib_Card.Configure.Parameter.Machine_AreaDryCloth2_Type == 1)
                {
                    i_max = Lib_Card.Configure.Parameter.Machine_AreaDryCloth2_CupMax;
                }
                else if (Lib_Card.Configure.Parameter.Machine_AreaDryCloth1_Type == 1)
                {
                    i_max = Lib_Card.Configure.Parameter.Machine_AreaDryCloth1_CupMax;
                }
                _i_max_num = i_max;

                int i_Interval_X = 0;
                i_Interval_X = (580 - 20) / (i_max / 10 + 1);
                for (int i = 0; i < i_max; i++)
                {
                    System.Windows.Forms.CheckBox cb = new System.Windows.Forms.CheckBox();
                    int x, y;
                    x = 5 + (i % 10) * 45;
                    y = 20 + (i / 10) * i_Interval_X;
                    cb.Location = new System.Drawing.Point(x, y);
                    cb.Size = new System.Drawing.Size(45, 20);
                    cb.Text = (i+1).ToString();
                    cb.Name = "chk_" + (i + 1).ToString("d3");
                    cb.AutoCheck = false;
                    cb.MouseDown += CheckBox_MouseDown;
                    cb.ContextMenuStrip = contextMenuStrip1;
                    gb.Controls.Add(cb);
                    _lis_chk.Add(cb);
                }

                //重置按钮
                System.Windows.Forms.Button button = new System.Windows.Forms.Button();
                button.Location = new Point(10 + 375 + 10 + 375 + 10 + 375 + 10+200,595);
                button.Text= "重置";
                button.Size = new System.Drawing.Size(60, 40);
                button.Click += but_Click;
                this.Controls.Add(button);
            }

            //区域1
            if (Lib_Card.Configure.Parameter.Machine_Area1_Type == 1 || Lib_Card.Configure.Parameter.Machine_Area1_Type == 2)
            {
                System.Windows.Forms.Panel panel1 = new System.Windows.Forms.Panel();
                panel1.Size = new System.Drawing.Size(375, 300);
                panel1.Location = new System.Drawing.Point(10, 10);

                this.Controls.Add(panel1);

                FADM_Control.DripBeater s = new DripBeater(Lib_Card.Configure.Parameter.Machine_Area1_Row, Lib_Card.Configure.Parameter.Machine_Area1_CupMin, Lib_Card.Configure.Parameter.Machine_Area1_CupMax);
                panel1.Controls.Add(s);
                string s_str1 = null;
                if (Lib_Card.Configure.Parameter.Machine_Area1_Type == 1)
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    {
                        s_str1 = "一号前处理区";
                    }
                    else
                    {
                        s_str1 = "No.1 pre-treatment area";
                    }
                }
                else
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    {
                        s_str1 = "一号滴液区";
                    }
                    else
                    {
                        s_str1 = "No.1 drip area";
                    }
                }

                foreach (Control c1 in s.Controls)
                {

                    if (c1 is GroupBox)
                    {
                        c1.Text = s_str1;
                        foreach (Control c2 in c1.Controls)
                        {
                            if (c2 is SmartDyeing.FADM_Control.Cup)
                            {
                                ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click2;
                                ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave2;
                                _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                            }
                        }
                    }
                }

            }
            else if (Lib_Card.Configure.Parameter.Machine_Area1_Type == 3)
            {
                System.Windows.Forms.Panel panel1 = new System.Windows.Forms.Panel();
                panel1.Size = new System.Drawing.Size(375, 300);
                panel1.Location = new System.Drawing.Point(10, 10);
                this.Controls.Add(panel1);

                if (Lib_Card.Configure.Parameter.Machine_Area1_DyeType == 0 || Lib_Card.Configure.Parameter.Machine_Area1_DyeType == 5)
                {
                    FADM_Control.TenBeater s = new TenBeater();
                    panel1.Controls.Add(s);

                    foreach (Control c1 in s.Controls)
                    {
                        if (c1 is GroupBox)
                        {
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            {
                                c1.Text = "一号染固色区";
                            }
                            else
                            {

                                c1.Text = "No.1 dyeing and fixation area";
                            }
                            int i_n = 0;
                            foreach (Control c2 in c1.Controls)
                            {
                                if (c2 is SmartDyeing.FADM_Control.Cup)
                                {
                                    ((SmartDyeing.FADM_Control.Cup)c2).Name = (Lib_Card.Configure.Parameter.Machine_Area1_CupMin + i_n).ToString();
                                    ((SmartDyeing.FADM_Control.Cup)c2).NO = (Lib_Card.Configure.Parameter.Machine_Area1_CupMin + i_n).ToString();
                                    ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click;
                                    ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave;
                                    _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                                    i_n++;
                                }
                                else if (c2 is Label)
                                {
                                    _lis_lab.Add((Label)c2);
                                }
                            }
                        }


                    }
                }
                else
                {
                    //6杯翻转缸
                    if (Lib_Card.Configure.Parameter.Machine_Area1_DyeType == 1)
                    {
                        FADM_Control.Beater s = new Beater();
                        panel1.Controls.Add(s);

                        foreach (Control c1 in s.Controls)
                        {
                            if (c1 is GroupBox)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    c1.Text = "一号染固色区";
                                }
                                else
                                {

                                    c1.Text = "No.1 dyeing and fixation area";
                                }
                                int i_n = 0;
                                foreach (Control c2 in c1.Controls)
                                {
                                    if (c2 is SmartDyeing.FADM_Control.Cup)
                                    {
                                        ((SmartDyeing.FADM_Control.Cup)c2).Name = (Lib_Card.Configure.Parameter.Machine_Area1_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).NO = (Lib_Card.Configure.Parameter.Machine_Area1_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click;
                                        ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave;
                                        _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                                        i_n++;
                                    }
                                    else if (c2 is Label)
                                    {
                                        _lis_lab.Add((Label)c2);
                                    }
                                }
                            }
                        }
                    }
                    //12杯翻转缸/12杯精密机
                    else if (Lib_Card.Configure.Parameter.Machine_Area1_DyeType == 2 || Lib_Card.Configure.Parameter.Machine_Area1_DyeType == 6)
                    {
                        FADM_Control.TwelveBeater s = new TwelveBeater();
                        panel1.Controls.Add(s);

                        foreach (Control c1 in s.Controls)
                        {
                            if (c1 is GroupBox)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    c1.Text = "一号染固色区";
                                }
                                else
                                {

                                    c1.Text = "No.1 dyeing and fixation area";
                                }
                                int i_n = 0;
                                foreach (Control c2 in c1.Controls)
                                {
                                    if (c2 is SmartDyeing.FADM_Control.Cup)
                                    {
                                        ((SmartDyeing.FADM_Control.Cup)c2).Name = (Lib_Card.Configure.Parameter.Machine_Area1_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).NO = (Lib_Card.Configure.Parameter.Machine_Area1_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click;
                                        ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave;
                                        _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                                        i_n++;
                                    }
                                    else if (c2 is Label)
                                    {
                                        _lis_lab.Add((Label)c2);
                                    }
                                }
                            }
                        }
                    }

                    //4杯翻转缸
                    else if (Lib_Card.Configure.Parameter.Machine_Area1_DyeType == 3)
                    {
                        FADM_Control.FourBeater s = new FourBeater();
                        panel1.Controls.Add(s);

                        foreach (Control c1 in s.Controls)
                        {
                            if (c1 is GroupBox)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    c1.Text = "一号染固色区";
                                }
                                else
                                {

                                    c1.Text = "No.1 dyeing and fixation area";
                                }
                                int i_n = 0;
                                foreach (Control c2 in c1.Controls)
                                {
                                    if (c2 is SmartDyeing.FADM_Control.Cup)
                                    {
                                        ((SmartDyeing.FADM_Control.Cup)c2).Name = (Lib_Card.Configure.Parameter.Machine_Area1_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).NO = (Lib_Card.Configure.Parameter.Machine_Area1_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click;
                                        ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave;
                                        _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                                        i_n++;
                                    }
                                    else if (c2 is Label)
                                    {
                                        _lis_lab.Add((Label)c2);
                                    }
                                }
                            }
                        }
                    }

                    //8杯精密机
                    else if (Lib_Card.Configure.Parameter.Machine_Area1_DyeType == 7)
                    {
                        FADM_Control.EightBeater s = new EightBeater();
                        panel1.Controls.Add(s);

                        foreach (Control c1 in s.Controls)
                        {
                            if (c1 is GroupBox)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    c1.Text = "一号染固色区";
                                }
                                else
                                {

                                    c1.Text = "No.1 dyeing and fixation area";
                                }
                                int i_n = 0;
                                foreach (Control c2 in c1.Controls)
                                {
                                    if (c2 is SmartDyeing.FADM_Control.Cup)
                                    {
                                        ((SmartDyeing.FADM_Control.Cup)c2).Name = (Lib_Card.Configure.Parameter.Machine_Area1_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).NO = (Lib_Card.Configure.Parameter.Machine_Area1_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click;
                                        ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave;
                                        _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                                        i_n++;
                                    }
                                    else if (c2 is Label)
                                    {
                                        _lis_lab.Add((Label)c2);
                                    }
                                }
                            }
                        }
                    }
                }



            }
            else
            {
                

                //加入吸光度模块
                if (Lib_Card.Configure.Parameter.Other_UseAbs == 1)
                {
                    if (!b_insertAbs)
                    {
                        System.Windows.Forms.Panel panel1 = new System.Windows.Forms.Panel();
                        panel1.Size = new System.Drawing.Size(375, 300);
                        panel1.Location = new System.Drawing.Point(10, 10);
                        this.Controls.Add(panel1);

                        b_insertAbs = true;
                        FADM_Control.AbsBeater s = new AbsBeater();
                        panel1.Controls.Add(s);

                        foreach (Control c1 in s.Controls)
                        {
                            if (c1 is GroupBox)
                            {

                                foreach (Control c2 in c1.Controls)
                                {
                                    if (c2 is Label)
                                    {
                                        _lis_lab_abs.Add((Label)c2);
                                    }
                                    else if (c2 is Cup)
                                    {
                                        ((Cup)c2).Click += Cup_Click3;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //区域2
            if (Lib_Card.Configure.Parameter.Machine_Area2_Type == 1 || Lib_Card.Configure.Parameter.Machine_Area2_Type == 2)
            {
                System.Windows.Forms.Panel panel2 = new System.Windows.Forms.Panel();
                panel2.Size = new System.Drawing.Size(375, 300);
                panel2.Location = new System.Drawing.Point(10 + 375 + 10, 10);
                this.Controls.Add(panel2);

                FADM_Control.DripBeater s = new DripBeater(Lib_Card.Configure.Parameter.Machine_Area2_Row, Lib_Card.Configure.Parameter.Machine_Area2_CupMin, Lib_Card.Configure.Parameter.Machine_Area2_CupMax);
                panel2.Controls.Add(s);
                string str1 = null;
                if (Lib_Card.Configure.Parameter.Machine_Area2_Type == 1)
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    {
                        str1 = "二号前处理区";
                    }
                    else
                    {
                        str1 = "No.2 pre-treatment area";
                    }
                }
                else
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    {
                        str1 = "二号滴液区";
                    }
                    else
                    {
                        str1 = "No.2 drip area";
                    }
                }

                foreach (Control c1 in s.Controls)
                {
                    if (c1 is GroupBox)
                    {
                        c1.Text = str1;
                        foreach (Control c2 in c1.Controls)
                        {
                            if (c2 is SmartDyeing.FADM_Control.Cup)
                            {
                                ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click2;
                                ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave2;
                                _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                            }
                        }
                    }

                }

            }
            else if (Lib_Card.Configure.Parameter.Machine_Area2_Type == 3)
            {
                System.Windows.Forms.Panel panel2 = new System.Windows.Forms.Panel();
                panel2.Size = new System.Drawing.Size(375, 300);
                panel2.Location = new System.Drawing.Point(10 + 375 + 10, 10);
                this.Controls.Add(panel2);

                if (Lib_Card.Configure.Parameter.Machine_Area2_DyeType == 0 || Lib_Card.Configure.Parameter.Machine_Area2_DyeType == 5)
                {
                    FADM_Control.TenBeater s = new TenBeater();
                    panel2.Controls.Add(s);

                    foreach (Control c1 in s.Controls)
                    {
                        if (c1 is GroupBox)
                        {
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            {
                                c1.Text = "二号染固色区";
                            }
                            else
                            {

                                c1.Text = "No.2 dyeing and fixation area";
                            }
                            int i_n = 0;
                            foreach (Control c2 in c1.Controls)
                            {
                                if (c2 is SmartDyeing.FADM_Control.Cup)
                                {
                                    ((SmartDyeing.FADM_Control.Cup)c2).Name = (Lib_Card.Configure.Parameter.Machine_Area2_CupMin + i_n).ToString();
                                    ((SmartDyeing.FADM_Control.Cup)c2).NO = (Lib_Card.Configure.Parameter.Machine_Area2_CupMin + i_n).ToString();
                                    ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click;
                                    ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave;
                                    _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                                    i_n++;
                                }
                                else if (c2 is Label)
                                {
                                    _lis_lab.Add((Label)c2);
                                }
                            }
                        }


                    }
                }
                else
                {
                    //6杯翻转缸
                    if (Lib_Card.Configure.Parameter.Machine_Area2_DyeType == 1)
                    {
                        FADM_Control.Beater s = new Beater();
                        panel2.Controls.Add(s);

                        foreach (Control c1 in s.Controls)
                        {
                            if (c1 is GroupBox)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    c1.Text = "二号染固色区";
                                }
                                else
                                {

                                    c1.Text = "No.2 dyeing and fixation area";
                                }
                                int i_n = 0;
                                foreach (Control c2 in c1.Controls)
                                {
                                    if (c2 is SmartDyeing.FADM_Control.Cup)
                                    {
                                        ((SmartDyeing.FADM_Control.Cup)c2).Name = (Lib_Card.Configure.Parameter.Machine_Area2_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).NO = (Lib_Card.Configure.Parameter.Machine_Area2_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click;
                                        ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave;
                                        _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                                        i_n++;
                                    }
                                    else if (c2 is Label)
                                    {
                                        _lis_lab.Add((Label)c2);
                                    }
                                }
                            }
                        }
                    }
                    //12杯翻转缸/12杯精密机
                    else if (Lib_Card.Configure.Parameter.Machine_Area2_DyeType == 2 || Lib_Card.Configure.Parameter.Machine_Area2_DyeType == 6)
                    {
                        FADM_Control.TwelveBeater s = new TwelveBeater();
                        panel2.Controls.Add(s);

                        foreach (Control c1 in s.Controls)
                        {
                            if (c1 is GroupBox)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    c1.Text = "二号染固色区";
                                }
                                else
                                {

                                    c1.Text = "No.2 dyeing and fixation area";
                                }
                                int i_n = 0;
                                foreach (Control c2 in c1.Controls)
                                {
                                    if (c2 is SmartDyeing.FADM_Control.Cup)
                                    {
                                        ((SmartDyeing.FADM_Control.Cup)c2).Name = (Lib_Card.Configure.Parameter.Machine_Area2_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).NO = (Lib_Card.Configure.Parameter.Machine_Area2_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click;
                                        ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave;
                                        _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                                        i_n++;
                                    }
                                    else if (c2 is Label)
                                    {
                                        _lis_lab.Add((Label)c2);
                                    }
                                }
                            }
                        }
                    }

                    //4杯翻转缸
                    else if (Lib_Card.Configure.Parameter.Machine_Area2_DyeType == 3)
                    {
                        FADM_Control.FourBeater s = new FourBeater();
                        panel2.Controls.Add(s);

                        foreach (Control c1 in s.Controls)
                        {
                            if (c1 is GroupBox)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    c1.Text = "二号染固色区";
                                }
                                else
                                {

                                    c1.Text = "No.2 dyeing and fixation area";
                                }
                                int i_n = 0;
                                foreach (Control c2 in c1.Controls)
                                {
                                    if (c2 is SmartDyeing.FADM_Control.Cup)
                                    {
                                        ((SmartDyeing.FADM_Control.Cup)c2).Name = (Lib_Card.Configure.Parameter.Machine_Area2_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).NO = (Lib_Card.Configure.Parameter.Machine_Area2_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click;
                                        ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave;
                                        _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                                        i_n++;
                                    }
                                    else if (c2 is Label)
                                    {
                                        _lis_lab.Add((Label)c2);
                                    }
                                }
                            }
                        }
                    }

                    //8杯精密机
                    else if (Lib_Card.Configure.Parameter.Machine_Area2_DyeType == 7)
                    {
                        FADM_Control.EightBeater s = new EightBeater();
                        panel2.Controls.Add(s);

                        foreach (Control c1 in s.Controls)
                        {
                            if (c1 is GroupBox)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    c1.Text = "二号染固色区";
                                }
                                else
                                {

                                    c1.Text = "No.2 dyeing and fixation area";
                                }
                                int i_n = 0;
                                foreach (Control c2 in c1.Controls)
                                {
                                    if (c2 is SmartDyeing.FADM_Control.Cup)
                                    {
                                        ((SmartDyeing.FADM_Control.Cup)c2).Name = (Lib_Card.Configure.Parameter.Machine_Area2_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).NO = (Lib_Card.Configure.Parameter.Machine_Area2_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click;
                                        ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave;
                                        _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                                        i_n++;
                                    }
                                    else if (c2 is Label)
                                    {
                                        _lis_lab.Add((Label)c2);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                //加入吸光度模块
                if (Lib_Card.Configure.Parameter.Other_UseAbs == 1)
                {
                    if (!b_insertAbs)
                    {
                        System.Windows.Forms.Panel panel2 = new System.Windows.Forms.Panel();
                        panel2.Size = new System.Drawing.Size(375, 300);
                        panel2.Location = new System.Drawing.Point(10 + 375 + 10, 10);
                        this.Controls.Add(panel2);

                        b_insertAbs = true;
                        FADM_Control.AbsBeater s = new AbsBeater();
                        panel2.Controls.Add(s);

                        foreach (Control c1 in s.Controls)
                        {
                            if (c1 is GroupBox)
                            {

                                foreach (Control c2 in c1.Controls)
                                {
                                    if (c2 is Label)
                                    {
                                        _lis_lab_abs.Add((Label)c2);
                                    }
                                    else if (c2 is Cup)
                                    {
                                        ((Cup)c2).Click += Cup_Click3;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //区域3
            if (Lib_Card.Configure.Parameter.Machine_Area3_Type == 1 || Lib_Card.Configure.Parameter.Machine_Area3_Type == 2)
            {
                System.Windows.Forms.Panel panel3 = new System.Windows.Forms.Panel();
                panel3.Size = new System.Drawing.Size(375, 300);
                panel3.Location = new System.Drawing.Point(10 + 375 + 10 + 375 + 10, 10);
                this.Controls .Add(panel3);

                FADM_Control.DripBeater s = new DripBeater(Lib_Card.Configure.Parameter.Machine_Area3_Row, Lib_Card.Configure.Parameter.Machine_Area3_CupMin, Lib_Card.Configure.Parameter.Machine_Area3_CupMax);
                panel3.Controls.Add(s);

                string s_str1 = null;
                if (Lib_Card.Configure.Parameter.Machine_Area3_Type == 1)
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    {
                        s_str1 = "三号前处理区";
                    }
                    else
                    {
                        s_str1 = "No.3 pre-treatment area";
                    }
                }
                else
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    {
                        s_str1 = "三号滴液区";
                    }
                    else
                    {
                        s_str1 = "No.3 drip area";
                    }
                }

                foreach (Control c1 in s.Controls)
                {
                    if (c1 is GroupBox)
                    {
                        c1.Text = s_str1;
                        foreach (Control c2 in c1.Controls)
                        {
                            if (c2 is SmartDyeing.FADM_Control.Cup)
                            {
                                ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click2;
                                ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave2;
                                _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                            }
                        }
                    }

                }

            }
            else if (Lib_Card.Configure.Parameter.Machine_Area3_Type == 3)
            {
                System.Windows.Forms.Panel panel3 = new System.Windows.Forms.Panel();
                panel3.Size = new System.Drawing.Size(375, 300);
                panel3.Location = new System.Drawing.Point(10 + 375 + 10 + 375 + 10, 10);
                this.Controls.Add(panel3);

                if (Lib_Card.Configure.Parameter.Machine_Area3_DyeType == 0 || Lib_Card.Configure.Parameter.Machine_Area3_DyeType == 5)
                {
                    FADM_Control.TenBeater s = new TenBeater();
                    panel3.Controls.Add(s);

                    foreach (Control c1 in s.Controls)
                    {
                        if (c1 is GroupBox)
                        {
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            {
                                c1.Text = "三号染固色区";
                            }
                            else
                            {

                                c1.Text = "No.3 dyeing and fixation area";
                            }
                            int i_n = 0;
                            foreach (Control c2 in c1.Controls)
                            {
                                if (c2 is SmartDyeing.FADM_Control.Cup)
                                {
                                    ((SmartDyeing.FADM_Control.Cup)c2).Name = (Lib_Card.Configure.Parameter.Machine_Area3_CupMin + i_n).ToString();
                                    ((SmartDyeing.FADM_Control.Cup)c2).NO = (Lib_Card.Configure.Parameter.Machine_Area3_CupMin + i_n).ToString();
                                    ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click;
                                    ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave;
                                    _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                                    i_n++;
                                }
                                else if (c2 is Label)
                                {
                                    _lis_lab.Add((Label)c2);
                                }
                            }


                        }


                    }
                }
                else
                {
                    //6杯翻转缸
                    if (Lib_Card.Configure.Parameter.Machine_Area3_DyeType == 1)
                    {
                        FADM_Control.Beater s = new Beater();
                        panel3.Controls.Add(s);

                        foreach (Control c1 in s.Controls)
                        {
                            if (c1 is GroupBox)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    c1.Text = "三号染固色区";
                                }
                                else
                                {

                                    c1.Text = "No.3 dyeing and fixation area";
                                }
                                int i_n = 0;
                                foreach (Control c2 in c1.Controls)
                                {
                                    if (c2 is SmartDyeing.FADM_Control.Cup)
                                    {
                                        ((SmartDyeing.FADM_Control.Cup)c2).Name = (Lib_Card.Configure.Parameter.Machine_Area3_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).NO = (Lib_Card.Configure.Parameter.Machine_Area3_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click;
                                        ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave;
                                        _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                                        i_n++;
                                    }
                                    else if (c2 is Label)
                                    {
                                        _lis_lab.Add((Label)c2);
                                    }
                                }
                            }
                        }
                    }
                    //12杯翻转缸/12杯精密机
                    else if (Lib_Card.Configure.Parameter.Machine_Area3_DyeType == 2 || Lib_Card.Configure.Parameter.Machine_Area3_DyeType == 6)
                    {
                        FADM_Control.TwelveBeater s = new TwelveBeater();
                        panel3.Controls.Add(s);

                        foreach (Control c1 in s.Controls)
                        {
                            if (c1 is GroupBox)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    c1.Text = "三号染固色区";
                                }
                                else
                                {

                                    c1.Text = "No.3 dyeing and fixation area";
                                }
                                int i_n = 0;
                                foreach (Control c2 in c1.Controls)
                                {
                                    if (c2 is SmartDyeing.FADM_Control.Cup)
                                    {
                                        ((SmartDyeing.FADM_Control.Cup)c2).Name = (Lib_Card.Configure.Parameter.Machine_Area3_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).NO = (Lib_Card.Configure.Parameter.Machine_Area3_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click;
                                        ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave;
                                        _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                                        i_n++;
                                    }
                                    else if (c2 is Label)
                                    {
                                        _lis_lab.Add((Label)c2);
                                    }
                                }
                            }
                        }
                    }

                    //4杯翻转缸
                    else if (Lib_Card.Configure.Parameter.Machine_Area3_DyeType == 3)
                    {
                        FADM_Control.FourBeater s = new FourBeater();
                        panel3.Controls.Add(s);

                        foreach (Control c1 in s.Controls)
                        {
                            if (c1 is GroupBox)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    c1.Text = "三号染固色区";
                                }
                                else
                                {

                                    c1.Text = "No.3 dyeing and fixation area";
                                }
                                int i_n = 0;
                                foreach (Control c2 in c1.Controls)
                                {
                                    if (c2 is SmartDyeing.FADM_Control.Cup)
                                    {
                                        ((SmartDyeing.FADM_Control.Cup)c2).Name = (Lib_Card.Configure.Parameter.Machine_Area3_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).NO = (Lib_Card.Configure.Parameter.Machine_Area3_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click;
                                        ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave;
                                        _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                                        i_n++;
                                    }
                                    else if (c2 is Label)
                                    {
                                        _lis_lab.Add((Label)c2);
                                    }
                                }
                            }
                        }
                    }

                    //8杯精密机
                    else if (Lib_Card.Configure.Parameter.Machine_Area3_DyeType == 7)
                    {
                        FADM_Control.EightBeater s = new EightBeater();
                        panel3.Controls.Add(s);

                        foreach (Control c1 in s.Controls)
                        {
                            if (c1 is GroupBox)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    c1.Text = "三号染固色区";
                                }
                                else
                                {

                                    c1.Text = "No.3 dyeing and fixation area";
                                }
                                int i_n = 0;
                                foreach (Control c2 in c1.Controls)
                                {
                                    if (c2 is SmartDyeing.FADM_Control.Cup)
                                    {
                                        ((SmartDyeing.FADM_Control.Cup)c2).Name = (Lib_Card.Configure.Parameter.Machine_Area3_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).NO = (Lib_Card.Configure.Parameter.Machine_Area3_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click;
                                        ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave;
                                        _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                                        i_n++;
                                    }
                                    else if (c2 is Label)
                                    {
                                        _lis_lab.Add((Label)c2);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                //加入吸光度模块
                if (Lib_Card.Configure.Parameter.Other_UseAbs == 1)
                {
                    if (!b_insertAbs)
                    {
                        System.Windows.Forms.Panel panel3 = new System.Windows.Forms.Panel();
                        panel3.Size = new System.Drawing.Size(375, 300);
                        panel3.Location = new System.Drawing.Point(10 + 375 + 10 + 375 + 10, 10);
                        this.Controls.Add(panel3);

                        b_insertAbs = true;
                        FADM_Control.AbsBeater s = new AbsBeater();
                        panel3.Controls.Add(s);

                        foreach (Control c1 in s.Controls)
                        {
                            if (c1 is GroupBox)
                            {

                                foreach (Control c2 in c1.Controls)
                                {
                                    if (c2 is Label)
                                    {
                                        _lis_lab_abs.Add((Label)c2);
                                    }
                                    else if (c2 is Cup)
                                    {
                                        ((Cup)c2).Click += Cup_Click3;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //区域4
            if (Lib_Card.Configure.Parameter.Machine_Area4_Type == 1 || Lib_Card.Configure.Parameter.Machine_Area4_Type == 2)
            {
                System.Windows.Forms.Panel panel4 = new System.Windows.Forms.Panel();
                panel4.Size = new System.Drawing.Size(375, 300);
                panel4.Location = new System.Drawing.Point(10, 10 + 300 + 10);
                this.Controls .Add(panel4);

                FADM_Control.DripBeater s = new DripBeater(Lib_Card.Configure.Parameter.Machine_Area4_Row, Lib_Card.Configure.Parameter.Machine_Area4_CupMin, Lib_Card.Configure.Parameter.Machine_Area4_CupMax);
                panel4.Controls.Add(s);
                string s_str1 = null;
                if (Lib_Card.Configure.Parameter.Machine_Area4_Type == 1)
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    {
                        s_str1 = "四号前处理区";
                    }
                    else
                    {
                        s_str1 = "No.4 pre-treatment area";
                    }
                }
                else
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    {
                        s_str1 = "四号滴液区";
                    }
                    else
                    {
                        s_str1 = "No.4 drip area";
                    }
                }

                foreach (Control c1 in s.Controls)
                {

                    if (c1 is GroupBox)
                    {
                        c1.Text = s_str1;
                        foreach (Control c2 in c1.Controls)
                        {
                            if (c2 is SmartDyeing.FADM_Control.Cup)
                            {
                                ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click2;
                                ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave2;
                                _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                            }
                        }
                    }
                }

            }
            else if (Lib_Card.Configure.Parameter.Machine_Area4_Type == 3)
            {
                System.Windows.Forms.Panel panel4 = new System.Windows.Forms.Panel();
                panel4.Size = new System.Drawing.Size(375, 300);
                panel4.Location = new System.Drawing.Point(10, 10 + 300 + 10);
                this.Controls.Add(panel4);

                if (Lib_Card.Configure.Parameter.Machine_Area4_DyeType == 0 || Lib_Card.Configure.Parameter.Machine_Area4_DyeType == 5)
                {
                    FADM_Control.TenBeater s = new TenBeater();
                    panel4.Controls.Add(s);

                    foreach (Control c1 in s.Controls)
                    {
                        if (c1 is GroupBox)
                        {
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            {
                                c1.Text = "四号染固色区";
                            }
                            else
                            {

                                c1.Text = "No.4 dyeing and fixation area";
                            }
                            int i_n = 0;
                            foreach (Control c2 in c1.Controls)
                            {
                                if (c2 is SmartDyeing.FADM_Control.Cup)
                                {
                                    ((SmartDyeing.FADM_Control.Cup)c2).Name = (Lib_Card.Configure.Parameter.Machine_Area4_CupMin + i_n).ToString();
                                    ((SmartDyeing.FADM_Control.Cup)c2).NO = (Lib_Card.Configure.Parameter.Machine_Area4_CupMin + i_n).ToString();
                                    ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click;
                                    ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave;
                                    _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                                    i_n++;
                                }
                                else if (c2 is Label)
                                {
                                    _lis_lab.Add((Label)c2);
                                }
                            }
                        }


                    }
                }
                else
                {
                    //6杯翻转缸
                    if (Lib_Card.Configure.Parameter.Machine_Area4_DyeType == 1)
                    {
                        FADM_Control.Beater s = new Beater();
                        panel4.Controls.Add(s);

                        foreach (Control c1 in s.Controls)
                        {
                            if (c1 is GroupBox)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    c1.Text = "四号染固色区";
                                }
                                else
                                {

                                    c1.Text = "No.4 dyeing and fixation area";
                                }
                                int i_n = 0;
                                foreach (Control c2 in c1.Controls)
                                {
                                    if (c2 is SmartDyeing.FADM_Control.Cup)
                                    {
                                        ((SmartDyeing.FADM_Control.Cup)c2).Name = (Lib_Card.Configure.Parameter.Machine_Area4_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).NO = (Lib_Card.Configure.Parameter.Machine_Area4_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click;
                                        ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave;
                                        _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                                        i_n++;
                                    }
                                    else if (c2 is Label)
                                    {
                                        _lis_lab.Add((Label)c2);
                                    }
                                }
                            }
                        }
                    }
                    //12杯翻转缸/12杯精密机
                    else if (Lib_Card.Configure.Parameter.Machine_Area4_DyeType == 2 || Lib_Card.Configure.Parameter.Machine_Area4_DyeType == 6)
                    {
                        FADM_Control.TwelveBeater s = new TwelveBeater();
                        panel4.Controls.Add(s);

                        foreach (Control c1 in s.Controls)
                        {
                            if (c1 is GroupBox)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    c1.Text = "四号染固色区";
                                }
                                else
                                {

                                    c1.Text = "No.4 dyeing and fixation area";
                                }
                                int i_n = 0;
                                foreach (Control c2 in c1.Controls)
                                {
                                    if (c2 is SmartDyeing.FADM_Control.Cup)
                                    {
                                        ((SmartDyeing.FADM_Control.Cup)c2).Name = (Lib_Card.Configure.Parameter.Machine_Area4_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).NO = (Lib_Card.Configure.Parameter.Machine_Area4_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click;
                                        ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave;
                                        _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                                        i_n++;
                                    }
                                    else if (c2 is Label)
                                    {
                                        _lis_lab.Add((Label)c2);
                                    }
                                }
                            }
                        }
                    }

                    //4杯翻转缸
                    else if (Lib_Card.Configure.Parameter.Machine_Area4_DyeType == 3)
                    {
                        FADM_Control.FourBeater s = new FourBeater();
                        panel4.Controls.Add(s);

                        foreach (Control c1 in s.Controls)
                        {
                            if (c1 is GroupBox)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    c1.Text = "四号染固色区";
                                }
                                else
                                {

                                    c1.Text = "No.4 dyeing and fixation area";
                                }
                                int i_n = 0;
                                foreach (Control c2 in c1.Controls)
                                {
                                    if (c2 is SmartDyeing.FADM_Control.Cup)
                                    {
                                        ((SmartDyeing.FADM_Control.Cup)c2).Name = (Lib_Card.Configure.Parameter.Machine_Area4_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).NO = (Lib_Card.Configure.Parameter.Machine_Area4_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click;
                                        ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave;
                                        _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                                        i_n++;
                                    }
                                    else if (c2 is Label)
                                    {
                                        _lis_lab.Add((Label)c2);
                                    }
                                }
                            }
                        }
                    }

                    //8杯精密机
                    else if (Lib_Card.Configure.Parameter.Machine_Area4_DyeType == 3)
                    {
                        FADM_Control.EightBeater s = new EightBeater();
                        panel4.Controls.Add(s);

                        foreach (Control c1 in s.Controls)
                        {
                            if (c1 is GroupBox)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    c1.Text = "四号染固色区";
                                }
                                else
                                {

                                    c1.Text = "No.4 dyeing and fixation area";
                                }
                                int i_n = 0;
                                foreach (Control c2 in c1.Controls)
                                {
                                    if (c2 is SmartDyeing.FADM_Control.Cup)
                                    {
                                        ((SmartDyeing.FADM_Control.Cup)c2).Name = (Lib_Card.Configure.Parameter.Machine_Area4_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).NO = (Lib_Card.Configure.Parameter.Machine_Area4_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click;
                                        ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave;
                                        _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                                        i_n++;
                                    }
                                    else if (c2 is Label)
                                    {
                                        _lis_lab.Add((Label)c2);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                //加入吸光度模块
                if (Lib_Card.Configure.Parameter.Other_UseAbs == 1)
                {
                    if (!b_insertAbs)
                    {
                        System.Windows.Forms.Panel panel4 = new System.Windows.Forms.Panel();
                        panel4.Size = new System.Drawing.Size(375, 300);
                        panel4.Location = new System.Drawing.Point(10, 10 + 300 + 10);
                        this.Controls.Add(panel4);

                        b_insertAbs = true;
                        FADM_Control.AbsBeater s = new AbsBeater();
                        panel4.Controls.Add(s);

                        foreach (Control c1 in s.Controls)
                        {
                            if (c1 is GroupBox)
                            {

                                foreach (Control c2 in c1.Controls)
                                {
                                    if (c2 is Label)
                                    {
                                        _lis_lab_abs.Add((Label)c2);
                                    }
                                    else if (c2 is Cup)
                                    {
                                        ((Cup)c2).Click += Cup_Click3;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //区域5
            if (Lib_Card.Configure.Parameter.Machine_Area5_Type == 1 || Lib_Card.Configure.Parameter.Machine_Area5_Type == 2)
            {
                System.Windows.Forms.Panel panel5 = new System.Windows.Forms.Panel();
                panel5.Size = new System.Drawing.Size(375, 300);
                panel5.Location = new System.Drawing.Point(10 + 375 + 10, 10 + 300 + 10);
                this.Controls.Add (panel5);

                FADM_Control.DripBeater s = new DripBeater(Lib_Card.Configure.Parameter.Machine_Area5_Row, Lib_Card.Configure.Parameter.Machine_Area5_CupMin, Lib_Card.Configure.Parameter.Machine_Area5_CupMax);
                panel5.Controls.Add(s);
                string s_str1 = null;
                if (Lib_Card.Configure.Parameter.Machine_Area5_Type == 1)
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    {
                        s_str1 = "五号前处理区";
                    }
                    else
                    {
                        s_str1 = "No.5 pre-treatment area";
                    }
                }
                else
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    {
                        s_str1 = "五号滴液区";
                    }
                    else
                    {
                        s_str1 = "No.5 drip area";
                    }
                }

                foreach (Control c1 in s.Controls)
                {

                    if (c1 is GroupBox)
                    {
                        c1.Text = s_str1;
                        foreach (Control c2 in c1.Controls)
                        {
                            if (c2 is SmartDyeing.FADM_Control.Cup)
                            {
                                ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click2;
                                ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave2;
                                _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                            }
                        }
                    }
                }

            }
            else if (Lib_Card.Configure.Parameter.Machine_Area5_Type == 3)
            {
                System.Windows.Forms.Panel panel5 = new System.Windows.Forms.Panel();
                panel5.Size = new System.Drawing.Size(375, 300);
                panel5.Location = new System.Drawing.Point(10 + 375 + 10, 10 + 300 + 10);
                this.Controls.Add(panel5);

                if (Lib_Card.Configure.Parameter.Machine_Area5_DyeType == 0 || Lib_Card.Configure.Parameter.Machine_Area5_DyeType == 5)
                {
                    FADM_Control.TenBeater s = new TenBeater();
                    panel5.Controls.Add(s);

                    foreach (Control c1 in s.Controls)
                    {
                        if (c1 is GroupBox)
                        {
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            {
                                c1.Text = "五号染固色区";
                            }
                            else
                            {

                                c1.Text = "No.5 dyeing and fixation area";
                            }
                            int i_n = 0;
                            foreach (Control c2 in c1.Controls)
                            {
                                if (c2 is SmartDyeing.FADM_Control.Cup)
                                {
                                    ((SmartDyeing.FADM_Control.Cup)c2).Name = (Lib_Card.Configure.Parameter.Machine_Area5_CupMin + i_n).ToString();
                                    ((SmartDyeing.FADM_Control.Cup)c2).NO = (Lib_Card.Configure.Parameter.Machine_Area5_CupMin + i_n).ToString();
                                    ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click;
                                    ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave;
                                    _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                                    i_n++;
                                }
                                else if (c2 is Label)
                                {
                                    _lis_lab.Add((Label)c2);
                                }
                            }
                        }


                    }
                }
                else
                {
                    //6杯翻转缸
                    if (Lib_Card.Configure.Parameter.Machine_Area5_DyeType == 1)
                    {
                        FADM_Control.Beater s = new Beater();
                        panel5.Controls.Add(s);

                        foreach (Control c1 in s.Controls)
                        {
                            if (c1 is GroupBox)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    c1.Text = "五号染固色区";
                                }
                                else
                                {

                                    c1.Text = "No.5 dyeing and fixation area";
                                }
                                int i_n = 0;
                                foreach (Control c2 in c1.Controls)
                                {
                                    if (c2 is SmartDyeing.FADM_Control.Cup)
                                    {
                                        ((SmartDyeing.FADM_Control.Cup)c2).Name = (Lib_Card.Configure.Parameter.Machine_Area5_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).NO = (Lib_Card.Configure.Parameter.Machine_Area5_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click;
                                        ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave;
                                        _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                                        i_n++;
                                    }
                                    else if (c2 is Label)
                                    {
                                        _lis_lab.Add((Label)c2);
                                    }
                                }
                            }
                        }
                    }
                    //12杯翻转缸/12杯精密机
                    else if (Lib_Card.Configure.Parameter.Machine_Area5_DyeType == 2 || Lib_Card.Configure.Parameter.Machine_Area5_DyeType == 6)
                    {
                        FADM_Control.TwelveBeater s = new TwelveBeater();
                        panel5.Controls.Add(s);

                        foreach (Control c1 in s.Controls)
                        {
                            if (c1 is GroupBox)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    c1.Text = "五号染固色区";
                                }
                                else
                                {

                                    c1.Text = "No.5 dyeing and fixation area";
                                }
                                int i_n = 0;
                                foreach (Control c2 in c1.Controls)
                                {
                                    if (c2 is SmartDyeing.FADM_Control.Cup)
                                    {
                                        ((SmartDyeing.FADM_Control.Cup)c2).Name = (Lib_Card.Configure.Parameter.Machine_Area5_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).NO = (Lib_Card.Configure.Parameter.Machine_Area5_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click;
                                        ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave;
                                        _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                                        i_n++;
                                    }
                                    else if (c2 is Label)
                                    {
                                        _lis_lab.Add((Label)c2);
                                    }
                                }
                            }
                        }
                    }

                    //4杯翻转缸
                    else if (Lib_Card.Configure.Parameter.Machine_Area5_DyeType == 3)
                    {
                        FADM_Control.FourBeater s = new FourBeater();
                        panel5.Controls.Add(s);

                        foreach (Control c1 in s.Controls)
                        {
                            if (c1 is GroupBox)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    c1.Text = "五号染固色区";
                                }
                                else
                                {

                                    c1.Text = "No.5 dyeing and fixation area";
                                }
                                int i_n = 0;
                                foreach (Control c2 in c1.Controls)
                                {
                                    if (c2 is SmartDyeing.FADM_Control.Cup)
                                    {
                                        ((SmartDyeing.FADM_Control.Cup)c2).Name = (Lib_Card.Configure.Parameter.Machine_Area5_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).NO = (Lib_Card.Configure.Parameter.Machine_Area5_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click;
                                        ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave;
                                        _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                                        i_n++;
                                    }
                                    else if (c2 is Label)
                                    {
                                        _lis_lab.Add((Label)c2);
                                    }
                                }
                            }
                        }
                    }

                    //8杯精密机
                    else if (Lib_Card.Configure.Parameter.Machine_Area5_DyeType == 7)
                    {
                        FADM_Control.EightBeater s = new EightBeater();
                        panel5.Controls.Add(s);

                        foreach (Control c1 in s.Controls)
                        {
                            if (c1 is GroupBox)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    c1.Text = "五号染固色区";
                                }
                                else
                                {

                                    c1.Text = "No.5 dyeing and fixation area";
                                }
                                int i_n = 0;
                                foreach (Control c2 in c1.Controls)
                                {
                                    if (c2 is SmartDyeing.FADM_Control.Cup)
                                    {
                                        ((SmartDyeing.FADM_Control.Cup)c2).Name = (Lib_Card.Configure.Parameter.Machine_Area5_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).NO = (Lib_Card.Configure.Parameter.Machine_Area5_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click;
                                        ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave;
                                        _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                                        i_n++;
                                    }
                                    else if (c2 is Label)
                                    {
                                        _lis_lab.Add((Label)c2);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                //加入吸光度模块
                if (Lib_Card.Configure.Parameter.Other_UseAbs == 1)
                {
                    if (!b_insertAbs)
                    {
                        System.Windows.Forms.Panel panel5 = new System.Windows.Forms.Panel();
                        panel5.Size = new System.Drawing.Size(375, 300);
                        panel5.Location = new System.Drawing.Point(10 + 375 + 10, 10 + 300 + 10);
                        this.Controls.Add(panel5);
                        b_insertAbs = true;
                        FADM_Control.AbsBeater s = new AbsBeater();
                        panel5.Controls.Add(s);

                        foreach (Control c1 in s.Controls)
                        {
                            if (c1 is GroupBox)
                            {

                                foreach (Control c2 in c1.Controls)
                                {
                                    if (c2 is Label)
                                    {
                                        _lis_lab_abs.Add((Label)c2);
                                    }
                                    else if (c2 is Cup)
                                    {
                                        ((Cup)c2).Click += Cup_Click3;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //区域6
            if (Lib_Card.Configure.Parameter.Machine_Area6_Type == 1 || Lib_Card.Configure.Parameter.Machine_Area6_Type == 2)
            {
                System.Windows.Forms.Panel panel6 = new System.Windows.Forms.Panel();
                panel6.Size = new System.Drawing.Size(375, 300);
                panel6.Location = new System.Drawing.Point(10 + 375 + 10 + 375 + 10, 10 + 300 + 10);
                this.Controls.Add(panel6);

                FADM_Control.DripBeater s = new DripBeater(Lib_Card.Configure.Parameter.Machine_Area6_Row, Lib_Card.Configure.Parameter.Machine_Area6_CupMin, Lib_Card.Configure.Parameter.Machine_Area6_CupMax);
                panel6.Controls.Add(s);
                string s_str1 = null;
                if (Lib_Card.Configure.Parameter.Machine_Area6_Type == 1)
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    {
                        s_str1 = "六号前处理区";
                    }
                    else
                    {
                        s_str1 = "No.6 pre-treatment area";
                    }
                }
                else
                {
                    if (Lib_Card.Configure.Parameter.Other_Language == 0)
                    {
                        s_str1 = "六号滴液区";
                    }
                    else
                    {
                        s_str1 = "No.6 drip area";
                    }
                }

                foreach (Control c1 in s.Controls)
                {

                    if (c1 is GroupBox)
                    {
                        c1.Text = s_str1;
                        foreach (Control c2 in c1.Controls)
                        {
                            if (c2 is SmartDyeing.FADM_Control.Cup)
                            {
                                ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click2;
                                ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave2;
                                _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                            }
                        }
                    }
                }

            }
            else if (Lib_Card.Configure.Parameter.Machine_Area6_Type == 3)
            {
                System.Windows.Forms.Panel panel6 = new System.Windows.Forms.Panel();
                panel6.Size = new System.Drawing.Size(375, 300);
                panel6.Location = new System.Drawing.Point(10 + 375 + 10 + 375 + 10, 10 + 300 + 10);
                this.Controls.Add(panel6);

                if (Lib_Card.Configure.Parameter.Machine_Area6_DyeType == 0 || Lib_Card.Configure.Parameter.Machine_Area6_DyeType == 5)
                {
                    FADM_Control.TenBeater s = new TenBeater();
                    panel6.Controls.Add(s);

                    foreach (Control c1 in s.Controls)
                    {
                        if (c1 is GroupBox)
                        {
                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            {
                                c1.Text = "六号染固色区";
                            }
                            else
                            {

                                c1.Text = "No.6 dyeing and fixation area";
                            }
                            int i_n = 0;
                            foreach (Control c2 in c1.Controls)
                            {
                                if (c2 is SmartDyeing.FADM_Control.Cup)
                                {
                                    ((SmartDyeing.FADM_Control.Cup)c2).Name = (Lib_Card.Configure.Parameter.Machine_Area6_CupMin + i_n).ToString();
                                    ((SmartDyeing.FADM_Control.Cup)c2).NO = (Lib_Card.Configure.Parameter.Machine_Area6_CupMin + i_n).ToString();
                                    ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click;
                                    ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave;
                                    _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                                    i_n++;
                                }
                                else if (c2 is Label)
                                {
                                    _lis_lab.Add((Label)c2);
                                }
                            }
                        }


                    }
                }
                else
                {
                    //6杯翻转缸
                    if (Lib_Card.Configure.Parameter.Machine_Area6_DyeType == 1)
                    {
                        FADM_Control.Beater s = new Beater();
                        panel6.Controls.Add(s);

                        foreach (Control c1 in s.Controls)
                        {
                            if (c1 is GroupBox)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    c1.Text = "六号染固色区";
                                }
                                else
                                {

                                    c1.Text = "No.6 dyeing and fixation area";
                                }
                                int i_n = 0;
                                foreach (Control c2 in c1.Controls)
                                {
                                    if (c2 is SmartDyeing.FADM_Control.Cup)
                                    {
                                        ((SmartDyeing.FADM_Control.Cup)c2).Name = (Lib_Card.Configure.Parameter.Machine_Area6_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).NO = (Lib_Card.Configure.Parameter.Machine_Area6_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click;
                                        ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave;
                                        _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                                        i_n++;
                                    }
                                    else if (c2 is Label)
                                    {
                                        _lis_lab.Add((Label)c2);
                                    }
                                }
                            }
                        }
                    }
                    //12杯翻转缸/12杯精密机
                    else if (Lib_Card.Configure.Parameter.Machine_Area6_DyeType == 2 || Lib_Card.Configure.Parameter.Machine_Area6_DyeType == 6)
                    {
                        FADM_Control.TwelveBeater s = new TwelveBeater();
                        panel6.Controls.Add(s);

                        foreach (Control c1 in s.Controls)
                        {
                            if (c1 is GroupBox)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    c1.Text = "六号染固色区";
                                }
                                else
                                {

                                    c1.Text = "No.6 dyeing and fixation area";
                                }
                                int i_n = 0;
                                foreach (Control c2 in c1.Controls)
                                {
                                    if (c2 is SmartDyeing.FADM_Control.Cup)
                                    {
                                        ((SmartDyeing.FADM_Control.Cup)c2).Name = (Lib_Card.Configure.Parameter.Machine_Area6_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).NO = (Lib_Card.Configure.Parameter.Machine_Area6_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click;
                                        ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave;
                                        _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                                        i_n++;
                                    }
                                    else if (c2 is Label)
                                    {
                                        _lis_lab.Add((Label)c2);
                                    }
                                }
                            }
                        }
                    }

                    //4杯翻转缸
                    else if (Lib_Card.Configure.Parameter.Machine_Area6_DyeType == 3)
                    {
                        FADM_Control.FourBeater s = new FourBeater();
                        panel6.Controls.Add(s);

                        foreach (Control c1 in s.Controls)
                        {
                            if (c1 is GroupBox)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    c1.Text = "六号染固色区";
                                }
                                else
                                {

                                    c1.Text = "No.6 dyeing and fixation area";
                                }
                                int i_n = 0;
                                foreach (Control c2 in c1.Controls)
                                {
                                    if (c2 is SmartDyeing.FADM_Control.Cup)
                                    {
                                        ((SmartDyeing.FADM_Control.Cup)c2).Name = (Lib_Card.Configure.Parameter.Machine_Area6_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).NO = (Lib_Card.Configure.Parameter.Machine_Area6_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click;
                                        ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave;
                                        _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                                        i_n++;
                                    }
                                    else if (c2 is Label)
                                    {
                                        _lis_lab.Add((Label)c2);
                                    }
                                }
                            }
                        }
                    }

                    //8杯精密机
                    else if (Lib_Card.Configure.Parameter.Machine_Area6_DyeType == 7)
                    {
                        FADM_Control.EightBeater s = new EightBeater();
                        panel6.Controls.Add(s);

                        foreach (Control c1 in s.Controls)
                        {
                            if (c1 is GroupBox)
                            {
                                if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                {
                                    c1.Text = "六号染固色区";
                                }
                                else
                                {

                                    c1.Text = "No.6 dyeing and fixation area";
                                }
                                int i_n = 0;
                                foreach (Control c2 in c1.Controls)
                                {
                                    if (c2 is SmartDyeing.FADM_Control.Cup)
                                    {
                                        ((SmartDyeing.FADM_Control.Cup)c2).Name = (Lib_Card.Configure.Parameter.Machine_Area6_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).NO = (Lib_Card.Configure.Parameter.Machine_Area6_CupMin + i_n).ToString();
                                        ((SmartDyeing.FADM_Control.Cup)c2).Click += Cup_Click;
                                        ((SmartDyeing.FADM_Control.Cup)c2).MouseLeave += Cup_MouseLeave;
                                        _lis_cup.Add((SmartDyeing.FADM_Control.Cup)c2);
                                        i_n++;
                                    }
                                    else if (c2 is Label)
                                    {
                                        _lis_lab.Add((Label)c2);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                //加入吸光度模块
                if (Lib_Card.Configure.Parameter.Other_UseAbs == 1)
                {
                    if (!b_insertAbs)
                    {
                        System.Windows.Forms.Panel panel6 = new System.Windows.Forms.Panel();
                        panel6.Size = new System.Drawing.Size(375, 300);
                        panel6.Location = new System.Drawing.Point(10 + 375 + 10 + 375 + 10, 10 + 300 + 10);
                        this.Controls.Add(panel6);

                        b_insertAbs = true;
                        FADM_Control.AbsBeater s = new AbsBeater();
                        panel6.Controls.Add(s);

                        foreach (Control c1 in s.Controls)
                        {
                            if (c1 is GroupBox)
                            {

                                foreach (Control c2 in c1.Controls)
                                {
                                    if (c2 is Label)
                                    {
                                        _lis_lab_abs.Add((Label)c2);
                                    }
                                    else if (c2 is Cup)
                                    {
                                        ((Cup)c2).Click += Cup_Click3;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            

            

            

            

           

            

            //this.Controls.Add(panel1);
            //this.Controls.Add(panel2);
            //this.Controls.Add(panel3);
            //this.Controls.Add(panel4);
            //this.Controls.Add(panel5);
            //this.Controls.Add(panel6);

            //FADM_Control.Beater s = new Beater();
            //s.groupBox1.Text = "1号打板区";
            //panel1.Controls.Add(s);


            //FADM_Control.Beater s2 = new Beater();
            //s2.groupBox1.Text = "2号打板区";
            //panel2.Controls.Add(s2);

            //FADM_Control.Beater s3 = new Beater();
            //s3.groupBox1.Text = "3号打板区";
            //panel3.Controls.Add(s3);

            //FADM_Control.Beater s4 = new Beater();
            //s4.groupBox1.Text = "4号打板区";
            //panel4.Controls.Add(s4);

            //FADM_Control.Beater s5 = new Beater();
            //s5.groupBox1.Text = "5号打板区";
            //panel5.Controls.Add(s5);

            //FADM_Control.Beater s6 = new Beater();
            //s6.groupBox1.Text = "6号打板区";
            //panel6.Controls.Add(s6);
        }


        void but_Click(object sender, EventArgs e)
        {
            Button b =(Button)sender;

            DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("是否确定重置放布区域", "温馨提示", MessageBoxButtons.YesNo, true);

            //重置
            if (dialogResult == DialogResult.Yes)
            {
                string s_sql1 = "Delete from Lay";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql1);

                for(int i = 1;i<=_i_max_num;i++)
                {
                    FADM_Object.Communal._fadmSqlserver.ReviseData("insert into Lay(Number,Status) Values ("+i+",0);");
                }
            }

        }

        void CheckBox_MouseDown(object sender, MouseEventArgs e)
        {
            CheckBox c = (CheckBox)sender;
            _i_checkBox_num = Convert.ToInt32(c.Name.Substring(4, 3));
        }

        void Cup_Click(object sender, EventArgs e)
        {
            SmartDyeing.FADM_Control.Cup Cup = (SmartDyeing.FADM_Control.Cup)sender;
            IntPtr ptr;
            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                ptr = FindWindow(null, "配液杯详细资料");
            else
                ptr = FindWindow(null, "CupDetails");

            if (ptr == IntPtr.Zero)
            {
                string s_num = Cup.NO.ToString();
                string s_state = _lis_lab[Communal._dic_dyecup_index[Convert.ToInt16(s_num)]].Text;
                if (!string.IsNullOrEmpty(s_state))
                {
                    if (s_state != "下线" && s_state != "待机")
                        new FADM_Form.CupDetails(Convert.ToInt16(s_num)).Show();
                }
            }
            else
            {
                //先删除页面，再重新打开
                PostMessage(ptr, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);

                string s_num = Cup.NO.ToString();

                string s_state = _lis_lab[Communal._dic_dyecup_index[Convert.ToInt16(s_num)]].Text;
                if (!string.IsNullOrEmpty(s_state))
                {
                    if (s_state != "下线" && s_state != "待机")
                        new FADM_Form.CupDetails(Convert.ToInt16(s_num)).Show();
                }
            }
        }

        void Cup_Click2(object sender, EventArgs e)
        {
            SmartDyeing.FADM_Control.Cup Cup = (SmartDyeing.FADM_Control.Cup)sender;
            IntPtr ptr;
            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                ptr = FindWindow(null, "配液杯滴液资料");
            else
                ptr = FindWindow(null, "CupDetails");
            if (ptr == IntPtr.Zero)
            {
                string s_num = Cup.NO.ToString();
                new FADM_Form.DripCupDetails(Convert.ToInt16(s_num)).Show();
            }
            else
            {
                //先删除页面，再重新打开
                PostMessage(ptr, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                string s_num = Cup.NO.ToString();
                new FADM_Form.DripCupDetails(Convert.ToInt16(s_num)).Show();
            }
        }

        void Cup_Click3(object sender, EventArgs e)
        {
            SmartDyeing.FADM_Control.Cup Cup = (SmartDyeing.FADM_Control.Cup)sender;
            IntPtr ptr;
            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                ptr = FindWindow(null, "吸光度杯资料");
            else
                ptr = FindWindow(null, "AbsCupDetails");
            if (ptr == IntPtr.Zero)
            {
                string s_num = Cup.NO.ToString();
                new FADM_Form.AbsCupDetails(Convert.ToInt16(s_num)).Show();
            }
            else
            {
                //先删除页面，再重新打开
                PostMessage(ptr, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                string s_num = Cup.NO.ToString();
                new FADM_Form.AbsCupDetails(Convert.ToInt16(s_num)).Show();
            }
        }

        //鼠标离开配液杯事件
        void Cup_MouseLeave(object sender, EventArgs e)
        {
            //IntPtr ptr = FindWindow(null, "配液杯详细资料");
            //if (ptr != IntPtr.Zero)
            //{
            //    PostMessage(ptr, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            //}

        }

        //鼠标离开配液杯事件
        void Cup_MouseLeave2(object sender, EventArgs e)
        {
            //IntPtr ptr = FindWindow(null, "配液杯详细资料");
            //if (ptr != IntPtr.Zero)
            //{
            //    PostMessage(ptr, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            //}

        }

        /// <summary>
        /// 更新配液杯信息
        /// </summary>
        /// <param name="cup"></param>
        private void cup_update()
        {
            lock (this)
            {
                try
                {
                    //获取配液杯资料
                    DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(
                        "SELECT * FROM cup_details order by CupNum;");

                    foreach (DataRow dr1 in dt_data.Rows)
                    {
                        SmartDyeing.FADM_Control.Cup cup = _lis_cup[Communal._dic_cup_index[Convert.ToInt16(dr1["CupNum"].ToString())]];
                        if (Convert.ToInt16(dr1["Type"].ToString()) == 3)
                        {
                            //int i_count = 0;
                            string s_num = dr1["CupNum"].ToString();

                            Label label = _lis_lab[Communal._dic_dyecup_index[Convert.ToInt16(dr1["CupNum"].ToString())]];


                            if (Lib_Card.Configure.Parameter.Other_Language == 0)
                                label.Text = dr1["Statues"].ToString();
                            else
                            {
                                if ("待机" == dr1["Statues"].ToString())
                                {
                                    label.Text = "standby";
                                }
                                else if ("上线" == dr1["Statues"].ToString())
                                {
                                    label.Text = "OnLine";
                                }
                                else if ("下线" == dr1["Statues"].ToString())
                                {
                                    label.Text = "OffLine";
                                }
                                else if ("检查待机状态" == dr1["Statues"].ToString())
                                {
                                    label.Text = "Check standby mode";
                                }
                                else if ("检查历史状态" == dr1["Statues"].ToString())
                                {
                                    label.Text = "Check historical status";
                                }
                                else if ("等待准备状态" == dr1["Statues"].ToString())
                                {
                                    label.Text = "Waiting for preparation status";
                                }
                                else if ("停止中" == dr1["Statues"].ToString())
                                {
                                    label.Text = "Stopping";
                                }
                                else if ("滴液" == dr1["Statues"].ToString())
                                {
                                    label.Text = "Drip";
                                }
                                else if ("滴液成功" == dr1["Statues"].ToString())
                                {
                                    label.Text = "Drip Success";
                                }
                                else if ("滴液失败" == dr1["Statues"].ToString())
                                {
                                    label.Text = "Drip Fail";
                                }
                                else if ("前洗杯" == dr1["Statues"].ToString())
                                {
                                    label.Text = "Front washing cup";
                                }
                                else if ("失败洗杯" == dr1["Statues"].ToString())
                                {
                                    label.Text = "Fail washing cup";
                                }
                                else
                                {
                                    label.Text = dr1["Statues"].ToString();
                                }
                            }
                        }

                        //获取配液杯资料
                        DataTable dt_head = FADM_Object.Communal._fadmSqlserver.GetData(
                            "SELECT * FROM drop_head WHERE CupNum = " + cup.NO + " AND BatchName != '0'; ");

                        if (dt_head.Rows.Count > 0)
                        {

                            //更新当前杯的最大值
                            DataTable dt_drop_details = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT SUM(CAST(ISNULL(ObjectDropWeight,0.00) as numeric(18,2))) FROM drop_details WHERE" +
                                " CupNum = '" + cup.NO + "' " +
                                " AND BottleNum > 0 AND ( BottleNum <= " + _i_machineType + ");");
                            if (0 < dt_drop_details.Rows.Count)
                            {
                                if (dt_drop_details.Rows[0][0] is DBNull)
                                {
                                    if (Convert.ToString(dt_head.Rows[0]["AddWaterChoose"]) == "0")
                                    {
                                        //不加水
                                        cup.maxValue = 1;
                                    }
                                    else
                                    {

                                        cup.maxValue = (decimal)Convert.ToInt16((Convert.ToDouble(0) +
                                                                 (Convert.ToDouble(dt_head.Rows[0]["ObjectAddWaterWeight"]))) > 1 ?
                                                                 (Convert.ToDouble(0) +
                                                                 (Convert.ToDouble(dt_head.Rows[0]["ObjectAddWaterWeight"]))) : 1);

                                    }
                                }
                                else
                                {
                                    if (Convert.ToString(dt_head.Rows[0]["AddWaterChoose"]) == "0")
                                    {
                                        //不加水
                                        cup.maxValue = (decimal)Convert.ToInt16(Convert.ToDouble(dt_drop_details.Rows[0][0]) > 1 ?
                                                          Convert.ToDouble(dt_drop_details.Rows[0][0]) : 1);
                                    }
                                    else
                                    {

                                        cup.maxValue = (decimal)Convert.ToInt16((Convert.ToDouble(dt_drop_details.Rows[0][0]) +
                                                                 (Convert.ToDouble(dt_head.Rows[0]["ObjectAddWaterWeight"]))) > 1 ?
                                                                 (Convert.ToDouble(dt_drop_details.Rows[0][0]) +
                                                                 (Convert.ToDouble(dt_head.Rows[0]["ObjectAddWaterWeight"]))) : 1);

                                    }
                                }
                            }

                            //更新当前杯的文本
                            cup.Title = Convert.ToString(dt_head.Rows[0]["FormulaCode"]);

                            //更新当前杯的液体颜色
                            if (Convert.ToString(dt_head.Rows[0]["CupFinish"]) == "0")
                            {
                                cup.LiquidColor = Color.DeepSkyBlue;

                                //更新当前杯的杯子颜色
                                if (cup.NO == FADM_Object.Communal._i_OptCupNum.ToString())
                                {
                                    if (this._b_cup_twinkle_run == false)
                                    {
                                        Thread P_thd_twinkle = new Thread(cup_twinkle);
                                        P_thd_twinkle.IsBackground = true;
                                        P_thd_twinkle.Start(cup);
                                    }
                                }
                                else
                                {
                                    cup.cupColor = Color.Black;
                                }
                            }
                            else
                            {

                                //完成
                                if (cup.LiquidColor != Color.Red && cup.LiquidColor != Color.Lime)
                                {

                                   

                                    string s_describeChar = dt_head.Rows[0]["DescribeChar"] is DBNull ? "" : dt_head.Rows[0]["DescribeChar"].ToString();
                                    if (s_describeChar.Contains("失败"))
                                    {
                                        cup.LiquidColor = Color.Red;
                                    }
                                    else
                                    {
                                        cup.LiquidColor = Color.Lime;

                                        //再次判断后处理是否有不合格项
                                        string s_sql = "SELECT * from dye_details where AssistantCode is not null And Finish = 1 And  CupNum = " + cup.NO;
                                        DataTable P_dt_data = FADM_Object.Communal._fadmSqlserver.GetData(s_sql);

                                        double d_bl_drop_allow_err = Lib_Card.Configure.Parameter.Other_AErr_Drip;

                                        bool b_err = false;
                                        //判断是否合格
                                        foreach (DataRow dr2 in P_dt_data.Rows)
                                        {
                                            double d_bl_RealErr = Convert.ToDouble(Lib_Card.Configure.Parameter.Machine_IsThousandsBalance == 0 ? string.Format("{0:F}", Convert.ToDouble(dr2["ObjectDropWeight"]) + (dr2["Compensation"] is DBNull ? 0.0 : Convert.ToDouble(dr2["Compensation"])) - Convert.ToDouble(dr2["RealDropWeight"])) : string.Format("{0:F3}", Convert.ToDouble(dr2["ObjectDropWeight"]) + (dr2["Compensation"] is DBNull ? 0.0 : Convert.ToDouble(dr2["Compensation"])) - Convert.ToDouble(dr2["RealDropWeight"])));
                                            d_bl_RealErr = d_bl_RealErr > 0 ? d_bl_RealErr : -d_bl_RealErr;
                                            if (dr2["StandError"] is DBNull)
                                            {
                                                d_bl_drop_allow_err = (dr2["UnitOfAccount"].ToString() == "%" ? Lib_Card.Configure.Parameter.Other_AErr_Drip : Lib_Card.Configure.Parameter.Other_AErr_Drip);
                                            }
                                            else
                                            {
                                                d_bl_drop_allow_err = Convert.ToDouble(dr2["StandError"]);
                                            }
                                            if (d_bl_RealErr > d_bl_drop_allow_err)
                                            {
                                                b_err = true;
                                                break;
                                            }

                                        }
                                        if (b_err)
                                        {
                                            cup.LiquidColor = Color.Red;
                                        }
                                    }
                                }


                            }


                            //更新杯子当前液量

                            DataTable dt_weight = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT SUM(CAST(ISNULL(RealDropWeight,0.00) as numeric(18,2))) FROM drop_details WHERE CupNum = '" + cup.NO + "';");

                            double d_bl_weight_1 = 0;
                            if (dt_weight.Rows[0][0] is DBNull)
                            {

                            }
                            else
                            {
                                if (dt_weight.Rows[0][0].ToString() != "")
                                {
                                    d_bl_weight_1 = Convert.ToDouble(dt_weight.Rows[0][0]);
                                }
                            }
                            double d_bl_weight_2 = 0;

                            if (Convert.ToString(dt_head.Rows[0]["AddWaterFinish"]) == "1")
                            {
                                d_bl_weight_2 = Convert.ToDouble(dt_head.Rows[0][dt_head.Columns["ObjectAddWaterWeight"]]);
                            }


                            double d_bl_value = (d_bl_weight_1 + d_bl_weight_2) > Convert.ToDouble(cup.maxValue) ?
                                Convert.ToDouble(cup.maxValue) : (d_bl_weight_1 + d_bl_weight_2);
                            cup.Value = (decimal)(d_bl_value);

                        }
                        else
                        {
                            cup.Title = null;
                            cup.Value = 0;
                        }
                    }
                }
                catch
                {

                }
            }

        }

        /// <summary>
        /// 放布区更新
        /// </summary>
        /// <param name="cup"></param>
        private void dry_update()
        {
            lock (this)
            {
                try
                {
                    //获取配液杯资料
                    DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(
                        "SELECT * FROM Lay order by Number;");

                    foreach (DataRow dr1 in dt_data.Rows)
                    {
                        
                        int num =Convert.ToInt32( dr1["Number"].ToString() );
                        if (num > _i_max_num)
                            return;
                        if (dr1["Status"].ToString() == "1")
                        {
                            _lis_chk[num - 1].Checked = true;
                        }
                        else
                        {
                            _lis_chk[num - 1].Checked = false;
                        }
                    }
                }
                catch
                {

                }
            }

        }

        private void tmr_Tick(object sender, EventArgs e)
        {
            try
            {
                cup_update();

                if (Lib_Card.Configure.Parameter.Other_UseAbs == 1/* && !FADM_Object.Communal._b_absErr*/ && _lis_lab_abs.Count > 0)
                {
                    abscup_update();
                }
                if (Communal._b_isUseClamp)
                    dry_update();
            }
            catch
            {
                // new Class_Alarm.MyAlarm(ex.Message, "定时器", false);
            }
        }

        //运行中杯子闪烁线程
        private bool _b_cup_twinkle_run = false;

        private void tsm_Update_Click(object sender, EventArgs e)
        {
            if (_lis_chk[_i_checkBox_num-1].Checked)
            {
                _lis_chk[_i_checkBox_num - 1].Checked = false;
                string s_sql1 = "update Lay set Status=0 where Number = " + _i_checkBox_num + " ; ";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql1);
            }
            else
            {
                _lis_chk[_i_checkBox_num - 1].Checked = true;
                string s_sql1 = "update Lay set Status=1 where Number = " + _i_checkBox_num + " ; ";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql1);
            }
        }

        private void tsm_Reset_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = FADM_Form.CustomMessageBox.Show("确定要确定删除吗?删除后会把布重清空", "温馨提示", MessageBoxButtons.YesNo, true);

            if (dialogResult == DialogResult.Yes)
            {
                _lis_chk[_i_checkBox_num - 1].Checked = false;
                string s_sql1 = "update Lay set Status=0 where Number = " + _i_checkBox_num + " ; ";
                FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql1);

                if (Communal._b_isUseCloth)
                {
                    int[] ia_values2 = new int[1];
                    ia_values2[0] = 3;
                    int TXT = _i_checkBox_num ;
                    int bb = 10000 + 3000 - 1 + Convert.ToInt32(TXT) - 1;

                Labelbb:
                    int statte = FADM_Object.Communal.HMIBaClo.Write(bb, ia_values2);
                    if (statte == -1)
                    {
                        //FADM_Form.CustomMessageBox.Show("锁定杯位布重状态失败", "温馨提示", MessageBoxButtons.OK, false);
                        goto Labelbb;
                    }
                    Lib_Log.Log.writeLogException("=======放布后重置布位状态 bb" + bb);
                }
            }
        }

        private void cup_twinkle(object obj)
        {
            this._b_cup_twinkle_run = true;
            while (((SmartDyeing.FADM_Control.Cup)obj).NO == SmartDyeing.FADM_Object.Communal._i_OptCupNum.ToString())
            {
                ((SmartDyeing.FADM_Control.Cup)obj).cupColor = Color.Yellow;
                Thread.Sleep(500);
                ((SmartDyeing.FADM_Control.Cup)obj).cupColor = Color.Black;
                Thread.Sleep(500);
            }
            this._b_cup_twinkle_run = false;
        }

        /// <summary>
        /// 更新吸光度信息
        /// </summary>
        /// <param name="cup"></param>
        private void abscup_update()
        {
            lock (this)
            {
                try
                {
                    //获取配液杯资料
                    DataTable dt_data = FADM_Object.Communal._fadmSqlserver.GetData(
                        "SELECT * FROM abs_cup_details order by CupNum;");

                    foreach (DataRow dr1 in dt_data.Rows)
                    {
                        Label label = _lis_lab_abs[Convert.ToInt16(dr1["CupNum"].ToString()) - 1];


                        if (Lib_Card.Configure.Parameter.Other_Language == 0)
                            label.Text = dr1["Statues"].ToString();
                        else
                        {
                            if ("待机" == dr1["Statues"].ToString())
                            {
                                label.Text = "standby";
                            }
                            else if ("上线" == dr1["Statues"].ToString())
                            {
                                label.Text = "OnLine";
                            }
                            else if ("下线" == dr1["Statues"].ToString())
                            {
                                label.Text = "OffLine";
                            }
                            else if ("检查待机状态" == dr1["Statues"].ToString())
                            {
                                label.Text = "Check standby mode";
                            }
                            else if ("检查历史状态" == dr1["Statues"].ToString())
                            {
                                label.Text = "Check historical status";
                            }
                            else if ("等待准备状态" == dr1["Statues"].ToString())
                            {
                                label.Text = "Waiting for preparation status";
                            }
                            else if ("停止中" == dr1["Statues"].ToString())
                            {
                                label.Text = "Stopping";
                            }
                            else if ("滴液" == dr1["Statues"].ToString())
                            {
                                label.Text = "Drip";
                            }
                            else if ("滴液成功" == dr1["Statues"].ToString())
                            {
                                label.Text = "Drip Success";
                            }
                            else if ("滴液失败" == dr1["Statues"].ToString())
                            {
                                label.Text = "Drip Fail";
                            }
                            else if ("前洗杯" == dr1["Statues"].ToString())
                            {
                                label.Text = "Front washing cup";
                            }
                            else if ("失败洗杯" == dr1["Statues"].ToString())
                            {
                                label.Text = "Fail washing cup";
                            }
                            else
                            {
                                label.Text = dr1["Statues"].ToString();
                            }
                        }




                    }
                }
                catch
                {

                }
            }

        }
    }
}
