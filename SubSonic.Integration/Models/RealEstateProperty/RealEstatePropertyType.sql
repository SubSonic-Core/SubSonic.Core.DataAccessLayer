--IF ((SELECT
--	COUNT(*)
--FROM sys.types 
--	JOIN sys.schemas ON types.schema_id = schemas.schema_id
--WHERE schemas.name = 'dbo' AND types.name = 'RealEstateProperty') = 0)
--BEGIN;
	CREATE TYPE [dbo].[RealEstateProperty] AS TABLE(
		[ID] [Int] NOT NULL,
		[StatusID] [Int] NOT NULL,
		[HasParallelPowerGeneration] [Bit] NULL);
--END;
