using PlcMotionControl.Models;

namespace PlcMotionControl.Communication;

/// <summary>
/// 标签管理器，管理 OPC UA 和 Modbus TCP 两种通信前端
/// 根据配置自动选择通信协议，提供统一标签访问接口
/// </summary>
public class TagManager : IDisposable
{
    private readonly OpcUaTagClient? _opcUaClient;
    private readonly ModbusTagClient? _modbusClient;
    private readonly bool _simulationMode;
    private readonly Random _random = new();
    private ITagClient? _activeClient;

    /// <summary>当前使用的通信协议名称</summary>
    public string ActiveProtocol => _simulationMode
        ? "模拟模式"
        : _activeClient?.ProtocolName ?? "未连接";

    /// <summary>是否已连接</summary>
    public bool IsConnected => _simulationMode || (_activeClient?.IsConnected ?? false);

    /// <summary>连接状态变化事件</summary>
    public event EventHandler<bool>? ConnectionStateChanged;

    /// <summary>
    /// 构造函数
    /// </summary>
    public TagManager(AppConfig config)
    {
        _simulationMode = config.SimulationMode;

        if (!_simulationMode)
        {
            // 根据配置创建对应客户端
            if (config.PreferredProtocol == "OPCUA" || config.PreferredProtocol == "Auto")
            {
                _opcUaClient = new OpcUaTagClient(config.OpcUaServerUrl, config.OpcUaTimeoutSeconds);
                _opcUaClient.ConnectionStateChanged += (_, state) =>
                    ConnectionStateChanged?.Invoke(this, state);
            }

            if (config.PreferredProtocol == "Modbus" || config.PreferredProtocol == "Auto")
            {
                _modbusClient = new ModbusTagClient(
                    config.ModbusIpAddress, config.ModbusPort, config.ModbusSlaveId);
                _modbusClient.ConnectionStateChanged += (_, state) =>
                    ConnectionStateChanged?.Invoke(this, state);
            }

            // 默认先尝试 OPC UA
            _activeClient = _opcUaClient ?? _modbusClient as ITagClient;
        }
    }

    /// <summary>
    /// 连接 PLC，按偏好顺序尝试各通信协议
    /// </summary>
    public async Task<bool> ConnectAsync()
    {
        if (_simulationMode)
        {
            ConnectionStateChanged?.Invoke(this, true);
            return true;
        }

        // 尝试 OPC UA
        if (_opcUaClient != null)
        {
            _activeClient = _opcUaClient;
            if (await _opcUaClient.ConnectAsync())
                return true;
        }

        // 尝试 Modbus TCP
        if (_modbusClient != null)
        {
            _activeClient = _modbusClient;
            if (await _modbusClient.ConnectAsync())
                return true;
        }

        _activeClient = null;
        return false;
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    public async Task DisconnectAsync()
    {
        if (_opcUaClient != null)
            await _opcUaClient.DisconnectAsync();
        if (_modbusClient != null)
            await _modbusClient.DisconnectAsync();
        _activeClient = null;
    }

    /// <summary>
    /// 读取布尔值
    /// </summary>
    public async Task<bool> ReadBoolAsync(string tagName)
    {
        if (_simulationMode)
            return _random.Next(2) == 1;

        if (_activeClient == null)
            throw new InvalidOperationException("未连接到 PLC");

        return await _activeClient.ReadBoolAsync(tagName);
    }

    /// <summary>
    /// 写入布尔值
    /// </summary>
    public async Task WriteBoolAsync(string tagName, bool value)
    {
        if (_simulationMode)
            return;

        if (_activeClient == null)
            throw new InvalidOperationException("未连接到 PLC");

        await _activeClient.WriteBoolAsync(tagName, value);
    }

    /// <summary>
    /// 读取浮点数
    /// </summary>
    public async Task<float> ReadFloatAsync(string tagName)
    {
        if (_simulationMode)
            return (float)(_random.NextDouble() * 1000);

        if (_activeClient == null)
            throw new InvalidOperationException("未连接到 PLC");

        return await _activeClient.ReadFloatAsync(tagName);
    }

    /// <summary>
    /// 写入浮点数
    /// </summary>
    public async Task WriteFloatAsync(string tagName, float value)
    {
        if (_simulationMode)
            return;

        if (_activeClient == null)
            throw new InvalidOperationException("未连接到 PLC");

        await _activeClient.WriteFloatAsync(tagName, value);
    }

    /// <summary>
    /// 读取整数
    /// </summary>
    public async Task<int> ReadIntAsync(string tagName)
    {
        if (_simulationMode)
            return _random.Next(0, 10000);

        if (_activeClient == null)
            throw new InvalidOperationException("未连接到 PLC");

        return await _activeClient.ReadIntAsync(tagName);
    }

    /// <summary>
    /// 写入整数
    /// </summary>
    public async Task WriteIntAsync(string tagName, int value)
    {
        if (_simulationMode)
            return;

        if (_activeClient == null)
            throw new InvalidOperationException("未连接到 PLC");

        await _activeClient.WriteIntAsync(tagName, value);
    }

    /// <summary>
    /// 读取双精度浮点数
    /// </summary>
    public async Task<double> ReadDoubleAsync(string tagName)
    {
        if (_simulationMode)
            return _random.NextDouble() * 1000;

        if (_activeClient == null)
            throw new InvalidOperationException("未连接到 PLC");

        return await _activeClient.ReadDoubleAsync(tagName);
    }

    /// <summary>
    /// 写入双精度浮点数
    /// </summary>
    public async Task WriteDoubleAsync(string tagName, double value)
    {
        if (_simulationMode)
            return;

        if (_activeClient == null)
            throw new InvalidOperationException("未连接到 PLC");

        await _activeClient.WriteDoubleAsync(tagName, value);
    }

    /// <summary>释放资源</summary>
    public void Dispose()
    {
        _opcUaClient?.Dispose();
        _modbusClient?.Dispose();
    }
}
