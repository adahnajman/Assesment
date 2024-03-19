using Assesment.Data;
using Assesment.Entities;
using Assesment.Models;
using Assesment.Models.PlatformModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Assesment.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        private readonly AEMContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiService(AEMContext ctx, HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _context = ctx;
        }

        public async Task<string> CallLoginAPi()
        {
            try
            {
                var apiBase = _configuration["AppSettings:Login"];
                var email = "user@aemenersol.com";
                var password = "Test@123";

                LoginModel loginAPI = new LoginModel();
                loginAPI.username = email;
                loginAPI.password = password;

                using (var httpClient = new HttpClient())
                {
                    StringContent content = new StringContent(JsonConvert.SerializeObject(loginAPI), Encoding.UTF8, "application/json");

                    using (var response = await httpClient.PostAsync(apiBase, content))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string token = await response.Content.ReadAsStringAsync();
                            token = token.Trim('"');


                            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                            //_httpContextAccessor.HttpContext.Session.SetString("Token", token);

                            return token;
                        }
                        else
                        {
                            string errorMessage = await response.Content.ReadAsStringAsync();
                            throw new Exception($"Failed to login: {response.StatusCode} - {errorMessage}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public async Task<List<PlatformVM>?> SyncDataToDatabase(string token)
        {
            try
            {
                var apiBase = _configuration["AppSettings:GetActualData"];
                var baseAddress = new Uri(apiBase, UriKind.Absolute);
                Platform platform = new Platform();

                using (var httpClient = new HttpClient())
                {
                    StringContent content = new StringContent(JsonConvert.SerializeObject(platform), Encoding.UTF8, "application/json");
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, apiBase);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    HttpResponseMessage response = httpClient.Send(request);
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonContent = await response.Content.ReadAsStringAsync();
                        var listing = JsonConvert.DeserializeObject<List<PlatformVM>>(jsonContent);
                        if (listing != null)
                        {
                            foreach (var item in listing)
                            {
                                Platform plt = new Platform();
                                plt.Id = item.Id;
                                plt.UniqueName = item.UniqueName;
                                plt.Longitude = item.Longitude;
                                plt.Latitude = item.Latitude;
                                plt.CreatedAt = item.CreatedAt;
                                plt.UpdatedAt = item.UpdatedAt;

                                //if exist: update
                                var existingPlatform = _context.Platforms.Find(plt.Id);
                                if (existingPlatform != null)
                                {
                                    existingPlatform.UniqueName = plt.UniqueName;
                                    existingPlatform.Longitude = plt.Longitude;
                                    existingPlatform.Latitude = plt.Latitude;
                                    existingPlatform.CreatedAt = plt.CreatedAt;
                                    existingPlatform.UpdatedAt = plt.UpdatedAt;
                                }
                                else
                                {
                                    _context.Platforms.Add(plt);

                                }
                                _context.SaveChanges();

                                var latestid = plt.Id;

                                foreach (var well in item.Well)
                                {
                                    Well wl = new Well();
                                    wl.PlatformId = plt.Id;
                                    wl.Longitude = well.Longitude;
                                    wl.Latitude = well.Latitude;
                                    wl.UniqueName = well.UniqueName;
                                    wl.CreatedAt = well.CreatedAt;
                                    wl.UpdatedAt = well.UpdatedAt;
                                    wl.Id = well.Id;

                                    //if exist: update
                                    var existWell = _context.Wells.Find(wl.Id);
                                    if (existWell != null)
                                    {
                                        existWell.Longitude = well.Longitude;
                                        existWell.Latitude = well.Latitude;
                                        existWell.UniqueName = well.UniqueName;
                                        existWell.CreatedAt = well.CreatedAt;
                                        existWell.UpdatedAt = well.UpdatedAt;
                                        existWell.Id = well.Id;
                                    }
                                    else
                                    {
                                        _context.Wells.Add(wl);
                                    }
                                    _context.SaveChanges();

                                }
                            }
                        }
                        return JsonConvert.DeserializeObject<List<PlatformVM>>(jsonContent);
                    }
                    else
                    {
                        // Handle the case where the request was not successful
                        throw new HttpRequestException($"Failed to fetch platforms. Status code: {response.StatusCode}");
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        public async Task<List<PlatformDummyVM>?> TestDummy(string token)
        {
            try
            {
                var apiBase = _configuration["AppSettings:TestDummy"];
                var baseAddress = new Uri(apiBase, UriKind.Absolute);
                Platform platform = new Platform();
                using (var httpClient = new HttpClient())
                {
                    StringContent content = new StringContent(JsonConvert.SerializeObject(platform), Encoding.UTF8, "application/json");
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, apiBase);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    HttpResponseMessage response = _httpClient.Send(request);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonContent = await response.Content.ReadAsStringAsync();
                        var listingDummy = JsonConvert.DeserializeObject<List<PlatformDummyVM>>(jsonContent);
                        if (listingDummy != null)
                        {
                            foreach (var item in listingDummy)
                            {
                                Platform plt = new Platform();
                                plt.Id = item.Id;
                                plt.UniqueName = item.UniqueName;
                                plt.Longitude = item.Longitude;
                                plt.Latitude = item.Latitude;
                                plt.UpdatedAt = item.LastUpdate;

                                //if exist: update
                                var existingPlatform = await _context.Platforms.FindAsync(plt.Id);
                                if (existingPlatform != null)
                                {
                                    existingPlatform.UniqueName = plt.UniqueName;
                                    existingPlatform.Longitude = plt.Longitude;
                                    existingPlatform.Latitude = plt.Latitude;
                                    existingPlatform.UpdatedAt = plt.UpdatedAt;
                                }
                                else
                                {
                                    _context.Platforms.Add(plt);
                                }

                                await _context.SaveChangesAsync();

                                var latestid = plt.Id;

                                foreach (var well in item.WELL)
                                {
                                    Well wl = new Well();
                                    wl.PlatformId = plt.Id;
                                    wl.Longitude = well.Longitude;
                                    wl.Latitude = well.Latitude;
                                    wl.UniqueName = well.UniqueName;
                                    wl.UpdatedAt = well.LastUpdate;
                                    wl.Id = well.Id;

                                    //if exist: update
                                    var existWell = await _context.Wells.FindAsync(wl.Id);
                                    if (existWell != null)
                                    {
                                        existWell.Longitude = well.Longitude;
                                        existWell.Latitude = well.Latitude;
                                        existWell.UniqueName = well.UniqueName;
                                        existWell.UpdatedAt = well.LastUpdate;
                                        existWell.Id = well.Id;
                                    }
                                    else
                                    {
                                        _context.Wells.Add(wl);
                                    }

                                    await _context.SaveChangesAsync();
                                }
                            }
                        }
                        return JsonConvert.DeserializeObject<List<PlatformDummyVM>>(jsonContent);
                    }
                    else
                    {
                        throw new HttpRequestException($"Failed to fetch platforms. Status code: {response.StatusCode}");
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
