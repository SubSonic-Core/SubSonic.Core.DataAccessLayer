CREATE PROCEDURE [dbo].[InsertRealEstateProperty]
	@properties [dbo].[RealEstateProperty] readonly
AS
	DECLARE @output [dbo].[RealEstateProperty];

	INSERT INTO [dbo].[RealEstateProperty]([StatusId], [HasParallelPowerGeneration])
	OUTPUT inserted.* INTO @output
	SELECT [StatusID], [HasParallelPowerGeneration] FROM @properties;

	SELECT [ID], [StatusID], [HasParallelPowerGeneration] FROM @output;
RETURN 0
