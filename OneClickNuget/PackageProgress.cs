namespace OneClickNuget
{
    public struct PackageProgress
    {
        public int Percent { get; set; }
        public string Message { get; set; }
        public ProgressResultType ProgressResultType { get; set; } 
    }
}
