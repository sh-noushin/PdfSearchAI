using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;

namespace InternalAIAssistant.Helpers
{
    public static class PdfHelper
    {
        /// <summary>
        /// Extracts text from a specific page of a PDF file.
        /// </summary>
        /// <param name="fileName">PDF file path</param>
        /// <param name="page">1-based page number</param>
        /// <returns>Text content of the page, or empty string if not found</returns>
        public static string ExtractPageText(string fileName, int page)
        {
            if (!File.Exists(fileName)) return "";
            using (var document = PdfDocument.Open(fileName))
            {
                var pdfPage = document.GetPage(page);
                return pdfPage?.Text ?? "";
            }
        }
    }
}
