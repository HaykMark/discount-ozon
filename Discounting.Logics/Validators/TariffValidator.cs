using System.Linq;
using Discounting.Common.Exceptions;
using Discounting.Common.Validation;
using Discounting.Common.Validation.Errors;
using Discounting.Entities;
using Discounting.Entities.TariffDiscounting;

namespace Discounting.Logics.Validators
{
    public interface ITariffValidator
    {
        void Validate(Tariff[] tariffs);
    }

    public class TariffValidator : ITariffValidator
    {
        public void Validate(Tariff[] tariffs)
        {
            if (!tariffs.Any())
            {
                throw new ForbiddenException();
            }

            if (tariffs.Last().UntilAmount != null)
            {
                throw new ValidationException(nameof(Tariff.UntilAmount),
                    "The last until amount should be null",
                    new InvalidErrorDetails());
            }

            if (tariffs.Last().UntilDay != null)
            {
                throw new ValidationException(nameof(Tariff.UntilDay),
                    "The last until day should be null",
                    new InvalidErrorDetails());
            }

            if (tariffs.Length == 1)
            {
                return;
            }

            var dayGroup = tariffs.GroupBy(t => new
                {
                    t.FromDay,
                    t.UntilDay
                }).Select(t => new Tariff
                {
                    FromDay = t.Key.FromDay,
                    UntilDay = t.Key.UntilDay
                })
                .OrderBy(t => t.FromDay)
                .ToList();

            var amountGroup = tariffs.GroupBy(t => new
                {
                    t.FromAmount,
                    t.UntilAmount
                }).Select(t => new Tariff
                {
                    FromAmount = t.Key.FromAmount,
                    UntilAmount = t.Key.UntilAmount
                })
                .OrderBy(t => t.FromAmount)
                .ToList();

            for (var i = 1; i < dayGroup.Count; i++)
            {
                var untilDay = dayGroup[i - 1].UntilDay;
                if ((i != dayGroup.Count - 1) && !untilDay.HasValue)
                {
                    throw new ValidationException(new RequiredFieldValidationError(nameof(Tariff.UntilDay)));
                }

                if (untilDay != null && 
                    (dayGroup[i].FromDay - untilDay.Value) != 1)
                {
                    throw new ValidationException(nameof(Tariff.FromDay),
                        "Tariff from day should be greater from previews tariffs UntilDay by 1",
                        new TariffRangeErrorDetails(dayGroup[i].FromDay, dayGroup[i].UntilDay));
                }
            }

            for (var i = 1; i < amountGroup.Count; i++)
            {
                var untilAmount = amountGroup[i - 1].UntilAmount;

                if ((i != amountGroup.Count - 1) && !untilAmount.HasValue)
                {
                    throw new ValidationException(new RequiredFieldValidationError(nameof(Tariff.UntilAmount)));
                }

                if (untilAmount != null && (amountGroup[i].FromAmount - untilAmount.Value) != 0.01M)
                {
                    throw new ValidationException(nameof(Tariff.FromDay),
                        "Tariff from amount should be greater from previews tariffs until amount by 0.01",
                        new TariffRangeErrorDetails(amountGroup[i].FromAmount, amountGroup[i].UntilAmount));
                }
            }

        }
    }
}