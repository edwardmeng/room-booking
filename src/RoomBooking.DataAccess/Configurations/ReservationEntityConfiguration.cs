using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoomBooking.DataAccess.Entities;

namespace RoomBooking.DataAccess.Configurations;

public sealed class ReservationEntityConfiguration : IEntityTypeConfiguration<ReservationEntity>
{
    public void Configure(EntityTypeBuilder<ReservationEntity> builder)
    {
        builder.ToTable("Reservations", table =>
        {
            table.HasCheckConstraint(
                "CK_Reservations_StartMinute",
                "StartMinute >= 0 AND StartMinute < 1440");
            table.HasCheckConstraint(
                "CK_Reservations_EndMinute",
                "EndMinute >= 0 AND EndMinute < 1440");
            table.HasCheckConstraint(
                "CK_Reservations_Time_Order",
                "StartMinute < EndMinute");
        });

        builder.HasKey(reservation => reservation.Id)
            .HasName("PK_Reservations");

        builder.Property(reservation => reservation.RoomId)
            .HasColumnType("INTEGER")
            .IsRequired();

        builder.Property(reservation => reservation.Title)
            .HasColumnType("TEXT")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(reservation => reservation.Date)
            .HasColumnType("TEXT")
            .IsRequired();

        builder.Property(reservation => reservation.StartMinute)
            .HasColumnType("INTEGER")
            .IsRequired();

        builder.Property(reservation => reservation.EndMinute)
            .HasColumnType("INTEGER")
            .IsRequired();

        builder.Property(reservation => reservation.CreatedAt)
            .HasColumnType("TEXT")
            .IsRequired();

        builder.HasOne(reservation => reservation.Room)
            .WithMany(room => room.Reservations)
            .HasForeignKey(reservation => reservation.RoomId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Reservations_Rooms_RoomId");

        builder.HasIndex(reservation => new
            {
                reservation.RoomId,
                reservation.Date,
                reservation.StartMinute,
                reservation.EndMinute
            })
            .HasDatabaseName("IX_Reservations");
    }
}