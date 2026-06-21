#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;

/// <summary>
/// Génère et améliore le décor de l'arène (tam-tams, tissus, drapeaux, foule, portique, sol, lumière).
/// Menus : Tools > Dakar Arena > ...
/// </summary>
public static class ArenaDecorGenerator
{
    private const string RootName = "ArenaDecor";

    // --- Réglages ajustables ---
    private const int NbTissus = 8;
    private const int NbDrapeaux = 12;
    private const int NbTamTams = 4;
    private const float MargeAutourDuSol = 2.5f;

    private const int NbRangsFoule = 3;
    private const int NbPersonnesParRangBase = 24;
    private const float EspacementRangFoule = 1.3f;

    private const float AngleEntreeDeg = 0f;     // direction de l'entrée (0° = "vers l'est" du cercle)
    private const float LargeurEntreeDeg = 28f;  // largeur du passage dégagé dans la foule
    private const string NomArene = "ARENA SANDAGA";
    private const float AngleOfficielsDeg = 90f; // séparé du portique (0°) et du cluster de tam-tams (200°)

    [MenuItem("Tools/Dakar Arena/Générer le décor")]
    public static void GenererDecor()
    {
        GameObject ground = GameObject.Find("Ground");
        if (ground == null || ground.GetComponent<Renderer>() == null)
        {
            Debug.LogError("Objet 'Ground' introuvable (ou sans Renderer). Vérifie le nom exact dans la Hierarchy.");
            return;
        }

        SupprimerDecor();

        GameObject root = new GameObject(RootName);

        Bounds b = ground.GetComponent<Renderer>().bounds;
        float radius = Mathf.Max(b.extents.x, b.extents.z) + MargeAutourDuSol;
        Vector3 center = new Vector3(b.center.x, b.min.y, b.center.z);
        float demiTailleSol = Mathf.Min(b.extents.x, b.extents.z);

        CreerTamTams(root.transform, center, radius);
        CreerTissus(root.transform, center, radius);
        CreerDrapeaux(root.transform, center, radius);
        CreerFoule(root.transform, center, radius);
        CreerPortique(root.transform, center, RayonExterieurFoule(radius));
        CreerLigneRing(root.transform, center, demiTailleSol);
        CreerBancOfficiels(root.transform, center, demiTailleSol);

        Selection.activeGameObject = root;
        Debug.Log($"Décor généré autour de '{ground.name}' (rayon ≈ {radius:F1}m). " +
                  "Astuce : lance aussi 'Tools > Dakar Arena > Améliorer le sol et la lumière' pour une ambiance plus chaude.");
    }

    [MenuItem("Tools/Dakar Arena/Supprimer le décor")]
    public static void SupprimerDecor()
    {
        GameObject existing = GameObject.Find(RootName);
        if (existing != null) Object.DestroyImmediate(existing);
    }

    [MenuItem("Tools/Dakar Arena/Améliorer le sol et la lumière")]
    public static void AmeliorerSolEtLumiere()
    {
        GameObject ground = GameObject.Find("Ground");
        if (ground != null && ground.GetComponent<Renderer>() != null)
        {
            Renderer r = ground.GetComponent<Renderer>();
            Undo.RecordObject(r, "Changer matériau du sol");
            r.sharedMaterial = NouveauMateriau("Mat_Sol_Sable", new Color(0.75f, 0.62f, 0.42f), 0.12f);
        }
        else
        {
            Debug.LogWarning("Ground introuvable, sol non modifié.");
        }

        GameObject light = GameObject.Find("Directional Light");
        if (light != null)
        {
            Light l = light.GetComponent<Light>();
            if (l != null)
            {
                Undo.RecordObject(l, "Régler la lumière");
                l.color = new Color(1f, 0.85f, 0.65f);
                l.intensity = 1.3f;
            }
            Undo.RecordObject(light.transform, "Régler l'angle de la lumière");
            light.transform.rotation = Quaternion.Euler(38f, -30f, 0f);
        }
        else
        {
            Debug.LogWarning("'Directional Light' introuvable, lumière non modifiée.");
        }

        Debug.Log("Sol et lumière mis à jour (Ctrl+Z pour annuler si le résultat ne te plaît pas).");
    }

    [MenuItem("Tools/Dakar Arena/Ajouter l'ambiance sonore")]
    public static void AjouterAmbianceSonore()
    {
        GameObject existing = GameObject.Find("AmbianceSonore");
        if (existing != null)
        {
            Debug.Log("L'objet 'AmbianceSonore' existe déjà dans la scène.");
            Selection.activeGameObject = existing;
            return;
        }

        GameObject obj = new GameObject("AmbianceSonore");
        obj.AddComponent<AudioSource>();
        obj.AddComponent<AmbianceArene>();
        Selection.activeGameObject = obj;
        Debug.Log("Ambiance sonore ajoutée (foule + tam-tams générés par code). Lance le jeu (Play) pour l'entendre.");
    }

    private static float RayonExterieurFoule(float radius)
    {
        return (radius + 1.0f) + (NbRangsFoule - 1) * EspacementRangFoule;
    }

    // --- Tam-tams : corps en bois + peau claire sur le dessus ---
    private static void CreerTamTams(Transform parent, Vector3 center, float radius)
    {
        float angleCluster = 200f * Mathf.Deg2Rad;
        Vector3 clusterCenter = center + new Vector3(Mathf.Cos(angleCluster), 0, Mathf.Sin(angleCluster)) * radius;

        for (int i = 0; i < NbTamTams; i++)
        {
            float hauteur = Random.Range(0.7f, 1.3f);
            float rayonTam = Random.Range(0.22f, 0.35f);
            Vector2 jitter = Random.insideUnitCircle * 0.8f;
            Vector3 basePos = clusterCenter + new Vector3(jitter.x, 0, jitter.y);

            GameObject corps = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            corps.name = "TamTam_Corps_" + i;
            corps.transform.SetParent(parent);
            corps.transform.localScale = new Vector3(rayonTam * 2f, hauteur * 0.5f, rayonTam * 2f);
            corps.transform.position = basePos + Vector3.up * (hauteur * 0.5f);
            corps.GetComponent<Renderer>().sharedMaterial =
                NouveauMateriau("Mat_TamTam_Corps_" + i, new Color(0.32f, 0.18f, 0.08f), 0.2f);

            float rayonPeau = rayonTam * 1.15f;
            GameObject peau = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            peau.name = "TamTam_Peau_" + i;
            peau.transform.SetParent(parent);
            peau.transform.localScale = new Vector3(rayonPeau * 2f, 0.04f, rayonPeau * 2f);
            peau.transform.position = basePos + Vector3.up * (hauteur + 0.02f);
            peau.GetComponent<Renderer>().sharedMaterial =
                NouveauMateriau("Mat_TamTam_Peau_" + i, new Color(0.85f, 0.75f, 0.55f), 0.1f);
        }
    }

    // --- Tissus façon wax : fond coloré + bande contrastée ---
    private static void CreerTissus(Transform parent, Vector3 center, float radius)
    {
        Color[] couleursFond =
        {
            new Color(0.85f, 0.25f, 0.10f),
            new Color(0.10f, 0.45f, 0.30f),
            new Color(0.80f, 0.70f, 0.10f),
            new Color(0.55f, 0.10f, 0.35f),
        };
        Color[] couleursBande =
        {
            new Color(0.95f, 0.85f, 0.55f),
            new Color(0.95f, 0.85f, 0.55f),
            new Color(0.20f, 0.15f, 0.10f),
            new Color(0.95f, 0.85f, 0.55f),
        };

        for (int i = 0; i < NbTissus; i++)
        {
            float angle = (360f / NbTissus) * i * Mathf.Deg2Rad;
            Vector3 pos = center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;

            GameObject panneau = new GameObject("TissuPanel_" + i);
            panneau.transform.SetParent(parent);
            panneau.transform.position = pos + Vector3.up * 1f;
            panneau.transform.LookAt(new Vector3(center.x, panneau.transform.position.y, center.z));

            GameObject fond = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fond.name = "Fond";
            fond.transform.SetParent(panneau.transform);
            fond.transform.localPosition = Vector3.zero;
            fond.transform.localRotation = Quaternion.identity;
            fond.transform.localScale = new Vector3(2.2f, 2f, 0.08f);
            fond.GetComponent<Renderer>().sharedMaterial =
                NouveauMateriau("Mat_TissuFond_" + i, couleursFond[i % couleursFond.Length], 0.15f);

            GameObject bande = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bande.name = "Bande";
            bande.transform.SetParent(panneau.transform);
            bande.transform.localPosition = new Vector3(0f, -0.65f, 0.02f);
            bande.transform.localRotation = Quaternion.identity;
            bande.transform.localScale = new Vector3(2.3f, 0.4f, 0.1f);
            bande.GetComponent<Renderer>().sharedMaterial =
                NouveauMateriau("Mat_TissuBande_" + i, couleursBande[i % couleursBande.Length], 0.15f);
        }
    }

    // --- Drapeaux sur piquets, légèrement inclinés façon "vent" ---
    private static void CreerDrapeaux(Transform parent, Vector3 center, float radius)
    {
        Color[] couleurs = { Color.red, new Color(0.95f, 0.85f, 0.1f), new Color(0.1f, 0.55f, 0.25f), new Color(0.9f, 0.4f, 0f) };
        float rayonDrapeaux = radius + 0.5f;
        float hauteurPiquet = 2.2f;

        for (int i = 0; i < NbDrapeaux; i++)
        {
            float angle = (360f / NbDrapeaux) * i * Mathf.Deg2Rad;
            Vector3 basePos = center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * rayonDrapeaux;

            GameObject piquet = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            piquet.name = "Piquet_" + i;
            piquet.transform.SetParent(parent);
            piquet.transform.localScale = new Vector3(0.05f, hauteurPiquet * 0.5f, 0.05f);
            piquet.transform.position = basePos + Vector3.up * (hauteurPiquet * 0.5f);
            piquet.GetComponent<Renderer>().sharedMaterial =
                NouveauMateriau("Mat_Piquet_" + i, new Color(0.4f, 0.3f, 0.2f), 0.2f);

            GameObject drapeau = GameObject.CreatePrimitive(PrimitiveType.Cube);
            drapeau.name = "Drapeau_" + i;
            drapeau.transform.SetParent(parent);
            drapeau.transform.localScale = new Vector3(0.5f, 0.35f, 0.02f);
            drapeau.transform.position = basePos + Vector3.up * hauteurPiquet + Vector3.right * 0.25f;
            drapeau.transform.rotation = Quaternion.Euler(0f, Random.Range(-15f, 15f), Random.Range(-6f, 6f));
            drapeau.GetComponent<Renderer>().sharedMaterial =
                NouveauMateriau("Mat_Drapeau_" + i, couleurs[i % couleurs.Length], 0.2f);
        }
    }

    // --- Foule : plusieurs rangs de spectateurs simples, en gradins, avec un passage pour le portique ---
    private static void CreerFoule(Transform parent, Vector3 center, float radius)
    {
        Color[] vetements =
        {
            new Color(0.95f, 0.95f, 0.92f), // boubou blanc
            new Color(0.85f, 0.25f, 0.10f),
            new Color(0.10f, 0.45f, 0.30f),
            new Color(0.80f, 0.70f, 0.10f),
            new Color(0.55f, 0.10f, 0.35f),
            new Color(0.20f, 0.30f, 0.55f),
        };

        Material[] matsVetements = new Material[vetements.Length];
        for (int v = 0; v < vetements.Length; v++)
            matsVetements[v] = NouveauMateriau("Mat_Spectateur_" + v, vetements[v], 0.25f);

        GameObject foule = new GameObject("Foule");
        foule.transform.SetParent(parent);

        float rayonDepart = radius + 1.0f;

        for (int rang = 0; rang < NbRangsFoule; rang++)
        {
            float rayonRang = rayonDepart + rang * EspacementRangFoule;
            float hauteurRang = rang * 0.5f;
            int nbPersonnes = NbPersonnesParRangBase + rang * 6;

            for (int p = 0; p < nbPersonnes; p++)
            {
                float angleDegBase = (360f / nbPersonnes) * p;

                // On laisse un passage dégagé pour l'entrée / le portique
                if (Mathf.Abs(Mathf.DeltaAngle(angleDegBase, AngleEntreeDeg)) < LargeurEntreeDeg * 0.5f)
                    continue;

                float angle = (angleDegBase + Random.Range(-1.5f, 1.5f)) * Mathf.Deg2Rad;
                float rJitter = Random.Range(-0.15f, 0.15f);
                Vector3 posSol = center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * (rayonRang + rJitter);
                posSol.y = center.y + hauteurRang;

                float taille = Random.Range(0.85f, 1.05f);
                GameObject personne = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                personne.name = $"Spectateur_{rang}_{p}";
                personne.transform.SetParent(foule.transform);
                personne.transform.localScale = new Vector3(0.35f * taille, 0.85f * taille, 0.35f * taille);
                personne.transform.position = posSol + Vector3.up * (0.85f * taille);

                Collider col = personne.GetComponent<Collider>();
                if (col != null) Object.DestroyImmediate(col);

                personne.GetComponent<Renderer>().sharedMaterial =
                    matsVetements[Random.Range(0, matsVetements.Length)];
            }
        }
    }

    // --- Portique d'entrée avec le nom de l'arène en texte 3D ---
    private static void CreerPortique(Transform parent, Vector3 center, float radiusFouleExterieur)
    {
        float angleRad = AngleEntreeDeg * Mathf.Deg2Rad;
        Vector3 dirVersExterieur = new Vector3(Mathf.Cos(angleRad), 0, Mathf.Sin(angleRad));

        float rayonPortique = radiusFouleExterieur + 1.5f;
        Vector3 positionPortique = center + dirVersExterieur * rayonPortique;

        float largeurPortique = 3.5f;
        float hauteurPilier = 4f;

        GameObject portique = new GameObject("Portique");
        portique.transform.SetParent(parent);
        portique.transform.position = positionPortique;
        // +Z du portique pointe vers l'extérieur : le texte sera lisible en arrivant de l'extérieur
        portique.transform.rotation = Quaternion.LookRotation(dirVersExterieur);

        Material matBois = NouveauMateriau("Mat_Portique_Bois", new Color(0.45f, 0.30f, 0.15f), 0.2f);

        for (int cote = -1; cote <= 1; cote += 2)
        {
            GameObject pilier = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pilier.name = "Pilier_" + (cote < 0 ? "Gauche" : "Droit");
            pilier.transform.SetParent(portique.transform);
            pilier.transform.localPosition = new Vector3(cote * largeurPortique * 0.5f, hauteurPilier * 0.5f, 0f);
            pilier.transform.localScale = new Vector3(0.3f, hauteurPilier * 0.5f, 0.3f);
            pilier.GetComponent<Renderer>().sharedMaterial = matBois;
        }

        GameObject linteau = GameObject.CreatePrimitive(PrimitiveType.Cube);
        linteau.name = "Linteau";
        linteau.transform.SetParent(portique.transform);
        linteau.transform.localPosition = new Vector3(0f, hauteurPilier + 0.3f, 0f);
        linteau.transform.localScale = new Vector3(largeurPortique + 0.6f, 0.6f, 0.6f);
        linteau.GetComponent<Renderer>().sharedMaterial = matBois;

        // Texte du nom de l'arène (TextMeshPro 3D, pas un Canvas UI).
        // On en place DEUX, un sur chaque face du linteau (au lieu de deviner de quel
        // côté regarde la caméra) : comme ça il y en a toujours un de visible et bien
        // orienté, sans dépendre de l'angle de vue, et aucun n'est caché par le bois.
        void CreerTexte(float decalageZ, float rotationY, string nom)
        {
            GameObject texteObj = new GameObject(nom, typeof(RectTransform));
            texteObj.transform.SetParent(portique.transform, false);
            texteObj.transform.localPosition = new Vector3(0f, hauteurPilier + 0.3f, decalageZ);
            texteObj.transform.localRotation = Quaternion.Euler(0f, rotationY, 0f);

            TextMeshPro tmp = texteObj.AddComponent<TextMeshPro>();
            tmp.text = NomArene;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.95f, 0.85f, 0.55f);
            if (TMP_Settings.defaultFontAsset != null)
                tmp.font = TMP_Settings.defaultFontAsset;

            // Auto-size : le texte rétrécit pour toujours rentrer dans la largeur du portique,
            // au lieu de déborder largement comme avant (fontSize fixe trop grand)
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 0.3f;
            tmp.fontSizeMax = 3f;
            tmp.overflowMode = TextOverflowModes.Truncate;

            RectTransform rt = texteObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(largeurPortique - 0.2f, 1f);
        }

        // décalage de 0.4 (le linteau ne fait que 0.3 de demi-épaisseur) : le texte
        // dépasse toujours légèrement du bois, jamais caché derrière
        CreerTexte(0.4f, 0f, "Texte_NomArene_FaceExterieure");
        CreerTexte(-0.4f, 180f, "Texte_NomArene_FaceInterieure");
    }

    // --- Table + sièges des officiels, sur le sable, entre le cercle du ring et le bord du terrain ---
    private static void CreerBancOfficiels(Transform parent, Vector3 center, float demiTailleSol)
    {
        float angleRad = AngleOfficielsDeg * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Cos(angleRad), 0, Mathf.Sin(angleRad));
        float rayon = demiTailleSol * 0.92f; // sur le sable, juste à l'extérieur du cercle du ring (0.85)
        Vector3 basePos = center + dir * rayon;

        GameObject zone = new GameObject("BancOfficiels");
        zone.transform.SetParent(parent);
        zone.transform.position = basePos;
        // +Z de la zone pointe vers le centre : la table et les officiels font face au ring
        zone.transform.rotation = Quaternion.LookRotation(-dir);

        Material matBois = NouveauMateriau("Mat_Officiels_Bois", new Color(0.42f, 0.28f, 0.16f), 0.2f);
        Material matTable = NouveauMateriau("Mat_Officiels_Tissu", new Color(0.85f, 0.85f, 0.80f), 0.3f);

        const float zTable = 0.3f;

        GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
        table.name = "Table";
        table.transform.SetParent(zone.transform);
        table.transform.localPosition = new Vector3(0f, 0.4f, zTable);
        table.transform.localScale = new Vector3(2.2f, 0.08f, 0.8f);
        table.GetComponent<Renderer>().sharedMaterial = matTable;

        for (int x = -1; x <= 1; x += 2)
        {
            for (int z = -1; z <= 1; z += 2)
            {
                GameObject pied = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pied.name = "PiedTable";
                pied.transform.SetParent(zone.transform);
                pied.transform.localScale = new Vector3(0.05f, 0.2f, 0.05f);
                pied.transform.localPosition = new Vector3(x * 1.0f, 0.2f, zTable + z * 0.3f);
                pied.GetComponent<Renderer>().sharedMaterial = matBois;
            }
        }

        // 3 sièges, derrière la table (dos à la foule, face au ring)
        float zSiege = zTable - 0.55f;
        float zDossier = zSiege - 0.18f;
        for (int i = -1; i <= 1; i++)
        {
            float offsetX = i * 0.8f;

            GameObject assise = GameObject.CreatePrimitive(PrimitiveType.Cube);
            assise.name = "Siege_Assise_" + i;
            assise.transform.SetParent(zone.transform);
            assise.transform.localPosition = new Vector3(offsetX, 0.25f, zSiege);
            assise.transform.localScale = new Vector3(0.45f, 0.08f, 0.45f);
            assise.GetComponent<Renderer>().sharedMaterial = matBois;

            GameObject dossier = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dossier.name = "Siege_Dossier_" + i;
            dossier.transform.SetParent(zone.transform);
            dossier.transform.localPosition = new Vector3(offsetX, 0.55f, zDossier);
            dossier.transform.localScale = new Vector3(0.45f, 0.5f, 0.06f);
            dossier.GetComponent<Renderer>().sharedMaterial = matBois;
        }
    }

    // --- Cercle au sol pour délimiter le ring, dessiné avec un LineRenderer ---
    private static void CreerLigneRing(Transform parent, Vector3 center, float demiTailleSol)
    {
        const int segments = 64;
        float rayonRing = demiTailleSol * 0.85f; // un peu à l'intérieur du bord du sol

        GameObject ligneObj = new GameObject("LigneRing");
        ligneObj.transform.SetParent(parent);
        ligneObj.transform.position = center + Vector3.up * 0.02f; // évite le z-fighting avec le sol

        LineRenderer lr = ligneObj.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.positionCount = segments;
        lr.startWidth = 0.15f;
        lr.endWidth = 0.15f;
        lr.numCapVertices = 4;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        for (int i = 0; i < segments; i++)
        {
            float angle = (360f / segments) * i * Mathf.Deg2Rad;
            lr.SetPosition(i, new Vector3(Mathf.Cos(angle) * rayonRing, 0f, Mathf.Sin(angle) * rayonRing));
        }

        lr.material = NouveauMateriau("Mat_LigneRing", new Color(0.95f, 0.93f, 0.88f), 0f,
            "Universal Render Pipeline/Unlit");
    }

    // Material mat (peu brillant) sauvegardé comme asset persistant
    private static Material NouveauMateriau(string nom, Color couleur, float smoothness,
        string shaderName = "Universal Render Pipeline/Lit")
    {
        const string dossier = "Assets/Generated/ArenaDecor";
        if (!AssetDatabase.IsValidFolder(dossier))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Generated"))
                AssetDatabase.CreateFolder("Assets", "Generated");
            AssetDatabase.CreateFolder("Assets/Generated", "ArenaDecor");
        }

        Shader shaderVoulu = Shader.Find(shaderName);
        Shader shader = shaderVoulu != null ? shaderVoulu : Shader.Find("Standard");

        Material mat = new Material(shader) { name = nom, color = couleur };
        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", smoothness);
        else if (mat.HasProperty("_Glossiness"))
            mat.SetFloat("_Glossiness", smoothness);
        if (mat.HasProperty("_Metallic"))
            mat.SetFloat("_Metallic", 0f);

        string chemin = AssetDatabase.GenerateUniqueAssetPath($"{dossier}/{nom}.mat");
        AssetDatabase.CreateAsset(mat, chemin);
        return mat;
    }
}
#endif