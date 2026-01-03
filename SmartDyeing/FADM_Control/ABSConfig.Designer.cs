namespace SmartDyeing.FADM_Control
{
    partial class ABSConfig
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
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.grp_BrewingProcess = new System.Windows.Forms.GroupBox();
            this.btn_Copy = new System.Windows.Forms.Button();
            this.btn_DyeingProcessDelete = new System.Windows.Forms.Button();
            this.btn_DyeingProcessUpdate = new System.Windows.Forms.Button();
            this.btn_DyeingProcessAdd = new System.Windows.Forms.Button();
            this.dgv_Child_DyeData = new System.Windows.Forms.DataGridView();
            this.btn_DyeingCodeDelete = new System.Windows.Forms.Button();
            this.btn_DyeingCodeAdd = new System.Windows.Forms.Button();
            this.txt_Dye_Code = new System.Windows.Forms.TextBox();
            this.lab_BrewProcessCode = new System.Windows.Forms.Label();
            this.dgv_Dye_Code = new System.Windows.Forms.DataGridView();
            this.grp_Dyeing = new System.Windows.Forms.GroupBox();
            this.grp_BrewingProcess.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Child_DyeData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Dye_Code)).BeginInit();
            this.grp_Dyeing.SuspendLayout();
            this.SuspendLayout();
            // 
            // grp_BrewingProcess
            // 
            this.grp_BrewingProcess.Controls.Add(this.btn_Copy);
            this.grp_BrewingProcess.Controls.Add(this.btn_DyeingProcessDelete);
            this.grp_BrewingProcess.Controls.Add(this.btn_DyeingProcessUpdate);
            this.grp_BrewingProcess.Controls.Add(this.btn_DyeingProcessAdd);
            this.grp_BrewingProcess.Controls.Add(this.dgv_Child_DyeData);
            this.grp_BrewingProcess.Controls.Add(this.btn_DyeingCodeDelete);
            this.grp_BrewingProcess.Controls.Add(this.btn_DyeingCodeAdd);
            this.grp_BrewingProcess.Controls.Add(this.txt_Dye_Code);
            this.grp_BrewingProcess.Controls.Add(this.lab_BrewProcessCode);
            this.grp_BrewingProcess.Font = new System.Drawing.Font("宋体", 10.5F);
            this.grp_BrewingProcess.Location = new System.Drawing.Point(276, 2);
            this.grp_BrewingProcess.Name = "grp_BrewingProcess";
            this.grp_BrewingProcess.Size = new System.Drawing.Size(1627, 977);
            this.grp_BrewingProcess.TabIndex = 8;
            this.grp_BrewingProcess.TabStop = false;
            this.grp_BrewingProcess.Text = "ABS工艺设定";
            // 
            // btn_Copy
            // 
            this.btn_Copy.Font = new System.Drawing.Font("宋体", 14.25F);
            this.btn_Copy.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btn_Copy.Location = new System.Drawing.Point(1524, 399);
            this.btn_Copy.Name = "btn_Copy";
            this.btn_Copy.Size = new System.Drawing.Size(83, 42);
            this.btn_Copy.TabIndex = 11;
            this.btn_Copy.Text = "复制";
            this.btn_Copy.UseVisualStyleBackColor = true;
            this.btn_Copy.Click += new System.EventHandler(this.btn_Copy_Click);
            // 
            // btn_DyeingProcessDelete
            // 
            this.btn_DyeingProcessDelete.Enabled = false;
            this.btn_DyeingProcessDelete.Font = new System.Drawing.Font("宋体", 14.25F);
            this.btn_DyeingProcessDelete.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btn_DyeingProcessDelete.Location = new System.Drawing.Point(1524, 329);
            this.btn_DyeingProcessDelete.Name = "btn_DyeingProcessDelete";
            this.btn_DyeingProcessDelete.Size = new System.Drawing.Size(83, 42);
            this.btn_DyeingProcessDelete.TabIndex = 7;
            this.btn_DyeingProcessDelete.Text = "删除";
            this.btn_DyeingProcessDelete.UseVisualStyleBackColor = true;
            this.btn_DyeingProcessDelete.Click += new System.EventHandler(this.btn_DyeingProcessDelete_Click);
            // 
            // btn_DyeingProcessUpdate
            // 
            this.btn_DyeingProcessUpdate.Enabled = false;
            this.btn_DyeingProcessUpdate.Font = new System.Drawing.Font("宋体", 14.25F);
            this.btn_DyeingProcessUpdate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btn_DyeingProcessUpdate.Location = new System.Drawing.Point(1524, 259);
            this.btn_DyeingProcessUpdate.Name = "btn_DyeingProcessUpdate";
            this.btn_DyeingProcessUpdate.Size = new System.Drawing.Size(83, 42);
            this.btn_DyeingProcessUpdate.TabIndex = 6;
            this.btn_DyeingProcessUpdate.Text = "修改";
            this.btn_DyeingProcessUpdate.UseVisualStyleBackColor = true;
            this.btn_DyeingProcessUpdate.Click += new System.EventHandler(this.btn_DyeingProcessUpdate_Click);
            // 
            // btn_DyeingProcessAdd
            // 
            this.btn_DyeingProcessAdd.Enabled = false;
            this.btn_DyeingProcessAdd.Font = new System.Drawing.Font("宋体", 14.25F);
            this.btn_DyeingProcessAdd.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btn_DyeingProcessAdd.Location = new System.Drawing.Point(1524, 190);
            this.btn_DyeingProcessAdd.Name = "btn_DyeingProcessAdd";
            this.btn_DyeingProcessAdd.Size = new System.Drawing.Size(83, 42);
            this.btn_DyeingProcessAdd.TabIndex = 5;
            this.btn_DyeingProcessAdd.Text = "添加";
            this.btn_DyeingProcessAdd.UseVisualStyleBackColor = true;
            this.btn_DyeingProcessAdd.Click += new System.EventHandler(this.btn_DyeingProcessAdd_Click);
            // 
            // dgv_Child_DyeData
            // 
            this.dgv_Child_DyeData.AllowUserToAddRows = false;
            this.dgv_Child_DyeData.AllowUserToDeleteRows = false;
            this.dgv_Child_DyeData.AllowUserToResizeColumns = false;
            this.dgv_Child_DyeData.AllowUserToResizeRows = false;
            this.dgv_Child_DyeData.BackgroundColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("宋体", 10.5F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgv_Child_DyeData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgv_Child_DyeData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_Child_DyeData.Location = new System.Drawing.Point(6, 192);
            this.dgv_Child_DyeData.MultiSelect = false;
            this.dgv_Child_DyeData.Name = "dgv_Child_DyeData";
            this.dgv_Child_DyeData.ReadOnly = true;
            this.dgv_Child_DyeData.RowHeadersVisible = false;
            this.dgv_Child_DyeData.RowHeadersWidth = 62;
            this.dgv_Child_DyeData.RowTemplate.Height = 23;
            this.dgv_Child_DyeData.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dgv_Child_DyeData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_Child_DyeData.Size = new System.Drawing.Size(1512, 779);
            this.dgv_Child_DyeData.TabIndex = 4;
            // 
            // btn_DyeingCodeDelete
            // 
            this.btn_DyeingCodeDelete.Font = new System.Drawing.Font("宋体", 14.25F);
            this.btn_DyeingCodeDelete.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btn_DyeingCodeDelete.Location = new System.Drawing.Point(1517, 60);
            this.btn_DyeingCodeDelete.Name = "btn_DyeingCodeDelete";
            this.btn_DyeingCodeDelete.Size = new System.Drawing.Size(83, 42);
            this.btn_DyeingCodeDelete.TabIndex = 3;
            this.btn_DyeingCodeDelete.Text = "删除";
            this.btn_DyeingCodeDelete.UseVisualStyleBackColor = true;
            this.btn_DyeingCodeDelete.Click += new System.EventHandler(this.btn_DyeingCodeDelete_Click);
            // 
            // btn_DyeingCodeAdd
            // 
            this.btn_DyeingCodeAdd.Font = new System.Drawing.Font("宋体", 14.25F);
            this.btn_DyeingCodeAdd.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btn_DyeingCodeAdd.Location = new System.Drawing.Point(1406, 60);
            this.btn_DyeingCodeAdd.Name = "btn_DyeingCodeAdd";
            this.btn_DyeingCodeAdd.Size = new System.Drawing.Size(83, 42);
            this.btn_DyeingCodeAdd.TabIndex = 2;
            this.btn_DyeingCodeAdd.Text = "新增";
            this.btn_DyeingCodeAdd.UseVisualStyleBackColor = true;
            this.btn_DyeingCodeAdd.Click += new System.EventHandler(this.btn_DyeingCodeAdd_Click);
            // 
            // txt_Dye_Code
            // 
            this.txt_Dye_Code.Enabled = false;
            this.txt_Dye_Code.Font = new System.Drawing.Font("宋体", 14.25F);
            this.txt_Dye_Code.Location = new System.Drawing.Point(190, 67);
            this.txt_Dye_Code.Name = "txt_Dye_Code";
            this.txt_Dye_Code.Size = new System.Drawing.Size(1210, 29);
            this.txt_Dye_Code.TabIndex = 1;
            this.txt_Dye_Code.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_Dye_Code_KeyDown);
            // 
            // lab_BrewProcessCode
            // 
            this.lab_BrewProcessCode.AutoSize = true;
            this.lab_BrewProcessCode.Font = new System.Drawing.Font("宋体", 14.25F);
            this.lab_BrewProcessCode.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lab_BrewProcessCode.Location = new System.Drawing.Point(29, 72);
            this.lab_BrewProcessCode.Name = "lab_BrewProcessCode";
            this.lab_BrewProcessCode.Size = new System.Drawing.Size(125, 19);
            this.lab_BrewProcessCode.TabIndex = 0;
            this.lab_BrewProcessCode.Text = "ABS工艺代码:";
            this.lab_BrewProcessCode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // dgv_Dye_Code
            // 
            this.dgv_Dye_Code.AllowUserToAddRows = false;
            this.dgv_Dye_Code.AllowUserToDeleteRows = false;
            this.dgv_Dye_Code.AllowUserToResizeColumns = false;
            this.dgv_Dye_Code.AllowUserToResizeRows = false;
            this.dgv_Dye_Code.BackgroundColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("宋体", 10.5F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgv_Dye_Code.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgv_Dye_Code.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_Dye_Code.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_Dye_Code.Location = new System.Drawing.Point(3, 19);
            this.dgv_Dye_Code.MultiSelect = false;
            this.dgv_Dye_Code.Name = "dgv_Dye_Code";
            this.dgv_Dye_Code.ReadOnly = true;
            this.dgv_Dye_Code.RowHeadersVisible = false;
            this.dgv_Dye_Code.RowHeadersWidth = 62;
            this.dgv_Dye_Code.RowTemplate.Height = 23;
            this.dgv_Dye_Code.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dgv_Dye_Code.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgv_Dye_Code.Size = new System.Drawing.Size(262, 955);
            this.dgv_Dye_Code.TabIndex = 0;
            this.dgv_Dye_Code.CurrentCellChanged += new System.EventHandler(this.dgv_Dye_Code_CurrentCellChanged);
            // 
            // grp_Dyeing
            // 
            this.grp_Dyeing.Controls.Add(this.dgv_Dye_Code);
            this.grp_Dyeing.Font = new System.Drawing.Font("宋体", 10.5F);
            this.grp_Dyeing.Location = new System.Drawing.Point(2, 2);
            this.grp_Dyeing.Name = "grp_Dyeing";
            this.grp_Dyeing.Size = new System.Drawing.Size(268, 977);
            this.grp_Dyeing.TabIndex = 7;
            this.grp_Dyeing.TabStop = false;
            this.grp_Dyeing.Text = "浏览";
            // 
            // ABSConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grp_BrewingProcess);
            this.Controls.Add(this.grp_Dyeing);
            this.Name = "ABSConfig";
            this.Size = new System.Drawing.Size(1904, 980);
            this.grp_BrewingProcess.ResumeLayout(false);
            this.grp_BrewingProcess.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Child_DyeData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Dye_Code)).EndInit();
            this.grp_Dyeing.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grp_BrewingProcess;
        private System.Windows.Forms.Button btn_Copy;
        private System.Windows.Forms.Button btn_DyeingProcessDelete;
        private System.Windows.Forms.Button btn_DyeingProcessUpdate;
        private System.Windows.Forms.Button btn_DyeingProcessAdd;
        private System.Windows.Forms.DataGridView dgv_Child_DyeData;
        private System.Windows.Forms.Button btn_DyeingCodeDelete;
        private System.Windows.Forms.Button btn_DyeingCodeAdd;
        private System.Windows.Forms.TextBox txt_Dye_Code;
        private System.Windows.Forms.Label lab_BrewProcessCode;
        private System.Windows.Forms.DataGridView dgv_Dye_Code;
        private System.Windows.Forms.GroupBox grp_Dyeing;
    }
}
