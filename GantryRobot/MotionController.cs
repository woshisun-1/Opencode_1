namespace GantryRobot;

public enum Axis { X, Y, Z }

public class AxisState
{
    public double Position { get; set; }
    public double TargetPosition { get; set; }
    public double Speed { get; set; }
    public double MaxSpeed { get; set; } = 200;
    public double Acceleration { get; set; } = 800;
    public double MinLimit { get; set; } = 0;
    public double MaxLimit { get; set; } = 500;
    public bool Homed { get; set; }
    public bool Moving { get; set; }

    internal double _currentSpeed;

    public AxisState()
    {
        Speed = MaxSpeed * 0.5;
    }
}

public class MotionController
{
    public AxisState X { get; } = new() { MaxLimit = 500 };
    public AxisState Y { get; } = new() { MaxLimit = 400 };
    public AxisState Z { get; } = new() { MaxLimit = 300 };

    public event Action? PositionChanged;
    public event Action<string>? StatusMessage;

    private System.Threading.Timer? _timer;
    private const double CycleTime = 0.010;

    public void Start()
    {
        _timer = new System.Threading.Timer(_ => UpdateMotion(), null, 0, (int)(CycleTime * 1000));
    }

    public void Stop()
    {
        _timer?.Dispose();
        _timer = null;
    }

    private void UpdateMotion()
    {
        UpdateAxis(X);
        UpdateAxis(Y);
        UpdateAxis(Z);
        PositionChanged?.Invoke();
    }

    private void UpdateAxis(AxisState axis)
    {
        double delta = axis.TargetPosition - axis.Position;
        double absDelta = Math.Abs(delta);

        if (absDelta < 0.005)
        {
            axis.Position = axis.TargetPosition;
            axis._currentSpeed = 0;
            axis.Moving = false;
            return;
        }

        axis.Moving = true;

        double speed = axis.Speed > 0 ? axis.Speed : axis.MaxSpeed * 0.5;

        double stopDist = (axis._currentSpeed * axis._currentSpeed) / (2 * axis.Acceleration);
        double decelFactor = absDelta < stopDist ? 0.3 : 1.0;
        double targetV = speed * decelFactor;
        double dv = axis.Acceleration * CycleTime;

        if (axis._currentSpeed < targetV)
            axis._currentSpeed = Math.Min(axis._currentSpeed + dv, targetV);
        else if (axis._currentSpeed > targetV)
            axis._currentSpeed = Math.Max(axis._currentSpeed - dv, targetV);

        double step = Math.Clamp(axis._currentSpeed * CycleTime, 0, absDelta);
        step = Math.Sign(delta) * step;

        double newPos = axis.Position + step;
        newPos = Math.Clamp(newPos, axis.MinLimit, axis.MaxLimit);
        axis.Position = newPos;

        if (Math.Abs(axis.Position - axis.TargetPosition) < 0.01)
        {
            axis.Position = axis.TargetPosition;
            axis._currentSpeed = 0;
            axis.Moving = false;
        }
    }

    public void MoveTo(double x, double y, double z)
    {
        X.TargetPosition = Math.Clamp(x, X.MinLimit, X.MaxLimit);
        Y.TargetPosition = Math.Clamp(y, Y.MinLimit, Y.MaxLimit);
        Z.TargetPosition = Math.Clamp(z, Z.MinLimit, Z.MaxLimit);
        StatusMessage?.Invoke($"绝对定位 → X={X.TargetPosition:F1}  Y={Y.TargetPosition:F1}  Z={Z.TargetPosition:F1}");
    }

    public void Jog(Axis axis, double delta)
    {
        var a = GetAxis(axis);
        double newTarget = a.TargetPosition + delta;
        a.TargetPosition = Math.Clamp(newTarget, a.MinLimit, a.MaxLimit);
        StatusMessage?.Invoke($"点动 {axis} {(delta > 0 ? "+" : "-")}: {a.TargetPosition:F1}");
    }

    public void HomeAxis(Axis axis)
    {
        var a = GetAxis(axis);
        a.TargetPosition = a.MinLimit;
        a.Position = a.MinLimit;
        a._currentSpeed = 0;
        a.Homed = true;
        a.Moving = false;
        StatusMessage?.Invoke($"回零完成: {axis}");
    }

    public void HomeAll()
    {
        HomeAxis(Axis.X);
        HomeAxis(Axis.Y);
        HomeAxis(Axis.Z);
        StatusMessage?.Invoke("全部轴回零完成");
    }

    public void StopAll()
    {
        X.TargetPosition = X.Position;
        Y.TargetPosition = Y.Position;
        Z.TargetPosition = Z.Position;
        X._currentSpeed = 0;
        Y._currentSpeed = 0;
        Z._currentSpeed = 0;
        X.Moving = false;
        Y.Moving = false;
        Z.Moving = false;
        StatusMessage?.Invoke("急停！所有轴已停止");
    }

    public void SetSpeed(Axis axis, double speed)
    {
        var a = GetAxis(axis);
        a.Speed = Math.Clamp(speed, 1, a.MaxSpeed);
    }

    private AxisState GetAxis(Axis axis) => axis switch
    {
        Axis.X => X,
        Axis.Y => Y,
        Axis.Z => Z,
        _ => X
    };
}
