using PlcMotionControl.Models;

namespace PlcMotionControl.Motion;

/// <summary>
/// 追飞剪功能
/// 在物料连续运动过程中，控制剪刀同步跟随并完成剪切
/// 包含：同步区、剪切点、返回区三段式凸轮曲线
/// </summary>
public class FlyingShear
{
    // ===== 飞剪参数 =====

    /// <summary>剪切长度（mm）</summary>
    public double CutLength { get; set; } = 500;

    /// <summary>物料速度（mm/s）</summary>
    public double MaterialSpeed { get; set; } = 100;

    /// <summary>剪切同步区长度（mm）</summary>
    public double SyncZoneLength { get; set; } = 100;

    /// <summary>剪刀行程（mm）</summary>
    public double ShearStroke { get; set; } = 200;

    /// <summary>剪切过程中剪刀下降高度（mm）</summary>
    public double CutDepth { get; set; } = 50;

    /// <summary>是否启用飞剪</summary>
    public bool IsEnabled { get; set; }

    /// <summary>飞剪状态</summary>
    public FlyingShearState CurrentState { get; private set; } = FlyingShearState.Waiting;

    /// <summary>已完成剪切次数</summary>
    public int CutCount { get; private set; }

    /// <summary>状态变化事件</summary>
    public event EventHandler<string>? StatusChanged;

    // ===== 内部计算 =====
    private double _masterPosition;     // 主轴（物料）当前位置
    private double _lastCutPosition;    // 上次剪切时主轴位置
    private double _camProgress;        // 凸轮执行进度 0~1

    /// <summary>凸轮曲线（主轴位置 -> 剪刀位置映射）</summary>
    public List<CamPoint> CurrentCamCurve { get; private set; } = new();

    /// <summary>
    /// 启用飞剪
    /// </summary>
    public void Enable()
    {
        IsEnabled = true;
        CurrentState = FlyingShearState.Waiting;
        _camProgress = 0;
        CutCount = 0;
        GenerateCamCurve();
        StatusChanged?.Invoke(this, $"[追飞剪] 已启用，剪切长度: {CutLength}mm");
    }

    /// <summary>
    /// 停用飞剪
    /// </summary>
    public void Disable()
    {
        IsEnabled = false;
        CurrentState = FlyingShearState.Stopped;
        StatusChanged?.Invoke(this, "[追飞剪] 已停用");
    }

    /// <summary>
    /// 更新主轴（物料）位置，计算剪刀目标位置
    /// </summary>
    /// <param name="masterPosition">物料当前位置</param>
    /// <returns>剪刀目标位置（Y轴）</returns>
    public double UpdateMasterPosition(double masterPosition)
    {
        _masterPosition = masterPosition;

        if (!IsEnabled)
            return 0;

        // 计算自上次剪切以来物料的移动距离
        double travelSinceCut = masterPosition - _lastCutPosition;

        if (travelSinceCut < 0)
            travelSinceCut += 1000000;  // 处理主轴回零

        // 当一个剪切周期完成时触发
        if (travelSinceCut >= CutLength && CurrentState == FlyingShearState.Waiting)
        {
            CurrentState = FlyingShearState.Synchronizing;
            _camProgress = 0;
            StatusChanged?.Invoke(this, "[追飞剪] 进入同步区");
        }

        // 执行凸轮曲线
        switch (CurrentState)
        {
            case FlyingShearState.Synchronizing:
            case FlyingShearState.Cutting:
                // 凸轮进度 = 同步区内相对位置
                _camProgress = (travelSinceCut - (CutLength - SyncZoneLength)) / SyncZoneLength;
                _camProgress = Math.Clamp(_camProgress, 0, 1);
                CurrentState = _camProgress >= 1.0
                    ? FlyingShearState.Returning
                    : FlyingShearState.Cutting;
                break;

            case FlyingShearState.Returning:
                // 返回阶段：剪刀回到起始位置
                _camProgress -= 0.02;  // 返回速度因子
                if (_camProgress <= 0)
                {
                    _camProgress = 0;
                    CurrentState = FlyingShearState.Waiting;
                    _lastCutPosition = masterPosition;
                    CutCount++;
                    StatusChanged?.Invoke(this,
                        $"[追飞剪] 剪切完成，第 {CutCount} 次剪切");
                }
                break;
        }

        // 通过凸轮曲线查表获得剪刀位置
        double shearPosition = InterpolateCamCurve(_camProgress);
        return shearPosition;
    }

    /// <summary>
    /// 生成飞剪凸轮曲线
    /// 三段式：同步区（剪切）-> 返回区
    /// </summary>
    public void GenerateCamCurve()
    {
        CurrentCamCurve.Clear();

        int points = 50;
        for (int i = 0; i <= points; i++)
        {
            double t = (double)i / points;  // 0~1 归一化进度
            double masterPos = t * CutLength;
            double slavePos;

            if (t <= 0.3)
            {
                // 同步区前段：加速追赶物料
                double localT = t / 0.3;
                slavePos = ShearStroke * localT * localT;  // 抛物线加速
            }
            else if (t <= 0.5)
            {
                // 剪切区：剪刀与物料同步运动
                double localT = (t - 0.3) / 0.2;
                // 正弦曲线实现平滑剪切
                slavePos = ShearStroke - CutDepth * Math.Sin(localT * Math.PI / 2);
            }
            else if (t <= 0.7)
            {
                // 剪切结束，抬起
                double localT = (t - 0.5) / 0.2;
                slavePos = ShearStroke - CutDepth + CutDepth * Math.Sin(localT * Math.PI / 2);
            }
            else
            {
                // 返回区：快速回到起始位置
                double localT = (t - 0.7) / 0.3;
                slavePos = ShearStroke * (1 - localT * localT);  // 抛物线减速
            }

            CurrentCamCurve.Add(new CamPoint(masterPos, slavePos));
        }

        StatusChanged?.Invoke(this,
            $"[追飞剪] 凸轮曲线已生成，{CurrentCamCurve.Count} 个点");
    }

    /// <summary>
    /// 凸轮曲线插值
    /// </summary>
    private double InterpolateCamCurve(double t)
    {
        if (CurrentCamCurve.Count == 0)
            return 0;

        double targetMaster = t * CutLength;

        for (int i = 0; i < CurrentCamCurve.Count - 1; i++)
        {
            var p1 = CurrentCamCurve[i];
            var p2 = CurrentCamCurve[i + 1];

            if (targetMaster >= p1.MasterPosition && targetMaster <= p2.MasterPosition)
            {
                double ratio = (targetMaster - p1.MasterPosition)
                             / (p2.MasterPosition - p1.MasterPosition);
                return p1.SlavePosition + ratio * (p2.SlavePosition - p1.SlavePosition);
            }
        }

        return CurrentCamCurve[^1].SlavePosition;
    }

    /// <summary>
    /// 手动触发一次剪切
    /// </summary>
    public void TriggerCut()
    {
        if (!IsEnabled) return;
        _lastCutPosition = _masterPosition;
        CurrentState = FlyingShearState.Synchronizing;
        _camProgress = 0;
        StatusChanged?.Invoke(this, "[追飞剪] 手动触发剪切");
    }

    /// <summary>
    /// 重置飞剪计数器
    /// </summary>
    public void ResetCounter()
    {
        CutCount = 0;
        _lastCutPosition = 0;
        CurrentState = FlyingShearState.Waiting;
        StatusChanged?.Invoke(this, "[追飞剪] 计数器已重置");
    }
}

/// <summary>
/// 飞剪运行状态
/// </summary>
public enum FlyingShearState
{
    Waiting,        // 等待剪切
    Synchronizing,  // 同步中
    Cutting,        // 剪切中
    Returning,      // 返回中
    Stopped         // 已停止
}
