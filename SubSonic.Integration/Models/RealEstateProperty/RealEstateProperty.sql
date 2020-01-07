CREATE TABLE [dbo].[RealEstateProperty]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [StatusId] INT NOT NULL, 
    [HasParallelPowerGeneration] BIT NULL
) WITH (DATA_COMPRESSION=PAGE)

GO

CREATE COLUMNSTORE INDEX [CStoreIX_RealEstateProperty] ON [dbo].[RealEstateProperty] ([Id], [StatusId], [HasParallelPowerGeneration])
