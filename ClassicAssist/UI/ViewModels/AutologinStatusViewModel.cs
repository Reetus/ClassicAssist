#region License

// Copyright (C) 2022 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Input;
using ClassicAssist.Shared.UI;

namespace ClassicAssist.UI.ViewModels
{
    public class AutologinStatusViewModel : BaseViewModel
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private ICommand _cancelCommand;

        public AutologinStatusViewModel()
        {
            
        }
        
        public AutologinStatusViewModel( CancellationTokenSource cancellationTokenSource )
        {
            _cancellationTokenSource = cancellationTokenSource;
        }

        public ICommand CancelCommand => _cancelCommand ?? ( _cancelCommand = new RelayCommand( Cancel ) );
        public ObservableCollection<string> Messages { get; set; } = new ObservableCollection<string>();

        private void Cancel( object obj )
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}