using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using static Cilibia_Malina_Lab4.PricePredictionModel;

namespace Price_Prediction.Controllers
{
    public class PredictionController : Controller
    {
        public IActionResult Price(ModelInput input)
        {
            // Load the model
            MLContext mlContext = new MLContext();
            // Create predection engine related to the loaded train model
            ITransformer mlModel =
           mlContext.Model.Load("PricePredictionModel.mlnet", out var modelInputSchema); 
            var predEngine = mlContext.Model.CreatePredictionEngine<ModelInput,
           ModelOutput>(mlModel);
            // Try model on sample data to predict fair price
            ModelOutput result = predEngine.Predict(input);
            ViewBag.Price = result.Score;
            return View(input);
        }


        public IActionResult Time(Cilibia_Malina_Lab4.TimePredictionModel.ModelInput input)
        {
            var mlContext = new MLContext();

            ITransformer mlModel = 
                mlContext.Model.Load("TimePredictionModel.mlnet", out var modelInputSchema);

            var predEngine = mlContext.Model.CreatePredictionEngine<Cilibia_Malina_Lab4.TimePredictionModel.ModelInput, Cilibia_Malina_Lab4.TimePredictionModel.ModelOutput>(mlModel);

            var result = predEngine.Predict(input);

            ViewBag.TimeSeconds = result.Score;

            return View(input);
        }
    }
}
