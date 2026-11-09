using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RaceDay.Web.Services
{//start of namespace

    //what every API call returns: whether it worked, the status code, the data and a friendly message
    public class ApiResult<T>
    {//start of class

        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;

    }//end of class

    //the only class in the MVC app that talks to the Part 2 API, and it keeps the API login cookie inside the MVC session
    public class ApiClient
    {//start of class

        private const string ApiCookieName = ".AspNetCore.Session";
        private const string ApiCookieKey = "ApiSessionCookie";
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _accessor;

        public ApiClient(HttpClient http, IHttpContextAccessor accessor, IConfiguration configuration)
        {//start of constructor

            _http = http;
            _accessor = accessor;

            //the API address comes from appsettings.json so Docker can change it later
            _http.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"]!);

        }//end of constructor

        //sends a GET request and reads the JSON answer into T
        public Task<ApiResult<T>> GetAsync<T>(string path)
        {//start of GetAsync
            return SendAsync<T>(HttpMethod.Get, path, null);
        }//end of GetAsync

        //sends a POST request with a JSON body
        public Task<ApiResult<T>> PostAsync<T>(string path, object? body)
        {//start of PostAsync
            return SendAsync<T>(HttpMethod.Post, path, ToJson(body));
        }//end of PostAsync

        //sends a PUT request with a JSON body
        public Task<ApiResult<T>> PutAsync<T>(string path, object? body)
        {//start of PutAsync
            return SendAsync<T>(HttpMethod.Put, path, ToJson(body));
        }//end of PutAsync

        //sends a DELETE request
        public Task<ApiResult<object>> DeleteAsync(string path)
        {//start of DeleteAsync
            return SendAsync<object>(HttpMethod.Delete, path, null);
        }//end of DeleteAsync

        //sends an image file to the API as form-data (the API then stores it in Azure Blob Storage)
        public async Task<ApiResult<object>> UploadFileAsync(string path, IFormFile file)
        {//start of UploadFileAsync

            var form = new MultipartFormDataContent();
            var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            form.Add(fileContent, "file", file.FileName);

            return await SendAsync<object>(HttpMethod.Post, path, form);

        }//end of UploadFileAsync

        //downloads an image from the API so the MVC app can pass it on to the browser (null if there is none)
        public async Task<(byte[] Bytes, string ContentType)?> GetImageAsync(string path)
        {//start of GetImageAsync

            try
            {//start of try
                using var request = new HttpRequestMessage(HttpMethod.Get, path);
                AddApiCookie(request);
                using var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {//start of not found check
                    return null;
                }//end of not found check

                byte[] bytes = await response.Content.ReadAsByteArrayAsync();
                string contentType = response.Content.Headers.ContentType?.ToString() ?? "image/jpeg";
                return (bytes, contentType);
            }//end of try
            catch (Exception)
            {//start of catch
                return null;
            }//end of catch

        }//end of GetImageAsync

        //does the real work of every request: adds the login cookie, sends it, saves any new cookie and reads the answer
        private async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string path, HttpContent? content)
        {//start of SendAsync

            try
            {//start of try
                using var request = new HttpRequestMessage(method, path) { Content = content };
                AddApiCookie(request);

                using var response = await _http.SendAsync(request);
                SaveApiCookie(response);

                string body = await response.Content.ReadAsStringAsync();
                var result = new ApiResult<T> { Success = response.IsSuccessStatusCode, StatusCode = (int)response.StatusCode };

                if (response.IsSuccessStatusCode)
                {//start of success branch
                    if (!string.IsNullOrWhiteSpace(body) && typeof(T) != typeof(object))
                    {//start of read data
                        result.Data = JsonSerializer.Deserialize<T>(body, JsonOptions);
                    }//end of read data
                }//end of success branch
                else
                {//start of failure branch
                    result.Message = ExtractMessage(body, response.StatusCode);
                }//end of failure branch

                return result;
            }//end of try
            catch (Exception)
            {//start of catch
                //never show raw errors to the user, so a connection problem becomes a friendly message
                return new ApiResult<T> { Success = false, StatusCode = 0, Message = "The RaceDay service could not be reached. Please try again in a moment." };
            }//end of catch

        }//end of SendAsync

        //turns the object into a JSON request body
        private static StringContent? ToJson(object? body)
        {//start of ToJson

            if (body == null)
            {//start of no body check
                return null;
            }//end of no body check

            return new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        }//end of ToJson

        //adds the saved API login cookie (if there is one) to an outgoing request
        private void AddApiCookie(HttpRequestMessage request)
        {//start of AddApiCookie

            string? cookie = _accessor.HttpContext?.Session.GetString(ApiCookieKey);
            if (!string.IsNullOrEmpty(cookie))
            {//start of cookie check
                request.Headers.Add("Cookie", cookie);
            }//end of cookie check

        }//end of AddApiCookie

        //saves the API login cookie from a response into the MVC session
        private void SaveApiCookie(HttpResponseMessage response)
        {//start of SaveApiCookie

            if (!response.Headers.TryGetValues("Set-Cookie", out var values))
            {//start of no cookie check
                return;
            }//end of no cookie check

            foreach (string value in values)
            {//start of cookie loop
                if (value.StartsWith(ApiCookieName + "=", StringComparison.Ordinal))
                {//start of session cookie check
                    //keep only the name=value part, without the path and other settings
                    string pair = value.Split(';')[0];
                    _accessor.HttpContext?.Session.SetString(ApiCookieKey, pair);
                }//end of session cookie check
            }//end of cookie loop

        }//end of SaveApiCookie

        //finds a short, friendly message in the API's error answer
        private static string ExtractMessage(string body, HttpStatusCode status)
        {//start of ExtractMessage

            try
            {//start of try
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                //the API's own errors look like { "message": "..." }
                if (root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String)
                {//start of message branch
                    return message.GetString() ?? string.Empty;
                }//end of message branch

                //validation errors look like { "errors": { "Field": [ "..." ] } }
                if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                {//start of validation branch
                    foreach (var field in errors.EnumerateObject())
                    {//start of field loop
                        foreach (var text in field.Value.EnumerateArray())
                        {//start of text loop
                            return text.GetString() ?? string.Empty;
                        }//end of text loop
                    }//end of field loop
                }//end of validation branch
            }//end of try
            catch (Exception)
            {//start of catch
                //the answer was not JSON, so fall through to the standard messages below
            }//end of catch

            return status switch
            {//start of status messages
                HttpStatusCode.Unauthorized => "Please log in to continue.",
                HttpStatusCode.Forbidden => "You do not have permission to do that.",
                HttpStatusCode.NotFound => "We could not find what you were looking for.",
                _ => "Something went wrong. Please check your details and try again."
            };//end of status messages

        }//end of ExtractMessage

    }//end of class
}//end of namespace