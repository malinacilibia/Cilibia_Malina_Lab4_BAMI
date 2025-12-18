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
        public async Task<IActionResult> History()
        {
            var history = await _context.PredictionHistories
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(history);
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
    }
}