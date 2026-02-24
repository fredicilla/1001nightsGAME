using BossFight;
using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("═══════════════════════════════════════");
        Debug.Log($"🎯 Goal triggered by: {other.name} (Tag: {other.tag})");
        Debug.Log("═══════════════════════════════════════");

        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("❌ GameManager not found!");
            return;
        }

        Debug.Log($"📊 Current Turn: {gameManager.currentTurn}, Turn Number: {gameManager.turnNumber}");

        // Check if key is required
        if (gameManager.RequiresKey && !gameManager.HasKey)
        {
            Debug.Log("🔒 Need key to reach goal!");
            return;
        }

        // Hero/Ghost reached goal
        if (other.CompareTag("Player"))
        {
            // Check if it's a Ghost
            GhostController ghostController = other.GetComponent<GhostController>();
            bool isGhost = ghostController != null;

            Debug.Log($"🔍 Is Ghost? {isGhost}");

            if (isGhost)
            {
                Debug.Log("👻 Ghost (Player tag) reached goal!");

                // In Turn 2 (Monster turn), if Ghost reaches goal = Monster FAILED!
                if (gameManager.currentTurn == TurnType.MonsterTurn)
                {
                    Debug.Log("❌ Ghost reached goal in Turn 2! Monster FAILED! Restarting Turn 2...");
                    gameManager.OnMonsterFailed();
                }
                // In Turn 5 (Second Monster turn), if Hero Ghost reaches goal = Monster2 FAILED!
                else if (gameManager.currentTurn == TurnType.SecondMonsterTurn)
                {
                    Debug.Log("❌ Hero Ghost reached goal in Turn 5! Monster2 FAILED! Restarting Turn 5...");
                    gameManager.OnSecondMonsterFailed();
                }
                else
                {
                    // Ghost shouldn't reach goal in other turns
                    Debug.LogWarning("⚠️ Ghost reached goal in unexpected turn!");
                }
            }
            else
            {
                // Real Player reached goal!
                Debug.Log("✅ Real Player reached goal! Calling OnGoalReached()...");
                gameManager.OnGoalReached();
            }
        }
        // Monster reached goal - should NOT happen! Monster's job is to kill Ghost, not reach goal
        else if (other.CompareTag("Monster"))
        {
            Debug.Log("⚠️ Monster reached goal - This should not happen! Monster must kill Ghost, not reach goal.");
        }
        else
        {
            Debug.LogWarning($"⚠️ Unknown tag '{other.tag}' reached goal!");
        }
    }
}
