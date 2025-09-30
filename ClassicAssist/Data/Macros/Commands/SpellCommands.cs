using System.Diagnostics;
using Assistant;
using ClassicAssist.Data.Spells;
using ClassicAssist.Misc;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Objects;
using UOC = ClassicAssist.UO.Commands;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class SpellCommands
    {
        private static readonly SpellManager _manager = SpellManager.GetInstance();

        [CommandsDisplay( Category = nameof( Strings.Spells ),
            Parameters = new[] { nameof( ParameterType.SpellName ) } )]
        public static void Cast( string name )
        {
            _manager?.CastSpell( name );
        }

        [CommandsDisplay( Category = nameof( Strings.Spells ),
            Parameters = new[] { nameof( ParameterType.SpellName ), nameof( ParameterType.SerialOrAlias ) } )]
        public static bool Cast( string name, object obj )
        {
            int serial = AliasCommands.ResolveSerial( obj );

            if ( serial == 0 )
            {
                UOC.SystemMessage( Strings.Invalid_or_unknown_object_id, true );
                return false;
            }

            SpellData sd = _manager.GetSpellData( name ) ?? _manager.GetMasteryData( name );

            if ( sd == null )
            {
                UOC.SystemMessage( Strings.Unknown_spell___ );
                return false;
            }

            // Debugging only
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //

            _manager.CastSpell( sd.ID );

            int index = 0;
            bool result;

            int senderSerial = -1;

            if ( Options.CurrentOptions.UseExperimentalFizzleDetection )
            {
                ( index, result ) = UOC.WaitForTargetOrFizzle( sd.Timeout + 1000, out senderSerial );
            }
            else
            {
                result = TargetCommands.WaitForTarget( sd.Timeout + 500 );
            }

            if ( index == 0 && !result )
            {
                UOC.SystemMessage( Strings.Timeout___ );
                return false;
            }

            if ( !result )
            {
                return false;
            }

            // Debugging only
            sw.Stop();

            if ( Options.CurrentOptions.Debug )
            {
                UOC.SystemMessage(
                    $"Target received in {sw.ElapsedMilliseconds}ms, Spell data timeout: {sd.Timeout}ms" );
            }
            //

            TargetCommands.Target( serial, senderSerial: senderSerial );

            return true;
        }

        [CommandsDisplay( Category = nameof( Strings.Spells ) )]
        public static void InterruptSpell()
        {
            PlayerMobile player = Engine.Player;

            if ( player == null )
            {
                return;
            }

            Layer[] layerPriority =
            {
                Layer.Shirt, Layer.Shoes, Layer.Pants, Layer.Helm, Layer.Gloves, Layer.Ring, Layer.Neck,
                Layer.Waist, Layer.InnerTorso, Layer.Bracelet, Layer.MiddleTorso, Layer.Earrings, Layer.Arms,
                Layer.Cloak, Layer.OuterTorso, Layer.OuterLegs, Layer.InnerLegs, Layer.TwoHanded, Layer.OneHanded
            };

            Layer selectedLayer = Layer.Invalid;
            int serial = 0;

            foreach ( Layer layer in layerPriority )
            {
                serial = player.GetLayer( layer );

                if ( serial == 0 )
                {
                    continue;
                }

                selectedLayer = layer;
                break;
            }

            if ( selectedLayer == Layer.Invalid )
            {
                return;
            }

            UOC.EquipItem( serial, selectedLayer, QueuePriority.High );
        }
    }
}