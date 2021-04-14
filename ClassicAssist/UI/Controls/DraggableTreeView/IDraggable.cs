using System;

namespace ClassicAssist.UI.Controls.DraggableTreeView
{
    public interface IDraggable : IComparable<IDraggable>
    {
        string Name { get; set; }
    }
}