# Plan d'implémentation — Grafiton (Phase 2 : Édition basique)

Suite de la Phase 1 (MVP Lecteur, terminée et validée). Cette phase ajoute les fonctionnalités d'édition "faciles" sur les PDF, en s'appuyant sur `PdfSharpCore` déjà intégré au projet.

---

## Point à clarifier avant de démarrer

> [!IMPORTANT]
> **Moteur de rendu WebView2** : confirmer explicitement si `ViewerView.xaml` utilise **PDF.js embarqué** (bundle JS chargé dans WebView2) ou le **moteur PDF natif d'Edge/Chromium**. C'est bloquant pour cette phase : les annotations et le surlignage doivent pouvoir se superposer au rendu et interagir avec les coordonnées du texte. Si le moteur natif est utilisé, migrer vers PDF.js embarqué avant de continuer.

---

## Fonctionnalités à développer

### 1. Réorganisation des pages
- Panneau de miniatures (déjà prévu en Phase 1) avec glisser-déposer pour réordonner les pages
- Bouton "Supprimer la page" par miniature
- Bouton "Faire pivoter" (90° gauche/droite) par miniature ou en masse (sélection multiple)

### 2. Fusion et scission de documents
- **Fusionner** : sélectionner plusieurs fichiers PDF et les combiner en un seul document, avec réordonnancement avant validation
- **Scinder** : extraire une plage de pages (ex. pages 3 à 7) vers un nouveau fichier
- Prévisualisation du résultat avant écriture sur disque

### 3. Filigrane (watermark)
- Ajouter un filigrane texte (police, taille, opacité, rotation, position configurables)
- Ajouter un filigrane image (logo, PNG avec transparence)
- Application sur toutes les pages ou une plage sélectionnée

### 4. Protection par mot de passe
- Ajouter un mot de passe d'ouverture (chiffrement)
- Retirer un mot de passe existant (si l'utilisateur le fournit)
- Restrictions optionnelles (impression, copie de texte désactivée)

### 5. Annotations
- Surlignage de texte sélectionné (nécessite le rendu PDF.js pour récupérer les coordonnées de sélection)
- Notes/commentaires collants (icône cliquable avec texte)
- Dessin libre (stylo, épaisseur et couleur configurables)
- Formes simples (rectangle, cercle, flèche)
- Toutes les annotations doivent être enregistrées dans le PDF lui-même (standard PDF annotations), pas dans un fichier séparé

---

## Architecture — nouveaux composants

```
Grafiton/
├── Services/
│   ├── IPdfEditService.cs      (interface : réorganiser, fusionner, scinder, filigrane, mot de passe)
│   ├── PdfEditService.cs       (implémentation via PdfSharpCore)
│   ├── IAnnotationService.cs   (interface : surlignage, notes, dessin, formes)
│   └── AnnotationService.cs
├── ViewModels/
│   ├── PageManagerViewModel.cs (miniatures, réorganisation, suppression, rotation)
│   ├── MergeSplitViewModel.cs
│   ├── WatermarkViewModel.cs
│   ├── PasswordViewModel.cs
│   └── AnnotationToolbarViewModel.cs
├── Views/
│   ├── PageManagerPanel.xaml
│   ├── MergeSplitDialog.xaml
│   ├── WatermarkDialog.xaml
│   ├── PasswordDialog.xaml
│   └── AnnotationToolbar.xaml
└── Models/
    └── Annotation.cs           (type, position, contenu, couleur, page associée)
```

---

## Design des interfaces (via serveur MCP Stitch)

> [!NOTE]
> Avant de coder les vues XAML de cette phase, demander au serveur MCP **Stitch** de générer des maquettes pour :
> - Le panneau de miniatures avec réorganisation (`PageManagerPanel`)
> - La boîte de dialogue de fusion/scission (`MergeSplitDialog`)
> - La barre d'outils d'annotation (`AnnotationToolbar`), avec icônes cohérentes pour surlignage/note/dessin/formes
>
> Utiliser ces maquettes comme référence visuelle avant l'implémentation XAML, pour garder une cohérence Fluent Design avec l'écran d'accueil existant.

---

## Verification Plan

### Automated Tests
- `dotnet build` : compilation sans erreur
- Tests unitaires sur `PdfEditService` : fusion de 2 PDF, scission, rotation, ajout de filigrane (vérifier le nombre de pages et le contenu résultant)
- Tests unitaires sur `AnnotationService` : ajout/suppression d'une annotation, sérialisation correcte dans le PDF

### Manual Verification
- Réorganiser des pages par glisser-déposer et vérifier l'ordre final après sauvegarde
- Fusionner 2-3 fichiers PDF et vérifier l'ordre et l'intégrité du résultat
- Scinder un document et vérifier que chaque fichier extrait s'ouvre correctement
- Ajouter un filigrane texte et image, vérifier le rendu sur plusieurs pages
- Protéger un PDF par mot de passe, fermer et rouvrir l'application, vérifier la demande de mot de passe
- Surligner du texte, ajouter une note et une forme, sauvegarder, rouvrir le fichier dans un autre lecteur PDF pour confirmer que les annotations sont bien standard et visibles ailleurs