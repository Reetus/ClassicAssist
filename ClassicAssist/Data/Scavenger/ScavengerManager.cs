using System;
using System.Collections.ObjectModel;

namespace ClassicAssist.Data.Scavenger
{
    public class ScavengerManager
    {
        private static ScavengerManager _instance;
        private static readonly object _instanceLock = new object();

        private ScavengerManager()
        {
        }

        public Action CheckArea { get; set; }

        public Func<bool> IsEnabled { get; set; }

        public ObservableCollection<ScavengerEntry> Items { get; set; }
        public Func<bool, bool> SetEnabled { get; set; }

        public static ScavengerManager GetInstance()
        {
            // ReSharper disable once InvertIf
            if ( _instance == null )
            {
                lock ( _instanceLock )
                {
                    if ( _instance == null )
                    {
                        _instance = new ScavengerManager();
                    }
                }
            }

            return _instance;
        }
    }
}