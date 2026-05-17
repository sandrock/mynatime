
[Index](AppCLI.0Index.fr.md) - Retour : [Ajouter une activité](AppCLI.Add-activity.fr.md) - Suite : pas de suite

---

Trucs et astuces - Documentation de Mynatime CLI
====================================

Utilisation quotidienne
---------------------

### Défaire une action avant de publier

Il arrive que l'on fasse une erreur quand on exécute une commande. L'application ne propose pas (pour le moment) d'annuler une commande. Toutefois, il est possible de modifier manuellement les changements en attente.

Ouvrez le fichier JSON de votre profil utilisateur. Cherchez l'élément `Transaction`. Il contient les données en attente de publication. Chaque type de donnée contient sa représentation de données propre. Vous devriez pouvoir bidouiller les données pour avoir l'effet désiré (changer une date, une activité…). Vous pouvez aussi supprimer complètement un élément de transaction.  

Utilisez la commande `m status` pour vérifier que le fichier est correct. 


### Rechercher une saisie passée


Ouvrez le fichier JSON de votre profil utilisateur. Cherchez l'élément `Commits`. Il contient les données publiées dans le passé ; celles-ci sont groupées par publication (`commit`). 


### Alias de catégorie d'activité #15

Lorsque vous utilisez le suivi d'activité, certaines catégories peuvent être difficiles à saisir car leur nom est complexe ou long. 

```bash
m act start interne
Too many possibilities for category "interne": "Company-interne", "Customer2-interne", "Meeting-interne"
```

Vous pouvez créer un alias de cette façon :

```bash
m act cat alias "Company-interne" interne
```

Puis utiliser l'alias au lieu du nom :

```bash
m act start interne
```


Développeurs
---------------------

This section is not translated because developers speak english. See [english page](AppCLI.Tips-tricks.en.md)


---

[Index](AppCLI.0Index.fr.md) - Retour : [Ajouter une activité](AppCLI.Add-activity.fr.md) - Suite : pas de suite
