using PlcMotionControl.Models;

namespace PlcMotionControl.Communication;

/// <summary>
/// 标签通信接口，统一 OPC UA 和 Modbus TCP 两种通信方式
/// </summary>
public interface ITagClient
{
    /// <summary>通信是否已连接</summary>
    bool IsConnected { get; }

    /// <summary>连接通信</summary>
    Task<bool> ConnectAsync();

    /// <summary>断开通信</summary>
    Task DisconnectAsync();

    /// <summary>读取布尔值</summary>
    Task<bool> ReadBoolAsync(string tagName);

    /// <summary>写入布尔值</summary>
    Task WriteBoolAsync(string tagName, bool value);

    /// <summary>读取整数（32位）</summary>
    Task<int> ReadIntAsync(string tagName);

    /// <summary>写入整数（32位）</summary>
    Task WriteIntAsync(string tagName, int value);

    /// <summary>读取浮点数</summary>
    Task<float> ReadFloatAsync(string tagName);

    /// <summary>写入浮点数</summary>
    Task WriteFloatAsync(string tagName, float value);

    /// <summary>读取双精度浮点数</summary>
    Task<double> ReadDoubleAsync(string tagName);

    /// <summary>写入双精度浮点数</summary>
    Task WriteDoubleAsync(string tagName, double value);

    /// <summary>连接状态变化事件</summary>
    event EventHandler<bool> ConnectionStateChanged;

    /// <summary>通信名称（用于日志）</summary>
    string ProtocolName { get; }
}
