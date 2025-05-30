USE Siyam_MiniAccountDB;
GO

IF OBJECT_ID('sp_ManageChartOfAccounts', 'P') IS NOT NULL
    DROP PROCEDURE sp_ManageChartOfAccounts;
GO

CREATE PROCEDURE sp_ManageChartOfAccounts
    @Action NVARCHAR(10),
    @AccountId INT = NULL,
    @AccountCode NVARCHAR(50) = NULL,
    @AccountName NVARCHAR(255) = NULL,
    @AccountType NVARCHAR(50) = NULL,
    @ParentAccountId INT = NULL,
    @IsActive BIT = NULL
AS
BEGIN
    IF @Action = 'Insert'
    BEGIN
        INSERT INTO ChartOfAccounts (AccountCode, AccountName, AccountType, ParentAccountId, IsActive)
        VALUES (@AccountCode, @AccountName, @AccountType, @ParentAccountId, ISNULL(@IsActive, 1));
        SELECT SCOPE_IDENTITY() AS NewAccountId;
    END
    ELSE IF @Action = 'Update'
    BEGIN
        UPDATE ChartOfAccounts
        SET
            AccountCode = ISNULL(@AccountCode, AccountCode),
            AccountName = ISNULL(@AccountName, AccountName),
            AccountType = ISNULL(@AccountType, AccountType),
            ParentAccountId = @ParentAccountId, -- Allow NULL to be set
            IsActive = ISNULL(@IsActive, IsActive),
            UpdatedDate = GETDATE()
        WHERE AccountId = @AccountId;
        SELECT @AccountId AS UpdatedAccountId;
    END
    ELSE IF @Action = 'Delete'
    BEGIN
        -- Check for child accounts first
        IF EXISTS (SELECT 1 FROM ChartOfAccounts WHERE ParentAccountId = @AccountId)
        BEGIN
            RAISERROR('Cannot delete account with child accounts.', 16, 1);
            RETURN;
        END

        -- Check for associated voucher details
        IF EXISTS (SELECT 1 FROM VoucherDetails WHERE AccountId = @AccountId)
        BEGIN
            RAISERROR('Cannot delete account with associated voucher entries.', 16, 1);
            RETURN;
        END

        DELETE FROM ChartOfAccounts WHERE AccountId = @AccountId;
        SELECT @AccountId AS DeletedAccountId;
    END
    ELSE IF @Action = 'Select'
    BEGIN
        SELECT
            coa.AccountId,
            coa.AccountCode,
            coa.AccountName,
            coa.AccountType,
            coa.ParentAccountId,
            ParentAccount.AccountName AS ParentAccountName,
            coa.IsActive,
            coa.CreatedDate,
            coa.UpdatedDate
        FROM ChartOfAccounts coa
        LEFT JOIN ChartOfAccounts ParentAccount ON coa.ParentAccountId = ParentAccount.AccountId
        WHERE (@AccountId IS NULL OR coa.AccountId = @AccountId)
          AND (@AccountCode IS NULL OR coa.AccountCode LIKE '%' + @AccountCode + '%')
          AND (@AccountName IS NULL OR coa.AccountName LIKE '%' + @AccountName + '%')
        ORDER BY coa.AccountCode;
    END
    ELSE IF @Action = 'SelectFlat' -- For dropdowns
    BEGIN
        SELECT AccountId, AccountCode, AccountName, AccountType
        FROM ChartOfAccounts
        WHERE IsActive = 1
        ORDER BY AccountCode;
    END
    ELSE IF @Action = 'SelectHierarchy' -- For hierarchical display
    BEGIN
        WITH AccountHierarchy AS (
            SELECT
                AccountId,
                AccountCode,
                AccountName,
                ParentAccountId,
                1 AS Level,
                CAST(AccountCode AS NVARCHAR(MAX)) AS SortPath
            FROM ChartOfAccounts
            WHERE ParentAccountId IS NULL -- Root accounts

            UNION ALL

            SELECT
                coa.AccountId,
                coa.AccountCode,
                coa.AccountName,
                coa.ParentAccountId,
                ah.Level + 1 AS Level,
                CAST(ah.SortPath + '.' + coa.AccountCode AS NVARCHAR(MAX)) AS SortPath
            FROM ChartOfAccounts coa
            JOIN AccountHierarchy ah ON coa.ParentAccountId = ah.AccountId
        )
        SELECT
            AccountId,
            AccountCode,
            AccountName,
            ParentAccountId,
            Level,
            REPLICATE('  ', Level - 1) + AccountName AS DisplayName
        FROM AccountHierarchy
        ORDER BY SortPath;
    END
END;
GO