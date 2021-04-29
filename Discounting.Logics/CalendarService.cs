using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discounting.Data.Context;
using Discounting.Entities;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;

namespace Discounting.Logics
{
    public interface ICalendarService
    {
        Task<FreeDay[]> GetFreeDaysAsync(DateTime? from, DateTime? until, bool withDeactivated);
        Task<List<FreeDay>> SaveFreeDaysAsync(List<FreeDay> freeDays);
    }

    public class CalendarService : ICalendarService
    {
        private readonly IUnitOfWork unitOfWork;

        public CalendarService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public Task<FreeDay[]> GetFreeDaysAsync(DateTime? from, DateTime? until, bool withDeactivated)
        {
            return unitOfWork.Set<FreeDay>()
                .Where(d => (withDeactivated || d.IsActive) &&
                            (!from.HasValue || d.Date.Date >= from.Value.Date) &&
                            (!until.HasValue || d.Date.Date <= until.Value.Date))
                .ToArrayAsync();
            ;
        }

        public async Task<List<FreeDay>> SaveFreeDaysAsync(List<FreeDay> freeDays)
        {
            var addedDays = new List<FreeDay>();
            var updatedDays = new List<FreeDay>();
            foreach (var freeDay in freeDays)
            {
                if (freeDay.Id == default)
                {
                    freeDay.CreatedAt = DateTime.UtcNow;
                    addedDays.Add(freeDay);
                }
                else
                {
                    if (!freeDay.IsActive)
                    {
                        freeDay.DeactivatedAt = DateTime.UtcNow;
                    }

                    updatedDays.Add(freeDay);
                }
            }

            await unitOfWork.Set<FreeDay>().AddRangeAsync(addedDays);
            unitOfWork.Set<FreeDay>().UpdateRange(updatedDays);
            await unitOfWork.SaveChangesAsync();
            return freeDays;
        }
    }
}