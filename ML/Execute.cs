using Microsoft.ML;

namespace ML
{
    public class Execute
    {
        private PredictionEngine<Inspect.ModelInput, Inspect.ModelOutput> _engine;

        /// <summary>
        /// load model from file
        /// </summary>
        /// <param name="modelPath"></param>
        /// <returns></returns>
        public Task LoadAsync(string modelPath)
        {
            return Task.Run(() =>
            {
                var mlContext = new MLContext();
                var transformer = mlContext.Model.Load(modelPath, out var schema);
                _engine = mlContext.Model.CreatePredictionEngine<Inspect.ModelInput, Inspect.ModelOutput>(transformer);
            });
        }

        /// <summary>
        /// run predict
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public async Task<(string label, float[] value)> PredictAsync(string fullPath)
        {
            var bytes = await File.ReadAllBytesAsync(fullPath);

            var inputModel = new Inspect.ModelInput
            {
                ImageSource = bytes
            };

            Inspect.ModelOutput output = new();

            await Task.Run(() => { output = _engine.Predict(inputModel); });

            return (output.PredictedLabel, output.Score);
        }

        /// <summary>
        /// make model and load new model
        /// </summary>
        /// <param name="imageFolder"></param>
        /// <param name="modelName"></param>
        /// <returns></returns>
        public Task BuildModel(string imageFolder, string modelName)
        {
            return Task.Run(() =>
            {
                var mlContext = new MLContext();

                var dataView = Inspect.LoadImageFromFolder(mlContext, imageFolder);
                var transformer = Inspect.RetrainModel(mlContext, dataView);

                mlContext.Model.Save(transformer, dataView.Schema, modelName);
            });
        }

        public Task DisposeAsync()
        {
            return Task.Run(_engine.Dispose);
        }
    }
}
