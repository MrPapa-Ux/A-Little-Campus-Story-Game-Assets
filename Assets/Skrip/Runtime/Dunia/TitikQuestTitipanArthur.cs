using System;
using UnityEngine;

[RequireComponent(typeof(AreaInteraksi), typeof(NpcYangBisaDiatur))]
public sealed class TitikQuestTitipanArthur : MonoBehaviour
{
    public QuestTitipanArthur quest;
    public bool titikBarang;
    public GameObject visualBarang;
    public BarisDialog[] sebelumTerima, sedangMencari, siapDiserahkan, sesudahSelesai;
    void OnEnable() { Segarkan(); }
    public void Segarkan()
    {
        if (quest == null) return;
        var a = GetComponent<AreaInteraksi>();
        var tahap = quest.Tahap;
        a.skenarioDialog = tahap == TahapTitipanArthur.BelumDiterima ? sebelumTerima :
            tahap == TahapTitipanArthur.CariBarang ? sedangMencari :
            tahap == TahapTitipanArthur.BawaKembali ? siapDiserahkan : sesudahSelesai;
        a.daftarOpsi = Array.Empty<OpsiInteraksi>();
        a.tampilkanTombolKeluar = true;
        if (!titikBarang) return;
        bool ada = tahap == TahapTitipanArthur.CariBarang;
        if (visualBarang != null) visualBarang.SetActive(ada);
        GetComponent<NpcYangBisaDiatur>().enabled = ada;
        var b = GetComponent<NPCBubbleTrigger>();
        if (b != null) { b.AturPenawaranJalur(false); b.enabled = ada; }
        a.enabled = ada;
    }
}
