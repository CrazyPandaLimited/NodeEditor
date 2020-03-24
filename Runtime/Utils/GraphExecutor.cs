using System;

namespace CrazyPanda.UnityCore.NodeEditor
{
    /// <summary>
    /// Helper class for executing graphs of types implementing <see cref="IExecutableGraphType{TArgs}"/>
    /// </summary>
    public static class GraphExecutor
    {
        /// <summary>
        /// Executes graph given its asset guid. May be called only from editor.
        /// </summary>
        /// <typeparam name="TArgs">Additional arguments type</typeparam>
        /// <param name="assetGuid"><see cref="AssetDatabase"/> GUID of a graph to execute</param>
        /// <param name="args">Additional arguments for the executor</param>
        /// <returns>Execution result</returns>
        public static IGraphExecutionResult ExecuteGraphAsset<TArgs>( string assetGuid, TArgs args )
        {
#if UNITY_EDITOR
            var graph = GraphSerializer.DeserializeFromGuid( assetGuid );
            return ExecuteGraph( graph, args );
#else
            throw new InvalidOperationException( $"{nameof(ExecuteGraphAsset)} may be called only from editor" );
#endif
        }

        /// <summary>
        /// Executes <see cref="GraphAsset"/>
        /// </summary>
        /// <typeparam name="TArgs">Additional arguments type</typeparam>
        /// <param name="asset">Graph asset to execute</param>
        /// <param name="args">Additional arguments for the executor</param>
        /// <returns>Execution result</returns>
        public static IGraphExecutionResult ExecuteGraphAsset<TArgs>( GraphAsset asset, TArgs args )
        {
            var graph = asset.Graph;
            return ExecuteGraph( graph, args );
        }

        /// <summary>
        /// Executes <see cref="GraphModel"/>
        /// </summary>
        /// <typeparam name="TArgs">Additional arguments type</typeparam>
        /// <param name="graph">Graph to execute</param>
        /// <param name="args">Additional arguments for the executor</param>
        /// <returns>Execution result</returns>
        public static IGraphExecutionResult ExecuteGraph<TArgs>( GraphModel graph, TArgs args )
        {
            var executor = graph.Type as IExecutableGraphType<TArgs>
                ?? throw new ArgumentException( $"Graph type '{graph.Type.Name}' is not executable" );

            return executor.Execute( graph, args );
        }
    }
}