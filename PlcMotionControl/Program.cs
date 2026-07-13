namespace PlcMotionControl;

/// <summary>
/// 应用程序入口
/// 三轴龙门机器人运动控制系统
/// 支持 OPC UA + Modbus TCP 双协议与 PLC 通信
/// 集成电子齿轮、电子凸轮、追飞剪、CNC G 代码四大功能
/// </summary>
static class Program
{
    /// <summary>
    /// 应用程序主入口点
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
