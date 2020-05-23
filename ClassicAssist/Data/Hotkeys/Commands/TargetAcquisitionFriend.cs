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
    public class TargetAcquisitionFriend
    {
        [HotkeyCommand( Name = "Get Next Friend", Category = "Target Acquisition - Friend" )]
        public class GetNextFriend : HotkeyCommand
        {
            public override void Execute()
            {
                TargetManager.GetInstance()
                    .GetFriend( TargetNotoriety.Friend, TargetBodyType.Any, TargetDistance.Next );
            }
        }

        [HotkeyCommand( Name = "Get Next Innocent", Category = "Target Acquisition - Friend" )]
        public class GetNextInnocent : HotkeyCommand
        {
            public override void Execute()
            {
                TargetManager.GetInstance()
                    .GetFriend( TargetNotoriety.Innocent, TargetBodyType.Any, TargetDistance.Closest );
            }
        }

        [HotkeyCommand( Name = "Get Next Gray", Category = "Target Acquisition - Friend" )]
        public class GetNextGray : HotkeyCommand
        {
            public override void Execute()
            {
                TargetManager.GetInstance().GetFriend( TargetNotoriety.Gray, TargetBodyType.Any, TargetDistance.Next );
            }
        }

        [HotkeyCommand( Name = "Get Next Criminal", Category = "Target Acquisition - Friend" )]
        public class GetNextCriminal : HotkeyCommand
        {
            public override void Execute()
            {
                TargetManager.GetInstance()
                    .GetFriend( TargetNotoriety.Criminal, TargetBodyType.Any, TargetDistance.Next );
            }
        }

        [HotkeyCommand( Name = "Get Next Gray / Criminal", Category = "Target Acquisition - Friend" )]
        public class GetNextGrayCriminal : HotkeyCommand
        {
            public override void Execute()
            {
                TargetManager.GetInstance().GetFriend( TargetNotoriety.Gray | TargetNotoriety.Criminal,
                    TargetBodyType.Any, TargetDistance.Next );
            }
        }

        [HotkeyCommand( Name = "Get Next Enemy", Category = "Target Acquisition - Friend" )]
        public class GetNextEnemy : HotkeyCommand
        {
            public override void Execute()
            {
                TargetManager.GetInstance().GetFriend( TargetNotoriety.Enemy, TargetBodyType.Any, TargetDistance.Next );
            }
        }

        [HotkeyCommand( Name = "Get Next Murderer", Category = "Target Acquisition - Friend" )]
        public class GetNextMurderer : HotkeyCommand
        {
            public override void Execute()
            {
                TargetManager.GetInstance()
                    .GetFriend( TargetNotoriety.Murderer, TargetBodyType.Any, TargetDistance.Next );
            }
        }

        [HotkeyCommand( Name = "Get Next Any", Category = "Target Acquisition - Friend" )]
        public class GetNextAny : HotkeyCommand
        {
            public override void Execute()
            {
                TargetManager.GetInstance().GetFriend( TargetNotoriety.Any, TargetBodyType.Any, TargetDistance.Next );
            }
        }
    }
}