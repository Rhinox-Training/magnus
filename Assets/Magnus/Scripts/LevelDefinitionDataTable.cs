#if USING_VORTEX
using Rhinox.Vortex;
using Rhinox.Vortex.File;

namespace Rhinox.Magnus
{
#if !VORTEX_0_5_0
    [DataEndPoint(typeof(FileEndPoint))]
#endif
    public class LevelDefinitionDataTable : DataTable<LevelDefinitionData>
    {
        protected override string _tableName => "level-definitions";

        protected override int GetID(LevelDefinitionData o)
        {
            return o.ID;
        }

        protected override LevelDefinitionData SetID(LevelDefinitionData dto, int id)
        {
            dto.ID = id;
            return dto;
        }
    }
}
#endif