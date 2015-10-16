
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetUserValidation]
	-- Add the parameters for the stored procedure here
	@username VARCHAR(50),
	@LDAP varchar(max)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Insert statements for procedure here	
	DECLARE @Email AS VARCHAR(max);
	DECLARE @Name AS VARCHAR(max);
	DECLARE @ActiveDirectoryName AS VARCHAR(50);
	DECLARE @ActiveDirectoryGUID AS UNIQUEIDENTIFIER;
	DECLARE @OpenSelect AS NVARCHAR(500);
	DECLARE @IsGroup bit

	SET @OpenSelect = '(SELECT 
							@Email = mail, 
							@Name = cn, 
							@ActiveDirectoryName = SAMAccountName, 
							@ActiveDirectoryGUID = Convert(uniqueidentifier, objectGUID),
							@IsGroup = case when left (objectcategory,8) = ''CN=Group'' then convert(bit,1) else convert(bit,0) end
						FROM 
							OpenQuery (ADSI,''SELECT mail, cn, SAMAccountName, objectGUID, objectCategory FROM  ''''' + @LDAP +''''' WHERE sAMAccountName = ''''' + @username + ''''''') AS tblADSI)
						';

	EXEC sp_executesql @OpenSelect
		,N'@Email varchar(max) out, @Name varchar(max) out, @ActiveDirectoryName varchar(50) out, @ActiveDirectoryGUID uniqueidentifier out, @IsGroup bit out'
		,@Email OUTPUT
		,@Name OUTPUT
		,@ActiveDirectoryName OUTPUT
		,@ActiveDirectoryGUID OUTPUT
		,@IsGroup OUTPUT;

	print @Email
	print @Name
	print @ActiveDirectoryName
	print @ActiveDirectoryGUID
	print @IsGroup
	

	
	IF @email IS NULL
	BEGIN
		SET @email = ''
	END;

	MERGE INTO SecurityObject
	USING (
		SELECT LOWER(@Email) AS Email
			,@Name AS NAME
			,@ActiveDirectoryName AS ActiveDirectoryName
			,@ActiveDirectoryGUID AS ActiveDirectoryGUID
			,@IsGroup as IsGroup
		) AS ActiveDirectoryResult
		ON SecurityObject.ActiveDirectoryId IS NULL
			AND SecurityObject.UserName = ActiveDirectoryResult.ActiveDirectoryName
			OR SecurityObject.ActiveDirectoryId = ActiveDirectoryResult.ActiveDirectoryGUID
	WHEN MATCHED
		THEN
			UPDATE
			SET ActiveDirectoryId = ActiveDirectoryResult.ActiveDirectoryGUID
				,UserName = ActiveDirectoryResult.ActiveDirectoryName
				,EmailAddress = ActiveDirectoryResult.Email
				,FullNAME = ActiveDirectoryResult.NAME
				,LastLogInOn = GETDATE()
				,IsGroup = ActiveDirectoryResult.IsGroup
	WHEN NOT MATCHED
		THEN
			INSERT (
				ActiveDirectoryId
				,Username
				,EmailAddress
				,FullName
				,LastLogInOn
				,IsGroup
				)
			VALUES (
				ActiveDirectoryResult.ActiveDirectoryGUID
				,ActiveDirectoryResult.ActiveDirectoryName
				,ActiveDirectoryResult.Email
				,ActiveDirectoryResult.NAME
				,GETDATE()
				,ActiveDirectoryResult.IsGroup
				)
	OUTPUT inserted.SecurityObjectId AS SecurityObjectId
		,inserted.FullName
		,inserted.Username
		,inserted.EmailAddress
		,inserted.ActiveDirectoryId
		,inserted.isGroup AS IsGroup
		,inserted.LastLogInOn
		,inserted.IsActive
		,inserted.HomeFolder;
END;