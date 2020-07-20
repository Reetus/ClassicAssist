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

using ClassicAssist.Data.Targeting;

namespace ClassicAssist.Data.Hotkeys.Commands
{
    public class TargetAcquisitionEnemy
    {
        [HotkeyCommand( Name = "Get Next Friend", Category = "Target Acquisition - Enemy" )]
        public class GetNextFriend : HotkeyCommand
        {
            public override void Execute()
            {
                TargetManager.GetInstance().GetEnemy( TargetNotoriety.Friend, TargetBodyType.Any, TargetDistance.Next );
            }
        }

        [HotkeyCommand( Name = "Get Next Innocent", Category = "Target Acquisition - Enemy" )]
        public class GetNextInnocent : HotkeyCommand
        {
            public override void Execute()
            {
                TargetManager.GetInstance()
                    .GetEnemy( TargetNotoriety.Innocent, TargetBodyType.Any, TargetDistance.Closest );
            }
        }

        [HotkeyCommand( Name = "Get Next Gray", Category = "Target Acquisition - Enemy" )]
        public class GetNextGray : HotkeyCommand
        {
            public override void Execute()
            {
                TargetManager.GetInstance().GetEnemy( TargetNotoriety.Gray, TargetBodyType.Any, TargetDistance.Next );
            }
        }

        [HotkeyCommand( Name = "Get Next Criminal", Category = "Target Acquisition - Enemy" )]
        public class GetNextCriminal : HotkeyCommand
        {
            public override void Execute()
            {
                TargetManager.GetInstance()
                    .GetEnemy( TargetNotoriety.Criminal, TargetBodyType.Any, TargetDistance.Next );
            }
        }

        [HotkeyCommand( Name = "Get Next Gray / Criminal", Category = "Target Acquisition - Enemy" )]
        public class GetNextGrayCriminal : HotkeyCommand
        {
            public override void Execute()
            {
                TargetManager.GetInstance().GetEnemy( TargetNotoriety.Gray | TargetNotoriety.Criminal,
                    TargetBodyType.Any, TargetDistance.Next );
            }
        }

        [HotkeyCommand( Name = "Get Next Enemy", Category = "Target Acquisition - Enemy" )]
        public class GetNextEnemy : HotkeyCommand
        {
            public override void Execute()
            {
                TargetManager.GetInstance().GetEnemy( TargetNotoriety.Enemy, TargetBodyType.Any, TargetDistance.Next );
            }
        }

        [HotkeyCommand( Name = "Get Next Murderer", Category = "Target Acquisition - Enemy" )]
        public class GetNextMurderer : HotkeyCommand
        {
            public override void Execute()
            {
                TargetManager.GetInstance()
                    .GetEnemy( TargetNotoriety.Murderer, TargetBodyType.Any, TargetDistance.Next );
            }
        }

        [HotkeyCommand( Name = "Get Next Any", Category = "Target Acquisition - Enemy" )]
        public class GetNextAny : HotkeyCommand
        {
            public override void Execute()
            {
                TargetManager.GetInstance().GetEnemy( TargetNotoriety.Any, TargetBodyType.Any, TargetDistance.Next );
            }
        }
    }
}