using System.Collections.Generic;

namespace OneClickNuget.WPF
{
    public class DependencyModel
    {
        public string Id { get; set; }
        
        public string Version { get; set; }

        public override string ToString()
        {
            return $"{Id} {Version}";
        }
    }

    public class DependencyModelIdComparator : EqualityComparer<DependencyModel>
    {
        public override bool Equals(DependencyModel x, DependencyModel y)
        {
            return x.Id == y.Id;
        }

        public override int GetHashCode(DependencyModel obj)
        {
            return obj?.Id?.GetHashCode() ?? 0;
        }
    }
}