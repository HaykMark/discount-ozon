using System;
using System.Collections.Generic;
using Discounting.Common.Exceptions;
using Discounting.Entities.TariffDiscounting;
using Discounting.Logics.Validators;
using Moq;
using Xunit;

namespace Discounting.Tests.UnitTests
{
    public class DiscountPaymentDateTests
    {
        [Fact]
        public void PlannedPaymentDateValidation_Calendar_PaymentDateIsCorrect_NoException()
        {
            //Mock
            var discountValidator = new Mock<DiscountValidator>(null);

            //Preparation
            var plannedPaymentDate = new DateTime(2020, 10, 21);
            var discountSettings = new DiscountSettings
            {
                DaysType = DaysType.Calendar,
                PaymentWeekDays = DaysOfWeek.Wednesday,
                MinimumDaysToShift = 3
            };

            //Weekends
            var freeDays = new HashSet<DateTime>
            {
                new DateTime(2020, 10, 17),
                new DateTime(2020, 10, 18),
            };

            //Act
            discountValidator.Object.ValidatePlannedPaymentDate(plannedPaymentDate, discountSettings, freeDays);
        }

        [Fact]
        public void PlannedPaymentDateValidation_Calendar_PaymentDateIsWrong_NoException()
        {
            //Mock
            var discountValidator = new Mock<DiscountValidator>(null);

            //Preparation
            var plannedPaymentDate = new DateTime(2020, 10, 22);
            var discountSettings = new DiscountSettings
            {
                DaysType = DaysType.Calendar,
                PaymentWeekDays = DaysOfWeek.Wednesday,
                MinimumDaysToShift = 3
            };

            //Weekends
            var freeDays = new HashSet<DateTime>
            {
                new DateTime(2020, 10, 17),
                new DateTime(2020, 10, 18),
            };

            //Act
            Assert.Throws<ValidationException>(() =>
                discountValidator.Object.ValidatePlannedPaymentDate(plannedPaymentDate, discountSettings, freeDays));
        }
        
        [Fact]
        public void PlannedPaymentDateValidation_Business_PaymentDateIsCorrect_NoException()
        {
            //Mock
            var discountValidator = new Mock<DiscountValidator>(null);

            //Preparation
            var plannedPaymentDate = new DateTime(2020, 10, 24);
            var discountSettings = new DiscountSettings
            {
                DaysType = DaysType.Business,
                PaymentWeekDays = DaysOfWeek.Wednesday | DaysOfWeek.Saturday,
                MinimumDaysToShift = 5
            };

            //Weekends
            var freeDays = new HashSet<DateTime>
            {
                new DateTime(2020, 10, 17),
                new DateTime(2020, 10, 18),
            };

            //Act
            discountValidator.Object.ValidatePlannedPaymentDate(plannedPaymentDate, discountSettings, freeDays);
        }

        [Fact]
        public void PlannedPaymentDateValidation_Business_PaymentDateIsWrong_NoException()
        {
            //Mock
            var discountValidator = new Mock<DiscountValidator>(null);

            //Preparation
            var plannedPaymentDate = new DateTime(2020, 10, 21);
            var discountSettings = new DiscountSettings
            {
                DaysType = DaysType.Business,
                PaymentWeekDays =  DaysOfWeek.Wednesday | DaysOfWeek.Saturday,
                MinimumDaysToShift = 5
            };

            //Weekends
            var freeDays = new HashSet<DateTime>
            {
                new DateTime(2020, 10, 17),
                new DateTime(2020, 10, 18),
            };

            //Act
            Assert.Throws<ValidationException>(() =>
                discountValidator.Object.ValidatePlannedPaymentDate(plannedPaymentDate, discountSettings, freeDays));
        }
    }
}