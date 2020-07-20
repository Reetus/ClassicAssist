using System.ComponentModel;
using ClassicAssist.UI.Misc;

namespace ClassicAssist.Data.Autoloot
{
    [TypeConverter( typeof( EnumDescriptionTypeConverter ) )]
    public enum AutolootOperator
    {
        [Description( "==" )]
        Equal,

        [Description( "!=" )]
        NotEqual,

        [Description( ">=" )]
        GreaterThan,

        [Description( "<=" )]
        LessThan,

        [Description( "X" )]
        NotPresent
    }
}