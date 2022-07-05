using GlobalEnums;
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
            _target = FindObjectsOfType<GameObject>().Where(go => go.layer == (int)PhysLayers.ENEMIES).OrderBy(enemy => (transform.position - enemy.transform.position).magnitude).First();
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