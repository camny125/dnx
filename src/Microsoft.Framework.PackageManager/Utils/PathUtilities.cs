// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.Framework.Runtime;

namespace Microsoft.Framework.PackageManager
{
    internal static class PathUtilities
    {
        private static Lazy<string> _runtimeHome = new Lazy<string>(FindRuntimeHome);

        public static string RuntimeHomeFolder
        {
            get
            {
                return _runtimeHome.Value;
            }
        }

        public static string RuntimeBinFolder
        {
            get
            {
                if (RuntimeHomeFolder == null)
                {
                    return null;
                }

                return Path.Combine(RuntimeHomeFolder, "bin");
            }
        }

        private static string FindRuntimeHome()
        {
            var runtimeHome = Environment.GetEnvironmentVariable(EnvironmentNames.Home);
            if (string.IsNullOrEmpty(runtimeHome))
            {
                var runtimeGlobalPath = DnuEnvironment.GetFolderPath(DnuFolderPath.DnxGlobalPath);
                var userRuntimeFolder = DnuEnvironment.GetFolderPath(DnuFolderPath.DefaultDnxHome);

                runtimeHome = $"{userRuntimeFolder};{runtimeGlobalPath}";
            }

            foreach (var probePath in runtimeHome.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string fullPath = Environment.ExpandEnvironmentVariables(probePath);

                if (Directory.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            return null;
        }
    }
}