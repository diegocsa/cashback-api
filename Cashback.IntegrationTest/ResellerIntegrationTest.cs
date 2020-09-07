using Cashback.API;
using Cashback.Domain;
using Cashback.Domain.Model;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Cashback.IntegrationTest
{
    public class ResellerIntegrationTest : IClassFixture<TestFixtures<Startup>>
    {
        private TestFixtures<Startup> _fixture;
        private HttpClient _httpClient;

        public ResellerIntegrationTest(TestFixtures<Startup> fixture)
        {
            _fixture = fixture;
            _httpClient = fixture.Client;
        }
        
        private string GenerateCPF()
        {
            int[] validateFirstDigitDArray = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] validateSecondDigitArraigity = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            
            var cpfBase = "0" + DateTime.Now.ToString("ddMMmmss");

            int stDigit = 0;
            for (int i = 0; i < 9; i++)
                stDigit += int.Parse(cpfBase[i].ToString()) * validateFirstDigitDArray[i];

            var modstDigit = stDigit % 11;
            stDigit = modstDigit < 2 ? 0 : 11 - modstDigit;

            cpfBase += stDigit.ToString();

            int ndDigit = 0;
            for (int i = 0; i < 10; i++)
                ndDigit += int.Parse(cpfBase[i].ToString()) * validateSecondDigitArraigity[i];

            var modNdDigit = ndDigit % 11;
            ndDigit = modNdDigit < 2 ? 0 : 11 - modNdDigit;

            cpfBase += ndDigit.ToString();
            return cpfBase;
        }

        private async Task<string> GetToken()
        {
            var requestLogin = "/api/v1/reseller/login";

            var responseLogin = await _httpClient.PostAsync(requestLogin, _fixture.AsStringContent(new LoginModel()
            {
                Email = "15350946056@teste.com.br",
                Password = "15350946056"
            }));

            return JsonConvert.DeserializeObject<dynamic>(await responseLogin.Content.ReadAsStringAsync()).token;
        }

        [Fact]
        public async Task Login_ValidLogin_SuccessAndReturnToken()
        {
            var request = "/api/v1/reseller/login";

            var response = await _httpClient.PostAsync(request, _fixture.AsStringContent(new LoginModel()
            {
                Email = "15350946056@teste.com.br",
                Password = "15350946056"
            }));

            var token = (string) JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync()).token;
            Assert.False(string.IsNullOrEmpty(token));
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Login_InvalidLogin_BadRequestAndReturnMessage()
        {
            var request = "/api/v1/reseller/login";

            var response = await _httpClient.PostAsync(request, _fixture.AsStringContent(new LoginModel()
            {
                Email = "XXX@teste.com.br",
                Password = "15350946056"
            }));

            var message = (string) JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync()).message;

            Assert.Equal(Messages.LoginOrPassInvalid, message);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetAccumulated_NotAuth_Unauthorized()
        {
            var request = "/api/v1/reseller/accumulated";

            var response = await _httpClient.GetAsync(request);

            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetAccumulated_Auth_Success()
        {
            var request = "/api/v1/reseller/accumulated?cpf=15350946056";
            var token = await GetToken();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var response = await _httpClient.GetAsync(request);
            
            response.EnsureSuccessStatusCode();
           
            _httpClient.DefaultRequestHeaders.Remove("Authorization");

        }

        [Fact]
        public async Task Post_ValidValues_Success()
        {
            var request = "/api/v1/reseller";
            var cpf = GenerateCPF();
            var model = new NewResellerModel()
            {
                CPF = cpf,
                Name = $"Test {cpf}",
                Email = $"{cpf}@test.com",
                Password = "XXX"
            };

            var response = await _httpClient.PostAsync(request, _fixture.AsStringContent(model));

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Post_InvalidValues_BadRequest()
        {
            var request = "/api/v1/reseller";
            var cpf = "aaa";
            var model = new NewResellerModel()
            {
                CPF = cpf,
                Name = $"Test {cpf}",
                Email = $"{cpf}@test.com",
                Password = "XXX"
            };

            var response = await _httpClient.PostAsync(request, _fixture.AsStringContent(model));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


    }
}
