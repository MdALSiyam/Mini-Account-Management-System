USE Siyam_MiniAccountDB;
GO

IF OBJECT_ID('sp_SaveVoucher', 'P') IS NOT NULL
    DROP PROCEDURE sp_SaveVoucher;
GO

CREATE PROCEDURE sp_SaveVoucher
    @Action NVARCHAR(10), 
    @VoucherId INT = NULL,
    @VoucherDate DATE = NULL,
    @ReferenceNo NVARCHAR(100) = NULL,
    @VoucherType NVARCHAR(50) = NULL,
    @Narration NVARCHAR(MAX) = NULL,
    @CreatedBy NVARCHAR(255) = NULL,
    @UpdatedBy NVARCHAR(255) = NULL,
    @VoucherDetailsJson NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OutputVoucherId INT;
    DECLARE @TotalDebit DECIMAL(18, 2);
    DECLARE @TotalCredit DECIMAL(18, 2);

    IF @Action = 'Insert'
    BEGIN

        IF EXISTS (SELECT 1 FROM Vouchers WHERE ReferenceNo = @ReferenceNo)
        BEGIN
            RAISERROR('Reference Number already exists.', 16, 1);
            RETURN -1; -- Indicate error
        END

        SELECT
            @TotalDebit = SUM(Debit),
            @TotalCredit = SUM(Credit)
        FROM OPENJSON(@VoucherDetailsJson)
        WITH (
            AccountId INT '$.AccountId',
            Debit DECIMAL(18, 2) '$.Debit',
            Credit DECIMAL(18, 2) '$.Credit'
        );

        IF @TotalDebit IS NULL OR @TotalCredit IS NULL OR @TotalDebit <> @TotalCredit
        BEGIN
            RAISERROR('Total Debit must equal Total Credit.', 16, 1);
            RETURN -2;
        END

        BEGIN TRY
            BEGIN TRANSACTION;

            INSERT INTO Vouchers (VoucherDate, ReferenceNo, VoucherType, Narration, CreatedBy)
            VALUES (@VoucherDate, @ReferenceNo, @VoucherType, @Narration, @CreatedBy);

            SET @OutputVoucherId = SCOPE_IDENTITY();

            INSERT INTO VoucherDetails (VoucherId, AccountId, Debit, Credit)
            SELECT
                @OutputVoucherId,
                AccountId,
                Debit,
                Credit
            FROM OPENJSON(@VoucherDetailsJson)
            WITH (
                AccountId INT '$.AccountId',
                Debit DECIMAL(18, 2) '$.Debit',
                Credit DECIMAL(18, 2) '$.Credit'
            );

            COMMIT TRANSACTION;
            SELECT @OutputVoucherId AS NewVoucherId;
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0
                ROLLBACK TRANSACTION;
            DECLARE @ErrorMessage NVARCHAR(4000);
            DECLARE @ErrorSeverity INT;
            DECLARE @ErrorState INT;
            SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
            RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
            RETURN -3; -- Indicate generic error
        END CATCH;
    END
    ELSE IF @Action = 'Update'
    BEGIN

        BEGIN TRY
            BEGIN TRANSACTION;

            UPDATE Vouchers
            SET
                VoucherDate = ISNULL(@VoucherDate, VoucherDate),
                ReferenceNo = ISNULL(@ReferenceNo, ReferenceNo),
                VoucherType = ISNULL(@VoucherType, VoucherType),
                Narration = ISNULL(@Narration, Narration),
                UpdatedBy = @UpdatedBy,
                UpdatedDate = GETDATE()
            WHERE VoucherId = @VoucherId;

            -- Delete existing details
            DELETE FROM VoucherDetails WHERE VoucherId = @VoucherId;

            -- Insert new details
            SELECT
                @TotalDebit = SUM(Debit),
                @TotalCredit = SUM(Credit)
            FROM OPENJSON(@VoucherDetailsJson)
            WITH (
                AccountId INT '$.AccountId',
                Debit DECIMAL(18, 2) '$.Debit',
                Credit DECIMAL(18, 2) '$.Credit'
            );

            IF @TotalDebit IS NULL OR @TotalCredit IS NULL OR @TotalDebit <> @TotalCredit
            BEGIN
                RAISERROR('Total Debit must equal Total Credit during update.', 16, 1);
                ROLLBACK TRANSACTION;
                RETURN -2;
            END

            INSERT INTO VoucherDetails (VoucherId, AccountId, Debit, Credit)
            SELECT
                @VoucherId,
                AccountId,
                Debit,
                Credit
            FROM OPENJSON(@VoucherDetailsJson)
            WITH (
                AccountId INT '$.AccountId',
                Debit DECIMAL(18, 2) '$.Debit',
                Credit DECIMAL(18, 2) '$.Credit'
            );

            COMMIT TRANSACTION;
            SELECT @VoucherId AS UpdatedVoucherId;
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0
                ROLLBACK TRANSACTION;
            DECLARE @ErrorMessageUpd NVARCHAR(4000);
            DECLARE @ErrorSeverityUpd INT;
            DECLARE @ErrorStateUpd INT;
            SELECT @ErrorMessageUpd = ERROR_MESSAGE(), @ErrorSeverityUpd = ERROR_SEVERITY(), @ErrorStateUpd = ERROR_STATE();
            RAISERROR(@ErrorMessageUpd, @ErrorSeverityUpd, @ErrorStateUpd);
            RETURN -3;
        END CATCH;
    END
    ELSE IF @Action = 'Delete'
    BEGIN
        BEGIN TRY
            BEGIN TRANSACTION;
            DELETE FROM VoucherDetails WHERE VoucherId = @VoucherId;
            DELETE FROM Vouchers WHERE VoucherId = @VoucherId;
            COMMIT TRANSACTION;
            SELECT @VoucherId AS DeletedVoucherId;
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0
                ROLLBACK TRANSACTION;
            DECLARE @ErrorMessageDel NVARCHAR(4000);
            DECLARE @ErrorSeverityDel INT;
            DECLARE @ErrorStateDel INT;
            SELECT @ErrorMessageDel = ERROR_MESSAGE(), @ErrorSeverityDel = ERROR_SEVERITY(), @ErrorStateDel = ERROR_STATE();
            RAISERROR(@ErrorMessageDel, @ErrorSeverityDel, @ErrorStateDel);
            RETURN -3;
        END CATCH;
    END
    ELSE IF @Action = 'Select'
    BEGIN
        SELECT
            v.VoucherId,
            v.VoucherDate,
            v.ReferenceNo,
            v.VoucherType,
            v.Narration,
            v.CreatedBy,
            v.CreatedDate,
            v.UpdatedBy,
            v.UpdatedDate
        FROM Vouchers v
        WHERE (@VoucherId IS NULL OR v.VoucherId = @VoucherId)
          AND (@VoucherType IS NULL OR v.VoucherType = @VoucherType)
          AND (@ReferenceNo IS NULL OR v.ReferenceNo LIKE '%' + @ReferenceNo + '%')
        ORDER BY v.VoucherDate DESC, v.ReferenceNo;
    END
    ELSE IF @Action = 'SelectDetails'
    BEGIN
        SELECT
            vd.VoucherDetailId,
            vd.VoucherId,
            vd.AccountId,
            coa.AccountCode,
            coa.AccountName,
            vd.Debit,
            vd.Credit
        FROM VoucherDetails vd
        JOIN ChartOfAccounts coa ON vd.AccountId = coa.AccountId
        WHERE vd.VoucherId = @VoucherId
        ORDER BY vd.VoucherDetailId;
    END
END;
GO