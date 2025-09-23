using System.Globalization;

namespace SeppApp;

public interface ISpeechRecognition
{

    Task<bool> RequestPermissions();

    Task<string> Listen(CultureInfo culture,
        IProgress<string> recognitionResult,
        CancellationToken cancellationToken);

}