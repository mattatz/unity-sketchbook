using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace mattatz.Sketchbook {

    public class BranchLines : SketchbookBehaviour {

        Segment root;
        public int depth = 5;
        public int branches = 5;
        public Vector2 rotation = new Vector2(-40f, 40f);
        public Vector2 decay = new Vector2(0.75f, 0.9f);

        [Range(1f, 20f)] public float animationSpeed = 10f;

        protected override void Setup() {
            var q = Quaternion.AngleAxis(Random.Range(-5, 5f), Vector3.forward);
            root = new Segment(depth, transform.position, transform.position + q * Vector3.up * Random.Range(1f, 2f));
            root.Grow(this);
        }

        void Update () {
            if(Input.GetMouseButtonDown(0)) {
                var q = Quaternion.AngleAxis(Random.Range(-5f, 5f), Vector3.forward);
                root = new Segment(depth, transform.position, transform.position + q * Vector3.up * Random.Range(1f, 2));
                root.Grow(this);
            }

            root.Animate(Time.deltaTime * Mathf.Max(animationSpeed, 1f));
        }

        protected override void Draw() {
            SetColor(Color.black);
            root.Draw(this);

            Lights();
            Fill();
            SetColor(new Color(1f, 0f, 0f, 0.5f));
            DrawBox(Vector3.zero, Quaternion.LookRotation(Vector3.forward), new Vector3(2f, 2f, 2f));
        }

        public class Segment {
            public Vector3 from, to;
            public float t = 0f;
            List<Segment> children;

            int depth;
            Vector3 cur;

            public Segment (int d, Vector3 p0, Vector3 p1) {
                children = new List<Segment>();

                depth = d;
                from = cur = p0;
                to = p1;
                t = 0f;
            }

            public virtual void Grow (BranchLines s) {
                if (depth <= 0) {
                    children.Add(new Flower(0, to, to));
                    return;
                }

                if (depth <= 1) {
                    var flowers = Random.Range(0, 2);
                    for(int i = 0; i < flowers; i++) {
                        children.Add(new Flower(0, Vector3.Lerp(from, to, Random.Range(0.75f, 0.9f)), to));
                    }
                }

                int count = Random.Range(1, s.branches);
                for(int i = 0; i < count; i++) {
                    var q = Quaternion.AngleAxis(Random.Range(s.rotation.x, s.rotation.y), Vector3.forward);
                    var segment = new Segment(depth - 1, to, to + q * (to - from) * Random.Range(s.decay.x, s.decay.y));
                    segment.Grow(s);
                    children.Add(segment);
                }
            }

            public virtual void Animate (float dt) {
                if(t >= 1f) {
                    children.ForEach(c => {
                        c.Animate(dt);
                    });
                    return;
                }

                t += dt;
                cur = Vector3.Lerp(from, to, Mathf.Clamp01(t));
            }

            public virtual void Draw(BranchLines s) {
                s.SetColor(Color.black);
                s.DrawLine(from, cur);
                children.ForEach(c => c.Draw(s));
            }
        }

        public class Flower : Segment {

            float offset;
            Color petal = new Color(1.0f, 0.72f, 0.8f);
            Color head = new Color(0.9f, 0.92f, 0.2f);

            public Flower (int d, Vector3 p0, Vector3 p1) : base(d, p0, p1) {
                offset = Random.Range(0f, TWO_PI);
            }

            public override void Grow(BranchLines s) {}

            public override void Animate(float dt) {
                t = Mathf.Clamp01(t + dt);
            }

            public override void Draw (BranchLines s) {

                s.SetColor(petal);

                for(int i = 0; i < 8; i++) {
                    float r = (float)i / 8 * TWO_PI + offset;
                    float x = Mathf.Cos(r), y = Mathf.Sin(r);
                    var p = new Vector3(x, y, 0f) * 0.1f * t;
                    s.DrawLine(from, from + p);
                }

                s.NoLights();
                s.SetColor(head);
                s.DrawCircle(from, Quaternion.LookRotation(Vector3.forward), 0.025f * t);

            }

        }

    }

}


