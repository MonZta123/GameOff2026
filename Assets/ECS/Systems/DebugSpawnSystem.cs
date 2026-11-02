using ECS.Entities;

namespace ECS.Systems
{
    public class DebugSpawnSystem
    {
        public void Execute(World world, Intent intent)
        {
            if (!intent.spawnPlayer)
                return;
            
            var snapshot= world.GetSnapshot();
            
            if (snapshot.numPlayers == snapshot.players.Length)
                return;
            
            var id = world.FetchPlayerId();
            
            var player = new Player(id); 
            
            world.SpawnPlayer(player);
        }
    }
}
