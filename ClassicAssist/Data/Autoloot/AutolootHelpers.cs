using System;
using System.Reflection;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.Data.Autoloot
{
    public static class AutolootHelpers
    {
        public static Action<int> SetAutolootContainer { get; set; }

        public static bool ItemHasObjectProperty( Item item, string propertyName )
        {
            PropertyInfo propertyInfo = item.GetType().GetProperty( propertyName );

            if ( propertyInfo == null )
            {
                return false;
            }

            return true;
        }

        public static T GetItemObjectPropertyValue<T>( Item item, string propertyName )
        {
            PropertyInfo propertyInfo = item.GetType().GetProperty( propertyName );

            if ( propertyInfo == null )
            {
                return default;
            }

            T val = (T) propertyInfo.GetValue( item );

            return val;
        }

        public static bool Operation( AutolootOperator @operator, int x, int y )
        {
            switch ( @operator )
            {
                case AutolootOperator.GreaterThan:
                    return x >= y;
                case AutolootOperator.LessThan:
                    return x <= y;
                case AutolootOperator.Equal:
                    return x == y;
                case AutolootOperator.NotEqual:
                    return x != y;
                default:
                    throw new ArgumentOutOfRangeException( nameof( @operator ), @operator, null );
            }
        }

        public static bool MatchProperty( Property property, int cliloc, PropertyEntry constraint,
            AutolootOperator @operator, int value )
        {
            return property.Cliloc == cliloc && ( constraint.ClilocIndex == -1 || Operation( @operator,
                                                      int.Parse( property.Arguments[constraint.ClilocIndex] ),
                                                      value ) );
        }
    }
}