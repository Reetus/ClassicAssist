using System.ComponentModel;
using ClassicAssist.Data;
using ClassicAssist.Data.Macros.Commands;
using ClassicAssist.Misc;
using Newtonsoft.Json.Linq;

namespace ClassicAssist.UI.ViewModels
{
    public class OptionsTabViewModel : BaseViewModel, ISettingProvider
    {
        private Options _currentOptions;

        public Options CurrentOptions
        {
            get => _currentOptions;
            set => SetProperty( ref _currentOptions, value );
        }

        public void Serialize( JObject json )
        {
            JObject options = new JObject();

            JObject useOnce = new JObject { ["Persist"] = CurrentOptions.PersistUseOnce };

            if ( CurrentOptions.PersistUseOnce )
            {
                JArray useOnceItems = new JArray();

                foreach ( int serial in ActionCommands.UseOnceList )
                {
                    useOnceItems.Add( serial );
                }

                useOnce.Add( "Items", useOnceItems );
            }

            options.Add( "UseOnce", useOnce );
            options.Add( "UseDeathScreenWhilstHidden", CurrentOptions.UseDeathScreenWhilstHidden );
            options.Add( "CommandPrefix", CurrentOptions.CommandPrefix );
            options.Add( "RangeCheckLastTarget", CurrentOptions.RangeCheckLastTarget );
            options.Add( "RangeCheckLastTargetAmount", CurrentOptions.RangeCheckLastTargetAmount );
            options.Add( "UseExperimentalFizzleDetection", CurrentOptions.UseExperimentalFizzleDetection );
            options.Add( "UseObjectQueue", CurrentOptions.UseObjectQueue );
            options.Add( "UseObjectQueueAmount", CurrentOptions.UseObjectQueueAmount );
            options.Add( "QueueLastTarget", CurrentOptions.QueueLastTarget );
            options.Add( "SmartTargetOption", CurrentOptions.SmartTargetOption.ToString() );
            options.Add( "LimitMouseWheelTrigger", CurrentOptions.LimitMouseWheelTrigger );
            options.Add( "LimitMouseWheelTriggerMS", CurrentOptions.LimitMouseWheelTriggerMS );

            json?.Add( "Options", options );
        }

        public void Deserialize( JObject json, Options options )
        {
            CurrentOptions = options;

            CurrentOptions.PropertyChanged += OnOptionsChanged;

            ActionCommands.UseOnceList.Clear();

            JToken config = json?["Options"];

            if ( config?["UseOnce"] != null )
            {
                CurrentOptions.PersistUseOnce = config["UseOnce"]["Persist"]?.ToObject<bool>() ?? false;

                if ( CurrentOptions.PersistUseOnce )
                {
                    foreach ( JToken token in config["UseOnce"]["Items"] )
                    {
                        ActionCommands.UseOnceList.Add( token.ToObject<int>() );
                    }
                }
            }

            CurrentOptions.UseDeathScreenWhilstHidden =
                config?["UseDeathScreenWhilstHidden"]?.ToObject<bool>() ?? false;
            CurrentOptions.CommandPrefix = config?["CommandPrefix"]?.ToObject<char>() ?? '=';
            CurrentOptions.RangeCheckLastTarget = config?["RangeCheckLastTarget"]?.ToObject<bool>() ?? false;
            CurrentOptions.RangeCheckLastTargetAmount = config?["RangeCheckLastTargetAmount"]?.ToObject<int>() ?? 11;

            CurrentOptions.UseExperimentalFizzleDetection =
                config?["UseExperimentalFizzleDetection"]?.ToObject<bool>() ?? false;

            CurrentOptions.UseObjectQueue = config?["UseObjectQueue"]?.ToObject<bool>() ?? false;
            CurrentOptions.UseObjectQueueAmount = config?["UseObjectQueueAmount"]?.ToObject<int>() ?? 5;
            CurrentOptions.QueueLastTarget = config?["QueueLastTarget"]?.ToObject<bool>() ?? false;
            CurrentOptions.SmartTargetOption =
                config?["SmartTargetOption"]?.ToObject<SmartTargetOption>() ?? SmartTargetOption.None;
            CurrentOptions.LimitMouseWheelTrigger = config?["LimitMouseWheelTrigger"]?.ToObject<bool>() ?? false;
            CurrentOptions.LimitMouseWheelTriggerMS = config?["LimitMouseWheelTriggerMS"]?.ToObject<int>() ?? 25;
        }

        // Replay CurrentOptions changes onto Options.CurrentOptions
        // TODO: Fix Options
        private void OnOptionsChanged( object sender, PropertyChangedEventArgs args )
        {
            object val = CurrentOptions.GetType().GetProperty( args.PropertyName )?.GetValue( CurrentOptions );

            if ( val == null )
            {
                return;
            }

            object oldVal = Options.CurrentOptions.GetType().GetProperty( args.PropertyName )
                ?.GetValue( Options.CurrentOptions );

            if ( !val.Equals( oldVal ) )
            {
                Options.CurrentOptions.GetType().GetProperty( args.PropertyName )
                    ?.SetValue( Options.CurrentOptions, val );
            }
        }
    }
}