using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PortalControl : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private Transform _destination;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            Vector3 portalToPlayer = _player.position - this.transform.position;
            portalToPlayer.x = -portalToPlayer.x;

            _player.GetComponent<CharacterController>().enabled = false;
            _player.position = _destination.position - portalToPlayer;
            _player.rotation = _destination.rotation;
            _player.GetComponent<CharacterController>().enabled = true;
        }
    }
}
