using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace ClassicAssist.Launcher
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        protected Dispatcher _dispatcher;

        public BaseViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // ReSharper disable once RedundantAssignment
        public virtual void SetProperty<T>( ref T obj, T value, [CallerMemberName] string propertyName = "" )
        {
            obj = value;
            NotifyPropertyChanged( propertyName );
            CommandManager.InvalidateRequerySuggested();
        }

        protected void NotifyPropertyChanged( [CallerMemberName] string propertyName = "" )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Func<object, bool> _canExecute;
        private readonly Action<object> _execute;

        public RelayCommand( Action<object> execute, Func<object, bool> canExecute = null )
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
                CanExecuteChangedInternal += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
                CanExecuteChangedInternal -= value;
            }
        }

        public bool CanExecute( object parameter )
        {
            return _canExecute == null || _canExecute( parameter );
        }

        public void Execute( object parameter )
        {
            _execute( parameter );
        }

        private event EventHandler CanExecuteChangedInternal;

        // ReSharper disable once UnusedMember.Global
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChangedInternal?.Invoke( this, EventArgs.Empty );
        }
    }

    public class RelayCommandAsync : ICommand
    {
        private readonly Func<object, bool> canExecuteMethod;
        private readonly Func<object, Task> executedMethod;

        public RelayCommandAsync( Func<object, Task> execute, Func<object, bool> canExecute )
        {
            executedMethod = execute ?? throw new ArgumentNullException( nameof( execute ) );
            canExecuteMethod = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute( object parameter )
        {
            return canExecuteMethod == null || canExecuteMethod( parameter );
        }

        public async void Execute( object parameter )
        {
            await executedMethod( parameter );
        }
    }
}