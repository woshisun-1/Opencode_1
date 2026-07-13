using PlcMotionControl.Motion;

namespace PlcMotionControl.UI;

/// <summary>
/// CNC 页面，G 代码编辑器与运动控制
/// </summary>
public class CncControl : UserControl
{
    private readonly CncController _cnc;
    private readonly CncInterpreter _interpreter = new();

    private readonly TextBox _txtEditor;
    private readonly Label _lblPosition;
    private readonly Label _lblLineInfo;
    private readonly Label _lblStatus;
    private readonly Button _btnLoad, _btnStart, _btnPause, _btnStop, _btnStep;
    private readonly TrackBar _trbFeedOverride;
    private readonly Label _lblFeedVal;
    private readonly System.Windows.Forms.Timer _execTimer;
    private readonly DataGridView _dgvCommands;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="cnc">CNC 控制器引用</param>
    public CncControl(CncController cnc)
    {
        _cnc = cnc;
        _cnc.StatusChanged += (_, msg) =>
            _lblStatus.Invoke(() => _lblStatus.Text = msg);

        _interpreter.ParseError += (_, msg) =>
            _lblStatus.Invoke(() => _lblStatus.Text = msg);

        _execTimer = new System.Windows.Forms.Timer { Interval = 100 };
        _execTimer.Tick += (_, _) =>
        {
            if (_cnc.IsRunning && !_cnc.IsPaused)
            {
                _cnc.ExecuteStep();
                UpdateDisplay();
            }
        };

        BackColor = Color.FromArgb(40, 40, 46);
        Font = new Font("Microsoft YaHei UI", 9);

        // 左侧：代码编辑区域
        var leftPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            BackColor = Color.FromArgb(38, 38, 44)
        };

        var title = new Label
        {
            Text = "G 代码编辑器",
            ForeColor = Color.FromArgb(120, 220, 120),
            Font = new Font("Microsoft YaHei UI", 14, FontStyle.Bold),
            Dock = DockStyle.Top, Height = 32
        };

        _txtEditor = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            Font = new Font("Consolas", 10),
            BackColor = Color.FromArgb(22, 25, 30),
            ForeColor = Color.FromArgb(200, 220, 200),
            BorderStyle = BorderStyle.FixedSingle,
            ScrollBars = ScrollBars.Both,
            Text = "G0 X0 Y0 Z0 F500\nG1 X100 Y50 Z10\nG1 X200 Y100 Z20\nG2 X300 Y150 I50 J0\nG0 Z0\nM30"
        };

        leftPanel.Controls.Add(_txtEditor);
        leftPanel.Controls.Add(title);

        // 右侧：控制面板
        var rightPanel = new Panel
        {
            Dock = DockStyle.Right,
            Width = 350,
            BackColor = Color.FromArgb(42, 42, 48)
        };

        // 位置显示
        var posGroup = new GroupBox
        {
            Text = "当前位置", ForeColor = Color.LimeGreen,
            Location = new Point(8, 8), Size = new Size(330, 50),
            BackColor = Color.FromArgb(42, 42, 48)
        };
        _lblPosition = new Label
        {
            Text = "X:   0.000  Y:   0.000  Z:   0.000",
            ForeColor = Color.LimeGreen,
            Font = new Font("Consolas", 12, FontStyle.Bold),
            Location = new Point(8, 20), Size = new Size(310, 24),
            TextAlign = ContentAlignment.MiddleLeft
        };
        posGroup.Controls.Add(_lblPosition);

        // 行信息
        var infoGroup = new GroupBox
        {
            Text = "执行信息", ForeColor = Color.FromArgb(180, 180, 190),
            Location = new Point(8, 64), Size = new Size(330, 50),
            BackColor = Color.FromArgb(42, 42, 48)
        };
        _lblLineInfo = new Label
        {
            Text = "行: 0 / 0   进度: 0%",
            ForeColor = Color.FromArgb(180, 180, 190),
            Font = new Font("Consolas", 10),
            Location = new Point(8, 20), Size = new Size(310, 24),
            TextAlign = ContentAlignment.MiddleLeft
        };
        infoGroup.Controls.Add(_lblLineInfo);

        // 控制按钮
        var btnPanel = new FlowLayoutPanel
        {
            Location = new Point(8, 120),
            Size = new Size(330, 40),
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        _btnLoad = MakeButton("加载", Color.FromArgb(50, 70, 90), Color.White, 65);
        _btnLoad.Click += (_, _) =>
        {
            _cnc.LoadProgram(_txtEditor.Text);
            RefreshCommandList();
        };

        _btnStart = MakeButton("运行", Color.FromArgb(30, 70, 50), Color.FromArgb(80, 220, 120), 65);
        _btnStart.Click += (_, _) =>
        {
            if (!_cnc.IsRunning)
                _cnc.LoadProgram(_txtEditor.Text);
            _cnc.Start();
            _execTimer.Start();
            RefreshCommandList();
        };

        _btnPause = MakeButton("暂停", Color.FromArgb(70, 70, 40), Color.Yellow, 65);
        _btnPause.Click += (_, _) =>
        {
            if (_cnc.IsPaused)
                _cnc.Resume();
            else
                _cnc.Pause();
            _btnPause.Text = _cnc.IsPaused ? "继续" : "暂停";
        };

        _btnStop = MakeButton("停止", Color.FromArgb(70, 30, 30), Color.Red, 65);
        _btnStop.Click += (_, _) => { _cnc.Stop(); _execTimer.Stop(); };

        _btnStep = MakeButton("单步", Color.FromArgb(60, 60, 70), Color.White, 65);
        _btnStep.Click += (_, _) =>
        {
            if (!_cnc.IsRunning) _cnc.Start();
            _cnc.ExecuteStep();
            UpdateDisplay();
        };

        btnPanel.Controls.AddRange(new Control[]
            { _btnLoad, _btnStart, _btnPause, _btnStop, _btnStep });

        // 进给倍率
        var feedGroup = new GroupBox
        {
            Text = "进给倍率", ForeColor = Color.FromArgb(180, 180, 190),
            Location = new Point(8, 168), Size = new Size(330, 60),
            BackColor = Color.FromArgb(42, 42, 48)
        };
        _trbFeedOverride = new TrackBar
        {
            Location = new Point(8, 20), Size = new Size(220, 30),
            Minimum = 10, Maximum = 200, Value = 100,
            TickFrequency = 10, BackColor = Color.FromArgb(42, 42, 48)
        };
        _trbFeedOverride.ValueChanged += (_, _) =>
        {
            _cnc.FeedRateOverride = _trbFeedOverride.Value;
            _lblFeedVal.Text = $"{_trbFeedOverride.Value}%";
        };
        _lblFeedVal = new Label
        {
            Text = "100%", ForeColor = Color.LimeGreen,
            Font = new Font("Consolas", 12, FontStyle.Bold),
            Location = new Point(235, 22), Size = new Size(80, 24)
        };
        feedGroup.Controls.AddRange(new Control[] { _trbFeedOverride, _lblFeedVal });

        // 指令列表
        var cmdGroup = new GroupBox
        {
            Text = "指令列表", ForeColor = Color.FromArgb(180, 180, 190),
            Location = new Point(8, 235), Size = new Size(330, 200),
            BackColor = Color.FromArgb(42, 42, 48)
        };
        _dgvCommands = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(25, 25, 32),
            ForeColor = Color.FromArgb(200, 200, 210),
            BackgroundColor = Color.FromArgb(25, 25, 32),
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            Font = new Font("Consolas", 9),
            ColumnCount = 3
        };
        _dgvCommands.Columns[0].Name = "行"; _dgvCommands.Columns[0].Width = 40;
        _dgvCommands.Columns[1].Name = "指令"; _dgvCommands.Columns[1].Width = 80;
        _dgvCommands.Columns[2].Name = "参数"; _dgvCommands.Columns[2].Width = 180;
        cmdGroup.Controls.Add(_dgvCommands);

        rightPanel.Controls.AddRange(new Control[]
        {
            posGroup, infoGroup, btnPanel, feedGroup, cmdGroup
        });

        // 底部状态
        _lblStatus = new Label
        {
            Text = "就绪",
            ForeColor = Color.FromArgb(150, 200, 150),
            Font = new Font("Consolas", 9),
            Dock = DockStyle.Bottom, Height = 28,
            Padding = new Padding(8, 4, 8, 4),
            BackColor = Color.FromArgb(30, 30, 38)
        };

        Controls.Add(leftPanel);
        Controls.Add(rightPanel);
        Controls.Add(_lblStatus);
    }

    /// <summary>
    /// 周期更新
    /// </summary>
    public void UpdateCycle()
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        _lblPosition.Text = $"X:{_cnc.CurrentX,8:F3}  Y:{_cnc.CurrentY,8:F3}  Z:{_cnc.CurrentZ,8:F3}";
        _lblLineInfo.Text = $"行: {_cnc.CurrentLine} / {_cnc.TotalLines}    进度: {_cnc.Progress:F1}%";
    }

    private void RefreshCommandList()
    {
        _dgvCommands.Rows.Clear();
        if (!_cnc.IsRunning)
        {
            var commands = _interpreter.ParseProgram(_txtEditor.Text);
            foreach (var cmd in commands)
            {
                string typeStr = cmd.Type switch
                {
                    CncCommandType.Rapid => "G0",
                    CncCommandType.Linear => "G1",
                    CncCommandType.ClockwiseArc => "G2",
                    CncCommandType.CounterClockwiseArc => "G3",
                    CncCommandType.Dwell => "G4",
                    CncCommandType.ProgramEnd => "M30",
                    _ => cmd.RawText.Split(' ')[0]
                };
                _dgvCommands.Rows.Add(cmd.LineNumber, typeStr, cmd.RawText);
            }
        }
    }

    private static Button MakeButton(string text, Color bg, Color fg, int width)
    {
        return new Button
        {
            Text = text, Width = width, Height = 32,
            FlatStyle = FlatStyle.Flat, BackColor = bg, ForeColor = fg,
            Font = new Font("Microsoft YaHei UI", 9)
        };
    }
}
