using UnityEngine;
using TMPro;

public class BoothController : MonoBehaviour
{
    [Header("Settings")]
    public string namaBooth;
    
    // Variabel gambar sesuai request kamu
    public Texture2D banner_big; 
    public Texture2D banner_1; 
    public Texture2D banner_2; 

    [Header("References")]
    public TMP_Text textDisplay; 
    
    // Variabel renderer tempat nempel gambar
    public Renderer mediaBanner_big; 
    public Renderer mediaBanner_1; 
    public Renderer mediaBanner_2;

    // Property Block untuk handle memory biar ga leak
    private MaterialPropertyBlock propBlock;

    void OnValidate()
    {
        UpdateBooth();
    }

    void Start()
    {
        UpdateBooth();
    }

    public void UpdateBooth()
    {
        // 1. Update Teks
        if (textDisplay != null)
        {
            textDisplay.text = namaBooth;
        }

        // 2. Update Gambar - Kita panggil fungsi bantuan di bawah
        // Formatnya: (Renderer-nya, Gambar-nya)
        ApplyTexture(mediaBanner_big, banner_big);
        ApplyTexture(mediaBanner_1, banner_1);
        ApplyTexture(mediaBanner_2, banner_2);
    }

    // --- FUNGSI BANTUAN (Helper) ---
    // Fungsi ini tugasnya nempel gambar ke renderer pakai PropertyBlock
    void ApplyTexture(Renderer targetRenderer, Texture2D targetTexture)
    {
        // Cek dulu, kalau renderer atau gambarnya kosong, jangan diproses biar ga error
        if (targetRenderer == null || targetTexture == null) return;

        // Inisialisasi Property Block kalau belum ada
        if (propBlock == null)
            propBlock = new MaterialPropertyBlock();

        // Ambil property yang sedang aktif di object itu
        targetRenderer.GetPropertyBlock(propBlock);

        // Set Texture baru
        // PENTING: Jika pake URP, ganti "_MainTex" jadi "_BaseMap"
        propBlock.SetTexture("_BaseMap", targetTexture);

        // Tempel balik ke object
        targetRenderer.SetPropertyBlock(propBlock);
    }
}