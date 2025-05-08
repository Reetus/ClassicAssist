#region License

// Copyright (C) 2025 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using System.Windows.Input;
using ClassicAssist.Data.Autoloot;
using ClassicAssist.Shared.UI;
using ICSharpCode.AvalonEdit.Document;

namespace ClassicAssist.UI.ViewModels.Agents.Autoloot.Python
{
    public class PythonFunctionsViewModel : BaseViewModel
    {
        private readonly AutolootManager _manager = AutolootManager.GetInstance();
        private int _caretPosition;

        private TextDocument _document;
        private string _documentText;
        private ICommand _saveCommand;

        public int CaretPosition
        {
            get => _caretPosition;
            set => SetProperty( ref _caretPosition, value );
        }

        public TextDocument Document
        {
            get => _document;
            set => SetProperty( ref _document, value );
        }

        public string DocumentText
        {
            get => _documentText;
            set => SetProperty( ref _documentText, value );
        }

        public ICommand SaveCommand => _saveCommand ?? ( _saveCommand = new RelayCommand( Save ) );

        private void Save( object obj )
        {
            _manager.SetPythonFunctionText( DocumentText );
            _manager.InitializeEngine();
        }
    }
}