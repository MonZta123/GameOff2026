using ECS.Entities;

namespace ECS
{
    public struct Snapshot
    {
        public short[] playerIdMap;
        public Player[] players;
        public short numPlayers;
        public short[] freePlayerIds;

        public short[] enemyIdMap;
        public Enemy[] enemies;
        public short numEnemies;
        public short[] freeEnemyIds;

        public short[] projectileIdMap;
        public Projectile[] projectiles;
        public short numProjectiles;
        public short[] freeProjectileIds;

        public void BuildFrom(Snapshot snapshot)
        {
            snapshot.playerIdMap.CopyTo(playerIdMap, 0);
            snapshot.players.CopyTo(players, 0);
            snapshot.freePlayerIds.CopyTo(freePlayerIds, 0);
            snapshot.enemyIdMap.CopyTo(enemyIdMap, 0);
            snapshot.enemies.CopyTo(enemies, 0);
            snapshot.freeEnemyIds.CopyTo(freeEnemyIds, 0);
            snapshot.projectileIdMap.CopyTo(projectileIdMap, 0);
            snapshot.projectiles.CopyTo(projectiles, 0);
            snapshot.freeProjectileIds.CopyTo(freeProjectileIds, 0);

            numPlayers = snapshot.numPlayers;
            numEnemies = snapshot.numEnemies;
            numProjectiles = snapshot.numProjectiles;
        }

        public void Initialize()
        {
            playerIdMap = new short[4];
            players = new Player[4];
            freePlayerIds = new short[4];

            enemyIdMap = new short[100];
            enemies = new Enemy[100];
            freeEnemyIds = new short[100];

            projectileIdMap = new short[500];
            projectiles = new Projectile[500];
            freeProjectileIds = new short[500];

            for (var i = 0; i < freePlayerIds.Length; i++)
            {
                freePlayerIds[i] = (short)(freePlayerIds.Length - i);
                playerIdMap[i] = -1;
            }

            for (var i = 0; i < freeEnemyIds.Length; i++)
            {
                freeEnemyIds[i] = (short)(freeEnemyIds.Length - i);
                enemyIdMap[i] = -1;
            }

            for (var i = 0; i < freeProjectileIds.Length; i++)
            {
                freeProjectileIds[i] = (short)(freeProjectileIds.Length - i);
                projectileIdMap[i] = -1;
            }
        }
    }
}
