using System.Drawing;
using System.Windows.Forms;

namespace SmartDyeing.FADM_Control
{
    partial class S_MainBottle_Only
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(S_MainBottle_Only));
            this.PnlBottle = new System.Windows.Forms.Panel();
            this.tmr = new System.Windows.Forms.Timer(this.components);
            this.lab_Low = new System.Windows.Forms.Label();
            this.lab_Expire = new System.Windows.Forms.Label();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsm_Check = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_Self = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_Water = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_Abs = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_InsertAbs = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsm_SignCheck = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_SignSelf = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_SignPause = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_SignStop = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_SignUpdate = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_TestAbs = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_TestBaseAbs = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_TestStanAbs = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_Wash = new System.Windows.Forms.ToolStripMenuItem();
            this.button1 = new System.Windows.Forms.Button();
            this.tsm_Brew = new System.Windows.Forms.ToolStripMenuItem();
            this.tsm_AutoBrew = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.contextMenuStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // PnlBottle
            // 
            resources.ApplyResources(this.PnlBottle, "PnlBottle");
            this.PnlBottle.Name = "PnlBottle";
            // 
            // tmr
            // 
            this.tmr.Enabled = true;
            this.tmr.Interval = 1000;
            this.tmr.Tick += new System.EventHandler(this.Tmr_Tick);
            // 
            // lab_Low
            // 
            this.lab_Low.AutoEllipsis = true;
            resources.ApplyResources(this.lab_Low, "lab_Low");
            this.lab_Low.ForeColor = System.Drawing.Color.Red;
            this.lab_Low.Name = "lab_Low";
            // 
            // lab_Expire
            // 
            this.lab_Expire.AutoEllipsis = true;
            resources.ApplyResources(this.lab_Expire, "lab_Expire");
            this.lab_Expire.ForeColor = System.Drawing.Color.Red;
            this.lab_Expire.Name = "lab_Expire";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsm_Check,
            this.tsm_Self,
            this.tsm_Water,
            this.tsm_Abs,
            this.tsm_InsertAbs,
            this.tsm_AutoBrew});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            resources.ApplyResources(this.contextMenuStrip1, "contextMenuStrip1");
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // tsm_Check
            // 
            this.tsm_Check.Name = "tsm_Check";
            resources.ApplyResources(this.tsm_Check, "tsm_Check");
            this.tsm_Check.Click += new System.EventHandler(this.tsm_CheckAndSelf_Click);
            // 
            // tsm_Self
            // 
            this.tsm_Self.Name = "tsm_Self";
            resources.ApplyResources(this.tsm_Self, "tsm_Self");
            this.tsm_Self.Click += new System.EventHandler(this.tsm_Self_Click);
            // 
            // tsm_Water
            // 
            this.tsm_Water.Name = "tsm_Water";
            resources.ApplyResources(this.tsm_Water, "tsm_Water");
            this.tsm_Water.Click += new System.EventHandler(this.tsm_Water_Click);
            // 
            // tsm_Abs
            // 
            this.tsm_Abs.Name = "tsm_Abs";
            resources.ApplyResources(this.tsm_Abs, "tsm_Abs");
            this.tsm_Abs.Click += new System.EventHandler(this.tsm_Abs_Click);
            // 
            // tsm_InsertAbs
            // 
            this.tsm_InsertAbs.Name = "tsm_InsertAbs";
            resources.ApplyResources(this.tsm_InsertAbs, "tsm_InsertAbs");
            this.tsm_InsertAbs.Click += new System.EventHandler(this.tsm_InsertAbs_Click);
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsm_SignCheck,
            this.tsm_SignSelf,
            this.tsm_SignPause,
            this.tsm_SignStop,
            this.tsm_SignUpdate,
            this.tsm_TestAbs,
            this.tsm_TestBaseAbs,
            this.tsm_TestStanAbs,
            this.tsm_Wash,
            this.tsm_Brew});
            this.contextMenuStrip2.Name = "contextMenuStrip2";
            resources.ApplyResources(this.contextMenuStrip2, "contextMenuStrip2");
            this.contextMenuStrip2.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip2_Opening);
            // 
            // tsm_SignCheck
            // 
            this.tsm_SignCheck.Name = "tsm_SignCheck";
            resources.ApplyResources(this.tsm_SignCheck, "tsm_SignCheck");
            this.tsm_SignCheck.Click += new System.EventHandler(this.tsm_SignCheck_Click);
            // 
            // tsm_SignSelf
            // 
            this.tsm_SignSelf.Name = "tsm_SignSelf";
            resources.ApplyResources(this.tsm_SignSelf, "tsm_SignSelf");
            this.tsm_SignSelf.Click += new System.EventHandler(this.tsm_SignSelf_Click);
            // 
            // tsm_SignPause
            // 
            this.tsm_SignPause.Name = "tsm_SignPause";
            resources.ApplyResources(this.tsm_SignPause, "tsm_SignPause");
            this.tsm_SignPause.Click += new System.EventHandler(this.tsm_SignPause_Click);
            // 
            // tsm_SignStop
            // 
            this.tsm_SignStop.Name = "tsm_SignStop";
            resources.ApplyResources(this.tsm_SignStop, "tsm_SignStop");
            this.tsm_SignStop.Click += new System.EventHandler(this.tsm_SignStop_Click);
            // 
            // tsm_SignUpdate
            // 
            this.tsm_SignUpdate.Name = "tsm_SignUpdate";
            resources.ApplyResources(this.tsm_SignUpdate, "tsm_SignUpdate");
            this.tsm_SignUpdate.Click += new System.EventHandler(this.tsm_SignUpdate_Click);
            // 
            // tsm_TestAbs
            // 
            this.tsm_TestAbs.Name = "tsm_TestAbs";
            resources.ApplyResources(this.tsm_TestAbs, "tsm_TestAbs");
            this.tsm_TestAbs.Click += new System.EventHandler(this.tsm_TestAbs_Click);
            // 
            // tsm_TestBaseAbs
            // 
            this.tsm_TestBaseAbs.Name = "tsm_TestBaseAbs";
            resources.ApplyResources(this.tsm_TestBaseAbs, "tsm_TestBaseAbs");
            this.tsm_TestBaseAbs.Click += new System.EventHandler(this.tsm_TestBaseAbs_Click);
            // 
            // tsm_TestStanAbs
            // 
            this.tsm_TestStanAbs.Name = "tsm_TestStanAbs";
            resources.ApplyResources(this.tsm_TestStanAbs, "tsm_TestStanAbs");
            this.tsm_TestStanAbs.Click += new System.EventHandler(this.tsm_TestStanAbs_Click);
            // 
            // tsm_Wash
            // 
            this.tsm_Wash.Name = "tsm_Wash";
            resources.ApplyResources(this.tsm_Wash, "tsm_Wash");
            this.tsm_Wash.Click += new System.EventHandler(this.tsm_Wash_Click);
            // 
            // button1
            // 
            resources.ApplyResources(this.button1, "button1");
            this.button1.Name = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // tsm_Brew
            // 
            this.tsm_Brew.Name = "tsm_Brew";
            resources.ApplyResources(this.tsm_Brew, "tsm_Brew");
            this.tsm_Brew.Click += new System.EventHandler(this.tsm_Brew_Click);
            // 
            // tsm_AutoBrew
            // 
            this.tsm_AutoBrew.Name = "tsm_AutoBrew";
            resources.ApplyResources(this.tsm_AutoBrew, "tsm_AutoBrew");
            this.tsm_AutoBrew.Click += new System.EventHandler(this.tsm_AutoBrew_Click);
            // 
            // S_MainBottle_Only
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.Controls.Add(this.button1);
            this.Controls.Add(this.lab_Expire);
            this.Controls.Add(this.lab_Low);
            this.Controls.Add(this.PnlBottle);
            this.Name = "S_MainBottle_Only";
            this.contextMenuStrip1.ResumeLayout(false);
            this.contextMenuStrip2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Panel PnlBottle;
        private System.Windows.Forms.Timer tmr;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel tss_lab_bottlelow;
        private Label label1;
        private Label label2;
        private Label lab_Low;
        private Label lab_Expire;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem tsm_Check;
        private ToolStripMenuItem tsm_Self;
        private ToolStripMenuItem tsm_Water;
        private ContextMenuStrip contextMenuStrip2;
        private ToolStripMenuItem tsm_SignCheck;
        private ToolStripMenuItem tsm_SignSelf;
        private ToolStripMenuItem tsm_SignPause;
        private ToolStripMenuItem tsm_SignStop;
        private ToolStripMenuItem tsm_SignUpdate;
        private ToolStripMenuItem tsm_TestAbs;
        private ToolStripMenuItem tsm_TestStanAbs;
        private ToolStripMenuItem tsm_Abs;
        private ToolStripMenuItem tsm_TestBaseAbs;
        private ToolStripMenuItem tsm_InsertAbs;
        private Button button1;
        private ToolStripMenuItem tsm_Wash;
        private ToolStripMenuItem tsm_Brew;
        private ToolStripMenuItem tsm_AutoBrew;
    }
}
