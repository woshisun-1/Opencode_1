using PlcMotionControl.Communication;
using PlcMotionControl.Models;

namespace PlcMotionControl.UI;

/// <summary>
/// 通信设置页面，配置 OPC UA 和 Modbus TCP 参数
/// </summary>
public class SettingsControl : UserControl
{
    private readonly AppConfig _config;
    private readonly TagManager _tagManager;

    private readonly TextBox _txtOpcUrl;
    private readonly TextBox _txtModbusIp;
    private readonly NumericUpDown _nudModbusPort;
    private readonly NumericUpDown _nudModbusSlaveId;
    private readonly ComboBox _cmbProtocol;
    private readonly CheckBox _chkSimulation;
    private readonly Label _lblConnStatus;
    private readonly Label _lblActiveProtocol;
    private readonly Label _lblStatus;
    private readonly Button _btnConnect;

    /// <summary>
    /// 连接状态变化事件，供主窗体订阅
    /// </summary>
    public event EventHandler<bool>? ConnectionChanged;

    /// <summary>
    /// 构造函数
    /// </summary>
    public SettingsControl(AppConfig config, TagManager tagManager)
    {
        _config = config;
        _tagManager = tagManager;

        BackColor = Color.FromArgb(40, 40, 46);
        Font = new Font("Microsoft YaHei UI", 9);

        // 主布局
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 8,
            Padding = new Padding(20)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        // 标题
        var title = new Label
        {
            Text = "通信设置",
            ForeColor = Color.FromArgb(240, 200, 80),
            Font = new Font("Microsoft YaHei UI", 18, FontStyle.Bold),
            Height = 40
        };
        layout.Controls.Add(title, 0, 0);
        layout.SetColumnSpan(title, 2);

        // OPC UA 设置
        var opcGroup = new GroupBox
        {
            Text = "OPC UA 设置",
            ForeColor = Color.FromArgb(100, 200, 255),
            BackColor = Color.FromArgb(45, 45, 52),
            Dock = DockStyle.Fill
        };
        var opcLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(6) };
        opcLayout.Controls.Add(new Label { Text = "服务器 URL:", ForeColor = Color.FromArgb(180, 180, 190) }, 0, 0);
        _txtOpcUrl = new TextBox
        {
            Text = _config.OpcUaServerUrl,
            BackColor = Color.FromArgb(50, 50, 56),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Dock = DockStyle.Fill
        };
        opcLayout.Controls.Add(_txtOpcUrl, 1, 0);
        opcGroup.Controls.Add(opcLayout);
        layout.Controls.Add(opcGroup, 0, 1);
        layout.SetColumnSpan(opcGroup, 2);

        // Modbus 设置
        var modGroup = new GroupBox
        {
            Text = "Modbus TCP 设置",
            ForeColor = Color.FromArgb(120, 220, 120),
            BackColor = Color.FromArgb(45, 45, 52),
            Dock = DockStyle.Fill, Height = 120
        };
        var modLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 3, Padding = new Padding(6) };

        modLayout.Controls.Add(new Label { Text = "IP:", ForeColor = Color.FromArgb(180, 180, 190) }, 0, 0);
        _txtModbusIp = new TextBox
        {
            Text = _config.ModbusIpAddress,
            BackColor = Color.FromArgb(50, 50, 56),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        modLayout.Controls.Add(_txtModbusIp, 1, 0);

        modLayout.Controls.Add(new Label { Text = "端口:", ForeColor = Color.FromArgb(180, 180, 190) }, 2, 0);
        _nudModbusPort = new NumericUpDown
        {
            Minimum = 1, Maximum = 65535, Value = _config.ModbusPort,
            Width = 70, BackColor = Color.FromArgb(50, 50, 56),
            ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle
        };
        modLayout.Controls.Add(_nudModbusPort, 3, 0);

        modLayout.Controls.Add(new Label { Text = "从站 ID:", ForeColor = Color.FromArgb(180, 180, 190) }, 0, 1);
        _nudModbusSlaveId = new NumericUpDown
        {
            Minimum = 1, Maximum = 247, Value = _config.ModbusSlaveId,
            Width = 70, BackColor = Color.FromArgb(50, 50, 56),
            ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle
        };
        modLayout.Controls.Add(_nudModbusSlaveId, 1, 1);

        modGroup.Controls.Add(modLayout);
        layout.Controls.Add(modGroup, 0, 2);
        layout.SetColumnSpan(modGroup, 2);

        // 协议选择
        int row = 3;
        layout.Controls.Add(MakeLabel("首选协议:"), 0, row);
        _cmbProtocol = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(50, 50, 56),
            ForeColor = Color.White, FlatStyle = FlatStyle.Flat
        };
        _cmbProtocol.Items.AddRange(new[] { "Auto", "OPCUA", "Modbus" });
        _cmbProtocol.SelectedItem = _config.PreferredProtocol;
        layout.Controls.Add(_cmbProtocol, 1, row++);

        // 模拟模式
        _chkSimulation = new CheckBox
        {
            Text = "模拟模式（无 PLC 时调试用）",
            Checked = _config.SimulationMode,
            ForeColor = Color.FromArgb(180, 180, 190),
            Height = 30
        };
        _chkSimulation.CheckedChanged += (_, _) => _config.SimulationMode = _chkSimulation.Checked;
        layout.Controls.Add(_chkSimulation, 0, row);
        layout.SetColumnSpan(_chkSimulation, 2);
        row++;

        // 连接状态
        layout.Controls.Add(MakeLabel("连接状态:"), 0, row);
        _lblConnStatus = MakeValueLabel("未连接");
        _lblConnStatus.ForeColor = Color.OrangeRed;
        layout.Controls.Add(_lblConnStatus, 1, row++);

        layout.Controls.Add(MakeLabel("活动协议:"), 0, row);
        _lblActiveProtocol = MakeValueLabel("--");
        layout.Controls.Add(_lblActiveProtocol, 1, row++);

        // 连接按钮
        var btnPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight,
            Height = 44
        };
        _btnConnect = new Button
        {
            Text = "连接 PLC", Width = 140, Height = 36,
            FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(30, 70, 50),
            ForeColor = Color.FromArgb(80, 220, 120),
            Font = new Font("Microsoft YaHei UI", 10, FontStyle.Bold)
        };
        _btnConnect.Click += async (_, _) => await ConnectAsync();
        btnPanel.Controls.Add(_btnConnect);

        var btnSave = new Button
        {
            Text = "保存配置", Width = 100, Height = 36,
            FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(50, 60, 80),
            ForeColor = Color.White
        };
        btnSave.Click += (_, _) => SaveConfig();
        btnPanel.Controls.Add(btnSave);

        layout.Controls.Add(btnPanel, 0, row);
        layout.SetColumnSpan(btnPanel, 2);

        // 状态栏
        _lblStatus = new Label
        {
            Text = "就绪",
            ForeColor = Color.FromArgb(150, 200, 150),
            Font = new Font("Consolas", 9),
            Dock = DockStyle.Bottom, Height = 28,
            Padding = new Padding(8, 4, 8, 4),
            BackColor = Color.FromArgb(30, 30, 38)
        };

        Controls.Add(layout);
        Controls.Add(_lblStatus);
    }

    /// <summary>
    /// 保存当前配置到 AppConfig 对象
    /// </summary>
    private void SaveConfig()
    {
        _config.OpcUaServerUrl = _txtOpcUrl.Text;
        _config.ModbusIpAddress = _txtModbusIp.Text;
        _config.ModbusPort = (int)_nudModbusPort.Value;
        _config.ModbusSlaveId = (byte)_nudModbusSlaveId.Value;
        _config.PreferredProtocol = _cmbProtocol.SelectedItem?.ToString() ?? "Auto";
        _config.SimulationMode = _chkSimulation.Checked;

        _lblStatus.Text = "[设置] 配置已保存";
    }

    /// <summary>
    /// 连接/断开 PLC
    /// </summary>
    private async Task ConnectAsync()
    {
        SaveConfig();

        if (_tagManager.IsConnected)
        {
            await _tagManager.DisconnectAsync();
            _btnConnect.Text = "连接 PLC";
            _lblConnStatus.Text = "未连接";
            _lblConnStatus.ForeColor = Color.OrangeRed;
            _lblStatus.Text = "[设置] 已断开连接";
            ConnectionChanged?.Invoke(this, false);
            return;
        }

        _btnConnect.Enabled = false;
        _lblStatus.Text = "[设置] 正在连接...";

        bool success = await _tagManager.ConnectAsync();

        _btnConnect.Enabled = true;

        if (success)
        {
            _btnConnect.Text = "断开";
            _lblConnStatus.Text = "已连接";
            _lblConnStatus.ForeColor = Color.LimeGreen;
            _lblStatus.Text = "[设置] 连接成功";
        }
        else
        {
            _lblConnStatus.Text = "连接失败";
            _lblConnStatus.ForeColor = Color.Red;
            _lblStatus.Text = "[设置] 连接失败，请检查地址和网络";
        }

        _lblActiveProtocol.Text = _tagManager.ActiveProtocol;
        ConnectionChanged?.Invoke(this, success);
    }

    /// <summary>更新连接状态显示</summary>
    public void UpdateStatus()
    {
        _lblConnStatus.Text = _tagManager.IsConnected ? "已连接" : "未连接";
        _lblConnStatus.ForeColor = _tagManager.IsConnected ? Color.LimeGreen : Color.OrangeRed;
        _lblActiveProtocol.Text = _tagManager.ActiveProtocol;
    }

    private static Label MakeLabel(string text)
    {
        return new Label { Text = text, ForeColor = Color.FromArgb(180, 180, 190),
            Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Height = 30 };
    }

    private static Label MakeValueLabel(string text)
    {
        return new Label { Text = text, ForeColor = Color.LimeGreen,
            Font = new Font("Consolas", 10, FontStyle.Bold),
            Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Height = 30 };
    }
}
