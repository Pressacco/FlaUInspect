namespace FlaUInspect.Core
{
    using FlaUI.Core.AutomationElements;

    using System;

    internal class ElementHoveredEventArgs : EventArgs
    {
        public ElementHoveredEventArgs(AutomationElement element, bool hasChanged)
        {
            Element = element;
            HasChanged = hasChanged;
        }

        internal AutomationElement Element { get; }

        internal bool HasChanged { get; }
    }
}
