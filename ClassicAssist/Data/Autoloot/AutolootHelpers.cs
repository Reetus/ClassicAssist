using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;

namespace ClassicAssist.Data.Autoloot
{
    public static class AutolootHelpers
    {
        private static readonly AutolootManager _manager;
        private static readonly Regex _regex;

        static AutolootHelpers()
        {
            _manager = AutolootManager.GetInstance();
            _regex = new Regex( "<BASEFONT[^>]*>(.*)(<\\/BASEFONT>)?", RegexOptions.Compiled | RegexOptions.IgnoreCase );
        }

        public static Action<int> SetAutolootContainer { get; set; }

        public static IEnumerable<Item> AutolootFilter( IEnumerable<Item> items, AutolootEntry entry )
        {
            return items == null
                ? null
                : ( from item in items
                    where entry.ID == -1 || item.ID == entry.ID
                    let predicates = ConstraintsToPredicates( entry.Constraints )
                    where !predicates.Any() || CheckPredicates( item, predicates )
                    select item ).ToList();
        }

        private static bool CheckPredicates( Item item, IEnumerable<Predicate<Item>> predicates )
        {
            return predicates.All( predicate => predicate( item ) );
        }

        public static IEnumerable<Predicate<Item>> ConstraintsToPredicates(
            IEnumerable<AutolootConstraintEntry> constraints )
        {
            List<Predicate<Item>> predicates = new List<Predicate<Item>>();

            foreach ( AutolootConstraintEntry constraint in constraints )
            {
                switch ( constraint.Property.ConstraintType )
                {
                    case PropertyType.Properties:
                        if ( constraint.Operator != AutolootOperator.NotPresent )
                        {
                            predicates.Add( i => i.Properties != null && constraint.Property.Clilocs.Any( cliloc =>
                                i.Properties.Any( p => MatchProperty( p, cliloc, constraint.Property,
                                    constraint.Operator, constraint.Value ) ) ) );
                        }
                        else
                        {
                            predicates.Add( i =>
                                i.Properties != null && !constraint.Property.Clilocs.Any( cliloc =>
                                    i.Properties.Any( p => p.Cliloc == cliloc ) ) );
                        }

                        break;
                    case PropertyType.Object:

                        predicates.Add( i =>
                            ItemHasObjectProperty( i, constraint.Property.Name ) && Operation( constraint.Operator,
                                GetItemObjectPropertyValue<int>( i, constraint.Property.Name ), constraint.Value ) );

                        break;
                    case PropertyType.Predicate:
                    case PropertyType.PredicateWithValue:

                        predicates.Add( i =>
                            constraint.Property.Predicate != null &&
                            constraint.Property.Predicate.Invoke( i, constraint ) );

                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return predicates;
        }

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

        public static bool Operation( AutolootOperator @operator, string x, string y )
        {
            switch ( @operator )
            {
                case AutolootOperator.Equal:
                    return x.Equals( y );
                case AutolootOperator.NotEqual:
                case AutolootOperator.GreaterThan:
                case AutolootOperator.LessThan:
                    return !x.Equals( y );
                default:
                    throw new ArgumentOutOfRangeException( nameof( @operator ), @operator, null );
            }
        }

        public static bool MatchProperty( Property property, int cliloc, PropertyEntry constraint,
            AutolootOperator @operator, int value )
        {
            try
            {
                bool result = property.Cliloc == cliloc && ( constraint.ClilocIndex == -1 || Operation( @operator,
                    int.Parse( property.Arguments[constraint.ClilocIndex] ), value ) );

                if ( result )
                {
                    return true;
                }

                if ( !_manager.MatchTextValue() )
                {
                    return false;
                }

                string clilocString = Cliloc.GetProperty( cliloc );

                if ( property.Text.Equals( clilocString ) )
                {
                    return true;
                }

                Match matches = _regex.Match( property.Text );

                return matches.Success && matches.Groups[1].Value.Equals( clilocString );
            }
            catch ( IndexOutOfRangeException )
            {
                return false;
            }
        }
    }
}