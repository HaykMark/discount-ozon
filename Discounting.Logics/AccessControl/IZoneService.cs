using System.Collections.Generic;
using Discounting.Common.AccessControl;

namespace Discounting.Logics.AccessControl
{
    public interface IZoneService
    {
        /// <summary>
        /// /// All registered zones identified by their id.
        /// </summary>
        Dictionary<string, Zone> GetAll();
    }
}