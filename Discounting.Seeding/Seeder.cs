using System.Threading.Tasks;
using Discounting.Data.Context;
using Discounting.Seeding.Seed;

namespace Discounting.Seeding
{
    /// <summary>
    /// Extension for seeding generic entity type.
    /// </summary>
    public class Seeder
    {
        private readonly DiscountingDbContext context;

        public Seeder(DiscountingDbContext context)
        {
            this.context = context;
        }

        public async Task Seed()
        {
            await Seed(new CompanySeed());
            await Seed(new UserSeed());
            await Seed(new RoleSeed());
            await Seed(new UserRoleSeed());
            await Seed(new PermissionSeed());
            
            await Seed(new CompanyOwnerPositionInfoSeed());
            await Seed(new CompanyAuthorizedUserInfoSeed());
            await Seed(new CompanyContactInfoSeed());
            await Seed(new MigrationCardInfoSeed());
            await Seed(new ResidentPassportInfoSeed());
            
            await Seed(new ContractSeed());
            await Seed(new SupplySeed());
            await Seed(new CompanySettingsSeed());
            await Seed(new FactoringAgreementSeed());
            await Seed(new TariffSeed());
            await Seed(new TemplateSeed());
            await Seed(new BuyerTemplateConnectionSeed());
        }

        public async Task SeedFromFile<T>() where T : class
        {
            var seedDataStrategy = new SeedDataFromFileStrategy<T>();

            await Seed(seedDataStrategy);
        }

        public async Task Seed<T>(ISeedDataStrategy<T> seedDataStrategy) where T : class
        {
            var data = seedDataStrategy.GetSeedData();
            context.AddRange(data);
            await context.SaveChangesAsync();
        }
    };
}