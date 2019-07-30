using System;
using System.IO;
using Dir = System.IO.Path;

namespace KE.VSIX
{
    public class PathContainer
    {
        #region Constants
        private static readonly string _CommonPath;
        private static readonly string _RoslynPath;
        private static readonly string _ExtensionsPath;
        #endregion

        #region Var
        public readonly string Path;
        public readonly string CommonPath;
        public readonly string RoslynPath;
        public readonly string DllLocation;
        public readonly string PackageName;
        public readonly string ExtensionsPath;
        #endregion

        #region Init
        static PathContainer()
        {
            _ExtensionsPath = Dir.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KE", "VSIX");
            _CommonPath = Dir.GetFullPath(Dir.Combine(_ExtensionsPath, "..", "Common"));
            _RoslynPath = Dir.Combine(_CommonPath, "roslyn");

            foreach (string path in new[] { _ExtensionsPath, _CommonPath, _RoslynPath })
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
        }

        internal PathContainer(Type Package)
        {
            ExtensionsPath = _ExtensionsPath;
            CommonPath = _CommonPath;
            RoslynPath = _RoslynPath;
            PackageName = Package.Assembly.GetName().Name;
            Path = Dir.Combine(_ExtensionsPath, PackageName);
            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);
            DllLocation = Dir.GetDirectoryName(new Uri(Package.Assembly.CodeBase, UriKind.Absolute).LocalPath);
        }
        #endregion

        #region Functions
        public string MapPath(string Path) => Dir.Combine(this.Path, Path);

        public override string ToString() => Path;
        public override int GetHashCode() => DllLocation.GetHashCode();
        public override bool Equals(object obj) => obj is PathContainer o && o.DllLocation == DllLocation;
        #endregion
    }
}
