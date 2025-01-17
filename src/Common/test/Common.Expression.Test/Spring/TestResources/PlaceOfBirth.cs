// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

namespace Steeltoe.Common.Expression.Internal.Spring.TestResources;

public class PlaceOfBirth
{
    public string Country;

    public override string ToString() => City;

    public string City { get; set; }

    public PlaceOfBirth(string str) => City = str;

    public int DoubleIt(int i) => i * 2;

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is not PlaceOfBirth other)
        {
            return false;
        }

        return City.Equals(other.City);
    }

    public override int GetHashCode() => City.GetHashCode();
}
