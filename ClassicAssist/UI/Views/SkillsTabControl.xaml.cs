using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ClassicAssist.Data.Skills;
using ClassicAssist.UI.Misc;

namespace ClassicAssist.UI.Views
{
    /// <summary>
    ///     Interaction logic for SkillsTabControl.xaml
    /// </summary>
    public partial class SkillsTabControl : UserControl
    {
        private ListSortDirection _lastDirection;
        private GridViewColumnHeader _lastHeaderClicked;

        public SkillsTabControl()
        {
            InitializeComponent();
        }

        private void GridViewHeaderOnClick( object sender, RoutedEventArgs e )
        {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if ( headerClicked == null || headerClicked.Role == GridViewColumnHeaderRole.Padding )
            {
                return;
            }

            if ( !Equals( headerClicked, _lastHeaderClicked ) )
            {
                direction = ListSortDirection.Ascending;
            }
            else
            {
                direction = _lastDirection == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
            }

            if ( headerClicked.Column is SkillsGridViewColumn column )
            {
                Sort( (string) headerClicked.Column.Header, direction, column.SortField );
            }

            _lastHeaderClicked = headerClicked;
            _lastDirection = direction;
        }

        private void Sort( string header, ListSortDirection direction, SkillsGridViewColumn.Enums sortField )
        {
            ListCollectionView dataView =
                (ListCollectionView) CollectionViewSource.GetDefaultView( listView.ItemsSource );

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription( header, direction );
            dataView.SortDescriptions.Add( sd );

            dataView.CustomSort = new SkillComparer( direction, sortField );
            dataView.Refresh();
        }
    }
}