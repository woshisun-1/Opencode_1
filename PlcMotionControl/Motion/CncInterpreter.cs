namespace PlcMotionControl.Motion;

/// <summary>
/// CNC 指令类型
/// </summary>
public enum CncCommandType
{
    Rapid,              // G0 快速定位
    Linear,             // G1 直线插补
    ClockwiseArc,       // G2 顺时针圆弧
    CounterClockwiseArc,// G3 逆时针圆弧
    Dwell,              // G4 暂停
    SetCoordinate,      // G54-G59 坐标系设定
    AbsoluteMode,       // G90 绝对坐标
    IncrementalMode,    // G91 相对坐标
    SetFeedRate,        // F 进给速率
    SetSpindleSpeed,    // S 主轴转速
    SpindleOn,          // M3 主轴正转
    SpindleOff,         // M5 主轴停止
    ProgramEnd,         // M30 程序结束
    Custom              // 其他
}

/// <summary>
/// CNC 指令解析结果
/// </summary>
public class CncCommand
{
    /// <summary>指令类型</summary>
    public CncCommandType Type { get; set; }

    /// <summary>原始行文本</summary>
    public string RawText { get; set; } = string.Empty;

    /// <summary>行号</summary>
    public int LineNumber { get; set; }

    /// <summary>目标 X 坐标</summary>
    public double? X { get; set; }

    /// <summary>目标 Y 坐标</summary>
    public double? Y { get; set; }

    /// <summary>目标 Z 坐标</summary>
    public double? Z { get; set; }

    /// <summary>圆弧圆心 I（X 偏移）</summary>
    public double? I { get; set; }

    /// <summary>圆弧圆心 J（Y 偏移）</summary>
    public double? J { get; set; }

    /// <summary>圆弧圆心 K（Z 偏移）</summary>
    public double? K { get; set; }

    /// <summary>圆弧半径 R</summary>
    public double? R { get; set; }

    /// <summary>进给速率</summary>
    public double? FeedRate { get; set; }

    /// <summary>暂停时间（秒）</summary>
    public double? DwellTime { get; set; }

    /// <summary>主轴转速</summary>
    public double? SpindleSpeed { get; set; }

    /// <summary>坐标系编号 (1-6 对应 G54-G59)</summary>
    public int? CoordinateSystem { get; set; }

    /// <summary>是否为绝对坐标模式</summary>
    public bool IsAbsolute { get; set; } = true;
}

/// <summary>
/// CNC G 代码解释器
/// 解析标准 ISO 代码（G 代码），支持常用 G、M、F、S 指令
/// </summary>
public class CncInterpreter
{
    /// <summary>解析错误事件</summary>
    public event EventHandler<string>? ParseError;

    /// <summary>解析成功事件</summary>
    public event EventHandler<CncCommand>? CommandParsed;

    /// <summary>
    /// 解析单行 G 代码
    /// </summary>
    /// <param name="line">G 代码行，如 "G0 X100 Y50 Z10 F500"</param>
    /// <returns>解析后的命令对象</returns>
    public CncCommand ParseLine(string line)
    {
        var cmd = new CncCommand
        {
            RawText = line,
            LineNumber = 0
        };

        try
        {
            // 移除注释
            string code = line;
            if (code.Contains(';'))
                code = code[..code.IndexOf(';')];
            if (code.Contains('('))
                code = code[..code.IndexOf('(')];

            code = code.Trim().ToUpper();

            if (string.IsNullOrEmpty(code))
            {
                cmd.Type = CncCommandType.Custom;
                return cmd;
            }

            // 按空格分割
            var tokens = code.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var token in tokens)
            {
                if (token.Length < 1) continue;

                char letter = token[0];
                string valueStr = token[1..];

                if (!double.TryParse(valueStr,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out double value))
                {
                    continue;
                }

                switch (letter)
                {
                    case 'N':
                        cmd.LineNumber = (int)value;
                        break;

                    case 'G':
                        cmd = ParseGCode(cmd, (int)value);
                        break;

                    case 'M':
                        cmd = ParseMCode(cmd, (int)value);
                        break;

                    case 'X':
                        cmd.X = value;
                        break;
                    case 'Y':
                        cmd.Y = value;
                        break;
                    case 'Z':
                        cmd.Z = value;
                        break;
                    case 'I':
                        cmd.I = value;
                        break;
                    case 'J':
                        cmd.J = value;
                        break;
                    case 'K':
                        cmd.K = value;
                        break;
                    case 'R':
                        cmd.R = value;
                        break;
                    case 'F':
                        cmd.FeedRate = value;
                        cmd.Type = CncCommandType.SetFeedRate;
                        break;
                    case 'S':
                        cmd.SpindleSpeed = value;
                        cmd.Type = CncCommandType.SetSpindleSpeed;
                        break;
                    case 'P':
                        cmd.DwellTime = value;
                        break;
                }
            }

            CommandParsed?.Invoke(this, cmd);
        }
        catch (Exception ex)
        {
            ParseError?.Invoke(this, $"解析错误: {ex.Message} → {line}");
        }

        return cmd;
    }

    /// <summary>
    /// 解析 G 代码指令
    /// </summary>
    private CncCommand ParseGCode(CncCommand cmd, int gCode)
    {
        cmd.Type = gCode switch
        {
            0 => CncCommandType.Rapid,
            1 => CncCommandType.Linear,
            2 => CncCommandType.ClockwiseArc,
            3 => CncCommandType.CounterClockwiseArc,
            4 => CncCommandType.Dwell,
            >= 54 and <= 59 => CncCommandType.SetCoordinate,
            90 => CncCommandType.AbsoluteMode,
            91 => CncCommandType.IncrementalMode,
            _ => cmd.Type
        };

        if (gCode >= 54 && gCode <= 59)
            cmd.CoordinateSystem = gCode - 53;

        if (gCode == 90)
            cmd.IsAbsolute = true;
        else if (gCode == 91)
            cmd.IsAbsolute = false;

        return cmd;
    }

    /// <summary>
    /// 解析 M 代码辅助指令
    /// </summary>
    private CncCommand ParseMCode(CncCommand cmd, int mCode)
    {
        cmd.Type = mCode switch
        {
            3 => CncCommandType.SpindleOn,
            5 => CncCommandType.SpindleOff,
            30 => CncCommandType.ProgramEnd,
            _ => cmd.Type
        };
        return cmd;
    }

    /// <summary>
    /// 解析多行 G 代码程序
    /// </summary>
    /// <param name="program">完整的 G 代码文本</param>
    /// <returns>指令列表</returns>
    public List<CncCommand> ParseProgram(string program)
    {
        var commands = new List<CncCommand>();
        var lines = program.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('%') || trimmed.StartsWith('/'))
                continue;

            var cmd = ParseLine(trimmed);
            commands.Add(cmd);
        }

        return commands;
    }
}
