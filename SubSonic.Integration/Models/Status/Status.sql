CREATE TABLE [dbo].[Status]
(
	[Id] INT IDENTITY (1, 1) NOT NULL, 
    [Name] VARCHAR(50) NOT NULL, 
    [IsAvailableStatus] BIT NOT NULL DEFAULT(0),
    PRIMARY KEY CLUSTERED ([Id] ASC) WITH (DATA_COMPRESSION=PAGE)
)

GO

CREATE COLUMNSTORE INDEX [CStoreIX_Status] ON [dbo].[Status] ([Id], [IsAvailableStatus], [Name])

GO