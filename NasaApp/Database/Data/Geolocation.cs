using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NasaApp.Database.Data
{
	public class Geolocation
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[ForeignKey("Meteorite")]
		public int MeteoriteId { get; set; }
		public Meteorite Meteorite { get; set; }

		[Required]
		public string Type { get; set; } = "Point";
		public double Longitude { get; set; }
		public double Latitude { get; set; }

		[NotMapped]
		public string Coordinates => $"[{Longitude}, {Latitude}]";
	}

	public class GeolocationDto
	{
		public string type { get; set; }
		public double[] coordinates { get; set; }
	}
}
