using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;

namespace CrazyPanda.UnityCore.NodeEditor
{
    /// <summary>
    /// Holds collection of all <see cref="INodeType"/>s available to a graph
    /// </summary>
    public interface INodeTypeRegistry
    {
        /// <summary>
        /// Collection of all available <see cref="INodeType"/>s
        /// </summary>
        IReadOnlyList<INodeType> AvailableNodes { get; }
    }

    /// <summary>
    /// Provides method to find <see cref="IConnectionType"/> for given port value types
    /// </summary>
    public interface IConnectionTypeRegistry
    {
        /// <summary>
        /// Finds <see cref="IConnectionType"/> for given port value types
        /// </summary>
        /// <param name="from">Type of output <see cref="PortModel"/></param>
        /// <param name="to">Type of input <see cref="PortModel"/></param>
        /// <returns>Requested type or null</returns>
        IConnectionType FindConnectionType( Type from, Type to );
    }

    /// <summary>
    /// Type of graph. Offers logic of how entire graph operates
    /// </summary>
    public interface IGraphType : INodeTypeRegistry, IConnectionTypeRegistry
    {
        /// <summary>
        /// Name of this type
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Inits <see cref="GraphModel"/> after it was loaded from serialized data
        /// </summary>
        /// <param name="graph">Graph to init</param>
        void PostLoad( GraphModel graph );
    }

    /// <summary>
    /// Type of graph that can be executed synchronously
    /// </summary>
    /// <typeparam name="TArgs">Type of arguments</typeparam>
    public interface IExecutableGraphType<TArgs> : IGraphType
    {
        /// <summary>
        /// Executes graph with given arguments
        /// </summary>
        /// <param name="graph">Graph to execute</param>
        /// <param name="args">Execution arguments</param>
        IGraphExecutionResult Execute( GraphModel graph, TArgs args );
    }

    /// <summary>
    /// Type of graph that can be executed asynchronously
    /// </summary>
    /// <typeparam name="TArgs">Type of arguments</typeparam>
    public interface IExecutableAsyncGraphType<TArgs> : IGraphType
    {
        /// <summary>
        /// Executes graph with given arguments
        /// </summary>
        /// <param name="graph">Graph to execute</param>
        /// <param name="args">Execution arguments</param>
        Task<IGraphExecutionResult> ExecuteAsync( GraphModel graph, TArgs args );
    }
}