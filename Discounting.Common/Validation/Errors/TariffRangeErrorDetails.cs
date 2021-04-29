using Newtonsoft.Json;

namespace Discounting.Common.Validation.Errors
{
    public class TariffRangeErrorDetails : ErrorDetails
    {
        public TariffRangeErrorDetails(object lowerLevel, object higherLevel)
        {
            Key = "wrong-tariff-range";
            Args = JsonConvert.SerializeObject(new
            {
                LOWER_LEVEL = lowerLevel,
                HIGHER_LEVEL = higherLevel
            });
        }

        public override string Key { get; }
        public override string Args { get; set; }
    }
}