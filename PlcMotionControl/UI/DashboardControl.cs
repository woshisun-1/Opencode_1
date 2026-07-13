using PlcMotionControl.Communication;
using PlcMotionControl.Models;

namespace PlcMotionControl.UI;

/// <summary>
/// 仪表盘页面，显示系统整体运行状态和各轴实时数据
/// </summary>
public class DashboardControl : UserControl
{
    private readonly TableLayoutPanel _mainLayout;
    private readonly Panel _pnlAxisX, _pnlAxisY, _pnlAxisZ;
    private readonly Label _lblXPos, _lblYPos, _lblZPos;
    private readonly Label _lblXStatus, _lblYStatus, _lblZStatus;
    private readonly Label _lblSysStatus, _lblCommStatus, _lblMode;
    private readonly Label _lblProtocol;
    private readonly ProgressBar _pbX, _pbY, _pbZ;

    private TagManager? _tagManager;

    /// <summary>
    /// 构造函数，初始化仪表盘界面
    /// </summary>
    public DashboardControl()
    {
        BackColor = Color.FromArgb(40, 40, 46);
        Font = new Font("Microsoft YaHei UI", 9);

        _mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 3,
            Padding = new Padding(12)
        };
        _mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        _mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        _mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        _mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 180));
        _mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 180));

        // 系统状态栏（跨三列）
        var pnlTopBar = CreateStatusBar(out _lblSysStatus, out _lblCommStatus, out _lblMode, out _lblProtocol);
        _mainLayout.Controls.Add(pnlTopBar, 0, 0);
        _mainLayout.SetColumnSpan(pnlTopBar, 3);

        // X 轴状态面板
        _pnlAxisX = CreateAxisPanel("X 轴", Color.FromArgb(255, 120, 100), out _lblXPos, out _lblXStatus, out _pbX);
        _mainLayout.Controls.Add(_pnlAxisX, 0, 1);

        // Y 轴状态面板
        _pnlAxisY = CreateAxisPanel("Y 轴", Color.FromArgb(100, 200, 255), out _lblYPos, out _lblYStatus, out _pbY);
        _mainLayout.Controls.Add(_pnlAxisY, 1, 1);

        // Z 轴状态面板
        _pnlAxisZ = CreateAxisPanel("Z 轴", Color.FromArgb(120, 220, 120), out _lblZPos, out _lblZStatus, out _pbZ);
        _mainLayout.Controls.Add(_pnlAxisZ, 2, 1);

        // 底部信息栏
        var pnlBottom = CreateBottomInfo();
        _mainLayout.Controls.Add(pnlBottom, 0, 2);
        _mainLayout.SetColumnSpan(pnlBottom, 3);

        Controls.Add(_mainLayout);
    }

    /// <summary>
    /// 连接 TagManager 用于读取通信状态
    /// </summary>
    public void SetTagManager(TagManager manager)
    {
        _tagManager = manager;
    }

    /// <summary>
    /// 更新各轴显示数据，由外部定时器调用
    /// </summary>
    public void UpdateAxes(double xPos, double yPos, double zPos,
        bool xMoving, bool yMoving, bool zMoving)
    {
        _lblXPos.Text = $"{xPos,8:F2} mm";
        _lblYPos.Text = $"{yPos,8:F2} mm";
        _lblZPos.Text = $"{zPos,8:F2} mm";

        _lblXStatus.Text = xMoving ? "运动中" : "就绪";
        _lblYStatus.Text = yMoving ? "运动中" : "就绪";
        _lblZStatus.Text = zMoving ? "运动中" : "就绪";

        _lblXStatus.ForeColor = xMoving ? Color.Yellow : Color.LimeGreen;
        _lblYStatus.ForeColor = yMoving ? Color.Yellow : Color.LimeGreen;
        _lblZStatus.ForeColor = zMoving ? Color.Yellow : Color.LimeGreen;

        // 更新进度条（模拟位置百分比，需根据实际行程归算）
        _pbX.Value = (int)Math.Clamp(xPos / 500 * 100, 0, 100);
        _pbY.Value = (int)Math.Clamp(yPos / 400 * 100, 0, 100);
        _pbZ.Value = (int)Math.Clamp(zPos / 300 * 100, 0, 100);
    }

    /// <summary>
    /// 更新系统状态
    /// </summary>
    public void UpdateSystemStatus(bool isRunning, string mode, string protocol)
    {
        _lblSysStatus.Text = isRunning ? "系统运行中" : "系统就绪";
        _lblSysStatus.ForeColor = isRunning ? Color.LimeGreen : Color.FromArgb(180, 180, 190);
        _lblMode.Text = $"模式: {mode}";
        _lblCommStatus.Text = _tagManager?.IsConnected == true
            ? "PLC 已连接" : "PLC 未连接";
        _lblCommStatus.ForeColor = _tagManager?.IsConnected == true
            ? Color.LimeGreen : Color.OrangeRed;
        _lblProtocol.Text = $"通信协议: {_tagManager?.ActiveProtocol ?? "未设置"}";
    }

    /// <summary>创建顶部状态栏</summary>
    private static Panel CreateStatusBar(out Label sysStatus, out Label commStatus,
        out Label mode, out Label protocol)
    {
        var panel = new Panel
        {
            Height = 80,
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(35, 35, 42),
            Padding = new Padding(12)
        };

        sysStatus = new Label
        {
            Text = "系统就绪",
            ForeColor = Color.FromArgb(180, 180, 190),
            Font = new Font("Microsoft YaHei UI", 14, FontStyle.Bold),
            Location = new Point(12, 10),
            Size = new Size(200, 30)
        };
        commStatus = new Label
        {
            Text = "PLC 未连接",
            ForeColor = Color.OrangeRed,
            Font = new Font("Microsoft YaHei UI", 10),
            Location = new Point(12, 44),
            Size = new Size(160, 22)
        };
        mode = new Label
        {
            Text = "模式: 手动",
            ForeColor = Color.FromArgb(180, 180, 190),
            Font = new Font("Microsoft YaHei UI", 10),
            Location = new Point(380, 44),
            Size = new Size(160, 22)
        };
        protocol = new Label
        {
            Text = "通信协议: 未设置",
            ForeColor = Color.FromArgb(180, 180, 190),
            Font = new Font("Microsoft YaHei UI", 10),
            Location = new Point(580, 44),
            Size = new Size(200, 22)
        };

        panel.Controls.AddRange(new Control[] { sysStatus, commStatus, mode, protocol });
        return panel;
    }

    /// <summary>创建单个轴状态面板</summary>
    private static Panel CreateAxisPanel(string axisName, Color axisColor,
        out Label posLabel, out Label statusLabel, out ProgressBar progressBar)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(45, 45, 52),
            Margin = new Padding(6),
            Padding = new Padding(12)
        };

        var title = new Label
        {
            Text = axisName,
            ForeColor = axisColor,
            Font = new Font("Microsoft YaHei UI", 16, FontStyle.Bold),
            Location = new Point(12, 10),
            Size = new Size(200, 32)
        };

        posLabel = new Label
        {
            Text = "0.00 mm",
            ForeColor = Color.LimeGreen,
            Font = new Font("Consolas", 22, FontStyle.Bold),
            Location = new Point(12, 50),
            Size = new Size(260, 40),
            TextAlign = ContentAlignment.MiddleLeft
        };

        statusLabel = new Label
        {
            Text = "就绪",
            ForeColor = Color.LimeGreen,
            Font = new Font("Microsoft YaHei UI", 10),
            Location = new Point(12, 100),
            Size = new Size(120, 24)
        };

        progressBar = new ProgressBar
        {
            Location = new Point(12, 130),
            Size = new Size(260, 16),
            Minimum = 0,
            Maximum = 100,
            Value = 0,
            Style = ProgressBarStyle.Continuous
        };

        panel.Controls.AddRange(new Control[] { title, posLabel, statusLabel, progressBar });
        return panel;
    }

    /// <summary>创建底部信息栏</summary>
    private static Panel CreateBottomInfo()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(35, 35, 42),
            Padding = new Padding(12),
            Height = 180
        };

        var infoLabel = new Label
        {
            Text = "三轴龙门机器人控制系统 | 支持 OPC UA + Modbus TCP 双协议通信\r\n" +
                   "功能：电子齿轮 · 电子凸轮 · 追飞剪 · CNC G 代码",
            ForeColor = Color.FromArgb(140, 140, 150),
            Font = new Font("Microsoft YaHei UI", 10),
            Location = new Point(12, 10),
            Size = new Size(700, 50)
        };

        var versionLabel = new Label
        {
            Text = $"v1.0.0 | {DateTime.Now:yyyy-MM-dd}",
            ForeColor = Color.FromArgb(100, 100, 110),
            Font = new Font("Consolas", 9),
            Location = new Point(12, 70),
            Size = new Size(300, 20)
        };

        panel.Controls.AddRange(new Control[] { infoLabel, versionLabel });
        return panel;
    }
}
