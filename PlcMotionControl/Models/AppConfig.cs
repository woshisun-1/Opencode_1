namespace PlcMotionControl.Models;

/// <summary>
/// 应用程序配置
/// </summary>
public class AppConfig
{
    /// <summary>OPC UA 服务器地址</summary>
    public string OpcUaServerUrl { get; set; } = "opc.tcp://localhost:4840";

    /// <summary>OPC UA 连接超时（秒）</summary>
    public int OpcUaTimeoutSeconds { get; set; } = 10;

    /// <summary>Modbus TCP 服务器 IP</summary>
    public string ModbusIpAddress { get; set; } = "192.168.1.100";

    /// <summary>Modbus TCP 端口</summary>
    public int ModbusPort { get; set; } = 502;

    /// <summary>Modbus 从站 ID</summary>
    public byte ModbusSlaveId { get; set; } = 1;

    /// <summary>首选通信方式（OPCUA / Modbus / Auto）</summary>
    public string PreferredProtocol { get; set; } = "Auto";

    /// <summary>是否启用模拟模式（无 PLC 时调试用）</summary>
    public bool SimulationMode { get; set; } = true;

    /// <summary>UI 刷新间隔（ms）</summary>
    public int UiRefreshIntervalMs { get; set; } = 50;

    /// <summary>各轴配置</summary>
    public List<AxisConfig> Axes { get; set; } = new();
}
