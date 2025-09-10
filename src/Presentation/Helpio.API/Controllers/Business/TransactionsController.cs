using Microsoft.AspNetCore.Mvc;
using Helpio.Ir.Application.Services.Business;
using Helpio.Ir.Application.DTOs.Business;
using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.API.Services;
using FluentValidation;

namespace Helpio.Ir.API.Controllers.Business
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly IOrganizationContext _organizationContext;
        private readonly IValidator<CreateTransactionDto> _createValidator;
        private readonly IValidator<UpdateTransactionDto> _updateValidator;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(
            ITransactionService transactionService,
            IOrganizationContext organizationContext,
            IValidator<CreateTransactionDto> createValidator,
            IValidator<UpdateTransactionDto> updateValidator,
            ILogger<TransactionsController> logger)
        {
            _transactionService = transactionService;
            _organizationContext = organizationContext;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        /// <summary>
        /// Get all transactions with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<TransactionDto>>> GetTransactions([FromQuery] PaginationRequest request)
        {
            try
            {
                var result = await _transactionService.GetTransactionsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions");
                return BadRequest("Error retrieving transactions");
            }
        }

        /// <summary>
        /// Get transaction by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionDto>> GetTransaction(int id)
        {
            var transaction = await _transactionService.GetByIdAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            return Ok(transaction);
        }

        /// <summary>
        /// Create a new transaction
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TransactionDto>> CreateTransaction(CreateTransactionDto createDto)
        {
            // Validate input
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _transactionService.CreateAsync(createDto);
                
                _logger.LogInformation("Transaction created: {TransactionId} with amount: {Amount}", 
                    result.Id, createDto.Amount);

                return CreatedAtAction(nameof(GetTransaction), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transaction with amount: {Amount}", createDto.Amount);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update transaction
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<TransactionDto>> UpdateTransaction(int id, UpdateTransactionDto updateDto)
        {
            // Validate input
            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _transactionService.UpdateAsync(id, updateDto);
                
                _logger.LogInformation("Transaction updated: {TransactionId}", id);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transaction: {TransactionId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete transaction (only pending transactions)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            try
            {
                var result = await _transactionService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                _logger.LogInformation("Transaction deleted: {TransactionId}", id);
                
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Attempt to delete non-pending transaction: {TransactionId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transaction: {TransactionId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get transactions by type
        /// </summary>
        [HttpGet("type/{type}")]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactionsByType(TransactionTypeDto type)
        {
            try
            {
                var transactions = await _transactionService.GetByTypeAsync(type);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions by type: {Type}", type);
                return BadRequest("Error retrieving transactions");
            }
        }

        /// <summary>
        /// Get transactions by status
        /// </summary>
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactionsByStatus(TransactionStatusDto status)
        {
            try
            {
                var transactions = await _transactionService.GetByStatusAsync(status);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions by status: {Status}", status);
                return BadRequest("Error retrieving transactions");
            }
        }

        /// <summary>
        /// Get transactions within a date range
        /// </summary>
        [HttpGet("date-range")]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactionsByDateRange(
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            try
            {
                var transactions = await _transactionService.GetByDateRangeAsync(startDate, endDate);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions by date range: {StartDate} to {EndDate}", startDate, endDate);
                return BadRequest("Error retrieving transactions");
            }
        }

        /// <summary>
        /// Get pending transactions
        /// </summary>
        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetPendingTransactions()
        {
            try
            {
                var transactions = await _transactionService.GetPendingTransactionsAsync();
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending transactions");
                return BadRequest("Error retrieving pending transactions");
            }
        }

        /// <summary>
        /// Get recent transactions
        /// </summary>
        [HttpGet("recent")]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetRecentTransactions([FromQuery] int count = 10)
        {
            if (count <= 0 || count > 100)
            {
                return BadRequest("Count must be between 1 and 100");
            }

            try
            {
                var transactions = await _transactionService.GetRecentTransactionsAsync(count);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent transactions");
                return BadRequest("Error retrieving recent transactions");
            }
        }

        /// <summary>
        /// Get total amount by transaction type
        /// </summary>
        [HttpGet("total-amount/{type}")]
        public async Task<ActionResult<decimal>> GetTotalAmountByType(TransactionTypeDto type)
        {
            try
            {
                var totalAmount = await _transactionService.GetTotalAmountByTypeAsync(type);
                return Ok(new { type, totalAmount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total amount for type: {Type}", type);
                return BadRequest("Error retrieving total amount");
            }
        }

        /// <summary>
        /// Approve a pending transaction
        /// </summary>
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveTransaction(int id)
        {
            try
            {
                var result = await _transactionService.ApproveTransactionAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                _logger.LogInformation("Transaction {TransactionId} approved", id);
                
                return Ok(new { message = "Transaction approved successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Attempt to approve non-pending transaction: {TransactionId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving transaction: {TransactionId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Reject a pending transaction
        /// </summary>
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectTransaction(int id, [FromBody] string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return BadRequest("Rejection reason is required");
            }

            try
            {
                var result = await _transactionService.RejectTransactionAsync(id, reason);
                if (!result)
                {
                    return NotFound();
                }

                _logger.LogInformation("Transaction {TransactionId} rejected with reason: {Reason}", id, reason);
                
                return Ok(new { message = "Transaction rejected successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Attempt to reject non-pending transaction: {TransactionId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting transaction: {TransactionId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Process a pending transaction
        /// </summary>
        [HttpPost("{id}/process")]
        public async Task<IActionResult> ProcessTransaction(int id)
        {
            try
            {
                var result = await _transactionService.ProcessTransactionAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                _logger.LogInformation("Transaction {TransactionId} processed", id);
                
                return Ok(new { message = "Transaction processed successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Attempt to process non-pending transaction: {TransactionId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing transaction: {TransactionId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get transaction statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<Dictionary<string, decimal>>> GetTransactionStatistics()
        {
            try
            {
                var statistics = await _transactionService.GetTransactionStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction statistics");
                return BadRequest("Error retrieving transaction statistics");
            }
        }

        /// <summary>
        /// Get transaction volume summary
        /// </summary>
        [HttpGet("volume-summary")]
        public async Task<IActionResult> GetVolumeSummary()
        {
            try
            {
                var statistics = await _transactionService.GetTransactionStatisticsAsync();
                
                var summary = new
                {
                    TotalVolume = statistics.GetValueOrDefault("TotalVolume", 0),
                    MonthlyVolume = statistics.GetValueOrDefault("MonthlyVolume", 0),
                    DailyVolume = statistics.GetValueOrDefault("DailyVolume", 0),
                    AverageTransaction = statistics.GetValueOrDefault("AverageTransaction", 0),
                    PendingAmount = statistics.GetValueOrDefault("PendingAmount", 0),
                    FailedAmount = statistics.GetValueOrDefault("FailedAmount", 0)
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction volume summary");
                return BadRequest("Error retrieving volume summary");
            }
        }
    }
}