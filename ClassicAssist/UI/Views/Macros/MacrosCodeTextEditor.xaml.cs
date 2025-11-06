#region License

// Copyright (C) 2025 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using Assistant;
using ClassicAssist.Data.Macros;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Search;

namespace ClassicAssist.UI.Views.Macros
{
    /// <summary>
    ///     Interaction logic for MacrosCodeTextEditor.xaml
    /// </summary>
    public partial class MacrosCodeTextEditor
    {
        public static readonly DependencyProperty FormatErrorProperty =
            DependencyProperty.Register( nameof( FormatError ), typeof( string ), typeof( MacrosCodeTextEditor ), new PropertyMetadata( default( string ) ) );

        public static readonly DependencyProperty IsPerformingActionProperty =
            DependencyProperty.Register( nameof( IsPerformingAction ), typeof( bool ), typeof( MacrosCodeTextEditor ), new PropertyMetadata( false ) );

        public static readonly DependencyProperty ClearExceptionCommandProperty = DependencyProperty.Register( nameof( ClearExceptionCommand ), typeof( ICommand ),
            typeof( MacrosCodeTextEditor ), new PropertyMetadata( default( ICommand ) ) );

        public static readonly DependencyProperty CaretPositionProperty =
            DependencyProperty.Register( nameof( CaretPosition ), typeof( int ), typeof( MacrosCodeTextEditor ), new PropertyMetadata( 0 ) );

        public static readonly DependencyProperty DocumentProperty = DependencyProperty.Register( nameof( Document ), typeof( TextDocument ), typeof( MacrosCodeTextEditor ),
            new PropertyMetadata( default( TextDocument ) ) );

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register( nameof( SelectedItem ), typeof( MacroEntry ), typeof( MacrosCodeTextEditor ),
            new PropertyMetadata( null ) );

        public MacrosCodeTextEditor()
        {
            InitializeComponent();
        }

        public int CaretPosition
        {
            get => (int) GetValue( CaretPositionProperty );
            set => SetValue( CaretPositionProperty, value );
        }

        public ICommand ClearExceptionCommand
        {
            get => (ICommand) GetValue( ClearExceptionCommandProperty );
            set => SetValue( ClearExceptionCommandProperty, value );
        }

        public TextDocument Document
        {
            get => (TextDocument) GetValue( DocumentProperty );
            set => SetValue( DocumentProperty, value );
        }

        public string FormatError
        {
            get => (string) GetValue( FormatErrorProperty );
            set => SetValue( FormatErrorProperty, value );
        }

        public bool IsPerformingAction
        {
            get => (bool) GetValue( IsPerformingActionProperty );
            set => SetValue( IsPerformingActionProperty, value );
        }

        public MacroEntry SelectedItem
        {
            get => (MacroEntry) GetValue( SelectedItemProperty );
            set => SetValue( SelectedItemProperty, value );
        }

        private void Grid_Initialized( object sender, EventArgs e )
        {
            CodeTextEditor.SyntaxHighlighting = HighlightingLoader.Load(
                new XmlTextReader( Path.Combine( Engine.StartupPath, "Python.Dark.xshd" ) ), HighlightingManager.Instance );

            SearchPanel.Install( CodeTextEditor );
        }
    }
}