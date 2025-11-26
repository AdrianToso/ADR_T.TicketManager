using ADR_T.TicketManager.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ADR_T.TicketManager.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasMany(u => u.TicketsCreados)
            .WithOne(t => t.CreadoByUser)
            .HasForeignKey(t => t.CreadoByUserId)
            .IsRequired(true)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany<Ticket>()
            .WithOne(t => t.AsignadoUser)
            .HasForeignKey(t => t.AsignadoUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(u => u.FechacCreacion).IsRequired();
        builder.Property(u => u.IsDeleted).IsRequired();

        // Ignorar la colección de eventos de dominio para que EF Core no intente mapearla a la DB
        builder.Ignore(u => u.DomainEvents);
    }
}