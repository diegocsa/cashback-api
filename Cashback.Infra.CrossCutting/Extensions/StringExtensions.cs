using Cashback.Domain;
using Cashback.Domain.Constants;
using Cashback.Infra.CrossCutting.Validation;
using System;
using System.Linq;

namespace Cashback.Infra.CrossCutting.Extensions
{
    public static class StringExtensions
    {
        public static string ApplyCPFFormat(this string cpf)
        {
            if (!Validators.CPFIsValid(cpf))
                throw new FormatException(Messages.CPFInvalid);

            return Convert.ToInt64(cpf.Replace(".", string.Empty).Replace("-", string.Empty)).ToString(@"000\.000\.000\-00");
        }

        public static string SwitchStatusToDescription(this string status)
        {
            if (string.IsNullOrEmpty(status) || !new string[] { Constants.PURCHASE_STATUS_APPROVED, Constants.PURCHASE_STATUS_WAITING_APPROVAL}.Contains(status))
                return string.Empty;

            var txt = status == Constants.PURCHASE_STATUS_APPROVED
                                    ? Messages.TextApprovedStatusPurchaseOrder
                                    : Messages.TextWaitingApprovalStatusPurchaseOrder;

            return txt;
        }
    }
}
