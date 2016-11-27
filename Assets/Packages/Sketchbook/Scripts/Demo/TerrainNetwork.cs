using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace mattatz.Sketchbook {

    public class TerrainNetwork : SketchbookBehaviour {

		List<Node> nodes;
		public int width = 20;
		public int height = 20;
		public float unit = 0.5f;

        protected override void Setup() {
			nodes = new List<Node>();

			var offsetX = - width * unit * 0.5f;
			var offsetY = - height * unit * 0.5f;

			for(int y = 0; y < height; y++) {
				for(int x = 0; x < width; x++) {
					var p = new Vector3(x * unit + offsetX, 0f, y * unit + offsetY);
					var n = new Node(p);
					nodes.Add(n);
				}
			}
        }

        void Update () {
			var dt = Time.deltaTime;
			nodes.ForEach(node => node.Update(dt));
        }

        protected override void Draw() {
			SetColor(Color.black);
			nodes.ForEach(node => node.Draw(this));
        }

		class Node {

			Vector3 position;
			Vector3 velocity;

			float offset;

			public Node (Vector3 p) {
				position = p;
				offset = Random.Range(0f, 100f);
			}

			public void Update (float dt) {
			}

			Quaternion look = Quaternion.LookRotation(Vector3.down, Vector3.back);
			public void Draw (TerrainNetwork s) {
				s.DrawCircle(position, look, 0.2f, 6);
			}

		}

    }

}


