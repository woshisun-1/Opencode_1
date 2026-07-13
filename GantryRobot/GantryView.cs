namespace GantryRobot;

public class GantryView : Panel
{
    private MotionController? _ctrl;
    private const double ViewMargin = 40;

    public void SetController(MotionController ctrl)
    {
        _ctrl = ctrl;
        ctrl.PositionChanged += () => Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        int w = ClientSize.Width;
        int h = ClientSize.Height;

        g.Clear(Color.FromArgb(30, 30, 36));

        if (_ctrl is null) return;

        double xRange = _ctrl.X.MaxLimit - _ctrl.X.MinLimit;
        double yRange = _ctrl.Y.MaxLimit - _ctrl.Y.MinLimit;
        double zRange = _ctrl.Z.MaxLimit - _ctrl.Z.MinLimit;

        if (xRange <= 0 || yRange <= 0) return;

        int topViewSize = Math.Min(w - (int)ViewMargin * 2 - 80, h - (int)ViewMargin * 2 - 80);
        topViewSize = Math.Max(topViewSize, 200);

        float ox = (float)ViewMargin;
        float oy = (float)ViewMargin;

        float scale = topViewSize / (float)Math.Max(xRange, yRange);

        // --- 俯视图 (XY) ---
        DrawTopView(g, ox, oy, scale, xRange, yRange);

        // --- Z 轴指示器 ---
        float zBarX = ox + topViewSize + 30;
        float zBarH = topViewSize;
        float zBarW = 30;
        DrawZIndicator(g, zBarX, oy, zBarW, zBarH, zRange);

        // --- 标签 ---
        using var labelFont = new Font("Segoe UI", 9, FontStyle.Bold);
        using var labelBrush = new SolidBrush(Color.FromArgb(180, 180, 190));

        g.DrawString("俯视图 X-Y", labelFont, labelBrush, ox + 4, oy - 18);
        g.DrawString("Z 轴", labelFont, labelBrush, zBarX + 2, oy - 18);

        // 当前位置信息
        string posText = $"X: {_ctrl.X.Position:F1}  Y: {_ctrl.Y.Position:F1}  Z: {_ctrl.Z.Position:F1}";
        using var infoFont = new Font("Consolas", 11, FontStyle.Bold);
        using var infoBrush = new SolidBrush(Color.LimeGreen);
        g.DrawString(posText, infoFont, infoBrush, ox, oy + topViewSize + 12);
    }

    private void DrawTopView(Graphics g, float ox, float oy, float scale, double xRange, double yRange)
    {
        float vw = (float)(xRange * scale);
        float vh = (float)(yRange * scale);

        // 工作区域背景
        using var bgBrush = new SolidBrush(Color.FromArgb(20, 25, 30));
        g.FillRectangle(bgBrush, ox, oy, vw, vh);

        // 网格
        using var gridPen = new Pen(Color.FromArgb(50, 55, 65), 1);
        float gridSize = 50f * scale / 50;
        gridSize = Math.Max(gridSize, 10);
        for (float x = ox; x < ox + vw; x += gridSize)
            g.DrawLine(gridPen, x, oy, x, oy + vh);
        for (float y = oy; y < oy + vh; y += gridSize)
            g.DrawLine(gridPen, ox, y, ox + vw, y);

        // 边界框
        using var framePen = new Pen(Color.FromArgb(100, 140, 200), 2);
        g.DrawRectangle(framePen, ox, oy, vw, vh);

        // 坐标轴标签
        using var axisFont = new Font("Segoe UI", 8);
        using var axisBrush = new SolidBrush(Color.FromArgb(150, 150, 160));
        g.DrawString("X →", axisFont, axisBrush, ox + vw / 2 - 10, oy + vh + 4);
        g.DrawString("← Y", axisFont, axisBrush, ox - 24, oy + vh / 2 - 6);

        if (_ctrl is null) return;

        // 计算位置 (相对比例)
        float xPos = ox + (float)(_ctrl.X.Position / (double)_ctrl.X.MaxLimit * vw);
        float yPos = oy + (float)(_ctrl.Y.Position / (double)_ctrl.Y.MaxLimit * vh);

        xPos = Math.Clamp(xPos, ox, ox + vw);
        yPos = Math.Clamp(yPos, oy, oy + vh);

        // 龙门桥架 (Y方向横梁)
        using var bridgePen = new Pen(Color.FromArgb(220, 80, 60), 3);
        g.DrawLine(bridgePen, ox, yPos, ox + vw, yPos);

        // 滑块/滑座 (X方向)
        using var carriagePen = new Pen(Color.FromArgb(60, 160, 220), 3);
        g.DrawLine(carriagePen, xPos, oy, xPos, oy + vh);

        // 工具头 (交叉点)
        float toolR = 8;
        using var toolBrush = new SolidBrush(Color.LimeGreen);
        g.FillEllipse(toolBrush, xPos - toolR, yPos - toolR, toolR * 2, toolR * 2);
        using var toolPen = new Pen(Color.White, 1.5f);
        g.DrawEllipse(toolPen, xPos - toolR, yPos - toolR, toolR * 2, toolR * 2);

        // 十字准线
        float crossLen = 5;
        using var crossPen = new Pen(Color.FromArgb(100, 255, 100), 1);
        g.DrawLine(crossPen, xPos - crossLen, yPos, xPos + crossLen, yPos);
        g.DrawLine(crossPen, xPos, yPos - crossLen, xPos, yPos + crossLen);

        // 极限标识
        using var limitPen = new Pen(Color.Red, 2);
        if (_ctrl.X.Position <= _ctrl.X.MinLimit + 0.5 || _ctrl.X.Position >= _ctrl.X.MaxLimit - 0.5)
            g.DrawLine(limitPen, ox + vw + 2, oy, ox + vw + 8, oy + vh);
        if (_ctrl.Y.Position <= _ctrl.Y.MinLimit + 0.5 || _ctrl.Y.Position >= _ctrl.Y.MaxLimit - 0.5)
            g.DrawLine(limitPen, ox, oy - 8, ox + vw, oy - 2);
    }

    private void DrawZIndicator(Graphics g, float x, float y, float w, float h, double zRange)
    {
        if (zRange <= 0) return;

        // 背景
        using var bgBrush = new SolidBrush(Color.FromArgb(20, 25, 30));
        g.FillRectangle(bgBrush, x, y, w, h);

        // 边框
        using var framePen = new Pen(Color.FromArgb(100, 140, 200), 1.5f);
        g.DrawRectangle(framePen, x, y, w, h);

        if (_ctrl is null) return;

        // Z位置填充
        float fillRatio = (float)(_ctrl.Z.Position / (double)_ctrl.Z.MaxLimit);
        fillRatio = Math.Clamp(fillRatio, 0, 1);
        float fillH = h * fillRatio;
        float fillY = y + h - fillH;

        using var fillBrush = new SolidBrush(Color.FromArgb(60, 140, 220));
        g.FillRectangle(fillBrush, x + 2, fillY, w - 4, fillH);

        // 值文本
        string zText = $"{_ctrl.Z.Position:F0}";
        using var valFont = new Font("Consolas", 9, FontStyle.Bold);
        using var valBrush = new SolidBrush(Color.White);
        var sz = g.MeasureString(zText, valFont);
        g.DrawString(zText, valFont, valBrush, x + w / 2 - sz.Width / 2, y + h - fillH - sz.Height - 4);

        // 最大/最小标签
        using var labelFont = new Font("Segoe UI", 7);
        using var labelBrush = new SolidBrush(Color.FromArgb(130, 130, 140));
        g.DrawString($"{_ctrl.Z.MaxLimit:F0}", labelFont, labelBrush, x + w + 4, y);
        g.DrawString($"{_ctrl.Z.MinLimit:F0}", labelFont, labelBrush, x + w + 4, y + h - 10);
    }
}
