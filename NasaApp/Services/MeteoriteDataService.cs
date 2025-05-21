using Microsoft.EntityFrameworkCore;
using NasaApp.Database;
using NasaApp.Database.Data;
using Newtonsoft.Json;
using System.Globalization;

namespace NasaApp.Services
{
	public class MeteoriteDataService
	{
		private readonly HttpClient _httpClient;
		private readonly AppDbContext _context;
		private readonly ILogger<MeteoriteDataService> _logger;

		public MeteoriteDataService(
			HttpClient httpClient,
			AppDbContext context,
			ILogger<MeteoriteDataService> logger)
		{
			_httpClient = httpClient;
			_context = context;
			_logger = logger;
		}
		public async Task FetchAndSaveMeteoriteDataAsync()
		{
			const string dataUrl = "https://raw.githubusercontent.com/biggiko/nasa-dataset/refs/heads/main/y77d-th95.json";
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				var response = await _httpClient.GetStringAsync(dataUrl);
				var meteoriteDtos = JsonConvert.DeserializeObject<List<MeteoriteDto>>(response);

				foreach (var dto in meteoriteDtos)
				{
					var meteoriteId = int.Parse(dto.id);
					var existingMeteorite = await _context.Meteorites
						.FirstOrDefaultAsync(m => m.Id == meteoriteId);

					if (existingMeteorite == null)
					{
						var newMeteorite = new Meteorite
						{
							Id = meteoriteId,
							Name = dto.name,
							Nametype = dto.nametype,
							Recclass = dto.recclass,
							Mass = double.TryParse(dto.mass, out var mass) ? mass : null,
							Fall = dto.fall,
							Year = ParseYear(dto.year),
							Reclat = double.TryParse(dto.reclat, out var reclat) ? reclat : null,
							Reclong = double.TryParse(dto.reclong, out var reclong) ? reclong : null
						};
						_context.Meteorites.Add(newMeteorite);
					}
					else
					{
						existingMeteorite.Name = dto.name;
						existingMeteorite.Nametype = dto.nametype;
						existingMeteorite.Recclass = dto.recclass;
						existingMeteorite.Mass = double.TryParse(dto.mass, out var mass) ? mass : null;
						existingMeteorite.Fall = dto.fall;
						existingMeteorite.Year = ParseYear(dto.year);
						existingMeteorite.Reclat = double.TryParse(dto.reclat, out var reclat) ? reclat : null;
						existingMeteorite.Reclong = double.TryParse(dto.reclong, out var reclong) ? reclong : null;
					}
				}
				await _context.SaveChangesAsync();

				foreach (var dto in meteoriteDtos)
				{
					if (!int.TryParse(dto.id, out var meteoriteId))
					{
						_logger.LogWarning($"Неверный ID метеорита: {dto.id}. Пропуск...");
						continue;
					}

					if (dto.geolocation == null || dto.geolocation.coordinates == null || dto.geolocation.coordinates.Length < 2)
					{
						_logger.LogWarning($"Метеорит {meteoriteId} содержит неверные данные о геолокации. Пропуск...");
						continue;
					}

					var existingGeolocation = await _context.Geolocations
						.FirstOrDefaultAsync(g => g.MeteoriteId == meteoriteId);

					if (existingGeolocation == null)
					{
						var newGeolocation = new Geolocation
						{
							MeteoriteId = meteoriteId,
							Type = dto.geolocation.type ?? "Point",
							Longitude = dto.geolocation.coordinates[0],
							Latitude = dto.geolocation.coordinates[1]
						};
						_context.Geolocations.Add(newGeolocation);
					}
					else
					{
						existingGeolocation.Type = dto.geolocation.type ?? "Point";
						existingGeolocation.Longitude = dto.geolocation.coordinates[0];
						existingGeolocation.Latitude = dto.geolocation.coordinates[1];
					}
				}
				await _context.SaveChangesAsync();
				_logger.LogInformation($"Данные о метеоритах успешно обновлены. Количество записей: {_context.Meteorites.Count()}");
				await transaction.CommitAsync();
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "Ошибка при получении и сохранении данных о метеорите");
				throw;
			}
		}

		private DateTime? ParseYear(string yearString)
		{
			if (string.IsNullOrEmpty(yearString)) return null;

			if (DateTime.TryParseExact(yearString, "yyyy-MM-ddTHH:mm:ss.FFF",
				CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
			{
				return date.ToUniversalTime();
			}

			if (DateTime.TryParse(yearString, out date))
			{
				return date.ToUniversalTime();
			}

			return null;
		}
	}
}
