using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScanDrive.Domain.Entities;

namespace ScanDrive.Infrastructure.Data.Configurations;

public class ChatConfiguration : IEntityTypeConfiguration<ChatSession>
{
    public void Configure(EntityTypeBuilder<ChatSession> builder)
    {
        builder.HasQueryFilter(s => !s.IsDeleted && s.IsActive);
    }
}

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.HasQueryFilter(m => !m.IsDeleted);
    }
} 