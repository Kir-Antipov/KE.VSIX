using System;

namespace KE.VSIX.Packages
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ProvideRoslynCompilersAttribute : Attribute
    {
        public readonly bool Provide;

        public ProvideRoslynCompilersAttribute() : this(true) { }
        public ProvideRoslynCompilersAttribute(bool Provide) => this.Provide = Provide;
    }
}
