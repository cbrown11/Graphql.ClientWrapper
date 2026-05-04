namespace Graphql.ClientWrapper
{
    public interface IGraphQLClientFacade
    {
        dynamic SendMutation(string url, string query, out IReadOnlyList<string>? errors);
        dynamic SendMutation(string url, string query, dynamic variables, out IReadOnlyList<string>? errors);
        dynamic SendMutation(string accessToken, string url, string query, dynamic variables, out IReadOnlyList<string>? errors);
        dynamic SendQuery(string url, string query, out IReadOnlyList<string>? errors);
        dynamic SendQuery(string url, string query, dynamic variables, out IReadOnlyList<string>? errors);
        dynamic SendQuery(string accessToken, string url, string query, dynamic variables, out IReadOnlyList<string>? errors); 
    }
}