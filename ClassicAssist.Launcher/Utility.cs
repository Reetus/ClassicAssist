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

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;

namespace ClassicAssist.Launcher
{
    public static class Utility
    {
        public static async Task<IPAddress> ResolveAddress( string hostname )
        {
            if ( IPAddress.TryParse( hostname, out IPAddress address ) )
            {
                return address;
            }

            IPHostEntry hostentry = await Dns.GetHostEntryAsync( hostname );

            return hostentry?.AddressList.FirstOrDefault( i => i.AddressFamily == AddressFamily.InterNetwork );
        }

        public static (bool netAssembly, string version) GetExeType( string path )
        {
            try
            {
                using ( FileStream stream = File.OpenRead( path ) )
                {
                    using ( PEReader peReader = new PEReader( stream ) )
                    {
                        if ( !peReader.HasMetadata )
                        {
                            return ( false, string.Empty );
                        }

                        MetadataReader metadataReader = peReader.GetMetadataReader();

                        string framework = GetTargetFramework( metadataReader );

                        return ( true, framework );
                    }
                }
            }
            catch ( Exception ex )
            {
                return ( false, string.Empty );
            }
        }

        private static string GetTargetFramework( MetadataReader reader )
        {
            foreach ( BlobReader valueBlob in from handle in reader.GetCustomAttributes( EntityHandle.AssemblyDefinition )
                     select reader.GetCustomAttribute( handle )
                     into attribute
                     let ctor = attribute.Constructor
                     let attrTypeName = GetAttributeTypeName( reader, ctor )
                     where attrTypeName == "System.Runtime.Versioning.TargetFrameworkAttribute"
                     select reader.GetBlobReader( attribute.Value ) )
            {
                // CustomAttributeValue: Prolog (2 bytes), then string
                valueBlob.ReadUInt16(); // skip prolog
                return valueBlob.ReadSerializedString();
            }

            return null;
        }

        private static string GetAttributeTypeName( MetadataReader reader, EntityHandle ctor )
        {
            if ( ctor.Kind != HandleKind.MemberReference )
            {
                return null;
            }

            MemberReference memberRef = reader.GetMemberReference( (MemberReferenceHandle) ctor );
            EntityHandle container = memberRef.Parent;

            if ( container.Kind != HandleKind.TypeReference )
            {
                return null;
            }

            TypeReference typeRef = reader.GetTypeReference( (TypeReferenceHandle) container );
            string ns = reader.GetString( typeRef.Namespace );
            string name = reader.GetString( typeRef.Name );
            return $"{ns}.{name}";
        }
    }
}