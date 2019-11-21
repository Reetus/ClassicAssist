using System.Collections;
using System.Collections.Generic;

namespace ClassicAssist.UO.Objects
{
    public sealed class MobileCollection : EntityCollection<Mobile>, IEnumerable<Mobile>
    {
        private readonly ItemCollection _linkedItemCollection;

        public MobileCollection(ItemCollection linkedItemCollection)
        {
            _linkedItemCollection = linkedItemCollection;
        }

        public bool GetMobile(int serial, out Mobile mobile)
        {
            mobile = GetEntity(serial);
            return mobile != null;
        }

        public Mobile[] GetMobiles()
        {
            return GetEntities();
        }

        public override void RemoveByDistance(int maxDistance, int x, int y)
        {
            bool changed = false;

            Mobile[] mobiles = SelectEntities(m =>
            {
                double d = UOMath.Distance(x, y, m.X, m.Y);
                return d > maxDistance;
            });

            if (mobiles == null)
                return;

            foreach (Mobile m in mobiles)
            {
                Remove(m.Serial);
                _linkedItemCollection.RemoveByOwner(m.Serial);
                changed = true;
            }

            if (changed)
                OnCollectionChanged();
        }

        public delegate void dCollectionChanged(int totalCount);

        public event dCollectionChanged CollectionChanged;

        protected override void OnCollectionChanged()
        {
            CollectionChanged?.Invoke( EntityList.Count );
        }

        public IEnumerator<Mobile> GetEnumerator()
        {
            return EntityList.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}