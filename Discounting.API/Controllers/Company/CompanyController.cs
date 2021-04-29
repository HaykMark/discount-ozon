using System;
using System.Threading.Tasks;
using AutoMapper;
using Discounting.API.Common.Constants;
using Discounting.API.Common.Email;
using Discounting.API.Common.ViewModels;
using Discounting.API.Common.ViewModels.Account;
using Discounting.API.Common.ViewModels.Common;
using Discounting.API.Common.ViewModels.Company;
using Discounting.API.Common.ViewModels.EmailNotification;
using Discounting.API.Common.ViewModels.TariffDiscounting;
using Discounting.Common.AccessControl;
using Discounting.Common.Response;
using Discounting.Entities.CompanyAggregates;
using Discounting.Logics.AccessControl;
using Discounting.Logics.CompanyServices;
using Microsoft.AspNetCore.Mvc;

namespace Discounting.API.Controllers.Company
{
    [ApiVersion("1.0")]
    [Route(Routes.Companies)]
    [Zone(Zones.Companies)]
    public class CompanyController : BaseController
    {
        private readonly ICompanyService companyService;
        private readonly ICompanyBankService companyBankService;
        private readonly IMailer mailer;
        private readonly IMapper mapper;

        public CompanyController(
            IMapper mapper,
            ICompanyService companyService,
            IFirewall firewall,
            IMailer mailer,
            ICompanyBankService companyBankService
        ) : base(firewall)
        {
            this.mapper = mapper;
            this.companyService = companyService;
            this.mailer = mailer;
            this.companyBankService = companyBankService;
        }

        [HttpGet]
        public async Task<ResourceCollection<CompanyDTO>> Get(
            int offset = Filters.Offset,
            int limit = Filters.Limit,
            string tin = null,
            CompanyType companyType = CompanyType.SellerBuyer
        )
        {
            var (companies, count) = await companyService.GetAll(offset, limit, tin,
                companyType);
            return new ResourceCollection<CompanyDTO>(mapper.Map<ResourceCollection<CompanyDTO>>(companies), count);
        }

        [HttpGet("{id}")]
        public async Task<CompanyDTO> Get(Guid id)
        {
            return mapper.Map<CompanyDTO>(await companyService.Get(id));
        }

        [HttpPost]
        public async Task<CompanyDTO> Post([FromBody] CompanyDTO model)
        {
            return mapper.Map<CompanyDTO>(
                await companyService.CreateAsync(mapper.Map<Entities.CompanyAggregates.Company>(model)));
        }

        [HttpPut("{id}")]
        public async Task<CompanyDTO> Put(Guid id, [FromBody] CompanyDTO model)
        {
            return mapper.Map<CompanyDTO>(
                await companyService.UpdateAsync(mapper.Map<Entities.CompanyAggregates.Company>(model)));
        }

        [HttpGet("{id}/users")]
        public async Task<ResourceCollection<UserDTO>> GetUsers(
            Guid id,
            int offset = Filters.Offset,
            int limit = Filters.Limit
        )
        {
            await firewall.RequiresAsync(Operations.Update, Zones.Users);
            var (users, count) = await companyService.GetUsers(id, offset, limit);
            return new ResourceCollection<UserDTO>(mapper.Map<ResourceCollection<UserDTO>>(users), count);
        }

        [HttpGet("{id}/contracts")]
        public async Task<ResourceCollection<ContractDTO>> GetContracts(
            Guid id,
            int offset = Filters.Offset,
            int limit = Filters.Limit
        )
        {
            var (contracts, count) = await companyService.GetContracts(id, offset, limit);
            return new ResourceCollection<ContractDTO>(mapper.Map<ResourceCollection<ContractDTO>>(contracts), count);
        }

        [HttpGet("{id}/tariffs")]
        public async Task<ResourceCollection<TariffDTO>> GetTariffs(
            Guid id,
            int offset = Filters.Offset,
            int limit = Filters.Limit
        )
        {
            var (tariffs, count) = await companyService.GetTariffs(id, offset, limit);
            return new ResourceCollection<TariffDTO>(mapper.Map<ResourceCollection<TariffDTO>>(tariffs), count);
        }

        [HttpPost("{id}/deactivate")]
        public async Task<NoContentResult> DeactivateCompany(Guid id, [FromBody] DeactivationDTO model)
        {
            await companyService.DeactivateAsync(id, model.DeactivationReason);
            return NoContent();
        }

        [HttpPost("{id}/activate")]
        public async Task<NoContentResult> ActivateCompany(Guid id)
        {
            await companyService.ActivateAsync(id);
            return NoContent();
        }

        [HttpPost("send-email-notification")]
        public async Task<NoContentResult> SendActivation([FromBody] CompanyEmailNotificationDTO model)
        {
            var company = await companyService.GetDetailedAsync(model.Id);
            await mailer.SendCompanyEmailAsync(company, model.Type);
            return NoContent();
        }

        [HttpGet("banks")]
        public async Task<ResourceCollection<CompanyBankInfoDTO>> GetAllBankInfos(bool? isActive = null)
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsSuperAdmin);
            return mapper.Map<ResourceCollection<CompanyBankInfoDTO>>(await companyBankService.GetAllAsync(isActive));
        }
    }
}