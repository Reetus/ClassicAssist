using Assistant;
using ClassicAssist.Misc;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Objects
{
    public class Item : Entity
    {
        private string _name;

        public Item( int serial ) : base( serial )
        {
        }

        public Item( int serial, int containerSerial ) : base( serial )
        {
            Owner = containerSerial;
        }

        public int ArtDataID { get; set; }
        public ItemCollection Container { get; set; }
        public int Count { get; set; }
        public int Flags { get; set; }
        public int Grid { get; set; }
        public bool IsContainer => Container != null;
        public Layer Layer { get; set; }
        public int Light { get; set; }

        public override string Name
        {
            get => string.IsNullOrEmpty( _name ) ? TileData.GetStaticTile( ID ).Name : _name;
            set => _name = value;
        }

        [DisplayFormat( typeof( HexFormatProvider ) )]
        public int Owner { get; set; }

        [DisplayFormat( typeof( HexFormatProvider ) )]
        public int RootOwner
        {
            get
            {
                int owner = Owner;
                Item ownerItem;

                while ( ( ownerItem = Engine.Items.GetItem( owner ) )?.Owner != 0 )
                {
                    if ( ownerItem == null )
                    {
                        break;
                    }

                    owner = ownerItem.Owner;
                }

                return owner;
            }
        }

        public bool IsDescendantOf( int serial )
        {
            int owner = Owner;

            do
            {
                if ( owner == serial )
                {
                    return true;
                }

                owner = Engine.Items.GetItem( owner )?.Owner ?? 0;
            }
            while ( owner != 0 );

            return false;
        }
    }
}