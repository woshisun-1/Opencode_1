using PlcMotionControl.Communication;
using PlcMotionControl.Models;
using PlcMotionControl.Motion;
using PlcMotionControl.UI;

namespace PlcMotionControl;

/// <summary>
/// 主窗口：三轴运动控制系统（OPC UA + Modbus TCP 双协议通信）
/// 集成电子齿轮、电子凸轮、追飞剪、CNC 四大功能模块
/// </summary>
public partial class MainForm : Form
{
    // ===== 通信层 =====
    private readonly AppConfig _config;
    private readonly TagManager _tagManager;

    // ===== 运动控制模块 =====
    private readonly ElectronicGear _gearModule;
    private readonly ElectronicCam _camModule;
    private readonly FlyingShear _shearModule;
    private readonly CncController _cncController;

    // ===== UI 页面 =====
    private readonly DashboardControl _dashboardPage;
    private readonly GearControl _gearPage;
    private readonly CamControl _camPage;
    private readonly FlyingShearControl _shearPage;
    private readonly CncControl _cncPage;
    private readonly SettingsControl _settingsPage;

    // ===== 模拟状态（无 PLC 时使用） =====
    private double _simX, _simY, _simZ;
    private bool _simXMoving, _simYMoving, _simZMoving;

    // ===== UI 刷新定时器 =====
    private readonly System.Windows.Forms.Timer _uiTimer;

    /// <summary>
    /// 构造函数，初始化所有模块和 UI
    /// </summary>
    public MainForm()
    {
        // 初始化配置
        _config = new AppConfig
        {
            SimulationMode = true,
            OpcUaServerUrl = "opc.tcp://192.168.1.100:4840",
            ModbusIpAddress = "192.168.1.100",
            ModbusPort = 502,
            ModbusSlaveId = 1,
            PreferredProtocol = "Auto"
        };

        // 初始化通信层
        _tagManager = new TagManager(_config);

        // 初始化运动控制模块
        _gearModule = new ElectronicGear();
        _camModule = new ElectronicCam();
        _shearModule = new FlyingShear();
        _cncController = new CncController();

        // 初始化 UI 页面
        _dashboardPage = new DashboardControl();
        _gearPage = new GearControl(_gearModule);
        _camPage = new CamControl(_camModule);
        _shearPage = new FlyingShearControl(_shearModule);
        _cncPage = new CncControl(_cncController);
        _settingsPage = new SettingsControl(_config, _tagManager);

        // 连接设置页面的通信状态变化事件
        _settingsPage.ConnectionChanged += (_, connected) =>
        {
            if (connected)
                AddLog("PLC 连接成功");
            else
                AddLog("PLC 已断开");
        };

        // 初始化主窗口
        InitializeComponent();

        // 启动 UI 刷新定时器
        _uiTimer = new System.Windows.Forms.Timer
        {
            Interval = _config.UiRefreshIntervalMs
        };
        _uiTimer.Tick += UiTimer_Tick;
        _uiTimer.Start();

        AddLog("系统初始化完成");
        AddLog("通信模式: 模拟模式（可在设置页修改）");
        AddLog($"OPC UA: {_config.OpcUaServerUrl}");
        AddLog($"Modbus TCP: {_config.ModbusIpAddress}:{_config.ModbusPort}");
    }

    /// <summary>
    /// UI 定时器刷新事件
    /// </summary>
    private void UiTimer_Tick(object? sender, EventArgs e)
    {
        // 模拟轴运动（模拟模式）
        if (_config.SimulationMode)
        {
            SimulateAxes();
        }

        // 更新各页面
        _dashboardPage.UpdateAxes(_simX, _simY, _simZ, _simXMoving, _simYMoving, _simZMoving);
        _dashboardPage.UpdateSystemStatus(
            _cncController.IsRunning,
            _cncController.IsRunning ? "CNC 自动" : "手动",
            _tagManager.ActiveProtocol);

        _gearPage.UpdateCycle();
        _camPage.UpdateCycle();
        _shearPage.UpdateCycle();
        _cncPage.UpdateCycle();
        _settingsPage.UpdateStatus();

        // 更新主窗体状态栏
        UpdateStatusBar();
    }

    /// <summary>
    /// 模拟三轴运动（无 PLC 时演示用）
    /// </summary>
    private void SimulateAxes()
    {
        // 简单模拟：各轴带随机扰动的小幅往复运动
        double targetX = 250 + 200 * Math.Sin(Environment.TickCount / 2000.0);
        double targetY = 200 + 150 * Math.Sin(Environment.TickCount / 3000.0 + 1);
        double targetZ = 150 + 100 * Math.Sin(Environment.TickCount / 1500.0 + 2);

        _simXMoving = Math.Abs(_simX - targetX) > 0.5;
        _simYMoving = Math.Abs(_simY - targetY) > 0.5;
        _simZMoving = Math.Abs(_simZ - targetZ) > 0.5;

        _simX += (targetX - _simX) * 0.05;
        _simY += (targetY - _simY) * 0.05;
        _simZ += (targetZ - _simZ) * 0.05;
    }

    /// <summary>
    /// 更新主窗体底部状态栏
    /// </summary>
    private void UpdateStatusBar()
    {
        _lblFooterStatus.Text = _tagManager.IsConnected
            ? "PLC 已连接"
            : "PLC 未连接（模拟模式）";

        _lblFooterPos.Text =
            $"X: {_simX,8:F2}  Y: {_simY,8:F2}  Z: {_simZ,8:F2}";

        _lblFooterProtocol.Text = _tagManager.ActiveProtocol;
    }

    /// <summary>
    /// 添加日志到日志框
    /// </summary>
    public void AddLog(string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        _lstLog.Items.Add($"[{timestamp}] {message}");
        if (_lstLog.Items.Count > 500)
            _lstLog.Items.RemoveAt(0);
        _lstLog.TopIndex = _lstLog.Items.Count - 1;
    }

    /// <summary>
    /// 窗口关闭时清理资源
    /// </summary>
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _uiTimer.Stop();
        _tagManager.Dispose();
        base.OnFormClosing(e);
    }
}
