using System.Collections.Generic;
using System.Linq;
using Discounting.Common.AccessControl;
using Discounting.Logics.AccessControl;

namespace Discounting.API.Common.Services
{
    public class ZoneService : IZoneService
    {
        private static Dictionary<string, Zone> zonesById;

        /// <summary>
        /// Initializer for static vars, in particular `Zones.All`
        /// </summary>
        static ZoneService()
        {
            zonesById = GetZoneIdsFromRegistry()
                .Select(zoneId => new Zone(zoneId))
                .ToDictionary(zone => zone.Id);
        }

        public Dictionary<string, Zone> GetAll()
        {
            return zonesById;
        }

        private static HashSet<string> GetZoneIdsFromRegistry()
        {
            return new HashSet<string>(
                typeof(Zones).GetFields().Select(f => (string)f.GetValue(null))
            );
        }
    }
}