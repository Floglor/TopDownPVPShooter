using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Metagame
{
    public class RoundController : NetworkBehaviour
    {
        public static RoundController Instance;
        [SerializeField]
        private Transform _defaultPositionPlayerOne;

        [SerializeField]
        private Transform _defaultPositionPlayerTwo;

        [SerializeField]
        private BaseSquareController[] Players;

        [SerializeField] private TextMeshProUGUI _1PScore;
        [SerializeField] private TextMeshProUGUI _2PScore;


        private NetworkVariable<int> scoreOne = new NetworkVariable<int>();
        private NetworkVariable<int> scoreTwo = new NetworkVariable<int>();

        private void OnEnable()
        {
            Players = new BaseSquareController[2];
            Instance = this;
            _1PScore.text = "0";
            _2PScore.text = "0";
            
            scoreOne.OnValueChanged += UpdateScoreOneClientRpc;
            scoreTwo.OnValueChanged += UpdateScoreTwoClientRpc;


        }

        [ClientRpc]
        private void UpdateScoreOneClientRpc(int previousvalue, int newvalue)
        {
            _1PScore.text = newvalue.ToString();
        }
        
        [ClientRpc]
        private void UpdateScoreTwoClientRpc(int previousvalue, int newvalue)
        {
            _2PScore.text = newvalue.ToString();
        }

        [ClientRpc]
        private void SetScoreClientRpc()
        {
            Debug.Log($"SetScoreClientRpc, One: {scoreOne.Value}, Two: {scoreTwo.Value}");
            _2PScore.text = scoreTwo.Value.ToString();
            _1PScore.text = scoreOne.Value.ToString();
        }

        public void LoseRound(BaseSquareController player)
        {
            if (FindWhatPlayer(player) == 0)
            {
                scoreTwo.Value++;
                _2PScore.text = scoreTwo.Value.ToString();

                ResetRound();
                Debug.Log("Player one won the round");
            }
            else
            {
                scoreOne.Value++;
                _1PScore.text = scoreOne.Value.ToString();

                ResetRound();
                Debug.Log("Player two won the round");
            }
        }

        IEnumerator SetScore()
        {
            yield return 20;
            SetScoreClientRpc();
        }

        private void ResetRound()
        {
            SetPosTo(_defaultPositionPlayerTwo, Players[1]);
            SetPosTo(_defaultPositionPlayerOne, Players[0]);

            foreach (BaseSquareController baseSquareController in Players)
            {
                baseSquareController.ResetState();
            }
            
            StartGame();
        }

        public void AddPlayer(BaseSquareController player)
        {
            if (!IsServer) return;

            player.StopMovementAndShooting();
            
            if (Players[0] != null)
            {
                Players[1] = player;
                SetPosTo(_defaultPositionPlayerTwo, Players[1]);
                StartGame();
            }
            else
            {
                Players[0] = player;
                SetPosTo(_defaultPositionPlayerOne, Players[0]);
            }
        }

        private void StartGame()
        {
            if (!IsServer) return;
            
            foreach (BaseSquareController baseSquareController in Players)
            {
                baseSquareController.ReleaseMovementAndShooting();
            }
        }

        private void SetPosTo(Transform position, BaseSquareController player)
        {
            player.transform.position = position.position;
            player.SetPositionClientRpc(position.position);
        }

        private int FindWhatPlayer(BaseSquareController player)
        {
            return Players[0] == player ? 0 : 1;
        }
    }
}