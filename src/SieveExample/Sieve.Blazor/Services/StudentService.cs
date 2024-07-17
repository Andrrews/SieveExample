using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Sieve.Blazor.Models;

namespace Sieve.Blazor.Services
{
    public class StudentService
    {
        private readonly HttpClient _httpClient;

        public StudentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PagedList<StudentDTO>> GetStudentsAsync(string queryParams)
        {
            var response = await _httpClient.GetAsync($"Students/domain-filter?{queryParams}");
            var content = await response.Content.ReadAsStringAsync();
            var studentobj = JsonConvert.DeserializeObject<PagedList<StudentDTO>>(content);
            return studentobj;
        }
    }

 
}
