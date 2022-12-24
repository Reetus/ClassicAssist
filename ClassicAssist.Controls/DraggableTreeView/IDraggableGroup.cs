using System.Collections.ObjectModel;

namespace ClassicAssist.Controls.DraggableTreeView
{
    public interface IDraggableGroup : IDraggable
    {
        ObservableCollection<IDraggable> Children { get; set; }
    }
}