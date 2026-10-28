using System.Drawing;
using System.Windows.Forms;

namespace SmartDyeing.FADM_Control
{
    partial class BottleDefin
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BottleDefin));
            this.grp_Browse = new System.Windows.Forms.GroupBox();
            this.dgv_Bottle = new System.Windows.Forms.DataGridView();
            this.grp_BottleDetails = new System.Windows.Forms.GroupBox();
            this.txt_WashSyringeSpan = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbo_DripReserveFirst = new System.Windows.Forms.ComboBox();
            this.lab_DripReserveFirst = new System.Windows.Forms.Label();
            this.cbo_OriginalBottleNum = new System.Windows.Forms.ComboBox();
            this.dtp_BrewingData = new System.Windows.Forms.DateTimePicker();
            this.cbo_BrewingCode = new System.Windows.Forms.ComboBox();
            this.lab_SyringeType = new System.Windows.Forms.Label();
            this.cbo_SyringeType = new System.Windows.Forms.ComboBox();
            this.txt_SettingConcentration = new System.Windows.Forms.TextBox();
            this.cbo_AssistantCode = new System.Windows.Forms.ComboBox();
            this.txt_CurrentWeight = new System.Windows.Forms.TextBox();
            this.txt_DropMinWeight = new System.Windows.Forms.TextBox();
            this.txt_AllowMaxWeight = new System.Windows.Forms.TextBox();
            this.txt_BottleNum = new System.Windows.Forms.TextBox();
            this.lab_OriginalBottleNum = new System.Windows.Forms.Label();
            this.btn_Delete = new System.Windows.Forms.Button();
            this.btn_Insert = new System.Windows.Forms.Button();
            this.btn_Save = new System.Windows.Forms.Button();
            this.lab_AllowMaxWeight = new System.Windows.Forms.Label();
            this.lab_BrewingData = new System.Windows.Forms.Label();
            this.lab_BrewingCode = new System.Windows.Forms.Label();
            this.lab_CurrentWeight = new System.Windows.Forms.Label();
            this.lab_SettingConcentration = new System.Windows.Forms.Label();
            this.lab_DropMinWeight = new System.Windows.Forms.Label();
            this.lab_AssistantCode = new System.Windows.Forms.Label();
            this.lab_BottleNum = new System.Windows.Forms.Label();
            this.grp_AssistantDetails = new System.Windows.Forms.GroupBox();
            this.rdo_3 = new System.Windows.Forms.RadioButton();
            this.rdo_4 = new System.Windows.Forms.RadioButton();
            this.rdo_2 = new System.Windows.Forms.RadioButton();
            this.rdo_1 = new System.Windows.Forms.RadioButton();
            this.txt_AllowMaxColoringConcentration = new System.Windows.Forms.TextBox();
            this.lab_Cost = new System.Windows.Forms.Label();
            this.txt_AllowMinColoringConcentration = new System.Windows.Forms.TextBox();
            this.lab_Intensity = new System.Windows.Forms.Label();
            this.txt_Cost = new System.Windows.Forms.TextBox();
            this.lab_TermOfValidity = new System.Windows.Forms.Label();
            this.txt_Intensity = new System.Windows.Forms.TextBox();
            this.lab_AllowMaxColoringConcentration = new System.Windows.Forms.Label();
            this.txt_TermOfValidity = new System.Windows.Forms.TextBox();
            this.lab_AllowMinColoringConcentration = new System.Windows.Forms.Label();
            this.lab_UnitOfAccount = new System.Windows.Forms.Label();
            this.txt_AssistantType = new System.Windows.Forms.TextBox();
            this.lab_AssistantType = new System.Windows.Forms.Label();
            this.txt_AssistantName = new System.Windows.Forms.TextBox();
            this.lab_AssistantName = new System.Windows.Forms.Label();
            this.cbo_Flowability = new System.Windows.Forms.ComboBox();
            this.lab_Flowability = new System.Windows.Forms.Label();
            this.grp_Browse.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Bottle)).BeginInit();
            this.grp_BottleDetails.SuspendLayout();
            this.grp_AssistantDetails.SuspendLayout();
            this.SuspendLayout();
            // 
            // grp_Browse
            // 
            this.grp_Browse.Controls.Add(this.dgv_Bottle);
            resources.ApplyResources(this.grp_Browse, "grp_Browse");
            this.grp_Browse.Name = "grp_Browse";
            this.grp_Browse.TabStop = false;
            // 
            // dgv_Bottle
            // 
            this.dgv_Bottle.AllowUserToAddRows = false;
            this.dgv_Bottle.AllowUserToDeleteRows = false;
            this.dgv_Bottle.AllowUserToResizeColumns = false;
            this.dgv_Bottle.AllowUserToResizeRows = false;
            this.dgv_Bottle.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgv_Bottle.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            resources.ApplyResources(this.dgv_Bottle, "dgv_Bottle");
            this.dgv_Bottle.MultiSelect = false;
            this.dgv_Bottle.Name = "dgv_Bottle";
            this.dgv_Bottle.ReadOnly = true;
            this.dgv_Bottle.RowHeadersVisible = false;
            this.dgv_Bottle.RowTemplate.Height = 23;
            this.dgv_Bottle.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_Bottle.CurrentCellChanged += new System.EventHandler(this.dgv_Bottle_CurrentCellChanged);
            // 
            // grp_BottleDetails
            // 
            this.grp_BottleDetails.Controls.Add(this.cbo_Flowability);
            this.grp_BottleDetails.Controls.Add(this.lab_Flowability);
            this.grp_BottleDetails.Controls.Add(this.txt_WashSyringeSpan);
            this.grp_BottleDetails.Controls.Add(this.label1);
            this.grp_BottleDetails.Controls.Add(this.cbo_DripReserveFirst);
            this.grp_BottleDetails.Controls.Add(this.lab_DripReserveFirst);
            this.grp_BottleDetails.Controls.Add(this.cbo_OriginalBottleNum);
            this.grp_BottleDetails.Controls.Add(this.dtp_BrewingData);
            this.grp_BottleDetails.Controls.Add(this.cbo_BrewingCode);
            this.grp_BottleDetails.Controls.Add(this.lab_SyringeType);
            this.grp_BottleDetails.Controls.Add(this.cbo_SyringeType);
            this.grp_BottleDetails.Controls.Add(this.txt_SettingConcentration);
            this.grp_BottleDetails.Controls.Add(this.cbo_AssistantCode);
            this.grp_BottleDetails.Controls.Add(this.txt_CurrentWeight);
            this.grp_BottleDetails.Controls.Add(this.txt_DropMinWeight);
            this.grp_BottleDetails.Controls.Add(this.txt_AllowMaxWeight);
            this.grp_BottleDetails.Controls.Add(this.txt_BottleNum);
            this.grp_BottleDetails.Controls.Add(this.lab_OriginalBottleNum);
            this.grp_BottleDetails.Controls.Add(this.btn_Delete);
            this.grp_BottleDetails.Controls.Add(this.btn_Insert);
            this.grp_BottleDetails.Controls.Add(this.btn_Save);
            this.grp_BottleDetails.Controls.Add(this.lab_AllowMaxWeight);
            this.grp_BottleDetails.Controls.Add(this.lab_BrewingData);
            this.grp_BottleDetails.Controls.Add(this.lab_BrewingCode);
            this.grp_BottleDetails.Controls.Add(this.lab_CurrentWeight);
            this.grp_BottleDetails.Controls.Add(this.lab_SettingConcentration);
            this.grp_BottleDetails.Controls.Add(this.lab_DropMinWeight);
            this.grp_BottleDetails.Controls.Add(this.lab_AssistantCode);
            this.grp_BottleDetails.Controls.Add(this.lab_BottleNum);
            resources.ApplyResources(this.grp_BottleDetails, "grp_BottleDetails");
            this.grp_BottleDetails.Name = "grp_BottleDetails";
            this.grp_BottleDetails.TabStop = false;
            // 
            // txt_WashSyringeSpan
            // 
            resources.ApplyResources(this.txt_WashSyringeSpan, "txt_WashSyringeSpan");
            this.txt_WashSyringeSpan.Name = "txt_WashSyringeSpan";
            this.txt_WashSyringeSpan.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_WashSyringeSpan_KeyDown);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // cbo_DripReserveFirst
            // 
            this.cbo_DripReserveFirst.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cbo_DripReserveFirst, "cbo_DripReserveFirst");
            this.cbo_DripReserveFirst.FormattingEnabled = true;
            this.cbo_DripReserveFirst.Items.AddRange(new object[] {
            resources.GetString("cbo_DripReserveFirst.Items"),
            resources.GetString("cbo_DripReserveFirst.Items1")});
            this.cbo_DripReserveFirst.Name = "cbo_DripReserveFirst";
            this.cbo_DripReserveFirst.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cbo_DripReserveFirst_KeyDown);
            // 
            // lab_DripReserveFirst
            // 
            resources.ApplyResources(this.lab_DripReserveFirst, "lab_DripReserveFirst");
            this.lab_DripReserveFirst.Name = "lab_DripReserveFirst";
            // 
            // cbo_OriginalBottleNum
            // 
            this.cbo_OriginalBottleNum.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cbo_OriginalBottleNum, "cbo_OriginalBottleNum");
            this.cbo_OriginalBottleNum.FormattingEnabled = true;
            this.cbo_OriginalBottleNum.Name = "cbo_OriginalBottleNum";
            this.cbo_OriginalBottleNum.Click += new System.EventHandler(this.cbo_OriginalBottleNum_Click);
            this.cbo_OriginalBottleNum.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cbo_OriginalBottleNum_KeyDown);
            // 
            // dtp_BrewingData
            // 
            resources.ApplyResources(this.dtp_BrewingData, "dtp_BrewingData");
            this.dtp_BrewingData.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtp_BrewingData.Name = "dtp_BrewingData";
            this.dtp_BrewingData.ShowUpDown = true;
            // 
            // cbo_BrewingCode
            // 
            this.cbo_BrewingCode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cbo_BrewingCode, "cbo_BrewingCode");
            this.cbo_BrewingCode.FormattingEnabled = true;
            this.cbo_BrewingCode.Name = "cbo_BrewingCode";
            this.cbo_BrewingCode.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cbo_BrewingCode_KeyDown);
            // 
            // lab_SyringeType
            // 
            resources.ApplyResources(this.lab_SyringeType, "lab_SyringeType");
            this.lab_SyringeType.Name = "lab_SyringeType";
            // 
            // cbo_SyringeType
            // 
            this.cbo_SyringeType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cbo_SyringeType, "cbo_SyringeType");
            this.cbo_SyringeType.FormattingEnabled = true;
            this.cbo_SyringeType.Items.AddRange(new object[] {
            resources.GetString("cbo_SyringeType.Items"),
            resources.GetString("cbo_SyringeType.Items1"),
            resources.GetString("cbo_SyringeType.Items2"),
            resources.GetString("cbo_SyringeType.Items3")});
            this.cbo_SyringeType.Name = "cbo_SyringeType";
            this.cbo_SyringeType.TextChanged += new System.EventHandler(this.cbo_SyringeType_TextChanged);
            this.cbo_SyringeType.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cbo_SyringeType_KeyDown);
            this.cbo_SyringeType.Leave += new System.EventHandler(this.cbo_SyringeType_Leave);
            // 
            // txt_SettingConcentration
            // 
            resources.ApplyResources(this.txt_SettingConcentration, "txt_SettingConcentration");
            this.txt_SettingConcentration.Name = "txt_SettingConcentration";
            this.txt_SettingConcentration.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_SettingConcentration_KeyDown);
            this.txt_SettingConcentration.Leave += new System.EventHandler(this.txt_SettingConcentration_Leave);
            // 
            // cbo_AssistantCode
            // 
            resources.ApplyResources(this.cbo_AssistantCode, "cbo_AssistantCode");
            this.cbo_AssistantCode.FormattingEnabled = true;
            this.cbo_AssistantCode.Name = "cbo_AssistantCode";
            this.cbo_AssistantCode.TextChanged += new System.EventHandler(this.cbo_AssistantCode_TextChanged);
            this.cbo_AssistantCode.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cbo_AssistantCode_KeyDown);
            this.cbo_AssistantCode.Leave += new System.EventHandler(this.cbo_AssistantCode_Leave);
            // 
            // txt_CurrentWeight
            // 
            resources.ApplyResources(this.txt_CurrentWeight, "txt_CurrentWeight");
            this.txt_CurrentWeight.Name = "txt_CurrentWeight";
            this.txt_CurrentWeight.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_CurrentWeight_KeyDown);
            // 
            // txt_DropMinWeight
            // 
            resources.ApplyResources(this.txt_DropMinWeight, "txt_DropMinWeight");
            this.txt_DropMinWeight.Name = "txt_DropMinWeight";
            this.txt_DropMinWeight.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_DropMinWeight_KeyDown);
            // 
            // txt_AllowMaxWeight
            // 
            resources.ApplyResources(this.txt_AllowMaxWeight, "txt_AllowMaxWeight");
            this.txt_AllowMaxWeight.Name = "txt_AllowMaxWeight";
            this.txt_AllowMaxWeight.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_AllowMaxWeight_KeyDown);
            // 
            // txt_BottleNum
            // 
            resources.ApplyResources(this.txt_BottleNum, "txt_BottleNum");
            this.txt_BottleNum.Name = "txt_BottleNum";
            this.txt_BottleNum.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_BottleNum_KeyDown);
            // 
            // lab_OriginalBottleNum
            // 
            resources.ApplyResources(this.lab_OriginalBottleNum, "lab_OriginalBottleNum");
            this.lab_OriginalBottleNum.Name = "lab_OriginalBottleNum";
            // 
            // btn_Delete
            // 
            resources.ApplyResources(this.btn_Delete, "btn_Delete");
            this.btn_Delete.Name = "btn_Delete";
            this.btn_Delete.UseVisualStyleBackColor = true;
            this.btn_Delete.Click += new System.EventHandler(this.btn_Delete_Click);
            // 
            // btn_Insert
            // 
            resources.ApplyResources(this.btn_Insert, "btn_Insert");
            this.btn_Insert.Name = "btn_Insert";
            this.btn_Insert.UseVisualStyleBackColor = true;
            this.btn_Insert.Click += new System.EventHandler(this.btn_Insert_Click);
            // 
            // btn_Save
            // 
            resources.ApplyResources(this.btn_Save, "btn_Save");
            this.btn_Save.Name = "btn_Save";
            this.btn_Save.UseVisualStyleBackColor = true;
            this.btn_Save.Click += new System.EventHandler(this.btn_Save_Click);
            // 
            // lab_AllowMaxWeight
            // 
            resources.ApplyResources(this.lab_AllowMaxWeight, "lab_AllowMaxWeight");
            this.lab_AllowMaxWeight.Name = "lab_AllowMaxWeight";
            // 
            // lab_BrewingData
            // 
            resources.ApplyResources(this.lab_BrewingData, "lab_BrewingData");
            this.lab_BrewingData.Name = "lab_BrewingData";
            // 
            // lab_BrewingCode
            // 
            resources.ApplyResources(this.lab_BrewingCode, "lab_BrewingCode");
            this.lab_BrewingCode.Name = "lab_BrewingCode";
            // 
            // lab_CurrentWeight
            // 
            resources.ApplyResources(this.lab_CurrentWeight, "lab_CurrentWeight");
            this.lab_CurrentWeight.Name = "lab_CurrentWeight";
            // 
            // lab_SettingConcentration
            // 
            resources.ApplyResources(this.lab_SettingConcentration, "lab_SettingConcentration");
            this.lab_SettingConcentration.Name = "lab_SettingConcentration";
            // 
            // lab_DropMinWeight
            // 
            resources.ApplyResources(this.lab_DropMinWeight, "lab_DropMinWeight");
            this.lab_DropMinWeight.Name = "lab_DropMinWeight";
            // 
            // lab_AssistantCode
            // 
            resources.ApplyResources(this.lab_AssistantCode, "lab_AssistantCode");
            this.lab_AssistantCode.Name = "lab_AssistantCode";
            // 
            // lab_BottleNum
            // 
            resources.ApplyResources(this.lab_BottleNum, "lab_BottleNum");
            this.lab_BottleNum.Name = "lab_BottleNum";
            // 
            // grp_AssistantDetails
            // 
            this.grp_AssistantDetails.Controls.Add(this.rdo_3);
            this.grp_AssistantDetails.Controls.Add(this.rdo_4);
            this.grp_AssistantDetails.Controls.Add(this.rdo_2);
            this.grp_AssistantDetails.Controls.Add(this.rdo_1);
            this.grp_AssistantDetails.Controls.Add(this.txt_AllowMaxColoringConcentration);
            this.grp_AssistantDetails.Controls.Add(this.lab_Cost);
            this.grp_AssistantDetails.Controls.Add(this.txt_AllowMinColoringConcentration);
            this.grp_AssistantDetails.Controls.Add(this.lab_Intensity);
            this.grp_AssistantDetails.Controls.Add(this.txt_Cost);
            this.grp_AssistantDetails.Controls.Add(this.lab_TermOfValidity);
            this.grp_AssistantDetails.Controls.Add(this.txt_Intensity);
            this.grp_AssistantDetails.Controls.Add(this.lab_AllowMaxColoringConcentration);
            this.grp_AssistantDetails.Controls.Add(this.txt_TermOfValidity);
            this.grp_AssistantDetails.Controls.Add(this.lab_AllowMinColoringConcentration);
            this.grp_AssistantDetails.Controls.Add(this.lab_UnitOfAccount);
            this.grp_AssistantDetails.Controls.Add(this.txt_AssistantType);
            this.grp_AssistantDetails.Controls.Add(this.lab_AssistantType);
            this.grp_AssistantDetails.Controls.Add(this.txt_AssistantName);
            this.grp_AssistantDetails.Controls.Add(this.lab_AssistantName);
            resources.ApplyResources(this.grp_AssistantDetails, "grp_AssistantDetails");
            this.grp_AssistantDetails.Name = "grp_AssistantDetails";
            this.grp_AssistantDetails.TabStop = false;
            // 
            // rdo_3
            // 
            resources.ApplyResources(this.rdo_3, "rdo_3");
            this.rdo_3.Name = "rdo_3";
            this.rdo_3.TabStop = true;
            this.rdo_3.UseVisualStyleBackColor = true;
            // 
            // rdo_4
            // 
            resources.ApplyResources(this.rdo_4, "rdo_4");
            this.rdo_4.Name = "rdo_4";
            this.rdo_4.TabStop = true;
            this.rdo_4.UseVisualStyleBackColor = true;
            // 
            // rdo_2
            // 
            resources.ApplyResources(this.rdo_2, "rdo_2");
            this.rdo_2.Name = "rdo_2";
            this.rdo_2.UseVisualStyleBackColor = true;
            // 
            // rdo_1
            // 
            resources.ApplyResources(this.rdo_1, "rdo_1");
            this.rdo_1.Name = "rdo_1";
            this.rdo_1.UseVisualStyleBackColor = true;
            // 
            // txt_AllowMaxColoringConcentration
            // 
            resources.ApplyResources(this.txt_AllowMaxColoringConcentration, "txt_AllowMaxColoringConcentration");
            this.txt_AllowMaxColoringConcentration.Name = "txt_AllowMaxColoringConcentration";
            this.txt_AllowMaxColoringConcentration.ReadOnly = true;
            // 
            // lab_Cost
            // 
            resources.ApplyResources(this.lab_Cost, "lab_Cost");
            this.lab_Cost.Name = "lab_Cost";
            // 
            // txt_AllowMinColoringConcentration
            // 
            resources.ApplyResources(this.txt_AllowMinColoringConcentration, "txt_AllowMinColoringConcentration");
            this.txt_AllowMinColoringConcentration.Name = "txt_AllowMinColoringConcentration";
            this.txt_AllowMinColoringConcentration.ReadOnly = true;
            // 
            // lab_Intensity
            // 
            resources.ApplyResources(this.lab_Intensity, "lab_Intensity");
            this.lab_Intensity.Name = "lab_Intensity";
            // 
            // txt_Cost
            // 
            resources.ApplyResources(this.txt_Cost, "txt_Cost");
            this.txt_Cost.Name = "txt_Cost";
            this.txt_Cost.ReadOnly = true;
            // 
            // lab_TermOfValidity
            // 
            resources.ApplyResources(this.lab_TermOfValidity, "lab_TermOfValidity");
            this.lab_TermOfValidity.Name = "lab_TermOfValidity";
            // 
            // txt_Intensity
            // 
            resources.ApplyResources(this.txt_Intensity, "txt_Intensity");
            this.txt_Intensity.Name = "txt_Intensity";
            this.txt_Intensity.ReadOnly = true;
            // 
            // lab_AllowMaxColoringConcentration
            // 
            resources.ApplyResources(this.lab_AllowMaxColoringConcentration, "lab_AllowMaxColoringConcentration");
            this.lab_AllowMaxColoringConcentration.Name = "lab_AllowMaxColoringConcentration";
            // 
            // txt_TermOfValidity
            // 
            resources.ApplyResources(this.txt_TermOfValidity, "txt_TermOfValidity");
            this.txt_TermOfValidity.Name = "txt_TermOfValidity";
            this.txt_TermOfValidity.ReadOnly = true;
            // 
            // lab_AllowMinColoringConcentration
            // 
            resources.ApplyResources(this.lab_AllowMinColoringConcentration, "lab_AllowMinColoringConcentration");
            this.lab_AllowMinColoringConcentration.Name = "lab_AllowMinColoringConcentration";
            // 
            // lab_UnitOfAccount
            // 
            resources.ApplyResources(this.lab_UnitOfAccount, "lab_UnitOfAccount");
            this.lab_UnitOfAccount.Name = "lab_UnitOfAccount";
            // 
            // txt_AssistantType
            // 
            resources.ApplyResources(this.txt_AssistantType, "txt_AssistantType");
            this.txt_AssistantType.Name = "txt_AssistantType";
            this.txt_AssistantType.ReadOnly = true;
            // 
            // lab_AssistantType
            // 
            resources.ApplyResources(this.lab_AssistantType, "lab_AssistantType");
            this.lab_AssistantType.Name = "lab_AssistantType";
            // 
            // txt_AssistantName
            // 
            resources.ApplyResources(this.txt_AssistantName, "txt_AssistantName");
            this.txt_AssistantName.Name = "txt_AssistantName";
            this.txt_AssistantName.ReadOnly = true;
            // 
            // lab_AssistantName
            // 
            resources.ApplyResources(this.lab_AssistantName, "lab_AssistantName");
            this.lab_AssistantName.Name = "lab_AssistantName";
            // 
            // cbo_Flowability
            // 
            this.cbo_Flowability.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.cbo_Flowability, "cbo_Flowability");
            this.cbo_Flowability.FormattingEnabled = true;
            this.cbo_Flowability.Items.AddRange(new object[] {
            resources.GetString("cbo_Flowability.Items"),
            resources.GetString("cbo_Flowability.Items1"),
            resources.GetString("cbo_Flowability.Items2")});
            this.cbo_Flowability.Name = "cbo_Flowability";
            this.cbo_Flowability.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cbo_Flowability_KeyDown);
            // 
            // lab_Flowability
            // 
            resources.ApplyResources(this.lab_Flowability, "lab_Flowability");
            this.lab_Flowability.Name = "lab_Flowability";
            // 
            // BottleDefin
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grp_AssistantDetails);
            this.Controls.Add(this.grp_BottleDetails);
            this.Controls.Add(this.grp_Browse);
            this.Name = "BottleDefin";
            this.grp_Browse.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Bottle)).EndInit();
            this.grp_BottleDetails.ResumeLayout(false);
            this.grp_BottleDetails.PerformLayout();
            this.grp_AssistantDetails.ResumeLayout(false);
            this.grp_AssistantDetails.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grp_Browse;
        private System.Windows.Forms.GroupBox grp_BottleDetails;
        private System.Windows.Forms.GroupBox grp_AssistantDetails;
        private System.Windows.Forms.Label lab_AllowMaxWeight;
        private System.Windows.Forms.Label lab_BrewingData;
        private System.Windows.Forms.Label lab_BrewingCode;
        private System.Windows.Forms.Label lab_CurrentWeight;
        private System.Windows.Forms.Label lab_SettingConcentration;
        private System.Windows.Forms.Label lab_DropMinWeight;
        private System.Windows.Forms.Label lab_AssistantCode;
        private System.Windows.Forms.Label lab_BottleNum;
        private System.Windows.Forms.Label lab_OriginalBottleNum;
        private System.Windows.Forms.Button btn_Delete;
        private System.Windows.Forms.Button btn_Insert;
        private System.Windows.Forms.Button btn_Save;
        private System.Windows.Forms.TextBox txt_BottleNum;
        private System.Windows.Forms.TextBox txt_CurrentWeight;
        private System.Windows.Forms.TextBox txt_DropMinWeight;
        private System.Windows.Forms.TextBox txt_AllowMaxWeight;
        private System.Windows.Forms.ComboBox cbo_AssistantCode;
        private System.Windows.Forms.ComboBox cbo_SyringeType;
        private System.Windows.Forms.TextBox txt_SettingConcentration;
        private System.Windows.Forms.ComboBox cbo_BrewingCode;
        private System.Windows.Forms.Label lab_SyringeType;
        private System.Windows.Forms.DateTimePicker dtp_BrewingData;
        private System.Windows.Forms.TextBox txt_AllowMaxColoringConcentration;
        private System.Windows.Forms.Label lab_Cost;
        private System.Windows.Forms.TextBox txt_AllowMinColoringConcentration;
        private System.Windows.Forms.Label lab_Intensity;
        private System.Windows.Forms.TextBox txt_Cost;
        private System.Windows.Forms.Label lab_TermOfValidity;
        private System.Windows.Forms.TextBox txt_Intensity;
        private System.Windows.Forms.Label lab_AllowMaxColoringConcentration;
        private System.Windows.Forms.TextBox txt_TermOfValidity;
        private System.Windows.Forms.Label lab_AllowMinColoringConcentration;
        private System.Windows.Forms.Label lab_UnitOfAccount;
        private System.Windows.Forms.TextBox txt_AssistantType;
        private System.Windows.Forms.Label lab_AssistantType;
        private System.Windows.Forms.TextBox txt_AssistantName;
        private System.Windows.Forms.Label lab_AssistantName;
        private System.Windows.Forms.RadioButton rdo_1;
        private System.Windows.Forms.RadioButton rdo_2;
        private System.Windows.Forms.DataGridView dgv_Bottle;
        private System.Windows.Forms.ComboBox cbo_OriginalBottleNum;
        private System.Windows.Forms.RadioButton rdo_3;
        private System.Windows.Forms.RadioButton rdo_4;
        private ComboBox cbo_DripReserveFirst;
        private Label lab_DripReserveFirst;
        private TextBox txt_WashSyringeSpan;
        private Label label1;
        private ComboBox cbo_Flowability;
        private Label lab_Flowability;
    }
}
