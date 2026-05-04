
using GraphQL;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json;

namespace Graphql.ClientWrapper
{
    public class GraphQLClientFacade : IGraphQLClientFacade
    {

        public GraphQLClientFacade() { }

        protected IGraphQLClient CreateGraphQLClient(string endpoint, string? accessToken = null)
        {
            var graphQLOptions = new GraphQLHttpClientOptions
            {
                EndPoint = new System.Uri(endpoint),
            };
            HttpClientHandler clientHandler = new HttpClientHandler();
            if (endpoint.Contains("localhost"))
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
            HttpClient client = new HttpClient(clientHandler);
            if (!string.IsNullOrEmpty(accessToken))
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken.Replace("Bearer ", string.Empty));
            return new GraphQLHttpClient(graphQLOptions, new NewtonsoftJsonSerializer(), client);
        }

        public dynamic SendQuery(string url, string query, out IReadOnlyList<string>? errors)
        {
            return SendQuery(url, query, null, out errors);
        }

        public dynamic SendQuery(string url, string query, dynamic? variables, out IReadOnlyList<string>? errors)
        {
            return SendQuery(null, url, query, variables, out errors);
        }

        public dynamic SendQuery(string accessToken, string url, string query, dynamic? variables, out IReadOnlyList<string>? errors)
        {
            var graphqlClient = CreateGraphQLClient(url, accessToken);
            var graphQLrequest = new GraphQLRequest
            {
                Query = System.Text.RegularExpressions.Regex.Unescape(query)
            };
            if (variables != null)
            {
                graphQLrequest.Variables = variables;
            }
            graphQLrequest.Variables = variables;
            var graphQLResponse = graphqlClient.SendQueryAsync<dynamic>(graphQLrequest).Result;
            errors = graphQLResponse?.Errors?.Select(x => x.Message).ToList();
            return graphQLResponse.Data;
        }

        public dynamic SendMutation(string url, string query, out IReadOnlyList<string>? errors)
        {
            return SendMutation(url, query, null, out errors);
        }

        public dynamic SendMutation(string url, string query, dynamic? variables, out IReadOnlyList<string>? errors)
        {
            return SendMutation(null, url, query, variables, out errors);
        }

        public dynamic SendMutation(string accessToken, string url, string query, dynamic? variables, out IReadOnlyList<string>? errors)
        {
            try
            {
                var graphqlClient = CreateGraphQLClient(url, accessToken);
                var graphQLrequest = new GraphQLRequest
                {
                    Query = System.Text.RegularExpressions.Regex.Unescape(query)
                };
                if (variables != null)
                {
                    graphQLrequest.Variables = variables;
                }
                graphQLrequest.Variables = variables;
                var graphQLResponse = graphqlClient.SendMutationAsync<dynamic>(graphQLrequest).Result;
                errors = graphQLResponse?.Errors?.Select(x => x.Message).ToList();
                return graphQLResponse.Data;
            }
            catch (AggregateException ex)
            {
                //TODO: Hack until the following get answered - https://github.com/graphql-dotnet/graphql-client/issues/374. Works on the Api correctly but not in tests
                var resultErr = ex.InnerExceptions?.OfType<GraphQL.Client.Http.GraphQLHttpRequestException>().Select(x => x.Content).ToList();
                errors = resultErr.Select(x => JsonConvert.DeserializeObject<RootError>(x)).FirstOrDefault().errors.Select(x => x.message).ToList();
                return "";
            }
            catch (System.Exception ex)
            {

                throw ex;
            }
        }
    }

    public class RootError
    {
            public List<Error> errors { get; set; }
    }

    public class Error
    {
        public string message { get; set; }
    }
}