using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScanDrive.Domain.Entities;

namespace ScanDrive.Infrastructure.Data.Configurations;

public class ChatQuestionConfiguration : IEntityTypeConfiguration<ChatQuestion>
{
    public void Configure(EntityTypeBuilder<ChatQuestion> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Question)
            .IsRequired()
            .HasMaxLength(500);
            
        builder.Property(x => x.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);
            
        builder.Property(x => x.Step)
            .IsRequired()
            .HasDefaultValue(1);
            
        // Índices
        builder.HasIndex(x => x.Step);
        builder.HasIndex(x => x.IsEnabled);
        builder.HasIndex(x => x.CreatedAt);
    }
} 