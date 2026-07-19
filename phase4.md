# Plan d'implémentation — Grafiton (Phase 4 : Fonctionnalités avancées)

Suite des Phases 1 (lecteur), 2 (édition basique) et 3 (conversion), toutes terminées et compilées avec succès. Cette phase ajoute les fonctionnalités avancées prévues au plan initial ainsi que de nouvelles fonctionnalités identifiées pour rapprocher Grafiton d'un remplacement complet des lecteurs PDF classiques et des sites comme ilovepdf.com.

---

## Rappel avant de démarrer

> [!IMPORTANT]
> Vérifier que `SixLabors.ImageSharp` a bien été mis à jour vers une version ≥ 3.1.7 (correction des vulnérabilités NU1902/NU1903 signalées précédemment) avant d'ajouter de nouvelles dépendances à ce package.

---

## Fonctionnalités à développer

### 1. OCR (reconnaissance de texte sur PDF scannés)
- Intégration de **Tesseract** (wrapper .NET, ex. `Tesseract` NuGet package, licence Apache 2.0)
- Détection automatique des pages sans couche de texte (scans purs) et proposition d'OCR
- Génération d'un PDF avec couche de texte invisible superposée à l'image scannée (texte cherchable et copiable sans changer l'apparence visuelle)
- Choix de la langue de reconnaissance (au minimum français et anglais)

### 2. Extraction de contenu
- Extraire tout le texte d'un PDF vers un fichier `.txt`
- Extraire toutes les images intégrées vers un dossier (formats d'origine conservés)
- Extraction par plage de pages (comme pour la scission déjà existante)

### 3. Formulaires PDF (AcroForms)
- Détection des champs de formulaire dans un PDF ouvert
- Remplissage des champs (texte, cases à cocher, boutons radio, listes déroulantes) directement dans le visualiseur
- Sauvegarde des valeurs remplies dans le PDF (aplati/"flatten" en option pour empêcher toute modification ultérieure)

### 4. Signature électronique simple
- Import d'une image de signature (PNG transparent) ou dessin à main levée (canvas)
- Positionnement libre sur la page (glisser-déposer, redimensionnement)
- Application définitive dans le document (fusion avec le contenu de la page)
- **Ne pas présenter ceci comme une signature électronique qualifiée/légale** — le décrire clairement dans l'UI comme une signature visuelle simple, pas un mécanisme de signature cryptographique certifiée

### 5. Comparaison de documents
- Sélection de deux versions d'un même PDF
- Rendu côte à côte ou superposé avec mise en évidence visuelle des différences (texte ajouté/supprimé, éléments déplacés)
- Navigation rapide entre les pages contenant des différences

### 6. Traitement par lot
- Sélection de plusieurs fichiers PDF simultanément
- Application en série d'une même opération : filigrane, compression, conversion (image ou Office), rotation, protection par mot de passe
- File d'attente avec barre de progression globale et par fichier, gestion des erreurs individuelles sans interrompre le lot entier

### 7. Rédaction / masquage définitif
- Sélection d'une zone ou d'un texte à masquer
- Suppression réelle du contenu sous-jacent (texte et image), pas seulement un rectangle noir superposé visuellement
- Confirmation explicite avant application (opération irréversible sur le fichier de sortie)

### 8. Signets et navigation
- Lecture des signets existants dans un PDF (arborescence de navigation)
- Ajout, suppression, renommage de signets par l'utilisateur
- Panneau latéral dédié (à côté du panneau de miniatures déjà existant)

### 9. En-têtes, pieds de page et numérotation
- Ajout de numéros de page (position, format configurables : "1", "Page 1/10", etc.)
- Ajout de texte répété en en-tête/pied de page (date, titre du document, etc.)

### 10. Export PDF/A (archivage)
- Conversion vers le format PDF/A (1b ou 2b) pour l'archivage long terme
- Vérification de conformité de base avant export (polices intégrées, pas de contenu dynamique incompatible)

### 11. Bibliothèque de documents
- Vue "Bibliothèque" en complément de l'écran d'accueil actuel : grille de vignettes des PDF récents/favoris
- Marquage de fichiers en favoris
- Recherche par nom de fichier dans l'historique

### 12. Lecture à voix haute (accessibilité)
- Utilisation de l'API de synthèse vocale de Windows (`System.Speech.Synthesis` ou `Windows.Media.SpeechSynthesis`)
- Lecture du texte extrait de la page courante, avec contrôle lecture/pause/vitesse
- Mise en évidence visuelle du texte lu au fur et à mesure (optionnel, si le temps le permet)

---

## Architecture — nouveaux composants

```
Grafiton/
├── Services/
│   ├── IOcrService.cs / OcrService.cs
│   ├── IExtractionService.cs / ExtractionService.cs
│   ├── IFormFillService.cs / FormFillService.cs
│   ├── ISignatureService.cs / SignatureService.cs
│   ├── IComparisonService.cs / ComparisonService.cs
│   ├── IBatchService.cs / BatchService.cs
│   ├── IRedactionService.cs / RedactionService.cs
│   ├── IBookmarkService.cs / BookmarkService.cs
│   ├── IHeaderFooterService.cs / HeaderFooterService.cs
│   ├── IPdfAExportService.cs / PdfAExportService.cs
│   ├── ILibraryService.cs / LibraryService.cs        (favoris, recherche)
│   └── ITextToSpeechService.cs / TextToSpeechService.cs
├── ViewModels/
│   ├── OcrViewModel.cs
│   ├── FormFillViewModel.cs
│   ├── SignatureViewModel.cs
│   ├── ComparisonViewModel.cs
│   ├── BatchViewModel.cs
│   ├── RedactionViewModel.cs
│   ├── BookmarkPanelViewModel.cs
│   ├── HeaderFooterViewModel.cs
│   ├── LibraryViewModel.cs
│   └── TextToSpeechViewModel.cs
├── Views/
│   ├── OcrDialog.xaml
│   ├── FormFillOverlay.xaml
│   ├── SignatureDialog.xaml
│   ├── ComparisonView.xaml
│   ├── BatchDialog.xaml
│   ├── RedactionOverlay.xaml
│   ├── BookmarkPanel.xaml
│   ├── HeaderFooterDialog.xaml
│   ├── LibraryView.xaml
│   └── TextToSpeechToolbar.xaml
└── Models/
    ├── BookmarkModel.cs
    ├── BatchJob.cs
    └── LibraryEntry.cs
```

---

## Priorisation suggérée (à ajuster selon usage réel)

1. **Traitement par lot** — le plus proche de l'usage type "site de conversion en ligne" que tu veux remplacer
2. **OCR** — débloque l'usage sur les documents scannés, très fréquent
3. **Signets et navigation** — amélioration d'ergonomie à faible coût de développement
4. **Formulaires PDF** et **Signature électronique** — souvent utilisés ensemble (remplir puis signer un document)
5. **Rédaction/masquage** — utile mais plus sensible (bien tester l'irréversibilité)
6. **Bibliothèque de documents** — confort d'usage à long terme
7. **Comparaison de documents**, **En-têtes/pieds de page**, **Export PDF/A**, **Lecture à voix haute** — compléments, à traiter selon le temps disponible

---

## Design des interfaces (via serveur MCP Stitch)

> [!NOTE]
> Demander au serveur MCP **Stitch** des maquettes pour les écrans à plus fort impact visuel avant codage XAML : `BatchDialog` (file d'attente et progression), `OcrDialog`, `FormFillOverlay`, `SignatureDialog`, `LibraryView` (grille de vignettes). Rester cohérent avec les maquettes déjà produites en Phases 2 et 3.

---

## Verification Plan

### Automated Tests
- `dotnet build` : compilation sans erreur
- Tests unitaires sur chaque nouveau service, en particulier :
  - `BatchService` : traitement de 3+ fichiers avec un échec simulé sur l'un d'eux (le lot doit continuer)
  - `RedactionService` : vérifier que le texte masqué n'est plus présent dans le flux extrait du PDF résultant (pas seulement caché visuellement)
  - `OcrService` : vérifier que le texte reconnu est bien recherchable dans le PDF de sortie

### Manual Verification
- OCR sur un PDF scanné réel, vérifier la recherche de texte après traitement
- Remplir un formulaire PDF existant, sauvegarder, rouvrir et vérifier la persistance des valeurs
- Ajouter une signature, vérifier son intégration définitive (le document ne doit plus permettre de la déplacer après sauvegarde)
- Lancer un traitement par lot sur 5 fichiers avec des opérations différentes, vérifier la progression et le résultat de chacun
- Masquer un texte sensible, sauvegarder, puis tenter d'extraire le texte du fichier résultant pour confirmer qu'il a bien disparu
- Ajouter/renommer/supprimer des signets et vérifier leur persistance après réouverture
- Tester la lecture à voix haute sur une page de texte simple