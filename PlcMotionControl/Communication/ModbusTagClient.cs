using EasyModbus;

namespace PlcMotionControl.Communication;

/// <summary>
/// Modbus TCP 客户端，连接作为 Modbus 从站的 PLC
/// 使用 EasyModbusTCP 库实现
/// </summary>
public class ModbusTagClient : ITagClient, IDisposable
{
    private readonly string _ipAddress;
    private readonly int _port;
    private readonly byte _slaveId;
    private ModbusClient? _client;
    private bool _disposed;

    /// <summary>通信协议名称</summary>
    public string ProtocolName => "Modbus TCP";

    /// <summary>是否已连接</summary>
    public bool IsConnected => _client?.Connected ?? false;

    /// <summary>连接状态变化事件</summary>
    public event EventHandler<bool>? ConnectionStateChanged;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="ipAddress">PLC IP 地址</param>
    /// <param name="port">端口号，默认 502</param>
    /// <param name="slaveId">从站 ID</param>
    public ModbusTagClient(string ipAddress, int port = 502, byte slaveId = 1)
    {
        _ipAddress = ipAddress;
        _port = port;
        _slaveId = slaveId;
    }

    /// <summary>
    /// 连接 Modbus TCP 服务器
    /// </summary>
    public Task<bool> ConnectAsync()
    {
        try
        {
            _client = new ModbusClient(_ipAddress, _port);
            _client.ConnectionTimeout = 3000;
            _client.Connect();
            ConnectionStateChanged?.Invoke(this, true);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Modbus TCP 连接失败: {ex.Message}");
            ConnectionStateChanged?.Invoke(this, false);
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// 断开 Modbus TCP 连接
    /// </summary>
    public Task DisconnectAsync()
    {
        try
        {
            _client?.Disconnect();
            ConnectionStateChanged?.Invoke(this, false);
        }
        catch { }
        return Task.CompletedTask;
    }

    /// <summary>
    /// 解析标签名，格式：
    ///   M0   -> 线圈, 地址 0
    ///   I0   -> 离散输入, 地址 0
    ///   AI0  -> 输入寄存器, 地址 0
    ///   AQ0  -> 保持寄存器, 地址 0
    ///   纯数字 -> 保持寄存器
    /// </summary>
    private (string Type, int Address) ParseTagName(string tagName)
    {
        tagName = tagName.Trim().ToUpper();

        if (tagName.StartsWith("M"))
            return ("Coil", int.Parse(tagName[1..]));
        if (tagName.StartsWith("I") && !tagName.StartsWith("AI"))
            return ("DiscreteInput", int.Parse(tagName[1..]));
        if (tagName.StartsWith("AI"))
            return ("InputRegister", int.Parse(tagName[2..]));
        if (tagName.StartsWith("AQ"))
            return ("HoldingRegister", int.Parse(tagName[2..]));
        if (int.TryParse(tagName, out int addr))
            return ("HoldingRegister", addr);

        throw new ArgumentException($"Modbus 标签格式无效: {tagName}");
    }

    /// <summary>
    /// 读取布尔值（支持线圈 M 和离散输入 I）
    /// </summary>
    public Task<bool> ReadBoolAsync(string tagName)
    {
        if (_client is null || !_client.Connected)
            throw new InvalidOperationException("Modbus 未连接");

        var (type, address) = ParseTagName(tagName);

        bool result = type switch
        {
            "Coil" => _client.ReadCoils(address, 1)[0],
            "DiscreteInput" => _client.ReadDiscreteInputs(address, 1)[0],
            _ => throw new ArgumentException($"Modbus 布尔类型不支持地址类型: {type}")
        };

        return Task.FromResult(result);
    }

    /// <summary>
    /// 写入布尔值（线圈 M）
    /// </summary>
    public Task WriteBoolAsync(string tagName, bool value)
    {
        if (_client is null || !_client.Connected)
            throw new InvalidOperationException("Modbus 未连接");

        var (type, address) = ParseTagName(tagName);

        if (type != "Coil")
            throw new ArgumentException("Modbus 只支持写入线圈(M)类型的布尔值");

        _client.WriteSingleCoil(address, value);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 读取整数值（保持寄存器 AQ 默认 32 位，占 2 个寄存器）
    /// </summary>
    public Task<int> ReadIntAsync(string tagName)
    {
        if (_client is null || !_client.Connected)
            throw new InvalidOperationException("Modbus 未连接");

        var (type, address) = ParseTagName(tagName);

        int result = type switch
        {
            "HoldingRegister" => ModbusClient.ConvertRegistersToInt(
                _client.ReadHoldingRegisters(address, 2)),
            "InputRegister" => ModbusClient.ConvertRegistersToInt(
                _client.ReadInputRegisters(address, 2)),
            _ => throw new ArgumentException($"Modbus 整数类型不支持的地址: {type}")
        };

        return Task.FromResult(result);
    }

    /// <summary>
    /// 写入整数值（保持寄存器，32 位占 2 个寄存器）
    /// </summary>
    public Task WriteIntAsync(string tagName, int value)
    {
        if (_client is null || !_client.Connected)
            throw new InvalidOperationException("Modbus 未连接");

        var (type, address) = ParseTagName(tagName);

        if (type != "HoldingRegister")
            throw new ArgumentException("Modbus 只支持写入保持寄存器(AQ)类型的整数");

        var registers = ModbusClient.ConvertIntToRegisters(value);
        _client.WriteMultipleRegisters(address, registers);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 读取浮点数（保持寄存器，32 位占 2 个寄存器）
    /// </summary>
    public Task<float> ReadFloatAsync(string tagName)
    {
        if (_client is null || !_client.Connected)
            throw new InvalidOperationException("Modbus 未连接");

        var (type, address) = ParseTagName(tagName);

        float result = type switch
        {
            "HoldingRegister" => ModbusClient.ConvertRegistersToFloat(
                _client.ReadHoldingRegisters(address, 2)),
            "InputRegister" => ModbusClient.ConvertRegistersToFloat(
                _client.ReadInputRegisters(address, 2)),
            _ => throw new ArgumentException($"Modbus 浮点数类型不支持的地址: {type}")
        };

        return Task.FromResult(result);
    }

    /// <summary>
    /// 写入浮点数（保持寄存器，32 位占 2 个寄存器）
    /// </summary>
    public Task WriteFloatAsync(string tagName, float value)
    {
        if (_client is null || !_client.Connected)
            throw new InvalidOperationException("Modbus 未连接");

        var (type, address) = ParseTagName(tagName);

        if (type != "HoldingRegister")
            throw new ArgumentException("Modbus 只支持写入保持寄存器(AQ)类型的浮点数");

        var registers = ModbusClient.ConvertFloatToRegisters(value);
        _client.WriteMultipleRegisters(address, registers);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 读取双精度浮点数（保持寄存器，64 位占 4 个寄存器）
    /// </summary>
    public Task<double> ReadDoubleAsync(string tagName)
    {
        if (_client is null || !_client.Connected)
            throw new InvalidOperationException("Modbus 未连接");

        var (type, address) = ParseTagName(tagName);

        if (type != "HoldingRegister" && type != "InputRegister")
            throw new ArgumentException($"Modbus 双精度不支持的地址: {type}");

        var registers = type == "HoldingRegister"
            ? _client.ReadHoldingRegisters(address, 4)
            : _client.ReadInputRegisters(address, 4);

        double result = ModbusClient.ConvertRegistersToDouble(registers);
        return Task.FromResult(result);
    }

    /// <summary>
    /// 写入双精度浮点数（保持寄存器，64 位占 4 个寄存器）
    /// </summary>
    public Task WriteDoubleAsync(string tagName, double value)
    {
        if (_client is null || !_client.Connected)
            throw new InvalidOperationException("Modbus 未连接");

        var (type, address) = ParseTagName(tagName);

        if (type != "HoldingRegister")
            throw new ArgumentException("Modbus 只支持写入保持寄存器(AQ)类型的双精度浮点数");

        var registers = ModbusClient.ConvertDoubleToRegisters(value);
        _client.WriteMultipleRegisters(address, registers);
        return Task.CompletedTask;
    }

    /// <summary>释放资源</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _client?.Disconnect();
        _disposed = true;
    }
}
