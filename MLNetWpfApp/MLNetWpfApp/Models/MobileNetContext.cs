using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Image;
using MLNetWpfApp.DataModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLNetWpfApp.Models
{
    public class MobileNetContext
    {
        public const string MobileNetOutputColumnName = "mobilenetv20_output_flatten0_reshape0";
        private static readonly (int Width, int Height) InputImageSize = (256, 256);
        private static readonly (int Width, int Height) CroppingImageSize = (224, 224);
        private static readonly (float Red, float Green, float Blue) ImageNormalize = (0.485f, 0.456f, 0.406f);
        private static readonly (float Red, float Green, float Blue) ImageScaling = (0.229f, 0.224f, 0.225f);

        private readonly MLContext context;
        private List<string> labels;
        private ITransformer model;

        public MobileNetContext()
        {
            context = new MLContext();
            LoadLabel(@".\Assets\labels.txt");
            LoadModel(@".\Assets\mobilenetv2-1.0.onnx");
        }

        public async Task<string> InferAsync(string imagePath)
            => await Task.Run<string>(() =>
            {
                try
                {
                    var images = new[] { new ImageData { ImagePath = imagePath } };
                    var data = context.Data.LoadFromEnumerable(images);
                    var predict = model.Transform(data);
                    var estimate = predict.GetColumn<float>(nameof(ImagePrediction.Estimate));
                    var index = predict.GetColumn<int>(nameof(ImagePrediction.Index));
                    var label = predict.GetColumn<string>(nameof(ImagePrediction.Label));
                    return label.FirstOrDefault();
                }
                catch
                {
                    return "?";
                }
            });

        private void LoadLabel(string labelLocation)
            => labels = File.ReadLines(Path.Combine(Directory.GetCurrentDirectory(), labelLocation)).Select(x => x.Split(',').First()).ToList();

        private void LoadModel(string modelLocation)
        {
            var data = context.Data.LoadFromEnumerable(new List<ImageData>());

            var pipeline = context.Transforms
                                  .LoadImages(outputColumnName: "image", imageFolder: string.Empty, inputColumnName: nameof(ImageData.ImagePath))
                                  .Append(context.Transforms.ResizeImages(outputColumnName: "resizedimage", imageWidth: InputImageSize.Width, imageHeight: InputImageSize.Height, resizing: ImageResizingEstimator.ResizingKind.Fill, inputColumnName: "image"))
                                  .Append(context.Transforms.ResizeImages(outputColumnName: "croppedimage", imageWidth: CroppingImageSize.Width, imageHeight: CroppingImageSize.Height, resizing: ImageResizingEstimator.ResizingKind.IsoCrop, cropAnchor: ImageResizingEstimator.Anchor.Center, inputColumnName: "resizedimage"))
                                  .Append(context.Transforms.ExtractPixels(outputColumnName: "red", colorsToExtract: ImagePixelExtractingEstimator.ColorBits.Red, offsetImage: ImageNormalize.Red * 255, scaleImage: 1 / (ImageScaling.Red * 255), inputColumnName: "croppedimage"))
                                  .Append(context.Transforms.ExtractPixels(outputColumnName: "green", colorsToExtract: ImagePixelExtractingEstimator.ColorBits.Green, offsetImage: ImageNormalize.Green * 255, scaleImage: 1 / (ImageScaling.Green * 255), inputColumnName: "croppedimage"))
                                  .Append(context.Transforms.ExtractPixels(outputColumnName: "blue", colorsToExtract: ImagePixelExtractingEstimator.ColorBits.Blue, offsetImage: ImageNormalize.Blue * 255, scaleImage: 1 / (ImageScaling.Blue * 255), inputColumnName: "croppedimage"))
                                  .Append(context.Transforms.Concatenate(outputColumnName: "data", "red", "green", "blue"))
                                  .Append(context.Transforms.ApplyOnnxModel(modelFile: modelLocation, outputColumnNames: new[] { MobileNetOutputColumnName }, inputColumnNames: new[] { "data" }))
                                  .Append(context.Transforms.CustomMapping<MobileNetPrediction, ImagePrediction>((networkResult, prediction) =>
                                  {
                                      prediction.Estimate = networkResult.Output.Max();
                                      prediction.Index = networkResult.Output.ToList().IndexOf(prediction.Estimate);
                                      prediction.Label = labels[prediction.Index];
                                  }, "prediction"));

            model = pipeline.Fit(data);
        }
    }
}
