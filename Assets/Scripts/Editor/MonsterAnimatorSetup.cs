using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class MonsterAnimatorSetup : MonoBehaviour
{
    [MenuItem("Tools/Setup Monster Animator")]
    public static void SetupAnimator()
    {
        AnimationClip idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/art 3d/Characters/AI alaa dev/Neutral Idle dev.fbx");
        AnimationClip walkClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/art 3d/Characters/AI alaa dev/Running dev.fbx");
        AnimationClip attackClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/art 3d/Characters/AI alaa dev/Throw dev.fbx");
        AnimationClip deathClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/art 3d/Characters/AI alaa dev/Writhing In Pain dev.fbx");
        
        if (idleClip == null || walkClip == null || attackClip == null || deathClip == null)
        {
            Debug.LogError("⚠️ لم يتم العثور على واحد أو أكثر من ملفات الأنيميشن!");
            return;
        }
        
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Animations/MonsterAnimator.controller");
        if (controller == null)
        {
            Debug.LogError("⚠️ لم يتم العثور على MonsterAnimator.controller!");
            return;
        }
        
        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        
        stateMachine.states = new ChildAnimatorState[0];
        stateMachine.anyStateTransitions = new AnimatorStateTransition[0];
        
        controller.parameters = new AnimatorControllerParameter[0];
        
        controller.AddParameter("isWalking", AnimatorControllerParameterType.Bool);
        controller.AddParameter("speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("die", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("isDead", AnimatorControllerParameterType.Bool);
        
        AnimatorState idleState = stateMachine.AddState("Idle", new Vector3(300, 100, 0));
        idleState.motion = idleClip;
        
        AnimatorState walkState = stateMachine.AddState("Walk", new Vector3(300, 200, 0));
        walkState.motion = walkClip;
        
        AnimatorState attackState = stateMachine.AddState("Attack", new Vector3(500, 150, 0));
        attackState.motion = attackClip;
        
        AnimatorState deathState = stateMachine.AddState("Death", new Vector3(300, 300, 0));
        deathState.motion = deathClip;
        
        stateMachine.defaultState = idleState;
        
        AnimatorStateTransition idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.hasExitTime = false;
        idleToWalk.duration = 0.2f;
        idleToWalk.AddCondition(AnimatorConditionMode.If, 0, "isWalking");
        
        AnimatorStateTransition walkToIdle = walkState.AddTransition(idleState);
        walkToIdle.hasExitTime = false;
        walkToIdle.duration = 0.2f;
        walkToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "isWalking");
        
        AnimatorStateTransition anyToAttack = stateMachine.AddAnyStateTransition(attackState);
        anyToAttack.hasExitTime = false;
        anyToAttack.duration = 0.1f;
        anyToAttack.AddCondition(AnimatorConditionMode.If, 0, "attack");
        
        AnimatorStateTransition attackToIdle = attackState.AddTransition(idleState);
        attackToIdle.hasExitTime = true;
        attackToIdle.exitTime = 0.9f;
        attackToIdle.duration = 0.2f;
        
        AnimatorStateTransition anyToDeath = stateMachine.AddAnyStateTransition(deathState);
        anyToDeath.hasExitTime = false;
        anyToDeath.duration = 0.1f;
        anyToDeath.AddCondition(AnimatorConditionMode.If, 0, "die");
        
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        
        Debug.Log("✅ تم إعداد Monster Animator بنجاح!");
    }
}
