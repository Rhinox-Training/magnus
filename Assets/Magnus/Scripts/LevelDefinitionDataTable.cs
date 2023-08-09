#if USING_VORTEX
using Rhinox.Vortex;
using Rhinox.Vortex.File;

namespace Rhinox.Magnus
{
    [DataEndPoint(typeof(FileEndPoint))]
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