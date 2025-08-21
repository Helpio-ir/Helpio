namespace Helpio.Ir.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        int? UserId { get; }
        string? UserEmail { get; }
        bool IsAuthenticated { get; }
    }
}