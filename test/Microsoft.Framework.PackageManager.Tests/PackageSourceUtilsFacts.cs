using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuGet;
using Xunit;

namespace Microsoft.Framework.PackageManager.Tests
{
    public class PackageSourceUtilsFacts
    {
        [Theory]
        [InlineData(@"C:\\foo\\bar", true)]
        [InlineData(@".\foo\bar", true)]
        [InlineData(@"..\foo\bar", true)]
        [InlineData(@"foo\bar", true)]
        [InlineData(@"/var/NuGet/packages", true)]
        [InlineData(@"foo/bar", true)]
        [InlineData(@"gopher://abc123", true)] // We won't actually use it, but we will think it's a path on the local FS :)
        [InlineData(@"http://www.nuget.org", false)]
        [InlineData(@"https://www.nuget.org", false)]
        [InlineData(@"HTTP://www.nuget.org", false)]
        [InlineData(@"HTTPS://www.nuget.org", false)]
        [InlineData(@"HtTP://www.nuget.org", false)]
        [InlineData(@"HTtPs://www.nuget.org", false)]
        public void IsLocalFileSystem_CorrectlyIdentifiesIfStringIsLocalFileSystemPath(string path, bool isFileSystem)
        {
            Assert.Equal(isFileSystem, new PackageSource(path).IsLocalFileSystem());
        }
    }
}
