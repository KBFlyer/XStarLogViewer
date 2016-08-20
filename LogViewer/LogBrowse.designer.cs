
namespace AutelXSPLogViewer
{
    partial class LogBrowse
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogBrowse));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.splitContainerGrf = new System.Windows.Forms.SplitContainer();
            this.zg1 = new ZedGraph.ZedGraphControl();
            this.gMapControl1 = new GMap.NET.WindowsForms.GMapControl();
            this.splitContainerLog = new System.Windows.Forms.SplitContainer();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showGraphToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mapTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.satToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.roadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hybridToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.analysisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainerH1Top = new System.Windows.Forms.SplitContainer();
            this.chk_time = new System.Windows.Forms.CheckBox();
            this.CMB_preselect = new System.Windows.Forms.ComboBox();
            this.BUT_cleargraph = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.lblProgMsg = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerGrf)).BeginInit();
            this.splitContainerGrf.Panel1.SuspendLayout();
            this.splitContainerGrf.Panel2.SuspendLayout();
            this.splitContainerGrf.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerLog)).BeginInit();
            this.splitContainerLog.Panel1.SuspendLayout();
            this.splitContainerLog.Panel2.SuspendLayout();
            this.splitContainerLog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerH1Top)).BeginInit();
            this.splitContainerH1Top.Panel1.SuspendLayout();
            this.splitContainerH1Top.Panel2.SuspendLayout();
            this.splitContainerH1Top.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerGrf
            // 
            this.splitContainerGrf.BackColor = System.Drawing.Color.OrangeRed;
            resources.ApplyResources(this.splitContainerGrf, "splitContainerGrf");
            this.splitContainerGrf.Name = "splitContainerGrf";
            // 
            // splitContainerGrf.Panel1
            // 
            this.splitContainerGrf.Panel1.BackColor = System.Drawing.SystemColors.ActiveCaption;
            resources.ApplyResources(this.splitContainerGrf.Panel1, "splitContainerGrf.Panel1");
            this.splitContainerGrf.Panel1.Controls.Add(this.zg1);
            // 
            // splitContainerGrf.Panel2
            // 
            this.splitContainerGrf.Panel2.Controls.Add(this.gMapControl1);
            // 
            // zg1
            // 
            resources.ApplyResources(this.zg1, "zg1");
            this.zg1.IsSynchronizeYAxes = true;
            this.zg1.Name = "zg1";
            this.zg1.ScrollGrace = 0D;
            this.zg1.ScrollMaxX = 0D;
            this.zg1.ScrollMaxY = 0D;
            this.zg1.ScrollMaxY2 = 0D;
            this.zg1.ScrollMinX = 0D;
            this.zg1.ScrollMinY = 0D;
            this.zg1.ScrollMinY2 = 0D;
            this.zg1.UseExtendedPrintDialog = true;
            this.zg1.ContextMenuBuilder += new ZedGraph.ZedGraphControl.ContextMenuBuilderEventHandler(this.zg1_ContextMenuBuilder);
            this.zg1.ZoomEvent += new ZedGraph.ZedGraphControl.ZoomEventHandler(this.zg1_ZoomEvent);
            this.zg1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.zg1_MouseDoubleClick);
            // 
            // gMapControl1
            // 
            this.gMapControl1.BackColor = System.Drawing.Color.White;
            this.gMapControl1.Bearing = 0F;
            this.gMapControl1.CanDragMap = true;
            resources.ApplyResources(this.gMapControl1, "gMapControl1");
            this.gMapControl1.EmptyTileColor = System.Drawing.Color.Navy;
            this.gMapControl1.GrayScaleMode = false;
            this.gMapControl1.HelperLineOption = GMap.NET.WindowsForms.HelperLineOptions.DontShow;
            this.gMapControl1.LevelsKeepInMemmory = 5;
            this.gMapControl1.MarkersEnabled = true;
            this.gMapControl1.MaxZoom = 2;
            this.gMapControl1.MinZoom = 2;
            this.gMapControl1.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
            this.gMapControl1.Name = "gMapControl1";
            this.gMapControl1.NegativeMode = false;
            this.gMapControl1.PolygonsEnabled = true;
            this.gMapControl1.RetryLoadTile = 0;
            this.gMapControl1.RoutesEnabled = true;
            this.gMapControl1.ScaleMode = GMap.NET.WindowsForms.ScaleModes.Integer;
            this.gMapControl1.SelectedAreaFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(65)))), ((int)(((byte)(105)))), ((int)(((byte)(225)))));
            this.gMapControl1.ShowTileGridLines = false;
            this.gMapControl1.Zoom = 0D;
            this.gMapControl1.OnRouteClick += new GMap.NET.WindowsForms.RouteClick(this.myGMAP1_OnRouteClick);
            // 
            // splitContainerLog
            // 
            this.splitContainerLog.BackColor = System.Drawing.Color.OrangeRed;
            resources.ApplyResources(this.splitContainerLog, "splitContainerLog");
            this.splitContainerLog.Name = "splitContainerLog";
            // 
            // splitContainerLog.Panel1
            // 
            this.splitContainerLog.Panel1.Controls.Add(this.statusStrip1);
            this.splitContainerLog.Panel1.Controls.Add(this.dataGridView1);
            // 
            // splitContainerLog.Panel2
            // 
            this.splitContainerLog.Panel2.Controls.Add(this.treeView1);
            resources.ApplyResources(this.splitContainerLog.Panel2, "splitContainerLog.Panel2");
            // 
            // statusStrip1
            // 
            resources.ApplyResources(this.statusStrip1, "statusStrip1");
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.statusStrip1_ItemClicked);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.PeachPuff;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.OrangeRed;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.Black;
            this.dataGridView1.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.OrangeRed;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.OrangeRed;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.OrangeRed;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle3;
            resources.ApplyResources(this.dataGridView1, "dataGridView1");
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.Black;
            this.dataGridView1.RowsDefaultCellStyle = dataGridViewCellStyle4;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellDoubleClick);
            this.dataGridView1.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.dataGridView1_CellValueNeeded);
            this.dataGridView1.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView1_ColumnHeaderMouseClick);
            this.dataGridView1.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_RowEnter);
            this.dataGridView1.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.dataGridView1_RowPostPaint);
            // 
            // treeView1
            // 
            this.treeView1.CheckBoxes = true;
            resources.ApplyResources(this.treeView1, "treeView1");
            this.treeView1.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this.treeView1.Name = "treeView1";
            this.treeView1.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            ((System.Windows.Forms.TreeNode)(resources.GetObject("treeView1.Nodes"))),
            ((System.Windows.Forms.TreeNode)(resources.GetObject("treeView1.Nodes1")))});
            this.treeView1.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterCheck);
            this.treeView1.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(this.treeView1_DrawNode);
            this.treeView1.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseClick);
            this.treeView1.DoubleClick += new System.EventHandler(this.treeView1_DoubleClick);
            // 
            // progressBar1
            // 
            resources.ApplyResources(this.progressBar1, "progressBar1");
            this.progressBar1.Name = "progressBar1";
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.LightGray;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.analysisToolStripMenuItem});
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.Name = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            resources.ApplyResources(this.openToolStripMenuItem, "openToolStripMenuItem");
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click_1);
            // 
            // saveAsToolStripMenuItem
            // 
            resources.ApplyResources(this.saveAsToolStripMenuItem, "saveAsToolStripMenuItem");
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click_1);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showGraphToolStripMenuItem,
            this.showMapToolStripMenuItem,
            this.mapTypeToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            resources.ApplyResources(this.viewToolStripMenuItem, "viewToolStripMenuItem");
            this.viewToolStripMenuItem.Click += new System.EventHandler(this.viewToolStripMenuItem_Click);
            // 
            // showGraphToolStripMenuItem
            // 
            this.showGraphToolStripMenuItem.Checked = true;
            this.showGraphToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showGraphToolStripMenuItem.Name = "showGraphToolStripMenuItem";
            resources.ApplyResources(this.showGraphToolStripMenuItem, "showGraphToolStripMenuItem");
            this.showGraphToolStripMenuItem.Click += new System.EventHandler(this.showGraphToolStripMenuItem_Click);
            // 
            // showMapToolStripMenuItem
            // 
            this.showMapToolStripMenuItem.Checked = true;
            this.showMapToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showMapToolStripMenuItem.Name = "showMapToolStripMenuItem";
            resources.ApplyResources(this.showMapToolStripMenuItem, "showMapToolStripMenuItem");
            this.showMapToolStripMenuItem.Click += new System.EventHandler(this.showMapToolStripMenuItem_Click);
            // 
            // mapTypeToolStripMenuItem
            // 
            this.mapTypeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.satToolStripMenuItem,
            this.roadToolStripMenuItem,
            this.hybridToolStripMenuItem});
            this.mapTypeToolStripMenuItem.Name = "mapTypeToolStripMenuItem";
            resources.ApplyResources(this.mapTypeToolStripMenuItem, "mapTypeToolStripMenuItem");
            // 
            // satToolStripMenuItem
            // 
            this.satToolStripMenuItem.Checked = true;
            this.satToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.satToolStripMenuItem.Name = "satToolStripMenuItem";
            resources.ApplyResources(this.satToolStripMenuItem, "satToolStripMenuItem");
            this.satToolStripMenuItem.Click += new System.EventHandler(this.satToolStripMenuItem_Click);
            // 
            // roadToolStripMenuItem
            // 
            this.roadToolStripMenuItem.Name = "roadToolStripMenuItem";
            resources.ApplyResources(this.roadToolStripMenuItem, "roadToolStripMenuItem");
            this.roadToolStripMenuItem.Click += new System.EventHandler(this.roadToolStripMenuItem_Click);
            // 
            // hybridToolStripMenuItem
            // 
            this.hybridToolStripMenuItem.Name = "hybridToolStripMenuItem";
            resources.ApplyResources(this.hybridToolStripMenuItem, "hybridToolStripMenuItem");
            this.hybridToolStripMenuItem.Click += new System.EventHandler(this.hybridToolStripMenuItem_Click);
            // 
            // analysisToolStripMenuItem
            // 
            this.analysisToolStripMenuItem.Name = "analysisToolStripMenuItem";
            resources.ApplyResources(this.analysisToolStripMenuItem, "analysisToolStripMenuItem");
            // 
            // splitContainerH1Top
            // 
            this.splitContainerH1Top.BackColor = System.Drawing.Color.OrangeRed;
            resources.ApplyResources(this.splitContainerH1Top, "splitContainerH1Top");
            this.splitContainerH1Top.Name = "splitContainerH1Top";
            // 
            // splitContainerH1Top.Panel1
            // 
            this.splitContainerH1Top.Panel1.Controls.Add(this.splitContainerGrf);
            // 
            // splitContainerH1Top.Panel2
            // 
            this.splitContainerH1Top.Panel2.Controls.Add(this.splitContainerLog);
            // 
            // chk_time
            // 
            resources.ApplyResources(this.chk_time, "chk_time");
            this.chk_time.Name = "chk_time";
            this.chk_time.UseVisualStyleBackColor = true;
            // 
            // CMB_preselect
            // 
            this.CMB_preselect.FormattingEnabled = true;
            resources.ApplyResources(this.CMB_preselect, "CMB_preselect");
            this.CMB_preselect.Name = "CMB_preselect";
            this.CMB_preselect.SelectedIndexChanged += new System.EventHandler(this.CMB_preselect_SelectedIndexChanged);
            // 
            // BUT_cleargraph
            // 
            resources.ApplyResources(this.BUT_cleargraph, "BUT_cleargraph");
            this.BUT_cleargraph.Name = "BUT_cleargraph";
            this.BUT_cleargraph.UseVisualStyleBackColor = true;
            // 
            // lblProgMsg
            // 
            resources.ApplyResources(this.lblProgMsg, "lblProgMsg");
            this.lblProgMsg.BackColor = System.Drawing.Color.LightGray;
            this.lblProgMsg.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblProgMsg.Name = "lblProgMsg";
            // 
            // LogBrowse
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.lblProgMsg);
            this.Controls.Add(this.chk_time);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.splitContainerH1Top);
            this.Controls.Add(this.CMB_preselect);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.BUT_cleargraph);
            this.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.HelpButton = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "LogBrowse";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.LogBrowse_FormClosed);
            this.Load += new System.EventHandler(this.LogBrowse_Load);
            this.splitContainerGrf.Panel1.ResumeLayout(false);
            this.splitContainerGrf.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerGrf)).EndInit();
            this.splitContainerGrf.ResumeLayout(false);
            this.splitContainerLog.Panel1.ResumeLayout(false);
            this.splitContainerLog.Panel1.PerformLayout();
            this.splitContainerLog.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerLog)).EndInit();
            this.splitContainerLog.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainerH1Top.Panel1.ResumeLayout(false);
            this.splitContainerH1Top.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerH1Top)).EndInit();
            this.splitContainerH1Top.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.SplitContainer splitContainerH1Top;
        private System.Windows.Forms.SplitContainer splitContainerGrf;
        private ZedGraph.ZedGraphControl zg1;
        private GMap.NET.WindowsForms.GMapControl gMapControl1;
        private System.Windows.Forms.SplitContainer splitContainerLog;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.CheckBox chk_time;
        private System.Windows.Forms.ComboBox CMB_preselect;
        private System.Windows.Forms.Button BUT_cleargraph;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showGraphToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showMapToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mapTypeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem satToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem roadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hybridToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem analysisToolStripMenuItem;
        private System.Windows.Forms.Label lblProgMsg;
    }
}

