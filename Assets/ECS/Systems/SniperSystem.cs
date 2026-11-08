using System;
using Domain;
using ECS.Entities;
using ECS.Events;
using UnityEngine;

namespace ECS.Systems
{
    public class SniperSystem
    {
        public SniperSystem(int tickRate)
        {
            _tickRate = tickRate;

            _shootCooldownTicks = (int)MathF.Floor(tickRate * _secondsPrimaryCooldown);
            
            _speed = Constants.SniperMoveSpeed;
        }

        private float _speed;
        
        private int _tickRate;

        private float _secondsPrimaryCooldown = 0.25f;
        private int _shootCooldownTicks;

        public void Execute(World world, int currentTick)
        {
            var moveEvt = world._moveEvents[3];
            ref var snapshot = ref world.GetSnapshot();
            ref var player = ref snapshot._players.GetEntityAt(3);

            if (player._primaryReadyAt <= currentTick)
            {
                if (moveEvt._primary)
                {
                    // Do Hitscan Shit
                    
                    player._primaryReadyAt = currentTick + _shootCooldownTicks;

                    
                }
            }

            player._velocity = moveEvt._movement * _speed;
            player._angle = moveEvt._angle;
        }
    }
}
