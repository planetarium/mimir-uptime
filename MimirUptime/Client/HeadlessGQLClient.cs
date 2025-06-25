using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace MimirUptime.Client;

public class HeadlessGQLClient
{
    public const string GetTipQuery =
        @"
            query nodeStatus {
                nodeStatus {
                    tip {
                        index
                    }
                }
            }";
    private readonly HttpClient _httpClient;
    private readonly Uri _url;
    private readonly string? _issuer;
    private readonly string? _secret;
    private const int RetryAttempts = 3;
    private const int DelayInSeconds = 5;

    public HeadlessGQLClient(Uri url, string? issuer, string? secret)
    {
        _httpClient = new HttpClient();
        _url = url;
        _issuer = issuer;
        _secret = secret;

        _httpClient.Timeout = TimeSpan.FromSeconds(3);
    }

    private string GenerateJwtToken(string secret, string issuer)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var expiration = DateTime.UtcNow.AddMinutes(5);

        var token = new JwtSecurityToken(
            issuer: issuer,
            expires: expiration,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<(T response, string jsonResponse)> PostGraphQLRequestAsync<T>(
        string query,
        object? variables,
        CancellationToken stoppingToken = default
    )
    {
        for (int attempt = 0; attempt < RetryAttempts; attempt++)
        {
            try
            {
                var request = new GraphQLRequest { Query = query, Variables = variables };
                var jsonRequest = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, _url)
                {
                    Content = content,
                };

                if (_secret is not null && _issuer is not null)
                {
                    var token = GenerateJwtToken(_secret, _issuer);
                    httpRequest.Headers.Authorization = new AuthenticationHeaderValue(
                        "Bearer",
                        token
                    );
                }

                var response = await _httpClient.SendAsync(httpRequest, stoppingToken);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync(stoppingToken);
                Console.WriteLine($"GraphQL Response: {jsonResponse}");
                var graphQLResponse = JsonSerializer.Deserialize<GraphQLResponse<T>>(jsonResponse);

                if (
                    graphQLResponse is null
                    || graphQLResponse.Data is null
                    || graphQLResponse.Errors is not null
                )
                {
                    throw new HttpRequestException($"Response data is null. Response: {jsonResponse}");
                }

                return (graphQLResponse.Data, jsonResponse);
            }
            catch (Exception)
            {
                await Task.Delay(TimeSpan.FromSeconds(DelayInSeconds), stoppingToken);
            }
        }

        throw new HttpRequestException("All attempts to request the GraphQL endpoint failed.");
    }

    public async Task<(GetTipResponse response, string jsonResponse)> GetTipAsync(
        CancellationToken stoppingToken = default
    )
    {
        return await PostGraphQLRequestAsync<GetTipResponse>(GetTipQuery, null, stoppingToken);
    }
}
