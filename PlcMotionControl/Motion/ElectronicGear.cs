using PlcMotionControl.Models;

namespace PlcMotionControl.Motion;

/// <summary>
/// 电子齿轮功能
/// 从轴以固定比例跟随主轴运动，实现电子齿轮比
/// </summary>
public class ElectronicGear
{
    // ===== 齿轮参数 =====

    /// <summary>主从轴齿轮比分子（从轴行程 / 主轴行程）</summary>
    public double GearRatioNumerator { get; set; } = 1.0;

    /// <summary>主从轴齿轮比分母</summary>
    public double GearRatioDenominator { get; set; } = 1.0;

    /// <summary>主轴名称</summary>
    public string MasterAxisName { get; set; } = "主轴A";

    /// <summary>从轴名称</summary>
    public string SlaveAxisName { get; set; } = "从轴B";

    /// <summary>齿轮比分子/分母的只读属性</summary>
    public double GearRatio => GearRatioNumerator / GearRatioDenominator;

    /// <summary>是否启用电子齿轮</summary>
    public bool IsEnabled { get; set; }

    /// <summary>主轴当前位置</summary>
    public double MasterPosition { get; private set; }

    /// <summary>从轴当前位置</summary>
    public double SlavePosition { get; private set; }

    // ===== 内部状态 =====

    private double _lastMasterPosition;
    private bool _firstCycle = true;

    /// <summary>状态变化事件</summary>
    public event EventHandler<string>? StatusChanged;

    /// <summary>
    /// 启用电子齿轮
    /// </summary>
    public void Enable()
    {
        IsEnabled = true;
        _firstCycle = true;
        StatusChanged?.Invoke(this, $"[电子齿轮] 已启用，齿轮比 {GearRatioNumerator}:{GearRatioDenominator}");
    }

    /// <summary>
    /// 停用电子齿轮
    /// </summary>
    public void Disable()
    {
        IsEnabled = false;
        StatusChanged?.Invoke(this, "[电子齿轮] 已停用");
    }

    /// <summary>
    /// 设置齿轮比
    /// </summary>
    /// <param name="numerator">分子</param>
    /// <param name="denominator">分母</param>
    public void SetRatio(double numerator, double denominator)
    {
        if (denominator == 0)
            throw new DivideByZeroException("齿轮比分母不能为 0");

        GearRatioNumerator = numerator;
        GearRatioDenominator = denominator;

        StatusChanged?.Invoke(this,
            $"[电子齿轮] 齿轮比设置为 {numerator}:{denominator} = {GearRatio:F4}");
    }

    /// <summary>
    /// 更新主轴位置，计算从轴跟随位置
    /// 每次调用时计算从轴应到达的位置
    /// </summary>
    /// <param name="masterPosition">主轴当前位置</param>
    /// <returns>从轴目标位置</returns>
    public double UpdateMasterPosition(double masterPosition)
    {
        MasterPosition = masterPosition;

        if (!IsEnabled)
            return SlavePosition;

        if (_firstCycle)
        {
            // 首周期同步，建立初始位置关系
            _lastMasterPosition = masterPosition;
            SlavePosition = masterPosition * GearRatio;
            _firstCycle = false;
            return SlavePosition;
        }

        // 计算主轴位移量
        double deltaMaster = masterPosition - _lastMasterPosition;
        _lastMasterPosition = masterPosition;

        // 根据齿轮比计算从轴位移量
        double deltaSlave = deltaMaster * GearRatio;

        // 更新从轴位置
        SlavePosition += deltaSlave;

        return SlavePosition;
    }

    /// <summary>
    /// 重置从轴位置为指定值
    /// </summary>
    public void ResetSlavePosition(double position = 0)
    {
        SlavePosition = position;
        _firstCycle = true;
        StatusChanged?.Invoke(this, $"[电子齿轮] 从轴位置已重置为 {position:F2}");
    }
}
