using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaintCoinach.Graphics {
    public class Model {
        #region Properties
        public ModelDefinition Definition { get; private set; }
        public ModelQuality Quality { get; private set; }
        public ModelHeader Header { get { return Definition.ModelHeaders[(int)Quality]; } }
        public Mesh[] Meshes { get; private set; }
        #endregion

        #region Constructor
        public Model(ModelDefinition definition, ModelQuality quality) {
            Definition = definition;
            Quality = quality;

            Build();
        }
        #endregion

        #region Build
        private void Build() {
            const int VertexPartOffset = 2;
            const int IndexPartOffset = 8;

            byte[] vertexBuffer = Definition.File.GetPart(VertexPartOffset + (int)Quality);
            byte[] indexBuffer = Definition.File.GetPart(IndexPartOffset + (int)Quality);

            this.Meshes = new Mesh[Header.MeshCount];
            for (int i = 0; i < Header.MeshCount; ++i) {
                Mesh mesh = new Mesh(this, Header.MeshOffset + i, vertexBuffer, indexBuffer);

                Meshes[i] = mesh;
            }
        }
        #endregion
    }
}
