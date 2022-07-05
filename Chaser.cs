using System.Linq;
using UnityEngine;

namespace AspyCompanion
{
    internal class Chaser : MonoBehaviour
    {
        public float accelerationForce = 25;
        public float speedMax = 30;

        private Rigidbody2D _rb;
        private GameObject _target;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            GameObject closestEnemy = null;
            float closestDistance = float.MaxValue;
            var enemies = FindObjectsOfType<GameObject>().Where(go => go.GetComponent<HealthManager>() != null).ToArray();
            for (int i = 0; i < enemies.Count(); i++)
            {
                var enemy = enemies[i];
                float distance = (enemy.transform.position - transform.position).magnitude;
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }
            _target = closestEnemy;
        }

        private void Update()
        {
            if (_target == null) return;

            Vector2 vector = _target.transform.position - transform.position;
			vector = Vector2.ClampMagnitude(vector, 1);
			vector *= accelerationForce;
			_rb.AddForce(vector);
			Vector2 velocity = _rb.velocity;
			velocity = Vector2.ClampMagnitude(velocity, speedMax);
			_rb.velocity = velocity;
        }
    }
}