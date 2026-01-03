namespace SmartDyeing.FADM_Form
{
    partial class AbsCheck
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
            this.label2 = new System.Windows.Forms.Label();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsm_Online = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_Offline = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_Stop = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_Reset = new System.Windows.Forms.ToolStripMenuItem();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.cup1 = new SmartDyeing.FADM_Control.Cup();
            this.cup2 = new SmartDyeing.FADM_Control.Cup();
            this.label3 = new System.Windows.Forms.Label();
            this.cup3 = new SmartDyeing.FADM_Control.Cup();
            this.label4 = new System.Windows.Forms.Label();
            this.cup4 = new SmartDyeing.FADM_Control.Cup();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoEllipsis = true;
            this.label1.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.5F);
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(27, 87);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(134, 25);
            this.label1.TabIndex = 114;
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.AutoEllipsis = true;
            this.label2.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.5F);
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(189, 87);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(134, 25);
            this.label2.TabIndex = 115;
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsm_Online,
            this.tsm_Offline,
            this.tsm_Stop,
            this.tsm_Reset});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(101, 92);
            // 
            // tsm_Online
            // 
            this.tsm_Online.Name = "tsm_Online";
            this.tsm_Online.Size = new System.Drawing.Size(100, 22);
            this.tsm_Online.Text = "上线";
            this.tsm_Online.Click += new System.EventHandler(this.tsm_Online_Click);
            // 
            // tsm_Offline
            // 
            this.tsm_Offline.Name = "tsm_Offline";
            this.tsm_Offline.Size = new System.Drawing.Size(100, 22);
            this.tsm_Offline.Text = "下线";
            this.tsm_Offline.Click += new System.EventHandler(this.tsm_Offline_Click);
            // 
            // tsm_Stop
            // 
            this.tsm_Stop.Name = "tsm_Stop";
            this.tsm_Stop.Size = new System.Drawing.Size(100, 22);
            this.tsm_Stop.Text = "停止";
            this.tsm_Stop.Click += new System.EventHandler(this.tsm_Stop_Click);
            // 
            // tsm_Reset
            // 
            this.tsm_Reset.Name = "tsm_Reset";
            this.tsm_Reset.Size = new System.Drawing.Size(100, 22);
            this.tsm_Reset.Text = "重置";
            this.tsm_Reset.Click += new System.EventHandler(this.tsm_Reset_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // cup1
            // 
            this.cup1.BackColor = System.Drawing.Color.Transparent;
            this.cup1.BottleColor = System.Drawing.Color.Black;
            this.cup1.LiquidColor = System.Drawing.Color.Transparent;
            this.cup1.Location = new System.Drawing.Point(76, 10);
            this.cup1.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.cup1.MaxValue = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.cup1.Name = "cup1";
            this.cup1.NO = "1";
            this.cup1.Size = new System.Drawing.Size(40, 80);
            this.cup1.TabIndex = 112;
            this.cup1.Title = "";
            this.cup1.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // cup2
            // 
            this.cup2.BackColor = System.Drawing.Color.Transparent;
            this.cup2.BottleColor = System.Drawing.Color.Black;
            this.cup2.LiquidColor = System.Drawing.Color.Transparent;
            this.cup2.Location = new System.Drawing.Point(236, 10);
            this.cup2.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.cup2.MaxValue = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.cup2.Name = "cup2";
            this.cup2.NO = "2";
            this.cup2.Size = new System.Drawing.Size(40, 80);
            this.cup2.TabIndex = 113;
            this.cup2.Title = "";
            this.cup2.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoEllipsis = true;
            this.label3.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.5F);
            this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label3.Location = new System.Drawing.Point(27, 207);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(134, 25);
            this.label3.TabIndex = 117;
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cup3
            // 
            this.cup3.BackColor = System.Drawing.Color.Transparent;
            this.cup3.BottleColor = System.Drawing.Color.Black;
            this.cup3.LiquidColor = System.Drawing.Color.Transparent;
            this.cup3.Location = new System.Drawing.Point(76, 130);
            this.cup3.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.cup3.MaxValue = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.cup3.Name = "cup3";
            this.cup3.NO = "3";
            this.cup3.Size = new System.Drawing.Size(40, 80);
            this.cup3.TabIndex = 116;
            this.cup3.Title = "";
            this.cup3.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoEllipsis = true;
            this.label4.Font = new System.Drawing.Font("Microsoft YaHei UI", 10.5F);
            this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label4.Location = new System.Drawing.Point(187, 207);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(134, 25);
            this.label4.TabIndex = 119;
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cup4
            // 
            this.cup4.BackColor = System.Drawing.Color.Transparent;
            this.cup4.BottleColor = System.Drawing.Color.Black;
            this.cup4.LiquidColor = System.Drawing.Color.Transparent;
            this.cup4.Location = new System.Drawing.Point(236, 130);
            this.cup4.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.cup4.MaxValue = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.cup4.Name = "cup4";
            this.cup4.NO = "4";
            this.cup4.Size = new System.Drawing.Size(40, 80);
            this.cup4.TabIndex = 118;
            this.cup4.Title = "";
            this.cup4.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            // 
            // AbsCheck
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(349, 244);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cup4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cup3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cup1);
            this.Controls.Add(this.cup2);
            this.Name = "AbsCheck";
            this.Text = "吸光度显示";
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private FADM_Control.Cup cup1;
        private FADM_Control.Cup cup2;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsm_Online;
        private System.Windows.Forms.ToolStripMenuItem tsm_Offline;
        private System.Windows.Forms.ToolStripMenuItem tsm_Stop;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripMenuItem tsm_Reset;
        private System.Windows.Forms.Label label3;
        private FADM_Control.Cup cup3;
        private System.Windows.Forms.Label label4;
        private FADM_Control.Cup cup4;
    }
}