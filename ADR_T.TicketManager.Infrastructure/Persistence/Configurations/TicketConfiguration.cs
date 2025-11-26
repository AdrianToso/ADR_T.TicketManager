using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ADR_T.TicketManager.Core.Domain.Entities;

namespace ADR_T.TicketManager.Infrastructure.Persistence.Configurations
{
    public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
    {
        public void Configure(EntityTypeBuilder<Ticket> builder)
        {
            builder.ToTable("Tickets");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .HasDefaultValueSql("NEWID()");

            builder.Property(t => t.Titulo)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.Descripcion)
                .IsRequired();

            builder.HasOne(t => t.CreadoByUser)
                .WithMany(u => u.TicketsCreados)
                .HasForeignKey(t => t.CreadoByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }

}
