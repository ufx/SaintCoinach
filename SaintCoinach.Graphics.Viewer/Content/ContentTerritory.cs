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
                            var transformation = Matrix.Scaling(asEobj.Header.Scale.ToDx())
                                    * Matrix.RotationX(asEobj.Header.Rotation.X)
                                    * Matrix.RotationY(asEobj.Header.Rotation.Y)
                                    * Matrix.RotationZ(asEobj.Header.Rotation.Z)
                                    * Matrix.Translation(asEobj.Header.Translation.ToDx());

                            _LgbPartsContainer.Add(new ContentSgb(engine, asEobj.Gimmick) {
                                Transformation = transformation
                            });
                            foreach (var rootGimGroup in asEobj.Gimmick.Data.OfType<Sgb.SgbGroup>()) {
                                foreach (var sgb1CEntry in rootGimGroup.Entries.OfType<Sgb.SgbGroup1CEntry>()) {
                                    var rootGimEntry = sgb1CEntry;
                                    if (rootGimEntry.Gimmick != null) {
                                        _LgbPartsContainer.Add(new ContentSgb(engine, sgb1CEntry.Gimmick) {
                                            Transformation = transformation
                                        });
                                        foreach (var subGimGroup in rootGimEntry.Gimmick.Data.OfType<Sgb.SgbGroup>()) {
                                            foreach (var subGimEntry in subGimGroup.Entries.OfType<Sgb.SgbGimmickEntry>()) {
                                                _LgbPartsContainer.Add(new ContentSgb(engine, subGimEntry.Gimmick) {
                                                    Transformation = transformation
                                                });
                                            }
                                        }
                                    }
                                }
                            }
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

                int lights = 0;
                List<string> lightStrs = new List<string>() { "import bpy" };
                List<string> vertStr = new List<string>();
                List<string> faceStr = new List<string>();
                Dictionary<string, bool> exportedPaths = new Dictionary<string, bool>();
                UInt64 vs = 1, vt = 1, vn = 1;
                UInt64 i = 0;
                Matrix IdentityMatrix = Matrix.Identity;

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

                Matrix CreateMatrix(SaintCoinach.Graphics.Vector3 translation, SaintCoinach.Graphics.Vector3 rotation, SaintCoinach.Graphics.Vector3 scale) {
                    return (Matrix.Scaling(scale.ToDx())
                        * Matrix.RotationX(rotation.X)
                        * Matrix.RotationY(rotation.Y)
                        * Matrix.RotationZ(rotation.Z)
                        * Matrix.Translation(translation.ToDx()));
                }

                void ExportModel(ref Mesh mesh, ref Matrix lgbTransform, ref string materialName, ref string modelFilePath,
                    ref Matrix rootGimTransform, ref Matrix currGimTransform, ref Matrix modelTransform) {
                    i++;

                    var k = 0;
                    UInt64 tempVs = 0, tempVn = 0, tempVt = 0;
                    foreach (var v in mesh.Vertices) {

                        var x = v.Position.Value.X;
                        var y = v.Position.Value.Y;
                        var z = v.Position.Value.Z;
                        var w = v.Position.Value.W;

                        var transform = (modelTransform * rootGimTransform * currGimTransform) * lgbTransform;

                        var t = Matrix.Translation(x, y, z) * transform;
                        x = t.TranslationVector.X;
                        y = t.TranslationVector.Y;
                        z = t.TranslationVector.Z;

                        vertStr.Add($"v {x} {y} {z} {v.Position.Value.W}");
                        tempVs++;

                        vertStr.Add($"vn {v.Normal.Value.X} {v.Normal.Value.Y} {v.Normal.Value.Z}");
                        tempVn++;

                        if (v.UV != null) {
                            vertStr.Add($"vt {v.UV.Value.X} {v.UV.Value.Y} {v.UV.Value.Z} {v.UV.Value.W}");
                            tempVt++;
                        }
                    }
                    vertStr.Add($"g {modelFilePath}_{i.ToString()}_{k.ToString()}");
                    vertStr.Add($"usemtl {materialName}");
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

                Dictionary<string, bool> exportedSgbFiles = new Dictionary<string, bool>();
                void ExportSgbModels(Sgb.SgbFile sgbFile, ref Matrix lgbTransform, ref Matrix rootGimTransform, ref Matrix currGimTransform) {


                    foreach (var sgbGroup in sgbFile.Data.OfType<Sgb.SgbGroup>()) {
                        bool newGroup = true;
                        foreach (var mdl in sgbGroup.Entries.OfType<Sgb.SgbModelEntry>()) {
                            if (newGroup) {
                                vertStr.Add($"o {sgbFile.File.Path}_{sgbGroup.Name}_{i}");
                                newGroup = false;
                            }
                            var hq = mdl.Model.Model.GetModel(ModelQuality.High);
                            var filePath = mdl.ModelFilePath;
                            var modelTransform = CreateMatrix(mdl.Header.Translation, mdl.Header.Rotation, mdl.Header.Scale);

                            for (var j = 0; j < hq.Meshes.Length; ++j) {
                                var mesh = hq.Meshes[j];
                                var mtl = mesh.Material.Get();
                                var path = mtl.File.Path.Replace('/', '_').Replace(".mtrl", ".tex");

                                ExportMaterials(mtl, path);
                                ExportModel(ref mesh, ref lgbTransform, ref path, ref filePath, ref rootGimTransform, ref currGimTransform, ref modelTransform);
                            }
                        }

                        foreach (var light in sgbGroup.Entries.OfType<Sgb.SgbLightEntry>()) {
                            var pos = light.Header.Translation;
                            var transform = (Matrix.Translation(pos.X, pos.Y, pos.Z) * (rootGimTransform * currGimTransform) * lgbTransform).TranslationVector;
                            pos.X = transform.X;
                            pos.Y = transform.Y;
                            pos.Z = transform.Z;

                            lightStrs.Add($"#LIGHT_{lights++}_{light.Name}");
                            lightStrs.Add($"#pos {pos.X} {pos.Y} {pos.Z}");
                            lightStrs.Add($"#UNKNOWN {light.Header.Rotation.X} {light.Header.Rotation.Y} {light.Header.Rotation.Z}");
                            lightStrs.Add($"#UNKNOWN2 {light.Header.Scale.X} {light.Header.Scale.Y} {light.Header.Scale.Z}");
                            lightStrs.Add($"#unk {light.Header.Entry1.X} {light.Header.Entry1.Y}");
                            lightStrs.Add($"#unk2 {light.Header.Entry2.X} {light.Header.Entry2.Y}");
                            lightStrs.Add($"#unk3 {light.Header.Entry3.X} {light.Header.Entry3.Y}");
                            lightStrs.Add($"#unk4 {light.Header.Entry4.X} {light.Header.Entry4.Y}");
                            lightStrs.Add("");
                        }
                    }
                }

                if (territory.Terrain != null) {
                    foreach (var part in territory.Terrain.Parts) {
                        var hq = part.Model.GetModel(ModelQuality.High);
                        var filePath = hq.Definition.File.Path;
                        var lgbTransform = CreateMatrix(part.Translation, part.Rotation, part.Scale);

                        for (var j = 0; j < hq.Meshes.Length; ++j) {
                            var mesh = hq.Meshes[j];
                            var mtl = mesh.Material.Get();
                            var path = mtl.File.Path.Replace('/', '_').Replace(".mtrl", ".tex");

                            ExportMaterials(mtl, path);
                            ExportModel(ref mesh, ref lgbTransform, ref path, ref filePath, ref IdentityMatrix, ref IdentityMatrix, ref IdentityMatrix);
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
                            /*
                            if (part.Type != Lgb.LgbEntryType.Light)
                                continue;
                            //*/
                            switch (part.Type) {
                                case Lgb.LgbEntryType.Model:
                                    validEntry = true;

                                    var asMdl = part as Lgb.LgbModelEntry;
                                    var hq = asMdl.Model.Model.GetModel(ModelQuality.High);
                                    var lgbTransform = CreateMatrix(asMdl.Header.Translation, asMdl.Header.Rotation, asMdl.Header.Scale);
                                    var filePath = asMdl.ModelFilePath;

                                    for (var j = 0; j < hq.Meshes.Length; ++j) {
                                        var mesh = hq.Meshes[j];
                                        var mtl = mesh.Material.Get();
                                        var path = mtl.File.Path.Replace('/', '_').Replace(".mtrl", ".tex");

                                        ExportMaterials(mtl, path);
                                        ExportModel(ref mesh, ref lgbTransform, ref path, ref filePath, ref IdentityMatrix, ref IdentityMatrix, ref IdentityMatrix);
                                    }
                                    break;
                                case Lgb.LgbEntryType.Gimmick:
                                    validEntry = true;
                                    var asGim = part as Lgb.LgbGimmickEntry;
                                    if (asGim.Gimmick == null)
                                        continue;

                                    lgbTransform = CreateMatrix(asGim.Header.Translation, asGim.Header.Rotation, asGim.Header.Scale);

                                    ExportSgbModels(asGim.Gimmick, ref lgbTransform, ref IdentityMatrix, ref IdentityMatrix);
                                    foreach (var rootGimGroup in asGim.Gimmick.Data.OfType<Sgb.SgbGroup>()) {
                                        foreach (var rootGimEntry in rootGimGroup.Entries.OfType<Sgb.SgbGimmickEntry>()) {
                                            if (rootGimEntry.Gimmick != null) {
                                                var rootGimTransform = CreateMatrix(rootGimEntry.Header.Translation, rootGimEntry.Header.Rotation, rootGimEntry.Header.Scale);
                                                ExportSgbModels(rootGimEntry.Gimmick, ref lgbTransform, ref rootGimTransform, ref IdentityMatrix);
                                                foreach (var subGimGroup in rootGimEntry.Gimmick.Data.OfType<Sgb.SgbGroup>()) {
                                                    foreach (var subGimEntry in subGimGroup.Entries.OfType<Sgb.SgbGimmickEntry>()) {
                                                        var subGimTransform = CreateMatrix(subGimEntry.Header.Translation, subGimEntry.Header.Rotation, subGimEntry.Header.Scale); 
                                                        ExportSgbModels(subGimEntry.Gimmick, ref lgbTransform, ref rootGimTransform, ref subGimTransform);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case Lgb.LgbEntryType.EventObject:
                                    validEntry = true;
                                    var asEobj = part as Lgb.LgbEventObjectEntry;
                                    if (asEobj.Gimmick == null)
                                        continue;

                                    lgbTransform = CreateMatrix(asEobj.Header.Translation, asEobj.Header.Rotation, asEobj.Header.Scale);

                                    ExportSgbModels(asEobj.Gimmick, ref lgbTransform, ref IdentityMatrix, ref IdentityMatrix);
                                    foreach (var rootGimGroup in asEobj.Gimmick.Data.OfType<Sgb.SgbGroup>()) {
                                        foreach (var rootGimEntry in rootGimGroup.Entries.OfType<Sgb.SgbGimmickEntry>()) {
                                            if (rootGimEntry.Gimmick != null) {
                                                var rootGimTransform = CreateMatrix(rootGimEntry.Header.Translation, rootGimEntry.Header.Rotation, rootGimEntry.Header.Scale);
                                                ExportSgbModels(rootGimEntry.Gimmick, ref lgbTransform, ref rootGimTransform, ref IdentityMatrix);
                                                foreach (var subGimGroup in rootGimEntry.Gimmick.Data.OfType<Sgb.SgbGroup>()) {
                                                    foreach (var subGimEntry in subGimGroup.Entries.OfType<Sgb.SgbGimmickEntry>()) {
                                                        var subGimTransform = CreateMatrix(subGimEntry.Header.Translation, subGimEntry.Header.Rotation, subGimEntry.Header.Scale);
                                                        ExportSgbModels(subGimEntry.Gimmick, ref lgbTransform, ref rootGimTransform, ref subGimTransform);
                                                    }
                                                }
                                            }
                                        }
                                        foreach (var sgb1CEntry in rootGimGroup.Entries.OfType<Sgb.SgbGroup1CEntry>()) {
                                            if (sgb1CEntry.Gimmick != null) {
                                                ExportSgbModels(sgb1CEntry.Gimmick, ref lgbTransform, ref IdentityMatrix, ref IdentityMatrix);
                                                foreach (var subGimGroup in sgb1CEntry.Gimmick.Data.OfType<Sgb.SgbGroup>()) {
                                                    foreach (var subGimEntry in subGimGroup.Entries.OfType<Sgb.SgbGimmickEntry>()) {
                                                        var subGimTransform = CreateMatrix(subGimEntry.Header.Translation, subGimEntry.Header.Rotation, subGimEntry.Header.Scale);
                                                        ExportSgbModels(subGimEntry.Gimmick, ref lgbTransform, ref IdentityMatrix, ref subGimTransform);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case Lgb.LgbEntryType.Light:
                                    //validEntry = true;
                                    var asLight = part as Lgb.LgbLightEntry;
                                    lightStrs.Add($"#LIGHT_{lights++}_{asLight.Name}");
                                    lightStrs.Add($"#pos {asLight.Header.Translation.X} {asLight.Header.Translation.Y} {asLight.Header.Translation.Z}");
                                    lightStrs.Add($"#UNKNOWN {asLight.Header.Rotation.X} {asLight.Header.Rotation.Y} {asLight.Header.Rotation.Z}");
                                    lightStrs.Add($"#UNKNOWN2 {asLight.Header.Scale.X} {asLight.Header.Scale.Y} {asLight.Header.Scale.Z}");
                                    lightStrs.Add($"#unk {asLight.Header.Entry1.X} {asLight.Header.Entry1.Y}");
                                    lightStrs.Add($"#unk2 {asLight.Header.Entry2.X} {asLight.Header.Entry2.Y}");
                                    lightStrs.Add($"#unk3 {asLight.Header.Entry3.X} {asLight.Header.Entry3.Y}");
                                    lightStrs.Add($"#unk4 {asLight.Header.Entry4.X} {asLight.Header.Entry4.Y}");
                                    lightStrs.Add("");
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
                System.IO.File.WriteAllLines(fileName + "_lights.txt", lightStrs);
                vertStr = null;
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
