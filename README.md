#  Mini Account Management System
The Mini Account Management System is a foundational financial application developed using ASP.NET Core with Razor Pages and MS SQL Server. This project serves as a comprehensive demonstration of key accounting functionalities, robust data management practices, and strict adherence to the use of stored procedures for all database interactions, ensuring no LINQ usage for data access operations. It's designed to provide a solid understanding of how to build a clean, scalable, and secure financial system with a focus on backend robustness and efficient database interaction.

![Mini Account1](Outputs/mini-account1.png)

##  Getting Started
To get a local copy up and running, follow these simple steps:

###  Prerequisites
Ensure you have the following installed on your machine:

-  .NET SDK 8.0 or higher (or the specific version you used)
-  SQL Server (2022 or later recommended)
-  Visual Studio 2022 (Community Edition is fine) or Visual Studio Code

###  Installation

**Clone the repository**:
```Bash
git clone https://github.com/MdALSiyam/Mini-Account-Management-System.git
```
```Bash
cd Mini-Account-Management-System
```

**Restore NuGet Packages**:
```PM
dotnet restore
```

**Update Connection String**:  Open `appsettings.json` in the `Siyam_MiniAccountManagementSystem` project and update the `DefaultConnection` string to point to your SQL Server instance:
```json
  "ConnectionStrings": {
    "DefaultConnection": "Server=(LocalDB)\\MSSQLLocalDB;Database=Siyam_MiniAccountDB10;Trusted_Connection=True;MultipleActiveResultSets=true;"
  },
```

**Create Migrations**
```PM >
Add-Migration InitialSetup
```

**Update the Databse**
```PM >
Update-Database
```

**Tables:**  Open database Siyam_MiniAccountDB10 from SQL Server Object Explorer in Visual Studio 2022. Right click the database and open new query, Copy queries from **Tables.sql** file and paste in that new query and execute one by one.

**Stored Procedures:**  Similarly. Copy and paste queries from **Stored-Procedures.sql** and execute one by one.

**Run Project:**  To run the project, press F5 or click the IIS Express button in Visual Studio 2022.


### Default Users for Testing

For easy testing of different roles and permissions, the application is pre-seeded with the following default users:

|     Role    |      Username        |    Password   |        Accessible Modules      |
| :---------- | :------------------- | :------------ | :----------------------------- |
|  **Admin**  |  `siyam@gmail.com`   |  `Siyam@123`  |  Accounts, Voucher, User/Role  |


##  📸  Project Outputs


###  🧾  Registration as a Viewer
![Mini Account2](Outputs/mini-account2.png)
![Mini Account3](Outputs/mini-account3.png)


###  🧾  Login as a Viewer
![Mini Account4](Outputs/mini-account4.png)


###  🧾  Viewer can access only Home Page and Privacy Page
![Mini Account5](Outputs/mini-account5.png)


###  🧾  But, Viewer can not aceess Account Page and Voucher Page
![Mini Account6](Outputs/mini-account6.png)
![Mini Account7](Outputs/mini-account7.png)


###  🧾  Login as Admin
![Mini Account8](Outputs/mini-account8.png)


**Admin can access all pages including Account Page, Voucher Page etc.**


###  🧾  Account List Page
![Mini Account9](Outputs/mini-account9.png)


###  🧾  Account Details Page
![Mini Account10](Outputs/mini-account10.png)


###  🧾  New Account is Creating
![Mini Account11](Outputs/mini-account11.png)
![Mini Account12](Outputs/mini-account12.png)


###  🧾  Existing Account is Updating
![Mini Account13](Outputs/mini-account13.png)
![Mini Account14](Outputs/mini-account14.png)


###  🧾  Existing Account is Deleting
![Mini Account15](Outputs/mini-account15.png)
![Mini Account16](Outputs/mini-account16.png)


###  🧾  Voucher List Page
![Mini Account17](Outputs/mini-account17.png)


###  🧾  Voucher Details Page
![Mini Account18](Outputs/mini-account18.png)


###  🧾  New Voucher is Creating
![Mini Account19](Outputs/mini-account19.png)
![Mini Account20](Outputs/mini-account20.png)


###  🧾  Existing Voucher is Updating
![Mini Account21](Outputs/mini-account21.png)
![Mini Account22](Outputs/mini-account22.png)


###  🧾  Existing Voucher is Deleting
![Mini Account23](Outputs/mini-account23.png)
