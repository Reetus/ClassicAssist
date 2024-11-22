using System;
using System.Collections.ObjectModel;
using System.Text;
using ClassicAssist.Controls.DraggableTreeView;
using ClassicAssist.Shared.UI;

namespace ClassicAssist.Data.Autoloot
{
    public class AutolootEntry : SetPropertyNotifyChanged, IDraggableEntry
    {
        private bool _autoloot = true;
        private ObservableCollection<AutolootBaseModel> _children = new ObservableCollection<AutolootBaseModel>();

        private bool _enabled = true;
        private AutolootGroup _group;
        private int _id;
        private string _name;
        private AutolootPriority _priority = AutolootPriority.Normal;
        private bool _rehue;
        private int _rehueHue = 1153;

        public bool Autoloot
        {
            get => _autoloot;
            set => SetProperty( ref _autoloot, value );
        }

        public ObservableCollection<AutolootBaseModel> Children
        {
            get => _children;
            set => SetProperty( ref _children, value );
        }

        public bool Enabled
        {
            get => _enabled;
            set => SetProperty( ref _enabled, value );
        }

        public AutolootGroup Group
        {
            get => _group;
            set => SetProperty( ref _group, value );
        }

        public int ID
        {
            get => _id;
            set => SetProperty( ref _id, value );
        }

        public AutolootPriority Priority
        {
            get => _priority;
            set => SetProperty( ref _priority, value );
        }

        public bool Rehue
        {
            get => _rehue;
            set => SetProperty( ref _rehue, value );
        }

        public int RehueHue
        {
            get => _rehueHue;
            set => SetProperty( ref _rehueHue, value );
        }

        public string Name
        {
            get => _name;
            set => SetProperty( ref _name, value );
        }

        public string Describe()
        {
            if ( Children.Count == 0 )
            {
                return string.Empty;
            }

            StringBuilder stringBuilder = new StringBuilder();

            for ( int index = 0; index < Children.Count; index++ )
            {
                AutolootBaseModel child = Children[index];

                if ( child is AutolootPropertyEntry entry )
                {
                    stringBuilder.Append( $"({DescribeContraints( entry )})" );
                }
                else if ( child is AutolootPropertyGroup group )
                {
                    stringBuilder.Append( $"({DescribeGroup( group )})" );

                    if ( index + 1 < Children.Count )
                    {
                        switch ( group.Operation )
                        {
                            case BooleanOperation.And:
                                stringBuilder.Append( " && " );
                                break;
                            case BooleanOperation.Or:
                                stringBuilder.Append( " || " );
                                break;
                            case BooleanOperation.Not:
                                stringBuilder.Append( "!" );
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }

            return stringBuilder.ToString();
        }

        private static string DescribeGroup( AutolootPropertyGroup group )
        {
            StringBuilder stringBuilder = new StringBuilder();

            for ( int index = 0; index < group.Children.Count; index++ )
            {
                AutolootBaseModel child = group.Children[index];

                switch ( child )
                {
                    case AutolootPropertyEntry entry:
                        stringBuilder.Append( $"{DescribeContraints( entry )}" );
                        break;
                    case AutolootPropertyGroup childGroup:
                        stringBuilder.Append( $"({DescribeGroup( childGroup )})" );

                        if ( group.Children.Count > 0 && index + 1 < group.Children.Count )
                        {
                            switch ( group.Operation )
                            {
                                case BooleanOperation.And:
                                    stringBuilder.Append( " && " );
                                    break;
                                case BooleanOperation.Or:
                                    stringBuilder.Append( " || " );
                                    break;
                                case BooleanOperation.Not:
                                    stringBuilder.Append( "!" );
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }

                        break;
                }
            }

            return stringBuilder.ToString();
        }

        private static string DescribeContraints( AutolootPropertyEntry entry )
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach ( AutolootConstraintEntry constraint in entry.Constraints )
            {
                if ( stringBuilder.Length > 0 )
                {
                    stringBuilder.Append( " && " );
                }

                string propertyName = constraint.Property.ShortName;

                if ( string.IsNullOrEmpty( propertyName ) )
                {
                    propertyName = constraint.Property.Name;
                }

                string op;

                switch ( constraint.Operator )
                {
                    case AutolootOperator.Equal:
                        op = "==";
                        break;
                    case AutolootOperator.NotEqual:
                        op = "!=";
                        break;
                    case AutolootOperator.GreaterThan:
                        op = ">=";
                        break;
                    case AutolootOperator.LessThan:
                        op = "<=";
                        break;
                    case AutolootOperator.NotPresent:
                        op = "!";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                stringBuilder.Append( $"{propertyName} {op}{( constraint.Operator == AutolootOperator.NotPresent ? "" : $" {constraint.Value.ToString()}" )}" );
            }

            return stringBuilder.ToString();
        }
    }
}