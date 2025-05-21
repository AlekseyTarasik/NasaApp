using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NasaApp.Database.Data
{
	public class Meteorite
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int Id { get; set; }

		[Required]
		public string Name { get; set; }
		public string Nametype { get; set; }
		public string Recclass { get; set; }
		public double? Mass { get; set; }
		public string Fall { get; set; }
		public DateTime? Year { get; set; }
		public double? Reclat { get; set; }
		public double? Reclong { get; set; }

		public Geolocation Geolocation { get; set; }
	}
	public class MeteoriteDto
	{
		public string name { get; set; }
		public string id { get; set; }
		public string nametype { get; set; }
		public string recclass { get; set; }
		public string mass { get; set; }
		public string fall { get; set; }
		public string year { get; set; }
		public string reclat { get; set; }
		public string reclong { get; set; }
		public GeolocationDto geolocation { get; set; }
	}
}
