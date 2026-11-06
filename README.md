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
   After running the prepare script, navigate to the **Server** folder. Open the `openrp.config.json` file and fill in your database information. You can also fill in the rest, but it's not required. 
   **Tip:** I personally use XAMPP to host a local database. If you choose another solution, I recommend at least using MariaDB.

5. **Run a Fresh Database Migration**  
   Ensure your database is running, then execute the `FreshMigration.ps1` script. This script performs a fresh migration of your database models, ideal for when you're updating the schema and want to avoid migration headaches (but at the cost of losing all present database data).

---

## When Your Server Has Real Data

`FreshMigration.ps1` deletes the whole database, so stop using it once your server has data you want to keep (players, characters, items). From then on, every change to the database goes through a migration:

1. **Start tracking your migrations.** Remove `/src/OpenRP.Boilerplate/Migrations` from `.gitignore` and commit the `Migrations` folder. Your database already matches its last migration, so nothing has to run.
2. **Add a migration for every change to the database.** After updating OpenRP.Framework or changing a model, run this from `src/OpenRP.Boilerplate`:
   ```powershell
   dotnet ef migrations add <DescriptiveName>
   ```
   Read the generated migration before committing it. EF sometimes drops and re-adds a column when you meant to rename it. Commit the migration together with the change that caused it.
3. **Apply it.** On your own development database, run `dotnet ef database update`. For a live server, generate a script, check it, and run it during your update:
   ```powershell
   dotnet ef migrations script --idempotent --output update.sql
   ```
4. **Working with others:**
   - Never edit or delete a migration that's already been applied to a shared database. Add a new one instead.
   - If you and someone else both added a migration, whoever merges second runs `dotnet ef migrations remove`, merges, and then runs `migrations add` again. That keeps the migrations' snapshot of the database correct.
5. **Seed data:** use `HasDefaultData(...)` instead of `HasData(...)`. It gives seed rows fixed timestamps, so new migrations don't keep updating them.
