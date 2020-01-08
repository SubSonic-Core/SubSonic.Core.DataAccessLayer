CREATE PROCEDURE [dbo].[DeleteRealEstateProperty]
	@properties [dbo].[RealEstateProperty] readonly
AS

	DELETE
	FROM [dbo].[RealEstateProperty]
	WHERE [Id] IN (SELECT [ID] FROM @properties);

RETURN 0
