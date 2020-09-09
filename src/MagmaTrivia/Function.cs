using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using AWS.Logger;
using AWS.Logger.SeriLog;
using Serilog;
using Serilog.Formatting.Compact;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace MagmaTrivia
{   public class Function
    {
        private static readonly HttpClient Client = new HttpClient();

        // ReSharper disable once InconsistentNaming
        private static async Task<string> GetCallingIP()
        {
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Add("User-Agent", "AWS Lambda .Net Client");

            var msg = await Client.GetStringAsync("http://checkip.amazonaws.com/").ConfigureAwait(continueOnCapturedContext:false);

            return msg.Replace("\n","");
        }

        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
        {
            LambdaLogger.Log($"FunctionHandler: event is {apigProxyEvent}");

            var configuration = new AWSLoggerConfig
            {
                LogGroup = "MagmaTriviaLogGroup",
                LogStreamNamePrefix = "MagmaTriviaLogStream",
                LogStreamNameSuffix = "dev",
                Region = "us-east-1",
                DisableLogGroupCreation = false,
                MaxQueuedMessages = 1
            };
            var logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                // .WriteTo.AWSSeriLog(configuration)
                .WriteTo.Console(new CompactJsonFormatter())
                .CreateLogger();
            logger.Information($"Serilog.FunctionHandler: event is {apigProxyEvent}");
            
            var location = await GetCallingIP();
            var body = new Dictionary<string, string>
            {
                { "message", "hello world" },
                { "location", location }
            };

            return new APIGatewayProxyResponse
            {
                Body = JsonConvert.SerializeObject(body),
                StatusCode = 200,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }
}
