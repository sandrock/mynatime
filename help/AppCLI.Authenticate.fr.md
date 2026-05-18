---
title: Se connecter au service
parent: Documentation (Français)
nav_order: 3
---

Se connecter au service
====================================

🇬🇧 [English version](AppCLI.Authenticate.en.md)

L'application supporte plusieurs profils utilisateur. Quand vous créez un profil, un fichier JSON est sauvegardé à l'emplacement `~/.config/mynatime/profile.xxxx.json`. Pour la plupart des cas, un seul profil suffit.

Utilisez l'une de ces commandes pour lister les profils disponibles.

```bash
m profiles
m pro
```

> MynatimeCLI  
> No profiles found.

Vous pouvez ajouter un profil avec l'une de ces commandes.

NOTE : votre identifiant et votre mot de passe seront sauvegardés dans le fichier de profil pour permettre à l'application de publier des informations sur le service sans demander le mot de passe à chaque fois.

```bash
m profile add
m pro add
```

> MynatimeCLI  
> Creating a new profile. Please authenticate.  
> Email address> test@test.com  
> Password>      ***************  
>  
> Authentication successful.  
>  
> Profile options:  
>  
>   Prompt before saving the profile locally. Recommended for development purposes.  
>   Enable local save confirmation? [y/N]  
>  
>   Prompt before sending each transaction item to the service.  
>   Enable service save confirmation? [Y/n]  
>  
> These settings can be changed at any time in the profile file.  
>  
> Profile saved to: /home/me/.config/mynatime/profile.20221009T121214008Z.json

Listez à nouveau les profils pour vérifier.

Une fois le profil créé, récupérez les catégories d'activité depuis le service — cette étape est nécessaire avant de pouvoir utiliser les noms de catégories dans les commandes :

```bash
m act cat refresh
```


Vérifier le statut du profil
-----------------------------------

Une fois un profil chargé, vous pouvez inspecter son contenu.

```bash
m profile status
m pro status
```

> Profile:    profile.20221009T121214008Z.json  
>   File:     /home/me/.config/mynatime/profile.20221009T121214008Z.json  
>   Username: test@test.com  
>   Name:     John DOE  
>   User ID:  12345  
>   Roles:    ROLE_USER_MT, ROLE_MANAGER_MT  
>   Session:  active (2 cookies)  
>   Categories: 12 categories, last refreshed 2022-10-01  
>   Pending:    0 items  
>   Commits:    3 items  

La ligne **Session** indique si l'application dispose d'une session valide avec le service. Si elle affiche `none`, l'application tentera de s'authentifier automatiquement en cas de besoin.

La ligne **Categories** affiche les catégories d'activité téléchargées depuis le service. Si elle affiche `none`, exécutez d'abord `m act cat refresh`.

La ligne **Pending** compte les activités enregistrées localement mais pas encore publiées. Exécutez `m status` pour les détails et `m commit` pour publier.
