using UnityEngine;

// Pasang pada titik tujuan teleport. Kosongkan jalur untuk kembali ke fisika Jasmine.
public sealed class TujuanJalurKampus : MonoBehaviour
{
    public JalurKampus jalur;
    [Tooltip("Setelah fade terbuka, kamera bergeser dan mendekat perlahan ke bingkai tujuan. Hanya untuk akses yang dipilih.")]
    public bool kameraMasukSinematik;
    [Min(.2f)] public float durasiKameraMasuk = 1.1f;
    [Tooltip("Geser awal relatif kamera. Nilai negatif dari kiri, positif dari kanan.")]
    public float geserAwalKamera = .8f;
}
