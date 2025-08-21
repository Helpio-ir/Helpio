using Helpio.Ir.Domain.Entities;

namespace Helpio.Ir.Domain.Interfaces.Services
{
    public interface INotificationService
    {
        Task SendTicketAssignedNotificationAsync(int ticketId, int supportAgentId);
        Task SendTicketUpdatedNotificationAsync(int ticketId);
        Task SendNewResponseNotificationAsync(int responseId);
        Task SendTicketResolvedNotificationAsync(int ticketId);
        Task SendTicketOverdueNotificationAsync(int ticketId);
        Task SendWelcomeNotificationAsync(int userId);
        Task SendTicketEscalatedNotificationAsync(int ticketId, int newTeamId);
        Task SendBulkNotificationAsync(IEnumerable<int> userIds, string subject, string message);
        Task SendEmailNotificationAsync(string email, string subject, string message);
        Task SendSmsNotificationAsync(string phoneNumber, string message);
    }
}