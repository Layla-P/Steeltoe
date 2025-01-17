// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using System;

namespace Steeltoe.Common.Transaction;

public class IllegalTransactionStateException : TransactionUsageException
{
    public IllegalTransactionStateException(string msg)
        : base(msg)
    {
    }

    public IllegalTransactionStateException(string msg, Exception cause)
        : base(msg, cause)
    {
    }
}
