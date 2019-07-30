using System;
using System.IO;
using System.CodeDom.Compiler;
using DefaultSharp = Microsoft.CSharp.CSharpCodeProvider;
using DefaultBasic = Microsoft.VisualBasic.VBCodeProvider;
using RoslynBasic = Microsoft.CodeDom.Providers.DotNetCompilerPlatform.VBCodeProvider;
using RoslynSharp = Microsoft.CodeDom.Providers.DotNetCompilerPlatform.CSharpCodeProvider;

namespace KE.VSIX.DotNetCompilerPlatform
{
    public static class CodeProvider
    {
        public static CodeDomProvider Create(Language Lang, string RoslynPath)
        {
            if (Lang != Language.CSharp && Lang != Language.VB)
                throw new ArgumentException(nameof(Lang));

            try
            {
                if (Directory.Exists(RoslynPath))
                    return Lang == Language.CSharp
                        ? new RoslynSharp(new CSCompilerSettings(RoslynPath))
                        : (CodeDomProvider)new RoslynBasic(new VBCompilerSettings(RoslynPath));
            }
            catch { }

            return Lang == Language.CSharp
                ? new DefaultSharp()
                : (CodeDomProvider)new DefaultBasic();
        }
        public static CodeDomProvider Create(Language Lang) => Create(Lang, string.Empty);
        public static CodeDomProvider Create(Language Lang, PathContainer Container) => Create(Lang, Container.RoslynPath);
    }
}
