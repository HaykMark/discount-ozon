using static Discounting.Common.AccessControl.Operations;

namespace Discounting.Common.AccessControl
{
    /// <summary>
    /// A "zone" is a predefined area or topic in which a certain
    /// set of supported operations can be performed.
    /// It is used in the context of access control.
    /// </summary>
    /// <remarks>
    /// A zone usually corresponds to an entity type. For example a "companies" zone
    /// may support CREATE, READ, UPDATE or DELETE companies.
    /// </remarks>
    public class Zone
    {
        public Zone()
        { }

        public Zone(string id, Operations supports = All)
        {
            Id = id;
            Supports = supports;
        }

        public string Id { get; set; }

        /// <summary>
        /// Should be used to convey extra information about what the zone is
        /// about and the meaning of the operations this zone supports.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Defines the set of allowed operations for this zone
        /// </summary>
        public Operations Supports { get; set; }
    }
}