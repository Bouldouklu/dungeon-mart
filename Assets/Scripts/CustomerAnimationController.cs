using UnityEngine;
using UnityEngine.AI;

namespace DungeonMart3D
{
    /// <summary>
    /// Controls customer animations based on NavMeshAgent movement.
    /// Attach this to the visual child GameObject (where the Animator is).
    /// Automatically finds the NavMeshAgent in the parent hierarchy.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class CustomerAnimationController : MonoBehaviour
    {
        private const string IS_WALKING_PARAM = "IsWalking";

        [Header("Movement Detection")]
        [SerializeField]
        [Tooltip("Velocity threshold to determine if customer is walking")]
        private float movementThreshold = 0.1f;

        private Animator animator;
        private NavMeshAgent navMeshAgent;
        private bool isWalking;

        private void Awake()
        {
            // Get Animator from this GameObject (visual child)
            animator = GetComponent<Animator>();

            // Find NavMeshAgent in parent hierarchy (on Customer GameObject)
            navMeshAgent = GetComponentInParent<NavMeshAgent>();

            // Validate components
            if (animator == null)
            {
                Debug.LogError($"CustomerAnimationController on {gameObject.name} requires an Animator component!");
            }

            if (navMeshAgent == null)
            {
                Debug.LogError($"CustomerAnimationController on {gameObject.name} could not find NavMeshAgent in parent hierarchy!");
            }
        }

        private void Update()
        {
            if (navMeshAgent == null || animator == null)
            {
                return;
            }

            // Determine if customer is moving based on NavMeshAgent velocity
            bool shouldBeWalking = navMeshAgent.velocity.magnitude > movementThreshold;

            // Only update animator parameter if state changed (optimization)
            if (shouldBeWalking != isWalking)
            {
                isWalking = shouldBeWalking;
                animator.SetBool(IS_WALKING_PARAM, isWalking);
            }
        }
    }
}
