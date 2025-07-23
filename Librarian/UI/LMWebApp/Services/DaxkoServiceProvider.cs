using LMWebApp.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LMWebApp.Services;

public interface IDaxkoServiceProvider
{
    Task<bool> InitAsync();
    Task<List<DaxkoOffering>> GetOfferingsAsync();
    Task<DaxkoOffering?> GetOfferingByIdAsync(string id);
    Task<bool> UpdateOfferingAsync(DaxkoOffering offering);
    Task<bool> DeleteOfferingAsync(string id);
    Task<bool> ApproveOfferingAsync(string id);
    Task<bool> RejectOfferingAsync(string id);
    Task<List<DaxkoMember>> GetMembersAsync();
}

public class DaxkoServiceProvider : IDaxkoServiceProvider
{
    private readonly ILogger<DaxkoServiceProvider> _logger;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly List<DaxkoOffering> _offerings;
    private string? _accessToken;
    private bool _isInitialized = false;

    public DaxkoServiceProvider(ILogger<DaxkoServiceProvider> logger, HttpClient httpClient, IConfiguration configuration)
    {
        _logger = logger;
        _httpClient = httpClient;
        _configuration = configuration;
        _offerings = new List<DaxkoOffering>();
    }

    public async Task<bool> InitAsync()
    {
        try
        {
            _logger.LogInformation("Initializing Daxko service with authentication");
            
            var clientId = _configuration["Daxko:ClientId"];
            var clientSecret = _configuration["Daxko:ClientSecret"];
            var scope = _configuration["Daxko:Scope"];
            
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(scope))
            {
                _logger.LogError("Daxko configuration is missing required values");
                return false;
            }
            
            var authRequest = new
            {
                client_id = clientId,
                client_secret = clientSecret,
                scope = scope,
                grant_type = "client_credentials"
            };

            var json = JsonSerializer.Serialize(authRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://demo-api.partners.daxko.com/auth/token", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent);
                
                if (authResponse?.AccessToken != null)
                {
                    _accessToken = authResponse.AccessToken;
                    _isInitialized = true;
                    _logger.LogInformation("Daxko service initialized successfully");
                    return true;
                }
            }
            
            _logger.LogError("Failed to initialize Daxko service. Status: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing Daxko service");
            return false;
        }
    }

    public async Task<List<DaxkoOffering>> GetOfferingsAsync()
    {
        if (!_isInitialized)
        {
            _logger.LogWarning("Daxko service not initialized. Attempting to initialize...");
            await InitAsync();
        }
        
        try
        {
            _logger.LogInformation("Retrieving Daxko offerings from API");
            
            if (string.IsNullOrEmpty(_accessToken))
            {
                _logger.LogError("Access token is null or empty. Cannot make API call.");
                return new List<DaxkoOffering>();
            }
            
            // Add authorization header
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
            
            var response = await _httpClient.GetAsync("https://demo-api.partners.daxko.com/api/v1/programs/offerings/search");
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var offeringsResponse = JsonSerializer.Deserialize<DaxkoOfferingsResponse>(responseContent);
                
                if (offeringsResponse?.Offerings != null)
                {
                    _offerings.Clear();
                    _offerings.AddRange(offeringsResponse.Offerings);
                    _logger.LogInformation("Successfully retrieved {Count} offerings from API", offeringsResponse.Offerings.Count);
                    return _offerings;
                }
            }
            
            _logger.LogError("Failed to retrieve offerings from API. Status: {StatusCode}", response.StatusCode);
            return new List<DaxkoOffering>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Daxko offerings from API");
            return new List<DaxkoOffering>();
        }
    }

    public async Task<DaxkoOffering?> GetOfferingByIdAsync(string id)
    {
        _logger.LogInformation("Retrieving Daxko offering with ID: {Id}", id);
        await Task.Delay(50); // Simulate async operation
        return _offerings.FirstOrDefault(o => o.Id == id);
    }

    public async Task<bool> UpdateOfferingAsync(DaxkoOffering offering)
    {
        _logger.LogInformation("Updating Daxko offering with ID: {Id}", offering.Id);
        await Task.Delay(200); // Simulate async operation
        
        var existingOffering = _offerings.FirstOrDefault(o => o.Id == offering.Id);
        if (existingOffering != null)
        {
            var index = _offerings.IndexOf(existingOffering);
            _offerings[index] = offering;
            return true;
        }
        return false;
    }

    public async Task<bool> DeleteOfferingAsync(string id)
    {
        _logger.LogInformation("Deleting Daxko offering with ID: {Id}", id);
        await Task.Delay(150); // Simulate async operation
        
        var offering = _offerings.FirstOrDefault(o => o.Id == id);
        if (offering != null)
        {
            _offerings.Remove(offering);
            return true;
        }
        return false;
    }

    public async Task<bool> ApproveOfferingAsync(string id)
    {
        _logger.LogInformation("Approving Daxko offering with ID: {Id}", id);
        await Task.Delay(100); // Simulate async operation
        
        var offering = _offerings.FirstOrDefault(o => o.Id == id);
        if (offering != null)
        {
            // Add approval logic here
            _logger.LogInformation("Offering {Id} approved successfully", id);
            return true;
        }
        return false;
    }

    public async Task<bool> RejectOfferingAsync(string id)
    {
        _logger.LogInformation("Rejecting Daxko offering with ID: {Id}", id);
        await Task.Delay(100); // Simulate async operation
        
        var offering = _offerings.FirstOrDefault(o => o.Id == id);
        if (offering != null)
        {
            // Add rejection logic here
            _logger.LogInformation("Offering {Id} rejected successfully", id);
            return true;
        }
        return false;
    }

    public async Task<List<DaxkoMember>> GetMembersAsync()
    {
        if (!_isInitialized)
        {
            _logger.LogWarning("Daxko service not initialized. Attempting to initialize...");
            await InitAsync();
        }
        
        try
        {
            _logger.LogInformation("Retrieving Daxko members from API");
            
            if (string.IsNullOrEmpty(_accessToken))
            {
                _logger.LogError("Access token is null or empty. Cannot make API call.");
                return new List<DaxkoMember>();
            }
            
            // Add authorization header
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
            
            var response = await _httpClient.GetAsync("https://demo-api.partners.daxko.com/api/v1/member-search");
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Raw API response: {Response}", responseContent);
                
                var membersResponse = JsonSerializer.Deserialize<DaxkoMembersResponse>(responseContent);
                
                if (membersResponse?.Members != null)
                {
                    _logger.LogInformation("Successfully retrieved {Count} members from API", membersResponse.Members.Count);
                    if (membersResponse.Members.Count > 0)
                    {
                        var firstMember = membersResponse.Members[0];
                        _logger.LogInformation("First member: {FirstName} {LastName}, ID: {MemberId}", 
                            firstMember.FirstName, firstMember.LastName, firstMember.MemberId);
                    }
                    return membersResponse.Members;
                }
                else
                {
                    _logger.LogWarning("Members response is null or empty");
                }
            }
            
            _logger.LogError("Failed to retrieve members from API. Status: {StatusCode}", response.StatusCode);
            return new List<DaxkoMember>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Daxko members from API");
            return new List<DaxkoMember>();
        }
    }

}

public class AuthResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
    
    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }
    
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}