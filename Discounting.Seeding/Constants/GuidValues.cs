using System;

namespace Discounting.Seeding.Constants
{
    public static class GuidValues
    {
        public static class UserGuids
        {
            public static readonly Guid Admin = new Guid("22a83cb2-fe81-4853-95ce-91d9e487affe");
            public static readonly Guid TestSeller = new Guid("e4d811f8-c9fc-4af2-8ee0-c72acd83fee6");
            public static readonly Guid TestBuyer = new Guid("bf672963-0b45-4cbf-aa3a-6ff6cccf8f04");
            public static readonly Guid TestSimpleUser = new Guid("3c3efa84-4ea8-4440-932b-2b27e8d968b1");
            public static readonly Guid BankUserOne = new Guid("d50e8e0c-c67e-449a-9a51-9ce21d5e2f6d");
            public static readonly Guid BankUserSecond = new Guid("c9d35bd5-b98c-49cc-b033-f2dd86e59686");
        }

        public static class RoleGuids
        {
            public static readonly Guid SimpleUser = new Guid("231fcb6b-7275-44f7-a2e7-59c06c7bb2b0");
            public static readonly Guid BankUser = new Guid("2056cd96-cc4d-4ab2-ad3b-58d4f80640bd");
            public static readonly Guid AdminRole = new Guid("8900eb56-931c-45d4-a377-0f040a04be28");
            public static readonly Guid InactiveCompanyRole = new Guid("ce3921ad-8f21-4235-89d0-3fed014e7ff3");
        }

        public static class CompanyGuids
        {
            public static readonly Guid Admin = new Guid("ff23e81d-0c04-4328-bb0e-66d430483871");
            public static readonly Guid TestSeller = new Guid("20ba6bf2-f4e8-418f-80e1-f27c42929a82");
            public static readonly Guid TestBuyer = new Guid("5bd966b1-b8f2-4c04-9b68-e838ddc5eef7");
            public static readonly Guid TestSimpleUser = new Guid("81f851e9-df5a-4f33-92cc-54025cc14431");
            public static readonly Guid BankUserOne = new Guid("7757111f-e59a-4314-91f2-8cfab10edbb7");
            public static readonly Guid BankUserSecond = new Guid("83c7101b-10c0-4127-ab0c-81409005d643");
        }

        public static class CompanySettingsGuids
        {
            public static readonly Guid Admin = new Guid("669fe959-269c-4bd0-8632-13a3aba175b8");
            public static readonly Guid TestSeller = new Guid("c7f68cb0-9fa7-4d9d-baaf-e450dcd5213f");
            public static readonly Guid TestBuyer = new Guid("22c696d2-56c0-4710-b1a2-047d07b4fd00");
        }

        public static class FactoringAgreementGuid
        {
            public static readonly Guid TestSellerBankTwo = new Guid("a91d6eb1-832a-4631-8a68-ee7656c084e5");
            public static readonly Guid TestBuyerBankOne = new Guid("d99d8154-294e-4ed6-916c-3e8cade53140");
        }

        public static class SupplyFactoringAgreementGuid
        {
            public static readonly Guid TestSellerBankTwoSupplyOne = new Guid("68c852f7-c1cd-4305-86e4-41d4d8cada8b");
            public static readonly Guid TestSellerBankTwoSupplyTwo = new Guid("a69bc8c4-251d-4277-8e3e-478cc6ac3554");
            public static readonly Guid TestBuyerBankOneSupplyOne = new Guid("85bef758-c5dc-446c-a199-933b58be4f1a");
            public static readonly Guid TestBuyerBankOneSupplyTwo = new Guid("5ed5451c-193c-47bb-8de1-3620c01e8915");
        }

        public static class ContractGuids
        {
            public static readonly Guid TestSeller = new Guid("6069d9af-8703-4d4b-b58a-106e2e15a8cf");
            public static readonly Guid TestBuyer = new Guid("4290f987-1e28-4942-8e0a-ce6f4665a695");
            public static readonly Guid TestSimpleUser = new Guid("4c1f32e6-bb1f-4b2e-8fdb-a7d5e2005c44");
        }

        public static class SupplyGuids
        {
            public static readonly Guid Torg12 = new Guid("9d1bc15d-443b-4c2a-a98c-6c5e02b612e3");
            public static readonly Guid Akt = new Guid("b6eb63b1-cb71-4336-b79c-10165bc847c5");
            public static readonly Guid Upd = new Guid("b39f1c36-d1e4-4c62-abf3-d340e4c11c84");
            public static readonly Guid Invoice = new Guid("ce2d5a89-f50d-4df4-a5eb-f9b0587c2681");
            public static readonly Guid Ukd = new Guid("a7252cea-369e-4912-99c6-dad8e14d3a2a");

            public static readonly Guid TorgSupplyId = new Guid("3488ad1c-82a3-421f-8e21-ab9d4ec8aade");
            public static readonly Guid AktSupplyId = new Guid("586599a7-f1ae-493b-ab83-4687864ca750");
            public static readonly Guid UpdSupplyId = new Guid("d1a29e90-722e-4497-908c-dbe6a713ee2c");
        }

        public static class TemplateGuids
        {
            public static readonly Guid Registry = new Guid("094b13a7-0089-4023-9fd3-1c26a0e9ca83");
            public static readonly Guid Verification = new Guid("b1bab6e5-180c-4522-ba54-e9c5adb012de");
            public static readonly Guid Discount = new Guid("f4ba6871-1aea-474f-ac3d-2150264ef31b");
            public static readonly Guid ProfileRegulationSellerBuyer = new Guid("85d0033e-4c38-4ccb-86c7-ebe9341741b7");

            public static readonly Guid ProfileRegulationPrivatePerson =
                new Guid("e89bf649-30b8-4f75-b4a0-de80f4b4e64a");

            public static readonly Guid ProfileRegulationBank = new Guid("5100755e-a9ea-4c15-ba3c-54daa14c3feb");
            public static readonly Guid ProfileRegulationUser = new Guid("63127c0c-6cfb-439d-9bef-89b3da7fe9e3");
        }

        public static class BuyerTemplateConnectionGuids
        {
            public static readonly Guid TestBuyerRegistry = new Guid("b0b82d00-9e53-489a-92cd-0e7ef9436d39");
            public static readonly Guid TestBuyerVerification = new Guid("f3a8ab16-8d64-4d59-b3ce-34b7552ce568");
        }
    }
}