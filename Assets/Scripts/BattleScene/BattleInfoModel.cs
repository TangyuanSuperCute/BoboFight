using System;
using System.Collections.Generic;

namespace BattleScene
{
    [Serializable]
    public class BattleInfoModel
    {
        public int hpMax;
        public int hpMin;
        public int hp;
        
        public int spiritCount;
        public int dodgeCount;

        public List<SkillModel> skills;
        
        public List<BuffModel> buffs;
    }

    [Serializable]
    public class SkillModel
    {
        public string name;
        public string description;
        public int cast;
        public SkillType type;
    }

    [Serializable]
    public class BuffModel
    {
        public string name;
        public string description;
        public int stack;
    }
}