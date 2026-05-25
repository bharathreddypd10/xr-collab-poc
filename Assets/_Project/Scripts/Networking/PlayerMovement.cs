using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector2 Move;
}

public class PlayerMovement : NetworkBehaviour
{
    public float speed = 5f;

    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out NetworkInputData input))
            return;

        Vector3 move = new Vector3(input.Move.x, 0f, input.Move.y);
        transform.position += move * speed * Runner.DeltaTime;
    }
}
