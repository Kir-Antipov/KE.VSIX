namespace KE.VSIX.DotNetCompilerPlatform
{
    internal class VBCompilerSettings : CompilerSettings
    {
        public VBCompilerSettings(string RoslynPath) : base(RoslynPath, "vbc") { }
    }
}
