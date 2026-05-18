---
title: Ajouter une activité
parent: Documentation (Français)
nav_order: 5
---

Ajouter une activité
====================================

🇬🇧 [English version](AppCLI.Add-activity.en.md)

Rafraîchir les catégories d'activité
-----------------------------------

Vous devez d'abord récupérer la liste des catégories d'activité depuis le service.

```bash
m activity category refresh
m act cat refresh
```

Vous pouvez afficher les catégories à tout moment :

```bash
m activity category
m act cat
```

Si une catégorie est créée par votre responsable, vous devrez exécuter à nouveau la commande refresh.

Vous pouvez rechercher une catégorie :

```bash
m activity category search project something
m act cat search project something
```


Ajouter une activité passée
-----------------------------------

### Utilisation des commandes

Le suivi d'activité utilise une liste d'événements pour créer des activités. Vous pouvez ignorer le suivi et créer des activités directement. C'est utile pour ajouter des activités passées.

```bash
m act add [date] <durée> [catégorie]                         # ajoute une activité
m act add <date> <heure-début> [date-fin] <heure-fin> [cat.] # ajoute une activité
                        [--message|-m <message>]              # vous pouvez ajouter un commentaire
```


### Exemple 1

Ajoutons une activité pour le jour courant, en utilisant une heure de début (08:30) et une heure de fin (11:50) pour le projet "Internal".

```bash
m act add 0830 1150 Internal
```

Ajoutons une activité pour hier (2022-09-15), en utilisant une heure de début (08:30) et une heure de fin (11:50) pour le projet "Internal".

```bash
m act add 2022-09-15 0830 1150 Internal
```

Vous pouvez aussi utiliser une durée (2.7 heures) au lieu des heures (pour "Project 123").

```bash
m act add 2022-09-15 2.7 Project-123
```

Vous pouvez aussi ajouter un commentaire avec l'argument `-m` :

```bash
m act add 2022-09-15 2.7 Project-123 -m "aide John sur le design"
```

Tout cela est uniquement sauvegardé sur votre ordinateur. Vous pouvez réviser ce que vous avez créé :

```bash
m status
```

Si cela vous convient, vous pouvez publier au service :

```bash
m commit
```
