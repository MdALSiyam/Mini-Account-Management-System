USE Siyam_MiniAccountDB10
GO;

CREATE PROCEDURE [sp_ManageChartOfAccounts]
    @AccountId INT = NULL,
    @AccountCode NVARCHAR(50) = NULL,
    @AccountName NVARCHAR(255) = NULL,
    @AccountType NVARCHAR(50) = NULL,
    @ParentAccountId INT = NULL, 
    @IsActive BIT = NULL,
    @CreatedBy NVARCHAR(255) = NULL,
    @UpdatedBy NVARCHAR(255) = NULL,
    @OperationType NVARCHAR(20) 
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TranCount INT;

    SELECT @TranCount = @@TRANCOUNT;

    IF @OperationType IN ('Insert', 'Update', 'Delete')
    BEGIN
        IF @TranCount = 0
        BEGIN
            BEGIN TRANSACTION;
        END
        ELSE
        BEGIN
            SAVE TRANSACTION ChartOfAccountsTransaction;
        END
    END

    BEGIN TRY
        DECLARE @NewAccountId INT;

        IF @OperationType = 'Insert'
        BEGIN
            DECLARE @Level INT;
            IF @ParentAccountId IS NULL OR @ParentAccountId = 0
            BEGIN
                SET @Level = 0;
            END
            ELSE
            BEGIN
                SELECT @Level = Level + 1 FROM ChartOfAccounts WHERE AccountId = @ParentAccountId;
            END
            DECLARE @DisplayName NVARCHAR(300);
            IF @Level = 0
            BEGIN
                SET @DisplayName = @AccountName;
            END
            ELSE
            BEGIN
                DECLARE @ParentDisplayName NVARCHAR(300);
                SELECT @ParentDisplayName = DisplayName FROM ChartOfAccounts WHERE AccountId = @ParentAccountId;
                SET @DisplayName = @ParentDisplayName + ' -> ' + @AccountName;
            END

            INSERT INTO ChartOfAccounts (
                AccountCode,
                AccountName,
                AccountType,
                ParentAccountId,
                IsActive,
                CreatedBy,
                CreatedDate,
                Level,
                DisplayName
            )
            VALUES (
                @AccountCode,
                @AccountName,
                @AccountType,
                @ParentAccountId,
                @IsActive,
                @CreatedBy,
                GETDATE(),
                @Level,
                @DisplayName
            );

            SET @NewAccountId = SCOPE_IDENTITY();
            SELECT @NewAccountId AS AccountId;

        END
        ELSE IF @OperationType = 'Update'
        BEGIN

            IF @AccountId IS NULL
            BEGIN
                RAISERROR('AccountId is required for Update operation.', 16, 1);
                RETURN -1;
            END
            DECLARE @CurrentLevel INT, @CurrentParentAccountId INT, @CurrentAccountName NVARCHAR(255);
            SELECT @CurrentLevel = Level, @CurrentParentAccountId = ParentAccountId, @CurrentAccountName = AccountName
            FROM ChartOfAccounts WHERE AccountId = @AccountId;

            DECLARE @UpdatedLevel INT = @CurrentLevel;
            DECLARE @UpdatedDisplayName NVARCHAR(300);

            IF (@ParentAccountId IS NOT NULL AND @ParentAccountId <> @CurrentParentAccountId) OR
               (@ParentAccountId IS NULL AND @CurrentParentAccountId IS NOT NULL) OR
               (@AccountName IS NOT NULL AND @AccountName <> @CurrentAccountName)
            BEGIN
                IF @ParentAccountId IS NULL OR @ParentAccountId = 0
                BEGIN
                    SET @UpdatedLevel = 0;
                END
                ELSE
                BEGIN
                    SELECT @UpdatedLevel = Level + 1 FROM ChartOfAccounts WHERE AccountId = @ParentAccountId;
                END

                DECLARE @NewParentDisplayName NVARCHAR(300);
                IF @UpdatedLevel = 0
                BEGIN
                    SET @UpdatedDisplayName = ISNULL(@AccountName, @CurrentAccountName);
                END
                ELSE
                BEGIN
                    SELECT @NewParentDisplayName = DisplayName FROM ChartOfAccounts WHERE AccountId = @ParentAccountId;
                    SET @UpdatedDisplayName = @NewParentDisplayName + ' -> ' + ISNULL(@AccountName, @CurrentAccountName);
                END
                UPDATE ChartOfAccounts
                SET
                    AccountCode = ISNULL(@AccountCode, AccountCode),
                    AccountName = ISNULL(@AccountName, AccountName),
                    AccountType = ISNULL(@AccountType, AccountType),
                    ParentAccountId = ISNULL(@ParentAccountId, ParentAccountId),
                    IsActive = ISNULL(@IsActive, IsActive),
                    UpdatedBy = @UpdatedBy,
                    UpdatedDate = GETDATE(),
                    Level = @UpdatedLevel,
                    DisplayName = @UpdatedDisplayName
                WHERE
                    AccountId = @AccountId;
            END
            ELSE
            BEGIN
                UPDATE ChartOfAccounts
                SET
                    AccountCode = ISNULL(@AccountCode, AccountCode),
                    AccountName = ISNULL(@AccountName, AccountName),
                    AccountType = ISNULL(@AccountType, AccountType),
                    IsActive = ISNULL(@IsActive, IsActive),
                    UpdatedBy = @UpdatedBy,
                    UpdatedDate = GETDATE()
                WHERE
                    AccountId = @AccountId;
            END

            SELECT @AccountId AS AccountId;

        END
        ELSE IF @OperationType = 'Delete'
        BEGIN
            IF EXISTS (SELECT 1 FROM ChartOfAccounts WHERE ParentAccountId = @AccountId)
            BEGIN
                RAISERROR('Cannot delete account because it has child accounts.', 16, 1);
                RETURN -1;
            END
            IF EXISTS (SELECT 1 FROM VoucherDetails WHERE AccountId = @AccountId)
            BEGIN
                RAISERROR('Cannot delete account because it is used in existing Voucher Details.', 16, 1);
                RETURN -1;
            END

            DELETE FROM ChartOfAccounts
            WHERE AccountId = @AccountId;

            SELECT @AccountId AS AccountId;
        END
        ELSE IF @OperationType = 'SelectFlat'
        BEGIN
            SELECT
                coa.AccountId,
                coa.AccountCode,
                coa.AccountName,
                coa.AccountType,
                coa.ParentAccountId,
                Parent.AccountName AS ParentAccountName,
                coa.IsActive,
                coa.CreatedBy,
                coa.CreatedDate,
                coa.UpdatedBy,
                coa.UpdatedDate,
                coa.Level,
                coa.DisplayName
            FROM
                ChartOfAccounts coa
            LEFT JOIN
                ChartOfAccounts Parent ON coa.ParentAccountId = Parent.AccountId
            ORDER BY
                coa.AccountCode;
        END
        ELSE IF @OperationType = 'SelectHierarchy'
        BEGIN
            WITH AccountCTE AS
            (
                SELECT
                    AccountId,
                    AccountCode,
                    AccountName,
                    AccountType,
                    ParentAccountId,
                    CAST(NULL AS NVARCHAR(255)) AS ParentAccountName,
                    IsActive,
                    CreatedBy,
                    CreatedDate,
                    UpdatedBy,
                    UpdatedDate,
                    Level,
                    DisplayName,
                    CAST(AccountCode AS NVARCHAR(MAX)) AS SortPath
                FROM
                    ChartOfAccounts
                WHERE
                    ParentAccountId IS NULL OR ParentAccountId = 0

                UNION ALL
                SELECT
                    coa.AccountId,
                    coa.AccountCode,
                    coa.AccountName,
                    coa.AccountType,
                    coa.ParentAccountId,
                    cte.AccountName AS ParentAccountName,
                    coa.IsActive,
                    coa.CreatedBy,
                    coa.CreatedDate,
                    coa.UpdatedBy,
                    coa.UpdatedDate,
                    coa.Level,
                    coa.DisplayName,
                    CAST(cte.SortPath + '.' + coa.AccountCode AS NVARCHAR(MAX)) AS SortPath
                FROM
                    ChartOfAccounts coa
                INNER JOIN
                    AccountCTE cte ON coa.ParentAccountId = cte.AccountId
            )
            SELECT
                AccountId,
                AccountCode,
                AccountName,
                AccountType,
                ParentAccountId,
                ParentAccountName,
                IsActive,
                CreatedBy,
                CreatedDate,
                UpdatedBy,
                UpdatedDate,
                Level,
                DisplayName
            FROM
                AccountCTE
            ORDER BY
                SortPath;
        END
        ELSE IF @OperationType = 'SelectOne'
        BEGIN
            IF @AccountId IS NULL
            BEGIN
                 RAISERROR('AccountId is required for SelectOne operation.', 16, 1);
                 RETURN -1;
            END

            SELECT
                coa.AccountId,
                coa.AccountCode,
                coa.AccountName,
                coa.AccountType,
                coa.ParentAccountId,
                Parent.AccountName AS ParentAccountName,
                coa.IsActive,
                coa.CreatedBy,
                coa.CreatedDate,
                coa.UpdatedBy,
                coa.UpdatedDate,
                coa.Level,
                coa.DisplayName
            FROM
                ChartOfAccounts coa
            LEFT JOIN
                ChartOfAccounts Parent ON coa.ParentAccountId = Parent.AccountId
            WHERE
                coa.AccountId = @AccountId;
        END
        ELSE
        BEGIN
            RAISERROR('Invalid OperationType specified.', 16, 1);
            RETURN -1;
        END
        IF @OperationType IN ('Insert', 'Update', 'Delete') AND @TranCount = 0
        BEGIN
            COMMIT TRANSACTION;
        END

    END TRY
    BEGIN CATCH
        IF @OperationType IN ('Insert', 'Update', 'Delete')
        BEGIN
            IF @TranCount = 0
            BEGIN
                IF @@TRANCOUNT > 0
                    ROLLBACK TRANSACTION;
            END
            ELSE
            BEGIN
                IF XACT_STATE() <> 0 AND @@TRANCOUNT > @TranCount -- Check if transaction is committable/rollbackable
                    ROLLBACK TRANSACTION ChartOfAccountsTransaction; -- Rollback to savepoint
            END
        END
        DECLARE @ErrorMessage NVARCHAR(MAX) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();

        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO;


CREATE PROCEDURE sp_GetAccountsFlat
AS
BEGIN
    SELECT
        ca.AccountId,
        ca.AccountCode,
        ca.AccountName,
        ca.AccountType,
        ca.ParentAccountId,
        pa.AccountName AS ParentAccountName,
        ca.IsActive,
        ca.CreatedBy,
        ca.CreatedDate,
        ca.UpdatedBy,
        ca.UpdatedDate,
        ca.Level,
        ca.DisplayName
    FROM
        ChartOfAccounts ca
    LEFT JOIN
        ChartOfAccounts pa ON ca.ParentAccountId = pa.AccountId
    ORDER BY
        ca.AccountCode;
END;
GO;


CREATE PROCEDURE [sp_RecalculateChartOfAccountHierarchy]
AS
BEGIN
    SET NOCOUNT ON;
    WITH AccountHierarchyRecalculator (AccountId, ParentAccountId, AccountName, Level, DisplayName)
    AS
    (
        SELECT
            coa.AccountId,
            coa.ParentAccountId,
            coa.AccountName,
            1 AS Level,
            CAST(coa.AccountName AS NVARCHAR(MAX)) AS DisplayName
        FROM
            ChartOfAccounts coa
        WHERE
            coa.ParentAccountId IS NULL

        UNION ALL
        SELECT
            coa.AccountId,
            coa.ParentAccountId,
            coa.AccountName,
            ahr.Level + 1 AS Level,
            CAST(REPLICATE('---', ahr.Level) + coa.AccountName AS NVARCHAR(MAX)) AS DisplayName
        FROM
            ChartOfAccounts coa
        INNER JOIN
            AccountHierarchyRecalculator ahr ON coa.ParentAccountId = ahr.AccountId
    )
    UPDATE coa
    SET
        coa.Level = ahr.Level,
        coa.DisplayName = ahr.DisplayName
    FROM
        ChartOfAccounts coa
    INNER JOIN
        AccountHierarchyRecalculator ahr ON coa.AccountId = ahr.AccountId;

END;
GO;




CREATE PROCEDURE sp_SaveVoucher
    @VoucherId INT,
    @VoucherDate DATE,
    @ReferenceNo NVARCHAR(100),
    @VoucherType NVARCHAR(50),
    @Narration NVARCHAR(MAX) = NULL,
    @TotalDebit DECIMAL(18, 2),
    @TotalCredit DECIMAL(18, 2),
    @CreatedBy NVARCHAR(255) = NULL,
    @UpdatedBy NVARCHAR(255) = NULL,
    @OperationType NVARCHAR(10),
    @VoucherDetails VoucherDetailType READONLY
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @NewVoucherId INT;

        IF @OperationType = 'Insert'
        BEGIN
            INSERT INTO Vouchers (
                VoucherDate,
                ReferenceNo,
                VoucherType,
                Narration,
                TotalDebit,
                TotalCredit,
                CreatedBy,
                CreatedDate
            )
            VALUES (
                @VoucherDate,
                @ReferenceNo,
                @VoucherType,
                @Narration,
                @TotalDebit,
                @TotalCredit,
                @CreatedBy,
                GETDATE()
            );
            SET @NewVoucherId = SCOPE_IDENTITY();

            INSERT INTO VoucherDetails (
                VoucherId,
                AccountId,
                Debit,
                Credit
            )
            SELECT
                @NewVoucherId,
                vd.AccountId,
                vd.Debit,
                vd.Credit
            FROM
                @VoucherDetails vd;

            SELECT @NewVoucherId AS VoucherId;

        END
        ELSE IF @OperationType = 'Update'
        BEGIN
            UPDATE Vouchers
            SET
                VoucherDate = @VoucherDate,
                ReferenceNo = @ReferenceNo,
                VoucherType = @VoucherType,
                Narration = @Narration,
                TotalDebit = @TotalDebit,
                TotalCredit = @TotalCredit,
                UpdatedBy = @UpdatedBy,
                UpdatedDate = GETDATE()
            WHERE
                VoucherId = @VoucherId;
            DELETE FROM VoucherDetails WHERE VoucherId = @VoucherId;

            INSERT INTO VoucherDetails (
                VoucherId,
                AccountId,
                Debit,
                Credit
            )
            SELECT
                @VoucherId,
                vd.AccountId,
                vd.Debit,
                vd.Credit
            FROM
                @VoucherDetails vd;

            SELECT @VoucherId AS VoucherId;
        END
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO;


CREATE PROCEDURE sp_GetAllVouchers
AS
BEGIN
    SELECT
        VoucherId, VoucherDate, ReferenceNo, VoucherType, Narration,
        TotalDebit, TotalCredit, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate
    FROM Vouchers
    ORDER BY VoucherDate DESC;
END;
GO;


CREATE PROCEDURE sp_GetVoucherById
    @VoucherId INT
AS
BEGIN
    SELECT
        VoucherId, VoucherDate, ReferenceNo, VoucherType, Narration,
        TotalDebit, TotalCredit, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate
    FROM Vouchers
    WHERE VoucherId = @VoucherId;
END;
GO;


CREATE PROCEDURE sp_GetVoucherDetailsByVoucherId
    @VoucherId INT
AS
BEGIN
    SELECT
        VoucherDetailId, VoucherId, AccountId, Debit, Credit
    FROM VoucherDetails
    WHERE VoucherId = @VoucherId;
END;
GO;