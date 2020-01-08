CREATE PROCEDURE [dbo].[UpdateRealEstateProperty]
	@properties [dbo].[RealEstateProperty] readonly
AS
	UPDATE [property] SET
		[StatusId] = [data].[StatusID],
		[HasParallelPowerGeneration] = [data].[HasParallelPowerGeneration]
	FROM [dbo].[RealEstateProperty] [property]
		JOIN @properties [data] ON [property].[Id] = [data].[ID];

RETURN 0
