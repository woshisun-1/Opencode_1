using PlcMotionControl.Models;

namespace PlcMotionControl.Motion;

/// <summary>
/// CNC 运动控制器
/// 管理 G 代码执行、坐标系统、各轴插补运动
/// </summary>
public class CncController
{
    // ===== 组件引用 =====
    private readonly CncInterpreter _interpreter = new();
    private readonly List<CncCommand> _program = new();

    // ===== 运行状态 =====
    private int _currentLine;
    private double _currentX;
    private double _currentY;
    private double _currentZ;
    private bool _isRunning;
    private bool _isPaused;
    private bool _absoluteMode = true;

    /// <summary>当前是否正在运行</summary>
    public bool IsRunning => _isRunning;

    /// <summary>当前是否暂停</summary>
    public bool IsPaused => _isPaused;

    /// <summary>当前执行行号</summary>
    public int CurrentLine => _currentLine;

    /// <summary>程序总行数</summary>
    public int TotalLines => _program.Count;

    /// <summary>执行进度百分比 0-100</summary>
    public double Progress => TotalLines > 0 ? (double)_currentLine / TotalLines * 100 : 0;

    /// <summary>进给速率倍率（百分比）</summary>
    public double FeedRateOverride { get; set; } = 100;

    /// <summary>主轴转速</summary>
    public double SpindleSpeed { get; set; }

    /// <summary>各轴位置（用于插补器读取）</summary>
    public double CurrentX => _currentX;
    public double CurrentY => _currentY;
    public double CurrentZ => _currentZ;

    // ===== 事件 =====
    public event EventHandler<string>? StatusChanged;
    public event EventHandler<CncCommand>? CommandExecuted;
    public event Action<double, double, double>? PositionUpdated;

    /// <summary>
    /// 构造函数，绑定解释器事件
    /// </summary>
    public CncController()
    {
        _interpreter.ParseError += (_, msg) =>
            StatusChanged?.Invoke(this, $"[CNC 错误] {msg}");
    }

    /// <summary>
    /// 加载 G 代码程序
    /// </summary>
    /// <param name="gCode">G 代码文本</param>
    public void LoadProgram(string gCode)
    {
        _program.Clear();
        _program.AddRange(_interpreter.ParseProgram(gCode));
        _currentLine = 0;
        StatusChanged?.Invoke(this,
            $"[CNC] 已加载程序，共 {_program.Count} 条指令");
    }

    /// <summary>
    /// 启动执行
    /// </summary>
    public void Start()
    {
        if (_program.Count == 0)
        {
            StatusChanged?.Invoke(this, "[CNC] 错误：未加载程序");
            return;
        }

        _isRunning = true;
        _isPaused = false;
        _currentLine = 0;

        // 重置位置（从原点开始）
        _currentX = 0;
        _currentY = 0;
        _currentZ = 0;

        StatusChanged?.Invoke(this, "[CNC] 程序开始执行");
    }

    /// <summary>
    /// 暂停执行
    /// </summary>
    public void Pause()
    {
        _isPaused = true;
        StatusChanged?.Invoke(this, "[CNC] 程序已暂停");
    }

    /// <summary>
    /// 恢复执行
    /// </summary>
    public void Resume()
    {
        _isPaused = false;
        StatusChanged?.Invoke(this, "[CNC] 程序继续执行");
    }

    /// <summary>
    /// 停止执行
    /// </summary>
    public void Stop()
    {
        _isRunning = false;
        _isPaused = false;
        StatusChanged?.Invoke(this, "[CNC] 程序已停止");
    }

    /// <summary>
    /// 执行单步（由外部定时器周期性调用）
    /// </summary>
    /// <returns>是否还有更多指令</returns>
    public bool ExecuteStep()
    {
        if (!_isRunning || _isPaused)
            return _currentLine < _program.Count;

        if (_currentLine >= _program.Count)
        {
            _isRunning = false;
            StatusChanged?.Invoke(this, "[CNC] 程序执行完成");
            return false;
        }

        var cmd = _program[_currentLine];
        ExecuteCommand(cmd);
        _currentLine++;
        return _currentLine < _program.Count;
    }

    /// <summary>
    /// 执行单条指令（更新插补目标位置）
    /// </summary>
    private void ExecuteCommand(CncCommand cmd)
    {
        switch (cmd.Type)
        {
            case CncCommandType.Rapid:       // G0
            case CncCommandType.Linear:      // G1
                if (cmd.X.HasValue) _currentX = cmd.X.Value;
                if (cmd.Y.HasValue) _currentY = cmd.Y.Value;
                if (cmd.Z.HasValue) _currentZ = cmd.Z.Value;
                break;

            case CncCommandType.ClockwiseArc:        // G2
            case CncCommandType.CounterClockwiseArc: // G3
                // 圆弧终点
                if (cmd.X.HasValue) _currentX = cmd.X.Value;
                if (cmd.Y.HasValue) _currentY = cmd.Y.Value;
                if (cmd.Z.HasValue) _currentZ = cmd.Z.Value;
                break;

            case CncCommandType.Dwell:  // G4
                // 暂停（由外部逻辑处理）
                break;

            case CncCommandType.AbsoluteMode:  // G90
                _absoluteMode = true;
                break;

            case CncCommandType.IncrementalMode:  // G91
                _absoluteMode = false;
                break;

            case CncCommandType.SetFeedRate:  // F
                // 进给速率已在解析时存储
                break;

            case CncCommandType.SetSpindleSpeed:  // S
                SpindleSpeed = cmd.SpindleSpeed ?? 0;
                break;

            case CncCommandType.ProgramEnd:  // M30
                _isRunning = false;
                StatusChanged?.Invoke(this, "[CNC] 程序结束");
                break;
        }

        CommandExecuted?.Invoke(this, cmd);
        PositionUpdated?.Invoke(_currentX, _currentY, _currentZ);
    }

    /// <summary>
    /// 获取当前位置文本（用于 UI 显示）
    /// </summary>
    public string GetPositionText()
    {
        return $"X:{_currentX,8:F3}  Y:{_currentY,8:F3}  Z:{_currentZ,8:F3}";
    }
}
