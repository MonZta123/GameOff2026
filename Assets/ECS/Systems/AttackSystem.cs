using System;
using Domain;
using ECS.Entities;
using UnityEngine;

namespace ECS.Systems
{
    public class AttackSystem
    {
        public AttackSystem(int tickRate)
        {
            _heavyCooldown = Mathf.CeilToInt(Constants.HeavyCooldown * tickRate);
            _heavyTtl = Mathf.CeilToInt(Constants.HeavyProjectileTtl * tickRate);

            _hackerCooldown = Mathf.CeilToInt(Constants.HackerCooldown * tickRate);
            _hackerTtl = Mathf.CeilToInt(Constants.HackerProjectileTtl * tickRate);

            _deltaTime = 1f / tickRate;
        }

        private readonly float _deltaTime;

        private readonly float _heavyCooldown;
        private readonly float _heavyTtl;

        private readonly float _hackerCooldown;
        private readonly float _hackerTtl;

        public void Execute(World world, int currentTick)
        {
            ref var snapshot = ref world.GetSnapshot();

            // for (var i = 0; i < snapshot._players.GetNumEntities(); i++)
            // {
            //     var moveEvent = world._moveEvents[i];
            //     if (!moveEvent._primary)
            //         continue;
            //
            //     ref var player = ref snapshot._players.GetEntityAt(i);
            //
            //     switch (player._type)
            //     {
            //         case PlayerType.Heavy:
            //             HandleHeavyPrimary(world, currentTick, ref player);
            //             break;
            //         case PlayerType.Hacker:
            //             HandleHackerPrimary(world, currentTick, ref player);
            //             break;
            //         case PlayerType.Demolition:
            //             break;
            //         case PlayerType.Stealth:
            //             break;
            //         default:
            //             throw new ArgumentOutOfRangeException();
            //     }
            // }
            //
            // for (var i = 0; i < snapshot._players.GetNumEntities(); i++)
            // {
            //     var moveEvent = world._moveEvents[i];
            //     if (!moveEvent._secondary)
            //         continue;
            //
            //     ref var player = ref snapshot._players.GetEntityAt(i);
            //
            //     if (player._cooldownSecondary <= 0)
            //     {
            //         switch (player._type)
            //         {
            //             case PlayerType.Heavy:
            //                 ActivateHeavySecondary(ref player);
            //                 Debug.Log("ActivateHeavySecondary");
            //                 break;
            //             case PlayerType.Hacker:
            //                 // HandleHackerSecondary(world, intent, currentTick, ref player);
            //                 break;
            //             case PlayerType.Demolition:
            //                 break;
            //             case PlayerType.Stealth:
            //                 break;
            //             default:
            //                 throw new ArgumentOutOfRangeException();
            //         }
            //     }
            //
            //     if (player._secondaryActive > 0)
            //     {
            //         switch (player._type)
            //         {
            //             case PlayerType.Heavy:
            //                 ActivateHeavySecondary(ref player);
            //                 break;
            //             case PlayerType.Hacker:
            //                 // HandleHackerSecondary(world, intent, currentTick, ref player);
            //                 break;
            //             case PlayerType.Demolition:
            //                 break;
            //             case PlayerType.Stealth:
            //                 break;
            //             default:
            //                 throw new ArgumentOutOfRangeException();
            //         }
            //     }
            // }
        }

        // private void HandleHeavySecondaryDone(World world, int currentTick, ref Player player)
        // {
        //     player._blockMovementSeconds = 6;
        // }
        //
        // private void ActivateHeavySecondary(ref Player player)
        // {
        //     player._cooldownSecondary = 3f;
        //     player._secondaryActive = 0.5f;
        // }

        // private void HandleHeavyPrimary(World world, int currentTick, ref Player player)
        // {
        //     if (currentTick < player._cooldownPrimary + _heavyCooldown)
        //         return;
        //
        //     var projectile = new Projectile()
        //     {
        //         _position = player._position,
        //         _angle = player._angle,
        //         _spawnTick = currentTick,
        //         _ttl = _heavyTtl,
        //         _type = ProjectileType.Heavy
        //     };
        //
        //     world.GetSnapshot()._projectiles.Spawn(projectile);
        //
        //     player._cooldownPrimary = currentTick;
        // }
        //
        // private void HandleHackerPrimary(World world, int currentTick, ref Player player)
        // {
        //     if (currentTick < player._cooldownPrimary + _hackerCooldown)
        //         return;
        //
        //     var projectile = new Projectile()
        //     {
        //         _position = player._position,
        //         _angle = player._angle,
        //         _spawnTick = currentTick,
        //         _ttl = _hackerTtl,
        //         _type = ProjectileType.Hacker
        //     };
        //
        //     world.GetSnapshot()._projectiles.Spawn(projectile);
        //
        //     player._cooldownPrimary = currentTick;
        // }
    }
}
