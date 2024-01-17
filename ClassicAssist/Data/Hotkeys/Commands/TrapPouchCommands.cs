#region License

// Copyright (C) 2024 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using ClassicAssist.Data.TrapPouch;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    [HotkeyCommand( Name = "Use Trap Pouch", Category = "Trap Pouches" )]
    public class UseTrapPouch : HotkeyCommand
    {
        public override void Execute()
        {
            TrapPouchManager manager = TrapPouchManager.GetInstance();

            manager.Use?.Invoke();
        }
    }

    [HotkeyCommand( Name = "Clear Trap Pouches", Category = "Trap Pouches" )]
    public class ClearTrapPouches : HotkeyCommand
    {
        public override void Execute()
        {
            TrapPouchManager manager = TrapPouchManager.GetInstance();

            manager.Clear?.Invoke();
        }
    }
}