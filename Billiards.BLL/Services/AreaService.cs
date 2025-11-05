using Billiards.DAL.Models;
using Billiards.DAL.Repositories;

namespace Billiards.BLL.Services;

public class AreaService
{
    private readonly AreaRepository _areaRepository;

    public AreaService()
    {
        _areaRepository = new AreaRepository();
    }

    public AreaService(AreaRepository areaRepository)
    {
        _areaRepository = areaRepository;
    }

    public List<Area> GetAllAreas()
    {
        return _areaRepository.GetAllAreas();
    }
}

