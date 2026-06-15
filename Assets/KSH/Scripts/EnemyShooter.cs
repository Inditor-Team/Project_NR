using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [SerializeField] private GameObject bullet;
    public float shootSpeed = 10.0f;

    public void SetPosition(Vector3 playerPosition)
    {
        Vector3 targetPostion = playerPosition - transform.position;
        GameObject newBullet = Instantiate(bullet, transform);
        
        Vector2 shootPositon = new Vector2(targetPostion.x, targetPostion.y);
        newBullet.GetComponent<Rigidbody2D>().AddForce(shootPositon * shootSpeed);
    }
}
