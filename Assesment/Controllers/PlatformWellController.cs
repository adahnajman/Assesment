using Assesment.Data;
using Assesment.Models;
using Assesment.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Assesment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlatformWellController : ControllerBase
    {
        private readonly ApiService _apiService;
        private readonly AEMContext _ctx;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PlatformWellController(ApiService apiService, AEMContext dbContext, HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _apiService = apiService;
            _ctx = dbContext;
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        //Call this function to login api
        [HttpPost("Login")]
        public async Task<string> Login()
        {
            string token = await _apiService.CallLoginAPi(); 
            return token;
        }

        //call this function to sync data from RESTAPI to database
        [HttpGet("GetActualData")]
        public IActionResult GetActualData(string token)
        {
            try
            {
                var platforms = _apiService.SyncDataToDatabase(token);
                return Ok("Data synced successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while syncing data: {ex.Message}");
            }
        }

        //call this function to test dummy data ->to update current data
        [HttpGet("TestDummy")]
        public IActionResult TestDummy(string token)
        {
            try
            {
                var platforms = _apiService.TestDummy(token);
                return Ok("Data synced successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while syncing data: {ex.Message}");
            }
        }
    }
}
