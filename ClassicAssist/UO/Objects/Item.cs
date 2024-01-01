using System;
using System.Text;
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
        public int Count { get; set; } = 1;

        public override int Distance
        {
            get
            {
                if ( Owner == 0 )
                {
                    return Math.Max( Math.Abs( X - Engine.Player?.X ?? X ), Math.Abs( Y - Engine.Player?.Y ?? Y ) );
                }

                Entity entity = UOMath.IsMobile( Owner )
                    ? (Entity) Engine.Mobiles.GetMobile( Owner )
                    : Engine.Items.GetItem( Owner );

                return entity?.Distance ?? 0;
            }
        }

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

        public bool IsDescendantOf( int serial, int searchLevel = -1, int stopSerial = -1 )
        {
            int owner = Owner;
            int level = 0;

            do
            {
                if ( owner == serial )
                {
                    return true;
                }

                level++;

                if ( searchLevel != -1 && level > searchLevel )
                {
                    break;
                }

                if ( stopSerial != -1 && owner == stopSerial )
                {
                    break;
                }

                owner = Engine.Items.GetItem( owner )?.Owner ?? 0;
            }
            while ( owner != 0 );

            return false;
        }

        protected override void ToString( StringBuilder sb )
        {
            base.ToString( sb );

            if ( Container != null )
            {
                sb.AppendLine( Container.ToString() );
            }
        }
    }
}