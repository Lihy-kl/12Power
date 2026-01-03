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
    public partial class AbsHMIMain : UserControl
    {
        public AbsHMIMain()
        {
            InitializeComponent();
            Communal._b_IsABSOpen = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (Convert.ToInt32(Communal._ia_d13088[50].ToString()) == 1)
                {
                    Btn_reqBase_1.BackColor = Color.Green;
                }
                else
                {
                    Btn_reqBase_1.BackColor = Button.DefaultBackColor;
                }
                txt_Status_1.Text = Communal._ia_d13088[51].ToString();
                txt_Beat_1.Text = Communal._ia_d13088[53].ToString();
                txt_LidSignal_1.Text = Communal._ia_d13088[54].ToString();
                txt_StatusCode_1.Text = Communal._ia_d13088[55].ToString();

                if (Convert.ToInt32(Communal._ia_d13088[56].ToString()) == 1)
                {
                    Btn_reqBase_2.BackColor = Color.Green;
                }
                else
                {
                    Btn_reqBase_2.BackColor = Button.DefaultBackColor;
                }
                txt_Status_2.Text = Communal._ia_d13088[57].ToString();
                txt_Beat_2.Text = Communal._ia_d13088[59].ToString();
                txt_LidSignal_2.Text = Communal._ia_d13088[60].ToString();
                txt_StatusCode_2.Text = Communal._ia_d13088[61].ToString();

                if (Convert.ToInt32(Communal._ia_d13088[62].ToString()) == 1)
                {
                    Btn_reqBase_3.BackColor = Color.Green;
                }
                else
                {
                    Btn_reqBase_3.BackColor = Button.DefaultBackColor;
                }
                txt_Status_3.Text = Communal._ia_d13088[63].ToString();
                txt_Beat_3.Text = Communal._ia_d13088[65].ToString();
                txt_LidSignal_3.Text = Communal._ia_d13088[66].ToString();
                txt_StatusCode_3.Text = Communal._ia_d13088[67].ToString();

                if (Convert.ToInt32(Communal._ia_d13088[68].ToString()) == 1)
                {
                    Btn_reqBase_4.BackColor = Color.Green;
                }
                else
                {
                    Btn_reqBase_4.BackColor = Button.DefaultBackColor;
                }
                txt_Status_4.Text = Communal._ia_d13088[69].ToString();
                txt_Beat_4.Text = Communal._ia_d13088[71].ToString();
                txt_LidSignal_4.Text = Communal._ia_d13088[72].ToString();
                txt_StatusCode_4.Text = Communal._ia_d13088[73].ToString();

                txt_WorkstationsStatus.Text = Communal._ia_d13088[74].ToString();
                txt_Receivedbytes.Text = Communal._ia_d13088[76].ToString();
                txt_P3.Text = Communal._ia_d13088[78].ToString();
                txt_CheckBeat.Text = Communal._ia_d13088[75].ToString();
                txt_Times.Text = Communal._ia_d13088[77].ToString();
                txt_P4.Text = Communal._ia_d13088[79].ToString();

                txt_Current_1.Text =(Convert.ToInt32( Communal._ia_d13088[80].ToString())/100.0f).ToString();
                txt_Current_2.Text = (Convert.ToInt32(Communal._ia_d13088[81].ToString()) / 100.0f).ToString();
                txt_Current_3.Text = (Convert.ToInt32(Communal._ia_d13088[82].ToString()) / 100.0f).ToString();
                txt_Current_4.Text = (Convert.ToInt32(Communal._ia_d13088[83].ToString()) / 100.0f).ToString();


                txt_RecReady_1.Text = Communal._ia_d13088[0].ToString();
                txt_RecFinish_1.Text = Communal._ia_d13088[1].ToString();
                txt_RecSave_1.Text = Communal._ia_d13088[2].ToString();
                //txt_RecStopTimes_1.Text = Communal._ia_d13088[3].ToString();
                txt_RecProcess_1.Text = Communal._ia_d13088[4].ToString();
                txt_RecBaseMark_1.Text = Communal._ia_d13088[5].ToString();
                txt_RecExtraction_1.Text = Communal._ia_d13088[6].ToString();
                txt_RecBaseResult_1.Text = Communal._ia_d13088[7].ToString();

                txt_RecReady_2.Text = Communal._ia_d13088[10].ToString();
                txt_RecFinish_2.Text = Communal._ia_d13088[11].ToString();
                txt_RecSave_2.Text = Communal._ia_d13088[12].ToString();
                //txt_RecStopTimes_2.Text = Communal._ia_d13088[13].ToString();
                txt_RecProcess_2.Text = Communal._ia_d13088[14].ToString();
                txt_RecBaseMark_2.Text = Communal._ia_d13088[15].ToString();
                txt_RecExtraction_2.Text = Communal._ia_d13088[16].ToString();
                txt_RecBaseResult_2.Text = Communal._ia_d13088[17].ToString();

                txt_RecReady_3.Text = Communal._ia_d13088[20].ToString();
                txt_RecFinish_3.Text = Communal._ia_d13088[21].ToString();
                txt_RecSave_3.Text = Communal._ia_d13088[22].ToString();
                //txt_RecStopTimes_3.Text = Communal._ia_d13088[23].ToString();
                txt_RecProcess_3.Text = Communal._ia_d13088[24].ToString();
                txt_RecBaseMark_3.Text = Communal._ia_d13088[25].ToString();
                txt_RecExtraction_3.Text = Communal._ia_d13088[26].ToString();
                txt_RecBaseResult_3.Text = Communal._ia_d13088[27].ToString();

                txt_RecReady_4.Text = Communal._ia_d13088[30].ToString();
                txt_RecFinish_4.Text = Communal._ia_d13088[31].ToString();
                txt_RecSave_4.Text = Communal._ia_d13088[32].ToString();
                //txt_RecStopTimes_4.Text = Communal._ia_d13088[33].ToString();
                txt_RecProcess_4.Text = Communal._ia_d13088[34].ToString();
                txt_RecBaseMark_4.Text = Communal._ia_d13088[35].ToString();
                txt_RecExtraction_4.Text = Communal._ia_d13088[36].ToString();
                txt_RecBaseResult_4.Text = Communal._ia_d13088[37].ToString();


                txt_SendStatus_1.Text = Communal._ia_d13188[0].ToString();
                txt_SendRequest_1.Text = Communal._ia_d13188[1].ToString();
                txt_SendRequestSave_1.Text = Communal._ia_d13188[2].ToString();
                //txt_SendBottleNum_1.Text = Communal._ia_d13188[3].ToString();
                //txt_SendDataCount_1.Text = Communal._ia_d13188[4].ToString();
                txt_SendAbnormalCode_1.Text = Communal._ia_d13188[5].ToString();
                txt_SendCutMark_1.Text = Communal._ia_d13188[6].ToString();
                txt_SendHistory_1.Text = Communal._ia_d13188[7].ToString();

                txt_SendStatus_2.Text = Communal._ia_d13188[10].ToString();
                txt_SendRequest_2.Text = Communal._ia_d13188[11].ToString();
                txt_SendRequestSave_2.Text = Communal._ia_d13188[12].ToString();
                //txt_SendBottleNum_2.Text = Communal._ia_d13188[13].ToString();
                //txt_SendDataCount_2.Text = Communal._ia_d13188[14].ToString();
                txt_SendAbnormalCode_2.Text = Communal._ia_d13188[15].ToString();
                txt_SendCutMark_2.Text = Communal._ia_d13188[16].ToString();
                txt_SendHistory_2.Text = Communal._ia_d13188[17].ToString();

                txt_SendStatus_3.Text = Communal._ia_d13188[20].ToString();
                txt_SendRequest_3.Text = Communal._ia_d13188[21].ToString();
                txt_SendRequestSave_3.Text = Communal._ia_d13188[22].ToString();
                //txt_SendBottleNum_3.Text = Communal._ia_d13188[23].ToString();
                //txt_SendDataCount_3.Text = Communal._ia_d13188[24].ToString();
                txt_SendAbnormalCode_3.Text = Communal._ia_d13188[25].ToString();
                txt_SendCutMark_3.Text = Communal._ia_d13188[26].ToString();
                txt_SendHistory_3.Text = Communal._ia_d13188[27].ToString();

                txt_SendStatus_4.Text = Communal._ia_d13188[30].ToString();
                txt_SendRequest_4.Text = Communal._ia_d13188[31].ToString();
                txt_SendRequestSave_4.Text = Communal._ia_d13188[32].ToString();
                //txt_SendBottleNum_4.Text = Communal._ia_d13188[33].ToString();
                //txt_SendDataCount_4.Text = Communal._ia_d13188[34].ToString();
                txt_SendAbnormalCode_4.Text = Communal._ia_d13188[35].ToString();
                txt_SendCutMark_4.Text = Communal._ia_d13188[36].ToString();
                txt_SendHistory_4.Text = Communal._ia_d13188[37].ToString();

                if ((Convert.ToInt32(Communal._ia_d13088[86].ToString()) & 1) > 0)
                {
                    chk_InPut_Close_1.Checked = true;
                }
                else
                {
                    chk_InPut_Close_1.Checked = false;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[86].ToString()) & 2) > 0)
                {
                    chk_InPut_Open_1.Checked = true;
                }
                else
                {
                    chk_InPut_Open_1.Checked = false;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[86].ToString()) & 4) > 0)
                {
                    chk_InPut_Close_2.Checked = true;
                }
                else
                {
                    chk_InPut_Close_2.Checked = false;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[86].ToString()) & 8) > 0)
                {
                    chk_InPut_Open_2.Checked = true;
                }
                else
                {
                    chk_InPut_Open_2.Checked = false;
                }
                if ((Convert.ToInt32(Communal._ia_d13088[86].ToString()) & 16) > 0)
                {
                    chk_InPut_Close_3.Checked = true;
                }
                else
                {
                    chk_InPut_Close_3.Checked = false;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[86].ToString()) & 32) > 0)
                {
                    chk_InPut_Open_3.Checked = true;
                }
                else
                {
                    chk_InPut_Open_3.Checked = false;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[86].ToString()) & 64) > 0)
                {
                    chk_InPut_Close_4.Checked = true;
                }
                else
                {
                    chk_InPut_Close_4.Checked = false;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[86].ToString()) & 128) > 0)
                {
                    chk_InPut_Open_4.Checked = true;
                }
                else
                {
                    chk_InPut_Open_4.Checked = false;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[86].ToString()) & 256) > 0)
                {
                    chk_InPut_Origin.Checked = true;
                }
                else
                {
                    chk_InPut_Origin.Checked = false;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[86].ToString()) & 512) > 0)
                {
                    chk_InPut_One.Checked = true;
                }
                else
                {
                    chk_InPut_One.Checked = false;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[86].ToString()) & 1024) > 0)
                {
                    chk_InPut_Two.Checked = true;
                }
                else
                {
                    chk_InPut_Two.Checked = false;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[88].ToString()) & 1) > 0)
                {
                    chk_OutPut_Pulse.Checked = true;
                }
                else
                {
                    chk_OutPut_Pulse.Checked = false;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[88].ToString()) & 2) > 0)
                {
                    chk_OutPut_Direction.Checked = true;
                }
                else
                {
                    chk_OutPut_Direction.Checked = false;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[88].ToString()) & 4) > 0)
                {
                    chk_OutPut_Drainage_1.Checked = true;
                    Btn_Drainage_1.BackColor = Color.Green;
                }
                else
                {
                    chk_OutPut_Drainage_1.Checked = false;
                    Btn_Drainage_1.BackColor = Button.DefaultBackColor;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[88].ToString()) & 8) > 0)
                {
                    chk_OutPut_Drainage_2.Checked = true;
                    Btn_Drainage_2.BackColor = Color.Green;
                }
                else
                {
                    chk_OutPut_Drainage_2.Checked = false;
                    Btn_Drainage_2.BackColor = Button.DefaultBackColor;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[88].ToString()) & 16) > 0)
                {
                    chk_OutPut_Drainage_3.Checked = true;
                    Btn_Drainage_3.BackColor = Color.Green;
                }
                else
                {
                    chk_OutPut_Drainage_3.Checked = false;
                    Btn_Drainage_3.BackColor = Button.DefaultBackColor;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[88].ToString()) & 32) > 0)
                {
                    chk_OutPut_Drainage_4.Checked = true;
                    Btn_Drainage_4.BackColor = Color.Green;
                }
                else
                {
                    chk_OutPut_Drainage_4.Checked = false;
                    Btn_Drainage_4.BackColor = Button.DefaultBackColor;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[88].ToString()) & 64) > 0)
                {
                    chk_OutPut_Forward_2.Checked = true;
                    Btn_Forward_2.BackColor = Color.Green;
                }
                else
                {
                    chk_OutPut_Forward_2.Checked = false;
                    Btn_Forward_2.BackColor = Button.DefaultBackColor;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[88].ToString()) & 128) > 0)
                {
                    chk_OutPut_Reversal_2.Checked = true;
                    Btn_Reversal_2.BackColor = Color.Green;
                }
                else
                {
                    chk_OutPut_Reversal_2.Checked = false;
                    Btn_Reversal_2.BackColor = Button.DefaultBackColor;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[88].ToString()) & 256) > 0)
                {
                    chk_OutPut_Forward_4.Checked = true;
                    Btn_Forward_4.BackColor = Color.Green;
                }
                else
                {
                    chk_OutPut_Forward_4.Checked = false;
                    Btn_Forward_4.BackColor = Button.DefaultBackColor;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[88].ToString()) & 512) > 0)
                {
                    chk_OutPut_Reversal_4.Checked = true;
                    Btn_Reversal_4.BackColor = Color.Green;
                }
                else
                {
                    chk_OutPut_Reversal_4.Checked = false;
                    Btn_Reversal_4.BackColor = Button.DefaultBackColor;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[88].ToString()) & 1024) > 0)
                {
                    chk_OutPut_Open_1.Checked = true;
                    Btn_OpenCover_1.BackColor = Color.Green;
                }
                else
                {
                    chk_OutPut_Open_1.Checked = false;
                    Btn_OpenCover_1.BackColor = Button.DefaultBackColor;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[88].ToString()) & 2048) > 0)
                {
                    chk_OutPut_Open_2.Checked = true;
                    Btn_OpenCover_2.BackColor = Color.Green;
                }
                else
                {
                    chk_OutPut_Open_2.Checked = false;
                    Btn_OpenCover_2.BackColor = Button.DefaultBackColor;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[88].ToString()) & 4096) > 0)
                {
                    chk_OutPut_Open_3.Checked = true;
                    Btn_OpenCover_3.BackColor = Color.Green;
                }
                else
                {
                    chk_OutPut_Open_3.Checked = false;
                    Btn_OpenCover_3.BackColor = Button.DefaultBackColor;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[88].ToString()) & 8192) > 0)
                {
                    chk_OutPut_Open_4.Checked = true;
                    Btn_OpenCover_4.BackColor = Color.Green;
                }
                else
                {
                    chk_OutPut_Open_4.Checked = false;
                    Btn_OpenCover_4.BackColor = Button.DefaultBackColor;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[88].ToString()) & 16384) > 0)
                {
                    chk_OutPut_Spray_1.Checked = true;
                    Btn_Spray_1.BackColor = Color.Green;
                }
                else
                {
                    chk_OutPut_Spray_1.Checked = false;
                    Btn_Spray_1.BackColor = Button.DefaultBackColor;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[88].ToString()) & 32768) > 0)
                {
                    chk_OutPut_Spray_2.Checked = true;
                    Btn_Spray_2.BackColor = Color.Green;
                }
                else
                {
                    chk_OutPut_Spray_2.Checked = false;
                    Btn_Spray_2.BackColor = Button.DefaultBackColor;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[89].ToString()) & 1) > 0)
                {
                    chk_OutPut_Spray_3.Checked = true;
                    Btn_Spray_3.BackColor = Color.Green;
                }
                else
                {
                    chk_OutPut_Spray_3.Checked = false;
                    Btn_Spray_3.BackColor = Button.DefaultBackColor;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[89].ToString()) & 2) > 0)
                {
                    chk_OutPut_Spray_4.Checked = true;
                    Btn_Spray_4.BackColor = Color.Green;
                }
                else
                {
                    chk_OutPut_Spray_4.Checked = false;
                    Btn_Spray_4.BackColor = Button.DefaultBackColor;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[89].ToString()) & 4) > 0)
                {
                    chk_OutPut_TotalDrainage.Checked = true;
                    Btn_Total.BackColor = Color.Green;
                }
                else
                {
                    chk_OutPut_TotalDrainage.Checked = false;
                    Btn_Total.BackColor = Button.DefaultBackColor;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[89].ToString()) & 8) > 0)
                {
                    chk_OutPut_One.Checked = true;
                    Btn_Extend1.BackColor = Color.Green;
                }
                else
                {
                    chk_OutPut_One.Checked = false;
                    Btn_Extend1.BackColor = Button.DefaultBackColor;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[89].ToString()) & 16) > 0)
                {
                    chk_OutPut_Two.Checked = true;
                    Btn_Extend2.BackColor = Color.Green;
                }
                else
                {
                    chk_OutPut_Two.Checked = false;
                    Btn_Extend2.BackColor = Button.DefaultBackColor;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[89].ToString()) & 32) > 0)
                {
                    chk_OutPut_Stir_1.Checked = true;
                    Btn_Stir_1.BackColor = Color.Green;
                }
                else
                {
                    chk_OutPut_Stir_1.Checked = false;
                    Btn_Stir_1.BackColor = Button.DefaultBackColor;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[89].ToString()) & 64) > 0)
                {
                    chk_OutPut_Stir_2.Checked = true;
                    Btn_Stir_2.BackColor = Color.Green;
                }
                else
                {
                    chk_OutPut_Stir_2.Checked = false;
                    Btn_Stir_2.BackColor = Button.DefaultBackColor;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[89].ToString()) & 128) > 0)
                {
                    chk_OutPut_Stir_3.Checked = true;
                    Btn_Stir_3.BackColor = Color.Green;
                }
                else
                {
                    chk_OutPut_Stir_3.Checked = false;
                    Btn_Stir_3.BackColor = Button.DefaultBackColor;
                }

                if ((Convert.ToInt32(Communal._ia_d13088[89].ToString()) & 256) > 0)
                {
                    chk_OutPut_Stir_4.Checked = true;
                    Btn_Stir_4.BackColor = Color.Green;
                }
                else
                {
                    chk_OutPut_Stir_4.Checked = false;
                    Btn_Stir_4.BackColor = Button.DefaultBackColor;
                }
            }
            catch (Exception ex) {
                //MessageBox.Show(ex.Message);
            }
        }

        private void Btn_OpenCover_1_Click(object sender, EventArgs e)
        {
            int[] values11 = new int[1];
            values11[0] = 1;
            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
            {
                FADM_Object.Communal._tcpModBusAbs.ReConnect();
            }
            FADM_Object.Communal._tcpModBusAbs.Write(13238, values11);
        }

        private void Btn_OpenCover_2_Click(object sender, EventArgs e)
        {
            int[] values11 = new int[1];
            values11[0] = 1;
            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
            {
                FADM_Object.Communal._tcpModBusAbs.ReConnect();
            }
            FADM_Object.Communal._tcpModBusAbs.Write(13243, values11);
        }

        private void Btn_OpenCover_3_Click(object sender, EventArgs e)
        {
            int[] values11 = new int[1];
            values11[0] = 1;
            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
            {
                FADM_Object.Communal._tcpModBusAbs.ReConnect();
            }
            FADM_Object.Communal._tcpModBusAbs.Write(13248, values11);
        }

        private void Btn_OpenCover_4_Click(object sender, EventArgs e)
        {
            int[] values11 = new int[1];
            values11[0] = 1;
            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
            {
                FADM_Object.Communal._tcpModBusAbs.ReConnect();
            }
            FADM_Object.Communal._tcpModBusAbs.Write(13253, values11);
        }

        private void Btn_Drainage_1_Click(object sender, EventArgs e)
        {
            int[] values11 = new int[1];
            values11[0] = 1;
            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
            {
                FADM_Object.Communal._tcpModBusAbs.ReConnect();
            }
            FADM_Object.Communal._tcpModBusAbs.Write(13239, values11);
        }

        private void Btn_Drainage_2_Click(object sender, EventArgs e)
        {
            int[] values11 = new int[1];
            values11[0] = 1;
            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
            {
                FADM_Object.Communal._tcpModBusAbs.ReConnect();
            }
            FADM_Object.Communal._tcpModBusAbs.Write(13244, values11);
        }

        private void Btn_Drainage_3_Click(object sender, EventArgs e)
        {
            int[] values11 = new int[1];
            values11[0] = 1;
            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
            {
                FADM_Object.Communal._tcpModBusAbs.ReConnect();
            }
            FADM_Object.Communal._tcpModBusAbs.Write(13249, values11);
        }

        private void Btn_Drainage_4_Click(object sender, EventArgs e)
        {
            int[] values11 = new int[1];
            values11[0] = 1;
            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
            {
                FADM_Object.Communal._tcpModBusAbs.ReConnect();
            }
            FADM_Object.Communal._tcpModBusAbs.Write(13254, values11);
        }

        private void Btn_Spray_1_Click(object sender, EventArgs e)
        {
            int[] values11 = new int[1];
            values11[0] = 1;
            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
            {
                FADM_Object.Communal._tcpModBusAbs.ReConnect();
            }
            FADM_Object.Communal._tcpModBusAbs.Write(13240, values11);
        }

        private void Btn_Spray_2_Click(object sender, EventArgs e)
        {
            int[] values11 = new int[1];
            values11[0] = 1;
            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
            {
                FADM_Object.Communal._tcpModBusAbs.ReConnect();
            }
            FADM_Object.Communal._tcpModBusAbs.Write(13245, values11);
        }

        private void Btn_Spray_3_Click(object sender, EventArgs e)
        {
            int[] values11 = new int[1];
            values11[0] = 1;
            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
            {
                FADM_Object.Communal._tcpModBusAbs.ReConnect();
            }
            FADM_Object.Communal._tcpModBusAbs.Write(13250, values11);
        }

        private void Btn_Spray_4_Click(object sender, EventArgs e)
        {
            int[] values11 = new int[1];
            values11[0] = 1;
            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
            {
                FADM_Object.Communal._tcpModBusAbs.ReConnect();
            }
            FADM_Object.Communal._tcpModBusAbs.Write(13255, values11);
        }

        private void Btn_Stir_1_Click(object sender, EventArgs e)
        {
            try
            {
                if (txt_1.Text == "")
                {
                    MessageBox.Show("搅拌速度不能空");
                    return;
                }
                int[] values11 = new int[2];
                values11[0] = 1;
                values11[1] = Convert.ToInt32(txt_1.Text);
                if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                {
                    FADM_Object.Communal._tcpModBusAbs.ReConnect();
                }
                FADM_Object.Communal._tcpModBusAbs.Write(13241, values11);
            }
            catch { }
        }

        private void Btn_Stir_2_Click(object sender, EventArgs e)
        {
            try
            {
                if (txt_2.Text == "")
                {
                    MessageBox.Show("搅拌速度不能空");
                    return;
                }
                int[] values11 = new int[2];
                values11[0] = 1;
                values11[1] = Convert.ToInt32(txt_2.Text);
                if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                {
                    FADM_Object.Communal._tcpModBusAbs.ReConnect();
                }
                FADM_Object.Communal._tcpModBusAbs.Write(13246, values11);
            }
            catch { }
        }

        private void Btn_Stir_3_Click(object sender, EventArgs e)
        {
            try
            {
                if (txt_3.Text == "")
                {
                    MessageBox.Show("搅拌速度不能空");
                    return;
                }
                int[] values11 = new int[2];
                values11[0] = 1;
                values11[1] = Convert.ToInt32(txt_3.Text);
                if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                {
                    FADM_Object.Communal._tcpModBusAbs.ReConnect();
                }
                FADM_Object.Communal._tcpModBusAbs.Write(13251, values11);
            }
            catch { }

            
        }

        private void Btn_Stir_4_Click(object sender, EventArgs e)
        {
            try
            {
                if (txt_4.Text == "")
                {
                    MessageBox.Show("搅拌速度不能空");
                    return;
                }
                int[] values11 = new int[2];
                values11[0] = 1;
                values11[1] = Convert.ToInt32(txt_4.Text);
                if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
                {
                    FADM_Object.Communal._tcpModBusAbs.ReConnect();
                }
                FADM_Object.Communal._tcpModBusAbs.Write(13256, values11);
            }
            catch { }
        }

        private void Btn_Forward_2_Click(object sender, EventArgs e)
        {
            int[] values11 = new int[1];
            values11[0] = 1;
            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
            {
                FADM_Object.Communal._tcpModBusAbs.ReConnect();
            }
            FADM_Object.Communal._tcpModBusAbs.Write(13258, values11);
        }

        private void Btn_Forward_4_Click(object sender, EventArgs e)
        {
            int[] values11 = new int[1];
            values11[0] = 1;
            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
            {
                FADM_Object.Communal._tcpModBusAbs.ReConnect();
            }
            FADM_Object.Communal._tcpModBusAbs.Write(13260, values11);
        }

        private void Btn_Reversal_2_Click(object sender, EventArgs e)
        {
            int[] values11 = new int[1];
            values11[0] = 1;
            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
            {
                FADM_Object.Communal._tcpModBusAbs.ReConnect();
            }
            FADM_Object.Communal._tcpModBusAbs.Write(13259, values11);
        }

        private void Btn_Reversal_4_Click(object sender, EventArgs e)
        {
            int[] values11 = new int[1];
            values11[0] = 1;
            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
            {
                FADM_Object.Communal._tcpModBusAbs.ReConnect();
            }
            FADM_Object.Communal._tcpModBusAbs.Write(13261, values11);
        }

        private void Btn_Extend1_Click(object sender, EventArgs e)
        {
            int[] values11 = new int[1];
            values11[0] = 1;
            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
            {
                FADM_Object.Communal._tcpModBusAbs.ReConnect();
            }
            FADM_Object.Communal._tcpModBusAbs.Write(13262, values11);
        }

        private void Btn_Extend2_Click(object sender, EventArgs e)
        {
            int[] values11 = new int[1];
            values11[0] = 1;
            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
            {
                FADM_Object.Communal._tcpModBusAbs.ReConnect();
            }
            FADM_Object.Communal._tcpModBusAbs.Write(13263, values11);
        }

        private void Btn_Total_Click(object sender, EventArgs e)
        {
            int[] values11 = new int[1];
            values11[0] = 1;
            if (!FADM_Object.Communal._tcpModBusAbs._b_Connect)
            {
                FADM_Object.Communal._tcpModBusAbs.ReConnect();
            }
            FADM_Object.Communal._tcpModBusAbs.Write(13264, values11);
        }
    }
}
