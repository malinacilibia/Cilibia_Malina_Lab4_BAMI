using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using Microsoft.EntityFrameworkCore;
using Cilibia_Malina_Lab4.Data;
using Cilibia_Malina_Lab4.Models;
using static Cilibia_Malina_Lab4.PricePredictionModel;

namespace Price_Prediction.Controllers
{
    public class PredictionController : Controller
    {
        private readonly AppDbContext _context;

        public PredictionController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Price()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Price(ModelInput input)
        {

            MLContext mlContext = new MLContext();
            ITransformer mlModel = mlContext.Model.Load("PricePredictionModel.mlnet", out var modelInputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);

            ModelOutput result = predEngine.Predict(input);
            ViewBag.Price = result.Score;

            var history = new PredictionHistory
            {
                PassengerCount = input.Passenger_count,
                TripDistance = input.Trip_distance,
                TripTimeInSecs = input.Trip_time_in_secs,
                PaymentType = input.Payment_type,
                PredictedPrice = result.Score,
                CreatedAt = DateTime.Now
            };

            _context.PredictionHistories.Add(history);
            await _context.SaveChangesAsync();

            return View(input);
        }


        [HttpGet]
        public async Task<IActionResult> History(
    string? paymentType,
    float? minPrice,
    float? maxPrice,
    DateTime? startDate,
    DateTime? endDate,
    string? sortOrder)
        {
            var query = _context.PredictionHistories.AsQueryable();

            if (!string.IsNullOrEmpty(paymentType))
            {
                query = query.Where(p => p.PaymentType == paymentType);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.PredictedPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.PredictedPrice <= maxPrice.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt < endDate.Value.AddDays(1));
            }

            query = sortOrder switch
            {
                "price_asc" => query.OrderBy(p => p.PredictedPrice),
                "price_desc" => query.OrderByDescending(p => p.PredictedPrice),
                "date_asc" => query.OrderBy(p => p.CreatedAt),
                "date_desc" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            ViewBag.CurrentPaymentType = paymentType;
            ViewBag.CurrentMinPrice = minPrice;
            ViewBag.CurrentMaxPrice = maxPrice;
            ViewBag.CurrentStartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentEndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentSortOrder = sortOrder;

            var result = await query.ToListAsync();

            return View(result);
        }


        [HttpGet]
        public IActionResult Time()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Time(Cilibia_Malina_Lab4.TimePredictionModel.ModelInput input)
        {


            var mlContext = new MLContext();

            ITransformer mlModel = mlContext.Model.Load("TimePredictionModel.mlnet", out var modelInputSchema);

            var predEngine = mlContext.Model.CreatePredictionEngine<Cilibia_Malina_Lab4.TimePredictionModel.ModelInput, Cilibia_Malina_Lab4.TimePredictionModel.ModelOutput>(mlModel);

            var result = predEngine.Predict(input);

            ViewBag.TimeSeconds = result.Score;

            return View(input);
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.PredictionHistories.AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt.Date >= fromDate.Value.Date);
            }
            if (toDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt.Date <= toDate.Value.Date);
            }

            var totalPredictions = await query.CountAsync();

            var paymentTypeStats = await query
                .GroupBy(p => p.PaymentType)
                .Select(g => new PaymentTypeStat
                {
                    PaymentType = g.Key,
                    AveragePrice = g.Average(x => x.PredictedPrice),
                    Count = g.Count()
                }).ToListAsync();

            var allPredictions = await query
                .Select(p => p.PredictedPrice)
                .ToListAsync();

            var buckets = new List<PriceBucketStat>
    {
        new PriceBucketStat { Label = "0-10" },
        new PriceBucketStat { Label = "10-20" },
        new PriceBucketStat { Label = "20-30" },
        new PriceBucketStat { Label = "30-50" },
        new PriceBucketStat { Label = "> 50" }
    };

            foreach (var price in allPredictions)
            {
                if (price < 10) buckets[0].Count++;
                else if (price < 20) buckets[1].Count++;
                else if (price < 30) buckets[2].Count++;
                else if (price < 50) buckets[3].Count++;
                else buckets[4].Count++;
            }

            var vm = new DashboardViewModel
            {
                TotalPredictions = totalPredictions,
                PaymentTypeStats = paymentTypeStats,
                PriceBuckets = buckets,
                FromDate = fromDate,
                ToDate = toDate
            };

            return View(vm);
        }
    }
}