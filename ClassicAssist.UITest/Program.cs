using System;
using System.Linq;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.UI.ViewModels;
using ClassicAssist.UI.Views;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.UITest
{
    internal class Program
    {
        private const string UO_FOLDER = @"path\to";

        [STAThread]
        private static void Main()
        {
            Engine.StartupPath = Environment.CurrentDirectory;

            LoadData( UO_FOLDER );
            ShowECV();
            //ShowMain();
        }

        // ReSharper disable once UnusedMember.Global
        public static void ShowMain()
        {
            AssistantOptions.Load();
            MainWindow window = new MainWindow();

            window.ShowDialog();
        }

        // ReSharper disable once UnusedMember.Global
        public static void ShowECV()
        {
            ItemCollection collection = new ItemCollection( 0 );

            for ( int i = 0; i < 6; i++ )
            {
                collection.Add( new Item( i )
                {
                    ID = 100 + i,
                    Properties = new[]
                    {
                        new Property
                        {
                            Cliloc = 100 + i + 1020000, Text = Cliloc.GetProperty( 100 + i + 1020000 )
                        }
                    }
                } );
            }

            Property antiqueProperty = new Property { Cliloc = 1152714, Text = Cliloc.GetProperty( 1152714 ) };
            Property majorProperty = new Property { Cliloc = 1151494, Text = Cliloc.GetProperty( 1151494 ) };
            Property legendProperty = new Property { Cliloc = 1151495, Text = Cliloc.GetProperty( 1151495 ) };
            Property castingFocusPropery = new Property
            {
                Cliloc = 1113696,
                Text = Cliloc.GetLocalString( 1113696, new[] { "15" } ),
                Arguments = new[] { "15" }
            };
            Property tamingProperty = new Property
            {
                Cliloc = 1060451,
                Text = Cliloc.GetLocalString( 1060451, new[] { "Animal Taming", "12" } ),
                Arguments = new[] { "Animal Taming", "12" }
            };

            collection[0].Properties = collection[0].Properties.Append( antiqueProperty ).ToArray();
            collection[1].Properties =
                collection[1].Properties.Append( majorProperty ).Append( tamingProperty ).ToArray();
            collection[2].Properties = collection[2].Properties.Append( legendProperty ).ToArray();
            collection[3].Properties =
                collection[3].Properties.Append( antiqueProperty ).Append( majorProperty ).ToArray();
            collection[4].Properties =
                collection[4].Properties.Append( antiqueProperty ).Append( legendProperty ).ToArray();
            collection[5].Properties = collection[5].Properties.Append( castingFocusPropery ).Append( legendProperty )
                .ToArray();

            new EntityCollectionViewer
            {
                DataContext =
                    new EntityCollectionViewerViewModel( collection ) { ShowProperties = true, ShowFilter = true }
            }.ShowDialog();
        }

        public static void LoadData( string folder )
        {
            Engine.ClientPath = folder;
            Art.Initialize( folder );
            Hues.Initialize( folder );
            Cliloc.Initialize( folder );
            Skills.Initialize( folder );
            Speech.Initialize( folder );
            TileData.Initialize( folder );
            Statics.Initialize( folder );
            MapInfo.Initialize( folder );
        }
    }
}