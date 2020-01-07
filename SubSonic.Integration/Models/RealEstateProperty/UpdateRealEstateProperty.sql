CREATE PROCEDURE [dbo].[UpdateRealEstateProperty]
	@properties [dbo].[RealEstateProperty] readonly
AS
	UPDATE property SET
		[StatusId] = [data].StatusId,
		[HasParallelPowerGeneration] = [data].HasParallelPowerGeneration
	FROM [dbo].[RealEstateProperty] property
		JOIN @properties [data] ON property.Id = [data].Id

	SELECT [ID], [StatusID], [HasParallelPowerGeneration] FROM @properties;

RETURN 0
