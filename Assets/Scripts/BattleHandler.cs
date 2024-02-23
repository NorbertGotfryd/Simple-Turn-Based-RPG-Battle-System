using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//klasa zarzadzajaca walka
public class BattleHandler : MonoBehaviour
{
    public static BattleHandler instance;

    [SerializeField] private Transform playerPrefab;
    [SerializeField] private Transform enemyPrefab;
    [SerializeField] private Vector3 playerPos;
    [SerializeField] private Vector3 enemyPos;

    private CharacterBase playerCharacterBattle;
    private CharacterBase enemyCharacterBattle;
    private CharacterBase activeCharacterBattle;
    private BattleState state;

    //status walki
    private enum BattleState
    {
        WaitingForPlayer,
        Busy,
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        playerCharacterBattle = SpawnCharacter(true);
        enemyCharacterBattle = SpawnCharacter(false);

        SetActiveCharacterBattle(playerCharacterBattle);
        state = BattleState.WaitingForPlayer;
    }

    private void Update()
    {
        //testowa funkcja, przy nacisnieciu spacji postac gracza atakuje
        if(state == BattleState.WaitingForPlayer)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                state = BattleState.Busy;
                playerCharacterBattle.CharacterAttack(enemyCharacterBattle, () =>
                {
                    ChooseActiveCharacterBattle();
                });
            }
        }
    }

    //spawn postaci, do przerobienia zeby lepiej rozpoznawalo kogo ma spawnowac i gdzie
    private CharacterBase SpawnCharacter(bool isPlayer)
    {
        Vector3 position;
        Transform character;
        if (isPlayer)
        {
            position = playerPos;
            character = playerPrefab;
        }
        else
        {
            position = enemyPos;
            character = enemyPrefab;
        }

        Transform characterTransform = Instantiate(character, position, Quaternion.identity);
        CharacterBase characterBattle = characterTransform.GetComponent<CharacterBase>();

        return characterBattle;
    }

    //logika znacznika aktywnej postaci
    private void SetActiveCharacterBattle(CharacterBase characterBattle)
    {
        if (activeCharacterBattle != null)
            activeCharacterBattle.HideSelection();

        activeCharacterBattle = characterBattle;
        activeCharacterBattle.ShowSelection();
    }

    //ustawienie aktywnej postaci, na razie jest to mega prymitywne bo albo rusza sie gracz albo przeciwnik
    //ale do tego potrzebujemy konkretnych informacji o mechanice walki
    private void ChooseActiveCharacterBattle()
    {
        if (TestBattleOver())
            return;

        if (activeCharacterBattle == playerCharacterBattle)
        {
            SetActiveCharacterBattle(enemyCharacterBattle);
            state = BattleState.Busy;

            enemyCharacterBattle.CharacterAttack(playerCharacterBattle, () =>
            {
                ChooseActiveCharacterBattle();
            });
        }
        else
        {
            SetActiveCharacterBattle(playerCharacterBattle);
            state = BattleState.WaitingForPlayer;
        }
    }

    //testowanie kto wygral
    private bool TestBattleOver() 
    {
        if (playerCharacterBattle.CharacterIsDead())
        {
            //enemy win
            Debug.Log("Enemy win");
            return true;
        }
        if (enemyCharacterBattle.CharacterIsDead())
        {
            //player win
            Debug.Log("Player win");
            return true;
        }

        return false;
    }
}
