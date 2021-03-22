#region License

// Copyright (C) 2020 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assistant;
using ClassicAssist.Misc;
using ClassicAssist.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Network.Packets;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class WandCommands
    {
        public enum WandTypes
        {
            Clumsy,
            Identification,
            Heal,
            Feeblemind,
            Weaken,
            Magic_Arrow,
            Harm,
            Fireball,
            Greater_Heal,
            Lightning,
            Mana_Drain
        }

        private static readonly Dictionary<WandTypes, int[]> _wandClilocs = new Dictionary<WandTypes, int[]>
        {
            { WandTypes.Clumsy, new[] { 3002011, 1017326 } },
            { WandTypes.Identification, new[] { 1044063, 1017350 } },
            { WandTypes.Heal, new[] { 3002014, 1017329 } },
            { WandTypes.Feeblemind, new[] { 3002013, 1017327 } },
            { WandTypes.Weaken, new[] { 3002018, 1017328 } },
            { WandTypes.Magic_Arrow, new[] { 3002015, 1060492 } },
            { WandTypes.Harm, new[] { 3002022, 1017334 } },
            { WandTypes.Fireball, new[] { 3002028, 1060487 } },
            { WandTypes.Greater_Heal, new[] { 3002039, 1017330 } },
            { WandTypes.Lightning, new[] { 3002040, 1060491 } },
            { WandTypes.Mana_Drain, new[] { 3002041, 1017339 } }
        };

        private static readonly int[] _wandIds = { 0xDF2, 0xDF3, 0xDF4, 0xDF5 };

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[] { nameof( ParameterType.WandName ), nameof( ParameterType.IntegerValue ) } )]
        [CommandsDisplayStringSeeAlso( new[] { nameof( WandTypes ) } )]
        public static bool EquipWand( string wandName, int minimumCharges = -1 )
        {
            try
            {
                WandTypes wandType = Utility.GetEnumValueByName<WandTypes>( wandName );

                int containerSerial = AliasCommands.ResolveSerial( "backpack" );

                Item[] matches = FindWands( wandType, containerSerial, minimumCharges ).Result;

                if ( matches == null )
                {
                    UOC.SystemMessage( Strings.Cannot_find_item___ );
                    return false;
                }

                ActionCommands.EquipItem( matches.First().Serial, Layer.OneHanded );

                return true;
            }
            catch ( InvalidOperationException )
            {
                UOC.SystemMessage( string.Format( Strings.Invalid_skill_name___0__, wandName ) );
            }

            return false;
        }

        [CommandsDisplay( Category = nameof( Strings.Entity ),
            Parameters = new[]
            {
                nameof( ParameterType.WandName ), nameof( ParameterType.SerialOrAlias ),
                nameof( ParameterType.IntegerValue )
            } )]
        [CommandsDisplayStringSeeAlso( new[] { nameof( WandTypes ) } )]
        public static bool FindWand( string wandName, object containerSource = null, int minimumCharges = -1 )
        {
            try
            {
                WandTypes wandType = Utility.GetEnumValueByName<WandTypes>( wandName );

                int containerSerial = AliasCommands.ResolveSerial( containerSource );

                if ( containerSource == null )
                {
                    containerSerial = -1;
                }

                Item[] matches = FindWands( wandType, containerSerial, minimumCharges ).Result;

                if ( matches == null )
                {
                    UOC.SystemMessage( Strings.Cannot_find_item___ );
                    return false;
                }

                AliasCommands.SetMacroAlias( "found", matches.First().Serial );

                if ( MacroManager.QuietMode )
                {
                    return true;
                }

                UOC.SystemMessage( string.Format( Strings.Object___0___updated___, "found" ) );

                return true;
            }
            catch ( InvalidOperationException )
            {
                UOC.SystemMessage( string.Format( Strings.Invalid_skill_name___0__, wandName ) );
            }

            return false;
        }

        private static async Task<Item[]> FindWands( WandTypes wandType, int containerSerial, int minimumCharges )
        {
            // Hybrid has FeatureFlags.AOS, think of better solution
            if ( !Engine.Features.HasFlag( FeatureFlags.AOS ) || Engine.CurrentShard.Name.Equals( "UOHybrid" ) )
            {
                Item[] allWands = Engine.Items.SelectEntities( i =>
                    _wandIds.Contains( i.ID ) && !ObjectCommands.InIgnoreList(i.Serial) && ( containerSerial == -1 || i.IsDescendantOf( containerSerial ) ) );

                if ( allWands == null )
                {
                    return null;
                }

                foreach ( Item wand in allWands )
                {
                    Engine.SendPacketToServer( new LookRequest( wand.Serial ) );
                    await UOC.WaitForIncomingPacketFilterAsync(
                        new PacketFilterInfo( 0xBF,
                            new[]
                            {
                                PacketFilterConditions.ShortAtPositionCondition( 0x10, 3 ),
                                PacketFilterConditions.IntAtPositionCondition( wand.Serial, 5 )
                            } ), 2500 );
                }
            }

            Item[] matches = Engine.Items.SelectEntities( i =>
                _wandIds.Contains( i.ID ) && !ObjectCommands.InIgnoreList(i.Serial) && ( containerSerial == -1 || i.IsDescendantOf( containerSerial ) ) &&
                i.Properties != null && i.Properties.Any( p =>
                    _wandClilocs[wandType].Contains( p.Cliloc ) &&
                    ( minimumCharges == -1 || int.Parse( p.Arguments[0] ) >= minimumCharges ) ) );

            return matches;
        }
    }
}
