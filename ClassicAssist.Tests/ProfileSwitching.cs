using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ClassicAssist.Data;
using ClassicAssist.Data.Hotkeys;
using ClassicAssist.Shared.Resources;
using ClassicAssist.UI.ViewModels;
using ClassicAssist.UO.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.Tests
{
    [TestClass]
    public class ProfileSwitching
    {
        [TestMethod]
        public void WontDuplicateSkillsCategory()
        {
            AppDomain appDomain = AppDomain.CreateDomain( "WontThrowExceptionOnDeserializeNullConfig",
                AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.SetupInformation );

            appDomain.DoCallBack( () =>
            {
                const string localPath = @"C:\Users\johns\Desktop\KvG Client 2.0";

                if ( !Directory.Exists( localPath ) )
                {
                    Debug.WriteLine( "Not running test, requires Cliloc.enu" );
                    return;
                }

                SkillsTabViewModel vm = new SkillsTabViewModel();

                Options options = Options.CurrentOptions;

                Skills.Initialize( localPath );

                JObject json = new JObject
                {
                    {
                        "Skills",
                        new JArray
                        {
                            new JObject
                            {
                                { "Name", "Hiding" },
                                {
                                    "Keys", new JObject { { "Keys", 94 }, { "SDLModifier", 0 }, { "Mouse", 7 } }
                                },
                                { "PassToUO", false }
                            }
                        }
                    }
                };

                vm.Deserialize( json, options );
                vm.Deserialize( json, options );

                HotkeyManager hotkeys = HotkeyManager.GetInstance();

                int count = hotkeys.Items.Count( hk => hk.IsCategory && hk.Name == Strings.Skills );

                Assert.AreEqual( 1, count );
            } );
        }
    }
}