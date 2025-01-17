// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Logging;
using System;

namespace Steeltoe.Management.Endpoint.HeapDump;

public class HeapDumpEndpoint : AbstractEndpoint<string>, IHeapDumpEndpoint
{
    private readonly ILogger<HeapDumpEndpoint> _logger;
    private readonly IHeapDumper _heapDumper;

    public HeapDumpEndpoint(IHeapDumpOptions options, IHeapDumper heapDumper, ILogger<HeapDumpEndpoint> logger = null)
        : base(options)
    {
        _heapDumper = heapDumper ?? throw new ArgumentNullException(nameof(heapDumper));
        _logger = logger;
    }

    public new IHeapDumpOptions Options => options as IHeapDumpOptions;

    public override string Invoke()
    {
        return _heapDumper.DumpHeap();
    }
}
