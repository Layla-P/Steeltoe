// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

// ReSharper disable IdentifierTypo

using System.Text.Json.Serialization;
using Steeltoe.Common.Util;

namespace Steeltoe.Security.DataProtection.CredHub;

/// <summary>
/// Uses for certificates.
/// </summary>
[JsonConverter(typeof(SnakeCaseNoCapsEnumMemberJsonConverter))]
public enum KeyUsage
{
    DigitalSignature,
    NonRepudiation,
    KeyEncipherment,
    DataEncipherment,
    KeyAgreement,
    KeyCertSign,
    CrlSign,
    EncipherOnly,
    DecipherOnly
}
