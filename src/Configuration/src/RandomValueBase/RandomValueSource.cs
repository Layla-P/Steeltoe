// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Steeltoe.Extensions.Configuration.RandomValue;

/// <summary>
/// Configuration source used in creating a <see cref="RandomValueProvider"/> that generates random numbers.
/// </summary>
public class RandomValueSource : IConfigurationSource
{
    public const string RandomPrefix = "random:";
    internal ILoggerFactory LoggerFactory;
    internal string Prefix;

    /// <summary>
    /// Initializes a new instance of the <see cref="RandomValueSource"/> class.
    /// </summary>
    /// <param name="logFactory">the logger factory to use.</param>
    public RandomValueSource(ILoggerFactory logFactory = null)
        : this(RandomPrefix, logFactory)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RandomValueSource"/> class.
    /// </summary>
    /// <param name="prefix">key prefix to use to match random number keys. Should end with the configuration separator.</param>
    /// <param name="logFactory">the logger factory to use.</param>
    public RandomValueSource(string prefix, ILoggerFactory logFactory = null)
    {
        LoggerFactory = logFactory;
        this.Prefix = prefix;
    }

    /// <summary>
    /// Builds a <see cref="RandomValueProvider"/> from the sources.
    /// </summary>
    /// <param name="builder">the provided builder.</param>
    /// <returns>the random number provider.</returns>
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new RandomValueProvider(Prefix, LoggerFactory);
    }
}
