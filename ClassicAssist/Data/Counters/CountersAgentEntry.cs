using System.Linq;
using Assistant;
using ClassicAssist.Shared.UI;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.Data.Counters
{
    public class CountersAgentEntry : SetPropertyNotifyChanged
    {
        private int _color;
        private int _count;

        public int Color
        {
            get => _color;
            set
            {
                SetProperty( ref _color, value );
                Recount();
            }
        }

        public int Count
        {
            get => _count;
            set => SetProperty( ref _count, value );
        }

        public int Graphic { get; set; }
        public string Name { get; set; }

        public void Recount()
        {
            if ( Engine.Player == null )
            {
                return;
            }

            if ( Engine.Player.Backpack?.Container == null )
            {
                Count = 0;
                return;
            }

            Item[] matches =
                Engine.Player.Backpack.Container.SelectEntities( i =>
                    i.ID == Graphic && ( Color == -1 || Color == i.Hue ) );

            Count = matches?.Sum( i => i.Count ) ?? 0;
        }
    }
}