using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace mattatz.Sketchbook {

    public class SnakePoints : SketchbookBehaviour {

        public List<Point> points;
        public Vector3 emitter = Vector3.zero;
        public Gradient grad;

        public float noiseSpeed = 1f;
        public float noiseScale = 5f;
        public float noiseIntensity = 10f;

        bool dragging;

        protected override void Setup() {
            points = new List<Point>();
        }

        void Update () {
            if(!dragging) {
                var dt = Time.deltaTime;
                emitter += new Vector3(
                    Mathf.PerlinNoise(Time.timeSinceLevelLoad, emitter.x) - 0.5f, 
                    Mathf.PerlinNoise(Time.timeSinceLevelLoad, emitter.y) - 0.5f, 
                    0f
                ) * 10f * dt;
                dragging = Input.GetMouseButtonDown(0);
            } else {
                var world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                emitter.x = world.x;
                emitter.y = world.y;

                if(Input.GetMouseButtonUp(0)) {
                    dragging = false;
                }
            }

            points.Add(new Point(emitter, grad.Evaluate(Random.value), Random.Range(0.02f, 0.2f), Random.Range(3f, 10f)));

            for(int i = points.Count - 1; i >= 0; i--) {
                var p = points[i];
                p.Update(this, Time.deltaTime);
                if (p.lifetime <= 0f) points.RemoveAt(i);
            }

        }

        protected override void Draw() {

            NoLights();

            points.ForEach(p => {
                p.Draw(this);
            });

        }

        public class Point {
            public Vector3 position;
            public Vector3 velocity;
            public Color color;
            public float radius;
            public float lifetime;
            float length;

            public Point (Vector3 p, Color c, float r, float l) {
                position = p;
                color = c;
                radius = r;
                lifetime = length = l;
            }

            public void Update (SnakePoints s, float dt) {
                float t = Time.timeSinceLevelLoad * s.noiseSpeed;

                float n = (Mathf.PerlinNoise((position.x + t) * s.noiseScale, (position.y + t) * s.noiseScale) - 0.5f) * TWO_PI * 2f; 
                velocity += new Vector3(Mathf.Cos(n), Mathf.Sin(n), 0f) * s.noiseIntensity * dt;
                velocity *= 0.9f; // decay
                lifetime -= dt;

                position += velocity;
            }

            public void Draw (SnakePoints s) {
                s.Fill();
                s.SetColor(color);
                s.DrawCircle(position, Quaternion.LookRotation(Vector3.forward), radius * (lifetime / length));

                s.NoFill();
                s.SetColor(Color.black);
                s.DrawCircle(position, Quaternion.LookRotation(Vector3.forward), radius * (lifetime / length));
            }
        }

    }

}


