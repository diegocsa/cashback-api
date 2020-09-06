using Cashback.Domain;
using Cashback.Domain.Constants;
using Cashback.Domain.Entities;
using Cashback.Domain.Exceptions;
using Cashback.Domain.Interfaces.Repository;
using Cashback.Domain.Model;
using Cashback.Infra.CrossCutting.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Cashback.Service.Test
{
    public class PurchaseOrderServiceTest
    {
        private Mock<IResellerRepository> _resellerRepository;
        private Mock<IPurchaseOrderRepository> _repository;
        private CalculatorRuleService _calculatorRuleService;
        private PurchaseOrderService _service;
        private NewPurchaseOrderModel _validNewNewPurchaseOrderModel;
        private Reseller _validReseller;

        public PurchaseOrderServiceTest()
        {
            _calculatorRuleService = new CalculatorRuleService(new NullLoggerFactory());
            _resellerRepository = new Mock<IResellerRepository>();
            _repository = new Mock<IPurchaseOrderRepository>();
            _service = new PurchaseOrderService(_resellerRepository.Object, _repository.Object, _calculatorRuleService, new NullLoggerFactory());

            _validReseller = new Reseller()
            {
                Id = Guid.NewGuid(),
                CPF = "153.509.460-56",
                AutoApproved = true,
                Name = "Usuário [153.509.460-56]",
                Email = "15350946056@teste.com.br",
                Password = "5o+mGdBwgsYcaGB4NGW5sVvFuYQ2+v+vLp5xQWkNAuQ="
            };

            _validNewNewPurchaseOrderModel = new NewPurchaseOrderModel()
            {
                CPF = _validReseller.CPF,
                Date = DateTime.Now.Date,
                Value = 600
            };

            TinyMapperConfiguration.AddTinyMapperConfiguration(null);

        }

        [Fact]
        public void Add_ValidNewPurchaseOrderModel_PurchaseOrderModel()
        {
            _resellerRepository.Setup(x => x.Retrieve(It.IsAny<Expression<Func<Reseller, bool>>>())).Returns(new[] { _validReseller }.AsQueryable);
            _repository.Setup(x => x.Add(It.IsAny<PurchaseOrder>()));
            var result = _service.Add(_validNewNewPurchaseOrderModel);

            _repository.Verify(x => x.Add(It.IsAny<PurchaseOrder>()), Times.Once);
            Assert.True(result.Exception == null);
            Assert.False(result.Result.Id == Guid.Empty);
        }

        [Fact]
        public async Task Add_InvalidCPF_Exception()
        {
            _validNewNewPurchaseOrderModel.CPF = "zzzzzzz";
            _resellerRepository.Setup(x => x.Retrieve(It.IsAny<Expression<Func<Reseller, bool>>>())).Returns(new[] { _validReseller }.AsQueryable);

            try
            {
                await _service.Add(_validNewNewPurchaseOrderModel);
            }
            catch (Exception ex)
            {
                _repository.Verify(x => x.Add(It.IsAny<PurchaseOrder>()), Times.Never);
                Assert.Equal(typeof(CashbackServiceException), ex.GetType());
                Assert.Equal(Messages.ResellerCPFInvalid, ex.Message);
            }

        }

        [Fact]
        public async Task Add_ModelNull_Exception()
        {
            _resellerRepository.Setup(x => x.Retrieve(It.IsAny<Expression<Func<Reseller, bool>>>())).Returns(new[] { _validReseller }.AsQueryable);

            try
            {
                await _service.Add(null);
            }
            catch (Exception ex)
            {
                _repository.Verify(x => x.Add(It.IsAny<PurchaseOrder>()), Times.Never);
                Assert.Equal(typeof(CashbackServiceException), ex.GetType());
                Assert.Equal(Messages.NullObject, ex.Message);
            }

        }

        [Fact]
        public async Task Add_SellerDoesnotExists_Exception()
        {
            _validReseller.CPF = "111.444.777-35";
            _resellerRepository.Setup(x => x.Retrieve(It.IsAny<Expression<Func<Reseller, bool>>>()));

            try
            {
                await _service.Add(_validNewNewPurchaseOrderModel);
            }
            catch (Exception ex)
            {
                _repository.Verify(x => x.Add(It.IsAny<PurchaseOrder>()), Times.Never);
                Assert.Equal(typeof(CashbackServiceException), ex.GetType());
                Assert.Equal(Messages.ResellerNotFoundByCPF, ex.Message);
            }

        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Add_ValueInvalid_Exception(decimal valuePO)
        {
            _resellerRepository.Setup(x => x.Retrieve(It.IsAny<Expression<Func<Reseller, bool>>>())).Returns(new[] { _validReseller }.AsQueryable);
            _validNewNewPurchaseOrderModel.Value = valuePO;
            try
            {
                await _service.Add(_validNewNewPurchaseOrderModel);
            }
            catch (Exception ex)
            {
                _repository.Verify(x => x.Add(It.IsAny<PurchaseOrder>()), Times.Never);
                Assert.Equal(typeof(CashbackServiceException), ex.GetType());
                Assert.Equal(Messages.PurchaseValueMustBeGreaterThanZero, ex.Message);
            }

        }

        [Theory]
        [InlineData(true, Constants.PURCHASE_STATUS_APPROVED)]
        [InlineData(false, Constants.PURCHASE_STATUS_WAITING_APPROVAL)]
        public async Task Add_AutoApprovedReseller_StatusBySeller(bool autoApproved, string status)
        {
            _validReseller.AutoApproved = autoApproved;
            _resellerRepository.Setup(x => x.Retrieve(It.IsAny<Expression<Func<Reseller, bool>>>())).Returns(new[] { _validReseller }.AsQueryable);
            var result = await _service.Add(_validNewNewPurchaseOrderModel);
            _repository.Verify(x => x.Add(It.IsAny<PurchaseOrder>()), Times.Once);
            Assert.Equal(status, result.Status);
        }

        [Fact]
        public void Gest_ValidValues_CorrectCalculation()
        {
            var assertTuple = new List<Tuple<Guid, decimal, decimal>>()
            {
               new Tuple<Guid, decimal, decimal>(new Guid("61e6294c-232a-4dee-8f56-8a53e4b1ad38"),10, (decimal)8.5),
                new Tuple<Guid, decimal, decimal>(new Guid("dabee139-12f2-47b6-8a8d-44748d5c19d3"),10, (decimal)8.5),
                new Tuple<Guid, decimal, decimal>(new Guid("467c6d3a-7772-4cd5-89c9-69925b84b99b"),10, (decimal)8.5),
                new Tuple<Guid, decimal, decimal>(new Guid("12355a50-4168-4905-930b-373013ba8012"),10, (decimal)8.5),
                new Tuple<Guid, decimal, decimal>(new Guid("1a834c21-74ee-451c-9be5-f5d33affe2ec"),10, (decimal)8.5),
                new Tuple<Guid, decimal, decimal>(new Guid("3566a440-f130-44d6-9f44-6eb8e86fc629"),10, (decimal)8.5),
                new Tuple<Guid, decimal, decimal>(new Guid("f2fa3e8b-20f0-4a72-9992-a3b5b520d7ea"),10, (decimal)8.5),
                new Tuple<Guid, decimal, decimal>(new Guid("4e7a6a48-65aa-4ad0-a47f-9449f8fa25be"),10, (decimal)8.5),
                new Tuple<Guid, decimal, decimal>(new Guid("5d295d32-d582-41a6-8aad-12c132bc2d94"),10, (decimal)8.5),
                new Tuple<Guid, decimal, decimal>(new Guid("fbf7bda6-46c0-4cc8-8fd4-e5677be7ce22"),10, (decimal)8.5),
                new Tuple<Guid, decimal, decimal>(new Guid("d44ecdf2-38ac-4e23-a5c0-f6a5a0b7fb53"),10, (decimal)8.5),
                new Tuple<Guid, decimal, decimal>(new Guid("3b4a39e3-b526-4a8e-9aad-348f839b356f"),15, (decimal)12.75),
                new Tuple<Guid, decimal, decimal>(new Guid("5821ed5c-6781-42d2-8a5e-894100d99666"),15, (decimal)12.75),
                new Tuple<Guid, decimal, decimal>(new Guid("991d430e-b980-4599-a351-b6efd1a7032b"),15, (decimal)12.75),
                new Tuple<Guid, decimal, decimal>(new Guid("eb00f62e-f16d-4736-9c4c-39df4dbf60f2"),15, (decimal)12.75),
                new Tuple<Guid, decimal, decimal>(new Guid("5edb9382-43c6-4a34-ad79-82270befa1c9"),15, (decimal)12.75),
                new Tuple<Guid, decimal, decimal>(new Guid("7fcf4cc0-8f56-47c0-acae-a1ff7e735ccb"),15, (decimal)12.75),
                new Tuple<Guid, decimal, decimal>(new Guid("8793cfad-b009-4814-873b-1d1fdfbf1968"),20, 17),
                new Tuple<Guid, decimal, decimal>(new Guid("77f85fd5-aca6-48dd-8ee0-e8f6b7700584"),20, 17),
                new Tuple<Guid, decimal, decimal>(new Guid("3780abc8-655e-48a5-8e6d-696273b325ea"),20, 17),
                new Tuple<Guid, decimal, decimal>(new Guid("3d17be51-ff67-40b1-b0f6-7e3200566eea"),20, 17),
                new Tuple<Guid, decimal, decimal>(new Guid("e11b2be9-c8d4-43f9-acaa-b79c21d2af2d"),20, 17),
                new Tuple<Guid, decimal, decimal>(new Guid("3b8cafbf-85d5-445c-b09b-92f22dd08618"),20, 17),
                new Tuple<Guid, decimal, decimal>(new Guid("32139a07-dfbf-4643-8457-1716083563d6"),20, 17),
                new Tuple<Guid, decimal, decimal>(new Guid("a57d6e03-ffc2-47ce-9bfe-6e524c1ba9a7"),20, 17),
                new Tuple<Guid, decimal, decimal>(new Guid("a387d4e4-7da1-4c63-8be0-ea286496bedf"),20, 17),
                new Tuple<Guid, decimal, decimal>(new Guid("36b7c4fb-30cc-4407-8ef4-68e177fd1e03"),20, 17),
                new Tuple<Guid, decimal, decimal>(new Guid("bd10cd5a-2150-44e4-805a-d458a6319436"),20, 17),
                new Tuple<Guid, decimal, decimal>(new Guid("68f26d3c-c114-4ace-a576-d97312f38ad1"),20, 17),
                new Tuple<Guid, decimal, decimal>(new Guid("64a9e7ac-5b07-47a6-ba76-e119554a8505"),20, 17),
                new Tuple<Guid, decimal, decimal>(new Guid("8c0b6021-92a3-4ed3-ae57-873d2db163ae"),20, 17)

        };
        
            var items = new List<PurchaseOrder>() {
                            new PurchaseOrder() { Id = new Guid("61e6294c-232a-4dee-8f56-8a53e4b1ad38"), Date = new DateTime(2020, 8, 1), Value = 85},
                            new PurchaseOrder() { Id = new Guid("dabee139-12f2-47b6-8a8d-44748d5c19d3"), Date = new DateTime(2020, 8, 2), Value = 85},
                            new PurchaseOrder() { Id = new Guid("467c6d3a-7772-4cd5-89c9-69925b84b99b"), Date = new DateTime(2020, 8, 3), Value = 85},
                            new PurchaseOrder() { Id = new Guid("12355a50-4168-4905-930b-373013ba8012"), Date = new DateTime(2020, 8, 4), Value = 85},
                            new PurchaseOrder() { Id = new Guid("1a834c21-74ee-451c-9be5-f5d33affe2ec"), Date = new DateTime(2020, 8, 5), Value = 85},
                            new PurchaseOrder() { Id = new Guid("3566a440-f130-44d6-9f44-6eb8e86fc629"), Date = new DateTime(2020, 8, 6), Value = 85},
                            new PurchaseOrder() { Id = new Guid("f2fa3e8b-20f0-4a72-9992-a3b5b520d7ea"), Date = new DateTime(2020, 8, 7), Value = 85},
                            new PurchaseOrder() { Id = new Guid("4e7a6a48-65aa-4ad0-a47f-9449f8fa25be"), Date = new DateTime(2020, 8, 8), Value = 85},
                            new PurchaseOrder() { Id = new Guid("5d295d32-d582-41a6-8aad-12c132bc2d94"), Date = new DateTime(2020, 8, 9), Value = 85},
                            new PurchaseOrder() { Id = new Guid("fbf7bda6-46c0-4cc8-8fd4-e5677be7ce22"), Date = new DateTime(2020, 8, 10), Value = 85},
                            new PurchaseOrder() { Id = new Guid("d44ecdf2-38ac-4e23-a5c0-f6a5a0b7fb53"), Date = new DateTime(2020, 8, 11), Value = 85},
                            new PurchaseOrder() { Id = new Guid("3b4a39e3-b526-4a8e-9aad-348f839b356f"), Date = new DateTime(2020, 8, 12), Value = 85},
                            new PurchaseOrder() { Id = new Guid("5821ed5c-6781-42d2-8a5e-894100d99666"), Date = new DateTime(2020, 8, 13), Value = 85},
                            new PurchaseOrder() { Id = new Guid("991d430e-b980-4599-a351-b6efd1a7032b"), Date = new DateTime(2020, 8, 14), Value = 85},
                            new PurchaseOrder() { Id = new Guid("eb00f62e-f16d-4736-9c4c-39df4dbf60f2"), Date = new DateTime(2020, 8, 15), Value = 85},
                            new PurchaseOrder() { Id = new Guid("5edb9382-43c6-4a34-ad79-82270befa1c9"), Date = new DateTime(2020, 8, 16), Value = 85},
                            new PurchaseOrder() { Id = new Guid("7fcf4cc0-8f56-47c0-acae-a1ff7e735ccb"), Date = new DateTime(2020, 8, 17), Value = 85},
                            new PurchaseOrder() { Id = new Guid("8793cfad-b009-4814-873b-1d1fdfbf1968"), Date = new DateTime(2020, 8, 18), Value = 85},
                            new PurchaseOrder() { Id = new Guid("77f85fd5-aca6-48dd-8ee0-e8f6b7700584"), Date = new DateTime(2020, 8, 19), Value = 85},
                            new PurchaseOrder() { Id = new Guid("3780abc8-655e-48a5-8e6d-696273b325ea"), Date = new DateTime(2020, 8, 20), Value = 85},
                            new PurchaseOrder() { Id = new Guid("3d17be51-ff67-40b1-b0f6-7e3200566eea"), Date = new DateTime(2020, 8, 21), Value = 85},
                            new PurchaseOrder() { Id = new Guid("e11b2be9-c8d4-43f9-acaa-b79c21d2af2d"), Date = new DateTime(2020, 8, 22), Value = 85},
                            new PurchaseOrder() { Id = new Guid("3b8cafbf-85d5-445c-b09b-92f22dd08618"), Date = new DateTime(2020, 8, 23), Value = 85},
                            new PurchaseOrder() { Id = new Guid("32139a07-dfbf-4643-8457-1716083563d6"), Date = new DateTime(2020, 8, 24), Value = 85},
                            new PurchaseOrder() { Id = new Guid("a57d6e03-ffc2-47ce-9bfe-6e524c1ba9a7"), Date = new DateTime(2020, 8, 25), Value = 85},
                            new PurchaseOrder() { Id = new Guid("a387d4e4-7da1-4c63-8be0-ea286496bedf"), Date = new DateTime(2020, 8, 26), Value = 85},
                            new PurchaseOrder() { Id = new Guid("36b7c4fb-30cc-4407-8ef4-68e177fd1e03"), Date = new DateTime(2020, 8, 27), Value = 85},
                            new PurchaseOrder() { Id = new Guid("bd10cd5a-2150-44e4-805a-d458a6319436"), Date = new DateTime(2020, 8, 28), Value = 85},
                            new PurchaseOrder() { Id = new Guid("68f26d3c-c114-4ace-a576-d97312f38ad1"), Date = new DateTime(2020, 8, 29), Value = 85},
                            new PurchaseOrder() { Id = new Guid("64a9e7ac-5b07-47a6-ba76-e119554a8505"), Date = new DateTime(2020, 8, 30), Value = 85},
                            new PurchaseOrder() { Id = new Guid("8c0b6021-92a3-4ed3-ae57-873d2db163ae"), Date = new DateTime(2020, 8, 31), Value = 85}};


            _resellerRepository.Setup(x => x.Retrieve(It.IsAny<Expression<Func<Reseller, bool>>>())).Returns(new[] { _validReseller }.AsQueryable);
            _repository.Setup(x => x.Retrieve(It.IsAny<Expression<Func<PurchaseOrder, bool>>>())).Returns(items.AsQueryable);
            
            var result = _service.Get("111.444.777-35", new DateTime(2020,8,1), new DateTime(2020,8,31));

            foreach (var item in result)
            {
                var line = assertTuple.Single(x => x.Item1 == item.Id);
                Assert.Equal(line.Item2, item.CashbackPercentual);
                Assert.Equal(line.Item3, item.CashbackValue);
            }
            
        }

        [Theory]
        [InlineData("dsadas")]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("111.444.777-99")]
        [InlineData("11144477799")]
        public void Get_CpfInvalid_Exception(string cpf)
        {
            _resellerRepository.Setup(x => x.Retrieve(It.IsAny<Expression<Func<Reseller, bool>>>())).Returns(new[] { _validReseller }.AsQueryable);
            _repository.Setup(x => x.Retrieve(It.IsAny<Expression<Func<PurchaseOrder, bool>>>()));

            try
            {
                _service.Get(cpf, new DateTime(2020, 8, 1), new DateTime(2020, 8, 31));
            }
            catch (Exception ex)
            {
                _resellerRepository.Verify(x => x.Retrieve(It.IsAny<Expression<Func<Reseller, bool>>>()), Times.Never);
                _repository.Verify(x => x.Retrieve(It.IsAny<Expression<Func<PurchaseOrder, bool>>>()), Times.Never);
                Assert.Equal(typeof(CashbackServiceException), ex.GetType());
                Assert.Equal(Messages.CPFInvalid, ex.Message);
            }
        }
    }
}
