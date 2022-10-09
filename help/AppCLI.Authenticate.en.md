
[Index](AppCLI.0Index.en.md) - [Back: Commands overview](AppCLI.Commands-overview.en.md) - [Next: Add an activity](AppCLI.Add-activity.en.md) 

---

Authenticate with service - Mynatime CLI app documentation
====================================

The app supports multiple user profiles. When you set up a user profile, a JSON file will be saved at location `~/.config/mynatime/profile.xxxx.json`. You typically need only one profile for most scenarios.

Use one of these commands to look at the available user profiles.

```bash
mynatime profiles
mynatime pro
```

> MynatimeCLI  
No profiles found.

You can add a profile using one of these commands.  
NOTE: your username and password will be saved in the profile file to help the app publish information to the service without asking the password each time.

```bash
mynatime profile add
mynatime pro add
```

> MynatimeCLI   
Creating a new profile. Please authenticate.   
Email address> test@test.com  
Password>      ***************  
Processing... OK.   
Profile saved to: /home/me/.config/mynatime/profile.20221009T121214008Z.json

List the profiles again to verify.



---

[Index](AppCLI.0Index.en.md) - [Back: Commands overview](AppCLI.Commands-overview.en.md) - [Next: Add an activity](AppCLI.Add-activity.en.md) 
