using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Domain.Entities.Ticketing;

namespace Helpio.Dashboard.Services
{
    public interface IVariableReplacementService
    {
        Task<string> ReplaceVariablesAsync(string content, Ticket ticket, User currentUser);
    }

    public class VariableReplacementService : IVariableReplacementService
    {
        public async Task<string> ReplaceVariablesAsync(string content, Ticket ticket, User currentUser)
        {
            if (string.IsNullOrEmpty(content))
                return content;

            var replacedContent = content;

            //TODO : Add more variable replacements as needed
            //TODO : Handle null values gracefully
            //TODO : Consider localization for date and time formats
            //TODO : Relace Hardcoded values with configuration settings
            // جایگزینی متغیرهای مربوط به مشتری
            if (ticket.Customer != null)
            {
                replacedContent = replacedContent.Replace("{نام_مشتری}",
                    $"{ticket.Customer.FirstName} {ticket.Customer.LastName}".Trim());
                replacedContent = replacedContent.Replace("{نام_شرکت}",
                    ticket.Customer.CompanyName ?? "");
                replacedContent = replacedContent.Replace("{ایمیل_مشتری}",
                    ticket.Customer.Email ?? "");
                replacedContent = replacedContent.Replace("{تلفن_مشتری}",
                    ticket.Customer.PhoneNumber ?? "");
            }

            // جایگزینی متغیرهای مربوط به تیکت
            replacedContent = replacedContent.Replace("{شماره_تیکت}", ticket.Id.ToString());
            replacedContent = replacedContent.Replace("{عنوان_تیکت}", ticket.Title ?? "");
            replacedContent = replacedContent.Replace("{دسته_بندی_تیکت}", ticket.TicketCategory?.Name ?? "");

            // جایگزینی متغیرهای مربوط به کارشناس
            if (currentUser != null)
            {
                replacedContent = replacedContent.Replace("{نام_کارشناس}",
                    $"{currentUser.FirstName} {currentUser.LastName}".Trim());
                replacedContent = replacedContent.Replace("{ایمیل_کارشناس}",
                    currentUser.Email ?? "");
            }

            // جایگزینی متغیرهای زمانی
            var now = DateTime.Now;
            var persianDate = ConvertToPersianDate(now);

            replacedContent = replacedContent.Replace("{تاریخ_امروز}", persianDate);
            replacedContent = replacedContent.Replace("{زمان_فعلی}", now.ToString("HH:mm"));
            replacedContent = replacedContent.Replace("{تاریخ_و_زمان}", $"{persianDate} {now:HH:mm}");

            // جایگزینی متغیرهای سیستمی
            replacedContent = replacedContent.Replace("{نام_سایت}", "Helpio");
            replacedContent = replacedContent.Replace("{آدرس_سایت}", "https://helpio.io");

            return await Task.FromResult(replacedContent);
        }

        private string ConvertToPersianDate(DateTime date)
        {
            try
            {
                var persianCalendar = new System.Globalization.PersianCalendar();
                var year = persianCalendar.GetYear(date);
                var month = persianCalendar.GetMonth(date);
                var day = persianCalendar.GetDayOfMonth(date);

                return $"{year:0000}/{month:00}/{day:00}";
            }
            catch
            {
                return date.ToString("yyyy/MM/dd");
            }
        }
    }
}