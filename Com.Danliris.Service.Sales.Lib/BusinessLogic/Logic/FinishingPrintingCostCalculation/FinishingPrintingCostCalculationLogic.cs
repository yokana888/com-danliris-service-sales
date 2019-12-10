﻿using Com.Danliris.Service.Sales.Lib.Models.FinishingPrintingCostCalculation;
using Com.Danliris.Service.Sales.Lib.Services;
using Com.Danliris.Service.Sales.Lib.Utilities;
using Com.Danliris.Service.Sales.Lib.Utilities.BaseClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Dynamic.Core;
using Com.Moonlay.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Com.Moonlay.NetCore.Lib;

namespace Com.Danliris.Service.Sales.Lib.BusinessLogic.Logic.FinishingPrintingCostCalculation
{
    public class FinishingPrintingCostCalculationLogic : BaseLogic<FinishingPrintingCostCalculationModel>
    {
        public FinishingPrintingCostCalculationLogic(IIdentityService IdentityService, SalesDbContext dbContext) : base(IdentityService, dbContext)
        {
        }

        public override ReadResponse<FinishingPrintingCostCalculationModel> Read(int page, int size, string order, List<string> select, string keyword, string filter)
        {
            var query = DbSet.AsQueryable();
            List<string> SearchAttributes = new List<string>()
            {
                "ProductionOrderNo", "PreSalesContractNo", "UnitName"
            };
            query = QueryHelper<FinishingPrintingCostCalculationModel>.Search(query, SearchAttributes, keyword);
            List<string> SelectedFields = new List<string>()
            {
                "Id", "CreatedUtc", "LastModifiedUtc", "ProductionOrderNo", "PreSalesContract", "OrderQuantity", "ConfirmPrice", "IsPosted"
            };
            Dictionary<string, object> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(filter);
            query = QueryHelper<FinishingPrintingCostCalculationModel>.Filter(query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(order);
            query = QueryHelper<FinishingPrintingCostCalculationModel>.Order(query, OrderDictionary);

            Pageable<FinishingPrintingCostCalculationModel> pageable = new Pageable<FinishingPrintingCostCalculationModel>(query, page - 1, size);
            List<FinishingPrintingCostCalculationModel> data = pageable.Data.ToList();
            int totalData = pageable.TotalCount;

            return new ReadResponse<FinishingPrintingCostCalculationModel>(data, totalData, OrderDictionary, SelectedFields);
        }

        public override Task<FinishingPrintingCostCalculationModel> ReadByIdAsync(long id)
        {
            return DbSet.Include(x => x.Machines).ThenInclude(y => y.Chemicals).FirstOrDefaultAsync(d => d.Id.Equals(id) && d.IsDeleted.Equals(false));
        }

        public override void Create(FinishingPrintingCostCalculationModel model)
        {
            do
            {
                model.Code = CodeGenerator.Generate();
            }
            while (DbSet.Any(entity => entity.Code.Equals(model.Code)));
            ProductionOrderNumberGenerator(model);
            int indexM = 0;
            model.Machines.ToList().ForEach(machine =>
            {
                machine.Index = indexM;
                indexM++;
                EntityExtension.FlagForCreate(machine, IdentityService.Username, "sales-service");
                machine.Chemicals.ToList().ForEach(chemical =>
                {
                    EntityExtension.FlagForCreate(chemical, IdentityService.Username, "sales-service");
                });
            });
            base.Create(model);
        }

        public override async Task DeleteAsync(long id)
        {
            FinishingPrintingCostCalculationModel model = await ReadByIdAsync(id);
            EntityExtension.FlagForDelete(model, IdentityService.Username, "sales-service", true);

            foreach (var machine in model.Machines)
            {
                EntityExtension.FlagForDelete(machine, IdentityService.Username, "sales-service", true);
                foreach (var chemical in machine.Chemicals)
                {
                    EntityExtension.FlagForDelete(chemical, IdentityService.Username, "sales-service", true);
                }
            }

            DbSet.Update(model);
        }

        public override void UpdateAsync(long id, FinishingPrintingCostCalculationModel model)
        {
            foreach(var machine in model.Machines)
            {
                EntityExtension.FlagForUpdate(machine, IdentityService.Username, "sales-service");
                foreach(var chemical in machine.Chemicals)
                {
                    EntityExtension.FlagForUpdate(chemical, IdentityService.Username, "sales-service");

                }
            }
            base.UpdateAsync(id, model);
        }

        public async Task CCPost(List<long> listId)
        {
            foreach (var id in listId)
            {
                var model = await ReadByIdAsync(id);
                model.IsPosted = true;
                UpdateAsync(id, model);
            }
        }

        private void ProductionOrderNumberGenerator(FinishingPrintingCostCalculationModel model)
        {
            var lastData = DbSet.IgnoreQueryFilters().Where(w => w.UnitName.Equals(model.UnitName)).OrderByDescending(x => x.CreatedUtc);

            string DocumentType = model.UnitName.ToLower().Equals("printing") ? "P" : "F";

            int YearNow = DateTime.Now.Year;
            int MonthNow = DateTime.Now.Month;
            int count = 0;
            if (lastData.Count() == 0)
            {
                count = 1;
                model.ProductionOrderNo = $"{DocumentType}/{YearNow}/{count.ToString().PadLeft(4, '0')}";
            }
            else
            {
                var lastCC = lastData.FirstOrDefault();
                if (YearNow > lastCC.CreatedUtc.Year)
                {
                    count = 1;
                    model.ProductionOrderNo = $"{DocumentType}/{YearNow}/{count.ToString().PadLeft(4, '0')}";
                }
                else
                {
                    count = lastData.Count() + 1;
                    model.ProductionOrderNo = $"{DocumentType}/{YearNow}/{count.ToString().PadLeft(4, '0')}";
                }
            }
        }

    }
}
