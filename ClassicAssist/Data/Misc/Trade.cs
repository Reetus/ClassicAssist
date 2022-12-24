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

using ClassicAssist.UO.Data;

namespace ClassicAssist.Data.Misc
{
    public class Trade
    {
        public int AcceptLocal { get; set; }
        public int AcceptRemote { get; set; }
        public TradeAction Action { get; set; }
        public int ContainerLocal { get; set; }
        public int ContainerRemote { get; set; }
        public int GoldLocal { get; set; }
        public int GoldRemote { get; set; }
        public int PlatinumLocal { get; set; }
        public int PlatinumRemote { get; set; }
        public int Serial { get; set; }
    }
}