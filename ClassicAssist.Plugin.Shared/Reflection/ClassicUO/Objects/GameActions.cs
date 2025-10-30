using System;
using System.Reflection;

namespace ClassicAssist.Plugin.Shared.Reflection.ClassicUO.Objects
{
    public static class GameActions
    {
        private const string TYPE = "ClassicUO.Game.GameActions";
        private static Type _type;
        private static MethodInfo _usePrimaryMethod;
        private static MethodInfo _useSecondaryMethod;

        public static bool UsePrimaryAbility()
        {
            if ( _type == null )
            {
                _type = ReflectionImpl.DefaultAssembly?.GetType( TYPE );
            }

            if ( _usePrimaryMethod == null )
            {
                _usePrimaryMethod = _type?.GetMethod( "UsePrimaryAbility", BindingFlags.Public | BindingFlags.Static );
            }

            if ( _usePrimaryMethod == null )
            {
                return false;
            }

            ReflectionImpl.TickWorkQueue.Enqueue( () => { _usePrimaryMethod.Invoke( null, null ); } );

            return true;
        }

        public static bool UseSecondaryAbility()
        {
            if ( _type == null )
            {
                _type = ReflectionImpl.DefaultAssembly?.GetType( TYPE );
            }

            if ( _useSecondaryMethod == null )
            {
                _useSecondaryMethod =
                    _type?.GetMethod( "UseSecondaryAbility", BindingFlags.Public | BindingFlags.Static );
            }

            if ( _useSecondaryMethod == null )
            {
                return false;
            }

            ReflectionImpl.TickWorkQueue.Enqueue( () => { _useSecondaryMethod.Invoke( null, null ); } );

            return true;
        }
    }
}