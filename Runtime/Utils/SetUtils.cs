using System;

namespace CrazyPanda.UnityCore.NodeEditor
{
    static class SetUtils
    {
        public static void SetOnce<T>( this object ctx, ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        {
            if( !Equals( field, default( T ) ) )
                throw new InvalidOperationException( $"Value '{field}' already set for property '{caller}' in {ctx.GetType().Name}" );

            field = value;
        }
    }
}