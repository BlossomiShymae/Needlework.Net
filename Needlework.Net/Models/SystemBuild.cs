namespace Needlework.Net.Models
{
    public class SystemBuild
    {
        public string Branch { get; set; } = string.Empty;
        public string Patchline { get; set; } = string.Empty;
        public string PatchlineVisibleName { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
    }
}
