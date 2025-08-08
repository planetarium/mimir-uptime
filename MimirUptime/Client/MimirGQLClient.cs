using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;

namespace MimirUptime.Client
{
    public class MimirGQLClient
    {
        public const string GetMetadataQuery =
            @"
            query aef($collectionName: String!){
                metadata(collectionName: $collectionName) {
                    collectionName
                    latestBlockIndex
                    pollerType
                }
            }
            ";

        public class GetMetadataResponse
        {
            [JsonPropertyName("metadata")]
            public required Metadata Metadata { get; set; }
        }

        public class Metadata
        {
            [JsonPropertyName("collectionName")]
            public required string collectionName { get; set; }

            [JsonPropertyName("latestBlockIndex")]
            public required long latestBlockIndex { get; set; }

            [JsonPropertyName("pollerType")]
            public required string pollerType { get; set; }
        }

        private readonly HttpClient _httpClient;
        private readonly Uri _url;
        private readonly string? _issuer;
        private readonly string? _secret;

        public MimirGQLClient(Uri url, string? issuer = null, string? secret = null)
        {
            _httpClient = new HttpClient();
            _url = url;
            _issuer = issuer;
            _secret = secret;

            _httpClient.Timeout = TimeSpan.FromSeconds(5);
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
            var graphQLResponse = JsonSerializer.Deserialize<GraphQLResponse<T>>(jsonResponse);

            if (
                graphQLResponse is null
                || graphQLResponse.Data is null
                || graphQLResponse.Errors is not null
            )
            {
                throw new HttpRequestException("Response data is null.");
            }

            return (graphQLResponse.Data, jsonResponse);
        }

        public async Task<(GetMetadataResponse response, string jsonResponse)> GetMetadataAsync(
            string key,
            CancellationToken stoppingToken = default
        )
        {
            return await PostGraphQLRequestAsync<GetMetadataResponse>(
                GetMetadataQuery,
                new { collectionName = key },
                stoppingToken
            );
        }
    }
}
