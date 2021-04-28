using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Threading;
using ClassicAssist.Data;
using ClassicAssist.Shared.UI;

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
}