USE Siyam_MiniAccountDB10
GO;

CREATE TABLE [dbo].[ChartOfAccounts] (
    [AccountId]       INT            IDENTITY (1, 1) NOT NULL,
    [AccountCode]     NVARCHAR (50)  NOT NULL,
    [AccountName]     NVARCHAR (255) NOT NULL,
    [AccountType]     NVARCHAR (50)  NOT NULL,
    [ParentAccountId] INT            NULL,
    [IsActive]        BIT            DEFAULT ((1)) NOT NULL,
    [CreatedBy]       NVARCHAR (255) NULL,
    [CreatedDate]     DATETIME       DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]       NVARCHAR (255) NULL,
    [UpdatedDate]     DATETIME       NULL,
    [Level]           INT            NOT NULL,
    [DisplayName]     NVARCHAR (500) NOT NULL,
    PRIMARY KEY CLUSTERED ([AccountId] ASC)
);

CREATE TABLE [dbo].[Vouchers] (
    [VoucherId]   INT             IDENTITY (1, 1) NOT NULL,
    [VoucherDate] DATE            NOT NULL,
    [ReferenceNo] NVARCHAR (100)  NOT NULL,
    [VoucherType] NVARCHAR (50)   NOT NULL,
    [Narration]   NVARCHAR (MAX)  NULL,
    [TotalDebit]  DECIMAL (18, 2) NOT NULL,
    [TotalCredit] DECIMAL (18, 2) NOT NULL,
    [CreatedBy]   NVARCHAR (255)  NULL,
    [CreatedDate] DATETIME        DEFAULT (getdate()) NOT NULL,
    [UpdatedBy]   NVARCHAR (255)  NULL,
    [UpdatedDate] DATETIME        NULL,
    PRIMARY KEY CLUSTERED ([VoucherId] ASC)
);

CREATE TABLE [dbo].[VoucherDetails] (
    [VoucherDetailId] INT             IDENTITY (1, 1) NOT NULL,
    [VoucherId]       INT             NOT NULL,
    [AccountId]       INT             NOT NULL,
    [Debit]           DECIMAL (18, 2) NOT NULL,
    [Credit]          DECIMAL (18, 2) NOT NULL,
    PRIMARY KEY CLUSTERED ([VoucherDetailId] ASC),
    CONSTRAINT [FK_VoucherDetails_Vouchers] FOREIGN KEY ([VoucherId]) REFERENCES [dbo].[Vouchers] ([VoucherId]) ON DELETE CASCADE,
    CONSTRAINT [FK_VoucherDetails_ChartOfAccounts] FOREIGN KEY ([AccountId]) REFERENCES [dbo].[ChartOfAccounts] ([AccountId])
);