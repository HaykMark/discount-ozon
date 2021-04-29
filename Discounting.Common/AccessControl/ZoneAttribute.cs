using System;

namespace Discounting.Common.AccessControl
{
    /// <summary>
    /// Used to connect a controller with a zone.
    /// By connecting a controller with a zone, the firewall will
    /// implicitly treat the following HTTP methods in the context
    /// of this zone as follows:
    /// 
    /// HttpPost   => CREATE 
    /// HttpGet    => READ 
    /// HttpPut    => UPDATE 
    /// HttpDelete => DELETE
    /// </summary>
    /// 
    /// <example>
    /// Given the following code...
    /// 
    ///     [Zone("zone-x")]
    ///     public class ControllerX
    ///     {
    ///         [HttpGet]
    ///         public IActionResult Get()
    ///     
    /// ...the user can only enter if he/she has the right READ for "zone-x".
    /// </example>
    /// <seealso cref="FirewallActionFilter" />
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ZoneAttribute : Attribute
    {
        public string ZoneId { get; set; }

        public ZoneAttribute(string zoneId)
        {
            this.ZoneId = zoneId;
        }
    }
}
