namespace PlcMotionControl;

partial class MainForm
{
    private System.ComponentModel.IContainer components;

    // 顶部 Tab 导航
    private TabControl _tabControl;
    private TabPage _tabDashboard;
    private TabPage _tabGear;
    private TabPage _tabCam;
    private TabPage _tabShear;
    private TabPage _tabCnc;
    private TabPage _tabSettings;

    // 底部状态栏
    private StatusStrip _statusStrip;
    private ToolStripStatusLabel _lblFooterStatus;
    private ToolStripStatusLabel _lblFooterPos;
    private ToolStripStatusLabel _lblFooterProtocol;
    private ListBox _lstLog;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    /// <summary>
    /// 初始化所有 UI 控件
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        // ===== 主窗口 =====
        Text = "三轴龙门机器人运动控制系统 — OPC UA + Modbus TCP";
        ClientSize = new Size(1280, 820);
        MinimumSize = new Size(1024, 680);
        BackColor = Color.FromArgb(40, 40, 46);
        ForeColor = Color.White;
        Font = new Font("Microsoft YaHei UI", 9F);
        StartPosition = FormStartPosition.CenterScreen;
        Icon = null;

        // ===== Tab 导航 =====
        _tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(50, 50, 56),
            ForeColor = Color.White,
            Font = new Font("Microsoft YaHei UI", 10),
            Padding = new Point(8, 4)
        };

        // 仪表盘
        _tabDashboard = new TabPage("仪表盘")
        {
            BackColor = Color.FromArgb(40, 40, 46),
            ForeColor = Color.White
        };
        _dashboardPage.Dock = DockStyle.Fill;
        _tabDashboard.Controls.Add(_dashboardPage);
        _tabControl.Controls.Add(_tabDashboard);

        // 电子齿轮
        _tabGear = new TabPage("电子齿轮")
        {
            BackColor = Color.FromArgb(40, 40, 46),
            ForeColor = Color.White
        };
        _gearPage.Dock = DockStyle.Fill;
        _tabGear.Controls.Add(_gearPage);
        _tabControl.Controls.Add(_tabGear);

        // 电子凸轮
        _tabCam = new TabPage("电子凸轮")
        {
            BackColor = Color.FromArgb(40, 40, 46),
            ForeColor = Color.White
        };
        _camPage.Dock = DockStyle.Fill;
        _tabCam.Controls.Add(_camPage);
        _tabControl.Controls.Add(_tabCam);

        // 追飞剪
        _tabShear = new TabPage("追飞剪")
        {
            BackColor = Color.FromArgb(40, 40, 46),
            ForeColor = Color.White
        };
        _shearPage.Dock = DockStyle.Fill;
        _tabShear.Controls.Add(_shearPage);
        _tabControl.Controls.Add(_tabShear);

        // CNC
        _tabCnc = new TabPage("CNC 控制")
        {
            BackColor = Color.FromArgb(40, 40, 46),
            ForeColor = Color.White
        };
        _cncPage.Dock = DockStyle.Fill;
        _tabCnc.Controls.Add(_cncPage);
        _tabControl.Controls.Add(_tabCnc);

        // 设置
        _tabSettings = new TabPage("通信设置")
        {
            BackColor = Color.FromArgb(40, 40, 46),
            ForeColor = Color.White
        };
        _settingsPage.Dock = DockStyle.Fill;
        _tabSettings.Controls.Add(_settingsPage);
        _tabControl.Controls.Add(_tabSettings);

        // ===== 底部日志面板 =====
        var pnlBottom = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 120,
            BackColor = Color.FromArgb(35, 35, 42),
            Padding = new Padding(4)
        };

        var lblLogTitle = new Label
        {
            Text = "运行日志",
            ForeColor = Color.FromArgb(150, 150, 160),
            Font = new Font("Microsoft YaHei UI", 8),
            Dock = DockStyle.Top,
            Height = 18,
            Padding = new Padding(4, 2, 0, 0)
        };

        _lstLog = new ListBox
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(25, 25, 32),
            ForeColor = Color.FromArgb(160, 190, 160),
            Font = new Font("Consolas", 9),
            BorderStyle = BorderStyle.None,
            HorizontalScrollbar = true
        };

        pnlBottom.Controls.Add(_lstLog);
        pnlBottom.Controls.Add(lblLogTitle);

        // ===== 底部状态条 =====
        _statusStrip = new StatusStrip
        {
            BackColor = Color.FromArgb(30, 30, 38),
            ForeColor = Color.FromArgb(180, 180, 190),
            Font = new Font("Consolas", 9),
            SizingGrip = false
        };

        _lblFooterStatus = new ToolStripStatusLabel
        {
            Text = "PLC 未连接（模拟模式）",
            ForeColor = Color.OrangeRed,
            BorderSides = ToolStripStatusLabelBorderSides.Right,
            Padding = new Padding(8, 2, 8, 2)
        };

        _lblFooterPos = new ToolStripStatusLabel
        {
            Text = "X: 0.00  Y: 0.00  Z: 0.00",
            ForeColor = Color.LimeGreen,
            BorderSides = ToolStripStatusLabelBorderSides.Right,
            Padding = new Padding(8, 2, 8, 2)
        };

        _lblFooterProtocol = new ToolStripStatusLabel
        {
            Text = "模拟模式",
            ForeColor = Color.FromArgb(180, 180, 190),
            Padding = new Padding(8, 2, 8, 2)
        };

        _statusStrip.Items.AddRange(new ToolStripItem[]
        {
            _lblFooterStatus, _lblFooterPos, _lblFooterProtocol
        });

        // ===== 布局 =====
        Controls.Add(_tabControl);
        Controls.Add(pnlBottom);
        Controls.Add(_statusStrip);
    }
}
