// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using System;

namespace Steeltoe.Common.Transaction;

public class TransactionTimedOutException : TransactionException
{
    public TransactionTimedOutException(string msg)
        : base(msg)
    {
    }

    public TransactionTimedOutException(string msg, Exception cause)
        : base(msg, cause)
    {
    }
}
