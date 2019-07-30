namespace KE.VSIX.DotNetCompilerPlatform
{
    internal class CSCompilerSettings : CompilerSettings
    {
        public CSCompilerSettings(string RoslynPath) : base(RoslynPath, "csc") { }
    }
}
