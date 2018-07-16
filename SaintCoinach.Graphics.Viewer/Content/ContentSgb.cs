using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace SaintCoinach.Graphics.Viewer.Content {
    public class ContentSgb : Drawable3DComponent {
        #region Fields
        private ComponentContainer _Content = new ComponentContainer();
        #endregion

        #region Properties
        public Sgb.SgbFile SgbFile{ get; private set; }
        public Data.ParametersBase Parameters { get; set; }
        public Matrix Transformation { get; set; }
        #endregion

        #region Constructor
        public ContentSgb(Engine engine, Sgb.SgbFile sgbFile):this(engine, sgbFile, null) { }
        public ContentSgb(Engine engine, Sgb.SgbFile sgbFile, Data.ParametersBase parameters) : base(engine) {
            this.SgbFile = sgbFile;
            this.Parameters = parameters;
            this.Transformation = Matrix.Identity;
            void LoadSgbFile(Sgb.SgbFile file, Sgb.SgbGimmickEntry transformGimmick = null) {
                if (file == null)
                    return;

                foreach (var group in file.Data.OfType<Sgb.SgbGroup>()) {
                    foreach (var mdl in group.Entries.OfType<Sgb.SgbModelEntry>()) {
                        //if (transformGimmick != null)
                        _Content.Add(new ContentModel(engine, mdl.Model) {
                            Parameters = parameters,
                        });
                    }
                    foreach (var sgb1cEntry in group.Entries.OfType<Sgb.SgbGroup1CEntry>()) {
                        //LoadSgbFile(sgb1cEntry.Gimmick, transformGimmick);
                    }
                    foreach (var gimmickEntry in group.Entries.OfType<Sgb.SgbGimmickEntry>()) {
                        LoadSgbFile(gimmickEntry.Gimmick, gimmickEntry);
                    }
                }
            }
            LoadSgbFile(sgbFile, null);
        }
        #endregion
        
        public override void LoadContent() {
            _Content.LoadContent();
            base.LoadContent();
        }
        public override void UnloadContent() {
            _Content.UnloadContent();
            base.UnloadContent();
        }
        public override void Update(EngineTime engineTime) {
            _Content.Update(engineTime);
            base.Update(engineTime);
        }
        public override void Draw(EngineTime time, ref SharpDX.Matrix world, ref SharpDX.Matrix view, ref SharpDX.Matrix projection) {
            var adjustedWorld = Transformation * world;
            _Content.Draw(time, ref adjustedWorld, ref view, ref projection);
        }
    }
}
