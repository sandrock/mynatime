
[Index](AppCLI.0Index.fr.md) - Précédent: [Commandes Mynatime](AppCLI.Commands-overview.fr.md) - Suivant : [Suivi d'activité](AppCLI.Tracking-activity.fr.md)

---

Se connecter au service - Documentation de Mynatime CLI
====================================

L'application supporte plusieurs profils utilisateurs. Lorsque vous configurez un profil, un fichier JSON est créé à l'emplacement `~/.config/mynatime/profile.xxxx.json`. Vous n'aurez besoin que d'un seul profil pour une utilisation quotidienne. 

Utilisez l'une de ces commande pour afficher la liste profils utilisateur.

```bash
m profiles
m pro
```

> MynatimeCLI  
No profiles found.

Vous pouvez ajouter un profil avec l'une de ces commandes.

NOTE: votre identifi your username and password will be saved in the profile file to help the app publish information to the service without asking the password each time.

```bash
m profile add
m pro add
```

> MynatimeCLI   
Creating a new profile. Please authenticate.   
Email address> test@test.com  
Password>      ***************  
Processing... OK.   
Profile saved to: /home/me/.config/mynatime/profile.20221009T121214008Z.json

List the profiles again to verify.



---

[Index](AppCLI.0Index.fr.md) - Précédent: [Commandes Mynatime](AppCLI.Commands-overview.fr.md) - Suivant : [Suivi d'activité](AppCLI.Tracking-activity.fr.md)
