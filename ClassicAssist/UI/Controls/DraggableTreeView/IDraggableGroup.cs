using System.Collections.ObjectModel;

namespace ClassicAssist.UI.Controls.DraggableTreeView
{
    public interface IDraggableGroup : IDraggable
    {
        ObservableCollection<IDraggable> Children { get; set; }
    }
}