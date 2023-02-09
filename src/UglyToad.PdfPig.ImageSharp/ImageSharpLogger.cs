namespace UglyToad.PdfPig.ImageSharp
{
    using System;
    using UglyToad.PdfPig.Logging;

    public class ImageSharpLogger : ILog
    {
        public void Debug(string message)
        {
            System.Diagnostics.Debug.Print("Debug: " + message);
        }

        public void Debug(string message, Exception ex)
        {
            System.Diagnostics.Debug.Print("Debug: " + message);
        }

        public void Error(string message)
        {
            System.Diagnostics.Debug.Print("Error: " + message);
        }

        public void Error(string message, Exception ex)
        {
            System.Diagnostics.Debug.Print("Error: " + message);
        }

        public void Warn(string message)
        {
            System.Diagnostics.Debug.Print("Warn: " + message);
        }
    }
}
