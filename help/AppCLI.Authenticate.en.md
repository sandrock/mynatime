---
title: Authenticate with service
parent: Documentation
nav_order: 3
---

# Authenticate with service

The app supports multiple user profiles. When you set up a user profile, a JSON file will be saved at location `~/.config/mynatime/profile.xxxx.json`. You typically need only one profile for most scenarios.

Use one of these commands to look at the available user profiles.

```bash
m profiles
m pro
```

> MynatimeCLI  
No profiles found.

You can add a profile using one of these commands.  
NOTE: your username and password will be saved in the profile file to help the app publish information to the service without asking the password each time.

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

List the profiles again to verify.

Once the profile is created, pull the activity categories from the service — this is required before you can use category names in commands:

```bash
m act cat refresh
```


Checking profile status
-----------------------------------

Once a profile is loaded, you can inspect what it contains.

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

The **Session** line tells you whether the app has a valid session with the service. If it shows `none`, the app will try to authenticate automatically when needed.

The **Categories** line shows the activity categories downloaded from the service. If it shows `none`, run `m act cat refresh` first.

The **Pending** line counts activities staged locally but not yet published. Run `m status` for details and `m commit` to publish.
