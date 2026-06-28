#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.Events;
using TMPro;

/// <summary>
/// Construit l'interface de la scène Victory (titre, texte de résultat, boutons Rejouer/Menu).
/// À utiliser depuis la scène "Victory" ouverte (pas Arena_Sandaga).
/// Menus : Tools > Dakar Arena > Construire / Supprimer le menu de victoire
/// </summary>
public static class VictoryUIBuilder
{
    private const string NomConteneur = "MenuVictoireUI";

    [MenuItem("Tools/Dakar Arena/Construire le menu de victoire")]
    public static void ConstruireMenuVictoire()
    {
        GameObject existing = GameObject.Find(NomConteneur);
        if (existing != null)
        {
            Debug.Log($"'{NomConteneur}' existe déjà. Supprime-le d'abord (Tools > Dakar Arena > Supprimer le menu de victoire) si tu veux le régénérer.");
            Selection.activeGameObject = existing;
            return;
        }

        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        GameObject canvasObj;
        if (canvas == null)
        {
            canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        else
        {
            canvasObj = canvas.gameObject;
        }

        // Le projet utilise le nouvel Input System (vu dans PlayerController) :
        // on ajoute le bon module d'UI, sinon les clics sur les boutons ne fonctionnent pas.
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
        }

        VictoryManager victoryManager = Object.FindAnyObjectByType<VictoryManager>();
        if (victoryManager == null)
        {
            GameObject vmObj = new GameObject("VictoryManager");
            victoryManager = vmObj.AddComponent<VictoryManager>();
        }

        GameObject conteneur = new GameObject(NomConteneur, typeof(RectTransform));
        conteneur.transform.SetParent(canvasObj.transform, false);
        RectTransform rtConteneur = conteneur.GetComponent<RectTransform>();
        rtConteneur.anchorMin = Vector2.zero;
        rtConteneur.anchorMax = Vector2.one;
        rtConteneur.offsetMin = Vector2.zero;
        rtConteneur.offsetMax = Vector2.zero;

        CreerTexte(conteneur.transform, "Titre", "VICTOIRE", new Vector2(0, 200), 72, FontStyles.Bold,
            new Color(0.95f, 0.85f, 0.55f));

        TextMeshProUGUI resultat = CreerTexte(conteneur.transform, "Resultat", "Fin du combat",
            new Vector2(0, 80), 40, FontStyles.Normal, Color.white);
        victoryManager.texteResultat = resultat;

        CreerBouton(conteneur.transform, "BoutonRejouer", "Rejouer", new Vector2(-140, -120), victoryManager.Replay);
        CreerBouton(conteneur.transform, "BoutonMenu", "Menu principal", new Vector2(140, -120), victoryManager.GoToMenu);

        Selection.activeGameObject = conteneur;
        Debug.Log("Menu de victoire construit (titre, résultat, 2 boutons). Lance Play depuis Arena_Sandaga pour le tester en conditions réelles.");
    }

    [MenuItem("Tools/Dakar Arena/Supprimer le menu de victoire")]
    public static void SupprimerMenuVictoire()
    {
        GameObject existing = GameObject.Find(NomConteneur);
        if (existing != null) Object.DestroyImmediate(existing);
    }

    private static TextMeshProUGUI CreerTexte(Transform parent, string nom, string contenu, Vector2 position,
        float taille, FontStyles style, Color couleur)
    {
        GameObject obj = new GameObject(nom, typeof(RectTransform));
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(900, 100);

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = contenu;
        tmp.fontSize = taille;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = couleur;

        return tmp;
    }

    private static void CreerBouton(Transform parent, string nom, string libelle, Vector2 position, UnityAction action)
    {
        GameObject obj = new GameObject(nom, typeof(RectTransform));
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(220, 70);

        Image image = obj.AddComponent<Image>();
        image.color = new Color(0.85f, 0.62f, 0.2f);

        Button bouton = obj.AddComponent<Button>();
        // Listener "persistant" : apparaît dans l'Inspector comme si on l'avait glissé à la main,
        // et reste bien enregistré avec la scène (contrairement à AddListener simple en code).
        UnityEventTools.AddVoidPersistentListener(bouton.onClick, action);

        GameObject texteObj = new GameObject("Texte", typeof(RectTransform));
        texteObj.transform.SetParent(obj.transform, false);
        RectTransform texteRt = texteObj.GetComponent<RectTransform>();
        texteRt.anchorMin = Vector2.zero;
        texteRt.anchorMax = Vector2.one;
        texteRt.offsetMin = Vector2.zero;
        texteRt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = texteObj.AddComponent<TextMeshProUGUI>();
        tmp.text = libelle;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.black;
        tmp.fontSize = 26;
    }
}
#endif