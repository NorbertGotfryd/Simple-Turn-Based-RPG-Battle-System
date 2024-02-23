using System;
using UnityEngine;

//klasa bazowa wspolnych funkcji dla wszystkich postaci
//nie przypisywac bezposrednio do obiektow
//na pewno cos z tej klasy trzeba bedzie przeniesc do CharacterPlayerBase lub CharacterEnemyBase ale 
public abstract class CharacterBase : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] protected int healhAmountMax;
    [SerializeField] protected int attackAmount;
    [SerializeField] protected int defAmount;

    protected int healhAmountCurrent;

    protected Vector3 targetPosition;
    protected CharacterState state;

    protected GameObject selectionVisual;

    protected Action onAttackComplete;

    public event EventHandler OnHealthChange;
    public event EventHandler OnDeath;

    //status postaci w czasie walki
    protected enum CharacterState
    {
        Idle,
        Attack,
        Busy,
    }

    private void Awake()
    {
        healhAmountCurrent = healhAmountMax;

        selectionVisual = transform.Find("SelectionVisual").gameObject;
        state = CharacterState.Idle;
        HideSelection();
    }

    private void Update()
    {
        CharacterBattleState();
    }

    //ustawienie statusu postaci, trzeba to troche przerobic
    private void CharacterBattleState()
    {
        switch (state)
        {
            case CharacterState.Idle:
                break;
            case CharacterState.Attack:
                float speed = 10f;
                transform.position += (targetPosition - GetCharacterPosition()) * speed * Time.deltaTime;

                float reachedDistance = 1f;
                if (Vector3.Distance(GetCharacterPosition(), targetPosition) < reachedDistance)
                {
                    transform.position = targetPosition;
                    onAttackComplete();
                }
                break;
            case CharacterState.Busy:
                break;
        }
    }

    //logika ataku krok po kroku, to tutaj mozna wrzucic animacje
    public void CharacterAttack(CharacterBase targetCharacterBattle, Action onAttackCompleted)
    {
        float speed = 1f;
        Vector3 attackTargetPos = 
            targetCharacterBattle.GetCharacterPosition() + (GetCharacterPosition() - targetCharacterBattle.GetCharacterPosition()).normalized * speed;
        Vector3 startingPosition = GetCharacterPosition();
        
        //"skok" do przeciwnika
        AttackToPosition(attackTargetPos, () =>
        {
            //dotarcie do celu i zaatakowanie go
            state = CharacterState.Busy;
                //atak zakonczony i powrot
                AttackToPosition(startingPosition, () => 
                {
                    targetCharacterBattle.HealthDamageCalculation();
                    state = CharacterState.Idle;
                    onAttackCompleted();
                });
        });
    }

    //zaatakowanie konkretnej pozycji przeciwnika
    private void AttackToPosition(Vector3 targetPos, Action onAttackComplete)
    {
        this.targetPosition = targetPos;
        this.onAttackComplete = onAttackComplete;
        state = CharacterState.Attack;
    }

    //obliczanie obrazen, do duzej zmiany to bylo do testow tak zrobione
    private int DamageDealth(int attAmount, int protAmount)
    {
        int damageDeal = UnityEngine.Random.Range(attAmount, attAmount) - (protAmount - 1);

        return damageDeal;
    }

    //obliczanie zycia postaci
    public void HealthDamageCalculation()
    {
        healhAmountCurrent -= DamageDealth(attackAmount, defAmount);
        if (healhAmountCurrent < 0)
            healhAmountCurrent = 0;

        if (OnHealthChange != null)
            OnHealthChange(this, EventArgs.Empty);

        if (healhAmountCurrent <= 0)
            CharacterDie();

        Debug.Log("Hit " + healhAmountCurrent + gameObject.name);
    }

    //smierc postaci
    public void CharacterDie()
    {
        if (OnDeath != null)
            OnDeath(this, EventArgs.Empty);
    }

    //ukrycie znacznika aktywnej postaci
    public void ShowSelection()
    {
        selectionVisual.SetActive(true);
    }

    //pokazanie znacznika aktywnej postaci
    public void HideSelection()
    {
        selectionVisual.SetActive(false);
    }

    //sprawdzenie czy postac jest martwa
    public bool CharacterIsDead() => healhAmountCurrent <= 0;

    //funkcja do zwrocenia pozycji postaci
    public Vector3 GetCharacterPosition() => transform.position;
}
