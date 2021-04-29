using System;
using Discounting.Entities.TariffDiscounting;
using Discounting.Seeding.Constants;

namespace Discounting.Seeding.Seed
{
    public class TariffSeed : ISeedDataStrategy<Tariff>
    {
        public Tariff[] GetSeedData()
        {
            return new[]
            {
                new Tariff
                {
                    Id = Guid.NewGuid(),
                    FromDay = 1,
                    UntilDay = 10,
                    FromAmount = 1,
                    UntilAmount = 10000,
                    Rate = 10,
                    UserId = GuidValues.UserGuids.TestBuyer,
                    CreationDate = DateTime.UtcNow,
                    Type = TariffType.Discounting
                },
                new Tariff
                {
                    Id = Guid.NewGuid(),
                    FromDay = 11,
                    UntilDay = 20,
                    FromAmount = 1,
                    UntilAmount = 10000,
                    Rate = 10,
                    UserId = GuidValues.UserGuids.TestBuyer,
                    CreationDate = DateTime.UtcNow,
                    Type = TariffType.Discounting
                },
                new Tariff
                {
                    Id = Guid.NewGuid(),
                    FromDay = 21,
                    FromAmount = 1,
                    UntilAmount = 10000,
                    Rate = 10,
                    UserId = GuidValues.UserGuids.TestBuyer,
                    CreationDate = DateTime.UtcNow,
                    Type = TariffType.Discounting
                }, 
                new Tariff
                {
                    Id = Guid.NewGuid(),
                    FromDay = 1,
                    UntilDay = 10,
                    FromAmount = 10000,
                    UntilAmount = 100000,
                    Rate = 9,
                    UserId = GuidValues.UserGuids.TestBuyer,
                    CreationDate = DateTime.UtcNow,
                    Type = TariffType.Discounting
                },
                new Tariff
                {
                    Id = Guid.NewGuid(),
                    FromDay = 11,
                    UntilDay = 20,
                    FromAmount = 10000,
                    UntilAmount = 100000,
                    Rate = 9,
                    UserId = GuidValues.UserGuids.TestBuyer,
                    CreationDate = DateTime.UtcNow,
                    Type = TariffType.Discounting
                },
                new Tariff
                {
                    Id = Guid.NewGuid(),
                    FromDay = 21,
                    FromAmount = 10000,
                    UntilAmount = 100000,
                    Rate = 9,
                    UserId = GuidValues.UserGuids.TestBuyer,
                    CreationDate = DateTime.UtcNow,
                    Type = TariffType.Discounting
                },
                new Tariff
                {
                    Id = Guid.NewGuid(),
                    FromDay = 1,
                    UntilDay = 10,
                    FromAmount = 100000,
                    Rate = 8,
                    UserId = GuidValues.UserGuids.TestBuyer,
                    CreationDate = DateTime.UtcNow,
                    Type = TariffType.Discounting
                },
                new Tariff
                {
                    Id = Guid.NewGuid(),
                    FromDay = 11,
                    UntilDay = 20,
                    FromAmount = 100000,
                    Rate = 8,
                    UserId = GuidValues.UserGuids.TestBuyer,
                    CreationDate = DateTime.UtcNow,
                    Type = TariffType.Discounting
                },
                new Tariff
                {
                    Id = Guid.NewGuid(),
                    FromDay = 21,
                    FromAmount = 100000,
                    Rate = 8,
                    UserId = GuidValues.UserGuids.TestBuyer,
                    CreationDate = DateTime.UtcNow,
                    Type = TariffType.Discounting
                },
            };
        }
    }
}