namespace PlcMotionControl.Models;

/// <summary>
/// 轴配置信息，定义单轴的参数
/// </summary>
public class AxisConfig
{
    /// <summary>轴名称，如 X、Y、Z、主轴A、主轴B</summary>
    public string AxisName { get; set; } = string.Empty;

    /// <summary>最小位置（mm）</summary>
    public double MinPosition { get; set; } = 0;

    /// <summary>最大位置（mm）</summary>
    public double MaxPosition { get; set; } = 500;

    /// <summary>最大速度（mm/s）</summary>
    public double MaxVelocity { get; set; } = 300;

    /// <summary>加速度（mm/s²）</summary>
    public double Acceleration { get; set; } = 1000;

    /// <summary>减速度（mm/s²）</summary>
    public double Deceleration { get; set; } = 1000;

    /// <summary>OPC UA 节点 ID，用于读取实际位置</summary>
    public string OpcUaActualPositionNodeId { get; set; } = string.Empty;

    /// <summary>OPC UA 节点 ID，用于写入目标位置</summary>
    public string OpcUaTargetPositionNodeId { get; set; } = string.Empty;

    /// <summary>Modbus 保持寄存器地址，用于读取实际位置</summary>
    public int ModbusActualPositionAddress { get; set; }

    /// <summary>Modbus 保持寄存器地址，用于写入目标位置</summary>
    public int ModbusTargetPositionAddress { get; set; }

    /// <summary>脉冲当量（每个脉冲对应的位移 mm）</summary>
    public double PulseEquivalence { get; set; } = 0.001;
}
