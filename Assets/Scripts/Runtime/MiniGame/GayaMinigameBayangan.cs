using System;
using TMPro;
using UnityEngine;

public sealed class GayaMinigameBayangan : ScriptableObject
{
    [Serializable]
    public struct Bagian
    {
        public Sprite sprite;
        public Vector2 posisi, ukuran;
        public float sudut;
        public Color warna;
    }

    [Serializable]
    public struct Pose
    {
        public Bagian[] bagian;
    }

    public TMP_FontAsset font;
    public Sprite kertas, kiri, kanan, aksi;
    public Texture2D sabit;
    public Pose diam, kepala;
    public Pose[] lari;
    public float durasiLari;
    public int tanganKanan, badan;
    public int versi;
}
