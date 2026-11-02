namespace ECS.Systems
{
    public class DebugDespawnSystem
    {
        public void Execute(World world, Intent intent)
        {
            if (intent.despawnPlayer1)
                world.DespawnPlayer(1);

            if (intent.despawnPlayer2)
                world.DespawnPlayer(2);

            if (intent.despawnPlayer3)
                world.DespawnPlayer(3);

            if (intent.despawnPlayer4)
                world.DespawnPlayer(4);
        }
    }
}
