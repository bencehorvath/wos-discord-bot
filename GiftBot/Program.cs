using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Hosting;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using WOS_OCR;

namespace GiftBot;

internal static class Program
{
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    private static async Task Main(string[] args)
    {

        if (args.Length == 2 && args[0] == "--captcha")
        {
            var fileName = args[1];
            if (!File.Exists(fileName)) Console.WriteLine($"File {fileName} not found");

            var readCaptcha = CaptchaReader.ReadBestAttempt(fileName);

            Console.WriteLine(string.IsNullOrEmpty(readCaptcha)
                ? $"Captcha file {fileName} could not be read"
                : $"The image contains the captcha: {readCaptcha}");

            return;
        }

        Console.WriteLine("WOS Gift Bot launching...");

        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddDiscordGateway().AddApplicationCommands();

        var host = builder.Build();
        host.AddModules(typeof(Program).Assembly);

        await host.RunAsync();
    }
}
