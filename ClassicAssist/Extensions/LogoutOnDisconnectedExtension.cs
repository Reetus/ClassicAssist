#region License

// Copyright (C) 2022 Reetus
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Threading;
using Assistant;
using ClassicAssist.Data;
using ClassicAssist.Data.ClassicUO.Objects.Gumps;
using ClassicAssist.Data.Macros.Commands;
using Sentry;
using CUO = ClassicAssist.Data.ClassicUO.Gumps;

namespace ClassicAssist.Extensions
{
    public class LogoutOnDisconnectedExtension : IExtension
    {
        private DispatcherTimer _timer;

        public void Initialize()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds( 60 ) };
            _timer.Tick += TimerOnTick;
            Engine.ConnectedEvent += EngineOnConnectedEvent;
            Engine.DisconnectedEvent += EngineOnDisconnectedEvent;
        }

        private static void TimerOnTick( object sender, EventArgs e )
        {
            if ( !Options.CurrentOptions.LogoutDisconnectedPrompt )
            {
                return;
            }

            Engine.TickWorkQueue.Enqueue( () =>
            {
                try
                {
                    IEnumerable<dynamic> gumps = CUO.GetGumps();

                    dynamic obj = gumps.FirstOrDefault( g => g.GetType().ToString().Contains( "MessageBoxGump" ) );

                    if ( obj == null )
                    {
                        return;
                    }

                    MessageBoxGump messageBox = new MessageBoxGump( obj );

                    dynamic label = messageBox.Children.FirstOrDefault(
                        ele => ele.GetType().ToString().Contains( "Label" ) );

                    dynamic button = messageBox.Children.FirstOrDefault(
                        ele => ele.GetType().ToString().Contains( "Button" ) );

                    if ( label == null || button == null )
                    {
                        return;
                    }

                    dynamic textProperty = label.GetType()
                        .GetProperty( "Text", BindingFlags.Instance | BindingFlags.Public );

                    dynamic textValue = textProperty.GetValue( label, null );

                    // TODO: resources
                    if ( !textValue.Contains( "Connection lost:" ) )
                    {
                        return;
                    }

                    //TODO: actually click the button
                    MainCommands.Logout();
                }
                catch ( Exception ex )
                {
                    Console.WriteLine( ex.ToString() );

                    SentrySdk.CaptureException( ex );
                }
            } );
        }

        private void EngineOnDisconnectedEvent()
        {
            _timer.Stop();
        }

        private void EngineOnConnectedEvent()
        {
            _timer.Start();
        }
    }
}