using System.Text;

namespace AiiaDemoApi.Helpers
{
    public static class HttpHelper
    {
        public static async Task<string> ReadRequestBody(Stream bodyStream)
        {
            string documentContents;
            using (bodyStream)
            {
                using (var readStream = new StreamReader(bodyStream, Encoding.UTF8))
                {
                    documentContents = await readStream.ReadToEndAsync();
                }
            }

            return documentContents;
        }
    }
}
