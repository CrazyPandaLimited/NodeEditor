using System;
using UnityEditor;

namespace CrazyPanda.UnityCore.NodeEditor
{
    /// <summary>
    /// Helper class for executing graphs of types implementing <see cref="IExecutableGraphType"/>
    /// </summary>
    public static class GraphExecutor
    {
        /// <summary>
        /// Executes graph given its asset guid
        /// </summary>
        /// <param name="assetGuid"><see cref="AssetDatabase"/> GUID of a graph to execute</param>
        /// <param name="targetPlatform">Target platform to use. Uses <see cref="EditorUserBuildSettings.activeBuildTarget"/> if not set</param>
        /// <returns>Execution result</returns>
        public static IGraphExecutionResult ExecuteGraphAsset(string assetGuid, BuildTarget targetPlatform = BuildTarget.NoTarget)
        {
            var graph = GraphSerializer.DeserializeFromGuid(assetGuid);
            return ExecuteGraph(graph, targetPlatform);
        }

        /// <summary>
        /// Executes <see cref="GraphAsset"/>
        /// </summary>
        /// <param name="asset">Graph asset to execute</param>
        /// <param name="targetPlatform">Target platform to use. Uses <see cref="EditorUserBuildSettings.activeBuildTarget"/> if not set</param>
        /// <returns>Execution result</returns>
        public static IGraphExecutionResult ExecuteGraphAsset(GraphAsset asset, BuildTarget targetPlatform = BuildTarget.NoTarget)
        {
            var graph = asset.Graph;
            return ExecuteGraph(graph, targetPlatform);
        }

        /// <summary>
        /// Executes <see cref="GraphModel"/>
        /// </summary>
        /// <param name="graph">Graph to execute</param>
        /// <param name="targetPlatform">Target platform to use. Uses <see cref="EditorUserBuildSettings.activeBuildTarget"/> if not set</param>
        /// <returns>Execution result</returns>
        public static IGraphExecutionResult ExecuteGraph(GraphModel graph, BuildTarget targetPlatform = BuildTarget.NoTarget)
        {
            if(targetPlatform == BuildTarget.NoTarget)
                targetPlatform = EditorUserBuildSettings.activeBuildTarget;

            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(targetPlatform);
            if(!BuildPipeline.IsBuildTargetSupported(buildTargetGroup, targetPlatform))
                throw new ArgumentException($"Target platform {targetPlatform} is not supported");

            var executor = graph.Type as IExecutableGraphType
                ?? throw new ArgumentException($"Graph type '{graph.Type.Name}' is not executable");

            return executor.Execute(graph, targetPlatform);
        }
    }
}