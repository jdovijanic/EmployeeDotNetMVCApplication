using EmployeeDotNetMVCApplication.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace EmployeeDotNetMVCApplication.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(ILogger<EmployeeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> GetEmployees()
        {
            string url = "https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code=vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();

                // Deserijalizacija stringa (content) u JSON formatu u listu zaposlenih
                List<Employee>? employees = JsonSerializer.Deserialize<List<Employee>>(content);

                List<EmployeeHours> employeeHours = new List<EmployeeHours>();

                Dictionary<string,double> dict = new Dictionary<string, double>();

                foreach (Employee employee in employees)
                {
                    string? name = employee.EmployeeName;

                    if (name == null) continue;

                    double hours = (employee.EndTimeUtc - employee.StarTimeUtc).TotalHours;

                    if (dict.ContainsKey(name))
                    {
                        double hrs = dict[name];
                        hrs += hours;
                        dict[name] = hrs;
                    }
                    else
                    {
                        dict.Add(name, hours);
                    }
                }

                foreach (var keyValuePair in dict)
                {
                    EmployeeHours empHours = new EmployeeHours
                    {
                        Name = keyValuePair.Key,
                        Hours = (int) keyValuePair.Value
                    };

                    employeeHours.Add(empHours);
                }

                employeeHours = employeeHours.OrderByDescending(g => g.Hours).ToList();          

                return View(employeeHours);
            }
            else
            {
                return View("Error");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
