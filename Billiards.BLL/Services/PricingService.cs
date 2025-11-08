using Billiards.DAL.Models;
using Billiards.DAL.Repositories;

namespace Billiards.BLL.Services;

public class PricingService
{
    private readonly PricingRepository _pricingRepository;
    private readonly TableTypeRepository _tableTypeRepository;

    public PricingService()
    {
        _pricingRepository = new PricingRepository();
        _tableTypeRepository = new TableTypeRepository();
    }

    public PricingService(PricingRepository pricingRepository, TableTypeRepository tableTypeRepository)
    {
        _pricingRepository = pricingRepository;
        _tableTypeRepository = tableTypeRepository;
    }

    // Pricing Rule methods
    public List<HourlyPricingRule> GetAllPricingRules()
    {
        return _pricingRepository.GetAll();
    }

    public List<HourlyPricingRule> GetPricingRulesByTableType(int tableTypeId)
    {
        return _pricingRepository.GetRules(tableTypeId);
    }

    public HourlyPricingRule? GetPricingRuleById(int pricingRuleId)
    {
        return _pricingRepository.GetById(pricingRuleId);
    }

    public void AddPricingRule(HourlyPricingRule rule)
    {
        _pricingRepository.Add(rule);
    }

    public void UpdatePricingRule(HourlyPricingRule rule)
    {
        _pricingRepository.Update(rule);
    }

    public void DeletePricingRule(int pricingRuleId)
    {
        _pricingRepository.Delete(pricingRuleId);
    }

    // TableType methods (for dropdown)
    public List<TableType> GetAllTableTypes()
    {
        return _tableTypeRepository.GetAll();
    }
}

