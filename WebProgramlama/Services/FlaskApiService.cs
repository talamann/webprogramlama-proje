using System.Text;
using System.Text.Json;

namespace WebProgramlama.Services
{
    public class FlaskApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public FlaskApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["FlaskApi:BaseUrl"] ?? "http://localhost:5000";
        }

        // Health check
        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // Method 1: Check folder for plagiarism
        public async Task<PlagiarismCheckResult> CheckFolderForPlagiarismAsync(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    return new PlagiarismCheckResult
                    {
                        Success = false,
                        HasPlagiarism = false,
                        Message = "Folder does not exist"
                    };
                }

                var requestData = new { klasor_yolu = folderPath };
                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/kopya", content);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<PlagiarismApiResponse>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    return new PlagiarismCheckResult
                    {
                        Success = true,
                        HasPlagiarism = apiResponse.Sonuclar?.Any(s => s.Benzerlik >= 70.0) ?? false,
                        Message = "Plagiarism check completed successfully",
                        Comparisons = apiResponse.Sonuclar?.Select(s => new PlagiarismComparison
                        {
                            File1 = s.Dosya1,
                            File2 = s.Dosya2,
                            Similarity = s.Benzerlik
                        }).ToList() ?? new List<PlagiarismComparison>()
                    };
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ErrorApiResponse>(jsonResponse);
                    return new PlagiarismCheckResult
                    {
                        Success = false,
                        HasPlagiarism = false,
                        Message = $"API Error: {errorResponse?.Hata ?? "Unknown error"}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new PlagiarismCheckResult
                {
                    Success = false,
                    HasPlagiarism = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }

        // Method 2: Check folder for AI content
        public async Task<AiDetectionResult> CheckFolderForAiAsync(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    return new AiDetectionResult
                    {
                        Success = false,
                        HasAiContent = false,
                        Message = "Folder does not exist"
                    };
                }

                var requestData = new { klasor_yolu = folderPath };
                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/ai", content);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<AiApiResponse>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    return new AiDetectionResult
                    {
                        Success = true,
                        HasAiContent = apiResponse.Sonuclar?.Any(s => s.AiOlasilik >= 0.7) ?? false,
                        Message = "AI detection completed successfully",
                        Results = apiResponse.Sonuclar?.Select(s => new AiFileResult
                        {
                            FileName = s.Dosya,
                            HumanProbability = s.InsanOlasilik,
                            AiProbability = s.AiOlasilik,
                            Error = s.Hata
                        }).ToList() ?? new List<AiFileResult>()
                    };
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ErrorApiResponse>(jsonResponse);
                    return new AiDetectionResult
                    {
                        Success = false,
                        HasAiContent = false,
                        Message = $"API Error: {errorResponse?.Hata ?? "Unknown error"}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new AiDetectionResult
                {
                    Success = false,
                    HasAiContent = false,
                    Message = $"Error: {ex.Message}"
                };
            }
        }
    }

    // Result classes
    public class PlagiarismCheckResult
    {
        public bool Success { get; set; }
        public bool HasPlagiarism { get; set; }
        public string Message { get; set; }
        public List<PlagiarismComparison> Comparisons { get; set; } = new();
    }

    public class PlagiarismComparison
    {
        public string File1 { get; set; }
        public string File2 { get; set; }
        public double Similarity { get; set; }
    }

    public class AiDetectionResult
    {
        public bool Success { get; set; }
        public bool HasAiContent { get; set; }
        public string Message { get; set; }
        public List<AiFileResult> Results { get; set; } = new();
    }

    public class AiFileResult
    {
        public string FileName { get; set; }
        public double HumanProbability { get; set; }
        public double AiProbability { get; set; }
        public string Error { get; set; }
    }

    // Flask API response models
    public class PlagiarismApiResponse
    {
        public string Durum { get; set; }
        public List<PlagiarismApiResult> Sonuclar { get; set; }
    }

    public class PlagiarismApiResult
    {
        public string Dosya1 { get; set; }
        public string Dosya2 { get; set; }
        public double Benzerlik { get; set; }
    }

    public class AiApiResponse
    {
        public string Durum { get; set; }
        public List<AiApiResult> Sonuclar { get; set; }
    }

    public class AiApiResult
    {
        public string Dosya { get; set; }
        public double InsanOlasilik { get; set; }
        public double AiOlasilik { get; set; }
        public string Hata { get; set; }
    }

    public class ErrorApiResponse
    {
        public string Hata { get; set; }
    }
}