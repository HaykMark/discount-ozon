using System;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Data.Context;
using Discounting.Entities;
using Discounting.Entities.Regulations;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Logics.Regulations
{
    public interface IRegulationService
    {
        Task<Regulation[]> GetAllAsync(RegulationType? type);
        public Task<Regulation> Get(Guid id);
        Task<Regulation> CreateAsync(Regulation entity);
        Task<Regulation> UpdateAsync(Regulation entity);
        Task<(string, string, string)> GetRegulationFilePath(Guid id);
    }

    public class RegulationService : IRegulationService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IUploadPathProviderService pathProviderService;

        public RegulationService(
            IUnitOfWork unitOfWork,
            IUploadPathProviderService pathProviderService
        )
        {
            this.unitOfWork = unitOfWork;
            this.pathProviderService = pathProviderService;
        }

        public Task<Regulation[]> GetAllAsync(RegulationType? type)
        {
            return unitOfWork.Set<Regulation>().Where(t =>
                    !type.HasValue || t.Type == type.Value)
                .ToArrayAsync();
        }

        public Task<Regulation> Get(Guid id) =>
            unitOfWork.GetOrFailAsync<Regulation, Guid>(id);

        public async Task<Regulation> CreateAsync(Regulation entity)
        {
            await ValidateTypeUniquenessAsync(entity);
            return await unitOfWork.AddAndSaveAsync(entity);
        }

        public async Task<Regulation> UpdateAsync(Regulation entity)
        {
            await ValidateTypeUniquenessAsync(entity);
            return await unitOfWork.UpdateAndSaveAsync<Regulation, Guid>(entity);
        }

        public async Task<(string, string, string)> GetRegulationFilePath(Guid id)
        {
            var upload = await unitOfWork.GetOrFailAsync(unitOfWork.Set<Upload>()
                .Where(u => u.ProviderId == id &&
                            u.Provider == UploadProvider.Regulation));
            return (pathProviderService.GetUploadPath(upload.Id, upload.ContentType, upload.Provider),
                upload.ContentType, upload.Name);
        }

        private async Task ValidateTypeUniquenessAsync(Regulation entity)
        {
            if (await unitOfWork.Set<Regulation>().AnyAsync(r => r.Id != entity.Id &&
                                                                 r.Type == entity.Type))
            {
                throw new ForbiddenException("regulation-with-this-type-exists",
                    "System can only have exactly one type of regulation");
            }
        }
    }
}