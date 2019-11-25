using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon.Athena;
using Amazon.Athena.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace GetAPI
{
    public class Functions
    {
        private const String ATHENA_TEMP_PATH = "s3://employeedg/Employee/";
        private const String ATHENA_DB = "emp_db";

        public APIGatewayProxyResponse Get(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine("Get Request\n");

            using (var client = new AmazonAthenaClient(Amazon.RegionEndpoint.USEast1))
            {
                String date = JsonConvert.DeserializeObject<String>(request?.Body);
                QueryExecutionContext qContext = new QueryExecutionContext();
                qContext.Database = ATHENA_DB;
                ResultConfiguration resConf = new ResultConfiguration();
                resConf.OutputLocation = ATHENA_TEMP_PATH;

                Console.WriteLine("Created Athena Client");
                run(client, qContext, resConf, date).Wait();
            }

            async Task run(IAmazonAthena client, QueryExecutionContext qContext, ResultConfiguration resConf, String date)
            {
                StartQueryExecutionRequest qReq = new StartQueryExecutionRequest()
                {
                    QueryString = "SELECT emp_name FROM emp_table WHERE date = " + date + "limit 10;",
                    QueryExecutionContext = qContext,
                    ResultConfiguration = resConf
                };

                try
                {
                    StartQueryExecutionResponse qRes = await client.StartQueryExecutionAsync(qReq);
                    List<Dictionary<String, String>> items = await getQueryExecution(client, qRes.QueryExecutionId);
                    foreach (var item in items)
                    {
                        foreach (KeyValuePair<String, String> pair in item)
                        {
                            Console.WriteLine("Col: {0}", pair.Key);
                            Console.WriteLine("Val: {0}", pair.Value);
                        }
                    }
                }
                catch (InvalidRequestException e)
                {
                    Console.WriteLine("Run Error: {0}", e.Message);
                }
            }

            async Task<List<Dictionary<String, String>>> getQueryExecution(IAmazonAthena client, String id)
            {
                List<Dictionary<String, String>> items = new List<Dictionary<String, String>>();
                GetQueryExecutionResponse results = null;
                QueryExecution q = null;

                GetQueryExecutionRequest qReq = new GetQueryExecutionRequest()
                {
                    QueryExecutionId = id
                };

                do
                {
                    try
                    {
                        results = await client.GetQueryExecutionAsync(qReq);
                        q = results.QueryExecution;
                        Console.WriteLine("Status: {0}... {1}", q.Status.State, q.Status.StateChangeReason);

                        await Task.Delay(5000);
                    }
                    catch (InvalidRequestException e)
                    {
                        Console.WriteLine("GetQueryExec Error: {0}", e.Message);
                    }
                } while (q.Status.State == "RUNNING" || q.Status.State == "QUEUED");

                Console.WriteLine("Data Scanned for {0}: {1} Bytes", id, q.Statistics.DataScannedInBytes);


                GetQueryResultsRequest resReq = new GetQueryResultsRequest()
                {
                    QueryExecutionId = id,
                    MaxResults = 10
                };

                GetQueryResultsResponse resResp = null;
                do
                {
                    resResp = await client.GetQueryResultsAsync(resReq);
                    foreach (Row row in resResp.ResultSet.Rows)
                    {
                        Dictionary<String, String> dict = new Dictionary<String, String>();
                        for (var i = 0; i < resResp.ResultSet.ResultSetMetadata.ColumnInfo.Count; i++)
                        {
                            dict.Add(resResp.ResultSet.ResultSetMetadata.ColumnInfo[i].Name, row.Data[i].VarCharValue);
                        }
                        items.Add(dict);
                    }

                    if (resResp.NextToken != null)
                    {
                        resReq.NextToken = resResp.NextToken;
                    }
                } while (resResp.NextToken != null);


                return items;
            }

            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = "GET Method Executed",
                Headers = new Dictionary<string,
                string> { { "Content-Type", "application/json" } }
            };

            return response;
        }
    }
}