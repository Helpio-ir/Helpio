using AutoMapper;
using Microsoft.Extensions.Logging;
using Helpio.Ir.Application.DTOs.Business;
using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.Application.Services.Business;
using Helpio.Ir.Application.Common.Exceptions;
using Helpio.Ir.Application.Common.Interfaces;
using Helpio.Ir.Domain.Interfaces;
using Helpio.Ir.Domain.Entities.Business;

namespace Helpio.Ir.Application.Services.Business
{
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<TransactionService> _logger;
        private readonly IDateTime _dateTime;

        public TransactionService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<TransactionService> logger,
            IDateTime dateTime)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _dateTime = dateTime;
        }

        public async Task<TransactionDto?> GetByIdAsync(int id)
        {
            var transaction = await _unitOfWork.Transactions.GetByIdAsync(id);
            return transaction != null ? _mapper.Map<TransactionDto>(transaction) : null;
        }

        public async Task<PaginatedResult<TransactionDto>> GetTransactionsAsync(PaginationRequest request)
        {
            var transactions = await _unitOfWork.Transactions.GetAllAsync();

            // Apply search filter
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                transactions = transactions.Where(t =>
                    (t.Description != null && t.Description.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (t.Reference != null && t.Reference.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            // Apply sorting
            transactions = request.SortBy?.ToLower() switch
            {
                "amount" => request.SortDescending ? transactions.OrderByDescending(t => t.Amount) : transactions.OrderBy(t => t.Amount),
                "type" => request.SortDescending ? transactions.OrderByDescending(t => t.Type) : transactions.OrderBy(t => t.Type),
                "status" => request.SortDescending ? transactions.OrderByDescending(t => t.Status) : transactions.OrderBy(t => t.Status),
                "processedat" => request.SortDescending ? transactions.OrderByDescending(t => t.ProcessedAt) : transactions.OrderBy(t => t.ProcessedAt),
                "createdat" => request.SortDescending ? transactions.OrderByDescending(t => t.CreatedAt) : transactions.OrderBy(t => t.CreatedAt),
                _ => transactions.OrderByDescending(t => t.CreatedAt)
            };

            var totalItems = transactions.Count();
            var items = transactions
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var transactionDtos = _mapper.Map<List<TransactionDto>>(items);

            return new PaginatedResult<TransactionDto>
            {
                Items = transactionDtos,
                TotalItems = totalItems,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<TransactionDto> CreateAsync(CreateTransactionDto createDto)
        {
            var transaction = _mapper.Map<Transaction>(createDto);
            transaction.Reference = GenerateReferenceNumber();
            transaction.ProcessedAt = _dateTime.UtcNow;

            var createdTransaction = await _unitOfWork.Transactions.AddAsync(transaction);

            _logger.LogInformation("Transaction created with ID: {TransactionId}, Reference: {Reference}, Amount: {Amount}", 
                createdTransaction.Id, createdTransaction.Reference, createdTransaction.Amount);

            return _mapper.Map<TransactionDto>(createdTransaction);
        }

        public async Task<TransactionDto> UpdateAsync(int id, UpdateTransactionDto updateDto)
        {
            var transaction = await _unitOfWork.Transactions.GetByIdAsync(id);
            if (transaction == null)
            {
                throw new NotFoundException("Transaction", id);
            }

            var oldStatus = transaction.Status;
            _mapper.Map(updateDto, transaction);

            // Set processed date if status changed to completed
            if (oldStatus != TransactionStatus.Completed && updateDto.Status == TransactionStatusDto.Completed)
            {
                transaction.ProcessedAt = _dateTime.UtcNow;
            }

            await _unitOfWork.Transactions.UpdateAsync(transaction);

            _logger.LogInformation("Transaction updated with ID: {TransactionId}, Status: {Status}", id, updateDto.Status);

            return _mapper.Map<TransactionDto>(transaction);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var transaction = await _unitOfWork.Transactions.GetByIdAsync(id);
            if (transaction == null)
            {
                return false;
            }

            // Only allow deletion of pending transactions
            if (transaction.Status != TransactionStatus.Pending)
            {
                throw new InvalidOperationException("Only pending transactions can be deleted");
            }

            await _unitOfWork.Transactions.DeleteAsync(transaction);

            _logger.LogInformation("Transaction deleted with ID: {TransactionId}", id);

            return true;
        }

        public async Task<IEnumerable<TransactionDto>> GetByTypeAsync(TransactionTypeDto type)
        {
            var domainType = _mapper.Map<TransactionType>(type);
            var transactions = await _unitOfWork.Transactions.GetByTypeAsync(domainType);
            return _mapper.Map<IEnumerable<TransactionDto>>(transactions);
        }

        public async Task<IEnumerable<TransactionDto>> GetByStatusAsync(TransactionStatusDto status)
        {
            var domainStatus = _mapper.Map<TransactionStatus>(status);
            var transactions = await _unitOfWork.Transactions.GetByStatusAsync(domainStatus);
            return _mapper.Map<IEnumerable<TransactionDto>>(transactions);
        }

        public async Task<IEnumerable<TransactionDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var transactions = await _unitOfWork.Transactions.GetByDateRangeAsync(startDate, endDate);
            return _mapper.Map<IEnumerable<TransactionDto>>(transactions);
        }

        public async Task<decimal> GetTotalAmountByTypeAsync(TransactionTypeDto type)
        {
            var domainType = _mapper.Map<TransactionType>(type);
            return await _unitOfWork.Transactions.GetTotalAmountByTypeAsync(domainType);
        }

        public async Task<IEnumerable<TransactionDto>> GetPendingTransactionsAsync()
        {
            var transactions = await _unitOfWork.Transactions.GetPendingTransactionsAsync();
            return _mapper.Map<IEnumerable<TransactionDto>>(transactions);
        }

        public async Task<bool> ApproveTransactionAsync(int transactionId)
        {
            var transaction = await _unitOfWork.Transactions.GetByIdAsync(transactionId);
            if (transaction == null)
            {
                throw new NotFoundException("Transaction", transactionId);
            }

            if (transaction.Status != TransactionStatus.Pending)
            {
                throw new InvalidOperationException("Only pending transactions can be approved");
            }

            // Note: Domain Transaction doesn't have Processing status, so we complete directly
            transaction.Status = TransactionStatus.Completed;
            transaction.ProcessedAt = _dateTime.UtcNow;
            await _unitOfWork.Transactions.UpdateAsync(transaction);

            _logger.LogInformation("Transaction {TransactionId} approved and completed", transactionId);

            return true;
        }

        public async Task<bool> RejectTransactionAsync(int transactionId, string reason)
        {
            var transaction = await _unitOfWork.Transactions.GetByIdAsync(transactionId);
            if (transaction == null)
            {
                throw new NotFoundException("Transaction", transactionId);
            }

            if (transaction.Status != TransactionStatus.Pending)
            {
                throw new InvalidOperationException("Only pending transactions can be rejected");
            }

            transaction.Status = TransactionStatus.Failed;
            transaction.Description = transaction.Description + $" | Rejection Reason: {reason}";
            transaction.ProcessedAt = _dateTime.UtcNow;
            await _unitOfWork.Transactions.UpdateAsync(transaction);

            _logger.LogInformation("Transaction {TransactionId} rejected with reason: {Reason}", transactionId, reason);

            return true;
        }

        public async Task<bool> ProcessTransactionAsync(int transactionId)
        {
            var transaction = await _unitOfWork.Transactions.GetByIdAsync(transactionId);
            if (transaction == null)
            {
                throw new NotFoundException("Transaction", transactionId);
            }

            if (transaction.Status != TransactionStatus.Pending)
            {
                throw new InvalidOperationException("Only pending transactions can be processed");
            }

            transaction.Status = TransactionStatus.Completed;
            transaction.ProcessedAt = _dateTime.UtcNow;
            await _unitOfWork.Transactions.UpdateAsync(transaction);

            _logger.LogInformation("Transaction {TransactionId} processed successfully", transactionId);

            return true;
        }

        public async Task<Dictionary<string, decimal>> GetTransactionStatisticsAsync()
        {
            var allTransactions = await _unitOfWork.Transactions.GetAllAsync();
            var completedTransactions = allTransactions.Where(t => t.Status == TransactionStatus.Completed);

            return new Dictionary<string, decimal>
            {
                ["TotalVolume"] = completedTransactions.Sum(t => t.Amount),
                ["NetVolume"] = completedTransactions.Sum(t => t.Amount), // No fees in current model
                ["MonthlyVolume"] = completedTransactions.Where(t => t.ProcessedAt >= _dateTime.UtcNow.AddMonths(-1)).Sum(t => t.Amount),
                ["DailyVolume"] = completedTransactions.Where(t => t.ProcessedAt >= _dateTime.UtcNow.AddDays(-1)).Sum(t => t.Amount),
                ["AverageTransaction"] = completedTransactions.Any() ? completedTransactions.Average(t => t.Amount) : 0,
                ["PendingAmount"] = allTransactions.Where(t => t.Status == TransactionStatus.Pending).Sum(t => t.Amount),
                ["FailedAmount"] = allTransactions.Where(t => t.Status == TransactionStatus.Failed).Sum(t => t.Amount)
            };
        }

        public async Task<IEnumerable<TransactionDto>> GetRecentTransactionsAsync(int count = 10)
        {
            var allTransactions = await _unitOfWork.Transactions.GetAllAsync();
            var recentTransactions = allTransactions
                .OrderByDescending(t => t.CreatedAt)
                .Take(count)
                .ToList();

            return _mapper.Map<IEnumerable<TransactionDto>>(recentTransactions);
        }

        #region Private Methods

        private static string GenerateReferenceNumber()
        {
            return $"TXN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }

        #endregion
    }
}