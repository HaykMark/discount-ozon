using System;
using System.Threading.Tasks;
using AutoMapper;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels.TariffDiscounting;
using Discounting.Common.AccessControl;
using Discounting.Common.Response;
using Discounting.Entities.TariffDiscounting;
using Discounting.Logics.AccessControl;
using Discounting.Logics.TariffDiscounting;
using Microsoft.AspNetCore.Mvc;

namespace Discounting.API.Controllers.Discounting
{
    [ApiVersion("1.0")]
    [Route(Routes.DiscountSettings)]
    [Zone(Zones.DiscountSettings)]
    public class DiscountSettingsController : BaseController
    {
        private readonly IDiscountSettingsService discountSettingsService;
        private readonly IMapper mapper;

        public DiscountSettingsController(
            IMapper mapper,
            IDiscountSettingsService discountSettingsService,
            IFirewall firewall
        ) : base(firewall)
        {
            this.mapper = mapper;
            this.discountSettingsService = discountSettingsService;
        }

        [HttpGet]
        public async Task<ResourceCollection<DiscountSettingsDTO>> Get(bool onlyCurrent)
        {
            var discountSettings = await discountSettingsService.GetAllAsync(onlyCurrent);
            return new ResourceCollection<DiscountSettingsDTO>(
                mapper.Map<ResourceCollection<DiscountSettingsDTO>>(discountSettings));
        }

        [HttpGet("{id}")]
        public async Task<DiscountSettingsDTO> Get(Guid id)
        {
            return mapper.Map<DiscountSettingsDTO>(await discountSettingsService.GetAsync(id));
        }

        [HttpPost]
        public async Task<DiscountSettingsDTO> Post([FromBody] DiscountSettingsDTO model)
        {
            return mapper.Map<DiscountSettingsDTO>(
                await discountSettingsService.CreateAsync(mapper.Map<DiscountSettings>(model)));
        }

        [HttpPut("{id}")]
        public async Task<DiscountSettingsDTO> Put(Guid id, [FromBody] DiscountSettingsDTO model)
        {
            return mapper.Map<DiscountSettingsDTO>(
                await discountSettingsService.UpdateAsync(mapper.Map<DiscountSettings>(model)));
        }
    }
}