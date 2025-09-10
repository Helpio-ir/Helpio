# Controllers Folder Structure

This document outlines the organization of controllers in the Helpio.API project, which has been structured to match the Domain entities organization.

## Folder Structure

### Core Controllers (`/Controllers/Core/`)
Controllers for core system entities:
- **ApiKeysController.cs** - API key management (fully implemented)
- **OrganizationsController.cs** - Organization management 
- **CustomersController.cs** - Customer management
- **SupportAgentsController.cs** - Support agent management
- **ProfilesController.cs** - User profile management
- **TeamsController.cs** - Team management
- **BranchesController.cs** - Branch management
- **UsersController.cs** - User management

### Business Controllers (`/Controllers/Business/`)
Controllers for business-related entities:
- **OrdersController.cs** - Order management (? fully implemented)
- **SubscriptionsController.cs** - Subscription management (? fully implemented)
- **TransactionsController.cs** - Transaction management (? fully implemented)

### Ticketing Controllers (`/Controllers/Ticketing/`)
Controllers for ticketing system entities:
- **TicketsController.cs** - Ticket management
- **ResponsesController.cs** - Ticket response management
- **NotesController.cs** - Ticket note management
- **AttachmentsController.cs** - Attachment management (handles Attachment, AttachmentNote, AttachmentResponse)
- **TicketCategoriesController.cs** - Ticket category management
- **TicketStatesController.cs** - Ticket state management

### Knowledge Controllers (`/Controllers/Knowledge/`)
Controllers for knowledge base entities:
- **ArticlesController.cs** - Knowledge base article management (? fully implemented)
- **CannedResponsesController.cs** - Canned response management (? fully implemented)

## Domain Entity Mapping

This structure directly mirrors the Domain project's entity organization:

```
Domain/Entities/
??? Core/
?   ??? ApiKey.cs ? Core/ApiKeysController.cs ?
?   ??? Organization.cs ? Core/OrganizationsController.cs
?   ??? Customer.cs ? Core/CustomersController.cs
?   ??? SupportAgent.cs ? Core/SupportAgentsController.cs
?   ??? Profile.cs ? Core/ProfilesController.cs
?   ??? Team.cs ? Core/TeamsController.cs
?   ??? Branch.cs ? Core/BranchesController.cs
?   ??? User.cs ? Core/UsersController.cs
??? Business/
?   ??? Order.cs ? Business/OrdersController.cs ?
?   ??? Subscription.cs ? Business/SubscriptionsController.cs ?
?   ??? Transaction.cs ? Business/TransactionsController.cs ?
??? Ticketing/
?   ??? Ticket.cs ? Ticketing/TicketsController.cs
?   ??? Response.cs ? Ticketing/ResponsesController.cs
?   ??? Note.cs ? Ticketing/NotesController.cs
?   ??? Attachment.cs ? Ticketing/AttachmentsController.cs
?   ??? AttachmentNote.cs ? Ticketing/AttachmentsController.cs
?   ??? AttachmentResponse.cs ? Ticketing/AttachmentsController.cs
?   ??? TicketCategory.cs ? Ticketing/TicketCategoriesController.cs
?   ??? TicketState.cs ? Ticketing/TicketStatesController.cs
??? Knowledge/
    ??? Articles.cs ? Knowledge/ArticlesController.cs ?
    ??? CannedResponse.cs ? Knowledge/CannedResponsesController.cs ?
```

## Implementation Status

### ? Fully Implemented Controllers

#### Core Controllers
- **ApiKeysController.cs** - Complete CRUD operations with revoke, regenerate functionality

#### Business Controllers
- **OrdersController.cs** - Complete CRUD operations with:
  - Get orders by customer, subscription, status, date range
  - Update order status
  - Revenue statistics and total revenue
  - Pending orders management
  
- **SubscriptionsController.cs** - Complete CRUD operations with:
  - Organization context integration
  - Get subscriptions by organization, status
  - Active, expiring, and expired subscriptions
  - Business operations: extend, cancel, renew
  
- **TransactionsController.cs** - Complete CRUD operations with:
  - Get transactions by type, status, date range
  - Recent transactions
  - Business operations: approve, reject, process
  - Statistical endpoints and volume summaries
  - Amount totals by type

#### Knowledge Controllers
- **ArticlesController.cs** - Complete CRUD operations with:
  - Organization context integration
  - Article publishing workflow (publish/unpublish)
  - View count tracking and statistics
  - Search by tags and author
  - Most viewed articles
  - Publication status management
  - Comprehensive article statistics
  
- **CannedResponsesController.cs** - Complete CRUD operations with:
  - Organization context integration
  - Usage tracking and analytics
  - Search by tags functionality
  - Most used responses tracking
  - Active/inactive response management
  - Detailed usage analytics and statistics
  - Duplicate name prevention per organization

### ?? Placeholder Controllers (awaiting implementation)
- **All Core controllers** (except ApiKeysController) - Created with proper namespace and basic structure
- **All Ticketing controllers** - Created with proper namespace and basic structure  

## Features Implemented in Knowledge Controllers

### Common Features Across All Knowledge Controllers:
- **FluentValidation integration** - Input validation using DTOs
- **Organization context** - Multi-tenant aware operations
- **Comprehensive logging** - Structured logging for all operations
- **Error handling** - Proper exception handling and HTTP status codes
- **Pagination support** - For list endpoints using PaginationRequest
- **Search and filtering** - Query-based filtering capabilities

### ArticlesController Specific Features:
- **Publishing workflow** - Publish/unpublish with automatic date tracking
- **View tracking** - Increment view count with dedicated endpoint
- **Content management** - Full content lifecycle management
- **Author management** - Article authorship tracking
- **Tag-based search** - Search articles by tags
- **Statistics and analytics** - Comprehensive article statistics
- **Organization isolation** - Multi-tenant content separation

### CannedResponsesController Specific Features:
- **Usage analytics** - Detailed usage tracking and reporting
- **Tag management** - Tag-based organization and search
- **Active/inactive states** - Response lifecycle management
- **Duplicate prevention** - Name uniqueness per organization
- **Usage increment API** - Track when responses are used
- **Analytics dashboard** - Usage distribution and tag analytics
- **Most used tracking** - Identify popular responses

## Next Steps

1. **Implement remaining Core controllers** following the business/knowledge controller patterns
2. **Implement Ticketing controllers** with proper workflow management
3. **Add authentication and authorization** across all controllers
4. **Create comprehensive API documentation** using Swagger/OpenAPI
5. **Add unit and integration tests** for all controllers
6. **Implement caching strategies** for frequently accessed content
7. **Add rate limiting** for public-facing endpoints

## Naming Conventions

- Controllers use plural names (e.g., `CustomersController`, not `CustomerController`)
- Route patterns follow REST conventions: `api/[controller]`
- Namespaces follow the folder structure: `Helpio.Ir.API.Controllers.{FolderName}`
- Business operations use descriptive action names (e.g., `/publish`, `/use`, `/approve`)
- Statistics endpoints use `/statistics` or `/analytics` suffixes