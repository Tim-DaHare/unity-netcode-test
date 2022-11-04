﻿using System.Linq;
using Enums;
using NetTypes;
using Unity.Netcode;
using Random = UnityEngine.Random;

namespace Behaviors
{
    public class LobbyManager : NetworkBehaviour
    {
        private NetworkList<NetPlayerLobbyData> _netLobbyPlayers;
        private readonly NetworkVariable<float> _matchStartedAt = new(-1);
        
        public NetworkList<NetPlayerLobbyData> LobbyPlayers => _netLobbyPlayers;
        public bool IsMatchStarted => _matchStartedAt.Value >= 0;
        public float MatchStartedAt => _matchStartedAt.Value;

        private void Awake()
        {
            _netLobbyPlayers = new NetworkList<NetPlayerLobbyData>();
        }

        public override void OnNetworkSpawn()
        {
            _netLobbyPlayers.OnListChanged += OnChangeConnectedPlayers;
            
            if (!IsServer) return; // Only the server can handle connects and disconnects

            NetworkManager.OnServerStarted += OnServerStarted;
            
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }

        public override void OnNetworkDespawn()
        {
            _netLobbyPlayers.OnListChanged -= OnChangeConnectedPlayers;
            
            if (!IsServer) return;
            
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }
        
        private void OnServerStarted()
        {
            _netLobbyPlayers.Add(new NetPlayerLobbyData(NetworkManager.LocalClientId));
            NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject().name = "Player " + NetworkManager.LocalClientId;
        }

        private void OnClientConnected(ulong connectedClientId)
        {
            _netLobbyPlayers.Add(new NetPlayerLobbyData(connectedClientId));
        }

        private void OnClientDisconnected(ulong disconnectedClientId)
        {
            _netLobbyPlayers.Remove(new NetPlayerLobbyData(disconnectedClientId));
        }

        private void OnChangeConnectedPlayers(NetworkListEvent<NetPlayerLobbyData> changeEvent)
        {
            if (!IsServer || changeEvent.Type != NetworkListEvent<NetPlayerLobbyData>.EventType.Add) return;
            
            // Handle renaming of player objects for editor hierarchy
            var connectedClientId = changeEvent.Value.ClientId;
            NetworkManager.SpawnManager.GetPlayerNetworkObject(connectedClientId).name = "Player " + connectedClientId;
        }

        public void StartMatch()
        {
            if (!IsServer || NetworkManager.ConnectedClients.Count == 0) return;
            
            _matchStartedAt.Value = NetworkManager.ServerTime.TimeAsFloat;
            AssignRolesToPlayers();
        }
        
        private void AssignRolesToPlayers()
        {
            var randKillerIndex = Random.Range(0, NetworkManager.ConnectedClients.Keys.Count());
            var killerClientId = NetworkManager.ConnectedClients.Keys.ElementAt(randKillerIndex);
            
            foreach (var connectedClient in NetworkManager.ConnectedClients)
            {
                var iPlayer = connectedClient.Value.PlayerObject.GetComponent<Player>();
                
                if (connectedClient.Key == killerClientId)
                {
                    iPlayer.SetPlayerRole(PlayerRoles.Killer);
                    continue;
                }
                
                iPlayer.SetPlayerRole(PlayerRoles.Civilian);
            }
        }
    }
}
