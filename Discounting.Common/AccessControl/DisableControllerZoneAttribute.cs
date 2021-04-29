using System;

namespace Discounting.Common.AccessControl
{
    /// <summary>
    /// Used to disable access-permissions check for a controller's action-method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class DisableControllerZoneAttribute : Attribute
    { }
}