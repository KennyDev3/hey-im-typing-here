using System.Collections.Generic;
using UnityEngine;

public class DynamicTrail : MonoBehaviour
{
    public float lifetime = 5f; // Lifetime of a point on the trail
    public float minimumVertexDistance = 0.1f; // Minimum distance before adding a new point
    public Vector3 velocity = new Vector3(0, -1, -1); // Direction and speed of the trail movement

    private LineRenderer line;
    private List<Vector3> points; // Stores the trail points
    private Queue<float> spawnTimes = new Queue<float>(); // Tracks when each point was added

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        points = new List<Vector3>() { transform.position }; // Start with one point at the player's position
        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
    }

    void AddPoint(Vector3 position)
    {
        points.Insert(1, position); // Insert new point at index 1 (right after the player's position)
        spawnTimes.Enqueue(Time.time); // Record the spawn time
    }

    void RemovePoint()
    {
        spawnTimes.Dequeue(); // Remove the oldest spawn time
        points.RemoveAt(points.Count - 1); // Remove the oldest point
    }

    void Update()
    {
        // Remove points that have exceeded their lifetime
        while (spawnTimes.Count > 0 && spawnTimes.Peek() + lifetime < Time.time)
        {
            RemovePoint();
        }

        // Move all points (except the first one, which is the player's position)
        Vector3 diff = velocity * Time.deltaTime;
        for (int i = 1; i < points.Count; i++)
        {
            points[i] += diff;
        }

        // Add a new point if the player has moved enough
        if (points.Count < 2 || Vector3.Distance(transform.position, points[1]) > minimumVertexDistance)
        {
            AddPoint(transform.position);
        }

        // Update the first point to the player's current position
        points[0] = transform.position;

        // Update the Line Renderer
        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
    }
}