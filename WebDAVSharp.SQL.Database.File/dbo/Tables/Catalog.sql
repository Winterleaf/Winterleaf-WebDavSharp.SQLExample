CREATE TABLE [dbo].[Catalog] (
    [pk_CatalogId]           UNIQUEIDENTIFIER CONSTRAINT [DF_Catalog_pk_CatalogId] DEFAULT (newsequentialid()) NOT NULL,
    [fk_CatalogCollectionId] UNIQUEIDENTIFIER NOT NULL,
    [Name]                   VARCHAR (MAX)    NOT NULL,
    [Offline]                BIT              NOT NULL,
    [Server]                 VARCHAR (50)     NOT NULL,
    [DatabaseName]           VARCHAR (100)    NOT NULL,
    [UserName]               VARCHAR (50)     NOT NULL,
    [Password]               VARCHAR (50)     NOT NULL,
    [fk_CatalogStatusId]     INT              CONSTRAINT [DF_Catalog_fk_CatalogStatusId] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Catalog] PRIMARY KEY CLUSTERED ([pk_CatalogId] ASC),
    CONSTRAINT [FK_Catalog_CatalogCollection] FOREIGN KEY ([fk_CatalogCollectionId]) REFERENCES [dbo].[CatalogCollection] ([pk_CatalogCollectionId]),
    CONSTRAINT [FK_Catalog_CatalogStatus] FOREIGN KEY ([fk_CatalogStatusId]) REFERENCES [dbo].[CatalogStatus] ([pk_CatalogStatusId])
);

