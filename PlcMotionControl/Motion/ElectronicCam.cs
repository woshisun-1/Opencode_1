using PlcMotionControl.Models;

namespace PlcMotionControl.Motion;

/// <summary>
/// 电子凸轮功能
/// 定义主轴与从轴的位移对应关系（凸轮曲线表），
/// 通过线性插值实现任意位置的跟随
/// </summary>
public class ElectronicCam
{
    /// <summary>凸轮曲线点集</summary>
    public List<CamPoint> CamCurve { get; private set; } = new();

    /// <summary>主轴名称</summary>
    public string MasterAxisName { get; set; } = "主轴A";

    /// <summary>从轴名称</summary>
    public string SlaveAxisName { get; set; } = "从轴C";

    /// <summary>是否启用电子凸轮</summary>
    public bool IsEnabled { get; set; }

    /// <summary>凸轮周期（主轴旋转一圈对应行程）</summary>
    public double CamCycle { get; set; } = 360.0;

    /// <summary>状态变化事件</summary>
    public event EventHandler<string>? StatusChanged;

    /// <summary>
    /// 启用电子凸轮
    /// </summary>
    public void Enable()
    {
        if (CamCurve.Count < 2)
        {
            StatusChanged?.Invoke(this, "[电子凸轮] 错误：凸轮曲线点数不足（至少需要 2 个点）");
            return;
        }
        IsEnabled = true;
        StatusChanged?.Invoke(this, $"[电子凸轮] 已启用，曲线点数: {CamCurve.Count}");
    }

    /// <summary>
    /// 停用电子凸轮
    /// </summary>
    public void Disable()
    {
        IsEnabled = false;
        StatusChanged?.Invoke(this, "[电子凸轮] 已停用");
    }

    /// <summary>
    /// 根据主轴位置和凸轮曲线计算从轴位置
    /// </summary>
    /// <param name="masterPosition">主轴当前位置</param>
    /// <returns>从轴目标位置（通过插值计算）</returns>
    public double CalculateSlavePosition(double masterPosition)
    {
        if (CamCurve.Count < 2 || !IsEnabled)
            return 0;

        // 将主轴位置映射到凸轮周期内（模运算）
        double mappedMaster = masterPosition % CamCycle;
        if (mappedMaster < 0)
            mappedMaster += CamCycle;

        // 在曲线点中搜索区间，进行线性插值
        for (int i = 0; i < CamCurve.Count - 1; i++)
        {
            var p1 = CamCurve[i];
            var p2 = CamCurve[i + 1];

            if (mappedMaster >= p1.MasterPosition && mappedMaster <= p2.MasterPosition)
            {
                // 线性插值
                double ratio = (mappedMaster - p1.MasterPosition)
                             / (p2.MasterPosition - p1.MasterPosition);

                return p1.SlavePosition + ratio * (p2.SlavePosition - p1.SlavePosition);
            }
        }

        // 如果不在范围内，使用第一个或最后一个点
        return mappedMaster < CamCurve[0].MasterPosition
            ? CamCurve[0].SlavePosition
            : CamCurve[^1].SlavePosition;
    }

    /// <summary>
    /// 添加凸轮曲线点
    /// </summary>
    public void AddCamPoint(double master, double slave)
    {
        CamCurve.Add(new CamPoint(master, slave));
        // 按主轴位置排序
        CamCurve = CamCurve.OrderBy(p => p.MasterPosition).ToList();
        StatusChanged?.Invoke(this,
            $"[电子凸轮] 已添加点 (M:{master:F1}, S:{slave:F1})，当前点数: {CamCurve.Count}");
    }

    /// <summary>
    /// 批量设置凸轮曲线
    /// </summary>
    public void SetCamCurve(List<CamPoint> points)
    {
        CamCurve = points.OrderBy(p => p.MasterPosition).ToList();
        StatusChanged?.Invoke(this,
            $"[电子凸轮] 已更新曲线，点数: {CamCurve.Count}");
    }

    /// <summary>
    /// 清空凸轮曲线
    /// </summary>
    public void ClearCurve()
    {
        CamCurve.Clear();
        StatusChanged?.Invoke(this, "[电子凸轮] 曲线已清空");
    }

    /// <summary>
    /// 获取正弦型凸轮曲线（常用飞剪曲线）
    /// </summary>
    /// <param name="pointCount">曲线点数</param>
    /// <param name="masterCycle">主轴周期</param>
    /// <param name="slaveStroke">从轴行程</param>
    public void GenerateSineCurve(int pointCount, double masterCycle, double slaveStroke)
    {
        CamCurve.Clear();
        CamCycle = masterCycle;

        for (int i = 0; i < pointCount; i++)
        {
            double master = (double)i / (pointCount - 1) * masterCycle;
            // 正弦加速度曲线：平滑启停
            double normalized = (double)i / (pointCount - 1);
            double slave = slaveStroke * (normalized - Math.Sin(2 * Math.PI * normalized) / (2 * Math.PI));
            CamCurve.Add(new CamPoint(master, slave));
        }

        StatusChanged?.Invoke(this,
            $"[电子凸轮] 已生成正弦曲线，点数: {pointCount}，行程: {slaveStroke}");
    }
}
