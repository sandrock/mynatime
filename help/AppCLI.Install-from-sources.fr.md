---
title: Installer et mettre à jour
parent: Documentation (Français)
nav_order: 1
---

Installer l'application Mynatime CLI
====================================

🇬🇧 [English version](AppCLI.Install-from-sources.en.md)

NOTE : des tâches sont en cours pour publier l'application sur [winget](https://github.com/sandrock/mynatime/issues/8) et [flatpak](https://github.com/sandrock/mynatime/issues/7)


Installer depuis un binaire pré-compilé (recommandé)
-----------------------------

La méthode la plus simple. Le script installe .NET si nécessaire.


### Linux

**Avec sudo** (installation système dans `/usr/local/`) :

```bash
curl -fsSL https://raw.githubusercontent.com/sandrock/mynatime/refs/heads/master/packages/install.sh | sudo bash
```

**Sans sudo** (installation dans `~/.local/`) :

```bash
curl -fsSL https://raw.githubusercontent.com/sandrock/mynatime/refs/heads/master/packages/install.sh | bash
```

Les deux commandes créent deux alias : `mynatime` et `m`.

Vérifiez l'installation :

```bash
m help
```

**Plus d'options :**

```bash
# installer la dernière pré-version
curl -fsSL https://raw.githubusercontent.com/sandrock/mynatime/refs/heads/master/packages/install.sh | bash -s -- --prerelease

# installation système, sans confirmation
curl -fsSL https://raw.githubusercontent.com/sandrock/mynatime/refs/heads/master/packages/install.sh | sudo bash -s -- --system --yes

# afficher toutes les options sans installer
curl -fsSL https://raw.githubusercontent.com/sandrock/mynatime/refs/heads/master/packages/install.sh | bash -s -- --help
```


### Windows

Fonctionne sur Windows x86-64. Le script installe .NET si nécessaire. Ouvrez **PowerShell** et exécutez :

```powershell
irm https://raw.githubusercontent.com/sandrock/mynatime/refs/heads/master/packages/install.ps1 | iex
```

Cela installe dans `%LOCALAPPDATA%\mynatime\` et ajoute le répertoire au PATH utilisateur. Les commandes `mynatime` et `m` seront disponibles après avoir redémarré votre terminal.

**Plus d'options :**

```powershell
# installation système (nécessite PowerShell en tant qu'administrateur)
irm https://raw.githubusercontent.com/sandrock/mynatime/refs/heads/master/packages/install.ps1 | iex -- -System

# installer la dernière pré-version
irm https://raw.githubusercontent.com/sandrock/mynatime/refs/heads/master/packages/install.ps1 | iex -- -Prerelease

# sans confirmation
irm https://raw.githubusercontent.com/sandrock/mynatime/refs/heads/master/packages/install.ps1 | iex -- -Yes
```

Si vous obtenez une erreur de politique d'exécution, lancez d'abord :

```powershell
Set-ExecutionPolicy RemoteSigned -Scope CurrentUser
```


Mettre à jour depuis un binaire pré-compilé
-----------------------------

Maintenez l'application à jour car le service peut modifier son API à tout moment.

**Linux :**

```bash
mynatime-update
```

**Windows :**

```powershell
mynatime-update
```


Installer depuis les sources
-----------------------------

Utilisez cette méthode si vous souhaitez travailler sur le code ou si aucun binaire pré-compilé n'est disponible pour votre plateforme.

**Prérequis** — installez depuis votre gestionnaire de paquets :

- dotnet-sdk (>=8)
- dotnet-runtime (>=8)

1. Clonez le dépôt

```bash
mkdir ~/.opt
git clone git@github.com:sandrock/mynatime.git ~/.opt/mynatime
cd ~/.opt/mynatime
```

2. Compilez

```bash
dotnet build src/Mynatime.sln -c Release -v q
```

3. Créez un alias et vérifiez

```bash
alias "m=~/.opt/mynatime/src/Mynatime/bin/Release/net8.0/Mynatime.CLI"
m help
```

4. Ajoutez l'alias à votre `~/.bashrc` / `~/.zshrc` pour le rendre permanent.


Mettre à jour depuis les sources
-----------------------------

```bash
cd ~/.opt/mynatime
git pull
dotnet build src/Mynatime.sln -c Release -v q
```
