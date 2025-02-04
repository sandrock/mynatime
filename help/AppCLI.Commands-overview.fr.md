
[Index](AppCLI.0Index.fr.md) - [Précédent : Installer et mettre à jour avec les sources](AppCLI.Install-from-sources.fr.md) - [Suite : Se connecter au service](AppCLI.Authenticate.fr.md)

---

Commandes - Documentation de Mynatime CLI
====================================

L'application est construite autour d'instructions en ligne de commande. Chaque commande possède un nom long, plus 
expressif ; et un nom court, plus rapide à saisir au clavier. Utilisez la forme que vous souhaitez.  

L'application va vous aider à effectuer des opérations hors ligne avec le service. Certaine commandes sauvegardent des 
informations sur le poste local, les autres commandes on besoin d'internet pour accéder au service. 

La première commande à apprendre est `help`. Elle liste les commandes disponibles. 

```bash
m help
```

> MynatimeCLI  Mynatime help
>
> Usage: `mynatime` `[app options]` `[command]` `[command options]`
>
> App options:
>
>   `--config-directory <directory>`   specifies the directory to open profiles from  
>   `--profile <profile>`              specifies the profile to use  
>   `-p <profile>`
>
> Commands:
>
>   HelpCommand  
>   `help`                             displays help
>
>   ProfileListCommand  
>   `pro list`                         lists user profiles
>
>   ProfileAddCommand  
>   `pro add`                          adds user profiles
>
>   ActivityCategoryCommand  
>   `act cat`                          lists activity categories  
>   `act cat refresh`                  updates activity categories from service  
>   `act cat search <q>`               searches categories  
>   `act cat alias <name> <alias>`     creates an alias for a category  
>
>   Add activity   
>   `act add [date] <duration> [category]`                         adds an activity  
>   `act add <date> <time-start> [date-end] <time-end> [category]` adds an activity
>
>   Activity tracker  
>   `act start [date] [time] [category]`      starts an activity  
>   `act stop [date] [time] [category]`       stops  an activity   
>   `act status`                       lists current activities
>
>   StatusCommand  
>   `status`                           lists pending changes
>
>   CommitCommand  
>   `commit`                           saves pending changes
>

Après avoir mis à jour l'application, utilisez la commande `help` pour prendre connaissance des nouvelles commandes. 

Si la commande `m` n'est pas disponible, vérifiez l'étape 
[d'installation de mynatime](AppCLI.Install-from-sources.fr.md). 


---

[Index](AppCLI.0Index.fr.md) - [Précédent : Installer et mettre à jour avec les sources](AppCLI.Install-from-sources.fr.md) - [Suite : Se connecter au service](AppCLI.Authenticate.fr.md)
