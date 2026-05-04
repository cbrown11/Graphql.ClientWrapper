using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;
using Amazon;
using Microsoft.Extensions.Configuration;


namespace Graphql.ClientWrapper.Integration.Tests
{
    public static class GeneralFunctions
    {
 
         public static string GetStandardUserToken(string username, string password)
        {
            var user = GetCognitoUser(username);
            InitiateSrpAuthRequest authRequest = new InitiateSrpAuthRequest()
            {
                Password = password
            };
            AuthFlowResponse authResponse = user.StartWithSrpAuthAsync(authRequest).Result; // .ConfigureAwait(true);
            var token = authResponse.AuthenticationResult.AccessToken;
            return token;
        }

        public static CognitoUser GetCognitoUser(string username)
        {
            var AWSConfig = GetAWSConfig();
            var provider = new AmazonCognitoIdentityProviderClient("", "", RegionEndpoint.GetBySystemName(AWSConfig.Region));
            var cognitoUserPool = new CognitoUserPool(AWSConfig.UserPoolId, AWSConfig.AppClientId, provider);
            return new CognitoUser(username, AWSConfig.AppClientId, cognitoUserPool, provider);
        }

        public static AWSConfig GetAWSConfig()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var configuration = builder.Build();
            return configuration.GetSection("AWSIdentityConfig").Get<AWSConfig>();

        }

    }
}