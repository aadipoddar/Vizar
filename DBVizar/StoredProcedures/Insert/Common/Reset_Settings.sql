CREATE PROCEDURE [dbo].[Reset_Settings]
AS
BEGIN
	DELETE FROM [Settings]

	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'EnableLoginWithCode'				, N'true'	, N'Enable or disable login with code feature')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'MaxLoginAttempts'				, N'5'		, N'Maximum number of login attempts before lockout')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'EnableUsersToResetPassword'		, N'true'	, N'Allow users to reset their passwords')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'CodeResendLimit'					, N'3'		, N'Maximum number of code resends allowed')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'CodeExpiryMinutes'				, N'10'		, N'Expiry time for codes in minutes')

	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'PrimaryCompanyLinkingId'			, N'1'		, N'Company Id for the Primary Company Account')

	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'LedgerCodePrefix'				, N'LD'		, N'Prefix for Ledger Codes')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'ItemCategoryCodePrefix'			, N'ITCT'	, N'Prefix for Item Category Codes')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'ItemTypeCodePrefix'				, N'ITTY'	, N'Prefix for Item Type Codes')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'ItemCodePrefix'					, N'IT'		, N'Prefix for Item Codes')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'ManufacturerCodePrefix'			, N'MFR'	, N'Prefix for Manufacturer Codes')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'PurchaseTransactionPrefix'		, N'PUR'	, N'Prefix for Purchase Transaction Numbers')

	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'UpdateItemMasterRateOnPurchase'	, N'true'	, N'Update Item Master Rate on Purchase Transactions')
	INSERT INTO [dbo].[Settings] ([Key], [Value], [Description]) VALUES (N'UpdateItemMasterUOMOnPurchase'	, N'true'	, N'Update Item Master Unit of Measurement on Purchase Transactions')

END