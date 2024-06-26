﻿namespace ClassLibrary1.Abstractions;

public interface INodeDescriptor
{
    string Url { get; }

    /// <summary>
    /// The node's memory consumption
    /// </summary>
    /// <returns>min 0 - max 1</returns>
    Task<double> GetMemoryUsageAsync();
}