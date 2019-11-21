using System.Linq;
using Assistant;
using ClassicAssist.ViewModels;
using DynamicData.Binding;
using ReactiveUI;

namespace ClassicAssist.UI.ViewModels
{
    public class GeneralTabViewModel : ViewModelBase
    {
        private int _totalItemCount;
        public string ClientPath { get; set; } = Engine.ClientPath;

        public int TotalItemCount
        {
            get => _totalItemCount;
            set => this.RaiseAndSetIfChanged(ref _totalItemCount, value);
        }

        public GeneralTabViewModel()
        {
            Engine.Items.ContainerContentsChanged += ( a, i ) => TotalItemCount = Engine.Items.GetTotalItemCount();
        }
    }
}