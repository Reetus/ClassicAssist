using System;
using ClassicAssist.Shared.UI;
using ClassicAssist.UO.Objects;
using Newtonsoft.Json;

namespace ClassicAssist.Data.Autoloot
{
    public class PropertyEntry : SetPropertyNotifyChanged, IComparable<PropertyEntry>
    {
        private Type _allowedValuesEnum;
        private int _clilocIndex;
        private int[] _clilocs;
        private PropertyType _constraintType;
        private string _name;
        private Func<Entity, AutolootConstraintEntry, bool> _predicate;
        private string _shortName;

        [JsonIgnore]
        public Type AllowedValuesEnum
        {
            get => _allowedValuesEnum;
            set => SetProperty( ref _allowedValuesEnum, value );
        }

        public int ClilocIndex
        {
            get => _clilocIndex;
            set => SetProperty( ref _clilocIndex, value );
        }

        public int[] Clilocs
        {
            get => _clilocs;
            set => SetProperty( ref _clilocs, value );
        }

        public PropertyType ConstraintType
        {
            get => _constraintType;
            set => SetProperty( ref _constraintType, value );
        }

        public string Name
        {
            get => _name;
            set => SetProperty( ref _name, value );
        }

        public string ShortName
        {
            get => _shortName;
            set => SetProperty( ref _shortName, value );
        }

        [JsonIgnore]
        public Func<Entity, AutolootConstraintEntry, bool> Predicate
        {
            get => _predicate;
            set => SetProperty( ref _predicate, value );
        }

        public int CompareTo( PropertyEntry other )
        {
            if ( ReferenceEquals( this, other ) )
            {
                return 0;
            }

            if ( ReferenceEquals( null, other ) )
            {
                return 1;
            }

            int nameComparison = string.Compare( _name, other._name, StringComparison.InvariantCultureIgnoreCase );

            return nameComparison;
        }
    }
}