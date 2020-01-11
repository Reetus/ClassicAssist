using ClassicAssist.Data.Macros.Commands;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    [HotkeyCommand( Category = "Consume", Name = "Cure Potion" )]
    public class UseCurePotion : HotkeyCommand
    {
        public override void Execute()
        {
            ObjectCommands.UseType( 0xf07, 0, "backpack" );
        }
    }

    [HotkeyCommand( Category = "Consume", Name = "Heal Potion" )]
    public class UseHealPotion : HotkeyCommand
    {
        public override void Execute()
        {
            ObjectCommands.UseType( 0xf0c, 0, "backpack" );
        }
    }

    [HotkeyCommand( Category = "Consume", Name = "Refresh Potion" )]
    public class UseRefreshPotion : HotkeyCommand
    {
        public override void Execute()
        {
            ObjectCommands.UseType( 0xf0b, 0, "backpack" );
        }
    }

    [HotkeyCommand( Category = "Consume", Name = "Strength Potion" )]
    public class UseStrengthPotion : HotkeyCommand
    {
        public override void Execute()
        {
            ObjectCommands.UseType( 0xf09, 0, "backpack" );
        }
    }

    [HotkeyCommand( Category = "Consume", Name = "Agility Potion" )]
    public class UseAgilityPotion : HotkeyCommand
    {
        public override void Execute()
        {
            ObjectCommands.UseType( 0xf08, 0, "backpack" );
        }
    }

    [HotkeyCommand( Category = "Consume", Name = "Explosion Potion" )]
    public class UseExplosionPotion : HotkeyCommand
    {
        public override void Execute()
        {
            ObjectCommands.UseType( 0xf0d, 0, "backpack" );
        }
    }

    [HotkeyCommand( Category = "Consume", Name = "Invisibility Potion" )]
    public class UseInvisibilityPotion : HotkeyCommand
    {
        public override void Execute()
        {
            ObjectCommands.UseType( 0xf06, 306, "backpack" );
        }
    }

    [HotkeyCommand( Category = "Consume", Name = "Confusion Blast Potion" )]
    public class UseConfusionBlastPotion : HotkeyCommand
    {
        public override void Execute()
        {
            ObjectCommands.UseType( 0xf06, 1165, "backpack" );
        }
    }

    [HotkeyCommand( Category = "Consume", Name = "Conflagration Potion" )]
    public class UseConflagPotion : HotkeyCommand
    {
        public override void Execute()
        {
            ObjectCommands.UseType( 0xf06, 1161, "backpack" );
        }
    }

    [HotkeyCommand( Category = "Consume", Name = "Smoke Bomb" )]
    public class UseSmokeBomb : HotkeyCommand
    {
        public override void Execute()
        {
            ObjectCommands.UseType( 0x2808, 0, "backpack" );
        }
    }

    [HotkeyCommand( Category = "Consume", Name = "Grapes of Wrath" )]
    public class UseGrapesOfWrath : HotkeyCommand
    {
        public override void Execute()
        {
            ObjectCommands.UseType( 0x2fd7, 1154, "backpack" );
        }
    }

    [HotkeyCommand( Category = "Consume", Name = "Fruit Bowl" )]
    public class UseFruitBowl : HotkeyCommand
    {
        public override void Execute()
        {
            ObjectCommands.UseType( 0x2d4f, 0, "backpack" );
        }
    }

    [HotkeyCommand( Category = "Consume", Name = "Enchanted Apple" )]
    public class UseEnchantedApple : HotkeyCommand
    {
        public override void Execute()
        {
            ObjectCommands.UseType( 0x2fd8, 1160, "backpack" );
        }
    }

    [HotkeyCommand( Category = "Consume", Name = "Trinsic Rose Petal" )]
    public class UseTrinsicPetal : HotkeyCommand
    {
        public override void Execute()
        {
            ObjectCommands.UseType( 0x1021, 14, "backpack" );
        }
    }

    [HotkeyCommand( Category = "Consume", Name = "Orange Petal" )]
    public class UseOrangePetal : HotkeyCommand
    {
        public override void Execute()
        {
            ObjectCommands.UseType( 0x1021, 43, "backpack" );
        }
    }

    [HotkeyCommand( Category = "Consume", Name = "Bola" )]
    public class UseBolaCommand : HotkeyCommand
    {
        public override void Execute()
        {
            ObjectCommands.UseType( 0x26ac, 0, "backpack" );
        }
    }
}