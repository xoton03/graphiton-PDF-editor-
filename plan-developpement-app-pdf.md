# Plan de développement — Application PDF complète (Windows, C#/WPF)

## 1. Objectif du projet

Créer une application Windows native en **C# / WPF** permettant de **lire**, **modifier** et **convertir** des fichiers PDF, dans le but de remplacer à la fois les lecteurs PDF classiques et les sites en ligne type ilovepdf.com. L'application doit rester 100% locale (aucun fichier envoyé sur un serveur externe pour le traitement).

Le style visuel et l'architecture doivent suivre la même approche que le projet existant **Aether Markdown** (WPF-UI, Fluent Design, effet Mica, architecture MVVM).

---

## 2. Stack technique

| Composant | Choix recommandé | Rôle |
|---|---|---|
| Framework | .NET 10, WPF | Base de l'application |
| UI | WPF-UI (Fluent Design, Mica) | Cohérence visuelle avec Aether Markdown |
| Rendu / lecture PDF | WebView2 + PDF.js | Visualisation, zoom, recherche, navigation |
| Manipulation de pages (fusion, rotation, filigrane, mots de passe) | PdfSharpCore (MIT, gratuit) | Opérations "faciles" sur les PDF |
| Extraction de texte / recherche full-text | PdfPig (MIT, gratuit) | Extraction, indexation, recherche |
| OCR (PDF scannés) | Tesseract (wrapper .NET) | Rendre le texte des scans cherchable |
| Conversion PDF ↔ Word/Excel/PowerPoint | LibreOffice en mode headless (`soffice --headless --convert-to`) | Conversion de formats bureautiques |
| Conversion image ↔ PDF | PdfSharpCore / SixLabors.ImageSharp | Export image, création de PDF depuis images |
| Architecture | MVVM (comme Aether Markdown) | Séparation UI / logique |

**Note de licence importante** : éviter iText7 et Aspose.PDF pour toute fonctionnalité liée à l'édition avancée de texte "in place", car leurs licences (AGPL ou commerciale) sont incompatibles avec une distribution fermée et gratuite. Rester sur des bibliothèques MIT/Apache (PdfSharpCore, PdfPig) tant que possible.

---

## 3. Architecture du projet

```
PdfApp/
├── PdfApp.sln
├── PdfApp/
│   ├── App.xaml
│   ├── Views/
│   │   ├── MainWindow.xaml
│   │   ├── ViewerView.xaml
│   │   ├── EditToolsPanel.xaml
│   │   └── ConversionDialog.xaml
│   ├── ViewModels/
│   │   ├── MainViewModel.cs
│   │   ├── ViewerViewModel.cs
│   │   ├── EditViewModel.cs
│   │   └── ConversionViewModel.cs
│   ├── Models/
│   │   ├── PdfDocument.cs
│   │   └── ConversionJob.cs
│   ├── Services/
│   │   ├── FileService.cs
│   │   ├── PdfRenderService.cs        (wrapper WebView2 / PDF.js)
│   │   ├── PdfEditService.cs          (wrapper PdfSharpCore)
│   │   ├── PdfTextService.cs          (wrapper PdfPig)
│   │   ├── OcrService.cs              (wrapper Tesseract)
│   │   └── ConversionService.cs       (wrapper LibreOffice headless)
│   └── Resources/
│       └── pdfjs/                     (bundle PDF.js pour WebView2)
└── PdfApp.Tests/
```

Cette structure reprend le même découpage que Aether Markdown (MainViewModel + FileService + services spécialisés), afin de rester cohérent entre les deux projets.

---

## 4. Phases de développement

### Phase 1 — MVP Lecteur (fondation)
- [ ] Créer le projet WPF avec WPF-UI, effet Mica, thème clair/sombre
- [ ] Intégrer WebView2 + PDF.js pour l'affichage des PDF
- [ ] Ouvrir un fichier PDF (bouton + glisser-déposer)
- [ ] Navigation entre les pages, zoom, mode "ajuster à la largeur"
- [ ] Recherche de texte dans le document
- [ ] Liste des fichiers récents
- [ ] Vue miniatures des pages (panneau latéral)

### Phase 2 — Édition basique
- [ ] Réorganiser les pages (glisser-déposer dans le panneau miniatures)
- [ ] Supprimer / faire pivoter des pages
- [ ] Fusionner plusieurs PDF en un seul
- [ ] Scinder un PDF (extraire une plage de pages)
- [ ] Ajouter un filigrane (texte ou image)
- [ ] Protéger/déprotéger par mot de passe
- [ ] Annotations : surlignage, notes, dessin libre, formes

### Phase 3 — Conversion
- [ ] PDF → images (PNG/JPG), par page ou document entier
- [ ] Images → PDF
- [ ] PDF → Word/Excel/PowerPoint (via LibreOffice headless)
- [ ] Word/Excel/PowerPoint → PDF (via LibreOffice headless)
- [ ] Détection et gestion de l'installation de LibreOffice (vérifier présence, sinon proposer d'installer ou d'utiliser un dossier portable embarqué)
- [ ] Compression / optimisation de la taille du PDF

### Phase 4 — Fonctionnalités avancées
- [ ] OCR sur PDF scannés (rendre le texte cherchable/extractible)
- [ ] Extraction de texte et d'images intégrées dans un PDF
- [ ] Formulaires PDF : lecture et remplissage de champs
- [ ] Signature électronique simple (image de signature + positionnement)
- [ ] Comparaison de deux versions d'un PDF (diff visuel)

### Phase 5 — Finitions
- [ ] Mode sombre/clair complet
- [ ] Raccourcis clavier (navigation, recherche, zoom)
- [ ] Gestion des erreurs et messages utilisateur clairs (fichier corrompu, protégé, etc.)
- [ ] Tests sur des PDF complexes (scannés, formulaires, gros fichiers)
- [ ] Packaging / installeur Windows (MSIX ou installeur classique)

---

## 5. Instructions pour l'agent (Antigravity)

- Développer phase par phase, dans l'ordre ci-dessus ; ne pas passer à la phase suivante avant que la précédente compile et fonctionne.
- Réutiliser le pattern MVVM et, si possible, le style visuel (thème WPF-UI, Mica) du projet Aether Markdown existant pour garder une cohérence entre les deux applications.
- Pour chaque service (PdfRenderService, PdfEditService, etc.), écrire une interface (`IPdfEditService`, etc.) pour permettre les tests unitaires et remplacer facilement une bibliothèque plus tard.
- Documenter dans le code toute limitation liée à la bibliothèque choisie (ex : PdfSharpCore ne permet pas l'édition de texte existant "in place").
- Vérifier la licence de toute nouvelle dépendance ajoutée avant de l'intégrer (rester MIT/Apache/BSD autant que possible).

---

## 6. Critères d'acceptation globaux

- L'application fonctionne 100% hors-ligne, aucun fichier n'est envoyé vers un service externe.
- Les opérations de lecture et d'édition ne corrompent jamais le fichier original (toujours travailler sur une copie / proposer "Enregistrer sous").
- L'interface reste fluide même sur des PDF de plusieurs centaines de pages.
- Les fonctionnalités de conversion gèrent proprement les cas d'erreur (fichier protégé, format non supporté, LibreOffice absent, etc.).
