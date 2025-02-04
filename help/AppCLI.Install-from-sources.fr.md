
[Back: Index](AppCLI.0Index.fr.md) - [Suite : Commandes Mynatime](AppCLI.Commands-overview.fr.md)

---

Installer et mettre à jour avec les sources - Documentation de Mynatime CLI
====================================

Obtenir les outils
-----------------------------

Installer ceci avec votre gestionnaire d'applications :

- dotnet-sdk (>=6)
- dotnet-runtime (>=6)

NOTE: il y a des travaux à réaliser pour permettre l'installation facile à l'aide de [winget](https://github.com/sandrock/mynatime/issues/8) et [flatpak](https://github.com/sandrock/mynatime/issues/7)


À propos des systèmes d'exploitation
-----------------------------

Ces commandes sont pour les utilisateurs de GNU+Linux. 

Utilisateur de Windows : utilisez *git bash* ou WSL ou powershell ou anything else.


Installer depuis les sources
-----------------------------

1. Clonez le repository  
```bash
mkdir ~/.opt
git clone git@github.com:sandrock/mynatime.git ~/.opt/mynatime
cd ~/.opt/mynatime
```
2. Compilez
```bash
dotnet build src/Mynatime.sln -c Release -v q
```
3. Créez un alias et le vérifier
```bash
alias "m=~/.opt/mynatime/src/Mynatime/bin/Release/net6.0/Mynatime.CLI"
which m
m help
```
4. Ajouter l'alias à `~/.bashrc` / `~/.zshrc`


Mettre à jour depuis les sources
-----------------------------

Veuillez garder l'application à jour car l'API du service change régulièrement. 

1. Mettre à jour les sources
```bash
cd ~/.opt/mynatime
git pull
```
2. Compilez
```bash
dotnet build src/Mynatime.sln -c Release -v q
```

---

[Back: Index](AppCLI.0Index.fr.md) - [Suite : Commandes Mynatime](AppCLI.Commands-overview.fr.md)
