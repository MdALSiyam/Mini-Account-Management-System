-- Create Database
USE Siyam_MiniAccountDB;
GO

-- Create Chart of Accounts Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ChartOfAccounts' and xtype='U')
CREATE TABLE ChartOfAccounts (
    AccountId INT PRIMARY KEY IDENTITY(1,1),
    AccountCode NVARCHAR(50) UNIQUE NOT NULL,
    AccountName NVARCHAR(255) NOT NULL,
    AccountType NVARCHAR(50) NOT NULL, 
    ParentAccountId INT NULL,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    UpdatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ParentAccountId) REFERENCES ChartOfAccounts(AccountId)
);
GO

-- Create Vouchers Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Vouchers' and xtype='U')
CREATE TABLE Vouchers (
    VoucherId INT PRIMARY KEY IDENTITY(1,1),
    VoucherDate DATE NOT NULL,
    ReferenceNo NVARCHAR(100) UNIQUE NOT NULL,
    VoucherType NVARCHAR(50) NOT NULL, -- e.g., Journal, Payment, Receipt
    Narration NVARCHAR(MAX) NULL,
    CreatedBy NVARCHAR(255) NOT NULL,
    CreatedDate DATETIME DEFAULT GETDATE(),
    UpdatedBy NVARCHAR(255) NULL,
    UpdatedDate DATETIME NULL
);
GO

-- Create VoucherDetails Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='VoucherDetails' and xtype='U')
CREATE TABLE VoucherDetails (
    VoucherDetailId INT PRIMARY KEY IDENTITY(1,1),
    VoucherId INT NOT NULL,
    AccountId INT NOT NULL,
    Debit DECIMAL(18, 2) DEFAULT 0,
    Credit DECIMAL(18, 2) DEFAULT 0,
    FOREIGN KEY (VoucherId) REFERENCES Vouchers(VoucherId),
    FOREIGN KEY (AccountId) REFERENCES ChartOfAccounts(AccountId)
);
GO

-- Create ModulePermissions Table
CREATE TABLE ModulePermissions (
    PermissionId INT PRIMARY KEY IDENTITY(1,1),
    RoleId NVARCHAR(450) NOT NULL,
    ModuleName NVARCHAR(100) NOT NULL,
    CanView BIT DEFAULT 0,
    CanCreate BIT DEFAULT 0,
    CanEdit BIT DEFAULT 0,
    CanDelete BIT DEFAULT 0,
    FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id),
    UNIQUE (RoleId, ModuleName)
);
GO

-- Stored procedure to manage module permissions (e.g., sp_ManageModulePermissions)
-- This would be called by Admin UI to set permissions per role.

-- Add indexes for performance
CREATE INDEX IX_Vouchers_VoucherDate ON Vouchers (VoucherDate);
CREATE INDEX IX_VoucherDetails_VoucherId ON VoucherDetails (VoucherId);
CREATE INDEX IX_VoucherDetails_AccountId ON VoucherDetails (AccountId);
CREATE INDEX IX_ChartOfAccounts_ParentAccountId ON ChartOfAccounts (ParentAccountId);
GO