
using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Tesseract;

namespace WOS_OCR;

public static class CaptchaReader
{
    public static string ReadBestAttempt(string inputPath)
    {
        float[] thresholds = { 0.45f, 0.50f, 0.55f, 0.60f, 0.65f };

        string bestText = "";
        float bestConfidence = 0f;

        foreach (float threshold in thresholds)
        {
            string tempPath = Path.Combine(
                Path.GetTempPath(),
                $"captcha_{threshold}_{Guid.NewGuid():N}.png");

            try
            {
                PreprocessImageWithThreshold(inputPath, tempPath, threshold);

                string tessDataPath = Path.Combine(AppContext.BaseDirectory, "tessdata");

                using var engine = new TesseractEngine(
                    tessDataPath,
                    "eng",
                    EngineMode.Default);

                engine.SetVariable(
                    "tessedit_char_whitelist",
                    "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");

                using Pix pix = Pix.LoadFromFile(tempPath);
                using Page page = engine.Process(pix, PageSegMode.SingleLine);

                string raw = page.GetText() ?? "";
                string cleaned = CleanOcrResult(raw);
                float confidence = page.GetMeanConfidence();

                if (!string.IsNullOrWhiteSpace(cleaned) && confidence > bestConfidence)
                {
                    bestText = cleaned;
                    bestConfidence = confidence;
                }
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        Console.WriteLine($"Best confidence: {bestConfidence:P1}");
        return bestText;
    }

    private static void PreprocessImageWithThreshold(
        string inputPath,
        string outputPath,
        float threshold)
    {
        using Image image = Image.Load(inputPath);

        image.Mutate(ctx =>
        {
            ctx.Resize(image.Width * 3, image.Height * 3);
            ctx.Grayscale();
            ctx.BinaryThreshold(threshold);
        });

        image.Save(outputPath);
    }

    private static string CleanOcrResult(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "";

        // Keep only alphanumeric characters.
        string cleaned = Regex.Replace(text, "[^a-zA-Z0-9]", "");

        return cleaned.Trim();
    }
}
