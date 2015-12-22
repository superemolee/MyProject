using UnityEngine;
using System.Collections;

public class Locomotion
{
    private Animator m_Animator = null;
    
    private int m_SpeedId = 0;
    private int m_AgularSpeedId = 0;
    private int m_DirectionId = 0;
	
	public int m_IsPickUpId = 0;

    public float m_SpeedDampTime = 0.1f;
    public float m_AnguarSpeedDampTime = 0.25f;
    public float m_DirectionResponseTime = 0.2f;
	
	public bool m_IsPickUp = false;
	
    
    public Locomotion(Animator animator)
    {
        m_Animator = animator;

        m_SpeedId = Animator.StringToHash("Speed");
        m_AgularSpeedId = Animator.StringToHash("AngularSpeed");
        m_DirectionId = Animator.StringToHash("Direction");
		m_IsPickUpId = Animator.StringToHash("IsPickUp");
    }

    public void Do(float speed, float direction)
    {
        AnimatorStateInfo state = m_Animator.GetCurrentAnimatorStateInfo(0);

        bool inTransition = m_Animator.IsInTransition(0);
        bool inIdle = state.IsName("Locomotion.Idle");
        bool inTurn = state.IsName("Locomotion.TurnOnSpot") || state.IsName("Locomotion.PlantNTurnLeft") || state.IsName("Locomotion.PlantNTurnRight");
        bool inWalkRun = state.IsName("Locomotion.WalkRun");

        float speedDampTime = inIdle ? 0 : m_SpeedDampTime;
        float angularSpeedDampTime = inWalkRun || inTransition ? m_AnguarSpeedDampTime : 0;
        float directionDampTime = inTurn || inTransition ? 1000000 : 0;

        float angularSpeed = direction / m_DirectionResponseTime;
        
        m_Animator.SetFloat(m_SpeedId, speed, speedDampTime, Time.deltaTime);
		m_Animator.SetFloat(m_AgularSpeedId, angularSpeed, angularSpeedDampTime, Time.deltaTime);
        m_Animator.SetFloat(m_DirectionId, direction, directionDampTime, Time.deltaTime);
    }	
	
	// Lianchao Li: add animation function to play animations other than moves
	public void PickUp(bool isPickUp){
		AnimatorStateInfo state = m_Animator.GetCurrentAnimatorStateInfo(0);
		m_Animator.SetBool(m_IsPickUpId, isPickUp);
		
	}

}
