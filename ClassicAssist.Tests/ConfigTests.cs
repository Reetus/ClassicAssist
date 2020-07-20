using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ClassicAssist.Shared;
using ClassicAssist.Data;
using ClassicAssist.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Tests
{
    [TestClass]
    [Serializable]
    public class ConfigTests
    {
        private string _profilePath;

        [TestMethod]
        public void WontThrowExceptionOnDeserializeNullConfig()
        {
            AppDomain appDomain = AppDomain.CreateDomain( "WontThrowExceptionOnDeserializeNullConfig",
                AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.SetupInformation );

            appDomain.DoCallBack( () => TestConfig( null ) );
        }

        [TestMethod]
        public void WontThrowExceptionOnDeserializeEmptyConfig()
        {
            AppDomain appDomain = AppDomain.CreateDomain( "WontThrowExceptionOnDeserializeEmptyConfig",
                AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.SetupInformation );

            appDomain.DoCallBack( () => TestConfig( new JObject() ) );
        }

        public void TestConfig( JObject json )
        {
            string path = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );

            _profilePath = Path.Combine( path, "Profiles" );

            if ( File.Exists( Path.Combine( _profilePath, "settings.json" ) ) )
            {
                File.Delete( Path.Combine( _profilePath, "settings.json" ) );
            }

            IEnumerable<Type> allSettingProvider = Assembly.GetAssembly( typeof( Engine ) ).GetTypes()
                .Where( t => typeof( ISettingProvider ).IsAssignableFrom( t ) && t.IsClass );

            Options options = new Options();

            foreach ( Type type in allSettingProvider )
            {
                if ( !type.IsPublic )
                {
                    continue;
                }

                ISettingProvider p = (ISettingProvider) Activator.CreateInstance( type );

                p.Deserialize( json, options );

                p.Serialize( json );
            }

            if ( File.Exists( Path.Combine( _profilePath, "settings.json" ) ) )
            {
                File.Delete( Path.Combine( _profilePath, "settings.json" ) );
            }
        }
    }
}