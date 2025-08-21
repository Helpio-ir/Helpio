using Helpio.Ir.Domain.Entities.Business;
using Helpio.Ir.Domain.Interfaces.Repositories.Business;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Ir.Infrastructure.Repositories.Business
{
    public class TransactionRepository : Repository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Transaction>> GetByTypeAsync(TransactionType type)
        {
            return await _dbSet
                .Where(t => t.Type == type && !t.IsDeleted)
                .OrderByDescending(t => t.ProcessedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByStatusAsync(TransactionStatus status)
        {
            return await _dbSet
                .Where(t => t.Status == status && !t.IsDeleted)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(t => t.ProcessedAt >= startDate && t.ProcessedAt <= endDate && !t.IsDeleted)
                .OrderByDescending(t => t.ProcessedAt)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalAmountByTypeAsync(TransactionType type)
        {
            return await _dbSet
                .Where(t => t.Type == type && t.Status == TransactionStatus.Completed && !t.IsDeleted)
                .SumAsync(t => t.Amount);
        }

        public async Task<IEnumerable<Transaction>> GetPendingTransactionsAsync()
        {
            return await _dbSet
                .Where(t => t.Status == TransactionStatus.Pending && !t.IsDeleted)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();
        }
    }
}