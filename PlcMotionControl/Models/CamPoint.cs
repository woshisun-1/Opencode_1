namespace PlcMotionControl.Models;

/// <summary>
/// 凸轮曲线点，定义主轴位置与从轴位置的对应关系
/// </summary>
public class CamPoint
{
    /// <summary>主轴位置（度或 mm）</summary>
    public double MasterPosition { get; set; }

    /// <summary>从轴位置（mm）</summary>
    public double SlavePosition { get; set; }

    /// <summary>构造函数</summary>
    public CamPoint() { }

    /// <summary>构造函数</summary>
    public CamPoint(double master, double slave)
    {
        MasterPosition = master;
        SlavePosition = slave;
    }
}
