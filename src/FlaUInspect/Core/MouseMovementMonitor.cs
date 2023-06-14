using FlaUInspect.Core;

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

public class MouseMovementMonitor
{
    private double Tolerance = 0.1;

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

    internal event EventHandler<CursorPositionEventArgs> MouseMoved;

    public MouseMovementMonitor()
    {
        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(500);
        _timer.Tick += Timer_Tick;
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

    private void Timer_Tick(object sender, EventArgs e)
    {
        if (GetCursorPos(out POINT point))
        {
            if (Math.Abs(point.X - _lastPosition.X) > Tolerance || Math.Abs(point.Y - _lastPosition.Y) > Tolerance)
            {
                _lastPosition = new Point(point.X, point.Y);

                POINT appOrigin = GetAppOrigin();
                var appPosition = new Point(point.X - appOrigin.X, point.Y - appOrigin.Y);
                var desktopPosition = new Point(point.X, point.Y);
                var programPosition = GetProgramPosition(point);

                var eventArgs = new CursorPositionEventArgs(programPosition, desktopPosition);
                MouseMoved?.Invoke(this, eventArgs);
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
