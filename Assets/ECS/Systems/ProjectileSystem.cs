using System;
using Domain;
using UnityEngine;

namespace ECS.Systems
{
    public class ProjectileSystem
    {
        public ProjectileSystem(int tickRate)
        {
            _heavyTtl = Mathf.CeilToInt(Constants.HeavyProjectileTtl * tickRate);
        }
        
        private float _heavyTtl;

        public void Execute(World world, ReadOnlySpan<Intent> intents, int tick)
        {
            
        }
    }
}
