using Xunit;
using System;
using Aksio.Specifications;
using System.Collections.Generic;
using Graphql.ClientWrapper;

namespace Graphql.ClientWrapper.Integration.Tests
{
    public abstract class GraphqlClientWrapperSpec : Specification
    {
        protected GraphQLClientFacade SUT;
        protected System.Exception _exception;
        protected string _endpoint;
        protected string _query;
        protected dynamic _rootResponse;
        protected object _result;
        protected dynamic _variables;
        protected string _accessToken;
        protected string _username;
        protected string _password;
        protected IReadOnlyList<string>? _queryErrors;

        void Establish()
        {
            SUT = new GraphQLClientFacade(); 
            _endpoint = Environment.GetEnvironmentVariable("ENDPOINT");
            _username = Environment.GetEnvironmentVariable("USERNAME");
            _password = Environment.GetEnvironmentVariable("PASSWORD");
            if (string.IsNullOrEmpty(_endpoint)) _endpoint = "https://1uoaft1mn9.execute-api.eu-west-1.amazonaws.com/graphql";
            if (string.IsNullOrEmpty(_username)) _username = "alanh@otonomo.io";
            
            if (string.IsNullOrEmpty(_password)) _password = "Password1!"; //TODO: Remove
        }

    }


    public class when_sending_query : GraphqlClientWrapperSpec
    {
        void Establish()
        {
            _query = @"query {config {apiConfig{timeOutInSecs}}}";

        }
        void Because() => _exception = Catch.Exception(() => { _rootResponse = SUT.SendQuery(_endpoint, _query, out _queryErrors); });

        [Fact] void should_have_not_raised_any_errors() => _exception.ShouldBeNull();

        [Fact] void should_have_data() => (_rootResponse.config.apiConfig.timeOutInSecs as object).ShouldNotBeNull();

    }

    public class when_sending_query_with_variables_and_token : GraphqlClientWrapperSpec
    {
        void Establish()
        {
            if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password))
                throw new ArgumentException("Username and password to test has not been provided and required");
            _accessToken = GeneralFunctions.GetStandardUserToken(_username, _password);
            _query = @"query ($speedFilter: SpeedFilterInput!) {
                    segmentsSpeeds(speedFilterInput: $speedFilter) {
                        segmentId
                    }
                }
                ";
            _variables = new
            {
                speedFilter = new
                {
                    regionCodes = "E14000854",
                    year = 2021,
                    aggregationPeriod = "ANNUAL",
                    dayPeriod = "WEEKEND"
                }
            };

        }
        void Because() => _exception = Catch.Exception(() =>
        {
            _rootResponse = SUT.SendQuery(_accessToken, _endpoint, _query, _variables, out _queryErrors);
        });

        [Fact] void should_have_not_raised_any_errors() => _exception.ShouldBeNull();

        [Fact] void should_have_data() => (_rootResponse.segmentsSpeeds[0].segmentId as object).ShouldNotBeNull();

    }


    /*
    public class when_sending_mutation_with_variables_and_token : GraphqlClientWrapperSpec
    {
        protected string _mutation;
        void Establish()
        {
            if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password))
                throw new ArgumentException("Username and password to test has not been provided and required");
            _accessToken = GeneralFunctions.GetStandardUserToken(_username, _password);
            _mutation = "mutation ($createDownloadFilter:CreateDownloadInput!){createDownload(createDownloadInput: $createDownloadFilter)}";
            _variables = new
            {
                createDownloadFilter = new
                {
                    requestSource = "MATS",
                    endpoint = "http://localhost",
                    pagingQuery = "query{segmentsSpeedsWithPaging(first:100,speedFilterInput:{regionCodes:[\"E14000854\"],year:2021,dayPeriod:WEEKDAY,aggregationPeriod:ANNUAL}){pageInfo{hasNextPage,startCursor,endCursor}totalCount,nodes{segmentId,medianSpeed,roadSegment{start{longitude,latitude}end{longitude,latitude}}}}}",
                }
            };
        }
        void Because() => _exception = Catch.Exception(() =>
        {
            _rootResponse = SUT.SendMutation(_accessToken, _endpoint, _mutation, _variables, out _queryErrors);
        });

        [Fact] void should_have_not_raised_any_errors() => _exception.ShouldBeNull();

        [Fact] void should_have_data() => (_rootResponse.createDownload as object).ShouldNotBeNull();

    }
    */

}