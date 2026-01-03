namespace SmartDyeing.FADM_Form
{
    partial class InsertAbs
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
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.BtnBottleCheckPause = new System.Windows.Forms.Button();
            this.BtnBottleCheckStart = new System.Windows.Forms.Button();
            this.TxtCheckBottleNo = new System.Windows.Forms.TextBox();
            this.dgv_DropRecord = new System.Windows.Forms.DataGridView();
            this.button1 = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_DropRecord)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 14.25F);
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(141, 260);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 19);
            this.label1.TabIndex = 13;
            this.label1.Text = "瓶号：";
            // 
            // BtnBottleCheckPause
            // 
            this.BtnBottleCheckPause.Font = new System.Drawing.Font("宋体", 14.25F);
            this.BtnBottleCheckPause.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.BtnBottleCheckPause.Location = new System.Drawing.Point(89, 389);
            this.BtnBottleCheckPause.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.BtnBottleCheckPause.Name = "BtnBottleCheckPause";
            this.BtnBottleCheckPause.Size = new System.Drawing.Size(153, 35);
            this.BtnBottleCheckPause.TabIndex = 11;
            this.BtnBottleCheckPause.Text = "删除";
            this.BtnBottleCheckPause.UseVisualStyleBackColor = true;
            this.BtnBottleCheckPause.Click += new System.EventHandler(this.BtnBottleCheckPause_Click);
            // 
            // BtnBottleCheckStart
            // 
            this.BtnBottleCheckStart.Font = new System.Drawing.Font("宋体", 14.25F);
            this.BtnBottleCheckStart.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.BtnBottleCheckStart.Location = new System.Drawing.Point(89, 338);
            this.BtnBottleCheckStart.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.BtnBottleCheckStart.Name = "BtnBottleCheckStart";
            this.BtnBottleCheckStart.Size = new System.Drawing.Size(153, 35);
            this.BtnBottleCheckStart.TabIndex = 10;
            this.BtnBottleCheckStart.Text = "加入批次";
            this.BtnBottleCheckStart.UseVisualStyleBackColor = true;
            this.BtnBottleCheckStart.Click += new System.EventHandler(this.BtnBottleCheckStart_Click);
            // 
            // TxtCheckBottleNo
            // 
            this.TxtCheckBottleNo.Font = new System.Drawing.Font("宋体", 14.25F);
            this.TxtCheckBottleNo.Location = new System.Drawing.Point(64, 294);
            this.TxtCheckBottleNo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.TxtCheckBottleNo.Name = "TxtCheckBottleNo";
            this.TxtCheckBottleNo.Size = new System.Drawing.Size(220, 29);
            this.TxtCheckBottleNo.TabIndex = 9;
            // 
            // dgv_DropRecord
            // 
            this.dgv_DropRecord.AllowUserToAddRows = false;
            this.dgv_DropRecord.AllowUserToDeleteRows = false;
            this.dgv_DropRecord.AllowUserToResizeColumns = false;
            this.dgv_DropRecord.AllowUserToResizeRows = false;
            this.dgv_DropRecord.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgv_DropRecord.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_DropRecord.Location = new System.Drawing.Point(368, 27);
            this.dgv_DropRecord.MultiSelect = false;
            this.dgv_DropRecord.Name = "dgv_DropRecord";
            this.dgv_DropRecord.ReadOnly = true;
            this.dgv_DropRecord.RowHeadersVisible = false;
            this.dgv_DropRecord.RowHeadersWidth = 72;
            this.dgv_DropRecord.RowTemplate.Height = 23;
            this.dgv_DropRecord.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dgv_DropRecord.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_DropRecord.Size = new System.Drawing.Size(424, 730);
            this.dgv_DropRecord.TabIndex = 14;
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("宋体", 14.25F);
            this.button1.Location = new System.Drawing.Point(89, 440);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(153, 35);
            this.button1.TabIndex = 15;
            this.button1.Text = "刷新";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 60000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // InsertAbs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(809, 773);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.dgv_DropRecord);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.BtnBottleCheckPause);
            this.Controls.Add(this.BtnBottleCheckStart);
            this.Controls.Add(this.TxtCheckBottleNo);
            this.Name = "InsertAbs";
            this.Text = "吸光度测量";
            ((System.ComponentModel.ISupportInitialize)(this.dgv_DropRecord)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button BtnBottleCheckPause;
        private System.Windows.Forms.Button BtnBottleCheckStart;
        private System.Windows.Forms.TextBox TxtCheckBottleNo;
        private System.Windows.Forms.DataGridView dgv_DropRecord;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Timer timer1;
    }
}