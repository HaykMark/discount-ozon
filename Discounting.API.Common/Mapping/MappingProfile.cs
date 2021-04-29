using System;
using System.Linq;
using AutoMapper;
using Discounting.API.Common.Constants;
using Discounting.API.Common.ViewModels;
using Discounting.API.Common.ViewModels.AccessControl;
using Discounting.API.Common.ViewModels.Account;
using Discounting.API.Common.ViewModels.Company;
using Discounting.API.Common.ViewModels.Registry;
using Discounting.API.Common.ViewModels.Regulations;
using Discounting.API.Common.ViewModels.Signature;
using Discounting.API.Common.ViewModels.TariffDiscounting;
using Discounting.API.Common.ViewModels.Templates;
using Discounting.API.Common.ViewModels.UnformalizedDocument;
using Discounting.Entities;
using Discounting.Entities.Account;
using Discounting.Entities.Auditing;
using Discounting.Entities.CompanyAggregates;
using Discounting.Entities.Regulations;
using Discounting.Entities.TariffDiscounting;
using Discounting.Entities.Templates;
using Discounting.Logics.Models;

namespace Discounting.API.Common.Mapping
{
    public class MappingProfile : Profile
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public MappingProfile()
        {
            // Domain entity to DTO and reverse.
            CreateMap<User, UserDTO>()
                .ReverseMap()
                .ForMember(to => to.LastLoggedIn, c => c.Ignore())
                .ForMember(to => to.UserRoles, c => c.Ignore());

            CreateMap<Audit, AuditDTO>()
                .ForMember(to => to.UserName,
                    from =>
                        from.MapFrom(entity =>
                            entity.User != null
                                ? $"{entity.User.Surname} {entity.User.FirstName} {entity.User.SecondName} / {entity.User.Email}"
                                : ""))
                .ForMember(to => to.CompanyShortName,
                    from =>
                        from.MapFrom(entity => entity.User != null && entity.User.Company != null 
                            ? entity.User.Company.ShortName 
                            : ""))
                .ForMember(to => to.CompanyId,
                    from =>
                        from.MapFrom(entity =>
                            entity.User != null
                                ? entity.User.CompanyId
                                : (Guid?) null))
                .ForMember(to => to.Tin,
                    from =>
                        from.MapFrom(entity =>
                            entity.User != null && entity.User.Company != null
                                ? entity.User.Company.TIN
                                : ""));

            CreateMap<Role, RoleDTO>()
                .ForMember(
                    dto => dto.Permissions,
                    c => c.MapFrom(entity =>
                        entity.Permissions.ToDictionary(p => p.ZoneId, p => p.Operations)))
                .ReverseMap()
                .ForMember(entity => entity.UserRoles, cfg => cfg.Ignore())
                .ForPath(
                    entity => entity.Permissions,
                    opt => opt.MapFrom(dto =>
                        dto.Permissions.Select(p =>
                            new Permission
                            {
                                RoleId = dto.Id,
                                ZoneId = p.Key,
                                Operations = p.Value
                            })));

            CreateMap<FreeDay, FreeDayDTO>().ReverseMap();
            CreateMap<Template, TemplateDTO>().ReverseMap();
            CreateMap<Template, TemplateLookupsDTO>();
            CreateMap<BuyerTemplateConnection, BuyerTemplateConnectionDTO>()
                .ForMember(to => to.BuyerName, c => c.MapFrom(entity => entity.Buyer.FullName))
                .ForMember(to => to.TemplateName, c => c.MapFrom(entity => entity.Template.Name))
                .ReverseMap()
                .ForMember(to => to.Buyer, c => c.Ignore())
                .ForMember(to => to.Template, c => c.Ignore());
            CreateMap<CompanyRegulation, CompanyRegulationDTO>().ReverseMap();
            CreateMap<Upload, UploadDTO>().ReverseMap();
            CreateMap<UserRole, UserRoleDTO>().ReverseMap();
            CreateMap<UserRole, UserRoleDTO>().ReverseMap();

            //Company aggregates
            CreateMap<Company, CompanyDTO>().ReverseMap();
            CreateMap<CompanyAuthorizedUserInfo, CompanyAuthorizedUserInfoDTO>().ReverseMap();
            CreateMap<CompanyContactInfo, CompanyContactInfoDTO>().ReverseMap();
            CreateMap<CompanyOwnerPositionInfo, CompanyOwnerPositionInfoDTO>().ReverseMap();
            CreateMap<MigrationCardInfo, MigrationCardInfoDTO>().ReverseMap();
            CreateMap<ResidentPassportInfo, ResidentPassportInfoDTO>().ReverseMap();
            CreateMap<CompanyBankInfo, CompanyBankInfoDTO>().ReverseMap();

            CreateMap<CompanySettings, CompanySettingsDTO>().ReverseMap();
            CreateMap<Tariff, TariffDTO>().ReverseMap();
            CreateMap<TariffArchive, TariffArchiveDTO>().ReverseMap();
            CreateMap<FactoringAgreement, FactoringAgreementDTO>()
                .ForMember(to => to.SupplyFactoringAgreementDtos,
                    c => c.MapFrom(from => from.SupplyFactoringAgreements))
                .ReverseMap()
                .ForMember(to => to.SupplyFactoringAgreements,
                    c => c.MapFrom(from => from.SupplyFactoringAgreementDtos));
            CreateMap<SupplyFactoringAgreement, SupplyFactoringAgreementDTO>().ReverseMap();
            CreateMap<Registry, RegistryDTO>()
                .ReverseMap();
            CreateMap<Discount, DiscountDTO>().ReverseMap();
            CreateMap<DiscountSettings, DiscountSettingsDTO>().ReverseMap();
            CreateMap<SupplyDiscount, SupplyDiscountDTO>().ReverseMap();
            CreateMap<RegistrationDTO, User>()
                .ForPath(to => to.Company,
                    opt => opt.MapFrom(dto =>
                        new Company
                        {
                            TIN = dto.TIN,
                            KPP = dto.KPP,
                            FullName = dto.FullName,
                            ShortName = dto.ShortName,
                            CompanyType = dto.Type
                        }));

            CreateMap<Contract, ContractDTO>()
                .ForMember(to => to.SellerTin, c => c.MapFrom(entity => entity.Seller.TIN))
                .ReverseMap()
                .ForPath(to => to.Seller,
                    opt => opt.MapFrom(dto =>
                        new Company
                        {
                            TIN = dto.SellerTin,
                            CompanyType = CompanyType.SellerBuyer
                        }));
            CreateMap<Supply, SupplyDTO>()
                .ForMember(to => to.SellerTin, c => c.MapFrom(entity => entity.Contract.Seller.TIN))
                .ForMember(to => to.BuyerTin, c => c.MapFrom(entity => entity.Contract.Buyer.TIN))
                .ReverseMap()
                .ForMember(x => x.Id, opt => opt.MapFrom(o => Guid.NewGuid()))
                .ForMember(x => x.CreationDate, opt => opt.MapFrom(o => DateTime.UtcNow))
                .ForPath(to => to.Contract,
                    opt => opt.MapFrom(dto =>
                        new Contract()
                        {
                            Seller = new Company
                            {
                                TIN = dto.SellerTin,
                                CompanyType = CompanyType.SellerBuyer,
                                Id = dto.ContractId,
                            },
                            Buyer = new Company
                            {
                                TIN = dto.BuyerTin,
                                CompanyType = CompanyType.SellerBuyer,
                                Id = dto.ContractId,
                            },
                        }));
            CreateMap<SignatureInfo, SignatureInfoDTO>().ReverseMap();
            CreateMap<RegistrySignature, RegistrySignatureDTO>()
                .ForMember(to => to.Info, c => c.MapFrom(entity => entity.SignatureInfo))
                .ReverseMap()
                .ForMember(to => to.SignatureInfo, c => c.Ignore());
            CreateMap<CompanyRegulationSignature, CompanyRegulationSignatureDTO>()
                .ForMember(to => to.Info, c => c.MapFrom(entity => entity.SignatureInfo))
                .ReverseMap()
                .ForMember(to => to.SignatureInfo, c => c.Ignore());
            CreateMap<UnformalizedDocumentSignature, UnformalizedDocumentSignatureDTO>()
                .ForMember(to => to.Info, c => c.MapFrom(entity => entity.SignatureInfo))
                .ReverseMap()
                .ForMember(to => to.SignatureInfo, c => c.Ignore());
            CreateMap<UserRegulationSignature, UserRegulationSignatureDTO>()
                .ForMember(to => to.Info, c => c.MapFrom(entity => entity.SignatureInfo))
                .ReverseMap()
                .ForMember(to => to.SignatureInfo, c => c.Ignore());
            CreateMap<SignatureRequestDTO, SignatureRequest>().ReverseMap();

            CreateMap<Regulation, RegulationDTO>().ReverseMap();

            CreateMap<UnformalizedDocumentReceiver, UnformalizedDocumentReceiverDTO>().ReverseMap();
            CreateMap<UnformalizedDocument, UnformalizedDocumentDTO>()
                .ForMember(to => to.Receivers, c => c.MapFrom(entity => entity.Receivers))
                .ReverseMap()
                .ForMember(to => to.Receivers, c => c.MapFrom(dto => dto.Receivers))
                .ForMember(to => to.Status, c => c.Ignore())
                .ForMember(to => to.IsSent, c => c.Ignore())
                .ForMember(to => to.CreationDate, c => c.Ignore())
                .ForMember(to => to.DeclinedBy, c => c.Ignore())
                .ForMember(to => to.DeclinedDate, c => c.Ignore())
                .ForMember(to => to.DeclineReason, c => c.Ignore());
            CreateMap<UnformalizedDocumentDeclineDTO, UnformalizedDocument>();

            CreateMap<UserProfileRegulationInfo, UserProfileRegulationInfoDTO>()
                .ReverseMap();

            CreateMap<UserRegulation, UserRegulationDTO>()
                .ForMember(to => to.UserProfileRegulationInfo,
                    c => c.Condition(src => src.Type == UserRegulationType.Profile))
                .ReverseMap()
                .ForMember(to => to.UserProfileRegulationInfo,
                    c => c.Condition(src => src.Type == UserRegulationType.Profile));
        }
    }
}