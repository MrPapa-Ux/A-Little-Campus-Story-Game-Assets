// PANDUAN PROYEK - Runtime / EfekUI
// Fungsi: Memutar daftar sprite sebagai animasi pada komponen UI Image; bukan pengendali gerak Arthur.
// Panduan: Assets/Scripts/Runtime/EfekUI/README.md

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FramePlayer : MonoBehaviour
{
    public Image targetImage;
    public List<Sprite> frames;
    public float framesPerSecond = 15f;
    public bool loop = true;

    private int index = 0;
    private float timer = 0f;

    void Update()
    {
        if (frames.Count == 0 || targetImage == null) return;

        timer += Time.deltaTime;

        if (timer >= 1f / framesPerSecond)
        {
            timer = 0f;
            targetImage.sprite = frames[index];
            index++;

            if (index >= frames.Count)
            {
                if (loop) index = 0;
                else index = frames.Count - 1;
            }
        }
    }
}