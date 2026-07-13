using PlcMotionControl.Models;
using PlcMotionControl.Motion;

namespace PlcMotionControl.UI;

/// <summary>
/// 电子凸轮页面，编辑凸轮曲线并实时监视跟随效果
/// </summary>
public class CamControl : UserControl
{
    private readonly ElectronicCam _cam;
    private readonly ListBox _lstPoints;
    private readonly NumericUpDown _nudMaster, _nudSlave;
    private readonly Label _lblStatus;
    private readonly Button _btnEnable, _btnAdd, _btnClear, _btnSine;
    private readonly CamChart _chart;

    private double _simulatedMaster;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="cam">电子凸轮模块引用</param>
    public CamControl(ElectronicCam cam)
    {
        _cam = cam;
        _cam.StatusChanged += (_, msg) =>
            _lblStatus.Invoke(() => _lblStatus.Text = msg);

        BackColor = Color.FromArgb(40, 40, 46);
        Font = new Font("Microsoft YaHei UI", 9);

        // 左侧：曲线编辑
        var leftPanel = new Panel
        {
            Dock = DockStyle.Left,
            Width = 320,
            Padding = new Padding(12),
            BackColor = Color.FromArgb(38, 38, 44)
        };

        var title = new Label
        {
            Text = "电子凸轮",
            ForeColor = Color.FromArgb(100, 200, 255),
            Font = new Font("Microsoft YaHei UI", 18, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 40
        };

        // 主从轴设置
        var lblMaster = new Label
        {
            Text = "主轴位置:",
            ForeColor = Color.FromArgb(180, 180, 190),
            Location = new Point(12, 50),
            Size = new Size(80, 24)
        };
        _nudMaster = new NumericUpDown
        {
            Location = new Point(100, 48),
            Size = new Size(80, 24),
            Minimum = 0,
            Maximum = 1000,
            Value = 0,
            BackColor = Color.FromArgb(50, 50, 56),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            DecimalPlaces = 1
        };

        var lblSlave = new Label
        {
            Text = "从轴位置:",
            ForeColor = Color.FromArgb(180, 180, 190),
            Location = new Point(12, 80),
            Size = new Size(80, 24)
        };
        _nudSlave = new NumericUpDown
        {
            Location = new Point(100, 78),
            Size = new Size(80, 24),
            Minimum = -500,
            Maximum = 500,
            Value = 0,
            BackColor = Color.FromArgb(50, 50, 56),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            DecimalPlaces = 1
        };

        _btnAdd = new Button
        {
            Text = "添加点",
            Location = new Point(200, 48),
            Size = new Size(80, 28),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(50, 70, 90),
            ForeColor = Color.White
        };
        _btnAdd.Click += (_, _) =>
        {
            _cam.AddCamPoint((double)_nudMaster.Value, (double)_nudSlave.Value);
            RefreshPointList();
        };

        // 凸轮曲线点列表
        _lstPoints = new ListBox
        {
            Location = new Point(12, 115),
            Size = new Size(270, 180),
            BackColor = Color.FromArgb(25, 25, 32),
            ForeColor = Color.FromArgb(180, 200, 180),
            Font = new Font("Consolas", 9),
            BorderStyle = BorderStyle.None
        };

        // 生成正弦曲线
        _btnSine = new Button
        {
            Text = "生成正弦曲线",
            Location = new Point(12, 302),
            Size = new Size(130, 28),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(50, 70, 50),
            ForeColor = Color.FromArgb(100, 220, 120)
        };
        _btnSine.Click += (_, _) =>
        {
            _cam.GenerateSineCurve(30, 360, 200);
            RefreshPointList();
            _chart.Invalidate();
        };

        _btnClear = new Button
        {
            Text = "清空",
            Location = new Point(150, 302),
            Size = new Size(80, 28),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(70, 50, 50),
            ForeColor = Color.FromArgb(220, 120, 100)
        };
        _btnClear.Click += (_, _) =>
        {
            _cam.ClearCurve();
            _lstPoints.Items.Clear();
            _chart.Invalidate();
        };

        _btnEnable = new Button
        {
            Text = "启用电子凸轮",
            Location = new Point(12, 340),
            Size = new Size(220, 34),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(30, 70, 50),
            ForeColor = Color.FromArgb(80, 220, 120),
            Font = new Font("Microsoft YaHei UI", 10)
        };
        _btnEnable.Click += (_, _) =>
        {
            if (_cam.IsEnabled)
            {
                _cam.Disable();
                _btnEnable.Text = "启用电子凸轮";
                _btnEnable.BackColor = Color.FromArgb(30, 70, 50);
            }
            else
            {
                _cam.Enable();
                _btnEnable.Text = "停用";
                _btnEnable.BackColor = Color.FromArgb(70, 30, 30);
            }
        };

        leftPanel.Controls.AddRange(new Control[]
        {
            title, lblMaster, _nudMaster, lblSlave, _nudSlave, _btnAdd,
            _lstPoints, _btnSine, _btnClear, _btnEnable
        });

        // 右侧：凸轮曲线图
        _chart = new CamChart(_cam)
        {
            Dock = DockStyle.Fill
        };

        // 底部状态
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

        Controls.Add(_chart);
        Controls.Add(leftPanel);
        Controls.Add(_lblStatus);
    }

    /// <summary>
    /// 周期更新
    /// </summary>
    public void UpdateCycle()
    {
        if (_cam.IsEnabled)
        {
            _simulatedMaster += 1.5;
            if (_simulatedMaster > 720)
                _simulatedMaster -= 720;
            _cam.CalculateSlavePosition(_simulatedMaster);
        }
        _chart.Invalidate();
    }

    private void InitializeComponent()
    {

    }

    private void RefreshPointList()
    {
        _lstPoints.Items.Clear();
        foreach (var pt in _cam.CamCurve)
        {
            _lstPoints.Items.Add($"M:{pt.MasterPosition,8:F1}  S:{pt.SlavePosition,8:F1}");
        }
        _chart.Invalidate();
    }
}

/// <summary>
/// 凸轮曲线绘图控件
/// </summary>
internal class CamChart : Panel
{
    private readonly ElectronicCam _cam;

    public CamChart(ElectronicCam cam)
    {
        _cam = cam;
        BackColor = Color.FromArgb(20, 25, 30);
        Resize += (_, _) => Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        int w = ClientSize.Width - 40;
        int h = ClientSize.Height - 40;
        if (w < 50 || h < 50) return;

        int ox = 20, oy = 20;

        // 背景
        g.Clear(Color.FromArgb(20, 25, 30));

        // 网格
        using var gridPen = new Pen(Color.FromArgb(40, 45, 55), 1);
        for (int i = 0; i <= 10; i++)
        {
            int x = ox + i * w / 10;
            int y = oy + i * h / 10;
            g.DrawLine(gridPen, x, oy, x, oy + h);
            g.DrawLine(gridPen, ox, y, ox + w, y);
        }

        // 边框
        using var framePen = new Pen(Color.FromArgb(100, 140, 200), 1.5f);
        g.DrawRectangle(framePen, ox, oy, w, h);

        if (_cam.CamCurve.Count < 2) return;

        // 计算范围
        double maxMaster = _cam.CamCurve.Max(p => p.MasterPosition);
        double minMaster = _cam.CamCurve.Min(p => p.MasterPosition);
        double maxSlave = _cam.CamCurve.Max(p => p.SlavePosition);
        double minSlave = _cam.CamCurve.Min(p => p.SlavePosition);
        double mRange = Math.Max(maxMaster - minMaster, 1);
        double sRange = Math.Max(maxSlave - minSlave, 1);

        // 绘制曲线
        using var curvePen = new Pen(Color.LimeGreen, 2);
        for (int i = 0; i < _cam.CamCurve.Count - 1; i++)
        {
            var p1 = _cam.CamCurve[i];
            var p2 = _cam.CamCurve[i + 1];

            float x1 = ox + (float)((p1.MasterPosition - minMaster) / mRange * w);
            float y1 = oy + h - (float)((p1.SlavePosition - minSlave) / sRange * h);
            float x2 = ox + (float)((p2.MasterPosition - minMaster) / mRange * w);
            float y2 = oy + h - (float)((p2.SlavePosition - minSlave) / sRange * h);

            g.DrawLine(curvePen, x1, y1, x2, y2);
        }

        // 绘制数据点
        using var dotBrush = new SolidBrush(Color.Yellow);
        foreach (var pt in _cam.CamCurve)
        {
            float x = ox + (float)((pt.MasterPosition - minMaster) / mRange * w);
            float y = oy + h - (float)((pt.SlavePosition - minSlave) / sRange * h);
            g.FillEllipse(dotBrush, x - 3, y - 3, 6, 6);
        }

        // 坐标轴标签
        using var lblFont = new Font("Segoe UI", 8);
        using var lblBrush = new SolidBrush(Color.FromArgb(130, 130, 140));
        g.DrawString("主轴位置", lblFont, lblBrush, ox + w / 2 - 20, oy + h + 6);
        using var rotate = new System.Drawing.Drawing2D.Matrix();
        g.TranslateTransform(ox - 30, oy + h / 2);
        g.RotateTransform(-90);
        g.DrawString("从轴位置", lblFont, lblBrush, -20, 0);
        g.ResetTransform();
    }
}
