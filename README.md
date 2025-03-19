# OpenRP.Boilerplate

**OpenRP.Boilerplate** is a boilerplate made to make it easy for contributors to test their code on a Windows environment. It streamlines the process by setting up a complete server environment with the open.mp server, SampSharp, the .NET runtime, and all required plug-ins.

---

## Features

- **Automated Setup:** Downloads and installs the open.mp server, SampSharp, .NET runtime, and plug-ins.
- **Windows Optimized:** Tailored for a smooth development experience on Windows.
- **Pre-configured Server Folder:** Easily locate and configure your server settings.
- **Simplified Database Migrations:** Quickly reset your database when making model changes without complex migrations.

---

## How to Set Up

1. **Clone the Repository**  
   ```bash
   git clone https://github.com/OpenRoleplay/OpenRP.Boilerplate.git
   ```

3. **Run the Prepare Script**  
   Execute the `Prepare.ps1` script. This will:
   - Download the open.mp server.
   - Download SampSharp.
   - Install the .NET runtime.
   - Download all necessary plug-ins.
   - Launch the ColAndreas wizard, **make sure to point it to your GTA SA installation.**

4. **Configure Your Server**  
   After running the prepare script, navigate to the **Server** folder. Open the `openrp.config.json` file and fill in your database information.  
   **Tip:** I personally use XAMPP. If you choose another solution, I recommend at least using MariaDB.

5. **Run a Fresh Database Migration**  
   Ensure your database is running, then execute the `FreshMigration.ps1` script. This script performs a fresh migration of your database models, ideal for when you're updating the schema and want to avoid migration headaches (but at the cost of losing all present database data).
