# Plan d'implémentation — Grafiton (Phase 3 : Conversion)

Suite des Phases 1 (lecteur) et 2 (édition basique), toutes deux terminées et compilées avec succès. Cette phase ajoute les fonctionnalités de conversion de formats, la partie la plus délicate techniquement du projet en raison de la dépendance à un moteur externe pour les formats bureautiques.

---

## Point à valider avant de démarrer

> [!IMPORTANT]
> **Test de conformité des annotations (reliquat Phase 2)** : avant d'attaquer cette phase, demander à l'agent de confirmer par un test manuel que les annotations (surlignage, note, dessin, formes) créées en Phase 2 s'ouvrent et s'affichent correctement dans un lecteur PDF tiers (Adobe Reader ou Edge), et pas seulement dans Grafiton. Ne pas démarrer la Phase 3 tant que ce point n'est pas confirmé.

---

## Fonctionnalités à développer

### 1. Conversion image ↔ PDF
- PDF → images : export de chaque page en PNG ou JPG, avec choix de la résolution (DPI)
- Images → PDF : sélection de plusieurs images, réordonnancement, génération d'un PDF (une image par page)
- Utiliser le rendu déjà disponible via WebView2/PDF.js pour l'export d'images depuis le PDF

### 2. Conversion PDF ↔ formats bureautiques (Word, Excel, PowerPoint)
- **Dépendance** : LibreOffice en mode headless (`soffice --headless --convert-to <format> <fichier>`)
- Détection au lancement de l'application : vérifier si LibreOffice est installé (chemin standard `Program Files\LibreOffice\program\soffice.exe`), sinon :
  - Proposer un lien de téléchargement officiel
  - Ou (optionnel, à discuter) embarquer une version portable de LibreOffice dans l'installeur de Grafiton
- Exécution du processus `soffice` en arrière-plan (`Process.Start`, redirection de sortie, gestion du timeout et des erreurs)
- Formats à couvrir :
  - PDF → Word (.docx)
  - PDF → Excel (.xlsx) — préciser que la qualité dépend fortement de la structure du PDF source (tableaux bien définis vs texte libre)
  - PDF → PowerPoint (.pptx)
  - Word/Excel/PowerPoint → PDF

### 3. Compression / optimisation de la taille
- Réduction de la résolution des images intégrées (avec choix qualité : faible/moyenne/haute)
- Suppression des métadonnées inutiles
- Affichage de la taille avant/après pour que l'utilisateur valide le résultat

### 4. Interface de conversion
- Écran ou dialogue centralisé "Convertir" listant les formats disponibles selon le fichier ouvert (ex. si un PDF est ouvert : proposer image, Word, Excel, PowerPoint ; si un fichier bureautique est glissé-déposé : proposer PDF)
- Barre de progression pendant la conversion (les conversions via LibreOffice peuvent prendre plusieurs secondes)
- Gestion claire des erreurs : fichier corrompu, LibreOffice absent, timeout, format non supporté

---

## Architecture — nouveaux composants

```
Grafiton/
├── Services/
│   ├── IConversionService.cs      (interface : toutes les conversions)
│   ├── ConversionService.cs       (orchestration : image/PDF direct, Office via LibreOffice)
│   ├── ILibreOfficeService.cs     (détection, exécution du process soffice)
│   ├── LibreOfficeService.cs
│   └── ICompressionService.cs / CompressionService.cs
├── ViewModels/
│   └── ConversionViewModel.cs     (état : format source/cible, progression, erreurs)
├── Views/
│   └── ConversionDialog.xaml      (sélection de format, lancement, barre de progression)
└── Models/
    └── ConversionJob.cs           (fichier source, format cible, statut, chemin de sortie)
```

---

## Design des interfaces (via serveur MCP Stitch)

> [!NOTE]
> Demander au serveur MCP **Stitch** de générer une maquette pour `ConversionDialog` (sélection de format avec icônes par type de fichier, barre de progression, état d'erreur), cohérente avec les maquettes déjà produites en Phase 2 (`PageManagerPanel`, `AnnotationToolbar`, etc.).

---

## Verification Plan

### Automated Tests
- `dotnet build` : compilation sans erreur
- Tests unitaires sur `ConversionService` pour les conversions image ↔ PDF (pas de dépendance externe, donc testable directement)
- Tests d'intégration sur `LibreOfficeService` : marquer comme "skip" automatiquement si LibreOffice n'est pas installé sur la machine de test, pour ne pas casser le build sur des environnements sans LibreOffice

### Manual Verification
- Exporter un PDF de plusieurs pages en images PNG, vérifier la résolution et la fidélité
- Créer un PDF à partir de 3 images, vérifier l'ordre des pages
- Convertir un PDF texte simple en Word, ouvrir le résultat et vérifier la lisibilité du texte
- Convertir un PDF contenant un tableau en Excel, vérifier que les colonnes/lignes sont correctement détectées (s'attendre à des imperfections selon la complexité du tableau)
- Convertir un document Word en PDF et vérifier la mise en page
- Tester le comportement de l'application quand LibreOffice n'est pas installé (message clair, pas de crash)
- Compresser un PDF contenant des images haute résolution, vérifier la réduction de taille et la qualité visuelle résultante