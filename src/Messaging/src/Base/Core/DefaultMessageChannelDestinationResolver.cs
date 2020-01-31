﻿// Copyright 2017 the original author or authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;

namespace Steeltoe.Messaging.Core
{
    public class DefaultMessageChannelDestinationResolver : IDestinationResolver<IMessageChannel>
    {
        public IDestinationRegistry Registry { get; }

        public DefaultMessageChannelDestinationResolver(IDestinationRegistry registry)
        {
            if (registry == null)
            {
                throw new ArgumentNullException(nameof(registry));
            }

            Registry = registry;
        }

        public virtual IMessageChannel ResolveDestination(string name)
        {
            if (Registry.Lookup(name) is IMessageChannel result)
            {
                return result;
            }

            return null;
        }

        object IDestinationResolver.ResolveDestination(string name)
        {
            var result = ResolveDestination(name);
            return result;
        }
    }
}