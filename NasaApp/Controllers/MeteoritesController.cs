using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NasaApp.Database;

namespace NasaApp.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class MeteoritesController : ControllerBase
	{
		private readonly AppDbContext _context;

		public MeteoritesController(AppDbContext context)
		{
			_context = context;
		}

		[HttpGet("grouped")]
		public async Task<IActionResult> GetGroupedMeteorites(
			[FromQuery] int? startYear = null,
			[FromQuery] int? endYear = null,
			[FromQuery] string recclass = null,
			[FromQuery] string nameContains = null,
			[FromQuery] string sortBy = "year",
			[FromQuery] string sortOrder = "asc")
		{
			var query = _context.Meteorites.AsQueryable();

			// Фильтрация
			if (startYear.HasValue)
				query = query.Where(m => m.Year.HasValue && m.Year.Value.Year >= startYear);

			if (endYear.HasValue)
				query = query.Where(m => m.Year.HasValue && m.Year.Value.Year <= endYear);

			if (!string.IsNullOrEmpty(recclass))
				query = query.Where(m => m.Recclass == recclass);

			if (!string.IsNullOrEmpty(nameContains))
				query = query.Where(m => m.Name.Contains(nameContains));

			// Группировка
			var groupedQuery = query
				.GroupBy(m => m.Year.HasValue ? m.Year.Value.Year : (int?)null)
				.Select(g => new
				{
					Year = g.Key,
					Count = g.Count(),
					TotalMass = g.Sum(m => m.Mass) ?? 0
				});

			var result = await groupedQuery.ToListAsync();

			// Сортируем в памяти
			switch (sortBy.ToLower())
			{
				case "year":
					result = sortOrder.ToLower() == "desc"
						? result.OrderByDescending(g => g.Year).ToList()
						: result.OrderBy(g => g.Year).ToList();
					break;
				case "count":
					result = sortOrder.ToLower() == "desc"
						? result.OrderByDescending(g => g.Count).ToList()
						: result.OrderBy(g => g.Count).ToList();
					break;
				case "totalmass":
					result = sortOrder.ToLower() == "desc"
						? result.OrderByDescending(g => g.TotalMass).ToList()
						: result.OrderBy(g => g.TotalMass).ToList();
					break;
				default:
					result = result.OrderBy(g => g.Year).ToList();
					break;
			}

			return Ok(result);
		}

		[HttpGet("years")]
		public async Task<IActionResult> GetAvailableYears()
		{
			var years = await _context.Meteorites
				.Where(m => m.Year.HasValue)
				.Select(m => m.Year.Value.Year)
				.Distinct()
				.OrderBy(y => y)
				.ToListAsync();

			return Ok(years);
		}

		[HttpGet("recclasses")]
		public async Task<IActionResult> GetAvailableRecclasses()
		{
			var classes = await _context.Meteorites
				.Where(m => !string.IsNullOrEmpty(m.Recclass))
				.Select(m => m.Recclass)
				.Distinct()
				.OrderBy(c => c)
				.ToListAsync();

			return Ok(classes);
		}
	}
}
