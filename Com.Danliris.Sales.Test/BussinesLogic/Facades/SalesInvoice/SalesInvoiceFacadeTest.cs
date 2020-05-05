﻿using Com.Danliris.Sales.Test.BussinesLogic.DataUtils.SalesInvoice;
using Com.Danliris.Sales.Test.BussinesLogic.Utils;
using Com.Danliris.Service.Sales.Lib;
using Com.Danliris.Service.Sales.Lib.BusinessLogic.Facades.SalesInvoice;
using Com.Danliris.Service.Sales.Lib.BusinessLogic.Logic.SalesInvoice;
using Com.Danliris.Service.Sales.Lib.Models.SalesInvoice;
using Com.Danliris.Service.Sales.Lib.Services;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Com.Danliris.Sales.Test.BussinesLogic.Facades.SalesInvoice
{
    public class SalesInvoiceFacadeTest : BaseFacadeTest<SalesDbContext, SalesInvoiceFacade, SalesInvoiceLogic, SalesInvoiceModel, SalesInvoiceDataUtil>
    {
        private const string ENTITY = "SalesInvoice";
        public SalesInvoiceFacadeTest() : base(ENTITY)
        {
        }

        protected override Mock<IServiceProvider> GetServiceProviderMock(SalesDbContext dbContext)
        {
            var serviceProviderMock = new Mock<IServiceProvider>();

            IIdentityService identityService = new IdentityService { Username = "Username" };

            serviceProviderMock
                .Setup(x => x.GetService(typeof(IdentityService)))
                .Returns(identityService);
            
            var salesInvoiceItemLogic = new SalesInvoiceItemLogic(serviceProviderMock.Object, identityService, dbContext);

            serviceProviderMock
                .Setup(x => x.GetService(typeof(SalesInvoiceItemLogic)))
                .Returns(salesInvoiceItemLogic);

            var salesInvoiceDetailLogic = new SalesInvoiceDetailLogic(serviceProviderMock.Object, identityService, dbContext);

            serviceProviderMock
                .Setup(x => x.GetService(typeof(SalesInvoiceDetailLogic)))
                .Returns(salesInvoiceDetailLogic);

            var salesInvoiceLogic = new SalesInvoiceLogic(serviceProviderMock.Object, identityService, dbContext);

            serviceProviderMock
                .Setup(x => x.GetService(typeof(SalesInvoiceLogic)))
                .Returns(salesInvoiceLogic);

            return serviceProviderMock;
        }

        [Fact]
        public async void Update_From_SalesReceipt_Success()
        {
            var dbContext = DbContext(GetCurrentMethod());
            var serviceProvider = GetServiceProviderMock(dbContext).Object;
            SalesInvoiceFacade facade = new SalesInvoiceFacade(serviceProvider, dbContext);

            var data = await DataUtil(facade, dbContext).GetTestData();
            var model = new SalesInvoiceUpdateModel()
            {
                IsPaidOff = false,
                TotalPaid = 1000,
            };


            var Response = await facade.UpdateFromSalesReceiptAsync((int)data.Id, model);
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public virtual async void Read_By_BuyerId_Success()
        {
            var dbContext = DbContext(GetCurrentMethod());
            var serviceProvider = GetServiceProviderMock(dbContext).Object;

            SalesInvoiceFacade facade = new SalesInvoiceFacade(serviceProvider, dbContext);

            var data = await DataUtil(facade).GetTestData();

            var Response = facade.ReadByBuyerId((int)data.BuyerId);

            Assert.NotEqual(Response.Count, 0);
        }
    }
}
