// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Runtime.Versioning;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.Runtime.Sources.Impl;

namespace dnx.host
{
    /// <summary>
    /// Application environment built by the DNX native host
    /// </summary>
    internal class HostApplicationEnvironment : IApplicationEnvironment
    {
        private readonly Assembly _assembly;
        private readonly ApplicationGlobalData _globalData;
        private AssemblyName _assemblyName;

        public HostApplicationEnvironment(string appBase, FrameworkName targetFramework, string configuration, Assembly assembly)
        {
            _assembly = assembly;
            _globalData = new ApplicationGlobalData(hostEnvironment: null);

            ApplicationBasePath = appBase;
            RuntimeFramework = targetFramework;
            Configuration = configuration;
        }

        public string ApplicationName => AssemblyName.Name;

        public string Configuration
        {
            get;
            private set;
        }

        public string ApplicationVersion => AssemblyName.Version.ToString();

        public string ApplicationBasePath
        {
            get;
            private set;
        }

        public FrameworkName RuntimeFramework
        {
            get;
            private set;
        }

        private AssemblyName AssemblyName
        {
            get
            {
                if (_assemblyName == null)
                {
                    _assemblyName = _assembly.GetName();
                }

                return _assemblyName;
            }
        }

        public object GetData(string name)
        {
            return _globalData.GetData(name);
        }

        public void SetData(string name, object value)
        {
            _globalData.SetData(name, value);
        }
    }
}
