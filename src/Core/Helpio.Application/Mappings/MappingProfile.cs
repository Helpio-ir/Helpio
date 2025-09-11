using AutoMapper;
using System.Text.RegularExpressions;
using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Domain.Entities.Ticketing;
using Helpio.Ir.Domain.Entities.Business;
using Helpio.Ir.Domain.Entities.Knowledge;
using Helpio.Ir.Application.DTOs.Core;
using Helpio.Ir.Application.DTOs.Ticketing;
using Helpio.Ir.Application.DTOs.Business;
using Helpio.Ir.Application.DTOs.Knowledge;
using CoreProfile = Helpio.Ir.Domain.Entities.Core.Profile;

namespace Helpio.Ir.Application.Mappings
{
    public class MappingProfile : AutoMapper.Profile
    {
        public MappingProfile()
        {
            CreateCoreMappings();
            CreateTicketingMappings();
            CreateBusinessMappings();
            CreateKnowledgeMappings();
        }

        private void CreateCoreMappings()
        {
            // User mappings
            CreateMap<User, UserDto>();
            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Handle in service
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
            
            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // Customer mappings
            CreateMap<Customer, CustomerDto>()
                .ForMember(dest => dest.TotalTickets, opt => opt.MapFrom(src => src.Tickets.Count));
            CreateMap<CreateCustomerDto, Customer>();
            CreateMap<UpdateCustomerDto, Customer>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // Organization mappings
            CreateMap<Organization, OrganizationDto>()
                .ForMember(dest => dest.BranchCount, opt => opt.MapFrom(src => src.Branches.Count))
                .ForMember(dest => dest.CategoryCount, opt => opt.MapFrom(src => src.TicketCategories.Count));
            CreateMap<CreateOrganizationDto, Organization>();
            CreateMap<UpdateOrganizationDto, Organization>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // Branch mappings
            CreateMap<Branch, BranchDto>()
                .ForMember(dest => dest.TeamCount, opt => opt.MapFrom(src => src.Teams.Count));
            CreateMap<CreateBranchDto, Branch>();
            CreateMap<UpdateBranchDto, Branch>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // Team mappings
            CreateMap<Team, TeamDto>()
                .ForMember(dest => dest.AgentCount, opt => opt.MapFrom(src => src.SupportAgents.Count))
                .ForMember(dest => dest.ActiveTicketCount, opt => opt.MapFrom(src => src.Tickets.Count(t => t.ResolvedDate == null)));
            CreateMap<CreateTeamDto, Team>();
            CreateMap<UpdateTeamDto, Team>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.BranchId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // SupportAgent mappings
            CreateMap<SupportAgent, SupportAgentDto>();
            CreateMap<CreateSupportAgentDto, SupportAgent>();
            CreateMap<UpdateSupportAgentDto, SupportAgent>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.ProfileId, opt => opt.Ignore())
                .ForMember(dest => dest.AgentCode, opt => opt.Ignore())
                .ForMember(dest => dest.HireDate, opt => opt.Ignore())
                .ForMember(dest => dest.Salary, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // Profile mappings - using alias
            CreateMap<CoreProfile, ProfileDto>();
            CreateMap<CreateProfileDto, CoreProfile>();
            CreateMap<UpdateProfileDto, CoreProfile>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.LastLoginDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // ApiKey mappings
            CreateMap<ApiKey, ApiKeyDto>();
            CreateMap<CreateApiKeyDto, ApiKey>()
                .ForMember(dest => dest.KeyValue, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.KeyHash, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.LastUsedAt, opt => opt.Ignore());
            CreateMap<UpdateApiKeyDto, ApiKey>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
                .ForMember(dest => dest.KeyValue, opt => opt.Ignore())
                .ForMember(dest => dest.KeyHash, opt => opt.Ignore())
                .ForMember(dest => dest.LastUsedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
        }

        private void CreateTicketingMappings()
        {
            // Ticket mappings
            CreateMap<Ticket, TicketDto>()
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => (TicketPriorityDto)src.Priority))
                .ForMember(dest => dest.ResponseCount, opt => opt.MapFrom(src => src.Responses.Count))
                .ForMember(dest => dest.NoteCount, opt => opt.MapFrom(src => src.Notes.Count))
                .ForMember(dest => dest.AttachmentCount, opt => opt.MapFrom(src => src.Attachments.Count));
            
            CreateMap<CreateTicketDto, Ticket>()
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => (Domain.Entities.Ticketing.TicketPriority)src.Priority));
            
            CreateMap<UpdateTicketDto, Ticket>()
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => (Domain.Entities.Ticketing.TicketPriority)src.Priority))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
                .ForMember(dest => dest.TicketStateId, opt => opt.Ignore())
                .ForMember(dest => dest.TeamId, opt => opt.Ignore())
                .ForMember(dest => dest.TicketCategoryId, opt => opt.Ignore())
                .ForMember(dest => dest.ResolvedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Resolution, opt => opt.Ignore())
                .ForMember(dest => dest.ActualHours, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // TicketState mappings
            CreateMap<TicketState, TicketStateDto>()
                .ForMember(dest => dest.TicketCount, opt => opt.MapFrom(src => src.Tickets.Count));
            CreateMap<CreateTicketStateDto, TicketState>();
            CreateMap<UpdateTicketStateDto, TicketState>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // TicketCategory mappings
            CreateMap<TicketCategory, TicketCategoryDto>()
                .ForMember(dest => dest.TicketCount, opt => opt.MapFrom(src => src.Tickets.Count));
            CreateMap<CreateTicketCategoryDto, TicketCategory>();
            CreateMap<UpdateTicketCategoryDto, TicketCategory>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // Response mappings
            CreateMap<Response, ResponseDto>();
            CreateMap<CreateResponseDto, Response>();
            CreateMap<UpdateResponseDto, Response>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TicketId, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.IsFromCustomer, opt => opt.Ignore())
                .ForMember(dest => dest.ReadAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // Note mappings
            CreateMap<Note, NoteDto>();
            CreateMap<CreateNoteDto, Note>()
                .ForMember(dest => dest.IsSystemNote, opt => opt.MapFrom(src => false));
            CreateMap<UpdateNoteDto, Note>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TicketId, opt => opt.Ignore())
                .ForMember(dest => dest.SupportAgentId, opt => opt.Ignore())
                .ForMember(dest => dest.IsSystemNote, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // Attachment mappings
            CreateMap<Attachment, AttachmentDto>();
            CreateMap<CreateAttachmentDto, Attachment>();
        }

        private void CreateBusinessMappings()
        {
            // Order mappings
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (OrderStatusDto)src.Status));
            CreateMap<CreateOrderDto, Order>()
                .ForMember(dest => dest.OrderNumber, opt => opt.Ignore()) // Generate in service
                .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Domain.Entities.Business.OrderStatus.Pending));
            CreateMap<UpdateOrderDto, Order>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (Domain.Entities.Business.OrderStatus)src.Status))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.SubscriptionId, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
                .ForMember(dest => dest.OrderNumber, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.TaxAmount, opt => opt.Ignore())
                .ForMember(dest => dest.DiscountAmount, opt => opt.Ignore())
                .ForMember(dest => dest.OrderDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // Subscription mappings
            CreateMap<Subscription, SubscriptionDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (SubscriptionStatusDto)src.Status));
            CreateMap<CreateSubscriptionDto, Subscription>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Domain.Entities.Business.SubscriptionStatus.Active));
            CreateMap<UpdateSubscriptionDto, Subscription>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (Domain.Entities.Business.SubscriptionStatus)src.Status))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
                .ForMember(dest => dest.StartDate, opt => opt.Ignore())
                .ForMember(dest => dest.Currency, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // Transaction mappings
            CreateMap<Transaction, TransactionDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (TransactionTypeDto)src.Type))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (TransactionStatusDto)src.Status));
            CreateMap<CreateTransactionDto, Transaction>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (Domain.Entities.Business.TransactionType)src.Type))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Domain.Entities.Business.TransactionStatus.Pending))
                .ForMember(dest => dest.ProcessedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<UpdateTransactionDto, Transaction>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (Domain.Entities.Business.TransactionStatus)src.Status))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentId, opt => opt.Ignore())
                .ForMember(dest => dest.Amount, opt => opt.Ignore())
                .ForMember(dest => dest.Currency, opt => opt.Ignore())
                .ForMember(dest => dest.Type, opt => opt.Ignore())
                .ForMember(dest => dest.Reference, opt => opt.Ignore())
                .ForMember(dest => dest.ProcessedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
        }

        private void CreateKnowledgeMappings()
        {
            // CannedResponse mappings
            CreateMap<CannedResponse, CannedResponseDto>();
            CreateMap<CreateCannedResponseDto, CannedResponse>()
                .ForMember(dest => dest.UsageCount, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));
            CreateMap<UpdateCannedResponseDto, CannedResponse>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
                .ForMember(dest => dest.UsageCount, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            // Articles mappings
            CreateMap<Articles, ArticlesDto>();
            CreateMap<CreateArticlesDto, Articles>()
                .ForMember(dest => dest.ViewCount, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.PublishedAt, opt => opt.MapFrom(src => src.IsPublished ? DateTime.UtcNow : (DateTime?)null));
            CreateMap<UpdateArticlesDto, Articles>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
                .ForMember(dest => dest.AuthorId, opt => opt.Ignore())
                .ForMember(dest => dest.ViewCount, opt => opt.Ignore())
                .ForMember(dest => dest.PublishedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
        }

        public static string GenerateSlug(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Convert to lowercase
            string slug = input.ToLower();
            
            // Remove special characters
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            
            // Replace spaces with hyphens
            slug = Regex.Replace(slug, @"\s+", "-");
            
            // Remove multiple hyphens
            slug = Regex.Replace(slug, @"-+", "-");
            
            // Trim hyphens from start and end
            slug = slug.Trim('-');
            
            return slug;
        }
    }
}