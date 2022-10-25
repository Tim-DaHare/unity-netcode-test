﻿using Enums;
using UnityEngine;

namespace Classes
{
    public class CivilianRole : PlayerRole
    {
        public override PlayerRoles Role => PlayerRoles.Civilian;
        public override PlayerTeams Team => PlayerTeams.Innocent;
        public override Color Color => Color.green;

        public override void UseAbility()
        {
            
        }
    }
}