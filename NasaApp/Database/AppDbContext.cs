using Microsoft.EntityFrameworkCore;
using NasaApp.Database.Data;

namespace NasaApp.Database
{
	public class AppDbContext : DbContext
	{
		public DbSet<Meteorite> Meteorites { get; set; }
		public DbSet<Geolocation> Geolocations { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Meteorite>(entity =>
			{
				modelBuilder.Entity<Meteorite>()
								.HasOne(m => m.Geolocation)
								.WithOne(g => g.Meteorite)
								.HasForeignKey<Geolocation>(g => g.MeteoriteId)
								.OnDelete(DeleteBehavior.Cascade);

				modelBuilder.Entity<Meteorite>()
					.Property(m => m.Id)
					.ValueGeneratedNever(); // Отключаем автогенерацию ID
			});

			modelBuilder.Entity<Geolocation>(entity =>
			{
				entity.HasIndex(e => e.MeteoriteId).IsUnique();
			});
		}

		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
	}
}
