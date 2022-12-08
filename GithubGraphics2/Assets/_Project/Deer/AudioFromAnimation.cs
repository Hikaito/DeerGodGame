using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioFromAnimation : StateMachineBehaviour
{
	public AudioClip startSound;
	public AudioClip loopSound;
	public AudioClip endSound;

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (animator.gameObject.GetComponent<AudioSource>().isPlaying)
		{
			animator.gameObject.GetComponent<AudioSource>().Stop();
		}
		if (startSound != null)
		{
			// if (!animator.gameObject.GetComponent<AudioSource>().isPlaying)
			// {
			animator.gameObject.GetComponent<AudioSource>().clip = startSound;
			animator.gameObject.GetComponent<AudioSource>().Play();
			// }
		}
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (loopSound != null)
		{
			if (!animator.gameObject.GetComponent<AudioSource>().isPlaying)
			{
				animator.gameObject.GetComponent<AudioSource>().clip = loopSound;
				animator.gameObject.GetComponent<AudioSource>().Play();
			}
		}
	}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (loopSound != null && animator.gameObject.GetComponent<AudioSource>().clip == loopSound)
		{
			animator.gameObject.GetComponent<AudioSource>().Stop();
		}
		if (endSound != null)
		{
			if (!animator.gameObject.GetComponent<AudioSource>().isPlaying)
			{
				animator.gameObject.GetComponent<AudioSource>().clip = endSound;
				animator.gameObject.GetComponent<AudioSource>().Play();
			}
		}
	}

	// OnStateMove is called right after Animator.OnAnimatorMove()
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	//{
	//    // Implement code that processes and affects root motion
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK()
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	//{
	//    // Implement code that sets up animation IK (inverse kinematics)
	//}
}
