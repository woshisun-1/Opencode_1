namespace GantryRobot;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel tlpMain;
    private Panel pnlView;
    private GantryView gantryView;
    private Panel pnlControls;
    private GroupBox grpX;
    private GroupBox grpY;
    private GroupBox grpZ;
    private GroupBox grpGlobal;
    private ListBox lstLog;

    private Label lblXPos;
    private Label lblYPos;
    private Label lblZPos;
    private TrackBar trkXSpeed;
    private TrackBar trkYSpeed;
    private TrackBar trkZSpeed;
    private Label lblXSpeedVal;
    private Label lblYSpeedVal;
    private Label lblZSpeedVal;
    private Button btnXJogN10;
    private Button btnXJogN1;
    private Button btnXJogP1;
    private Button btnXJogP10;
    private Button btnXHome;
    private Button btnYJogN10;
    private Button btnYJogN1;
    private Button btnYJogP1;
    private Button btnYJogP10;
    private Button btnYHome;
    private Button btnZJogN10;
    private Button btnZJogN1;
    private Button btnZJogP1;
    private Button btnZJogP10;
    private Button btnZHome;
    private Button btnHomeAll;
    private Button btnStop;
    private Button btnGo;
    private NumericUpDown nudAbsX;
    private NumericUpDown nudAbsY;
    private NumericUpDown nudAbsZ;
    private Label lblAbsX;
    private Label lblAbsY;
    private Label lblAbsZ;
    private Label lblStatus;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        tlpMain = new TableLayoutPanel();
        pnlView = new Panel();
        gantryView = new GantryView();
        pnlControls = new Panel();
        grpX = new GroupBox();
        grpY = new GroupBox();
        grpZ = new GroupBox();
        grpGlobal = new GroupBox();
        lstLog = new ListBox();
        lblStatus = new Label();

        lblXPos = new Label();
        lblYPos = new Label();
        lblZPos = new Label();
        trkXSpeed = new TrackBar();
        trkYSpeed = new TrackBar();
        trkZSpeed = new TrackBar();
        lblXSpeedVal = new Label();
        lblYSpeedVal = new Label();
        lblZSpeedVal = new Label();

        btnXJogN10 = new Button();
        btnXJogN1 = new Button();
        btnXJogP1 = new Button();
        btnXJogP10 = new Button();
        btnXHome = new Button();
        btnYJogN10 = new Button();
        btnYJogN1 = new Button();
        btnYJogP1 = new Button();
        btnYJogP10 = new Button();
        btnYHome = new Button();
        btnZJogN10 = new Button();
        btnZJogN1 = new Button();
        btnZJogP1 = new Button();
        btnZJogP10 = new Button();
        btnZHome = new Button();
        btnHomeAll = new Button();
        btnStop = new Button();
        btnGo = new Button();
        nudAbsX = new NumericUpDown();
        nudAbsY = new NumericUpDown();
        nudAbsZ = new NumericUpDown();
        lblAbsX = new Label();
        lblAbsY = new Label();
        lblAbsZ = new Label();

        SuspendLayout();

        this.Text = "三轴龙门机器人控制系统";
        this.ClientSize = new Size(1280, 800);
        this.MinimumSize = new Size(960, 600);
        this.BackColor = Color.FromArgb(45, 45, 50);
        this.ForeColor = Color.White;
        this.Font = new Font("Microsoft YaHei UI", 9F);
        this.StartPosition = FormStartPosition.CenterScreen;

        // tlpMain
        tlpMain.Dock = DockStyle.Fill;
        tlpMain.ColumnCount = 2;
        tlpMain.RowCount = 2;
        tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        tlpMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));
        tlpMain.BackColor = Color.FromArgb(45, 45, 50);
        tlpMain.Padding = new Padding(6);

        // pnlView
        pnlView.Dock = DockStyle.Fill;
        pnlView.Padding = new Padding(4);
        pnlView.BackColor = Color.FromArgb(35, 35, 40);
        pnlView.BorderStyle = BorderStyle.FixedSingle;

        gantryView.Dock = DockStyle.Fill;
        gantryView.BackColor = Color.FromArgb(30, 30, 36);

        pnlView.Controls.Add(gantryView);

        // pnlControls
        pnlControls.Dock = DockStyle.Fill;
        pnlControls.AutoScroll = true;
        pnlControls.BackColor = Color.FromArgb(45, 45, 50);
        pnlControls.Padding = new Padding(4, 0, 4, 0);

        // === X轴 ===
        grpX.Text = "X 轴控制";
        grpX.ForeColor = Color.FromArgb(255, 120, 100);
        grpX.BackColor = Color.FromArgb(50, 50, 55);
            grpX.Dock = DockStyle.Top;
            grpX.Height = 120;
            grpX.Padding = new Padding(8);
            grpX.Margin = new Padding(0, 0, 0, 4);

        lblXPos.Text = "位置: 0.0 mm";
        lblXPos.ForeColor = Color.LimeGreen;
        lblXPos.Font = new Font("Consolas", 12, FontStyle.Bold);
        lblXPos.Location = new Point(10, 22);
        lblXPos.Size = new Size(220, 24);

        trkXSpeed.Minimum = 1;
        trkXSpeed.Maximum = 100;
        trkXSpeed.Value = 50;
        trkXSpeed.TickFrequency = 10;
        trkXSpeed.Location = new Point(10, 50);
        trkXSpeed.Size = new Size(180, 30);
        trkXSpeed.BackColor = Color.FromArgb(50, 50, 55);
        trkXSpeed.ValueChanged += TrkSpeed_ValueChanged;

        lblXSpeedVal.Text = "速度: 50%";
        lblXSpeedVal.ForeColor = Color.FromArgb(200, 200, 210);
        lblXSpeedVal.Font = new Font("Segoe UI", 8);
        lblXSpeedVal.Location = new Point(195, 54);
        lblXSpeedVal.Size = new Size(80, 16);
        lblXSpeedVal.Tag = "X";

        int jogY = 80;
        btnXJogN10 = MakeJogBtn("-10", 10, jogY, 40, 28, Axis.X, -10);
        btnXJogN1 = MakeJogBtn("-1", 54, jogY, 32, 28, Axis.X, -1);
        btnXJogP1 = MakeJogBtn("+1", 90, jogY, 32, 28, Axis.X, 1);
        btnXJogP10 = MakeJogBtn("+10", 126, jogY, 40, 28, Axis.X, 10);
        btnXHome = MakeHomeBtn(175, jogY, "回零", Axis.X);

        grpX.Controls.AddRange(new Control[] { lblXPos, trkXSpeed, lblXSpeedVal,
            btnXJogN10, btnXJogN1, btnXJogP1, btnXJogP10, btnXHome });

        // === Y轴 ===
        grpY.Text = "Y 轴控制";
        grpY.ForeColor = Color.FromArgb(100, 200, 255);
        grpY.BackColor = Color.FromArgb(50, 50, 55);
        grpY.Dock = DockStyle.Top;
        grpY.Height = 120;
        grpY.Padding = new Padding(8);
        grpY.Margin = new Padding(0, 0, 0, 4);

        lblYPos.Text = "位置: 0.0 mm";
        lblYPos.ForeColor = Color.LimeGreen;
        lblYPos.Font = new Font("Consolas", 12, FontStyle.Bold);
        lblYPos.Location = new Point(10, 22);
        lblYPos.Size = new Size(220, 24);

        trkYSpeed.Minimum = 1;
        trkYSpeed.Maximum = 100;
        trkYSpeed.Value = 50;
        trkYSpeed.TickFrequency = 10;
        trkYSpeed.Location = new Point(10, 50);
        trkYSpeed.Size = new Size(180, 30);
        trkYSpeed.BackColor = Color.FromArgb(50, 50, 55);
        trkYSpeed.ValueChanged += TrkSpeed_ValueChanged;

        lblYSpeedVal.Text = "速度: 50%";
        lblYSpeedVal.ForeColor = Color.FromArgb(200, 200, 210);
        lblYSpeedVal.Font = new Font("Segoe UI", 8);
        lblYSpeedVal.Location = new Point(195, 54);
        lblYSpeedVal.Size = new Size(80, 16);
        lblYSpeedVal.Tag = "Y";

        btnYJogN10 = MakeJogBtn("-10", 10, jogY, 40, 28, Axis.Y, -10);
        btnYJogN1 = MakeJogBtn("-1", 54, jogY, 32, 28, Axis.Y, -1);
        btnYJogP1 = MakeJogBtn("+1", 90, jogY, 32, 28, Axis.Y, 1);
        btnYJogP10 = MakeJogBtn("+10", 126, jogY, 40, 28, Axis.Y, 10);
        btnYHome = MakeHomeBtn(175, jogY, "回零", Axis.Y);

        grpY.Controls.AddRange(new Control[] { lblYPos, trkYSpeed, lblYSpeedVal,
            btnYJogN10, btnYJogN1, btnYJogP1, btnYJogP10, btnYHome });

        // === Z轴 ===
        grpZ.Text = "Z 轴控制";
        grpZ.ForeColor = Color.FromArgb(120, 220, 120);
        grpZ.BackColor = Color.FromArgb(50, 50, 55);
        grpZ.Dock = DockStyle.Top;
        grpZ.Height = 120;
        grpZ.Padding = new Padding(8);
        grpZ.Margin = new Padding(0, 0, 0, 4);

        lblZPos.Text = "位置: 0.0 mm";
        lblZPos.ForeColor = Color.LimeGreen;
        lblZPos.Font = new Font("Consolas", 12, FontStyle.Bold);
        lblZPos.Location = new Point(10, 22);
        lblZPos.Size = new Size(220, 24);

        trkZSpeed.Minimum = 1;
        trkZSpeed.Maximum = 100;
        trkZSpeed.Value = 50;
        trkZSpeed.TickFrequency = 10;
        trkZSpeed.Location = new Point(10, 50);
        trkZSpeed.Size = new Size(180, 30);
        trkZSpeed.BackColor = Color.FromArgb(50, 50, 55);
        trkZSpeed.ValueChanged += TrkSpeed_ValueChanged;

        lblZSpeedVal.Text = "速度: 50%";
        lblZSpeedVal.ForeColor = Color.FromArgb(200, 200, 210);
        lblZSpeedVal.Font = new Font("Segoe UI", 8);
        lblZSpeedVal.Location = new Point(195, 54);
        lblZSpeedVal.Size = new Size(80, 16);
        lblZSpeedVal.Tag = "Z";

        btnZJogN10 = MakeJogBtn("-10", 10, jogY, 40, 28, Axis.Z, -10);
        btnZJogN1 = MakeJogBtn("-1", 54, jogY, 32, 28, Axis.Z, -1);
        btnZJogP1 = MakeJogBtn("+1", 90, jogY, 32, 28, Axis.Z, 1);
        btnZJogP10 = MakeJogBtn("+10", 126, jogY, 40, 28, Axis.Z, 10);
        btnZHome = MakeHomeBtn(175, jogY, "回零", Axis.Z);

        grpZ.Controls.AddRange(new Control[] { lblZPos, trkZSpeed, lblZSpeedVal,
            btnZJogN10, btnZJogN1, btnZJogP1, btnZJogP10, btnZHome });

        // === 全局控制 ===
        grpGlobal.Text = "全局控制";
        grpGlobal.ForeColor = Color.FromArgb(240, 200, 80);
        grpGlobal.BackColor = Color.FromArgb(50, 50, 55);
        grpGlobal.Dock = DockStyle.Top;
        grpGlobal.Height = 160;
        grpGlobal.Padding = new Padding(8);
        grpGlobal.Margin = new Padding(0, 0, 0, 4);

        btnHomeAll = new Button();
        btnHomeAll.Text = "全部回零";
        btnHomeAll.Location = new Point(10, 22);
        btnHomeAll.Size = new Size(100, 34);
        btnHomeAll.FlatStyle = FlatStyle.Flat;
        btnHomeAll.FlatAppearance.BorderColor = Color.FromArgb(100, 140, 200);
        btnHomeAll.BackColor = Color.FromArgb(60, 70, 90);
        btnHomeAll.ForeColor = Color.White;
        btnHomeAll.Font = new Font("Microsoft YaHei UI", 9);
        btnHomeAll.Click += BtnHomeAll_Click;

        btnStop = new Button();
        btnStop.Text = "急停";
        btnStop.Location = new Point(120, 22);
        btnStop.Size = new Size(100, 34);
        btnStop.FlatStyle = FlatStyle.Flat;
        btnStop.FlatAppearance.BorderColor = Color.Red;
        btnStop.BackColor = Color.FromArgb(90, 30, 30);
        btnStop.ForeColor = Color.Red;
        btnStop.Font = new Font("Microsoft YaHei UI", 10, FontStyle.Bold);
        btnStop.Click += BtnStop_Click;

        // 绝对定位
        lblAbsX = new Label();
        lblAbsX.Text = "X:";
        lblAbsX.ForeColor = Color.FromArgb(200, 200, 210);
        lblAbsX.Location = new Point(10, 70);
        lblAbsX.Size = new Size(20, 24);
        lblAbsY = new Label();
        lblAbsY.Text = "Y:";
        lblAbsY.ForeColor = Color.FromArgb(200, 200, 210);
        lblAbsY.Location = new Point(100, 70);
        lblAbsY.Size = new Size(20, 24);
        lblAbsZ = new Label();
        lblAbsZ.Text = "Z:";
        lblAbsZ.ForeColor = Color.FromArgb(200, 200, 210);
        lblAbsZ.Location = new Point(190, 70);
        lblAbsZ.Size = new Size(20, 24);

        nudAbsX = new NumericUpDown();
        nudAbsX.Location = new Point(30, 70);
        nudAbsX.Size = new Size(62, 24);
        nudAbsX.Minimum = 0;
        nudAbsX.Maximum = 500;
        nudAbsX.Value = 0;
        nudAbsX.BackColor = Color.FromArgb(40, 40, 45);
        nudAbsX.ForeColor = Color.White;
        nudAbsX.BorderStyle = BorderStyle.FixedSingle;

        nudAbsY = new NumericUpDown();
        nudAbsY.Location = new Point(120, 70);
        nudAbsY.Size = new Size(62, 24);
        nudAbsY.Minimum = 0;
        nudAbsY.Maximum = 400;
        nudAbsY.Value = 0;
        nudAbsY.BackColor = Color.FromArgb(40, 40, 45);
        nudAbsY.ForeColor = Color.White;
        nudAbsY.BorderStyle = BorderStyle.FixedSingle;

        nudAbsZ = new NumericUpDown();
        nudAbsZ.Location = new Point(210, 70);
        nudAbsZ.Size = new Size(62, 24);
        nudAbsZ.Minimum = 0;
        nudAbsZ.Maximum = 300;
        nudAbsZ.Value = 0;
        nudAbsZ.BackColor = Color.FromArgb(40, 40, 45);
        nudAbsZ.ForeColor = Color.White;
        nudAbsZ.BorderStyle = BorderStyle.FixedSingle;

        btnGo = new Button();
        btnGo.Text = "GO";
        btnGo.Location = new Point(10, 105);
        btnGo.Size = new Size(260, 34);
        btnGo.FlatStyle = FlatStyle.Flat;
        btnGo.FlatAppearance.BorderColor = Color.FromArgb(80, 200, 120);
        btnGo.BackColor = Color.FromArgb(30, 70, 50);
        btnGo.ForeColor = Color.FromArgb(80, 220, 120);
        btnGo.Font = new Font("Microsoft YaHei UI", 10, FontStyle.Bold);
        btnGo.Click += BtnGo_Click;

        grpGlobal.Controls.AddRange(new Control[] {
            btnHomeAll, btnStop,
            lblAbsX, lblAbsY, lblAbsZ,
            nudAbsX, nudAbsY, nudAbsZ, btnGo
        });

        pnlControls.Controls.Add(grpGlobal);
        pnlControls.Controls.Add(grpZ);
        pnlControls.Controls.Add(grpY);
        pnlControls.Controls.Add(grpX);

        // === 底部日志 ===
        var pnlLog = new Panel();
        pnlLog.Dock = DockStyle.Fill;
        pnlLog.BackColor = Color.FromArgb(35, 35, 42);
        pnlLog.Padding = new Padding(4);

        lstLog.Dock = DockStyle.Fill;
        lstLog.BackColor = Color.FromArgb(25, 25, 32);
        lstLog.ForeColor = Color.FromArgb(180, 200, 180);
        lstLog.Font = new Font("Consolas", 9);
        lstLog.BorderStyle = BorderStyle.None;
        lstLog.HorizontalScrollbar = true;

        lblStatus.Dock = DockStyle.Bottom;
        lblStatus.Height = 26;
        lblStatus.BackColor = Color.FromArgb(30, 30, 38);
        lblStatus.ForeColor = Color.FromArgb(150, 200, 150);
        lblStatus.Font = new Font("Consolas", 9);
        lblStatus.Padding = new Padding(8, 4, 8, 4);
        lblStatus.Text = "就绪";

        pnlLog.Controls.Add(lstLog);
        pnlLog.Controls.Add(lblStatus);

        // 布局
        tlpMain.Controls.Add(pnlView, 0, 0);
        tlpMain.Controls.Add(pnlControls, 1, 0);
        tlpMain.Controls.Add(pnlLog, 0, 1);
        tlpMain.SetColumnSpan(pnlLog, 2);
        Controls.Add(tlpMain);

        ResumeLayout(false);
    }

    private Button MakeJogBtn(string text, int x, int y, int w, int h, Axis axis, double delta)
    {
        var btn = new Button();
        btn.Text = text;
        btn.Location = new Point(x, y);
        btn.Size = new Size(w, h);
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderColor = Color.FromArgb(80, 90, 110);
        btn.BackColor = Color.FromArgb(55, 58, 65);
        btn.ForeColor = Color.White;
        btn.Font = new Font("Consolas", 9, FontStyle.Bold);
        btn.Tag = (axis, delta);
        btn.Click += BtnJog_Click;
        return btn;
    }

    private Button MakeHomeBtn(int x, int y, string text, Axis axis)
    {
        var btn = new Button();
        btn.Text = text;
        btn.Location = new Point(x, y);
        btn.Size = new Size(45, 28);
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderColor = Color.FromArgb(80, 140, 100);
        btn.BackColor = Color.FromArgb(40, 70, 50);
        btn.ForeColor = Color.FromArgb(100, 220, 130);
        btn.Font = new Font("Microsoft YaHei UI", 8);
        btn.Tag = axis;
        btn.Click += BtnHome_Click;
        return btn;
    }
}
