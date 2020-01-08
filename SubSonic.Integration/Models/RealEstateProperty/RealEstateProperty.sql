CREATE TABLE [dbo].[RealEstateProperty]
(
	[Id] INT IDENTITY (1, 1) NOT NULL, 
    [StatusId] INT NOT NULL, 
    [HasParallelPowerGeneration] BIT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC) WITH (DATA_COMPRESSION=PAGE), 
    CONSTRAINT [FK_RealEstateProperty_Status] FOREIGN KEY ([StatusId]) REFERENCES [dbo].[Status]([Id])
);

GO

CREATE COLUMNSTORE INDEX [CStoreIX_RealEstateProperty] ON [dbo].[RealEstateProperty] ([Id], [StatusId], [HasParallelPowerGeneration])
