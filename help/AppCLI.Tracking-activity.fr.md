---
title: Suivi d'activité
parent: Documentation (Français)
nav_order: 4
---

Suivi d'activité
====================================

🇬🇧 [English version](AppCLI.Tracking-activity.en.md)

Rafraîchir les catégories d'activité
-----------------------------------

Vous devez d'abord récupérer la liste des catégories d'activité depuis le service.

```bash
m activity category refresh
m act cat refresh
```

> MynatimeCLI  
> Refreshing categories...  
> Renewing session...  
> ActCategory 51615 &lt;Out of office - planned&gt;  
> ActCategory 51077 &lt;MyCompany - Internal&gt;  
> ActCategory 51432 &lt;MyCompany - Demos&gt;  
> ActCategory 51712 &lt;Project 123&gt;  
> ActCategory 51402 &lt;Internal System Administration&gt;  
> ActCategory 51556 &lt;Production System Administration&gt;  
> ActCategory 51112 &lt;Project 789&gt;  
> ...

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


Suivi d'activité
-----------------------------------

### Workflow

La meilleure façon d'ajouter des activités est de suivre votre travail au fur et à mesure. Vous utilisez la commande `activity start/stop` pour indiquer l'activité que vous démarrez ou arrêtez.

Plus tard, une liste d'activités sera générée à partir de ces déclarations. Vous pourrez les envoyer au service.


### Exemple 1

L'utilisation de base est la suivante :

- à `08:51` : vous arrivez au travail
    - `m act start`
- à `09:07` : vous commencez à travailler sur project1
    - `m act start project1`
- à `12:08` : vous vous arrêtez
    - `m act stop`
- à `13:45` : vous reprenez le travail
    - `m act start`
- à `17:41` : en vous arrêtant, vous indiquez le projet
    - `m act stop project1`
- à `17:41` : vous savez que vous partirez à 17:45
    - `m act stop 1745`
- à `17:41` : vous pouvez réviser les activités générées
    - `m status`
- à `17:42` : et vous pouvez les publier
    - `m commit`

La commande status affiche les activités générées :

```
0	Activity tracker
Activities:
- 2022-10-30 08:51 09:07
- 2022-10-30 09:07 12:08 Project1
- 2022-10-30 13:45 17:41 Project1
- 2022-10-30 17:41 17:45
```

La commande status est importante car elle détecte les erreurs de saisie habituelles et les affiche.


### Avertissements et erreurs de suivi

Quand vous utilisez la commande status, ces avertissements peuvent s'afficher :

- *"Nightly activity between 2022-10-29 and 2022-10-30." (NightlyItem)*
    - Soit vous avez oublié de faire un `stop` en quittant le travail, soit vous avez travaillé après minuit.
- *"Multiple days activity between 2022-10-27 and 2022-10-29. (ManyDaysItem)"*
    - Une activité générée s'étendrait sur plusieurs jours. C'est inhabituel.

Et ces erreurs peuvent s'afficher :

- *"Stop event 2022-10-27-1000 is not following a start event." (StopNotFollowingStart)*
- *"Event 2022-10-27-1100 is of unknown type." (UnknownEventType)*
    - Peut survenir si vous modifiez manuellement le fichier de données.


### Exemple 2

Voici un exemple montrant ce qui se passe quand vous oubliez de spécifier certains changements d'activité.

Disons que vous arrivez au travail et commencez à travailler sur project1.

```bash
# il est 08:40
m act start project1
```

Vous commencez à travailler sur project1 et c'est l'heure de la réunion quotidienne.

```bash
# il est 09:30
m act start daily
```

La réunion quotidienne est terminée... Vous êtes passé sur project2... Il est midi, c'est l'heure du déjeuner...
Vous avez oublié de terminer la réunion et de changer de projet. Vous pouvez le faire maintenant en spécifiant les heures.

```bash
# il est 12:02
m act stop 0940       # ceci arrête le "daily"
m act start 0940      # ceci démarre quelque chose
m act stop project2   # et l'arrête, en l'associant à project2
```

Chaque fois que vous changez de tâche, vous n'avez qu'à la "démarrer". Quand vous vous arrêtez, vous faites un "stop".

En fin de journée, vous pouvez réviser les activités :

```bash
m act status         # affiche les détails complets
m status             # affiche un résumé des changements
```

Et les publier :

```bash
m commit
```


### Utilisation des commandes

Voici tout ce que vous pouvez faire avec le suivi d'activité :

```bash
m act start [heure] [catégorie]              # démarre une activité
m act stop  [heure] [catégorie]              # arrête une activité
m act status                                 # liste les activités générées (vue tableau)
m act events                                 # liste les événements start/stop bruts
m act clear                                  # supprime tous les événements
m act start/stop [date] [heure] [catégorie]  # vous pouvez spécifier une date
                 [--message|-m <message>]    # vous pouvez ajouter un commentaire
```
