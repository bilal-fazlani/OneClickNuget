using System;

namespace OneClickNuget.WPF
{
    [Serializable]
    public class ModelState
    {
        public string ApiKey { get; set; }
        public bool AlwaysLoadFromInternet { get; set; }
        public string LastReleaseNotes { get; set; }
    }
}