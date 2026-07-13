using PlcMotionControl.Motion;

namespace PlcMotionControl.UI;

/// <summary>
/// 电子齿轮页面，配置主从轴齿轮比并监视跟随状态
/// </summary>
public class GearControl : UserControl
{
    private readonly ElectronicGear _gear;

    private readonly ComboBox _cmbMasterAxis;
    private readonly ComboBox _cmbSlaveAxis;
    private readonly NumericUpDown _nudNumerator;
    private readonly NumericUpDown _nudDenominator;
    private readonly Label _lblRatio;
    private readonly Label _lblMasterPos;
    private readonly Label _lblSlavePos;
    private readonly Label _lblStatus;
    private readonly Button _btnEnable;
    private readonly Button _btnReset;
    private readonly CheckBox _chkFollow;

    private double _simulatedMaster;
    private double _slaveOutput;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="gear">电子齿轮模块引用</param>
    public GearControl(ElectronicGear gear)
    {
        _gear = gear;
        _gear.StatusChanged += (_, msg) =>
            _lblStatus.Invoke(() => _lblStatus.Text = msg);

        BackColor = Color.FromArgb(40, 40, 46);
        Font = new Font("Microsoft YaHei UI", 9);

        // 主布局
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 7,
            Padding = new Padding(20)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        // 标题
        var title = new Label
        {
            Text = "电子齿轮",
            ForeColor = Color.FromArgb(255, 120, 100),
            Font = new Font("Microsoft YaHei UI", 18, FontStyle.Bold),
            Dock = DockStyle.Fill,
            Height = 40
        };
        layout.Controls.Add(title, 0, 0);
        layout.SetColumnSpan(title, 2);

        // 主轴选择
        layout.Controls.Add(CreateLabel("主轴（主动轴）:"), 0, 1);
        _cmbMasterAxis = CreateCombo("主轴A", "主轴B", "物料轴");
        layout.Controls.Add(_cmbMasterAxis, 1, 1);

        // 从轴选择
        layout.Controls.Add(CreateLabel("从轴（从动轴）:"), 0, 2);
        _cmbSlaveAxis = CreateCombo("从轴C", "从轴B", "剪刀轴");
        layout.Controls.Add(_cmbSlaveAxis, 1, 2);
        _cmbSlaveAxis.SelectedIndex = 0;

        // 齿轮比设置
        layout.Controls.Add(CreateLabel("齿轮比 (分子:分母):"), 0, 3);

        var ratioPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        _nudNumerator = new NumericUpDown
        {
            Minimum = 0.001m, Maximum = 100, Value = 1, DecimalPlaces = 3,
            Width = 80, BackColor = Color.FromArgb(50, 50, 56),
            ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle
        };
        var lblColon = new Label
        {
            Text = " : ",
            ForeColor = Color.White, Font = new Font("Consolas", 14, FontStyle.Bold),
            AutoSize = true, TextAlign = ContentAlignment.MiddleCenter
        };
        _nudDenominator = new NumericUpDown
        {
            Minimum = 0.001m, Maximum = 100, Value = 1, DecimalPlaces = 3,
            Width = 80, BackColor = Color.FromArgb(50, 50, 56),
            ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle
        };
        var btnApplyRatio = new Button
        {
            Text = "应用",
            Width = 60, Height = 28, BackColor = Color.FromArgb(60, 70, 90),
            ForeColor = Color.White, FlatStyle = FlatStyle.Flat
        };
        btnApplyRatio.Click += (_, _) =>
        {
            _gear.SetRatio((double)_nudNumerator.Value, (double)_nudDenominator.Value);
            UpdateRatioDisplay();
        };

        _lblRatio = new Label
        {
            Text = "= 1.0000",
            ForeColor = Color.LimeGreen,
            Font = new Font("Consolas", 11, FontStyle.Bold),
            AutoSize = true,
            Margin = new Padding(10, 5, 0, 0)
        };

        ratioPanel.Controls.AddRange(new Control[]
            { _nudNumerator, lblColon, _nudDenominator, btnApplyRatio, _lblRatio });
        layout.Controls.Add(ratioPanel, 1, 3);

        // 位置显示
        layout.Controls.Add(CreateLabel("主轴位置:"), 0, 4);
        _lblMasterPos = CreateValueLabel("0.00");
        layout.Controls.Add(_lblMasterPos, 1, 4);

        layout.Controls.Add(CreateLabel("从轴位置:"), 0, 5);
        _lblSlavePos = CreateValueLabel("0.00");
        layout.Controls.Add(_lblSlavePos, 1, 5);

        // 控制按钮
        var ctrlPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false, Height = 40
        };

        _btnEnable = new Button
        {
            Text = "启用电子齿轮", Width = 140, Height = 34,
            BackColor = Color.FromArgb(30, 70, 50),
            ForeColor = Color.FromArgb(80, 220, 120),
            FlatStyle = FlatStyle.Flat
        };
        _btnEnable.Click += (_, _) =>
        {
            if (_gear.IsEnabled)
            {
                _gear.Disable();
                _btnEnable.Text = "启用电子齿轮";
                _btnEnable.BackColor = Color.FromArgb(30, 70, 50);
            }
            else
            {
                _gear.MasterAxisName = _cmbMasterAxis.Text;
                _gear.SlaveAxisName = _cmbSlaveAxis.Text;
                _gear.SetRatio((double)_nudNumerator.Value, (double)_nudDenominator.Value);
                _gear.Enable();
                _btnEnable.Text = "停用";
                _btnEnable.BackColor = Color.FromArgb(70, 30, 30);
            }
        };

        _btnReset = new Button
        {
            Text = "重置从轴位置", Width = 120, Height = 34,
            BackColor = Color.FromArgb(60, 60, 70),
            ForeColor = Color.White, FlatStyle = FlatStyle.Flat
        };
        _btnReset.Click += (_, _) => _gear.ResetSlavePosition(0);

        _chkFollow = new CheckBox
        {
            Text = "模拟主轴运动",
            ForeColor = Color.FromArgb(180, 180, 190),
            Checked = true
        };

        ctrlPanel.Controls.AddRange(new Control[] { _btnEnable, _btnReset, _chkFollow });
        layout.Controls.Add(ctrlPanel, 0, 6);
        layout.SetColumnSpan(ctrlPanel, 2);

        // 状态标签
        _lblStatus = new Label
        {
            Text = "就绪",
            ForeColor = Color.FromArgb(150, 200, 150),
            Font = new Font("Consolas", 9),
            Dock = DockStyle.Bottom,
            Height = 28,
            Padding = new Padding(8, 4, 8, 4),
            BackColor = Color.FromArgb(30, 30, 38)
        };

        Controls.Add(layout);
        Controls.Add(_lblStatus);
    }

    /// <summary>
    /// 周期更新（由外部定时器调用）
    /// </summary>
    public void UpdateCycle()
    {
        if (_chkFollow.Checked && _gear.IsEnabled)
        {
            // 模拟主轴连续旋转
            _simulatedMaster += 2;
            if (_simulatedMaster > 3600)
                _simulatedMaster = 0;

            _slaveOutput = _gear.UpdateMasterPosition(_simulatedMaster);
        }

        _lblMasterPos.Text = $"{_simulatedMaster,8:F2} mm";
        _lblSlavePos.Text = $"{_slaveOutput,8:F2} mm";
    }

    private void UpdateRatioDisplay()
    {
        _lblRatio.Text = $"= {_gear.GearRatio:F4}";
    }

    private static Label CreateLabel(string text)
    {
        return new Label
        {
            Text = text,
            ForeColor = Color.FromArgb(180, 180, 190),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Height = 32
        };
    }

    private static Label CreateValueLabel(string value)
    {
        return new Label
        {
            Text = value,
            ForeColor = Color.LimeGreen,
            Font = new Font("Consolas", 14, FontStyle.Bold),
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Height = 32
        };
    }

    private static ComboBox CreateCombo(params string[] items)
    {
        var cb = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(50, 50, 56),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Height = 28
        };
        cb.Items.AddRange(items);
        cb.SelectedIndex = 0;
        return cb;
    }
}
