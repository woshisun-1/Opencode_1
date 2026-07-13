namespace PlcMotionControl.Models;

/// <summary>
/// 运动状态枚举
/// </summary>
public enum MotionStatus
{
    Idle,       // 空闲
    Running,    // 运行中
    Jogging,    // 点动
    Homing,     // 回零
    Error,      // 错误
    Stopped     // 停止
}

/// <summary>
/// 轴实时状态数据
/// </summary>
public class AxisState
{
    /// <summary>轴名称</summary>
    public string AxisName { get; set; } = string.Empty;

    /// <summary>当前位置（mm）</summary>
    public double ActualPosition { get; set; }

    /// <summary>当前速度（mm/s）</summary>
    public double ActualVelocity { get; set; }

    /// <summary>目标位置（mm）</summary>
    public double TargetPosition { get; set; }

    /// <summary>运动状态</summary>
    public MotionStatus Status { get; set; } = MotionStatus.Idle;

    /// <summary>是否使能</summary>
    public bool Enabled { get; set; }

    /// <summary>是否报警</summary>
    public bool Alarm { get; set; }

    /// <summary>是否回零完成</summary>
    public bool Homed { get; set; }

    /// <summary>是否到达正限位</summary>
    public bool PositiveLimit { get; set; }

    /// <summary>是否到达负限位</summary>
    public bool NegativeLimit { get; set; }
}

/// <summary>
/// 系统运行状态
/// </summary>
public class SystemState
{
    /// <summary>系统是否就绪</summary>
    public bool IsReady { get; set; }

    /// <summary>系统是否运行中</summary>
    public bool IsRunning { get; set; }

    /// <summary>当前模式 0=手动 1=自动 2=回零</summary>
    public int WorkMode { get; set; }

    /// <summary>当前指令号</summary>
    public int CurrentCommandIndex { get; set; }

    /// <summary>累计运行时间（秒）</summary>
    public double RunTimeSeconds { get; set; }

    /// <summary>错误信息</summary>
    public string ErrorMessage { get; set; } = string.Empty;
}
