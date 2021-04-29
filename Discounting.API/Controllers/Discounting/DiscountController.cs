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
    [Route(Routes.Discounts)]
    [Zone(Zones.Discounts)]
    public class DiscountController : BaseController
    {
        private readonly IDiscountService discountService;
        private readonly IMapper mapper;

        public DiscountController(
            IMapper mapper,
            IDiscountService discountService,
            IFirewall firewall
        ) : base(firewall)
        {
            this.mapper = mapper;
            this.discountService = discountService;
        }

        [HttpGet]
        public async Task<ResourceCollection<DiscountDTO>> Get(
            int offset = Filters.Offset,
            int limit = Filters.Limit
        )
        {
            await firewall.RequiresAsync(ctx => ctx.User.IsSuperAdmin);
            var (discounts, count) = await discountService.GetAll(offset, limit);
            return new ResourceCollection<DiscountDTO>(
                mapper.Map<ResourceCollection<DiscountDTO>>(discounts), count);
        }

        [HttpGet("{id}")]
        public async Task<DiscountDTO> Get(Guid id)
        {
            return mapper.Map<DiscountDTO>(await discountService.Get(id));
        }

        [HttpPost]
        public async Task<DiscountDTO> Post([FromBody] DiscountDTO model)
        {
            return mapper.Map<DiscountDTO>(await discountService.CreateAsync(mapper.Map<Discount>(model)));
        }

        [HttpPut("{id}")]
        public async Task<DiscountDTO> Put(Guid id, [FromBody] DiscountDTO model)
        {
            discountService.IPAddress = GetIpAddress();
            return mapper.Map<DiscountDTO>(await discountService.UpdateAsync(mapper.Map<Discount>(model)));
        }


        [HttpPost("{id}/supplies-discounts")]
        public async Task<ResourceCollection<SupplyDiscountDTO>> CreateSupplyDiscount(Guid id,
            [FromBody] SupplyDiscountDTO[] supplyDiscounts)
        {
            discountService.IPAddress = GetIpAddress();
            return mapper.Map<ResourceCollection<SupplyDiscountDTO>>(
                await discountService.CreateSupplyDiscountAsync(id, mapper.Map<SupplyDiscount[]>(supplyDiscounts)));
        }
    }
}