using System.Collections;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.Friends;
using ClassicAssist.Data.Macros;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;
using ClassicAssist.UO.Network.PacketFilter;
using ClassicAssist.UO.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests.MacroCommands
{
    [TestClass]
    public class TargetCommandTests
    {
        [TestMethod]
        public void WillTargetTileRelativeSelf()
        {
            Engine.Player = new PlayerMobile( 0x01 ) { X = 100, Y = 100, Direction = Direction.East };

            AutoResetEvent are = new AutoResetEvent( false );

            void OnInternalPacketSentEvent( byte[] data, int length )
            {
                if ( data[0] != 0x6C )
                {
                    Assert.Fail();
                }

                int x = ( data[11] << 8 ) | data[12];
                //int y = ( data[13] << 8 ) | data[14];

                if ( x != 101 )
                {
                    Assert.Fail();
                }

                are.Set();
            }

            Engine.InternalPacketSentEvent += OnInternalPacketSentEvent;

            AliasCommands.SetAlias( "self", 0x01 );

            Engine.TargetType = TargetType.Tile;
            TargetCommands.TargetTileRelative( "self", 1 );

            bool result = are.WaitOne( 5000 );

            Assert.IsTrue( result );

            Engine.InternalPacketSentEvent -= OnInternalPacketSentEvent;
            Engine.Player = null;
        }

        [TestMethod]
        public void WillTargetTileRelativeSelfRunning()
        {
            Engine.Player = new PlayerMobile( 0x01 )
            {
                X = 100, Y = 100, Direction = (Direction) ( (int) Direction.East | 0x80 )
            };

            AutoResetEvent are = new AutoResetEvent( false );

            void OnInternalPacketSentEvent( byte[] data, int length )
            {
                if ( data[0] != 0x6C )
                {
                    Assert.Fail();
                }

                int x = ( data[11] << 8 ) | data[12];
                //int y = ( data[ 13 ] << 8 ) | data[ 14 ];

                if ( x != 101 )
                {
                    Assert.Fail();
                }

                are.Set();
            }

            Engine.InternalPacketSentEvent += OnInternalPacketSentEvent;

            AliasCommands.SetAlias( "self", 0x01 );

            Engine.TargetType = TargetType.Tile;
            TargetCommands.TargetTileRelative( "self", 1 );

            bool result = are.WaitOne( 5000 );

            Assert.IsTrue( result );

            Engine.InternalPacketSentEvent -= OnInternalPacketSentEvent;
            Engine.Player = null;
        }

        [TestMethod]
        public void WillSetLastTargetSerialOnInternalTargetSent()
        {
            Engine.Player = new PlayerMobile( 0x01 );

            Assert.AreEqual( 0, Engine.Player.LastTargetSerial );

            TargetCommands.Target( 0x00aabbcc );

            Assert.AreEqual( 0x00aabbcc, Engine.Player.LastTargetSerial );

            Engine.Player = null;
        }

        [TestMethod]
        public void WillKeepLastBetweenCasts()
        {
            Engine.Player = new PlayerMobile( 0x01 );
            Engine.PacketWaitEntries = new PacketWaitEntries();

            AutoResetEvent are = new AutoResetEvent( false );

            void OnWaitEntryAddedEvent( PacketWaitEntry entry )
            {
                PacketWriter target = new PacketWriter( 0x19 );
                target.Write( (byte) 0x6C );
                target.Fill();

                Engine.SendPacketToClient( target );
                Engine.PacketWaitEntries.CheckWait( target.ToArray(), PacketDirection.Incoming );
            }

            Engine.PacketWaitEntries.WaitEntryAddedEvent += OnWaitEntryAddedEvent;

            void OnInternalPacketSentEvent( byte[] data, int length )
            {
                if ( data[0] != 0x6C )
                {
                    return;
                }

                int serial = ( data[7] << 24 ) | ( data[8] << 16 ) | ( data[9] << 8 ) | data[10];

                if ( serial != 0x00aabbcc )
                {
                    Assert.Fail();
                }

                are.Set();
            }

            Engine.InternalPacketSentEvent += OnInternalPacketSentEvent;

            Task.Run( () => SpellCommands.Cast( "Explosion", 0x00aabbcc ) );

            bool result = are.WaitOne( 60000 );

            Assert.AreEqual( 0x00aabbcc, AliasCommands.GetAlias( "last" ) );

            Assert.IsTrue( result );

            Task.Run( () => SpellCommands.Cast( "Explosion", "last" ) );

            result = are.WaitOne( 60000 );

            Assert.AreEqual( 0x00aabbcc, AliasCommands.GetAlias( "last" ) );

            Assert.IsTrue( result );

            Assert.AreEqual( 0x00aabbcc, Engine.Player.LastTargetSerial );

            Engine.PacketWaitEntries.WaitEntryAddedEvent -= OnWaitEntryAddedEvent;
            Engine.InternalPacketSentEvent -= OnInternalPacketSentEvent;
            Engine.Player = null;
        }

        //TODO more robust testing
        [TestMethod]
        public void WillGetFriend()
        {
            Engine.Player = new PlayerMobile( 0x01 );

            Mobile[] mobiles =
            {
                new Mobile( 0x02 ) { Notoriety = Notoriety.Innocent },
                new Mobile( 0x03 ) { Notoriety = Notoriety.Ally },
                new Mobile( 0x04 ) { Notoriety = Notoriety.Murderer },
                new Mobile( 0x05 ) { Notoriety = Notoriety.Criminal }
            };

            Engine.Mobiles.Add( mobiles );

            TargetCommands.GetFriend( new[] { "Innocent" } );

            Assert.AreEqual( 0x02, AliasCommands.GetAlias( "friend" ) );

            TargetCommands.GetFriend( new[] { "Friend" } );

            Assert.AreEqual( 0x03, AliasCommands.GetAlias( "friend" ) );

            TargetCommands.GetFriend( new[] { "Murderer" } );

            Assert.AreEqual( 0x04, AliasCommands.GetAlias( "friend" ) );

            TargetCommands.GetFriend( new[] { "Innocent", "Friend", "Murderer" } );

            int friend = AliasCommands.GetAlias( "friend" );

            Assert.IsTrue( ( (IList) new[] { 0x02, 0x03, 0x04 } ).Contains( friend ) );

            Engine.Mobiles.Remove( mobiles );
            Engine.Player = null;
        }

        [TestMethod]
        public void WillOnlyGetClosestFriend()
        {
            Engine.Player = new PlayerMobile( 0x01 );

            Mobile mate = new Mobile( 0x02 ) { X = 5, Y = 5, Notoriety = Notoriety.Murderer };
            Mobile random = new Mobile( 0x03 ) { X = 1, Y = 1, Notoriety = Notoriety.Murderer };

            Engine.Mobiles.Add( mate );
            Engine.Mobiles.Add( random );

            Options.CurrentOptions.Friends.Add( new FriendEntry { Name = "Mate", Serial = mate.Serial } );

            bool result = TargetCommands.GetFriend( new[] { "Murderer" }, "Any", "Closest" );

            Assert.IsTrue( result );

            Assert.AreEqual( random.Serial, AliasCommands.GetAlias( "friend" ) );

            // Will get mate even though he is further away
            result = TargetCommands.GetFriendListOnly( "Closest" );

            Assert.IsTrue( result );

            Assert.AreEqual( mate.Serial, AliasCommands.GetAlias( "friend" ) );

            Options.CurrentOptions.Friends.Clear();
            Engine.Mobiles.Clear();
            Engine.Player = null;
        }

        [TestMethod]
        public void WontGetEnemyFriendClosest()
        {
            Engine.Player = new PlayerMobile( 0x01 );

            Mobile mate = new Mobile( 0x02 ) { X = 1, Y = 1, Notoriety = Notoriety.Murderer };
            Mobile random = new Mobile( 0x03 ) { X = 5, Y = 5, Notoriety = Notoriety.Murderer };

            Engine.Mobiles.Add( mate );
            Engine.Mobiles.Add( random );

            Options.CurrentOptions.Friends.Add( new FriendEntry { Name = "Mate", Serial = mate.Serial } );

            // Won't get mate even though he is closest
            bool result = TargetCommands.GetEnemy( new[] { "Murderer" }, "Any", "Closest" );

            Assert.IsTrue( result );

            Assert.AreEqual( random.Serial, AliasCommands.GetAlias( "enemy" ) );

            Options.CurrentOptions.Friends.Clear();
            Engine.Mobiles.Clear();
            Engine.Player = null;
        }

        [TestMethod]
        public void WontGetEnemyFriendNext()
        {
            Engine.Player = new PlayerMobile( 0x01 );

            Mobile mate = new Mobile( 0x02 ) { X = 1, Y = 1, Notoriety = Notoriety.Murderer };

            Engine.Mobiles.Add( mate );

            // Will get mate because he's not in the friends list yet
            bool result = TargetCommands.GetEnemy( new[] { "Murderer" } );

            Assert.IsTrue( result );
            Assert.AreEqual( mate.Serial, AliasCommands.GetAlias( "enemy" ) );

            Options.CurrentOptions.Friends.Add( new FriendEntry { Name = "Mate", Serial = mate.Serial } );

            // Won't get a result because mate is in the friends list
            result = TargetCommands.GetEnemy( new[] { "Murderer" } );

            Assert.IsFalse( result );

            Options.CurrentOptions.Friends.Clear();
            Engine.Mobiles.Clear();
            Engine.Player = null;
        }

        [TestMethod]
        public void WillGetEnemyClosestBodyType()
        {
            Engine.Player = new PlayerMobile( 0x01 );

            Mobile any = new Mobile( 0x02 ) { ID = 1, X = 1, Y = 1, Notoriety = Notoriety.Murderer };
            Mobile humanoid = new Mobile( 0x03 ) { ID = 400, X = 2, Y = 2, Notoriety = Notoriety.Murderer };
            Mobile transformation = new Mobile( 0x04 ) { ID = 748, X = 3, Y = 3, Notoriety = Notoriety.Murderer };

            Engine.Mobiles.Add( new[] { any, humanoid, transformation } );

            TargetCommands.GetEnemy( new[] { "Murderer" }, "Any", "Closest" );
            Assert.AreEqual( any.Serial, AliasCommands.GetAlias( "enemy" ) );

            TargetCommands.GetEnemy( new[] { "Murderer" }, "Humanoid", "Closest" );
            Assert.AreEqual( humanoid.Serial, AliasCommands.GetAlias( "enemy" ) );

            TargetCommands.GetEnemy( new[] { "Murderer" }, "Transformation", "Closest" );
            Assert.AreEqual( transformation.Serial, AliasCommands.GetAlias( "enemy" ) );

            TargetCommands.GetEnemy( new[] { "Murderer" }, "Both", "Closest" );
            Assert.AreEqual( humanoid.Serial, AliasCommands.GetAlias( "enemy" ) );

            Engine.Player = null;
        }

        [TestMethod]
        public void WillTargetType()
        {
            Engine.Player = new PlayerMobile( 0x01 );
            Item backpack =
                new Item( 0x40000000, Engine.Player.Serial ) { Container = new ItemCollection( 0x40000000 ) };
            Engine.Items.Add( backpack );
            Engine.Player.SetLayer( Layer.Backpack, backpack.Serial );

            Item item = new Item( 0x40000001, backpack.Serial ) { ID = 0xfeef };
            backpack.Container.Add( item );

            AutoResetEvent are = new AutoResetEvent( false );

            void PassOnTargetSent( byte[] data, int length )
            {
                if ( data[0] == 0x6C )
                {
                    are.Set();
                }
            }

            Engine.InternalPacketSentEvent += PassOnTargetSent;

            TargetCommands.TargetType( item.ID );

            bool result = are.WaitOne( 5000 );

            Assert.IsTrue( result );

            Engine.InternalPacketSentEvent -= PassOnTargetSent;
            Engine.Items.Clear();
            Engine.Player = null;
        }

        [TestMethod]
        public void WillTargetTypeLessThanSearchLevel()
        {
            Engine.Player = new PlayerMobile( 0x01 );
            Item backpack =
                new Item( 0x40000000, Engine.Player.Serial ) { Container = new ItemCollection( 0x40000000 ) };
            Engine.Items.Add( backpack );
            Engine.Player.SetLayer( Layer.Backpack, backpack.Serial );

            ItemCollection container = backpack.Container;

            for ( int i = 0; i < 5; i++ )
            {
                Item subitem = new Item( container.Serial + 1, container.Serial )
                {
                    Container = new ItemCollection( container.Serial + 1 ), Owner = container.Serial
                };

                container.Add( subitem );

                container = subitem.Container;
            }

            Item item = new Item( container.Serial + 1, container.Serial ) { ID = 0xfeef };
            container.Add( item );

            AutoResetEvent are = new AutoResetEvent( false );

            void PassOnTargetSent( byte[] data, int length )
            {
                if ( data[0] == 0x6C )
                {
                    are.Set();
                }
            }

            Engine.InternalPacketSentEvent += PassOnTargetSent;

            TargetCommands.TargetType( item.ID, -1, 5 );

            bool result = are.WaitOne( 5000 );

            Assert.IsTrue( result );

            Engine.InternalPacketSentEvent -= PassOnTargetSent;
            Engine.Items.Clear();
            Engine.Player = null;
        }

        [TestMethod]
        public void WontTargetTypeGreaterThanSearchLevel()
        {
            Engine.Player = new PlayerMobile( 0x01 );
            Item backpack =
                new Item( 0x40000000, Engine.Player.Serial ) { Container = new ItemCollection( 0x40000000 ) };
            Engine.Items.Add( backpack );
            Engine.Player.SetLayer( Layer.Backpack, backpack.Serial );

            ItemCollection container = backpack.Container;

            for ( int i = 0; i < 5; i++ )
            {
                Item subitem = new Item( container.Serial + 1, container.Serial )
                {
                    Container = new ItemCollection( container.Serial + 1 ), Owner = container.Serial
                };

                container.Add( subitem );

                container = subitem.Container;
            }

            Item item = new Item( container.Serial + 1, container.Serial ) { ID = 0xfeef };
            container.Add( item );

            void FailOnTargetSent( byte[] data, int length )
            {
                if ( data[0] == 0x6C )
                {
                    Assert.Fail();
                }
            }

            Engine.InternalPacketSentEvent += FailOnTargetSent;

            TargetCommands.TargetType( item.ID, -1, 4 );

            Engine.InternalPacketSentEvent -= FailOnTargetSent;
            Engine.Items.Clear();
            Engine.Player = null;
        }

        [TestMethod]
        public void WillTargetGround()
        {
            Engine.Player = new PlayerMobile( 0x01 );

            Mobile mobile = new Mobile( 0x02 ) { ID = 0x191 };
            Engine.Mobiles.Add( mobile );

            AutoResetEvent are = new AutoResetEvent( false );

            void PassOnTargetSent( byte[] data, int length )
            {
                if ( data[0] == 0x6C )
                {
                    are.Set();
                }
            }

            Engine.InternalPacketSentEvent += PassOnTargetSent;

            TargetCommands.TargetGround( mobile.ID );

            bool result = are.WaitOne( 5000 );

            Assert.IsTrue( result );

            Engine.InternalPacketSentEvent -= PassOnTargetSent;
            Engine.Mobiles.Clear();
            Engine.Player = null;
        }

        [TestMethod]
        public void WillGetLowestEnemy()
        {
            Engine.Player = new PlayerMobile( 0x01 );

            for ( int i = 2; i < 10; i++ )
            {
                Engine.Mobiles.Add( new Mobile( i )
                {
                    Notoriety = Notoriety.Criminal,
                    Hits = 25,
                    HitsMax = 25,
                    X = i,
                    Y = i
                } );
            }

            Engine.Mobiles.GetMobile( 7 ).Hits = 10;

            TargetCommands.GetEnemy( new[] { "Criminal" }, "Any", "Closest", "Lowest" );

            Assert.AreEqual( 7, AliasCommands.GetAlias( "enemy" ) );

            Engine.Mobiles.Clear();
            Engine.Player = null;
        }

        [TestMethod]
        public void WillGetParalyzedEnemy()
        {
            Engine.Player = new PlayerMobile( 0x01 );

            for ( int i = 2; i < 10; i++ )
            {
                Engine.Mobiles.Add( new Mobile( i )
                {
                    Notoriety = Notoriety.Criminal,
                    Hits = 25,
                    HitsMax = 25,
                    X = i,
                    Y = i
                } );
            }

            Engine.Mobiles.GetMobile( 7 ).Status |= MobileStatus.Frozen;

            TargetCommands.GetEnemy( new[] { "Criminal" }, "Any", "Closest", "Paralyzed" );

            Assert.AreEqual( 7, AliasCommands.GetAlias( "enemy" ) );

            Engine.Mobiles.Clear();
            Engine.Player = null;
        }

        [TestMethod]
        public void WillGetPoisonedEnemy()
        {
            Engine.Player = new PlayerMobile( 0x01 );

            for ( int i = 2; i < 10; i++ )
            {
                Engine.Mobiles.Add( new Mobile( i )
                {
                    Notoriety = Notoriety.Criminal,
                    Hits = 25,
                    HitsMax = 25,
                    X = i,
                    Y = i
                } );
            }

            Engine.Mobiles.GetMobile( 7 ).HealthbarColour |= HealthbarColour.Green;

            TargetCommands.GetEnemy( new[] { "Criminal" }, "Any", "Closest", "Poisoned" );

            Assert.AreEqual( 7, AliasCommands.GetAlias( "enemy" ) );

            Engine.Mobiles.Clear();
            Engine.Player = null;
        }

        [TestMethod]
        public void WillGetMortaledEnemy()
        {
            Engine.Player = new PlayerMobile( 0x01 );

            for ( int i = 2; i < 10; i++ )
            {
                Engine.Mobiles.Add( new Mobile( i )
                {
                    Notoriety = Notoriety.Criminal,
                    Hits = 25,
                    HitsMax = 25,
                    X = i,
                    Y = i
                } );
            }

            Engine.Mobiles.GetMobile( 7 ).HealthbarColour |= HealthbarColour.Yellow;

            TargetCommands.GetEnemy( new[] { "Criminal" }, "Any", "Closest", "Mortaled" );

            Assert.AreEqual( 7, AliasCommands.GetAlias( "enemy" ) );

            Engine.Mobiles.Clear();
            Engine.Player = null;
        }

        [TestMethod]
        public void WillGetUnmounted()
        {
            Engine.Player = new PlayerMobile( 0x01 );

            for ( int i = 2; i < 10; i++ )
            {
                Mobile m = new Mobile( i )
                {
                    Notoriety = Notoriety.Criminal,
                    Hits = 25,
                    HitsMax = 25,
                    X = i,
                    Y = i
                };

                m.SetLayer( Layer.Mount, 1 );
                Engine.Mobiles.Add( m );
            }

            Engine.Mobiles.GetMobile( 7 ).SetLayer( Layer.Mount, 0 );

            TargetCommands.GetEnemy( new[] { "Criminal" }, "Any", "Closest", "Unmounted" );

            Assert.AreEqual( 7, AliasCommands.GetAlias( "enemy" ) );

            TargetCommands.GetFriend( new[] { "Criminal" }, "Any", "Closest", "Unmounted" );

            Assert.AreEqual( 7, AliasCommands.GetAlias( "friend" ) );

            Engine.Mobiles.Clear();
            Engine.Player = null;
        }

        [TestMethod]
        public void WillGetUnmountedFriendOnly()
        {
            Engine.Player = new PlayerMobile( 0x01 );
            Options.CurrentOptions.Friends = new ObservableCollection<FriendEntry>();

            for ( int i = 2; i < 10; i++ )
            {
                Mobile m = new Mobile( i )
                {
                    Notoriety = Notoriety.Criminal,
                    Hits = 25,
                    HitsMax = 25,
                    X = i,
                    Y = i
                };
                m.SetLayer( Layer.Mount, 1 );
                Engine.Mobiles.Add( m );
                Options.CurrentOptions.Friends.Add( new FriendEntry { Name = "Friend", Serial = i } );
            }

            Engine.Mobiles.GetMobile( 7 ).SetLayer( Layer.Mount, 0 );

            TargetCommands.GetFriendListOnly( "Closest", "Unmounted" );

            Assert.AreEqual( 7, AliasCommands.GetAlias( "friend" ) );

            Options.CurrentOptions.Friends.Clear();
            Engine.Mobiles.Clear();
            Engine.Player = null;
            AliasCommands.SetAlias( "friend", -1 );
        }

        [TestMethod]
        public void WillGetLowestFriendOnly()
        {
            Engine.Player = new PlayerMobile( 0x01 );
            Options.CurrentOptions.Friends = new ObservableCollection<FriendEntry>();

            for ( int i = 2; i < 10; i++ )
            {
                Engine.Mobiles.Add( new Mobile( i )
                {
                    Notoriety = Notoriety.Criminal,
                    Hits = 25,
                    HitsMax = 25,
                    X = i,
                    Y = i
                } );
                Options.CurrentOptions.Friends.Add( new FriendEntry { Name = "Friend", Serial = i } );
            }

            Engine.Mobiles.GetMobile( 7 ).Hits = 10;

            TargetCommands.GetFriendListOnly( "Closest", "Lowest" );

            Assert.AreEqual( 7, AliasCommands.GetAlias( "friend" ) );

            Options.CurrentOptions.Friends.Clear();
            Engine.Mobiles.Clear();
            Engine.Player = null;
            AliasCommands.SetAlias( "friend", -1 );
        }

        [TestMethod]
        public void WillGetPoisonedFriendOnly()
        {
            Engine.Player = new PlayerMobile( 0x01 );
            Options.CurrentOptions.Friends = new ObservableCollection<FriendEntry>();

            for ( int i = 2; i < 10; i++ )
            {
                Engine.Mobiles.Add( new Mobile( i )
                {
                    Notoriety = Notoriety.Criminal,
                    Hits = 25,
                    HitsMax = 25,
                    X = i,
                    Y = i
                } );
                Options.CurrentOptions.Friends.Add( new FriendEntry { Name = "Friend", Serial = i } );
            }

            Engine.Mobiles.GetMobile( 7 ).HealthbarColour |= HealthbarColour.Green;

            TargetCommands.GetFriendListOnly( "Closest", "Poisoned" );

            Assert.AreEqual( 7, AliasCommands.GetAlias( "friend" ) );

            Options.CurrentOptions.Friends.Clear();
            Engine.Mobiles.Clear();
            Engine.Player = null;
            AliasCommands.SetAlias( "friend", -1 );
        }

        [TestMethod]
        public void WillGetMortaledFriendOnly()
        {
            Engine.Player = new PlayerMobile( 0x01 );
            Options.CurrentOptions.Friends = new ObservableCollection<FriendEntry>();

            for ( int i = 2; i < 10; i++ )
            {
                Engine.Mobiles.Add( new Mobile( i )
                {
                    Notoriety = Notoriety.Criminal,
                    Hits = 25,
                    HitsMax = 25,
                    X = i,
                    Y = i
                } );
                Options.CurrentOptions.Friends.Add( new FriendEntry { Name = "Friend", Serial = i } );
            }

            Engine.Mobiles.GetMobile( 7 ).HealthbarColour |= HealthbarColour.Yellow;

            TargetCommands.GetFriendListOnly( "Closest", "Mortaled" );

            Assert.AreEqual( 7, AliasCommands.GetAlias( "friend" ) );

            Options.CurrentOptions.Friends.Clear();
            Engine.Mobiles.Clear();
            Engine.Player = null;
            AliasCommands.SetAlias( "friend", -1 );
        }

        [TestMethod]
        public void WillGetLowestFriend()
        {
            Engine.Player = new PlayerMobile( 0x01 );

            for ( int i = 2; i < 10; i++ )
            {
                Engine.Mobiles.Add( new Mobile( i )
                {
                    Notoriety = Notoriety.Ally,
                    Hits = 25,
                    HitsMax = 25,
                    X = i,
                    Y = i
                } );
            }

            Engine.Mobiles.GetMobile( 7 ).Hits = 10;

            TargetCommands.GetFriend( new[] { "Friend" }, "Any", "Closest", "Lowest" );

            Assert.AreEqual( 7, AliasCommands.GetAlias( "friend" ) );

            Engine.Mobiles.Clear();
            Engine.Player = null;
            AliasCommands.SetAlias( "friend", -1 );
        }

        [TestMethod]
        public void WillGetPoisonedFriend()
        {
            Engine.Player = new PlayerMobile( 0x01 );

            for ( int i = 2; i < 10; i++ )
            {
                Engine.Mobiles.Add( new Mobile( i )
                {
                    Notoriety = Notoriety.Ally,
                    Hits = 25,
                    HitsMax = 25,
                    X = i,
                    Y = i
                } );
            }

            Engine.Mobiles.GetMobile( 7 ).HealthbarColour |= HealthbarColour.Green;

            TargetCommands.GetFriend( new[] { "Friend" }, "Any", "Closest", "Poisoned" );

            Assert.AreEqual( 7, AliasCommands.GetAlias( "friend" ) );

            Engine.Mobiles.Clear();
            Engine.Player = null;
            AliasCommands.SetAlias( "friend", -1 );
        }

        [TestMethod]
        public void WillGetMortaledFriend()
        {
            Engine.Player = new PlayerMobile( 0x01 );

            for ( int i = 2; i < 10; i++ )
            {
                Engine.Mobiles.Add( new Mobile( i )
                {
                    Notoriety = Notoriety.Ally,
                    Hits = 25,
                    HitsMax = 25,
                    X = i,
                    Y = i
                } );
            }

            Engine.Mobiles.GetMobile( 7 ).HealthbarColour |= HealthbarColour.Yellow;

            TargetCommands.GetFriend( new[] { "Friend" }, "Any", "Closest", "Mortaled" );

            Assert.AreEqual( 7, AliasCommands.GetAlias( "friend" ) );

            Engine.Mobiles.Clear();
            Engine.Player = null;
            AliasCommands.SetAlias( "friend", -1 );
        }

        [TestMethod]
        public void WillGetNextLowestEnemy()
        {
            Engine.Player = new PlayerMobile( 0x01 );

            for ( int i = 2; i < 10; i++ )
            {
                Engine.Mobiles.Add( new Mobile( i )
                {
                    Notoriety = Notoriety.Criminal,
                    Hits = 25,
                    HitsMax = 25,
                    X = i,
                    Y = i
                } );
            }

            Engine.Mobiles.GetMobile( 7 ).Hits = 10;
            Engine.Mobiles.GetMobile( 8 ).Hits = 11;

            TargetCommands.GetEnemy( new[] { "Criminal" }, "Any", "Next", "Lowest" );

            Assert.AreEqual( 7, AliasCommands.GetAlias( "enemy" ) );

            TargetCommands.GetEnemy( new[] { "Criminal" }, "Any", "Next", "Lowest" );

            Assert.AreEqual( 8, AliasCommands.GetAlias( "enemy" ) );

            Engine.Mobiles.Clear();
            Engine.Player = null;
        }

        [TestMethod]
        public void WillUseIgnoreListIfEnabledGetClosestEnemy()
        {
            Options.CurrentOptions.GetFriendEnemyUsesIgnoreList = true;

            Engine.Player = new PlayerMobile( 0x01 );
            Mobile mobile = new Mobile( 0x02 ) { Notoriety = Notoriety.Murderer };
            Engine.Mobiles.Add( mobile );

            bool result = TargetCommands.GetEnemy( new[] { Notoriety.Murderer.ToString() }, "Any", "Closest" );

            Assert.IsTrue( result );
            Assert.AreEqual( mobile.Serial, AliasCommands.GetAlias( "enemy" ) );

            AliasCommands.UnsetAlias( "enemy" );
            ObjectCommands.IgnoreObject( mobile.Serial );

            result = TargetCommands.GetEnemy( new[] { Notoriety.Murderer.ToString() }, "Any", "Closest" );

            Assert.IsFalse( result );

            ObjectCommands.ClearIgnoreList();
            Engine.Mobiles.Clear();
            Engine.Player = null;
        }

        [TestMethod]
        public void WillUseIgnoreListIfEnabledGetNextEnemy()
        {
            Options.CurrentOptions.GetFriendEnemyUsesIgnoreList = true;

            Engine.Player = new PlayerMobile( 0x01 );
            Mobile mobile = new Mobile( 0x02 ) { Notoriety = Notoriety.Murderer };
            Engine.Mobiles.Add( mobile );

            bool result = TargetCommands.GetEnemy( new[] { Notoriety.Murderer.ToString() } );

            Assert.IsTrue( result );
            Assert.AreEqual( mobile.Serial, AliasCommands.GetAlias( "enemy" ) );

            AliasCommands.UnsetAlias( "enemy" );
            ObjectCommands.IgnoreObject( mobile.Serial );

            result = TargetCommands.GetEnemy( new[] { Notoriety.Murderer.ToString() } );

            Assert.IsFalse( result );

            ObjectCommands.ClearIgnoreList();
            Engine.Mobiles.Clear();
            Engine.Player = null;
        }

        [TestMethod]
        public void WillUseIgnoreListIfEnabledGetClosestFriend()
        {
            Options.CurrentOptions.GetFriendEnemyUsesIgnoreList = true;

            Engine.Player = new PlayerMobile( 0x01 );
            Mobile mobile = new Mobile( 0x02 ) { Notoriety = Notoriety.Murderer };
            Engine.Mobiles.Add( mobile );

            bool result = TargetCommands.GetFriend( new[] { Notoriety.Murderer.ToString() }, "Any", "Closest" );

            Assert.IsTrue( result );
            Assert.AreEqual( mobile.Serial, AliasCommands.GetAlias( "friend" ) );

            AliasCommands.UnsetAlias( "enemy" );
            ObjectCommands.IgnoreObject( mobile.Serial );

            result = TargetCommands.GetFriend( new[] { Notoriety.Murderer.ToString() }, "Any", "Closest" );

            Assert.IsFalse( result );

            ObjectCommands.ClearIgnoreList();
            Engine.Mobiles.Clear();
            Engine.Player = null;
            AliasCommands.SetAlias( "friend", -1 );
        }

        [TestMethod]
        public void WillUseIgnoreListIfEnabledGetNextFriend()
        {
            Options.CurrentOptions.GetFriendEnemyUsesIgnoreList = true;

            Engine.Player = new PlayerMobile( 0x01 );
            Mobile mobile = new Mobile( 0x02 ) { Notoriety = Notoriety.Murderer };
            Engine.Mobiles.Add( mobile );

            bool result = TargetCommands.GetFriend( new[] { Notoriety.Murderer.ToString() } );

            Assert.IsTrue( result );
            Assert.AreEqual( mobile.Serial, AliasCommands.GetAlias( "friend" ) );

            AliasCommands.UnsetAlias( "enemy" );
            ObjectCommands.IgnoreObject( mobile.Serial );

            result = TargetCommands.GetFriend( new[] { Notoriety.Murderer.ToString() } );

            Assert.IsFalse( result );

            ObjectCommands.ClearIgnoreList();
            Engine.Mobiles.Clear();
            Engine.Player = null;
            AliasCommands.SetAlias( "friend", -1 );
        }

        [TestMethod]
        public void WillUseIgnoreListIfEnabledGetClosestFriendOnly()
        {
            Options.CurrentOptions.GetFriendEnemyUsesIgnoreList = true;

            Engine.Player = new PlayerMobile( 0x01 );
            Mobile mobile = new Mobile( 0x02 ) { Notoriety = Notoriety.Murderer };
            Engine.Mobiles.Add( mobile );
            Options.CurrentOptions.Friends.Add( new FriendEntry { Name = "Friend", Serial = mobile.Serial } );

            bool result = TargetCommands.GetFriendListOnly( "Closest" );

            Assert.IsTrue( result );
            Assert.AreEqual( mobile.Serial, AliasCommands.GetAlias( "friend" ) );

            AliasCommands.UnsetAlias( "enemy" );
            ObjectCommands.IgnoreObject( mobile.Serial );

            result = TargetCommands.GetFriendListOnly( "Closest" );

            Assert.IsFalse( result );

            Options.CurrentOptions.Friends.Clear();
            ObjectCommands.ClearIgnoreList();
            Engine.Mobiles.Clear();
            Engine.Player = null;
            AliasCommands.SetAlias( "friend", -1 );
        }

        [TestMethod]
        public void WillUseIgnoreListIfEnabledGetNextFriendOnly()
        {
            Options.CurrentOptions.GetFriendEnemyUsesIgnoreList = true;

            Engine.Player = new PlayerMobile( 0x01 );
            Mobile mobile = new Mobile( 0x02 ) { Notoriety = Notoriety.Murderer };
            Engine.Mobiles.Add( mobile );
            Options.CurrentOptions.Friends.Add( new FriendEntry { Name = "Friend", Serial = mobile.Serial } );

            bool result = TargetCommands.GetFriendListOnly();

            Assert.IsTrue( result );
            Assert.AreEqual( mobile.Serial, AliasCommands.GetAlias( "friend" ) );

            AliasCommands.UnsetAlias( "enemy" );
            ObjectCommands.IgnoreObject( mobile.Serial );

            result = TargetCommands.GetFriendListOnly();

            Assert.IsFalse( result );

            Options.CurrentOptions.Friends.Clear();
            ObjectCommands.ClearIgnoreList();
            Engine.Mobiles.Clear();
            Engine.Player = null;
            AliasCommands.SetAlias( "friend", -1 );
        }

        [TestMethod]
        public void WontGetInvalidBodyGetFriendListOnlyTransformation()
        {
            Engine.Player = new PlayerMobile( 0x01 );
            Mobile mobile = new Mobile( 0x02 ) { Notoriety = Notoriety.Murderer, ID = 0x190 };
            Engine.Mobiles.Add( mobile );
            Options.CurrentOptions.Friends.Add( new FriendEntry { Name = "Friend", Serial = mobile.Serial } );

            bool result = TargetCommands.GetFriendListOnly( "Closest", "Any", "Humanoid" );

            Assert.IsTrue( result );
            Assert.AreEqual( mobile.Serial, AliasCommands.GetAlias( "friend" ) );

            result = TargetCommands.GetFriendListOnly( "Closest", "Any", "Transformation" );

            Assert.IsFalse( result );

            result = TargetCommands.GetFriendListOnly();

            Assert.IsTrue( result );
            Assert.AreEqual( mobile.Serial, AliasCommands.GetAlias( "friend" ) );

            Options.CurrentOptions.Friends.Clear();
            ObjectCommands.ClearIgnoreList();
            Engine.Mobiles.Clear();
            Engine.Player = null;
            AliasCommands.SetAlias( "friend", -1 );
        }

        [TestMethod]
        public void WillGetDeadFriend()
        {
            Engine.Player = new PlayerMobile( 0x01 );

            for ( int i = 2; i < 10; i++ )
            {
                Engine.Mobiles.Add( new Mobile( i )
                {
                    Notoriety = Notoriety.Ally,
                    Hits = 25,
                    HitsMax = 25,
                    X = i,
                    Y = i
                } );
            }

            Engine.Mobiles.GetMobile( 7 ).ID = 0x0192;

            TargetCommands.GetFriend( new[] { "Friend" }, "Any", "Closest", "Dead" );

            Assert.AreEqual( 7, AliasCommands.GetAlias( "friend" ) );

            Engine.Mobiles.Clear();
            Engine.Player = null;
            AliasCommands.SetAlias( "friend", -1 );
        }

        [TestMethod]
        public void WontGetDeadFriendLowest()
        {
            Engine.Player = new PlayerMobile( 0x01 );

            for ( int i = 2; i < 10; i++ )
            {
                Engine.Mobiles.Add( new Mobile( i )
                {
                    Notoriety = Notoriety.Ally,
                    Hits = 25,
                    HitsMax = 25,
                    X = i,
                    Y = i
                } );
            }

            Engine.Mobiles.GetMobile( 7 ).Hits = 0;
            Engine.Mobiles.GetMobile( 7 ).ID = 0x192;

            TargetCommands.GetFriend( new[] { "Friend" }, "Any", "Closest", "Lowest" );

            Assert.AreNotEqual( 7, AliasCommands.GetAlias( "friend" ) );

            Engine.Mobiles.Clear();
            Engine.Player = null;
            AliasCommands.SetAlias( "friend", -1 );
        }

        [TestMethod]
        public void WillGetMobileDistanceLimit()
        {
            Engine.Player = new PlayerMobile( 0x01 );
            IncomingPacketHandlers.Initialize();
            MacroManager.GetInstance().IsRecording = () => false;

            Mobile[] mobiles =
            {
                new Mobile( 0x02 ) { Notoriety = Notoriety.Innocent, X = 5, Y = 5 },
                new Mobile( 0x03 ) { Notoriety = Notoriety.Murderer, X = 5, Y = 5 }
            };

            Engine.Mobiles.Add( mobiles );

            bool result = TargetCommands.GetFriend( new[] { "Innocent" }, "Any", "Next", "Any", 5);

            Assert.AreEqual( 0x02, AliasCommands.GetAlias( "friend" ) );
            Assert.IsTrue( result );

            result = TargetCommands.GetFriend( new[] { "Innocent" }, "Any", "Next", "Any", 4 );

            Assert.IsFalse( result );

            Options.CurrentOptions.Friends.Add( new FriendEntry { Name = "Mate", Serial = 0x02 } );

            result = TargetCommands.GetFriendListOnly( "Next", "Any", "Any", 5 );

            Assert.AreEqual( 0x02, AliasCommands.GetAlias( "friend" ) );
            Assert.IsTrue( result );

            result = TargetCommands.GetFriendListOnly( "Next", "Any", "Any", 4 );

            Assert.IsFalse( result );

            result = TargetCommands.GetEnemy( new[] { "Murderer" }, "Any", "Next", "Any", 5 );

            Assert.AreEqual( 0x03, AliasCommands.GetAlias( "enemy" ) );
            Assert.IsTrue( result );

            result = TargetCommands.GetEnemy( new[] { "Murderer" }, "Any", "Next", "Any", 4 );

            Assert.IsFalse( result );


            Engine.Mobiles.Remove( mobiles );
            Engine.Player = null;
        }
    }
}