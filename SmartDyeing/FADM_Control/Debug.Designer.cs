using System.Drawing;
using System.Windows.Forms;

namespace SmartDyeing.FADM_Control
{
    partial class Debug
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Debug));
            this.grp_move = new System.Windows.Forms.GroupBox();
            this.BtnStop = new System.Windows.Forms.Button();
            this.TxtRPosY = new System.Windows.Forms.TextBox();
            this.TxtRPosX = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.RdoZ = new System.Windows.Forms.RadioButton();
            this.RdoY = new System.Windows.Forms.RadioButton();
            this.RdoX = new System.Windows.Forms.RadioButton();
            this.TxtCPosZ = new System.Windows.Forms.TextBox();
            this.TxtCSpeedZ = new System.Windows.Forms.TextBox();
            this.TxtUpTimeZ = new System.Windows.Forms.TextBox();
            this.TxtHSpeedZ = new System.Windows.Forms.TextBox();
            this.TxtLSpeedZ = new System.Windows.Forms.TextBox();
            this.TxtPulseZ = new System.Windows.Forms.TextBox();
            this.TxtCSpeedY = new System.Windows.Forms.TextBox();
            this.TxtUpTimeY = new System.Windows.Forms.TextBox();
            this.TxtHSpeedY = new System.Windows.Forms.TextBox();
            this.TxtLSpeedY = new System.Windows.Forms.TextBox();
            this.TxtPulseY = new System.Windows.Forms.TextBox();
            this.TxtCSpeedX = new System.Windows.Forms.TextBox();
            this.TxtUpTimeX = new System.Windows.Forms.TextBox();
            this.TxtHSpeedX = new System.Windows.Forms.TextBox();
            this.TxtLSpeedX = new System.Windows.Forms.TextBox();
            this.TxtPulseX = new System.Windows.Forms.TextBox();
            this.BtnMove = new System.Windows.Forms.Button();
            this.BtnHome = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btn_reset_a = new System.Windows.Forms.Button();
            this.LabBalanceValue = new System.Windows.Forms.Label();
            this.grp_in = new System.Windows.Forms.GroupBox();
            this.ChkInPut_Dye_Stop = new System.Windows.Forms.CheckBox();
            this.ChkInPut_Slow_Cylinder_Mid_3 = new System.Windows.Forms.CheckBox();
            this.ChkInPut_Slow_Cylinder_Mid_2 = new System.Windows.Forms.CheckBox();
            this.ChkInPut_SupportCover = new System.Windows.Forms.CheckBox();
            this.ChkInPut_Back = new System.Windows.Forms.CheckBox();
            this.ChkInPut_Slow_Cylinder_Mid = new System.Windows.Forms.CheckBox();
            this.ChkInPut_Cylinder_Block = new System.Windows.Forms.CheckBox();
            this.ChkInPut_Apocenosis_Up = new System.Windows.Forms.CheckBox();
            this.ChkInPut_Decompression_Up = new System.Windows.Forms.CheckBox();
            this.ChkInPut_Block_Out = new System.Windows.Forms.CheckBox();
            this.ChkInPut_Block_In = new System.Windows.Forms.CheckBox();
            this.ChkInPut_Decompression_Down = new System.Windows.Forms.CheckBox();
            this.ChkInPut_Cylinder_Mid = new System.Windows.Forms.CheckBox();
            this.ChkInPut_Y_Ready = new System.Windows.Forms.CheckBox();
            this.ChkInPut_Tray_Out = new System.Windows.Forms.CheckBox();
            this.ChkInPut_Stop = new System.Windows.Forms.CheckBox();
            this.ChkInPut_Sunx_B = new System.Windows.Forms.CheckBox();
            this.ChkInPut_Sunx_A = new System.Windows.Forms.CheckBox();
            this.ChkInPut_Syringe = new System.Windows.Forms.CheckBox();
            this.ChkInPut_Tongs_B = new System.Windows.Forms.CheckBox();
            this.ChkInPut_Tongs_A = new System.Windows.Forms.CheckBox();
            this.ChkInPut_Tray_In = new System.Windows.Forms.CheckBox();
            this.ChkInPut_Cylinder_Down = new System.Windows.Forms.CheckBox();
            this.ChkInPut_Cylinder_Up = new System.Windows.Forms.CheckBox();
            this.ChkInPut_Z_Origin = new System.Windows.Forms.CheckBox();
            this.ChkInPut_Z_Corotation = new System.Windows.Forms.CheckBox();
            this.ChkInPut_Y_Origin = new System.Windows.Forms.CheckBox();
            this.ChkInPut_Y_Alarm = new System.Windows.Forms.CheckBox();
            this.ChkInPut_Y_Reverse = new System.Windows.Forms.CheckBox();
            this.ChkInPut_Y_Corotation = new System.Windows.Forms.CheckBox();
            this.ChkInPut_X_Origin = new System.Windows.Forms.CheckBox();
            this.ChkInPut_X_Alarm = new System.Windows.Forms.CheckBox();
            this.ChkInPut_X_Ready = new System.Windows.Forms.CheckBox();
            this.ChkInPut_X_Reverse = new System.Windows.Forms.CheckBox();
            this.ChkInPut_X_Corotation = new System.Windows.Forms.CheckBox();
            this.grp_out = new System.Windows.Forms.GroupBox();
            this.BtnOutPut_Slow_3 = new System.Windows.Forms.Button();
            this.BtnOutPut_Slow_2 = new System.Windows.Forms.Button();
            this.BtnOutPut_Dissolving = new System.Windows.Forms.Button();
            this.BtnOutPut_Wash_Blow = new System.Windows.Forms.Button();
            this.BtnOutPut_Wash_Out = new System.Windows.Forms.Button();
            this.BtnOutPut_Wash_In = new System.Windows.Forms.Button();
            this.BtnOutPut_Block_Cylinder = new System.Windows.Forms.Button();
            this.BtnOutPut_Slow = new System.Windows.Forms.Button();
            this.BtnOutPut_ResetY = new System.Windows.Forms.Button();
            this.BtnOutPut_ResetX = new System.Windows.Forms.Button();
            this.BtnOutPut_Block_Out = new System.Windows.Forms.Button();
            this.BtnOutPut_Decompression = new System.Windows.Forms.Button();
            this.BtnOutPut_Apocenosis = new System.Windows.Forms.Button();
            this.BtnOutPut_Buzzer = new System.Windows.Forms.Button();
            this.BtnOutPut_Tray = new System.Windows.Forms.Button();
            this.BtnOutPut_Water = new System.Windows.Forms.Button();
            this.BtnOutPut_Waste = new System.Windows.Forms.Button();
            this.BtnOutPut_Blender = new System.Windows.Forms.Button();
            this.BtnOutPut_Cylinder_Down = new System.Windows.Forms.Button();
            this.BtnOutPut_Tongs = new System.Windows.Forms.Button();
            this.BtnOutPut_Y_Power = new System.Windows.Forms.Button();
            this.BtnOutPut_X_Power = new System.Windows.Forms.Button();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.RdoPHTest = new System.Windows.Forms.RadioButton();
            this.RdoWashBottle = new System.Windows.Forms.RadioButton();
            this.RdoSysFinish = new System.Windows.Forms.RadioButton();
            this.RdoPower = new System.Windows.Forms.RadioButton();
            this.RdoBrew = new System.Windows.Forms.RadioButton();
            this.RdoSalinity = new System.Windows.Forms.RadioButton();
            this.RdoPH = new System.Windows.Forms.RadioButton();
            this.RdoSupportCover = new System.Windows.Forms.RadioButton();
            this.RdoExtraction = new System.Windows.Forms.RadioButton();
            this.RdoStandBy = new System.Windows.Forms.RadioButton();
            this.RdoAbs = new System.Windows.Forms.RadioButton();
            this.RdoWash = new System.Windows.Forms.RadioButton();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.RdoWetClamp = new System.Windows.Forms.RadioButton();
            this.RdoDryClamp = new System.Windows.Forms.RadioButton();
            this.RdoWetCloth = new System.Windows.Forms.RadioButton();
            this.RdoDryCloth = new System.Windows.Forms.RadioButton();
            this.RdoStress = new System.Windows.Forms.RadioButton();
            this.RdoDecompression = new System.Windows.Forms.RadioButton();
            this.RdoBalance = new System.Windows.Forms.RadioButton();
            this.RdoCup = new System.Windows.Forms.RadioButton();
            this.RdoBottle = new System.Windows.Forms.RadioButton();
            this.BtnStartMove = new System.Windows.Forms.Button();
            this.TxtNum = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.Tmr = new System.Windows.Forms.Timer(this.components);
            this.grp_move.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.grp_in.SuspendLayout();
            this.grp_out.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // grp_move
            // 
            resources.ApplyResources(this.grp_move, "grp_move");
            this.grp_move.Controls.Add(this.BtnStop);
            this.grp_move.Controls.Add(this.TxtRPosY);
            this.grp_move.Controls.Add(this.TxtRPosX);
            this.grp_move.Controls.Add(this.label7);
            this.grp_move.Controls.Add(this.RdoZ);
            this.grp_move.Controls.Add(this.RdoY);
            this.grp_move.Controls.Add(this.RdoX);
            this.grp_move.Controls.Add(this.TxtCPosZ);
            this.grp_move.Controls.Add(this.TxtCSpeedZ);
            this.grp_move.Controls.Add(this.TxtUpTimeZ);
            this.grp_move.Controls.Add(this.TxtHSpeedZ);
            this.grp_move.Controls.Add(this.TxtLSpeedZ);
            this.grp_move.Controls.Add(this.TxtPulseZ);
            this.grp_move.Controls.Add(this.TxtCSpeedY);
            this.grp_move.Controls.Add(this.TxtUpTimeY);
            this.grp_move.Controls.Add(this.TxtHSpeedY);
            this.grp_move.Controls.Add(this.TxtLSpeedY);
            this.grp_move.Controls.Add(this.TxtPulseY);
            this.grp_move.Controls.Add(this.TxtCSpeedX);
            this.grp_move.Controls.Add(this.TxtUpTimeX);
            this.grp_move.Controls.Add(this.TxtHSpeedX);
            this.grp_move.Controls.Add(this.TxtLSpeedX);
            this.grp_move.Controls.Add(this.TxtPulseX);
            this.grp_move.Controls.Add(this.BtnMove);
            this.grp_move.Controls.Add(this.BtnHome);
            this.grp_move.Controls.Add(this.label6);
            this.grp_move.Controls.Add(this.label5);
            this.grp_move.Controls.Add(this.label4);
            this.grp_move.Controls.Add(this.label3);
            this.grp_move.Controls.Add(this.label2);
            this.grp_move.Controls.Add(this.label1);
            this.grp_move.Name = "grp_move";
            this.grp_move.TabStop = false;
            // 
            // BtnStop
            // 
            resources.ApplyResources(this.BtnStop, "BtnStop");
            this.BtnStop.Name = "BtnStop";
            this.BtnStop.UseVisualStyleBackColor = true;
            this.BtnStop.Click += new System.EventHandler(this.BtnStop_Click);
            // 
            // TxtRPosY
            // 
            resources.ApplyResources(this.TxtRPosY, "TxtRPosY");
            this.TxtRPosY.Name = "TxtRPosY";
            // 
            // TxtRPosX
            // 
            resources.ApplyResources(this.TxtRPosX, "TxtRPosX");
            this.TxtRPosX.Name = "TxtRPosX";
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // RdoZ
            // 
            resources.ApplyResources(this.RdoZ, "RdoZ");
            this.RdoZ.Name = "RdoZ";
            this.RdoZ.TabStop = true;
            this.RdoZ.UseVisualStyleBackColor = true;
            // 
            // RdoY
            // 
            resources.ApplyResources(this.RdoY, "RdoY");
            this.RdoY.Name = "RdoY";
            this.RdoY.TabStop = true;
            this.RdoY.UseVisualStyleBackColor = true;
            // 
            // RdoX
            // 
            resources.ApplyResources(this.RdoX, "RdoX");
            this.RdoX.Name = "RdoX";
            this.RdoX.TabStop = true;
            this.RdoX.UseVisualStyleBackColor = true;
            // 
            // TxtCPosZ
            // 
            resources.ApplyResources(this.TxtCPosZ, "TxtCPosZ");
            this.TxtCPosZ.Name = "TxtCPosZ";
            // 
            // TxtCSpeedZ
            // 
            resources.ApplyResources(this.TxtCSpeedZ, "TxtCSpeedZ");
            this.TxtCSpeedZ.Name = "TxtCSpeedZ";
            // 
            // TxtUpTimeZ
            // 
            resources.ApplyResources(this.TxtUpTimeZ, "TxtUpTimeZ");
            this.TxtUpTimeZ.Name = "TxtUpTimeZ";
            // 
            // TxtHSpeedZ
            // 
            resources.ApplyResources(this.TxtHSpeedZ, "TxtHSpeedZ");
            this.TxtHSpeedZ.Name = "TxtHSpeedZ";
            // 
            // TxtLSpeedZ
            // 
            resources.ApplyResources(this.TxtLSpeedZ, "TxtLSpeedZ");
            this.TxtLSpeedZ.Name = "TxtLSpeedZ";
            // 
            // TxtPulseZ
            // 
            resources.ApplyResources(this.TxtPulseZ, "TxtPulseZ");
            this.TxtPulseZ.Name = "TxtPulseZ";
            // 
            // TxtCSpeedY
            // 
            resources.ApplyResources(this.TxtCSpeedY, "TxtCSpeedY");
            this.TxtCSpeedY.Name = "TxtCSpeedY";
            // 
            // TxtUpTimeY
            // 
            resources.ApplyResources(this.TxtUpTimeY, "TxtUpTimeY");
            this.TxtUpTimeY.Name = "TxtUpTimeY";
            // 
            // TxtHSpeedY
            // 
            resources.ApplyResources(this.TxtHSpeedY, "TxtHSpeedY");
            this.TxtHSpeedY.Name = "TxtHSpeedY";
            // 
            // TxtLSpeedY
            // 
            resources.ApplyResources(this.TxtLSpeedY, "TxtLSpeedY");
            this.TxtLSpeedY.Name = "TxtLSpeedY";
            // 
            // TxtPulseY
            // 
            resources.ApplyResources(this.TxtPulseY, "TxtPulseY");
            this.TxtPulseY.Name = "TxtPulseY";
            // 
            // TxtCSpeedX
            // 
            resources.ApplyResources(this.TxtCSpeedX, "TxtCSpeedX");
            this.TxtCSpeedX.Name = "TxtCSpeedX";
            // 
            // TxtUpTimeX
            // 
            resources.ApplyResources(this.TxtUpTimeX, "TxtUpTimeX");
            this.TxtUpTimeX.Name = "TxtUpTimeX";
            // 
            // TxtHSpeedX
            // 
            resources.ApplyResources(this.TxtHSpeedX, "TxtHSpeedX");
            this.TxtHSpeedX.Name = "TxtHSpeedX";
            // 
            // TxtLSpeedX
            // 
            resources.ApplyResources(this.TxtLSpeedX, "TxtLSpeedX");
            this.TxtLSpeedX.Name = "TxtLSpeedX";
            // 
            // TxtPulseX
            // 
            resources.ApplyResources(this.TxtPulseX, "TxtPulseX");
            this.TxtPulseX.Name = "TxtPulseX";
            // 
            // BtnMove
            // 
            resources.ApplyResources(this.BtnMove, "BtnMove");
            this.BtnMove.Name = "BtnMove";
            this.BtnMove.UseVisualStyleBackColor = true;
            this.BtnMove.Click += new System.EventHandler(this.BtnMove_Click);
            // 
            // BtnHome
            // 
            resources.ApplyResources(this.BtnHome, "BtnHome");
            this.BtnHome.Name = "BtnHome";
            this.BtnHome.UseVisualStyleBackColor = true;
            this.BtnHome.Click += new System.EventHandler(this.BtnHome_Click);
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // groupBox2
            // 
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Controls.Add(this.btn_reset_a);
            this.groupBox2.Controls.Add(this.LabBalanceValue);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // btn_reset_a
            // 
            resources.ApplyResources(this.btn_reset_a, "btn_reset_a");
            this.btn_reset_a.Name = "btn_reset_a";
            this.btn_reset_a.UseVisualStyleBackColor = true;
            this.btn_reset_a.Click += new System.EventHandler(this.btn_reset_a_Click);
            // 
            // LabBalanceValue
            // 
            resources.ApplyResources(this.LabBalanceValue, "LabBalanceValue");
            this.LabBalanceValue.Name = "LabBalanceValue";
            // 
            // grp_in
            // 
            resources.ApplyResources(this.grp_in, "grp_in");
            this.grp_in.Controls.Add(this.ChkInPut_Dye_Stop);
            this.grp_in.Controls.Add(this.ChkInPut_Slow_Cylinder_Mid_3);
            this.grp_in.Controls.Add(this.ChkInPut_Slow_Cylinder_Mid_2);
            this.grp_in.Controls.Add(this.ChkInPut_SupportCover);
            this.grp_in.Controls.Add(this.ChkInPut_Back);
            this.grp_in.Controls.Add(this.ChkInPut_Slow_Cylinder_Mid);
            this.grp_in.Controls.Add(this.ChkInPut_Cylinder_Block);
            this.grp_in.Controls.Add(this.ChkInPut_Apocenosis_Up);
            this.grp_in.Controls.Add(this.ChkInPut_Decompression_Up);
            this.grp_in.Controls.Add(this.ChkInPut_Block_Out);
            this.grp_in.Controls.Add(this.ChkInPut_Block_In);
            this.grp_in.Controls.Add(this.ChkInPut_Decompression_Down);
            this.grp_in.Controls.Add(this.ChkInPut_Cylinder_Mid);
            this.grp_in.Controls.Add(this.ChkInPut_Y_Ready);
            this.grp_in.Controls.Add(this.ChkInPut_Tray_Out);
            this.grp_in.Controls.Add(this.ChkInPut_Stop);
            this.grp_in.Controls.Add(this.ChkInPut_Sunx_B);
            this.grp_in.Controls.Add(this.ChkInPut_Sunx_A);
            this.grp_in.Controls.Add(this.ChkInPut_Syringe);
            this.grp_in.Controls.Add(this.ChkInPut_Tongs_B);
            this.grp_in.Controls.Add(this.ChkInPut_Tongs_A);
            this.grp_in.Controls.Add(this.ChkInPut_Tray_In);
            this.grp_in.Controls.Add(this.ChkInPut_Cylinder_Down);
            this.grp_in.Controls.Add(this.ChkInPut_Cylinder_Up);
            this.grp_in.Controls.Add(this.ChkInPut_Z_Origin);
            this.grp_in.Controls.Add(this.ChkInPut_Z_Corotation);
            this.grp_in.Controls.Add(this.ChkInPut_Y_Origin);
            this.grp_in.Controls.Add(this.ChkInPut_Y_Alarm);
            this.grp_in.Controls.Add(this.ChkInPut_Y_Reverse);
            this.grp_in.Controls.Add(this.ChkInPut_Y_Corotation);
            this.grp_in.Controls.Add(this.ChkInPut_X_Origin);
            this.grp_in.Controls.Add(this.ChkInPut_X_Alarm);
            this.grp_in.Controls.Add(this.ChkInPut_X_Ready);
            this.grp_in.Controls.Add(this.ChkInPut_X_Reverse);
            this.grp_in.Controls.Add(this.ChkInPut_X_Corotation);
            this.grp_in.Name = "grp_in";
            this.grp_in.TabStop = false;
            // 
            // ChkInPut_Dye_Stop
            // 
            resources.ApplyResources(this.ChkInPut_Dye_Stop, "ChkInPut_Dye_Stop");
            this.ChkInPut_Dye_Stop.Name = "ChkInPut_Dye_Stop";
            this.ChkInPut_Dye_Stop.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_Slow_Cylinder_Mid_3
            // 
            resources.ApplyResources(this.ChkInPut_Slow_Cylinder_Mid_3, "ChkInPut_Slow_Cylinder_Mid_3");
            this.ChkInPut_Slow_Cylinder_Mid_3.Name = "ChkInPut_Slow_Cylinder_Mid_3";
            this.ChkInPut_Slow_Cylinder_Mid_3.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_Slow_Cylinder_Mid_2
            // 
            resources.ApplyResources(this.ChkInPut_Slow_Cylinder_Mid_2, "ChkInPut_Slow_Cylinder_Mid_2");
            this.ChkInPut_Slow_Cylinder_Mid_2.Name = "ChkInPut_Slow_Cylinder_Mid_2";
            this.ChkInPut_Slow_Cylinder_Mid_2.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_SupportCover
            // 
            resources.ApplyResources(this.ChkInPut_SupportCover, "ChkInPut_SupportCover");
            this.ChkInPut_SupportCover.Name = "ChkInPut_SupportCover";
            this.ChkInPut_SupportCover.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_Back
            // 
            resources.ApplyResources(this.ChkInPut_Back, "ChkInPut_Back");
            this.ChkInPut_Back.Name = "ChkInPut_Back";
            this.ChkInPut_Back.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_Slow_Cylinder_Mid
            // 
            resources.ApplyResources(this.ChkInPut_Slow_Cylinder_Mid, "ChkInPut_Slow_Cylinder_Mid");
            this.ChkInPut_Slow_Cylinder_Mid.Name = "ChkInPut_Slow_Cylinder_Mid";
            this.ChkInPut_Slow_Cylinder_Mid.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_Cylinder_Block
            // 
            resources.ApplyResources(this.ChkInPut_Cylinder_Block, "ChkInPut_Cylinder_Block");
            this.ChkInPut_Cylinder_Block.Name = "ChkInPut_Cylinder_Block";
            this.ChkInPut_Cylinder_Block.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_Apocenosis_Up
            // 
            resources.ApplyResources(this.ChkInPut_Apocenosis_Up, "ChkInPut_Apocenosis_Up");
            this.ChkInPut_Apocenosis_Up.Name = "ChkInPut_Apocenosis_Up";
            this.ChkInPut_Apocenosis_Up.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_Decompression_Up
            // 
            resources.ApplyResources(this.ChkInPut_Decompression_Up, "ChkInPut_Decompression_Up");
            this.ChkInPut_Decompression_Up.Name = "ChkInPut_Decompression_Up";
            this.ChkInPut_Decompression_Up.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_Block_Out
            // 
            resources.ApplyResources(this.ChkInPut_Block_Out, "ChkInPut_Block_Out");
            this.ChkInPut_Block_Out.Name = "ChkInPut_Block_Out";
            this.ChkInPut_Block_Out.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_Block_In
            // 
            resources.ApplyResources(this.ChkInPut_Block_In, "ChkInPut_Block_In");
            this.ChkInPut_Block_In.Name = "ChkInPut_Block_In";
            this.ChkInPut_Block_In.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_Decompression_Down
            // 
            resources.ApplyResources(this.ChkInPut_Decompression_Down, "ChkInPut_Decompression_Down");
            this.ChkInPut_Decompression_Down.Name = "ChkInPut_Decompression_Down";
            this.ChkInPut_Decompression_Down.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_Cylinder_Mid
            // 
            resources.ApplyResources(this.ChkInPut_Cylinder_Mid, "ChkInPut_Cylinder_Mid");
            this.ChkInPut_Cylinder_Mid.Name = "ChkInPut_Cylinder_Mid";
            this.ChkInPut_Cylinder_Mid.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_Y_Ready
            // 
            resources.ApplyResources(this.ChkInPut_Y_Ready, "ChkInPut_Y_Ready");
            this.ChkInPut_Y_Ready.Name = "ChkInPut_Y_Ready";
            this.ChkInPut_Y_Ready.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_Tray_Out
            // 
            resources.ApplyResources(this.ChkInPut_Tray_Out, "ChkInPut_Tray_Out");
            this.ChkInPut_Tray_Out.Name = "ChkInPut_Tray_Out";
            this.ChkInPut_Tray_Out.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_Stop
            // 
            resources.ApplyResources(this.ChkInPut_Stop, "ChkInPut_Stop");
            this.ChkInPut_Stop.Name = "ChkInPut_Stop";
            this.ChkInPut_Stop.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_Sunx_B
            // 
            resources.ApplyResources(this.ChkInPut_Sunx_B, "ChkInPut_Sunx_B");
            this.ChkInPut_Sunx_B.Name = "ChkInPut_Sunx_B";
            this.ChkInPut_Sunx_B.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_Sunx_A
            // 
            resources.ApplyResources(this.ChkInPut_Sunx_A, "ChkInPut_Sunx_A");
            this.ChkInPut_Sunx_A.Name = "ChkInPut_Sunx_A";
            this.ChkInPut_Sunx_A.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_Syringe
            // 
            resources.ApplyResources(this.ChkInPut_Syringe, "ChkInPut_Syringe");
            this.ChkInPut_Syringe.Name = "ChkInPut_Syringe";
            this.ChkInPut_Syringe.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_Tongs_B
            // 
            resources.ApplyResources(this.ChkInPut_Tongs_B, "ChkInPut_Tongs_B");
            this.ChkInPut_Tongs_B.Name = "ChkInPut_Tongs_B";
            this.ChkInPut_Tongs_B.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_Tongs_A
            // 
            resources.ApplyResources(this.ChkInPut_Tongs_A, "ChkInPut_Tongs_A");
            this.ChkInPut_Tongs_A.Name = "ChkInPut_Tongs_A";
            this.ChkInPut_Tongs_A.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_Tray_In
            // 
            resources.ApplyResources(this.ChkInPut_Tray_In, "ChkInPut_Tray_In");
            this.ChkInPut_Tray_In.Name = "ChkInPut_Tray_In";
            this.ChkInPut_Tray_In.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_Cylinder_Down
            // 
            resources.ApplyResources(this.ChkInPut_Cylinder_Down, "ChkInPut_Cylinder_Down");
            this.ChkInPut_Cylinder_Down.Name = "ChkInPut_Cylinder_Down";
            this.ChkInPut_Cylinder_Down.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_Cylinder_Up
            // 
            resources.ApplyResources(this.ChkInPut_Cylinder_Up, "ChkInPut_Cylinder_Up");
            this.ChkInPut_Cylinder_Up.Name = "ChkInPut_Cylinder_Up";
            this.ChkInPut_Cylinder_Up.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_Z_Origin
            // 
            resources.ApplyResources(this.ChkInPut_Z_Origin, "ChkInPut_Z_Origin");
            this.ChkInPut_Z_Origin.Name = "ChkInPut_Z_Origin";
            this.ChkInPut_Z_Origin.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_Z_Corotation
            // 
            resources.ApplyResources(this.ChkInPut_Z_Corotation, "ChkInPut_Z_Corotation");
            this.ChkInPut_Z_Corotation.Name = "ChkInPut_Z_Corotation";
            this.ChkInPut_Z_Corotation.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_Y_Origin
            // 
            resources.ApplyResources(this.ChkInPut_Y_Origin, "ChkInPut_Y_Origin");
            this.ChkInPut_Y_Origin.Name = "ChkInPut_Y_Origin";
            this.ChkInPut_Y_Origin.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_Y_Alarm
            // 
            resources.ApplyResources(this.ChkInPut_Y_Alarm, "ChkInPut_Y_Alarm");
            this.ChkInPut_Y_Alarm.Name = "ChkInPut_Y_Alarm";
            this.ChkInPut_Y_Alarm.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_Y_Reverse
            // 
            resources.ApplyResources(this.ChkInPut_Y_Reverse, "ChkInPut_Y_Reverse");
            this.ChkInPut_Y_Reverse.Name = "ChkInPut_Y_Reverse";
            this.ChkInPut_Y_Reverse.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_Y_Corotation
            // 
            resources.ApplyResources(this.ChkInPut_Y_Corotation, "ChkInPut_Y_Corotation");
            this.ChkInPut_Y_Corotation.Name = "ChkInPut_Y_Corotation";
            this.ChkInPut_Y_Corotation.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_X_Origin
            // 
            resources.ApplyResources(this.ChkInPut_X_Origin, "ChkInPut_X_Origin");
            this.ChkInPut_X_Origin.Name = "ChkInPut_X_Origin";
            this.ChkInPut_X_Origin.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_X_Alarm
            // 
            resources.ApplyResources(this.ChkInPut_X_Alarm, "ChkInPut_X_Alarm");
            this.ChkInPut_X_Alarm.Name = "ChkInPut_X_Alarm";
            this.ChkInPut_X_Alarm.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_X_Ready
            // 
            resources.ApplyResources(this.ChkInPut_X_Ready, "ChkInPut_X_Ready");
            this.ChkInPut_X_Ready.Name = "ChkInPut_X_Ready";
            this.ChkInPut_X_Ready.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_X_Reverse
            // 
            resources.ApplyResources(this.ChkInPut_X_Reverse, "ChkInPut_X_Reverse");
            this.ChkInPut_X_Reverse.Name = "ChkInPut_X_Reverse";
            this.ChkInPut_X_Reverse.UseVisualStyleBackColor = true;
            // 
            // ChkInPut_X_Corotation
            // 
            resources.ApplyResources(this.ChkInPut_X_Corotation, "ChkInPut_X_Corotation");
            this.ChkInPut_X_Corotation.Name = "ChkInPut_X_Corotation";
            this.ChkInPut_X_Corotation.UseVisualStyleBackColor = true;
            // 
            // grp_out
            // 
            resources.ApplyResources(this.grp_out, "grp_out");
            this.grp_out.Controls.Add(this.BtnOutPut_Slow_3);
            this.grp_out.Controls.Add(this.BtnOutPut_Slow_2);
            this.grp_out.Controls.Add(this.BtnOutPut_Dissolving);
            this.grp_out.Controls.Add(this.BtnOutPut_Wash_Blow);
            this.grp_out.Controls.Add(this.BtnOutPut_Wash_Out);
            this.grp_out.Controls.Add(this.BtnOutPut_Wash_In);
            this.grp_out.Controls.Add(this.BtnOutPut_Block_Cylinder);
            this.grp_out.Controls.Add(this.BtnOutPut_Slow);
            this.grp_out.Controls.Add(this.BtnOutPut_ResetY);
            this.grp_out.Controls.Add(this.BtnOutPut_ResetX);
            this.grp_out.Controls.Add(this.BtnOutPut_Block_Out);
            this.grp_out.Controls.Add(this.BtnOutPut_Decompression);
            this.grp_out.Controls.Add(this.BtnOutPut_Apocenosis);
            this.grp_out.Controls.Add(this.BtnOutPut_Buzzer);
            this.grp_out.Controls.Add(this.BtnOutPut_Tray);
            this.grp_out.Controls.Add(this.BtnOutPut_Water);
            this.grp_out.Controls.Add(this.BtnOutPut_Waste);
            this.grp_out.Controls.Add(this.BtnOutPut_Blender);
            this.grp_out.Controls.Add(this.BtnOutPut_Cylinder_Down);
            this.grp_out.Controls.Add(this.BtnOutPut_Tongs);
            this.grp_out.Controls.Add(this.BtnOutPut_Y_Power);
            this.grp_out.Controls.Add(this.BtnOutPut_X_Power);
            this.grp_out.Name = "grp_out";
            this.grp_out.TabStop = false;
            // 
            // BtnOutPut_Slow_3
            // 
            resources.ApplyResources(this.BtnOutPut_Slow_3, "BtnOutPut_Slow_3");
            this.BtnOutPut_Slow_3.Name = "BtnOutPut_Slow_3";
            this.BtnOutPut_Slow_3.UseVisualStyleBackColor = true;
            this.BtnOutPut_Slow_3.Click += new System.EventHandler(this.BtnOutPut_Slow_3_Click);
            // 
            // BtnOutPut_Slow_2
            // 
            resources.ApplyResources(this.BtnOutPut_Slow_2, "BtnOutPut_Slow_2");
            this.BtnOutPut_Slow_2.Name = "BtnOutPut_Slow_2";
            this.BtnOutPut_Slow_2.UseVisualStyleBackColor = true;
            this.BtnOutPut_Slow_2.Click += new System.EventHandler(this.BtnOutPut_Slow_2_Click);
            // 
            // BtnOutPut_Dissolving
            // 
            resources.ApplyResources(this.BtnOutPut_Dissolving, "BtnOutPut_Dissolving");
            this.BtnOutPut_Dissolving.Name = "BtnOutPut_Dissolving";
            this.BtnOutPut_Dissolving.UseVisualStyleBackColor = true;
            this.BtnOutPut_Dissolving.Click += new System.EventHandler(this.BtnOutPut_Dissolving_Click);
            // 
            // BtnOutPut_Wash_Blow
            // 
            resources.ApplyResources(this.BtnOutPut_Wash_Blow, "BtnOutPut_Wash_Blow");
            this.BtnOutPut_Wash_Blow.Name = "BtnOutPut_Wash_Blow";
            this.BtnOutPut_Wash_Blow.UseVisualStyleBackColor = true;
            this.BtnOutPut_Wash_Blow.Click += new System.EventHandler(this.BtnOutPut_Wash_Blow_Click);
            // 
            // BtnOutPut_Wash_Out
            // 
            resources.ApplyResources(this.BtnOutPut_Wash_Out, "BtnOutPut_Wash_Out");
            this.BtnOutPut_Wash_Out.Name = "BtnOutPut_Wash_Out";
            this.BtnOutPut_Wash_Out.UseVisualStyleBackColor = true;
            this.BtnOutPut_Wash_Out.Click += new System.EventHandler(this.BtnOutPut_Wash_Out_Click);
            // 
            // BtnOutPut_Wash_In
            // 
            resources.ApplyResources(this.BtnOutPut_Wash_In, "BtnOutPut_Wash_In");
            this.BtnOutPut_Wash_In.Name = "BtnOutPut_Wash_In";
            this.BtnOutPut_Wash_In.UseVisualStyleBackColor = true;
            this.BtnOutPut_Wash_In.Click += new System.EventHandler(this.BtnOutPut_Wash_In_Click);
            // 
            // BtnOutPut_Block_Cylinder
            // 
            resources.ApplyResources(this.BtnOutPut_Block_Cylinder, "BtnOutPut_Block_Cylinder");
            this.BtnOutPut_Block_Cylinder.Name = "BtnOutPut_Block_Cylinder";
            this.BtnOutPut_Block_Cylinder.UseVisualStyleBackColor = true;
            this.BtnOutPut_Block_Cylinder.Click += new System.EventHandler(this.BtnOutPut_Block_Cylinder_Click);
            // 
            // BtnOutPut_Slow
            // 
            resources.ApplyResources(this.BtnOutPut_Slow, "BtnOutPut_Slow");
            this.BtnOutPut_Slow.Name = "BtnOutPut_Slow";
            this.BtnOutPut_Slow.UseVisualStyleBackColor = true;
            this.BtnOutPut_Slow.Click += new System.EventHandler(this.BtnOutPut_Slow_Click);
            // 
            // BtnOutPut_ResetY
            // 
            resources.ApplyResources(this.BtnOutPut_ResetY, "BtnOutPut_ResetY");
            this.BtnOutPut_ResetY.Name = "BtnOutPut_ResetY";
            this.BtnOutPut_ResetY.UseVisualStyleBackColor = true;
            this.BtnOutPut_ResetY.Click += new System.EventHandler(this.button4_Click);
            // 
            // BtnOutPut_ResetX
            // 
            resources.ApplyResources(this.BtnOutPut_ResetX, "BtnOutPut_ResetX");
            this.BtnOutPut_ResetX.Name = "BtnOutPut_ResetX";
            this.BtnOutPut_ResetX.UseVisualStyleBackColor = true;
            this.BtnOutPut_ResetX.Click += new System.EventHandler(this.button3_Click);
            // 
            // BtnOutPut_Block_Out
            // 
            resources.ApplyResources(this.BtnOutPut_Block_Out, "BtnOutPut_Block_Out");
            this.BtnOutPut_Block_Out.Name = "BtnOutPut_Block_Out";
            this.BtnOutPut_Block_Out.UseVisualStyleBackColor = true;
            this.BtnOutPut_Block_Out.Click += new System.EventHandler(this.BtnOutPut_Block_Click);
            // 
            // BtnOutPut_Decompression
            // 
            resources.ApplyResources(this.BtnOutPut_Decompression, "BtnOutPut_Decompression");
            this.BtnOutPut_Decompression.Name = "BtnOutPut_Decompression";
            this.BtnOutPut_Decompression.UseVisualStyleBackColor = true;
            this.BtnOutPut_Decompression.Click += new System.EventHandler(this.BtnOutPut_Decompression_Click);
            // 
            // BtnOutPut_Apocenosis
            // 
            resources.ApplyResources(this.BtnOutPut_Apocenosis, "BtnOutPut_Apocenosis");
            this.BtnOutPut_Apocenosis.Name = "BtnOutPut_Apocenosis";
            this.BtnOutPut_Apocenosis.UseVisualStyleBackColor = true;
            this.BtnOutPut_Apocenosis.Click += new System.EventHandler(this.BtnOutPut_Apocenosis_Click);
            // 
            // BtnOutPut_Buzzer
            // 
            resources.ApplyResources(this.BtnOutPut_Buzzer, "BtnOutPut_Buzzer");
            this.BtnOutPut_Buzzer.Name = "BtnOutPut_Buzzer";
            this.BtnOutPut_Buzzer.UseVisualStyleBackColor = true;
            this.BtnOutPut_Buzzer.Click += new System.EventHandler(this.BtnOutPut_Buzzer_Click);
            // 
            // BtnOutPut_Tray
            // 
            resources.ApplyResources(this.BtnOutPut_Tray, "BtnOutPut_Tray");
            this.BtnOutPut_Tray.Name = "BtnOutPut_Tray";
            this.BtnOutPut_Tray.UseVisualStyleBackColor = true;
            this.BtnOutPut_Tray.Click += new System.EventHandler(this.BtnOutPut_Tray_Click);
            // 
            // BtnOutPut_Water
            // 
            resources.ApplyResources(this.BtnOutPut_Water, "BtnOutPut_Water");
            this.BtnOutPut_Water.Name = "BtnOutPut_Water";
            this.BtnOutPut_Water.UseVisualStyleBackColor = true;
            this.BtnOutPut_Water.Click += new System.EventHandler(this.BtnOutPut_Water_Click);
            // 
            // BtnOutPut_Waste
            // 
            resources.ApplyResources(this.BtnOutPut_Waste, "BtnOutPut_Waste");
            this.BtnOutPut_Waste.Name = "BtnOutPut_Waste";
            this.BtnOutPut_Waste.UseVisualStyleBackColor = true;
            this.BtnOutPut_Waste.Click += new System.EventHandler(this.BtnOutPut_Waste_Click);
            // 
            // BtnOutPut_Blender
            // 
            resources.ApplyResources(this.BtnOutPut_Blender, "BtnOutPut_Blender");
            this.BtnOutPut_Blender.Name = "BtnOutPut_Blender";
            this.BtnOutPut_Blender.UseVisualStyleBackColor = true;
            this.BtnOutPut_Blender.Click += new System.EventHandler(this.BtnOutPut_Blender_Click);
            // 
            // BtnOutPut_Cylinder_Down
            // 
            resources.ApplyResources(this.BtnOutPut_Cylinder_Down, "BtnOutPut_Cylinder_Down");
            this.BtnOutPut_Cylinder_Down.Name = "BtnOutPut_Cylinder_Down";
            this.BtnOutPut_Cylinder_Down.UseVisualStyleBackColor = true;
            this.BtnOutPut_Cylinder_Down.Click += new System.EventHandler(this.BtnOutPut_Cylinder_Down_Click);
            // 
            // BtnOutPut_Tongs
            // 
            resources.ApplyResources(this.BtnOutPut_Tongs, "BtnOutPut_Tongs");
            this.BtnOutPut_Tongs.Name = "BtnOutPut_Tongs";
            this.BtnOutPut_Tongs.UseVisualStyleBackColor = true;
            this.BtnOutPut_Tongs.Click += new System.EventHandler(this.BtnOutPut_Tongs_Click);
            // 
            // BtnOutPut_Y_Power
            // 
            resources.ApplyResources(this.BtnOutPut_Y_Power, "BtnOutPut_Y_Power");
            this.BtnOutPut_Y_Power.Name = "BtnOutPut_Y_Power";
            this.BtnOutPut_Y_Power.UseVisualStyleBackColor = true;
            this.BtnOutPut_Y_Power.Click += new System.EventHandler(this.BtnOutPut_Y_Power_Click);
            // 
            // BtnOutPut_X_Power
            // 
            resources.ApplyResources(this.BtnOutPut_X_Power, "BtnOutPut_X_Power");
            this.BtnOutPut_X_Power.Name = "BtnOutPut_X_Power";
            this.BtnOutPut_X_Power.UseVisualStyleBackColor = true;
            this.BtnOutPut_X_Power.Click += new System.EventHandler(this.BtnOutPut_X_Power_Click);
            // 
            // groupBox5
            // 
            resources.ApplyResources(this.groupBox5, "groupBox5");
            this.groupBox5.Controls.Add(this.RdoPHTest);
            this.groupBox5.Controls.Add(this.RdoWashBottle);
            this.groupBox5.Controls.Add(this.RdoSysFinish);
            this.groupBox5.Controls.Add(this.RdoPower);
            this.groupBox5.Controls.Add(this.RdoBrew);
            this.groupBox5.Controls.Add(this.RdoSalinity);
            this.groupBox5.Controls.Add(this.RdoPH);
            this.groupBox5.Controls.Add(this.RdoSupportCover);
            this.groupBox5.Controls.Add(this.RdoExtraction);
            this.groupBox5.Controls.Add(this.RdoStandBy);
            this.groupBox5.Controls.Add(this.RdoAbs);
            this.groupBox5.Controls.Add(this.RdoWash);
            this.groupBox5.Controls.Add(this.button4);
            this.groupBox5.Controls.Add(this.button3);
            this.groupBox5.Controls.Add(this.button2);
            this.groupBox5.Controls.Add(this.RdoWetClamp);
            this.groupBox5.Controls.Add(this.RdoDryClamp);
            this.groupBox5.Controls.Add(this.RdoWetCloth);
            this.groupBox5.Controls.Add(this.RdoDryCloth);
            this.groupBox5.Controls.Add(this.RdoStress);
            this.groupBox5.Controls.Add(this.RdoDecompression);
            this.groupBox5.Controls.Add(this.RdoBalance);
            this.groupBox5.Controls.Add(this.RdoCup);
            this.groupBox5.Controls.Add(this.RdoBottle);
            this.groupBox5.Controls.Add(this.BtnStartMove);
            this.groupBox5.Controls.Add(this.TxtNum);
            this.groupBox5.Controls.Add(this.label12);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.TabStop = false;
            // 
            // RdoPHTest
            // 
            resources.ApplyResources(this.RdoPHTest, "RdoPHTest");
            this.RdoPHTest.Name = "RdoPHTest";
            this.RdoPHTest.TabStop = true;
            this.RdoPHTest.UseVisualStyleBackColor = true;
            // 
            // RdoWashBottle
            // 
            resources.ApplyResources(this.RdoWashBottle, "RdoWashBottle");
            this.RdoWashBottle.Name = "RdoWashBottle";
            this.RdoWashBottle.TabStop = true;
            this.RdoWashBottle.UseVisualStyleBackColor = true;
            // 
            // RdoSysFinish
            // 
            resources.ApplyResources(this.RdoSysFinish, "RdoSysFinish");
            this.RdoSysFinish.Name = "RdoSysFinish";
            this.RdoSysFinish.TabStop = true;
            this.RdoSysFinish.UseVisualStyleBackColor = true;
            // 
            // RdoPower
            // 
            resources.ApplyResources(this.RdoPower, "RdoPower");
            this.RdoPower.Name = "RdoPower";
            this.RdoPower.TabStop = true;
            this.RdoPower.UseVisualStyleBackColor = true;
            // 
            // RdoBrew
            // 
            resources.ApplyResources(this.RdoBrew, "RdoBrew");
            this.RdoBrew.Name = "RdoBrew";
            this.RdoBrew.TabStop = true;
            this.RdoBrew.UseVisualStyleBackColor = true;
            // 
            // RdoSalinity
            // 
            resources.ApplyResources(this.RdoSalinity, "RdoSalinity");
            this.RdoSalinity.Name = "RdoSalinity";
            this.RdoSalinity.TabStop = true;
            this.RdoSalinity.UseVisualStyleBackColor = true;
            // 
            // RdoPH
            // 
            resources.ApplyResources(this.RdoPH, "RdoPH");
            this.RdoPH.Name = "RdoPH";
            this.RdoPH.TabStop = true;
            this.RdoPH.UseVisualStyleBackColor = true;
            // 
            // RdoSupportCover
            // 
            resources.ApplyResources(this.RdoSupportCover, "RdoSupportCover");
            this.RdoSupportCover.Name = "RdoSupportCover";
            this.RdoSupportCover.TabStop = true;
            this.RdoSupportCover.UseVisualStyleBackColor = true;
            // 
            // RdoExtraction
            // 
            resources.ApplyResources(this.RdoExtraction, "RdoExtraction");
            this.RdoExtraction.Name = "RdoExtraction";
            this.RdoExtraction.TabStop = true;
            this.RdoExtraction.UseVisualStyleBackColor = true;
            // 
            // RdoStandBy
            // 
            resources.ApplyResources(this.RdoStandBy, "RdoStandBy");
            this.RdoStandBy.Name = "RdoStandBy";
            this.RdoStandBy.TabStop = true;
            this.RdoStandBy.UseVisualStyleBackColor = true;
            // 
            // RdoAbs
            // 
            resources.ApplyResources(this.RdoAbs, "RdoAbs");
            this.RdoAbs.Name = "RdoAbs";
            this.RdoAbs.TabStop = true;
            this.RdoAbs.UseVisualStyleBackColor = true;
            // 
            // RdoWash
            // 
            resources.ApplyResources(this.RdoWash, "RdoWash");
            this.RdoWash.Name = "RdoWash";
            this.RdoWash.TabStop = true;
            this.RdoWash.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            resources.ApplyResources(this.button4, "button4");
            this.button4.Name = "button4";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click_1);
            // 
            // button3
            // 
            resources.ApplyResources(this.button3, "button3");
            this.button3.Name = "button3";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click_1);
            // 
            // button2
            // 
            resources.ApplyResources(this.button2, "button2");
            this.button2.Name = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click_2);
            // 
            // RdoWetClamp
            // 
            resources.ApplyResources(this.RdoWetClamp, "RdoWetClamp");
            this.RdoWetClamp.Name = "RdoWetClamp";
            this.RdoWetClamp.TabStop = true;
            this.RdoWetClamp.UseVisualStyleBackColor = true;
            // 
            // RdoDryClamp
            // 
            resources.ApplyResources(this.RdoDryClamp, "RdoDryClamp");
            this.RdoDryClamp.Name = "RdoDryClamp";
            this.RdoDryClamp.TabStop = true;
            this.RdoDryClamp.UseVisualStyleBackColor = true;
            // 
            // RdoWetCloth
            // 
            resources.ApplyResources(this.RdoWetCloth, "RdoWetCloth");
            this.RdoWetCloth.Name = "RdoWetCloth";
            this.RdoWetCloth.TabStop = true;
            this.RdoWetCloth.UseVisualStyleBackColor = true;
            // 
            // RdoDryCloth
            // 
            resources.ApplyResources(this.RdoDryCloth, "RdoDryCloth");
            this.RdoDryCloth.Name = "RdoDryCloth";
            this.RdoDryCloth.TabStop = true;
            this.RdoDryCloth.UseVisualStyleBackColor = true;
            // 
            // RdoStress
            // 
            resources.ApplyResources(this.RdoStress, "RdoStress");
            this.RdoStress.Name = "RdoStress";
            this.RdoStress.TabStop = true;
            this.RdoStress.UseVisualStyleBackColor = true;
            // 
            // RdoDecompression
            // 
            resources.ApplyResources(this.RdoDecompression, "RdoDecompression");
            this.RdoDecompression.Name = "RdoDecompression";
            this.RdoDecompression.TabStop = true;
            this.RdoDecompression.UseVisualStyleBackColor = true;
            // 
            // RdoBalance
            // 
            resources.ApplyResources(this.RdoBalance, "RdoBalance");
            this.RdoBalance.Name = "RdoBalance";
            this.RdoBalance.TabStop = true;
            this.RdoBalance.UseVisualStyleBackColor = true;
            // 
            // RdoCup
            // 
            resources.ApplyResources(this.RdoCup, "RdoCup");
            this.RdoCup.Name = "RdoCup";
            this.RdoCup.TabStop = true;
            this.RdoCup.UseVisualStyleBackColor = true;
            // 
            // RdoBottle
            // 
            resources.ApplyResources(this.RdoBottle, "RdoBottle");
            this.RdoBottle.Name = "RdoBottle";
            this.RdoBottle.TabStop = true;
            this.RdoBottle.UseVisualStyleBackColor = true;
            // 
            // BtnStartMove
            // 
            resources.ApplyResources(this.BtnStartMove, "BtnStartMove");
            this.BtnStartMove.Name = "BtnStartMove";
            this.BtnStartMove.UseVisualStyleBackColor = true;
            this.BtnStartMove.Click += new System.EventHandler(this.BtnStartMove_Click);
            // 
            // TxtNum
            // 
            resources.ApplyResources(this.TxtNum, "TxtNum");
            this.TxtNum.Name = "TxtNum";
            // 
            // label12
            // 
            resources.ApplyResources(this.label12, "label12");
            this.label12.Name = "label12";
            // 
            // Tmr
            // 
            this.Tmr.Enabled = true;
            this.Tmr.Tick += new System.EventHandler(this.Tmr_Tick);
            // 
            // Debug
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.grp_out);
            this.Controls.Add(this.grp_in);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.grp_move);
            this.Name = "Debug";
            this.Load += new System.EventHandler(this.Debug_Load);
            this.Leave += new System.EventHandler(this.Debug_Leave);
            this.grp_move.ResumeLayout(false);
            this.grp_move.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.grp_in.ResumeLayout(false);
            this.grp_in.PerformLayout();
            this.grp_out.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grp_move;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox grp_in;
        private System.Windows.Forms.GroupBox grp_out;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TxtPulseX;
        private System.Windows.Forms.Button BtnMove;
        private System.Windows.Forms.Button BtnHome;
        private System.Windows.Forms.TextBox TxtCPosZ;
        private System.Windows.Forms.TextBox TxtCSpeedZ;
        private System.Windows.Forms.TextBox TxtUpTimeZ;
        private System.Windows.Forms.TextBox TxtHSpeedZ;
        private System.Windows.Forms.TextBox TxtLSpeedZ;
        private System.Windows.Forms.TextBox TxtPulseZ;
        private System.Windows.Forms.TextBox TxtCSpeedY;
        private System.Windows.Forms.TextBox TxtUpTimeY;
        private System.Windows.Forms.TextBox TxtHSpeedY;
        private System.Windows.Forms.TextBox TxtLSpeedY;
        private System.Windows.Forms.TextBox TxtPulseY;
        private System.Windows.Forms.TextBox TxtCSpeedX;
        private System.Windows.Forms.TextBox TxtUpTimeX;
        private System.Windows.Forms.TextBox TxtHSpeedX;
        private System.Windows.Forms.TextBox TxtLSpeedX;
        private System.Windows.Forms.Label LabBalanceValue;
        private System.Windows.Forms.CheckBox ChkInPut_Sunx_A;
        private System.Windows.Forms.CheckBox ChkInPut_Syringe;
        private System.Windows.Forms.CheckBox ChkInPut_Tongs_B;
        private System.Windows.Forms.CheckBox ChkInPut_Tongs_A;
        private System.Windows.Forms.CheckBox ChkInPut_Tray_In;
        private System.Windows.Forms.CheckBox ChkInPut_Cylinder_Down;
        private System.Windows.Forms.CheckBox ChkInPut_Cylinder_Up;
        private System.Windows.Forms.CheckBox ChkInPut_Z_Origin;
        private System.Windows.Forms.CheckBox ChkInPut_Z_Corotation;
        private System.Windows.Forms.CheckBox ChkInPut_Y_Origin;
        private System.Windows.Forms.CheckBox ChkInPut_Y_Alarm;
        private System.Windows.Forms.CheckBox ChkInPut_Y_Reverse;
        private System.Windows.Forms.CheckBox ChkInPut_Y_Corotation;
        private System.Windows.Forms.CheckBox ChkInPut_X_Origin;
        private System.Windows.Forms.CheckBox ChkInPut_X_Alarm;
        private System.Windows.Forms.CheckBox ChkInPut_X_Ready;
        private System.Windows.Forms.CheckBox ChkInPut_X_Reverse;
        private System.Windows.Forms.CheckBox ChkInPut_X_Corotation;
        private System.Windows.Forms.CheckBox ChkInPut_Stop;
        private System.Windows.Forms.CheckBox ChkInPut_Sunx_B;
        private System.Windows.Forms.Button BtnOutPut_Cylinder_Down;
        private System.Windows.Forms.Button BtnOutPut_Tongs;
        private System.Windows.Forms.Button BtnOutPut_Y_Power;
        private System.Windows.Forms.Button BtnOutPut_X_Power;
        private System.Windows.Forms.Button BtnOutPut_Buzzer;
        private System.Windows.Forms.Button BtnOutPut_Tray;
        private System.Windows.Forms.Button BtnOutPut_Water;
        private System.Windows.Forms.Button BtnOutPut_Waste;
        private System.Windows.Forms.Button BtnOutPut_Blender;
        private System.Windows.Forms.Button BtnStartMove;
        private System.Windows.Forms.TextBox TxtNum;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Timer Tmr;
        private System.Windows.Forms.RadioButton RdoZ;
        private System.Windows.Forms.RadioButton RdoY;
        private System.Windows.Forms.RadioButton RdoX;
        private System.Windows.Forms.CheckBox ChkInPut_Tray_Out;
        private System.Windows.Forms.RadioButton RdoBalance;
        private System.Windows.Forms.RadioButton RdoCup;
        private System.Windows.Forms.RadioButton RdoBottle;
        private CheckBox ChkInPut_Y_Ready;
        private CheckBox ChkInPut_Cylinder_Mid;
        private Button button1;
        private CheckBox ChkInPut_Apocenosis_Up;
        private CheckBox ChkInPut_Decompression_Up;
        private CheckBox ChkInPut_Block_Out;
        private CheckBox ChkInPut_Block_In;
        private CheckBox ChkInPut_Decompression_Down;
        private Button BtnOutPut_Block_Out;
        private Button BtnOutPut_Decompression;
        private Button BtnOutPut_Apocenosis;
        private RadioButton RdoDecompression;
        private TextBox TxtRPosY;
        private TextBox TxtRPosX;
        private Label label7;
        private Button BtnOutPut_ResetY;
        private Button BtnOutPut_ResetX;
        private Button btn_reset_a;
        private RadioButton RdoStress;
        private Button BtnStop;
        private RadioButton RdoWetClamp;
        private RadioButton RdoDryClamp;
        private RadioButton RdoWetCloth;
        private RadioButton RdoDryCloth;
        private CheckBox ChkInPut_Slow_Cylinder_Mid;
        private CheckBox ChkInPut_Cylinder_Block;
        private Button BtnOutPut_Slow;
        private Button BtnOutPut_Block_Cylinder;
        private CheckBox ChkInPut_Back;
        private Button button4;
        private Button button3;
        private Button button2;
        private RadioButton RdoWash;
        private CheckBox ChkInPut_SupportCover;
        private Button BtnOutPut_Wash_In;
        private Button BtnOutPut_Wash_Out;
        private Button BtnOutPut_Wash_Blow;
        private Button BtnOutPut_Dissolving;
        private RadioButton RdoExtraction;
        private RadioButton RdoStandBy;
        private RadioButton RdoAbs;
        private RadioButton RdoSupportCover;
        private CheckBox ChkInPut_Slow_Cylinder_Mid_3;
        private CheckBox ChkInPut_Slow_Cylinder_Mid_2;
        private Button BtnOutPut_Slow_3;
        private Button BtnOutPut_Slow_2;
        private CheckBox ChkInPut_Dye_Stop;
        private RadioButton RdoSalinity;
        private RadioButton RdoPH;
        private RadioButton RdoBrew;
        private RadioButton RdoPower;
        private RadioButton RdoSysFinish;
        private RadioButton RdoWashBottle;
        private RadioButton RdoPHTest;
    }
}
