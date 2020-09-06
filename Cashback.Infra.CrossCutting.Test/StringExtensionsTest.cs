using Cashback.Domain;
using Cashback.Domain.Constants;
using Cashback.Infra.CrossCutting.Extensions;
using System;
using Xunit;

namespace Cashback.Infra.CrossCutting.Test
{
    public class StringExtensionsTest
    {
        [Theory]
        [InlineData("11144477735")]
        [InlineData("111.444.777-35")]
        public void ApplyCPFFormat_ValidCpfWithWithoutFormats_FormmatedPassword(string cpf)
        {
            var result = StringExtensions.ApplyCPFFormat(cpf);
            Assert.Equal("111.444.777-35", result);
        }

        [Theory]
        [InlineData("1s1144477735")]
        [InlineData("11.444.777-35")]
        [InlineData("1sadsa35")]
        [InlineData("")]
        [InlineData(null)]
        public void ApplyCPFFormat_InvalidCpfOrFormats_ThrowsException(string cpf)
        {
            Action act = () => StringExtensions.ApplyCPFFormat(cpf);
            var exception = Assert.Throws<FormatException>(act);
            Assert.Equal(Messages.CPFInvalid, exception.Message);
        }


        [Fact]
        public void SwitchStatusToDescription_ApprovedStatusLetter_TextToStatus()
        {
            var result = StringExtensions.SwitchStatusToDescription(Constants.PURCHASE_STATUS_APPROVED);
            Assert.Equal(Messages.TextApprovedStatusPurchaseOrder, result);
        }

        [Fact]
        public void SwitchStatusToDescription_WaitingApprovalStatusLetter_TextToStatus()
        {
            var result = StringExtensions.SwitchStatusToDescription(Constants.PURCHASE_STATUS_WAITING_APPROVAL);
            Assert.Equal(Messages.TextWaitingApprovalStatusPurchaseOrder, result);
        }

        [Theory]
        [InlineData("XXX")]
        [InlineData(null)]
        [InlineData("")]
        public void SwitchStatusToDescription_InvalidStatusLetter_StringEmpty(string status)
        {
            var result = StringExtensions.SwitchStatusToDescription(status);
            Assert.Equal(string.Empty, result);
        }
        

    }
}
