using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ClassicAssist.Data;
using ClassicAssist.Shared.Resources;
using ClassicAssist.Shared.UI;

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
        private ObservableCollection<string> _profiles = new ObservableCollection<string>();
        private string _selectedProfile;

        public NewProfileViewModel()
        {
            Profiles = new ObservableCollection<string>( Options.GetProfiles().ToList().Select( Path.GetFileName ) );

            SelectedProfile = Profiles.FirstOrDefault( e => e.Equals( Options.CurrentOptions.Name ) );
        }

        public string FileName { get; set; }

        public string Name
        {
            get => _name;
            set => SetProperty( ref _name, value );
        }

        public ICommand OKCommand =>
            _okCommand ?? ( _okCommand = new RelayCommand( OK,
                o => !string.IsNullOrEmpty( Name ) &&
                     ( Option == NewProfileOption.Blank || !string.IsNullOrEmpty( SelectedProfile ) ) ) );

        public NewProfileOption Option
        {
            get => _option;
            set => SetProperty( ref _option, value );
        }

        public ObservableCollection<string> Profiles
        {
            get => _profiles;
            set => SetProperty( ref _profiles, value );
        }

        public string SelectedProfile
        {
            get => _selectedProfile;
            set => SetProperty( ref _selectedProfile, value );
        }

        private void OK( object obj )
        {
            string profileName = Name.Trim();

            bool valid = profileName.IndexOfAny( Path.GetInvalidFileNameChars() ) == -1;

            if ( valid )
            {
                string profilePath = Options.GetProfilePath();

                if ( File.Exists( Path.Combine( profilePath, $"{profileName}.json" ) ) )
                {
                    MessageBox.Show( Strings.A_profile_already_exists_by_this_name__choose_a_new_name___,
                        Strings.Error );
                    return;
                }

                FileName = $"{profileName}.json";

                if ( Option == NewProfileOption.Duplicate )
                {
                    string fullPath = Path.Combine( profilePath, FileName );

                    if ( SelectedProfile == Options.CurrentOptions.Name )
                    {
                        Options options = Options.CurrentOptions;
                        options.Name = $"{profileName}.json";
                        Options.Save( options );
                    }
                    else
                    {
                        File.Copy( Path.Combine( profilePath, SelectedProfile ), fullPath, true );
                        Options.Load( FileName, CurrentOptions );
                        Options.Save( CurrentOptions );
                    }
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