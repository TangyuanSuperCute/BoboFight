using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattleScene
{
    [Serializable]
    public class BattleInfoModel
    {
        public string name;
        public Sprite avatar;

        public int hpMax;
        public int hpMin;
        public int hp;

        public int spiritMax;
        public int spiritCount;
        
        public int dodgeMax;
        public int dodgeCount;

        public List<SkillModel> skills = new();
        public List<BuffModel> buffs = new();
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