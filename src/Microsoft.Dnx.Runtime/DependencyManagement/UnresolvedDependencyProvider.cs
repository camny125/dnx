// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

namespace Microsoft.Dnx.Runtime
{
    public class UnresolvedDependencyProvider : IDependencyProvider
    {
        public LibraryDescription GetDescription(LibraryRange libraryRange, FrameworkName targetFramework)
        {
            return new LibraryDescription
            {
                LibraryRange = libraryRange,
                Identity = new LibraryIdentity
                {
                    Name = libraryRange.Name,
                    IsGacOrFrameworkReference = libraryRange.IsGacOrFrameworkReference,
                    Version = libraryRange.VersionRange?.MinVersion
                },
                Dependencies = Enumerable.Empty<LibraryDependency>(),
                Resolved = false
            };
        }

        public void Initialize(IEnumerable<LibraryDescription> dependencies, FrameworkName targetFramework, string runtimeIdentifier)
        {
        }

        public IEnumerable<string> GetAttemptedPaths(FrameworkName targetFramework)
        {
            return Enumerable.Empty<string>();
        }
    }
}