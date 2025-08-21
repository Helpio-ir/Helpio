using Helpio.Ir.Application.DTOs.Business;
using Helpio.Ir.Application.DTOs.Common;

namespace Helpio.Ir.Application.Services.Business
{
    public interface ITransactionService
    {
        Task<TransactionDto?> GetByIdAsync(int id);
        Task<PaginatedResult<TransactionDto>> GetTransactionsAsync(PaginationRequest request);
        Task<TransactionDto> CreateAsync(CreateTransactionDto createDto);
        Task<TransactionDto> UpdateAsync(int id, UpdateTransactionDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<TransactionDto>> GetByTypeAsync(TransactionTypeDto type);
        Task<IEnumerable<TransactionDto>> GetByStatusAsync(TransactionStatusDto status);
        Task<IEnumerable<TransactionDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalAmountByTypeAsync(TransactionTypeDto type);
        Task<IEnumerable<TransactionDto>> GetPendingTransactionsAsync();
        Task<bool> ApproveTransactionAsync(int transactionId);
        Task<bool> RejectTransactionAsync(int transactionId, string reason);
        Task<bool> ProcessTransactionAsync(int transactionId);
        Task<Dictionary<string, decimal>> GetTransactionStatisticsAsync();
        Task<IEnumerable<TransactionDto>> GetRecentTransactionsAsync(int count = 10);
    }
}