// Copyright (C) 2022 Reetus
//  
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using ClassicAssist.Annotations;
using ClassicAssist.Data;
using ClassicAssist.Shared.UI;

namespace ClassicAssist.UI.Views.General
{
    /// <summary>
    ///     Interaction logic for AutologinConfigureWindow.xaml
    /// </summary>
    public partial class AutologinConfigureWindow : INotifyPropertyChanged
    {
        private string _account;
        private int _characterIndex;
        private TimeSpan _connectDelay;
        private ICommand _okCommand;
        private string _password;
        private TimeSpan _reconnectDelay;
        private int _serverIndex;

        public AutologinConfigureWindow()
        {
            InitializeComponent();

            foreach ( KeyValuePair<string, string> savedPassword in AssistantOptions.SavedPasswords )
            {
                Accounts.Add( savedPassword.Key );
            }

            Account = Options.CurrentOptions.AutologinUsername;
            Password = Options.CurrentOptions.AutologinPassword;
            ServerIndex = Options.CurrentOptions.AutologinServerIndex;
            CharacterIndex = Options.CurrentOptions.AutologinCharacterIndex;
            ConnectDelay = Options.CurrentOptions.AutologinConnectDelay;
            ReconnectDelay = Options.CurrentOptions.AutologinReconnectDelay;
        }

        public string Account
        {
            get => _account;
            set
            {
                _account = value;
                OnPropertyChanged();

                if ( value != null && AssistantOptions.SavedPasswords.ContainsKey( value ) )
                {
                    Password = AssistantOptions.SavedPasswords[value];
                }
            }
        }

        public ObservableCollection<string> Accounts { get; set; } = new ObservableCollection<string>();

        public int CharacterIndex
        {
            get => _characterIndex;
            set
            {
                _characterIndex = value;
                OnPropertyChanged();
            }
        }

        public TimeSpan ConnectDelay
        {
            get => _connectDelay;
            set
            {
                _connectDelay = value;
                OnPropertyChanged();
            }
        }

        public ICommand OkCommand => _okCommand ?? ( _okCommand = new RelayCommand( Ok ) );

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public TimeSpan ReconnectDelay
        {
            get => _reconnectDelay;
            set
            {
                _reconnectDelay = value;
                OnPropertyChanged();
            }
        }

        public int ServerIndex
        {
            get => _serverIndex;
            set
            {
                _serverIndex = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Ok( object obj )
        {
            Options.CurrentOptions.AutologinUsername = Account;
            Options.CurrentOptions.AutologinPassword = Password;
            Options.CurrentOptions.AutologinServerIndex = ServerIndex;
            Options.CurrentOptions.AutologinCharacterIndex = CharacterIndex;
            Options.CurrentOptions.AutologinConnectDelay = ConnectDelay;
            Options.CurrentOptions.AutologinReconnectDelay = ReconnectDelay;

            Options.Save( Options.CurrentOptions );
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }

    public class ComboValidationRule : ValidationRule
    {
        public override ValidationResult Validate( object value, CultureInfo cultureInfo )
        {
            return new ValidationResult( value != null, null );
        }
    }
}