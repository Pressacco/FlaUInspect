namespace FlaUInspect.Core
{
    using System;
    using System.Windows;

    internal class CursorPositionEventArgs : EventArgs
    {
        public CursorPositionEventArgs(Point programPosition, Point desktopPosition)
        {
            this.ProgramPosition = programPosition;
            this.DesktopPosition = desktopPosition;
        }

        /// <summary>
        /// Represents the cursor position relative to
        /// the application that is visible beneath the mouse.
        /// </summary>
        /// <remarks>
        /// The origin is beneath the application's title bar.
        /// </remarks>
        internal Point ProgramPosition;

        internal Point DesktopPosition;
    }
}
