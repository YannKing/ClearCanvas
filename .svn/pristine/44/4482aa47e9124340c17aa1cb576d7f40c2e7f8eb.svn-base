using System;

namespace ClearCanvas.Common.Shreds
{
    public enum ShredIsolationLevel
    {
        /// <summary>将新的AppDomain专用于Shred。
        /// Dedicate a new AppDomain to the shred.
        /// </summary>
        OwnAppDomain = 0,
        /// <summary>Shred将在主shredhost服务进程的默认AppDomain中实例化。
        /// The shred will be instantiated in the default AppDomain for the main shredhost service process.
        /// </summary>
        None
        //NamedAppDomain?, OwnProcess?
    }

    /// <summary>用于指定如何分离给定的Shred（如果有的话）。
    /// Use to specify how a given shred should be isolated, if at all.
    /// </summary>
    public class ShredIsolationAttribute : Attribute
    {
        public ShredIsolationAttribute()
        {
            Level = ShredIsolationLevel.OwnAppDomain;
        }

        /// <summary>
        /// 指定<see cref ="IShred"> shred </see>的隔离级别。
        /// Specifies the level of isolation for a <see cref="IShred">shred</see>.
        /// </summary>
        public ShredIsolationLevel Level;
    }
}
