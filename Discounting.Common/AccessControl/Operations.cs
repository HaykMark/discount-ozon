using System;

namespace Discounting.Common.AccessControl
{
    /// <summary>
    /// Operations are actions the user can perform in the system.
    /// </summary>
    /// <remarks>
    /// Use the "|" binary operator to combine operations. 
    /// </remarks>
    [Flags]
    public enum Operations : byte
    {
        /// <summary>convenience operation that indicates that nothing is allowed</summary>
        None = 0x00, // 0000

        /// <summary>right to newly create an item</summary>
        Create = 0x01, // 0001

        /// <summary>right to access (read) an item.</summary>
        Read = 0x02, // 0010

        /// <summary>right modify an existing item.</summary>
        Update = 0x04, // 0100

        /// <summary>right to delete create an</summary>
        Delete = 0x08, // 1000

        /// <summary>convenience operation that holds all operations</summary>
        All = Create | Read | Update | Delete , // 1111

    }
}