namespace PlcMotionControl.Communication;

/// <summary>
/// OPC UA 客户端 — 连接作为 OPC UA 服务器的 PLC
///
/// 【重要】本实现使用模拟模式演示接口协议。
/// 如需对接真实 OPC UA 服务器：
///   1. 解除 PlcMotionControl.csproj 中 OPC Foundation 包的注释
///   2. 取消下方 #if USE_OPCUA 块的注释
///   3. 注释掉模拟回退代码
///
/// 使用 OPC Foundation UA .NET Standard SDK:
///   Install-Package OPCFoundation.NetStandard.Opc.Ua
///   Install-Package OPCFoundation.NetStandard.Opc.Ua.Client
/// </summary>
public class OpcUaTagClient : ITagClient, IDisposable
{
    private readonly string _serverUrl;
    private readonly int _timeoutMs;
    private bool _disposed;
    private bool _isConnected;

    private readonly Random _random = new();

    /// <summary>通信协议名称</summary>
    public string ProtocolName => "OPC UA";

    /// <summary>是否已连接</summary>
    public bool IsConnected => _isConnected;

    /// <summary>连接状态变化事件</summary>
    public event EventHandler<bool>? ConnectionStateChanged;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="serverUrl">OPC UA 服务器地址</param>
    /// <param name="timeoutSeconds">连接超时秒数</param>
    public OpcUaTagClient(string serverUrl, int timeoutSeconds = 10)
    {
        _serverUrl = serverUrl;
        _timeoutMs = timeoutSeconds * 1000;
    }

    /// <summary>
    /// 连接 OPC UA 服务器
    ///
    /// === 生产环境实现说明（解除注释后替换下方模拟代码）===
    /// 1. 创建 ApplicationConfiguration
    /// 2. 调用 CoreClientUtils.SelectEndpoint() 选择端点
    /// 3. 创建 Session 并 Open()
    /// 4. 参考 OPC Foundation 官方示例：
    ///    https://github.com/OPCFoundation/UA-.NETStandard/tree/master/Samples
    /// </summary>
    public Task<bool> ConnectAsync()
    {
        try
        {
            // ===== 模拟连接 =====
            System.Diagnostics.Debug.WriteLine(
                $"[OPC UA] 模拟连接 {_serverUrl}（需安装 OPC Foundation 包后启用真实连接）");

            _isConnected = true;
            ConnectionStateChanged?.Invoke(this, true);
            return Task.FromResult(true);

            /* ===== 真实 OPC UA 连接 (取消注释) =====
            #if USE_OPCUA
            var config = new Opc.Ua.ApplicationConfiguration
            {
                ApplicationName = "PlcMotionControl",
                ApplicationType = Opc.Ua.ApplicationType.Client,
                SecurityConfiguration = new Opc.Ua.SecurityConfiguration
                {
                    ApplicationCertificate = new Opc.Ua.CertificateIdentifier(),
                    AutoAcceptUntrustedCertificates = true,
                },
                TransportQuotas = new Opc.Ua.TransportQuotas
                {
                    OperationTimeout = _timeoutMs,
                    MaxStringLength = 65535,
                    MaxByteStringLength = 65535,
                    MaxArrayLength = 65535,
                    MaxMessageSize = 4194304,
                    MaxBufferSize = 65535,
                },
                ClientConfiguration = new Opc.Ua.ClientConfiguration
                {
                    DefaultSessionTimeout = _timeoutMs,
                }
            };

            await config.ValidateAsync(Opc.Ua.ApplicationType.Client);
            var endpoint = Opc.Ua.CoreClientUtils.SelectEndpoint(_serverUrl, false, _timeoutMs);
            var endpointConfiguration = Opc.Ua.EndpointConfiguration.Create(config);
            var transportChannel = new Opc.Ua.Client.TransportChannel(
                config, endpoint, endpointConfiguration);

            _session = Opc.Ua.Client.Session.Create(
                config, transportChannel, new Opc.Ua.ReverseConnectManager(),
                null, "PLC Motion Control Client", _timeoutMs, null, null);

            _session.Open();
            #endif
            */
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OPC UA 连接失败: {ex.Message}");
            _isConnected = false;
            ConnectionStateChanged?.Invoke(this, false);
            return Task.FromResult(false);
        }
    }

    /// <summary>断开连接</summary>
    public Task DisconnectAsync()
    {
        _isConnected = false;
        ConnectionStateChanged?.Invoke(this, false);
        return Task.CompletedTask;
    }

    /// <summary>读取布尔值（模拟）</summary>
    public Task<bool> ReadBoolAsync(string tagName)
    {
        return Task.FromResult(_random.Next(2) == 1);
    }

    /// <summary>写入布尔值（模拟）</summary>
    public Task WriteBoolAsync(string tagName, bool value)
    {
        return Task.CompletedTask;
    }

    /// <summary>读取整数（模拟）</summary>
    public Task<int> ReadIntAsync(string tagName)
    {
        return Task.FromResult(_random.Next(0, 10000));
    }

    /// <summary>写入整数（模拟）</summary>
    public Task WriteIntAsync(string tagName, int value)
    {
        return Task.CompletedTask;
    }

    /// <summary>读取浮点数（模拟）</summary>
    public Task<float> ReadFloatAsync(string tagName)
    {
        return Task.FromResult((float)(_random.NextDouble() * 1000));
    }

    /// <summary>写入浮点数（模拟）</summary>
    public Task WriteFloatAsync(string tagName, float value)
    {
        return Task.CompletedTask;
    }

    /// <summary>读取双精度浮点数（模拟）</summary>
    public Task<double> ReadDoubleAsync(string tagName)
    {
        return Task.FromResult(_random.NextDouble() * 1000);
    }

    /// <summary>写入双精度浮点数（模拟）</summary>
    public Task WriteDoubleAsync(string tagName, double value)
    {
        return Task.CompletedTask;
    }

    /// <summary>释放资源</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }
}
