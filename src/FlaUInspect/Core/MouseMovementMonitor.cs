using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace FlaUInspect.Core
{
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

        private readonly DispatcherTimer _timer;
        private Point _lastPosition;

        internal Point LastPosition => _lastPosition;

        public event EventHandler<Point> MouseMoved;

        public MouseMovementMonitor()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(500);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (GetCursorPos(out POINT point))
            {
                if (Math.Abs(point.X - LastPosition.X) > Tolerance || Math.Abs(point.Y - LastPosition.Y) > Tolerance)
                {
                    _lastPosition = new Point(point.X, point.Y);
                    MouseMoved?.Invoke(this, _lastPosition);
                }
            }
        }
    }
}