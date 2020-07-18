using System;
using System.Text;
using Assistant;
using ClassicAssist.Misc;
using ClassicAssist.UO.Data;

namespace ClassicAssist.UO.Objects
{
    public abstract class Entity : IEquatable<Entity>
    {
        protected Entity( int serial )
        {
            Serial = serial;
        }

        public Direction Direction { get; set; }

        public virtual int Distance =>
            Math.Max( Math.Abs( X - Engine.Player?.X ?? X ), Math.Abs( Y - Engine.Player?.Y ?? Y ) );

        public int Hue { get; set; }

        [DisplayFormat( typeof( HexFormatProvider ) )]
        public virtual int ID { get; set; }

        public virtual string Name { get; set; }

        [DisplayFormat( typeof( PropertyArrayFormatProvider ) )]
        public Property[] Properties { get; set; }

        [DisplayFormat( typeof( HexFormatProvider ) )]
        public int Serial { get; }

        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public bool Equals( Entity other )
        {
            if ( ReferenceEquals( null, other ) )
            {
                return false;
            }

            if ( ReferenceEquals( this, other ) )
            {
                return true;
            }

            return Serial == other.Serial;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            ToString( sb );

            return sb.ToString();
        }

        protected virtual void ToString( StringBuilder sb )
        {
            sb.Append( $"Name: {Name}\n" );
            sb.Append( $"Serial: 0x{Serial:x}\n" );
            sb.Append( $"ID: 0x{ID:x}\n" );
            sb.Append( $"Hue: {(uint) Hue}\n" );
            sb.Append( $"X: {X}, Y: {Y}, Z: {Z}\n" );
        }

        public override bool Equals( object obj )
        {
            if ( ReferenceEquals( null, obj ) )
            {
                return false;
            }

            if ( ReferenceEquals( this, obj ) )
            {
                return true;
            }

            if ( obj.GetType() != GetType() )
            {
                return false;
            }

            return Equals( (Entity) obj );
        }

        public override int GetHashCode()
        {
            return Serial;
        }
    }
}