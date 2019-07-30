using System.IO;
using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;

namespace KE.VSIX.DotNetCompilerPlatform
{
    public class CompilerSettings : ICompilerSettings
    {
        public string CompilerFullPath { get; }
        public int CompilerServerTimeToLive { get; }

        protected CompilerSettings(string RoslynPath, string CompilerName)
        {
            CompilerFullPath = Path.Combine(RoslynPath, $"{CompilerName}.exe");
            CompilerServerTimeToLive = 0;
        }
    }
}
