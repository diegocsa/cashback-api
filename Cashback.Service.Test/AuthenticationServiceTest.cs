using Cashback.Domain;
using Cashback.Domain.Entities;
using Cashback.Domain.Exceptions;
using Cashback.Domain.Interfaces.Repository;
using Cashback.Domain.Model;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace Cashback.Service.Test
{
    public class AuthenticationServiceTest
    {
        private Mock<IResellerRepository> _repository;
        private AuthenticationService _service;


        public AuthenticationServiceTest()
        {

            var myConfiguration = new Dictionary<string, string>
                {
                    {"TokenJWT:Issuer", "X"} ,
                    { "TokenJWT:Audience", "Y"},
                    { "TokenJWT:DurationMinutes", "60"},
                    { "TokenJWT:Key", "Ef8RYiV92pNCFim1njVWcyZIhLyyURwUXvsWT4AP9RhpLksMiRFgROApgHIEVbzPfSANapchI6z9qCsnolWYO"}
                };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            _repository = new Mock<IResellerRepository>();
            _service = new AuthenticationService(_repository.Object, configuration, new NullLoggerFactory());
        }

        [Fact]
        public async Task Login_ValidCredentials_Token()
        {
            _repository.Setup(x => x.Retrieve(It.IsAny<Expression<Func<Reseller, bool>>>())).Returns(new[] { new Reseller() { Password = "grctFHsacYhp9DFjjM8GCXODdEaaj5q0I+qPSF5I9RA=" } }.AsQueryable);
            var token = await _service.Login(new LoginModel() { Email = "teste@teste.com", Password = "123456" });
            Assert.True(!string.IsNullOrEmpty(token));
        }

        [Fact]
        public async Task Login_PasswordWrong_Exception()
        {
            _repository.Setup(x => x.Retrieve(It.IsAny<Expression<Func<Reseller, bool>>>())).Returns(new[] { new Reseller() { Password = "XXXXXXX+qPSF5I9RA=" } }.AsQueryable);

            void act() => _service.Login(new LoginModel() { Email = "teste@teste.com", Password = "123456" });
            var exception = Assert.Throws<CashbackServiceException>(act);
            Assert.Equal(Messages.LoginOrPassInvalid, exception.Message);
        }

        [Fact]
        public async Task Login_UserDoesNotExists_Exception()
        {
            _repository.Setup(x => x.Retrieve(It.IsAny<Expression<Func<Reseller, bool>>>())).Returns(new[] { new Reseller() { } }.AsQueryable);

            void act() => _service.Login(new LoginModel() { Email = "teste@teste.com", Password = "123456" });
            var exception = Assert.Throws<CashbackServiceException>(act);
            Assert.Equal(Messages.LoginOrPassInvalid, exception.Message);

        }
    }
}
