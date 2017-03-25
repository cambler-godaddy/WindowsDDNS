This project is a Windows Service that handles Dynamic DNS.

The main solution has three projects:

1. GoDaddyDNSUpdate

This is the NT Service. It performs dynamic DNS by periodically checking the public IP of the server it is running on, and if
different than what GoDaddy's DNS thinks is the IP will use the GoDaddy API to update the IP.

All configuration is stored in the registry.

This includes the domain, an optional sub-domain, and the GoDaddy API Key/Secret.

2. Tester

This is a simple command line testing program to exercise the core functionality of the service without running the service.

3. gddnsserviceuserinfo

This is a simple windows form program that collect the settings values and writes them into the registry.  It could be run standalone, but is used in the installer.

4. Setup2

This is a Visual Studio install shield project for the service installer.

5. assetsforinstaller

These are various assets (bitmaps, icons, eula) for the installer.