CREATE TABLE [dbo].[CatalogCollection] (
    [pk_CatalogCollectionId] UNIQUEIDENTIFIER CONSTRAINT [DF_CatalogCollection_pk_CatalogCollectionId] DEFAULT (newsequentialid()) NOT NULL,
    [Name]                   VARCHAR (50)     NOT NULL,
    CONSTRAINT [PK_CatalogCollection] PRIMARY KEY CLUSTERED ([pk_CatalogCollectionId] ASC)
);

