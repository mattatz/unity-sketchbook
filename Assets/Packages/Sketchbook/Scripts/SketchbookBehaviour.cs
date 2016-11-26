using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace mattatz.Sketchbook {

    public abstract class SketchbookBehaviour : MonoBehaviour {

        public const string SHADER_PATH = "mattatz/Sketchbook";
        public const float TWO_PI = Mathf.PI * 2f;

        Material material;

        #region Properties

        Color color = Color.white;
        bool fill = true;
        bool useLight = false;
        List<Matrix4x4> matrices = new List<Matrix4x4>() { Matrix4x4.identity };

        #endregion

        #region Monobehaviour Lifecycle

        void Awake () {
            material = new Material(Shader.Find(SHADER_PATH));
            Setup();
        }

        void OnRenderObject () {
            if (material == null) return;
            Draw();
            ClearMatrix();
        }

        #endregion
        
        #region Sketchbook Lifecycle

        protected virtual void Setup () {}
        protected virtual void Draw () {}

        #endregion

        #region Color

        public void SetColor (Color c) {
            this.color = c;
        }

        public void Fill () {
            fill = true;
        }

        public void NoFill () {
            fill = false;
        }

        public void Lights () {
            useLight = true;
        }

        public void NoLights () {
            useLight = false;
        }

        #endregion

        #region Matrix 

        public void PushMatrix () {
            matrices.Add(Matrix4x4.identity);
        }

        public void PopMatrix () {
            if(matrices.Count > 0) {
                matrices.RemoveAt(matrices.Count - 1);
            }
        }

        Matrix4x4 CurrentMatrix () {
            if(matrices.Count > 0) {
                return matrices.Last();
            }
            return Matrix4x4.identity;
        }

        void ClearMatrix () {
            matrices.Clear();
            matrices.Add(Matrix4x4.identity);
        }

        public void Translate (Vector3 position) {
            int n = matrices.Count;
            if(n > 0) {
                matrices[n - 1] = matrices[n - 1] * Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
            }
        }

        public void Rotate (Quaternion rotation) {
            int n = matrices.Count;
            if(n > 0) {
                matrices[n - 1] = matrices[n - 1] * Matrix4x4.TRS(Vector3.zero, rotation, Vector3.one);
            }
        }

        public void Scale (Vector3 scale) {
            int n = matrices.Count;
            if(n > 0) {
                matrices[n - 1] = matrices[n - 1] * Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
            }
        }

        #endregion

        #region Draw

        void Draw (int mode, Matrix4x4 model, Action draw) {
            if(mode == GL.TRIANGLES && useLight) {
                material.SetPass(1);
            } else {
                material.SetPass(0);
            }

            GL.PushMatrix();

            /*
            GL.LoadIdentity();
            GL.MultMatrix(Camera.current.cameraToWorldMatrix * CurrentMatrix() * model);
            */

            GL.MultMatrix(transform.localToWorldMatrix * CurrentMatrix() * model);

            GL.Begin(mode);
            GL.Color(color);
            draw();
            GL.End();
            
            GL.PopMatrix();
        }

        public void DrawLine (Vector3 from, Vector3 to) {
            Draw(GL.LINES, Matrix4x4.identity, () => {
                GL.Vertex(from);
                GL.Vertex(to);
            });
        }

        public void DrawTriangle (Vector3 a, Vector3 b, Vector3 c) {
            if(fill) {
                Draw(GL.LINES, Matrix4x4.identity, () => {
                    GL.Vertex(a); GL.Vertex(b);
                    GL.Vertex(b); GL.Vertex(c);
                    GL.Vertex(c); GL.Vertex(a);
                });
            } else {
                Draw(GL.TRIANGLES, Matrix4x4.identity, () => {
                    GL.Vertex(a);
                    GL.Vertex(b);
                    GL.Vertex(c);
                });
            }
        }

        public void DrawRect (Vector3 center, Quaternion look, Vector2 size) {
            var mat = Matrix4x4.TRS(center, look, Vector3.one);
            var hx = size.x * 0.5f;
            var hy = size.y * 0.5f;

            if(fill) {
                Draw(GL.TRIANGLES, mat, () => {
                    GL.Vertex(new Vector3(-hx, -hy, 0f));
                    GL.Vertex(new Vector3( hx,  hy, 0f));
                    GL.Vertex(new Vector3( hx, -hy, 0f));

                    GL.Vertex(new Vector3(-hx, -hy, 0f));
                    GL.Vertex(new Vector3(-hx,  hy, 0f));
                    GL.Vertex(new Vector3( hx,  hy, 0f));
                });
            } else {
                Draw(GL.LINES, mat, () => {
                    // bottom
                    GL.Vertex(new Vector3(-hx, -hy, 0f));
                    GL.Vertex(new Vector3( hx, -hy, 0f));

                    // right
                    GL.Vertex(new Vector3( hx, -hy, 0f));
                    GL.Vertex(new Vector3( hx,  hy, 0f));

                    // top
                    GL.Vertex(new Vector3( hx,  hy, 0f));
                    GL.Vertex(new Vector3(-hx,  hy, 0f));

                    // left
                    GL.Vertex(new Vector3(-hx,  hy, 0f));
                    GL.Vertex(new Vector3(-hx, -hy, 0f));
                });
            }
        }

        public void DrawCircle (Vector3 center, Quaternion look, float radius, int resolution = 36) {
            var mat = Matrix4x4.TRS(center, look, Vector3.one);

            float step = TWO_PI / resolution;

            if(fill) {
                Draw(GL.TRIANGLES, mat, () => {
                    for(float t = 0f; t <= TWO_PI; t += step) {
                        var x1 = Mathf.Cos(t) * radius;
                        var x2 = Mathf.Cos(t + step) * radius;
                        var y1 = Mathf.Sin(t) * radius;
                        var y2 = Mathf.Sin(t + step) * radius;
                        GL.Vertex3(x1, y1, 0f);
                        GL.Vertex3(0f, 0f, 0f);
                        GL.Vertex3(x2, y2, 0f);
                    }
                });
            } else {
                Draw(GL.LINES, mat, () => {
                    for(float t = 0f; t <= TWO_PI; t += step) {
                        var x1 = Mathf.Cos(t) * radius;
                        var x2 = Mathf.Cos(t + step) * radius;
                        var y1 = Mathf.Sin(t) * radius;
                        var y2 = Mathf.Sin(t + step) * radius;
                        GL.Vertex3(x1, y1, 0f);
                        GL.Vertex3(x2, y2, 0f);
                    }
                });
            }
        }

        Vector3[] corners = new Vector3[] {
            // bottom
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3( 0.5f, -0.5f, -0.5f),
            new Vector3( 0.5f, -0.5f,  0.5f),
            new Vector3(-0.5f, -0.5f,  0.5f),

            // top
            new Vector3(-0.5f,  0.5f, -0.5f),
            new Vector3( 0.5f,  0.5f, -0.5f),
            new Vector3( 0.5f,  0.5f,  0.5f),
            new Vector3(-0.5f,  0.5f,  0.5f),
        };

        public void DrawBox (Vector3 center, Quaternion look, Vector3 size) {
            if(fill) {
                Draw(GL.TRIANGLES, Matrix4x4.TRS(center, look, size), () => {
                    // bottom
                    GL.Vertex(corners[0]);
                    GL.Vertex(corners[2]);
                    GL.Vertex(corners[3]);

                    GL.Vertex(corners[0]);
                    GL.Vertex(corners[1]);
                    GL.Vertex(corners[2]);

                    // top
                    GL.Vertex(corners[4]);
                    GL.Vertex(corners[7]);
                    GL.Vertex(corners[6]);

                    GL.Vertex(corners[4]);
                    GL.Vertex(corners[6]);
                    GL.Vertex(corners[5]);

                    // front
                    GL.Vertex(corners[0]);
                    GL.Vertex(corners[4]);
                    GL.Vertex(corners[5]);

                    GL.Vertex(corners[0]);
                    GL.Vertex(corners[5]);
                    GL.Vertex(corners[1]);

                    // back
                    GL.Vertex(corners[3]);
                    GL.Vertex(corners[6]);
                    GL.Vertex(corners[7]);

                    GL.Vertex(corners[2]);
                    GL.Vertex(corners[6]);
                    GL.Vertex(corners[3]);

                    // left
                    GL.Vertex(corners[0]);
                    GL.Vertex(corners[3]);
                    GL.Vertex(corners[7]);

                    GL.Vertex(corners[0]);
                    GL.Vertex(corners[7]);
                    GL.Vertex(corners[4]);

                    // right
                    GL.Vertex(corners[2]);
                    GL.Vertex(corners[1]);
                    GL.Vertex(corners[5]);

                    GL.Vertex(corners[2]);
                    GL.Vertex(corners[5]);
                    GL.Vertex(corners[6]);
                });
            } else {
                Draw(GL.LINES, Matrix4x4.TRS(center, look, size), () => {
                    // bottom
                    GL.Vertex(corners[0]);
                    GL.Vertex(corners[1]);
                    GL.Vertex(corners[1]);
                    GL.Vertex(corners[2]);
                    GL.Vertex(corners[2]);
                    GL.Vertex(corners[3]);
                    GL.Vertex(corners[3]);
                    GL.Vertex(corners[0]);

                    // top
                    GL.Vertex(corners[4]);
                    GL.Vertex(corners[5]);
                    GL.Vertex(corners[5]);
                    GL.Vertex(corners[6]);
                    GL.Vertex(corners[6]);
                    GL.Vertex(corners[7]);
                    GL.Vertex(corners[7]);
                    GL.Vertex(corners[4]);

                    // side
                    GL.Vertex(corners[0]);
                    GL.Vertex(corners[4]);
                    GL.Vertex(corners[1]);
                    GL.Vertex(corners[5]);
                    GL.Vertex(corners[2]);
                    GL.Vertex(corners[6]);
                    GL.Vertex(corners[3]);
                    GL.Vertex(corners[7]);
                });
            }

        }

        public void DrawSphere (Vector3 center, Quaternion look, float radius, int resolution = 16) {
            DrawSphere(center, look, radius, resolution, resolution);
        }

        public void DrawSphere (Vector3 center, Quaternion look, float radius, int uresolution, int vresolution) {
            int len = (uresolution + 1) * vresolution + 2;

            var vertices = new Vector3[len];
            vertices[0] = Vector3.up * radius;
            for (int v = 0; v < vresolution; v++) {
                float a1 = Mathf.PI * (float)(v + 1) / (vresolution + 1);
                float sin = Mathf.Sin(a1);
                float cos = Mathf.Cos(a1);
                for (int u = 0; u <= uresolution; u++) {
                    float a2 = TWO_PI * (float)(u == uresolution ? 0 : u) / uresolution;
                    float sin2 = Mathf.Sin(a2);
                    float cos2 = Mathf.Cos(a2);
                    vertices[u + v * (uresolution + 1) + 1] = new Vector3(sin * cos2, cos, sin * sin2) * radius;
                }
            }
            vertices[vertices.Length - 1] = Vector3.up * -radius;

            var mat = Matrix4x4.TRS(center, look, Vector3.one);

            if(fill) {
                Draw(GL.TRIANGLES, mat, () => {
                    // top cap
                    for (int u = 0; u < uresolution; u++) {
                        GL.Vertex(vertices[u + 2]);
                        GL.Vertex(vertices[u + 1]);
                        GL.Vertex(vertices[0]);
                    }
                    // middle
                    for (int v = 0; v < vresolution - 1; v++) {
                        for (int u = 0; u < uresolution; u++) {
                            int current = u + v * (uresolution + 1) + 1;
                            int next = current + uresolution + 1;

                            GL.Vertex(vertices[current]);
                            GL.Vertex(vertices[current + 1]);
                            GL.Vertex(vertices[next + 1]);

                            GL.Vertex(vertices[current]);
                            GL.Vertex(vertices[next + 1]);
                            GL.Vertex(vertices[next]);
                        }
                    }
                    // bottom cap
                    for (int u = 0; u < uresolution; u++) {
                        GL.Vertex(vertices[len - 1]);
                        GL.Vertex(vertices[len - (u + 2) - 1]);
                        GL.Vertex(vertices[len - (u + 1) - 1]);
                    }
                });
            } else {
                Draw(GL.LINES, mat, () => {
                    // top cap
                    for (int u = 0; u < uresolution; u++) {
                        GL.Vertex(vertices[0]);
                        GL.Vertex(vertices[u + 1]);
                    }
                    // middle
                    for (int v = 0; v < vresolution - 1; v++) {
                        for (int u = 0; u < uresolution; u++) {
                            int current = u + v * (uresolution + 1) + 1;
                            int next = current + uresolution + 1;

                            GL.Vertex(vertices[current]);
                            GL.Vertex(vertices[current + 1]);

                            GL.Vertex(vertices[next + 1]);
                            GL.Vertex(vertices[next]);

                            GL.Vertex(vertices[current]);
                            GL.Vertex(vertices[next]);
                        }
                    }
                    // bottom cap
                    for (int u = 0; u < uresolution; u++) {
                        GL.Vertex(vertices[len - 1]);
                        GL.Vertex(vertices[len - (u + 1) - 1]);
                    }
                });
            }

        }

        #endregion

    }

}


