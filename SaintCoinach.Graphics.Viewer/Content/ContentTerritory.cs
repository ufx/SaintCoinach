using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaintCoinach.Graphics.Viewer.Content {
    using SharpDX;
    using System.Numerics;

    public class ContentTerritory : Drawable3DComponent {
        #region Fields
        private ComponentContainer _TerrainContainer = new ComponentContainer();
        private ComponentContainer _LgbPartsContainer = new ComponentContainer();
        private string _ExportFileName;
        private string _ExportDirectory;
        #endregion

        #region Properties
        public Territory Territory { get; private set; }
        public Task ExportTask { get; private set; }
        #endregion

        #region Constructor
        public ContentTerritory(Engine engine, Territory territory) : base(engine) {
            this.Territory = territory;
            if (System.Windows.Forms.MessageBox.Show("Task will run in background", $"Export Territory {territory.Name}?", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes) {
                ExportTask = Task.Run(() => Export(territory));
            }
            if (territory.Terrain != null) {
                foreach (var part in territory.Terrain.Parts)
                    _TerrainContainer.Add(new ContentModel(engine, part));
            }
            foreach (var lgb in territory.LgbFiles) {
                foreach(var group in lgb.Groups) {
                    foreach(var part in group.Entries) {
                        var asMdl = part as Lgb.LgbModelEntry;
                        var asGim = part as Lgb.LgbGimmickEntry;
                        var asEobj = part as Lgb.LgbEventObjectEntry;

                        if (asMdl != null && asMdl.Model != null) {
                            _LgbPartsContainer.Add(new ContentModel(engine, asMdl.Model));
                        }
                        if (asGim != null && asGim.Gimmick != null) {
                            _LgbPartsContainer.Add(new ContentSgb(engine, asGim.Gimmick) {
                                Transformation =
                                    Matrix.Scaling(asGim.Header.Scale.ToDx())
                                    * Matrix.RotationX(asGim.Header.Rotation.X)
                                    * Matrix.RotationY(asGim.Header.Rotation.Y)
                                    * Matrix.RotationZ(asGim.Header.Rotation.Z)
                                    * Matrix.Translation(asGim.Header.Translation.ToDx())
                            });
                        }
                        if (asEobj != null && asEobj.Gimmick != null) {
                            _LgbPartsContainer.Add(new ContentSgb(engine, asEobj.Gimmick) {
                                Transformation =
                                    Matrix.Scaling(asEobj.Header.Scale.ToDx())
                                    * Matrix.RotationX(asEobj.Header.Rotation.X)
                                    * Matrix.RotationY(asEobj.Header.Rotation.Y)
                                    * Matrix.RotationZ(asEobj.Header.Rotation.Z)
                                    * Matrix.Translation(asEobj.Header.Translation.ToDx())
                            });
                        }
                    }
                }
            }
        }
        #endregion

        #region Export

        public void Export(Territory territory) {
            try {
                _ExportDirectory = $"./{territory.Name}/";
                Dictionary<string, int> objCount = new Dictionary<string, int>();
                if (!System.IO.Directory.Exists(Environment.CurrentDirectory + $"{_ExportDirectory}")) {
                    System.IO.Directory.CreateDirectory(Environment.CurrentDirectory + $"{_ExportDirectory}");
                }

                var fileName = $"./{_ExportDirectory}/{territory.Name}.obj";
                _ExportFileName = fileName;
                var f = System.IO.File.Create(fileName);
                f.Dispose();
                f.Close();
                System.IO.File.AppendAllText(fileName, $"o {territory.Name}\n");

                List<string> vertStr = new List<string>();
                List<string> faceStr = new List<string>();
                Dictionary<string, bool> exportedPaths = new Dictionary<string, bool>();
                UInt64 vs = 1, vt = 1, vn = 1;
                UInt64 i = 0;

                void ExportMaterials(Material m, string path) {
                    bool found = false;
                    if (exportedPaths.TryGetValue(path, out found)) {
                        return;
                    }
                    exportedPaths.Add(path, true);
                    System.IO.File.Delete($"{_ExportDirectory}/{path}.mtl");
                    vertStr.Add($"mtllib {path}.mtl");
                    System.IO.File.AppendAllText($"{_ExportDirectory}/{path}.mtl", $"newmtl {path}\n");
                    foreach (var img in m.TexturesFiles) {
                        var mtlName = img.Path.Replace('/', '_');
                        if (exportedPaths.TryGetValue(path + mtlName, out found)) {
                            continue;
                        }

                        SaintCoinach.Imaging.ImageConverter.Convert(img).Save($"{_ExportDirectory}/{mtlName}.png");
                        if (mtlName.Contains("_dummy_"))
                            continue;
                        if (mtlName.Contains("_n.tex")) {
                            System.IO.File.AppendAllText($"./{_ExportDirectory}/{path}.mtl", $"bump {mtlName}.png\n");
                        }
                        else if (mtlName.Contains("_s.tex")) {
                            System.IO.File.AppendAllText($"./{_ExportDirectory}/{path}.mtl", $"map_Ks {mtlName}.png\n");
                        }
                        else if (!mtlName.Contains("_a.tex")) {
                            System.IO.File.AppendAllText($"./{_ExportDirectory}/{path}.mtl", $"map_Kd {mtlName}.png\n");
                        }
                        else {
                            System.IO.File.AppendAllText($"./{_ExportDirectory}/{path}.mtl", $"map_Ka {mtlName}.png\n");
                        }

                        exportedPaths.Add(path + mtlName, true);
                    }
                }

                void ExportModel(ref Mesh mesh, TransformedModel tlMdl, string materialName, string modelFilePath, TransformedModel ogMdl = null, List<Graphics.Vector3> transformations = null) {
                    i++;

                    var k = 0;
                    UInt64 tempVs = 0, tempVn = 0, tempVt = 0;
                    foreach (var v in mesh.Vertices) {
                        if (v.Position != null) {

                            var x = v.Position.Value.X;
                            var y = v.Position.Value.Y;
                            var z = v.Position.Value.Z;
                            var w = v.Position.Value.W;

                            //else 
                            {
var transform = Matrix4x4.CreateScale(tlMdl.Scale.X, tlMdl.Scale.Y, tlMdl.Scale.Z)
                            * Matrix4x4.CreateRotationX(tlMdl.Rotation.X)
                            * Matrix4x4.CreateRotationY(tlMdl.Rotation.Y)
                            * Matrix4x4.CreateRotationZ(tlMdl.Rotation.Z)
                            * Matrix4x4.CreateTranslation(tlMdl.Translation.X, tlMdl.Translation.Y, tlMdl.Translation.Z);

                            var t = Matrix4x4.CreateTranslation(x, y, z) * transform;
                            x = t.Translation.X;
                            y = t.Translation.Y;
                            z = t.Translation.Z;
                            }

                            if (transformations != null) {
                                for (var j = transformations.Count; j > 0; j -= 3) {

                                    var translation = transformations[j - 3];
                                    var rot = transformations[j - 2];
                                    var scale = transformations[j - 1];

                                    var tform = Matrix4x4.CreateScale(scale.X, scale.Y, scale.Z)
                                    * Matrix4x4.CreateRotationX(rot.X)
                                    * Matrix4x4.CreateRotationY(rot.Y)
                                    * Matrix4x4.CreateRotationZ(rot.Z)
                                    * Matrix4x4.CreateTranslation(translation.X, translation.Y, translation.Z);

                                    var tlation = Matrix4x4.CreateTranslation(x, y, z) * tform;
                                    x = tlation.Translation.X;
                                    y = tlation.Translation.Y;
                                    z = tlation.Translation.Z;
                                    break;
                                }
                            }

                            if (ogMdl != null) {
                                var transform = Matrix4x4.CreateScale(ogMdl.Scale.X, ogMdl.Scale.Y, ogMdl.Scale.Z)
                                * Matrix4x4.CreateRotationX(ogMdl.Rotation.X)
                                * Matrix4x4.CreateRotationY(ogMdl.Rotation.Y)
                                * Matrix4x4.CreateRotationZ(ogMdl.Rotation.Z)
                                * Matrix4x4.CreateTranslation(ogMdl.Translation.X, ogMdl.Translation.Y, ogMdl.Translation.Z);

                                var t = Matrix4x4.CreateTranslation(x, y, z) * transform;
                                x = t.Translation.X;
                                y = t.Translation.Y;
                                z = t.Translation.Z;
                            }

                            vertStr.Add($"v {x} {y} {z} {v.Position.Value.W}");
                            tempVs++;
                        }
                        if (v.Normal != null) {
                            vertStr.Add($"vn {v.Normal.Value.X} {v.Normal.Value.Y} {v.Normal.Value.Z}");
                            tempVn++;
                        }
                        if (v.UV != null) {
                            vertStr.Add($"vt {v.UV.Value.X} {v.UV.Value.Y} {v.UV.Value.Z} {v.UV.Value.W}");
                            tempVt++;
                        }
                    }
                    vertStr.Add($"g {modelFilePath}_{i.ToString()}_{k.ToString()}");
                    if (materialName != null) {
                        vertStr.Add($"usemtl {materialName}");
                    }
                    for (UInt64 j = 0; j + 3 < (UInt64)mesh.Indices.Length + 1; j += 3) {
                        vertStr.Add(
                            $"f " +
                            $"{mesh.Indices[j] + vs}/{mesh.Indices[j] + vt}/{mesh.Indices[j] + vn} " +
                            $"{mesh.Indices[j + 1] + vs}/{mesh.Indices[j + 1] + vt}/{mesh.Indices[j + 1] + vn} " +
                            $"{mesh.Indices[j + 2] + vs}/{mesh.Indices[j + 2] + vt}/{mesh.Indices[j + 2] + vn}");
                    }
                    if (i % 3000 == 0) {
                        System.IO.File.AppendAllLines(_ExportFileName, vertStr);
                        vertStr.Clear();
                    }
                    vs += tempVs;
                    vn += tempVn;
                    vt += tempVt;
                }


                void ExportSgbFile(Sgb.SgbFile sgbFile, Graphics.Vector3 translation, Graphics.Vector3 rotation, Graphics.Vector3 scale, List<Graphics.Vector3> transformations = null) {
                    if (sgbFile == null)
                        return;
                    foreach (var sgbGroup in sgbFile.Data.OfType<Sgb.SgbGroup>()) {
                        bool newGroup = true;
                        foreach (var mdl in sgbGroup.Entries.OfType<Sgb.SgbModelEntry>()) {
                            if (newGroup) {
                                vertStr.Add($"o {sgbFile.File.Path}_{sgbGroup.Name}_{i}");
                                newGroup = false;
                            }
                            var newMdl = mdl.Model.Model.GetModel(ModelQuality.High);
                            var tlMdl = new TransformedModel(newMdl.Definition, translation, rotation, scale);

                            for (var j = 0; j < newMdl.Meshes.Length; ++j) {
                                var mesh = newMdl.Meshes[j];
                                var mtl = mesh.Material.Get();
                                var path = mtl.File.Path.Replace('/', '_').Replace(".mtrl", ".tex");

                                ExportMaterials(mtl, path);
                                ExportModel(ref mesh, mdl.Model, path, mdl.ModelFilePath, tlMdl, transformations);
                            }
                        }
                        foreach (var gimmickEntry in sgbGroup.Entries.OfType<Sgb.SgbGimmickEntry>()) {

                            transformations = transformations ?? new List<Graphics.Vector3>();
                            //transformations.Clear();
                            transformations.Add(gimmickEntry.Header.Translation);
                            transformations.Add(gimmickEntry.Header.Rotation);
                            transformations.Add(gimmickEntry.Header.Scale);

                            ExportSgbFile(gimmickEntry.Gimmick, translation, rotation, scale, transformations);
                        }
                        foreach (var sgb1c in sgbGroup.Entries.OfType<Sgb.SgbGroup1CEntry>()) {
                            //ExportSgbFile(sgb1c.Gimmick, translation, rotation, scale, transformations);
                        }
                    }
                }

                if (territory.Terrain != null) {
                    foreach (var part in territory.Terrain.Parts) {
                        var mdl = part.Model.GetModel(ModelQuality.High);

                        for (var j = 0; j < mdl.Meshes.Length; ++j) {
                            var mesh = mdl.Meshes[j];
                            var mtl = mesh.Material.Get();
                            var path = mtl.File.Path.Replace('/', '_').Replace(".mtrl", ".tex");

                            ExportMaterials(mtl, path);
                            ExportModel(ref mesh, part, path, mdl.Definition.File.Path);
                        }
                    }
                }

                foreach (var lgb in territory.LgbFiles) {
                    foreach (var lgbGroup in lgb.Groups) {
                        bool newGroup = true;
                        foreach (var part in lgbGroup.Entries) {
                            if (part == null)
                                continue;
                            bool validEntry = false;
                            switch (part.Type) {
                                case Lgb.LgbEntryType.Model:
                                    validEntry = true;

                                    var asMdl = part as Lgb.LgbModelEntry;
                                    var hq = asMdl.Model.Model.GetModel(ModelQuality.High);

                                    for (var j = 0; j < hq.Meshes.Length; ++j) {
                                        var mesh = hq.Meshes[j];
                                        var mtl = mesh.Material.Get();
                                        var path = mtl.File.Path.Replace('/', '_').Replace(".mtrl", ".tex");

                                        ExportMaterials(mtl, path);
                                        ExportModel(ref mesh, asMdl.Model, path, asMdl.ModelFilePath);
                                    }
                                    break;
                                case Lgb.LgbEntryType.Gimmick:
                                    validEntry = true;
                                    var asGim = part as Lgb.LgbGimmickEntry;
                                    ExportSgbFile(asGim.Gimmick, asGim.Header.Translation, asGim.Header.Rotation, asGim.Header.Scale);
                                    break;
                                case Lgb.LgbEntryType.EventObject:
                                    validEntry = true;
                                    var asEobj = part as Lgb.LgbEventObjectEntry;
                                    ExportSgbFile(asEobj.Gimmick, asEobj.Header.Translation, asEobj.Header.Rotation, asEobj.Header.Scale);
                                    break;
                            }
                            if (newGroup && validEntry) {
                                vertStr.Add($"o {lgbGroup.Name}_{i}");
                                newGroup = false;
                            }
                        }
                    }
                }
                System.IO.File.AppendAllLines(fileName, vertStr);
                vertStr.Clear();
                System.Windows.Forms.MessageBox.Show("Finished exporting " + fileName, "", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Windows.Forms.MessageBox.Show(e.Message, "Unable to export " + territory.Name);
            }
        }
        #endregion

        public override void LoadContent() {
            _TerrainContainer.LoadContent();
            _LgbPartsContainer.LoadContent();
            base.LoadContent();
        }
        public override void UnloadContent() {
            _TerrainContainer.UnloadContent();
            _LgbPartsContainer.UnloadContent();
            base.UnloadContent();
        }
        public override void Update(EngineTime engineTime) {
            _TerrainContainer.Update(engineTime);
            _LgbPartsContainer.Update(engineTime);
            base.Update(engineTime);
        }
        public override void Draw(EngineTime time, ref SharpDX.Matrix world, ref SharpDX.Matrix view, ref SharpDX.Matrix projection) {
            _TerrainContainer.Draw(time, ref world, ref view, ref projection);
            _LgbPartsContainer.Draw(time, ref world, ref view, ref projection);
        }
    }
}
