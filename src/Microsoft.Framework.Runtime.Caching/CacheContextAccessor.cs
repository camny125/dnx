﻿using System;
using Microsoft.Framework.Runtime.Compilation;

namespace Microsoft.Framework.Runtime.Caching
{
    public class CacheContextAccessor : ICacheContextAccessor
    {
        [ThreadStatic]
        private static CacheContext _threadInstance;

        public static CacheContext ThreadInstance
        {
            get { return _threadInstance; }
            set { _threadInstance = value; }
        }

        public CacheContext Current
        {
            get { return ThreadInstance; }
            set { ThreadInstance = value; }
        }
    }
}