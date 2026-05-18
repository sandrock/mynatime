---
title: Trucs et astuces
parent: Documentation (Français)
nav_order: 6
---

Trucs et astuces
====================================

🇬🇧 [English version](AppCLI.Tips-tricks.en.md)

Utilisation quotidienne
---------------------

### Défaire une action avant de publier

Il arrive que l'on fasse une erreur quand on exécute une commande. L'application ne propose pas (pour le moment) d'annuler une commande. Toutefois, il est possible de modifier manuellement les changements en attente avant de publier.

Ouvrez le fichier JSON de votre profil. Cherchez l'élément `Transaction`. Il contient les actions en attente. Chaque type d'action a sa propre représentation de données. Vous devriez pouvoir modifier les données pour obtenir l'effet souhaité. Vous pouvez aussi supprimer complètement un élément de transaction pour vous en débarrasser. Utilisez la commande `m status` pour vérifier que votre fichier est correct.


### Consulter les actions passées

Ouvrez le fichier JSON de votre profil. Cherchez l'élément `Commits`. Il contient les actions passées, groupées par publications.


### Alias de catégorie d'activité #15

Lors du suivi d'activité, certaines catégories ont des noms longs ou ambigus.

```bash
m act start interne
Too many possibilities for category "interne": "Company-interne", "Customer2-interne", "Meeting-interne"
```

Vous pouvez créer un alias :

```bash
m act cat alias "Company-interne" interne
```

Et utiliser l'alias pour le suivi ou l'ajout d'activités :

```bash
m act start interne
```


Développeurs
---------------------

### Nommer les profils pour une sélection plus facile

Les profils sont stockés dans `~/.config/mynatime/`. Le nom de fichier est généré à partir de la date d'authentification. Vous pouvez renommer les fichiers pour faciliter la sélection. Exemple :

- renommez `profile.20220918T100535632Z.json` en `kevin.json`
  - et utilisez l'argument `-pkevin`
- renommez `profile.20220924T041201230Z.json` en `test.json`
  - et utilisez l'argument `-ptest`


### Définir un profil par défaut

Quand vous utilisez plusieurs profils, notamment en développement, vous pouvez définir un profil par défaut.

Ouvrez le fichier JSON du profil à utiliser par défaut et ajoutez l'entrée JSON `"IsDefault": true,` dans l'objet racine :

```json
{
  "__manifest": "MynatimeProfile",
  "IsDefault": true,
  "DateCreated": "2022-09-18T10:05:35.6283817Z",
  ...
}
```


### Options du profil

Les fichiers de profil sont au format JSON et supportent des options supplémentaires que vous pouvez définir manuellement :

| Option | Défaut | Description |
|--------|--------|-------------|
| `ConfirmLocalSave` | false | Demande confirmation avant de sauvegarder le fichier de profil localement. Recommandé en développement pour éviter des écritures accidentelles. |
| `ConfirmServiceSave` | false | Demande confirmation avant d'envoyer chaque élément de transaction au service. Utile pour réviser les éléments un par un avant de les publier. |

Ces options sont également proposées de façon interactive lors de la création d'un profil avec `pro add`.
