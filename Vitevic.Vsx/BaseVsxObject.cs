namespace Vitevic.Vsx
{
    /// <summary>
    /// Base class for VSX commands, services, etc
    /// All properties shoud be set by package during binding. <see cref="BasePackage.CreateVsxServiceObject"/> to get the idia.
    /// </summary>
    public class BaseVsxObject : IBaseVsxObject
    {
        public BasePackage Package { get; internal set; }
    }
}