CREATE TABLE [dbo].[Unit]
(
	[Id] INT IDENTITY (1, 1) NOT NULL,
	[Bedrooms] INT NOT NULL,
	[Bathrooms] INT NOT NULL,
	[SquareFootage] INT NULL,
	[RealEstatePropertyId] INT NOT NULL,
	[StatusId] INT NOT NULL,
	PRIMARY KEY CLUSTERED ([Id] ASC) WITH (DATA_COMPRESSION=PAGE), 
	CONSTRAINT [FK_Unit_RealEstateProperty] FOREIGN KEY ([RealEstatePropertyId]) REFERENCES [dbo].[RealEstateProperty]([Id]),
    CONSTRAINT [FK_Unit_Status] FOREIGN KEY ([StatusId]) REFERENCES [dbo].[Status]([Id])
)
