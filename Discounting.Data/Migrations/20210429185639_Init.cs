using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Discounting.Data.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FullName = table.Column<string>(type: "varchar(500)", nullable: true),
                    ShortName = table.Column<string>(type: "varchar(500)", nullable: true),
                    TIN = table.Column<string>(type: "varchar(12)", nullable: false),
                    KPP = table.Column<string>(type: "varchar(12)", nullable: true),
                    PSRN = table.Column<string>(type: "varchar(15)", nullable: true),
                    IncorporationForm = table.Column<string>(type: "varchar(500)", nullable: true),
                    RegisteringAuthorityName = table.Column<string>(type: "varchar(500)", nullable: true),
                    RegistrationStatePlace = table.Column<string>(type: "varchar(500)", nullable: true),
                    StateStatisticsCode = table.Column<string>(type: "varchar(500)", nullable: true),
                    PaidUpAuthorizedCapitalInformation = table.Column<string>(type: "varchar(500)", nullable: true),
                    StateRegistrationDate = table.Column<DateTime>(nullable: true),
                    OwnerFullName = table.Column<string>(type: "varchar(300)", nullable: true),
                    OwnerPosition = table.Column<string>(type: "varchar(300)", nullable: true),
                    OwnerDocument = table.Column<string>(type: "varchar(250)", nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    HasPowerOfAttorney = table.Column<bool>(nullable: false),
                    DeactivationReason = table.Column<string>(type: "varchar(2000)", nullable: true),
                    CompanyType = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FreeDays",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    DeactivatedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FreeDays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Regulations",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Type = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regulations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Remarks = table.Column<string>(type: "varchar(500)", nullable: true),
                    Type = table.Column<byte>(nullable: false),
                    IsSystemDefault = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanyAuthorizedUserInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CompanyId = table.Column<Guid>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Number = table.Column<string>(type: "varchar(500)", nullable: false),
                    FirstName = table.Column<string>(type: "varchar(500)", nullable: false),
                    SecondName = table.Column<string>(type: "varchar(500)", nullable: false),
                    LastName = table.Column<string>(type: "varchar(500)", nullable: false),
                    Citizenship = table.Column<string>(type: "varchar(500)", nullable: false),
                    PlaceOfBirth = table.Column<string>(type: "varchar(500)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(nullable: false),
                    AuthorityValidityDate = table.Column<DateTime>(nullable: false),
                    IdentityDocument = table.Column<string>(type: "varchar(500)", nullable: false),
                    IsResident = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyAuthorizedUserInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyAuthorizedUserInfos_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyContactInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CompanyId = table.Column<Guid>(nullable: false),
                    Address = table.Column<string>(type: "varchar(500)", nullable: false),
                    OrganizationAddress = table.Column<string>(type: "varchar(500)", nullable: false),
                    Phone = table.Column<string>(type: "varchar(11)", nullable: false),
                    Email = table.Column<string>(type: "varchar(50)", nullable: false),
                    MailingAddress = table.Column<string>(type: "varchar(50)", nullable: true),
                    NameOfGoverningBodies = table.Column<string>(type: "varchar(500)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyContactInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyContactInfos_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyPositionInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CompanyId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(type: "varchar(500)", nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Number = table.Column<string>(type: "varchar(500)", nullable: false),
                    FirstName = table.Column<string>(type: "varchar(500)", nullable: false),
                    SecondName = table.Column<string>(type: "varchar(500)", nullable: false),
                    LastName = table.Column<string>(type: "varchar(500)", nullable: false),
                    Citizenship = table.Column<string>(type: "varchar(500)", nullable: false),
                    PlaceOfBirth = table.Column<string>(type: "varchar(500)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(nullable: false),
                    AuthorityValidityDate = table.Column<DateTime>(nullable: false),
                    IdentityDocument = table.Column<string>(type: "varchar(500)", nullable: false),
                    IsResident = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyPositionInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyPositionInfos_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiscountSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CompanyId = table.Column<Guid>(nullable: false),
                    MinimumDaysToShift = table.Column<int>(nullable: false),
                    DaysType = table.Column<byte>(nullable: false),
                    PaymentWeekDays = table.Column<int>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscountSettings_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FactoringAgreements",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CompanyId = table.Column<Guid>(nullable: false),
                    BankId = table.Column<Guid>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    IsConfirmed = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    FactoringContractNumber = table.Column<string>(type: "varchar(150)", nullable: true),
                    FactoringContractDate = table.Column<DateTime>(nullable: true),
                    BankName = table.Column<string>(type: "varchar(250)", nullable: true),
                    BankCity = table.Column<string>(type: "varchar(250)", nullable: true),
                    BankBic = table.Column<string>(type: "varchar(9)", nullable: true),
                    BankOGRN = table.Column<string>(type: "varchar(13)", nullable: true),
                    BankCorrespondentAccount = table.Column<string>(type: "varchar(20)", nullable: true),
                    BankCheckingAccount = table.Column<string>(type: "varchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FactoringAgreements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FactoringAgreements_Companies_BankId",
                        column: x => x.BankId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FactoringAgreements_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MigrationCardInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CompanyId = table.Column<Guid>(nullable: false),
                    RightToResideDocument = table.Column<string>(type: "varchar(500)", nullable: false),
                    Address = table.Column<string>(type: "varchar(500)", nullable: false),
                    RegistrationAddress = table.Column<string>(type: "varchar(500)", nullable: false),
                    Phone = table.Column<string>(type: "varchar(11)", nullable: false),
                    PositionType = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MigrationCardInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MigrationCardInfos_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResidentPassportInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CompanyId = table.Column<Guid>(nullable: false),
                    Series = table.Column<string>(type: "varchar(4)", nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Number = table.Column<string>(type: "varchar(6)", nullable: false),
                    UnitCode = table.Column<string>(type: "varchar(7)", nullable: false),
                    IssuingAuthorityPSRN = table.Column<string>(type: "varchar(500)", nullable: false),
                    SNILS = table.Column<string>(type: "varchar(500)", nullable: true),
                    PositionType = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResidentPassportInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResidentPassportInfos_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CompanyId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(type: "varchar(1024)", nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "varchar(255)", nullable: false),
                    Type = table.Column<byte>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Templates_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UnformalizedDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    SenderId = table.Column<Guid>(nullable: false),
                    Type = table.Column<byte>(nullable: false, defaultValue: (byte)5),
                    Topic = table.Column<string>(type: "varchar(100)", nullable: false),
                    Message = table.Column<string>(type: "varchar(1000)", nullable: true),
                    Status = table.Column<byte>(nullable: false, defaultValue: (byte)1),
                    IsSent = table.Column<bool>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    SentDate = table.Column<DateTime>(nullable: true),
                    DeclinedBy = table.Column<Guid>(nullable: true),
                    DeclinedDate = table.Column<DateTime>(nullable: true),
                    DeclineReason = table.Column<string>(type: "varchar(1000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnformalizedDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnformalizedDocuments_Companies_DeclinedBy",
                        column: x => x.DeclinedBy,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UnformalizedDocuments_Companies_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FirstName = table.Column<string>(type: "varchar(500)", nullable: false),
                    Surname = table.Column<string>(type: "varchar(500)", nullable: false),
                    SecondName = table.Column<string>(type: "varchar(500)", nullable: true),
                    Position = table.Column<string>(type: "varchar(500)", nullable: true),
                    Phone = table.Column<string>(type: "varchar(11)", nullable: true),
                    Password = table.Column<string>(type: "varchar(4000)", nullable: false),
                    Token = table.Column<string>(nullable: true),
                    Salt = table.Column<string>(type: "varchar(2000)", nullable: false),
                    DisplayName = table.Column<string>(nullable: true),
                    CanSign = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    IsAdmin = table.Column<bool>(nullable: false, defaultValue: false),
                    IsSuperAdmin = table.Column<bool>(nullable: false),
                    IsTestUser = table.Column<bool>(nullable: false, defaultValue: false),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    ActivationToken = table.Column<string>(type: "varchar(4000)", nullable: true),
                    ActivationTokenCreationDateTime = table.Column<DateTime>(nullable: true),
                    LastLoggedIn = table.Column<DateTimeOffset>(nullable: true),
                    SerialNumber = table.Column<string>(type: "varchar(50)", nullable: false),
                    Email = table.Column<string>(type: "varchar(450)", nullable: false),
                    IsEmailConfirmed = table.Column<bool>(nullable: false),
                    IsConfirmedByAdmin = table.Column<bool>(nullable: false),
                    DeactivationReason = table.Column<string>(type: "varchar(2000)", nullable: true),
                    PasswordRetryLimit = table.Column<int>(nullable: false),
                    CompanyId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<Guid>(nullable: false),
                    ZoneId = table.Column<string>(nullable: true),
                    Operations = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplyFactoringAgreements",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FactoringAgreementId = table.Column<Guid>(nullable: false),
                    Number = table.Column<string>(type: "varchar(150)", nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    Status = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplyFactoringAgreements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplyFactoringAgreements_FactoringAgreements_FactoringAgre~",
                        column: x => x.FactoringAgreementId,
                        principalTable: "FactoringAgreements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BuyerTemplateConnections",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    BuyerId = table.Column<Guid>(nullable: false),
                    BankId = table.Column<Guid>(nullable: false),
                    TemplateId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuyerTemplateConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuyerTemplateConnections_Companies_BankId",
                        column: x => x.BankId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BuyerTemplateConnections_Companies_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BuyerTemplateConnections_Templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "Templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnformalizedDocumentReceivers",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UnformalizedDocumentId = table.Column<Guid>(nullable: false),
                    ReceiverId = table.Column<Guid>(nullable: false),
                    NeedSignature = table.Column<bool>(nullable: false),
                    IsSigned = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnformalizedDocumentReceivers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnformalizedDocumentReceivers_Companies_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UnformalizedDocumentReceivers_UnformalizedDocuments_Unforma~",
                        column: x => x.UnformalizedDocumentId,
                        principalTable: "UnformalizedDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Audits",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Incident = table.Column<int>(nullable: false),
                    SourceId = table.Column<string>(type: "varchar(500)", nullable: true),
                    UserId = table.Column<Guid>(nullable: true),
                    IncidentDate = table.Column<DateTime>(nullable: false),
                    IpAddress = table.Column<string>(type: "varchar(50)", nullable: true),
                    Message = table.Column<string>(nullable: true),
                    IncidentResult = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Audits_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanyBankInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CompanyId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(type: "varchar(250)", nullable: false),
                    City = table.Column<string>(type: "varchar(250)", nullable: false),
                    Bic = table.Column<string>(type: "varchar(9)", nullable: false),
                    OGRN = table.Column<string>(type: "varchar(13)", nullable: false),
                    CorrespondentAccount = table.Column<string>(type: "varchar(20)", nullable: false),
                    CheckingAccount = table.Column<string>(type: "varchar(20)", nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyBankInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyBankInfos_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanyBankInfos_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CompanyRegulations",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    CompanyId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(type: "varchar(1024)", nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "varchar(255)", nullable: false),
                    Type = table.Column<byte>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyRegulations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyRegulations_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanyRegulations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanySettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CompanyId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    DefaultTariff = table.Column<Guid>(nullable: true),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    IsSendAutomatically = table.Column<bool>(nullable: false),
                    IsAuction = table.Column<bool>(nullable: false),
                    ForbidSellerEditTariff = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanySettings_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanySettings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    SellerId = table.Column<Guid>(nullable: false),
                    BuyerId = table.Column<Guid>(nullable: false),
                    Status = table.Column<byte>(nullable: false),
                    CreatorId = table.Column<Guid>(nullable: true),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    Provider = table.Column<byte>(nullable: false),
                    IsDynamicDiscounting = table.Column<bool>(nullable: false),
                    IsFactoring = table.Column<bool>(nullable: false),
                    IsRequiredRegistry = table.Column<bool>(nullable: false),
                    IsRequiredNotification = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contracts_Companies_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_Companies_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TariffArchives",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FromAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    UntilAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    FromDay = table.Column<int>(nullable: false),
                    UntilDay = table.Column<int>(nullable: true),
                    Rate = table.Column<decimal>(type: "numeric(6,3)", nullable: false),
                    CreatorId = table.Column<Guid>(nullable: false),
                    ActionTime = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    GroupId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TariffArchives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TariffArchives_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TariffArchives_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tariffs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FromAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    UntilAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    FromDay = table.Column<int>(nullable: false),
                    UntilDay = table.Column<int>(nullable: true),
                    Rate = table.Column<decimal>(type: "numeric(6,3)", nullable: false),
                    Type = table.Column<byte>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tariffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tariffs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Uploads",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(type: "varchar(1024)", nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "varchar(255)", nullable: false),
                    Provider = table.Column<byte>(nullable: false),
                    ProviderId = table.Column<Guid>(nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Uploads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Uploads_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRegulations",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Type = table.Column<byte>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRegulations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRegulations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    RoleId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                    table.UniqueConstraint("AK_UserRoles_UserId_RoleId", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccessTokenHash = table.Column<string>(nullable: true),
                    AccessTokenExpiresDateTime = table.Column<DateTimeOffset>(nullable: false),
                    RefreshTokenIdHash = table.Column<string>(type: "varchar(450)", nullable: false),
                    RefreshTokenIdHashSource = table.Column<string>(type: "varchar(450)", nullable: true),
                    RefreshTokenExpiresDateTime = table.Column<DateTimeOffset>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Registries",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Number = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Amount = table.Column<decimal>(nullable: false),
                    ContractId = table.Column<Guid>(nullable: false),
                    CreatorId = table.Column<Guid>(nullable: false),
                    Status = table.Column<byte>(nullable: false, defaultValue: (byte)1),
                    SignStatus = table.Column<byte>(nullable: false, defaultValue: (byte)1),
                    FinanceType = table.Column<byte>(nullable: false),
                    IsVerified = table.Column<bool>(nullable: false),
                    IsConfirmed = table.Column<bool>(nullable: false),
                    Remark = table.Column<string>(type: "varchar(4000)", nullable: true),
                    BankId = table.Column<Guid>(nullable: true),
                    FactoringAgreementId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Registries_Companies_BankId",
                        column: x => x.BankId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Registries_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Registries_Companies_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Registries_FactoringAgreements_FactoringAgreementId",
                        column: x => x.FactoringAgreementId,
                        principalTable: "FactoringAgreements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserProfileRegulationInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Number = table.Column<string>(type: "varchar(500)", nullable: true),
                    Citizenship = table.Column<string>(type: "varchar(500)", nullable: false),
                    PlaceOfBirth = table.Column<string>(type: "varchar(500)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(nullable: false),
                    AuthorityValidityDate = table.Column<DateTime>(nullable: true),
                    IdentityDocument = table.Column<string>(type: "varchar(500)", nullable: false),
                    IsResident = table.Column<bool>(nullable: false),
                    PassportSeries = table.Column<string>(nullable: true),
                    PassportDate = table.Column<DateTime>(nullable: true),
                    PassportNumber = table.Column<string>(nullable: true),
                    PassportUnitCode = table.Column<string>(nullable: true),
                    PassportIssuingAuthorityPSRN = table.Column<string>(nullable: true),
                    PassportSNILS = table.Column<string>(nullable: true),
                    MigrationCardRightToResideDocument = table.Column<string>(nullable: true),
                    MigrationCardAddress = table.Column<string>(nullable: true),
                    MigrationCardRegistrationAddress = table.Column<string>(nullable: true),
                    MigrationCardPhone = table.Column<string>(nullable: true),
                    UserRegulationId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfileRegulationInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProfileRegulationInfos_UserRegulations_UserRegulationId",
                        column: x => x.UserRegulationId,
                        principalTable: "UserRegulations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Discounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    PlannedPaymentDate = table.Column<DateTime>(nullable: false),
                    AmountToPay = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DiscountedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(6,3)", nullable: false),
                    DiscountingSource = table.Column<byte>(nullable: false),
                    HasChanged = table.Column<bool>(nullable: false),
                    RegistryId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Discounts_Registries_RegistryId",
                        column: x => x.RegistryId,
                        principalTable: "Registries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Signatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    SignerId = table.Column<Guid>(nullable: false),
                    Type = table.Column<byte>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Size = table.Column<long>(nullable: false),
                    CompanyRegulationId = table.Column<Guid>(nullable: true),
                    RegistryId = table.Column<Guid>(nullable: true),
                    UnformalizedDocumentId = table.Column<Guid>(nullable: true),
                    UploadId = table.Column<Guid>(nullable: true),
                    UserRegulationId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Signatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Signatures_CompanyRegulations_CompanyRegulationId",
                        column: x => x.CompanyRegulationId,
                        principalTable: "CompanyRegulations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Signatures_Registries_RegistryId",
                        column: x => x.RegistryId,
                        principalTable: "Registries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Signatures_Users_SignerId",
                        column: x => x.SignerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Signatures_UnformalizedDocuments_UnformalizedDocumentId",
                        column: x => x.UnformalizedDocumentId,
                        principalTable: "UnformalizedDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Signatures_Uploads_UploadId",
                        column: x => x.UploadId,
                        principalTable: "Uploads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Signatures_UserRegulations_UserRegulationId",
                        column: x => x.UserRegulationId,
                        principalTable: "UserRegulations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Supplies",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Number = table.Column<string>(type: "varchar(150)", nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Type = table.Column<byte>(nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ContractId = table.Column<Guid>(nullable: false),
                    CreatorId = table.Column<Guid>(nullable: false),
                    BaseDocumentId = table.Column<Guid>(nullable: true),
                    BaseDocumentNumber = table.Column<string>(type: "varchar(150)", nullable: true),
                    BaseDocumentType = table.Column<byte>(nullable: false),
                    BaseDocumentDate = table.Column<DateTime>(nullable: true),
                    ContractNumber = table.Column<string>(type: "varchar(150)", nullable: false),
                    ContractDate = table.Column<DateTime>(nullable: false),
                    DelayEndDate = table.Column<DateTime>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    SupplyId = table.Column<Guid>(nullable: false),
                    RegistryId = table.Column<Guid>(nullable: true),
                    IsAccepted = table.Column<bool>(nullable: false),
                    Provider = table.Column<byte>(nullable: false),
                    Status = table.Column<byte>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: true),
                    HasVerification = table.Column<bool>(nullable: false),
                    BankId = table.Column<Guid>(nullable: true),
                    FactoringAgreementId = table.Column<Guid>(nullable: true),
                    AddedBySeller = table.Column<bool>(nullable: false),
                    SellerVerified = table.Column<bool>(nullable: false),
                    BuyerVerified = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supplies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Supplies_Companies_BankId",
                        column: x => x.BankId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Supplies_Supplies_BaseDocumentId",
                        column: x => x.BaseDocumentId,
                        principalTable: "Supplies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Supplies_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Supplies_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Supplies_FactoringAgreements_FactoringAgreementId",
                        column: x => x.FactoringAgreementId,
                        principalTable: "FactoringAgreements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Supplies_Registries_RegistryId",
                        column: x => x.RegistryId,
                        principalTable: "Registries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SignatureInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Serial = table.Column<string>(type: "varchar(1000)", nullable: true),
                    Thumbprint = table.Column<string>(type: "varchar(2000)", nullable: true),
                    ValidFrom = table.Column<DateTime>(nullable: true),
                    ValidTill = table.Column<DateTime>(nullable: true),
                    Company = table.Column<string>(type: "varchar(1000)", nullable: true),
                    Name = table.Column<string>(type: "varchar(1000)", nullable: true),
                    Email = table.Column<string>(type: "varchar(400)", nullable: true),
                    INN = table.Column<string>(type: "varchar(20)", nullable: true),
                    OGRN = table.Column<string>(type: "varchar(20)", nullable: true),
                    SNILS = table.Column<string>(type: "varchar(20)", nullable: true),
                    SignatureId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignatureInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SignatureInfos_Signatures_SignatureId",
                        column: x => x.SignatureId,
                        principalTable: "Signatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplyDiscounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(6,3)", nullable: false),
                    DiscountedAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SupplyId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplyDiscounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplyDiscounts_Supplies_SupplyId",
                        column: x => x.SupplyId,
                        principalTable: "Supplies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Audits_UserId",
                table: "Audits",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BuyerTemplateConnections_BankId",
                table: "BuyerTemplateConnections",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_BuyerTemplateConnections_BuyerId",
                table: "BuyerTemplateConnections",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_BuyerTemplateConnections_TemplateId",
                table: "BuyerTemplateConnections",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_TIN",
                table: "Companies",
                column: "TIN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyAuthorizedUserInfos_CompanyId",
                table: "CompanyAuthorizedUserInfos",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyBankInfos_CompanyId",
                table: "CompanyBankInfos",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyBankInfos_UserId",
                table: "CompanyBankInfos",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyContactInfos_CompanyId",
                table: "CompanyContactInfos",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyPositionInfos_CompanyId",
                table: "CompanyPositionInfos",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyRegulations_CompanyId",
                table: "CompanyRegulations",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyRegulations_UserId",
                table: "CompanyRegulations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySettings_CompanyId",
                table: "CompanySettings",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanySettings_UserId",
                table: "CompanySettings",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_BuyerId",
                table: "Contracts",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_CreatorId",
                table: "Contracts",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_SellerId_BuyerId",
                table: "Contracts",
                columns: new[] { "SellerId", "BuyerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Discounts_RegistryId",
                table: "Discounts",
                column: "RegistryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiscountSettings_CompanyId",
                table: "DiscountSettings",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FactoringAgreements_BankId",
                table: "FactoringAgreements",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_FactoringAgreements_CompanyId",
                table: "FactoringAgreements",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_MigrationCardInfos_CompanyId",
                table: "MigrationCardInfos",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_RoleId",
                table: "Permissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Registries_BankId",
                table: "Registries",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_Registries_ContractId",
                table: "Registries",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Registries_CreatorId",
                table: "Registries",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Registries_FactoringAgreementId",
                table: "Registries",
                column: "FactoringAgreementId");

            migrationBuilder.CreateIndex(
                name: "IX_Regulations_Type",
                table: "Regulations",
                column: "Type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResidentPassportInfos_CompanyId",
                table: "ResidentPassportInfos",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SignatureInfos_SignatureId",
                table: "SignatureInfos",
                column: "SignatureId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Signatures_CompanyRegulationId",
                table: "Signatures",
                column: "CompanyRegulationId");

            migrationBuilder.CreateIndex(
                name: "IX_Signatures_RegistryId",
                table: "Signatures",
                column: "RegistryId");

            migrationBuilder.CreateIndex(
                name: "IX_Signatures_SignerId",
                table: "Signatures",
                column: "SignerId");

            migrationBuilder.CreateIndex(
                name: "IX_Signatures_UnformalizedDocumentId",
                table: "Signatures",
                column: "UnformalizedDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Signatures_UploadId",
                table: "Signatures",
                column: "UploadId");

            migrationBuilder.CreateIndex(
                name: "IX_Signatures_UserRegulationId",
                table: "Signatures",
                column: "UserRegulationId");

            migrationBuilder.CreateIndex(
                name: "IX_Supplies_BankId",
                table: "Supplies",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_Supplies_BaseDocumentId",
                table: "Supplies",
                column: "BaseDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Supplies_ContractId",
                table: "Supplies",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Supplies_CreatorId",
                table: "Supplies",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Supplies_FactoringAgreementId",
                table: "Supplies",
                column: "FactoringAgreementId");

            migrationBuilder.CreateIndex(
                name: "IX_Supplies_RegistryId",
                table: "Supplies",
                column: "RegistryId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyDiscounts_SupplyId",
                table: "SupplyDiscounts",
                column: "SupplyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplyFactoringAgreements_FactoringAgreementId",
                table: "SupplyFactoringAgreements",
                column: "FactoringAgreementId");

            migrationBuilder.CreateIndex(
                name: "IX_TariffArchives_CreatorId",
                table: "TariffArchives",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_TariffArchives_UserId",
                table: "TariffArchives",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tariffs_UserId",
                table: "Tariffs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_CompanyId",
                table: "Templates",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_UnformalizedDocumentReceivers_ReceiverId",
                table: "UnformalizedDocumentReceivers",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_UnformalizedDocumentReceivers_UnformalizedDocumentId",
                table: "UnformalizedDocumentReceivers",
                column: "UnformalizedDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_UnformalizedDocuments_DeclinedBy",
                table: "UnformalizedDocuments",
                column: "DeclinedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UnformalizedDocuments_SenderId",
                table: "UnformalizedDocuments",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Uploads_UserId",
                table: "Uploads",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfileRegulationInfos_UserRegulationId",
                table: "UserProfileRegulationInfos",
                column: "UserRegulationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRegulations_UserId",
                table: "UserRegulations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CompanyId",
                table: "Users",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTokens_UserId",
                table: "UserTokens",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Audits");

            migrationBuilder.DropTable(
                name: "BuyerTemplateConnections");

            migrationBuilder.DropTable(
                name: "CompanyAuthorizedUserInfos");

            migrationBuilder.DropTable(
                name: "CompanyBankInfos");

            migrationBuilder.DropTable(
                name: "CompanyContactInfos");

            migrationBuilder.DropTable(
                name: "CompanyPositionInfos");

            migrationBuilder.DropTable(
                name: "CompanySettings");

            migrationBuilder.DropTable(
                name: "Discounts");

            migrationBuilder.DropTable(
                name: "DiscountSettings");

            migrationBuilder.DropTable(
                name: "FreeDays");

            migrationBuilder.DropTable(
                name: "MigrationCardInfos");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Regulations");

            migrationBuilder.DropTable(
                name: "ResidentPassportInfos");

            migrationBuilder.DropTable(
                name: "SignatureInfos");

            migrationBuilder.DropTable(
                name: "SupplyDiscounts");

            migrationBuilder.DropTable(
                name: "SupplyFactoringAgreements");

            migrationBuilder.DropTable(
                name: "TariffArchives");

            migrationBuilder.DropTable(
                name: "Tariffs");

            migrationBuilder.DropTable(
                name: "UnformalizedDocumentReceivers");

            migrationBuilder.DropTable(
                name: "UserProfileRegulationInfos");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "UserTokens");

            migrationBuilder.DropTable(
                name: "Templates");

            migrationBuilder.DropTable(
                name: "Signatures");

            migrationBuilder.DropTable(
                name: "Supplies");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "CompanyRegulations");

            migrationBuilder.DropTable(
                name: "UnformalizedDocuments");

            migrationBuilder.DropTable(
                name: "Uploads");

            migrationBuilder.DropTable(
                name: "UserRegulations");

            migrationBuilder.DropTable(
                name: "Registries");

            migrationBuilder.DropTable(
                name: "Contracts");

            migrationBuilder.DropTable(
                name: "FactoringAgreements");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
