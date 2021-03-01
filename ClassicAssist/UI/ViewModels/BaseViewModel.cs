using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using ClassicAssist.Data;

namespace ClassicAssist.UI.ViewModels
{
    public class BaseViewModel : SetPropertyNotifyChanged
    {
        private static readonly List<BaseViewModel> _viewModels = new List<BaseViewModel>();
        protected Dispatcher _dispatcher;

        public BaseViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;

            _viewModels.Add( this );

            Options.CurrentOptions.PropertyChanged += OnOptionChanged;
        }

        public static BaseViewModel[] Instances => _viewModels.ToArray();

        ~BaseViewModel()
        {
            _viewModels.Remove( this );
        }

        protected void OnOptionChanged( object sender, PropertyChangedEventArgs e )
        {
            PropertyInfo[] properties = GetType().GetProperties( BindingFlags.Public | BindingFlags.Instance );

            foreach ( PropertyInfo property in properties )
            {
                OptionsBindingAttribute attr = property.GetCustomAttribute<OptionsBindingAttribute>();

                if ( attr == null || attr.Property != e.PropertyName )
                {
                    continue;
                }

                if ( !( sender is Options options ) )
                {
                    continue;
                }

                PropertyInfo optionsProperty = options.GetType().GetProperty( e.PropertyName );

                if ( optionsProperty == null )
                {
                    continue;
                }

                object propertyValue = optionsProperty.GetValue( Options.CurrentOptions );

                property.SetValue( this, propertyValue );
            }
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