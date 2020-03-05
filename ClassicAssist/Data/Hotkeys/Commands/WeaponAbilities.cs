using Assistant;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.UO.Network.Packets;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    [HotkeyCommand( Category = "Abilities", Name = "Set Primary Ability" )]
    public class SetPrimaryAbility : HotkeyCommand
    {
        public override void Execute()
        {
            AbilitiesCommands.SetAbility( "primary", "on" );
        }
    }

    [HotkeyCommand( Category = "Abilities", Name = "Unset Primary Ability" )]
    public class UnsetPrimaryAbility : HotkeyCommand
    {
        public override void Execute()
        {
            AbilitiesCommands.SetAbility( "primary", "off" );
        }
    }

    [HotkeyCommand( Category = "Abilities", Name = "Set Secondary Ability" )]
    public class SetSecondaryAbility : HotkeyCommand
    {
        public override void Execute()
        {
            AbilitiesCommands.SetAbility( "secondary", "on" );
        }
    }

    [HotkeyCommand( Category = "Abilities", Name = "Unset Secondary Ability" )]
    public class UnsetSecondaryAbility : HotkeyCommand
    {
        public override void Execute()
        {
            AbilitiesCommands.SetAbility( "secondary", "off" );
        }
    }

    [HotkeyCommand( Category = "Abilities", Name = "Toggle Primary Ability" )]
    public class TogglePrimaryAbility : HotkeyCommand
    {
        public override void Execute()
        {
            AbilitiesCommands.SetAbility( "primary" );
        }
    }

    [HotkeyCommand( Category = "Abilities", Name = "Toggle Secondary Ability" )]
    public class ToggleSecondaryAbility : HotkeyCommand
    {
        public override void Execute()
        {
            AbilitiesCommands.SetAbility( "secondary" );
        }
    }

    [HotkeyCommand( Category = "Abilities", Name = "Clear Weapon Ability" )]
    public class ClearWeaponAbility : HotkeyCommand
    {
        public override void Execute()
        {
            AbilitiesCommands.ClearAbility();
        }
    }

    [HotkeyCommand( Category = "Abilities", Name = "Stun" )]
    public class StunAbility : HotkeyCommand
    {
        public override void Execute()
        {
            Engine.SendPacketToServer( new StunRequest() );
        }
    }

    [HotkeyCommand( Category = "Abilities", Name = "Disarm" )]
    public class DisarmAbility : HotkeyCommand
    {
        public override void Execute()
        {
            Engine.SendPacketToServer( new DisarmRequest() );
        }
    }
}