using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Bot : MonoBehaviour
{
    public NavMeshAgent navAgent;
    
    [Header("Boundaries")] [SerializeField] private Vector3 bounds = Vector3.one;

    //[SerializeField] private Vector3 boundsOffset = Vector3.zero;
    
    //[SerializeField] private float boundsMovementMax = 2f;
    //private float boundsMovementMultiplier = 0;

    
    private void OnDrawGizmos()
    {
        //Vector3 boundsOffset = this.boundsOffset + (new Vector3(0, 0, boundsMovementMax));
        Gizmos.DrawWireCube(Vector3.zero, bounds * 2);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        navAgent.SetDestination(Vector3.zero);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.R))
        {
            
            float x = Random.Range(-bounds.x, bounds.x);
            float z = Random.Range(-bounds.z, bounds.z);
            Vector3 destination = new Vector3(x, transform.position.y, z);
            Debug.Log("Destiny: " + destination);
            navAgent.SetDestination(destination);
        }
    }
}
