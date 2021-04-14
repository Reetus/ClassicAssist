namespace ClassicAssist.UI.Controls.DraggableTreeView
{
    public interface IDraggableEntry : IDraggable
    {
        string Group { get; set; }
    }
}