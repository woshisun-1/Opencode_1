namespace GantryRobot;

public partial class MainForm : Form
{
    private readonly MotionController _motion;
    private readonly System.Windows.Forms.Timer _uiTimer;

    public MainForm()
    {
        InitializeComponent();

        _motion = new MotionController();
        _motion.StatusMessage += msg =>
        {
            if (lstLog.IsHandleCreated)
                lstLog.Invoke(() => AddLog(msg));
        };

        gantryView.SetController(_motion);
        _motion.Start();

        _uiTimer = new System.Windows.Forms.Timer { Interval = 50 };
        _uiTimer.Tick += UpdateUI;
        _uiTimer.Start();

        AddLog("系统初始化完成");
        AddLog("三轴龙门机器人控制系统就绪");
    }

    private void UpdateUI(object? sender, EventArgs e)
    {
        lblXPos.Text = $"位置: {_motion.X.Position:F1} mm";
        lblYPos.Text = $"位置: {_motion.Y.Position:F1} mm";
        lblZPos.Text = $"位置: {_motion.Z.Position:F1} mm";

        lblStatus.Text = _motion.X.Moving || _motion.Y.Moving || _motion.Z.Moving
            ? "运动中..." : "就绪";

        lblXPos.ForeColor = _motion.X.Moving ? Color.Yellow : Color.LimeGreen;
        lblYPos.ForeColor = _motion.Y.Moving ? Color.Yellow : Color.LimeGreen;
        lblZPos.ForeColor = _motion.Z.Moving ? Color.Yellow : Color.LimeGreen;
    }

    private void BtnJog_Click(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.Tag is (Axis axis, double delta))
            _motion.Jog(axis, delta);
    }

    private void BtnHome_Click(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.Tag is Axis axis)
            _motion.HomeAxis(axis);
    }

    private void BtnHomeAll_Click(object? sender, EventArgs e)
    {
        _motion.HomeAll();
    }

    private void BtnStop_Click(object? sender, EventArgs e)
    {
        _motion.StopAll();
    }

    private void BtnGo_Click(object? sender, EventArgs e)
    {
        double x = (double)nudAbsX.Value;
        double y = (double)nudAbsY.Value;
        double z = (double)nudAbsZ.Value;
        _motion.MoveTo(x, y, z);
    }

    private void TrkSpeed_ValueChanged(object? sender, EventArgs e)
    {
        if (sender is TrackBar trk)
        {
            Axis axis;
            Label? label = null;

            if (trk == trkXSpeed) { axis = Axis.X; label = lblXSpeedVal; }
            else if (trk == trkYSpeed) { axis = Axis.Y; label = lblYSpeedVal; }
            else if (trk == trkZSpeed) { axis = Axis.Z; label = lblZSpeedVal; }
            else return;

            double pct = trk.Value;
            double speed = pct / 100.0 * GetMaxSpeed(axis);
            _motion.SetSpeed(axis, speed);
            if (label != null)
                label.Text = $"速度: {pct:F0}%";
        }
    }

    private static double GetMaxSpeed(Axis axis) => axis switch
    {
        Axis.X => 200,
        Axis.Y => 200,
        Axis.Z => 100,
        _ => 200
    };

    private void AddLog(string msg)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        lstLog.Items.Add($"[{timestamp}] {msg}");
        if (lstLog.Items.Count > 200)
            lstLog.Items.RemoveAt(0);
        lstLog.TopIndex = lstLog.Items.Count - 1;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _motion.Stop();
        base.OnFormClosing(e);
    }
}
