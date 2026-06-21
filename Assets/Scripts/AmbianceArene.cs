using UnityEngine;

/// <summary>
/// Ambiance sonore de fond pour l'arène : foule (murmure) + tam-tams (rythme),
/// générée par synthèse audio directement en code (pas besoin de fichier audio).
///
/// Si tu as un vrai enregistrement (libre de droits) plus tard, glisse-le dans
/// "Clip Personnalise" dans l'Inspector : il remplacera automatiquement le son généré.
///
/// À placer sur un GameObject avec un AudioSource (ou utilise le menu
/// Tools > Dakar Arena > Ajouter l'ambiance sonore qui crée tout automatiquement).
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AmbianceArene : MonoBehaviour
{
    [Header("Remplace le son généré par un vrai enregistrement si tu en as un")]
    public AudioClip clipPersonnalise;

    [Header("Réglages du son généré par code (ignorés si un clip personnalisé est fourni)")]
    [Range(0f, 1f)] public float volumeFoule = 0.35f;
    [Range(0f, 1f)] public float volumeTamTam = 0.5f;
    public float tempoBPM = 100f;
    [Range(0f, 1f)] public float volumeGenerale = 0.5f;

    private AudioSource source;

    void Start()
    {
        source = GetComponent<AudioSource>();
        source.loop = true;
        source.spatialBlend = 0f; // ambiance 2D, pas un son positionnel dans l'espace
        source.volume = volumeGenerale;

        source.clip = clipPersonnalise != null ? clipPersonnalise : GenererAmbianceProcedurale();
        source.Play();
    }

    private AudioClip GenererAmbianceProcedurale()
    {
        const int sampleRate = 44100;
        const int nbBattementsParBoucle = 8; // 2 mesures de 4 temps

        float dureeBattement = 60f / Mathf.Max(tempoBPM, 1f);
        float dureeBoucle = dureeBattement * nbBattementsParBoucle;
        int nbEchantillons = Mathf.CeilToInt(dureeBoucle * sampleRate);

        float[] data = new float[nbEchantillons];

        // --- Couche 1 : tam-tams, rythme simple façon clave (en unités de "temps") ---
        float[] tempsDesCoups = { 0f, 1.5f, 2f, 3f, 4f, 5.5f, 6f, 7f };
        foreach (float t in tempsDesCoups)
            AjouterCoupTamTam(data, sampleRate, t * dureeBattement, volumeTamTam);

        // --- Couche 2 : murmure de foule (bruit blanc filtré passe-bas pour adoucir le souffle) ---
        float etatFiltre = 0f;
        const float coefficientFiltre = 0.97f; // proche de 1 = plus grave/sourd, plus réaliste qu'un bruit blanc brut
        System.Random rng = new System.Random(12345); // graine fixe : même ambiance à chaque lancement

        for (int i = 0; i < nbEchantillons; i++)
        {
            float bruit = (float)(rng.NextDouble() * 2.0 - 1.0);
            etatFiltre = etatFiltre * coefficientFiltre + bruit * (1f - coefficientFiltre);
            data[i] += etatFiltre * volumeFoule;
        }

        // Sécurité anti-saturation (évite les craquements si les deux couches s'additionnent trop fort)
        for (int i = 0; i < nbEchantillons; i++)
            data[i] = Mathf.Clamp(data[i], -1f, 1f);

        AudioClip clip = AudioClip.Create("AmbianceProcedurale", nbEchantillons, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private void AjouterCoupTamTam(float[] data, int sampleRate, float tStart, float volume)
    {
        const float frequence = 95f;   // grave, façon corps de tam-tam
        const float duree = 0.22f;     // durée du "thump"

        int indexDepart = Mathf.RoundToInt(tStart * sampleRate);
        int nbEchantillonsCoup = Mathf.RoundToInt(duree * sampleRate);

        for (int i = 0; i < nbEchantillonsCoup; i++)
        {
            int index = indexDepart + i;
            if (index < 0 || index >= data.Length) continue;

            float t = i / (float)sampleRate;
            float enveloppe = Mathf.Exp(-t * 18f); // décroissance rapide façon percussion
            float onde = Mathf.Sin(2f * Mathf.PI * frequence * t);
            data[index] += onde * enveloppe * volume;
        }
    }
}