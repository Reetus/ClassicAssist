#region License

// Copyright (C) 2021 Reetus
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CG.Web.MegaApiClient;
using ClassicAssist.Shared.Resources;

namespace ClassicAssist.Data.Backup.Mega
{
    public static class MegaClient
    {
        private static MegaApiClient _client;
        private static IEnumerable<INode> _nodes;

        public static async Task<MegaApiClient> GetClient( MegaApiClient.LogonSessionToken authentication )
        {
            if ( _client != null )
            {
                return _client;
            }

            _client = new MegaApiClient();
            await _client.LoginAsync(
                new MegaApiClient.LogonSessionToken( authentication.SessionId, authentication.MasterKey ) );

            return _client;
        }

        public static async Task<List<INode>> GetAllNodes( MegaApiClient client, bool forceRefresh = false )
        {
            if ( _nodes != null && !forceRefresh )
            {
                return _nodes.ToList();
            }

            _nodes = await client.GetNodesAsync();

            return _nodes.ToList();
        }

        public static async Task<INode> GetNode( MegaApiClient client, string name, INodeInfo parent )
        {
            IEnumerable<INode> nodes = ( await GetAllNodes( client ) ).Where( e => e.ParentId == parent.Id );

            return nodes.FirstOrDefault( e => e.Name == name );
        }

        public static async Task<INode> GetRootNode( MegaApiClient client )
        {
            return ( await GetAllNodes( client ) ).FirstOrDefault( e => e.Type == NodeType.Root );
        }

        public static async Task<INode> GetDirectoryById( MegaApiClient client, string name )
        {
            List<INode> nodes = await GetAllNodes( client );

            try
            {
                return nodes.First( e => e.Id == name && e.Type == NodeType.Directory );
            }
            catch ( Exception e )
            {
                throw new Exception( Strings.Cannot_locate_backup_folder, e );
            }
        }
    }
}