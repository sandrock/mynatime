---
title: Commandes Mynatime
parent: Documentation (Français)
nav_order: 2
---

Aperçu des commandes
====================================

🇬🇧 [English version](AppCLI.Commands-overview.en.md)

L'application fonctionne autour d'instructions en ligne de commande. Chaque commande possède un nom long et un nom court. Le nom long est plus explicite, le nom court est plus rapide à saisir. Utilisez celui que vous préférez.

L'application vous permet de travailler hors-ligne avec le service. Certaines commandes effectuent des actions localement, d'autres envoient vos changements au service.

La première commande à connaître est la commande `help`. Elle liste les commandes disponibles.

```bash
m help
```

> MynatimeCLI  Mynatime help
>
> Usage : `mynatime` `[options]` `[commande]` `[options de la commande]`
>
> Options :
>
>   `--config-directory <répertoire>`   spécifie le répertoire des profils  
>   `--profile <profil>`                spécifie le profil à utiliser  
>   `-p <profil>`
>
> Commandes :
>
>   Aide  
>   `help`                             affiche l'aide
>
>   Profils  
>   `pro list`                         liste les profils utilisateur  
>   `pro add`                          crée un profil utilisateur (interactif)  
>   `pro status`                       affiche le statut du profil actuel
>
>   Catégories d'activité  
>   `act cat`                          liste les catégories d'activité  
>   `act cat refresh`                  met à jour les catégories depuis le service  
>   `act cat search <q>`               recherche des catégories  
>   `act cat alias <nom> <alias>`      crée un alias pour une catégorie  
>   `act cat unalias <alias>`          supprime un alias d'une catégorie
>
>   Ajouter une activité  
>   `act add [date] <durée> [catégorie]`                         ajoute une activité  
>   `act add <date> <heure-début> [date-fin] <heure-fin> [cat.]` ajoute une activité
>
>   Suivi d'activité  
>   `act start [date] [heure] [catégorie]`    démarre une activité  
>   `act stop  [date] [heure] [catégorie]`    arrête une activité  
>   `act status`                              liste les activités suivies  
>   `act events`                              liste les événements start/stop bruts  
>   `act clear`                               supprime tous les événements
>
>   Révision et publication  
>   `status`                           liste tous les changements en attente  
>   `commit`                           envoie les changements en attente au service
>
>   Options de l'application  
>   `--version`, `-V`                  affiche la version actuelle
>

Après une mise à jour de l'application, exécutez la commande `help` pour voir les nouvelles commandes disponibles.

Si la commande `m` n'est pas disponible, vérifiez [votre installation de mynatime](AppCLI.Install-from-sources.fr.md).
