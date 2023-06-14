using FlaUInspect.Core;

using System;
using System.Diagnostics.Tracing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

public class MouseMovementMonitor
{
    private double PositionTolerance = 0.1;
    private TimeSpan ElapsedTolerance = TimeSpan.FromSeconds(2);

    private struct POINT
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);
    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);
    [DllImport("user32.dll")]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);
    [DllImport("user32.dll")]
    private static extern IntPtr WindowFromPoint(POINT point);

    private struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    private struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    private readonly DispatcherTimer _timer;
    private Point _lastPosition;
    private DateTime _lastCaptureRequested;

    internal event EventHandler<CursorPositionEventArgs> PositionChanged;

    internal event EventHandler<CursorPositionEventArgs> CaptureRequested;

    public MouseMovementMonitor()
    {
        _lastCaptureRequested = DateTime.Now;

        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(500);
        _timer.Tick += OnTimerElapased;
        _timer.Start();
    }

    private POINT GetAppOrigin()
    {
        POINT appOrigin = new POINT { X = 0, Y = 0 };
        IntPtr monitorHandle = MonitorFromPoint(appOrigin, 2 /* MONITOR_DEFAULTTONEAREST */);
        MONITORINFO monitorInfo = new MONITORINFO { cbSize = Marshal.SizeOf(typeof(MONITORINFO)) };
        if (GetMonitorInfo(monitorHandle, ref monitorInfo))
        {
            appOrigin.X = monitorInfo.rcWork.left;
            appOrigin.Y = monitorInfo.rcWork.top;
        }
        return appOrigin;
    }

    private void OnTimerElapased(object sender, EventArgs e)
    {
        var isNewPosition = false;
        var isCaptureRequested = false;

        if (GetCursorPos(out POINT point))
        {
            if (Math.Abs(point.X - _lastPosition.X) > PositionTolerance || Math.Abs(point.Y - _lastPosition.Y) > PositionTolerance)
            {
                isNewPosition = true;
            }

            if (DateTime.Now.Subtract(_lastCaptureRequested) > ElapsedTolerance &&
                System.Windows.Input.Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Alt) &&
                System.Windows.Input.Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Shift))
            {
                isCaptureRequested = true;
            }

            if (isNewPosition || isCaptureRequested)
            {
                POINT appOrigin = GetAppOrigin();
                var appPosition = new Point(point.X - appOrigin.X, point.Y - appOrigin.Y);
                var desktopPosition = new Point(point.X, point.Y);
                var programPosition = GetProgramPosition(point);

                var eventArgs = new CursorPositionEventArgs(programPosition, desktopPosition);

                if (isNewPosition)
                {
                    _lastPosition = new Point(point.X, point.Y);
                    this.PositionChanged?.Invoke(this, eventArgs);
                }

                if (isCaptureRequested)
                {
                    _lastCaptureRequested = DateTime.Now;
                    this.CaptureRequested?.Invoke(this, eventArgs);
                }
            }
        }
    }

    private Point GetProgramPosition(POINT point)
    {
        IntPtr windowHandle = WindowFromPoint(point);
        if (windowHandle != IntPtr.Zero)
        {
            POINT clientPoint = point;
            if (ScreenToClient(windowHandle, ref clientPoint))
            {
                return new Point(clientPoint.X, clientPoint.Y);
            }
        }
        return new Point(-1, -1); // Failed to get the program position
    }

    [DllImport("user32.dll")]
    private static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);
}
