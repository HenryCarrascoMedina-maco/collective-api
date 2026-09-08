using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Collective.Api.Modules.Contact;

/// <summary>Configuracion de persistencia del modulo. Prefijo contact_ (regla M6).</summary>
public sealed class ContactDbConfig : IEntityTypeConfiguration<ContactRequest>
{
    public void Configure(EntityTypeBuilder<ContactRequest> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("contact_requests");

        builder.HasKey(request => request.Id);

        builder.Property(request => request.Name).HasMaxLength(120).IsRequired();
        builder.Property(request => request.Email).HasMaxLength(254).IsRequired();
        builder.Property(request => request.Company).HasMaxLength(160);
        builder.Property(request => request.Phone).HasMaxLength(40);
        builder.Property(request => request.Message).HasMaxLength(4000).IsRequired();
        builder.Property(request => request.ProductSlug).HasMaxLength(80);
        builder.Property(request => request.SourcePage).HasMaxLength(300);
        builder.Property(request => request.UserAgent).HasMaxLength(400);
        builder.Property(request => request.IpHash).HasMaxLength(64);

        // Los enum se guardan como texto: legibles en SQL y no se rompen al
        // reordenarlos (regla D3).
        builder.Property(request => request.Reason)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(request => request.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(request => request.CreatedAt).IsRequired();

        builder.HasIndex(request => request.CreatedAt);
        builder.HasIndex(request => request.Email);
    }
}
