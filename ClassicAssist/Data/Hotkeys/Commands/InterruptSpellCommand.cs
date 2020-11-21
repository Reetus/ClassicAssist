using ClassicAssist.Data.Macros.Commands;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    [HotkeyCommand( Name = "Interrupt Spell" )]
    public class InterruptSpellCommand : HotkeyCommand
    {
        public override void Execute()
        {
            SpellCommands.InterruptSpell();
        }
    }
}