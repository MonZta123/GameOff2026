using Actors;
using ECS;
using UnityEngine;

public class SnapshotRenderer : MonoBehaviour
{
    [SerializeField]
    private GameObjectPool pool;

    private readonly Player[] _playerMap = new Player[4];
    private readonly Enemy[] _enemyMap = new Enemy[100];
    private readonly Projectile[] _projectileMap = new Projectile[500];

    public void SetSnapshot(Snapshot snapshot)
    {
        UpdatePlayers(snapshot);
    }

    private void UpdatePlayers(Snapshot snapshot)
    {
        for (var i = 0; i < snapshot.numPlayers; i++)
        {
            var currentPlayer = snapshot.players[i];

            var obj = _playerMap[currentPlayer._id - 1];
            if (!obj)
            {
                obj = pool.GetPlayer(currentPlayer);
                _playerMap[currentPlayer._id - 1] = obj;
            }

            obj.SetState(currentPlayer);
        }

        for (var i = 0; i < _playerMap.Length; i++)
        {
            if (!_playerMap[i])
                continue;

            if (snapshot.playerIdMap[i] == -1)
            {
                pool.ReturnPlayer(_playerMap[i]);
                _playerMap[i] = null;
            }
        }
    }
}
