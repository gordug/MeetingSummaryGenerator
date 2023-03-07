using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace MeetingSummaryGenerator
{
    public class MeetingSummaryGenerator
    {

        [FunctionName("MeetingSummaryGenerator")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "keyPhrases", In = ParameterLocation.Query, Required = true, Type = typeof(KeyPhraseBatchResult), Description = "The meeting Key Phrases")]
        [OpenApiParameter(name: "sentiment", In = ParameterLocation.Query, Required = true, Type = typeof(SentimentBatchResult), Description = "The meeting sentiment")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public static Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
        {
            var phrases = req.Query["keyPhrases"];
            var sentiments = req.Query["sentiment"];
            var sentiment = JsonConvert.DeserializeObject<SentimentBatchResult>(sentiments);
            var keyPhrases = JsonConvert.DeserializeObject<KeyPhraseBatchResult>(phrases);

            var summary = GenerateSummary(sentiment, keyPhrases);

            return Task.FromResult<IActionResult>(new OkObjectResult(summary));
        }

        private static string GenerateSummary(
            SentimentBatchResult sentiment,
            KeyPhraseBatchResult keyPhrases)
        {
            // TODO - Replace with your own logic to generate a summary
            // Determine the overall sentiment of the meeting
            var overallSentiment = sentiment.Documents[0].Score;
            var sentimentLabel = overallSentiment >= 0.5 ? "positive" : "negative";

            // Get the top key phrases mentioned in the meeting
            var keyPhraseList = keyPhrases.Documents[0].KeyPhrases.ToList();
            var numKeyPhrases = Math.Min(keyPhraseList.Count, 3);
            var topKeyPhrases = string.Join(", ", keyPhraseList.Take(numKeyPhrases));

            // Generate the meeting summary based on the sentiment and key phrases
            var summary = $"The meeting was {sentimentLabel}, and the top topics discussed were: {topKeyPhrases}.";
            return summary;
        }
    }
}

