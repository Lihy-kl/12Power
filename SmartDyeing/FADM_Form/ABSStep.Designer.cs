namespace SmartDyeing.FADM_Form
{
    partial class ABSStep
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txt_ParallelizingDishTime = new System.Windows.Forms.TextBox();
            this.lab_Rev = new System.Windows.Forms.Label();
            this.txt_StirringTime = new System.Windows.Forms.TextBox();
            this.lab_Rate = new System.Windows.Forms.Label();
            this.txt_StirringRate = new System.Windows.Forms.TextBox();
            this.lab_Temp = new System.Windows.Forms.Label();
            this.cbo_TechnologyName = new System.Windows.Forms.ComboBox();
            this.btn_Save = new System.Windows.Forms.Button();
            this.txt_DrainTime = new System.Windows.Forms.TextBox();
            this.txt_StepNum = new System.Windows.Forms.TextBox();
            this.lab_ProportionOrTime = new System.Windows.Forms.Label();
            this.lab_TechnologyName = new System.Windows.Forms.Label();
            this.lab_StepNum = new System.Windows.Forms.Label();
            this.txt_PumpingTime = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txt_StartingWavelength = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txt_EndWavelength = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txt_WavelengthInterval = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txt_Dosage = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txt_ParallelizingDishTime
            // 
            this.txt_ParallelizingDishTime.Font = new System.Drawing.Font("宋体", 14.25F);
            this.txt_ParallelizingDishTime.Location = new System.Drawing.Point(216, 216);
            this.txt_ParallelizingDishTime.Name = "txt_ParallelizingDishTime";
            this.txt_ParallelizingDishTime.Size = new System.Drawing.Size(202, 29);
            this.txt_ParallelizingDishTime.TabIndex = 46;
            // 
            // lab_Rev
            // 
            this.lab_Rev.AutoSize = true;
            this.lab_Rev.Font = new System.Drawing.Font("宋体", 14.25F);
            this.lab_Rev.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lab_Rev.Location = new System.Drawing.Point(24, 223);
            this.lab_Rev.Name = "lab_Rev";
            this.lab_Rev.Size = new System.Drawing.Size(163, 19);
            this.lab_Rev.TabIndex = 45;
            this.lab_Rev.Text = "排比色皿时间(s):";
            this.lab_Rev.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txt_StirringTime
            // 
            this.txt_StirringTime.Font = new System.Drawing.Font("宋体", 14.25F);
            this.txt_StirringTime.Location = new System.Drawing.Point(216, 136);
            this.txt_StirringTime.Name = "txt_StirringTime";
            this.txt_StirringTime.Size = new System.Drawing.Size(202, 29);
            this.txt_StirringTime.TabIndex = 43;
            // 
            // lab_Rate
            // 
            this.lab_Rate.AutoSize = true;
            this.lab_Rate.Font = new System.Drawing.Font("宋体", 14.25F);
            this.lab_Rate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lab_Rate.Location = new System.Drawing.Point(24, 143);
            this.lab_Rate.Name = "lab_Rate";
            this.lab_Rate.Size = new System.Drawing.Size(125, 19);
            this.lab_Rate.TabIndex = 42;
            this.lab_Rate.Text = "搅拌时间(s):";
            this.lab_Rate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txt_StirringRate
            // 
            this.txt_StirringRate.Font = new System.Drawing.Font("宋体", 14.25F);
            this.txt_StirringRate.Location = new System.Drawing.Point(216, 96);
            this.txt_StirringRate.Name = "txt_StirringRate";
            this.txt_StirringRate.Size = new System.Drawing.Size(202, 29);
            this.txt_StirringRate.TabIndex = 41;
            // 
            // lab_Temp
            // 
            this.lab_Temp.AutoSize = true;
            this.lab_Temp.Font = new System.Drawing.Font("宋体", 14.25F);
            this.lab_Temp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lab_Temp.Location = new System.Drawing.Point(24, 103);
            this.lab_Temp.Name = "lab_Temp";
            this.lab_Temp.Size = new System.Drawing.Size(95, 19);
            this.lab_Temp.TabIndex = 40;
            this.lab_Temp.Text = "搅拌速度:";
            this.lab_Temp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cbo_TechnologyName
            // 
            this.cbo_TechnologyName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbo_TechnologyName.Font = new System.Drawing.Font("宋体", 14.25F);
            this.cbo_TechnologyName.FormattingEnabled = true;
            this.cbo_TechnologyName.Items.AddRange(new object[] {
            "加药",
            "加水",
            "抽染液",
            "搅拌",
            "排液",
            "测吸光度",
            "加溶解剂"});
            this.cbo_TechnologyName.Location = new System.Drawing.Point(216, 58);
            this.cbo_TechnologyName.Name = "cbo_TechnologyName";
            this.cbo_TechnologyName.Size = new System.Drawing.Size(201, 27);
            this.cbo_TechnologyName.TabIndex = 39;
            this.cbo_TechnologyName.SelectedIndexChanged += new System.EventHandler(this.cbo_TechnologyName_SelectedIndexChanged);
            // 
            // btn_Save
            // 
            this.btn_Save.Font = new System.Drawing.Font("宋体", 14.25F);
            this.btn_Save.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btn_Save.Location = new System.Drawing.Point(185, 461);
            this.btn_Save.Name = "btn_Save";
            this.btn_Save.Size = new System.Drawing.Size(83, 42);
            this.btn_Save.TabIndex = 47;
            this.btn_Save.Text = "保存";
            this.btn_Save.UseVisualStyleBackColor = true;
            this.btn_Save.Click += new System.EventHandler(this.btn_Save_Click);
            // 
            // txt_DrainTime
            // 
            this.txt_DrainTime.Font = new System.Drawing.Font("宋体", 14.25F);
            this.txt_DrainTime.Location = new System.Drawing.Point(216, 176);
            this.txt_DrainTime.Name = "txt_DrainTime";
            this.txt_DrainTime.Size = new System.Drawing.Size(202, 29);
            this.txt_DrainTime.TabIndex = 44;
            // 
            // txt_StepNum
            // 
            this.txt_StepNum.Enabled = false;
            this.txt_StepNum.Font = new System.Drawing.Font("宋体", 14.25F);
            this.txt_StepNum.Location = new System.Drawing.Point(216, 18);
            this.txt_StepNum.Name = "txt_StepNum";
            this.txt_StepNum.Size = new System.Drawing.Size(202, 29);
            this.txt_StepNum.TabIndex = 38;
            // 
            // lab_ProportionOrTime
            // 
            this.lab_ProportionOrTime.AutoSize = true;
            this.lab_ProportionOrTime.Font = new System.Drawing.Font("宋体", 14.25F);
            this.lab_ProportionOrTime.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lab_ProportionOrTime.Location = new System.Drawing.Point(24, 183);
            this.lab_ProportionOrTime.Name = "lab_ProportionOrTime";
            this.lab_ProportionOrTime.Size = new System.Drawing.Size(125, 19);
            this.lab_ProportionOrTime.TabIndex = 37;
            this.lab_ProportionOrTime.Text = "排液时间(s):";
            this.lab_ProportionOrTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lab_TechnologyName
            // 
            this.lab_TechnologyName.AutoSize = true;
            this.lab_TechnologyName.Font = new System.Drawing.Font("宋体", 14.25F);
            this.lab_TechnologyName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lab_TechnologyName.Location = new System.Drawing.Point(24, 63);
            this.lab_TechnologyName.Name = "lab_TechnologyName";
            this.lab_TechnologyName.Size = new System.Drawing.Size(95, 19);
            this.lab_TechnologyName.TabIndex = 36;
            this.lab_TechnologyName.Text = "操作流程:";
            this.lab_TechnologyName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lab_StepNum
            // 
            this.lab_StepNum.AutoSize = true;
            this.lab_StepNum.Font = new System.Drawing.Font("宋体", 14.25F);
            this.lab_StepNum.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lab_StepNum.Location = new System.Drawing.Point(24, 23);
            this.lab_StepNum.Name = "lab_StepNum";
            this.lab_StepNum.Size = new System.Drawing.Size(57, 19);
            this.lab_StepNum.TabIndex = 35;
            this.lab_StepNum.Text = "步号:";
            this.lab_StepNum.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txt_PumpingTime
            // 
            this.txt_PumpingTime.Font = new System.Drawing.Font("宋体", 14.25F);
            this.txt_PumpingTime.Location = new System.Drawing.Point(216, 256);
            this.txt_PumpingTime.Name = "txt_PumpingTime";
            this.txt_PumpingTime.Size = new System.Drawing.Size(202, 29);
            this.txt_PumpingTime.TabIndex = 49;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 14.25F);
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(24, 263);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(125, 19);
            this.label1.TabIndex = 48;
            this.label1.Text = "抽液时间(s):";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txt_StartingWavelength
            // 
            this.txt_StartingWavelength.Font = new System.Drawing.Font("宋体", 14.25F);
            this.txt_StartingWavelength.Location = new System.Drawing.Point(216, 296);
            this.txt_StartingWavelength.Name = "txt_StartingWavelength";
            this.txt_StartingWavelength.Size = new System.Drawing.Size(202, 29);
            this.txt_StartingWavelength.TabIndex = 51;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 14.25F);
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(24, 303);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 19);
            this.label2.TabIndex = 50;
            this.label2.Text = "开始波长:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txt_EndWavelength
            // 
            this.txt_EndWavelength.Font = new System.Drawing.Font("宋体", 14.25F);
            this.txt_EndWavelength.Location = new System.Drawing.Point(216, 336);
            this.txt_EndWavelength.Name = "txt_EndWavelength";
            this.txt_EndWavelength.Size = new System.Drawing.Size(202, 29);
            this.txt_EndWavelength.TabIndex = 53;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 14.25F);
            this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label3.Location = new System.Drawing.Point(24, 343);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(95, 19);
            this.label3.TabIndex = 52;
            this.label3.Text = "结束波长:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txt_WavelengthInterval
            // 
            this.txt_WavelengthInterval.Font = new System.Drawing.Font("宋体", 14.25F);
            this.txt_WavelengthInterval.Location = new System.Drawing.Point(216, 376);
            this.txt_WavelengthInterval.Name = "txt_WavelengthInterval";
            this.txt_WavelengthInterval.Size = new System.Drawing.Size(202, 29);
            this.txt_WavelengthInterval.TabIndex = 55;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("宋体", 14.25F);
            this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label4.Location = new System.Drawing.Point(24, 383);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(95, 19);
            this.label4.TabIndex = 54;
            this.label4.Text = "波长间隔:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txt_Dosage
            // 
            this.txt_Dosage.Font = new System.Drawing.Font("宋体", 14.25F);
            this.txt_Dosage.Location = new System.Drawing.Point(215, 418);
            this.txt_Dosage.Name = "txt_Dosage";
            this.txt_Dosage.Size = new System.Drawing.Size(202, 29);
            this.txt_Dosage.TabIndex = 57;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("宋体", 14.25F);
            this.label5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label5.Location = new System.Drawing.Point(23, 425);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(106, 19);
            this.label5.TabIndex = 56;
            this.label5.Text = "加药量(g):";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ABSStep
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(466, 521);
            this.Controls.Add(this.txt_Dosage);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txt_WavelengthInterval);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txt_EndWavelength);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txt_StartingWavelength);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txt_PumpingTime);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txt_ParallelizingDishTime);
            this.Controls.Add(this.lab_Rev);
            this.Controls.Add(this.txt_StirringTime);
            this.Controls.Add(this.lab_Rate);
            this.Controls.Add(this.txt_StirringRate);
            this.Controls.Add(this.lab_Temp);
            this.Controls.Add(this.cbo_TechnologyName);
            this.Controls.Add(this.btn_Save);
            this.Controls.Add(this.txt_DrainTime);
            this.Controls.Add(this.txt_StepNum);
            this.Controls.Add(this.lab_ProportionOrTime);
            this.Controls.Add(this.lab_TechnologyName);
            this.Controls.Add(this.lab_StepNum);
            this.Name = "ABSStep";
            this.Text = "吸光度工艺设定";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txt_ParallelizingDishTime;
        private System.Windows.Forms.Label lab_Rev;
        private System.Windows.Forms.TextBox txt_StirringTime;
        private System.Windows.Forms.Label lab_Rate;
        private System.Windows.Forms.TextBox txt_StirringRate;
        private System.Windows.Forms.Label lab_Temp;
        private System.Windows.Forms.ComboBox cbo_TechnologyName;
        private System.Windows.Forms.Button btn_Save;
        private System.Windows.Forms.TextBox txt_DrainTime;
        private System.Windows.Forms.TextBox txt_StepNum;
        private System.Windows.Forms.Label lab_ProportionOrTime;
        private System.Windows.Forms.Label lab_TechnologyName;
        private System.Windows.Forms.Label lab_StepNum;
        private System.Windows.Forms.TextBox txt_PumpingTime;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txt_StartingWavelength;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txt_EndWavelength;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txt_WavelengthInterval;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txt_Dosage;
        private System.Windows.Forms.Label label5;
    }
}