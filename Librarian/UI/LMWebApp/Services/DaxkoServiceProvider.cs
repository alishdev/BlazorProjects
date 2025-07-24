using System.Dynamic;
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
    Task<Tuple<string[], List<ExpandoObject>>>GetRandomAPI(string endpoint);
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

    private static List<ExpandoObject> DeserializeEndpointResponse(string responseContent, out string[] headers)
    {
        headers = new string[0];
        var result = new List<ExpandoObject>();
        
        try
        {
            // Parse the JSON response using JsonDocument for better control
            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;
            
            // Find the array property (it's not "total" or "has_more_records")
            string? arrayPropertyName = null;
            
            foreach (var property in root.EnumerateObject())
            {
                string propertyName = property.Name;
                if (propertyName != "total" && propertyName != "has_more_records" && 
                    property.Value.ValueKind == JsonValueKind.Array)
                {
                    arrayPropertyName = propertyName;
                    break;
                }
            }
            
            if (arrayPropertyName != null)
            {
                var array = root.GetProperty(arrayPropertyName);
                
                if (array.GetArrayLength() > 0)
                {
                    // Get headers from the first object in the array
                    var firstItem = array[0];
                    var headerList = new List<string>();
                    foreach (var prop in firstItem.EnumerateObject())
                    {
                        headerList.Add(prop.Name);
                    }
                    headers = headerList.ToArray();
                    
                    // Convert each item in the array to a dynamic object
                    foreach (var item in array.EnumerateArray())
                    {
                        dynamic dynamicItem = new ExpandoObject();
                        foreach (var prop in item.EnumerateObject())
                        {
                            //dynamicItem[prop.Name] = prop.Value.GetRawText();
                            ((IDictionary<string, object>)dynamicItem).Add(prop.Name, prop.Value.GetRawText());
                        }
                        result.Add(dynamicItem);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log error or handle gracefully
            Console.WriteLine($"Error deserializing response: {ex.Message}");
        }
        
        return result;
    }

    public async Task<Tuple<string[], List<ExpandoObject>>> GetRandomAPI(string endpoint)
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
                return new Tuple<string[], List<ExpandoObject>>(Array.Empty<string>(), new List<ExpandoObject>() );
            }
            
            // Add authorization header
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
            
            var response = await _httpClient.GetAsync(endpoint);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Raw API response: {Response}", responseContent);

                string[] headers;
                var list = DeserializeEndpointResponse(responseContent, out headers);
                return new Tuple<string[], List<ExpandoObject>>(headers, list);
            }
            
            _logger.LogError("Failed to retrieve members from API. Status: {StatusCode}", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Daxko members from API");
        }
        
        return new Tuple<string[], List<ExpandoObject>>(Array.Empty<string>(), new List<ExpandoObject>() );
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