using Cashback.API;
using Cashback.Domain;
using Cashback.Domain.Model;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;


namespace Cashback.IntegrationTest
{
    public class PurchaseOrderIntegrationTest : IClassFixture<TestFixtures<Startup>>
    {
        private TestFixtures<Startup> _fixture;
        private HttpClient _httpClient;
        private NewPurchaseOrderModel _validNewNewPurchaseOrderModel;

        public PurchaseOrderIntegrationTest(TestFixtures<Startup> fixture)
        {
            _fixture = fixture;
            _httpClient = fixture.Client;

            _validNewNewPurchaseOrderModel = new NewPurchaseOrderModel()
            {
                CPF = "153.509.460-56",
                Date = DateTime.Now.Date,
                Value = 600
            };
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
        public async Task Post_ValidValues_Success()
        {
            var request = "/api/v1/purchaseorder";

            var token = await GetToken();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            var response = await _httpClient.PostAsync(request, _fixture.AsStringContent(_validNewNewPurchaseOrderModel));
            response.EnsureSuccessStatusCode();
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
        }

        [Fact]
        public async Task Post_NoAuth_Unauthorized()
        {
            var request = "/api/v1/purchaseorder";

            var response = await _httpClient.PostAsync(request, _fixture.AsStringContent(_validNewNewPurchaseOrderModel));

            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Post_InvalidValues_BadRequest()
        {
            var request = "/api/v1/purchaseorder";

            var token = await GetToken();
            _validNewNewPurchaseOrderModel.CPF = "111.444.777-35";
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            var response = await _httpClient.PostAsync(request, _fixture.AsStringContent(_validNewNewPurchaseOrderModel));

            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
        }


        [Fact]
        public async Task Get_ValidValues_Success()
        {
            var requestInsert = "/api/v1/purchaseorder";

            var token = await GetToken();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            var responseInsert = await _httpClient.PostAsync(requestInsert, _fixture.AsStringContent(_validNewNewPurchaseOrderModel));
            var insertedId = JsonConvert.DeserializeObject<PurchaseOrderModel>(await responseInsert.Content.ReadAsStringAsync()).Id;

            var request = $"/api/v1/purchaseorder?cpf={_validNewNewPurchaseOrderModel.CPF.Replace("-", "").Replace(".", "")}&start={DateTime.Now:yyyy-MM-01}&end={DateTime.Now.AddDays(1):yyyy-MM-dd}";

            var response = await _httpClient.GetAsync(request);

            var items = JsonConvert.DeserializeObject<List<PurchaseOrderWithCashbackValueModel>>(await response.Content.ReadAsStringAsync());

            Assert.Contains(items, x => x.Id == insertedId);
            response.EnsureSuccessStatusCode();
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
        }

        [Fact]
        public async Task Get_NoAuth_Unauthorized()
        {
            var request = $"/api/v1/purchaseorder?cpf={_validNewNewPurchaseOrderModel.CPF.Replace("-", "").Replace(".", "")}&start={DateTime.Now:yyyy-MM-01}&end{DateTime.Now:yyyy-MM-28}";

            var response = await _httpClient.GetAsync(request);

            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Get_InvalidValues_BadRequest()
        {
            var request = $"/api/v1/purchaseorder?cpf=qq1q1q1q1&start={DateTime.Now:yyyy-MM-01}&end{DateTime.Now:yyyy-MM-28}";

            var token = await GetToken();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            var response = await _httpClient.GetAsync(request);

            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
        }

        [Fact]
        public async Task Get_NoItems_NoContent()
        {
            var request = $"/api/v1/purchaseorder?cpf={_validNewNewPurchaseOrderModel.CPF.Replace("-", "").Replace(".", "")}&start=2010-01-01&end=2010-01-01";

            var token = await GetToken();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            var response = await _httpClient.GetAsync(request);

            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
        }
    }
}
