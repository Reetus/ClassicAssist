using System.Collections.Generic;
using System.Windows.Threading;
using ClassicAssist.Data;
using ClassicAssist.Shared.UI;

namespace ClassicAssist.UI.ViewModels
{
    public class BaseViewModel : SetPropertyNotifyChanged
    {
        private static readonly List<BaseViewModel> _viewModels = new List<BaseViewModel>();
        private Options _currentOptions = Options.CurrentOptions;
        protected Dispatcher _dispatcher;

        public BaseViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;

            _viewModels.Add( this );

            AssistantOptions.ProfileChangingEvent += profile => { CurrentOptions = Options.CurrentOptions; };
        }

        public Options CurrentOptions
        {
            get => _currentOptions;
            set => SetProperty( ref _currentOptions, value );
        }

        public static BaseViewModel[] Instances => _viewModels.ToArray();

        ~BaseViewModel()
        {
            _viewModels.Remove( this );
        }
    }
}