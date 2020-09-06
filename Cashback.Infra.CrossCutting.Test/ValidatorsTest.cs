using Cashback.Domain;
using Cashback.Domain.Constants;
using Cashback.Infra.CrossCutting.Extensions;
using Cashback.Infra.CrossCutting.Validation;
using System;
using Xunit;

namespace Cashback.Infra.CrossCutting.Test
{
    public class ValidatorsTest
    {
        [Theory]
        [InlineData("test@test.com")]
        [InlineData("test@test.com.br")]
        [InlineData("test@test.co")]
        [InlineData("test@test.io")]
        public void EmailIsValid_ValidEmail_True(string email)
        {
            var result = Validators.EmailIsValid(email);
            Assert.True(result);
        }

        [Theory]
        [InlineData("test#test.com")]
        [InlineData("test@test")]
        [InlineData("testtest.co")]
        public void EmailIsValid_InvalidEmail_False(string email)
        {
            var result = Validators.EmailIsValid(email);
            Assert.False(result);
        }

        [Theory]
        [InlineData("11144477735")]
        [InlineData("111.444.777-35")]
        [InlineData("78638433085")]
        [InlineData("47063617109")]
        public void CPFIsValid_ValidCpf_True(string cpf)
        {
            var result = Validators.CPFIsValid(cpf);
            Assert.True(result);
        }

        [Theory]
        [InlineData("111444s77735")]
        [InlineData("111a.444.777-35")]
        [InlineData("786333085")]
        [InlineData("4706a3617109")]
        [InlineData("string")]
        [InlineData("")]
        [InlineData(null)]
        public void CPFIsValid_InvalidCpf_False(string cpf)
        {
            var result = Validators.CPFIsValid(cpf);
            Assert.False(result);
        }


    }
}
