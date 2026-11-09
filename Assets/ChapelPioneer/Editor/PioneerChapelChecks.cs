// PANDUAN PROYEK - Paket Chapel / ChapelPioneer
// Fungsi: Memeriksa ukuran, susunan, akses, mesh, material, dan pratinjau revisi aktif.
// Panduan: Assets/ChapelPioneer/BACA DULU.md

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class PioneerChapelChecks
{
    const string Root = "Assets/ChapelPioneer";
    [Serializable] class Manifest
    {
        public int revision, parts, triangles, pews, rows, balcony_pews, balcony_stair_steps_per_side;
        public float[] estimated_dimensions_m;
        public Window[] windows;
        public ColliderData[] colliders;
        public StairDesign stair_design;
        public Balcony balcony;
        public Pillars pillars;
        public RearDoors rear_doors;
        public PulpitSideDoors pulpit_side_doors;
        public SideAccess stage_access;
        public PodiumStair podium_stair;
        public FlowerArrangements flower_arrangements;
        public PulpitWoodChairs pulpit_wood_chairs;
        public RoofIntegrity roof_integrity;
        public RearFoyer rear_foyer;
        public ExteriorAccess exterior_access;
        public string[] static_mesh_colliders;
        public ScreenSet sanctuary_screens;
        public ProjectionLayout projection_layout;
        public SeatingArea seating_area;
        public FoyerClearances foyer_clearances;
        public bool upper_side_windows_removed, rear_dark_screen_removed, red_chairs_removed;
    }
    [Serializable] class Window { public string name; public bool wall_opening; }
    [Serializable] class ColliderData { public string name; public float[] center, size; }
    [Serializable] class StairDesign { public int routes, upper_exit_openings; public string configuration; public string[] stages; public bool enclosed; }
    [Serializable] class Balcony { public float level_m, clear_front_walkway_m; public int seating_rows, wooden_pews, side_stair_bays; }
    [Serializable] class Pillars { public int count, rows; public float height_m; public bool terminate_at_green_mark; }
    [Serializable] class RearDoors { public int separate_entry_exit_areas; public string type; public bool center_double_door; }
    [Serializable] class PulpitSideDoors { public int count; public string purpose, top; }
    [Serializable] class SideAccess { public int front_facing_aisle_doors, turned_stage_doors; public string path; }
    [Serializable] class PodiumStair { public int count; public float width_m, tread_m, rise_m; public string position, material; }
    [Serializable] class FlowerArrangements { public int count; public float length_each_m,total_length_m; public string position, type; }
    [Serializable] class PulpitWoodChairs { public int count; public string placement, material; }
    [Serializable] class RoofIntegrity { public bool closed_slabs,end_infill_closed; public float nave_eave_m,wing_eave_m,nave_ridge_m; public int front_arched_windows,rear_arched_windows; }
    [Serializable] class RearFoyer { public int inner_doors,outer_doors; public bool clear_circulation; public float ground_glazed_room_max_y_m; }
    [Serializable] class ExteriorAccess { public int rear_facade,rear_side_exits,rear_total,sanctuary_side_exits; }
    [Serializable] class ScreenSet { public string center, left, right; }
    [Serializable] class ProjectionLayout { public int count; public string position; }
    [Serializable] class SeatingArea { public float[] bounds_m; public float center_aisle_width_m, front_clear_zone_from_z_m; }
    [Serializable] class FoyerClearances { public float rear_cross_lobby_m,central_link_m,hall_door_width_m; }

    static readonly List<string> results = new List<string>();
    static void Check(bool value, string message) => results.Add((value ? "OK: " : "FAIL: ") + message);

    public static string Validate()
    {
        results.Clear();
        var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(Root + "/PioneerChapel_Data.json"));
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/PioneerChapel_Interior.prefab");
        Check(manifest != null && manifest.revision == 18, "Data paket berisi revisi model terbaru");
        Check(prefab != null, "Prefab lengkap tersedia");
        if (prefab == null || manifest == null) return Finish();

        var filters = prefab.GetComponentsInChildren<MeshFilter>(true);
        var renderers = prefab.GetComponentsInChildren<MeshRenderer>(true);
        var colliders = prefab.GetComponentsInChildren<BoxCollider>(true);
        Check(filters.Length == manifest.parts, "Jumlah bagian mesh cocok dengan manifest");
        Check(filters.Sum(f => f.sharedMesh != null ? f.sharedMesh.triangles.Length / 3 : 0) == manifest.triangles, "Jumlah segitiga cocok dengan manifest");
        Check(filters.All(f => f.sharedMesh != null && f.sharedMesh.vertexCount > 0 && f.sharedMesh.subMeshCount > 0), "Semua bagian model memiliki mesh berpermukaan");
        Check(prefab.transform.localScale == Vector3.one, "Prefab memakai skala 1:1");

        var materials = renderers.SelectMany(r => r.sharedMaterials).Where(m => m != null).Distinct().ToArray();
        Check(materials.Length >= 17, "Material chapel tersambung ke bagian model");
        Check(materials.All(m => m.mainTexture != null && m.shader != null && m.shader.isSupported), "Shader dan tekstur material tersedia");
        Check(materials.All(m => m.mainTexture.width == 512 && m.mainTexture.height == 512), "Tekstur model berukuran 512 x 512");
        foreach (var material in materials)
        {
            var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(material.mainTexture)) as TextureImporter;
            var android = importer != null ? importer.GetPlatformTextureSettings("Android") : new TextureImporterPlatformSettings();
            Check(importer != null && importer.mipmapEnabled && importer.wrapMode == TextureWrapMode.Repeat && android.overridden && android.format == TextureImporterFormat.ASTC_6x6,
                "Mipmaps dan kompresi Android ASTC: " + material.name);
        }

        var room = new Bounds(Vector3.zero, Vector3.zero);
        foreach (var filter in filters) if (filter.sharedMesh != null) room.Encapsulate(filter.sharedMesh.bounds);
        Check(manifest.estimated_dimensions_m != null && manifest.estimated_dimensions_m.Length == 3 &&
              Mathf.Abs(manifest.estimated_dimensions_m[0] - 41.6f) < .1f && Mathf.Abs(manifest.estimated_dimensions_m[2] - 72f) < .1f &&
              room.size.x > 41.5f && room.size.z > 71.9f && room.max.y > 23.2f,
              "Aula lebar sekitar 41,6 m, panjang 72 m, dan bubungan utama lebih dari 23 m");

        var naveRoof = Find(filters, "05_Langit_Langit");
        var wingRoof = Find(filters, "17_Atap_Rendah_DuaSayap");
        Check(naveRoof != null && naveRoof.sharedMesh.bounds.max.y > 23.4f && naveRoof.sharedMesh.bounds.size.z > 71.9f,
            "Atap nave tengah membentuk bubungan tinggi yang menyambung sepanjang aula");
        Check(wingRoof != null && wingRoof.sharedMesh.bounds.max.y < 8.1f && wingRoof.sharedMesh.bounds.max.x > 20f &&
              manifest.roof_integrity != null && manifest.roof_integrity.closed_slabs && manifest.roof_integrity.end_infill_closed,
            "Atap sayap berupa panel tertutup pada kepala pilar, dan bidang ujung mengikuti kemiringan atap");
        var frontWall = Find(filters,"07_Dinding_Depan");var backWall = Find(filters,"08_Dinding_Belakang");
        Check(frontWall != null && backWall != null && frontWall.sharedMesh.bounds.max.y > 23.4f && backWall.sharedMesh.bounds.max.y > 23.4f &&
              !filters.Any(f=>f.name.StartsWith("15_Jendela_Depan")),
            "Dinding ujung menyentuh bubungan dan panel abu-abu lama di atas layar sudah dihapus");

        var pillars = Find(filters, "16_Pilar_Delapan_PerSisiAula");
        var pillarColliders = colliders.Where(c => c.name.StartsWith("Pilar nave ")).ToArray();
        Check(pillars != null && pillars.sharedMesh.bounds.max.y <= 7.6f && pillars.sharedMesh.bounds.min.x < -14f && pillars.sharedMesh.bounds.max.x > 14f,
            "Delapan pilar per sisi berhenti pada garis hijau, sekitar 7,5 m");
        Check(manifest.pillars != null && manifest.pillars.count == 16 && manifest.pillars.height_m <= 7.5f && manifest.pillars.terminate_at_green_mark &&
              pillarColliders.Length == 16 && pillarColliders.All(c => Mathf.Abs(c.size.y - 7.5f) < .02f && Mathf.Abs(Mathf.Abs(c.center.x)-15.65f)<.02f &&
                  pillarColliders.Any(d=>Mathf.Abs(d.center.x+c.center.x)<.02f && Mathf.Abs(d.center.z-c.center.z)<.02f)),
            "Tepat delapan pilar pada masing-masing sisi berada di luar ujung bangku");

        var upperSide = Find(filters, "23_DindingAtas_SayapJendela");
        Check(manifest.upper_side_windows_removed && upperSide != null && upperSide.sharedMesh.bounds.min.y >= 7.4f && upperSide.sharedMesh.bounds.max.y <= 8.0f &&
              manifest.windows.Count(w => w.name.StartsWith("Jendela atas sayap")) == 0,
            "Pita dinding tinggi di sisi menjadi sambungan plafon kayu, tanpa kaca lantai atas");
        Check(manifest.windows.Count(w => w.name.StartsWith("Jendela sisi")) == 18 &&
              manifest.windows.Count(w => w.name.StartsWith("Jendela gable lengkung 07_")) == 3 &&
              manifest.windows.Count(w => w.name.StartsWith("Jendela gable lengkung 08_")) == 5 &&
              manifest.windows.Count(w => w.name.StartsWith("Oculus gable")) == 2 && manifest.windows.All(w => w.wall_opening),
            "Jendela berdaun berpetak mengikuti foto; bay pintu dan bagian tertutup tidak memakai kaca");

        var pewMeshes = filters.Where(f => f.name.StartsWith("10_Bangku_Blok")).ToArray();
        Check(manifest.pews == 80 && manifest.rows == 20 && pewMeshes.Length == 16 && pewMeshes.All(f => f.sharedMesh.bounds.size.y < 1.3f),
            "80 bangku kayu tersusun dalam 20 baris; empat baris dikembalikan sampai batas terbaru");
        var pewBounds = new Bounds(Vector3.zero, Vector3.zero);
        foreach (var filter in pewMeshes) pewBounds.Encapsulate(filter.sharedMesh.bounds);
        Check(manifest.seating_area != null && manifest.seating_area.bounds_m.Length == 4 && pewBounds.min.z >= -24.4f && pewBounds.max.z > 13.2f && pewBounds.max.z <= 13.4f &&
              manifest.seating_area.front_clear_zone_from_z_m == 13.4f,
            "Bangku mendekati batas merah terbaru dengan ruang bebas sekitar 6,4 m sebelum tangga");
        Check(pewMeshes.All(f => f.sharedMesh.bounds.max.x < -1.6f || f.sharedMesh.bounds.min.x > 1.6f) &&
              manifest.seating_area.center_aisle_width_m >= 3.4f,
            "Lorong tengah selebar sedikitnya 3,4 m tetap kosong dan dapat dilalui");
        Check(!manifest.red_chairs_removed && filters.Count(f => f.name.StartsWith("18_Kursi_Merah_Sisi_"))==72 &&
              colliders.Count(c => c.name.StartsWith("Kursi merah sisi "))==72 &&
              colliders.Where(c=>c.name.StartsWith("Kursi merah sisi ")).All(c=>Mathf.Abs(c.center.x)>17.4f),
            "72 kursi merah tersusun dua jejer per sisi; jejer dalam bertanda hijau dihapus");
        var sidePosts=Find(filters,"04_Kolom_dan_Lampu");
        Check(sidePosts!=null && sidePosts.sharedMesh.bounds.min.y>7.6f,
            "Tiang kecil, AC dan kisi AC di tengah sayap dihapus; hanya lampu plafon tersisa");
        Check(colliders.Count(c=>c.name.StartsWith("Pilar penyangga balkon "))==6 &&
              Find(filters,"31_EnamPilar_PenyanggaBalkon")!=null,
            "Enam pilar tambahan menopang balkon secara simetris");
        Check(Find(filters,"03_Jendela_Samping").sharedMesh.bounds.min.y<.04f,
            "Jendela sisi menjangkau lantai tanpa plint dinding bawah");

        var balcony = Find(filters, "14_Balkon_Belakang");
        var balconyPews = Find(filters, "19_Bangku_Panjang_Kayu_Balkon");
        Check(balcony != null && balcony.sharedMesh.bounds.min.y > 4.7f && balconyPews != null && balconyPews.sharedMesh.bounds.min.y >= 5.03f && manifest.balcony != null &&
              manifest.balcony.wooden_pews == 16 && manifest.balcony.seating_rows == 2 && manifest.balcony.clear_front_walkway_m >= 2.4f,
            "Balkon memiliki dua baris bangku kayu dan lorong depan yang lapang");

        var stairMesh = Find(filters, "20_Tangga_L_DuaAkses_Balkon");
        int stairColliders = colliders.Count(c => c.name.StartsWith("Tangga tertutup "));
        Check(stairMesh != null && stairMesh.sharedMesh.bounds.size.x > 40f && manifest.stair_design != null && !manifest.stair_design.enclosed &&
              manifest.stair_design.routes == 2 && manifest.stair_design.upper_exit_openings == 2 && manifest.stair_design.stages.Length == 3 &&
              manifest.stair_design.stages[0].Contains("landing") && manifest.stair_design.stages[1].Contains("24-step") &&
              manifest.stair_design.stages[2].Contains("landing") && stairColliders == 48,
            "Dua tangga sisi ringkas memiliki 24 anak tangga dan tidak menutup pintu keluar ujung");
        var podiumStairColliders = colliders.Where(c => c.name.StartsWith("Tangga mimbar tengah ")).ToArray();
        var orderedPodiumSteps = podiumStairColliders.OrderBy(c => int.Parse(c.name.Split(' ').Last())).ToArray();
        var podiumMesh = Find(filters, "11_Panggung_dan_Tangga");
        bool stairsRiseTowardStage = orderedPodiumSteps.Length == 4 &&
            Enumerable.Range(1, 3).All(i => orderedPodiumSteps[i].center[1] > orderedPodiumSteps[i - 1].center[1] &&
                                             orderedPodiumSteps[i].center[2] > orderedPodiumSteps[i - 1].center[2]);
        Check(podiumStairColliders.Length == 4 && podiumStairColliders.All(c => c.size.x <= 3.36f) && stairsRiseTowardStage &&
              podiumMesh != null && !podiumMesh.GetComponent<MeshRenderer>().sharedMaterials.Any(m => m.name == "Merah") &&
              manifest.podium_stair != null && manifest.podium_stair.count == 4 && manifest.podium_stair.position == "center only" &&
              manifest.podium_stair.material.Contains("stone") &&
              !colliders.Any(c => c.name.StartsWith("Tangga panggung ") || c.name.StartsWith("Tangga mimbar -")),
            "Empat undakan batu yang ringkas hanya berada di tengah mimbar, tanpa tepi merah");

        var rearDoors = colliders.Where(c => c.name.StartsWith("Pintu masuk belakang ")).ToArray();
        Check(manifest.rear_doors != null && manifest.rear_doors.separate_entry_exit_areas == 5 && manifest.rear_doors.center_double_door &&
              manifest.exterior_access!=null && manifest.exterior_access.rear_facade==3 && manifest.exterior_access.rear_side_exits==2 &&
              manifest.exterior_access.sanctuary_side_exits==2 && Find(filters,"33_PintuLuar_SampingDanMimbar")!=null,
            "Lima akses belakang: tiga fasad dan dua ujung samping; dua pintu area mimbar ditambahkan terpisah");
        Check(Find(filters, "25_PintuSampingMimbar_DuaAkses") != null && manifest.pulpit_side_doors != null &&
              manifest.pulpit_side_doors.count == 0 && manifest.pulpit_side_doors.top == "rectangular",
            "Akses samping panggung berupa dua bukaan persegi tanpa daun pintu");
        var approachDoors = colliders.Where(c => c.name.StartsWith("Pintu depan lorong mimbar ")).ToArray();
        var stageDoors = colliders.Where(c => c.name.StartsWith("Pintu akses atas mimbar ")).ToArray();
        Check(approachDoors.Length == 0 && stageDoors.Length == 0 && manifest.stage_access != null &&
              manifest.stage_access.front_facing_aisle_doors == 0 && manifest.stage_access.turned_stage_doors == 0 &&
              manifest.stage_access.path.Contains("U-shaped"),
            "Jalur U melewati samping blok layar persegi, tanpa lubang kecil atau pintu di muka depan");
        Check(filters.Count(f=>f.name.StartsWith("32_Speaker_"))==2,
            "Dua speaker berada di bawah layar proyektor kiri dan kanan");
        var flowers = Find(filters, "27_KaranganBunga_SampingTanggaMimbar");
        Check(flowers != null && manifest.flower_arrangements != null && manifest.flower_arrangements.count == 2 &&
              manifest.flower_arrangements.length_each_m >= 11.4f && flowers.sharedMesh.bounds.size.x > 27f &&
              flowers.sharedMesh.bounds.min.y >= .8f && flowers.sharedMesh.bounds.max.y < 1.9f &&
              flowers.sharedMesh.vertices.All(v=>Mathf.Abs(v.x)>1.8f),
            "Karangan bunga memanjang total 23 m pada dua sisi tepi podium dan tidak menutup tangga tengah");
        var pulpitFurniture = Find(filters, "12_Alat_Musik_dan_Kursi");
        var pulpitChairs = colliders.Where(c => c.name.StartsWith("Kursi kayu mimbar ")).ToArray();
        Check(manifest.pulpit_wood_chairs != null && manifest.pulpit_wood_chairs.count == 5 &&
              manifest.pulpit_wood_chairs.material == "wood" && pulpitChairs.Length == 5 &&
              pulpitChairs.All(c => Mathf.Abs(c.center[0]) <= 2.5f) && pulpitFurniture != null &&
              !pulpitFurniture.GetComponent<MeshRenderer>().sharedMaterials.Any(m => m.name == "Merah"),
            "Lima kursi panggung dipusatkan dan memakai kayu, bukan merah");

        var screens = Find(filters, "21_Layar_Mimbar_TengahHitam_DuaPutih");
        var screenMaterials = screens != null ? screens.GetComponent<MeshRenderer>().sharedMaterials : new Material[0];
        Check(manifest.sanctuary_screens != null && manifest.sanctuary_screens.center == "LayarHitam" &&
              manifest.sanctuary_screens.left == "LayarPutih" && manifest.sanctuary_screens.right == "LayarPutih" &&
              screenMaterials.Any(m => m.name == "LayarHitam") && screenMaterials.Any(m => m.name == "LayarPutih"),
            "Area mimbar memakai satu layar hitam tengah dan dua layar proyektor putih");
        var whiteVertices=new List<Vector3>();
        if(screens!=null)
        {
            for(int sub=0;sub<screenMaterials.Length;sub++)
                if(screenMaterials[sub].name=="LayarPutih")
                    whiteVertices.AddRange(screens.sharedMesh.GetTriangles(sub).Select(i=>screens.sharedMesh.vertices[i]));
        }
        var projectors=filters.Where(f=>f.name.StartsWith("30_Proyektor_")).ToArray();
        Check(whiteVertices.Count>0 && whiteVertices.All(v=>Mathf.Abs(v.x)>14.8f && v.z<21.2f) &&
              projectors.Length==2 && projectors.All(f=>f.sharedMesh.bounds.max.y>2.25f && f.sharedMesh.bounds.size.x<1.2f) &&
              manifest.projection_layout!=null && manifest.projection_layout.count==2,
            "Layar putih berada di bidang depan sayap di samping pintu luar; dua proyektor fisik berdiri pada tiang logam di depannya");
        var blackScreen = materials.FirstOrDefault(m => m.name == "LayarHitam");
        var whiteScreen = materials.FirstOrDefault(m => m.name == "LayarPutih");
        Check(blackScreen != null && whiteScreen != null && IsOpaque(blackScreen) && IsOpaque(whiteScreen),
            "Layar hitam dan putih memakai material solid, bukan kaca");
        var groundOffice=Find(filters,"22_Ruang_Berkaca_Gelap_Belakang");
        Check(manifest.rear_dark_screen_removed && groundOffice != null && groundOffice.sharedMesh.bounds.max.y < 4.81f &&
              manifest.rear_foyer != null && manifest.rear_foyer.clear_circulation && manifest.rear_foyer.inner_doors==3 &&
              manifest.rear_foyer.outer_doors==5 && !colliders.Any(c=>c.name.StartsWith("Pintu dalam foyer ")) &&
              Find(filters,"34_Foyer_PlaponKolomDanDetail")!=null,
            "Foyer lengkap dengan ruangan berkaca hitam menghadap aula, pintu servis, plafon kayu, lampu dan percabangan");
        Check(manifest.foyer_clearances!=null && manifest.foyer_clearances.rear_cross_lobby_m>=6 &&
              manifest.foyer_clearances.central_link_m>=4.7f && manifest.foyer_clearances.hall_door_width_m>=3.2f,
            "Foyer lebar lebih dari 6 m, jalur tengah lebih dari 4,7 m, dan pintu aula 3,2 m tanpa collider penutup bukaan");
        var exteriorDoors=Find(filters,"33_PintuLuar_SampingDanMimbar");
        Check(exteriorDoors!=null && !exteriorDoors.sharedMesh.vertices.Any(v=>v.z>19 && v.z<24),
            "Tidak ada kusen atau daun pintu luar pada bidang depan dekat layar; pintu luar berada setelah belokan");
        var balconySupports=Find(filters,"31_EnamPilar_PenyanggaBalkon");
        Check(balconySupports!=null && balconySupports.sharedMesh.bounds.max.y<=4.81f &&
              balconySupports.sharedMesh.bounds.size.x>=30f,
            "Kolom balkon ditutup balok menerus di bawah dek, tanpa kolom besar melayang di atas pagar");

        foreach (var glass in materials.Where(m => m.name == "Kaca" || m.name == "KacaTerang"))
            Check(!glass.HasProperty("_Surface") || glass.GetFloat("_Surface") == 1f, "Material kaca tetap transparan: " + glass.name);
        var meshColliders=prefab.GetComponentsInChildren<MeshCollider>(true);
        Check(colliders.Any(c=>c.name=="Lantai") && manifest.static_mesh_colliders!=null &&
              meshColliders.Length==manifest.static_mesh_colliders.Length && meshColliders.All(c=>c.sharedMesh!=null && !c.convex),
            "Collider mesh statis mengikuti bukaan dinding, lantai akses dan lubang tangga; collider bangku serta lantai tersedia");

        var view = prefab.GetComponent<PioneerChapelView>();
        Check(view != null && view.atap != null && view.atapSayap != null && view.balkon != null && view.dindingBelakang != null,
            "Cutaway mengaitkan atap utama, atap sayap, balkon, dan dinding belakang");
        var cutaway = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/PioneerChapel_Cutaway.prefab");
        Check(cutaway != null && cutaway.GetComponent<PioneerChapelView>() != null && cutaway.GetComponent<PioneerChapelView>().bukaAtap,
            "Prefab Cutaway tersedia dengan atap terbuka");
        return Finish();
    }

    static MeshFilter Find(MeshFilter[] filters, string name) => filters.FirstOrDefault(f => f.name == name);
    static bool IsOpaque(Material material) => !material.HasProperty("_Surface") || material.GetFloat("_Surface") == 0f;
    static string Finish()
    {
        string report = string.Join("\n", results);
        string path = "work/chapel-analysis/unity-checks.txt";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, (results.Any(r => r.StartsWith("FAIL:")) ? "FAIL\n" : "PASS\n") + report + "\n");
        if (results.Any(r => r.StartsWith("FAIL:"))) throw new InvalidOperationException("Chapel checks failed:\n" + report);
        return "PASS\n" + report + "\n";
    }

    public static void RenderAll()
    {
        Render("unity-lorong", new Vector3(0, 3, -5), new Vector3(0, 7.5f, 35), false);
        Render("unity-balkon", new Vector3(12, 6.6f, -31), new Vector3(0, 9, 30), false);
        Render("unity-belakang", new Vector3(0, 3, -8), new Vector3(0, 9, -35.8f), false);
        Render("unity-cutaway", new Vector3(62, 48, -72), new Vector3(0, 6, 1), true);
        Render("unity-jendela", new Vector3(13, 2.4f, -12), new Vector3(20.5f, 4, -4), false);
        Render("unity-jendela-panggung", new Vector3(-5, 2.4f, 22), new Vector3(-13.7f, 3, 26.2f), false);
        Render("unity-layar-mimbar", new Vector3(0, 4, 8), new Vector3(0, 2.7f, 29), false);
        Render("unity-sayap", new Vector3(13.5f, 4, -12), new Vector3(20.5f, 7.2f, -12), false);
        Render("unity-pintu-sisi-mimbar", new Vector3(19.0f, 2.0f, 15.0f), new Vector3(18.0f, 2.8f, 21.1f), false);
        Render("unity-akses-atas-podium", new Vector3(10.5f, 2.1f, 26.2f), new Vector3(14.62f, 2.1f, 26.2f), false);
        Render("unity-foyer", new Vector3(-12,2.1f,-33.4f),new Vector3(10,2.3f,-31.9f),false);
        Render("unity-tangga-tertutup", new Vector3(17.1f,2.1f,-27.8f),new Vector3(18.35f,3.4f,-33.0f),false);
        Render("unity-proyektor-kanan",new Vector3(13.8f,2.7f,9.4f),new Vector3(17.5f,3.0f,21.1f),false);
        Render("unity-proyektor-kiri",new Vector3(-13.8f,2.7f,9.4f),new Vector3(-17.5f,3.0f,21.1f),false);
        Render("unity-tiga-pintu",new Vector3(0,2.1f,-34.6f),new Vector3(0,2.4f,-25.5f),false);
        Render("unity-bawah-balkon",new Vector3(13,2.4f,-12),new Vector3(-4,2.7f,-24),false);
        Render("unity-lorong-U",new Vector3(19.3f,2.1f,20),new Vector3(19.3f,2.5f,27),false);
        Render("unity-foyer-cabang",new Vector3(14,1.8f,-25.7f),new Vector3(20.6f,1.8f,-26.4f),false);
        Render("unity-pintu-ujung",new Vector3(17.85f,1.8f,-21),new Vector3(17.85f,1.8f,-25.7f),false);
        Render("unity-pintu-mimbar",new Vector3(19.3f,1.8f,23.5f),new Vector3(20.5f,1.8f,26.4f),false);
        Render("unity-fasad",new Vector3(0,5,-53),new Vector3(0,6,-35.7f),false);
        Render("unity-kaca-hitam",new Vector3(7,2,-19),new Vector3(5.35f,2.1f,-25.3f),false);
        Render("unity-r13-bidang-tanpa-pintu",new Vector3(-5,2.4f,12),new Vector3(19,3.0f,23),false);
        Render("unity-r13-kolom-balkon",new Vector3(13,3,-10),new Vector3(-7,4.4f,-22),false);
        Render("unity-r13-lorong-tengah",new Vector3(0,1.8f,-33.8f),new Vector3(0,2.2f,-23),false);
        Render("unity-r13-jendela-detail",new Vector3(16.5f,3.5f,-1),new Vector3(20.4f,3.5f,2.4f),false);
    }

    static void Render(string name, Vector3 eye, Vector3 target, bool cutaway)
    {
        var preview = new PreviewRenderUtility();
        Texture2D image = null;
        try
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/PioneerChapel_" + (cutaway ? "Cutaway" : "Interior") + ".prefab");
            var go = UnityEngine.Object.Instantiate(prefab);
            preview.AddSingleGO(go);
            preview.camera.transform.position = eye;
            preview.camera.transform.LookAt(target);
            preview.camera.fieldOfView = cutaway ? 42 : name=="unity-tiga-pintu" ? 105 : 72;
            preview.camera.nearClipPlane = .08f;
            preview.camera.farClipPlane = 200;
            preview.camera.useOcclusionCulling = false;
            preview.camera.clearFlags = CameraClearFlags.SolidColor;
            preview.camera.backgroundColor = new Color(.72f, .81f, .84f);
            preview.ambientColor = new Color(.7f, .66f, .57f);
            preview.lights[0].intensity = .75f;
            preview.lights[0].color = new Color(1, .91f, .77f);
            preview.lights[0].transform.rotation = Quaternion.Euler(48, -28, 0);
            preview.lights[1].intensity = .34f;
            preview.lights[1].color = new Color(.73f, .81f, .87f);
            preview.lights[1].transform.rotation = Quaternion.Euler(30, 145, 0);
            preview.BeginStaticPreview(new Rect(0, 0, 1500, 1000));
            preview.Render(true);
            preview.Render(true);
            image = preview.EndStaticPreview();
            File.WriteAllBytes("work/chapel-analysis/" + name + ".png", image.EncodeToPNG());
        }
        finally
        {
            if (image != null) UnityEngine.Object.DestroyImmediate(image);
            preview.Cleanup();
        }
    }
}
