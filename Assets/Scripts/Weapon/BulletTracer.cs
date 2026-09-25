using UnityEngine;

namespace Medallas.Weapon
{
    // Estela visual del disparo: viaja del cañón al punto de impacto en poco
    // tiempo (no es un proyectil real, el daño ya se aplico por raycast) y se
    // autodestruye al llegar, para que el jugador vea por donde fue la bala.
    public class BulletTracer : MonoBehaviour
    {
        Vector3 start;
        Vector3 end;
        float speed;
        float travelTime;
        float elapsed;

        public void Init(Vector3 startPoint, Vector3 endPoint, float tracerSpeed)
        {
            start = startPoint;
            end = endPoint;
            speed = Mathf.Max(1f, tracerSpeed);
            transform.position = start;

            Vector3 delta = end - start;
            float distance = delta.magnitude;
            if (distance > 0.001f)
                transform.rotation = Quaternion.LookRotation(delta);

            travelTime = distance / speed;
            if (travelTime <= 0f) travelTime = 0.01f;
        }

        void Update()
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / travelTime);
            transform.position = Vector3.Lerp(start, end, t);

            if (t >= 1f)
                Destroy(gameObject);
        }
    }
}
