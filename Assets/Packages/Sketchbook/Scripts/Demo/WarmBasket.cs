using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace mattatz.Sketchbook {

    public class WarmBasket : SketchbookBehaviour {

        public float sphereRadius = 4.5f;
        public float rotationScale = 0.1f;
        public float rotationSpeed = 50f;
        public bool useGravity = false;

        List<Warm> strokes;

        protected override void Setup() {
            strokes = new List<Warm>();

            for(int i = 0; i < 50; i++) {
                strokes.Add(new Warm(Random.insideUnitSphere * 0.25f, Color.black, 30));
            }
        }

        void Update () {
            var dt = Time.deltaTime;

            strokes.ForEach(stroke => {
                stroke.Update(this, dt);
            });

            var t = Time.timeSinceLevelLoad * rotationScale;
            var axis = new Vector3(
                Mathf.PerlinNoise(t, 0f) - 0.5f,
                Mathf.PerlinNoise(0f, t) - 0.5f,
                Mathf.PerlinNoise(t, t) - 0.5f
            );
            transform.localRotation *= Quaternion.AngleAxis(Time.deltaTime * rotationSpeed, axis);

            if(!useGravity) {
                useGravity = Input.GetMouseButtonDown(0);
            } else {
                useGravity = !Input.GetMouseButtonUp(0);
            }
        }

        protected override void Draw() {

            strokes.ForEach(stroke => {
                stroke.Draw(this);
            });

            SetColor(new Color(0.8f, 0.8f, 0.8f, 0.5f));
            NoFill();
            DrawSphere(Vector3.zero, Quaternion.LookRotation(Vector3.forward), sphereRadius, 10, 4);
        }

        public class Warm {

            List<Vector3> points;
            Color color;
            Vector3 velocity;

            float offset;
            float decay;
            float limit = 0.45f;
            
            public Warm (Vector3 p, Color c, int count) {
                color = c;
                points = new List<Vector3>();
                for(int i = 0; i < count; i++) {
                    points.Add(p);
                }

                offset = Random.Range(0f, 300f);
                decay = Random.Range(0.5f, 0.95f);
            }

            public void Update (WarmBasket s, float dt) {
                var head = points[0];

                var t = Time.timeSinceLevelLoad * 0.25f;

                var nx = Mathf.PerlinNoise(t, head.x * 0.1f + offset) - 0.5f;
                var ny = Mathf.PerlinNoise(head.y * 0.1f + offset, t) - 0.5f;
                var nz = Mathf.PerlinNoise(t + offset + head.z * 0.1f, t + offset) - 0.5f;

                const float intensity = 1.0f;
                velocity.x += Mathf.Cos(nx * TWO_PI * 2f) * intensity;
                velocity.y += Mathf.Sin(ny * TWO_PI * 2f) * intensity;
                velocity.z += Mathf.Cos(nz * TWO_PI * 2f) * intensity;

                if(s.useGravity) {
                    velocity += s.transform.InverseTransformDirection(Vector3.down) * 5f;
                }

                if(head.magnitude >= s.sphereRadius) {
                    velocity += -head;
                }

                velocity *= decay;

                head += velocity * dt;
                head = head.normalized * Mathf.Clamp(head.magnitude, 0f, s.sphereRadius);

                points[0] = head;

                Chain();
            }

            void Chain () {
                for(int i = 1, n = points.Count; i < n; i++) {
                    var p0 = points[i - 1];
                    var p1 = points[i];
                    var dir = (p1 - p0);
                    if(dir.magnitude > limit) {
                        p1 = p0 + dir.normalized * limit;
                        points[i] = p1;
                    }
                }
            }

			Quaternion look = Quaternion.LookRotation(Vector3.forward);

            public void Draw (WarmBasket s) {
                s.SetColor(color);
                for(int i = 1, n = points.Count; i < n; i++) {
                    s.DrawLine(points[i - 1], points[i]);
                }

                s.Fill();

                float t = (Mathf.Sin(offset + Time.timeSinceLevelLoad * (decay * 5.0f)) + 1.0f) * 0.5f + 0.5f;
                t = Mathf.Clamp01(t);
                t = t * t;
                s.DrawSphere(points[0], look, limit * 0.25f * t);
            }

        }

    }

}


