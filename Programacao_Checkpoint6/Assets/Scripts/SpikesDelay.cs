using System.Collections;
using UnityEngine;

public class SpikesDelay : MonoBehaviour
{
    [SerializeField] private float waitTime;

    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        StartCoroutine(StartAnimation());
    }

    IEnumerator StartAnimation()
    {
        yield return new WaitForSeconds(waitTime);

        anim.SetBool("pStart" , true);
    }
}
