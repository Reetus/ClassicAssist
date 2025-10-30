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
using ClassicAssist.Data.Macros.Commands;
using Sentry;

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

            if ( ReflectionCommands.HasConnectionLostGump() )
            {
                ReflectionCommands.Logout();
            }
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