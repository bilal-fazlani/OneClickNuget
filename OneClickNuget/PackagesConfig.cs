using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.SqlServer.Server;
using NuGet;
using OneClickNuget.Data;

namespace OneClickNuget
{
    public class PackagesConfigReader
    {
        private readonly PackageRetrieveOptions _options;

        public PackagesConfigReader(PackageRetrieveOptions options)
        {
            _options = options;
        }

        public List<NuGet.Packaging.PackageReference> Read()
        {
            using (FileStream fs = new FileStream(_options.PackageConfigPath, FileMode.Open))
            {
                var packages = new NuGet.Packaging.PackagesConfigReader(fs)
                    .GetPackages()
                    .ToList();
                return packages;
            }
        }
    }
}
