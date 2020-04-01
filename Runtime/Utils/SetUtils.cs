using System;

namespace CrazyPanda.UnityCore.NodeEditor
{
    static class SetUtils
    {
        /// <summary>
        /// Sets <paramref name="value"/> to <paramref name="field"/> if it has default value. If not - throws
        /// </summary>
        /// <typeparam name="T">Type of <paramref name="field"/> and <paramref name="value"/></typeparam>
        /// <param name="ctx"><paramref name="field"/> owner</param>
        /// <param name="field">Where to set <paramref name="value"/></param>
        /// <param name="value">Value to set</param>
        /// <param name="caller">Caller member name</param>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="field"/> has non default value</exception>
        public static void SetOnce<T>( this object ctx, ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string caller = null )
        {
            if( !Equals( field, default( T ) ) )
                throw new InvalidOperationException( $"Value '{field}' already set for property '{caller}' in {ctx.GetType().Name}" );

            field = value;
        }

        /// <summary>
        /// Sets <paramref name="value"/> to <paramref name="field"/>. Allows changes NotNull -> Null, Null -> NotNull, Null -> Null
        /// </summary>
        /// <typeparam name="T">Type of <paramref name="field"/> and <paramref name="value"/></typeparam>
        /// <param name="ctx"><paramref name="field"/> owner</param>
        /// <param name="field">Where to set <paramref name="value"/></param>
        /// <param name="value">Value to set</param>
        /// <param name="caller">Caller member name</param>
        /// <exception cref="InvalidOperationException">Thrown when both <paramref name="field"/> and <paramref name="value"/> is not null</exception>
        public static void SetOnceOrNull<T>( this object ctx, ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string caller = null )
            where T : class
        {
            if( field != null && value != null )
                throw new InvalidOperationException( $"Cannot change property '{caller}' from '{field}' to '{value}' in {ctx.GetType().Name}" );

            field = value;
        }
    }
}