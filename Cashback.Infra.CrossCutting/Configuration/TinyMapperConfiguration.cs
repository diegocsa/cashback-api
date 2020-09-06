using Cashback.Domain.Entities;
using Cashback.Domain.Model;
using Cashback.Infra.CrossCutting.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Nelibur.ObjectMapper;
using System;
using System.ComponentModel;
using System.Globalization;

namespace Cashback.Infra.CrossCutting.Configuration
{
    public static class TinyMapperConfiguration
    {
        public static void AddTinyMapperConfiguration(this IServiceCollection services)
        {
            TypeDescriptor.AddAttributes(typeof(PurchaseOrder), new TypeConverterAttribute(typeof(PurchaseOrderToPurchaseOrderModelConverter)));
            
            TinyMapper.Bind<Reseller, ResellerModel>();
            TinyMapper.Bind<NewResellerModel, Reseller>();
            TinyMapper.Bind<NewPurchaseOrderModel, PurchaseOrder>();
            TinyMapper.Bind<PurchaseOrder, PurchaseOrderModel>();
        }

        public class PurchaseOrderToPurchaseOrderModelConverter : TypeConverter
        {
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
                => destinationType == typeof(PurchaseOrderModel);

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                var concreteValue = (PurchaseOrder)value;
                return new PurchaseOrderModel
                {
                    Id = concreteValue.Id,
                    Date = concreteValue.Date,
                    Value = concreteValue.Value,
                    Status = concreteValue.Status.SwitchStatusToDescription(),
                    Reseller = TinyMapper.Map<ResellerModel>(concreteValue.Reseller)
                };
            }
        }
    }

    
}
