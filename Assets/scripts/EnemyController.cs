using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private Transform player; //punto en el espacio

    private NavMeshAgent _agent; //guarda la referencia  a la componente nav de mi agente (configurar el destino)

    private float visionRange = 20f;
    private float attackRange = 10f;

    private bool playerInVisionRange;
    private bool playerInAttackRange;

    [SerializeField] private LayerMask playerLayer;

    //Patrulla
    [SerializeField] private Transform[] waypoints; //puntos de patrulla
    private int totalWaypoints;
    private int nextPoint;

    //Attack
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform spawnPoint;
    private float timeBetweenAttacks = 2f;
    private bool canAttack;
    private float upAttackForce = 15f;
    private float forwardAttackForce = 18f;


    private void Awake() //tener acceso a la componente nav
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        totalWaypoints = waypoints.Length; //saber cuantos puntos tengo para saber cuando volver a empezar
        nextPoint = 1;
        canAttack = true;
    }


    private void Update() 
    {
        Vector3 pos = transform.position; //posición del enemigo
        playerInVisionRange = Physics.CheckSphere(pos, visionRange, playerLayer); //esfera que detecta a la capa player
        playerInAttackRange = Physics.CheckSphere(pos, attackRange, playerLayer);

        if(!playerInVisionRange && !playerInAttackRange)
        {
            Patrol();
        }

        if(playerInVisionRange && !playerInAttackRange)
        {
            Chase();
        }

        if(playerInVisionRange && playerInAttackRange)
        {
            Attack();
        }
    }

    private void Patrol()
    {
        if (Vector3.Distance(transform.position, waypoints[nextPoint].position) < 2.5f)
        {
            nextPoint++;
            if(nextPoint == totalWaypoints)
            {
                nextPoint = 0;
            }
            transform.LookAt(waypoints[nextPoint].position);
        }
        _agent.SetDestination(waypoints[nextPoint].position);
    }

    private void Chase()
    {
        _agent.SetDestination(player.position);
        transform.LookAt(player);
    }

    private void Attack()
    {
        _agent.SetDestination(transform.position);
        if (canAttack)
        {
            Rigidbody rigidbody = Instantiate(bullet, transform.position, Quaternion.identity).GetComponent<Rigidbody>();
            rigidbody.AddForce(transform.forward * forwardAttackForce, ForceMode.Impulse);
            rigidbody.AddForce(transform.up * upAttackForce, ForceMode.Impulse);

            canAttack = false;
            StartCoroutine(AttackCooldown());
        }
    }

    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(timeBetweenAttacks);
        canAttack = true;
    }

    private void OnDrawGizmos()
    {
        //esfera de visión
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        //esfera de ataque
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
