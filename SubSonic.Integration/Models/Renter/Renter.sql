CREATE TABLE [dbo].[Renter]
(
	[PersonId] INT NOT NULL,
	[UnitId] INT NOT NULL,
	[Rent] MONEY NOT NULL,
	[StartDate] DATETIME NOT NULL DEFAULT(GETDATE()),
	[EndDate] DATETIME NULL,
	PRIMARY KEY CLUSTERED ([PersonId] ASC, [UnitId] ASC) WITH (DATA_COMPRESSION=PAGE), 
    CONSTRAINT [FK_Renter_Person] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Person]([Id]),
	CONSTRAINT [FK_Renter_Unit] FOREIGN KEY ([UnitId]) REFERENCES [dbo].[Unit]([Id])
)

GO

CREATE COLUMNSTORE INDEX [CStoreIX_Renter] ON [dbo].[Renter] ([PersonId], [UnitId],  [Rent], [StartDate], [EndDate])

GO
