CREATE TABLE [dbo].[Person]
(
	[Id] INT IDENTITY (1, 1) NOT NULL,
	[FirstName] VARCHAR(50) NOT NULL,
	[MiddleInitial] VARCHAR(1) NULL,
	[FamilyName] VARCHAR(50) NOT NULL,
	[FullName] AS (([FirstName] + ISNULL(' ' + [MiddleInitial] + '. ', ' ') + [FamilyName])) PERSISTED NOT NULL,
	PRIMARY KEY CLUSTERED ([Id] ASC) WITH (DATA_COMPRESSION=PAGE)
)

GO

CREATE COLUMNSTORE INDEX [CStoreIX_Person] ON [dbo].[Person] ([Id], [FamilyName], [FirstName], [MiddleInitial])

GO