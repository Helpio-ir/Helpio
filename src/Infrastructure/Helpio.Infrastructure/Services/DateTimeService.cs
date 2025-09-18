using Helpio.Ir.Application.Common.Interfaces;

namespace Helpio.Ir.Infrastructure.Services
{
    public class DateTimeService : IDateTime
    {
        public DateTime Now => DateTime.Now;
        public DateTime UtcNow => DateTime.UtcNow;
    }
}