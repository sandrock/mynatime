---
title: Accueil
nav_order: 1
---

Mynatime
====================

🇬🇧 [English version](README.md)

Informations
-------------

||                                                                                                                                                              |
|--|--------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Statut du projet    | **bêta** (collecte de retours utilisateurs)                                                                                                                  |
| Objectif            | **fournir un CLI rapide pour gérer les activités dans le service de suivi du temps**.                                                                        |
| Relation au service | application CLI et GUI non officielle pour Manatime, à utiliser à vos risques et périls                                                                     |
| Statut du CLI       | **bêta** (collecte de retours utilisateurs)                                                                                                                  |
| Statut du GUI       | **expérimental**                                                                                                                                             |
| Build               | net8.0                                                                                                                                                       |
| Licence             | GNU General Public License Version 3                                                                                                                         |
| Contributions       | **ouvert aux contributions** : via les issues et les PRs                                                                                                     |
| CI                  | [![CI](https://github.com/sandrock/mynatime/actions/workflows/ci.yml/badge.svg)](https://github.com/sandrock/mynatime/actions/workflows/ci.yml)              |

Pourquoi l'utiliser ?

- hors-ligne en priorité : préparez les changements localement et envoyez-les quand vous êtes connecté
- workflow style git : stagez les activités, révisez-les, faites un commit
- plus rapide que l'interface web pour les opérations courantes
- suivi d'activité simple avec des événements start/stop


Installation
-------------

**Linux** — une seule commande, installe .NET si nécessaire :

```bash
curl -fsSL https://raw.githubusercontent.com/sandrock/mynatime/refs/heads/master/packages/install.sh | bash
```

**Windows** — ouvrez PowerShell :

```powershell
irm https://raw.githubusercontent.com/sandrock/mynatime/refs/heads/master/packages/install.ps1 | iex
```

Les deux installent les commandes `mynatime` et `m`, ainsi qu'une commande `mynatime-update` pour les mises à jour futures.

[📖 Documentation d'installation complète](help/AppCLI.Install-from-sources.fr.md)


Application CLI [📖](help/AppCLI.0Index.fr.md)
-------------

Le CLI est conçu pour une utilisation hors-ligne. Inspiré du [stage git](https://git-scm.com/book/fr/v2/Les-bases-de-Git-Enregistrer-des-modifications-dans-le-d%C3%A9p%C3%B4t), vous pouvez préparer des changements et les publier quand vous êtes connecté.

[📖 Documentation complète de l'application Mynatime CLI](help/AppCLI.0Index.fr.md). Utilisation des commandes :

```
Usage: mynatime [options] [commande] [options de la commande]

Options :

  --config-directory <répertoire>  spécifie le répertoire des profils
  --profile <profil>               spécifie le profil à utiliser
  -p <profil>
  --version, -V                    affiche la version actuelle

Commandes :

  ## Profils
  pro list                         liste les profils utilisateur
  pro add                          crée un profil utilisateur (interactif)

  ## Catégories d'activité
  act cat                          liste les catégories d'activité
  act cat refresh                  met à jour les catégories depuis le service
  act cat search <q>               recherche des catégories

  ## Suivi d'activité
  act start [time] [category]             démarre une activité
  act stop  [time] [category]             arrête une activité
  act status                              liste les activités suivies
  act events                              liste les événements start/stop bruts
  act clear                               supprime tous les événements
  act start/stop [date] [time] [category] vous pouvez spécifier une date

  ## Ajouter une activité
  act add [date] <durée> [catégorie]                         ajoute une activité
  act add <date> <heure-début> [date-fin] <heure-fin> [cat.] ajoute une activité

  ## Révision et publication
  status                           liste tous les changements en attente
  commit                           envoie les changements en attente au service
```

Voir aussi : [liste TODO](TODO.md) pour les commandes prévues.


Compiler depuis les sources
-------------

```bash
dotnet run --project src/Mynatime/Mynatime.csproj
dotnet run --project src/MynatimeGUI/MynatimeGUI.csproj
```

Ou ouvrez le répertoire `src` dans Visual Studio, Rider...


Application GUI
-------------

Expérimentale. Ne fonctionne pas encore.


Contribuer
-------------

Consultez les issues sur GitHub. N'hésitez pas à forker et améliorer.


Ressources
-------------

https://docs.avaloniaui.net/
