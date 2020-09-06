using Cashback.Domain;
using Cashback.Domain.Constants;
using Cashback.Domain.Entities;
using Cashback.Domain.Exceptions;
using Cashback.Domain.Interfaces.Repository;
using Cashback.Domain.Model;
using Cashback.Infra.CrossCutting.Configuration;
using Cashback.Infra.CrossCutting.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Cashback.Service.Test
{
    public class FakeHttpMessageHandler : DelegatingHandler
    {
        private HttpResponseMessage _fakeResponse;

        public FakeHttpMessageHandler(HttpResponseMessage responseMessage)
        {
            _fakeResponse = responseMessage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await Task.FromResult(_fakeResponse);
        }
    }

    public class ResellerServiceTest
    {
        private IConfigurationRoot _configuration;
        private Mock<IResellerRepository> _repository;
        private ResellerService _service;
        private Reseller _validReseller;

        public ResellerServiceTest()
        {
            IHttpClientFactory httpClientFactoryMock = HttpFakeConfig();

            var myConfiguration = new Dictionary<string, string>
                {
                    {"BalanceAPI:RelativeURL", "v1/cashback?cpf=11144477735"}
                };

            _configuration = new ConfigurationBuilder()
               .AddInMemoryCollection(myConfiguration)
               .Build();

            _repository = new Mock<IResellerRepository>();
            _service = new ResellerService(_repository.Object, httpClientFactoryMock, _configuration, new NullLoggerFactory());

            _validReseller = new Reseller()
            {
                Id = Guid.NewGuid(),
                CPF = "153.509.460-56",
                AutoApproved = true,
                Name = "Usuário [153.509.460-56]",
                Email = "15350946056@teste.com.br",
                Password = "5o+mGdBwgsYcaGB4NGW5sVvFuYQ2+v+vLp5xQWkNAuQ="
            };

            TinyMapperConfiguration.AddTinyMapperConfiguration(null);

        }

        private static IHttpClientFactory HttpFakeConfig(string name = "balance-api", bool validReturn = true)
        {
            var _objectToReturn = JsonConvert.SerializeObject(new
            {
                statusCode = 200,
                body = new { credit = 4321 }
            });

            var httpClientFactoryMock = Substitute.For<IHttpClientFactory>();
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    validReturn
                        ? _objectToReturn
                        : JsonConvert.SerializeObject(new
                        {
                            foo = "bar"
                        }), Encoding.UTF8, "application/json")
            }); ;
            var fakeHttpClient = new HttpClient(fakeHttpMessageHandler)
            {
                BaseAddress = new Uri("https://mdaqk8ek5j.execute-api.us-east-1.amazonaws.com")
            };
            httpClientFactoryMock.CreateClient(name).Returns(fakeHttpClient);
            return httpClientFactoryMock;
        }

        [Fact]
        public async Task GetAccumulated_APIOK_Value()
        {
            var mockedValue = 4321;
            _repository.Setup(x => x.Retrieve(It.IsAny<Expression<Func<Reseller, bool>>>())).Returns(new[] { _validReseller }.AsQueryable);
            var result = await _service.GetAccumulated(_validReseller.CPF);
            Assert.Equal(mockedValue, result);
        }

        [Fact]
        public async Task GetAccumulated_APIError_Exception()
        {
            IHttpClientFactory httpClientFactoryMock = HttpFakeConfig("error");

            try
            {
                _service = new ResellerService(_repository.Object, httpClientFactoryMock, _configuration, new NullLoggerFactory());
                _repository.Setup(x => x.Retrieve(It.IsAny<Expression<Func<Reseller, bool>>>())).Returns(new[] { _validReseller }.AsQueryable);
                await _service.GetAccumulated(_validReseller.CPF);
            }
            catch (Exception ex)
            {
                Assert.Equal(typeof(CashbackServiceException), ex.GetType());
                Assert.Equal(Messages.ErrorOnAccessExternalAPI, ex.Message);
            }


        }

        [Fact]
        public async Task GetAccumulated_APIErrorOnConvertReturn_Exception()
        {
            IHttpClientFactory httpClientFactoryMock = HttpFakeConfig(validReturn: false);

            try
            {
                _service = new ResellerService(_repository.Object, httpClientFactoryMock, _configuration, new NullLoggerFactory());
                _repository.Setup(x => x.Retrieve(It.IsAny<Expression<Func<Reseller, bool>>>())).Returns(new[] { _validReseller }.AsQueryable);
                await _service.GetAccumulated(_validReseller.CPF);
            }
            catch (Exception ex)
            {
                Assert.Equal(typeof(CashbackServiceException), ex.GetType());
                Assert.Equal(Messages.ErrorOnConvertResult, ex.Message);
            }
        }

        [Theory]
        [InlineData("546456")]
        [InlineData("11144477788")]
        [InlineData("das")]
        [InlineData("")]
        [InlineData(null)]
        public async Task GetAccumulated_CPFInvalid_Exception(string cpf)
        {
            IHttpClientFactory httpClientFactoryMock = HttpFakeConfig(validReturn: false);
            try
            {
                _service = new ResellerService(_repository.Object, httpClientFactoryMock, _configuration, new NullLoggerFactory());
                _repository.Setup(x => x.Retrieve(It.IsAny<Expression<Func<Reseller, bool>>>())).Returns(new[] { _validReseller }.AsQueryable);
                await _service.GetAccumulated(cpf);
            }
            catch (Exception ex)
            {
                Assert.Equal(typeof(CashbackServiceException), ex.GetType());
                Assert.Equal(Messages.CPFInvalid, ex.Message);
            }
        }

        [Theory]
        [InlineData("111.444.777-35")]
        [InlineData("11144477735")]
        public async Task GetAccumulated_CPFNotInDatabase_Exception(string cpf)
        {
            IHttpClientFactory httpClientFactoryMock = HttpFakeConfig(validReturn: false);
            try
            {
                _service = new ResellerService(_repository.Object, httpClientFactoryMock, _configuration, new NullLoggerFactory());
                _repository.Setup(x => x.Retrieve(It.IsAny<Expression<Func<Reseller, bool>>>()));
                await _service.GetAccumulated(cpf);
            }
            catch (Exception ex)
            {
                Assert.Equal(typeof(CashbackServiceException), ex.GetType());
                Assert.Equal(Messages.ResellerNotFoundByCPF, ex.Message);
            }
        }


        [Theory]
        [InlineData("dsadas")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("111.444.777-99")]
        [InlineData("11144477799")]
        public async Task Add_InvalidCPF_Exception(string cpf)
        {
            try
            {
                var model = new NewResellerModel()
                {
                    CPF = cpf,
                    Name = "Name X",
                    Email = $"abc12345@teste.com",
                    Password = "xxxx"

                };
                await _service.Add(model);
            }
            catch (Exception ex)
            {
                _repository.Verify(x => x.Add(It.IsAny<Reseller>()), Times.Never);
                Assert.Equal(typeof(CashbackServiceException), ex.GetType());
                Assert.Equal(Messages.CPFInvalid, ex.Message);
            }

        }

        [Theory]
        [InlineData("dsadas")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("111.444.777-99")]
        [InlineData("111.444.777-99@")]
        [InlineData("11144477799@tes")]
        [InlineData("11144477799@teste.")]
        public async Task Add_InvalidEmail_Exception(string email)
        {
            try
            {
                var model = new NewResellerModel()
                {
                    CPF = "11144477735",
                    Name = "Name X",
                    Email = email,
                    Password = "xxxx"

                };
                await _service.Add(model);
            }
            catch (Exception ex)
            {
                _repository.Verify(x => x.Add(It.IsAny<Reseller>()), Times.Never);
                Assert.Equal(typeof(CashbackServiceException), ex.GetType());
                Assert.Equal(Messages.EmailInvalid, ex.Message);
            }
        }

        [Fact]
        public async Task Add_NameInvalid_Exception()
        {
            Exception _ex = new Exception();
            try
            {
                var model = new NewResellerModel()
                {
                    CPF = "11144477735",
                    Name = new string('A', 101),
                    Email = "teste@teste.com",
                    Password = "xxxx"

                };
                await _service.Add(model);
            }
            catch (Exception ex)
            {
                _repository.Verify(x => x.Add(It.IsAny<Reseller>()), Times.Never);
                _ex = ex;
            }
            Assert.Equal(typeof(CashbackServiceException), _ex.GetType());
            Assert.Equal(Messages.NameShouldBeLessThan100Chars, _ex.Message);

        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task Add_NameEmpty_Exception(string name)
        {
            Exception _ex = new Exception();
            try
            {
                var model = new NewResellerModel()
                {
                    CPF = "11144477735",
                    Name = name,
                    Email = "teste@teste.com",
                    Password = "xxxx"

                };
                await _service.Add(model);
            }
            catch (Exception ex)
            {
                _repository.Verify(x => x.Add(It.IsAny<Reseller>()), Times.Never);
                _ex = ex;
            }
            Assert.Equal(typeof(CashbackServiceException), _ex.GetType());
            Assert.Equal(Messages.NameIsObrigatory, _ex.Message);

        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task Add_PasswordEmpty_Exception(string password)
        {
            Exception _ex = new Exception();
            try
            {
                var model = new NewResellerModel()
                {
                    CPF = "11144477735",
                    Name = "name",
                    Email = "teste@teste.com",
                    Password = password

                };
                await _service.Add(model);
            }
            catch (Exception ex)
            {
                _repository.Verify(x => x.Add(It.IsAny<Reseller>()), Times.Never);
                _ex = ex;
            }
            Assert.Equal(typeof(CashbackServiceException), _ex.GetType());
            Assert.Equal(Messages.PasswordIsObrigatory, _ex.Message);

        }

        [Fact]
        public async Task Add_ModelNull_Exception()
        {
            try
            {
                await _service.Add(null);
            }
            catch (Exception ex)
            {
                _repository.Verify(x => x.Add(It.IsAny<Reseller>()), Times.Never);
                Assert.Equal(typeof(CashbackServiceException), ex.GetType());
                Assert.Equal(Messages.NullObject, ex.Message);
            }

        }

        [Fact]
        public async Task Add_EmailInUse_Exception()
        {
            var model = new NewResellerModel()
            {
                CPF = "11144477735",
                Name = "name",
                Email = "teste@teste.com",
                Password = "XXX"

            };

            _repository.Setup(m => m.Retrieve(x => x.Email == model.Email)).Returns(new[] { _validReseller }.AsQueryable);

            Exception _ex = new Exception();
            try
            {
                await _service.Add(model);
            }
            catch (Exception ex)
            {
                _repository.Verify(x => x.Add(It.IsAny<Reseller>()), Times.Never);
                _ex = ex;
            }
            Assert.Equal(typeof(CashbackServiceException), _ex.GetType());
            Assert.Equal(Messages.ResellerEmailInUse, _ex.Message);
        }

        [Fact]
        public async Task Add_CpfInUse_Exception()
        {
            var model = new NewResellerModel()
            {
                CPF = "11144477735",
                Name = "name",
                Email = "teste@teste.com",
                Password = "XXX"

            };

            _repository.Setup(m => m.Retrieve(x => x.CPF == model.CPF.ApplyCPFFormat())).Returns(new[] { _validReseller }.AsQueryable);

            Exception _ex = new Exception();
            try
            {
                await _service.Add(model);
            }
            catch (Exception ex)
            {
                _repository.Verify(x => x.Add(It.IsAny<Reseller>()), Times.Never);
                _ex = ex;
            }
            Assert.Equal(typeof(CashbackServiceException), _ex.GetType());
            Assert.Equal(Messages.ResellerCPFInUse, _ex.Message);
        }

        [Fact]
        public async Task Add_ValidData_SaveAndReturn()
        {
            var model = new NewResellerModel()
            {
                CPF = "11144477735",
                Name = "name",
                Email = "teste@teste.com",
                Password = "XXX"

            };

            _repository.Setup(x => x.Retrieve(It.IsAny<Expression<Func<Reseller, bool>>>()));

            Exception _ex = new Exception();
            
            var result = await _service.Add(model);
            _repository.Verify(x => x.Add(It.IsAny<Reseller>()), Times.Once);

            Assert.Equal(model.Email, result.Email);
            Assert.Equal(model.Name, result.Name);
            Assert.Equal(model.CPF.ApplyCPFFormat(),result.CPF);
            Assert.NotEqual(Guid.Empty, result.Id);
        }
    }
}
