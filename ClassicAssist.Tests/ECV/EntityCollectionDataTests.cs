using System.Collections.Generic;
using System.ComponentModel;
using ClassicAssist.UI.ViewModels;
using ClassicAssist.UO.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests.ECV
{
    [TestClass]
    public class EntityCollectionDataTests
    {
        [TestMethod]
        public void NotifyPropertiesUpdatedRaisesChangeForDisplayedProperties()
        {
            EntityCollectionData data = new EntityCollectionData { Entity = new Item( 0x40000001 ) };

            List<string> raised = new List<string>();

            void OnPropertyChanged( object sender, PropertyChangedEventArgs e )
            {
                raised.Add( e.PropertyName );
            }

            data.PropertyChanged += OnPropertyChanged;

            try
            {
                data.NotifyPropertiesUpdated();
            }
            finally
            {
                data.PropertyChanged -= OnPropertyChanged;
            }

            CollectionAssert.Contains( raised, nameof( EntityCollectionData.Name ) );
            CollectionAssert.Contains( raised, nameof( EntityCollectionData.FullName ) );
            CollectionAssert.Contains( raised, "Bitmap" );
        }
    }
}
