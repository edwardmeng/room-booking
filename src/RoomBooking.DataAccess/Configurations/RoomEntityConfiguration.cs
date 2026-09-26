using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoomBooking.DataAccess.Entities;

namespace RoomBooking.DataAccess.Configurations;

public sealed class RoomEntityConfiguration : IEntityTypeConfiguration<RoomEntity>
{
    public void Configure(EntityTypeBuilder<RoomEntity> builder)
    {
        _ = builder.ToTable("Rooms", table =>
            table.HasCheckConstraint("CK_Rooms_Capacity", "Capacity > 0"));

        _ = builder.HasKey(room => room.Id)
            .HasName("PK_Rooms");

        _ = builder.Property(room => room.Name)
            .HasColumnType("TEXT")
            .HasMaxLength(200)
            .IsRequired();

        _ = builder.Property(room => room.Location)
            .HasColumnType("TEXT")
            .HasMaxLength(200)
            .IsRequired();

        _ = builder.Property(room => room.Description)
            .HasColumnType("TEXT")
            .HasMaxLength(1000);

        _ = builder.Property(room => room.Capacity)
            .HasColumnType("INTEGER")
            .IsRequired();

        _ = builder.HasData(
            new RoomEntity
            {
                Id = 1,
                Name = "Small Room",
                Location = "Floor 1, East Wing",
                Description = "Quiet room for small meetings and interviews.",
                Capacity = 4
            },
            new RoomEntity
            {
                Id = 2,
                Name = "Conference Room",
                Location = "Floor 2, Central Wing",
                Description = "Conference room with presentation facilities.",
                Capacity = 10
            },
            new RoomEntity
            {
                Id = 3,
                Name = "Auditorium",
                Location = "Ground Floor, West Wing",
                Description = "Large room for presentations and group sessions.",
                Capacity = 50
            });
    }
}
