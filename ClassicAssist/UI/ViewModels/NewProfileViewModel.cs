using System.IO;
using System.Windows;
using System.Windows.Input;
using ClassicAssist.Data;
using ClassicAssist.Resources;

namespace ClassicAssist.UI.ViewModels
{
    public enum NewProfileOption
    {
        Blank,
        Duplicate
    }

    public class NewProfileViewModel : BaseViewModel
    {
        private string _name;
        private ICommand _okCommand;
        private NewProfileOption _option = NewProfileOption.Duplicate;

        public string FileName { get; set; }

        public string Name
        {
            get => _name;
            set => SetProperty( ref _name, value );
        }

        public ICommand OkCommand =>
            _okCommand ?? ( _okCommand = new RelayCommand( Ok, o => !string.IsNullOrEmpty( Name ) ) );

        public NewProfileOption Option
        {
            get => _option;
            set => SetProperty( ref _option, value );
        }

        private void Ok( object obj )
        {
            string profileName = Name.Trim();

            bool valid = profileName.IndexOfAny( Path.GetInvalidFileNameChars() ) == -1;

            if ( valid )
            {
                FileName = $"{profileName}.json";

                if ( Option == NewProfileOption.Duplicate )
                {
                    Options options = Options.CurrentOptions;
                    options.Name = $"{profileName}.json";
                    Options.Save( options );
                }
                else
                {
                    Options.ClearOptions();
                    Options options = new Options { Name = $"{profileName}.json" };
                    Options.CurrentOptions = options;
                    Options.Load( options.Name, options );
                    Options.Save( options );
                }
            }
            else
            {
                MessageBox.Show( Strings.Profile_name_contains_illegal_characters_, Strings.Error, MessageBoxButton.OK,
                    MessageBoxImage.Error );
            }
        }
    }
}