namespace Domain
{
    public static class Constants
    {
        public const float RewindDuration = 1.5f;

        public const int PlayerCount = 4;
        public const int EnemyCount = 100;
        public const int ProjectileCount = 200;
        public const int DoorCount = 20;
        public const int TriggerCount = 50;
        public const int EnemyDiedNotificationCount = 10;
        public const int SnipeShotNotificationCount = 5;
        public const int DoorDestructionNotificationCount = 10;
        public const int PlacedBombCount = 10;
        public const int SplosionNotificationCount = 10;

        public const int PlayerHealth = 150;
        public const int GhostHealth = 300;

        public const float HeavyDamageConeAngle = 45.0f;
        public const float HeavyDamageConeRadius = 3.0f;

        public const float HeavyMoveSpeed = 7.5f;
        public const float HeavyColliderRadius = 0.5f;
        public const int HeavyDamage = 60;
        public const float HeavyBlastRadius = 2f;
        public const int HeavyBlastDamage = 100;

        public const float HackerProjectileSpeed = 50f;
        public const float HackerMoveSpeed = 7.5f;
        public const float HackerColliderRadius = 1.0f;
        public const int HackerDamage = 30;

        public const float DemolitionCooldown = 0.75f;
        public const float DemolitionProjectileTtl = 1f;
        public const float DemolitionProjectileSpeed = 5f;
        public const float DemolitionMoveSpeed = 7.5f;
        public const float DemolitionColliderRadius = 1.0f;
        public const int DemolitionDamage = 100;
        public const float DemolitionBlastRadius = 3f;
        public const float DemolitionMineReach = 1f;

        public const float SniperCooldown = 1.5f;
        public const float SniperMoveSpeed = 7.5f;
        public const float SniperColliderRadius = 1.0f;
        public const float SniperRecoilForce = 16.0f;

        public const float RedHairGuyCooldown = 3.0f;
        public const float RedHairGuyProjectileTtl = 5.0f;
        public const float RedHairGuyProjectileSpeed = 5.0f;
        public const float RedHairGuyColliderRadius = 0.5f;
        public const float RedHairGuyMoveSpeed = 2.0f;
        public const int RedHairGuyDamage = 10;

        public const float ChickCooldown = 3.0f;
        public const float ChickColliderRadius = 0.5f;
        public const float ChickMoveSpeed = 2.0f;
        public const int ChickDamage = 20;

        public const float ChickRange = 1f;
        public const float ChickSpawnDistanceAverageUnits = 7f;

        public const float WtfHairMoveSpeed = 2.0f;
        public const float ButtlerMoveSpeed = 0.0f;

        public const float ButtlerLockOnSeconds = 3.0f;
        public const float ButtlerCooldown = 1.0f;
        public const float ButtlerColliderRadius = 0.5f;
        public const int ButtlerMaxViewDistanceUnits = 15;
        public const int ButtlerRespawnDistanceUnits = 20;
        public const float ButtlerRespawnCooldownSeconds = 3.0f;
        public const int ButtlerRespawnMinDistanceUnits = 10;
        public const int EnemyHealth = 100;
    }
}
