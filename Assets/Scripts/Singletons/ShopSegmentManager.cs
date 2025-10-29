using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Defines a shop segment that can be unlocked through upgrades.
/// Each segment contains shelf slots and contributes to rent.
/// </summary>
[System.Serializable]
public class ShopSegment
{
    [Tooltip("Display name for this segment")]
    public string segmentName;

    [Tooltip("Root GameObject containing all segment elements (shelves, walls, etc.)")]
    public GameObject segmentRoot;

    [Tooltip("Transform array for shelf positions in this segment")]
    public Transform[] shelfSlots;

    [Tooltip("Cost to unlock this segment")]
    public int unlockCost;

    [Tooltip("Is this segment currently unlocked?")]
    public bool isUnlocked = false;
}

/// <summary>
/// Manages shop expansion segments for progression.
/// Handles unlocking new shop areas as player purchases expansions.
/// </summary>
public class ShopSegmentManager : MonoBehaviour
{
    public static ShopSegmentManager Instance;

    [Header("Shop Segments")]
    [Tooltip("Array of shop segments (Segment 0 should be unlocked by default)")]
    [SerializeField] private ShopSegment[] segments;

    [Header("Rent Contribution")]
    [Tooltip("Additional rent cost per unlocked segment (after first)")]
    [SerializeField] private int rentPerSegment = 100;

    /// <summary>
    /// Event triggered when a segment is unlocked.
    /// Parameters: segment index, segment name
    /// </summary>
    public event System.Action<int, string> OnSegmentUnlocked;

    // Properties
    public int TotalSegments => segments?.Length ?? 0;
    public int UnlockedSegmentCount => segments?.Count(s => s.isUnlocked) ?? 0;
    public int RentPerSegment => rentPerSegment;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // Validate segments array
        if (segments == null || segments.Length == 0)
        {
            Debug.LogError("ShopSegmentManager: No segments assigned!");
            return;
        }

        // Ensure first segment (starter area) is always unlocked
        if (!segments[0].isUnlocked)
        {
            UnlockSegment(0);
        }

        // Disable all other segments by default
        for (int i = 1; i < segments.Length; i++)
        {
            if (!segments[i].isUnlocked && segments[i].segmentRoot != null)
            {
                segments[i].segmentRoot.SetActive(false);
            }
        }

        Debug.Log($"ShopSegmentManager initialized: {UnlockedSegmentCount}/{TotalSegments} segments unlocked");
    }

    /// <summary>
    /// Unlocks a specific segment by index.
    /// </summary>
    public bool UnlockSegment(int segmentIndex)
    {
        // Validate index
        if (segmentIndex < 0 || segmentIndex >= segments.Length)
        {
            Debug.LogError($"Invalid segment index: {segmentIndex}");
            return false;
        }

        ShopSegment segment = segments[segmentIndex];

        // Check if already unlocked
        if (segment.isUnlocked)
        {
            Debug.LogWarning($"Segment {segmentIndex} ({segment.segmentName}) is already unlocked");
            return false;
        }

        // Unlock segment
        segment.isUnlocked = true;

        // Enable segment GameObject
        if (segment.segmentRoot != null)
        {
            segment.segmentRoot.SetActive(true);
            Debug.Log($"✅ Unlocked segment {segmentIndex}: {segment.segmentName}");
        }
        else
        {
            Debug.LogWarning($"Segment {segmentIndex} has no root GameObject assigned!");
        }

        // Fire event
        OnSegmentUnlocked?.Invoke(segmentIndex, segment.segmentName);

        return true;
    }

    /// <summary>
    /// Checks if a specific segment is unlocked.
    /// </summary>
    public bool IsSegmentUnlocked(int segmentIndex)
    {
        if (segmentIndex < 0 || segmentIndex >= segments.Length) return false;
        return segments[segmentIndex].isUnlocked;
    }

    /// <summary>
    /// Gets all shelf slot transforms from unlocked segments.
    /// </summary>
    public List<Transform> GetUnlockedShelfSlots()
    {
        List<Transform> slots = new List<Transform>();

        foreach (ShopSegment segment in segments)
        {
            if (segment.isUnlocked && segment.shelfSlots != null)
            {
                slots.AddRange(segment.shelfSlots.Where(t => t != null));
            }
        }

        return slots;
    }

    /// <summary>
    /// Gets segment data by index.
    /// </summary>
    public ShopSegment GetSegment(int index)
    {
        if (index < 0 || index >= segments.Length) return null;
        return segments[index];
    }

    /// <summary>
    /// Calculates total rent contribution from unlocked segments.
    /// First segment is free, each additional segment adds rentPerSegment.
    /// </summary>
    public int GetRentContribution()
    {
        int unlockedCount = UnlockedSegmentCount;
        if (unlockedCount <= 1) return 0; // First segment is free

        return (unlockedCount - 1) * rentPerSegment;
    }

    /// <summary>
    /// Gets list of all segment names and their unlock status (for debugging).
    /// </summary>
    public string GetSegmentStatusDebug()
    {
        string status = "Shop Segments:\n";
        for (int i = 0; i < segments.Length; i++)
        {
            ShopSegment segment = segments[i];
            string lockStatus = segment.isUnlocked ? "✅ UNLOCKED" : "🔒 LOCKED";
            status += $"  [{i}] {segment.segmentName}: {lockStatus}\n";
        }
        return status;
    }
}
