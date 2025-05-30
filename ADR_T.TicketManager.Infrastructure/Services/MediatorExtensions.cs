//using MediatR;
//using ADR_T.TicketManager.Core.Domain.Entities;
//using ADR_T.TicketManager.Infrastructure.Persistence;

//namespace ADR_T.TicketManager.Infrastructure.Services;
//public static class MediatorExtensions
//{
//    public static async Task DispatchDomainEventsAsync(this IMediator mediator, AppDbContext context)
//    {
//        var entities = context.ChangeTracker
//            .Entries<EntityBase>()
//            .Where(e => e.Entity.DomainEvents.Any())
//            .Select(e => e.Entity)
//            .ToList();

//        var domainEvents = entities
//            .SelectMany(e => e.DomainEvents)
//            .ToList();

//        entities.ForEach(e => e.ClearDomainEvent()); 

//        foreach (var domainEvent in domainEvents)
//        {
//            await mediator.Publish(domainEvent);
//        }
//    }
//}