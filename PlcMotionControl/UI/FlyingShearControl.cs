using PlcMotionControl.Motion;

namespace PlcMotionControl.UI;

/// <summary>
/// 追飞剪页面，配置飞剪参数并监视剪切过程
/// </summary>
public class FlyingShearControl : UserControl
{
    private readonly FlyingShear _shear;

    private readonly NumericUpDown _nudCutLength;
    private readonly NumericUpDown _nudMaterialSpeed;
    private readonly NumericUpDown _nudSyncZone;
    private readonly NumericUpDown _nudCutDepth;
    private readonly NumericUpDown _nudShearStroke;
    private readonly Label _lblState;
    private readonly Label _lblCutCount;
    private readonly Label _lblShearPos;
    private readonly Label _lblStatus;
    private readonly Button _btnEnable;
    private readonly Button _btnTrigger;
    private readonly Button _btnReset;

    private double _simulatedMaster;
    private double _shearPosition;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="shear">飞剪模块引用</param>
    public FlyingShearControl(FlyingShear shear)
    {
        _shear = shear;
        _shear.StatusChanged += (_, msg) =>
            _lblStatus.Invoke(() => _lblStatus.Text = msg);

        BackColor = Color.FromArgb(40, 40, 46);
        Font = new Font("Microsoft YaHei UI", 9);

        // 主布局
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 10,
            Padding = new Padding(20)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        // 标题
        var title = new Label
        {
            Text = "追飞剪控制",
            ForeColor = Color.FromArgb(240, 200, 80),
            Font = new Font("Microsoft YaHei UI", 18, FontStyle.Bold),
            Height = 40
        };
        layout.Controls.Add(title, 0, 0);
        layout.SetColumnSpan(title, 2);

        // 参数输入
        int row = 1;
        layout.Controls.Add(MakeLabel("剪切长度 (mm):"), 0, row);
        _nudCutLength = MakeNud(100, 5000, 500);
        layout.Controls.Add(_nudCutLength, 1, row++);

        layout.Controls.Add(MakeLabel("物料速度 (mm/s):"), 0, row);
        _nudMaterialSpeed = MakeNud(10, 2000, 100);
        layout.Controls.Add(_nudMaterialSpeed, 1, row++);

        layout.Controls.Add(MakeLabel("同步区长度 (mm):"), 0, row);
        _nudSyncZone = MakeNud(10, 500, 100);
        layout.Controls.Add(_nudSyncZone, 1, row++);

        layout.Controls.Add(MakeLabel("剪刀行程 (mm):"), 0, row);
        _nudShearStroke = MakeNud(10, 500, 200);
        layout.Controls.Add(_nudShearStroke, 1, row++);

        layout.Controls.Add(MakeLabel("剪切深度 (mm):"), 0, row);
        _nudCutDepth = MakeNud(1, 200, 50);
        layout.Controls.Add(_nudCutDepth, 1, row++);

        // 状态显示
        layout.Controls.Add(MakeLabel("当前状态:"), 0, row);
        _lblState = MakeValueLabel("等待剪切");
        layout.Controls.Add(_lblState, 1, row++);

        layout.Controls.Add(MakeLabel("剪切次数:"), 0, row);
        _lblCutCount = MakeValueLabel("0");
        layout.Controls.Add(_lblCutCount, 1, row++);

        layout.Controls.Add(MakeLabel("剪刀位置:"), 0, row);
        _lblShearPos = MakeValueLabel("0.00 mm");
        layout.Controls.Add(_lblShearPos, 1, row++);

        // 按钮
        var btnPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight,
            Height = 40, WrapContents = false
        };

        _btnEnable = new Button
        {
            Text = "启用飞剪", Width = 120, Height = 34,
            FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(30, 70, 50),
            ForeColor = Color.FromArgb(80, 220, 120)
        };
        _btnEnable.Click += (_, _) =>
        {
            if (_shear.IsEnabled)
            {
                _shear.Disable();
                _btnEnable.Text = "启用飞剪";
                _btnEnable.BackColor = Color.FromArgb(30, 70, 50);
            }
            else
            {
                ApplyParams();
                _shear.Enable();
                _btnEnable.Text = "停用";
                _btnEnable.BackColor = Color.FromArgb(70, 30, 30);
            }
        };

        _btnTrigger = new Button
        {
            Text = "手动剪切", Width = 100, Height = 34,
            FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(60, 60, 70),
            ForeColor = Color.White
        };
        _btnTrigger.Click += (_, _) => _shear.TriggerCut();

        _btnReset = new Button
        {
            Text = "重置计数器", Width = 100, Height = 34,
            FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(60, 50, 50),
            ForeColor = Color.FromArgb(220, 140, 120)
        };
        _btnReset.Click += (_, _) => _shear.ResetCounter();

        btnPanel.Controls.AddRange(new Control[] { _btnEnable, _btnTrigger, _btnReset });
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
    /// 周期更新
    /// </summary>
    public void UpdateCycle()
    {
        if (_shear.IsEnabled)
        {
            _simulatedMaster += (double)_nudMaterialSpeed.Value * 0.05;
            _shearPosition = _shear.UpdateMasterPosition((double)_simulatedMaster);
        }

        _lblState.Text = _shear.CurrentState switch
        {
            FlyingShearState.Waiting => "等待剪切",
            FlyingShearState.Synchronizing => "同步中...",
            FlyingShearState.Cutting => "剪切中!",
            FlyingShearState.Returning => "返回中",
            FlyingShearState.Stopped => "已停止",
            _ => "未知"
        };
        _lblState.ForeColor = _shear.CurrentState == FlyingShearState.Cutting
            ? Color.Yellow : Color.LimeGreen;

        _lblCutCount.Text = _shear.CutCount.ToString();
        _lblShearPos.Text = $"{_shearPosition,8:F2} mm";
    }

    private void ApplyParams()
    {
        _shear.CutLength = (double)_nudCutLength.Value;
        _shear.MaterialSpeed = (double)_nudMaterialSpeed.Value;
        _shear.SyncZoneLength = (double)_nudSyncZone.Value;
        _shear.ShearStroke = (double)_nudShearStroke.Value;
        _shear.CutDepth = (double)_nudCutDepth.Value;
        _shear.GenerateCamCurve();
    }

    private static Label MakeLabel(string text)
    {
        return new Label
        {
            Text = text, ForeColor = Color.FromArgb(180, 180, 190),
            Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft,
            Height = 32
        };
    }

    private static Label MakeValueLabel(string text)
    {
        return new Label
        {
            Text = text, ForeColor = Color.LimeGreen,
            Font = new Font("Consolas", 13, FontStyle.Bold),
            Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft,
            Height = 32
        };
    }

    private static NumericUpDown MakeNud(decimal min, decimal max, decimal val)
    {
        return new NumericUpDown
        {
            Minimum = min, Maximum = max, Value = val,
            Width = 120, BackColor = Color.FromArgb(50, 50, 56),
            ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle
        };
    }
}
